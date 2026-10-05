# Release train `0.42.90` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 대기열에서 고정한 PR #48(Release build nullable 경고 정리)을 담은 `0.42.90-admin-smoke`를 package부터 Lane 3까지 한 바퀴 돌려 operational current로 승격한다. 3c 이후 첫 train이다. pair는 orchestrator로 한 번에 돌고, evidence 값은 `pcvverify train-facts`, Lane 3 승격 spec은 `pcvverify lane3-spec`이 만든다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 train task 순서를 따른다. branch는 `train/04290-20261005`(`origin/main` `a66a8cd` 기준)다. pair evidence PR을 먼저 merge하고, Lane 3는 그 merge 위의 branch에서 PR 하나로 merge한다. 직전 train 계획: `docs/superpowers/plans/2026-10-04-purecvisor-desktop-node-train-04289.md`.

**Tech Stack:** `packaging/windows-desktop-node/installer/build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, current-card capture, `pcvverify train-facts`, `pcvverify train-evidence`, `pcvverify lane3-spec`, `Invoke-PcvLane3PromotionDocs.ps1`

## 출발 승인 (2026-10-05)

승인 원문: `1,2,3` (hygiene campaign 최종 보고의 다음 승인. 1과 2는 branch 정리이고 이 계획 밖에서 끝났다).

| 항목 | 범위 |
| --- | --- |
| 1 | PR #43 close, `docs/train-phase3c-procedure-20261004` 원격·로컬 삭제. 끝남: PR #43 `CLOSED`(#44를 가리키는 comment), branch 삭제 |
| 2 | `codex/04275-stage1-immutable-preflight` bundle 보존 뒤 로컬 삭제. 끝남: `artifacts/branch-bundles/codex-04275-stage1-immutable-preflight-216a6ca.bundle`(SHA-256 `4ce7d6e5…`, verify 통과) |
| 3 | release train `0.42.90-admin-smoke` 출발. queue 행 PR #48 고정, package build, pair host mutation(제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX), fullgate, current-card, Lane 3 `current-evidence.json` 쓰기, push, PR, green CI 뒤 merge. 정기 출발일(2026-10-11) 전이므로 사용자 요청에 의한 출발이다 |

정차하면 이 승인은 끝난다. public trusted signing과 external stable publication은 범위 밖이다.

## 적재 변경

| PR | 변경 commit | 요약 | Lane 2 probe |
| --- | --- | --- | --- |
| #48 | `b433c50` | Release build nullable 경고 CS8622·CS8600 정리(실행 값 변경 없음) | 없음 |

## Global Constraints

- 보존 VM(`pcv-guest-installed-04253-r1`)의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner의 `pcv-cleanhost-*`와 fullgate route smoke VM뿐이다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. guest 인증 정보는 orchestrator 호출 직전에 `PSCredential`로 만든다.
- evidence와 artifact는 새 이름(`-04290`, `-04289-04290`)으로 쓴다. 0.42.89 evidence, facts, artifact는 바꾸지 않는다.
- baseline package는 `artifacts/admin-smoke-package-20261004-04289`(MSI, payload)이고, baseline update ZIP·catalog는 3a 도구 출력 `artifacts/update-package-check-20261004/20261004-04289`를 쓴다.
- orchestrator 입력: base VHD `D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\image-cache\windows-server-2022-eval-vhd\20348.5622-20260930.vhd`, switch `Default Switch`, MSIX template `packaging/windows-desktop-node/msix/template-layout`, 설치 manifest `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json`, 서명 thumbprint `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`.
- ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`를 쓴다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다), clean-host 180분.

## Task 0: 출발

