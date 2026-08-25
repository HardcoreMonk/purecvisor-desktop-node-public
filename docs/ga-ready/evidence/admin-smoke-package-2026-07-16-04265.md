# Admin smoke package 2026-07-16 0.42.65

evidence_id: `admin-smoke-package-2026-07-16-04265`
result: `PACKAGE_BUILD_PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.65-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260716-04265`
provenance: `artifacts/admin-smoke-package-20260716-04265/PureCVisorDesktopNode-0.42.65-admin-smoke-windows-x64.provenance.json`
publication: `artifacts/admin-smoke-package-20260716-04265/PureCVisorDesktopNode-0.42.65-admin-smoke-windows-x64.publication.json`
msi: `artifacts/admin-smoke-package-20260716-04265/PureCVisorDesktopNode-0.42.65-admin-smoke-windows-x64.msi`
msi_sha256_sidecar: `artifacts/admin-smoke-package-20260716-04265/PureCVisorDesktopNode-0.42.65-admin-smoke-windows-x64.msi.sha256`
msi_sha256: `5709edb0d5f265393c8690c212dd6d1f61873f7cbbaa110b1654a2e380e6b748`
payload_file_count: `8`
payload_aggregate_sha256: `3b4fefb3c03c1a70ba804e959931bdec0ee36923139a84602e85be69e96e251a`
provenance_commit: `4855947fe0199cedc978e8b40ffb45e96ced6876`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Payload

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83` |
| PCVCLI EXE | `822999cd35a3a3addf073f962a5750c60cacd0ff598097778eb10782c25cd7e2` |

product_manifest_schema_version: `2`
desktop_node_host_present: `true`
pcvcli_present: `true`
pcvtui_present: `false`
product_manifest_root_tui_property_present: `false`
product_manifest_paths_tui_exe_present: `false`
msi_file_row_count: `8`
msi_active_tui_file_rows: `0`

이 package build는 ADR-0011의 CLI/Web-only active product payload를 생성했다. Payload와 MSI
File table에는 `DesktopNode.Host.exe`와 `pcvcli.exe`가 있고 active TUI file/reference는 없다.

동일 version의 operational full admin host mutation, 실제 VM functional correctness, installed
CLI/Web current-card 결과는 별도 evidence가 소유한다. 이 package는
`AllowUnsignedDev`/`LocalTest` internal admin-smoke 전용이며 public trusted signing 또는 외부
stable publication을 주장하지 않는다.
