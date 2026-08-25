# PureCVisor Desktop Node Local API + Web Console Spike

이 spike는 PureCVisor Desktop Node의 Phase 2A/2B/2C/2D/2E/2F/2G/2H Local API daemon 경계, Phase 3A/3B/10 static Web Console 경계, Phase 4/10/15 CLI-facing route 계약, Phase 5 explicit LAN mode hardening 경계, Phase 6 Windows service packaging handoff, Phase 7 service token file hardening 경계, Phase 8 installer hardening 경계, Phase 9 Local API runtime hardening 경계, Phase 11 제품 런타임 승격 보류 경계, Phase 13 loopback static auth boundary, Phase 14 MSI installer와 product wrapper service/data configuration 경계, Phase 15 protected token source 경계, Phase 16 JSONL first diagnostics 경계, Phase 17 LAN security policy 경계, Phase 18 update/rollback/config migration product wrapper 경계, Phase 19 제품 승격 재판정 경계, Phase 24 Local API job runtime boundary 후보, Phase 25 .NET/TypeScript 전환 후보를 검증한다. 2026-05-01 replacement slice 이후 제품 설치 기본 service host는 `DesktopNode.Host.exe`이고, 후속 native adapter slices 이후 current served Hyper-V routes는 `src/DesktopNode.Api/**`의 native adapter가 처리한다. 이 PowerShell Local API는 component/adapter 기준과 parity baseline으로 유지한다.

Related docs:

- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3a-web-console-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase4-cli-mvp-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase5-lan-mode-hardening-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase6-service-packaging-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase7-service-token-file-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase8-installer-hardening-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase9-local-api-runtime-hardening-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase10-web-cli-productization-design.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
- `docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper-design.md`
- `docs/superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage-design.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics-design.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy-design.md`
- `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration-design.md`
- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`
- `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2a-local-api.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2b-job-api.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2c-worker-queue.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2d-persisted-jobs.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2e-job-control.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2f-static-web-console.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2g-api-token.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2h-worker-pool.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3a-web-console.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase4-cli-mvp.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase5-lan-mode-hardening.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase6-service-packaging.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase7-service-token-file.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase8-installer-hardening.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase9-local-api-runtime-hardening.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase10-web-cli-productization.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision.md`
- `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md`
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy.md`
- `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision.md`
- `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md`
- `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md`
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
- `archive/spikes/purecvisor-desktop-node/README.md`
- `archive/spikes/purecvisor-desktop-node/hyperv/README.md`
- `archive/spikes/purecvisor-desktop-node/cli/README.md`
- `archive/spikes/purecvisor-desktop-node/service/README.md`

Example requests:

- `archive/spikes/purecvisor-desktop-node/api/examples/host-status.http.txt`
- `archive/spikes/purecvisor-desktop-node/api/examples/vm-list.http.txt`
- `archive/spikes/purecvisor-desktop-node/api/examples/vm-detail.http.txt`
- `archive/spikes/purecvisor-desktop-node/api/examples/vm-create.http.txt`
- `archive/spikes/purecvisor-desktop-node/api/examples/vm-start.http.txt`

Phase 24 기준 Local API 상태: local API skeleton, VM create job 계약, FIFO worker queue, bounded worker-pool tick, optional JSON job persistence, queued job cancellation, failed job retry with attempt limit, runtime policy route, Phase 24 `job_runtime` boundary contract, read-only `network.inventory` route, optional static file serving, optional bearer-token/token-file/protected-token-file gate, explicit LAN opt-in, JSONL event log, opt-in Windows Firewall rule management, service token file preparation handoff가 완료됐다. Phase 16은 `events.jsonl`을 Local API listener/firewall/runtime event의 1차 운영 로그로 유지한다. Phase 17은 LAN mode를 product security policy와 runtime policy network object로 고정한다. daemon은 여전히 격리된 spike이며 Linux `purecvisorsd` runtime에 연결되지 않는다.

Phase 3A/10 status: a bundled static Web Console lives in `web/`. It renders host status, VM inventory, a VM create job form, browser-local tracked jobs, job cancel/retry controls, checkpoint controls, and optional bearer token requests against the existing Local API routes.

