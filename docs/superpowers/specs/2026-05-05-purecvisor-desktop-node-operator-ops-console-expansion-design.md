# PureCVisor Desktop Node 운영 콘솔 확장 설계

## 목적

이 설계는 내부 전용 Windows/Hyper-V Desktop Node 운영 UX 확장이다. `https://purecvisor.site/ui/guide.html`에서 채택 가능한 운영 패턴을 P0, P1, P2로 나누되 Desktop Node의 현재 ADR-0004 제품 경계와 verification ownership을 유지한다.

이 설계는 Linux Single Edge runtime, public trusted signing, 외부 stable publication, 일반 사용자 public release를 완료 조건으로 삼지 않는다. 적용 범위는 Web Console, Local API read-only 운영 visibility, 문서/검증 gate 정리다.

## 상태 마커

```text
DESKTOP_NODE_OPERATOR_OPS_CONSOLE_EXPANSION: internal-windows-hyperv-readonly-first
```

이 문서는 ADR이 아니다. P0/P1/P2 구현은 각 slice의 plan, TDD red/green, 영향 범위 검증, 커밋 단위로 진행한다.

## 접근안 검토

### 권장안: read-only 운영 visibility 먼저

P0에서 `GET /api/v1/jobs` read-only list route와 Web Console activity/troubleshooting view를 먼저 만든다. 기존 browser-local `Tracked Jobs`는 유지하고, server-side `jobs.json` snapshot에서 읽을 수 있는 job list를 별도 activity source로 표시한다. Troubleshooting Center는 host status, runtime policy, VMMS/Hyper-V readiness, structured `PCV_*` error guide를 보여주지만 OS mutation을 실행하지 않는다.

장점은 P0가 실제 Hyper-V/service/MSI/firewall/trust-store/LAN mutation 없이 운영자가 필요한 맥락을 바로 얻는다는 점이다. 단점은 Event Log JSONL tail, diagnostic bundle 자동 수집, retention action 같은 더 넓은 운영 자동화가 후속으로 남는다는 점이다.

### 대안: Web Console UI-only 확장

Local API route를 추가하지 않고 browser-local `Tracked Jobs`와 static troubleshooting guide만 확장한다. 가장 빠르지만 다른 브라우저나 service 재시작 후 job history를 운영자가 볼 수 없고, `jobs.json`이 이미 제품 path에 있는 상황에서 activity view가 과소 구현된다.

### 대안: full operations backend 선행

P0에서 server-wide audit/activity route, `events.jsonl` reader, diagnostic bundle trigger, retention 정책을 모두 넣는다. 기능은 넓지만 route/retention/redaction/OS mutation gate가 한 번에 섞여 P0 blast radius가 커진다. 현재 내부 서비스 개발 가속 기준에는 맞지 않는다.

따라서 권장안으로 진행한다.

## P0 범위

### Operator Activity / Audit Timeline

P0 activity view는 운영자가 Web Console에서 다음 흐름을 한 화면에서 확인하는 것을 목표로 한다.

- 현재 브라우저 세션의 `Tracked Jobs`
- Local API가 유지하는 server-side job list
- job `operation`, `status`, `attempt`, `retry_of`, `created_at`, `updated_at`
- `result` 또는 structured `PCV_*` failure summary
- cancel/retry 가능 여부

현재 제품 route는 `GET /api/v1/jobs/{job_id}`를 제공하지만 `GET /api/v1/jobs`는 contract에만 있고 served read route가 아니다. 따라서 P0 첫 구현은 `GET /api/v1/jobs`를 read-only route로 구현하고 Web Console에서 이를 activity source로 사용한다. 이 route는 `jobs.json` schema migration apply가 아니며 job store write를 새로 만들지 않는다.

`events.jsonl`은 P0에서 직접 tail/read API를 만들지 않는다. Event Log source 등록/removal과 JSONL diagnostics는 별도 OS mutation 또는 diagnostics owner 범위다. P0 문서와 UI는 `events.jsonl` 위치와 diagnostics bundle 흐름을 안내하되 자동 등록, 자동 수집, 자동 mutation을 실행하지 않는다.

### Troubleshooting Center

P0 troubleshooting view는 다음 read-only 정보를 표시한다.

- host readiness: `/api/v1/host/status`
- runtime/auth/network policy: `/api/v1/runtime/policy`
- VMMS/Hyper-V readiness summary
- loopback/LAN exposure 현재 상태와 LAN approval boundary
- protected token source/storage 상태 요약
- common `PCV_*` error code guide
- diagnostic bundle 위치와 수동 수집 command 안내

이 화면은 `journalctl`, libvirt, ZFS, OVS/OVN, Linux service command를 가져오지 않는다. 또한 Event Log source registration, firewall rule enable/remove, trust-store install/remove, MSI install/repair/uninstall, service install/delete/start/stop, Hyper-V VM mutation, Task Scheduler, reboot를 실행하지 않는다.

## P1 범위

### Monitoring and Alerts

P1은 read-only 운영 신호를 추가한다.

- service/API connectivity
- Hyper-V feature/VMMS 상태
- failed job count와 active job backlog
- VM inventory risk summary
- disk free/checkpoint count warning 후보

P1은 public Prometheus, unauthenticated metrics, eBPF/node_exporter/PSI, ZFS/OVS/OVN metrics를 가져오지 않는다. 경보는 Web Console 표시와 문서 기준으로 시작하며 외부 notification channel은 별도 후보로 둔다.

### Token/Auth Policy Hardening

