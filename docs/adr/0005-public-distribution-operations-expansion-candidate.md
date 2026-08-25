# ADR-0005: Public Distribution 및 Operations 확장 후보

상태: 미채택/종료

```text
DESKTOP_NODE_PUBLIC_DISTRIBUTION_DECISION_CANDIDATE: closed-not-adopted
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only
```

## 맥락

Desktop Node는 ADR-0004 기준 내부 전용 서비스 범위의 GA-ready product runtime이다. 현재 완료 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal `RequireSigned` trust model까지를 다룬다. public trusted signing, external stable publication, 일반 사용자 public release는 아직 적용 결정이 아니다.

이 ADR 후보는 제품 범위를 public distribution/운영 확장으로 넓힐 때 필요한 gate를 한 곳에 묶었던 역사 기록이다. 2026-05-10 결정으로 Desktop Node 제품 범위는 내부 사설망 전용으로 고정됐고, public distribution 후보는 적용하지 않는다. 현재 적용 decision은 `docs/adr/0006-internal-private-network-distribution.md`와 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`가 소유한다.

## 종료 결정 (2026-05-10)

ADR-0005는 미채택/종료한다. Public trusted signing provider/cert chain/timestamp evidence, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed install/update/rollback smoke는 내부 사설망 전용 제품에서 `out-of-scope`다.

`docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`와 관련 public preflight/evidence는 보존용 역사 기록으로 남긴다. 현재 release blocker와 운영 gate는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`의 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke로 재분류한다.

첫 PR 범위였던 `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`와 `packaging/windows-desktop-node/tools/New-PcvPublicDistributionDescriptor.ps1` dry-run descriptor는 public candidate history로 유지한다. 실제 host mutation, publication, signed public update/rollback 실행은 수행하지 않는다.

## P2 Evidence 재정리 (2026-05-12)

ADR-0005의 evidence는 public distribution을 채택하기 위한 실행 증거가 아니라,
public distribution을 채택하지 않기로 한 후보/차단 이력이다. 아래 표가 현재 해석
기준이다. 이후의 상세 후속 기록은 이 기준을 보강하는 history로 읽는다.

| Evidence 범주 | 현재 판정 | 해석 |
| --- | --- | --- |
| Public trusted signing, timestamp | `out-of-scope` / `not-claimed` | public CA chain, trusted timestamp, public signing material evidence가 없다. Internal Root 또는 `AllowUnsignedDev` admin-smoke는 public signing evidence가 아니다. |
| External stable publication/catalog upload | `out-of-scope` / `not-claimed` | external package endpoint, upload credential, public stable installer URL evidence가 없다. Internal artifact descriptor와 private catalog/channel은 ADR-0006 범위다. |
| Winget validation/submission | validation history only / `not-submitted` | `winget validate` 통과 기록은 manifest shape 확인일 뿐 repository submission이나 public availability를 의미하지 않는다. |
| Public signed update/rollback smoke | `blocked-by-public-signing-and-publication` | public signed package와 external publication이 없으므로 clean-host public update/rollback execution은 수행하지 않았다. Internal clean-host smoke는 public smoke를 대체하지 않는다. |
| Burn/MSIX package lifecycle | internal smoke only | internal Root/leaf 또는 `AllowUnsignedDev` lifecycle smoke이며 public signing/public store publication evidence가 아니다. |
| Credential Manager, Event Log, TLS, service token | internal installed/admin-smoke PASS | 운영 hardening evidence로 재사용 가능하지만 public distribution claim을 승격하지 않는다. |
| Diagnostic bundle, timeout/rate-limit, Web/API/TUI surfaces | code-level 또는 installed internal evidence | operator surface와 runtime readiness evidence이며 external stable publication evidence가 아니다. |

따라서 ADR-0005에서 재사용 가능한 것은 public release claim이 아니라, public release를
막는 조건과 내부 운영 evidence의 경계 문구다. 현재 적용되는 배포 결정, release
blocker, manual-admin campaign 입력은 ADR-0006과
`docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`가 소유한다.

`0.42.3-admin-smoke` package descriptor와 full admin host mutation campaign은
internal/private admin-smoke evidence로만 해석한다. 이 새 evidence가 PASS하더라도
public trusted signing, external stable publication, public update channel, winget
submission 상태는 ADR-0005에서 계속 `not-claimed`/`out-of-scope`다.

