# Web Console UI/UX 재설계 설계

- 상태: 채택(2026-11-05 사용자 승인 `1,2,3,4,5`), 결정 기록은 ADR-0019
- 작성: Claude(설계), 조사: grok(2026-11-05)
- 기준: 이 저장소 `cbe5459`, 참고 purecvisor-single `c45dbf5`
- campaign: `ui-redesign-20261105`, plan `docs/superpowers/plans/2026-11-05-purecvisor-desktop-node-ui-redesign.md`

## 1. 배경과 진단

2026-11-05 S4 사용자 시연(LAN의 다른 PC 브라우저, 설치본 `0.42.96`)에서 다음이 드러났다. 필요한 API와 입력란은 대부분
있었고, 사용자가 찾지 못하거나 확인할 수 없는 모양으로 놓여 있었다.

| 증상 | 원인 | 바꿀 방향 |
| --- | --- | --- |
| 복제가 `PCV_VM_CLONE_NAME_REQUIRED`로 끝남 | 상단 `Clone VM`(`web/src/modules/vm.ts:238`)은 이름 없이 실행되고, 이름 칸(`vm.ts:276-278`)은 자원 격자 마지막 칸 | 복제 모달. 이름이 첫 칸, 제안값, 요청 전 검사 |
| 콘솔이 작아 입력 반영을 볼 수 없음 | 기본 프레임 `640x480`(`vm-console.ts:519`), canvas가 그 너비를 넘지 않음(`:573`), 맞춤·전체 화면 없음 | 콘솔 탭. 기본 `1024x768`, 창에 맞춤, 전체 화면, 키보드 입력 상태 |
| 콘솔과 checkpoint가 첫 화면 밖 | VM 상세가 한 스크롤(`VM_TABS = []`, `vm.ts:224-302`) | VM 상세 6개 탭 |
| 예약 저장 뒤 패널이 그대로(BL-0020) | 저장 성공 뒤 이전 `selectedVm`으로 다시 그림 | 저장 응답으로 즉시 다시 읽고 그림 |
| 비밀번호를 잊은 admin을 되살릴 수 없음 | 계정 API가 목록·생성·비활성화뿐 | 계정 화면, 비밀번호 재설정(부록 A) |
| 파괴 조작 확인이 브라우저 기본 창 | `window.confirm`(`vm.ts:618`, `:1048`, `:1090`, `:1118`) | 앱 안 확인 모달 |
| 생성 dialog에 예시 경로가 초기값 | `web/index.html:451-469` | 4단계 마법사, 예시는 placeholder |
| 상세 조작 문구가 영어 고정 | `Clone VM`, `Target VM name`, `Create VM` 등 | i18n 키 |

## 2. 참고 비교

ADR-0018로 셸, Supanova 테마, 모듈 순서 빌드는 이미 purecvisor-single과 같다. 다른 것은 화면 구성이다.

| 흐름 | purecvisor-single | Desktop Node(현재) | 가져올 것 |
| --- | --- | --- | --- |
| VM 상세 | 탭 5개(`ui/modules/shell.js:36`) | 탭 없음 | 탭 구조. Hyper-V 데이터에 맞춰 6개 |
| 복제 | 모달 첫 칸 `새 VM 이름`, 제안 `<원본>-clone`(`vm-lifecycle.js:1605-1638`) | 상단 버튼 + 격자 칸 | 모달 배치와 제안값. 이름 규칙은 Desktop Node 서버 규칙(7절) |
| 생성 | 3단계 마법사(`vm-lifecycle.js:501-546`) | dialog 한 장 | 단계와 "이름 통과 전 다음 불가" |
| 콘솔 | noVNC, 맞춤·전체 화면 | frame 폴링, 맞춤 없음 | 맞춤·전체 화면 조작. noVNC·WebSocket은 가져오지 않음 |
| 확인 | `customConfirm`(`ui.js:777`) | `window.confirm` | 앱 안 확인 |
| 빈 상태 | `emptyStatePro`(`ui.js:1201`), 읽기 실패 구분 | `.muted` 문장 | 빈 상태와 읽기 실패 구분 |
| 작업 | `waitForJob`, 접수 토스트 | 작업 화면과 폴링 | 조작한 자리의 진행 줄 |

가져오지 않는 것: 컨테이너, ZFS·스토리지 정책, OVS/OVN·VPC, 보안 그룹, iSCSI, DPDK, SR-IOV, GPU, 자가치유, Swagger, TOTP,
push, noVNC·VNC WebSocket, cow·guestfs 복제 옵션.

