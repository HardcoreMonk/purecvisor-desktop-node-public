# Release train `0.42.91`과 완료 probe Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** PR #53(Hyper-V Notes 원소 하나 쓰기)을 실은 `0.42.91-admin-smoke`를 package부터 Lane 3까지 돌려 operational current로 승격한다. 실은 변경의 Lane 2 probe를 겸해 완료 정의 C4의 남은 항목(P1-6, P1-7, P1-9, P2-11, P1-10)을 `0.42.91` 설치본에서 확인하고, 2026-10-19 뒤 C5를 확인해 프로젝트 완료를 판정한다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 train 순서다. pair는 orchestrator, evidence 값은 `pcvverify train-facts`, Lane 3 spec은 `pcvverify lane3-spec`, Lane 3 consume은 orchestrator의 `<campaign>-closed` descriptor다. branch는 `train/04291-20261006`(`origin/main` `05f42a2` 기준)이고, pair evidence PR을 먼저 merge한 뒤 Lane 3를 그 위의 branch에서 PR 하나로 merge한다. 완료 probe 설계는 `docs/superpowers/plans/2026-10-06-purecvisor-desktop-node-completion-probes.md`(0.42.90에서 Task 1 정차)를 이어 받는다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, current-card capture, 설치본 `pcvcli`, `pcvverify train-facts`/`train-evidence`/`lane3-spec`, `Invoke-PcvLane3PromotionDocs.ps1`

## 출발 승인 (2026-10-06)

승인 원문: `1,2` (campaign `completion-probes-20261006` 정차 보고의 다음 승인 2). 앞선 승인 `1,2,3`(2026-10-06, 완료 probe와 C5)의 probe 범위와 C5 확인을 이어 받는다.

| 항목 | 범위 |
| --- | --- |
| train | release train `0.42.91-admin-smoke` 출발. queue 행 PR #53 고정, package build, pair host mutation(제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX), fullgate, current-card, Lane 3 `current-evidence.json` 쓰기, push, PR, green CI 뒤 merge |
| probe | `0.42.91` 설치본에서 완료 probe 재실행. P1-6·P1-7(probe VM 생성·삭제), P1-9(probe 계정 생성·비활성화), P2-11(noVNC target 설정 뒤 원복), P1-10(probe VM 생성·삭제와 서비스 재시작) |
| C5 | 2026-10-19 뒤 Ubuntu 26 runner에서 첫 `main` run 확인(Lane 0/1). 실패하면 `ubuntu-24.04` pin 판단 |

정차하면 이 승인은 끝난다. 정기 출발일(직전 출발 2026-10-05의 7일 뒤) 전이므로 사용자 요청에 의한 출발이다. public trusted signing과 external stable publication은 하지 않는다.

## 적재 변경

| PR | 변경 commit | 요약 | Lane 2 probe |
| --- | --- | --- | --- |
| #53 | `c8be5bc` | Hyper-V Notes를 원소 하나로 써서 template lock marker가 저장되게 함(vm.manage 잠재 결함 포함) | `vm.template.lock` (Task 6) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner `pcv-cleanhost-*`, fullgate route smoke VM, probe VM `pcv-probe-c4-*`뿐이고 끝나면 지운다.
- 계정이 하나도 없거나 enabled admin이 probe 계정뿐이 되면 멈춘다. probe 계정 비밀번호는 실행 경계에서 무작위로 만들어 환경 변수로만 넘긴다. noVNC target은 시작 상태(파일 없음)로 되돌린다. LAN target을 쓰지 않는다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. guest 인증 정보는 clean-host runner 기본값을 실행 스크립트가 runner 소스에서 읽어 만든다.
- evidence와 artifact는 새 이름(`-04291`, `-04290-04291`)으로 쓴다. 이전 train의 evidence, facts, artifact를 바꾸지 않는다.
- baseline package는 `artifacts/admin-smoke-package-20261005-04290`(MSI, payload, update ZIP, catalog)다.
- orchestrator 입력은 train `0.42.90`과 같다(base VHD `20348.5622-20260930.vhd`, `Default Switch`, MSIX template, 설치 manifest, 서명 thumbprint `8C5F…`). ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 0: 출발

