# Service Plan P1-5 managed full clone code-level PASS (2026-08-27)

evidence_id: `service-plan-p1-managed-full-clone-code-level-2026-08-27`
slice_id: `service-plan-p1-managed-full-clone-code-level-2026-08-27`
result: `PASS`
scope: `code-level-p1-managed-full-clone`
Design-ID: `purecvisor-desktop-node-p1-managed-full-clone-v1`
approval_locator: `User-Approval: pcv-p1-managed-full-clone-20260827`
spec: `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone-design.md`
plan: `docs/superpowers/plans/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone.md`
change_tier: `M`
verification_lane: `Full`
current_version_anchor: `0.42.75-admin-smoke`
canonical_current_changed: `false`
host_mutation_performed: `false`
package_build_performed: `false`
package_candidate_created: `false`
installed_product_changed: `false`
installed_smoke: `not-run`
actual_vm_validation: `not-run`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

managed Generation 2 VM의 독립 VHDX full clone 경로를 code-level 제품 표면에 열었다.
Local API `POST /api/v1/vms/{vmId}/clone/preview`는 즉시 `PreviewCloneVm` 계획만
반환하고 파일을 만들지 않는다. `POST /api/v1/vms/{vmId}/clone`는 queued `vm.clone`
job을 만든다. Web `Clone VM`과 `pcvcli vm clone <source> --name <target> --yes` /
`--dry-run`이 같은 route를 쓴다.

Feature ID는 `pcv.vm.clone`이다. 사람 투영 catalog는 28 feature / 62 route다. 이
기능의 evidence 단계는 모두 `not-assessed`이며 승격 후보가 아니다.
`config/desktop-node-feature-evidence-ledger.json`과
`docs/ga-ready/current-evidence.json`은 쓰지 않았다. `0.42.76` package는 열지 않았다.

## 계약

- Route: `POST /api/v1/vms/{vmId}/clone/preview` / `PreviewCloneVm` /
  `NativeProductOperation`. `POST /api/v1/vms/{vmId}/clone` / `QueueCloneVm` /
  `QueuedMutation`. family `hyperv-vm`. catalog `62`. QueuedMutation `27`.
- Feature ID: `pcv.vm.clone`. present surfaces `api` / `cli` / `web`.
- Body: `{ "confirm_name": "<decoded vmId>", "name": "<target>" }`.
  `confirm_name`은 소스 `{vmId}`와 Ordinal.
- Preview `200`는 `planned_copy_bytes`(소스 VHDX 파일 길이 합)를 포함한다. clone
  enqueue `202` params는 `{ source, name }`.
- Native: 소스는 managed, Generation 2, 전원 `Off`, checkpoint 0, 독립 VHDX만
  허용한다. 대상 Notes는 `managed-by=purecvisor-desktop-node`만 넣는다. 실패 시
  대상만 rollback하고 소스 VM은 변경하지 않는다. helper fallback 없음.
- CLI: `pcvcli vm clone <source> --name <target> --dry-run` → preview.
  `pcvcli vm clone <source> --name <target> --yes` → clone. `--yes`가 없으면
  `PCV_CLI_CONFIRMATION_REQUIRED`.
- Web: VM detail `Clone VM`. 대상 이름 입력, preview 후 confirmation에 소스 표시
  이름, 대상 이름, `planned_copy_bytes`를 보여 준다. 안내 문구는 “독립 VHDX를
  복사한 새 managed VM을 만든다. 소스 VM은 변경하지 않는다.” RBAC `operate`.
  coverage `vm.clone.preview` / `vm.clone`. present web `54`, excluded `8`.
- served route: `POST /api/v1/vms/{vm_id}/clone/preview`,
  `POST /api/v1/vms/{vm_id}/clone`.

### 문제 코드

| code | 단계 | 의미 |
| --- | --- | --- |
| `PCV_CLI_CONFIRMATION_REQUIRED` | CLI enqueue 전 | `--yes` 없음 |
| `PCV_VM_CLONE_CONFIRMATION_MISMATCH` | 400 enqueue 없음 | `confirm_name`이 디코드된 `{vmId}`와 Ordinal 불일치 |
| `PCV_VM_CLONE_NAME_REQUIRED` | 400 enqueue 없음 | 대상 `name` 없음 |
| `PCV_VM_CLONE_NAME_CONFLICT` | 400 enqueue 없음 | 대상 이름이 소스와 같음 |
| `PCV_VM_NOT_FOUND` | preview 실패 / job failed | 소스 VM이 없음 |
| `PCV_VM_NOT_MANAGED_BY_PURECVISOR` | preview 실패 / job failed | 먼저 `vm.manage` |
| `PCV_VM_GENERATION_UNSUPPORTED` | preview 실패 / job failed | Gen2만 허용 |
| `PCV_VM_CLONE_SOURCE_NOT_OFF` | preview 실패 / job failed | 소스를 Off로 만든 뒤 재시도 |
| `PCV_VM_CLONE_CHECKPOINTS_PRESENT` | preview 실패 / job failed | checkpoint를 삭제한 뒤 재시도 |
| `PCV_VM_CLONE_DISK_NOT_INDEPENDENT` | preview 실패 / job failed | 독립 VHDX만 허용 |
| `PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED` | preview 실패 / job failed | TPM/shielded는 이 경로 밖 |
| `PCV_VM_ALREADY_EXISTS` | preview 실패 / job failed | 다른 대상 이름 |
| `PCV_VM_NAME_INVALID` | job failed | 대상 표시 이름을 고친다 |

## 검증 결과

| 검증 | 결과 | 출처 |
| --- | --- | --- |
| `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~ApiHandlerAdapterContractTests` | PASS `15/15` | Task 1 |
| `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~VmClone` | PASS `26/26` | Task 5 |
| `dotnet test src/DesktopNode.Cli.Tests --filter FullyQualifiedName~Clone` | PASS `4/4` | Task 7 |
| `dotnet test src/DesktopNode.Cli.Tests --filter FullyQualifiedName~DesktopNodeCliProjectContractTests` | PASS `5/5` | Task 9. `<a id="pcv-vm-clone"></a>` 및 feature `28` / route `62` |
| `npm run test:required --prefix web` | PASS `233/233`. `web=54` `excluded=8` | Task 8 carry-forward, Task 9 재실행 |
| `git diff --check` | PASS (출력 없음) | Task 9 |

## 의도적으로 남긴 항목

- 설치본 Hyper-V clone mutation, package/fullgate/manual-admin campaign, Lane 2
  probe는 이 slice의 required 조건이 아니다.
- `pcv.vm.clone` evidence 단계는 `not-assessed`다. Lane 3 승격 입력이 아니다.
- `0.42.76-admin-smoke`와 `current-evidence.json` write는 열지 않았다.
- linked clone, export/import, OVF, TPM/shielded, Gen1, unmanaged 소스 clone은
  제품 경계 밖이다.

## Nonclaims

- 설치본 smoke와 actual-VM clone mutation은 `not-run`이다. 실제 Hyper-V/VM
  mutation을 실행하지 않았다.
- `package_candidate_created=false`. `0.42.76`과 `0.42.75 -> next` campaign을
  열지 않았다.
- `docs/ga-ready/current-evidence.json`과 generated current block을 바꾸지 않았다.
- `config/desktop-node-feature-evidence-ledger.json`을 쓰지 않았다.
- operational current는 `0.42.75-admin-smoke` 그대로다.
  `canonical_current_changed=false`.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- host mutation, MSI, service 재시작을 실행하지 않았다.
