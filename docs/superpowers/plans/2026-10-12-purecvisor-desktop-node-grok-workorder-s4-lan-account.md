# Grok 작업 지시서: S4 LAN 계정 시나리오 마무리 (2026-10-12)

- 작성: Claude (조사·설계·감사). 수행: Grok (서비스 개발).
- 기준: `main` `1043c8d`(PR #91 merge 뒤) + 아래 2절의 커밋되지 않은 Task 3 보정.
- campaign: `docs/ga-ready/active-campaign.json` id `s4-lan-account-20261011`, `next_task` Task 4.
- plan: `docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-s4-lan-account.md`
- 설계: `docs/superpowers/specs/2026-10-11-purecvisor-desktop-node-s4-lan-account-design.md`
- 시연 순서표: `docs/ga-ready/demo/s4-lan-runbook.md`
- 개정(2026-10-12, 2차, 승인 `1,2`): 단계 A(PR #92 merge)와 단계 B(사용자 admin 계정)는 끝났다. D2 정정으로 단계 B2(plan Task 8,
  `configure-installed` LAN prefix 옵션)를 단계 C 앞에 더하고, 단계 C·E의 재구성 명령과 단계 F의 release train 문장을 바꿨다.
  **다음은 단계 B2다.**

이 지시서는 campaign plan을 대체하지 않는다. 내용이 충돌하면 `AGENTS.md`, `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`,
campaign plan, 이 지시서 순서로 따른다.

## 0. 끝나면 시연 가능해지는 것

사용자가 자기 PC 브라우저로 `http://<host-lan-ip>/`에 접속해 계정으로 로그인하고 S1(ISO로 VM 생성과 브라우저 콘솔),
S2(template 복제), S3(checkpoint 생성·복원·예약)를 한다. 그 시연 기록으로 완료 기준 S4를 `passed`로 닫는다.
제품 변경은 단계 B2 하나(`configure-installed` LAN prefix 옵션)다. 설치 service 구성, 방화벽, 계정은 시연 뒤 되돌린다.

## 1. 역할과 경계

- Grok은 구현, 실행, 검증, commit, PR을 맡는다. 설계 판단이 필요한 발견은 고치지 말고 report-only로 남겨 Claude에게 넘긴다.
  campaign `stop_on`의 `new-design-required`다.
- task 진행은 `.claude/skills/pcv-campaign/SKILL.md`, push부터 merge 뒤 `main` 확인까지는 `.claude/skills/pcv-ship/SKILL.md`
  절차를 따른다.
- 승인 범위는 campaign `approval_locator` 그대로다.
  - Lane 1: 로컬 commit, push, PR, green CI 뒤 merge.
  - Lane 2: 설치본 `--allow-lan` 재구성, 방화벽 관리 규칙 두 개, 계정 생성·disable, ADR-0016 `pcv-it-s4-*` VM 생성·삭제.
  - 2026-10-12 2차: plan Task 8 Lane 1 제품 변경(Host, Host.Tests, 관련 문서, release-train queue 행)과, Task 4·6에서 merge된
    `main` 빌드본 `DesktopNode.Host.exe`로 설치 service를 재구성·복귀하는 것. service 실행 파일은 설치본 그대로 둔다.
  - 그 밖은 하지 않는다: MSI install/repair/uninstall, service install/delete, release train, `pcv-it-s4-*` 밖의 VM 변경,
    보존 VM `pcv-guest-installed-04253-r1`과 template `pcv-it-s2-source` 변경, `current-evidence.json` 수정.
- admin 계정은 사용자만 만든다. Grok은 admin 계정을 만들지 않고 그 비밀번호를 받지도 않는다.
- 공개 저장소다. 문서, 커밋, PR, summary에 LAN 사설 IP, 호스트명, 사용자 홈 경로, token, 비밀번호를 적지 않는다.
  주소는 `<host-lan-ip>`, token 파일은 `<token-file>`로 쓴다. 리허설 비밀번호는 환경 변수 `PCV_S4_PASSWORD`로만 넘긴다.
- 저장소 밖 파일(사용자 프로필의 plugin hook 등)은 고치지 않는다. Grok 시작 때의 `session_start hook ... failed, ignored`
  오류는 실패해도 세션이 계속되는(fail-open) 오류이므로 이 작업의 blocker가 아니다. 환경 조치는 사용자가 한다.
- 셸이 Git Bash면 `MSYS_NO_PATHCONV=1`이 git `file://` clone 시험(`CutoverGitBoundaryTests.RejectsShallowHistory`)을 거짓 실패시킨다.
  `dotnet test`는 PowerShell에서 돌리거나 `env -u MSYS_NO_PATHCONV`를 붙인다.

## 2. 1차 시작 상태 (Claude 감사, 2026-10-12. 단계 A·B는 끝났다)

- Task 1(PR #90)과 Task 3(PR #91)은 merge됐다. Task 2(C5 runner 확인)는 `not_before` 2026-10-19다.
- 작업 트리에 Task 3 보정이 커밋되지 않고 남아 있다. 파일 5개다.
  - `web/scripts/run-s4-lan-scenario.mjs`: `createVm`이 `job_id`를 받은 직후 `createdVms`에 넣는다. cleanup이 남은 console VM을
    delete 전에 poweroff하고, 실패하면 `cleanup_error`에 남긴다. `--skip-auto`는 s3을 고른 경우에만 `pass-without-auto`다.
  - `web/node-tests/s4-lan-scenario.test.mjs`: T10~T12를 더했다. Claude가 `node --test web/node-tests/s4-lan-scenario.test.mjs`를
    돌려 12/12를 확인했고 `git diff --check`도 통과했다.
  - `docs/ga-ready/demo/s4-lan-runbook.md`: 사용자가 loopback에서 admin을 먼저 만들고(계정 0개면 admin이 아닌 역할은
    `409 PCV_ACCOUNT_BOOTSTRAP_ADMIN_REQUIRED`), LAN 재구성 뒤에 에이전트가 operator를 만드는 순서로 바꿨다.
  - plan Task 3 보정 문단, Task 4·5 문장과 `active-campaign.json` `next_step`.
- 빠진 것: plan 보정 문단은 "검증 수치는 이 문단 아래 보정 검증에 적는다"로 끝나지만 그 문단이 없다.
  `npm run test:required`, public source safety, Delivery·Verification 시험 결과도 아직 없다.
- campaign `stop_on`에 `dirty-tree-at-start`가 있다. 그래서 보정을 먼저 commit해야 Task 4를 시작할 수 있다.

## 3. 설계 해석: 시연 A의 S3 판정 (Claude 결정)

설계 1절 2항은 각 단계가 "기존 시나리오와 같은 판정 값"을 쓴다고 정했다. 기존 S3 통과
(`docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md`)는 service worker가 예약 checkpoint를 실제로 만든 것까지
확인했다. 그래서 다음과 같이 판정한다.

- 사용자 시연 A(Task 5)의 S3는 예약 실행까지 확인한다. 예약 저장 뒤 `next_due_at`이 지나 `pcv-schedule-*` checkpoint가
  생긴 것을 읽어 기록하고, 그 다음에 예약을 지운다. 예약 간격의 최솟값은 60분이므로 시연 A는 두 번에 나눠 진행한다(6절 단계 D).
- 리허설 R(Task 4)은 지금처럼 `--skip-auto`, 기대값 `result=pass-without-auto`다. R은 S4 판정 근거가 아니다.
- 런북 3절 표의 S3 행("예약 checkpoint가 생기고, 끝나면 예약을 지운다")은 그대로 둔다. 표 아래에 "예약 저장 뒤 `next_due_at`이
  지나야 예약 checkpoint가 생긴다. 사용자는 그동안 자리를 비워도 되고, 확인은 그 뒤에 한다"는 문장 하나만 더한다.

사용자가 다르게 정하면 그 결정을 따른다.

## 4. 멈춤 조건

campaign `stop_on`(`same-cause-fail-3`, `budget`, `checkpoint-limit`, `verification-red`, `approval-required-mutation`,
`new-design-required`, `permission-denied`, `dirty-tree-at-start`)과 회로 차단기 규칙을 따른다. 그 밖에 다음 경우에도 멈춘다.

- 사용자 작업(admin 계정, 시연)이 필요한 지점에 왔다.
- 리허설이나 시연에서 제품 결함이 나왔다. 설치본에서 고치지 말고 재현 정보와 함께 보고한다. 제품 변경은 새 설계가 필요하다.
- LAN 재구성 뒤 `http://127.0.0.1/`이 듣지 않는 것은 정상이다. 오류로 보고 되돌리지 않는다.

멈추면 `git status`와 diff를 확인하고 green checkpoint만 남긴다. 그 뒤 보고한다.

## 5. 보고 형식 (task마다)

```text
checkpoint k/N · task · lane · 경과 시간/예산 · tool batch 수
완료 / 미완료 / blocker / host mutation 여부(무엇을 바꿨고 끝 상태가 무엇인지)
검증: 명령별 결과 수치
commit / PR / CI run id / merge 여부
다음 단계와 사용자에게 필요한 일
```

Task 7 전에는 S4 통과를 주장하지 않는다.

## 6. 작업 순서

### 단계 A. Task 3 보정 닫기 (Lane 1, 30분·tool batch 18회)

1. 2절의 보정 변경을 읽고, 3절의 런북 문장 하나를 더한다.
2. 검증한다.
   - `node web/scripts/run-s4-lan-scenario.mjs` (plan-only, exit 0)
   - `node --test web/node-tests/s4-lan-scenario.test.mjs` (12/12)
   - `npm run test:required --prefix web` (exit 0. `test:web-contracts` 개수가 바뀌어 pin 시험이 실패하면 pin을 맞춘다)
   - `npm run test:public-source-safety --prefix web` (기대 19/20. 실패 1건은 기존 두 문서의
     `network.observed-private-endpoint`이고 BL에 이미 있다. 새 발견이 있으면 고친다)
   - `git diff --check`
3. plan Task 3 보정 문단 바로 아래에 "보정 검증(2026-10-12)" 문단을 쓰고 위 수치를 적는다.
4. 이 지시서 파일도 같은 commit에 넣는다. commit 제목 예:
   `fix: s4-lan-account-20261011 Task 3 correction (admin-first runbook, console cleanup)`
5. commit 뒤 clean HEAD에서 `dotnet test src/DesktopNode.Delivery.Tests -c Release`와
   `dotnet test src/DesktopNode.Verification.Tests -c Release`를 돌린다. PolicyBoundary 시험은 dirty tree에서 실패하므로
   반드시 commit 뒤에 돌린다.
6. `pcv-ship`: branch push, PR, Required CI green 뒤 merge, 로컬 `main` 동기화, merge 뒤 `main` run green 확인.

완료 조건: 보정이 `main`에 merge되고 작업 트리가 깨끗하다. host mutation은 없다.

### 단계 B. 사용자 admin 계정 요청 (사용자 작업)

Task 4의 시작 조건이다. Grok은 여기서 멈추고 사용자에게 다음을 요청한다. 설치본은 아직 loopback 상태다.

```powershell
pcvcli --protected-token-file <token-file> account create --username <admin> --role admin --password-stdin --yes
```

- 비밀번호는 사용자만 안다. admin이 생기면 loopback 세션이 닫히므로 이 호스트의 로컬 콘솔도 그 계정으로 로그인해야 한다.
- 사용자가 끝났다고 하면 Grok은 `pcvcli --protected-token-file <token-file> account list`로 admin 역할 계정이 있는지만 확인한다.
  계정 이름은 기록에 `<admin>`으로 남긴다.

### 단계 B2. Task 8 `configure-installed` LAN prefix 옵션 (Lane 1, 30분·tool batch 18회)

명세의 단일 출처는 plan Task 8이다. 요약하면 다음과 같다.

1. `lane1/` branch를 만든다. `configure-installed|repair-installed`가 `--prefix`, `--web-prefix`를 받게 한다. 옵션이 없으면 service
   명령줄이 지금과 바이트 단위로 같아야 한다.
2. loopback이 아니면 `--allow-lan`을 요구하고 명령줄에 넣는다. wildcard host, http가 아닌 scheme, API·Web host 불일치,
   LAN에서 API port가 7777이 아닌 값을 거절한다.
3. 명령 경로(`DesktopNodeHostServiceAction.cs`)와 native 구성(`DesktopNodeHostServiceAction.ServiceConfig.cs`)에 같은 값을 쓴다.
   native 구성이 credential target과 batch evidence root를 유지하는 동작은 바꾸지 않는다.
4. `DesktopNode.Host.Tests`에 plan Task 8의 시험을 더한다. README와, 인수를 고정하는 catalog·문서·시험이 있으면 맞춘다.
5. `docs/ga-ready/release-train.json` `queue` 행을 더한다(PR #86 형식).
6. 검증: `dotnet test src/DesktopNode.Host.Tests -c Release`, commit 뒤 clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`,
   `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`, `git diff --check`.
   plan Task 8에 실행 기록을 적는다.
7. `pcv-ship`: push, PR, green CI 뒤 merge, merge 뒤 `main` run 확인. host mutation은 없다.

완료 조건: Task 8이 `main`에 merge됐다. 설계 판단이 필요한 발견(예: 옵션 이름이나 검증 규칙이 기존 계약과 충돌)은 고치지 말고 보고한다.

### 단계 C. Task 4 리허설 R (Lane 2, 45분·tool batch 12회)

런북 1절과 2절을 그대로 따른다. 요약하면 다음과 같다.

1. 바꾸기 전 상태를 읽는다: service 명령줄 전체(경로는 자리 표시로), service prefix, 방화벽 관리 규칙, 계정 목록, `pcv-it-s4-*` VM 없음, 보존 VM 이름.
2. Task 8이 merge된 `main`에서 `dotnet build src/DesktopNode.Host -c Release`로 만든 `<main-build>\DesktopNode.Host.exe`로 재구성한다.
   `service-action configure-installed --product-root <product-root> --data-root <data-root> --service-exe <product-root>\DesktopNode.Host.exe --prefix http://<host-lan-ip>:7777/ --web-prefix http://<host-lan-ip>:80/ --allow-lan`
   재구성 뒤 service 명령줄을 1번 기록과 비교한다. `--prefix`, `--web-prefix`, `--allow-lan`만 달라야 한다. 실행 파일 경로,
   credential target, batch evidence root가 바뀌었으면 멈추고 단계 E 1번으로 되돌린다.
3. 방화벽 관리 규칙 두 개를 연다. 둘 다 profile `Private`, remote `LocalSubnet`이다. API는 local port 7777,
   Web은 local port 80과 규칙 이름 `PureCVisor Desktop Node Web LAN`이다.
4. operator 계정을 만든다. 비밀번호는 `PCV_S4_PASSWORD`로만 넘긴다.
   `pcvcli --api http://<host-lan-ip>:7777 --protected-token-file <token-file> account create --username <operator> --role operator --password-env PCV_S4_PASSWORD --yes`
5. 이 호스트 브라우저로 `http://<host-lan-ip>/`를 열어 로그인 페이지가 나오는지 본다.
6. preflight: `node web/scripts/run-s4-lan-scenario.mjs --web http://<host-lan-ip> --user <operator> --phases=none --execute`
   기대값은 exit 0, `result=pass`, `host_kind=lan`이다.
7. 전체 1회: `node web/scripts/run-s4-lan-scenario.mjs --web http://<host-lan-ip> --user <operator> --iso <artifacts-iso> --skip-auto --execute`
   기대값은 `result=pass-without-auto`이고, `cleanup_error`가 없고, `vms_remaining`이 비어 있다. ISO는 저장소 `artifacts` 아래 것을 쓴다.
8. 끝 상태(LAN 구성, 규칙 두 개, operator 활성)는 Task 6까지 유지한다. 결과는 plan Task 4 실행 기록에 적는다.
   summary는 저장소 `artifacts` 아래에 두고 커밋하지 않는다.

리허설에서 실패하면 원인을 보고하고 멈춘다. 같은 원인으로 세 번 실패하면 Task 6 순서로 되돌린 뒤 보고한다.

### 단계 D. Task 5 사용자 시연 A (사용자 작업 + Grok 기록)

시연 기록은 새 파일 `docs/ga-ready/demo/s4-lan-account-demo-<yyyy-mm-dd>-04296.md`에 쓴다. 기존 시연 기록과 같은 구조로
시나리오, 기준, 상태, 단계별 결과, 끝 상태, 판정, 확인자 칸을 둔다. 세 번에 나눠 진행한다.

1. D-1 (Grok): 시연 전 상태를 읽어 기록한다. 항목은 런북 1절과 같다.
2. D-2 (사용자): 자기 PC 브라우저로 런북 3절 순서 1~5를 한다. S3에서는 예약을 저장한 다음 거기서 멈춘다.
   Grok은 저장 직후 예약 readback(`next_due_at`)과 checkpoint 목록을 기록한다.
3. D-3 (`next_due_at`이 지난 뒤 사용자가 "계속"이라고 하면): Grok이 `pcv-schedule-*` checkpoint가 생겼는지 읽어 기록한다.
   사용자는 브라우저에서 예약을 지우고 시연 VM을 끈다(런북 3절 순서 6). 끝으로 Grok이 시연 뒤 상태를 기록한다.

확인자 칸은 사용자가 채운다. 리허설 R의 결과는 이 문서의 판정 근거로 쓰지 않는다.

### 단계 E. Task 6 끝 상태 (Lane 2, 45분·tool batch 12회)

런북 4절 순서를 따른다. 순서가 중요하다.

1. 같은 `<main-build>` `configure-installed`를 `--prefix`, `--web-prefix`, `--allow-lan` 없이 실행해 loopback으로 되돌린다.
   Web `http://127.0.0.1/`, API `http://127.0.0.1:7777/`이다. service 명령줄이 단계 C 1번 기록과 같은지 확인한다.
2. 방화벽 관리 규칙 두 개를 지운다. PureCVisor 규칙 수가 0인지 확인한다.
3. loopback으로 돌아온 뒤 operator를 끈다:
   `pcvcli --protected-token-file <token-file> account disable <operator> --yes`
   계정 파일은 지우지 않는다.
4. 남은 `pcv-it-s4-*` VM을 지운다. 보존 VM과 `pcv-it-s2-source`는 그대로 둔다.
5. 다시 읽어 확인한다: `http://127.0.0.1/` HTTP 200, `http://<host-lan-ip>/`에서 이 서비스가 응답하지 않음, 규칙 0개,
   operator disabled, `pcv-it-s4-*` 0개. 시연 기록의 끝 상태 절과 plan Task 6에 적는다.

### 단계 F. Task 7 S4 판정 (Lane 1, 30분·tool batch 18회)

1. 시연 기록이 설계 1절 판정 세 항목(다른 PC 로그인과 loopback 세션 미사용, S1~S3 같은 판정 값, LAN 노출 범위와 끝 상태)을
   모두 담았는지 확인한다. 하나라도 빠지면 판정하지 않고 보고한다.
2. `config/project-completion-criteria.json` S4를 `status=passed`, `demo_record`를 시연 기록 경로로 바꾼다.
3. S3를 닫을 때처럼 `docs/ADR_INDEX.md`, `docs/DOCUMENTATION_INDEX.md`, `docs/DEVELOPER_INDEX.md`의 S4 상태 문장을 맞춘다.
4. 완료 판정:
   `dotnet run --project src/DesktopNode.Verification -c Release -- completion --ci-runs artifacts/completion/<yyyymmdd>/main-runs.json --output artifacts/completion/<yyyymmdd>/result.json`
   S4가 `met`인지 본다. C5는 Task 2 전이므로 `complete=true`가 아니어도 된다.
5. `dotnet test src/DesktopNode.Verification.Tests -c Release`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`(clean HEAD),
   `git diff --check`.
6. Task 8 제품 변경이 queue에 있으므로 S4 판정 뒤 release train 1회(2026-10-09 사전 승인)를 다음 campaign으로 연다.
   이 campaign에서는 train을 실행하지 않는다. plan Task 7 기록에 그 사실을 적는다.
7. `active-campaign.json`의 `next_task`를 Task 2로 둔다(`not_before` 2026-10-19).
8. `pcv-ship`으로 PR, green CI 뒤 merge, merge 뒤 `main` run 확인까지 한다.

### 그 뒤

- Task 2(C5 runner 확인)는 2026-10-19 이후에 한다. 내용은 plan Task 2와 같다.
- campaign이 닫히면 다음은 legacy-retirement campaign(2026-10-10 승인 3)이다. 그 campaign의 설계는 Claude가 먼저 쓴다.
  Grok은 승인된 plan이 생긴 뒤에 시작한다.

## 7. Nonclaims

- 이 지시서는 새 승인을 만들지 않는다. 범위는 campaign `approval_locator`까지다.
- operational current는 `0.42.96-admin-smoke` 그대로다. public trusted signing과 external stable publication을 주장하지 않는다.
- Task 7 판정 전에는 S4 통과와 completion `complete=true`를 주장하지 않는다.
