# Service Plan P0-2 checkpoint restore reconciliation code-level PASS (2026-08-14)

evidence_id: `service-plan-p0-checkpoint-restore-reconciliation-code-level-2026-08-14`
slice_id: `service-plan-p0-checkpoint-restore-reconciliation-code-level-2026-08-14`
result: `CODE_LEVEL_PASS`
Design-ID: `purecvisor-desktop-node-p0-checkpoint-restore-reconciliation-v1`
approval_locator: `User-Approval: service-plan-p0-restore-reconcile-20260814`
spec: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-checkpoint-restore-reconciliation-design.md`
plan: `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md`
change_tier: `M`
verification_lane: `Full`
operational_current_changed: `false`
host_mutation_performed: `false`
package_build_performed: `false`
package_candidate_created: `false`
installed_product_changed: `false`
installed_smoke: `not-run`
public_trusted_signing: `false`
external_stable_publication: `false`

## 판정

끊긴 `checkpoint.restore` job을 기존 `POST /api/v1/jobs/{jobId}/reconcile`로만 판정하는
P0-2 계약을 code-level로 닫았다. `checkpoint.list` additive `is_current`와 enqueue
baseline `pcv-checkpoint-restore-reconciliation/v1`이 선행하고, reconcile은
`checkpoint.list`만 읽는다. `checkpoint.restore`를 다시 호출하지 않는다.

`succeeded`는 요청 이름+VM row가 정확히 1개이고 그 row의 `is_current=true`일 때만이다.
이름 presence만으로는 성공이 아니다. Web interrupted restore row는 `Reconcile restore`를
보여 주고, create 버튼 `Reconcile checkpoint`는 유지한다. CLI는 기존
`pcvcli job reconcile <job_id>` route를 유지하며 interactive help만
`Reconcile an interrupted rename, delete, checkpoint create, or restore`로 넓힌다.

이 기록은 Tasks 11-12 구현 검증과 Task 13 운영자 affordance/문서를 묶은 code-level
범위다. 설치본 restore smoke, 다음 package campaign, operational current 승격은
주장하지 않는다.

## 계약

- Restore enqueue: 기존 `POST /api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore` /
  `QueueRestoreVmCheckpoint` / family `hyperv-vm` / `QueuedMutation`.
- Reconcile: 기존 `POST /api/v1/jobs/{jobId}/reconcile` / `ReconcileJob` /
  permission `operate` / stance `ProductOperation`.
- Catalog count: `57` 유지. 새 HTTP route 없음.
- Job operation: `checkpoint.restore`.
- Schema: `pcv-checkpoint-restore-reconciliation/v1`.
- List field: additive `is_current` (`true` / `false` / `null`). public JSON에
  `instance_id` 없음.
- `succeeded` 조건: 요청 이름 row 정확히 1개 **그리고** `is_current=true`.
- Web: `canReconcileVmMutation` allowlist에 `checkpoint.restore`. 버튼
  `Reconcile restore`. RBAC `operate`.
- CLI: `pcvcli job reconcile <job_id>`. 새 subcommand 없음.

### 문제 코드 / 분류

| 관측 | 결과 |
| --- | --- |
| 요청 이름 1개이고 `is_current=true` | 200, job `succeeded`, `action=reconciled`, `postcondition-confirmed` |
| 요청 이름 1개인데 `is_current=false` | 409 `PCV_JOB_RECONCILIATION_REQUIRED`, `not-applied`, job `failed` 유지 |
| 요청 이름 1개인데 `is_current=null` | 409, `current-unavailable` |
| 요청 이름 0개 | 409, `not-applied` |
| 요청 이름 2개 이상 | 409, `ambiguous-duplicate-checkpoint-names` |
| list 실패 | 409, `readback-unavailable` |
| captured baseline 없음 | 409, `baseline-unavailable` 또는 `job-not-reconcilable` |

enqueue `capture_status=captured`는 요청 이름 1개, current 1개, current ≠ 요청일 때만이다.
이미 current인 restore는 `PCV_CHECKPOINT_ALREADY_CURRENT`로 `unavailable`이다. HTTP `202`는
유지한다.

## 검증 결과

아래 Task 13 행은 이 작업에서 실행해 관측한 결과다. Task 11/12는 해당 작업 보고와 git
log를 인용하며 전체 .NET solution을 다시 돌리지 않았다.

| 검증 | 결과 | 출처 |
| --- | --- | --- |
| `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --nologo` | PASS `142/142` | Task 11 `bf50561b` / `task-11-report.md` |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~Checkpoint --nologo` | PASS `10/10` | Task 11 `bf50561b` / `task-11-report.md` |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --nologo` | PASS `276/276` | Task 12 `3b503a2c` / `task-12-report.md` |
| `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --filter FullyQualifiedName~JobRuntimeReconciliationTests --nologo` | PASS `8/8` | Task 12 `3b503a2c` / `task-12-report.md` |
| `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1` | PASS `5/5` | Task 12 `3b503a2c` / `task-12-report.md` |
| `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1` | PASS `4/4` | Task 12 `3b503a2c` / `task-12-report.md` |
| `npm test --prefix web` | PASS (`tsc --noEmit`, `check:served`, `check:frontend-batches`) | Task 13 |
| `npm run verify:parity --prefix web` | PASS (`check:served`, static parity, `browser:fixture`) | Task 13 |
| `Invoke-Pester -Path web/tests -Output Detailed` | PASS `49/49` | Task 13 |
| `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --nologo` | PASS `134/134` | Task 13 |
| `git diff --check` | PASS (출력 없음) | Task 13 |

Task 13 TDD:

- RED: Pester `Reconcile restore` 정적 단언 실패 (`48` PASS / `1` FAIL). CLI
  `HelpListsAvailableCommandsAsSingleCommandRows`가 새 help 문자열을 찾지 못해 FAIL.
- GREEN: `render-jobs.ts` allowlist/라벨과 CLI help 적용, `web/`에서
  `node scripts/build-served-asset.mjs --write`로 `app.js` 재생성 후 위 Task 13 행 PASS.

Wave 2C fixture
`packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json`는
수정하지 않았다. `excluded_operations`의 `checkpoint.restore`는 그대로다.

## 의도적으로 남긴 항목

- 설치본 Hyper-V restore smoke와 package/fullgate/manual-admin campaign은 이 slice의
  required 조건이 아니다.
- Slice C Hyper-V Saved (`vm.save` / `vm.resume-saved`)는 시작하지 않는다.
- `0.42.74`와 `docs/ga-ready/current-evidence.json`을 열거나 바꾸지 않는다.

## Nonclaims

- 설치본 smoke는 `not-run`이다. 실제 Hyper-V restore Apply를 실행하지 않았다.
- `package_candidate_created=false`. `0.42.74`와 `0.42.73 -> next` campaign을 열지 않았다.
- `docs/ga-ready/current-evidence.json`과 generated current block을 바꾸지 않았다.
- operational current는 `0.42.73-admin-smoke` 그대로다. `operational_current_changed=false`.
- catalog는 `57`이다. 새 reconcile HTTP route를 만들지 않았다.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- host mutation, MSI, service 재시작을 실행하지 않았다.
- Hyper-V exactly-once와 mixed-version writer를 주장하지 않는다 (ADR-0013).
