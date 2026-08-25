# PureCVisor Desktop Node Task 4 Canonical Feature Qualification Runtime 설계

상태: 사용자 설계 승인 완료 / 구현 계획 작성 전
일자: 2026-08-23
범위: AR-001 Task 4 확대 경계

## 1. 목적

설치·rollback 기준선인 `operational_current`와 기능별 승격 자격인
`feature_qualification`을 문서 생성기와 설치본 API가 동일한 canonical record에서 읽게 한다.
현재 `0.42.74-admin-smoke`는 operational current로 유지하되 Saved lifecycle의
`actual_vm_tested=fail`을 숨기지 않는다. 이후 다른 candidate version은 필수 기능 blocker가
남아 있으면 current로 승격할 수 없다.

이 설계는 PowerShell generator, publish asset, C# provider와 Ops Summary projection을 하나의
read-only data flow로 연결한다. C#에 0.42.74 blocker를 하드코딩하거나 batch-evidence의
`available/ok` 상태를 기능 자격으로 재해석하지 않는다.

## 2. 배경과 문제

ADR-0015와 `config/desktop-node-feature-evidence-ledger.json`은 operational current와 기능별
qualification을 분리했다. `PcvFeatureEvidencePromotion.psm1`은 ledger와 stage observation에서
결정론적인 `pcv-feature-promotion-decision-v1` 결과를 만든다.

기존 Task 4 계획에는 다음 연결이 빠져 있었다.

- `docs/ga-ready/current-evidence.json`에는 `feature_qualification`이 없다.
- `Update-PcvCurrentEvidenceDocs.ps1`에는 evaluator result를 읽거나 candidate를 차단하는 경계가 없다.
- `DesktopNodeApiOpsSummaryBuilder`는 batch evidence만 받고 canonical current JSON을 읽지 않는다.
- Batch evidence reader는 feature qualification을 제공하지 않는다.
- 설치본 API가 canonical JSON을 받을 publish asset 계약이 없다.

이 상태에서 C# builder에 현재 blocker를 상수로 넣으면 문서와 API가 서로 다른 진실 원천을 갖는다.

## 3. 결정

### 3.1 단일 진실

`docs/ga-ready/current-evidence.json`이 operational current와 current feature qualification의
단일 진실이다. `feature_qualification`은 evaluator 결과와 같은 shape를 저장한다.

```json
{
  "feature_qualification": {
    "schema_version": 1,
    "contract": "pcv-feature-promotion-decision-v1",
    "promotion_eligible": false,
    "blockers": [
      {
        "feature_id": "pcv.vm.saved-lifecycle",
        "stage": "actual_vm_tested",
        "verdict": "fail"
      }
    ]
  }
}
```

Canonical record에는 파생값인 `status`를 저장하지 않는다. API provider가 다음과 같이 계산한다.

- `promotion_eligible=true`, blocker 0개: `eligible`
- `promotion_eligible=false`, blocker 1개 이상: `blocked`
- publish asset 누락 또는 손상: `unavailable`

### 3.2 publish asset

`src/DesktopNode.Api/DesktopNode.Api.csproj`는 repository canonical file을 다음 경로로 연결한다.

```text
repository: docs/ga-ready/current-evidence.json
build/publish: evidence/current-evidence.json
runtime: <AppContext.BaseDirectory>/evidence/current-evidence.json
```

MSBuild metadata는 `Link`, `CopyToOutputDirectory=PreserveNewest`,
`CopyToPublishDirectory=PreserveNewest`를 사용한다. 별도 runtime decision JSON을 만들지 않는다.

### 3.3 immutable load

API request processor를 구성할 때 provider가 asset을 한 번만 읽고 immutable snapshot을 만든다.
`ops.summary` 요청마다 파일을 다시 읽지 않으며 watcher와 mtime cache를 추가하지 않는다.
설치/update로 asset이 바뀌면 서비스가 다시 시작될 때 새 snapshot을 읽는다.

`DesktopNodeApiRequestProcessor.CreateDefault`의 마지막 optional parameter로
`currentEvidencePath`를 추가한다. 값이 없으면 publish 기본 경로를 사용하고, 테스트는 임시 파일을
주입한다. 기존 호출자는 변경하지 않는다.

## 4. 데이터 흐름

