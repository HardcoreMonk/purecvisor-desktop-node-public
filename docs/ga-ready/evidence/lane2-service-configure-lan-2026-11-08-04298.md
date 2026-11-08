# Lane 2 S4 계정 복원과 LAN 재적용 `0.42.98-admin-smoke` (2026-11-08)

evidence_id: `lane2-service-configure-lan-2026-11-08-04298`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.98-admin-smoke`
installed_product_version: `0.42.98-admin-smoke+2dfe2ac7e1af85a51b676127f83d264a0b57d840`
installed_host_sha256: `501b41f7304fca9831f1fbde9b6b34edb81aeadf623eb1ff61e85b0c420d6cc0`
train_carriages: `PR #94` (`271ab040`), `PR #95` (`08c50e08`)
host_mutation_performed: `true`
lan_exposure: `private-profile-local-subnet`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

train `0.42.98-admin-smoke` 적재 PR #94(`configure-installed`의 LAN prefix 옵션과 `--allow-lan`)와 PR #95(실행 중 service 재시작)의 Lane 2
probe이고, 2026-11-08 승인 3의 S4 상태 복원이다. 기준은 `docs/superpowers/plans/2026-11-08-purecvisor-desktop-node-train-04298.md` Task 7이다.

train 전(train `0.42.97` Task 2)에 S4 계정 저장소와 service 명령줄을 저장소 밖 보호된 위치에 백업하고, 계정을 치운 뒤 loopback으로
돌렸다. 이 기록은 그 백업으로 계정을 되돌리고, 설치본 `0.42.98`의 `DesktopNode.Host.exe`로 LAN 설정을 다시 적용한 결과다. LAN 주소는
`<host-lan-ip>`로 적는다. 백업 위치, 비밀번호, token은 적지 않는다. 실행 스크립트는 세션 작업 폴더의 일회용
`task7-restore-lan-04298.ps1`이고, 출력의 사설 IPv4와 사용자 경로를 가린다.

## 실행

2026-11-08 17:41~17:43 KST.

1. 계정 복원: data root(`C:\ProgramData\PureCVisor\desktop-node`)에는 fullgate 재설치가 만든 빈 계정 저장소(계정 0개, 174 byte)가 있었다.
   이것을 백업 폴더로 옮기고, 백업한 `accounts.json`을 넣었다. SHA-256은 백업과 같다
   (`926caa7969e16afeb49930747f8608caaaa550e6c0392d772c995e0e5860bc8e`). 파일 ACL은 제품이 빈 저장소에 둔 것과 같게 맞췄다(상속 끊음,
   `NT AUTHORITY\SYSTEM`·`BUILTIN\Administrators` 읽기만).
2. LAN 재적용: 설치본 `DesktopNode.Host.exe service-action configure-installed`를 MSI custom action과 같은 `--product-root`,
   `--data-root`, `--service-exe`, 지금 명령줄의 `--batch-evidence-root`와 백업 명령줄의 `--prefix http://<host-lan-ip>:7777/`,
   `--web-prefix http://<host-lan-ip>:80/`, `--allow-lan`으로 실행했다. exit `0`. 실행 중 service가 멈췄다가 새 명령줄로 다시 켜졌다(PR #95).
3. 방화벽: `service-action firewall-enable`을 API 규칙(기본값)과 Web 규칙(`--firewall-local-port 80`,
   `--firewall-rule-name "PureCVisor Desktop Node Web LAN"`)으로 두 번 실행했다. 둘 다 exit `0`.
   앞선 두 시도는 상태를 바꾸지 않고 거절됐다. 첫 시도는 제품 경로 인자가 없어 `PCV_HOST_PRODUCT_ROOT_REQUIRED`(exit `2`)였고, 둘째 시도는
   `--allow-lan`이 없어 `PCV_HOST_FIREWALL_LAN_APPROVAL_REQUIRED`(exit `1`)였다. 방화벽 규칙도 LAN 노출의 명시 승인을 요구하는 설계대로다.

## 확인

| 항목 | 관측 | 판정 |
| --- | --- | --- |
| service | Running, Automatic | `PASS` |
| 명령줄 | `--prefix`, `--web-prefix`가 `<host-lan-ip>`, `--allow-lan` 있음, `127.0.0.1` prefix 없음. 백업한 S4 명령줄과 비교하면 `--batch-evidence-root`만 다르다(train `0.42.96` fullgate 경로 → train `0.42.98` fullgate 경로, MSI 재설치가 바꾼 값). web root 표기 `DesktopNode\.\web`과 `DesktopNode\web`은 같은 폴더로 보고 비교했다 | `PASS` |
| listener | `<host-lan-ip>:7777`, `<host-lan-ip>:80`(HTTP.sys, PID 4). `127.0.0.1`은 듣지 않는다(Web `http://127.0.0.1/` 연결 없음) | `PASS` |
| 방화벽 | PureCVisor 규칙 2개: `PureCVisor Desktop Node Local API LAN`(TCP 7777), `PureCVisor Desktop Node Web LAN`(TCP 80). 둘 다 enabled, Inbound, Allow, profile `Private`, remote `LocalSubnet` | `PASS` |
| loopback 세션 | LAN 주소로 `POST /api/v1/auth/loopback-session` → `403` `PCV_LOOPBACK_SESSION_NOT_LOOPBACK`, token 없음 | `PASS` |
| 계정 | 3개: `pcv-admin`(admin, disabled), `pcv-admin2`(admin, enabled), `pcv-s4-operator`(operator, enabled). PCVCLI `--api http://<host-lan-ip>:7777`, service bearer(보호된 token 파일) | `PASS` |
| Web | `http://<host-lan-ip>/` 200 | `PASS` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running. 그대로다 | `PASS` |

계획은 loopback 세션 거절을 `409`로 적었다. `409`는 loopback 주소로 들어온 요청이 계정이 있어 거절될 때의 코드다. 지금 service는 LAN 주소만
들으므로 이 호스트에서도 요청이 LAN 주소로 들어오고, 제품은 그보다 먼저 `403` `PCV_LOOPBACK_SESSION_NOT_LOOPBACK`으로 거절한다. 확인할 성질
(LAN 모드에서 loopback 세션 없음)은 같다.

## 끝 상태

- S4 LAN 상태로 돌아왔다. 이 호스트의 브라우저도 `http://<host-lan-ip>/`로 열고 계정으로 로그인한다.
- 빈 계정 저장소는 백업 폴더에 남겼다. 백업은 지우지 않았다(삭제는 사용자에게 묻는다, Task 14).
- 설치본 version은 `0.42.98-admin-smoke` 그대로다. operational current는 `0.42.96-admin-smoke` 그대로다.
