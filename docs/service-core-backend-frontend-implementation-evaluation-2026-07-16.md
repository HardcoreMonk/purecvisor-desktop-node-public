# PureCVisor Desktop Node 서비스 코어/백엔드/프론트엔드 기능 구현 평가서

평가일: `2026-07-16`  
평가 기준 revision: `2e98ff4f2df250c36700e86ace0db46ef0aca420` (`main`)  
설치본 기준: `0.42.65-admin-smoke`  
활성 운영자 표면: `Web Console`, `PCVCLI` (`TUI removed`)  
평가 범위: 저장소 소스, 자동화 테스트, 설치본 read-only runtime probe, 실제 브라우저 렌더링,
0.42.65 package/fullgate/actual-VM/current-card evidence  
평가 중 host mutation 수행: `false`

## 1. 종합 판정

현재 제품은 **서비스 코어와 백엔드가 실제 Windows/Hyper-V 내부 운영에 사용할 수 있는 수준까지
구현됐지만, 프론트엔드의 기본 설치 사용자 경로가 아직 완성되지 않은 internal admin-smoke
제품**이다.

- 서비스 코어는 Windows Service, MSI lifecycle, Hyper-V provider, Host Ops, token/ACL,
  firewall/Event Log/trust-store/data-root 경계를 실제 호스트에서 통과했다.
- 백엔드는 54개 API route contract와 22개 queued mutation route를 제공하며 VM lifecycle,
  checkpoint, QoS, guest execution, job, diagnostics, auth/RBAC contract를 폭넓게 구현했다.
- 프론트엔드는 41개 route coverage descriptor와 VM/QoS/guest/job/diagnostics/evidence UI를
  구현했지만, 현재 설치본에서 account auth가 구성되지 않았고 브라우저에 service token도 없어
  첫 진입 시 API 8개가 모두 HTTP 401로 실패한다.
- 인증 실패 상태에서도 `Connected`, `Ready`, `VM: 3/3`, `4/5`, `API: 10ms avg` 같은 정적
  샘플 값이 표시된다. 따라서 Web Console은 현재 **기능 코드 존재**와 **신뢰 가능한 실사용**이
  일치하지 않는다.
- CLI는 protected token file을 직접 사용할 수 있어 설치본 기능 접근의 실질적인 정상 경로다.

종합 구현 성숙도는 `77/100`으로 평가한다. 이 점수는 절대적인 개발 완료율이 아니라 아래 네
항목을 구분한 내부 비교 지표다.

| 영역 | 기능 존재 35 | 설치본 실사용 30 | 검증 20 | 유지보수성 15 | 종합 | 판정 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| 서비스 코어 | 90 | 90 | 95 | 65 | **87** | 내부 운영 가능 |
| 백엔드/API | 92 | 82 | 92 | 58 | **84** | 기능 폭은 충분, 일부 운영 경계 미완 |
| 프론트엔드 | 78 | 42 | 65 | 35 | **58** | 기능 코드는 존재하나 기본 사용자 경로 미완 |
| 가중 종합 |  |  |  |  | **77** | internal admin-smoke, GA 아님 |

가중치는 서비스 코어 35%, 백엔드 35%, 프론트엔드 30%다.

## 2. 평가 방법과 판정 기준

각 기능을 다음 세 단계로 나눴다.

1. `구현`: source와 route/provider/UI action이 존재한다.
2. `검증`: unit/contract/browser fixture 또는 installed smoke가 PASS했다.
3. `운영 승격`: 현재 설치본과 실제 호스트 evidence에서 동작하고 cleanup까지 확인됐다.

`구현`만 있는 기능을 완료로 간주하지 않았다. 특히 Web Console은 정적 asset HTTP 200과 실제
API-backed 기능 사용 가능성을 분리했다.

실행한 read-only 점검은 다음과 같다.

- 설치본 `pcvcli`: `host status`, `runtime policy`, `ops summary`, `network inventory`
- SCM: `PureCVisorDesktopNode` state/start mode/binary path
- Web: `/`, `/pcv-config.js`, `/app.js` HTTP 응답과 asset 크기
- 실제 Chromium: 첫 진입 console/network/snapshot, Dashboard→Virtual Machines navigation
- 소스 계량: API route family, queued mutation, Web route coverage/action/render 함수, 대형 모듈
- 검증 결과: main L등급 Release 7개 suite summary

