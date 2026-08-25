# Built-in TLS Certificate Lifecycle Preflight Evidence - 2026-05-08

evidence_id: builtin-tls-certificate-lifecycle-preflight-2026-05-08
scope: builtin-tls-certificate-lifecycle-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
tls_certificate_lifecycle: blocked-by-no-mutation-preflight
tls_certificate_mutation: not-run
private_key_material_created: false
trust_store_mutation: not-run
lan_binding_mutation: not-run

## 요약

이 slice는 ADR-0005의 built-in TLS certificate lifecycle row를 실제 certificate/private key/trust-store/LAN binding mutation 전 plan-only preflight로 고정한다. `New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`는 서비스명, certificate subject, HTTPS bind prefix, 현재 TLS mode, 목표 built-in service certificate mode, lifecycle check 목록을 `summary.json`과 Built-in TLS certificate lifecycle plan preview에 기록한다.

이 도구는 private key material 생성, certificate import/export, trust-store mutation, HTTPS/LAN listener binding, service/MSI/firewall/update mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. 실제 certificate generation/import/rotation/removal, private key storage policy, trust boundary, LAN binding evidence가 닫히기 전까지 `tls_certificate_lifecycle: blocked-by-no-mutation-preflight`, `tls_certificate_mutation: not-run`, `private_key_material_created: false`, `trust_store_mutation: not-run`, `lan_binding_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1 -ArtifactRoot 'artifacts/builtin-tls-certificate-lifecycle-preflight-20260508-dryrun' -ServiceName 'PureCVisorDesktopNode' -CertificateSubject 'CN=PureCVisor Desktop Node Local API' -HttpsBindPrefix 'https://127.0.0.1:7443/' -CurrentTlsMode 'external-terminator-or-none' -PlanOnly
```

## Contract

```text
scope: builtin-tls-certificate-lifecycle-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
tls_certificate_lifecycle: blocked-by-no-mutation-preflight
tls_certificate_mutation: not-run
private_key_material_created: false
trust_store_mutation: not-run
lan_binding_mutation: not-run
lifecycle_checks:
  service-name-present
  certificate-subject-present
  https-bind-prefix-recorded
  current-tls-mode-recorded
  target-tls-mode-recorded
  private-key-not-created
  certificate-import-not-executed
  trust-store-mutation-not-executed
  lan-binding-not-executed
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1`는 `New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 built-in TLS certificate lifecycle preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- Dry-run artifact root: `artifacts/builtin-tls-certificate-lifecycle-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=builtin-tls-certificate-lifecycle-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `tls_certificate_lifecycle=blocked-by-no-mutation-preflight`, `tls_certificate_mutation=not-run`, `private_key_material_created=false`, `trust_store_mutation=not-run`, `lan_binding_mutation=not-run`.

이 GREEN은 TLS lifecycle plan preview와 blocker descriptor만 확인한다. Private key material 생성, certificate import/export, trust-store mutation, HTTPS/LAN binding, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
