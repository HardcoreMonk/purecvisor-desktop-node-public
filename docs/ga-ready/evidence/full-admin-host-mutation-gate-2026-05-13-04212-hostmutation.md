# 전체 관리자 Host Mutation Gate - 2026-05-13 0.42.12 Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate`
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-13-04212-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260513-04212`
- Runtime version: `0.42.12-admin-smoke`
- Installed manifest version: `0.42.12-admin-smoke`
- Provenance commit: `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`
- Full-gate MSI SHA-256:
  `74735f98bb7afbaa46127eddb200a3de6e5a954b240d7a65578072960368e233`
- Package build MSI SHA-256:
  `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 실행 당시 `0.42.12-admin-smoke` 기준 full admin host mutation evidence를
기록했다. 2026-05-14 rerun 이후 current claim은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`가
소유하며, 이 문서는 historical predecessor로 보존한다. 선행 product payload package build는
`docs/ga-ready/evidence/ops-summary-data-builder-package-2026-05-13-04212.md`가
소유한다. 닫힌 full package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`가 소유한다.
이전 `0.42.9-admin-smoke -> 0.42.11-admin-smoke` package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`로 보존한다.

## 산출물

- Package build:
  `artifacts/admin-smoke-package-20260513-04212`
- Package MSI SHA-256:
  `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`
- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-04212/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-04212`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260513-04212`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-04212/PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-batch-evidence-current-card-20260513-04212/summary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-04212` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260513-04212` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`, duration `145000ms`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`, duration `11078ms`.
- Final installed service: `PureCVisorDesktopNode`, state `Running`, start mode
  `Auto`.
- Installed manifest: `0.42.12-admin-smoke`.
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
- `latest.batch_id`: `full-admin-host-mutation-gate-20260513-04212`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.12-admin-smoke`
- `latest.release.msi_sha256`:
  `74735f98bb7afbaa46127eddb200a3de6e5a954b240d7a65578072960368e233`
- `latest.release.signing_mode`: `AllowUnsignedDev`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `latest.host_final_state.service_state`: `Running`
- `latest.host_final_state.firewall_rule_count`: `0`
- `errors_count`: `0`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260513-04212`
- `installed_runtime.evidence_status`: `available`
- `service_path_has_batch_evidence_root`: `true`
- `wrapper_repair_used_native_service_action`: `true`
- `wrapper_repair_skipped_outer_start`: `true`

따라서 이 실행 당시 Web Console current evidence card는 설치본
`GET /api/v1/ops/summary`의 `data.batch_evidence.latest`를 통해 04212 batch root를
표시할 수 있었다. 2026-05-14 rerun 이후 current card anchor는
`full-admin-host-mutation-gate-20260514-04212-rerun`이다.

## OS Mutation 정리 상태

- Firewall final rule count: `0`
- Event Log source present: `false`
- Internal trust store: root/publisher cert restored
- LAN listener smoke: `http://[redacted-private-endpoint]:7777/`
- Public trusted signing: excluded
- External stable publication: not claimed

## Manual-admin Package-pair 상태

`0.42.11-admin-smoke -> 0.42.12-admin-smoke` package-pair는 후속 manual-admin
campaign에서 PASS로 닫혔다.

- Readiness summary:
  `artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-rebaseline-readiness/summary.json`
- Campaign descriptor summary:
  `artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised/summary.json`
- Status: `pass`
- PASS bucket: installed update/rollback, clean-host with Windows Update, Burn lifecycle,
  MSIX lifecycle, installed runtime ops summary, descriptor generation

## 릴리스 경계

이 evidence는 internal/admin-smoke host mutation evidence다. Public trusted signing,
external stable publication, winget submission, public stable installer URL, public
clean-host release claim을 추가하지 않는다.
