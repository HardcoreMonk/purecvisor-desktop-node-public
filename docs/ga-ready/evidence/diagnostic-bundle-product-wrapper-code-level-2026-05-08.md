# Diagnostic Bundle Product Wrapper Code-Level Evidence - 2026-05-08

evidence_id: diagnostic-bundle-product-wrapper-code-level-2026-05-08
actual_execution: code-level-product-wrapper-test
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
diagnostic_bundle_server_generation: partial-code-level-api-action
diagnostic_bundle_host_listener_execution: code-level-host-listener
diagnostic_bundle_installed_listener_execution: not-run
diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator
diagnostic_bundle_request_id_propagation: code-level-host-header
installed_listener_blocker: closed-by-0.39.0-installed-listener-rerun

## Summary

이 evidence는 ADR-0005 diagnostic bundle 후속 작업 중 product wrapper `CollectDiagnostics` delegation을 code-level로 닫는다. `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`가 `New-PcvDesktopNodeDiagnosticBundle`로 위임되고, bundle directory 안에 `product-wrapper-delegation-redacted.json`을 기록한다. 반환 result와 artifact는 `actual_execution: code-level-product-wrapper`, `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `host_mutation_performed: false`, `public_trusted_signing: not-claimed`, `external_stable_publication: not-claimed`를 함께 보존한다.

이 product wrapper slice 당시 shell은 `IsElevated=false`였기 때문에 installed Windows service listener, MSI/service mutation, firewall/trust-store/LAN mutation은 실행하지 않았다. 이후 0.38.9 artifact 점검에서 installed SCM `PathName`의 native service-action config gap이 확인되어 `diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`가 후속 blocker owner가 됐다. 그 blocker는 `0.39.0-admin-smoke` elevated MSI/service rerun의 installed listener create/download PASS로 닫혔다. 이 문서 자체는 code-level product wrapper evidence이며 public trusted signing 또는 external stable publication evidence가 아니다.

## Verification

- RED: `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed -FullNameFilter 'PcvDesktopNodeProduct diagnostics.runs CollectDiagnostics through the product action orchestrator'`
  - 최초 실패: `actual_execution`이 `$null`이었다.
- GREEN: 같은 focused Pester test PASS.

## Evidence Files

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`
- `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`

## Exclusions

- Installed service listener execution: 별도 `0.39.0-admin-smoke` evidence가 소유한다.
- Host mutation/MSI/service/firewall/trust-store/LAN mutation: not-run.
- Public trusted signing and external stable publication: not-claimed.
