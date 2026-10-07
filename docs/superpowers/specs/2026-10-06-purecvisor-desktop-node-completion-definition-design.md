# Desktop Node 프로젝트 완료 정의

- Design-ID: `pcv-project-completion-definition-v1`
- 작성일: `2026-10-06`
- 문서 상태: `accepted` (2026-10-06 사용자 승인 `1, public trusted signing, 외부 stable 배포는 없음`)
- 대체: 2026-10-07 `pcv-project-completion-definition-v2`(`docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-definition-v2-design.md`). 현재 기준은 v2다.
- 변경 등급: S (판정 기준 문서. 제품 동작 변경 없음)
- 입력: `docs/project-status-audit-2026-10-06.md`
- 관련: ADR-0004(GA-ready 제품 런타임), ADR-0006(내부 사설망 배포), ADR-0015(기능 evidence 승격), `docs/SERVICE_PLAN.md` §7.1·§9·§10
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`

## 1. 문제

저장소에는 "프로젝트 완료"의 정의가 없었다. SERVICE_PLAN은 Workstation 패리티 100%와 GA 선언을 목표에서 뺐고, 완료가
정의된 층(GA-ready 런타임, operational current, 기능 승격, 서비스 기획)은 문서마다 따로 있다. 그래서 "얼마나 남았는가"에
같은 답을 낼 수 없었다.

## 2. 결정

Desktop Node 프로젝트는 아래 C1~C6이 동시에 참일 때 완료다. 각 조건은 저장소의 파일이나 CI 결과로 확인한다.

| # | 조건 | 확인 방법 |
| --- | --- | --- |
| C1 | 제품 런타임 GA-ready가 유지된다. GA 범위 current route·product operation에 transition-helper·blocked 행이 없고, 제품 경로에 PowerShell helper가 없다 | ADR-0004 aggregate gate, Required CI `dotnet`·`delivery` shard |
| C2 | operational current가 마지막 product payload를 담는다. `current-evidence.json` current와 `release-train.json` `operational_current`가 같은 version이고, 그 train이 `promoted`이며, `queue`가 비어 있다 | `docs/ga-ready/current-evidence.json`, `docs/ga-ready/release-train.json` |
| C3 | 기능 승격 후보가 모두 승격 가능하다. 후보 feature의 필수 stage 다섯 개가 모두 `pass`다 | `config/desktop-node-feature-evidence-ledger.json`, ADR-0015 evaluator |
| C4 | SERVICE_PLAN §7.1 P0~P2 `15`개 항목이 각각 설치본 actual-VM PASS evidence로 닫히거나, 설치본 evidence가 필요 없다는 결정이 문서로 있다. evidence는 operation 단위로 `docs/FEATURE_IMPLEMENTATION_LEDGER.md` 후보 stage 표 또는 "비후보 actual-VM 관측" 절에 있다 | `docs/project-status-audit-<date>.md` §3 |
| C5 | `main`의 Required CI(Development Gates 네 shard, Public Boundary)가 green이고, 기한이 있는 알려진 위험이 열려 있지 않다 | GitHub Actions `main` run, 감사 §4 |
| C6 | 영구 범위 밖 항목(§3)은 남은 일로 세지 않는다 | 이 문서 |

비후보 feature `24`개는 C4로 판정한다. 2026-10-03 결정대로 feature evidence ledger를 넓히지 않으며, 완료 판정을 위해
후보를 늘리지 않는다.

## 3. 영구 범위 밖

다음은 "나중에 할 일"이 아니라 이 제품이 하지 않는 일이다. 완료율 계산과 남은 일 목록에 넣지 않는다.

- public trusted signing (2026-10-06 사용자 결정, ADR-0004·ADR-0006)
- external stable publication, 일반 사용자용 public release (같은 근거)
- VMware Workstation 기능 패리티 100% (SERVICE_PLAN §10)
- Hyper-V exactly-once 보장, reconcile 완전성 주장
- Linux `purecvisor-single`, KVM/libvirt/LXC/ZFS/OVS/OVN 기능

## 4. 2026-10-06 판정

감사 `docs/project-status-audit-2026-10-06.md` 기준이다.

| 조건 | 상태 | 남은 것 |
| --- | --- | --- |
| C1 | 충족 | 없음 |
| C2 | 충족 | 없음 (`0.42.90-admin-smoke`, queue `0`) |
| C3 | 충족 | 없음 (후보 `4/4`) |
| C4 | 미충족 | 설치본 evidence 없음 `4`개(P1-6 inventory 시각·메모, P1-7 template lock, P1-9 admin account CRUD, P2-11 noVNC target 설정), 부분 `1`개(P1-10 family별 조건부 reconcile) |
| C5 | 미충족(기한 대기) | 2026-10-19 GitHub `ubuntu-latest` Ubuntu 26 이동 뒤 첫 `main` run 확인 |
| C6 | 충족 | 없음 |

보정(2026-10-06, release train `0.42.91`): C4의 남은 항목을 설치본 `0.42.91`에서 확인했다. P1-6과 P1-7은 PASS다(`lane2-completion-inventory-template-lock-actual-vm-2026-10-06-04291`). P1-7은 0.42.90에서 FAIL했고 PR #53 수정으로 닫혔다. P2-11은 PASS다(`lane2-completion-novnc-target-2026-10-06-04291`). P1-10은 다섯 family 모두 끊긴 job의 reconcile 판정이 실제 상태와 일치했다(`lane2-completion-family-reconcile-actual-vm-2026-10-06-04291`, not-applied 경로. confirmed 경로는 0.42.84 evidence와 code-level 시험). P1-9는 사용자 결정(2026-10-06)으로 설치본 evidence 불필요다(계정 없는 bootstrap이 의도된 기본값이고 probe가 인증 상태를 바꾼다). 그래서 C4는 충족이다. C2는 train `0.42.91` Lane 3 merge로 다시 충족된다. C5는 2026-10-19 확인을 기다린다.

## 5. 남은 일

C4와 C5만 남는다. 새 기능 구현은 필요 없다.

1. Lane 2 설치본 probe (operational current `0.42.90` 설치본, product payload 변경 없음): P1-6, P1-7, P1-9, P2-11을
   operation 단위로 실행하고 PASS면 `FEATURE_IMPLEMENTATION_LEDGER` 비후보 관측 절에 행을 더한다. host mutation은 probe VM
   생성·삭제, probe 계정 생성·비활성화, noVNC target 설정과 원복이다.
2. Lane 2 설치본 probe: P1-10 family별 reconcile(create, shutdown, restart, QoS)을 job 중단 뒤 reconcile로 확인한다.
   0.42.84 전원 상태 reconcile probe와 같은 방식(서비스 프로세스 강제 종료와 재시작)이다.
3. 2026-10-19 뒤 첫 `main` run이 green인지 확인한다. 실패하면 `ubuntu-24.04` pin을 판단한다(감사 2026-10-03 §12).

probe에서 결함이 나오면 그 항목은 Lane 1 수정과 release train으로 닫는다. 그때는 C2를 다시 확인한다.

## 6. 유지

- 완료는 GA 선언이 아니다. 완료 뒤에도 내부 사설망 admin-smoke 범위이며 public trusted signing과 external stable publication을 주장하지 않는다.
- 이 정의를 바꾸려면 새 결정 문서를 쓴다. 감사 문서는 날짜마다 새로 쓰고 기존 감사를 덮어쓰지 않는다.
