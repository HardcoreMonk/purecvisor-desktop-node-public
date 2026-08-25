# Desktop Node ADR 인덱스

## 2026-08-23 현재 기준

ADR-0015를 채택해 설치·rollback 기준선인 `operational_current`와 기능별
`feature_qualification`을 분리했다. Stable Feature ID, required stage와 dated evidence locator는
`config/desktop-node-feature-evidence-ledger.json`이 소유한다. Candidate promotion evaluator와
current/Ops Summary enforcement는 AR-001 후속 구현 전이므로 현재 상태를
`채택 / evaluator 구현 전`으로 제한한다. 이 결정은 0.42.75 campaign이나 host mutation을 승인하지
않는다.

## 2026-08-03 현재 기준

ADR-0013을 적용해 JSON job store의 current writer를 canonical-path write-transaction named mutex와
loaded-base identity CAS로 제한했다. Schema v1/v2에는 revision 필드를 추가하지 않고 실제 frozen
`0.42.65-admin-smoke` reader 호환성을 유지한다. 이 결정은 process lifetime lease, mixed-version
concurrent writer 또는 Hyper-V side effect exactly-once를 주장하지 않는다. Wave 2A completion
evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`가
소유한다. Post-reboot `0.42.66-admin-smoke` legacy 설치본 checkpoint PASS는
`docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`가
소유하며 operational full-admin anchor는 `0.42.65-admin-smoke`를 유지한다.

## 2026-07-16 현재 기준

ADR-0011을 적용해 Desktop Node의 active operator surface를 Web Console과 PCVCLI로
고정했다. TUI source/package/smoke/current 문서 계약은 제거하며 Local API/backend는
유지한다. Code-level evidence는
`docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`가 소유한다.
`0.42.65-admin-smoke` package/fullgate/actual-VM functional correctness/CLI-Web installed
current-card가 current operational anchor다. `0.42.64-admin-smoke`는 immediate CLI/Web predecessor이며
`0.42.62-admin-smoke` Web/TUI/CLI current-card는 dated historical predecessor다. 이 승격은
ADR-0011의 operator surface와 ADR-0006 public release boundary를 변경하지 않는다.

## 2026-05-29 historical predecessor

ADR-0007을 적용한 PCVCLI Hyper-V QoS/guest service parity 경계는 유지한다. 최신 운영
증거는 `0.42.59-admin-smoke` full admin host mutation anchor와 installed Web/TUI/CLI
current-card가 함께 소유한다. 최신 닫힌 manual-admin package-pair closure는
`0.42.58-admin-smoke -> 0.42.59-admin-smoke`가 소유한다. 설치본 Web/TUI/CLI current-card는 04259 fullgate 후 PASS했고,
실제 VM 기반 설치본 TUI row projection은
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`에서
PASS로 닫혔다. Anchor는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
`current-evidence-ledger-2026-05-29-04259-public-boundary-docs-maintenance-postpush-pass`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`다.
최신 closed manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`가 소유한다.
최신 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`가
소유하고 run `26636072420`, job `78496568595`, head
`5a2f91762a6c2a8ab6b84d334fa6cb420474671f`에서 PASS했다. `0.42.60-admin-smoke`
installed current-card payload 후보는 이미 열려 있으며, docs-maintenance postpush는 추가
package 후보를 열지 않는다. account/noVNC는
0.42.58 PASS를 carry-forward하고 actual VM Guest Execution/QoS smoke는 provider/control payload
변경 때 재실행한다. 0.42.57/0.42.56/0.42.54 fullgate/running cancel public-boundary,
0.42.53 public-boundary, PR #169 public-boundary와 후속
`docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`는
historical predecessor로 보존한다. Public trusted signing과 external stable publication은 여전히 ADR-0006
out-of-scope다. 아래 이전 날짜 current 문단은 historical predecessor로 해석한다.

Post-04245 확장 로드맵은 Phase 2-5를 규약 산출물로 먼저 분리했고, ADR-0008은
2026-05-26 slice에서 Hyper-V QoS mutation의 dry-run, queued apply,
rollback/readback descriptor, PCVCLI UX, native adapter WMI code path까지 product payload로
승격했다. Source evidence는
`docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`이고, 설치본 evidence는
`docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`다.
`0.42.47-admin-smoke` package build, full admin host mutation gate, 실제 VM 대상 PCVCLI
storage/network QoS mutation smoke는 PASS했다. manual-admin package-pair closure는
`0.42.45-admin-smoke -> 0.42.47-admin-smoke`로 닫혔으므로 최신 operational anchor 완료
기능으로 표시한다. ADR-0009는 Guest Execution /
Guest Channel security boundary contract로 적용됐고, 0.42.53에서 provider route,
channel verify/repair, Web/TUI direct-control surface까지 열었다. 실제 Windows guest
credentialed execution smoke는 persistent Windows VHD target과 DPAPI LocalMachine
credential reference 기준으로 PASS했다. Running interrupt/cancel은 0.42.54 설치본 package/current-card와
actual long-running Windows guest smoke에서 PASS했고, 0.42.55는 Web/TUI running cancel affordance와
actual credentialed guest-exec를 설치본으로 재확인했다.
ADR-0010은 noVNC target config mutation을 loopback-only/LAN explicit gate 정책 후보로
유지한다. ADR-0007의 readback-first 지원 경계는
`vm blkio-get`, `vm bandwidth`, guest-service readback 명령에 대해 유지된다.

ADR-0009의 최신 product payload는
`docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253.md`에서
provider route, channel verify/repair, Web/TUI direct-control, redaction/audit contract로 PASS했다.
`0.42.55-admin-smoke` package/current-card와 actual credentialed guest-exec smoke는 PASS했고,
04250→04254 manual-admin readiness는 현재 host baseline mismatch로 blocked다. Full admin host
mutation anchor는 `0.42.55-admin-smoke`다.

Phase 3 Web/TUI QoS direct control은 같은 ADR-0008 route contract를 Web Console/TUI에
제품화했다. Code-level evidence는
`docs/ga-ready/evidence/phase3-web-tui-qos-direct-control-code-level-2026-05-26.md`이고,
package/fullgate/current-card evidence는 `0.42.48-admin-smoke`,
`docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`가 소유한다.
`0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair closure는 닫혔다.

