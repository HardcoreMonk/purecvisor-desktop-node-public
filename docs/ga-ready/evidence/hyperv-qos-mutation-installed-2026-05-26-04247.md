# Hyper-V QoS Mutation Installed Evidence 2026-05-26 0.42.47

evidence_id: `hyperv-qos-mutation-installed-2026-05-26-04247`
result: `PASS_INSTALLED_WITH_MANUAL_ADMIN_CLOSED`
scope: `phase2-hyperv-qos-mutation-installed-package-fullgate-actual-vm-smoke`
adr: `docs/adr/0008-hyperv-qos-mutation-policy.md`
code_level_evidence: `docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`
package_version: `0.42.47-admin-smoke`
package_artifact_root: `artifacts/admin-smoke-package-20260526-04247`
package_msi_sha256: `9589086d092ee902b72ff7790cac5a25e6d806cdaac0d98e431a27048dc5e197`
package_payload_aggregate_sha256: `b206399efff98c9abf598580051ee9b81d87cc8450c4991de7d1944dafbb4aac`
package_product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
package_host_exe_sha256: `4adfc1acca292430c38afe3b447effd4107cc7b22ff42db7e1b65ced32bea92d`
package_cli_sha256: `b8b335313e4e847240badd87584d49f0858e9e1f02e5b51466f3af0e51677def`
package_tui_sha256: `f428fcfe9bf9a1b93dedf76ad3c891eef751ce629da007ba12909cfa9b1a75e6`
package_provenance_commit: `77f1a3f291b4f736218cb5110dcecd3b464860d4`
full_admin_host_mutation_gate: `PASS`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260526-04247`
full_admin_host_mutation_summary: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04247/summary.json`
installed_actual_vm_smoke: `PASS`
installed_actual_vm_smoke_artifact: `artifacts/installed-cli-qos-mutation-smoke-20260526-04247/summary.json`
manual_admin_package_pair: `closed-0.42.45-to-0.42.47`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`
installed_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04247.md`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결론

Phase 2 Hyper-V QoS mutation은 0.42.47 설치본 기준으로 package build, full admin host
mutation gate, 실제 VM 대상 PCVCLI storage/network QoS mutation smoke, manual-admin
package-pair closure를 PASS했다.
Storage QoS는 `pcvcli vm blkio-set`, network QoS는 `pcvcli vm bandwidth-set`으로
dry-run, queued apply, rollback restore까지 확인했다.

`0.42.45-admin-smoke -> 0.42.47-admin-smoke` manual-admin package-pair가
`manual-admin-campaign-descriptor-20260526-04245-04247-closed`로 닫혔으므로 최신 운영
anchor는 0.42.47 fullgate/manual-admin/current-card closure가 소유한다.

## 설치본 검증

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| Package build | `PASS` | `artifacts/admin-smoke-package-20260526-04247` |
| Full admin host mutation gate | `PASS` | `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04247/summary.json` |
| Service/MSI/Hyper-V route parity step | `PASS` | `service-msi-hyperv-admin-smoke`, exit code `0` |
| OS mutation gate step | `PASS` | `os-mutation-gate`, exit code `0` |
| Installed PCVCLI actual VM smoke | `PASS` | `artifacts/installed-cli-qos-mutation-smoke-20260526-04247/summary.json` |
| Token/password redaction check | `PASS` | `token_value_observed=false`, `password_value_observed=false` |
| Cleanup | `PASS` | `cleanup.vm_removed=true`, `cleanup.vm_root_removed=true` |

## Actual VM QoS Smoke

실제 VM smoke는 설치 경로의
`C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`를 호출했다. 대상 VM은
`pcv-cli-qos-mut-fa3483ba`였고, smoke 종료 후 VM과 임시 VM root를 제거했다.

확인한 명령 범위는 아래와 같다.

| 단계 | 명령 | 기대 결과 |
| --- | --- | --- |
| Host readiness | `pcvcli --json host status` | `host.status` PASS |
| VM 생성 | `pcvcli --json vm create ...` | queued job PASS |
| VM 조회 | `pcvcli --json vm list`, `pcvcli --json vm get <vm>` | 실제 VM 확인 |
| Storage preview | `pcvcli --json vm blkio-set <vm> --disk disk0 --maximum-iops 120 --minimum-iops 0 --dry-run` | `vm.qos.storage.preview` PASS, host mutation 없음 |
| Network preview | `pcvcli --json vm bandwidth-set <vm> --adapter adapter0 --maximum-kbps 20480 --minimum-kbps 0 --dry-run` | `vm.qos.network.preview` PASS, host mutation 없음 |
| Storage apply | `pcvcli --json vm blkio-set <vm> --disk disk0 --maximum-iops 120 --minimum-iops 0 --yes` | queued apply PASS |
| Network apply | `pcvcli --json vm bandwidth-set <vm> --adapter adapter0 --maximum-kbps 20480 --minimum-kbps 0 --yes` | queued apply PASS |
| Storage rollback | `pcvcli --json vm blkio-set <vm> --disk disk0 --maximum-iops 0 --minimum-iops 0 --yes` | restore PASS |
| Network rollback | `pcvcli --json vm bandwidth-set <vm> --adapter adapter0 --maximum-kbps 0 --minimum-kbps 0 --yes` | restore PASS |
| Cleanup | `pcvcli --json vm delete <vm> --yes` | queued delete PASS |

## 0.42.46 Superseded Diagnostic

0.42.46 설치본 smoke는 storage QoS dry-run/apply는 통과했지만, 기본 VM network port에
`Msvm_EthernetSwitchPortBandwidthSettingData` feature가 없을 때 network apply가
`PCV_VM_NETWORK_QOS_TARGET_NOT_FOUND`로 실패했다. 이 실패는 0.42.47 fix commit
`77f1a3f291b4f736218cb5110dcecd3b464860d4`에서 network port를 별도로 찾고,
bandwidth feature가 없으면 `Msvm_VirtualSystemManagementService.AddFeatureSettings`로
새 feature setting을 추가하도록 수정해 supersede했다.

0.42.46 artifact는 diagnostic predecessor로만 보존한다.

- package artifact: `artifacts/admin-smoke-package-20260526-04246`
- rerun smoke artifact: `artifacts/installed-cli-qos-mutation-smoke-20260526-04246-rerun/summary.json`
- superseded reason: `network-bandwidth-feature-missing-before-add-feature-settings-fix`

## Manual-admin Closure

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| Package pair | `PASS` | `0.42.45-admin-smoke -> 0.42.47-admin-smoke` |
| Descriptor | `PASS` | `manual-admin-campaign-descriptor-20260526-04245-04247-closed`, `missing_count=0`, `not_pass_count=0` |
| Clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20260526-04245-04247/clean-host-windows-update-corrected/summary.json` |
| Burn/MSIX/runtime ops | `PASS` | `artifacts/manual-admin-campaign-20260526-04245-04247/burn-bootstrapper-lifecycle/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260526-04245-04247/summary.json`, installed runtime ops summary |
| Installed current-card | `PASS` | `artifacts/installed-operator-surface-current-card-20260526-04247/summary.json` |

Public trusted signing, external stable publication, public GA 배포는 여전히 claim하지 않는다.
