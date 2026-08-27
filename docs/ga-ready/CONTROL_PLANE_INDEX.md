# Desktop Node 제어 평면 인덱스

<!-- BEGIN GENERATED CURRENT EVIDENCE -->
## Current operational evidence (generated)

- Version: `0.42.75-admin-smoke`
- Active operator surfaces: Web Console and PCVCLI; `tui_present=false`.
- Package evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md`.
- Full admin host mutation: `full-admin-host-mutation-gate-20260821-04275` / `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md`.
- Actual-VM functional evidence: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`.
- Feature qualification: `contract=pcv-feature-promotion-decision-v1`; `promotion_eligible=true`; `blocker_count=0`; `blockers=none`.
- Installed CLI/Web current-card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md`; CLI exit 0, Web HTTP 200, service Running/Automatic, TUI absent.
- Clean MSI SHA-256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`.
- Operational MSI SHA-256: `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b`.
- Operational payload aggregate SHA-256: `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7`.
- Provenance commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`.
- Latest closed manual-admin pair: `0.42.74-admin-smoke -> 0.42.75-admin-smoke` / `manual-admin-campaign-descriptor-20260827-04274-04275`.
- Claims: `public_trusted_signing=false`; `external_stable_publication=false`.
<!-- END GENERATED CURRENT EVIDENCE -->

## 2026-08-25 public Required CI authority

- Public repository final `main` authority is SHA
  `6e2bdb93ce308b632c929e2c17f5550ac3845401`. Development Gates run
  [`32904006595`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595)
  passed exact provider-required contexts `dotnet`, `web`, `delivery`, `installer-policy`.
- Branch protection uses `strict=true`, admin enforcement, and only those four GitHub Actions
  contexts. Required CI executable Pester, non-admin PowerShell, and host mutation invocation are
  `0`; details are in
  `docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md`.
- Public Boundary run
  [`32904006619`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006619),
  job `97983888524` (`public-boundary-ci-required`) is a Pester/PowerShell non-required residue with
  `provider_required=false`. It is not part of the exact four protected contexts.
- This documentation/CI closure did not open a package candidate. 당시 operational authority는
  `0.42.74-admin-smoke`였고, 이후 2026-08-27 Lane 3가 `0.42.75-admin-smoke`로 승격했다.

## 2026-08-27 `0.42.75` SERVICE_PLAN P0 current promotion

- 최신 product payload package는 `0.42.75-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md`가 기록한다. Clean MSI
  SHA-256은 `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`다.
- full admin host mutation current는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md` /
  `full-admin-host-mutation-gate-20260821-04275`다.
- installed current-card는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md`다.
- actual-VM functional은
  `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
  PASS다. SERVICE_PLAN P0 actual-VM은
  `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-27-04275.md` PASS다.
- `0.42.74-admin-smoke -> 0.42.75-admin-smoke` package-pair는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md` /
  `manual-admin-campaign-descriptor-20260827-04274-04275`로 PASS다.
- operational current는 `0.42.75-admin-smoke`다. `promotion_eligible=true`, blockers는
  없다. 04274 P0 `vm.save` FAIL는 historical predecessor다. token R4는 04272
  carry-forward다. public trusted signing과 external stable publication은 주장하지 않는다.

## 2026-08-21 `0.42.74` SERVICE_PLAN P0 predecessor promotion

- 최신 product payload package는 `0.42.74-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md`가 기록한다. Clean MSI
  SHA-256은 `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`다.
- full admin host mutation current는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md` /
  `full-admin-host-mutation-gate-20260820-04274`다. Operational MSI SHA-256은
  `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`, payload aggregate
  SHA-256은 `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`, provenance
  commit은 `adc04673b569ef9b587371fdb23bc11ceb14e2e2`다.
- installed current-card는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md`다.
  CLI `3/3` exit `0`, Web `2/2` HTTP `200`, service `Running/Automatic`,
  `tui_present=false`다. promotion 판정은 `promoted-current`다.
- actual-VM functional은
  `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md`
  PASS다. SERVICE_PLAN P0 actual-VM은
  `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md`에서 attach/
  restore/manage PASS, `vm.save` FAIL이다. 이 FAIL는 열린 결함이다.
- `0.42.73-admin-smoke -> 0.42.74-admin-smoke` package-pair는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md` /
  `manual-admin-campaign-descriptor-20260820-04273-04274-closed`로 PASS다.
- 당시 operational current는 `0.42.74-admin-smoke`였다. P0 `vm.save` FAIL는 그 승격이 고치지
  않았다. 당시 P0 landing public-boundary predecessor는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-21-04274-p0-landing-pass.md`
  다. 현재 provider-required authority는 위 2026-08-25 exact-four Development Gates가 소유한다.
  token R4는 04272 carry-forward다. public trusted signing과 external stable
  publication은 주장하지 않는다.

## 2026-07-14 0.42.62 -> 0.42.63 manual-admin follow-up BLOCKED

- `docs/ga-ready/evidence/manual-admin-campaign-2026-07-14-04262-04263.md`는 PlanOnly readiness
  exit `0` 뒤 campaign을 `blocked-by-installed-baseline-version-mismatch`로 판정했다. 설치본은
  이미 `0.42.63-admin-smoke`였고 host mutation은 수행되지 않았다.
- 다음 action은
  `run-on-dedicated-0.42.62-baseline-host-with-approved-burn-msix-runners`다. Closed descriptor나
  Burn/MSIX runner 결과를 만들지 않았으며, 최신 closed package-pair는 계속
  `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
  `manual-admin-campaign-descriptor-20260529-04258-04259-closed`다.
- 이 blocked follow-up은 historical package-pair 사실로 유지하며 당시 0.42.64
  package/fullgate/CLI-Web installed anchor를 변경하지 않았다. 현재 operational authority는
  위 생성 블록의 `0.42.75-admin-smoke`다.

## 2026-07-14 0.42.63 CLI/Web-only package build Evidence

- 최신 product payload package build는 `0.42.63-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-07-14-04263.md`가 기록한다. MSI SHA-256은
  `d2f2fff7fb400647135d96449f36704af2d080e1a6a97a551354290cdf1a6f04`, payload aggregate
  SHA-256은 `19f80f3e0b849d180a3e62461742a8a2ab7371e632dbfecfc8fad28bf59721f4`다.
- Product manifest schema는 `2`이고 Host/CLI는 존재하며 `pcvtui.exe`, root `tui`,
  `paths.tui_exe`와 MSI active TUI file row는 없다.
- Package build는 0.42.63 full admin host mutation과 CLI/Web installed current-card까지
  actual-host PASS로 승격됐다. 0.42.62는 historical TUI predecessor다.
- 이 evidence는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke 전용이며 public trusted
  signing 또는 external stable publication evidence가 아니다.

## 2026-07-14 CLI/Web-only operator surface code-level decision

- ADR-0011 `docs/adr/0011-cli-web-only-operator-surface.md`가 active surface를 Web Console과
  PCVCLI로 고정하고 TUI를 active product에서 제거한다.
- Code-level PASS는
  `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`가 소유하며
  Local API/backend는 유지된다.
- `0.42.65-admin-smoke` package/fullgate/actual-VM functional correctness/CLI-Web installed
  current-card가 current다. `0.42.64-admin-smoke`는 immediate CLI/Web predecessor,
  `0.42.62-admin-smoke` Web/TUI/CLI installed current-card는 historical TUI predecessor다.

## 2026-07-13 0.42.62 WMI topology recovery package/fullgate/installed Evidence

- 최신 product payload package는 `0.42.62-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md`가 기록한다. Clean MSI
  SHA-256은 `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`, clean payload
  aggregate SHA-256은 `0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e`다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04262-hostmutation.md` /
  `full-admin-host-mutation-gate-20260713-04262`다. Operational MSI SHA-256은
  `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`, payload aggregate
  SHA-256은 `ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`, provenance commit은
  `7f71f0a518c5b592f233373522d36b5401c3f1df`다.
- 2026-07-13 installed Operator Surface predecessor current-card는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04262.md` /
  `artifacts/installed-operator-surface-current-card-20260713-04262/summary.json`가 기록하며
  당시 Web/TUI/CLI, internal switch topology, service state를 PASS했다.
- `docs/ga-ready/evidence/wmi-internal-switch-topology-recovery-2026-07-13-04260-04262.md`는
  04260/04261의 package/MSI lifecycle PASS 뒤 route failure와 OS mutation 미실행 경계를
  보존한다. 두 버전은 PASS anchor가 아니다.
