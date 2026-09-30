# 0.42.85 package pair와 Lane 2 검증 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `vm.list` readback 확장(PR #24, merge `5b84a4e`)을 담은 `0.42.85-admin-smoke`를 빌드한다. 그 package로 다음을 PASS로 만든다.
- manual-admin pair(`0.42.84 → 0.42.85`)
- fullgate
- installed current-card
- 새 readback과 media reconcile의 Lane 2 actual-VM probe

Lane 3 승격은 하지 않는다.

**Architecture:** 0.42.84 pair(`2026-09-30-purecvisor-desktop-node-04284-package-pair.md`)의 Task 3~7 순서를 따른다. branch는 `lane2/04285-package-pair-20261001`(`origin/main` `5b84a4e` 기준)이다. commit마다 push하고, 끝에 PR 하나를 연다.

**Tech Stack:** WiX installer `build.ps1`, `msiexec`, `Invoke-PcvDesktopNodeProduct.ps1`, manual-admin runner, `Invoke-PcvBatchSupervisor.ps1`, PCVCLI

## 사용자 결정 (2026-10-01)

| 항목 | 결정 |
| --- | --- |
| 승인 | "0.42.85 package pair, fullgate, 실제 VM 확인" |
| 호스트 작업 범위 | MSI 제거·설치, 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX, fullgate, current-card, Lane 2 probe VM의 생성·조작·삭제 |
| push/PR | commit마다 push, 끝에 PR 하나 |
| 승인 밖 | PR merge(보고 뒤 따로 묻는다), Lane 3 승격, `current-evidence.json` 쓰기, public trusted signing, external stable publication |

## 착수 상태 (2026-10-01)

- operational current와 이 호스트 설치본은 모두 `0.42.84-admin-smoke`다. 설치본은 fullgate build `+ee90e0e`이고 ARP는 `{EE05403C-…}` 1개, 서비스는 `Running/Automatic`이다.
- VM은 보존 VM `pcv-guest-installed-04253-r1`(Off) 하나다.
- 0.42.84 build 뒤 product payload 변경은 PR #24(`dvd_media`, `vm.disk.inspect`, media reconcile, disk-resize 판정, Web `DVD Media` 행)다.

## Global Constraints

- 보존 VM의 전원, Notes, 디스크를 바꾸지 않는다.
- 새 VM은 clean-host runner의 `pcv-cleanhost-*`와 Task 7 probe VM(과 그 import VM)뿐이다. 성공하면 모두 지운다. managed delete는 VHD 디렉터리를 남기므로 probe가 만든 디렉터리는 참조가 없는지 확인한 뒤 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다.
- evidence는 새 파일로만 쓴다. 같은 version을 다시 빌드하지 않는다.
- 한도:
  - Lane 1: 30분, tool batch 18회
  - Lane 2: 45분, tool batch 12회
  - clean-host: 0.42.84 pair와 같은 180분 한도
- runner 입력 함정(memory `lane2-pair-runner-gotchas`)을 따른다.
  - MSIX 버전은 세 자리로 넘긴다.
  - current-card 스크립트의 SHA 상수를 모두 바꾼다.
  - update ZIP은 직접 만든다.
- 같은 원인으로 3번 실패하거나, 범위 밖 설계가 필요하거나, 권한이 거부되면 멈춘다.

## Task 1: `0.42.85-admin-smoke` package (Lane 1)

- [x] clean HEAD에서 `build.ps1 -Version 0.42.85-admin-smoke -MsiProductVersion 0.42.85 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261001-04285`를 실행한다. update ZIP을 만들고 evidence `admin-smoke-package-2026-10-01-04285`를 쓴다.

실행 기록(2026-10-01): clean HEAD `f5b6d10`에서 빌드했다(exit `0`, `38`초). 결과는 MSI `cba74683…`, payload aggregate `796faba6…`, Host `e1c0cb89…`, CLI `2e7e5130…`, payload `8`개이고, provenance commit이 HEAD와 같다. update ZIP은 `74c8eeb2…`다. evidence는 `admin-smoke-package-2026-10-01-04285`다.

## Task 2a: pair readiness (Lane 2)

- [x] baseline `0.42.84`(clean package `admin-smoke-package-20260930-04284`)와 target `0.42.85`로 `New-PcvManualAdminRebaselineReadiness.ps1 -PlanOnly`를 실행한다.

실행 기록(2026-10-01): 설치본이 baseline `0.42.84`라 설치 변경 없이 실행했다. `-PlanOnly`, campaign `manual-admin-campaign-20261001-04284-04285` 결과는 `ok=true`, `ready-current-baseline-target-package-pair`, `installed_version_matches_requested=true`, target MSI `cba74683…`, host mutation 없음이다. summary SHA `8a7e6c2a…`.

## Task 2f: installed runtime ops summary (Lane 2)

- [x] baseline `0.42.84`가 설치된 동안 ops summary를 캡처한다. evidence `installed-runtime-ops-summary-2026-10-01-04284`.

