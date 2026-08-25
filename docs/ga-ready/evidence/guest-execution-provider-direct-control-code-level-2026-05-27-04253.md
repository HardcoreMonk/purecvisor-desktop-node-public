# Guest Execution provider/direct-control code-level 2026-05-27 0.42.53

evidence_id: `guest-execution-provider-direct-control-code-level-2026-05-27-04253`
result: `PASS_CODE_AND_INSTALLED_PROMOTED`
scope: `guest-execution-provider-channel-verify-repair-web-tui-direct-control`
version: `0.42.53-admin-smoke`
package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-27-04253.md`
fullgate_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-27-04253-hostmutation.md`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-27-04253.md`
manual_admin_readiness: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-27-04250-04253-readiness-blocked.md`
provenance_commit: `cc774b257d6cd772c3a890266aca62aa8ab8eadc`
host_mutation_performed: `true-via-04253-fullgate`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 구현 범위

이번 slice는 ADR-0009의 deferred boundary를 제품 기능으로 한 단계 열었다.

| 영역 | 결과 |
| --- | --- |
| Hyper-V provider | `IDesktopNodeHyperVGuestExecutionProvider`와 PowerShell Direct 기반 provider 추가 |
| Credential resolver | `wincred:`, `credential-manager:`, `dpapi:` reference만 허용 |
| Audit/redaction | `guest-execution-audit-v1`, `guest-execution-redaction-v1` contract를 유지하며 raw credential/secret echo 없이 hash, byte count, digest 중심 결과 유지 |
| Timeout/cancel | provider timeout은 구현, running interrupt는 아직 `false`; queued job cancel만 계약 |
| API | `/guest/exec`, `/guest/channel/verify`, `/guest/channel` queued mutation route 추가 |
| CLI | `vm guest-exec`, `vm guest-agent-ensure-channel --verify/--repair` 실제 route 연결 |
| Web/TUI | Guest Execution direct-control surface와 TUI runtime affordance 추가 |
| Runtime policy | `guest_execution.enabled=true`, `execute_enabled=true`, channel verify/repair enabled |

## 검증

```powershell
dotnet test src\DesktopNode.sln --no-restore
npm test --prefix web
npm run verify:parity --prefix web
```

추가로 `0.42.53-admin-smoke` 설치본에서 다음 current-card를 확인했다.

- `pcvcli --json runtime policy`: `vm.guest.exec`, `vm.guest.channel.verify`,
  `vm.guest.channel.ensure`가 `native_mutation_operations`에 포함됨.
- `pcvcli --json vm guest-exec pcv-smoke --dry-run --credential-ref ... -- powershell ...`:
  `execute_enabled=true`, `execution_queued=false`, credential target 원문 미노출.
- `pcvcli --json vm guest-agent-ensure-channel pcv-smoke --dry-run`:
  `verify_enabled=true`, `repair_enabled=true`.
- `pcvtui --smoke-once runtime --no-color`: API reachable.
- Web `/`, `/pcv-config.js`: HTTP `200`.

## 남은 경계

실제 VM 내부 명령 실행은 유효한 Windows guest와 보호 credential reference가 필요하므로
이번 fullgate에서는 dry-run/current-card까지 닫았다. Running job interrupt, credential
rotation UI, 실제 Windows guest credential smoke는 다음 slice에서 별도 evidence로 닫는다.