Phase 3B status: the Local API now exposes VM detail and queued lifecycle job routes, and the bundled Web Console adds a VM detail drawer with lifecycle job actions. VM detail uses the Hyper-V inventory contract fields `memory.assigned_mb`, `generation`, `storage[].attached`, `checkpoints.count`, and `console.available_local` when available.

Phase 4 status: the Local API exposes checkpoint list/create/restore/delete routes used by the Desktop Node CLI MVP. Checkpoint mutations are queued jobs, matching VM create and lifecycle job semantics.

Phase 5 status: the listener remains loopback-only by default. Non-loopback prefixes require `-AllowLan` and a non-empty token source. `-EventLogPath` writes JSONL listener/firewall events, and `-EnsureFirewallRule` can ensure an inbound Windows Firewall TCP rule when LAN mode is intentionally enabled.

Phase 6 status: Windows service packaging lives in `archive/spikes/purecvisor-desktop-node/service/`. It builds `sc.exe` commands that run this Local API listener through `pwsh.exe`, supports `-WhatIf` preview, and keeps actual service installation as an elevated opt-in action.

Phase 7 status: `-ApiTokenFile` lets the listener read a bearer token from a file at startup. Inline `-ApiToken` and `-ApiTokenFile` are mutually exclusive. Service packaging can pass the token file path into the service binary path so a long-lived token value does not appear directly in the Windows service command line.

Phase 8 상태: service packaging은 기본 `%ProgramData%\PureCVisor\desktop-node\api-token.txt` token file을 준비하고, 난수 token을 stdout에 출력하지 않은 채 생성하며, 관리자와 service account용 `icacls.exe` ACL command를 만들고, elevated install path를 preview할 수 있다. 실제 service installation, firewall 적용, ACL inspection은 계속 관리자 opt-in smoke 단계로 분리한다.

Phase 9 상태: Local API는 `GET /api/v1/runtime/policy`로 persistence, retry, cancel, worker, CORS, auth, token storage 결정을 노출한다. JSON file persistence, manual retry, queued-only cancel, bounded worker tick, no CORS/OPTIONS, single bearer token auth를 현재 정책으로 고정하고, `attempt=3` failed job의 manual retry는 `PCV_JOB_RETRY_LIMIT_REACHED`로 거부한다.

Phase 10 상태: bundled Web Console은 VM detail panel에서 checkpoint list/create/restore/delete controls를 제공하고, tracked job history를 browser `localStorage`에 최대 50개까지 저장한다. CLI는 `--token-file`을 지원해 Phase 7/8 token file 정책과 같은 token source를 사용할 수 있다.

Phase 19 상태: Desktop Node는 제품 런타임으로 승격하지 않고 `archive/spikes/purecvisor-desktop-node/**` 격리 spike로 유지한다. 결정 표식은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`와 `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`다. DPAPI protected token, JSONL diagnostics/redaction, LAN preview policy, manifest-first update/rollback/config migration은 제품화 증거로 인정한다. 2026-04-30 local signed RC/elevated MSI lifecycle, Hyper-V product-flow lifecycle, release approval/signing preflight, firewall cleanup, 운영/Event Log source lifecycle evidence는 draft-ready 기준으로 기록됐다. 2026-05-01에는 current-head `3d35aa2` 기준 local test `RequireSigned` MSI lifecycle과 product-wrapper update/rollback/config migration smoke도 기록했다. Public trusted signing, stable publication, GA 제품 런타임 승격은 별도 판단으로 남는다.

Phase 13 상태: product wrapper는 당시 WinSW service wrapper를 service host로 사용했다. Loopback listener에서는 Web Console static asset을 bearer token 없이 열 수 있고, API route는 계속 bearer token을 요구한다. Non-loopback LAN mode에서는 static asset도 bearer token 정책을 유지한다. 이 loopback static auth 경계는 Phase 25 .NET Host replacement 이후에도 유지된다.

Phase 14 상태: `packaging/windows-desktop-node/installer/`가 WiX MSI-first installer source/build/provenance를 제공한다. MSI는 Program Files 제품 파일 설치/repair/제거를 소유하고, product wrapper는 `ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled`로 service/data configuration만 수행한다. Unsigned dev MSI build는 개발 toolchain에서 검증됐고, 2026-04-30 `0.23.8-rc.1` local test certificate signed RC MSI와 elevated `msiexec` install/repair/uninstall/`REMOVE_DATA=1` lifecycle evidence가 기록됐다. 2026-05-01 `0.23.9-rc.1` local test `RequireSigned` MSI도 current-head `3d35aa2`에서 빌드/서명/검증했고 같은 lifecycle이 전부 exit `0`으로 통과했다. 이 evidence는 public trusted/stable signing 또는 GA 승격 evidence가 아니다.

Phase 15 상태: 제품 wrapper 기본 bearer token source는 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` protected token file이다. Local API는 `-ApiTokenProtectedFile`을 읽고 runtime policy에 `token_storage = dpapi-local-machine`을 노출한다. 기존 `-ApiTokenFile`은 호환 경로로 남기되 product wrapper 기본값에서는 사용하지 않는다.

