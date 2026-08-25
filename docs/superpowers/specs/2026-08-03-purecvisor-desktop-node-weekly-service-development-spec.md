# PureCVisor Desktop Node 1주 단위 서비스 개발 명세서

- Weekly-Service-Spec-ID: `purecvisor-desktop-node-weekly-service-development-v3`
- 작성 기준: `2026-08-03`, `Asia/Seoul`
- 문서 상태: `non-authoritative-delivery-projection`
- 일정 상태: `forecast`; 달력 시작일 미확정
- Canonical controller: `Plan-ID: purecvisor-desktop-node-luna-successor-20260803`
- Active plan revision locator: `Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v3`
- Revision approval locator: `User-Approval: weekly-service-delivery-v3-luna-max-20260803`
- Projection source commit: `7e284d811d4dd5d9901b6e2b159bb89f774bad08`
- Projection row count / SHA-256: `17` / `8444b7b74697661a47e25f7f6d8ea044eb7346ef8383893b2cc681d6fbc60304`
- Model routing: S/M `gpt-5.6-luna`/`max`; L/Release `gpt-5.6-sol`/`high|ultra`
- Host mutation performed by this specification: `false`

## 1. 목적

이 문서는 PureCVisor Desktop Node의 C# 구조 개선, request lifetime 완결, ASP.NET Core transport
전환, Hyper-V provider seam, 품질·evidence 폐쇄를 1주 단위의 관찰 가능한 서비스 결과로 설명한다.

이 일정의 기본 목표는 새 제품 기능을 임의로 추가하는 것이 아니라 현재 Web Console, PCVCLI와 Local API가
제공하는 Windows/Hyper-V 기능을 유지하면서 service lifetime, HTTP transport, native provider와 evidence의
신뢰성을 높이는 것이다. ASP.NET Core는 TypeScript Web Console을 대체하지 않는다. TypeScript source,
build와 browser runtime은 유지되고 선택된 .NET server가 생성된 static asset을 제공한다.

## 2. 권위와 해석 순서

이 문서는 사람이 검토하기 위한 delivery view이며 DAG, selector, mutable state, 승인 또는 완료 판정의
새 owner가 아니다.

이 문서의 MUST/MUST NOT은 서비스 outcome 설명과 evidence projection에만 적용한다. Card 집합,
dependency, selector, 승인 또는 mutable state와 충돌하면 이 문서가 `stale/invalid`이며 실행 권한을 만들지
않는다. Successor는 아직 `controller-definition`이고 effective current가 아니므로 materialization과
activation 전에는 predecessor 실행 계획이 계속 current다.

| 대상 | Canonical owner |
|---|---|
| Required card와 direct dependency | `docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md` §5.1 |
| `current_task_id`, selector와 직렬 실행 | 같은 successor §4.2 |
| Build/install/TLS/Hyper-V/lifecycle 승인 | 같은 successor §4.4와 승인 ledger |
| 주간 anchor, commitment, carry-over와 observation | 같은 successor §5.2 및 materialization 뒤 `execution-state.json` |
| 사용자 제공 기능과 역할 | `docs/USER_FEATURE_USAGE_SPEC.md` |
| API route contract | `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` |
| CLI command contract | `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs` |
| Operational current | `docs/ga-ready/current-evidence.json` |
| Public/internal 경계 | `docs/PUBLIC_RELEASE_BOUNDARY.md`와 ADR-0006 |

충돌 시 위 owner를 우선한다. 이 문서의 주간 표는 승인된 successor §5.2를 서비스 결과 관점으로 투영한
snapshot이다. Plan revision, DAG 또는 weekly row digest가 달라지면 이 문서는 즉시 `stale`이며 실행 근거로
사용하지 않는다.

## 3. 네 계층 기준선

`current`, `installed`, `source`와 `forecast`를 하나의 PASS로 합치지 않는다.

### 3.1 Operational current

