# Admin smoke package 2026-05-29 0.42.59

evidence_id: `admin-smoke-package-2026-05-29-04259`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-post-guest-execution-and-qos-hardening-anchor`
version: `0.42.59-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260529-04259`
provenance: `artifacts/admin-smoke-package-20260529-04259/PureCVisorDesktopNode-0.42.59-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260529-04259/PureCVisorDesktopNode-0.42.59-admin-smoke-windows-x64.msi`
msi_sha256: `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`
payload_aggregate_sha256: `666a1351d58963c7908aad4f66d6469de42747a7c7f70d1e30fb0e94771a5808`
provenance_commit: `63d57feba605f82dabd44a96ed50a4d622f6310a`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.59-admin-smoke` package는 Guest Execution redaction hardening과 Hyper-V QoS
mutation value hardening을 포함한 `63d57feba605f82dabd44a96ed50a4d622f6310a`
기준으로 빌드했다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `bb0c02fe07723a5636e36446e46c6bd41107ad6f8af32f27b0bb03f9304e04e6` |
| PCVCLI EXE | `473d2d4399fc4824251394634515810f2d80a10e242b56e5e459b265a1dc99d1` |
| PCVTUI EXE | `4a4521ea82ed782d29cb944b3908963a6317437f5819c120e8ac96b0e639a399` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.59-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260529-04259 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
