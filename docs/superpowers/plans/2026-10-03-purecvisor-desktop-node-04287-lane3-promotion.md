# 0.42.87 Lane 3 승격 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `0.42.87-admin-smoke`를 operational current로 승격한다. package, pair, fullgate, current-card, Lane 2 확인은 PR #28(merge `d48de55`)에 이미 PASS로 있다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §6 순서를 따른다. evidence 문서와 `current-evidence.json`을 먼저 쓰고, 승격 spec 하나로 `Invoke-PcvLane3PromotionDocs.ps1`를 dry-run, `-Apply`, `-Check` 순서로 돌린다. branch는 `lane3/04287-promotion-20261003`(`origin/main` `d48de55` 기준)이고 PR 하나로 merge한다.

**Tech Stack:** Markdown evidence, JSON, `Invoke-PcvLane3PromotionDocs.ps1`, `New-PcvManualAdminCampaignDescriptor.ps1`, C# xUnit

## 사용자 결정 (2026-10-03)

승인 원문: `전부 승인 합니다` (0.42.87 pair 최종 보고의 다음 승인 대상 "PR #28 merge와 `0.42.87-admin-smoke` Lane 3 승격(`current-evidence.json` 쓰기 포함)"에 대한 답).

| 항목 | 결정 |
| --- | --- |
| PR #28 merge | 완료(`d48de55`) |
| Lane 3 승격 | `0.42.87-admin-smoke`, `current-evidence.json` 쓰기 포함 |
| push/PR/merge | commit마다 push, PR 하나, green CI 뒤 merge |
| 승인 밖 | host mutation, public trusted signing, external stable publication |

## Global Constraints

- host mutation을 하지 않는다. consume과 descriptor는 `-PlanOnly`다.
- evidence는 새 파일로 쓴다. 승격 시점 current-card evidence의 머리말을 `promoted-current`로 바꾸는 것은 0.42.84·0.42.86 선례를 따른다.
- 한도: Lane 3 30분·tool batch 12회.

## Task 1: 승격 evidence 문서

- [x] functional carry-forward `functional-correctness-actual-host-validation-2026-10-03-04287-carryforward`.
- [x] manual-admin single-root consume: 여섯 bucket summary JSON 7개를 `artifacts/manual-admin-campaign-20261003-04286-04287/`로 복사하고 `consume-manifest.json`, descriptor `-PlanOnly`(batch id `manual-admin-campaign-descriptor-20261003-04286-04287-consume`), evidence `manual-admin-campaign-2026-10-03-04286-04287`.
- [x] PR #28 merge 뒤 main push evidence `public-boundary-ci-main-push-2026-10-03-04287-pr28-postmerge-pass`.
- [x] current-card evidence `installed-operator-surface-current-card-2026-10-03-04287` 머리말을 `promoted-current`로 바꾼다.

실행 기록(2026-10-03): functional carry-forward(0.42.75 PASS, summary `a907535a…` 확인), consume(JSON `7`개 복사, manifest `6f0d5584…`, descriptor `-PlanOnly` `overall_status=pass` `6/6`, host mutation 없음), main push evidence(`d48de55`, Public Boundary run `37112963009`, Development Gates run `37112963018` success), current-card 머리말 `promoted-current`.

## Task 2: current-evidence와 승격 문서 도구

- [ ] `docs/ga-ready/current-evidence.json` current를 `0.42.87-admin-smoke`로 쓴다.
- [ ] 승격 spec `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04287.json`으로 `Invoke-PcvLane3PromotionDocs.ps1` dry-run, `-Apply`, `-Check`.
- [ ] `CurrentEvidenceVerifierTests` 기대 버전, `DOCUMENTATION_INDEX`·`FEATURE_IMPLEMENTATION_LEDGER` operational 줄을 맞춘다.

## Task 3: 종료와 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln`, Pester, `npm run test:required --prefix web`, Required CI 네 shard.
- [ ] campaign을 닫고 push, PR, green CI 뒤 merge.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- 승격은 internal admin-smoke 범위다.
