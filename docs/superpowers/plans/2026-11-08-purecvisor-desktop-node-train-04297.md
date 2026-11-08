# release train `0.42.97` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-11-08 사용자 지시 "새 화면을 먼저 반영해"와 승인 `1,2,3,4`로 release train `0.42.97-admin-smoke`를 S4 판정보다 먼저 출발한다. `release-train.json` queue 13행(PR #94~#107, #96은 queue 행 없음: `configure-installed` LAN 옵션, Web Console 재설계 전체, 비밀번호 재설정 API)을 package부터 Lane 3까지 돌려 operational current로 승격한다. 그동안 S4 시연용 설치본 상태(계정, LAN 설정)를 보관했다가 되살리고, 새 화면의 설치본에서 S4 시연을 이어 간다. `ui-redesign-20261105`의 남은 Task 11~14(S4 시연, 끝 상태, 판정, C5 확인)는 이 campaign의 Task 14~17로 이관한다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 train과 직전 train(`docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-train-04296.md`, 마무리 `docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-train-04296-finish.md`)의 task 구조를 따른다. 직전 train처럼 두 PR로 나눈다: train branch `lane1/train-04297-20261108`(PR A: 출발, package, pair, fullgate, current-card, probe 기록, Lane 3 없음)를 merge한 뒤 `main`에서 Lane 3 PR B를 쓴다. payload commit은 이 opening의 `origin/main` `9036c62`(PR #107 merge)다. 설치본은 `0.42.96-admin-smoke`이고 S4용으로 LAN 노출(`--allow-lan`, 방화벽 관리 규칙 2개)과 계정 3개가 있다. 계정이 있으면 loopback 세션이 `409 PCV_LOOPBACK_SESSION_DISABLED`라 train 도구(설치본 smoke, current-card, probe)가 돌지 않는다. 그래서 계정 파일을 보관하고 loopback으로 돌린 뒤 train 끝에 되살린다. 2026-11-05 재클론으로 gitignore된 `artifacts/`가 없어 pair baseline package(`0.42.96`)와 smoke ISO를 먼저 복구한다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, `DesktopNode.Host.exe configure-installed`, `pcvcli account`, Edge/Chrome CDP 브라우저 스크립트(`web/scripts/verify-browser-ui-gates.mjs` 방식)

## 사용자 결정 (2026-11-08)

승인 원문: `1,2,3,4` (같은 날 사용자 지시 "새 화면을 먼저 반영해"에 대한 출발 승인 요청 1~4의 답). 직전 campaign의 ADR-0016 standing approval과 S4 승인을 그대로 옮긴다.

