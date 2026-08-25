# Web/TUI QoS/guest readback actual VM 2026-05-21 0.42.40

evidence_id: `web-tui-qos-guest-readback-actual-vm-2026-05-21-04240`
result: `PASS_WITH_TUI_INSTALLED_BLOCKER_AND_04241_TRIGGER`
scope: `actual-vm-web-tui-qos-guest-readback-ux`
artifact_root: `artifacts/web-tui-qos-guest-readback-actual-vm-20260521-04240`
summary: `artifacts/web-tui-qos-guest-readback-actual-vm-20260521-04240/summary.json`
installed_version: `0.42.40-admin-smoke`
source_fix_trigger_version: `0.42.41-admin-smoke`
actual_vm_name: `pcv-ux-qos-04240`
actual_vm_iso: `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`
host_mutation_performed: `true`
cleanup_status: `pass`
vm_still_present_after_cleanup: `false`
vm_root_removed_after_cleanup: `true`
web_actual_vm_qos_guest_panel: `pass`
web_no_overlap_check: `pass`
web_overlap_failure_count: `0`
web_text_fit_failure_count: `0`
installed_cli_actual_vm_readback: `pass`
installed_tui_actual_vm_row_projection: `blocked-04240`
source_tui_row_projection_fix: `pass-code-level`
source_tui_fix_files: `src/DesktopNode.Tui/TuiPoller.cs`, `src/DesktopNode.Tui.Tests/TuiStateTests.cs`
package_chain_trigger: `0.42.41-admin-smoke-required-for-installed-TUI-row-projection-fix`
token_value_observed: `false`
password_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 실제 Hyper-V VM을 생성해 Web/TUI QoS/guest readback surface를 확인한 기록이다.
Web Console은 실제 VM `pcv-ux-qos-04240`의 selected VM detail에서 read-only
`QoS / Guest Readback` panel을 표시했고, DOM 측정은 card overlap/text-fit failure `0`을
기록했다. 설치본 CLI/API readback도 `vm.blkio-get`, `vm.bandwidth`,
`vm.guest-agent-status`, `vm.guest-ping`에서 Hyper-V 전용 false/compatibility flag를
반환했다.

## 실행 결과

| 항목 | 결과 |
| --- | --- |
| VM create/start/readback | PASS, `summary.pre-web.json` |
| Web authenticated capture | PASS, `web-capture/summary.json` |
| Web focused QoS panel capture | PASS, `web-qos-panel/summary.json` |
| Installed TUI `pcvtui --smoke-once vm` | BLOCKED: API body에는 VM이 있으나 table row는 `(no rows)` |
| Source TUI after fix | PASS: `pcv-ux-qos-04240`, `running`, `1 vCPU`, `1024 MB` row 표시 |
| Cleanup | PASS, VM 삭제 및 temp VM root 제거 |

## Web UX evidence

| 화면 | 파일 | SHA-256 |
| --- | --- | --- |
| VM detail | `artifacts/web-tui-qos-guest-readback-actual-vm-20260521-04240/web-capture/vm-detail.png` | `17134db0f67f3864cfb976fb1cbb0e5c592d3443efa70ee8e6e452008601e101` |
| QoS panel desktop tall | `artifacts/web-tui-qos-guest-readback-actual-vm-20260521-04240/web-qos-panel/qos-panel-desktop-tall.png` | `79c65d32a921df411e86ff0f1b1940e6160d90adadaaeff17bfb80934b8dda38` |
| QoS panel mobile | `artifacts/web-tui-qos-guest-readback-actual-vm-20260521-04240/web-qos-panel/qos-panel-mobile.png` | `72822989c221288a457e9db975b9da5b150ea3fec3951514c045a7b09ec1d656` |

`web-qos-panel/summary.json`는 `linux_blkio_compatible=false`,
`linux_bandwidth_compatible=false`, `qemu_guest_agent=false`,
`guest_heartbeat_verified=false`, `overlap_failures=0`, `text_fit_failures=0`을 기록한다.

## TUI blocker와 fix

설치본 `0.42.40-admin-smoke`의 `pcvtui --smoke-once vm`은 API route body에
`pcv-ux-qos-04240` VM data를 받았지만 table row projection을 하지 않아 `(no rows)`를
렌더링했다. 원인은 `TuiPoller`가 성공 snapshot body만 저장하고 `vm.list` data envelope를
`TuiState.Rows`로 투영하지 않는 것이었다.

이번 branch는 TDD로 `PollerSuccessProjectsVmListDataEnvelopeIntoRows`를 먼저 실패시킨 뒤,
`TuiPoller`에 탭별 row projection을 추가했다. Source TUI smoke
`pcvtui-source-smoke-vm-after-row-projection-fix.stdout.txt`는 실제 VM row를 표시한다.
설치본 반영에는 새 product payload package `0.42.41-admin-smoke`가 필요하다.

## 경계

이 evidence는 internal admin-smoke 실제 VM UX/readback evidence다. Linux cgroup QoS,
libvirt blkio mutation, qemu guest agent 호환, public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
