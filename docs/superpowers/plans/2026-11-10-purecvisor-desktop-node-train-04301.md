# release train `0.43.1` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** backlog counts 수정 campaign(`backlog-counts-20261110`)이 queue에 쌓은 Web Console 변경 한 행(PR #134, BL-0036~BL-0041)을 release train `0.43.1-admin-smoke`로 싣는다. package부터 Lane 3까지 단일 PR train으로 돌려 operational current로 승격한다. 끝에 clean `main` 판정에서 hygiene C2(queue 0, train promoted)까지 확인한다.

- 적재: `release-train.json` queue 1행(PR #134)이다. `web` 영역이고 `lane2_probe`는 `null`이다.
- 계정: 지금 설치본 `0.43.0`의 계정을 train 동안 보관하고, 끝에 되살린다(train `0.43.0`과 같은 이유: fullgate `REMOVE_DATA`가 data root를 지우고, train 도구는 계정이 없을 때만 열리는 loopback 세션을 쓴다).

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10의 단일 PR train이다(`pcv-single-pr-train-v2`, 설계 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`). task 구조는 직전 승격 train plan `docs/superpowers/plans/2026-11-10-purecvisor-desktop-node-train-04300.md`를 그대로 따른다.

- train branch: `lane1/train-04301-20261110`. 출발, package, 계정 보관, pair, fullgate, current-card, probe, 계정 복원, Lane 3를 모두 담고 PR 하나로 merge한다.
- payload commit: 이 opening의 `origin/main` `c685bd51`(PR #134 merge)다. 그 뒤 train branch commit은 문서와 Lane 3 pin 여섯 파일(SHA-256 한 줄씩, `allowed_pin_paths`)뿐이어야 한다.
- Lane 3 main push evidence는 payload commit의 main push run을 인용한다(`main-push-payload` 템플릿, `train_pr_head`, `path_check_line`).
- version: `0.43.1-admin-smoke`(꼬리표 `04301`).
- 설치본: `0.43.0-admin-smoke`(train `0.43.0` fullgate build `1f646ee`)이고 loopback 전용이다. Task 2에서 실측한다.
- 다시 쓰는 입력은 셋이다.
  - pair baseline: train `0.43.0` Task 1의 `artifacts/admin-smoke-package-20261110-04300`
  - smoke ISO: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
  - clean-host base: `current-base.json`이 가리키는 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, Playwright(설치본 Web Console 화면 probe), `gh`

## 사용자 결정 (2026-11-10)

승인 원문: `"release train 0.43.1-admin-smoke 출발: queue #134 적재. Lane 2 unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe(Web Console 화면). Lane 3 current-evidence.json 승격. 단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR."` (`backlog-counts-20261110` 다음 승인 1번 문장을 사용자가 그대로 보냄)

| 항목 | 범위 |
| --- | --- |
| 1 | train `0.43.1-admin-smoke` 출발(`train-departure`). queue 1행(PR #134)을 `carriages`로 고정한다. Lane 0/1/2/3. push, PR |
| 2 | version은 `0.43.1-admin-smoke`. MSI ProductVersion `0.43.1`, artifact 꼬리표 `04301`, pair `0.43.0 -> 0.43.1` |
| 3 | Lane 2 host mutation. 원문은 `unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe(Web Console 화면)`다. 해석: MSI install·repair·uninstall, REMOVE_DATA, service, pair 여섯 bucket(clean-host VM, 제품 Update/Rollback 포함), fullgate route parity VM, fullgate os-mutation-gate, 설치본 읽기, 설치본 Web Console을 브라우저로 여는 화면 probe다 |
| 4 | `Lane 3 current-evidence.json 승격`이다. `0.43.1-admin-smoke`로 쓴다. Task 8~9 |
| 5 | `단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR`. Task 10. 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`과 같다(`merge_policy=after-green-ci`) |
| 6 | 계정 보관과 복원은 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 연다(train `0.43.0` 사용자 결정 6과 같다). 범위는 train 전 `accounts.json` 백업(저장소와 제품 data 폴더 밖 보호 위치)과 data root에서 치우기, train 뒤 복원과 해시 비교다. 백업 삭제는 사용자에게 묻는다 |
| 7 | 마무리 뒤 사용자 결정 "1, 2 수정"(2026-11-10, 마무리 보고의 다음 승인 1·2). 1: train 계정 백업을 지우고 기록은 문서 PR 하나로 남겨 Required CI green 뒤 merge한다. 2: BL-0042를 `counts`로 분류하고 고친다(`수정`). 고치는 일은 따로 여는 Lane 1 campaign에서 한다 |
| probe | queue 행의 `lane2_probe`는 `null`이다. 승인 3의 `probe(Web Console 화면)` 범위 안에서 설치본이 내주는 Web Console에 BL-0036~BL-0041 수정이 들어 있는지 확인한다(Task 6). VM은 만들지 않는다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다. disk는 `artifacts/` 아래에 둔다 |

## 적재 변경

| PR | 변경 commit(merge) | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #134 | `1e06c13c`(`c685bd51`) | web | backlog counts BL-0036~BL-0041: 좁은 폭 셸 검색 버튼 이름, 진단 서비스 값 실행 중/멈춤 문구, 삭제 확인 gate가 DELETE 요청을 기다림, version 표시가 loopback 세션을 기다림(첫 방문 `runtime/policy` 401 없음), 좁은 VM 목록의 전체 이름·상태 점·템플릿 배지, 고대비 테마 token 적용과 mockup 테마 퇴역 | Task 6 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다.
  - `pcv-guest-installed-04253-r1`의 부모 VHD를 고치거나 옮기지 않는다(BL-0028).
  - 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다.
  - 새 VM은 clean-host runner와 fullgate route smoke VM뿐이고 끝나면 지운다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다.
  - guest 인증 정보는 실행 경계에서 무작위로 만든다.
  - 계정 백업은 저장소와 제품 data 폴더 밖 보호 위치(상속 끊음, 사용자·SYSTEM·Administrators만)에 두고, 그 경로는 문서에 적지 않는다.
  - 화면 probe 캡처는 `artifacts/` 아래(gitignore)에만 두고 커밋하지 않는다. evidence 문서에는 호스트명을 적지 않는다.
  - Playwright MCP가 저장소 root에 남기는 `.playwright-mcp/`는 이제 gitignore다. 그래도 commit은 경로를 이름으로 stage한다.
- evidence와 artifact는 새 이름(`-04301`, `-04300-04301`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). 예외는 Lane 3 문서 도구가 바꾸는 pin 여섯 파일의 SHA-256 줄이다(`allowed_pin_paths`). 제품 결함이 나오면 정차하고 수정 PR로 간다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다.
  - 정차하면 train 항목을 `release-train.json`에서 지우고 queue를 되돌린다.
  - 정차 사실은 plan·campaign·backlog에 기록한다.
  - FAIL은 current에 쓰지 않는다.
  - 정차하면 계정 복원(Task 7)을 먼저 하고 멈춘다.
- 같은 version 설치 전에는 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).
  - 장시간 도구 실행(build, pair, fullgate)의 대기는 그 checkpoint 시간에 넣는다.
  - 기다림은 background 명령의 완료 알림으로 한다.

## Task 0: 출발

- [x] 먼저 payload commit `c685bd51`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다.
  - `release-train.json`의 `queue` 1행을 train `0.43.1-admin-smoke`의 `carriages` `[134]`로 옮긴다.
  - `status=running`, `departed_at`, `source_commit`을 적고, `queue`는 빈 목록으로 둔다.
  - `docs/ga-ready/trains/0.43.1-admin-smoke.host-inputs.json`을 쓴다. ISO는 위 smoke ISO다.
  - 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-10): 출발 시각은 `2026-11-10T16:41:00+09:00`이다.
