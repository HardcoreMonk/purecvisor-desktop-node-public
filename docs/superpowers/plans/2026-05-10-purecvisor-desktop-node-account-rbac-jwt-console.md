# PureCVisor Desktop Node Account/RBAC/JWT/Console Slice

## Goal

Add a Windows Desktop Node scoped account-auth slice:

- local account login with JWT access token
- refresh-token flow
- role-based authorization for read, operator mutation, diagnostics, and console routes
- Web Console session UX for login, refresh, logout, RBAC state, and console capability

## Boundaries

- Keep this repository Windows-only. Do not add Linux `purecvisor-single`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime code.
- Keep the existing service bearer-token path compatible for installed/internal automation.
- Do not hardcode product passwords or render token/password values.
- noVNC is surfaced as a capability-gated console feature. Until a Windows VNC/WebSocket bridge is configured, the Local API reports `novnc.enabled=false` and exposes the Hyper-V `vmconnect` handoff contract.

## Backend Contract

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/session`
- `GET /api/v1/auth/rbac`
- `GET /api/v1/console/capabilities`
- `GET /api/v1/vms/{id}/console`

Roles:

- `viewer`: read-only API state
- `operator`: read state, queue VM/checkpoint/job/diagnostic actions, view console handoff
- `admin`: all operator permissions plus account administration placeholder

## Implementation Order

1. Add failing backend tests for login, refresh, RBAC denial, and console capabilities.
2. Implement account auth options, PBKDF2 password verification, HMAC-SHA256 JWT issue/validate, session and RBAC payloads.
3. Wire Host listener to pass JWT Authorization through while preserving service bearer-token behavior.
4. Add failing Web Console static tests for auth routes, session controls, RBAC disabled states, and console panel.
5. Implement Web Console login/refresh/logout/session state and console capability panel.
6. Run targeted .NET tests, Web static/parity tests, generated asset check, and `git diff --check`.

## Follow-Up Closure

- Service binary path now carries `--account-file` and `--jwt-signing-key-file`.
- Native `configure-installed` and `repair-installed` prepare `accounts.json` and `jwt-signing-key.txt` bootstrap files.
- Bootstrap account file intentionally contains no default account/password.
- When account files are present but no account is configured, protected bearer-token auth remains authoritative.
- Code-level evidence is recorded in `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`.
- Installed account login smoke and noVNC bridge are tracked by follow-up evidence in `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`. Installed account login execution is `installed-admin-smoke-pass` in `artifacts/installed-account-login-smoke-20260510-0410-final`, and noVNC bridge remains disabled until explicit target host/port configuration.