2026-05-16 `0.42.23-admin-smoke` package build와 `0.42.22-admin-smoke ->
0.42.23-admin-smoke` manual-admin package-pair PASS도 같은 원칙을 따른다.
`docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04223.md`와
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`는 internal
admin-smoke evidence다. MSI SHA-256은
`2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, provenance
commit은 `676b4177b10dc80209969066857bab6008ff2473`이다. Public trusted signing,
external stable publication, winget submission, public stable installer URL, public
signed clean-host smoke를 주장하지 않는다.

2026-05-16 `0.42.22-admin-smoke` package build, full admin host mutation, installed
operator current-card, post-merge public-boundary CI PASS도 같은 원칙을 따른다.
`docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md`,
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass.md`는
internal admin-smoke와 CI guard evidence이며 public trusted signing, external stable
publication, winget submission, public stable installer URL, public signed clean-host
smoke를 주장하지 않는다. Clean package MSI SHA-256은
`68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`, full-gate MSI
SHA-256은 `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`,
provenance commit은 `8a38995cc25a888f64473e9a2869740949ad6b24`다.

2026-05-16 `0.42.23-admin-smoke` full admin host mutation과 installed operator
current-card, post-merge public-boundary CI PASS도 같은 원칙을 따른다.
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md`,
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md`는
internal admin-smoke와 CI guard evidence이며 public trusted signing, external stable
publication, winget submission, public stable installer URL, public signed clean-host
smoke를 주장하지 않는다. Full-gate MSI SHA-256은
`ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932`이고 closed package
MSI SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`이다.
Full-gate provenance commit은 `d11a096086326004f27facd9612c2296ded15a4b`이고 closed
package provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`이다.

2026-05-16 `0.42.24-admin-smoke` package build, full admin host mutation, Runtime/API
`current_evidence` rollup, installed Web/TUI/CLI current-card smoke도 같은 원칙을
따른다. `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`는
internal admin-smoke evidence이며 public trusted signing, external stable publication,
winget submission, public stable installer URL, public signed clean-host smoke를 주장하지
않는다. ADR 변경 없이는 이 evidence 계열은 ADR-0006
`internal-private-network-only` 범위에 남고 public distribution evidence는 계속
`out-of-scope`다.

후속 `public-distribution-readiness-preflight`는 `packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1`다. 이 도구는 packaging publication descriptor에서 winget singleton schema header가 포함된 manifest preview와 `winget validate` command hint를 만들지만, public trusted signing, external stable publication, repository submission은 계속 `not-claimed`/`not-submitted`로 남긴다. `host_mutation_performed: false`를 유지한다.

후속 `winget-manifest-compliance-preflight`는 `packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1`다. 이 도구는 readiness preflight가 만든 singleton manifest preview를 읽어 required package fields, HTTPS installer URL, SHA-256, MSI installer type을 offline compliance로 검증하지만, winget CLI validation, repository submission, public trusted signing, external stable publication은 실행하지 않는다. 후속 실제 CLI evidence `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`는 `winget validate --manifest` exit code `0`으로 `winget_validation_status: winget-cli-validate-pass`를 기록했다. `winget_submission: not-submitted`, public trusted signing/external stable publication `not-claimed`는 유지한다.

후속 `public-external-gates-blocked`는 `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`다. 이 scan은 local SignTool x64, winget CLI, GitHub CLI auth를 확인했지만 public signing material, timestamp URL, external catalog/package upload endpoint and credentials, public stable installer URL, public clean-host publication input이 없어 timestamp evidence, external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback을 blocked로 기록한다. 이 evidence는 public release execution이 아니며 현재 internal-only-service 경계를 바꾸지 않는다. ADR-0006 internal clean-host install/update/rollback PASS는 public clean-host release gate를 대체하지 않는다.

후속 `public-ops-final-followup-attempt`는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`와 `packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1`다. 이 descriptor는 1-7 final public operations follow-up prerequisite scan을 `artifacts/public-ops-final-followup-attempt-20260509-0391`에 기록하고, `remaining_follow_up_count: 7`, `actual_execution: local-final-followup-prerequisite-scan-executed`, `host_mutation_performed: false`, `public_release: not-claimed`를 유지한다. Public trusted signing/external stable publication은 계속 주장하지 않는다.

