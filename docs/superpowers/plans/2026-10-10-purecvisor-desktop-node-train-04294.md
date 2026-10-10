# release train `0.42.94` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-10-10 승인 1로 `release-train.json` queue 78·79·81을 train `0.42.94-admin-smoke`로 package부터 Lane 3까지 한 PR로 돌려 operational current로 승격한다. 적재 세 행의 Lane 2 probe(`vm.create`, `web.console.shell`, `checkpoint.schedule.set` + S3 브라우저 예약 저장 재시연)를 설치본 `0.42.94`에서 한다. 승인 2(S4 campaign)와 승인 3(legacy-retirement campaign)은 이 campaign이 닫힌 뒤 그 순서로 연다. C5 runner 확인(2026-10-19 이후)은 이관 task로 남긴다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 단일 PR train(`pcv-single-pr-train-v2`). 직전 train plan은 `docs/superpowers/plans/2026-10-08-purecvisor-desktop-node-completion-20261008.md`(train `0.42.93`)이고 task 순서를 그대로 따른다. branch는 `lane1/train-04294-20261010`, payload commit은 이 opening의 `origin/main` `a054db3`(PR #82 merge; main push Development Gates `38047675817`, Public Boundary `38047675792` success)다. 적재 변경의 배경은 `docs/superpowers/plans/2026-10-09-purecvisor-desktop-node-audit-green-lean.md`(78), `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-s3-checkpoint.md`(79), `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure.md`(81, ADR-0018)다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, Playwright(Chromium) 브라우저 시연, `web/scripts/run-s3-checkpoint-scenario.mjs`

## 사용자 결정 (2026-10-10)

승인 원문: `1,2,3` (campaign `single-edge-frontend-structure-20261010` 최종 보고의 다음 승인 1~3에 대한 답). 직전 campaign의 ADR-0016 standing approval을 그대로 옮긴다.

| 항목 | 범위 |
| --- | --- |
| 1 | `train 0.42.94-admin-smoke 출발: queue 78·79·81을 고정하고 package build, 개발 호스트 pair host mutation(제품 Update/Rollback, ADR-0016 pcv-it- 접두사 VM 생성·구성·삭제, disk는 artifacts 아래), fullgate와 current-card, Lane 2 probe 기능군 vm.create·checkpoint.schedule.set·web.console.shell(설치본 브라우저에서 Single Edge 셸의 loopback 세션·VM 목록·S3 예약 저장 재시연), Lane 3 current-evidence.json 쓰기, 단일 PR train의 push/PR과 green CI 뒤 merge, merge 뒤 main push가 red면 revert PR.` 해석: `train-departure`. Lane 0/1/2/3. fullgate는 `full-admin-host-mutation-gate`(service-msi-hyperv + required os-mutation-gate)이며 mutation scope는 정책 `train-departure` 행과 같다: MSI install, MSI repair, MSI uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, fullgate os-mutation-gate (firewall rule, Event Log source, LAN listener; 끝 상태 PureCVisor firewall 규칙 0, loopback 복귀), probe VM create and delete(`pcv-it-` 접두사, disk는 artifacts 아래), 설치본 브라우저 시연(읽기와 queued job). Lane 3 `current-evidence.json` 쓰기. push, PR, green CI 뒤 merge. main push red면 revert PR |
| 2 | `train 뒤 S4 campaign을 연다(2026-10-10 승인 3의 순서; 설치본 --allow-lan/firewall 변경은 그 campaign의 기존 사전 승인 범위 안에서만).` 이 campaign이 닫히면 Task 10이 `pcv-campaign-open`으로 S4 campaign을 열고 이 행을 승인 원문으로 쓴다. criteria S3 닫기(`config/project-completion-criteria.json`)는 train branch에서 `config/`를 바꿀 수 없어 그 campaign의 첫 PR에서 한다 |
| 3 | `train 뒤 legacy-retirement campaign을 연다: web/index.legacy.html·app.js·styles.css·web/src/served/* 제거와 옛 static contracts·negative·web Pester·parity·browser fixture·migration manifest·Delivery/packaging/config pin 재기준선(6~8 checkpoint), Lane 1, push/PR, green CI 뒤 merge.` S4 campaign이 닫힌 뒤 연다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정, disk는 artifacts 아래 |
| 이관 | `single-edge-frontend-structure-20261010` Task 18(C5 runner 확인, `not_before` 2026-10-19; 원래 `s3-checkpoint-20261010` Task 4)을 Task 11로 옮긴다. 문장과 push, PR, green CI 뒤 merge는 원래 승인 그대로다 |

## 적재 변경

