# PureCVisor Desktop Node guide 기반 운영/확장 backlog 설계

## 목적

이 문서는 `https://purecvisor.site/ui/guide.html`와 `https://purecvisor.site/ui/guide-content.md`의 Single Edge 운영 가이드에서 Desktop Node에 이식 가능한 운영/확장 패턴을 분류한다.

Desktop Node는 Windows/Hyper-V 내부 전용 서비스다. 따라서 이 문서는 Linux `purecvisorsd`, KVM/libvirt, LXC, ZFS, OVS/OVN, public release, public trusted signing, 외부 stable publication을 제품 범위로 가져오지 않는다.

## 적용 상태

```text
DESKTOP_NODE_GUIDE_BASED_EXPANSION_BACKLOG: windows-hyperv-ops-patterns-only
```

이 backlog는 ADR이 아니다. 후속 구현은 각 후보별 별도 spec/plan, 검증 owner, rollback/final-state proof 요구사항을 먼저 가져야 한다.

2026-05-07 재기준화 evidence는 `docs/ga-ready/evidence/packaging-operator-backlog-rebaseline-2026-05-07.md`에 기록한다. 이 evidence는 packaging/distribution future phase와 Operator/Web UX 완료 상태를 분리해 추적하며, packaging publication descriptor code-level partial을 제외한 실제 host mutation 또는 public release claim을 추가하지 않는다.

## 이미 처리된 항목

| 항목 | 상태 | 근거 |
|------|------|------|
| VM delete UI | implemented | `940999e Add VM delete action to web console` |
| Operator Activity / Troubleshooting P0 | implemented | `a01a4f5 Add read-only job activity list route`, `5ff3911 Add operator activity and troubleshooting console` |
| Monitoring/Auth/Checkpoint warning P1 | implemented | `afc831e Add read-only monitoring signals to web console` |
| Web UI Operator Workflow Polish / Quality Gate P2 | implemented | `7de0057 Add P2 operator workflow polish` |
| Web Dashboard Ops Cockpit 재설계 | implemented | spec `17c58b6 Document web dashboard ops cockpit redesign`, plan `2418234 Plan web dashboard ops cockpit redesign`, implementation `d5d0360 Add read-only ops summary route`, `f7bb595 Block ops summary on unsupported job store schema`, `41e9cf5 Redesign web console as ops cockpit`, `1178537 Cover ops cockpit summary fallback behavior`, `9fe2cba Align ops cockpit runtime policy summary fields` |
| API Operations Hardening P2 | implemented | `a7b3b33 Add API request job correlation` |
| Network Inventory Web View | implemented | `docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md` |
| Diagnostic Bundle Operator Handoff UI | implemented | `docs/ga-ready/evidence/web-console-diagnostic-bundle-ui-2026-05-07.md` |
| Job Activity Retention/Pagination Hardening | implemented | `docs/ga-ready/evidence/api-web-retention-pagination-hardening-2026-05-07.md` |
| Token Rotation Operator UX | implemented | `docs/ga-ready/evidence/web-console-token-rotation-ux-2026-05-07.md` |

VM delete UI는 guide의 비동기 destructive VM operation 패턴을 채택했지만, 새 Hyper-V runtime을 추가하지 않았다. Web Console은 기존 `DELETE /api/v1/vms/{id}` queued job route를 호출하고, running VM은 UI에서 먼저 차단하며, managed marker guard는 API가 authoritative boundary로 유지한다.

Operator Activity / Troubleshooting P0는 guide의 audit/job completion/troubleshooting 패턴을 read-only 운영 visibility로 채택했다. Local API는 `GET /api/v1/jobs` server-side job snapshot을 제공하고, Web Console은 Activity와 Troubleshooting surface를 추가했다. 이 구현은 OS/Hyper-V/provider mutation, Event Log registration, firewall/trust-store/LAN/MSI/service mutation을 실행하지 않는다.

Monitoring/Auth/Checkpoint warning P1은 guide의 monitoring, sustained condition alert, token lifecycle visibility, snapshot retention warning 패턴을 read-only 운영 신호로 채택했다. Web Console은 service/API, VMMS, active/failed job, token policy, LAN exposure, checkpoint warning, Token Rotation operator UX를 표시한다. Retention delete, service token rotation/revoke mutation, public metrics endpoint는 구현하지 않았다.

Web UI Operator Workflow Polish / Quality Gate P2는 guide의 command palette/status bar/quality gate 패턴 중 Desktop Node에 맞는 VM filter, safer destructive confirmation, served asset status, forbidden Linux runtime term guard만 채택했다. Server-side activity retention/pagination hardening은 2026-05-07 후속 slice에서 `GET /api/v1/jobs` additive metadata와 Web Console Activity page summary로 구현됐다.