후속 `public-ops-gate-execution-readiness`는 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`와 `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`다. 이 descriptor는 external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback, Windows Credential Manager service default transition, built-in TLS certificate lifecycle, Windows Event Log provider hardening의 실행 가능 상태를 `artifacts/public-ops-gate-execution-readiness-20260509-0392`에 기록한다. 외부 release 입력은 blocked로 유지하고 TLS는 `partial-code-level-cert-generate-rotate-delete-pass`, `tls_private_key_material_written=false`, `tls_binding=not-run`, `host_mutation_performed=false`로 code-level readiness를 기록한다. Public trusted signing/external stable publication은 계속 주장하지 않는다.

후속 `public-ops-installed-hardening-code-level`는 `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`다. 이 slice는 native `DesktopNode.Host.exe service-action credential-manager-system-proof`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard` path를 추가해 Credential Manager SYSTEM proof runner와 Event Log repair/write/volume guard code-level readiness를 기록한다. 이후 `windows-credential-manager-default-transition-installed-2026-05-10-0395`는 installed MSI deferred `LocalSystem` custom action으로 service default token-source migration, reload, old source rejection, rollback diagnostics를 PASS로 닫았다. `windows-event-log-default-transition-installed-2026-05-10-0396`는 installed MSI deferred `eventlog-default-transition`으로 default writer, provider repair/remove/restore, schema v1 event write, volume guard를 PASS로 닫았다. Internal HTTPS binding/trust boundary는 `internal-https-tls-lifecycle-installed-2026-05-10-0397`에서 PASS로 닫혔고, public trusted signing과 external stable publication은 out-of-scope/not-claimed 상태다.

후속 `updater-catalog-publication-preflight`는 `packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1`다. 이 도구는 updater catalog schema v1과 selected HTTPS channel을 읽어 catalog publication preview와 SHA-256 sidecar를 만들지만, external catalog upload, public endpoint validation, public trusted signing, external stable publication은 실행하지 않는다. `catalog_publication: not-published`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

후속 `public-signed-update-rollback-smoke-preflight`는 `packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`다. 이 도구는 selected catalog channel에서 clean-host smoke plan preview를 만들지만, install/update/rollback execution, public trusted signing, external stable publication은 실행하지 않는다. Public trusted signing과 external stable publication evidence가 import되기 전까지 `public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication`, `clean_host_smoke_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

후속 `windows-credential-manager-transition-preflight`는 `packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1`다. 이 도구는 서비스명, credential target, 현재 DPAPI LocalMachine protected token file storage, 목표 Windows Credential Manager storage, transition plan preview를 기록하지만 token value read, credential write/delete, service reload, host mutation은 실행하지 않는다. 후속 `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`는 current-user Advapi32 `CredWriteW`/`CredReadW`/`CredDeleteW` capability smoke PASS와 당시 installed service `LocalSystem` blocker를 기록한다. `public-ops-installed-hardening-code-level-2026-05-09-0393`는 native SYSTEM proof runner를 추가했고, 최신 `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`는 `credential_manager_system_context_proof: installed-local-system-proof-pass`, `service_credential_manager_default_transition: installed-admin-smoke-pass`, `token_source_migration: protected-file-to-credential-manager`, `service_reload_status: restarted`, `old_source_rejection_status: protected-file-source-rejected-after-reload`, `rollback_diagnostics_status: written`, `token_value_observed: false`를 기록한다.

후속 `windows-event-log-provider-transition-preflight`는 `packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1`다. 이 도구는 서비스명, provider name, log name, 현재 JSONL-first/Event Log opt-in writer policy, 목표 default Windows Event Log provider writer, provider transition plan preview를 기록한다. 후속 `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`는 installed native `eventlog-register` corrected rerun으로 `PureCVisor Desktop Node` source registration과 event id `39100` write/query를 PASS로 확인했다. `public-ops-installed-hardening-code-level-2026-05-09-0393`는 `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard` native action을 추가했고, 최신 `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`는 MSI deferred LocalSystem `eventlog-default-transition`으로 `event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass`, `event_log_default_writer: installed-admin-smoke-pass`, `event_log_schema_version: 1`을 기록한다. Public trusted signing/external stable publication은 주장하지 않는다.

후속 `builtin-tls-certificate-lifecycle-preflight`는 `packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`다. 이 도구는 서비스명, certificate subject, HTTPS bind prefix, 현재 TLS mode, 목표 built-in service certificate mode, TLS lifecycle plan preview를 기록하지만 private key material 생성, certificate import/export, trust-store mutation, HTTPS/LAN binding, host mutation은 실행하지 않는다. 후속 execution-readiness slice는 initial/rotated public certificate generation과 hash 기록, private key dispose를 code-level로 확인해 `tls_certificate_lifecycle: partial-code-level-cert-generate-rotate-delete-pass`, `tls_private_key_material_written: false`를 기록했다. ADR-0006 internal scope의 실제 HTTPS binding, trust boundary, rotation/removal host mutation evidence는 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`에서 PASS로 닫혔다. Public trusted signing/external stable publication claim은 계속 없다.

