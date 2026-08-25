# PureCVisor Desktop Node Web Dashboard Ops Cockpit 재설계

## 목적

이 문서는 Desktop Node Web Console을 제품형 운영 dashboard로 재구성하기 위한 설계를 정의한다.

현재 Web Console은 P0 Operator Activity/Troubleshooting, P1 Monitoring/Auth/Checkpoint warning, P2 workflow polish를 이미 갖고 있다. 다음 배치는 새 host mutation 기능을 추가하는 작업이 아니라, 기존 기능을 운영자가 계속 켜놓고 사용할 수 있는 다중 view UI/UX로 재배치하는 작업이다.

## 적용 상태

```text
DESKTOP_NODE_WEB_DASHBOARD_REDESIGN_CANDIDATE: ops-cockpit-main-vm-workbench-and-incident-command-subpages
```

이 설계는 ADR이 아니다. 후속 구현은 별도 implementation plan, Web fixture, static parity, .NET API 검증을 먼저 통과해야 한다.

## 승인된 방향

승인된 제품 정보 구조는 다음과 같다.

| View | 역할 | 출처 방향 |
|------|------|-----------|
| Dashboard | Ops Cockpit 메인 관제 화면 | A |
| Virtual Machines | VM Workbench 전용 운영 화면 | B |
| Troubleshooting | Incident Command 실패/진단 화면 | C |

`Dashboard`는 첫 화면이다. Host/service 상태, VM 수와 running 수, active/failed jobs, LAN/token/runtime policy, checkpoint warning, 최근 operator activity를 한 화면에서 보여준다.

`Virtual Machines`는 VM 중심 작업 공간이다. VM 목록, 검색/필터, selected VM detail, lifecycle/checkpoint action, selected VM job/activity context를 함께 제공한다.

`Troubleshooting`은 문제 중심 화면이다. 실패 job, 최근 error, runtime policy mismatch, VMMS/Hyper-V readiness, LAN/auth 상태, checkpoint risk를 묶고 read-only 진단 가이드를 제공한다.

## 범위

이번 배치는 `web/**` UI/UX 재구성과 작은 read-only API 보강을 허용한다.

허용 범위:

- 기존 단일 긴 page를 hash 기반 다중 view 구조로 정리
- Dashboard를 Ops Cockpit 메인 화면으로 재구성
- VM Workbench와 Incident Command를 전용 서브 페이지로 제공
- `GET /api/v1/ops/summary` 같은 read-only aggregate route 추가
- 기존 개별 route 기반 fallback 렌더링 유지
- browser fixture와 static parity 검증 강화

제외 범위:

- Hyper-V mutation 신규 route 추가
- service/MSI/firewall/trust-store/LAN/Event Log mutation
- config/job store migration apply 구현
- public trusted signing 또는 외부 stable publication claim
- Linux `purecvisorsd`, KVM/libvirt, LXC, ZFS, OVS/OVN runtime 문구 또는 코드 재유입
- PowerShell helper fallback 재도입

## UI 구조

### Shared Shell

Shared Shell은 topbar connection form, sidebar navigation, global alert, refresh state, asset provenance를 소유한다. Token 값은 password input의 입력 상태로만 유지하고 DOM text, job row, activity row, diagnostics text에 노출하지 않는다.

Sidebar는 view 전환을 명확히 해야 한다.

- Dashboard
- Virtual Machines
- Jobs 또는 Activity
- Troubleshooting

`Jobs`와 `Activity`는 Dashboard의 일부 summary로 흡수하되, 기존 browser-local `Tracked Jobs` 기능을 잃지 않는다. 구현 plan에서 독립 view 유지가 더 안전하면 `Jobs`를 보조 view로 남길 수 있다.

### Dashboard / Ops Cockpit

Dashboard는 제품의 첫 인상이다. 마케팅 hero나 장식형 card layout이 아니라, 운영자가 현재 상태를 빠르게 판단하는 고밀도 cockpit이다.

필수 영역:

- Host/service/API readiness
- VM count, running count, checkpoint warning count
- Active/failed job count
- Token/auth policy와 LAN exposure 상태
- 최근 operator activity
- 가장 중요한 warning 1-3개
- served asset/provenance 상태

Dashboard의 주된 질문은 "지금 이 노드는 정상인가, 무엇을 먼저 봐야 하는가"다.

### Virtual Machines / VM Workbench

VM Workbench는 VM 조작과 상세 상태를 한 곳에서 처리한다.

필수 영역:

- VM search/filter
- VM inventory table
- selected VM detail panel
- lifecycle action confirmation
- checkpoint list/action
- selected VM 관련 job/activity context

기존 destructive action confirmation과 managed marker guard 설명은 유지한다. 실제 권한 판단과 guard는 계속 API가 authoritative boundary로 소유한다.