| Field | Value |
|---|---|
| 상태 | `verified_operational` |
| Version | `0.42.65-admin-smoke` |
| Active surfaces | Web Console, PCVCLI; `tui_present=false` |
| Service | `PureCVisorDesktopNode`, `Running/Automatic` |
| Default endpoints | Web `http://127.0.0.1/`, API `http://127.0.0.1:7777/api/v1/...` |
| Clean MSI SHA-256 | `5709edb0d5f265393c8690c212dd6d1f61873f7cbbaa110b1654a2e380e6b748` |
| Operational MSI SHA-256 | `9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30` |
| Payload aggregate SHA-256 | `5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28` |
| Provenance commit | `4855947fe0199cedc978e8b40ffb45e96ced6876` |
| Public claims | `public_trusted_signing=false`, `external_stable_publication=false` |

### 3.2 Installed but non-promoted candidate

| Field | Value |
|---|---|
| 상태 | `installed_non_promoted` |
| Version | `0.42.68-admin-smoke` |
| MSI SHA-256 | `99957937f00c3f26392cae86df7ea090d84f6020821348cc6eb879dd667a2e70` |
| Payload aggregate SHA-256 | `b0e47050aab167890c1a3e0bec09e4eb6f4889eb1068c1896d58ec8f15d1afa8` |
| Provenance commit | `f93370610bf221da00e89131d874e903ba72b644` |
| 확인 범위 | 관리자 설치, Web/API listener, read-only PCVCLI 3개 PASS |
| 미확인 범위 | Full admin host mutation, actual VM, package pair, ASP.NET Core, operational promotion |

`0.42.68` 설치 사실은 `0.42.65` operational current를 대체하지 않는다.

### 3.3 Current source snapshot

| Field | Value |
|---|---|
| 상태 | `code_complete_promotion_not_triggered`와 pending source의 혼합 snapshot |
| Source base HEAD | `7e284d811d4dd5d9901b6e2b159bb89f774bad08` |
| API routes | `55` |
| Route families | `13` |
| Mutation stance | ReadOnly `22`, ProductOperation `11`, QueuedMutation `22` |
| Route projection SHA-256 | `7450d1a1991efa5eeb325ec17a76c89dc524974a77c3ec1644e0ce086bad2004` |
| Installed claim | 없음; `LC-027`에서 다시 측정해야 함 |

55-route 수치는 current source contract이며 `0.42.65` 설치본이 55개 전체를 검증했다는 뜻이 아니다.
Wave 2C reconciliation과 Wave 5A lifetime source도 package/current 승격 전에는 사용자 제공 완료로 표현하지
않는다.

### 3.4 Planned weekly outcome

| Field | Value |
|---|---|
| 상태 | `forecast` |
| Horizon | `SW-01`~`SW-17`; 완료되지 않으면 `SW-18+` append |
| Start | `LC-026 completed/pass` 뒤 처음 도래하는 Asia/Seoul 월요일 00:00 |
| Window | 정확히 7일 `[starts_at, ends_at_exclusive)`; state에는 UTC 저장 |
| Completion authority | Canonical card/evidence와 validator; 주간 표 자체가 아님 |

## 4. 서비스 호환성 불변조건

모든 주차는 다음 조건을 보존한다.

1. Web Console과 PCVCLI의 활성 표면을 유지하고 TUI를 재도입하지 않는다.
2. Activation rebaseline의 API route, Hyper-V operation과 Host Ops action 집합을 승인 없이 확대·축소하지 않는다.
3. API method/path, auth/RBAC, `PCV_*` 오류, request/correlation ID, diagnostics redaction과 body cap 의미를 유지한다.
4. TypeScript Web source와 browser behavior를 유지하고 packaged/served asset hash를 검증한다.
5. VM, checkpoint, QoS와 guest mutation은 authorization, confirmation, guard와 queued job 경계를 통과한다.
6. Network inventory, Activity, Evidence와 Monitoring 조회 화면에서 새 OS mutation을 실행하지 않는다.
7. Service/MSI/firewall/trust-store/LAN/Event Log/TLS/update/rollback 작업은 관리자 opt-in runbook으로 분리한다.
8. Linux KVM/libvirt/LXC/ZFS/OVS/OVN 의미와 public signing/publication을 Desktop Node 완료 범위에 넣지 않는다.
9. Token, password, JWT, credential value와 certificate private key를 log, evidence 또는 diagnostic bundle에 남기지 않는다.
10. 실패한 설치·VM mutation은 rollback, expected final state와 cleanup evidence 없이 PASS로 닫지 않는다.

