# PureCVisor Desktop Node 가이드

## 2026-08-25 현재 진입점

- Operational current의 단일 진실은 `docs/ga-ready/current-evidence.json`과 문서 최상단에
  생성 블록이 있는 current-facing 문서다. 이 가이드는 version/hash를 복제하지 않는다.
- 활성 운영자 표면은 Web Console과 PCVCLI다. TUI는 ADR-0011에 따라 제거됐으며 아래 2026-05
  TUI 서술은 historical snapshot으로만 읽는다.
- 현재 Web 검증 후속은
  `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b.md`의
  Task 1~13 local-parity 구현과 full completion audit까지 완료했다. Legacy Pester와 Node
  positive projection은 각각 `50/50`, migration manifest는 `62`행이고 Web 행만 `mapped`/local
  pass/CI pending이다. 근거는
  `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`다. Required CI
  dual-run, required CI의 Pester 및 non-admin PowerShell 제거와 cutover는 모두 pending이다.

## 2026-05-29 historical snapshot

최신 operational anchor는 `0.42.59-admin-smoke` /
`full-admin-host-mutation-gate-20260529-04259`다. Latest package evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`, full admin host
mutation evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`,
manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`, current ledger는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`가 소유한다.

설치본 Web/TUI/CLI current-card는 04259 fullgate 후 PASS했고,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`
및 `artifacts/installed-operator-surface-current-card-20260529-04259/summary.json`에서
Web/TUI/CLI smoke, runtime policy running interrupt, Web/TUI running cancel affordance,
실제 Windows guest credentialed execution을 확인했다.
직전 `0.42.58-admin-smoke` predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.
04250→04254 manual-admin readiness는 0.42.54 fullgate target artifact를 확인했지만 현재 host
baseline mismatch로 blocked 기록만 남겼다. Web/TUI running guest execution cancel affordance는
설치본 PASS로 승격됐다.
초기 full-gate attempt의 service repair idempotence hang은
`76c77a86bbb72e415b1968169c16f1638b76fa56`에서 수정했고 r2 gate가 PASS했다.
Hyper-V QoS mutation closure는 `77f1a3f291b4f736218cb5110dcecd3b464860d4` 기준으로 닫았고,
Phase 3 Web/TUI QoS direct control은 `46e745efc698a06e4b065a19c3f07217e821155e` 기준으로
package/fullgate/manual-admin current-card를 닫았고, Guest Execution provider/direct-control은
`cc774b257d6cd772c3a890266aca62aa8ab8eadc` 기준으로 provider/fullgate/current-card를 닫은 뒤
`2c11e359709c775be7a57ea9624716720c5b62d6` 기준으로 0.42.54 fullgate까지 닫고,
`958052181012f7d1be6ccff535316bfaeeef07df` 기준으로 0.42.55 fullgate/current-card를 닫았고,
`0.42.59-admin-smoke`는 Guest Execution redaction hardening과 Hyper-V QoS value hardening을
package/fullgate/manual-admin/current-card chain으로 승격했다. 최신 public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`가
PASS했고, installed current-card payload 후보는 이미 열린 `0.42.60-admin-smoke`를 유지한다.
docs-maintenance postpush만으로 추가 package 후보를 열지 않는다.
이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable
publication evidence가 아니다. 아래 이전 날짜 current 문단은 historical predecessor로
해석한다.

## 2026-05-21 historical predecessor

최신 operational anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`다. Full admin host mutation은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`,
manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md` /
`manual-admin-campaign-descriptor-20260521-04239-04240-closed`, current ledger는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`가 소유한다.

설치본 Web/TUI/CLI current-card는 04240 기준으로 PASS했고,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`
및 `artifacts/installed-operator-surface-current-card-20260521-04240/summary.json`에서
Web/TUI/CLI smoke와 0.42.39→0.42.40 manual-admin closure current-card를 확인했다.
설치본 PCVCLI targeted smoke는
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`에서
실제 VM 대상 `vm limit`, `blkio-get`, `bandwidth`, `guest-agent-status`, `guest-ping`을
추가 확인했다. Actual VM Web/TUI QoS/guest readback smoke는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`에서
기록하며, 설치본 TUI row projection blocker는 source fix code-level PASS 후
`0.42.41-admin-smoke` package chain trigger로 남겼다.
Historical 0.42.38 VM media/resource mutation route promotion과 0.42.37 Hyper-V pause
lifecycle smoke는 predecessor로 보존한다. PR #167 public-boundary PASS는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`다.
아래 이전 날짜 current 문단은 historical predecessor로 해석한다.

