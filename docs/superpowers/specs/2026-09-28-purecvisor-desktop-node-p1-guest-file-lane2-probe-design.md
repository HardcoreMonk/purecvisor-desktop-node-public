# PureCVisor Desktop Node P1-8 guest file Lane 2 프로브 설계

- Design-ID: `purecvisor-desktop-node-p1-guest-file-lane2-probe-v1`
- 작성일: `2026-09-28`
- 문서 상태: `proposed`
- 승인 locator: `User-Approval: 2026-09-28 P1-8 guest file actual-VM 설계 먼저`
- 구현 계획: `docs/superpowers/plans/2026-09-28-purecvisor-desktop-node-p1-guest-file-lane2.md`
- 선행 설계: `docs/superpowers/specs/2026-09-20-purecvisor-desktop-node-p1-guest-file-job-design.md`(Lane 2는 비목표였다)
- 선행 evidence: `guest-exec-credentialed-smoke-2026-08-09-04271-pass`, `persistent-windows-guest-target-policy-2026-05-28-04255`
- 이 문서가 수행하는 host mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 문제

P1-8 guest file(`vm.guest.file.preview`, `vm.guest.file`)은 Lane 1 계약·route·CLI·Web까지 닫혔지만 실제 guest에
파일을 넣는 설치본 검증이 없다. Off VM 기능군과 달리 guest OS가 켜져 있어야 하고(PowerShell Direct), credential이
필요하다.

## 2. Lane 0 관측 (2026-09-28, 읽기 전용)

