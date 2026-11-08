# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-11-09, 완료와 운영 위생)

- 기준: `main` `67d3bbe`(PR #119 merge). operational current는 `0.42.99-admin-smoke`다.
- 판정: clean `main`에서 `pcvverify completion`(완료 정의 v3, ADR-0017)을 실행했다.
  - 결과: `complete=true met=7/7 gaps=0 head=67d3bbe2acd5244a45b4125096df930f6578cb0e`
  - 결과 파일: 로컬 `artifacts/completion/20261109-r2/result.json`(gitignore). 입력 CI 목록은 같은 폴더의 `main-runs.json`이다.
- 직전 감사: `docs/project-status-audit-2026-11-08.md`(`main` `2845ec4`, `met=7/7`, C7 미충족).

## 1. 결론

완료 정의 v3의 일곱 조건이 계속 충족되고, 판정 갭은 0이다. 이번에는 판정 밖 운영 위생 네 조건(C2, C3, C4, C7)도 모두
충족한다. 2026-11-08 감사에서 남았던 두 가지가 닫혔다.

- C7: backlog `undecided` 18건은 사용자 결정 2026-11-09 `"A 3건 닫기, C는 권장안대로, B 구현 시작"`대로 분류해 모두 닫았다.
  - 결정: PR #114
  - B1 도구·시험: PR #115
  - B2 Web·API: PR #116
  - campaign `backlog-burn-20261109`
- C2: B1·B2와 in-app 안내서(PR #113, #115, #116)를 release train `0.42.99`로 실어 operational current로 올렸다.
  - evidence: PR #118
  - Lane 3: PR #119
  - campaign `train-04299-20261109`

이 결론은 판정 결과를 옮긴 것이다.

## 2. 조건

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 충족 | `main` `67d3bbe` Development Gates success(run `42519197472`) |
| S1 브라우저에서 ISO로 VM 생성 → OS 설치 → 네트워크 확인 | 충족 | `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md` |
| S2 설치된 VM을 template으로 잠그고 복제 | 충족 | `docs/ga-ready/demo/s2-template-clone-demo-2026-10-09.md` |
| S3 checkpoint 생성·복원·예약을 브라우저에서 | 충족 | `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md` |
| S4 LAN의 다른 PC에서 계정으로 S1~S3 | 충족 | `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md`(2026-11-08 설치본 `0.42.98`에서 판정) |
| C5 CI와 기한 위험 | 충족 | Development Gates와 Public Boundary Contract(run `42519197448`) success. 열린 기한 위험 0 |
| C6 영구 범위 밖 | 충족 | `permanent_out_of_scope` 5개 항목 |

운영 위생(판정 밖):

| 조건 | 상태 | 값 |
| --- | --- | --- |
| C2 current와 train | 충족 | current `0.42.99-admin-smoke` = operational current, train `promoted`, queue 0, 미완료 train 0 |
| C3 feature qualification | 충족 | candidate 4개 모두 pass, `promotion_eligible=true` |
| C4 SERVICE_PLAN 항목 | 충족 | 15개 모두 닫힘 |
| C7 backlog 분류 | 충족 | `open_counts=0`, `undecided=0`(35행 모두 닫힘) |

## 3. 이 기간에 한 일 (2026-11-08 → 2026-11-09)

- backlog 18건(`backlog-burn-20261109`):
  - A 3건은 바로 닫았다(BL-0012, 0013, 0029).
  - C 4건은 권장안대로 정했다.
    - BL-0026: 라이트 테마 제거
    - BL-0030: N-1 update preflight fixture와 시험
    - BL-0033: 켜진 VM 복원 사전 거절
    - BL-0028: 보존 VM 부모 VHD 재연결
  - B 구현 15건을 두 PR로 고쳤다.
  - Lane 2에서 BL-0034를 재현했다. 원인은 삭제 중 VM 목록 요약 공백이 목록 전체를 실패시키는 것이었다.
- release train `0.42.99`(`train-04299-20261109`):
  - package, pair 여섯 bucket, fullgate, current-card가 PASS했다.
  - 설치본 probe 결과:
    - 삭제 중 VM 목록은 59회 읽어 실패 0회였다. 수정 전에는 삭제마다 1회 실패했다.
    - 켜진 VM 복원은 job 없이 `PCV_CHECKPOINT_RESTORE_VM_RUNNING`으로 거절됐다.
  - fullgate `REMOVE_DATA` 동안 계정 파일을 data root 밖에 보관했다가 되돌렸다.
- merge 규칙: 2026-11-09 사용자 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`으로 PR #117~#119를 merge했다.

## 4. 이 호스트

- 설치본: `0.42.99-admin-smoke+a95987b`(operational current와 같음, ARP 1개). service `PureCVisorDesktopNode`는 Running/Automatic이다.
- 네트워크: loopback 전용이다. Web `http://127.0.0.1/` 200, PureCVisor 방화벽 규칙 0개.
- 계정 3개: `pcv-admin` disabled, `pcv-admin2` admin enabled, `pcv-s4-operator` disabled. 계정이 있어 loopback 세션은 409로 거절된다.
- VM:
  - 보존 VM `pcv-guest-installed-04253-r1` Off. 부모 VHD는 public 저장소 `artifacts/image-cache` 파일로 다시 연결했다(BL-0028).
  - S2 template `pcv-it-s2-source` Off.
  - 그 밖의 `pcv-it-` VM은 0개다.
- train 계정 백업(백업 `accounts.json`, fullgate가 만든 빈 저장소, ACL 기록)은 사용자 승인 `"train 계정 백업 삭제 승인"`(2026-11-09)으로
  저장소 밖 보호 위치에서 지웠다. data root에 되돌린 계정 파일은 그대로다(해시와 ACL이 지우기 전과 같다).

## 5. 남은 일

| 항목 | 다음 행동 |
| --- | --- |
| report-only | Hyper-V 통합 시험 기본 ISO 경로가 없다(`PCV_HYPERV_INTEGRATION_ISO`로 지정). `web/src/modules/vm.ts`가 2,739줄이다(ratchet 상한). BL-0024 목록 밖 카드 문구가 영어로 남았다 |
| 다음 release train | queue에 product payload 변경이 생기면 출발 조건(DEVELOPMENT_PROCEDURE §10)대로 |
| completion autopilot | 정책 `status=paused`(ADR-0017) 그대로다. 재개 여부는 사용자 결정이다 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이다. 완료는 `pcvverify completion` exit `0`의 결과로만 적는다.
- 이 문서는 operational current를 바꾸지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 내부 사설망 전용 배포(ADR-0006) 범위다.