Web Dashboard Ops Cockpit 재설계는 guide의 운영 dashboard, 작업대, incident command 패턴을 Desktop Node Web Console 정보 구조로 채택했다. `Dashboard`는 Ops Cockpit 메인으로 host readiness, VM/job count, runtime policy, priority warning, recent activity를 보여주고, `Virtual Machines`는 VM Workbench로 VM 검색, 선택 VM 상세, lifecycle/checkpoint action, VM-local activity context를 제공한다. `Troubleshooting`은 Incident Command로 failed jobs, runtime/auth/LAN/VMMS/checkpoint risk와 read-only diagnostic guidance를 묶는다. 구현은 read-only `GET /api/v1/ops/summary` aggregate route와 기존 route fallback을 추가했으며, lifecycle/checkpoint/delete는 기존 queued job route와 확인 dialog를 그대로 사용한다. 새 OS mutation, public trusted signing, 외부 stable publication claim은 포함하지 않는다.

API Operations Hardening P2는 request/job correlation id와 additive response shape hardening을 구현했다. 모든 Local API JSON response는 `request_id`를 포함하고, queued job snapshot은 `request_id`/`correlation_id`를 보존한다. Web Console Activity는 해당 id를 선택적으로 표시한다. 이 구현은 public CORS, public metrics, Event Log/firewall/trust-store/LAN/MSI/service mutation을 포함하지 않는다.

Network Inventory Web View는 기존 C# native read route인 `GET /api/v1/network/inventory`를 Web Console의 `Network` 화면으로 승격했다. 화면은 switch source, read-only mutation mode, switch/default count, switch type, management OS, external adapter field를 표시한다. 이 구현은 Hyper-V switch 생성/삭제, IP/firewall 변경, service/MSI/trust-store/LAN/update mutation을 실행하지 않는다.

Diagnostic Bundle Operator Handoff UI는 기존 product wrapper `CollectDiagnostics` 절차를 Web Console `Troubleshooting` 화면에서 발견 가능하게 만들었다. 화면은 diagnostics root, operator handoff mode, no host mutation, token value/Authorization header redaction boundary를 표시한다. Web API를 통한 bundle 생성/download action, elevated wrapper 실행 대행, service/MSI/firewall/trust-store/LAN/update mutation은 구현하지 않았다.

Job Activity Retention/Pagination Hardening은 Local API `GET /api/v1/jobs`에 bounded `limit`/`offset` query와 additive metadata를 추가했다. 기본 page는 `limit=50&offset=0`, 최대 limit은 200이며, terminal job `succeeded`/`failed`/`canceled`는 최신 500개를 보존하고 `queued`/`running` active job은 보존한다. Persisted job store를 로드할 때도 오래된 terminal job을 pruning하고 store에 반영한다. Web Console `Activity`는 첫 page를 표시하고 `count`/`returned`/`next_offset`/retention 요약을 보여준다. Timeout/rate-limit policy, realtime event stream, destructive cleanup workflow는 구현하지 않았다.

Token Rotation Operator UX는 Web Console `Troubleshooting` 화면에 protected token file root, token storage policy, browser token presence, browser token clear action, `rotation handoff`, `no service token mutation` 경계를 표시한다. 이 UX는 token 값을 렌더링하지 않으며 Authorization header도 표시하지 않는다. 실제 service token file rotation/revoke mutation, service restart, Windows Credential Manager transition은 구현하지 않았다.

## Packaging/distribution future phase 재분류

Packaging/distribution future phase는 Operator/Web UX backlog와 별도다. 현재 내부 전용 Desktop Node 제품 런타임의 완료 조건은 아니며, 별도 future spec/plan/ADR 또는 release approval 없이는 구현 완료로 주장하지 않는다.

