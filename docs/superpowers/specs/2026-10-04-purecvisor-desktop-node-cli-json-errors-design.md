# PCVCLI `--json` 오류 출력 설계

- Design-ID: `pcv-cli-json-errors-v1`
- 작성일: `2026-10-04`
- 문서 상태: `implemented` (소스. 설치 반영은 다음 release train)
- 변경 등급: M (CLI 출력 계약)
- host/VM/service/package mutation: `false`

## 1. 문제

`pcvcli --json`은 성공 응답을 API envelope 그대로 stdout에 쓰지만, 오류는 형식과 상관없이 stderr에 `code=…`, `message=…`, `detail=…`, `Next action: …` 텍스트로만 쓰고 stdout을 비운다. 자동화가 `--json` 출력을 JSON으로 읽으면 오류 내용을 놓친다. 2026-10-03 reconcile 문구 Lane 2 probe(`lane2-reconcile-wording-actual-vm-2026-10-03-04287`)의 자동 판정이 이 때문에 비었다.

## 2. 결정

`--json`(또는 `--format json`)이면 오류도 stdout에 JSON envelope 하나로 쓴다. stderr 텍스트와 exit code는 바꾸지 않는다. stderr의 `code=` 줄을 읽는 기존 도구(예: manual-admin runner)와 사람이 읽는 출력을 깨지 않기 위해서다.

| 오류 | stdout (`--json`) |
| --- | --- |
| API envelope `{"ok":false,"error":{...}}` | 그대로. `ok`가 없으면 `false`를 더한다 |
| API root problem `{"code":...}` | `{"ok":false,"operation":"pcvcli","error":<원본>}` |
| JSON이 아닌 응답 | code `PCV_CLI_HTTP_<status>`, message는 본문 |
| usage 오류(exit `2`) | code `PCV_CLI_ARGUMENT_INVALID`. 오류 문구가 `PCV_*|`로 시작하면 그 code |
| token 등 실행 오류(exit `1`) | 오류 문구의 `PCV_*|` code, 없으면 `PCV_CLI_OPERATION_FAILED` |
| transport 오류(exit `1`) | `PCV_CLI_TRANSPORT_ERROR` |

`--json` 판단은 인자 파싱이 실패해도 할 수 있도록 원본 인자에서 `--json`과 `--format json`을 찾는다. table, plain, csv 형식의 오류 출력은 바뀌지 않는다(stdout 비움).

구현은 `src/DesktopNode.Cli/DesktopNodeCliErrorJson.cs`이고 `DesktopNodeCliApplication`이 오류 경로에서 부른다. `DesktopNodeCliApplicationTests`가 네 경우(API envelope, root problem과 비 JSON 응답, usage·transport 오류, table 형식 불변)를 시험한다.

## 3. Lane 2 probe (release train)

다음 train의 설치본에서 한다. 기능군은 `cli.json-errors`이고 VM을 만들지 않는다.

1. `pcvcli --protected-token-file <file> --json vm get pcv-no-such-vm`: exit `1`, stdout이 JSON으로 파싱되고 `ok=false`, `error.code`가 `PCV_`로 시작한다. stderr에 `code=` 줄이 있다.
2. `pcvcli --json no-such-command`: exit `2`, stdout JSON의 `error.code`가 `PCV_CLI_`로 시작한다.
3. `pcvcli vm get pcv-no-such-vm`(table): exit `1`, stdout이 비어 있다.
4. 출력에 token 형태 문자열이 없다.

## 4. 비목표

- 성공 응답의 `--json` 출력은 바꾸지 않는다.
- API 오류 응답의 모양은 바꾸지 않는다.
