# PureCVisor Desktop Node Phase 3A Web Console 설계

## 목적

Phase 3A는 Desktop Node Local API spike 위에 처음으로 실제 사용 가능한 정적 Web Console 화면을 올린다.

이 단계의 목표는 완성형 제품 UI가 아니라, 홈랩 운영자가 Windows PC 한 대를 로컬 가상화 노드처럼 볼 수 있는 첫 운영 화면을 만드는 것이다. 사용자는 브라우저에서 host status, VM inventory, 최근 job 상태를 확인하고, Linux VM 생성 job을 제출할 수 있어야 한다.

## 현재 구현 상태

Phase 3A 구현은 완료되어 `spikes/purecvisor-desktop-node/web/`에 병합됐다. 구현된 정적 Web Console은 `index.html`, `styles.css`, `app.js`로 구성되며, 기존 Local API `-WebRootPath` static serving 경계 위에서 동작한다. Phase 3B 이후 같은 web root는 VM detail drawer와 lifecycle job actions까지 포함한다.

Phase 3A 완료 당시 검증 기준:

- Web static suite: 6 passed, 0 failed
- JavaScript syntax check: `node --check spikes/purecvisor-desktop-node/web/app.js`
- Local API regression suite: 46 passed, 0 failed
- Hyper-V helper non-integration suite: 41 passed, 0 failed, 1 NotRun
- Hyper-V helper 통합 포함 suite: 준비된 관리자 Hyper-V 호스트 기준 42 passed, 0 failed

현재 통합 검증 기준은 Phase 3B 확장까지 포함한다.

- Web static suite: 9 passed, 0 failed
- Local API regression suite: 62 passed, 0 failed
- Hyper-V helper non-integration suite: 39 passed, 0 failed, 1 NotRun

## 승인된 방향

브레인스토밍에서 `A. Dashboard + VM Table` 방향을 선택했다.

첫 화면은 마케팅/랜딩 페이지가 아니라 다음을 한 화면에 배치한 운영 콘솔이다.

- host status 요약
- VM count와 VM table
- 최근 job summary
- VM create 진입점
- API 연결 상태와 optional bearer token 설정

## 범위

Phase 3A에 포함한다.

- `spikes/purecvisor-desktop-node/web/` 아래 독립 정적 Web Console 자산
- `index.html`, `styles.css`, `app.js`
- Phase 2H Local API와 연결되는 fetch wrapper
- optional `Authorization: Bearer <token>` header 지원
- `GET /api/v1/host/status` 기반 host dashboard
- `GET /api/v1/vms` 기반 VM table
- `POST /api/v1/vms` 기반 create VM form
- create response의 `job_id` 표시
- `GET /api/v1/jobs/{job_id}` 기반 단순 job polling
- `POST /api/v1/jobs/{job_id}/cancel` 및 `POST /api/v1/jobs/{job_id}/retry` 버튼 연결
- API error의 `PCV_*` code와 message 표시
- 정적 파일 serving은 기존 `-WebRootPath`를 사용

Phase 3A에서 제외한다.

- Single Edge 기존 `ui/` 통합
- 프론트엔드 framework 도입
- 번들러, npm package, TypeScript 도입
- VM lifecycle start/stop/poweroff UI
- checkpoint UI
- VM detail route
- VMConnect 실행
- LAN mode UX
- token 발급, 저장소 암호화, multi-user login
- WebSocket/event stream
- 실제 Hyper-V integration test 자동 실행

## 파일 경계

Desktop Node Web Console은 기존 Single Edge UI와 분리한다.

```text
spikes/purecvisor-desktop-node/web/
  index.html
  styles.css
  app.js
```

`spikes/purecvisor-desktop-node/api/`는 API daemon과 static serving 계약을 유지한다. Phase 3A가 API route를 추가하지는 않는다.

기존 Single Edge UI 경로인 `ui/`는 Linux/KVM Single Edge 공개판 표면이므로 Phase 3A 구현에서 수정하지 않는다. 구현 단계에서는 상위 문서의 Desktop Node spike 설명 문구를 Phase 3A 상태에 맞게 갱신한다.

## 화면 구조

첫 화면은 고정된 운영 콘솔 레이아웃이다.

```text
Top bar
- PureCVisor Desktop Node
- API base URL
- connection state
- token input/save/clear
- refresh button

Left nav
- Dashboard
- Virtual Machines
- Jobs

Main
- Host status summary
- VM/job metrics strip
- Virtual Machines table
- Recent jobs panel
- Create VM modal/form
```

카드는 반복 항목과 상태 요약에만 사용한다. 전체 페이지를 card 안에 넣지 않는다. 레이아웃은 조용하고 조밀한 운영 도구 형태를 따른다.

## Host Dashboard

Host dashboard는 `GET /api/v1/host/status` 응답의 `data`를 표시한다.

표시 우선순위는 다음과 같다.

- Windows edition/support 여부
- admin mode 여부
- Hyper-V available/enabled 여부
- VMMS service 상태
- Default Switch 상태
- helper가 반환하는 raw diagnostic fields

응답 shape가 환경별로 일부 달라도 UI가 깨지지 않도록 unknown field는 key/value 목록으로 표시한다.

## VM Table

VM table은 `GET /api/v1/vms` 응답의 `data`를 표시한다.

