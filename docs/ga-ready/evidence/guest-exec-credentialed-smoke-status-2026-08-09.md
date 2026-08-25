# Credentialed Windows guest-exec 왕복 smoke 상태 (2026-08-09)

evidence_id: `guest-exec-credentialed-smoke-status-2026-08-09`
result: `SUPERSEDED-BY-PASS`
evidence_scope: `status-only-historical`
successor_evidence: `docs/ga-ready/evidence/guest-exec-credentialed-smoke-2026-08-09-04271-pass.md`
host_mutation_performed: `false` (이 status 문서 자체)
guest_command_performed: `false` (이 status 문서 자체; 후속 PASS evidence에서 실행)
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

> **2026-08-09 후속:** 관리자 opt-in으로 설치본 `0.42.71` credentialed guest-exec + FC-12(b)
> argv fidelity 왕복을 실행했고 PASS다. 소유 evidence는
> `guest-exec-credentialed-smoke-2026-08-09-04271-pass.md`다.

## 1. 이미 닫힌 것

| 항목 | 상태 | 소유 |
| --- | --- | --- |
| FC-12(b) argv 데이터 전달 source 수정 | PASS | `guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md` |
| ADR-0009 argv fidelity 조항 | PASS | ADR-0009 `## Argv Fidelity 경계` |
| `0.42.71` package / fullgate / manual-admin / current-card | PASS | 설치본에 수정 반영, Hyper-V route + `GuestArgvInvocation` 문자열 |
| code-level argv fidelity tests | PASS | `GuestExecutionArgvFidelityTests` |

followup-work-plan §5가 말한 “설치본 반영” 조건은 `0.42.71` chain으로 닫혔다. fullgate는
credentialed guest 왕복을 범위에 넣지 않는다.

## 2. 아직 별도 smoke인 것

**credentialed Windows guest `guest-exec` 왕복 재실행**은 설치본에 부팅 가능한 격리 Windows
guest VHD, DPAPI/credential ref, 관리자 opt-in이 필요하다.

| 선행 PASS smoke | 비고 |
| --- | --- |
| `guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md` | 0.42.55 actual credentialed |
| `guest-execution-running-cancel-installed-2026-05-28-04254-pass.md` | long-running cancel |

이번 세션에서는 해당 smoke를 **실행하지 않았다.** 이유:

1. 관리자 host mutation / guest credentialed execution은 사용자 opt-in 없이 돌리지 않는다
2. 현재 workspace에 즉시 쓸 수 있는 dedicated Windows guest VHD·credential 존재를 확인·주장하지
   않았다
3. FC-12(b) 설치본 반영은 package chain이 이미 닫았고, 왕복 smoke는 선택적 재확인이다

관련 tool 후보: `packaging/windows-desktop-node/tools/Invoke-PcvInstalledCliQosGuestSmoke.ps1`
(QoS/guest 설치본 CLI smoke; credentialed argv 왕복 전용 runner와 동일하지 않을 수 있음).

## 3. 착수 조건 (다음 실행자)

1. 사용자 관리자 opt-in
2. 설치본 operational anchor(`0.42.71-admin-smoke` 이상)와 부팅 가능 Windows guest target
3. credential ref / protected secret 경로 (command line에 raw secret 금지)
4. 성공 시 evidence: `docs/ga-ready/evidence/guest-execution-...-04271-...md` 신규 작성

## 4. Nonclaims

- 이 문서는 smoke PASS를 주장하지 않는다
- guest 쪽 비 ASCII 왕복 PASS를 주장하지 않는다
- host mutation / guest command를 실행하지 않았다
