# Winget CLI Validate Evidence - 2026-05-09

evidence_id: winget-cli-validate-2026-05-09
scope: winget-cli-validate
result: PASS
artifact_root: artifacts/winget-cli-validate-20260509-0391
actual_execution: winget-validate-executed
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
winget_validation_status: winget-cli-validate-pass
winget_submission: not-submitted

## Summary

`winget validate` was executed against the generated ADR-0005 singleton manifest preview at `artifacts/winget-cli-validate-20260509-0391/winget/PureCVisor.DesktopNode.yaml`.

The first real CLI attempt exposed a schema-header warning in the preview manifest. `New-PcvPublicDistributionReadiness.ps1` now writes the winget singleton schema header, and the rerun returned exit code `0`.

This evidence validates the manifest with the local Windows Package Manager CLI only. It does not submit to `microsoft/winget-pkgs`, upload an installer, claim public trusted signing, or claim external stable publication.

## Observed Result

- manifest: `PureCVisor.DesktopNode.yaml`
- package version: `0.39.1`
- installer type: `msi`
- installer URL: `https://downloads.example.invalid/PureCVisorDesktopNode-0.39.1-windows-x64.msi`
- installer SHA-256: `19b93e72f567e1d5598c7998da2385edde574732284c3ff82a1a5954857f915d`
- winget validate exit code: `0`
- `winget_submission`: `not-submitted`

## Verification

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1 -Output Detailed
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1 -Output Detailed
winget validate --manifest artifacts/winget-cli-validate-20260509-0391/winget/PureCVisor.DesktopNode.yaml --disable-interactivity
```