- [x] `release-train.json`의 queue 행(PR #53)을 train `0.42.91-admin-smoke`의 `carriages`로 옮기고 `status=running`. 이 계획과 campaign.

실행 기록(2026-10-06): 출발 `2026-10-06T19:40:00+09:00`, source `05f42a2`, carriages `[53]`, queue 비움.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.91-admin-smoke -MsiProductVersion 0.42.91 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261006-04291`, `New-PcvAdminSmokeUpdatePackage.ps1`. train-facts 입력과 package 문서(`build_utc` ISO 8601 확인). 로컬 commit.

실행 기록(2026-10-06): HEAD `59cd8b6`에서 build `42`초, MSI `bdef7609…`, payload `f4503dfc…`, update ZIP `f31ae34c…`, Host/CLI `+59cd8b6`. `build_utc`는 `2026-10-06T10:39:40.0750092Z`(PR #51 수정 확인). 도구에 `-BuildSeconds 42`를 넘겨 facts가 `build_seconds`를 직접 만들므로 첫 `train-facts`가 서술 값과 겹쳐 `narrative-conflict:package:build_seconds`로 멈췄다. 서술에서 빼고 다시 돌려 package 문서 `written`, `check` `current`. evidence `admin-smoke-package-2026-10-06-04291`.

## Task 2: pair 실행

- [x] orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261006-04290-04291`). 여섯 bucket PASS, closed descriptor, `observation_error` 없음. 로컬 commit.

실행 기록(2026-10-06): 시작 상태 설치본 `0.42.90`(`+648139d`). `-PlanOnly` `ok=true` 뒤 `-Execute`(`10:40:59Z`~`10:45:20Z`, 약 `261`초). baseline `0.42.90` 정렬 뒤 여섯 bucket 모두 PASS, closed descriptor, restoration 불필요. `observations.json` `15`개 모두 `observation_error` 없음. 끝 상태: 설치본 clean `0.42.91`(`+59cd8b6`), ARP `{AC7292AD-3E1D-4512-BF63-996F831EF90B}` `0.42.91` 1개, service Running/Automatic, Web `200`, 보존 VM Off. artifact `artifacts/manual-admin-campaign-20261006-04290-04291`.

## Task 3: pair 문서

- [x] `train-facts`에 pair 문서 `6`개를 더해 렌더한다. 로컬 commit.

실행 기록(2026-10-06): 생성 값 ops-summary `10`, update-rollback `23`, clean-host `21`, burn `16`, msix `17`, pair-descriptor `17`, 사람 값 `0`. baseline package 출처가 들어가자 package `zip_note`에 0.42.90 ZIP과의 항목 구성 비교가 붙어 Task 1 문서가 달라졌으므로 `--allow-update`로 package 문서를 갱신했다. `check` `current`(`7`개).

## Task 4: fullgate

- [x] clean `0.42.91` 위에서 `full-admin-host-mutation-gate-20261006-04291`. 사후 검사(build commit, 같은 version ARP 1개, firewall 규칙 0). 로컬 commit.

실행 기록(2026-10-06): pair가 남긴 clean `0.42.91`(`{AC7292AD-…}`) 위에서 시작했다. 0.42.90 manifest에서 version, batch id, 경로만 바꿨고 supervisor `-DryRun -AllowHostMutation` 뒤 `10:47:05Z`~`11:00:40Z` 실행, `ok=true`, `status=completed`. step: service-msi-hyperv exit `0`(시도 `1`, `802.8`초), os-mutation exit `0`(`11.1`초). 사후 검사: 설치본 Host/CLI `+990a4b2`가 gate build(clean HEAD `990a4b2`)와 같다. 같은 version ARP `{ED64B13A-742B-422C-9142-DED650CB856B}` `0.42.91` 1개, service Running/Automatic, Web `200`, PureCVisor firewall 규칙 `0`, 보존 VM Off.

## Task 5: installed current-card