- 최신 closed manual-admin package-pair는 별도 campaign이 없었으므로 계속
  `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
  `manual-admin-campaign-descriptor-20260529-04258-04259-closed`다.
- 04262는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke evidence이며 public trusted
  signing 또는 external stable publication evidence가 아니다.

## 2026-05-29 0.42.59 package fullgate manual-admin installed Evidence

- 최신 product payload package smoke는 `0.42.59-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`가 기록한다.
  Clean MSI SHA-256은 `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`,
  payload aggregate SHA-256은 `666a1351d58963c7908aad4f66d6469de42747a7c7f70d1e30fb0e94771a5808`,
  provenance commit은 `63d57feba605f82dabd44a96ed50a4d622f6310a`다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md` /
  `full-admin-host-mutation-gate-20260529-04259`다. Full-gate MSI SHA-256은
  `dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`다.
- 최신 closed manual-admin package-pair는 `0.42.58-admin-smoke -> 0.42.59-admin-smoke`이며
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md` /
  `manual-admin-campaign-descriptor-20260529-04258-04259-closed`가 기록한다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.59-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md` /
  `artifacts/installed-operator-surface-current-card-20260529-04259/summary.json`가
  Web/TUI/CLI current-card와 fullgate/manual-admin projection을 PASS로 기록한다.
- account/noVNC Operator Surface smoke는
  `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`에서
  account login/browser QA와 target-backed noVNC streaming rerun을 PASS로 기록한다.
- 최신 public-boundary main push CI PASS는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md` /
  run `26636072420`, job `78496568595`, head
  `5a2f91762a6c2a8ab6b84d334fa6cb420474671f`가 기록한다. `0.42.60-admin-smoke`
  installed current-card payload 후보는 직전 product payload evidence가 이미 열었고,
  docs-maintenance postpush는 추가 package 후보를 열지 않는다. account/noVNC는
  0.42.58 PASS를 carry-forward하고, actual VM Guest Execution/QoS smoke는 provider/control payload
  변경 때 재실행한다.
- 0.42.56 predecessor는 `0.42.56-admin-smoke`,
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04256.md`,
  `full-admin-host-mutation-gate-20260528-04256`,
  `manual-admin-campaign-descriptor-20260528-04255-04256-closed`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md`로
  public-boundary follow-up의 기준 anchor를 보존한다.

## 2026-05-28 0.42.55 Web/TUI running cancel affordance installed Evidence

- 최신 product payload package smoke는 `0.42.55-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04255.md`가 기록한다.
  Clean MSI SHA-256은 `530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627`,
  payload aggregate SHA-256은 `ada13e719c47a439c8836fc2138f6419d447fc1eccfcd02fe73d3686a2127ef6`,
  provenance commit은 `958052181012f7d1be6ccff535316bfaeeef07df`다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04255-hostmutation.md` /
  `full-admin-host-mutation-gate-20260528-04255`다. Full-gate MSI SHA-256은
  `cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb`, payload aggregate
  SHA-256은 `69019129347920bba88c269a4828dae5b214eace8a6d31bd60bc7fa7f1b81934`,
  provenance commit은 `958052181012f7d1be6ccff535316bfaeeef07df`다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.55-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md` /
  `artifacts/installed-operator-surface-current-card-20260528-04255/summary.json`가
  Web/TUI/CLI current-card, runtime policy running interrupt, API/CLI dry-run preview,
  Web running cancel affordance 설치본 표시, 실제 Windows guest credentialed execution을 PASS로 기록한다.
- 실제 Windows guest credentialed execution smoke는
  `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md`가
  소유한다. `pcv-guest-installed-04253-r1` persistent Windows VHD target, DPAPI LocalMachine
  credential reference, channel verify와 guest-exec가 `succeeded`로 닫혔다.
- 04250→04254 baseline host prep과 closure attempt는
  `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-04255-fullgate.md`에
  plan-only blocked evidence로 기록한다. baseline/target artifact는 모두 있으나 현재 host가 이미
  0.42.55라 dedicated 0.42.50 baseline host가 필요하다.
- 최신 public-boundary main push CI PASS는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md` /
  run `26578120570`, job `78303066840`, head
  `7a7d5de822bdb058b04149eeeef0a7eb462828b5`가 기록한다.

## 2026-05-28 0.42.54 Guest Execution running cancel fullgate predecessor Evidence

- 최신 product payload package smoke는 `0.42.54-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04254.md`가 기록한다.
  Clean MSI SHA-256은 `a0181bd156e4e01a57c177639a3eb418009f6fd9dd8bf090a3bb123e69aad36b`,
  payload aggregate SHA-256은 `8443b217a45551bfcaf28d366ff33af80f95fc4527509addf4919621472f6bb3`,
  provenance commit은 `5a1058f55fcd42d28c7075514e1924c5ccdfb525`다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04254-hostmutation.md` /
  `full-admin-host-mutation-gate-20260528-04254`다. Full-gate MSI SHA-256은
  `937ac686aa782a69dc41d06d8694a020cf4a78b45cf7a6674e85593cce3c4cb1`, payload aggregate
  SHA-256은 `bdcb61002f5e3e739ca3db5cb0a189548b9c9b25ef5747c437c7b23d615fef84`,
  provenance commit은 `2c11e359709c775be7a57ea9624716720c5b62d6`다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.54-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04254.md` /
  `artifacts/installed-operator-surface-current-card-20260528-04254/summary.json`가
  Web/TUI/CLI current-card, runtime policy running interrupt, API/CLI dry-run preview, 실제
  Windows guest running cancel, secret echo guard를 PASS로 기록한다.
- Guest Execution provider/direct-control 구현 evidence는
  `docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253.md`다.
  실제 Windows guest credentialed execution smoke는
  `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass.md`에서
  persistent Windows VHD target, DPAPI LocalMachine credential reference, channel verify,
  guest-exec, Web/TUI smoke를 PASS로 닫았다. ISO boot-shell blocker는
  `docs/ga-ready/evidence/guest-execution-actual-vm-web-tui-smoke-2026-05-27-04253-blocked.md`에
  predecessor로 보존한다.
- Guest Execution running cancel policy는
  `docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md`가 기록한다.
  0.42.54는 persistent Windows guest의 long-running `guest-exec` job cancel을 terminal
  `canceled`와 `PCV_NATIVE_OPERATION_CANCELED`로 닫았다.
- 최신 closed Manual-admin package-pair는 계속 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`다.
  04250→04253 readiness는 현재 host baseline mismatch로
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-27-04250-04253-readiness-blocked.md`에
  blocked evidence로 기록한다.
- 04250→04254 baseline host prep과 closure attempt는
  `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-fullgate.md`에
  plan-only blocked evidence로 기록한다. baseline/target artifact는 모두 있으나 현재 host가 이미
  0.42.54라 dedicated 0.42.50 baseline host가 필요하다.
- Web/TUI running guest execution cancel affordance는
  `docs/ga-ready/evidence/web-tui-running-job-cancel-affordance-code-level-2026-05-28.md`에서
  code-level PASS로 기록한다. 다음 package/current-card에서 설치본으로 승격한다.

## 2026-05-27 0.42.53 Guest Execution provider/direct-control predecessor

- `0.42.53-admin-smoke`는 Guest Execution provider/direct-control, Windows credentialed smoke,
  full admin host mutation anchor의 predecessor로 보존한다.

## 2026-05-27 0.42.50 Guest Execution preview API/CLI predecessor

- `0.42.50-admin-smoke`는 preview API/CLI predecessor로 보존한다.
- Evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-27-04250.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-27-04250-hostmutation.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-27-04250.md`.

## 2026-05-26 0.42.49 Guest Execution policy/API preview historical predecessor Evidence

- 최신 product payload package smoke는 `0.42.49-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`가 기록한다.
  Clean MSI SHA-256은 `322bddcb89b05a882ed323429bcfce29f6a856701b801925b53c37423de0a6e2`,
  payload aggregate SHA-256은 `e348a46ad635b61347688750162de100914ad991dd255d10892d319872f19d10`,
  provenance commit은 `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04249-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04249`다. Full-gate MSI SHA-256은
  `465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32`, payload aggregate
  SHA-256은 `d49e70c1e291dd28040821fcb659222f4ff524b9c7353994f5e5447ec08610c5`,
  provenance commit은 `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.49-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04249.md` /
  `artifacts/installed-operator-surface-current-card-20260526-04249/summary.json`가
  Web/TUI/CLI current-card, runtime policy guest execution disabled boundary, direct preview
  `PCV_GUEST_EXEC_DISABLED` secret echo guard를 PASS로 기록한다.
- Guest Execution policy/API preview 구현 evidence는
  `docs/ga-ready/evidence/guest-execution-policy-api-preview-code-level-2026-05-26-04249.md`다.
  실제 guest command execution/provider/Web/TUI command panel은 아직 열지 않는다.
- 최신 closed Manual-admin package-pair는 계속 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`,
  descriptor `manual-admin-campaign-descriptor-20260526-04247-04248-closed`다.
  04248→04249 readiness는 현재 host baseline mismatch로
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04248-04249-readiness-blocked.md`에
  blocked evidence로 기록한다.
- 최신 public-boundary main push CI PASS는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md` /
  run `26518952796`, job `78104102372`, head
  `12bc72e856ea9ac7c6d54c4094873b2d8db9f672`가 기록한다.

