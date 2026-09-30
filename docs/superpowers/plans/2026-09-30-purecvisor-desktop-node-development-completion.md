# 코어/백엔드/프론트 개발 완료 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 소스에 남은 개발 공백을 닫는다. 백엔드는 상태를 관찰할 수 있는 모든 mutation job에 조건부 reconcile을 둔다.
프론트는 정책으로 닫힌 route만 빼고 Web Console binding을 채운다. 코어 계약은 그 최종 상태를 테스트로 고정한다.

**Architecture:** task 하나가 Lane 1 checkpoint 하나다(30분, tool batch 18회). 브랜치
`feat/development-completion-20260930`에서 task마다 로컬 commit한다. push와 PR은 이 campaign이 승인하지 않았다.

**Tech Stack:** C# / .NET 10 xUnit, TypeScript Web Console(`web/src/served/*.ts` → `web/app.js`), JSON 계약 ledger

## 사용자 결정 (2026-09-30)

| 항목 | 결정 |
| --- | --- |
| 목표 | "코어.백엔드.프론트 개발 완료를 목표로 작업 개시" |
| 범위 | Lane 0/1 소스 작업. 백엔드 reconcile 범위, Web Console binding, 코어 계약 종결 |
| commit | task마다 로컬 commit (`local-commit-per-task`, 2026-09-27 정책) |
| 승인 밖 | push/PR, host mutation, Lane 2 actual-VM, package pair, Lane 3, `current-evidence.json` 쓰기 |

## 착수 시점 공백 (2026-09-30, `f72d5fe`)

- **백엔드:** `job.reconcile`은 13개 operation만 다룬다(`vm.delete`, `vm.create`, `vm.shutdown`, `vm.restart`,
  `vm.rename`, `checkpoint.create`, `checkpoint.restore`, `checkpoint.schedule.set/clear`,
  `vm.qos.storage.set`, `vm.qos.network.set`, `console.novnc-target.set/clear`). 나머지 mutation은 분류가 없다.
- **프론트:** `config/desktop-node-feature-surface-ledger.json`의 Web 제외가 `20`개다. 그중 `16`개는 운영 흐름
  선택으로 제외됐다. 나머지 `4`개는 정책 근거가 있다. noVNC target 저장 `3`개는 `SERVICE_PLAN.md` §7.2
  "지금은 열지 않음"이 근거이고, `vm.limit`은 Web의 명시적 QoS/자원 제어와 겹친다.
- **코어:** surface ledger가 "개발 완료" 상태를 테스트로 고정하지 않는다. Web 제외가 다시 늘어나도 막는 장치가 없다.

## Global Constraints

- Lane 1만 쓴다. Hyper-V VM, service, MSI, firewall, Event Log mutation은 하지 않는다.
- reconcile 규칙은 P1-10과 같다. 대상은 `failed` 상태이면서 `PCV_JOB_INTERRUPTED`인 job뿐이다. `succeeded`는
  제출 때 잡은 before-state, 단일 identity, 기대한 after-state readback이 모두 맞을 때만 준다. 하나라도 모호하면
  `409 PCV_JOB_RECONCILIATION_REQUIRED`를 돌려준다. 자동 retry는 하지 않는다.
- 모듈 크기 라쳇(`packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`)을 넘기지 않는다.
  `DesktopNodeApiJobReconciliationHandler.cs`(`674`줄)와 `web/src/served/mutate.ts`처럼 상한에 가까운 파일에는
  새 family를 partial/신규 모듈로 둔다.
- `web/app.js`는 생성물이다. `web/src/served`를 고치고 `npm run build:served --prefix web`로 만든다.
- Web destructive 동작은 대상 이름을 보이고 확인을 받는다. preview가 있는 family는 preview를 먼저 둔다.
- noVNC target Web 저장 폼과 `vm.limit` Web binding은 열지 않는다.
- 새 `Add-Type`, P/Invoke, native ACL, installer handoff가 필요하면 멈춘다.
- public trusted signing과 external stable publication은 주장하지 않는다.

## 공통 검증

