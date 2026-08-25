# PureCVisor Desktop Node 운영 가이드

## 2026-07-14 현재 운영자 표면

ADR-0011에 따라 일상 운영은 Web Console을 기본 대화형 표면으로, PCVCLI를 terminal
automation/JSON 표면으로 사용한다. TUI는 active product에서 제거됐으며 Local API/backend
기능은 유지된다. Code-level evidence는
`docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`다.
`0.42.62-admin-smoke` package/fullgate는 operational anchor를 유지하지만 설치본
Web/TUI/CLI current-card는 dated predecessor다. `0.42.63-admin-smoke` CLI-Web installed
promotion은 pending이다.

## 2026-05-29 historical predecessor

운영 기준 current ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
`current-evidence-ledger-2026-05-29-04259-public-boundary-docs-maintenance-postpush-pass`다. 최신 operational anchor는
`0.42.59-admin-smoke` / `full-admin-host-mutation-gate-20260529-04259`이며,
manual-admin package-pair closure는 `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다.

설치본 운영 smoke current-card는 `0.42.59-admin-smoke` fullgate 후 PASS했다.
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`와
`artifacts/installed-operator-surface-current-card-20260529-04259/summary.json`에서
Web/TUI/CLI current-card, ops summary, public-boundary current evidence, runtime policy running interrupt,
Web/TUI running cancel affordance, 실제 Windows guest credentialed execution smoke를 기록했다. 04250→04254 manual-admin readiness는 현재 host baseline
mismatch로 blocked다. Web/TUI running guest execution cancel affordance는 code-level PASS이며
04259 설치본 current-card로 재확인됐다.
Actual VM 기반 설치본 TUI row projection은
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`가
PASS로 기록한다. 최신 public-boundary PASS는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`가
소유하고 run `26636072420`, job `78496568595`, head
`5a2f91762a6c2a8ab6b84d334fa6cb420474671f`에서 PASS했다. `0.42.60-admin-smoke`
installed current-card payload 후보는 이미 열려 있으며, docs-maintenance postpush는 추가
package 후보를 열지 않는다. account/noVNC는
0.42.58 PASS를 carry-forward하고 actual VM Guest Execution/QoS smoke는 provider/control payload 변경
때 재실행한다. 04257/04256/04254 fullgate/running cancel/04253 public-boundary, PR #169 public-boundary와 후속 no-product-payload 판단은 historical predecessor로
보존한다.
Public trusted signing 또는 외부 stable publication evidence가 아니며, 아래 이전 날짜 current
문단은 historical predecessor로 해석한다.
직전 0.42.58 predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.

## 2026-05-21 historical predecessor

운영 기준 current ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
`current-evidence-ledger-2026-05-21-04240-current-card-04241-trigger`다. 최신
operational anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`이며, manual-admin package-pair closure는
`0.42.39-admin-smoke -> 0.42.40-admin-smoke` /
`manual-admin-campaign-descriptor-20260521-04239-04240-closed`다.

설치본 운영 smoke current-card는 `0.42.40-admin-smoke` 기준으로 PASS했다.
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`와
`artifacts/installed-operator-surface-current-card-20260521-04240/summary.json`에서
Web/TUI/CLI current-card와 04239→04240 manual-admin closure PASS를 기록했다.
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`는
설치본 `pcvcli`로 실제 VM 대상 QoS/guest-service targeted smoke를 PASS로 기록한다. Historical
0.42.38 VM media/resource mutation route promotion과 0.42.37 실제 VM lifecycle smoke는
predecessor로 보존한다.
Actual VM Web/TUI QoS/guest readback smoke는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`가
기록하며, 설치본 TUI row projection blocker는 `0.42.41-admin-smoke` package chain
trigger다.
Public trusted signing 또는 외부 stable publication evidence가 아니며, 아래 이전 날짜
current 문단은 historical predecessor로 해석한다.

## 2026-05-17 현재 기준

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

Historical PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`로 보존한다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.

작성 기준: 2026-05-10

이 문서는 설치된 PureCVisor Desktop Node를 운영하는 사람이 보는 runbook이다. 일반 사용자 화면 사용법은 `docs/USER_GUIDE.md`를 따르고, 개발 검증 기준은 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

## 운영 경계

PureCVisor Desktop Node는 Windows Desktop Node 전용 내부 서비스다. 현재 적용 결정은 ADR-0004의 `ga-ready-product-runtime`와 ADR-0006의 `internal-private-network-only`이며, 배포 경계는 내부 사설망 전용이다.

현재 operational evidence anchor는 `0.42.29-admin-smoke`다. 최신 full admin host
mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-17-04229-hostmutation.md`
및 `full-admin-host-mutation-gate-20260517-04229`이 소유하고, installed Web/TUI/CLI
current-card는 `artifacts/installed-operator-surface-current-card-20260517-04229`에서
`runtime-api-current-evidence-rollup-v1`, `batch_evidence.status=available`,
Runtime/API registry bridge route detail count `4`, Host Ops Web diagnostics table
`host-ops-web-diagnostics-bucket-table-v1`, unauthenticated `PCV_AUTH_REQUIRED` boundary를
확인했다. `0.42.28-admin-smoke -> 0.42.29-admin-smoke` Manual admin package-pair는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`에서 PASS로 닫혔고
descriptor `manual-admin-campaign-descriptor-20260517-04228-04229-closed`는
`missing_count=0`, `not_pass_count=0`이다. 이전 04226→04227 및 04225→04226 closure는
historical predecessor로 보존한다. 04226→04227 predecessor evidence는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`이고 descriptor는
`manual-admin-campaign-descriptor-20260517-04226-04227-closed`이며
target MSI SHA-256 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`,
update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`,
provenance commit `69aba3eb3ff08c843f1a481818ddc86eac2f019b`,
`host-ops-lifecycle-descriptor-bridge-v1` /
`service-action-eventlog-firewall-truststore-credential-manager-data-root-separated` 계약을 유지한다.

