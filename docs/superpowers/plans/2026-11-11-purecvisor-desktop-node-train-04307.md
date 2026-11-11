# release train `0.43.7` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** queue에 쌓인 네 행을 release train `0.43.7-admin-smoke`로 싣는다.
- PR #152는 대시보드 첫 로드, PR #153은 브라우저 콘솔 입력을 고친다.
- PR #157은 콘솔 route를 전역 API lock 밖에서 처리하고 loopback 세션을 하나로 만든다.
- PR #159는 콘솔 route를 전역 요청 한도에서 빼고, 첫 service worker claim reload를 없앤다.

dev probe(`lane2-devprobe-2026-11-11-04307`)에서 통과한 효과를 train 설치본 probe로 다시 확인하고, 단일 PR train으로 operational current로 승격한다. 그 설치본에서 S1~S3를 브라우저로 다시 시연한다(Ubuntu 설치, 설치된 VM 복제, checkpoint). 같은 campaign에서 BL-0047(텍스트 보내기 폼)을 고치고, dev probe 계정 백업을 지운다.

- 적재: `release-train.json` queue 4행(PR #152 `web`, PR #153 `hyperv+web`, PR #157 `api+hyperv+web`, PR #159 `api+web`)이다. 모두 `lane2_probe`는 `null`이다.
- 계정: 지금 설치본 `0.43.6`의 계정을 train 동안 보관하고, 끝에 되살린다(train `0.43.6`과 같은 이유: fullgate `REMOVE_DATA`가 data root를 지우고, train 도구는 계정이 없을 때만 열리는 loopback 세션을 쓴다).

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10의 단일 PR train이다(`pcv-single-pr-train-v2`, 설계 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`). task 구조는 직전 train plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04306.md`(Task 6에서 정차)를 그대로 따른다.

- train branch: `lane1/train-04307-20261111`. 출발, package, 계정 보관, pair, fullgate, current-card, probe, 계정 복원, Lane 3를 모두 담고 PR 하나로 merge한다.
- payload commit: 이 opening의 `origin/main` `bf5c214`(PR #160 merge)다. product source는 `dc042b8`(PR #159 merge)와 같고, #160은 문서뿐이다. 그 뒤 train branch commit은 문서와 Lane 3 pin 여섯 파일(SHA-256 한 줄씩, `allowed_pin_paths`)뿐이어야 한다.
- Lane 3 main push evidence는 payload commit의 main push run을 인용한다(`main-push-payload` 템플릿, `train_pr_head`, `path_check_line`).
- version: `0.43.7-admin-smoke`(꼬리표 `04307`). dev probe 출력 `artifacts/devprobe-package-20261111-04307`과 다른 root를 쓴다.
- 설치본: 승격하지 않은 `0.43.6-admin-smoke`(정차한 train `0.43.6` fullgate build `bfb30c1`)다. operational current는 `0.43.4-admin-smoke`다. Task 2에서 실측한다.
- 다시 쓰는 입력은 셋이다.
  - pair baseline: train `0.43.6` Task 1의 `artifacts/admin-smoke-package-20261111-04306`
  - smoke ISO: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
  - clean-host base: `current-base.json`이 가리키는 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`
- BL-0047은 제품 코드를 바꾸므로 train branch에 싣지 않는다(`train-path-check`). grok이 저장소 밖 worktree에서 train과 병렬로 조사·구현하고, PR은 train merge 뒤에 연다(Task 18).

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, Playwright·Edge CDP(설치본 Web Console probe와 시나리오), Herdr grok, `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"1,2,3,4"` (`bl0050-bl0051-devprobe-20261111` 마무리 보고의 다음 승인 1~4에 대한 답). 각 번호의 문장은 아래와 같다.

| 항목 | 범위 |
| --- | --- |
| 1 | "(권장) release train 0.43.7-admin-smoke 출발: queue #152, #153, #157, #159 적재. Lane 2 unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 계정 파일 보관과 복원, 설치본 current-card와 probe(대시보드 첫 로드: loopback 모드 ops/summary 1회·취소 0회와 목록 시각, 브라우저 콘솔 키 입력: pcv-it- VM에서 요청 시간·거절 0건·20자 두 칸 일치. 텍스트 보내기 폼은 BL-0047로 따로 다루므로 판정에서 뺀다). Lane 3 current-evidence.json 승격. 단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR." Task 0~11 |
| 2 | "현행 설치본 S1~S3 브라우저 재시연(train 0.43.7 승격 뒤): Lane 2 브라우저 시나리오 스크립트와 브라우저 콘솔 키보드 입력으로 S1 Ubuntu 설치와 네트워크 확인, S2 설치된 VM template 잠금과 복제(60초 기준), S3 checkpoint 생성·복원·예약 실행(ADR-0016 pcv-it- VM 생성·시작·checkpoint·복제·삭제, 시나리오 operator 계정 pcv-s13-operator 사용과 비밀번호 재설정, 끝난 뒤 disable, 설치본·service·MSI 변경 없음). 정리에서 pcv-it-s1r-ubuntu와 이번에 만든 pcv-it- VM을 지운다. 시연 기록 PR, green CI 뒤 merge." Task 12~17 |
| 3 | "BL-0047 텍스트 보내기 폼: Lane 1 원인 조사와 수정(Msvm_Keyboard TypeText가 Ubuntu 설치 프로그램에서 글자를 잃는 이유, 글자를 키 이벤트로 보내는 방식 검토), host mutation 없음, grok 구현, push, PR, green CI 뒤 merge. 설치본 확인은 다음 train probe나 dev probe 승인으로." Task 18 |
| 4 | "dev probe 계정 백업 삭제: 이번 dev probe 동안 저장소 밖 보호 위치에 둔 백업 accounts.json과 ACL 기록을 지운다. data root의 계정 파일은 그대로 둔다. 지우기 전에 SHA-256이 같은지 확인한다. 기록은 다음 문서 PR에 남긴다." Task 2 앞부분(기록은 train PR) |
| train | train `0.43.7-admin-smoke` 출발(`train-departure`). queue 4행(PR #152, #153, #157, #159)을 `carriages`로 고정한다. MSI ProductVersion `0.43.7`, artifact 꼬리표 `04307`, pair `0.43.6 -> 0.43.7`(pair baseline은 정차한 train `0.43.6` package). 단일 PR train, Lane 3 같은 branch |
| Lane 3 | 직전 두 train이 정차했으므로 `lane3-spec`의 `index_sections.previous_version`은 직전 승격 `0.43.4-admin-smoke`, `previous_spec`은 `lane3-promotion-docs-spec-04304.json`이다(DEVELOPMENT_PROCEDURE §10, BL-0021). pair 값은 pair baseline `0.43.6` 그대로다 |
| 계정 | 승인 1의 `계정 파일 보관과 복원`: train 전 `accounts.json` 백업(저장소와 제품 data 폴더 밖 보호 위치)과 data root에서 치우기, train 뒤 복원과 해시 비교. 이번 train 백업의 삭제는 다음 승인으로 묻는다 |
| probe | 승인 1의 probe 범위다(Task 6). 대시보드는 loopback 모드에서 `ops/summary` 횟수·취소와 목록 시각을 본다. 콘솔 키 입력은 `pcv-it-s1r-ubuntu`(Ubuntu 설치 프로그램 사용자 정보 화면)에서 요청 시간, 거절 건수, 20자 두 칸 일치를 본다. 텍스트 보내기 폼은 판정에서 뺀다 |
| merge | 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`(`merge_policy=after-green-ci`). merge 뒤 main push run을 확인한다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다 |

## 적재 변경

| PR | 변경 commit(merge) | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #152 | `ff40acd7`(`4d50a83f`) | web | 대시보드 첫 로드: 자동 호출은 같은 인증의 갱신에 합류하고, 대시보드 단계를 먼저 동시에 그린 뒤 나머지를 동시에 돈다 | Task 6 |
| #153 | `25709198`(`91009557`) | hyperv+web | BL-0046·BL-0047: 같은 VM 콘솔 입력 순서, `Msvm_Keyboard` path 캐시, `kind=text` 글자마다 | Task 6 |
| #157 | `39976fd2`(`ee821cbd`) | api+hyperv+web | BL-0048·BL-0049: 콘솔 frame·input은 rate limit만 전역 lock 안, 한 페이지 loopback 세션 요청 하나, 키마다 VM 하나 상태 | Task 6 |
| #159 | `04b1a91`(`dc042b8`) | api+web | BL-0050·BL-0051: 콘솔 route는 전역 요청 한도에서 빠지고, 첫 service worker claim에서 reload하지 않는다 | Task 6 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다.
  - `pcv-guest-installed-04253-r1`의 부모 VHD를 고치거나 옮기지 않는다(BL-0028).
  - 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다.
  - train 안의 새 VM은 clean-host runner와 fullgate route smoke VM뿐이고 끝나면 지운다. 재시연(Task 12~16)의 VM은 `pcv-it-s1r2-ubuntu`, `pcv-it-s2r-clone`이고 Task 16에서 지운다.
  - `pcv-it-s1r-ubuntu`(S1 시도 VM)는 train 동안 그대로 둔다. current-card에는 기타 VM으로 기록된다. Task 6 probe의 콘솔 입력 대상으로 쓰고 Task 16에서 지운다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다.
  - guest 인증 정보는 실행 경계에서 무작위로 만든다.
  - 계정 백업은 저장소와 제품 data 폴더 밖 보호 위치(상속 끊음, 사용자·SYSTEM·Administrators만)에 두고, 그 경로는 문서에 적지 않는다.
  - 화면 probe 캡처는 `artifacts/` 아래(gitignore)에만 두고 커밋하지 않는다. evidence 문서에는 호스트명을 적지 않는다.
  - commit은 경로를 이름으로 stage한다.
- evidence와 artifact는 새 이름(`-04307`, `-04306-04307`)으로 쓴다. 이전 train과 dev probe의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). 예외는 Lane 3 문서 도구가 바꾸는 pin 여섯 파일의 SHA-256 줄이다(`allowed_pin_paths`). 제품 결함이 나오면 정차하고 수정 PR로 간다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다.
  - 정차하면 train 항목을 `release-train.json`에서 지우고 queue를 되돌린다.
  - 정차 사실은 plan·campaign·backlog에 기록한다.
  - FAIL은 current에 쓰지 않는다.
  - 정차하면 계정 복원(Task 7)을 먼저 하고 멈춘다.
- 같은 version 설치 전에는 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다.
- Lane 3 spec 입력(`trains/<v>.lane3-spec-input.json`)의 `narrative.functional_note`에는 backtick을 쓰지 않는다(`PCV_PROMOTION_INDEX_INVALID`, train `0.42.89`·`0.43.2`에서 두 번 났다).
- 시나리오 브라우저(Edge, Playwright)는 실행마다 profile 표지로 남은 process를 지운다(직전 campaign에서 helper가 Edge를 남겼다).
- BL-0047 grok 작업은 저장소 밖 worktree에서만 한다. main checkout은 train branch에 둔다(fullgate가 checkout HEAD에서 build한다). worktree에서는 HyperV 시험이 `.git` 파일 때문에 실패하므로 PR gate는 train merge 뒤 main checkout에서 그 commit을 detached로 돈다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).
  - 장시간 도구 실행(build, pair, fullgate)의 대기는 그 checkpoint 시간에 넣는다.
  - 기다림은 background 명령의 완료 알림으로 한다.

## Task 0: 출발

- [x] 먼저 payload commit `bf5c214`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다.
  - `release-train.json`의 `queue` 4행을 train `0.43.7-admin-smoke`의 `carriages` `[152, 153, 157, 159]`로 옮긴다.
  - `status=running`, `departed_at`, `source_commit`을 적고, `queue`는 빈 목록으로 둔다.
  - `docs/ga-ready/trains/0.43.7-admin-smoke.host-inputs.json`을 쓴다. ISO는 위 smoke ISO다.
  - BL-0047 grok brief를 보낸다(저장소 밖 worktree, base `bf5c214`, 조사·구현·focused 시험까지, push 없음). 판정과 PR은 Task 18이다.
  - 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-11): 출발 시각은 `2026-11-11T21:43:00+09:00`이다.
