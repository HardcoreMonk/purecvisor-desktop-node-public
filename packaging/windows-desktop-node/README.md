# PureCVisor Desktop Node Service-first Product Wrapper

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

## 2026-07-13 historical TUI predecessor

당시 operational package/full-gate anchor는 `0.42.62-admin-smoke` /
`full-admin-host-mutation-gate-20260713-04262`다. Package, full admin host mutation, installed
current-card evidence는 각각 `docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04262-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04262.md`가 소유한다.
Clean MSI SHA-256은 `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`,
operational MSI SHA-256은 `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`,
operational payload aggregate SHA-256은
`ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`, provenance commit은
`7f71f0a518c5b592f233373522d36b5401c3f1df`다.

`docs/ga-ready/evidence/wmi-internal-switch-topology-recovery-2026-07-13-04260-04262.md`는
04260 name-only topology failure와 04261 incomplete WMI object projection failure를 보존한다.
두 package는 설치/MSI lifecycle까지 PASS했지만 full gate 첫 단계에서 실패했고 OS mutation은
실행되지 않아 PASS anchor가 아니다. 04262 full gate는 2/2 단계 PASS, installed current-card는
CLI 5개/TUI 2개 exit `0`, Web 3개 HTTP `200`, 두 internal switch topology와 service
`Running/Automatic`을 PASS했다.

최신 closed manual-admin package-pair는 계속 `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. 04262는
`AllowUnsignedDev`/`LocalTest` internal admin-smoke이며 public trusted signing 또는 외부 stable
publication evidence가 아니다. 아래 이전 날짜 current 문단은 historical predecessor로 해석한다.

## 2026-05-29 현재 기준

최신 operational package/full-gate anchor는 `0.42.59-admin-smoke` /
`full-admin-host-mutation-gate-20260529-04259`다. Package build evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`, full admin host
mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`,
manual-admin package-pair evidence는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. Package MSI SHA-256은
`6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`, operational MSI
SHA-256은 `dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`, payload aggregate
SHA-256은 `3f015e7743efac3b61de81962c236a03c1bcf882053fc92fd3c525da280a1687`다.

최신 product payload package는 `0.42.59-admin-smoke`다. Installed Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`에서
04259 fullgate 후 PASS로 재확인했다. 최신 닫힌 manual-admin package-pair closure는
`0.42.58-admin-smoke -> 0.42.59-admin-smoke`이며, Windows Update clean-host, Burn, MSIX,
installed update/rollback, runtime ops summary, descriptor v2가 PASS다. 04250→04254 manual-admin readiness는
현재 host baseline mismatch로 blocked다. 최신 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`이고
installed current-card payload 후보는 이미 열린 `0.42.60-admin-smoke`를 유지한다. docs-maintenance
postpush만으로 추가 package 후보를 열지 않는다. Public trusted signing 또는 외부 stable
publication evidence가 아니며, 아래 이전 날짜 current 문단은 historical predecessor로 해석한다.
직전 0.42.58 predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.

## 2026-05-21 historical predecessor

최신 operational package/full-gate anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`다. Package build evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`, full admin host
mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`,
manual-admin package-pair evidence는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md` /
`manual-admin-campaign-descriptor-20260521-04239-04240-closed`다. Operational MSI
SHA-256은 `eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41`, update ZIP
SHA-256은 `96599dc4493e26e8cf467e19fabc5ab20306166896c1139bdbeb52566623ab25`다.

최신 product payload package는 `0.42.40-admin-smoke`다. Web/TUI QoS/guest readback
surface package evidence는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`가
소유한다. Installed Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`에서
04240 기준 PASS로 재확인했다.
설치본 PCVCLI QoS/guest targeted smoke는
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`가
기록한다. Actual VM Web/TUI QoS/guest readback smoke는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`에서
기록하며 설치본 TUI row projection blocker를 `0.42.41-admin-smoke` package chain trigger로
남겼다. Historical 0.42.38 VM media/resource mutation route promotion과 0.42.37
Hyper-V pause lifecycle smoke는 predecessor로 보존한다. Public trusted signing 또는
외부 stable publication evidence가 아니며, 아래 이전 날짜 current 문단은 historical
predecessor로 해석한다.

## 2026-05-18 현재 기준

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

PR #156 post-merge public-boundary main push는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`, run `26017721669`, job `76471545641`, head `a4509c552c003ee0fc87b54b26529686e6dfeb84`에서 PASS했고 historical public-boundary anchor로 보존한다. PR #155, PR #154, PR #152 public-boundary evidence도 historical predecessor로 보존한다. PR #153 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`, run `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`로 보존한다. 이후 사용자 승인으로 0.42.30 package chain을 열어 `full-admin-host-mutation-gate-20260518-04230`과 `manual-admin-campaign-descriptor-20260518-04229-04230-closed`를 current installed/package anchor로 승격했다.

Historical `0.42.24-admin-smoke` Runtime/API current evidence rollup은 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`로 보존한다. Historical `0.42.24-admin-smoke -> 0.42.25-admin-smoke` package-pair는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`, descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`, target/full-gate MSI SHA-256 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance commit `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`로 보존한다. Historical `0.42.25-admin-smoke -> 0.42.26-admin-smoke` package-pair는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`, descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`, target MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP SHA-256 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`, provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`로 보존한다. Historical PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`이다. 이 evidence들은 public trusted signing 또는 외부 stable publication evidence가 아니다.

이 디렉터리는 Desktop Node Phase 12 Service-first 제품 런타임 wrapper다.

현재 wrapper 요약:

- 기본 제품 service host는 WinSW가 아니라 `DesktopNode.Host.exe` .NET Windows Service 실행 파일이다.
- 최신 product payload package record는 `0.42.30-admin-smoke`다. Package evidence는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04230.md`,
  artifact root는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`,
  MSI SHA-256은 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`,
  provenance commit은 `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`다. 초기 clean package
  `artifacts/admin-smoke-package-20260518-04230`, MSI SHA-256
  `c80be181ab99e9d9d5d7f59d7eb40c22841fa202dea36dcff549e5ba94552763`는 file version
  보정 전 superseded artifact로 보존한다.
- 최신 internal admin-smoke full-gate/current-card anchor는 `0.42.30-admin-smoke`다.
  Full gate evidence는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`,
  installed operator surface evidence는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`다.
  0.42.30 full-gate MSI SHA-256은
  `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, provenance
  commit은 `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`다. 최신 닫힌 manual-admin package-pair
  evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`,
  descriptor `manual-admin-campaign-descriptor-20260518-04229-04230-closed`,
  update ZIP SHA-256 `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`,
  `missing_count=0`, `not_pass_count=0`이다. Host Ops lifecycle descriptor bridge는
  `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`으로 current-card와
  Web diagnostics bucket table `host-ops-web-diagnostics-bucket-table-v1`에 연결된다.
  Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` package-pair는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` /
  `manual-admin-campaign-descriptor-20260517-04227-04228-closed`로 보존한다.
  Full admin host mutation batch는 `full-admin-host-mutation-gate-20260517-04228`이다.
  Target MSI SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`,
  clean package MSI SHA-256은 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`,
  update ZIP SHA-256은 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`,
  provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`이다.
  Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` /
  `manual-admin-campaign-descriptor-20260517-04226-04227-closed`이며
  `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
  계약을 보존한다.
  2026-05-16 0.42.25 -> 0.42.26 descriptor는
  `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`
  initial blocked candidate로 보존한다.
  Historical `0.42.22-admin-smoke -> 0.42.23-admin-smoke` package-pair는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`와
  `manual-admin-campaign-descriptor-20260516-04222-04223-closed`로 보존한다.
  Target MSI SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`,
  update ZIP SHA-256은 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`,
  provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`이다.
- Legacy WinSW PowerShell Local API generation은 retired error로 차단하며 product manifest는 `helper_script`/`api_script` path를 기록하지 않는다.
- 제품 기본 bearer token source는 DPAPI LocalMachine protected token file이다.
- 활성 operator client payload는 `pcvcli.exe`와 Web Console이다. TUI payload는 ADR-0011에 따라 제거됐다.
- 장기 진단 정책은 JSONL first versioned diagnostics contract를 따른다.
- LAN mode는 loopback 기본값, preview/admin opt-in, reverse proxy/TLS 전제, non-loopback static bearer auth, firewall admin opt-in lifecycle로 고정한다.
- Update/rollback/config migration은 manifest-first safe update 정책을 따른다. Update는 local `-SourceRoot` payload, file/HTTPS ZIP `-SourceUri` source gate, file/HTTPS JSON `-UpdateCatalogUri`/`-UpdateChannel` catalog gate를 지원하며, remote package는 `-ExpectedSha256` 또는 catalog SHA-256 검증과 extract-before-service-stop preflight를 통과해야 한다.
- ADR-0004는 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`을 내부 전용 서비스 범위의 현재 결정으로 채택한다.
- `DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only`에 따라 public trusted signing, trusted timestamp, 외부 stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke는 `out-of-scope`다.
- ADR-0002는 release/version policy와 installer artifact/channel contract를 현재 적용 결정으로 채택한다.
- ADR-0003은 내부 서비스 운영용 internal Root/leaf `RequireSigned` signing trust model을 채택한다.
- ADR-0005 `public-distribution-operations-expansion-candidate`는 미채택/종료 상태이며 `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`는 보존용 closed-not-adopted matrix다. 현재 적용 gate는 ADR-0006 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`이며 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke, Lifecycle/Packaging current rebaseline을 중심으로 추적한다. Public ops bundle, winget validate, Burn/MSIX, Credential Manager, Event Log, service token, diagnostic bundle, installed listener load/rate-limit evidence는 역사/내부 운영 evidence로 보존하고 public trusted signing/external stable publication은 주장하지 않는다.
- Public distribution ops execution bundle evidence는 `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`다. `tools/New-PcvPublicDistributionOperationsBundle.ps1`는 `artifacts/public-distribution-ops-execution-bundle-20260509-0391`에서 ADR-0005 preflight 13개를 local non-mutating bundle로 실행/수집했고 `public_distribution_ops_execution_bundle=code-level-nonmutating-bundle-pass`, `actual_execution=local-preflight-bundle-executed`, `host_mutation_performed=false`, public trusted signing/external stable publication `not-claimed`를 기록한다.
- Winget CLI validate evidence는 `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`다. `artifacts/winget-cli-validate-20260509-0391`에서 generated singleton manifest preview에 대해 실제 `winget validate --manifest`를 실행했고 exit code `0`으로 PASS했다. Submission은 실행하지 않았다.
- Public external gates blocked evidence는 `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`다. `artifacts/public-external-gates-blocked-20260509-0391`에서 SignTool/winget/gh availability는 확인했지만 public signing material, timestamp URL, upload endpoint/credential, public stable installer URL, public clean-host publication input 부재로 timestamp, external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback은 public scope에서 blocked/out-of-scope다. Internal clean-host install/update/rollback은 ADR-0006 evidence에서 별도로 PASS다.
- Public ops final follow-up attempt evidence는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`다. `packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1`가 `artifacts/public-ops-final-followup-attempt-20260509-0391`에 1-7 final public operations follow-up prerequisite scan을 기록했고 `remaining_follow_up_count: 7`, `actual_execution=local-final-followup-prerequisite-scan-executed`, `host_mutation_performed=false`, `public_release=not-claimed`를 유지한다. Public trusted signing/external stable publication은 주장하지 않는다. 2026-05-10 local 재생성에서도 `ok=true`, `remaining_follow_up_count=7`, `host_mutation_performed=false`를 유지했다.
- Public ops gate execution readiness evidence는 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`다. `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`가 `artifacts/public-ops-gate-execution-readiness-20260509-0392`에 6개 잔여 gate readiness를 기록했고 TLS는 `partial-code-level-cert-generate-rotate-delete-pass`, `tls_private_key_material_written=false`, `tls_binding=not-run`이었다. External stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback은 blocked이며 public trusted signing/external stable publication은 주장하지 않는다. Credential Manager SYSTEM proof blocker는 후속 installed evidence에서 닫혔다. 2026-05-10 `-RunLocalTlsLifecycle` 재생성에서도 `ok=true`, `host_mutation_performed=false`, public release `not-claimed`를 유지했다.
- Public ops installed hardening evidence는 `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`다. `DesktopNode.Host.exe service-action credential-manager-system-proof`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard` code-level PASS 이후 Credential Manager installed SYSTEM proof/default token-source migration, Event Log installed default writer/repair/remove/volume/schema, internal HTTPS binding이 PASS로 닫혔다. Public trusted signing/external stable publication은 주장하지 않는다.
- Installed account login/noVNC bridge follow-up evidence는 `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`다. `Invoke-PcvInstalledAccountLoginSmoke.ps1`는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 installed admin smoke PASS를 기록했고, 필요 시 `-RunBrowserQa`, `-BrowserQaUrl`, `-BrowserQaArtifactRoot`로 설치본 Web Console browser QA를 같은 임시 account token 경계 안에서 이어 실행할 수 있다. `DesktopNode.Host.exe listen`은 explicit `--novnc-target-host`/`--novnc-target-port` 구성에서 `/api/v1/console/novnc/{vm_id}` WebSocket-to-VNC TCP bridge를 제공한다. Target-backed noVNC installed streaming과 installed TUI operator smoke는 `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`에서 PASS로 기록한다. 이 evidence는 public trusted signing/external stable publication은 주장하지 않는다.
- Frontend/backend auth console live smoke evidence는 `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`다. `artifacts/installed-account-login-browser-live-smoke-20260510-235543`와 `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`에서 설치본 Web Console real account login form, auth/session/RBAC/console route, diagnostic create/download, responsive screenshot QA가 PASS였다. `artifacts/installed-web-asset-refresh-20260510-235258`의 installed `web/app.js` refresh도 기록하며 token/password 값은 evidence에 남기지 않는다.
- Web/API port split evidence는 `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`와 `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`다. Product plan 기본값은 Web Console `http://127.0.0.1:80/`, Web API `http://127.0.0.1:7777/`이며 `/pcv-config.js`가 browser API base URL을 주입한다. 설치본 smoke artifact `artifacts/installed-port-split-20260510-010714-0392`는 service `PathName` `--web-prefix "http://127.0.0.1:80/"`, Web `200`, API `200`, Web-port API `PCV_API_ROUTE_ON_WEB_PORT`, CORS preflight `204`를 PASS로 기록한다. 80번 Web Console browser QA artifact는 `artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80`이며 diagnostics create/download와 responsive screenshots를 PASS로 기록한다. Public trusted signing/external stable publication은 주장하지 않는다.
- Internal HTTPS/TLS lifecycle installed smoke는 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`, `artifacts/internal-https-tls-lifecycle-installed-20260510-0397`에서 PASS했다. `Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1`는 temporary LocalMachine cert, HTTP.sys SSL binding, service HTTPS prefix, cert rotation, binding/cert removal, original HTTP service restore를 확인한다.
- Internal clean-host install/update/rollback smoke는 `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, `artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417`에 기록했다. 현재 상태는 `pass`다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed update/rollback, internal clean-host update/rollback PASS로 닫혔다.
- Clean-host Windows Update NoContact recovery guard는 `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`에 기록했다. `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`는 Windows Update reboot 이후 heartbeat `NoContact` + CPU idle 상태가 threshold 이상 지속되면 VM power cycle recovery를 한 번 수행하고 `recovery_actions`를 남긴다. 실제 clean-host execution은 계속 manual-admin/elevated opt-in 범위다.
- Latest full admin host mutation gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`다. `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04230`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`, `artifacts/os-mutation-gates-batch-profile-20260518-04230`에서 `0.42.30-admin-smoke` Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store gate와 installed listener `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260518-04230`, `runtime-api-current-evidence-rollup-v1` current-card smoke를 PASS로 확인했다. full-gate MSI SHA-256은 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, superseded initial clean package MSI SHA-256은 `c80be181ab99e9d9d5d7f59d7eb40c22841fa202dea36dcff549e5ba94552763`, provenance commit은 `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`, signing mode는 `AllowUnsignedDev`다. Runtime/API registry bridge는 `runtime-api-diagnostics-ops-summary-registry-bridge-v2`로 operator-visible ops summary와 Web Console diagnostics panel에 표시되고 route detail count는 `4`다. Installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`에서 PASS했고 Host Ops Web diagnostics bucket table contract는 `host-ops-web-diagnostics-bucket-table-v1`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Latest manual-admin package-pair evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`다. `0.42.29-admin-smoke -> 0.42.30-admin-smoke` installed update/rollback, clean-host with Windows Update, Burn, MSIX, installed runtime ops summary, descriptor generation v2, installed current-card recheck가 PASS다. Target operational MSI SHA-256은 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, update ZIP SHA-256은 `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`, provenance commit은 `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`다. Descriptor `manual-admin-campaign-descriptor-20260518-04229-04230-closed`는 `missing_count=0`, `not_pass_count=0`이다. 이전 `0.42.28-admin-smoke -> 0.42.29-admin-smoke`, `0.42.27-admin-smoke -> 0.42.28-admin-smoke`, `0.42.26-admin-smoke -> 0.42.27-admin-smoke`, `0.42.25-admin-smoke -> 0.42.26-admin-smoke`는 historical closed package-pair로 보존한다.
- Previous full admin host mutation gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`다. `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`에서 `0.42.20-admin-smoke` Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store gate와 installed listener `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04220` current-card smoke를 PASS로 확인했다. full-gate MSI SHA-256은 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, clean package MSI SHA-256은 `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`, provenance commit은 `0895d018935298721b25b5d9ce1ae083a6690c25`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 historical predecessor이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Historical manual-admin package-pair evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`와 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`다. `0.42.11-admin-smoke -> 0.42.12-admin-smoke` target MSI SHA-256은 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, update ZIP SHA-256은 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`이고, `0.42.12-admin-smoke -> 0.42.13-admin-smoke` target MSI SHA-256은 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`다. 두 evidence 모두 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Latest product payload package record는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04230.md`다. `0.42.30-admin-smoke` operational package root는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`, MSI SHA-256은 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, provenance commit은 `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`이다. 초기 clean package `artifacts/admin-smoke-package-20260518-04230`, MSI SHA-256 `c80be181ab99e9d9d5d7f59d7eb40c22841fa202dea36dcff549e5ba94552763`는 file version 보정 전 superseded artifact로 보존한다. Descriptor selector guard는 `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`, selectorfix version `0.42.14-admin-smoke`, selectorfix MSI SHA-256 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`로 보존한다. `0.42.29-admin-smoke`, `0.42.28-admin-smoke`, `0.42.27-admin-smoke`, `0.42.26-admin-smoke`, `0.42.25-admin-smoke` package/fullgate/current-card는 historical predecessor로 보존한다. 최신 닫힌 package-pair PASS는 `0.42.29-admin-smoke -> 0.42.30-admin-smoke`이며 다음 후보는 다음 product payload 이후 `0.42.30-admin-smoke -> 0.42.31-admin-smoke`다.
- `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`는 `0.42.10-admin-smoke` duplicate outer `sc.exe start` 1056 failure를 historical RCA로 보존한다. `0.42.11-admin-smoke`가 `native-service-action-controls-final-state`로 닫았으며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Post-04212 follow-up triage evidence는 `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`다. `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이 없어 `0.42.13-admin-smoke` package build, full admin host mutation, `0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign은 열지 않았다. Clean-host recovery summary key는 다음 실제 run의 `recovery_actions`와 `automatic_recovery_performed`로 판정한다.
- Post-04212 `1-2-3-4-5` current-card follow-up evidence는 `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`다. `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product payload 변경이 없어 `0.42.13-admin-smoke` package/host mutation chain은 보류했고, `artifacts/web-console-current-card-20260514-04212-rerun-followup`에서 Dashboard와 Evidence view가 `full-admin-host-mutation-gate-20260514-04212-rerun`, `0.42.12-admin-smoke`를 표시함을 확인했다. 이 evidence는 host mutation을 실행하지 않았다.
- Product wrapper native repair package evidence는 `docs/ga-ready/evidence/product-wrapper-native-repair-package-2026-05-13-04211.md`다. `RepairInstalled -BatchEvidenceRoot`가 native `DesktopNode.Host.exe service-action repair-installed`를 호출하고 `native-service-action-controls-final-state` reason으로 outer start를 skip해 SCM `PathName`과 current-card root를 안정화한다.
- Burn bootstrapper lifecycle evidence는 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`다. `artifacts/burn-bootstrapper-lifecycle-20260510-0416`에서 bundle SHA-256 `5e67bd3a1fed7262447531000328825180fd678b252170793cf88e50fc41535d`, install/repair/remove exit `0`, direct MSI restore, final service `Running`을 확인했다. Public trusted signing/external stable publication은 주장하지 않는다.
- Windows Credential Manager/Event Log provider evidence는 `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`다. Credential Manager는 current-user write/read/delete capability PASS 이후 installed LocalSystem default transition PASS를 기록했고, Event Log provider는 installed native provider register/write/query와 installed default writer hardening PASS를 기록한다.
- Installed listener external load/rate-limit evidence는 `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`다. `artifacts/installed-listener-external-load-rate-limit-20260509-0391`에서 설치된 listener에 HTTP 요청 180개를 보내 200 `140`, 429 `40`, unexpected `0`, 모든 429의 `Retry-After`/`PCV_RATE_LIMIT_EXCEEDED` problem details를 확인했다. Token value는 evidence에 기록하지 않았다.
- MSIX package lifecycle evidence는 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`다. `0.41.5-admin-smoke` baseline payload and `0.41.6-admin-smoke` target payload에서 `PureCVisor.DesktopNode.MsixSmoke` package identity와 `PureCVisorDesktopNodeMsixSmoke` packaged service를 빌드해 install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence를 PASS로 확인했다. 이 evidence는 internal Root/leaf signing과 restricted service capability smoke이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- MSI/update package apply evidence는 `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`다. `artifacts/msi-update-package-20260509-0391`에서 `0.39.1-admin-smoke` MSI build, update ZIP/catalog validation, elevated MSI apply, final installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 PASS로 확인했다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`다. 이 evidence는 AllowUnsignedDev internal admin-smoke evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Previous full admin host mutation gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`다. `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`에서 `0.42.9-admin-smoke` Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store gate와 installed listener `batch_evidence.status=available` current-card smoke를 PASS로 확인했다. full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. `0.42.8-admin-smoke` / 0428 evidence와 이전 evidence는 historical로 보존한다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Historical manual-admin package-pair evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`다. `0.42.7-admin-smoke -> 0.42.8-admin-smoke` installed update/rollback, dedicated clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 PASS이며 descriptor는 `artifacts/manual-admin-campaign-20260512-0427-0428/manual-admin-campaign-descriptor/summary.json`이다. Post-merge rebuild는 `artifacts/admin-smoke-package-20260512-0428-postmerge`, MSI SHA-256 `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, provenance commit `5397e580c98a34e8b7beb5b9773d1d857025315b`로 보존한다. 0425→0426 package-pair와 0427 full gate는 historical predecessor로 보존한다.
- Historical full admin host mutation gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-rerun.md`다. `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-032525-0391-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-032525-0391-rerun`, `artifacts/os-mutation-gates-batch-profile-20260509-032525-0391-rerun`에서 `0.39.1-admin-smoke` Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store gate를 PASS로 확인했다. MSI SHA-256은 `25a88e41ed926a6bccaf3eba1fdd44d0976091aca9fd6ef77f52eea2bddf3c37`, provenance commit은 `0815a6281bcb98b5b1795e8d054073e1c9fb4892`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Standalone product wrapper asset copy는 repo migration follow-up 이후 repo-root `web/**` Web Console 자산만 제품 asset으로 stage한다. Legacy API/Hyper-V/service component 자산은 component/archive baseline으로만 남기며 standalone product asset source가 아니다. MSI installed payload는 product wrapper, `DesktopNode.Host.exe`, repo-root `web/**`, manifest만 stage한다.

## 기본 경로

