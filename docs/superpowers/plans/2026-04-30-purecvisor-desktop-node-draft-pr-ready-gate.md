# PureCVisor Desktop Node Draft PR Ready Gate

## 목적

이 문서는 현재 draft PR을 ready로 전환하기 전에 닫아야 하는 evidence gate를 한 곳에 정리한다. 목표는 PR 상태 판단을 자동화하거나 우회하는 것이 아니라, 이미 수집한 evidence와 아직 관리자 opt-in이 필요한 evidence를 분리하는 것이다.

이 문서는 Desktop Node GA 승격을 선언하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`와 `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`를 유지한다.

## 현재 기준

- 작성 기준: 2026-04-30
- 문서 작성 시작 head 기준: `66951d3`
- 최신 evidence 수집 기준 head: `65447f4`
- firewall smoke rule cleanup evidence: `artifacts/p2-firewall-smoke-rule-removal-20260430-2030`
- release approval/signing preflight evidence: `artifacts/p1-release-approval-and-signing-preflight-20260430-2045`
- local self-signed/root trust workaround evidence: `artifacts/p0-local-root-trust-workaround-20260430-2120`
- operational/Event Log lifecycle evidence: `artifacts/p1-operational-eventlog-lifecycle-20260430-2050`
- admin opt-in service/Hyper-V/firewall/LAN/TLS operational hardening evidence: `artifacts/admin-optin-continuation-20260501-162940`
- WinSW wrapper service restore evidence: `artifacts/admin-optin-service-wrapper-restore-20260501-162904`
- TLS reverse proxy preview evidence: `artifacts/admin-optin-tls-reverse-proxy-preview-20260501-163308`
- latest admin opt-in Hyper-V/service/MSI/firewall/Event Log evidence: `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`
- .NET Windows Service Host replacement evidence: `artifacts/dotnet-host-admin-smoke-20260501-213444`
- PR 본문 head 기준: push 후 최신 commit으로 갱신한다.
- PR 상태: ready 전환 대상
- 자동 reboot: 금지. `-Reboot` 요청은 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 실패해야 한다.
- host mutation: MSI lifecycle, service install/start/stop/delete, Hyper-V VM 생성/삭제, firewall rule, Event Log source 등록, Task Scheduler 등록은 명시적 관리자 opt-in으로만 실행한다.

## Ready 전환 조건

| Gate | 현재 상태 | Ready 전환 조건 | 비고 |
| --- | --- | --- | --- |
| Signed RC MSI build / release approval | release approval 및 local trust workaround 기록 완료 | signed RC artifact, provenance, SHA-256, SignTool 결과가 PR evidence에 기록되어 있고, public trusted cert가 없으면 release approval evidence가 있어야 한다. | `0.23.8-rc.1` local test certificate evidence는 public trusted/stable signing evidence가 아니다. `artifacts/p1-release-approval-and-signing-preflight-20260430-2045`에서 SignTool은 있으나 code-signing cert/PFX 후보가 없음을 기록했고, 사용자 명시 승인으로 release approval gate를 닫았다. `artifacts/p0-local-root-trust-workaround-20260430-2120`에서 CurrentUser Root/TrustedPublisher trust workaround 뒤 Authenticode `Valid`, SignTool verify exit `0`을 확인했다. stable unsigned build는 `PCV_INSTALLER_RELEASE_SIGNING_REQUIRED`로 계속 차단된다. |
| Signed MSI lifecycle | internal enterprise 보강 smoke 완료 | 같은 signed MSI로 install, repair, uninstall, `REMOVE_DATA=1` uninstall이 모두 계약상 성공이어야 한다. | `artifacts/p0-signed-msi-lifecycle-rerun-20260430-191040`에서 local RC lifecycle을 닫았고, `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`에서 internal enterprise `RequireSigned` `0.23.10-rc.1` MSI install/repair/uninstall/install-remove-data/uninstall-remove-data/final restore가 모두 성공했다. 자동 reboot는 비활성/미관측이었다. |
| MSI repair reboot-required policy | 문서화 완료 | `0`, 조건부 `3010`, 실패/중단 `1641` 계약이 docs/tests/PR body에 반영되어 있어야 한다. | `1641`은 실제 reboot initiated 결과이므로 성공으로 닫지 않는다. |
| Post-reboot verification | current-head evidence 완료 | reboot 이후 service/web/token/runtime 상태와 continuation profile evidence가 기록되어야 한다. | `artifacts/p1-post-reboot-verification-current-head-20260430-191839`에서 ProductStatus 후 PackagingRegression과 HyperVNonIntegration continuation이 성공했다. 자동 reboot와 Task Scheduler 등록은 실행하지 않았다. |
| Hyper-V product-flow lifecycle | product-flow 및 direct 보강 smoke 완료 | product API VM create/start/checkpoint/poweroff/remove 또는 cleanup, failure/retry/job-store consistency evidence가 있어야 한다. | `artifacts/phase21-product-flow-rerun-20260430-190840`에서 checkpoint raw evidence 3종과 `verified_visible` assessment, cleanup 완료를 기록했다. `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`에서 direct Hyper-V create/start/checkpoint list/poweroff/remove도 성공했고 checkpoint list와 direct snapshot 모두 이름을 확인해 이전 checkpoint list 누락 이슈를 닫았다. 실패 job retry는 현 계약상 `409/PCV_JOB_NOT_RETRYABLE`로 확인했다. |
| Web Console QA | 완료 | anonymous/authenticated smoke, console/page error 결과가 기록되어 있어야 한다. | `artifacts/p2-web-console-qa-20260430-102021`에서 loopback root, auth-required 기본 상태, navigation/empty state, console error 0을 확인했다. 브라우저 폼에는 장기 token을 저장하지 않았다. |
| Security/operations | hardening evidence 보강 완료 | token redaction, protected token file, firewall opt-in, Event Log opt-in, ProgramData ACL ownership, long-lived service inline-token 거부가 docs/tests에 반영되어야 한다. | `artifacts/p2-security-ops-check-20260430-192500`에서 service/token/listener/ACL은 기대값이었다. 사용자 명시 승인 후 `artifacts/p2-firewall-smoke-rule-removal-20260430-2030`에서 enabled inbound rule `PureCVisor Desktop Node API Smoke`를 제거했고 after count `0`을 확인했다. `artifacts/p1-operational-eventlog-lifecycle-20260430-2050`에서 기존 service Running, ProductStatus/CollectDiagnostics exit `0`, Web root `200`, Event Log source register/write/read/remove lifecycle 성공을 기록했다. 이후 `artifacts/admin-optin-continuation-20260501-162940`에서 WinSW service reinstall/start, SCM failure action apply, protected token ACL inspection, firewall create/update/delete, Event Log scoped source lifecycle, LAN listener/firewall preview, direct Hyper-V lifecycle, Product API Hyper-V lifecycle, 75초 운영 sampling을 통과했다. `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`에서 final service `Running`, Web root `200`, final failure action 조회, firewall smoke cleanup count `0`, Event Log source write/read/remove, 자동 reboot 비활성/미관측을 재확인했다. `artifacts/admin-optin-tls-reverse-proxy-preview-20260501-163308`에서 trust store 변경 없이 self-signed TLS reverse proxy preview를 통과했고 private key cleanup을 확인했다. |
| Phase 25 .NET/TypeScript slices | 충족 | .NET contract/runtime/API/service/host/route parity와 TypeScript parity scaffold/verification flow가 keep-spike/GA 별도 판단 조건에서 검증되어야 한다. | 초기 `src/DesktopNode.Contracts/**`, `src/DesktopNode.Runtime/**`, `src/DesktopNode.Api/**`, `src/DesktopNode.Service/**`, Web TypeScript scaffold는 side-by-side 후보였다. `src/DesktopNode.Host/**`는 2026-05-01 replacement slice에서 기본 제품 service host와 MSI installed action runner가 됐다. Route parity 시작 slice는 `src/DesktopNode.Api/**` helper-backed routes와 queued job runtime을 추가했고, 2026-05-02에는 `host.status` C# native read adapter와 guarded `network.inventory` C# native-first/helper-fallback read adapter가 완료됐다. `artifacts/dotnet-host-admin-smoke-20260501-213444`는 unsigned admin-smoke evidence이며 GA를 의미하지 않는다. |

## Ready 전환 판단

2026-05-01 후속 evidence 기준으로 draft-ready 직접 차단 사유는 해소됐다.

1. Public trusted/stable signing evidence는 없지만, SignTool/certificate preflight와 사용자 release approval evidence가 남았다.
2. 장기 운영/Event Log provider/source lifecycle evidence가 수집됐다.
3. firewall smoke rule cleanup evidence가 남았다.
4. service/Hyper-V/firewall/Event Log/LAN/TLS preview hardening evidence가 추가로 남았다.
5. 최신 admin opt-in 보강 evidence는 internal enterprise `RequireSigned` MSI lifecycle, final service restore, Web root, Hyper-V checkpoint list/direct snapshot visibility, firewall cleanup, Event Log source lifecycle을 재확인했다.
6. local test certificate 기반 `0.23.8-rc.1` evidence는 public trusted release evidence로 승격하지 않는다.
7. local self-signed/root trust 우회와 self-signed TLS preview는 host-local verification workaround이며 public trusted release evidence로 승격하지 않는다.
8. Internal enterprise `RequireSigned` evidence는 내부 서비스 운영용이며 public trusted signing/stable publication으로 승격하지 않는다.
9. 제품 service host replacement는 `DesktopNode.Host.exe` 기본 경로로 완료됐지만 unsigned admin-smoke evidence 범위다. Stable publication과 GA 제품 런타임 승격은 별도 판단이다.

## 허용되는 후속 작업

관리자 opt-in 없이 이어갈 수 있는 후속 작업:

- docs/test guard 정리
- PR body 최신 head/evidence 반영
- .NET/TypeScript 후보와 .NET Host replacement 설계 문서화
- non-mutating Pester/xUnit/node syntax 검증
- existing evidence classifier 또는 redaction rule 테스트

명시적 관리자 opt-in 전에는 실행하지 않는 작업:

- signed MSI install/repair/uninstall/`REMOVE_DATA=1`
- Hyper-V VM 생성/시작/checkpoint/poweroff/remove
- service install/start/stop/delete
- Windows Firewall rule ensure
- Windows Event Log source registration
- Task Scheduler registration
- `Restart-Computer` 또는 자동 reboot

## 검증 후보

문서 또는 side-by-side code 후속 변경 후 기본 검증:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

제품 wrapper, installer, API, Web Console, Hyper-V helper 계약을 건드린 경우 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`의 영향 범위에 맞춰 suite를 추가한다.
