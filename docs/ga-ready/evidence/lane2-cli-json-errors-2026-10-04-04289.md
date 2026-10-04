# Lane 2 `pcvcli --json` 오류 출력 확인 `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `lane2-cli-json-errors-2026-10-04-04289`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
family: `cli.json-errors`
design: `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-cli-json-errors-design.md`
version: `0.42.89-admin-smoke`
installed_product_version: `0.42.89-admin-smoke+a780928ee41f6064cbeebec754eca647dfff42f1`
release_train: `0.42.89-admin-smoke`
source_fix: `0c95852` (PR #34 merge `d6711f3`)
artifact_root: `artifacts/lane2-cli-json-errors-20261004-04289`
probe_script_sha256: `49a128623ae3c4427377a73a79c2cb0171213157ec3b4a2a25fbd5270d9dcf77`
probe_summary_sha256: `9093b4eeb5b6975cf04c318e81a357519cf1eefa3d84c0efd2d32824ffa2f375`
vm_created: `false`
host_mutation_performed: `false`
secret_observed: `false`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

release train `0.42.89`에 실은 PR #34의 Lane 2 probe다. 설계 §3의 네 항목을 fullgate 설치본 `pcvcli.exe`(`0.42.89-admin-smoke+a780928…`)로 확인했다. 실행 스크립트는 artifact root의 `probe.ps1`이고 VM을 만들지 않는다. 존재하지 않는 VM 이름 `pcv-no-such-vm`은 실행 전에 Hyper-V에 없음을 확인했다. token은 보호된 token 파일로만 넘겼고 값은 출력하거나 기록하지 않았다.

## 결과

| case | 명령 | exit | stdout | stderr |
| --- | --- | ---: | --- | --- |
| 1 | `pcvcli --protected-token-file <api-token.dpapi.json> --json vm get pcv-no-such-vm` | `1` | API envelope 그대로(`ok=false`, `operation=vm.get`, `error.code=PCV_VM_NOT_FOUND`, `287` byte) | `code=PCV_VM_NOT_FOUND`, `message=`, `detail=` 줄 |
| 2 | `pcvcli --json no-such-command` | `2` | `{"ok":false,"operation":"pcvcli","error":{"code":"PCV_CLI_USAGE",...}}` (`149` byte) | `PCV_CLI_USAGE|Unknown command group 'no-such-command'.` |
| 3 | `pcvcli --protected-token-file <api-token.dpapi.json> vm get pcv-no-such-vm` (table) | `1` | 비어 있음(`0` byte) | case 1과 같은 세 줄 |

| 설계 §3 항목 | 판정 |
| --- | --- |
| 1. exit `1`, stdout JSON, `ok=false`, `error.code`가 `PCV_`로 시작, stderr `code=` 줄 | `PASS` |
| 2. exit `2`, stdout JSON `error.code`가 `PCV_CLI_`로 시작 | `PASS` (`PCV_CLI_USAGE`) |
| 3. table 형식은 exit `1`, stdout 비움 | `PASS` |
| 4. 출력에 token 형태 문자열 없음 | `PASS` (세 case 모두 `0`개) |

case 2의 code는 오류 문구 앞의 `PCV_CLI_USAGE|`에서 왔다. 설계 §2 표의 "오류 문구가 `PCV_*|`로 시작하면 그 code" 규칙대로다. summary의 자동 판정 11개 check가 모두 통과했다(`failed_checks` 없음).

## Nonclaims

- 읽기 전용 probe다. 설치본, 서비스, VM을 바꾸지 않았다.
- 결과는 `installed_non_promoted_candidate`다. operational current는 `0.42.88-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
