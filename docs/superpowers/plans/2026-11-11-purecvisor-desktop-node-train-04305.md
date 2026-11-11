# release train `0.43.5` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 제로베이스 감사 후속 campaign(`zero-base-followup-20261111`)이 queue에 쌓은 두 행(PR #152 대시보드 첫 로드, PR #153 브라우저 콘솔 입력)을 release train `0.43.5-admin-smoke`로 싣는다. package부터 Lane 3까지 단일 PR train으로 돌려 operational current로 승격한다. 그 설치본에서 S1~S3를 브라우저로 다시 시연한다(Ubuntu 설치, 설치된 VM 복제, checkpoint). backlog BL-0045를 고친다.

- 적재: `release-train.json` queue 2행(PR #152 `web`, PR #153 `hyperv+web`)이다. 둘 다 `lane2_probe`는 `null`이다.
- 계정: 지금 설치본 `0.43.4`의 계정을 train 동안 보관하고, 끝에 되살린다(train `0.43.4`과 같은 이유: fullgate `REMOVE_DATA`가 data root를 지우고, train 도구는 계정이 없을 때만 열리는 loopback 세션을 쓴다).

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10의 단일 PR train이다(`pcv-single-pr-train-v2`, 설계 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`). task 구조는 직전 승격 train plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04304.md`를 그대로 따른다.

- train branch: `lane1/train-04305-20261111`. 출발, package, 계정 보관, pair, fullgate, current-card, probe, 계정 복원, Lane 3를 모두 담고 PR 하나로 merge한다.
- payload commit: 이 opening의 `origin/main` `91009557`(PR #153 merge)다. 그 뒤 train branch commit은 문서와 Lane 3 pin 여섯 파일(SHA-256 한 줄씩, `allowed_pin_paths`)뿐이어야 한다.
- Lane 3 main push evidence는 payload commit의 main push run을 인용한다(`main-push-payload` 템플릿, `train_pr_head`, `path_check_line`).
- version: `0.43.5-admin-smoke`(꼬리표 `04305`).
- 설치본: `0.43.4-admin-smoke`(train `0.43.4` fullgate build `7b929ec`)이고 loopback 전용이다. Task 2에서 실측한다.
- 다시 쓰는 입력은 셋이다.
  - pair baseline: train `0.43.4` Task 1의 `artifacts/admin-smoke-package-20261111-04304`
  - smoke ISO: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
  - clean-host base: `current-base.json`이 가리키는 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, Playwright(설치본 Web Console 화면 probe), `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"1,2,3 fix"` (`zero-base-followup-20261111` 마무리 보고의 다음 승인 1~3에 대한 답). 각 번호의 문장은 아래와 같다.

| 항목 | 범위 |
| --- | --- |
| 1 | "(권장) release train 0.43.5-admin-smoke 출발: queue #152, #153 적재. Lane 2 unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 계정 파일 보관과 복원, 설치본 current-card와 probe(Web Console 대시보드 첫 로드와 브라우저 콘솔 입력: pcv-it- VM에서 키 하나 시간, 20자 두 칸 입력 일치, 텍스트 폼). Lane 3 current-evidence.json 승격. 단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR." Task 0~11 |
| 2 | "현행 설치본 S1~S3 브라우저 재시연(train 0.43.5 승격 뒤): Lane 2 브라우저 시나리오 스크립트와 브라우저 콘솔로 S1 Ubuntu 설치와 네트워크 확인, S2 설치된 VM template 잠금과 복제(60초 기준), S3 checkpoint 생성·복원·예약 실행(ADR-0016 pcv-it- VM 생성·시작·checkpoint·복제·삭제, 시나리오 operator 계정 pcv-s13-operator 사용과 비밀번호 재설정, 끝난 뒤 disable, 설치본·service·MSI 변경 없음). 정리에서 pcv-it-s1r-ubuntu와 이번에 만든 pcv-it- VM을 지운다. 시연 기록 PR, green CI 뒤 merge." Task 13~18 |
| 3 | "backlog 분류: BL-0045(Host 시험 ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste가 포트 0 경합으로 간헐 실패)를 counts 또는 out-of-scope로 정한다." 답 `fix`는 `counts`로 분류하고 고친다는 뜻이다(선례 `"2 수정"`, `"1, 2 문구 수정"`). Task 12 |
| train | train `0.43.5-admin-smoke` 출발(`train-departure`). queue 2행(PR #152, #153)을 `carriages`로 고정한다. MSI ProductVersion `0.43.5`, artifact 꼬리표 `04305`, pair `0.43.4 -> 0.43.5`. 단일 PR train, Lane 3 같은 branch |
| 계정 | 승인 1의 `계정 파일 보관과 복원`: train 전 `accounts.json` 백업(저장소와 제품 data 폴더 밖 보호 위치)과 data root에서 치우기, train 뒤 복원과 해시 비교. 백업 삭제는 사용자에게 묻는다 |
| probe | queue 행의 `lane2_probe`는 `null`이다. 승인 1의 probe 범위다(Task 6). 대시보드 첫 로드와 브라우저 콘솔 입력을 본다. 콘솔 입력은 직전 campaign의 `pcv-it-s1r-ubuntu`(Ubuntu 설치 프로그램 사용자 정보 화면)에서 본다 |
| 순서 | BL-0045 수정은 시험만 바꾸지만 `src/` 아래라 train 출발 뒤 merge하면 train path check에 걸린다. 그래서 train merge 뒤에 한다(Task 12, 재시연과 병렬) |
| merge | 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`(`merge_policy=after-green-ci`). merge 뒤 main push run을 확인한다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다 |

## 적재 변경

| PR | 변경 commit(merge) | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #152 | `ff40acd7`(`4d50a83f`) | web | 대시보드 첫 로드: 자동 호출은 같은 인증의 갱신에 합류하고, 대시보드 단계(`ops.summary`, `host.status`, `vm.list`)를 먼저 동시에 그린 뒤 나머지를 동시에 돈다 | Task 6 |
| #153 | `25709198`(`91009557`) | hyperv+web | backlog BL-0046·BL-0047: 같은 VM 콘솔 입력은 도착 순서대로 하나씩(service 레인과 Web Console 큐), `Msvm_Keyboard` path 캐시, `kind=text`는 글자마다 `TypeText` | Task 6 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다.
  - `pcv-guest-installed-04253-r1`의 부모 VHD를 고치거나 옮기지 않는다(BL-0028).
  - 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다.
  - train 안의 새 VM은 clean-host runner와 fullgate route smoke VM뿐이고 끝나면 지운다. 재시연(Task 13~17)의 VM은 `pcv-it-s1r2-ubuntu`, `pcv-it-s2r-clone`이고 Task 17에서 지운다.
  - `pcv-it-s1r-ubuntu`(직전 campaign S1 시도 VM)는 train 동안 그대로 둔다. current-card에는 기타 VM으로 기록된다. Task 6 probe의 콘솔 입력 대상으로 쓰고 Task 17에서 지운다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다.
  - guest 인증 정보는 실행 경계에서 무작위로 만든다.
  - 계정 백업은 저장소와 제품 data 폴더 밖 보호 위치(상속 끊음, 사용자·SYSTEM·Administrators만)에 두고, 그 경로는 문서에 적지 않는다.
  - 화면 probe 캡처는 `artifacts/` 아래(gitignore)에만 두고 커밋하지 않는다. evidence 문서에는 호스트명을 적지 않는다.
  - commit은 경로를 이름으로 stage한다.
- evidence와 artifact는 새 이름(`-04305`, `-04304-04305`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
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

- [x] 먼저 payload commit `91009557`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다.
  - `release-train.json`의 `queue` 2행을 train `0.43.5-admin-smoke`의 `carriages` `[152, 153]`로 옮긴다.
  - `status=running`, `departed_at`, `source_commit`을 적고, `queue`는 빈 목록으로 둔다.
  - `docs/ga-ready/trains/0.43.5-admin-smoke.host-inputs.json`을 쓴다. ISO는 위 smoke ISO다.
  - 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-11): 출발 시각은 `2026-11-11T15:19:00+09:00`이다.
- payload: `91009557a806037dc4f2894b342132e919e2cc4a`(PR #153 merge, origin/main)
- payload main push run: Development Gates `42943183265`과 Public Boundary Contract `42943183245`가 success다.
- carriages `[152, 153]`(2행)이다. queue를 비우고 train `status=running`으로 두었다.
- `docs/ga-ready/trains/0.43.5-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다.
- 검증: `PcvReleaseTrainContractTests` `6/6`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 다음 두 명령을 돌린다.
  - `build.ps1 -Version 0.43.5-admin-smoke -MsiProductVersion 0.43.5 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04305`
  - `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`
  - payload 수는 `update-preflight-compatibility.json` fixture와 맞는지 본다(`web/app.js`·`web/styles.css` 호환 파일 포함).
  - `train-facts`로 package 문서를 쓴다(`docs/ga-ready/trains/0.43.5-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행).
  - 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-11): clean HEAD `884d442`에서 빌드했다. product source는 payload `91009557`과 같고, 그 뒤 commit은 문서뿐이다.
- 빌드: `build.ps1` `46`초, `build_utc` `2026-11-11T06:20:51Z`, provenance `884d442ef0a30079b70061ab0b0cae82c1de4e9b`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `5f4699097d267ef3bddda081d0351f5f14ae286803d1447ce1eeeba5ef727d8d`.
- payload: `31`개, aggregate `b5151f72c9ebfa5c7cb462627ec4bb3964dac79bad8bfe34c0647c2a565b1f8d`. `0.43.4`와 같은 수이고 호환 파일 `web/app.js`·`web/styles.css`가 들어 있다.
- update ZIP: SHA-256 `a302528b383a9a29f81eb2506fdd5b26b8db751a6a4d61b2c0217c00c0ac6e4b`(entry 31). catalog SHA-256 `e2d9033b500ad715505aadb900cf9f776447670a7a95f5cef29dc04f6731fe4f`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-11-04305.md`(`generated=20`, `narrative=3`). `train-evidence --write` 뒤 `--check` 결과는 `current`다.
- 시험: `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `31/31`.

host mutation은 없다. operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 2: 계정 보관

- [x] Lane 2. 설치본 version, 계정 이름과 상태, service, 방화벽 규칙, 보존 VM을 먼저 실측한다. data root의 `accounts.json`을 보호 위치에 백업하고 SHA-256을 비교한다. 그다음 data root에서 치우고 service를 다시 시작한다.
  - 확인: `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Automatic, 보존 VM 둘 그대로.
  - 계정 이름과 상태만 적고 해시와 경로는 적지 않는다. 로컬 commit.

실행 기록(2026-11-11, 15:22 KST):
- 실측(전): 설치본 Host `0.43.4-admin-smoke+7b929ec`, ARP `{8C298285…}` `0.43.4` 1개, service Running/Automatic, PureCVisor 방화벽 규칙 0, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`.
- VM: `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s1r-ubuntu` Running(직전 campaign S1 시도 VM, Task 6 대상).
- 계정 4개: `pcv-admin` admin disabled, `pcv-admin2` admin enabled, `pcv-s13-operator` operator enabled(직전 campaign 시나리오 계정), `pcv-s4-operator` operator disabled.
- 백업: data root `accounts.json`(1,421 bytes)을 사용자 프로필 아래 보호 위치로 복사했다. 그 위치는 상속을 끊었고 사용자·SYSTEM·Administrators만 접근한다. 원본 ACL 표기도 같은 위치에 두었다. 복사본 SHA-256은 원본과 같다.
- 원본을 data root에서 지우고 service를 다시 시작했다.
- 확인: loopback 세션 `200`, Web `200`, service Running, data root에 계정 파일 없음.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 3: pair

- [x] Lane 2. orchestrator를 `-PlanOnly`로 돌린 뒤 `-Execute`로 돌린다.
  - 입력: campaign `manual-admin-campaign-20261111-04304-04305`, baseline `artifacts/admin-smoke-package-20261111-04304`, target Task 1 package, base VHD `20348.5761-20261108.vhd`.
  - 통과 조건: 여섯 bucket PASS, closed descriptor, `observation_error` 없음.
  - 끝 상태: 설치본 `0.43.5`·ARP 1개·service Running/Automatic·Web 200·보존 VM 둘 그대로.
  - `train-facts`로 pair 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11): orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). 실행 전 ARP는 `0.43.4` 1개였다.
- guest 인증 정보는 실행 스크립트 안에서 무작위로 만들어 `pscredential`로만 넘겼다. 출력하거나 저장하지 않았다.
- `-Execute`는 root `artifacts/manual-admin-campaign-20261111-04304-04305`에서 15:23:38부터 15:56:08 KST까지 돌았다(`1954`초, 직전 train `1222`초).
  - 대부분은 clean-host bucket(15:24:00~15:54:52, 약 31분)이다. 나머지 다섯 bucket은 각각 1분 안에 끝났다.
  - bucket은 PASS다.
- 여섯 bucket이 모두 PASS다: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`, `clean-host-windows-update`, `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.
- descriptor: closed descriptor를 냈다(SHA-256 `f218968d1fff75a32fb55ff696a1e45582bc06bb84557df4f7d1eef3e845bf66`). observation은 15개이고 `observation_error`는 0이다. restoration은 시도하지 않았다.
- clean-host: base UBR `5761`이고 blocker는 `none`이다.
- 끝 상태
  - 설치본 `0.43.5-admin-smoke+884d442…`, ARP `{E5963E51…}` `0.43.5` 1개, service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0
  - `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로), `pcv-it-s1r-ubuntu` Running(그대로)
  - data root에는 업그레이드된 service가 만든 빈 계정 저장소가 있고 loopback 세션은 200이다(직전 train과 같다). Task 7에서 다룬다.
- 문서: `train-facts`로 pair 문서 여섯 개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 썼다. `train-evidence --check`는 일곱 문서 모두 `current`다.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 4: fullgate

- [x] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만든다. LAN prefix가 필요하면 지금 호스트 주소에서 실행 경계로 만들고 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘긴다.
  - supervisor를 `-DryRun -AllowHostMutation`으로 돌린 뒤 `full-admin-host-mutation-gate-20261111-04305`을 돌린다.
  - 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM 둘 그대로.
  - `train-facts`로 fullgate 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11):
- manifest: `train-host-inputs --kind fullgate-manifest --write`로 만들었다(SHA-256 `d8e7e2f3336b7ed95dce1b7dfe30d7ecd181d7a1e1ddb8411f3a8533d26e9cc1`).
  - 이 호스트에 기본 게이트웨이가 있는 어댑터가 두 개라, 첫 시도(어댑터 하나를 기대하는 스크립트)는 manifest를 쓰기 전에 멈췄다.
  - LAN prefix는 직전 train manifest의 값을 쓰되, 그 주소가 지금 이 호스트의 Up 어댑터에 있는지 확인한 뒤 실행 경계에서 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘겼다. 출력하지 않았다.
- supervisor `-DryRun -AllowHostMutation`은 두 단계 계획에 `ok=true`였다. 실행 전 ARP는 1개였다.
- 실행: 15:58:38부터 16:05:19 KST까지 돌았다. `ok=true`, `status=completed`다. stdout은 `artifacts/batch-manifests/full-admin-host-mutation-gate-20261111-04305.result.json`이고 stderr는 0 byte다.
- step: `service-msi-hyperv-admin-smoke` exit 0(시도 1), `os-mutation-gate` exit 0(시도 1)
- 사후 검사
  - 설치본 Host/CLI `0.43.5-admin-smoke+8173777`는 gate build HEAD `8173777`(Task 3 commit)와 같다. product source는 payload `91009557`과 같다.
  - ARP `{BF412A3E…}` `0.43.5` 1개, PureCVisor 방화벽 규칙 `0`, listener `127.0.0.1:7777`·`127.0.0.1:80`
  - service Running/Automatic, Web `200`, loopback 세션 `200`
  - `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s1r-ubuntu` Running(그대로)
- 문서: `train-facts`로 fullgate 문서 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-11-11-04305-hostmutation.md`(`generated=35`, `narrative=2`)를 썼다. `train-evidence --check`는 여덟 문서 모두 `current`다. 문서에는 LAN 주소가 없다(`[redacted-private-endpoint]`).

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 5: installed current-card

- [x] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다.
  - 통과 조건: `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음.
  - `pcv-it-s2-source`는 S2 template으로 기록한다. 도구가 받지 않으면 멈추고 묻는다.
  - `train-facts`로 current-card 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11): `train-host-inputs --kind current-card --write`로 렌더된 fullgate facts에서 capture 스크립트(SHA-256 `ddae5ec621e21b571f88d7b289c63df7faaf09e7e928cb5681dca7af0c2d1678`)를 만들었다. 저장소 root(card root 밖)에서 `2026-11-11T07:06:33Z`에 `6`초 실행했다.
- 결과: `status=pass`, `promotion_ledger_status=not-promoted`, `canonical_current_changed=false`, secret 없음, host mutation 없음.
- 설치본: `0.43.5-admin-smoke+8173777`, CLI 3/3(JSON 3/3), Web 2, web config token 없음, service Running/Auto.
- 남은 시험 VM: 0이다. non-spike VM은 보존 VM `pcv-guest-installed-04253-r1`, S2 template `pcv-it-s2-source`, 직전 campaign S1 시도 VM `pcv-it-s1r-ubuntu`다.
- 스크립트는 root의 `capture-current-card.ps1`로 복사했다(summary SHA-256 `d5e0794743aaa955361532ac405ea5457df4032da7eceddaa20af1c099024925`).
- 문서: `train-facts` current-card `generated=31`, `narrative=1`. `--check`는 아홉 문서 모두 `current`이고, Verification 시험(golden, facts, host-inputs)은 `36/36`이다.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 6: probe 대시보드 첫 로드와 브라우저 콘솔 입력 (queue 152, 153)

- [x] Lane 2. 설치본 `0.43.5`가 loopback(`http://127.0.0.1/`)으로 내주는 Web Console을 확인한다. 계정은 Task 2에서 보관 중이라 loopback 세션이다.
  - 내주는 파일: `app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js`의 SHA-256이 Task 1 package payload의 같은 파일과 같은지, `sw.js` CACHE_NAME이 payload commit 소스와 같은지 본다.
  - 대시보드(#152)
    - 1440×900과 768×1024에서 새 context를 세 번씩 연다. `#/dashboard`가 다 차는 시각(경고 카드 목록 또는 empty, VM 수)과 `ops/summary` 요청 수(취소 포함)를 잰다.
    - 기준은 train `0.43.4` probe다(9.2~10.2초, 취소 2~3회).
    - 회귀: BL-0044 읽는 중 문구, BL-0043, 셸 version `v0.43.5-admin-smoke`, HTTP 4xx·console error 0, 화면 아홉 개
  - 콘솔 입력(#153): `pcv-it-s1r-ubuntu`의 콘솔 탭에서 연다.
    - 키 하나(press와 release) 시간을 Web Console 입력 요청 시간으로 잰다. 기준은 `0.43.4`의 요청당 약 2초다.
    - 사용자 정보 화면의 두 비밀번호 칸에 같은 20자(실행 경계에서 만듦, 기록하지 않음)를 사람이 치는 속도로 넣는다. "Passwords do not match"가 없어야 한다.
    - 텍스트 보내기 폼으로 이름 칸에 `pcv`, 서버 이름 칸에 `pcvs1r`를 넣고 그대로 들어가는지 본다.
    - 이 VM은 설치를 끝내지 않는다(재시연은 새 VM으로 한다).
  - 캡처는 `artifacts/lane2-04305-probe-20261111/`에 둔다(커밋하지 않음).
  - evidence: `docs/ga-ready/evidence/lane2-dashboard-console-probe-2026-11-11-04305.md`. FAIL이면 정차한다. 로컬 commit.

실행 기록(2026-11-11 16:07~16:16 KST): **FAIL**. 상세는 evidence `docs/ga-ready/evidence/lane2-dashboard-console-probe-2026-11-11-04305.md`에 있다.
- 내주는 파일 다섯 개와 CACHE_NAME `pcv-ui-vb668651a`는 payload와 같다.
- 대시보드(#152)
  - 새 context 여섯 번 모두 `ops/summary` 3회 요청, 그중 2회 취소였다.
  - 첫 `200`은 10.0~10.3초, 목록은 약 12.1초, 첫 갱신 끝은 12.9~13.2초였다. train 0.43.4의 목록 9.2~10.2초보다 늦다.
  - 회귀는 없다(version, 4xx 0, console error 0, "읽지 못했습니다" 0).
- 콘솔 입력(#153, `pcv-it-s1r-ubuntu`)
  - `console/input` 요청이 중앙값 2,429ms, p95 2,666ms였다(n=55). `0.43.4`의 요청당 약 2초에서 줄지 않았다.
  - 사람이 치는 속도로 보내면 처리하지 못한 입력이 Web Console 큐에 쌓였다가 브라우저를 닫을 때 사라졌다.
  - 20자 두 칸 일치와 텍스트 폼은 판단하지 못했다.
- 두 변경 모두 설치본에서 목표한 개선을 보이지 않아 정차한다(Global Constraints, 승인 1은 정차로 끝난다). backlog BL-0048(대시보드)과 BL-0049(콘솔 입력 요청 시간)를 `undecided`로 더했다.

## Task 7: 계정 복원

- [x] Lane 2. Task 2 백업의 `accounts.json`을 data root에 되돌리고 SHA-256을 비교한다. fullgate 재설치가 만든 빈 저장소는 지우지 않고 백업 위치로 옮긴다. 파일 ACL은 train 전 원본 표기와 같게 맞추고 service를 다시 시작한다.
  - 확인: 계정 이름과 상태가 Task 2와 같다, loopback 세션 `409`, Web 200, service Running/Automatic.
  - 이름과 상태만 기록한다. 로컬 commit.

실행 기록(2026-11-11 16:16 KST, 정차 절차의 첫 단계):
- 백업 `accounts.json`(1,421 bytes)을 data root에 되돌렸다. SHA-256과 ACL 표기가 백업 기록과 같다.
- fullgate 재설치가 만든 빈 저장소(174 bytes)는 지우지 않고 백업 위치로 옮겼다.
- service를 다시 시작했다.
- 확인: 계정 4개가 Task 2와 같다(`pcv-admin` disabled, `pcv-admin2` enabled, `pcv-s13-operator` enabled, `pcv-s4-operator` disabled). loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`, Web `200`, service Running/Automatic.

## 정차 (2026-11-11)

- train `0.43.5-admin-smoke`는 Task 6 probe FAIL로 정차했다. 승인 1은 여기서 끝난다.
- `release-train.json`: 출발 commit의 `0.43.5` 항목을 지우고 queue를 출발 전 상태(PR #152, #153)로 되돌렸다. 이 항목은 `main`에 merge된 적이 없다.
- 설치본은 fullgate build `0.43.5-admin-smoke+8173777`이다. 승격하지 않았으므로 operational current는 `0.43.4-admin-smoke`다.
- 하지 않은 task
  - Task 8~11(Lane 3, train PR, 판정)
  - Task 13~18(재시연과 정리, 승격된 설치본이 필요하다)
  - 재시연은 다음 승인으로 넘긴다.
- 남은 task: Task 12(BL-0045, 승인 3)는 train과 무관하므로 이 campaign에서 이어 한다.
- 이 기록(Task 0~7 evidence와 정차)은 문서 PR 하나로 merge한다.

## Task 8: Lane 3 데이터와 lane3-spec

- [ ] Lane 3. 같은 train branch에서 한다.
  - consume manifest, functional carry-forward·pair consume·`main-push-payload`(payload `91009557`의 main push run 인용, `train_pr_head`, `path_check_line`) facts를 쓴다. `main-push` 템플릿과 함께 쓰지 않는다.
  - 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돈다.
  - `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.43.4-admin-smoke`, `previous_spec`은 `lane3-promotion-docs-spec-04304.json`이다.
  - `update-preflight-compatibility.json` fixture의 `operational-current` 행을 새 version으로 고친다(DEVELOPMENT_PROCEDURE §10, BL-0030). 로컬 commit.

## Task 9: 승격 문서와 release-train 승격

- [ ] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1`을 dry-run, `-Apply`, `-Check` 순서로 돌린다. Lane 3 숨은 단계도 한다: descriptor chain, ledger 행, index 절, D2 시험, spec pin.
  - `release-train.json`: train `0.43.5-admin-smoke` `status=promoted`, `operational_current=0.43.5-admin-smoke`.
  - 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Update-PcvContractSpecPins.ps1 -Apply`/`-Check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`를 돈다.
  - `train-path-check --payload 91009557 --head HEAD`가 `allowed_pin_paths`만 남기는지 본다.
  - 수기 갱신: `DOCUMENTATION_INDEX` 권위 줄(공개 소스 HEAD는 payload commit), `FEATURE_IMPLEMENTATION_LEDGER` operational 줄, `DEVELOPER_INDEX` train 줄. 로컬 commit.

## Task 10: train PR

- [ ] clean HEAD 종료 검증을 한다.
  - `dotnet build src/DesktopNode.sln -c Release`
  - `Invoke-PcvPrGate.ps1`
  - Pester 4종
  - `npm run test:public-source-safety --prefix web` 신규 0
  - `train-path-check --payload 91009557 --head HEAD` exit `0`
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check origin/main...HEAD`
  - 그다음 push와 PR을 한다. merge 직전 base가 `origin/main`과 같은지 본다. Required CI 다섯 개 green 뒤 merge한다(`--match-head-commit`). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 11: 판정

- [ ] clean `main`에서 `pcvverify completion`을 돌린다. `complete=true`이고 hygiene C2가 queue 0·train promoted로 충족되는지 본다.
  - 결과는 이 plan에 적고 Task 12의 PR로 남긴다. 판정 입력이 바뀌지 않았으므로 새 감사 문서는 쓰지 않는다.
  - train 계정 백업 삭제는 마무리 보고의 다음 승인으로 묻는다.

## Task 12: BL-0045 Host 시험 포트 경합 (승인 3)

- [x] Lane 1. grok이 별도 worktree에서 고치고 Claude가 리뷰한다. 재시연(Task 13~)과 병렬이다.
  - 대상: `DesktopNodeHostLoopbackBootstrapBrowserTests.ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste`. 원인은 `http://127.0.0.1:0/`으로 고른 포트를 등록하기 전에 병렬 시험이 같은 포트를 먼저 쓰는 경합이다.
  - 시험 쪽에서만 고친다. 예: 시험 host 시작을 포트 충돌(`HttpListenerException`) 때 새 포트로 몇 번 다시 시도한다. 제품 코드(`src/DesktopNode.Host`)는 바꾸지 않는다. 바꿔야 하면 멈추고 보고한다.
  - backlog BL-0045를 `counts`(근거: 사용자 결정 `fix`)로 하고, merge 뒤 닫는다.
  - 검증: `dotnet test src/DesktopNode.Host.Tests -c Release`(그 시험을 여러 번), PR gate(원래 checkout에서 detached로). push, PR, green CI 뒤 merge.

실행 기록(2026-11-11): grok이 별도 worktree에서 train과 병렬로 구현했고 Claude가 리뷰했다. commit `b3be320`.
- 원인: `NormalizePrefix`가 포트 0이면 `GetFreeLoopbackPort`로 포트를 고른다. 그 함수는 확인용 소켓을 닫은 뒤에 HttpListener가 prefix를 등록한다. 그 틈에 같은 assembly의 다른 시험 class(xUnit 기본은 class마다 병렬)가 같은 포트를 먼저 등록할 수 있다.
- 고친 것: 포트 0 host를 여는 시험 class 다섯 개를 xUnit collection `DesktopNode host listener` 하나로 묶었다. 한 collection 안에서는 병렬로 돌지 않는다. 다른 시험은 그대로 병렬이다.
  - 재시도 방식은 경합을 가리고 46곳을 고쳐야 해서 쓰지 않았다.
  - 제품 코드(`src/DesktopNode.Host`)는 바꾸지 않았다.
- 남는 위험(report-only): 다른 시험 assembly(별도 프로세스)와의 포트 경합은 이 수정 밖이다.
- 결과: `dotnet test src/DesktopNode.Host.Tests -c Release` 다섯 번 모두 246/246(각 24초). 해당 시험만 20번 반복 모두 통과. `git diff --check` clean.
- backlog BL-0045를 `counts`(사용자 결정 `fix`)로 분류하고 이 PR로 닫았다.

## 마무리 (2026-11-11)

- train `0.43.5`는 Task 6에서 정차했고(PR #154), Task 12(BL-0045)로 campaign을 닫는다.
- 승인 2(S1~S3 재시연)는 승격된 설치본이 없어 하지 않았다. 다시 하려면 BL-0048·BL-0049를 고친 뒤 새 train을 출발해야 한다.

## Task 13: S1 재시연 시작 (승인 2)

- [ ] Lane 2. 설치본 `0.43.5`(승격 뒤)에서 한다.
  - `pcv-s13-operator` 비밀번호를 실행 경계에서 새로 만들어 `pcvcli account reset-password --password-env`로 바꾼다.
  - 브라우저 시나리오 스크립트 S1 `--execute`로 `pcv-it-s1r2-ubuntu`(Ubuntu ISO, vCPU 2, 4096MB, 32GB, Default Switch)를 만들고 시작한다.
  - 브라우저 콘솔로 Ubuntu Server 설치를 진행한다.

## Task 14: S1 설치 완료와 네트워크 (승인 2)

- [ ] Lane 2. 브라우저 콘솔로 사용자 정보(게스트 비밀번호는 실행 경계에서 만들고 기록하지 않음), OpenSSH, 설치 완료, 재부팅, 로그인을 한다. `ip -4 addr`와 `ping`(주소와 이름)으로 네트워크를 확인한다.

## Task 15: S2 설치된 VM 복제 (승인 2)

- [ ] Lane 2. 스크립트 S2로 `pcv-it-s1r2-ubuntu`를 끄고 template으로 잠근 뒤 `pcv-it-s2r-clone`을 복제한다(`clone_seconds`를 60초 기준과 비교). 복제본을 시작해 브라우저 콘솔에서 로그인 화면을 확인한다.

## Task 16: S3 checkpoint (승인 2)

- [ ] Lane 2. 스크립트 S3로 `pcv-it-s2r-clone`에서 checkpoint를 만들고 복원한다. 예약을 저장하고 첫 예약 실행을 기다린 뒤 지운다(`--wait-schedule`).

## Task 17: 정리 (승인 2)

- [ ] Lane 2. 스크립트 `--cleanup`으로 `pcv-it-s2r-clone`, `pcv-it-s1r2-ubuntu`(template 잠금을 먼저 푼다), `pcv-it-s1r-ubuntu`를 지운다. `pcv-s13-operator`를 끄고 저장소 밖 임시 비밀 파일을 지운다.
  - 끝 상태: 이번 `pcv-it-` VM 0개, 보존 VM 두 개 그대로, 계정은 `pcv-admin2`만 enabled, 설치본·service 그대로

## Task 18: 재시연 기록과 마무리

- [ ] `docs/ga-ready/demo/s1-s3-browser-reproof-2026-11-11-04305.md`에 Task 13~17 결과를 쓴다. 판정 입력(`config/project-completion-criteria.json`)은 바꾸지 않는다. campaign을 닫는다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 Task 10 merge 뒤에만 `0.43.5-admin-smoke`가 된다. 그 전까지는 `0.43.4-admin-smoke`다.
- probe는 설치본이 #152·#153을 내주는지 보는 것이다. 디자인 감사나 접근성 판정을 새로 하지 않는다(그것은 Web gate가 한다).
- public trusted signing과 external stable publication을 주장하지 않는다.
