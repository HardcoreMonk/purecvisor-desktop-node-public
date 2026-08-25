# Admin smoke package 2026-05-27 0.42.53

evidence_id: `admin-smoke-package-2026-05-27-04253`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-guest-execution-provider-direct-control`
version: `0.42.53-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260527-04253`
provenance: `artifacts/admin-smoke-package-20260527-04253/PureCVisorDesktopNode-0.42.53-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260527-04253/PureCVisorDesktopNode-0.42.53-admin-smoke-windows-x64.msi`
msi_sha256: `39df998c061d9dcecbbc21a966f9ffb495f27502922f2057bd5defc93c9a19ea`
payload_aggregate_sha256: `7cdf2a98d2076149b0c1e6215d85e6b92968066308e15c77aa2eb25fe80745d9`
provenance_commit: `cc774b257d6cd772c3a890266aca62aa8ab8eadc`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.53-admin-smoke` package는 Guest Execution provider, channel verify/repair,
CLI/Web/TUI direct-control payload를 포함한다. Runtime policy와 preview response는
provider-open 상태를 보고한다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `4bb8a57a28df76b42a0299a31e49351520d5dc4031b3fa0a5245e06ef469b8b5` |
| PCVCLI EXE | `546fcea34bee82a6f66319c555830a3ab55c0a5fcfa9ad0c52ebee31756a94d3` |
| PCVTUI EXE | `51534c3e512eb64c5ee1d4016cfc9295d17103872048443f0972e4864be1924e` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.53-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260527-04253 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

초기 `0.42.51`/`0.42.52` package 후보는 runtime policy와 preview response 드리프트를
찾아 superseded로 처리했고, 최종 current package는 `0.42.53-admin-smoke`다.

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.

