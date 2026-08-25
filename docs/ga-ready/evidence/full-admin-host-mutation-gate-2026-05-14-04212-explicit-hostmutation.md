# 전체 관리자 Host Mutation Gate - 2026-05-14 0.42.12 Explicit Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate` explicit rerun
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
- Runtime version: `0.42.12-admin-smoke`
- Installed manifest version: `0.42.12-admin-smoke`
- Full-gate provenance commit:
  `d338b8a99f3e1e3839ac89a6de0da034ff3da148`
- Full-gate MSI SHA-256:
  `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`
- Product payload package build:
  `artifacts/admin-smoke-package-20260513-04212`
- Product payload package MSI SHA-256:
  `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`
- Product payload provenance commit:
  `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`
- Payload aggregate SHA-256:
  `6d3aaa69c218942ddf02e4773b24ab62f4fa51f9843008e02304d658d6f42cd3`
- Product wrapper SHA-256:
  `5ba0708413d863e356b166a69ab8e4ae43f26d9609d65b7a3b9cce13f6344c33`
- Service host SHA-256:
  `9533af2b977ef843c07656b6a014382e9924a52b15c7ce44b38812789576bb86`
- CLI SHA-256:
  `85904efd64d3c38e3128b6b365d84926b25012a7eca388849579c75769bf8887`
- TUI SHA-256:
  `ba82121d2f3f2e4a5ae0811565bbceb2a822220ca66243c1854d333a5f1a18fe`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 사용자의 명시적 "host mutation 실행" 승인에 따른
`0.42.12-admin-smoke` full admin host mutation explicit rerun을 현재 기준 evidence로
승격한다. 선행 product payload package build는
`docs/ga-ready/evidence/ops-summary-data-builder-package-2026-05-13-04212.md`가
소유한다. 닫힌 full package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`가 소유한다.
2026-05-14 04212 rerun
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`와
2026-05-13 04212 full gate
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md`는
historical predecessor로 보존한다.

## 산출물

- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260514-140126-04212-explicit`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit/PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-batch-evidence-current-card-20260514-140126-04212-explicit/summary.json`
- Product repair evidence:
  `artifacts/installed-batch-evidence-current-card-20260514-140126-04212-explicit/product-repair-installed.json`
- Installed ops summary capture:
  `artifacts/installed-batch-evidence-current-card-20260514-140126-04212-explicit/ops-summary-compact-main-artifacts-root.json`
- Web/API boundary capture:
  `artifacts/installed-batch-evidence-current-card-20260514-140126-04212-explicit/web-api-boundary.json`
- Web Console current-card browser smoke:
  `artifacts/web-console-current-card-20260514-140126-04212-explicit/summary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260514-140126-04212-explicit` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`, duration `96946ms`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`, duration `11067ms`.
- GPU snapshots: `16`, peak adapter MiB `4501.93`.
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
- `latest.batch_id`: `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.12-admin-smoke`
- `latest.release.msi_sha256`:
  `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`
- `latest.release.git_commit`:
  `d338b8a99f3e1e3839ac89a6de0da034ff3da148`
- `latest.release.signing_mode`: `AllowUnsignedDev`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `latest.host_final_state.service_state`: `Running`
- `latest.host_final_state.firewall_rule_count`: `0`
- `latest.gpu_snapshot_count`: `16`
- `errors_count`: `0`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
- `installed_runtime.evidence_status`: `available`
- `service_path_has_batch_evidence_root`: `true`
- `wrapper_repair_used_native_service_action`: `true`
- `wrapper_repair_skipped_outer_start`: `true`

Web Console current-card browser smoke도 같은 installed listener에서 PASS했다. Dashboard와
Evidence view는 `full-admin-host-mutation-gate-20260514-140126-04212-explicit`와
`0.42.12-admin-smoke`를 표시했고, `token_value_observed_in_ui_text=false`였다.

따라서 Web Console current evidence card는 설치본
`GET /api/v1/ops/summary`의 `data.batch_evidence.latest`를 통해
2026-05-14 explicit rerun batch root를 표시한다.

## OS Mutation 정리 상태

- Firewall final rule count: `0`
- Event Log source present: `false`
- Internal trust store: root/publisher cert restored
- LAN listener smoke: `http://[redacted-private-endpoint]:7777/`
- Public trusted signing: excluded
- External stable publication: not claimed

## Package-pair 및 Version 경계

이 explicit rerun은 `0.42.12-admin-smoke` product payload를 새 버전으로 올리지 않는다.
`0.42.13-admin-smoke` package build는 열지 않았고,
`0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign도 열지 않았다.
다음 product payload 변경이 생기면 별도 package build와 manual-admin package-pair
candidate를 연다.

## 릴리스 경계

이 evidence는 internal/admin-smoke host mutation evidence다. Public trusted signing,
external stable publication, winget submission, public stable installer URL, public
clean-host release claim을 추가하지 않는다.
