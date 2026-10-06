# Lane 2 family별 조건부 reconcile 확인 `0.42.91-admin-smoke` (2026-10-06)

evidence_id: `lane2-completion-family-reconcile-actual-vm-2026-10-06-04291`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
family: `job.reconcile` for `vm.create`, `vm.restart`, `vm.shutdown`, `vm.qos.storage.set`, `vm.qos.network.set` (SERVICE_PLAN P1-10)
design: `docs/superpowers/specs/2026-09-20-purecvisor-desktop-node-p1-remaining-reconcile-design.md`
version: `0.42.91-admin-smoke`
installed_product_version: `0.42.91-admin-smoke+990a4b2713f6d51dca416b476308a0bf92296155`
release_train: `0.42.91-admin-smoke`
artifact_root: `artifacts/lane2-completion-family-reconcile-20261006-04291-r4` (이전 시도 `-04291`, `-r2`, `-r3` 보존)
probe_summary_sha256: `07a9fdc9c4b5fb6064d3bdabf2f98f61af122dbbfcd570b78ef07601dea4863f`
vm_created: `true` (`pcv-probe-c4-rc`, 끝에 삭제)
host_mutation_performed: `true` (probe VM, 서비스 프로세스 종료와 재시작)
secret_observed: `false`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 방법

family마다 job을 큐에 넣고 `running`이 되면 `DesktopNode.Host` 프로세스를 강제 종료한 뒤 서비스를 다시 올렸다. 재시작 뒤 job은 `failed` +
`PCV_JOB_INTERRUPTED`가 된다. 그다음 `vm list`·`blkio-get`·`bandwidth`로 실제 상태를 따로 읽고 `pcvcli job reconcile <job>`을 실행했다.
판정이 실제 상태와 일치하면 PASS다. r4는 `2026-10-06T11:49:33Z`~`11:50:21Z`에 실행했다.

## 결과 (r4)

| family | 끊긴 시도 | 실제 상태 | reconcile |
| --- | ---: | --- | --- |
| `vm.create` | `1` | VM 없음 | `PCV_JOB_RECONCILIATION_REQUIRED`, `Classification: not-applied` |
| `vm.restart` | `1` | Running, `last_powered_on` 그대로 | `not-applied` |
| `vm.shutdown` | `1` | Running | `not-applied` |
| `vm.qos.storage.set` | `1` | readback에 정책 값 없음 | `not-applied` |
| `vm.qos.network.set` | `1` | readback에 정책 값 없음 | `not-applied` |

다섯 판정 모두 실제 상태와 일치한다. 이번 probe는 모두 적용 전에 끊긴 not-applied 경로다. postcondition-confirmed 경로는 0.42.84 전원 상태 reconcile
설치본 evidence(`lane2-development-completion-actual-vm-2026-09-30-04284`)와 family별 code-level 시험이 덮는다.

## 시도 기록과 발견

- r1(`11:08Z`): `vm.create`가 끊겼고 VM이 없어 reconcile이 `not-applied`였다. 스크립트가 빈 `vm list`를 처리하지 못해 멈췄다.
- r2, r3: 끊긴 `vm.create`가 VM 폴더에 `disk0.vhdx`(4 MB)를 남겼고, 다음 create는 `PCV_VHD_ALREADY_EXISTS`로 막혔다(정리 스크립트의 경로 결함 포함).
  r3의 끊긴 create도 reconcile `not-applied`였다.
- r4: 정리 경로를 고쳐 같은 고아 `disk0.vhdx`를 기록하고 지운 뒤 나머지 family를 측정했다.
- 발견(report-only): 끊긴 `vm.create`는 고아 디스크를 남길 수 있고 reconcile `not-applied`는 그것을 언급하지 않는다. 처리 방식은 campaign
  `train-04291-20261006` Task 13 설계가 다룬다. QoS readback은 `mutation_supported: false`를 보였다.

## 끝 상태

probe VM과 VM 폴더 `0`, 보존 VM Off, service Running/Automatic, Web `200`.

## Nonclaims

- probe는 승격 근거가 아니다. Hyper-V exactly-once와 reconcile 완전성을 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