- payload: `c685bd514a5c9e6f42e3e647ec6177260e541b6d`(PR #134 merge, origin/main)
- payload main push run: Development Gates `42726129133`과 Public Boundary Contract `42726128946`이 success다.
- carriages `[134]`(1행)이다. queue를 비우고 train `status=running`으로 두었다.
- `docs/ga-ready/trains/0.43.1-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다.
- 검증: `PcvReleaseTrainContractTests` `6/6`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 다음 두 명령을 돌린다.
  - `build.ps1 -Version 0.43.1-admin-smoke -MsiProductVersion 0.43.1 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261110-04301`
  - `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`
  - payload 수는 `update-preflight-compatibility.json` fixture와 맞는지 본다(`web/app.js`·`web/styles.css` 호환 파일 포함).
  - `train-facts`로 package 문서를 쓴다(`docs/ga-ready/trains/0.43.1-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행).
  - 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-10): clean HEAD `0be6561`에서 빌드했다. product source는 payload `c685bd51`과 같고, 그 뒤 commit은 문서뿐이다.
- 빌드: `build.ps1` `51`초, `build_utc` `2026-11-10T07:42:52Z`, provenance `0be6561e0a22b5163a7214e9de9b5ebb889778cc`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `37c3163fa024703c3e285d6c8f2343422437b9db933e78a08df4acce88b120f1`.
- payload: `31`개, aggregate `97b42b7495666c0674486980e5c097c53f2cd49d238737c83f87f1d2ec4d79e4`. `0.43.0`과 같은 수이고 호환 파일 `web/app.js`·`web/styles.css`가 들어 있다.
- update ZIP: SHA-256 `a504d2304544b1bf9ba468d5256d5e51951be9ae5fd7e56a45ed437f2e7c077e`(entry 31). catalog SHA-256 `3d81a9ab29d6b55cf101375fc4b5a5e86b0986be33883ea2e91967762815ec59`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-10-04301.md`(`generated=20`, `narrative=3`). `train-evidence --write` 뒤 `--check` 결과는 `current`다.
- 시험: `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `27/27`.

