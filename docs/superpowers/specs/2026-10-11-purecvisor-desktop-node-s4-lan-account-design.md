# S4 LAN 계정 사용 시나리오 설계

- 상태: 제안(결정 대기). campaign `s4-lan-account-20261011` Task 1의 산출물이다.
- 기준: `config/project-completion-criteria.json` S4 "LAN의 다른 PC에서 계정으로 로그인해 S1~S3을 쓴다", ADR-0017(판정은 설치본
  시나리오 스크립트 1회 통과와 시연 기록, LAN 노출은 ADR-0006·ADR-0010 경계 안).
- 사전 승인: 2026-10-09 감사 개선안 3(설치본 `--allow-lan`과 방화벽 변경), 2026-10-10 승인 2(그 범위 안에서만), ADR-0016(`pcv-it-`
  VM 생성·구성·삭제).
- 이 문서는 설계다. 구현, host mutation, 시연은 아래 결정이 승인된 뒤 campaign task로 더한다.

## 1. 목표와 판정

S4는 설치본 `0.42.96-admin-smoke` 계열 제품을 이 호스트 밖의 브라우저에서 계정으로 쓰는 시나리오다. 판정은 다음을 모두 기록한
시연 문서 하나와 시나리오 스크립트 1회 통과다.

1. 다른 PC의 브라우저가 `http://<host-lan-ip>/`에서 로그인 페이지를 받고 계정으로 로그인한다. loopback 세션은 쓰지 않는다.
2. 그 세션으로 S1(ISO로 VM 생성, 브라우저 콘솔), S2(template 복제), S3(checkpoint 생성·복원·예약)를 한다. 각 단계는 기존
   시나리오와 같은 판정 값을 쓴다.
3. LAN 노출은 Private 방화벽 profile과 `LocalSubnet` 범위 안이고, 시연 뒤 끝 상태(유지 또는 loopback 복귀)를 기록한다.

## 2. 코드에서 확인한 사실 (2026-10-11, `main` `d28b74a`)

| 영역 | 사실 | S4 영향 |
| --- | --- | --- |
| listener | `DesktopNodeHostOptions`는 API·Web prefix가 loopback이 아니면 `--allow-lan`을 요구하고(`PCV_HOST_PREFIX_NOT_LOOPBACK`), LAN mode는 token source를 요구한다(`PCV_HOST_LAN_TOKEN_REQUIRED`). API prefix와 Web prefix는 각각 하나다 | LAN prefix로 바꾸면 그 주소로만 듣는다 |
| Web config | `/pcv-config.js`의 `apiBaseUrl`은 API listener prefix의 authority다(`DesktopNodeHostApplication.StaticAuth.cs`) | API prefix가 `<host-lan-ip>`면 원격 브라우저에 맞다. wildcard(`+`, `*`) prefix면 브라우저가 쓸 수 없는 값이 된다 |
| CSP | `connect-src 'self' http://127.0.0.1:* http://localhost:* http://*:7777` | LAN 주소의 7777 API 호출은 허용된다 |
| 방화벽 | `DesktopNode.Host.exe service-action firewall-enable|firewall-remove`가 관리 규칙 하나를 만든다. 기본 port `7777`, profile `Private`, remote `LocalSubnet` | Web Console port `80`에는 규칙이 없다. 원격 브라우저가 로그인 페이지를 받으려면 80도 열어야 한다 |
| 계정 | `pcvcli account list|create|disable`. 첫 계정은 admin, 삭제 명령은 없고 마지막 enabled admin은 disable할 수 없다. 계정이 하나라도 있으면 `POST /api/v1/auth/loopback-session`은 `409 PCV_LOOPBACK_SESSION_DISABLED`다 | 계정을 만들면 이 호스트의 로컬 콘솔도 로그인해야 한다. 되돌리려면 `%ProgramData%` 아래 `accounts.json`을 손으로 치워야 한다 |
| 기존 LAN 도구 | `Invoke-PcvOsMutationGateSmoke.ps1`은 별도 Host process를 `--allow-lan`과 LAN prefix로 띄워 확인하고 방화벽 규칙을 지운다(설치 service는 바꾸지 않는다) | 설치 service의 LAN 구성 경로는 시연된 적이 없다 |
| 시연 guest | S1 guest는 Ubuntu Server(브라우저 없음)였다 | "다른 PC" 브라우저는 따로 정해야 한다 |

## 3. 결정 사항

### D1. "다른 PC"를 무엇으로 할지

| 안 | 내용 | 장점 | 단점 |
| --- | --- | --- | --- |
| A | 사용자의 실제 PC(같은 LAN)에서 사용자가 시연하고, 에이전트는 순서표·확인 스크립트·기록을 준비한다 | 시나리오 문장 그대로다. 시연 기록의 확인자 칸을 사용자가 채운다(감사 §10의 자가 보고 문제 해소) | 사용자 시간이 든다 |
| B | `pcv-it-` guest VM(데스크톱 OS와 브라우저)을 Default Switch에 두고 그 브라우저로 접속한다 | 에이전트가 끝까지 자동으로 한다 | guest는 NAT 뒤에 있어 "LAN의 다른 PC"가 아니다. 방화벽 범위를 Default Switch 대역으로 넓혀야 하고, 브라우저 있는 guest 이미지를 새로 만들어야 한다 |
| C | External switch를 물리 NIC에 묶고 guest를 LAN에 직접 붙인다 | guest가 실제 LAN 주소를 받는다 | 호스트 네트워크 구성을 바꾼다. 사전 승인 범위(`--allow-lan`, 방화벽) 밖이고 호스트 연결이 끊길 수 있다 |

