# Wave 6 package and installed-service preflight

- evidence status: `PACKAGE_PASS / INSTALLED_BLOCKED`
- decision: `wave6-package-preflight-2026-08-03`
- source commit: `fa5fbaa8930715f8d6d84fed60f94b5d9712ef92`
- candidate version: `0.42.67-admin-smoke`
- current operational anchor: `0.42.65-admin-smoke` (unchanged)
- signing boundary: `AllowUnsignedDev / LocalTest`
- public trusted signing: `false`
- external stable publication: `false`
- Hyper-V mutation performed: `false`
- host/service mutation performed: `false`

## Package result

The merged Wave 2C `checkpoint.create` payload was built as a self-contained Windows x64
MSI. This is a local internal/admin-smoke candidate only; it does not promote the operational
anchor or claim installed-service evidence.

| item | result |
|---|---|
| installer dry-run | PASS |
| WiX | PASS (`5.0.2+aa65968c`) |
| self-contained `DesktopNode.Host.exe` publish | PASS |
| self-contained `pcvcli.exe` publish | PASS |
| MSI build | PASS |
| MSI SHA-256 | `478a8befa7cb6612cc6e078bd2c529a24518686b2c536443040e6f76b803abd9` |
| payload aggregate SHA-256 | `d9dad4e34d12749ac5e619fecb6be0a0e6b89bb78087d312076073b54bf463eb` |
| service-host SHA-256 | `130cfd44741deb36d7de154b454f22000ad95c45b0f10feda30851acea542ca5` |
| PCVCLI SHA-256 | `05e1ad00b977ead3cbfb77802f4219f96f19a88060c9d5402c087d5d01af8b40` |
| product manifest | PASS (`schema_version=2`, `0.42.67-admin-smoke`) |

Artifact root:
`artifacts/admin-smoke-package-20260803-04267`.

## Installed-service gate

The read-only preflight did not open an install or service mutation gate:

- current PowerShell is not elevated (`elevated=false`);
- TCP `7777` is currently owned by PID 4/System;
- Windows excluded TCP range contains `7777-7777`;
- no MSI install, service create/start/stop/delete, firewall, reboot, or data-root mutation was
  attempted.

Therefore installed Web/API/PCVCLI smoke and Hyper-V mutation remain pending. The next operator
run must first use an elevated PowerShell session, resolve the TCP 7777 exclusion/ownership, and
re-run the read-only port/service preflight before invoking any MSI lifecycle command.

## ASP.NET Core boundary

This candidate still uses the current `System.Net.HttpListener` transport. ASP.NET Core remains a
separate Wave 5/6 code slice and was not silently enabled by this package build. No TypeScript Web
Console replacement, endpoint contract change, or transport-side mutation was introduced here.
