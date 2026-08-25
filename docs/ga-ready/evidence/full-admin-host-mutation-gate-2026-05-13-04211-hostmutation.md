# 전체 관리자 Host Mutation Gate - 2026-05-13 0.42.11 Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate`
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-13-04211-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260513-0429-04211`
- Runtime version: `0.42.11-admin-smoke`
- Installed manifest version: `0.42.11-admin-smoke`
- Provenance commit: `987beb51025a5aa926df7d9a905019b4d6d29705`
- Full-gate MSI SHA-256:
  `902e175cd6354843da2c928e2b6772f04d40240f02783e4edfed460ba0f9fce2`
- Package build MSI SHA-256:
  `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 `0.42.11-admin-smoke` 기준 최신 full admin host mutation evidence를
기록한다. 선행 product wrapper package build는
`docs/ga-ready/evidence/product-wrapper-native-repair-package-2026-05-13-04211.md`가
소유하고, full package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`가 소유한다.

## 산출물

- Package build:
  `artifacts/admin-smoke-package-20260513-04211`
- Package MSI SHA-256:
  `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`
- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-0429-04211/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-0429-04211`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260513-0429-04211`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-0429-04211/PureCVisorDesktopNode-0.42.11-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-batch-evidence-current-card-20260513-04211/summary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-0429-04211` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260513-0429-04211` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`, duration `133057ms`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`, duration `11064ms`.
- MSI lifecycle install, repair, uninstall preserve, install-remove-data,
  uninstall-remove-data, final restore install은 모두 exit `0`.
- Final installed service: `PureCVisorDesktopNode`, state `Running`, start mode
  `Auto`.
- Installed manifest: `0.42.11-admin-smoke`.
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

## 설치본 Listener Current Card

Full gate 이후 설치 서비스에 `RepairInstalled -BatchEvidenceRoot`를 product wrapper로
실행했다. 이 wrapper는 설치된 `DesktopNode.Host.exe service-action repair-installed`를
호출했고, native action이 final service state를 관리하므로 outer `sc.exe start`를
건너뛰었다.

`pcvcli.exe --protected-token-file ... --format json ops summary` 설치본 smoke 결과:

- `batch_evidence.status`: `available`
- `batch_evidence.configured`: `true`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260513-0429-04211`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.11-admin-smoke`
- `latest.release.msi_sha256`:
  `902e175cd6354843da2c928e2b6772f04d40240f02783e4edfed460ba0f9fce2`
- `latest.release.signing_mode`: `AllowUnsignedDev`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `latest.host_final_state.service_state`: `Running`
- `latest.host_final_state.firewall_rule_count`: `0`
- `errors_count`: `0`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260513-0429-04211`
- `installed_runtime.evidence_status`: `available`
- `service_path_has_batch_evidence_root`: `true`
- `wrapper_repair_used_native_service_action`: `true`
- `wrapper_repair_skipped_outer_start`: `true`

따라서 Web Console current evidence card는 설치본
`GET /api/v1/ops/summary`의 `data.batch_evidence.latest`를 통해 04211 batch root를
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

## 릴리스 경계

이 evidence는 internal/private 관리자 host mutation readiness와 installed listener
current-card contract를 닫는다. 다음 항목은 명시적으로 아직 claim하지 않는다.

- Public trusted signing
- External stable publication
- Public update channel availability
- External customer GA support boundary

따라서 `0.42.11-admin-smoke`는 internal/private network distribution 기준의 최신
full admin host mutation PASS로 사용할 수 있지만, public GA release로 승격하려면
trusted signing과 외부 stable publication evidence를 별도 추가해야 한다.