| PR | 변경 commit | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #78 | `1b04ff46` | hyperv | console frame throttle clock 주입, Verification 실패 테스트명 stderr 출력, EnabledState 전이 값 어휘와 vm.create inventory readback 대기(BL-0014), PR gate·ADR-0016 fixture 안정화 | `vm.create` (Task 5) |
| #79 | `04ffe117` | web | VM 상세 click 위임 핸들러가 form 안 type=submit 버튼을 건너뛰게 수정(BL-0016, checkpoint schedule 폼 저장 복구), S3 시나리오 스크립트와 회귀 테스트 | `checkpoint.schedule.set` + S3 재시연 (Task 7) |
| #81 | `4e109e70` | web | Single Edge 프론트엔드 구조 차용(ADR-0018): 로그인 페이지+앱 셸 `index.html`을 `/`에서 서빙, `window.PCV` 모듈 23개 `app.bundle.js`, help 카탈로그·문서 포털·PWA, Host 정적 content-type과 소스 404, `WebPayload.wxs` 생성; 옛 콘솔은 `index.legacy.html` 유지 | `web.console.shell` (Task 6) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner, fullgate route smoke VM, probe VM(`pcv-it-` 접두사)뿐이고 끝나면 지운다. probe VM disk는 `artifacts/` 아래에 둔다.
- token, credential, password는 command line, summary, evidence, 스크린샷에 남기지 않는다. guest 인증 정보는 호출 직전에 만들어 `-GuestCredential`로 넘긴다.
- evidence와 artifact는 새 이름(`-04294`, `-04293-04294`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다.
- baseline package는 `artifacts/admin-smoke-package-20261008-04293`(설치본 `0.42.93-admin-smoke+818d00f`, ARP 1개, service Running/Automatic, Web 200)이다. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`.
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). Lane 3 문서 도구의 pin 여섯 파일만 예외다. criteria S3 닫기와 backlog 분류 같은 `config/` 변경은 train merge 뒤 별도 PR이다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다. FAIL은 current에 쓰지 않는다.
- 같은 version fullgate 재실행 전에 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다(`same-version-msi-stale-productcodes`).
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다. 판정 뒤 남은 갭이 Task 11이 덮는 `deadline-wait`와 사용자 결정 갭뿐이면 새 campaign은 승인 2(S4)만 연다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 0: 출발

- [x] payload commit `a054db3`의 main push Development Gates `38047675817`와 Public Boundary `38047675792`가 success임을 적고, `release-train.json` `queue` 세 행을 train `0.42.94-admin-smoke`의 `carriages` `[78, 79, 81]`로 옮기고 `status=running`, `departed_at`, `source_commit`을 적으며 `queue`는 빈 목록으로 둔다. 검증: `PcvReleaseTrainContractTests`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-10): 출발 `2026-10-10T20:32:00+09:00`, payload `a054db355babbd37b466cc7d8711b12c2aa8dfb4`(PR #82 merge, origin/main), carriages `[78, 79, 81]`, queue 비움, train `status=running`. payload main push Development Gates `38047675817` success, Public Boundary Contract `38047675792` success. 설치본은 `0.42.93-admin-smoke+818d00f`(ARP 1개, service Running/Automatic, Web 200). host mutation 없음. operational current는 `0.42.93-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.94-admin-smoke -MsiProductVersion 0.42.94 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261010-04294`, `New-PcvAdminSmokeUpdatePackage.ps1`. `train-facts`로 package 문서(`docs/ga-ready/trains/0.42.94-admin-smoke.train-facts-input.json`, narrative는 적재 변경 세 행). 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음. 로컬 commit.

실행 기록(2026-10-10): clean HEAD `59af872`에서 build `42`초, `build_utc` `2026-10-10T11:34:23.0989417Z`, provenance `59af872e559d3debc6c0a2b0e0cb56014a50291a`, WiX `5.0.2+aa65968c`(source Product.wxs, ProductActions.wxs, WebPayload.wxs). MSI SHA-256 `a9a4ec7700b84535b5ce7af27738560c575e76b031ebf1e5dab9685b73431096`, payload SHA-256 `2e1b20b78cad161979227a79057531be5b0a9f5ba369f04ce6485b3f9b6a932b`, payload `32`(web 자산 27개 포함; 이전 train 8), update ZIP SHA-256 `f649c5d613069fa042666a288c993168c06838aeff9cf39d3ea329e772f40018`, catalog SHA-256 `623b77a5faf75949e823a27f6f862b0ab4b524deb88df0422b02722fb5f6c9eb`. update package는 첫 실행이 build 시간을 받지 않아(`build_seconds` null) ZIP·catalog·package-facts.json을 지우고 `-BuildSeconds 42`로 한 번 다시 만들었다(ZIP SHA 동일, catalog SHA만 바뀜). `train-facts` package `generated=20` `narrative=3`, `train-evidence --write` 뒤 `--check` `current`. `TrainEvidenceGoldenTests`와 `TrainFactsBuilderTests` `20/20`. host mutation 없음. operational current는 `0.42.93-admin-smoke` 그대로다.

## Task 2: pair

- [ ] orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1` `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261010-04293-04294`, baseline `artifacts/admin-smoke-package-20261008-04293`, target Task 1 package root). 여섯 bucket PASS, closed descriptor, `observation_error` 없음, 끝 상태 설치본 `0.42.94`·ARP 1개·service Running/Automatic·Web 200·보존 VM Off. `train-facts`로 pair 문서 6개. 로컬 commit.