- [x] 0.42.90 스크립트에서 root, evidence id, fullgate batch, SHA 상수 `4`개, 40자리 `provenance_commit`, manifest version을 바꿔 root 밖에서 실행. `status=pass`, `not-promoted`. 로컬 commit.

실행 기록(2026-10-06, `11:01:36Z`): 0.42.90 스크립트에서 root, evidence id, fullgate batch, SHA 상수(clean MSI `bdef7609…`, fullgate MSI `46dddccb…`, clean payload `f4503dfc…`, fullgate payload `c79dff87…`), `provenance_commit` `990a4b2`, manifest version, `canonical_current_evidence` `0.42.90-admin-smoke`를 바꿔 root 밖에서 실행했다(§10 절차, 한 번에 맞음). `status=pass`, CLI `3/3`, Web `2/2`, ARP 1개, 설치본 Host/CLI가 fullgate payload와 같음, 테스트 VM `0`, secret 없음, `not-promoted`. summary SHA `0b49c396…`.

## Task 6: probe P1-6, P1-7 (실은 변경 `vm.template.lock`)

- [x] 0.42.90 probe 스크립트로 probe VM의 `created_at`·`last_powered_on`·`notes` readback, template-lock 뒤 `template_lock=true`와 Notes marker, rename·set-memory·poweroff 거절(`PCV_VM_TEMPLATE_LOCKED`)과 start 허용, unlock 뒤 허용, 삭제를 확인한다. 로컬 commit.

실행 기록(2026-10-06, `11:02:14Z`~`11:02:58Z`, 설치본 `0.42.91+990a4b2`): `16`단계 모두 PASS. P1-6: `created_at` 있음, `last_powered_on`은 running에서만, `notes`는 marker 줄을 뺌(보존 VM 운영자 메모 readback 포함). P1-7: lock 뒤 `template_lock=true`, rename·set-memory·poweroff는 `PCV_VM_TEMPLATE_LOCKED`로 거절, start는 허용, unlock 뒤 poweroff·rename 허용. 0.42.90에서 FAIL했던 P1-7이 PR #53 수정으로 닫혔다. 정리 단계가 이름이 바뀐 probe VM도 지우게 고쳤다. probe VM `0`, 보존 VM Off, service Running/Automatic, Web `200`. evidence `lane2-completion-inventory-template-lock-actual-vm-2026-10-06-04291`(artifact `artifacts/lane2-completion-inventory-template-lock-20261006-04291`).

## Task 7: probe P1-9, P2-11

- [x] 계정 목록 확인(enabled admin이 probe 밖에 있어야 함), probe 계정 `pcv-probe-c4-operator` 생성·목록·disable. noVNC target loopback preview·set·capabilities readback·clear로 원복(파일 없음). 로컬 commit.

실행 기록(2026-10-06): P1-9는 계정 목록이 비어 있었다(`accounts: []`, `bootstrap_state: no-default-account`, `artifacts/lane2-completion-account-novnc-target-20261006-04291/01-account-list.json`). 첫 계정은 admin이 되고 마지막 enabled admin은 비활성화가 거절되므로 안전장치가 계정을 만들기 전에 멈췄다. 사용자 결정(2026-10-06): P1-9는 설치본 evidence 불필요로 닫는다(계정 없는 bootstrap이 의도된 기본값이고 probe가 인증 상태를 바꾸므로). P2-11은 `11:05:08Z`~`11:05:13Z` PASS: preview는 파일을 만들지 않음, set이 `127.0.0.1:5901`을 저장, clear가 `enabled=false`를 남김, 그 파일을 지워 시작 상태(파일 없음, PathName에 noVNC 인자 없음)로 원복, service Running/Automatic, Web `200`. evidence `lane2-completion-novnc-target-2026-10-06-04291`.

## Task 8: probe P1-10

- [x] probe VM으로 `vm.create`, `vm.shutdown`, `vm.restart`, QoS mutation job을 진행 중에 서비스 프로세스를 끊어 `PCV_JOB_INTERRUPTED`로 만들고 `job reconcile` 판정을 확인한다. probe VM 삭제, service 상태 확인. 로컬 commit.

