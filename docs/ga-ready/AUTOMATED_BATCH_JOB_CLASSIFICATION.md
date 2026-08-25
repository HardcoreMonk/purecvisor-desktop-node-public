# 후속 Queue 및 자동 Batch Job 분류

## 2026-07-16 CLI/Web-only operator surface operational closure

- ADR-0011과
  `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`의 source/docs
  검증은 non-mutating `AUTO-REPO`다.
- 제거된 TUI installed smoke는 active queue에서 제외하며 dated execution 기록만 historical
  predecessor로 보존한다.
- `0.42.65-admin-smoke` package/fullgate/actual-VM functional correctness/CLI-Web installed
  current-card는 승인된 `MANUAL-ADMIN` 실행으로 PASS했다. Installed update/rollback
  compensation과 public release는 이 closure에 포함되지 않는다.

## 2026-05-19 historical predecessor closure 진행 상태

최신 installed operational anchor는 `0.42.34-admin-smoke` Runtime/API current evidence rollup이며 `full-admin-host-mutation-gate-20260519-04234`, `manual-admin-campaign-descriptor-20260519-04232-04234-closed`, Host Ops lifecycle descriptor bridge `host-ops-lifecycle-descriptor-bridge-v1`를 기준으로 한다. Host Ops bucket contract는 `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract는 `host-ops-web-diagnostics-bucket-table-v1`다. Target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, payload aggregate SHA-256은 `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`다. PR #156/#155/#154 public-boundary evidence와 0.42.32 이하 package-pair는 historical predecessor로 보존한다. Installed account/noVNC smoke는 0.42.29 historical PASS이며 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이고 public trusted signing과 외부 stable publication evidence가 아니다.

Historical `0.42.31-admin-smoke -> 0.42.32-admin-smoke` predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, descriptor `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 보존한다. Historical `0.42.30-admin-smoke -> 0.42.31-admin-smoke` predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04230-04231.md`, `full-admin-host-mutation-gate-20260518-04231`, descriptor `manual-admin-campaign-descriptor-20260518-04230-04231-closed`로 보존한다.

Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`, descriptor `manual-admin-campaign-descriptor-20260517-04226-04227-closed`, target MSI SHA-256 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`, update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`, provenance commit `69aba3eb3ff08c843f1a481818ddc86eac2f019b`로 보존한다. Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Operator Surface predecessor는 full admin host mutation batch `full-admin-host-mutation-gate-20260517-04228`, target MSI SHA-256 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, clean package MSI SHA-256 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`, update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit `b9676f6dc37d667ae0d60367e9f4e576a27e3864`로 보존한다. PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`이며 public trusted signing과 외부 stable publication evidence가 아니다.

Historical `0.42.28-admin-smoke -> 0.42.29-admin-smoke` selector/package-chain predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`, descriptor `manual-admin-campaign-descriptor-20260517-04228-04229-closed`, target MSI SHA-256 `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`, update ZIP SHA-256 `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`, provenance commit `d306712ad671c8a00d5c560765b8952e24a07502`로 보존한다. PR #153 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`, run `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`이며 public trusted signing과 외부 stable publication evidence가 아니다.


classification_date: 2026-05-10
last_updated: 2026-05-19
status: approved-for-document-based-progression
approval_basis: user-approved-document-based-progression-2026-05-10
host_mutation_performed_by_classification: false
public_release: not-claimed

이 문서는 Windows-only Desktop Node 저장소의 후속 Queue와 batch 가능한 작업을
분류한다. 이 문서 자체는 새 실행 evidence가 아니며, evidence 소유권은 연결된
evidence 파일과 artifact record에 남는다.

