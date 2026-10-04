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
- [x] 2c clean-host(baseline `0.42.88` clean MSI, target update ZIP).
- [x] 2d Burn(제품 Update로 target에 맞추고 15초 뒤).
- [x] 2e MSIX(`0.42.88 → 0.42.89`).
- [x] 2g descriptor `-PlanOnly`.

실행 기록 2a(2026-10-04): 설치본이 baseline `0.42.88`(fullgate build `+47ff198`)이라 설치 변경 없이 실행했다. campaign `manual-admin-campaign-20261004-04288-04289`, `ok=true`, `ready-current-baseline-target-package-pair`, 설치본 일치, baseline MSI `81ef8527…`, target MSI `e4574861…`, host mutation 없음. summary SHA `489958e1…`.

실행 기록 2f(2026-10-04): CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, VM `1`개, token 형태 문자열 `0`개. summary SHA `f9d129f4…`. evidence `installed-runtime-ops-summary-2026-10-04-04288`.

실행 기록 2b(2026-10-04): Update(`11`단계)와 Rollback(`7`단계) 모두 exit `0`, `ok=true`. update 직후 Host `+b463903`, Web `200`. 최종 manifest `0.42.88`, failed `0.42.89`, service Running, 재부팅·VM 변화 없음. evidence `product-update-rollback-2026-10-04-04288-04289`.

실행 기록 2c(2026-10-04): 약 `156`초, exit `0`. base `current-base`(UBR `5622`), Windows Update 대상 `0`개. install, update, rollback 모두 exit `0`, 최종 manifest `0.42.88`, Web `200`, VM 삭제됨. evidence `internal-clean-host-install-update-rollback-smoke-2026-10-04-04288-04289`.

실행 기록 2d(2026-10-04): 제품 Update로 `0.42.89`에 맞추고 15초 기다린 뒤 실행했다. build, install, repair, remove, 복구 모두 exit `0`(`32`초). 최종 ARP `0.42.89` `{0E27390F-…}` 1개, Host `+b463903`, Running/Automatic, Web `200`. evidence `burn-bootstrapper-lifecycle-smoke-2026-10-04-04289`.

실행 기록 2e(2026-10-04): 세 자리 버전으로 한 번에 PASS(`20`초). pack·sign·verify `0`, install·update·remove 통과, smoke 패키지와 서비스 없음, manifest `0.42.89` 유지. evidence `msix-package-lifecycle-smoke-2026-10-04-04288-04289`.

실행 기록 2g(2026-10-04): `overall_status=pass`, runner `6/6`, missing `0`, not-pass `0`, host mutation 없음, next candidate `0.42.89-admin-smoke`. evidence `manual-admin-campaign-descriptor-2026-10-04-04288-04289`.

## Task 3: fullgate

- [x] clean `0.42.89` 설치본 위에서 시작한다. 사후 검사(build commit, 같은 version ARP 1개) 통과.

실행 기록(2026-10-04): clean `0.42.89`(`{0E27390F-…}`) 위에서 시작, 약 `329`초, 두 step 모두 exit `0`. 사후 build commit 검사 `a780928` 일치, 같은 version ARP `{CD234EF2-…}` 1개, fullgate MSI `fe5677ff`, payload `e4f9387c`. route smoke `vm delete`의 `storage_cleanup`이 디스크와 디렉터리를 지웠다. evidence `full-admin-host-mutation-gate-2026-10-04-04289-hostmutation`.

## Task 4: installed current-card

- [x] `status=pass`, `not-promoted`.

실행 기록(2026-10-04): `status=pass`, CLI `3/3`, Web `2/2`, ARP `0.42.89` 1개, 설치본 Host/CLI가 fullgate payload와 같음, 테스트 VM `0`, secret 없음, `not-promoted`. summary SHA `a3d66257…`. evidence `installed-operator-surface-current-card-2026-10-04-04289`.

## Task 5.1: Lane 2 probe `cli.json-errors`

- [x] 설계 §3의 네 항목을 설치본 `pcvcli`로 확인한다.

실행 기록(2026-10-04): 설치본 `pcvcli` `+a780928`로 세 case를 실행했다. case 1 exit `1`과 stdout API envelope(`PCV_VM_NOT_FOUND`)과 stderr `code=` 줄, case 2 exit `2`와 `PCV_CLI_USAGE`, case 3 table exit `1`과 빈 stdout, token 형태 `0`개. 11개 check 모두 통과. evidence `lane2-cli-json-errors-2026-10-04-04289`.

## Task 6: pair evidence PR과 merge

- [x] clean HEAD 종료 검증 뒤 push, PR, green CI 뒤 merge.

실행 기록(2026-10-04, clean HEAD `ceb7b77`): `dotnet test src/DesktopNode.sln` 실패 `0`(Delivery `758`, Verification `557`, Api `488`, HyperV `255`, Host `216`, Contracts `200`, Cli `183`, Runtime `129`, Service `11`). Pester 네 종 `151/151`. `npm run test:required --prefix web` exit `0`. Release build 뒤 Required CI 네 shard 모두 `ok=true`, `plan_only=false`(shard를 솔루션 테스트보다 먼저 돌렸다). 이 기록 commit 뒤 push, PR, green CI 뒤 merge한다.

## Task 7: Lane 3

- [x] functional carry-forward, single-root consume, Task 6 merge의 main push evidence, current-card 머리말 `promoted-current`.
- [x] `current-evidence.json`, 승격 spec `lane3-promotion-docs-spec-04289.json`으로 문서 도구 dry-run, `-Apply`, `-Check`. descriptor chain의 다음 출발 조건 값은 `release-train-departure-after-04289`.
- [x] `release-train.json` `operational_current=0.42.89-admin-smoke`, train `status=promoted`.
- [x] `CurrentEvidenceVerifierTests`, `DOCUMENTATION_INDEX`, `FEATURE_IMPLEMENTATION_LEDGER` 정렬.

실행 기록(2026-10-04): branch `lane3/04289-promotion-20261004`(`origin/main` `8631947`, PR #35 merge 기준). functional carry-forward(0.42.75 PASS), single-root consume(JSON `7`개, manifest `52062a3b…`, descriptor `-PlanOnly` `overall_status=pass` `6/6`, host mutation 없음), main push evidence(`8631947`, Public Boundary run `37171644656`, Development Gates run `37171644667` success), current-card 머리말 `promoted-current`. `current-evidence.json` current `0.42.89-admin-smoke`(operational MSI `fe5677ff…`, payload `e4f9387c…`, provenance `a780928`, descriptor `…-consume`). 승격 spec `lane3-promotion-docs-spec-04289.json`(descriptor chain 다음 출발 조건 `release-train-departure-after-04289`)으로 dry-run, `-Apply`, `-Check` 모두 통과. 첫 dry-run은 `functional_note`의 backtick 때문에 `PCV_PROMOTION_INDEX_INVALID`였고 backtick을 빼고 다시 돌렸다. `release-train.json` `operational_current=0.42.89-admin-smoke`, train `status=promoted`. 수기 정렬: `CurrentEvidenceVerifierTests`, `DOCUMENTATION_INDEX` 권위 줄, `FEATURE_IMPLEMENTATION_LEDGER` operational 줄.

## Task 8: 종료와 Lane 3 merge

- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Nonclaims

- train은 internal admin-smoke 범위다. public trusted signing과 external stable publication을 주장하지 않는다.
