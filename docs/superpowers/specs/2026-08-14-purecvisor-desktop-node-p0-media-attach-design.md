# PureCVisor Desktop Node P0-1 미디어 재장착 설계

- Design-ID: `purecvisor-desktop-node-p0-media-attach-v1`
- 작성일: `2026-08-14`
- 문서 상태: `draft-awaiting-approval`
- 승인 locator: `User-Approval: service-plan-p0-media-attach-20260814`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P0-1, §8
- 운영 앵커: `0.42.73-admin-smoke`
- 이 설계가 수행하는 host mutation: `false`
- 변경 등급: `M` (`api-cli-web-contract`)
- 최소 검증 레인: `Full`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 `vm.eject`의 빈 짝인 가상 DVD ISO 재장착만 닫는다. USB/3D 장치 상점, DVD
드라이브 추가, network switch mutation, package campaign은 범위 밖이다.

## 1. 문제

04273은 `POST /api/v1/vms/{id}/eject`로 DVD media를 제거할 수 있다. 같은 drive에 ISO를
다시 넣는 제품 경로는 없다. 운영자는 create 때 넣은 ISO를 빼면 재설치/복구 미디어를
다시 걸 수 없다.

기존 eject 계약:

- queued job, operation `vm.eject`
- native `DesktopNodeHyperVWmiVmMediaProvider`가 첫 DVD의 `HostResource`를 빈 배열로 설정
- DVD가 없으면 `PCV_VM_DVD_DRIVE_NOT_FOUND`
- helper fallback 없음
- Web `Eject media`, `pcvcli vm eject <vm>`

## 2. 목표와 비목표

### 목표

- `POST /api/v1/vms/{vmId}/attach`가 `{ "iso_path": "<absolute-path>" }`를 받아 queued
  `vm.attach` job을 만든다.
- Web와 `pcvcli vm attach`가 같은 route를 쓴다.
- 기존 DVD 한 개의 `HostResource`만 교체한다. 이미 ISO가 있으면 덮어쓴다.
- ISO 파일이 없으면 create와 같은 `PCV_ISO_NOT_FOUND`를 반환한다.
- DVD가 없으면 eject와 같은 `PCV_VM_DVD_DRIVE_NOT_FOUND`를 반환한다. 드라이브를 만들지 않는다.

### 비목표

- USB passthrough, 3D, NIC/DVD 추가 (SERVICE_PLAN P2-15 / 거부 항목)
- attach preview/dry-run (eject family에 preview가 없다)
- attach reconcile (P0-2는 restore만)
- `0.42.74` 또는 package-pair 개방
- public trusted signing, 외부 publication
- TUI

## 3. 계약

### 3.1 Route

| 항목 | 값 |
| --- | --- |
| Method/path | `POST /api/v1/vms/{vmId}/attach` |
| OperationName | `QueueAttachVmMedia` |
| Job operation | `vm.attach` |
| Family | `hyperv-vm` |
| Stance | `QueuedMutation` |
| Permission | `operate` |
| Catalog count | `56` → `57` |
| QueuedMutation count | `22` → `23` |

Body:

```json
{ "iso_path": "D:\\isos\\ubuntu.iso" }
```

Enqueue `202` job params:

```json
{ "name": "<vm display name>", "iso_path": "D:\\isos\\ubuntu.iso" }
```

성공 job result data:

```json
{ "name": "<vm>", "action": "attach", "iso_path": "D:\\isos\\ubuntu.iso" }
```

### 3.2 실패 코드

| 코드 | HTTP | 다음 행동 |
| --- | --- | --- |
| `PCV_VM_ATTACH_ISO_REQUIRED` | 400 enqueue | `iso_path`를 넣는다 |
| `PCV_VM_NAME_INVALID` | job failed | 표시 이름을 고친다 |
| `PCV_ISO_NOT_FOUND` | job failed | 호스트에 있는 ISO 절대 경로를 쓴다 |
| `PCV_VM_DVD_DRIVE_NOT_FOUND` | job failed | DVD가 있는 VM만 attach. 드라이브 추가는 열지 않음 |
| `PCV_VM_NOT_FOUND` | job failed | 대상 VM 이름을 확인 |
| `PCV_VM_MEDIA_FAILED` | job failed, retryable | 진단 후 수동 재시도 |

경로 정책은 create와 같다. `File.Exists`만 검사한다. 새 UNC/allowlist 정책을 만들지 않는다.

### 3.3 Native

`IDesktopNodeHyperVVmMediaProvider`를 request record로 바꾼다.

```csharp
public sealed record DesktopNodeHyperVVmMediaRequest(
    string Operation,
    string VmName,
    string? IsoPath = null);

public interface IDesktopNodeHyperVVmMediaProvider
{
    DesktopNodeHyperVVmMediaInfo Invoke(
        DesktopNodeHyperVVmMediaRequest request,
        CancellationToken cancellationToken);
}
```

`DesktopNodeHyperVWmiVmMediaProvider`는 `vm.eject`와 `vm.attach`만 허용한다. attach는 기존
`FindDvdDrive`로 첫 DVD를 찾고 `HostResource = new[] { isoPath }`를 넣은 뒤
`ModifyResourceSettings`를 호출한다. 여러 DVD가 있으면 eject와 같이 첫 일치만 다룬다.

### 3.4 CLI / Web

- CLI: `pcvcli vm attach <vm> --iso <path>` 와 `--iso_path` alias. `--yes` 없음 (eject와 짝).
- Web: VM detail에 ISO 경로 입력 + `Attach media`. confirmation은 VM 표시 이름과 `iso_path`를
  보여 준다. RBAC `operate`.
- served route coverage: `vm.media.attach` = `POST /api/v1/vms/{vm_id}/attach`.
- 기존 `vm.media` eject row는 유지한다.

### 3.5 검증

Code-level:

- catalog 57, attach row, QueuedMutation 23, snapshot digest 갱신
- native adapter `iso_path` 전달, missing ISO, missing DVD
- API enqueue `vm.attach` without helper fallback
- CLI catalog, Web fixture/static, `node scripts/build-served-asset.mjs --write`

설치본 smoke와 package campaign은 이 slice의 required 조건이 아니다. 설치본 검증은
P0 네 항목을 묶거나 별도 승인 후에만 연다.

## 4. 비주장

- operational current는 `0.42.73-admin-smoke` 유지
- host mutation performed `false`
- public trusted signing / external stable publication `not-claimed`
