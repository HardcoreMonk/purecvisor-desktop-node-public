# Winget Manifest Compliance Preflight Evidence - 2026-05-08

evidence_id: winget-manifest-compliance-preflight-2026-05-08
scope: winget-manifest-compliance-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
winget_submission: not-submitted
validation_status: offline-compliance-pass

## 요약

이 slice는 readiness preflight가 생성한 winget singleton manifest preview를 offline compliance preflight로 한 단계 더 고정한다. `New-PcvWingetManifestCompliancePreflight.ps1`는 manifest YAML preview를 읽어 필수 package field, singleton manifest type/version, HTTPS installer URL, 64-hex SHA-256, MSI installer type을 검증하고 `summary.json`과 normalized manifest metadata를 쓴다.

이 도구는 winget CLI validation, winget-pkgs repository submission, public trusted signing, external stable publication, signed public update/rollback, service/MSI/firewall/trust-store/LAN mutation을 실행하지 않는다. `validation_status: offline-compliance-pass`, `winget_submission: not-submitted`, `actual_execution: not-run`, `host_mutation_performed: false`를 machine-readable anchor로 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1 -ManifestPath 'artifacts/public-distribution-readiness-preflight-20260507-dryrun/winget/PureCVisor.DesktopNode.yaml' -ArtifactRoot 'artifacts/winget-manifest-compliance-preflight-20260508-dryrun' -PlanOnly
```

## Contract

```text
scope: winget-manifest-compliance-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
winget_submission: not-submitted
validation_status: offline-compliance-pass
compliance_checks:
  manifest-file-present
  singleton-manifest-type
  manifest-version-supported
  package-identifier-present
  package-version-winget-compatible
  installer-url-https
  installer-sha256-valid
  installer-type-msi
  winget-cli-validation-not-executed
  winget-submission-not-executed
  public-claim-not-made
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1`는 `New-PcvWingetManifestCompliancePreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 winget manifest compliance preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 7 tests.
- Dry-run artifact root: `artifacts/winget-manifest-compliance-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=winget-manifest-compliance-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `winget_submission=not-submitted`, `validation_status=offline-compliance-pass`, normalized manifest path `winget-manifest.normalized.json`.

이 GREEN은 winget manifest preview의 offline compliance descriptor만 확인한다. Winget CLI validation, repository submission, public trusted signing, external stable publication, signed public update/rollback, host mutation은 수행하지 않았다.
