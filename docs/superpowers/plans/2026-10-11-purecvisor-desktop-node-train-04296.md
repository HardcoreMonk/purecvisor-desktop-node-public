# release train `0.42.96` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-10-11 승인 2로 `release-train.json` queue 78·79·81·84·86을 train `0.42.96-admin-smoke`로 package부터 Lane 3까지 한 PR로 돌려 operational current로 승격한다. train `0.42.94`(BL-0017)와 `0.42.95`(BL-0019)가 정차했고 수정 PR #84와 #86이 merge됐다. 적재 행의 Lane 2 probe(`vm.create`, `web.console.shell`, `checkpoint.schedule.set` + S3 브라우저 재시연; `burn.lifecycle`은 pair Burn bucket PASS)를 설치본 `0.42.96`에서 한다. 승인 3(S4 campaign, 그 뒤 legacy-retirement)은 이 campaign이 닫힌 뒤 연다. C5 runner 확인(2026-10-19 이후)은 이관 task로 남긴다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 단일 PR train(`pcv-single-pr-train-v2`). 직전 train plan은 `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-train-04295.md`(Task 6 정차, Task 0~5 PASS)와 `docs/superpowers/plans/2026-10-08-purecvisor-desktop-node-completion-20261008.md`(train `0.42.93`, 승격)이다. branch는 `lane1/train-04296-20261011`, payload commit은 이 opening의 `origin/main`(PR #86 merge)이다. baseline package는 `artifacts/admin-smoke-package-20261010-04295`(0.42.95, ZIP·catalog 포함)이고 설치본은 `0.42.95-admin-smoke+b9898cf`(PR #86 dev probe의 Rollback 뒤)다. S3 예약 실행은 schedule 저장 뒤 한 interval(최소 60분)이 지나야 생기므로 브라우저 시연(Task 7)과 예약 실행 확인(Task 9) 사이에 Lane 3(Task 8)을 둔다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, Playwright(Chromium), `web/scripts/run-s3-checkpoint-scenario.mjs`

## 사용자 결정 (2026-10-11)

승인 원문: `1,2,3` (train `0.42.95` 정차 보고의 다음 승인 1~3에 대한 답; 이 campaign은 승인 2). 직전 campaign의 ADR-0016 standing approval을 그대로 옮긴다.

| 항목 | 범위 |
| --- | --- |
| 2 | `수정 merge 뒤 train 0.42.96 출발: queue 78·79·81·84 + 수정 PR 행, package build, pair host mutation(ADR-0016 범위), fullgate(os-mutation-gate 포함)·current-card, Lane 2 probe 3종(S3 재시연 포함), Lane 3 current-evidence 쓰기, 단일 PR merge, main push red면 revert PR. baseline은 설치본 0.42.95 package.` 해석: `train-departure`. Lane 0/1/2/3. mutation scope는 정책 `train-departure` 행과 같다: MSI install, MSI repair, MSI uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, fullgate os-mutation-gate (firewall rule, Event Log source, LAN listener; 끝 상태 PureCVisor firewall 규칙 0, loopback 복귀), probe VM create and delete(`pcv-it-` 접두사, disk는 artifacts 아래), 설치본 브라우저 시연(읽기와 queued job). Lane 3 `current-evidence.json` 쓰기. push, PR, green CI 뒤 merge. main push red면 revert PR |
| 3 | `train 뒤 S4 campaign, 그 뒤 legacy-retirement campaign(기존 승인 그대로).` 이 campaign이 닫히면 Task 11이 `pcv-campaign-open`으로 S4 campaign을 연다(criteria S3 닫기와 C5 이관 포함) |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정, disk는 artifacts 아래 |
| 이관 | `shell-nav-view-sync-20261011` Task 4(C5 runner 확인, `not_before` 2026-10-19; 원래 `completion-20261008` Task 10)를 Task 12로 옮긴다 |

## 적재 변경

