# release train `0.43.4` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** BL-0044 수정 campaign(`bl0044-alerts-loading-20261111`)이 queue에 쌓은 변경 한 행(PR #146)을 release train `0.43.4-admin-smoke`로 싣는다. package부터 Lane 3까지 단일 PR train으로 돌려 operational current로 승격한다. 끝에 clean `main` 판정에서 hygiene C2(queue 0, train promoted)까지 확인한다.

- 적재: `release-train.json` queue 1행(PR #146)이다. `web` 영역이고 `lane2_probe`는 `null`이다.
- 계정: 지금 설치본 `0.43.3`의 계정을 train 동안 보관하고, 끝에 되살린다(train `0.43.3`과 같은 이유: fullgate `REMOVE_DATA`가 data root를 지우고, train 도구는 계정이 없을 때만 열리는 loopback 세션을 쓴다).

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10의 단일 PR train이다(`pcv-single-pr-train-v2`, 설계 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`). task 구조는 직전 승격 train plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04303.md`를 그대로 따른다.

- train branch: `lane1/train-04304-20261111`. 출발, package, 계정 보관, pair, fullgate, current-card, probe, 계정 복원, Lane 3를 모두 담고 PR 하나로 merge한다.
- payload commit: 이 opening의 `origin/main` `e957f15b`(PR #146 merge)다. 그 뒤 train branch commit은 문서와 Lane 3 pin 여섯 파일(SHA-256 한 줄씩, `allowed_pin_paths`)뿐이어야 한다.
- Lane 3 main push evidence는 payload commit의 main push run을 인용한다(`main-push-payload` 템플릿, `train_pr_head`, `path_check_line`).
- version: `0.43.4-admin-smoke`(꼬리표 `04304`).
- 설치본: `0.43.3-admin-smoke`(train `0.43.3` fullgate build `db1a762`)이고 loopback 전용이다. Task 2에서 실측한다.
- 다시 쓰는 입력은 셋이다.
  - pair baseline: train `0.43.3` Task 1의 `artifacts/admin-smoke-package-20261111-04303`
  - smoke ISO: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
  - clean-host base: `current-base.json`이 가리키는 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, Playwright(설치본 Web Console 화면 probe), `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"release train 0.43.4-admin-smoke 출발: queue #146 적재. Lane 2 unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe(Web Console 화면). Lane 3 current-evidence.json 승격. 단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR."` (`bl0044-alerts-loading-20261111` 다음 승인 1번 문장을 사용자가 그대로 보냄)

| 항목 | 범위 |
| --- | --- |
| 1 | train `0.43.4-admin-smoke` 출발(`train-departure`). queue 1행(PR #146)을 `carriages`로 고정한다. Lane 0/1/2/3. push, PR |
| 2 | version은 `0.43.4-admin-smoke`. MSI ProductVersion `0.43.4`, artifact 꼬리표 `04304`, pair `0.43.3 -> 0.43.4` |
| 3 | Lane 2 host mutation. 원문은 `unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe(Web Console 화면)`다. 해석: MSI install·repair·uninstall, REMOVE_DATA, service, pair 여섯 bucket(clean-host VM, 제품 Update/Rollback 포함), fullgate route parity VM, fullgate os-mutation-gate, 설치본 읽기, 설치본 Web Console을 브라우저로 여는 화면 probe다 |
| 4 | `Lane 3 current-evidence.json 승격`이다. `0.43.4-admin-smoke`로 쓴다. Task 8~9 |
| 5 | `단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR`. Task 10. 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`과 같다(`merge_policy=after-green-ci`) |
| 6 | 계정 보관과 복원은 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 연다(train `0.43.3` 사용자 결정 6과 같다). 범위는 train 전 `accounts.json` 백업(저장소와 제품 data 폴더 밖 보호 위치)과 data root에서 치우기, train 뒤 복원과 해시 비교다. 백업 삭제는 사용자에게 묻는다 |
| probe | queue 행의 `lane2_probe`는 `null`이다. 승인 3의 `probe(Web Console 화면)` 범위 안에서 설치본이 BL-0044 수정(대시보드 "경고" 카드의 첫 갱신 중 읽는 중 문구)을 내주는지 확인한다(Task 6). VM은 만들지 않는다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다. disk는 `artifacts/` 아래에 둔다 |

## 적재 변경

| PR | 변경 commit(merge) | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #146 | `08589a5f`(`e957f15b`) | web | backlog BL-0044: 첫 갱신이 진행 중이고 아직 끝난 적이 없으면 대시보드 "경고" 카드가 "운영 요약을 읽지 못했습니다" 대신 새 문구 "운영 요약을 읽고 있습니다."(`data-alerts-state="loading"`)를 보인다 | Task 6 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다.
  - `pcv-guest-installed-04253-r1`의 부모 VHD를 고치거나 옮기지 않는다(BL-0028).
  - 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다.
  - 새 VM은 clean-host runner와 fullgate route smoke VM뿐이고 끝나면 지운다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다.
  - guest 인증 정보는 실행 경계에서 무작위로 만든다.
  - 계정 백업은 저장소와 제품 data 폴더 밖 보호 위치(상속 끊음, 사용자·SYSTEM·Administrators만)에 두고, 그 경로는 문서에 적지 않는다.
  - 화면 probe 캡처는 `artifacts/` 아래(gitignore)에만 두고 커밋하지 않는다. evidence 문서에는 호스트명을 적지 않는다.
  - commit은 경로를 이름으로 stage한다.
- evidence와 artifact는 새 이름(`-04304`, `-04303-04304`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). 예외는 Lane 3 문서 도구가 바꾸는 pin 여섯 파일의 SHA-256 줄이다(`allowed_pin_paths`). 제품 결함이 나오면 정차하고 수정 PR로 간다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다.
  - 정차하면 train 항목을 `release-train.json`에서 지우고 queue를 되돌린다.
  - 정차 사실은 plan·campaign·backlog에 기록한다.
  - FAIL은 current에 쓰지 않는다.
  - 정차하면 계정 복원(Task 7)을 먼저 하고 멈춘다.
- 같은 version 설치 전에는 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다.
- Lane 3 spec 입력(`trains/<v>.lane3-spec-input.json`)의 `narrative.functional_note`에는 backtick을 쓰지 않는다(`PCV_PROMOTION_INDEX_INVALID`, train `0.42.89`·`0.43.2`에서 두 번 났다).
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).
  - 장시간 도구 실행(build, pair, fullgate)의 대기는 그 checkpoint 시간에 넣는다.
  - 기다림은 background 명령의 완료 알림으로 한다.

## Task 0: 출발

- [x] 먼저 payload commit `e957f15b`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다.
  - `release-train.json`의 `queue` 1행을 train `0.43.4-admin-smoke`의 `carriages` `[146]`로 옮긴다.
  - `status=running`, `departed_at`, `source_commit`을 적고, `queue`는 빈 목록으로 둔다.
  - `docs/ga-ready/trains/0.43.4-admin-smoke.host-inputs.json`을 쓴다. ISO는 위 smoke ISO다.
  - 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-11): 출발 시각은 `2026-11-11T03:17:00+09:00`이다.
- payload: `e957f15b08e233ce9faffa809a178056c6b0e952`(PR #146 merge, origin/main)
- payload main push run: Development Gates `42866127498`과 Public Boundary Contract `42866127722`가 success다.
- carriages `[146]`(1행)이다. queue를 비우고 train `status=running`으로 두었다.
- `docs/ga-ready/trains/0.43.4-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다.
- 검증: `PcvReleaseTrainContractTests` `6/6`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.43.3-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 다음 두 명령을 돌린다.
  - `build.ps1 -Version 0.43.4-admin-smoke -MsiProductVersion 0.43.4 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04304`
  - `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`
  - payload 수는 `update-preflight-compatibility.json` fixture와 맞는지 본다(`web/app.js`·`web/styles.css` 호환 파일 포함).
  - `train-facts`로 package 문서를 쓴다(`docs/ga-ready/trains/0.43.4-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행).
  - 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-11): clean HEAD `54a817c`에서 빌드했다. product source는 payload `e957f15b`와 같고, 그 뒤 commit은 문서뿐이다.
- 빌드: `build.ps1` `47`초, `build_utc` `2026-11-10T18:18:32Z`, provenance `54a817c4ed0a8ab14056405310a998571c99d578`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `c24ef8317c838d659834b7e77446625d9377e5774f9057fedc813318a739fccb`.
- payload: `31`개, aggregate `29ba7ece25684f58b11c2e1ba4122569047494a814ef1a4ab49dee7c8959fb3d`. `0.43.3`과 같은 수이고 호환 파일 `web/app.js`·`web/styles.css`가 들어 있다.
- update ZIP: SHA-256 `92630abe393ea54fa3acb0c8a775e03d18fc8c27d0aeb1c4d8b1bc81f3cdb2cc`(entry 31). catalog SHA-256 `13b5d0bafcf30fb0d0b41cb1b84d8539182cc2de60f7233f25a247e59fbe51a6`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-11-04304.md`(`generated=20`, `narrative=3`). `train-evidence --write` 뒤 `--check` 결과는 `current`다.
- 시험: `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `30/30`.

host mutation은 없다. operational current는 `0.43.3-admin-smoke` 그대로다.

## Task 2: 계정 보관

- [x] Lane 2. 설치본 version, 계정 이름과 상태, service, 방화벽 규칙, 보존 VM을 먼저 실측한다. data root의 `accounts.json`을 보호 위치에 백업하고 SHA-256을 비교한다. 그다음 data root에서 치우고 service를 다시 시작한다.
  - 확인: `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Automatic, 보존 VM 둘 그대로.
  - 계정 이름과 상태만 적고 해시와 경로는 적지 않는다. 로컬 commit.

실행 기록(2026-11-11, 03:20 KST):
- 실측(전): 설치본 Host `0.43.3-admin-smoke+db1a762`, ARP `{07427BC2…}` `0.43.3` 1개, service Running/Automatic, PureCVisor 방화벽 규칙 0, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`, `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off.
- 계정 3개: `pcv-admin` admin disabled, `pcv-s4-operator` operator disabled, `pcv-admin2` admin enabled(train `0.43.3` 복원 뒤와 같다).
- 백업: service 명령줄의 `--account-file`(data root `accounts.json`, 1,153 bytes)을 사용자 프로필 아래 보호 위치로 복사했다. 그 위치는 상속을 끊었고 사용자·SYSTEM·Administrators만 접근한다. 원본 파일의 ACL 표기도 같은 위치에 두었다. 복사본 SHA-256은 원본과 같다.
- 원본을 data root에서 지우고 service를 다시 시작했다(승인 3의 service 중지·시작).
- 확인: `POST /api/v1/auth/loopback-session` `200`, Web `http://127.0.0.1/` `200`, service Running/Automatic, data root에 계정 파일 없음, PureCVisor 방화벽 규칙 0, 보존 VM 둘 Off(그대로).

operational current는 `0.43.3-admin-smoke` 그대로다.

## Task 3: pair

- [x] Lane 2. orchestrator를 `-PlanOnly`로 돌린 뒤 `-Execute`로 돌린다.
  - 입력: campaign `manual-admin-campaign-20261111-04303-04304`, baseline `artifacts/admin-smoke-package-20261111-04303`, target Task 1 package, base VHD `20348.5761-20261108.vhd`.
  - 통과 조건: 여섯 bucket PASS, closed descriptor, `observation_error` 없음.
  - 끝 상태: 설치본 `0.43.4`·ARP 1개·service Running/Automatic·Web 200·보존 VM 둘 그대로.
  - `train-facts`로 pair 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11): orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). 실행 전 ARP는 `0.43.3` 1개였다.
- guest 인증 정보는 실행 스크립트 안에서 무작위로 만들어 `pscredential`로만 넘겼다. 출력하거나 저장하지 않았다.
- `-Execute`는 root `artifacts/manual-admin-campaign-20261111-04303-04304`에서 03:20:39부터 03:41:01 KST까지 `1222`초 돌았다(직전 train `252`초).
  - 늘어난 시간은 clean-host bucket(03:21~03:39)이다. Windows Update 준비 재부팅 뒤 guest heartbeat가 `NoContact`였고, runner의 무응답 복구 정책(기준 `900`초)을 거쳐 두 번째 PowerShell Direct 시도에서 이어졌다.
  - bucket은 PASS다. 시험 guest 부팅 지연을 runner가 설계대로 복구한 것이라 제품 결함으로 보지 않는다.
- 여섯 bucket이 모두 PASS다: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`, `clean-host-windows-update`, `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.
- descriptor: closed descriptor를 냈다(SHA-256 `574b22c1200ad59e19dda450bf1dd6fefe4dd52dbe4e16f3c17999a374addb1d`). observation은 15개이고 `observation_error`는 0이다. restoration은 시도하지 않았다(필요 없음).
- clean-host: base UBR `5761`이고 blocker는 `none`이다. baseline `0.43.3` MSI 설치 뒤 제품 Update로 `0.43.4`가 됐고 Rollback으로 돌아왔다.
- 끝 상태:
  - 설치본 `0.43.4-admin-smoke+54a817c…`, ARP `{BF2AD1AA…}` `0.43.4` 1개, service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0이다.
  - `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로)다.
  - data root에는 업그레이드된 service가 만든 빈 계정 저장소(0개)가 있고 loopback 세션은 200이다(직전 train과 같다). Task 7에서 fullgate가 남긴 빈 저장소와 같이 다룬다.
- 문서: `train-facts`로 pair 문서 여섯 개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 썼다. `train-evidence --check`는 일곱 문서 모두 `current`다. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`는 `30/30`이다. 새 문서의 주소는 loopback뿐이다.

operational current는 `0.43.3-admin-smoke` 그대로다.

## Task 4: fullgate

- [x] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만든다. LAN prefix가 필요하면 지금 호스트 주소에서 실행 경계로 만들고 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘긴다.
  - supervisor를 `-DryRun -AllowHostMutation`으로 돌린 뒤 `full-admin-host-mutation-gate-20261111-04304`을 돌린다.
  - 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM 둘 그대로.
  - `train-facts`로 fullgate 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11):
- manifest: `train-host-inputs --kind fullgate-manifest --write`로 만들었다(SHA-256 `794d8cc256c23ed3c569b2327cbfd5669210429483135ec11a927745cea0bd00`).
  - LAN prefix는 기본 게이트웨이가 있는 호스트 사설 IPv4와 직전 manifest의 port로 실행 경계에서 만들었다.
  - 값은 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘겼고 출력하지 않았다.
- supervisor `-DryRun -AllowHostMutation`은 두 단계 계획에 `ok=true`였다. 실행 전 ARP는 1개였다.
- 실행: 03:42:47부터 03:49:20 KST까지 돌았다. `ok=true`, `status=completed`다. stdout은 `artifacts/batch-manifests/full-admin-host-mutation-gate-20261111-04304.result.json`이고 stderr는 0 byte다.
- step:
  - `service-msi-hyperv-admin-smoke` exit 0(시도 1, 381.7초)
  - `os-mutation-gate` exit 0(시도 1, 11.1초)
  - executed 2, skipped 0
- 사후 검사:
  - 설치본 Host/CLI `0.43.4-admin-smoke+7b929ec`는 gate build HEAD `7b929ec`(Task 3 commit)와 같다. product source는 payload `e957f15b`와 같다.
  - ARP `{8C298285…}` `0.43.4` 1개, PureCVisor 방화벽 규칙 `0`, listener `127.0.0.1:7777`·`127.0.0.1:80`.
  - service Running/Automatic, Web `200`, loopback 세션 `200`.
  - `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(그대로).
- 문서: `train-facts`로 fullgate 문서 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-11-11-04304-hostmutation.md`(`generated=35`, `narrative=2`)를 썼다. `train-evidence --check`는 여덟 문서 모두 `current`이고, 시험은 `30/30`이다. 문서에는 LAN 주소가 없다(`[redacted-private-endpoint]`).

operational current는 `0.43.3-admin-smoke` 그대로다.

## Task 5: installed current-card

- [ ] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다.
  - 통과 조건: `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음.
  - `pcv-it-s2-source`는 S2 template으로 기록한다. 도구가 받지 않으면 멈추고 묻는다.
  - `train-facts`로 current-card 문서를 쓴다. 로컬 commit.

## Task 6: probe Web Console 화면 (queue 146)

- [ ] Lane 2. 설치본 `0.43.4`가 loopback(`http://127.0.0.1/`)으로 내주는 Web Console을 확인한다. VM은 만들지 않는다.
  - 내주는 파일: `app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js`의 SHA-256이 Task 1 package payload의 같은 파일과 같은지, `sw.js` CACHE_NAME이 payload commit 소스와 같은지 본다.
  - 화면: Playwright 브라우저로 1440×900과 768×1024에서 연다. 새 브라우저 context(저장소 비어 있음)에서 `#/dashboard`를 열고 15초 동안 0.5초 간격으로 "경고" 카드 상태를 본다. 같은 확인을 화면마다 새 context로 세 번 한다.
    - BL-0044: 첫 갱신이 끝나기 전에는 카드가 `data-alerts-state="loading"`과 "운영 요약을 읽고 있습니다."를 보이고, "운영 요약을 읽지 못했습니다"는 보이지 않는다. 바뀐 시각을 적는다.
    - 회귀(BL-0043): 첫 갱신이 끝나면 카드가 목록 또는 `empty`가 되고 15초 뒤 "운영 요약을 읽지 못했습니다"가 없다.
    - 회귀: 셸 version 표시가 `v0.43.4-admin-smoke`다(BL-0042). 첫 방문에 `auth/loopback-session`이 `runtime/policy`보다 먼저이고(BL-0040), HTTP 4xx와 console error가 0이다.
    - 나머지 화면(가상 머신, template, 네트워크, 작업, 활동, evidence, 계정, 문제 해결)이 그려지고 console error 0이다.
  - 캡처는 `artifacts/lane2-bl0044-probe-20261111-04304/`에 둔다(커밋하지 않음).
  - evidence: `docs/ga-ready/evidence/lane2-bl0044-probe-2026-11-11-04304.md`. 로컬 commit.

## Task 7: 계정 복원

- [ ] Lane 2. Task 2 백업의 `accounts.json`을 data root에 되돌리고 SHA-256을 비교한다. fullgate 재설치가 만든 빈 저장소는 지우지 않고 백업 위치로 옮긴다. 파일 ACL은 train 전 원본 표기와 같게 맞추고 service를 다시 시작한다.
  - 확인: 계정 이름과 상태가 Task 2와 같다, loopback 세션 `409`, Web 200, service Running/Automatic.
  - 이름과 상태만 기록한다. 로컬 commit.

## Task 8: Lane 3 데이터와 lane3-spec

- [ ] Lane 3. 같은 train branch에서 한다.
  - consume manifest, functional carry-forward·pair consume·`main-push-payload`(payload `e957f15b`의 main push run 인용, `train_pr_head`, `path_check_line`) facts를 쓴다. `main-push` 템플릿과 함께 쓰지 않는다.
  - 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돈다.
  - `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.43.3-admin-smoke`, `previous_spec`은 `lane3-promotion-docs-spec-04303.json`이다.
  - `update-preflight-compatibility.json` fixture의 `operational-current` 행을 새 version으로 고친다(DEVELOPMENT_PROCEDURE §10, BL-0030). 로컬 commit.

## Task 9: 승격 문서와 release-train 승격

- [ ] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1`을 dry-run, `-Apply`, `-Check` 순서로 돌린다. Lane 3 숨은 단계도 한다: descriptor chain, ledger 행, index 절, D2 시험, spec pin.
  - `release-train.json`: train `0.43.4-admin-smoke` `status=promoted`, `operational_current=0.43.4-admin-smoke`.
  - 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Update-PcvContractSpecPins.ps1 -Apply`/`-Check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`를 돈다.
  - `train-path-check --payload e957f15b --head HEAD`가 `allowed_pin_paths`만 남기는지 본다.
  - 수기 갱신: `DOCUMENTATION_INDEX` 권위 줄(공개 소스 HEAD는 payload commit), `FEATURE_IMPLEMENTATION_LEDGER` operational 줄, `DEVELOPER_INDEX` train 줄. 로컬 commit.

## Task 10: train PR

- [ ] clean HEAD 종료 검증을 한다.
  - `dotnet build src/DesktopNode.sln -c Release`
  - `Invoke-PcvPrGate.ps1`
  - Pester 4종
  - `npm run test:public-source-safety --prefix web` 신규 0
  - `train-path-check --payload e957f15b --head HEAD` exit `0`
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check origin/main...HEAD`
  - 그다음 push와 PR을 한다. merge 직전 base가 `origin/main`과 같은지 본다. Required CI 다섯 개 green 뒤 merge한다(`--match-head-commit`). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 11: 판정과 마무리

- [ ] clean `main`에서 `pcvverify completion`을 돌린다. `complete=true`이고 hygiene C2가 queue 0·train promoted로 충족되는지 본다.
  - exit `0`이면 판정을 인용한 감사 문서 `docs/project-status-audit-2026-11-11-r2.md`를 쓴다(같은 날 두 번째 감사다).
  - 계정 백업 삭제를 사용자에게 물을 문장을 남긴다.
  - campaign을 닫는다. push와 PR 뒤 Required CI green이면 merge한다.

## Nonclaims

- operational current는 Task 10 merge 뒤에만 `0.43.4-admin-smoke`가 된다. 그 전까지는 `0.43.3-admin-smoke`다.
- 화면 probe는 설치본이 BL-0044 수정을 내주는지 보는 것이다. 디자인 감사나 접근성 판정을 새로 하지 않는다(그것은 Web gate가 한다).
- public trusted signing과 external stable publication을 주장하지 않는다.