## 2026-05-26 ADR-0009 Guest Execution Security Boundary Contract

- 최신 control-plane docs-contract는 ADR-0009 Guest Execution security boundary다.
  적용 문서는 `docs/adr/0009-guest-execution-security-boundary.md`, evidence는
  `docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md`다.
- 이 contract는 `pcvcli vm guest-exec`, `pcvcli vm guest-agent-ensure-channel`,
  Local API guest exec/channel route, Web/TUI direct command panel을 열기 전에 필요한
  credential reference, audit schema, secret redaction, timeout/cancel, RBAC,
  channel dry-run/verify/repair 경계를 확정한다.
- 이번 변경은 docs/spec/plan/evidence/index만 바꾸는 docs-contract slice다. Product payload
  변경이 아니므로 package build, full admin host mutation, manual-admin package-pair는 다음
  guest-execution code payload에서 연다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-26 0.42.48 Phase 3 Web/TUI QoS direct control historical predecessor Evidence

- 0.42.48 predecessor product payload package smoke는 `0.42.48-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`가 기록한다.
  Clean MSI SHA-256은 `a0014960979ed23cec8d882cddd22baaaf9435a71287bdc133a79ff0b381338c`,
  payload aggregate SHA-256은 `2013756155ce1d744ab4383ffdb70dfcc6d9d7c462192b51f4425f921a53850a`,
  provenance commit은 `46e745efc698a06e4b065a19c3f07217e821155e`다.
- 0.42.48 operational full admin host mutation predecessor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04248`다. Full-gate MSI SHA-256은
  `a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf`, payload aggregate
  SHA-256은 `2a14e47bf3fd48b17755ce901ec02b924ba9246ecbe91414f952428ca376d92f`,
  provenance commit은 `46e745efc698a06e4b065a19c3f07217e821155e`다.
- 최신 closed Manual-admin package-pair는 아직 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`,
  descriptor `manual-admin-campaign-descriptor-20260526-04247-04248-closed`다.
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`가 기록하며,
  Windows Update clean-host, Burn, MSIX, installed update/rollback 모두 PASS다.
- 0.42.48 설치본 Operator Surface current-card predecessor smoke는 `0.42.48-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md` /
  `artifacts/installed-operator-surface-current-card-20260526-04248-manual-admin/summary.json`가
  Web/TUI/CLI current-card를 PASS로 기록한다.
- 다음 개발 slice는
  `docs/ga-ready/evidence/post-04248-next-slice-selection-2026-05-26.md`에서
  ADR-0009 Guest Execution security boundary로 선택했다.
- Phase 2 Hyper-V QoS mutation은
  `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`에서 설치본
  package/fullgate/actual VM smoke/manual-admin closure까지 PASS로 닫혔다. Storage QoS는
  `pcvcli vm blkio-set`, network QoS는 `pcvcli vm bandwidth-set`로 dry-run, queued apply,
  rollback restore까지 확인했다.
- 최신 public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md` /
  run `26518952796`, job `78104102372`, head
  `12bc72e856ea9ac7c6d54c4094873b2d8db9f672`가 PASS로 기록한다. 04253 Guest Execution evidence closure는 local
  internal admin-smoke evidence이며 public trusted signing 또는 외부 stable publication을
  주장하지 않는다.

## 2026-05-26 0.42.47 closure historical predecessor Evidence

- `0.42.47-admin-smoke`는 Phase 2 Hyper-V QoS mutation 설치본 anchor였다.
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04247.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04247-hostmutation.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04247.md`,
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`를 historical
  predecessor로 보존한다.

## 2026-05-26 0.42.45 closure historical predecessor Evidence

- 최신 product payload package smoke는 `0.42.45-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04245.md`가 기록한다.
  MSI SHA-256은 `376218a0ee394e124f019e0e49a25718077585bac48f09c951da845bd96087bf`,
  payload aggregate SHA-256은 `3c1f9c9ab17144301976b9996d709c611a99122beb1296b457bf6444e2c6787a`,
  provenance commit은 `76c77a86bbb72e415b1968169c16f1638b76fa56`다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04245-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04245`다. Full-gate MSI SHA-256은
  `379fc96a63d853deb3fb57fa44231479a3785a6f9ca58bf8c924d96410bc3246`, payload aggregate
  SHA-256은 `d0568f69ac061815d06b1a41c819594da7cbb6c577dced2382945ae4502498a3`,
  provenance commit은 `76c77a86bbb72e415b1968169c16f1638b76fa56`다.
- 최신 closed Manual-admin package-pair는 `0.42.44-admin-smoke -> 0.42.45-admin-smoke`,
  descriptor `manual-admin-campaign-descriptor-20260526-04244-04245-closed`다.
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04244-04245.md`가 기록하며,
  Windows Update clean-host, Burn, MSIX, installed update/rollback 모두 PASS다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.45-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04245.md` /
  `artifacts/installed-operator-surface-current-card-20260526-04245/summary.json`가
  Web/TUI/CLI current-card를 PASS로 기록한다. 설치본 console access, account login,
  target-backed noVNC streaming smoke도 같은 package closure에 연결됐다.
- Web Console Console/noVNC UX QA는
  `docs/ga-ready/evidence/web-console-console-novnc-ux-qa-2026-05-26-04245.md`에서 PASS했다.
  `npm run browser:fixture`가 selected VM `pcv-browser-fixture`의 Account/Console card,
  noVNC path/reason, `Open selected console` handoff를 검증했고 installed account/browser/noVNC
  smoke artifact와 연결한다.
- Phase 1 direct-control 잔여 범위는
  `docs/ga-ready/evidence/phase1-account-novnc-direct-control-residual-review-2026-05-26.md`에서
  닫았다. Phase 1은 read-only Console Access Card와 open handoff만 열고 Guest Exec,
  Hyper-V QoS mutation, Web/TUI direct mutation은 다음 phase로 유지한다.
- Post-04245 extension Phase 2-5 planning은
  `docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26.md`에서 닫았다.
  Phase 2 Hyper-V QoS Mutation Policy는 ADR-0008과
  `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md`가
  소유한다. Phase 3 Web/TUI Direct Control은 backend policy closure 전까지 닫고,
  Phase 4 Guest Execution과 Phase 5 noVNC target config mutation은 각각 ADR-0009,
  ADR-0010 후보로 보류한다.
- Phase 2 Hyper-V QoS mutation code-level slice는
  `docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`가 기록한다.
  Source payload는 preview/apply API, queued job dispatch, native WMI code path,
  `pcvcli vm blkio-set`/`pcvcli vm bandwidth-set` UX, Runtime Policy operation set을
  포함한다. 설치본 승격 evidence는
  `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`가 기록하며,
  `0.42.47-admin-smoke` package build, `full-admin-host-mutation-gate-20260526-04247`,
  실제 VM 대상 PCVCLI storage/network QoS dry-run/apply/rollback smoke가 PASS했다.
  `0.42.45-admin-smoke -> 0.42.47-admin-smoke` manual-admin package-pair closure가 닫히며
  04247 current anchor로 승격됐다.
- 당시 public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass.md` /
  run `26413569064`, job `77753058728`, head
  `4f1f0bd8f7ffe9488dbb7175f65013870cf8d58f`가 PASS로 기록한다. PR #169 public-boundary는
  predecessor로 보존한다.

## 2026-05-25 0.42.44 closure historical predecessor Evidence

- 당시 product payload package smoke는 `0.42.44-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04244.md`가 기록한다.
  MSI SHA-256은 `eb9b6232a7c61431e2289850eecaba1c9a1d92bc93b88ce8eb4bd6f2ed3e8fe2`,
  payload aggregate SHA-256은 `debe36f469dd4f9782f056854142ff7392a62298962d1d4b9835cd14c3758f38`,
  provenance commit은 `9e96ffd423addfb0de139b1dfde0f8fc555c7566`다.
