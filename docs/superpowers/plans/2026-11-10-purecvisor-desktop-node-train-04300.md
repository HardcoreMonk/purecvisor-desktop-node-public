# release train `0.43.0` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** UI 개편 campaign(`ui-parity-20261109`, ADR-0020 U1~U8)이 queue에 쌓은 Web Console 변경 여덟 행을 release train `0.43.0-admin-smoke`로 싣는다. package부터 Lane 3까지 단일 PR train으로 돌려 operational current로 승격한다. 끝에 clean `main` 판정에서 hygiene C2(queue 0, train promoted)까지 확인한다.

- 적재: `release-train.json` queue 8행(PR #123~#130)이다. 모두 `web` 영역이고 `lane2_probe`는 `null`이다.
- 계정: 지금 설치본 `0.42.99`의 계정을 train 동안 보관하고, 끝에 되살린다(train `0.42.99`와 같은 이유: fullgate `REMOVE_DATA`가 data root를 지우고, train 도구는 계정이 없을 때만 열리는 loopback 세션을 쓴다).

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10의 단일 PR train이다(`pcv-single-pr-train-v2`, 설계 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`). task 구조는 직전 승격 train plan `docs/superpowers/plans/2026-11-09-purecvisor-desktop-node-train-04299.md`를 따르되, Lane 3를 같은 train branch에 쌓는다.

- train branch: `lane1/train-04300-20261110`. 출발, package, 계정 보관, pair, fullgate, current-card, probe, 계정 복원, Lane 3를 모두 담고 PR 하나로 merge한다.
- payload commit: 이 opening의 `origin/main` `6355e6ba`(PR #130 merge)다. 그 뒤 train branch commit은 문서와 Lane 3 pin 여섯 파일(SHA-256 한 줄씩, `allowed_pin_paths`)뿐이어야 한다.
- Lane 3 main push evidence는 payload commit의 main push run을 인용한다(`main-push-payload` 템플릿, `train_pr_head`, `path_check_line`).
- version: `0.43.0-admin-smoke`(꼬리표 `04300`). 사용자 결정 2를 본다.
- 설치본: `0.42.99-admin-smoke`(train `0.42.99` fullgate build `a95987b`)이고 loopback 전용이다. Task 2에서 실측한다.
- 다시 쓰는 입력은 셋이다.
  - pair baseline: train `0.42.99` Task 1의 `artifacts/admin-smoke-package-20261109-04299`
  - smoke ISO: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
  - clean-host base: `current-base.json`이 가리키는 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, Playwright(설치본 Web Console 화면 probe), `gh`

## 사용자 결정 (2026-11-10)

승인 원문 1: `"release train 0.42.100-admin-smoke 출발: queue #123, #124, #125, #126, #127, #128, #129, #130 적재. Lane 2 unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe(Web Console 화면). Lane 3 current-evidence.json 승격. 단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR."` (`ui-parity-20261109` 최종 보고의 다음 승인 1번에 대한 답)

승인 원문 2: `"0.43.0으로 출발 (권장)"` (2026-11-10 질문 "출발 문장의 version을 어떻게 할까요?"에 대한 답). train 도구는 version을 다섯 자리 꼬리표로 쓴다(`TrainLane3SpecInput.TagOf`). `0.42.100`은 꼬리표 `042100`이 되어 fullgate batch 이름 검사(`TrainHostInputs.FullgateBatchPattern`), descriptor chain 검사(`^0\d{4}$`), 다음 train의 직전 spec 경로 검사가 거절한다. 그래서 version만 `0.43.0-admin-smoke`로 바꾸고 나머지 승인 문장은 그대로 쓴다.

| 항목 | 범위 |
| --- | --- |
| 1 | train `0.43.0-admin-smoke` 출발(`train-departure`). queue 8행(PR #123~#130)을 `carriages`로 고정한다. Lane 0/1/2/3. push, PR |
| 2 | version은 `0.43.0-admin-smoke`(승인 원문 2). MSI ProductVersion `0.43.0`, artifact 꼬리표 `04300`, pair `0.42.99 -> 0.43.0` |
| 3 | Lane 2 host mutation. 원문은 `unsigned 개발 MSI build, pair 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거와 REMOVE_DATA, service 중지·시작, ADR-0016 pcv-it- VM), 설치본 current-card와 probe(Web Console 화면)`다. 해석: MSI install·repair·uninstall, REMOVE_DATA, service, pair 여섯 bucket(clean-host VM, 제품 Update/Rollback 포함), fullgate route parity VM, fullgate os-mutation-gate, 설치본 읽기, 설치본 Web Console을 브라우저로 여는 화면 probe다 |
| 4 | `Lane 3 current-evidence.json 승격`이다. `0.43.0-admin-smoke`로 쓴다. Task 8~9 |
| 5 | `단일 PR train, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR`. Task 10. 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`과 같다(`merge_policy=after-green-ci`) |
| 6 | 계정 보관과 복원은 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 연다(train `0.42.99` 사용자 결정 5와 같다). 범위는 train 전 `accounts.json` 백업(저장소와 제품 data 폴더 밖 보호 위치)과 data root에서 치우기, train 뒤 복원과 해시 비교다. 백업 삭제는 사용자에게 묻는다 |
| probe | queue 행의 `lane2_probe`는 모두 `null`이다. 승인 3의 `probe(Web Console 화면)` 범위 안에서 설치본이 내주는 Web Console이 U1~U8 화면인지 확인한다(Task 6). VM은 만들지 않는다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다. disk는 `artifacts/` 아래에 둔다 |

## 적재 변경

| PR | 변경 commit(merge) | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #123 | `866d18ef`(`aa2726ef`) | web | U1: 모바일 전원·삭제, 새 route 없는 로그인 상자, TLS scheme 표시, 명령 팔레트 하나, 등록된 단축키, 로그아웃이 modal을 닫음, 로그인 form 하나, 도움말 비밀번호 route, 화면 제목, VM 읽기 라벨 | Task 6 |
| #124 | `7b189a98`(`4fbba1b7`) | web | U2: VM 목록을 `vm-list.ts`로, 목록·상세 배치(선택 뒤 260px 목록, 900px 아래 상세만), 이름 한 번, `#/vms/<vm-id>/<tab>` deep link, 빈 상태 CTA와 읽기 실패 재시도 | Task 6 |
| #125 | `06d958af`(`e60e061a`) | web | U3: VM 행 메뉴(…, 오른쪽 클릭, ContextMenu, Shift+F10), 머리글 정렬과 j/k 선택, checkbox 선택 막대와 일괄 시작·종료, 대상 수를 입력하는 일괄 삭제 확인 | Task 6 |
| #126 | `a0178508`(`9514fd7a`) | web | U4: 화면별 렌더 등록표(`PCV.views.register`), 네트워크 화면을 `view-network.ts`로(종류 chip, 연결 VM 수 표, 빈 상태와 읽기 실패 구분) | Task 6 |
| #127 | `43bba65a`(`0dd4da16`) | web | U5: 작업 화면(상태 chip, 종류 선택, 정렬·CSV 표, 다음 page)과 이벤트 화면(심각도 카드와 chip, 이벤트 표), 작업·ops 영어 문구 i18n | Task 6 |
| #128 | `bdc5874e`(`c343c834`) | web | U6: 대시보드(KPI 카드 6개, `+ 새 VM` pagehead, 상위 VM 표, 24시간 실패 작업·ops 사건 알림), 옛 hero·빠른 실행 tile 제거 | Task 6 |
| #129 | `388bf658`(`cc0f9b9a`) | web | U7: 진단 화면(카드 4개와 닫힌 고급 절), evidence 운영자 카드, 계정 역할 chip과 역할 표 | Task 6 |
| #130 | `d32daa8f`(`6355e6ba`) | web | U8: 옛 명령 팔레트·스와이프·옛 그리기 본문 제거, 팔레트 계정 항목 `account.manage`, 패널 배경 token, 진단 i18n, `[data-view-link]` class 유지, 진단 카드 grid, axe 기준선 2,626→1,519 | Task 6 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다.
  - `pcv-guest-installed-04253-r1`의 부모 VHD를 고치거나 옮기지 않는다(BL-0028).
  - 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다.
  - 새 VM은 clean-host runner와 fullgate route smoke VM뿐이고 끝나면 지운다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다.
  - guest 인증 정보는 실행 경계에서 무작위로 만든다.
  - 계정 백업은 저장소와 제품 data 폴더 밖 보호 위치(상속 끊음, 사용자·SYSTEM·Administrators만)에 두고, 그 경로는 문서에 적지 않는다.
  - 화면 probe 캡처는 `artifacts/` 아래(gitignore)에만 두고 커밋하지 않는다. evidence 문서에는 호스트명을 적지 않는다.
- evidence와 artifact는 새 이름(`-04300`, `-04299-04300`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다.
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

- [x] 먼저 payload commit `6355e6ba`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다.
  - `release-train.json`의 `queue` 8행을 train `0.43.0-admin-smoke`의 `carriages` `[123, 124, 125, 126, 127, 128, 129, 130]`로 옮긴다.
  - `status=running`, `departed_at`, `source_commit`을 적고, `queue`는 빈 목록으로 둔다.
  - `docs/ga-ready/trains/0.43.0-admin-smoke.host-inputs.json`을 쓴다. ISO는 위 smoke ISO다.
  - 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-10): 출발 시각은 `2026-11-10T06:47:00+09:00`이다.
- payload: `6355e6ba6ae5592fa669e34e84ef316925d1876f`(PR #130 merge, origin/main)
- payload main push run: Development Gates `42650495121`과 Public Boundary Contract `42650495136`이 success다.
- carriages `[123, 124, 125, 126, 127, 128, 129, 130]`(8행)이다. queue를 비우고 train `status=running`으로 두었다.
- `docs/ga-ready/trains/0.43.0-admin-smoke.host-inputs.json`의 ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다.
- 검증: `PcvReleaseTrainContractTests` `6/6`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.42.99-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 다음 두 명령을 돌린다.
  - `build.ps1 -Version 0.43.0-admin-smoke -MsiProductVersion 0.43.0 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261110-04300`
  - `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`
  - payload 수는 `update-preflight-compatibility.json` fixture와 맞는지 본다(`web/app.js`·`web/styles.css` 호환 파일 포함).
  - `train-facts`로 package 문서를 쓴다(`docs/ga-ready/trains/0.43.0-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행).
  - 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-10): clean HEAD `87eb923`에서 빌드했다. product source는 payload `6355e6ba`와 같고, 그 뒤 commit은 문서뿐이다.
- 빌드: `build.ps1` `64`초, `build_utc` `2026-11-09T21:49:58Z`, provenance `87eb923659ff55d24cf30674b27dd32b19be8c13`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `1eb5d98424ba72c6cb42b7ef5d9c7bbe3778c29ab49a0ae28494bb0b9e1997ec`.
- payload: `31`개, aggregate `5a1c8d806932f4abbf90bf6e588e3e2e71bce33b65b496f595d757ccf9873202`. `0.42.99`와 같은 수이고 호환 파일 `web/app.js`·`web/styles.css`가 들어 있다.
- update ZIP: SHA-256 `548af0f2e9e010662f9977315c78521eb5670cc2a4e3b6d668c62b6571e229f5`(entry 31). catalog SHA-256 `1d4f31a3c6a8045a5b79e128b66afa82dcab63c2765a4397cf932a0dff51922a`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-10-04300.md`(`generated=20`, `narrative=3`). `train-evidence --write` 뒤 `--check` 결과는 `current`다.
- 시험: `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `26/26`.

host mutation은 없다. operational current는 `0.42.99-admin-smoke` 그대로다.

## Task 2: 계정 보관

- [x] Lane 2. 설치본 version, 계정 이름과 상태, service, 방화벽 규칙, 보존 VM을 먼저 실측한다. data root의 `accounts.json`을 보호 위치에 백업하고 SHA-256을 비교한다. 그다음 data root에서 치우고 service를 다시 시작한다.
  - 확인: `POST /api/v1/auth/loopback-session` 200, Web `http://127.0.0.1/` 200, service Running/Automatic, 보존 VM 둘 그대로.
  - 계정 이름과 상태만 적고 해시와 경로는 적지 않는다. 로컬 commit.

실행 기록(2026-11-10, 06:50~06:52 KST):
- 실측(전): 설치본 Host `0.42.99-admin-smoke+a95987b`, ARP `0.42.99` 1개, service Running/Auto, PureCVisor 방화벽 규칙 0, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`, `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off.
- 계정 3개: `pcv-admin` admin disabled, `pcv-s4-operator` operator disabled, `pcv-admin2` admin enabled(train `0.42.99` 복원 뒤와 같다).
- 백업: service 명령줄의 `--account-file`(data root `accounts.json`, 1,153 bytes)을 사용자 프로필 아래 보호 위치로 복사했다. 그 위치는 상속을 끊었고 사용자·SYSTEM·Administrators만 접근한다. 원본 파일의 ACL 표기도 같은 위치에 두었다. 복사본 SHA-256은 원본과 같다.
- 원본을 data root에서 지우고 service를 다시 시작했다(승인 3의 service 중지·시작).
- 확인: `POST /api/v1/auth/loopback-session` `200`, Web `http://127.0.0.1/` `200`, service Running/Automatic, data root에 계정 파일 없음, PureCVisor 방화벽 규칙 0, 보존 VM 둘 Off(그대로).

operational current는 `0.42.99-admin-smoke` 그대로다.

## Task 3: pair

- [x] Lane 2. orchestrator를 `-PlanOnly`로 돌린 뒤 `-Execute`로 돌린다.
  - 입력: campaign `manual-admin-campaign-20261110-04299-04300`, baseline `artifacts/admin-smoke-package-20261109-04299`, target Task 1 package, base VHD `20348.5761-20261108.vhd`.
  - 통과 조건: 여섯 bucket PASS, closed descriptor, `observation_error` 없음.
  - 끝 상태: 설치본 `0.43.0`·ARP 1개·service Running/Automatic·Web 200·보존 VM 둘 그대로.
  - `train-facts`로 pair 문서를 쓴다. 로컬 commit.

실행 기록(2026-11-10): orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). 실행 전 ARP는 `0.42.99` 1개였다.
- guest 인증 정보는 실행 스크립트 안에서 무작위로 만들어 `pscredential`로만 넘겼다. 출력하거나 저장하지 않았다.
- `-Execute`는 root `artifacts/manual-admin-campaign-20261110-04299-04300`에서 06:53:57부터 06:58:48 KST까지 `291`초 돌았다.
- 여섯 bucket이 모두 PASS다: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`, `clean-host-windows-update`, `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.
- descriptor: closed descriptor를 냈다(SHA-256 `8ee611f2782d121e03d0c89295280786c5df46ed683b3ff7dda40a79156d39a0`). observation은 15개이고 `observation_error`는 0이다. restoration은 시도하지 않았다(필요 없음).
- clean-host: base는 `explicit` UBR `5761`이고 blocker는 `none`이다. baseline `0.42.99` MSI 설치 뒤 제품 Update로 `0.43.0`이 됐고 Rollback으로 돌아왔다.
- 끝 상태:
  - 설치본 `0.43.0-admin-smoke+87eb923…`, ARP `{C5E6067A…}` `0.43.0` 1개, service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0이다.
  - `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off(그대로)다.
  - data root에는 제품이 다시 시작하며 만든 빈 계정 저장소가 있고 loopback 세션은 200이다.
- 문서: `train-facts`로 pair 문서 여섯 개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 썼다. `train-evidence --check`는 일곱 문서 모두 `current`다. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`는 `26/26`이다. 새 문서의 주소는 loopback과 MSIX version뿐이다.

operational current는 `0.42.99-admin-smoke` 그대로다.

## Task 4: fullgate

- [ ] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만든다. LAN prefix가 필요하면 지금 호스트 주소에서 실행 경계로 만들고 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘긴다.
  - supervisor를 `-DryRun -AllowHostMutation`으로 돌린 뒤 `full-admin-host-mutation-gate-20261110-04300`을 돌린다.
  - 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM 둘 그대로.
  - `train-facts`로 fullgate 문서를 쓴다. 로컬 commit.

## Task 5: installed current-card

- [ ] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다.
  - 통과 조건: `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0, secret 없음.
  - `pcv-it-s2-source`는 S2 template으로 기록한다. 도구가 받지 않으면 멈추고 묻는다.
  - `train-facts`로 current-card 문서를 쓴다. 로컬 commit.

## Task 6: probe Web Console 화면 (queue 123~130)

- [ ] Lane 2. 설치본 `0.43.0`이 loopback(`http://127.0.0.1/`)으로 내주는 Web Console을 확인한다. VM은 만들지 않는다.
  - 내주는 파일: `app.bundle.js`, `index.html`, `style.css`, `sw.js`의 SHA-256이 Task 1 package payload의 같은 파일과 같은지, `sw.js` CACHE_NAME이 payload commit 소스와 같은지 본다.
  - 화면: Playwright 브라우저로 1440×900과 768×1024에서 대시보드, VM 목록, VM 상세(보존 VM 읽기만), 네트워크, 작업, 이벤트, 진단, evidence, 계정, 템플릿, 도움말, 명령 팔레트(Ctrl+K)를 연다. 화면마다 등록표 화면이 그려지는지(옛 `#command-palette` 없음, KPI 카드와 pagehead 버튼 class 유지, 진단 카드 grid), console error 0을 본다.
  - 캡처는 `artifacts/lane2-ui-parity-probe-20261110-04300/`에 둔다(커밋하지 않음).
  - evidence: `docs/ga-ready/evidence/lane2-ui-parity-probe-2026-11-10-04300.md`. 로컬 commit.

## Task 7: 계정 복원

- [ ] Lane 2. Task 2 백업의 `accounts.json`을 data root에 되돌리고 SHA-256을 비교한다. fullgate 재설치가 만든 빈 저장소는 지우지 않고 백업 위치로 옮긴다. 파일 ACL은 train 전 원본 표기와 같게 맞추고 service를 다시 시작한다.
  - 확인: 계정 이름과 상태가 Task 2와 같다, loopback 세션 `409`, Web 200, service Running/Automatic.
  - 이름과 상태만 기록한다. 로컬 commit.

## Task 8: Lane 3 데이터와 lane3-spec

- [ ] Lane 3. 같은 train branch에서 한다.
  - consume manifest, functional carry-forward·pair consume·`main-push-payload`(payload `6355e6ba`의 main push run 인용, `train_pr_head`, `path_check_line`) facts를 쓴다. `main-push` 템플릿과 함께 쓰지 않는다.
  - 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돈다.
  - `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.42.99-admin-smoke`, `previous_spec`은 `lane3-promotion-docs-spec-04299.json`이다.
  - `update-preflight-compatibility.json` fixture의 `operational-current` 행을 새 version으로 고친다(DEVELOPMENT_PROCEDURE §10, BL-0030). 로컬 commit.

## Task 9: 승격 문서와 release-train 승격

- [ ] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1`을 dry-run, `-Apply`, `-Check` 순서로 돌린다. Lane 3 숨은 단계도 한다: descriptor chain, ledger 행, index 절, D2 시험, spec pin.
  - `release-train.json`: train `0.43.0-admin-smoke` `status=promoted`, `operational_current=0.43.0-admin-smoke`.
  - 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Update-PcvContractSpecPins.ps1 -Apply`/`-Check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`를 돈다.
  - `train-path-check --payload 6355e6ba --head HEAD`가 `allowed_pin_paths`만 남기는지 본다.
  - 수기 갱신: `DOCUMENTATION_INDEX` 권위 줄(공개 소스 HEAD는 payload commit), `FEATURE_IMPLEMENTATION_LEDGER` operational 줄, `DEVELOPER_INDEX` train 줄. 로컬 commit.

## Task 10: train PR

- [ ] clean HEAD 종료 검증을 한다.
  - `dotnet build src/DesktopNode.sln -c Release`
  - `Invoke-PcvPrGate.ps1`
  - Pester 4종
  - `npm run test:public-source-safety --prefix web` 신규 0
  - `train-path-check --payload 6355e6ba --head HEAD` exit `0`
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check origin/main...HEAD`
  - 그다음 push와 PR을 한다. merge 직전 base가 `origin/main`과 같은지 본다. Required CI 다섯 개 green 뒤 merge한다(`--match-head-commit`). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 11: 판정과 마무리

- [ ] clean `main`에서 `pcvverify completion`을 돌린다. `complete=true`이고 hygiene C2가 queue 0·train promoted로 충족되는지 본다.
  - exit `0`이면 판정을 인용한 감사 문서 `docs/project-status-audit-2026-11-10.md`를 쓴다.
  - 계정 백업 삭제를 사용자에게 물을 문장을 남긴다.
  - campaign을 닫는다. push와 PR 뒤 Required CI green이면 merge한다.

## Nonclaims

- operational current는 Task 10 merge 뒤에만 `0.43.0-admin-smoke`가 된다. 그 전까지는 `0.42.99-admin-smoke`다.
- 화면 probe는 설치본이 U1~U8 화면을 내주는지 보는 것이다. 디자인 감사나 접근성 판정을 새로 하지 않는다(그것은 Web gate가 한다).
- public trusted signing과 external stable publication을 주장하지 않는다.
