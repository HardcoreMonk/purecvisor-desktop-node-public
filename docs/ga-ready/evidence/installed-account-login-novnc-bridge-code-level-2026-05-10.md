# Installed Account Login And noVNC Bridge Evidence - 2026-05-10

evidence_id: installed-account-login-novnc-bridge-code-level-2026-05-10
status: installed-admin-smoke-pass-and-code-level-novnc-pass
actual_execution: installed-service-account-login-smoke-and-code-level-tests
host_mutation_performed: true
host_mutation_scope: temporary account/JWT file replacement, service restart, restore
installed_account_login_smoke_execution: installed-admin-smoke-pass
installed_account_login_smoke_runner: packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1
installed_account_login_smoke_artifact_root: artifacts/installed-account-login-smoke-20260510-0410-final
installed_account_login_smoke_status_codes: login=200 session=200 rbac=200 console=200
installed_account_login_smoke_runtime_auth_mode: account_rbac_jwt
installed_account_login_smoke_restore_status: restored
installed_account_login_smoke_service_restart_status: restarted-after-restore
installed_account_login_smoke_acl_restore_status: accounts.json=restored jwt-signing-key.txt=restored
novnc_bridge: code-level-websocket-to-vnc-tcp-pass
novnc_bridge_default: disabled-until-target-configured
novnc_websocket_path_template: /api/v1/console/novnc/{vm_id}
novnc_target_default: not-configured
token_value_observed: false
password_value_observed: false
refresh_token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed

## Scope

This evidence closes the installed account login smoke follow-up and the code-level noVNC bridge follow-up. The installed smoke ran against the installed LocalSystem service on `http://127.0.0.1:7777`, temporarily replaced the protected account/JWT files, restarted the service, verified account login/session/RBAC/console routes, restored the original files and ACLs, and restarted the service again.

The current installed account login smoke runner also supports optional `-RunBrowserQa`, `-BrowserQaUrl`, and `-BrowserQaArtifactRoot` parameters. Those parameters chain the installed Web Console browser QA after account login using the temporary access token and record a redacted `browser_qa` summary block. The canonical `artifacts/installed-account-login-smoke-20260510-0410-final` account-login evidence remains the installed account login smoke PASS and does not become public release evidence.

The noVNC bridge remains opt-in code-level functionality. It is disabled until explicit `--novnc-target-host` and `--novnc-target-port` are configured.

## Verified Commands

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiAccountAuthRequestProcessorTests
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostApplicationTests"
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1 -Output Detailed
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-smoke-20260510-0410-final
```

## Observed Result

- Installed account login smoke: PASS
- Installed smoke artifact root: `artifacts/installed-account-login-smoke-20260510-0410-final`
- Installed smoke HTTP status: login `200`, session `200`, RBAC `200`, console capabilities `200`
- Installed smoke runtime auth mode: `account_rbac_jwt`
- Installed smoke restore status: `restored`
- Installed service restart status after restore: `restarted-after-restore`
- Protected account/JWT ACL restore status: `accounts.json=restored`, `jwt-signing-key.txt=restored`
- API account/console focused tests: PASS
- Host option and listener focused tests: PASS
- noVNC bridge test proxied a binary WebSocket frame to a loopback TCP echo target and returned the same frame.
- Installed account login smoke runner static guard: PASS
- noVNC bridge requires explicit target host/port.
- Non-loopback noVNC target is rejected unless LAN mode is explicit.
- Account/JWT/password/token values are not recorded in evidence output.

## Boundaries

The noVNC bridge is Windows Desktop Node local listener functionality. It does not add Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime code. The bridge is disabled unless `--novnc-target-host` and `--novnc-target-port` are configured.

The installed account login smoke performed an elevated administrator opt-in host mutation limited to temporary account/JWT file replacement and service restart. The original files and protected ACLs were restored. Token, password, refresh token, JWT signing key, and bearer values were not recorded. This is not public trusted signing or external stable publication evidence.