- C#: `dotnet test src/DesktopNode.Api.Tests`, ledger를 읽는 Cli 테스트(`DesktopNodeCliCommandCatalogTests`,
  `DesktopNodeCliProjectContractTests`)
- Web: `npm run test:required --prefix web` (`check:feature-surfaces`, `check:served`, web contract, static parity 포함)
- 공통: `git diff --check`

## Task 1: 전원 reconcile — `vm.start`, `vm.poweroff`

- [x] 제출 때 before-state를 잡는다(`VmCapture` 패턴). reconcile은 VM identity와 전원 상태 readback(`Running`/`Off`)으로 판정한다.
- [x] 새 partial(`DesktopNodeApiJobReconciliationHandler.PowerReconcile.cs`)에 두고, 본 파일 dispatch가 라쳇 상한을 넘으면 dispatch를 표 형태로 줄인다.
- [x] 성공, 상태 불일치, identity 모호, readback 실패를 Api 테스트로 고정한다.
- [x] 검증: `dotnet test src/DesktopNode.Api.Tests`, `git diff --check`

실행 기록(2026-09-30): 새 schema `pcv-vm-power-state-reconciliation/v1`이다. reconcile 허용 목록은 Api dispatch와 Runtime `IsReconciliationSupportedOperation` 두 곳에 있어서 둘 다 넓혔다. 본 파일은 dispatch를 switch 표로 줄여 `674`→`629`줄이 됐다. Runtime `Persistence.cs`는 허용 목록을 패턴식으로 줄여 상한 `505` 안(`498`)에 뒀다. 큐 등록 때 `vm.list` readback이 생겨서, 호출 수를 세는 기존 fake adapter 두 개는 그 readback을 mutation 호출로 세지 않게 했다. 검증: Api `424`/`424`, Runtime `128`/`128`, Delivery `744`/`744`, `git diff --check`.

## Task 2: 일시정지/저장 reconcile — `vm.pause`, `vm.resume`, `vm.save`, `vm.resume-saved`

- [x] Task 1 partial에 `Paused`/`Saved`/`Running` 기대 상태를 더한다.
- [x] 네 operation마다 성공과 불일치 테스트를 둔다.
- [x] 검증: `dotnet test src/DesktopNode.Api.Tests`, `git diff --check`

실행 기록(2026-09-30): Task 1 schema를 그대로 쓰고 기대 상태(`paused`, `running`, `saved`)만 더했다. 이제 lifecycle route 여섯 개가 모두 큐 등록 때 `vm.list` baseline을 잡는다. `saving` 같은 전이 상태는 `incomplete-power-state`다. 검증: Api `436`/`436`, Runtime `128`/`128`, Delivery `744`/`744`, `git diff --check`.

## Task 3: `checkpoint.delete` reconcile

- [x] 제출 때 checkpoint identity를 잡는다(`CheckpointCapture` 패턴). readback에서 그 identity가 사라졌을 때만 `succeeded`다.
- [x] 같은 이름의 다른 checkpoint가 남은 경우처럼 모호하면 `409`를 준다.
- [x] 검증: `dotnet test src/DesktopNode.Api.Tests`, `git diff --check`

실행 기록(2026-09-30): schema는 `pcv-checkpoint-delete-reconciliation/v1`이고 identity는 `(vm_name, name, created_at)`이다. 같은 이름이 남아 있으면 `created_at`이 같을 때 `not-applied`, 다를 때 `replacement-observed`, 둘 이상이면 `ambiguous-duplicate-names`다. 모두 `409`다. `report-only`: schedule retention worker(`DesktopNodeCheckpointScheduleDueWorker`)가 넣는 `checkpoint.delete`는 baseline이 없다. 그래서 중단되면 `baseline-unavailable`이다. 검증: Api `443`/`443`, Runtime `128`/`128`, Delivery `744`/`744`, `git diff --check`.

## Task 4: 자원 reconcile — `vm.set-memory`, `vm.set-vcpu`, `vm.disk-resize`

- [x] 요청 값과 before-state를 잡는다. readback이 요청 값과 같을 때만 `succeeded`다. disk는 shrink가 없으므로 before 이하 값은 불일치다.
- [x] 검증: `dotnet test src/DesktopNode.Api.Tests`, `git diff --check`

