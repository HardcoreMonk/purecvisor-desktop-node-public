# Desktop Node 프로젝트 완료 정의 v2

- Design-ID: `pcv-project-completion-definition-v2`
- 작성일: `2026-10-07`
- 문서 상태: `accepted` (2026-10-07 사용자 승인 `1,2,3,4,5`의 1). 2026-10-08 ADR-0017(`docs/adr/0017-scenario-delivery-completion.md`)
  채택으로 역사 기록이 된다. `pcvverify completion`은 v3 전환(campaign `adr17-s1-console-20261008` Task 2)까지 이 문서를 따른다.
- 대체: `pcv-project-completion-definition-v1`(`docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-completion-definition-design.md`).
  v1 §6("이 정의를 바꾸려면 새 결정 문서를 쓴다")에 따른 새 문서다. v1은 고치지 않는다.
- 변경 등급: S (판정 기준 문서. 제품 동작 변경 없음)
- 관련: 설계 `pcv-completion-autopilot-v1`(`docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-autopilot-design.md`),
  `pcv-campaign-runner-v2`, `docs/DEVELOPMENT_PROCEDURE.md` §10(release train)
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`

## 1. 문제

- v1 판정은 사람이 쓴 표였다. 2026-10-07 PR #59(product payload, `vm.create` 잔여물 회수)가 merge되어
  `release-train.json` `queue`에 행이 생기면서 C2가 다시 미충족이 됐다. 그런데 v1 §4 표와 감사 문서는 "충족"으로 남았고,
  직전 campaign의 Task 13은 C5만 확인한 뒤 "프로젝트 완료 판정"을 적도록 되어 있었다.
- campaign의 `report-only` 발견은 최종 보고와 plan 실행 기록에만 남았다. 예를 들어 `vm.create` reconcile 지문에 장치 연결이
  빠진 문제(설계 `pcv-interrupted-create-residue-v1` §6)와 QoS readback `mutation_supported: false`가 있다. v1은 이런 발견을
  남은 일로도, 범위 밖으로도 세지 않았다.
- C5의 기한 위험은 감사 문서 §4 표에만 있어서 기계가 기한을 읽을 수 없었다.

## 2. 결정

### 2.1 조건

Desktop Node 프로젝트는 아래 C1~C7이 동시에 참일 때 완료다. C1~C6의 뜻은 v1 §2와 같다.

| # | 조건 | 판정 출처 |
| --- | --- | --- |
| C1 | 제품 런타임 GA-ready 유지. GA 범위 current route·product operation에 transition-helper·blocked 행이 없고 제품 경로에 PowerShell helper가 없다 | `main` Required CI `dotnet`·`delivery` shard(ADR-0004 aggregate gate) |
| C2 | operational current가 마지막 product payload를 담는다. `current-evidence.json` current와 `release-train.json` `operational_current`가 같고, 그 train이 `promoted`이며, `queue`가 비어 있다 | `docs/ga-ready/current-evidence.json`, `docs/ga-ready/release-train.json` |
| C3 | 기능 승격 후보의 필수 stage 다섯 개가 모두 `pass`다 | `config/desktop-node-feature-evidence-ledger.json` |
| C4 | SERVICE_PLAN §7.1 P0~P2 `15`개 항목이 설치본 actual-VM PASS evidence로 닫히거나 면제 결정 문서가 있다 | `config/project-completion-criteria.json` 항목 목록 |
| C5 | `main` Required CI(Development Gates, Public Boundary)가 green이고 기한이 있는 위험이 열려 있지 않다 | GitHub Actions `main` run, criteria `deadline_risks` |
| C6 | 영구 범위 밖 항목(§3)은 남은 일로 세지 않는다 | 이 문서 |
| C7 | backlog에 `status=open`이면서 분류가 `counts` 또는 `undecided`인 행이 없다 | `docs/ga-ready/backlog.json` |

### 2.2 기계 판정 출처

- `config/project-completion-criteria.json`(계약 `pcv-project-completion-criteria-v1`)이 조건별 판정 출처를 담는다.
  C4 항목마다 id, 제목, 설치본 evidence id 목록 또는 면제 결정 문서 경로가 있고, C5 기한 위험마다 id, 내용, 기한,
  상태(`open`, `closed`), 닫은 근거가 있다.
- 판정 명령 `pcvverify completion`(설계 `pcv-completion-autopilot-v1`)이 C1~C7을 판정한다. 프로젝트 완료는 이 명령이
  exit `0`을 낸 `main` HEAD에서만 적는다. 사람이 쓴 판정 표는 기록이고 판정 근거가 아니다.
- 날짜별 감사 문서는 계속 쓸 수 있다. 감사가 판정을 인용할 때는 명령 결과(HEAD SHA와 조건별 값)를 옮긴다.

### 2.3 backlog

- campaign의 `report-only` 발견은 그 checkpoint 안에서 `docs/ga-ready/backlog.json`(계약 `pcv-backlog-v1`)에 한 행으로
  쓴다. 행은 id, 발견 날짜, 출처(plan task 또는 설계 절), 요약, 분류, 분류 근거, 상태를 가진다.
- 분류는 세 가지다.
  - `counts`: 남은 일이다. Lane 1 수정이나 새 설계로 닫는다. 닫으면 `status=closed`와 닫은 PR을 적는다.
  - `out-of-scope`: §3 항목 하나를 근거로 적어야 한다. 근거가 없으면 판정기가 `undecided`로 센다.
  - `undecided`: 새 행의 기본값이다. 분류는 사용자 결정이다.
- 분류를 사람이 바꾸면 분류 근거에 결정 날짜와 승인 문장을 적는다.

### 2.4 위험 목록 이관

감사 2026-10-06 §4의 행(GitHub `ubuntu-latest`의 Ubuntu 26 이동, 기한 2026-10-19)을 criteria `deadline_risks`로 옮긴다.
감사 문서는 고치지 않는다. 이후 기한 위험은 criteria 파일에만 더한다.

## 3. 영구 범위 밖

v1 §3과 같다. 완료율 계산과 남은 일 목록에 넣지 않는다.

- public trusted signing (2026-10-06 사용자 결정, ADR-0004·ADR-0006)
- external stable publication, 일반 사용자용 public release
- VMware Workstation 기능 패리티 100% (SERVICE_PLAN §10)
- Hyper-V exactly-once 보장, reconcile 완전성 주장
- Linux `purecvisor-single`, KVM/libvirt/LXC/ZFS/OVS/OVN 기능

reconcile이 실제와 다른 상태를 `postcondition-confirmed`로 판정하는 것은 "완전성"이 아니라 판정 오류일 수 있다. 그래서
해당 backlog 행은 `undecided`로 두고 사용자가 분류한다.

## 4. 2026-10-07 판정

`main` `3f55831` 기준이다. 판정기는 아직 없으므로 사람이 v1 판정과 오늘 바뀐 파일을 대조했다. 판정기가 생기면
(같은 campaign Task 8) 그 결과로 대체한다.

| 조건 | 상태 | 남은 것 |
| --- | --- | --- |
| C1 | 충족 | 없음 (`main` `3f55831` Development Gates success) |
| C2 | 미충족 | `queue` 1행(PR #59, `ee90474`). train `0.42.92` 출발과 승격 |
| C3 | 충족 | 없음 (후보 `4/4`) |
| C4 | 충족 | 없음 (v1 §4 보정, P1-9 면제) |
| C5 | 미충족(기한 대기) | 2026-10-19 뒤 첫 `main` run |
| C6 | 충족 | 없음 |
| C7 | 미충족 | backlog `undecided` 2행(`vm.create` reconcile 지문, QoS `mutation_supported: false`). 같은 campaign Task 3이 등록한다 |

충족 `4/7`.

## 5. 남은 일

1. backlog 2행 분류(사용자 결정). `counts`면 Lane 1 수정이나 새 설계로 닫는다.
2. train `0.42.92` 출발과 승격(C2). 적재는 PR #59, Lane 2 probe 기능군은 `vm.create`다.
3. 2026-10-19 뒤 첫 `main` run 확인(C5). 실패하면 `ubuntu-24.04` pin을 판단한다.

이 목록은 `pcv-completion-autopilot-v1`이 판정 결과에서 다시 만든다. 여기 적힌 목록은 2026-10-07 시점 기록이다.

## 6. 유지

- 완료는 GA 선언이 아니다. 완료 뒤에도 내부 사설망 admin-smoke 범위이며 public trusted signing과 external stable
  publication을 주장하지 않는다.
- 완료는 시점 판정이다. 완료 뒤 product payload가 merge되면 C2가 다시 미충족이 된다.
- 이 정의를 바꾸려면 새 결정 문서를 쓴다. 감사 문서는 날짜마다 새로 쓰고 기존 감사를 덮어쓰지 않는다.
