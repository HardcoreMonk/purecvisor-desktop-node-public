# Functional correctness actual-host validation 2026-11-08 `0.42.98` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-11-08-04298-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.98-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92 (functional-correctness-actual-host-validation-2026-10-08-04292-carryforward) -> 0.42.93 (functional-correctness-actual-host-validation-2026-10-08-04293-carryforward) -> 0.42.96 (functional-correctness-actual-host-validation-2026-10-11-04296-carryforward) -> 0.42.98`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.98-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.42.96 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.96` (`functional-correctness-actual-host-validation-2026-10-11-04296-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.98` PASS (`full-admin-host-mutation-gate-20261108-04298`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-11-08-04298`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04298 product payload 변경 | `configure-installed` LAN prefix 옵션과 실행 중 service 재시작(PR #94, #95), Web Console 재설계와 옛 콘솔 퇴역, 브라우저 품질 게이트(PR #97~#103, #105~#107), 계정 비밀번호 재설정·변경 route(PR #104), 이전 버전 제품 Update 호환 파일(PR #108, BL-0029). 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.96` fullgate provenance `9ac8eb3` 뒤 제품 변경은 release train `0.42.98`에 실은 PR #94, #95, #97~#108이다(train `0.42.97`은 pair clean-host bucket에서 정차해 승격하지 않았다). 변경 commit은 `271ab040`, `08c50e08`, `13fc25dd`, `ab9b6704`, `8b0ae171`, `16da5810`, `f0db33e4`, `eb0bdcf7`, `289d98ea`, `213bfe36`, `8403d47d`, `3d2fd89e`, `449a62ce`, `d8bf7be9`이고 payload commit `8e4ebe2`에서 도달한다. 그 뒤 `main` 변경은 train 기록 PR #109(문서와 캡처)뿐이고 MSI payload는 바뀌지 않았다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| 설치 service를 LAN prefix와 `--allow-lan`으로 다시 구성하면 실행 중 service가 새 listener로 다시 켜지고, Web Console이 새 셸(사이드바 그룹, VM 상세 탭, 4단계 새 VM 마법사, 복제·삭제 확인 모달, 체크포인트 예약 즉시 반영)로 바뀌며, 계정 비밀번호 재설정·변경 route가 생기고, 이전 버전 설치본의 제품 Update가 새 payload를 받는다 | `271ab040`, `08c50e08`, `13fc25dd`, `ab9b6704`, `8b0ae171`, `16da5810`, `f0db33e4`, `eb0bdcf7`, `289d98ea`, `213bfe36`, `8403d47d`, `3d2fd89e`, `449a62ce`, `d8bf7be9` | `lane2-service-configure-lan-2026-11-08-04298` PASS, `web-console-redesign-demo-2026-11-08` PASS, `lane2-account-password-reset-2026-11-08-04298` PASS(무인증 거절), pair clean-host bucket PASS(guest `0.42.96` → `0.42.98` 제품 Update) |

이 변경들은 service 구성, Web Console 표시·구조, 계정 자격 관리, web payload 파일 목록을 바꾸며 feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
