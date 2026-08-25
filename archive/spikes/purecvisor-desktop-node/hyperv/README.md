# PureCVisor Desktop Node Hyper-V Helper Spike

이 spike는 Windows Hyper-V backend용 PowerShell helper 계약을 검증한다. Phase 1에서 시작했지만 현재 helper는 host diagnostics, VM inventory, read-only network inventory, ISO 기반 VM 생성, VM lifecycle, checkpoint operation, Phase 3B VM detail에 필요한 inventory field를 함께 제공한다.

2026-05-03 VM power-state/checkpoint/native lifecycle/delete adapter slices 이후 제품 .NET Host의 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete 실행 경로는 C# WMI adapter가 소유한다. 이 helper의 VM lifecycle/checkpoint 계약은 component/regression 검증용으로 남긴다. Native VM create product path는 이번 slice에서 Hyper-V Generation 2만 지원하고 native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다.

관련 문서:

- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle-design.md`
- `docs/superpowers/plans/2026-04-24-purecvisor-desktop-node-phase1-spike.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-power-state-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-mutation-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-restore-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-create-shutdown-restart-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-delete-native-adapter.md`
- `archive/spikes/purecvisor-desktop-node/api/README.md`

현재 상태: helper spike는 완료된 격리 실험 코드이며 Linux `purecvisorsd` runtime에 연결하지 않는다. Local API는 이 helper를 subprocess로 호출하고, Phase 3B Web Console은 Local API를 통해 VM detail/lifecycle job을 사용한다.

Phase 19 기준 Desktop Node는 제품 런타임으로 승격하지 않고 `archive/spikes/purecvisor-desktop-node/**` 격리 spike로 유지한다. 실제 Hyper-V lifecycle integration evidence는 GA 차단 gate로 남아 있으며, root 결정은 `archive/spikes/purecvisor-desktop-node/README.md`와 `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`를 따른다.

## 지원 호스트

- Windows 10 or Windows 11 Pro, Enterprise, or Education
- Hyper-V enabled
- PowerShell 7 available as `pwsh`
- Administrator shell for real Hyper-V operations

## Helper Operations

허용된 operation은 runner allowlist로 고정한다.

- `host.status`
- `network.inventory`
- `vm.list`
- `vm.create`
- `vm.start`
- `vm.shutdown`
- `vm.poweroff`
- `vm.restart`
- `checkpoint.list`
- `checkpoint.create`
- `checkpoint.restore`
- `checkpoint.delete`

`vm.list` inventory는 Phase 3B detail panel이 사용하는 `cpu.count`, `memory.startup_mb`, `memory.assigned_mb`, `generation`, `storage[].path`, `storage[].size_gb`, `storage[].attached`, `network[].switch`, `checkpoints.count`, `console.type`, `console.available_local`, `managed_by_purecvisor`를 포함한다.

`network.inventory`는 Phase 24 Local API job runtime boundary 후보의 read-only orchestration slice다. 이 operation은 `Get-VMSwitch`를 통해 Hyper-V switch 목록을 `name`, `type`, `is_default`, `allow_management_os`, `net_adapter_interface_description` 계약으로 반환하며, switch 생성/수정/삭제 cmdlet은 호출하지 않는다.

## Non-Integration Tests

실제 VM을 만들지 않고 contract/unit suite를 실행한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

현재 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

Host readiness는 optional-feature 조회가 불가능하거나 느린 환경에서 Hyper-V cmdlet 사용 가능 여부를 Hyper-V 사용 가능 신호로 취급한다. VM lookup은 로컬라이즈된 Hyper-V "not found" 오류도 host lookup failure가 아니라 VM 부재로 분류한다.

## Checkpoint Evidence Assessment

Phase 21 product-flow rerun에서 checkpoint create/list 불일치를 판정할 때는 `PcvHyperVEvidence.psm1`의 `Get-PcvPhase21CheckpointEvidenceAssessment`를 사용한다. 이 helper는 evidence artifact에 `checkpoint_job_result`, `checkpoint_list_response`, `direct_snapshots`가 모두 있는지 확인한 뒤 checkpoint 이름이 job result, Product API list response, direct `Get-VMSnapshot` 결과에 모두 보일 때만 `verified_visible`로 분류한다.

2026-04-30 이전 smoke처럼 `checkpoint_status = succeeded`, `checkpoint_list_contains_name = false`만 남은 artifact는 checkpoint 유실 증거가 아니라 `inconclusive_missing_raw_evidence` / `evidence_capture_incomplete`로 기록한다.

## Manual Host Status

실행:

```powershell
Get-Content archive/spikes/purecvisor-desktop-node/hyperv/examples/host-status.json -Raw |
  pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1
```

예상 출력은 `ok=true`, `operation=host.status`, `data.hyperv` object를 포함하는 compact JSON object다.

## Gated Hyper-V Integration Test

Integration suite는 기본 제외한다. 지원되는 Hyper-V host의 관리자 PowerShell에서, local Linux ISO가 준비된 경우에만 실행한다.

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
$env:PCV_HYPERV_TEST_ISO='D:\iso\ubuntu-24.04-live-server-amd64.iso'
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1' -Tag Integration -Output Detailed"
```

`PCV_HYPERV_TEST_ISO`는 존재하는 local ISO file을 가리켜야 한다. 테스트는 임시 `pcv-spike-*` VM을 만들고 `host.status`, `vm.create`, `vm.list`, `vm.start`, `checkpoint.create`, `vm.poweroff`를 실행한 뒤, ownership check가 통과하면 VM과 임시 VM directory를 정리한다.

Gated integration test를 포함한 전체 Hyper-V helper suite는 같은 환경 변수를 유지한 상태에서 전체 테스트 디렉터리를 `-ExcludeTag Integration` 없이 실행한다:

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
$env:PCV_HYPERV_TEST_ISO='D:\iso\ubuntu-24.04-live-server-amd64.iso'
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/hyperv/tests' -Output Detailed"
```

준비된 관리자 Hyper-V 호스트 기준 전체 suite 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.
