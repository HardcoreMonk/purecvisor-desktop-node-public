# Installed noVNC And TUI Operator Smoke Evidence - 2026-05-10

evidence_id: installed-novnc-tui-operator-smoke-2026-05-10-0411
scope: target-backed-novnc-installed-streaming-and-installed-tui-operator-smoke
created_at: 2026-05-10T15:45:53+09:00
product_version: 0.41.1-admin-smoke
source_version: 0.41.0-admin-smoke
host_mutation_performed: true
target_backed_novnc_installed_streaming_smoke: pass
installed_tui_operator_smoke: pass
token_value_observed: false
password_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

This evidence records two installed operator follow-ups against the installed `PureCVisorDesktopNode` service:

- Target-backed noVNC WebSocket streaming smoke: PASS.
- Installed interactive TUI operator smoke using `pcvtui.exe --smoke-once runtime`: PASS.

The product payload was rebuilt as `0.41.1-admin-smoke` from commit `a3226ef637ea895d2f2a9956599e0d5e79d00410` and applied over installed `0.41.0-admin-smoke`.

## Artifacts

- Build and product update root: `artifacts/installed-novnc-tui-operator-smoke-20260510-0411`
- noVNC installed streaming smoke: `artifacts/target-backed-novnc-installed-streaming-smoke-20260510-0411`
- TUI operator smoke: `artifacts/installed-tui-operator-smoke-20260510-0411`

Build details:

- MSI: `artifacts/installed-novnc-tui-operator-smoke-20260510-0411/build/PureCVisorDesktopNode-0.41.1-admin-smoke-windows-x64.msi`
- MSI SHA-256: `0583f71c4fcc1ed0da886e55f2fbac6713d8bc731fad7d33d6c189c214fcea6e`
- Payload aggregate SHA-256: `e6af8189b98f7af3ea4b25ecb9ce8e6d541ebae4d3252fd36fa83b89c2f5006a`
- Host SHA-256: `929cb622918f7c706833e3c8cf71ee50c923c9c66305fc8641df0bcef16a23d7`
- TUI SHA-256: `58ef426c3ca913281ee73fe016855de47fc177fa8b98b255f5cd30cc9fae5fa6`
- Signing mode: `AllowUnsignedDev`
- Signing trust model: `LocalTest`

Installed update result:

- Update: `0.41.0-admin-smoke -> 0.41.1-admin-smoke`
- Update journal: `succeeded`
- Health: HTTP `200`
- Final service: `Running`

## noVNC Streaming Smoke

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1 -ArtifactRoot artifacts/target-backed-novnc-installed-streaming-smoke-20260510-0411
```

Observed summary:

- `ok: true`
- `target_backed_novnc_installed_streaming_smoke: pass`
- `actual_execution: installed-service-target-backed-novnc-streaming-smoke`
- WebSocket path: `/api/v1/console/novnc/{vm_id}`
- Target host: `127.0.0.1`
- Target frame length: `49`
- Target frame SHA-256: `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
- Echoed frame SHA-256: `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
- Service `PathName` restored: `true`
- Final service: `Running`
- `host_mutation_performed: true`
- `token_value_observed: false`

This smoke temporarily configured the installed service with `--novnc-target-host`, `--novnc-target-port`, and `--novnc-websocket-path`, restarted the service, proxied a binary WebSocket frame into a loopback TCP target, verified the echo, then restored the original service `PathName`.

## TUI Operator Smoke

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1 -ArtifactRoot artifacts/installed-tui-operator-smoke-20260510-0411
```

Observed summary:

- `ok: true`
- `installed_tui_operator_smoke: pass`
- `actual_execution: installed-pcvtui-smoke-once-runtime-route`
- Initial tab: `runtime`
- Exit code: `0`
- Final service: `Running`
- `host_mutation_performed: false`
- `token_value_observed: false`

The redacted stdout artifact includes `PureCVisor Desktop Node TUI`, `api=reachable`, and `RUNTIME TABLE`. The runner removed raw stdout/stderr and retained only redacted artifacts.

## Verification

Focused verification executed in this workspace:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostApplicationTests" --no-restore
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvInstalledNoVncTuiSmoke.Tests.ps1 -Output Detailed
```

Observed result:

- `DesktopNode.Tui.Tests`: 115 passed, 0 failed, 0 skipped.
- `DesktopNode.Host.Tests` filtered noVNC/host listener coverage: 31 passed, 0 failed, 0 skipped.
- `PcvInstalledNoVncTuiSmoke.Tests.ps1`: 2 passed, 0 failed.

## Boundary

This is internal installed admin-smoke evidence. It does not claim public trusted signing, trusted timestamp evidence, external stable publication, winget submission, public installer URL, public release, or public clean-host signed install/update/rollback evidence.

Installed account login smoke execution is separate and is tracked as PASS by `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`.
