# 전체 관리자 Host Mutation Gate - 2026-05-13 0.42.9 Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate`
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260513-040213-0429`
- Runtime version: `0.42.9-admin-smoke`
- Installed manifest version: `0.42.9-admin-smoke`
- Provenance commit: `f0620f2e18ae25de8751333684cb74b5051dcdc6`
- Full-gate MSI SHA-256:
  `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`
- Package build MSI SHA-256:
  `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 `0.42.9-admin-smoke` 기준 이전 full admin host mutation evidence를
기록한다. 선행 package build는
`docs/ga-ready/evidence/batch-evidence-root-service-action-package-2026-05-13-0429.md`가
소유하고, full package-pair PASS는 여전히
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`가 마지막 닫힌
package-pair evidence다.

## Artifact

- Package build:
  `artifacts/admin-smoke-package-20260513-0429`
- Package MSI SHA-256:
  `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`
- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429/PureCVisorDesktopNode-0.42.9-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-batch-evidence-current-card-20260513-0429/summary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`, duration `205602ms`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`, duration `11063ms`.
- MSI lifecycle install, repair, uninstall preserve, install-remove-data,
  uninstall-remove-data, final restore install은 모두 exit `0`.
- Final installed service: `PureCVisorDesktopNode`, state `Running`, start mode
  `Auto`.
- Installed manifest: `0.42.9-admin-smoke`.
- Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`,
  Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary
  `401` / `PCV_AUTH_REQUIRED`.
- Hyper-V managed VM route smoke는 create, start, restart, poweroff, delete,
  checkpoint create/restore/delete를 PASS로 기록했다.
- Hyper-V guest shutdown은 guest tools 부재 조건에서
  `PCV_VM_SHUTDOWN_NOT_AVAILABLE` expected error로 기록됐다.
- Unmanaged VM delete boundary는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단됐다.
- `remaining_pcv_vms=[]`.
- Boot time unchanged: true.

## Installed Listener Current Card

Full gate 이후 설치 서비스 `PathName`에 `--batch-evidence-root
"D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts"`를 다시 추가하고
서비스를 재시작했다. Batch evidence reader는 child route/OS evidence까지 읽어야
하므로 개별 batch run directory가 아니라 `artifacts` parent root를 사용했다.

`pcvcli.exe --protected-token-file ... --format json ops summary` 설치본 smoke 결과:

- `batch_evidence.status`: `available`
- `batch_evidence.configured`: `true`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260513-040213-0429`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.9-admin-smoke`
- `latest.release.msi_sha256`:
  `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`
- `latest.release.signing_mode`: `AllowUnsignedDev`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `latest.host_final_state.service_state`: `Running`
- `latest.host_final_state.firewall_rule_count`: `0`
- `errors_count`: `0`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260513-040213-0429`
- `installed_runtime.evidence_status`: `available`

따라서 Web Console current evidence card는 설치본
`GET /api/v1/ops/summary`의 `data.batch_evidence.latest`를 통해 0429 batch root를
표시할 수 있다.

## OS Mutation 정리 상태

- Final firewall rule count: `0`
- Final Event Log source present: `false`
- Final trust-store root present: `true`
- Final trust-store publisher present: `true`
- Root thumbprint: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
- Publisher thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- LAN prefix: `http://[redacted-private-endpoint]:7777/`

테스트 과정에서 생성한 firewall rule과 Event Log source는 제거했고, 사전에 존재하던
internal trust cert는 복원했다.

## Release Boundary

이 evidence는 internal/private 관리자 host mutation readiness와 installed listener
current-card contract를 닫는다. 다음 항목은 명시적으로 아직 claim하지 않는다.

- Public trusted signing
- External stable publication
- Public update channel availability
- External customer GA support boundary

따라서 `0.42.9-admin-smoke`는 `0.42.11-admin-smoke` full gate 이전 internal/private network distribution 기준의 predecessor
full admin host mutation PASS로 사용할 수 있지만, public GA release로 승격하려면
trusted signing과 외부 stable publication evidence를 별도 추가해야 한다.