- 설치본 CLI read-only surface smoke는 package smoke predecessor로
  `docs/ga-ready/evidence/installed-cli-readonly-surface-smoke-2026-05-25-04244.md` /
  `artifacts/installed-cli-readonly-surface-smoke-20260525-04244/summary.json`가 PASS로 기록한다.
  `runtime policy`, `ops summary`, `network inventory`, `network list`는 direct command와
  interactive REPL 모두에서 실제 table data를 출력한다.
- 당시 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-25-04244-hostmutation.md` /
  `full-admin-host-mutation-gate-20260525-04244-r2`다. Full-gate MSI SHA-256은
  `bd1f45b62c683571fe238d8b570642d4f5818bd0b3f3c2e8d9a587841028e701`, payload aggregate
  SHA-256은 `3bbac62cea3c1e6651367ca8f66bcc49633d398743445325abadc63a35192847`,
  provenance commit은 `c7c7b0c9d4ea0b0296bc3ba423beb8eb7ac865e2`다.
- 당시 closed Manual-admin package-pair는 `0.42.43-admin-smoke -> 0.42.44-admin-smoke`,
  descriptor `manual-admin-campaign-descriptor-20260525-04243-04244-closed`다.
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-25-04243-04244.md`가 기록하며,
  Windows Update clean-host, Burn, MSIX, installed update/rollback 모두 PASS다.
- 당시 설치본 Operator Surface current-card smoke는 `0.42.44-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-25-04244-r2.md` /
  `artifacts/installed-operator-surface-current-card-20260525-04244-r2/summary.json`가
  Web/TUI/CLI current-card를 PASS로 기록한다.

## 2026-05-25 0.42.43 package smoke predecessor Evidence

- 최신 product payload package smoke는 `0.42.43-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04243.md`가 기록한다.
  MSI SHA-256은 `38be93dd0d944e3657ea6fea2f3e0f922ab4577c09d57183b5be299de90297b1`,
  payload aggregate SHA-256은 `95ba31a501bbf7e3cbb2ba103feb9638e0d01ebdfea922237ddbb15cea0c25f7`,
  provenance commit은 `93131de2bfab5fccfc2761538ead0460d3e7d85d`다.
- 최신 설치본 CLI usage-trim smoke는
  `docs/ga-ready/evidence/installed-cli-usage-trim-smoke-2026-05-25-04243.md` /
  `artifacts/installed-cli-usage-trim-smoke-20260525-04243/summary.json`가 PASS로 기록한다.
  `pcvcli vm get`과 interactive `vm get`의 command-specific usage error는 이제 전체
  `Usage:` block 없이 한 줄만 출력한다.
- 최신 operational full admin host mutation anchor는 아직 `0.42.41-admin-smoke` /
  `full-admin-host-mutation-gate-20260522-04241`이고, 최신 closed manual-admin package-pair도
  `0.42.40-admin-smoke -> 0.42.41-admin-smoke`다. `0.42.43-admin-smoke`의 full admin host
  mutation, clean-host, Burn, MSIX, manual-admin package-pair closure는 아직 실행하지 않았으며
  다음 gate 대상이다.

## 2026-05-25 0.42.42 package smoke predecessor Evidence

- 최신 product payload package smoke는 `0.42.42-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04242.md`가 기록한다.
  MSI SHA-256은 `d92e4c8bc8ee47da4a4c3b64d381725b3a1971b41ee41c9c24ba0a5f65a73582`,
  payload aggregate SHA-256은 `ad5ca2730ea932f08d72541b33b04cfb611ed6ca055f459b8988b48b74737c88`,
  provenance commit은 `37632159aaf0c9445c9b712f11f1dfee1a6f9c4f`다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.42-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-25-04242.md` /
  `artifacts/installed-operator-surface-current-card-20260525-04242/summary.json`가
  PASS로 기록한다. 이 smoke는 최상위 `pcvcli snapshot ...` command group 제거와
  `pcvcli vm snapshot ...` alias 유지를 설치본에서 확인했다.
- 최신 operational full admin host mutation anchor는 아직 `0.42.41-admin-smoke` /
  `full-admin-host-mutation-gate-20260522-04241`이고, 최신 closed manual-admin package-pair도
  `0.42.40-admin-smoke -> 0.42.41-admin-smoke`다. `0.42.42-admin-smoke`의 full admin host
  mutation, clean-host, Burn, MSIX, manual-admin package-pair closure는 아직 실행하지 않았으며
  다음 gate 대상이다.

## 2026-05-22 0.42.41 closure historical predecessor Evidence

- 최신 operational full admin host mutation anchor는 `0.42.41-admin-smoke` /
  `full-admin-host-mutation-gate-20260522-04241`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-22-04241.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-22-04241-hostmutation.md`가 기록한다.
  Full-gate MSI SHA-256은 `e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb`,
  payload aggregate SHA-256은 `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`,
  provenance commit은 `2f41da1073df6e65113ae8ddaeb183e9b55874f4`다.
- 최신 closed Manual-admin package-pair는 `0.42.40-admin-smoke -> 0.42.41-admin-smoke`,
  descriptor `manual-admin-campaign-descriptor-20260522-04240-04241-closed`다.
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-22-04240-04241.md`가 기록하며,
  Windows Update clean-host, Burn, MSIX, installed update/rollback 모두 PASS다.
- 최신 설치본 Operator Surface current-card smoke는 `0.42.41-admin-smoke`이고
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-22-04241.md` /
  `artifacts/installed-operator-surface-current-card-20260522-04241/summary.json`가
  Web/TUI/CLI current-card를 PASS로 기록한다. 해당 ops summary는 full-gate batch
  `full-admin-host-mutation-gate-20260522-04241`와 closed manual-admin package-pair
  `0.42.40-admin-smoke -> 0.42.41-admin-smoke`를 노출한다.
- ADR-0007 PCVCLI Hyper-V QoS/guest-service parity는 `vm.limit` mutation과
  `vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status`, `vm.guest-ping` readback을
  Hyper-V semantics로 닫았다. Historical predecessor evidence는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04239-hostmutation.md` /
  `full-admin-host-mutation-gate-20260520-04239`와
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`에 보존한다.
  Linux cgroup/qemu guest agent 호환 claim은 하지 않는다.
- 설치본 PCVCLI QoS/guest targeted smoke는
  `docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md` /
  `artifacts/installed-cli-qos-guest-smoke-20260521-04239/summary.json`가 PASS로 기록한다.
  이 targeted smoke는 0.42.39 설치본 CLI command path anchor다.
- Web/TUI QoS/guest readback surface는
  `docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`가 code-level
  PASS로 기록한다. Web 선택 VM detail `QoS / Guest Readback` panel과 TUI 선택 VM
  `G` readback은 read-only route만 조회하며 direct mutation/control은 열지 않는다.
  이 변경은 Operator Surface product payload 변경이므로 `0.42.40-admin-smoke`
  package chain을 열었고 manual-admin package-pair로 닫았다.
- 실제 VM 기반 설치본 TUI row projection smoke는
  `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`가 기록한다.
  실제 VM `pcv-ux-qos-04241`에서 설치본 `pcvtui --smoke-once vm`이 row projection을 PASS했고
  VM/root cleanup도 PASS했다. 04240 Web no-overlap/readback PASS와 설치본 TUI blocker 기록은
  predecessor로 보존한다.
- PR #169 public-boundary PASS는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md`가
  기록한다. 후속
  `docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`는
  product payload 변경이 없어서 `0.42.42-admin-smoke` package chain과 installed
  account/noVNC smoke를 열지 않는다고 고정한다. PR #168, PR #167, PR #164, PR #163,
  PR #162, PR #160 public-boundary는 historical predecessor로 보존한다.
  Public trusted signing, public stable installer URL, winget submission, 외부
  stable publication evidence가 아니다. 아래 0.42.35/0.42.37 및 2026-05-19 section은
  historical predecessor로 해석한다.

## 2026-05-20 0.42.35 closure + 0.42.37 installed lifecycle historical predecessor

- `0.42.35-admin-smoke` full admin host mutation과
  `0.42.34-admin-smoke -> 0.42.35-admin-smoke` manual-admin package-pair closure는
  immediate predecessor로
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04235-hostmutation.md`,
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md`에 보존한다.
  Target MSI SHA-256은 `12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697`,
  update ZIP SHA-256은 `71ccbe6188de9a52465beae9afc165f7777631bacbbc14a3137d0f9a6379994d`다.