### Troubleshooting / Incident Command

Troubleshooting은 문제 중심으로 정렬한다.

필수 영역:

- failed/retryable job list
- 최근 normalized error
- runtime policy mismatch
- VMMS/Hyper-V readiness
- auth/token/LAN 상태
- checkpoint risk
- read-only diagnostic guidance

진단 가이드는 사용자가 상태를 이해하도록 돕는 문구와 route/link 중심이어야 한다. 자동 OS mutation command를 노출하거나 실행하지 않는다.

## 데이터 흐름

초기 load와 manual refresh는 기존 route를 계속 사용한다.

```text
GET /api/v1/host/status
GET /api/v1/vms
GET /api/v1/jobs
GET /api/v1/runtime/policy
```

작은 read-only API 보강은 `GET /api/v1/ops/summary` 하나로 제한한다. 이 route는 새 영속 데이터를 만들지 않고 기존 host/vm/job/runtime snapshot을 aggregate한다.

`ops.summary` 후보 response data:

```json
{
  "host": {},
  "vm_counts": {
    "total": 0,
    "running": 0,
    "checkpoint_warnings": 0
  },
  "job_counts": {
    "queued": 0,
    "running": 0,
    "failed": 0
  },
  "runtime_policy": {},
  "signals": [],
  "recent_activity": []
}
```

이 shape는 additive contract다. 구현 시 기존 `Body(true, "ops.summary", ...)` JSON envelope와 `request_id` 포함 정책을 유지한다.

## Fallback

`GET /api/v1/ops/summary`가 실패해도 Web Console 전체가 실패하면 안 된다.

Fallback 원칙:

- summary 실패는 Dashboard warning으로만 표시한다.
- Dashboard와 Troubleshooting은 기존 개별 route state로 degraded render를 수행한다.
- 개별 route 실패는 normalized error로 유지한다.
- Troubleshooting은 실패한 route와 error code를 우선 보여준다.
- refresh 중 기존 화면이 전부 비어 보이지 않도록 마지막 성공 snapshot을 가능한 한 유지한다.

## 상태 관리

현재 단일 browser state object는 유지한다. 새 framework나 state library를 추가하지 않는다.

추가 상태 후보:

```text
activeView: dashboard | vms | troubleshooting | jobs
opsSummary: optional aggregate snapshot
summaryError: optional normalized error
```

기존 `selectedVmId`, `vmFilter`, `trackedJobs`, `serverJobs`, `runtimePolicy`는 유지한다. View 전환은 hash 기반으로 처리해 새 dev server나 router dependency를 요구하지 않는다.

## 보안 및 운영 경계

이번 배치는 read-only UI/UX 재구성이다. `ops.summary`도 read-only aggregate route여야 한다.

금지 사항:

- token 값을 DOM text, localStorage job row, activity row, fixture output, diagnostics text에 노출
- Hyper-V/service/MSI/firewall/trust-store/LAN/Event Log mutation 추가
- PowerShell helper fallback 재도입
- Linux/KVM/libvirt/LXC/ZFS/OVS/OVN runtime 용어를 제품 UI에 노출
- public CORS, unauthenticated metrics, external stable publication claim 추가

## 접근성 및 화면 품질

Web Console은 운영 도구다. 화면은 조밀하지만 읽을 수 있어야 한다.

품질 기준:

- 모바일/좁은 화면에서 topbar form, sidebar, VM detail panel이 겹치지 않는다.
- button text와 badge text가 parent box 밖으로 넘치지 않는다.
- Dashboard 첫 viewport에서 핵심 상태가 보인다.
- VM Workbench와 Troubleshooting은 사용자가 명시적으로 이동하는 서브 page다.
- 색상은 상태 구분을 돕되 단일 hue palette로 흐르지 않는다.
- 기존 static asset provenance 표시를 유지한다.

## 검증 기준

문서/spec 변경:

```powershell
git diff --check
```

구현 배치:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run generate:parity --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

실제 Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, update/rollback, config/job store migration apply는 이번 UI/UX 재설계 검증의 필수 항목이 아니다.

## 완료 기준

- Dashboard가 Ops Cockpit 메인 화면으로 동작한다.
- VM Workbench와 Troubleshooting이 전용 서브 page로 제공된다.
- 기존 P0/P1/P2 기능이 삭제되지 않는다.
- 필요한 경우 `GET /api/v1/ops/summary`가 read-only aggregate로 제공된다.
- `ops.summary` 실패 시 기존 route 기반 degraded render가 동작한다.
- token secret, forbidden Linux runtime 용어, PowerShell helper fallback이 재유입되지 않는다.
- Web static parity, browser fixture, Node syntax, Pester 검증이 통과한다.