권장: A. 그 전에 에이전트가 이 호스트의 브라우저로 `http://<host-lan-ip>/`에 접속하는 리허설(R)을 돌린다. R은 loopback이 아닌
주소에서의 로그인, `apiBaseUrl`, CSP, RBAC 경로를 확인하지만 "다른 PC"가 아니므로 S4 판정 근거로 쓰지 않는다.

### D2. listener를 어떻게 열지

| 안 | 내용 | 제품 변경 |
| --- | --- | --- |
| A | 설치 service를 API `http://<host-lan-ip>:7777/`, Web `http://<host-lan-ip>:80/`로 재구성한다 | 없음. 그동안 `http://127.0.0.1/`은 듣지 않는다 |
| B | `/pcv-config.js`가 요청 Host header를 기준으로 `apiBaseUrl`을 만들고, listener가 loopback과 LAN 주소를 함께 듣게 한다 | Host 변경, 시험, dev probe, 다음 release train |

권장: 시연은 A로 한다(제품 변경 없음, 사전 승인 범위). B는 LAN을 상시로 쓸 때의 개선이며 S4 판정에 필요하지 않다. 사용자가 LAN 상시
사용을 원하면 B를 Lane 1 campaign으로 따로 승인받는다.

### D3. 방화벽과 끝 상태

- 관리 규칙 두 개: API `7777`, Web `80`, profile `Private`, remote `LocalSubnet`. 지금 도구는 규칙 이름이 하나로 고정돼 있어 두 번째
  규칙에는 `--firewall-rule-name`과 `--firewall-local-port`를 쓴다(이름은 `PureCVisor Desktop Node Web LAN`).
- 끝 상태 권장: 시연 뒤 loopback 복귀(service 재구성, 규칙 두 개 제거, PureCVisor 규칙 0)다. LAN을 계속 쓸지는 사용자가 정한다.

### D4. 계정

- 계정을 만들면 loopback 세션이 닫히고 삭제 명령이 없다(2절). 그래서 계정 생성은 되돌리기 어려운 변경이다.
- 권장: 사용자가 자기 PC에서 쓸 admin 계정을 직접 만든다(`pcvcli account create --username <name> --role admin --password-stdin --yes`).
  비밀번호는 사용자만 안다. 에이전트 리허설(R)은 같은 시점에 에이전트용 operator 계정을 실행 경계에서 만든 비밀번호로 만들고, 리허설 뒤
  `pcvcli account disable`로 끈다. 비밀번호는 기록하지 않는다.
- loopback 세션 복귀가 필요하면 별도 승인으로 `accounts.json`을 백업 뒤 치우는 절차를 쓴다(이 설계는 하지 않는다).

### D5. 시나리오 스크립트

- 새 `web/scripts/run-s4-lan-scenario.mjs`: base URL(`http://<host-lan-ip>`)과 계정 이름을 받고 비밀번호는 환경 변수에서 읽는다.
  `POST /api/v1/auth/login` 뒤 S1~S3 시나리오 스크립트의 API 단계를 같은 JWT로 돈다(VM 이름 `pcv-it-s4-*`, disk는 artifacts 아래).
  인자는 `--name=value`와 공백 형식을 모두 받는다(BL-0022의 교훈).
- 사용자 시연(A)은 같은 순서를 브라우저로 하고, 에이전트는 시연 전후 상태(service 구성, 규칙, 계정 목록, VM)를 읽어 기록한다.

## 4. 승인 뒤 task 순서 (권장안 기준)

1. Lane 1: `run-s4-lan-scenario.mjs`와 시험, 시연 순서표 문서. push, PR, green CI 뒤 merge.
2. Lane 2(사전 승인 범위): 설치 service를 LAN prefix로 재구성, 방화벽 규칙 두 개, 에이전트 operator 계정 생성, 이 호스트 브라우저 리허설 R과
   스크립트 1회. 기록.
3. 사용자 시연 A: 사용자가 admin 계정을 만들고 자기 PC 브라우저로 S1~S3. 에이전트는 전후 상태를 기록하고 확인자 칸은 사용자가 채운다.
4. Lane 2: 끝 상태(권장 loopback 복귀, 규칙 0), 에이전트 계정 disable, `pcv-it-s4-*` VM 정리. 기록.
5. Lane 1: criteria S4 `passed`, 시연 기록 링크. push, PR, merge. 그 뒤 release train 1회(사전 승인 문장의 "이어서 release train 1회"),
   적재할 제품 변경이 없으면 train은 열지 않고 그 사실을 적는다.

## 5. 경계와 nonclaims

- Linux Single Edge runtime, WebSocket event, public 443, public trusted signing, external stable publication은 범위 밖이다.
- LAN IP, 호스트명, 계정 비밀번호, token 값은 public 문서와 캡처에 남기지 않는다. LAN 주소는 `<host-lan-ip>`로 쓴다.
- 이 설계 문서는 S4 통과를 주장하지 않는다.
