# Public Signed Update/Rollback Smoke Preflight Evidence - 2026-05-08

evidence_id: public-signed-update-rollback-smoke-preflight-2026-05-08
scope: public-signed-update-rollback-smoke-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication
clean_host_smoke_status: not-run

## 요약

이 slice는 ADR-0005의 public signed update/rollback smoke row를 clean-host smoke plan preview로 고정한다. `New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`는 updater catalog의 selected channel을 읽어 target package URI/SHA-256, baseline version, clean host profile, required evidence 목록을 `summary.json`과 plan preview에 기록한다.

이 도구는 실제 install/update/rollback, service/MSI/firewall/trust-store/LAN mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. Public trusted signing evidence와 external stable publication evidence가 import되기 전까지 `public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication`, `clean_host_smoke_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1 -CatalogPath '<catalog.json>' -Channel stable -ArtifactRoot 'artifacts/public-signed-update-rollback-smoke-preflight-20260508-dryrun' -BaselineVersion '0.38.8' -CleanHostProfile 'clean-windows-hyperv-public-smoke' -PlanOnly
```

## Contract

```text
scope: public-signed-update-rollback-smoke-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication
clean_host_smoke_status: not-run
preflight_checks:
  catalog-schema-v1
  selected-channel-present
  package-uri-https
  package-sha256-present
  baseline-version-present
  clean-host-profile-recorded
  public-trusted-signing-required
  external-stable-publication-required
  signed-update-rollback-smoke-not-executed
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1`는 `New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 public signed update/rollback smoke preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 7 tests.
- Dry-run artifact root: `artifacts/public-signed-update-rollback-smoke-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=public-signed-update-rollback-smoke-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `public_signed_update_rollback_smoke=blocked-by-public-signing-and-publication`, `clean_host_smoke_status=not-run`.

이 GREEN은 clean-host smoke plan preview와 blocker descriptor만 확인한다. Public signed install/update/rollback, clean-host mutation, public trusted signing, external stable publication은 수행하지 않았다.
