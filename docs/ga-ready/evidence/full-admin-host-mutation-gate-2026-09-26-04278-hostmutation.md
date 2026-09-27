# Full admin host mutation gate `0.42.78-admin-smoke` (2026-09-26)

evidence_id: `full-admin-host-mutation-gate-2026-09-26-04278-hostmutation`
result: `FAIL`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.78-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260926-04278`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260926-04278`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260926-04278`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260926-04278`
failed_step_id: `service-msi-hyperv-admin-smoke`
first_error: `PCV_NETWORK_INVENTORY_FAILED`
host_status_reason: `PCV_DEFAULT_SWITCH_UNKNOWN`
attempt_count: `2`
operational_msi_sha256: `b8e579d75b150e843e62f6fbed130638bfd9a0d4ea4657477ce0b3222dd7d6f5`
operational_payload_aggregate_sha256: `08774058b6ffb7fcbe8a5173c34fc645a4a6b7bbce482b98c635ca6aea446ebf`
service_host_sha256: `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b`
cli_sha256: `9222ef938873bf5e05e12f8b5af9be30cf0e2f72602309d12dcaf8669748ade7`
provenance_commit: `e098e0a55333afe7eccd9150c5ef9ca14578cc40`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts |
| --- | --- | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `FAIL` | `1` | `2` |
| `os-mutation-gate` | `not-run` |  |  |

MSI build, service-action, MSI lifecycle은 `completed`다. 설치본 Hyper-V route smoke는
`GET /api/v1/network/inventory`에서 멈췄다. 두 attempt의 최초 오류는 같다.

`PCV_NETWORK_INVENTORY_FAILED`. detail은 `일반 오류입니다.` VM create는 시작하지 않았다.

같은 시각의 `host.status`는 `ok=true`, `supported=false`,
`reasons=PCV_DEFAULT_SWITCH_UNKNOWN`, `default_switch_present=false`,
`vmms_running=true`다. 게이트 이후 read-only `pcvcli network inventory`와
`Get-VMSwitch`도 같은 일반 오류다.

## 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 manifest | `0.42.78-admin-smoke` |
| ARP DisplayVersion | `0.42.78` |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| boot time | `2026-09-25T19:10:30.5000000+09:00` unchanged |
| `pcv-spike-*` | `0` |
| `pcv-guest-installed-04253-r1` | `Off`, generation 1 |
| `pcv-cleanhost-20260910-r2-04274-04275` | `Saved`, generation 1 |

## Nonclaims

- 이 실행은 full admin host mutation PASS가 아니다.
- OS mutation gate, installed current-card, current-evidence 쓰기를 수행하지 않았다.
- `docs/ga-ready/current-evidence.json`은 `0.42.77-admin-smoke`다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
