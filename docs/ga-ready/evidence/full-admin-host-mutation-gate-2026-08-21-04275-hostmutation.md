# Full admin host mutation gate `0.42.75-admin-smoke` (2026-08-21)

evidence_id: `full-admin-host-mutation-gate-2026-08-21-04275-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.75-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260821-04275`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260821-04275`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260821-04275`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260821-04275`
operational_fullgate_msi_sha256: `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b`
operational_fullgate_payload_aggregate_sha256: `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7`
service_host_sha256: `f8fd9147b9a2fd8ab51cf5c8a5aedea6c06bbfcd581b37dbc218680e6b780580`
cli_sha256: `7e2b99bc0eda1fb11dcaac40b24b829581de7167d79552e0c48c40decdf1211d`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
provenance_commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `236.922s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.099s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-8f5c8162` |
| generation / switch | Gen2 / `Default Switch` |
| VM list / checkpoint list | 대상 포함 `true` / checkpoint 포함 `true` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| unmanaged VM | `pcv-spike-api-foreign-2bc29950` |
| unmanaged delete guard | `PCV_VM_NOT_MANAGED_BY_PURECVISOR` |
| cleanup | managed/unmanaged 임시 경로 정리, `remaining_pcv_vms=[]` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.75-admin-smoke` (DisplayVersion `0.42.75`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| Web `/` / `/pcv-config.js` | HTTP `200` / HTTP `200` |
| TUI | `pcvtui.exe` absent |

## Provenance

Operational MSI는 gate 내부 build가 source commit `dbe1b48`에서 생성했다. Clean package와
Host/CLI/module 핵심 hash는 같지만 MSI와 payload aggregate는 별도 빌드 값이다.

| artifact | MSI SHA-256 | payload aggregate |
| --- | --- | --- |
| clean package | `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6` | `3c33a35b21eb9cdd2b24156cc98afe2268f82f3ca32c7dd6a03882a262afdd2c` |
| operational fullgate | `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b` | `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7` |

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair, actual-VM functional, credential rebootstrap, token rotation,
  installed current-card 승격은 각각 별도 evidence가 소유한다.
- canonical `current-evidence.json` 승격은 같은 Lane 3 ledger update가 소유한다. operational
  current는 `0.42.75-admin-smoke`다.
