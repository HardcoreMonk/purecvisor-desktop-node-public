# Admin smoke package 2026-07-13 0.42.62

evidence_id: `admin-smoke-package-2026-07-13-04262`
result: `PACKAGE_BUILD_PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.62-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260713-04262`
provenance: `artifacts/admin-smoke-package-20260713-04262/PureCVisorDesktopNode-0.42.62-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260713-04262/PureCVisorDesktopNode-0.42.62-admin-smoke-windows-x64.msi`
msi_sha256: `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`
payload_aggregate_sha256: `0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e`
provenance_commit: `7f71f0a518c5b592f233373522d36b5401c3f1df`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Payload

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `2620470c7279952e5b69e4cc331232f8813bcdfe0bad5f1a22568ad34ad04025` |
| PCVCLI EXE | `adb7a2ec6b8c803d5723b03a9ddff362235dafc6f4eaccca04f3bfc30eb130d0` |
| PCVTUI EXE | `67e7b73ee798ddf1fc29df317529dbf591ca2f22609046f2d8be617a81f2b4fb` |

이 package는 arbitrary-name internal switch를 WMI association topology로 판정하면서
association traversal에 필요한 완전한 WMI object path를 보존하는 복구 payload다.
`0.42.60`과 `0.42.61` package의 MSI lifecycle PASS는 predecessor로 보존하지만 두 버전의
full gate는 실패했으므로 PASS anchor로 승격하지 않는다.

이 evidence는 unsigned local-test internal admin-smoke package build이며 public trusted signing
또는 외부 stable publication을 주장하지 않는다.