| 후보 | 현재 상태 | 다음 조건 |
|------|-----------|-----------|
| Burn bootstrapper | future-noncurrent | MSI chain/bootstrapper 설치 경계, rollback/final-state proof, 관리자 opt-in smoke 별도 정의 |
| MSIX | future-noncurrent | AppX/MSIX identity, service install 가능성, certificate/trust-store boundary, MSI coexistence 정책 정의 |
| winget manifest | future-noncurrent | public/external publication 여부 ADR, package identifier, installer URL/hash/source policy 정의 |
| network download updater | catalog-channel-code-level-partial | file/HTTPS ZIP source gate, SHA-256 verification, extract-before-service-stop, `PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED` HTTP block은 `docs/ga-ready/evidence/network-download-update-source-gate-2026-05-07.md`에 구현 evidence를 기록했다. file/HTTPS JSON catalog/channel resolver와 package SHA-256 handoff는 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`에 code-level evidence를 기록했다. External publication service, public trusted signing, 외부 stable publication, installed destructive catalog update smoke는 별도 future gate |
| full transactional rollback | filesystem-rollback-code-level-partial | update payload validation 직후 service stop 전에 `update-transaction.begin` journal을 쓰고 success/`failed-rolled-back`/`PCV_*` error diagnostics를 기록하는 단일 active journal은 `docs/ga-ready/evidence/update-transaction-journal-diagnostics-2026-05-07.md`에 구현 evidence를 기록했다. Product root backup 이후 copy/config/start/health failure에서 previous root restore를 시도하는 filesystem rollback은 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`에 code-level evidence를 기록했다. Post-crash resume/reconcile, service/data/config/job-store transaction manager, installed destructive smoke는 별도 future gate |
| packaging/publication descriptor | descriptor-code-level-partial | installer build output은 `.publication.json` sidecar를 작성하고 public trusted signing/external stable publication `not-claimed`, Burn/MSIX `not-built`, winget `not-generated`, catalog publication `not-published`를 기록한다. Evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`다. 실제 Burn/MSIX/winget artifact generation, external publication service, public stable channel publication은 별도 future gate |
| Windows Credential Manager transition | future-noncurrent | DPAPI protected token file migration/rollback/redaction contract 정의 |
| default Windows Event Log writer/provider transition | future-noncurrent | JSONL-first primary 정책 대체 여부, provider registration lifecycle, retention/query evidence 정의 |
| built-in TLS certificate lifecycle | future-noncurrent | local certificate issuance/renewal/removal, private key protection, LAN exposure approval boundary 정의 |

현재 evidence는 WiX MSI, internal `RequireSigned`, `AllowUnsignedDev` admin-smoke, internal stable release/update/rollback, native Event Log source registration/removal action, network update source gate code-level, updater catalog/channel resolver code-level, update transaction journal diagnostics code-level, update filesystem rollback code-level, packaging publication descriptor code-level 범위다. 위 future phase는 public trusted signing 또는 외부 stable publication을 자동으로 열지 않는다.

## 우선순위 후보

| 우선순위 | 후보 | guide에서 채택한 패턴 | Desktop Node 범위 | 제외 |
|----------|------|----------------------|-------------------|------|
| P0 | Operator Activity / Audit Timeline | audit log, job completion, destructive RPC result tracking | `Tracked Jobs`, `jobs.json`, `events.jsonl`, structured `PCV_*` failure를 Web Console activity view로 연결하는 설계 | SQLite audit DB clone, Linux audit path, 숨은 provider mutation |
| P0 | Troubleshooting Center | troubleshooting table, debug command flow, log locations | host readiness, service state, VMMS, runtime policy, diagnostic bundle, common error-code guide를 운영자 UI/문서로 정리 | `journalctl`, libvirt/ZFS/OVS 명령, 자동 Event Log 등록, reboot/task scheduling |
| P1 | Monitoring and Alerts | metrics, sustained-condition alert, ACK/dedup history | Windows service/Hyper-V feature/VMMS/job backlog/failed job/disk free/checkpoint count를 read-only 상태로 먼저 노출 | eBPF, node_exporter, PSI, ZFS pool, OVS/OVN metrics, unauthenticated public Prometheus |
| P1 | Token/Auth Policy Hardening | bearer auth, token lifecycle, RBAC boundary | token source/runtime policy visibility, token rotation/revoke operator UX, diagnostics redaction check는 implemented. Service token mutation API는 후속 후보 | Single Edge JWT user DB/bootstrap admin/RBAC wholesale port, token DOM/log/fixture 노출 |
| P1 | Checkpoint Retention | snapshot retention, bulk delete, history | Hyper-V checkpoint age/count warning, keep latest N 같은 guarded queued job 후보 | ZFS snapshot, fsfreeze/thaw, S3 upload, remote replication, RPO/RTO claim |
| P2 | API Operations Hardening | correlation, timeout/rate-limit, pagination | request/job correlation id, additive response shape, job list pagination, terminal job retention은 implemented. Timeout/rate-limit은 후속 후보 | public CORS expansion, Single Edge REST surface copy, unauthenticated metrics broadening |
| P2 | Web UI Operator Workflow Polish | command palette, status bar, real-time event | VM search/filter, safer destructive confirmations, stale asset/version indicator, accessibility pass | container/storage/network page that implies Linux runtime support |
| P2 | Quality Gates for Product Expansion | quality gate matrix, static asset checks | forbidden Linux runtime term/import checks, served asset hash/provenance, browser fixture expansion, optional Playwright 후보 | `make`, Valgrind, C runtime gates, public trusted signing gate |

P0 Operator Activity / Troubleshooting 구현 경계는 `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-operator-ops-console-expansion-design.md`와 `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p0.md`를 따른다. P0의 첫 구현 단위는 read-only `GET /api/v1/jobs` route와 Web Console activity/troubleshooting view로 완료됐으며, 실제 OS/Hyper-V/provider mutation을 실행하지 않았다.

## Migration Apply Closure 현행화

현행화(2026-05-07): Route promotion matrix의 config/job store migration apply 2개 row는 code-level actual apply product operation으로 전환된 뒤, `0.38.6-admin-smoke` installed destructive admin smoke PASS로 `current-native` 승격됐다.

| 후보 | 현재 상태 | 문서 기반 후속 |
|------|-----------|----------------|
| product config migration apply | `product-operation`, `dotnet-native`, `current-native`, installed smoke PASS | `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-product-config-migration-apply-plan-only.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-code-level-2026-05-06.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`가 current config source inventory, schema owner, owned path, backup root, atomic replace, rollback diagnostics, service stopped precondition, installed destructive smoke PASS를 기록한다. |
| job store migration apply | `product-operation`, `dotnet-native`, `current-native`, installed smoke PASS | `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-job-store-migration-apply-plan-only.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-code-level-2026-05-06.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`가 job store path inventory, runtime writer stopped proof, schema version, backup root, atomic replace, recovery evidence, installed destructive smoke PASS를 기록한다. |

이 두 항목은 `0.38.6-admin-smoke` installed destructive admin smoke 이후 current-native GA closure evidence로 사용할 수 있다. 이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

## Web Console coverage map

현재 Web Console이 직접 제공하는 범위:

- Ops Cockpit `Dashboard`의 host readiness, VM/job count, runtime policy, priority warning, recent activity
- VM Workbench `Virtual Machines`의 VM search/filter, selected VM detail, lifecycle/checkpoint action, VM-local activity context
- Network Inventory `Network`의 `/api/v1/network/inventory` read-only switch inventory
- Incident Command `Troubleshooting`의 failed jobs, runtime/auth/LAN/VMMS/checkpoint risk, diagnostic bundle operator handoff, Token Rotation operator UX, read-only diagnostic guidance
- VM create/list/detail
- VM start/shutdown/poweroff/restart/delete queued job
- checkpoint create/list/restore/delete queued job
- browser-local `Tracked Jobs` get/cancel/retry/polling
- Operator Activity의 server-side job snapshot 및 request/correlation id 표시
- Operator Activity의 `/api/v1/jobs?limit=50&offset=0` pagination/retention summary 표시
- runtime policy 기반 monitoring/troubleshooting 요약

현재 API 직접 호출 또는 운영자 도구로 남는 범위:

- diagnostic bundle server-side collection/download action
- timeout/rate-limit policy
- service token rotation/revoke mutation API
- Event Log source registration/removal
- firewall/trust-store/LAN/MSI/service mutation gates

## 검증 기준

문서/backlog 변경:

```powershell
git diff --check
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

