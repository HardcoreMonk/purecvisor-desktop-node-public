# PureCVisor Desktop Node Installer

## 2026-07-14 현재 payload 경계

ADR-0011에 따라 새 MSI의 활성 운영자 payload는 Web Console과 `pcvcli.exe`다. TUI
payload는 제거됐고 upgrade는 이전 설치본의 잔여 실행 파일을 정리해야 한다. Package
manifest schema는 `2`를 유지한다. Code-level evidence는
`docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`다.
`0.42.62-admin-smoke` 설치본 current-card는 당시 TUI 포함 사실을 보존하는 dated
predecessor이며, `0.42.63-admin-smoke` MSI upgrade cleanup/package/fullgate/CLI-Web
installed current-card는 아직 pending이다.

## 2026-05-29 historical predecessor

최신 operational installer anchor는 `0.42.59-admin-smoke` /
`full-admin-host-mutation-gate-20260529-04259`이며 package evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`,
manual-admin closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`가
소유한다. Package MSI SHA-256은
`6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`, operational MSI
SHA-256은 `dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`, payload aggregate
SHA-256은 `3f015e7743efac3b61de81962c236a03c1bcf882053fc92fd3c525da280a1687`다.

최신 설치본 package는 `0.42.59-admin-smoke`이며
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`가
Web/TUI/CLI current-card smoke PASS를 기록한다. 04250→04254 manual-admin readiness는 현재
host baseline mismatch로 blocked다. 최신 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`이고
installed current-card payload 후보는 이미 열린 `0.42.60-admin-smoke`를 유지한다. docs-maintenance
postpush만으로 추가 package 후보를 열지 않는다. Public trusted signing 또는 외부 stable
publication evidence가 아니며, 아래 이전 날짜 current 문단은 historical predecessor로
해석한다.
직전 `0.42.58-admin-smoke` predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.

## 2026-05-21 historical predecessor

최신 operational installer anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`이며 package evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`,
manual-admin closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md`
가 소유한다. Operational MSI SHA-256은
`eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41`, update ZIP SHA-256은
`96599dc4493e26e8cf467e19fabc5ab20306166896c1139bdbeb52566623ab25`다.

최신 설치본 package는 `0.42.40-admin-smoke`이며
`docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`,
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`가
PCVCLI Hyper-V QoS/guest-service parity와 Web/TUI/CLI current-card smoke를 기록한다.
Actual VM Web/TUI QoS/guest readback smoke는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`가
기록하며, 설치본 TUI row projection blocker는 `0.42.41-admin-smoke` package chain trigger로
남겼다.
Public trusted signing 또는 외부 stable publication evidence가 아니며, 아래 이전 날짜
current 문단은 historical predecessor로 해석한다.

## 2026-05-18 현재 기준

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

PR #156 post-merge public-boundary main push는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`, run `26017721669`, job `76471545641`, head `a4509c552c003ee0fc87b54b26529686e6dfeb84`에서 PASS했고 historical public-boundary anchor로 보존한다. PR #155, PR #154, PR #152 public-boundary evidence도 historical predecessor로 보존한다. PR #153 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`, run `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`로 보존한다. 이후 사용자 승인으로 0.42.30 package chain을 열어 `full-admin-host-mutation-gate-20260518-04230`과 `manual-admin-campaign-descriptor-20260518-04229-04230-closed`를 current installed/package anchor로 승격했다.

Historical PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Operator Surface package-pair predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` / `manual-admin-campaign-descriptor-20260517-04227-04228-closed`로 보존한다. Full admin host mutation batch는 `full-admin-host-mutation-gate-20260517-04228`, target MSI SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, clean package MSI SHA-256은 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`, update ZIP SHA-256은 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`이다.

이 디렉터리는 Desktop Node Phase 14 WiX MSI-first installer 경계를 담는다.

ADR-0004/ADR-0006 기준으로 이 installer는 내부 사설망 전용 서비스 제품 런타임의 MSI installer다. Public trusted signing, trusted timestamp, 외부 stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke, 일반 사용자 대상 public release는 `out-of-scope`다. 현재 배포 gate는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`에서 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke, Lifecycle/Packaging current rebaseline으로 추적한다. HTTPS/TLS installed smoke는 `internal-https-tls-lifecycle-installed-2026-05-10-0397` PASS, clean-host install/update/rollback은 `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417` PASS, Lifecycle/Packaging current rebaseline은 `lifecycle-packaging-rebaseline-2026-05-10-0415-0416` PASS 상태다.

Release evidence 요약:

- 2026-05-17: `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Manual admin
  package-pair closure를 기록했다. Evidence는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md`,
  campaign root는 `artifacts/manual-admin-campaign-20260517-04227-04228`이다.
  Installed update/rollback, Windows Update `KB5087545` 포함 dedicated clean-host,
  Burn lifecycle, MSIX lifecycle, installed runtime ops summary, descriptor generation
  v2, installed current-card recheck, account/noVNC package-pair smoke가 모두 PASS다.
  Target operational MSI SHA-256은
  `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, update ZIP
  SHA-256은 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`,
  provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`이다. Descriptor
  `manual-admin-campaign-descriptor-20260517-04227-04228-closed`는
  `missing_count=0`, `not_pass_count=0`, `overall_status=pass`다.

- 2026-05-17: `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle
  historical predecessor를 보존한다. Evidence는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`,
  descriptor는 `manual-admin-campaign-descriptor-20260517-04226-04227-closed`다.
  Host Ops lifecycle descriptor bridge `host-ops-lifecycle-descriptor-bridge-v1`와
  `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
  bucket contract를 유지하며 public trusted signing과 외부 stable publication
  evidence가 아니다.

- 2026-05-17: `0.42.25-admin-smoke -> 0.42.26-admin-smoke` Manual admin
  package-pair closure를 기록했다. Evidence는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`,
  campaign root는 `artifacts/manual-admin-campaign-20260517-04225-04226`이다.
  Installed update/rollback, Windows Update 포함 dedicated clean-host, Burn lifecycle,
  MSIX lifecycle, installed runtime ops summary, descriptor generation v2가 모두 PASS다.
  Target operational MSI SHA-256은
  `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP
  SHA-256은 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`,
  provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`이다. Descriptor
  `manual-admin-campaign-descriptor-20260517-04225-04226-closed`는
  `missing_count=0`, `not_pass_count=0`, `overall_status=pass`다.
- 2026-05-16: `0.42.26-admin-smoke` package build, `0.42.25-admin-smoke -> 0.42.26-admin-smoke`
  Manual admin descriptor/readiness, full admin host mutation gate, installed
  Web/TUI/CLI current-card, PR #145 post-merge public-boundary evidence를 기록했다.
  Package evidence는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04226.md`,
  artifact `artifacts/admin-smoke-package-20260516-04226`, MSI SHA-256
  `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`다. Full gate
  evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`,
  batch `full-admin-host-mutation-gate-20260516-04226`, route/OS artifact
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226`와
  `artifacts/os-mutation-gates-batch-profile-20260516-04226`가 소유한다. 최신 operational
  MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`,
  provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`다. Current-card evidence는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`,
  artifact `artifacts/installed-operator-surface-current-card-20260516-04226`이며
  `runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge route detail count
  `4`, Web `200`, TUI/CLI PASS를 확인했다. 04225→04226 descriptor evidence는
  `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`이며
  readiness PASS, `missing_count=4`, `not_pass_count=1`인 initial blocked candidate로
  보존한다. 이 package-pair는 2026-05-17 closure evidence에서 PASS로 승격됐다. PR #145 public-boundary evidence는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md`,
  run `25961834812`, job `76318357776`, head
  `d6500c01c972cbc7ca1e290e51120181ceea1501`이다.