운영자가 먼저 지켜야 할 원칙:

- 기본 listener는 loopback-only이며 Web Console은 `http://127.0.0.1/`, Web API는 `http://127.0.0.1:7777/api/v1/...`로 분리한다.
- API route는 bearer token을 요구한다.
- Account/RBAC/JWT route는 local `accounts.json`와 `jwt-signing-key.txt`가 완성된 뒤에만 service bearer token gate를 대체할 수 있다. 기본 bootstrap은 `no-default-account`다.
- Token 값은 command line, issue, 문서, diagnostic bundle, stdout/stderr에 남기지 않는다.
- Protected token file 경로는 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`다.
- CLI에서 token source를 생략하면 기본 protected token file을 자동으로 읽는다.
- Account file 경로는 `%ProgramData%\PureCVisor\desktop-node\accounts.json`, JWT signing key file 경로는 `%ProgramData%\PureCVisor\desktop-node\jwt-signing-key.txt`다.
- Service/MSI/Hyper-V/firewall/trust-store/LAN/Event Log/update/rollback mutation은 관리자 opt-in으로만 실행한다.
- 자동 reboot, scheduled-task reboot, 무승인 service mutation은 운영 절차가 아니다.
- Public trusted signing, trusted timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke는 `out-of-scope`다.
- 내부 운영 배포 gate는 `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`의 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke를 따른다.
- Web Console/API 기본 listener는 여전히 loopback HTTP 기준이다. HTTPS/TLS는 내부 installed smoke evidence로 추적하지만 public 443 publication을 의미하지 않는다.
- noVNC streaming bridge는 기본 disabled다. `--novnc-target-host`와 `--novnc-target-port`를 명시한 listener만 WebSocket-to-VNC TCP bridge를 노출하며, 실제 Windows console fallback은 Hyper-V `vmconnect` 기준이다.

## 기본 자산

| 항목 | 값 |
|------|----|
| Service name | `PureCVisorDesktopNode` |
| Web Console URL | `http://127.0.0.1/` |
| Web API URL | `http://127.0.0.1:7777/api/v1/...` |
| Product root | `C:\Program Files\PureCVisor\DesktopNode` |
| Command-line client | `pcvcli.exe` (`C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`) |
| Data root | `%ProgramData%\PureCVisor\desktop-node` |
| Host executable | `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe` |
| Product manifest | `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json` |
| Protected token file | `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` |
| Account file | `%ProgramData%\PureCVisor\desktop-node\accounts.json` |
| JWT signing key file | `%ProgramData%\PureCVisor\desktop-node\jwt-signing-key.txt` |
| Job store | `%ProgramData%\PureCVisor\desktop-node\jobs.json` |
| Event log JSONL | `%ProgramData%\PureCVisor\desktop-node\events.jsonl` |
| Diagnostic root | `%ProgramData%\PureCVisor\desktop-node\diagnostics` |
| Install log | `%ProgramData%\PureCVisor\desktop-node\install.jsonl` |

## 빠른 상태 확인

일상 운영에서 먼저 확인할 것:

```powershell
Get-Service PureCVisorDesktopNode
Get-Service vmms
Get-Item "C:\Program Files\PureCVisor\DesktopNode\product-manifest.json"
```

Repository checkout이 있는 운영자 환경에서는 product wrapper status를 사용한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

Web Console smoke:

```powershell
Start-Process "http://127.0.0.1/"
```

정상 기준:

- `PureCVisorDesktopNode` service 상태가 `Running`이다.
- VMMS가 Hyper-V 작업을 위해 `Running`이다.
- Product manifest version이 의도한 설치 version이다.
- Loopback Web Console이 열린다.
- 새 터미널에서 `pcvcli`가 전체 경로 없이 실행된다.
- API route 호출에는 bearer token이 필요하다.
- Account file이 `no-default-account` 상태이면 account login route가 service bearer gate를 우회하지 않는다.
- Port 80이 다른 서비스에 점유돼 있으면 Web Console listener가 시작되지 않을 수 있다. 새 설치본 evidence에서는 `http://127.0.0.1/`, `/pcv-config.js`, `http://127.0.0.1:7777/api/v1/...`를 함께 확인한다.

## 현재 운영자 Surface 여정

Web Console과 CLI는 같은 installed Local API와 backend contract를 사용한다. 2026-05-15
`docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`의 TUI 포함 여정은
historical predecessor로 보존한다.

| Surface | 운영자 목적 | 판단 기준 |
| --- | --- | --- |
| Web Console | Dashboard/Evidence current-card, diagnostics, operator handoff | `batch_evidence.status=available`, latest batch는 full admin operational evidence |
| CLI | package/update/diagnostics/operator command 실행 | token value를 출력하지 않고 기본 protected token file 또는 redacted artifact를 사용 |

## Account login / noVNC bridge follow-up