Phase 16 상태: JSONL event log는 Event Log로 대체하지 않는다. Product wrapper의 diagnostics policy v1이 `events.jsonl`, `install.jsonl`, service host logs rotation/retention과 diagnostic bundle manifest를 소유하고, Windows Event Log source 등록은 관리자 opt-in plan으로만 제공한다.

Phase 17 상태: LAN mode는 기본 설치에서 열리지 않는다. 제품 보안 정책은 loopback-only 기본값, LAN preview/admin opt-in, reverse proxy 또는 외부 TLS terminator 전제, non-loopback static bearer auth, firewall admin opt-in lifecycle을 명시한다. Product wrapper는 TLS endpoint를 직접 제공하지 않는다.

Phase 18 상태: update/rollback/config migration은 Local API route가 아니라 product wrapper 구현 경계다. Installed `product-manifest.json`을 product root 버전의 단일 진실로 두고, safe update, 단일 previous root rollback slot, config migration validation, job store non-destructive compatibility를 packaging wrapper에서 검증한다. 관리자 update/rollback smoke도 product wrapper plan의 `완료 증거`에 기록하며, Local API route surface는 추가하지 않는다.

Phase 24 상태: Local API job runtime은 `GET /api/v1/runtime/policy`의 `job_runtime` object로 public boundary를 노출한다. 현재 owner는 `local-api`, state store는 script-scope memory와 JSON file snapshot, dispatch는 bounded synchronous worker tick, host mutation은 Hyper-V helper process boundary 뒤로 고정한다. Persisted `running` job은 restart 이후 자동 재개하지 않고 `PCV_JOB_INTERRUPTED` retryable failure로 복구하며 queue에 다시 넣지 않는다. Future job store version은 조용히 로드하지 않고 `.unsupported.<version>.<timestamp>` quarantine 후 빈 store로 시작한다. Cancel은 queued job에만 허용하고, retry는 failed job 중 `error.retryable = true`인 경우에만 새 queued job으로 허용한다. 기본 구현 방향은 PowerShell orchestration, Pester contract, injectable runner, diagnostics evidence이며, C++23 native core는 state machine 또는 supervision 문제가 PowerShell 경계를 넘어설 때만 재검토한다.

Phase 24 후속 slice는 `GET /api/v1/network/inventory`를 read-only helper route로 추가했다. 이 route는 `network.inventory` operation을 호출해 Hyper-V switch inventory를 반환하며, switch 생성/수정/삭제 같은 host mutation은 포함하지 않는다.