테이블은 다음 열을 기본으로 한다.

- Name
- State
- CPU
- Memory
- Generation
- Uptime 또는 Updated
- Notes/Error

응답에 해당 field가 없으면 `-`로 표시한다. 배열이 아닌 응답, 빈 배열, helper failure는 각각 다른 상태로 보여준다.

## Jobs Panel

Phase 2H API는 전체 job listing endpoint를 아직 제공하지 않는다. 따라서 Phase 3A의 최근 jobs panel은 Web Console 세션 안에서 생성하거나 조작한 job id만 추적한다.

동작은 다음과 같다.

- VM create 성공 시 반환된 `job_id`를 in-memory list에 추가한다.
- job id마다 `GET /api/v1/jobs/{job_id}`를 polling한다.
- queued/running job은 cancel 버튼을 표시한다.
- failed job은 retry 버튼을 표시한다.
- retry 성공 시 새 `job_id`를 list에 추가하고 원본 실패 job은 그대로 표시한다.

브라우저 새로고침 이후 job history 보존은 Phase 3A 범위가 아니다.

## Create VM Form

Phase 3A는 4단계 wizard 전체를 만들지 않고, 첫 사용 가능한 create form으로 시작한다.

필드는 Phase 2H API payload와 일치한다.

```json
{
  "name": "ubuntu-lab-01",
  "iso_path": "D:\\iso\\ubuntu-24.04-live-server-amd64.iso",
  "cpu": 2,
  "memory_mb": 4096,
  "disk_gb": 40,
  "vm_root": "D:\\PureCVisor\\VMs",
  "generation": 2
}
```

기본값은 홈랩 Linux server 기준으로 둔다.

- CPU: `2`
- Memory: `4096`
- Disk: `40`
- VM root: `D:\PureCVisor\VMs`
- Generation: `2`

폼 검증은 브라우저에서 먼저 수행한다. 이름, ISO path, VM root는 비어 있으면 제출하지 않는다. CPU, memory, disk, generation은 숫자로 변환해 전송한다.

## API Client

`app.js`는 작은 fetch wrapper를 갖는다.

입력값:

- API base URL: 기본값은 현재 origin
- API token: 비어 있으면 Authorization header를 보내지 않음

요청 규칙:

- 모든 API request는 `/api/v1/...` path를 사용한다.
- token이 있으면 `Authorization: Bearer <token>`을 추가한다.
- JSON body request에는 `Content-Type: application/json`을 추가한다.
- HTTP error와 `ok=false` response를 모두 UI error로 정규화한다.

정규화된 error shape:

```text
status
operation
code
message
detail
retryable
```

## 상태 관리

Phase 3A는 framework 없이 단일 `app.js` 파일에서 명시적 state object를 사용한다.

```text
state.apiBaseUrl
state.apiToken
state.host
state.vms
state.trackedJobs
state.loading
state.error
```

DOM update는 `render()` 함수에서 수행한다. `innerHTML` 사용은 정적 template 조각과 escaped text helper를 통해 제한한다. 사용자/API 문자열은 `escapeHtml()`을 거친 뒤 렌더링한다.

## Error Handling

오류 표시는 숨기지 않는다.

- auth missing: `PCV_AUTH_REQUIRED`
- auth rejected: `PCV_AUTH_FORBIDDEN`
- route/method mismatch: `PCV_ROUTE_NOT_FOUND`, `PCV_METHOD_NOT_ALLOWED`
- helper failure: helper가 반환한 `PCV_*` code
- network failure: `PCV_NETWORK_ERROR`
- malformed response: `PCV_RESPONSE_INVALID`

상단 connection state는 마지막 refresh 결과를 기준으로 `connected`, `auth required`, `error`, `idle` 중 하나로 표시한다.

## 검증 전략

Phase 3A 구현은 다음 검증을 요구한다.

- Web static suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`
- `node --check spikes/purecvisor-desktop-node/web/app.js`
- 정적 smoke test: HTML/CSS/JS 파일 존재, API endpoint 문자열, create payload field, token header 문자열 확인
- Local API Pester suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`
- Hyper-V non-integration suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"`
- `git diff --check`

브라우저 기반 UX QA는 후속 단계에서 수행한다. 기준 실행 명령은 다음 형태다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/' -WebRootPath 'spikes/purecvisor-desktop-node/web' -ApiToken 'change-me' -WorkerCount 4
```

## 문서 현행화

Phase 3A 구현 완료 후 다음 문서가 현재 상태에 맞게 갱신됐다.

- `spikes/purecvisor-desktop-node/api/README.md`
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3a-web-console.md`
- `README.md`
- `AGENTS.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/GUIDE.md`
- `ui/guide-content.md`

## 성공 기준

Phase 3A는 다음을 만족하면 완료다.

- `-WebRootPath spikes/purecvisor-desktop-node/web`로 첫 화면이 제공된다.
- token이 없을 때와 있을 때 모두 API client 동작이 명확하다.
- host status와 VM table이 API response를 기반으로 렌더링된다.
- create form이 `POST /api/v1/vms` job을 만들고 job id를 표시한다.
- session-tracked jobs가 polling, cancel, retry 흐름을 제공한다.
- 기존 Phase 2H API와 Hyper-V helper non-integration tests가 깨지지 않는다.