| PR | 변경 commit | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #78 | `1b04ff46` | hyperv | console frame throttle clock 주입, EnabledState 전이 값 어휘와 vm.create inventory readback 대기(BL-0014), PR gate·ADR-0016 fixture 안정화 | `vm.create` (Task 5) |
| #79 | `04ffe117` | web | VM 상세 click 위임 핸들러가 form 안 type=submit 버튼을 건너뛰게 수정(BL-0016), S3 시나리오 스크립트와 회귀 테스트 | `checkpoint.schedule.set` + S3 재시연 (Task 7, Task 9) |
| #81 | `4e109e70` | web | Single Edge 프론트엔드 구조 차용(ADR-0018): 로그인 페이지+앱 셸 `index.html`을 `/`에서 서빙, `app.bundle.js`, help 카탈로그·문서 포털·PWA, Host 정적 content-type과 소스 404, `WebPayload.wxs` 생성; 옛 콘솔은 `index.legacy.html` 유지 | `web.console.shell` (Task 6) |
| #84 | `3d50f14b` | installer | 생성 `WebPayload.wxs`의 web 디렉터리마다 `RemoveFolder`, `Product.wxs`의 web 폴더·INSTALLFOLDER 제거(BL-0017) | `burn.lifecycle` = pair Burn bucket PASS (Task 2) |
| #86 | `179bd198` | web | Single Edge nav가 보이는 section의 주인(`PCV.nav.activeView`), 옛 view 상태 동기화, 도움말 전용 section, `#/view` hash(BL-0019) | `web.console.shell` (Task 6) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 `pcv-it-s2-source`(S2 template)의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner, fullgate route smoke VM, probe VM(`pcv-it-` 접두사)뿐이고 끝나면 지운다. probe VM disk는 `artifacts/` 아래에 둔다.
- token, credential, password는 command line, summary, evidence, 스크린샷에 남기지 않는다. guest 인증 정보는 호출 직전에 만들어 `-GuestCredential`로 넘긴다.
- evidence와 artifact는 새 이름(`-04296`, `-04295-04296`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
- baseline package는 `artifacts/admin-smoke-package-20261010-04295`. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`.
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). Lane 3 문서 도구의 pin 여섯 파일만 예외다. criteria S3 닫기와 backlog 분류 같은 `config/` 변경은 train merge 뒤 별도 PR이다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다(train 항목은 `release-train.json`에서 지우고 queue를 복귀, 기록은 plan·campaign·backlog). FAIL은 current에 쓰지 않는다.
- 같은 version fullgate 재실행 전에 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다). Task 9의 예약 실행 대기는 checkpoint 시간에 넣지 않는다(대기 중 다른 task를 하지 않으면 그 사이를 기다린다).

## Task 0: 출발

- [x] payload commit(PR #86 merge)의 main push Development Gates와 Public Boundary가 success임을 run id로 적고, `release-train.json` `queue` 다섯 행을 train `0.42.96-admin-smoke`의 `carriages` `[78, 79, 81, 84, 86]`로 옮기고 `status=running`, `departed_at`, `source_commit`을 적으며 `queue`는 빈 목록으로 둔다. `docs/ga-ready/trains/0.42.96-admin-smoke.host-inputs.json`을 commit한다. 검증: `PcvReleaseTrainContractTests`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-11): 출발 `2026-10-11T01:52:00+09:00`, payload `87a7deb9434c721a5e1f9f74dacc7581d0a98455`(PR #86 merge, origin/main), carriages `[78, 79, 81, 84, 86]`, queue 비움, train `status=running`. payload main push Development Gates `38069202154` success, Public Boundary Contract `38069202145` success. 설치본은 `0.42.95-admin-smoke+b9898cf`(PR #86 dev probe의 Rollback 뒤; ARP 1개, service Running/Automatic, Web 200). `docs/ga-ready/trains/0.42.96-admin-smoke.host-inputs.json`은 opening commit에 있다. host mutation 없음. operational current는 `0.42.93-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.96-admin-smoke -MsiProductVersion 0.42.96 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261011-04296`, `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`. `train-facts`로 package 문서(`docs/ga-ready/trains/0.42.96-admin-smoke.train-facts-input.json`, narrative는 적재 변경 다섯 행). 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음. 로컬 commit.

실행 기록(2026-10-11): clean HEAD `e07113c`에서 build `42`초, `build_utc` `2026-10-10T16:52:56.3848655Z`, provenance `e07113c5f555715955856897c687c7c8843db1dc`, WiX `5.0.2+aa65968c`. MSI SHA-256 `326b867a161ffa5038f4b3cecd8405ca0999cff00875c4a9fb8f5ac92f3cdeed`, payload SHA-256 `b76c53108e93b37f6d8ca9236f51a1aee5ea252f67c9ffc1ca68883983449104`, payload `32`, update ZIP SHA-256 `e4324e2420724dc4383b142debb600465dee84d415243183f138e602530c6a94`, catalog SHA-256 `424b5ec6daf3722bc9f2a6cf8ffa18bf6f047161a0f6d31aba5351d4d491db34`. `train-facts` package `generated=20` `narrative=3`, `train-evidence --write` 뒤 `--check` `current`. `TrainEvidenceGoldenTests`와 `TrainFactsBuilderTests` `22/22`. host mutation 없음. operational current는 `0.42.93-admin-smoke` 그대로다.

## Task 2: pair

- [x] orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261011-04295-04296`, baseline `artifacts/admin-smoke-package-20261010-04295`, target Task 1 package root). 여섯 bucket PASS(Burn bucket PASS가 queue 84의 Lane 2 probe다), closed descriptor, `observation_error` 없음, 끝 상태 설치본 `0.42.96`·ARP 1개·service Running/Automatic·Web 200·보존 VM Off. `train-facts`로 pair 문서 6개. 로컬 commit.