host mutation은 없다. operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 2: 계정 보관

- [x] Lane 2. 설치본 version, 계정 이름과 상태, service, 방화벽 규칙, 보존 VM을 먼저 실측한다. data root의 `accounts.json`을 보호 위치에 백업하고 SHA-256을 비교한다. 그다음 data root에서 치우고 service를 다시 시작한다.
  - 확인: `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Automatic, 보존 VM 둘 그대로.
  - 계정 이름과 상태만 적고 해시와 경로는 적지 않는다. 로컬 commit.

실행 기록(2026-11-10, 16:44~16:45 KST):
- 실측(전): 설치본 Host `0.43.0-admin-smoke+1f646ee`, ARP `{C948BEA2…}` `0.43.0` 1개, service Running/Automatic, PureCVisor 방화벽 규칙 0, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`, `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off.
- 계정 3개: `pcv-admin` admin disabled, `pcv-s4-operator` operator disabled, `pcv-admin2` admin enabled(train `0.43.0` 복원 뒤와 같다).
- 백업: service 명령줄의 `--account-file`(data root `accounts.json`, 1,153 bytes)을 사용자 프로필 아래 보호 위치로 복사했다. 그 위치는 상속을 끊었고 사용자·SYSTEM·Administrators만 접근한다. 원본 파일의 ACL 표기도 같은 위치에 두었다. 복사본 SHA-256은 원본과 같다.
- 원본을 data root에서 지우고 service를 다시 시작했다(승인 3의 service 중지·시작).
- 확인: `POST /api/v1/auth/loopback-session` `200`, Web `http://127.0.0.1/` `200`, service Running/Automatic, data root에 계정 파일 없음, PureCVisor 방화벽 규칙 0, 보존 VM 둘 Off(그대로).

operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 3: pair

- [x] Lane 2. orchestrator를 `-PlanOnly`로 돌린 뒤 `-Execute`로 돌린다.
  - 입력: campaign `manual-admin-campaign-20261110-04300-04301`, baseline `artifacts/admin-smoke-package-20261110-04300`, target Task 1 package, base VHD `20348.5761-20261108.vhd`.
  - 통과 조건: 여섯 bucket PASS, closed descriptor, `observation_error` 없음.
  - 끝 상태: 설치본 `0.43.1`·ARP 1개·service Running/Automatic·Web 200·보존 VM 둘 그대로.
  - `train-facts`로 pair 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-10): orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). 실행 전 ARP는 `0.43.0` 1개였다.