- 2026-05-16: `0.42.25-admin-smoke` full admin host mutation gate, installed
  Web/TUI/CLI current-card, `0.42.24-admin-smoke -> 0.42.25-admin-smoke` Manual admin
  package-pair closure, PR #144 post-merge public-boundary evidence를 기록했다. Full gate
  evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04225-hostmutation.md`,
  batch `full-admin-host-mutation-gate-20260516-04225`, route/OS artifact
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04225`와
  `artifacts/os-mutation-gates-batch-profile-20260516-04225`가 소유한다. 최신 operational
  MSI SHA-256은 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`,
  provenance commit은 `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`다. Current-card evidence는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04225.md`,
  artifact `artifacts/installed-operator-surface-current-card-20260516-04225`이며
  `runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge route detail count
  `4`, Web `200`, TUI/CLI PASS를 확인했다. Manual admin campaign evidence는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`이고 descriptor
  `manual-admin-campaign-descriptor-20260516-04224-04225-closed`는 `missing_count=0`,
  `not_pass_count=0`, `overall_status=pass`다. Public-boundary evidence는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md`,
  run `25959505688`, job `76312299500`, head
  `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`이다. Earlier package build record
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md`는 MSI SHA-256
  `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`, provenance commit
  `403d4474c4b88136774600cc81ca2d941c0b5e4b`로 historical package candidate로 보존한다.
- 2026-05-16: `0.42.24-admin-smoke` Runtime/API current evidence rollup package build,
  full admin host mutation gate, installed Web/TUI/CLI current-card를 기록했다.
  Package evidence는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`,
  `artifacts/admin-smoke-package-20260516-04224`이며 MSI SHA-256은
  `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, provenance
  commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`다. Full gate evidence는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`,
  batch `full-admin-host-mutation-gate-20260516-04224`, full-gate MSI SHA-256
  `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`다. Installed
  current-card evidence는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`,
  artifact `artifacts/installed-operator-surface-current-card-20260516-04224`이며
  `runtime-api-current-evidence-rollup-v1`과 Runtime/API registry bridge route detail
  count `4`를 확인했다. 04223→04224 Manual admin descriptor는
  `blocked-by-missing-evidence`로 생성됐으며 닫힌 package-pair PASS가 아니다. 이 04224
  fullgate/current-card는 04226 closure 이후 historical predecessor로 보존한다.
- 2026-05-16: `0.42.23-admin-smoke` package build와 `0.42.22-admin-smoke -> 0.42.23-admin-smoke` manual-admin package-pair campaign을 기록했다. Package evidence는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04223.md`, `artifacts/admin-smoke-package-20260516-04223`이며 MSI SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`다. Campaign evidence는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`, update ZIP SHA-256 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`, descriptor `manual-admin-campaign-descriptor-20260516-04222-04223-closed`, `missing_count=0`, `not_pass_count=0`이다.
- 2026-05-16: `0.42.23-admin-smoke` full admin host mutation gate와 installed Web/TUI/CLI current-card를 기록했다. Full gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`, batch `full-admin-host-mutation-gate-20260516-04223`, full-gate MSI SHA-256 `ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932`, full-gate provenance commit `d11a096086326004f27facd9612c2296ded15a4b`다. Installed Web/TUI/CLI current-card evidence는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md`, artifact `artifacts/installed-operator-surface-current-card-20260516-04223`이며 Runtime/API registry bridge route detail count `4`를 확인했다. Public-boundary post-merge는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md`, run `25954744127`, job `76299282407`에서 PASS했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-16: `0.42.22-admin-smoke` package build와 full admin host mutation gate를 기록했다. Package evidence는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`, `artifacts/admin-smoke-package-20260516-04222`이며 MSI SHA-256은 `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`, provenance commit은 `8a38995cc25a888f64473e9a2869740949ad6b24`다. Full gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`, batch `full-admin-host-mutation-gate-20260516-04222`, full-gate MSI SHA-256 `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`다. Installed Web/TUI/CLI current-card evidence는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md`, artifact `artifacts/installed-operator-surface-current-card-20260516-04222`이며 Runtime/API registry bridge route detail count `4`를 확인했다. `0.42.21-admin-smoke -> 0.42.22-admin-smoke`는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`에서 Burn idempotence blocker로 보존한다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-04-30: `0.23.8-rc.1` signed RC MSI smoke와 elevated MSI lifecycle smoke를 수집했다.
- 2026-05-01: current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` MSI lifecycle, product-wrapper update/rollback/config migration, final MSI restore install evidence를 기록했다.
- `0.23.9-rc.1` evidence는 local test signing certificate 기준이므로 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-01: ADR-0003에 따라 `0.23.10-rc.1` internal enterprise `RequireSigned` MSI를 빌드했다.
- 같은 run에서 internal Root/leaf trust import, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS evidence를 `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`에 기록했다.
- 2026-05-06: `0.38.4-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`을 `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`에 기록했다. MSI SHA-256은 `0b4c60d60098f89bd0adea4d183a5224d32b862e9bf69bd6dbaa41077377e8b9`, provenance commit은 `6bbb39f0a3a271e4a1187ce7de2014e009977425`, signing trust model은 `InternalEnterprise`다.
- 2026-05-07: `0.38.7-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`을 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`에 기록했다. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`다.
- 같은 non-elevated shell의 `0.38.7-admin-smoke` full admin host mutation gate와 update/rollback mutation attempt는 각각 `PCV_BATCH_ADMIN_REQUIRED`, `sc.exe stop PureCVisorDesktopNode` exit `5`, `PCV_PRODUCT_SERVICE_STOP_TIMEOUT`으로 차단됐고 host mutation은 수행하지 않았다.
- 2026-05-07: elevated `0.38.8-admin-smoke` installed destructive update/rollback smoke를 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`에서 PASS로 기록했다. MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`, provenance commit은 `fd4f854646fc159d54f7578230f00c51f80e201f`이다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 `0.38.6-admin-smoke`로 복원하며 `DesktopNode.failed` diagnostics root를 보존했다. Final service는 `Running`, boot time unchanged, `host_mutation_performed=true`다.
- 2026-05-07: installer build output에 `PureCVisorDesktopNode-<version>-windows-x64.publication.json` descriptor sidecar를 추가했다. Evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`이며, descriptor는 public trusted signing/external stable publication을 `not-claimed`, Burn/MSIX/winget/catalog publication을 미실행 상태로 기록한다.
- 2026-05-10: internal MSIX package lifecycle smoke를 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/msix-package-lifecycle-smoke-20260510-0416`에 기록했다. `PureCVisor.DesktopNode.MsixSmoke` package identity와 `PureCVisorDesktopNodeMsixSmoke` packaged service로 build/sign/verify, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence가 PASS였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-09: `0.39.1-admin-smoke` MSI/update package apply를 `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`, `artifacts/msi-update-package-20260509-0391`에 기록했다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`이다. Elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`이었다. 이 evidence는 AllowUnsignedDev internal admin-smoke evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-09: `0.39.1-admin-smoke` full admin host mutation gate를 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-rerun.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-032525-0391-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-032525-0391-rerun`, `artifacts/os-mutation-gates-batch-profile-20260509-032525-0391-rerun`에 기록했다. MSI SHA-256은 `25a88e41ed926a6bccaf3eba1fdd44d0976091aca9fd6ef77f52eea2bddf3c37`, provenance commit은 `0815a6281bcb98b5b1795e8d054073e1c9fb4892`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store gate가 PASS였고 final service는 `Running`, firewall final count `0`, Event Log source absent, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10: Burn bootstrapper lifecycle smoke를 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416`에 기록했다. Bundle SHA-256은 `5e67bd3a1fed7262447531000328825180fd678b252170793cf88e50fc41535d`이며 install/repair/remove exit `0`, direct MSI restore, final service `Running`을 확인했다. 이 evidence는 internal AllowUnsignedDev smoke이며 public trusted signing, timestamping, external stable publication, winget submission, clean-host public signed update/rollback evidence가 아니다.
- 2026-05-10: Web/API port split evidence는 `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`와 `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`에 기록했다. Installer/product service plan 기본값은 Web Console `http://127.0.0.1:80/`, Web API `http://127.0.0.1:7777/`이며 installed listener smoke는 `artifacts/installed-port-split-20260510-010714-0392`에서 service `PathName` `--web-prefix "http://127.0.0.1:80/"`, Web `200`, API `200`, Web-port API rejection, CORS preflight `204`를 PASS로 확인했다. Port 80 browser QA는 `artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80`에 dashboard/jobs/network/troubleshooting/diagnostics/responsive screenshots를 기록했다. HTTPS/443 binding은 아직 주장하지 않는다.
- 2026-05-13: `0.42.11-admin-smoke` product wrapper native repair package와 full admin host mutation gate를 기록했다. Package evidence는 `docs/ga-ready/evidence/product-wrapper-native-repair-package-2026-05-13-04211.md`, `artifacts/admin-smoke-package-20260513-04211`이며 MSI SHA-256은 `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`, provenance commit은 `987beb51025a5aa926df7d9a905019b4d6d29705`다. Full gate evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md`, batch `full-admin-host-mutation-gate-20260513-0429-04211`, full-gate MSI SHA-256 `902e175cd6354843da2c928e2b6772f04d40240f02783e4edfed460ba0f9fce2`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13: `0.42.9-admin-smoke` product payload package build를 `docs/ga-ready/evidence/batch-evidence-root-service-action-package-2026-05-13-0429.md`, `artifacts/admin-smoke-package-20260513-0429`에 기록했다. MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`이다. MSI `EventLogDefaultTransition`/`EventLogDefaultTransitionRepair` custom action은 `--eventlog-default-transition-timeout-seconds 60`을 전달한다. 0429 full host mutation current claim은 별도 full gate evidence가 소유하며, 이 build는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- Internal evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.

