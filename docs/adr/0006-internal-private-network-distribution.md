# ADR-0006: 내부 사설망 배포 경계

상태: 적용
일자: 2026-05-10

## 결정 마커

```text
DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only
DESKTOP_NODE_PUBLIC_DISTRIBUTION_DECISION_CANDIDATE: closed-not-adopted
previous_04256_scope_lock_latest_internal_admin_smoke: 0.42.56-admin-smoke
previous_04256_scope_lock_latest_manual_admin_package_pair: 0.42.55-admin-smoke -> 0.42.56-admin-smoke
previous_04256_scope_lock_latest_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260528-04256
scope_lock_latest_internal_admin_smoke: 0.42.57-admin-smoke
scope_lock_latest_manual_admin_package_pair: 0.42.56-admin-smoke -> 0.42.57-admin-smoke
scope_lock_latest_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260528-04257
scope_lock_latest_target_msi_sha256: 809eacb97a49aeaa32fc0ea3dce8ac5bdeb7c66b8b4502352519a338a512847e
scope_lock_latest_clean_package_msi_sha256: 2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9
scope_lock_latest_payload_aggregate_sha256: 7a34468d3a59c2da182835a03f440f22df9e70f31ff062dc625530a9143ef94d
scope_lock_latest_update_zip_sha256: c50e846e51a568a184cd706dc71506cdad95d8248c4e89713f2f52b690236946
scope_lock_latest_provenance_commit: 16cc0d6b592d7f2f9ead14c41d8f4ad0e1f28b76
scope_change_requires_adr: true
previous_scope_lock_internal_admin_smoke: 0.42.34-admin-smoke
historical_04234_internal_admin_smoke: 0.42.34-admin-smoke
historical_04232_04234_manual_admin_package_pair: 0.42.32-admin-smoke -> 0.42.34-admin-smoke
historical_04234_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260519-04234
historical_04234_target_msi_sha256: aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78
historical_04234_payload_aggregate_sha256: a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5
historical_04232_04234_update_zip_sha256: da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad
historical_04234_provenance_commit: fc8cc284b7824172b8bf035858fb86b21bd26e5d
historical_04232_internal_admin_smoke: 0.42.32-admin-smoke
historical_04231_04232_manual_admin_package_pair: 0.42.31-admin-smoke -> 0.42.32-admin-smoke
historical_04232_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260519-04232
historical_04232_target_msi_sha256: 3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9
historical_04232_payload_aggregate_sha256: 21e2f8136ac53384bf86966e51f9040f7bbb37e62bc9e761640c0d1aeff35956
historical_04232_update_zip_sha256: c2e5c577d1a9bbec1ce6ca7ca2f79588d17b908d4aa639adb7968e5a09ce38da
historical_04232_provenance_commit: fc8cc284b7824172b8bf035858fb86b21bd26e5d
historical_04231_internal_admin_smoke: 0.42.31-admin-smoke
historical_04230_04231_manual_admin_package_pair: 0.42.30-admin-smoke -> 0.42.31-admin-smoke
historical_04231_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260518-04231
historical_04231_target_msi_sha256: c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f
historical_04231_payload_aggregate_sha256: cea7d1f798e6f0889cf0cd02da049dc7d7b0131e8df51a768c12e02ea76c22f4
historical_04231_update_zip_sha256: de258c8f58ff8fd25ea78ea74483746c89190b3a7aa84345f3789eaa02458a44
historical_04231_provenance_commit: fc8cc284b7824172b8bf035858fb86b21bd26e5d
historical_04230_internal_admin_smoke: 0.42.30-admin-smoke
historical_04229_internal_admin_smoke: 0.42.29-admin-smoke
historical_04228_internal_admin_smoke: 0.42.28-admin-smoke
historical_04229_04230_manual_admin_package_pair: 0.42.29-admin-smoke -> 0.42.30-admin-smoke
historical_04230_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260518-04230
historical_04230_target_msi_sha256: 90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86
historical_04230_payload_aggregate_sha256: 0fddc06c7ced0239ea04a89fd90cc0c152a64688904e0f58b97c3fcd5368a28c
historical_04230_update_zip_sha256: f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2
historical_04230_provenance_commit: f4349cf049db66b0ae1d5d38a948a6b03a8b0648
historical_04226_04227_manual_admin_package_pair: 0.42.26-admin-smoke -> 0.42.27-admin-smoke
historical_04227_04228_manual_admin_package_pair: 0.42.27-admin-smoke -> 0.42.28-admin-smoke
historical_04228_04229_manual_admin_package_pair: 0.42.28-admin-smoke -> 0.42.29-admin-smoke
historical_04228_04229_manual_admin_descriptor: manual-admin-campaign-descriptor-20260517-04228-04229-closed
historical_04229_target_msi_sha256: 2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d
historical_04229_update_zip_sha256: 3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542
historical_04229_provenance_commit: d306712ad671c8a00d5c560765b8952e24a07502
historical_04228_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260517-04228
historical_04228_target_msi_sha256: 223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e
historical_04228_clean_package_msi_sha256: a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74
historical_04228_update_zip_sha256: e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c
historical_04228_provenance_commit: b9676f6dc37d667ae0d60367e9f4e576a27e3864
historical_pr151_public_boundary_job_id: 76380096421
```

