# 내부 사설망 경계 근거 - 2026-05-10

```text
evidence_id: internal-private-network-boundary-2026-05-10
decision_marker: internal-private-network-only
adr: ADR-0006
scope: docs-boundary-reclassification
actual_execution: docs-only-boundary-reclassification
host_mutation_performed: false
public_distribution_candidate: closed-not-adopted
public_trusted_signing: out-of-scope
trusted_timestamp_evidence: out-of-scope
external_stable_publication: out-of-scope
winget_submission: out-of-scope
public_stable_installer_url: out-of-scope
clean_host_public_signed_install_update_rollback_smoke: out-of-scope
internal_distribution_matrix: docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md
internal_https_tls_lifecycle_installed_smoke: pass
internal_clean_host_install_update_rollback_smoke: pass
internal_clean_host_install_update_rollback_evidence: docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md
public_release: not-claimed
```

## 결과

Desktop Node 제품 범위를 내부 사설망 전용으로 고정했다. ADR-0005 public distribution candidate는 적용하지 않고 미채택/종료 상태로 보존한다.

이 evidence는 public trusted signing, 외부 stable publication, winget submission, public signed clean-host smoke를 주장하지 않는다. 해당 gate들은 내부 사설망 제품의 release blocker가 아니며 `out-of-scope`로 재분류한다.

## 내부 범위

현재 적용 gate는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`다. 내부 배포 판단은 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal service token/Credential Manager/Event Log evidence, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke를 기준으로 한다.

Internal HTTPS/TLS lifecycle installed smoke는 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`에서 PASS로 닫혔다. Internal clean-host install/update/rollback smoke는 `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`에서 dedicated Hyper-V clean-host install/update/rollback PASS로 닫혔다. 이 두 항목은 public CA, public timestamp, winget, external upload credential이 필요하지 않다.