실행 기록(2026-10-01): CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, VM `1`개이고 token 형태 문자열은 없다. evidence `installed-runtime-ops-summary-2026-10-01-04284`.

## Task 2b: 설치본 update/rollback (Lane 2)

- [x] `0.42.85` payload로 Update 뒤 Rollback한다. evidence `product-update-rollback-2026-10-01-04284-04285`.

실행 기록(2026-10-01): Update(`0.42.84 → 0.42.85`)와 Rollback 모두 exit `0`, `ok=true`이고 단계 수는 0.42.84 run과 같다(`11`/`7`). update 직후 Host `+f5b6d10`, Web `200`이었다. 최종 manifest `0.42.84`, failed `0.42.85`, service Running, Web `200`이고 재부팅과 VM 변화는 없다. evidence `product-update-rollback-2026-10-01-04284-04285`.

## Task 2c: dedicated clean-host Windows Update (Lane 2)

- [x] baseline `0.42.84` clean MSI, target `0.42.85` update ZIP, `current-base.json` base로 clean-host runner를 실행한다.

실행 기록(2026-10-01): 약 `134`초, exit `0`. base `current-base`(UBR `5622`), Windows Update 대상 `0`개다. install, update, rollback이 모두 exit `0`이고 최종 manifest `0.42.84`, Web `200`이며 VM은 삭제됐다. evidence `internal-clean-host-install-update-rollback-smoke-2026-10-01-04284-04285`.

## Task 2d: Burn (Lane 2)

- [x] 설치본을 `0.42.85`에 맞춘 뒤 Burn lifecycle runner를 실행한다.

실행 기록(2026-10-01): r1은 repair `3010`(Host.exe 사용 중, 재부팅 뒤 교체)으로 FAIL이었고 복구는 PASS였다. Host.exe 교체 재부팅 대기가 없음을 확인한 뒤 새 root `-r2`로 다시 실행해 PASS했다(모든 exit `0`). 최종 ARP `0.42.85` `{B23EB9BF-…}` 1개, service Running/Automatic, Web `200`. evidence `burn-bootstrapper-lifecycle-smoke-2026-10-01-04285`(r2 기준).

## Task 2e: MSIX (Lane 2)

- [x] MSIX lifecycle runner를 `0.42.84 → 0.42.85`로 실행한다.

실행 기록(2026-10-01): 세 자리 버전으로 한 번에 PASS(`19`초). pack·sign·verify `0`, install·update·remove 통과, smoke 패키지와 서비스 없음, manifest `0.42.85` 유지. evidence `msix-package-lifecycle-smoke-2026-10-01-04284-04285`.

## Task 2g: pair descriptor (Lane 2, non-mutating)

- [x] 여섯 bucket summary로 descriptor를 `-PlanOnly`로 만든다.

실행 기록(2026-10-01): `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`, host mutation 없음, next candidate `0.42.85-admin-smoke`. evidence `manual-admin-campaign-descriptor-2026-10-01-04284-04285`.

## Task 3: fullgate (Lane 2)

- [x] 설치본을 데이터 보존으로 제거해 ARP를 `0`으로 만들고, 0.42.84 manifest에서 version, batch id, 경로만 바꿔 fullgate를 실행한다.

실행 기록(2026-10-01): batch `ok=true`, 두 step `PASS`, MSI lifecycle 6 phase exit `0`. 설치본 Host/CLI SHA는 gate build와 같다. ARP 1개, Web `200`, firewall rule `0`. evidence `full-admin-host-mutation-gate-2026-10-01-04285-hostmutation`.

## Task 4: installed current-card (Lane 2)

- [x] `0.42.85`를 설치된 채로 두고 current-card를 캡처한다.

실행 기록(2026-10-01): `status=pass`, CLI `3/3`, Web `2/2`, 설치본 Host/CLI가 fullgate operational payload와 같다. `promotion_ledger_status=not-promoted`, canonical current는 `0.42.84-admin-smoke`로 유지된다. evidence `installed-operator-surface-current-card-2026-10-01-04285`.

## Task 5: 새 기능 actual-VM probe (Lane 2)

- [ ] 설치본 `0.42.85`에서 probe VM 하나로 다음을 확인한다.
  - `vm list`에 `dvd_media`가 나오는지(생성 때 붙은 ISO), eject 뒤 비는지, attach 뒤 새 ISO가 나오는지
  - media job의 capture baseline(`before_media`)이 채워지는지
  - disk-resize job의 capture `before_value`가 실제 VHD byte인지(`vm.disk.inspect`)
  - interrupt를 만들 수 있으면 media job 하나의 reconcile 판정
- [ ] probe VM과 디렉터리를 지운다. evidence `lane2-vm-list-readback-actual-vm-2026-10-01-04285`.

## Task 6: 종료

- [ ] 종료 검증 뒤 PR을 열고 campaign을 닫는다. `next_step`에 PR merge와 Lane 3 승격이 승인 대상이라고 적는다.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 결과는 `installed_non_promoted_candidate`까지다.
- public trusted signing과 external stable publication을 주장하지 않는다.