후속 `service-token-rotation-revoke-preflight`는 `packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1`다. 이 도구는 서비스명, protected token path, 현재 DPAPI protected token file storage, rotation mode, service token rotation revoke plan preview를 기록하지만 token value read, new token generation, protected token write, service reload, old-token rejection verification, audit record write, host mutation은 실행하지 않는 dry-run descriptor다. 후속 installed-admin smoke `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`는 `DesktopNode.Host.exe service-action service-token-rotation-revoke`의 protected token backup/write/atomic replace, service restart, old bearer rejection, new bearer acceptance, redacted audit write를 PASS로 확인했다. Matrix 상태는 `service_token_rotation_revoke: installed-admin-smoke-pass`, `service_token_mutation: performed`, `token_value_observed: false`, `new_token_value_created: true`, `service_reload_status: restarted`, `old_token_rejection_status: old-token-rejected-after-reload`, `token_rotation_audit_status: written`, `host_mutation_performed: true`다.

후속 `diagnostic-bundle-server-preflight`는 `packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1`다. 이 도구는 서비스명, diagnostics root, Local API generation route, download route template, bearer authorization policy, redaction policy, retention policy, diagnostic bundle server-side plan preview를 기록하지만 Local API action execution, archive creation, download serving, redaction execution, retention application, product diagnostics runner delegation, host mutation은 실행하지 않는다. 실제 server-side generation/download implementation과 authz/redaction/retention evidence가 닫히기 전까지 `diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight`, `diagnostic_bundle_api_action: not-run`, `diagnostic_bundle_archive_created: false`, `diagnostic_bundle_download_served: false`, `diagnostic_bundle_redaction_status: not-run`, `diagnostic_bundle_authz_status: not-run`, `diagnostic_bundle_retention_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

후속 `diagnostic-bundle-server-code-level`는 `DesktopNodeApiRequestProcessor`, `DesktopNodeHostOptions`, `PcvDesktopNodeProduct.psm1`가 소유한다. 이 slice는 `POST /api/v1/diagnostics/bundles`에서 redacted `.bundle.json` archive를 만들고 `GET /api/v1/diagnostics/bundles/{bundle_id}/download`에서 저장된 bundle을 제공하며 product service plan에 `--diagnostics-root`를 연결한다. Installed listener execution, product wrapper diagnostics delegation, elevated host mutation은 실행하지 않는다. 실제 installed listener/product diagnostics delegation evidence가 닫히기 전까지 `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`, `host_mutation_performed: false`를 유지한다.

후속 `diagnostic-bundle-listener-code-level`는 `DesktopNodeHostApplication`이 소유한다. 이 slice는 in-process HttpListener에서 bearer-required create/download와 `X-PCV-Request-Id`/`X-Request-Id` propagation을 확인한다. 이 code-level slice 자체는 installed service listener execution, elevated host mutation을 실행하지 않으며 `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`를 유지한다.

후속 `diagnostic-bundle-product-wrapper-code-level`는 `PcvDesktopNodeProduct.psm1`이 소유한다. 이 slice는 `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`가 `New-PcvDesktopNodeDiagnosticBundle`로 위임되고 `product-wrapper-delegation-redacted.json`을 기록하는 product wrapper path를 확인한다. Matrix는 `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `actual_execution: code-level-product-wrapper`, `host_mutation_performed: false`를 기록한다. Installed service listener PASS는 별도 elevated MSI/service rerun evidence가 소유한다.

후속 `diagnostic-bundle-native-service-action-config-code-level`는 `DesktopNodeHostServiceAction`이 소유한다. 이 slice는 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config가 `--diagnostics-root`, protected token file, `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`를 `DesktopNodeWindowsServiceConfiguration.BinaryPathName`에 포함하도록 보강한다. 0.38.9 installed final `PathName`은 아직 이 인자들을 포함하지 않았지만, 후속 `0.39.0-admin-smoke` elevated MSI/service rerun은 installed listener execution을 `installed-listener-pass`, blocker `none`으로 닫았다.

