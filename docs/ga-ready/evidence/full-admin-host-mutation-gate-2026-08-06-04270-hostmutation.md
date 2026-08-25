# Full admin host mutation gate `0.42.70-admin-smoke` (2026-08-06)

evidence_id: `full-admin-host-mutation-gate-2026-08-06-04270-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.70-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260806-04270`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260806-04270`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260806-04270`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260806-04270`
operational_fullgate_msi_sha256: `90aeda60633ec7e6d32d88f71cbea2b2d5bb54eff205cf49d51cd894b44d8165`
operational_fullgate_payload_aggregate_sha256: `625a08ce4fcc4435c2ffa9af6804dbffc9c4b87450ea4b0613b1df52cb217f99`
provenance_commit: `e91389880febdfb3c1ba430f97c84c2f7e006591`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `91s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11s` |

두 단계 모두 재시도 없이 통과했고 batch `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, `timed_out=false`다.

`service-msi-hyperv-admin-smoke` 내부 단계는 모두 `completed`로 끝났다.

| 내부 step | result |
| --- | --- |
| `build-current-admin-smoke-msi` | `completed` |
| `service-action-smoke` | `completed` |
| `msi-lifecycle-smoke` | `completed` |
| `installed-dotnet-host-hyperv-api-route-smoke` | `completed` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.70-admin-smoke` |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| `boot_time_unchanged` | `true` (양 단계, `2026-08-06 11:42:07` 유지) |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| `final_firewall_rule_count` | `0` |
| `final_eventlog_source_present` | `false` |

기존 VM `pcv-guest-installed-04253-r1`은 조작하지 않았다.

## 설치본 Hyper-V route 실행 확인

`installed-dotnet-host-hyperv-api-route-smoke`는 설치된 `0.42.70` 서비스로 실제 Hyper-V VM을
생성·조작·삭제했다. 코드 레벨 관측이 아니라 설치된 제품의 실행 결과다.

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-c8b5decc` |
| `vm_list_contains_vm` | `true` |
| `checkpoint_list_contains_checkpoint` | `true` |
| `checkpoint_restore_precondition` | `vm.poweroff-before-restore` |
| `vm_list_absent_after_delete` | `true` |
| unmanaged VM | `pcv-spike-api-foreign-57e0dec4` |
| `unmanaged_vm_created` | `true` |
| `unmanaged_vm_still_exists_after_block` | `true` |

unmanaged VM이 삭제되지 않고 남는 것은 `PCV_VM_NOT_MANAGED_BY_PURECVISOR` guard가 동작한다는
증거이며, smoke 종료 시 정리돼 최종 잔여 `pcv-spike-*` VM은 `0`이다.

## 이전 anchor gate와의 소요 시간 차이

`0.42.69` gate의 `service-msi-hyperv-admin-smoke`는 `433s`, 이번은 `91s`다. 두 실행 모두
같은 4개 내부 step을 `exit 0`으로 끝냈고 VM 생성·checkpoint·삭제 관측도 동일하다. 차이는
build 단계의 증분 컴파일과 ISO 처리 시간 차이로 보이며, 이 문서는 원인을 확정하지 않는다.

## Provenance

operational MSI는 이 gate가 승격 시점 HEAD `e9138988`에서 새로 빌드했다. Clean package
(`b28e1876...`, commit `821a6a34`)와는 별개 빌드이므로 MSI/payload hash가 다르다. 두 빌드의
payload source 경로 diff는 `0`건이며 근거는
`docs/ga-ready/evidence/admin-smoke-package-2026-08-06-04270.md`가 소유한다.

## Nonclaims

- 이 evidence는 internal `AllowUnsignedDev`/`LocalTest` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair closure는 이 gate가 닫지 않는다. closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md`가 소유한다.