Installed account login smoke runner:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-smoke-<timestamp>
```

이 runner는 temporary operator account와 JWT signing key를 설치 data root에 쓰고 service restart 후 `auth.login`, `auth.session`, `auth.rbac`, `console.capabilities`를 확인한 뒤 원본 account/JWT 파일과 보호 ACL을 복원한다. 2026-05-10 관리자 실행 evidence는 `artifacts/installed-account-login-smoke-20260510-0410-final`이며 login/session/RBAC/console `200`, `runtime_auth_mode=account_rbac_jwt`, restore `restored`를 기록했다. 실행은 elevated operator opt-in이며 token/password/refresh token value를 출력하거나 evidence에 기록하지 않는다.

필요하면 같은 임시 account access token으로 설치본 Web Console browser QA를 이어서 실행할 수 있다. 이 옵션은 `summary.json`의 `browser_qa` block에 status, artifact root, screenshot/action/accessibility probe 요약을 기록하며 token 값은 기록하지 않는다.
설치본 frontend/backend auth console live smoke evidence는 `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`이며, `artifacts/installed-account-login-browser-live-smoke-20260510-235543`와 `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`에서 real account login form, session/RBAC/console route, diagnostic create/download, responsive screenshots를 PASS로 기록했다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 `
  -ArtifactRoot artifacts/installed-account-login-smoke-with-browser-qa-<timestamp> `
  -RunBrowserQa `
  -BrowserQaUrl http://127.0.0.1/
```

noVNC bridge는 기본 disabled다. Listener에 `--novnc-target-host`, `--novnc-target-port`, 선택적으로 `--novnc-websocket-path /api/v1/console/novnc/{vm_id}`를 명시해야 `websocket-to-vnc-tcp` bridge가 켜진다. Target host는 기본 loopback-only이며 non-loopback target은 explicit LAN mode 없이 거부된다.

Target-backed noVNC installed streaming smoke:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1 -ArtifactRoot artifacts/target-backed-novnc-installed-streaming-smoke-<timestamp>
```

이 runner는 loopback TCP echo target을 만들고 installed service `PathName`에 noVNC target을 임시로 붙인 뒤 WebSocket binary frame 왕복을 확인한다. 종료 시 원래 `PathName`을 복원하고 service를 `Running`으로 되돌려야 PASS다.

## 최신 기준 evidence

현재 운영 기준의 최신 PASS evidence:

| 범위 | 최신 evidence |
|------|---------------|
| Full admin host mutation gate | `0.42.30-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04230`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`, `artifacts/os-mutation-gates-batch-profile-20260518-04230`, full-gate/operational MSI SHA-256 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, superseded initial clean package MSI SHA-256 `c80be181ab99e9d9d5d7f59d7eb40c22841fa202dea36dcff549e5ba94552763`, provenance `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`, `AllowUnsignedDev`; installed listener `batch_evidence.status=available`, latest batch `full-admin-host-mutation-gate-20260518-04230`; current-card artifact `artifacts/installed-operator-surface-current-card-20260518-04230-r2`; Runtime/API current evidence `runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`; installed Web/TUI/CLI current-card PASS; 04229 및 이전 evidence는 historical 보존; public trusted signing 또는 외부 stable publication evidence가 아님 |
| Previous full admin host mutation gate | `0.42.25-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04225-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04225`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04225`, `artifacts/os-mutation-gates-batch-profile-20260516-04225`; full-gate MSI SHA-256 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`; historical predecessor로 보존한다. |
| Historical 04220 full admin host mutation gate | `0.42.20-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`; full-gate MSI SHA-256 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance `0895d018935298721b25b5d9ce1ae083a6690c25`; public trusted signing 또는 외부 stable publication evidence가 아니다. |
| Manual-admin package-pair campaign | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`, baseline `0.42.29-admin-smoke`, target `0.42.30-admin-smoke`, readiness, installed update/rollback, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary, descriptor generation v2, installed current-card recheck PASS; descriptor `artifacts/manual-admin-campaign-20260518-04229-04230/manual-admin-campaign-descriptor/summary.json`; target operational package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`, MSI SHA-256 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, update ZIP SHA-256 `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`, provenance `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`, descriptor `missing_count=0`, `not_pass_count=0`; 이전 `0.42.28-admin-smoke -> 0.42.29-admin-smoke` 및 earlier package-pair는 historical predecessor로 보존한다. |
| Latest product payload package build | `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04230.md`, `0.42.30-admin-smoke`, artifact `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`, MSI SHA-256 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`, provenance `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`; 초기 clean package `c80be181...`는 file version 보정 전 superseded artifact다. |
| Latest manual-admin package-pair | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`, descriptor batch `manual-admin-campaign-descriptor-20260518-04229-04230-closed`, summary `artifacts/manual-admin-campaign-20260518-04229-04230/manual-admin-campaign-descriptor/summary.json`, status `pass`; update ZIP SHA-256 `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`, descriptor `missing_count=0`, `not_pass_count=0`. |
| Previous 0.42.24 Runtime/API current evidence rollup | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`; full-gate batch `full-admin-host-mutation-gate-20260516-04224`, package build MSI SHA-256 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance `b974d6b541423f2e4160f726f96155b16f105e9d`; descriptor `manual-admin-campaign-descriptor-20260516-04223-04224`는 `missing_count=5`, `not_pass_count=1`로 blocked이며 04226 closure 이후 historical predecessor로 보존한다. |
| Previous product payload package build | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`, `0.42.22-admin-smoke`, artifact `artifacts/admin-smoke-package-20260516-04222`, MSI SHA-256 `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`, provenance `8a38995cc25a888f64473e9a2869740949ad6b24`; 이 package build는 full admin host mutation으로 승격했고, `0.42.21 -> 0.42.22` package-pair는 Burn idempotence blocker로 historical 보존한다. |
| Historical manual-admin package-pair anchors | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`, `0.42.12-admin-smoke -> 0.42.13-admin-smoke`, target MSI SHA-256 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`; `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`, `0.42.11-admin-smoke -> 0.42.12-admin-smoke`, target MSI SHA-256 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, update ZIP SHA-256 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`; `0.42.9-admin-smoke -> 0.42.11-admin-smoke` provenance `987beb51025a5aa926df7d9a905019b4d6d29705`, target MSI SHA-256 `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`; 모두 historical predecessor로 보존하며 public trusted signing 또는 외부 stable publication evidence가 아니다. |
| Ops summary selector guard package | `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`, package `0.42.14-admin-smoke`, artifact `artifacts/admin-smoke-package-20260514-04214-selectorfix`, MSI SHA-256 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`; 04218 follow-up current-card artifact `artifacts/installed-current-card-20260515-04218-fullgate`; latest batch `full-admin-host-mutation-gate-20260515-163107-04218`, `descriptor_excluded_from_operational_latest=true`; public trusted signing 또는 외부 stable publication evidence가 아님 |
| Clean-host Windows Update NoContact recovery guard | `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`; `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`는 Windows Update reboot 이후 heartbeat `NoContact` + CPU idle 상태가 `WindowsUpdateNoContactRecoverySeconds` 이상 지속되면 한 번만 `Stop-VM -TurnOff -Force; Start-VM`을 수행하고 `recovery_actions`를 summary에 남긴다. Guard code는 `AUTO-REPO`로 검증 가능하지만 실제 clean-host execution은 계속 `MANUAL-ADMIN`이다. |
| Post-04218 contract alignment | `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`; Runtime/Core API route diagnostics bridge, Hyper-V VM/checkpoint/network dispatch catalog, Host Ops lifecycle bucket, packaging next trigger, Web Console/TUI/CLI operator journey, ADR-0005/0006 public boundary를 `0.42.18-admin-smoke` 기준으로 정렬했다. 이 evidence는 host mutation performed `false`이며 package build, clean-host, full admin host mutation, public trusted signing, external stable publication을 실행하지 않는다. |
| Post-04212 follow-up triage | `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`; `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이 없어 `0.42.13-admin-smoke` package build, full admin host mutation, `0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign을 열지 않았다. Clean-host recovery summary key는 다음 실제 clean-host run의 `recovery_actions`와 `automatic_recovery_performed`로 판정한다. |
| Post-04212 `1-2-3-4-5` current-card follow-up | `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`; `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product payload 변경이 없어 package/host mutation chain은 보류했다. `artifacts/web-console-current-card-20260514-04212-rerun-followup`에서 Dashboard/Evidence view current-card smoke PASS, 당시 표시 batch `full-admin-host-mutation-gate-20260514-04212-rerun`, version `0.42.12-admin-smoke`, host mutation performed `false`. |
| Manual-admin next package-pair candidate | `pending-next-product-payload-after-04230-package-pair`; next product payload change까지 열지 않는다. 2026-05-16 `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`는 `0.42.25-admin-smoke -> 0.42.26-admin-smoke` initial blocked descriptor로 보존하고, current closed package-pair는 `0.42.29-admin-smoke -> 0.42.30-admin-smoke` PASS다. |