Phase 25 상태: `.NET contract mirror`, 순수 .NET job state transition validator, .NET API host candidate contract, .NET Service host contract, TypeScript Web Console static parity scaffold/generated manifest/verification flow가 추가됐다. `src/DesktopNode.Host/**`는 제품 기본 `DesktopNode.Host.exe listen`과 MSI `service-action` runner를 제공한다. `src/DesktopNode.Api/**`는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation routes, checkpoint create/restore/delete native mutation routes, job get/cancel/retry, JSON job store save/load/recovery를 처리하는 route parity slice를 포함한다. 2026-05-03 후속 slice에서 .NET Host의 `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native adapter가 직접 처리하며 native parity failure는 PowerShell helper fallback 없이 structured failure로 반환한다. VM create/start/shutdown/poweroff/restart/delete는 C# WMI adapter가 직접 실행하고, checkpoint create/restore/delete는 C# WMI snapshot service adapter가 직접 실행한다. Native VM create product path는 Hyper-V Generation 2만 지원하며 Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다. Native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. TypeScript Web Console은 repo-root `web/**`로 이동했고 `web/src/served-app.ts`가 `web/app.js`를 생성하며, `npm test --prefix web`와 `npm run verify:parity --prefix web`로 static asset parity를 검증한다.

## Supported Host

- Windows 10 or Windows 11 Pro, Enterprise, or Education
- PowerShell 7 available as `pwsh`
- Hyper-V helper available at `archive/spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1`

## Endpoints

| Method | Path | Operation |
|--------|------|------------------|
| `GET` | `/api/v1/host/status` | `host.status` |
| `GET` | `/api/v1/network/inventory` | `network.inventory` |
| `GET` | `/api/v1/vms` | `vm.list` |
| `GET` | `/api/v1/vms/{id}` | VM detail from `vm.list` inventory |
| `POST` | `/api/v1/vms` | `vm.create` through a job |
| `POST` | `/api/v1/vms/{id}/start` | queued native `vm.start` lifecycle job |
| `POST` | `/api/v1/vms/{id}/shutdown` | queued `vm.shutdown` lifecycle job |
| `POST` | `/api/v1/vms/{id}/poweroff` | queued native `vm.poweroff` lifecycle job |
| `POST` | `/api/v1/vms/{id}/restart` | queued `vm.restart` lifecycle job |
| `DELETE` | `/api/v1/vms/{id}` | queued native `vm.delete` lifecycle job |
| `GET` | `/api/v1/vms/{id}/checkpoints` | `checkpoint.list` |
| `POST` | `/api/v1/vms/{id}/checkpoints` | queued native `checkpoint.create` job |
| `POST` | `/api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore` | queued native `checkpoint.restore` job |
| `DELETE` | `/api/v1/vms/{id}/checkpoints/{checkpoint_id}` | queued native `checkpoint.delete` job |
| `GET` | `/api/v1/jobs/{job_id}` | job lookup |
| `POST` | `/api/v1/jobs/{job_id}/cancel` | cancel queued job |
| `POST` | `/api/v1/jobs/{job_id}/retry` | retry failed job as a new job |
| `GET` | `/api/v1/runtime/policy` | Local API runtime policy decisions |
| `GET` | `/` | static `index.html` when `-WebRootPath` is supplied |
| `GET` | `/{asset}` | static Web Console assets when `-WebRootPath` is supplied |

By default, the listener accepts only loopback prefixes:

- `http://127.0.0.1:<port>/`
- `http://localhost:<port>/`
- `http://[::1]:<port>/`

LAN binding is available only as an explicit Phase 5 opt-in:

- pass a non-loopback `-Prefix`
- pass `-AllowLan`
- pass a non-empty `-ApiToken`, `-ApiTokenFile`, or `-ApiTokenProtectedFile`
- optionally pass `-EventLogPath`
- optionally pass `-EnsureFirewallRule`

Windows service installation의 실제 실행, database-backed persistence, automatic retry policy/backoff 실행, running helper interruption, runspace/threaded worker execution, CORS/OPTIONS handling, Windows Credential Manager 전환, multi-user auth, server-side browser job history sync, VMConnect launch, shell completion, public trusted/stable signing, network download updater, Windows Event Log writer/provider 전환, 내장 TLS certificate lifecycle, reverse proxy integration smoke, service recovery, C++23 native job runtime 구현은 Phase 19 GA 차단 gate 또는 별도 후속 단계로 분리한다. Phase 18의 local payload update/rollback smoke는 product wrapper 경계에서만 검증하고 Local API endpoint로 노출하지 않는다.

## Non-Integration Tests

Run the local API contract suite without opening a listener:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

현재 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

Run the CLI contract suite:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
```

현재 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

Run the static Web Console contract suite and JavaScript syntax check:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
```

현재 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다. Node syntax check는 exit 0을 기대한다.

## Start Local API

Run from the repository root:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/'
```

To persist jobs across restarts, pass `-JobStorePath`:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -JobStorePath "$env:TEMP\pcv-desktop-jobs.json"
```

To serve a static Web Console directory on the same loopback listener, pass `-WebRootPath`:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -WebRootPath "web"
```

To require a bearer token for API routes, pass `-ApiToken`. On loopback prefixes, static Web Console assets remain loadable without a bearer token so the browser can open `/` before the user enters a token. On non-loopback LAN prefixes, static assets stay behind the same bearer token policy. Inline `-ApiToken` is a short-lived developer/manual smoke path only; product or installed service paths must use `-ApiTokenProtectedFile` so long-lived bearer token values do not appear in command lines.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -ApiToken 'change-me'
```

For long-lived product service usage, create a protected token file and pass `-ApiTokenProtectedFile`. The listener reads the DPAPI LocalMachine protected token metadata and rejects missing, empty, invalid, or ambiguous token sources:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json'
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json'
```

For legacy or explicit developer usage, store the token in a plain file and pass `-ApiTokenFile` instead. The listener trims trailing newlines and rejects missing, empty, or ambiguous token sources:

```powershell
New-Item -ItemType Directory -Path 'D:\PureCVisor\desktop-node' -Force | Out-Null
Set-Content -LiteralPath 'D:\PureCVisor\desktop-node\api-token.txt' -Value 'replace-with-a-long-random-token' -Encoding UTF8 -NoNewline
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -ApiTokenFile 'D:\PureCVisor\desktop-node\api-token.txt'
```

Phase 15 service helper로 기본 protected token file을 준비할 수 있다. 이 명령은 token 값을 JSON 출력에 포함하지 않고, 기본 위치는 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`이다. 실제 실행은 token file ACL을 적용하므로 elevated PowerShell에서 수행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile
```

관리자 권한 없이 preview만 확인하려면 `-WhatIf`를 사용한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile -WhatIf
```

To write listener lifecycle and firewall events as JSONL, pass `-EventLogPath`:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -EventLogPath "$env:TEMP\pcv-desktop-api-events.jsonl"
```

To intentionally expose the listener on the LAN, pass `-AllowLan` and a token source together. `-ApiTokenProtectedFile` is preferred for service-style product launches:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 `
  -Prefix 'http://0.0.0.0:7777/' `
  -AllowLan `
  -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json' `
  -EventLogPath "$env:TEMP\pcv-desktop-api-events.jsonl"
```

To also ensure a Windows Firewall inbound TCP rule for the listener port, add `-EnsureFirewallRule`. This operation normally requires an elevated PowerShell session:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 `
  -Prefix 'http://0.0.0.0:7777/' `
  -AllowLan `
  -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json' `
  -EnsureFirewallRule `
  -FirewallRuleName 'PureCVisor Desktop Node API' `
  -FirewallProfile private `
  -EventLogPath "$env:TEMP\pcv-desktop-api-events.jsonl"
```

To process more than one queued job after each listener response, pass `-WorkerCount`:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -WorkerCount 4
```

Then call:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/host/status'
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/vms'
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm'
```

Replace `demo-vm` with a VM id or name returned by `/api/v1/vms`.

With `-ApiToken`, `-ApiTokenFile`, or `-ApiTokenProtectedFile` enabled, include `Authorization: Bearer <token>`:

```powershell
$headers = @{ Authorization = 'Bearer change-me' }
Invoke-RestMethod -Headers $headers -Uri 'http://127.0.0.1:7777/api/v1/host/status'
Invoke-RestMethod -Headers $headers -Uri 'http://127.0.0.1:7777/api/v1/vms'
Invoke-RestMethod -Headers $headers -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm'
```

The API response body preserves the helper response shape:

```json
{
  "ok": true,
  "operation": "host.status",
  "data": {},
  "error": null
}
```

Unsupported methods return `405` with `PCV_METHOD_NOT_ALLOWED`. Unsupported routes return `404` with `PCV_ROUTE_NOT_FOUND`.

## Runtime Policy

Read the current Local API runtime decisions:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/runtime/policy'
```

The route is read-only and returns the current spike decisions for JSON file persistence, no automatic retry, manual retry max attempts, queued-only cancel, bounded worker tick, no CORS/OPTIONS, and single bearer-token auth backed by the configured token storage. Product wrapper defaults report `token_storage = dpapi-local-machine`. Phase 24 adds `job_runtime` to make the job runtime boundary explicit: `contract_version = 1`, `owner = local-api`, `state_store.backend = script-scope-memory`, `state_store.persistence = json-file-snapshot`, `state_store.corrupt_store = quarantine-and-start-empty`, `state_store.unsupported_future_version = quarantine-and-start-empty`, `dispatch.mode = bounded-synchronous-worker-tick`, `dispatch.helper_boundary = hyperv-helper-process`, `control.cancel.queued_only = true`, `control.cancel.running_interrupt = false`, `control.retry.manual_only = true`, `control.retry.failed_error_retryable_only = true`, `control.retry.max_attempts = 3`, `control.retry.creates_new_job = true`, `host_mutation = helper-process-only`, `orchestration.primary = powershell`, `orchestration.contract = plan-contract-injectable-runner-diagnostics`, `native_core.status = not-planned-unless-runtime-boundary-deepens`, `native_core.reason = windows-hyperv-orchestration-not-dataplane`, `native_core.revisit_when = state-machine-or-supervision-outgrows-powershell`, `managed_core.status = service-host-default`, and `managed_core.host_replacement = dotnet-windows-service-host`. Phase 25 .NET Host runtime policy reports the current native boundary separately: `dispatch.helper_boundary = dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation`, `dispatch.native_probe_operations = [host.status,network.inventory,vm.list,checkpoint.list]`, `dispatch.native_mutation_operations = [vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete]`, `dispatch.mutation_dispatch = native-vm-create-lifecycle-delete-checkpoint-mutation`, `host_mutation = native-read-routes-vm-create-lifecycle-delete-and-checkpoint-mutation`, `orchestration.primary = dotnet`, `orchestration.contract = dotnet-native-adapter-contract-tests-admin-smoke`, `native_core.status = read-route-vm-create-lifecycle-and-checkpoint-mutation-started`, and `native_core.reason = host.status,network.inventory,vm.list,checkpoint.list,vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete`. Non-GET requests to this route return `405` with `PCV_METHOD_NOT_ALLOWED`.

## Optional API Token

The listener is unauthenticated by default for local development. When `-ApiToken`, `-ApiTokenFile`, or `-ApiTokenProtectedFile` is set, every request must include an `Authorization` header in `Bearer <token>` form before route handling, helper execution, job mutation, or static file reads.

Missing or malformed credentials return `401` with `PCV_AUTH_REQUIRED`. A bearer token that does not match the configured token returns `403` with `PCV_AUTH_FORBIDDEN`.

LAN mode is stricter than loopback mode. A non-loopback prefix without `-AllowLan` is rejected with `PCV_PREFIX_NOT_LOOPBACK`; a non-loopback prefix with `-AllowLan` but no token source is rejected with `PCV_LAN_TOKEN_REQUIRED`.

`-ApiToken`, `-ApiTokenFile`, and `-ApiTokenProtectedFile` are mutually exclusive. Supplying more than one returns `PCV_API_TOKEN_CONFLICT`. A missing plain token file returns `PCV_API_TOKEN_FILE_NOT_FOUND`, and an empty plain token file returns `PCV_API_TOKEN_FILE_EMPTY`. Protected token file read failures are returned as `PCV_SERVICE_PROTECTED_TOKEN_*` errors from the service token helper.

## Create VM Job

Submit a VM create request. The request returns a queued job before the native worker executes it. The .NET native product path currently supports Hyper-V Generation 2 only:

```powershell
$body = Get-Content archive/spikes/purecvisor-desktop-node/api/examples/vm-create.http.txt -Raw
$json = ($body -split '\r?\n\r?\n', 2)[1]
$job = Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7777/api/v1/vms' -ContentType 'application/json' -Body $json
$job.data.job_id
```

Check the stored job:

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($job.data.job_id)"
```

## VM Detail And Lifecycle Jobs

Read a VM detail record from the `vm.list` inventory by VM id or name:

```powershell
$vm = Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm'
$vm.data.name
```

In the examples, replace `demo-vm` with a VM id or name returned by `/api/v1/vms`.

Queue a lifecycle job. The response returns a job before the helper runs:

```powershell
$startJob = Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm/start'
$startJob.data.job_id
```

Check the lifecycle job with the existing job lookup route:

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($startJob.data.job_id)"
```

## Checkpoint Routes

List checkpoints for a VM:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm/checkpoints'
```

Queue a checkpoint create job:

```powershell
$checkpointBody = @{ name = 'before-upgrade' } | ConvertTo-Json
$checkpointJob = Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm/checkpoints' -ContentType 'application/json' -Body $checkpointBody
$checkpointJob.data.job_id
```

Queue checkpoint restore and delete jobs:

```powershell
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm/checkpoints/before-upgrade/restore'
Invoke-RestMethod -Method Delete -Uri 'http://127.0.0.1:7777/api/v1/vms/demo-vm/checkpoints/before-upgrade'
```

Checkpoint create/restore/delete routes return queued jobs. Check their final state through `GET /api/v1/jobs/{job_id}`.

Phase 2H stores jobs in memory and can also persist them to a JSON file when `-JobStorePath` is supplied. In the listener spike, a bounded worker-pool tick is run after each HTTP response is sent; in tests, `Invoke-PcvApiWorkerTick` and `Invoke-PcvApiWorkerPoolTick` are called directly for deterministic verification.

If the persisted job store cannot be parsed, `Initialize-PcvApiJobStore` moves it to a `.corrupt.<timestamp>` quarantine path and starts with an empty store instead of crashing. If the store has a future `version` newer than the supported v1 schema, it moves the file to `.unsupported.<version>.<timestamp>`, returns `PCV_JOB_STORE_UNSUPPORTED_VERSION`, and starts empty instead of loading unknown state. Persisted jobs that were `running` when the API stopped are loaded as failed `PCV_JOB_INTERRUPTED` jobs with `retryable = true`; they are not re-enqueued automatically.

## Worker Pool Tick

`Invoke-PcvApiWorkerPoolTick -WorkerCount <n>` processes up to `n` queued jobs in FIFO order and returns `processed`, `processed_count`, `jobs`, and `remaining_queue`. `WorkerCount` defaults to `1`, preserving the original one-job worker tick behavior.

The Phase 2H pool is intentionally bounded and deterministic: it drains multiple jobs per tick but does not create PowerShell runspaces or background threads.

## Control Jobs

Cancel a queued job before the worker starts it:

```powershell
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:7777/api/v1/jobs/$($job.data.job_id)/cancel"
```

Only `queued` jobs can be canceled. Completed, failed, running, or already canceled jobs return `409` with `PCV_JOB_NOT_CANCELABLE`.

Retry a failed job as a new queued job:

```powershell
$retry = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:7777/api/v1/jobs/$($job.data.job_id)/retry"
$retry.data.retry_of
```

Only `failed` jobs with `error.retryable = true` can be retried. The original failed job remains unchanged; the retry response contains a new `job_id`, `retry_of`, and incremented `attempt`. Failed jobs with `error.retryable = false` return `409` with `PCV_JOB_NOT_RETRYABLE`. Manual retry is capped at attempt `3`; retrying a failed job that already reached attempt `3` returns `409` with `PCV_JOB_RETRY_LIMIT_REACHED`.

## Serve Static Web Console Files

When `-WebRootPath` is supplied, non-API `GET` requests are resolved under that directory:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/'
Invoke-WebRequest -Uri 'http://127.0.0.1:7777/app.js'
```

Static routes never override `/api/v1/...` routes. `GET /` maps to `<WebRootPath>\index.html`, and directory requests such as `GET /assets/` map to `<WebRootPath>\assets\index.html`.

The static server blocks parent-directory and drive-qualified path segments. Missing static files return `404` with `PCV_STATIC_FILE_NOT_FOUND`; forbidden paths return `403` with `PCV_STATIC_PATH_FORBIDDEN`.

## Bundled Static Web Console

Phase 3A/3B Web Console의 현재 제품 경로는 repo-root `web/`이다. Serve it from the API listener:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -WebRootPath "web"
```

Then open `http://127.0.0.1:7777/`. The app loads `/app.js` and `/styles.css`, defaults the API base URL to the current origin, and can send `Authorization: Bearer <token>` when the listener was started with `-ApiToken`, `-ApiTokenFile`, or `-ApiTokenProtectedFile`.

The Phase 10 screen includes host status, VM inventory, a VM detail drawer, VM create job submission, lifecycle job actions, checkpoint controls, browser-local tracked job history, queued job cancellation, and failed job retry. It remains isolated from the Linux Single Edge `ui/` tree.
