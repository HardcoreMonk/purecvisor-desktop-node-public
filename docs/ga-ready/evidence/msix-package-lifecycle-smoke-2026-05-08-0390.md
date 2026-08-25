# MSIX Package Lifecycle Smoke Evidence - 2026-05-08 0.39.0

evidence_id: msix-package-lifecycle-smoke-2026-05-08-0390
scope: msix-package-build-install-update-remove-smoke
result: PASS
product_version_source: 0.39.0-admin-smoke installed listener payload
artifact_root: artifacts/msix-package-lifecycle-smoke-20260508-230452-0390
summary: artifacts/msix-package-lifecycle-smoke-20260508-230452-0390/summary.json
host_mutation_performed: true
public_trusted_signing: excluded
external_stable_publication: not-claimed
msix: build-install-update-remove-pass

이 evidence는 `0.39.0-admin-smoke` installed listener payload를 입력으로 별도 MSIX smoke package identity를 빌드하고, install/update/remove lifecycle을 실제로 실행한 관리자 opt-in host mutation 기록이다.

이 evidence는 ADR-0003 internal Root/leaf signing trust model 범위다. Public trusted signing 또는 외부 stable publication evidence가 아니다.

## Package Boundary

- Package identity: `PureCVisor.DesktopNode.MsixSmoke`
- Packaged service name: `PureCVisorDesktopNodeMsixSmoke`
- Publisher: `CN=PureCVisor Desktop Node Internal Code Signing`
- Signer thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- Internal root thumbprint: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
- Restricted capabilities: `runFullTrust`, `packagedServices`, `localSystemServices`
- Source payload: `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/payload`
- Source publication descriptor: `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/PureCVisorDesktopNode-0.39.0-admin-smoke-windows-x64.publication.json`

## Execution

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File artifacts/msix-package-lifecycle-smoke-20260508-230452-0390/msix-package-lifecycle-runner.ps1
```

The generated runner built two signed MSIX packages with the Windows SDK `makeappx.exe` and `signtool.exe`, installed version `0.39.0.0`, updated to version `0.39.1.0`, then removed the package.

## Observed Result

- v1 MSIX: `PureCVisorDesktopNode-MsixSmoke-0.39.0.0.msix`
- v1 SHA-256: `0f06de07027e7d7ce6502a7653de595ae89881c0696963dc3e975ef85c0cbf6a`
- v2 MSIX: `PureCVisorDesktopNode-MsixSmoke-0.39.1.0.msix`
- v2 SHA-256: `fc4166eb6e04c58b60c00c433e47b1687b6d298141d3fd540f01ef4c6f10da3e`
- `makeappx` package creation: PASS for both versions
- `signtool sign`: PASS for both versions
- `signtool verify /pa /v`: PASS for both versions, warnings `0`, errors `0`
- `Add-AppxPackage` install v1: PASS, package `PureCVisor.DesktopNode.MsixSmoke_0.39.0.0_x64__he18zmqd2ahap`
- Packaged service after install: present, `Stopped`
- `Add-AppxPackage` update v2: PASS, package `PureCVisor.DesktopNode.MsixSmoke_0.39.1.0_x64__he18zmqd2ahap`
- `Remove-AppxPackage` remove v2: PASS
- Final smoke package absent: `true`
- Final smoke service absent: `true`
- Existing MSI service `PureCVisorDesktopNode`: `Running`

## Boundary

This closes only the internal MSIX package build/install/update/remove smoke for the smoke identity. It does not replace the MSI-first product distribution decision, does not submit a Store/MSIX public package, does not claim public trusted signing, and does not claim external stable publication.
