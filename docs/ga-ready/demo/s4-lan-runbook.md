# S4 LAN 시연 순서표

이 순서표는 설계 4절 task 2~4를 사람이 따라 하는 절차다. 이 문서를 추가한 변경은 아래 명령을 실행하지 않는다.
주소는 `<host-lan-ip>`만 쓴다. 비밀번호, token, 실제 LAN 주소, 호스트명, 사용자 홈 경로는 적지 않는다.
`<token-file>`은 설치 service가 쓰는 protected token 파일의 자리 표시다. 실제 경로와 값은 적지 않는다.
`pcvcli`의 `--api`와 `--protected-token-file`은 명령어 앞에 둔다.
리허설 R은 S4 판정 근거가 아니다. 판정 근거는 사용자 시연 A다.

스크립트는 `npm run scenario:s4-lan --prefix web`이다. 비밀번호는 환경 변수 `PCV_S4_PASSWORD`에만 둔다.
`--password` 인자는 없다. `--execute`가 없을 때는 요청을 보내지 않는다.

## 1. 리허설 전 상태 읽기

설치본을 바꾸기 전에 아래를 읽어 적는다. 값은 자리 표시로만 남긴다. 이 절은 loopback 상태다.

- service `PureCVisorDesktopNode`의 상태와 실행 인자. Web prefix와 API prefix가 loopback인지 본다.
- 방화벽 관리 규칙. 기대 시작점은 PureCVisor 규칙이 없거나 loopback 운영의 기존 규칙뿐이다.
- 계정 목록. 리허설 operator 계정이 아직 없다. 계정이 0개일 수 있다.

  `pcvcli --protected-token-file <token-file> account list`

- VM 목록. `pcv-it-s4-console`, `pcv-it-s4-source`, `pcv-it-s4-clone`, `pcv-it-s4-checkpoint`가 없다.
  보존 VM `pcv-guest-installed-04253-r1`과 template `pcv-it-s2-source`는 이름을 확인만 하고 건드리지 않는다.

## 사전. 사용자 admin 계정 (loopback, 2절 앞)

LAN으로 바꾸기 전에 사용자가 admin 계정을 만든다. 계정이 0개일 때 admin이 아닌 역할은 `409 PCV_ACCOUNT_BOOTSTRAP_ADMIN_REQUIRED`로 거절된다. 이 명령은 loopback 상태의 기본 주소를 쓴다.

`pcvcli --protected-token-file <token-file> account create --username <admin> --role admin --password-stdin --yes`

비밀번호는 사용자만 안다. admin이 생기면 loopback 세션이 닫히므로 이 호스트의 로컬 콘솔도 그 계정으로 로그인해야 한다. 에이전트 operator는 이 계정 뒤에, 2절에서 만든다. admin 계정이 없으면 2절은 멈추고 사용자에게 이 단계를 요청한다.

## 2. 리허설 R

Lane 2 작업이 설치 service를 재구성한다. 제품 코드는 바꾸지 않는다. 시작 조건은 사전 단계의 admin 계정이다.

1. `DesktopNode.Host.exe service-action configure-installed`로 API prefix `http://<host-lan-ip>:7777/`, Web prefix `http://<host-lan-ip>:80/`, `--allow-lan`, token source를 넣는다. 그 동안 `http://127.0.0.1/`은 듣지 않는다.
2. 방화벽 관리 규칙 두 개를 연다. 둘 다 profile `Private`, remote `LocalSubnet`이다.
   API는 local port `7777`이다. Web은 `--firewall-local-port 80`과 `--firewall-rule-name "PureCVisor Desktop Node Web LAN"`이다.
3. 에이전트가 operator를 실행 경계 비밀번호로 만든다. service는 LAN 주소만 듣는다. 비밀번호는 `PCV_S4_PASSWORD`로만 넘긴다.

   `pcvcli --api http://<host-lan-ip>:7777 --protected-token-file <token-file> account create --username <operator> --role operator --password-env PCV_S4_PASSWORD --yes`

