# Desktop Node P1-9 admin account CRUD 설계

- Design-ID: `purecvisor-desktop-node-p1-admin-account-crud-v1`
- 작성일: `2026-09-20`
- 문서 상태: `implemented-slice-2`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P1-9
- 선행: `pcv.account.session` login/refresh/logout/session/rbac, loopback session, `no-default-account` bootstrap
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

설치본은 `accounts.json`과 `jwt-signing-key.txt`를 만들지만 계정은 0개다.
`bootstrap_state=no-default-account`이며 login은 `409 PCV_ACCOUNT_AUTH_NOT_CONFIGURED`다.
운영자가 지원 경로로 첫 admin을 만들거나 이후 계정을 끄지 못한다. 파일 직접 편집은 제품
표면이 아니다. SERVICE_PLAN P1-9는 **create/disable만** 열고 기본 계정을 만들지 않는다.

## 현재 계약

- Feature `pcv.account.session` route 6개. Web/API present, CLI excluded (PCVCLI는 service bearer).
- Role `viewer` / `operator` / `admin`. `account.manage`는 admin만. 이 permission에 붙은 route는 아직 없다.
- `Ready` = signing key + account 1개 이상. Ready가 아니면 loopback session(role=`operator`)과 service bearer가 권위다.
- Ready가 되면 loopback session은 `409 PCV_LOOPBACK_SESSION_DISABLED`. service bearer는 유지한다.
- `DesktopNodeAccountAuthService`는 기동 시 `FromFiles`로 한 번 읽고, 이후 파일 쓰기를 하지 않는다.
- `DesktopNodeAccountAuthFile`는 issuer/audience/accounts만 역직렬화한다. persist는
  `schema_version`과 `bootstrap_state`를 보존해야 한다.
- 28 feature 유지. P0 evidence 후보는 04275 기준 4개 그대로.

## 결정

- Feature는 새 ID를 만들지 않는다. `pcv.account.session`에 붙인다.
- 기본 계정을 MSI/configure/bootstrap이 만들지 않는다. `no-default-account` 빈 파일은 유지한다.
- 연산은 create와 disable만. delete, rename, role 변경, password reset, 재활성은 이 설계 밖이다.
- HTTP는 동기 RuntimeProductOperation / RuntimeReadOnly다. Hyper-V job이 아니므로 queued mutation이 아니다.
- persist는 `%ProgramData%\PureCVisor\desktop-node\accounts.json` 원자 교체 + in-process reload. service restart를 요구하지 않는다.
- password는 기존 `pbkdf2-sha256` (`DesktopNodeAccountPassword`)만 쓴다. list/API/CLI/Web/log에 hash를 넣지 않는다.
- disable된 계정 login은 기존과 같이 `401 PCV_LOGIN_FAILED`다. 존재 여부를 나누지 않는다.
- 마지막 enabled admin은 disable할 수 없다 (`PCV_ACCOUNT_LAST_ADMIN`). store가 다시 빈 `no-default-account`로 돌아가지 않는다.
- 첫 계정은 반드시 `admin`이다. 그 한 번은 loopback session 또는 service bearer로 만들 수 있다.
  loopback principal은 `operator`라 `account.manage`가 없다. 이 예외는 `!Ready`이고 loopback remote일 때만이다.
- Ready 이후 create/disable/list는 `account.manage` 또는 service bearer다.
- CLI는 session login을 계속 제외한다. account list/create/disable만 CLI present다.
- 비밀번호는 CLI argv에 두지 않는다. `--password-env` 또는 stdin.

## Route (slice 3)

| operation_id | method / template | stance | permission |
| --- | --- | --- | --- |
| `account.list` | `GET /api/v1/accounts` | ReadOnly | `account.manage` (`!Ready`이면 loopback 또는 service bearer, 빈 목록) |
| `account.create` | `POST /api/v1/accounts` | ProductOperation | `!Ready`: loopback 또는 service bearer, role은 admin만. Ready: `account.manage` |
| `account.disable` | `POST /api/v1/accounts/{username}/disable` | ProductOperation | `account.manage`. body `confirm_username`이 path와 같아야 한다 |

이후 catalog: routes 65→68, ProductOperation 14→16, ReadOnly 22→23, queued 29 유지.
Digest는 slice 3에서 다시 pin한다. Hyper-V domain/dispatch는 건드리지 않는다.

Create body: `username`, `password`, `role`, optional `display_name`.
Create 응답: id, username, role, display_name, enabled, bootstrap_state. password 없음.
Disable 응답: username, enabled=false, action=`disable` 또는 이미 꺼져 있으면 `already-disabled`.
List 항목에 `password_hash` 없음.

## 계정 레코드

