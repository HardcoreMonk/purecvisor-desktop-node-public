# release train `0.42.99` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** backlog 정리 campaign(`backlog-burn-20261109`)이 queue에 쌓은 제품 변경 세 행을 release train `0.42.99-admin-smoke`로 싣는다. package부터 Lane 3까지 돌려 operational current로 승격한다. 끝에 clean `main` 판정에서 hygiene C2(queue 0, train promoted)까지 확인한다.

- 적재: `release-train.json` queue 3행(PR #113, #115, #116)이다. 내용은 다음 넷이다.
  - in-app 안내서 갱신
  - console 입력 제한기 clock seam
  - Web Console 수정 일곱 건(BL-0024, 0026, 0027, 0031, 0032, 0033, 0034)
  - native VM 목록 일시 공백 재읽기
- 계정: 지금 설치본 `0.42.98`의 계정 3개(`pcv-admin` disabled, `pcv-admin2` admin enabled, `pcv-s4-operator` disabled)를 train 동안 보관하고, 끝에 되살린다. 보관하는 이유는 둘이다.
  - fullgate의 `REMOVE_DATA` 제거가 data root를 지운다.
  - train 도구는 계정이 없을 때만 열리는 loopback 세션을 쓴다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10의 train이다. 직전 승격 train plan `docs/superpowers/plans/2026-11-08-purecvisor-desktop-node-train-04298.md`의 task 구조와 두 PR 방식을 따른다.

- PR A: train branch `lane1/train-04299-20261109`이다. 출발, package, 계정 보관, pair, fullgate, current-card, probe, 계정 복원 기록을 담는다. merge한 뒤 `main`에서 PR B로 Lane 3를 쓴다.
- payload commit: 이 opening의 `origin/main` `f7426d1`(PR #117 merge)이다. 그 뒤 commit은 문서뿐이어야 한다.
- 설치본: `0.42.98-admin-smoke`(train `0.42.98` fullgate build `2dfe2ac`, ARP 1개)이고 loopback 전용이다. PureCVisor 방화벽 규칙은 0개다.
- 다시 쓰는 입력은 셋이다.
  - pair baseline: train `0.42.98` Task 1의 `artifacts/admin-smoke-package-20261108-04298`
  - smoke ISO: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
  - clean-host base: `current-base.json`이 가리키는 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, `pcvcli`

## 사용자 결정 (2026-11-09)

승인 원문: `"release train 0.42.99-admin-smoke 출발"`(2026-11-09 backlog 정리 마무리 보고의 다음 승인 1번에 대한 답). 그 문장은 다음과 같다. "release train `0.42.99-admin-smoke` 출발: queue #113, #115, #116 적재. Lane 2 unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 `REMOVE_DATA`, service 중지·시작, ADR-0016 `pcv-it-` VM), 설치본 current-card와 probe. Lane 3 `current-evidence.json` 승격."

| 항목 | 범위 |
| --- | --- |
| 1 | train `0.42.99-admin-smoke` 출발(`train-departure`). queue 3행(PR #113, #115, #116)을 `carriages`로 고정한다. Lane 0/1/2/3. push, PR |
| 2 | Lane 2 host mutation. 원문은 `unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe`다. 해석: MSI install·repair·uninstall, REMOVE_DATA, service, pair 여섯 bucket(clean-host VM, 제품 Update/Rollback 포함), fullgate route parity VM, fullgate os-mutation-gate, probe VM 생성·삭제(`pcv-it-probe-*`, disk는 `artifacts/` 아래), 설치본 읽기와 queued job이다 |
| 3 | `Lane 3 current-evidence.json 승격`이다. `0.42.99-admin-smoke`로 쓴다. Task 9~11 |
| 4 | merge는 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`을 따른다(`merge_policy=after-green-ci`). merge 뒤 main push run이 red면 그 merge를 되돌리는 revert PR을 열고 멈춘다(§10) |
| 5 | 계정 보관과 복원은 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 연다. fullgate `REMOVE_DATA`가 admin 계정 `pcv-admin2`를 지우지 않게 하는 보존 조치다. 범위는 train 전 `accounts.json` 백업(저장소와 제품 data 폴더 밖 보호 위치)과 data root에서 치우기, train 뒤 복원과 해시 비교다. 백업 삭제는 사용자에게 묻는다 |
| probe | queue 행의 `lane2_probe`는 모두 `null`이다. 승인 2의 probe 범위 안에서 B2 수정 둘을 설치본으로 확인한다. BL-0034는 삭제 중 VM 목록, BL-0033은 켜진 VM 복원 거절이다(Task 6) |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다. disk는 `artifacts/` 아래에 둔다 |

## 적재 변경

| PR | 변경 commit(merge) | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #113 | `154d16cc`(`9c1df79`) | web | in-app 안내서 `web/guide-content.md`를 ADR-0019 Web Console에 맞춰 다시 썼다 | 없음 |
| #115 | `0575b859`(`0363083`) | api | BL-0011: console 입력 제한기의 1초 창을 hardening clock으로 잰다(동작 같음) | 없음 |
| #116 | `5f6c1f0c`(`3eb2a52`) | web | backlog B2. 예약 입력 유지(BL-0032), 켜진 VM 복원 거절 `PCV_CHECKPOINT_RESTORE_VM_RUNNING`(BL-0033), 포커스 복원(BL-0027), 화면 다듬기(BL-0031), 라이트 테마 제거(BL-0026), 셸 문구 i18n(BL-0024), native VM 목록 재읽기와 지운 행 제거(BL-0034) | Task 6(BL-0033, BL-0034) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다.
  - `pcv-guest-installed-04253-r1`의 부모 VHD `artifacts/image-cache/windows-server-2022-eval-vhd/20348.169…vhd`를 고치거나 옮기지 않는다(BL-0028).
  - 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다.
  - 새 VM은 clean-host runner, fullgate route smoke VM, probe VM(`pcv-it-probe-*`)뿐이고 끝나면 지운다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다.
  - guest 인증 정보는 실행 경계에서 무작위로 만든다.
  - 계정 백업은 저장소와 제품 data 폴더 밖 보호 위치(상속 끊음, 사용자·SYSTEM·Administrators만)에 두고, 그 경로는 문서에 적지 않는다.
- evidence와 artifact는 새 이름(`-04299`, `-04298-04299`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). 제품 결함이 나오면 정차하고 수정 PR로 간다.
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

- [x] 먼저 payload commit `f7426d1`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다.
  - `release-train.json`의 `queue` 3행을 train `0.42.99-admin-smoke`의 `carriages` `[113, 115, 116]`로 옮긴다.
  - `status=running`, `departed_at`, `source_commit`을 적고, `queue`는 빈 목록으로 둔다.
  - `docs/ga-ready/trains/0.42.99-admin-smoke.host-inputs.json`을 쓴다. ISO는 위 smoke ISO다.
  - 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-09): 출발 시각은 `2026-11-09T04:23:00+09:00`이다.
- payload: `f7426d1a32706ff3c52291bed4e19bff5140ea32`(PR #117 merge, origin/main)
- payload main push run: Development Gates `42514135089`와 Public Boundary Contract `42514135093`이 success다.
- carriages `[113, 115, 116]`(3행)이다. queue를 비우고 train `status=running`으로 두었다.
- `docs/ga-ready/trains/0.42.99-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다.
- 검증: `PcvReleaseTrainContractTests` `6/6`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.42.98-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 다음 두 명령을 돌린다.
  - `build.ps1 -Version 0.42.99-admin-smoke -MsiProductVersion 0.42.99 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261109-04299`
  - `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`
  - payload 수는 `update-preflight-compatibility.json` fixture와 맞는지 본다(`web/app.js`·`web/styles.css` 호환 파일 포함).
  - `train-facts`로 package 문서를 쓴다(`docs/ga-ready/trains/0.42.99-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행).
  - 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-09): clean HEAD `4931d6d`에서 빌드했다. product source는 payload `f7426d1`과 같고, 그 뒤 commit은 문서뿐이다.
- 빌드: `build.ps1` `51`초, `build_utc` `2026-11-08T19:25:01Z`, provenance `4931d6d59c45a0775f443aa87ea5fe9208467203`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `7946f464245edf9f7ab84685add805fa7656274a8511525fade9a2c0c6842af0`.
- payload: `31`개, aggregate `e03dcb32faa37db01b8c4900151e0b51b7859fcbb3bb31e1f3f6e69099ea9c57`. `0.42.98`과 같은 수이고 호환 파일 `web/app.js`·`web/styles.css`가 들어 있다.
- update ZIP: SHA-256 `aec1eb05f07c64e31e8ba4f8cdcd55f3c220af621055a6193a84e17d51498e84`(entry 31). catalog SHA-256 `ae1fe6842fc898ef4314c530c6afd7224944e5e7c5cbecbc6af4677506d967cd`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-09-04299.md`(`generated=20`, `narrative=3`). `train-evidence --write` 뒤 `--check` 결과는 `current`다.
- 시험: `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `25/25`.

host mutation은 없다. operational current는 `0.42.98-admin-smoke` 그대로다.

## Task 2: 계정 보관

- [x] Lane 2. data root의 `accounts.json`을 보호 위치에 백업하고 SHA-256을 비교한다. 그다음 data root에서 치운다.
  - 확인: `GET /api/v1/auth/bootstrap-state`가 `no-default-account`, `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Automatic, 보존 VM 둘 그대로.
  - 계정 이름과 상태만 적고 해시와 경로는 적지 않는다. 로컬 commit.

실행 기록(2026-11-09, 04:30 KST 전후):
- 백업: service 명령줄의 `--account-file`(data root 바로 아래 `accounts.json`)을 사용자 프로필 아래 보호 위치로 복사했다. 그 위치는 상속을 끊었고 사용자·SYSTEM·Administrators만 접근한다. 원본 파일의 ACL 표기도 같은 위치에 함께 두었다. 복사본 SHA-256은 원본과 같다.
- 계정 3개: `pcv-admin` admin disabled, `pcv-s4-operator` operator disabled, `pcv-admin2` admin enabled.
- 원본을 data root에서 지웠다. service가 계정 상태를 메모리에 들고 있어서 loopback 세션은 그대로 `409` `PCV_LOOPBACK_SESSION_DISABLED`였다. 그래서 service를 다시 시작했다(승인 2의 service 중지·시작).
- 확인: `POST /api/v1/auth/loopback-session` `200`, Web `http://127.0.0.1/` `200`, service Running/Automatic, data root에 계정 파일 없음, PureCVisor 방화벽 규칙 0, `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로).
- 계획의 `GET /api/v1/auth/bootstrap-state`는 인증 없이 `401`이라 쓰지 않았다. loopback 세션 `200`이 같은 상태(계정 없음)를 보인다.

operational current는 `0.42.98-admin-smoke` 그대로다.

## Task 3: pair

- [x] Lane 2. orchestrator를 `-PlanOnly`로 돌린 뒤 `-Execute`로 돌린다.
  - 입력: campaign `manual-admin-campaign-20261109-04298-04299`, baseline `artifacts/admin-smoke-package-20261108-04298`, target Task 1 package, base VHD `20348.5761-20261108.vhd`.
  - 통과 조건: 여섯 bucket PASS, closed descriptor, `observation_error` 없음.
  - 끝 상태: 설치본 `0.42.99`·ARP 1개·service Running/Automatic·Web 200·보존 VM 둘 그대로.
  - `train-facts`로 pair 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-09): orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). 실행 전 ARP는 `0.42.98` 1개였다.
- `-Execute`는 root `artifacts/manual-admin-campaign-20261109-04298-04299`에서 04:29:46부터 04:33:59 KST까지 `253`초 돌았다.
- 여섯 bucket이 모두 PASS다: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`, `clean-host-windows-update`, `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.
- descriptor: closed descriptor를 냈다(SHA-256 `1bc134120e125a9566a93690a157af5c73091703188d1f978e74e666c63cb3b4`). observation은 15개이고 `observation_error`는 0이다. restoration은 시도하지 않았다(필요 없음).
- clean-host:
  - base는 `explicit` UBR `5761`이다. `current-base.json`은 같은 base라 다시 지정하지 않았다.
  - baseline `0.42.98` MSI(`9a26d4ce…`) 설치 뒤 제품 Update로 `0.42.99`가 됐고, Rollback으로 돌아왔다. 둘 다 exit 0이다.
  - Web은 200이고 blocker는 `none`이다.
- 끝 상태:
  - 설치본 `0.42.99-admin-smoke+4931d6d…`, ARP `{004AC8CC…}` `0.42.99` 1개, service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0이다.
  - `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로)다.
  - data root에는 제품이 다시 시작하며 만든 빈 계정 저장소(0개)가 있고 loopback 세션은 200이다.
- 문서: `train-facts`로 pair 문서 여섯 개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 썼다. `train-evidence --check`는 일곱 문서 모두 `current`다. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`는 `25/25`다. 새 문서의 주소는 loopback과 MSIX version뿐이다.

operational current는 `0.42.98-admin-smoke` 그대로다.

## Task 4: fullgate

- [x] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만든다. LAN prefix가 필요하면 지금 호스트 주소에서 실행 경계로 만들고 환경 변수로만 넘긴다.
  - supervisor를 `-DryRun -AllowHostMutation`으로 돌린 뒤 `full-admin-host-mutation-gate-20261109-04299`를 돌린다.
  - 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM 둘 그대로.
  - `train-facts`로 fullgate 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-09):
- manifest: `train-host-inputs --kind fullgate-manifest --write`로 만들었다(SHA-256 `790309fe7c0e317e9cea4ee2a796a97f39f565d67f93e7408a451c6f75a38ad6`).
  - LAN prefix는 기본 게이트웨이가 있는 호스트 사설 IPv4로 실행 경계에서 만들었다. 직전 train이 쓴 S4 백업은 지웠기 때문이다.
  - 값은 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘겼고 출력하지 않았다.
- supervisor `-DryRun -AllowHostMutation`은 두 단계 계획에 `ok=true`였다. 실행 전 ARP는 1개였다.
- 실행: 04:36:39부터 04:43:51 KST까지 돌았다. `ok=true`, `status=completed`다. stdout은 `artifacts/batch-manifests/full-admin-host-mutation-gate-20261109-04299.result.json`이고 stderr는 0 byte다.
- step:
  - `service-msi-hyperv-admin-smoke` exit 0(시도 1, 420.2초)
  - `os-mutation-gate` exit 0(시도 1, 11.1초)
  - executed 2, skipped 0
- 사후 검사:
  - 설치본 Host/CLI `0.42.99-admin-smoke+a95987b`는 gate build HEAD `a95987b`(Task 3 commit)와 같다. product source는 payload `f7426d1`과 같다.
  - ARP `{23EEF15A…}` `0.42.99` 1개, PureCVisor 방화벽 규칙 `0`, listener `127.0.0.1:7777`·`127.0.0.1:80`.
  - service Running/Automatic, Web `200`, loopback 세션 `200`.
  - `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(그대로).
- 문서: `train-facts`로 fullgate 문서 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-11-09-04299-hostmutation.md`(`generated=35`, `narrative=2`)를 썼다. `train-evidence --check`는 여덟 문서 모두 `current`이고, 시험은 `25/25`다. 문서에는 LAN 주소가 없다(`[redacted-private-endpoint]`).

operational current는 `0.42.98-admin-smoke` 그대로다.

## Task 5: installed current-card

- [x] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다.
  - 통과 조건: `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음.
  - `pcv-it-s2-source`는 S2 template으로 기록한다. 도구가 받지 않으면 멈추고 묻는다.
  - `train-facts`로 current-card 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-09): `train-host-inputs --kind current-card --write`가 렌더된 fullgate facts에서 capture 스크립트(SHA-256 `aa0f343d2af7f5b4b7a37108d646503b37d43d6dc26f6124c542922829b1d412`)를 만들었다. 저장소 root(card root 밖)에서 `2026-11-08T19:45:15Z`에 `5`초 실행했다.
- 결과: `status=pass`, `promotion_ledger_status=not-promoted`, `canonical_current_changed=false`, secret 없음, host mutation 없음.
- 설치본: `0.42.99-admin-smoke+a95987b`, CLI 3/3(JSON 3/3), Web 2, web config token 없음, service Running/Auto. payload와 설치본 CLI·Host가 일치한다.
- 남은 시험 VM: 0이다. non-spike VM은 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`다.
- 스크립트를 root에 `capture-current-card.ps1`로 복사했다(summary SHA-256 `0620149871e072955e4be6bcdd517b09e8f012d7c9dd52cf1086f0d019b9805f`).
- 문서: `train-facts` current-card `generated=31`, `narrative=1`. `--check`는 아홉 문서 모두 `current`이고, Verification 시험(golden, facts, host-inputs)은 `30/30`이다.

operational current는 `0.42.98-admin-smoke` 그대로다.

## Task 6: probe B2 수정 (queue 116)

- [ ] Lane 2. 설치본 `0.42.99` loopback에서 PCVCLI(service bearer)로 확인한다. 쓰는 VM은 ADR-0016 `pcv-it-probe-list-1109b`(Generation 2, smoke ISO, disk `artifacts/` 아래)이다.
  - BL-0034: 만들고 지우기를 세 번 돌린다. 각 job 동안과 끝난 직후 `vm list`를 이어 읽는다. Task 4(2026-11-09) 재현과 같은 방법이다. 기대는 목록 실패 0회다.
  - BL-0033: VM을 켠 상태에서 checkpoint를 만들고 복원을 요청한다. `409` `PCV_CHECKPOINT_RESTORE_VM_RUNNING`이 job 없이 오는지 본다. 끈 뒤 복원은 `202`로 job이 성공하는지 본다.
  - 끝 상태: `pcv-it-probe-*` 0개, 폴더 정리.
  - evidence: `docs/ga-ready/evidence/lane2-backlog-b2-probe-2026-11-09-04299.md`. 로컬 commit.

## Task 7: 계정 복원

- [ ] Lane 2. Task 2 백업의 `accounts.json`을 data root에 되돌리고 SHA-256을 비교한다. fullgate 재설치가 만든 빈 저장소는 대체한다. 파일 ACL은 제품이 빈 저장소에 둔 것과 같게 맞춘다.
  - 확인: 계정 3개(`pcv-admin` disabled, `pcv-admin2` enabled, `pcv-s4-operator` disabled), loopback 세션 `409`, Web 200, service Running/Automatic.
  - 이름과 상태만 기록한다. 로컬 commit.

## Task 8: train PR A

- [ ] clean HEAD 종료 검증을 한다.
  - `dotnet build src/DesktopNode.sln -c Release`
  - `Invoke-PcvPrGate.ps1`
  - Pester 4종
  - `npm run test:public-source-safety --prefix web` 신규 0
  - `train-path-check --payload f7426d1 --head HEAD` exit `0`
  - 그다음 push와 PR을 한다. Required CI 다섯 개 green 뒤 merge한다(`--match-head-commit`, 상시 승인). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 9: Lane 3 데이터와 lane3-spec

- [ ] Lane 3. `main`에서 branch를 만든다.
  - consume manifest, functional carry-forward·pair consume·`main-push`(PR A merge의 main push run 인용) facts를 쓴다.
  - 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돈다.
  - `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.42.98-admin-smoke`다.
  - `update-preflight-compatibility.json` fixture의 `operational-current` 행을 새 version으로 고친다(DEVELOPMENT_PROCEDURE §10, BL-0030). 로컬 commit.

## Task 10: 승격 문서와 release-train 승격

- [ ] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1`을 dry-run, `-Apply`, `-Check` 순서로 돌린다. Lane 3 숨은 단계도 한다: descriptor chain, ledger 행, index 절, D2 시험, spec pin, current evidence 시험 version 줄.
  - `release-train.json`: train `0.42.99-admin-smoke` `status=promoted`, `operational_current=0.42.99-admin-smoke`.
  - 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Apply`/`-Check`, `Update-PcvContractSpecPins.ps1 -Apply`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`를 돈다.
  - 수기 갱신: `DOCUMENTATION_INDEX` 권위 줄, `FEATURE_IMPLEMENTATION_LEDGER` operational 줄, `DEVELOPER_INDEX` train 줄. 로컬 commit.

## Task 11: Lane 3 PR B

- [ ] clean HEAD 종료 검증(Task 8과 같음) 뒤 push와 PR을 한다. Required CI 다섯 개 green 뒤 merge한다. merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 12: 판정과 마무리

- [ ] clean `main`에서 `pcvverify completion`을 돌린다. `complete=true`이고 hygiene C2가 queue 0·train promoted로 충족되는지 본다.
  - 계정 백업 삭제를 사용자에게 물을 문장을 남긴다.
  - campaign을 닫는다. push와 PR 뒤 Required CI green이면 merge한다.

## Nonclaims

- operational current는 Task 11 merge 뒤에만 `0.42.99-admin-smoke`가 된다. 그 전까지는 `0.42.98-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
