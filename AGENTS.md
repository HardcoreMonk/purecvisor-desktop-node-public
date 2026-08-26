# AGENTS.md

<!-- BEGIN GENERATED CURRENT EVIDENCE -->
## Current operational evidence (generated)

- Version: `0.42.74-admin-smoke`
- Active operator surfaces: Web Console and PCVCLI; `tui_present=false`.
- Package evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md`.
- Full admin host mutation: `full-admin-host-mutation-gate-20260820-04274` / `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md`.
- Actual-VM functional evidence: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md`.
- Feature qualification: `contract=pcv-feature-promotion-decision-v1`; `promotion_eligible=false`; `blocker_count=1`; `blockers=pcv.vm.saved-lifecycle/actual_vm_tested/fail`.
- Installed CLI/Web current-card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md`; CLI exit 0, Web HTTP 200, service Running/Automatic, TUI absent.
- Clean MSI SHA-256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`.
- Operational MSI SHA-256: `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`.
- Operational payload aggregate SHA-256: `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`.
- Provenance commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`.
- Latest closed manual-admin pair: `0.42.73-admin-smoke -> 0.42.74-admin-smoke` / `manual-admin-campaign-descriptor-20260820-04273-04274-closed`.
- Claims: `public_trusted_signing=false`; `external_stable_publication=false`.
<!-- END GENERATED CURRENT EVIDENCE -->

## 에이전트 실행 회로 차단기 (필수)

- 단일 진실: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
- 기계 계약: `config/agent-execution-circuit-breaker.json`
- `vague_resume_policy`: `one-bounded-checkpoint`
- `out_of_scope_findings`: `report-only`
- 기본 한도는 30분, 도구 작업 묶음 18회, 정규 리뷰 1회와 제한 재검토 2회다.
- 먼저 도달한 한도 또는 동일 원인 3회 실패 시 추가 구현을 중단하고 stop protocol만 수행한다.
- 사용자의 명시적 승인 없이는 예산, 범위 또는 checkpoint를 연장하지 않는다.

## 2026-07-13 historical TUI predecessor

당시 operational full admin host mutation anchor는 `0.42.62-admin-smoke` /
`full-admin-host-mutation-gate-20260713-04262`다. Package build evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md`, full admin host mutation
evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04262-hostmutation.md`, 설치본
Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04262.md` /
`artifacts/installed-operator-surface-current-card-20260713-04262/summary.json`에서 PASS했다.
Clean MSI SHA-256은 `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`,
operational full-gate MSI SHA-256은
`c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`, operational payload
aggregate SHA-256은 `ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`,
provenance commit은 `7f71f0a518c5b592f233373522d36b5401c3f1df`다.

WMI internal switch topology recovery와 `0.42.60`/`0.42.61` 실패 경계는
`docs/ga-ready/evidence/wmi-internal-switch-topology-recovery-2026-07-13-04260-04262.md`가
소유한다. 두 predecessor는 package 설치/MSI lifecycle은 PASS했지만 첫 route 단계에서 각각
`PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`, `PCV_NETWORK_INVENTORY_FAILED`로
full gate가 실패했고 OS mutation은 실행되지 않았으므로 PASS anchor가 아니다. 04262
current-card는 `Default Switch`와 `WSL (Hyper-V firewall)`을 모두 `internal` /
`allow_management_os=true`로 확인했다.

최신 closed manual-admin package-pair는 별도 campaign이 실행되지 않았으므로 계속
`0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. 04262 evidence는
`AllowUnsignedDev`/`LocalTest` internal admin-smoke 범위이며 public trusted signing 또는 외부
stable publication evidence가 아니다. 아래 2026-05-29 이하 문단은 historical predecessor로
해석한다.

## 2026-05-29 현재 기준

최신 operational full admin host mutation anchor는 `0.42.59-admin-smoke` /
`full-admin-host-mutation-gate-20260529-04259`다. Package build evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`, full admin host
mutation evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`가
현재 소유한다. 설치본 Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md` /
`artifacts/installed-operator-surface-current-card-20260529-04259/summary.json`에서 PASS했다.
Package MSI SHA-256은
`6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`, operational full-gate
MSI SHA-256은 `dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`,
payload aggregate SHA-256은 `3f015e7743efac3b61de81962c236a03c1bcf882053fc92fd3c525da280a1687`,
provenance commit은 `63d57feba605f82dabd44a96ed50a4d622f6310a`이다.

최신 closed manual-admin package-pair closure는 `0.42.58-admin-smoke ->
0.42.59-admin-smoke`이며
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`가 소유한다. Windows Update
clean-host, Burn, MSIX, installed update/rollback, runtime ops summary, descriptor
generation이 PASS했고 `missing_count=0`, `not_pass_count=0`이다.
직전 0.42.58 predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.
0.42.57 predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04257.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04257-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04256-04257.md`,
`manual-admin-campaign-descriptor-20260528-04256-04257-closed`로 보존한다.
0.42.56 predecessor는 `0.42.56-admin-smoke`,
`docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04256.md`,
`full-admin-host-mutation-gate-20260528-04256`,
`manual-admin-campaign-descriptor-20260528-04255-04256-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md`로
public-boundary follow-up의 기준 anchor를 보존한다.

`0.42.50-admin-smoke -> 0.42.54-admin-smoke` manual-admin readiness는 현재 host가 이미
0.42.55로 올라간 상태라 `blocked-by-installed-baseline-version-mismatch`로 닫지 않았다.
Guest Execution은 설치본 runtime policy/API/CLI queued provider route와 Web/TUI direct-control
surface까지 열렸고, 실제 Windows guest credentialed execution smoke는
`docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md`에서
PASS로 재확인했다.

Actual VM 기반 설치본 TUI row projection evidence는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`가
소유한다. 실제 VM `pcv-ux-qos-04241`로 `pcvtui --smoke-once vm` row projection과 cleanup을
PASS했고, 0.42.40 설치본 TUI row projection blocker는 0.42.41 package/fullgate/manual-admin
closure로 닫혔다. 최신 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`에서
run `26636072420`, job `78496568595`, head `5a2f91762a6c2a8ab6b84d334fa6cb420474671f`로
PASS했다. `0.42.60-admin-smoke` installed current-card payload 후보는 이미 열려 있으며,
docs-maintenance postpush는 추가 package 후보를 열지 않는다. account/noVNC는 0.42.58 PASS를 carry-forward하고 actual VM
Guest Execution/QoS smoke는 provider/control payload 변경 때 재실행한다. 0.42.57 public-boundary,
0.42.56 public-boundary, 0.42.54 fullgate public-boundary, 0.42.54 running cancel public-boundary, 0.42.53 provider public-boundary,
0.42.50 preview public-boundary, PR #169 public-boundary와
`docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`는
historical predecessor로 보존한다. PR #168/PR #167/PR #164/PR #163/PR #162/PR #160
public-boundary도 historical predecessor로 보존한다. 이 evidence는 internal admin-smoke
범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다. 아래
2026-05-21/05-18/05-19 기준 문단은 historical predecessor로 해석한다.

## 2026-05-21 historical predecessor

