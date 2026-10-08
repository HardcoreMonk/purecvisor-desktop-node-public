# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-10-09)

- 기준: `main` `6b76274`(PR #74 merge), operational current `0.42.93-admin-smoke`
- 판정: `pcvverify completion`(완료 정의 v3, ADR-0017) 읽기 전용 실행. 결과 `artifacts/completion/20261009/result.json`
- 직전 감사: `docs/project-status-audit-2026-10-06.md`(완료 정의 v1 C1~C6 대조)

## 1. 결론

완료 정의는 2026-10-08 ADR-0017로 "시연 가능한 사용자 시나리오" 기준(v3)이 됐다. 판정은 `complete=false met=1/7`이다.
충족은 C6(영구 범위 밖)뿐이다. S1~S4 시나리오는 아직 시연 기록이 없고, C1·C5는 `main` push Development Gates가
간헐 실패 시험 때문에 red이며 C5에는 2026-10-19 기한 위험도 남아 있다. v2의 운영 위생 항목(train, 기능 승격, SERVICE_PLAN
15개, backlog)은 모두 충족이다.

## 2. 조건별 상태

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 미충족 | `main` `6b76274` Development Gates run `37781431209` failure(`dotnet` shard, backlog `BL-0011`) |
| S1 브라우저에서 ISO로 VM 생성 → 브라우저 콘솔로 OS 설치 → 네트워크 확인 | 진행 중 | 기능은 PR #74로 merge. 이 호스트 dev probe 설치본(`0.42.94-admin-smoke`)에서 S1 시나리오 스크립트 PASS, Ubuntu 26.04.1 시연은 Task 5에서 멈춤(아래 4절) |
| S2 template 복제로 1분 안에 새 VM | 미착수 | 시나리오 스크립트·시연 없음 |
| S3 checkpoint 생성·복원·예약을 브라우저에서 | 미착수 | 기능은 있으나 시연 기록 없음 |
| S4 LAN의 다른 PC에서 계정으로 S1~S3 | 미착수 | LAN 노출 승인과 시연 필요 |
| C5 CI와 기한 위험 | 미충족 | Development Gates failure, `ubuntu-26-runner` 위험 2026-10-19까지 열림 |
| C6 영구 범위 밖 | 충족 | 5개 항목 |

운영 위생(판정에 넣지 않음): C2 `current=operational_current=0.42.93-admin-smoke`, train promoted, queue `0` / C3 후보 `4/4`
pass / C4 SERVICE_PLAN `15/15` / C7 backlog open `counts`·`undecided` `0`(이 감사가 새로 연 `BL-0011`, `BL-0012` 제외 전 기준).

## 3. 2026-10-06 이후 바뀐 것

- ADR-0017 채택(PR #73): 완료 정의 v3, 평소 기능 PR은 Lane 1 + Required CI + 설치본 smoke, release train은 시나리오 단계
  완료 때만, completion autopilot은 `status=paused`.
- S1 브라우저 콘솔(PR #74): WMI 화면(`GET /api/v1/vms/{vmId}/console/frame/{size}`, `console.view`)과 `Msvm_Keyboard` 입력
  (`POST /api/v1/vms/{vmId}/console/input`, 새 권한 `console.input`, 입력 내용 없는 audit `console-input-audit.jsonl`),
  Web VM 상세 Browser console 카드, `npm run scenario:s1-console`. route `82`개, feature `29`개.
- 콘솔 spike(PR #72): Hyper-V `2179` RDP 경로는 표준 협상 응답이 없어 보류, WMI 경로 채택. VMware Workstation 설치는 보류.
- backlog `BL-0010`(web public source safety 기존 실패) 수정(campaign `s1-installed-20261008` Task 1, 아직 PR 전).

## 4. 이 호스트 상태(2026-10-09)

- 설치본은 dev probe로 바뀐 `0.42.94-admin-smoke`(제품 코드 = `main` `6b76274`)다. 승격 근거가 아니며 campaign
  `s1-installed-20261008` Task 9가 Rollback으로 `0.42.93-admin-smoke`에 되돌린다.
- 시연 VM `pcv-it-s1-ubuntu`(Gen 2, Secure Boot `MicrosoftUEFICertificateAuthority`)가 Ubuntu 설치 프로그램 첫 화면에서 실행
  중이다. Task 5는 Lane 2 checkpoint 한도(tool batch 12회)를 넘어 멈췄고, 한도 연장 승인을 기다린다.

## 5. 남은 일과 결정

| 항목 | 종류 | 다음 행동 |
| --- | --- | --- |
| `BL-0011` 콘솔 rate limit 시험이 시간에 기대 간헐 실패 | backlog `undecided` | 시계 주입으로 결정적 시험으로 고침(사용자 분류 필요) |
| `BL-0012` `DesktopNodeHostLoopbackBootstrapBrowserTests` 재발(BL-0009 이후) | backlog `undecided` | runner 시간 여유 재조정(사용자 분류 필요) |
| S1 Ubuntu 시연 | 승인 대기 | Lane 2 한도 연장 승인 뒤 Task 5~8, Task 9 Rollback |
| S2~S4 | 다음 단계 | 시나리오별 campaign 승인 |
| C5 runner | 기한 대기 | 2026-10-19 뒤 campaign Task 11 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이며 완료를 주장하지 않는다. 완료는 `pcvverify completion` exit `0`일 때만 적는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
