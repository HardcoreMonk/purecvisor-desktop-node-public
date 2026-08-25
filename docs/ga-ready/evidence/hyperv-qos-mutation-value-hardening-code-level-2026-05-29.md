# Hyper-V QoS mutation value hardening code-level 2026-05-29

evidence_id: `hyperv-qos-mutation-value-hardening-code-level-2026-05-29`
result: `PASS_CODE_LEVEL`
scope: `hyperv-qos-mutation-api-cli-value-boundary-hardening`
status: `pass-code-level-next-package-required`
product_payload_change: `true`
host_mutation_performed: `false`
package_build_performed: `false`
next_package_gate_candidate: `0.42.59-admin-smoke`
next_manual_admin_package_pair_candidate: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
adr: `docs/adr/0008-hyperv-qos-mutation-policy.md`
plan: `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md`
problem_codes: `PCV_VM_QOS_STORAGE_RANGE_INVALID`, `PCV_VM_QOS_NETWORK_RANGE_INVALID`
preview_native_adapter_called_on_invalid_range: `false`
apply_job_created_on_invalid_range: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 변경

- Local API `POST /api/v1/vms/{vm}/qos/storage/preview`와
  `POST /api/v1/vms/{vm}/qos/network/preview`는 음수, `1,000,000,000` 초과,
  `minimum > maximum` 값을 native adapter 호출 전에 `400`으로 거절한다.
- Local API queued apply route인 `POST /api/v1/vms/{vm}/qos/storage`와
  `POST /api/v1/vms/{vm}/qos/network`도 같은 range contract를 적용해 invalid payload가
  job queue에 들어가지 않게 한다.
- PCVCLI `vm blkio-set`과 `vm bandwidth-set`은 같은 range contract를 command-specific
  error로 먼저 반환한다. 전체 `Usage:` block은 출력하지 않는다.
- 기존 rollback/manual restore semantics 때문에 `0`은 유효한 값으로 유지한다.

## 검증

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~Qos"
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~Qos"
```

두 focused test suite는 PASS다.

## 경계

이 evidence는 code-level hardening이다. 설치본 package build, full admin host mutation gate,
manual-admin package-pair closure, installed Web/TUI/CLI current-card smoke는 아직 실행하지
않았다. 다음 제품화 gate는 Guest Execution hardening과 같은 `0.42.59-admin-smoke` package
chain에서 함께 닫는다.
