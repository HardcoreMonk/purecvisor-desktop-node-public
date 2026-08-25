# Admin smoke package 2026-05-27 0.42.50

evidence_id: `admin-smoke-package-2026-05-27-04250`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-guest-execution-preview-api-cli`
version: `0.42.50-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260527-04250`
provenance: `artifacts/admin-smoke-package-20260527-04250/PureCVisorDesktopNode-0.42.50-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260527-04250/PureCVisorDesktopNode-0.42.50-admin-smoke-windows-x64.msi`
msi_sha256: `782f4417a5ad9ab0d1a4875bcf94c6473d0163340cd316d3cd715257c302072a`
payload_aggregate_sha256: `c2fbb63bede628a62de02803a2da9ce292cc0be3c6be837416838c79b4d89585`
provenance_commit: `d42ff7fddc67cbcebbfcbbec3342278511edafb3`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.50-admin-smoke` package는 Guest Execution API/CLI preview payload를 포함한다.
실제 guest command execution, channel verify/repair, Web/TUI direct command control은
포함하지 않는다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `a1e9f95d5646473cf528fd93f632c27d0b8d69d98c0df466c84450bdf6cfa743` |
| PCVCLI EXE | `fb99950e842f8448fa5e189c074ce091871723c883a96fd153ff0d772a084c5f` |
| PCVTUI EXE | `634f903f1649f42366149880de66e37be70fa8d0a3a034d321e05a27eceb08b9` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.50-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260527-04250 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