## 3. 원칙

1. 필수 입력은 그 조작을 여는 모달의 첫 칸에 두고 합리적인 제안값을 채운다.
2. 형식과 중복은 입력하는 동안 칸 아래에 알린다. 통과해야 주 버튼이 켜진다. 서버 검사는 최종 판정으로 그대로 둔다.
3. job은 조작한 자리에서 접수, 실행 중, 완료, 실패를 한 줄로 보여 준다. 작업 화면은 전체 기록이다.
4. 삭제, 복원, 강제 끄기, 잠금 해제는 결과를 설명하는 앱 안 확인 모달로 묻는다. 삭제는 대상 이름을 입력해야 버튼이 켜진다.
5. 새 문구는 i18n 키로 둔다. ko가 기본이다.

## 4. 정보 구조

### 4.1 사이드바

| 묶음 | 항목(view id) | 비고 |
| --- | --- | --- |
| 운영 | 대시보드(`dashboard`) | |
| 워크로드 | 가상 머신(`vms`), 템플릿(`templates`, 신규) | 템플릿은 `template_lock === true` VM 목록, 주 조작은 복제 |
| 인프라 | 네트워크(`network`) | 읽기 전용 |
| 관제 | 작업(`jobs`), 이벤트(`activity`), 증적(`evidence`) | |
| 시스템 | 계정(`accounts`, 신규, admin), 진단(`troubleshooting`) | 계정은 `account.manage` 권한이 있을 때만 보인다 |
| 도움말 | 도움말(`helppage`) | |

새 view id는 `web/src/modules/core.ts` `VALID_VIEWS`, `web/src/modules/shell.ts` `NAV_SECTIONS`, `web/index.html` 섹션에 함께
넣는다(Task 7).

### 4.2 VM 상세

- 헤더: 경로(가상 머신 › 이름), 이름, 상태 배지, 상태별 전원 조작(시작 / 종료·다시 시작 등), 콘솔 열기, 더 보기 메뉴(복제…, 템플릿으로
  잠금 또는 잠금 해제…, 내보내기…, vmconnect로 열기, 강제 끄기…, 삭제…). 삭제는 메뉴 맨 아래 위험 색이다.
- 탭: 요약, 콘솔, 체크포인트, 하드웨어, 게스트, 작업 기록. 마지막으로 연 탭을 기억하고 hash로 열 수 있다.

| 탭 | 내용 | 지금 위치 |
| --- | --- | --- |
| 요약 | 상태, 자원, 미디어와 네트워크, QoS·통계 요약, 최근 작업 | 상세 KV, QoS, 통계 |
| 콘솔 | 5.4절 | 상세 아래 콘솔 패널 |
| 체크포인트 | 목록, 만들기, 복원, 삭제, 예약 카드 | 상세 아래 checkpoint 패널 |
| 하드웨어 | ISO, 이름 변경, 스위치, 장치, 메모리, vCPU, 디스크. 칸마다 라벨, 도움말, 적용 버튼 | 자원 격자 |
| 게스트 | guest exec, 파일, 채널 | 상세 아래 guest 패널 |
| 작업 기록 | 이 VM의 job 목록 | 작업 화면에서 필터 |

템플릿으로 잠긴 VM은 허용 조작(시작, 복제, 잠금 해제, 콘솔, 내보내기)만 켠다. 잠금 상태는 `template_lock`(7.1절)으로 읽는다.

## 5. 화면

와이어프레임은 campaign을 승인한 설계안 페이지에 있다. 이 절은 그 화면의 규칙이다.

### 5.1 W1 가상 머신 목록

- 도구 줄: 이름 검색, 상태 칩(전체, 실행 중, 꺼짐, 템플릿, 개수 표시), `+ 새 VM`.
- 표: 이름, 상태 배지, 세대, vCPU, 메모리, 표시(템플릿 배지), 행 조작. 행 주 조작은 상태에 따라 하나다(실행 중이면 콘솔, 템플릿이면
  복제…, 꺼져 있으면 시작). 나머지는 `⋯` 메뉴.
- 상태는 색과 글자를 함께 쓴다.

### 5.2 W2 VM 상세 요약과 더 보기

4.2절. 최근 작업은 이 VM의 job 두세 개를 진행 줄로 보이고 작업 기록 탭으로 이어진다.

### 5.3 W3 복제 모달

