# PureCVisor Desktop Node 서비스 기획

- Document-ID: `purecvisor-desktop-node-service-plan-v1`
- 작성일: `2026-08-14`
- 문서 상태: `planning-baseline`
- 운영 앵커: `0.42.73-admin-smoke`
- 기준 HEAD: `17b9828c50add14901a1a632c8f5ccdca4645f08` (`origin/main`)
- payload provenance: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
- 비교 기준 제품: VMware Workstation Pro `26H1` (2026-05-14, build `25388281`)
- host mutation performed by this document: `false`
- 다음 package-pair: `not-opened-awaiting-next-product-payload`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 2026-08-14에 재확인한 설치본·evidence·API 계약과 VMware Workstation 26H1 공식 기능을
1:1로 대조한 뒤, Desktop Node가 **어떤 서비스인지**와 **다음에 열 기능 / 열지 않을 기능**을
고정한다. 사용 절차는 `docs/USER_FEATURE_USAGE_SPEC.md`가, 현재 버전 숫자는
`docs/ga-ready/current-evidence.json`이 소유한다. 이 문서는 `0.42.74`를 만들지 않고
package/fullgate/manual-admin campaign을 열지 않는다.

선행 평가 `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md`는
`0.42.65` / 설치 직후 Web 401 경로를 기준으로 한다. 그 평가의 프론트엔드 기본 경로 미완은
`POST /api/v1/auth/loopback-session`과 04273 설치본 loopback smoke로 닫혔다. 아래 현재 판은
그 평가를 대체하지 않고 **04273 기준 기획 판**으로 읽는다.

## 1. 한 줄 정의

PureCVisor Desktop Node는 Windows Hyper-V 호스트를 로컬에서 운영하는 **내부 전용 제어 평면
서비스**다. 자체 하이퍼바이저가 아니며, VMware Workstation을 복제한 데스크톱 앱도 아니다.

| 이 서비스가 하는 일 | 이 서비스가 하지 않는 일 |
| --- | --- |
| 이미 있는 Hyper-V를 Local API/job/RBAC로 운영한다 | `vmware-vmx` 같은 Type-2 엔진을 깐다 |
| Web Console과 PCVCLI로 같은 계약을 제공한다 | TUI, Linux 호스트, vSphere 원격 |
| mutation은 queued job과 `PCV_*`로 남긴다 | GUI에서 동기 성공을 주장한다 |
| 내부 admin-smoke 설치본으로 검증한다 | public trusted signing, 외부 stable publication |

## 2. 현재 운영 사실 (2026-08-14 실측)

| 항목 | 값 |
| --- | --- |
| operational current | `0.42.73-admin-smoke` |
| 활성 표면 | Web Console, PCVCLI. `tui_present=false` |
| 설치본 DisplayVersion | `0.42.73` |
| 설치본 manifest | `0.42.73-admin-smoke` |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| Web `/`, `/pcv-config.js` | HTTP `200` |
| API `/api/v1/runtime/policy` | unauthenticated `401` |
| 계정 | `accounts.json` 계정 `0`, `no-default-account` |
| 닫힌 package-pair | `0.42.72-admin-smoke -> 0.42.73-admin-smoke` |
| descriptor | `manual-admin-campaign-descriptor-20260814-04272-04273-closed` |
| 다음 pair trigger | `product-payload-change-after-04273` |
| HEAD CI | Public Boundary `31778853925`, Development Gates `31778853926` success |

고정 해시:

- clean MSI `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`
- operational MSI `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1`
- operational payload `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85`

04273 payload에 들어간 제품 변경은 Web loopback session, 미구성 login `409`
`PCV_ACCOUNT_AUTH_NOT_CONFIGURED`, in-process Chromium Host gate, PR #189 diagnostics
list다. provenance `b84441f0` 이후 `src` / Web payload / product wrapper 변경은 `0`건이다.

## 3. 서비스 정체성

### 3.1 대상 사용자

