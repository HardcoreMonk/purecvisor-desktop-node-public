# release train `0.42.98` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.42.97`이 pair clean-host bucket에서 정차했다(BL-0029). 수정 PR #108이 merge됐으므로 release train `0.42.98-admin-smoke`로 다시 출발한다.

- 적재: `release-train.json` queue 14행이다. PR #94~#108이고, #96은 queue 행이 없다. 내용은 `configure-installed` LAN 옵션, Web Console 재설계 전체, 비밀번호 재설정 API, 이전 버전 제품 Update 호환 파일이다.
- 범위: package부터 Lane 3까지 돌려 operational current로 승격한다.
- S4 상태: train `0.42.97` Task 2가 보관한 S4 시연 상태(계정, LAN 설정)를 train 끝에 되살린다. 그 뒤 새 화면의 설치본에서 S4 시연을 이어 간다.
- 이관: train `0.42.97`의 남은 Task 4~17을 이 campaign의 Task 2~15로 옮긴다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 train의 정차 뒤 다음 patch version 출발이다(train `0.42.94` 정차 → `0.42.95`와 같다). 직전 plan `docs/superpowers/plans/2026-11-08-purecvisor-desktop-node-train-04297.md`(정차)의 task 구조와 두 PR 방식을 따른다.

- PR A: train branch `lane1/train-04298-20261108`(출발, package, pair, fullgate, current-card, probe 기록, S4 복원)이다. merge한 뒤 PR B로 Lane 3를 쓴다.
- payload commit: 이 opening의 `origin/main` `8e4ebe2`(PR #108 merge)다.
- 설치본: train `0.42.97` Task 4b가 제품 Rollback으로 되돌린 `0.42.96-admin-smoke`(ARP 1개)다. loopback이고, 계정 파일은 보관 위치에 있다(data root에는 service가 만든 빈 계정 저장소).
- 다시 쓰는 입력: pair baseline은 train `0.42.97` Task 1이 다시 만든 `artifacts/admin-smoke-package-20261108-04296r`다. smoke ISO는 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`, clean-host base는 Task 3a의 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.5761-20261108.vhd`다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `New-PcvCleanHostBaseVhd.ps1 -SetCurrentBasePath`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvLane3PromotionDocs.ps1`, `DesktopNode.Host.exe configure-installed`, `pcvcli account`, Edge/Chrome CDP 브라우저 스크립트

## 사용자 결정 (2026-11-08)

승인 원문: `"PR #108 merge 승인"`(train `0.42.97` 정지 보고의 다음 승인 1번 권장안에 대한 답)과 권장안 자동 승인 `앞으로는 권장 사항 자동 승인 처리 한다.`(같은 날 사용자 지시, train `0.42.97` plan 사용자 결정 7). 권장안 원문은 다음과 같다. "merge하고 `main` push run green을 확인한 뒤 train `0.42.98-admin-smoke`로 다시 출발합니다. 적재는 queue 14행이고, 범위는 2026-11-08 승인 2~4를 새 version에 그대로 적용합니다. Task 4~17은 그 campaign으로 옮깁니다."

| 항목 | 범위 |
| --- | --- |
| 1 | train `0.42.98-admin-smoke` 출발(`train-departure`). queue 14행(PR #94~#108)을 `carriages`로 고정한다. Lane 0/1/2/3. push, PR. merge는 PR마다 사용자의 명시 merge 문장으로 한다. 권장안 자동 승인만으로는 auto mode가 merge를 거부했다(2026-11-08) |
| 2 | 2026-11-08 승인 2를 그대로 적용한다. 원문: `Lane 2 host mutation: unsigned 개발 MSI 빌드, 업그레이드 시험과 fullgate(msiexec 설치·업그레이드·repair·제거(REMOVE_DATA 포함), service 중지·시작, ADR-0016 pcv-it- 일회용 VM), 설치본 확인 카드와 동작 확인.` 해석: MSI install·repair·uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, fullgate os-mutation-gate, probe VM 생성·삭제(`pcv-it-` 접두사, disk는 `artifacts/` 아래), 설치본 브라우저 확인(읽기와 queued job), 제품 Update/Rollback. clean-host PASS 뒤 `current-base.json` 지정을 포함한다 |
| 3 | 2026-11-08 승인 3을 그대로 적용한다. 원문: `S4 상태 보존: train 전에 accounts.json을 저장소 밖 보호된 위치에 백업하고 치운 뒤 loopback으로 train을 돌린다. 끝나면 계정을 복원하고 새 설치본의 configure-installed로 LAN 설정(--allow-lan, 방화벽 관리 규칙 2개)을 다시 적용한 뒤 새 화면으로 S4 시연을 이어 간다. 보존 VM, template, pcv-it-s4-console은 건드리지 않는다(도구가 요구하면 멈추고 묻는다).` 해석: 보관은 train `0.42.97` Task 2가 이미 했다. 복원은 Task 7이다 |
| 4 | `Lane 3: current-evidence.json을 0.42.98-admin-smoke로 승격한다.` Lane 3 `current-evidence.json` 쓰기. Task 9~11 |
| 5 | 권장안 자동 승인(train `0.42.97` 사용자 결정 7)이 계속된다. 정지 보고의 "(권장)" 선택지는 그 문장 범위대로 잇는다. merge, 되돌릴 수 없는 사용자 데이터 삭제(S4 계정 백업), 사용자 몫(S4 사용자 시연, admin `pcv-admin2` 비밀번호)은 사용자에게 묻는다 |
| merge | PR마다 사용자의 명시 merge 문장(`merge_policy=none`). main push run이 red면 그 merge를 되돌리는 revert PR을 열고 멈춘다 |
| 이관 | train `0.42.97`(`train-04297-20261108`) Task 4~17을 Task 2~15로 옮긴다. 문장은 version을 `0.42.98`로, artifact 이름을 `-04298`/`-04296-04298`로 바꾼 것 말고는 그대로다. 원래 S4 task(`ui-redesign-20261105` Task 11~14)의 승인 범위도 그대로다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다. disk는 `artifacts/` 아래에 둔다 |

## 적재 변경

train `0.42.97` plan의 적재 변경 13행(PR #94~#107)과 다음 한 행이다.

| PR | merge commit | 영역 | 요약 | Lane 2 probe |
| --- | --- | --- | --- | --- |
| #108 | `d8bf7be9`(merge `8e4ebe2`) | web | 주석만 든 `web/app.js`, `web/styles.css`를 payload에 남긴다(WebPayload.wxs 소유). `0.42.96` 이하 설치본의 제품 Update preflight가 새 payload를 받는다(BL-0029) | 없음(pair clean-host bucket이 본다) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`, S2 template `pcv-it-s2-source`, S1 시연 VM `pcv-it-s4-console`의 전원, Notes, 디스크, checkpoint를 바꾸지 않는다. 도구가 남은 `pcv-it-` VM을 문제 삼으면 멈추고 묻는다. 새 VM은 clean-host runner, fullgate route smoke VM, probe VM(`pcv-it-probe-*`)뿐이고 끝나면 지운다. probe VM disk는 `artifacts/` 아래에 둔다.
- token, credential, password, 계정 해시는 command line, summary, evidence, 스크린샷에 남기지 않는다. guest 인증 정보는 실행 경계에서 무작위로 만든다. 계정 백업과 보관한 service 명령줄은 저장소와 제품 데이터 폴더 밖 보호된 위치에만 두고 그 경로는 문서에 적지 않는다. 백업은 S4 판정(Task 14)까지 두고, 지울지는 사용자에게 묻는다.
- evidence와 artifact는 새 이름(`-04298`, `-04296-04298`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다. 사용자 홈 경로·LAN IP·호스트명은 문서에 적지 않는다(LAN 주소는 `<host-lan-ip>`).
- train branch에서는 `src/`, `web/src/`, `config/`, `.github/`를 바꾸지 않는다(`train-path-check`). 제품 결함이 나오면 정차하고 수정 PR로 간다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다(train 항목은 `release-train.json`에서 지우고 queue를 복귀, 기록은 plan·campaign·backlog). FAIL은 current에 쓰지 않는다.
- 같은 version 설치 전에는 ARP의 `PureCVisor Desktop Node` 항목 수를 확인한다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다). 장시간 도구 실행(build, pair, fullgate) 대기는 그 checkpoint 시간에 넣고, 넘으면 보고하고 멈춘다. 기다림은 완료 알림(background 명령)으로 하고, 파일 대기 스크립트는 경로를 POSIX 형식으로 바로 쓴다.

## Task 0: 출발

- [x] payload commit `8e4ebe2`의 main push Development Gates와 Public Boundary가 success임을 run id로 적는다. `release-train.json` `queue` 14행을 train `0.42.98-admin-smoke`의 `carriages` `[94, 95, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108]`로 옮기고 `status=running`, `departed_at`, `source_commit`을 적은 뒤 `queue`는 빈 목록으로 둔다. `docs/ga-ready/trains/0.42.98-admin-smoke.host-inputs.json`을 쓴다(ISO는 train `0.42.97` Task 1의 smoke ISO). 검증: `PcvReleaseTrainContractTests`, `git diff --check`. host mutation 없음. 로컬 commit.

실행 기록(2026-11-08): 출발 시각은 `2026-11-08T15:50:00+09:00`, payload는 `8e4ebe24b7fc93241fb2f09979d7019f81271f22`(PR #108 merge, origin/main)다. carriages는 `[94, 95, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108]`(14행)이고, queue를 비우고 train `status=running`으로 두었다. payload main push Development Gates `42456130722`와 Public Boundary Contract `42456130718`은 success다. `docs/ga-ready/trains/0.42.98-admin-smoke.host-inputs.json`의 ISO는 train `0.42.97` Task 1의 `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`(`45056` byte, 있음)다. 설치본은 `0.42.96`(train `0.42.97` Task 4b Rollback)이고, loopback이며, 계정은 보관 중이다. 검증: `PcvReleaseTrainContractTests` `6/6`, `dotnet test src/DesktopNode.Delivery.Tests -c Release` `781/781`, `git diff --check` 통과. host mutation은 없다. operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.98-admin-smoke -MsiProductVersion 0.42.98 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261108-04298`, `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds <build 초>`. payload에 `web/app.js`·`web/styles.css` 호환 파일이 있는지 확인한다(31개). `train-facts`로 package 문서(`docs/ga-ready/trains/0.42.98-admin-smoke.train-facts-input.json`, narrative는 적재 변경 행). 검증: `train-evidence --check`, `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`. host mutation 없음(빌드만). 로컬 commit.

실행 기록(2026-11-08): clean HEAD `54cb61e`에서 빌드했다. product source는 payload `8e4ebe2`와 같고, 그 뒤 commit은 문서뿐이다.

- 빌드: `packaging/windows-desktop-node/installer/build.ps1` `47`초, `build_utc` `2026-11-08T06:52:27Z`, provenance `54cb61ef3128df7d57191aefd0f9a684b8de2f8f`, WiX `5.0.2+aa65968c`, unsigned(`AllowUnsignedDev`/`LocalTest`).
- MSI: SHA-256 `9a26d4cee9c4360cb8a04ce520b57de4d2e99c3e7b64997d8b4567e7c04733b0`.
- payload: `31`개, aggregate `b8caacbba9855907c656ccd761a01052783d5a79f1518d8290e3672688eeee5c`. PR #108의 `web/app.js`·`web/styles.css` 호환 파일이 들어 있다(`0.42.97`의 29개 + 2).
- update ZIP: SHA-256 `c18bc3de426cc031cff4acc939038bd045513e4e401c8b61dbd3adb371777494`(entry 31). catalog SHA-256 `bc634c57c8d57c6d4736f4ba15eaf9ca4482be0fb8746277671da899f7e816ef`.
- `train-facts`: package 문서 `docs/ga-ready/evidence/admin-smoke-package-2026-11-08-04298.md`(`generated=20`, `narrative=3`). `train-evidence --write` 뒤 `--check` 결과는 `current`다.
- 시험: `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests` `24/24`.

host mutation은 없다. operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 2: pair (이관: `train-04297-20261108` Task 4)

- [x] Lane 2. orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261108-04296-04298`, baseline `artifacts/admin-smoke-package-20261108-04296r`, target Task 1 package, base VHD `20348.5761-20261108.vhd`). 여섯 bucket PASS, closed descriptor, `observation_error` 없음. 끝 상태: 설치본 `0.42.98`·ARP 1개·service Running/Automatic·Web 200·보존 VM·template·`pcv-it-s4-console` 그대로. clean-host bucket이 PASS하면 `New-PcvCleanHostBaseVhd.ps1 -SetCurrentBasePath <base> -Execute`(Windows PowerShell 5.1)로 `current-base.json`을 지정한다. `train-facts`로 pair 문서. 로컬 commit.

실행 기록(2026-11-08): orchestrator `-PlanOnly`는 `ok=true`였다(host mutation 없음). `-Execute`는 root `artifacts/manual-admin-campaign-20261108-04296-04298`에서 15:54부터 15:58 KST까지 `268`초 돌았다. 여섯 bucket이 모두 PASS다: `manual-admin-rebaseline-readiness`, `lifecycle-product-update-rollback`, `clean-host-windows-update`, `burn-bootstrapper-lifecycle`, `msix-package-lifecycle-smoke`, `installed-runtime-ops-summary`.

- descriptor: closed descriptor를 냈다(SHA-256 `a90c8b36ee302ecbafd75c6443b753746160e3b035287b9777de0790a985dd57`). observation은 15개이고 `observation_error`는 0이다.
- clean-host: base는 `explicit` UBR `5761`(KB5126655)이고, Windows Update는 재부팅 없이 끝났다. baseline `0.42.96` MSI 설치는 exit 0이었다. 그 설치본의 제품 Update로 `0.42.98`이 됐고(exit 0, BL-0029 수정 확인), Rollback으로 `0.42.96`에 돌아왔다(exit 0). Web은 200이고 blocker는 `none`이다. 시험 VM은 지웠다.
- `current-base.json`: Windows PowerShell 5.1에서 `New-PcvCleanHostBaseVhd.ps1 -SetCurrentBasePath` plan 뒤 `-Execute`로 지정했다(`20348.5761-20261108.vhd`, SHA-256 `c110fc1d…`, `previous_base_file` 없음, `updated_utc` `2026-11-08T07:00:17Z`).
- 끝 상태: 설치본 `0.42.98-admin-smoke`(Host `0.42.98-admin-smoke+54cb61e…`), ARP `{625D101D-…}` `0.42.98` 1개, service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0, `pcv-guest-installed-04253-r1` Off·`pcv-it-s2-source` Off·`pcv-it-s4-console` Running(그대로).
- 문서: `train-facts`로 pair 문서 여섯 개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 썼고, `train-evidence --check`는 일곱 문서 모두 `current`다. `TrainEvidenceGoldenTests`·`TrainFactsBuilderTests`는 `24/24`다. 새 문서의 주소는 loopback과 MSIX version뿐이다.

operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 3: fullgate (이관: `train-04297-20261108` Task 5)

- [x] Lane 2. `pcvverify train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들고(LAN prefix는 환경 변수로만 넘김) supervisor `-DryRun -AllowHostMutation` 뒤 `full-admin-host-mutation-gate-20261108-04298`을 돌린다. 사후 검사: 설치본 Host/CLI ProductVersion == gate build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0, service Running/Automatic, Web 200, 보존 VM·template·`pcv-it-s4-console` 그대로. `train-facts`로 fullgate 문서. 로컬 commit.

실행 기록(2026-11-08): `train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들었다(SHA-256 `8be4bd2eae5d35d0a7e63dfd4926f022c391ca11bbda66528f70a98d4a26b119`). LAN prefix는 직전 manifest artifact가 재클론으로 없어서, S4 상태 보관 때 저장소 밖에 백업한 service 명령줄의 `--prefix`에서 읽어 환경 변수 `PCV_TRAIN_LAN_PREFIX`로만 넘겼다. supervisor `-DryRun -AllowHostMutation`은 두 단계 planned에 `ok=true`였다. 실행 전 ARP는 1개였다.

- 실행: 16:03:13부터 16:09:24 KST까지 `ok=true`, `status=completed`였다. stdout은 `artifacts/batch-manifests/full-admin-host-mutation-gate-20261108-04298.result.json`, stderr는 0 byte다.
- step: `service-msi-hyperv-admin-smoke` exit 0(시도 1, 358.8초), `os-mutation-gate` exit 0(시도 1, 11.1초). executed 2, skipped 0.
- 사후 검사: 설치본 Host/CLI `0.42.98-admin-smoke+2dfe2ac` == gate build HEAD `2dfe2ac`(Task 2 commit, product source는 payload `8e4ebe2`와 같음). ARP `{5673340D-…}` `0.42.98` 1개, PureCVisor 방화벽 규칙 `0`, listener `127.0.0.1:7777`·`127.0.0.1:80`, service Running/Automatic, Web `200`. `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running(그대로).
- 문서: `train-facts`로 fullgate 문서 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-11-08-04298-hostmutation.md`(`generated=35`, `narrative=2`)를 썼다. `train-evidence --check`는 여덟 문서 모두 `current`이고, 시험은 `24/24`다. 문서에는 LAN 주소가 없다(`[redacted-private-endpoint]`).

operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 4: installed current-card (이관: `train-04297-20261108` Task 6)

- [x] Lane 2. `train-host-inputs --kind current-card`로 capture 스크립트를 만들어 root 밖에서 실행한다. `status=pass`, `promotion_ledger_status=not-promoted`, 남은 시험 VM 0이어야 한다(`pcv-it-s2-source`는 S2 template, `pcv-it-s4-console`은 S4 시연 VM으로 기록. 도구가 받지 않으면 멈추고 묻는다). secret 없음. `train-facts`로 current-card 문서. 로컬 commit.

실행 기록(2026-11-08): `train-host-inputs --kind current-card --write`가 렌더된 fullgate facts에서 capture 스크립트(SHA-256 `0a534c064515bc0a5f3ed802a32c527742097b0827eb99927265e24853d818fe`)를 만들었다. 저장소 root(card root 밖)에서 `2026-11-08T07:11:03Z`에 `7`초 실행했다.

- 결과: `status=pass`, `promotion_ledger_status=not-promoted`, `canonical_current_changed=false`, secret 없음, host mutation 없음.
- 설치본: `0.42.98-admin-smoke+2dfe2ac`, ARP 1, CLI 3/3(JSON 3/3), Web 2, web config token 없음, service Running/Auto. payload와 설치본 CLI·Host가 일치한다.
- 남은 시험 VM: 0이다. non-spike VM은 보존 VM `pcv-guest-installed-04253-r1`, S2 template `pcv-it-s2-source`, S4 시연 VM `pcv-it-s4-console`이다.
- 스크립트를 root에 `capture-current-card.ps1`로 복사했다(summary SHA-256 `86a9cf60ce544851e0a44ad7d12ecffb57bb54ff8daa7fb459a4e2ca37325197`).
- 문서: `train-facts` current-card `generated=31`, `narrative=1`. `--check`는 아홉 문서 모두 `current`이고, Verification 시험(golden, facts, host-inputs)은 `29/29`다.

operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 5: probe Web Console 재설계 (queue 97~103, 105~108; 이관: `train-04297-20261108` Task 7)

- [x] Lane 2. 설치본 `0.42.98`의 `http://127.0.0.1/`를 loopback 세션으로 브라우저(CDP)에서 연다. 다음을 본다.
  - 사이드바 화면 전환, 템플릿·계정 화면, VM 목록 상태 칩
  - 옛 주소: `/index.legacy.html` 404. `/app.js`·`/styles.css`는 PR #108의 호환 파일이라 주석만 담고 200이다.
  - 새 VM 마법사: `pcv-it-probe-wizard-1108`(Generation 2, smoke ISO, disk `artifacts/` 아래)을 만들고 켠다.
  - 콘솔 탭: 맞춤, 전체 화면 버튼, 입력 상태 칩
  - 체크포인트 탭: 생성, 복원 확인 모달, 예약 미리보기·저장 뒤 즉시 반영, 예약 지우기
  - 더 보기 메뉴의 복제 모달: 이름 첫 칸, 이름 `pcv-it-probe-clone-1108`
  - 삭제 이름 확인 모달로 두 VM을 지운다.

  캡처는 `docs/ga-ready/demo/web-console-redesign-20261108/`(PNG, 사용자 홈 경로·LAN IP·호스트명·token 없음), 기록은 `docs/ga-ready/demo/web-console-redesign-demo-2026-11-08.md`(확인자 칸 비움)다. 끝 상태 `pcv-it-probe-*` 0개. 로컬 commit.

실행 기록(2026-11-08): grok이 세션 작업 폴더에 일회용 Node CDP 스크립트 `probe-web-console-04298.mjs`를 쓰고 설치본에 반복 실행했다(시스템 Edge, 새 npm 의존성 없음, 저장소에 넣지 않음). 마지막 실행은 17:26:57부터 17:33:07 KST까지 `ok=true`, 종료 코드 0이다. Claude가 결과 JSON, 캡처 16장, 스크립트의 금지 동작을 검토했다.

- 여덟 단계가 PASS다. 셸 10 화면과 상태 칩, 옛 주소 `/index.legacy.html` 404와 호환 파일 `/app.js`(246 byte)·`/styles.css`(249 byte) 200 주석만, 마법사 4단계로 `pcv-it-probe-wizard-1108` 생성(Gen2, 1 vCPU, 512 MB, 8 GB, smoke ISO, `artifacts/` 아래) 뒤 시작, 콘솔 탭 맞춤·전체 화면·입력 칩, 체크포인트 생성·복원 확인·예약 미리보기·저장 즉시 반영·끄기, 복제 모달 첫 칸 이름과 `pcv-it-probe-clone-1108` 복제, 이름 확인 모달로 두 VM 삭제, API에 `pcv-it-probe-*` 0개다.
- 실행 중에 `running`/`queued`로 기록된 job 다섯 개(`vm.create`, `vm.poweroff` 두 건, `checkpoint.delete`, `vm.delete`)는 끝난 뒤 `GET /api/v1/jobs/<id>`로 다시 읽었고 모두 `succeeded`다.
- 스크립트가 피해 간 동작 셋을 backlog에 적었다: `BL-0032`(예약 미리보기가 입력을 비워 바로 저장하면 `PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID`), `BL-0033`(켜진 VM 복원이 원시 `PCV_HYPERV_WMI_METHOD_FAILED`), `BL-0034`(VM 목록 갱신 `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE` 때 지운 행이 남음). 셋 다 이 train의 회귀가 아니다. 예약 form은 `bb54287`부터 같은 구조이고 `0.42.96` payload `87a7deb`에도 있다. 적재 payload `87a7deb..8e4ebe2`에 `src/DesktopNode.HyperV` 변경이 없다. 화면 다듬기 거리는 `BL-0031`이다. 그래서 train을 세우지 않는다.
- 캡처: 1600×1000으로 찍고 500 KB 넘는 여덟 장을 줄였다. 찍기 전 치환은 07~16에서 장마다 1곳이다. Claude가 열어 보고 사용자 홈 경로·LAN IP·호스트명·token이 없는 것을 확인했다.
- 끝 상태: `Get-VM`은 보존 VM 세 개(`pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running)뿐이고 probe 폴더 둘은 지워졌다. service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0이다.
- 예산: Lane 2 checkpoint 45분을 넘겼다(16:12~17:37 KST, 약 85분). grok이 스크립트를 쓰고 고치며 반복 실행한 시간이다. 실행 중에 끊으면 probe VM이 남아서 끝까지 기다렸다. 권장안 자동 승인(사용자 결정 5)으로 기록하고 Task 6을 새 checkpoint로 잇는다.

operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 6: probe 비밀번호 재설정 (queue 104; 이관: `train-04297-20261108` Task 8)

- [x] Lane 2. 설치본 `0.42.98` loopback에서 계정이 없는 동안 Local API·PCVCLI의 재설정 route가 인증 없이 거절되는지 확인한다. 그다음 Task 7 계정 복원 뒤, 에이전트용 operator 계정 `pcv-s4-operator`로만 본인 재설정 경로를 돈다: 로그인 token을 받고, 본인 재설정 뒤 옛 token이 `401`, 새 비밀번호 로그인이 성공하는지 본다. 에이전트가 그 계정의 현재 비밀번호를 갖고 있지 않으면 이 단계는 하지 않고 Task 12 사용자 시연 항목으로 넘긴다(admin `pcv-admin2` 비밀번호는 사용자 전용이다). 비밀번호와 token은 실행 경계에서 만들고 남기지 않는다. evidence `docs/ga-ready/evidence/lane2-account-password-reset-2026-11-08-04298.md`. 로컬 commit.

실행 기록(2026-11-08): 17:38 KST에 계정 0개(`bootstrap_state=no-default-account`), loopback 전용 설치본 `0.42.98-admin-smoke+2dfe2ac`에서 일회용 스크립트로 열두 호출을 돌렸다. 비밀번호는 임의 일회용 값, loopback token은 메모리와 `--token-env`로만 다뤘고 출력은 상태·오류 코드뿐이다.

- API: reset은 인증 없음 `401` `PCV_AUTH_REQUIRED`, 무효 bearer `403` `PCV_AUTH_FORBIDDEN`, loopback 세션 `409` `PCV_ACCOUNT_BOOTSTRAP_NOT_AVAILABLE`. change는 `401`, `403`, loopback 세션 `403` `PCV_ACCOUNT_PASSWORD_CHANGE_FORBIDDEN`.
- PCVCLI: 무효 token과 loopback 세션은 같은 코드로 exit `1`. argv `--password`는 `PCV_CLI_USAGE`, `--yes` 없음은 `PCV_CLI_CONFIRMATION_REQUIRED`로 exit `2`(API 호출 전).
- 응답에 token 없음, `account-mutation-audit.jsonl` 실행 전후 없음, 실행 뒤 계정 0개. host mutation과 계정 변경은 없다.
- 둘째 단계(`pcv-s4-operator` 본인 재설정)는 에이전트가 현재 비밀번호를 갖고 있지 않아 하지 않았다. 계획대로 Task 12 사용자 시연 항목으로 넘긴다. service bearer로 비밀번호를 바꾸는 것은 계획 문장 밖이라 하지 않았다.
- report-only: loopback 세션 reset의 거절 코드가 bootstrap 이름이다(계정 생성과 같은 `RejectReadyAuth` 경로). 거절은 맞다.
- evidence: `docs/ga-ready/evidence/lane2-account-password-reset-2026-11-08-04298.md`.

operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 7: S4 복원과 LAN 재적용 (queue 94, 95; 이관: `train-04297-20261108` Task 9)

- [x] Lane 2. train `0.42.97` Task 2 백업의 `accounts.json`을 되돌리고 해시를 비교한다(data root의 빈 계정 저장소를 대체). 설치본 `0.42.98`의 `configure-installed`를 백업한 LAN prefix와 `--allow-lan`으로 실행한다(PR #94: prefix 옵션과 `--allow-lan`이 service 명령줄에 들어감, PR #95: 실행 중 service가 Stop → Configure → Start로 새 listener를 연다). 확인할 것:
  - 명령줄
  - listener 7777·80 LAN 주소
  - 방화벽 관리 규칙 2개(Private, Inbound)
  - loopback 세션 `409`
  - 계정 3개(`pcv-admin` disabled, `pcv-admin2`·`pcv-s4-operator` enabled)
  - Web 200(LAN 주소)

  evidence `docs/ga-ready/evidence/lane2-service-configure-lan-2026-11-08-04298.md`(LAN 주소는 `<host-lan-ip>`). 로컬 commit.

실행 기록(2026-11-08): 17:41~17:43 KST에 일회용 스크립트(출력의 사설 IPv4와 사용자 경로를 가림)로 했다.

- 계정: data root에는 fullgate 재설치가 만든 빈 계정 저장소(0개)가 있어서 백업 폴더로 옮기고 백업 `accounts.json`을 넣었다. SHA-256이 백업과 같다. 파일 ACL은 제품이 빈 저장소에 둔 것(상속 끊음, SYSTEM·Administrators 읽기만)과 같게 맞췄다.
- 설치본 `DesktopNode.Host.exe service-action configure-installed`를 MSI custom action 경로 인자, 지금의 `--batch-evidence-root`, 백업 명령줄의 `--prefix`·`--web-prefix`(LAN), `--allow-lan`으로 실행해 exit `0`이었다. 실행 중 service가 멈췄다가 새 명령줄로 켜졌다(PR #95).
- `service-action firewall-enable` API 규칙과 Web 규칙(`--firewall-local-port 80`, `--firewall-rule-name "PureCVisor Desktop Node Web LAN"`) 둘 다 exit `0`. 앞선 두 시도는 거절만 됐다(경로 인자 없음 `PCV_HOST_PRODUCT_ROOT_REQUIRED`, `--allow-lan` 없음 `PCV_HOST_FIREWALL_LAN_APPROVAL_REQUIRED`). 규칙도 LAN 노출 명시 승인을 요구한다.
- 확인: service Running/Automatic. 명령줄은 LAN prefix 둘과 `--allow-lan`이고, 백업한 S4 명령줄과 `--batch-evidence-root`만 다르다(MSI 재설치 값). listener는 `<host-lan-ip>:7777`·`:80`(HTTP.sys)뿐이고 `127.0.0.1`은 듣지 않는다. 방화벽 규칙 2개(TCP 7777·80, Inbound, Allow, Private, LocalSubnet). 계정 3개(`pcv-admin` disabled, `pcv-admin2`·`pcv-s4-operator` enabled). Web `http://<host-lan-ip>/` 200. 보존 VM 셋 그대로.
- loopback 세션은 LAN 주소로 `403` `PCV_LOOPBACK_SESSION_NOT_LOOPBACK`이었다. 계획의 `409`는 loopback 주소로 들어온 요청의 코드이고, 지금은 loopback listener가 없어 요청이 LAN 주소로 들어온다. LAN 모드에서 loopback 세션이 없다는 성질은 같다.
- evidence: `docs/ga-ready/evidence/lane2-service-configure-lan-2026-11-08-04298.md`.

operational current는 `0.42.96-admin-smoke` 그대로다.

## Task 8: train PR A (이관: `train-04297-20261108` Task 10)

- [x] clean HEAD 종료 검증을 한다: `dotnet build src/DesktopNode.sln -c Release`, `Invoke-PcvPrGate.ps1` 또는 Required CI shard 4개 로컬, Pester 4종, `npm run test:public-source-safety --prefix web` 신규 0, `train-path-check --payload 8e4ebe2 --head HEAD` exit `0`. 그다음 push, PR. green CI 뒤 사용자 merge 문장으로 merge한다(`--match-head-commit`). merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

실행 기록(2026-11-08): clean HEAD `641bf4b`에서 종료 검증을 했다.
- `train-path-check --payload 8e4ebe2 --head HEAD` `ok=true`: 바뀐 경로 37개, `product_paths`·`allowed_pin_paths` 없음
- `dotnet build src/DesktopNode.sln -c Release` 오류 0
- PR gate `artifacts/pr-gate/20261108-084449` `ok=true`: build, `dotnet-test-release`, module-size-ratchet 3/3
- Pester: packaging 527 통과·1 실패(기존 BL-0025 `Test-Json`), installer 49, web 50, manual-admin 137
- `Update-PcvCurrentEvidenceDocs.ps1 -Check` current. public-source-safety는 기존 BL-0015 두 건 말고 신규 0

PR #109(`lane1/train-04298-20261108`)를 열었고 Required CI 다섯 개(`dotnet`, `web`, `delivery`, `installer-policy`, `public-boundary-ci-required`)가 pass였다. 사용자 문장 "PR #109 merge 승인" 뒤 base가 `origin/main`과 같은 것을 확인하고 `--match-head-commit 641bf4b`로 merge했다(merge commit `4e8562f`). main push run은 Public Boundary `42467121432`, Development Gates `42467121444`이고 둘 다 success다.

## Task 9: Lane 3 데이터와 lane3-spec (이관: `train-04297-20261108` Task 11)

- [x] Lane 3. `main`에서 branch를 만든다. consume manifest, functional carry-forward·pair consume·`main-push`(PR A merge의 main push run 인용) facts를 쓴다. 그 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`. `current-evidence.json`과 `lane3-spec --write`를 쓴다. `index_sections.previous_version`은 직전 승격 `0.42.96-admin-smoke`다(§10, 정차 train `0.42.97`을 건너뜀). 로컬 commit.

실행 기록(2026-11-08): `main` `4e8562f`에서 branch `lane3/train-04298-promotion-20261108`를 만들었다.
- consume manifest: `artifacts/manual-admin-campaign-20261108-04296-04298/consume-manifest.json`을 새로 썼다. closed descriptor가 읽은 summary 7개의 SHA-256과 byte 수를 담고, 파일은 복사하지 않았다.
- facts: functional carry-forward(04296 다음, payload 변경 14 commit), pair consume, 두 PR용 `main-push`를 더했다. `main-push`는 PR #109 merge `4e8562f`, Public Boundary `42467121432` job `128689088875`, Development Gates `42467121444`이고, 첫 부모 `8e4ebe2`와 비교해 `src`·`config`·`web/src`·`.github` 변경이 없다. current-card는 `promoted-current`로 바꿨다.
- `train-evidence --write --allow-update <current-card>` 결과 새 문서 3개, current-card updated. `--check`는 열두 문서 모두 current다.
- `current-evidence.json` current는 `0.42.98-admin-smoke`다. provenance `2dfe2ac`, clean MSI `9a26d4ce…`, operational MSI `0a42b89f…`, payload `963f4f74…`, manual_admin은 `0.42.96 -> 0.42.98`(`manual-admin-campaign-20261108-04296-04298-closed`)이다.
- `lane3-spec --write`로 `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04298.json`을 만들었다. `index_sections.previous_version=0.42.96-admin-smoke`다.

`release-train.json` 승격은 Task 10이 한다.

## Task 10: 승격 문서와 release-train 승격 (이관: `train-04297-20261108` Task 12)

- [x] Lane 3. `Invoke-PcvLane3PromotionDocs.ps1` dry-run, `-Apply`, `-Check`와 Lane 3 숨은 단계(descriptor chain, ledger 행, index 절, D2 시험, spec pin, current evidence 시험 version 줄)를 한다. `release-train.json` train `0.42.98-admin-smoke` `status=promoted`, `operational_current=0.42.98-admin-smoke`. 그다음 `Update-PcvCurrentEvidenceDocs.ps1 -Apply`/`-Check`, `Update-PcvContractSpecPins.ps1 -Apply`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

실행 기록(2026-11-08): promotion 도구와 수기 갱신을 했다.
- `Invoke-PcvLane3PromotionDocs.ps1 -SpecPath packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04298.json`
  - dry-run `ok`: current evidence 문서 stale, 나머지는 planned
  - `-Apply` `ok`: current evidence 문서, descriptor chain, ledger head·rows, index 절, spec pin 갱신
  - `-Check`: 모든 단계 current
- 숨은 단계: config spec pin 3개와 Delivery verifier 상수 3개는 도구가 갱신했다. current evidence 시험은 version 정규식이라 손댈 줄이 없다.
- `release-train.json`: train `0.42.98-admin-smoke` `status=promoted`, `operational_current=0.42.98-admin-smoke`
- 수기 갱신:
  - `docs/DOCUMENTATION_INDEX.md` 권위 줄 넷(payload `8e4ebe2`, 운영 `0.42.98-admin-smoke`, 최신 `main` `4e8562f`, 설치본과 S4 LAN 모드)
  - `docs/FEATURE_IMPLEMENTATION_LEDGER.md` operational 줄 둘
  - `docs/DEVELOPER_INDEX.md` train `0.42.97`·`0.42.98` 줄
- 검증:
  - `Update-PcvContractSpecPins.ps1 -Check` current(spec 9개)
  - `Update-PcvCurrentEvidenceDocs.ps1 -Check` ok
  - `dotnet test src/DesktopNode.Delivery.Tests -c Release` 781/781
  - `DesktopNode.Verification.Tests` 678/679: dirty tree의 PolicyBoundary 1건만 실패, commit 뒤 Task 11에서 다시 본다

## Task 11: Lane 3 PR B (이관: `train-04297-20261108` Task 13)

- [ ] clean HEAD 종료 검증(Task 8과 같음). push, PR. green CI 뒤 사용자 merge 문장으로 merge한다. merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

## Task 12: S4 사용자 시연 A (이관: `train-04297-20261108` Task 14, 원래 `ui-redesign-20261105` Task 11)

- [ ] 원래 문장: Task 4 전에 만든 admin 계정으로 자기 PC 브라우저에서 S1~S3을 시연한다. 에이전트는 시연 전후 상태(service 구성, 방화벽 규칙, 계정 목록, VM)를 읽어 기록하고 확인자 칸은 사용자가 채운다. 이 기록만 "다른 PC" 시연이다. 리허설 R은 S4 판정 근거로 쓰지 않는다. 2026-11-08 변경: 시연은 설치본 `0.42.98`의 새 화면으로 이어 간다(S2 복제 이름은 복제 모달 첫 칸). 기록은 `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md`이고 초안은 로컬 branch `lane1/s4-demo-record-20261108` `cf2b2f6`에 있다. 사용자가 시연 결과를 알리면 처리한다.

추가(2026-11-08, Task 6): 비밀번호 재설정과 변경(PR #104)의 성공 경로를 시연에 넣는다. admin이 Web 계정 화면에서 `pcv-s4-operator` 비밀번호를 재설정하고, 그 계정의 옛 세션이 끊기는지(`401`), 새 비밀번호로 로그인되는지, 로그인한 사용자가 상단의 "비밀번호 변경"으로 자기 비밀번호를 바꿀 수 있는지 본다. 비밀번호는 사용자가 정하고 기록하지 않는다.

## Task 13: S4 끝 상태 (이관: `train-04297-20261108` Task 15, 원래 `ui-redesign-20261105` Task 12)

- [ ] Lane 2. 원래 문장: 끝 상태는 loopback 복귀(같은 `main` 빌드본 `configure-installed`를 prefix 옵션과 `--allow-lan` 없이 실행, Task 4 전 service 명령줄과 같아야 한다), 방화벽 규칙 0, 에이전트 operator 계정 disable, `pcv-it-s4-*` VM 정리다. 기록한다. 계정 파일 삭제는 이 task가 하지 않는다. 2026-11-08 메모: 설치본 `0.42.98`의 `configure-installed`가 PR #94·#95를 담는다.

## Task 14: criteria S4 판정 (이관: `train-04297-20261108` Task 16, 원래 `ui-redesign-20261105` Task 13)

- [ ] Lane 1. 원래 문장: 시연 기록이 설계 1절의 판정을 모두 담으면 `config/project-completion-criteria.json` S4를 `passed`로 두고 시연 기록 링크를 적는다. push, PR, 사용자 merge 승인 뒤 merge. 판정 전에는 S4 통과를 주장하지 않는다. 보관한 계정 백업을 지울지 사용자에게 묻는다.

## Task 15: C5 runner 확인 (이관: `train-04297-20261108` Task 17, 원래 `ui-redesign-20261105` Task 14)

- [ ] 원래 문장: `not_before` 2026-10-19. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR. merge는 사용자 승인 뒤.

## Nonclaims

- operational current는 Task 11의 Lane 3 PR B가 `main`에 merge된 뒤에만 `0.42.98-admin-smoke`로 바뀐다. 그 전 모든 task 기록은 `0.42.96-admin-smoke` 그대로다.
- train `0.42.97`은 정차했고 승격하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다. package는 `AllowUnsignedDev`/`LocalTest`다.
- S4 통과는 Task 14 판정 전에는 주장하지 않는다. 재빌드한 baseline package는 원래 `0.42.96` 승격 evidence를 대신하지 않는다.
