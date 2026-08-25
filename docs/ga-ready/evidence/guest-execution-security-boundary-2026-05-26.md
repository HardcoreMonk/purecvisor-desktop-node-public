# Guest Execution Security Boundary 문서 계약 증거

evidence_id: `guest-execution-security-boundary-2026-05-26`
result: `PASS_DOCS_CONTRACT`
scope: `phase4-guest-execution-security-boundary`
adr: `docs/adr/0009-guest-execution-security-boundary.md`
predecessor_adr: `docs/adr/0009-guest-execution-security-boundary-candidate.md`
design: `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary-design.md`
plan: `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md`
selected_from: `docs/ga-ready/evidence/post-04248-next-slice-selection-2026-05-26.md`
product_payload_change_detected: `false`
host_mutation_performed: `false`
package_build_performed: `false`
manual_admin_package_pair_performed: `false`
public_release: `not-claimed`

## 결과

ADR-0009를 후보 상태에서 적용 중인 security boundary contract로 승격했다. 이 evidence는
Guest Execution 기능 구현 완료 증거가 아니라, 구현 전에 닫아야 하는 정책/보안 경계를
닫은 문서 계약 증거다.

확정한 범위:

1. `pcvcli vm guest-exec <vm> -- <command>`는 raw credential을 받지 않고
   `credential_ref` 기반으로만 열 수 있다.
2. 모든 guest execution은 `guest-execution-audit-v1` audit record를 남긴다.
3. Command line, environment, stdin/stdout/stderr, diagnostics artifact에
   `guest-execution-redaction-v1`을 적용한다.
4. Execution은 queued job이며 default timeout 60초, max timeout 600초, cancel terminal
   state를 가진다.
5. `operate`만으로는 부족하며 `guest.exec`, `guest.channel.configure`, `job.cancel`
   capability를 분리한다.
6. `guest-agent-ensure-channel`은 qemu guest agent channel 생성이 아니라 Hyper-V/Windows
   guest execution channel prerequisite preview/verify/repair로 정의한다.
7. Web/TUI direct command input은 policy/API implementation과 confirmation/audit UX가
   닫히기 전까지 열지 않는다.

## Gate 판단

이번 변경은 docs/spec/plan/evidence/index 갱신이다. API route, CLI executable behavior,
Web/TUI runtime asset을 바꾸지 않으므로 product payload change가 아니다.

| Gate | 상태 | 이유 |
| --- | --- | --- |
| package build | `not-run-no-product-payload-change-docs-contract-only` | 설치 payload 변경 없음 |
| full admin host mutation | `not-run-no-product-payload-change-docs-contract-only` | host mutation 실행 대상 없음 |
| manual-admin package-pair | `not-run-no-product-payload-change-docs-contract-only` | 새 package 없음 |
| public-boundary | `required-after-main-push` | docs boundary guard는 push 후 CI로 확인 |

## 다음 Product Payload 조건

아래 중 하나라도 코드로 열면 다음 `0.42.x-admin-smoke` package chain을 실행한다.

- Local API guest execution policy/preview/execute route 추가.
- PCVCLI `vm guest-exec` 또는 `vm guest-agent-ensure-channel` behavior 변경.
- Web/TUI Guest Execution status/preview/direct control asset 변경.
- Hyper-V provider PowerShell Direct 또는 channel repair code path 추가.

## 경계

이 evidence는 Guest Execution 사용 가능, 실제 VM guest command execution, credential
creation, host mutation, package build, public trusted signing, external stable publication을
주장하지 않는다.