## 책임 경계

- Phase 14 결정은 `DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first`다.
- MSI는 `C:\Program Files\PureCVisor\DesktopNode` 파일 설치, repair, 제거를 소유한다.
- `DesktopNode.Host.exe` service-action runner는 MSI installed action의 protected token file 준비와 service create/config/start/stop/delete를 native SCM controller로 소유한다.
- Product wrapper는 standalone 관리자 smoke와 diagnostics/update/rollback 경로에서 protected token file 준비, health check, diagnostics 같은 service/data configuration을 소유한다.
- 기존 standalone `Install`/`Uninstall` product action은 개발자 CLI와 관리자 smoke용으로 유지한다.
- MSI custom action은 설치된 payload의 `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed`를 호출하고, `REMOVE_DATA=1` uninstall에서만 service 제거 뒤 `DesktopNode.Host.exe service-action data-root-remove --remove-data`를 추가 호출한다.

## 설치 후 사용

MSI 설치 후 제품은 `PureCVisorDesktopNode` Windows service와 loopback Web Console로 사용한다. 최신 기본값은 Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...` 분리다.

```powershell
Start-Process "http://127.0.0.1/"
Get-Service PureCVisorDesktopNode
```

설치본 상태와 bearer-protected runtime policy는 product wrapper status action으로 확인한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

기본 설치는 LAN을 열지 않는다. LAN exposure, firewall rule, trust-store 변경은 관리자 opt-in gate에서만 실행하고 rollback/final-state proof를 남긴다.

설치 후 일반 사용자 절차는 `docs/USER_GUIDE.md`를 따른다.

## 데이터 보존

기본 uninstall은 `%ProgramData%\PureCVisor\desktop-node`를 보존한다.

`REMOVE_DATA=1` uninstall만 다음 ProgramData 항목을 삭제한다. 이때 `remove-installed --remove-data`는 삭제 handoff descriptor만 만들고, 실제 삭제는 service absent precondition을 확인하는 별도 `data-root-remove --remove-data` action에서 수행한다.

- protected token file: `api-token.dpapi.json`
- legacy raw token file: `api-token.txt`
- job store: `jobs.json`
- event log: `events.jsonl`
- install log: `install.jsonl`
- diagnostics directory: `diagnostics\`

Service host log directory는 Phase 14 기본 remove-data 대상이 아니다. Phase 16 기준 장기 로그 정책은 JSONL first로 확정됐고, Windows Event Log source 등록은 관리자 opt-in 계획으로만 제공한다.

## Build toolchain

현재 검증된 개발 toolchain은 `.NET SDK 10.0.203`과 WiX CLI `5.0.2`다. WiX CLI는 다음처럼 설치할 수 있다.

```powershell
dotnet tool install --global wix --version 5.0.2
```

새 shell에서 `wix`가 PATH에 잡히지 않으면 `-WixPath "$env:USERPROFILE\.dotnet\tools\wix.exe"`를 `build.ps1`에 명시한다.

- Phase 14 build script는 WiX source 파일인 `Product.wxs`와 `ProductActions.wxs`를 `wix build`에 전달한다.
- `.wixproj`는 source include와 IDE/MSBuild 보조 경계이며, 제품 build 경로의 주 입력이 아니다.

## 개발자 dry-run

`-Version`은 artifact 파일명과 provenance의 release/display version으로 유지한다.

- MSI `ProductVersion`은 숫자형 `major.minor.patch`만 허용하므로 기본값은 `-Version`의 선행 semver에서 파생한다.
- 예: `0.14.0-dev`는 MSI `ProductVersion=0.14.0`으로 빌드한다.
- 필요한 경우 `-MsiProductVersion 0.14.0`으로 명시할 수 있다.

Phase 22 release/version policy 기준으로 installer build output은 다음 naming을 사용한다.

- MSI: `PureCVisorDesktopNode-<version>-windows-x64.msi`
- Provenance: `PureCVisorDesktopNode-<version>-windows-x64.provenance.json`
- MSI hash sidecar: `PureCVisorDesktopNode-<version>-windows-x64.msi.sha256`
- Publication descriptor: `PureCVisorDesktopNode-<version>-windows-x64.publication.json`

`build.ps1`는 `-Version` suffix에서 `release_channel`을 판별해 build plan과 provenance에 기록한다. `dev`와 `admin-smoke`는 `AllowUnsignedDev`를 허용하지만, `rc`와 `stable` version은 `RequireSigned` signing mode만 허용한다.

Publication descriptor는 artifact base name, architecture, MSI path/SHA-256, provenance path, signing mode/trust model을 기록한다. Publication boundary는 `internal-artifact-descriptor-only`이고 public trusted signing, 외부 stable publication, Burn bootstrapper, MSIX, winget manifest, catalog publication은 완료로 주장하지 않는다.

MSIX package lifecycle smoke는 MSI publication descriptor 상태를 public publication으로 승격하지 않는다. `0.41.5-admin-smoke` baseline payload and `0.41.6-admin-smoke` target payload 기반 smoke package는 internal Root/leaf 서명과 restricted service capability(`runFullTrust`, `packagedServices`, `localSystemServices`)로만 build/install/update/remove를 확인하며, external stable publication은 계속 `not-claimed`다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.23.8-dev `
  -DesktopNodeHostPath '<DesktopNode.Host.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode AllowUnsignedDev `
  -DryRun
