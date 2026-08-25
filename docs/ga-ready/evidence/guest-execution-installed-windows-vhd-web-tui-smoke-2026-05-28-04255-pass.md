# Guest Execution installed Windows VHD Web/TUI smoke 2026-05-28 0.42.55

evidence_id: `guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass`
result: `PASS`
scope: `persistent-installed-windows-vhd-credentialed-guest-exec-web-tui-current-card`
version: `0.42.55-admin-smoke`
artifact_root: `artifacts/guest-execution-installed-windows-vhd-smoke-20260528-04255-r1`
summary: `artifacts/guest-execution-installed-windows-vhd-smoke-20260528-04255-r1/summary.json`
surface_summary: `artifacts/guest-execution-installed-windows-vhd-smoke-20260528-04255-r1/surface-smoke-summary.json`
vm_name: `pcv-guest-installed-04253-r1`
credential_ref_type: `dpapi-local-machine`
credential_ref: `dpapi:<protected-file>`
credential_file_path: `C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi`
host_mutation_performed: `false`
persistent_vm_left_running: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행 명령

```powershell
pcvcli --json vm get pcv-guest-installed-04253-r1
pcvcli --json vm guest-agent-ensure-channel pcv-guest-installed-04253-r1 --verify --credential-ref dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi --timeout-sec 60
pcvcli --json vm guest-exec pcv-guest-installed-04253-r1 --credential-ref dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi --timeout-sec 60 -- powershell -NoProfile -Command hostname
```

## 결과

| 항목 | 결과 |
| --- | --- |
| VM projection | `state=running`, `guest_family=windows`, `managed_by_purecvisor=true` |
| Channel verify job | `job-92e44ca99cde460b9e34567168dbb7cd`, `succeeded`, transport `windows-powershell-direct` |
| Guest exec job | `job-0e05ae5a574d49a5822237337c1e9ad3`, `succeeded`, transport `windows-powershell-direct` |
| Command | `powershell -NoProfile -Command hostname` |
| Guest exec exit code | `0` |
| Audit schema | `guest-execution-audit-v1` |
| Redaction policy | `guest-execution-redaction-v1` |
| stdout digest | `269f926cfaa497cbc7a78ad24a0e94ee0ada7ba299ac32dda4a2c544b9fcf8c1` |
| stderr digest | `e4c6775194510285ec5acdf7a0debde93bf32a7223e4288fdd833aa18ae7d0a6` |
| Web smoke | `/` HTTP `200`, `/pcv-config.js` HTTP `200`, running guest exec cancel affordance installed |
| TUI smoke | `pcvtui --smoke-once vm`, VM row visible, guest execution affordance visible |
| Secret guard | token/password value observed `false`, credential ref redacted `true` |

## 경계

이 smoke는 기존 persistent Windows VHD target을 재사용했다. VM 생성, VM 삭제, credential 평문
입력, stdout/stderr 원문 캡처는 수행하지 않았다. `pcv-guest-installed-04253-r1`은 다음
evidence cycle까지 keep policy로 보존한다.
