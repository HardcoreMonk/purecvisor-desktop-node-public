# Desktop Node P2-13 Hyper-V export/import 설계

- Design-ID: `purecvisor-desktop-node-p2-hyperv-export-import-v1`
- 작성일: `2026-09-21`
- 문서 상태: `implemented-slice-5`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P2-13
- 선행: `pcv.vm.managed-import` `vm.manage`, `pcv.vm.clone` TPM/보안 가드
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

lab VM을 호스트 밖으로 옮기려면 Hyper-V Manager export에 의존한다. 그 경로는 managed
marker를 보장하지 않고, OVF나 vTPM 키 자료가 제품 데이터 루트 밖으로 샐 수 있다.

SERVICE_PLAN P2-13은 export/import를 열기 **전에** managed marker 유지를 닫고, 만능 OVF와
vTPM 키 노출 형태를 열지 말라고 한다.

이 설계는 새 Feature ID를 만들지 않는다. `pcv.vm.managed-import`에 붙인다.
P0 evidence 후보 4개와 catalog feature 28개는 그대로다.

## 현재 계약

- `POST /api/v1/vms/{vmId}/manage`는 기존 Hyper-V VM에 marker를 붙인다. 파일 export가 아니다.
- `POST /api/v1/vms/{vmId}/clone`는 독립 VHDX 복사다. export 폴더를 만들지 않는다.
- clone은 TPM/key protector/shielded를 `PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED`로 거절한다.
- Hyper-V export/import HTTP route는 없다.

## 결정

- Feature는 `pcv.vm.managed-import`다. 29번째 feature id를 만들지 않는다.
- permission은 기존 `operate`다. service bearer도 받는다.
- export 대상은 managed Generation 2, 전원 `Off`만. unmanaged·Gen1은 거절한다.
- TPM, key protector, shielded가 있으면 export와 import 모두 거절한다. vTPM 키를 복사하거나
  `.vmgs` 키 자료를 제품 경로 밖으로 내보내지 않는다.
- 패키지 종류는 Hyper-V export 폴더만. OVF/OVA는 `PCV_VM_IMPORT_OVF_FORBIDDEN`.
- 경로는 allowlist 루트 아래 하위 디렉터리만. UNC·드라이브 루트·루트 자체는 거절한다.
  기본 루트는 `%ProgramData%\PureCVisor\desktop-node\exports`.
- import는 새 VM 표시 이름과 **새 identity**만 허용한다. in-place import는 거절한다.
  성공 import는 managed marker를 붙인다. 소스 Notes/template-lock을 복사하지 않는다.
- export는 VM을 바꾸지 않는다. template-lock된 managed VM의 export는 허용한다.
- 정책 평가는 IO가 없다. 패키지에 `.vmcx`가 있는지는 호출자가 request에 넣는다.
- Web은 이 설계에서 export/import 폼을 열지 않는다. CLI/API만 이후 slice에서 연다.
- P2-14 네트워크 변경, P2-15 NIC/DVD add, current-evidence write, package-pair,
  Lane 2 Hyper-V 복구는 이 설계 밖이다.

## 스키마 `pcv-vm-export-import-v1`

```json
{
  "schema": "pcv-vm-export-import-v1",
  "action": "export",
  "vm_name": "lab-vm",
  "directory": "C:\\\\ProgramData\\\\PureCVisor\\\\desktop-node\\\\exports\\\\lab-vm",
  "package_kind": "hyperv-export",
  "generate_new_id": true,
  "apply_managed_marker": true
}
```

## Slice

### Slice 1 — 정책 계약 (이번)

- `DesktopNode.Contracts` static `VmExportImportPolicy`.
- `EvaluateExportPreview` / `EvaluateExport` / `EvaluateImportPreview` / `EvaluateImport`.
- HTTP, persist, CLI, Web, WMI export는 이 slice가 아니다.

### Slice 2 — preview HTTP/CLI

- `POST /api/v1/vms/{vmId}/export/preview`, `POST /api/v1/vms/import/preview`.
- persist와 queued mutation은 이 slice가 아니다.

### Slice 3 — queued export

- `POST /api/v1/vms/{vmId}/export` queued `vm.export`. `--yes`.
- import는 이 slice가 아니다.

### Slice 4 — queued import와 marker

- `POST /api/v1/vms/import` queued `vm.import`. 성공 후 managed marker.
- Web 폼은 이 slice가 아니다.

### Slice 5 — Web excluded/readback

- Web은 export/import 저장 폼을 열지 않는다. 필요하면 status readback만.

## 거절 코드

`PCV_VM_EXPORT_FORBIDDEN`, `PCV_VM_EXPORT_VM_REQUIRED`,
`PCV_VM_EXPORT_CONFIRMATION_MISMATCH`, `PCV_VM_NOT_MANAGED_BY_PURECVISOR`,
`PCV_VM_GENERATION_UNSUPPORTED`, `PCV_VM_EXPORT_SOURCE_NOT_OFF`,
`PCV_VM_EXPORT_SECURITY_FEATURES_UNSUPPORTED`, `PCV_VM_EXPORT_PATH_REQUIRED`,
`PCV_VM_EXPORT_PATH_NOT_ALLOWED`, `PCV_VM_IMPORT_NAME_REQUIRED`,
`PCV_VM_IMPORT_CONFIRMATION_MISMATCH`, `PCV_VM_IMPORT_OVF_FORBIDDEN`,
`PCV_VM_IMPORT_PACKAGE_INVALID`, `PCV_VM_IMPORT_SECURITY_FEATURES_UNSUPPORTED`,
`PCV_VM_IMPORT_INPLACE_FORBIDDEN`, `PCV_VM_ALREADY_EXISTS`, `PCV_VM_NAME_INVALID`,
`PCV_VM_IMPORT_PATH_REQUIRED`, `PCV_VM_IMPORT_PATH_NOT_ALLOWED`.

Auth 실패는 경로/패키지 검사보다 먼저다.

## 검증

- Slice 1: `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj -c Release --filter FullyQualifiedName~VmExportImportPolicyTests`
- Slice 2~4: focused API/CLI + catalog pin. dirty-tree는 four-shard PASS가 아니다.
- Slice 5: `npm run test:required --prefix web`. Web present 60 / excluded 18. export/import 저장 폼 없음.
- 설치본 actual-VM export/import는 Lane 2이며 이 캠페인이 열지 않는다.