실행 기록(2026-10-11): `-PlanOnly` `ok=true`(host mutation 없음, 설치본 `0.42.95-admin-smoke+b9898cf`가 baseline version과 같아 baseline_alignment required=false). `-Execute`로 여섯 bucket PASS(readiness, product Update/Rollback, clean-host, burn-bootstrapper-lifecycle — queue 84의 Lane 2 probe, BL-0017 수정 뒤 제품 root 부재 확인 —, msix, ops-summary), closed descriptor `manual-admin-campaign-20261011-04295-04296-closed`, observations 0개 `observation_error` 0개, restoration 없음. 끝 상태: 설치본 `0.42.96-admin-smoke+e07113c5f555715955856897c687c7c8843db1dc`, ARP `{EBCDF75B-F5A7-49D1-92BE-84F442F67E99}` `0.42.96` 1개, service Running/Automatic, Web 200, 보존 VM과 `pcv-it-s2-source` Off. campaign root `artifacts/manual-admin-campaign-20261011-04295-04296`. `train-facts`로 pair 문서 6개(ops-summary 10, update-rollback 23, clean-host 21, burn 16, msix 17, pair-descriptor 17 생성값), `train-evidence --write` 뒤 `--check` 모두 `current`. golden 시험 `22/22`. operational current는 `0.42.93-admin-smoke` 그대로다.

## Task 3: fullgate

- [ ] `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들고(LAN prefix는 직전 manifest에서 읽어 환경 변수로만 넘김) supervisor `-DryRun -AllowHostMutation` 뒤 clean `0.42.96` 위에서 `full-admin-host-mutation-gate-20261011-04296`을 돌린다. 사후 검사(설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM Off). `train-facts`로 fullgate 문서. 로컬 commit.

## Task 4: installed current-card

- [ ] fullgate 문서를 렌더한 뒤 `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다. `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0(`pcv-it-s2-source`는 S2 template로 기록), secret 없음. 스크립트를 root에 `capture-current-card.ps1`로 복사한다. `train-facts`로 current-card 문서. 로컬 commit.

## Task 5: probe `vm.create` (queue 78)

- [ ] 설치본 `0.42.96`에서 Local API로 `pcv-it-probe-create-1011`(managed Generation 2, cpu 1, memory 512 MB, disk 8 GB, smoke ISO, VM root `artifacts/pcv-it-probe-create-1011`)을 만들고 create job `succeeded` 직후 `GET /api/v1/vms`와 `pcvcli --json vm list`가 `PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE` 없이 새 VM을 off 어휘로 돌려주는지 확인한다(BL-0014). VM을 켜지 않는다. managed delete 뒤 VM과 VM 디렉터리가 없음을 확인한다. evidence `docs/ga-ready/evidence/lane2-vm-create-readback-actual-vm-2026-10-11-04296.md`. 로컬 commit.

## Task 6: probe `web.console.shell` (queue 81, 86)

- [ ] 설치본 `0.42.96`의 `http://127.0.0.1/`를 Playwright(Chromium)로 열어 Single Edge 셸이 token 입력 없이 loopback 세션으로 열리고, 사이드바 8 화면 각각과 `#/vms` hash 전환마다 그 section만 보이며 polling 재렌더 뒤에도 유지되고, VM 목록에 보존 VM이 보이며, 도움말 카탈로그·`docs.html` reader·`manifest.json`·`sw.js`(registration `active`)·`offline.html`·옛 콘솔 `/index.legacy.html` 200을 확인한다. 캡처는 TEMPLATE 규칙대로 `docs/ga-ready/demo/web-console-shell-20261011/`에 PNG(500 KB 이하, 사용자 홈 경로·LAN IP·호스트명·token 없음)로 둔다. host mutation 없음(읽기만). 기록 `docs/ga-ready/demo/web-console-shell-demo-2026-10-11.md`(TEMPLATE.md 형식, 확인자 칸 비움). 로컬 commit.