- guest 인증 정보는 실행 스크립트 안에서 무작위로 만들어 `pscredential`로만 넘겼다. 출력하거나 저장하지 않았다.
- `-Execute`는 root `artifacts/manual-admin-campaign-20261110-04300-04301`에서 16:45:40부터 16:50:20 KST까지 `280`초 돌았다.
- 여섯 bucket이 모두 PASS다: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`, `clean-host-windows-update`, `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.
- descriptor: closed descriptor를 냈다(SHA-256 `a3d3aabefb6965e226b5422c07473b9df02dca36f6dcdb4cc9bf24a11ae28e3b`). observation은 15개이고 `observation_error`는 0이다. restoration은 시도하지 않았다(필요 없음).
- clean-host: base UBR `5761`이고 blocker는 `none`이다. baseline `0.43.0` MSI 설치 뒤 제품 Update로 `0.43.1`이 됐고 Rollback으로 돌아왔다.
- 끝 상태:
  - 설치본 `0.43.1-admin-smoke+0be6561…`, ARP `{31E528BD…}` `0.43.1` 1개, service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0이다.
  - `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로)다.
  - data root에는 계정 파일이 없고 loopback 세션은 200이다.
- 문서: `train-facts`로 pair 문서 여섯 개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 썼다. `train-evidence --check`는 일곱 문서 모두 `current`다. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`는 `27/27`이다. 새 문서의 주소는 loopback뿐이다.

operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 4: fullgate

- [x] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만든다. LAN prefix가 필요하면 지금 호스트 주소에서 실행 경계로 만들고 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘긴다.
  - supervisor를 `-DryRun -AllowHostMutation`으로 돌린 뒤 `full-admin-host-mutation-gate-20261110-04301`을 돌린다.
  - 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM 둘 그대로.
  - `train-facts`로 fullgate 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-10):
- manifest: `train-host-inputs --kind fullgate-manifest --write`로 만들었다(SHA-256 `5089711ffd2328ee535e56d4b8b2099234409a14d2d75949ab8ea19ab81f849b`).
  - LAN prefix는 기본 게이트웨이가 있는 호스트 사설 IPv4와 직전 manifest의 port로 실행 경계에서 만들었다.
  - 값은 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘겼고 출력하지 않았다.
- supervisor `-DryRun -AllowHostMutation`은 두 단계 계획에 `ok=true`였다. 실행 전 ARP는 1개였다.
- 실행: 16:51:53부터 17:01:20 KST까지 돌았다. `ok=true`, `status=completed`다. stdout은 `artifacts/batch-manifests/full-admin-host-mutation-gate-20261110-04301.result.json`이고 stderr는 0 byte다.
- step:
  - `service-msi-hyperv-admin-smoke` exit 0(시도 1, 554.1초)
  - `os-mutation-gate` exit 0(시도 1, 11.1초)
  - executed 2, skipped 0
- 사후 검사:
  - 설치본 Host/CLI `0.43.1-admin-smoke+143a55b`는 gate build HEAD `143a55b`(Task 3 commit)와 같다. product source는 payload `c685bd51`과 같다.
  - ARP `{AFC67E1F…}` `0.43.1` 1개, PureCVisor 방화벽 규칙 `0`, listener `127.0.0.1:7777`·`127.0.0.1:80`.
  - service Running/Automatic, Web `200`, loopback 세션 `200`.
  - `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(그대로).
- 문서: `train-facts`로 fullgate 문서 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-11-10-04301-hostmutation.md`(`generated=35`, `narrative=2`)를 썼다. `train-evidence --check`는 여덟 문서 모두 `current`이고, 시험은 `27/27`이다. 문서에는 LAN 주소가 없다(`[redacted-private-endpoint]`).

operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 5: installed current-card

