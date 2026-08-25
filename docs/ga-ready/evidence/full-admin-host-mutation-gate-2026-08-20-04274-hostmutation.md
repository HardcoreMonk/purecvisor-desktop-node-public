# Full admin host mutation gate `0.42.74-admin-smoke` (2026-08-20)

evidence_id: `full-admin-host-mutation-gate-2026-08-20-04274-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.74-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260820-04274`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260820-04274`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260820-04274`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260820-04274`
operational_fullgate_msi_sha256: `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`
operational_fullgate_payload_aggregate_sha256: `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`
service_host_sha256: `328de2af97a8ba2c132bb0a5de15504bf602233b24a2ce687c2a83f4b10335f9`
cli_sha256: `21b22cdaa9640ea8b63a031e4a815da1f583a60ca3c6e8486595bdc4a5eb07b0`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
provenance_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.74-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `218.355s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.106s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-79522716` |
| generation / switch | Gen2 / `Default Switch` |
| VM list / checkpoint list | 대상 포함 `true` / checkpoint 포함 `true` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| repeated delete | 첫 delete `action=delete`, 두 번째 delete idempotent `action=absent` |
| unmanaged VM | `pcv-spike-api-foreign-74c65cc4` |
| unmanaged delete guard | `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, VM 유지 `true` |
| cleanup | managed/unmanaged 임시 경로 정리, `remaining_pcv_vms=[]` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.74-admin-smoke` (DisplayVersion `0.42.74`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| boot time | 양 단계에서 `2026-08-20 16:10:29.5 +09:00` 유지 |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| final firewall rule count | `0` |
| final eventlog source present | `false` |
| internal trust root / publisher | restore 후 모두 present |
| Web `/` / `/pcv-config.js` | HTTP `200` / HTTP `200` |
| TUI | `pcvtui.exe` absent |

## Provenance

Operational MSI는 gate 내부 build가 source commit `adc04673`에서 생성했다. Clean package와
Host/CLI/module 핵심 hash는 같지만 MSI와 payload aggregate는 별도 빌드 값이다.

| artifact | MSI SHA-256 | payload aggregate |
| --- | --- | --- |
| clean package | `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e` | `c55cd17d14fed521252e6fee1bf08c828410339b23172fadb01dbd19f7d2578e` |
| operational fullgate | `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787` | `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d` |

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair, actual-VM functional, credential rebootstrap, token rotation,
  installed current-card 승격은 각각 별도 evidence가 소유한다.
- canonical `current-evidence.json` 승격은 같은 날 ledger update가 소유한다. operational
  current는 `0.42.74-admin-smoke`다. P0 `vm.save` FAIL는 열린 결함이다.
