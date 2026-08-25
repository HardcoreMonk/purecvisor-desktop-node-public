# MANUAL-ADMIN 캠페인 2026-05-12 0425→0426

```text
evidence_id: manual-admin-campaign-2026-05-12-0425-0426
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.5-admin-smoke
target_version: 0.42.6-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.3 -> 0.42.4` campaign에서 발견한 MSI custom action sequence
blocker를 반영한 다음 package-pair 재검증이다. `0.42.5-admin-smoke` baseline과
`0.42.6-admin-smoke` target으로 installed update/rollback, dedicated clean-host
install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove,
installed runtime ops summary capture를 모두 확인했다.

이 evidence는 internal/admin-smoke 운영 evidence다. Public trusted signing, winget,
external stable publication, public clean-host release claim은 추가하지 않는다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260512-0425` |
| baseline MSI | `PureCVisorDesktopNode-0.42.5-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `7693276610f0d5f5d11252cd307b682e3fde2e715a878dbeff772635f0475a2c` |
| target package root | `artifacts/admin-smoke-package-20260512-0426-r2` |
| target MSI | `PureCVisorDesktopNode-0.42.6-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `feccda5f205e95010d8ebeb756e7bcbae72136a5879f2650aea6e7a0066d1f83` |
| update package | `artifacts/manual-admin-campaign-20260512-0425-0426/lifecycle/PureCVisorDesktopNode-0.42.6-admin-smoke-update.zip` |
| update package SHA-256 | `0e2fa5eeed2a19d48fc2ba7c6e91c03e7910337118c5d319d88313b35a5bc803` |

## Post-merge Provenance 재빌드

PR merge 이후 `0.42.6-admin-smoke` package provenance를 merge commit에 맞춰 다시
빌드했다. 이 rebuild는 다음 campaign input 후보이며, 이 문서의 lifecycle PASS를
소급 교체하지 않는다.

| 항목 | 값 |
| --- | --- |
| post-merge package root | `artifacts/admin-smoke-package-20260512-0426-postmerge` |
| post-merge MSI SHA-256 | `9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16` |
| post-merge provenance commit | `37f4d6b83d6caef1338e0a60e5df0a60209b51f8` |
| signing mode | `AllowUnsignedDev` |
| follow-up triage | `docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md` |

## Campaign Descriptor

새 descriptor 도구는 이미 실행된 evidence를 읽어 package-pair campaign 상태를 한
JSON으로 묶는다. 이 도구 자체는 `PlanOnly` read-only descriptor이며 host mutation을
하지 않는다.

- Tool: `packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1`
- Descriptor root: `artifacts/manual-admin-campaign-20260512-0425-0426/manual-admin-campaign-descriptor`
- Summary: `overall_status=pass`, `runner_count=6`, `missing_count=0`, `not_pass_count=0`
- Readiness: `artifacts/manual-admin-campaign-20260512-0425-0426/manual-admin-rebaseline-readiness-r2/summary.json`

## Installed Update/Rollback

Installed-host product update/rollback은 PASS다.

- Baseline install snapshot: `artifacts/manual-admin-campaign-20260512-0425-0426/installed-baseline/baseline-0425-installed-snapshot.json`
- Update PASS: `artifacts/manual-admin-campaign-20260512-0425-0426/lifecycle/product-update-rollback/update-0425-to-0426-summary.json`
- Updated snapshot: `artifacts/manual-admin-campaign-20260512-0425-0426/lifecycle/product-update-rollback/updated-0426-installed-snapshot.json`
- Rollback PASS: `artifacts/manual-admin-campaign-20260512-0425-0426/lifecycle/product-update-rollback/rollback-0426-to-0425-summary.json`
- Final rollback snapshot: `artifacts/manual-admin-campaign-20260512-0425-0426/lifecycle/product-update-rollback/rollback-0425-installed-snapshot.json`

최종 installed manifest는 `0.42.5-admin-smoke`, service는 `Running`이다.

## Dedicated Clean-host

Dedicated clean-host install/update/rollback은 PASS다.

- Artifact root: `artifacts/manual-admin-campaign-20260512-0425-0426/clean-host`
- Result: `internal_clean_host_install_update_rollback_smoke=pass`
- Baseline manifest: `0.42.5-admin-smoke`
- Updated manifest: `0.42.6-admin-smoke`
- Final manifest: `0.42.5-admin-smoke`
- Final service: `Running`
- Final Web Console: HTTP `200`
- Blocker: `none`

Windows Update는 clean-host guest에서 요청/적용됐고, reboot 후 PowerShell Direct
재접속으로 install/update/rollback을 완료했다.

## Burn Bootstrapper

Burn bootstrapper lifecycle은 PASS다.

- Artifact root: `artifacts/manual-admin-campaign-20260512-0425-0426/burn-bootstrapper-lifecycle-r2`
- Summary: `burn-lifecycle-summary.json`
- Install exit code: `0`
- Repair exit code: `0`
- Remove exit code: `0`
- Baseline restore exit code: `0`
- Final installed state: `0.42.5-admin-smoke`, service `Running`

첫 Burn repair 시도는 `CredentialManagerDefaultTransitionRepair`가 repair에서
일회성 Credential Manager migration을 다시 실행해 실패했다. 이 branch는 MSI repair
sequence에서 해당 repair custom action을 제거했다. `RepairInstalled`는 이미
Credential Manager token source를 보존하므로 repair에서 migration을 재실행하지 않는다.

## MSIX Lifecycle

MSIX internal package lifecycle은 PASS다.

- Artifact root: `artifacts/msix-package-lifecycle-smoke-20260512-0425-0426`
- Summary: `summary.json`
- v1 package: `PureCVisorDesktopNode-MsixSmoke-0.42.5.0.msix`
- v1 SHA-256: `6fa40fb40578a357a94b0a21020949bb616f0a1953cbc10947e450e9782a70a7`
- v2 package: `PureCVisorDesktopNode-MsixSmoke-0.42.6.0.msix`
- v2 SHA-256: `ccc5b08b41dad52366ad5f150a9f8c5fa6468ac429be07fe1e034f729121ba71`
- Install: PASS
- Update: PASS
- Remove: PASS
- Final MSIX package/service absence: PASS

이 MSIX evidence는 internal Root/leaf signing smoke이며 public trusted signing 또는
external stable publication evidence가 아니다.

## Installed Runtime Ops Summary

Installed runtime ops summary capture는 PASS다.

- Artifact root: `artifacts/manual-admin-campaign-20260512-0425-0426/installed-runtime-ops-summary`
- Command surface: installed `pcvcli.exe --protected-token-file ... --json ops summary`
- Route: `GET /api/v1/ops/summary`
- Auth token value observed: `false`
- Token storage observed: `windows-credential-manager`
- Unauthenticated API boundary: `PCV_AUTH_REQUIRED`
- Network exposure: `loopback`
- Console mode: `windows-hyperv-console-handoff`
- Public trusted signing: `not-claimed`
- External stable publication: `not-claimed`

## 판정

`0.42.5 -> 0.42.6` package-pair는 installed update/rollback, dedicated clean-host,
Burn, MSIX, installed runtime summary 기준으로 PASS다. 0423→0424 clean-host blocker는
역사 evidence로 보존하고, current package-pair claim은 이 문서와 descriptor
summary를 기준으로 한다. Post-merge `0.42.6-admin-smoke` rebuild는 provenance-aligned
input으로 보존하며, 새 full admin host mutation claim은 다음 version/elevated campaign이
별도로 소유한다.
