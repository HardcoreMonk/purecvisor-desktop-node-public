# 전체 관리자 Host Mutation Gate - 2026-05-12 0.42.8 Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate`
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-12-0428-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260512-233650-0428-r2`
- Runtime version: `0.42.8-admin-smoke`
- Installed manifest version: `0.42.8-admin-smoke`
- Provenance commit: `5397e580c98a34e8b7beb5b9773d1d857025315b`
- Full-gate MSI SHA-256:
  `01762ee3fd103981ac6fce121b6749e832dfabc7420123a6363f7fbe0e0f8f99`
- Post-merge package MSI SHA-256:
  `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 `0.42.8-admin-smoke` 기준 이전 full admin host mutation evidence를
기록한다. 2026-05-13 이후 최신 full admin host mutation claim은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`가
소유한다. 선행 package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`가 소유한다.

## Artifact

- Post-merge package build:
  `artifacts/admin-smoke-package-20260512-0428-postmerge`
- Post-merge package MSI SHA-256:
  `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`
- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-233650-0428-r2/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260512-233650-0428-r2`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2/PureCVisorDesktopNode-0.42.8-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-batch-evidence-current-card-20260512-0428-post-gate-r2/summary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260512-233650-0428-r2` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`.
- MSI lifecycle install, repair, uninstall preserve, install-remove-data,
  uninstall-remove-data, final restore install은 모두 exit `0`.
- Final installed service: `PureCVisorDesktopNode`, state `Running`, start mode
  `Auto`.
- Installed manifest: `0.42.8-admin-smoke`.
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

## Recovered Attempt

첫 full gate attempt
`full-admin-host-mutation-gate-20260512-231543-0428`는 route parity MSI lifecycle
`repair`에서 timeout으로 실패했다. MSI repair log는
`EventLogDefaultTransitionRepair` deferred custom action의
`DesktopNode.Host.exe service-action eventlog-default-transition` 프로세스가 남아
있음을 보여줬다.

해당 stuck service-action process를 정리한 뒤 같은 installed command를 정확한 argv로
직접 실행한 evidence
`artifacts/eventlog-default-transition-direct-repro-20260512-0428-after-repair-timeout`는
exit `0`, duration `177ms`로 PASS했다. 이어 direct MSI repair evidence
`artifacts/msi-repair-direct-repro-20260512-0428-after-eventlog-timeout`도 exit `0`,
duration `2353ms`로 PASS했다. 따라서 첫 attempt는 latest/current PASS claim이
아니며, r2 batch가 current full gate evidence다.

## Installed Listener Current Card

Full gate 이후 설치 서비스 `PathName`에 `--batch-evidence-root
"D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts"`를 다시 추가하고
서비스를 재시작했다. Batch evidence reader는 child route/OS evidence까지 읽어야
하므로 개별 batch run directory가 아니라 `artifacts` parent root를 사용했다.

`pcvcli.exe --protected-token-file ... --json ops summary` 설치본 smoke 결과:

- `batch_evidence.status`: `available`
- `batch_evidence.configured`: `true`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260512-233650-0428-r2`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.8-admin-smoke`
- `latest.release.msi_sha256`:
  `01762ee3fd103981ac6fce121b6749e832dfabc7420123a6363f7fbe0e0f8f99`
- `latest.release.signing_mode`: `AllowUnsignedDev`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `latest.host_final_state.service_state`: `Running`
- `latest.host_final_state.firewall_rule_count`: `0`
- `errors_count`: `0`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260512-233650-0428-r2`
- `installed_runtime.evidence_status`: `available`

따라서 Web Console current evidence card는 설치본
`GET /api/v1/ops/summary`의 `data.batch_evidence.latest`를 통해 0428 r2 batch root를
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

따라서 `0.42.8-admin-smoke`는 historical internal/private network distribution 및
manual admin validation PASS로 보존한다. 현재 최신 full admin host mutation PASS는
04211 evidence가 최신 current claim을 소유하며, public GA release로 승격하려면 trusted signing과 외부
stable publication evidence를 별도 추가해야 한다.