실행 기록(2026-10-10, FAIL·정차): orchestrator `-PlanOnly` `ok=true`(host mutation 없음). `-Execute` 1차(root `artifacts/manual-admin-campaign-20261010-04293-04294`, 20:36 KST): readiness PASS, product Update/Rollback PASS, clean-host PASS, `burn-bootstrapper-lifecycle` FAIL `remove-absence product remains`(bundle install/repair/remove exit 모두 0, runner가 target MSI를 복원해 설치본 `0.42.94`). 환경 원인(서비스·파일 핸들 정리 지연) 가능성으로 30초 뒤 새 root `-r2`(20:47 KST, 196초)로 한 번 재실행: 같은 세 bucket PASS, Burn 같은 FAIL. 원인 확정: lifecycle bucket의 ZIP 제품 Update가 MSI 설치 전에 `web\samples`, `web\vendor\coolicons`, `web\vendor\pretendard\woff2`를 만들고, Windows Installer는 자신이 만든 빈 폴더만 지우며 생성된 `installer/WebPayload.wxs`에는 `RemoveFolder`가 없어 bundle 제거 뒤 그 디렉터리와 제품 root가 남는다 (0.42.93은 web 하위 디렉터리가 없어 통과). MSIX bucket과 descriptor는 돌지 않았고 closed descriptor 없음. host 끝 상태: 설치본 `0.42.94-admin-smoke+59af872`(복원), ARP `{9EDA7FF1-8CC2-47B8-9E08-C8F2212442C4}` `0.42.94` 1개, service Running/Automatic, Web 200, clean-host VM 제거됨, 보존 VM Off, `pcv-it-s2-source` Off(S2 template). train `0.42.94` 항목은 `release-train.json`에서 지우고 carriages 78·79·81은 payload commit의 queue 행으로 되돌렸다(다음 출발이 수정 PR 행과 함께 고정; `status=stopped` 항목을 남기면 completion 평가기와 그 저장소 결합 시험이 unfinished train gap을 내므로 재출발으로 대체되는 정차 train은 항목을 두지 않는다. 정차 사실은 이 기록·campaign·backlog·DOCUMENTATION_INDEX가 가진다). FAIL은 current에 쓰지 않는다. 수정은 Lane 1: `Update-PcvWebPayloadWix.ps1`이 생성 디렉터리마다 `RemoveFolder` component를 내고 `WixSourceContractVerifier`·Delivery·installer Pester의 component 수 pin을 맞춘 뒤 `0.42.95`로 재출발한다. backlog BL-0017(결함), BL-0018(Burn runner absence 진단).

## Task 3: fullgate

