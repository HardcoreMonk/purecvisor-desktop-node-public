# Desktop Node 저장소 경계

## 2026-08-25 public source authority boundary

Source integration authority is `HardcoreMonk/purecvisor-desktop-node-public`, created from a sanitized
parentless root. Public visibility is governed by the rights-reserved `LICENSE`, `SECURITY.md`, and
`docs/PUBLIC_SOURCE_AUTHORITY.md`; it does not grant an open-source license.

Source publication and binary publication are separate. This boundary creates no package candidate,
installer, trusted signature, stable update channel, winget submission, or external stable release.
`public_trusted_signing=false`, `external_stable_publication=false`, and `promotion_eligible=false` remain
current. The operational version remains `0.42.74-admin-smoke` and its saved-lifecycle actual-VM blocker is
not closed by source publication.

## 2026-07-16 현재 운영자/배포 경계

ADR-0011에 따라 active operator surface는 Web Console과 PCVCLI다. 이 결정은 Local
API/backend와 ADR-0006 internal-private-network-only 배포 경계를 바꾸지 않는다.
Code-level evidence는
`docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`다.
`0.42.65-admin-smoke` internal package/fullgate/actual-VM functional correctness/CLI-Web
installed current-card가 PASS했다. Operational gate는
`full-admin-host-mutation-gate-20260716-04265`이고 0.42.64는 immediate CLI/Web predecessor, 0.42.62
Web/TUI/CLI current-card는 historical TUI predecessor다. 이 승격은 internal admin-smoke에
한정되며 public trusted signing과 external stable publication은 계속 out-of-scope다.

## 2026-05-29 historical predecessor

이 저장소의 배포 경계는 계속 ADR-0006 internal-private-network-only다. 최신 operational
evidence anchor는 `0.42.59-admin-smoke` /
`full-admin-host-mutation-gate-20260529-04259`, manual-admin package-pair closure는
`0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. 최신 설치본 smoke는
`0.42.59-admin-smoke`이며
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`에서
확인했다. Package evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`, full admin host mutation
evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`,
manual-admin evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`다.
최신 public-boundary main push CI evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`,
run `26636072420`, job `78496568595`, head `5a2f91762a6c2a8ab6b84d334fa6cb420474671f`이다.
`0.42.60-admin-smoke` installed current-card payload 후보는 이미 열려 있으며, docs-maintenance
postpush만으로 추가 package 후보를 열지 않는다. account/noVNC는 0.42.58 PASS를 carry-forward하고 actual VM Guest Execution/QoS smoke는
provider/control payload 변경 때 재실행한다.
Public trusted signing, trusted timestamp, winget public submission, public stable installer
URL, external stable publication은 계속 out-of-scope다. 아래 이전 날짜 current 문단은
historical predecessor로 해석한다.
직전 0.42.58 predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.

## 2026-05-21 historical predecessor

이 저장소의 배포 경계는 계속 ADR-0006 internal-private-network-only다. 최신 operational
evidence anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`, manual-admin package-pair closure는
`0.42.39-admin-smoke -> 0.42.40-admin-smoke` /
`manual-admin-campaign-descriptor-20260521-04239-04240-closed`다. 최신 설치본 smoke는
`0.42.40-admin-smoke`이며
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`와
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`에서
확인했다. Actual VM Web/TUI QoS/guest readback smoke는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`에서
기록하고, 설치본 TUI row projection blocker는 `0.42.41-admin-smoke` package chain trigger로
남겼다. Historical 0.42.38 VM media/resource mutation route promotion과 0.42.37
Hyper-V pause lifecycle smoke는 predecessor로 보존한다. PR #167 public-boundary PASS는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`다.
Public trusted signing, trusted timestamp, winget public submission, public stable installer
URL, external stable publication은 계속 out-of-scope다. 아래 이전 날짜 current 문단은
historical predecessor로 해석한다.

## 2026-05-18 현재 기준

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

> 대상: `purecvisor-desktop-node`

이 문서는 기존 `purecvisor-single` 공개 릴리스 경계 문서 이름을 보존하지만, 현재 저장소에서는 Windows Desktop Node 독립 저장소 경계를 정의한다.

2026-05-18 최신 internal installed operational evidence는 `0.42.30-admin-smoke`다.
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`는
`runtime-api-current-evidence-rollup-v1` installed current-card PASS를 기록하지만,
public trusted signing, trusted timestamp, external stable publication, winget
submission, public stable installer URL은 계속 `out-of-scope`다.
최신 닫힌 manual-admin package-pair는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md` /
`manual-admin-campaign-descriptor-20260518-04229-04230-closed`이며 이 역시 public
release claim이 아니다. PR #156 post-merge public-boundary evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`,
run `26017721669`, job `76471545641`, head `a4509c552c003ee0fc87b54b26529686e6dfeb84`이다.
PR #155 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`,
run `26013384587`, job `76458402221`, head `2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f`로 historical predecessor에 보존한다.
PR #154 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md`,
run `25989986761`, job `76394250912`, head `d7f611dfc14a9fa1507f936559209513272b585a`로 historical predecessor에 보존한다.
PR #155/PR #154 후속 package decision은 당시 historical deferred 판단으로 보존한다.
PR #153 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`,
run `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`로 historical predecessor에 보존한다.
Historical `0.42.28-admin-smoke -> 0.42.29-admin-smoke` selector/package-chain predecessor는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md` /
`manual-admin-campaign-descriptor-20260517-04228-04229-closed`이며 target MSI
SHA-256 `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`,
update ZIP SHA-256 `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`,
provenance commit `d306712ad671c8a00d5c560765b8952e24a07502`로 보존한다.
이 package-pair도 public trusted signing 또는 외부 stable publication evidence가 아니다.
이후 사용자 승인으로 `0.42.30-admin-smoke` package chain을 열어 internal admin-smoke
current anchor로 승격했지만 public release boundary는 바뀌지 않는다.
Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke`
predecessor는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` /
`manual-admin-campaign-descriptor-20260517-04227-04228-closed`이며
full admin host mutation batch `full-admin-host-mutation-gate-20260517-04228`,
target MSI SHA-256 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`,
clean package MSI SHA-256 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`,
update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`,
provenance commit `b9676f6dc37d667ae0d60367e9f4e576a27e3864`로 보존한다.
Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` /
`manual-admin-campaign-descriptor-20260517-04226-04227-closed`이며
`host-ops-lifecycle-descriptor-bridge-v1` /
`service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
계약을 보존한다. 다음 manual-admin 후보는 새 product payload가 생길 때 연다.

## 결정

```text
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike
DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned
DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT: dotnet-windows-service-host-default-with-keep-spike
DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start
DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_READ_PARITY_GUARD: network-inventory-vm-list-vm-detail-and-checkpoint-list-native-structured-failure-on-incomplete-parity
DESKTOP_NODE_PHASE25_NATIVE_CHECKPOINT_MUTATION_START: checkpoint-create-restore-delete-dotnet-native-adapter
DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime
DESKTOP_NODE_PUBLIC_DISTRIBUTION_DECISION_CANDIDATE: closed-not-adopted
DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only
```

