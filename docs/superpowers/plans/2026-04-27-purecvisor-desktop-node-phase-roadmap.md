# PureCVisor Desktop Node Phase 전체 단계 계획

> **For agentic workers:** REQUIRED SUB-SKILL: 새 Phase를 실제 구현할 때는 이 로드맵을 먼저 읽고, 해당 Phase 전용 plan을 `docs/superpowers/plans/YYYY-MM-DD-purecvisor-desktop-node-phaseN-<name>.md`로 작성한 뒤 `superpowers:executing-plans` 또는 `superpowers:subagent-driven-development`로 실행한다. 이 문서는 전체 단계 순서와 gate를 정의하며, 개별 Phase의 완료 증거는 각 Phase plan에만 기록한다.

**Goal:** Desktop Node Phase 1부터 현재 Phase/ADR-0004 적용 상태까지의 전체 순서, 현재 상태, 다음 전환 조건을 한 문서에서 추적한다.

**Architecture:** Desktop Node component 검증 경계는 `archive/spikes/purecvisor-desktop-node/{api,hyperv,service,cli}/**`에 read-only/component baseline으로 남아 있고, 제품 Web Console source는 2026-05-03 served asset/root migration slice 이후 repo-root `web/**`가 소유한다. 제품 배포 계층은 `packaging/windows-desktop-node/**`에서 다룬다. Phase 25 replacement slice 이후 기본 제품 service host와 MSI installed action runner는 `src/DesktopNode.Host/**`의 `DesktopNode.Host.exe`가 소유한다. 2026-05-09 active .NET CLI slice 이후 제품 CLI source는 `src/DesktopNode.Cli/**`가 소유하고 installed command name은 `pcvcli.exe`다. ADR-0004 이후 Desktop Node는 내부 전용 GA-ready 제품 런타임이며 public trusted signing과 외부 stable publication은 scope 밖이다. Phase 12-18 product wrapper, Phase 19 제품 승격 재판정, Phase 24 Local API job runtime boundary 후보, Phase 25 .NET/TypeScript 전환 후보는 Linux `purecvisorsd`, Single Edge 공개 UI/API 표면과 연결하지 않는다.

**Tech Stack:** PowerShell 7, Pester 5, Hyper-V PowerShell cmdlet, Windows `HttpListener`, TypeScript Web Console with served static output, historical WinSW service wrapper, .NET 10/C#/xUnit Windows Service host, JSON file persistence, DPAPI LocalMachine protected token file, JSONL diagnostics, LAN security policy, manifest-first update policy, evidence-first promotion gate tracking.

---

## 문서 역할

이 문서는 Phase별 세부 구현 계획을 대체하지 않는다.

- 설계 결정은 해당 Phase spec과 `docs/adr/`를 따른다.
- 검증 명령과 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.
- 완료 증거와 관리자 smoke 결과는 해당 Phase plan의 `완료 증거`에 기록한다.
- 다음 작업 대기열은 `follower.md`에 요약한다.
- Phase 번호를 새로 부여하기 전에 이 문서에서 기존 후보 번호와 충돌하지 않는지 확인한다.

## 전체 Phase 순서