| 역할 | 하는 일 | 쓰는 표면 |
| --- | --- | --- |
| viewer | 호스트/VM/job/진단 조회 | Web, 읽기 API |
| operator | VM lifecycle, checkpoint, QoS, guest exec, diagnostic | Web, PCVCLI |
| admin | 계정·설치·update·firewall 등 호스트 운영 | runbook, product wrapper. Web은 대행하지 않음 |
| 자동화 | JSON 반복 조회와 job 추적 | `pcvcli --json` |

기본 설치는 계정을 만들지 않는다. loopback Web은 `POST /api/v1/auth/loopback-session`으로
짧은 JWT를 받는다. LAN과 비-loopback은 service bearer 또는 계정 JWT가 필요하다.

### 3.2 운영자 하루 작업

운영자가 이 서비스에 기대는 일은 다음이다.

1. 이 호스트의 Hyper-V/서비스/token이 살아 있는지 본다.
2. managed VM을 만들고 켜고 끄고 이름을 바꾸고 지운다.
3. checkpoint로 되돌릴 지점을 남긴다.
4. 디스크를 키우고, 스토리지/네트워크 QoS를 건다.
5. Integration Services를 확인하고, credential-ref로 guest 명령을 돌린다.
6. job이 멈추면 cancel/retry/일부 reconcile로 수습한다.
7. 장애 때 redacted diagnostic bundle을 뜬다.

Workstation lab에서 자주 하는 **미디어 재장착, Saved 상태, 복제, 기존 VM 들이기**는
아직 제품 API가 비어 있다. 이 공백이 다음 기획의 대상이다.

### 3.3 표면과 기본 주소

| 표면 | 주소 / 경로 | 성격 |
| --- | --- | --- |
| Web Console | `http://127.0.0.1/` | 기본 대화형 운영 |
| Web API | `http://127.0.0.1:7777/api/v1/...` | 상시 Local API. `DesktopNode.Host.exe` |
| PCVCLI | `pcvcli.exe` | JSON/스크립트. protected token file |
| Product wrapper | `Invoke-PcvDesktopNodeProduct.ps1` | 설치/update/diagnostics runbook |
| TUI | 없음 | ADR-0011 |

REST를 켤 때만 띄우는 Workstation `vmrest`와 달리, Desktop Node API는 Windows Service의
본체다.

## 4. 현재 제공하는 서비스

기능 존재와 설치본 검증을 나눈다. 검증 열의 PASS는 04273 또는 명시된 carry-forward
evidence다.

### 4.1 제어 평면

| 서비스 | API / CLI | 04273 상태 |
| --- | --- | --- |
| Host status | `GET /host/status`, `pcvcli host status` | current-card PASS |
| Runtime policy | `GET /runtime/policy`, `pcvcli runtime policy` | current-card PASS |
| Ops summary | `GET /ops/summary`, `pcvcli ops summary` | 제품 경로 존재 |
| Network inventory | `GET /network/inventory` | read-only. switch mutation 없음 |
| Jobs | list/get/cancel/retry/reconcile | 상시. restore 등 reconcile은 부분 |
| Diagnostics bundle | create/list/download | redaction + pagination |
| Account session | login/refresh/logout/session/rbac | 계정 0이면 login `409` |
| Loopback session | `POST /auth/loopback-session` | 04273 설치본 smoke PASS |
| Rate limit / route timeout | 429 / 504 problem+json | code-level + service plan |
| Token storage | DPAPI LocalMachine | R4는 04272 carry-forward |

### 4.2 Hyper-V 운영

| 서비스 | 계약 | 한계 |
| --- | --- | --- |
| VM list/detail | native read, helper fallback 없음 | unmanaged도 보이지만 mutate는 가드 |
| Create | Gen2, ISO, Default Switch, queued | Gen1 `PCV_GENERATION_INVALID` |
| Start / shutdown / poweroff / restart | queued | shutdown은 Integration 필요 |
| Pause / resume | queued | Saved(suspend) 아님 |
| Rename / delete | queued. delete는 managed만 | unmanaged `PCV_VM_NOT_MANAGED_BY_PURECVISOR` |
| Checkpoint list/create/restore/delete | queued | restore reconcile 제외 |
| set-memory / set-vcpu / limit | queued | |
| disk-resize | expand only | shrink `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` |
| media eject | queued | **이후 attach 없음** |
| QoS storage/network | preview 후 queued apply | 회선 loss/latency 시뮬 아님 |
| Guest agent status/ping | read | qemu agent 아님 |
| Guest channel / exec | preview + credential-ref job | raw secret 거절, running cancel |