| 항목 | 범위 |
| --- | --- |
| 1 | `release train 0.42.97을 지금 출발한다. 이 UI campaign은 Task 11~14를 남긴 채 멈추고 train campaign을 연다. 적재는 queue 14행(PR #94~#107: UI 재설계 전부, 비밀번호 재설정, configure-installed LAN 옵션 변경).` 해석: 실제 queue 행은 13개다(#96은 설계 문서라 queue 행이 없다). `ui-redesign-20261105`를 닫고 Task 11~14를 이 campaign Task 14~17로 이관한다. Lane 0/1. push, PR. merge는 PR마다 사용자 merge 승인 |
| 2 | `Lane 2 host mutation: unsigned 개발 MSI 빌드, 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거(REMOVE_DATA 포함), service 중지·시작, ADR-0016 pcv-it- 일회용 VM), 설치본 확인 카드와 동작 확인.` 해석: 직전 train `train-departure` 범위와 같다(MSI install·repair·uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, fullgate os-mutation-gate, probe VM 생성·삭제(`pcv-it-` 접두사, disk는 `artifacts/` 아래), 설치본 브라우저 확인(읽기와 queued job)). baseline package 재빌드 포함 |
| 3 | `S4 상태 보존: train 전에 accounts.json을 저장소 밖 보호된 위치에 백업하고 치운 뒤 loopback으로 train을 돌린다. 끝나면 계정을 복원하고 새 설치본의 configure-installed로 LAN 설정(--allow-lan, 방화벽 관리 규칙 2개)을 다시 적용한 뒤 0.42.97 새 화면으로 S4 시연을 이어 간다. 보존 VM, template, pcv-it-s4-console은 건드리지 않는다(도구가 요구하면 멈추고 묻는다).` Task 2, Task 9 |
| 4 | `Lane 3: current-evidence.json을 0.42.97-admin-smoke로 승격한다.` Lane 3 `current-evidence.json` 쓰기. Task 11~13 |
| 5 | `1`(Task 4 정지 보고의 다음 승인 1번: "기준 VHD를 다시 마련하고 train을 이어 갑니다."). 해석: clean-host bucket의 기준 VHD가 2026-11-05 재클론 때 사라졌다. Microsoft에서 Windows Server 2022 평가판 VHD와 최신 LCU를 받아 `New-PcvCleanHostBaseVhd.ps1`로 복사본을 오프라인 서비싱(VHD mount, DISM으로 SSU 다음 LCU)한 뒤 Task 4부터 잇는다. 평가판 VHD는 사용자 대신 에이전트가 Microsoft 공식 링크로 받는다. 원본은 읽기만 한다. VM·service·방화벽 변경 없음. Task 3a |
| 6 | `1`(Task 4 FAIL 정지 보고의 다음 승인 1번: "train을 멈추고 호환성을 고친 뒤 다시 출발합니다. 정지 기록을 commit합니다(plan 실행 기록, backlog 행). Lane 1 PR을 엽니다. 이전 버전 Update 기능이 받을 수 있도록 payload에 web/app.js, web/styles.css를 최소 호환 파일로 남깁니다. 이 두 파일은 새 셸이 읽지 않습니다. 필수 목록의 위치를 찾아 그 근거도 PR에 적습니다. merge는 사용자 승인으로 합니다. 그 뒤 package부터 업그레이드 시험을 다시 돌립니다. 그동안 호스트는 제품 Rollback으로 0.42.96에 맞춰 둡니다."). 해석: `docs/DEVELOPMENT_PROCEDURE.md` §10에 따라 train `0.42.97`은 정차다. `release-train.json`에서 train 항목을 빼고 적재 13행을 `queue`로 되돌린다. 출발 승인 1~4는 정차로 끝난다. Task 4a는 정차 기록(Lane 1), Task 4b는 설치본 제품 Rollback(Lane 2, 승인 2의 제품 Update/Rollback 범위), Task 4c는 호환 수정(Lane 1, grok 구현·Claude 리뷰, queue 행 추가), Task 4d는 종료 검증·push·PR이다. merge는 사용자 merge 승인으로만 한다. 다시 출발할 때는 다음 patch version `0.42.98-admin-smoke`를 쓰고 출발 승인을 새로 받는다. 남은 Task 4~17은 그 train campaign으로 옮긴다 |
| merge | PR마다 사용자의 명시 merge 승인 문장(`merge_policy=none`). main push run이 red면 그 merge를 되돌리는 revert PR을 열고 멈춘다 |
| 이관 | `ui-redesign-20261105` Task 11(S4 시연 A), Task 12(S4 끝 상태, Lane 2, 2026-10-12 승인 범위), Task 13(S4 판정), Task 14(C5 확인)를 Task 14~17로 옮긴다. 원래 문장과 승인 범위는 그대로다 |

## 적재 변경

| PR | merge commit | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #94 | `271ab040` | host | `configure-installed`·`repair-installed`가 `--prefix`·`--web-prefix`를 받고, loopback이 아니면 `--allow-lan`을 service 명령줄에 넣는다 | `host.service.configure-lan` (Task 9) |
| #95 | `08c50e08` | host | 실행 중 service의 명령줄이 바뀌면 Stop → Configure → Start로 listener를 바꾼다(BL-0023) | `host.service.configure-lan` (Task 9) |
| #97 | `13fc25dd` | web | 공통 컴포넌트(확인 모달, 입력 검사, 알림, job 진행 줄, 탭, 빈 상태) | 없음(Task 7에서 함께 봄) |
| #98 | `ab9b6704` | web | VM 상세 6개 탭, 더 보기 메뉴, 복제 이름 첫 칸 모달, 삭제 이름 확인 모달 | `web.console.vm-detail` (Task 7) |
| #99 | `8b0ae171` | web | 콘솔 탭: 1024x768 기본, 창 맞춤, 전체 화면, Ctrl+Alt+Del, 입력 상태 칩 | `web.console.frame-view` (Task 7) |
| #100 | `16da5810` | web | 체크포인트 탭, 복원 확인 모달, 예약 카드, job 완료 뒤 즉시 반영(BL-0020) | `web.console.checkpoints` (Task 7) |
| #101 | `f0db33e4` | web | 새 VM 4단계 마법사 | `web.console.vm-create` (Task 7) |
| #102 | `eb0bdcf7` | web | 사이드바 개편, 템플릿·계정 화면, VM 목록 상태 칩 | `web.console.shell-views` (Task 7) |
| #103 | `289d98ea` | web | `window.confirm` 0, 화면 문구 i18n | `web.console.confirm-copy` (Task 7) |
| #104 | `213bfe36` | api | 계정 비밀번호 재설정(admin·본인), `cv` claim으로 재설정·disable 뒤 기존 token 무효화, PCVCLI와 Web | `api.account.password` (Task 8) |
| #105 | `8403d47d` | web | 옛 콘솔 퇴역 9a(계약·시험 새 셸 기준, Failed 칩) | 없음(Task 7) |
| #106 | `3d2fd89e` | web | 옛 콘솔 퇴역 9b(옛 파일 삭제, 옛 주소 404, MSI web payload) | 없음(Task 7, pair·fullgate의 payload 확인) |
| #107 | `449a62ce` | web | 브라우저 품질 게이트(axe-core 개발 전용), 강조 버튼 대비 | 없음(Task 7) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`, S2 template `pcv-it-s2-source`, S1 시연 VM `pcv-it-s4-console`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다. 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다. 새 VM은 clean-host runner, fullgate route smoke VM, probe VM(`pcv-it-probe-*`)뿐이고 끝나면 지운다. probe VM disk는 `artifacts/` 아래에 둔다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다. 계정 백업과 지금 service 명령줄(LAN 주소 포함)은 저장소와 제품 데이터 폴더 밖의 사용자 프로필 아래 보호된 위치에만 둔다(`REMOVE_DATA`가 지우지 않는 곳). 그 위치 경로는 문서에 적지 않는다. 백업은 S4 판정(Task 16)까지 두고 지울지는 사용자에게 묻는다.
- evidence와 artifact는 새 이름(`-04297`, `-04296-04297`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다(LAN 주소는 `<host-lan-ip>`).
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). 제품 결함이 나오면 train을 정차하고 수정 PR로 간다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다(기록은 plan·campaign·backlog). FAIL은 current에 쓰지 않는다.
- 같은 version 설치 전에는 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다). 장시간 도구 실행(build, pair, fullgate) 대기는 그 checkpoint 시간에 넣고, 넘으면 보고하고 멈춘다.