## 3. 전체 아키텍처 지도

```text
Windows SCM / MSI
  └─ DesktopNode.Host
      ├─ service lifecycle / token / ACL
      ├─ firewall / Event Log / trust-store / Credential Manager / data-root
      ├─ HTTP static Web listener :80
      └─ Local API listener :7777
          └─ DesktopNode.Api
              ├─ auth/RBAC/JWT contract
              ├─ job runtime + JSON snapshot
              ├─ diagnostics + ops/current evidence projection
              └─ queued mutation dispatcher
                  └─ DesktopNode.HyperV
                      ├─ WMI host/network/VM/checkpoint providers
                      ├─ VM lifecycle/resource/QoS mutation
                      └─ PowerShell Direct guest execution

운영자 표면
  ├─ PCVCLI: protected token file 사용 가능, 현재 정상 운영 경로
  └─ Web Console: static page는 로드되지만 최초 API 인증 bootstrap 미완
```

신뢰 경계는 loopback 기본, LAN preview admin opt-in이다. Loopback static asset은 인증 없이
제공되지만 API는 bearer token을 요구한다. 현재 account login은 구성되지 않았다.

## 4. 서비스 코어 평가

### 4.1 구현 완료 기능

| 기능 | 구현/설치 상태 | 평가 |
| --- | --- | --- |
| Windows Service | `Running/Auto`, self-contained .NET Host | 완료 |
| API/Web listener 분리 | API `127.0.0.1:7777`, Web `127.0.0.1:80` | 완료 |
| MSI lifecycle | install/repair/uninstall/restore와 service handoff | 완료 |
| Host Ops | service, Event Log, firewall, trust-store, Credential Manager, data-root 6 bucket | 완료 |
| Token/secret 경계 | protected token, Credential Manager, ACL hardening, redaction | 완료 |
| Hyper-V provider | WMI native provider와 operation dispatch catalog | 완료 |
| Boot/cleanup 보존 | boot unchanged, firewall 0, temp EventLog source absent, test VM 0 | 완료 |
| Current evidence projection | 0.42.65 fullgate/current evidence를 ops summary로 노출 | 대부분 완료 |

0.42.65 full admin host mutation은 service/MSI/Hyper-V route와 OS mutation `2/2`, OS 세부 단계
`11/11`을 통과했다. 실제 호스트 최종 상태는 service `Running`, firewall rule `0`, 임시 Event
Log source 없음, trust root/publisher 복원, boot unchanged다.

### 4.2 강점

- Windows-only 저장소 경계가 분명하고 Linux/KVM 계열 런타임이 섞이지 않는다.
- host mutation을 route/job/provider와 별도 운영 evidence로 추적한다.
- unmanaged VM delete 차단, disk shrink guard, QoS 단위 변환, secret redaction처럼 파괴적
  경계에 방어 규칙이 있다.
- service, installer, Host Ops에 실제 관리자 호스트 검증이 존재한다.

### 4.3 약점과 위험

1. `DesktopNodeHostServiceAction.cs`가 약 3.9K line 규모로 service lifecycle, migration,
   token, ACL, cleanup, native action orchestration을 한 파일에 집중한다.
2. 최신 closed manual-admin package pair는 `0.42.58 -> 0.42.59`이고, `0.42.62 -> 0.42.63`
   follow-up은 installed baseline mismatch로 blocked다. 즉 0.42.65의 fullgate는 PASS했지만 최신
   update/rollback/clean-host package-pair closure는 아니다.
3. trusted public signing과 external stable publication은 의도적으로 `not-claimed`다. 내부 배포
   목표에는 문제가 없지만 공개 GA 목표라면 release blocker다.

### 4.4 서비스 코어 판정

내부 관리자용 Windows Desktop Node core로는 실질적인 운영 수준이다. 다음 개선은 기능 추가보다
대형 service action 모듈 분리와 0.42.65 package-pair lifecycle closure가 우선이다.

## 5. 백엔드/API 평가

### 5.1 구현 범위

API contract는 총 54개 route다.