## 2026-05-17 historical snapshot

Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` / `manual-admin-campaign-descriptor-20260517-04226-04227-closed`이며, target MSI SHA-256 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`, update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`, provenance commit `69aba3eb3ff08c843f1a481818ddc86eac2f019b`와 함께 `host-ops-lifecycle-descriptor-bridge-v1` / `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated` 계약을 보존한다.

Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Operator Surface predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` / `manual-admin-campaign-descriptor-20260517-04227-04228-closed`이며, target MSI SHA-256 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit `b9676f6dc37d667ae0d60367e9f4e576a27e3864`로 보존한다. PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`로 보존한다. 둘 다 public trusted signing 또는 외부 stable publication evidence가 아니다.

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

이 문서는 `purecvisor-desktop-node` 독립 저장소의 상위 가이드다.

## 전제

- Windows 10/11 Pro/Enterprise
- Hyper-V 활성화 host
- PowerShell 7
- Pester 5
- 정적 JavaScript syntax check용 Node.js
- Web Console browser fixture parity 검증용 Node.js
- .NET Windows Service Host와 MSI artifact build가 필요할 때 .NET SDK 10과 WiX CLI
- Phase 25 Web Console 후보 typecheck/parity 검증이 필요할 때만 TypeScript toolchain

## 제품 사용 빠른 시작

설치된 제품은 Windows service `PureCVisorDesktopNode`로 실행된다. 기본 Web Console은 loopback-only `http://127.0.0.1/`에서 열고, Web API는 `http://127.0.0.1:7777/api/v1/...`로 분리한다.

```powershell
Start-Process "http://127.0.0.1/"
Get-Service PureCVisorDesktopNode
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

제품 루트는 `C:\Program Files\PureCVisor\DesktopNode`, 데이터 루트는 `%ProgramData%\PureCVisor\desktop-node`, service host는 `DesktopNode.Host.exe`다. Web Console static root는 loopback에서 바로 열 수 있지만, API route는 bearer token을 요구한다. Token 값은 command line에 노출하지 않고 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` protected token file 경로를 사용한다.

LAN exposure는 기본 실행 경로가 아니다. 관리자 opt-in, `-AllowLan`, token source, firewall approval gate가 있을 때만 수행하고 rollback/final-state proof를 남긴다.

## 주요 진입점

