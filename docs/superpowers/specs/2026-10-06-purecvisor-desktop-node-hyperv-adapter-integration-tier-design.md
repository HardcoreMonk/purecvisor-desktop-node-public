# Hyper-V 어댑터 integration 단계 설계

- Design-ID: `pcv-hyperv-adapter-integration-tier-v1`
- 작성일: `2026-10-06`
- 문서 상태: `proposed` (2026-10-06 사용자 승인 `1,2,3,4`의 4, 설계만. 구현과 ADR은 별도 승인)
- 변경 등급: L (host mutation 경계를 다룬다. 제품 동작 변경 없음)
- host/VM/service/package mutation: `false` (이 문서 작성 중)
- public trusted signing / external stable publication: `false`

## 1. 문제

2026-09-27부터 2026-10-06까지 제품 수정 `9`건은 모두 실제 Hyper-V(설치본이나 actual VM) 단계에서 발견됐다. 단위 시험은 하나도
잡지 못했고, 수정할 때 회귀 시험을 넣었다(9건 모두 시험 파일 포함).

| 수정 commit | 결함 | 발견한 곳 |
| --- | --- | --- |
| `9402774` | Off VM을 inventory가 `stopped`로 보고하는데 export·network connect가 `Off`만 받음 | 0.42.78 설치본 P2 Off-VM. commit 메시지: "Route test fakes used "off" and hid the defect" |
| `017c008` | import가 자기 Gen2 export를 거부, private switch가 inventory를 깨뜨림 | `service-plan-p2-offvm-*-04279` FAIL |
| `a3dba76` | import가 원본 디스크 경로를 그대로 씀 | `service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04280` FAIL |
| `59e94d5`, `3945f88` | 이름 있는 external switch 분류, route DVD guard가 inventory DVD를 못 봄 | P2 Off-VM Lane 2 report-only |
| `77346bf` | guest 파일 job이 부모 폴더를 만들지 않음 | `service-plan-p1-guest-file-actual-vm-2026-09-28-04281` FAIL |
| `fb95de1` | DVD eject와 빈 드라이브 attach | 0.42.85 pair(승격하지 않음) |
| `96e8570` | managed delete가 VM 디스크를 남김 | Lane 2 pair runner |
| `c8be5bc` | 여러 줄 Notes를 원소 여러 개로 써서 template lock marker가 사라짐 | 0.42.90 설치본 완료 probe P1-7 FAIL |

비용: 발견까지 루프가 train(재시도 없이 `43`~`50`분, 0.42.91은 probe 재시도 포함 `102`분)이거나 dev probe(제품 Update/Rollback,
Lane 2 승인)다. 2026-09-28~29에는 probe 운반용 package가 `4`개(`0.42.79`~`0.42.82`) 나왔다(train 설계 §1). `HyperV.Tests`
(`257`개)는 WMI를 가짜 객체 경로(`\\HOST\root\virtualization\v2:...`)로만 시험한다.

## 2. 목표와 비목표

목표:

- `src/DesktopNode.HyperV`의 실제 WMI 동작을 설치본 없이 in-process로 몇 분 안에 확인한다.
- Lane 1 PR 전에 돌릴 수 있게 한다. 위 결함 가운데 guest OS가 필요 없는 `8`건(`77346bf` 제외)의 종류를 첫 시험 묶음으로 삼는다.

비목표:

- Required CI에 넣지 않는다(GitHub runner에는 Hyper-V가 없고, Required CI는 host mutation을 하지 않는다).
- train의 Lane 2 probe와 승격 근거를 대신하지 않는다. 결과는 `hyperv-integration` 관측이며 `actual_vm_tested`가 아니다.
- service, MSI, firewall, switch 생성, 보존 VM은 다루지 않는다.

## 3. 설계

### 3.1 시험 project

- `src/DesktopNode.HyperV.IntegrationTests`(xUnit). `DesktopNode.sln`과 `config/development-verification-suites.json`에 넣지
  않는다. 그래서 `dotnet test src/DesktopNode.sln`과 Required CI 네 shard는 이 project를 돌리지 않는다.
- 실행은 명시적으로만 한다: `dotnet test src/DesktopNode.HyperV.IntegrationTests -c Release`.
- 어댑터는 제품과 같은 경로로 만든다: `new DesktopNodeHyperVNativeAdapter(DesktopNodeHyperVProviderSet.CreateDefaultWmi())`.
  `DesktopNode.HyperV`의 `InternalsVisibleTo`에 이 project를 더한다.

### 3.2 실행 가드

fixture가 첫 시험 전에 확인하고, 하나라도 어긋나면 어떤 VM도 만들지 않고 전부 실패시킨다.

| 가드 | 조건 |
| --- | --- |
| 승인 | 환경 변수 `PCV_HYPERV_INTEGRATION_APPROVAL`이 비어 있지 않고, 그 값이 현재 campaign의 `approval_locator`에 들어 있다 |
| 권한 | 관리자 권한이거나 `Hyper-V Administrators` 구성원이다(2026-10-06 이 호스트 세션은 관리자, 그룹 구성원 아님) |
| 호스트 상태 | `Msvm_VirtualEthernetSwitch`를 읽을 수 있고 `Default Switch`가 있다. 없으면 `environment` 실패로 보고한다(2026-09 vmswitch 사례) |
| 이름 | 만들 VM은 모두 `pcv-it-<run id>-` 접두사다. 시작 때 VM 이름 목록을 저장한다 |
| 저장 위치 | VM 폴더와 디스크는 저장소 `artifacts/hyperv-integration/<run id>/` 아래에만 둔다 |

