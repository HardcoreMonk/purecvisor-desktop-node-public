# Full admin host mutation gate `0.42.72-admin-smoke` (2026-08-09)

evidence_id: `full-admin-host-mutation-gate-2026-08-09-04272-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.72-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260809-04272`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260809-04272`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260809-04272`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260809-04272`
operational_fullgate_msi_sha256: `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa`
operational_fullgate_payload_aggregate_sha256: `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933`
service_host_sha256: `c989fa5db901a7e64bd9b5040024804b0c3a3ee9a3ad138a94a06007d7ef86b3`
cli_sha256: `c7fac8d2f671596878ae58808b79028d7a2951dee371c5f371984a1d23f2d60c`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
provenance_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `237.060s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.098s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-d887815f` |
| generation / switch | Gen2 / `Default Switch` |
| VM list / checkpoint list | 대상 포함 `true` / checkpoint 포함 `true` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| repeated delete | 첫 delete 후 absent, 두 번째 delete도 idempotent `absent` |
| unmanaged VM | `pcv-spike-api-foreign-7a2f66b1` |
| unmanaged delete guard | `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, VM 유지 `true` |
| cleanup | managed/unmanaged 임시 경로와 VM 정리 |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.72-admin-smoke` (DisplayVersion `0.42.72`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| boot time | 양 단계에서 `2026-08-09 14:01:16.5 +09:00` 유지 |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| final firewall rule count | `0` |
| final eventlog source present | `false` |
| internal trust root / publisher | restore 후 모두 present |

## Provenance

Operational MSI는 gate 내부 build가 source commit `02428fab`에서 생성했다. Clean package와
Host/CLI/module 핵심 hash는 같지만 MSI와 payload aggregate는 별도 빌드 값이다.

| artifact | MSI SHA-256 | payload aggregate |
| --- | --- | --- |
| clean package | `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22` | `39475ad14a9bbd48ecf41c24bac5e42b391535783276cd5ed4d960af276962f0` |
| operational fullgate | `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa` | `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933` |

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair, actual-VM functional, credential rebootstrap, token rotation은 각각
  별도 evidence가 소유한다.
