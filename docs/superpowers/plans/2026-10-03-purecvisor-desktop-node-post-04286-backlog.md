# 0.42.86 이후 backlog Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `docs/project-status-audit-2026-10-03.md` §10 권고 1~5를 실행한다.

**Architecture:** 제품 동작 변경은 Task 3의 reconcile 안내 문구 기본값뿐이다. Task 2는 관리자 smoke 도구에 읽기 전용 preflight를 넣는다. Task 1은 CI runner를 probe branch로 확인한다. Task 4는 feature evidence ledger 모델 확장을 결정하고 적용한다. Task 5는 저장소 위생이다. host mutation과 Lane 2/3은 이 계획 밖이다.

**Tech Stack:** GitHub Actions, PowerShell / Pester 5, C# / .NET 10 xUnit, JSON schema, Markdown

## 사용자 결정 (2026-10-03)

승인 원문: `1,2,3,4,5 전부 모두 승인` (감사 §10 표 순위 1~5에 대한 답). 순위 6(Lane 2 actual-VM, 다음 package pair)은 승인 밖이다.

| 항목 | 결정 |
| --- | --- |
| 1 Ubuntu 26 runner | probe branch를 push해 `ubuntu-26.04`에서 web job과 Public Boundary job을 돌린다. 결과로 `ubuntu-latest` 유지 또는 pin을 정한다. probe branch는 끝나면 지운다 |
| 2 같은 version 재빌드 | 설계 권고 C(읽기 전용 preflight)를 구현한다 |
| 3 reconcile 안내 문구 | 비대상 operation의 기본값 `rename`을 없앤다 |
| 4 feature evidence ledger | 모델 확장 여부를 결정하고 적용한다 |
| 5 저장소 위생 | PR #5, 병합된 worktree, 병합된 원격 branch, 로컬 `main` fast-forward, `artifacts/` 정리 |

## Global Constraints

