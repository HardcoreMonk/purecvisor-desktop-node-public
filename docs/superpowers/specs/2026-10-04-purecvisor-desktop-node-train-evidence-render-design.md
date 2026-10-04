# Release train 증적 렌더러 설계 (2단계)

- Design-ID: `pcv-train-evidence-render-v1`
- 작성일: `2026-10-04`
- 문서 상태: `accepted` (2026-10-04 사용자 승인 "train 2단계 증적 생성기")
- 변경 등급: M (검증 도구, 문서 계약)
- 상위 설계: `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-release-train-design.md` §6, §9 2단계
- host/VM/service/package mutation: `false`

## 1. 문제

0.42.89 train 한 바퀴에서 evidence `12`개를 손으로 썼다. pair `9`개(package, ops summary, update/rollback, clean-host, Burn, MSIX, descriptor, fullgate, current-card)와 Lane 3 `3`개(functional carry-forward, consume, main push)다. 모두 직전 문서를 복사해 값만 바꾼 것이다. 같은 틀을 매번 손으로 옮기면 값이 틀리거나 한 곳만 바뀌어도 검사하는 장치가 없다. 0.42.88 Lane 3 spec의 `package-build-current` 행이 package 경로를 `admin-smoke-package-20261003-04287`로 잘못 적은 것이 그런 예다.

## 2. 결정

틀(template)과 값(facts)을 나눈다. C# 렌더러가 facts 파일 하나로 evidence 문서를 만들고, Required CI가 커밋된 문서와 렌더 결과가 byte 단위로 같은지 시험한다.

| 구성 | 위치 |
| --- | --- |
| 틀 `12`개 | `docs/ga-ready/trains/templates/<template>.md.tmpl` |
| train별 facts | `docs/ga-ready/trains/<version>.evidence-facts.json` |
| 렌더러와 CLI | `src/DesktopNode.Verification/TrainEvidence/` (`pcvverify train-evidence`) |
| 시험 | `src/DesktopNode.Verification.Tests/TrainEvidence*Tests.cs` |

- 틀은 0.42.89 문서에서 값을 자리표시자로 바꿔 만든다. 첫 facts 파일은 `0.42.89-admin-smoke`이고, 렌더 결과가 커밋된 0.42.89 문서 `12`개와 같아야 한다(golden 시험).
- 틀 확장자를 `.md.tmpl`로 두어 evidence 문서 검사 도구가 틀을 evidence로 읽지 않게 한다.
- Lane 2 probe evidence는 probe마다 판정 기준이 달라 계속 손으로 쓴다. 계획 파일 실행 기록과 Lane 3 문서 도구 spec도 이 범위 밖이다.

## 3. facts 계약 `pcv-train-evidence-facts-v1`

```json
{
  "schema_version": 1,
  "contract": "pcv-train-evidence-facts-v1",
  "version": "0.42.89-admin-smoke",
  "documents": [
    { "template": "package", "path": "docs/ga-ready/evidence/admin-smoke-package-2026-10-04-04289.md", "values": { "evidence_id": "..." } }
  ]
}
```

- `template`은 틀 이름 `12`개 중 하나다. `path`는 `docs/ga-ready/evidence/` 바로 아래 `.md`다.
- `values`의 key는 `[a-z0-9_]+`, 값은 문자열이고 CR/LF를 담지 않는다.
- 문서 하나의 `values`에 틀이 쓰지 않는 key가 있거나, 틀이 쓰는 key가 없으면 실패한다.
- 같은 `path`가 두 번 나오면 실패한다.

## 4. 틀 문법

| 문법 | 뜻 |
| --- | --- |
| `{{key}}` | 값으로 바꾼다 |
| 줄 맨 앞 `{{?key}}` | 값이 비어 있지 않으면 표시를 지우고 줄을 남긴다. 비어 있거나 없으면 줄을 지운다 |

반복과 조건 블록은 없다. 표의 행 수는 틀이 고정한다. 선택 줄은 current-card 머리말처럼 Lane 3 전후로 줄이 달라지는 곳에만 쓴다.

## 5. CLI

```text
pcvverify train-evidence --facts <path> --check
pcvverify train-evidence --facts <path> --write [--allow-update <evidence path>]...
```

- `--check`는 쓰지 않고 비교한다. 다르거나 없는 문서가 있으면 exit `1`이고 stdout JSON에 문서별 상태(`current`, `stale`, `missing`)를 적는다.
- `--write`는 새 문서만 만든다. 이미 있는 문서의 내용이 다르면 실패한다. evidence는 새 파일로 쓴다는 규칙 때문이다. Lane 3의 current-card 머리말 승격처럼 정해진 갱신만 `--allow-update`로 문서를 하나씩 지정해 허용한다.
- 오류는 기존 `pcvverify` 오류 형식(`PCV_DEV_VERIFY_CONFIG_INVALID`, detail `train-evidence:<reason>`)을 따른다. 기존 `verify` 문법은 바꾸지 않는다.

## 6. train 절차에서 쓰는 곳

`docs/DEVELOPMENT_PROCEDURE.md` §10 train task에서 pair와 fullgate, current-card evidence를 쓸 때 직전 train facts 파일을 복사해 값을 바꾸고 `--write`한다. Lane 3는 Lane 3 문서 `3`개와 current-card 머리말 값을 facts에 더해 `--write --allow-update <current-card>`한다. 종료 검증의 `dotnet test`가 golden 시험으로 모든 facts 파일을 `--check`와 같은 방식으로 확인한다.

facts 값은 이번 단계에서는 사람이 채운다. runner 출력에서 facts를 자동으로 채우는 것은 host 상태(ARP, service, 설치본 hash, 사전 상태)를 실행 중에 기록해야 하므로 3단계 pair orchestrator 설계에서 다룬다.

## 7. 비목표

- 0.42.89 이전 evidence를 다시 렌더하거나 고치지 않는다.
- Lane 3 문서 도구(`Invoke-PcvLane3PromotionDocs.ps1`)와 그 spec 형식은 바꾸지 않는다.
- Required CI의 suite catalog와 shard 구성은 바꾸지 않는다. 시험은 `DesktopNode.Verification.Tests` 안의 xUnit이다.
