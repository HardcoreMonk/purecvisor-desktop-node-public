# Guest-exec argv fidelity와 FC-12(b) guest 측 종결 (2026-08-06)

evidence_id: `guest-exec-argv-fidelity-fc-12b-closure-2026-08-06`
result: `PASS`
evidence_scope: `code-level-fix-with-actual-windows-guest-verification`
guest_vm: `pcv-guest-installed-04253-r1`
host_mutation_performed: `true`
guest_command_performed: `true`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
password_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

`docs/ga-ready/evidence/fc-05-fc-12b-fc-13-verification-2026-08-06-04270.md` §3은 FC-12(b)
guest 측을 `미확정`으로 남기고 두 가설(stream 인코딩 / argv 전달)을 분리하지 못했다고 기록했다.
이 문서가 그 항목을 닫는다. **원인은 인코딩이 아니라 argv 전달이었다.**

## 1. 근본 원인

`src/DesktopNode.HyperV/DesktopNodeHyperVPowerShellDirectGuestExecutionProvider.cs`의 bridge가
argv 배열을 공백으로 이어붙여 guest에서 PowerShell로 **재파싱**했다.

```powershell
$script = [scriptblock]::Create(([string[]]$payload.command -join ' '))
```

결과는 두 가지다.

- 공백이 든 인자가 **쪼개진다.** FC-12(b) 표본 `café 한글 日本語 Ж Ω ß`는 인코딩으로 뭉개진 게
  아니라 공백에서 `6`개 인자로 갈라져 `6`줄로 출력됐다. 기록된 `27` bytes가 UTF-8(`33`)도
  OEM 손실(`19`)도 아니었던 이유가 이것이다.
- PowerShell 메타문자가 든 인자가 **실행된다.** `$(...)`는 평가되고 `;`는 두 번째 문장을 연다.

이 경로는 ADR-0009가 보안 bounded context로 고정한 곳이고, PCVCLI 계약은
`pcvcli vm guest-exec <vm> -- <command>`로 argv 전달을 문서화하고 있었다. 즉 구현이 문서화된
계약을 지키지 않았다. 현재 동작을 잠그는 테스트는 없었다.

> **2026-08-08 종결.** 당시 ADR-0009에는 argv fidelity 조항 자체가 없었다. 계약을 든 것은 PCVCLI
> 문서뿐이어서, 구현이 그것을 어겨도 **위반되는 ADR이 없는** 구조였다. 이 간극을
> `docs/followup-work-plan-2026-08-07.md` §4가 결정 항목으로 올렸고, 2026-08-08에 선택지 `A`로
> 결정해 ADR-0009에 `## Argv Fidelity 경계` 절과 결정 마커 `argv_fidelity_policy`, 검증 Gate
> `10`번을 추가했다. 구현 변경은 없다 — 이 문서가 기록한 수정과
> `GuestExecutionArgvFidelityTests` `6`건이 이미 그 형태다.

`dcb703ad`의 UTF-8 stream 고정은 옳았고 충분했다. argv join이 그 위에서 결과를 망가뜨려
인코딩 결함처럼 보이게 만들었을 뿐이다.

## 2. 수정

argv를 코드가 아닌 **데이터**로 넘긴다. 원소 `0`이 명령이고 나머지는 splat된 인자다.

```powershell
param([string[]]$argv)
if ($argv.Length -eq 1) { & $argv[0] }
else { & $argv[0] @($argv[1..($argv.Length - 1)]) }
```

`Invoke-Command`에 `-ArgumentList (, $pcvArgv)`로 전달한다. 길이 `1` 분기는 중복이 아니다.
PowerShell 범위는 내림차순도 성립해 `$argv[1..0]`이 `@($argv[1], $argv[0])`을 돌려주므로,
단일 원소일 때 명령이 자기 자신을 인자로 받게 된다.

## 3. 실제 guest 실측

`pcv-guest-installed-04253-r1`을 기동해 같은 세션에서 수정 전/후를 나란히 실행했다. 두 형태 모두
PowerShell Direct 경계를 실제로 넘는다. credential은 DPAPI LocalMachine 참조로만 해석했고 raw
password는 어디에도 남기지 않았다.

