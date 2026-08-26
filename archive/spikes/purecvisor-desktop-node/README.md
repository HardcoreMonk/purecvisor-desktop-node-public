# PureCVisor Desktop Node Spike

## Historical Phase 19/22 predecessor snapshot

```text
PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike
DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike
DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
```

이 디렉터리는 Windows 10/11 Pro/Enterprise + Hyper-V 기반 PureCVisor Desktop Node 제품 방향을 검증하기 위한 격리 spike다. 위 `keep-spike` 표식은 Phase 19/22 당시의 historical decision이며, archive 자산 자체를 보존하는 이유를 기록한다.

현재 운영 제품은 repo-root `src/DesktopNode.*`, `web/**`,
`packaging/windows-desktop-node/**`가 소유하는 `0.42.74-admin-smoke`다. Web Console과
PCVCLI가 active operator surface이고 TUI는 absent다. 최신 닫힌 manual-admin
package-pair는 `0.42.73-admin-smoke -> 0.42.74-admin-smoke`이며 feature qualification은
`promotion_eligible=false`다. Required CI는 final `main`
`6e2bdb93ce308b632c929e2c17f5550ac3845401`, run `32904006595`의 exact contexts
`dotnet`, `web`, `delivery`, `installer-policy`가 소유한다. pwsh 기반 Public Boundary run
`32904006619`는 non-required transition residue다.

