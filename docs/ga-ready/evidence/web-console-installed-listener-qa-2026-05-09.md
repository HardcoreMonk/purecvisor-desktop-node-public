# Web Console Installed Listener QA Evidence - 2026-05-09

evidence_id: web-console-installed-listener-qa-2026-05-09
scope: windows-desktop-node-web-console-installed-listener
base_url: http://127.0.0.1:7777/
installed_payload_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-130105-0391-frontend-final2
browser_qa_artifact_root: artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b
host_mutation_performed_by_browser_qa: false
installed_payload_host_mutation_performed: true
token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
linux_runtime_excluded: true

## Summary

Installed listener browser QA was executed against `http://127.0.0.1:7777/`
using headless Chrome over CDP. The browser test drove the installed Web Console,
not the Node fixture.

The installed listener baseline was the final frontend payload rerun
`20260509-130105-0391-frontend-final2`, which followed the requested
`20260509-122028-0391-frontend` host mutation run after final Web Console
hardening.

## Installed Payload Baseline

- Batch artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-130105-0391-frontend-final2`
- Route parity artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-130105-0391-frontend-final2`
- OS mutation artifact: `artifacts/os-mutation-gates-batch-profile-20260509-130105-0391-frontend-final2`
- Batch result: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- MSI provenance commit: `38e31b3ca0b84cb0cdd417b75011100a4de8ad8b`
- MSI SHA-256: `46e0feb24bd4d12117027191c70c9d8ab329a707d3064a6252d801f56fbba201`
- Signing mode: `AllowUnsignedDev`
- Final service: `PureCVisorDesktopNode` `Running`
- Firewall final rule count: `0`
- Event Log final source present: `false`
- Boot time unchanged: `true`
- Public trusted signing: `excluded`
- External stable publication: `not-claimed`

## Browser QA Result

- Summary path: `artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b/summary.json`
- Engine: Chrome CDP headless
- Token supplied to browser session: `true`
- Token value observed or written to evidence: `false`
- Dashboard loaded: `true`
- VM filter/sort exercised: `true`
- VM select clicked: `false`
- Jobs view clicked: `true`
- Network view clicked: `true`
- Troubleshooting view clicked: `true`
- Diagnostic create clicked: `true`
- Diagnostic download clicked: `true`
- Missing button labels: `0`
- Unlabeled inputs/selects: `0`

The installed Hyper-V smoke cleanup left no selectable VM in the final inventory,
so browser QA did not create a VM only to click one. The run still exercised VM
filter/sort controls and verified that the detail panel remained stable in the
empty/no-selection state.

The later destructive lifecycle UI follow-up is tracked in
`docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md`.
That separate admin opt-in browser run used
`artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391` and drove
VM create/start/restart/poweroff/delete plus checkpoint create/restore/delete
through the installed Web Console controls.

## Screenshot Evidence

The run produced the following screenshot hashes under
`artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b`.
Token values were not written to the summary or screenshot evidence.

| Screen | Viewport | SHA-256 |
|--------|----------|---------|
| `dashboard-wide.png` | `2048x1152` | `509bb151d6794dd8bca2e073712f91f2f35ec875d347edb50d82ddca50f624ad` |
| `vm-detail.png` | `1366x900` | `43a68647ef374f51d2d242896bea65718c454f67d11de8be3b80a0c01cdee6e7` |
| `jobs.png` | `1366x900` | `18dc127c9528c81e9ac047a86c19aaafcd13157e20092d1c1d784770f7d2c984` |
| `network.png` | `1366x900` | `30ea60fc810435fe180ee5b6a36061ae1d57b2288e25eac0c6f3de052f7356ec` |
| `troubleshooting-diagnostics.png` | `1366x900` | `5259a93011735b74bec6c759138bfa79a4dbbc2b69fa85f7a930bd8acb1a02fe` |
| `dashboard-1366.png` | `1366x768` | `90ce548028a15105032e20a63ed940bad31ee164c085eaf563e495aea7978ab1` |
| `dashboard-tablet.png` | `900x900` | `1374b1623cc0bbae123f413b519a9e8e8ecb3cd359b1557a15764da49affb3cb` |
| `dashboard-mobile.png` | `390x860` | `0f1665ba7b87c8cbdca22f2ccd7e4cda4949473f99cc9d807de6f8da21968c75` |

## Frontend UX Coverage

Installed listener QA confirmed real navigation and actions for dashboard, VM
workbench, jobs, network, troubleshooting, diagnostic bundle create, and
diagnostic bundle download.

Static and browser-fixture checks cover the conditional states that cannot all be
forced from the healthy installed listener without mutating backend behavior:
diagnostic API unsupported/401/404/500/timeout, token clear refresh copy, retained
terminal jobs, active jobs, pagination boundary, native parity failure, no-switch
network inventory, structured problem-details fields, reduced motion, responsive
viewports, and no direct host mutation command strings.

## Verdict

The installed Web Console browser QA is PASS for the final frontend payload. This
evidence does not mutate host state by itself, does not record token values, and
does not claim public trusted signing or external stable publication.