- payload: `bf5c2143e9e8ea90809b529dbf49805dd61c0a15`(PR #160 merge, origin/main. product source는 `dc042b8` PR #159 merge와 같다)
- payload main push run: Development Gates `43017019405`과 Public Boundary Contract `43017019394`가 success다.
- carriages `[152, 153, 157, 159]`(4행)이다. queue를 비우고 train `status=running`으로 두었다.
- `docs/ga-ready/trains/0.43.7-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다.
- BL-0047 grok brief를 21:43 KST에 보냈다. worktree는 저장소 밖 `pcv-wt-bl0047`, branch `lane1/bl0047-text-keys-20261111`(base `bf5c214`)이다.
- 검증: `PcvReleaseTrainContractTests` `6/6`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 다음 두 명령을 돌린다.
  - `build.ps1 -Version 0.43.7-admin-smoke -MsiProductVersion 0.43.7 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04307`
  - `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`
  - payload 수는 `update-preflight-compatibility.json` fixture와 맞는지 본다(`web/app.js`·`web/styles.css` 호환 파일 포함).
  - `train-facts`로 package 문서를 쓴다(`docs/ga-ready/trains/0.43.7-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행).
  - 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-11, 21:45 KST): clean HEAD `74b9562`에서 빌드했다. product source는 payload `bf5c214`(= PR #159 merge `dc042b8`)와 같고, 그 뒤 commit은 문서뿐이다.
- 빌드: `build.ps1` `47`초, `build_utc` `2026-11-11T12:45:02Z`, provenance `74b956256c585c032536a117281dadc128d9794a`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `a808cda4d54eae4caf012a8d7b012663b6caa46f683741599150dae7e0223a31`.
- payload: `31`개, aggregate `33e39bd2191fc825d62815f296b9e11b4a2b2f052125665e77d427d504a4f7ef`. 호환 파일 `web/app.js`·`web/styles.css`가 들어 있다.
- update ZIP: SHA-256 `fcc5182ac1a2e44b8ea09a5e462f1b7c6ec2b2cba4a04c9fb10b43774dbafe7d`(entry 31). catalog SHA-256 `80b202b81e303fcafe96b68bb4c17ce45626cb749f46c651f4a7cb6ace12b7df`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-11-04307.md`. facts 입력의 `canonical_current_evidence`는 `0.43.4-admin-smoke`이고, `baseline_version`은 pair baseline `0.43.6-admin-smoke`다. `train-evidence --check`는 `current`다. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `33/33`.

host mutation은 없다. operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 2: dev probe 백업 삭제와 계정 보관

- [x] Lane 2. 설치본 version, 계정 이름과 상태, service, 방화벽 규칙, 보존 VM을 먼저 실측한다.
  - 승인 4: dev probe 백업의 `accounts.json`과 data root `accounts.json`의 SHA-256이 같은지 본다. 같으면 백업 `accounts.json`과 ACL 기록 파일, 빈 백업 폴더를 지운다. 다르면 지우지 않고 멈추고 묻는다.
  - 그다음 data root의 `accounts.json`을 새 보호 위치에 백업하고 SHA-256을 비교한다. data root에서 치우고 service를 다시 시작한다.
  - 확인: `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Automatic, 보존 VM 둘 그대로.
  - 계정 이름과 상태만 적고 해시와 경로는 적지 않는다. 로컬 commit.

실행 기록(2026-11-11, 21:46~21:47 KST):
- 실측(전): 설치본 Host `0.43.6-admin-smoke+bfb30c1`(승격하지 않은 train `0.43.6` fullgate build), ARP `0.43.6` 1개, service Running/Automatic, PureCVisor 방화벽 규칙 0, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`.
- VM: `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s1r-ubuntu` Running(Task 6 대상).
- 계정 4개: `pcv-admin` admin disabled, `pcv-admin2` admin enabled, `pcv-s13-operator` operator enabled, `pcv-s4-operator` operator disabled.
- 승인 4(dev probe 백업 삭제): 백업 폴더에는 `accounts.json`(1,421 bytes)과 ACL 기록 두 파일만 있었다. 백업 `accounts.json`의 SHA-256과 ACL 표기가 data root 원본과 같아서, 두 파일과 빈 폴더를 지웠다. data root 계정 파일은 지우지 않았다(아래 train 보관 전까지 그대로).
- train 백업: data root `accounts.json`(1,421 bytes)을 사용자 프로필 아래 새 보호 위치로 복사했다. 그 위치는 상속을 끊었고 사용자·SYSTEM·Administrators만 접근한다. 원본 ACL 표기도 같은 위치에 두었다. 복사본 SHA-256은 원본과 같다.
- 원본을 data root에서 지우고 service를 다시 시작했다.
- 확인: loopback 세션 `200`, Web `200`, service Running/Automatic, data root에 계정 파일 없음, 보존 VM 둘 Off.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 3: pair

- [x] Lane 2. orchestrator를 `-PlanOnly`로 돌린 뒤 `-Execute`로 돌린다.
  - 입력: campaign `manual-admin-campaign-20261111-04306-04307`, baseline `artifacts/admin-smoke-package-20261111-04306`, target Task 1 package, base VHD `20348.5761-20261108.vhd`.
  - 통과 조건: 여섯 bucket PASS, closed descriptor, `observation_error` 없음.
  - 끝 상태: 설치본 `0.43.7`·ARP 1개·service Running/Automatic·Web 200·보존 VM 둘 그대로.
  - `train-facts`로 pair 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11):
- orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). 실행 전 ARP는 `0.43.6` 1개였다.
- 첫 실행 `manual-admin-campaign-20261111-04306-04307`(21:47:47~22:04:04 KST)은 **FAIL**이었다.
  - bucket `manual-admin-rebaseline-readiness`와 `lifecycle-product-update-rollback`은 PASS였다.
  - 세 번째 bucket `clean-host-windows-update`가 FAIL이었다. clean-host guest(UBR `5761`)가 2026-11 누적 업데이트 `KB5131090` 1개를 찾았지만 다운로드와 설치 결과가 `4`, HRESULT `0x80246007`(다운로드되지 않음)였다.
  - 같은 업데이트는 train `0.43.6` pair(같은 날 19시)에서 설치됐다. 그래서 환경 원인 일시 실패로 보고, Global Constraints대로 새 root로 한 번만 다시 돌렸다. 첫 실행 artifact는 그대로 둔다.
- 재실행 `manual-admin-campaign-20261111-04306-04307-r2`(22:05:21~22:22:25 KST)는 PASS였다.
  - orchestrator는 설치본이 target `0.43.7`(첫 실행이 남긴 제품 Update 상태)인 것을 보고 baseline으로 먼저 맞췄다(`after:baseline-alignment`).
  - 여섯 bucket이 PASS다: readiness, Update/Rollback, clean-host, Burn, MSIX, ops summary. closed descriptor를 냈고 `observation_error`는 없다.
  - 이번 clean-host guest는 적용할 누적 업데이트를 `0`개 찾았다. 그래서 UBR `5761` 그대로 설치·업데이트·롤백을 시험했고, 재부팅은 없었다(문서에 그대로 적었다).
  - 끝 상태: 설치본 Host `0.43.7-admin-smoke+74b9562`, ARP `0.43.7` 1개, service Running, Web `200`, 보존 VM 둘 Off, `pcv-it-s1r-ubuntu` Running(그대로).
- 문서: facts 입력의 `campaign_root`를 `-r2`로 두고 `train-facts`로 pair 문서 여섯 개를 썼다. `train-evidence --check`는 일곱 문서 모두 `current`다.
- 회로 차단기: 첫 실행의 원인을 읽는 동안 Lane 2 tool batch 한도(12회)를 넘겼고, 재실행은 그 뒤에 시작했다. 회로를 열고 멈춘 뒤 진행 중인 재실행만 끝까지 두었다. 사용자 승인 `"1"`(2026-11-11, Task 3 checkpoint 재개)로 새 시작 계약을 열고 재실행 결과를 판정했다.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 4: fullgate

- [x] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만든다. LAN prefix가 필요하면 직전 manifest 값이 지금 Up 어댑터에 있는지 확인하고 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘긴다.
  - supervisor를 `-DryRun -AllowHostMutation`으로 돌린 뒤 `full-admin-host-mutation-gate-20261111-04307`을 돌린다.
  - 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM 둘 그대로.
  - `train-facts`로 fullgate 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11):
- manifest: `train-host-inputs --kind fullgate-manifest --write`로 만들었다(SHA-256 `092d7355049766d7257d7f3c4dd1213fe1383cd279a1a513a809f8c6fe07f861`).
  - LAN prefix는 직전 train manifest의 값을 쓰되, 그 주소가 지금 이 호스트의 Up 어댑터에 있는지 확인한 뒤 실행 경계에서 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘겼다.
- supervisor `-DryRun -AllowHostMutation`은 `ok=true`였다. 실행 전 ARP는 1개였다.
- 실행: 22:23:59부터 22:30:46 KST까지 돌았다. `ok=true`, `status=completed`이고 stderr는 0 byte다.
- step: `service-msi-hyperv-admin-smoke` exit 0(시도 1), `os-mutation-gate` exit 0(시도 1). MSI lifecycle 6 phase가 모두 exit `0`이다.
- 사후 검사
  - 설치본 Host/CLI `0.43.7-admin-smoke+3898139`는 gate build HEAD `3898139`(Task 3 commit)와 같다. product source는 payload `bf5c214`와 같다.
  - ARP `{028F1BBE…}` `0.43.7` 1개, PureCVisor 방화벽 규칙 `0`, listener `127.0.0.1:7777`·`127.0.0.1:80`
  - service Running/Automatic, Web `200`, loopback 세션 `200`
  - 보존 VM 둘 Off, `pcv-it-s1r-ubuntu` Running(그대로)
- 문서: `train-facts`로 fullgate 문서 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-11-11-04307-hostmutation.md`를 썼다. 문서에는 LAN 주소가 없다(`[redacted-private-endpoint]`). `train-evidence --check`는 여덟 문서 모두 `current`다.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 5: installed current-card

- [x] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다.
  - 통과 조건: `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음.
  - `pcv-it-s2-source`는 S2 template으로 기록한다. 도구가 받지 않으면 멈추고 묻는다.
  - `train-facts`로 current-card 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-11): `train-host-inputs --kind current-card --write`로 capture 스크립트(SHA-256 `3ecfd687c662481be162265c3c23778980ad1ce11447a59594ad6a349e711829`)를 만들었다. 저장소 root(card root 밖)에서 `2026-11-11T13:31:55Z`에 `6`초 실행했다.
- 결과: `status=pass`, `promotion_ledger_status=not-promoted`, `canonical_current_changed=false`, secret 없음, host mutation 없음.
- 남은 시험 VM: 0이다. non-spike VM은 보존 VM 둘과 `pcv-it-s1r-ubuntu`다.
- 스크립트는 root의 `capture-current-card.ps1`로 복사했다(summary SHA-256 `afc97278eec4678b12c5f2f0a7c698547628405bcecdf6068a5b114b61f78805`).
- 문서: `train-facts` current-card. `--check`는 아홉 문서 모두 `current`다.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 6: probe 대시보드 첫 로드와 브라우저 콘솔 키 입력 (queue 152, 153, 157, 159)

- [x] Lane 2. 설치본 `0.43.7`이 loopback(`http://127.0.0.1/`)으로 내주는 Web Console을 확인한다. 계정은 Task 2에서 보관 중이라 loopback 모드다.
  - 내주는 파일: `app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js`의 SHA-256이 Task 1 package payload와 같은지, `sw.js` CACHE_NAME이 payload commit 소스와 같은지 본다.
  - 대시보드
    - 1440×900과 768×1024에서 새 context를 세 번씩 연다. loopback 세션 POST 횟수, 문서 로드 횟수, `ops/summary` 요청·취소 횟수, 목록이 그려지는 시각과 첫 갱신 끝 시각을 잰다.
    - 통과 조건: `ops/summary` 1회, 취소 0회, 문서 로드 1회. 목록이 train `0.43.6` probe(8.5초)보다 빨리 그려진다(dev probe 4.3~5.1초).
    - 회귀: 셸 version `v0.43.7-admin-smoke`, HTTP 4xx·console error 0, "운영 요약을 읽지 못했습니다" 0
  - 콘솔 키 입력: `pcv-it-s1r-ubuntu`의 콘솔 탭에서 frame이 도는 중에 canvas 키 처리기로 사람이 치는 속도(키 사이 약 0.3초)로 보낸다. 입력 큐가 빈 뒤 판정한다.
    - `console/input` 요청 시간 중앙값 0.5초 이하(train `0.43.5` 2,429ms), 거절(2xx 아님) 0건(train `0.43.6` 30건)
    - 사용자 정보 화면의 두 비밀번호 칸에 같은 20자(실행 경계에서 만듦, 기록하지 않음)를 넣는다. "Passwords do not match"가 없어야 한다.
    - 텍스트 보내기 폼은 판정에서 뺀다(BL-0047, Task 18).
  - 캡처는 `artifacts/lane2-04307-probe-20261111/`에 둔다(커밋하지 않음).
  - evidence: `docs/ga-ready/evidence/lane2-dashboard-console-probe-2026-11-11-04307.md`. FAIL이면 정차한다. 로컬 commit.

실행 기록(2026-11-11 22:34~22:37 KST): **통과**. 상세는 evidence `docs/ga-ready/evidence/lane2-dashboard-console-probe-2026-11-11-04307.md`에 있다.
- 내주는 파일 다섯 개는 Task 1 payload와 같고, `sw.js` CACHE_NAME은 payload commit 소스와 같다.
- 대시보드(loopback 모드, 1440·768 각 세 번): 매번 loopback POST 1회, 문서 로드 1회, `ops/summary` 1회, 취소 0회. 목록은 4.9~5.7초다(`0.43.6` 8.5초). version `v0.43.7-admin-smoke`, 4xx·console error 0.
- 콘솔 키 입력(`pcv-it-s1r-ubuntu`): 요청 188건, 중앙값 176ms, 최대 405ms, 거절 0건. 새 20자를 두 비밀번호 칸에 넣은 결과가 일치했다.
- 텍스트 보내기 폼은 승인 문장대로 판정에서 뺐다(BL-0047, Task 18).

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 7: 계정 복원

- [x] Lane 2. Task 2 백업의 `accounts.json`을 data root에 되돌리고 SHA-256을 비교한다. fullgate 재설치가 만든 빈 저장소는 지우지 않고 백업 위치로 옮긴다. 파일 ACL은 train 전 원본 표기와 같게 맞추고 service를 다시 시작한다.
  - 확인: 계정 이름과 상태가 Task 2와 같다, loopback 세션 `409`, Web 200, service Running/Automatic.
  - 이름과 상태만 기록한다. 로컬 commit.

실행 기록(2026-11-12 00:33 KST):
- fullgate 재설치가 만든 빈 저장소(계정 0개, 174 bytes)는 지우지 않고 백업 위치로 옮겼다.
- Task 2 백업의 `accounts.json`을 data root에 되돌렸다. SHA-256과 ACL 표기가 백업·train 전 원본 표기와 같다. service를 다시 시작했다.
- 확인: 계정 4개가 Task 2와 같다(`pcv-admin` admin disabled, `pcv-admin2` admin enabled, `pcv-s13-operator` operator enabled, `pcv-s4-operator` operator disabled). loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`, Web `200`, service Running/Automatic.
- 설치본 Host `0.43.7-admin-smoke+3898139`, 보존 VM 둘 Off, `pcv-it-s1r-ubuntu` Running(그대로).
- 이번 train 백업(백업 `accounts.json`, ACL 기록, 옮긴 빈 저장소)은 저장소 밖 보호 위치에 남아 있다. 삭제는 다음 승인으로 묻는다.

operational current는 `0.43.4-admin-smoke` 그대로다.

## Task 8: Lane 3 데이터와 lane3-spec

- [ ] Lane 3. 같은 train branch에서 한다.
  - consume manifest, functional carry-forward·pair consume·`main-push-payload`(payload `bf5c214`의 main push run 인용, `train_pr_head`, `path_check_line`) facts를 쓴다. `main-push` 템플릿과 함께 쓰지 않는다.
  - 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돈다.
  - `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.43.4-admin-smoke`, `previous_spec`은 `lane3-promotion-docs-spec-04304.json`이다(직전 train `0.43.5`·`0.43.6` 정차, BL-0021).
  - `update-preflight-compatibility.json` fixture의 `operational-current` 행을 새 version으로 고친다(DEVELOPMENT_PROCEDURE §10, BL-0030). 로컬 commit.

## Task 9: 승격 문서와 release-train 승격

- [ ] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1`을 dry-run, `-Apply`, `-Check` 순서로 돌린다. Lane 3 숨은 단계도 한다: descriptor chain, ledger 행, index 절, D2 시험, spec pin.
  - `release-train.json`: train `0.43.7-admin-smoke` `status=promoted`, `operational_current=0.43.7-admin-smoke`.
  - 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Update-PcvContractSpecPins.ps1 -Apply`/`-Check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`를 돈다.
  - `train-path-check --payload bf5c214 --head HEAD`가 `allowed_pin_paths`만 남기는지 본다.
  - 수기 갱신: `DOCUMENTATION_INDEX` 권위 줄(공개 소스 HEAD는 payload commit), `FEATURE_IMPLEMENTATION_LEDGER` operational 줄, `DEVELOPER_INDEX` train 줄. 로컬 commit.

## Task 10: train PR

- [ ] clean HEAD 종료 검증을 한다.
  - `dotnet build src/DesktopNode.sln -c Release`
  - `Invoke-PcvPrGate.ps1`
  - Pester 4종
  - `npm run test:public-source-safety --prefix web` 신규 0
  - `train-path-check --payload bf5c214 --head HEAD` exit `0`
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check origin/main...HEAD`
  - 그다음 push와 PR을 한다. merge 직전 base가 `origin/main`과 같은지 본다. Required CI 다섯 개 green 뒤 merge한다(`--match-head-commit`). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 11: 판정

- [ ] clean `main`에서 `pcvverify completion`을 돌린다. `complete=true`이고 hygiene C2가 queue 0·train promoted로 충족되는지 본다. 결과는 이 plan에 적고 Task 17의 PR로 남긴다. 새 감사 문서는 쓰지 않는다.

## Task 12: S1 재시연 시작 (승인 2)

- [ ] Lane 2. 설치본 `0.43.7`(승격 뒤)에서 한다.
  - `pcv-s13-operator` 비밀번호를 실행 경계에서 새로 만들어 `pcvcli account reset-password --password-env`로 바꾼다.
  - 브라우저 시나리오 스크립트 S1 `--execute`로 `pcv-it-s1r2-ubuntu`(Ubuntu ISO, vCPU 2, 4096MB, 32GB, Default Switch)를 만들고 시작한다.
  - 브라우저 콘솔 키보드 입력으로 Ubuntu Server 설치를 진행한다.

## Task 13: S1 설치 완료와 네트워크 (승인 2)

- [ ] Lane 2. 브라우저 콘솔 키보드 입력으로 사용자 정보(게스트 비밀번호는 실행 경계에서 만들고 기록하지 않음), OpenSSH, 설치 완료, 재부팅, 로그인을 한다. `ip -4 addr`와 `ping`(주소와 이름)으로 네트워크를 확인한다.

## Task 14: S2 설치된 VM 복제 (승인 2)

- [ ] Lane 2. 스크립트 S2로 `pcv-it-s1r2-ubuntu`를 끄고 template으로 잠근 뒤 `pcv-it-s2r-clone`을 복제한다(`clone_seconds`를 60초 기준과 비교). 복제본을 시작해 브라우저 콘솔에서 로그인 화면을 확인한다.

## Task 15: S3 checkpoint (승인 2)

- [ ] Lane 2. 스크립트 S3로 `pcv-it-s2r-clone`에서 checkpoint를 만들고 복원한다. 예약을 저장하고 첫 예약 실행을 기다린 뒤 지운다(`--wait-schedule`).

## Task 16: 정리 (승인 2)

- [ ] Lane 2. 스크립트 `--cleanup`으로 `pcv-it-s2r-clone`, `pcv-it-s1r2-ubuntu`(template 잠금을 먼저 푼다), `pcv-it-s1r-ubuntu`를 지운다. `pcv-s13-operator`를 끄고 저장소 밖 임시 비밀 파일을 지운다.
  - 끝 상태: 이번 `pcv-it-` VM 0개, 보존 VM 두 개 그대로, 계정은 `pcv-admin2`만 enabled, 설치본·service 그대로

## Task 17: 재시연 기록

- [ ] `docs/ga-ready/demo/s1-s3-browser-reproof-2026-11-11-04307.md`에 Task 12~16 결과를 쓴다. 판정 입력(`config/project-completion-criteria.json`)은 바꾸지 않는다. push, PR, green CI 뒤 merge.

## Task 18: BL-0047 텍스트 보내기 폼 (승인 3)

- [ ] Lane 1. grok이 Task 0부터 저장소 밖 worktree에서 한 조사와 구현을 검토한다. host mutation은 없다.
  - 원인: `Msvm_Keyboard.TypeText`가 Ubuntu 설치 프로그램에서 글자를 잃는 이유(키 입력 경로는 통과했다). 글자를 키 이벤트(가상 키와 Shift)로 보내는 방식을 검토한다.
  - 수정은 단위 시험으로 덮는다. 같은 PR에 `release-train.json` `queue` 행과 backlog BL-0047 `closed_by`(설치본 확인은 다음 probe로 남긴다)를 더한다.
  - `origin/main`(train merge 뒤)으로 rebase하고, main checkout에서 그 commit을 detached로 PR gate를 돈다. push, PR, green CI 뒤 merge, main push run 확인.
  - 이 PR에서 campaign을 닫는다.

## Nonclaims

- operational current는 Task 10 merge 뒤에만 `0.43.7-admin-smoke`가 된다. 그 전까지는 `0.43.4-admin-smoke`다(train `0.43.5`·`0.43.6`은 정차했다).
- probe는 설치본이 #152·#153·#157·#159를 내주는지 보는 것이다. 디자인 감사나 접근성 판정을 새로 하지 않는다(그것은 Web gate가 한다).
- BL-0047 수정은 이번 train 설치본에 없다. 설치본 확인은 다음 train probe나 dev probe 승인으로 한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
