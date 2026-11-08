# Lane 2 계정 비밀번호 재설정 거절 확인 `0.42.98-admin-smoke` (2026-11-08)

evidence_id: `lane2-account-password-reset-2026-11-08-04298`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.98-admin-smoke`
installed_product_version: `0.42.98-admin-smoke+2dfe2ac7e1af85a51b676127f83d264a0b57d840`
installed_host_sha256: `501b41f7304fca9831f1fbde9b6b34edb81aeadf623eb1ff61e85b0c420d6cc0`
installed_cli_sha256: `bfab42fdc3ae0389afd69ec7aa403c2e846438ff138c1a378bd76f7fbd74506a`
train_carriage: `PR #104` (`213bfe36`)
host_mutation_performed: `false`
account_mutation_performed: `false`
self_reset_path: `deferred-to-user-demo`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

train `0.42.98-admin-smoke` 적재 PR #104(계정 비밀번호 재설정과 변경)의 Lane 2 probe다. 기준은
`docs/superpowers/plans/2026-11-08-purecvisor-desktop-node-train-04298.md` Task 6이다.

설치본은 loopback 전용이고, S4 계정 저장소를 train 동안 치워 둔 상태다(`account list`: 계정 0개, `bootstrap_state=no-default-account`).
이 상태에서 계정 자격이 없는 호출자가 두 route를 부를 때 거절되는지만 본다.

- `POST /api/v1/accounts/{username}/password` (`account.password.reset`, PCVCLI `account reset-password`)
- `POST /api/v1/auth/password` (`auth.password.change`, PCVCLI `account change-password`)

대상 이름은 `pcv-s4-operator`다. 요청 본문의 비밀번호는 실행 때 임의로 만든 일회용 값이고, 출력하거나 저장하지 않았다. loopback 세션
token은 메모리에만 두었고, PCVCLI에는 환경 변수(`--token-env`)로만 넘겼다. 무효 bearer는 임의 문자열이다. 출력은 상태 코드와 오류
코드뿐이다. 실행 스크립트는 세션 작업 폴더의 일회용 `probe-password-reset-04298.ps1`이고 저장소에는 넣지 않았다.

## 결과

`2026-11-08T17:38+09:00`에 돌렸다.

| 호출 | 관측 | 판정 |
| --- | --- | --- |
| API reset, 인증 없음 | `401` `PCV_AUTH_REQUIRED` | `PASS` |
| API reset, 무효 bearer | `403` `PCV_AUTH_FORBIDDEN` | `PASS` |
| API reset, loopback 세션 | `409` `PCV_ACCOUNT_BOOTSTRAP_NOT_AVAILABLE` | `PASS` |
| API change, 인증 없음 | `401` `PCV_AUTH_REQUIRED` | `PASS` |
| API change, 무효 bearer | `403` `PCV_AUTH_FORBIDDEN` | `PASS` |
| API change, loopback 세션 | `403` `PCV_ACCOUNT_PASSWORD_CHANGE_FORBIDDEN` | `PASS` |
| CLI reset, 무효 token | exit `1` `PCV_AUTH_FORBIDDEN` | `PASS` |
| CLI change, 무효 token | exit `1` `PCV_AUTH_FORBIDDEN` | `PASS` |
| CLI reset, loopback 세션 | exit `1` `PCV_ACCOUNT_BOOTSTRAP_NOT_AVAILABLE` | `PASS` |
| CLI change, loopback 세션 | exit `1` `PCV_ACCOUNT_PASSWORD_CHANGE_FORBIDDEN` | `PASS` |
| CLI reset, 비밀번호를 argv(`--password`)로 | exit `2` `PCV_CLI_USAGE`(API 호출 전 거절) | `PASS` |
| CLI reset, `--yes` 없음 | exit `2` `PCV_CLI_CONFIRMATION_REQUIRED` | `PASS` |

- 어느 응답에도 `access_token`이나 `refresh_token`이 없다.
- 계정 변경 감사 파일 `account-mutation-audit.jsonl`은 실행 전후 모두 없다. 재설정과 변경이 한 번도 받아들여지지 않았다.
- 실행 뒤 `account list`는 여전히 계정 0개다.

loopback 세션의 reset 거절 코드는 `PCV_ACCOUNT_BOOTSTRAP_NOT_AVAILABLE`이다. reset이 계정 생성과 같은 인증 경로(`RejectReadyAuth`)를 써서,
계정이 없을 때 loopback 세션 요청을 bootstrap 요청으로 보고 거절한다. 거절은 맞지만 코드 이름이 재설정 맥락과 다르다(report-only).

## 미룬 확인

계획의 둘째 단계는 Task 7 계정 복원 뒤 에이전트용 operator 계정 `pcv-s4-operator`로 본인 재설정 경로(로그인, 재설정 뒤 옛 token
`401`, 새 비밀번호 로그인)를 도는 것이다. 에이전트는 그 계정의 현재 비밀번호를 갖고 있지 않다(계정 비밀번호는 실행 경계에서 만들고
남기지 않는다). service bearer로 비밀번호를 바꾸는 것은 계획 문장 밖의 계정 변경이라 하지 않았다. 계획대로 이 단계는 Task 12 S4 사용자
시연 항목으로 넘긴다. 옛 token 무효화(`credential_version`, `PCV_TOKEN_CREDENTIAL_STALE`)는 PR #104의 단위 시험이 덮는다.

## 끝 상태

- host mutation 없음, 계정 변경 없음. service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0개.
- 보존 VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running. 그대로다.
- operational current는 `0.42.96-admin-smoke` 그대로다.