```

## 개발자 unsigned MSI build

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.23.8-dev `
  -DesktopNodeHostPath '<DesktopNode.Host.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode AllowUnsignedDev
```

WiX CLI가 없는 환경에서는 실제 MSI build가 `PCV_INSTALLER_WIX_NOT_FOUND`로 중단될 수 있다. 이 경우 기본 Pester static/dry-run suite 통과와 WiX 미설치 사유를 검증 결과에 기록한다.

검증된 개발 환경에서는 explicit `DesktopNode.Host.exe` 입력과 `AllowUnsignedDev` signing mode로 unsigned MSI와 provenance manifest 생성을 확인했다.

Build script가 host path를 받지 않으면 `DesktopNode.Host`를 self-contained single-file exe로 publish해 payload에 staging한다. Clean-host smoke는 .NET runtime이 없는 Windows Server VM에서도 service가 시작되어야 하므로 기본 package build는 framework-dependent payload를 만들지 않는다.

Repo migration follow-up 이후 MSI payload staging은 product wrapper, `DesktopNode.Host.exe`, repo-root `web/**`, `product-manifest.json`만 포함한다. Legacy API/Hyper-V/service component files는 MSI payload input이 아니다.

최근 MSI 검증 요약:

- `0.23.8-rc.1`, `0.23.9-rc.1`: local test certificate 기준 `RequireSigned` MSI lifecycle PASS. Public trust chain 검증은 닫지 않았다.
- `0.23.10-rc.1`: internal Root/leaf signer와 `signing_trust_model = InternalEnterprise` 기준 build/검증/lifecycle PASS.
- `0.26.0-admin-smoke`: .NET service host replacement 경로의 direct service-action, MSI lifecycle, Hyper-V helper integration smoke PASS.
- `0.26.6-admin-smoke`: route parity mutation runner 기준 service-action, MSI lifecycle, 설치본 .NET Host Hyper-V API route smoke PASS.
- `0.26.8-admin-smoke`: C# native-first/helper-fallback `network.inventory` read route 포함 PASS.
- `0.26.9-admin-smoke`: native topology parity fallback, MSI repair service 재생성 guard, request processor 직렬화 보강 포함 PASS.
- `0.27.1-admin-smoke`: native `host.status` C# registry/WMI/service/admin adapter와 `native_core.reason=host.status,network.inventory` 포함 PASS.
- `0.27.6-admin-smoke`: runtime policy dispatch boundary contract 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS. MSI SHA-256은 `4485fc3aba902d38a5d1293e9231497ae5f35b4c0730d1815c8df561a67c009c`이며 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `0.28.3-admin-smoke`: checkpoint create/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS. Installed runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.delete]`를 보고했고 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `0.28.6-admin-smoke`: checkpoint create/restore/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS. Installed runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`를 보고했고 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다. Restore smoke는 `vm.poweroff-before-restore` 최소 안정 조건으로 실행했다.
- `0.29.0-admin-smoke`: VM create/start/restart/poweroff와 checkpoint create/restore/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-vm-create-restart-shutdown-20260503-0290`이며 final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `0.30.1-admin-smoke`: VM delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301`이며 managed delete `action=delete`, repeat delete `action=absent`, unmanaged guard `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `0.30.3-admin-smoke`: service/data-root handoff, MSI `REMOVE_DATA=1`, installed Hyper-V API route smoke PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`이며 service 존재 중 `data-root-remove --remove-data` 차단, `remove-installed --remove-data` handoff-only, service absent 이후 allowlist data-root 삭제, non-allowlist log 보존, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `0.33.0-admin-smoke`: Service/MSI/Hyper-V mutation과 row-isolated firewall/trust-store mutation PASS. Artifact는 `artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330`이며 current native owner migration 전 historical firewall/trust-store actual mutation evidence로만 사용한다.
- `0.34.1-admin-smoke`: current native MSI/firewall/LAN/internal trust-store gate PASS. Artifact는 `artifacts/os-mutation-gates-20260505-003459-0341`이며 MSI provenance commit `6f97a24aa2bdfacf33d7bd987559eb85e363e119`, MSI SHA-256 `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`, payload file count 7이다. MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, native firewall rule enable/remove, LAN IP runtime policy `HTTP 200`, internal Root/TrustedPublisher install/remove/restore를 확인했다. Follow-up commit `49a06acd3493066a10ec26fe541d5d8be1005c2b`는 Windows Firewall missing-rule lookup hardening이다.
- `0.35.4-admin-smoke`: 실행 당시 HEAD native MSI/firewall/LAN/internal trust-store gate fresh PASS. Artifact는 `artifacts/os-mutation-gates-20260505-033503-0354`이며 MSI provenance commit `744a15536569e89f948927bea9179fc0eeae3ff4`, MSI SHA-256 `bf7d0d2bd83545e83fbdf0dfb96b715f8e09471474445ae1c0db1d076be2c1e4`다. MSI install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall 후 internal signed stable `0.35.2` MSI로 final restore했고, firewall owned rule enable/remove final absent, LAN IP runtime policy/Web root `HTTP 200`, internal Root/TrustedPublisher install/remove/restore final present, final service loopback `Running`, installed DisplayVersion `0.35.2`, boot time unchanged를 확인했다.
- `0.35.5-admin-smoke`: 실행 당시 HEAD native Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate fresh PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`와 `artifacts/os-mutation-gates-20260505-101659-0355-final`이며 MSI provenance commit `2fb38f20a8c74433684345ded8a33ba16a863621`, MSI SHA-256 `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, firewall owned rule enable/remove final absent, LAN IP runtime policy/Web assets `HTTP 200`, internal Root/TrustedPublisher install/remove/restore final present, final service loopback `Running`, installed DisplayVersion `0.35.5`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음을 확인했다.
- `0.35.6-admin-smoke`: 실행 당시 code HEAD native Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate fresh PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun`와 `artifacts/os-mutation-gates-20260505-170454-0356-rerun`이며 MSI provenance commit `cc723e28ed62f6f1c5e49c74ca68b87d0f1b8b3a`, MSI SHA-256 `a24de44049519dea8405854a17272ebb362b061ff03a051cd61fb31669bc7d02`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, firewall owned rule enable/remove final absent, LAN IP runtime policy/Web assets `HTTP 200`, internal Root/TrustedPublisher install/remove/restore final present, final service loopback `Running`, installed DisplayVersion `0.35.6`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음을 확인했다.
- `0.35.7-admin-smoke`: 현재 HEAD native Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate fresh PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이며 MSI provenance commit `2ec9e71d45b702e106824c86500cd6152b18fab7`, MSI SHA-256 `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, firewall owned rule enable/remove final absent, LAN IP bearer runtime policy/Web assets `HTTP 200`, config-migration-apply blocked/no-mutation descriptor, internal Root/TrustedPublisher install/remove/restore final present, final service loopback `Running`, installed DisplayVersion `0.35.7`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음을 확인했다.
- `0.38.4-rc.1`: internal enterprise `RequireSigned` MSI build PASS. Artifact는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`이며 MSI SHA-256은 `0b4c60d60098f89bd0adea4d183a5224d32b862e9bf69bd6dbaa41077377e8b9`, provenance commit은 `6bbb39f0a3a271e4a1187ce7de2014e009977425`, signing trust model은 `InternalEnterprise`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.38.7-rc.1`: internal enterprise `RequireSigned` MSI build PASS. Artifact는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`이며 MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.36.0-admin-smoke`: active product .NET 100% cleanup 후속 Service/MSI/Hyper-V route parity rerun PASS. Artifact는 `artifacts/routeparity-service-msi-hyperv-dotnet100-20260505-0.36.0`이며 MSI provenance commit `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256 `70cb8b720588c6ef69aca59fed48f870865d7bca8c7a4ea8e623ab6b6e99d048`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service loopback `Running`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다.
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, step `timed_out=false`, `exit_code=0`, heartbeat lines `25`다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`, signing mode는 `AllowUnsignedDev`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.37.0-admin-smoke` MSI lifecycle evidence는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`이다. 최종 resume run은 install, repair, uninstall preserve, install-remove-data, uninstall-remove-data, final restore exit `0`을 기록했고 MSI SHA-256은 `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`다. 첫 attempt repair `1603`은 direct `repair-installed`, manual MSI repair, Batch Supervisor `-Resume`이 모두 성공했기 때문에 recovered transient evidence로 보존한다. Full gate artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`와 `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`다.
- `0.38.9-admin-smoke` MSI lifecycle evidence는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389`에 historical evidence로 남는다. 최종 run은 install, repair, uninstall preserve, install-remove-data, uninstall-remove-data, final restore exit `0`을 기록했고 MSI SHA-256은 `86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3`, MSI provenance commit은 `159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce`, signing mode는 `AllowUnsignedDev`다. Batch Supervisor full gate artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`와 `artifacts/os-mutation-gates-batch-profile-20260508-202255-0389`이며, Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store gate가 PASS했다. Final service는 loopback-only `Running`, product manifest version은 `0.38.9-admin-smoke`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.39.0-admin-smoke` MSI/service installed listener rerun evidence는 `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`이다. MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, signing mode는 `AllowUnsignedDev`다. Batch Supervisor artifact는 `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`이고 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false다. Final service는 loopback-only `Running`, product manifest version은 `0.39.0-admin-smoke`, SCM `PathName`은 `--diagnostics-root`, protected token file, route timeout/request-limit/burst/retry-after 인자를 포함한다. Installed diagnostic bundle listener create/download는 POST `201`, GET `200`, redaction PASS였다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.39.1-admin-smoke` MSI/update package apply evidence는 `artifacts/msi-update-package-20260509-0391`이다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, MSI provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, signing mode는 `AllowUnsignedDev`다. Elevated apply command completed with MSI exit `0`; final service is `Running`, product manifest version is `0.39.1-admin-smoke`, and loopback Web Console returned HTTP `200`. Firewall/trust-store/LAN/Event Log OS gate and diagnostic bundle installed listener create/download are not part of this apply evidence, and this evidence is not public trusted signing or external stable publication evidence.
- 2026-05-13 `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`은 `0.42.9-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`이고 full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260513-040213-0429`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.9-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `batch-evidence-root-service-action-package-2026-05-13-0429`은 `0.42.9-admin-smoke` 이전 product payload package build evidence다. Artifact는 `artifacts/admin-smoke-package-20260513-0429`, MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Installer Event Log default transition timeout propagation을 포함하며, 0429 full host mutation current claim은 별도 full gate evidence가 소유한다. 다음 package-pair 후보는 `0.42.8-admin-smoke -> 0.42.9-admin-smoke`이며 현재 installed update/rollback만 PASS다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `manual-admin-operator-hardening-followup-2026-05-10-0415`는 `artifacts/manual-admin-followup-20260510-0415`에서 installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition을 `0.41.5-admin-smoke` 기준 PASS로 재확인했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.39.0-admin-smoke` installed listener 후속 OS mutation gate evidence는 `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`이다. Batch Supervisor `OsMutationGate` summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false였고 firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service는 `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`이며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `0.38.7-admin-smoke` full admin host mutation gate attempt는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260507-0387`에서 non-elevated shell preflight `PCV_BATCH_ADMIN_REQUIRED`로 차단됐고 Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store step은 시작하지 않았다. `artifacts/product-update-rollback-mutation-20260507-0387` update/rollback mutation attempt도 update `sc.exe stop PureCVisorDesktopNode` exit `5`, rollback `PCV_PRODUCT_SERVICE_STOP_TIMEOUT`, `host_mutation_performed=false`로 끝났다. Product root manifest는 `0.38.6-admin-smoke`, previous root는 absent, final service는 `Running`이다. 이 evidence는 PASS evidence가 아니다.
- `0.38.8-admin-smoke` installed destructive update/rollback smoke는 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`에서 elevated PASS했다. MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`, provenance commit은 `fd4f854646fc159d54f7578230f00c51f80e201f`, signing mode는 `AllowUnsignedDev`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 current root를 `0.38.6-admin-smoke`로 복원하고 `0.38.8-admin-smoke` root를 `DesktopNode.failed` diagnostics로 보존했다. Final service는 `Running`, boot time unchanged, `host_mutation_performed=true`다. 최초 `artifacts/product-update-rollback-mutation-20260507-0388` non-elevated attempt는 blocked history이며 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 현재 route parity MSI lifecycle smoke는 각 MSI step 후 `msi-lifecycle-smoke.json`을 partial evidence로 저장한다. Repair `1603`은 `RepairInstalled returned actual error code -1073741510`, `MsiSystemRebootPending = 1` 등 좁은 log marker가 있을 때만 `msi-repair-retryable-transient`로 분류하며, 이 분류는 Batch Supervisor retry/resume 권고일 뿐 MSI 성공 판정이 아니다.
- Installer-ISO VM의 `vm.shutdown`은 guest shutdown integration 미준비 상태를 `PCV_VM_SHUTDOWN_NOT_AVAILABLE` structured failure로 반환함을 확인했다.
- Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD 기반 Gen1 differencing VM으로 확인했다. Installed Local API `vm.shutdown` job은 `succeeded`, final VM state는 `Off`, smoke VM/ProgramData cleanup은 완료 상태다.

각 상세 artifact 경로, commit, SHA-256은 아래 관리자 smoke evidence 목록을 따른다.

## Phase 16 diagnostics boundary

MSI는 Phase 16에서도 Windows Event Log provider/source를 기본 등록하지 않는다. Product wrapper는 다음 경계를 유지한다.

- `events.jsonl`과 `install.jsonl`은 1차 운영 로그다.
- Service host logs는 `%ProgramData%\PureCVisor\desktop-node\service-logs`에 보존한다.
- Diagnostic bundle은 `diagnostics-manifest.json`과 redacted artifact 목록을 포함한다.
- Windows Event Log source `PureCVisor Desktop Node`는 등록 계획 object와 관리자 opt-in smoke로만 다룬다.
- MSI WiX source는 `%ProgramData%\PureCVisor\desktop-node` 경로 계산만 담당하고 data-root ACL을 직접 소유하지 않는다. Sensitive token file ACL과 `REMOVE_DATA=1` 삭제 전 ACL repair는 product wrapper/service token helper 계약으로 검증한다.

## Release signing 입력

Release build는 `RequireSigned`를 사용한다. Code signing 인증서, private key, PFX password, API token 값은 repo와 provenance manifest에 기록하지 않는다. `RequireSigned` build는 `-SigningTrustModel`을 명시해야 한다.

허용되는 trust model:

- `LocalTest`: 개발자/test host workaround. 운영 release evidence가 아니다.
- `InternalEnterprise`: ADR-0003 internal Root/leaf signing. 현재 내부 서비스 운영 release evidence 기본값이다.
- `PublicTrusted`: 현재 내부 전용 서비스 scope 밖이다. 외부 배포를 별도 ADR로 채택하고 public CA 또는 Azure Trusted Signing 같은 공개 신뢰 체인을 실제로 사용할 때만 쓴다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.23.8-rc.1 `
  -DesktopNodeHostPath '<DesktopNode.Host.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode RequireSigned `
  -SigningTrustModel InternalEnterprise `
  -SignToolPath '<signtool.exe>' `
  -CertificateThumbprint '<thumbprint>' `
  -TimestampUrl '<timestamp-url>'
```

`-CertificatePath`를 사용할 수는 있지만, certificate file과 password는 외부 secret 입력으로만 관리한다.

WiX/SignTool 실행 실패는 stdout/stderr를 `tool_output` JSON 필드로 캡처한다. 인증서 private key, PFX password, certificate path/thumbprint, API token 값은 repo와 provenance manifest에 기록하지 않는다.

## 내부 signing trust bootstrap

AD CS/Intune/MDM이 없는 내부 서비스 운영에서는 `New-PcvInternalCodeSigningTrust.ps1`로 전용 internal Root CA와 leaf Code Signing 인증서를 준비할 수 있다. 이 스크립트는 private key/PFX를 export하지 않고, public `.cer`와 JSON summary만 evidence에 남긴다.

## Internal RequireSigned gate runbook

Internal `RequireSigned` release gate는 세 단계로 분리한다. 첫 단계는 non-mutating plan 확인이고, 두 번째는 관리자 opt-in trust bootstrap이며, 세 번째는 signed MSI build와 별도 elevated lifecycle smoke다. Dry-run은 LocalMachine trust import를 실행하지 않는다.

- Plan-only check: `New-PcvInternalCodeSigningTrust.ps1 -DryRun`
- Build mode: `SigningMode RequireSigned`
- Trust model: `SigningTrustModel InternalEnterprise`
- Evidence boundary: public `.cer`, SignTool/Authenticode result, MSI SHA-256, provenance만 남긴다.
- Secret boundary: private key/PFX/password, certificate password, API token, protected token blob은 repo/provenance/evidence에 기록하지 않는다.
- Admin opt-in boundary: 실제 internal Root/leaf 생성, `LocalMachine` Root/TrustedPublisher import, signed MSI build, `msiexec` lifecycle smoke는 별도 승인 없이는 실행하지 않는다.
- Publication boundary: 이 gate는 내부 신뢰 기반 release evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

Dry-run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/New-PcvInternalCodeSigningTrust.ps1 `
  -SigningStoreScope CurrentUser `
  -TrustStoreScope LocalMachine `
  -DryRun
```

실제 생성/신뢰 등록 예:

```powershell
$evidence = 'artifacts/internal-enterprise-requiresigned-rc-msi-<timestamp>'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/New-PcvInternalCodeSigningTrust.ps1 `
  -SigningStoreScope CurrentUser `
  -TrustStoreScope LocalMachine `
  -PublicCertificateOutputRoot "$evidence/public-certs"
```

결과 JSON의 `build_arguments.CertificateThumbprint` 값을 `build.ps1 -CertificateThumbprint`에 전달한다. Target host에는 `public-certs` 아래 public root/leaf `.cer`만 배포하고 private key/PFX는 배포하지 않는다.

## 로컬 test signing trust 우회

2026-04-30 `artifacts/p0-local-root-trust-workaround-20260430-2120` evidence는 local self-signed test signer를 현재 사용자 신뢰 저장소에만 추가해 signed RC MSI 검증을 통과시킨 host-local workaround다.

- 대상 MSI: `artifacts/p0-signed-rc-msi-20260430/PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi`
- signer: `CN=PureCVisor Desktop Node Test Code Signing`
- thumbprint: `59A60D62BBB26F3816C77EB535931C8A3C8DFF9C`
- 적용 저장소: `Cert:\CurrentUser\Root`, `Cert:\CurrentUser\TrustedPublisher`
- 미적용 저장소: `Cert:\LocalMachine\Root`, `Cert:\LocalMachine\TrustedPublisher`
- 결과: Authenticode `Valid`, SignTool verify exit `0`

이 우회는 public trusted signing, stable publication, GA 승격 evidence가 아니다. Test host에서 제거하려면 다음을 실행한다.

```powershell
certutil -user -delstore Root 59A60D62BBB26F3816C77EB535931C8A3C8DFF9C
certutil -user -delstore TrustedPublisher 59A60D62BBB26F3816C77EB535931C8A3C8DFF9C
```

## 관리자 smoke

아래 명령은 실제 Windows Installer, service, 제품 루트, 데이터 루트를 변경하므로 elevated PowerShell에서 명시적으로만 실행한다.
Repair smoke는 `/fa` shorthand를 사용하지 않는다.

- 2026-04-30 signed RC smoke에서 `/fa`는 Windows Installer server command line에 `REBOOT=ReallySuppress`를 전달하지 않았다.
- 당시 `REINSTALLMODE=a`, `MsiSystemRebootPending=1`, `ReplacedInUseFiles=1` 조합은 실제 reboot `1641`을 유발했다.
- 이후 repair smoke 계약은 `packaging/windows-desktop-node/installer/PcvDesktopNodeMsiLifecycle.psm1`의 plan과 같이 `/i` + `REINSTALL=ALL` + `REINSTALLMODE=vomus` + `REBOOT=ReallySuppress` + `MSIRESTARTMANAGERCONTROL=Disable`을 사용한다.
- 자동 reboot script는 사용하지 않는다.

```powershell
$msi = 'artifacts/windows-desktop-node/PureCVisorDesktopNode-0.23.8-admin-smoke-windows-x64.msi'
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install.log
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
msiexec /i $msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log
msiexec /x $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall.log
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install-remove-data.log
msiexec /x $msi REMOVE_DATA=1 REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall-remove-data.log
```

확인 기준:

- install 후 service가 started 상태다.
- protected token 포함 runtime policy 요청이 HTTP 200을 반환하고 `token_storage`가 `dpapi-local-machine`이다.
- loopback Web Console root 요청이 HTTP 200을 반환한다.
- repair는 exit code `0`을 기본 성공으로 본다. `3010`은 service/runtime/data 보존 검증이 모두 통과할 때만 `reboot_required=true` 성공으로 기록한다.
- repair 후 protected token, job store, event log, diagnostics가 보존된다.
- `1641`은 Windows Installer가 실제 reboot를 시작한 결과이므로 기본 smoke 성공으로 닫지 않는다. 해당 run은 실패/중단 evidence로 기록하고 post-reboot verification evidence와 원인 분석을 별도로 남긴다.
- 기본 uninstall 후 ProgramData data root가 보존된다.
- `REMOVE_DATA=1` uninstall 후 protected token, legacy raw token, job store, event log, install log, diagnostics가 제거된다.
- uninstall 후 Windows service와 port `80`/`7777` listener가 남지 않는다.

2026-04-30 signed RC lifecycle evidence:

- MSI: `artifacts/p0-signed-rc-msi-20260430/PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi`
- evidence root: `artifacts/p0-signed-msi-lifecycle-rerun-20260430-191040`
- preclean uninstall preserve: exit `0`
- install: exit `0`, runtime healthy
- repair: exit `0`, runtime healthy, reboot required false
- 기본 uninstall: exit `0`, ProgramData/protected token preserved
- remove-data smoke용 reinstall: exit `0`, runtime healthy
- `REMOVE_DATA=1` uninstall: exit `0`, protected token, legacy token, job store, events/install logs, diagnostics removed
- restore install: exit `0`, runtime healthy
- automatic reboot: not used, boot time unchanged during run

2026-05-01 current-head local `RequireSigned` lifecycle/update evidence:

- MSI: `artifacts/p0-local-requiresigned-rc-msi-20260501-165251/PureCVisorDesktopNode-0.23.9-rc.1-windows-x64.msi`
- evidence root: `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`
- git commit: `3d35aa2`
- signing mode: `RequireSigned`
- signer: local self-signed `CN=PureCVisor Desktop Node Test Code Signing`
- public trusted signing: blocked, local public trusted certificate/PFX/private key not found
- stable publication: not executed
- preclean uninstall of `0.23.8-rc.1`: exit `0`
- install: exit `0`, runtime healthy
- repair: exit `0`, runtime healthy, reboot required false
- 기본 uninstall: exit `0`, protected token preserved
- remove-data smoke용 reinstall: exit `0`, runtime healthy
- `REMOVE_DATA=1` uninstall: exit `0`, protected token, legacy token, job store, events/install logs, diagnostics removed
- product-wrapper install/update/config-migration/rollback/CollectDiagnostics/cleanup: all PASS
- final MSI restore install: exit `0`, service Running
- automatic reboot: not used, boot time unchanged during run

2026-05-01 internal enterprise `RequireSigned` lifecycle evidence:

- MSI: `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021/PureCVisorDesktopNode-0.23.10-rc.1-windows-x64.msi`
- evidence root: `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`
- git commit: `318ebc39b8f224c7c24895c485089b1469c4ac66`
- signing mode: `RequireSigned`
- signing trust model: `InternalEnterprise`
- signer: `CN=PureCVisor Desktop Node Internal Code Signing`
- issuer: `CN=PureCVisor Internal Code Signing Root CA`
- trust stores: `Cert:\LocalMachine\Root`, `Cert:\LocalMachine\TrustedPublisher`
- MSI SHA-256: `5355507f5909d5e17280a90b8ac41af858b871633b8ec2e1b03f2b4eb26297ba`
- SignTool verify exit: `0`
- Authenticode: `Valid`
- install: PASS, runtime healthy
- repair: PASS, runtime healthy
- 기본 uninstall: PASS, protected token preserved
- remove-data smoke용 reinstall: PASS, runtime healthy
- `REMOVE_DATA=1` uninstall: PASS, protected token, legacy token, job store, events/install logs, diagnostics removed
- final MSI restore install: PASS, service `Running`
- automatic reboot: not used, boot time unchanged during run
- public trusted signing: not claimed
- external stable publication: not executed

2026-05-01 product path/service/MSI start unsigned admin-smoke evidence:

- MSI: `artifacts/product-path-service-msi-start-20260501-194840/msi-build/PureCVisorDesktopNode-0.26.0-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/product-path-service-msi-start-20260501-194840/lifecycle-sequential-20260501-200211`
- git commit in provenance: `d9ef8a36c4fcca440d25fd96912244284c909b0b`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `b9bfff35195f88bd1b9e4c4f35f3d883e39e5a721a32bb2f15023f5fe60446f8`
- install: exit `0`, runtime healthy
- repair: exit `0`, runtime healthy, reboot required false
- 기본 uninstall: exit `0`, service removed, Web root unavailable
- remove-data smoke용 reinstall: exit `0`, runtime healthy
- `REMOVE_DATA=1` uninstall: exit `0`, service removed, Web root unavailable
- final MSI restore install: exit `0`, service `Running`, Web root `200`
- automatic reboot: not used, boot time unchanged during run
- reboot required/post-reboot verification required: false
- public trusted signing: not claimed
- external stable publication: not executed
- invalid evidence note: 같은 artifact root의 초기 `lifecycle/` runner는 `msiexec` 대기 방식 오류로 1618 경합을 만들었으므로 lifecycle evidence로 사용하지 않는다.

2026-05-02 route parity service/MSI/Hyper-V unsigned admin-smoke evidence:

- MSI: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729/PureCVisorDesktopNode-0.26.6-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729`
- git commit in provenance: `22c38284dcb3d3804b077c7f5c0fbf074b3ef034`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `a468357f06c0176c75f02266b900aef17c5d0393590bb5b638797cd0345874a8`
- direct service-action smoke: PASS
- MSI lifecycle install/repair/uninstall preserve/install-remove-data/uninstall-remove-data/final restore: PASS
- installed .NET Host Hyper-V API routes: `host.status`, `network.inventory`, `vm.create`, `vm.list`, `vm.get`, `vm.start`, `checkpoint.create`, `checkpoint.list`, `checkpoint.delete`, `vm.poweroff` PASS
- final service: `Running`, `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`
- automatic reboot: not used, boot time unchanged during run
- `pcv-spike-*` VM leftovers: none
- public trusted signing: not claimed
- external stable publication: not executed

2026-05-02 native network inventory included admin-smoke evidence:

- MSI: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126/PureCVisorDesktopNode-0.26.8-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`
- git commit in provenance: `b23030efb2cc305925ea3765d5c8a341e40069a9`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `50694850b3ff3bd199025f950fc69802bb01066474acc71c8ea275f026235e71`
- installed `network.inventory`: PASS, `source=hyperv`, `mutating=false`, `Default Switch` present
- service-action, MSI lifecycle install/repair/uninstall/`REMOVE_DATA=1`/final restore: PASS
- installed .NET Host Hyper-V API route smoke: PASS
- final service: `Running`, `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`
- automatic reboot: not used, boot time unchanged during run
- `pcv-spike-*` VM leftovers: none
- public trusted signing: not claimed
- external stable publication: not executed

리뷰 수정 후속:

- `repair-installed`는 `sc.exe config` 전에 missing SCM service를 다시 생성한다.
- 당시 native `network.inventory`는 switch topology parity field가 불완전하면 PowerShell helper로 fallback했다.
- 후속 native adapter slices에서 `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native read route로 전환됐고 read-route helper fallback은 제거됐다. Checkpoint create/restore/delete는 C# WMI snapshot service adapter로 전환됐다.

2026-05-02 리뷰 수정 service/MSI/Hyper-V unsigned admin-smoke evidence:

- MSI: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406/PureCVisorDesktopNode-0.26.9-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`
- provenance git commit: `352aa256b77109ea9104602aebd424c627db11ed`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `d517baee2149d9dfcf6bd34d77b4f9de8663fd7e416558c1ba0ffb3de16788e3`
- 반영된 수정: MSI repair missing-service 재생성, native `network.inventory` topology parity fallback, shared request processor 직렬화
- installed `network.inventory`: PASS, `source=hyperv`, `mutating=false`, `Default Switch`, `type=internal`, `allow_management_os=true`
- service-action, MSI lifecycle install/repair/uninstall/`REMOVE_DATA=1`/final restore: PASS
- installed .NET Host Hyper-V API route smoke: PASS
- final service: `Running`, `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`
- automatic reboot: 사용하지 않음, boot time unchanged
- `pcv-spike-*` VM 잔여물: 없음
- public trusted signing: 주장하지 않음
- external stable publication: 실행하지 않음

2026-05-02 `host.status` native adapter service/MSI/Hyper-V unsigned admin-smoke evidence:

- MSI: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154/PureCVisorDesktopNode-0.27.1-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`
- provenance git commit: `7120ef58b924cfdf664f868b857fb91537bf6be9`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `9e6c57ef852df2df7794598fd0193141ad4f95f7ec365c565453a6fc05b9c48f`
- 반영된 수정: native `host.status` C# registry/WMI/service/admin adapter, `native_core.reason=host.status,network.inventory`
- installed `host.status`: PASS, `supported=true`, Windows 10 Pro for Workstations `25H2`, admin elevated, Hyper-V enabled, VMMS running, Default Switch present
- installed `network.inventory`: PASS, `source=hyperv`, `mutating=false`, `Default Switch`, `type=internal`, `allow_management_os=true`
- service-action, MSI lifecycle install/repair/uninstall/`REMOVE_DATA=1`/final restore: PASS
- installed .NET Host Hyper-V API route smoke: PASS
- final service: `Running`, `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`
- automatic reboot: 사용하지 않음, boot time unchanged
- `pcv-spike-*` VM 잔여물: 없음
- public trusted signing: 주장하지 않음
- external stable publication: 실행하지 않음

2026-05-05 current native OS gate unsigned admin-smoke evidence:

- MSI: `artifacts/os-mutation-gates-20260505-003459-0341/msi-build/PureCVisorDesktopNode-0.34.1-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/os-mutation-gates-20260505-003459-0341`
- provenance git commit: `6f97a24aa2bdfacf33d7bd987559eb85e363e119`
- follow-up firewall lookup hardening commit: `49a06acd3493066a10ec26fe541d5d8be1005c2b`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`
- install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall/final restore: exit `0`
- final service: `Running`, loopback prefix `http://127.0.0.1:7777/`, protected token file source
- native firewall enable/remove: owned rule `PureCVisor Desktop Node Local API LAN`, `TCP/7777`, `Private`, `LocalSubnet`, final absent
- LAN exposure smoke: `0.0.0.0` prefix unsupported by HttpListener recorded, LAN IP prefix `http://[redacted-private-endpoint]:7777/` runtime policy `HTTP 200` with bearer token
- trust-store install/remove/restore: internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E` and TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`, final present
- automatic reboot: 사용하지 않음, boot time unchanged
- public trusted signing: 제외, 주장하지 않음
- external stable publication: 실행하지 않음

2026-05-05 `0.35.5-admin-smoke` 실행 당시 HEAD native OS gate fresh evidence:

- MSI: `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355/PureCVisorDesktopNode-0.35.5-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/os-mutation-gates-20260505-101659-0355-final`
- provenance git commit: `2fb38f20a8c74433684345ded8a33ba16a863621`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`
- install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall/final restore install: exit `0`
- final installed DisplayVersion: `0.35.5`
- native firewall enable/remove: owned rule `PureCVisor Desktop Node Local API LAN`, `TCP/7777`, `Private`, `LocalSubnet`, final absent
- Event Log register/remove: final source absent
- native trust-store install/remove/restore: ADR-0003 internal Root/TrustedPublisher final present
- LAN exposure smoke: LAN IP prefix `http://[redacted-private-endpoint]:7777/`, runtime policy `HTTP 200`, Web assets `HTTP 200`
- trust-store install/remove/restore: internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E` and TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`, final present
- final service: `Running`, loopback prefix `http://127.0.0.1:7777/`, installed DisplayVersion `0.35.5`
- automatic reboot: 사용하지 않음, boot time unchanged
- public trusted signing: 제외, 주장하지 않음
- external stable publication: 실행하지 않음

2026-05-05 `0.35.7-admin-smoke` 현재 HEAD native OS gate fresh evidence:

- MSI: `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357/PureCVisorDesktopNode-0.35.7-admin-smoke-windows-x64.msi`
- evidence root: `artifacts/os-mutation-gates-20260505-180434-0357-rerun`
- provenance git commit: `2ec9e71d45b702e106824c86500cd6152b18fab7`
- signing mode: `AllowUnsignedDev`
- MSI SHA-256: `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`
- install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall/final restore install: exit `0`
- final installed DisplayVersion: `0.35.7`
- native firewall enable/remove: owned rule `PureCVisor Desktop Node Local API LAN`, `TCP/7777`, `Private`, `LocalSubnet`, final absent
- Event Log register/remove: final source absent
- native trust-store install/remove/restore: ADR-0003 internal Root/TrustedPublisher final present
- LAN exposure smoke: LAN IP prefix `http://[redacted-private-endpoint]:7777/`, bearer runtime policy `HTTP 200`, Web assets `HTTP 200`
- config migration apply smoke: service-running precondition returned `PCV_CONFIG_MIGRATION_SERVICE_RUNNING`, `MutationPlanned=false`, `MutationPerformed=false`
- trust-store install/remove/restore: internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E` and TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`, final present
- final service: `Running`, loopback prefix `http://127.0.0.1:7777/`, installed DisplayVersion `0.35.7`
- automatic reboot: 사용하지 않음, boot time unchanged
- public trusted signing: 제외, 주장하지 않음
- external stable publication: 실행하지 않음

## 기본 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Legacy root boundary Pester는 component/archive baseline으로 분리한다. 기본 installer 개발 loop의 required command에는 active `spikes/**` Pester path를 넣지 않는다.
