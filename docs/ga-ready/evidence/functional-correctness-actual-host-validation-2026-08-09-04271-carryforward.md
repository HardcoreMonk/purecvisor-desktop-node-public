# Functional correctness actual-host validation 2026-08-09 `0.42.71` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-08-09-04271-carryforward`
result: `PASS-CARRY-FORWARD`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.71-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-06-04270.md`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.71-admin-smoke` package chain은 product payload를 포함하지만, actual-host
functional correctness suite를 이 날짜에 재실행하지 않았다. 대신 아래를 근거로
`0.42.70` functional PASS를 carry-forward한다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.70` PASS (`functional-correctness-actual-host-validation-2026-08-06-04270`) |
| fullgate Hyper-V installed route smoke | `0.42.71` PASS (`full-admin-host-mutation-gate-20260808-04271`) |
| FC-12(b) 설치본 문자열 | 설치본 `DesktopNode.Host.exe`에 `GuestArgvInvocation` 포함 (fullgate evidence) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-08-09-04271`) |

FC-12(b) argv fidelity의 source/test 계약은
`GuestExecutionArgvFidelityTests`와 ADR-0009, package/fullgate evidence가 소유한다.
credentialed Windows guest `guest-exec` 왕복 재실행은 이 carry-forward가 주장하지 않는다.

## Nonclaims

- 이 문서는 새 actual-host functional suite 재실행 evidence가 아니다.
- public trusted signing / external stable publication을 주장하지 않는다.