- [ ] `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들고(LAN prefix는 직전 manifest에서 읽어 환경 변수로만 넘김) supervisor `-DryRun -AllowHostMutation` 뒤 clean `0.42.94` 위에서 `full-admin-host-mutation-gate-20261010-04294`를 돌린다. 사후 검사(설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM Off). `train-facts`로 fullgate 문서. 로컬 commit.

## Task 4: installed current-card

- [ ] fullgate 문서를 렌더한 뒤 `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다. `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음. 스크립트를 root에 `capture-current-card.ps1`로 복사한다. `train-facts`로 current-card 문서. 로컬 commit.

## Task 5: probe `vm.create` (queue 78)

- [ ] 설치본 `0.42.94`에서 `pcvcli vm create pcv-it-probe-create-1010`(managed Generation 2, cpu 1, memory 512 MB, disk 8 GB, ISO는 smoke ISO, VM root `artifacts/pcv-it-probe-create-1010`)의 create job이 `succeeded`한 직후 `pcvcli --json vm list`와 `GET /api/v1/vms`가 `PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE` 없이 새 VM을 off 어휘(`stopped`)로 돌려주는지 확인한다(BL-0014 수정, `vm.create` inventory readback 대기). VM을 켜지 않는다. managed delete 뒤 VM과 폴더가 없음을 확인한다. evidence `docs/ga-ready/evidence/lane2-vm-create-readback-actual-vm-2026-10-10-04294.md`. 보존 VM은 그대로 둔다. 로컬 commit.

## Task 6: probe `web.console.shell` (queue 81)

- [ ] 설치본 `0.42.94`의 `http://127.0.0.1/`를 Playwright(Chromium)로 열어 Single Edge 셸이 token 입력 없이 loopback 세션으로 열리고(`#login-page` → `#app`, sessionStorage 세션, auth gate 없음), 사이드바 8 화면(dashboard, vms, network, jobs, activity, evidence, troubleshooting, help)이 렌더되며 VM 목록에 보존 VM이 보이고, Help 카탈로그(route 행 수), `docs.html` reader(장 목록), `manifest.json`·`sw.js`(registration `active`, CACHE_NAME), `offline.html`, 옛 콘솔 `/index.legacy.html` 200을 확인한다. 스크린샷은 TEMPLATE 규칙대로 `docs/ga-ready/demo/web-console-shell-20261010/`에 PNG(파일당 500 KB 이하, 사용자 홈 경로·LAN IP·호스트명·token 없음)로 둔다. host mutation 없음(읽기만). 기록 `docs/ga-ready/demo/web-console-shell-demo-2026-10-10.md`(TEMPLATE.md 형식, 확인자 칸 비움). 로컬 commit.

## Task 7: probe `checkpoint.schedule.set` + S3 브라우저 재시연 (queue 79)

- [ ] `npm run scenario:s3-checkpoint --prefix web -- --execute`로 `pcv-it-s3-source`(managed Generation 2, Off)와 checkpoint `s3-cp1`을 만든 뒤, Playwright로 새 셸의 VM 상세에서 checkpoint 생성·복원·예약 미리보기·예약 저장(폼 버튼 클릭, BL-0016 수정 확인)·readback(enabled, next due, last enqueued)·clear를 한 번씩 수행하고, `--verify-only --expect-count=<n>`으로 예약 실행을 확인한다. 끝에 스크립트 또는 API로 VM을 삭제하고 `pcv-it-s3-*` VM 0개, VM 폴더 없음을 확인한다. 기록 `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-10-04294.md`(TEMPLATE.md 형식, 판정은 브라우저 폼 예약 저장 PASS/FAIL 그대로). criteria S3 닫기는 Task 10 뒤 S4 campaign 첫 PR에서 한다. 로컬 commit.

## Task 8: Lane 3

- [ ] functional carry-forward, consume(`…-closed` descriptor와 consume manifest), payload commit `a054db3`의 main push를 `train-path-check`로 확인한 `main-push-payload` 문서, current-card `promoted-current`. `current-evidence.json`, `lane3-spec`, 문서 도구 dry-run·`-Apply`·`-Check`, `release-train.json` 승격(`operational_current=0.42.94-admin-smoke`, train `status=promoted`). `train-path-check`가 제품 경로를 보고하면 Lane 3를 쓰지 않고 멈춘다. 로컬 commit.

## Task 9: 종료와 train PR merge

- [ ] clean HEAD 종료 검증(`dotnet build -c Release`, Required CI shard 4개 로컬, Pester 4종), `train-path-check --payload a054db3 --head HEAD` exit `0`, push, PR 하나, green CI 뒤 merge(`--match-head-commit`). merge 직전 base는 `origin/main`과 같다. merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 10: 판정과 후속 campaign

- [ ] clean `main`에서 `pcvverify completion`을 돌려 결과를 기록한다. exit `0`이면 감사 문서로 완료를 적고 push, PR, green CI 뒤 merge한다. 그 밖이면 이 campaign을 `status=closed`로 닫고(Task 11은 `carried_tasks`로 새 campaign에 옮김) 승인 2로 S4 campaign을 `pcv-campaign-open`으로 연다(그 campaign 첫 PR에 criteria S3 닫기와 Task 11 이관 포함). 승인 3(legacy-retirement)은 S4 campaign이 닫힌 뒤 연다.

## Task 11: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `single-edge-frontend-structure-20261010` Task 18(원래 `s3-checkpoint-20261010` Task 4, `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 Task 8의 Lane 3 commit이 main에 merge된 뒤에만 `0.42.94-admin-smoke`로 바뀐다. 그 전 모든 task 기록은 `0.42.93-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다. package는 `AllowUnsignedDev`/`LocalTest`다.
- criteria S3·S4 닫기, backlog 분류, legacy 콘솔 제거는 이 campaign의 산출물이 아니다.
