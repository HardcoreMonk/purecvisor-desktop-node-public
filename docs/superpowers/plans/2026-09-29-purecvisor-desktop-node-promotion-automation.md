# Lane 3 승격 수작업 자동화 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Lane 3 승격마다 반복하는 문서·계약 수작업(descriptor chain, ledger key와 status table 행, index 승격 절, 버전별 C# verifier, spec SHA pin)을 생성기와 데이터 기반 검증으로 바꿔, 승격 한 번에 필요한 수기 편집과 commit 수를 줄인다.

**Architecture:** 입력은 `docs/ga-ready/current-evidence.json`과 승격 spec JSON 하나다. 생성기는 모두 기본 dry-run, `-Apply`, `-Check` 세 모드를 가진다. 완료 기준은 직전 실제 승격(0.42.78)을 replay해 해당 commit 결과와 byte 단위로 일치시키는 것이다. 이미 있는 `Update-PcvCurrentEvidenceDocs.ps1`(생성 블록 target `8`개)은 바꾸지 않는다.

**Tech Stack:** PowerShell 7 tool + Pester 5, C# / .NET 10 xUnit (`DesktopNode.Delivery.Tests`)

## 배경: 0.42.78 승격 수작업 실측

기준: `main` `c7eecd0`. 0.42.78 승격은 commit 네 개(`064f6f2`, `7345d62`, `ff77943`, `ab88076`)로 나뉘었다.

| 단계 | 0.42.78 commit | 수작업 내용 | 상태 |
| --- | --- | --- | --- |
| 생성 블록 `8`개 | `064f6f2` | `current-evidence.json` 수정 후 `Update-PcvCurrentEvidenceDocs.ps1` 실행 | 기존 자동화 |
| descriptor chain | `ff77943` | `MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md` key `45`개를 `previous_<tag>_*`로 강등(+89/−44) | Task 0 완료 |
| ledger key·status table | `ff77943` | `CURRENT_EVIDENCE_LEDGER.md` head key `4`개 강등, status table 행 `8`종 강등·교체 | Task 1 |
| index 승격 절 | `ab88076` | `EVIDENCE_INDEX.md` 32줄, `CONTROL_PLANE_INDEX.md` 27줄 변경: 새 current 절, 직전 절 제목 강등, descriptor consume 절 | Task 2 |
| 버전별 C# verifier | `ab88076` | `Verify04277Current`를 `Verify04278Current`로 바꾸고 상수 교체, `Pcv04278PromotionEvidenceContractTests` 신규, `Verify04277PreviousCurrent` 추가 | Task 3(설계 선행) |
| spec SHA pin | `ab88076` | `config/pcv-*-contract-spec-v1.json` 3개와 verifier SHA 상수 3곳 갱신 | Task 4 |
| consume·main push evidence | `7345d62` | 호스트 실행 결과를 모아 evidence 문서 작성 | 범위 밖 |

`Verify04278Current`의 값(version, MSI SHA, provenance, descriptor id)은 `current-evidence.json`과 같은 값을 C# 상수로 다시 적은 것이다. 그래서 승격마다 같은 사실을 JSON, 생성 블록, descriptor, ledger, index, C#에 여러 번 적는다.

## Global Constraints

- 생성기는 입력 spec에 없는 줄을 byte 단위로 보존한다. 과거 dated section, `previous_*`, `historical_*` 값은 재해석하거나 삭제하지 않는다.
- 새 evidence는 새 파일로 추가하고 기존 evidence를 덮어쓰지 않는다.
- 생성기 인터페이스는 `Update-PcvManualAdminDescriptorChain.ps1`와 맞춘다. 기본 dry-run, `-Apply`, `-Check`를 두고, 결과는 JSON 한 줄(`schema_version`, `ok`, `mode`), 오류는 `PCV_*_INVALID|<field>|<detail>` 형식이다.
- spec JSON은 `System.Text.Json.JsonDocument`로 읽는다. `ConvertFrom-Json`은 `updated_at` 같은 ISO 시각을 `DateTime`으로 바꾼다(`ac7945b`에서 replay로 확인).
- 각 task의 완료 기준:
  - 0.42.78 replay가 해당 commit 결과와 byte 단위로 일치한다.
  - 새 Pester/xUnit 테스트가 통과한다.
  - `git diff --check`에 문제가 없다.