2026-05-28 현재 최신 internal admin-smoke evidence는 `0.42.55-admin-smoke` /
`full-admin-host-mutation-gate-20260528-04255`다. Full-gate MSI SHA-256은
`cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb`,
payload aggregate SHA-256은
`69019129347920bba88c269a4828dae5b214eace8a6d31bd60bc7fa7f1b81934`,
provenance commit은 `958052181012f7d1be6ccff535316bfaeeef07df`이다. Guest Execution
running cancel affordance installed evidence와 actual credentialed guest-exec evidence를 포함하며
Host Ops Web diagnostics bucket table contract는 `host-ops-web-diagnostics-bucket-table-v1`로 유지한다.
이 evidence 역시 public trusted signing 또는 external stable publication claim이 아니다.
직전 0.42.53 scope lock, 0.42.50 scope lock, 0.42.48 scope lock, 0.42.47 scope lock, 0.42.45 scope lock, 0.42.41 scope lock과 0.42.34 scope lock은 historical predecessor로 보존한다.
0.42.34 full-gate MSI SHA-256
`aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, payload aggregate
SHA-256 `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`, provenance
commit `fc8cc284b7824172b8bf035858fb86b21bd26e5d`를 이력으로 남긴다.

## 맥락

Desktop Node는 독립 Windows 저장소의 내부 운영 서비스다. ADR-0005는 public distribution/운영 확장 후보로 public trusted signing, timestamp evidence, winget submission, external stable publication, public signed clean-host smoke를 추적했지만, 제품 범위가 내부 사설망 전용으로 고정되면서 public distribution 후보는 적용하지 않는다.

기존 ADR-0003의 internal Root/leaf `RequireSigned` trust model과 내부 관리자 smoke evidence는 유지한다. 개발/검증 중 `AllowUnsignedDev` evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.

## 결정

Desktop Node의 배포 경계는 내부 사설망 전용이다.

- Public trusted signing provider/cert chain/timestamp evidence는 out-of-scope다.
- External stable publication/catalog upload는 out-of-scope다.
- Winget public submission은 out-of-scope다.
- Public stable installer URL은 out-of-scope다.
- Clean-host public signed install/update/rollback smoke는 out-of-scope다.
- ADR-0005 public distribution candidate는 미채택/종료한다.
- 배포/운영 gate는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`를 기준으로 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle, internal clean-host install/update/rollback smoke를 추적한다.

## 내부 배포 기준

내부 배포는 다음 evidence 계열로 판정한다.

- Internal Root/leaf `RequireSigned` signing 또는 명시적 internal admin-smoke signing boundary
- Internal signed MSI build/install/update evidence
- Internal updater catalog/channel resolver evidence
- Private LAN listener smoke and rate-limit smoke
- Installed service token rotation/revoke, Windows Credential Manager, Windows Event Log default writer evidence
- Internal HTTPS/TLS certificate lifecycle installed smoke
- Internal clean-host install/update/rollback smoke

## 결과

ADR-0005의 public gate matrix와 evidence는 역사 기록으로 보존하되 현재 적용 gate가 아니다. `PUBLIC_DISTRIBUTION_GATE_MATRIX`는 closed-not-adopted 상태로 재분류하고, public signing/winget/external upload/public clean-host smoke 항목은 out-of-scope로 처리한다.

내부 사설망 제품에서 public CA 또는 public distribution 채널이 없더라도 release blocker로 보지 않는다. 운영자는 내부 신뢰 저장소, 사설망 artifact location, 내부 updater catalog, 내부 clean-host runner evidence를 기준으로 배포 가능성을 판단한다.