- [x] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다.
  - 통과 조건: `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음.
  - `pcv-it-s2-source`는 S2 template으로 기록한다. 도구가 받지 않으면 멈추고 묻는다.
  - `train-facts`로 current-card 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-10): `train-host-inputs --kind current-card --write`가 렌더된 fullgate facts에서 capture 스크립트(SHA-256 `37361625a1cb6c6d84b42acd4f42fde6e11359a152ac6a99c215ada5d8321926`)를 만들었다. 저장소 root(card root 밖)에서 `2026-11-10T08:02:48Z`에 `6`초 실행했다.
- 결과: `status=pass`, `promotion_ledger_status=not-promoted`, `canonical_current_changed=false`, secret 없음, host mutation 없음.
- 설치본: `0.43.1-admin-smoke+143a55b`, CLI 3/3(JSON 3/3), Web 2, web config token 없음, service Running/Auto. fullgate payload와 설치본 CLI·Host가 일치한다.
- 남은 시험 VM: 0이다. non-spike VM은 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`다.
- 스크립트를 root에 `capture-current-card.ps1`로 복사했다(summary SHA-256 `b55202c60a28faa83657a7198c7bebab4560dfafe21d60b06cb419eaa74e6b44`).
- 문서: `train-facts` current-card `generated=31`, `narrative=1`. `--check`는 아홉 문서 모두 `current`이고, Verification 시험(golden, facts, host-inputs)은 `32/32`이다.

operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 6: probe Web Console 화면 (queue 134)

- [x] Lane 2. 설치본 `0.43.1`이 loopback(`http://127.0.0.1/`)으로 내주는 Web Console을 확인한다. VM은 만들지 않는다.
  - 내주는 파일: `app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js`의 SHA-256이 Task 1 package payload의 같은 파일과 같은지, `sw.js` CACHE_NAME이 payload commit 소스와 같은지 본다.
  - 화면: Playwright 브라우저로 1440×900과 768×1024에서 연다. 새 브라우저 context에서 시작해 다음을 본다.
    - BL-0040: 첫 방문에 `auth/loopback-session`이 `runtime/policy`보다 먼저이고, HTTP 4xx와 console error가 0이다. version 표시가 보인다.
    - BL-0036: 768 폭에서 셸 검색 버튼의 접근 가능한 이름이 "검색"이다.
    - BL-0041: 진단 카드의 서비스 값이 `true`가 아니라 실행 중/멈춤 문구다.
    - BL-0037: 보존 VM을 골라(읽기만) 좁아진 목록에 이름 칸, 상태 점, 템플릿 배지가 보인다.
    - BL-0039: `supanova-hicontrast`에서 `--bg`, `--fg`, `--accent`가 기본 테마와 다르다. 저장값 `supanova-mockup`은 `supanova-cyan`으로 바뀐다.
    - 나머지 화면(대시보드, 네트워크, 작업, 이벤트, evidence, 계정, 템플릿, 도움말, 명령 팔레트)이 그려지고 console error 0이다.
  - BL-0038은 browser gate 시험만 바꿨으므로 설치본 probe 대상이 아니다.
  - 캡처는 `artifacts/lane2-backlog-counts-probe-20261110-04301/`에 둔다(커밋하지 않음).
  - evidence: `docs/ga-ready/evidence/lane2-backlog-counts-probe-2026-11-10-04301.md`. 로컬 commit.

실행 기록(2026-11-10 17:03~17:20 KST): 설치본 `0.43.1-admin-smoke+143a55b`(계정 없음, loopback 세션 `200`)를 읽기만 했다.
- 내주는 파일: `app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js` 다섯 개의 SHA-256이 package payload와 payload commit 소스와 같다. CACHE_NAME `pcv-ui-v6c9f22e6`도 같다.
- 수정 여섯 건:
  - BL-0040: 새 browser context의 첫 방문에서 `auth/loopback-session`이 `runtime/policy`보다 먼저이고 policy 요청은 Bearer를 가진다. 1440과 768 모두 HTTP 4xx 0, console error 0.
  - BL-0036: 768 폭 `.shell-search`는 `button "검색"`이다.
  - BL-0041: 진단 서비스 값은 "실행 중"이고 `true`·`false` 값 줄은 0이다.
  - BL-0037: 1440에서 VM을 고른 뒤 목록 2행 모두 이름 전체, 상태 점, 화면 읽기용 상태, S2 template 행의 템플릿 배지. 3열·7열 숨김.
  - BL-0039: 고대비는 `--bg #030405`, `--fg #ffffff`, `--accent #facc15`. 저장값 `supanova-mockup`은 `supanova-cyan`, `contrast`는 `supanova-hicontrast`로 바뀐다.
  - BL-0038: browser gate 시험만 바꿨으므로 대상이 아니다.
