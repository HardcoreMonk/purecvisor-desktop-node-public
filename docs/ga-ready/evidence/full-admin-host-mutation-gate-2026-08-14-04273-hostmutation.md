# Full admin host mutation gate `0.42.73-admin-smoke` (2026-08-14)

evidence_id: `full-admin-host-mutation-gate-2026-08-14-04273-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.73-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260814-04273`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260814-04273`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260814-04273`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260814-04273`
operational_fullgate_msi_sha256: `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1`
operational_fullgate_payload_aggregate_sha256: `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85`
service_host_sha256: `a437a78b7198cb04d588e8b80688a522b3497fe5b8cdddc41d6f3483e197e9e2`
cli_sha256: `b8a7374e843999d2979ba5181d18fb91909a375ef0482b840cb942c253b40bc2`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
provenance_commit: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `84.873s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.112s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-b2cafc24` |
| generation / switch | Gen2 / `Default Switch` |
| VM list / checkpoint list | 대상 포함 `true` / checkpoint 포함 `true` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| repeated delete | 첫 delete `action=delete`, 두 번째 delete idempotent `action=absent` |
| unmanaged VM | `pcv-spike-api-foreign-24af5066` |
| unmanaged delete guard | `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, VM 유지 `true` |
| cleanup | managed/unmanaged 임시 경로 정리, `remaining_pcv_vms=[]` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.73-admin-smoke` (DisplayVersion `0.42.73`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| boot time | 양 단계에서 `2026-08-13 06:57:43.5 +09:00` 유지 |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| final firewall rule count | `0` |
| final eventlog source present | `false` |
| internal trust root / publisher | restore 후 모두 present |
| Web `/` / `/pcv-config.js` | HTTP `200` / HTTP `200` |
| TUI | `pcvtui.exe` absent |

## Provenance

Operational MSI는 gate 내부 build가 source commit `b84441f0`에서 생성했다. Clean package와
Host/CLI/module 핵심 hash는 같지만 MSI와 payload aggregate는 별도 빌드 값이다.

| artifact | MSI SHA-256 | payload aggregate |
| --- | --- | --- |
| clean package | `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee` | `bbe2bfde532260eab7bd80de13e4e13350ae6553e4ef6a4037faa6e650359660` |
| operational fullgate | `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1` | `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85` |

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair, actual-VM functional, credential rebootstrap, token rotation,
  installed current-card 승격은 각각 별도 evidence가 소유한다.
- canonical `current-evidence.json`은 이 fullgate만으로 승격하지 않는다.