| 케이스 | argv | 수정 전 | 수정 후 |
| --- | --- | --- | --- |
| 공백 인자 | `Write-Output` / `a b c` | `a` `b` `c` 3줄, `7` bytes | **`a b c`, `5` bytes** |
| subexpression | `Write-Output` / `$(1+1)` | **`2`로 평가됨**, `1` byte | **`$(1+1)`, `6` bytes** |
| 문장 구분자 | `Write-Output` / `x; Write-Output INJECTED` | **`x`와 `INJECTED` 2줄 실행됨**, `11` bytes | **`x; Write-Output INJECTED`, `24` bytes** |
| 비 ASCII | `Write-Output` / `café 한글 日本語 Ж Ω ß` | `6`줄로 분해, `36` bytes | **`café 한글 日本語 Ж Ω ß`, `31` bytes** |
| 단일 인자 | `hostname` | `15` bytes | `15` bytes (동일) |

표본의 기대 UTF-8 길이는 `31` bytes이고 수정 후 관측값이 **정확히 일치**한다. FC-12(b)의 guest
측 왕복은 이것으로 닫힌다.

단일 인자 케이스가 양쪽에서 같은 것은 정상이다. 메타문자도 공백도 없어 join이 원본과 같은
문자열을 만든다. 감사 §12가 `hostname`을 `17` bytes로 정상 기록했던 것과 같은 이유다.

## 4. 영향 범위

수정 전 동작은 `guest.exec` 권한을 가진 호출자가 argv 원소에 넣은 값이 guest에서 실행됐다는
뜻이다. 다만 이 endpoint의 목적 자체가 인가된 호출자의 guest 명령 실행이므로 **권한 상승은
아니다.** 실질 영향은 두 가지다.

- 운영자가 넘긴 리터럴 인자가 조용히 재해석된다 (정합성 결함).
- 자동화가 신뢰할 수 없는 데이터를 argv 원소로 넘기면 그 데이터가 실행 가능해진다 (호출자 측 위험).

## 5. 검증

| 명령 | 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.sln` | 통과 `842`, 실패 `0`, 건너뜀 `0` (HyperV `131` → `137`) |
| `GuestExecutionArgvFidelityTests` | 통과 `6`, 실패 `0` |
| 실제 guest 왕복 (위 §3) | `5`개 케이스 전부 기대값 일치 |

신규 테스트가 공허하지 않음을 실측했다. 구현을 옛 join 형태로 되돌리면 `6`건 중 `5`건이
실패하고, 복구하면 `6/6` 통과한다. 통과한 `1`건은 단일 인자 케이스로, 양쪽에서 동작이 같은 것이
정상이다.

## 6. guest 자산 상태

이전 FC evidence는 이 VM의 `AutomaticCheckpointsEnabled=True` 때문에 단순 기동만으로 디스크
체인이 바뀐다고 기록하고 권고로 남겼다. 이번에 그 권고를 실행한 뒤 기동했다.

| 항목 | 기동 전 | 종료 후 |
| --- | --- | --- |
| state | `Off` | `Off` |
| `AutomaticCheckpointsEnabled` | `True` → `False` (이번에 변경) | `False` |
| checkpoints | `0` | `0` |
| 연결 디스크 | `pcv-guest-installed-04253-r1.vhd` | `pcv-guest-installed-04253-r1.vhd` |

이번 실행은 디스크 체인에 부작용을 남기지 않았다. keep policy, Notes, credential은 그대로다.

## Nonclaims

- 설치본을 바꾸지 않았다. `0.42.70-admin-smoke` 설치본에는 여전히 수정 전 코드가 들어 있고, 이
  수정은 다음 package 후보에 포함된다. 이 문서는 **source 수정의 실제 guest 검증**이지 설치본
  검증이 아니다.
- operational anchor를 승격하지 않는다. `0.42.70-admin-smoke` 그대로다.
- 수정 전 동작을 권한 상승 취약점으로 주장하지 않는다. §4 참조.
- public trusted signing과 external stable publication은 범위 밖이며 주장하지 않는다.
