# MANUAL-ADMIN 캠페인 2026-05-16 04220->04221

```text
evidence_id: manual-admin-campaign-2026-05-16-04220-04221
result: PASS
package_pair: 0.42.20-admin-smoke -> 0.42.21-admin-smoke
baseline_version: 0.42.20-admin-smoke
target_version: 0.42.21-admin-smoke
host_mutation_performed: true
manual_admin_descriptor_generation_contract: manual-admin-descriptor-generation-contract-v2
descriptor_batch_id: manual-admin-campaign-descriptor-20260516-04220-04221
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.20-admin-smoke -> 0.42.21-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign을 닫는다. Readiness, installed
update/rollback, Windows Update가 적용된 dedicated clean-host
install/update/rollback, Burn bootstrapper lifecycle, MSIX build/install/update/remove,
installed runtime ops summary, descriptor generation이 모두 PASS다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260516-04220-04221` |
| baseline package root | `artifacts/admin-smoke-package-20260516-04220` |
| baseline MSI SHA-256 | `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f` |
| target package root | `artifacts/admin-smoke-package-20260516-04221` |
| target MSI SHA-256 | `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b` |
| target payload aggregate SHA-256 | `3bbbbf22f238993d20f7c6674d753d90e32e0eb7958d61c313dbf0817ac88789` |
| provenance commit | `3b8c48deb4c31675f6fce46c320703f23c27c131` |
| update ZIP SHA-256 | `09e1c3f5a7c8d2afac3d70bddbb1d91f575de2c45c9174a8da2bbb73c2e89767` |
| signing mode | `AllowUnsignedDev` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260516-04220-04221/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260516-04220-04221/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update` | `artifacts/manual-admin-campaign-20260516-04220-04221/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260516-04220-04221/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260516-04220-04221/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260516-04220-04221/installed-runtime-ops-summary/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260516-04220-04221/manual-admin-campaign-descriptor-supervised/summary.json` |

## 관찰

- Installed update/rollback은 baseline manifest `0.42.20-admin-smoke`, update 후
  `0.42.21-admin-smoke`, rollback 후 `0.42.20-admin-smoke`를 확인했다.
- Clean-host는 blocker `none`, baseline `0.42.20-admin-smoke`, update
  `0.42.21-admin-smoke`, final rollback `0.42.20-admin-smoke`, Web Console HTTP `200`으로 닫혔다.
- Burn bundle SHA-256은 `f0d6a18f233261ed7609ce4689a28b7aa05b68c905f7a676a2d340c378c239ee`다.
- MSIX lifecycle은 `0.42.20.0 -> 0.42.21.0` build/install/update/remove를 통과했다.
- Installed runtime ops summary는 `batch_evidence.status=available`,
  `latest.batch_id=full-admin-host-mutation-gate-20260516-04221`,
  `runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`를 확인했다.

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260516-04220-04221` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04220-04221/manifest.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260516-04220-04221/manual-admin-campaign-descriptor-supervised/summary.json` |
| overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |
| host mutation by descriptor batch | `false` |

이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing
또는 외부 stable publication evidence가 아니다.