- host mutation(MSI, service, Hyper-V VM, firewall, Event Log), current-evidence write, Lane 2/3을 하지 않는다.
- 새 `Add-Type`/P/Invoke/native ACL/installer handoff가 필요하면 멈춘다.
- `config/pcv-development-policy-contract-spec-v1.json`의 `source_files`/`legacy_files`에 있는 파일을 바꾸면 spec SHA와 `ExpectedSpecSha256`을 같은 task에서 갱신한다.
- packaging에 새 `*.Tests.ps1` 파일을 만들지 않는다(Delivery inventory 계약). 기존 테스트 파일에 더한다.
- task 하나가 checkpoint 하나다(30분, tool batch 18회). 캠페인 러너(`pcv-campaign-runner-v1`)로 실행하고 commit은 `commit_policy`를 따른다.
- push는 Task 1 probe branch와 Task 5(원격 branch 삭제, PR #5)에만 쓴다. 이 backlog branch의 push/PR/merge는 계획 밖이다.
- 범위 밖 발견은 report-only다.

## Task 1: Ubuntu 26 runner probe와 결정

**수정:** probe branch `probe/ubuntu-26-runner-20261003`(병합하지 않음), 결정 기록은 `docs/project-status-audit-2026-10-03.md` 후속 절

- [x] `origin/main`에서 probe branch를 만들고, `public-boundary.yml`과 `development-gates.yml` web job의 `runs-on`을 `ubuntu-26.04`로 바꾸며 push trigger에 probe branch를 더한다.
- [x] push 뒤 두 workflow run의 결과, runner image 버전, `pwsh` 버전을 기록한다.
- [x] 둘 다 통과하면 `ubuntu-latest`를 유지한다. 실패하면 원인에 따라 이 backlog branch에서 `ubuntu-24.04` pin 또는 `pwsh` 설치 step을 넣는다(`development-gates.yml`은 검증 정책 영향을 같이 본다).
- [x] probe branch를 원격과 로컬에서 지운다.

검증: 두 workflow run 결과, `git diff --check`. workflow를 바꾸면 `dotnet test src/DesktopNode.Delivery.Tests`.

실행 기록(2026-10-03): probe `60b44e3`에서 Public Boundary run `37101948685` success(Pester `90/90`), Development Gates web job `111143061898` success. image는 Ubuntu `26.04.1 LTS` `20260927.149.1`, `pwsh` `7.6.6`. Windows shard 세 개는 workflow 파일 변경에 따른 고정 검사(`workflow-active-shell`, `source-sha`, `policy-boundaries`)로 실패했고 image와 무관하다. `ubuntu-latest`를 유지하고 workflow는 바꾸지 않았다. probe branch는 원격과 로컬에서 지웠다. 감사 문서 §12.

## Task 2: 같은 version 재빌드 preflight (설계 권고 C)

**수정:** `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`(또는 그 helper), 기존 테스트 파일, `docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md` 상태

- [x] 착수 시 smoke의 build·install 순서와 build commit 출처를 확인한다.
- [x] MSI install 전에 ARP(`HKLM` Uninstall 64/32bit)에서 같은 version `PureCVisor Desktop Node` 항목을 읽어, 있으면 명확한 오류 코드로 멈춘다. registry 읽기는 주입할 수 있게 한다.
- [x] gate 뒤 설치본 `DesktopNode.Host.exe` ProductVersion의 `+<commit>`이 gate build commit과 같은지 확인하고, 다르면 명확한 오류 코드로 멈춘다.
- [x] 설계 문서 상태를 C 구현으로 고친다. A는 계속 미결이다.

검증: 관련 Pester, `dotnet test src/DesktopNode.Delivery.Tests`, `git diff --check`. 이 호스트에서 smoke를 실행하지 않는다.

실행 기록(2026-10-03): smoke는 build 전에 provenance를 모르므로 ARP 검사는 build 전, build commit 검사는 `final-restore-install` 뒤에 둔다. gate commit 출처는 build provenance `git_commit`이고 설치본 ProductVersion은 `0.42.86-admin-smoke+b807803f…` 형식이다. packaging Pester는 C# Delivery로 이관돼 legacy 개수가 고정되어 있어 Pester `It`을 더하지 않고 C# 계약 `PcvRouteParitySameVersionPreflightContractTests` 5개를 더했다. orchestration spec의 smoke SHA와 `ExpectedSpecSha256`은 `Update-PcvContractSpecPins.ps1 -Apply`로 갱신했다. `-SelfTest` exit `0`(live ARP 항목 `1`개 읽음), Delivery `749/749`. 이 호스트에서 gate는 돌리지 않았다.

## Task 3: reconcile 안내 문구 기본값

**수정:** `src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.cs`, `src/DesktopNode.Runtime/DesktopNodeJobRuntime.Persistence.cs`, 해당 테스트

- [x] 비대상 operation 안내가 `rename`을 말하지 않도록 기본값을 중립 문구로 바꾼다.
- [x] 비대상 operation 안내를 단언하는 테스트를 Api와 Runtime 쪽에 더한다.

검증: 바뀐 프로젝트의 `*.Tests`, `git diff --check`.

실행 기록(2026-10-03): Api switch에는 `vm.rename`이 없어서 rename 경로도 기본값으로 `rename`을 받고 있었다. `vm.rename => rename`을 명시하고, rename 전용 경로 세 곳은 `vm.rename`을 넘기며, 기본값은 Api와 Runtime 모두 `mutation`으로 바꿨다. `ApiReconcileClassificationTests`의 `vm.guest.exec` 비대상 사례와 Runtime `NonTargetReconciliationGuidanceNamesNoSpecificMutation`이 "confirm whether the mutation applied"를 단언하고, rename 미확인 사례는 계속 "confirm whether the rename applied"를 단언한다. Runtime `129/129`, Api `488/488`.

## Task 4: feature evidence ledger 모델 확장 결정

**수정:** 결정 기록, `config/desktop-node-feature-evidence-ledger.json`과 schema, `docs/FEATURE_IMPLEMENTATION_LEDGER.md`, 관련 테스트

- [ ] 착수 시 ledger JSON을 읽는 곳(C#, PowerShell, web)을 모두 찾는다.
- [ ] 결정: 비후보 feature에 stage별 관측 evidence를 담는 선택 필드를 더할지, `not-assessed`를 유지할지. 모든 reader가 선택 필드를 견디고 evaluator(`candidate_required`만 판단)가 그대로면 확장한다. reader 변경이 크거나 새 설계가 필요하면 결정 기록만 남기고 멈춘다.
- [ ] 확장하면 PASS evidence가 있는 stage만 기록한다(예: `pcv.vm.clone` 0.42.77 actual-VM, `pcv.vm.media-eject` 0.42.86 actual-VM). `candidate_required`와 promotion 판정은 바꾸지 않는다.

검증: `PcvFeatureEvidencePromotion` Pester, `dotnet test src/DesktopNode.Delivery.Tests`, Verification tests, `git diff --check`.

## Task 5: 저장소 위생

**수정:** git 저장소 상태, 감사 문서 §8 후속 기록

- [ ] PR #5: `main`을 합쳐 CI를 다시 돌리고 green이면 merge한다. 충돌이나 red면 닫고 사유를 남긴다.
- [ ] 병합된 worktree를 지운다: `pcv-public-wt-descriptor-chain`, Temp `pcv-fullgate-04286`. 미병합·dirty인 `04275-stage1-immutable-preflight`(5파일)와 `04277-manual-admin-promotion`(8파일)은 남긴다.
- [ ] `origin/main`에 병합된 원격 branch를 지운다. 로컬은 `git branch -d`로 병합된 것만 지운다.
- [ ] 로컬 `main`을 `origin/main`으로 fast-forward한다.
- [ ] `artifacts/`: tracked 파일이 참조하는 dir과 clean-host base가 쓰는 dir은 남기고, 참조 없는 dir만 지운다. 목록과 크기를 기록한다.

검증: `git worktree list`, `git branch -r`, `gh pr view 5`, 남긴 artifacts 참조 재확인.

## Task 6: 종료 검증

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln` 실패 `0`.
- [ ] Pester `PcvAdminSmokeEvidenceDocs`와 바뀐 영역 suite.
- [ ] `npm run test:required --prefix web`.
- [ ] clean HEAD에서 Required CI 네 shard.
- [ ] campaign `next_task`를 `null`로 두고 `next_step`을 완료 상태로 고친다.

## 계획 밖 (승인 필요)

- 이 backlog branch의 push/PR/merge
- 설계 권고 A(`AllowSameVersionUpgrades`)
- Lane 2 actual-VM 검증, 다음 package pair
- dirty worktree 두 개, 별도 clone `D:\data\projects\codex-zone\pcv-04286-fullshard`

## Nonclaims

- runner probe는 Ubuntu 26 image의 한 시점 관측이다. 이후 image 갱신을 보증하지 않는다.
- preflight는 잘못된 PASS를 막을 뿐 같은 version 재설치 동작을 고치지 않는다.
- feature stage 기록은 새 검증 실행이 아니다.