| Phase | 상태 | 목표 | 주요 산출물 | 다음 전환 조건 |
|-------|------|------|-------------|----------------|
| Phase 1 | 완료 | Hyper-V helper spike로 host, VM inventory, VM create, lifecycle, checkpoint JSON 계약을 증명한다. | `archive/spikes/purecvisor-desktop-node/hyperv/**`, Phase 1 plan | 현재는 component/archive baseline이며 product path는 .NET native adapter다. |
| Phase 2A | 완료 | loopback Local API daemon spike를 만든다. | `archive/spikes/purecvisor-desktop-node/api/**` listener와 route dispatch | 현재는 component/archive baseline이며 product API path는 `src/DesktopNode.Api/**`다. |
| Phase 2B | 완료 | VM create를 job API 계약으로 감싼다. | `POST /api/v1/vms`, `GET /api/v1/jobs/{job_id}` | request path에서 장시간 작업을 분리할 수 있어야 한다. |
| Phase 2C | 완료 | in-memory worker queue를 추가한다. | queued/running/succeeded/failed job 상태 전이 | job 상태가 재시작 후에도 보존될 수 있어야 한다. |
| Phase 2D | 완료 | JSON file job persistence를 추가한다. | `-JobStorePath`, corrupt store quarantine | 사용자가 job을 취소하거나 실패 job을 재시도할 수 있어야 한다. |
| Phase 2E | 완료 | queued cancel과 failed retry를 추가한다. | job control routes, retry source tracking | Web Console과 CLI가 같은 job contract를 사용할 수 있어야 한다. |
| Phase 2F | 완료 | Local API가 static Web Console을 optional로 제공한다. | `-WebRootPath`, traversal 방지 | API token gate가 LAN mode 이전에 성립해야 한다. |
| Phase 2G | 완료 | optional bearer token gate를 추가한다. | `-ApiToken`, authorization check | worker throughput을 bounded 방식으로 조정할 수 있어야 한다. |
| Phase 2H | 완료 | bounded worker-pool tick contract를 추가한다. | `Invoke-PcvApiWorkerPoolTick -WorkerCount` | Web Console 첫 화면이 기존 API만으로 동작해야 한다. |
| Phase 3A | 완료 | 첫 static Web Console dashboard를 만든다. | host status, VM table, create job form, session job panel | VM detail과 lifecycle action을 UI/API에 연결해야 한다. |
| Phase 3B | 완료 | VM detail drawer와 lifecycle job action을 추가한다. | VM detail route, lifecycle routes, detail drawer | CLI가 같은 Local API 계약을 사용할 수 있어야 한다. |
| Phase 4 | 완료 | Local API thin client CLI MVP를 만든다. | `archive/spikes/purecvisor-desktop-node/cli/**`; active product follow-up `src/DesktopNode.Cli/**` | archived PowerShell CLI는 component/archive baseline이다. 2026-05-09 active .NET CLI follow-up은 product command `pcvcli.exe`로 별도 구현/검증/패키징됐다. |
| Phase 5 | 완료 | LAN mode hardening을 추가한다. | `-AllowLan`, token 필수, event log, firewall command builder | service 실행에서 token 값 노출을 줄여야 한다. |
| Phase 6 | 완료 | Windows service packaging spike를 추가한다. | `sc.exe` command builder, injectable runner, service README | service command line에 장기 token 값을 직접 남기지 않아야 한다. |
| Phase 7 | 완료 | service token file hardening을 추가한다. | `-ApiTokenFile`, token source conflict 검증 | token file 생성과 ACL 적용 경계를 installer 준비 단계로 올려야 한다. |
| Phase 8 | 완료 | installer hardening의 기본 경계를 추가한다. | token file helper, `icacls.exe` ACL builder, service account | Local API runtime policy를 명시적으로 노출해야 한다. |
| Phase 9 | 완료 | Local API runtime hardening을 추가한다. | `/api/v1/runtime/policy`, retry limit, cancel policy | Web Console/CLI 제품화 후속 UX를 정리해야 한다. |
| Phase 10 | 완료 | Web Console/CLI 제품화 후속을 추가한다. | checkpoint UI, browser-local job history, CLI `--token-file` | 제품 런타임 승격 여부를 명확히 결정해야 한다. |
| Phase 11 | 완료 | Desktop Node를 계속 spike로 유지한다는 제품 승격 판단을 기록한다. | `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`, root boundary guard | 제품 후보 배포 계층을 spike와 분리해 검증해야 한다. |
| Phase 12 | 완료 | Service-first product wrapper를 만든다. | product root/data root, manifest, action plan, diagnostics, dry-run smoke | SCM service host 차단점을 해결해야 한다. |
| Phase 13 | 완료 | WinSW service wrapper로 service install/start/status/diagnostics/uninstall 기준 경로를 만든다. | WinSW XML/staging, product action 전환, loopback static auth boundary, closure smoke 보강 | Phase 14 signed installer/repair UX 구현으로 이어졌다. |
| Phase 14 | 기본 구현/검증 완료 | signed installer와 repair/uninstall UX를 구현한다. | WiX MSI-first source/build, MSI 전용 product wrapper action, signing/provenance, installer 문서와 검증 정책, unsigned dev MSI build | signed release build와 elevated `msiexec` smoke는 조건부로 남기고, Phase 15 secure token storage 구현으로 이어졌다. |
| Phase 15 | 기본 구현/검증 완료 | DPAPI LocalMachine protected token file을 제품 기본 token source로 확정한다. | `api-token.dpapi.json`, `-ApiTokenProtectedFile`, `--protected-token-file`, rotation/revoke helper, diagnostics redaction, legacy token migration | Event Log와 장기 diagnostics 정책 구현으로 이어졌다. |
| Phase 16 | 기본 구현/검증 완료 | Windows Event Log와 long-term diagnostics 정책을 확정한다. | JSONL first decision, diagnostics policy v1, log rotation, diagnostic bundle manifest, Event Log opt-in registration plan | Phase 17 LAN mode 제품 보안 정책과 TLS/reverse proxy 전제로 이어진다. |
| Phase 17 | 기본 구현/검증 완료 | LAN mode 제품 보안 정책과 TLS/reverse proxy 전제를 확정한다. | LAN default policy, TLS/reverse proxy stance, non-loopback static auth rule, firewall opt-in lifecycle, runtime policy network object, diagnostics LAN policy artifact | Phase 18 update/rollback/config migration 구현으로 이어졌다. |
| Phase 18 | 기본 구현/검증 및 관리자 smoke 완료 | update/rollback/config migration을 manifest-first safe update 정책으로 고정한다. | update policy v1, manifest validation, safe update orchestration, rollback validation, update diagnostics artifact, mutating update/rollback smoke | Phase 19 제품 승격 재판정으로 이어진다. |
| Phase 19 | 완료 | Desktop Node 제품 승격을 증거 우선(evidence-first)으로 다시 판정한다. | `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`, GA 차단 게이트 목록, 활성 문서/root boundary guard | 서명된 릴리스/MSI 수명주기, Hyper-V 제품 통합, stable 발행, 장기 운영 로그 증거를 후속 게이트로 닫아야 한다. |
| Phase 20 | local/internal signed RC lifecycle evidence 및 release approval 완료 | signed release build와 elevated MSI lifecycle evidence를 수집한다. | Phase 20 spec/plan, `RequireSigned` build runbook, elevated `msiexec` lifecycle checklist, 2026-04-30 signed RC MSI evidence, non-mutating MSI lifecycle plan/classification tests, `artifacts/p0-signed-msi-lifecycle-rerun-20260430-191040`, `artifacts/p1-release-approval-and-signing-preflight-20260430-2045`, `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`, `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021` | local test certificate 기준 `0.23.8-rc.1` signed RC build와 elevated install/repair/uninstall/`REMOVE_DATA=1` exit 0 smoke는 완료됐다. 2026-05-01에는 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` build와 동일 lifecycle, final MSI restore install도 exit 0으로 재확인했다. 같은 날 internal Root/leaf signer 기준 `0.23.10-rc.1` `RequireSigned` build, Authenticode `Valid`, SignTool verify exit `0`, elevated lifecycle PASS도 확인했다. Public trusted signing material은 없어 외부 stable publication은 실행하지 않았다. |
| Phase 21 | product-flow lifecycle evidence 완료 | signed/elevated product install 흐름과 묶인 Hyper-V lifecycle integration evidence를 수집한다. | Phase 21 spec/plan, Hyper-V non-integration preflight, product API lifecycle runbook, checkpoint evidence classifier, cleanup checklist, `artifacts/phase21-product-flow-rerun-20260430-190840` | product API VM create/start/checkpoint/poweroff/cleanup과 checkpoint raw evidence 3종 `verified_visible` assessment를 기록했다. 실패 job retry는 현재 runtime policy의 `failed_error_retryable_only` 계약상 `409/PCV_JOB_NOT_RETRYABLE`로 확인했다. |
| Phase 22 | 정책/installer 계약 반영, ADR-0002/0003 적용 중 | 릴리스 채널, 버전 명명, 산출물 명명, 업그레이드/다운그레이드, 롤백 호환성 정책을 GA 선언 없이 고정한다. | Phase 22 spec/plan, ADR-0002, ADR-0003, dev/admin-smoke/rc/stable 채널 매트릭스, 산출물 명명 매트릭스, installer `windows-x64` 산출물 명명, provenance `release_channel`, `signing_trust_model`, unsigned RC/stable 차단 | 정책은 현재 적용 결정으로 채택했지만, stable 발행은 릴리스 승인과 selected trust model 서명/관리자 권한 증거 이후에만 다룬다. |
| Phase 23 | 운영 hardening evidence 추가 완료 | Windows 장기 운영 evidence와 Event Log 전환 판단 기준을 수집한다. | Phase 23 spec/plan, JSONL first 운영 evidence runbook, service failure/recovery checklist, diagnostics redaction 기준, `artifacts/p1-operational-eventlog-lifecycle-20260430-2050`, `artifacts/admin-optin-continuation-20260501-162940`, `artifacts/admin-optin-tls-reverse-proxy-preview-20260501-163308` | 기존 service Running 관측, ProductStatus/CollectDiagnostics exit 0, Web root 200, Event Log source register/write/read/remove lifecycle을 관리자 opt-in evidence로 기록했다. 2026-05-01에는 WinSW service reinstall/start, SCM failure action apply, protected token ACL inspection, firewall create/update/delete, Event Log scoped source lifecycle, LAN listener/firewall preview, direct Hyper-V lifecycle, Product API Hyper-V lifecycle, self-signed TLS reverse proxy preview, 75초 운영 sampling을 추가로 기록했다. JSONL-first는 primary 유지, Event Log writer/provider 기본 활성화와 GA 승격은 별도 판단이다. |
| Phase 24 | 후보 baseline slice 완료 | Local API job runtime public boundary를 고정한다. | Phase 24 spec/plan, runtime policy `job_runtime` contract, Local API/CLI/Pester contract, diagnostics self-audit | Windows Hyper-V는 dataplane 구현보다 orchestration 문제에 가까우므로 PowerShell-first 계약을 유지하고, C++23 core는 state machine/supervision이 깊어질 때만 재검토한다. |
| Phase 25 | 후보/현재 .NET 및 TypeScript parity/API/service scaffold, .NET service host replacement, native read routes, VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete native mutation adapter 완료 | .NET contract/runtime core, TypeScript Web Console, PowerShell adapter 전환 경계를 고정하고 기본 제품 service host를 .NET으로 이동한다. | Phase 25 spec/plan, .NET Host replacement spec/plan, native adapter plans, `src/DesktopNode.Contracts/**`, `src/DesktopNode.Runtime/**`, `src/DesktopNode.Api/**`, `src/DesktopNode.Service/**`, `src/DesktopNode.Host/**`, `web/src/**`, `web/generated/parity/**`, `web/scripts/**`, .NET contract/runtime/API/service/host tests, TypeScript `tsc --noEmit` and `verify:parity` scaffold | 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner는 `DesktopNode.Host.exe`로 교체됐다. `src/DesktopNode.Api/**`는 native read routes, VM lifecycle/checkpoint native mutation routes, queued job runtime, job store save/load/recovery를 처리한다. Public trusted/stable signing과 외부 publication은 내부 전용 서비스 scope 밖이며, GA-ready 제품 런타임 판단은 Phase 26/ADR-0004로 적용됐다. |
| Phase 26 | 완료/적용 | GA-ready 제품 런타임을 내부 전용 current decision으로 적용하고 route promotion matrix, repo migration map, verification ownership map을 supporting docs로 고정한다. | ADR-0004, GA-ready redesign spec, Phase 26 alignment plan, `docs/ga-ready/**`, 2026-05-05 aggregate closure evidence | GA-scope blocked row 0개, PowerShell-backed current owner 0개, active product `spikes/**` reference 0개, internal stable release/update/rollback evidence pass 기준으로 ADR-0004가 적용됐다. Public trusted signing과 외부 stable publication은 scope 밖이다. |

## Phase 13 Closure Gate

Phase 13은 기본 구현, 회귀 보강, 관리자 smoke closure gate가 완료됐다. 상세 pass count와 환경별 증거는 Phase 13 plan의 `완료 증거`에만 기록한다.

완료된 closure 항목:

1. `Install -WinSwPath` health check가 token file bearer token으로 runtime policy를 확인한다.
2. `Uninstall`과 `Rollback`은 WinSW stop 이후 status polling으로 stopped/missing 상태를 확인한다.
3. Product root 제거는 WinSW executable lock 같은 일시적 접근 거부를 제한적으로 재시도한다.
4. `Uninstall -RemoveData`는 hardened token file 삭제 전에 관리자 삭제 권한을 복구한다.
5. 실제 관리자 smoke로 install, status, token 포함 runtime policy, loopback root, diagnostics, 기본 uninstall을 같은 실행 흐름에서 확인했다.
6. 실제 `Rollback` smoke를 실행했다.
7. 실제 `Uninstall -RemoveData` smoke를 실행했다.
8. Hyper-V helper integration smoke 기록은 Phase 13 plan에 남아 있다. 2026-04-30에는 signed/elevated product install 흐름과 묶인 Hyper-V product-flow lifecycle evidence를 별도 Phase 21 evidence로 기록했다. 이 결과는 public trusted/stable signing 또는 GA 승격을 의미하지 않는다.

예상 수정 파일:

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- `packaging/windows-desktop-node/README.md`
- `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md`
- `follower.md`

기본 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

관리자 smoke 기준:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1 -SelfTest
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

실제 MSI install/repair/uninstall/`REMOVE_DATA=1`, Hyper-V mutation, firewall/LAN/trust-store mutation은 사용자 관리자 opt-in 시 packaging README와 route matrix gate에 맞춰 실행한다. Product path는 `DesktopNode.Host.exe` native service-action과 protected token file schema를 사용하며 active `spikes/**` module import를 사용하지 않는다.

## Phase 14: Signed Installer와 Repair/Uninstall UX

목표:

- WinSW 기반 product wrapper를 사용자가 설치 가능한 signed installer 산출물로 감싼다.
- repair, uninstall, remove data, product root/data root 표시를 사용자 UX와 CLI JSON 계약 모두에서 일관되게 제공한다.

설계 결정:

```text
DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first
```

설계 문서:

- `docs/superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md`

구현 계획:

- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md`

반영된 항목:

1. WiX MSI source와 build script의 파일 경계를 나눈다.
2. Product wrapper에 MSI 전용 `ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled` action을 추가한다.
3. Code signing 대상과 외부 입력 계약을 구현한다.
4. WinSW executable provenance와 release build 차단 정책을 구현한다.
5. 기본 uninstall data preservation과 `REMOVE_DATA=1` equivalent를 MSI property로 노출한다.
6. Installer README, product wrapper README, 검증 정책, 개발자 인덱스, 후속 작업 대기열을 Phase 14 상태에 맞춰 갱신한다.

완료 조건:

- installer suite, packaging suite, root boundary suite, static check가 통과해야 한다.
- `.NET SDK`와 WiX CLI가 준비된 개발 환경에서는 unsigned dev MSI build가 통과해야 한다.
- installer가 Phase 13 WinSW service lifecycle을 깨지 않는지 관리자 opt-in smoke에서 확인한다.
- signed artifact provenance와 SHA 기록 절차가 문서화되어야 한다.
- local test certificate 기준 signed RC MSI build와 elevated `msiexec` install/repair/uninstall/`REMOVE_DATA=1` smoke는 2026-04-30 evidence로 기록됐다. 2026-05-01에는 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` build와 동일 lifecycle 전부 exit `0`을 재확인했다. 같은 날 internal Root/leaf signer 기준 `0.23.10-rc.1` `RequireSigned` build/lifecycle PASS도 기록했다. 외부 public trusted signing과 public stable publication은 public trusted certificate/PFX/private key 준비 이후에만 다룬다.

## Phase 15: Secure Token Storage

목표:

- 장기 bearer token을 plain token file 중심에서 DPAPI LocalMachine protected token file 중심으로 승격한다.

설계 결정:

```text
DESKTOP_NODE_PHASE15_TOKEN_STORAGE_DECISION: dpapi-local-machine-protected-file-first
```

설계 문서:

- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage-design.md`

구현 계획:

- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage.md`

반영된 항목:

1. 제품 wrapper 기본 token path를 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`으로 전환했다.
2. service helper에 protected token prepare, rotate, revoke action을 추가했다.
3. Local API는 `-ApiTokenProtectedFile` source와 runtime policy `token_storage = dpapi-local-machine`을 지원한다.
4. CLI는 `--protected-token-file`을 지원한다.
5. 기존 `api-token.txt`는 legacy migration과 rollback compatibility, `RemoveData` 삭제 대상으로만 유지한다.
6. diagnostics bundle과 manifest redaction은 protected token blob과 token hash를 복제하지 않는다.

완료 조건:

- service command line과 product manifest에 raw token 값이 남지 않는다.
- diagnostics, logs, event output에서 raw token 값, protected token blob, token hash가 redaction된다.
- downgrade 또는 rollback 시 token source가 모호해지지 않는다.
- 관리자 권한 service/MSI smoke에서 protected token runtime policy 200 응답을 확인하는 절차가 문서화되어야 한다.

## Phase 16: Event Log와 Long-Term Diagnostics

목표:

- Windows Event Log provider/source lifecycle, JSONL log retention, diagnostic bundle schema를 제품 운영 기준으로 확정한다.

설계 결정:

```text
DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred
```

설계 문서:

- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics-design.md`

구현 계획:

- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics.md`

반영된 항목:

1. JSONL event/install log를 1차 운영 로그로 유지한다.
2. Product plan과 manifest에 diagnostics policy v1을 포함한다.
3. Diagnostic bundle에 `diagnostics-manifest.json`과 redacted source artifact 목록을 포함한다.
4. `events.jsonl`, `install.jsonl`, WinSW service log rotation/retention helper를 추가한다.
5. Windows Event Log source 등록은 기본 install/repair/diagnostics 경로에서 실행하지 않고 admin opt-in registration plan으로만 노출한다.

완료 조건:

- service lifecycle, Local API runtime, installer action, rollback 실패가 같은 diagnostic bundle에서 추적된다.
- token과 host-sensitive path가 redaction된다.
- 운영자가 문제 원인을 product root와 data root만 보고 재현할 수 있다.
- 실제 Windows Event Log source 등록은 관리자 opt-in smoke로 분리한다.

## Phase 17: LAN Mode 제품 보안 정책

목표:

- Desktop Node LAN mode를 loopback 기본값, preview/admin opt-in, reverse proxy/TLS 전제, non-loopback static bearer auth, firewall admin opt-in lifecycle로 고정한다.

결정:

```text
DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required
```

참조 문서:

- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy-design.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy.md`

반영된 항목:

1. product plan과 manifest에 LAN security policy v1을 추가했다.
2. Local API runtime policy에 network policy를 추가했다.
3. non-loopback static Web Console bearer token policy를 테스트로 고정했다.
4. TLS는 product wrapper가 직접 제공하지 않고 reverse proxy 또는 외부 TLS terminator 전제로 노출한다.
5. firewall rule lifecycle은 installer 자동 적용이 아니라 product action 또는 manual command opt-in으로 유지한다.
6. LAN exposure, TLS, firewall stance를 diagnostics에 연결했다.

완료 조건:

- LAN mode는 explicit opt-in이고 기본 install로 열리지 않는다.
- non-loopback Web Console과 API auth policy가 같은 위협 모델을 따른다.
- firewall, token, TLS/reverse proxy 상태가 diagnostics에 남는다.

## Phase 18: Update/Rollback/Config Migration

목표:

- Phase 12/13의 best-effort update/rollback을 제품 수준의 version policy, config migration, rollback verification으로 고도화한다.

결정:

```text
DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration
```

참조 문서:

- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration-design.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration.md`

구현된 항목:

1. product plan과 installed `product-manifest.json`에 update policy v1을 포함한다.
2. installed manifest를 product root 버전의 단일 진실로 검증하고, invalid previous manifest는 rollback restore 전에 차단한다.
3. 기본 rollback slot은 `DesktopNode.previous` 하나로 제한하고, failed root는 diagnostics 수집을 위해 보존한다.
4. `Update` action은 service stop, product root backup, local payload copy, config migration dry-run, service start, health check 순서로 실행한다.
5. config migration, service start, health check 실패는 service start 차단 또는 previous root rollback 시도로 이어진다.
6. diagnostic bundle은 update policy, migration plan, rollback state artifact를 포함한다.
7. updater download/signature verification은 후속으로 두되, update policy는 installer signing/provenance stance를 따른다.

완료 조건:

- update 실패 시 service가 이전 정상 버전으로 돌아간다.
- rollback 후 runtime policy, job store, diagnostics가 일관된다.
- config migration 실패가 silent data loss로 이어지지 않는다.
- 관리자 mutating smoke에서 `Update -Version 0.18.0-admin-smoke`, `Rollback`, `CollectDiagnostics`가 통과하고, 결과를 Phase 18 plan의 `완료 증거`에 기록했다. 2026-05-01에는 `0.23.9-admin-smoke-baseline`에서 `0.23.9-admin-smoke-update`로 update/config migration dry-run을 실행한 뒤 rollback, CollectDiagnostics, cleanup까지 통과했고 `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`에 기록했다.

## Phase 19: 제품 승격 재판정

목표:

- Phase 11의 `keep-spike` 결정을 Phase 12-18 evidence 기준으로 다시 판정한다.

결정:

```text
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
```

참조 문서:

- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision.md`

판정 항목:

1. DPAPI protected token, JSONL diagnostics/redaction, LAN preview policy는 제품화 증거로 인정한다.
2. manifest-first update/rollback/config migration 기본 구현과 관리자 update/rollback smoke는 제품화 증거로 인정한다.
3. 서명된 릴리스 빌드 증거는 아직 GA 차단 게이트다.
4. 관리자 권한 MSI install/repair/uninstall/`REMOVE_DATA=1` 스모크는 아직 GA 차단 게이트다.
5. 서명/관리자 권한 제품 설치 흐름과 묶인 Hyper-V 수명주기 통합 증거는 아직 GA 차단 게이트다.
6. 릴리스/버전 정책과 장기 운영 로그 증거는 Phase 19 당시 GA 차단 게이트였다. Phase 22 후속 개발로 정책과 installer 계약 일부는 반영됐고 ADR-0002로 현재 적용 결정에 채택됐다. ADR-0003으로 내부 서비스용 internal trust signing evidence는 닫혔지만, stable 발행 증거는 아직 남아 있다.
7. Single Edge 공개 릴리스 경계와 Desktop Node 제품 경계는 계속 분리한다.

완료 조건:

- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지한다고 기록했다.
- `DEVELOPER_INDEX.md`, `DEVELOPMENT_VERIFICATION_POLICY.md`, `PUBLIC_RELEASE_BOUNDARY.md`, `follower.md`, component README가 같은 상태를 가리킨다.
- Root boundary/documentation sync guard가 Phase 19 상태와 stale backlog wording을 검증한다.

## Phase 24 이후: GA-ready gate 해소

Phase 19 이후 새 phase는 하나의 큰 GA 승격 작업으로 묶지 않고 증거 단위로 닫았다. Phase 25는 mixed runtime 전환 후보와 .NET service host replacement slice를 문서화했고, Phase 26/ADR-0004는 내부 전용 GA-ready 제품 런타임 current decision을 적용했다.

1. Phase 20 public trusted/stable signing evidence 또는 release approval: release approval evidence 기록 완료. Local test `RequireSigned` RC build/lifecycle evidence는 반복 확인했고, internal enterprise `RequireSigned` RC build/lifecycle evidence도 완료했지만 public trusted signing evidence는 없음.
2. Phase 22 stable 발행 증거와 릴리스 승인을 별도 게이트로 유지. 2026-05-01 local environment에는 public trusted certificate/PFX/private key가 없어 외부 stable publication은 실행하지 않음.
3. Phase 23 장기 운영 증거 실제 실행과 Event Log writer/provider 전환 여부 판정: draft-ready evidence와 2026-05-01 운영 hardening evidence 완료, JSONL-first 유지.
4. enabled firewall smoke rule 유지/제거 결정: 제거 완료.
5. Phase 25 .NET contract/runtime core, TypeScript Web Console, PowerShell adapter 전환 후보와 .NET service host replacement를 product evidence로 검증했다. `0.26.0-admin-smoke`와 후속 admin-smoke evidence는 public trusted/stable signing evidence가 아니라 내부/admin-smoke supporting evidence다.
6. 2026-05-05 aggregate closure로 GA-scope blocked row 0개, PowerShell-backed current owner 0개, active product `spikes/**` reference 0개, internal stable release/update/rollback evidence pass를 기록했다.
7. Phase 26 GA-ready alignment는 ADR-0004와 `docs/ga-ready/**` matrix 문서를 current decision supporting docs로 바꾸며 PowerShell-free product ops/runtime 목표 상태를 내부 전용 서비스 범위에서 적용했다.

관련 후속 gate와 Phase 25 Web Console 경계:

- Draft PR ready 전환 판단은 `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-draft-pr-ready-gate.md`를 따른다.
- TypeScript Web Console 후속 구현 판단은 `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`의 static asset parity-first 경계를 따른다. 2026-05-03 served asset/root migration 이후 source, generated parity manifest, parity verification script는 repo-root `web/**`에 있고 served `web/app.js`는 `web/src/served-app.ts` build output이다.
- .NET Windows Service Host replacement 판단은 `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`와 `docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md`를 따른다. Hyper-V helper operation route parity 시작 slice는 `src/DesktopNode.Api/**` request processor와 `src/DesktopNode.Host/**` body/helper/job-store forwarding에 반영됐다. Native read routes, VM create/start/shutdown/poweroff/restart/delete, checkpoint create/restore/delete native mutation adapter는 `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-dotnet-native-network-inventory-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-list-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-detail-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-list-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-power-state-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-mutation-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-restore-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-create-shutdown-restart-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-delete-native-adapter.md`를 따른다. Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에 기록됐다.
- Phase 26/ADR-0004 현행 판단은 `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`, `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-ga-ready-phase26-alignment.md`, `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`를 따른다.