최신 installed operational anchor는 `0.42.34-admin-smoke` Runtime/API current
evidence rollup이며, `full-admin-host-mutation-gate-2026-05-19-04234-hostmutation`,
`installed-operator-surface-current-card-2026-05-19-04234`,
`admin-smoke-package-2026-05-19-04234`가 최신 installed/package evidence다.
최신 닫힌 MANUAL-ADMIN package-pair는
`manual-admin-campaign-2026-05-19-04232-04234`이며 descriptor
`manual-admin-campaign-descriptor-20260519-04232-04234-closed`는
`missing_count=0`, `not_pass_count=0`으로 PASS다. Public-boundary CI guard는
PR #156 post-merge run `26017721669`, job `76471545641`, head SHA
`a4509c552c003ee0fc87b54b26529686e6dfeb84`에서 PASS했다. PR #155 post-merge evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`,
run `26013384587`, job `76458402221`, head SHA
`2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f`는 historical predecessor다. PR #154 post-merge evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md`,
run `25989986761`, job `76394250912`, head SHA
`d7f611dfc14a9fa1507f936559209513272b585a`는 historical predecessor다. PR #153 post-merge evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`,
run `25987705546`, job `76388078056`, head SHA
`d306712ad671c8a00d5c560765b8952e24a07502`는 historical predecessor다. PR #152 post-merge evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md`,
run `25985786230`, job `76382711230`, head SHA
`ca07514097f4e9524a7f3630d321c9666593c962`는 historical predecessor다. Earlier
`manual-admin-campaign-descriptor-2026-05-16-04225-04226`는 readiness PASS지만
`missing_count=4`, `not_pass_count=1`이었던 initial blocked descriptor로 보존한다.
Current/historical evidence 중복은 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`로 압축한다.
Post-04226 contract follow-up evidence는
`docs/ga-ready/evidence/post-04226-ledger-contract-followup-2026-05-17.md`이며
`current_card_descriptor_batch_id`, `descriptor_schema_version=2`,
`manual-admin-descriptor-generation-contract-v2`, Batch Supervisor `-DescriptorBatchId`
전달을 고정했다. 이 branch 이전 product payload
change는 없었지만 이 branch 자체가 Runtime/API/Web/packaging payload 변경이므로
다음 package/fullgate/package-pair trigger는 `post-04226-ledger-contract-merge`다.
PR #156 follow-up branch 자체는 product payload 변경이 없어 package chain을 열지 않았고, 이후 사용자 승인으로
`0.42.32-admin-smoke -> 0.42.34-admin-smoke` package chain을 닫았다. 다음 package-pair 후보는
`pending-next-product-payload-after-04234-package-pair`로 유지한다.

## 분류 규칙

`AUTO-REPO`는 repository-only 자동화다. 모든 step이 `requires_admin=false`,
`mutates_host=false`이고 installed service state, upload, signing, publication,
reboot, scheduled task, Hyper-V/firewall/trust-store/LAN/Event Log mutation을
건드리지 않을 때 일반 repo runner에서 unattended로 실행할 수 있다.

`AUTO-PREFLIGHT`는 local descriptor, readiness, plan-preview 자동화다. 로컬
evidence artifact를 쓸 수 있지만 `host_mutation_performed=false`,
`public_release=not-claimed`를 유지해야 하며, 없는 외부 credential 또는 public
endpoint를 실제 실행으로 승격하지 않는다.

`AUTO-INSTALLED-READONLY`는 이미 설치된 service 또는 console을 조회만 하는
자동화다. service configuration, token, account, VM state, OS state, product file을
바꾸지 않는 dedicated installed-smoke node에서만 사용한다.

`MANUAL-ADMIN`은 해당 run에 대해 명시적 elevated operator opt-in이 있을 때만
batch-supervised 또는 scripted 실행을 허용한다. install/update/rollback, service
restart, protected ProgramData, Hyper-V/firewall/trust-store/LAN/Event Log/TLS state,
`Invoke-PcvBatchSupervisor.ps1 -AllowHostMutation`이 이 범위에 들어간다.

`BLOCKED-EXTERNAL`은 public signing, trusted timestamp, external publication,
winget submission, public stable URL, public clean-host environment 또는 ADR-0006
범위 밖 public release dependency에 막힌 항목이다. history로 보존하되 자동 실행에
넣지 않는다.

`STATUS-ONLY`는 UI/status surface 또는 evidence dashboard다. frontend 자동화로
rendering을 확인할 수 있지만, 화면에 표시된 operation을 실행하지 않는다.

Batch Supervisor 경계는 단순하다. manifest step 중 하나라도 `requires_admin=true`
또는 `mutates_host=true`이면 `Invoke-PcvBatchSupervisor.ps1`는 `-AllowHostMutation`을
요구하며, 자동 reboot와 scheduled-task command는 금지한다.
Batch Supervisor host mutation rerun은 `-AllowHostMutation`이 있는 elevated shell에서만
허용한다.

## 승인된 진행 범위

문서 기반 진행 승인은 classification, local evidence/readiness routing, non-mutating
verification을 자동으로 진행할 수 있다는 뜻이다. 그 자체로 unattended host mutation을
승인하지 않는다.

자동 진행 가능:

- `AUTO-REPO`: repository regression, browser fixture/parity, code-level test.
- `AUTO-PREFLIGHT`: public claim을 blocked/out-of-scope로 유지하는 local descriptor/readiness scan.
- `AUTO-INSTALLED-READONLY`: service/product/account/token/VM/TLS/Event Log/Credential Manager/firewall/LAN/trust-store/update/rollback/clean-host state를 바꾸지 않는 dedicated installed 조회.

별도 elevated operator opt-in 유지:

- `MANUAL-ADMIN`: installed/admin-smoke campaign.
- `BLOCKED-EXTERNAL`: public trusted signing, timestamp, external publication, winget submission, public stable URL, public signed clean-host gate.

## 현재 후속 Queue

| 후속 영역 | 현재 상태 | 분류 | 자동화 결정 |
|-----------|-----------|------|-------------|
| Frontend completion auto batches | `docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.md`의 5 staged batches / 25 work items 완료 | `AUTO-REPO`, closed | open follow-up 없음. browser fixture, Pester web tests, `npm test`, `npm run verify:parity`, `node --check`를 repo 자동화로 유지한다. |
| Automatic non-mutating regression batch | `docs/ga-ready/evidence/auto-nonmutating-regression-batch-2026-05-09.md`, `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232` PASS | `AUTO-REPO` | packaging Pester, installer Pester, web tests, npm verification, dotnet solution tests, `git diff --check` rerun 가능. |
| Public ops final 1-7 follow-up attempt | `remaining_follow_up_count=7` historical scan, public release not claimed | `BLOCKED-EXTERNAL` 및 historical `AUTO-PREFLIGHT` scan | public 실행으로 schedule하지 않는다. 필요하면 local blocked-status evidence만 재생성한다. |
| Public ops gate execution readiness | `PARTIAL_CODE_LEVEL_WITH_EXTERNAL_BLOCKERS`; private key/binding/trust-store mutation 없는 TLS lifecycle descriptor | `AUTO-PREFLIGHT` descriptor, public release gate는 `BLOCKED-EXTERNAL` | local readiness regeneration만 허용한다. public signing/upload/winget/public clean-host smoke는 unscheduled 상태다. |
| Web Console beta follow-up status | `docs/ga-ready/evidence/web-console-beta-followup-status-2026-05-09.md` PASS status surface | `STATUS-ONLY` | frontend/browser status rendering만 테스트한다. MSI/firewall/trust-store/LAN/signing/update/rollback/Credential Manager/Event Log/TLS/service-token mutation을 browser automation에서 시작하지 않는다. |
| Installed listener external load/rate-limit 및 API hardening concurrency | code-level load test PASS, installed listener load/rate-limit evidence PASS | `AUTO-REPO` in-process, dedicated installed listener load smoke는 `AUTO-INSTALLED-READONLY` | repo concurrency tests는 non-mutating 자동화로 유지한다. installed listener load smoke는 준비된 installed-smoke node에서 service state를 바꾸지 않을 때만 실행한다. |
| Full admin host mutation gate | 최신 PASS: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`, `0.42.26-admin-smoke`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04226`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226`, `artifacts/os-mutation-gates-batch-profile-20260516-04226`, full-gate MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`, signing mode `AllowUnsignedDev`, installed listener `batch_evidence.status=available`, latest batch `full-admin-host-mutation-gate-20260516-04226`, Runtime/API current evidence contract `runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge route detail count `4`; public trusted signing/external stable publication not claimed. 설치본 Web/TUI/CLI current-card smoke는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`에서 PASS했다. 04225 및 이전 PASS evidence는 historical로 보존한다. | `MANUAL-ADMIN` | elevated shell과 explicit `-AllowHostMutation`이 있을 때만 Batch Supervisor rerun 가능. unattended schedule 금지. |
| Clean-host Windows Update NoContact recovery guard | `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md` code-level PASS. `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`가 Windows Update reboot 후 heartbeat `NoContact` + CPU idle 상태를 snapshot으로 남기고 threshold 초과 시 한 번만 VM power cycle recovery를 수행하도록 보강됐다. Summary key는 `automatic_recovery_performed`, `recovery_actions`, `WindowsUpdateNoContactRecoverySeconds`다. | guard code는 `AUTO-REPO`; 실제 clean-host execution은 `MANUAL-ADMIN` | code/test/docs 검증은 자동화 가능하다. Dedicated VM 생성, Windows Update, MSI install/update/rollback, recovery action 실행은 manual-admin campaign 안에서만 허용한다. |
| Post-04212 follow-up triage | `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md` PASS. `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이 없어 `0.42.13-admin-smoke` package build, full admin host mutation, package-pair campaign을 열지 않았다. Decision token은 `package_build_decision=deferred-until-next-product-payload-change`, `full_admin_host_mutation_campaign_decision=not-run-no-product-payload`, `manual_admin_package_pair_campaign_decision=deferred-until-next-product-payload-change`다. Clean-host recovery summary key는 다음 실제 run의 `recovery_actions`와 `automatic_recovery_performed`로 판정한다. | `AUTO-PREFLIGHT` triage evidence | host mutation, clean-host VM 실행, MSI/update package build를 수행하지 않는다. 다음 product payload 변경 전까지 current claim은 04212 evidence가 소유한다. |
| Post-04212 `1-2-3-4-5` current-card follow-up | `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md` PASS, status `pass-dashboard-current-card-smoke-deferred-product-chain`. `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 `src`, `web`, product wrapper, installer payload 변경이 없어 package/host mutation chain은 열지 않았고, Web Console Dashboard/Evidence current-card smoke PASS만 `artifacts/web-console-current-card-20260514-04212-rerun-followup`에서 실행했다. 당시 UI는 `full-admin-host-mutation-gate-20260514-04212-rerun`, `0.42.12-admin-smoke`를 표시했고 `0.42.13-admin-smoke` package build는 보류했다. | `AUTO-PREFLIGHT` + browser smoke evidence | host mutation, clean-host VM 실행, MSI/update package build를 수행하지 않는다. Browser smoke는 installed listener current-card 표시만 검증한다. |
| Manual-admin 04211→04212 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md` PASS. `0.42.11-admin-smoke -> 0.42.12-admin-smoke` installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다. Target MSI SHA-256은 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, update ZIP SHA-256은 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`다. | `MANUAL-ADMIN`, closed | 이미 실행된 historical evidence로 보존한다. public trusted signing 또는 외부 stable publication evidence가 아니다. |
| Manual-admin 04212→04213 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md` PASS. `0.42.12-admin-smoke -> 0.42.13-admin-smoke` installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다. Target MSI SHA-256은 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`, update ZIP SHA-256은 `638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9`다. Clean-host는 Windows Update `NoContact` recovery summary key `automatic_recovery_performed=true`, `recovery_actions=1`을 남겼다. | `MANUAL-ADMIN`, closed | 이미 실행된 evidence로 보존한다. 재실행은 elevated operator opt-in과 새 artifact root가 있을 때만 허용한다. |
| Manual-admin 04214→04215 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04214-04215.md` PASS. `0.42.14-admin-smoke -> 0.42.15-admin-smoke` readiness, installed update/rollback, Windows Update NoContact recovery 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다. Target MSI SHA-256은 `80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7`, update ZIP SHA-256은 `06f5879431bac90da6da09f243c1e91c6bb875358779e4cedc98a9a3860dad6b`다. Clean-host는 `automatic_recovery_performed=true`, `recovery_actions=1`을 남겼다. | `MANUAL-ADMIN`, closed | 이미 실행된 evidence로 보존한다. 재실행은 elevated operator opt-in과 새 artifact root가 있을 때만 허용한다. |
| Manual-admin 04216→04218 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md` PASS. `0.42.16-admin-smoke -> 0.42.18-admin-smoke` readiness, installed update/rollback, Windows Update NoContact recovery 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다. Target MSI SHA-256은 `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`, update ZIP SHA-256은 `8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd`다. Clean-host는 `automatic_recovery_performed=true`, `recovery_actions=1`을 남겼다. | `MANUAL-ADMIN`, closed | 이미 실행된 evidence로 보존한다. 재실행은 elevated operator opt-in과 새 artifact root가 있을 때만 허용한다. |
| Manual-admin 04219→04220 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04219-04220.md` PASS. `0.42.19-admin-smoke -> 0.42.20-admin-smoke` readiness, installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다. Target MSI SHA-256은 `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`, update ZIP SHA-256은 `8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b`다. | `MANUAL-ADMIN`, closed | 이미 실행된 evidence로 보존한다. 재실행은 elevated operator opt-in과 새 artifact root가 있을 때만 허용한다. |
| Manual-admin 04220→04221 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04220-04221.md` PASS. `0.42.20-admin-smoke -> 0.42.21-admin-smoke` readiness, installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation v2가 모두 PASS다. Target MSI SHA-256은 `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b`, update ZIP SHA-256은 `09e1c3f5a7c8d2afac3d70bddbb1d91f575de2c45c9174a8da2bbb73c2e89767`다. Full admin host mutation current-card는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`, batch `full-admin-host-mutation-gate-20260516-04221`, `batch_evidence.status=available`, runtime bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`를 기록한다. Public-boundary successor main push는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md`, run `25938745434`, job `76250726268`에서 PASS했다. | `MANUAL-ADMIN`, closed-current | 이미 실행된 internal admin-smoke evidence로 보존한다. 재실행은 elevated operator opt-in과 새 artifact root가 있을 때만 허용한다. public trusted signing과 external stable publication은 `not-claimed`다. |
| 0.42.23 package build/manual-admin package-pair | `0.42.23-admin-smoke`; `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04223.md`와 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md` PASS. Target MSI SHA-256 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, provenance `676b4177b10dc80209969066857bab6008ff2473`, update ZIP SHA-256 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`, descriptor `manual-admin-campaign-descriptor-20260516-04222-04223-closed`, missing count `0`, not-pass count `0`이다. | package build는 `AUTO-REPO`; package-pair runner는 `MANUAL-ADMIN`; descriptor는 `AUTO-PREFLIGHT` | Package-pair runner 재실행은 elevated operator opt-in, 새 artifact root, host mutation 승인 때만 허용한다. Public trusted signing/external stable publication은 not claimed. |
| 0.42.23 full host mutation/current-card follow-up | `docs/ga-ready/evidence/post-04223-full-host-mutation-current-card-2026-05-16.md` PASS. Full gate `full-admin-host-mutation-gate-20260516-04223`, installed current-card `artifacts/installed-operator-surface-current-card-20260516-04223`, public-boundary post-merge run `25954744127`, job `76299282407`를 묶고 next product payload candidate를 `0.42.24-admin-smoke`로 선정했다. Local remote-gone merged codex branch 12개는 삭제했고, linked worktree/unmerged gone branch는 보존했다. | full gate는 `MANUAL-ADMIN`; current-card repair/capture는 `MANUAL-ADMIN`; branch cleanup은 local repo maintenance | 재실행은 새 artifact root와 `-AllowHostMutation`이 있을 때만 허용한다. 다음 code payload는 Runtime/API current evidence rollup으로 시작한다. Public trusted signing/external stable publication은 not claimed. |
| 0.42.24 package/fullgate/current-card | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md` PASS. Full-gate batch는 `full-admin-host-mutation-gate-20260516-04224`다. Package build MSI SHA-256은 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, provenance commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`다. Full-gate MSI SHA-256은 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`이다. `0.42.23-admin-smoke -> 0.42.24-admin-smoke` descriptor는 `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04223-04224.md`에 생성됐지만 `missing_count=5`, `not_pass_count=1`로 blocked다. Runtime/API current evidence contract는 `runtime-api-current-evidence-rollup-v1`이다. | package build는 `AUTO-REPO`; full gate/current-card는 `MANUAL-ADMIN`; descriptor는 `AUTO-PREFLIGHT` | 04226 closure 이후 historical predecessor로 보존한다. |
| 0.42.25 fullgate/current-card/manual-admin closure | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04225-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04225.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md` PASS. Full-gate/target MSI SHA-256 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`, update ZIP SHA-256 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`. Earlier package build record는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md`, MSI SHA-256 `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`, provenance commit `403d4474c4b88136774600cc81ca2d941c0b5e4b`로 historical candidate record로 보존한다. Descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`는 `missing_count=0`, `not_pass_count=0`이다. PR #144 post-merge public-boundary guard는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md`, run `25959505688`, job `76312299500`에서 PASS했다. | full gate/current-card/package-pair runner는 `MANUAL-ADMIN`; package build record는 `AUTO-REPO`; descriptor는 `AUTO-PREFLIGHT` | 04226 manual-admin closure 이후 historical predecessor로 보존한다. 재실행은 새 artifact root와 elevated operator opt-in이 있을 때만 허용한다. Public trusted signing/external stable publication은 not claimed. |
| 0.42.26 package/fullgate/current-card/manual-admin closure | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04226.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`, `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md` PASS. Package MSI SHA-256 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`, full-gate/target operational MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP SHA-256 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`, provenance `d6500c01c972cbc7ca1e290e51120181ceea1501`. Initial descriptor `manual-admin-campaign-descriptor-20260516-04225-04226`는 readiness PASS지만 당시 `missing_count=4`, `not_pass_count=1`이었다. Closure descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`는 `missing_count=0`, `not_pass_count=0`이다. PR #145 post-merge public-boundary guard는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md`, run `25961834812`, job `76318357776`에서 PASS했다. | full gate/current-card/package-pair runner는 `MANUAL-ADMIN`; package build record는 `AUTO-REPO`; descriptor는 `AUTO-PREFLIGHT` | 최신 operational 및 닫힌 package-pair evidence다. 다음 package-pair는 next product payload change 전까지 열지 않는다. Public trusted signing/external stable publication은 not claimed. |
| 0.42.22 package build/current-card host mutation | `docs/ga-ready/evidence/post-04222-package-host-mutation-current-card-2026-05-16.md` PASS WITH DESCRIPTOR HISTORY. Package build `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`, full gate `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`, installed operator current-card `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md`, package MSI SHA-256 `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`, full-gate MSI SHA-256 `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`, public-boundary post-merge run `25952150476`, job `76291983316`, provenance commit `8a38995cc25a888f64473e9a2869740949ad6b24`를 묶는다. | package build는 `AUTO-REPO`; full gate는 `MANUAL-ADMIN`; installed current-card는 `AUTO-INSTALLED-READONLY` | 0.42.22 full gate는 이미 elevated opt-in으로 실행됐다. 재실행은 새 artifact root와 `-AllowHostMutation`이 있을 때만 허용한다. |
| Manual-admin 04221→04222 descriptor candidate / blocker | `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04221-04222.md`는 초기 descriptor candidate를 `blocked-by-missing-evidence`로 기록했고, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`가 Burn install exit `1603` Credential Manager idempotence blocker를 보존한다. | `MANUAL-ADMIN`, historical-blocked-package-pair | PASS claim이 아니다. Closure는 0.42.23 package-pair evidence가 소유한다. |
| Post-04221 successor operator surface | `docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md` CODE_LEVEL_AND_OPERATOR_SURFACE_PASS. Public-boundary successor run `25938745434`, installed Web/TUI/CLI current-card smoke, 04221 canonical latest key cleanup, Web Console diagnostics registry bridge direct expose를 기록한다. 다음 product payload candidate는 `0.42.22-admin-smoke`다. | `AUTO-REPO` + `AUTO-INSTALLED-READONLY`, open-next-package-candidate | host mutation performed `false`. 이 slice는 package build, clean-host, full admin host mutation, public trusted signing, external stable publication을 실행하지 않는다. |
| Manual-admin 04215→04216 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04215-04216.md` historical predecessor PASS. `0.42.15-admin-smoke -> 0.42.16-admin-smoke` readiness, installed update/rollback, Windows Update NoContact recovery 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다. Target MSI SHA-256은 `8b67c774f5d986c90749f494cc2084626d5bdf63904d3f9dd26b9b5daadde366`, update ZIP SHA-256은 `acd5209aa73cb14ffc655122b5905f45c87a9b9c610dd2f15307a61de7a966ab`다. | `MANUAL-ADMIN`, closed | 04218 PASS 이후 historical로 보존한다. |
| Ops summary descriptor selector guard | `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md` PASS. `manual-admin-campaign-descriptor-*` batch를 current-card operational evidence 후보에서 제외하고, 04218 full gate current-card smoke `artifacts/installed-current-card-20260515-04218-fullgate`가 `full-admin-host-mutation-gate-20260515-163107-04218`을 표시함을 확인했다. Selector guard MSI SHA-256은 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`다. | code/test는 `AUTO-REPO`; package install/current-card smoke는 `MANUAL-ADMIN` | selector code guard는 자동 회귀 테스트로 유지한다. 설치본 package apply와 service repair는 manual-admin evidence로만 실행한다. |
| Post-04218 contract alignment | `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md` PASS. Runtime/Core route/evidence bridge, Hyper-V dispatch catalog detail, Host Ops lifecycle bucket, packaging next trigger, Web Console/TUI/CLI operator journey, ADR-0005/0006 public boundary를 문서 계약으로 고정했다. | `AUTO-REPO`, closed | host mutation performed `false`. package build, clean-host, full admin host mutation, public signing/publication을 실행하지 않는다. |
| Post-04218 runtime/domain development slice | `docs/ga-ready/evidence/post-04218-runtime-domain-slices-2026-05-15.md` CODE_LEVEL_PASS. Runtime route-family bridge, Hyper-V handler contract, Host Ops lifecycle bucket key, 0.42.19 next-candidate descriptor metadata, current-card operator journey를 code/test contract로 고정했다. | `AUTO-REPO`, closed | host mutation performed `false`. package build, clean-host, full admin host mutation, public signing/publication을 실행하지 않는다. |
| Post-04218 follow-up execution | `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md` PACKAGE_BUILD_PASS_CODE_CONTRACT_PASS. `0.42.19-admin-smoke` package build artifact는 `artifacts/admin-smoke-package-20260515-04219`이고 Runtime route registry, Hyper-V dispatch registry, Host Ops family helper, current-card snapshot parity, `public-boundary-ci-required` guard를 고정했다. | `AUTO-REPO`, closed | package build는 완료했지만 update ZIP/package-pair campaign/full admin host mutation은 실행하지 않았다. public trusted signing과 external stable publication은 `not-claimed`다. |
| Post-04219 follow-up execution | `docs/ga-ready/evidence/post-04219-followup-execution-2026-05-16.md` CODE_CONTRACT_PASS_DESCRIPTOR_READINESS_EXECUTED_CI_GUARD_WIRED. `0.42.19-admin-smoke` 기준 manual-admin readiness와 `manual-admin-campaign-descriptor-20260516-04218-04219` descriptor batch를 실행했고, Runtime queued mutation registry, Hyper-V `operation-level-telemetry-error-contract-v1`, Host Ops extended family helper, `public-boundary-ci-required` workflow를 고정했다. | `AUTO-REPO` + `AUTO-PREFLIGHT`, closed | descriptor/readiness는 non-mutating 실행이다. full admin host mutation은 `prepared` dry-run manifest만 남겼고 actual host mutation은 실행하지 않았다. public trusted signing과 external stable publication은 `not-claimed`다. |
| Post-04220 development slices | `docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md` CODE_LEVEL_PASS. `0.42.20-admin-smoke` 기준 Runtime diagnostics/ops summary evidence contract, Hyper-V WMI common helper catalog, Host Ops mutation boundary, `0.42.20 -> next` packaging trigger를 code/test contract로 고정했다. Public-boundary workflow run `25933428239` / job `76232707240`는 PASS했으며, 이전 run `25931297085`의 GitHub billing/spending-limit blocker는 historical로 보존한다. | `AUTO-REPO`, closed | host mutation performed `false`. package build, clean-host, full admin host mutation, public trusted signing, external stable publication을 실행하지 않는다. |
| Branch protection fallback / Public-boundary CI maintenance | 최신 post-merge `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md` PASS. `main` push run `26017721669`, job `76471545641`, head `a4509c552c003ee0fc87b54b26529686e6dfeb84`에서 `public-boundary-ci-required`가 PASS했다. Branch protection/ruleset API는 private repo 현재 플랜에서 unavailable이므로 fallback guard는 PR/merge `public-boundary-ci-required` 확인이다. GitHub Actions checkout은 `actions/checkout@v6.0.2`로 maintenance한다. PR #156 후속에는 product payload 변경이 없어 package chain은 보류했다. 이전 PR #155 evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`, run `26013384587`, PR #154 run `25989986761`, PR #153 run `25987705546`, PR #152 run `25985786230`, PR #151 run `25984814303`, PR #150 run `25983307305`, PR #149 run `25974335803`, PR #145 run `25961834812`, PR #144 run `25959505688`, scope-lock run `25958514394`, post-merge run `25954744127`, successor run `25938745434`, 04221 run `25935332346`, 04220 run `25933861585`는 historical로 보존한다. | `AUTO-REPO`, closed-current | CI guard evidence 자체는 public trusted signing, clean-host public signed gate, external stable publication을 실행하지 않는다. |
| Post-04227 PR #150 public-boundary follow-up | `docs/ga-ready/evidence/post-04227-pr150-public-boundary-followup-2026-05-17.md` PASS. PR #150 main push public-boundary evidence를 current로 올리고 `0.42.28-admin-smoke` package chain을 `deferred-until-next-product-payload-change`로 유지했다. Host Ops Web diagnostics bucket table은 다음 Operator Surface product payload 변경 시 구현/검증 후보이며 installed account/noVNC smoke도 같은 변경 시 재확인한다. | `AUTO-REPO`, closed-current | host mutation performed `false`. package build, full admin host mutation, installed account/noVNC smoke, public trusted signing, external stable publication을 실행하지 않는다. |
| Post-04228 Operator Surface admin-smoke | `docs/ga-ready/evidence/post-04228-operator-surface-admin-smoke-2026-05-17.md` PASS. Host Ops Web diagnostics bucket table을 `host-ops-web-diagnostics-bucket-table-v1`로 구현하고 `0.42.28-admin-smoke` package build, full admin host mutation, installed Web/TUI/CLI current-card, installed account/browser, target-backed noVNC smoke를 PASS했다. | `MANUAL-ADMIN`, closed | host mutation performed `true`; public trusted signing, clean-host public signed gate, external stable publication은 실행하지 않는다. 후속 package-pair는 아래 04227→04228 campaign에서 닫혔다. |
| Manual-admin 04227→04228 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` PASS. `0.42.27-admin-smoke -> 0.42.28-admin-smoke` readiness, installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation v2, installed current-card recheck, account/noVNC package-pair smoke를 모두 닫았다. Target MSI SHA-256 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, descriptor `manual-admin-campaign-descriptor-20260517-04227-04228-closed`, `missing_count=0`, `not_pass_count=0`이다. | `MANUAL-ADMIN`, closed-current | host mutation performed `true`; public trusted signing, clean-host public signed gate, external stable publication은 실행하지 않는다. 다음 package-pair는 새 product payload 이후 연다. |
| Public-boundary checkout v6.0.2 main push | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md` PASS. `main` push run `25934411998`, job `76236050409`, head `3933231e6e2abf3a398dfcc3fdc999b3df38dac6`에서 `actions/checkout@v6.0.2`와 `public-boundary-ci-required`가 PASS했다. Node.js 20 deprecation warning은 관찰되지 않았다. | `AUTO-REPO`, closed | CI guard maintenance evidence다. host mutation, clean-host, full admin host mutation, public trusted signing, external stable publication을 실행하지 않는다. |
| Post-ci-maintenance development slices | `docs/ga-ready/evidence/post-ci-maintenance-dev-slices-2026-05-16.md` CODE_LEVEL_PASS. Runtime/API registry bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, Hyper-V provider call-site drift guard, Host Ops dry-run/mutation reason code, manual-admin descriptor generation v2를 고정하고 next product payload candidate를 `0.42.21-admin-smoke`로 선택했다. | `AUTO-REPO`, open-next-package-candidate | host mutation performed `false`. 이 slice는 다음 package build 후보를 선택하지만 package build, clean-host, full admin host mutation, public trusted signing, external stable publication을 실행하지 않는다. |
| Installed account login smoke | historical latest PASS: `artifacts/installed-account-login-smoke-20260510-0410-final`; frontend/backend auth console live smoke PASS `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`, `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543` | `MANUAL-ADMIN` | temporary account/JWT state를 쓰고 service restart 후 protected files/ACL을 restore하므로 explicit operator opt-in만 허용. Browser QA도 installed asset refresh와 임시 account boundary를 포함하면 manual-admin evidence로만 기록한다. |
| Target-backed noVNC installed streaming smoke | `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md` PASS | `MANUAL-ADMIN` | installed product payload apply/replace와 noVNC target routing을 수행하므로 installed admin-smoke approval 필요. |
| Historical installed TUI operator smoke | `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md` PASS predecessor | `historical-only`, unscheduled | ADR-0011 이후 active queue에서 제거한다. Dated evidence 사실만 보존한다. |
| manual-admin 0.41.5 rebaseline readiness | `docs/ga-ready/evidence/manual-admin-rebaseline-readiness-2026-05-10-0415.md` PASS; baseline host 기준으로 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`에 superseded | `AUTO-PREFLIGHT` descriptor only; resulting smoke는 `MANUAL-ADMIN` | local readiness regeneration은 안전하다. Credential Manager, Event Log, Burn/MSIX/MSI, update/rollback, clean-host, service restart, host mutation은 실행하지 않는다. |
| Manual-admin operator/hardening follow-up | Operator Access, Internal Service Hardening, Lifecycle/Packaging current rebaseline PASS: `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md` | installed/mutating smoke는 `MANUAL-ADMIN`; `0.41.5` to `0.41.6` package-pair rebaseline은 closed | Account/JWT, noVNC service path, service token, Credential Manager, Event Log, TLS, service restart, installer mutation, update/rollback, clean-host mutation은 elevated operator opt-in 필요. |
| Internal HTTPS/TLS, clean-host install/update/rollback, Credential Manager, Event Log, service token, Burn/MSIX, MSI update/rollback | internal-only PASS evidence 존재, public release evidence 아님 | installed/mutating smoke는 `MANUAL-ADMIN`; plan-only script는 `AUTO-PREFLIGHT` | unattended 실행 금지. explicit elevated operator opt-in을 사용하고 public trusted signing/external stable publication은 `not-claimed`로 보존한다. |

## Manual Admin 상세 Matrix

이 follow-up들은 scripted 또는 batch-supervised로 묶을 수 있지만 unattended automatic
job이 아니다.

묶음 manual-admin campaign을 다시 실행하기 전 installed product version과 모든
runner의 default payload version을 확인한다. 최신 full admin host mutation PASS는
`0.42.23-admin-smoke` / 2026-05-16 04223 evidence이며
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`가
소유한다. 직전 `0.42.22-admin-smoke` / 04222 evidence, `0.42.21-admin-smoke` / 04221 evidence, `0.42.20-admin-smoke` / 04220 evidence와 2026-05-15
`0.42.18-admin-smoke` / 04218 evidence, 2026-05-15 `0.42.16-admin-smoke` / 04216 evidence,
2026-05-14 04215 evidence, 2026-05-14 04212 explicit/rerun evidence, 2026-05-13
`0.42.12-admin-smoke` / 04212 evidence, 이전 `0.42.11-admin-smoke` / 04211 evidence,
`0.42.9-admin-smoke` / 0429 evidence, `0.42.8-admin-smoke` / 0428 evidence,
`0.42.7-admin-smoke` / 0427 evidence, `0.42.3-admin-smoke` / 0423 evidence와
`0.42.2-admin-smoke` / 0422 evidence는 historical로 보존한다. historical anchor에는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md`와
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-11-0422-hostmutation.md`를
포함한다. 04212 explicit batch는
`artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit`,
MSI SHA-256 `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`,
provenance commit `d338b8a99f3e1e3839ac89a6de0da034ff3da148`로 보존한다. 04211
package-pair predecessor는 MSI SHA-256
`750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb`와 provenance commit
`987beb51025a5aa926df7d9a905019b4d6d29705`를 historical-only로 유지한다. 이전
`0.42.2-admin-smoke` / 0422 evidence는 historical로 보존한다.
Historical preservation summary: `0.42.12-admin-smoke` 04212 evidence historical,
`0.42.11-admin-smoke` 04211 evidence historical, `0.42.9-admin-smoke` 0429 evidence
historical, `0.42.8-admin-smoke` 0428 evidence historical, `0.42.7-admin-smoke` 0427
evidence historical, `0.42.3-admin-smoke` 0423 evidence historical, `0.42.2-admin-smoke`
0422 evidence historical.
Single-line historical anchors: `0.42.9-admin-smoke` 0429 evidence historical;
`0.42.8-admin-smoke` 0428 evidence historical; `0.42.7-admin-smoke` 0427 evidence
historical; `0.42.3-admin-smoke` 0423 evidence historical; `0.42.2-admin-smoke` 0422
evidence historical.
Single-line legacy fallback: `0.42.7-admin-smoke` 0427 evidence historical.
이전 `0.42.2-admin-smoke` / 0422 evidence는 historical로 보존한다.
04220 historical full gate anchor
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`
uses batch `full-admin-host-mutation-gate-20260516-04220`, full-gate MSI SHA-256
`12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit
`0895d018935298721b25b5d9ce1ae083a6690c25`, and public trusted signing/external stable
publication `not-claimed`.
04218 historical full gate anchor
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`
uses batch `full-admin-host-mutation-gate-20260515-163107-04218`, full-gate MSI SHA-256
`0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09`, provenance commit
`9121d1f5e7fa83d803c484a44698d4fc8e825c19`, and public trusted signing/external stable
publication `not-claimed`.
04220 historical public-boundary main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md` records run
`25933861585`, job `76234195716`, head `686e4201f823295dc65cde302f613a982ab8cade`,
required guard `public-boundary-ci-required`, and public trusted signing/external stable
publication `not-claimed`.
04220 billing blocker 해소는 `docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md`
run `25933428239`로 확인했고, earlier billing/spending-limit blocker runs는 historical로 보존한다.
04211 native repair historical package-pair anchor keeps `0.42.11-admin-smoke`,
provenance commit `987beb51025a5aa926df7d9a905019b4d6d29705`, package MSI SHA-256
`750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`, and public trusted
signing/external stable publication `not-claimed`.
더 이전 `0.41.5-admin-smoke` / 0415 evidence도 historical로 보존하며
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`를
canonical historical anchor로 둔다. Lifecycle/Packaging current rebaseline은
`docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`의
`0.41.5-admin-smoke` to `0.41.6-admin-smoke` package pair로 closed 상태다.
이전에 요청된 0.41.2 rebaseline은 historical이며 `blocked-by-installed-version-mismatch`
상태로 남긴다. historical lifecycle runner 일부는 여전히 `0.39.x` 또는 `0.38.x` payload에 묶여 있으므로, 현재 installed node에서 실행하려면 operator가
downgrade/restore run을 명시적으로 수락해야 한다.

다음 manual-admin campaign은 `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`를
먼저 갱신한 뒤 실행 여부를 판단한다. 이 descriptor는 현재 닫힌
`0.42.20-admin-smoke -> 0.42.21-admin-smoke` PASS와 이전
`0.42.19-admin-smoke -> 0.42.20-admin-smoke` package-pair/RCA를 구분한다.
`0.42.10-admin-smoke` RCA는
`docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`가
소유하고, 다음 package-pair candidate는 실제 product payload 변경이 생길 때
`0.42.21-admin-smoke -> next` 후보를 연다. 2026-05-16 Web Console diagnostics direct expose는
다음 product payload candidate `0.42.22-admin-smoke`를 열었다. 2026-05-15 `0.42.19-admin-smoke`
package build는 product payload 변경을 반영했지만 package-pair/full gate는 열지
않았고, 2026-05-16 `0.42.21-admin-smoke` campaign이 latest current claim을
닫았다. 이전 `origin/main` merge commit
`14f56fd7348572e1757413657a68cd17c0aeca52` 기준 post-merge package build 보류
record는 historical로 보존한다. Batch Supervisor의
`ManualAdminCampaignDescriptor` profile은 descriptor 생성을 non-mutating manifest
step으로 연결한다. 최신 manifest id는
`manual-admin-campaign-descriptor-20260516-04220-04221`이다. 이전
`manual-admin-campaign-descriptor-20260514-04214-04215`,
`manual-admin-campaign-descriptor-20260514-04212-04213`은 historical predecessor로
보존한다. Descriptor 갱신 자체는
`AUTO-PREFLIGHT`이지만, campaign 실행은 계속 `MANUAL-ADMIN`이다.
2026-05-14 post-04212 follow-up triage는
`docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`에 보존한다.
`main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이
없어 `0.42.13-admin-smoke` build/full gate/package-pair는 열지 않았다. Clean-host
Windows Update recovery summary key는 다음 실제 clean-host campaign에서
`recovery_actions`와 `automatic_recovery_performed`로 판정한다.
2026-05-14 `1-2-3-4-5` 재승인 follow-up은
`docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`에
보존한다. `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product payload
변경이 없어 package/host mutation chain은 보류했고, Web Console Dashboard/Evidence
current-card smoke PASS를 `pass-dashboard-current-card-smoke-deferred-product-chain`으로 닫았다.

Post-0423 triage는 `docs/ga-ready/evidence/post-0423-followup-triage-2026-05-12.md`에
historical planning record로 보존한다. 0427→0428 PASS 이후 current package-pair
claim은 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`가
소유한다. Operator Access, noVNC/TUI, internal service hardening, Lifecycle/Packaging
rerun은 계속 `MANUAL-ADMIN`이며 public trusted signing과 external stable publication은
`out-of-scope`/`not-claimed`다.