후속 `os-mutation-gate-installed-listener-rerun`은 `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`가 추적한다. 이 evidence는 `0.39.0-admin-smoke` installed listener artifact를 입력으로 `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`에서 firewall enable/remove, LAN listener IP smoke, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore를 실행한 host mutation PASS다. Public trusted signing은 `excluded`, external stable publication은 `not-claimed`이며 public distribution claim 상태를 바꾸지 않는다.

후속 `timeout-rate-limit-hardening-preflight`는 `packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1`다. 이 도구는 서비스명, Local API route prefix, route timeout target, request limit target, retry-after target, UI/API error contract, timeout/rate-limit hardening plan preview를 기록하지만 server config mutation, middleware enablement, retry semantics change, UI/API error behavior verification, load test execution, host mutation은 실행하지 않는다. 실제 timeout/rate-limit hardening implementation과 UI/API/load-test evidence가 닫히기 전까지 `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

후속 `timeout-rate-limit-hardening-code-level`는 `DesktopNodeApiRequestProcessor`, `DesktopNodeHostApplication`, `DesktopNodeHostOptions`가 소유한다. 이 slice는 `/api/v1/` per-client request window, HTTP 429, `Retry-After`, `application/problem+json`, `PCV_RATE_LIMIT_EXCEEDED`를 code-level actual path로 적용하지만 route timeout enforcement, load test execution, server config mutation, installed service mutation, host mutation은 실행하지 않는다. 실제 route timeout/load-test/config evidence가 닫히기 전까지 `timeout_rate_limit_hardening: partial-code-level-request-limit`, `route_timeout_policy: not-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, `host_mutation_performed: false`를 유지한다.

후속 `timeout-rate-limit-hardening-route-timeout-code-level`는 `DesktopNodeApiRequestProcessor`가 소유한다. 이 slice는 `/api/v1/` GET/read route response deadline, HTTP 504, `Retry-After`, `application/problem+json`, `PCV_ROUTE_TIMEOUT`, `Gateway Timeout`, `route_timeout_seconds`, `request_id`를 code-level actual path로 적용한다. Mutation-route cancellation, native adapter cooperative cancellation, load test execution, server config mutation, installed service mutation, host mutation은 실행하지 않는다. 실제 load-test/config evidence가 닫히기 전까지 `timeout_rate_limit_hardening: partial-code-level-route-and-request-limit`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, `host_mutation_performed: false`를 유지한다.

후속 `timeout-rate-limit-hardening-server-config-code-level`는 `PcvDesktopNodeProduct.psm1`와 `DesktopNodeHostServiceAction`이 소유한다. 이 slice는 product service plan과 native service-action SCM config의 `DesktopNode.Host.exe listen` binary path에 `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`를 code-level로 연결하고 `service.hardening` descriptor 또는 `BinaryPathName`에 같은 값을 기록한다. Installed service mutation, service stop/start, load test execution, host mutation은 실행하지 않는다. 실제 load-test/installed service mutation evidence가 닫히기 전까지 `timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, `host_mutation_performed: false`를 유지한다.

후속 `timeout-rate-limit-hardening-load-test-code-level`는 `ApiHardeningRequestProcessorTests`가 소유한다. 이 slice는 `DesktopNodeApiRequestProcessor` in-process 경로에서 같은 client의 `/api/v1/runtime/policy` 요청 64개를 병렬 실행해 HTTP 200 20건, HTTP 429 44건, unexpected status 0건과 `PCV_RATE_LIMIT_EXCEEDED`, `retry_after_seconds=9`, problem-details JSON contract를 확인한다. 후속 실제 installed listener evidence `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`는 설치된 listener에 외부 HTTP 요청 180개를 보내 HTTP 200 140건, HTTP 429 40건, unexpected status 0건, 모든 429의 `Retry-After`와 `PCV_RATE_LIMIT_EXCEEDED` problem details를 확인했다. Matrix는 `load_test_status: code-level-inprocess-pass`, `installed_listener_external_load_rate_limit: pass`, `installed_listener_external_rate_limit_contract: retry-after-problem-details-pass`, `host_mutation_performed: false`를 기록한다.

후속 `burn-bootstrapper-preflight`는 `packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1`다. 이 도구는 packaging publication descriptor와 HTTPS MSI URL을 읽어 WiX Burn authoring preview를 만든다. 후속 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`는 actual WiX Burn bundle build와 install/repair/remove, direct MSI restore를 PASS로 기록했고 `burn_bootstrapper: build-install-repair-remove-pass-internal-smoke`, `host_mutation_performed: true`를 소유한다. Public trusted signing, timestamp evidence, external stable publication, winget submission, clean-host public signed update/rollback은 주장하지 않는다.