실행 기록(2026-09-30): schema는 `pcv-vm-resource-reconciliation/v1`이다. identity는 바뀌는 자원 값을 빼고 `id`, `platform`, `guest_family`, `generation`, `managed_by_purecvisor`로 잡는다. disk는 provider와 같은 규칙(첫 번째로 연결된 VHD/VHDX)으로 경로를 고정한다. readback 값이 요청 값이면 성공, before 값이면 `not-applied`, 그 밖이면 `incomplete-resource-value`다. `vm.limit`은 baseline을 잡지 않는다(Task 6 분류). Runtime `Persistence.cs`가 상한 `505`에 닿았으니 다음 task에서 허용 목록과 mutation 이름을 한 표로 합친다. 검증: Api `455`/`455`, Runtime `128`/`128`, Delivery `744`/`744`, `git diff --check`.

## Task 5: media/잠금 reconcile — `vm.attach`, `vm.eject`, `vm.template.lock`

- [x] DVD media path readback과 template lock marker readback으로 판정한다. (착수 실측: `vm.list`에 DVD media 경로가 없어 `vm.template.lock`만 구현했다. `vm.attach`/`vm.eject`는 Task 6 비대상으로 넘긴다.)
- [x] 검증: `dotnet test src/DesktopNode.Api.Tests`, `git diff --check`

실행 기록(2026-09-30): schema는 `pcv-vm-template-lock-reconciliation/v1`이다. `vm.list`는 잠기지 않은 VM의 `template_lock`을 생략하므로 없으면 `false`로 읽는다. 착수 실측 결과 `vm.list` storage에는 `vhdx`만 있고 `size_gb`는 항상 `null`이다(`DesktopNodeHyperVWmiVmProvider.cs:338`). DVD는 개수(`dvd_drives.count`)만 있다. 그래서 `vm.attach`/`vm.eject`는 readback이 없어 비대상이다. Task 4 `vm.disk-resize` reconcile은 실제 호스트에서 `readback-value-unavailable`(`409`)만 낸다(거짓 성공 없음). 둘 다 `vm.list` readback 확장이 필요하며 계획 밖 결정 항목에 올렸다. Runtime은 허용 목록과 mutation 이름을 사전 하나(`ReconcilableMutations`)로 합쳤다(`504`줄). 검증: Api `462`/`462`, Runtime `128`/`128`, Delivery `744`/`744`, `git diff --check`.

## Task 6: 나머지 mutation reconcile 분류

- [x] `vm.network.connect`는 NIC switch readback으로 reconcile한다.
- [x] 관찰할 수 없거나 부작용이 반복될 수 있는 operation은 이유를 붙여 명시적 비대상으로 분류한다. 후보는 `vm.attach`, `vm.eject`(Task 5 실측: media readback 없음), `vm.guest.*`,
      `account.*`, `diagnostic.bundle.create`, `vm.device.add`, `vm.clone`, `vm.import`, `vm.export`, `vm.manage`,
      `vm.limit`이다. 착수 때 readback 가능 여부를 다시 확인해 확정한다.
- [x] surface ledger의 모든 mutating operation이 "reconcile 대상" 또는 "이유 있는 비대상" 중 하나라는 테스트를 둔다.
- [x] 검증: `dotnet test src/DesktopNode.Api.Tests`, `git diff --check`

실행 기록(2026-09-30): 대상은 Runtime `DesktopNodeJobRuntime.ReconcilableMutations`(public, `26`개)가, 비대상과 이유는 Api `ReconcileNonTargets`(`19`개)가 소유한다. `vm.network.connect`는 연결된 switch가 요청 switch 하나뿐이고 before에 그 switch가 없었을 때만 성공이다. `vm.manage`는 착수 실측에서 `managed_by_purecvisor` readback이 있어 비대상 후보에서 대상으로 옮겼다. identity는 VM `id`다. 비대상 job을 reconcile하면 `409`와 분류 이유가 나온다. Runtime 거부 경로에 남아 있던 고정 operation 목록 메시지는 표에서 만든다. `ApiReconcileClassificationTests`가 ledger의 mutating operation 전부가 둘 중 정확히 한 곳에 있음을 고정한다. 검증: Api `474`/`474`, Runtime `128`/`128`, Delivery `744`/`744`, `git diff --check`.