```text
Task 3 evaluator result
        |
        v
docs/ga-ready/current-evidence.json
        |-- JSON schema + generator validation
        |-- generated Markdown current blocks (8 targets)
        `-- dotnet build/publish Content link
                    |
                    v
      evidence/current-evidence.json
                    |
        API construction-time provider load
                    |
                    v
 immutable feature qualification snapshot
        |                              |
        v                              v
current_evidence.feature_qualification  signals[feature-promotion]
```

Batch evidence는 package/fullgate/manual-admin 상태를 계속 소유한다. Feature qualification은 batch
status와 독립적이며 `available`, `ok`, `degraded`로 변환하지 않는다.

## 5. Canonical schema 계약

`docs/ga-ready/current-evidence.schema.json`은 `feature_qualification`을 required property로 추가하고
다음을 검증한다.

- `schema_version=1`
- `contract=pcv-feature-promotion-decision-v1`
- `promotion_eligible`은 boolean
- blocker `feature_id`는 `^pcv\.[a-z0-9._-]+$`
- stage는 `code_tested`, `packaged`, `installed_tested`, `actual_vm_tested`,
  `manual_admin_tested` 중 하나
- verdict는 `fail`, `blocked`, `missing` 중 하나
- eligible이면 blocker는 정확히 0개
- not eligible이면 blocker는 최소 1개
- 알 수 없는 property는 거절

현재 canonical record는 `0.42.74-admin-smoke`와 Saved lifecycle actual-VM fail blocker 한 개를
저장한다.

## 6. Generator와 promotion guard

`Update-PcvCurrentEvidenceDocs.ps1`은 canonical qualification shape를 자체 validation에서도 검사한다.
Markdown current block에는 다음 의미를 한 줄로 투영한다.

```text
Feature qualification: contract=<contract>; promotion_eligible=<bool>;
blocker_count=<count>; blockers=<feature_id>/<stage>/<verdict>.
```

Generator는 입력 record와 repository canonical baseline을 구분한다.

1. 기본 `EvidencePath`이면 current record를 검증하고 현재 blocker를 렌더링한다.
2. 별도 `EvidencePath`이면 repository canonical version과 proposed `current.version`을 비교한다.
3. version이 달라 candidate promotion을 시도하면서 `promotion_eligible=false`이면
   `PCV_FEATURE_PROMOTION_BLOCKED`를 반환한다.
4. 같은 0.42.74 record의 false는 소급 삭제하지 않고 허용한다.
5. qualification shape가 잘못되면 `PCV_CURRENT_EVIDENCE_INVALID`를 반환한다.

`-Check`는 expected Markdown과 현재 target만 비교하며 쓰기를 수행하지 않는다. Candidate rejection도
어떤 파일도 변경하기 전에 발생한다. 테스트는 대상과 source의 SHA-256 전후 값을 비교한다.

## 7. API 구성 요소

### 7.1 Provider

신규 `DesktopNodeCurrentEvidenceProvider.cs`는 다음 책임만 가진다.

- publish asset을 한 번 읽는다.
- current record와 qualification 최소 shape를 검증한다.
- blocker 순서를 보존한 immutable snapshot을 반환한다.
- missing, invalid JSON, invalid contract를 unavailable snapshot으로 변환한다.
- 원본 path, parser exception, 파일 내용을 API payload에 넣지 않는다.

Snapshot은 qualification schema version, contract, status, promotion eligibility, blockers와 선택적
error code를 가진다. 정상 snapshot의 error code는 null이다.

### 7.2 Query와 builder

Request processor는 provider snapshot을 `DesktopNodeApiOpsSummaryQuery`에 전달한다. Query가 만드는
`DesktopNodeApiOpsSummarySnapshot`은 같은 snapshot reference를 포함하며, builder가 다음 두 곳에
투영한다.

```text
data.current_evidence.feature_qualification
data.signals[key=feature-promotion]
```

정상 blocked payload는 canonical contract, status, false, blocker 배열을 그대로 유지한다. Signal은
`tone=error`, `value=<blocker count>`다. Eligible이면 `tone=ok`, `value=0`이다.

### 7.3 Unavailable 동작

Asset 누락 또는 손상은 VM/host API 전체를 중단시키지 않는다.

```text
status=unavailable
promotion_eligible=false
error_code=PCV_CURRENT_EVIDENCE_UNAVAILABLE
blockers=[]
signal tone=error
signal value=unavailable
```

Unavailable은 정상 decision으로 위장하지 않으며, synthetic feature blocker도 만들지 않는다.

## 8. 오류와 보안 경계

- 외부 요청은 evidence path를 지정할 수 없다.
- 기본 path는 `AppContext.BaseDirectory` 아래 publish asset으로 고정한다.
- 테스트 seam의 path는 processor 구성 시에만 주입한다.
- API 응답은 절대 path, exception message, JSON 원문을 노출하지 않는다.
- Provider는 file write, watcher, host mutation을 수행하지 않는다.
- Generator는 `-Check`에서 source와 target을 쓰지 않는다.
- Qualification 실패는 batch evidence의 `available/ok`로 덮지 않는다.
- Public trusted signing과 external stable publication claim은 계속 false다.

## 9. 파일 경계

### 새 파일

- `src/DesktopNode.Api/DesktopNodeCurrentEvidenceProvider.cs`
- `src/DesktopNode.Api.Tests/DesktopNodeCurrentEvidenceProviderTests.cs`

### 수정 파일

- `docs/ga-ready/current-evidence.json`
- `docs/ga-ready/current-evidence.schema.json`
- `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1`
- `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`
- `src/DesktopNode.Api/DesktopNode.Api.csproj`
- `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- `src/DesktopNode.Api/DesktopNodeApiOpsSummaryHandler.cs`
- `src/DesktopNode.Api/DesktopNodeApiOpsSummaryBuilder.cs`
- `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

Generator projection이 바뀌므로 다음 8개 owned document의 generated block도 갱신한다.

- `README.md`
- `AGENTS.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/ga-ready/EVIDENCE_INDEX.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `packaging/windows-desktop-node/README.md`