| Route family | 수 | 주요 기능 |
| --- | ---: | --- |
| Hyper-V VM | 17 | list/detail/create/start/shutdown/poweroff/restart/pause/resume/rename/eject/resource/disk/delete |
| Hyper-V VM QoS | 7 | blkio/bandwidth readback, storage/network preview/apply, resource limit |
| Auth | 5 | login/refresh/logout/session/RBAC |
| Jobs | 5 | list/get/cancel/retry/delete-status |
| Guest execution | 5 | exec preview/apply, channel preview/verify/ensure |
| Checkpoint | 4 | list/create/restore/delete |
| Diagnostics | 3 | list/create/download |
| Console | 2 | capabilities/VM console projection |
| Guest service | 2 | agent status/ping |
| Host/network/runtime/ops | 4 | host status, network inventory, runtime policy, ops summary |

Route stance는 read-only 22개, product operation 10개, queued native mutation 22개다. Hyper-V
dispatch catalog는 34개 native operation을 provider boundary와 handler에 매핑한다.

### 5.2 실제 동작이 확인된 기능

- Host status와 internal switch topology readback
- VM create/list/detail/start/restart/poweroff/delete와 unmanaged delete guard
- checkpoint create/restore/delete
- memory/vCPU/disk resource mutation
- disk shrink guard와 10→11 GiB expansion
- network QoS `2048 Kbps -> 2,048,000 bps` 변환과 native readback
- job queue, terminal state, cancel/retry contract
- guest execution/channel verify/repair provider contract와 이전 actual Windows guest smoke
- diagnostic bundle list/create/download, retention/pagination contract
- runtime policy, ops summary, batch/current evidence projection

### 5.3 현재 부분 구현 또는 비활성 기능

| 항목 | 현재 상태 | 영향 |
| --- | --- | --- |
| Account auth | code/RBAC/JWT route는 구현, 설치본 mode는 `account_rbac_jwt_not_configured` | Web account login을 기본 경로로 사용할 수 없음 |
| noVNC | console projection은 구현, runtime은 `not_configured` | Web console은 vmconnect/local handoff 중심 |
| Job persistence | memory state + JSON file snapshot | 단일 호스트에는 동작하지만 transaction/concurrency/crash recovery 한계 |
| Worker | `bounded-synchronous-worker-tick` | 장기 작업/동시성/서비스 재시작 경계의 확장성 제한 |
| Default Switch reservation | non-zero minimum QoS가 native `0x80070057` | queue 전 preflight/problem-code 개선 필요 |
| Evidence projection | fullgate는 0.42.65, `next_package_pair`와 public-boundary projection은 오래된 0.42.56 계열 값 포함 | UI/ops summary의 단일 진실 일관성 저하 |

### 5.4 구조적 위험

- `DesktopNodeApiRequestProcessor.cs`가 약 3.1K line으로 routing, validation, job state,
  diagnostics, auth integration, mutation queueing을 함께 소유한다.
- `DesktopNodeHyperVNativeAdapter.cs`도 약 1.9K line으로 여러 provider domain의 orchestration을
  집중한다.
- route registry 자체는 잘 계약화됐지만 runtime implementation의 module depth가 낮아 기능을
  추가할수록 회귀 영향 범위가 커질 가능성이 높다.

### 5.5 백엔드 판정

기능 폭과 실제 Hyper-V 동작은 강하다. 현재 병목은 기능 부재보다 auth/noVNC 활성화 결정,
job runtime 내구성, evidence projection 일관성, 대형 request processor 분리다.

## 6. 프론트엔드 평가

### 6.1 구현된 화면과 액션

Web Console은 다음 view를 제공한다.

- Dashboard/Ops Cockpit
- Virtual Machines/VM Workbench
- Network Inventory
- Jobs/Activity/Event Center
- Evidence/Monitoring
- Troubleshooting/Diagnostics
- Account/Console projection
- command palette, filter/sort, workspace tabs

소스에는 41개 route coverage descriptor, 41개 `data-action`, 38개 async function, 47개 render
function이 있다. VM lifecycle, checkpoint, resource mutation, QoS preview/apply, guest execution,
job cancel/retry, diagnostic create/download UI action이 구현돼 있다.

### 6.2 설치본 실제 브라우저 결과

정적 asset 상태는 정상이다.

| 항목 | 결과 |
| --- | --- |
| `/` | HTTP 200, 약 17.8 KB |
| `/pcv-config.js` | HTTP 200 |
| `/app.js` | HTTP 200, 약 200.7 KB |
| HTML 구조 | section 10, button 31, form 3 |
| Dashboard→Virtual Machines hash navigation | 동작 |

그러나 초기 API-backed 기능은 정상 상태가 아니다.