Manual-admin 0423→0424 campaign은
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`가 소유한다.
결과는 `historical-partial-pass-clean-host-blocked`다. Full admin host mutation, Operator
Access, Internal Service Hardening, installed update/rollback은 PASS지만 dedicated
clean-host package-pair는 baseline `0.42.3-admin-smoke` MSI가 clean host에서
`EventLogDefaultTransition`을 `ConfigureInstalled`보다 먼저 실행해 blocked다.
이 branch는 `ConfigureInstalled -> EventLogDefaultTransition ->
CredentialManagerDefaultTransition` sequence fix를 code-level로 적용했다. 이
campaign은 historical-only로 보존하며 current package-pair claim은 아니다. Public
trusted signing과 external stable publication은 주장하지 않는다.

Manual-admin 0425→0426 campaign은
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`가 소유한다.
결과는 `pass`다. Installed update/rollback, dedicated clean-host install/update/rollback,
Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops
summary capture가 모두 PASS다. `New-PcvManualAdminCampaignDescriptor.ps1`는 실행을
하지 않는 `AUTO-PREFLIGHT` descriptor 도구이며, `ManualAdminCampaignDescriptor`
Batch Supervisor profile로 manifest에 직접 연결할 수 있다. 다음 descriptor manifest는
`New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`가 생성한다. Post-merge rebuild는
`artifacts/admin-smoke-package-20260512-0426-postmerge`, MSI SHA-256
`9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16`, provenance commit
`37f4d6b83d6caef1338e0a60e5df0a60209b51f8`로 보존한다. 실제 install/update/rollback,
clean-host, Burn, MSIX 실행은 계속 `MANUAL-ADMIN`이다. `0.42.7-admin-smoke` build는
사용자 승인 후 실행됐고 full admin host mutation gate와 installed listener current-card smoke까지 PASS했다. 0427→0428 campaign PASS 이후에는 historical predecessor로 보존한다.