- 제목 `VM 복제`, 부제에 원본 이름, 잠금 여부, 전원 상태.
- 첫 칸 `새 VM 이름`(필수). 제안값 `<원본>-clone`. 이미 있으면 `-clone-2`부터 비어 있는 번호.
- 입력 중 검사(7.2절 규칙): 비어 있음, 앞뒤 공백, 제어 문자·`/`·`\`, `.`·`..`, 128자 초과, 원본과 같음, 목록에 같은 이름(대소문자
  무시). 오류는 칸 아래 한 문장으로 무엇을 고치면 되는지 적는다.
- 저장 위치: 기본 VM 위치 사용(서버 기본값). 바꾸는 칸은 접어 둔다.
- 검사를 통과하면 `clone/preview`를 부르고 결과를 모달 안 패널에 보인다. preview 실패는 그 패널에 오류 코드와 고칠 방법을 보인다.
- `복제 시작`은 preview가 성공한 뒤에만 켜진다. 누르면 같은 모달에 job 진행 줄을 보이고, 완료되면 새 VM 상세로 이동하는 링크를 준다.
- 새 셸에서 복제의 `window.confirm`을 없앤다. 요청 바디는 지금과 같은 `{confirm_name, name}`(필요 시 `vm_root`)이다.

### 5.4 W4 콘솔 탭

- 도구 줄: 창에 맞춤 / 실제 크기, 해상도(`1024x768` 기본, `800x600`, `640x480`), 갱신 fps, 전체 화면, Ctrl+Alt+Del, 키보드 입력 상태
  칩(입력 중 / 콘솔을 클릭하면 입력).
- 창에 맞춤은 받은 프레임을 CSS로 비율 유지 확대·축소한다. frame·input API(`/api/v1/vms/{id}/console/frame/{size}`,
  `/console/input`)는 그대로다.
- 바닥 줄: 프레임 크기, 표시 배율, 마지막 갱신, 입력 해제 방법. vmconnect 핸드오프 링크는 유지한다.
- 권한: 보기는 `console.view`, 입력은 `console.input`. 입력 권한이 없으면 입력 상태 칩이 그 사실을 알린다.

### 5.5 W5 체크포인트 탭

- 목록: 이름, 만든 시각, 종류(수동, 예약), 행 조작(복원…, 삭제…). `checkpoint 만들기`는 이름 제안값이 있는 작은 모달.
- 예약 카드: 사용 여부, 간격, 보존 개수, 다음 실행(`next_due_at`), 마지막 결과, 미리보기, 저장, 예약 끄기.
- 예약 저장·해제와 checkpoint 생성·복원 job이 끝나면 선택 VM 상세를 다시 읽고 탭을 다시 그린다(BL-0020).
- 빈 목록과 읽기 실패는 다른 문구다. 복원 확인은 현재 상태를 잃는다는 결과와, 실행 중이면 그 사실을 적는다.

### 5.6 W6 새 VM 마법사

- 단계: 1 기본(이름, 세대), 2 자원(vCPU, 메모리, 디스크), 3 설치 미디어와 네트워크(ISO 경로, 스위치), 4 확인(요약).
- 1단계는 생성 이름 규칙(7.2절 `IsValidVmCreateName`)과 목록 중복을 통과해야 다음으로 간다.
- 예시 경로(`D:\iso\installer.iso` 등)를 초기값으로 채우지 않는다. placeholder로만 둔다.
- 스위치 목록은 `network.inventory`에서 읽는다. 만들기 뒤 모달이 job 진행 줄로 바뀐다.

### 5.7 W7 삭제 확인 모달

- 지울 대상과 결과를 한 문장으로 설명한다(VM 구성과 디스크, 함께 지워지는 checkpoint 수).
- 대상 이름을 정확히 입력해야 삭제 버튼이 켜진다. 같은 컴포넌트를 복원, 강제 끄기, 잠금 해제에 쓰되 이름 입력은 삭제에만 요구한다.

## 6. 디자인 체계

- token은 `web/style.css` `[data-theme="supanova"]`를 쓴다: `--bg #ffffff`, `--bg2 #f5f7fa`, `--bg3 #edf1f5`, `--border #dce1e8`,
  `--fg #171c24`, `--fg2 #5c6675`, `--accent #12627a`, `--green #18794e`, `--yellow #8a6100`, `--red #b42332`. `web/DESIGN.md`를 이 값으로
  고쳤다(Task 1).