- `0.42.37-admin-smoke` Hyper-V pause lifecycle fast-follow는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04237.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md`,
  `artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`가 기록한다.
  MSI SHA-256은 `05dc31965af68792d21d919e19cb07997207d0514fd0ee39169d92129e95f67e`,
  payload aggregate SHA-256은 `1e2487bfe474daad624a3ef67837a278ab5d25a71c654f8b7c18c95e3cc94e9e`다.

## 2026-05-19 0.42.34 Closure historical predecessor

- 최신 installed/package anchor는 `0.42.34-admin-smoke`이며,
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`,
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가
  기록한다. Full admin host mutation batch는 `full-admin-host-mutation-gate-20260519-04234`다.
  범위는 PCVCLI Linux palette/UTF-8 interactive shell, full admin host
  mutation, manual-admin package-pair, 설치본 Web/TUI/CLI current-card smoke,
  자동 token discovery, 전역 PATH 실행 확인이다.
- Manual-admin package-pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`,
  descriptor는 `manual-admin-campaign-descriptor-20260519-04232-04234-closed`다.
  Target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`,
  update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`,
  payload aggregate SHA-256은 `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`다.
- 이 closure는 0.42.33 intermediate package의 redirected UTF-8 prompt glyph 문제를
  닫고, 0.42.32 closure를 historical predecessor로 내린다.
- Public trusted signing, public stable installer URL, winget submission, 외부 stable
  publication evidence가 아니다.

## 2026-05-19 0.42.32 Closure historical predecessor

- 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`와 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04232-hostmutation.md`에 보존한다. Full admin host mutation은 `full-admin-host-mutation-gate-20260519-04232`, current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04232.md`, manual-admin package-pair는 `0.42.31-admin-smoke -> 0.42.32-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260519-04231-04232-closed`다. Target MSI SHA-256은 `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`, update ZIP SHA-256은 `c2e5c577d1a9bbec1ce6ca7ca2f79588d17b908d4aa639adb7968e5a09ce38da`, payload aggregate SHA-256은 `21e2f8136ac53384bf86966e51f9040f7bbb37e62bc9e761640c0d1aeff35956`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. PR #156/#155/#154 public-boundary evidence와 0.42.31 이하 package-pair는 historical predecessor로 보존하고, 0.42.29 account/noVNC smoke는 historical PASS로 유지한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1` / `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated` / `host-ops-web-diagnostics-bucket-table-v1`다.


이 문서는 GA-ready 제어 평면의 현재 결정, matrix, evidence 진입점을 묶는다.
최신 운영 판단은 문서 상단 생성 블록과 2026-08-25 Required CI authority를 먼저 보고,
실행 증거는 `EVIDENCE_INDEX.md`와 `CURRENT_EVIDENCE_LEDGER.md`를 확인한다. 아래 dated
aggregate 목록은 historical snapshot이다.

## 현재 결정

- ADR 인덱스: `docs/ADR_INDEX.md`
- 내부 사설망 배포: `docs/adr/0006-internal-private-network-distribution.md`
- Release/version 정책: `docs/adr/0002-release-version-policy.md`
- 내부 signing 정책: `docs/adr/0003-internal-trusted-signing-policy.md`
- Public distribution 미채택 종료: `docs/adr/0005-public-distribution-operations-expansion-candidate.md`

## 매트릭스

- Route 승격: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- 내부 배포: `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- Public 미채택 종료 이력: `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`
- 저장소 migration: `docs/ga-ready/REPO_MIGRATION_MAP.md`
- 검증 ownership: `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
- 증거 인덱스: `docs/ga-ready/EVIDENCE_INDEX.md`
- Current evidence ledger: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`

## 최신 Evidence — historical aggregate snapshot

이하 `최신` 또는 `current` 표기는 각 dated evidence가 작성된 당시 의미이며 현재 authority가
아니다. 현재 operational/CI 권위는 문서 상단 두 블록이 소유한다.

- 최신 full admin host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04249-hostmutation.md`
  - version `0.42.49-admin-smoke`
  - batch `full-admin-host-mutation-gate-20260526-04249`
  - MSI SHA-256 `465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32`, provenance commit `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`
- 최신 closed manual-admin package-pair closure: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`
  - baseline `0.42.47-admin-smoke`, target `0.42.48-admin-smoke`
  - descriptor `manual-admin-campaign-descriptor-20260526-04247-04248-closed`
  - readiness, installed update/rollback, Windows Update clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 PASS
  - public trusted signing과 외부 stable publication evidence가 아님
- Historical Host Ops lifecycle predecessor: `0.42.26-admin-smoke -> 0.42.27-admin-smoke`
  - evidence `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`, descriptor `manual-admin-campaign-descriptor-20260517-04226-04227-closed`
  - Host Ops lifecycle descriptor bridge `host-ops-lifecycle-descriptor-bridge-v1`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
  - public trusted signing과 외부 stable publication evidence가 아님
  - Historical Operator Surface predecessor: `0.42.27-admin-smoke -> 0.42.28-admin-smoke`
    - evidence `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md`, descriptor `manual-admin-campaign-descriptor-20260517-04227-04228-closed`
    - full admin host mutation batch `full-admin-host-mutation-gate-20260517-04228`
    - target MSI SHA-256 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, clean package MSI SHA-256 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`, update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
    - public trusted signing과 외부 stable publication evidence가 아님
  - Historical selector/package-chain predecessor: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
    - evidence `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`, descriptor `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
    - target MSI SHA-256 `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`, update ZIP SHA-256 `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`, provenance commit `d306712ad671c8a00d5c560765b8952e24a07502`
    - public trusted signing과 외부 stable publication evidence가 아님
- 최신 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md`
  - run `26518952796`, job `78104102372`, head `12bc72e856ea9ac7c6d54c4094873b2d8db9f672`
  - previous 0.42.53 credentialed Windows guest execution smoke public-boundary 같은 evidence, run `26516950720`
  - previous 0.42.53 ISO evidence roll-forward public-boundary 같은 evidence, run `26512890221`
  - previous 0.42.53 evidence closure public-boundary 같은 evidence, run `26511891436`
  - previous 0.42.53 evidence closure roll-forward public-boundary 같은 evidence, run `26510159990`
  - previous 0.42.53 evidence gates roll-forward public-boundary 같은 evidence, run `26496046109`
  - earlier 0.42.53 evidence gates roll-forward public-boundary 같은 evidence, run `26495580805`
  - initial 0.42.53 evidence closure public-boundary 같은 evidence, run `26494683032`
  - previous 0.42.53 provider public-boundary `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-provider-postpush-pass.md`, run `26494136304`
  - previous 0.42.50 public-boundary `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass.md`, run `26489610881`
  - post-merge follow-up `docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`
  - guard `public-boundary-ci-required`, checkout `actions/checkout@v6.0.2`, PASS
  - 이전 0.42.48 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04248-manual-admin-postpush-pass.md`, run `26445409133`, job `77850326001`, head `ea1e7b85757f35feb10811dda4bbc38d94b304ac`로 보존한다.
  - 이전 PR #169 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md`, run `26288103559`, job `77380766318`, head `11b123311d718cf77e87ccc7b8dea7c5728dc463`로 보존한다.
  - PR #167, PR #164, PR #163, PR #162, PR #160, PR #156 후속 deferred 판단은 historical predecessor로 보존한다.
  - 이전 PR #167 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`, run `26228675428`, job `77182631331`, head `f173f9857089de61ca1fb2b7a2da7839a3dd73a8`로 보존한다.
  - 이전 PR #164 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md`, run `26170972989`, job `76988240617`, head `03402f1607b735f2d92291ae6109d7986d9a57b8`로 보존한다.
  - Historical PR #156 public-boundary main push CI:
    `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`,
    run `26017721669`, job `76471545641`, head
    `a4509c552c003ee0fc87b54b26529686e6dfeb84`
  - PR #155 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`, run `26013384587`, job `76458402221`, head `2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f`로 historical predecessor 보존
  - PR #154 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md`, run `25989986761`, job `76394250912`, head `d7f611dfc14a9fa1507f936559209513272b585a`로 historical predecessor 보존
  - PR #154 후속 package decision은 `deferred-no-product-payload-change-after-pr154`
  - PR #153 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`, run `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`로 historical predecessor 보존
  - PR #152 main push evidence는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md`, run `25985786230`, job `76382711230`, head `ca07514097f4e9524a7f3630d321c9666593c962`로 historical predecessor 보존
  - public trusted signing과 외부 stable publication evidence가 아님

