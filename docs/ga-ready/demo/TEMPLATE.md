# <시나리오 id> <제목> 시연 (<yyyy-mm-dd>)

- 시나리오: `<S1|S2|S3|S4>`. <ADR-0017 표의 시나리오 문장>.
- 기준: `config/project-completion-criteria.json`의 <시나리오 id>. ADR-0017.
- 이 기록은 이 호스트 설치본 `<version>`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. 다른 시나리오는 주장하지 않는다.

## 설치본

- 설치본 version, 기준 commit, package 모드(`AllowUnsignedDev` / `LocalTest`), 시연 뒤 Rollback 여부.
- service 상태, Web HTTP 상태, 보존 VM 상태.

## 실행

- 명령: `<스크립트와 인자>`
- 결과: `summary.json` `result=<pass|fail>`, 소요 시간, job 결과.

## 화면 캡처

캡처는 git 밖 `artifacts/`가 아니라 `docs/ga-ready/demo/<시나리오 id>-<yyyymmdd>/` 아래에 PNG로 보존한다. 파일당 500 KB
이하로 줄이고, 사용자 홈 경로·LAN 사설 IP·호스트명·token이 보이면 가린다(AGENTS.md 저장소 경계).

- `docs/ga-ready/demo/<시나리오 id>-<yyyymmdd>/<장면>.png`: <무엇을 보여 주는지 한 줄>

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | <사람 이름 또는 역할. 에이전트가 아니다> |
| 확인 일시 | <yyyy-mm-dd HH:mm, Asia/Seoul> |
| 확인 방법 | <직접 화면을 봤다 / 캡처를 검토했다> |

확인자 칸이 비어 있으면 `pcvverify completion`이 passed로 세더라도 이 기록은 "에이전트 자가 보고"다(2026-10-09 감사 §10).

## 정리

- 만든 VM의 끝 상태(`pcv-it-` 신규 VM 0개), Rollback 결과, 보존 VM 상태.

## 판정

- `pcvverify completion` 결과 줄과 `artifacts/completion/<yyyymmdd>/result.json` 경로. 완료를 주장하지 않는다.