## Task 7: Web pause/resume, rename

- [x] VM detail에 Pause/Resume 버튼과 Rename(새 이름 입력, 대상 이름 확인) 동작을 둔다. `routes.ts` 등록, 오류 매핑, 신규 모듈을 둔다.
- [x] ledger의 `vm.pause`, `vm.resume`, `vm.rename`을 Web present(`coverage_id`)로 옮기고 `FEATURE_IMPLEMENTATION_LEDGER.md`,
      `USER_FEATURE_USAGE_SPEC.md` 투영을 맞춘다.
- [x] `npm run build:served --prefix web`, `npm run generate:parity --prefix web`
- [x] 검증: 공통 검증 전체

실행 기록(2026-09-30): 새 모듈 `web/src/served/vm-detail-extensions.ts`가 lifecycle action 표(`VM_LIFECYCLE_ACTIONS`)와 확장 submit hook(`handleVmDetailExtensionSubmit`)을 가진다. `served-app.ts`는 `426`→`419`줄로 줄었다(상한 `429`). 다음 Web task는 이 hook에 붙는다. Web 제외는 `20`→`17`이며, `verify-feature-surface-parity.mjs`의 고정 개수도 `63`/`17`로 바꿨다. `USER_FEATURE_USAGE_SPEC.md`의 VM power 행은 이미 "VM detail actions"라 바꾸지 않았다. 검증: `npm run test:required --prefix web` exit `0`, web Pester `50`/`0`, Cli `179`/`179`, Api `474`/`474`, Delivery `744`/`744`, `git diff --check`.

## Task 8: Web telemetry — `vm.memory-stats`, `vm.cpu-stats`

- [x] VM detail에 memory/CPU telemetry 읽기 패널을 둔다. 읽기 전용이며 실패는 `PCV_*`와 다음 행동을 보인다.
- [x] ledger와 투영 문서를 맞추고 build/parity를 재생성한다.
- [x] 검증: 공통 검증 전체

실행 기록(2026-09-30): 새 패널 대신 기존 VM detail "QoS / Guest Readback" 패널의 readback 로더에 `memory_stats`, `cpu_stats` 단계와 카드 두 개를 더했다. 같은 Refresh 버튼을 쓰고, 실패는 카드에 `PCV_*`와 detail로 보인다. browser fixture에 두 GET 응답과 기대 문자열을 더했다. Web 제외는 `17`→`15`이다. `report-only`: browser fixture가 "no schedule save form", "no export/import save form", "CLI/API vm.network.connect only" 같은 제외 문구를 기대 문자열로 고정하고 있어서, Task 9~11은 그 문구와 fixture를 함께 바꿔야 한다. 검증: `npm run test:required --prefix web` exit `0`, web Pester `50`/`0`, Cli `179`/`179`, Api `474`/`474`, Delivery `744`/`744`, `git diff --check`.

## Task 9: Web checkpoint schedule — preview/set/clear

- [x] checkpoint 패널에 schedule preview → 저장 → 해제 폼을 둔다. 해제는 확인을 받는다.
- [x] ledger와 투영 문서를 맞추고 build/parity를 재생성한다.
- [x] 검증: 공통 검증 전체

실행 기록(2026-09-30): checkpoint schedule readback 아래에 "Preview schedule → Save schedule → Clear schedule" 폼을 두었다. preview 결과는 폼 아래에 표시하고, 저장과 해제는 확인을 받은 뒤 job으로 추적한다. `served-app.ts`에 확장 click hook(`VM_DETAIL_EXTENSION_CLICK_ACTIONS`에 있는 action만)을 더했다(`421`줄). 이전 결정을 고정하던 세 계약(static 계약 `no-save-form`, 그 negative 테스트, browser fixture의 negative 검사 세 줄)을 새 폼 기준(`schedule-preview`, `name="interval_minutes"`)으로 바꿨다. Web 제외는 `15`→`12`이다. 검증: `npm run test:required --prefix web` exit `0`, web Pester `50`/`0`, Cli `179`/`179`, Api `474`/`474`, Delivery `744`/`744`, `git diff --check`.