## 최근 Historical Anchor

- `0.42.24-admin-smoke` Runtime/API current evidence rollup predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`로 보존한다. Package MSI SHA-256은 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256은 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`다.
- `0.42.24-admin-smoke -> 0.42.25-admin-smoke` package-pair predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`, descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`로 보존한다. Target/full-gate MSI SHA-256은 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, update ZIP SHA-256은 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`, provenance commit은 `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`이다.
- `0.42.25-admin-smoke -> 0.42.26-admin-smoke` package-pair predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`, descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`로 보존한다. Target MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP SHA-256은 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`, provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`이다.
- `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`, descriptor `manual-admin-campaign-descriptor-20260517-04226-04227-closed`로 보존한다. Target MSI SHA-256은 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`, update ZIP SHA-256은 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`, provenance commit은 `69aba3eb3ff08c843f1a481818ddc86eac2f019b`이다.
- `0.42.27-admin-smoke -> 0.42.28-admin-smoke` package-pair predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md`, descriptor `manual-admin-campaign-descriptor-20260517-04227-04228-closed`로 보존한다. Target MSI SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, clean package MSI SHA-256은 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`, update ZIP SHA-256은 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`이다.
| 0.42.10 duplicate outer start RCA | `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`; `0.42.10-admin-smoke` target은 native service-action repair 이후 outer wrapper duplicate `sc.exe start`가 `1056 already running`을 반환해 historical-only로 보존; target MSI SHA-256 `bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273`, update ZIP SHA-256 `05a107f4803ec8ed1e08f7aeba1b49fa3795c7d16565db8f904fd599ba07633f`; `0.42.11-admin-smoke`가 `native-service-action-controls-final-state`로 닫았으며 current release/public evidence가 아님 |
| Post-0426 follow-up triage | `docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md`; Batch Supervisor `ManualAdminCampaignDescriptor` profile은 non-mutating descriptor step이고 helper는 `packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`; latest manifest id `manual-admin-campaign-descriptor-20260512-0427-0428`; 0423→0424 blocker는 historical-only로 보존; 사용자 승인 후 `0.42.7-admin-smoke` build/full admin host mutation gate/installed listener current-card smoke를 실행했고, 추가 승인으로 0427→0428 package-pair와 0428 full admin host mutation gate까지 PASS 완료 |
| Previous full admin host mutation gate | `0.42.9-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`, full-gate MSI SHA-256 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance `f0620f2e18ae25de8751333684cb74b5051dcdc6`, `AllowUnsignedDev`; 04211 full gate 이후 historical predecessor로 보존; public trusted signing 또는 외부 stable publication evidence가 아님 |
| Manual-admin operator/hardening follow-up | `0.41.5-admin-smoke`, `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `artifacts/manual-admin-followup-20260510-0415`; installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, HTTPS/TLS lifecycle, Event Log default transition PASS; Lifecycle/Packaging current rebaseline PASS |
| Lifecycle/Packaging current rebaseline | `0.41.5-admin-smoke` to `0.41.6-admin-smoke`, `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`; installed update/rollback PASS, internal clean-host update/rollback PASS |
| MSI/update package installed apply | `0.39.1-admin-smoke`, `artifacts/msi-update-package-20260509-0391` |
| Burn bootstrapper lifecycle smoke | `0.41.6-admin-smoke`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416`, bundle SHA-256 `5e67bd3a1fed7262447531000328825180fd678b252170793cf88e50fc41535d` |
| Windows Event Log provider/default writer transition | `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `artifacts/windows-event-log-default-transition-installed-20260510-0396`, provider register/write PASS plus installed default writer repair/remove/volume/schema PASS |
| Windows Credential Manager default transition | `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `artifacts/windows-credential-manager-default-transition-installed-20260510-0395`, `installed-local-system-default-transition-pass`, service default transition PASS |
| Internal HTTPS/TLS lifecycle installed smoke | `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`, `artifacts/internal-https-tls-lifecycle-installed-20260510-0397`, certificate generate/bind/rotate/remove PASS, final service restored |
| Internal clean-host install/update/rollback smoke | `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, `artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417`, `pass` |
| Internal private network distribution matrix | `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`, ADR-0006, public signing/winget/external upload/public clean-host smoke `out-of-scope` |
| Account/RBAC/JWT console code-level | `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`, local account file/JWT route, RBAC role gate, Web Console session UX, vmconnect handoff, noVNC default disabled until explicit target, installed account login smoke PASS in `artifacts/installed-account-login-smoke-20260510-0410-final` |
| Installed account login and noVNC bridge | `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`, installed smoke PASS, noVNC WebSocket-to-VNC TCP bridge code-level PASS |
| Target-backed noVNC and installed TUI operator smoke | `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`, `artifacts/target-backed-novnc-installed-streaming-smoke-20260510-0411`, `artifacts/installed-tui-operator-smoke-20260510-0411`, `0.41.1-admin-smoke`, both PASS |
| Web/API port split | `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`, `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`, Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...`, installed listener PASS |
| Public distribution historical blocked scan | `artifacts/public-external-gates-blocked-20260509-0391`, ADR-0005 closed-not-adopted history only |
| Public ops final 1-7 follow-up attempt | `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, historical prerequisite scan, not current release blocker |
| Public ops gate execution readiness | `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`, historical readiness descriptor with TLS `partial-code-level-cert-generate-rotate-delete-pass`; internal TLS follow-up moved to ADR-0006 matrix |
| Public ops installed hardening | `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`, Credential Manager SYSTEM proof/default transition PASS, Event Log default writer hardening PASS, internal HTTPS smoke PASS |
| Follow-up queue / automated batch classification | `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`, unattended automation limited to non-mutating repo regressions, preflight descriptors, and dedicated installed read-only checks |
| MSI/service installed listener | `0.39.0-admin-smoke`, `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390` |
| Installed listener OS mutation gate | `0.39.0-admin-smoke`, `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390` |
| Internal MSIX package lifecycle smoke | `0.41.5-admin-smoke` baseline payload and `0.41.6-admin-smoke` target payload, `artifacts/msix-package-lifecycle-smoke-20260510-0416` |
| Config/job store migration apply | `0.38.6-admin-smoke`, `artifacts/config-jobstore-migration-apply-installed-20260507-0386` |
| Update/rollback destructive smoke | `0.38.8-admin-smoke`, `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass` |
| Internal enterprise signed MSI build | `0.38.7-rc.1`, `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387` |

이 evidence는 내부 서비스 운영 근거다. Public trusted signing 또는 external stable publication 근거로 해석하지 않는다.

## 설치본 listener 확인

`0.39.0-admin-smoke` installed listener 기준 SCM `PathName`에는 다음 인자가 있어야 한다.

- `--diagnostics-root`
- `--api-token-protected-file`
- `--account-file`
- `--jwt-signing-key-file`
- `--route-timeout-seconds 30`
- `--request-limit-per-minute 120`
- `--request-burst-limit 20`
- `--retry-after-seconds 15`

확인 명령:

```powershell
Get-CimInstance Win32_Service -Filter "Name='PureCVisorDesktopNode'" |
  Select-Object Name, State, StartMode, PathName
