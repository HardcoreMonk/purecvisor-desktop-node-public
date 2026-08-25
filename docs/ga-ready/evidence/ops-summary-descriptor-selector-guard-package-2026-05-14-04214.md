# Ops Summary Descriptor Selector Guard Package 2026-05-14 04214

```text
evidence_id: ops-summary-descriptor-selector-guard-package-2026-05-14-04214
scope: runtime-core-ops-summary-current-card-selector
result: PASS
package_version: 0.42.14-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

`manual-admin-campaign-descriptor-*` Batch Supervisor run은 이미 실행된
manual-admin package-pair evidence를 묶는 descriptor이며, Web Console current-card의
운영 evidence anchor가 아니다. `0.42.12 -> 0.42.13` descriptor batch 생성 뒤
canonical `artifacts` root에서 descriptor가 최신 `batch-runs` 항목이 되자 기존
selector가 이를 선택해 `batch_evidence.status=degraded`를 노출했다.

## 코드 변경

- `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`
  - `manual-admin-campaign-descriptor-*` batch 또는
    `step_id=manual-admin-campaign-descriptor` run을 latest operational evidence
    후보에서 제외한다.
  - malformed 최신 run은 기존처럼 parse failure로 보고해 숨기지 않는다.
- `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - `OpsSummarySkipsManualAdminDescriptorWhenSelectingLatestOperationalEvidence`
    회귀 테스트를 추가했다.

## 패키지

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260514-04214-selectorfix` |
| MSI | `PureCVisorDesktopNode-0.42.14-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb` |
| payload aggregate SHA-256 | `6d90b75e2e49c55dc6bc594b85d818cb7b17352e57a7c48682b53803102f1818` |
| service host SHA-256 | `506df2fa8658b8cff829fdd4e34b642125c03906240dd72c65c7cd1009003124` |
| provenance commit | `a28bb808386f206c9dbf7dcaeee232eacb648434` |
| signing mode | `AllowUnsignedDev` |

## 설치본 검증

- MSI install exit code: `0`
- Product `RepairInstalled -BatchEvidenceRoot artifacts` exit code: `0`
- Installed manifest: `0.42.14-admin-smoke`
- Service state: `Running`
- Ops summary artifact:
  `artifacts/installed-current-card-selectorfix-20260514-04214/ops-summary-after-selectorfix.json`
- Ops summary result:
  - `batch_evidence.status=available`
  - `latest.batch_id=full-admin-host-mutation-gate-20260514-140126-04212-explicit`
  - `latest.release.version=0.42.12-admin-smoke`
  - `installed_runtime.evidence_anchor=full-admin-host-mutation-gate-20260514-140126-04212-explicit`
  - `errors=[]`

## Web Console Current-card

- Artifact root:
  `artifacts/web-console-current-card-20260514-04214-selectorfix`
- Summary:
  `artifacts/web-console-current-card-20260514-04214-selectorfix/summary.json`
- Result: `pass`
- Expected batch:
  `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
- Expected version: `0.42.12-admin-smoke`
- Screenshots: `dashboard-current-card.png`, `evidence-view.png`
- Token value UI text exposure: `false`

## 04215 후속 검증

`0.42.14 -> 0.42.15` manual-admin descriptor batch와 `0.42.15-admin-smoke`
full admin host mutation gate 이후에도 같은 guard가 유지된다.

- Descriptor batch:
  `manual-admin-campaign-descriptor-20260514-04214-04215`
- Full gate batch:
  `full-admin-host-mutation-gate-20260514-234158-04215`
- Installed current-card artifact:
  `artifacts/installed-current-card-20260514-04215-fullgate`
- Result: `pass`
- `batch_evidence.status=available`
- `latest.batch_id=full-admin-host-mutation-gate-20260514-234158-04215`
- `latest.release.version=0.42.15-admin-smoke`
- `descriptor_excluded_from_operational_latest=true`

## 판정

Descriptor batch는 package-pair contract evidence로 보존하고, Web Console
current-card는 최신 full-admin operational evidence를 계속 표시한다. 이 evidence는
internal/admin-smoke selector fix와 설치본 current-card 검증이며 public trusted
signing, external stable publication, public release claim은 추가하지 않는다.