- 제품 루트: `C:\Program Files\PureCVisor\DesktopNode`
- 데이터 루트: `%ProgramData%\PureCVisor\desktop-node`
- 서비스명: `PureCVisorDesktopNode`
- 기본 Web Console URL: `http://127.0.0.1/`
- 기본 Web API URL: `http://127.0.0.1:7777/api/v1/...`
- CLI 실행 파일: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
- TUI payload: ADR-0011에 따라 제거됨
- 설치 로그: `%ProgramData%\PureCVisor\desktop-node\install.jsonl`
- 진단 번들: `%ProgramData%\PureCVisor\desktop-node\diagnostics\`
- 기본 protected token file: `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`
- legacy raw token file: `%ProgramData%\PureCVisor\desktop-node\api-token.txt`
- 서비스 실행 파일: `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`
- 서비스 로그: `%ProgramData%\PureCVisor\desktop-node\service-logs\`
- job store primary: `%ProgramData%\PureCVisor\desktop-node\jobs.json`
- unresolved commit guard: `%ProgramData%\PureCVisor\desktop-node\jobs.json.commit-pending`

## 설치본 사용

기본 설치본은 loopback-only service로 실행한다.

```powershell
Start-Process "http://127.0.0.1/"
Get-Service PureCVisorDesktopNode
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

Web Console static root는 loopback에서 bearer token 없이 먼저 열 수 있다. API route와 non-loopback LAN exposure는 bearer token을 요구한다. Token 값은 command line에 넣지 않고 protected token file 경로만 전달한다.

Service 재시작이 필요하면 Windows SCM service를 기준으로 조작한다.

```powershell
Restart-Service PureCVisorDesktopNode
```

LAN exposure는 기본 설치 상태가 아니다. `DesktopNode.Host.exe service-action firewall-enable|firewall-remove`와 non-loopback listener는 관리자 opt-in gate에서만 실행하고 final firewall rule absence 또는 intended final state를 evidence에 남긴다.

일반 사용자 절차는 `docs/USER_GUIDE.md`의 Web Console, VM, checkpoint, job sections를 따른다.

## API/Host Job Hardening Installed Smoke

Job store physical writer는 `jobs.json.tmp.<GUID-N>`와
`jobs.json.commit-pending.tmp.<GUID-N>`를 각각 flush한 뒤 fixed pending marker→primary 순서로
commit한다. Fixed marker가 남으면 current runtime이 candidate/previous identity를 reconcile할 때까지
job mutation/dispatch를 차단한다. Product Update/Rollback과 preserve-data RemoveInstalled/Uninstall도
service stop+wait 뒤 marker 존재/검사 실패를 fail-closed한다. Marker를 삭제하거나 구 binary로
rollback하지 말고 `docs/OPERATIONS_GUIDE.md`의 Pending-commit recovery 절차를 따른다.

명시적 `-RemoveData`만 `jobs.json`, fixed marker, legacy `jobs.json.tmp`와 exact GUID-N orphan temp를
allowlist로 제거한다. 이름이 비슷하지만 GUID-N이 아닌 파일은 제거하지 않는다.

설치본 API host/job hardening 검증 runner는 `packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1`다. 기본 실행은 설치된 listener에 HTTP 요청만 보내며 service 재시작, SCM `binPath` 변경, MSI install/repair, firewall, trust-store, Hyper-V VM mutation을 수행하지 않는다. 실행 evidence 문서 작성은 별도 설치본 evidence 실행 범위다.

계획/비변경 확인은 설치본 service나 관리자 권한 없이 실행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -ArtifactRoot artifacts/api-host-job-hardening-installed-evidence-dryrun -DryRun
```

설치본 listener smoke는 기본적으로 `PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN` process environment variable에서 bearer token을 읽는다. Token 값은 command line, command history, summary에 남기지 않고 `token_value_observed=false`를 유지한다. 대화형 실행에서는 secure prompt로 token을 읽은 뒤 environment variable name만 runner에 전달한다.

```powershell
$secureToken = Read-Host -AsSecureString -Prompt 'Installed listener bearer token'
$tokenPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
try {
    $env:PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenPtr)
    pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -ArtifactRoot artifacts/api-host-job-hardening-installed-evidence-20260511 -ApiBaseUri http://127.0.0.1:7777
} finally {
    if ($tokenPtr -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenPtr) }
    Remove-Item Env:\PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN -ErrorAction SilentlyContinue
}
```

CI나 sealed local shell에서는 같은 process-scoped environment variable을 미리 주입하고, 실행 뒤 즉시 지운다. 다른 이름을 써야 하면 raw token 값 대신 `-BearerTokenEnvironmentVariableName`에 environment variable name만 전달한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -ArtifactRoot artifacts/api-host-job-hardening-installed-evidence-20260511 -ApiBaseUri http://127.0.0.1:7777
Remove-Item Env:\PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN -ErrorAction SilentlyContinue
```

`-BearerToken` 직접 전달은 기존 호출 호환성 용도만 남긴다. 이 방식은 process argv에 raw token이 노출될 수 있으므로 권장하지 않는다.

이 runner는 oversized `POST /api/v1/auth/login`의 HTTP `413`/`PCV_REQUEST_BODY_TOO_LARGE`, `GET /api/v1/runtime/policy`, `GET /api/v1/jobs?limit=1&offset=0`, `GET /api/v1/diagnostics/bundles?limit=1&offset=0`, `GET /api/v1/console/capabilities`, 없는 job에 대한 `POST /api/v1/jobs/pcv-installed-hardening-missing-job/cancel`, service `Running`, read route responsiveness를 summary에 기록한다. 기본 실행에서는 controlled route-timeout probe와 rate-limit load probe를 실행하지 않는다. `-RunRouteTimeoutProbe`는 설치 listener가 `--controlled-route-timeout-probe-delay-ms`로 명시 구성된 경우에만 `GET /api/v1/runtime/route-timeout-probe`의 HTTP `504`/`PCV_ROUTE_TIMEOUT`/`Retry-After`/`application/problem+json` contract를 확인한다. `-RunRateLimitProbe`는 명시적으로 요청한 경우에만 `/api/v1/runtime/policy` 반복 요청으로 HTTP `429`/`PCV_RATE_LIMIT_EXCEEDED`를 확인한다. `wmi_abort_claim`은 항상 `not-claimed`이며 public trusted signing과 external stable publication도 claim하지 않는다.

2026-05-11 installed route-timeout follow-up evidence는 `docs/ga-ready/evidence/api-host-job-hardening-installed-route-timeout-2026-05-11.md`다. 이 evidence는 `0.41.8-admin-smoke` payload update, 임시 service-action repair, installed `504 PCV_ROUTE_TIMEOUT` PASS, 그리고 기본 PathName 복원 후 controlled probe `404 PCV_ROUTE_NOT_FOUND`를 기록한다.

## Dry-run plan

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall -RemoveData -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

## 현재 .NET service host

제품 wrapper의 기본 plan은 Windows SCM에 `DesktopNode.Host.exe listen`을 등록한다.

- Service `binPath`는 loopback prefix, Web root, job store, event log, diagnostics root, protected token file, route timeout, request limit, burst, retry-after 값을 인자로 전달한다.
- 기본 `binPath`는 API listener `--prefix http://127.0.0.1:7777/`와 Web Console listener `--web-prefix http://127.0.0.1:80/`를 함께 전달한다.
- Raw token 값은 command line에 넣지 않는다.
- 기본 plan은 WinSW executable staging이나 WinSW XML 생성을 하지 않는다.
- Installed 또는 개발 listener는 `DesktopNode.Host.exe listen --batch-evidence-root <path>`로 시작할 때 `GET /api/v1/ops/summary`의 `data.batch_evidence`에 compact read-only Batch Supervisor evidence summary를 노출할 수 있다. Listener는 HTTP request에서 evidence path를 받지 않으며, 최신 Batch Supervisor summary 누락은 `missing`, 최신 summary parse/read 실패는 `unavailable`, child evidence(route/MSI/Hyper-V, OS mutation, provenance, MSI lifecycle, GPU snapshots) 누락 또는 malformed/partial 상태는 `degraded` `batch_evidence.status`와 signal로 보고한다. 응답에는 command stdout/stderr, command arguments, bearer token, protected token file content/path, absolute evidence root, repository root를 싣지 않는다.
- `DesktopNode.Host.exe service-action configure-installed|repair-installed --batch-evidence-root <path>`는 installed service `PathName`에 Batch evidence root를 제품 옵션으로 기록한다. `repair-installed`는 explicit 값이 없으면 기존 `PathName`의 `--batch-evidence-root`를 보존하고, explicit 값이 있으면 override한다. MSI는 `BATCH_EVIDENCE_ROOT="<path>"` public property로 configure/repair custom action에 전달할 수 있고, standalone wrapper는 `Invoke-PcvDesktopNodeProduct.ps1 -BatchEvidenceRoot <path>`를 plan에 기록한다.

`docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`는 `DesktopNode.Host.exe service-action configure-installed|repair-installed` C# native SCM config도 product wrapper plan과 동일한 `--diagnostics-root`, protected token file, hardening arguments를 `BinaryPathName`에 기록하는지 확인한다. 이 evidence는 code-level guard이며 installed service mutation이나 installed diagnostic bundle listener PASS 자체를 주장하지 않는다. Installed listener execution은 후속 `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`의 `0.39.0-admin-smoke` elevated MSI/service rerun에서 `installed-listener-pass`, blocker `none`으로 닫혔다.

MSI installed path는 product wrapper, `DesktopNode.Host.exe`, Web Console payload staging까지 소유하는 1차 검증 경로다. Standalone product wrapper `Install`은 개발자 smoke나 이미 product root에 host executable이 staging된 환경에서만 사용한다.

관리자 opt-in standalone install smoke 예:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install
```

Product manifest와 diagnostic bundle은 `service_host` mode, staged executable path, SHA-256, SCM status, service log redaction artifact를 기록한다.

기본 product health check는 protected token file이 있으면 bearer token을 읽어 `/api/v1/runtime/policy`를 확인한다.

Loopback Web Console static asset은 bearer token 없이 먼저 열 수 있다. API route는 계속 bearer token을 요구한다. Separate Web listener mode에서는 `/pcv-config.js`가 `http://127.0.0.1:7777` API origin을 주입하고, Web listener의 `/api/*` request는 `PCV_API_ROUTE_ON_WEB_PORT`로 거부된다. Non-loopback LAN mode에서는 static asset도 token 정책을 유지한다.

`Uninstall`과 `Rollback`은 `sc.exe stop` 이후 `sc.exe query`가 stopped/missing 상태가 될 때까지 기다린 뒤 service delete, product root 제거, 이전 product root 복원을 진행한다. Service host executable lock 등 일시적인 접근 거부가 발생하면 제거를 제한적으로 재시도한다.

## Route parity mutation smoke

설치본 기준 .NET Host route parity를 실제 service/MSI/Hyper-V mutation으로 검증할 때는 tracked 도구인 `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`를 사용한다.

- 이 도구는 stdout/stderr를 비동기 drain한 뒤 child process 종료를 기다려 pipe deadlock을 피한다.
- `progress.json`에는 단계 시작/완료 marker를 먼저 남긴다.
- `Get-VM` wildcard 조회는 `Get-VM | Where-Object Name -like 'pcv-spike-*'` 형태로 수행해 Hyper-V provider wildcard binding 오류가 원래 실패 원인을 가리지 않게 한다.
- 설치본 bearer token read는 spike service module import 없이 runner 내부에서 protected token file schema와 DPAPI LocalMachine scope를 직접 검증한다.

