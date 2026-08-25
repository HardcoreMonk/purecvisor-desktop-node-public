# Admin smoke package 2026-05-28 0.42.54

evidence_id: `admin-smoke-package-2026-05-28-04254`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-running-guest-cancel-installed-payload`
version: `0.42.54-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260528-04254`
provenance: `artifacts/admin-smoke-package-20260528-04254/PureCVisorDesktopNode-0.42.54-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260528-04254/PureCVisorDesktopNode-0.42.54-admin-smoke-windows-x64.msi`
msi_sha256: `a0181bd156e4e01a57c177639a3eb418009f6fd9dd8bf090a3bb123e69aad36b`
payload_aggregate_sha256: `8443b217a45551bfcaf28d366ff33af80f95fc4527509addf4919621472f6bb3`
provenance_commit: `5a1058f55fcd42d28c7075514e1924c5ccdfb525`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.54-admin-smoke` package는 running guest execution cancel token path와
Hyper-V VM Notes 기반 `guest_family` projection fix를 포함한다. Runtime policy는
`job_runtime.control.cancel.running_interrupt=true`, `queued_only=false`,
`guest_execution.timeout.cancel=queued-and-running-guest-execution-cancel-with-provider-token-interrupt`를 보고한다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `35fba81abfaf44155032cc093c52f2b60f345ad7c232bf72cd4bbafc309c974d` |
| PCVCLI EXE | `998e18108b9ff630b1135e7ed1b4cbb013b1c297c06d63854b4df52a5314950b` |
| PCVTUI EXE | `ee4ac8c8e38231f2c04b4a0d275098bcccc47d6bbafc97512934a930df3a8bb8` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.54-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260528-04254 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