Batch evidence reader, Hyper-V provider, MSI, service control과 Web source는 변경하지 않는다.

## 10. TDD와 검증

### PowerShell RED/GREEN

- canonical record가 qualification을 요구한다.
- eligible/blocker conditional schema가 동작한다.
- current 0.42.74 blocker가 Markdown에 투영된다.
- blocked candidate 0.42.75가 `PCV_FEATURE_PROMOTION_BLOCKED`로 실패한다.
- `-Check`와 candidate rejection의 source/target hash가 변하지 않는다.
- csproj publish metadata가 canonical asset을 가리킨다.

### C# RED/GREEN

- provider가 blocked canonical asset을 읽고 blocker 순서를 보존한다.
- missing/malformed asset은 unavailable snapshot을 만든다.
- unavailable 응답에 path와 parser detail이 없다.
- processor end-to-end 응답이 false를 유지한다.
- feature-promotion signal은 batch signal과 독립적이다.

### 최종 non-mutating 검증

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~OpsSummary|FullyQualifiedName~CurrentEvidenceProvider"
dotnet test src/DesktopNode.sln -c Release --no-restore
Invoke-Pester -Path packaging/windows-desktop-node/tests
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check
git diff --check
```

모든 검증은 host, VM, service, MSI와 firewall을 변경하지 않는다.

## 11. 완료 조건

- 문서 generator와 설치본 API가 같은 canonical file을 읽는다.
- C# blocker 상수 또는 별도 runtime decision JSON이 없다.
- current 0.42.74 Saved failure가 문서와 API에서 동일하다.
- blocked 0.42.75 candidate는 current promotion 전에 거절된다.
- batch evidence가 available이어도 feature signal은 error를 유지한다.
- asset 누락/손상 시 API는 계속 응답하되 promotion을 fail-closed한다.
- publish output에 `evidence/current-evidence.json`이 존재한다.
- focused와 전체 non-mutating test가 모두 통과한다.
- host/VM/service/package mutation count는 0이다.

## 12. 비목표

- 0.42.75 campaign 실행 또는 current 승격
- 실제 VM SavedOnly/Full P0 재실행
- MSI 설치, update, rollback 또는 service restart
- batch evidence schema 변경
- file watcher 또는 runtime hot reload
- Web Console UI 변경
- public trusted signing 또는 external stable publication 주장

## 13. 롤백

API provider와 projection을 제거하고 csproj Content link를 삭제하면 직전 API shape로 돌아간다.
Generator guard가 이미 candidate promotion workflow에 사용된 뒤에는 canonical current version을 먼저
바꾸지 않는다. Provider rollback과 generator rollback을 같은 commit에서 수행하며, current
qualification evidence를 pass로 조작하는 방식으로 롤백하지 않는다.