Manual-admin 0427→0428 campaign은
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`가 소유한다.
결과는 `pass`다. Installed update/rollback, dedicated clean-host install/update/rollback,
Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops
summary capture, descriptor generation이 모두 PASS다. Target post-merge rebuild는
`artifacts/admin-smoke-package-20260512-0428-postmerge`, MSI SHA-256
`e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, provenance commit
`5397e580c98a34e8b7beb5b9773d1d857025315b`로 보존한다. Descriptor manifest는
`manual-admin-campaign-descriptor-20260512-0427-0428`이고, 후속 0428 full admin host
mutation gate와 installed listener current-card smoke도 PASS했다. 실제
install/update/rollback, clean-host, Burn, MSIX 재실행은 계속 `MANUAL-ADMIN`이다.

| 항목 | 최신 evidence | `MANUAL-ADMIN` 유지 이유 | Batch stance |
|------|---------------|--------------------------|--------------|
| Full admin gate | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-17-04229-hostmutation.md`; `0.42.29-admin-smoke`; artifacts `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04229`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229`, `artifacts/os-mutation-gates-batch-profile-20260517-04229`; installed listener `batch_evidence.status=available`, latest batch `full-admin-host-mutation-gate-20260517-04229`, current evidence `runtime-api-current-evidence-rollup-v1`; 이전 2026-05-17 04228/04227, 2026-05-16 04226/04225/04224/04223/04222/04221/04220, 2026-05-15 04218/04216, 2026-05-14 04215/04212 explicit/rerun, 2026-05-13 04212/04211/0429, 2026-05-12 0428/0427/0423 evidence는 historical predecessor. 보존 anchor: `full-admin-host-mutation-gate-2026-05-17-04228-hostmutation`, `full-admin-host-mutation-gate-2026-05-17-04227-hostmutation`, `full-admin-host-mutation-gate-2026-05-16-04226-hostmutation`, `full-admin-host-mutation-gate-2026-05-16-04225-hostmutation`, `full-admin-host-mutation-gate-2026-05-16-04224-hostmutation`, `full-admin-host-mutation-gate-2026-05-16-04223-hostmutation`, `full-admin-host-mutation-gate-2026-05-16-04222-hostmutation`, `full-admin-host-mutation-gate-2026-05-16-04221-hostmutation`, `full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation`, `full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation`, `full-admin-host-mutation-gate-2026-05-13-04212-hostmutation`, `full-admin-host-mutation-gate-2026-05-13-04211-hostmutation`, `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`, `full-admin-host-mutation-gate-2026-05-12-0428-hostmutation` | batch manifest step이 `requires_admin=true`, `mutates_host=true`이며 MSI payload apply, Hyper-V route parity, firewall/LAN/Event Log/internal trust-store gate, installed service state 검증을 수행한다. | elevated shell에서 `Invoke-PcvBatchSupervisor.ps1 -AllowHostMutation`으로만 실행. |
| Manual-admin 0423→0424 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`; `historical-partial-pass-clean-host-blocked`; target root `artifacts/admin-smoke-package-20260512-0424`; installed lifecycle root `artifacts/manadm-0424/lifecycle/product-update-rollback`; clean-host blocker root `artifacts/manadm-0424/clean-host-rerun` | full admin gate, operator access, internal service hardening, installed update/rollback이 모두 installed/admin mutation을 수행했다. dedicated clean-host는 Hyper-V VM과 guest MSI lifecycle을 mutation했고 blocker를 발견했다. | PASS bucket은 evidence로 보존하되 current package-pair claim으로 쓰지 않는다. |
| Manual-admin 0425→0426 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`; `pass-historical-predecessor`; descriptor `artifacts/manual-admin-campaign-20260512-0425-0426/manual-admin-campaign-descriptor/summary.json`; post-merge rebuild `artifacts/admin-smoke-package-20260512-0426-postmerge` | installed product files/service state, clean-host VM, Burn/MSIX package lifecycle, installed runtime API 조회가 포함된다. | 0427→0428 PASS 이후 current package-pair claim에서는 내려가고 historical predecessor로 보존한다. |
| Manual-admin 0427→0428 campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`; `pass`; descriptor `artifacts/manual-admin-campaign-20260512-0427-0428/manual-admin-campaign-descriptor/summary.json`; latest manifest `manual-admin-campaign-descriptor-20260512-0427-0428`; target rebuild `artifacts/admin-smoke-package-20260512-0428-postmerge` | installed product files/service state, clean-host VM, Burn/MSIX package lifecycle, installed runtime API 조회가 포함된다. | evidence descriptor 생성은 `AUTO-PREFLIGHT`; `ManualAdminCampaignDescriptor` profile과 `New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`는 non-mutating. 실제 lifecycle 재실행은 manual/admin opt-in 유지. |
| Installed account login | `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, historical `artifacts/installed-account-login-smoke-20260510-0410-final` | 임시 account/JWT 파일을 쓰고 service restart 후 login/session/RBAC/console을 확인한 뒤 protected files/ACL을 restore한다. | shared node 또는 recurring unattended job 금지. |
| noVNC | `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, historical `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md` | target-backed streaming은 installed payload/version state와 explicit target routing에 의존하고, combined evidence는 installed product payload를 적용했다. | code-level bridge tests는 `AUTO-REPO`; target-backed installed streaming은 manual. |
| TLS | `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, historical `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md` | installed TLS lifecycle은 internal HTTPS certificate state를 generate/bind/rotate/remove하고 service listener configuration을 restore한다. | installed lifecycle smoke는 manual; `New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`는 `AUTO-PREFLIGHT`. |
| clean-host | `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, package-pair rebaseline `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md` | dedicated clean Windows host 또는 VM에서 install/update/rollback lifecycle을 수행한다. | dedicated lab/admin run only. readiness descriptor는 preflight 가능. |
| Credential Manager | `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, historical `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md` | LocalSystem service credential storage, reload, old-source rejection, rollback diagnostics를 검증한다. | installed transition smoke는 manual; transition preflight와 code-level tests는 automatic-safe. |
| Event Log | `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, historical `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md` | provider/source state를 register/repair/remove하고 test record write와 volume/schema behavior를 확인한다. | installed provider/default-writer smoke는 manual; provider plan/code-level checks는 `AUTO-PREFLIGHT` 또는 `AUTO-REPO`. |
| update/rollback | `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, historical `docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md`, `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md` | installed product files, service state, manifest/journal state, rollback roots를 mutation하며 failure path가 restore를 검증한다. | manual installed destructive smoke. catalog/rollback code-level tests는 `AUTO-REPO`; public signed clean-host update/rollback은 `BLOCKED-EXTERNAL`. |

