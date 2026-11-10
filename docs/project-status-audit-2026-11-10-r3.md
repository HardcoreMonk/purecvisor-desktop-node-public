# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-11-10 r3, BL-0042 train 승격)

- 기준: `main` `1466b3c`(PR #139 merge). operational current는 `0.43.2-admin-smoke`다.
- 판정: clean `main`에서 `pcvverify completion`(완료 정의 v3, ADR-0017)을 실행했다.
  - 결과: `complete=true met=7/7 gaps=0 head=1466b3c46f4b3a67333ffd334bcccd070a25878f`
  - 결과 파일: 로컬 `artifacts/completion/20261110-r6/result.json`(gitignore). 입력 CI 목록은 같은 폴더의 `main-runs.json`이다.
- 직전 감사: `docs/project-status-audit-2026-11-10-r2.md`(`main` `5253b7e`, `met=7/7`, C7 `undecided=1`). 같은 날 세 번째 감사라 이름 끝에 `-r3`을 붙였다.

## 1. 결론

완료 정의 v3의 일곱 조건이 계속 충족되고, 판정 갭은 0이다. BL-0042 수정(PR #138)을 release train `0.43.2`로 실어 operational current로 올렸다. 설치본 Web Console의 version 표시가 이제 `v0.43.2-admin-smoke`로 보인다. 운영 위생 C2(current와 train)는 충족한다. C7(backlog 분류)은 이번 train의 설치본 probe에서 나온 `undecided` 1건(BL-0043)이 남아 충족하지 않는다. 그 분류는 사용자 결정이다.

이 결론은 판정 결과를 옮긴 것이다.

## 2. 조건

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 충족 | `main` `1466b3c` Development Gates success(run `42813686523`) |
| S1 브라우저에서 ISO로 VM 생성 → OS 설치 → 네트워크 확인 | 충족 | `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md` |
| S2 설치된 VM을 template으로 잠그고 복제 | 충족 | `docs/ga-ready/demo/s2-template-clone-demo-2026-10-09.md` |
| S3 checkpoint 생성·복원·예약을 브라우저에서 | 충족 | `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md` |
| S4 LAN의 다른 PC에서 계정으로 S1~S3 | 충족 | `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md` |
| C5 CI와 기한 위험 | 충족 | Development Gates와 Public Boundary Contract(run `42813686484`) success. 열린 기한 위험 0 |
| C6 영구 범위 밖 | 충족 | `permanent_out_of_scope` 5개 항목 |

운영 위생(판정 밖):

| 조건 | 상태 | 값 |
| --- | --- | --- |
| C2 current와 train | 충족 | current `0.43.2-admin-smoke` = operational current, train `promoted`, queue 0, 미완료 train 0 |
| C3 feature qualification | 충족 | candidate 4개 모두 pass, `promotion_eligible=true` |
| C4 SERVICE_PLAN 항목 | 충족 | 15개 모두 닫힘 |
| C7 backlog 분류 | 미충족 | `open_counts=0`, `undecided=1`(BL-0043) |

## 3. 이 기간에 한 일 (2026-11-10 r2 → r3)

- train `0.43.1` 계정 백업 삭제와 BL-0042 `counts` 분류(사용자 결정 `"1, 2 수정"`, PR #137).
- BL-0042 수정(`bl0042-version-badge-20261110`, PR #138):
  - runtime policy 응답과 `ops.summary`의 `runtime_policy`에 `product_version`을 더했다. 값은 Host가 자기 informational version을 읽어 넘긴다.
  - Web Console version 표시가 `v<version>`을 보이고 title에 전체 값을 둔다. 온라인 update 확인이 없으므로(ADR-0006) "✓ 최신 버전" 표시는 뺐다.
- release train `0.43.2`(`train-04302-20261110`, 단일 PR #139):
  - package, pair 여섯 bucket(`0.43.1 -> 0.43.2`), fullgate 2/2, current-card가 PASS했다.
  - 설치본 probe: 내주는 Web 파일 5개가 payload와 같다. `runtime/policy`·`ops.summary`의 `product_version`이 설치본 Host ProductVersion과 같다. version 표시가 1440·768에서 `v0.43.2-admin-smoke`로 보인다. console error 0, HTTP 4xx 0.
  - fullgate `REMOVE_DATA` 동안 계정 파일을 data root 밖에 보관했다가 되돌렸다.
  - Lane 3를 같은 PR에 담았다. `main-push-payload` evidence가 payload `00b10b66`의 main push run을 인용하고, path check에서 바뀐 pin은 허용 pin 6개뿐이다.
- merge 규칙: 2026-11-09 사용자 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`으로 PR #137~#139를 merge했다.

## 4. 이 호스트

- 설치본: `0.43.2-admin-smoke+2f19778`(operational current와 같음, ARP 1개). service `PureCVisorDesktopNode`는 Running/Automatic이다.
- 네트워크: loopback 전용이다. Web `http://127.0.0.1/` 200, PureCVisor 방화벽 규칙 0개.
- 계정 3개: `pcv-admin` disabled, `pcv-admin2` admin enabled, `pcv-s4-operator` disabled. 계정이 있어 loopback 세션은 409로 거절된다.
- VM: 보존 VM `pcv-guest-installed-04253-r1` Off, S2 template `pcv-it-s2-source` Off. 그 밖의 `pcv-it-` VM은 0개다.
- train 계정 백업(백업 `accounts.json`, fullgate가 만든 빈 저장소, ACL 기록)은 저장소 밖 보호 위치에 남아 있다. 지울지는 사용자에게 묻는다.

## 5. 남은 일

| 항목 | 다음 행동 |
| --- | --- |
| C7 backlog 분류 | BL-0043(설치본 대시보드 "경고" 카드가 `ops.summary`를 받은 뒤에도 "운영 요약을 읽지 못했습니다"를 보인다)을 counts 또는 out-of-scope로 정한다 |
| train 계정 백업 | 사용자 승인 뒤 지운다 |
| 다음 release train | queue에 product payload 변경이 생기면 출발 조건(DEVELOPMENT_PROCEDURE §10)대로 |
| completion autopilot | 정책 `status=paused`(ADR-0017) 그대로다. 재개 여부는 사용자 결정이다 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이다. 완료는 `pcvverify completion` exit `0`의 결과로만 적는다.
- 이 문서는 operational current를 바꾸지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 내부 사설망 전용 배포(ADR-0006) 범위다.