### 3.3 정리

- 시험마다 `finally`에서 그 시험의 VM을 지운다. fixture 끝에서 접두사 VM을 한 번 더 찾아 끄고 지우며, 저장 위치 폴더를 지운다.
- 끝 상태 검사: 시작 때 VM 이름 목록과 지금 목록이 같아야 한다(보존 VM 포함). 다르면 실패이고 남은 이름을 보고한다.
- 결과는 `artifacts/hyperv-integration/<run id>/summary.json`(시험별 결과, 소요 시간, 정리 결과, 남은 VM `0`)으로 남긴다.

### 3.4 첫 시험 묶음

| 시험 | 덮는 결함 |
| --- | --- |
| Gen2 VM 생성 후 inventory 상태가 `stopped`이고 export·network connect 정책이 받아들임 | `9402774` |
| export 뒤 import가 자기 Gen2 export를 받고 디스크를 새 VM 폴더로 복사함 | `017c008`, `a3dba76` |
| inventory가 `Default Switch`를 읽고 switch 분류가 바뀌지 않음(읽기 전용) | `59e94d5` |
| DVD attach, eject, 빈 드라이브 attach와 DVD guard readback | `fb95de1`, `3945f88` |
| managed delete가 그 VM 디스크를 지움 | `96e8570` |
| template lock이 Notes를 원소 하나로 쓰고 marker가 readback됨 | `c8be5bc` |

VM은 1 vCPU, 512 MB, 1 GB 디스크, 시작하지 않는 것이 기본이다. 목표 시간은 전체 `5`분 이내다.

### 3.5 절차 안의 자리

- `src/DesktopNode.HyperV/**`나 Hyper-V route handler를 바꾸는 Lane 1 변경은 PR 전에 이 단계를 돌린다. 결과는 계획 실행 기록에
  `hyperv-integration` 관측으로 적는다.
- train의 Lane 2 probe와 Lane 3 기준은 그대로다. dev probe(train 설계 §4.5)는 설치본 경로를 봐야 할 때만 남긴다.

## 4. 정책과 ADR

- 이 단계는 Hyper-V VM 생성·삭제라 host mutation이다. 지금 규칙(`CLAUDE.md` §6, `docs/DEVELOPMENT_PROCEDURE.md` §5 standing
  approval)은 checkpoint 하나와 기능군 하나에만 standing approval을 허용한다. Lane 1 변경마다 돌리려면 범위를 넓힌 standing
  approval이 필요하다.
- 그래서 구현 전에 ADR이 필요하다. 후보 `ADR-0016 Hyper-V adapter integration tier`가 정할 것:
  - 허용 범위: 접두사 VM의 생성·설정·삭제, 저장소 `artifacts/` 아래 디스크, 읽기 전용 switch 조회
  - 금지: service·MSI·firewall·switch 생성·보존 VM 변경·`Restart-Computer`
  - 승인 방식: 사용자가 standing approval 문장을 한 번 주고, `PCV_HYPERV_INTEGRATION_APPROVAL`이 그 locator를 가리킨다.
    철회는 그 문장을 campaign에서 빼는 것으로 한다
  - 실패 처리: 정리 실패는 다음 run을 막는다(남은 접두사 VM이 있으면 시작하지 않음)
- ADR 채택과 구현 승인 없이 이 단계를 돌리지 않는다.

## 5. 위험과 대응

| 위험 | 대응 |
| --- | --- |
| VM·디스크가 남음 | 시험별·fixture별 정리, 끝 상태 이름 목록 비교, 남으면 다음 run 차단 |
| 보존 VM이나 사용자 VM을 건드림 | 접두사 밖 VM에 대한 mutation 호출을 fixture가 거부, 시작·끝 이름 목록 비교 |
| 호스트 설정 변화(vmswitch 사례)를 제품 결함으로 오인 | 호스트 상태 가드가 `environment` 실패로 먼저 보고 |
| 설치된 service와 동시 실행 | 이름이 겹치지 않고 job store를 쓰지 않는다. 설치본 service 상태는 시작·끝에 읽기만 한다 |

## 6. 도입 단계

| 단계 | 내용 | 승인 |
| --- | --- | --- |
| 1 | ADR-0016 후보와 채택 결정 | 사용자 결정 |
| 2 | project, 가드, 정리, 첫 시험 `1`개(Off/stopped), Delivery 계약(sln·Required CI 제외 확인) | Lane 1 구현 승인과 standing approval |
| 3 | 나머지 시험과 §3.5 절차 반영 | 같은 승인 범위 |

## 7. Nonclaims

- 설계만이다. project, ADR, 절차, 호스트를 바꾸지 않았다. 이 문서를 쓰며 한 호스트 확인은 읽기 전용이다
  (현재 사용자 권한, `Msvm_ComputerSystem` 읽기).
- 이 단계의 PASS는 승격 근거가 아니다. public trusted signing과 external stable publication을 주장하지 않는다.