사용자 역할은 `viewer` 조회, `operator` VM/checkpoint/job/diagnostic/console operation, `admin` 별도
service/update/security runbook으로 구분한다. 다음 값은 현재 설치 호스트 관찰이 아니라 제품 기본 계약이다:
`account=no-default-account`, account 미구성 시 protected bearer token gate authoritative, noVNC target 미구성 시
`not_configured`와 Hyper-V `vmconnect` handoff만 기본 제공한다.

## 5. 주간 commitment 계약

### 5.1 선행 gate

`LC-001`~`LC-026`은 materialization, activation과 effective-current 증명을 담당하며 기간을 약속하지 않는다.
이 gate가 닫히기 전에는 실제 날짜, capacity 또는 주간 commitment를 확정하지 않는다. `SW-01` anchor는
`LC-026` PASS instant보다 엄격히 뒤에 있는 첫 Asia/Seoul 월요일 00:00이다. Anchor를 기다리느라 eligible
card를 멈추지 않으며, LC-026 PASS와 anchor 사이에 완료된 card는 pre-anchor result로 기록하고 첫 forecast
revision에서 해당 pool로부터 제거한다.

### 5.2 Definition of Ready

한 주의 exact commitment에는 다음이 모두 필요하다.

- `planning_base_head`와 canonical selector simulation artifact
- planning base에서 선행 card 성공을 가정해 selector를 반복한 deterministic prefix인 ordered
  `planned_card_ids`; 각 card의 dependency와 eligibility는 실제 실행 시점에 PASS해야 함
- 현재 주 또는 바로 다음 주라는 시간 조건
- `available_capacity_hours`와 card별 독립 `estimated_effort_hours`
- planned effort가 available capacity 이하라는 계산
- 필요한 test environment, clean host, actual VM과 external runner availability
- mutation card별 `approval_categories`, approval acquisition plan과 예상 target; exact command/artifact hash/target의
  별도 approval record는 해당 card가 `approval_required`가 된 뒤 실제 mutation 직전에 취득
- observation 중이면 payload tuple과 sample cadence가 유지된다는 확인

`S|M|L` 검증 위험 등급을 개발 시간으로 환산하지 않는다.

### 5.3 Definition of Done

한 주는 다음 조건을 모두 만족할 때만 `met`로 파생한다.

- commitment의 모든 card가 `completed/pass`
- 주간 exit condition과 필요한 review가 PASS
- 요구된 Fast/Full/Release lane과 skip/coverage guard가 PASS
- result/evidence commit과 artifact locator가 존재
- 승인된 mutation의 rollback, cleanup과 expected final state가 확인됨
- secret redaction과 public/internal boundary가 유지됨
- 다음 주로 넘길 미완료 card 또는 blocker가 없음

주말이 됐다는 사실이나 문서 checkbox만으로 완료 처리하지 않는다.

### 5.4 상태와 carry-over

`forecast|committed|in_progress|met|partial|blocked|superseded`는 append-only event, 현재 시간과
canonical card/evidence에서 파생한다. 과거 row/event를 수정하지 않는다. 미완료 card는 다음 forecast
revision의 첫 항목으로 옮기고 `carryover_from`, blocker와 superseding locator를 기록한다. `SW-17` 뒤에도
완료되지 않으면 동일한 7일 규칙으로 `SW-18+`를 append한다.

완료된 card는 다시 계획하지 않는다. Carry-over 때문에 같은 미완료 card가 여러 historical week에
나타나면 새 row의 `carryover_from`이 직전 row와 immutable locator를 정확히 연결해야 한다. 과거 row를
삭제하거나 최신 row로 덮어서 이력을 한 주처럼 보이게 만들지 않는다.

### 5.5 Observation