4. 이 호스트의 브라우저로 `http://<host-lan-ip>/`를 연다. 로그인 페이지가 나오고, loopback 세션이 아닌지 본다.
5. 스크립트로 preflight만 돈다. VM을 만들지 않는다.

   `node web/scripts/run-s4-lan-scenario.mjs --web http://<host-lan-ip> --user <operator> --phases=none --execute`

   기대: exit 0, `result=pass`, summary의 `host_kind`는 `lan`, `web`과 `api`의 host는 `<host-lan-ip>`이다.
6. preflight가 통과하면 같은 계정으로 S1~S3을 한 번 돈다. ISO 경로는 저장소 `artifacts` 아래를 쓴다. `--skip-auto`를 붙인다. 자동 checkpoint의 최소 간격은 60분이고 Lane 2 한도는 45분이므로, R은 자동 checkpoint를 증명하지 않는다.

   `node web/scripts/run-s4-lan-scenario.mjs --web http://<host-lan-ip> --user <operator> --iso <artifacts-iso> --skip-auto --execute`

   기대: `result=pass-without-auto`. `cleanup_error`는 없다. `vms_remaining`은 비어 있다. 이 결과는 리허설 기록이지 S4 통과가 아니다.

## 3. 사용자 시연 A

사용자는 자기 PC의 브라우저만 쓴다. 로그인은 사전 단계에서 만든 admin 계정이다. 에이전트는 시연 전후 상태를 읽고, 확인자 칸은 사용자가 채운다.

| 순서 | 사용자 브라우저 | 확인할 것 | 확인자 |
| --- | --- | --- | --- |
| 1 | `http://<host-lan-ip>/`를 연다 | 로그인 페이지. loopback 세션을 쓰지 않는다 |  |
| 2 | 사전 단계의 admin 계정으로 로그인한다 | 역할이 화면에 맞다 |  |
| 3 | S1. ISO로 Generation 2 VM을 만들고 콘솔을 연다 | 화면이 비어 있지 않고, 입력 뒤 화면이 바뀐다 |  |
| 4 | S2. template을 잠그고 복제한다 | 복제 job이 한도 안에 끝난다 |  |
| 5 | S3. checkpoint를 만들고, 복원하고, 예약을 미리본 뒤 저장한다 | 예약 checkpoint가 생기고, 끝나면 예약을 지운다 |  |
| 6 | 시연 VM을 끈다 | `pcv-it-s4-*`만 대상이다 |  |

예약 저장 뒤 `next_due_at`이 지나야 예약 checkpoint가 생긴다. 사용자는 그동안 자리를 비워도 되고, 확인은 그 뒤에 한다.

시연 전후에 에이전트가 읽는 항목은 1절과 같다. service prefix, 방화벽 규칙 수, 계정 목록, VM 목록을 기록한다.

## 4. 끝 상태 복귀 확인

시연이 끝나면 Lane 2 작업이 아래를 되돌리고, 각각을 다시 읽어 확인한다. operator disable은 loopback 복귀 다음이다. 복귀 전에는 service가 LAN만 들으므로 disable에 `--api http://<host-lan-ip>:7777`이 필요하다. 이 순서표는 복귀를 먼저 한다.

- service prefix를 loopback으로 되돌린다. Web `http://127.0.0.1/`, API `http://127.0.0.1:7777/`. `--allow-lan`은 빠진다.
- 방화벽 관리 규칙 두 개를 제거한다. PureCVisor 규칙은 0이다.
- 복귀 뒤에 리허설 operator를 끈다. 기본 주소 `http://127.0.0.1:7777`과 service bearer를 쓴다. admin 계정 파일을 지우지는 않는다.

  `pcvcli --protected-token-file <token-file> account disable <operator> --yes`

- `pcv-it-s4-*` VM이 남아 있으면 지운다. 보존 VM과 `pcv-it-s2-source`는 그대로다.
- `http://127.0.0.1/`가 다시 200이다. `http://<host-lan-ip>/`는 더 이상 이 서비스가 아니어야 한다.

이 순서표는 S4 통과를 주장하지 않는다.