## Task 0: 출발

- [x] payload commit `9036c62`의 main push Development Gates와 Public Boundary가 success임을 run id로 적고, `release-train.json` `queue` 13행을 train `0.42.97-admin-smoke`의 `carriages` `[94, 95, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107]`로 옮겨 `status=running`, `departed_at`, `source_commit`을 적고 `queue`는 빈 목록으로 둔다. `docs/ga-ready/trains/0.42.97-admin-smoke.host-inputs.json`(ISO 경로는 Task 1 결과)을 쓴다. 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-08): 출발 `2026-11-08T05:26:00+09:00`, payload `9036c6249aad4385d04ec9e77aa7dbf3516fb31c`(PR #107 merge, origin/main), carriages `[94, 95, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107]`(13행), queue 비움, train `status=running`. payload main push Development Gates `42408233600` success, Public Boundary Contract `42408233594` success. `docs/ga-ready/trains/0.42.97-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`이고 Task 1이 만든다. 설치본은 `0.42.96`이고 S4용 LAN 노출과 계정 3개가 있다(Task 2가 보관한다). `dotnet test src/DesktopNode.Delivery.Tests -c Release` 781/781(release-train 계약 포함), `git diff --check` 통과. host mutation 없음. operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 1: 입력 복구 (baseline package와 smoke ISO)

- [x] `0.42.96` payload source(`87a7deb`)의 임시 worktree에서 `build.ps1 -Version 0.42.96-admin-smoke -MsiProductVersion 0.42.96 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot <저장소 artifacts/admin-smoke-package-20261108-04296r>`와 `New-PcvAdminSmokeUpdatePackage.ps1`로 baseline package를 다시 만든다(MSI·ZIP·catalog SHA가 원래 evidence와 다를 수 있음을 기록). smoke ISO는 `artifacts/smoke-media-20261108/` 아래에 이 호스트에 이미 있는 도구(Windows ADK `oscdimg` 등)로 만들거나 이미 있는 ISO를 복사한다. 새 `Add-Type`/P/Invoke가 필요하면 멈추고 묻는다. pair `-PlanOnly`로 baseline을 받아 주는지 확인한다(받지 않으면 정차하고 rebaseline 설계를 묻는다). host mutation 없음(빌드만). 기록은 Task 3 commit에 적는다.

실행 기록(2026-11-08): baseline package를 `87a7deb` 임시 worktree(`artifacts/` 아래, 끝나고 지움)에서 `artifacts/admin-smoke-package-20261108-04296r`로 다시 만들었다. build `51`초, `build_utc` `2026-11-07T20:28:54.3137908Z`, provenance `87a7deb9434c721a5e1f9f74dacc7581d0a98455`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`). MSI SHA-256 `3fcdada1cc5f390e8398ce424ba8ee6fb051ede845044c6a5b6bc6f04a021933`, payload `32`개 aggregate `237aaea06cb1acb7f4d620ff13e1114fdfb73aa40507ebcf397ea02362df513a`, update ZIP SHA-256 `806d4f88049eeaac24cfaaa9c713a5473f600d1dc8f24f0d211f0a5bc133f35e`(entry 32). 원래 `0.42.96` package evidence(MSI `326b867a…`, payload `b76c5310…`)와 SHA가 다르다(같은 source의 재빌드라도 비트가 같지 않다). 이 package는 pair baseline 입력일 뿐 `0.42.96` 승격 evidence를 대신하지 않는다. smoke ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, SHA-256 `2296bf3b9a520429432996c19791474813c30c9296257fc1af9d3723e73425b1`, Windows `StorageType=ISO`)다. 이 호스트에 `oscdimg`가 없어 README 하나만 든 ISO 9660 이미지를 저장소 밖 임시 Python 스크립트로 썼다(원래 `20261003` smoke media와 같은 구조, `Add-Type` 없음). pair `-PlanOnly`의 baseline 수락 확인은 target package가 필요해 Task 4에서 한다. host mutation 없음(빌드만). operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 2: S4 상태 보관과 loopback 전환

- [x] Lane 2. 지금 service 명령줄과 `accounts.json`을 보호된 위치에 백업하고 해시를 비교한다. `accounts.json`을 치우고, merge된 `main` 빌드본 `DesktopNode.Host.exe configure-installed`를 prefix 옵션과 `--allow-lan` 없이 실행해 loopback으로 되돌린다. 확인: service 명령줄에 LAN prefix와 `--allow-lan` 없음, listener 7777·80 loopback, 방화벽 관리 규칙 0, `POST /api/v1/auth/loopback-session` 200, Web 200, 보존 VM·template·`pcv-it-s4-console` 상태 그대로. 기록 `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md` 진행 기록 절(값 없이 사실만). 로컬 commit.

실행 기록(2026-11-08, Task 3 다음에 실행): 지금 service 명령줄과 `accounts.json`을 사용자 프로필 아래 보호된 위치(상속을 끊고 사용자·SYSTEM·Administrators만)에 백업했고 SHA-256이 원본과 같았다. 원본 `accounts.json`(data root 바로 아래)을 지웠다. Task 3 package의 `host-publish\DesktopNode.Host.exe`(`main` 제품 source)로 `service-action configure-installed`를 MSI custom action과 같은 `--product-root`, `--data-root`, `--service-exe`와 지금 명령줄의 `--batch-evidence-root`로, prefix 옵션과 `--allow-lan` 없이 실행해 exit 0이었다. 실행 중 service가 멈췄다 다시 시작해(PR #95 동작) listener 7777·80이 loopback이다. 명령줄 차이는 LAN prefix 둘이 loopback prefix로 바뀌고 `--allow-lan`이 빠진 것, web root 표기가 `DesktopNode\.\web`에서 `DesktopNode\web`(같은 폴더)으로 바뀐 것이다. `service-action firewall-remove`로 관리 규칙 두 개를 지웠다(Web 규칙은 `--firewall-local-port 80`을 같이 줘야 지워져 한 번 다시 실행). 확인: PureCVisor 방화벽 규칙 0, `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Auto, VM `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off·`pcv-it-s4-console` Running(그대로). 앞선 두 호출은 상태를 바꾸지 않았다: 첫 호출은 PowerShell 도구가 실행 전에 막았고(`Remove-Item`과 정규식 조합), 둘째 호출은 백업 폴더 권한만 바꾼 뒤 경로 인자 누락(`PCV_HOST_PRODUCT_ROOT_REQUIRED`, exit 2)으로 service·방화벽을 바꾸지 않았다. S4 시연 기록 갱신은 그 기록 branch를 다시 올리는 Task 14에서 한다. operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 3: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.97-admin-smoke -MsiProductVersion 0.42.97 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261108-04297`, `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`. `train-facts`로 package 문서(`docs/ga-ready/trains/0.42.97-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행). 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음. 로컬 commit(Task 1 기록 포함).

실행 기록(2026-11-08): Task 2보다 먼저 했다. Task 2의 loopback 전환에 이 package의 `main` 빌드 `DesktopNode.Host.exe`(`host-publish`)를 쓰려는 순서 조정이고 범위는 같다. clean HEAD `3a8da14`(product source는 payload `9036c62`와 같고 그 뒤 commit은 문서뿐)에서 build `40`초, `build_utc` `2026-11-07T20:30:36Z`, provenance `3a8da146e1d1d39bad2b6d5ff04b008deed8bdc8`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`). MSI SHA-256 `cc7127b63ef909f0e91378ac0e4e7987597ea1cb4e5dda225669c82f3fc7e6d7`, payload `29`개 aggregate `e2a0fd6b6be2764e4313d238a2450ce61f8d8062600d6932da0c1b20481c3641`(옛 콘솔 `web/app.js`, `web/styles.css`, `web/index.legacy.html`이 빠져 32→29), update ZIP SHA-256 `fdb2dce5a80d132c72321b0c352d49a55f1b440586133dacb324e3f08e5e4723`(entry 29), catalog SHA-256 `3eb365cd1adb89d27548bd6e4316090f65bbb1987d66b83ab83ea68c81443feb`. `train-facts` package `generated=20` `narrative=3`(입력의 `fullgate_final_firewall_rule_count`는 문자열이어야 해서 한 번 고쳐 다시 돌렸다), `train-evidence --write` 뒤 `--check` `current`. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `23/23`. host mutation 없음. operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 3a: clean-host base VHD 재생성

- [x] Lane 2(사용자 결정 5). Microsoft 공식 링크에서 평가판 VHD(`20348.169`)를 `artifacts/image-cache/windows-server-2022-eval-vhd/`로, Update Catalog에서 2026-10 LCU를 `artifacts/image-cache/lcu-kb5126655/`로 받는다. Windows PowerShell 5.1로 `New-PcvCleanHostBaseVhd.ps1` plan 뒤 `-Execute`(SSU cab 먼저, LCU 다음, 복사본만 서비싱). 검증: sidecar의 build `20348`·UBR·KB·SHA, 오프라인 이미지 build 읽기. clean-host run이 PASS하면 `-SetCurrentBasePath -Execute`로 current를 지정한다(Task 4 pair의 clean-host bucket이 그 run이다). VM·service·방화벽 변경 없음. 로컬 commit.

실행 기록(2026-11-08): Task 4 착수 점검에서 clean-host 기준 VHD(`20348.5622-20260930.vhd`)와 그 원본이 모두 없음을 확인하고 멈췄다(재클론 때 archive 저장소의 `artifacts/image-cache`가 사라졌다). 사용자 결정 5 뒤 첫 checkpoint는 batch 예산으로 멈췄고 사용자 `1`로 같은 task를 새 예산으로 이었다. 평가판 VHD는 `go.microsoft.com` fwlink가 `software-download.microsoft.com`으로 보내는 `20348.169…serverdatacentereval_en-us.vhd`(`10208214528` bytes)를 받았다. SHA-256 `588355586a3b99f1d47cee02f4861680a7e1bcb353582fbe7da11e2988e7562f`로, `docs/ga-ready/evidence/clean-host-base-vhd-offline-refresh-2026-09-30-5622.md`의 `source_sha256`과 같다. LCU는 Update Catalog의 `windows10.0-kb5126655-x64_8053239c….msu`(KB5126655, 2026-10-13, OS build `20348.5761`, `612997918` bytes)다. SHA1이 catalog 파일 이름 해시와 같고 Authenticode `Valid`(Microsoft Corporation), SHA-256은 `b1514e05147aace38c3357e4ba05717bd860f56e0a8bff7df77eadbbea2cdc1d`다. Windows PowerShell 5.1(관리자)에서 plan 뒤 `-Execute`를 `1604`초 돌렸다. 적용 순서는 `SSU-20348.5739-x64.cab`(SHA-256 `51c330f5…124ecc`) 다음 `Windows10.0-KB5126655-x64.cab`(SHA-256 `6757ccd7…783e2e`)다. 오프라인 이미지 build `20348`, UBR `5761`, edition `ServerDatacenterEval`. base `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`는 `18084412928` bytes, SHA-256 `c110fc1dab16b082aa12eb11058af09520c3b7ad9da7c14a13635bd8719dbd73`이고 sidecar는 `pcv-clean-host-base-vhd-v1`이다. 원본은 `source_modified=false`이고 수정 시각은 다운로드가 끝난 때 그대로다. `current-base.json`은 아직 쓰지 않았다(`current_base_changed=false`). Task 4 pair의 clean-host bucket이 이 base로 PASS하면 `-SetCurrentBasePath -Execute`로 지정한다. VM·service·방화벽 변경 없음. 범위 밖 발견은 BL-0028(보존 VM `pcv-guest-installed-04253-r1`의 differencing 부모가 없음)이다.

## Task 4: pair

- [ ] Lane 2. orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261108-04296-04297`, baseline Task 1 package, target Task 3 package). 여섯 bucket PASS, closed descriptor, `observation_error` 없음, 끝 상태 설치본 `0.42.97`·ARP 1개·service Running/Automatic·Web 200·보존 VM·template·`pcv-it-s4-console` 그대로. `train-facts`로 pair 문서. 로컬 commit.

실행 기록(2026-11-08, FAIL·정차): Task 3a base로 orchestrator `-PlanOnly`가 `ok=true`였다(host mutation 없음). `-Execute`는 root `artifacts/manual-admin-campaign-20261108-04296-04297`에서 07:55부터 07:58 KST까지 `168`초 돌았다. bucket 결과:

- PASS: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`.
- FAIL: `clean-host-windows-update`.
- 돌지 않음: `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.
- closed descriptor는 없다.

clean guest는 base `20348.5761`이고 Windows Update는 0건이었다. baseline `0.42.96` MSI 설치는 exit 0이었고 service Running(loopback), Web 200이었다. 그 설치본의 `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`(target update ZIP SHA-256 `fdb2dce5…`, catalog 일치)는 payload preflight에서 거절됐다. 오류는 `PCV_PRODUCT_UPDATE_PAYLOAD_FILE_MISSING`(`web\app.js, web\styles.css`)이다.

- 원인: `0.42.96` payload(`87a7deb`)의 `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` `Test-PcvDesktopNodeUpdatePayload`는 update payload에 `web\app.js`, `web\index.html`, `web\styles.css`를 요구한다. PR #106(옛 콘솔 퇴역 9b, `3d2fd89`)이 옛 콘솔 파일을 payload에서 지우고 필수 목록을 `web\app.bundle.js`, `web\index.html`, `web\style.css`로 바꿨다.
- 이 호스트의 lifecycle bucket이 PASS한 이유: 저장소의 새 wrapper로 돌기 때문이다. 이전 버전 wrapper 호환은 clean-host bucket만 본다.
- 제품 결함이라 환경 원인 재실행 대상이 아니다. 실패 VM은 runner가 지웠다.

끝 상태:

- 설치본 manifest `0.42.97-admin-smoke`(lifecycle bucket의 마지막 Update), MSI ARP `0.42.96` 1개.
- service Running/Automatic(loopback), Web 200, PureCVisor 방화벽 규칙 0.
- `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running(그대로).

clean-host가 PASS하지 않아 `current-base.json`은 지정하지 않았다. backlog는 BL-0029와 BL-0030이다. 사용자 결정 6으로 정차한다. operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 4a: 정차 기록

- [x] Lane 1(사용자 결정 6). Task 4 실행 기록과 backlog BL-0029·BL-0030을 쓴다. `release-train.json`에서 train `0.42.97-admin-smoke`를 빼고 적재 13행을 `queue`로 되돌린다(payload `9036c62`의 내용 그대로). campaign에 Task 4a~4d를 넣는다. 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-08): `release-train.json`을 payload `9036c62`의 내용으로 되돌렸다. 그 결과 queue는 13행이고 train `0.42.97-admin-smoke` 항목은 없다. Task 0~3의 package 문서(`docs/ga-ready/evidence/admin-smoke-package-2026-11-08-04297.md`)와 `docs/ga-ready/trains/0.42.97-admin-smoke.*`는 정차 train 기록으로 남긴다(train `0.42.94`·`0.42.95` 정차 때와 같다). backlog에는 두 행을 더했다. BL-0029는 이전 버전 제품 Update 거절이고, BL-0030은 pair lifecycle bucket이 이전 버전 wrapper를 보지 않는 시험 공백과 호환 파일 종료 시점이다. campaign은 `checkpoint_limit` 23, `next_task` Task 4b다. 검증은 `dotnet test src/DesktopNode.Delivery.Tests -c Release` `781/781`, `git diff --check` 통과다. host mutation은 없다.

