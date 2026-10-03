# Lane 2 actual-VM probe: 개발 완료 기능 `0.42.84-admin-smoke` (2026-09-30)

evidence_id: `lane2-development-completion-actual-vm-2026-09-30-04284`
result: `PASS`
status: `installed-non-promoted-candidate`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/lane2-development-completion-actual-vm-20260930-04284`
summary_sha256: `a3f9887a53eeb51204545fbdeb523fbac0c731f687ceb263467da335fe89c77b`
installed_product_version: `0.42.84-admin-smoke+ee90e0ea2c042e21cd0bf0346440ae2db41dfe43`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ...`
probe_vms: `pcv-lane2-devcomp-0930`(rename 뒤 `pcv-lane2-devcomp-0930-renamed`), `pcv-lane2-devcomp-0930-import`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

개발 완료 campaign(PR #22)이 Web Console에 연결한 route와 새 reconcile 경로를 fullgate 설치본 `0.42.84`에서 실제 VM으로 실행했다. Web Console과 PCVCLI는 같은 API route를 쓴다. 그래서 route는 설치본 `pcvcli`로 실행했고, Web binding이 설치본 자산에 들어 있는지는 `GET /app.js`로 따로 확인했다. 실행 스크립트는 artifact root의 `probe.ps1`이고, 2026-09-30 `06:27:04Z`부터 `06:28:00Z`까지 돌았다.

## 사전 상태

VM은 보존 VM `pcv-guest-installed-04253-r1`(Off) 하나였다. ISO는 `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`다.

## Web 자산

설치본 `http://127.0.0.1/app.js`에 새 binding 표식 10개가 모두 있다: `vm-pause`, `vm-rename`, `getVmMemoryStats`, `checkpoint-schedule-preview`, `vm-export-preview`, `vm-import-preview`, `vm-network-connect`, `vm-device-add`, `vm-guest-exec-preview`, `guest-agent-channel-preview`.

## route 실행

| 기능 | route | 결과 |
| --- | --- | --- |
| VM 생성(준비) | `vm.create` | job `succeeded` |
| telemetry | `vm.memory-stats`, `vm.cpu-stats` | exit `0`, `ok` |
| rename | `vm.rename` | job `succeeded`, readback에 새 이름 |
| checkpoint schedule | preview / set / clear | preview `ok`, set·clear job `succeeded` |
| switch 연결 | `vm.network.connect` (Default Switch) | job `succeeded` |
| 장치 추가 NIC | `vm.device.add` `nic` | job `succeeded`, readback network 2개 |
| 장치 추가 DVD | `vm.device.add` `dvd` | 거절 `PCV_VM_DEVICE_ALREADY_PRESENT`(생성 때 DVD drive가 이미 있다. 정책대로 거절) |
| export | preview / 실행 | preview `ok`, job `succeeded`, 디렉터리 생성 |
| import | preview(`--has-vmcx`) / 실행 | preview `ok`, job `succeeded`, 새 VM 생성 |
| pause / resume | `vm.pause`, `vm.resume` | job `succeeded`, readback `paused` → `running` |
| guest preview | `vm.guest.exec.preview`, `vm.guest.channel.preview` | exit `0`, `ok` (job 없음) |
| 전원 | `vm.start`, `vm.poweroff` | job `succeeded` |

## reconcile

| 경로 | 방법 | 결과 |
| --- | --- | --- |
| 비대상 | 끝난 `vm.export` job을 reconcile | 거절, `vm.export is not a reconcile target: Export writes host files that vm.list does not report.` |
| 대상(interrupt) | `vm.start` job이 `running`일 때 `DesktopNode.Host` 프로세스를 강제 종료하고 서비스를 다시 올림 | 재시작 뒤 job `failed` + `PCV_JOB_INTERRUPTED`. `job reconcile` exit `0`, job `succeeded`(readback VM `running`). 서비스는 Web `200`으로 회복 |

interrupt는 `06:27:45Z`에 만들었다. 이것으로 새 전원 상태 reconcile(`pcv-vm-power-state-reconciliation/v1`)이 실제 호스트 readback으로 `postcondition-confirmed`를 낸다는 것을 확인했다.

## 정리

- probe VM 두 개를 PCVCLI `vm delete --yes`로 지웠다(job `succeeded`). export 디렉터리도 지웠다.
- managed delete(WMI `DestroySystem`)는 VM 디렉터리의 `disk0.vhdx`를 남긴다. 그래서 어떤 VM도 참조하지 않는 것을 확인한 뒤, 이번 probe가 만든 두 디렉터리를 직접 지웠다.
- 최종 상태: VM은 보존 VM 하나(Off), service Running, Web `200`, secret 관측 없음이다.

## report-only 발견

- 비대상 reconcile 안내 문구가 "confirm whether the rename applied"로 나온다. 비대상 operation의 mutation 이름이 `rename`으로 기본 처리되기 때문이다(Api `ReconciliationRequiredError`, Runtime 같은 기본값).
- managed delete가 VHD와 VM 디렉터리를 남긴다. 이전 run의 잔여 `D:/PureCVisor/VMs/pcv-p1-clone-04276-34a8e66d-dst`도 남아 있으며, 이번에는 건드리지 않았다.

## Nonclaims

- 상태는 `installed-non-promoted-candidate`다. operational current는 `0.42.83-admin-smoke`다.
- Hyper-V exactly-once나 reconcile 완전성을 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
