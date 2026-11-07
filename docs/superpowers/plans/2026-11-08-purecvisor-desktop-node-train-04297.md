# release train `0.42.97` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-11-08 사용자 지시 "새 화면을 먼저 반영해"와 승인 `1,2,3,4`로 release train `0.42.97-admin-smoke`를 S4 판정보다 먼저 출발한다. `release-train.json` queue 14행(PR #94~#107: `configure-installed` LAN 옵션, Web Console 재설계 전체, 비밀번호 재설정 API)을 package부터 Lane 3까지 돌려 operational current로 승격한다. 그동안 S4 시연용 설치본 상태(계정, LAN 설정)를 보관했다가 되살리고, 새 화면의 설치본에서 S4 시연을 이어 간다. `ui-redesign-20261105`의 남은 Task 11~14(S4 시연, 끝 상태, 판정, C5 확인)는 이 campaign의 Task 14~17로 이관한다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 train과 직전 train(`docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-train-04296.md`, 마무리 `docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-train-04296-finish.md`)의 task 구조를 따른다. 직전 train처럼 두 PR로 나눈다: train branch `lane1/train-04297-20261108`(PR A: 출발, package, pair, fullgate, current-card, probe 기록, Lane 3 없음)를 merge한 뒤 `main`에서 Lane 3 PR B를 쓴다. payload commit은 이 opening의 `origin/main` `9036c62`(PR #107 merge)다. 설치본은 `0.42.96-admin-smoke`이고 S4용으로 LAN 노출(`--allow-lan`, 방화벽 관리 규칙 2개)과 계정 3개가 있다. 계정이 있으면 loopback 세션이 `409 PCV_LOOPBACK_SESSION_DISABLED`라 train 도구(설치본 smoke, current-card, probe)가 돌지 않는다. 그래서 계정 파일을 보관하고 loopback으로 돌린 뒤 train 끝에 되살린다. 2026-11-05 재클론으로 gitignore된 `artifacts/`가 없어 pair baseline package(`0.42.96`)와 smoke ISO를 먼저 복구한다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, `DesktopNode.Host.exe configure-installed`, `pcvcli account`, Edge/Chrome CDP 브라우저 스크립트(`web/scripts/verify-browser-ui-gates.mjs` 방식)

## 사용자 결정 (2026-11-08)

승인 원문: `1,2,3,4` (같은 날 사용자 지시 "새 화면을 먼저 반영해"에 대한 출발 승인 요청 1~4의 답). 직전 campaign의 ADR-0016 standing approval과 S4 승인을 그대로 옮긴다.

| 항목 | 범위 |
| --- | --- |
| 1 | `release train 0.42.97을 지금 출발한다. 이 UI campaign은 Task 11~14를 남긴 채 멈추고 train campaign을 연다. 적재는 queue 14행(PR #94~#107: UI 재설계 전부, 비밀번호 재설정, configure-installed LAN 옵션 변경).` 해석: `ui-redesign-20261105`를 닫고 Task 11~14를 이 campaign Task 14~17로 이관한다. Lane 0/1. push, PR. merge는 PR마다 사용자 merge 승인 |
| 2 | `Lane 2 host mutation: unsigned 개발 MSI 빌드, 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거(REMOVE_DATA 포함), service 중지·시작, ADR-0016 pcv-it- 일회용 VM), 설치본 확인 카드와 동작 확인.` 해석: 직전 train `train-departure` 범위와 같다(MSI install·repair·uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, fullgate os-mutation-gate, probe VM 생성·삭제(`pcv-it-` 접두사, disk는 `artifacts/` 아래), 설치본 브라우저 확인(읽기와 queued job)). baseline package 재빌드 포함 |
| 3 | `S4 상태 보존: train 전에 accounts.json을 저장소 밖 보호된 위치에 백업하고 치운 뒤 loopback으로 train을 돌린다. 끝나면 계정을 복원하고 새 설치본의 configure-installed로 LAN 설정(--allow-lan, 방화벽 관리 규칙 2개)을 다시 적용한 뒤 0.42.97 새 화면으로 S4 시연을 이어 간다. 보존 VM, template, pcv-it-s4-console은 건드리지 않는다(도구가 요구하면 멈추고 묻는다).` Task 2, Task 9 |
| 4 | `Lane 3: current-evidence.json을 0.42.97-admin-smoke로 승격한다.` Lane 3 `current-evidence.json` 쓰기. Task 11~13 |
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

- [ ] payload commit `9036c62`의 main push Development Gates와 Public Boundary가 success임을 run id로 적고, `release-train.json` `queue` 14행을 train `0.42.97-admin-smoke`의 `carriages` `[94, 95, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107]`로 옮겨 `status=running`, `departed_at`, `source_commit`을 적고 `queue`는 빈 목록으로 둔다. `docs/ga-ready/trains/0.42.97-admin-smoke.host-inputs.json`(ISO 경로는 Task 1 결과)을 쓴다. 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

## Task 1: 입력 복구 (baseline package와 smoke ISO)

- [ ] `0.42.96` payload source(`87a7deb`)의 임시 worktree에서 `build.ps1 -Version 0.42.96-admin-smoke -MsiProductVersion 0.42.96 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot <저장소 artifacts/admin-smoke-package-20261108-04296r>`와 `New-PcvAdminSmokeUpdatePackage.ps1`로 baseline package를 다시 만든다(MSI·ZIP·catalog SHA가 원래 evidence와 다를 수 있음을 기록). smoke ISO는 `artifacts/smoke-media-20261108/` 아래에 이 호스트에 이미 있는 도구(Windows ADK `oscdimg` 등)로 만들거나 이미 있는 ISO를 복사한다. 새 `Add-Type`/P/Invoke가 필요하면 멈추고 묻는다. pair `-PlanOnly`로 baseline을 받아 주는지 확인한다(받지 않으면 정차하고 rebaseline 설계를 묻는다). host mutation 없음(빌드만). 기록은 Task 3 commit에 적는다.

## Task 2: S4 상태 보관과 loopback 전환

- [ ] Lane 2. 지금 service 명령줄과 `accounts.json`을 보호된 위치에 백업하고 해시를 비교한다. `accounts.json`을 치우고, merge된 `main` 빌드본 `DesktopNode.Host.exe configure-installed`를 prefix 옵션과 `--allow-lan` 없이 실행해 loopback으로 되돌린다. 확인: service 명령줄에 LAN prefix와 `--allow-lan` 없음, listener 7777·80 loopback, 방화벽 관리 규칙 0, `POST /api/v1/auth/loopback-session` 200, Web 200, 보존 VM·template·`pcv-it-s4-console` 상태 그대로. 기록 `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md` 진행 기록 절(값 없이 사실만). 로컬 commit.

## Task 3: package

- [ ] clean HEAD에서 `build.ps1 -Version 0.42.97-admin-smoke -MsiProductVersion 0.42.97 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261108-04297`, `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`. `train-facts`로 package 문서(`docs/ga-ready/trains/0.42.97-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행). 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음. 로컬 commit(Task 1 기록 포함).

## Task 4: pair

- [ ] Lane 2. orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261108-04296-04297`, baseline Task 1 package, target Task 3 package). 여섯 bucket PASS, closed descriptor, `observation_error` 없음, 끝 상태 설치본 `0.42.97`·ARP 1개·service Running/Automatic·Web 200·보존 VM·template·`pcv-it-s4-console` 그대로. `train-facts`로 pair 문서. 로컬 commit.

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