Required observation ID는 정확히 다음 세 개이고 dependency상 순차다.

| Observation | Start/close projection |
|---|---|
| `OBS-LIFETIME-LEGACY-TRACKED` | SW-03 start, SW-05 close |
| `OBS-ASPNET-DEFAULT` | SW-10 start, SW-11 close |
| `OBS-ASPNET-ONLY` | SW-15 start, SW-17 close |

각 observation은 독립된 7×24시간, 최소 8개 sample, sample gap 26시간 이하와 P0/P1 0건을 요구한다.
따라서 재시작이 없어도 관찰 시간만 최소 21일이다. Executable one-shot sampler owner는 `OP-003`이 만드는
`packaging/windows-desktop-node/tools/Invoke-PcvInstalledObservationSample.ps1`과 대응 Pester다. Sample은
최소 `sample_id`, `attempt_id`, `sampled_at_utc`, `previous_sample_id`, `gap_seconds`, installed 4-tuple,
service/transport/listener/current-card 결과, P0/P1 query window/count, package/recovery event, source artifact와
overall status를 기록한다.

Start는 version, operational MSI SHA-256, payload aggregate SHA-256, provenance commit, observed payload hash,
workspace source HEAD, attempt ID와 UTC start를 고정한다. Payload drift, sample 누락, 26시간 초과 gap 또는
P0/P1 회귀가 발생하면 해당 attempt를 처음부터 다시 시작한다. 주간 경계에 맞추기 위해 관찰 시간을
반올림하거나 이전 attempt 시간을 합산하지 않는다. 최종 current evidence, installed current-card,
rollback evidence와 `OBS-ASPNET-ONLY`는 같은 version/operational-MSI/payload/provenance 4-tuple이어야 한다.

### 5.6 Canonical state와 companion outcome record

이 명세는 `execution-state.json` schema를 확장하지 않는다. Canonical state에는 successor §5.2가 소유하는
다음 field만 기록한다.

- `week_id`, `sequence`, `starts_at_utc`, `ends_at_exclusive_utc`, `goal`
- `forecast_card_pool`, exact ordered `planned_card_ids`, `planning_base_head`, `selection_simulation_ref`
- `entry_conditions`, `exit_conditions`, `approval_categories`, `observation_ids`
- `available_capacity_hours`, card별 `estimated_effort_hours`, `planned_effort_hours`
- `carryover_from`, `carryover_blockers`, `result_refs`, append-only `events`, validator-derived `derived_status`

사람이 읽는 주간 서비스 보고서가 필요하면 state를 바꾸지 않는 immutable companion artifact에 다음
projection field를 둔다.

- `source_week_ref`, `projection_source_commit`, `projection_row_digest`
- `outcome_class`: `code-ready|installed-non-promoted|actual-host-verified|observation-gate|quality-gate|promotion`
- `service_outcome`, `feature_contract_refs`, `proof_refs`, `non_claims`
- Commitment 때의 approval acquisition plan과, 취득 후 연결하는 exact approval/artifact/command/target/rollback refs

아래 baseline 표는 forecast snapshot이지 committed week record가 아니다. 위 companion field의 exact 값과
기능 문서 locator는 실제 commitment/review artifact에서 채운다.

## 6. 17주 서비스 개발 명세

아래 card pool은 successor §5.2의 비규범 projection이다. Direct dependency와 selector를 재정의하지 않는다.