## 2026-05-21 historical predecessor

ADR-0007을 적용해 PCVCLI Hyper-V QoS/guest service parity 경계를 닫았다. 최신 운영
증거는 `0.42.40-admin-smoke` full admin host mutation / manual-admin package-pair
closure가 소유한다. 설치본 Web/TUI/CLI current-card는 04240 기준으로 PASS했고,
실제 VM Web/TUI QoS/guest readback smoke는 설치본 TUI row projection blocker를
드러낸 뒤 source fix code-level PASS와 `0.42.41-admin-smoke` package chain trigger로
분리했다. Anchor는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
`current-evidence-ledger-2026-05-21-04240-current-card-04241-trigger`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`,
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`다.
최신 closed manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md`가 소유한다.

`0.42.39-admin-smoke`는 ADR-0007의 Hyper-V QoS/guest-service 제한 승격을 설치본으로
닫은 검증이다. 설치본 targeted smoke
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`는
실제 VM 대상 CLI command path를 추가 확인했다. `0.42.38-admin-smoke -> 0.42.39-admin-smoke`
manual-admin candidate는 closed 상태이며
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`에 보존한다.
Web/TUI QoS/guest readback surface는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`에서 code-level
PASS했고, ADR-0007의 Web/TUI 결정은 `implemented-readback-surface-no-direct-control`이다.
Direct mutation/control은 여전히 닫혀 있으며 `0.42.40-admin-smoke` package chain은
`closed-manual-admin-package-pair-04239-04240`로 닫혔다.
PR #167 public-boundary PASS는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`로
보존하며 current public-boundary는 PR #167 evidence가 소유한다. PR #164/PR #163/PR #162/PR #160
public-boundary는 historical predecessor로 보존한다. Public trusted
signing과 external stable publication은 여전히 ADR-0006 out-of-scope다. 아래 이전 날짜
current 문단은 historical predecessor로 해석한다.

## 2026-05-17 현재 기준

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

> 대상: `purecvisor-desktop-node` Windows 전용 저장소

```text
DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index
```

이 문서는 Desktop Node 저장소에서 현재 적용 중인 설계 결정의 진입점이다. Phase spec과 plan은 상세 설계와 이력을 보존하고, ADR은 현재 적용되는 결정과 변경 시 확인해야 할 검증 기준을 짧게 고정한다.

Linux `purecvisor-single`의 ADR은 이 저장소의 단일 진실이 아니다. Desktop Node 결정은 이 인덱스와 `docs/adr/` 아래의 Desktop Node ADR을 우선한다.

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
mutation/current-card다.
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`가
`runtime-api-current-evidence-rollup-v1`, installed Web/TUI/CLI current-card PASS를
기록한다. 최신 닫힌 Manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`가 소유하고,
`manual-admin-campaign-descriptor-20260518-04229-04230-closed`는 `missing_count=0`,
`not_pass_count=0`으로 닫혔다.
Target operational MSI SHA-256은 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`,
update ZIP SHA-256은 `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`,
provenance commit은 `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`다.
`docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`는
initial blocked descriptor를 `blocked-by-missing-evidence`로 보존한다. 이 evidence도
ADR-0006 internal-private-network-only 범위이며 public trusted signing 또는 외부
stable publication evidence가 아니다. Earlier
`docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md` package build record는
historical package candidate로 보존하고, 최신 package build record는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04230.md`다.
이전 `0.42.24-admin-smoke -> 0.42.25-admin-smoke` Manual-admin package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`와
`manual-admin-campaign-descriptor-20260516-04224-04225-closed`로 historical closed
package-pair evidence로 보존한다. Target/full-gate MSI SHA-256은
`e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, update ZIP
SHA-256은 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`,
provenance commit은 `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`이다.
Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle package-pair는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` /
`manual-admin-campaign-descriptor-20260517-04226-04227-closed`로 보존하고,
Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Operator Surface package-pair는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` /
`manual-admin-campaign-descriptor-20260517-04227-04228-closed`로 보존한다.
PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`,
run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`이다.

## 현재 적용 중인 ADR

| ADR | 상태 | 결정 | 관련 문서 |
|-----|------|------|-----------|
| `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md` | 적용 중 | 독립 Windows 저장소와 phase 19 evidence-first 이력 보존. 제품 런타임 승격 판단은 ADR-0004가 대체 | Phase 11, Phase 12-18, Phase 19 spec |
| `docs/adr/0002-release-version-policy.md` | 적용 중 | Phase 22 release/version policy와 installer artifact/channel contract 채택 | Phase 22 spec/plan, Phase 20 evidence plan, installer README |
| `docs/adr/0003-internal-trusted-signing-policy.md` | 적용 중 | 내부 서비스용 internal root/leaf `RequireSigned` signing trust model 채택. public trusted signing은 내부 전용 서비스 범위 밖 | Phase 20 evidence plan, installer README |
| `docs/adr/0004-ga-ready-product-runtime-candidate.md` | 적용 중 | 내부 전용 서비스 범위의 PowerShell-free product ops/runtime을 현재 제품 런타임 결정으로 채택 | GA-ready redesign spec, route promotion matrix, repo migration map, verification ownership map, aggregate closure evidence |
| `docs/adr/0006-internal-private-network-distribution.md` | 적용 중 | 내부 사설망 전용 배포 경계 고정. public signing/winget/external upload/public clean-host smoke는 out-of-scope로 재분류하고 internal distribution matrix를 적용 | `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`, `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| `docs/adr/0007-pcvcli-hyperv-qos-guest-service-parity.md` | 적용 중 | PCVCLI Linux parity 잔여 QoS/guest-service 명령을 Hyper-V readback-first semantics로 제한 승격. Linux cgroup/qemu guest agent claim은 하지 않음 | `docs/ga-ready/evidence/pcvcli-hyperv-qos-guest-service-slice-2026-05-20.md`, `docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md` |
| `docs/adr/0008-hyperv-qos-mutation-policy.md` | 적용 중 | `vm blkio-set` / `vm bandwidth-set`를 Hyper-V QoS mutation으로 재정의하고 preview/queued apply/native WMI/CLI UX를 구현. 0.42.47 package/fullgate/actual VM smoke와 manual-admin closure가 PASS. Phase 3 Web/TUI direct control은 0.42.48 package/fullgate/current-card/manual-admin package-pair closure PASS | `docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`, `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`, `docs/ga-ready/evidence/phase3-web-tui-qos-direct-control-code-level-2026-05-26.md`, `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`, `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation-design.md`, `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md`, `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase3-web-tui-direct-control-design.md`, `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase3-web-tui-direct-control.md` |
| `docs/adr/0009-guest-execution-security-boundary.md` | 적용 중 | Guest Execution / Guest Channel의 credential reference, audit schema, secret redaction, **argv fidelity**, timeout/cancel, RBAC, channel lifecycle 경계를 확정. Route/CLI/Web/TUI 실행은 다음 product payload까지 disabled. 2026-08-08에 argv fidelity 조항을 추가했다 — guest 실행 경계를 넘는 인자는 데이터로 전달하며 guest 측에서 코드로 재해석하지 않는다 | `docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md`, `docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`, `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary-design.md`, `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md` |
| `docs/adr/0011-cli-web-only-operator-surface.md` | 적용 중 | 활성 운영자 표면을 Web Console과 PCVCLI로 제한하고 TUI source/package/smoke/current 문서 계약 제거. Local API/backend 유지 | `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md` |
| `docs/adr/0013-job-store-single-writer-transaction-lease.md` | 적용 중 | schema v1/v2를 유지하면서 canonical-path transaction mutex와 loaded-base SHA/length CAS로 stale current writer의 lost update를 거절. lifetime/mixed-version writer와 exactly-once는 비주장 | `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`, `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`, `docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md` |
| `docs/adr/0015-feature-evidence-promotion-policy.md` | 채택 / evaluator 구현 전 | operational current와 기능별 qualification을 분리하고 mandatory stage가 모두 PASS일 때만 candidate promotion eligibility를 허용 | `config/desktop-node-feature-evidence-ledger.schema.json`, `config/desktop-node-feature-evidence-ledger.json`, `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md` |

