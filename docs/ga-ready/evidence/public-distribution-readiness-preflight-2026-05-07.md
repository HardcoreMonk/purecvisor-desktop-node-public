# Public Distribution Readiness Preflight Evidence - 2026-05-07

evidence_id: public-distribution-readiness-preflight-2026-05-07
scope: public-distribution-readiness-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
winget_submission: not-submitted

## 요약

이 slice는 packaging publication descriptor를 입력으로 받아 public distribution readiness summary와 winget manifest preview를 생성하는 dry-run preflight다. `New-PcvPublicDistributionReadiness.ps1`는 `summary.json`과 `winget/PureCVisor.DesktopNode.yaml` preview를 만들지만, `winget validate` 실행, repository submission, public trusted signing, external stable publication, signed public update/rollback, host mutation은 수행하지 않는다.

Microsoft Learn 기준으로 winget manifest는 required package fields와 installer URL/SHA-256을 포함해야 하고, submission 전 `winget validate` 검증과 Windows Package Manager repository PR 흐름을 따른다. MSIX service packaging은 별도 OS/support design evidence가 필요하므로 이번 preflight에서는 feasibility blocker로만 남긴다. Public MSI/EXE distribution은 trusted-root chain Authenticode signing 입력이 필요하므로 public trusted signing은 계속 `not-claimed`다.

## Sources

- Microsoft Learn: [Create your package manifest](https://learn.microsoft.com/en-us/windows/package-manager/package/manifest)
- Microsoft Learn: [Submit your manifest to the repository](https://learn.microsoft.com/en-us/windows/package-manager/package/repository)
- Microsoft Learn: [Plan for your deployment - MSIX](https://learn.microsoft.com/en-us/windows/msix/desktop/managing-your-msix-deployment-targetdevices)
- Microsoft Learn: [Code signing options for Windows app developers](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options)

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1 -PublicationDescriptorPath '<publication.json>' -ArtifactRoot 'artifacts/public-distribution-readiness-preflight-20260507-dryrun' -InstallerUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi' -InstallerSha256 '<64-hex-sha256>' -SigningProvider AzureArtifactSigning -ReleaseApproval 'approved-for-dry-run-readiness-only' -PlanOnly
```

## Contract

```text
scope: public-distribution-readiness-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
winget manifest preview: generated
winget validate: manual follow-up
winget_submission: not-submitted
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1`는 `New-PcvPublicDistributionReadiness.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 readiness evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1','archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed`
- Result: PASS, 26 tests.
- Dry-run artifact root: `artifacts/public-distribution-readiness-preflight-20260507-dryrun`
- Dry-run summary: `ok=true`, `scope=public-distribution-readiness-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `winget_submission=not-submitted`.

이 GREEN은 winget manifest preview와 readiness descriptor만 확인한다. `winget validate` 실행, Microsoft winget-pkgs PR, public trusted signing, external stable publication, signed public update/rollback, host mutation은 수행하지 않았다.
