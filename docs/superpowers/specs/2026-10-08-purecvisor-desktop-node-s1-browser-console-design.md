# S1 브라우저 콘솔 설계

- Design-ID: `pcv-s1-browser-console-v1`
- 작성일: `2026-10-08`
- 문서 상태: `accepted` (사용자 승인 `1,2,3`의 2, campaign `adr17-s1-console-20261008` Task 5)
- 근거: ADR-0017 시나리오 S1, spike `docs/superpowers/specs/2026-10-08-purecvisor-desktop-node-hyperv-browser-console-spike.md`(E 권장)
- 변경 등급: M (새 read route, 새 guest 입력 route, 새 권한, Web 패널)

## 1. 목적

브라우저에서 VM 화면을 보고 키보드로 입력해 OS 설치를 끝낼 수 있게 한다. 경로는 WMI만 쓴다. 화면은
`Msvm_VirtualSystemManagementService.GetVirtualSystemThumbnailImage`, 입력은 `Msvm_Keyboard`다. 외부 설치, TCP `2179`,
브라우저의 Windows 계정 입력은 없다. 기존 `vmconnect` handoff와 noVNC(ADR-0010)는 보조 경로로 남는다.

## 2. 화면 route

`GET /api/v1/vms/{vmId}/console/frame?width=<w>&height=<h>&encoding=<deflate|raw>`

- 계약: `RuntimeReadOnly`, operation `console.frame`, 이름 `GetVmConsoleFrame`, feature `pcv.vm.console-frame`,
  route family `console`, 권한 `console.view`.
- 크기: 기본 `640×480`. width `160~1024`, height `120~768`. 범위 밖이면 `400 PCV_CONSOLE_FRAME_SIZE_INVALID`.
- encoding: 기본 `deflate`(`System.IO.Compression.DeflateStream`, 서버 native 호출 없음). `raw`도 받는다.
- 응답 `data`: `vm`, `width`, `height`, `format=rgb565le`, `encoding`, `byte_length`(압축 전), `frame_base64`, `captured_at`.
- 오류: 없는 VM `404 PCV_VM_NOT_FOUND`, 켜져 있지 않음 `409 PCV_CONSOLE_VM_NOT_RUNNING`, WMI 실패
  `502 PCV_CONSOLE_FRAME_FAILED`, 같은 VM에 100ms 안 재요청 `429 PCV_CONSOLE_RATE_LIMITED`.
- adapter operation `vm.console.frame`(읽기): 실현된 `Msvm_VirtualSystemSettingData`를 `TargetSystem`으로 넘기고
  `WidthPixels`, `HeightPixels`를 준다. 반환 `ImageData`는 `width×height×2` byte로 자른다(spike에서 끝에 4 byte가 더 붙었다).

## 3. 입력 route

`POST /api/v1/vms/{vmId}/console/input`

- 계약: `RuntimeProductOperation`, operation `console.input`, 이름 `SendVmConsoleInput`, feature `pcv.vm.console-input`,
  route family `console`, 권한 새 `console.input`. job이 아니라 동기 처리다(키 입력 지연 때문).
- body:
  - `{"kind":"key","action":"type|press|release","key_code":<1~254 Windows virtual-key>}`
  - `{"kind":"text","text":"<1~256자 출력 가능 ASCII>"}`
  - `{"kind":"ctrl-alt-del"}`
- adapter operation `vm.console.input`: VM의 `Msvm_Keyboard`에 `TypeKey`, `PressKey`, `ReleaseKey`, `TypeText`,
  `TypeCtrlAltDel`을 부른다.
- 응답 `data`: `vm`, `kind`, `action`, `accepted=true`. 입력 값은 되돌려 주지 않는다.
- 오류: 형식 오류 `400 PCV_CONSOLE_INPUT_INVALID`, 켜져 있지 않음 `409 PCV_CONSOLE_VM_NOT_RUNNING`, WMI 실패
  `502 PCV_CONSOLE_INPUT_FAILED`, VM당 초당 50회 초과 `429 PCV_CONSOLE_RATE_LIMITED`.

## 4. 권한과 노출

- 새 권한 `console.input`은 `operator` 이상이다(`route_policy` 표에 `console.input = operator`). `admin`은 `*`로 갖는다.
- loopback 요청은 기존 인증(service bearer, loopback session, account JWT)을 따른다.
- loopback이 아닌 요청의 입력은 account JWT만 받는다. service bearer면 `403 PCV_CONSOLE_INPUT_ACCOUNT_REQUIRED`.
  LAN 노출 자체는 기존 listener 경계(ADR-0006, ADR-0010의 명시 gate)를 따르고 이 설계가 넓히지 않는다.
- 화면 route는 `console.view`만 본다. 화면에 비밀번호 입력이 보일 수 있으므로 `console.view`는 지금처럼 `operator` 이상이다.

## 5. 입력 audit

- 요청마다 data root `console-input-audit.jsonl`에 한 줄을 더한다: `ts`, `request_id`, `principal`(account 이름 또는
  `service-token`), `origin`(`loopback`/`remote`), `vm`, `kind`, `action`, `key_code`(key만), `text_length`(text만), `result`.
- text 내용과 그 hash는 남기지 않는다(짧은 문자열의 hash는 되돌릴 수 있다).
- 파일이 1 MiB를 넘으면 `console-input-audit.1.jsonl`로 한 번 돌린다. audit 쓰기에 실패하면 입력을 보내지 않고
  `503 PCV_CONSOLE_AUDIT_UNAVAILABLE`.

## 6. Web 패널

- VM 상세에 `Console` 패널: canvas, 시작/일시 정지, 갱신률 `1/2/5` fps(기본 `2`), 크기 `640×480`/`800×600`/`1024×768`.
- frame은 `DecompressionStream('deflate')`로 풀고 RGB565를 RGBA `ImageData`로 바꿔 그린다. 서버는 이미지 변환을 하지 않는다.
- VM이 꺼져 있으면 polling을 멈추고 상태 문구를 보인다. 탭이 숨으면 멈춘다.
- 입력(Task 11): canvas를 누르면 포커스를 잡고 `keydown`/`keyup`을 `event.code` → virtual-key 표로 `press`/`release`로
  보낸다. `Ctrl+Alt+Del` 버튼과 text 보내기 칸(`kind=text`)을 둔다. `console.input`이 없으면 읽기 전용이다.
- 기존 `vmconnect` handoff 안내는 패널 아래 보조 문구로 낮춘다.

## 7. 시험

- adapter: fake WMI로 frame 크기 자르기, VM 없음, 꺼짐, 반환 코드 실패, 입력 method 선택과 인자.
- route: 계약 행, 권한(`console.view`, `console.input`), 크기·body 검증, rate limit, remote service bearer 거부,
  audit 줄에 text 내용이 없음.
- Web: RGB565→RGBA 변환, virtual-key 표, 권한 없는 읽기 전용.
- 실제 VM: Task 10이 `pcv-it-` VM에서 frame 읽기와 firmware 화면 키 입력 뒤 화면 변화를 본다(ADR-0016 단계, guest OS 접속 없음).

## 8. 범위 밖

- 마우스(`Msvm_SyntheticMouse`), 소리, 클립보드, USB, 2179 RDP 경로.
- noVNC 동작 변경, LAN 노출 확대, 설치본 MSI 변경.

## Nonclaims

- 이 설계는 S1 통과를 주장하지 않는다. S1은 설치본 시나리오 스크립트와 시연 기록이 생긴 뒤에만 적는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