## 적용 전/대체 ADR 후보

| ADR | 상태 | 결정 후보 | 관련 문서 |
|-----|------|-----------|-----------|
| `docs/adr/0009-guest-execution-security-boundary-candidate.md` | 대체됨 | Guest Execution / Guest Channel은 credential, audit log, secret redaction, timeout/cancel, RBAC security boundary가 닫힐 때까지 구현하지 않음. 현재 적용 문서는 `docs/adr/0009-guest-execution-security-boundary.md` | `docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26.md` |
| `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md` | 후보 | noVNC target config mutation은 loopback-only 기본값, LAN explicit gate, audit/rollback/service reload policy가 닫힐 때까지 구현하지 않음 | `docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26.md` |
| `docs/adr/0012-api-read-concurrency-policy.md` | 폐기됨 (`closed-not-adopted`) | API read concurrency 병행 대안은 채택하지 않고 processor 전체 직렬화와 single mutation worker를 최종 불변조건으로 유지 | `docs/ga-ready/evidence/csharp-architecture-wave5a-admission-lifetime-code-slice-2026-08-03.md`, C# architecture Wave 5 plan |

ADR-0004 supporting evidence:

- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`는 ADR-0004 내부 전용 제품 런타임 판단 이후 이전 full admin host mutation gate evidence다. 이 evidence는 `0.42.24-admin-smoke`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04224`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04224`, `artifacts/os-mutation-gates-batch-profile-20260516-04224`를 근거로 한다. Full-gate MSI SHA-256은 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, package build MSI SHA-256은 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, provenance commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V, firewall/LAN/Event Log/internal trust-store, Web Console `http://127.0.0.1/`, `/pcv-config.js`, `PCV_AUTH_REQUIRED` boundary와 installed listener `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04224`, `runtime-api-current-evidence-rollup-v1` current-card smoke를 함께 확인했다. Runtime/API registry bridge는 `runtime-api-diagnostics-ops-summary-registry-bridge-v2`이고 route detail count는 `4`다. `manual-admin-campaign-descriptor-20260516-04223-04224`는 `blocked-by-missing-evidence`, `missing_count=5`, `not_pass_count=1`인 historical blocked descriptor다. 2026-05-16 04225 evidence 이후 historical predecessor로 보존한다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- Previous 04221 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221`, `artifacts/os-mutation-gates-batch-profile-20260516-04221`, full-gate MSI SHA-256 `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`, provenance commit `3b8c48deb4c31675f6fce46c320703f23c27c131`로 보존한다.
- Previous 04220 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`, full-gate MSI SHA-256 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`로 보존한다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`, `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04226.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`, `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`는 historical `0.42.26-admin-smoke` operational/package evidence와 closed package-pair evidence다. Full-gate/target operational MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, package build MSI SHA-256은 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`, provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`다. `0.42.25-admin-smoke -> 0.42.26-admin-smoke` initial descriptor `manual-admin-campaign-descriptor-20260516-04225-04226`는 readiness PASS지만 당시 `missing_count=4`, `not_pass_count=1`이었고, 2026-05-17 closure descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`가 `missing_count=0`, `not_pass_count=0`으로 닫았다. PR #145 post-merge public-boundary evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md`, run `25961834812`, job `76318357776`, head `d6500c01c972cbc7ca1e290e51120181ceea1501`이다. Current 닫힌 Manual-admin package-pair는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`가 소유한다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`는 `0.42.22-admin-smoke` previous product payload package evidence다. MSI SHA-256은 `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`이고 provenance commit은 `8a38995cc25a888f64473e9a2869740949ad6b24`다. Historical 04222 full-gate MSI SHA-256은 `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`다. `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04221-04222.md`는 `0.42.21-admin-smoke -> 0.42.22-admin-smoke` descriptor candidate를 `blocked-by-missing-evidence`로 기록했고, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`가 실제 Burn idempotence blocker를 보존한다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04220-04221.md`는 `0.42.20-admin-smoke` baseline과 `0.42.21-admin-smoke` target package의 이전 닫힌 MANUAL-ADMIN package-pair PASS evidence다. Readiness, installed update/rollback, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation v2가 PASS다. Target MSI SHA-256은 `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b`, update ZIP SHA-256은 `09e1c3f5a7c8d2afac3d70bddbb1d91f575de2c45c9174a8da2bbb73c2e89767`, provenance commit은 `3b8c48deb4c31675f6fce46c320703f23c27c131`다. `0.42.19-admin-smoke -> 0.42.20-admin-smoke` package-pair와 `0.42.17-admin-smoke` clean-host regression은 historical evidence로 보존한다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- Historical MANUAL-ADMIN package-pair evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`와 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`로 보존한다. `0.42.11-admin-smoke -> 0.42.12-admin-smoke` target MSI SHA-256은 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, update ZIP SHA-256은 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`이고, `0.42.12-admin-smoke -> 0.42.13-admin-smoke` target MSI SHA-256은 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`다. 둘 다 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`는 `0.42.14-admin-smoke` product payload package evidence이며 04218 full gate 후속에서도 유지된 selector guard record다. `BatchEvidenceSummaryReader`가 `manual-admin-campaign-descriptor-*` batch를 Web Console current-card의 operational latest 후보에서 제외하도록 보강했고, `artifacts/installed-current-card-20260515-04218-fullgate`가 `full-admin-host-mutation-gate-20260515-163107-04218`를 latest로 표시했다. Package MSI SHA-256은 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`, provenance commit은 `a28bb808386f206c9dbf7dcaeee232eacb648434`다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`는 04211→04212 clean-host Windows Update reboot 중 관찰된 heartbeat `NoContact` + CPU idle 수동 recovery를 runner contract로 승격한 code-level evidence다. 다음 clean-host manual-admin run은 `WindowsUpdateNoContactRecoverySeconds`, VM 상태 snapshot, `automatic_recovery_performed`, `recovery_actions`를 summary에 남긴다. 이 evidence 자체는 host mutation을 실행하지 않았고 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`는 사용자 승인 `1-2-3-4-5` 이후 `0.42.13-admin-smoke` package build/full admin host mutation/package-pair campaign 실행 여부를 판정한 triage evidence다. `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이 없어 다음 package build와 host mutation campaign을 열지 않았다. Clean-host recovery summary key는 다음 실제 run의 `recovery_actions`와 `automatic_recovery_performed`로 판정한다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`는 `0.42.10-admin-smoke` duplicate outer start RCA를 historical-only로 닫는다. Target MSI SHA-256은 `bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273`, update ZIP SHA-256은 `05a107f4803ec8ed1e08f7aeba1b49fa3795c7d16565db8f904fd599ba07633f`, provenance commit은 `d7d5ba38ee1d4f74676477eb13701af65abca008`다. Native service-action repair가 service를 `Running`으로 만든 뒤 outer wrapper duplicate `sc.exe start`가 `1056 already running`을 반환했고, `0.42.11-admin-smoke`가 `native-service-action-controls-final-state` outer start skip으로 닫았다. 이 record는 current package-pair/full gate claim이 아니며 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`는 ADR-0004 내부 전용 제품 런타임 판단 이후 이전 full admin host mutation gate evidence다. 이 evidence는 `0.42.9-admin-smoke`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`를 근거로 한다. Admin-smoke provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V, firewall/LAN/Event Log/internal trust-store, Web Console `http://127.0.0.1/`, `/pcv-config.js`, `PCV_AUTH_REQUIRED` boundary와 installed listener `batch_evidence.status=available` current-card smoke를 함께 확인했다. `0.42.8-admin-smoke` full gate, `0.42.7-admin-smoke` full gate, `0.42.3-admin-smoke` full gate, `0.42.2-admin-smoke` full gate, `0.41.5-admin-smoke` full gate, `0.41.2-admin-smoke` full gate, `0.41.0-admin-smoke` account-linked full gate, `0.39.1-admin-smoke` full gate와 `0.38.7-rc.1` signed build evidence는 historical evidence로 보존한다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`는 `0.41.5-admin-smoke` 이전 full admin host mutation gate historical evidence다. 이 evidence는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`를 근거로 하고, MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`는 `0.39.0-admin-smoke` installed listener 후속 focused OS mutation gate evidence다. Artifact는 `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`이고 firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore를 확인했다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`이며 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/host-mutation-signed-build-attempt-2026-05-07-0387.md`는 최신 internal signed build evidence다. `0.38.7-rc.1` InternalEnterprise `RequireSigned` MSI artifact는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`, MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 같은 문서는 non-elevated `0.38.7-admin-smoke` host mutation/update rollback attempts가 blocked history일 뿐 PASS evidence가 아님도 분리 기록한다.
- `docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md`는 `0.38.8-admin-smoke` installed destructive update/rollback PASS evidence다. Elevated artifact root는 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`이고 MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`다. Update `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`, rollback restore to `0.38.6-admin-smoke`, final service `Running`, boot time unchanged, `DesktopNode.failed` diagnostics root 보존, `host_mutation_performed=true`를 확인한다. 이 evidence는 `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`는 ADR-0002 installer artifact/channel contract의 후속 sidecar evidence다. Installer build는 `PureCVisorDesktopNode-<version>-windows-x64.publication.json`을 작성하고 artifact SHA/provenance와 publication boundary를 연결한다. 이 descriptor는 `internal-artifact-descriptor-only`이며 public trusted signing, 외부 stable publication, Burn/MSIX/winget publication을 주장하지 않는다.
- `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`는 `0.39.1-admin-smoke` MSI/update package apply PASS evidence다. Artifact root는 `artifacts/msi-update-package-20260509-0391`이고 MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, signing mode는 `AllowUnsignedDev`다. Elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 확인한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`는 Web Console `http://127.0.0.1/`와 Web API `http://127.0.0.1:7777/api/v1/...` 분리 기본값을 code-level로 추적한다. 후속 `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`는 `artifacts/installed-port-split-20260510-010714-0392`에서 설치본 service `PathName`의 `--web-prefix "http://127.0.0.1:80/"`, Web `200`, API `200`, Web-port API `PCV_API_ROUTE_ON_WEB_PORT`, CORS preflight `204`를 PASS로 기록한다. HTTPS/443 binding, public trusted signing, 외부 stable publication은 주장하지 않는다.
- `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`는 Windows-local account/RBAC/JWT route, Web Console session UX, service binary path의 `--account-file`/`--jwt-signing-key-file`, no-default-account bootstrap, Hyper-V `vmconnect` handoff, noVNC 기본 disabled 경계를 code-level로 추적한다.
- `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`는 installed account login smoke PASS와 opt-in noVNC WebSocket-to-VNC TCP bridge code-level PASS를 추적한다. Installed smoke artifact는 `artifacts/installed-account-login-smoke-20260510-0410-final`이다. `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`는 target-backed noVNC installed streaming PASS와 installed TUI operator smoke PASS를 추적한다. noVNC bridge는 explicit target host/port 구성 전까지 disabled다. Public trusted signing, 외부 stable publication은 주장하지 않는다.
- `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`는 installed Web Console real account login form, auth/session/RBAC/console route contract, diagnostic create/download, responsive browser QA를 installed listener에서 PASS로 확인한다. Artifact는 `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`, `artifacts/installed-web-asset-refresh-20260510-235258`이다. Token/password 값은 관측하지 않았고 public trusted signing, 외부 stable publication은 주장하지 않는다.
- `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`는 `0.41.5-admin-smoke` installed Operator Access와 hardening follow-up evidence다. `artifacts/manual-admin-followup-20260510-0415`에서 installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition을 PASS로 재확인했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. Public trusted signing, 외부 stable publication은 주장하지 않는다.
- `docs/ga-ready/evidence/runtime-host-hyperv-domain-followup-code-level-2026-05-12.md`는 Stabilize Then Split 후속 code-level 경계 evidence다. Runtime/Core request processor는 auth/session, jobs, diagnostics handler를 거쳐 dispatch하고, Host Ops는 config migration, job store migration, service token을 독립 Ops owner로 분리했으며, Hyper-V는 WMI provider catalog로 domain provider boundary와 implementation type을 연결했다. 이 evidence는 host mutation, installed listener rerun, MSI apply, public trusted signing, 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/runtime-hyperv-operator-followup-code-level-2026-05-12.md`는 후속 1-2-3-4-5 code-level evidence다. Runtime/Core console/ops-summary dispatch, Hyper-V WMI provider 파일 경계, 다음 manual-admin descriptor, P1 historical evidence 한국어 재작성을 연결하며 host mutation rerun은 새 package input 확정 전에는 필요하지 않다고 기록한다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`는 `0.42.3-admin-smoke` baseline과 `0.42.4-admin-smoke` target package의 MANUAL-ADMIN historical blocker evidence다. Full admin host mutation, Operator Access, Internal Service Hardening, installed update/rollback은 PASS였지만 dedicated clean-host package-pair는 baseline MSI custom action sequence blocker로 보류했다. Current package-pair claim은 0425→0426 PASS evidence가 소유한다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`는 `0.42.5-admin-smoke` baseline과 `0.42.6-admin-smoke` target package의 MANUAL-ADMIN package-pair PASS evidence다. Installed update/rollback, dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture가 PASS다. `New-PcvManualAdminCampaignDescriptor.ps1`는 이미 실행된 runner evidence를 `overall_status=pass` descriptor로 묶고, `New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`는 descriptor manifest `manual-admin-campaign-descriptor-20260512-0425-0426`을 생성했다. Post-merge rebuild는 `docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md`에서 MSI SHA-256 `9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16`, provenance commit `37f4d6b83d6caef1338e0a60e5df0a60209b51f8`로 보존한다. 사용자 승인 후 `0.42.7-admin-smoke` package build/full admin host mutation gate/installed listener current-card smoke를 실행했으며, 현재는 0427→0428 package-pair와 0428 full gate가 이 evidence를 historical predecessor로 낮춘다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`는 `0.42.7-admin-smoke` baseline과 `0.42.8-admin-smoke` target package의 이전 닫힌 MANUAL-ADMIN package-pair PASS evidence다. Installed update/rollback, dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation이 PASS다. Target post-merge MSI SHA-256은 `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, update ZIP SHA-256은 `f8bb7900687c1a19eafc57266adbd388c826b15b4926808beac8ac0e79871ccc`, provenance commit은 `5397e580c98a34e8b7beb5b9773d1d857025315b`다. 후속 `0.42.9-admin-smoke`, `0.42.11-admin-smoke`, `0.42.12-admin-smoke` full admin host mutation gate와 installed listener current-card smoke도 PASS였고, 최신 full gate claim은 후속 04212 evidence가 소유한다. `0.42.8-admin-smoke -> 0.42.9-admin-smoke` candidate는 installed update/rollback까지만 PASS다. Public trusted signing 또는 외부 stable publication을 주장하지 않는다.

## 제안 중인 ADR 후보

현재 제안 중인 ADR 후보는 없다. `ADR-0015`는 2026-08-23 채택돼 위 적용 결정 표가 소유한다.
2026-08-03 번호 감사에서 `ADR-0013`은 이미 적용 중인
`docs/adr/0013-job-store-single-writer-transaction-lease.md`가 소유하는 것을 재확인했다. Wave 6
ASP.NET Core server/rollout 결정은 Wave 5A와 ADR-0012 concurrency policy가 종결된 뒤
`ADR-0014`로 추가한다. 아직 적용 전인 후보 문서는 생성하지 않았으며, 현재는
`docs/ga-ready/evidence/csharp-architecture-wave5a-admission-lifetime-code-slice-2026-08-03.md`와
계획 문서의 번호 예약만 유효하다.

## 종료된 ADR 후보

| ADR | 상태 | 후보 결정 | 관련 문서 |
|-----|------|-----------|-----------|
| `docs/adr/0005-public-distribution-operations-expansion-candidate.md` | 미채택/종료 | ADR-0005 `closed-not-adopted`: public trusted signing, external stable publication, winget public submission, public clean-host smoke 후보를 역사 기록으로 보존. 현재 적용 배포 경계는 ADR-0006 internal-private-network-only | `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`, `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`, `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionDescriptor.ps1`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`, `packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1`, `packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1`, `packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1`, `packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1`, `packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1` |

