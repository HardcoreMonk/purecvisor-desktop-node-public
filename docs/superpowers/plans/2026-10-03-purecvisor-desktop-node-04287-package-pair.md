# 0.42.87 package pair, 같은 version 재설치(A)와 Lane 2 검증 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 같은 version 재설치 설계 권고 A(`AllowSameVersionUpgrades`)를 넣고, A와 post-0.42.86 backlog의 reconcile 문구 수정을 담은 `0.42.87-admin-smoke`를 빌드한다. 그 package로 다음을 PASS로 만든다.
- manual-admin pair(`0.42.86 → 0.42.87`)
- fullgate. 같은 version 설치본 위에서 시작해 A를 실증한다.
- installed current-card
- reconcile 안내 문구의 설치본·actual-VM 확인

Lane 3 승격은 하지 않는다.

**Architecture:** 0.42.85 pair(`2026-10-01-purecvisor-desktop-node-04285-package-pair.md`)와 10-02 작업 지시서(`2026-10-02-purecvisor-desktop-node-development-work-order.md`)의 순서를 따른다. branch는 PR #28의 `docs/project-status-audit-20261003`이다. PR이 아직 merge 전이므로 같은 branch에 commit을 쌓고, commit마다 push한다.

**Tech Stack:** WiX v4 `Product.wxs`, installer `build.ps1`, `msiexec`, `Invoke-PcvDesktopNodeProduct.ps1`, manual-admin runner, `Invoke-PcvBatchSupervisor.ps1`, PCVCLI

## 사용자 결정 (2026-10-03)

승인 원문: `1,2,3` (post-0.42.86 backlog 최종 보고의 결정 1~3에 대한 답).