## Task 4b: 설치본 제품 Rollback

- [ ] Lane 2(사용자 결정 6, 승인 2의 제품 Update/Rollback 범위). 설치본 wrapper로 `-Action Rollback`을 plan(`-WhatIf` 또는 plan 출력) 뒤 실행해 `0.42.96-admin-smoke`로 되돌린다. 확인:
  - manifest `0.42.96`, ARP `0.42.96` 1개.
  - service Running/Automatic, loopback listener, Web 200, PureCVisor 방화벽 규칙 0.
  - 계정 파일은 보관 위치에 그대로 있다.
  - 보존 VM·template·`pcv-it-s4-console`은 그대로다.

  기록은 이 plan에 남긴다. 로컬 commit.

## Task 4c: 이전 버전 제품 Update 호환 수정

- [ ] Lane 1(사용자 결정 6). 이 branch에서 `lane1/n1-update-compat-20261108`을 만든다. 이 PR은 train PR이 아니라 정차 기록과 수정을 함께 담는다. grok이 구현하고 Claude가 리뷰한다. 바꿀 것:
  - `web/app.js`, `web/styles.css`를 새 셸이 읽지 않는 최소 호환 파일(설명 주석만)로 되살려 `web/payload-manifest.json` `files`에 넣는다.
  - `Update-PcvWebPayloadWix.ps1 -Apply`로 `installer/WebPayload.wxs`를 다시 만든다. MSI가 두 파일을 소유하므로 제거 뒤 제품 root에 남지 않는다.
  - Task 9b 계약의 `web\app.js`·`web\styles.css` 부재 단언과 관련 spec·pin을 "호환 파일이 있다"로 바꾸고, 이유(`0.42.96` 이하 설치본의 update preflight)를 적는다.
  - `release-train.json` `queue`에 이 수정 행을 더한다.

  검증:
  - `87a7deb`의 `Test-PcvDesktopNodeUpdatePayload`를 새 build의 update payload에 읽기 전용으로 돌려 `ok=true`인지 본다.
  - `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `npm run test:required --prefix web`.
  - Pester 세 종(packaging, installer, web), `git diff --check`.

  경계: `src/DesktopNode.Host`는 바꾸지 않는다. 새 `Add-Type`/P/Invoke는 없다. 로컬 commit.

## Task 4d: 수정 PR

- [ ] Lane 1. clean HEAD 종료 검증(`dotnet build src/DesktopNode.sln -c Release`, `Invoke-PcvPrGate.ps1`, Pester 4종)을 하고 push, PR을 연다.
  - PR 본문에 정차 기록, 필수 목록 위치(`PcvDesktopNodeProduct.psm1` `Test-PcvDesktopNodeUpdatePayload`), 호환 파일 근거를 적는다.
  - queue 행의 `pr`과 `merge_commit`을 채우고 green Required CI를 기다린다.
  - merge는 사용자 merge 승인으로만 한다.
  - 이 campaign은 여기서 닫는다. Task 4~17은 다시 출발할 train `0.42.98-admin-smoke` campaign으로 옮긴다.

## Task 5: fullgate

- [ ] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들고(LAN prefix는 환경 변수로만 넘김) supervisor `-DryRun -AllowHostMutation` 뒤 `full-admin-host-mutation-gate-20261108-04297`을 돌린다. 사후 검사(설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM·template·`pcv-it-s4-console` 그대로). `train-facts`로 fullgate 문서. 로컬 commit.

## Task 6: installed current-card

- [ ] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다. `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0(`pcv-it-s2-source`는 S2 template, `pcv-it-s4-console`은 S4 시연 VM으로 기록. 도구가 받지 않으면 멈추고 묻는다), secret 없음. `train-facts`로 current-card 문서. 로컬 commit.