Phase 12는 이 spike 자산을 이동하지 않고 `packaging/windows-desktop-node/` Service-first wrapper가 제품 후보 설치 루트로 복사해 검증한다. Phase 13은 같은 wrapper에서 WinSW service wrapper를 service host로 사용해 Windows SCM service 시작 차단점을 해소했다. Phase 14는 같은 packaging 경계에 WiX MSI-first installer source/build/provenance와 repair/uninstall/remove-data UX를 추가한다. Phase 15는 같은 배포 계층의 기본 bearer token source를 DPAPI LocalMachine protected token file로 전환한다. Phase 16은 같은 wrapper에 JSONL first diagnostics policy, log rotation, versioned diagnostic bundle manifest, Windows Event Log opt-in registration plan을 추가한다. Phase 17은 같은 경계에서 LAN mode를 loopback 기본값, preview/admin opt-in, reverse proxy/TLS 전제, non-loopback static bearer auth, firewall opt-in lifecycle로 제한하는 제품 보안 정책으로 고정한다. Phase 18은 같은 경계에서 manifest-first safe update/rollback/config migration 기본 구현, 검증, 관리자 update/rollback smoke를 둔다. Phase 22는 같은 packaging 경계에서 release/version policy와 installer artifact/channel contract 일부를 강제한다. ADR-0003은 같은 installer 경계에서 내부 서비스용 internal Root/leaf `RequireSigned` signing trust model을 채택한다. Phase 24 후보는 `archive/spikes/purecvisor-desktop-node/api/**`에서 Local API job runtime public boundary를 먼저 고정한다. Phase 25 후보는 `src/DesktopNode.*`와 `web/src/**`에 .NET/TypeScript side-by-side contract와 parity scaffold를 추가했고, 2026-05-01 replacement slice에서 기본 제품 service host와 MSI installed custom action runner를 `DesktopNode.Host.exe`로 교체했다. Route parity 시작 slice는 `src/DesktopNode.Api/**`에 native read routes, queued VM/checkpoint lifecycle routes, job get/cancel/retry, JSON job store save/load/recovery를 추가했다. `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native adapter product path가 직접 처리하며 native parity failure는 PowerShell helper fallback 없이 structured failure로 반환한다. VM create/start/shutdown/poweroff/restart/delete는 C# native lifecycle adapter product path가 직접 처리하고 checkpoint create/restore/delete는 C# WMI snapshot service adapter product path가 직접 처리한다. Native VM create는 이번 slice에서 Hyper-V Generation 2만 지원하며, native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. Web Console은 repo-root `web/**`로 이동했고 `web/src/served-app.ts`가 served `web/app.js`를 생성한다. 기존 PowerShell Local API와 Hyper-V helper는 component/regression 검증 경계로 남는다. 따라서 `spikes/`는 아직 API/Hyper-V/service/CLI component 검증 경계이고, `web/`는 제품 Web Console 경계이며, `packaging/windows-desktop-node/`는 Service-first/.NET service host/MSI/protected-token/diagnostics/LAN-security/safe-update/release-artifact/signing-trust 배포 계층이다.

이 spike는 Linux `purecvisorsd` 런타임, Single Edge Web UI/API, Single Edge release artifact와 연결하지 않는다. Single Edge 공개 릴리스 gate와 Desktop Node 검증 gate는 분리한다.

## Historical spike component snapshot

- Hyper-V helper: host diagnostics, VM inventory, VM create/lifecycle/checkpoint JSON 계약 검증
- Local API: loopback-first HTTP listener, queued jobs, persistence, manual retry, runtime policy, Phase 24 `job_runtime` boundary contract, token/token-file/protected-token-file, LAN opt-in 검증
- Web Console: static asset, host dashboard, VM table, VM detail/lifecycle/checkpoint UI, browser-local job history, TypeScript static parity scaffold/generated manifest/verification/browser fixture flow 검증
- CLI: Local API thin client, runtime policy, VM/job/checkpoint command, `--token-file`, `--protected-token-file` 검증
- Service packaging: `sc.exe` command builder, token/protected token file preparation, ACL command builder, `-WhatIf` smoke 검증
- Product wrapper: Phase 12 Service-first install/rollback/uninstall/status/diagnostics wrapper, Phase 13 WinSW service wrapper 이력, Phase 14 WiX MSI installer, Phase 15 DPAPI protected token storage, Phase 16 JSONL first long-term diagnostics, Phase 17 LAN security policy, Phase 18 update/rollback/config migration 기본 구현과 관리자 smoke 검증, Phase 22 installer artifact/channel contract, ADR-0003 internal signing trust model, Phase 25 .NET service host replacement. 2026-04-30 local signed RC/elevated MSI lifecycle evidence를 기록했고, 2026-05-01 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` MSI lifecycle과 product-wrapper update/rollback/config migration smoke도 통과했다. 같은 날 `0.23.10-rc.1` internal enterprise `RequireSigned` MSI lifecycle도 통과했다. 이후 `0.26.0-admin-smoke` .NET Host replacement service/MSI/Hyper-V helper smoke가 자동 reboot 없이 통과했다. Public trusted/stable signing과 full updater는 후속 판단 범위다.
- .NET/TypeScript 후보: `src/DesktopNode.Contracts/**`, `src/DesktopNode.Runtime/**`, `src/DesktopNode.Api/**`, `src/DesktopNode.Service/**`, `src/DesktopNode.Host/**`와 Web Console TypeScript source/parity scaffold/browser fixture가 추가됐다. 제품 service host는 `DesktopNode.Host.exe`가 기본값이며, `src/DesktopNode.Api/**` route parity slice는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation, checkpoint create/restore/delete native mutation, queued job runtime을 처리한다. Web Console served `app.js`는 repo-root `web/src/served-app.ts` generated output이다.

## 디렉터리 역할

| 경로 | 역할 |
|------|------|
| `hyperv/` | Hyper-V PowerShell helper와 non-integration/gated integration tests |
| `api/` | Local API listener, job queue, runtime policy, static Web Console serving |
| `../../../web/` | Local API가 제공하는 제품 static Web Console assets |
| `cli/` | Local API thin client |
| `service/` | Windows service packaging과 token file preparation helper |
| `tests/` | Desktop Node root boundary와 제품 승격 판단 문서 검증 |
| `../../../packaging/windows-desktop-node/` | Phase 12 Service-first 제품 후보 wrapper, Phase 13 WinSW service host 이력, Phase 14 WiX MSI installer, Phase 15 protected token storage, Phase 16 diagnostics policy/rotation/Event Log plan, Phase 17 LAN security policy, Phase 18 safe update/rollback/config migration, Phase 25 .NET service host integration, product manifest, diagnostics, packaging/installer Pester suite |
| `../../../src/DesktopNode.*` | Phase 25 .NET contract/runtime/API/service/host candidate와 xUnit tests |

## 제품 승격 판단

2026-04-25 Phase 11 결정은 `keep-spike`다. 2026-04-26 Phase 12 결정은 `DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper`이며, 설치/업데이트/롤백/로그 수집/서비스 복구 gate를 Service-first wrapper에서 부분 해소하기 시작한다. Phase 13 결정은 `DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper`이며, 당시 제품 service host는 WinSW executable/XML staging을 기준으로 했다. 2026-04-27 Phase 14 결정은 `DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first`이며, MSI source/build/provenance와 repair/uninstall/remove-data UX를 제품 후보 배포 계층에 추가한다. 2026-04-28 Phase 15 결정은 `DESKTOP_NODE_PHASE15_TOKEN_STORAGE_DECISION: dpapi-local-machine-protected-file-first`이며, 제품 기본 bearer token source를 `api-token.dpapi.json`으로 전환한다. 2026-04-28 Phase 16 결정은 `DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred`이며, JSONL 로그와 versioned diagnostic bundle을 1차 운영 진단 경계로 확정한다. 2026-04-28 Phase 17 결정은 `DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required`이며, LAN mode를 preview/admin opt-in과 외부 TLS 전제 정책으로 제한한다. 2026-04-28 Phase 18 결정은 `DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration`이며, update/rollback/config migration의 기본 구현 기준과 관리자 update/rollback smoke 증거를 고정한다. 2026-04-29 Phase 19 결정은 `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`이며, Phase 12-18 evidence를 충족/부분 충족/GA 차단 gate로 재분류했다. 2026-04-29 Phase 22 후속 개발은 release/version policy를 문서화하고 installer `windows-x64` artifact naming, provenance `release_channel`, unsigned RC/stable 차단을 build contract로 강제했으며, ADR-0002가 이를 현재 적용 결정으로 채택한다. 2026-05-01 ADR-0003은 `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`로 내부 서비스용 signing trust model을 채택했다. 2026-04-30 Phase 24 후보는 `DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first`로 Local API job runtime의 public boundary를 고정하기 시작했다. 같은 날 Phase 25 후보는 `DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first` 아래 .NET contract/runtime/API/service와 TypeScript Web Console parity scaffold를 side-by-side로 추가했다. 2026-05-01 Phase 25 replacement slice는 `DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT: dotnet-windows-service-host-default-with-keep-spike`로 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner를 `DesktopNode.Host.exe`로 교체했다.

- public trusted signing evidence와 stable publication approval. 내부 서비스용 internal enterprise signing evidence는 2026-05-01에 별도로 기록됐다.
- Desktop Node GA 제품 런타임 승격 재판정
- service failure action 실제 적용, recovery, log retention evidence
- Single Edge release gate와 Desktop Node release gate의 CI/문서 분리 유지

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
- `docs/adr/0003-internal-trusted-signing-policy.md`
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision.md`
- `docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime-design.md`
- `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime.md`
- `docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper-design.md`
- `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md`
- `docs/superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md`
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage-design.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics-design.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy-design.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration-design.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration.md`
- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`
- `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`
- `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`
- `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`
- `docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-web-served-asset-root-migration.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-list-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-detail-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-list-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-power-state-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-mutation-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-restore-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-create-shutdown-restart-native-adapter.md`
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-delete-native-adapter.md`
- `packaging/windows-desktop-node/README.md`
- `packaging/windows-desktop-node/installer/README.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/DEVELOPER_INDEX.md`

## 기본 검증

현재 Required CI는 `.github/workflows/development-gates.yml`의 .NET verifier shards가
소유한다. final `main`/run 기준 required contexts는 `dotnet`, `web`, `delivery`,
`installer-policy` 네 개다. 아래 Pester 명령은 archive/component의 legacy/manual parity
검증이며 Required CI가 아니다. pwsh 기반 Public Boundary run `32904006619`도
non-required transition residue다.

Historical root boundary decision 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Historical component/manual 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
```

관리자 권한 통합 검증은 별도 opt-in이다. 실제 Hyper-V VM 생성, Windows service install/start/stop/delete, firewall rule 적용, token/protected token file ACL inspection은 기본 suite에서 실행하지 않는다.