### 4.3 의도적으로 닫힌 제품 한계

닫힌 한계는 미구현으로 재해석하지 않는다.

- create는 Generation 2만
- disk shrink 거부
- Network는 inventory만. Web/CLI가 switch/IP/firewall을 바꾸지 않음
- LAN은 preview. 기본 loopback, TLS는 외부 reverse-proxy
- noVNC는 기본 off. target mutation은 ADR-0010 후보
- 계정 CRUD API 없음. 기본 계정 없음
- TUI 없음
- Linux runtime, public signing, 외부 publication 없음

## 5. 완결도 판정

선언된 현재 제품 코어는 **구현되어 04273에 설치되어 동작한다.** 백엔드 전체가 “남은 일 0”은
아니다.

| 영역 | 판정 | 근거 |
| --- | --- | --- |
| 서비스 코어 (SCM, MSI, listener, token) | 내부 운영 가능 | 04273 fullgate + current-card |
| 백엔드 제품 경로 | 구현 완료, 일부 family 미완 | route registry + 04273 functional 10/10 |
| Web 기본 경로 | 04273에서 열림 | loopback smoke, token 붙여넣기 없음 |
| Job reconcile | 부분 | rename/delete/checkpoint.create만 |
| 다음 lab 운영 공백 | 기획 대상 | attach, Saved, clone, managed import |

2026-07-16 평가의 “Web 첫 진입 401 + 가짜 Connected”는 후속 진실성 slice와 loopback
bootstrap으로 닫혔다. 남은 백엔드는 새 하이퍼바이저가 아니라 **기존 family의 빈 짝과
조건부 복구**다.

## 6. 참조 제품: Workstation 26H1

Workstation은 호스트 OS 위의 Type-2 데스크톱 하이퍼바이저다. 26H1의 성격은 기능 폭발이
아니라 64-bit Windows 본체, lifecycle timestamp, folder notes, credential 식별, ARM ESXi
Tech Preview, 신규 OS다. 큰 기능 추가는 25H2의 HW22, USB 3.2, `dictTool`, Hyper-V 탐지다.

대조의 결론:

- 기능 개수는 Workstation이 많다 (클론, OVF, 3D, USB, 가상 스위치, 공유 폴더, 내장 콘솔).
- 운영 제어 평면은 Desktop Node가 더 제품화되어 있다 (상시 API, job, RBAC, credential-ref,
  QoS apply, diagnostics, managed delete).
- 빈칸의 대부분은 미구현이 아니라 제품 경계다.

공식 출처: Broadcom 26H1 릴리스 노트, 사용 설명서, 제품 비교표, `vmrun`/`vmrest` 문서.

## 7. 따라갈 것 / 따라가지 않을 것

기준: Hyper-V 로컬 노드 + CLI/Web + job/RBAC에 들어가면 따라간다. Type-2 엔진이 되면
따라가지 않는다.

### 7.1 따라갈 기능 (우선순위)

Workstation 이름을 가져오지 않는다. 같은 운영 공백을 PCV 계약으로 닫는다.

#### P0 — 기존 계약의 빈 짝

| # | 기획 이름 | PCV 형태 | 완료 조건 초안 | 하지 않을 것 |
| ---: | --- | --- | --- | --- |
| 1 | 미디어 재장착 | `POST /vms/{id}/attach` + `pcvcli vm attach`, eject의 짝 | queued job, confirmation, Web/CLI 동일 route, 설치본 smoke | USB/3D 장치 상점 |
| 2 | checkpoint restore 추적 | restore **reconcile** | before-state + 단일 identity일 때만 `succeeded`. 모호하면 `409 PCV_JOB_RECONCILIATION_REQUIRED` | list에 checkpoint가 있다고 자동 성공 |
| 3 | 호스트 재부팅 내구 정지 | Hyper-V **Saved** suspend/resume-from-saved. 지금 `pause`와 분리 | 새 operation, pause와 문서/API 이름 분리, actual-VM | pause를 suspend로 개명만 |
| 4 | 기존 lab VM 들이기 | 기존 Hyper-V VM **managed 승격** opt-in job | marker 기록, 이후 delete 가드 통과. 거부 경로 유지 | 아무 VM이나 delete |

