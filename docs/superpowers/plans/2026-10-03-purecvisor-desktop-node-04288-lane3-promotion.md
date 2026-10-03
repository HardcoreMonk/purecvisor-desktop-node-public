# 0.42.88 Lane 3 승격 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `0.42.88-admin-smoke`를 operational current로 승격한다. package, pair, fullgate, current-card, managed delete Lane 2 확인은 PR #31(merge `d27086f`)에 이미 PASS로 있다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §6 순서를 따른다. evidence 문서와 `current-evidence.json`을 먼저 쓰고, 승격 spec 하나로 `Invoke-PcvLane3PromotionDocs.ps1`를 dry-run, `-Apply`, `-Check` 순서로 돌린다. branch는 `lane3/04288-promotion-20261003`(`origin/main` `d27086f` 기준)이고 PR 하나로 merge한다.

**Tech Stack:** Markdown evidence, JSON, `Invoke-PcvLane3PromotionDocs.ps1`, `New-PcvManualAdminCampaignDescriptor.ps1`, C# xUnit

## 사용자 결정 (2026-10-03)

승인 원문: `1,2` (0.42.88 pair 최종 보고의 다음 승인 1 "branch push/PR/merge", 2 "`0.42.88-admin-smoke` Lane 3 승격(`current-evidence.json` 쓰기 포함)"에 대한 답).

| 항목 | 결정 |
| --- | --- |
| PR #31 merge | 완료(`d27086f`) |
| Lane 3 승격 | `0.42.88-admin-smoke`, `current-evidence.json` 쓰기 포함 |
| push/PR/merge | PR 하나, green CI 뒤 merge |
| 승인 밖 | host mutation, public trusted signing, external stable publication |

## Global Constraints

- host mutation을 하지 않는다. consume과 descriptor는 `-PlanOnly`다.
- evidence는 새 파일로 쓴다. 승격 시점 current-card evidence의 머리말을 `promoted-current`로 바꾸는 것은 0.42.84·0.42.86·0.42.87 선례를 따른다.
- 한도: Lane 3 30분·tool batch 12회.

## Task 1: 승격 evidence 문서

- [x] functional carry-forward `functional-correctness-actual-host-validation-2026-10-03-04288-carryforward`.
- [x] manual-admin single-root consume: 여섯 bucket summary JSON 7개를 `artifacts/manual-admin-campaign-20261003-04287-04288/`로 복사하고 `consume-manifest.json`, descriptor `-PlanOnly`(batch id `manual-admin-campaign-descriptor-20261003-04287-04288-consume`), evidence `manual-admin-campaign-2026-10-03-04287-04288`.
- [x] PR #31 merge 뒤 main push evidence `public-boundary-ci-main-push-2026-10-03-04288-pr31-postmerge-pass`.
- [x] current-card evidence `installed-operator-surface-current-card-2026-10-03-04288` 머리말을 `promoted-current`로 바꾼다.

실행 기록(2026-10-03): functional carry-forward(0.42.75 PASS), consume(JSON `7`개, manifest `897faf7d…`, descriptor `-PlanOnly` `overall_status=pass` `6/6`, host mutation 없음), main push evidence(`d27086f`, Public Boundary run `37122873756`, Development Gates run `37122873755` success), current-card 머리말 `promoted-current`. campaign 개설 commit `781510c`에 들어간 이 계획 파일은 인코딩 오류로 비어 있었고, 이 task에서 내용을 다시 썼다.

## Task 2: current-evidence와 승격 문서 도구

- [x] `docs/ga-ready/current-evidence.json` current를 `0.42.88-admin-smoke`로 쓴다.
- [x] 승격 spec `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04288.json`으로 `Invoke-PcvLane3PromotionDocs.ps1` dry-run, `-Apply`, `-Check`.
- [x] `CurrentEvidenceVerifierTests` 기대 버전, `DOCUMENTATION_INDEX`·`FEATURE_IMPLEMENTATION_LEDGER` operational 줄을 맞춘다.

실행 기록(2026-10-03): `current-evidence.json` current `0.42.88-admin-smoke`(operational MSI `32b35113…`, payload `163ece95…`, provenance `47ff198`, descriptor `…-consume`). 승격 spec `lane3-promotion-docs-spec-04288.json`(0.42.87 fixture에서 값만 바꿔 생성, 옛 값 잔여 없음)으로 dry-run(`stale`/`planned`), `-Apply`, `-Check`(여섯 단계 모두 `current`/`ok`). 수기 정렬: `CurrentEvidenceVerifierTests` 한 줄, `DOCUMENTATION_INDEX` 권위 줄, `FEATURE_IMPLEMENTATION_LEDGER` operational 줄.

## Task 3: 종료와 merge

- [x] clean HEAD에서 `dotnet test src/DesktopNode.sln`, Pester, `npm run test:required --prefix web`, Required CI 네 shard.
- [x] campaign을 닫고 push, PR, green CI 뒤 merge.

실행 기록(2026-10-03, clean HEAD `11d4841`): `dotnet test src/DesktopNode.sln` 실패 `0`(Delivery `752`, Verification `557`, Api `488`, HyperV `255`, Host `216`, Contracts `200`, Cli `179`, Runtime `129`, Service `11`). Pester 네 종 `151/151`. `npm run test:required --prefix web` exit `0`. Required CI shard `web`, `delivery`, `installer-policy`는 첫 실행에서 `ok=true`. `dotnet` shard는 첫 실행에서 `127.0.0.1:7666` listener 등록 충돌로 실패했다(직전 솔루션 테스트 프로세스의 연결이 `FIN_WAIT_2`/`CLOSE_WAIT`로 남아 있었다). 다시 실행해 `ok=true`, `plan_only=false`로 통과했다. 이 commit 뒤 PR을 열고 CI green이면 merge한다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- 승격은 internal admin-smoke 범위다.
