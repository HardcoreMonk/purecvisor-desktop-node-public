# 전체 관리자 Host Mutation Gate - 2026-05-14 0.42.15

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor `FullAdminHostMutationGate`
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-14-04215-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260514-234158-04215`
- Runtime version: `0.42.15-admin-smoke`
- Installed manifest version: `0.42.15-admin-smoke`
- Full-gate provenance commit:
  `8ddf4b9715dd50cd4aa94c4fa77eb17ba8beaaff`
- Full-gate MSI SHA-256:
  `a00f07e3a86b5e62569c9ddaa17052d74f881f48d9ec6c9043be9815762e690d`
- Clean product package build:
  `artifacts/admin-smoke-package-20260514-04215-clean`
- Clean product package MSI SHA-256:
  `80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7`
- Product payload provenance commit:
  `8ddf4b9715dd50cd4aa94c4fa77eb17ba8beaaff`
- Full-gate payload aggregate SHA-256:
  `e203be49495efcdc8d6e79ef8fe91d9a86f498103297a1948aed017ea941fa1d`
- Clean package payload aggregate SHA-256:
  `9318522dbf926746a758547f30cdfb9c6b528cbc2744052f65219330a691aab1`
- Product wrapper SHA-256:
  `5ba0708413d863e356b166a69ab8e4ae43f26d9609d65b7a3b9cce13f6344c33`
- Service host SHA-256:
  `f8ce7de453e8e753bd68b78373215a24917d9c7cbf0900a40a857a50f7435670`
- CLI SHA-256:
  `39f6c68278ce52ab6c0a3138f4232bcc97735834ca8b3544fdb5f63b21dfc40b`
- TUI SHA-256:
  `a06d3f52d29dbbeb7056957ab667bf0a35648a883846d3becf2501b02bf7ae06`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 사용자의 명시적 host mutation 승인에 따른 `0.42.15-admin-smoke` full
admin host mutation gate를 현재 기준 evidence로 승격한다. 선행 package-pair는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04214-04215.md`가 소유한다.
`0.42.12-admin-smoke` explicit full gate와 `0.42.12 -> 0.42.13` package-pair는
historical predecessor로 보존한다.

## 산출물

- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260514-234158-04215/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-234158-04215`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260514-234158-04215`
- Full-gate provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-234158-04215/PureCVisorDesktopNode-0.42.15-admin-smoke-windows-x64.provenance.json`
- Installed listener current-card smoke:
  `artifacts/installed-current-card-20260514-04215-fullgate/summary.json`
- Installed ops summary capture:
  `artifacts/installed-current-card-20260514-04215-fullgate/ops-summary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-234158-04215` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260514-234158-04215` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal trust
cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`, duration `97329ms`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`,
  attempt count `1`, duration `11068ms`.
- GPU snapshots: `16`.
- Final installed service: `PureCVisorDesktopNode`, state `Running`, start mode
  `Auto`.
- Installed manifest: `0.42.15-admin-smoke`.
- Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`.
- Hyper-V managed VM route smoke는 create, start, restart, poweroff, delete,
  checkpoint create/restore/delete를 PASS로 기록했다.
- Hyper-V guest shutdown은 guest tools 부재 조건에서
  `PCV_VM_SHUTDOWN_NOT_AVAILABLE` expected error로 기록됐다.
- Unmanaged VM delete boundary는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단됐다.
- `remaining_pcv_vms=[]`.
- Boot time unchanged: true.

## 설치본 Current Card

Full gate 이후 MSI lifecycle이 service `PathName`의 `--batch-evidence-root` 옵션을
제거했으므로, 설치본 current-card smoke 전에 product wrapper
`RepairInstalled -BatchEvidenceRoot artifacts`를 다시 적용했다.

Installed `pcvcli.exe --protected-token-file ... --format json ops summary` 결과:

- `batch_evidence.status`: `available`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260514-234158-04215`
- `latest.ok`: `true`
- `latest.status`: `completed`
- `latest.release.version`: `0.42.15-admin-smoke`
- `latest.route_msi_hyperv.status`: `available`
- `latest.os_mutation.status`: `available`
- `descriptor_batch_id`: `manual-admin-campaign-descriptor-20260514-04214-04215`
- `descriptor_excluded_from_operational_latest`: `true`
- `service_path_has_batch_evidence_root`: `true`
- Web Console: HTTP `200`
- `/pcv-config.js`: HTTP `200`

Plain selector summary: `batch_evidence.status=available`,
`latest.batch_id=full-admin-host-mutation-gate-20260514-234158-04215`,
`descriptor_excluded_from_operational_latest=true`.

따라서 Web Console current evidence card의 operational latest는 04215 full gate이며,
manual-admin descriptor batch는 selector guard에 의해 current-card anchor에서 제외된다.

## OS Mutation 정리 상태

- Firewall final rule count: `0`
- Event Log source present: `false`
- Internal trust store: root/publisher cert restored
- LAN listener smoke: `http://[redacted-private-endpoint]:7777/`
- Public trusted signing: excluded
- External stable publication: not claimed

## 릴리스 경계

이 evidence는 internal/admin-smoke host mutation evidence다. Public trusted signing,
external stable publication, winget submission, public stable installer URL, public
clean-host release claim을 추가하지 않는다.
