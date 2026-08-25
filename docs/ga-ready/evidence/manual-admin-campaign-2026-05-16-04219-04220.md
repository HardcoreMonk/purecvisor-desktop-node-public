# MANUAL-ADMIN 캠페인 2026-05-16 04219->04220

```text
evidence_id: manual-admin-campaign-2026-05-16-04219-04220
result: PASS
baseline_version: 0.42.19-admin-smoke
target_version: 0.42.20-admin-smoke
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.19-admin-smoke -> 0.42.20-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign을 닫는다. Readiness, installed
update/rollback, Windows Update가 적용된 dedicated clean-host
install/update/rollback, Burn bootstrapper lifecycle, MSIX build/install/update/remove,
installed runtime ops summary, descriptor generation을 모두 통과했다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260516-04219-04220` |
| baseline package root | `artifacts/admin-smoke-package-20260515-04219` |
| baseline version | `0.42.19-admin-smoke` |
| baseline MSI SHA-256 | `3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55` |
| target package root | `artifacts/admin-smoke-package-20260516-04220` |
| target version | `0.42.20-admin-smoke` |
| target MSI SHA-256 | `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f` |
| target payload aggregate SHA-256 | `d3a2d660b27daf7ee120a5a31cd11ab36fa3fde36ac0f76434d6ae5d6cfe4ed2` |
| target wrapper SHA-256 | `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f` |
| target host SHA-256 | `d6e44f8b7fb3921453532c9900fec9c8f6b08f10813d07a33f2ca0c636674fa0` |
| target CLI SHA-256 | `052ee5909e61de0e600750f78f256b54c6b6b680394c91ab93a4aa12b772c242` |
| target TUI SHA-256 | `281b24a7fc27afa5dc17c14c03be038a165e1c2fd9da73b215718e6b08865ad0` |
| provenance commit | `0895d018935298721b25b5d9ce1ae083a6690c25` |
| signing mode | `AllowUnsignedDev` |
| update ZIP SHA-256 | `8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260516-04219-04220/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update` | `artifacts/manual-admin-campaign-20260516-04219-04220/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260516-04219-04220/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260516-04219-04220/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260516-04219-04220/installed-runtime-ops-summary/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-campaign-descriptor-supervised/summary.json` |

## 관찰

- Installed update/rollback은 baseline manifest `0.42.19-admin-smoke`, update 후
  `0.42.20-admin-smoke`, rollback 후 `0.42.19-admin-smoke`를 확인했다.
- Clean-host는 install/update/rollback exit `0`, blocker `none`, final Web Console
  HTTP `200`, final manifest `0.42.19-admin-smoke`로 닫혔다.
- Burn bundle SHA-256은
  `207d4245dba5b5b91de777bb98ce54193a9a0589d5bdf044c823576df6715fe8`다.
  install, repair, remove, baseline restore와 native repair가 모두 exit `0`이다.
- MSIX lifecycle은 `0.42.19.0 -> 0.42.20.0` build/install/update/remove를 통과했다.
  v1 SHA-256은 `bc495a018b1522b7dbbe35538f1c4560a94e6a6f524e98ab8369ca029a4ff7e2`,
  v2 SHA-256은 `09bcb2e7867183e733e3401329fb61797eef7fe3ba55891d6585cd49a2cff81b`다.
  Runner catch-summary type failure가 있었지만, 두 package build/sign 이후 직접
  `Add-AppxPackage` install/update/remove 검증으로 lifecycle을 통과했음을 별도
  summary에 기록했다.
- Installed runtime ops summary는 installed manifest `0.42.20-admin-smoke`, service
  `Running`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`,
  unauthenticated runtime policy `401` / `PCV_AUTH_REQUIRED`, absolute batch
  evidence root를 확인했다.

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260516-04219-04220` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04219-04220/manifest.json` |
| batch summary | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04219-04220/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-campaign-descriptor-supervised/summary.json` |
| overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |
| host mutation by descriptor batch | `false` |

## 결정

`0.42.19-admin-smoke -> 0.42.20-admin-smoke` package-pair는 PASS로 닫는다.
이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted
signing, public stable URL, winget submission, external stable publication을 주장하지
않는다. 최신 operational current-card anchor와 full admin host mutation PASS는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`가
소유한다.