- 나머지 화면 열 개와 명령 팔레트(항목 15개)가 1440과 768에서 그려지고, 가로 스크롤 0, console error 0, HTTP 4xx·5xx 0이다.
- probe 실수 하나를 고쳤다: 처음 테마 확인은 같은 URL로 `goto`해서 페이지가 다시 읽히지 않았다. `reload()`로 다시 확인했다.
- 범위 밖: version 표시가 설치본에서 보이지 않는다(`runtime/policy`에 version 값 없음, 회귀 아님). backlog `BL-0042` `undecided`.
- 캡처 28장은 `artifacts/lane2-backlog-counts-probe-20261110-04301/`에 있다(커밋하지 않음). 테마 저장값은 원래 값으로 되돌렸다.
- evidence: `docs/ga-ready/evidence/lane2-backlog-counts-probe-2026-11-10-04301.md`.

operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 7: 계정 복원

- [x] Lane 2. Task 2 백업의 `accounts.json`을 data root에 되돌리고 SHA-256을 비교한다. fullgate 재설치가 만든 빈 저장소는 지우지 않고 백업 위치로 옮긴다. 파일 ACL은 train 전 원본 표기와 같게 맞추고 service를 다시 시작한다.
  - 확인: 계정 이름과 상태가 Task 2와 같다, loopback 세션 `409`, Web 200, service Running/Automatic.
  - 이름과 상태만 기록한다. 로컬 commit.

