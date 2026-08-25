# PureCVisor Desktop Node Phase 3B VM Detail + Lifecycle 설계

## 목적

Phase 3B는 Phase 3A Web Console의 VM 목록을 실제 운영 화면으로 확장한다. 사용자는 VM table에서 VM 한 대를 선택해 상세 정보를 확인하고, 같은 화면에서 start, graceful shutdown, force power off, restart 작업을 job으로 요청할 수 있어야 한다.

이 단계의 핵심은 새 Hyper-V lifecycle 기능을 만드는 것이 아니라, Hyper-V helper에 이미 존재하는 lifecycle operation을 Phase 2H Local API job 경계와 Phase 3A Web Console UX로 연결하는 것이다.

## 현재 상태

현재 구현은 완료되어 다음 상태를 기준으로 한다.

- Hyper-V helper: `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart` operation과 Phase 3B detail panel이 쓰는 `memory.assigned_mb`, `generation`, `storage[].attached` inventory field가 `spikes/purecvisor-desktop-node/hyperv/`에 구현되어 있다.
- Local API: `GET /api/v1/host/status`, `GET /api/v1/vms`, `GET /api/v1/vms/{id}`, `POST /api/v1/vms`, lifecycle job route, job get/cancel/retry, optional API token, static Web Console serving, bounded worker pool이 구현되어 있다.
- Web Console: host dashboard, VM table, VM detail drawer, VM create form, lifecycle job actions, session job panel, cancel/retry control이 구현되어 있다.
- 검증 기준: API suite 62 passed, Web static suite 9 passed, Hyper-V helper non-integration suite 39 passed / 0 failed / 1 NotRun.

## 범위

Phase 3B에 포함한다.

- `GET /api/v1/vms/{id}` route
- `POST /api/v1/vms/{id}/start` route
- `POST /api/v1/vms/{id}/shutdown` route
- `POST /api/v1/vms/{id}/poweroff` route
- `POST /api/v1/vms/{id}/restart` route
- lifecycle route를 기존 job queue와 worker tick으로 처리
- VM table row selection
- selected VM detail drawer 또는 detail panel
- selected VM의 CPU, memory, storage, network, checkpoint count, console metadata, managed flag 표시
- lifecycle action 버튼과 destructive action 확인
- lifecycle job을 기존 session job panel에 추적
- Pester API contract tests, Web static tests, JavaScript syntax check
- 관련 README, 개발 인덱스, 검증 정책 문서 갱신

Phase 3B에서 제외한다.

- checkpoint 목록, 생성, 복원, 삭제 UI
- checkpoint Local API route 추가
- VMConnect 직접 실행 또는 protocol handler
- persistent browser job history
- WebSocket, event stream, server-sent events
- LAN mode UX와 LAN binding
- token 발급, token 저장소 암호화, multi-user login
- runspace/threaded background worker
- Windows service 설치
- Linux Single Edge `ui/` 구현 변경
- 신규 frontend framework, bundler, npm package, TypeScript 도입

## API 설계

### VM Detail

`GET /api/v1/vms/{id}`는 `vm.list` helper 결과에서 `id` 또는 `name`이 route id와 일치하는 VM을 찾아 반환한다.

Phase 3B는 별도 `vm.get` helper operation을 만들지 않는다. Hyper-V helper의 inventory shape가 이미 VM detail에 필요한 필드를 포함하고 있고, 새 helper operation을 추가하면 helper 계약과 테스트 범위가 불필요하게 커진다.

응답은 기존 helper response shape를 유지한다.

```json
{
  "ok": true,
  "operation": "vm.get",
  "data": {
    "id": "ubuntu-lab-01",
    "name": "ubuntu-lab-01",
    "state": "running"
  },
  "error": null
}
```

일치하는 VM이 없으면 `404`와 `PCV_VM_NOT_FOUND`를 반환한다. route id는 URL decode 후 빈 문자열을 거부한다. VM 이름의 최종 유효성 검사는 helper lifecycle operation의 기존 검증을 따른다.

### Lifecycle Jobs