## Task 7: probe Web Console 재설계 (queue 97~103, 105~107)

- [ ] Lane 2. 설치본 `0.42.97`의 `http://127.0.0.1/`를 loopback 세션으로 브라우저(CDP)에서 연다. 사이드바 화면 전환, 템플릿·계정 화면, VM 목록 상태 칩, 옛 주소(`/index.legacy.html`, `/app.js`, `/styles.css`) 404를 본다. 새 VM 마법사로 `pcv-it-probe-wizard-1108`(Generation 2, smoke ISO, disk `artifacts/` 아래)을 만들고 켠 뒤 콘솔 탭(맞춤, 전체 화면 버튼, 입력 상태 칩), 체크포인트 탭(생성, 복원 확인 모달, 예약 미리보기·저장 뒤 즉시 반영, 예약 지우기), 더 보기 메뉴의 복제 모달(이름 첫 칸, 이름 `pcv-it-probe-clone-1108`), 삭제 이름 확인 모달로 두 VM을 지운다. 캡처 `docs/ga-ready/demo/web-console-redesign-20261108/`(PNG, 사용자 홈 경로·LAN IP·호스트명·token 없음), 기록 `docs/ga-ready/demo/web-console-redesign-demo-2026-11-08.md`(확인자 칸 비움). 끝 상태 `pcv-it-probe-*` 0개. 로컬 commit.