ADR-0005 public distribution ops execution bundle follow-up은 `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`와 `packaging/windows-desktop-node/tools/New-PcvPublicDistributionOperationsBundle.ps1`가 추적한다. 이 slice는 descriptor/readiness/Burn/MSIX/winget/catalog/public-signed-rollback/Credential Manager/Event Log/TLS/service-token/timeout/diagnostic preflight generators를 local non-mutating bundle로 실행/수집해 `public_distribution_ops_execution_bundle: code-level-nonmutating-bundle-pass`, `actual_execution: local-preflight-bundle-executed`, `host_mutation_performed: false`, public trusted signing/external stable publication `not-claimed`를 기록한다. 이 evidence는 ADR-0005를 적용 결정으로 바꾸지 않는다.

ADR-0006 internal private network distribution decision은 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`가 추적한다. Public trusted signing, timestamp evidence, external stable publication/catalog upload, winget submission, public stable installer URL, clean-host public signed install/update/rollback smoke는 `out-of-scope`이며, internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke가 현재 적용 gate다. `internal-https-tls-lifecycle-installed-2026-05-10-0397`는 installed HTTPS certificate generate/bind/rotate/remove PASS이고, `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417`는 dedicated Hyper-V clean-host install/update/rollback PASS다.

ADR-0005 MSIX package lifecycle history는 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`와 `artifacts/msix-package-lifecycle-smoke-20260510-0416`에 보존한다. `PureCVisor.DesktopNode.MsixSmoke` package identity는 `0.41.5.0` install, `0.41.6.0` update, remove까지 `build-install-update-remove-pass-internal-smoke`로 확인했다. 현재 matrix에서는 internal package lifecycle smoke로 유지하며 public publication은 ADR-0006 기준 out-of-scope다.