현재 적용 결정의 진입점은 `docs/ADR_INDEX.md`이며, 이 문서는 공개 릴리스 경계와 금지 표면을 요약한다. Desktop Node의 배포 범위는 ADR-0006 기준 내부 사설망 전용이다. Public trusted signing, trusted timestamp, 외부 stable publication/catalog upload, winget public submission, public stable installer URL, 일반 사용자 대상 public release, clean-host public signed install/update/rollback smoke는 `out-of-scope`다.

ADR-0005는 `public-distribution-operations-expansion-candidate` 제안이었지만 2026-05-10 ADR-0006 결정으로 미채택/종료했다. `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`는 보존용 closed-not-adopted matrix이며 public signing/winget/external upload/public clean-host smoke는 `out-of-scope`다. 현재 적용 배포 gate는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`이고 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke를 중심으로 추적한다.

ADR-0005의 public preflight, blocked scan, public ops final follow-up attempt, winget validate, Burn/MSIX, Credential Manager, Event Log, service token, diagnostic bundle, installed listener load/rate-limit evidence는 역사/내부 운영 evidence로 보존한다. 이 evidence들은 public trusted signing 또는 external stable publication을 주장하지 않는다.

ADR-0006 내부 배포 후속으로 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`는 installed service HTTPS binding, certificate rotation, binding/cert removal, original HTTP service restore를 PASS로 기록한다. `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`는 dedicated Hyper-V clean-host에서 internal signed MSI install, internal catalog update, rollback, final service/Web health를 PASS로 기록한다.

2026-05-16 `0.42.23-admin-smoke` package-pair closure와 full admin host mutation은 internal admin-smoke historical evidence로만 해석한다. 당시 full gate는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`, current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md`, public-boundary CI guard는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md`가 소유한다. Public-boundary CI는 run `25954744127`, job `76299282407`에서 PASS했다. Full-gate MSI SHA-256은 `ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932`, closed package MSI SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, full-gate provenance commit은 `d11a096086326004f27facd9612c2296ded15a4b`, closed package provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`다. 2026-05-16 `0.42.24-admin-smoke`와 `0.42.25-admin-smoke` full gate는 이후 `0.42.26-admin-smoke` fullgate/current-card가 current anchor를 소유하므로 historical predecessor로 보존한다. Public trusted signing, external stable publication, winget submission, public stable installer URL, public signed clean-host smoke는 계속 claim하지 않는다.

2026-05-16 `0.42.25-admin-smoke` full-gate/current-card/manual-admin campaign은
internal admin-smoke evidence이며 public release boundary를 바꾸지 않는다.
Full-gate/target MSI SHA-256은
`e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance commit은
`4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`, update ZIP SHA-256은
`393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`다.
`0.42.24-admin-smoke -> 0.42.25-admin-smoke` descriptor
`manual-admin-campaign-descriptor-20260516-04224-04225-closed`는 `missing_count=0`,
`not_pass_count=0`으로 PASS다. Public-boundary CI guard는 PR #144 post-merge run
`25959505688`, job `76312299500`, head SHA
`4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`에서 PASS했다. Earlier package build record
`docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md`는 MSI SHA-256
`5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`, provenance commit
`403d4474c4b88136774600cc81ca2d941c0b5e4b`로 historical candidate record로 보존한다.

2026-05-17 `0.42.26-admin-smoke` package/full-gate/current-card evidence와
`0.42.25-admin-smoke -> 0.42.26-admin-smoke` package-pair closure도 internal
admin-smoke evidence이며 public release boundary를 바꾸지 않는다. Package MSI
SHA-256은 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`,
full-gate MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`,
provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`다. Closure descriptor
`manual-admin-campaign-descriptor-20260517-04225-04226-closed`는 `missing_count=0`,
`not_pass_count=0`이고 update ZIP SHA-256은
`4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`다. 2026-05-16
descriptor `manual-admin-campaign-descriptor-20260516-04225-04226`는 readiness PASS지만
당시 `missing_count=4`, `not_pass_count=1`이었던 initial blocked descriptor로 보존한다.
Public-boundary CI guard는 PR #145 post-merge run `25961834812`, job
`76318357776`, head SHA `d6500c01c972cbc7ca1e290e51120181ceea1501`에서 PASS했다.

2026-05-17 `0.42.27-admin-smoke` package/full-gate/current-card evidence와
`0.42.26-admin-smoke -> 0.42.27-admin-smoke` package-pair closure도 internal
admin-smoke evidence이며 public release boundary를 바꾸지 않는다. PR #149 post-merge
public-boundary CI guard는 run `25974335803`, job `76351743536`, head SHA
`dd895306c4b08802d262b4afb890382dd991a4d0`에서 PASS했다. 해당 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr149-postmerge-pass.md`
가 소유한다. 이후 PR #151 main push evidence와 `0.42.28-admin-smoke` package chain이
current anchor를 소유한다. Public trusted signing, external stable
publication, winget submission, public stable installer URL, clean-host public signed
install/update/rollback smoke는 계속 out-of-scope다.

PR #150 post-merge public-boundary CI guard도 run `25983307305`, job `76375957834`,
head SHA `6d4b5d95742044bdbd8def933fbc8cdefbba71b3`에서 PASS했다. 해당 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass.md`가
소유한다.

PR #151 post-merge public-boundary CI guard는 run `25984814303`, job `76380096421`,
head SHA `26ae50fa7bef11b4919b441e706bde505463aded`에서 PASS했다. 해당 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`가
소유한다. 이어진 Host Ops Web diagnostics bucket table product payload 변경으로
`0.42.28-admin-smoke` package chain, full admin host mutation, installed account/noVNC smoke를
실행했고, 이후 `0.42.27-admin-smoke -> 0.42.28-admin-smoke` manual-admin package-pair도
PASS로 닫았다. ADR-0005 public distribution evidence는 historical/out-of-scope로 유지한다.

PR #152 post-merge public-boundary CI guard는 run `25985786230`, job `76382711230`,
head SHA `ca07514097f4e9524a7f3630d321c9666593c962`에서 PASS했다. 해당 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md`가
소유한다. 이 CI evidence도 public trusted signing, external stable publication, winget
submission, public stable installer URL을 claim하지 않는다.

