# Admin smoke package 2026-07-14 0.42.63

evidence_id: `admin-smoke-package-2026-07-14-04263`
result: `PACKAGE_BUILD_PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.63-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260714-04263`
provenance: `artifacts/admin-smoke-package-20260714-04263/PureCVisorDesktopNode-0.42.63-admin-smoke-windows-x64.provenance.json`
publication: `artifacts/admin-smoke-package-20260714-04263/PureCVisorDesktopNode-0.42.63-admin-smoke-windows-x64.publication.json`
msi: `artifacts/admin-smoke-package-20260714-04263/PureCVisorDesktopNode-0.42.63-admin-smoke-windows-x64.msi`
msi_sha256_sidecar: `artifacts/admin-smoke-package-20260714-04263/PureCVisorDesktopNode-0.42.63-admin-smoke-windows-x64.msi.sha256`
msi_sha256: `d2f2fff7fb400647135d96449f36704af2d080e1a6a97a551354290cdf1a6f04`
payload_file_count: `8`
payload_aggregate_sha256: `19f80f3e0b849d180a3e62461742a8a2ab7371e632dbfecfc8fad28bf59721f4`
provenance_commit: `50bea2b36f912ad74d59a2234e4a2cbe7fc79f2b`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Payload

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `19f5867d4e22a89d4a988c832bcfc0f6bc4ceddee8a0ba3c18133249bbc1b3de` |
| PCVCLI EXE | `14afdc0ae97cc7c1b4abfb76ad42a4093d06c9170da37977cdc1e7b29d474566` |

product_manifest_schema_version: `2`
desktop_node_host_present: `true`
pcvcli_present: `true`
pcvtui_present: `false`
product_manifest_root_tui_property_present: `false`
product_manifest_paths_tui_exe_present: `false`
msi_active_tui_file_rows: `0`

이 package build는 ADR-0011의 CLI/Web-only active product payload를 생성했다. Payload와 MSI
File table에는 `DesktopNode.Host.exe`와 `pcvcli.exe`가 있고 active TUI file/reference는 없다.

이 evidence는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke package build만 PASS로
승격한다. `0.42.63-admin-smoke` full admin host mutation gate와 installed CLI/Web current-card는
아직 실행 또는 PASS로 주장하지 않는다. Public trusted signing 또는 외부 stable publication도
주장하지 않는다.