| 항목 | 값 |
| --- | --- |
| 설치본 | `0.42.81-admin-smoke`, `pcvcli vm guest-file` 있음 |
| 대상 VM | `pcv-guest-installed-04253-r1`, Windows Server 2022 Datacenter Evaluation, Gen1, 구성 `12.0`, 현재 `Off` |
| Notes | `persistent_policy=keep-until-next-evidence-cycle`, `managed-by=purecvisor-desktop-node-smoke` |
| credential-ref | `dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi` (파일 있음, DPAPI LocalMachine) |
| host allowlist root | `C:\ProgramData\PureCVisor\desktop-node\guest-files\` (있음) |
| guest 접두사 | `C:\Users\Public\PureCVisor\` |
| 크기 상한 | `64 MiB` |
| integration service | Heartbeat, KVP, Shutdown, Time sync, VSS 켜짐. Guest Service Interface 꺼짐(PowerShell Direct에는 불필요) |

제품 copy는 `New-PSSession -VMName` + `Copy-Item -ToSession -Destination <guest_path>`다. 대상 부모 디렉터리를
만들지 않는다. guest에 `C:\Users\Public\PureCVisor\`가 없으면 첫 copy가 실패할 수 있다(예상 위험, §5).

## 3. 접근

- 대상은 새 VM이 아니라 **보존 guest** `pcv-guest-installed-04253-r1`이다. guest OS 설치·credential 준비 비용이
  커서 guest-exec evidence도 이 VM을 재사용했다. VM 생성·삭제, Notes·credential 변경을 하지 않는다.
- 전원은 시작 전 상태를 기록하고 끝나면 그 상태로 되돌린다(현재 `Off` → 시작 → 검증 → 정지). 제품 전원 명령만 쓴다.
- 새 runner `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1GuestFileActualVmSmoke.ps1`. P2 Off VM runner의
  골격(설치본 version guard, atomic summary, RuntimeAdapter, secret 검사, 정확한 정리)을 따르고 VM identity는
  기록한 Hyper-V id로 확인만 한다.
- credential은 ref 경로만 CLI 인자로 넘긴다. 비밀번호·token 값은 command line, summary, evidence에 싣지 않는다.
- guest 쪽 확인과 정리는 제품 `vm guest-exec`만 쓴다(native PowerShell Direct fallback 없음). stdout 원문은 싣지 않고
  digest와 byte 수만 쓴다.

## 4. slice

고정 순서, fail-stop. 거절 slice가 mutation slice보다 앞선다.

| slice | 동작 | PASS |
| --- | --- | --- |
| `preflight` | 설치본 version, VM 1개·id, keep policy Notes, credential 파일, host root, 시작 전 전원 기록 | 모두 확인. mutation 없음 |
| `vm_start` | 시작 전 `Off`면 제품 start, `Running` 대기 | Hyper-V `Running`, heartbeat OK |
| `channel_verify` | `vm guest-agent-ensure-channel --verify` | job succeeded, transport `windows-powershell-direct` |
| `guest_prefix_probe` | guest-exec `Test-Path C:\Users\Public\PureCVisor` | 결과 기록(만들지 않음) |
| `host_staging` | host root 아래 run 전용 디렉터리에 `1 MiB` 난수 payload와 `64 MiB + 1` 파일 생성 | 길이 일치, payload SHA-256 기록 |
| `preview_path_not_allowed` | 접두사 밖 guest 경로로 `--dry-run` | `PCV_GUEST_FILE_PATH_NOT_ALLOWED`, job 없음 |
| `preview_size_limit` | `64 MiB + 1` host 파일로 `--dry-run` | `PCV_GUEST_FILE_SIZE_LIMIT`, job 없음 |
| `preview_ok` | run 전용 `1 MiB` 난수 파일 `--dry-run` | 계획 byte = 파일 길이, guest 파일 없음 |
| `copy_confirm_required` | `--dry-run`/`--yes` 없이 호출 | CLI 거절, job 없음 |
| `copy` | `--yes` | job succeeded |
| `guest_readback` | guest-exec `(Get-FileHash -Algorithm SHA256 <guest_path>).Hash` | stdout digest가 기대 `<HASH>` + CRLF의 SHA-256과 같음 |
| `cleanup` | guest-exec로 run 파일 하나만 `Remove-Item`(접두사 안, 정확한 이름), 부재 확인, host staging 삭제, 시작 전 전원으로 복귀 | guest 파일 없음, host staging 없음, 전원 = 시작 전 |

이름: run key는 `Get-ShortHash("$Version|$artifactRoot")`, host staging `guest-files\pcv-p1-guestfile-<tag>-<8hex>\`,
guest 파일 `C:\Users\Public\PureCVisor\pcv-p1-guestfile-<tag>-<8hex>.bin`. runner는 guest 접두사 디렉터리를 만들거나
지우지 않는다.

## 5. 예상 위험

- **guest 접두사 부재**: 제품 copy가 부모 디렉터리를 만들지 않아 `PCV_GUEST_FILE_COPY_FAILED`가 날 수 있다. runner는
  `guest_prefix_probe`로 기록만 하고 우회하지 않는다. 실패하면 제품이 allowlist 접두사를 세션 안에서 만들도록
  Lane 1에서 고친다.
- **평가판 guest**: Windows Server 2022 Evaluation 기한·업데이트로 부팅이 늦을 수 있다. `vm_start`와 `channel_verify`
  timeout을 넉넉히 두고, 실패는 제품 결함과 구분해 기록한다.
- **정지 실패**: cleanup이 전원을 되돌리지 못하면 FAIL로 기록하고 수동 정지를 보고한다. VM을 지우지 않는다.

## 6. 승인

Lane 2 run은 host mutation이다: 보존 VM 시작·정지, guest 파일 쓰기·삭제, host staging 파일 생성·삭제. 이 설계
승인과 별개로 run 승인이 필요하다. VM 생성·삭제, credential 변경, MSI 설치는 이 설계 밖이다.

## 7. 비주장

- guest-to-host pull, 디렉터리 복사, Linux guest는 다루지 않는다.
- current-evidence, feature ledger pass, fullgate, pair를 바꾸지 않는다.
- public trusted signing / external stable publication `not-claimed`