```

Installed listener diagnostic bundle evidence는 `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`와 다음 artifacts를 따른다.

- `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/installed-service-listener-post-rerun.json`
- `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/installed-diagnostic-bundle-listener-smoke.json`

PASS 기준:

- Diagnostic bundle create는 HTTP `201`이다.
- Download는 HTTP `200`이다.
- `X-PCV-Diagnostic-Bundle-Id`가 생성 bundle id와 일치한다.
- Downloaded bundle은 `[REDACTED]`를 포함한다.
- Downloaded bundle은 test secret을 포함하지 않는다.

## LAN/firewall/Event Log/trust-store gate

LAN, firewall, Event Log, trust-store 작업은 기본 운영 경로가 아니다. 사용자 관리자 opt-in과 elevated shell이 있을 때만 실행한다.

최신 focused OS mutation gate evidence:

- 문서: `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`
- Batch root: `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`
- OS root: `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`

PASS 기준:

- Batch summary `ok=true`
- Firewall enable/remove completed
- Final firewall rule count `0`
- LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy and Web assets HTTP `200`
- Event Log register/remove completed
- Final Event Log source absent
- ADR-0003 internal Root/TrustedPublisher install/remove/restore completed
- Internal trust certs present after restore
- Boot time unchanged
- `public_trusted_signing=excluded`
- `external_stable_publication=not-claimed`

## Diagnostic bundle 운영

Diagnostic bundle은 장애 분석용 redacted archive다.

운영 기준:

- Output root는 `%ProgramData%\PureCVisor\desktop-node\diagnostics`다.
- API route는 bearer token을 요구한다.
- Archive는 redaction을 적용해야 한다.
- Token value와 Authorization header value는 bundle에 남기지 않는다.
- Bundle download route는 저장된 bundle만 제공한다.

Web Console Troubleshooting 화면은 diagnostic bundle handoff를 보여준다. Host mutation, firewall, trust-store, LAN, Event Log, MSI lifecycle을 자동 실행하지 않는다.

Repository checkout이 있는 운영자 환경에서 product wrapper를 사용한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
```