최신 operational full admin host mutation anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`다. Package build evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`, full admin host
mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`가
소유한다. 설치본 Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md` /
`artifacts/installed-operator-surface-current-card-20260521-04240/summary.json`에서
04240 기준 PASS로 재확인했다.
Operational full-gate MSI SHA-256은
`eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41`, payload aggregate
SHA-256은 `cd49f061dfd0e2e5afe45cd34befcfb28e02bbd9038eff1fbaef34f8c9616ea5`,
provenance commit은 `adb7b8c77ff60b64c5ac4d840e2bdfac62a3793a`이다.

최신 closed manual-admin package-pair closure는 `0.42.39-admin-smoke ->
0.42.40-admin-smoke`이며
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md` /
`manual-admin-campaign-descriptor-20260521-04239-04240-closed`가 소유한다.
Dedicated clean-host with Windows Update에서 `KB5087545`, UBR `5139`,
install/update/rollback PASS, Burn/MSIX lifecycle PASS, descriptor `missing_count=0`,
`not_pass_count=0`으로 closure 전환했다. 이전 0.42.37→0.42.38 clean-host `1603`
RCA는 historical predecessor로 보존한다.

설치본 PCVCLI QoS/guest targeted smoke는
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md` /
`artifacts/installed-cli-qos-guest-smoke-20260521-04239/summary.json`에서 PASS했다.
Web/TUI QoS/guest readback surface는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`에서
code-level PASS했다. Web/TUI direct QoS mutation/control은 계속 닫고 read-only
readback panel만 열었으며, 이 product payload 변경으로 `0.42.40-admin-smoke`
package chain은 manual-admin closure로 닫혔다.
Actual VM 기반 Web/TUI QoS/guest readback evidence는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`가
소유한다. Web no-overlap/readback은 PASS했고 설치본 TUI row projection blocker는 source
fix code-level PASS 후 `0.42.41-admin-smoke` package chain trigger로 분리했다.

Hyper-V pause lifecycle fast-follow인 `0.42.37-admin-smoke` evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04237.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md`,
`artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`에 historical
predecessor로 보존한다. 실제 VM `create/start/memory-stats/cpu-stats/pause/resume/rename/cleanup`,
Web/TUI/CLI current-card, PCVCLI neon palette, TUI runtime smoke가 PASS했다. PR #164 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md`에
historical predecessor로 보존한다. PR #167 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`에서
PASS했고, PR #164/PR #163/PR #162/PR #160 public-boundary는 historical predecessor로 보존한다. 이 evidence는
internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가
아니다. 아래 2026-05-18/05-19 기준 문단은 historical predecessor로 해석한다.

## 2026-05-18 현재 기준

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

Historical PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` package-pair predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` / `manual-admin-campaign-descriptor-20260517-04227-04228-closed`로 보존한다. Full admin host mutation batch는 `full-admin-host-mutation-gate-20260517-04228`이다. Target MSI SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, clean package MSI SHA-256은 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`, update ZIP SHA-256은 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`다.

PR #156 post-merge public-boundary main push는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`, run `26017721669`, job `76471545641`, head `a4509c552c003ee0fc87b54b26529686e6dfeb84`에서 PASS했고 historical public-boundary anchor로 보존한다. PR #155, PR #154, PR #152 public-boundary evidence도 historical predecessor로 보존한다. PR #153 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`, run `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`로 보존한다. 이후 사용자 승인으로 0.42.30 package chain을 열어 `full-admin-host-mutation-gate-20260518-04230`과 `manual-admin-campaign-descriptor-20260518-04229-04230-closed`를 current installed/package anchor로 승격했다.

이 저장소는 `purecvisor-desktop-node` 독립 Windows 저장소다.

## 저장소 경계

- 단일 진실: 이 저장소는 Windows Desktop Node 전용이다.
- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime 코드를 추가하지 않는다.
- 현재 코드 경로는 phase 이력과 테스트 계약을 보존하기 위해 `archive/spikes/purecvisor-desktop-node/**`와 `packaging/windows-desktop-node/**`를 유지한다.
- 현재 적용 결정은 `docs/ADR_INDEX.md`와 `docs/adr/`를 우선한다.
- 현재 operational full admin host mutation anchor는 `0.42.74-admin-smoke` /
  `full-admin-host-mutation-gate-20260820-04274`다. 설치본 운영자 표면은 Web Console과
  PCVCLI이며 `tui_present=false`다. 최신 closed manual-admin package-pair는
  `0.42.73-admin-smoke -> 0.42.74-admin-smoke` /
  `manual-admin-campaign-descriptor-20260820-04273-04274-closed`다. 정확한 hash와 evidence
  tuple은 이 파일 최상단 generated current-evidence 블록과
  `docs/ga-ready/current-evidence.json`이 소유한다. 이 evidence는 internal admin-smoke
  범위이고 Runtime/API current-card contract는 `runtime-api-current-evidence-rollup-v1`이다.
  Public trusted signing 또는 외부 stable publication evidence가 아니다.

## 문서 진입점