ADR-0005 winget CLI validate follow-up은 `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`가 추적한다. 이 slice는 readiness preflight가 생성한 singleton manifest preview에 winget schema header를 추가하고 실제 `winget validate --manifest`를 실행해 `winget_validation_status: winget-cli-validate-pass`를 기록했다. `winget_submission: not-submitted`이며 public trusted signing/external stable publication은 주장하지 않는다.

ADR-0005 public external gates blocked scan은 `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`가 추적한다. 이 scan은 SignTool x64, winget CLI, GitHub CLI auth가 로컬에 있음을 확인했지만 public signing material, timestamp URL, external catalog/package upload endpoint and credentials, public stable installer URL, public clean-host publication input이 없어 `timestamp_evidence: blocked-by-missing-public-signing-cert-and-timestamp-url`, `external_stable_publication: blocked-by-missing-upload-endpoint-and-credentials`, `catalog_publication: not-uploaded`, `winget_submission: blocked-by-no-public-signed-stable-installer-and-public-url`, `clean_host_public_signed_install_update_rollback_smoke: blocked-by-public-signing-publication-and-clean-host`를 기록한다. 이 evidence는 ADR-0005를 적용 결정으로 바꾸지 않으며 ADR-0006 internal clean-host PASS와 별개다.

ADR-0005 public ops final follow-up attempt는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`와 `packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1`가 추적한다. 이 slice는 1-7 final public operations follow-up prerequisite scan을 `artifacts/public-ops-final-followup-attempt-20260509-0391`에 기록하고 `remaining_follow_up_count: 7`, `actual_execution: local-final-followup-prerequisite-scan-executed`, `host_mutation_performed: false`, `public_release: not-claimed`를 유지한다. Public trusted signing/external stable publication은 계속 주장하지 않는다.

ADR-0005 public ops gate execution readiness는 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`와 `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`가 추적한다. 이 slice는 6개 잔여 gate를 `artifacts/public-ops-gate-execution-readiness-20260509-0392`에 기록하고 external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback blocker를 보존한다. TLS는 `partial-code-level-cert-generate-rotate-delete-pass`, `tls_private_key_material_written=false`, `tls_binding=not-run`, `host_mutation_performed=false`로 code-level readiness를 닫았고, Credential Manager SYSTEM proof blocker는 후속 installed evidence로 닫혔다. Public trusted signing/external stable publication은 계속 주장하지 않는다.