- Post-04226 ledger/contract follow-up: `docs/ga-ready/evidence/post-04226-ledger-contract-followup-2026-05-17.md`
  - Runtime/API current-card descriptor id direct expose: `current_card_descriptor_batch_id`
  - manual-admin descriptor schema v2: `descriptor_schema_version=2`, `manual-admin-descriptor-generation-contract-v2`
  - pre-branch product payload change: `false`; this branch opens next trigger `post-04226-ledger-contract-merge`
  - host mutation performed `false`, public trusted signing과 외부 stable publication evidence가 아님
- Historical `0.42.26-admin-smoke` full admin host mutation PASS: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`
  - version: `0.42.26-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04226`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226`, `artifacts/os-mutation-gates-batch-profile-20260516-04226`
  - full-gate MSI SHA-256: `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`
  - full-gate provenance commit: `d6500c01c972cbc7ca1e290e51120181ceea1501`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04226`
  - current-card artifact: `artifacts/installed-operator-surface-current-card-20260516-04226`
  - current evidence contract: `runtime-api-current-evidence-rollup-v1`
  - runtime bridge: `runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`
  - public trusted signing과 외부 stable publication evidence가 아님
- Historical `0.42.25-admin-smoke -> 0.42.26-admin-smoke` Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`
  - baseline `0.42.25-admin-smoke`, target `0.42.26-admin-smoke`
  - target MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`
  - update ZIP SHA-256 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`
  - readiness, installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation PASS
  - descriptor summary: `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json`
  - descriptor status: `pass`, `missing_count=0`, `not_pass_count=0`
  - public trusted signing과 외부 stable publication evidence가 아님
- Historical `0.42.26-admin-smoke` product payload package build: `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04226.md`
  - version `0.42.26-admin-smoke`
  - artifact `artifacts/admin-smoke-package-20260516-04226`
  - MSI SHA-256 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`, provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`
  - public trusted signing과 외부 stable publication evidence가 아님
- Historical PR #151 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`
  - run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`
  - guard `public-boundary-ci-required`, checkout `actions/checkout@v6.0.2`, PASS
  - public trusted signing과 외부 stable publication evidence가 아님
  - 이전 Manual admin package-pair initial descriptor: `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`
    - baseline `0.42.25-admin-smoke`, target `0.42.26-admin-smoke`
    - readiness PASS, `overall_status=blocked-by-missing-evidence`, `missing_count=4`, `not_pass_count=1`
    - 2026-05-17 campaign에서 닫힌 package-pair PASS로 승격됐으며 이 initial descriptor는 historical record로 보존한다.
  - 이전 Manual admin package-pair PASS: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`
    - baseline `0.42.24-admin-smoke`, target `0.42.25-admin-smoke`
    - target/full-gate MSI SHA-256 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance commit `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`
    - update ZIP SHA-256 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`
    - descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`, `missing_count=0`, `not_pass_count=0`
    - 04226 closure 이후 historical closed package-pair로 보존한다.
- Historical `0.42.26-admin-smoke` installed operator surface current-card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`
  - artifact `artifacts/installed-operator-surface-current-card-20260516-04226`
  - Web Console `200`, TUI runtime smoke PASS, CLI ops summary PASS
  - latest batch `full-admin-host-mutation-gate-20260516-04226`
  - current evidence contract `runtime-api-current-evidence-rollup-v1`
  - Runtime/API registry bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`
  - host mutation performed `true`, public trusted signing과 외부 stable publication evidence가 아님
- Post-04223 full host mutation current-card: `docs/ga-ready/evidence/post-04223-full-host-mutation-current-card-2026-05-16.md`
  - result `FULL_HOST_MUTATION_CURRENT_CARD_PASS_NEXT_SLICE_SELECTED`
  - next product payload candidate `0.42.24-admin-smoke`
  - next package-pair candidate `0.42.23-admin-smoke -> 0.42.24-admin-smoke`
  - closed package-pair evidence `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`, descriptor `manual-admin-campaign-descriptor-20260516-04222-04223-closed`, target MSI SHA-256 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, update ZIP SHA-256 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`, provenance commit `676b4177b10dc80209969066857bab6008ff2473`
  - stale local codex branch cleanup `12`개 삭제, worktree-bound/unmerged gone branch 보존
  - public trusted signing과 외부 stable publication evidence가 아님
- 이전 04224 Runtime/API current evidence rollup package/fullgate/current-card:
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`
  - batch `full-admin-host-mutation-gate-20260516-04224`, package build MSI SHA-256 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit `b974d6b541423f2e4160f726f96155b16f105e9d`
  - Runtime/API current evidence contract `runtime-api-current-evidence-rollup-v1`; 04226 closure 이후 historical predecessor로 보존한다.
- 이전 04222 full admin host mutation PASS: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`
  - version: `0.42.22-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04222`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04222`, `artifacts/os-mutation-gates-batch-profile-20260516-04222`
  - full-gate MSI SHA-256: `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`
  - clean package MSI SHA-256: `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`
  - provenance commit: `8a38995cc25a888f64473e9a2869740949ad6b24`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04222`
  - public-boundary post-merge run `25952150476`, job `76291983316`
  - public trusted signing과 외부 stable publication evidence가 아님
- 이전 full admin host mutation PASS: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`
  - version: `0.42.21-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221`, `artifacts/os-mutation-gates-batch-profile-20260516-04221`
  - full-gate MSI SHA-256: `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`
  - clean package MSI SHA-256: `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b`
  - provenance commit: `3b8c48deb4c31675f6fce46c320703f23c27c131`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04221`
  - public-boundary successor run `25938745434`, job `76250726268`
  - public trusted signing과 외부 stable publication evidence가 아님
- 이전 full admin host mutation PASS: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`
  - version: `0.42.20-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`
  - full-gate MSI SHA-256: `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`
  - clean package MSI SHA-256: `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`
  - provenance commit: `0895d018935298721b25b5d9ce1ae083a6690c25`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04220`
  - current-card artifact: `artifacts/installed-current-card-20260516-04220-fullgate`
  - public-boundary workflow rerun은 `docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md` run `25933428239`, job `76232707240`에서 PASS; 이전 run `25930077313`은 GitHub billing/spending-limit historical blocker
  - public trusted signing과 외부 stable publication evidence가 아님
- 이전 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md`
  - run `25933861585`, job `76234195716`, head `686e4201f823295dc65cde302f613a982ab8cade`
  - guard `public-boundary-ci-required` PASS
  - checkout maintenance target `actions/checkout@v6.0.2`
  - branch protection/ruleset은 private repo 현재 플랜에서 unavailable이므로 PR/merge guard를 대체 required guard로 운영
  - package build decision `deferred-no-product-payload-change-after-04220`
  - public trusted signing과 외부 stable publication evidence가 아님
- 이전 public-boundary checkout v6.0.2 main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md`
  - run `25934411998`, job `76236050409`, head `3933231e6e2abf3a398dfcc3fdc999b3df38dac6`
  - `actions/checkout@v6.0.2`, guard `public-boundary-ci-required` PASS
  - Node.js 20 deprecation warning 미관찰
  - public trusted signing과 외부 stable publication evidence가 아님
- Post-ci-maintenance development slices: `docs/ga-ready/evidence/post-ci-maintenance-dev-slices-2026-05-16.md`
  - result `CODE_LEVEL_PASS`
  - source anchor `0.42.20-admin-smoke`, next product payload candidate `0.42.21-admin-smoke`
  - `runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`
  - `hyperv_provider_callsite_guard=hyperv-wmi-provider-callsite-drift-guard-v1`
  - `host_ops_reason_code_contract=host-ops-dryrun-mutation-reason-code-v1`
  - `manual_admin_descriptor_generation_contract=manual-admin-descriptor-generation-contract-v2`
  - host mutation performed `false`, public trusted signing과 외부 stable publication evidence가 아님
- Post-04221 successor operator surface: `docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md`
  - result `CODE_LEVEL_AND_OPERATOR_SURFACE_PASS`
  - public-boundary successor run `25938745434`, installed operator current-card artifact `artifacts/installed-operator-surface-current-card-20260516-04221`
  - Web Console diagnostics direct expose `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
  - next product payload candidate `0.42.22-admin-smoke`
  - host mutation performed `false`, public trusted signing과 외부 stable publication evidence가 아님
