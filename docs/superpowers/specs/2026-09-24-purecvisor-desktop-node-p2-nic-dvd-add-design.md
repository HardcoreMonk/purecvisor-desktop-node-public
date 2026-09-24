# Desktop Node P2-15 NIC/DVD 추가 설계

- Design-ID: `purecvisor-desktop-node-p2-nic-dvd-add-v1`
- 작성일: `2026-09-24`
- 문서 상태: `implemented-slice-2`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P2-15
- 선행: `vm.attach`는 있는 DVD의 ISO만 바꾼다. `vm.network.connect`는 있는 NIC의 스위치만 바꾼다.
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

VM create는 synthetic NIC 하나와 synthetic DVD 하나를 만든다. 가져온 VM에 DVD가 없거나
lab VM에 NIC가 하나 더 필요할 때 장치를 추가하는 제품 경로가 없다. `vm.attach`는 DVD
드라이브를 만들지 않는다.

SERVICE_PLAN P2-15는 한 번에 장치 하나만 queued add한다. USB, 3D, PCI 장치 상점은
열지 않는다.

## 결정

- 정책 schema는 `pcv-vm-device-add-v1`이다. Slice 1은 feature id와 route를 만들지 않는다.
- 허용 종류는 `nic`와 `dvd`뿐이다. 요청 수량은 항상 1이다.
- NIC는 이미 있는 inventory 스위치에만 붙인다. 스위치 생성, NAT, DHCP는 거절한다.
- NIC는 기존 수가 `2` 미만일 때만 추가한다. create가 이미 하나를 만든다.
- DVD는 기존 수가 `0`일 때만 빈 드라이브를 추가한다. ISO를 같이 넣지 않는다. ISO는 `vm.attach`다.
- VM은 managed, template-lock 아님, 전원 Off, Generation 2다.
- auth는 operate 또는 service bearer다. 실패는 종류 검사보다 먼저다.
- Web switch/NAT/DHCP 에디터, package-pair, current-evidence write는 이 설계가 열지 않는다.

## Slice

### Slice 1 — 정책 계약 (이번)

- `DesktopNode.Contracts` static `VmDeviceAddPolicy`.
- `EvaluatePreview` / `EvaluateAdd`.
- HTTP, CLI, Web, WMI는 이 slice가 아니다.

### Slice 2 — queued add (이번)

- `POST /api/v1/vms/{vmId}/devices`는 catalog operation `vm.device.add`다.
- body `device=nic|dvd`에 따라 job operation은 `vm.nic.add` 또는 `vm.dvd.add`다.
- CLI는 `pcvcli vm device add <vm> --kind nic --switch NAME --yes`와 `--kind dvd --yes`다.
- Web 장치 상점과 NAT/DHCP 폼은 열지 않는다. Web present `60` / excluded `20`이다.

## 거절 코드

`PCV_VM_DEVICE_ADD_FORBIDDEN`, `PCV_VM_DEVICE_VM_REQUIRED`,
`PCV_VM_NOT_MANAGED_BY_PURECVISOR`, `PCV_VM_TEMPLATE_LOCKED`,
`PCV_VM_DEVICE_SOURCE_NOT_OFF`, `PCV_VM_GENERATION_UNSUPPORTED`,
`PCV_VM_DEVICE_KIND_UNSUPPORTED`, `PCV_VM_DEVICE_QUANTITY_INVALID`,
`PCV_VM_DEVICE_LIMIT`, `PCV_VM_DEVICE_ALREADY_PRESENT`,
`PCV_VM_NETWORK_SWITCH_REQUIRED`, `PCV_NETWORK_SWITCH_NOT_FOUND`,
`PCV_NETWORK_NAT_FORBIDDEN`, `PCV_NETWORK_DHCP_FORBIDDEN`,
`PCV_VM_DEVICE_ISO_FORBIDDEN`.

## 검증

- Slice 1: `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj -c Release --filter FullyQualifiedName~VmDeviceAddPolicyTests`
- Slice 2: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter FullyQualifiedName~ApiVmDeviceAddMutationTests`와 CLI catalog 테스트.
- 설치본 actual-VM device add는 Lane 2이며 이 캠페인이 열지 않는다.
