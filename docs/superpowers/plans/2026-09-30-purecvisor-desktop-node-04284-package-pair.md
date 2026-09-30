# 0.42.84 package pair와 Lane 2 검증 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 개발 완료 campaign(`development-completion-20260930`)의 변경을 담은 `0.42.84-admin-smoke`를 빌드한다. 그 package로 다음을 PASS로 만든다.
- manual-admin pair(`0.42.83 → 0.42.84`)
- fullgate
- installed current-card
- 새 Web 동작과 reconcile 경로의 Lane 2 actual-VM probe

그리고 `vm.list` readback 확장을 설계한다. Lane 3 승격은 하지 않는다.

**Architecture:** 0.42.83 pair(`docs/superpowers/plans/2026-09-29-purecvisor-desktop-node-04283-lane3-promotion.md`)의 Task 1~4 순서를 따른다. 거기에 push/PR, 설계, actual-VM probe를 더한다. 한 task가 한 checkpoint다. 모든 commit은 PR branch `feat/development-completion-20260930`에 쌓고 commit마다 push한다. 머지하지 않은 PR 위에 새 branch를 쌓지 않는다.

**Tech Stack:** WiX installer `build.ps1`, `msiexec`, `Invoke-PcvDesktopNodeProduct.ps1`, manual-admin runner, `Invoke-PcvBatchSupervisor.ps1`, PCVCLI, C# / .NET 10, Pester 5

## 사용자 결정 (2026-09-30)

| 항목 | 결정 |
| --- | --- |
| 승인 | "전부 승인 합니다". 개발 완료 보고의 다음 단계 네 가지(push/PR, package pair와 fullgate, Lane 2 actual-VM, `vm.list` 확장 설계) |
| 호스트 작업 범위 | MSI 제거·설치, 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX, fullgate, current-card, Lane 2 probe VM 하나의 생성·조작·삭제 |
| push/PR | commit마다 push, PR 하나. merge는 사용자가 한다(auto mode가 `gh pr merge`를 막는다) |
| 승인 밖 | merge, Lane 3 승격, `current-evidence.json` 쓰기, public trusted signing, external stable publication |

## 착수 상태 (2026-09-30)

- 설치본은 `0.42.83-admin-smoke`(operational current)다. ARP는 `{A531920C-…}` 1개, 서비스는 `Running/Automatic`이다.
- VM은 보존 VM `pcv-guest-installed-04253-r1`(Off) 하나다.
- 소스 HEAD는 `feat/development-completion-20260930`(`5850f28`)이다. product payload가 0.42.83 build 뒤로 바뀌었다(reconcile, Web binding).

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다.
- 새 VM은 둘뿐이다. clean-host runner가 만드는 `pcv-cleanhost-*` 하나와 Task 7 probe VM(`pcv-lane2-devcomp-*`) 하나다. 성공하면 둘 다 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. 장기 token은 `-ApiTokenProtectedFile`로 넘긴다.
- evidence는 새 파일로만 쓴다. 기존 evidence는 덮어쓰지 않는다.
- 같은 version을 다시 빌드하지 않는다. 다시 빌드해야 하면 version을 올린다(`0.42.85`).
- 한도:
  - Lane 1: 30분, tool batch 18회
  - Lane 2: 45분, tool batch 12회
  - clean-host(Task 4c): 0.42.83 pair와 같은 180분 한도
- 같은 원인으로 3번 실패하거나, 범위 밖 설계가 필요하거나, 권한이 거부되면 멈춘다.
- public trusted signing과 external stable publication은 주장하지 않는다.

## Task 1: push와 PR (Lane 0)

- [x] `feat/development-completion-20260930`을 push하고 PR을 연다. 본문에는 개발 완료 13개 task, 이 campaign의 후속 commit이 같은 PR에 쌓인다는 점, merge 권한이 사용자에게 있다는 점을 적는다.

