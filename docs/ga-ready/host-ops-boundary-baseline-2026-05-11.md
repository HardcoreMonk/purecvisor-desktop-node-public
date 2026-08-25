# Host Ops 경계 기준선 - 2026-05-11

entrypoint: `DesktopNodeHostServiceAction.CreatePlan` 및 `DesktopNodeHostServiceAction.ExecuteAsync`
host_mutation_default: explicit-admin-opt-in-only
behavior_change_allowed: false
ops_catalog: `src/DesktopNode.Host/Ops/DesktopNodeHostOpsCatalog.cs`

## Operation 계열

- service lifecycle: `DesktopNodeServiceLifecycleOps`가 `status`, `start`, `stop`, `configure-installed`, `repair-installed`, `remove-installed`를 소유한다.
- data root lifecycle: `DesktopNodeDataRootLifecycleOps`가 `data-root-remove`를 소유한다.
- config migration: `DesktopNodeConfigMigrationOps`가 `config-migration-apply`를 소유한다.
- job store migration: `DesktopNodeJobStoreMigrationOps`가 `job-store-migration-apply`를 소유한다.
- service token: `DesktopNodeServiceTokenOps`가 `service-token-rotation-revoke`를 소유한다.
- Credential Manager: `DesktopNodeCredentialManagerOps`가 `credential-manager-system-proof`, `credential-manager-default-transition`을 소유한다.
- Event Log: `DesktopNodeEventLogOps`가 `eventlog-register`, `eventlog-remove`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard`, `eventlog-default-transition`을 소유한다.
- firewall: `DesktopNodeFirewallOps`가 `firewall-enable`, `firewall-remove`를 소유한다.
- trust store: `DesktopNodeTrustStoreOps`가 `trust-store-install`, `trust-store-remove`를 소유한다.

## 불변 조건

- 어떤 operation 계열도 PowerShell command fallback을 다시 도입할 수 없다.
- Firewall LAN exposure는 명시적 LAN approval이 필요하다.
- Trust store install은 release approval이 필요하다.
- Data-root delete는 service absent 상태와 explicit remove-data가 필요하다.
- Credential Manager default transition은 service reload가 필요하더라도 Credential Manager ops family에서 dispatch한다.
- Config/job-store migration과 service-token rotation은 각각 독립 Ops family에서 dispatch하며 service lifecycle family에 다시 합치지 않는다.

## 0.42.18 이후 Lifecycle Smoke Bucket

`docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`는
`host_ops_lifecycle_buckets=service-eventlog-firewall-truststore-data-root-separated`로
Host Ops bucket을 고정한다. `DesktopNodeHostOpsCatalog.TryGetOperation`과
`DesktopNodeHostOpsCatalog.OperationBelongsTo`는 request/action dispatch가 family를
섞지 않는지 확인하는 기준이다.

| Bucket | Operation family | 대표 operation | Mutation boundary |
| --- | --- | --- | --- |
| service-action lifecycle | `service-lifecycle` | `configure-installed`, `repair-installed`, `remove-installed` | service SCM/config only |
| Event Log | `event-log` | `eventlog-repair`, `eventlog-write-test`, `eventlog-default-transition` | provider/event writer only |
| firewall | `firewall` | `firewall-enable`, `firewall-remove` | Windows Firewall rule only |
| trust store | `trust-store` | `trust-store-install`, `trust-store-remove` | X509 store only |
| data-root lifecycle | `data-root` | `data-root-remove` | allowlisted ProgramData root only |