실행 기록(2026-11-10 17:10 KST 전후):
- data root에는 fullgate 재설치가 만든 빈 계정 저장소(0개, 상속 끊은 ACL)가 있었다. 이 파일은 지우지 않고 백업 위치로 옮겼다.
- Task 2 백업 `accounts.json`을 data root에 복사했다. SHA-256은 백업과 같다.
- 파일 ACL은 Task 2에 적어 둔 train 전 원본 표기 그대로 맞췄고, 다시 읽어 같음을 확인했다.
- service를 다시 시작했다(승인 3). loopback 세션은 `409` `PCV_LOOPBACK_SESSION_DISABLED`다. 계정 모드로 돌아왔다는 뜻이다.
- 계정 3개: `pcv-admin` admin disabled, `pcv-s4-operator` operator disabled, `pcv-admin2` admin enabled(Task 2와 같음).
- 확인: service Running/Automatic, Web `200`, PureCVisor 방화벽 규칙 0, `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로).
- 백업 위치에는 백업 `accounts.json`, 옮긴 빈 저장소, ACL 기록이 남아 있다. 지울지는 Task 11에서 사용자에게 묻는다.

operational current는 `0.43.0-admin-smoke` 그대로다.

## Task 8: Lane 3 데이터와 lane3-spec

- [x] Lane 3. 같은 train branch에서 한다.
  - consume manifest, functional carry-forward·pair consume·`main-push-payload`(payload `c685bd51`의 main push run 인용, `train_pr_head`, `path_check_line`) facts를 쓴다. `main-push` 템플릿과 함께 쓰지 않는다.
  - 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돈다.
  - `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.43.0-admin-smoke`, `previous_spec`은 `lane3-promotion-docs-spec-04300.json`이다.
  - `update-preflight-compatibility.json` fixture의 `operational-current` 행을 새 version으로 고친다(DEVELOPMENT_PROCEDURE §10, BL-0030). 로컬 commit.

실행 기록(2026-11-10): train branch `1623d62`에서 했다.
- consume manifest: `artifacts/manual-admin-campaign-20261110-04300-04301/consume-manifest.json`을 새로 썼다. closed descriptor가 읽은 summary 7개의 SHA-256과 byte 수를 담고, 파일은 복사하지 않았다.
- facts: functional carry-forward(04300 다음, 변경 commit 1개), pair consume, 단일 PR train용 `main-push-payload`를 더했다.
  - `main-push-payload`는 payload `c685bd51`(PR #134 merge)의 Public Boundary `42726128946` job `129532099888`, Development Gates `42726129133`(shard job 4개)을 인용한다.
  - `train_pr_head`는 `1623d62`이고, 그때 `train-path-check --payload c685bd51`은 `ok=true`, 바뀐 경로 18개, `product_paths`·`allowed_pin_paths` 없음이다.
  - current-card는 `promoted-current`로 바꿨다.
- `train-evidence --write --allow-update <current-card>` 결과 새 문서 3개, current-card updated. `--check`는 열두 문서 모두 current다.
- `current-evidence.json` current는 `0.43.1-admin-smoke`다. provenance `143a55b`, clean MSI `37c3163f…`, manual_admin은 `0.43.0 -> 0.43.1`(`manual-admin-campaign-20261110-04300-04301-closed`)이다.
- `update-preflight-compatibility.json`: `0.43.1-admin-smoke` `operational-current` 행(source `c685bd51`, 파일 목록은 제품 wrapper가 `6355e6ba` 뒤 바뀌지 않아 `0.43.0`과 같음)을 더했다. `0.43.0`은 `retained-older-install`로 남겼다.
- `lane3-spec --write`로 `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04301.json`을 만들었다. `index_sections.previous_version=0.43.0-admin-smoke`, descriptor chain `previous_tag=04300`이다.
- 시험: Verification(lane3-spec, train evidence, facts, host-inputs, path check) `86/86`, `UpdatePreflightCompatibilityContractTests` `3/3`. current evidence 문서 시험과 D2 시험은 Task 9(승격 문서 도구, `release-train.json` 승격)를 기다린다.

## Task 9: 승격 문서와 release-train 승격

- [x] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1`을 dry-run, `-Apply`, `-Check` 순서로 돌린다. Lane 3 숨은 단계도 한다: descriptor chain, ledger 행, index 절, D2 시험, spec pin.
  - `release-train.json`: train `0.43.1-admin-smoke` `status=promoted`, `operational_current=0.43.1-admin-smoke`.
  - 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Update-PcvContractSpecPins.ps1 -Apply`/`-Check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`를 돈다.
  - `train-path-check --payload c685bd51 --head HEAD`가 `allowed_pin_paths`만 남기는지 본다.
  - 수기 갱신: `DOCUMENTATION_INDEX` 권위 줄(공개 소스 HEAD는 payload commit), `FEATURE_IMPLEMENTATION_LEDGER` operational 줄, `DEVELOPER_INDEX` train 줄. 로컬 commit.

실행 기록(2026-11-10): promotion 도구와 수기 갱신을 했다(commit `2e060cc`).
- `Invoke-PcvLane3PromotionDocs.ps1 -SpecPath packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04301.json`
  - dry-run `ok`: current evidence 문서 stale, 나머지는 planned
  - `-Apply` `ok`: current evidence 문서, descriptor chain(꼬리표 `04301`), ledger head·rows, index 절, spec pin 갱신
  - `-Check`: 모든 단계 current
- 숨은 단계: config spec pin 3개와 Delivery verifier 상수 3개는 도구가 갱신했다. `DEVELOPER_INDEX`의 version 묶음도 도구가 갱신했다.
- `release-train.json`: train `0.43.1-admin-smoke` `status=promoted`, `operational_current=0.43.1-admin-smoke`
- 수기 갱신:
  - `docs/DOCUMENTATION_INDEX.md` 권위 줄 넷(공개 소스 HEAD는 payload `c685bd51`, 운영 `0.43.1-admin-smoke`, 최신 `main`, 설치본과 계정)
  - `docs/FEATURE_IMPLEMENTATION_LEDGER.md` operational 줄 둘
  - `docs/DEVELOPER_INDEX.md` train `0.43.1` 단락
- 검증:
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check` ok(target 8개 current)
  - `Update-PcvContractSpecPins.ps1 -Apply`/`-Check` current(spec 9개, 변경 0)
  - `dotnet test src/DesktopNode.Delivery.Tests -c Release` 785/785
  - `train-path-check --payload c685bd51 --head 2e060cc`: `ok=true`, 바뀐 경로 41개, `product_paths` 없음, `allowed_pin_paths` 6개(config spec 3개, Delivery verifier 3개)

## Task 10: train PR

