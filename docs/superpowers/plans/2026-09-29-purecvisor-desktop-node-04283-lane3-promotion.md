# 0.42.83 Lane 3 승격 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 새 product payload를 담은 `0.42.83-admin-smoke`를 빌드한다. 그 package로 manual-admin pair(`0.42.78 → 0.42.83`), fullgate, installed current-card를 PASS로 만들고, operational current를 `0.42.78-admin-smoke`에서 `0.42.83-admin-smoke`로 승격한다.

**Architecture:** 0.42.78 승격과 같은 구성이다.
1. package
2. pair 여섯 bucket과 descriptor
3. fullgate
4. current-card
5. Lane 3 문서

Lane 3 문서 단계는 PR #17의 `Invoke-PcvLane3PromotionDocs.ps1`와 데이터 기반 C# 계약을 쓴다. 한 task가 한 checkpoint다. 호스트 task는 시작할 때 해당 runner의 인자를 Lane 0으로 확인하고, 실제 명령을 실행 기록에 남긴다.

**Tech Stack:** WiX installer `build.ps1`, `msiexec`, `Invoke-PcvDesktopNodeProduct.ps1`, manual-admin runner, `Invoke-PcvBatchSupervisor.ps1`, PCVCLI, C# / .NET 10, Pester 5

## 사용자 결정 (2026-09-29)

| 항목 | 결정 |
| --- | --- |
| 진행 방식 | 계획서 작성 후 호스트 작업을 연속으로 실행한다 |
| 호스트 작업 범위 | MSI 제거·설치, 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX, fullgate, current-card |
| current-evidence 쓰기 | Task 5에서 한다 |
| push/PR | 아직 승인 전이다. Task 6에서 묻는다 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다.
- 새 VM은 clean-host runner가 만드는 `pcv-cleanhost-*` 하나뿐이다. 성공하면 runner가 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. 장기 token은 `-ApiTokenProtectedFile`로 넘긴다.
- evidence는 새 파일로만 쓴다. 기존 evidence는 덮어쓰지 않는다.
- 같은 version을 다시 빌드하지 않는다. 잔여 ProductCode 문제를 피하기 위해서다(`post-04278-backlog` Task 3). 다시 빌드해야 하면 version을 올린다.
- 한도: Lane 1과 Lane 3은 30분, tool batch 18회다. Lane 2는 45분, tool batch 12회다. clean-host(Task 2c)가 45분을 넘을 것으로 보이면 착수할 때 연장 승인을 받는다.
- 같은 원인으로 3번 실패하거나, 범위 밖 설계가 필요하거나, 권한이 거부되면 멈춘다.
- public trusted signing과 external stable publication은 주장하지 않는다.

## Task 1: `0.42.83-admin-smoke` package (Lane 1)

- [x] clean HEAD에서 빌드한다. 명령: `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.83-admin-smoke -MsiProductVersion 0.42.83 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`
- [x] 산출물(MSI, update ZIP, payload, provenance)과 SHA를 확인한다. evidence `docs/ga-ready/evidence/admin-smoke-package-2026-09-29-04283.md`를 새로 쓴다. 이 task는 설치하지 않는다.
실행 기록(2026-09-29): clean HEAD `bcda14f`에서 빌드했다. 결과는 MSI `52d7cfd5…`, payload aggregate `76d9ae9c…`, Host `8d8622fb…`, CLI `c217df9f…`, payload `8`개이고, provenance commit이 HEAD와 같다. update ZIP은 build 산출물에 없다. 0.42.78의 ZIP은 pair 단계에서 만들어졌으므로 Task 2에서 만드는 방법을 확인한다. evidence는 `admin-smoke-package-2026-09-29-04283`이다.

## Task 2a: pair readiness (Lane 2)

- [x] `New-PcvManualAdminRebaselineReadiness.ps1`로 baseline `0.42.78`과 target `0.42.83`의 readiness summary를 만든다.
실행 기록(2026-09-29): readiness는 설치본이 baseline과 같아야 한다(`installed_version_matches_requested`). 그래서 Task 2b의 baseline 설치를 여기서 먼저 했다. `0.42.82` 제거(`{3027FF28-…}`, exit `0`, `REMOVE_DATA` 없음) 뒤 `0.42.78` MSI를 설치했다(exit `0`). 결과: ARP `0.42.78` 1개, service `Running/Automatic`, Web `200`, API `401`. 로그는 `artifacts/manual-admin-campaign-20260929-04278-04283/baseline-install`에 있다. readiness(`-PlanOnly`, campaign `manual-admin-campaign-20260929-04278-04283`)는 `ok=true`, `ready-current-baseline-target-package-pair`, target MSI `52d7cfd5…`, `host_mutation_performed=false`다. summary SHA는 `47707174…`이고 root는 `artifacts/manual-admin-rebaseline-readiness-20260929-04278-04283`이다.

