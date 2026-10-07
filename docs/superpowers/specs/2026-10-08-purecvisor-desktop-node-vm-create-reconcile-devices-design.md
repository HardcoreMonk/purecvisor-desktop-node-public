# `vm.create` reconcile 지문에 연결 장치 넣기

- Design-ID: `pcv-vm-create-reconcile-devices-v1`
- 작성일: `2026-10-08`
- 문서 상태: `accepted` (2026-10-08 사용자 승인 `1,2,3,4,5,6,7`의 1, backlog `BL-0001` counts와 새 설계 승인)
- 관련: `pcv-interrupted-create-residue-v1` §6(이 문제를 범위 밖으로 남김), `pcv-p1-remaining-reconcile`(family별 조건부 reconcile)
- 변경 등급: M (Api reconcile 판정. 제품 payload 변경이며 다음 release train에 싣는다)
- host/VM/service/package mutation: `false`

## 1. 문제

`vm.create` reconcile은 `vm.list`에서 같은 이름의 VM이 하나이고 `managed_by_purecvisor=true`이며 generation이 같으면
`postcondition-confirmed`로 판정한다(`CreateFingerprintMatches`). create provider는 `DefineSystem`으로 VM을 등록한 뒤
`disk0.vhdx` 연결, ISO 연결, `Default Switch` 연결을 차례로 한다. 그 사이에 서비스 프로세스가 끊기면 VM은 managed 표식과
generation을 가진 채 장치 없이 남는다. 지금 판정은 이 VM을 "create 완료"로 보고 job을 성공으로 닫는다. 운영자는 장치 없는 VM을
정상 VM으로 받는다.

## 2. 결정

### 2.1 기대 장치

create가 끝까지 돌면 VM에는 다음 세 가지가 있다. 모두 provider가 항상 만드는 것이다.

| 장치 | 기대값 | `vm.list` 행에서 읽는 곳 |
| --- | --- | --- |
| 디스크 | `<vm_root>\<name>\disk0.vhdx`가 `attached=true` | `storage[]`의 `kind=vhdx`, `path`, `attached` |
| ISO | job 인자 `iso_path` | `dvd_media[]`의 `path` |
| switch | `Default Switch` | `network[]`의 `switch` |

`<vm_root>`는 job 인자 `vm_root`가 있으면 그 값, 없으면 기본 VM root(`DesktopNodeHyperVVmImportRequest.DefaultVmRoot`)다.
경로와 switch 이름은 대소문자를 구분하지 않는다. 경로는 `Path.GetFullPath`로 정규화해 비교한다.

### 2.2 판정

- 기대 장치를 baseline에 새로 저장하지 않고 reconcile 때 job 인자에서 계산한다. 그래서 업그레이드 전에 끊긴 job에도 같은
  규칙이 적용되고 baseline 계약 `pcv-vm-create-reconciliation/v1`은 바뀌지 않는다.
- 지금 조건(같은 이름 하나, managed, generation)에 더해 세 장치가 모두 맞아야 `postcondition-confirmed`다.
- managed VM이 하나 있고 generation은 맞지만 장치가 하나라도 빠지면 지금처럼 `target-fingerprint-mismatch`로 판정한다.
  hint에 빠진 장치(`disk`, `iso`, `switch`)와 "managed delete로 지우고 같은 이름으로 다시 create한다"는 회수 경로를 붙인다.
- reconcile은 계속 readback만 한다. 장치를 붙이거나 VM을 지우지 않는다.
- 판정 결과의 `reconciliation`에 `expected_devices`(디스크 경로, ISO 경로, switch)와 `missing_devices`를 남긴다.

### 2.3 시험

- `DesktopNode.Api.Tests`: 세 장치가 모두 맞으면 `postcondition-confirmed`. 디스크 미연결(`attached=false` 또는 없음), ISO 없음,
  switch 없음 각각 `target-fingerprint-mismatch`와 `missing_devices`, hint. `vm_root` 인자 반영. 경로 대소문자 무시.
- 기존 create reconcile 시험의 성공 fixture는 세 장치를 갖도록 바꾼다.

### 2.4 다음 train Lane 2 probe

설치본에서 probe VM `pcv-probe-*`로 `vm.create`를 queue하고, `vm.list`에 VM이 나타난 직후(장치 연결 전) 서비스 프로세스를
끊는다. reconcile이 `target-fingerprint-mismatch`와 빠진 장치를 내는지, managed delete와 같은 이름 create로 회복되는지 본다.
그 창이 너무 짧아 잡히지 않으면 r 시도 기록과 함께 code-level 시험을 근거로 남긴다. host mutation은 probe VM 생성·삭제와
서비스 재시작이다.

## 3. 버린 선택지

| 안 | 버린 이유 |
| --- | --- |
| baseline `expected_after`에 장치를 저장(schema v2) | 업그레이드 전 job에는 적용되지 않고 baseline reader와 계약이 넓어진다 |
| 장치가 빠진 VM을 reconcile이 지움 | reconcile readback 경계가 깨진다 |
| `DefineSystem` 전에 장치를 준비 | provider 순서와 WMI 호출을 크게 바꾼다. 판정만 고치면 오판정은 없어진다 |

## 4. Nonclaims

- Hyper-V exactly-once와 reconcile 완전성을 주장하지 않는다. 이 변경은 장치가 빠진 create를 완료로 보지 않게 할 뿐이다.
- public trusted signing과 external stable publication을 주장하지 않는다.