- [x] `release-train.json`의 queue 행(PR #48)을 train `0.42.90-admin-smoke`의 `carriages`로 옮기고 `status=running`. 이 계획과 campaign.

실행 기록(2026-10-05): 출발 `2026-10-05T21:06:00+09:00`, source `a66a8cd`, carriages `[48]`, queue 비움.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.90-admin-smoke -MsiProductVersion 0.42.90 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261005-04290`, 그 뒤 `New-PcvAdminSmokeUpdatePackage.ps1`로 update ZIP, catalog, `package-facts.json`. train-facts 입력을 만들고 package 문서를 `train-facts`와 `train-evidence --write`로 쓴다. 검증 `train-evidence --check`. 로컬 commit.

실행 기록(2026-10-05): HEAD `0bcc328`에서 build `44`초, MSI `54277baa…`, payload `e6cec1d0…`(`8`개), update ZIP `1c91de5c…`, catalog `3169a44f…`, Host/CLI `+0bcc328`. 입력 `docs/ga-ready/trains/0.42.90-admin-smoke.train-facts-input.json`으로 package 문서(생성 `19`, 서술 `4`) `written`, `check` `current`. evidence `admin-smoke-package-2026-10-05-04290`. 도구가 `build_utc`를 culture 형식(`10/05/2026 12:07:30`)으로 썼다(report-only).

## Task 2: pair 실행

- [x] 설치본·서비스·보존 VM·switch 상태를 먼저 본다. orchestrator `-PlanOnly` `ok=true` 뒤 `-Execute`(campaign `manual-admin-campaign-20261005-04289-04290`). 여섯 bucket PASS, closed descriptor, `observations.json`에 `observation_error` 없음. FAIL이면 restoration 결과를 보고하고 정차한다. 실행 기록과 로컬 commit.

실행 기록(2026-10-05): 시작 상태는 설치본 `0.42.89`(`+b7fe7b2`, 3b-r2 fullgate build), service Running/Automatic, Web `200`, 보존 VM Off, `Default Switch`. guest 인증 정보는 clean-host runner 기본값을 실행 스크립트가 runner 소스에서 읽어 `PSCredential`로 만들었고 명령줄과 출력에 남기지 않았다. `-PlanOnly` `ok=true`(host mutation 없음) 뒤 `-Execute`(`12:13Z`경 시작). baseline catalog로 `0.42.89`에 맞춘 뒤 여섯 bucket(readiness, Update/Rollback, clean-host Windows Update, Burn, MSIX, ops summary) 모두 PASS, closed descriptor 생성, restoration 불필요. `observations.json` `15`개 항목 모두 `observation_error` 없음. 끝 상태: 설치본 clean `0.42.90`(`+0bcc328`), ARP `{8AE1F6ED-2174-4484-A57B-E60378BB1F72}` `0.42.90` 1개, Web `200`, 보존 VM Off. artifact `artifacts/manual-admin-campaign-20261005-04289-04290`.

## Task 3: pair 문서

- [ ] `train-facts`에 pair 문서(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 더해 렌더한다. 검증 `train-evidence --check`, `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

## Task 4: fullgate

- [ ] clean `0.42.90` 설치본 위에서 full admin host mutation gate(`full-admin-host-mutation-gate-20261005-04290`)를 돈다. 사후 검사(build commit, 같은 version ARP 1개, MSI log) 통과. 실행 기록과 로컬 commit.

## Task 5: installed current-card

- [ ] `installed-operator-surface-current-card-20261005-04290` 캡처, `status=pass`, `not-promoted`. 실행 기록과 로컬 commit.

## Task 6: fullgate·current-card 문서와 pair evidence merge

- [ ] `train-facts`에 fullgate, current-card 문서를 더해 렌더한다. clean HEAD 종료 검증 뒤 push, PR, green CI 뒤 merge.

## Task 7: Lane 3

- [ ] functional carry-forward, single-root consume, Task 6 merge의 main push evidence를 facts로 렌더하고 current-card 승격 값을 넣는다. `current-evidence.json`, 입력 `docs/ga-ready/trains/0.42.90-admin-smoke.lane3-spec-input.json`과 `lane3-spec --write`로 승격 spec, 문서 도구 dry-run·`-Apply`·`-Check`. `release-train.json` `operational_current=0.42.90-admin-smoke`, train `status=promoted`. 로컬 commit.

## Task 8: 종료와 Lane 3 merge

- [ ] clean HEAD 종료 검증, campaign 닫기, push, PR, green CI 뒤 merge.

## Nonclaims

- train은 internal admin-smoke 범위다. public trusted signing과 external stable publication을 주장하지 않는다.