실행 후 확인:

- 생성된 bundle path가 diagnostics root 아래다.
- Redaction marker가 있다.
- Token, Authorization header, host secret이 없다.
- 필요하면 evidence 문서와 artifact path를 incident record에 연결한다.

## VM/Checkpoint 운영

VM lifecycle과 checkpoint 작업은 queued job으로 실행된다.

기본 확인:

- Web Console `Virtual Machines`에서 VM 목록과 state를 확인한다.
- `Jobs` 또는 `Activity`에서 job status를 확인한다.
- 실패 시 `PCV_*` error code와 request/correlation id를 기록한다.

주의:

- VM create는 현재 Hyper-V Generation 2 product path만 지원한다.
- Generation 1 request는 `PCV_GENERATION_INVALID` structured failure다.
- VM delete는 PureCVisor managed marker가 있어야 한다.
- Unmanaged VM delete는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단되어야 한다.
- Checkpoint restore는 VM state에 민감하다. 검증된 smoke는 `vm.poweroff-before-restore` 조건을 사용했다.
- Guest shutdown integration이 없으면 `PCV_VM_SHUTDOWN_NOT_AVAILABLE` structured failure가 정상적인 차단 결과일 수 있다.

## Job store와 retention

`GET /api/v1/jobs`는 server-side job list를 paged response로 반환한다.

운영 기준:

- 기본 page는 `limit=50&offset=0`이다.
- 최대 limit은 `200`이다.
- Terminal job `succeeded`, `failed`, `canceled`는 최신 500개를 보존한다.
- Active job `queued`, `running`은 보존한다.
- Persisted job store load도 같은 retention cap을 적용한다.

장애 분석 시 먼저 확인할 것:

- `jobs.json` 존재와 schema
- `jobs.json.commit-pending` 존재 여부와 access 가능 여부
- Active job 수
- Failed job error code
- Request/correlation id
- Service restart 전후 active job recovery 상태

### Pending-commit recovery 절차

`PCV_JOB_STORE_SAVE_FAILED`의 indeterminate detail 또는 `PCV_JOB_STORE_LOAD_FAILED`가 보이면
`jobs.json.commit-pending`을 단순 삭제하지 않는다. 이 marker는 candidate와 previous primary의
exact length/SHA-256을 보존해 current runtime이 어느 snapshot이 authoritative인지 판정하는 guard다.
고정 marker, `jobs.json.tmp.<GUID-N>`와 `jobs.json.commit-pending.tmp.<GUID-N>`를 primary로
승격하거나 stale backup을 자동 복원하지 않는다.

일반 복구 순서:

1. Web/CLI mutation, migration, update/rollback과 제거 작업을 중단한다. Desktop Node service를
   stop하고 `Stopped`를 확인한다. 같은 data root를 사용하는 다른 Host/maintenance writer가 없어야
   한다.
2. 설치 version, current `DesktopNode.Host.exe` SHA-256, service 상태와 Event Log를 기록한다.
   `jobs.json`, fixed marker와 존재하는 GUID temp를 별도 incident evidence directory에 복사하고 각
   파일의 exact byte length, SHA-256, UTC timestamp, owner/ACL과 원래 path를 기록한다. 원문에는
   request parameter나 secret이 있을 수 있으므로 public evidence에는 hash/length만 싣는다.
3. Marker를 read-only로 검사한다. JSON object, `version=1`, nonnegative candidate length, 64-hex
   candidate SHA-256, boolean `previous_exists`를 요구한다. Previous가 존재하면 length와 64-hex
   SHA-256도 필요하다. Writer가 완전히 멈춘 상태에서 primary length/SHA-256을 계산해 candidate
   또는 previous identity와 exact match하는지 확인한다.
4. Access/ACL 문제가 원인이면 파일 내용을 바꾸지 말고 승인된 ACL 복구만 수행한다. Queue dispatch
   재개 승인이 끝난 뒤 marker-aware current binary를 시작한다. Current runtime은 primary가 candidate
   또는 previous identity와 정확히 일치할 때 fixed marker를 자동 제거하고 해당 primary를 load한다.
5. API job/queue projection, `loadBlock` 해제와 예상 밖 native invoke 0을 확인한다. Candidate match는
   HTTP response가 유실됐을 수 있으므로 correlation/job을 조회하고 원 mutation을 blind resubmit하지
   않는다. Previous match도 외부 side effect 상태를 확인하기 전 자동 retry하지 않는다.

다음 조건에서는 서비스를 계속 stopped로 두고 incident별 forensic 승인을 요청한다.

- marker 또는 primary가 unreadable/malformed다.
- primary가 candidate/previous identity 어느 쪽에도 일치하지 않는다.
- 현재 runtime restart 뒤에도 fixed marker가 남거나 load block이 유지된다.
- job ID/queue/status/result/error/attempt 의미 무결성 또는 persisted running job의 외부 side effect가
  불명확하다.

