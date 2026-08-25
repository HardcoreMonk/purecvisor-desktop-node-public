# Installed operator surface current-card 2026-05-29 0.42.58

evidence_id: `installed-operator-surface-current-card-2026-05-29-04258`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04258-fullgate-manual-admin`
version: `0.42.58-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260529-04258`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260529-04258/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260529-04258`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260529-04257-04258-closed`
clean_package_msi_sha256: `6ae889eeb1b7134fab9618941748528f6260727abbc8ff36eee301b59dff6c0b`
operational_fullgate_msi_sha256: `7e0aef503b3f56eb116d5931c9560a3dcd2c4ba347f1eb24e4b505b28e6c2845`
host_mutation_performed: `false-smoke-after-fullgate-and-manual-admin`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

설치본 `pcvcli`, `pcvtui`, Web Console `http://127.0.0.1/`를 같은 current-card 기준으로
재확인했다. CLI `ops summary`, `host status`, `vm list`, TUI `--smoke-once runtime`,
`--smoke-once job`, Web `/`, `/pcv-config.js`, `/app.js`가 PASS이고 token/password 노출은
`false/false`다.

| 표면 | 결과 | 근거 |
| --- | --- | --- |
| CLI ops summary | `PASS` | `artifacts/installed-operator-surface-current-card-20260529-04258/pcvcli-ops-summary-json.stdout.txt` |
| CLI table current-card | `PASS` | `current.public_boundary_main_push`, `current.public_boundary_head_sha` 표시 |
| TUI runtime current-card | `PASS` | `PUBLIC BOUNDARY CURRENT`, `current-card=ops-summary` 표시 |
| Web Console | `PASS` | `/`, `/pcv-config.js`, `/app.js` 모두 HTTP `200` |

이 evidence는 설치본 operator surface smoke이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
