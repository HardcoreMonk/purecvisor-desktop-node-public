# Lane 2 `vm.create` reconcile 장치 지문 `0.42.93-admin-smoke` (2026-10-08)

evidence_id: `lane2-vm-create-reconcile-devices-actual-vm-2026-10-08-04293`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.93-admin-smoke`
installed_product_version: `0.42.93-admin-smoke+818d00f113b3d3eaf1573652cd4c2d86a551834d`
installed_host_sha256: `9f537217caf49818ce254ae419ebbad57f8ce578ed78c27a77f704ebf9348b49`
installed_cli_sha256: `df02f453118c6f59102d3b563de86ec166f71dd9ef1190e4601aea6b2425ad76`
create_reconcile_fix: `65c376d`
design: `pcv-vm-create-reconcile-devices-v1`
probe_vm: `pcv-probe-reconcile-1008`
artifact_root: `artifacts/lane2-vm-create-reconcile-devices-20261008-04293`
host_mutation_performed: `true`
probe_vm_removed: `true`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

설치본 `0.42.93-admin-smoke`에서 설계 `pcv-vm-create-reconcile-devices-v1`의 설치본 probe만 확인했다. 설치 빌드 `818d00f`는 `65c376d`를 포함한다. CLI는 `--protected-token-file`로 인증했고 token 값은 기록하지 않았다.

probe VM은 Generation 2, CPU 2, memory 2048 MB, disk 8 GB로 큐에 넣었고 켜지 않았다. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`다. Hyper-V에 VM이 보인 직후 `DesktopNode.Host` 프로세스를 종료하고 서비스를 다시 올렸다.

## 결과

`2026-10-08T16:10:54+09:00`에 시작해 `2026-10-08T16:11:10+09:00`에 끝났다. 걸린 시간은 16초다. 첫 시도에서 창을 잡았다.

| 단계 | 관측 | 판정 |
| --- | --- | --- |
| 끊김 | create 호출 후 1561 ms에 VM이 보여 프로세스를 종료. Off, Generation 2, managed, 디스크 0, ISO 0, Default Switch 0 | `PASS` |
| job | `job-a25d2b4676714642a16e5440f193cce5` `failed`, `PCV_JOB_INTERRUPTED`. 서비스는 1초 만에 Running | `PASS` |
| `job reconcile` | exit `1`, `PCV_JOB_RECONCILIATION_REQUIRED`, `target-fingerprint-mismatch`, `missing_devices=disk,iso,switch`, `no mutation was attempted` | `PASS` |
| `postcondition-confirmed` | 응답에 없음 | `PASS` |
| hint | managed `vm.delete` 후 같은 이름으로 다시 create | `PASS` |
| 회수 | delete `job-6ea6235da7a64170aa458379fa111233` 뒤 create `job-13027d87dd1c4286b18c24ed8668c415` `succeeded`. 디스크 1, ISO 1, Default Switch 1, Off | `PASS` |
| 최종 삭제 | `job-cd8c6a104bd1486e8ff33b43b2269a6f` `succeeded`, VM과 폴더 없음 | `PASS` |

## 정리

- 보존 VM `pcv-guest-installed-04253-r1`은 Off 그대로다. VM id, Notes, 디스크 경로는 바뀌지 않았다.
- service는 Running/Automatic, Web은 `200`, PureCVisor firewall 규칙은 0, ARP는 `0.42.93` `{70A3822D-89D0-4E0D-A60B-892CE8CDC776}` 하나다.
- secret은 관측되지 않았다.

## Nonclaims

- 상태는 `installed_non_promoted_candidate`다. operational current는 `0.42.92-admin-smoke`다.
- pair, fullgate, current-card, current-evidence 쓰기는 하지 않았다.
- Hyper-V exactly-once와 reconcile 완전성을 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