## Task 10: Web export/import — preview와 제출

- [x] VM detail에 export preview → export를, inventory에 import preview → import를 둔다. 경로 입력은 API 검증 결과를 그대로 보인다.
- [x] ledger와 투영 문서를 맞추고 build/parity를 재생성한다.
- [x] 검증: 공통 검증 전체

실행 기록(2026-09-30): plan과 달리 import 폼도 inventory가 아니라 VM detail의 기존 "Export / Import" 카드에 두었다. export → import 흐름이 한 카드에서 이어진다. import preview는 CLI `--has-vmcx`와 같은 운영자 확인 체크박스를 보낸다. 실행은 확인 후 job으로 추적하고, preview 결과는 카드 안에 표시한다. 폼 부재를 고정하던 계약(static 계약 두 줄, negative 테스트 하나, browser fixture negative 검사 다섯 줄)을 새 폼 기준으로 바꿨다. `FEATURE_IMPLEMENTATION_LEDGER.md` 요약표는 ledger에서 Routes/Web/CLI 열을 다시 계산한다. 그 과정에서 80-route ledger와 어긋나 있던 두 행(`pcv.network.inventory` `3` route, `pcv.vm.managed-import` `5` route)도 바로잡았다. Web 제외는 `12`→`8`이다. 검증: `npm run test:required --prefix web` exit `0`, web Pester `50`/`0`, Cli `179`/`179`, Api `474`/`474`, Delivery `744`/`744`, `git diff --check`.

## Task 11: Web network connect, device add

- [ ] VM detail에 switch 연결 폼(Network inventory의 switch 목록 사용)과 NIC/DVD 추가 폼을 둔다. NAT/DHCP 편집기는 만들지 않는다.
- [ ] ledger와 투영 문서를 맞추고 build/parity를 재생성한다.
- [ ] 검증: 공통 검증 전체

## Task 12: Web guest exec/channel preview

- [ ] guest exec와 channel ensure 앞에 preview를 둔다. 실행 전 preview 결과를 보인다.
- [ ] ledger와 투영 문서를 맞추고 build/parity를 재생성한다.
- [ ] 검증: 공통 검증 전체

## Task 13: 코어 계약 종결과 종료 검증

- [ ] Web 제외를 정책 근거 `4`개(noVNC target `3`, `vm.limit`)로, CLI 제외를 현재 `7`개(auth/session `6`, console capabilities `1`)로 고정하는 계약 테스트를 둔다.
- [ ] `SERVICE_PLAN.md` §5 완결도, `FEATURE_IMPLEMENTATION_LEDGER.md`, `USER_FEATURE_USAGE_SPEC.md`, `DOCUMENTATION_INDEX.md` campaign 줄을 현행화한다.
- [ ] 종료 검증: `dotnet test src/DesktopNode.sln`, `npm run test:required --prefix web`,
      packaging Pester(`packaging/windows-desktop-node/tests`), `git diff --check`
- [ ] campaign `next_task`를 `null`로 두고 `next_step`에 완료와 다음 승인 대상(push/PR, package pair, Lane 2 actual-VM)을 적는다.

## 계획 밖 (승인 필요)

- push와 PR, 다음 package pair와 fullgate, 새 기능의 Lane 2 actual-VM 검증, Lane 3 승격
- 같은 version 재빌드 installer 정책 구현(installer handoff)
- `vm.list` readback 확장(DVD media 경로, VHD `size_gb`): `vm.attach`/`vm.eject` reconcile과 `vm.disk-resize` reconcile 성공 판정의 선행 조건이다. HyperV WMI provider와 API 계약 변경이라 별도 설계와 Lane 2 확인이 필요하다.
- noVNC target Web 저장 폼, LAN 기본 on

## Nonclaims

- 이 campaign은 소스 개발 완료를 다룬다. operational current, 설치본, actual-VM 동작을 바꾸거나 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