## Task 2b: 설치본 update/rollback (Lane 2)

- [x] 설치본을 `0.42.82`에서 baseline `0.42.78`로 되돌린다. MSI 제거 뒤 `0.42.78` MSI를 설치한다.
- [x] `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`로 `0.42.83` payload에 update한다. 그 뒤 `-Action Rollback`을 실행한다.
실행 기록(2026-09-29): baseline 설치는 Task 2a에서 했다. Update(`0.42.78 → 0.42.83`)와 Rollback 모두 exit `0`, `ok=true`이고, 실행 단계는 0.42.78 run과 같다. 최종 상태는 manifest `0.42.78`, `DesktopNode.failed` `0.42.83`, service Running이다. evidence `product-update-rollback-2026-09-29-04278-04283`.

## Task 2c: dedicated clean-host Windows Update (Lane 2)

- [ ] `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`을 실행한다.
  - baseline `0.42.78` MSI, target `0.42.83` update package
  - `-InstallWindowsUpdates -RemoveVmOnSuccess`
- [ ] base VHD가 있는지 먼저 확인한다.

## Task 2d: Burn install/repair/remove (Lane 2)

- [ ] Burn bootstrapper lifecycle runner를 `0.42.83` package로 실행한다.

## Task 2e: MSIX build/install/update/remove (Lane 2)

- [ ] MSIX lifecycle runner를 `0.42.78 → 0.42.83`으로 실행한다.

## Task 2f: installed runtime ops summary (Lane 2)

- [x] 설치본 `pcvcli --protected-token-file <file> --json ops summary`로 runtime ops summary를 만든다.
실행 기록(2026-09-29): baseline `0.42.78`이 설치된 동안 Task 2c보다 먼저 실행했다(0.42.78 pair 때도 baseline 설치본에서 캡처했다). 결과는 CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, token 값 없음이다. evidence `installed-runtime-ops-summary-2026-09-29-04278`.

## Task 2g: pair descriptor (Lane 2, non-mutating)

- [ ] 여섯 bucket root로 `New-PcvManualAdminCampaignDescriptor`를 실행한다.
  - 판정 기준: `overall_status=pass`, `missing_count=0`, `not_pass_count=0`
- [ ] 결과로 descriptor evidence를 쓴다.

## Task 3: full admin host mutation gate (Lane 2)

- [ ] 같은 version의 잔여 ProductCode를 먼저 점검한다.
- [ ] `0.42.83`으로 fullgate batch를 실행한다(Service/MSI/Hyper-V route, OS mutation).
- [ ] 결과를 evidence `full-admin-host-mutation-gate-2026-09-29-04283-hostmutation.md`로 쓴다.

## Task 4: 최종 설치와 installed current-card (Lane 2)

- [ ] `0.42.83`을 설치된 상태로 두고, installed operator surface current-card를 실행한다.
  - 판정 기준: CLI exit `0`, Web `200`, service `Running/Automatic`, TUI 없음

## Task 5: Lane 3 문서 (Lane 3)

- [ ] functional carry-forward evidence를 쓴다(04275 actual-VM PASS를 이어 받는다).
- [ ] manual-admin single-root consume과 consume evidence를 쓴다.
- [ ] `current-evidence.json`을 `0.42.83`으로 쓴다.
- [ ] 승격 spec 하나로 `Invoke-PcvLane3PromotionDocs.ps1`를 dry-run, `-Apply`, `-Check` 순서로 실행한다.
- [ ] `DOCUMENTATION_INDEX`와 `FEATURE_IMPLEMENTATION_LEDGER`를 정렬한다.
- [ ] 솔루션 테스트, Pester, pin `-Check`를 돌린다.

## Task 6: 종료 검증과 push/PR

- [ ] clean HEAD에서 전체 검증을 돌린다.
- [ ] push/PR은 승인을 받은 뒤에 한다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- 보존 VM과 기존 evidence는 바꾸지 않는다.
