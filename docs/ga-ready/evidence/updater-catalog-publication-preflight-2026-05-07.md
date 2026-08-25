# Updater Catalog Publication Preflight Evidence - 2026-05-07

evidence_id: updater-catalog-publication-preflight-2026-05-07
scope: updater-catalog-publication-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
catalog_publication: not-published

## 요약

이 slice는 full updater catalog/channel resolver를 public distribution 후보 matrix에 연결하는 plan-only publication preflight다. `New-PcvUpdaterCatalogPublicationPreflight.ps1`는 file/HTTPS catalog resolver evidence와 별도로, external static hosting에 올릴 수 있는 catalog publication preview와 SHA-256 sidecar를 artifact에 쓴다.

이 도구는 catalog publication preview만 만든다. 실제 upload, CDN/public endpoint validation, external stable publication, public trusted signing, signed public update/rollback, service/MSI/firewall/trust-store/LAN mutation은 실행하지 않는다. `catalog_publication: not-published`, `actual_execution: not-run`, `host_mutation_performed: false`를 machine-readable anchor로 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1 -CatalogPath '<catalog.json>' -Channel stable -PublicCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' -ArtifactRoot 'artifacts/updater-catalog-publication-preflight-20260507-dryrun' -PlanOnly
```

## Contract

```text
scope: updater-catalog-publication-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
catalog_publication: not-published
publication_checks:
  catalog-schema-v1
  selected-channel-present
  catalog-uri-https
  package-uri-https
  package-sha256-present
  public-claim-not-made
  publication-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1`는 `New-PcvUpdaterCatalogPublicationPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 updater catalog publication preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 8 tests.
- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1','archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed`
- Result: PASS, 29 tests.
- Dry-run artifact root: `artifacts/updater-catalog-publication-preflight-20260507-dryrun`
- Dry-run summary: `ok=true`, `scope=updater-catalog-publication-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `catalog_publication=not-published`, selected channel `stable`, preview SHA-256 `ef222145302846806565317b43ac8f5a311e516a58bb99020e38da515561ec73`.

이 GREEN은 catalog publication preview descriptor만 확인한다. External stable publication endpoint, public trusted signing, public signed update/rollback, host mutation은 수행하지 않았다.