캡처/토큰 회귀 self-test:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1 -SelfTest
```

관리자 opt-in full smoke는 실제 service install/start/delete, MSI install/repair/uninstall/`REMOVE_DATA=1`, Hyper-V VM/checkpoint lifecycle을 수행한다.

MSI 호출은 계속 `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/norestart`를 포함하고, `Restart-Computer`는 실행하지 않는다.

2026-05-02 route parity evidence 요약:

- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729`: `0.26.6-admin-smoke` unsigned MSI build, direct service-action, MSI lifecycle, installed .NET Host `host.status`/`network.inventory`/VM/checkpoint route smoke PASS.
- Code-level slice: `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native read adapter product path로 전환됐고 read route PowerShell helper fallback은 제거됐다. VM create/start/shutdown/poweroff/restart/delete는 C# native lifecycle adapter product path로 전환됐고, checkpoint create/restore/delete는 C# WMI snapshot service adapter product path로 전환됐다. Native VM create는 이번 slice에서 Hyper-V Generation 2만 지원하며, native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다.
- Product diagnostic self-audit는 현재 active product runtime contract로 `dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation`만 유효로 인식한다. 이전 helper/hybrid boundary 값은 historical evidence 해석용 문서 맥락으로만 남고 current self-audit pass 조건에는 포함하지 않는다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`: `0.26.8-admin-smoke` 설치본 smoke PASS. Installed `network.inventory`는 `source=hyperv`, `mutating=false`, `Default Switch`를 반환했다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`: `0.26.9-admin-smoke` 설치본 smoke PASS. `repair-installed` missing service 재생성, native topology parity fallback, shared request processor 직렬화가 반영됐다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`: `0.27.1-admin-smoke` 설치본 smoke PASS. Installed `host.status`는 native C# adapter로 Windows 10 Pro for Workstations `25H2`, `supported=true`, admin elevated, Hyper-V enabled, VMMS running, Default Switch present를 반환했다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-140824`: `0.27.6-admin-smoke` 설치본 smoke PASS. Commit `3178a62bcf22d00977bf564063befa8e2b2562a5`, MSI SHA-256 `4485fc3aba902d38a5d1293e9231497ae5f35b4c0730d1815c8df561a67c009c`. 당시 installed runtime policy는 `native_probe_operations=[host.status,network.inventory,vm.list,checkpoint.list]`와 `mutation_dispatch=helper-process-direct`를 반환했다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283`: `0.28.3-admin-smoke` 설치본 smoke PASS. Checkpoint create/delete native mutation adapter가 installed .NET Host route smoke에서 성공했고 installed runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.delete]`와 `mutation_dispatch=native-checkpoint-mutation-plus-helper-process-remainder`를 반환했다.
- `artifacts/routeparity-service-msi-hyperv-restore-mutation-20260503-0286`: `0.28.6-admin-smoke` 설치본 smoke PASS. Checkpoint create/restore/delete native mutation adapter가 installed .NET Host route smoke에서 성공했고 installed runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`와 `mutation_dispatch=native-checkpoint-mutation-plus-helper-process-remainder`를 반환했다. Restore smoke는 `vm.poweroff-before-restore` 최소 안정 조건으로 실행했다.
- `artifacts/routeparity-service-msi-hyperv-vm-create-restart-shutdown-20260503-0290`: `0.29.0-admin-smoke` 설치본 smoke PASS. VM create/start/restart/poweroff와 checkpoint create/restore/delete native mutation adapter가 installed .NET Host route smoke에서 성공했고 final service는 `Running`, boot time은 unchanged, `pcv-spike-*` VM 잔여물은 없었다.
- `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301`: `0.30.1-admin-smoke` 설치본 smoke PASS. VM delete native mutation adapter가 installed .NET Host route smoke에서 managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete guard `PCV_VM_NOT_MANAGED_BY_PURECVISOR`를 반환했고 final service는 `Running`, boot time은 unchanged, `pcv-spike-*` VM 잔여물은 없었다.
- `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`: `0.30.3-admin-smoke` 설치본 smoke PASS. Service/data-root handoff smoke는 service 존재 중 `data-root-remove --remove-data` 차단, `remove-installed --remove-data` handoff-only, service absent 이후 allowlist data-root 삭제와 non-allowlist log 보존을 확인했다. MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore와 installed Hyper-V route smoke도 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1412-0310`: `0.31.0-admin-smoke` 설치본 smoke PASS. Repo migration active path removal 이후 product-owned MSI payload로 service-action, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, installed Hyper-V route smoke를 재확인했고 final service는 `DesktopNode.Host.exe` 경로로 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1515-0320`: `0.32.0-admin-smoke` 설치본 smoke PASS. Standalone product asset boundary 이후 active `spikes/**` product asset 없이 service-action, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, installed Hyper-V route smoke를 재확인했다. Build commit은 `d852ff54bafb403e16e86057b3cecec2813bf0b6`, MSI SHA-256은 `f3e4456e94d5ee16a8e0bd6d02d17ac04d682be5bd58c77098072f97711d25f5`, payload file count는 7이고 final service는 `DesktopNode.Host.exe` 경로로 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `artifacts/eventlog-source-registration-20260504-actual-registry`: `DesktopNode.Host.exe service-action eventlog-register` 실제 Event Log source registry 등록 PASS. `Application` log의 `PureCVisor Desktop Node` source는 installed `DesktopNode.Host.exe`를 `EventMessageFile`로 가리키며, 이 smoke는 service/MSI/Hyper-V/firewall/trust-store mutation 없이 Event Log source registry 등록만 수행했다.
- 2026-05-04 Event Log source removal owner migration slice는 `DesktopNode.Host.exe service-action eventlog-remove` code-level registry-backed action을 추가했다. Fake controller/xUnit으로 owned source removal path와 missing source idempotent success를 확인했고, 실제 Event Log source removal registry mutation은 2026-05-05 `artifacts/os-mutation-gates-20260505-101659-0355-final`에서 처음 register/remove/final absent로 실행했으며, 최신 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`에서 현재 HEAD 기준으로 다시 확인했다.
- 2026-05-04 firewall/trust-store owner migration slice는 `DesktopNode.Host.exe service-action firewall-enable|firewall-remove|trust-store-install|trust-store-remove` code-level native action을 추가했다. Fake controller/xUnit으로 LAN approval gate, release approval gate, owned mutation path, foreign ownership block, missing removal idempotency를 확인했다. 실제 firewall/trust-store mutation은 이후 `0.34.1`, `0.35.4`, `0.35.5`, `0.35.6` admin-smoke artifact에서 별도 관리자 opt-in으로 실행했다.
- `artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330`: 사용자 관리자 opt-in으로 `0.33.0-admin-smoke` Service/MSI/Hyper-V mutation과 row-isolated firewall/trust-store mutation PASS. Build commit은 `dca492c67c0cb3843832d5f6e1e76c8d686c3cdf`, MSI SHA-256은 `e6522114963be755beab1f54e183eef212a9f32979751e1fe67159a20cd2a4ff`, payload file count는 7이다. Firewall-only smoke는 owned inbound allow rule을 `TCP/47778`, `Private`, `LocalSubnet` scope로 create/enable/remove 후 final rule count 0을 확인했다. Trust-store-only smoke는 self-signed test cert thumbprint `18FFB486CB56EBF6AD0C8B841ACF932FE482CACF`를 LocalMachine Root/TrustedPublisher에 import한 뒤 Root/TrustedPublisher/CurrentUser My final absence를 확인했다. Final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다.
- `artifacts/os-mutation-gates-20260505-003459-0341`: 사용자 fast-mode 관리자 opt-in으로 `0.34.1-admin-smoke` current native MSI/firewall/LAN/internal trust-store gate PASS. MSI provenance commit은 `6f97a24aa2bdfacf33d7bd987559eb85e363e119`, MSI SHA-256은 `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`, payload file count는 7이다. MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore는 exit `0`였고 final service는 loopback-only `Running`으로 복구됐다. Native firewall action은 owned rule `PureCVisor Desktop Node Local API LAN`을 `TCP/7777`, `Private`, `LocalSubnet` scope로 create/remove했고 final rule absence를 확인했다. LAN smoke는 `0.0.0.0` prefix unsupported를 기록한 뒤 LAN IP prefix `http://[redacted-private-endpoint]:7777/`에서 bearer token runtime policy `HTTP 200`을 확인했다. Native trust-store action은 기존 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`를 install/remove/restore했고 최종 present 상태를 확인했다. Follow-up commit `49a06acd3493066a10ec26fe541d5d8be1005c2b`는 COM missing-rule lookup의 `FileNotFoundException` edge를 처리한다.
- `artifacts/os-mutation-gates-20260505-033503-0354`: 사용자 재승인으로 `0.35.4-admin-smoke` 실행 당시 HEAD MSI/firewall/LAN/internal trust-store gate를 fresh 실행했다. MSI provenance commit은 `744a15536569e89f948927bea9179fc0eeae3ff4`, MSI SHA-256은 `bf7d0d2bd83545e83fbdf0dfb96b715f8e09471474445ae1c0db1d076be2c1e4`다. MSI install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall은 exit `0`였고, final restore는 internal signed stable `0.35.2` MSI SHA-256 `7d9cf1f7ed157027ff128c3fadfa8fd82576d86166f6a214ac52c7190191e959`로 복구했다. Native firewall action은 owned LAN rule enable/remove 후 final rule count `0`, LAN smoke는 `http://[redacted-private-endpoint]:7777/` runtime policy와 Web root `HTTP 200`, trust-store action은 ADR-0003 internal Root/TrustedPublisher install/remove/restore와 final present 상태를 확인했다. Final service는 loopback-only `Running`, installed DisplayVersion은 `0.35.2`, boot time unchanged다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`와 `artifacts/os-mutation-gates-20260505-101659-0355-final`: 사용자 재승인으로 `0.35.5-admin-smoke` 실제 Hyper-V/MSI/service/data-root/firewall/LAN/Event Log/trust-store gate를 실행했다. MSI provenance commit은 `2fb38f20a8c74433684345ded8a33ba16a863621`, MSI SHA-256은 `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, firewall enable/remove, LAN IP `http://[redacted-private-endpoint]:7777/` runtime policy/Web asset `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service는 loopback-only `Running`, installed DisplayVersion은 `0.35.5`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. Config/job store migration apply는 route matrix상 `future-route/not-implemented/blocked`라 실행하지 않았고, supported job store mutation은 installed runtime job writes와 synthetic data-root allowlist removal evidence로만 기록했다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`: 사용자 재승인으로 `0.35.7-admin-smoke` 현재 HEAD 실제 Hyper-V/MSI/service/data-root/firewall/LAN/Event Log/trust-store gate를 실행했다. MSI provenance commit은 `2ec9e71d45b702e106824c86500cd6152b18fab7`, MSI SHA-256은 `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, firewall enable/remove, LAN IP `http://[redacted-private-endpoint]:7777/` bearer runtime policy/Web asset `HTTP 200`, config-migration-apply blocked/no-mutation descriptor, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service는 loopback-only `Running`, installed DisplayVersion은 `0.35.7`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. Job store migration apply는 route matrix상 `future-route/not-implemented/blocked`라 실행하지 않았다.
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, step `timed_out=false`, `exit_code=0`, heartbeat lines `25`다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`, signing mode는 `AllowUnsignedDev`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370` evidence는 사용자 관리자 opt-in으로 `0.37.0-admin-smoke` full admin host mutation gate를 Batch Supervisor 아래에서 완료했음을 기록한다. MSI provenance commit은 `485b1a7338fb2b682c3964c858ccc13c322950d7`, MSI SHA-256은 `f7fc56ab9ca83ba863008c864894d1ae8d2c0dd4a961895a43d95`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, timeout false였고, Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 PASS였다. MSI lifecycle install/repair/uninstall preserve/install-remove-data/uninstall-remove-data/final restore는 모두 exit `0`였고 VM create/start/restart/poweroff/delete, checkpoint create/restore/delete, unmanaged delete guard, firewall enable/remove, LAN listener IP smoke, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. 첫 batch attempt의 MSI repair `1603`은 direct `repair-installed`와 manual MSI repair exit `0`, 이후 Batch Supervisor `-Resume` PASS로 recovered transient evidence로 분류한다. Final service는 loopback-only `Running`, installed DisplayVersion은 `0.37.0`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387` evidence는 `0.38.7-rc.1` internal enterprise `RequireSigned` MSI build를 완료한 최신 internal signed build evidence다. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 이전 `0.38.4-rc.1` signed build evidence는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`에 보존한다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389`, `artifacts/os-mutation-gates-batch-profile-20260508-202255-0389` evidence는 사용자 관리자 opt-in으로 `0.38.9-admin-smoke` full admin host mutation gate를 Batch Supervisor 아래에서 완료한 historical evidence다. MSI provenance commit은 `159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce`, MSI SHA-256은 `86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, Service/MSI/Hyper-V step `attempt_count=1`, OS mutation gate `attempt_count=1`, timeout false였고, Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 PASS였다. MSI lifecycle install/repair/uninstall preserve/install-remove-data/uninstall-remove-data/final restore는 모두 exit `0`였고 firewall enable/remove, LAN listener IP smoke, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service는 loopback-only `Running`, product manifest version은 `0.38.9-admin-smoke`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390` evidence는 사용자 관리자 opt-in으로 `0.39.0-admin-smoke` MSI/service installed listener rerun을 완료했다. MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false였고 final service는 loopback-only `Running`, product manifest version `0.39.0-admin-smoke`, SCM `PathName` diagnostic bundle/hardening args present다. Installed diagnostic bundle listener create/download는 POST `201`, GET `200`, redaction PASS였다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390` evidence는 사용자 관리자 opt-in으로 `0.39.0-admin-smoke` installed listener 후속 OS mutation gate를 완료했다. Batch summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false였고 firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service는 `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`이며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `batch-evidence-root-service-action-package-2026-05-13-0429`은 `0.42.9-admin-smoke` 이전 product payload package build evidence다. `DesktopNode.Host.exe service-action eventlog-default-transition` timeout guard와 MSI custom action timeout propagation을 포함한다. Artifact는 `artifacts/admin-smoke-package-20260513-0429`, MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. 0429 latest full admin host mutation PASS는 별도 full gate evidence가 소유하며, 이 package build evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-11 `full-admin-host-mutation-gate-2026-05-11-0422-hostmutation`은 `0.42.2-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260511-232659-0422`, `artifacts/os-mutation-gates-batch-profile-20260511-232659-0422`이고 MSI SHA-256은 `e4d66d006cd14355b57507fea3c9a41b6c17a002f9ff824bec35830ce029fc29`, provenance commit은 `1d68a3b6c2ac1d9202d0ec53d0ccb35858d84ee6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.42.2-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `manual-admin-operator-hardening-followup-2026-05-10-0415`는 `artifacts/manual-admin-followup-20260510-0415`에서 installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition을 `0.41.5-admin-smoke` 기준 PASS로 재확인했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `docs/ga-ready/evidence/beta-web-dashboard-smoke-2026-05-07.md` / `artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743` evidence는 Beta-0 Web Dashboard `WebRegression` read-only/static fixture smoke PASS다. `web/tests` Pester 26 tests, `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web/app.js`가 통과했고 Batch summary는 `ok=true`, `status=completed`, `total_steps=4`, `executed_steps=4`다. 이 evidence는 Hyper-V/service/MSI/firewall/trust-store/LAN/update mutation evidence가 아니다.
- `docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md` / `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass` evidence는 `0.38.8-admin-smoke` AllowUnsignedDev installed destructive update/rollback smoke PASS를 기록한다. MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`, provenance commit은 `fd4f854646fc159d54f7578230f00c51f80e201f`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 current root를 `0.38.6-admin-smoke`로 복원하고 `0.38.8-admin-smoke`를 `DesktopNode.failed` diagnostics root로 보존했다. Final service는 `Running`, boot time unchanged, `host_mutation_performed=true`다. 최초 `artifacts/product-update-rollback-mutation-20260507-0388` non-elevated attempt는 blocked history이며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-06 code-level 후속에서 `config-migration-apply`는 supported `product-config-v1-to-v2` plan/version 1에 대해 product manifest backup + same-directory temp replace + rollback diagnostics를 수행하는 actual apply path로 전환됐다. 같은 후속에서 `job-store-migration-apply`는 supported `job-store-v1-to-v2` plan/version 1에 대해 `jobs.json` backup + same-directory temp replace + recovery diagnostics를 수행한다. 두 action 모두 service stopped/runtime writer stopped proof를 요구하고 implicit service stop/start, token mutation, service identity mutation, MSI/update/rollback, Hyper-V/firewall/trust-store/LAN/Event Log mutation은 수행하지 않는다. Evidence는 `docs/ga-ready/evidence/config-jobstore-migration-apply-code-level-2026-05-06.md`이며 installed destructive admin smoke는 별도 gate다.
- `packaging/windows-desktop-node/tools/Invoke-PcvConfigJobStoreMigrationApplySmoke.ps1`는 config/job store migration apply installed destructive admin smoke 전용 runner다. 2026-05-06 `0.38.5-admin-smoke` 시도 `artifacts/config-jobstore-migration-apply-installed-20260506-231702-0385`는 현재 Codex shell이 elevated admin이 아니라 preflight에서 `PCV_MIGRATION_SMOKE_PREFLIGHT_FAILED`로 차단됐고 host mutation은 수행하지 않았다. 이 시도는 PASS evidence가 아니며 current-native 승격 근거로 쓰지 않는다.
- `artifacts/config-jobstore-migration-apply-installed-20260507-0386` evidence는 사용자 관리자 opt-in으로 `0.38.6-admin-smoke` config/job store migration apply installed destructive admin smoke를 완료했다. MSI provenance commit은 `d4259670e0aa90dae869bbd0e35c8910033fb59e`, MSI SHA-256은 `d252110bee12e8c5c129b97474e2e08a51941d79d81d460fd6fe45932b290593`, signing mode는 `AllowUnsignedDev`다. Summary는 `ok=true`, `host_mutation_performed=true`, final service `Running`, product manifest schema `2`, job store schema `2`, boot time unchanged, post-migration API read ok를 확인했다. 이 evidence로 route matrix의 `product config migration apply`와 `job store migration apply` row는 `current-native`로 승격됐다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Installer-ISO VM의 `vm.shutdown`은 guest shutdown integration 미준비 상태를 `PCV_VM_SHUTDOWN_NOT_AVAILABLE` structured failure로 반환함을 확인했다.
- Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD 기반 Gen1 differencing VM으로 확인했다. Installed Local API `vm.shutdown` job은 `succeeded`, final VM state는 `Off`, smoke VM/ProgramData cleanup은 완료 상태다.
- 모든 route parity evidence는 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났고 자동 reboot를 실행하지 않았다.
- 위 evidence는 `AllowUnsignedDev` admin-smoke, scoped test certificate mutation, 또는 ADR-0003 internal trust-store restore 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

## Batch Supervisor / Hang Guard

`packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1`는 JSON manifest 기반으로 저장소 내부 개발/검증 배치를 실행하는 runner다. 제품 런타임 코드가 아니며 service, scheduled task, firewall rule, trust-store entry, reboot action을 등록하지 않는다.

Supervisor evidence root에는 다음 파일이 남는다.

- `batch-manifest.resolved.json`
- `current-step.json`
- `heartbeat.jsonl`
- `gpu-snapshots.jsonl`
- `step-results/*.json`
- `summary.json`

`retry_count`가 `1` 이상인 step은 실패 attempt를 `step-results/<ordinal>-<step>.attempt-NN.json`에 남기고, 최종 aggregate를 `step-results/<ordinal>-<step>.json`에 쓴다. Aggregate에는 `retry_count`, `attempt_count`, `attempts`, `final_attempt`가 포함되며 기존 `ok`, `exit_code`, `timed_out`, `stdout`, `stderr`, `command_fingerprint` field도 유지된다. 재시도 중에는 `current-step.json`과 `heartbeat.jsonl`에 `retrying` 상태가 기록된다.

실행 중인 step attempt는 기본 5초 간격으로 Windows GPU adapter/process memory counter snapshot을 `gpu-snapshots.jsonl`에 JSONL로 남긴다. Counter 수집 실패는 batch 실패로 승격하지 않고 해당 snapshot line의 `status=unavailable`과 `error`로 기록한다.

비파괴 packaging regression 예:

```powershell
Import-Module ./packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 -Force
$artifact = 'artifacts/batch-runs/packaging-regression-local'
$manifest = New-PcvBatchSupervisorManifest `
  -BatchId 'packaging-regression' `
  -RepoRoot (Resolve-Path .).Path `
  -ArtifactRoot $artifact `
  -Profile PackagingRegression
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$manifestPath = Join-Path $artifact 'manifest.json'
Save-PcvBatchSupervisorManifest -Manifest $manifest -Path $manifestPath
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath $manifestPath
```

Host-mutating admin gate profile은 manifest 생성과 dry-run 검토까지는 non-mutating이다. 실제 실행은 항상 `-AllowHostMutation`과 elevated shell을 요구한다. v2 profile은 다음 세 가지다.

- `ServiceMsiHyperVAdminSmoke`: 기존 `Invoke-PcvRouteParityMutationSmoke.ps1`를 감싸 Service/MSI/Hyper-V route parity smoke를 실행하는 profile.
- `OsMutationGate`: `Invoke-PcvOsMutationGateSmoke.ps1`를 감싸 firewall/LAN/Event Log/trust-store OS gate를 실행하는 profile.
- `FullAdminHostMutationGate`: Service/MSI/Hyper-V route parity 이후 OS mutation gate를 순서대로 실행하는 composite profile.
- `ManualAdminCampaignDescriptor`: `New-PcvManualAdminCampaignDescriptor.ps1 -PlanOnly`를 감싸 이미 실행된 MANUAL-ADMIN runner summary들을 descriptor로 묶는 non-mutating profile.

`ServiceMsiHyperVAdminSmoke`는 기본 `service_msi_hyperv_retry_count=1`을 사용한다. `FullAdminHostMutationGate`도 Service/MSI/Hyper-V step은 기본 `1`, OS mutation gate step은 기본 `os_gate_retry_count=0`이다. 이 기본값은 `0.37.0-admin-smoke`에서 관측된 MSI repair `1603` recovered transient를 같은 host mutation 승인 범위 안에서 한 번 재시도하기 위한 runner policy이며, repair 실패 자체를 성공으로 바꾸지 않는다.

`ServiceMsiHyperVAdminSmoke` manifest 생성 예:

```powershell
Import-Module ./packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 -Force
$repo = (Resolve-Path .).Path
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$manifest = New-PcvBatchSupervisorManifest `
  -BatchId "service-msi-hyperv-admin-smoke-$stamp" `
  -RepoRoot $repo `
  -ArtifactRoot "artifacts/batch-runs/service-msi-hyperv-admin-smoke-$stamp" `
  -Profile ServiceMsiHyperVAdminSmoke `
  -ProfileOptions @{
    version = '0.36.2-admin-smoke'
    iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
    routeparity_artifact_root = "artifacts/routeparity-service-msi-hyperv-batch-profile-$stamp-0362"
  }
Save-PcvBatchSupervisorManifest -Manifest $manifest -Path (Join-Path $manifest.artifact_root 'manifest.json')
```

`OsMutationGate` manifest 생성 예:

```powershell
Import-Module ./packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 -Force
$repo = (Resolve-Path .).Path
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$manifest = New-PcvBatchSupervisorManifest `
  -BatchId "os-mutation-gate-$stamp" `
  -RepoRoot $repo `
  -ArtifactRoot "artifacts/batch-runs/os-mutation-gate-$stamp" `
  -Profile OsMutationGate `
  -ProfileOptions @{
    version = '0.36.2-admin-smoke'
    routeparity_artifact_root = 'artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361'
    os_gate_artifact_root = "artifacts/os-mutation-gates-batch-profile-$stamp-0362"
    lan_prefix = 'http://[redacted-private-endpoint]:7777/'
  }
Save-PcvBatchSupervisorManifest -Manifest $manifest -Path (Join-Path $manifest.artifact_root 'manifest.json')
```

Admin profile 검토용 dry-run 예:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/os-mutation-gate-<stamp>/manifest.json -DryRun -AllowHostMutation
```

Manual-admin descriptor manifest 생성 예:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1 -RepoRoot (Resolve-Path .).Path -PassThru
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/manual-admin-campaign-descriptor-20260512-0425-0426/manifest.json -DryRun
```

이 manifest의 기본 batch id는 `manual-admin-campaign-descriptor-20260512-0425-0426`이고
`requires_admin=false`, `mutates_host=false`다. `0.42.7-admin-smoke` package build는
사용자 승인 후 실행됐고 full admin host mutation gate와 installed listener
current-card smoke까지 PASS했다.

- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-11 `full-admin-host-mutation-gate-2026-05-11-0422-hostmutation`은 `0.42.2-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260511-232659-0422`, `artifacts/os-mutation-gates-batch-profile-20260511-232659-0422`이고 MSI SHA-256은 `e4d66d006cd14355b57507fea3c9a41b6c17a002f9ff824bec35830ce029fc29`, provenance commit은 `1d68a3b6c2ac1d9202d0ec53d0ccb35858d84ee6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.42.2-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `manual-admin-operator-hardening-followup-2026-05-10-0415`는 `artifacts/manual-admin-followup-20260510-0415`에서 installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition을 `0.41.5-admin-smoke` 기준 PASS로 재확인했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.

`requires_admin=true` 또는 `mutates_host=true` step은 entrypoint에서 `-AllowHostMutation`을 명시해야 하며 elevated shell에서만 실행한다. 자동 reboot 또는 scheduled-task command는 v1에서 금지한다.

## Phase 14 WiX MSI installer

Phase 14는 `packaging/windows-desktop-node/installer/` 아래 WiX MSI-first installer 산출물을 추가한다.

최근 MSI evidence 요약:

- 기본 구현, Pester 검증, `.NET SDK 10.0.203` + WiX CLI `5.0.2` unsigned dev MSI build 완료.
- 2026-04-30: `0.23.8-rc.1` signed RC MSI smoke와 elevated MSI lifecycle smoke 수집.
- 2026-05-01: `0.23.9-rc.1` local test `RequireSigned` MSI lifecycle, product wrapper update/rollback/config migration, final restore install을 `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`에 기록.
- 2026-05-01: `0.23.10-rc.1` internal enterprise `RequireSigned` MSI build, LocalMachine trust import, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS를 `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`에 기록.
- 2026-05-06: `0.38.4-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`을 `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`에 기록. MSI SHA-256은 `0b4c60d60098f89bd0adea4d183a5224d32b862e9bf69bd6dbaa41077377e8b9`, signing trust model은 `InternalEnterprise`다.
- 2026-05-07: `0.38.7-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`을 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`에 기록. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, signing trust model은 `InternalEnterprise`다.
- 2026-05-07: `0.38.7-admin-smoke` full admin host mutation gate와 update/rollback mutation attempt는 non-elevated shell에서 `PCV_BATCH_ADMIN_REQUIRED`, `sc.exe stop PureCVisorDesktopNode` exit `5`, `PCV_PRODUCT_SERVICE_STOP_TIMEOUT`으로 차단됐고 host mutation은 수행하지 않았다.
- 2026-05-07: `0.38.8-admin-smoke` installed destructive update/rollback smoke는 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`에서 PASS했다. Update journal은 `succeeded/health`, rollback 후 current root는 `0.38.6-admin-smoke`, `DesktopNode.failed` diagnostics root는 `0.38.8-admin-smoke`다. 최초 non-elevated attempt는 blocked history다.
- 2026-05-01: `0.26.0-admin-smoke` unsigned MSI로 .NET service host direct service-action, MSI lifecycle, Hyper-V helper integration smoke를 `artifacts/dotnet-host-admin-smoke-20260501-213444`에 기록.
- Local test signing certificate evidence와 `admin-smoke` evidence는 public trusted signing 또는 외부 stable publication evidence가 아니며, internal enterprise evidence도 외부 public stable publication evidence가 아니다.

- MSI는 Program Files 제품 파일 설치, repair, 제거를 소유한다.
- Product wrapper는 service/data configuration만 소유한다.
- MSI custom action은 `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed`를 호출하며, `REMOVE_DATA=1` uninstall에서는 service 제거 이후 `data-root-remove --remove-data` action을 별도로 실행한다. .NET runner가 protected token file 준비와 SCM service create/configure/start/stop/delete를 native controller로 처리한다.
- 기본 uninstall은 ProgramData를 보존한다. `REMOVE_DATA=1` uninstall은 `remove-installed --remove-data` handoff 이후 별도 `data-root-remove --remove-data` gate에서 token/job/event/install/diagnostics allowlist만 삭제한다.
- WiX는 `%ProgramData%\PureCVisor\desktop-node` data-root ACL을 직접 소유하지 않는다. Product wrapper plan의 `data_acl` policy가 sensitive token file ACL ownership, SYSTEM/Administrators 운영 경계, `REMOVE_DATA=1` 삭제 전 ACL repair 필요성을 기록한다.
- 기존 `Install`/`Uninstall` action은 standalone 관리자 smoke와 개발자 CLI용으로 유지한다.

자세한 build, signing, 관리자 smoke 절차는 [installer README](installer/README.md)를 따른다.

기본 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

WiX CLI가 있는 개발 환경에서는 [installer README](installer/README.md)의 unsigned MSI build 명령도 함께 실행한다.

## Phase 15 Secure token storage

Phase 15는 제품 wrapper 기본 token source를 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`으로 전환한다. 파일은 DPAPI LocalMachine scope로 보호된 bearer token metadata를 담고, .NET service host command line은 `--api-token-protected-file` 경로만 전달한다. Raw token 값은 service command line, product manifest, diagnostic bundle에 기록하지 않는다.

기존 설치에서 protected token file이 없고 legacy `%ProgramData%\PureCVisor\desktop-node\api-token.txt`가 있으면 product wrapper는 legacy token을 protected file로 migration하고 legacy file은 rollback compatibility를 위해 남긴다. `RemoveData`는 protected token file과 legacy raw token file을 모두 삭제 대상으로 다룬다.

관리자 또는 service component smoke에서 protected token file을 직접 준비하는 legacy service helper는 component/archive baseline에만 남긴다. 제품 설치와 MSI installed action은 `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed|data-root-remove` 경로를 사용한다.

## Account/RBAC/JWT auth bootstrap

Account/RBAC/JWT slice는 Windows Desktop Node local auth로만 동작한다. 제품 wrapper와 native service action은 `%ProgramData%\PureCVisor\desktop-node\accounts.json` 및 `%ProgramData%\PureCVisor\desktop-node\jwt-signing-key.txt`를 service binary path에 전달하고, configure/repair 시 두 bootstrap 파일을 준비한다.

Bootstrap은 의도적으로 기본 account/password를 만들지 않는다. `accounts.json`이 비어 있거나 `bootstrap_state`가 `no-default-account`이면 account auth는 configured-but-not-ready 상태이고, 기존 protected bearer token route gate가 계속 authoritative다. 실제 account가 추가된 뒤에만 `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`, `GET /api/v1/auth/session`, `GET /api/v1/auth/rbac`가 JWT/RBAC 운영 경로가 된다.

`RemoveData`는 account file과 JWT signing key file도 삭제 대상으로 다룬다. Diagnostic bundle과 product manifest는 account path/policy만 기록하며 password hash, JWT, refresh token, signing key material은 기록하지 않는다.

## Phase 16 Long-term diagnostics

Phase 16 결정은 `DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred`다.

- `events.jsonl`은 Local API listener/firewall/runtime event의 1차 운영 로그다.
- `install.jsonl`은 product wrapper action lifecycle의 1차 운영 로그다.
- Service host logs는 `%ProgramData%\PureCVisor\desktop-node\service-logs` 아래에 유지한다.
- Product manifest와 plan은 diagnostics policy v1을 포함한다.
- Diagnostic bundle은 `diagnostics-manifest.json`을 포함하고, artifact 목록은 host absolute path 대신 bundle 내부 파일명으로만 기록한다.
- Windows Event Log source 등록/제거는 기본 install/repair/diagnostics 경로에서 실행하지 않는다. `PureCVisor Desktop Node` source 등록은 `DesktopNode.Host.exe service-action eventlog-register` 관리자 opt-in 경로가 소유하며, `artifacts/eventlog-source-registration-20260504-actual-registry`에서 실제 registry 등록 evidence를 기록했다. Source 제거는 `DesktopNode.Host.exe service-action eventlog-remove` code-level registry-backed action이 소유하며, 최신 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`에서 실제 registry removal과 final absent evidence를 현재 HEAD 기준으로 기록했다.

기본 rotation/retention 기준:

- `events.jsonl`, `install.jsonl`: 5 MiB, 보존 파일 5개
- Service host `*.log`, `*.out`, `*.err`: 10 MiB, 보존 파일 10개

`Invoke-PcvDesktopNodeLogRotation` helper는 정책 기준으로 파일을 `file.1`, `file.2` 순서로 회전한다. 기본 검증에서는 실제 Windows Event Log provider/source를 등록하지 않는다.

## Phase 17 LAN security policy

Phase 17 결정은 `DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required`다.

제품 wrapper 기본 plan은 계속 loopback-only다. 기본 .NET service host와 MSI installed action 경로는 `--allow-lan`, firewall rule ensure, non-loopback prefix를 추가하지 않는다.

LAN mode는 administrator opt-in preview로만 다룬다. Non-loopback static Web Console asset은 API route와 같은 bearer token 정책을 유지하며, 제품 wrapper는 TLS endpoint를 직접 제공하지 않는다. LAN 노출에는 reverse proxy 또는 외부 TLS terminator가 필요하다는 전제를 product manifest, runtime policy, diagnostic bundle에 기록한다.

Windows Firewall rule lifecycle은 installer 자동 적용이 아니다. Local API의 firewall command builder는 유지하지만 실제 rule ensure는 product action 또는 수동 관리자 명령의 explicit opt-in으로만 실행한다.

## Phase 18 Update/Rollback/Config Migration

Phase 18 결정은 `DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration`이다.

구현 기준:

- installed `product-manifest.json`을 product root 버전의 단일 진실로 둔다.
- update는 기본적으로 local payload/source 기반 product wrapper action이다. 2026-05-07 후속부터 file/HTTPS ZIP `-SourceUri` package source gate를 code-level로 지원하며, `-ExpectedSha256` 검증과 extract-before-service-stop preflight를 통과한 payload만 기존 update validation에 넘긴다.
- file/HTTPS JSON `-UpdateCatalogUri`와 `-UpdateChannel`은 catalog schema v1에서 selected channel의 version, package URI, SHA-256을 service stop 전에 검증하고 기존 source gate로 넘긴다. Catalog result는 update result와 update transaction journal의 `update_catalog`에 기록된다.
- `http://` update source는 `PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED`로 service stop 전에 차단한다.
- Network update source gate evidence는 `docs/ga-ready/evidence/network-download-update-source-gate-2026-05-07.md`이며, 이는 full updater/public trusted signing/외부 stable publication 완료 evidence가 아니다.
- Updater catalog/channel resolver evidence는 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`이며, 이는 external publication service/public trusted signing/외부 stable publication 완료 evidence가 아니다.
- update payload validation이 끝나면 service stop 전에 `%ProgramData%\PureCVisor\desktop-node\update-transaction.json` 단일 active transaction journal을 쓴다. Journal은 `update-transaction.begin`, stage transition, success 또는 `failed-rolled-back`, rollback result, structured `PCV_*` error를 기록한다.
- Product root backup 이후 copy/config/start/health failure가 발생하면 previous root restore를 시도하고 journal에 `rollback_result`를 기록한다. Restore 성공은 `failed-rolled-back`, restore 실패는 `failed-rollback-failed`로 남긴다.
- Update transaction journal diagnostics evidence는 `docs/ga-ready/evidence/update-transaction-journal-diagnostics-2026-05-07.md`이며, product root filesystem rollback evidence는 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`다. Post-crash resume/reconcile, service/data/config/job-store transaction manager, public trusted signing, 외부 stable publication 완료 evidence가 아니다.
- Installer publication descriptor evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`다. Installer build output은 MSI/provenance/hash sidecar와 함께 `.publication.json`을 작성하지만, Burn bootstrapper, MSIX, winget manifest, external publication service, public trusted signing, 외부 stable publication 완료 evidence가 아니다.
- ADR-0005 public distribution phase 1 evidence는 `docs/ga-ready/evidence/public-distribution-operations-expansion-phase1-2026-05-07.md`다. `PUBLIC_DISTRIBUTION_GATE_MATRIX`와 `New-PcvPublicDistributionDescriptor.ps1`는 `public-distribution-operations-expansion-candidate` dry-run descriptor를 기록하며 actual execution, host mutation, public trusted signing, external stable publication은 모두 `not-run`/`not-claimed`다.
- Public distribution ops execution bundle evidence는 `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`다. `New-PcvPublicDistributionOperationsBundle.ps1`는 descriptor/readiness/Burn/MSIX/winget/catalog/public-signed-rollback/Credential Manager/Event Log/TLS/service-token/timeout/diagnostic preflight를 하나의 artifact root에 수집한다. 이 bundle은 local descriptor execution evidence일 뿐 Burn build, winget submission, catalog upload, clean-host public signed rollback smoke, Credential Manager/Event Log/TLS/token mutation을 실행하지 않는다.
- Diagnostic bundle list pagination/retention evidence는 `docs/ga-ready/evidence/diagnostic-bundle-list-pagination-retention-2026-05-09.md`다. `GET /api/v1/diagnostics/bundles?limit=&offset=`는 retention 적용 후 최신순 bundle page와 `next_offset`을 반환하고, Web Console Troubleshooting panel은 retained bundle list와 `Load more bundles` UX를 표시한다. 이 slice는 read-only API/Web hardening이며 host mutation, public trusted signing, external stable publication은 `not-claimed`다.
- Service token rotation/revoke installed evidence는 `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`, `artifacts/service-token-rotation-revoke-installed-20260509-150334`다. `DesktopNode.Host.exe service-action service-token-rotation-revoke`는 installed service의 DPAPI protected token backup/write/atomic replace, service restart, old bearer `403`, new bearer `200`, redacted audit write를 PASS로 확인했다. `service_token_mutation=performed`, `token_value_observed=false`, `host_mutation_performed=true`이며 public trusted signing/external stable publication은 `not-claimed`다.
- Public distribution readiness preflight evidence는 `docs/ga-ready/evidence/public-distribution-readiness-preflight-2026-05-07.md`다. `New-PcvPublicDistributionReadiness.ps1`는 `.publication.json` descriptor에서 winget manifest preview와 `winget validate` manual follow-up을 산출하지만, submission은 `not-submitted`이고 `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
- Updater catalog publication preflight evidence는 `docs/ga-ready/evidence/updater-catalog-publication-preflight-2026-05-07.md`다. `New-PcvUpdaterCatalogPublicationPreflight.ps1`는 selected HTTPS catalog channel의 catalog publication preview와 SHA-256 sidecar를 산출하지만, `catalog_publication: not-published`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
- Windows Credential Manager transition preflight evidence는 `docs/ga-ready/evidence/windows-credential-manager-transition-preflight-2026-05-08.md`다. `New-PcvWindowsCredentialManagerTransitionPreflight.ps1`는 service name, credential target, 현재 DPAPI protected token file storage, 목표 Windows Credential Manager storage, transition plan preview를 산출하지만, token value read, credential write/delete, service reload, host mutation은 실행하지 않는다. 후속 `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`는 current-user Credential Manager write/read/delete capability PASS와 당시 installed service `LocalSystem` context blocker를 기록했고, 최신 `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`는 installed MSI deferred LocalSystem custom action으로 migration/reload/old-source rejection/rollback diagnostics PASS를 기록한다. Matrix 상태는 `credential_manager_transition: installed-local-system-default-transition-pass`, `credential_manager_mutation: local-system-write-read-delete-and-protected-file-migration`, `service_credential_manager_default_transition: installed-admin-smoke-pass`, `token_value_observed: false`, public trusted signing/external stable publication `not-claimed`다.
- Windows Event Log provider transition preflight evidence는 `docs/ga-ready/evidence/windows-event-log-provider-transition-preflight-2026-05-08.md`다. `New-PcvWindowsEventLogProviderTransitionPreflight.ps1`는 service name, provider name, log name, 현재 JSONL-first/Event Log opt-in writer policy, 목표 default Windows Event Log provider writer, provider transition plan preview를 산출한다. 후속 `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`는 installed native provider registration과 event id `39100` write/query를 PASS로 기록했고, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`는 installed MSI deferred LocalSystem default writer/repair/remove/volume/schema PASS를 기록한다. Matrix 상태는 `event_log_provider_transition: installed-provider-register-write-pass`, `event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass`, `event_log_default_writer: installed-admin-smoke-pass`, public trusted signing/external stable publication `not-claimed`다.
- Built-in TLS certificate lifecycle preflight evidence는 `docs/ga-ready/evidence/builtin-tls-certificate-lifecycle-preflight-2026-05-08.md`다. `New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`는 service name, certificate subject, HTTPS bind prefix, 현재 TLS mode, 목표 built-in service certificate mode, TLS lifecycle plan preview를 산출하지만, private key material 생성, certificate import/export, trust-store mutation, HTTPS/LAN binding, host mutation은 실행하지 않는다. 후속 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`와 `New-PcvPublicOpsGateExecutionReadiness.ps1`는 public certificate generation/rotation/delete code-level readiness를 `partial-code-level-cert-generate-rotate-delete-pass`로 기록했고, `tls_private_key_material_written=false`, `tls_binding=not-run`, `host_mutation_performed=false`, public trusted signing/external stable publication `not-claimed`를 유지한다. 현재 ADR-0006 internal HTTPS/TLS lifecycle installed PASS는 `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`가 소유하며, 이것이 HTTPS/443 기본 Web Console listener 또는 public publication을 의미하지는 않는다.
- Service token rotation/revoke preflight evidence는 `docs/ga-ready/evidence/service-token-rotation-revoke-preflight-2026-05-08.md`다. `New-PcvServiceTokenRotationRevokePreflight.ps1`는 service name, protected token path, 현재 DPAPI protected token file storage, rotation mode, Service token rotation revoke plan preview를 산출하지만 host mutation은 실행하지 않는 preflight다. 후속 `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`는 installed-admin smoke로 실제 token backup/write/atomic replace, service restart, old-token rejection, audit write를 닫았다.
- Diagnostic bundle server-side preflight evidence는 `docs/ga-ready/evidence/diagnostic-bundle-server-preflight-2026-05-08.md`다. `New-PcvDiagnosticBundleServerPreflight.ps1`는 service name, diagnostics root, Local API generation route, download route template, bearer authorization policy, redaction policy, retention policy, Diagnostic bundle server-side plan preview를 산출하지만, Local API action execution, archive creation, download serving, redaction execution, retention application, product diagnostics runner delegation, host mutation은 실행하지 않는다. `diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight`, `diagnostic_bundle_api_action: not-run`, `diagnostic_bundle_archive_created: false`, `diagnostic_bundle_download_served: false`, `diagnostic_bundle_redaction_status: not-run`, `diagnostic_bundle_authz_status: not-run`, `diagnostic_bundle_retention_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
- Diagnostic bundle server code-level evidence는 `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`다. `DesktopNodeApiRequestProcessor`는 `POST /api/v1/diagnostics/bundles`에서 redacted `.bundle.json` artifact를 만들고 `GET /api/v1/diagnostics/bundles/{bundle_id}/download`에서 다운로드로 제공한다. `PcvDesktopNodeProduct.psm1` service plan은 `--diagnostics-root "C:\ProgramData\PureCVisor\desktop-node\diagnostics"`를 포함한다. Installed listener execution, product wrapper `CollectDiagnostics` delegation, service mutation, host mutation은 실행하지 않는다. `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`, public trusted signing/external stable publication은 `not-claimed`다.
- Diagnostic bundle Host listener code-level evidence는 `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`다. `DesktopNodeHostApplication`은 `X-PCV-Request-Id`/`X-Request-Id`를 Local API processor로 전달하고, focused Host test는 bearer-required create/download, redacted `.bundle.json`, `X-PCV-Diagnostic-Bundle-Id` download header를 in-process listener로 확인한다. 이 code-level evidence 자체는 installed service listener, service mutation, host mutation을 실행하지 않는다. `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`, public trusted signing/external stable publication은 `not-claimed`다.
- Diagnostic bundle product wrapper code-level evidence는 `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`다. `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`는 `New-PcvDesktopNodeDiagnosticBundle`로 위임하고 bundle 안에 `product-wrapper-delegation-redacted.json`을 기록한다. `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `actual_execution: code-level-product-wrapper`, `host_mutation_performed: false`이며 installed service listener PASS는 별도 `0.39.0-admin-smoke` elevated rerun evidence가 소유한다.
- Timeout/rate-limit hardening preflight evidence는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md`다. `New-PcvTimeoutRateLimitHardeningPreflight.ps1`는 service name, Local API route prefix, route timeout target, request limit target, retry-after target, UI/API error contract, Timeout and rate-limit hardening plan preview를 산출하지만, server config mutation, middleware enablement, retry semantics change, UI/API error behavior verification, load test execution, host mutation은 실행하지 않는다. `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
- Timeout/rate-limit hardening code-level evidence는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-code-level-2026-05-08.md`다. `DesktopNodeApiRequestProcessor`와 `DesktopNodeHostApplication`는 `/api/v1/` per-client request window를 적용하고 초과 시 HTTP 429, `Retry-After`, `application/problem+json`, `PCV_RATE_LIMIT_EXCEEDED`를 반환한다. Route timeout enforcement, load test, server config mutation, installed service mutation, host mutation은 실행하지 않는다. `timeout_rate_limit_hardening: partial-code-level-request-limit`, `route_timeout_policy: not-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, public trusted signing/external stable publication은 `not-claimed`다.
- Timeout/rate-limit route-timeout code-level evidence는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md`다. `DesktopNodeApiRequestProcessor`는 `/api/v1/` GET/read route response deadline을 적용하고 초과 시 HTTP 504, `Retry-After`, `application/problem+json`, `PCV_ROUTE_TIMEOUT`, `Gateway Timeout`, `route_timeout_seconds`, `request_id`를 반환한다. Mutation-route cancellation, native adapter cooperative cancellation, load test, server config mutation, installed service mutation, host mutation은 실행하지 않는다. `timeout_rate_limit_hardening: partial-code-level-route-and-request-limit`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, public trusted signing/external stable publication은 `not-claimed`다.
- Timeout/rate-limit server-config code-level evidence는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md`다. `PcvDesktopNodeProduct.psm1` service plan과 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config는 `DesktopNode.Host.exe listen` binary path에 `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`를 포함한다. Installed service mutation, service stop/start, load test, host mutation은 실행하지 않는다. `timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, public trusted signing/external stable publication은 `not-claimed`다.
- Diagnostic bundle MSI/service installed listener evidence는 `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`다. `0.39.0-admin-smoke` installed service `PathName`은 `--diagnostics-root`, protected token file, route timeout, request-limit, burst, retry-after 인자를 포함하고, protected-token listener round trip은 diagnostic bundle POST `201`, download `200`, matching `X-PCV-Diagnostic-Bundle-Id`, redaction PASS를 기록한다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니며 public trusted signing/external stable publication은 `not-claimed`다.
- Installed listener 후속 OS mutation gate evidence는 `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`다. `0.39.0-admin-smoke` focused gate는 `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`에서 firewall/trust-store/LAN/Event Log mutation을 PASS로 확인했고 LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, final firewall count `0`, Event Log absent, internal trust cert present, boot time unchanged를 기록한다. Public trusted signing은 `excluded`, external stable publication은 `not-claimed`다.
- Timeout/rate-limit load-test code-level evidence는 `docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md`다. `ApiHardeningRequestProcessorTests`는 `DesktopNodeApiRequestProcessor` in-process 경로에서 같은 client의 `/api/v1/runtime/policy` 요청 64개를 병렬 실행해 HTTP 200 20건, HTTP 429 44건, unexpected status 0건과 `PCV_RATE_LIMIT_EXCEEDED` problem-details contract를 확인한다. Installed listener load, external load generator, installed service config mutation, host mutation은 실행하지 않는다. `timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: code-level-inprocess-pass`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, public trusted signing/external stable publication은 `not-claimed`다.
- Burn bootstrapper preflight evidence는 `docs/ga-ready/evidence/burn-bootstrapper-preflight-2026-05-07.md`다. `New-PcvBurnBootstrapperPreflight.ps1`는 packaging publication descriptor와 HTTPS MSI URL에서 WiX Burn authoring preview를 산출한다. 후속 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`는 actual bundle build/install/repair/remove와 direct MSI restore를 PASS로 기록해 `burn_bootstrapper: build-install-repair-remove-pass-internal-smoke` 상태를 소유한다. Public trusted signing/external stable publication은 `not-claimed`다.
- MSIX packaging feasibility preflight evidence는 `docs/ga-ready/evidence/msix-packaging-feasibility-preflight-2026-05-07.md`다. `New-PcvMsixPackagingFeasibilityPreflight.ps1`는 MSIX package manifest preview를 산출하지만 service packaging design evidence가 없으므로 `msix: feasibility-blocked-by-service-packaging-design`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
- MSIX package lifecycle smoke evidence는 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`다. `artifacts/msix-package-lifecycle-smoke-20260510-0416`에서 internally signed MSIX build/sign/verify, install/update/remove, final package/service absence를 PASS로 확인했다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`다.
- MSI/update package apply evidence는 `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`다. `artifacts/msi-update-package-20260509-0391`에서 AllowUnsignedDev `0.39.1-admin-smoke` MSI/update ZIP/catalog를 build/validate했고 elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 확인했다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`다.
- 기본 rollback slot은 `C:\Program Files\PureCVisor\DesktopNode.previous` 하나다.
- config migration dry-run/validation이 실패하면 service start를 막고 previous root rollback을 시도한다.
- service start 또는 protected-token health check 실패도 previous root rollback을 시도한다.
- diagnostic bundle은 update policy, migration plan, rollback state artifact와 existing journal이 있을 때 `update-transaction-journal-redacted.json`을 포함한다.
- job store는 기본적으로 파괴적 rewrite를 하지 않고 schema mismatch를 read-only 또는 blocked diagnostics로 남긴다.
- 실제 mutating update/rollback smoke는 관리자 권한 opt-in으로 분리한다. 2026-04-28에는 기존 `0.14.0-dev` 설치를 `0.18.0-admin-smoke`로 update한 뒤 rollback해 service health와 diagnostics artifact를 확인했다. 2026-05-01에는 `0.23.9-admin-smoke-baseline` product-wrapper install, `0.23.9-admin-smoke-update` update/config-migration dry-run, rollback, CollectDiagnostics, cleanup을 같은 elevated evidence run에서 다시 확인했다. 상세 증거는 Phase 18 plan의 `완료 증거`와 `artifacts/p0-local-requiresigned-rc-msi-20260501-165251/signed-msi-update-smoke-evidence.json`를 따른다.

## Phase 19 Promotion Redecision

Phase 19 결정은 `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`다.

- DPAPI protected token, JSONL diagnostics/redaction, LAN preview policy, manifest-first update/rollback/config migration은 제품화 증거로 인정한다.
- signed RC MSI build와 elevated MSI install/repair/uninstall/`REMOVE_DATA=1` 전체 exit 0 smoke, Hyper-V product-flow lifecycle, release approval/signing preflight, firewall cleanup, 운영/Event Log source lifecycle evidence는 2026-04-30 draft-ready 기준으로 기록됐다. 2026-05-01 관리자 opt-in hardening smoke에서 WinSW service reinstall/start, SCM failure action apply, protected token ACL inspection, firewall create/update/delete, Event Log scoped source lifecycle, LAN listener/firewall preview, direct Hyper-V lifecycle, Product API Hyper-V lifecycle, self-signed TLS reverse proxy preview, 75초 운영 sampling도 통과했다. 같은 날 current-head `3d35aa2` 기준 local test `RequireSigned` RC MSI lifecycle과 product-wrapper update/rollback/config migration smoke도 통과했다. 이후 internal Root/leaf signer 기준 `0.23.10-rc.1` `RequireSigned` RC MSI lifecycle도 통과했다. Public trusted signing evidence와 외부 stable publication은 내부 전용 서비스 scope 밖이다. 제품 런타임 승격 판단은 ADR-0004가 소유한다.
- 이 wrapper는 내부 전용 서비스 배포 계층이며 Single Edge Linux `purecvisorsd` 공개 런타임과 연결하지 않는다.

## Phase 22 Release/Version Policy

Phase 22는 release channel과 artifact naming contract를 고정한다.

- `dev`, `admin-smoke`, `rc`, `stable` channel 의미를 분리한다.
- MSI/provenance/hash sidecar output은 `PureCVisorDesktopNode-<version>-windows-x64.*` naming을 사용한다.
- build plan과 provenance는 `product.release_channel`을 기록한다.
- `dev`와 `admin-smoke`는 unsigned 개발 build를 허용할 수 있지만, `rc`와 `stable` version은 `RequireSigned` signing mode만 허용한다.
- `RequireSigned` provenance는 `signing_trust_model`을 기록한다. 내부 서비스 운영은 ADR-0003의 `InternalEnterprise`를 사용한다. 외부 배포용 `PublicTrusted`는 현재 scope 밖이며 별도 ADR 없이는 사용하지 않는다.
- internal stable 발행은 selected trust model approval, signed stable MSI lifecycle evidence, update compatibility evidence가 닫힌 뒤에만 다룬다. 외부 public stable publication은 내부 전용 서비스 scope 밖이다.
- ADR-0002는 Phase 22 정책과 installer artifact/channel contract를 현재 적용 결정으로 채택한다.

## Post-reboot verification dry-run/runner evidence

Windows reboot 이후에 이어서 실행할 검증 command plan은 post-reboot verification 도구의 dry-run으로 먼저 확인한다.

```powershell
$evidence = Join-Path $env:TEMP ('pcv-post-reboot-' + [guid]::NewGuid().ToString('N'))
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir $evidence -Profile ProductStatus -DryRun
```

현재 구현 slice는 `post-reboot-state.json`과 scheduled task action plan을 생성하고 redaction/profile/principal 제약을 검증한다. Runner entrypoint는 state file을 읽어 command를 실행하고 `post-reboot-result.json`, `post-reboot-summary.md`, command별 stdout/stderr artifact, `post-reboot-complete.json`을 남긴다. Command 실패와 task cleanup 실패는 result JSON에 기록하며 evidence 파일 작성은 유지한다. `post-reboot-complete.json`이 이미 있으면 command를 재실행하지 않고 cleanup만 다시 시도한다.

`-DryRun` 없는 실행은 명시적으로 post-reboot scheduled task를 등록하는 opt-in 경로다. 이때 `-ContinuationProfiles PackagingRegression` 같은 후속 profile을 state에 포함하면 reboot 이후 `ProductStatus`가 성공한 뒤 packaging regression 검증을 자동으로 이어 실행한다. Active product post-reboot profile은 `ProductStatus`, `PackagingRegression`만 허용한다. 예전 Hyper-V component continuation profile은 product post-reboot profile에서 퇴역했으며 요청 시 `PCV_POST_REBOOT_PROFILE_RETIRED`로 실패한다. 단, `-Reboot`는 항상 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 거부한다. 실제 Windows 재부팅은 사용자가 별도로 수행해야 하며, Codex나 script가 `Restart-Computer`를 호출하지 않는다.

## 관리자 smoke

아래 명령은 실제 Windows service, 제품 루트, 데이터 루트를 변경할 수 있으므로 관리자 PowerShell에서 명시적으로만 실행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
Invoke-WebRequest http://127.0.0.1/
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall
```

`Action Status`는 product wrapper health check에서 protected token file을 읽어 bearer-protected `/api/v1/runtime/policy`를 확인한다. 직접 bearer API route까지 검증해야 하면 `Invoke-PcvRouteParityMutationSmoke.ps1`를 사용한다.

제품 service install/start/stop/delete smoke는 product wrapper 또는 MSI의 `DesktopNode.Host.exe service-action` 경로를 사용한다. Legacy service helper의 `Install` action은 component/service helper 검증용이며, 제품 설치된 `PureCVisorDesktopNode` service를 재설치하는 경로로 사용하지 않는다.

Phase 18 update/rollback smoke는 현재 기본 .NET service host plan을 사용한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -Version '0.18.0-admin-smoke'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceUri 'file:///D:/staging/PureCVisorDesktopNode-0.39.0.zip' -ExpectedSha256 '<64-hex-sha256>' -DownloadRoot 'C:\ProgramData\PureCVisor\desktop-node\updates' -Version '0.39.0'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri 'file:///D:/staging/purecvisor-desktop-node-catalog.json' -UpdateChannel 'internal-dev' -DownloadRoot 'C:\ProgramData\PureCVisor\desktop-node\updates'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
```

Rollback 후 `.failed` product root는 diagnostic 보존 정책 확인을 위해 남을 수 있다. 활성 product root와 service health를 확인한 뒤 별도 정리 승인이 있을 때만 삭제한다.

## 데이터 보존

기본 uninstall은 `%ProgramData%\PureCVisor\desktop-node` 데이터를 보존한다. protected token file, legacy raw token file, account file, JWT signing key file, job store, event log, install log, diagnostics를 제거하려면 `-RemoveData` 또는 MSI `REMOVE_DATA=1`을 명시한다. MSI 경로에서 `remove-installed --remove-data`는 service 삭제 후 data-root removal handoff만 남기고, 실제 삭제는 service absent precondition을 확인하는 `data-root-remove --remove-data` action이 수행한다. Token/account signing key file은 읽기 전용 ACL로 hardening되어 있으므로, data-root 제거 action은 삭제 직전에 관리자 삭제 권한을 복구한 뒤 파일을 제거한다. Data-root ACL은 MSI WiX source가 아니라 product wrapper `data_acl` policy와 service token helper/RemoveData repair test가 소유하며, 실제 host ACL inspection은 관리자 opt-in evidence로만 수행한다. Service host log 디렉터리는 별도 운영 로그로 보존한다.

Rollback은 `%ProgramData%`의 protected token, legacy token, job store, event log를 삭제하지 않는다. 이전 제품 루트는 `C:\Program Files\PureCVisor\DesktopNode.previous` 경로를 기준으로 복원하고, 실패한 현재 제품 루트는 `.failed` 경로로 격리한다.

## Diagnostic bundle

`CollectDiagnostics`는 protected token file, legacy token file, account file, JWT signing key file 내용을 복사하지 않는다. Bundle은 service status, service-host status/log/metadata, runtime policy 응답, product manifest, event log, install log, redaction된 job store, update policy, migration plan, rollback state, diagnostics self-audit, operational evidence summary, `diagnostics-manifest.json`을 파일로 남긴다.

Redaction 대상:

- token 값
- protected token blob과 token hash
- `Authorization` header
- `api_token`, `api_token_protected_file`, `protected_token`, `token_sha256`, `access_token`, `refresh_token`, `jwt_signing_key`, `password`, `password_hash`, `secret` 계열 key
- source/product/data root의 전체 경로

`diagnostics-self-audit.json`은 runtime policy 수집 결과가 사용 가능하고 Phase 24 `job_runtime` public contract를 포함하는지 요약한다. `operational-evidence-redacted.json`은 SCM failure action recovery policy, service log retention policy, 관찰된 service log artifact 이름, Event Log deferred policy, host mutation 미수행 여부를 요약한다. `diagnostics-manifest.json`은 schema version, redaction version, diagnostics policy, self-audit 요약, source artifact 목록을 기록한다. Manifest와 bundle artifact에는 raw token, protected token blob/hash, host absolute source/product/data root가 남지 않아야 한다.

## 제외 범위

Phase 19/22 기준 wrapper는 WiX MSI-first installer source/build, signing/provenance contract, Phase 22 artifact/channel contract, installer publication descriptor sidecar, DPAPI LocalMachine protected token file, JSONL first diagnostics policy, LAN security policy, manifest-first update/rollback/config migration 기본 구현, network update source gate code-level partial, updater catalog/channel resolver code-level partial, product root filesystem rollback code-level partial을 다룬다. Diagnostic bundle server-side generation/download는 code-level API action, `--diagnostics-root` service plan wiring, installed listener execution, list pagination/retention UX까지 후속 evidence로 진행됐다. Service token rotation/revoke, installed listener external load/rate-limit smoke, Burn bootstrapper internal lifecycle smoke, Windows Credential Manager service default transition, Windows Event Log provider register/write/query smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke, Lifecycle/Packaging current 0.41.5 to 0.41.6 rebaseline도 internal ops evidence로 닫혔다. Winget submission, external publication service/catalog upload, public timestamp evidence, clean-host public signed update/rollback, post-crash transaction resume/reconcile은 scope 밖 또는 별도 후속 Phase로 남긴다. 이 배포 계층은 ADR-0004 이후 내부 전용 GA-ready 제품 런타임의 packaging owner이며, Single Edge Linux `purecvisorsd` 공개 런타임과 연결하지 않는다.

## 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```
