# Network Download Update Source Gate Evidence - 2026-05-07

```text
evidence_id: network-download-update-source-gate-2026-05-07
```

## 요약

2026-05-07 후속 slice는 packaging/distribution future phase의 `network download updater` 중 source resolution gate만 code-level로 구현했다.

- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`는 기존 local `-SourceRoot` update 경로를 유지한다.
- 새 `-SourceUri`, `-ExpectedSha256`, `-DownloadRoot` 입력은 file/HTTPS ZIP package를 staging root로 복사 또는 다운로드하고, SHA-256을 검증한 뒤 extract된 payload root를 기존 update payload validation에 넘긴다.
- `update-source-preflight`는 service stop, product root backup, copy, rollback, health check보다 먼저 실행된다.
- `http://` source는 `PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED`로 차단된다.
- Active product root 아래의 download staging root는 `PCV_PRODUCT_UPDATE_DOWNLOAD_ROOT_ACTIVE_ROOT`로 차단된다.
- SHA-256 누락, invalid hash, mismatch, missing file, download failure, extract failure는 structured `PCV_PRODUCT_UPDATE_*` diagnostics로 반환한다.

## 범위

이 evidence는 network update source gate code-level evidence다. 다음 항목은 완료로 주장하지 않는다.

- external stable publication
- public trusted signing
- winget/MSIX/Burn bootstrapper publication
- remote update catalog/channel service
- installed destructive update/rollback smoke
- firewall/trust-store/LAN/MSI/service mutation
- full transactional rollback

실제 host mutation은 수행하지 않았다.

## 구현 계약

Product plan의 `update.source_resolution`은 다음 계약을 노출한다.

```text
mode: local-or-https-package-with-sha256
allowed_schemes: file, https
expected_sha256_required: true
extracts_before_service_stop: true
host_mutation_before_validation: false
```

`Update` plan에서 `-SourceUri`를 지정하면 `update_source`가 추가된다.

```text
enabled: true
resolution_stage: before-service-stop
mutates_host: false
```

## 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

결과:

```text
Tests Passed: 66
Failed: 0
```

검증된 behavior:

- plan에 network update source gate contract가 포함된다.
- `file://` ZIP package는 SHA-256 검증 후 `$DownloadRoot\payloads\<sha256>`로 extract된다.
- extract된 source root가 기존 `Test-PcvDesktopNodeUpdatePayload`와 `CopyAssets` 경로로 전달된다.
- `update-source-preflight`는 `service.stop`보다 먼저 실행된다.
- `http://` update source는 service stop 전에 `PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED`로 차단된다.
- Active product root 아래 download root는 service stop 전에 `PCV_PRODUCT_UPDATE_DOWNLOAD_ROOT_ACTIVE_ROOT`로 차단된다.

## 판정

`network download updater` 전체는 아직 future distribution phase다. 다만 downloaded/file package를 제품 update mutation에 넘기기 전 신뢰/무결성/source-root preflight를 수행하는 code-level source gate는 구현됐다.

이 evidence는 internal code-level packaging evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

후속 catalog/channel resolver code-level evidence는 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`에 별도로 기록한다.