## Task 8: probe 비밀번호 재설정 (queue 104)

- [ ] Lane 2. 설치본 `0.42.97` loopback에서 계정이 없는 동안 Local API·PCVCLI의 재설정 route가 인증 없이 거절되는지 확인한다. 그다음 Task 9 계정 복원 뒤, 에이전트용 operator 계정 `pcv-s4-operator`로만 본인 재설정 경로를 돈다: 로그인 token을 받고, 본인 재설정 뒤 옛 token이 `401`, 새 비밀번호 로그인이 성공하는지 본다. 에이전트가 그 계정의 현재 비밀번호를 갖고 있지 않으면 이 단계는 하지 않고 Task 14 사용자 시연 항목으로 넘긴다(admin `pcv-admin2` 비밀번호는 사용자 전용이다). 비밀번호와 token은 실행 경계에서 만들고 남기지 않는다. evidence `docs/ga-ready/evidence/lane2-account-password-reset-2026-11-08-04297.md`. 로컬 commit.

## Task 9: S4 복원과 LAN 재적용 (queue 94, 95)

- [ ] Lane 2. Task 2 백업의 `accounts.json`을 되돌리고 해시를 비교한다. 설치본 `0.42.97`의 `configure-installed`를 백업한 LAN prefix와 `--allow-lan`으로 실행한다(PR #94: prefix 옵션과 `--allow-lan`이 service 명령줄에 들어감, PR #95: 실행 중 service가 Stop → Configure → Start로 새 listener를 연다). 확인: 명령줄, listener 7777·80 LAN 주소, 방화벽 관리 규칙 2개(Private, Inbound), loopback 세션 `409`, 계정 3개(`pcv-admin` disabled, `pcv-admin2`·`pcv-s4-operator` enabled), Web 200(LAN 주소). evidence `docs/ga-ready/evidence/lane2-service-configure-lan-2026-11-08-04297.md`(LAN 주소는 `<host-lan-ip>`). 로컬 commit.

## Task 10: train PR A

- [ ] clean HEAD 종료 검증(`dotnet build src/DesktopNode.sln -c Release`, `Invoke-PcvPrGate.ps1` 또는 Required CI shard 4개 로컬, Pester 4종, `npm run test:public-source-safety --prefix web` 신규 0), `train-path-check --payload 9036c62 --head HEAD` exit `0`, push, PR. green CI 뒤 사용자 merge 승인으로 merge(`--match-head-commit`). merge 뒤 main push run을 기다리고 red면 revert PR을 열고 멈춘다.

## Task 11: Lane 3 데이터와 lane3-spec

- [ ] Lane 3. `main`에서 branch를 만든다. consume manifest, functional carry-forward·pair consume·`main-push`(PR A merge의 main push run 인용) facts를 쓴 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`. `current-evidence.json`과 `lane3-spec --write`. 로컬 commit.

## Task 12: 승격 문서와 release-train 승격

- [ ] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1` dry-run, `-Apply`, `-Check`와 Lane 3 숨은 단계(descriptor chain, ledger 행, index 절, D2 시험, spec pin, current evidence 시험 version 줄). `release-train.json` train `0.42.97-admin-smoke` `status=promoted`, `operational_current=0.42.97-admin-smoke`. `Update-PcvCurrentEvidenceDocs.ps1 -Apply`/`-Check`, `Update-PcvContractSpecPins.ps1 -Apply`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

## Task 13: Lane 3 PR B

- [ ] clean HEAD 종료 검증(Task 10과 같음). push, PR. green CI 뒤 사용자 merge 승인으로 merge. merge 뒤 main push run을 기다리고 red면 revert PR을 열고 멈춘다.

## Task 14: S4 사용자 시연 A (이관: `ui-redesign-20261105` Task 11)

- [ ] 원래 문장: Task 4 전에 만든 admin 계정으로 자기 PC 브라우저에서 S1~S3을 시연한다. 에이전트는 시연 전후 상태(service 구성, 방화벽 규칙, 계정 목록, VM)를 읽어 기록하고 확인자 칸은 사용자가 채운다. 이 기록만 "다른 PC" 시연이다. 리허설 R은 S4 판정 근거로 쓰지 않는다. 2026-11-08 변경: 시연은 설치본 `0.42.97`의 새 화면으로 이어 간다(S2 복제 이름은 복제 모달 첫 칸). 기록은 `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md`이고 초안은 로컬 branch `lane1/s4-demo-record-20261108` `cf2b2f6`에 있다. 사용자가 시연 결과를 알리면 처리한다.

## Task 15: S4 끝 상태 (이관: `ui-redesign-20261105` Task 12)

- [ ] Lane 2. 원래 문장: 끝 상태는 loopback 복귀(같은 `main` 빌드본 `configure-installed`를 prefix 옵션과 `--allow-lan` 없이 실행, Task 4 전 service 명령줄과 같아야 한다), 방화벽 규칙 0, 에이전트 operator 계정 disable, `pcv-it-s4-*` VM 정리다. 기록한다. 계정 파일 삭제는 이 task가 하지 않는다. 2026-11-08 메모: 설치본 `0.42.97`의 `configure-installed`가 PR #94·#95를 담는다.

## Task 16: criteria S4 판정 (이관: `ui-redesign-20261105` Task 13)

- [ ] Lane 1. 원래 문장: 시연 기록이 설계 1절의 판정을 모두 담으면 `config/project-completion-criteria.json` S4를 `passed`로 두고 시연 기록 링크를 적는다. push, PR, 사용자 merge 승인 뒤 merge. 판정 전에는 S4 통과를 주장하지 않는다. Task 2 백업을 지울지 사용자에게 묻는다.

## Task 17: C5 runner 확인 (이관: `ui-redesign-20261105` Task 14)

- [ ] 원래 문장: `not_before` 2026-10-19. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR. merge는 사용자 승인 뒤.

## Nonclaims

- operational current는 Task 13의 Lane 3 PR B가 `main`에 merge된 뒤에만 `0.42.97-admin-smoke`로 바뀐다. 그 전 모든 task 기록은 `0.42.96-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다. package는 `AllowUnsignedDev`/`LocalTest`다.
- S4 통과는 Task 16 판정 전에는 주장하지 않는다. 재빌드한 baseline package는 원래 `0.42.96` 승격 evidence를 대신하지 않는다.