Lifecycle route는 모두 즉시 Hyper-V helper를 실행하지 않는다. 기존 `vm.create`와 같은 job-first 패턴을 따른다.

```text
POST /api/v1/vms/{id}/start     -> operation vm.start
POST /api/v1/vms/{id}/shutdown  -> operation vm.shutdown
POST /api/v1/vms/{id}/poweroff  -> operation vm.poweroff
POST /api/v1/vms/{id}/restart   -> operation vm.restart
```

각 route는 다음 job params를 만든다.

```json
{
  "name": "ubuntu-lab-01"
}
```

성공 응답은 `POST /api/v1/vms`와 같은 job object를 반환한다. `job.operation`은 실제 helper operation 이름을 보존한다.

```json
{
  "ok": true,
  "operation": "job.create",
  "data": {
    "job_id": "uuid",
    "operation": "vm.start",
    "status": "queued",
    "params": {
      "name": "ubuntu-lab-01"
    }
  },
  "error": null
}
```

worker는 기존 `Invoke-PcvApiWorkerTick`과 `Invoke-PcvApiWorkerPoolTick` 경로로 lifecycle job을 처리한다. retry와 cancel 정책은 기존 job control 정책을 그대로 따른다.

## Web Console 설계

### VM 선택

VM table row는 선택 가능한 행이 된다. 사용자가 row를 선택하면 `state.selectedVmId`를 갱신하고 detail panel을 연다. 이미 `GET /api/v1/vms` 응답에 해당 VM 객체가 있으면 즉시 표시하고, 이어서 `GET /api/v1/vms/{id}`를 호출해 최신 detail을 반영한다.

VM id는 `vm.id || vm.name` 순서로 선택한다. 표시명은 `vm.name || vm.id`를 사용한다.

### Detail Panel

Phase 3B의 detail panel은 데스크톱에서는 오른쪽 drawer로 구현하고, 좁은 화면에서는 VM table 아래로 내려오는 panel로 구현한다. 새 HTML route를 만들지 않는다. 정적 Web Console은 계속 `index.html`, `styles.css`, `app.js` 단일 화면 구조를 유지한다.

detail panel은 다음 정보를 표시한다.

- name, id, state
- CPU count
- startup memory MB와 assigned memory MB
- generation
- storage path, size GB, attached state
- network adapter와 switch
- checkpoint count
- console metadata
- `managed_by_purecvisor`
- raw notes 또는 error summary

객체나 배열 필드는 사람이 읽기 쉬운 작은 표나 key-value list로 렌더링한다. `innerHTML`을 사용할 때는 기존 `escapeHtml()` 경로를 유지한다.

### Lifecycle Actions

detail panel에는 다음 버튼을 둔다.

- Start
- Shutdown
- Power off
- Restart

Start는 확인 없이 실행할 수 있다. Shutdown은 graceful action으로 표시한다. Power off와 Restart는 destructive 성격이 있으므로 확인 dialog를 요구한다. Phase 3B에서는 브라우저 기본 `confirm()` 사용을 허용한다. 이후 디자인 polish 단계에서 custom dialog로 대체할 수 있다.

버튼 클릭은 lifecycle route를 호출하고 반환된 job을 `trackedJobs`에 추가한다. lifecycle job이 queued 또는 running이면 기존 polling이 동작한다. job이 완료되면 VM 목록과 selected VM detail을 다시 새로고침한다.

### 상태별 버튼

Phase 3B는 복잡한 Hyper-V 상태 전이 행렬을 Web Console에 복제하지 않는다. 버튼은 기본적으로 모두 표시하되, API/helper가 부적합한 상태를 structured error로 반환하게 둔다. 단, 선택된 VM이 없거나 요청 중인 버튼은 disabled 처리한다.

이 선택은 UI와 helper의 상태 판단이 어긋나는 문제를 줄이고, Hyper-V helper를 상태 진실 소스로 유지하기 위한 것이다.

## 오류 처리

API error는 기존 normalized error shape를 따른다.

