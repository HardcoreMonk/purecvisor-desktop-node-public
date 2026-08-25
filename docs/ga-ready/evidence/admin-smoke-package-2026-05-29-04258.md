# Admin smoke package 2026-05-29 0.42.58

evidence_id: `admin-smoke-package-2026-05-29-04258`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-post-04257-public-boundary-main-push-anchor`
version: `0.42.58-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260529-04258`
provenance: `artifacts/admin-smoke-package-20260529-04258/PureCVisorDesktopNode-0.42.58-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260529-04258/PureCVisorDesktopNode-0.42.58-admin-smoke-windows-x64.msi`
msi_sha256: `6ae889eeb1b7134fab9618941748528f6260727abbc8ff36eee301b59dff6c0b`
payload_aggregate_sha256: `9e162bc59527d107c0c6e35105bd5a0f17c7449a94e23cfe138cdc268f3d7184`
provenance_commit: `96182b440b35c17183802ad323a123ff6e4b6730`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.58-admin-smoke` package는 `0.42.57-admin-smoke` manual-admin closure 이후
main head `96182b440b35c17183802ad323a123ff6e4b6730`을 기준으로 빌드했다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `0560bf745fdd776c56126dff4e732d47f98180cedb9ba19f536d568b5c4a09ef` |
| PCVCLI EXE | `77959b4ed24a5179edf8bee693e99e0ded6a3a480a00641eb429415496b44f71` |
| PCVTUI EXE | `d74903282344a0b14b753bb5720697e84894392f8f22ea5beedc07545230922a` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.58-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260529-04258 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