실행 기록(2026-09-30): branch를 push하고 PR #22(`https://github.com/HardcoreMonk/purecvisor-desktop-node-public/pull/22`)를 열었다. 이 campaign의 후속 commit은 같은 branch에 push한다.

## Task 2: `vm.list` readback 확장 설계 (Lane 1)

- [x] 설계 문서 `docs/superpowers/specs/2026-09-30-purecvisor-desktop-node-vm-list-readback-extension-design.md`를 쓴다. 범위는 DVD media 경로와 VHD `size_gb`를 WMI에서 읽는 방법, API 계약 변경, `vm.attach`/`vm.eject` reconcile 대상 전환 조건, `vm.disk-resize` 성공 판정, 테스트와 Lane 2 확인 방법이다.
- [x] 구현은 하지 않는다. 설계 승인은 따로 받는다.

실행 기록(2026-09-30): 설계 문서를 썼다. DVD media는 `vm.list`에 새 필드 `dvd_media`로 싣는다(추가 WMI 호출 없음). VHD 크기는 `vm.list`가 아니라 공개 route 없는 내부 read operation `vm.disk.inspect`(`MaxInternalSize`)로 읽는다. `vm.attach`/`vm.eject`를 대상으로 옮기는 조건과 `vm.disk-resize` 판정 변경을 표로 정했다. 구현 승인은 따로 받는다. 검증: Delivery `744`/`744`, `git diff --check`.

## Task 3: `0.42.84-admin-smoke` package (Lane 1)

- [x] clean HEAD에서 빌드한다: `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.84-admin-smoke -MsiProductVersion 0.42.84 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20260930-04284`
- [x] 산출물과 SHA를 확인하고 evidence `admin-smoke-package-2026-09-30-04284`를 쓴다. 설치하지 않는다.

실행 기록(2026-09-30): clean HEAD `aab0bc1`에서 빌드했다(exit `0`, `43`초). 결과는 MSI `12a582ef…`, payload aggregate `9d8c92c5…`, Host `12512b67…`, CLI `78273f6d…`, payload `8`개이고, provenance commit이 HEAD와 같다. evidence는 `admin-smoke-package-2026-09-30-04284`다.

## Task 4a: pair readiness (Lane 2)

- [x] `New-PcvManualAdminRebaselineReadiness.ps1 -PlanOnly`로 baseline `0.42.83`(설치본)과 target `0.42.84`의 readiness를 만든다.

실행 기록(2026-09-30): 설치본이 이미 baseline `0.42.83`이라 설치 변경 없이 실행했다. `-PlanOnly`, campaign `manual-admin-campaign-20260930-04283-04284`로 돌렸고 결과는 `ok=true`, `ready-current-baseline-target-package-pair`, `installed_version_matches_requested=true`, `host_mutation_performed=false`다. summary SHA는 `09d15758…`이고 root는 `artifacts/manual-admin-rebaseline-readiness-20260930-04283-04284`다.

## Task 4f: installed runtime ops summary (Lane 2)

- [x] baseline `0.42.83`이 설치된 동안 `pcvcli --protected-token-file <file> --json ops summary`를 캡처한다. evidence `installed-runtime-ops-summary-2026-09-30-04283`.

실행 기록(2026-09-30): baseline `0.42.83`이 설치된 동안 캡처했다. 결과는 CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, VM `1`개이고 token 형태 문자열은 없다. evidence는 `installed-runtime-ops-summary-2026-09-30-04283`이다.

## Task 4b: 설치본 update/rollback (Lane 2)

- [x] `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`로 `0.42.84` payload에 update하고 `-Action Rollback`을 실행한다. evidence `product-update-rollback-2026-09-30-04283-04284`.

실행 기록(2026-09-30): Update(`0.42.83 → 0.42.84`)와 Rollback 모두 exit `0`, `ok=true`이고 실행 단계는 0.42.83 run과 같다. update 직후 Host는 `+aab0bc1`, Web은 `200`이었다. 최종 상태는 manifest `0.42.83`, `DesktopNode.failed` `0.42.84`, service Running, Web `200`이고 재부팅과 VM 변화는 없다. evidence는 `product-update-rollback-2026-09-30-04283-04284`다.

