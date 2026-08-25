# Release, LAN, OS Gated Preapproval Evidence - 2026-05-04

evidence_id: release-lan-os-gated-preapproval-2026-05-04
created_at: 2026-05-04T01:03:00+09:00
source_commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
route_matrix_commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
public_trusted_signing: excluded
release_execution: not-approved
lan_exposure_execution: not-approved
os_mutation_execution: scoped-opt-in-recorded
aggregate_gate_effect: blocked
followup_stable_internal_release_execution: pass-2026-05-05
followup_scoped_lan_exposure_execution: pass-2026-05-05
followup_aggregate_gate_status: closed-candidate-2026-05-05
machine_readable_json_created: no

## Release-gated Rows

release_gated_rows: local payload update, rollback restore, trust store install, trust store removal
release_gated_prerelease_evidence_status: pass

Allowed pre-release evidence:

- allowed_pre_release_evidence: package manifest/hash/provenance validation
- allowed_pre_release_evidence: ADR-0002 channel/version contract validation
- allowed_pre_release_evidence: update/rollback dry-run planning
- allowed_pre_release_evidence: non-mutating product root and previous root ownership checks
- allowed_pre_release_evidence: trust certificate artifact identity parsing without store write
- allowed_pre_release_evidence: diagnostics redaction and no-auto-reboot evidence

Forbidden before separate release approval:

- forbidden_execution: stable publication
- forbidden_execution: public trusted signing execution
- forbidden_execution: certificate store write/delete
- forbidden_execution: external update activation
- forbidden_execution: rollback restore activation

## LAN-gated Row

lan_gated_rows: firewall rule enable LAN exposure
lan_gated_preapproval_evidence_status: pass

Allowed pre-LAN evidence:

- allowed_pre_release_evidence: firewall rule tuple validation
- allowed_pre_release_evidence: loopback default preservation proof
- allowed_pre_release_evidence: token source proof without token mutation
- allowed_pre_release_evidence: non-mutating firewall ownership checks
- allowed_pre_release_evidence: scope planning, conflict diagnostics, redaction evidence, no-auto-reboot evidence

Forbidden before separate LAN exposure approval:

- forbidden_execution: firewall rule create/update/delete
- forbidden_execution: non-loopback listener exposure
- forbidden_execution: token source mutation
- forbidden_execution: external network reachability proof

## OS Mutation Rows

os_mutation_rows: Event Log source registration, Event Log source removal, firewall rule enable LAN exposure, firewall rule removal, trust store install, trust store removal
os_mutation_execution: scoped-opt-in-recorded

OS mutation rows have scoped actual mutation evidence for Event Log registration, firewall-only create/enable/remove, and trust-store-only import/remove. At this preapproval snapshot, Event Log removal actual mutation was not-run. Later `0.35.5-admin-smoke`, `0.35.6-admin-smoke`, and `0.35.7-admin-smoke` evidence executed Event Log register/remove with final source absence. Release approval and LAN exposure approval remain separate gates even when preapproval evidence is pass.

## 후속 관리자 Opt-In 실행

이 문서의 header 값은 2026-05-04T01:03:00+09:00 preapproval snapshot이다. 이후 사용자가 별도 관리자/host mutation opt-in을 제공해 실제 OS mutation evidence를 분리 수집했다.

- `artifacts/eventlog-source-registration-20260504-actual-registry`: Event Log source registration 실제 registry mutation.
- `artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330`: Service/MSI/Hyper-V `0.33.0-admin-smoke`, row-isolated firewall-only create/enable/remove smoke, row-isolated trust-store-only import/remove smoke.
- public trusted signing과 stable publication은 계속 excluded이며, 위 후속 실행은 `AllowUnsignedDev` admin-smoke와 scoped test certificate trust-store mutation evidence다.
- 후속 실행 이후 current owner migration은 code-level native owner 기준으로 닫혔다. 이 시점에는 release approval, LAN exposure approval, physical archive move, repo migration blocker가 별도 closure로 남아 aggregate gate가 blocked였고, 2026-05-05 closure 후보는 별도 보고서에 기록한다.

## 2026-05-05 후속 Fast-Mode Opt-In 실행

사용자는 public trusted signing 제외, AllowUnsignedDev/internal trust evidence 사용, OS mutation gate 일괄 실행, rollback/final-state proof 포함 조건으로 후속 작업을 승인했다. 이 승인은 stable publication 또는 public trusted signing 승인이 아니다.

