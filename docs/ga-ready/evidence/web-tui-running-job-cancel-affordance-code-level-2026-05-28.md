# Web/TUI running job cancel affordance code-level 2026-05-28

evidence_id: `web-tui-running-job-cancel-affordance-code-level-2026-05-28`
result: `PASS_CODE_LEVEL_PROMOTED_BY_04255_PACKAGE`
scope: `web-tui-running-guest-execution-cancel-affordance`
product_payload_change_detected: `true`
host_mutation_performed: `false`
package_build_performed: `0.42.55-admin-smoke`
manual_admin_package_pair_performed: `false`
installed_promotion_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 변경

- Web Console job/activity rows에서 running `guest.exec` job cancel button을 `Cancel running guest exec`로 표시한다.
- Web Console cancel button에 `data-job-cancel-scope="running-guest-execution"`을 추가하고,
  해당 scope는 `running guest execution cancel` RBAC context로 검사한다.
- TUI Job tab에서 running `vm.guest.exec` job cancel 확인 문구를
  `Confirm Cancel Running Guest Execution <job>`로 표시한다.

## 검증

| 검증 | 결과 |
| --- | --- |
| `npm run build:served --prefix web` | `PASS` |
| `Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed` | `PASS`, 48/48 |
| `dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj --filter FullyQualifiedName~SelectedJobCancelRequiresConfirmationBeforeMutationAndEnterCancelsJob` | `PASS`, 1/1 |

## 경계

이 evidence는 code-level product payload change다. 0.42.54 full admin host mutation은 이 변경
이전 commit을 검증했고, 이후 0.42.55 package/fullgate/current-card에서 설치본으로 승격했다.