하나의 operator campaign으로 묶더라도 아래 bucket은 unattended schedule 하나로 합치지 않는다.

- 기준 host gate: full admin host mutation gate. 최신 PASS는 `0.42.29-admin-smoke` / 04229 evidence이며 `MANUAL-ADMIN`이다. 이전 `0.42.28-admin-smoke`, `0.42.27-admin-smoke`, `0.42.26-admin-smoke`, `0.42.25-admin-smoke`, `0.42.24-admin-smoke`, `0.42.23-admin-smoke`, `0.42.22-admin-smoke`, `0.42.21-admin-smoke`, `0.42.20-admin-smoke`, `0.42.18-admin-smoke`, `0.42.16-admin-smoke`, `0.42.15-admin-smoke`, `0.42.12-admin-smoke` explicit/rerun, `0.42.11-admin-smoke` / 04211 evidence와 더 이전 evidence는 historical로 보존한다. 다음 full gate도 새 version/elevated `-AllowHostMutation` campaign으로만 실행한다.
- 운영자 접근: installed account login smoke와 target-backed noVNC installed streaming smoke. 임시 account/JWT 교체, service restart, noVNC target configuration mutation 때문에 `MANUAL-ADMIN`이다.
- 내부 service hardening: internal HTTPS/TLS lifecycle installed smoke, Credential Manager default transition, Event Log default transition, service token rotation/revoke. certificate, credential, token, Event Log/provider, service reload state를 mutation하므로 `MANUAL-ADMIN`이다.
- Lifecycle/Packaging: internal clean-host install/update/rollback smoke, MSI/update/rollback, Burn/MSIX lifecycle. install, update, rollback, repair, remove, clean-host environment mutation을 수행하므로 `MANUAL-ADMIN`이다.

