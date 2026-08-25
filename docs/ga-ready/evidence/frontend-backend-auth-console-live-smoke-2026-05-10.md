# Frontend/Backend Auth Console Live Smoke Evidence - 2026-05-10

evidence_id: frontend-backend-auth-console-live-smoke-2026-05-10-235543
scope: frontend-backend-route-contract-auth-console-installed-browser-live-smoke
status: pass
route_coverage_metadata: auth_logout-added
api_handler_adapter_contract: auth-console-routes-added
installed_listener_execution: installed-listener-browser-live-smoke-pass
web_console_url: http://127.0.0.1/
api_base_url: http://127.0.0.1:7777
account_smoke_artifact_root: artifacts/installed-account-login-browser-live-smoke-20260510-235543
browser_qa_artifact_root: artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543
installed_web_asset_refresh_artifact_root: artifacts/installed-web-asset-refresh-20260510-235258
host_mutation_performed: true
host_mutation_scope: installed web/app.js refresh, temporary account/JWT replacement, service restart/restore
token_value_observed: false
password_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## 요약

이 evidence는 2026-05-10 frontend/backend route contract review 이후 남아 있던
auth/console 통합 gap을 설치본 live smoke로 닫은 기록이다. 설치본 Web Console은
실제 account login form을 통해 account JWT/RBAC session state를 채운 뒤 diagnostic,
console, responsive browser QA를 수행했다. Service bearer token을 account JWT처럼
사용하는 우회는 사용하지 않았다.

이 실행으로 `DESKTOP_NODE_ROUTE_COVERAGE`에는 `POST /api/v1/auth/logout`가
추가되었고, `ApiHandlerAdapterContract`에는 account auth와 console route가 함께
기록되었다.

확인한 route contract:

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/session`
- `GET /api/v1/auth/rbac`
- `GET /api/v1/console/capabilities`
- `GET /api/v1/vms/{vmId}/console`

Web Console은 성공적인 diagnostic bundle list refresh 이후 stale diagnostic auth
error를 지웠다. Browser QA는 diagnostic create/download button action, responsive
screenshots, missing label/input accessibility probe를 같은 임시 account boundary
안에서 확인했다.

## 설치본 Evidence

| 항목 | 값 |
| --- | --- |
| Installed web asset refresh artifact | `artifacts/installed-web-asset-refresh-20260510-235258` |
| Before `app.js` SHA-256 | `065b724b1a5e75bc87a491c6c0ca0d349a35cb2b8a90eb90ab9563d5edecf9e4` |
| Repo/after `app.js` SHA-256 | `53c2cd53248cb57d586c50092ead1791ced3089912005f4f525be0b4d8c82bc9` |
| Service restart required for asset refresh | `false` |
| Account smoke root | `artifacts/installed-account-login-browser-live-smoke-20260510-235543` |
| Browser QA root | `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543` |
| Installed account login smoke | `pass` |
| Restore status | `restored` |
| Service restart status | `restarted-after-restore` |
| Login/session/RBAC/console status | `200` / `200` / `200` / `200` |
| Runtime auth mode | `account_rbac_jwt` |
| Browser QA status | `pass` |
| Screenshot count | `8` |
| Diagnostic create clicked | `true` |
| Diagnostic download clicked | `true` |
| Missing button labels | `0` |
| Unlabeled inputs | `0` |
| Token value observed | `false` |
| Password value observed | `false` |

Screenshot hash inventory:

| Viewport | Size | SHA-256 |
| --- | --- | --- |
| dashboard-wide | `2048x1152` | `7073e8b67d87f77987b7d776f8528e5a9e65d041240711a4f13b5cd4744e05de` |
| vm-detail | `1366x900` | `f996f3c30bf9e497b7ed190321b8e3864e943365d4c532147e088ac1e891d2b4` |
| jobs | `1366x900` | `507c3daa105e95936e74a3a7fb6dc066921d8863c5abf46c5f452b8d917e767a` |
| network | `1366x900` | `855ad67c415e04675a1f7f2b9c20eca735ab27aea6a135f27986d6885eff73eb` |
| troubleshooting-diagnostics | `1366x900` | `5e5eac0900de474e6f92f1a7900be4700dd8a98db0843e478ca99ed3ea2a4038` |
| dashboard-1366 | `1366x768` | `f2172d113122f9a47d6109fea3807bf215cbbc82124eeb9aef75ef453bc4ad12` |
| dashboard-tablet | `900x900` | `2972258fe5db46de8c4ef8deda78ffae5063128710ced8b0c0884227700163e1` |
| dashboard-mobile | `390x860` | `da2d25577f7058116f4e410592e6bd59bacefd1090cc3b661ca588481c45f2fa` |

## Verification

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests --no-restore
Invoke-Pester -Path web\tests\PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
Invoke-Pester -Path packaging\windows-desktop-node\tests\PcvInstalledAccountLoginSmoke.Tests.ps1 -Output Detailed
node --check web\scripts\capture-installed-listener-qa.mjs
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-browser-live-smoke-20260510-235543 -RunBrowserQa -BrowserQaUrl http://127.0.0.1/ -BrowserQaArtifactRoot artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543
```

## 경계

이 evidence는 내부 설치 service에서 수행한 account/JWT/Web Console live smoke다.
설치본 `web/app.js` refresh, 임시 account/JWT 교체, service restart/restore는
host mutation으로 기록한다. HTTPS/443 publication, public trusted signing, winget
submission, external stable publication/catalog upload, public distribution readiness는
주장하지 않는다.