## Task 7: probe `checkpoint.schedule.set` + S3 브라우저 시연 (queue 79)

- [ ] Local API로 `pcv-it-s3-source`(managed Generation 2, cpu 1, memory 512 MB, disk 8 GB, smoke ISO, VM root `artifacts/s3-checkpoint-20261011/vms`, Off)만 만든 뒤 checkpoint와 schedule은 브라우저에서 한다. 새 셸의 VM 상세에서 checkpoint 생성·복원, 예약 미리보기와 예약 저장(폼 버튼 클릭, BL-0016 수정 확인)을 한 번씩 하고 readback(enabled, interval, retention, next due, last enqueued)을 읽는다. schedule은 켠 채 두고 VM을 남긴다(예약 실행은 Task 9). 기록 초안 `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md`(TEMPLATE.md 형식, 캡처 `docs/ga-ready/demo/s3-checkpoint-20261011/`). 로컬 commit.

## Task 8: Lane 3

- [ ] functional carry-forward, consume(`…-closed` descriptor와 consume manifest), payload commit의 main push를 `train-path-check`로 확인한 `main-push-payload` 문서, current-card `promoted-current`. `current-evidence.json`, `lane3-spec`, 문서 도구 dry-run·`-Apply`·`-Check`, `release-train.json` 승격(`operational_current=0.42.96-admin-smoke`, train `status=promoted`). `train-path-check`가 제품 경로를 보고하면 Lane 3를 쓰지 않고 멈춘다. 로컬 commit.

## Task 9: S3 예약 실행 확인과 정리 (queue 79)

- [ ] Task 7 schedule의 `next_due_at`이 지난 뒤 시나리오 스크립트 `--execute --verify-only --expect-count=<n>`과 브라우저 readback(checkpoint 목록에 `pcv-schedule-*`, last enqueued 갱신)으로 예약 실행을 확인하고, 브라우저에서 schedule clear를 한 뒤 스크립트 `--execute --cleanup-only`로 VM을 지우고 `pcv-it-s3-*` VM 0개, VM 폴더 없음을 확인한다. Task 7 기록을 완성한다(판정, 캡처). 로컬 commit.

## Task 10: 종료와 train PR merge

- [ ] clean HEAD 종료 검증(`dotnet build -c Release`, PR gate 또는 Required CI shard 4개 로컬, Pester 4종, `npm run test:public-source-safety --prefix web` 신규 0), `train-path-check --payload <payload> --head HEAD` exit `0`, push, PR 하나, green CI 뒤 merge(`--match-head-commit`). merge 직전 base는 `origin/main`과 같다. merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 11: 판정과 후속 campaign

- [ ] clean `main`에서 `pcvverify completion`을 돌려 결과를 기록한다. exit `0`이면 감사 문서로 완료를 적고 push, PR, green CI 뒤 merge한다. 그 밖이면 이 campaign을 `status=closed`로 닫고(Task 12는 `carried_tasks`로 새 campaign에 옮김) 승인 3으로 S4 campaign을 `pcv-campaign-open`으로 연다(그 campaign 첫 PR에 criteria S3 닫기와 Task 12 이관 포함). legacy-retirement campaign은 S4 campaign이 닫힌 뒤 연다.

## Task 12: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `shell-nav-view-sync-20261011` Task 4(원래 `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 Task 8의 Lane 3 commit이 main에 merge된 뒤에만 `0.42.96-admin-smoke`로 바뀐다. 그 전 모든 task 기록은 `0.42.93-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다. package는 `AllowUnsignedDev`/`LocalTest`다.
- criteria S3·S4 닫기, backlog 분류, legacy 콘솔 제거는 이 campaign의 산출물이 아니다.