PR #156 post-merge public-boundary CI guard는 run `26017721669`, job `76471545641`,
head SHA `a4509c552c003ee0fc87b54b26529686e6dfeb84`에서 PASS했다. 해당 evidence는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`가
소유한다. 이 follow-up은 2026-05-18 `origin/main...HEAD` diff가 비어 있음을 확인했으므로
`0.42.30-admin-smoke` package build, full admin host mutation, 04229->04230 manual-admin
package-pair를 실행하지 않는다.

2026-05-09 public external gates blocked scan은 `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`, `artifacts/public-external-gates-blocked-20260509-0391`에서 public signing material, timestamp URL, external upload endpoint/credential, public stable installer URL, clean-host runner가 없음을 기록했다. 따라서 timestamp evidence, external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback은 blocked이며 현재 boundary를 public release로 승격하지 않는다.

2026-05-09 public ops final follow-up attempt는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `artifacts/public-ops-final-followup-attempt-20260509-0391`에서 1-7 final public operations follow-up prerequisite scan을 기록했다. `remaining_follow_up_count: 7`, `actual_execution=local-final-followup-prerequisite-scan-executed`, `host_mutation_performed=false`, `public_release=not-claimed`이며 public trusted signing/external stable publication은 계속 주장하지 않는다. 2026-05-10 local 재생성도 `ok=true`, `remaining_follow_up_count=7`, `host_mutation_performed=false`, public release `not-claimed`를 유지했다.

2026-05-09 public ops gate execution readiness는 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`, `artifacts/public-ops-gate-execution-readiness-20260509-0392`에서 6개 잔여 public operations gate 상태를 기록했다. `New-PcvPublicOpsGateExecutionReadiness.ps1`는 external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback을 blocked로 유지하고, TLS는 `partial-code-level-cert-generate-rotate-delete-pass`, `tls_private_key_material_written=false`, `tls_binding=not-run`, `host_mutation_performed=false`로 기록한다. 2026-05-10 `-RunLocalTlsLifecycle` 재생성도 `ok=true`, `host_mutation_performed=false`, public trusted signing `not-claimed`, public release `not-claimed`를 유지했다. 이 evidence도 public trusted signing/external stable publication을 주장하지 않는다.

2026-05-09 public ops installed hardening code-level evidence는 `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`에서 native `credential-manager-system-proof`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard` service-action path를 기록했다. 후속 2026-05-10 `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`가 installed SYSTEM proof, service default token-source migration, service reload, old source rejection, rollback diagnostics를 PASS로 닫았고, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`가 installed Event Log default writer, provider repair/remove/restore, schema v1 event write, volume guard를 PASS로 닫았다. Internal HTTPS binding/trust boundary는 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`에서 PASS로 닫혔다. Public trusted signing/external stable publication은 주장하지 않는다.

2026-05-10 Burn bootstrapper lifecycle smoke는 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416`에서 internal AllowUnsignedDev bundle build/install/repair/remove와 direct MSI restore PASS를 기록했다. 2026-05-09 Windows Event Log provider default transition은 `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `artifacts/windows-event-log-provider-default-transition-20260509-0391`에서 installed provider register/write/query PASS를 기록했고, 2026-05-10 `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `artifacts/windows-event-log-default-transition-installed-20260510-0396`에서 installed default writer hardening PASS를 기록했다. 2026-05-09 Windows Credential Manager transition은 `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `artifacts/windows-credential-manager-transition-20260509-0391`에서 current-user capability PASS와 당시 `LocalSystem` blocker를 기록했고, 2026-05-10 `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `artifacts/windows-credential-manager-default-transition-installed-20260510-0395`에서 installed LocalSystem default transition PASS를 기록했다. 이 evidence들은 public trusted signing 또는 external stable publication evidence가 아니다.

2026-05-10 MSIX package lifecycle smoke는 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/msix-package-lifecycle-smoke-20260510-0416`에서 별도 internal package identity `PureCVisor.DesktopNode.MsixSmoke`의 build/sign/verify, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence를 PASS로 확인했다. 이 evidence는 `host_mutation_performed: true`인 관리자 opt-in evidence지만 public trusted signing은 `excluded`, external stable publication은 `not-claimed`이며, 현재 public release boundary를 승격하지 않는다.

2026-05-09 MSI/update package apply evidence는 `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`, `artifacts/msi-update-package-20260509-0391`에서 `0.39.1-admin-smoke` AllowUnsignedDev MSI build, update ZIP/catalog validation, elevated MSI apply, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 PASS로 확인했다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`이다. 이 evidence는 `host_mutation_performed: true`인 internal admin-smoke evidence지만 public trusted signing은 `excluded`, external stable publication은 `not-claimed`이며, 현재 public release boundary를 승격하지 않는다.

2026-05-10 Web/API port split evidence는 `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`와 `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`가 추적한다. 이 slice는 기본 Web Console surface를 `http://127.0.0.1/`로 두고 Local API surface를 `http://127.0.0.1:7777/api/v1/...`에 유지한다. `/pcv-config.js` API origin injection, Web listener `/api/*` `PCV_API_ROUTE_ON_WEB_PORT` rejection, API listener CORS origin contract를 code-level과 설치본 listener smoke로 확인했다. 설치본 smoke는 service `PathName` `--web-prefix "http://127.0.0.1:80/"`, Web `200`, API `200`, CORS preflight `204`를 PASS로 기록한다. Public HTTPS/443, public trusted signing, external stable publication은 주장하지 않는다. 내부 HTTPS/TLS lifecycle은 ADR-0006 internal matrix의 별도 installed smoke로 추적하며 기본 loopback HTTP listener 판단을 바꾸지 않는다.

2026-05-15 post-04218 contract alignment evidence는
`docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`가 추적한다.
이 evidence는 `0.42.18-admin-smoke`를 source anchor로 Runtime/Core API route
diagnostics bridge, Hyper-V dispatch catalog, Host Ops lifecycle bucket, packaging
next trigger, Web Console/TUI/CLI operator journey를 문서 계약으로 정렬했다.
`host_mutation_performed=false`이며 ADR-0005는 `closed-not-adopted`, ADR-0006은
`internal-private-network-only`로 유지한다. Public trusted signing, public stable
installer URL, winget submission, external stable publication, public clean-host
signed install/update/rollback을 주장하지 않는다.

2026-05-15 post-04218 follow-up execution evidence는
`docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`가 추적한다.
`0.42.19-admin-smoke` package build는
`artifacts/admin-smoke-package-20260515-04219`에 남겼고 MSI SHA-256은
`3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55`다.
이 실행은 `PUBLIC_BOUNDARY_CI_CONTRACT`를 `public-boundary-ci-required` guard로
추가하지만, public trusted signing, external stable publication, winget public
submission, public stable installer URL, public signed clean-host smoke를 주장하지
않는다. Package-pair campaign, update ZIP build, full admin host mutation도 이
evidence에서는 실행하지 않았다.

2026-05-10 Account/RBAC/JWT/console code-level evidence는 `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`가 추적한다. 이 slice는 `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`, `GET /api/v1/auth/session`, `GET /api/v1/auth/rbac`, `GET /api/v1/console/capabilities`, `GET /api/v1/vms/{id}/console`을 Windows Desktop Node local auth/console surface로 고정한다. 기본 bootstrap은 `no-default-account`이며 계정 미구성 상태에서는 기존 protected bearer token gate가 계속 authoritative하다. 후속 `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`는 installed account login smoke PASS와 opt-in noVNC WebSocket-to-VNC TCP bridge code-level PASS를 기록한다. Installed account login artifact는 `artifacts/installed-account-login-smoke-20260510-0410-final`이고, `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`는 `artifacts/installed-account-login-browser-live-smoke-20260510-235543`와 `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`에서 설치본 Web Console real account login form, session/RBAC/console route, diagnostic create/download, responsive browser QA PASS를 기록한다. `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`는 target-backed noVNC installed streaming PASS와 installed TUI operator smoke PASS를 기록한다. noVNC bridge는 explicit target host/port 구성 전까지 disabled다. 이 evidence들은 public release boundary 승격을 주장하지 않는다.

