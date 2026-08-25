# Functional correctness actual-host validation `0.42.69-admin-smoke` (2026-08-05)

evidence_id: `functional-correctness-actual-host-validation-2026-08-05-04269`
result: `PARTIAL_PASS_WITH_CARRY_FORWARD`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.69-admin-smoke`
fullgate_batch: `full-admin-host-mutation-gate-20260805-04269`
provenance_commit: `7236b813d6a4f594abb8e126b2b5dfb2ad56c1e9`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 `0.42.69` anchor의 functional evidence 소유자다. **이 버전에서 실제로 재실행한 것과
이전 버전에서 이월한 것을 분리해서 기록한다.** 이월 항목을 재실행한 것처럼 읽어서는 안 된다.

## 이 버전에서 실제로 실행한 것

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| 설치본 Hyper-V API route smoke | `PASS` | `full-admin-host-mutation-gate-20260805-04269`의 `installed-dotnet-host-hyperv-api-route-smoke` |
| 실제 VM 생성/조작/정리 | `PASS` | 잔여 pcv 검증 VM `0` |
| Gen2 boot order (FC-13) | `PASS` | 같은 날 code-level 수정 후 설치본 경로에서 동작 |
| Gen2 Secure Boot 템플릿 (FC-13) | `PASS` | 동일 |
| service/MSI lifecycle | `PASS` | 같은 gate |
| OS mutation gate | `PASS` | 방화벽 `0`, Event Log source 없음, boot unchanged |

FC-13의 근본 원인 분석과 실호스트 관측 상세는
`docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.md` 부록 A가
소유한다.

## 이 버전에서 재실행하지 않고 이월한 것

아래는 `0.42.65`에서 확인한 결과이며 `0.42.69`에서 **다시 실행하지 않았다.**

| 항목 | 이월 출처 |
| --- | --- |
| network QoS `2048 Kbps -> 2,048,000 bps` 변환과 native readback | `functional-correctness-actual-host-validation-2026-07-16-04265.md` |
| disk shrink guard와 `10 -> 11 GiB` expansion | 동일 |

해당 provider 코드는 `0.42.65` 이후 Gen2 firmware 설정만 추가됐고 QoS/disk resize 경로는 바뀌지
않았다. 그럼에도 이월은 재검증이 아니므로, 다음 QoS 또는 disk resize payload 변경 때 실제 VM으로
재실행해야 한다.

## 미검증으로 남은 것

| 항목 | 상태 | 이유 |
| --- | --- | --- |
| FC-05 credentialed guest execution | 미검증 | 부팅 가능한 격리 Windows guest와 전용 credential 부재 |
| FC-12(b) 비 ASCII credential 왕복 | code-level만 닫힘 | 동일. 코드 레벨 절반은 `dcb703ad`가 닫았다 |

상세는 `docs/project-status-audit-2026-08-05.md` §12가 소유한다.

## Nonclaims

- 이 evidence는 internal admin-smoke 범위다.
- manual-admin package-pair closure를 승격하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
