# Guest Execution Actual VM Web/TUI Smoke 2026-05-27 0.42.53 Blocked

evidence_id: `guest-execution-actual-vm-web-tui-smoke-2026-05-27-04253-blocked`
result: `BLOCKED_BY_MISSING_PROTECTED_GUEST_CREDENTIAL_AND_EMPTY_VM_INVENTORY`
scope: `actual-vm-guest-exec-web-tui-direct-control-smoke`
version: `0.42.53-admin-smoke`
installed_cli: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
installed_tui: `C:\Program Files\PureCVisor\DesktopNode\pcvtui.exe`
vm_inventory: `empty`
credential_inventory: `no-purecvisor-guest-credential-target`
windows_install_media_inventory: `found`
windows_install_media_path: `D:\Downloads\Windows.iso`
windows_install_media_size_bytes: `4713021440`
windows_iso_boot_shell_smoke: `pass-create-start-readback-poweroff-delete-cleaned-up`
windows_iso_boot_shell_artifact_root: `artifacts/guest-execution-windows-iso-boot-shell-smoke-20260527-04253-r1`
windows_iso_boot_shell_vm_name: `pcv-guest-winiso-04253-r1`
windows_iso_boot_shell_create_job_id: `job-e5ed7403a26a496baf0d481fee1c608c`
windows_iso_boot_shell_start_job_id: `job-9f0d339612994e39850df0085757814f`
windows_iso_boot_shell_poweroff_job_id: `job-39a20d0867ee4357941f2f534d800d7d`
windows_iso_boot_shell_delete_job_id: `job-1bc7df7c12284b9c89ad3c37ec39a26a`
windows_iso_boot_shell_cleanup: `vm-removed-vm-root-removed-final-inventory-empty`
available_iso_inventory: `Comanche 4.iso, Rocky-10.1-x86_64-dvd1.iso, Rocky-10.1-x86_64-minimal.iso, Windows.iso`
web_listener: `http://127.0.0.1/`
web_listener_status: `200`
tui_vm_smoke: `pass-empty-inventory-no-selected-vm`
guest_exec_target_vm: `pcv-guest-winiso-04253-r1`
guest_exec_job_id: `job-b2182509e138466eb45b7b5404f339cf`
guest_channel_verify_job_id: `job-b858de5e7fd7498a9f59a6573a19877a`
guest_exec_terminal_status: `failed-before-guest-mutation`
guest_exec_error_code: `PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED`
audit_schema: `guest-execution-audit-v1`
redaction_policy: `guest-execution-redaction-v1`
host_mutation_performed: `true-windows-iso-boot-shell-create-start-poweroff-delete-cleanup-plus-guest-exec-credential-blocked`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
successor_evidence: `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass.md`
successor_status: `pass-installed-windows-vhd-credentialed-guest-exec`

## 실행

```powershell
pcvcli --json runtime policy
pcvcli --json vm list
Get-CimInstance -Namespace root\virtualization\v2 -ClassName Msvm_ComputerSystem
cmdkey /list | Select-String -Pattern "PureCVisor|pcv|guest"
Get-ChildItem -Path D:\Downloads,C:\Users\Operator\Downloads,D:\ISO,D:\isos,D:\data\iso,D:\data\downloads,C:\ISO,C:\Users\Public\Downloads -Filter *.iso
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledCliQosGuestSmoke.ps1 `
  -ArtifactRoot artifacts/guest-execution-windows-iso-boot-shell-smoke-20260527-04253-r1 `
  -IsoPath D:\Downloads\Windows.iso `
  -VmRoot D:\PureCVisor\Temp\guest-execution-windows-iso-boot-shell-smoke-20260527-04253-r1 `
  -VmName pcv-guest-winiso-04253-r1 `
  -JobTimeoutSeconds 300 `
  -CommandTimeoutSeconds 180
pcvcli --json vm guest-exec pcv-guest-winiso-04253-r1 --credential-ref wincred:PureCVisor/guest/admin --timeout-sec 5 -- powershell -NoProfile -Command hostname
pcvcli --json vm guest-agent-ensure-channel pcv-guest-winiso-04253-r1 --verify --credential-ref wincred:PureCVisor/guest/admin
pcvtui --smoke-once vm
Invoke-WebRequest -UseBasicParsing -Uri http://127.0.0.1/
```

## 결과

- Runtime policy는 Guest Execution provider route가 enabled임을 보고했다.
- `pcvcli --json vm list`와 Hyper-V WMI inventory 모두 VM을 반환하지 않았다.
- Credential Manager에는 `PureCVisor`/`pcv`/`guest` 계열 guest credential target이 없었다.
- ISO inventory에는 `D:\Downloads\Windows.iso`가 존재한다. 파일 크기는 `4713021440` bytes다.
- Windows ISO boot shell smoke는 `pcv-guest-winiso-04253-r1`을 생성하고 `D:\Downloads\Windows.iso`를 attach한 뒤
  start/readback/poweroff/delete cleanup까지 PASS했다. Artifact는
  `artifacts/guest-execution-windows-iso-boot-shell-smoke-20260527-04253-r1/summary.json`이다.
- 이 smoke는 Windows 설치 완료나 guest credential 생성이 아니다. 최종 `pcvcli --json vm list`와 Hyper-V WMI inventory는
  다시 empty로 돌아왔다.
- actual `guest-exec` queue는 job `job-b2182509e138466eb45b7b5404f339cf`를 만들었지만 provider 실행 전에
  `PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED`로 failed가 됐다.
- `guest-agent-ensure-channel --verify` queue는 job `job-b858de5e7fd7498a9f59a6573a19877a`를 만들었지만
  같은 protected credential reference 부재에서 failed가 됐다.
- Job payload는 `guest-execution-audit-v1`, `guest-execution-redaction-v1`, command hash,
  `[redacted-ref]` credential reference를 남겼고 raw credential secret은 출력하지 않았다.
- TUI `--smoke-once vm`은 API reachable, `vm-count=0`, `(no rows)`, selected VM 없음,
  Guest Execution affordance 노출을 확인했다.
- Web listener는 HTTP `200`으로 응답했다. 현재 served HTML은 정적 demo asset row를 포함하지만
  actual VM inventory는 API 기준 empty이므로 Web direct-control actual VM smoke PASS로 claim하지 않는다.

## 판정

이 evidence 자체는 actual Windows guest command execution PASS가 아니다. Windows install media boot shell은 제품
VM create/start 경로로 검증했지만, unattended Windows 설치와 guest credential target 등록은 이 문서에서 닫지 않았다.

후속 `guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass` evidence가 기존 clean-host
Windows Server 2022 eval base VHD 기반 persistent Windows guest, DPAPI LocalMachine protected credential reference,
`vm guest-agent-ensure-channel --verify`, `vm guest-exec -- hostname`, Web listener, TUI selected VM row smoke를
PASS로 닫았다. 이 문서는 ISO boot-shell predecessor blocker로 보존한다.
