# Account/RBAC/JWT Console Code-Level Evidence - 2026-05-10

evidence_id: account-rbac-jwt-console-code-level-2026-05-10
scope: windows-desktop-node-account-rbac-jwt-console
created_at: 2026-05-10T00:00:00+09:00
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

This evidence records the Windows Desktop Node account/RBAC/JWT and console code-level slice.

Implemented Local API routes:

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/session`
- `GET /api/v1/auth/rbac`
- `GET /api/v1/console/capabilities`
- `GET /api/v1/vms/{id}/console`

Implemented Web Console UX:

- account login, refresh, logout, session, RBAC role/permission display
- RBAC disabled/forbidden handling for lifecycle, checkpoint, job, diagnostic, and console actions
- console capability panel with Windows `vmconnect` handoff
- noVNC shown as default disabled in this slice; follow-up bridge support is recorded by `installed-account-login-novnc-bridge-code-level-2026-05-10`

Installed service provisioning contract:

- service binary path includes `--account-file "%ProgramData%\PureCVisor\desktop-node\accounts.json"`
- service binary path includes `--jwt-signing-key-file "%ProgramData%\PureCVisor\desktop-node\jwt-signing-key.txt"`
- native `configure-installed`/`repair-installed` prepares `accounts.json` and `jwt-signing-key.txt`
- `accounts.json` starts with an empty account list and `bootstrap_state=no-default-account`
- account auth does not take over protected bearer-token routes until accounts and signing key are ready

## Verification

Commands run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiAccountAuthRequestProcessorTests
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostApplicationTests|FullyQualifiedName~DesktopNodeHostServiceActionTests|FullyQualifiedName~DesktopNodeHostOptionsTests"
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1,packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1,packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1 -Output Detailed
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
npm test --prefix web
npm run verify:parity --prefix web
dotnet test src/DesktopNode.sln
git diff --check
```

Observed final results:

- API account auth tests: 6 passed
- Host targeted tests: 72 passed
- Packaging product plan/manifest/invoke Pester: 90 passed
- Web static Pester: 44 passed
- Admin smoke evidence docs Pester: 21 passed
- Web npm test and parity fixture: passed
- Follow-up focused reruns: DesktopNode.Api.Tests 139 passed, DesktopNode.Tui.Tests 115 passed
- `git diff --check`: exit 0, with LF/CRLF conversion warnings only

## Boundary

This evidence itself does not claim target-backed noVNC streaming on an installed service, public trusted signing, external stable publication, winget submission, HTTPS/443 binding, firewall mutation, trust-store mutation, LAN mutation, Event Log mutation, MSI lifecycle, or Hyper-V lifecycle execution.

The account auth default is intentionally safe: no default password or account is created. Operators must add account records with PBKDF2 password hashes before username/password login becomes ready.

Follow-up `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md` records installed account login smoke PASS in `artifacts/installed-account-login-smoke-20260510-0410-final` and explicit-target noVNC WebSocket-to-VNC TCP bridge code-level PASS. It still does not claim public release evidence.
