# 데이터 기반 승격 verifier 설계 (2026-09-29)

상태: 설계 제안. 구현은 사용자 승인 뒤에 한다(`docs/superpowers/plans/2026-09-29-purecvisor-desktop-node-promotion-automation.md` Task 3).

## 1. 문제

승격할 때마다 `src/DesktopNode.Delivery.Tests/Delivery/Evidence/D2EvidenceContractVerifier.cs`를 손으로 고친다. 0.42.78(`ab88076`)에서 한 일은 다음과 같다.

- `Verify04277Current`를 `Verify04278Current`로 바꾸고 상수 `11`개를 교체했다.
- `Verify04277PreviousCurrent`를 새로 쓰고, `Pcv04278PromotionEvidenceContractTests`를 추가했다.

이 상수들은 `current-evidence.json`에 이미 있는 값(version, MSI SHA, provenance, descriptor id)을 C#에 다시 적은 것이다. `Invoke-PcvLane3PromotionDocs.ps1`가 문서 `15`개를 자동으로 쓰게 된 뒤에는 이 C# 편집이 승격의 유일한 수기 코드 변경이다.

`Verify04277PreviousCurrent`는 `record.ManualAdmin.LatestClosedBaseline == "0.42.77-admin-smoke"`를 요구한다. 그래서 다음 승격에서는 반드시 깨진다. "과거 사실"을 검사하는 것 같지만 실제로는 현재 상태에 묶여 있다.

## 2. 지금 verifier가 지키는 불변식

| # | 불변식 | 현재 구현 | 대체 |
| --- | --- | --- | --- |
| I1 | `current-evidence.json`이 실수로 바뀌지 않는다 | version, SHA, provenance 상수와 `Assert.Equal` | spec pin. `config/pcv-manual-admin-readiness-contract-spec-v1.json`이 `current-evidence.json`의 SHA를 pin하고 `ManualAdminContractVerifier`가 검사한다. 파일이 바뀌면 `Update-PcvContractSpecPins.ps1 -Apply`를 거쳐야 green이 된다. |
| I2 | record 형식과 eligibility | `D2CurrentEvidenceVerifier.Validate`, `PromotionEligible`, `Blockers` | 그대로 둔다(버전 무관) |
| I3 | descriptor current key가 record와 같다 | `RequireMetadata` 상수 `5`개 | record에서 기대값을 계산한다(아래 §3) |
| I4 | index 문장이 current version을 말한다 | `canonical current는 ...다`, `operational current는 ...다` 상수 regex | `record.Current.Version`으로 regex를 만든다 |
| I5 | ledger current 행이 current version이다 | `full-admin-host-mutation-current`, `package-build-current` 상수 regex | version으로 regex를 만든다 |
| I6 | 직전 current가 predecessor로 남는다 | `previous_04277_*` 상수 `5`개, ledger `predecessor after 04278 promotion` | ledger head key chain에서 직전 version을 읽는다(아래 §4) |

## 3. current 일관성 (I2~I5)

`VerifyCurrentPromotion()` 하나가 record를 읽고 기대값을 계산한다.

- descriptor(`MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`)의 기대값:
  - `current_manual_admin_package_pair` = `<LatestClosedBaseline> -> <LatestClosedTarget>`
  - `current_manual_admin_descriptor_batch_manifest` = `LatestClosedDescriptor`
  - `current_manual_admin_target_msi_sha256` = `Current.CleanMsiSha256`
  - `current_full_admin_host_mutation_batch` = `Current.FullgateBatch`
  - `current_full_admin_host_mutation_provenance_commit` = `Current.ProvenanceCommit`
- `LatestClosedTarget == Current.Version`을 요구한다. `New-PcvPromotionIndexSections.ps1`와 같은 규칙이다.
- index와 ledger regex는 `Regex.Escape(version)`으로 만든다.

## 4. 직전 current (I6)

직전 version은 ledger head key chain에서 읽는다. `current_full_admin_host_mutation` 바로 아래 줄이 `previous_<tag>_current_full_admin_host_mutation: <prev>`다. 이 규칙은 `Update-PcvManualAdminDescriptorChain.ps1`가 보장한다.

`<tag>`와 `<prev>`를 얻은 뒤 다음을 검사한다.

- descriptor에 `previous_<tag>_current_manual_admin_package_pair`, `previous_<tag>_current_full_admin_host_mutation_batch`, `previous_<tag>_current_full_admin_host_mutation_provenance_commit`이 있고, 값이 비어 있지 않다.
- ledger에 `full-admin-host-mutation-current`와 `package-build-current`의 `<prev>` 행이 있고, 두 행 모두 `predecessor after <current tag> promotion`을 담는다.
- `<prev>`는 current와 다르다.

값 자체는 고정하지 않는다. 직전 값은 이미 chain 줄과 evidence 문서에 있고, 그 줄은 생성기가 다시 쓰지 않는다.

## 5. 결정 (권장안)

1. **I1 대체:** 버전 상수 대신 spec pin으로 `current-evidence.json`을 고정한다. 새 도구를 만들 필요는 없다.
2. **직전 current:** ledger head chain에서 읽는다(§4).
3. **기존 테스트:**
   - `Verify04278Current`, `Verify04277PreviousCurrent`와 `Pcv04277`/`Pcv04278PromotionEvidenceContractTests`를 지운다. 두 class는 manifest에 없는 일반 Fact이고, 새 테스트가 같은 불변식을 덮는다.
   - 대신 `PcvCurrentPromotionEvidenceContractTests`(current 일관성 `1`개, 직전 current `1`개)를 둔다.
   - `Pcv04273PromotionEvidenceContractTests`와 `Pcv04274PackageEvidenceContractTests`는 과거 evidence 문서 자체를 검사하는 manifest 등록 legacy contract(`PcvLegacyContract`)이므로 그대로 둔다.
4. **ADR:** `docs/DEVELOPMENT_VERIFICATION_POLICY.md`와 ADR은 버전별 verifier를 요구하지 않는다(검색 결과 없음). 그래서 ADR 없이 이 설계와 계획 기록으로 처리한다. `docs/DEVELOPMENT_PROCEDURE.md` §6의 "버전별 C# verifier는 아직 수기로 고친다" 줄은 지운다.

## 6. 검증

- 현재 저장소(0.42.78)에서 새 두 테스트가 통과한다.
- 순수 함수 단위 테스트:
  - 기대값 계산: record → descriptor key, regex
  - 직전 version 추출: ledger 줄 → `(tag, prev)`
  - 입력이 어긋나면 실패하는지: target ≠ current, chain 줄 없음, prev == current
- 0.42.78 replay: 승격 직전 tree(Task 5 replay 방식)에 orchestrator를 적용한 결과에 새 테스트가 C# 수정 없이 통과해야 한다. 이것은 Delivery 테스트를 replay tree에서 실행해야 하므로, 대신 두 가지로 확인한다.
  - 순수 함수에 `ab88076` 시점 파일 내용을 넣는 단위 테스트
  - 현재 저장소에서의 통과
- Delivery.Tests 전체, `git diff --check`

## 7. 범위 밖

- evidence 문서 본문 생성, `current-evidence.json` 작성
- `DOCUMENTATION_INDEX.md`/`FEATURE_IMPLEMENTATION_LEDGER.md` 정렬
- 과거 legacy contract(`Pcv04273`, `Pcv04274`)의 구조 변경
