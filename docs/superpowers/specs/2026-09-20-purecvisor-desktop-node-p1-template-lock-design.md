# Desktop Node P1-7 template lock 설계

- Design-ID: `purecvisor-desktop-node-p1-template-lock-v1`
- 작성일: `2026-09-20`
- 문서 상태: `implemented-slice-3`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P1-7
- 선행: P1-5 clone, P1-6 inventory timestamps
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

lab 원본 VM을 실수로 save/rename/delete/attach 하면 클론 소스가 깨진다. SERVICE_PLAN
P1-7은 template에 **start와 clone만** 허용한다.

## Slice 1 범위

- Notes marker `template-lock=true`가 있으면 그 VM은 template이다.
- Native adapter mutation 중 허용은 `vm.start`, `vm.clone`뿐이다.
  `vm.clone.preview`와 모든 Read는 그대로다. `vm.create`는 대상 VM이 없으므로 검사하지
  않는다.
- 거절 코드는 `PCV_VM_TEMPLATE_LOCKED`다.
- `DesktopNodeHyperVVmInfo.template_lock`은 true일 때만 JSON에 나온다.
- 운영자 `notes`에서 marker 줄을 뺀다. clone 대상 Notes는 기존처럼 managed marker만
  넣으므로 클론은 template이 아니다.
- lock/unlock HTTP route는 이 slice가 아니다. marker는 Notes로 붙인다.

## Slice 2 범위

- `POST /api/v1/vms/{vmId}/template-lock` queued job. body `confirm_name` + `locked`.
- Native operation `vm.template.lock`은 lock/unlock 모두 허용한다.
- lock은 managed VM만. unlock은 marker를 뺀다.
- `pcvcli vm template-lock <vm> --yes` / `pcvcli vm template-unlock <vm> --yes`.
- Web 버튼은 이 slice가 아니다.

## Slice 3 범위

- Web Console VM detail `Lock template` / `Unlock template`. 같은 queued route,
  `confirm_name` + `locked`.
- 잠긴 VM은 Start, Clone, Unlock만 활성. 나머지 mutation 컨트롤은 disabled.
- Template 행은 `locked` / `no`. coverage_id `vm.template.lock`.
- Feature `pcv.vm.clone` Web 3 present / 0 excluded.

## 비목표

- template를 Off로 강제
- linked clone
- current-evidence write, package-pair, Lane 2
