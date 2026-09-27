# 0.42.78 이후 backlog Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `docs/project-status-audit-2026-09-27.md` §9~10이 남긴 검증 사각지대를 Lane 1 안에서 줄인다.

**Architecture:** 제품 동작을 바꾸지 않는다. 라쳇 등록, feature ledger 기록, 설계 문서, CI runner 점검만 한다. host mutation과 Lane 2/3은 이 계획 밖이다.

**Tech Stack:** JSON fixture, C# / .NET 10 xUnit, Pester 5, Markdown

## 사용자 결정 (2026-09-27)

| 항목 | 결정 |
| --- | --- |
| 미등록 `500`줄 초과 비테스트 source `12`개 | 현재 크기로 라쳇에 등록(동결). 분해는 각자 나중 |
| feature evidence | 이미 있는 evidence stage만 `FEATURE_IMPLEMENTATION_LEDGER.md`에 기록하고 틀린 clone note를 고친다. feature evidence ledger의 `candidate_required`는 바꾸지 않는다 |
| 같은 version 재빌드 installer 문제 | 설계 문서만 먼저 쓴다. 구현은 결정 뒤 |

## Global Constraints

- 제품 동작, route 계약, 설치본, current-evidence를 바꾸지 않는다.
- `config/pcv-development-policy-contract-spec-v1.json`의 `source_files`/`legacy_files`에 있는 파일(예: `module-size-ratchet.json`)을 바꾸면 spec SHA와 `ExpectedSpecSha256`을 같은 task에서 갱신한다.
- 문서 변경 task도 관련 Pester(`PcvAdminSmokeEvidenceDocs`)와 `EfficientDevelopmentProcedureDocumentationTests`를 돌린다.
- task 하나가 checkpoint 하나다(30분, tool batch 18회). 캠페인 러너(`pcv-campaign-runner-v1`)로 실행하며 commit은 `commit_policy`를 따른다. push/PR/merge는 이 계획 밖이며 사용자가 부를 때만 한다.

## Task 1: 미등록 대형 모듈 라쳇 등록

**수정:** `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`, `DevelopmentPolicyContractVerifier.cs`(모듈 수 고정값), 필요하면 `PcvModuleSizeRatchet.Tests.ps1`, spec pin

- [ ] 착수 시 `500`줄 초과 비테스트 `.cs`/`.ts` 목록을 다시 재고, 각 파일을 현재 줄 수 `max_lines`와 `owner`, `note`로 등록한다.
- [ ] verifier의 `modules.Length` 고정값과 Pester의 모듈 수 단언을 새 개수로 맞춘다.
- [ ] 바뀐 pin 파일의 spec SHA와 `ExpectedSpecSha256`을 갱신한다.
- [ ] `dotnet test src/DesktopNode.Delivery.Tests` 실패 `0`, `Invoke-Pester PcvModuleSizeRatchet.Tests.ps1`.

## Task 2: feature evidence stage 기록

**수정:** `docs/FEATURE_IMPLEMENTATION_LEDGER.md`(생성물이면 그 원천)

- [ ] 착수 시 stage 표가 손으로 쓰는 표인지 생성물인지 확인한다.
- [ ] repo에 PASS evidence가 있는 stage만 기록한다(예: `pcv.vm.clone` actual-VM `service-plan-p1-clone-actual-vm-2026-08-29-04277-r2`). 근거가 없는 stage는 `not-assessed`로 둔다.
- [ ] `pcv.vm.clone` non-claim 문장을 evidence에 맞게 고친다.
- [ ] 관련 Pester와 Delivery 테스트.

## Task 3: 같은 version 재빌드 installer 설계

**생성:** `docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md`

- [ ] 원인(같은 version의 새 ProductCode 공존, equal-version file no-overwrite, uninstall 시 다른 client)과 09-27 관측을 적는다.
- [ ] 선택지(WiX `AllowSameVersionUpgrades`, gate build 고유 version, fullgate 전 잔여 ProductCode preflight)의 장단점과 권고안을 적는다.
- [ ] 구현은 하지 않는다.

## Task 4: Ubuntu 26 runner 영향 점검

- [ ] `.github/workflows/*.yml`의 `runs-on`과 web/public-boundary job 의존성을 읽고 영향과 권고를 감사 문서 후속 절이나 설계 메모로 기록한다. workflow는 바꾸지 않는다.

## Task 5: 종료 검증

- [ ] `dotnet test src/DesktopNode.sln` 실패 `0`(clean HEAD).
- [ ] Pester `PcvModuleSizeRatchet`, `PcvCSharpArchitectureGapRegistry`, `PcvAdminSmokeEvidenceDocs`.
- [ ] `npm run test:required --prefix web`.
- [ ] clean HEAD에서 Required CI 네 shard.

## 계획 밖 (승인 필요)

- PR `#5`, stale worktree `04275-stage1-immutable-preflight`, merge된 원격 branch 정리
- P1-8~P2-15 actual-VM 검증과 다음 package pair(Lane 2)
- push/PR/merge

## Nonclaims

- 라쳇 등록은 분해가 아니다. 등록된 파일의 크기를 줄였다고 주장하지 않는다.
- feature stage 기록은 새 검증 실행이 아니다.