- Post-04220 development slices: `docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md`
  - result `CODE_LEVEL_PASS`
  - source anchor `0.42.20-admin-smoke`
  - `runtime_diagnostics_ops_summary_contract=runtime-api-diagnostics-ops-summary-contract-v1`
  - `hyperv_wmi_common_helper_contract=hyperv-wmi-common-helper-contract-v1`
  - `host_ops_mutation_boundary_contract=service-eventlog-firewall-truststore-credential-manager-data-root`
  - `packaging_release_next_trigger=product-payload-change-after-04220-fullgate`
  - public-boundary workflow rerun run `25933428239`는 PASS; 이전 run `25931297085`는 GitHub billing/spending-limit historical blocker
  - host mutation performed `false`, public trusted signing과 외부 stable publication evidence가 아님
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04219-04220.md`
  - baseline `0.42.19-admin-smoke`, target `0.42.20-admin-smoke`
  - target MSI SHA-256 `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`
  - update ZIP SHA-256 `8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b`
  - readiness, installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation PASS
  - descriptor summary: `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-campaign-descriptor-supervised/summary.json`
- Post-04219 follow-up execution: `docs/ga-ready/evidence/post-04219-followup-execution-2026-05-16.md`
  - version `0.42.19-admin-smoke`
  - descriptor batch `manual-admin-campaign-descriptor-20260516-04218-04219`
  - manual-admin readiness와 descriptor batch execution은 완료, descriptor는 누락 lifecycle evidence 때문에 `blocked-by-missing-evidence`를 기록
  - full admin host mutation은 `prepared`; 같은 0.42.19 version string으로 수정 payload를 재사용하지 않기 위해 실행하지 않음
  - Runtime queued mutation route registry, Hyper-V `operation-level-telemetry-error-contract-v1`, Host Ops extended family helper, `public-boundary-ci-required` workflow를 고정
  - public trusted signing과 외부 stable publication evidence가 아님
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md`
  - baseline `0.42.16-admin-smoke`, target `0.42.18-admin-smoke`
  - target MSI SHA-256 `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`, provenance commit `9121d1f5e7fa83d803c484a44698d4fc8e825c19`
  - update ZIP SHA-256 `8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd`
  - readiness, installed update/rollback, Windows Update NoContact recovery 포함 clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation PASS
  - descriptor summary: `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised/summary.json`
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`
  - baseline `0.42.12-admin-smoke`, target `0.42.13-admin-smoke`
  - target MSI SHA-256 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`, provenance commit `a28bb808386f206c9dbf7dcaeee232eacb648434`
  - update ZIP SHA-256 `638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9f700b83059a8c9`
  - status: 04214→04215 PASS 이후 historical predecessor
- 이전 full admin host mutation PASS: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`
  - version: `0.42.18-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260515-163107-04218`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260515-163107-04218`, `artifacts/os-mutation-gates-batch-profile-20260515-163107-04218`
  - full-gate MSI SHA-256: `0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09`
  - clean package MSI SHA-256: `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`
  - provenance commit: `9121d1f5e7fa83d803c484a44698d4fc8e825c19`
  - signing mode: `AllowUnsignedDev`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260515-163107-04218`, `descriptor_excluded_from_operational_latest=true`
  - current-card artifact: `artifacts/installed-current-card-20260515-04218-fullgate`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 product payload package build: `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`
  - version `0.42.19-admin-smoke`, artifact `artifacts/admin-smoke-package-20260515-04219`
  - MSI SHA-256 `3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55`, provenance commit `2b7bd9ed702a785361ea5bbaa8a969280d400360`
  - post-04218 code contract payload package build; update ZIP/package-pair/full host mutation은 미실행
  - guard `public-boundary-ci-required`, public trusted signing과 외부 stable publication evidence가 아님
- Ops summary descriptor selector guard package: `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`
  - `0.42.14-admin-smoke`, artifact `artifacts/admin-smoke-package-20260514-04214-selectorfix`
  - MSI SHA-256 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`
  - 04218 follow-up current-card artifact: `artifacts/installed-current-card-20260515-04218-fullgate`
  - descriptor batch 이후에도 `batch_evidence.status=available`, latest batch `full-admin-host-mutation-gate-20260515-163107-04218`, descriptor excluded `true`
- Post-04218 contract alignment: `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`
  - source anchor: `0.42.18-admin-smoke`
  - `runtime_api_diagnostics_bridge=route-family-evidence-linked`
  - `hyperv_dispatch_catalog_contract=vm-checkpoint-network-fixed`
  - `host_ops_lifecycle_buckets=service-eventlog-firewall-truststore-data-root-separated`
  - `packaging_release_next_trigger=pending-next-product-payload-after-04218-fullgate`
  - `operator_surface_journey_alignment=web-console-tui-cli-current-card`
  - `public_boundary_preserved=adr-0005-closed-adr-0006-internal-only`
  - host mutation performed: `false`
- Post-04218 runtime/domain development slice:
  `docs/ga-ready/evidence/post-04218-runtime-domain-slices-2026-05-15.md`
  - `actual_execution=code-contract-regression`
  - `runtime_api_diagnostics_bridge=route-family-evidence-linked`
  - `hyperv_dispatch_catalog_contract=vm-checkpoint-network-fixed`
  - `host_ops_lifecycle_bucket_contract=service-eventlog-firewall-truststore-data-root-separated`
  - `packaging_release_next_trigger=product-payload-change-after-04218-fullgate`
  - `operator_surface_journey_alignment=web-console-tui-cli-current-card`
  - `public_boundary_preserved=adr-0005-closed-adr-0006-internal-only`
  - host mutation performed: `false`
- Post-04218 follow-up execution:
  `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`
  - `package_build_decision=executed-0.42.19-admin-smoke`
  - artifact `artifacts/admin-smoke-package-20260515-04219`
  - `runtime_route_registry_source=ApiHandlerAdapterContract`
  - `hyperv_dispatch_model=handler-registry-delegate-map`
  - `host_ops_family_helpers=service-eventlog-firewall-truststore-data-root`
  - `operator_surface_snapshot_parity=web-console-tui-cli-current-card`
  - `public_boundary_guard=public-boundary-ci-required`
  - host mutation performed: `false`
- Post-04212 `1-2-3-4-5` current-card follow-up:
  `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`
  - `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product payload 변경이 없어 `0.42.13-admin-smoke` package build, package-pair, clean-host campaign, full admin host mutation은 열지 않았다.
  - Web Console Dashboard와 Evidence view current-card smoke는 `artifacts/web-console-current-card-20260514-04212-rerun-followup`에서 PASS다.
  - 표시 batch/version: `full-admin-host-mutation-gate-20260514-04212-rerun`, `0.42.12-admin-smoke`
  - host mutation performed by this evidence: `false`
- 이전 product payload package build: `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`
  - version `0.42.14-admin-smoke`, artifact `artifacts/admin-smoke-package-20260514-04214-selectorfix`
  - MSI SHA-256 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`, provenance commit `a28bb808386f206c9dbf7dcaeee232eacb648434`
  - selector guard code/package evidence로 보존한다.
  - historical note: 04215 full gate 전에는 full host mutation current claim은 04212 evidence가 소유했다.
    - 이전 2026-05-14 explicit/rerun, 2026-05-13 `0.42.12-admin-smoke` / 04212, `0.42.11-admin-smoke` / 04211, `0.42.9-admin-smoke` / 0429, `0.42.8-admin-smoke` / 0428, `0.42.7-admin-smoke` / 0427, `0.42.3-admin-smoke` / 0423, `0.42.2-admin-smoke` / 0422 full admin host mutation evidence는 historical로 보존한다: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit`, full-gate MSI SHA-256 `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`, full-gate provenance commit `d338b8a99f3e1e3839ac89a6de0da034ff3da148`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-11-0422-hostmutation.md`
- Post-04210 follow-up execution: `docs/ga-ready/evidence/post-04210-followup-execution-2026-05-13.md`
  - `origin/main` `371e05055c7488f923c0038f87f1a1288054c271` 기준 당시 새 product payload 변경이 없어 package build는 `deferred-until-next-product-payload-change`, full admin host mutation campaign은 `not-run-no-new-product-payload`였고, 후속 ops summary data builder payload 변경으로 04212 package build/full gate/package-pair campaign을 실행해 닫았다.
  - `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`를 한국어 evidence로 채택
  - adopted artifacts: `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`, `artifacts/installed-web-asset-refresh-20260510-235258`
