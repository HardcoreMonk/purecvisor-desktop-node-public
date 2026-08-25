# ADR-0001: 독립 Windows 저장소와 evidence-first keep-spike

- 상태: 적용 중
- 날짜: 2026-04-29
- 결정 마커:
  - `DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`
  - `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`
  - `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`
  - `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`
  - `DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter`
  - `DESKTOP_NODE_PHASE25_NATIVE_VM_POWER_STATE_MUTATION_START: vm-start-poweroff-dotnet-native-adapter`
  - `DESKTOP_NODE_PHASE25_NATIVE_CHECKPOINT_MUTATION_START: checkpoint-create-restore-delete-dotnet-native-adapter`
  - `DESKTOP_NODE_PHASE25_NATIVE_VM_LIFECYCLE_MUTATION_START: vm-create-shutdown-restart-dotnet-native-adapter`

## 맥락

`purecvisor-desktop-node`는 Windows Desktop Node 전용 저장소다. Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN Single Edge runtime은 이 저장소의 구현 대상이 아니다.

Phase 12-18은 Service-first wrapper, WinSW service host, WiX MSI-first installer, DPAPI LocalMachine protected token file, JSONL first diagnostics, LAN preview security policy, manifest-first update/rollback/config migration을 추가했다.

최근 결정과 구현 상태:

- Phase 19는 이 증거를 다시 평가했지만 GA 제품 런타임 승격에 필요한 release evidence는 아직 닫히지 않았다.
- Phase 22는 release/version policy와 installer artifact/channel contract를 일부 반영했고, ADR-0002가 이 좁은 정책을 현재 적용 결정으로 채택했다.
- ADR-0003은 내부 서비스 운영 제약에 맞춰 public CA 없이 internal root/leaf `RequireSigned` signing model을 채택했다.
- 2026-05-01 Phase 25 replacement slice는 기본 제품 service host와 MSI installed action runner를 WinSW/PowerShell entrypoint에서 `DesktopNode.Host.exe`로 교체했다.
- Route parity 시작 slice는 .NET request processor에 helper-backed routes와 queued job runtime을 추가했다.
- 2026-05-02에는 `host.status` read route가 C# registry/WMI/service/admin native adapter로 전환됐다.
- `network.inventory` read route는 C# native WMI adapter가 직접 처리하며 topology parity가 불완전하면 native structured failure를 반환한다.
- 2026-05-03에는 `vm.list`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints` read route가 native product path로 전환됐다. Native VM inventory 또는 checkpoint parity가 부족할 때는 helper fallback 없이 native structured failure를 반환한다.
- 같은 날 Web Console browser fixture parity code-level slice가 served `app.js`를 Node `vm` 최소 DOM과 fixture Local API 응답으로 실행해 dashboard/VM/job 렌더링을 검증한다.
- Mutation route owner contract는 served route 단위로 정리됐다. 2026-05-03 VM power-state/checkpoint/native lifecycle/delete adapter slices 이후 current served Hyper-V mutation routes인 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete는 .NET request processor queue를 유지하되 C# WMI adapter가 실행한다. Native VM create product path는 이번 slice에서 Hyper-V Generation 2만 지원하며 Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다. Native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다.
- 이 ADR은 저장소 경계와 Phase 19 evidence-first keep-spike 이력을 유지한다. 현재 제품 런타임 승격 판단은 ADR-0004가 대체한다.

## 결정

Desktop Node는 독립 Windows 저장소로 유지한다. Desktop Node 전체는 2026-04-29 기준 GA 제품 런타임으로 승격하지 않았으나, 2026-05-05 내부 전용 서비스 범위의 제품 런타임 승격 판단은 ADR-0004가 대체한다.

`packaging/windows-desktop-node/**`는 Service-first/.NET service host/MSI/protected-token/diagnostics/LAN-security/safe-update 제품 후보 배포 계층이다.

- Phase 13 WinSW 경계는 이력과 compatibility test로 보존한다.
- `archive/spikes/purecvisor-desktop-node/**`는 component 구현 원천과 검증 경계다.

Single Edge release gate와 Desktop Node GA 승격 판단은 분리한다.

## 충족된 제품화 gate

- DPAPI LocalMachine protected token file을 제품 wrapper 기본 bearer token source로 둔다.
- Diagnostic bundle은 raw token, protected token blob, token hash, host absolute path를 redaction한다.
- LAN mode는 loopback 기본값과 preview/admin opt-in 정책을 유지한다.
- Update/rollback/config migration은 manifest-first safe update 정책과 단일 previous slot을 사용한다.

## 부분 해소된 gate

- Phase 22 release/version policy는 dev/admin-smoke/rc/stable channel, artifact naming, upgrade/downgrade/rollback boundary를 문서화했다.
- Installer `build.ps1`는 Phase 22 `windows-x64` MSI/provenance/hash sidecar naming, provenance `release_channel`, unsigned RC/stable 차단을 강제한다.
- Local test signer 기준 elevated MSI lifecycle 전체 exit 0 smoke와 product-wrapper update/rollback/config migration smoke는 2026-04-30 및 2026-05-01 evidence로 반복 확인했다. 이는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Internal root/leaf signer 기준 `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021` evidence로 확인했다.
- 이 internal signing evidence는 내부 서비스용이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Default product service host, listener owner, SCM binary path, MSI installed custom action runner는 `DesktopNode.Host.exe`로 교체됐다.
- 이 service host replacement는 `artifacts/dotnet-host-admin-smoke-20260501-213444`에서 direct service-action, MSI lifecycle, Hyper-V helper integration smoke로 확인했다.
- .NET Host route parity service-action/MSI/Hyper-V API smoke는 `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729`에서 확인했다.
- `host.status` read route는 C# registry/WMI/service/admin native adapter로 전환됐다.
- `network.inventory` read route는 C# native WMI adapter가 직접 처리하며 switch type, `allow_management_os`, external adapter field가 불완전하면 native structured failure를 반환한다.
- Native `network.inventory` 설치본 evidence는 `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`, 리뷰 수정 후 당시 topology parity fallback evidence는 `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`에 기록했다.
- Native `host.status` 설치본 evidence는 `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`에 기록했다.
- Native `vm.list` WMI query guard 수정 후 설치본 evidence는 `artifacts/routeparity-service-msi-hyperv-mutation-20260503-113517`에 기록했다.
- `GET /api/v1/vms/{id}` native-first 설치본 smoke는 `artifacts/routeparity-service-msi-hyperv-mutation-20260503-115135`에 기록됐다.
- `GET /api/v1/vms/{id}/checkpoints` native-first 설치본 non-mutating smoke는 `artifacts/installed-nonmutating-checkpoint-list-20260503-121824`에 기록됐다. 사용자 explicit opt-in 범위의 VM create/checkpoint lifecycle cleanup smoke는 `artifacts/installed-vm-create-checkpoint-list-20260503-122705`, `artifacts/installed-checkpoint-lifecycle-cleanup-20260503-124330`에 기록됐다.
- Checkpoint create/delete native mutation adapter 설치본 evidence는 `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283`에 기록됐다. Checkpoint restore까지 포함한 설치본 evidence는 `artifacts/routeparity-service-msi-hyperv-restore-mutation-20260503-0286`에 기록됐다. VM start/poweroff까지 포함한 설치본 evidence는 `artifacts/routeparity-service-msi-hyperv-vm-power-state-mutation-20260503-0288`에 기록됐다. Installed runtime policy는 당시 VM start/poweroff와 checkpoint create/restore/delete native mutation operation을 보고했고 final service와 cleanup evidence는 정상 상태로 끝났다.
- VM create/shutdown/restart native lifecycle adapter slice는 runtime policy를 `native_mutation_operations=[vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,checkpoint.create,checkpoint.restore,checkpoint.delete]`, `mutation_dispatch=native-vm-create-lifecycle-checkpoint-mutation`으로 갱신했다. 후속 VM delete slice는 `vm.delete`를 native mutation operation에 추가하고 `mutation_dispatch=native-vm-create-lifecycle-delete-checkpoint-mutation`으로 갱신했다. `0.30.1-admin-smoke` 설치본 mutation smoke는 VM create/start/restart/poweroff/delete와 checkpoint create/restore/delete native route를 통과했고, managed VM delete `action=delete`, repeat delete `action=absent`, unmanaged VM delete block `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, installer-ISO shutdown unavailable `PCV_VM_SHUTDOWN_NOT_AVAILABLE`을 확인했다. `artifacts/guest-shutdown-windows-smoke-20260503-222750`은 Microsoft Windows Server 2022 Evaluation VHD guest에서 installed Local API `vm.shutdown` job `succeeded`, final VM `Off`, cleanup 완료를 확인했다.
- VM summary/storage/network native parity code-level slices는 WMI CPU/startup memory/generation/checkpoint count, storage path, network switch mapping을 추가했다. 이 시점에는 ADR-0001의 `keep-spike` 판단을 바꾸지 않았다.
- Web Console browser fixture parity code-level slice는 `npm run verify:parity`와 `npm run browser:fixture` 범위에서 served `app.js` initial render smoke를 추가했다. 이 시점에는 ADR-0001의 `keep-spike` 판단을 바꾸지 않았다.
- VM lifecycle native slices는 C# WMI `Msvm_ComputerSystem.RequestStateChange`와 native VM create job execution을 추가했다. 이 시점에는 ADR-0001의 `keep-spike` 판단을 바꾸지 않았다.
- Checkpoint create/restore/delete native mutation adapter slices는 C# WMI `CreateSnapshot`/`ApplySnapshot`/`DestroySnapshot` job execution을 추가했다. 이 시점에는 ADR-0001의 `keep-spike` 판단을 바꾸지 않았다.
- 위 evidence는 unsigned admin-smoke 및 code-level evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

## GA 차단 gate

ADR-0004 적용 전에는 다음 증거가 닫히기 전까지 Desktop Node를 GA 제품 런타임으로 승격하지 않았다.

- 외부 배포가 필요한 경우 Public trusted/stable signing evidence 또는 release approval
- selected trust model로 signed stable MSI 기준 Elevated MSI lifecycle 전체 exit 0 smoke
- selected trust model과 묶인 Hyper-V lifecycle integration evidence
- 내부 stable 또는 외부 stable 발행 승인과 release evidence
- JSONL first 장기 운영 evidence 또는 Windows Event Log writer/provider 전환 evidence의 GA 승격 판단
- Single Edge release gate와 Desktop Node release gate의 CI/문서 분리 유지

최근 evidence 요약:

- 2026-04-30: elevated MSI lifecycle, Hyper-V product-flow, release approval/signing preflight, firewall cleanup, 운영/Event Log source lifecycle을 draft-ready 기준으로 기록했다.
- 2026-05-01: 관리자 opt-in hardening evidence에서 service recovery, protected token ACL, firewall, Event Log scoped source lifecycle, LAN listener/firewall preview를 기록했다.
- 같은 hardening evidence에서 direct/Product API Hyper-V lifecycle, self-signed TLS reverse proxy preview, 75초 운영 sampling도 기록했다.
- 2026-05-01: `0.23.9-rc.1` local test `RequireSigned` MSI lifecycle과 product-wrapper update/rollback/config migration evidence를 기록했다.
- 2026-05-01: internal root/leaf signer 기준 `0.23.10-rc.1` `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS를 기록했다.
- 2026-05-02: `0.26.x-admin-smoke` evidence에서 .NET Host replacement, route parity, native `network.inventory`, MSI repair 재생성, native topology parity fallback, request processor 직렬화가 자동 reboot 없이 통과했다.
- 2026-05-02: `0.27.1-admin-smoke` evidence에서 native `host.status` 포함 설치본 service/MSI/Hyper-V route smoke가 자동 reboot 없이 통과했다.
- 2026-05-03: `0.27.3-admin-smoke` evidence에서 native `vm.list` WMI query guard 수정 후 설치본 service/MSI/Hyper-V route smoke가 자동 reboot 없이 통과했다.
- 2026-05-03: `0.27.4-admin-smoke` evidence에서 `GET /api/v1/vms/{id}` native-first slice 이후 설치본 service/MSI/Hyper-V route smoke가 자동 reboot 없이 통과했다.
- 2026-05-03: `0.27.5-admin-smoke` evidence에서 `GET /api/v1/vms/{id}/checkpoints` native-first 설치본 non-mutating smoke와 사용자 explicit opt-in VM/checkpoint lifecycle cleanup smoke가 자동 reboot 없이 통과했다.
- 2026-05-03: `0.27.6-admin-smoke` evidence에서 runtime policy dispatch boundary contract 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke가 자동 reboot 없이 통과했다. 당시 installed runtime policy는 native read probe operation 목록과 `helper-process-direct` mutation dispatch marker를 보고했고, final service는 `Running`, `pcv-spike-*` VM 잔여물은 없었다.
- 2026-05-03: Web Console browser fixture parity code-level slice에서 served `app.js` dashboard/VM/job initial render smoke가 npm 검증으로 통과했다.
- 2026-05-03: `0.28.3-admin-smoke` evidence에서 checkpoint create/delete native mutation adapter 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke가 자동 reboot 없이 통과했다. 이어 `0.28.6-admin-smoke` evidence에서 checkpoint restore까지 포함한 설치본 route smoke가 `vm.poweroff-before-restore` 최소 안정 조건으로 통과했다.
- 2026-05-03: `0.28.8-admin-smoke` evidence에서 VM start/poweroff native power-state adapter 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke가 자동 reboot 없이 통과했다. Installed runtime policy는 당시 `native_mutation_operations=[vm.start,vm.poweroff,checkpoint.create,checkpoint.restore,checkpoint.delete]`를 보고했고, final service는 `Running`, `pcv-spike-*` VM 잔여물은 없었다.
- 2026-05-03: VM create/shutdown/restart native lifecycle adapter slice에서 current runtime policy는 `native_mutation_operations=[vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,checkpoint.create,checkpoint.restore,checkpoint.delete]`와 `mutation_dispatch=native-vm-create-lifecycle-checkpoint-mutation`을 보고했다. 후속 VM delete slice에서 current runtime policy는 `native_mutation_operations=[vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete]`와 `mutation_dispatch=native-vm-create-lifecycle-delete-checkpoint-mutation`을 보고한다. Native VM create는 Generation 2 product path만 지원하고, native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. `0.30.1-admin-smoke` 설치본 evidence는 managed VM delete `action=delete`, repeat delete `action=absent`, unmanaged VM delete block, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다. `artifacts/guest-shutdown-windows-smoke-20260503-222750`은 Microsoft Windows Server 2022 Evaluation VHD guest에서 installed Local API `vm.shutdown` job `succeeded`, final VM `Off`, cleanup 완료를 확인했다.

Public trusted certificate/PFX/private key가 local environment에 없어 외부 stable publication은 실행하지 않았다.

Internal trusted signing evidence와 .NET host admin-smoke evidence는 이후 GA-ready aggregate closure와 internal stable release/update/rollback evidence로 보강됐다. 현재 내부 전용 서비스 제품 런타임 승격 판단은 ADR-0004가 소유하며, public trusted signing과 외부 stable publication은 범위 밖이다.

## 대안

### Linux ADR 복사

선택하지 않는다. Linux ADR은 Single Edge runtime과 C service 경계를 다루며, Desktop Node Windows product wrapper 결정의 단일 진실이 아니다.

### Phase 11-19 전체 개별 ADR 분해

지금은 선택하지 않는다. 이미 phase spec과 plan이 상세 이력을 보존하므로, 현재 적용 결정을 빠르게 찾는 경량 ADR이 더 적합하다.

## 영향 범위

- 포함 경로:
  - `archive/spikes/purecvisor-desktop-node/**`
  - `packaging/windows-desktop-node/**`
  - `docs/**`
- 제외 경로:
  - Linux `purecvisorsd`
  - Linux Single Edge UI/API
  - KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- 운영 영향:
  - signed build, elevated MSI lifecycle, 실제 Hyper-V lifecycle은 계속 관리자 opt-in gate다.

## 검증 기준

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- `docs/adr/0003-internal-trusted-signing-policy.md`
- `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`
- `docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md`