#### P1 — 로컬 Hyper-V 운영 가치

| # | 기획 이름 | PCV 형태 | 선행 |
| ---: | --- | --- | --- |
| 5 | managed full clone | 새 VHDX 복사 + 새 marker + queued job | managed 정의가 분명해야 함 |
| 6 | inventory 시각/메모 | created / last_powered_on, optional notes | read 위주 |
| 7 | template lock | start/clone만 허용, 직접 mutate 금지 | clone 또는 import |
| 8 | 제한적 guest 파일 job | credential-ref + path/size allowlist | HGFS 복제 금지 |
| 9 | admin account CRUD | create/disable만. 기본 계정 없음 | `no-default-account` 유지 |
| 10 | 나머지 조건부 reconcile | create/shutdown/restart/QoS를 family별 slice | 자동 retry 금지 |

#### P2 — 정책이 먼저

| # | 기획 이름 | 열기 전 조건 | 열면 안 되는 형태 |
| ---: | --- | --- | --- |
| 11 | noVNC target 설정 | ADR-0010: audit, rollback, loopback 기본, reload | 화면에서 target 저장, LAN 기본 on |
| 12 | 주기 checkpoint | retention, 용량 가드 | 무한 AutoProtect |
| 13 | Hyper-V export/import | managed marker 유지 | 만능 OVF, vTPM 키 노출 |
| 14 | 네트워크 변경 | admin runbook / service-action만 | Web switch/NAT/DHCP 에디터 |
| 15 | NIC/DVD 추가 | 한 개씩 queued add | 장치 상점 |

P0–P1을 열면 그때 **새 product payload**가 생기고, 그때만 `0.42.73 -> next` package-pair를
검토한다. 이 문서 자체는 그 campaign을 열지 않는다.

### 7.2 따라가지 않을 기능

#### 하면 이 서비스가 아님

| 거부 항목 | 이유 |
| --- | --- |
| 자체 하이퍼바이저 | 저장소 경계. Hyper-V 제어 평면 |
| Linux 호스트, Fusion, ARM 네이티브 게스트 | Windows Desktop Node only |
| vSphere/ESXi 원격 | 로컬 호스트 제품 |
| 자체 NAT/DHCP/브리지 스택 | Hyper-V 스위치를 읽는다 |
| Web 기본 Virtual Network Editor | inventory read-only 유지 |
| USB passthrough, DX11/OpenGL 상점 | 호스티드 하이퍼바이저 장치 모델 |
| HGFS 공유/미러 폴더 | VMware Tools 파일시스템 |
| Unity, Bluetooth hub, `.vmx`/`dictTool` | 호스트 UI이거나 우회 mutation |
| Player SKU, P2V, `vctl`/K8s | 범위 밖 또는 Workstation도 제거 |
| 공개 스토어·라이선스 키 | ADR-0006 |

#### 이름은 닮았지만 베끼면 안 됨

| Workstation 방식 | Desktop Node가 유지할 방식 |
| --- | --- |
| `vmrest` 수동 기동 | 상시 `DesktopNode.Host` |
| GUI 즉시 전원/클론 | queued job |
| `vmrun` raw password | credential-ref, preview, audit, cancel |
| 아무 VM 삭제 | managed marker |
| disk shrink / Gen1 마법사 | 구조화 거절 |
| 기본 계정, 항상 열린 콘솔 | `no-default-account`, noVNC 기본 off |
| 동기 성공 | `PCV_JOB_INTERRUPTED`, 자동 retry 없음 |
| TUI | ADR-0011. Web + PCVCLI |

#### 지금은 열지 않음

- linked clone / 차이 디스크 트리
- noVNC target self-service (ADR-0010 전)
- 브라우저에서 firewall/trust-store/MSI
- 설치 시 기본 계정 생성
- Hyper-V exactly-once, mixed-version 동시 writer (ADR-0013 비주장)

