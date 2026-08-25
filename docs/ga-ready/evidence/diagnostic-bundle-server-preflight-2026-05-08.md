# Diagnostic Bundle Server Preflight Evidence - 2026-05-08

evidence_id: diagnostic-bundle-server-preflight-2026-05-08
scope: diagnostic-bundle-server-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight
diagnostic_bundle_api_action: not-run
diagnostic_bundle_archive_created: false
diagnostic_bundle_download_served: false
diagnostic_bundle_redaction_status: not-run
diagnostic_bundle_authz_status: not-run
diagnostic_bundle_retention_status: not-run

## 요약

이 slice는 ADR-0005의 diagnostic bundle server-side generation/download row를 실제 Local API diagnostic bundle action 전 plan-only preflight로 고정한다. `New-PcvDiagnosticBundleServerPreflight.ps1`는 서비스명, diagnostics root, Local API generation route, download route template, bearer authorization policy, redaction policy, retention policy, diagnostic check 목록을 `summary.json`과 Diagnostic bundle server-side plan preview에 기록한다.

이 도구는 Local API action 실행, server-side bundle archive 생성, download serving, redaction execution, retention application, product diagnostics runner delegation, service/MSI/firewall/trust-store/LAN/update mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. 실제 server-side generation/download implementation과 authz/redaction/retention evidence가 닫히기 전까지 `diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight`, `diagnostic_bundle_api_action: not-run`, `diagnostic_bundle_archive_created: false`, `diagnostic_bundle_download_served: false`, `diagnostic_bundle_redaction_status: not-run`, `diagnostic_bundle_authz_status: not-run`, `diagnostic_bundle_retention_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1 -ArtifactRoot 'artifacts/diagnostic-bundle-server-preflight-20260508-dryrun' -ServiceName 'PureCVisorDesktopNode' -DiagnosticsRoot '%ProgramData%\PureCVisor\desktop-node\diagnostics' -ApiRoute '/api/v1/diagnostics/bundles' -DownloadRouteTemplate '/api/v1/diagnostics/bundles/{bundle_id}/download' -RetentionDays 14 -MaxBundleCount 50 -PlanOnly
```

## Contract

```text
scope: diagnostic-bundle-server-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight
diagnostic_bundle_api_action: not-run
diagnostic_bundle_archive_created: false
diagnostic_bundle_download_served: false
diagnostic_bundle_redaction_status: not-run
diagnostic_bundle_authz_status: not-run
diagnostic_bundle_retention_status: not-run
diagnostic_checks:
  service-name-present
  diagnostics-root-recorded
  api-route-recorded
  download-route-recorded
  authz-policy-recorded
  archive-creation-not-executed
  download-serving-not-executed
  redaction-not-executed
  retention-not-executed
  wrapper-execution-not-delegated
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1`는 `New-PcvDiagnosticBundleServerPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 diagnostic bundle server-side preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- Dry-run artifact root: `artifacts/diagnostic-bundle-server-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=diagnostic-bundle-server-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `diagnostic_bundle_server_generation=blocked-by-no-mutation-preflight`, `diagnostic_bundle_api_action=not-run`, `diagnostic_bundle_archive_created=false`, `diagnostic_bundle_download_served=false`, `diagnostic_bundle_redaction_status=not-run`, `diagnostic_bundle_authz_status=not-run`, `diagnostic_bundle_retention_status=not-run`.

이 GREEN은 server-side diagnostic bundle plan preview와 blocker descriptor만 확인한다. Local API action execution, archive creation, download serving, redaction execution, retention application, product diagnostics runner delegation, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