Web Console 구현 후보:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

실제 Hyper-V, service/MSI, firewall, Event Log, trust-store, LAN, update/rollback, config/job store migration apply는 관리자 opt-in gate로만 실행한다.

## 완료 기준

- guide 기반 후보가 Desktop Node Windows/Hyper-V 내부 서비스 범위로만 분류되어 있다.
- VM delete UI는 implemented 상태로 기록되어 있고, UI-only evidence와 OS mutation gate evidence를 혼동하지 않는다.
- Operator/Web UX P0/P1/P2, VM delete UI, Web Dashboard Ops Cockpit plan과 Network Inventory Web View는 closure 상태이며 남은 UX 후보는 새 backlog로만 추적한다.
- Packaging/distribution future phase는 대부분 future-noncurrent로 재분류되어 있고, network download updater는 source gate와 catalog/channel resolver code-level partial, full transactional rollback은 transaction journal diagnostics와 product root filesystem rollback code-level partial, packaging/publication은 descriptor code-level partial만 갖는다. 현재 GA-ready closure blocker, full updater 완료 주장, full transactional rollback 완료 주장, public publication 완료 주장으로 세지 않는다.
- config/job store migration apply는 `0.38.6-admin-smoke` 이후 current-native로 승격됐고, 남은 future-route 후보는 route matrix에서 별도 future implementation plan requirement 여부를 따로 판단한다.
- public trusted signing과 외부 stable publication을 완료 조건으로 주장하지 않는다.