- host mutation, 설치본 변경, public trusted signing, external stable publication은 범위 밖이다.
- task 하나가 checkpoint 하나다(30분, tool batch 18회, 리뷰 1회 + 제한 재검토 2회). 범위 밖 발견은 `report-only`로 처리한다. commit은 campaign `promotion-automation-20260929`의 `commit_policy`(`local-commit-per-task`)를 따르고, push/PR은 별도 승인을 받는다.
- P1-8 브랜치(`lane2/p1-guest-file-20260928`, PR #16)와 섞지 않는다. 이 계획은 `origin/main` 기반 `tooling/descriptor-chain-20260929` 브랜치에서 진행한다.

## Task 0: descriptor chain rotation (완료)

**생성:** `packaging/windows-desktop-node/tools/Update-PcvManualAdminDescriptorChain.ps1`, `packaging/windows-desktop-node/manual-admin-tests/PcvManualAdminDescriptorChain.Tests.ps1` (`ac7945b`)

- [x] rotation 규칙을 정했다.
  - 이전 값은 key 바로 아래 `previous_<tag>_<key>` 줄로 옮긴다.
  - `next_*` key에는 새 버전 tag를, 나머지 key에는 직전 버전 tag를 붙인다.
  - 같은 key가 여러 번 있으면 첫 번째 line-anchored occurrence만 바꾼다.
  - 같은 rotation을 두 번 적용하면 거부한다.
- [x] 0.42.78 replay 결과: key `45`개(값 변경 `44`, 값 유지 `1`)를 처리했고 `ff77943` 결과와 byte 단위로 일치했다. `-Check`는 `current`를 냈다.
- [x] Pester `15`개가 통과했다(새 suite `9`개, `PcvManualAdminDescriptorCurrency` `6`개).

## Task 1: ledger key block과 status table 행 rotation

**수정 후보:** `packaging/windows-desktop-node/tools/Update-PcvManualAdminDescriptorChain.ps1`
**생성 후보:** `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceLedgerRows.ps1`, `packaging/windows-desktop-node/manual-admin-tests/PcvCurrentEvidenceLedgerRows.Tests.ps1`

- [x] head key `4`개(`current_full_admin_host_mutation`, `current_manual_admin_package_pair`, `current_descriptor_batch_id`, `current_manual_admin_evidence`)가 descriptor와 같은 규칙으로 강등되는지 확인한다. 같으면 기존 도구를 `-DescriptorPath docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`로 재사용한다. 그 전에 key block 탐지 범위(첫 `key: ` 줄부터 다음 `## `까지)가 생성 블록이나 표와 겹치지 않는지 먼저 본다.
- [x] `ff77943` ledger diff 전체 줄을 읽고, status table 행 `8`종의 처리 방식을 표로 확정한다.
  - 강등 후 삽입: 기존 행의 근거 칸을 `predecessor after <ver> promotion`으로 바꾸고, 바로 아래 새 current 행을 넣는다. 대상은 `manual-admin-package-pair-current`, `package-build-current`, `installed-operator-surface-smoke-latest`, `manual-admin-package-pair-latest-candidate`다.
  - 교체: 기존 행을 새 행으로 바꾼다. 대상은 `latest-product-payload-smoke`, `functional-correctness-actual-host-latest`, `manual-admin-package-pair-next`다.
  - `full-admin-host-mutation-current`는 기존 행의 artifact 칸도 바뀌었다. 그래서 별도 규칙이 필요한지 확인한다.
- [x] 완료 기준: `ff77943^` ledger에 0.42.78 spec을 replay한 결과가 `ff77943` ledger와 byte 단위로 일치한다.

실행 기록(2026-09-29): 실측이 계획과 두 곳에서 달랐다.
- key block(`23`~`854`행)은 생성 블록(`3`~`19`행)과 `## 현재 Anchor` 표(`866`행~)와 겹치지 않는다. 그래서 head key는 기존 도구를 고치지 않고 `-DescriptorPath`로 재사용한다. tag 규칙도 같다(`current_*`는 직전 버전 tag).
- `full-admin-host-mutation-current`도 강등 후 삽입이다. 강등 행 5종은 모두 같은 규칙을 따른다.
  - 근거 칸: 첫 evidence 문서를 남기고, 둘째 항목이 `artifacts/batch-runs/<leaf>`이면 `<leaf>`(batch id)를 남긴 뒤 `predecessor after <tag> promotion`을 붙인다.
  - 상태 칸: 바꾸지 않는다.
  - 운영 규칙 칸: 템플릿(`<a>→<b> pair PASS는 predecessor다.`, `<tag> fullgate|package|current-card PASS는 predecessor다.`)으로 만든다.
  - 0.42.78에서 템플릿과 다른 문장 `2`개(`installed-operator-surface-smoke-latest`, `manual-admin-package-pair-latest-candidate`)는 spec의 `predecessor_rule`로 받는다.
- 같은 id가 앞쪽 과거 표에도 `4`행 있다. 그래서 도구는 `## 현재 Anchor` 표 header로 범위를 잡는다.

새 도구 `Update-PcvCurrentEvidenceLedgerRows.ps1`(contract `pcv-current-evidence-ledger-rows-rotation-v1`)를 만들었다. `supersede`/`replace` 목록을 받는다. 0.42.78 replay는 head key `4`개(기존 도구) 뒤 행 `8`종(강등 후 삽입 `5`, 교체 `3`)을 적용했고, 결과가 `ff77943` ledger와 byte 단위로 일치했다. 두 도구의 `-Check`는 `current`를 냈다. 적용 전 ledger의 `-Check`는 `8`종 모두 stale이었고, 같은 rotation을 두 번 적용하면 `already-rotated`로 거부된다. Pester는 새 suite `9`개를 포함해 `24`개가 통과했다(`PcvManualAdminDescriptorChain` `9`개, `PcvManualAdminDescriptorCurrency` `6`개).

## Task 2: index 승격 절 생성

**생성:** `packaging/windows-desktop-node/tools/New-PcvPromotionIndexSections.ps1`, `packaging/windows-desktop-node/manual-admin-tests/PcvPromotionIndexSections.Tests.ps1`

- [x] `EVIDENCE_INDEX.md`를 갱신한다.
  - 기존 current 절 위에 새 `## <date> \`<ver>\` current promotion` 절을 넣는다. 내용은 package, fullgate, current-card, functional, pair, claims의 `6`개 bullet이다.
  - 기존 절 제목은 `predecessor promotion`으로 바꾼다.
  - 도입 문단에 `<date> Lane 3가 \`<ver>\`로 다시 승격했다.` 줄을 추가한다.
- [x] `CONTROL_PLANE_INDEX.md`에 `operational current promotion` 절과 `<prev> -> <ver>` descriptor consume 절을 만든다.
- [x] 문장 템플릿과 입력 값을 정한다.
  - 문장 템플릿은 0.42.78 절을 그대로 옮긴다.
  - `promotion_eligible`과 blocker 문장은 `current-evidence.json`의 `feature_qualification`에서 만든다.
  - carry-forward 설명처럼 사람이 판단해야 하는 문장은 spec의 `notes` 배열로 받는다.
- [x] 완료 기준:
  - `ab88076^`에 0.42.78 spec을 replay한 결과가 두 index 파일 모두 `ab88076`과 byte 단위로 일치한다.
  - `D2EvidenceContractVerifier`의 `RequireMatches` 패턴(`canonical current는 ...다`, `operational current는 ...다`)이 생성 결과에 그대로 맞는다.

실행 기록(2026-09-29): `ab88076` diff 실측은 위 항목과 두 파일의 역할이 달랐다.
- `EVIDENCE_INDEX.md`: 제목 바로 아래에 `operational current promotion` 절과 `<prev> -> <ver>` descriptor consume 절을 넣기만 한다. 기존 절 제목과 도입 문단은 바꾸지 않는다.
- `CONTROL_PLANE_INDEX.md`: closure 문단 끝에 `<date> Lane 3가 <ver>로 다시 승격했다.` 줄을 붙이고, 새 `current promotion` 절을 넣은 뒤, 직전 절 제목을 `predecessor promotion`으로 바꾼다. consume 절은 없다.

새 도구 `New-PcvPromotionIndexSections.ps1`(contract `pcv-promotion-index-sections-v1`)를 만들었다.
- 값은 `current-evidence.json`에서 읽는다. version, 문서 경로, SHA, provenance, pair(baseline, target, descriptor)가 여기에 든다.
- spec은 사람이 정하는 값만 받는다. `date`, `previous_version`, `installed_version`, `p0_feature_ledger_version`, `pair_evidence`, `functional_note`, 선택 항목 `main_push_evidence`다.
- 다음 경우는 거부한다.
  - `promotion_eligible=false` 또는 blocker가 있음
  - claim이 `true`
  - pair target이 current가 아님
  - 직전 current 절 제목의 version이 `previous_version`과 다름
  - closure 줄이 없음
  - 같은 절을 두 번 적용함

0.42.78 replay 결과는 `EVIDENCE_INDEX.md` `+32`행, `CONTROL_PLANE_INDEX.md` `+25`행이고, 두 파일 모두 `ab88076`과 byte 단위로 일치했다. 결과가 같으므로 verifier 패턴도 그대로 맞는다. `-Check`는 `current`, 적용 전 파일은 두 target 모두 stale이었다. Pester 새 suite `7`개가 통과했다.

## Task 3: 버전별 C# verifier를 데이터 기반으로 전환 (설계 선행)

**대상:** `src/DesktopNode.Delivery.Tests/Delivery/Evidence/D2EvidenceContractVerifier.cs`(1,311줄), `Pcv04273PromotionEvidenceContractTests.cs`, `Pcv04277PromotionEvidenceContractTests.cs`, `Pcv04278PromotionEvidenceContractTests.cs`

- [x] 구현 전에 설계 문서를 쓴다: `docs/superpowers/specs/<date>-purecvisor-desktop-node-data-driven-promotion-verifier-design.md`. 결정할 사항은 다음과 같다.
  1. 버전 고정 상수가 지키던 불변식을 무엇으로 대체할지 정한다. 이 상수는 `current-evidence.json`이 실수로 바뀌는 것을 막는다. 대안은 `current-evidence.json`이 바뀌면 descriptor, ledger, index의 `-Check` 통과를 필수로 요구하는 방식이다.
  2. previous-current 검증을 descriptor의 `previous_<tag>_*` chain에서 읽을지 정한다.
  3. 기존 `Pcv04273`/`Pcv04277`/`Pcv04278` 테스트를 evidence anchor로 보존할지, 은퇴시킬지 정한다.
- [ ] 이 변경이 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`의 검증 정책을 바꾸면 ADR로 처리한다. 구현은 사용자 승인 후에 한다.
실행 기록(2026-09-29): 설계 문서 `docs/superpowers/specs/2026-09-29-purecvisor-desktop-node-data-driven-promotion-verifier-design.md`를 썼다.- 권장 결정: I1(`current-evidence.json` 고정)은 spec pin으로 대신한다. 직전 current는 ledger head chain에서 읽는다.- `Pcv04277`/`Pcv04278` 테스트는 generic `PcvCurrentPromotionEvidenceContractTests`로 바꾼다. manifest 등록 legacy contract(`Pcv04273`, `Pcv04274`)는 유지한다.- 검증 정책 문서와 ADR은 버전별 verifier를 요구하지 않으므로 ADR은 필요 없다.- 구현은 설계 승인 뒤에 한다.

## Task 4: spec SHA pin 갱신 도구

**생성:** `packaging/windows-desktop-node/tools/Update-PcvContractSpecPins.ps1`, `packaging/windows-desktop-node/manual-admin-tests/PcvContractSpecPins.Tests.ps1`

- [x] 착수 전에 pin 구조를 확인한다. 예상 구조는 두 단계다.
  - source file SHA를 spec JSON에 적는다.
  - spec JSON의 SHA를 verifier 상수에 적는다.
- [x] 대상은 `config/pcv-*-contract-spec-v1.json` `9`개와 verifier 상수다. verifier 상수는 `ExpectedSpecSha256` `6`곳과, `ab88076`에서 함께 바뀐 `InstalledContractVerifier`/`ManualAdminContractVerifier`의 SHA 상수다.
- [x] `-Check`는 SHA 불일치를 보고하고, `-Apply`는 두 단계를 순서대로 갱신한다.
- [x] 완료 기준: `ab88076^`에서 replay한 결과가 `ab88076`의 spec `3`개와 verifier 상수 `3`곳과 일치한다.

실행 기록(2026-09-29): 실측으로 확인한 구조는 다음과 같다.
- pin은 spec JSON의 `source_files[]`와 `legacy_files[]` 안 `{ "path", "sha256" }` 항목이다. raw text에서 `"path"` 바로 뒤에 `"sha256"`이 온다.
- spec을 pin하는 verifier는 `SpecPath` 상수로 찾는다(`9`개 spec에 하나씩). SHA 상수는 두 형식이다.
  - `ExpectedSpecSha256 =` 다음 줄
  - `LegacyBatchContractVerifier`의 `new(SpecPath, "<sha>", ...)` 인자
- hash는 verifier와 같이 계산한다. BOM을 뺀 UTF-8 text이고, 줄바꿈은 정규화하지 않는다(`.gitattributes`가 eol을 고정한다).
- verifier가 `StructuredTransitionSources`에 둔 경로는 source SHA 검사에서 빠진다. 도구도 이 경로는 갱신하지 않고 `exempt_stale`로만 보고한다. 현재 development policy `4`개, orchestration `1`개다.
- spec이 verifier나 다른 spec을 pin하는 순환은 없다.

새 도구 `Update-PcvContractSpecPins.ps1`은 입력 spec 없이 저장소를 읽는다. dry-run, `-Apply`, `-Check` 세 모드를 가진다.

replay 방법: `ab88076` 트리에서 pin 파일 `6`개(spec `3`, verifier `3`)만 `ab88076^`로 되돌린 뒤 `-Apply`했다.
- 결과: spec `3`개의 pin `4`개(`AGENTS.md`, packaging `README.md`, `current-evidence.json`, descriptor 문서)와 verifier 상수 `3`곳이 갱신됐다.
- spec `9`개와 verifier `9`개가 모두 `ab88076`과 byte 단위로 일치했다.
- 현재 저장소의 `-Check`는 `current`다. Pester 새 suite `4`개가 통과했다.

C# verifier가 spec 없이 직접 pin하는 상수(`ExpectedLegacySha256`, `ExpectedModuleSha256`, `ProductInvokeContractVerifier`의 `4`개)는 이 도구 범위 밖이다(`report-only`).

## Task 5: 승격 문서 orchestration

**생성:** `packaging/windows-desktop-node/tools/Invoke-PcvLane3PromotionDocs.ps1`

- [x] 승격 spec 하나로 아래 순서를 실행하고, 결과를 JSON 하나로 요약한다.
  1. `Update-PcvCurrentEvidenceDocs.ps1`
  2. Task 0~2 생성기
  3. Task 4 pin 도구
- [x] 자동화된 단계를 승격 절차 문서에 반영한다. 그 문서가 spec pin 대상이면 Task 4 도구로 pin을 함께 갱신한다.

실행 기록(2026-09-29): `Invoke-PcvLane3PromotionDocs.ps1`(contract `pcv-lane3-promotion-docs-v1`)를 만들었다.
- 입력: spec 하나에 `descriptor_chain`, `ledger_head`, `ledger_rows`, `index_sections` 네 절이 든다. 각 절은 원문 그대로(`GetRawText`) 해당 도구에 넘긴다.
- 실행 순서: 생성 블록 → descriptor chain → ledger head key → ledger 행 → index 절 → spec pin.
- `-Apply`: spec을 받는 네 단계를 먼저 dry-run으로 검증하고, 하나라도 `planned`가 아니면 아무것도 쓰지 않는다. 적용 중에는 첫 실패에서 멈춘다.
- dry-run: `Update-PcvCurrentEvidenceDocs.ps1`에는 dry-run이 없어서 `-Check`로 대신한다.

0.42.78 replay 방법: `ab88076` 트리에서 도구가 다루는 파일 `15`개(생성 target `8`, descriptor, spec `3`, verifier `3`)를 승격 직전(`064f6f2^`) 상태로 되돌렸다. 그 뒤 spec 하나로 dry-run, `-Apply`, `-Check`를 차례로 실행했다.
- dry-run: 생성 블록 `stale`, 나머지 다섯 단계 `planned`
- `-Apply`: 여섯 단계 모두 적용됐다.
- 결과: `15`개 파일이 모두 `ab88076`과 byte 단위로 일치했다. 0.42.78 승격 commit `4`개 가운데 이 파일들에 대한 수기 편집을 spec 하나가 대신한다.
- `-Check`: 여섯 단계 모두 `current`다.

그 spec은 견본으로 `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04278.json`에 두었다. `docs/DEVELOPMENT_PROCEDURE.md` §6에 문서 반영 순서를 적었다. 이 문서는 contract spec pin 대상이 아니다. pin 도구 `-Check`는 `current`였고, 절차 문서 계약 테스트 `3`개가 통과했다. Pester 새 suite `4`개가 통과했다.

남은 수기 작업:
- evidence 문서
- `current-evidence.json`
- 버전별 C# verifier(Task 3, 설계 승인 필요)
- `DOCUMENTATION_INDEX.md`와 `FEATURE_IMPLEMENTATION_LEDGER.md` 정렬(`ff77943`에 있었다)

검증 보정(2026-09-29): Task 0~5는 새 Pester 파일 `5`개를 `packaging/windows-desktop-node/tests`에 두었다.
- 그 폴더는 Pester-free 이관 뒤 고정된 inventory다. 파일 `55`개가 `LegacyPesterContractParserTests`와 `MigrationManifestV2`에 고정돼 있다. 그래서 솔루션 테스트에서 Delivery `3`건이 실패했다.
- P1-8 선례(`PcvServicePlanP1GuestFileActualVmSmokeContractTests`)를 따라 두 가지를 바꿨다.
  - 다섯 suite를 inventory 밖 `packaging/windows-desktop-node/manual-admin-tests`로 옮겼다.
  - Delivery.Tests에 C# 정적 계약 `PcvLane3PromotionDocsToolsContractTests`(`11`개)를 더했다. 다섯 도구의 mode, contract 이름, 오류 코드, 단계 순서를 고정하고, host mutation 호출이 없는지 확인한다.
- 같이 실패한 `Verification.Tests`의 `policy-boundaries`는 원인이 달랐다. cutover git boundary가 dirty worktree를 거부했고(`cutover-worktree=dirty`), 당시 Task 5가 미커밋이었다.
- task별 focused 검증에 `dotnet test src/DesktopNode.Delivery.Tests`가 빠져 있었다.

## 순서와 측정

- 진행 순서는 Task 1 → 2 → 4 → 5이며, 각 task는 독립 checkpoint다. Task 3은 설계 문서와 사용자 승인 뒤에 진행한다.
- 효과는 다음 승격에서 측정한다. 0.42.78의 commit 네 개, 수기 편집 줄 수와 비교해 기록한다.

## 범위 밖

- private 저장소(`0.42.74`) 반영
- evidence 문서 본문 생성: 호스트 실행 결과이므로 사람이나 runner가 만든다.
- consume·main push evidence 작성
- host mutation