2026-05-10 manual-admin operator/hardening follow-up evidence는 `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `artifacts/manual-admin-followup-20260510-0415`가 추적한다. 0.41.5 installed account login, target-backed noVNC, service token rotation/revoke, Windows Credential Manager default transition, internal HTTPS/TLS lifecycle, Windows Event Log default transition은 PASS다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. 이 evidence는 public trusted signing, external stable publication, public release boundary 승격을 주장하지 않는다.

ADR-0005의 `diagnostic-bundle-server-code-level`는 `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`가 추적한다. 이 slice는 `POST /api/v1/diagnostics/bundles`가 redacted `.bundle.json`을 만들고 `GET /api/v1/diagnostics/bundles/{bundle_id}/download`가 다운로드로 제공하는 code-level Local API action이며, service plan은 `--diagnostics-root`를 포함한다. Matrix 상태는 `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`다. Installed listener execution, product wrapper diagnostics delegation, host mutation, public trusted signing, external stable publication을 주장하지 않는다.

ADR-0005의 `diagnostic-bundle-listener-code-level`는 `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`가 추적한다. 이 slice는 in-process `DesktopNodeHostApplication` listener에서 bearer-required create/download, `X-PCV-Request-Id` propagation, `X-PCV-Diagnostic-Bundle-Id` download header를 확인한다. Matrix 상태는 `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`다. 이 code-level slice 자체는 installed service listener execution, host mutation, public trusted signing, external stable publication을 주장하지 않는다.

ADR-0005의 `diagnostic-bundle-product-wrapper-code-level`는 `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`가 추적한다. 이 slice는 product wrapper `CollectDiagnostics` action이 `New-PcvDesktopNodeDiagnosticBundle`로 위임되고 `product-wrapper-delegation-redacted.json`을 기록하는 code-level evidence다. Matrix 상태는 `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `actual_execution: code-level-product-wrapper`, `host_mutation_performed: false`다. Installed service listener PASS는 별도 `0.39.0-admin-smoke` rerun evidence가 소유하며 public distribution claim은 계속 제외한다.

ADR-0005의 `diagnostic-bundle-native-service-action-config-code-level`는 `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`가 추적한다. 이 slice는 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config가 `--diagnostics-root`, protected token file, route timeout, request limit, burst, retry-after 인자를 `BinaryPathName`에 포함하도록 보강한다. 0.38.9 installed final `PathName`은 아직 이 인자들을 포함하지 않았지만, 후속 `0.39.0-admin-smoke` elevated MSI/service rerun에서 installed service listener execution은 `installed-listener-pass`, blocker는 `none`으로 닫혔다. Public distribution claim은 계속 제외한다.

ADR-0005의 `timeout-rate-limit-hardening-preflight`는 `packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1`와 `docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md`가 추적한다. 이 descriptor는 `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지하며 public trusted signing 또는 external stable publication을 주장하지 않는다.

ADR-0005의 `timeout-rate-limit-hardening-code-level`는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-code-level-2026-05-08.md`가 추적한다. 이 slice는 Local API request limit, HTTP 429, `Retry-After`, problem-details JSON error contract를 code-level로 적용하지만 route timeout enforcement, load test, installed service config mutation, host mutation, public trusted signing, external stable publication을 주장하지 않는다.

ADR-0005의 `timeout-rate-limit-hardening-route-timeout-code-level`는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md`가 추적한다. 이 slice는 Local API GET/read route response deadline, HTTP 504, `Retry-After`, problem-details JSON error contract를 code-level로 적용하지만 mutation-route cancellation, native adapter cooperative cancellation, load test, installed service config mutation, host mutation, public trusted signing, external stable publication을 주장하지 않는다. Matrix 상태는 `timeout_rate_limit_hardening: partial-code-level-route-and-request-limit`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`이다.

ADR-0005의 `timeout-rate-limit-hardening-server-config-code-level`는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md`와 `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`가 추적한다. 이 slice는 product service plan과 native service-action SCM config의 `DesktopNode.Host.exe listen` binary path에 route timeout/request limit/burst/retry-after 기본 인자를 code-level로 연결하지만 installed service config mutation, service stop/start, load test, host mutation, public trusted signing, external stable publication을 주장하지 않는다. Matrix 상태는 `timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: code-level-product-and-native-service-plan-applied`이다.

ADR-0005의 `timeout-rate-limit-hardening-load-test-code-level`는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md`가 추적한다. 이 slice는 `DesktopNodeApiRequestProcessor` in-process 경로에서 같은 client의 `/api/v1/runtime/policy` 요청 64개를 병렬 실행해 HTTP 200 20건, HTTP 429 44건, unexpected status 0건과 `PCV_RATE_LIMIT_EXCEEDED` problem-details contract를 확인하지만 installed listener load, external load generator, installed service config mutation, host mutation, public trusted signing, external stable publication을 주장하지 않는다. Matrix 상태는 `timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: code-level-inprocess-pass`, `server_config_mutation: code-level-product-and-native-service-plan-applied`이다.

ADR-0005의 `installed-listener-external-load-rate-limit`는 `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`가 추적한다. 이 smoke는 설치된 listener에 실제 HTTP 요청 180개를 보내 HTTP 200 140건, HTTP 429 40건, unexpected status 0건, 모든 429의 `Retry-After`와 `PCV_RATE_LIMIT_EXCEEDED` problem details를 확인했다. Host mutation, public trusted signing, external stable publication을 주장하지 않는다.

## 포함 표면

- `archive/spikes/purecvisor-desktop-node/**`: Hyper-V helper, Local API, Web Console, CLI, service helper, component tests
- `packaging/windows-desktop-node/**`: product wrapper, .NET Windows Service host integration, historical WinSW compatibility boundary, WiX MSI installer, packaging tests
- `src/DesktopNode.*`: Phase 25 .NET contract/runtime/API/service/host candidate, default service host replacement slice, xUnit tests
- `docs/superpowers/**`: Desktop Node phase 설계/계획 문서

## 금지 표면

이 저장소에는 다음을 추가하지 않는다.

- Linux `purecvisorsd`
- Linux Single Edge C runtime
- KVM/libvirt/LXC/ZFS/OVS/OVN implementation
- Single Edge Web UI/API 공개 표면
- Multi Edge cluster/federation/live migration implementation

## Phase 11-25 상태

Phase 11-18 요약:

- Phase 11은 당시 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 결정을 기록했다.
- Phase 12는 `packaging/windows-desktop-node/` 아래 Service-first product wrapper를 둔다.
- Phase 13은 WinSW service wrapper와 loopback static auth boundary를 검증한다.
- Phase 14는 WiX MSI-first installer source/build/provenance와 repair/uninstall/remove-data UX를 추가한다.
- Phase 15는 DPAPI LocalMachine protected token file을 제품 기본 bearer token source로 검증한다.
- Phase 16은 JSONL first diagnostics policy, diagnostic bundle manifest, log rotation, Windows Event Log opt-in registration plan을 검증한다.
- Phase 17은 loopback 기본값, LAN preview/admin opt-in, reverse proxy/TLS 전제, non-loopback static bearer auth, firewall opt-in lifecycle을 고정한다.
- Phase 18은 manifest-first safe update/rollback/config migration 기본 구현과 검증을 제품 wrapper 경계에 둔다.

