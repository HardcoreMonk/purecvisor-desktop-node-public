# PureCVisor Desktop Node Phase 21 Hyper-V Lifecycle Integration Evidence 설계

## 목적

Phase 21은 Phase 19에서 남은 GA 차단 gate 중 실제 Hyper-V host VM lifecycle integration evidence를 수집하기 위한 실행 경계를 정의한다.

이 단계는 Desktop Node를 GA 제품 런타임으로 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지한다. Phase 21의 목표는 signed/elevated product install 흐름에서 Local API, job store, Hyper-V helper가 실제 Windows Hyper-V host의 VM create/start/poweroff/checkpoint/remove lifecycle을 일관되게 처리한다는 증거 기준을 정의하는 것이다.

## 현재 결정

```text
PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
DESKTOP_NODE_PHASE21_HYPERV_LIFECYCLE_EVIDENCE: pending-admin-opt-in
```

Phase 21은 evidence gate다. 문서 작성만으로 gate를 닫지 않는다. 실제 Hyper-V host mutation은 관리자 opt-in, 명시적 테스트 VM prefix, cleanup checklist가 준비된 경우에만 실행한다.

## 범위

Phase 21에 포함한다.

- 지원 Windows Hyper-V host capability 확인 기준
- signed/elevated product install flow와 Hyper-V lifecycle evidence의 연결 기준
- Local API job route를 통한 VM create/start/poweroff/checkpoint/remove smoke 기준
- failure interruption 후 retry와 job store consistency evidence 기준
- Hyper-V helper 직접 integration suite와 product service/API integration evidence의 역할 분리
- test VM name prefix, ownership marker, VM root, checkpoint naming, cleanup checklist
- evidence redaction과 문서화 기준
- 기본 non-integration validation command 목록

Phase 21에서 제외한다.

- 실제 Hyper-V VM command를 기본 검증으로 실행
- 관리자 권한 요구를 문서 작성 또는 일반 검증의 전제로 설정
- signed release build 또는 elevated MSI lifecycle gate 자체를 닫는 작업
- Hyper-V helper, Local API, product wrapper의 대규모 runtime redesign
- 단, 실제 evidence 신뢰도를 높이기 위한 read-after-write 검증과 non-integration test 보강은 evidence-hardening 후속으로 허용한다. 이 보강은 GA 승격이나 spike 경계 해제를 의미하지 않는다.
- 개별 Phase 21 evidence 기록 중 shared index에 pass count를 복제하는 작업
- release/version policy 확정
- Windows Event Log writer/provider 전환
- Desktop Node GA 제품 런타임 승격
- Linux `purecvisor-single`, Linux `purecvisorsd`, Single Edge UI/API 변경

## 증거 모델

Phase 21 evidence는 세 층으로 분리한다.

### 1. Non-integration contract evidence

관리자 권한과 실제 Hyper-V VM mutation 없이 실행하는 기본 검증이다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

이 검증은 helper allowlist, host status shape, VM inventory shape, lifecycle/checkpoint structured failure, provisioning cleanup contract, root documentation guard를 확인한다. 이 검증만으로 실제 Hyper-V lifecycle gate를 충족 처리하지 않는다.

### 2. Helper integration evidence

지원 Hyper-V host의 elevated PowerShell에서만 실행한다. 기존 integration suite는 `pcv-spike-*` VM을 만들고 helper runner로 `host.status`, `vm.create`, `vm.list`, `vm.start`, `checkpoint.create`, `vm.poweroff`를 실행한 뒤 ownership marker 또는 VM path가 맞을 때만 cleanup한다.

이 층은 Hyper-V helper가 실제 host에서 동작한다는 증거다. 하지만 product service install flow, protected token, Local API job persistence까지 묶이지 않으므로 Phase 21 gate의 일부 증거로만 인정한다.

### 3. Product service/API lifecycle evidence

Phase 21 gate를 닫으려면 signed/elevated product install 흐름과 연결된 Local API route를 통해 같은 lifecycle을 확인해야 한다. Phase 20의 signed/elevated MSI evidence가 준비된 같은 artifact 또는 명시적으로 기록된 product install 상태에서 다음 순서를 검증한다.

1. protected token source로 product service가 running 상태인지 확인한다.
2. `GET /api/v1/runtime/policy`가 HTTP 200과 `token_storage = dpapi-local-machine`을 반환한다.
3. `GET /api/v1/host/status`가 Hyper-V enabled, VMMS running, supported host state를 반환한다.
4. 명시적 prefix를 가진 테스트 VM을 `POST /api/v1/vms`로 생성 job에 넣는다.
5. `GET /api/v1/jobs/{job_id}`로 create job이 `succeeded`가 될 때까지 확인한다.
6. `GET /api/v1/vms` 또는 `GET /api/v1/vms/{id}`에서 테스트 VM이 inventory에 나타나는지 확인한다.
7. `POST /api/v1/vms/{id}/start` lifecycle job을 제출하고 `succeeded`를 확인한다.
8. `POST /api/v1/vms/{id}/checkpoints`로 checkpoint create job을 제출하고 `succeeded`를 확인한다.
9. `GET /api/v1/vms/{id}/checkpoints` raw response와 direct `Get-VMSnapshot -VMName <test-vm>` 결과를 redaction 후 evidence에 남기고, 두 경로 모두에서 checkpoint가 나타나는지 확인한다.
10. `POST /api/v1/vms/{id}/poweroff` lifecycle job을 제출하고 `succeeded`를 확인한다.
11. 테스트 VM remove는 현재 Local API endpoint가 아니라 명시적 cleanup checklist로 수행한다. VM name prefix와 ownership marker/path가 일치할 때만 삭제한다.