기존 필드에 `enabled` (기본 true)를 더한다. disable은 `enabled=false`와 `disabled_at`(UTC round-trip)만
쓴다. username은 대소문자 무시 중복 금지, `^[A-Za-z][A-Za-z0-9._-]{2,31}$`, 예약어
`loopback-session`. password 최소 12자, username과 같으면 거절. role은 `viewer`/`operator`/`admin`.

`bootstrap_state`: 계정 0개면 `no-default-account`. 첫 성공 create 이후 `accounts-configured`.

## 거절 코드

| code | 때 |
| --- | --- |
| `PCV_ACCOUNT_USERNAME_INVALID` | 형식/예약어 |
| `PCV_ACCOUNT_USERNAME_CONFLICT` | 중복 |
| `PCV_ACCOUNT_PASSWORD_INVALID` | 비어 있음, 12자 미만, username과 동일 |
| `PCV_ACCOUNT_ROLE_INVALID` | viewer/operator/admin 아님 |
| `PCV_ACCOUNT_BOOTSTRAP_ADMIN_REQUIRED` | 첫 계정이 admin이 아님 |
| `PCV_ACCOUNT_BOOTSTRAP_NOT_AVAILABLE` | Ready인데 bootstrap 예외를 쓰려 함, 또는 비-loopback에서 loopback 예외 |
| `PCV_ACCOUNT_NOT_FOUND` | disable 대상 없음 |
| `PCV_ACCOUNT_LAST_ADMIN` | 마지막 enabled admin disable |
| `PCV_ACCOUNT_CONFIRMATION_MISMATCH` | `confirm_username` ≠ path |
| `PCV_ACCOUNT_MANAGE_FORBIDDEN` | Ready 이후 `account.manage` 없음 (기존 `PCV_RBAC_FORBIDDEN`으로 매핑해도 됨) |

이미 disable된 대상은 성공 `already-disabled` (template-lock idempotent와 같음).

## 첫 계정 (bootstrap)

1. 설치 직후 Web loopback은 기존 `POST /api/v1/auth/loopback-session`으로 operator JWT를 받는다.
2. 그 세션으로 `POST /api/v1/accounts`에 첫 admin을 만든다. LAN/비-loopback은 이 예외가 없다.
3. CLI는 같은 create를 service bearer로 호출한다 (`pcvcli`는 token file을 이미 읽는다).
4. persist 직후 in-memory `Ready=true`. 이후 loopback session 발급은 닫힌다.
5. 기존 loopback access token은 만료까지 남을 수 있다. account CRUD는 Ready 이후 loopback token을
   받지 않는다. 운영자는 login으로 admin JWT를 받는다.

## Slice 1 범위

- `AccountMutationContract.EvaluateCreate` / `EvaluateDisable` (순수, IO 없음).
- 거절 코드와 username/password/role/last-admin/bootstrap 규칙을 테스트로 고정한다.
- HTTP, persist, CLI, Web은 이 slice가 아니다.

## Slice 2 범위

- `accounts.json` 원자 쓰기, ACL harden, in-process reload.
- `enabled` / `disabled_at` / `bootstrap_state` round-trip. extra JSON 필드 보존.
- Login이 disable 계정을 `PCV_LOGIN_FAILED`로 거절한다. Refresh는 `PCV_REFRESH_ACCOUNT_NOT_FOUND`.
- HTTP/CLI/Web은 이 slice가 아니다.

## Slice 3 범위

- 위 3 HTTP route, frozen catalog/digest/`http-transport-contract-v1` `route_count`.
- CLI: `pcvcli account list`, `pcvcli account create --username NAME --role ROLE --password-env VAR|--password-stdin --yes`, `pcvcli account disable NAME --yes`.
- `--yes` 없는 create/disable은 `PCV_CLI_CONFIRMATION_REQUIRED`.
- Web은 excluded.

## Slice 4 범위

- Web Account panel: `!Ready`+loopback이면 Create first admin. Ready+`account.manage`이면 list/create/disable.
- coverage_id `account.list` / `account.create` / `account.disable`.
- password input은 submit 후 DOM에서 지운다 (login과 동일).

## 비목표

- 기본 계정, 기본 비밀번호, MSI/configure가 사용자를 넣는 일
- delete, undelete, rename, role change, password reset, 재활성
- MFA, OIDC, email, LDAP
- CLI login/refresh/logout
- queued Hyper-V job
- service bearer 폐기
- current-evidence write, package-pair, Lane 2
- 29번째 feature, P0 evidence 후보 변경

## 검증

- Slice 1: Contracts 또는 Api 단위 테스트. host mutation 없음.
- Slice 2~3: focused `dotnet test` + catalog pin. dirty-tree는 four-shard PASS가 아니다.
- Slice 4: `npm run test:required --prefix web`.
- 설치본 login smoke는 Lane 2이며 이 캠페인이 열지 않는다.