## Task 4c: dedicated clean-host Windows Update (Lane 2)

- [x] `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`을 baseline `0.42.83` MSI, target `0.42.84` update package, `current-base.json` base로 `-InstallWindowsUpdates -RemoveVmOnSuccess` 실행한다.

실행 기록(2026-09-30): update ZIP(`09b22e1c…`)을 0.42.83 ZIP과 같은 구조로 만들어 runner를 실행했다(약 `130`초, exit `0`). base는 `current-base.json`(UBR `5622`)이 골랐고 Windows Update 대상은 `0`개였다. install, update, rollback이 모두 exit `0`이고 최종 manifest는 `0.42.83`, Web `200`, VM은 삭제됐다. evidence는 `internal-clean-host-install-update-rollback-smoke-2026-09-30-04283-04284`다.

## Task 4d: Burn install/repair/remove (Lane 2)

- [x] 설치본을 `0.42.84`에 맞춘 뒤 Burn bootstrapper lifecycle runner를 실행한다.

실행 기록(2026-09-30): 제품 Update로 설치본을 `0.42.84`에 맞춘 뒤 `-Execute`를 실행했다(약 `31`초). build, install, repair, remove, target MSI 복구 exit가 모두 `0`이고 결과는 PASS다. 최종 상태는 ARP `0.42.84` `{F50C37FD-…}` 1개, service `Running/Automatic`, Web `200`이다. evidence는 `burn-bootstrapper-lifecycle-smoke-2026-09-30-04284`다.

## Task 4e: MSIX build/install/update/remove (Lane 2)

- [x] MSIX lifecycle runner를 `0.42.83 → 0.42.84`로 실행한다.

실행 기록(2026-09-30): 첫 실행은 네 자리 버전(`0.42.83.0`)을 넘긴 입력 실수로 `pack-baseline`에서 `FAIL`이었다. runner가 `.0`을 붙여 `0.42.83.0.0`이 되었고, 설치 전이라 host mutation은 없었다. 세 자리 버전으로 `-r2`를 다시 실행했다(`19`초). pack, sign, verify가 모두 `0`이고 install, update, remove를 통과해 PASS다. 최종 상태는 smoke 패키지와 서비스 없음, MSI 서비스 `Running/Automatic`, manifest `0.42.84` 그대로다. evidence는 `msix-package-lifecycle-smoke-2026-09-30-04283-04284`다.

## Task 4g: pair descriptor (Lane 2, non-mutating)

- [x] 여섯 bucket root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행한다. 판정 기준은 `overall_status=pass`, `missing_count=0`, `not_pass_count=0`이다.

실행 기록(2026-09-30): 여섯 bucket summary 경로로 `-PlanOnly`를 실행했다. 결과는 `overall_status=pass`, runner `6`, `missing_count=0`, `not_pass_count=0`, host mutation 없음, next candidate `0.42.84-admin-smoke`다. evidence는 `manual-admin-campaign-descriptor-2026-09-30-04283-04284`다.

## Task 5: full admin host mutation gate (Lane 2)

- [x] 같은 version의 잔여 ProductCode를 점검하고, 설치본을 데이터 보존으로 제거해 ARP를 `0`으로 만든다.
- [x] 0.42.83 manifest에서 version, batch id, 경로만 바꿔 fullgate batch를 실행한다. evidence `full-admin-host-mutation-gate-2026-09-30-04284-hostmutation`.

실행 기록(2026-09-30): Burn이 남긴 clean `0.42.84`(`{F50C37FD-…}`)를 데이터 보존 `msiexec /x`로 제거해 ARP를 `0`으로 만들었다(첫 시도는 인자 구문 오류로 msiexec 미실행). gate HEAD `ee90e0e`에서 두 step 모두 PASS(`206.7s`, `11.1s`)였다. 설치본은 ARP `{EE05403C-…}` 1개, Host/CLI SHA와 ProductVersion `+ee90e0e`가 gate build와 같다. data root에서는 설계대로 `install.jsonl`만 없어졌다. evidence는 `full-admin-host-mutation-gate-2026-09-30-04284-hostmutation`이다.

