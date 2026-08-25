# Installed token rotation smoke 2026-08-09 `0.42.72`

evidence_id: `installed-token-rotation-smoke-2026-08-09-04272`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.72-admin-smoke`
source_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
r1_artifact_root: `artifacts/installed-token-rotation-smoke-reconciliation-20260809-04272`
r1_summary_sha256: `ffb6a9d8804c853dc58839fd54a78662dc5c34ee228b606f0cc1d5fe5ac9bcf2`
r2_artifact_root: `artifacts/installed-token-rotation-smoke-reconciliation-r2-20260809-04272`
r2_summary_sha256: `cb136def04e431e5a0e5120eb33919815669e48c5f3b3a2b259dc92cac739878`
r3_artifact_root: `artifacts/installed-token-rotation-smoke-reconciliation-r3-20260810-04272`
r3_summary_sha256: `0ad425f5da6c7dc3f0b30b1f56e5962c911d566d0b8ec8edc19173f8f804a2bd`
final_attempt: `r4`
final_artifact_root: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272`
final_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
final_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
final_summary_bytes: `19840`
r4_runner_raw_sha256: `c6e138a008315bc2b75b76eb51a202cb75163cd37b961e4a9dfb5f14c2b98414`
r4_runner_contract_sha256: `259547e6eb82d66f172f7bf5f02d9171af1a6b84bcf2d9f8680780b7eb0b424f`
current_claim_eligible: `true`
host_mutation_performed: `false`
read_only_reconciliation_host_mutation_performed: `false`
historical_retry2_host_mutation_performed: `true`
token_value_recorded: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## R4 final 판정

R4 read-only reconciliation은 retry2 native child가 수행한 token rotation을 보존 evidence와
live readback으로 재검증해 `PASS`로 닫았다.

| 항목 | 값 |
| --- | --- |
| `ok` / stage | `true` / `read-only-reconciliation-complete` |
| classification | `native-rotation-succeeded-verifier-false-negative-reconciled` |
| native operation | `service-token-rotation-revoke`, child exit `0`, natural termination |
| mutation descriptor | backup `written`, atomic replace `completed`, service `restarted`, audit `written` |
| token hash relation | backup=descriptor old, current=descriptor new, old/new distinct — 모두 `true` |
| direct auth readback | old token HTTP `403`, new token HTTP `200` |
| audit | target record 정확히 `1`, descriptor field parity `true` |
| secret scan | findings `0`, read failures `0`, raw values recorded `false` |
| R4 mutation boundary | host/service/MSI/registry/Hyper-V mutation 모두 `false` |

Retry2의 원래 실패 단언은 backup `CreationTime`이 fresh해야 한다는 잘못된 가정이었다.
`File.Replace`는 replaced source의 creation time을 보존한다. R4는 exact single-file delta,
descriptor path, filename/directory/current-token write time을 freshness evidence로 사용해 이
false-negative를 닫았다.

보존된 installed mutation attempt:

| artifact | result | mutation | 경계 |
| --- | --- | --- | --- |
| `installed-token-rotation-smoke-20260809-04272` | `FAIL` | `false` | pre-mutation runner type resolution failure |
| `installed-token-rotation-smoke-retry-20260809-04272` | `FAIL` | `false` | pre-mutation verifier property failure; original state unchanged |
| `installed-token-rotation-smoke-retry2-20260809-04272` | `FAIL` | `true` | rotation attempted; backup creation freshness verifier failure; restoration completed |

Host mutation은 이 표의 retry2만 소유한다. 초기 attempt와 retry는 pre-mutation에서
실패했고, 아래 reconciliation R1/R2/R3는 모두 read-only다.

## Read-only reconciliation R1/R2/R3

| run | artifact summary | SHA-256 | last/stage | error code | mutation | token recorded |
| --- | --- | --- | --- | --- | --- | --- |
| R1 | `installed-token-rotation-smoke-reconciliation-20260809-04272/summary.json` | `ffb6a9d8804c853dc58839fd54a78662dc5c34ee228b606f0cc1d5fe5ac9bcf2` | `read-only-reconciliation-failed` | `PCV_RECON_PRESERVED_ROOT_IDENTITY` | all `false` | `false` |
| R2 | `installed-token-rotation-smoke-reconciliation-r2-20260809-04272/summary.json` | `cb136def04e431e5a0e5120eb33919815669e48c5f3b3a2b259dc92cac739878` | `read-only-reconciliation-failed` | `PCV_RECON_UNCLASSIFIED_FAILURE` | all `false` | `false` |
| R3 | `installed-token-rotation-smoke-reconciliation-r3-20260810-04272/summary.json` | `0ad425f5da6c7dc3f0b30b1f56e5962c911d566d0b8ec8edc19173f8f804a2bd` | after `after-module-import`; `read-only-reconciliation-failed` | `PCV_RECON_ARGUMENT_EXCEPTION` | all `false` | `false` |

`all false`는 각 summary의 host mutation, service action, MSI invocation, registry write,
Hyper-V mutation을 뜻한다. 세 summary 모두 `classification=read-only-verification-incomplete`,
`token_value_recorded=false`이며 public signing/publication을 주장하지 않는다.

## Operational readback

| surface | R4 readback |
| --- | --- |
| package | operational MSI `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa` |
| payload | aggregate `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933` |
| service | `Running` / `Auto` / `Automatic` / `LocalSystem` |
| token source | Windows Credential Manager, legacy/raw token flags absent |
| jobs | total `17`, queue `0`, active `0`, pending commit absent |
| publication | public trusted signing / external stable publication `not-claimed` |

R4 runner는 두 token value를 memory에서만 비교했고 raw values를 summary나 scan surface에
기록하지 않았다. Secret scan 35개 target에서 finding/read failure는 `0/0`이다.

## Nonclaims

- R4 reconciliation 자체는 read-only다. 실제 host mutation은 보존된 retry2가 소유한다.
- raw token value를 기록하거나 공개하지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