Phase 19-22 evidence 요약:

- Phase 19는 evidence-first 재판정으로 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지했지만, 2026-05-05 ADR-0004가 내부 전용 서비스 범위에서 이를 대체했다.
- DPAPI protected token, JSONL diagnostics/redaction, LAN preview policy, manifest-first update/rollback/config migration은 충족 gate로 본다.
- Phase 22는 dev/admin-smoke/rc/stable release channel, artifact naming, provenance `release_channel`, unsigned RC/stable 차단을 installer contract에 일부 반영했고 ADR-0002로 현재 적용 결정에 채택했다.
- `0.23.8-rc.1` signed RC MSI evidence는 local test certificate 기준이며 GA를 닫지 않는다.
- 2026-04-30에는 elevated MSI lifecycle, Hyper-V product-flow lifecycle, release approval/signing preflight, firewall cleanup, 운영/Event Log source lifecycle evidence를 draft-ready 기준으로 기록했다.
- 2026-05-01에는 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` MSI build, elevated MSI lifecycle, product-wrapper update/rollback/config migration, final MSI restore install evidence를 추가했다.

ADR-0003은 public CA, AD CS, Intune, MDM 없이 전용 internal Root CA + leaf Code Signing certificate 기반 `RequireSigned` signing trust model을 채택한다.

- `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`은 `0.23.10-rc.1` internal enterprise `RequireSigned` MSI build, LocalMachine trust import, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS, boot time unchanged를 기록한다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`은 `0.38.4-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`을 기록한다. MSI SHA-256은 `0b4c60d60098f89bd0adea4d183a5224d32b862e9bf69bd6dbaa41077377e8b9`, provenance commit은 `6bbb39f0a3a271e4a1187ce7de2014e009977425`, signing trust model은 `InternalEnterprise`다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`은 `0.38.7-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`을 기록한다. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`다.
- 이 결과는 internal trusted signing evidence이며 public trusted signature 또는 외부 stable publication을 의미하지 않는다.
- 2026-05-05 ADR-0004는 aggregate gate closure와 internal stable release/update/rollback evidence를 근거로 Desktop Node를 내부 전용 서비스 범위의 GA-ready 제품 런타임으로 확정했다.

Phase 24 후보는 Local API job runtime의 public boundary를 고정한 개발 작업이다. 이 후보는 `archive/spikes/purecvisor-desktop-node/api/**` 경계 안에서 runtime policy와 job behavior를 안정화했으며, 당시 공개 release boundary, 제품 런타임 승격 판단, signed/elevated evidence gate를 변경하지 않았다. 현재 제품 런타임 승격 판단은 ADR-0004가 소유한다.

Phase 25 후보는 C#/.NET contract/runtime/API/service/host, TypeScript Web Console, PowerShell Windows adapter 역할 분리를 정의했다. ADR-0004 적용 이후 active product runtime/ops는 C#/.NET native path를 current decision으로 둔다.

초기 Phase 25 slice:

- `src/DesktopNode.Contracts/**`, `src/DesktopNode.Runtime/**`, `src/DesktopNode.Api/**`, `src/DesktopNode.Service/**`는 side-by-side contract/scaffold였다.
- Web Console TypeScript parity scaffold는 2026-05-03 served asset/root migration slice에서 `web/src/served-app.ts`가 `web/app.js`를 생성하는 제품 Web Console source로 승격됐다.

현재 Phase 25 경계:

- `DesktopNode.Host.exe`는 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner다.
- 기본 loopback surface는 Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...` 분리다. Web static listener는 `/pcv-config.js`로 API origin을 주입하고 API listener는 bearer token과 CORS origin contract를 유지한다. 이 port split 자체는 public release boundary를 승격하지 않는다.
- `src/DesktopNode.Api/**`는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation routes, checkpoint create/restore/delete native mutation routes, job control, JSON job store save/load/recovery를 처리한다. Current served Hyper-V mutation route contract는 `dotnet-native` product path이며 PowerShell helper fallback을 사용하지 않는다.
- `host.status`는 C# registry/WMI/service/admin native read adapter가 처리한다.
- `network.inventory`는 C# native WMI adapter가 직접 처리하며 switch topology parity field가 불완전하면 native structured failure를 반환한다.
- `vm.list`는 C# native WMI adapter가 직접 처리한다. Empty inventory는 유효한 success이며, VM identity/state, CPU/startup memory/generation/checkpoint count, storage/network parity가 불완전하면 PowerShell helper fallback 없이 native structured failure를 반환한다.
- `GET /api/v1/vms/{id}`는 native `vm.list` result를 사용하며 native inventory miss/failure를 helper 재시도 없이 반환한다.
- `GET /api/v1/vms/{id}/checkpoints`는 native VM inventory와 WMI snapshot association을 사용하며 native VM/checkpoint parity failure를 helper 재시도 없이 반환한다.
- `POST /api/v1/vms/{id}/start`, `POST /api/v1/vms/{id}/shutdown`, `POST /api/v1/vms/{id}/poweroff`, `POST /api/v1/vms/{id}/restart`는 .NET request processor queue를 거친 뒤 C# WMI `Msvm_ComputerSystem.RequestStateChange` adapter가 직접 실행하며 PowerShell helper fallback을 사용하지 않는다.
- `POST /api/v1/vms`는 native VM create adapter가 처리한다. 이번 native product path는 Hyper-V Generation 2 create만 지원하고 Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다.
- `DELETE /api/v1/vms/{id}`는 .NET request processor queue를 거친 뒤 C# WMI `DestroySystem` adapter가 직접 실행한다. Missing VM은 idempotent `action=absent` success이며, managed marker가 없는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다. `0.30.1-admin-smoke` installed destructive smoke에서 managed delete, repeat absent, unmanaged guard block, cleanup/no-reboot evidence를 확인했다.
- `POST /api/v1/vms/{id}/checkpoints`, `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore`, `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}`는 .NET request processor queue를 거친 뒤 C# WMI snapshot service adapter가 직접 실행하며 PowerShell helper fallback을 사용하지 않는다.
- Existing PowerShell Local API와 Hyper-V helper는 `archive/spikes/**` component/archive baseline으로만 남는다. Active product `DesktopNode.Host.exe listen`은 `--helper-script`를 `PCV_HOST_HELPER_SCRIPT_RETIRED`로 차단하고 product manifest는 `helper_script`/`api_script` path를 기록하지 않는다. Legacy WinSW PowerShell Local API generation은 retired error로 차단한다. Served `web/app.js`는 TypeScript build output이다.
- Service product ops는 `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed|data-root-remove` native SCM/data-root action path를 갖는다. `remove-installed --remove-data`는 handoff descriptor만 반환하고 실제 ProgramData allowlist 삭제는 service absent precondition의 `data-root-remove --remove-data`가 맡는다. `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`의 `0.30.3-admin-smoke`는 installed destructive create/configure/repair/delete/remove-data evidence를 추가했지만, unsigned `AllowUnsignedDev` admin-smoke이므로 public release boundary나 GA 승격 판단을 닫지 않는다.
- Unsupported future job store schema는 quarantine/move 없이 409 blocked diagnostics/no-mutation으로 처리한다. Schema v2는 2026-05-06 code-level `job-store-v1-to-v2` migration target으로 지원하며, v99 같은 더 새로운 schema는 계속 blocked/no-mutation이다. Config/job store migration apply actual path는 code-level product operation이며, installed destructive admin smoke는 2026-05-07 `0.38.6-admin-smoke`에서 PASS됐다. Public release evidence는 계속 별도이며 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
- Network update source gate와 updater catalog/channel resolver는 code-level internal packaging evidence다. file/HTTPS JSON catalog는 selected channel의 package URI/SHA-256을 service stop 전에 검증하고 기존 source gate로 넘기지만, external publication service, public trusted signing, 외부 stable publication, installed destructive catalog update smoke는 이 경계에 포함하지 않는다. Evidence는 `docs/ga-ready/evidence/network-download-update-source-gate-2026-05-07.md`와 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`다.
- Update filesystem rollback은 code-level internal packaging evidence다. Product root backup 이후 copy/config/start/health failure에서 previous root restore를 시도하지만, post-crash resume/reconcile, service/data/config/job-store 전체 transaction manager, public trusted signing, 외부 stable publication은 이 경계에 포함하지 않는다. Evidence는 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`다.
- Installer publication descriptor는 code-level internal packaging evidence다. `.publication.json` sidecar는 artifact SHA/provenance와 publication boundary를 연결하고 public trusted signing/external stable publication을 `not-claimed`, Burn/MSIX/winget/catalog publication을 미실행 상태로 기록하지만, Burn bootstrapper, MSIX, winget manifest submission, external publication service, public stable channel은 이 경계에 포함하지 않는다. Evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`다.
- Public distribution readiness preflight는 code-level dry-run evidence다. `New-PcvPublicDistributionReadiness.ps1`는 `.publication.json`에서 winget manifest preview와 `winget validate` manual follow-up을 산출하지만, actual validation/submission/public trusted signing/external stable publication은 실행하지 않는다. Evidence는 `docs/ga-ready/evidence/public-distribution-readiness-preflight-2026-05-07.md`다.
- Public distribution ops execution bundle은 local non-mutating bundle evidence다. `New-PcvPublicDistributionOperationsBundle.ps1`는 ADR-0005 preflight generators를 한 artifact root로 실행/수집하지만 Burn build, winget submission, catalog upload, clean-host public signed update/rollback smoke, Credential Manager/Event Log/TLS/token mutation, public trusted signing, external stable publication은 실행하거나 주장하지 않는다. Evidence는 `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`와 `artifacts/public-distribution-ops-execution-bundle-20260509-0391`다.
- Winget manifest compliance preflight는 code-level dry-run evidence다. `New-PcvWingetManifestCompliancePreflight.ps1`는 generated singleton manifest preview를 offline compliance로 검증하지만, winget CLI validation/submission/public trusted signing/external stable publication은 실행하지 않는다. Evidence는 `docs/ga-ready/evidence/winget-manifest-compliance-preflight-2026-05-08.md`다.
- Updater catalog publication preflight는 code-level dry-run evidence다. `New-PcvUpdaterCatalogPublicationPreflight.ps1`는 selected HTTPS catalog channel에서 catalog publication preview와 SHA-256 sidecar를 산출하지만, catalog upload/public endpoint validation/public trusted signing/external stable publication은 실행하지 않는다. Evidence는 `docs/ga-ready/evidence/updater-catalog-publication-preflight-2026-05-07.md`다.
- Public signed update/rollback smoke preflight는 code-level dry-run evidence다. `New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`는 selected catalog channel에서 clean-host smoke plan preview를 산출하지만, install/update/rollback execution, public trusted signing, external stable publication은 실행하지 않는다. Evidence는 `docs/ga-ready/evidence/public-signed-update-rollback-smoke-preflight-2026-05-08.md`다.
- Burn bootstrapper preflight는 code-level dry-run evidence다. `New-PcvBurnBootstrapperPreflight.ps1`는 packaging publication descriptor와 HTTPS MSI URL에서 WiX Burn authoring preview를 산출하지만, bundle build/chained lifecycle smoke/public trusted signing/external stable publication은 실행하지 않는다. Evidence는 `docs/ga-ready/evidence/burn-bootstrapper-preflight-2026-05-07.md`다.
- MSIX packaging feasibility preflight는 code-level dry-run evidence다. `New-PcvMsixPackagingFeasibilityPreflight.ps1`는 MSIX package manifest preview를 산출하지만, 그 preflight 자체는 package build/install/update/remove/public trusted signing/external stable publication을 실행하지 않는다. Evidence는 `docs/ga-ready/evidence/msix-packaging-feasibility-preflight-2026-05-07.md`다. 후속 internal lifecycle smoke `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`는 `PureCVisor.DesktopNode.MsixSmoke` package build/install/update/remove를 PASS로 확인했지만 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Web Console browser fixture parity는 served `app.js`를 Node `vm` 최소 DOM과 fixture Local API 응답으로 실행하는 code-level/npm 검증이며, 실제 browser/dev server/Local API/Hyper-V mutation evidence가 아니다.

Evidence 기준:

- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`: native `network.inventory` route 포함 설치본 unsigned admin-smoke evidence.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`: MSI repair 재생성, topology parity fallback, request processor 직렬화 포함 설치본 unsigned admin-smoke evidence.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`: native `host.status`, guarded `network.inventory`, service-action, MSI lifecycle, Hyper-V API route smoke PASS evidence.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-113517`: native `vm.list` WMI query guard 수정 후 service-action, MSI lifecycle, Hyper-V API route smoke PASS evidence.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-115135`: `GET /api/v1/vms/{id}` native-first slice 이후 service-action, MSI lifecycle, Hyper-V API route smoke PASS evidence.
- `artifacts/installed-nonmutating-checkpoint-list-20260503-121824`: `0.27.5-admin-smoke` current commit 설치 후 GET-only checkpoint list/missing VM route smoke PASS evidence.
- `artifacts/installed-vm-create-checkpoint-list-20260503-122705`, `artifacts/installed-checkpoint-lifecycle-cleanup-20260503-124330`: 사용자 explicit opt-in 범위의 VM create, checkpoint create/delete, VM poweroff/delete cleanup smoke PASS evidence.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-140824`: `0.27.6-admin-smoke` runtime dispatch boundary contract 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS evidence. Final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이며 당시 installed runtime policy는 `dispatch.native_probe_operations=[host.status,network.inventory,vm.list,checkpoint.list]`와 `dispatch.mutation_dispatch=helper-process-direct`를 보고했다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283`: `0.28.3-admin-smoke` checkpoint create/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS evidence. Installed runtime policy는 `dispatch.native_mutation_operations=[checkpoint.create,checkpoint.delete]`와 `dispatch.mutation_dispatch=native-checkpoint-mutation-plus-helper-process-remainder`를 보고했고, final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `artifacts/routeparity-service-msi-hyperv-restore-mutation-20260503-0286`: `0.28.6-admin-smoke` checkpoint create/restore/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS evidence. Installed restore smoke는 `vm.poweroff-before-restore` 최소 안정 조건에서 `{ vm_name, name, action=restore }` payload를 확인했고 installed runtime policy는 `dispatch.native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`를 보고했다. Final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `artifacts/routeparity-service-msi-hyperv-vm-create-restart-shutdown-20260503-0290`: `0.29.0-admin-smoke` VM create/start/restart/poweroff 및 checkpoint create/restore/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS evidence. Final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301`: `0.30.1-admin-smoke` VM delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS evidence. Managed VM delete는 `action=delete`, repeat delete는 `action=absent`, unmanaged VM delete는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 blocked, final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `artifacts/service-action-status-start-stop-20260504-002359`: installed `DesktopNode.Host.exe service-action status/stop/start/status` native SCM smoke PASS evidence. Service owner verified, stopped/running state observation, restart 후 runtime policy health `200`, final service `Running`, boot time unchanged다.
- `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`: `0.30.3-admin-smoke` service/data-root handoff smoke PASS evidence. Service 존재 중 `data-root-remove --remove-data`는 `PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS`로 차단됐고, `remove-installed --remove-data`는 handoff만 반환했으며, service absent 이후 `data-root-remove --remove-data`가 allowlist data-root 항목만 삭제하고 non-allowlist log를 보존했다. MSI lifecycle과 installed Hyper-V route smoke도 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`: GA-ready aggregate gate closure는 closed다. 2026-05-05 당시 재계산 값은 GA-scope blocked row 0개, PowerShell-backed current owner 0개, active product `spikes/**` reference 0개, future implementation exclusion 2개였다. 2026-05-07 `0.38.6-admin-smoke` 이후 현재 route matrix 재계산 값은 GA-scope `current-route` 18개, `product-operation` 24개, `future-route` exclusion 0개, `current-native` 42개, blocked/ga-ready-candidate 0개다. Route parity smoke import, MSI installer payload spike staging, standalone product wrapper asset staging, post-reboot active spike command, docs required verification command direct spike path, product wrapper protected-token spike service module import, Event Log source registration/removal current owner migration, firewall/trust-store current owner migration, actual registry registration/removal evidence, current native LAN/firewall/Event Log/internal trust-store evidence, physical archive move evidence, internal stable release/update/rollback evidence가 닫혔다. 이 closure는 ADR-0004 current decision 승격 근거이며, public trusted signing 또는 외부 stable publication claim이 아니다.
- `artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330`: 사용자 관리자 opt-in으로 `0.33.0-admin-smoke` Service/MSI/Hyper-V mutation과 row-isolated firewall/trust-store mutation을 실행했다. Service-action, MSI lifecycle, installed Hyper-V route smoke, firewall-only create/enable/remove, trust-store-only LocalMachine Root/TrustedPublisher import/remove가 PASS였고 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다. 이 evidence는 `AllowUnsignedDev` admin-smoke와 scoped test certificate trust-store mutation이며 public trusted signing 또는 외부 stable publication evidence가 아니다. 제품 런타임 승격 판단은 2026-05-05 aggregate closure와 ADR-0004가 소유한다.
- `artifacts/os-mutation-gates-20260505-003459-0341`: 사용자 fast-mode 관리자 opt-in으로 `0.34.1-admin-smoke` current native MSI/firewall/LAN/internal trust-store gate를 실행했다. MSI provenance commit은 `6f97a24aa2bdfacf33d7bd987559eb85e363e119`, follow-up firewall lookup hardening commit은 `49a06acd3493066a10ec26fe541d5d8be1005c2b`, MSI SHA-256은 `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`다. MSI lifecycle은 exit `0`, native firewall rule은 enable/remove 후 final absent, LAN IP listener는 bearer token runtime policy `HTTP 200`, internal Root/TrustedPublisher cert는 remove 후 final restore present를 확인했다. 이 evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다. 제품 런타임 승격 판단은 2026-05-05 aggregate closure와 ADR-0004가 소유한다.
- `artifacts/os-mutation-gates-20260505-033503-0354`: 사용자 재승인으로 `0.35.4-admin-smoke` 실행 당시 HEAD native MSI/firewall/LAN/internal trust-store gate를 fresh 실행했다. MSI provenance commit은 `744a15536569e89f948927bea9179fc0eeae3ff4`, MSI SHA-256은 `bf7d0d2bd83545e83fbdf0dfb96b715f8e09471474445ae1c0db1d076be2c1e4`다. MSI install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall, firewall enable/remove, LAN runtime policy와 Web root `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final restore는 internal signed stable `0.35.2`, final service loopback `Running`, firewall final count `0`, installed DisplayVersion `0.35.2`, boot time unchanged로 끝났다. 이 evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`와 `artifacts/os-mutation-gates-20260505-101659-0355-final`: 사용자 재승인으로 `0.35.5-admin-smoke` 실행 당시 HEAD native Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate를 fresh 실행했다. MSI provenance commit은 `2fb38f20a8c74433684345ded8a33ba16a863621`, MSI SHA-256은 `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, firewall enable/remove, LAN runtime policy와 Web assets `HTTP 200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service loopback `Running`, firewall final count `0`, Event Log source absent, installed DisplayVersion `0.35.5`, boot time unchanged로 끝났다. 이 evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun`와 `artifacts/os-mutation-gates-20260505-170454-0356-rerun`: 사용자 재승인으로 `0.35.6-admin-smoke` 실행 당시 code HEAD native Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate를 fresh 실행했다. MSI provenance commit은 `cc723e28ed62f6f1c5e49c74ca68b87d0f1b8b3a`, MSI SHA-256은 `a24de44049519dea8405854a17272ebb362b061ff03a051cd61fb31669bc7d02`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, firewall enable/remove, LAN runtime policy와 Web assets `HTTP 200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service loopback `Running`, firewall final count `0`, Event Log source absent, installed DisplayVersion `0.35.6`, boot time unchanged로 끝났다. 이 evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`: 사용자 재승인으로 `0.35.7-admin-smoke` 현재 HEAD native Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate를 fresh 실행했다. MSI provenance commit은 `2ec9e71d45b702e106824c86500cd6152b18fab7`, MSI SHA-256은 `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, firewall enable/remove, LAN bearer runtime policy와 Web assets `HTTP 200`, Event Log register/remove, config-migration-apply blocked/no-mutation descriptor, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service loopback `Running`, firewall final count `0`, Event Log source absent, installed DisplayVersion `0.35.7`, boot time unchanged로 끝났다. 이 evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/routeparity-service-msi-hyperv-dotnet100-20260505-0.36.0`: 사용자 승인 범위에서 `0.36.0-admin-smoke` active product .NET 100% cleanup Service/MSI/Hyper-V route parity를 rerun했다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `70cb8b720588c6ef69aca59fed48f870865d7bca8c7a4ea8e623ab6b6e99d048`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service loopback `Running`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다. 이 evidence는 `AllowUnsignedDev` admin-smoke이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, step `timed_out=false`, `exit_code=0`, heartbeat lines `25`다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`, signing mode는 `AllowUnsignedDev`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`: 사용자 관리자 opt-in으로 `0.37.0-admin-smoke` full admin host mutation gate를 Batch Supervisor 아래에서 완료했다. MSI provenance commit은 `485b1a7338fb2b682c3964c858ccc13c322950d7`, MSI SHA-256은 `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`다. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store mutation pass를 기록하지만 public trusted signing, public stable channel, external stable publication evidence는 아니다. First-attempt MSI repair `1603`은 recovered transient로 기록했다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389`, `artifacts/os-mutation-gates-batch-profile-20260508-202255-0389`: 사용자 관리자 opt-in으로 `0.38.9-admin-smoke` full admin host mutation gate를 Batch Supervisor 아래에서 완료한 historical evidence다. MSI provenance commit은 `159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce`, MSI SHA-256은 `86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store mutation pass를 기록하지만 public trusted signing, public stable channel, external stable publication evidence는 아니다.
- `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`: 사용자 관리자 opt-in으로 `0.39.0-admin-smoke` MSI/service installed listener rerun을 완료했다. MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode는 `AllowUnsignedDev`다. Final SCM `PathName`은 diagnostic bundle/hardening 인자를 포함했고 protected-token diagnostic bundle create/download는 POST `201`, GET `200`, redaction PASS였다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니며 public trusted signing, public stable channel, external stable publication evidence가 아니다.
- `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`: 사용자 관리자 opt-in으로 `0.39.0-admin-smoke` installed listener 후속 firewall/trust-store/LAN/Event Log OS mutation gate를 완료했다. OS summary는 `host_mutation_performed=true`, `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`이며 LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, final firewall count `0`, Event Log source absent, internal trust Root/TrustedPublisher present, boot time unchanged를 확인했다. 이 evidence는 public trusted signing, public stable channel, external stable publication evidence가 아니다.
- `artifacts/msi-update-package-20260509-0391`: 사용자 관리자 opt-in으로 `0.39.1-admin-smoke` MSI/update package apply를 완료했다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, signing mode는 `AllowUnsignedDev`다. Elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 확인했다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`이며 이 evidence는 public trusted signing, public stable channel, external stable publication evidence가 아니다.
- 2026-05-16 `manual-admin-campaign-2026-05-16-04222-04223`은 `0.42.22-admin-smoke -> 0.42.23-admin-smoke` 이전 닫힌 package-pair PASS evidence다. Target MSI SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`, update ZIP SHA-256은 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`다. Descriptor `manual-admin-campaign-descriptor-20260516-04222-04223-closed`는 `missing_count=0`, `not_pass_count=0`이고, public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-16 `full-admin-host-mutation-gate-2026-05-16-04222-hostmutation`은 `0.42.22-admin-smoke` historical full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04222`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04222`, `artifacts/os-mutation-gates-batch-profile-20260516-04222`이고 full-gate MSI SHA-256은 `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`, clean package MSI SHA-256은 `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`, provenance commit은 `8a38995cc25a888f64473e9a2869740949ad6b24`, signing mode는 `AllowUnsignedDev`다. Product wrapper repair, Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고 installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04222`, route/OS child evidence `available`, errors `0`, Runtime/API registry bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를 확인했다. Current-card artifact는 `artifacts/installed-current-card-20260516-04222-fullgate`이고, 설치본 Web/TUI/CLI current-card smoke는 `artifacts/installed-operator-surface-current-card-20260516-04222`에서 PASS했다. final service `Running`, installed manifest `0.42.22-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`를 확인했다. Public-boundary post-merge는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass.md` run `25952150476` / job `76291983316`에서 PASS했다. 2026-05-16 04223이 current latest이고 04221 및 이전 evidence도 historical predecessor로 보존한다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Previous 04221 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221`, `artifacts/os-mutation-gates-batch-profile-20260516-04221`, full-gate MSI SHA-256 `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`, provenance commit `3b8c48deb4c31675f6fce46c320703f23c27c131`로 보존한다.
- Historical 04220 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`, full-gate MSI SHA-256 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`로 보존한다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`은 `0.42.9-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`이고 full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고 installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260513-040213-0429`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.9-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`: `0.38.8-admin-smoke` installed destructive update/rollback smoke는 elevated shell에서 PASS했다. MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 current root를 `0.38.6-admin-smoke`로 복원하고 `0.38.8-admin-smoke` root를 `DesktopNode.failed` diagnostics로 보존했다. `host_mutation_performed=true`이며 이 evidence는 `AllowUnsignedDev` admin-smoke라 public trusted signing, public stable channel, external stable publication evidence가 아니다.
- `docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md`: public trusted signing은 제외하고 release/LAN/OS gated operation의 preapproval boundary를 기록한다. 후속 Event Log/firewall/trust-store actual mutation evidence, firewall/trust-store native owner code-level evidence, `0.34.1-admin-smoke`, `0.35.4-admin-smoke`, `0.35.5-admin-smoke`, `0.35.6-admin-smoke`, `0.35.7-admin-smoke` native LAN/internal trust-store evidence, internal stable release/update/rollback evidence는 ledger와 aggregate closure에 분리 기록한다. Public trusted signing과 외부 stable publication은 excluded/not-claimed이며, ADR-0004는 2026-05-05 내부 전용 서비스 current decision으로 적용됐다.
- Installer-ISO VM의 `vm.shutdown`은 guest shutdown integration 미준비 상태를 `PCV_VM_SHUTDOWN_NOT_AVAILABLE` structured failure로 반환했다.
- Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD 기반 Gen1 differencing VM으로 확인했다. Installed Local API `vm.shutdown` job은 `succeeded`, final VM state는 `Off`, smoke VM/ProgramData cleanup은 완료 상태다.
- 2026-05-03 VM summary/storage/network native parity code-level slices는 WMI CPU/startup memory/generation/checkpoint count, storage path, network switch mapping을 추가했지만, 이 slice 단독으로 release boundary나 제품 런타임 승격 판단을 바꾸지 않았다. 이후 2026-05-05 aggregate closure/ADR-0004 supporting evidence에 편입됐다.
- 2026-05-03 Web Console browser fixture parity code-level slice는 generated manifest/static parity와 served `app.js` initial render smoke를 npm 검증으로 묶었지만, 이 slice 단독으로 release boundary나 제품 런타임 승격 판단을 바꾸지 않았다. 이후 2026-05-05 aggregate closure/ADR-0004 supporting evidence에 편입됐다.
- `artifacts/dotnet-host-admin-smoke-20260501-213444`, `0.26.x-admin-smoke`, `0.27.x-admin-smoke`, `0.33.0-admin-smoke` evidence는 unsigned `AllowUnsignedDev` admin-smoke 범위이며 공개 release boundary, GA 승격 판단, public trusted signing 또는 외부 stable publication gate를 닫지 않는다. Scoped test certificate trust-store mutation도 public trusted signing evidence가 아니다.

## 검증

저장소 경계, phase 상태, 검증 문서가 바뀌면 다음을 실행한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Legacy root boundary Pester는 component/archive baseline 소유권으로 분리하며, public release boundary의 active verification command에는 `spikes/**` Pester path를 넣지 않는다.