2026-05-12 `manual-admin-campaign-2026-05-12-0423-0424`는 ADR-0006 경계를
유지한 채 `0.42.3-admin-smoke` baseline과 `0.42.4-admin-smoke` target package를
재검증했다. Full admin host mutation, Operator Access, Internal Service Hardening,
installed update/rollback은 PASS였지만 dedicated clean-host package-pair는
`0.42.3` baseline MSI custom action sequence 결함으로 blocked다. 이 결함은 public
distribution blocker가 아니라 internal MSI lifecycle 품질 결함이며, 이 branch에서
`ConfigureInstalled -> EventLogDefaultTransition -> CredentialManagerDefaultTransition`
순서로 code-level 수정했다. 이 evidence는 0425→0426 PASS 이후 historical-only
blocker record로 보존한다.

2026-05-12 `manual-admin-campaign-2026-05-12-0425-0426`은 위 sequence fix와 Burn
repair fix를 포함한 다음 package-pair evidence다. `0.42.5-admin-smoke` baseline과
`0.42.6-admin-smoke` target에서 installed update/rollback, dedicated clean-host
install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove,
installed runtime ops summary capture가 PASS다. 이 PASS는 internal/admin-smoke 배포
경계를 강화하지만 public trusted signing 또는 외부 stable publication claim을
추가하지 않는다. Post-merge `0.42.6-admin-smoke` rebuild는
`docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md`에서
MSI SHA-256 `9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16`,
provenance commit `37f4d6b83d6caef1338e0a60e5df0a60209b51f8`로 보존한다.

2026-05-18 최신 scope lock은 `0.42.31-admin-smoke`다. 04231 full admin host
mutation, installed Web/TUI/CLI current-card, Host Ops lifecycle descriptor bridge,
Host Ops Web diagnostics bucket table,
`0.42.30-admin-smoke -> 0.42.31-admin-smoke` manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04230-04231.md` /
`manual-admin-campaign-descriptor-20260518-04230-04231-closed`가 소유하는
ADR-0006 `internal-private-network-only` evidence다. Public trusted signing, external
stable publication, winget submission, public stable installer URL, public signed
clean-host smoke는 계속 `out-of-scope`다. 04229 account/noVNC smoke와 이전 scope
lock evidence는 historical predecessor로 보존한다.
직전 `0.42.29-admin-smoke -> 0.42.30-admin-smoke` closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md` /
`manual-admin-campaign-descriptor-20260518-04229-04230-closed`로 historical predecessor
보존한다.
직전 `0.42.28-admin-smoke -> 0.42.29-admin-smoke` selector/package-chain
predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md` /
`manual-admin-campaign-descriptor-20260517-04228-04229-closed`가 소유하며 target MSI
SHA-256 `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`,
update ZIP SHA-256 `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`,
provenance commit `d306712ad671c8a00d5c560765b8952e24a07502`로 보존한다.
이 predecessor도 public trusted signing 또는 외부 stable publication evidence가 아니다.

2026-05-16 이전 scope lock은 `0.42.26-admin-smoke`다. 04226 full admin host
mutation, installed Web/TUI/CLI current-card, 04225→04226 descriptor candidate,
`0.42.24-admin-smoke -> 0.42.25-admin-smoke` manual-admin package-pair closure,
PR #145 post-merge public-boundary guard는 ADR-0006
`internal-private-network-only` evidence이며 public trusted signing, external stable
publication, winget submission, public stable installer URL, public signed clean-host
smoke는 여전히 `out-of-scope`다. 04225 Runtime/API `current_evidence` rollup은
historical predecessor로 보존한다. 이 경계를 바꾸려면 ADR-0006을 supersede하는 새
ADR 또는 명시적인 ADR 변경이 필요하다.

## 검증

이 ADR 자체는 host mutation을 수행하지 않는다. 적용 검증은 문서 guard, internal matrix guard, `git diff --check`, 그리고 별도 installed/internal smoke evidence로 수행한다. 현재 internal HTTPS/TLS lifecycle은 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`, historical internal clean-host install/update/rollback은 `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`에서 PASS로 닫혔다. 0423→0424 clean-host rerun은 historical blocker evidence로 보존하며 current clean-host PASS는 0425→0426 campaign이 소유한다.
