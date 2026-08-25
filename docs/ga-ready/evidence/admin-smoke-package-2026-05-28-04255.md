# Admin smoke package 2026-05-28 0.42.55

evidence_id: `admin-smoke-package-2026-05-28-04255`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-web-tui-running-cancel-affordance-installed-payload`
version: `0.42.55-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260528-04255`
provenance: `artifacts/admin-smoke-package-20260528-04255/PureCVisorDesktopNode-0.42.55-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260528-04255/PureCVisorDesktopNode-0.42.55-admin-smoke-windows-x64.msi`
msi_sha256: `530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627`
payload_aggregate_sha256: `ada13e719c47a439c8836fc2138f6419d447fc1eccfcd02fe73d3686a2127ef6`
provenance_commit: `958052181012f7d1be6ccff535316bfaeeef07df`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.55-admin-smoke` package는 Web/TUI running guest execution cancel affordance를
설치본 payload로 승격한다. Runtime/API Guest Execution provider, channel verify/repair,
queued/running cancel policy, Hyper-V VM Notes 기반 `guest_family` projection은 0.42.54
계열의 predecessor capability를 유지한다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `058ea3fc138b2d3d9fccbef17d40703461215f7e154e3e8e0a3ead665db5bf1b` |
| PCVCLI EXE | `d2355a4222bc7aa909907369d1b3b26c0027249c45a097049d16b2f3a5b65c91` |
| PCVTUI EXE | `dbbcd57b4ad40311d3967e745a9595108d11c00e8f73e084a68c0ab05046885f` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.55-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260528-04255 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