- `0.42.10-admin-smoke` duplicate outer start RCA: `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`
  - status `historical-rca-only`, target MSI SHA-256 `bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273`, provenance commit `d7d5ba38ee1d4f74676477eb13701af65abca008`
  - native service-action repair가 service를 `Running`으로 만든 뒤 outer wrapper의 duplicate `sc.exe start`가 `1056 already running`을 반환했다.
  - `0.42.11-admin-smoke`가 `native-service-action-controls-final-state` reason으로 outer start를 skip해 닫았다.
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`
  - installed update/rollback, wrapper repair, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture PASS
  - target MSI SHA-256 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, provenance commit `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`
  - update ZIP SHA-256 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`
  - campaign descriptor summary: `artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised/summary.json`
  - previous package-pair: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`
    - previous target MSI SHA-256 `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`, provenance commit `987beb51025a5aa926df7d9a905019b4d6d29705`
- Clean-host Windows Update NoContact recovery guard: `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`
  - code-level runner hardening: Windows Update reboot 이후 heartbeat `NoContact` + CPU idle 상태가 threshold 이상 지속되면 `Stop-VM -TurnOff -Force; Start-VM` recovery를 한 번 수행하고 `recovery_actions`를 summary에 기록
  - host mutation performed by this evidence: `false`
- Post-04212 follow-up execution: `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`
  - `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이 없어 `0.42.13-admin-smoke` package build, full admin host mutation, package-pair campaign을 열지 않았다.
  - clean-host recovery guard는 다음 실제 clean-host run에서 `recovery_actions`와 `automatic_recovery_performed` summary key로 판정한다.
  - host mutation performed by this evidence: `false`
- Manual admin hardening과 package-pair rebaseline: `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`
- Manual admin 0427→0428 campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`
  - installed update/rollback, dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture PASS
  - target post-merge MSI SHA-256 `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, provenance commit `5397e580c98a34e8b7beb5b9773d1d857025315b`
  - update ZIP SHA-256 `f8bb7900687c1a19eafc57266adbd388c826b15b4926808beac8ac0e79871ccc`
  - campaign descriptor summary: `artifacts/manual-admin-campaign-20260512-0427-0428/manual-admin-campaign-descriptor/summary.json`
- Manual admin 0428→0429 candidate: `docs/ga-ready/evidence/manual-admin-campaign-candidate-2026-05-13-0428-0429.md`
  - installed update/rollback PASS, clean-host/Burn/MSIX/descriptor는 아직 PASS claim 아님
  - target MSI SHA-256 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, update ZIP SHA-256 `7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f`
- Post-0426 follow-up triage: `docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md`
  - post-merge package provenance aligned, Batch Supervisor `ManualAdminCampaignDescriptor` profile added
  - 0423→0424 campaign은 historical-only blocker record로 재분류
    - 사용자 승인 후 `0.42.7-admin-smoke` build/full admin host mutation gate/installed listener current-card smoke를 실행했고, 추가 승인으로 0427→0428 package-pair와 0428 full gate를 PASS했다. 이후 0429 full gate와 0429→04211 package-pair, 04211 full gate, 04212 full gate까지 PASS했으므로 최신 full gate current claim은 04212 evidence가 소유
- Historical 0423→0424 campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`
  - `historical-partial-pass-clean-host-blocked`
  - PASS bucket은 보존하지만 current package-pair claim은 0427→0428 evidence가 소유
- Historical post-0423 triage와 implementation slices: `docs/ga-ready/evidence/post-0423-followup-triage-2026-05-12.md`, `docs/superpowers/plans/2026-05-12-purecvisor-desktop-node-post-0423-followup-slices.md`
  - 0427→0428 PASS 이후 current plan이 아니라 historical planning record로 보존
- Runtime/Core, Host Ops, Hyper-V provider boundary code-level follow-up: `docs/ga-ready/evidence/runtime-host-hyperv-domain-followup-code-level-2026-05-12.md`
- Runtime/Core console/ops-summary, Hyper-V provider file split, historical docs follow-up: `docs/ga-ready/evidence/runtime-hyperv-operator-followup-code-level-2026-05-12.md`

## Historical next Manual Admin preparation snapshot

- 다음 campaign descriptor: `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`
- Current closed package-pair는 `0.42.25-admin-smoke -> 0.42.26-admin-smoke`이며
  `overall_status=pass`, `missing_count=0`, `not_pass_count=0`이다.
- `0.42.26-admin-smoke` full admin host mutation gate는 Runtime/API
  `current_evidence` rollup과 descriptor/current-card selector guard를 유지한 상태로
  operational latest anchor를 04226 full gate로 올렸고, 2026-05-17 package-pair
  closure도 같은 operational package root로 닫혔다. `0.42.25-admin-smoke` 및
  이전 full gate는 historical predecessor로 보존한다.
- 다음 clean-host campaign은 Windows Update NoContact recovery guard를 포함한 runner contract로 실행한다.
- Post-04212 follow-up execution evidence는 `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`다.
  `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이
  없으므로 다음 package build와 host mutation campaign은 `next-product-payload-change`까지 보류한다.
- Post-04212 `1-2-3-4-5` current-card follow-up evidence는
  `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`다.
  `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product payload 변경이
  없으므로 package/host mutation chain은 계속 보류하고 Web Console current-card smoke만 PASS로 닫았다.
- `0.42.10-admin-smoke`는
  `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`
  historical RCA로만 보존한다.
- Post-04210 follow-up execution evidence는 `docs/ga-ready/evidence/post-04210-followup-execution-2026-05-13.md`다.
- 최신 닫힌 package-pair descriptor는 `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json`이다.
- 최신 Batch Supervisor descriptor manifest는 `manual-admin-campaign-descriptor-20260517-04225-04226-closed`이고 summary는 `ok=true`, `executed_steps=1`, `missing_count=0`, `not_pass_count=0`이다.
- 최신 `0.42.26-admin-smoke` package build record는 `artifacts/admin-smoke-package-20260516-04226`에 보존하며, MSI SHA-256은 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`이다. 최신 operational package root는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226`이고 MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`이다.
- 최신 full admin host mutation은 `0.42.26-admin-smoke` / `full-admin-host-mutation-gate-20260516-04226`이며 current-card smoke가 `batch_evidence.status=available`로 닫혔다.
- Batch Supervisor `ManualAdminCampaignDescriptor` profile은 descriptor generation을 non-mutating manifest step으로 실행한다.
- 실제 host mutation은 elevated operator opt-in과 `Invoke-PcvBatchSupervisor.ps1 -AllowHostMutation`이 있을 때만 실행한다.

## Historical Dashboard/Wiki Current Card snapshot

- dashboard/wiki current card 상태는 `installed-listener-batch-evidence-available`이다.
- Web Console은 정적 문서가 아니라 `GET /api/v1/ops/summary`의 `batch_evidence.latest`를 current evidence card로 표시한다.
- 설치본 smoke에서 installed manifest `0.42.26-admin-smoke`, `batch_evidence.status=available`,
  `latest.batch_id=full-admin-host-mutation-gate-20260516-04226`,
  Runtime/API current evidence `runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge
  route detail count `4`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`,
  unauthenticated runtime policy `401`/`PCV_AUTH_REQUIRED`를 확인했다.
- `artifacts/installed-operator-surface-current-card-20260516-04226`
  current-card smoke는 2026-05-16 04225→04226 descriptor candidate, 2026-05-17 04225→04226 closed package-pair PASS, 이전 04224→04225 package-pair PASS(`manual-admin-campaign-descriptor-20260516-04224-04225-closed`)를 구분하며 04226 full gate가 최신 full-admin batch로 표시됨을 확인했다.
- GA-ready current card는 04226 full admin host mutation PASS와 04225→04226 닫힌
  package-pair PASS, 04224→04225 historical closed package-pair, 04224 및 이전 historical predecessor를 함께 가리킨다.
- Zone wiki canonical path `/data/projects/codex-zone/wiki/index.md`는 이 workspace에 없어 별도 파일 수정 대상이 없었다.

## 패키징 진입점

- Product wrapper: `packaging/windows-desktop-node/README.md`
- Installer: `packaging/windows-desktop-node/installer/README.md`
- Product module: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Installer build: `packaging/windows-desktop-node/installer/build.ps1`

## 현재 Required CI 검증

아래 exact-four shard는 clean committed HEAD에서만 실행한다. 먼저 `git status --short`가
아무것도 출력하지 않는지 확인하고, `--no-build --no-restore` 실행 전에 다음 prerequisite를
완료한다.

```cmd
git status --short
dotnet restore src\DesktopNode.sln
dotnet build src\DesktopNode.sln -c Release --no-restore
npm ci --prefix web
dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-gates-dotnet --shard dotnet
dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/development-gates-web --shard web
dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/development-gates-delivery --shard delivery
dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/development-gates-installer-policy --shard installer-policy
git diff --check
```

Final-main run `32904006595`가 위 네 shard를 PASS했다. Pester/PowerShell 명령은
local/manual compatibility 또는 non-required Public Boundary residue에만 남고 provider-required
command가 아니다. `web` shard는 verification catalog를 통해 이미
`npm run test:required --prefix web`를 실행하므로 이를 별도 Required 단계로 중복 실행하지 않는다.