## 8. 다음 서비스 단계 (기획만)

실행 순서는 P0 네 항목이다. 각 항목은 별도 설계 slice와 사용자 승인 없이 product payload를
열지 않는다.

```text
P0-1 media attach
  -> P0-2 checkpoint restore reconcile
  -> P0-3 Hyper-V Saved suspend
  -> P0-4 managed import
  -> (승인 시) P1 full clone
```

공통 구현 계약:

- Web와 `pcvcli`가 같은 route를 쓴다.
- mutation은 queued job이다. preview가 있는 family는 dry-run을 먼저 둔다.
- destructive 동작은 confirmation과 대상 이름을 보여 준다.
- 실패는 `PCV_*` + 다음 운영 행동이다.
- helper fallback 없이 native structured failure다.
- 설치본 검증 전에는 operational current를 올리지 않는다.
- public trusted signing과 외부 publication을 주장하지 않는다.

## 9. 성공 기준

기획이 성공하려면 다음이 동시에 참이어야 한다.

| 기준 | 측정 |
| --- | --- |
| 정체성 유지 | Hyper-V 제어 평면, CLI/Web-only, 내부 사설망 |
| 운영 공백 감소 | P0 네 항목이 각각 설계→code-level→설치본  evidenc로 닫힘 |
| 계약 회귀 없음 | Gen2-only, no shrink, managed delete, credential-ref, loopback 기본 |
| current 규율 | payload 없는 docs는 `0.42.73` 유지. 다음 버전은 payload 이후에만 |
| 비주장 | public signing, 외부 publication, exactly-once Hyper-V |

## 10. 비주장 / 비목표

- 이 문서는 ADR이 아니다. ADR-0006, 0009, 0010, 0011, 0013을 바꾸지 않는다.
- `0.42.74` 또는 `0.42.73 -> next` campaign을 열지 않는다.
- Workstation 기능 패리티 100%를 목표로 하지 않는다.
- 2026-07-16 평가 점수 `77/100`을 재계산하거나 GA를 선언하지 않는다.
- host mutation, MSI apply, 공개 배포를 이 문서가 실행하지 않는다.

## 11. 관련 문서

| 문서 | 역할 |
| --- | --- |
| `docs/ga-ready/current-evidence.json` | 현재 버전 단일 진실 |
| `docs/USER_FEATURE_USAGE_SPEC.md` | 사용자 기능 사용 명세 |
| `docs/CLI_COMMAND_USAGE.md` | PCVCLI 계약 |
| `docs/USER_GUIDE.md` | 사용 절차 |
| `docs/OPERATIONS_GUIDE.md` | 관리자 runbook |
| `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` | 04265 구현 평가 (선행) |
| `docs/adr/0006-internal-private-network-distribution.md` | 내부 사설망 |
| `docs/adr/0009-guest-execution-security-boundary.md` | guest exec |
| `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md` | noVNC target 보류 |
| `docs/adr/0011-cli-web-only-operator-surface.md` | CLI/Web-only |
| `docs/adr/0013-job-store-single-writer-transaction-lease.md` | job store 비주장 |
| `docs/ga-ready/EVIDENCE_INDEX.md` | 04273 evidence |
| `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md` | 다음 pair trigger |
| `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md` | P0 개발 계획 |
| `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-media-attach-design.md` | P0-1 attach 설계 |
| `docs/ga-ready/evidence/service-plan-p0-media-attach-code-level-2026-08-14.md` | P0-1 attach code-level evidence |
| `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-hyperv-saved-design.md` | P0-3 Hyper-V Saved 설계 |
| `docs/ga-ready/evidence/service-plan-p0-hyperv-saved-code-level-2026-08-14.md` | P0-3 Hyper-V Saved code-level evidence |
| `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-managed-import-design.md` | P0-4 managed import 설계 |
| `docs/ga-ready/evidence/service-plan-p0-managed-import-code-level-2026-08-14.md` | P0-4 managed import code-level evidence |
| `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone-design.md` | P1-5 managed full clone 설계 |
| `docs/superpowers/plans/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone.md` | P1-5 managed full clone 구현 계획 |
