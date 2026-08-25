# Operational credential rebootstrap recovery R2 2026-08-09 `0.42.72`

evidence_id: `operational-credential-rebootstrap-recovery-r2-2026-08-09-04272`
result: `PASS`
evidence_scope: `read-only-recovery-attestation`
version: `0.42.72-admin-smoke`
source_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
artifact_root: `artifacts/operational-credential-rebootstrap-recovery-r2-20260809-04272`
artifact_summary: `artifacts/operational-credential-rebootstrap-recovery-r2-20260809-04272/summary.json`
summary_sha256: `529626336fcb79696f5cf765e7f1dacbf81a96beafc30000e00fa591ec7bfacb`
operational_msi_sha256: `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa`
operational_payload_sha256: `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933`
credential_rebootstrap_outcome: `pass`
verification_mode: `read-only-reconciliation`
additional_msi_mutation_performed: `false`
host_mutation_performed: `false`
token_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

선행 credential rebootstrap runner는 uninstall/install 모두 exit `0`이고 최종 credential
state도 ready였지만, transition JSON content hash가 이전과 같다는 이유로 실패했다. R2는
추가 MSI/service/registry/product-file mutation 없이 보존된 transition과 rollback evidence를
다시 읽어 이 실패를 deterministic verifier false-negative로 분류했다.

| 항목 | R2 readback |
| --- | --- |
| classification | `confirmed-verification-false-negative-transition-content-hash-non-change` |
| transition acceptance | identical content hash accepted under exact evidence rule |
| transition SHA-256 | `9a42e709d81e13765ecfd6be73a080bc3cca939dca7dac9cdea53e2c9204bae9` |
| rollback SHA-256 | `38eff5de84ce78f54c790c813fa396d18473e041df0aea3a0b9ed0d888bb50b9` |
| install window | transition/rollback last-write 모두 install window 안 |
| ordering | rollback precedes or equals transition `true` |
| ACL | transition/rollback exact ACL `true` |
| identity / target | `NT AUTHORITY\SYSTEM` / `PureCVisor/PureCVisorDesktopNode/api-token` |

## Live state

| surface | 값 |
| --- | --- |
| registration | operational singleton, DisplayVersion `0.42.72`, clean registration absent |
| installed payload | 8 files, aggregate `deb40a67c…` |
| Host / CLI | `c989fa5d…86b3` / `c7fac8d2…d60c` |
| service | `Running/Auto/LocalSystem`, credential source, argv exact |
| Web | index/config HTTP `200/200`, installed hashes exact |
| jobs | queue/active/temp/pending 모두 `0` |
| protected token metadata | SYSTEM owner, untrusted write access `false`, raw content read `false` |

R2 runner의 read-only surface에는 service control, MSI process, registry write, product-file write
명령이 모두 `0`개다. Evidence-local ACL materialization과 pinned copy/summary write만
수행했으며 host ACL은 바꾸지 않았다.

## Nonclaims

- 이 evidence는 선행 MSI transition을 새로 실행한 것이 아니라 보존 evidence를 read-only로
  재판정한 것이다.
- raw token을 읽거나 저장하지 않았다.
- token rotation PASS와 public trusted signing/external stable publication을 주장하지 않는다.
