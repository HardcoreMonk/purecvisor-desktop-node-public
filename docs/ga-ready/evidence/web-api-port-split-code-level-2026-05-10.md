# Web/API Port Split Code-level Evidence - 2026-05-10

evidence_id: web-api-port-split-code-level-2026-05-10
scope: windows-desktop-node-host-listener-web-api-port-split
web_console_prefix: http://127.0.0.1:80/
web_console_url: http://127.0.0.1/
api_prefix: http://127.0.0.1:7777/
api_route_prefix: http://127.0.0.1:7777/api/v1/...
web_api_same_port: false
web_config_script: /pcv-config.js
api_route_on_web_port_error_code: PCV_API_ROUTE_ON_WEB_PORT
cors_allowed_origin: web-listener-origin
installed_listener_execution: not-run
installed_listener_followup: docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
https_443_binding: not-run
tls_binding: not-run

## Summary

The Desktop Node host listener now separates the default Web Console surface from
the Local API listener:

- Web Console static assets: `http://127.0.0.1/`
- Web API routes: `http://127.0.0.1:7777/api/v1/...`

The host accepts `DesktopNode.Host.exe listen --web-prefix` for the static Web
Console listener. When the Web listener is separate, `/pcv-config.js` is served
before `/app.js` and sets the browser API base URL to the Local API origin.

API routes are not served from the Web listener. A request to `/api/*` on the Web
port returns `PCV_API_ROUTE_ON_WEB_PORT`. The API listener allows the configured
Web origin through CORS so the port split remains browser-usable without copying
API routes onto port 80.

This is a code-level and documentation evidence slice. It did not install or
reconfigure the running Windows service, did not mutate URL ACLs, did not bind
HTTPS/443, and does not claim public trusted signing or external stable
publication. The later installed listener follow-up is recorded separately in
`docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`.

## Implementation Surface

- `src/DesktopNode.Host/DesktopNodeHostOptions.cs`: parses `--web-prefix` and
  requires a static web root when a separate Web Console listener is requested.
- `src/DesktopNode.Host/DesktopNodeHostApplication.cs`: starts separate API and
  Web listeners, serves `/pcv-config.js`, rejects API routes on the Web listener,
  and emits CORS headers for the configured Web origin.
- `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`: writes default service
  configuration with `--prefix http://127.0.0.1:7777/` and
  `--web-prefix http://127.0.0.1:80/`.
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: exposes
  `prefix` and `web_prefix` product defaults and passes both into the service
  plan.
- `web/index.html`: loads `/pcv-config.js` before `/app.js`.
- `web/src/served/state.ts`: resolves the API base URL from listener-provided
  config before falling back to the Web origin.

## Verification

Fresh verification in this workspace before documentation optimization:

```text
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeHostOptionsTests.ListenOptionsParseSeparateWebPrefix|FullyQualifiedName~DesktopNodeHostServiceActionTests.ConfigureInstalled"
PASS

Invoke-Pester -Path packaging\windows-desktop-node\tests\PcvDesktopNodeProduct.Plan.Tests.ps1 -Output Detailed
PASS: 22 passed, 0 failed

npm test --prefix web
PASS

dotnet test src\DesktopNode.sln --no-restore
PASS: 273 passed, 0 failed

Invoke-Pester -Path web\tests\PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
PASS: 42 passed, 0 failed

npm run verify:parity --prefix web
PASS

Invoke-Pester -Path packaging\windows-desktop-node\tests -Output Detailed
PASS: 269 passed, 0 failed

Invoke-Pester -Path packaging\windows-desktop-node\installer\tests -Output Detailed
PASS: 43 passed, 0 failed

git diff --check
PASS
```

Existing line-ending warnings in PowerShell files were informational only.

## Installed Listener Follow-up

The installed listener follow-up was executed on 2026-05-10 and is recorded in
`docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`.
The smoke verified:

- `http://127.0.0.1/` returns the Web Console static root.
- `http://127.0.0.1/pcv-config.js` points the browser to
  `http://127.0.0.1:7777`.
- `http://127.0.0.1/api/v1/runtime/policy` is rejected with
  `PCV_API_ROUTE_ON_WEB_PORT`.
- `http://127.0.0.1:7777/api/v1/runtime/policy` returns normal API responses
  with the protected bearer token.
- Final service `PathName` includes `--web-prefix "http://127.0.0.1:80/"`.

HTTPS/443 remains outside this slice. ADR-0005 TLS readiness currently records
code-level certificate generation/rotation/delete readiness and `tls_binding:
not-run`; no built-in HTTPS listener or public TLS binding is claimed here.