- `/host/status`, `/vms`, `/network/inventory`, `/runtime/policy`, `/ops/summary`,
  `/console/capabilities`, `/jobs`, `/diagnostics/bundles`가 모두 HTTP 401이다.
- 브라우저 console error는 8개다.
- 화면 상단은 `Auth required`와 `PCV_PARTIAL_REFRESH_DEGRADED`를 표시한다.
- 설치본 account auth가 구성되지 않아 로그인 대체 경로도 즉시 사용할 수 없다.
- service protected token은 CLI가 사용할 수 있지만 브라우저로 안전하게 bootstrap되는 경로가 없다.

가장 심각한 문제는 인증 실패와 동시에 정적 샘플 운영값을 실제 상태처럼 표시한다는 점이다.

| 정적 표시 | 소스 | 실제 runtime과 충돌 |
| --- | --- | --- |
| `pcv-node-a` | `web/index.html` | 실제 host identity가 아님 |
| 활성 워크로드 `4/5` | `web/index.html` | API inventory 미로드 상태 |
| footer `Connected` | 정적 HTML | API 8개가 401 |
| `VM: 3/3` | `web/index.html` | 실제 ops summary는 VM total 1, Web은 미인증 |
| `API: 10ms avg` | `web/index.html` | 측정값이 아닌 정적 문구 |
| Host `Ready` 일부 카드 | fallback render | 인증 실패 중에도 readiness로 보임 |

이 문제는 단순 디자인 품질이 아니라 **운영 상태의 진실성** 문제다. 사용자는 연결 실패와 실제
호스트 상태를 구분하기 어렵다.

### 6.3 테스트 품질

장점:

- TypeScript `tsc --noEmit`, served asset parity, static parity, error/degraded state fixture가 있다.
- Web Pester 48개와 npm verification이 main Release gate에서 PASS한다.
- API error normalization, pagination, retention, RBAC gating, destructive confirmation을 fixture로
  검증한다.

한계:

- 기본 browser fixture는 `node:vm`과 자체 fake document/window를 사용한다.
- 현재 required gate에는 실제 Chromium 설치본 auth/login/API flow E2E가 없다.
- 정적 HTML의 가짜 status와 실제 API 401 조합을 실패로 판정하는 테스트가 없다.
- `served-app.ts` 약 3.6K line, generated `app.js` 약 4.4K line, `styles.css` 약 1.65K line으로
  단일 파일 응집도가 지나치게 높다.

### 6.4 프론트엔드 판정

프론트엔드는 “화면과 action code가 없다”가 아니라 “기능은 많지만 secure first-run과 state
truthfulness가 닫히지 않았다”가 정확한 판정이다. 현재 UI/UX 문제의 우선 원인은 시각 디자인보다
인증 bootstrap, 실제 상태 binding, 오류 상태 일관성이다.

## 7. 검증 현황

main revision에서 L등급 Release lane 7개 suite가 PASS했다.

| Suite | 결과 |
| --- | --- |
| .NET solution | 7 assemblies, 591 tests PASS |
| Web npm | typecheck, served asset, frontend batch plan, static parity, browser fixture PASS |
| Packaging Pester | 407 tests discovered, exit 0 |
| Installer Pester | 49/49 PASS |
| Web Pester | 48/48 PASS |
| `git diff --check` | PASS |
| Current evidence generator `-Check` | PASS |

이 검증은 코드/계약 회귀 방지에는 강하지만 Web의 실제 authenticated installed journey를 required
gate로 닫지는 않는다.

## 8. 우선순위별 미완료 항목

### P0 — 제품 신뢰성과 기본 사용 경로

1. **Web secure bootstrap 완성**
   - account auth를 설치 시 구성하거나, loopback 전용의 안전한 one-time session bootstrap을
     설계해야 한다.
   - raw service token을 static config나 HTML에 넣는 방식은 사용하지 않는다.
2. **가짜 운영 상태 제거**
   - `Connected`, host name, VM count, API latency, workload count를 실제 state에서만 표시한다.
   - 인증 전에는 `Unknown/Auth required`로 명시하고 fallback fixture 값을 출력하지 않는다.
3. **401 초기 진입을 정상 UX로 전환**
   - 8개의 console error를 발생시키기 전에 인증 준비 상태를 판별한다.
   - 미인증 상태에서는 protected route fan-out을 중단하고 단일 login/bootstrap 안내를 제공한다.