실행 기록(2026-10-06): 첫 checkpoint는 같은 원인 두 번과 Lane 2 예산으로 멈췄다. r1은 스크립트 결함(빈 `vm list`가 `$null`로 풀림)으로 멈췄고, r2·r3에서는 끊긴 `vm.create`가 VM 폴더에 `disk0.vhdx`(4 MB)를 남겨 다음 create가 `PCV_VHD_ALREADY_EXISTS`로 막혔다(정리 경로 결함 포함). r1과 r3의 끊긴 create는 reconcile이 `not-applied`(VM 없음)로 판정했다. 사용자 승인 `1,2`(2026-10-06)로 새 checkpoint에서 정리 경로를 `Join-Path`로 고쳐 r4(`11:49:33Z`~`11:50:21Z`)를 돌렸다. `vm.create`, `vm.restart`, `vm.shutdown`, `vm.qos.storage.set`, `vm.qos.network.set` 다섯 family 모두 첫 시도에서 running 중 서비스 프로세스 종료로 `PCV_JOB_INTERRUPTED`가 되었고, `job reconcile`이 `PCV_JOB_RECONCILIATION_REQUIRED` `Classification: not-applied`를 냈다. 다섯 판정 모두 독립 readback(VM 없음, `last_powered_on` 그대로, Running, QoS 값 없음)과 일치한다(measured `5`, fail `0`). 이번 probe는 모두 적용 전에 끊긴 not-applied 경로이고, confirmed 경로는 0.42.84 전원 상태 reconcile 설치본 evidence와 family별 code-level 시험이 덮는다. 끝 상태: probe VM·폴더 `0`, 보존 VM Off, service Running/Automatic, Web `200`. report-only: 끊긴 create의 고아 디스크(Task 13 설계), QoS readback `mutation_supported: false`. evidence `lane2-completion-family-reconcile-actual-vm-2026-10-06-04291`(artifact `-r4`, 이전 시도 r1~r3 보존).

## Task 9: 문서와 pair evidence merge

- [x] `train-facts`에 fullgate, current-card 문서를 더해 렌더한다. probe evidence `3`개와 `FEATURE_IMPLEMENTATION_LEDGER` 비후보 관측 행, 완료 정의 §4 C4 보정을 쓴다. clean HEAD 종료 검증 뒤 push, PR, green CI 뒤 merge.

실행 기록(2026-10-06, clean HEAD `804c772`): fullgate(생성 `35`, 사람 `2`)와 current-card(생성 `31`, 사람 `1`) 문서를 렌더해 pair 문서 `9`개가 모두 facts에서 나왔다. probe evidence `3`개, `FEATURE_IMPLEMENTATION_LEDGER` 비후보 관측 행 `4`개, 완료 정의 §4 C4 보정(C4 충족), `DOCUMENTATION_INDEX` 호스트 줄 `+990a4b2`. Release build 경고 `0`, `dotnet test src/DesktopNode.sln -c Release` 실패 `0`(Verification `614`, Delivery `763`, Api `488`, HyperV `257`, Host `216`, Contracts `200`, Cli `183`, Runtime `129`, Service `11`), Pester 네 종 실패 `0`, `npm run test:required --prefix web` exit `0`, Required CI 네 shard `ok=true`·`plan_only=false`. 이 기록 commit 뒤 push, PR, green CI 뒤 merge한다.

## Task 10: Lane 3

- [x] functional carry-forward, consume(`…-closed` descriptor와 consume manifest), Task 9 merge의 main push evidence, current-card `promoted-current`. `current-evidence.json`, `lane3-spec` 입력과 spec `04291`, 문서 도구 dry-run·`-Apply`·`-Check`, `release-train.json` 승격, 수기 정렬. 로컬 commit.