| 주차 / successor §5.2 projection pool | Outcome class | 서비스 제공 결과 | API / Web / CLI 영향 | 주간 acceptance와 evidence | 승인 category |
|---|---|---|---|---|---|
| SW-01 `LC-027..028`, `LT-001..005` | `code-ready` | Effective-current 재기준화와 bounded lifetime 기반 | Route/command/UI 계약 불변; 사용자 기능 변화 없음 | 55-route/34-op/22-action 재측정, late commit 방지·cancel/admission/noVNC foundation, focused+Full PASS | 없음 |
| SW-02 `LT-006..011` | `code-ready` | Listener/worker fault, drain, overload와 tracked lifetime code 완결 | Stable overload/error만 노출; route/command shape 불변 | Fault injection, 10초 drain, `503`/`Retry-After`, static/noVNC/service parity, Release PASS | 없음 |
| SW-03 `OP-001..003`, `OP-005`, `OP-LT-101..105` | `installed-non-promoted` | Legacy-tracked candidate build/install/실제 환경 폐쇄와 첫 관찰 시작 | 기존 API/Web/CLI를 새 lifetime에서 동일 제공하는 non-promoted candidate | Exact artifact tuple, current-card, 반복 start/load/drain/noVNC/actual-VM/lifecycle와 rollback PASS | `package_service/build`, `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-04 `HO-100..110` | `code-ready` | 9-family/22-action Host Ops owner 이동 | Ops summary와 diagnostics 의미 유지; 사용자 기능 변화 없음 | Callback-free owner, focused owner tests, Full PASS, observation cadence 유지 | 없음 |
| SW-05 `OP-LT-106`, `LT-012`, `AC-301..303`, `AC-307`, `OP-AC-302..305` | `installed-non-promoted` | 첫 관찰 종료, legacy lifetime code 제거와 static/noVNC hardening candidate | 55-route/CLI 불변; Web 보안 강화 candidate이며 operational current는 유지 | Observation ≥7일/8 samples/P0·P1 0, Origin/containment/hash parity, install/rollback PASS | `package_service/build`, `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-06 `AC-304..306`, `AC-401..406` | `code-ready` | Exclusive ASP.NET transport와 core pipeline code path | 아직 default/current 제공 아님; TypeScript Web 유지 | Adapter, body cap, static, noVNC, auth, admission과 service lifetime dual-path parity | 없음 |
| SW-07 `AC-407..411`, `OP-AC-411..413`, `AC-501` | `installed-non-promoted` | ASP.NET opt-in 실제 server parity와 source-default 준비 | Opt-in candidate에서 동일 API/Web/CLI; operational current 유지 | 55-route exact parity, dynamic bind, self-contained publish, TLS/install/rollback/current-card PASS | `package_service/build`, `package_service/install`, `http_binding_tls`, `lifecycle_rollback` |
| SW-08 `OP-501..503` | `installed-non-promoted` | ASP.NET-default/legacy-retained candidate | Candidate에서 기존 API/Web/CLI를 ASP.NET Core로 제공; TypeScript/CLI 계약 불변 | Exact tuple, Running/Automatic, configured=effective server, Web/API/CLI/current-card, 반복 start/performance budget PASS | `package_service/build`, `package_service/install`, `http_binding_tls` |
| SW-09 `HV-101..108`, `HV-110..113`, `OP-004` | `code-ready` | Hyper-V canonical registry와 read/provider seam | Host/network/VM readback 의미 불변 | 34-operation projection, fake executor/read provider, mutation-disabled actual-VM PlanOnly PASS | 없음 |
| SW-10 `OP-504..506` | `installed-non-promoted` | ASP.NET-default actual-VM/lifecycle 폐쇄와 두 번째 관찰 시작 | Default candidate의 VM/job/noVNC 기능을 실제 host에서 검증 | Targeted actual-VM, queued mutation/cancel/recovery, install/update/repair/rollback, target restore/rehash PASS | `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-11 `OP-507` | `observation-gate` | ASP.NET-default 안정성 관찰 완료 | 새 기능 없음; default candidate 안정성 증명 | Observation ≥7일, ≥8 samples, gap ≤26시간, P0/P1 0; payload freeze | 없음 |
| SW-12 `AC-600..603` | `code-ready` | `HttpListener` production reachability source 제거 | Code-level `aspnet_only`; final 설치본 제공 전 | Historical fixture 유지, selector/implementation/package reachability 0, compiled guard와 Release PASS | 없음 |
| SW-13 `HV-120..128`, `OP-HV-120..128` | `actual-host-verified` | Hyper-V mutation seam의 code/actual-VM evidence 폐쇄; package/current 승격 없음 | API/CLI와 Web 지원 subset backend 신뢰성 검증; 사용자 기능 확대 claim 없음 | Create/delete/power/checkpoint/rename/eject/compute/disk/QoS별 fake matrix, pre/post/readback/cleanup, remaining VM 0 | `hyperv_actual_vm` |
| SW-14 `QG-101..115` | `code-ready` | Evidence owner, architecture와 coverage 폐쇄 | Evidence/Troubleshooting 표시 계약 유지; current 승격 없음 | Path containment, projector/architecture tests, historical link 정리, TRX/Cobertura 0.0%p ratchet | 없음 |
| SW-15 `OP-601..606` | `installed-non-promoted` | ASP.NET-only final candidate와 최종 관찰 시작 | 동일 55-route, TypeScript Web과 PCVCLI를 ASP.NET-only service에서 candidate로 제공 | Final tuple build/install/TLS/current-card/actual-VM/lifecycle/target restore PASS | `package_service/build`, `package_service/install`, `http_binding_tls`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-16 `FC-001..002` | `quality-gate` | Final 품질·Release lane 폐쇄와 관찰 유지 | Product payload/사용자 기능 변화 없음 | 전체 .NET/Web/Pester/installer/Release, skip 0, coverage ratchet, observation cadence PASS | 없음 |
| SW-17 `OP-607`, `FC-003..007` | `promotion` | 최종 관찰과 원자 승격·완료 attestation | 이 주의 atomic promotion 뒤에만 ASP.NET-only Web/API/CLI를 operational current로 주장 | Observation PASS, FA 28/DOD 12, exact final tuple, atomic current/index promotion과 post-merge attestation | release/current-evidence promotion checkpoint; §4.4 host-mutation category 아님 |

### 6.1 공식 사용자 기능 계약 추적

아래 표는 `docs/USER_FEATURE_USAGE_SPEC.md`의 기능 매트릭스를 canonical card evidence에 연결하는
companion trace다. 새 card나 완료 gate를 만들지 않는다. 표시된 주차의 기존 `result_refs`에서 proof locator를
수집하고, 증거가 없으면 기능을 PASS로 추정하지 않고 gap으로 보고한다. 새 실행이 필요하면 successor
Plan-Revision으로만 추가한다.

| 공식 기능 계약 | Candidate evidence 주차 | Actual-host / final candidate 주차 | SW-17 승격 조건 | 금지 claim |
|---|---|---|---|---|
| Host status · Runtime policy (§89) | SW-03 current-card, SW-08 `OP-502` | SW-15 `OP-602` | 동일 final tuple의 Web/API/CLI locator 연결 | SW-08 candidate를 operational current로 표현 금지 |
| Network inventory (§102) | SW-03 current-card, SW-08 Web/API/CLI parity | SW-15 `OP-602` | 조회 결과와 route/CLI/Web locator 연결 | 조회가 network mutation을 수행한다고 표현 금지 |
| VM list/detail (§115) | SW-03 actual-VM/current-card | SW-10 `OP-504`, SW-15 `OP-604` | 동일 final tuple의 inventory/readback locator 연결 | Test VM을 사용자 보존 VM으로 표현 금지 |
| VM create (§134) | SW-10 queued mutation smoke | SW-13 `HV-120`/`OP-HV-120`, SW-15 `OP-604` | Authorization/confirmation/job/cleanup locator 연결 | Web 미지원 operation을 지원한다고 확대 금지 |
| VM power (§147) | SW-10 queued mutation/cancel/recovery | SW-13 `HV-122`/`OP-HV-122`, SW-15 `OP-604` | 공식 start/guest-shutdown/forced-poweroff/restart와 추가 pause/resume 각각의 readback·cleanup 연결 | CLI `stop` alias나 API/CLI 추가 operation을 Web 지원 증거로 대체 금지 |
| VM QoS/guest readback (§166) | SW-03 actual-VM compatibility | SW-13 `HV-128`/`OP-HV-128`, SW-15 final current-card/actual-VM | QoS unit·pre/post와 별도 guest readback locator 연결 | QoS proof를 guest execution proof로 대체 금지 |
| VM delete (§213) | SW-10 queued mutation smoke | SW-13 `HV-121`/`OP-HV-121`, SW-15 `OP-604` | Managed guard, final inventory와 cleanup 연결 | Unmanaged VM 삭제 지원 claim 금지 |
| Checkpoints (§233) | SW-10 queued mutation smoke | SW-13 `HV-123`/`OP-HV-123`, SW-15 `OP-604` | Create/restore/delete readback과 cleanup 연결 | Checkpoint cleanup 누락 상태에서 PASS 금지 |
| Jobs/Activity (§246) | SW-03 lifetime/admission, SW-10 cancel/recovery | SW-15 `OP-604` | Queued job terminal state와 Activity locator 연결 | Running job을 성공으로 간주 금지 |
| Diagnostics (§261) | SW-08 `OP-502` | SW-15 `OP-602` | Create/list/download, containment와 redaction locator 연결 | Secret 포함 bundle 또는 UI-only smoke로 PASS 금지 |
| Account/RBAC/JWT (§280) | SW-08 `OP-502` | SW-15 `OP-602` | `viewer/operator/admin`, auth/session negative locator 연결 | 기본 계정 존재 또는 token 값 기록 claim 금지 |
| Console/noVNC (§309) | SW-03 noVNC, SW-05 hardening, SW-08 `OP-502` | SW-10 `OP-504`, SW-15 `OP-604` | Origin/auth/target/stream 또는 `not_configured` locator 연결 | Target 미설정 시 streaming 제공 claim 금지 |

이 표의 주차 표시는 forecast다. 실제 companion artifact의 `feature_contract_refs`는 문서 section뿐 아니라
정확한 test/evidence locator를 포함해야 하며, candidate·actual-host·operational current 상태를 서로 바꾸어
표현하지 않는다.

## 7. 승인과 mutation 명세

이 문서 승인, 주간 commitment, Git push/PR/merge 승인은 host mutation 승인이 아니다.

| Category | 별도 승인 시 고정할 값 | 완료 시 요구 증거 |
|---|---|---|
| `package_service/build` | Source HEAD, version, recipe, signing mode, output path | Artifact hash, provenance, host mutation 0건 |
| `package_service/install` | Exact MSI/hash, host, service action과 data preservation | Install log, service final state, manifest/hash와 rollback plan |
| `http_binding_tls` | Prefix/port, URL ACL, certificate thumbprint, SSL binding | Listener ownership, auth boundary, rollback과 final binding state |
| `hyperv_actual_vm` | Host, exact VM/checkpoint/operation, cleanup target | Pre/post/readback, remaining VM, cleanup와 failure recovery |
| `lifecycle_rollback` | Baseline/target tuple, update/repair/rollback/uninstall command | Version transition, health, data hash, service/host final state |

여러 category가 필요한 주는 모든 record가 승인돼야 한다. Exact command, artifact hash와 target이 달라지면
승인을 다시 받아야 하며 이전 주의 승인을 carry-forward하지 않는다.

## 8. 서비스 제공 상태 표현

| Label | 허용 의미 |
|---|---|
| `verified_operational` | Current evidence가 exact installed tuple과 사용자 표면을 검증함 |
| `installed_non_promoted` | 설치·한정 smoke는 PASS했지만 current 승격 gate가 열려 있음 |
| `code_complete_promotion_not_triggered` | Source/test 완료; 설치 사용자 제공 claim 없음 |
| `forecast` | Capacity·dependency·승인 확인 전의 예정 결과 |
| `blocked` | 선행 조건 또는 외부 gate가 없어 실행 불가 |

`code_complete`, package PASS, installed smoke와 operational current를 모두 “서비스 제공 완료”로 표현하지 않는다.

## 9. 현재 blocker와 refresh trigger

### 9.1 Control 상태

- `WSD-B001`: successor v3가 §5.2를 canonical owner로 유지하고 §19 SW-01 seed를
  `LC-027`, `LC-028`, `LT-001`~`LT-005`로 정렬한다. Revision commit
  `7e284d811d4dd5d9901b6e2b159bb89f774bad08`, merge commit
  `7c20b87c01826536cc9cf66f9844af8bbe82cc8e`와 post-merge CI run `30821012028` /
  `30821011947`이 이를 보존한다. v2에서 확정한 17개 row와 digest는 바뀌지 않았으므로 resolved다.
- `WSD-B002/bootstrap`: stable-design amendment commit
  `49fb197ae5ae85bb30f1b7821ec5694274f9663c`와 merge commit
  `d93051fbcfa34e83f46e752624306cd94c2ff3c9`에서 resolved다.
- `WSD-B002/max-routing`: Luna Max amendment commit
  `6fb8a3e946391b70b2ca968189e7e7fa824f6863`, merge commit
  `c973cfe14aae6027ab1c5f934ec0228e8b291b71`와 post-merge CI run `30820050609` /
  `30820050083`에서 stable projection은 resolved다.
- `WSD-B002/materialization`: `User-Approval: luna-control-materialization-dbac0ae5abd8-20260803`은
  LC transaction에 사용되지 않은 v2/ultra stale approval이다. 이 v3 파생 문서 merge 뒤 exact fresh-main
  SHA를 대상으로 새 승인을 받고 LC-001~LC-024가 완료될 때까지 pending이다.
- `WSD-B002/activation`: merge-group/required-check/ruleset 증명, LC-025 activation과 LC-026 attestation이
  PASS할 때까지 pending이다. 현재 GitHub private/free 경계에서는 해당 enforcement 증명이 없어 blocked다.
- Runtime에서 `Luna Max (gpt-5/6-luna, max)` selection의 alias가 canonical execution ID
  `gpt-5.6-luna`로 resolve됨을 증명하지 못하면 `blocked/model_identifier_unresolved`, callable하지 않으면
  `blocked/model_unavailable`이다. 두 경우 모두 Sol/Terra로 자동 대체하지 않는다.

### 9.2 Operational blocker 후보

- External lifecycle runner와 clean-host/actual-VM availability
- Installed baseline/version mismatch
- TCP 7777 listener ownership 또는 excluded range 회귀
- Source/MSI/payload/provenance tuple 불일치
- Observation payload drift, sample gap 또는 P0/P1 회귀

### 9.3 Refresh trigger

다음 중 하나가 발생하면 이 문서를 다시 생성·감사한다.

- Active `Plan-Revision`, successor weekly row 또는 canonical DAG 변경
- `LC-027` route/operation/action rebaseline 결과 변경
- Current operational tuple 또는 public claims 변경
- API route digest, CLI/Web contract 또는 TUI boundary 변경
- Weekly row digest 불일치, active revision 교체 또는 `WSD-B002` subgate 상태 변경

## 10. 검증 계약

문서 변경은 최소 다음을 확인한다.

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check
git diff --check
```