후속 `msix-packaging-feasibility-preflight`는 `packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1`다. 이 도구는 packaging publication descriptor에서 MSIX package manifest preview를 만들지만, 그 preflight 자체는 package build, install/update/remove smoke, public trusted signing, external stable publication을 실행하지 않는다.

2026-05-08 후속 `msix-package-lifecycle-smoke`는 별도 internal package identity `PureCVisor.DesktopNode.MsixSmoke`와 packaged service `PureCVisorDesktopNodeMsixSmoke`로 build/sign/verify, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence를 PASS로 확인했다. Evidence는 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/msix-package-lifecycle-smoke-20260510-0416`다. 이 evidence는 internal Root/leaf signing과 restricted service capability smoke이며 public trusted signing 또는 external stable publication evidence가 아니다.

## 과거 제안/보존 기록

ADR-0005는 채택하지 않는다. 아래 항목은 과거 public distribution/operations expansion 후보의 보존 기록이며, 현재 내부 사설망 제품의 release blocker가 아니다.

2026-05-17 PR #149 post-merge public-boundary CI evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr149-postmerge-pass.md`
는 run `25974335803`, job `76351743536`, head
`dd895306c4b08802d262b4afb890382dd991a4d0`에서 ADR-0006
`internal-private-network-only` guard PASS를 기록할 뿐 ADR-0005를 재개하지 않는다.
`0.42.28-admin-smoke` package chain은 다음 product payload 변경 전까지 보류하며, public
trusted signing, external stable publication, winget public submission, public stable installer
URL, public signed clean-host smoke는 ADR 변경 없이는 계속 out-of-scope다.

2026-05-17 PR #150 post-merge public-boundary CI evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass.md`
도 run `25983307305`, job `76375957834`, head
`6d4b5d95742044bdbd8def933fbc8cdefbba71b3`에서 같은 ADR-0006 guard PASS를 기록한다.
이 후속 evidence 역시 ADR-0005를 재개하지 않으며 public distribution claim을 승격하지 않는다.

- public trusted signing preflight and timestamp evidence (out-of-scope)
- Burn bootstrapper (internal lifecycle smoke pass, public signing/publication out-of-scope)
- MSIX feasibility/package path (internal lifecycle smoke pass, public publication out-of-scope)
- winget manifest validation and submission readiness (`winget validate` pass history, public submission out-of-scope)
- updater catalog publication (external upload out-of-scope; internal catalog/channel remains in ADR-0006 scope)
- public signed update/rollback smoke (out-of-scope; internal clean-host install/update/rollback remains in ADR-0006 scope)
- Windows Credential Manager transition (current-user capability pass plus installed LocalSystem service default transition pass)
- default Windows Event Log writer/provider transition (installed default writer repair/remove/volume/schema pass)
- built-in TLS certificate lifecycle (internal HTTPS/TLS lifecycle remains in ADR-0006 scope)
- service token rotation/revoke mutation API
- diagnostic bundle server-side generation/download
- timeout/rate-limit hardening (code-level and installed listener external load pass)

## Gate Matrix

보존용 public candidate 추적 표는 `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`다. 이 matrix는 `closed-not-adopted` 상태이며, public trusted signing/external stable publication/winget submission/public clean-host smoke를 `out-of-scope`로 재분류한다.

현재 적용 gate matrix는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`다.

## Dry-run Descriptor

`New-PcvPublicDistributionDescriptor.ps1`는 plan-only descriptor를 `summary.json`으로 작성한다. 이 descriptor는 `actual_execution: not-run`, `host_mutation_performed: false`, `public_trusted_signing: not-claimed`, `external_stable_publication: not-claimed`를 machine-readable anchor로 기록한다.

예상 command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvPublicDistributionDescriptor.ps1 -Version '0.39.0-public-candidate' -ArtifactRoot 'artifacts/public-distribution-operations-expansion-phase1-20260507-dryrun' -PlanOnly
```

## 검증

- `packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1`
- `src/DesktopNode.Api.Tests/ApiDiagnosticBundleRequestProcessorTests.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1`
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
- `git diff --check`

이 후보는 public trusted signing 또는 external stable publication evidence가 아니다.
