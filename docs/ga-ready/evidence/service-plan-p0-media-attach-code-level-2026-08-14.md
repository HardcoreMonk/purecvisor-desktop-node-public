# Service Plan P0-1 미디어 재장착 code-level PASS (2026-08-14)

evidence_id: `service-plan-p0-media-attach-code-level-2026-08-14`
slice_id: `service-plan-p0-media-attach-code-level-2026-08-14`
result: `CODE_LEVEL_PASS`
Design-ID: `purecvisor-desktop-node-p0-media-attach-v1`
approval_locator: `User-Approval: service-plan-p0-media-attach-20260814`
spec: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-media-attach-design.md`
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

`vm.eject`의 빈 짝인 가상 DVD ISO 재장착을 code-level 제품 경로로 닫았다. Local API
`POST /api/v1/vms/{vmId}/attach`는 `{ "iso_path": "<absolute-path>" }`를 받아 queued
`vm.attach` job을 만든다. Web `Attach media`와 `pcvcli vm attach`가 같은 route를 쓴다.
기존 DVD 한 개의 `HostResource`만 교체하고, DVD가 없으면 드라이브를 만들지 않는다.

이 기록은 Tasks 1-6 구현 검증과 Task 7 운영자 문서를 묶은 code-level 범위다. 설치본
smoke, 다음 package campaign, operational current 승격은 주장하지 않는다.

## 계약

- Route: `POST /api/v1/vms/{vmId}/attach` / `QueueAttachVmMedia` / family `hyperv-vm` /
  `QueuedMutation`. catalog count `57`. QueuedMutation count `23`.
- Job operation: `vm.attach`. enqueue `202` params `{ name, iso_path }`.
- Native: `DesktopNodeHyperVVmMediaRequest` + `DesktopNodeHyperVWmiVmMediaProvider`.
  attach는 첫 DVD의 `HostResource = new[] { isoPath }`다. helper fallback 없음.
- CLI: `pcvcli vm attach <vm> --iso <path>`와 `--iso_path` alias. `--yes` 없음.
- Web: VM detail ISO 입력 + `Attach media`. confirmation은 VM 표시 이름과 `iso_path`를
  보여 준다. coverage `vm.media.attach`. 기존 `vm.media` eject row는 유지.
- served route: `POST /api/v1/vms/{vm_id}/attach`.

### 문제 코드

| code | HTTP | 의미 |
| --- | ---: | --- |
| `PCV_VM_ATTACH_ISO_REQUIRED` | 400 enqueue | `iso_path`가 없음 |
| `PCV_VM_NAME_INVALID` | job failed | 표시 이름이 유효하지 않음 |
| `PCV_ISO_NOT_FOUND` | job failed | 호스트 ISO 절대 경로가 없음. create와 같은 코드 |
| `PCV_VM_DVD_DRIVE_NOT_FOUND` | job failed | DVD가 없음. 드라이브를 만들지 않음 |
| `PCV_VM_NOT_FOUND` | job failed | 대상 VM이 없음 |
| `PCV_VM_MEDIA_FAILED` | job failed, retryable | native media mutation 실패 |

## 검증 결과

아래는 Tasks 1-6에서 실제 실행해 관측한 결과다. Task 7은 운영자 문서와 이 evidence만
추가하며 전체 suite는 Task 8이 소유한다.

| 검증 | 결과 | 출처 |
| --- | --- | --- |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests` | PASS `13/13` | Task 1 |
| `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj` | PASS `139/139` | Task 2 |
| `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --filter FullyQualifiedName~HyperVDomainContractTests` | PASS `36/36` | Task 3 |
| `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter FullyQualifiedName~RuntimePolicyContractTests` | PASS `13/13` | Task 3 |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests` | PASS `136/136` | Task 4 |
| `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj` | PASS `134/134` | Task 5 |
| `npm test --prefix web` | PASS (`tsc --noEmit`, `check:served`, `check:frontend-batches`) | Task 6 |
| `npm run verify:parity --prefix web` | PASS (`check:served`, static parity, `browser:fixture`) | Task 6 |
| `Invoke-Pester -Path web/tests -Output Detailed` | PASS `49/49` | Task 6 |
| `git diff --check` | PASS (출력 없음) | Task 7 |

## 의도적으로 남긴 항목

- 설치본 Hyper-V attach smoke와 package/fullgate/manual-admin campaign은 이 slice의
  required 조건이 아니다.
- SDD ledger의 deferred minor(adapter-level missing-ISO/DVD unit, invoker allowlist
  전용 assert, whitespace-only `iso_path` fact, cancelled confirm `form.reset()`)는
  이 문서에서 고치지 않는다.

## Nonclaims

- 설치본 smoke는 `not-run`이다. 실제 Hyper-V/VM mutation을 실행하지 않았다.
- `package_candidate_created=false`. `0.42.74`와 `0.42.73 -> next` campaign을 열지 않았다.
- `docs/ga-ready/current-evidence.json`과 generated current block을 바꾸지 않았다.
- operational current는 `0.42.73-admin-smoke` 그대로다. `operational_current_changed=false`.
- USB passthrough, DVD 드라이브 추가, attach preview/dry-run, attach reconcile을
  열지 않았다.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- host mutation, MSI, service 재시작을 실행하지 않았다.