Projection digest는 successor source commit에서 SW-01~17 raw Markdown row를 순서대로 UTF-8/LF로 join하고
마지막 LF를 붙이지 않아 계산한다.

```powershell
$plan = 'docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md'
$rows = @(Get-Content -LiteralPath $plan | Where-Object { $_ -match '^\| SW-(?:0[1-9]|1[0-7]) \|' })
$bytes = [Text.Encoding]::UTF8.GetBytes($rows -join "`n")
[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
```

추가 정적 감사:

- Successor weekly source row 수 `17`과 SHA-256 일치
- Canonical 158-card coverage의 duplicate/missing/extra `0`
- Weekly dependency reversal `0`
- 모든 문서 link 존재
- 변경 파일에 secret, current promotion 또는 mutation evidence claim 없음
- 사용자 소유 untracked 파일을 stage하거나 수정하지 않음

## 11. 완료 기준

이 명세의 17주 baseline을 예정대로 소화한 사실만으로 프로젝트 100%가 되지 않는다. Canonical FA 28개와
DOD 12개, 세 observation, exact package tuple, atomic current evidence promotion과 post-merge attestation이
모두 PASS할 때만 successor validator가 program complete를 파생한다. `SW-17`에 미완료가 있으면 실패를
숨기지 않고 `SW-18+`로 carry-over한다.
