# Clean-host install/update/rollback `0.42.77-admin-smoke` -> `0.42.78-admin-smoke`

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-09-25-04277-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20260925-04277-04278`
vm_name: `pcv-cleanhost-20260925-04278`
baseline_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
update_package_sha256: `6cb31a6b75d58a10a35a4aba5dd6d78e249f5c72f2ac8ad4a41601585939a821`
target_provenance_commit: `e098e0a55333afe7eccd9150c5ef9ca14578cc40`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
guest_reboot_performed: `true`
smoke_vm_removed_on_success: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

전용 Generation 1 VM `pcv-cleanhost-20260925-04278`를 Windows Server 2022 Evaluation
기준 VHD의 differencing 디스크로 만들고 `Default Switch`에 연결했다. Windows Update
준비는 게스트 재부팅을 포함해 통과했다. 기준 MSI를 설치한 뒤 카탈로그 update로
`0.42.78-admin-smoke`를 적용하고 rollback으로 `0.42.77-admin-smoke`를 복원했다.

| 단계 | 결과 |
| --- | --- |
| summary `ok` | `true` |
| `internal_clean_host_install_update_rollback_smoke` | `pass` |
| baseline MSI install exit | `0` |
| catalog update exit | `0` |
| rollback exit | `0` |
| baseline manifest | `0.42.77-admin-smoke` |
| updated manifest | `0.42.78-admin-smoke` |
| final manifest | `0.42.77-admin-smoke` |
| final Web Console | HTTP `200` |
| final service | `PureCVisorDesktopNode` Running |
| rollback 후 failed root | 있음 |
| blocker | `none` |

성공 후 전용 VM과 differencing 디스크, VM 폴더를 제거했다. 기존 VM
`pcv-cleanhost-20260910-r2-04274-04275`는 Saved, `pcv-guest-installed-04253-r1`은 Off다.

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| Burn install/repair/remove | `not-run` |
| MSIX build/install/update/remove | `not-run` |
| installed runtime ops summary | `not-run` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `0.42.77-admin-smoke -> 0.42.78-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.77-admin-smoke` |

## Nonclaims

- 이 기록은 전용 clean-host의 MSI 설치, 카탈로그 update, rollback만 증명한다.
- operational current를 `0.42.78-admin-smoke`로 올리지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
