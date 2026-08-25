# Post-04245 Extension Phase 2-5 Planning Evidence

evidence_id: `post-04245-extension-phase2-5-planning-2026-05-26`
result: `PASS_DOCS_ONLY`
scope: `extension-phase2-5-roadmap-to-implementation-plan`
version_anchor: `0.42.45-admin-smoke`
phase2_hyperv_qos_mutation_policy_adr: `docs/adr/0008-hyperv-qos-mutation-policy.md`
phase2_hyperv_qos_mutation_design: `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation-design.md`
phase2_hyperv_qos_mutation_plan: `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md`
phase3_direct_control_guard: `backend-policy-first`
phase4_guest_execution_security_adr: `docs/adr/0009-guest-execution-security-boundary-candidate.md`
phase5_account_novnc_target_config_security_adr: `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md`
host_mutation_performed: `false`
package_build_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결론

`1-2-3-4-5` 승인 범위는 실제 host mutation이나 package build가 아니라 다음 implementation
slice를 흔들리지 않게 만들기 위한 Phase 2-5 규약 산출물로 닫는다.

Phase 2는 ADR-0008과 implementation plan으로 쪼갰다. `vm blkio-set`과 bandwidth mutation은
Hyper-V 전용 policy command 후보이며, dry-run preview, queued apply, rollback descriptor,
readback evidence, actual VM admin smoke가 닫히기 전까지 지원 완료로 표시하지 않는다.

Phase 3은 `backend-policy-first`로 유지한다. Web/TUI direct mutation control은 Phase 2 backend
policy와 installed mutation evidence가 닫힌 뒤에만 시작한다.

Phase 4는 ADR-0009로 security boundary 후보를 만들었다. Guest Execution / Guest Channel은
credential, audit log, secret redaction, timeout/cancel, RBAC가 닫힐 때까지 계속 미지원이다.

Phase 5 account/noVNC target config mutation은 ADR-0010 후보로 분리했다. noVNC target host/port
self-service 변경은 loopback-only 기본값, LAN explicit gate, audit, rollback, service reload
policy가 닫히기 전까지 제품 기능으로 열지 않는다.

## 산출물

| 범위 | 산출물 | 상태 |
| --- | --- | --- |
| Phase 2 Hyper-V QoS Mutation Policy | `docs/adr/0008-hyperv-qos-mutation-policy.md` | `candidate-written` |
| Phase 2 design spec | `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation-design.md` | `ready-for-implementation-plan` |
| Phase 2 implementation plan | `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md` | `ready-for-first-code-slice` |
| Phase 3 Web/TUI Direct Control | this evidence guard | `deferred-backend-policy-first` |
| Phase 4 Guest Execution / Guest Channel | `docs/adr/0009-guest-execution-security-boundary-candidate.md` | `security-boundary-deferred` |
| Phase 5 Account/noVNC target config | `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md` | `security-policy-required-before-mutation` |

## 다음 구현 순서

1. Phase 2 contract/DTO tests.
2. Hyper-V QoS planner preview.
3. Local API preview routes.
4. queued apply job and rollback descriptor.
5. PCVCLI `blkio-set` / `bandwidth-set` dry-run and apply UX.
6. actual VM admin smoke, package/fullgate/manual-admin closure.
7. Phase 3 Web/TUI direct control design only after Phase 2 closure.

## 경계

이 evidence는 docs/spec/plan guard다. 제품 코드, installed service, host config, Hyper-V VM,
package artifact를 변경하지 않았다. Public trusted signing, winget public submission, public stable
installer URL, external stable publication을 주장하지 않는다.
