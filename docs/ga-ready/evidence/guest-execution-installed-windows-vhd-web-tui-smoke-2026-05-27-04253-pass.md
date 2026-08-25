# Guest Execution Installed Windows VHD Web/TUI Smoke 2026-05-27 0.42.53 PASS

evidence_id: `guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass`
result: `PASS_CREDENTIALED_WINDOWS_GUEST_EXECUTION`
scope: `installed-windows-vhd-guest-exec-channel-web-tui-smoke`
version: `0.42.53-admin-smoke`
artifact_root: `artifacts/guest-execution-installed-windows-vhd-smoke-20260527-04253-r1`
summary: `artifacts/guest-execution-installed-windows-vhd-smoke-20260527-04253-r1/summary.json`
surface_summary: `artifacts/guest-execution-installed-windows-vhd-smoke-20260527-04253-r1/surface-smoke-summary.json`
vm_name: `pcv-guest-installed-04253-r1`
base_vhd_path: `artifacts/image-cache/windows-server-2022-eval-vhd/20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd`
differencing_vhd_path: `D:\PureCVisor\SmokeVMs\pcv-guest-installed-04253-r1\pcv-guest-installed-04253-r1.vhd`
guest_os: `Microsoft Windows Server 2022 Datacenter Evaluation`
guest_transport: `windows-powershell-direct`
credential_ref_type: `dpapi-local-machine`
credential_ref: `dpapi:<protected-file>`
credential_file_path: `C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi`
credential_file_acl: `administrators-system-full-control`
channel_verify_job_id: `job-6290745d8e9a416487a864a4f5bea169`
channel_verify_status: `succeeded`
guest_exec_job_id: `job-1626457b2af94dd9b8eff2e1109ad99d`
guest_exec_status: `succeeded`
guest_exec_terminal_state: `succeeded`
web_listener_status: `200`
tui_smoke_status: `pass-vm-row-visible`
tui_vm_count: `1`
host_mutation_performed: `true-persistent-installed-windows-vhd-created-and-left-running`
persistent_vm_left_running: `true`
token_value_observed: `false`
password_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

```powershell
pcvcli --json vm get pcv-guest-installed-04253-r1
pcvcli --json vm guest-agent-ensure-channel pcv-guest-installed-04253-r1 `
  --verify `
  --credential-ref dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi `
  --timeout-sec 60
pcvcli --json vm guest-exec pcv-guest-installed-04253-r1 `
  --credential-ref dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi `
  --timeout-sec 60 `
  -- hostname
pcvcli --json job get job-6290745d8e9a416487a864a4f5bea169
pcvcli --json job get job-1626457b2af94dd9b8eff2e1109ad99d
pcvtui --smoke-once vm
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1/
```

## 결과

- 기존 clean-host Windows Server 2022 eval base VHD에서 differencing VHD를 만들고
  `pcv-guest-installed-04253-r1` Hyper-V VM을 persistent target으로 부팅했다.
- PowerShell Direct probe는 guest OS `Microsoft Windows Server 2022 Datacenter Evaluation`,
  guest user `Administrator`, PowerShell `5.1.20348.1`을 반환했다.
- Guest credential은 raw password CLI argument나 repository file로 남기지 않고
  `DataProtectionScope.LocalMachine` DPAPI protected file에 저장했다. ACL은
  Administrators/SYSTEM full control로 제한했고 evidence에는 `dpapi:<protected-file>`로만 기록한다.
- `vm guest-agent-ensure-channel --verify` job `job-6290745d8e9a416487a864a4f5bea169`는
  `windows-powershell-direct` transport에서 `succeeded`가 됐다.
- `vm guest-exec ... -- hostname` job `job-1626457b2af94dd9b8eff2e1109ad99d`는
  `windows-powershell-direct` transport에서 `terminal_state=succeeded`가 됐다.
- Job result는 `guest-execution-audit-v1`, stdout/stderr byte count, digest를 남겼고 raw
  credential secret은 출력하지 않았다.
- Web listener는 HTTP `200`으로 응답했다.
- `pcvtui --smoke-once vm`은 `vm-count=1`, selected VM
  `pcv-guest-installed-04253-r1`, `Guest Execution: E queue exec C verify channel`,
  QoS direct-control affordance를 표시했다.

## 경계

이 evidence는 internal admin-smoke host mutation이다. Public trusted signing, external
stable publication, winget submission, public stable installer URL, public signed clean-host
smoke를 주장하지 않는다.

ISO boot-shell evidence
`guest-execution-actual-vm-web-tui-smoke-2026-05-27-04253-blocked`는 historical
predecessor로 유지한다. 해당 evidence는 Windows ISO attach/start/readback/poweroff/delete
cleanup만 PASS했고 installed Windows guest credentialed command execution은 이 문서가 닫는다.
