# Desktop Node P2-14 네트워크 변경 설계

- Design-ID: `purecvisor-desktop-node-p2-network-change-v1`
- 작성일: `2026-09-21`
- 문서 상태: `implemented-slice-5`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P2-14
- 선행: `pcv.network.inventory` read-only, Host `firewall-enable`/`firewall-remove`
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

VM create/clone은 `Default Switch`에만 붙는다. lab 스위치를 만들거나 NIC를 다른 스위치에
옮기려면 Hyper-V Manager에 의존한다. SERVICE_PLAN P2-14는 네트워크 변경을 열기 **전에**
admin runbook / service-action만 허용하고, Web switch/NAT/DHCP 에디터를 열지 말라고 한다.
자체 NAT/DHCP/브리지 스택은 제품 경계 밖이다.

이 설계는 새 Feature ID를 만들지 않는다. `pcv.network.inventory`에 붙인다.
P0 evidence 후보 4개와 catalog feature 28개는 그대로다.

## 현재 계약

- `GET /api/v1/network/inventory`는 read-only다. switch create/remove HTTP는 없다.
- Web Network 화면은 inventory만 표시한다.
- VM create/clone는 `Default Switch`만 연결한다.
- Host service-action은 firewall/trust-store 등이며 Hyper-V switch mutation은 없다.
- catalog routes는 slice 2까지 78이었다. Slice 3이 `POST /api/v1/vms/{vmId}/network`를 추가해 79 / queued 36이다.

## 결정

- Feature는 `pcv.network.inventory`다. 29번째 feature id를 만들지 않는다.
- 스위치 생성/삭제는 Host `service-action`만. Local API `/network/switches`와 Web 에디터는
  열지 않는다. 자체 NAT/DHCP 서버도 열지 않는다.
- 허용 스위치 종류는 Hyper-V `internal`과 `private`만. `external`은 물리 어댑터 바인딩이라
  거절한다.
- 제품이 만든 스위치 이름만 다룬다. 접두사 `pcv-`. `Default Switch`와 `WSL`로 시작하는
  이름은 예약이며 생성/삭제 모두 거절한다.
- Internal은 `allow_management_os=true`만. Private는 `allow_management_os=false`만.
- 삭제는 붙은 VM이 0일 때만. in-use는 거절한다.
- VM의 기존 NIC를 **이미 있는** inventory 스위치에 붙이는 것은 이후 slice의 queued
  `vm.network.connect`다. 새 NIC를 다는 것은 P2-15다. Web 네트워크 에디터는 둘 다 아니다.
- VM connect는 managed, 전원 Off, template-lock 아님. 대상 스위치는 inventory에 있어야 한다.
  Default Switch로 되돌리는 것은 허용한다.
- 스위치 create/remove auth는 admin 또는 service bearer. VM connect auth는 operate 또는
  service bearer.
- 정책 평가는 IO가 없다. 존재 여부·연결 VM 수는 호출자가 request에 넣는다.
- P2-15 NIC/DVD add, current-evidence write, package-pair, Lane 2 Hyper-V 복구는 이 설계 밖이다.

## 스키마 `pcv-network-change-v1`

```json
{
  "schema": "pcv-network-change-v1",
  "action": "switch-create",
  "switch_name": "pcv-lab-internal",
  "switch_type": "internal",
  "allow_management_os": true
}
```

## Slice

### Slice 1 — 정책 계약 (이번)

- `DesktopNode.Contracts` static `NetworkChangePolicy`.
- `EvaluateSwitchCreatePreview` / `EvaluateSwitchCreate` / `EvaluateSwitchRemove`.
- `EvaluateVmConnectPreview` / `EvaluateVmConnect`.
- HTTP, service-action, CLI, Web, WMI는 이 slice가 아니다.

### Slice 2 — Host service-action switch create/remove

- `DesktopNode.Host.exe service-action switch-create|switch-remove`.
- Local API switch editor는 이 slice가 아니다.

### Slice 3 — queued VM connect (이번)

- `POST /api/v1/vms/{vmId}/network` queued `vm.network.connect`. `--yes`.
- CLI `pcvcli vm network connect <vm> --switch NAME --yes`도 이 slice에 포함한다. Slice 4는 문서 잔여다.
- Web 에디터는 이 slice가 아니다.

### Slice 4 — CLI

- `pcvcli vm network connect <vm> --switch NAME --yes`.
- switch create/remove CLI는 열지 않는다. Host service-action이 소유한다.

### Slice 5 — Web excluded/readback

- Network 화면은 inventory read-only다. `network-change-readback`은 product `pcv-` 스위치, 예약 이름, external 유무를 표시한다.
- 경계 칩은 `no switch create form`, `no NAT editor`, `no DHCP editor`, `CLI/API vm.network.connect only`다.
- switch create/remove, NAT/DHCP, `vm.network.connect` 저장 폼은 열지 않는다.
- Web present `60` / excluded `20`이다. `vm.device.add`는 Web에서 제외된다.

## 거절 코드

`PCV_NETWORK_CHANGE_FORBIDDEN`, `PCV_NETWORK_SWITCH_NAME_REQUIRED`,
`PCV_NETWORK_SWITCH_NAME_RESERVED`, `PCV_NETWORK_SWITCH_NAME_NOT_PRODUCT`,
`PCV_NETWORK_SWITCH_TYPE_UNSUPPORTED`, `PCV_NETWORK_NAT_FORBIDDEN`,
`PCV_NETWORK_DHCP_FORBIDDEN`, `PCV_NETWORK_SWITCH_ALREADY_EXISTS`,
`PCV_NETWORK_SWITCH_NOT_FOUND`, `PCV_NETWORK_SWITCH_IN_USE`,
`PCV_NETWORK_MANAGEMENT_OS_INVALID`, `PCV_VM_NETWORK_VM_REQUIRED`,
`PCV_VM_NOT_MANAGED_BY_PURECVISOR`, `PCV_VM_TEMPLATE_LOCKED`,
`PCV_VM_NETWORK_SOURCE_NOT_OFF`, `PCV_VM_NETWORK_SWITCH_REQUIRED`.

Auth 실패는 이름/타입 검사보다 먼저다.

## 검증

- Slice 1: `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj -c Release --filter FullyQualifiedName~NetworkChangePolicyTests`
- Slice 2~4: focused Host/API/CLI + catalog pin. dirty-tree는 four-shard PASS가 아니다.
- Slice 5: `npm run test:required --prefix web`. Web present `60` / excluded `20`. switch/NAT/DHCP/`vm.network.connect` 저장 폼 없음. `vm.device.add` Web exclusion이 20번째다.
- 설치본 actual-VM switch mutation은 Lane 2이며 이 캠페인이 열지 않는다.