- 글꼴 Pretendard, 아이콘 Coolicons 스프라이트. 새 서드파티 자산은 넣지 않는다.
- 컴포넌트(Task 2): 버튼(주, 보조, 위험, 링크형), 상태 배지(실행 중, 꺼짐, 일시 정지, 실패, 템플릿), 입력 칸(기본, 포커스, 오류, 도움말),
  모달(포커스 가둠, Esc, 배경 클릭은 위험 조작에서 닫지 않음), 확인 모달(이름 입력 옵션), 탭, 표, 알림, job 진행 줄, 빈 상태와 읽기 실패.
- 상태: normal, hover, focus(`:focus-visible`), active, disabled, loading, empty, error.

## 7. API 확인 결과 (Task 1 조사)

### 7.1 템플릿 잠금

- `GET /api/v1/vms` 항목의 `template_lock`(bool)이다. 잠겼을 때만 JSON에 나오고 없으면 잠금이 아니다
  (`src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs:235-237`, `WhenWritingDefault`). 상세 `GET /api/v1/vms/{id}`도 같은 항목이다.
- 새 셸은 `isTemplateLockedVm`(`web/src/modules/core.ts:618-619`)으로 읽는다.
- 그래서 W1 템플릿 배지·필터와 템플릿 화면은 **API 변경 없이** 목록을 거르면 된다. 직렬화 생략 규칙은 바꾸지 않는다.

### 7.2 이름 규칙

