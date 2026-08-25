# Full Updater Catalog Channel Evidence - 2026-05-07

```text
evidence_id: full-updater-catalog-channel-2026-05-07
```

## 요약

2026-05-07 후속 slice는 packaging/distribution future phase의 updater catalog/channel resolver를 code-level로 구현했다.

- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`는 기존 local `-SourceRoot`와 file/HTTPS ZIP `-SourceUri` 경로를 유지한다.
- 새 `-UpdateCatalogUri`, `-UpdateChannel` 입력은 file/HTTPS JSON catalog에서 channel entry를 선택한다.
- Catalog resolver는 `schema_version: 1`, `product: PureCVisor Desktop Node`, channel `version`, `package_uri` 또는 `source_uri`, 64-hex `sha256` 또는 `expected_sha256`를 service stop 전에 검증한다.
- 선택된 catalog entry는 기존 `Resolve-PcvDesktopNodeUpdatePackage` source gate로 연결되어 package SHA-256 검증과 extract-before-service-stop preflight를 통과해야 한다.
- `update-catalog-preflight`는 `update-source-preflight`, service stop, product root backup, copy, rollback, health check보다 먼저 실행된다.
- Catalog resolution 결과는 update result와 `%ProgramData%\PureCVisor\desktop-node\update-transaction.json` journal의 `update_catalog`에 기록된다.
- Catalog의 `publication.public_trusted_signing`과 `publication.external_stable_publication`은 기본값 `not-claimed`이며, 이 slice는 public trusted signing 또는 외부 stable publication을 열지 않는다.

## 범위

이 evidence는 updater catalog/channel resolver code-level evidence다. 다음 항목은 완료로 주장하지 않는다.

- external stable publication service
- public trusted signing
- winget/MSIX/Burn bootstrapper publication
- automatic polling updater
- installed destructive catalog-channel update smoke
- firewall/trust-store/LAN/MSI/service mutation
- full transactional filesystem rollback

실제 host mutation은 수행하지 않았다.

## 구현 계약

Product plan의 `update.catalog` policy는 다음 계약을 노출한다.

```text
mode: file-or-https-json-channel-catalog
schema_version: 1
allowed_schemes: file, https
channel_required: true
package_sha256_required: true
resolves_before_service_stop: true
host_mutation_before_validation: false
publication_claim: internal-catalog-only
```

`Update` plan에서 `-UpdateCatalogUri`와 `-UpdateChannel`을 지정하면 `update_catalog`가 추가된다.

```text
enabled: true
resolution_stage: before-service-stop
mutates_host: false
publication.public_trusted_signing: not-claimed
publication.external_stable_publication: not-claimed
```

## 검증

RED:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

결과:

```text
Expected: 'PCV_PRODUCT_UPDATE_CATALOG_SCHEMA_UNSUPPORTED'
But was:  'PCV_PRODUCT_ACTION_FAILED'
```

GREEN:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

결과:

```text
Tests Passed: 70
Failed: 0
```

검증된 behavior:

- plan에 `update_catalog` catalog URI, channel, download root, publication `not-claimed`, before-service-stop resolution stage가 포함된다.
- catalog channel resolver는 service stop 전에 JSON catalog를 읽고 selected channel을 검증한다.
- selected channel의 package URI와 SHA-256은 기존 update source package gate로 전달된다.
- catalog entry version은 update target version과 transaction journal의 `update_catalog`에 반영된다.
- missing channel은 `PCV_PRODUCT_UPDATE_CATALOG_CHANNEL_NOT_FOUND`로 service stop 전에 차단된다.
- unsupported schema는 `PCV_PRODUCT_UPDATE_CATALOG_SCHEMA_UNSUPPORTED`로 service stop 전에 차단된다.
- direct `-SourceUri`와 `-UpdateCatalogUri` 동시 지정은 plan 단계에서 `PCV_PRODUCT_UPDATE_SOURCE_CONFLICT`로 차단된다.

## 판정

`network download updater` 전체는 아직 external publication/distribution future phase다. 다만 내부 file/HTTPS JSON catalog와 channel 선택을 통해 package URI/SHA-256 source gate로 연결하는 updater catalog resolver는 code-level로 구현됐다.

이 evidence는 internal code-level packaging evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
