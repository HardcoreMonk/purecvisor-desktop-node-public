# MANUAL-ADMIN 캠페인 2026-05-16 04222->04223

```text
evidence_id: manual-admin-campaign-2026-05-16-04222-04223
result: PASS
package_pair: 0.42.22-admin-smoke -> 0.42.23-admin-smoke
baseline_version: 0.42.22-admin-smoke
target_version: 0.42.23-admin-smoke
host_mutation_performed: true
manual_admin_descriptor_generation_contract: manual-admin-descriptor-generation-contract-v2
descriptor_batch_id: manual-admin-campaign-descriptor-20260516-04222-04223-closed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.22-admin-smoke -> 0.42.23-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign을 닫는다. `0.42.21 -> 0.42.22` Burn blocker였던
Credential Manager transition 재실행 문제를 `0.42.23-admin-smoke`에서 idempotent
contract로 보정한 뒤, readiness, installed update/rollback, Windows Update가 적용된
dedicated clean-host install/update/rollback, Burn bootstrapper lifecycle, MSIX
build/install/update/remove, installed runtime ops summary, descriptor generation을 모두
PASS로 확인했다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260516-04222-04223` |
| baseline package root | `artifacts/admin-smoke-package-20260516-04222` |
| baseline MSI SHA-256 | `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3` |
| target package root | `artifacts/admin-smoke-package-20260516-04223` |
| target MSI SHA-256 | `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406` |
| target payload aggregate SHA-256 | `ab22bb9b2f9525991b31e5c1233bbfd5d8610556f5bcddc52a9570e02e8c195d` |
| provenance commit | `676b4177b10dc80209969066857bab6008ff2473` |
| update ZIP SHA-256 | `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb` |
| signing mode | `AllowUnsignedDev` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260516-04222-04223/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260516-04222-04223/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update` | `artifacts/manual-admin-campaign-20260516-04222-04223/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260516-04222-04223/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260516-04222-04223/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260516-04222-04223/installed-runtime-ops-summary/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260516-04222-04223/manual-admin-campaign-descriptor-supervised/summary.json` |

## 관찰

- Installed update/rollback은 baseline manifest `0.42.22-admin-smoke`, update 후
  `0.42.23-admin-smoke`, rollback 후 `0.42.22-admin-smoke`, final current
  `0.42.23-admin-smoke`를 확인했다.
- Clean-host는 blocker `none`, baseline `0.42.22-admin-smoke`, update
  `0.42.23-admin-smoke`, final rollback `0.42.22-admin-smoke`, Web Console HTTP
  `200`으로 닫혔다.
- Burn bundle SHA-256은
  `6b31e448762e54ee6e568509ff040c3eac4434a61bdb150c21211c6ef3beff9b`다.
- MSIX lifecycle은 `0.42.22.0 -> 0.42.23.0` build/install/update/remove를 통과했다.
  v1 SHA-256은 `5047a762f19db66520559667b34cc309ef59c2d20e0d9cd16954d746784e1042`,
  v2 SHA-256은 `fbb6882fd5217fe568f6fc5edb95e129b16232c68e79d765c4840c58ccc36310`이다.
- Installed runtime ops summary는 Web Console HTTP `200`, `/pcv-config.js` HTTP
  `200`, unauthenticated runtime policy `401`/`PCV_AUTH_REQUIRED`,
  `batch_evidence.status=available`, latest batch
  `full-admin-host-mutation-gate-20260516-04222`,
  `runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`,
  route detail count `4`를 확인했다.

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260516-04222-04223-closed` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04222-04223-closed/manifest.json` |
| batch summary | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04222-04223-closed/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260516-04222-04223/manual-admin-campaign-descriptor-supervised/summary.json` |
| overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |
| host mutation by descriptor batch | `false` |

이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing
또는 외부 stable publication evidence가 아니다.
