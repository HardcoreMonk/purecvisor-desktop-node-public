# 0.42.84 Lane 3 승격과 `vm.list` readback 구현 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 두 가지를 한다.
1. PASS evidence가 모인 `0.42.84-admin-smoke`를 operational current로 승격한다(Lane 3).
2. `vm.list` readback 확장 설계(`docs/superpowers/specs/2026-09-30-purecvisor-desktop-node-vm-list-readback-extension-design.md`)를 구현한다(Lane 1).

**Architecture:** Task 1~5는 0.42.83 승격(`2026-09-29-purecvisor-desktop-node-04283-lane3-promotion.md` Task 5~6)과 같은 순서다. 문서 단계는 `Invoke-PcvLane3PromotionDocs.ps1`가 맡는다. Lane 3 branch는 `lane3/04284-promotion-20260930`(`origin/main` `4afaed7` 기준)이다. Task 6~9는 Lane 3 PR이 merge된 뒤 `origin/main`에서 새 branch `feat/vm-list-readback-20260930`을 만들어 진행한다. 머지하지 않은 branch 위에 쌓지 않는다.

**Tech Stack:** PowerShell 7 + Pester 5, C# / .NET 10 xUnit, TypeScript Web Console

## 사용자 결정 (2026-09-30)

| 항목 | 결정 |
| --- | --- |
| 승인 | "전부 승인 합니다". 0.42.84 pair 보고의 다음 단계 세 가지(PR #22 merge, 0.42.84 Lane 3 승격, `vm.list` 확장 설계 구현) |
| PR #22 | merge 완료(`4afaed7`) |
| push/PR/merge | commit마다 push, branch마다 PR 하나, CI 통과 뒤 merge |
| current-evidence 쓰기 | Task 3에서 한다 |
| 승인 밖 | `vm.list` 구현의 설치본 검증(새 package pair `0.42.85`), public trusted signing, external stable publication |

## Global Constraints

- evidence는 새 파일로만 쓴다. 예외는 0.42.83 승격과 같은 current-card의 `promoted-current` 표기다.
- `current-evidence.json`에는 PASS 근거만 적는다.
- 호스트 mutation은 하지 않는다. Lane 3는 문서와 계약만 바꾼다.
- 한도:
  - Lane 3: 30분, tool batch 12회
  - Lane 1: 30분, tool batch 18회
- 같은 원인으로 3번 실패하거나, 범위 밖 설계가 필요하거나, 권한이 거부되면 멈춘다.
- public trusted signing과 external stable publication은 주장하지 않는다.

## Task 1: 승격 evidence (Lane 3, 5a)

- [x] functional carry-forward evidence `functional-correctness-actual-host-validation-2026-09-30-04284-carryforward`를 쓴다. 04283 carry-forward에 0.42.84의 fullgate route smoke, current-card, 개발 완료 Lane 2 probe를 더한다.
- [x] single-root consume을 만든다. 여섯 bucket summary를 `artifacts/manual-admin-campaign-20260930-04283-04284`에 모으고, consume descriptor `manual-admin-campaign-descriptor-20260930-04283-04284-consume`(`-PlanOnly`)를 만든다. consume evidence `manual-admin-campaign-2026-09-30-04283-04284`를 쓴다.
- [x] current-card evidence를 `promoted-current`로 바꾼다.

실행 기록(2026-09-30): carry-forward 원본 SHA(`a907535a…`)와 feature ledger(`bb15f66` 뒤 변경 없음)를 다시 확인했다. consume은 여섯 bucket JSON `16`개를 `artifacts/manual-admin-campaign-20260930-04283-04284`에 모았다. consume descriptor `manual-admin-campaign-descriptor-20260930-04283-04284-consume`(`-PlanOnly`)는 runner `6/6` pass, missing `0`, not_pass `0`이다. descriptor 도구는 bucket 경로를 명시적으로 받는다(`-CampaignArtifactRoot`만 주면 인자 오류). current-card evidence는 0.42.83 형식 헤더로 다시 쓰고 `promoted-current`로 바꿨다. 검증: `PcvAdminSmokeEvidenceDocs` Pester 통과.

## Task 2: main push evidence (Lane 3, 5a)

- [x] PR #22 merge `4afaed7`의 Development Gates와 Public Boundary run이 success인지 확인하고 `public-boundary-ci-main-push-2026-09-30-04284-pr22-postmerge-pass`를 쓴다.

실행 기록(2026-09-30): PR #22 merge `4afaed7`의 Development Gates `36697433260`(네 shard)와 Public Boundary `36697433373`(job `109828645222`)가 success다. `aab0bc1..ee90e0e..4afaed7` 사이에 product payload 경로 변경이 없음을 확인했다. evidence는 `public-boundary-ci-main-push-2026-09-30-04284-pr22-postmerge-pass`다.

## Task 3: current-evidence와 승격 문서 (Lane 3, 5b)

- [x] `current-evidence.json`을 `0.42.84`로 쓴다(clean MSI `12a582ef…`, operational MSI `f9e1e341…`, payload `77481bdb…`, provenance `ee90e0e`, manual_admin `0.42.83 → 0.42.84` consume descriptor).
- [x] 승격 spec `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04284.json`을 04283 견본 구성대로 만든다. `Invoke-PcvLane3PromotionDocs.ps1`를 dry-run, `-Apply`, `-Check` 순서로 실행한다.
- [x] `CurrentEvidenceVerifierTests`의 기대 버전을 바꾼다.

실행 기록(2026-09-30): `current-evidence.json`을 `0.42.84`로 썼다(`operator_surfaces` 한 줄 서식은 원본대로 유지). 승격 spec `lane3-promotion-docs-spec-04284.json`은 04283 견본과 key 구성이 같다(descriptor key `45`, ledger head `4`, 행 supersede `5`와 replace `3`, index 절). orchestrator 결과: dry-run은 생성 블록 `stale`에 나머지 `planned`, `-Apply`는 6단계를 적용해 파일 `15`개를 바꿨고, `-Check`는 6단계 모두 `current`다. C#은 `CurrentEvidenceVerifierTests` 기대 버전 한 줄만 고쳤다. 검증: `CurrentEvidenceVerifierTests` `13/13`, Delivery `744/744`, Pester(evidence docs, manual-admin tests) `219/219`, `git diff --check`.

## Task 4: 정렬과 전체 검증 (Lane 3, 5c)

- [ ] `DOCUMENTATION_INDEX`와 `FEATURE_IMPLEMENTATION_LEDGER`의 current 줄을 `0.42.84`로 정렬한다.
- [ ] 검증: `dotnet test src/DesktopNode.sln`(clean tree), Pester(`packaging/windows-desktop-node/tests`, `manual-admin-tests`), `npm run test:required --prefix web`, `Update-PcvContractSpecPins.ps1 -Check`, orchestrator `-Check`, `git diff --check`

## Task 5: PR과 merge (Lane 0)

- [ ] PR을 열고 CI 통과를 확인한 뒤 merge한다. main push CI를 확인한다.

## Task 6: `vm.list` `dvd_media` (Lane 1)

- [ ] `origin/main`에서 `feat/vm-list-readback-20260930`을 만든다.
- [ ] `GetStorageSummaries`가 `ResourceSubType`을 읽는다. VHD는 `storage[]`에, `Virtual CD/DVD Disk`는 새 `dvd_media[]`에 담는다. model, 매핑, HyperV 테스트를 더한다.

## Task 7: 내부 read operation `vm.disk.inspect` (Lane 1)

- [ ] clone provider의 `GetVirtualHardDiskSettingData` helper를 공용화한다. `vm.disk.inspect`(`{name, path}` → `{path, max_internal_size_bytes, disk_type}`)를 domain, dispatch, provider catalog와 Api invoker 허용 목록에 등록한다. `path`가 그 VM의 `storage[]`에 없으면 `PCV_VM_DISK_NOT_FOUND`다.

## Task 8: reconcile 전환 (Lane 1)

- [ ] `vm.attach`/`vm.eject`를 비대상에서 대상으로 옮기고 `dvd_media` 판정을 구현한다. `vm.disk-resize` 판정은 `vm.disk.inspect` bytes로 바꾼다. 분류 계약(대상 `28`, 비대상 `17`)과 테스트를 갱신한다.

## Task 9: Web 표시와 종료 (Lane 1)

- [ ] VM detail Storage 행에 DVD media를 표시하고 browser fixture를 맞춘다.
- [ ] 종료 검증 뒤 PR, CI, merge를 한다. campaign을 닫고, `next_step`에 설치본 검증(`0.42.85` package pair)이 승인 대상이라고 적는다.

## Nonclaims

- Lane 3 승격은 internal admin-smoke 범위다. public trusted signing과 external stable publication을 주장하지 않는다.
- Task 6~9는 소스 구현이다. 설치본과 actual-VM 동작은 주장하지 않는다.
