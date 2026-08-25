# C# 구조 개선 Wave 5A package preflight (`0.42.68-admin-smoke`)

- evidence status: `PACKAGE_PASS / INSTALLED_BLOCKED`
- decision: `wave5a-admission-package-preflight-2026-08-03`
- source commit: `f93370610bf221da00e89131d874e903ba72b644`
- candidate version: `0.42.68-admin-smoke`
- current operational anchor: `0.42.65-admin-smoke` (unchanged)
- signing boundary: `AllowUnsignedDev / LocalTest`
- public trusted signing: `false`
- external stable publication: `false`
- Hyper-V mutation performed: `false`
- host/service mutation performed: `false`

## Package result

The Wave 5A bounded admission/task-tracking code slice was rebuilt as a self-contained Windows x64
MSI. The package keeps `legacy` HttpListener as the default and does not silently enable the
tracked mode or ASP.NET Core transport. This is a local internal/admin-smoke candidate only.

| item | result |
|---|---|
| installer dry-run | PASS |
| WiX | PASS (`5.0.2+aa65968c`) |
| self-contained `DesktopNode.Host.exe` publish | PASS |
| self-contained `pcvcli.exe` publish | PASS |
| MSI build | PASS |
| MSI SHA-256 | `99957937f00c3f26392cae86df7ea090d84f6020821348cc6eb879dd667a2e70` |
| payload aggregate SHA-256 | `b0e47050aab167890c1a3e0bec09e4eb6f4889eb1068c1896d58ec8f15d1afa8` |
| service-host SHA-256 | `21c49044338b2756596723c56f597837ef5b66bb5407379c0923b56ada06d485` |
| PCVCLI SHA-256 | `ca106ca95ed4a10ad2c0ec0ad67bfe4f50f31ef0d6de1d95b83c1243b562cc56` |
| product manifest | PASS (`schema_version=2`, `0.42.68-admin-smoke`) |

Artifact root:
`artifacts/admin-smoke-package-20260803-04268`.

## Installed-service gate

No install or service mutation was opened from the current non-elevated shell. The earlier
read-only port audit still requires a separate elevated operator run: active TCP 7777 is coupled to
the product HTTP.sys listener and the exact active exclusion row is not evidence of an HNS/WinNAT/WSL
portproxy owner. No MSI install, service create/start/stop/delete, firewall, reboot, or data-root
mutation was attempted by this package build.

Installed Web/API/PCVCLI smoke, service stop/start drain, package lifecycle, and Hyper-V/VM mutation
remain pending explicit administrator execution. This package candidate must not be promoted to the
`0.42.65-admin-smoke` operational anchor without those gates.

