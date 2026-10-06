# Lane 2 noVNC target 설정 확인과 계정 probe 결정 `0.42.91-admin-smoke` (2026-10-06)

evidence_id: `lane2-completion-novnc-target-2026-10-06-04291`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
family: `console.novnc-target` (SERVICE_PLAN P2-11). P1-9 admin account는 결정 기록
design: `docs/superpowers/specs/2026-09-21-purecvisor-desktop-node-p2-novnc-target-config-design.md`, ADR-0010
version: `0.42.91-admin-smoke`
installed_product_version: `0.42.91-admin-smoke+990a4b2713f6d51dca416b476308a0bf92296155`
release_train: `0.42.91-admin-smoke`
artifact_root: `artifacts/lane2-completion-novnc-target-20261006-04291`
probe_summary_sha256: `990f27ca7e75d50c8f28171c31ec72f9ab77e8cffb87348ce7e0303f23921c23`
vm_created: `false`
host_mutation_performed: `true` (noVNC target 파일 설정 뒤 원복)
secret_observed: `false`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## P2-11 noVNC target

`2026-10-06T11:05:08Z`~`11:05:13Z`. 시작 상태는 `%ProgramData%\PureCVisor\desktop-node\novnc-target.json` 없음이고, 서비스 PathName에 noVNC 인자가 없다.

| 단계 | 기대 | 관측 |
| --- | --- | --- |
| `console novnc-target preview --host 127.0.0.1 --port 5901` | dry-run ok, 파일 안 생김 | ok, 파일 없음 |
| `set --host 127.0.0.1 --port 5901 --yes` | queued job 성공, 파일에 loopback target | `succeeded`, `127.0.0.1:5901` enabled |
| `clear --yes` | queued job 성공, 파일에 `enabled=false` | `succeeded`, `enabled=false` |
| 원복 | 시작 상태(파일 없음) | 파일을 지워 같은 상태 |
| service | Running/Automatic, Web `200` | 그대로 |

LAN target은 쓰지 않았다.

## P1-9 admin account

probe는 계정 목록부터 읽었다(`artifacts/lane2-completion-account-novnc-target-20261006-04291/01-account-list.json`). 결과는 `accounts: []`,
`bootstrap_state: no-default-account`다. 첫 계정은 admin이 되고 마지막 enabled admin은 비활성화가 거절되므로, probe 계정을 만들면 지울 수 없는
활성 admin이 남고 이 호스트의 인증 상태가 바뀐다. 안전장치가 계정을 만들기 전에 멈췄다.

사용자 결정(2026-10-06): P1-9는 설치본 evidence 없이 닫는다. 계정 없는 bootstrap이 의도된 기본값이고 probe가 인증 상태를 바꾸기 때문이다.
code-level 계약(`account.list`, `account.create`, `account.disable`, 마지막 enabled admin 비활성화 거절)과 설계
`docs/superpowers/specs/2026-09-20-purecvisor-desktop-node-p1-admin-account-crud-design.md`가 근거로 남는다.

## Nonclaims

- probe는 승격 근거가 아니다. 계정을 만들지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
