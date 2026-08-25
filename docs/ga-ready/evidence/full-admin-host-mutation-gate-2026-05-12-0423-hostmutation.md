# 전체 관리자 Host Mutation Gate - 2026-05-12 0.42.3 Host Mutation

## 판정

- 상태: PASS
- 실행 형태: Batch Supervisor full admin host mutation gate
- Host mutation performed: true
- Dry run: false
- Evidence ID: `full-admin-host-mutation-gate-2026-05-12-0423-hostmutation`
- Batch ID: `full-admin-host-mutation-gate-20260512-021337-0423`
- Runtime version: `0.42.3-admin-smoke`
- Installed manifest version: `0.42.3-admin-smoke`
- Provenance commit: `61a015a56a71a8a3194d18f0882b39d620ddf896`
- MSI SHA-256: `31ea6df1ff11cbaa9a9681b083cb5d1f61bc87ecd49db52c4e60e7a141cb229d`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: excluded / not claimed
- External stable publication: not claimed
- public trusted signing / external stable publication: not claimed
- Mutation surfaces: Service/MSI/Hyper-V, firewall, Event Log, trust-store, LAN listener smoke

이 문서는 실행 당시 `0.42.3-admin-smoke` 기준 full admin host mutation evidence를
기록한다. Hyper-V WMI helper 추출과 Ops summary data builder 분리 후 같은 source
commit에서 admin-smoke MSI를 빌드하고, Service/MSI/Hyper-V runtime smoke와
firewall, LAN listener, Event Log, internal trust-store OS mutation gate를 실제
관리자 권한으로 실행했다.

사후 상태: 2026-05-12 이후 최신 full admin host mutation claim은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md`가
소유한다. 이 0423 evidence는 historical PASS로 보존한다.

## Artifact

- Batch supervisor summary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423/summary.json`
- Service/MSI/Hyper-V artifact root:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-021337-0423`
- OS mutation artifact root:
  `artifacts/os-mutation-gates-batch-profile-20260512-021337-0423`
- Provenance:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-021337-0423/PureCVisorDesktopNode-0.42.3-admin-smoke-windows-x64.provenance.json`
- Publication descriptor:
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-021337-0423/PureCVisorDesktopNode-0.42.3-admin-smoke-windows-x64.publication.json`
- Post-campaign Web/API auth boundary:
  `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423/post-campaign-web-api-auth-boundary.json`

## 실행 범위

Batch Supervisor는 두 단계를 모두 실행했다.

| Step | 결과 | Artifact |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | PASS | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-021337-0423` |
| `os-mutation-gate` | PASS | `artifacts/os-mutation-gates-batch-profile-20260512-021337-0423` |

`service-msi-hyperv-admin-smoke`는 current admin-smoke MSI build, service-action smoke,
MSI lifecycle smoke, installed `.NET` host Hyper-V API route smoke를 포함했다.
`os-mutation-gate`는 config migration blocked-while-running proof, Event Log
register/remove, firewall enable/remove, LAN listener smoke, existing internal
trust cert export/install/remove/restore를 포함했다.

## 주요 관찰

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`,
  `executed_steps=2`.
- Service/MSI/Hyper-V step: exit code `0`, timeout `false`, retry count `1`,
  attempt count `1`.
- OS mutation step: exit code `0`, timeout `false`, retry count `0`, attempt
  count `1`.
- Final service: `PureCVisorDesktopNode`, state `Running`, start mode `Auto`.
- Installed manifest: `0.42.3-admin-smoke`.
- MSI lifecycle install, repair, uninstall preserve, install-remove-data,
  uninstall-remove-data, final restore install은 모두 exit `0`.
- Final service path에는 API prefix `http://127.0.0.1:7777/`, Web prefix
  `http://127.0.0.1:80/`, Windows Event Log writer, diagnostics root,
  Credential Manager token target, account/JWT files, route timeout, rate limit,
  body limit이 포함됐다.
- Hyper-V managed VM route smoke는 create, start, restart, poweroff, delete,
  checkpoint create/restore/delete를 PASS로 기록했다.
- Hyper-V guest shutdown은 guest tools 부재 조건에서
  `PCV_VM_SHUTDOWN_NOT_AVAILABLE` expected error로 기록됐다.
- Unmanaged VM delete boundary는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단됐다.
- `remaining_pcv_vms=[]`.
- Boot time unchanged: true.

## Web Console 및 Auth Boundary

관리자 실행 후 loopback Web/API 확인은 host mutation 없이 후속 read-only check로
수행했다.

- `http://127.0.0.1/`: HTTP `200`
- `http://127.0.0.1/pcv-config.js`: HTTP `200`
- `http://127.0.0.1:7777/api/v1/runtime/policy`: unauthenticated request HTTP
  `401`, `PCV_AUTH_REQUIRED`

LAN listener smoke도 token redaction과 bearer-required boundary를 유지한 상태로
PASS였다.

- LAN prefix: `http://[redacted-private-endpoint]:7777/`
- Auth mode: `bearer-protected-token-file`
- Non-loopback static auth: `bearer-required`
- Checked paths: `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js`

## OS Mutation 정리 상태

- Final firewall rule count: `0`
- Final Event Log source present: `false`
- Final trust-store root present: `true`
- Final trust-store publisher present: `true`
- Root thumbprint: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
- Publisher thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`

이 상태는 테스트 과정에서 생성한 firewall rule과 Event Log source를 제거했고,
사전에 존재하던 internal trust cert는 복원했다는 의미다. Public trusted signing
또는 외부 stable publication evidence는 이 gate의 범위가 아니며, GA/public release
판정에서는 별도 evidence가 필요하다.

## Release Boundary

이 evidence는 private/internal 관리자 host mutation readiness를 닫는다. 다음 항목은
명시적으로 아직 claim하지 않는다.

- Public trusted signing
- External stable publication
- Public update channel availability
- External customer GA support boundary

따라서 `0.42.3-admin-smoke`는 internal/private network distribution 및 manual admin
validation 기준의 최신 PASS로 사용할 수 있지만, public GA release로 승격하려면
trusted signing과 외부 stable publication evidence를 별도 추가해야 한다.