## 스크립트 Routing 기준

| Script 또는 pattern | 기본 class | 메모 |
|---------------------|------------|------|
| 모든 manifest step이 `requires_admin=false`, `mutates_host=false`인 `Invoke-PcvBatchSupervisor.ps1` | `AUTO-REPO` | reboot와 scheduled-task command는 계속 금지. |
| admin 또는 host-mutating step이 하나라도 있는 `Invoke-PcvBatchSupervisor.ps1` | `MANUAL-ADMIN` | elevated shell과 `-AllowHostMutation` 필요. artifact heartbeat/summary로 audit한다. |
| `New-PcvPublicOpsFinalFollowupAttempt.ps1`, `New-PcvPublicOpsGateExecutionReadiness.ps1`, public distribution/readiness/winget/updater catalog/public signed smoke preflight generator | `AUTO-PREFLIGHT` | local evidence와 blocked-status regeneration만 허용. public claim 또는 external submission 금지. |
| `New-PcvManualAdminRebaselineReadiness.ps1` | `AUTO-PREFLIGHT` | local readiness descriptor only. installed-version mismatch와 current package input을 보고할 수 있지만 Credential Manager, Event Log, Burn/MSIX/MSI, update/rollback, clean-host, service restart, host mutation은 실행하지 않는다. |
| `New-PcvManualAdminCampaignDescriptorBatchManifest.ps1` | `AUTO-PREFLIGHT` | `ManualAdminCampaignDescriptor` batch manifest를 생성하고 dry-run 검토할 수 있다. `requires_admin=false`, `mutates_host=false`이며 installed lifecycle을 실행하지 않는다. |
| `New-PcvTimeoutRateLimitHardeningPreflight.ps1`, `New-PcvDiagnosticBundleServerPreflight.ps1`, TLS/service-token/Burn/MSIX preflight generator | `AUTO-PREFLIGHT` | 별도 installed admin-smoke runner를 명시적으로 호출하지 않는 한 plan-only. |
| `Invoke-PcvInstalledTuiOperatorSmoke.ps1` | `AUTO-INSTALLED-READONLY` | 준비된 service에 대한 standalone installed read-only check일 때만 안전. |
| `Invoke-PcvInstalledAccountLoginSmoke.ps1`, `Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`, `Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1`, `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`, `Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1`, `Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1`, MSI/update/rollback/Burn/MSIX lifecycle smoke | `MANUAL-ADMIN` | mutating 또는 environment-coupled evidence이므로 unattended schedule 금지. |
| Public trusted signing, timestamp, external stable publication/catalog upload, winget submission, public signed clean-host smoke | `BLOCKED-EXTERNAL` | ADR-0006 internal private network distribution에 따라 범위 밖. |

## 현재 결정

현재 unattended host-mutating automation으로 옮길 open follow-up은 없다.

상시 자동 batch 후보는 아래 세 가지뿐이다.

- `AUTO-REPO`: non-mutating repo regression 및 frontend fixture/parity verification.
- `AUTO-PREFLIGHT`: public claim을 blocked 또는 out-of-scope로 유지하는 local descriptor/readiness scan.
- `AUTO-INSTALLED-READONLY`: installed service state를 수정하지 않는 dedicated installed listener/TUI/load 조회.

admin-smoke, installed mutation, clean-host, update/rollback, account/JWT restore,
TLS binding, service token, Credential Manager, Event Log, firewall, LAN, trust-store,
Hyper-V, public release work는 계속 explicit operator opt-in에 남긴다.