### P1 — 운영 완결성과 회귀 방지

4. 실제 Chromium installed-listener E2E를 required gate에 추가한다.
5. 0.42.65 기준 manual-admin install/update/rollback/clean-host package-pair를 닫는다.
6. current-evidence JSON, ops summary, manual-admin next/public-boundary projection의 source를 하나로
   통합한다.
7. `DesktopNodeApiRequestProcessor`, `DesktopNodeHostServiceAction`, `served-app.ts`를 domain별로
   분리한다.
8. job runtime의 crash recovery, atomic persistence, concurrency/SLA를 명시하고 부하 검증을
   추가한다.

### P2 — 선택 기능과 제품 경계

9. account/RBAC와 noVNC를 실제 기본 기능으로 제공할지 명시적으로 결정한다. 제공하지 않을 경우
   UI에서 inactive feature를 제거하거나 명확한 setup-only 상태로 축소한다.
10. Default Switch non-zero bandwidth reservation을 queue 전 validation 또는 전용 problem code로
    변환한다.
11. 공개 배포가 목표에 포함될 경우 trusted signing, timestamp, stable publication, public clean-host
    검증을 별도 release program으로 연다.

## 9. 권장 후속 개발 순서

| 순서 | 변경 등급 | 작업 | 완료 조건 |
| ---: | --- | --- | --- |
| 1 | L | Web auth/bootstrap 설계와 구현 | 새 설치 후 secret 노출 없이 Web API 연결 |
| 2 | M | 정적 sample status 제거와 connection state 단일화 | 401 상태에서 가짜 Connected/Ready/VM count 0건 |
| 3 | M | Playwright installed Web E2E | login/bootstrap→dashboard→VM read-only flow required PASS |
| 4 | M | evidence projection 단일 진실화 | JSON/ops/Web의 current/manual/public 값 일치 |
| 5 | L | 0.42.65 successor package-pair campaign | update/rollback/clean-host descriptor closure |
| 6 | L | core/backend/frontend 대형 모듈 분리 | 기존 54 route와 41 Web coverage 회귀 없이 domain module화 |
| 7 | M/L | job runtime 내구성 강화 | atomic store/recovery/concurrency fault tests PASS |

## 10. 최종 결론

현재 상태를 `100% 개발 완료`로 판정할 수 없다.

- **서비스 코어:** 내부 Windows/Hyper-V 운영 기능은 거의 완성됐다.
- **백엔드:** 주요 제품 기능은 구현됐고 실제 mutation도 동작한다. 다만 auth/noVNC 활성화,
  job persistence, evidence 일관성, 모듈 구조 개선이 남았다.
- **프론트엔드:** 기능 surface는 넓지만 최초 인증과 실제 상태 표시가 완결되지 않아 최종 사용자
  제품으로는 미완성이다.

따라서 다음 개발의 최우선 목표는 새 기능 추가가 아니라 **Web Console이 설치 직후 안전하게
인증되고, 실제 backend 상태만 정확하게 표시하며, 실제 브라우저 E2E로 검증되는 상태**를 만드는
것이다. 이 조건이 닫히면 서비스 코어/백엔드의 이미 구현된 기능을 사용자가 신뢰할 수 있는 제품
경험으로 전환할 수 있다.

## 11. 주요 근거

- `docs/ga-ready/current-evidence.json`
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-16-04265-hostmutation.md`
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.md`
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-16-04265.md`
- `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
- `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs`
- `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- `web/src/served/routes.ts`
- `web/src/served-app.ts`
- `web/scripts/verify-browser-fixture.mjs`
- `web/index.html`
- `artifacts/development-verification-release-04265-main/summary.json`

## 12. 서비스 코어 대형 모듈 처리 addendum (2026-08-06)

§4.3-1과 §8 P1-7의 `DesktopNodeHostServiceAction` 분해를 완료했다. `4,069`줄에서 `1,174`줄로
줄었고 도메인 `9`개가 `Ops/` 소유로 옮겨졌다. 상세는
`docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`가 소유한다.

§8 P1-7의 나머지 두 파일(`DesktopNodeApiRequestProcessor.cs`, `web/src/served-app.ts`)은 별도
계획으로 남는다. §4.3-3 public signing은 ADR-0006이 `closed-not-adopted`로 닫은 범위 밖 항목이다.