ADR-0005 public ops installed hardening follow-up은 `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`가 추적한다. Native `credential-manager-system-proof`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard` code-level PASS 이후 Credential Manager installed LocalSystem default transition, Event Log installed default writer/repair/remove/volume/schema smoke, internal HTTPS binding/trust boundary smoke가 PASS로 닫혔다. Matrix 상태는 `service_credential_manager_default_transition: installed-admin-smoke-pass`, `event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass`, `event_log_default_writer: installed-admin-smoke-pass`, `internal_https_tls_lifecycle_installed_smoke: pass`다. Public trusted signing/external stable publication은 주장하지 않는다.

ADR-0005 Burn bootstrapper lifecycle follow-up은 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`가 추적한다. 이 slice는 WiX Burn bundle을 실제 build하고 install/repair/remove를 모두 exit `0`으로 확인한 뒤 direct MSI restore로 final service `Running`을 복구했다. Matrix 상태는 `burn_bootstrapper: build-install-repair-remove-pass-internal-smoke`이며 public trusted signing, timestamp evidence, external stable publication, winget submission, clean-host public signed update/rollback은 주장하지 않는다.

ADR-0005 Windows Credential Manager transition follow-up은 `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`가 추적한다. Current-user Advapi32 capability smoke는 선행 PASS였고, 최신 installed smoke는 MSI deferred LocalSystem custom action으로 `credential_manager_transition: installed-local-system-default-transition-pass`, `service_credential_manager_default_transition: installed-admin-smoke-pass`, `token_source_migration: protected-file-to-credential-manager`, `service_reload_status: restarted`, `old_source_rejection_status: protected-file-source-rejected-after-reload`, `rollback_diagnostics_status: written`, `token_value_observed: false`를 기록한다.

