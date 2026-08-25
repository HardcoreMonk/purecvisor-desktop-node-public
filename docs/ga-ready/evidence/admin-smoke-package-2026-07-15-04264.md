# Admin smoke package 2026-07-15 0.42.64

evidence_id: `admin-smoke-package-2026-07-15-04264`
result: `PACKAGE_BUILD_PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.64-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260715-04264`
provenance: `artifacts/admin-smoke-package-20260715-04264/PureCVisorDesktopNode-0.42.64-admin-smoke-windows-x64.provenance.json`
publication: `artifacts/admin-smoke-package-20260715-04264/PureCVisorDesktopNode-0.42.64-admin-smoke-windows-x64.publication.json`
msi: `artifacts/admin-smoke-package-20260715-04264/PureCVisorDesktopNode-0.42.64-admin-smoke-windows-x64.msi`
msi_sha256_sidecar: `artifacts/admin-smoke-package-20260715-04264/PureCVisorDesktopNode-0.42.64-admin-smoke-windows-x64.msi.sha256`
msi_sha256: `8ba9714995d153e97a84c90afcf01b3ab1a612a166089e764b7046aae46c1cb7`
payload_file_count: `8`
payload_aggregate_sha256: `d3070394a44d09d34b78a3c06b4e7f99a5bc266ba91306ae41dd1bacf611487f`
provenance_commit: `a0491e39992093b9ad506619cfacb1675939d6a3`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Payload

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `e746c4edc99b34e9939df347e60dd61c6a9f6bf44adfddd3dbf32bdba6b1091d` |
| PCVCLI EXE | `85cae98017d7ffd644d0baf2719a62193a78d9b3c04b462a8798ce0e642461db` |

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
