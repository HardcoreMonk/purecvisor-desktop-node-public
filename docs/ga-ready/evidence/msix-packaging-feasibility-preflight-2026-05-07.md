# MSIX Packaging Feasibility Preflight Evidence - 2026-05-07

evidence_id: msix-packaging-feasibility-preflight-2026-05-07
scope: msix-packaging-feasibility-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
msix: feasibility-blocked-by-service-packaging-design

## 요약

이 slice는 ADR-0005의 MSIX row를 package build가 아니라 feasibility blocker evidence로 고정한다. `New-PcvMsixPackagingFeasibilityPreflight.ps1`는 packaging publication descriptor를 입력으로 받아 `summary.json`과 MSIX package manifest preview를 생성한다.

이 도구는 MSIX package를 빌드하지 않는다. Desktop Node는 Windows service 설치/시작/중지/삭제와 MSI lifecycle을 현재 제품 배포 표면으로 사용하므로, MSIX pass claim 전에는 service packaging design, AppxManifest capability boundary, install/update/remove evidence, public signing decision이 별도로 필요하다. `msix: feasibility-blocked-by-service-packaging-design`, `actual_execution: not-run`, `host_mutation_performed: false`를 machine-readable anchor로 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1 -PublicationDescriptorPath '<publication.json>' -ArtifactRoot 'artifacts/msix-packaging-feasibility-preflight-20260507-dryrun' -PlanOnly
```

## Contract

```text
scope: msix-packaging-feasibility-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
msix: feasibility-blocked-by-service-packaging-design
msix_checks:
  publication-descriptor-schema-v1
  package-identity-preview-written
  service-packaging-design-required
  install-update-remove-evidence-required
  capability-boundary-required
  public-claim-not-made
  msix-build-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1`는 `New-PcvMsixPackagingFeasibilityPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 MSIX packaging feasibility preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1','archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed`
- Result: PASS, 29 tests.
- Dry-run artifact root: `artifacts/msix-packaging-feasibility-preflight-20260507-dryrun`
- Dry-run summary: `ok=true`, `scope=msix-packaging-feasibility-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `msix=feasibility-blocked-by-service-packaging-design`, package identity `PureCVisor.DesktopNode`, MSIX version `0.39.0.0`.

이 GREEN은 MSIX package manifest preview와 blocked feasibility descriptor만 확인한다. MSIX build, package install/update/remove, public trusted signing, external stable publication, host mutation은 수행하지 않았다.
