# 0.42.78 Lane 3 operational promotion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `docs/ga-ready/current-evidence.json`의 operational current를 `0.42.77-admin-smoke`에서 `0.42.78-admin-smoke`로 승격하고, 생성 문서·수기 문서·계약 테스트를 같은 current로 맞춘다.

**Architecture:** 2026-09-20 `0.42.77` 승격(`a842ede`)과 같은 carry-forward 방식이다. host mutation은 하지 않는다. 사용자 결정(2026-09-27): P0 candidate feature `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)의 `0.42.75` actual-VM PASS와 functional evidence를 carry-forward한다.

**Tech Stack:** JSON evidence ledger, `Update-PcvCurrentEvidenceDocs.ps1`, C# / .NET 10 xUnit, Pester 5

## 승격 근거

기준: `origin/main` `3c677a4` (PR #11 merge). branch `docs/04278-lane3-promotion`.

| 평면 | evidence | 결과 |
| --- | --- | --- |
| package | `admin-smoke-package-2026-09-25-04278` (clean MSI `c3390c1e…`) | PASS |
| full admin host mutation | `full-admin-host-mutation-gate-2026-09-27-04278-r2-hostmutation` (batch `full-admin-host-mutation-gate-20260927-04278-r2`, MSI `0856d07e…`, payload `2dfabb93…`, provenance `0de176f`) | PASS |
| manual-admin pair | `manual-admin-campaign-descriptor-2026-09-25-04277-04278` (plan-only descriptor, runner `6`) | PASS |
| clean-host / Burn / MSIX / update-rollback | `2026-09-25` `04277-04278` evidence | PASS |
| installed current-card | `installed-operator-surface-current-card-2026-09-27-04278` | PASS, not promoted |
| feature qualification | `config/desktop-node-feature-evidence-ledger.json` candidate `4`개 모두 `0.42.75` PASS, ledger 변경 없음 | carry-forward |
| functional | `0.42.77` carry-forward 체인(원본 `functional-correctness-actual-host-validation-2026-08-27-04275`) | carry-forward |

0.42.77 뒤에 추가된 P1-9~P2-15 기능은 feature evidence ledger의 candidate가 아니므로 ADR-0015 blocker가 아니다. 이 승격은 그 기능들의 actual-VM PASS를 주장하지 않는다.

## Global Constraints

- current evidence에는 PASS 근거만 기록한다. 09-26 FAIL과 09-27 첫 실행(제한)은 historical evidence로 둔다.
- host mutation, 설치본 변경, public trusted signing, external stable publication은 범위 밖이다.
- 생성 문서(`CurrentEvidenceVerifier.OwnedRelativePaths` `7`개)는 손으로 고치지 않고 `Update-PcvCurrentEvidenceDocs.ps1`로 만든다.
- `config/pcv-development-policy-contract-spec-v1.json`의 `source_files`/`legacy_files`에 있는 파일(`AGENTS.md` 포함)이 바뀌면 spec SHA와 `ExpectedSpecSha256`을 같은 task에서 갱신한다.
- task 하나가 checkpoint 하나다. Lane 3 한도(30분, tool batch 12회)를 따른다. commit은 `commit_policy`를 따르고 push/PR은 Task 5에서만 한다. merge는 별도 권한이 필요하다.

## Task 1: 승격 evidence 문서

**생성:** `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-09-27-04278-carryforward.md`
**수정:** `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-09-27-04278.md`

- [ ] `0.42.77` carry-forward 문서 형식으로 `0.42.78` functional carry-forward 문서를 쓴다. 원본 체인과 P0 candidate `4`개, 새 기능의 비주장을 적는다.
- [ ] current-card를 `promotion_ledger_status: promoted-current`, `canonical_current_evidence: 0.42.78-admin-smoke`, `canonical_current_changed: true`로 바꾸고 승격 경계 절을 갱신한다(``a842ede``가 04277 current-card에 한 방식).
- [ ] `Invoke-Pester packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`.

## Task 2: current-evidence 기록과 생성 문서

**수정:** `docs/ga-ready/current-evidence.json`, 생성 문서 `7`개, `src/DesktopNode.Verification.Tests/CurrentEvidenceVerifierTests.cs`

- [ ] `current`의 version, evidence 경로, MSI/payload SHA, provenance와 `manual_admin`의 closed baseline/target/descriptor를 `0.42.78` 값으로 바꾼다.
- [ ] `Update-PcvCurrentEvidenceDocs.ps1`로 생성 문서를 갱신하고 `-Check`로 확인한다.
- [ ] `CurrentEvidenceVerifierTests`의 기대 버전을 `0.42.78-admin-smoke`로 바꾼다.
- [ ] `dotnet test src/DesktopNode.Verification.Tests --filter CurrentEvidenceVerifierTests`.

## Task 3: 수기 문서, spec, 계약 테스트

**수정:** `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`, `packaging/windows-desktop-node/README.md`, `docs/DOCUMENTATION_INDEX.md`, `docs/FEATURE_IMPLEMENTATION_LEDGER.md`, `src/DesktopNode.Delivery.Tests/Delivery/Evidence/D2EvidenceContractVerifier.cs`, 관련 config spec, development policy spec pin
**생성:** `src/DesktopNode.Delivery.Tests/Delivery/Evidence/Pcv04278PromotionEvidenceContractTests.cs`

- [ ] 착수 시 `a842ede`의 spec/test 변경을 다시 읽고 같은 항목을 `0.42.78`로 옮긴다.
- [ ] `Pcv04277PromotionEvidenceContractTests`와 같은 형태로 `0.42.78` 승격 계약 테스트를 추가한다.
- [ ] pin된 파일이 바뀌었으면 spec SHA와 `ExpectedSpecSha256`을 갱신한다.
- [ ] `dotnet test src/DesktopNode.Delivery.Tests` 실패 `0`.

## Task 4: 종료 검증

- [ ] `Update-PcvCurrentEvidenceDocs.ps1 -Check`.
- [ ] `dotnet test src/DesktopNode.sln` 실패 `0`(clean HEAD).
- [ ] Pester `PcvModuleSizeRatchet`, `PcvCSharpArchitectureGapRegistry`, `PcvAdminSmokeEvidenceDocs`.
- [ ] `npm run test:required --prefix web`.
- [ ] clean HEAD에서 Required CI 네 shard.

## Task 5: push와 PR

- [ ] branch를 push하고 `main` 대상 PR을 만든다. PR 본문 끝에 Claude Code 표기를 붙인다.
- [ ] CI 결과를 확인한다. merge는 사용자 권한 뒤에만 한다.

## Nonclaims

- 새 actual-VM, host mutation, 설치본 변경을 주장하지 않는다.
- P1-9~P2-15 기능의 actual-VM PASS를 주장하지 않는다.
- public trusted signing, external stable publication을 주장하지 않는다.
