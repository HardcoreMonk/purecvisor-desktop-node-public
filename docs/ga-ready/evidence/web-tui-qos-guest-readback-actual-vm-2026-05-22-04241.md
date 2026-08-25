# Installed TUI actual VM row projection 2026-05-22 0.42.41

evidence_id: `web-tui-qos-guest-readback-actual-vm-2026-05-22-04241`
result: `PASS`
scope: `installed-tui-actual-vm-row-projection-after-04241-package-chain`
version: `0.42.41-admin-smoke`
artifact_root: `artifacts/web-tui-qos-guest-readback-actual-vm-20260522-04241`
summary: `artifacts/web-tui-qos-guest-readback-actual-vm-20260522-04241/summary.json`
actual_vm_name: `pcv-ux-qos-04241`
actual_vm_root: `C:\Users\Operator\AppData\Local\Temp\pcv-ux-qos-04241`
actual_vm_iso: `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`
installed_tui_actual_vm_row_projection: `pass`
package_chain_trigger: `closed-by-0.42.41-installed-smoke`
host_mutation_performed: `true`
token_value_observed: `false`
password_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

`0.42.40-admin-smoke` 실제 VM smoke에서 설치본 `pcvtui --smoke-once vm`이 API body의
실제 VM을 table row로 투영하지 못하는 blocker를 드러냈다. `0.42.41-admin-smoke` 설치본은
같은 실제 VM 조건에서 `pcv-ux-qos-04241`, `running`, `1 vCPU`, `1024 MB` row를 렌더링했고,
cleanup 후 VM과 Temp VM root가 제거됐다.

## 확인

| 항목 | 결과 |
| --- | --- |
| `pcvcli --json host status` | PASS, Hyper-V feature/VMMS/default switch ready |
| `pcvcli --json vm create pcv-ux-qos-04241 ...` | PASS, job succeeded |
| `pcvcli --json vm start pcv-ux-qos-04241` | PASS, job succeeded |
| `pcvcli --json vm list` after start | PASS, row state `running` |
| `pcvtui --smoke-once vm` | PASS, table contains `pcv-ux-qos-04241` row |
| cleanup `poweroff/delete` | PASS, final `vm list` empty, VM root removed |

## 경계

이 evidence는 설치본 TUI row projection smoke다. Web direct mutation/control, Linux
cgroup QoS 호환, qemu guest agent 호환, public trusted signing, 외부 stable publication은
주장하지 않는다.
