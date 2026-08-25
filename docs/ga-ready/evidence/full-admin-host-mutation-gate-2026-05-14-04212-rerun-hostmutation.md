# 전체 관리자 Host Mutation Gate - 2026-05-14 0.42.12 Rerun Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate` rerun
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260514-04212-rerun`
- Runtime version: `0.42.12-admin-smoke`
- Installed manifest version: `0.42.12-admin-smoke`
- Full-gate provenance commit:
  `b9c2c25b2ea88f67a0b0ffa5e7e03240eb0ce2fe`
- Full-gate MSI SHA-256:
  `b18d86c197a568ed9b5f6bb38580e568de7a989dda8d730e585684d1c5131b7a`
- Product payload package build:
  `artifacts/admin-smoke-package-20260513-04212`
- Product payload package MSI SHA-256:
  `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`
- Product payload provenance commit:
  `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`
- Payload aggregate SHA-256:
  `6438519fab68df8c0b5d62570a441a97fc6d357975b3ce6013b1203191d30cd1`
- Product wrapper SHA-256:
  `5ba0708413d863e356b166a69ab8e4ae43f26d9609d65b7a3b9cce13f6344c33`
- Service host SHA-256:
  `fde8e37c165dbe39e6d25fa1d624bc8883c62eaf13dd85f275387bd78c4aed76`
- CLI SHA-256:
  `95205a7df87eebb195f5e29da37e3516d4f7ac9b2c571e73e795be7436db6b5d`
- TUI SHA-256:
  `f596191b495f40c7df0780d22549d7d298346131a17d9a4ee8a659d3e19f6bf8`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 사용자 승인에 따른 `0.42.12-admin-smoke` full admin host mutation rerun을
현재 기준 evidence로 승격한다. 선행 product payload package build는
`docs/ga-ready/evidence/ops-summary-data-builder-package-2026-05-13-04212.md`가
소유한다. 닫힌 full package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`가 소유한다.
2026-05-13 04212 full gate `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md`는 historical predecessor로 보존한다.

## 산출물

- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260514-04212-rerun/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-04212-rerun`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260514-04212-rerun`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-04212-rerun/PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-batch-evidence-current-card-20260514-04212-rerun/summary.json`
- Product repair evidence:
  `artifacts/installed-batch-evidence-current-card-20260514-04212-rerun/product-repair-installed.json`
- Installed ops summary capture:
  `artifacts/installed-batch-evidence-current-card-20260514-04212-rerun/ops-summary-compact-main-artifacts-root.json`
- Web/API boundary capture:
  `artifacts/installed-batch-evidence-current-card-20260514-04212-rerun/web-api-boundary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-04212-rerun` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260514-04212-rerun` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`, duration `96668ms`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`, duration `11046ms`.
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
- `latest.batch_id`: `full-admin-host-mutation-gate-20260514-04212-rerun`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.12-admin-smoke`
- `latest.release.msi_sha256`:
  `b18d86c197a568ed9b5f6bb38580e568de7a989dda8d730e585684d1c5131b7a`
- `latest.release.git_commit`:
  `b9c2c25b2ea88f67a0b0ffa5e7e03240eb0ce2fe`
- `latest.release.signing_mode`: `AllowUnsignedDev`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `latest.host_final_state.service_state`: `Running`
- `latest.host_final_state.firewall_rule_count`: `0`
- `latest.gpu_snapshot_count`: `16`
- `errors_count`: `0`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260514-04212-rerun`
- `installed_runtime.evidence_status`: `available`
- `service_path_has_batch_evidence_root`: `true`
- `wrapper_repair_used_native_service_action`: `true`
- `wrapper_repair_skipped_outer_start`: `true`

따라서 Web Console current evidence card는 설치본
`GET /api/v1/ops/summary`의 `data.batch_evidence.latest`를 통해
2026-05-14 rerun batch root를 표시할 수 있다.

## OS Mutation 정리 상태

- Firewall final rule count: `0`
- Event Log source present: `false`
- Internal trust store: root/publisher cert restored
- LAN listener smoke: `http://[redacted-private-endpoint]:7777/`
- Public trusted signing: excluded
- External stable publication: not claimed

## Package-pair 및 Version 경계

이 rerun은 `0.42.12-admin-smoke` product payload를 새 버전으로 올리지 않는다.
`0.42.13-admin-smoke` package build는 열지 않았고,
`0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign도 열지 않았다.
다음 product payload 변경이 생기면 별도 package build와 manual-admin package-pair
candidate를 연다.

## 릴리스 경계

이 evidence는 internal/admin-smoke host mutation evidence다. Public trusted signing,
external stable publication, winget submission, public stable installer URL, public
clean-host release claim을 추가하지 않는다.