ADR-0005 Windows Event Log provider/default writer follow-up은 `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`가 추적한다. Installed native `eventlog-register` corrected rerun은 provider registration과 event id `39100` write/query를 PASS로 확인했고, latest installed smoke는 MSI deferred LocalSystem `eventlog-default-transition`으로 provider repair, event id `39101` schema v1 write/query, volume guard, provider remove/restore, final service Event Log writer args를 PASS로 확인했다. Matrix 상태는 `event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass`다.

ADR-0005 diagnostic bundle server code-level follow-up은 `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`가 추적한다. 이 slice는 `POST /api/v1/diagnostics/bundles`와 `GET /api/v1/diagnostics/bundles/{bundle_id}/download`를 code-level Local API action으로 적용하고, product service plan에 `--diagnostics-root`를 연결한다. Matrix 상태는 `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`다. Installed listener execution, product wrapper diagnostics delegation, host mutation, public trusted signing, external stable publication은 주장하지 않는다.

ADR-0005 diagnostic bundle list pagination/retention follow-up은 `docs/ga-ready/evidence/diagnostic-bundle-list-pagination-retention-2026-05-09.md`가 추적한다. 이 slice는 `GET /api/v1/diagnostics/bundles?limit=&offset=` read-only route와 Web Console retained bundle list/`Load more bundles` UX를 적용한다. Matrix 상태는 `diagnostic_bundle_list_pagination_retention: code-level-applied`, `diagnostic_bundle_web_list_pagination_ux: code-level-applied`이며 host mutation, public trusted signing, external stable publication은 주장하지 않는다.

ADR-0005 service token rotation/revoke installed follow-up은 `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`가 추적한다. 이 slice는 `DesktopNode.Host.exe service-action service-token-rotation-revoke`가 DPAPI protected token file backup/write/atomic replace, service restart, old bearer rejection, new bearer acceptance, redacted audit write를 실제 installed service에서 확인했다. Matrix 상태는 `service_token_rotation_revoke: installed-admin-smoke-pass`, `service_token_mutation: performed`, `token_value_observed: false`, `new_token_value_created: true`, `service_reload_status: restarted`, `old_token_rejection_status: old-token-rejected-after-reload`, `token_rotation_audit_status: written`이다. Public trusted signing과 external stable publication은 주장하지 않는다.

ADR-0005 diagnostic bundle Host listener code-level follow-up은 `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`가 추적한다. 이 slice는 in-process `DesktopNodeHostApplication` listener에서 bearer-required create/download와 `X-PCV-Request-Id` propagation을 확인한다. Matrix 상태는 `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`다. 이 code-level slice 자체는 installed service listener execution, host mutation, public trusted signing, external stable publication을 주장하지 않는다.

ADR-0005 diagnostic bundle product wrapper code-level follow-up은 `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`가 추적한다. 이 slice는 `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`가 `New-PcvDesktopNodeDiagnosticBundle`로 위임되고 `product-wrapper-delegation-redacted.json`을 기록하는 것을 확인한다. Matrix 상태는 `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `actual_execution: code-level-product-wrapper`, `host_mutation_performed: false`다. Installed service listener PASS는 별도 `0.39.0-admin-smoke` rerun evidence가 소유한다.

ADR-0005 diagnostic bundle native service-action config code-level follow-up은 `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`가 추적한다. 이 slice는 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config가 `--diagnostics-root`, protected token file, route timeout, request limit, burst, retry-after 인자를 `DesktopNodeWindowsServiceConfiguration.BinaryPathName`에 포함하도록 보강한다. 0.38.9 installed final `PathName`은 아직 이 인자들을 포함하지 않았지만, `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`의 `0.39.0-admin-smoke` elevated MSI/service rerun이 `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`에서 installed listener execution을 `installed-listener-pass`, blocker `none`으로 닫았다. MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, signing mode는 `AllowUnsignedDev`다.

ADR-0005 timeout/rate-limit hardening follow-up은 `packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1`와 `docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md`가 추적한다. 이 후보는 `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false` 상태이며 public trusted signing/external stable publication을 주장하지 않는다.

ADR-0005 timeout/rate-limit hardening code-level follow-up은 `docs/ga-ready/evidence/timeout-rate-limit-hardening-code-level-2026-05-08.md`가 추적한다. 이 slice는 `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`이지만 `route_timeout_policy: not-applied`, `load_test_status: not-run`, `server_config_mutation: not-run` 상태이며 public trusted signing/external stable publication을 주장하지 않는다.

ADR-0005 timeout/rate-limit route-timeout code-level follow-up은 `docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md`가 추적한다. 이 slice는 `timeout_rate_limit_hardening: partial-code-level-route-and-request-limit`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`이지만 `load_test_status: not-run`, `server_config_mutation: not-run` 상태이며 public trusted signing/external stable publication을 주장하지 않는다.