실행 기록(2026-10-06): branch `lane3/04291-promotion-20261006`(`origin/main` `4b04653`, PR #54 merge 기준). Lane 3 문서 `3`개를 facts로 렌더했다: functional carry-forward(0.42.75 PASS, ledger 마지막 변경 `bb15f66`), consume(`manual-admin-campaign-20261006-04290-04291-closed`, consume manifest `e6c7db31…`, 복사 없음), main push(`4b04653`, Public Boundary run `37460495186`, Development Gates run `37460494865` success). current-card `promoted-current`로 `--allow-update`. `current-evidence.json` current `0.42.91-admin-smoke`(provenance `990a4b2`, descriptor `…-closed`). `lane3-spec`이 `lane3-promotion-docs-spec-04291.json`을 만들었고(사람 값 `9`개) 문서 도구 dry-run·`-Apply`·`-Check` 모두 `ok`. `release-train.json` `operational_current=0.42.91-admin-smoke`, train `status=promoted`. 수기 정렬: `CurrentEvidenceVerifierTests`, `DOCUMENTATION_INDEX` 권위·호스트·consume 줄, `FEATURE_IMPLEMENTATION_LEDGER` operational 줄.

## Task 11: 종료와 Lane 3 merge

- [x] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

실행 기록(2026-10-06, clean HEAD `32cb460`): Release build 경고 `0`. `dotnet test src/DesktopNode.sln -c Release` 실패 `0`(Verification `615`, Delivery `763`, Api `488`, HyperV `257`, Host `216`, Contracts `200`, Cli `183`, Runtime `129`, Service `11`). Pester 네 종 실패 `0`(`528`, `49`, `50`, `129`). `npm run test:required --prefix web` exit `0`. Required CI 네 shard 모두 `ok=true`, `plan_only=false`. 이 기록 commit 뒤 push, PR, green CI 뒤 merge하면 train `0.42.91`이 끝난다.

## Task 13: 끊긴 create의 고아 디스크 처리 설계

- [x] 사용자 승인 `1,2`(2026-10-06)의 2. Task 8에서 본 결함(끊긴 `vm.create`가 VM 폴더에 `disk0.vhdx`를 남기고, reconcile `not-applied`는 그것을 언급하지 않으며, 다음 create는 `PCV_VHD_ALREADY_EXISTS`로 막힘)의 처리 방식을 설계 문서로 정하고 다음 train 후보로 올린다. Lane 1 설계, host mutation 없음. Task 11 뒤, Task 12 대기 전에 실행한다. 로컬 commit, push, PR, green CI 뒤 merge.

실행 기록(2026-10-06): PR #55(Task 11)를 head `79f705f` 고정으로 merge(`5cf64ac`)한 뒤 branch `lane1/orphan-disk-create-design-20261006`에서 설계했다. `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-interrupted-create-residue-design.md`(`pcv-interrupted-create-residue-v1`, `proposed`): create가 `disk0.vhdx` 전에 소유 표식 `.pcv-create-pending.json`(주인 프로세스 id와 시작 시각)을 쓰고, 같은 이름의 다음 create가 표식이 있고 주인 프로세스가 끝났고 디스크가 참조되지 않을 때만 잔여물을 지우고 진행한다. 거절 이유 `6`개는 `PCV_VHD_ALREADY_EXISTS` detail에 붙인다. reconcile은 readback 전용을 유지하고 `not-applied` hint에 회수 경로를 더한다. managed delete 정리는 표식 파일도 지운다. 구현은 다음 train 후보(Lane 1 checkpoint 하나, merge 뒤 `release-train.json` `queue` 행)이고 실제 확인은 다음 train Lane 2 probe다. report-only: `DefineSystem` 뒤 장치 연결 전에 끊긴 create를 reconcile이 `postcondition-confirmed`로 판정함(지문에 연결 장치 없음). host mutation 없음.

## Task 12: C5 runner 확인 (2026-10-19 이후)

- [ ] 2026-10-19 이후에만 실행한다. 그 전에 오면 기한 대기로 멈춘다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 완료 정의 §4에 C5 충족과 프로젝트 완료 판정을 적고 campaign을 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- train은 internal admin-smoke 범위다. public trusted signing과 external stable publication을 주장하지 않는다.
