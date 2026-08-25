# WMI internal switch topology recovery 2026-07-13 0.42.60-0.42.62

evidence_id: `wmi-internal-switch-topology-recovery-2026-07-13-04260-04262`
result: `PASS_RECOVERY_CLOSED_BY_0.42.62`
evidence_scope: `internal-admin-smoke-only`
closure_version: `0.42.62-admin-smoke`
0.42.60_failure_code: `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`
0.42.61_failure_code: `PCV_NETWORK_INVENTORY_FAILED`
0.42.61_exception_type: `System.InvalidOperationException`
0.42.60_package_and_msi_lifecycle: `PASS`
0.42.61_package_and_msi_lifecycle: `PASS`
0.42.60_os_mutation_performed: `false`
0.42.61_os_mutation_performed: `false`
0.42.60_pass_anchor: `false`
0.42.61_pass_anchor: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실패 경계

| 버전 | package MSI / payload SHA-256 | provenance | full-gate 결과 |
| --- | --- | --- | --- |
| `0.42.60-admin-smoke` | `1ab362a9fc6221ea8060626c201748446620a2ed370b91ee3e16119e30cdc7ba` / `0d49ae12954f4f4ddb11d27203a7dde4c9e30bcb7f46c0609f5341cab525fc3f` | `384629e5fd07bf8547eda2194f793ae34a6110c5` | `full-admin-host-mutation-gate-20260713-04260`, `failed`, 1/2 단계 실행 |
| `0.42.61-admin-smoke` | `7e428ea87d7128859cd368e26a058f18a34259dfdf5452136a32b11271628a59` / `2af7cfb5c027bfbe0fe5020b2424ca3d6976c62fbc03205e1ad7c00b6509c43d` | `3dfef42d52f002af494a1c4d83d7a1e50a355336` | `full-admin-host-mutation-gate-20260713-04261`, `failed`, 1/2 단계 실행 |

두 package 모두 설치와 service action, MSI lifecycle을 PASS했다. 그러나 첫
`service-msi-hyperv-admin-smoke` 단계의 설치본 `network.inventory`에서 중단되어 VM 생성과
두 번째 OS mutation 단계는 실행되지 않았다. 따라서 `0.42.60`과 `0.42.61`은 package
lifecycle 선행 증거일 뿐 full-gate 또는 current PASS anchor가 아니다.

`0.42.60`은 실제 host의 `Default Switch`와 `WSL (Hyper-V firewall)`이 모두 Hyper-V
internal switch이고 management-OS adapter를 가진다는 elevated topology capture와 달리,
name-only 분류가 WSL switch를 `unknown`으로 남겨
`PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`를 반환했다.

## 0.42.61 elevated 진단

`artifacts/run-logs/full-admin-host-mutation-gate-20260713-04261/wmi-traversal-diagnostic.json`은
부분 속성으로 투영한 두 `Msvm_VirtualEthernetSwitch`의 `Path`가 모두 빈 문자열이었고,
첫 `GetRelated("Msvm_EthernetSwitchPort")`가 `System.InvalidOperationException`으로 실패했음을
기록한다. 이 실패는 API에서 `PCV_NETWORK_INVENTORY_FAILED`로 정규화됐다.

`artifacts/run-logs/full-admin-host-mutation-gate-20260713-04261/wmi-traversal-select-all-diagnostic.json`은
`SELECT *` 조회에서 두 switch path가 복원되고, 두 switch 모두
`Msvm_EthernetSwitchPort`와 `Msvm_EthernetPortAllocationSettingData` association traversal이
성공했음을 기록한다. 이 진단으로 name-independent topology 판정과 WMI association traversal
모두에 완전한 WMI object projection이 필요하다는 원인을 확정했다.

## 0.42.62 closure

`0.42.62-admin-smoke`는 완전한 switch object projection으로 복구했다. Clean package MSI는
`ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`, operational
full-gate MSI는 `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`,
provenance는 `7f71f0a518c5b592f233373522d36b5401c3f1df`다.

`full-admin-host-mutation-gate-20260713-04262`는 2/2 단계를 실행해 route와 OS mutation을
모두 PASS했다. 이후 read-only installed current-card에서 두 switch가 모두
`internal`, `allow_management_os=true`로 확인됐고 Web/TUI/CLI도 PASS했다.

최신 closed manual-admin package-pair는 별도 campaign이 실행되지 않았으므로 계속
`0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. 이 closure는
`AllowUnsignedDev`/`LocalTest` internal admin-smoke 증거이며 public trusted signing 또는 외부
stable publication을 주장하지 않는다.