- [x] clean HEAD 종료 검증을 한다.
  - `dotnet build src/DesktopNode.sln -c Release`
  - `Invoke-PcvPrGate.ps1`
  - Pester 4종
  - `npm run test:public-source-safety --prefix web` 신규 0
  - `train-path-check --payload c685bd51 --head HEAD` exit `0`
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check origin/main...HEAD`
  - 그다음 push와 PR을 한다. merge 직전 base가 `origin/main`과 같은지 본다. Required CI 다섯 개 green 뒤 merge한다(`--match-head-commit`). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

실행 기록(2026-11-10): clean HEAD `60f7811`에서 종료 검증을 했다.
- PR gate `artifacts/pr-gate/20261110-081628` `ok=True`: build, `dotnet-test-release`(솔루션 전체, clean tree), module-size-ratchet
- Pester 실패 0: packaging 528, installer 49, web 50, manual-admin 140
- `test:public-source-safety` exit 0, `Update-PcvCurrentEvidenceDocs.ps1 -Check` current, `git diff --check origin/main...HEAD` clean, base는 `origin/main`
- `train-path-check --payload c685bd51 --head 2e060cc` `ok=true`(Task 9 기록). `60f7811`은 plan과 campaign만 더 바꿨다.

PR #135(`lane1/train-04301-20261110`)를 열었고 Required CI 다섯 개(`dotnet`, `web`, `delivery`, `installer-policy`, `public-boundary-ci-required`)가 pass였다. base가 `origin/main`과 같은 것을 확인하고, 상시 merge 승인으로 `--match-head-commit 60f7811` merge했다(merge commit `5253b7e`). main push run은 Development Gates `42782473981`, Public Boundary `42782474271`이고 둘 다 success다. operational current는 이제 `0.43.1-admin-smoke`다.

## Task 11: 판정과 마무리

- [x] clean `main`에서 `pcvverify completion`을 돌린다. `complete=true`이고 hygiene C2가 queue 0·train promoted로 충족되는지 본다.
  - exit `0`이면 판정을 인용한 감사 문서 `docs/project-status-audit-2026-11-10-r2.md`를 쓴다(같은 날 첫 감사 문서는 train `0.43.0` 것이다).
  - 계정 백업 삭제를 사용자에게 물을 문장을 남긴다.
  - campaign을 닫는다. push와 PR 뒤 Required CI green이면 merge한다.

실행 기록(2026-11-10): clean `main` `5253b7e`에서 `pcvverify completion`(`artifacts/completion/20261110-r4/`)을 돌렸다.
- 결과: `complete=true met=7/7 gaps=0`, exit `0`이다.
- hygiene: C2 `met=true`(current `0.43.1-admin-smoke` = operational current, train promoted, queue 0, 미완료 train 0), C3 4/4, C4 15/15, C7 `met=false`(`open_counts=0, undecided=1`: BL-0042, `user-decision`).
- 판정을 인용한 감사 문서 `docs/project-status-audit-2026-11-10-r2.md`를 썼다(§6.2).
- campaign을 닫았다. 다음 승인으로 train 계정 백업 삭제와 BL-0042 분류를 사용자에게 묻는다.

마무리 뒤 기록(2026-11-10, 사용자 결정 7):
- 지우기 전에 data root 계정 파일과 백업 `accounts.json`의 SHA-256이 같은 것(1,153 bytes)을 확인했다.
- 보호 위치의 백업 `accounts.json`, 옮긴 빈 저장소, ACL 기록 파일을 지웠다. 비게 된 백업 폴더도 지웠다.
- 확인: data root 계정 파일의 SHA-256과 ACL이 지우기 전과 같다. service Running/Automatic, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`(계정 모드).
- backlog BL-0042를 `counts`로 분류했다(상태 `open`). 운영 위생 C7은 이제 `open_counts=1, undecided=0`이고, 고쳐 닫아야 충족한다.
- campaign `next_approval_required`를 비웠다. BL-0042 수정은 같은 결정의 2로 승인됐고, 이 PR merge 뒤 Lane 1 campaign으로 연다.

## Nonclaims

- operational current는 Task 10 merge 뒤에만 `0.43.1-admin-smoke`가 된다. 그 전까지는 `0.43.0-admin-smoke`다.
- 화면 probe는 설치본이 BL-0036~BL-0041 수정을 내주는지 보는 것이다. 디자인 감사나 접근성 판정을 새로 하지 않는다(그것은 Web gate가 한다).
- public trusted signing과 external stable publication을 주장하지 않는다.