- `artifacts/os-mutation-gates-20260505-003459-0341`: `0.34.1-admin-smoke` current native MSI/firewall/LAN/internal trust-store gate.
- MSI provenance commit은 `6f97a24aa2bdfacf33d7bd987559eb85e363e119`, 후속 firewall missing-rule lookup hardening commit은 `49a06acd3493066a10ec26fe541d5d8be1005c2b`, MSI SHA-256은 `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`다.
- MSI lifecycle은 install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall/final restore 모두 exit `0`였고 final service는 loopback-only `Running`이다.
- LAN/firewall 실행은 owned rule `PureCVisor Desktop Node Local API LAN`의 native enable/remove와 LAN IP prefix `http://[redacted-private-endpoint]:7777/` runtime policy `HTTP 200`으로 확인했다. `0.0.0.0` prefix는 Windows HttpListener unsupported 결과로 기록하고 LAN IP prefix로 재시도했다.
- Trust-store 실행은 ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`의 native install/remove/restore로 확인했고 final present 상태로 복구했다.
- Public trusted signing, stable publication, local payload update, rollback restore는 이 OS mutation gate에서는 실행하지 않았다. LAN/firewall/internal trust-store scoped execution evidence는 이 artifact로 분리하고, stable internal release/update/rollback execution은 아래 후속 evidence로 분리한다.

## 2026-05-05 Current HEAD OS Mutation Gate 재실행

사용자 재승인 후 당시 HEAD `744a15536569e89f948927bea9179fc0eeae3ff4` 기준으로 실제 MSI 설치/제거, firewall enable/remove, LAN exposure, trust-store install/remove/restore gate를 fresh 실행했다. Public trusted signing과 외부 stable publication은 계속 제외했다.

- `artifacts/os-mutation-gates-20260505-033503-0354`: `0.35.4-admin-smoke` current native MSI/firewall/LAN/internal trust-store gate.
- MSI SHA-256은 `bf7d0d2bd83545e83fbdf0dfb96b715f8e09471474445ae1c0db1d076be2c1e4`이고 signing mode는 `AllowUnsignedDev`다.
- MSI lifecycle은 install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall 모두 exit `0`였고 final restore는 internal signed stable `0.35.2` MSI SHA-256 `7d9cf1f7ed157027ff128c3fadfa8fd82576d86166f6a214ac52c7190191e959`, Authenticode `Valid` artifact로 수행했다.
- LAN/firewall 실행은 owned rule `PureCVisor Desktop Node Local API LAN`의 native enable/remove, final firewall rule count `0`, LAN IP prefix `http://[redacted-private-endpoint]:7777/` runtime policy/Web root `HTTP 200`으로 확인했다.
- Trust-store 실행은 ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`의 native install/remove/restore로 확인했고 final present 상태로 복구했다.
- Final state는 installed DisplayVersion `0.35.2`, loopback-only service `Running`, runtime policy/host status/Web root `HTTP 200`, boot time unchanged다.

## 2026-05-05 0.35.5 OS Mutation Gate 재실행

사용자 재승인 후 당시 HEAD `2fb38f20a8c74433684345ded8a33ba16a863621` 기준으로 실제 Hyper-V, MSI, firewall, trust-store, LAN, Event Log, runtime job store write와 data-root allowlist removal evidence를 다시 수집했다. Public trusted signing과 외부 stable publication은 계속 제외했다. Config/job store migration apply는 current product route가 아니므로 이 실행 범위에서 제외했다.

- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`: `0.35.5-admin-smoke` MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke.
- `artifacts/os-mutation-gates-20260505-101659-0355-final`: Event Log register/remove, firewall enable/remove, LAN IP runtime policy/Web assets `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore.
- MSI SHA-256은 `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`이고 signing mode는 `AllowUnsignedDev`다.
- Final state는 installed DisplayVersion `0.35.5`, loopback-only service `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `product config migration apply`와 `job store migration apply`는 current product route가 아니며 `future-route/not-implemented/blocked`라 실행하지 않았다. Job store 관련 evidence는 installed runtime job writes로 제한하고, synthetic `jobs.json` 삭제는 service/data-root allowlist removal evidence로 분리한다.

## 2026-05-05 0.35.6 OS Mutation Gate 재실행

사용자 재승인 후 실행 당시 code HEAD `cc723e28ed62f6f1c5e49c74ca68b87d0f1b8b3a` 기준으로 실제 Hyper-V, MSI, firewall, trust-store, LAN, Event Log, runtime job store write와 data-root allowlist removal evidence를 다시 수집했다. Public trusted signing과 외부 stable publication은 계속 제외했다. Config/job store migration apply는 current product route가 아니므로 이 실행 범위에서 제외했다.

- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun`: `0.35.6-admin-smoke` MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke.
- `artifacts/os-mutation-gates-20260505-170454-0356-rerun`: Event Log register/remove, firewall enable/remove, LAN IP runtime policy/Web assets `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore.
- MSI SHA-256은 `a24de44049519dea8405854a17272ebb362b061ff03a051cd61fb31669bc7d02`이고 signing mode는 `AllowUnsignedDev`다.
- Final state는 installed DisplayVersion `0.35.6`, loopback-only service `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `product config migration apply`와 `job store migration apply`는 current product route가 아니며 `future-route/not-implemented/blocked`라 실행하지 않았다.

## 2026-05-05 0.35.7 OS Mutation Gate 재실행

사용자 재승인 후 현재 HEAD `2ec9e71d45b702e106824c86500cd6152b18fab7` 기준으로 실제 Hyper-V, MSI, firewall, trust-store, LAN, Event Log, runtime job store write와 data-root allowlist removal evidence를 다시 수집했다. Public trusted signing과 외부 stable publication은 계속 제외했다.

- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`: `0.35.7-admin-smoke` MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke.
- `artifacts/os-mutation-gates-20260505-180434-0357-rerun`: Event Log register/remove, firewall enable/remove, LAN IP bearer runtime policy/Web assets `HTTP 200`, config-migration-apply blocked/no-mutation descriptor, ADR-0003 internal Root/TrustedPublisher install/remove/restore.
- MSI SHA-256은 `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`이고 signing mode는 `AllowUnsignedDev`다.
- Final state는 installed DisplayVersion `0.35.7`, loopback-only service `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- 첫 `artifacts/os-mutation-gates-20260505-175453-0357` 시도는 LAN static asset probe에서 bearer Authorization을 누락해 401로 실패했고, cleanup은 완료됐다. `180434-0357-rerun`은 bearer auth로 static Web asset `HTTP 200`을 확인한 superseding artifact다.
- `job store migration apply`는 current product route가 아니며 `future-route/not-implemented/blocked`라 실행하지 않았다.

## 2026-05-05 Stable Internal Release/Update/Rollback 실행

사용자는 physical archive move, blocked row 14개 closure, tier2/tier3 full fresh evidence, stable release/update/rollback execution을 추가 승인했다. 이 승인은 public trusted signing 또는 외부 stable publication 승인이 아니다.

- `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353`: internal enterprise `RequireSigned` stable `0.35.2`/`0.35.3` build, install/repair/uninstall/remove-data, local payload update, rollback restore evidence.
- `0.35.2` MSI SHA-256은 `7d9cf1f7ed157027ff128c3fadfa8fd82576d86166f6a214ac52c7190191e959`이고 `0.35.3` MSI SHA-256은 `25942dea9fb0476bc8648acfae6cb09f1194b6366bb60d4bdf23f2c488e2d8de`다.
- 두 MSI는 Authenticode `Valid`, SignTool verify exit `0`, signer `CN=PureCVisor Desktop Node Internal Code Signing`, issuer `CN=PureCVisor Internal Code Signing Root CA`, thumbprint `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`로 검증됐다.
- 실제 실행 26단계는 모두 pass였고 final active product version은 `0.35.2`, failed diagnostics root version은 `0.35.3`, final service는 loopback-only `Running`, final active root legacy WinSW files는 `0`개, boot time은 unchanged다.
- 이 후속 실행으로 release/update/rollback execution blocker는 닫혔다. Public trusted signing과 외부 stable publication claim은 계속 excluded/not-claimed다.

## 2026-05-05 0.36.1 Batch-Supervised Service/MSI/Hyper-V 재실행

사용자 승인 후 `0.36.1-admin-smoke` Service/MSI/Hyper-V route parity를 Batch Supervisor로 감싸 실행했다. 이 실행은 firewall/LAN/Event Log/trust-store OS gate rerun이 아니며 public trusted signing 또는 외부 stable publication 승인이 아니다.

- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`: Batch Supervisor summary `ok=true`, `status=completed`, `timed_out=false`, heartbeat lines `25`.
- `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: Service/MSI/Hyper-V route parity `0.36.1-admin-smoke`.
- MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`이고 signing mode는 `AllowUnsignedDev`다.
- Final state는 installed DisplayVersion `0.36.1`, loopback-only service `Running`, boot time unchanged, `remaining_pcv_vms=[]`다.
- 최신 OS mutation gate는 계속 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다.

## 판정

이 evidence의 header 본문은 2026-05-04 preapproval snapshot이며 release execution 또는 LAN exposure 실행 승인 기록이 아니었다. 2026-05-05 후속 fast-mode opt-in과 당시 HEAD 재실행은 current native firewall/LAN/Event Log/internal trust-store scoped execution evidence와 stable internal release/update/rollback execution evidence로 분리한다. Public trusted signing과 외부 stable publication은 제외한다. Aggregate closure 후보는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`에 별도 기록한다.