P1은 token 값을 노출하지 않고 다음 정보를 보여준다.

- token required 여부
- token storage/source 종류
- loopback/LAN별 static asset auth policy
- diagnostics redaction guard
- rotation/revoke UX 후보

P1은 Single Edge JWT DB, bootstrap admin, RBAC wholesale port를 가져오지 않는다. Token value, Authorization header value, protected token blob/hash는 DOM, fixture, log, 문서 예시에 나오면 안 된다.

### Checkpoint Retention

P1은 Hyper-V checkpoint age/count warning을 먼저 제공한다. `keep latest N` 또는 bulk delete는 실제 checkpoint mutation이므로 별도 plan과 관리자 opt-in/rollback/final-state proof 전에는 구현하지 않는다.

## P2 범위

### API Operations Hardening

P2는 request/job correlation id, consistent error shape, server-side activity/history retention contract를 정리한다. Public CORS expansion, Single Edge REST surface copy, unauthenticated metrics broadening은 제외한다. Route-wide API contract 변경은 `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-api-operations-hardening-design.md`와 `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-api-operations-hardening.md`에서 별도 후속으로 다룬다.

### Web UI Operator Workflow Polish

P2는 VM search/filter, destructive confirmation 개선, stale asset/version indicator, accessibility pass를 다룬다. Linux runtime page, container/storage/network page, KVM/libvirt/ZFS/OVS/OVN 기능을 암시하는 화면은 만들지 않는다.

### Quality Gates for Product Expansion

P2는 forbidden Linux runtime term/import guard, served asset hash/provenance, browser fixture 확장, optional Playwright 후보를 검토한다. Playwright는 후속 후보이며 P0/P1 필수 dependency가 아니다. Public trusted signing gate도 이 범위에 넣지 않는다.

## 데이터 흐름

P0 read-only activity flow:

1. Web Console `Refresh`가 `/api/v1/host/status`, `/api/v1/vms`, `/api/v1/runtime/policy`, `/api/v1/jobs`를 호출한다.
2. Browser-local `Tracked Jobs`는 기존 localStorage key `pcvDesktopTrackedJobs.v1`에 유지된다.
3. Server-side job list는 Local API in-memory/job store snapshot에서 read-only로 반환된다.
4. UI는 두 source를 병합하지 않고 source label을 붙여 표시한다. 같은 job id가 양쪽에 있으면 server-side job을 authoritative status로 보여주고 browser-local source는 “tracked locally”로 표시한다.
5. cancel/retry는 기존 `/api/v1/jobs/{job_id}/cancel`, `/api/v1/jobs/{job_id}/retry`만 사용한다.

P0 troubleshooting flow:

1. `/api/v1/host/status`에서 Windows/Hyper-V/VMMS/admin/support 상태를 읽는다.
2. `/api/v1/runtime/policy`에서 auth/network/job/native operation policy를 읽는다.
3. Web Console은 token 값을 출력하지 않고 storage/source enum과 LAN/static asset auth policy만 표시한다.
4. `PCV_*` failure는 alert region과 troubleshooting guide table에 code/message/detail을 표시한다.

## 오류 처리

- `GET /api/v1/jobs`가 실패하면 activity server source만 degraded로 표시하고 browser-local `Tracked Jobs`는 유지한다.
- `PCV_AUTH_REQUIRED` 또는 `PCV_AUTH_FORBIDDEN`은 connection state를 auth로 유지하고 token 값을 표시하지 않는다.
- unsupported future job store schema는 기존 `PCV_JOB_STORE_SCHEMA_UNSUPPORTED` blocked diagnostics를 유지하며 P0 route는 migration, quarantine, rewrite를 수행하지 않는다.
- network failure는 `PCV_NETWORK_ERROR`로 normalize하되 token/header 내용을 포함하지 않는다.

## 보안/운영 경계

- P0 구현은 기본적으로 read-only/operator visibility slice로 시작하며, 실제 OS/Hyper-V/provider mutation은 별도 관리자 opt-in gate와 rollback/final-state proof 전에는 실행하지 않는다.
- 기본 listener는 loopback-only다. LAN exposure는 explicit approval, token source, firewall gate, final-state proof가 있을 때만 운영한다.
- `archive/spikes/**`는 historical/component baseline이며 active product source, packaging input, required verification command로 사용하지 않는다.
- Public trusted signing, 외부 stable publication, 일반 사용자 public release는 완료 조건이 아니다.

## 검증 기준

문서/spec/plan만 변경할 때:

```powershell
git diff --check
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

P0 Local API + Web Console 구현이 들어갈 때:

```powershell
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

Packaging/MSI/service/firewall/trust-store/LAN/Event Log mutation은 P0 기본 검증에 포함하지 않는다. 해당 boundary를 바꾸는 slice에서만 별도 관리자 opt-in으로 실행한다.

## 완료 기준

- P0/P1/P2 후보가 Desktop Node 내부 전용 Windows/Hyper-V 운영 UX 범위로만 정리되어 있다.
- P0 첫 implementation plan이 `GET /api/v1/jobs` read-only route와 Web Console activity/troubleshooting view를 TDD slice로 분리한다.
- `jobs.json` migration apply, config migration apply, Event Log/firewall/trust-store/LAN/MSI/service mutation을 P0 구현 완료 조건으로 주장하지 않는다.
- Token value와 public trusted signing claim이 문서, fixture, DOM, logs에 포함되지 않는다.
- Web Console verification ownership은 npm/TypeScript/browser fixture/Web Pester를 유지한다.
