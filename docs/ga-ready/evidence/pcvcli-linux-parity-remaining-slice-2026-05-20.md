# PCVCLI Linux parity remaining slice 2026-05-20

evidence_id: `pcvcli-linux-parity-remaining-slice-2026-05-20`
result: `PASS_SCOPE_LOCK_CLOSED_CODE_LEVEL_PROMOTED`
scope: `pcvcli-linux-pcvctl-compatible-remaining-command-slice-selection`
base_evidence: `docs/ga-ready/evidence/pcvcli-backend-command-gap-slice-2026-05-19.md`
current_product_anchor: `0.42.38-admin-smoke`
closed_by_adr: `docs/adr/0007-pcvcli-hyperv-qos-guest-service-parity.md`
closed_by_evidence: `docs/ga-ready/evidence/pcvcli-hyperv-qos-guest-service-slice-2026-05-20.md`
host_mutation_performed: `false`
package_build_performed: `false`
public_release: `not-claimed`

`0.42.38-admin-smoke` 기준으로 Windows Desktop Node Hyper-V Local API가 제품 claim으로
제공하는 PCVCLI command surface는 `host/runtime/ops/network`, VM inventory/create/
lifecycle/media/resource/delete/checkpoint, console/noVNC, job, diagnostics까지 닫혔다.
따라서 다음 Linux parity 작업은 단순 command 추가가 아니라 Linux/libvirt/cgroup
semantics를 Hyper-V 제품 정책으로 재해석할지 결정하는 scope-lock slice다.

## 선정 결과

다음 개발 slice는 `pcvcli-linux-parity-remaining-scope-lock-and-operator-ux`로 선정했고,
ADR-0007에서 Hyper-V 제품 의미로 닫을 수 있는 부분만 code-level 승격했다.

| 잔여 Linux `pcvctl` 영역 | 현재 분류 | 다음 결정 |
| --- | --- | --- |
| `vm limit` | `hyperv-resource-mutation-promoted` | Hyper-V CPU/MEM resource mutation으로 code-level 승격했다. Linux cgroup limit 호환은 주장하지 않는다. |
| `vm blkio-get` | `hyperv-storage-readback-promoted` | Hyper-V disk/storage inventory readback으로 승격했다. `blkio-set`은 미지원이다. |
| `vm bandwidth` | `hyperv-network-readback-promoted` | Hyper-V network adapter inventory readback으로 승격했다. bandwidth shaping mutation은 미지원이다. |
| `vm guest-agent-status/guest-ping` | `hyperv-guest-service-readback-promoted` | Hyper-V Integration Services readiness/readback으로 승격했다. qemu guest agent 또는 credentialless heartbeat claim은 하지 않는다. |
| `vm guest-agent-ensure-channel/guest-exec` | `security-boundary-deferred` | guest channel 생성/guest exec는 credential/audit/secret redaction ADR이 필요해 미지원으로 유지한다. |
| `nic/iso/storage/device/container/ovn/dpdk/sriov/template/backup/alert/agent/batch/prometheus/webhook/security/security-group/gpu/config/grpc/cloud` | `linux-single-runtime-object-out-of-product-scope` | Desktop Node Windows Hyper-V product boundary 밖으로 유지한다. |

## 구현 진입 조건 결과

ADR-0007에서 아래처럼 닫았다.

1. Linux command name은 유지하되 Hyper-V semantics를 help/docs/payload flag에 명시한다.
2. `vm limit`만 VM resource mutation으로 분류하고, `vm blkio-get`, `vm bandwidth`,
   `vm guest-agent-status`, `vm guest-ping`은 readback/readiness로 분류한다.
3. readback payload는 Linux compatibility flag를 `false`로 기록한다.
4. 제품 payload 변경이므로 `0.42.39-admin-smoke` package/fullgate/manual-admin package-pair
   campaign을 후속 gate로 연다.
5. `guest-agent-ensure-channel`, `guest-exec`, `blkio-set`은 별도 ADR 전까지 미지원으로 유지한다.

## 후속 후보

후속 후보는 `guest-exec-security-boundary`와 `hyperv-qos-mutation-policy`로 분리한다.
`vm blkio-set`, switch port bandwidth mutation, guest command execution은 credential,
rollback/readback, audit log, secret redaction contract가 닫힌 뒤에만 다시 검토한다.

이 문서는 slice selection evidence이며 host mutation, package build, public trusted
signing, external stable publication을 주장하지 않는다.