| 항목 | 결정 |
| --- | --- |
| 1 push/PR | backlog branch push와 PR 하나(PR #28). 이 campaign의 commit도 같은 PR에 push한다 |
| 2 package pair와 Lane 2 | `0.42.87` package, manual-admin pair 여섯 bucket, fullgate, current-card, reconcile 문구 Lane 2 확인 |
| 3 설계 권고 A | `MajorUpgrade AllowSameVersionUpgrades="yes"`. 재검증은 2의 pair와 fullgate가 맡는다 |
| 호스트 작업 범위 | MSI 제거·설치, 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX, fullgate, current-card, Lane 2 probe VM의 생성·조작·삭제 |
| 승인 밖 | PR merge, Lane 3 승격, `current-evidence.json` 쓰기, public trusted signing, external stable publication |

## 착수 상태 (2026-10-03)

- operational current와 이 호스트 설치본은 모두 `0.42.86-admin-smoke`다. 설치본은 fullgate build `+b807803`이고 ARP는 `{1994F4DF-…}` 1개, 서비스는 `Running/Automatic`이다.
- 0.42.86 build 뒤 product payload 변경은 reconcile 안내 문구(`5b5738e`)뿐이다. Task 1이 installer `Product.wxs`를 바꾼다.

## Global Constraints

- 보존 VM의 전원, Notes, 디스크를 바꾸지 않는다.
- 새 VM은 clean-host runner의 `pcv-cleanhost-*`와 Task 6 probe VM뿐이다. 성공하면 모두 지운다. managed delete가 남기는 VHD 디렉터리는 참조가 없는지 확인한 뒤 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다.
- evidence는 새 파일로만 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, clean-host 180분.
- runner 입력 함정(memory `lane2-pair-runner-gotchas`)을 따른다: MSIX 세 자리 버전, current-card SHA 상수 전부 교체, update ZIP 직접 생성, 제품 Update 직후 Burn은 잠시 기다린다.
- 같은 원인으로 3번 실패하거나, 범위 밖 설계가 필요하거나, 권한이 거부되면 멈춘다.

## Task 1: 같은 version 재설치 A (Lane 1)

**수정:** `packaging/windows-desktop-node/installer/Product.wxs`, `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`, 관련 C# 계약과 spec pin, 설계 문서

- [x] `MajorUpgrade`에 `AllowSameVersionUpgrades="yes"`를 더한다. 기본 `Schedule`(`afterInstallValidate`)이 이전 제품을 먼저 지우는지 확인한다.
- [x] A가 있으면 같은 version 잔여 항목은 gate install의 major upgrade가 지운다. smoke의 `same-version-preflight`는 차단 대신 기록(`same_version_upgrade_expected`)으로 바꾸고, `final-restore-install` 뒤 같은 version ARP 항목이 정확히 `1`개인지 검사해 아니면 멈춘다. build commit 검사는 그대로 차단한다.
- [x] installer 계약 테스트와 smoke 계약을 고치고 spec pin을 갱신한다. 설계 문서 상태를 A 채택으로 고친다.

검증: `dotnet test src/DesktopNode.Delivery.Tests`, installer 관련 테스트, smoke `-SelfTest`, `Update-PcvContractSpecPins.ps1 -Check`, `git diff --check`.

실행 기록(2026-10-03): `MajorUpgrade`에 `AllowSameVersionUpgrades="yes"`와 `Schedule="afterInstallValidate"`를 명시했다. smoke 사전 검사는 기록만 하고, 사후에 build commit과 같은 version ARP `1`개를 차단 검사한다. C# 계약 `PcvSameVersionUpgradeContractTests` `1`개와 smoke 계약 `1`개를 더하고 기존 smoke 계약 하나를 비차단으로 바꿨다. orchestration spec pin 갱신. smoke `-SelfTest` exit `0`, Delivery `752/752`. WiX build 확인은 Task 2가 한다.

## Task 2: `0.42.87-admin-smoke` package (Lane 1)

- [x] clean HEAD에서 `build.ps1 -Version 0.42.87-admin-smoke -MsiProductVersion 0.42.87 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261003-04287`를 실행한다. update ZIP을 만들고 evidence `admin-smoke-package-2026-10-03-04287`을 쓴다.

실행 기록(2026-10-03): clean HEAD `8d940da`에서 빌드했다(exit `0`, `40`초). MSI `a0041c9f…`, payload aggregate `7c339388…`, Host `f6916d3f…`(ProductVersion `+8d940da`), CLI `3c681733…`, payload `8`개, provenance commit이 HEAD와 같다. update ZIP `28dc8af8…`. MSI Upgrade 행이 `VersionMax=0.42.87` 포함(Attributes `513`)이고 `RemoveExistingProducts`가 `1401`이다(0.42.86은 Attributes `1`). evidence `admin-smoke-package-2026-10-03-04287`.

## Task 3a: pair readiness (Lane 2)

- [x] baseline `0.42.86`(clean package `admin-smoke-package-20261002-04286`)과 target `0.42.87`로 readiness를 `-PlanOnly`로 실행한다.

실행 기록(2026-10-03): 설치본이 baseline `0.42.86`(fullgate build `+b807803`)이라 설치 변경 없이 실행했다. `-PlanOnly`, campaign `manual-admin-campaign-20261003-04286-04287` 결과는 `ok=true`, `ready-current-baseline-target-package-pair`, `installed_version_matches_requested=true`, baseline MSI `8edb19ce…`, target MSI `a0041c9f…`, host mutation 없음이다. summary SHA `e460a21e…`, root `artifacts/manual-admin-rebaseline-readiness-20261003-04286-04287`.

## Task 3f: installed runtime ops summary (Lane 2)

- [x] baseline `0.42.86`이 설치된 동안 ops summary를 캡처한다.

실행 기록(2026-10-03): CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, VM `1`개, token 형태 문자열 `0`개. summary SHA `a03b8588…`. evidence `installed-runtime-ops-summary-2026-10-03-04286`.

## Task 3b: 설치본 update/rollback (Lane 2)

- [x] `0.42.87` payload로 Update 뒤 Rollback한다.

실행 기록(2026-10-03): Update(`0.42.86 → 0.42.87`, `11`단계)와 Rollback(`7`단계) 모두 exit `0`, `ok=true`. update 직후 Host `+8d940da`, Web `200`. 최종 manifest `0.42.86`, failed `0.42.87`, service Running, Web `200`, 재부팅·VM 변화 없음. evidence `product-update-rollback-2026-10-03-04286-04287`.

## Task 3c: dedicated clean-host Windows Update (Lane 2)

- [x] baseline `0.42.86` clean MSI, target `0.42.87` update ZIP, `current-base.json` base로 clean-host runner를 실행한다.

실행 기록(2026-10-03): 약 `135`초, exit `0`. base `current-base`(UBR `5622`), Windows Update 대상 `0`개. install, update, rollback 모두 exit `0`이고 최종 manifest `0.42.86`, Web `200`, VM 삭제됨. evidence `internal-clean-host-install-update-rollback-smoke-2026-10-03-04286-04287`.

## Task 3d: Burn (Lane 2)

- [x] 설치본을 `0.42.87`에 맞춘 뒤 Burn lifecycle runner를 실행한다.

실행 기록(2026-10-03): 제품 Update로 `0.42.87`에 맞추고 15초 기다린 뒤 실행했다. build, install, repair, remove, target MSI 복구 모두 exit `0`, `status=PASS`, `restoration_status=PASS`(약 `43`초, repair `3010` 없음). 최종 ARP `0.42.87` `{5EF95755-…}` 1개, Host `+8d940da`, service Running/Automatic, Web `200`. evidence `burn-bootstrapper-lifecycle-smoke-2026-10-03-04287`.

## Task 3e: MSIX (Lane 2)

- [x] MSIX lifecycle runner를 `0.42.86 → 0.42.87`로 실행한다.

실행 기록(2026-10-03): 세 자리 버전으로 한 번에 PASS(`19`초). pack·sign·verify `0`, install·update·remove 통과, smoke 패키지와 서비스 없음, manifest `0.42.87` 유지. evidence `msix-package-lifecycle-smoke-2026-10-03-04286-04287`.

## Task 3g: pair descriptor (Lane 2, non-mutating)

- [x] 여섯 bucket summary로 descriptor를 `-PlanOnly`로 만든다.

실행 기록(2026-10-03): `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`, host mutation 없음, next candidate `0.42.87-admin-smoke`. evidence `manual-admin-campaign-descriptor-2026-10-03-04286-04287`.

## Task 4: fullgate와 A 실증 (Lane 2)

- [x] 설치본을 비우지 않고 clean `0.42.87`이 설치된 상태(ARP `0.42.87` 1개)에서 fullgate를 시작한다. gate의 첫 install이 같은 version major upgrade다.
- [x] gate 뒤 ARP `0.42.87` 항목 `1`개, 설치본 Host/CLI build commit이 gate build와 같음, uninstall 단계에 `another client exists`가 없음을 기록한다.

실행 기록(2026-10-03): clean `0.42.87` `{5EF95755}`이 설치된 채로 시작했다. 이전 gate의 Ubuntu ISO가 호스트에 없어, OS를 부팅하지 않는 최소 ISO 9660(`artifacts/smoke-media-20261003/`)을 썼다. batch `ok=true`, 두 step `PASS`(attempt `1`), MSI lifecycle 6 phase exit `0`, 약 `212`초. install log에 `WIX_UPGRADE_DETECTED`=`{5EF95755}`와 `RemoveExistingProducts`가 있고, 네 MSI log에 `another client exists`와 `Won't Overwrite`가 없다. 사후 검사 둘 다 통과: 설치본 build `8ade930` == gate build, 같은 version ARP `{F1D32C79}` 1개. Host/CLI SHA가 gate build와 같고 Web `200`, firewall rule `0`. evidence `full-admin-host-mutation-gate-2026-10-03-04287-hostmutation`.

## Task 5: installed current-card (Lane 2)

- [x] `0.42.87`을 설치된 채로 두고 current-card를 캡처한다. 결과는 `installed_non_promoted_candidate`다.

실행 기록(2026-10-03): `status=pass`, CLI `3/3`, Web `2/2`, 설치본 Host/CLI가 fullgate operational payload와 같다. ARP `0.42.87` 1개, 테스트 VM `0`, secret 관측 없음. `promotion_ledger_status=not-promoted`, canonical current `0.42.86-admin-smoke` 유지. evidence `installed-operator-surface-current-card-2026-10-03-04287`.

## Task 6: reconcile 안내 문구 Lane 2 확인 (Lane 2)

- [x] 설치본 `0.42.87`에서 비대상 operation job의 reconcile 응답이 "confirm whether the mutation applied"이고 `rename`을 말하지 않음을 확인한다.
- [x] probe VM 하나로 `vm.rename` job의 reconcile 응답이 계속 rename을 말하는지 확인한다. probe VM과 디렉터리를 지운다.

실행 기록(2026-10-03): 설치본 `0.42.87`(`+8ade930`)에서 probe VM을 만들고 rename, export했다. 끝난 `vm.rename` job의 reconcile 안내는 "confirm whether the rename applied", 비대상 `vm.export` job은 "confirm whether the mutation applied"이고 `rename`이 없다. probe 자동 summary는 CLI 오류가 텍스트 형식이라 파싱하지 못해 `FAIL`로 남았고, 원본 응답으로 다시 판정한 `wording-check.json`이 `PASS`다. VM, export 디렉터리, 남은 VM 디렉터리를 지웠다. evidence `lane2-reconcile-wording-actual-vm-2026-10-03-04287`.

## Task 7: 종료

- [ ] clean HEAD 종료 검증 뒤 push하고 campaign을 닫는다. `next_step`에 PR merge와 Lane 3 승격이 승인 대상이라고 적는다.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 결과는 `installed_non_promoted_candidate`까지다.
- A는 같은 version 재설치를 major upgrade로 바꿀 뿐 downgrade 정책을 바꾸지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
