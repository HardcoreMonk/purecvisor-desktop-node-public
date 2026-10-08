# Lane 2 `vm.qos` readback `0.42.93-admin-smoke` (2026-10-08)

evidence_id: `lane2-vm-qos-actual-vm-2026-10-08-04293`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.93-admin-smoke`
installed_product_version: `0.42.93-admin-smoke+818d00f113b3d3eaf1573652cd4c2d86a551834d`
installed_host_sha256: `9f537217caf49818ce254ae419ebbad57f8ce578ed78c27a77f704ebf9348b49`
installed_cli_sha256: `df02f453118c6f59102d3b563de86ec166f71dd9ef1190e4601aea6b2425ad76`
qos_readback_fix: `92146da`
probe_vm: `pcv-probe-qos-1008`
artifact_root: `artifacts/lane2-vm-qos-actual-vm-20261008-04293`
host_mutation_performed: `true`
probe_vm_removed: `true`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

설치본 `0.42.93-admin-smoke`에서 `vm.blkio-get`과 `vm.bandwidth` readback의 `mutation_supported`가 dispatch catalog의 mutation과 같은지만 확인했다. 설치 빌드 `818d00f`는 `92146da`를 포함한다. CLI는 `--protected-token-file`로 인증했고 token 값은 기록하지 않았다.

probe VM은 Generation 2, CPU 2, memory 2048 MB, disk 20 GB로 만들었고 켜지 않았다. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`다. `vm.blkio-set`과 `vm.bandwidth-set`은 호출하지 않았다.

## 결과

`2026-10-08T15:57:34+09:00`에 시작해 `2026-10-08T15:57:44+09:00`에 끝났다. 걸린 시간은 10초다.

| 단계 | 관측 | 판정 |
| --- | --- | --- |
| `vm create` | job `job-1546619f0cc141f6aa25ad81237c5e43` `succeeded`, 3초, Generation 2, Off, 디스크 1 | `PASS` |
| `vm blkio-get` `storage_qos.mutation_supported` | `true` (`hyperv-storage-inventory-readback-v1`, `linux_blkio_compatible=false`, state `stopped`) | `PASS` |
| catalog `vm.qos.storage.set` | `DesktopNodeHyperVOperationKind.Mutation` | `PASS` |
| `vm bandwidth` `network_qos.mutation_supported` | `true` (`hyperv-network-inventory-readback-v1`, `linux_bandwidth_compatible=false`, adapter 1) | `PASS` |
| catalog `vm.qos.network.set` | `DesktopNodeHyperVOperationKind.Mutation` | `PASS` |

`IsCatalogMutation`은 catalog entry의 Kind가 `Mutation`일 때 `true`다. 두 readback 모두 `true`라 catalog mutation과 같다.

## 정리

- `vm delete --yes` job `job-1e64c1109a0f45c6857e8ac0ed158b37`이 `succeeded`다. 3초.
- 제품이 `D:\PureCVisor\VMs\pcv-probe-qos-1008`을 지웠다. 이후 그 디렉터리는 없었고, 다른 VM이 그 경로를 참조하지 않았다.
- 보존 VM `pcv-guest-installed-04253-r1`은 Off 그대로다. VM id, Notes, 디스크 경로는 바뀌지 않았다.
- service는 Running/Automatic, PureCVisor firewall 규칙은 0, ARP는 `0.42.93` `{70A3822D-89D0-4E0D-A60B-892CE8CDC776}` 하나다.
- secret은 관측되지 않았다.

## Nonclaims

- 상태는 `installed_non_promoted_candidate`다. operational current는 `0.42.92-admin-smoke`다.
- pair, fullgate, current-card, current-evidence 쓰기는 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
