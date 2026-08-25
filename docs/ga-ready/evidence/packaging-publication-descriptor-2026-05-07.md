# Packaging Publication Descriptor Evidence - 2026-05-07

```text
evidence_id: packaging-publication-descriptor-2026-05-07
```

## 요약

2026-05-07 후속 slice는 installer `build.ps1`의 artifact contract에 내부 publication descriptor sidecar를 추가했다.

- Dry-run plan은 `publication_path`와 `publication` boundary object를 노출한다.
- 실제 MSI build output은 `PureCVisorDesktopNode-<version>-windows-x64.publication.json`을 provenance/hash sidecar와 같은 output root에 작성한다.
- Descriptor는 artifact base name, architecture, MSI path/SHA-256, provenance path, signing mode/trust model을 기계가 읽을 수 있게 연결한다.
- Descriptor의 publication boundary는 `internal-artifact-descriptor-only`이며 public trusted signing, external stable publication, Burn/MSIX/winget publication을 주장하지 않는다.

## 범위

이 evidence는 code-level installer artifact descriptor evidence다. 다음 항목은 완료로 주장하지 않는다.

- Burn bootstrapper build
- MSIX package build
- winget manifest generation/submission
- external publication service 또는 public stable channel publication
- public trusted signing
- installed destructive catalog update smoke
- service/MSI/firewall/trust-store/LAN/Event Log mutation

실제 host mutation은 수행하지 않았다.

## 구현 계약

Installer build output은 기존 산출물에 publication descriptor를 추가한다.

```text
MSI: PureCVisorDesktopNode-<version>-windows-x64.msi
Provenance: PureCVisorDesktopNode-<version>-windows-x64.provenance.json
MSI hash sidecar: PureCVisorDesktopNode-<version>-windows-x64.msi.sha256
Publication descriptor: PureCVisorDesktopNode-<version>-windows-x64.publication.json
```

Publication descriptor의 핵심 boundary field는 다음 값을 갖는다.

```text
schema_version: 1
mode: internal-artifact-descriptor-only
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
burn_bootstrapper: not-built
msix: not-built
winget_manifest: not-generated
network_download_updater: catalog-channel-code-level-partial
catalog_publication: not-published
```

Provenance에도 동일한 `publication` boundary object를 포함해 artifact hash와 publication claim을 함께 검토할 수 있게 한다.

## 검증

RED:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1' -Output Detailed"
```

결과:

```text
Expected 'publication_path' to be found in collection ...
Cannot bind argument to parameter 'Path' because it is null.
```

GREEN:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1' -Output Detailed"
```

결과:

```text
Tests Passed: 17
Failed: 0
```

검증된 behavior:

- dry-run plan이 publication descriptor path와 boundary object를 노출한다.
- fake WiX build는 `.publication.json` sidecar를 작성한다.
- descriptor의 MSI SHA-256은 provenance의 MSI SHA-256과 일치한다.
- descriptor와 provenance 모두 public trusted signing/external stable publication을 `not-claimed`로 기록한다.
- Burn bootstrapper, MSIX, winget manifest, catalog publication은 미실행 상태로 기록된다.

## 판정

Packaging/publication 후속 slice는 내부 artifact descriptor 수준으로 닫혔다. Burn bootstrapper, MSIX, winget manifest, external publication service, public trusted signing, 외부 stable publication은 계속 별도 future gate다.

이 evidence는 internal code-level packaging evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