ADR-0005 timeout/rate-limit server-config code-level follow-up은 `docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md`와 `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`가 추적한다. 이 slice는 product service plan과 native service-action config에 hardening 기본 인자를 연결해 `timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `server_config_mutation: code-level-product-and-native-service-plan-applied` 상태지만 `load_test_status: not-run`이고 installed service mutation, host mutation, public trusted signing/external stable publication을 주장하지 않는다.

ADR-0005 timeout/rate-limit load-test code-level follow-up은 `docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md`가 추적한다. 이 slice는 `DesktopNodeApiRequestProcessor` in-process 경로에서 같은 client의 `/api/v1/runtime/policy` 요청 64개를 병렬 실행해 HTTP 200 20건, HTTP 429 44건, unexpected status 0건과 `PCV_RATE_LIMIT_EXCEEDED` problem-details contract를 확인한다. Matrix 상태는 `timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: code-level-inprocess-pass`, `server_config_mutation: code-level-product-and-native-service-plan-applied`다. Installed listener load, external load generator, installed service config mutation, host mutation, public trusted signing/external stable publication은 주장하지 않는다.

ADR-0005 installed listener external load/rate-limit follow-up은 `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`가 추적한다. 이 slice는 설치된 listener에 실제 HTTP 요청 180개를 보내 HTTP 200 140건, HTTP 429 40건, unexpected status 0건, 모든 429의 `Retry-After`와 `PCV_RATE_LIMIT_EXCEEDED` problem-details contract를 확인했다. Matrix 상태는 `installed_listener_external_load_rate_limit: pass`, `installed_listener_external_rate_limit_contract: retry-after-problem-details-pass`다. Host mutation, public trusted signing, external stable publication은 주장하지 않는다.

## 결정 마커

```text
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper
DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper
DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first
DESKTOP_NODE_PHASE15_TOKEN_STORAGE_DECISION: dpapi-local-machine-protected-file-first
DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred
DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required
DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike
DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned
DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
DESKTOP_NODE_PHASE25_TYPESCRIPT_WEB_CONSOLE_BOUNDARY_CANDIDATE: static-asset-parity-scaffold-first
DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT: dotnet-windows-service-host-default-with-keep-spike
DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start
DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_READ_PARITY_GUARD: network-inventory-vm-list-vm-detail-and-checkpoint-list-native-structured-failure-on-incomplete-parity
DESKTOP_NODE_PHASE25_NATIVE_VM_POWER_STATE_MUTATION_START: vm-start-poweroff-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_CHECKPOINT_MUTATION_START: checkpoint-create-restore-delete-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_VM_LIFECYCLE_MUTATION_START: vm-create-shutdown-restart-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_VM_DELETE_MUTATION_START: vm-delete-dotnet-native-adapter
DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime
DESKTOP_NODE_PUBLIC_DISTRIBUTION_DECISION_CANDIDATE: closed-not-adopted
DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only
```

## ADR 작성 규칙

- 새 설계 결정이 Desktop Node 공개 경계, 제품 승격 gate, installer/service/update/security policy를 바꾸면 ADR을 추가하거나 기존 ADR을 supersede한다.
- Phase spec은 상세 설계와 구현 계획을 담고, ADR은 현재 적용 결정과 영향 범위를 담는다.
- ADR 상태는 `제안`, `적용 중`, `대체됨`, `폐기됨` 중 하나를 사용한다.
- ADR 변경 후에는 component/archive root documentation guard와 `git diff --check`를 실행한다.

## 관련 진입점

- `AGENTS.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy-design.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md`
- `docs/adr/0003-internal-trusted-signing-policy.md`
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md` (ADR 미채택 후보)
- `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md` (ADR 미채택 후보)
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md` (ADR 미채택 후보)
- `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md` (ADR 미채택 후보)
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md` (ADR 미채택 후보)
- `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-dotnet-native-network-inventory-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-list-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-detail-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-list-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-mutation-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-power-state-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-create-shutdown-restart-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-delete-native-adapter.md` (ADR 미채택 구현 slice)
- `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md` (ADR-0004 적용 결정 근거)
- `docs/adr/0004-ga-ready-product-runtime-candidate.md` (적용 중, 내부 전용 서비스 제품 런타임 결정)
- `docs/adr/0005-public-distribution-operations-expansion-candidate.md` (ADR-0005 미채택/종료, public distribution candidate history)
- `docs/adr/0006-internal-private-network-distribution.md` (적용 중, 내부 사설망 전용 배포 결정)
- `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` (GA-ready current route matrix)
- `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` (ADR-0005 보존용 closed-not-adopted matrix. Public signing/winget/external upload/public clean-host smoke는 out-of-scope이며 historical rows keep `timestamp_evidence: blocked-by-missing-public-signing-cert-and-timestamp-url`, `historical_public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication`, `historical_clean_host_smoke_status: not-run`)
- `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md` (ADR-0006 현재 적용 matrix. Internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke 중심)
- `docs/ga-ready/REPO_MIGRATION_MAP.md` (GA-ready current repo migration map)
- `docs/ga-ready/VERIFICATION_OWNERSHIP.md` (GA-ready current verification ownership map)
