# C# architecture Wave 5A `0.42.68-admin-smoke` administrator install and PCVCLI smoke PASS (2026-08-03)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Package preflight: `docs/ga-ready/evidence/csharp-architecture-wave5a-package-preflight-2026-08-03-04268.md`
- Result: `INSTALL_PASS / INSTALLED_READONLY_CLI_SMOKE_PASS`
- Version: `0.42.68-admin-smoke`
- Install log: `artifacts/admin-smoke-package-20260803-04268/admin-install-0.42.68.log`
- CLI smoke artifact: `artifacts/installed-cli-smoke-20260803-04268/summary.json`
- Host/service mutation performed: `true` (administrator MSI install and installed service restart)
- Hyper-V/VM mutation performed: `false`
- Native provider mutation performed: `false`
- Actual-VM validation performed: `false`
- Package update/rollback/repair/uninstall/remove-data performed: `false`
- ASP.NET Core introduced: `false`
- TypeScript Web Console replaced: `false`
- Public trusted signing: `false`
- External stable publication: `false`

This is an explicitly approved internal administrator install and read-only installed operator
smoke. It is not a full-admin host mutation gate, actual-VM validation, manual-admin package-pair
closure or operational-anchor promotion.

## Package and MSI result

| Field | Result |
|---|---|
| MSI | `artifacts/admin-smoke-package-20260803-04268/PureCVisorDesktopNode-0.42.68-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `99957937f00c3f26392cae86df7ea090d84f6020821348cc6eb879dd667a2e70` |
| Payload aggregate SHA-256 | `b0e47050aab167890c1a3e0bec09e4eb6f4889eb1068c1896d58ec8f15d1afa8` |
| Provenance commit | `f93370610bf221da00e89131d874e903ba72b644` |
| Signing/channel | `AllowUnsignedDev / LocalTest` |
| MSI invocation | administrator `msiexec /i /qn /norestart /L*v` |
| MSI result | PASS, exit code `0`, `MainEngineThread is returning 0` |
| Installed display version | `0.42.68` |

The product manifest is schema `2` with version `0.42.68-admin-smoke`. Installed payload hashes
match the package preflight: Host `21c49044338b2756596723c56f597837ef5b66bb5407379c0923b56ada06d485`
and PCVCLI `ca106ca95ed4a10ad2c0ec0ad67bfe4f50f31ef0d6de1d95b83c1243b562cc56`.

## Installed service and listener

| Verification | Result |
|---|---|
| Windows service | PASS, `PureCVisorDesktopNode`, `Running`, `Automatic`, `LocalSystem`, exit code `0` |
| Observed service PID | `154584` at smoke time |
| SCM endpoints | API `http://127.0.0.1:7777/`, Web `http://127.0.0.1:80/` |
| HTTP.sys ownership | PASS, both URL groups attached to `PureCVisorDesktopNode`; TCP owner is HTTP.sys/System PID `4` |
| Web `/` | PASS, HTTP `200` |
| Web `/pcv-config.js` | PASS, HTTP `200` |
| API unauthenticated `/api/v1/runtime/policy` | PASS, HTTP `401` protected boundary |
| Web-port API `/api/v1/runtime/policy` | PASS, HTTP `404` port-split rejection |

The service command line remains loopback-only and uses the protected Credential Manager token
target; no raw token appears in the SCM path. The installed service keeps the default `legacy`
HttpListener request lifetime because tracked admission is opt-in and was not enabled by the MSI
service command line.

## Administrator PCVCLI read-only smoke

The smoke was run from an elevated process without placing a token in argv. The protected token was
resolved by the installed CLI and no token, password or refresh token value was recorded.

| Command | Result |
|---|---|
| `pcvcli --format json runtime policy` | PASS, exit `0` |
| `pcvcli --format json host status` | PASS, exit `0`; Hyper-V feature, VMMS and Default Switch reported available |
| `pcvcli --format json ops summary` | PASS, exit `0`; summary errors `0`, job store healthy |

Artifact summary: `artifacts/installed-cli-smoke-20260803-04268/summary.json` (`3/3` commands
exit `0`). The CLI smoke only read runtime policy, host capability and persisted operations
summary; it did not create, start, stop, rename, delete or otherwise mutate a VM.

## Port reservation and data-root checks

- IPv4 and IPv6 excluded-port tables contain exact `80-80` and `7777-7777` rows but no former
  covering range `7765-7864`.
- The exact `7777-7777` row is coupled to the active HTTP.sys listener and is not evidence of an
  HNS, WinNAT or WSL port owner.
- `hnsdiag list portranges` has no active product range, `Get-NetNat` is empty and
  `netsh interface portproxy show all` is empty.
- WSL `Ubuntu-24.04` is `Stopped`.
- `C:\ProgramData\PureCVisor\desktop-node\jobs.json` remains 18 jobs, queue/running `0`,
  SHA-256 `78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b`; the install did
  not rewrite the authoritative job store.

## Remaining gates and nonclaims

The installed candidate is available for the next explicitly approved Wave 5A checks, but the
following remain open: tracked-mode admission/load/cancellation and drain verification, account
login/RBAC/noVNC/diagnostics lifecycle smoke where stale, full admin host mutation and actual-VM
validation, package update/rollback pair, ASP.NET Core transport decision/implementation, and
operational promotion. This evidence does not claim public trusted signing, external stable
publication, Hyper-V side-effect exactly-once, or replacement of the TypeScript Web Console.