- 전체 문서 카탈로그: [docs/DOCUMENTATION_INDEX.md](docs/DOCUMENTATION_INDEX.md)
- 개발 문서 진입점: `docs/DEVELOPER_INDEX.md`
- 유저 가이드: `docs/USER_GUIDE.md`
- 유저 기능 사용 명세서: `docs/USER_FEATURE_USAGE_SPEC.md`
- 운영 가이드: `docs/OPERATIONS_GUIDE.md`
- 검증 기준: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- 저장소 경계: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- ADR 현재 적용 상태 인덱스: `docs/ADR_INDEX.md`
- 설계 결정 단일 진실: `docs/adr/`
- GA-ready 제어 평면 인덱스: `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- 증거 인덱스: `docs/ga-ready/EVIDENCE_INDEX.md`
- 한국어 문서 재작성 롤아웃: `docs/KOREAN_DOCUMENTATION_ROLLOUT.md`
- Phase roadmap: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Pester-free Web verification Wave B 설계/계획:
  `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b-design.md`,
  `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b.md`;
  Task 1~13 local-parity 구현 뒤 Wave C~E까지 완료했으며 현재 Required CI는 `dotnet`, `web`,
  `delivery`, `installer-policy` 네 shard다. Migration ledger는 62 files / 627 contracts 전체
  `cutover / local pass / CI pass`이고 Required CI의 Pester 및 비관리자 PowerShell process
  invocation은 각각 `0`이다. 비필수 public-boundary workflow와 legacy/manual/admin
  PowerShell은 별도 residue로 남는다.
- Phase 19 제품 승격 재판정: `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- Phase 22 release/version policy와 installer artifact contract: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md`
- 내부 신뢰 기반 signing policy: `docs/adr/0003-internal-trusted-signing-policy.md`
- Post-reboot verification dry-run/runner evidence: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-post-reboot-verification.md`, `packaging/windows-desktop-node/README.md`
- Phase 24 Local API job runtime 경계 후보: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`, `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md`
- Phase 25 .NET/TypeScript 전환 후보: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`, `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md`
- Phase 25 TypeScript Web Console parity 후보: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`
- Phase 25 Web Console browser fixture/root migration: `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-web-console-browser-fixture-parity.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-web-served-asset-root-migration.md`
- Web Console Network Inventory view evidence: `docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md`
- Web Console Diagnostic Bundle UI evidence: `docs/ga-ready/evidence/web-console-diagnostic-bundle-ui-2026-05-07.md`
- Diagnostic Bundle list pagination/retention evidence: `docs/ga-ready/evidence/diagnostic-bundle-list-pagination-retention-2026-05-09.md`
- API/Web retention pagination hardening evidence: `docs/ga-ready/evidence/api-web-retention-pagination-hardening-2026-05-07.md`
- Web Console Token Rotation UX evidence: `docs/ga-ready/evidence/web-console-token-rotation-ux-2026-05-07.md`
- Web Console beta follow-up status evidence: `docs/ga-ready/evidence/web-console-beta-followup-status-2026-05-09.md`
- Follow-up queue / automated batch job classification: `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`
- Web/API port split code-level/installed listener evidence: `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`, `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`
- Account/RBAC/JWT/console code-level evidence: `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`, `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-account-rbac-jwt-console.md`
- Installed account login/noVNC bridge follow-up evidence: `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`, `packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1`, `packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`, `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-installed-account-novnc-evidence.md`
- Frontend/backend auth console live smoke evidence: `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`, `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`; installed Web Console real account login form, auth/session/RBAC/console route, diagnostic create/download, responsive screenshot PASS
- Historical 2026-05-22 full admin host mutation evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-22-04241-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260522-04241`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260522-04241`, `artifacts/os-mutation-gates-batch-profile-20260522-04241`; `0.42.41-admin-smoke`, full-gate MSI SHA-256 `e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb`, payload aggregate SHA-256 `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`, provenance commit `2f41da1073df6e65113ae8ddaeb183e9b55874f4`, signing mode `AllowUnsignedDev`; installed current-card는 04241 기준 PASS. public trusted signing 또는 외부 stable publication evidence가 아님
- Previous full admin host mutation evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04222`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04222`, `artifacts/os-mutation-gates-batch-profile-20260516-04222`; full-gate MSI SHA-256 `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`, provenance commit `8a38995cc25a888f64473e9a2869740949ad6b24`; historical predecessor로 보존한다.
- Historical 04220 full admin host mutation evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`; full-gate MSI SHA-256 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`; public trusted signing 또는 외부 stable publication evidence가 아님
- Historical 2026-05-22 product payload package build record: `docs/ga-ready/evidence/admin-smoke-package-2026-05-22-04241.md`, `artifacts/admin-smoke-package-20260522-04241`; `0.42.41-admin-smoke`, clean MSI SHA-256 `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`, payload aggregate SHA-256 `21aeb02757495d8296151ce20dda987ef36fcb2f3320f5163131ffc90e65c361`, provenance commit `2f41da1073df6e65113ae8ddaeb183e9b55874f4`. 0.42.41은 설치본 TUI row projection fix package이며 full admin host mutation gate까지 PASS했다.
- Historical 2026-05-22 closed manual-admin package-pair evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-22-04240-04241.md`, `artifacts/manual-admin-campaign-20260522-04240-04241/manual-admin-campaign-descriptor-r2-windows-update/summary.json`; `0.42.40-admin-smoke -> 0.42.41-admin-smoke`, target MSI SHA-256 `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`, update ZIP SHA-256 `9ab7e266c093b98982aa854c19f901a6bb133f51c66904b9bfcdf56d538fee73`, provenance commit `2f41da1073df6e65113ae8ddaeb183e9b55874f4`, descriptor `manual-admin-campaign-descriptor-20260522-04240-04241-closed`, `missing_count=0`, `not_pass_count=0`; public trusted signing 또는 외부 stable publication evidence가 아님
- Previous closed manual-admin package-pair evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md`, `artifacts/manual-admin-campaign-20260520-04234-04235/manual-admin-campaign-descriptor/summary.json`; `0.42.34-admin-smoke -> 0.42.35-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260520-04234-04235-closed`; historical predecessor로 보존
- Previous `0.42.26-admin-smoke -> 0.42.27-admin-smoke` manual-admin package-pair PASS는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`, target MSI SHA-256 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`, update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`, provenance commit `69aba3eb3ff08c843f1a481818ddc86eac2f019b`, descriptor `manual-admin-campaign-descriptor-20260517-04226-04227-closed`, `missing_count=0`, `not_pass_count=0`으로 historical predecessor로 보존한다.
- Previous `0.42.25-admin-smoke -> 0.42.26-admin-smoke` manual-admin package-pair PASS는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`, target MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP SHA-256 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`, provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`, descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`, `missing_count=0`, `not_pass_count=0`으로 historical predecessor로 보존한다.
- Previous `0.42.24-admin-smoke -> 0.42.25-admin-smoke` manual-admin package-pair PASS는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`, target/full-gate MSI SHA-256 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, update ZIP SHA-256 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`, provenance commit `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`, descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`, `missing_count=0`, `not_pass_count=0`으로 historical closed package-pair evidence로 보존한다.
- Previous `0.42.24-admin-smoke` Runtime/API current evidence rollup package/fullgate/current-card evidence는 historical predecessor로 보존한다. Full-gate batch는 `full-admin-host-mutation-gate-20260516-04224`이고, package build MSI SHA-256은 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256은 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`다.
- Historical `0.42.22-admin-smoke -> 0.42.23-admin-smoke` package-pair PASS는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`로 보존한다. Target MSI SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, update ZIP SHA-256은 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`, provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`, descriptor는 `manual-admin-campaign-descriptor-20260516-04222-04223-closed`다.
- Post-04218 contract alignment evidence: `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`; `0.42.18-admin-smoke` 기준 Runtime/Core route/evidence bridge, Hyper-V dispatch catalog detail, Host Ops lifecycle bucket, packaging next trigger, Web Console/TUI/CLI operator journey, ADR-0005/0006 public boundary preservation; host mutation performed `false`, public trusted signing 또는 external stable publication evidence가 아님
- Post-04218 runtime/domain development slice evidence: `docs/ga-ready/evidence/post-04218-runtime-domain-slices-2026-05-15.md`; Runtime/Core route-family bridge, Hyper-V dispatch handler contract, Host Ops lifecycle bucket key, 0.42.19 next-candidate descriptor metadata, Web Console/TUI/CLI current-card journey를 code/test contract로 고정; host mutation performed `false`, public trusted signing 또는 external stable publication evidence가 아님
- Post-04218 follow-up execution evidence: `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`; Runtime route registry source를 `ApiHandlerAdapterContract`로 통합하고 Hyper-V dispatch를 `handler-registry-delegate-map`으로 고정하며 Host Ops family helper와 current-card snapshot parity를 test contract로 닫았다. `0.42.19-admin-smoke` package build artifact는 `artifacts/admin-smoke-package-20260515-04219`이고 `public-boundary-ci-required` guard를 추가했다. host mutation performed `false`, public trusted signing 또는 external stable publication evidence가 아님
- Historical manual-admin package-pair anchors: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`는 `0.42.11-admin-smoke -> 0.42.12-admin-smoke` PASS, target MSI SHA-256 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, update ZIP SHA-256 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`; `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`는 `0.42.12-admin-smoke -> 0.42.13-admin-smoke` PASS, target MSI SHA-256 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`; 둘 다 public trusted signing 또는 외부 stable publication evidence가 아님
- Clean-host Windows Update NoContact recovery guard evidence: `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`; `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`가 Windows Update reboot 이후 heartbeat `NoContact` + CPU idle 상태를 감지해 한 번만 VM power cycle recovery를 수행하고 `recovery_actions`를 남긴다. 이 evidence 자체는 code-level이며 host mutation을 실행하지 않음.
- Post-04212 follow-up triage evidence: `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`; `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이 없어 `0.42.13-admin-smoke` package build, full admin host mutation, `0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign을 열지 않았다. Clean-host recovery summary key는 다음 실제 run의 `recovery_actions`와 `automatic_recovery_performed`로 판정한다.
- Post-04212 `1-2-3-4-5` current-card follow-up evidence: `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`; `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 새 product payload 변경이 없어 `0.42.13-admin-smoke` package/host mutation chain은 보류했고, `artifacts/web-console-current-card-20260514-04212-rerun-followup`에서 Dashboard/Evidence current-card smoke만 PASS로 확인했다. 당시 표시 batch는 `full-admin-host-mutation-gate-20260514-04212-rerun`, version은 `0.42.12-admin-smoke`다. 이 evidence는 host mutation을 실행하지 않았고 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Historical manual-admin package-pair predecessor note: `0.42.38-admin-smoke -> 0.42.39-admin-smoke`와 `0.42.39-admin-smoke` full admin host mutation은 immediate predecessor로 보존한다. 현재 closed package-pair는 `0.42.39-admin-smoke -> 0.42.40-admin-smoke`이고 `0.42.37-admin-smoke -> 0.42.38-admin-smoke`는 historical predecessor다. 최초 0.42.37→0.42.38 clean-host `1603` blocker는 Windows Update 미적용 RCA로 보존한다. `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md` / `manual-admin-campaign-descriptor-20260517-04228-04229-closed`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` / `manual-admin-campaign-descriptor-20260517-04227-04228-closed`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` / `manual-admin-campaign-descriptor-20260517-04226-04227-closed`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md` / `manual-admin-campaign-descriptor-20260517-04225-04226-closed`는 historical predecessor로 보존한다. `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md` / `manual-admin-campaign-descriptor-20260516-04225-04226`는 historical initial `blocked-by-missing-evidence` candidate로 보존한다. `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04223-04224.md` / `manual-admin-campaign-descriptor-20260516-04223-04224`는 historical `blocked-by-missing-evidence` candidate로 보존한다. `0.42.21-admin-smoke -> 0.42.22-admin-smoke` Burn blocker, `0.42.17-admin-smoke` clean-host failure, `0.42.10-admin-smoke` duplicate outer start RCA `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`는 `0.42.11-admin-smoke` `native-service-action-controls-final-state` closure와 함께 historical RCA로만 보존한다.
- Previous full admin host mutation evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`; `0.42.9-admin-smoke`
- Historical full admin host mutation evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423`; `0.42.3-admin-smoke`
- Historical full admin host mutation evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-11-0422-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422`; `0.42.2-admin-smoke`
- Latest manual-admin operator/hardening follow-up evidence: `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `artifacts/manual-admin-followup-20260510-0415`; installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition, and Lifecycle/Packaging current 0.41.5 to 0.41.6 rebaseline PASS
- Lifecycle/Packaging current rebaseline evidence: `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`; package pair `0.41.5-admin-smoke` to `0.41.6-admin-smoke`, installed product update/rollback PASS, internal clean-host install/update/rollback PASS
- Product TUI service plan closure evidence: `docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md`, `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-product-tui-service.md`
- Historical .NET TUI predecessor (active 아님): `docs/adr/0011-cli-web-only-operator-surface.md`, `docs/superpowers/specs/2026-05-10-purecvisor-desktop-node-product-tui-service-design.md`, `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-product-tui-service.md`, `docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md`, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`
- Timeout/rate-limit hardening preflight evidence: `docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md`
- Timeout/rate-limit hardening code-level evidence: `docs/ga-ready/evidence/timeout-rate-limit-hardening-code-level-2026-05-08.md`
- Timeout/rate-limit route-timeout code-level evidence: `docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md`
- Timeout/rate-limit server config code-level evidence: `docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md`
- Timeout/rate-limit load test code-level evidence: `docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md`
- Diagnostic bundle server code-level evidence: `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`
- Diagnostic bundle Host listener code-level evidence: `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`
- Diagnostic bundle product wrapper code-level evidence: `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`
- Diagnostic bundle MSI/service installed listener evidence: `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`
- Diagnostic bundle installed listener OS mutation gate evidence: `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`
- Updater catalog/channel resolver evidence: `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`
- Update filesystem rollback evidence: `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`
- Packaging publication descriptor evidence: `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`
- ADR-0005 public distribution gate 종료/보존 기록: `docs/adr/0005-public-distribution-operations-expansion-candidate.md`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`, `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionDescriptor.ps1`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionOperationsBundle.ps1`, `packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1`, `packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1`
- ADR-0006 내부 사설망 전용 배포 결정: `docs/adr/0006-internal-private-network-distribution.md`, `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`, `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md`
- Internal clean-host install/update/rollback smoke evidence: `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, `artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417`, `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`
- Winget CLI validate evidence: `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`, `artifacts/winget-cli-validate-20260509-0391`
- Public external gates blocked evidence: `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`, `artifacts/public-external-gates-blocked-20260509-0391`
- Public ops final follow-up attempt evidence: `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `artifacts/public-ops-final-followup-attempt-20260509-0391`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1`
- Public ops gate execution readiness evidence: `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`, `artifacts/public-ops-gate-execution-readiness-20260509-0392`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`, TLS `partial-code-level-cert-generate-rotate-delete-pass`, public trusted signing/external stable publication not claimed
- Public ops installed hardening code-level evidence: `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `DesktopNode.Host.exe service-action credential-manager-system-proof|eventlog-repair|eventlog-write-test|eventlog-volume-guard`, public trusted signing/external stable publication not claimed
- Burn bootstrapper lifecycle smoke evidence: `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416`
- Windows Credential Manager transition evidence: `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `artifacts/windows-credential-manager-transition-20260509-0391`
- Windows Credential Manager default transition installed evidence: `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `artifacts/windows-credential-manager-default-transition-installed-20260510-0395`, installed LocalSystem proof/service token-source migration/reload/old-source rejection/rollback diagnostics PASS
- Windows Event Log provider default transition evidence: `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `artifacts/windows-event-log-provider-default-transition-20260509-0391`
- Windows Event Log default transition installed evidence: `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `artifacts/windows-event-log-default-transition-installed-20260510-0396`, installed default writer/provider repair/remove/volume/schema PASS
- Installed listener external load/rate-limit evidence: `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`, `artifacts/installed-listener-external-load-rate-limit-20260509-0391`
- Internal MSIX lifecycle smoke evidence: `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/msix-package-lifecycle-smoke-20260510-0416`
- MSI/update package apply evidence: `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`, `artifacts/msi-update-package-20260509-0391`
- Service token rotation/revoke installed evidence: `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`, `artifacts/service-token-rotation-revoke-installed-20260509-150334`
- .NET Windows Service Host replacement slice: `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`, `docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md`
- .NET native read/mutation adapter slices: `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-dotnet-native-network-inventory-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-list-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-detail-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-list-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-power-state-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-mutation-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-restore-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-create-shutdown-restart-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-delete-native-adapter.md`
- Service/data-root product ops follow-up: `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-dotnet-service-status-start-stop.md`, `docs/superpowers/plans/2026-05-04-purecvisor-desktop-node-data-root-remove-handoff.md`
- 제품 wrapper README: `packaging/windows-desktop-node/README.md`
- Installer README: `packaging/windows-desktop-node/installer/README.md`
- Desktop Node root README: `archive/spikes/purecvisor-desktop-node/README.md`
- Hyper-V helper README: `archive/spikes/purecvisor-desktop-node/hyperv/README.md`
- Local API README: `archive/spikes/purecvisor-desktop-node/api/README.md`
- Active .NET CLI usage: `docs/CLI_COMMAND_USAGE.md`, `src/DesktopNode.Cli/README.md`
- User feature usage spec: `docs/USER_FEATURE_USAGE_SPEC.md`
- Active .NET CLI plan: `docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-active-dotnet-cli.md`
- Archived CLI README: `archive/spikes/purecvisor-desktop-node/cli/README.md`
- Service README: `archive/spikes/purecvisor-desktop-node/service/README.md`