일반 runbook은 위 상태에서 marker 이동·삭제나 primary 편집을 승인하지 않는다. Exact identity와
job/queue 의미 무결성을 별도 검증하고 incident 승인자가 dispatch 재개를 승인한 경우에만, 서비스를
stopped로 유지한 채 marker를 같은 directory의 충돌 없는 timestamped quarantine 이름으로 원자적
rename하는 별도 offline recovery record를 작성할 수 있다. 전/후 hash, 판정(candidate/previous),
승인자와 current binary hash를 남기며 `jobs.json`과 orphan temp는 편집하지 않는다. `W0-FI-04`
semantic validator가 완료되기 전에는 malformed/identity-mismatch를 이 예외 절차로 처리하지 않는다.

Marker가 있는 동안 Update, Rollback, job-store migration과 preserve-data RemoveInstalled/Uninstall은
fail-closed하는 것이 정상이다. 제거 시 service가 stopped로 남았다면 같은 current marker-aware
binary를 다시 시작해 reconciliation한 후 작업을 재시도한다. 0.42.65 같은 marker-unaware 구 binary를
시작하지 않는다. `RemoveData=true`는 primary와 marker를 영구 삭제하는 별도 명시적 데이터 파기
승인이므로 recovery 절차가 아니다.

## Config/job store migration apply

Config/job store migration apply는 installed destructive admin smoke에서 PASS한 product operation이다.

최신 PASS evidence:

- `artifacts/config-jobstore-migration-apply-installed-20260507-0386`
- version `0.38.6-admin-smoke`
- final service `Running`
- product manifest schema `2`
- job store schema `2`
- boot time unchanged
- post-migration API read ok

운영 기준:

- 지원되는 migration plan/version에서만 write를 수행한다.
- Service stopped precondition과 runtime writer stopped proof가 필요하다.
- `jobs.json.commit-pending`이 없어야 하며 marker 존재/검사 실패는 backup과 rewrite 전에 차단한다.
- Backup과 rollback/recovery diagnostics를 남겨야 한다.
- Implicit service stop/start, token mutation, service identity mutation, MSI/update/rollback, Hyper-V/firewall/trust-store/LAN/Event Log mutation을 함께 실행하지 않는다.
- Service running 상태에서 apply가 차단되는 것은 기대 동작이다.

## Update와 rollback

Update는 manifest-first safe update 정책을 따른다. Rollback은 이전 product root 복원을 시도하고 diagnostics root에 실패 root를 보존한다.

최신 MSI/update package apply PASS evidence:

- `artifacts/msi-update-package-20260509-0391`
- version `0.39.1-admin-smoke`
- MSI SHA-256 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`
- update ZIP SHA-256 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`
- provenance commit `8f0c4b6fbac8787932d0e966437fcc62d86e6068`
- MSI exit code `0`
- installed product manifest `0.39.1-admin-smoke`
- final service `Running`
- loopback Web Console HTTP `200`
- `public_trusted_signing=excluded`
- `external_stable_publication=not-claimed`

최신 installed destructive PASS evidence:

- `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`
- update `0.38.6-admin-smoke -> 0.38.8-admin-smoke`
- rollback after update
- final service `Running`
- final product root manifest `0.38.6-admin-smoke`
- failed root manifest `0.38.8-admin-smoke`
- update journal `succeeded/health`
- boot time unchanged

운영 기준:

- File/HTTPS ZIP source는 SHA-256 source gate를 통과해야 한다.
- Catalog update는 schema, product id, selected channel, package URI, SHA-256을 service stop 전에 검증해야 한다.
- Remote package는 extract-before-service-stop preflight를 통과해야 한다.
- Service stop 실패, source gate 실패, health 실패는 structured diagnostics로 남긴다.
- Clean-host public signed update/rollback smoke는 ADR-0006 기준 out-of-scope다. 내부 clean-host install/update/rollback PASS는 `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`가 소유하며, current 0.41.5 to 0.41.6 Lifecycle/Packaging rebaseline PASS는 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`가 소유한다. 2026-05-14 이후 clean-host runner는 Windows Update reboot 후 `NoContact` + CPU idle 상태를 자동 recovery guard로 기록한다.

## Internal signing 운영

현재 내부 운영 signing policy는 ADR-0003을 따른다.

최신 internal enterprise `RequireSigned` evidence:

- `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`
- version `0.38.7-rc.1`
- Authenticode `Valid`
- SignTool verify exit `0`
- MSI SHA-256 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`

주의:

- Internal Root/leaf trust model은 public trusted signing이 아니다.
- `AllowUnsignedDev` admin-smoke evidence는 내부 검증 evidence일 뿐 외부 배포 evidence가 아니다.
- ADR-0005는 closed-not-adopted 이력이며 public trusted signing은 ADR-0006 기준 out-of-scope다.

## MSIX package lifecycle smoke

MSIX는 최신 evidence에서 internal smoke package lifecycle만 PASS했다. 현재 일반 운영 배포 경로는 여전히 MSI/service first다.

최신 PASS evidence:

- `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`
- `artifacts/msix-package-lifecycle-smoke-20260510-0416`
- baseline source payload: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415/payload`
- target source payload: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/target-0416/payload`
- v1 package version: `0.41.5.0`
- v2 package version: `0.41.6.0`
- v1 SHA-256: `c2efc20e29d950f4e2abd924c13c003cb734bc46e95ccd5aacdd7a724a188674`
- v2 SHA-256: `8329e0af985185515dac65353398763f5951852faecc928b9925de6fb03dc871`

PASS 기준:

- MSIX build/sign/verify completed for v1 and v2.
- Install of v1 completed.
- Update to v2 completed.
- Remove completed.
- Final `PureCVisor.DesktopNode.MsixSmoke` package is absent.
- Final `PureCVisorDesktopNodeMsixSmoke` service is absent.
- Existing MSI service `PureCVisorDesktopNode` remains `Running`.

운영 해석:

- This is internal Root/leaf signing and restricted packaged service capability evidence.
- It uses the separate smoke identity `PureCVisor.DesktopNode.MsixSmoke`, not the production MSI identity.
- It does not replace MSI-first internal service distribution.
- It does not claim Store submission, public MSIX publication, public trusted signing, or external stable publication.
- Do not run the smoke package on production hosts outside an approved elevated admin-smoke window.

## 장애 대응 흐름

1. Service 상태 확인

```powershell
Get-Service PureCVisorDesktopNode
Get-CimInstance Win32_Service -Filter "Name='PureCVisorDesktopNode'" |
  Select-Object Name, State, StartMode, PathName
```

2. Hyper-V와 VMMS 확인

```powershell
Get-Service vmms
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All
```

3. Product status 확인

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

4. Web Console 확인

```powershell
Start-Process "http://127.0.0.1/"
```

5. Job과 error code 확인

- Web Console `Activity`
- Web Console `Troubleshooting`
- `%ProgramData%\PureCVisor\desktop-node\jobs.json`
- `%ProgramData%\PureCVisor\desktop-node\events.jsonl`

6. Diagnostic bundle 수집

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
```

7. Evidence 연결

Incident record에는 다음을 함께 남긴다.

- Product manifest version
- Service state and `PathName`
- Relevant `PCV_*` error code
- Job id and request/correlation id
- Diagnostic bundle path
- 적용한 artifact root 또는 evidence doc
- Host mutation 여부
- Public claim 여부

## 정기 점검

주간 또는 release candidate 검토 시 확인할 것:

- Service `Running`
- VMMS `Running`
- Product manifest version이 의도한 release와 일치
- SCM `PathName`이 diagnostics/hardening args 포함
- Web Console loopback open
- Diagnostic bundle create/download/redaction evidence 최신성
- Firewall final rule count가 의도한 상태
- Event Log source final state가 evidence와 일치
- Internal Root/TrustedPublisher final state가 ADR-0003 운영 정책과 일치
- Boot time unchanged 여부
- `remaining_pcv_vms=[]` 여부
- Public trusted signing/external stable publication이 잘못 claim되지 않았는지 확인

## 절대 하지 말 것

- Token 값을 command line 인자나 문서에 적지 않는다.
- Protected token file 내용을 출력하지 않는다.
- Public trusted signing evidence 없이 public signing 완료를 주장하지 않는다.
- External stable publication evidence 없이 외부 배포 완료를 주장하지 않는다.
- Non-elevated blocked attempt를 PASS evidence로 승격하지 않는다.
- Code-level evidence를 installed listener/host mutation evidence로 해석하지 않는다.
- Dry-run/preflight descriptor를 actual execution으로 해석하지 않는다.
- Service/MSI/firewall/trust-store/LAN/Event Log mutation을 한 command 안에 임의로 섞지 않는다.
- Automatic reboot를 실행하지 않는다.
- Linux `purecvisor-single`, `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime을 이 저장소 운영 절차에 포함하지 않는다.

## Public/External 이력 및 내부 follow-up 상태

다음은 ADR-0005 closed-not-adopted 이력과 ADR-0006 내부 운영 evidence 상태다. Public gate는 현재 내부 전용 서비스 완료 조건이 아니다.

| 항목 | 현재 상태 |
|------|-----------|
| Public trusted signing | ADR-0006 out-of-scope; internal root/leaf signing only; public release `not-claimed` |
| External stable publication | ADR-0006 out-of-scope |
| Public distribution ops execution bundle | `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`, local non-mutating bundle PASS, public claims unchanged |
| Winget submission | offline validation PASS; public submission out-of-scope/not run |
| Public updater catalog publication | external publication out-of-scope/not published; internal updater catalog/channel code-level PASS |
| Public signed update/rollback clean-host smoke | out-of-scope; internal clean-host install/update/rollback PASS |
| Burn bootstrapper | internal lifecycle smoke PASS, `build-install-repair-remove-pass-internal-smoke`; public signing/publication not claimed |
| Public MSIX/Store publication | internal lifecycle smoke PASS; public publication not claimed |
| Windows Credential Manager transition | current-user capability PASS plus installed LocalSystem service default transition PASS |
| Default Windows Event Log writer/provider transition | installed default writer repair/remove/volume/schema PASS |
| Built-in TLS certificate lifecycle | ADR-0005 public/built-in preflight preserved; ADR-0006 internal HTTPS/TLS lifecycle installed PASS |
| Service token rotation/revoke mutation API | installed admin-smoke PASS |
| Installed/external timeout/rate-limit load generator | installed listener external load/rate-limit PASS; external/public load publication not claimed |

자동 배치 작업 분류는 `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`를 따른다. `AUTO-REPO`, `AUTO-PREFLIGHT`, `AUTO-INSTALLED-READONLY`만 무인/전용 runner 후보이며, `MANUAL-ADMIN`은 elevated operator opt-in과 해당 run의 명시 승인이 필요하고, `BLOCKED-EXTERNAL`은 ADR-0006 기준 자동 배치 대상이 아니다.

## 문서와 evidence 업데이트 기준

운영 evidence를 추가한 뒤에는 다음 문서를 함께 확인한다.

- `docs/DEVELOPER_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`
- `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- 해당 evidence doc

문서 guard는 다음 명령으로 확인한다.

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
git diff --check
```
