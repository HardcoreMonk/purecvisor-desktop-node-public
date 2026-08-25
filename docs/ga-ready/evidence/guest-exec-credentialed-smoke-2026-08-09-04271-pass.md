# Credentialed Windows guest-exec 설치본 왕복 smoke `0.42.71` (2026-08-09)

evidence_id: `guest-exec-credentialed-smoke-2026-08-09-04271-pass`
result: `PASS`
evidence_scope: `installed-windows-vhd-credentialed-guest-exec-and-argv-fidelity`
version: `0.42.71-admin-smoke`
artifact_root: `artifacts/guest-exec-credentialed-smoke-20260809-04271`
summary: `artifacts/guest-exec-credentialed-smoke-20260809-04271/summary-final.json`
vm_name: `pcv-guest-installed-04253-r1`
credential_ref_type: `dpapi-local-machine`
credential_ref: `dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi`
host_mutation_performed: `true` (VM start/stop only; no create/delete)
guest_command_performed: `true`
secret_observed: `false`
password_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

선행 status: `docs/ga-ready/evidence/guest-exec-credentialed-smoke-status-2026-08-09.md`  
설치본 반영: `0.42.71` package/fullgate/manual-admin/current-card  
FC-12(b) source: `docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`

## 1. 실행 환경

| 항목 | 값 |
| --- | --- |
| 설치본 version | `0.42.71-admin-smoke` |
| service | `PureCVisorDesktopNode` Running |
| CLI | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| token | protected file (DPAPI LocalMachine); raw token not observed |
| guest VHD | `D:\PureCVisor\SmokeVMs\pcv-guest-installed-04253-r1\pcv-guest-installed-04253-r1.vhd` |
| elevation | 관리자 opt-in (`Start-Process -Verb RunAs`) |

## 2. 결과

### 2.1 Channel verify + baseline exec

| 단계 | job_id | status | exit | transport |
| --- | --- | --- | ---: | --- |
| `vm.guest.channel.verify` | `job-59d18b5443f840a69db68b280f2e414e` | succeeded | 0 | windows-powershell-direct |
| `vm.guest.exec -- hostname` | `job-afbd103a33e5459bbcee00f28e9d6556` | succeeded | 0 | windows-powershell-direct |

hostname `stdout_byte_count=17` (guest host name + CRLF 계열; 이전 04255/04270 관측과 동일 대역).

### 2.2 FC-12(b) argv fidelity (설치본)

argv 형태는 FC-12(b) 실측과 동일하다. **`powershell -Command` wrapper를 쓰지 않는다**
(`-Command`는 후속 인자를 공백 join 후 코드로 재해석하므로 fidelity 검증 대상이 아니다).

| 케이스 | argv | 기대 content bytes | 관측 `stdout_byte_count` | content (=관측-2 CRLF) | 판정 |
| --- | --- | ---: | ---: | ---: | --- |
| space-arg | `Write-Output`, `a b c` | 5 | **7** | **5** | PASS |
| subexpression | `Write-Output`, `$(1+1)` | 6 | **8** | **6** | PASS |
| statement-sep | `Write-Output`, `x; Write-Output INJECTED` | 24 | **26** | **24** | PASS |
| non-ascii | `Write-Output`, `café 한글 日本語 Ж Ω ß` | 31 | **33** | **31** | PASS |

Windows guest `Write-Output`이 끝에 CRLF(`+2`)를 붙인다. FC-12(b) source evidence의 기대값은
**본문 UTF-8 길이**이며, 설치본 관측은 본문+CRLF다. 네 케이스 모두 **본문 길이가 기대값과
정확히 일치**하고, subexpression이 `2`로 평가되지 않으며 statement separator가 두 줄 실행되지
않는다.

audit: `guest-execution-audit-v1`, redaction: `guest-execution-redaction-v1`,
`credential_ref` redacted, token/password 값 미관측.

### 2.3 guest 자산

| 항목 | 종료 후 |
| --- | --- |
| VM state | `Off` |
| keep policy / Notes / credential | 유지 |
| VM create/delete | 하지 않음 |

## 3. 실행 명령 (대표)

```powershell
# elevated
pcvcli --format json --protected-token-file C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json `
  vm guest-agent-ensure-channel pcv-guest-installed-04253-r1 --verify `
  --credential-ref dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi `
  --timeout-sec 90

pcvcli --format json --protected-token-file ... vm guest-exec pcv-guest-installed-04253-r1 `
  --credential-ref dpapi:... --timeout-sec 90 -- hostname

pcvcli ... vm guest-exec ... -- Write-Output "a b c"
pcvcli ... vm guest-exec ... -- Write-Output '$(1+1)'
pcvcli ... vm guest-exec ... -- Write-Output 'x; Write-Output INJECTED'
pcvcli ... vm guest-exec ... -- Write-Output 'café 한글 日本語 Ж Ω ß'
```

artifact: `run-elevated-guest-exec-smoke-r3.ps1`, `summary-r3.json`, `summary-final.json`, `run-r3.log`.

## 4. Nonclaims

- public trusted signing / external stable publication을 주장하지 않는다
- guest VM 생성·삭제·credential 평문 입력을 수행하지 않았다
- stdout/stderr 원문을 evidence에 싣지 않았다 (digest + byte count만)
- operational anchor 승격은 이 smoke 단독으로 하지 않는다 (`0.42.71-admin-smoke` 유지)