- 유저 가이드: `docs/USER_GUIDE.md`
- 유저 기능 사용 명세서: `docs/USER_FEATURE_USAGE_SPEC.md`
- 운영 가이드: `docs/OPERATIONS_GUIDE.md`
- Phase 19 제품 승격 재판정: `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- Phase 22 release/version 정책: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md`
- 내부 서비스 signing trust 정책: `docs/adr/0003-internal-trusted-signing-policy.md`
- Phase 24 Local API job runtime 경계: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`
- Phase 25 .NET/TypeScript 전환 후보: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`
- Phase 25 TypeScript Web Console parity 후보: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`
- guide 기반 운영/확장 backlog: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
- guide 기반 VM delete UI: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-vm-delete-ui-design.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-vm-delete-ui.md`
- .NET Windows Service Host replacement: `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`
- 내부 전용 GA-ready 제품 런타임 결정: `docs/adr/0004-ga-ready-product-runtime-candidate.md`, `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`
- 내부 사설망 전용 배포 결정: `docs/adr/0006-internal-private-network-distribution.md`, `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- GA-ready route/ownership 문서: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
- 검증 기준: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- 저장소/출시 경계: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- 제품 wrapper: `packaging/windows-desktop-node/README.md`
- Installer: `packaging/windows-desktop-node/installer/README.md`
- Local API: `archive/spikes/purecvisor-desktop-node/api/README.md`
- Active .NET CLI: `src/DesktopNode.Cli/` (`pcvcli.exe`)
- Historical .NET TUI predecessor: `docs/adr/0011-cli-web-only-operator-surface.md`
- Archived PowerShell CLI baseline: `archive/spikes/purecvisor-desktop-node/cli/README.md`
- Hyper-V helper: `archive/spikes/purecvisor-desktop-node/hyperv/README.md`
- Service helper: `archive/spikes/purecvisor-desktop-node/service/README.md`

## 경계

이 저장소는 Windows Desktop Node 전용이다. Linux `purecvisorsd` runtime과 Single Edge 릴리스 판단은 포함하지 않는다.

현재 경계 요약:

- ADR-0006 기준 배포 범위는 `internal-private-network-only`다.
- Public trusted signing, trusted timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke는 `out-of-scope`다.
- 내부 배포 gate는 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke를 기준으로 한다. HTTPS/TLS installed smoke는 `internal-https-tls-lifecycle-installed-2026-05-10-0397` PASS이고, clean-host smoke는 `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417` dedicated Hyper-V PASS 상태다.

- ADR-0004 기준 제품 승격 판단은 내부 전용 서비스 범위의 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`이다.
- Phase 22 release/version policy와 installer artifact/channel contract 일부는 ADR-0002로 현재 적용 결정에 채택됐다.
- ADR-0003은 내부 서비스 운영용 internal Root/leaf `RequireSigned` signing trust model을 채택한다.
- Phase 24 후보는 Local API job runtime 경계를 고정한다.
- Phase 25 후보는 C#/.NET contract/runtime/API/service/host, TypeScript Web Console, PowerShell Windows adapter 역할 분리를 정의했다.
- `src/DesktopNode.Host/**`는 기본 제품 service host와 MSI installed action runner다.
- `src/DesktopNode.Api/**`는 `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list native read adapter, VM create/start/shutdown/poweroff/restart/delete native lifecycle adapter, checkpoint create/restore/delete native mutation adapter를 포함한다. Current served Hyper-V read/mutation path는 PowerShell helper fallback 없이 structured success/failure를 반환한다.
- Native VM create product path는 이번 slice에서 Hyper-V Generation 2만 지원하고 Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다. Native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. 같은 API layer가 queued job runtime, job get/cancel/retry, JSON job store save/load/recovery도 처리한다.
- Web Console served `web/app.js`는 `web/src/served-app.ts` TypeScript build output으로 교체됐다. served freshness, generated manifest/static parity, Node `vm` 최소 DOM fixture는 `npm run verify:parity --prefix web` 검증에 포함된다.
- Web/API port split evidence는 `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`와 `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`다. 기본 Web Console은 `http://127.0.0.1/`, Web API는 `http://127.0.0.1:7777/api/v1/...`이며 `/pcv-config.js`가 API origin을 주입한다. 설치본 listener smoke는 Web 80/API 7777 분리 PASS를 기록했다. 기본 Web Console은 loopback HTTP이고, internal HTTPS/TLS lifecycle installed smoke는 ADR-0006 별도 evidence에서 PASS다. Public 443 publication은 scope 밖이다.
- Installed account login/noVNC bridge evidence는 `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`다. Account login installed smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 PASS했고, noVNC bridge는 explicit target host/port가 있을 때만 WebSocket-to-VNC TCP bridge로 켜진다. Target-backed noVNC installed streaming과 installed TUI operator smoke는 `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`에서 PASS로 기록한다.
- 2026-05-10 `manual-admin-operator-hardening-followup-2026-05-10-0415`는 `artifacts/manual-admin-followup-20260510-0415`에서 0.41.5 installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition을 PASS로 재확인했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-17 `manual-admin-campaign-2026-05-17-04225-04226`와 2026-05-16 `full-admin-host-mutation-gate-2026-05-16-04226-hostmutation`, `installed-operator-surface-current-card-2026-05-16-04226`, `admin-smoke-package-2026-05-16-04226`, `manual-admin-campaign-descriptor-2026-05-16-04225-04226`, `public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass`는 `0.42.26-admin-smoke` 최신 full admin host mutation, Web/TUI/CLI current-card, package build, 04225→04226 manual-admin closure, PR #145 post-merge public-boundary PASS evidence다. Package MSI SHA-256은 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`, full-gate/target operational MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP SHA-256은 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`, provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`이다. Closure descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`는 `missing_count=0`, `not_pass_count=0`이다. Current-card는 `full-admin-host-mutation-gate-20260516-04226`, `runtime-api-current-evidence-rollup-v1`, registry bridge route detail count `4`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, unauthenticated API boundary `401`/`PCV_AUTH_REQUIRED`를 확인했다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-16 `full-admin-host-mutation-gate-2026-05-16-04225-hostmutation`, `installed-operator-surface-current-card-2026-05-16-04225`, `manual-admin-campaign-2026-05-16-04224-04225`, `public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass`는 `0.42.25-admin-smoke` previous full admin host mutation, Web/TUI/CLI current-card, 04224→04225 MANUAL-ADMIN package-pair PASS, PR #144 post-merge public-boundary PASS evidence다. Full-gate/target MSI SHA-256은 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, update ZIP SHA-256은 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`, provenance commit은 `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`이다. Descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`는 `missing_count=0`, `not_pass_count=0`이다.
- 2026-05-16 `admin-smoke-package-2026-05-16-04225`는 earlier package build record로 보존한다. Package MSI SHA-256은 `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`, provenance commit은 `403d4474c4b88136774600cc81ca2d941c0b5e4b`다. `0.42.24-admin-smoke` full-gate/current-card evidence는 historical predecessor로 보존하며 full-gate batch `full-admin-host-mutation-gate-20260516-04224`, package build MSI SHA-256 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit `b974d6b541423f2e4160f726f96155b16f105e9d`를 기록한다.
- 2026-05-16 `manual-admin-campaign-descriptor-2026-05-16-04223-04224`는 `0.42.23-admin-smoke -> 0.42.24-admin-smoke` descriptor 생성 evidence다. Descriptor batch `manual-admin-campaign-descriptor-20260516-04223-04224`는 `blocked-by-missing-evidence`, `missing_count=5`, `not_pass_count=1`이므로 닫힌 package-pair PASS가 아니다. 최신 닫힌 package-pair PASS는 `manual-admin-campaign-2026-05-18-04229-04230`이 소유한다.
- Previous 04221 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221`, `artifacts/os-mutation-gates-batch-profile-20260516-04221`로 보존한다.
- Historical 04220 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`로 보존한다. Full-gate MSI SHA-256은 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit은 `0895d018935298721b25b5d9ce1ae083a6690c25`이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-15 `full-admin-host-mutation-gate-2026-05-15-04218-hostmutation`은 `0.42.18-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260515-163107-04218`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260515-163107-04218`, `artifacts/os-mutation-gates-batch-profile-20260515-163107-04218`이고 full-gate MSI SHA-256은 `0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09`, clean package MSI SHA-256은 `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`, provenance commit은 `9121d1f5e7fa83d803c484a44698d4fc8e825c19`, signing mode는 `AllowUnsignedDev`다. Installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260515-163107-04218`, descriptor excluded `true`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, unauthenticated API boundary `401`/`PCV_AUTH_REQUIRED`, token value UI text exposure `false`를 확인했다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-14 `full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation`은 `0.42.12-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit`, `artifacts/os-mutation-gates-batch-profile-20260514-140126-04212-explicit`이고 full-gate MSI SHA-256은 `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`, package MSI SHA-256은 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, full-gate provenance commit은 `d338b8a99f3e1e3839ac89a6de0da034ff3da148`, package provenance commit은 `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`, signing mode는 `AllowUnsignedDev`다. Product wrapper native service-action repair, Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고 installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260514-140126-04212-explicit`, route/OS child evidence `available`, errors `0`을 확인했다. Web Console current-card smoke는 `artifacts/web-console-current-card-20260514-140126-04212-explicit`에서 PASS였고 token value는 UI text에 노출되지 않았다. final service `Running`, installed manifest `0.42.12-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 2026-05-14 rerun과 2026-05-13 04212 full gate evidence는 historical predecessor로 보존한다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`은 `0.42.9-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`이고 full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260513-040213-0429`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.9-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 최신 MSI/service installed listener PASS evidence는 `0.39.0-admin-smoke`이며 `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`를 근거로 한다. MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode는 `AllowUnsignedDev`다. Final service `PathName`은 diagnostic bundle/hardening 인자를 포함했고 protected-token diagnostic bundle create/download는 POST `201`, GET `200`, redaction PASS였다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다.
- 최신 focused firewall/trust-store/LAN/Event Log OS gate PASS evidence는 `0.39.0-admin-smoke`이며 `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`를 근거로 한다. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false였고 LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, firewall final count `0`, Event Log source absent, internal trust Root/TrustedPublisher present, boot time unchanged로 끝났다. Public trusted signing은 `excluded`, external stable publication은 `not-claimed`다.
- 최신 MSI/update package apply PASS evidence는 `0.39.1-admin-smoke`이며 `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`, `artifacts/msi-update-package-20260509-0391`를 근거로 한다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, signing mode는 `AllowUnsignedDev`다. Elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 확인했다. Public trusted signing은 `excluded`, external stable publication은 `not-claimed`다.
- Public distribution ops execution bundle evidence는 `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`, `artifacts/public-distribution-ops-execution-bundle-20260509-0391`다. 이 bundle은 ADR-0005 preflight를 local non-mutating 방식으로 수집했으며 public trusted signing/external stable publication 또는 host mutation을 주장하지 않는다.
- Burn bootstrapper lifecycle smoke는 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416`에서 internal bundle build/install/repair/remove와 MSI restore PASS로 기록됐다. Windows Event Log provider/default writer transition은 `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `artifacts/windows-event-log-provider-default-transition-20260509-0391`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `artifacts/windows-event-log-default-transition-installed-20260510-0396`에서 installed provider register/write/query와 installed default writer/repair/remove/volume/schema PASS로 기록됐다. Windows Credential Manager transition은 `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `artifacts/windows-credential-manager-transition-20260509-0391`에서 current-user capability PASS와 당시 `LocalSystem` service blocker를 기록했고, 최신 `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `artifacts/windows-credential-manager-default-transition-installed-20260510-0395`가 installed LocalSystem default transition PASS를 기록했다.
- Public external gates blocked scan은 `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`, `artifacts/public-external-gates-blocked-20260509-0391`다. Public signing material, timestamp URL, external upload endpoint/credential, public stable installer URL, public clean-host publication input이 없어 timestamp evidence, external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback은 public scope에서 blocked/out-of-scope다. Internal clean-host install/update/rollback은 ADR-0006 evidence에서 별도로 PASS다.
- Public ops final 1-7 follow-up attempt는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `artifacts/public-ops-final-followup-attempt-20260509-0391`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1`다. `remaining_follow_up_count: 7`, `host_mutation_performed=false`, `public_release=not-claimed`이며 public trusted signing/external stable publication은 주장하지 않는다.
- Public ops gate execution readiness는 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`, `artifacts/public-ops-gate-execution-readiness-20260509-0392`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`다. External stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback은 blocked이고 TLS는 `partial-code-level-cert-generate-rotate-delete-pass`, `tls_binding=not-run`, `host_mutation_performed=false`다. Public trusted signing/external stable publication은 주장하지 않는다.
- Public ops installed hardening evidence는 `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`다. `credential-manager-system-proof`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard` native service-action path는 선행 code-level readiness를 기록했고, 후속 `windows-credential-manager-default-transition-installed-2026-05-10-0395`, `windows-event-log-default-transition-installed-2026-05-10-0396`, `internal-https-tls-lifecycle-installed-2026-05-10-0397` evidence가 Credential Manager default transition, Event Log default writer hardening, internal HTTPS binding/trust boundary를 installed admin-smoke PASS로 닫았다.
- 최신 internal signed build PASS evidence는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`의 `0.38.7-rc.1`이다. Authenticode는 `Valid`, SignTool verify exit는 `0`, MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`다. 같은 `0.38.7-admin-smoke` host mutation/update rollback attempts는 non-elevated blocked history이며 PASS evidence가 아니다.
- 최신 installed destructive update/rollback PASS evidence는 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`의 `0.38.8-admin-smoke`다. MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 `0.38.6-admin-smoke`로 복원하면서 `DesktopNode.failed` diagnostics root를 보존했다. Final service는 `Running`, boot time unchanged, `host_mutation_performed=true`다. Config/job store migration apply installed destructive admin smoke는 `artifacts/config-jobstore-migration-apply-installed-20260507-0386`의 `0.38.6-admin-smoke`에서 PASS했다. Public trusted signing과 외부 stable publication은 내부 전용 서비스 scope 밖이다.
- Installer build output은 MSI/provenance/hash sidecar에 더해 `.publication.json` descriptor를 작성한다. 최신 descriptor evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`이며, public trusted signing과 외부 stable publication은 `not-claimed`, Burn/MSIX/winget publication은 미실행 상태로 기록한다.
- MSIX package lifecycle smoke는 `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`와 `artifacts/msix-package-lifecycle-smoke-20260510-0416`에 기록됐다. 별도 internal smoke identity `PureCVisor.DesktopNode.MsixSmoke`로 build/sign/verify, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence가 PASS였지만 public trusted signing 또는 외부 stable publication evidence는 아니다.
- GA-ready 제품 재설계는 PowerShell-free product ops/runtime, `spikes/**` archive/remove, TypeScript Web Console app 승격, xUnit/npm/browser-level fixture 후보 중심 검증 전환을 목표로 하며 ADR-0004로 현재 적용 결정이 됐다. Playwright는 후속 도구 후보일 뿐 현재 required dependency가 아니다.
- ADR-0004는 내부 전용 서비스 범위의 `ga-ready-product-runtime` 현재 적용 결정이다.
