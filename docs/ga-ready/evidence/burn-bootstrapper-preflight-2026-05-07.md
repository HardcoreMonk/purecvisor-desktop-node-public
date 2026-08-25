# Burn Bootstrapper Preflight Evidence - 2026-05-07

evidence_id: burn-bootstrapper-preflight-2026-05-07
scope: burn-bootstrapper-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
burn_bootstrapper: not-built

## 요약

이 slice는 ADR-0005의 Burn bootstrapper gate를 실제 bundle build 전 plan-only preflight로 고정한다. `New-PcvBurnBootstrapperPreflight.ps1`는 packaging publication descriptor와 HTTPS MSI URL을 입력으로 받아 `summary.json`과 WiX Burn authoring preview를 생성한다.

이 도구는 Burn bundle을 빌드하지 않는다. Chained install/repair/remove smoke, public signed update/rollback smoke, public trusted signing, external stable publication, host mutation은 실행하지 않는다. `burn_bootstrapper: not-built`, `actual_execution: not-run`, `host_mutation_performed: false`를 machine-readable anchor로 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1 -PublicationDescriptorPath '<publication.json>' -ArtifactRoot 'artifacts/burn-bootstrapper-preflight-20260507-dryrun' -MsiUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi' -PlanOnly
```

## Contract

```text
scope: burn-bootstrapper-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
burn_bootstrapper: not-built
burn_checks:
  publication-descriptor-schema-v1
  msi-url-https
  msi-sha256-present
  bundle-upgrade-code-valid
  wix-burn-authoring-preview-written
  public-claim-not-made
  bundle-build-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1`는 `New-PcvBurnBootstrapperPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 Burn bootstrapper preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 7 tests.
- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1','archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed`
- Result: PASS, 29 tests.
- Dry-run artifact root: `artifacts/burn-bootstrapper-preflight-20260507-dryrun`
- Dry-run summary: `ok=true`, `scope=burn-bootstrapper-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `burn_bootstrapper=not-built`, bundle upgrade code `{8F455BB4-640E-47A2-A982-338C7A6318B5}`.

이 GREEN은 WiX Burn authoring preview descriptor만 확인한다. Bundle build, chained install/repair/remove smoke, public trusted signing, external stable publication, host mutation은 수행하지 않았다.