- 복제 대상: `IsValidHyperVName`(`src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Shared.cs:89-97`). 1~128자, 앞뒤 공백 없음, 제어
  문자·`/`·`\` 없음. 추가로 `.`·`..` 거절, 원본과 같으면 `PCV_VM_CLONE_NAME_CONFLICT`, VM root 하위 경로여야 함. 중복은 WMI `FindVm`
  (대소문자 무시)이 `PCV_VM_ALREADY_EXISTS`로 거절한다.
- 생성: `IsValidVmCreateName`(`Shared.cs:99-104`). 1~63자, 첫 글자는 문자나 숫자, 나머지는 문자·숫자·`.`·`_`·`-`.
- purecvisor-single의 `/^[a-zA-Z0-9_-]{1,63}$/`는 Desktop Node 복제 규칙과 다르므로 쓰지 않는다. 클라이언트 규칙은 위 C# 함수를 옮기고,
  같은 사례를 node 시험과 C# 시험에 둔다.

### 7.3 계정

- route: `GET/POST /api/v1/accounts`, `POST /api/v1/accounts/{username}/disable`(`account.manage`). 재설정·변경 route는 없다.
- 사용자 이름 `^[A-Za-z][A-Za-z0-9._-]{2,31}$`, 비밀번호 12자 이상이고 사용자 이름과 다름. 해시는 PBKDF2-SHA256 10만 회.
- refresh 폐기는 프로세스 메모리의 `jti` 집합뿐이고, access JWT(기본 15분)를 무효화할 수단이 없다. 마지막 admin 비활성화는
  `PCV_ACCOUNT_LAST_ADMIN`(409)으로 막는다.

## 8. 구현 방식과 계약 영향

- 새 셸 모듈 점진 교체(ADR-0019). `web/index.html`, `web/src/modules/*.ts`, `web/src/bootstrap.ts`, `web/style.css`를 화면 단위로 고친다.
  `modules.json`에 새 모듈을 넣을 때는 순서표 검사(`web/scripts/build-served-asset.mjs:133-135`)를 맞춘다. 생성물은
  `npm run build:served --prefix web`로만 만든다.
- 옛 콘솔은 Task 9 전까지 동결한다. `web/contracts/web-static-contracts.mjs` `verifyVmLifecycleActions`(`:1060-1096`)는 옛 파일만 읽으므로
  새 셸 변경으로 깨지지 않는다. 깨지면 그 변경을 멈추고 보고한다.
- MSI payload 파일 이름은 바뀌지 않는다(Task 9 전). `config/*spec*.json`은 웹 파일 해시를 고정하지 않는다.
- 새 route(Task 8)는 route·operation·permission 고정값(카탈로그 `ApiHandlerAdapterContract.cs`, route coverage, snapshot SHA, Host fixture,
  ratchet)을 같은 PR에서 맞추고 `release-train.json` queue 행을 더한다.

## 9. 시험 전략

- 컴포넌트·흐름은 node 시험(`web/node-tests`)으로 고정한다: 모달 포커스와 Esc, 입력 검사 사례, 빈 이름이면 API 미호출, preview 다음 clone
  payload, 탭 전환 가시성, 예약 저장 뒤 다시 그림, 콘솔 맞춤 배율.
- 이름 규칙 사례 표는 C# 시험과 node 시험이 같은 사례를 쓴다.
- Task 10: axe 접근성 검사(위반 0을 목표로 기준선), 화면 전환·탭·모달 브라우저 시험, 화면별 스크린샷 기록.
- 설치본 동작은 release train 뒤 S1~S3 재시연 기록으로만 주장한다.

## 10. 단계

| campaign task | 단계 | 담당 |
| --- | --- | --- |
| Task 1 | P0 설계 확정(이 문서, ADR-0019, `web/DESIGN.md`) | Claude |
| Task 2 | P1 공통 컴포넌트 | grok 구현, Claude 리뷰 |
| Task 3 | P2 VM 상세 탭, 헤더, 복제 모달 | 같음 |
| Task 4 | P3 콘솔 탭 | 같음 |
| Task 5 | P4 체크포인트 탭, BL-0020 | 같음 |
| Task 6 | P5 새 VM 마법사 | 같음 |
| Task 7 | P6a 사이드바, 템플릿·계정 화면, 한국어화 | 같음 |
| Task 8 | P6b 비밀번호 재설정(부록 A) | 같음 |
| Task 9 | P7 옛 콘솔 퇴역 | 같음 |
| Task 10 | P8 품질 게이트, 종료 검증 | 같음 |

## 부록 A. 비밀번호 재설정

### A.1 범위

- admin이 다른 계정의 비밀번호를 재설정한다.
- 로그인한 사용자가 자기 비밀번호를 바꾼다.
- 모든 admin이 비밀번호를 잊었을 때는 호스트의 PCVCLI가 서비스 bearer(기본 보호 token 파일)로 재설정한다. 지금 계정 생성과 같은 인가 경로다.

### A.2 route

| 메서드와 경로 | operation | 권한 | 바디 |
| --- | --- | --- | --- |
| `POST /api/v1/accounts/{username}/password` | `account.password.reset` | `account.manage` 또는 서비스 bearer | `confirm_username`, `new_password` |
| `POST /api/v1/auth/password` | `auth.password.change` | 로그인한 계정 access JWT | `current_password`, `new_password` |

- 비밀번호 정책은 지금 `IsPasswordValid`와 같다(12자 이상, 사용자 이름과 다름). 실패는 `PCV_ACCOUNT_PASSWORD_INVALID`.
- 재설정은 계정의 `enabled` 상태를 바꾸지 않는다. 비활성 계정을 다시 켜는 route는 이 범위가 아니다.
- `auth.password.change`는 `current_password`가 틀리면 `PCV_ACCOUNT_PASSWORD_MISMATCH`(401)이고, loopback 세션은 쓸 수 없다.
- 응답과 로그에 비밀번호 값을 넣지 않는다. 바디의 비밀번호는 진단 번들·이벤트·job 기록에 남지 않는다.

### A.3 세션 무효화

- 계정 레코드에 `credential_version`(정수, 없으면 0)을 더한다. 재설정·변경 때 1 올린다.
- 발급하는 access·refresh JWT에 `cv` claim을 넣는다. 검증할 때 계정의 현재 `credential_version`과 다르면 `PCV_TOKEN_CREDENTIAL_STALE`(401)로
  거절한다. claim이 없는 기존 token은 0으로 본다.
- 그래서 비밀번호가 바뀌면 그 계정의 기존 access·refresh token이 서비스 재시작과 관계없이 바로 무효가 된다. 본인 변경은 응답에 새 token
  쌍을 준다.

### A.4 CLI와 Web

- PCVCLI: `pcvcli account reset-password NAME --password-env VAR|--password-stdin --yes`, `pcvcli account change-password --current-password-env VAR
  --password-env VAR`. 비밀번호를 인자로 받지 않는다.
- Web 계정 화면: 행 메뉴 `비밀번호 재설정…`(새 비밀번호와 확인, 정책 안내, 대상 이름 표시). 사용자 메뉴 `비밀번호 변경`.

### A.5 감사와 시험

- 재설정·변경은 기존 이벤트 기록 경로에 operation, 수행자, 대상, 시각만 남긴다.
- C# 시험: 정책 실패, 권한 없음, 서비스 bearer 재설정, `cv` 불일치 거절, claim 없는 token 호환, 파일에 비밀번호 평문이 없음.
- CLI 시험: 인자로 비밀번호를 받지 않음, `--yes` 필수. Web node 시험: 비밀번호 값이 DOM·로그에 남지 않음.

## Nonclaims

- operational current는 `0.42.96-admin-smoke` 그대로다. 이 설계의 변경은 release train 전까지 설치본에 없다.
- S4 통과는 S4 판정 task 전에는 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
