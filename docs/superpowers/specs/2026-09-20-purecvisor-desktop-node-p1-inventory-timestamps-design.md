# Desktop Node P1-6 inventory 시각 설계

- Design-ID: `purecvisor-desktop-node-p1-inventory-timestamps-v1`
- 작성일: `2026-09-20`
- 문서 상태: `implemented-slice-2`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P1-6
- 선행: P1-5 clone actual-VM PASS `0.42.77-admin-smoke`
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

VM list/detail은 전원·CPU·메모리는 보여 주지만 생성 시각과 마지막 전원 시각이 없다.
Web inventory `Updated` 칸은 `created_at`을 이미 읽지만 API가 비워 둔다.

## Slice 1 범위

- `GET /api/v1/vms`와 `GET /api/v1/vms/{id}` native `DesktopNodeHyperVVmInfo`에
  `created_at`, `last_powered_on`을 선택 필드로 넣는다.
- `created_at`은 `Msvm_VirtualSystemSettingData.CreationTime`이다.
- `last_powered_on`은 상태가 `running`일 때만
  `Msvm_ComputerSystem.TimeOfLastStateChange`다. Off/Saved/Paused면 생략한다.
- null은 JSON에서 생략한다. 기존 호출부는 깨지지 않는다.
- Notes 편집, template lock, host mutation은 이 slice가 아니다.

## Slice 2 범위

- `notes`는 운영자 메모만 노출한다. `managed-by=purecvisor-desktop-node` marker는 뺀다.
  marker만 있으면 필드를 생략한다.
- `pcvcli vm list` 표/plain/csv에 `created_at`, `last_powered_on`, `notes`를 붙인다.
  값이 없으면 `-`다.
- Web inventory는 기존 `created_at`/`notes` 칸을 그대로 쓴다. `last_powered_on` 열은
  이 slice가 아니다.

## 비목표

- P1-7 template lock
- 운영자 notes 쓰기
- last_powered_on을 Off 상태에서도 추정하는 별도 저장소
- current-evidence write, package-pair, Lane 2
