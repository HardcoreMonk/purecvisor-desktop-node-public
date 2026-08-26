# GA-ready 검증 Ownership

이 문서는 ADR-0004 적용 이후 내부 전용 GA-ready 제품 런타임에서 제품 검증 primary owner를 Pester 중심 legacy suite에서 xUnit/npm/browser-level fixture/package contract 중심으로 옮긴 기준을 고정한다.

현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`과 `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`다. 이 문서는 ADR-0004의 verification ownership supporting contract다.

verification_ownership_replacement_status: pass
last_updated_at: 2026-05-10T00:00:00+09:00
active_operator_surface_decision: cli-web-only
tui_product_status: removed-from-active-product
tui_removal_code_level_evidence: docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md
installed_operator_surface_promotion: pass-0.42.74-admin-smoke
current_required_ci_final_main_sha: 6e2bdb93ce308b632c929e2c17f5550ac3845401
current_required_ci_run_id: 32904006595
current_required_ci_contexts: dotnet,web,delivery,installer-policy
current_required_ci_provider_required: true
current_public_boundary_residue_run_id: 32904006619
current_public_boundary_residue_job_id: 97983888524
current_public_boundary_residue_provider_required: false

## Ownership Rule

- .NET product path는 xUnit과 installed smoke contract를 primary로 둔다.
- Web Console은 npm과 TypeScript typecheck를 primary로 두며, 현재 첫 browser-level fixture는 npm/package-owned Node `vm` 최소 DOM smoke로 제한한다.
- Browser-level fixture 구현 도구는 첫 implementation slice에서 Node built-ins로 선택했다. 실제 browser automation은 후속 후보로만 둔다.
- Playwright는 후속 도구 후보이며 이 alignment slice에서 dependency로 도입하지 않는다.
- Packaging은 installer/package contract tests와 signed/unsigned channel policy tests를 primary로 둔다.
- Operator smoke는 no-auto-reboot, install/repair/uninstall/remove-data, update/rollback, diagnostics bundle, cleanup evidence를 유지한다.
- Pester는 PowerShell component/runtime behavior suite에서 archive compatibility verification으로 축소한다.
- Root documentation guard와 policy synchronization guard는 component/archive baseline으로 유지하되, 기본 개발 loop의 active required command에는 직접 넣지 않는다.

## Pester Retirement Gate

- 2026-05-04 repo migration active path removal 후속 slice 이후 기본 개발 loop의 active required command는 product-owned package/xUnit/npm 검증으로 제한한다.
- Legacy Pester suite는 component/archive baseline으로 분리하며, active product required command로 직접 호출하지 않는다.
- Suite별 retirement는 대체 xUnit/npm/package/browser fixture evidence가 생긴 뒤에만 허용한다.
- 각 retirement는 owner replacement, equivalent coverage mapping, archive baseline path, docs command update, CI/local command replacement, rollback 기준을 기록해야 한다.
- 2026-05-04T23:37:43+09:00 기준 product primary verification replacement는 pass다. Legacy suite는 component/archive baseline으로만 남고, product path coverage는 아래 equivalent coverage map의 xUnit/npm/package/post-reboot owners가 소유한다.
- PowerShell helper 또는 `spikes/**`가 active product path에 남아 있으면 해당 product area는 migration blocked로 유지한다. 단 해당 legacy Pester suite는 product primary가 아니라 component/archive baseline으로 분리할 수 있다.

## Default Command Ownership

아래 Required shard의 `--no-build --no-restore`는 clean-checkout prerequisite가 완료됐다는
뜻이다. Exact four는 clean committed HEAD에서만 실행하며, 먼저 `git status --short`가 빈
출력인지 확인한 뒤 다음을 수행한다.

```cmd
git status --short
dotnet restore src\DesktopNode.sln
dotnet build src\DesktopNode.sln -c Release --no-restore
npm ci --prefix web
```

| Command 영역 | 활성 command owner | 활성 command |
|---|---|---|
| Required `dotnet` | `src/DesktopNode.Verification`, .NET product tests | `dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-gates-dotnet --shard dotnet` |
| Required `web` | `src/DesktopNode.Verification`, Web contracts | `dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/development-gates-web --shard web` |
| Required `delivery` | `src/DesktopNode.Verification`, Packaging delivery contracts | `dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/development-gates-delivery --shard delivery` |
| Required `installer-policy` | `src/DesktopNode.Verification`, installer policy contracts | `dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/development-gates-installer-policy --shard installer-policy` |
| .NET product path | `src/DesktopNode.sln` | `dotnet test src/DesktopNode.sln` |
| Diff hygiene | repository root | `git diff --check` |

Final `main` SHA `6e2bdb93ce308b632c929e2c17f5550ac3845401`의 Development Gates run
`32904006595`가 위 exact four provider-required contexts를 PASS했다. Required workflow의
executable Pester 및 비관리자 PowerShell invocation은 `0`이다. `Invoke-Pester` 기반 legacy suite,
manual/admin scripts, 별도 Public Boundary run `32904006619` / job `97983888524`는 local/manual 또는
non-required residue로만 남으며 repository-wide Pester/PowerShell 제거를 뜻하지 않는다. Required
`web` shard는 verification catalog 내부에서 `npm run test:required --prefix web`를 이미 실행하므로
별도 Required 명령으로 중복 호출하지 않는다.

## Component/Archive Baseline

| Legacy suite | Baseline owner | 현재 상태 | 활성 product command 상태 |
|---|---|---|---|
| `archive/spikes/purecvisor-desktop-node/api/tests` | PowerShell Local API component | component/archive baseline | excluded from default required command |
| `archive/spikes/purecvisor-desktop-node/service/tests` | PowerShell service helper component | component/archive baseline | excluded from default required command |
| `archive/spikes/purecvisor-desktop-node/cli/tests` | PowerShell CLI component | component/archive baseline | excluded from default required command |
| `archive/spikes/purecvisor-desktop-node/hyperv/tests` | PowerShell Hyper-V helper component | component/archive baseline | excluded from default required command |
| `archive/spikes/purecvisor-desktop-node/tests` | legacy documentation/root boundary guard | component/archive baseline | excluded from default required command |

2026-05-04 후속 slice에서 AGENTS, README, verification policy, public release boundary, follower의 기본 required verification command에서 direct spike Pester path를 제거했다. `PcvPostRebootVerification.psm1`의 `HyperVNonIntegration` profile도 active product profile에서 퇴역했고, 요청 시 `PCV_POST_REBOOT_PROFILE_RETIRED`로 실패한다.

## Equivalent Coverage Map

| Product 영역 | Legacy/component baseline | Product primary replacement | Replacement 상태 |
|---|---|---|---|
| Local API route contract, Account/RBAC/JWT, console capability, and job runtime | `archive/spikes/purecvisor-desktop-node/api/tests` | `src/DesktopNode.Api.Tests`, `src/DesktopNode.Contracts.Tests`, `src/DesktopNode.Host.Tests`, `web/tests`, installed route smoke ledger | pass |
| Hyper-V read and mutation behavior | `archive/spikes/purecvisor-desktop-node/hyperv/tests` | `src/DesktopNode.Api.Tests`, `src/DesktopNode.Host.Tests`, explicit admin opt-in installed smoke evidence | pass |
| Service lifecycle and SCM actions | `archive/spikes/purecvisor-desktop-node/service/tests` | `src/DesktopNode.Host.Tests`, `src/DesktopNode.Service.Tests`, `packaging/windows-desktop-node/tests` | pass |
| Protected token preparation and health auth | PowerShell service helper protected-token tests | `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`, `src/DesktopNode.Host.Tests`, route parity smoke protected-token self-test | pass |
| CLI command routing and Local API client behavior | `archive/spikes/purecvisor-desktop-node/cli/tests` | `src/DesktopNode.Cli/**`, `src/DesktopNode.Cli.Tests`, installer CLI payload tests, product manifest/update payload tests; installed command name is `pcvcli.exe` | pass |
| CLI/Web-only boundary | historical TUI evidence only | solution/package boundary tests, `src/DesktopNode.Cli.Tests`, Web verification, 0.42.74 installed current-card | pass-installed-0.42.74 |
| Web Console served asset behavior | legacy Web Console Pester/static parity | `web/tests`, `web/package.json`, `npm test --prefix web`, `npm run verify:parity --prefix web`, `npm run browser:fixture --prefix web` | pass |
| Packaging/MSI payload and installed root markers | legacy service module payload checks | `packaging/windows-desktop-node/tests`, `packaging/windows-desktop-node/installer/tests`, root-level `DesktopNode.Host.exe` installed marker checks | pass |
| Post-reboot product verification | retired Hyper-V non-integration profile | `ProductStatus`, `PackagingRegression`, continuation profiles without `spikes/**` command paths | pass |
| Root documentation guard | `archive/spikes/purecvisor-desktop-node/tests` | component/archive baseline only; not a product primary command | pass |

## Browser-level Fixture Contract

- Browser-level fixture는 Web Console package가 소유하는 npm/package-owned fixture로 둔다.
- 이 fixture는 실제 Local API listener를 띄우지 않는 package-owned loopback fixture로 분류한다.
- 현재 fixture와 static tests는 static `index.html`/served app load, dashboard render, deterministic `GET /api/v1/host/status`, `GET /api/v1/vms`, `GET /api/v1/jobs`, tracked `GET /api/v1/jobs/{id}`, Account/RBAC/JWT login/refresh/logout/session/RBAC route coverage와 session display, console capability/noVNC 기본 disabled, token/redaction 확인을 포함한다.
- 기존 browser fixture guard가 추적하던 static asset load, initial render, deterministic `GET /api/v1/runtime/policy` connection, optional bearer 401/200 handling도 같은 Web Console package-owned fixture 범위로 유지한다.
- 후속 browser-level fixture 후보는 real browser smoke 확장으로 남아 있다. Installed account login smoke execution은 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 PASS했고, noVNC WebSocket-to-VNC TCP bridge는 Host xUnit loopback TCP/WebSocket test와 `artifacts/target-backed-novnc-installed-streaming-smoke-20260510-0411` installed smoke가 함께 소유한다.
- Browser-level fixture의 제외 범위는 API route contract, route parity, Hyper-V, service/MSI/firewall/Event Log/trust store mutation, LAN exposure, Playwright required dependency다.
- Browser-level fixture는 API route contract, route parity, Hyper-V mutation, installer lifecycle, release signing 검증을 대체하지 않는다.
- Playwright는 이 fixture를 구현할 때 검토할 후속 도구 후보이며, 이 alignment slice의 required dependency가 아니다.

## Diagnostics and Redaction Boundary

- Diagnostics evidence는 diagnostics bundle manifest, `events.jsonl`, `install.jsonl`, service logs, lifecycle step name, exit code, redacted tool stdout/stderr, cleanup result를 포함할 수 있다.
- Diagnostics evidence는 raw bearer token, API token, `Authorization` header value, `api-token.dpapi.json` content, legacy raw token file content, account file content, JWT, refresh token, JWT signing key material, password, private key, PFX password, certificate secret material을 포함하면 안 된다.
- Release/signing diagnostics는 certificate file path, private key path, PFX password, signing tool secret arguments를 redacted value로만 기록한다.
- Path redaction은 repo root와 data root를 각각 `[REPO_ROOT]`, `[DATA_ROOT]` token으로 치환한다.
- Redaction evidence는 operation code, artifact name, sanitized path token, exit code, cleanup status처럼 secret 없는 troubleshooting field를 유지해야 한다.

## Data Root Lifecycle Boundary

- Program Files product root lifecycle과 ProgramData data root lifecycle은 분리한다.
- 기본 uninstall은 ProgramData data root를 보존한다.
- Repair는 protected token file, legacy raw token file, job store, `events.jsonl`, `install.jsonl`, diagnostics directory를 보존한다.
- `REMOVE_DATA=1` 또는 explicit `RemoveData`만 ProgramData delete target을 연다.
- `REMOVE_DATA=1` delete target은 `api-token.dpapi.json`, `api-token.txt`, `accounts.json`, `jwt-signing-key.txt`, `jobs.json`, `events.jsonl`, `install.jsonl`, diagnostics directory로 제한한다.
- Service host log directory는 현재 RemoveData delete target에 포함하지 않는다.
- WiX는 ProgramData path 계산만 담당하고 data-root ACL을 직접 소유하지 않는다.
- Product action `data_acl` policy가 sensitive token file ACL ownership, SYSTEM/Administrators boundary, `RemoveData` 전 ACL repair를 소유한다.
- ACL repair 대상 sensitive file은 `api-token.dpapi.json`, `api-token.txt`, `accounts.json`, `jwt-signing-key.txt`다.

## Map

| 영역 | 현재 검증 | 목표 검증 | 전환 규칙 |
|---|---|---|---|
| API contract | xUnit + component/archive baseline | xUnit + installed route smoke | route owner가 .NET이면 xUnit이 primary |
| Account/RBAC/JWT and console capability | xUnit + web static tests + installed account smoke | xUnit + npm/static parity + installed account smoke | 기본 bootstrap은 `no-default-account`; installed account login execution은 `installed-admin-smoke-pass`; noVNC bridge는 explicit target host/port 구성 전 disabled |
| CLI/Web-only operator surface | ADR-0011 source/package/docs boundary guard | CLI/Web verification + 현재 anchor의 installed current-card | TUI source/package/smoke는 active product에 재도입하지 않고 dated evidence만 보존한다. current-card의 현재 판은 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md` 생성 블록이 가리킨다 |
| Hyper-V read routes | xUnit + installed non-mutating smoke + Pester archive compatibility | xUnit adapter tests + installed non-mutating smoke | 2026-05-03 read-route fallback removal 이후 xUnit이 product path primary |
| Hyper-V mutation routes | xUnit job tests + admin opt-in + component/archive baseline | xUnit job tests + admin opt-in route smoke | destructive operation은 explicit opt-in 유지 |
| Web Console | web Pester static tests + npm parity + Node `vm` browser fixture | npm + TypeScript + package-owned browser fixture | served build output은 repo-root `web/**`가 소유 |
| Packaging/MSI | Pester installer tests | package contract tests + installed lifecycle smoke | PowerShell 제거 slice마다 package tests 갱신 |
| Release/signing | Pester/build script checks | channel/provenance/signing contract + signed lifecycle evidence | public/internal trust model 구분 유지 |

## Non-mutating Default

기본 검증은 host mutation을 실행하지 않는다. 실제 Hyper-V mutation, service install create, service configure update, service repair missing service recreation, service repair config drift correction, service uninstall stop/delete, product root removal preserve-data, service uninstall remove-data request, service start, service stop, firewall rule enable/removal, Event Log source registration/removal, trust store install/removal, MSI lifecycle은 explicit admin opt-in smoke에서만 실행한다. 2026-05-05 `0.34.1-admin-smoke`는 사용자 fast-mode opt-in으로 native firewall/LAN/internal trust-store gate를 실행한 milestone evidence이며, 같은 날 `0.35.7-admin-smoke` OS gate는 `docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0357.md`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`에서 Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store mutation을 당시 HEAD 기준으로 다시 확인했다. 두 건은 dated milestone이며 현재 판이 아니다. 현재 OS mutation gate 판은 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md` 생성 블록의 full admin host mutation evidence가 소유한다. 기본 개발 loop 또는 CI 기본 command에는 OS mutation을 추가하지 않는다.
