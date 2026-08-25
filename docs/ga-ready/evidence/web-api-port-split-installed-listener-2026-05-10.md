# Web/API Port Split Installed Listener Evidence - 2026-05-10

evidence_id: web-api-port-split-installed-listener-2026-05-10-0392
scope: windows-desktop-node-installed-listener-web-api-port-split
artifact_root: artifacts/installed-port-split-20260510-010714-0392
payload_version: 0.39.2-port-split-smoke
web_console_url: http://127.0.0.1/
web_console_prefix: http://127.0.0.1:80/
api_prefix: http://127.0.0.1:7777/
api_route_prefix: http://127.0.0.1:7777/api/v1/...
web_api_same_port: false
installed_listener_execution: installed-listener-pass
host_mutation_performed: true
service_mutation: stop-copy-repair-installed-start
api_route_on_web_port_error_code: PCV_API_ROUTE_ON_WEB_PORT
https_443_binding: not-run
tls_binding: not-run
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

The installed `PureCVisorDesktopNode` service was updated from the previous
single-listener `0.39.1-admin-smoke` shape to the Web/API split listener shape.
The service now runs:

- Web Console static assets: `http://127.0.0.1/`
- Web API routes: `http://127.0.0.1:7777/api/v1/...`

The installed service `PathName` now includes `--web-prefix
"http://127.0.0.1:80/"`, `--prefix "http://127.0.0.1:7777/"`, the installed
`web` root, and `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`.
The earlier installed `PathName` did not include `--web-prefix` and still used a
Credential Manager target argument.

This was an admin host mutation against the installed service. It does not claim
HTTPS/443, public trusted signing, external stable publication, winget
submission, or clean-host public release readiness.

## Artifact Contents

- `artifacts/installed-port-split-20260510-010714-0392/prepare-summary.json`
- `artifacts/installed-port-split-20260510-010714-0392/service-stop.json`
- `artifacts/installed-port-split-20260510-010714-0392/service-repair-installed.json`
- `artifacts/installed-port-split-20260510-010714-0392/installed-port-split-smoke.json`
- `artifacts/installed-port-split-20260510-010714-0392/installed-root-before`
- `artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80/summary.json`

Payload hashes recorded in `prepare-summary.json` and confirmed after install:

- `DesktopNode.Host.exe` SHA-256:
  `c3c0ece28d5a9205da919ab134b58acbbc14b20e25009b04e79ce0bf4c76e4d3`
- `pcv.exe` SHA-256:
  `d86fc7a44e0dde5e11822872185e12cb2c05bcd02898ae3a4d71ba8aa1766528`
- `web/app.js` SHA-256:
  `76dc0e06625052f835827c576b5967eebd6b5d2d113a06479704d1191165dca6`
- `web/index.html` SHA-256:
  `2ea5a9684969f24ebb3ab721cf1ce0dada8658961ce30a9719c4a1d277c7c170`

## Verification

Installed listener smoke result:

```text
service stop exit: 0
service repair-installed exit: 0
final service: Running
before ports: 127.0.0.1:7777
after ports: 127.0.0.1:80, 127.0.0.1:7777
GET http://127.0.0.1/ -> 200 text/html
GET http://127.0.0.1/pcv-config.js -> 200, contains http://127.0.0.1:7777
GET http://127.0.0.1/api/v1/runtime/policy -> 404 PCV_API_ROUTE_ON_WEB_PORT
GET http://127.0.0.1:7777/api/v1/runtime/policy -> 200 with bearer protected token
OPTIONS http://127.0.0.1:7777/api/v1/runtime/policy -> 204 CORS for http://127.0.0.1
```

Final installed service `PathName`:

```text
"C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe" listen --prefix "http://127.0.0.1:7777/" --web-prefix "http://127.0.0.1:80/" --web-root "C:\Program Files\PureCVisor\DesktopNode\web" --job-store "C:\ProgramData\PureCVisor\desktop-node\jobs.json" --event-log "C:\ProgramData\PureCVisor\desktop-node\events.jsonl" --diagnostics-root "C:\ProgramData\PureCVisor\desktop-node\diagnostics" --api-token-protected-file "C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json" --route-timeout-seconds 30 --request-limit-per-minute 120 --request-burst-limit 20 --retry-after-seconds 15
```

## Browser QA Follow-up

Headless Chrome CDP QA was rerun against the new default Web Console URL
`http://127.0.0.1/`:

```text
node web/scripts/capture-installed-listener-qa.mjs --url=http://127.0.0.1/ --out=artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80
PASS
```

Summary:

- artifact root:
  `artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80`
- token supplied: `true`
- token value observed: `false`
- dashboard loaded: `true`
- VM filter/sort exercised: `true`
- VM select clicked: `false` because no selectable VM remained after cleanup
- jobs/network/troubleshooting views clicked: `true`
- diagnostic create/download clicked: `true`/`true`
- missing button labels: `0`
- unlabeled inputs: `0`

Screenshot hashes:

| Screenshot | Size | SHA-256 |
| --- | --- | --- |
| `dashboard-wide.png` | `2048x1152` | `509bb151d6794dd8bca2e073712f91f2f35ec875d347edb50d82ddca50f624ad` |
| `vm-detail.png` | `1366x900` | `43a68647ef374f51d2d242896bea65718c454f67d11de8be3b80a0c01cdee6e7` |
| `jobs.png` | `1366x900` | `00c707abebea7a6509c1fc7af9a087fe1bdd9871f0b524bea4e04c1b91ccd09d` |
| `network.png` | `1366x900` | `30ea60fc810435fe180ee5b6a36061ae1d57b2288e25eac0c6f3de052f7356ec` |
| `troubleshooting-diagnostics.png` | `1366x900` | `5259a93011735b74bec6c759138bfa79a4dbbc2b69fa85f7a930bd8acb1a02fe` |
| `dashboard-1366.png` | `1366x768` | `90ce548028a15105032e20a63ed940bad31ee164c085eaf563e495aea7978ab1` |
| `dashboard-tablet.png` | `900x900` | `c40062e4239acc97ce88ddfdfbc9ac4edee0b6be9dcc4163cba4c8a66bf9bb6d` |
| `dashboard-mobile.png` | `390x860` | `a454367b8eef62b6f3f15e439b6e3c9d9d2bde7365084c2f48568a27d762938a` |

## Boundary

This evidence closes the installed listener follow-up for the Web/API port split
only. The browser QA created and downloaded a diagnostic bundle through the
installed API; it did not perform host mutation. This evidence does not bind
HTTPS/443 and does not mutate firewall, trust-store, LAN, Event Log, Hyper-V,
MSI registration, winget, or public catalog publication state. It is internal
installed-service evidence and is not public trusted signing or external stable
publication evidence.