## Failure, Retry, Job Store Consistency 기준

Phase 21 gate는 정상 lifecycle만으로 닫지 않는다. 실패, 중단, 재실행, job store 일관성 evidence가 필요하다.

충족 기준:

- failed job은 원본 job record가 유지되고, retry job은 새 `job_id`와 `retry_of`를 가진다.
- retry attempt는 기존 Phase 2E/9 정책과 같이 제한을 지킨다.
- service restart 또는 listener interruption 이후 persisted job store를 다시 읽을 수 있다.
- 이미 `succeeded` 또는 `failed`로 종료된 job이 재시작 후 임의로 `running`으로 남지 않는다.
- failed create 후 cleanup 상태가 evidence에 남는다.
- corrupt job store가 발생한 경우 quarantine 동작을 기록하고, silent data loss로 gate를 닫지 않는다.
- 실패 로그에는 raw token, protected token blob, private key, VM guest secret이 남지 않는다.

실패 유도는 실제 host를 손상하지 않는 방식으로만 수행한다. 예를 들어 존재하지 않는 ISO path, invalid create params, 이미 존재하는 test VM name 같은 입력 실패를 우선 사용한다. running helper process kill, service kill, host reboot 같은 강한 interruption은 별도 관리자 opt-in과 cleanup plan 없이는 실행하지 않는다.

## 안전 제약

실제 Hyper-V host mutation을 실행할 때 다음 제약을 반드시 지킨다.

- 테스트 VM 이름은 명시적 prefix를 사용한다. 기본 prefix는 `pcv-phase21-`이다.
- 기존 사용자의 VM, prefix가 다른 VM, ownership marker가 없는 VM은 삭제하지 않는다.
- cleanup 전 `Get-VM -Name '<test-vm>'` 결과, Notes ownership marker, VM path 또는 configuration location을 확인한다.
- VM root는 전용 test directory를 사용하고, cleanup은 해당 directory 아래로 제한한다.
- checkpoint 이름은 `pcv-phase21-before-poweroff`처럼 테스트 전용 이름을 사용한다.
- admin opt-in 없이는 elevated PowerShell, `msiexec`, `Get-VM`, `New-VM`, `Start-VM`, `Stop-VM`, `Checkpoint-VM`, `Remove-VM`을 실행하지 않는다.
- ISO path, VM root, evidence directory는 실행 전에 명시적으로 기록한다.
- cleanup이 실패하면 남은 VM name, state, checkpoint list, VM root path를 redaction 후 기록하고 자동 삭제를 반복하지 않는다.

## Signed/Elevated Product Install Flow 연결

Phase 21은 Phase 20 gate를 대체하지 않는다. signed/elevated product install flow와 Hyper-V lifecycle evidence는 다음 방식으로 연결한다.

- Phase 20 signed MSI artifact 또는 그와 동일한 provenance를 가진 installed product를 사용한다.
- product service는 protected token file을 통해 Local API를 실행해야 한다.
- Local API route 호출은 bearer token을 사용하되, evidence에는 token 값을 기록하지 않는다.
- product wrapper status, runtime policy, diagnostic bundle, job store path를 함께 기록한다.
- Phase 20이 아직 pending이면 Phase 21 runbook은 준비 상태로 남고, 실제 product-flow evidence는 pending으로 둔다.

## 문서화 기준

실행 결과는 Phase 21 plan의 `완료 증거`에만 기록한다. high-level docs와 index에는 pass count를 복제하지 않는다.

기록해야 하는 항목:

- 실행 날짜와 host 요약
- Windows edition/version, PowerShell version
- Hyper-V feature/cmdlet/VMMS/default switch 상태
- product install artifact 또는 installed version/provenance 요약
- test VM name, VM root, checkpoint name
- lifecycle command 또는 API route sequence
- job id, final status, retry relationship, sanitized error code
- cleanup 결과
- redaction 확인

기록하지 않는 항목:

- raw API token
- protected token blob
- signing secret, PFX password, private key
- guest OS password, SSH key, cloud-init secret
- 전체 certificate private material

## 완료 기준

Phase 21 문서 시작 작업은 다음을 만족하면 완료다.

- Phase 21 spec과 plan/runbook이 존재한다.
- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 유지가 명시되어 있다.
- 실제 Hyper-V host mutation과 administrator action은 opt-in gate로 분리되어 있다.
- default validation은 non-integration Pester와 root docs suite로 남아 있다.
- shared index에는 진입점만 두고, evidence pass count는 이 Phase plan에만 둔다.

Phase 21 evidence gate 자체는 다음을 만족해야 닫힌다.

- 지원 Hyper-V host에서 create/start/poweroff/checkpoint/remove lifecycle evidence가 기록된다.
- signed/elevated product install flow와 연결된 Local API job evidence가 기록된다.
- failure/retry/job-store consistency evidence가 기록된다.
- cleanup checklist가 완료되거나 남은 리소스가 명확히 기록된다.
- evidence에 secret, raw token, protected token blob, private key가 포함되지 않는다.