- route 없음: `PCV_ROUTE_NOT_FOUND`
- method 불일치: `PCV_METHOD_NOT_ALLOWED`
- VM 없음: `PCV_VM_NOT_FOUND`
- job 생성 실패: `PCV_JOB_CREATE_FAILED`
- helper lifecycle 실패: 기존 `PCV_LIFECYCLE_*` 또는 `PCV_LIFECYCLE_FAILED`

Web Console은 기존 alert region에 `PCV_*` code, message, detail을 표시한다. lifecycle job 자체의 실패는 tracked job row에 status와 error summary를 표시한다.

## 보안과 격리

Phase 3B는 기존 Local API 보안 경계를 바꾸지 않는다.

- listener는 loopback prefix만 허용한다.
- `-ApiToken`이 설정되면 lifecycle route와 VM detail route도 bearer token gate를 통과해야 한다.
- static file serving 경계는 유지한다.
- Linux Single Edge `ui/`와 API 공개 표면을 수정하지 않는다.

Lifecycle action은 VM 상태를 바꾸는 mutation이므로 request body 없이 route id만으로 작업을 만들되, helper에 넘기는 params는 allowlisted operation과 `{ name }`만 포함한다.

## 검증 전략

Phase 3B 구현은 다음 검증을 요구한다.

### API Pester

`spikes/purecvisor-desktop-node/api/tests`에 다음 contract를 추가한다.

- `GET /api/v1/vms/{id}`가 `vm.list` 결과에서 matching VM을 반환한다.
- matching VM이 없으면 `404`와 `PCV_VM_NOT_FOUND`를 반환한다.
- lifecycle route가 helper를 즉시 호출하지 않고 queued job을 만든다.
- lifecycle job의 `operation`이 `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart`로 저장된다.
- worker tick이 lifecycle job을 helper에 전달한다.
- `-ApiToken`이 설정된 경우 detail/lifecycle route도 인증을 요구한다.
- unsupported lifecycle action은 route not found로 처리한다.

기준 명령:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

### Web Static Pester

`spikes/purecvisor-desktop-node/web/tests`에 다음 contract를 추가한다.

- `app.js`가 VM detail route와 lifecycle route 문자열을 선언한다.
- `index.html`이 detail panel mount point를 포함한다.
- lifecycle action button text와 data action이 존재한다.
- destructive action 확인 경로가 존재한다.
- JavaScript syntax validation이 통과한다.

기준 명령:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

### Hyper-V Helper

Phase 3B는 helper lifecycle implementation을 새로 만들지 않는다. 기존 helper non-integration suite가 깨지지 않아야 한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

실제 VM start/shutdown/poweroff/restart 검증은 Hyper-V 관리자 권한과 테스트 VM이 필요한 integration 검증으로 분리한다.

## 문서 갱신

구현 단계에서는 다음 문서를 현재 상태에 맞게 갱신한다.

- `spikes/purecvisor-desktop-node/api/README.md`
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `README.md`
- `AGENTS.md`
- `ui/guide-content.md`

문서 갱신은 Phase 3B 구현과 검증 이후 현행 상태를 반영한다. 상세 완료 증거는 `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle.md`의 `Completion Status`를 따른다.

## 성공 기준

Phase 3B는 다음을 만족하면 완료다.

- VM table에서 VM을 선택하면 detail panel이 열린다.
- `GET /api/v1/vms/{id}`가 VM detail을 반환한다.
- `POST /api/v1/vms/{id}/start|shutdown|poweroff|restart`가 lifecycle job을 만든다.
- worker tick이 lifecycle job을 기존 Hyper-V helper operation으로 실행한다.
- lifecycle job이 기존 job panel에 표시되고 polling, cancel, retry 패턴과 충돌하지 않는다.
- poweroff와 restart는 사용자 확인 후 요청된다.
- optional bearer token이 detail/lifecycle route에도 적용된다.
- Web Console static tests, API tests, Hyper-V helper non-integration tests, `node --check`가 통과한다.
- Desktop Node spike 경계가 유지되고 Linux Single Edge `ui/` 구현 파일은 변경되지 않는다.
