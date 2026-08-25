# 전체 관리자 Host Mutation Gate - 2026-05-16 0.42.25

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04225-hostmutation
result: PASS
version: 0.42.25-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04225
host_mutation_performed: true
dry_run: false
batch_evidence.status: available
runtime_api_current_evidence_contract: runtime-api-current-evidence-rollup-v1
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_route_count: 4
public_trusted_signing: excluded
external_stable_publication: not-claimed
```

Batch Supervisor가 `FullAdminHostMutationGate` profile을 elevated
`-AllowHostMutation` 범위에서 실행했다. Service/MSI/Hyper-V route parity와
firewall/LAN/Event Log/internal trust-store OS mutation gate가 모두 PASS했다.

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04225` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04225` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260516-04225` |
| full-gate MSI SHA-256 | `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b` |
| full-gate provenance commit | `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1` |
| payload aggregate SHA-256 | `3ad6856606ab71fddef89adf2c59e17d7c68ee257723444922431e0e0070a6cb` |
| product wrapper SHA-256 | `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f` |
| service host SHA-256 | `3ac98ebf2777ad2c05b83988107d3cdced43d3183e411d47f33eea6f4cb336df` |
| CLI SHA-256 | `343c53f84e9ffb4866b5f477f8ae16718ea72e5c1993b31e8307b77de6c645c6` |
| TUI SHA-256 | `001af8613f35c7dd5c8682a54cff2f58aa12cd01fddd46b7338066ed0d258954` |
| signing mode | `AllowUnsignedDev` |

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`다.
`service-msi-hyperv-admin-smoke`는 attempt `1`, duration `242633ms`, exit `0`이고
`os-mutation-gate`는 attempt `1`, duration `11090ms`, exit `0`이다.

OS mutation final state는 boot time unchanged `true`, final service `Running`,
firewall rule count `0`, Event Log source present `false`, trust root/publisher
present `true`/`true`다. 이 evidence는 internal `AllowUnsignedDev` admin-smoke
범위이며 public trusted signing 또는 external stable publication evidence가 아니다.