## 현재 핵심 결정

```text
DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike
DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT: dotnet-windows-service-host-default-with-keep-spike
DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start
DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_READ_PARITY_GUARD: network-inventory-vm-list-vm-detail-and-checkpoint-list-native-structured-failure-on-incomplete-parity
DESKTOP_NODE_PHASE25_NATIVE_CHECKPOINT_MUTATION_START: checkpoint-create-restore-delete-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_VM_LIFECYCLE_MUTATION_START: vm-create-shutdown-restart-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_VM_DELETE_MUTATION_START: vm-delete-dotnet-native-adapter
DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime
DESKTOP_NODE_PUBLIC_DISTRIBUTION_DECISION_CANDIDATE: closed-not-adopted
DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only
```

Phase별 상세 결정은 `docs/ADR_INDEX.md`, phase roadmap, 관련 phase spec을 따른다.
Desktop Node는 내부 전용 서비스로 확정됐고, public trusted signing과 외부 stable publication은 scope 밖이다. 내부 서비스 운영에서는 public CA가 없으면 ADR-0003의 internal root/leaf `RequireSigned` signing trust model을 따른다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
ADR-0005는 public distribution/운영 확장 제안 후보였지만 2026-05-10 ADR-0006으로 미채택/종료했다. `PUBLIC_DISTRIBUTION_GATE_MATRIX`는 보존용 closed-not-adopted matrix이며 public trusted signing/timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed install/update/rollback smoke는 `out-of-scope`다. 현재 적용 배포 기준은 `INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX`이며 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke, Lifecycle/Packaging current rebaseline을 중심으로 추적한다. Internal HTTPS/TLS lifecycle installed smoke는 `internal-https-tls-lifecycle-installed-2026-05-10-0397` PASS, internal clean-host install/update/rollback은 `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417` dedicated Hyper-V clean-host PASS, Lifecycle/Packaging current rebaseline은 `lifecycle-packaging-rebaseline-2026-05-10-0415-0416` PASS 상태다. 기존 public ops bundle, blocked scan, final follow-up attempt `remaining_follow_up_count: 7`, winget validate, Burn/MSIX, Credential Manager, Event Log, service token, diagnostic bundle, installed listener load/rate-limit evidence는 역사/내부 운영 evidence로 보존하고 public trusted signing/external stable publication은 주장하지 않는다.
`0.41.5-admin-smoke` to `0.41.6-admin-smoke` MSIX lifecycle smoke는 사용자 관리자 opt-in으로 `artifacts/msix-package-lifecycle-smoke-20260510-0416`에서 PASS했다. `PureCVisor.DesktopNode.MsixSmoke` package identity와 `PureCVisorDesktopNodeMsixSmoke` packaged service로 build/sign/verify, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence를 확인했다. 이 evidence는 internal Root/leaf signing과 restricted service capability smoke이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
`0.39.1-admin-smoke` MSI/update package apply는 사용자 관리자 opt-in으로 `artifacts/msi-update-package-20260509-0391`에서 PASS했다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, signing mode는 `AllowUnsignedDev`다. Elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
`0.39.1-admin-smoke` full admin host mutation gate는 사용자 관리자 opt-in으로 `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-032525-0391-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-032525-0391-rerun`, `artifacts/os-mutation-gates-batch-profile-20260509-032525-0391-rerun`에서 PASS했다. MSI provenance commit은 `0815a6281bcb98b5b1795e8d054073e1c9fb4892`, MSI SHA-256은 `25a88e41ed926a6bccaf3eba1fdd44d0976091aca9fd6ef77f52eea2bddf3c37`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, timeout false였고 Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store mutation을 확인했다. Final service `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
`0.39.1-admin-smoke` frontend payload host mutation run은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-frontend.md`에서 PASS로 기록한다. 후속 installed Web Console browser QA는 `docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md`에서 실제 installed listener `http://127.0.0.1:7777/` 기준 dashboard/VM/jobs/network/troubleshooting/diagnostic create-download/responsive screenshot hash를 확인했고, `token_value_observed=false`, `host_mutation_performed_by_browser_qa=false`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
ADR-0005의 `diagnostic-bundle-server-code-level` follow-up은 `DesktopNodeApiRequestProcessor`의 `POST /api/v1/diagnostics/bundles`, `GET /api/v1/diagnostics/bundles/{bundle_id}/download`, `DesktopNodeHostOptions`의 `--diagnostics-root`, `PcvDesktopNodeProduct.psm1` service plan wiring이 소유한다. 이 slice는 redacted `.bundle.json` archive creation/download, token-required route contract, retention max-count application을 code-level로 확인해 `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`를 기록한다. Installed listener execution, product wrapper diagnostics delegation, host mutation, public trusted signing, external stable publication은 실행하거나 주장하지 않는다.
ADR-0005의 `diagnostic-bundle-listener-code-level` follow-up은 `DesktopNodeHostApplication`이 `X-PCV-Request-Id`와 `X-Request-Id`를 `DesktopNodeApiRequestProcessor`로 전달하는 listener path를 소유한다. 이 slice는 bearer-required `POST /api/v1/diagnostics/bundles`, redacted `.bundle.json`, authenticated `GET /api/v1/diagnostics/bundles/{bundle_id}/download`, `X-PCV-Diagnostic-Bundle-Id` header를 in-process HttpListener test로 확인해 `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`를 기록한다. 이 code-level slice 자체는 installed service listener execution, host mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다.
ADR-0005의 `diagnostic-bundle-product-wrapper-code-level` follow-up은 `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`가 `New-PcvDesktopNodeDiagnosticBundle`로 위임하는 product wrapper path를 소유한다. 이 slice는 `product-wrapper-delegation-redacted.json`과 action result에 `actual_execution: code-level-product-wrapper`, `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `host_mutation_performed: false`, public trusted signing/external stable publication `not-claimed`를 기록한다. Installed service listener PASS는 별도 `0.39.0-admin-smoke` elevated MSI/service rerun evidence가 소유한다.
ADR-0005의 `diagnostic-bundle-native-service-action-config-code-level` follow-up은 `DesktopNode.Host.exe service-action configure-installed|repair-installed` C# native SCM config path를 소유한다. 이 slice는 `DesktopNodeWindowsServiceConfiguration.BinaryPathName`에 `--diagnostics-root`, protected token file, `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`가 들어가도록 보강한다. 0.38.9 installed final `PathName`에는 아직 이 인자들이 없었지만, 후속 `0.39.0-admin-smoke` elevated MSI/service rerun에서 installed listener execution은 `installed-listener-pass`, blocker는 `none`으로 닫혔다.
ADR-0005의 `timeout-rate-limit-hardening-preflight`는 `New-PcvTimeoutRateLimitHardeningPreflight.ps1`로 service name, Local API route prefix, route timeout target, request limit target, retry-after target, UI/API error contract, plan preview를 기록한다. Server config mutation, middleware enablement, retry semantics change, UI/API error behavior verification, load test execution, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication `not-claimed`를 유지한다.
ADR-0005의 `timeout-rate-limit-hardening-code-level`는 `DesktopNodeApiRequestProcessor`와 `DesktopNodeHostApplication`에 `/api/v1/` per-client request window, HTTP 429, `Retry-After`, `application/problem+json`, `PCV_RATE_LIMIT_EXCEEDED` contract를 적용한다. Route timeout enforcement, load test, server config mutation, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: partial-code-level-request-limit`, `route_timeout_policy: not-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, public trusted signing/external stable publication `not-claimed`를 유지한다.
ADR-0005의 `timeout-rate-limit-hardening-route-timeout-code-level`는 `DesktopNodeApiRequestProcessor`에 `/api/v1/` GET/read route response deadline, HTTP 504, `Retry-After`, `application/problem+json`, `PCV_ROUTE_TIMEOUT`, `route_timeout_seconds`, `request_id` contract를 적용한다. Mutation-route cancellation, native adapter cooperative cancellation, load test, server config mutation, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: partial-code-level-route-and-request-limit`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, public trusted signing/external stable publication `not-claimed`를 유지한다.
ADR-0005의 `timeout-rate-limit-hardening-server-config-code-level`는 `PcvDesktopNodeProduct.psm1` service plan과 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config에 `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`를 연결한다. Installed service mutation, service stop/start, load test, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, public trusted signing/external stable publication `not-claimed`를 유지한다.
ADR-0005의 `timeout-rate-limit-hardening-load-test-code-level`는 `ApiHardeningRequestProcessorTests`에서 같은 client identity의 64개 in-process request load를 실행하고 HTTP 200 `20`, HTTP 429 `44`, unexpected status `0`, `PCV_RATE_LIMIT_EXCEEDED` problem-details contract를 확인한다. Installed listener load, external load generator, service mutation, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: code-level-inprocess-pass`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, public trusted signing/external stable publication `not-claimed`를 유지한다.
Phase 24는 현재 ADR이 아니라 Local API job runtime public boundary 안정화 후보이며, C++23 전환 결정을 의미하지 않는다.
Phase 25는 현재 ADR이 아니라 C#/.NET contract/runtime/API/service/host, TypeScript Web Console, PowerShell Windows adapter 역할 분리 후보였다. ADR-0004 적용 이후 active product runtime/ops는 C#/.NET native path를 기본값으로 둔다.

- 2026-05-01 slice에서 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner가 `DesktopNode.Host.exe`로 교체됐다.
- `src/DesktopNode.Api/**`는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation routes, checkpoint create/restore/delete native mutation routes, job get/cancel/retry, JSON job store save/load/recovery를 처리한다.
- `GET /api/v1/jobs`는 additive pagination metadata를 가진 read-only job list다. 기본 page는 `limit=50&offset=0`, 최대 limit은 200이고 terminal job `succeeded`/`failed`/`canceled`는 최신 500개를 보존하며 active `queued`/`running` job은 보존한다. Persisted job store load도 같은 retention cap을 적용한다. Web Console `Activity`는 이 첫 page와 retention summary를 표시한다.
- Account/RBAC/JWT routes는 `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`, `GET /api/v1/auth/session`, `GET /api/v1/auth/rbac`다. 기본 bootstrap은 `%ProgramData%\PureCVisor\desktop-node\accounts.json`와 `jwt-signing-key.txt`를 준비하지만 계정을 만들지 않는 `no-default-account` 상태이며, 계정 미구성 상태에서는 existing bearer token gate가 authoritative하다.
- Console routes는 `GET /api/v1/console/capabilities`와 `GET /api/v1/vms/{id}/console`다. Windows console handoff는 Hyper-V `vmconnect` 기준이고 noVNC/WebSocket bridge는 기본 disabled다. `--novnc-target-host`와 `--novnc-target-port`를 명시하면 `/api/v1/console/novnc/{vm_id}` WebSocket-to-VNC TCP bridge를 사용할 수 있으며, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`에서 target-backed installed streaming PASS를 기록한다. Linux console backend 또는 browser-started host mutation을 가져오지 않는다.
- TUI source/package/smoke는 ADR-0011에 따라 active product에서 제거됐다. 활성 운영자 표면은 Web Console과 `pcvcli.exe`이며, 과거 `pcvtui.exe --smoke-once runtime` 결과는 historical predecessor evidence로만 해석한다.
- `host.status`는 C# registry/WMI/service/admin read route가 직접 처리한다.
- `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native read route product path가 직접 처리한다. Native parity가 불완전하면 helper fallback 없이 native structured failure를 반환한다.
- Web Console `Network` 화면은 `GET /api/v1/network/inventory`를 read-only로 표시하며 switch type/default/management OS/external adapter field를 TypeScript/static/browser fixture로 검증한다. 이 화면은 Hyper-V switch/IP/firewall mutation을 실행하지 않는다.
- Web Console `Troubleshooting` 화면의 Diagnostic Bundle 패널은 server-side bundle API create/download, product wrapper `CollectDiagnostics` fallback 안내, `%ProgramData%\PureCVisor\desktop-node\diagnostics` root, token value/Authorization header redaction boundary를 표시한다. Elevated wrapper 실행 대행, service/MSI/firewall/trust-store/LAN/update mutation은 실행하지 않는다.
- Web Console `Troubleshooting` 화면의 Token Rotation 패널은 protected token file root, runtime policy token storage, browser token presence, `Clear browser token`, `rotation handoff`, `no service token mutation` 경계를 표시한다. Browser token clear는 Web Console 입력/세션 token만 지우며 service protected token file 또는 host mutation을 실행하지 않는다.
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`는 file/HTTPS ZIP `-SourceUri` source gate와 file/HTTPS JSON `-UpdateCatalogUri`/`-UpdateChannel` catalog gate를 지원한다. Catalog gate는 schema v1, product id, selected channel version, package URI, SHA-256을 service stop 전에 검증하고 기존 update package source gate로 넘기며, result와 update transaction journal에 `update_catalog`을 기록한다. 이 code-level evidence는 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`이며 public trusted signing, 외부 stable publication, installed destructive catalog update smoke evidence가 아니다.
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`는 product root backup 이후 copy/config/start/health failure에서 previous root restore를 시도하는 filesystem rollback path를 갖는다. Journal은 restore 성공 시 `failed-rolled-back`, restore 실패 시 `failed-rollback-failed`를 기록한다. 이 code-level evidence는 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`이며 post-crash resume/reconcile, service/data/config/job-store transaction manager, public trusted signing, 외부 stable publication evidence가 아니다.
- Installer `build.ps1`는 MSI/provenance/hash sidecar와 함께 `PureCVisorDesktopNode-<version>-windows-x64.publication.json` descriptor를 작성한다. Descriptor는 artifact SHA/provenance와 `internal-artifact-descriptor-only` publication boundary를 연결하고 public trusted signing/external stable publication은 `not-claimed`, Burn/MSIX는 `not-built`, winget은 `not-generated`, catalog publication은 `not-published`로 기록한다. 이 code-level evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`이며 external publication service, public trusted signing, 외부 stable publication evidence가 아니다.
- `GET /api/v1/vms/{id}`는 native `vm.list` handled 결과를 사용하고, missing VM 또는 native parity failure를 helper 재시도 없이 반환한다.
- `GET /api/v1/vms/{id}/checkpoints`는 native VM inventory와 WMI snapshot association을 사용하고, native VM/checkpoint parity failure를 helper 재시도 없이 반환한다.
- `POST /api/v1/vms/{id}/start`, `POST /api/v1/vms/{id}/shutdown`, `POST /api/v1/vms/{id}/poweroff`, `POST /api/v1/vms/{id}/restart`는 C# WMI `Msvm_ComputerSystem.RequestStateChange` adapter가 직접 실행한다.
- `POST /api/v1/vms`는 native VM create adapter가 처리한다. 이번 native product path는 Hyper-V Generation 2만 지원한다.
- `DELETE /api/v1/vms/{id}`는 C# WMI `DestroySystem` adapter가 직접 실행하며 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. `0.30.1-admin-smoke` installed destructive smoke는 managed delete `action=delete`, repeat delete `action=absent`, unmanaged guard block, cleanup/no-reboot evidence를 확인했다.
- `POST /api/v1/vms/{id}/checkpoints`, `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore`, `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}`는 C# WMI snapshot service adapter가 직접 실행한다.
- Active product `DesktopNode.Host.exe listen`은 Hyper-V PowerShell helper script path를 받지 않는다. `--helper-script`는 `PCV_HOST_HELPER_SCRIPT_RETIRED`로 차단되며 product manifest에도 `helper_script`/`api_script` path를 기록하지 않는다.
- `0.30.1-admin-smoke`는 VM create/start/restart/poweroff/delete와 checkpoint create/restore/delete native mutation adapter 포함 installed service/MSI/Hyper-V route smoke를 `vm.poweroff-before-restore` 최소 안정 조건과 VM delete guard 조건으로 통과했다.
- Installer-ISO VM의 `vm.shutdown`은 guest shutdown integration 미준비 상태를 `PCV_VM_SHUTDOWN_NOT_AVAILABLE` structured failure로 반환함을 확인했다.
- Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD 기반 Gen1 differencing VM으로 확인했다. Installed Local API `vm.shutdown` job은 `succeeded`, final VM state는 `Off`, smoke VM/ProgramData cleanup은 완료 상태다.
- `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed|data-root-remove`는 native SCM/data-root action path를 갖는다. `remove-installed --remove-data`는 direct ProgramData deletion 없이 `data-root-remove` handoff descriptor를 반환하고, `data-root-remove --remove-data`는 service absent precondition에서 allowlist data-root path만 삭제한다. `0.30.3-admin-smoke` installed destructive service/data-root lifecycle smoke는 `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`에서 PASS였고, unsigned `AllowUnsignedDev` admin-smoke evidence다.
- `DesktopNode.Host.exe service-action firewall-enable|firewall-remove|trust-store-install|trust-store-remove`는 native Windows Firewall COM/X509Store action path를 갖는다. `0.35.7-admin-smoke` historical OS gate는 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`에서 firewall rule enable/remove, LAN IP bearer runtime policy/Web assets `HTTP 200`, Event Log register/remove, config-migration-apply blocked/no-mutation descriptor, ADR-0003 internal Root/TrustedPublisher install/remove/restore를 PASS로 확인했다. MSI/Hyper-V/service/data-root gate는 `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`에서 확인했고, MSI provenance commit은 `2ec9e71d45b702e106824c86500cd6152b18fab7`, MSI SHA-256은 `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`이다.
- `0.36.0-admin-smoke` 후속 active product .NET 100% cleanup gate는 `artifacts/routeparity-service-msi-hyperv-dotnet100-20260505-0.36.0`에서 Service/MSI/Hyper-V route parity를 PASS로 확인했다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `70cb8b720588c6ef69aca59fed48f870865d7bca8c7a4ea8e623ab6b6e99d048`, final service는 `Running`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다.
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, step `timed_out=false`, `exit_code=0`, heartbeat lines `25`다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`, signing mode는 `AllowUnsignedDev`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.38.7-rc.1`은 사용자 관리자 opt-in으로 internal enterprise `RequireSigned` MSI build와 Authenticode/SignTool verify를 통과한 최신 internal signed build evidence다. Evidence root는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`이고 MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`, signer thumbprint는 `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`이다. 이 evidence는 ADR-0003 internal trust 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.38.4-rc.1` signed build evidence는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`에 historical evidence로 보존한다.
- `0.38.9-admin-smoke`는 사용자 관리자 opt-in으로 Batch Supervisor full admin host mutation gate를 통과한 historical Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store evidence다. Evidence root는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389`, `artifacts/os-mutation-gates-batch-profile-20260508-202255-0389`이다. MSI provenance commit은 `159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce`, MSI SHA-256은 `86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, attempt count 1, timeout false였고, final service `Running`, product manifest version `0.38.9-admin-smoke`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`로 완료했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.39.0-admin-smoke`는 사용자 관리자 opt-in으로 MSI/service installed listener rerun을 `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`에서 PASS했다. MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false였고 final service `Running`, product manifest version `0.39.0-admin-smoke`, SCM `PathName` diagnostic bundle/hardening args present, diagnostic bundle POST `201`, download `200`, redaction PASS, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.39.0-admin-smoke` 후속 OS mutation gate는 사용자 관리자 opt-in으로 `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`에서 PASS했다. Batch summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false였고 firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged이며 `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.39.1-admin-smoke`는 사용자 관리자 opt-in으로 MSI/update package apply를 `artifacts/msi-update-package-20260509-0391`에서 PASS했다. MSI provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, signing mode는 `AllowUnsignedDev`다. Elevated MSI apply exit `0`, final service `Running`, product manifest version `0.39.1-admin-smoke`, loopback Web Console HTTP `200`이다. Firewall/trust-store/LAN/Event Log OS gate와 diagnostic bundle installed listener create/download는 이 apply 범위가 아니며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`은 `0.42.9-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`이고 full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260513-040213-0429`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.9-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0428-hostmutation`은 `0.42.8-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-233650-0428-r2`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2`, `artifacts/os-mutation-gates-batch-profile-20260512-233650-0428-r2`이고 full-gate MSI SHA-256은 `01762ee3fd103981ac6fce121b6749e832dfabc7420123a6363f7fbe0e0f8f99`, post-merge package MSI SHA-256은 `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, provenance commit은 `5397e580c98a34e8b7beb5b9773d1d857025315b`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `batch-evidence-root-service-action-package-2026-05-13-0429`은 `0.42.9-admin-smoke` 이전 product payload package build evidence다. Artifact는 `artifacts/admin-smoke-package-20260513-0429`, MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Event Log default transition timeout guard와 installer timeout propagation을 포함한다. 0429 full host mutation current claim은 별도 0429 full gate evidence가 소유한다. `0.42.8-admin-smoke -> 0.42.9-admin-smoke` package-pair candidate는 installed update/rollback만 PASS이며 clean-host/Burn/MSIX/descriptor는 아직 PASS claim이 아니다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-11 `full-admin-host-mutation-gate-2026-05-11-0422-hostmutation`은 `0.42.2-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260511-232659-0422`, `artifacts/os-mutation-gates-batch-profile-20260511-232659-0422`이고 MSI SHA-256은 `e4d66d006cd14355b57507fea3c9a41b6c17a002f9ff824bec35830ce029fc29`, provenance commit은 `1d68a3b6c2ac1d9202d0ec53d0ccb35858d84ee6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.42.2-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation evidence로 보존한다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.38.8-admin-smoke`는 elevated installed destructive update/rollback smoke를 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`에서 PASS했다. MSI provenance commit은 `fd4f854646fc159d54f7578230f00c51f80e201f`, MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`, signing mode는 `AllowUnsignedDev`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 current product root를 `0.38.6-admin-smoke`로 복원하고 `0.38.8-admin-smoke` root를 `DesktopNode.failed` diagnostics로 보존했다. Final service는 `Running`, boot time unchanged, `host_mutation_performed=true`다. 최초 `artifacts/product-update-rollback-mutation-20260507-0388` non-elevated attempt는 blocked history이며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `DesktopNode.Host.exe service-action config-migration-apply|job-store-migration-apply`는 product config/job store migration apply actual path를 갖는다. `product-config-v1-to-v2`와 `job-store-v1-to-v2` plan/version 1에서 service stopped/runtime writer stopped proof와 owned schema v1 precondition을 통과하면 backup write, same-directory temp replace, rollback/recovery diagnostics를 기록하며 schema v2로 승격한다. 두 action 모두 implicit service stop/start, token mutation, service identity mutation, MSI/update/rollback, Hyper-V/firewall/trust-store/LAN/Event Log mutation은 수행하지 않는다. `0.38.6-admin-smoke` installed destructive admin smoke는 `artifacts/config-jobstore-migration-apply-installed-20260507-0386`에서 PASS했고 final service `Running`, manifest/job store schema `2`, boot time unchanged, post-migration API read ok를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- PowerShell Local API와 Hyper-V helper는 `archive/spikes/**` component/archive baseline으로만 유지한다. Active product runtime/request/admin ops path에는 PowerShell helper process fallback을 두지 않고, legacy WinSW PowerShell Local API generation은 retired error로 차단한다.

이 변경들은 `AllowUnsignedDev` admin-smoke 및 .NET/PowerShell contract 검증 범위이며 public trusted signing 또는 외부 stable publication을 의미하지 않는다. 내부 전용 서비스 제품 런타임 승격 판단은 ADR-0004가 소유한다.

## 필수 검증

Desktop Node 변경 후에는 영향 범위에 맞춰 `DesktopNode.Verification`의 Required CI 네 shard와
Web required entrypoint를 우선 실행한다.

`installer-policy` shard는 cutover 경계 때문에 clean committed HEAD를 요구한다. 따라서 변경 중에는
다음 pre-commit 검증을 실행한다.

```text
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
npm ci --prefix web
npm run test:required --prefix web
git diff --check
```

전체 solution test의 `policy-boundaries`는 활성 cutover 계약상 clean committed HEAD를 요구한다.
변경 중에는 영향 범위의 focused test만 실행한다. Clean committed HEAD에서 전체 solution test는
`dotnet` shard가, Installer 필터와 clean-worktree policy boundary는 `installer-policy` shard가 검증한다.

커밋 후 `git status --short` 출력이 비어 있는 상태에서 Required CI exact four를 실행한다. `web`
shard가 `npm run test:required`를 포함하므로 별도로 다시 실행하지 않는다.

```text
git status --short
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/local-dotnet --shard dotnet
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/local-web --shard web
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/local-delivery --shard delivery
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/local-installer-policy --shard installer-policy
```

위 `.ps1` 값은 shard 선택용 changed-path 데이터이며 PowerShell process 호출이 아니다. Component/archive
baseline의 Pester와 관리자 PowerShell은 verification ownership map에 따라 별도 실행하는 비필수
legacy/manual/admin 검증이다. Active Required CI의 Pester 및 비관리자 PowerShell process invocation은 `0`이다.

실제 Hyper-V VM 생성, service install/start/stop/delete, Windows Firewall 변경, Event Log source 등록, Task Scheduler 등록, `Restart-Computer`, MSI `msiexec` install/repair/uninstall/`REMOVE_DATA=1`, signed release build, mutating update/rollback은 관리자 권한 opt-in 검증으로만 실행한다.

## 작업 원칙

- 모든 신규/수정 문서는 한국어 본문을 기본으로 작성한다. 코드 식별자, 명령어, 파일 경로, route, product/version/evidence id, test fixture token은 원문을 유지한다.
- 역사 evidence/phase 문서는 검증 anchor와 과거 기록을 깨뜨릴 수 있으므로 `docs/KOREAN_DOCUMENTATION_ROLLOUT.md`의 우선순위에 따라 단계적으로 재작성한다.
- PowerShell 7과 Pester 5는 legacy parity 또는 manual/admin 검증에만 사용한다. 기본 Required CI
  검증 기준은 C#/.NET `DesktopNode.Verification`과 Node required entrypoint다.
- 장기 token 값은 command line에 노출하지 않는다. `-ApiTokenProtectedFile` 또는 token file 경로를 우선한다.
- Local API listener는 기본 loopback-only 정책을 유지한다. 최신 기본 surface는 Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...` 분리다. `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`는 설치본 service `PathName`의 `--web-prefix "http://127.0.0.1:80/"`, Web `200`, API `200`, Web-port API `PCV_API_ROUTE_ON_WEB_PORT` rejection을 PASS로 기록한다. `/pcv-config.js`가 browser API origin을 주입하고 LAN mode는 명시적 `-AllowLan`과 token source가 있을 때만 허용한다.
- 실제 host mutation은 `-WhatIf` 또는 injectable runner 테스트와 분리한다.
