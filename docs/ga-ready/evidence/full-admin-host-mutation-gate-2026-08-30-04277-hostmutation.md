# Full admin host mutation gate `0.42.77-admin-smoke` (2026-08-30)

evidence_id: `full-admin-host-mutation-gate-2026-08-30-04277-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.77-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260830-04277`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260830-04277`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260830-04277`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260830-04277`
batch_summary_sha256: `b18a2774ee10094c64220b1e3d42e8ecb5f1005357c7bd0c85064d5e2f045fce`
routeparity_summary_sha256: `a45e50c5271619ca12213468242070a475dd4041c13e51b1a7bcb940f3499d50`
os_summary_sha256: `54a74f373a57ca2d921e679f72b90353a36e9b072c4f82183cae1f7546abda97`
operational_fullgate_msi_sha256: `d4ebba77adcd7af92275509a65809c926f5bc6fb6bf8f61c49a610943998000f`
operational_fullgate_payload_aggregate_sha256: `d16e498a3d14ed67e361bef26a26feb87839490425e2101453f28742839d84a1`
service_host_sha256: `810cccc9e1ef86f8f31532e4775a033ba63410dba6d1e4d07e74e18e9e883859`
cli_sha256: `6cbfa2df1b77da55e8308d95e25ff89e4292f07dc15f9ccd094589b6ad033383`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
provenance_commit: `9f051b5a9cca80634e8ad7c4d15267a414c79d66`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `206.291s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.088s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다. boot time
`2026-08-28T11:30:49.5+09:00`는 변하지 않았다.

OS gate는 Event Log register/remove, firewall enable/remove, LAN listener smoke,
existing internal trust-store install/remove/restore가 PASS다. final firewall
count `0`, Event Log source absent, internal Root/TrustedPublisher present다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-02cea841` |
| generation / switch | Gen2 / `Default Switch` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| unmanaged VM | `pcv-spike-api-foreign-a7c0b1ed` |
| unmanaged delete guard | `PCV_VM_NOT_MANAGED_BY_PURECVISOR` |
| routeparity `remaining_pcv_vms` | `[]` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.77-admin-smoke` (DisplayVersion `0.42.77`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| Web `/` / `/pcv-config.js` | HTTP `200` / HTTP `200` |
| TUI | `pcvtui.exe` absent |

## Provenance

Operational MSI는 gate 내부 build가 source commit `9f051b5`에서 생성했다. Clean
package `0.42.77-admin-smoke`는 `04b3c9f` 기준이라 Host/CLI/MSI/payload aggregate
hash가 다르다. product wrapper SHA-256은 04275/04277 clean package와 같다.

| artifact | MSI SHA-256 | payload aggregate |
| --- | --- | --- |
| clean package | `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957` | `370a267f7c9fdec1d89c9a1890af4941c688d25b9cad634d45de3774b5e4b99c` |
| operational fullgate | `d4ebba77adcd7af92275509a65809c926f5bc6fb6bf8f61c49a610943998000f` | `d16e498a3d14ed67e361bef26a26feb87839490425e2101453f28742839d84a1` |

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair, installed current-card, Lane 3 current 승격은 이 문서가
  소유하지 않는다.
- `docs/ga-ready/current-evidence.json`은 `0.42.75-admin-smoke`로 유지한다.
- 호스트에 남은 `pcv-guest-installed-04253-r1`는 이 gate가 만들지 않았고 지우지 않았다.
  report-only다.
