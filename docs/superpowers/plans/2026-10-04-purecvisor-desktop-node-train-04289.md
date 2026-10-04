# Release train `0.42.89` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 첫 release train. 대기열에서 고정한 PR #34(`pcvcli --json` 오류 출력)를 담은 `0.42.89-admin-smoke`를 package부터 Lane 3까지 한 바퀴 돌려 operational current로 승격한다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 train task 순서를 따른다. branch는 `train/04289-20261004`(`origin/main` `d6711f3` 기준)다. pair evidence PR을 먼저 merge하고, Lane 3는 그 merge 위의 branch에서 PR 하나로 merge한다.

**Tech Stack:** WiX installer `build.ps1`, `msiexec`, `Invoke-PcvDesktopNodeProduct.ps1`, manual-admin runner, `Invoke-PcvBatchSupervisor.ps1`, PCVCLI, `Invoke-PcvLane3PromotionDocs.ps1`

## 출발 승인 (2026-10-04)

승인 원문:

> 0.42.89 train 출발 승인: 대기열 PR #34(cli.json-errors) 고정, package build, pair host 작업(MSI 제거·설치, 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX), fullgate, current-card, Lane 2 probe cli.json-errors, Lane 3 current-evidence.json 쓰기, push/PR, green CI 뒤 merge

정차하면 이 승인은 끝난다. public trusted signing과 external stable publication은 범위 밖이다.

## 적재 변경

| PR | 변경 commit | 요약 | Lane 2 probe |
| --- | --- | --- | --- |
| #34 | `0c95852` | `pcvcli --json`이 오류를 stdout JSON envelope으로 쓴다 | `cli.json-errors` (`docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-cli-json-errors-design.md` §3) |

## Global Constraints

- 보존 VM의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner의 `pcv-cleanhost-*`와 fullgate route smoke VM뿐이다. `cli.json-errors` probe는 VM을 만들지 않는다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다.
- evidence는 새 파일로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회, clean-host 180분.
- ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`를 쓴다.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다.

## Task 0: 출발

- [x] `release-train.json`의 queue 행(PR #34)을 train `0.42.89-admin-smoke`의 `carriages`로 옮기고 `status=running`.
- [x] 이 계획과 campaign.

실행 기록(2026-10-04): 출발 `2026-10-04T11:08:04+09:00`, source `d6711f3`, carriages `[34]`, queue 비움.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.89-admin-smoke -MsiProductVersion 0.42.89 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261004-04289`. update ZIP, evidence `admin-smoke-package-2026-10-04-04289`.

실행 기록(2026-10-04): HEAD `b463903`, MSI `e4574861`, payload `01bbfae5`, update ZIP `b8526416`, evidence `admin-smoke-package-2026-10-04-04289`.

## Task 2a~2g: pair

- [x] 2a readiness `-PlanOnly`(baseline `0.42.88`, clean package `admin-smoke-package-20261003-04288`).
- [x] 2f ops summary(baseline 설치 상태).
- [x] 2b 설치본 Update와 Rollback.
- [ ] 2c clean-host(baseline `0.42.88` clean MSI, target update ZIP).
- [ ] 2d Burn(제품 Update로 target에 맞추고 15초 뒤).
- [ ] 2e MSIX(`0.42.88 → 0.42.89`).
- [ ] 2g descriptor `-PlanOnly`.

실행 기록 2a(2026-10-04): 설치본이 baseline `0.42.88`(fullgate build `+47ff198`)이라 설치 변경 없이 실행했다. campaign `manual-admin-campaign-20261004-04288-04289`, `ok=true`, `ready-current-baseline-target-package-pair`, 설치본 일치, baseline MSI `81ef8527…`, target MSI `e4574861…`, host mutation 없음. summary SHA `489958e1…`.

실행 기록 2f(2026-10-04): CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, VM `1`개, token 형태 문자열 `0`개. summary SHA `f9d129f4…`. evidence `installed-runtime-ops-summary-2026-10-04-04288`.

실행 기록 2b(2026-10-04): Update(`11`단계)와 Rollback(`7`단계) 모두 exit `0`, `ok=true`. update 직후 Host `+b463903`, Web `200`. 최종 manifest `0.42.88`, failed `0.42.89`, service Running, 재부팅·VM 변화 없음. evidence `product-update-rollback-2026-10-04-04288-04289`.

## Task 3: fullgate

- [ ] clean `0.42.89` 설치본 위에서 시작한다. 사후 검사(build commit, 같은 version ARP 1개) 통과.

## Task 4: installed current-card

- [ ] `status=pass`, `not-promoted`.

## Task 5.1: Lane 2 probe `cli.json-errors`

- [ ] 설계 §3의 네 항목을 설치본 `pcvcli`로 확인한다.

## Task 6: pair evidence PR과 merge

- [ ] clean HEAD 종료 검증 뒤 push, PR, green CI 뒤 merge.

## Task 7: Lane 3

- [ ] functional carry-forward, single-root consume, Task 6 merge의 main push evidence, current-card 머리말 `promoted-current`.
- [ ] `current-evidence.json`, 승격 spec `lane3-promotion-docs-spec-04289.json`으로 문서 도구 dry-run, `-Apply`, `-Check`. descriptor chain의 다음 출발 조건 값은 `release-train-departure-after-04289`.
- [ ] `release-train.json` `operational_current=0.42.89-admin-smoke`, train `status=promoted`.
- [ ] `CurrentEvidenceVerifierTests`, `DOCUMENTATION_INDEX`, `FEATURE_IMPLEMENTATION_LEDGER` 정렬.

## Task 8: 종료와 Lane 3 merge

- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Nonclaims

- train은 internal admin-smoke 범위다. public trusted signing과 external stable publication을 주장하지 않는다.
