# ADR-0019: Web Console 정보 구조와 상호작용 규칙

상태: 채택
일자: 2026-11-05 (제안·채택 같은 날, 채택은 사용자 승인 `1,2,3,4,5`)

## 결정 마커

```text
DESKTOP_NODE_WEB_INFORMATION_ARCHITECTURE: sidebar-groups-vm-detail-tabs-v1
DESKTOP_NODE_WEB_INTERACTION_RULES: required-first-validate-inline-progress-inapp-confirm-v1
DESKTOP_NODE_WEB_CONSOLE_VIEW: fit-default-1024x768-fullscreen
DESKTOP_NODE_WEB_LEGACY_CONSOLE: frozen-until-retirement
```

## 맥락

- 2026-11-05 S4 사용자 시연(LAN의 다른 PC 브라우저)에서 Web Console의 배치 결함이 드러났다. 상단 `Clone VM` 버튼은 이름 없이
  실행돼 `PCV_VM_CLONE_NAME_REQUIRED`로 끝났고, 이름 칸은 VM 상세 자원 격자의 마지막 칸에 묻혀 있었다. VM SCREEN은
  `640x480`이 기본이고 canvas가 그 너비를 넘지 않아 키 입력이 화면에 반영됐는지 눈으로 볼 수 없었다. 같은 시연 전에 admin이
  비밀번호를 잊어 되살릴 방법이 없었다(계정 API는 목록·생성·비활성화뿐).
- grok 조사(2026-11-05, purecvisor-single `c45dbf5`와 이 저장소 `cbe5459` 비교)는 기술 기반이 아니라 화면 구성이 원인이라고
  정리했다. ADR-0018로 셸·Supanova 테마·모듈 구조는 이미 같다. 다른 것은 VM 상세 탭(요약, 콘솔, 스냅샷 등), 이름이 첫 칸인
  복제 모달, 생성 마법사, 앱 안 확인, 빈 상태, 작업 대기, 콘솔 맞춤·전체 화면이다.
- 결함 목록과 비교표는 설계 문서 1·2절에 있다.

## 결정

- ADR-0018(`single-edge-ui-v1`)은 그대로 유지한다. 파일 배치, `window.PCV` 모듈, 순서표 빌드, vendor 자산, Linux runtime
  화면 제외, WebSocket 미사용은 바뀌지 않는다. 프레임워크 도입과 새 셸 병행은 하지 않는다.
- **정보 구조**: 사이드바는 운영(대시보드), 워크로드(가상 머신, 템플릿), 인프라(네트워크), 관제(작업, 이벤트, 증적), 시스템(계정,
  진단), 도움말 묶음이다. 계정 관리는 admin 전용 화면으로 진단에서 나눈다. VM 상세는 요약, 콘솔, 체크포인트, 하드웨어, 게스트,
  작업 기록 6개 탭이고 마지막으로 연 탭을 기억한다. 헤더에는 상태 배지, 전원 조작, 콘솔 열기, 더 보기 메뉴(복제, 템플릿 잠금,
  내보내기, vmconnect, 강제 끄기, 삭제)만 둔다.
- **상호작용 규칙**:
  1. 필수 입력은 그 조작을 여는 모달의 첫 칸에 두고, 합리적인 제안값을 채운다(복제 이름은 `<원본>-clone`).
  2. 형식과 중복은 요청 전에 칸 아래에 알리고, 규칙을 통과해야 주 버튼이 켜진다. 서버 검사는 그대로 둔다.
  3. job은 조작한 자리에서 접수, 실행 중, 완료, 실패를 한 줄로 보여 준다. 작업 화면은 전체 기록이다.
  4. 파괴·되돌림 조작(삭제, 복원, 강제 끄기, 잠금 해제)은 결과를 설명하는 앱 안 확인 모달로 묻는다. 삭제는 대상 이름을 입력해야
     버튼이 켜진다. 새 셸은 `window.confirm`을 쓰지 않는다.
  5. 새 문구는 i18n 키로 두고 ko를 기본으로 한다.
- **콘솔**: 기본 프레임은 `1024x768`, 기본 표시는 창에 맞춤이고 실제 크기와 전체 화면(Fullscreen API), Ctrl+Alt+Del, 키보드 입력
  상태 표시를 둔다. 전송은 기존 frame·input API다.
- **옛 콘솔**: `web/index.legacy.html`, `web/app.js`, `web/styles.css`, `web/src/served-app.ts`, `web/src/served/*.ts`는 퇴역 task 전까지
  고치지 않는다(동결). 퇴역은 campaign `ui-redesign-20261105` Task 9이고, 2026-10-10 승인 3의 legacy-retirement campaign을 대신한다.
- **비밀번호 재설정**: Local API route, PCVCLI `account` 하위 명령, 계정 화면을 함께 만든다. 권한, 본인 변경, 감사, 마지막 admin
  보호, 세션 무효화는 설계 문서 부록에서 정한다.

## 결과

- campaign `ui-redesign-20261105`(Lane 1, Task 1~10 재설계, Task 11~14 S4 이관)가 이 결정을 구현한다. 단계마다 PR 하나이고
  merge는 PR마다 사용자 승인 뒤다.
- 설치본 반영은 release train 뒤다. 그때 S1~S3를 새 화면으로 다시 시연한다. 브라우저 동작은 설치본 시연 기록으로만 주장한다.
- 옛 콘솔을 읽는 정적 계약(`web/contracts/web-static-contracts.mjs`의 `verifyVmLifecycleActions` 등), parity, browser fixture는 Task 9
  전까지 그대로 통과해야 한다. 새 셸 시험은 Task 2부터 컴포넌트·탭·모달 단위로 더한다.

## 검증

- `npm run test:required --prefix web`, 새 node 시험(컴포넌트 상태, 탭 전환 가시성, 빈 이름이면 API 미호출), Task 10의 axe 접근성
  검사와 브라우저 시험, `dotnet test src/DesktopNode.sln -c Release`(clean HEAD), Delivery Release, PR gate.

## 관련 문서

- 설계: `docs/superpowers/specs/2026-11-05-purecvisor-desktop-node-ui-redesign-design.md`
- 계획: `docs/superpowers/plans/2026-11-05-purecvisor-desktop-node-ui-redesign.md`
- 선행 결정: ADR-0018, ADR-0017(시나리오 S1~S4), ADR-0006
- 시각 규격: `web/DESIGN.md`