## Task 6: 최종 설치와 installed current-card (Lane 2)

- [x] `0.42.84`를 설치된 상태로 두고 installed operator surface current-card를 캡처한다. 판정 기준은 CLI exit `0`, Web `200`, service `Running/Automatic`, TUI 없음이다.

실행 기록(2026-09-30): 0.42.83 캡처 스크립트를 바꿔 썼다. 첫 캡처는 판정 `pass`였지만 남은 0.42.83 상수 때문에 summary의 기대 SHA와 canonical 값이 틀려서 `-r2`로 다시 캡처했다. 결과는 PASS다. CLI 3개 exit `0`, Web 2개 `200`, service `Running/Auto`(credential manager, token flag 없음), TUI 없음, ARP 1개, 테스트 VM 0개, secret 없음이고 설치본은 fullgate build와 같다. evidence는 `installed-operator-surface-current-card-2026-09-30-04284`(`installed-non-promoted-candidate`)다.

## Task 7: 새 기능 Lane 2 actual-VM probe (Lane 2)

- [x] 설치본 `0.42.84` API로 probe VM 하나를 만든다. 새 Web 동작의 route(pause/resume, rename, memory/CPU stats, checkpoint schedule preview/set/clear, export preview/export, import preview/import, switch connect, device add, guest exec/channel preview)를 실제 VM에 실행한다. pre-state, mutation, readback, cleanup을 기록한다.
- [x] reconcile 경로는 두 가지를 확인한다. 하나는 비대상 job이 분류 이유를 돌려주는지다. 다른 하나는 interrupt를 만들 수 있는 범위에서 대상 job 하나의 postcondition 판정이다. 만들 수 없으면 그 사실을 기록한다.
- [x] probe VM과 export 산출물을 지운다. evidence `lane2-development-completion-actual-vm-2026-09-30-04284`. 상태는 `installed_non_promoted_candidate`다.

실행 기록(2026-09-30): 설치본 PCVCLI로 새 Web 동작의 route를 실제 VM에 실행했다. 모두 성공했고, DVD 추가만 정책대로 `PCV_VM_DEVICE_ALREADY_PRESENT`로 거절됐다. 설치본 `app.js`에 새 binding 표식 10개가 있다. 비대상 reconcile은 분류 이유를 돌려줬다. `vm.start` running 중 Host를 강제 종료해 `PCV_JOB_INTERRUPTED`를 만들었고, reconcile이 `succeeded`로 확정했다. probe VM 두 개와 export를 지웠고, managed delete가 남긴 probe 디렉터리 두 개도 지웠다. evidence는 `lane2-development-completion-actual-vm-2026-09-30-04284`다.

## Task 8: 종료

- [x] 종료 검증(`dotnet test src/DesktopNode.sln`, `npm run test:required --prefix web`, `git diff --check`)을 돌리고 campaign을 닫는다. `next_step`에는 Lane 3 승격이 별도 승인 대상이라고 적는다.

실행 기록(2026-09-30): 깨끗한 HEAD `c0dae89`에서 `dotnet test src/DesktopNode.sln`이 전 프로젝트 통과했다(Api `477`, Verification `557`, Delivery `744`, Host `216`, HyperV `238`, Contracts `200`, Cli `179`, Runtime `128`, Service `11`). `npm run test:required --prefix web` exit `0`, `git diff --check`도 통과했다. campaign을 닫는다. 결과는 package pair PASS, fullgate PASS, installed current-card PASS, Lane 2 probe PASS이고 모두 `installed_non_promoted_candidate`다.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 결과는 `installed_non_promoted_candidate`까지다.
- public trusted signing과 external stable publication을 주장하지 않는다.
