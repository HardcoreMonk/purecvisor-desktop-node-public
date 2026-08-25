# PureCVisor Desktop Node CLI 명령 사용법

작성 기준: 2026-08-12

운영자 대상 용어와 제품 경계 문구는 `docs/OPERATOR_SURFACE_TERMS.md`에 모은다.

`pcvcli.exe`는 설치된 PureCVisor Desktop Node Local API를 호출하는 .NET command-line client다. Web Console과 같은 API contract를 사용하며, Hyper-V helper나 Linux `purecvisor-single` runtime을 직접 실행하지 않는다. 현재 first-class CLI command surface는 host/runtime/ops/network/VM/job/diagnostics이며, Account/RBAC/JWT login/refresh/logout은 Web Console 또는 Web API 직접 호출 경로가 소유한다. `POST /api/v1/auth/loopback-session`도 Web Console 전용이다. PCVCLI는 설치본 protected token file을 계속 사용한다.

이 문서는 설치된 제품을 사용하는 운영자와 repository checkout에서 CLI를 검증하는 개발자를 함께 대상으로 한다.

## 빠른 시작

설치본은 제품 경로를 machine `PATH`에 등록한다. 설치 후 새 PowerShell/터미널에서는 전체 경로 없이 실행할 수 있다.

```powershell
pcvcli
pcvcli --help
```

개발 checkout에서 실행:

```powershell
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj --
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- --help
```

기본 API base URL은 `http://127.0.0.1:7777`이다. 설치 service가 기본 Web/API port split으로 실행 중이면 별도 `--api`를 지정하지 않아도 된다.

Token source를 생략하면 CLI는 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`을 자동으로 읽는다. 이 파일은 installed service token store contract와 같은 DPAPI LocalMachine protected token file이다.

```powershell
pcvcli host status
```

Automation에서는 JSON 출력을 권장한다.

```powershell
pcvcli --json vm list
```

## Interactive shell

`pcvcli`를 인자 없이 실행하거나 `pcvcli --interactive`, `pcvcli -i`를 실행하면
interactive shell로 진입한다. 시작 화면은 Linux `pcvctl`과 같은 cyber palette
banner와 `(pcv) ❯` prompt를 표시한다.

```text
 ___  _   _  ___  ___  ___  _ _  _  ___  ___  ___
| . \| | | || . \| __>|  _>| | || |/ __>/ . \| . \
|  /| |_| ||   /| _> | <__| V || |\__ \| | ||   /
|_|  \___/ |_|_\<___>\___/ \_/ |_|<___/\___/|_|_\
            [ NEURAL LINK ESTABLISHED ]

Type 'help' for commands | 'exit' to quit | Tab to complete

(pcv) ❯
```

REPL 안에서는 `help`, `exit`, `quit`을 shell command로 처리한다. 그 외 입력은
one-shot CLI와 같은 parser, token resolver, Local API transport를 사용한다.
따라서 아래 두 호출은 같은 API route로 라우팅된다.

```powershell
pcvcli --json host status

# interactive shell 안에서
--json host status
```

Tab completion은 command prefix 기준으로 동작한다. 예를 들어 `network l<Tab>`은
`network list`로 완성된다.

## 명령 구조

Global option은 command group 앞에 둔다. Command-specific option은 해당 command 뒤에 둔다.

```text
pcvcli [global-options] <group> <command> [arguments] [command-options]
```

예:

```powershell
pcvcli --api http://127.0.0.1:7777 --json vm list
pcvcli --token-env PCV_TOKEN job list --limit 25 --offset 50
pcvcli --token <internal-token> diagnostics bundle create
```

Global option은 `--name value`와 `--name=value`를 모두 지원한다. Command-specific option은 `--name value` 형식을 사용한다.

## 전역 옵션

| Option | 설명 |
|--------|------|
| `--api <url>` | Local API base URL. 기본값은 `http://127.0.0.1:7777` |
| `--format table|json|plain|csv` | 출력 형식 선택. 기본값은 `table` |
| `--json` | `--format json` shortcut |
| `--plain` | `--format plain` shortcut |
| `--csv` | `--format csv` shortcut |
| `--no-color` | Color output 억제용 compatibility flag |
| `--verbose`, `-v` | 요청 method/path를 stderr에 출력. Token 값은 `[redacted]`로 표시 |
| `--interactive`, `-i` | Interactive shell 진입 |
| `--help`, `-h` | 사용법 출력 |
| `--token <token>` | Inline bearer token |
| `--token-file <path>` | Plaintext token file |
| `--token-env <name>` | Environment variable에서 token 읽기 |
| `--protected-token-file <path>` | DPAPI LocalMachine protected token file 읽기 |

Token source를 생략하면 CLI는 기본 protected token file `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`을 자동으로 사용한다. 명시 token source는 한 번에 하나만 지정할 수 있다. 여러 source를 동시에 지정하면 CLI가 API 요청 전에 exit code `2`로 중단한다.

## 인증 token 처리

반복 실행 script에서는 token source를 생략해 기본 protected token file을 사용하는 흐름을 우선한다. 설치본 smoke와 CLI 기능 점검에서 운영자가 매번 token 파일 경로를 계산할 필요가 없도록 하기 위한 계약이다. 기본 파일이 없으면 CLI는 token 없이 route를 호출하고, bearer token이 필요한 route는 API가 `401`을 반환할 수 있다.

명시 override가 필요한 경우에는 `--protected-token-file`, `--token-env`, `--token-file` 순서로 선호한다. `--token`은 임시 운영 확인용으로만 사용한다.

Protected token file 예:

```powershell
$tokenFile = Join-Path $env:ProgramData 'PureCVisor\desktop-node\api-token.dpapi.json'
pcvcli --protected-token-file $tokenFile runtime policy
```

Environment variable 예:

```powershell
$env:PCV_TOKEN = '<internal-token>'
pcvcli --token-env PCV_TOKEN host status
```

Inline token 예:

```powershell
pcvcli --token <internal-token> job get <job_id>
```

CLI는 inline token 값을 stdout/stderr에 출력하지 않는다. `--verbose`에서도 token은 `[redacted]`로 표시한다. 그래도 shell history, CI log, terminal recording에 남을 수 있으므로 운영 script에는 inline token을 피한다.

## 출력 형식

| Format | 용도 |
|--------|------|
| `table` | 기본값. Route-specific 조회는 row/page metadata table로, 그 외 JSON object는 top-level scalar `name=value` summary로 표시 |
| `json` | API response envelope을 그대로 출력. Automation 권장 |
| `plain` | Route-specific 조회는 row/page key-value line으로, 그 외 summary field는 줄 단위로 표시 |
| `csv` | Route-specific 조회는 row/page metadata를 다중 column으로, 그 외 summary 또는 body는 단일 CSV field로 출력 |

`vm list`와 `diagnostics bundle list` 같은 route-specific list는 `table`, `plain`, `csv`에서
목록 row와, route가 제공하는 경우 pagination metadata를 직접 표시한다. `json`은 동일 응답의
원본 envelope을 보존한다.

API가 problem JSON을 반환하면 CLI는 stderr에 `PCV_*: message` 형태로 표시하고 exit code `1`을 반환한다.

## 종료 코드

| Code | 의미 |
|------|------|
| `0` | 성공 또는 help 출력 성공 |
| `1` | API error, transport error, non-success response |
| `2` | CLI usage error, token source conflict, token file/env/protected token 오류 |

## 호스트와 runtime 조회

| Command | API route | 설명 |
|---------|-----------|------|
| `pcvcli host status` | `GET /api/v1/host/status` | Hyper-V, service, admin, host readiness 확인 |
| `pcvcli runtime policy` | `GET /api/v1/runtime/policy` | Runtime/auth/job/native operation policy 확인 |
| `pcvcli ops summary` | `GET /api/v1/ops/summary` | 운영 summary 확인 |
| `pcvcli network inventory` / `pcvcli network list` | `GET /api/v1/network/inventory` | Hyper-V switch inventory 확인 |

예:

```powershell
pcvcli --json runtime policy
pcvcli network inventory
pcvcli network list
```

## 가상 머신 명령

| Command | API route | 설명 |
|---------|-----------|------|
| `pcvcli vm list` | `GET /api/v1/vms` | VM 목록 조회 |
| `pcvcli vm get <vm>` | `GET /api/v1/vms/{vm}` | VM 상세 조회 |
| `pcvcli vm create --name <name> --iso <path> --cpu <n> --memory-mb <mb> --disk-gb <gb> [--vm-root <path>] [--generation <n>]` | `POST /api/v1/vms` | VM 생성 job queue |
| `pcvcli vm create <name> --iso_path <path> --vcpu <n> --memory_mb <mb> --disk_size_gb <gb> [--image_dir <path>]` | `POST /api/v1/vms` | Linux `pcvctl vm create` shape 호환 alias |
| `pcvcli vm start <vm>` | `POST /api/v1/vms/{vm}/start` | VM start job queue |
| `pcvcli vm stop <vm>` | `POST /api/v1/vms/{vm}/poweroff` | Linux `pcvctl vm stop` shape 호환 alias. Desktop Node에서는 poweroff job queue |
| `pcvcli vm shutdown <vm>` | `POST /api/v1/vms/{vm}/shutdown` | Guest shutdown job queue |
| `pcvcli vm guest-shutdown <vm>` | `POST /api/v1/vms/{vm}/shutdown` | Linux `pcvctl vm guest-shutdown` shape 호환 alias |
| `pcvcli vm poweroff <vm>` | `POST /api/v1/vms/{vm}/poweroff` | 강제 전원 종료 job queue |
| `pcvcli vm restart <vm>` | `POST /api/v1/vms/{vm}/restart` | VM restart job queue |
| `pcvcli vm pause <vm>` | `POST /api/v1/vms/{vm}/pause` | VM pause job queue |
| `pcvcli vm resume <vm>` | `POST /api/v1/vms/{vm}/resume` | VM resume job queue |
| `pcvcli vm save <vm>` | `POST /api/v1/vms/{vm}/save` | Hyper-V Saved 상태 저장 job queue. pause와 다른 operation |
| `pcvcli vm resume-saved <vm>` | `POST /api/v1/vms/{vm}/resume-saved` | Hyper-V Saved 상태에서 재개 job queue. `vm resume saved` 두 단어는 거부 |
| `pcvcli vm rename <vm> <new_name>` | `POST /api/v1/vms/{vm}/rename` | VM rename job queue |
| `pcvcli vm console <vm>` / `pcvcli vm vnc <vm>` | `GET /api/v1/vms/{vm}/console` | Console/noVNC session 조회 |
| `pcvcli vm memory-stats <vm>` | `GET /api/v1/vms/{vm}/memory-stats` | Hyper-V VM memory metric 조회 |
| `pcvcli vm cpu-stats <vm>` | `GET /api/v1/vms/{vm}/cpu-stats` | Hyper-V VM CPU metric 조회 |
| `pcvcli vm limit <vm> --cpu N [--memory-mb MB]` | `POST /api/v1/vms/{vm}/limit` | Hyper-V vCPU/startup memory queued mutation. Linux cgroup limit 호환 claim은 하지 않음 |
| `pcvcli vm set-memory <vm> <memory_mb>` | `POST /api/v1/vms/{vm}/set-memory` | Startup memory(MiB) 변경 job queue |
| `pcvcli vm set-vcpu <vm> <vcpu_count>` | `POST /api/v1/vms/{vm}/set-vcpu` | Virtual processor 수 변경 job queue |
| `pcvcli vm disk-resize <vm> <disk_gb>` | `POST /api/v1/vms/{vm}/disk-resize` | 연결된 virtual disk 확장 job queue. 축소는 지원하지 않음 |
| `pcvcli vm eject <vm>` | `POST /api/v1/vms/{vm}/eject` | Virtual DVD drive의 연결 media 제거 job queue |
| `pcvcli vm attach <vm> --iso <path>` | `POST /api/v1/vms/{vm}/attach` | Virtual DVD drive에 ISO를 연결하는 job queue. `--iso_path` alias |
| `pcvcli vm delete-status <vm>` | `GET /api/v1/vms/{vm}/delete-status` | VM delete 진행 상태 조회 |
| `pcvcli vm blkio-get <vm>` | `GET /api/v1/vms/{vm}/blkio` | Hyper-V storage readback. Linux blkio throttle 호환 claim은 하지 않음 |
| `pcvcli vm blkio-set <vm> --disk <disk> --maximum-iops <n> [--minimum-iops <n>] --dry-run` | `POST /api/v1/vms/{vm}/qos/storage/preview` | Storage QoS 변경 preview. Host mutation 없음 |
| `pcvcli vm blkio-set <vm> --disk <disk> --maximum-iops <n> [--minimum-iops <n>] --yes` | `POST /api/v1/vms/{vm}/qos/storage` | Storage QoS 변경 job queue |
| `pcvcli vm bandwidth <vm>` | `GET /api/v1/vms/{vm}/bandwidth` | Hyper-V network adapter readback. Linux bandwidth shaping 호환 claim은 하지 않음 |
| `pcvcli vm bandwidth-set <vm> --adapter <adapter> --maximum-kbps <n> [--minimum-kbps <n>] --dry-run` | `POST /api/v1/vms/{vm}/qos/network/preview` | Network bandwidth 변경 preview. Host mutation 없음 |
| `pcvcli vm bandwidth-set <vm> --adapter <adapter> --maximum-kbps <n> [--minimum-kbps <n>] --yes` | `POST /api/v1/vms/{vm}/qos/network` | Network bandwidth 변경 job queue |
| `pcvcli vm guest-agent-status <vm>` | `GET /api/v1/vms/{vm}/guest-agent/status` | Hyper-V Integration Services readiness readback |
| `pcvcli vm guest-ping <vm>` | `GET /api/v1/vms/{vm}/guest-agent/ping` | VM state 기반 guest service readiness readback |
| `pcvcli vm guest-agent-ensure-channel <vm> --dry-run` | `POST /api/v1/vms/{vm}/guest/channel/preview` | Guest channel 상태와 조치 계획 preview |
| `pcvcli vm guest-agent-ensure-channel <vm> --verify --credential-ref <ref> [--timeout-sec <n>]` | `POST /api/v1/vms/{vm}/guest/channel/verify` | Protected credential reference로 guest channel 검증 job queue |
| `pcvcli vm guest-agent-ensure-channel <vm> --repair --yes` | `POST /api/v1/vms/{vm}/guest/channel` | Guest channel 복구 job queue. 명시 확인 필요 |
| `pcvcli vm guest-exec <vm> --dry-run [--credential-ref <ref>] [--timeout-sec <n>] -- <command...>` | `POST /api/v1/vms/{vm}/guest/exec/preview` | Command hash/redaction/audit preview. Guest 실행 없음 |
| `pcvcli vm guest-exec <vm> --credential-ref <ref> [--timeout-sec <n>] -- <command...>` | `POST /api/v1/vms/{vm}/guest/exec` | Protected credential reference 기반 guest execution job queue |
| `pcvcli vm manage <vm> --yes` | `POST /api/v1/vms/{vm}/manage` | existing Hyper-V VM을 PureCVisor managed로 승격. `--yes` 필수. body `confirm_name`은 `<vm>` 인자 그대로 |
| `pcvcli vm delete <vm> --yes` | `DELETE /api/v1/vms/{vm}` | Managed VM delete job queue |

VM 생성 예:

```powershell
pcvcli vm create `
  --name ubuntu-lab-01 `
  --iso D:\isos\ubuntu.iso `
  --cpu 2 `
  --memory-mb 4096 `
  --disk-gb 40 `
  --vm-root D:\PureCVisor\VMs `
  --generation 2
```

Linux `pcvctl` 형식의 VM 생성 예:

```powershell
pcvcli vm create ubuntu-lab-01 `
  --iso_path D:\isos\ubuntu.iso `
  --vcpu 2 `
  --memory_mb 4096 `
  --disk_size_gb 40 `
  --image_dir D:\PureCVisor\VMs
```

`pcvcli vm manage <vm> --yes`는 existing Hyper-V VM에 managed marker를 붙이는 queued job이다. `--yes`가 없으면 `PCV_CLI_CONFIRMATION_REQUIRED`다. body `confirm_name`은 `<vm>` 인자를 그대로 넣는다.

VM delete는 destructive host mutation을 queue하므로 `--yes`가 필수다. API는 PureCVisor managed marker가 없는 VM을 provider mutation 전에 차단한다. unmanaged delete 거절은 manage 이후에도 다른 unmanaged VM에 유지된다.

```powershell
pcvcli vm manage ubuntu-lab-01 --yes
pcvcli vm delete ubuntu-lab-01 --yes
```

VM 이름이나 checkpoint 이름에 공백이 있으면 PowerShell quoting을 사용한다.

```powershell
pcvcli vm get 'ubuntu lab'
```

## VM media와 resource 변경

`set-memory`, `set-vcpu`, `disk-resize`, `eject`, `attach`는 Local API에 Hyper-V host mutation
job을 queue한다. 응답으로 받은 job ID는 `pcvcli job get <job_id>`로 추적한다.

```powershell
# Startup memory를 4096 MiB로 변경
pcvcli --json vm set-memory ubuntu-lab-01 4096

# Virtual processor 수를 4개로 변경
pcvcli --json vm set-vcpu ubuntu-lab-01 4

# Virtual disk 목표 크기를 80 GiB로 확장
pcvcli --json vm disk-resize ubuntu-lab-01 80

# Virtual DVD drive에서 ISO/media 제거
pcvcli --json vm eject ubuntu-lab-01

# Virtual DVD drive에 ISO 연결
pcvcli --json vm attach ubuntu-lab-01 --iso D:\isos\ubuntu.iso

# 비동기 delete 진행 상태 조회
pcvcli --json vm delete-status ubuntu-lab-01
```

Resource 값은 정수로 전달한다. `set-memory`는 MiB, `set-vcpu`는 processor count,
`disk-resize`는 GiB 단위의 목표 크기다. VM 상태나 Hyper-V capability가 변경을 허용하지
않으면 queued job이 실패 상태와 `PCV_*` 오류를 반환한다. `disk-resize`는 확장만 지원하며
현재 virtual disk보다 작은 값을 요청하면 `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`로 거절한다.

Storage/network QoS는 먼저 `--dry-run`으로 확인한 뒤 같은 값에 `--yes`를 붙여 적용한다.

```powershell
pcvcli --json vm blkio-set ubuntu-lab-01 `
  --disk disk0 --maximum-iops 1200 --minimum-iops 100 --dry-run
pcvcli --json vm blkio-set ubuntu-lab-01 `
  --disk disk0 --maximum-iops 1200 --minimum-iops 100 --yes

pcvcli --json vm bandwidth-set ubuntu-lab-01 `
  --adapter adapter0 --maximum-kbps 20480 --minimum-kbps 1024 --dry-run
pcvcli --json vm bandwidth-set ubuntu-lab-01 `
  --adapter adapter0 --maximum-kbps 20480 --minimum-kbps 1024 --yes
```

## Guest channel과 guest execution

Guest operation은 ADR-0009 보안 경계를 따른다. 먼저 channel preview를 확인하고,
검증에는 raw password가 아니라 Windows Credential Manager 또는 DPAPI가 소유하는
protected credential reference를 전달한다.

```powershell
$credentialRef = 'wincred:PureCVisor/guest/admin'

# Read-only plan preview
pcvcli --json vm guest-agent-ensure-channel ubuntu-lab-01 --dry-run

# Channel 검증 job
pcvcli --json vm guest-agent-ensure-channel ubuntu-lab-01 `
  --verify --credential-ref $credentialRef --timeout-sec 30

# 필요한 channel 복구 job
pcvcli --json vm guest-agent-ensure-channel ubuntu-lab-01 --repair --yes
```

Guest command 앞의 `--`는 CLI option과 guest argv의 경계다. `--` 뒤 인자는 배열 순서를
유지해 provider로 전달하며 CLI가 다시 command string으로 결합하거나 해석하지 않는다.

```powershell
# 실행하지 않고 redaction/audit contract만 확인
pcvcli --json vm guest-exec ubuntu-lab-01 `
  --dry-run --credential-ref $credentialRef --timeout-sec 30 -- `
  powershell.exe -NoProfile -Command hostname

# 실제 guest execution job queue
pcvcli --json vm guest-exec ubuntu-lab-01 `
  --credential-ref $credentialRef --timeout-sec 45 -- `
  powershell.exe -NoProfile -Command hostname
```

실제 실행에는 `--credential-ref`가 필수다. `--password`, `--token`, `--secret`, raw key처럼
secret-bearing command option은 `PCV_CLI_CREDENTIAL_REF_REQUIRED`로 거절된다. Credential
값을 `--` 뒤 guest argv에 넣거나 shell history, 문서, job 이름에 기록하지 않는다. 실행
상태는 `job get`으로 확인하고 queued/running execution 취소는 `job cancel`을 사용한다.

## 체크포인트 명령

| Command | API route | 설명 |
|---------|-----------|------|
| `pcvcli vm checkpoint list <vm>` | `GET /api/v1/vms/{vm}/checkpoints` | Checkpoint 목록 조회 |
| `pcvcli vm checkpoint create <vm> --name <checkpoint>` | `POST /api/v1/vms/{vm}/checkpoints` | Checkpoint 생성 job queue |
| `pcvcli vm checkpoint restore <vm> <checkpoint>` | `POST /api/v1/vms/{vm}/checkpoints/{checkpoint}/restore` | Checkpoint restore job queue |
| `pcvcli vm checkpoint delete <vm> <checkpoint>` | `DELETE /api/v1/vms/{vm}/checkpoints/{checkpoint}` | Checkpoint delete job queue |
| `pcvcli vm snapshot list <vm>` | `GET /api/v1/vms/{vm}/checkpoints` | Linux `vm snapshot list` shape 호환 alias |
| `pcvcli vm snapshot create <vm> --name <checkpoint>` | `POST /api/v1/vms/{vm}/checkpoints` | Snapshot create alias |
| `pcvcli vm snapshot rollback <vm> <checkpoint>` | `POST /api/v1/vms/{vm}/checkpoints/{checkpoint}/restore` | Snapshot rollback alias |
| `pcvcli vm snapshot delete <vm> <checkpoint>` | `DELETE /api/v1/vms/{vm}/checkpoints/{checkpoint}` | Snapshot delete alias |

CLI checkpoint restore/delete는 명시 subcommand 자체가 operator intent이며 추가
interactive prompt를 띄우지 않는다. Web Console checkpoint restore/delete는
confirmation dialog를 통과해야 job을 queue한다.

예:

```powershell
pcvcli vm checkpoint create ubuntu-lab-01 --name before-upgrade
pcvcli vm checkpoint restore ubuntu-lab-01 before-upgrade
pcvcli vm checkpoint delete ubuntu-lab-01 before-upgrade
pcvcli vm snapshot rollback ubuntu-lab-01 before-upgrade
```

Checkpoint restore/delete는 VM workload에 영향을 줄 수 있다. 운영에서는 VM 상태와 workload 영향도를 먼저 확인한다.

## 작업 명령

| Command | API route | 설명 |
|---------|-----------|------|
| `pcvcli job list [--limit <n>] [--offset <n>]` | `GET /api/v1/jobs` | Server-side job snapshot 조회 |
| `pcvcli job get <job_id>` | `GET /api/v1/jobs/{job_id}` | Job 상세 조회 |
| `pcvcli job cancel <job_id>` | `POST /api/v1/jobs/{job_id}/cancel` | Job 취소 요청 |
| `pcvcli job retry <job_id>` | `POST /api/v1/jobs/{job_id}/retry` | Retryable failed job 재시도 |
| `pcvcli job reconcile <job_id>` | `POST /api/v1/jobs/{job_id}/reconcile` | Interrupted mutation의 provider postcondition을 읽어 durable job 상태 조정 |

예:

```powershell
pcvcli --json job list --limit 50 --offset 0
pcvcli job get job-123
pcvcli job cancel job-123
pcvcli job retry job-123
pcvcli --json job reconcile job-123
```

`limit`과 `offset`은 integer여야 한다. API의 job retention 정책은 user guide의 Operator Activity 설명을 따른다.

`job reconcile`은 일반 retry가 아니다. Service restart 등으로 `PCV_JOB_INTERRUPTED`가 된
`vm.rename`, `vm.delete`, `checkpoint.create`, `checkpoint.restore` job에만 사용한다. API는 먼저 Hyper-V/provider
readback으로 원래 mutation의 postcondition을 확인한다. Postcondition이 확정되면 기존
mutation을 중복 제출하지 않고 기존 job을 reconciled terminal state로 저장한다. 결과가 없거나
모호하면 `409`와 `PCV_JOB_RECONCILIATION_REQUIRED`를 반환한다. 이때 `retry`를 먼저 실행하지
말고 `job get`, Hyper-V 상태, diagnostic bundle을 보존해 운영 절차에 따라 확인한다.

## 진단 bundle 명령

| Command | API route | 설명 |
|---------|-----------|------|
| `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]` | `GET /api/v1/diagnostics/bundles?limit=<n>&offset=<n>` | Bundle metadata 목록과 pagination 조회 |
| `pcvcli diagnostics bundle create` | `POST /api/v1/diagnostics/bundles` | Diagnostic bundle 생성 |
| `pcvcli diagnostics bundle download <bundle_id> --output <path>` | `GET /api/v1/diagnostics/bundles/{bundle_id}/download` | Bundle body를 file로 저장 |

예:

```powershell
$page = pcvcli --json diagnostics bundle list --limit 10 --offset 0 | ConvertFrom-Json
$page.data.bundles | Select-Object bundle_id, created_at, size_bytes, download_url

$created = pcvcli --json diagnostics bundle create | ConvertFrom-Json
pcvcli diagnostics bundle download $created.data.bundle_id --output D:\evidence\$($created.data.bundle_id).json
```

`pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]`는
`GET /api/v1/diagnostics/bundles?limit=<n>&offset=<n>`에 매핑되어 diagnostics root에 남아 있는
bundle을 최신순으로 조회한다. Account/RBAC mode에서는 `diagnostics.read` permission이 필요하다.

응답의 주요 필드:

| 필드 | 의미 |
|------|------|
| `bundles[]` | 현재 page의 bundle metadata 목록 |
| `bundle_id`, `file_name` | Download에 사용할 식별자와 server-side 파일명 |
| `created_at`, `last_write_time_utc`, `size_bytes` | 생성 시각, 최종 기록 시각, 크기 |
| `download_url` | 해당 bundle의 download route |
| `count`, `returned` | Retention 적용 후 전체 유효 bundle 수와 현재 page 반환 수 |
| `limit`, `offset`, `next_offset` | 현재 pagination과 다음 page offset. 마지막 page는 `next_offset=null` |
| `retention` | `retention_days`, `max_bundle_count`, `cutoff_utc`, 이번 호출에서 제거한 `removed[]` |

`limit` 기본값은 `10`, 허용 범위는 `1..100`, `offset` 기본값은 `0`이다. 기본 retention은
14일 또는 최대 50개다. 목록 API는 응답을 만들기 전에 retention을 적용하므로 만료됐거나
최대 개수를 넘은 bundle 파일을 diagnostics root에서 제거할 수 있다. Hyper-V, VM, service,
MSI, firewall을 변경하는 host mutation은 아니지만 완전한 filesystem no-op 조회도 아니다.

Diagnostics root가 구성되지 않았으면 `409 PCV_DIAGNOSTIC_BUNDLE_ROOT_NOT_CONFIGURED`를
반환한다. CLI에서 non-integer `--limit`/`--offset`은 API 요청 전에 `PCV_CLI_USAGE`로 실패한다.
Malformed direct API query는 `PCV_DIAGNOSTIC_BUNDLE_LIST_PAGE_INVALID`로 거절한다. Integer
범위를 벗어난 `limit`/`offset`은 각각 `PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE`,
`PCV_DIAGNOSTIC_BUNDLE_LIST_OFFSET_OUT_OF_RANGE`로 거절한다.

Download command는 성공 시 output directory를 만들고 response body를 지정한 path에 저장한다.

## API/Web Console 전용 discovery 기능

### Console capabilities

`GET /api/v1/console/capabilities`는 현재 listener가 제공할 수 있는 console transport와
접근 조건을 설명하는 read-only discovery API다. 이 호출 자체가 `vmconnect`를 실행하거나
noVNC session을 만들지는 않으며 host mutation도 수행하지 않는다.

전역 capability discovery는 API/Web Console 전용이다. CLI의 전역 운영 정책 조회는
`pcvcli runtime policy`, VM별 console handoff/session 조회는 `pcvcli vm console|vnc <vm>`이
소유하며 `pcvcli console capabilities` command는 없다. 이는 의도한 operator surface
ownership이며 backend gap이 아니다.

응답의 주요 필드:

| 필드 | 의미 |
|------|------|
| `windows_console` | Local Hyper-V `vmconnect` 사용 가능 여부와 `operator-local-handoff` launch mode |
| `novnc` | Browser streaming 활성 여부, bridge mode, transport, WebSocket path template, 비활성 이유 |
| `console_access.contract` | Web/CLI가 함께 해석하는 `console-access-card.v1` contract |
| `console_access.account` | 인증 surface(`service-token-or-account-jwt`), 실제 session에 필요한 `console.view`, token redaction 상태 |
| `console_access.status` | `disabled`, `local-console-handoff-ready`, `browser-streaming-available` 중 현재 상태 |
| `console_access.next_action` | 현재 설정에 맞는 vmconnect/noVNC 다음 조치 안내 |

Capabilities route는 authenticated `read` discovery이고, 실제 VM별 console session
`GET /api/v1/vms/{vm}/console`은 Account/RBAC mode에서 `console.view` permission을 요구한다.
noVNC 상태는 console 전체가 꺼졌으면 `disabled`, local vmconnect만 가능하면
`not_configured`, WebSocket/VNC bridge가 구성됐으면 `available`이다.

```powershell
$capabilities = Invoke-RestMethod `
  -Uri 'http://127.0.0.1:7777/api/v1/console/capabilities' `
  -Headers $headers

$capabilities.data.console_access

# 실제 VM별 handoff/session metadata는 기존 CLI command로 조회
pcvcli --json vm console ubuntu-lab-01
pcvcli --json vm vnc ubuntu-lab-01
```

PCVCLI에 별도 `console capabilities` command를 두지 않은 것은 실제 운영 action이 VM별
`vm console|vnc`이고, 전역 capability card는 Web Console의 연결/문제 해결 화면이 주로
소비하기 때문이다. `vm console|vnc`도 GUI를 자동 실행하지 않고 vmconnect 또는 noVNC로
이어갈 session/handoff metadata를 출력한다.

## 문제 해결

| 증상 | 확인 |
|------|------|
| `PCV_CLI_TOKEN_SOURCE_CONFLICT` | `--token`, `--token-file`, `--token-env`, `--protected-token-file` 중 하나만 사용 |
| `PCV_CLI_TOKEN_ENV_EMPTY` | 지정한 environment variable이 비어 있거나 설정되지 않음 |
| `PCV_CLI_TOKEN_FILE_NOT_FOUND` | Plain token file path 확인 |
| `PCV_CLI_PROTECTED_TOKEN_FILE_NOT_FOUND` | Protected token file path 확인 |
| `PCV_CLI_PROTECTED_TOKEN_UNSUPPORTED` | Protected token file이 DPAPI LocalMachine format인지 확인 |
| `PCV_CLI_API_INVALID` | `--api` URL 형식 확인 |
| `PCV_CLI_TRANSPORT_ERROR` | Service 실행 상태, API port, firewall/LAN policy 확인 |
| `PCV_RATE_LIMIT_EXCEEDED` | 잠시 후 재시도하거나 operator policy 확인 |
| `PCV_CLI_CONFIRMATION_REQUIRED` | `vm delete <vm> --yes`처럼 명시 확인 flag 추가 |
| `PCV_CLI_CREDENTIAL_REF_REQUIRED` | Guest execution에 raw secret option 대신 protected `--credential-ref` 사용 |
| `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` | 현재 disk보다 크거나 같은 `disk_gb` 목표값으로 다시 요청 |
| `PCV_JOB_RECONCILIATION_REQUIRED` | Interrupted job의 provider 상태가 모호함. Retry하지 말고 상태와 evidence 확인 |

서비스 상태:

```powershell
Get-Service PureCVisorDesktopNode
```

기본 API listener 확인:

```powershell
Invoke-WebRequest -Uri 'http://127.0.0.1:7777/api/v1/runtime/policy'
```

Token이 필요한 route는 unauthenticated 호출에서 `401`을 반환할 수 있다. 기본 protected token file이 없거나 별도 token을 검증해야 한다면 명시 token source를 지정해서 다시 실행한다.

## 경계

- `pcvcli.exe`는 Local API thin client다. Service install/start/stop, MSI repair/remove, firewall, trust-store, LAN listener mutation, Event Log provider mutation, update/rollback mutation을 직접 실행하지 않는다.
- VM lifecycle/checkpoint/delete command는 API에 queued job을 요청한다. 실제 host mutation은 installed service와 API authorization/guard contract가 소유한다.
- 2026-05-18 기준 Linux `HardcoreMonk/purecvisor` source commit `abc76d364b716ea4bfca322e914bf8803f013bf6`의 `src/cli/purecvisorctl.c` route table은 KVM/libvirt/LXC/ZFS/OVS/OVN/DPDK/SR-IOV/cloud 등 Linux-only command를 포함한다. Desktop Node PCVCLI는 그중 Windows Hyper-V Local API가 실제 제공하는 host/network/vm/console/checkpoint/job route에 대응되는 alias만 포함한다.
- Public trusted signing, external stable publication, winget public submission은 ADR-0006 내부 사설망 전용 제품 범위 밖이다.

## Linux 호환 명령 승격 상태

`pcvcli-backend-command-gap-slice-2026-05-19`에서 분리했던 Hyper-V 대응 가능
명령 중 media/delete-status와 resource mutation 묶음은 `0.42.38-admin-smoke`
slice에서 code-level 제품 route로 승격했다. 아래 Linux-only 명령은 계속 Desktop
Node 제품 범위 밖으로 유지한다.

code-level 승격 완료:

| 명령 | 현재 분류 |
|------|-----------|
| `vm memory-stats/cpu-stats` | read-only Hyper-V API/CLI route 승격, 0.42.37 설치본 lifecycle smoke에서 확인 |
| `vm rename/pause/resume` | queued mutation API/Hyper-V adapter/CLI route 승격, 0.42.37 설치본 lifecycle smoke와 0.42.38 current-card에서 확인 |
| `vm eject/delete-status` | virtual media/delete progress API/CLI route 승격, `0.42.38-admin-smoke` 설치본 evidence에서 닫음 |
| `vm set-memory/set-vcpu/disk-resize` | `vm-resource-mutation` queued mutation API/CLI/native adapter route 승격, `0.42.38-admin-smoke` full admin host mutation/current-card와 manual-admin package-pair closure에서 확인 |
| `vm limit` | Hyper-V CPU/MEM resource mutation alias로 승격, `0.42.39-admin-smoke` full admin host mutation/current-card/manual-admin package-pair closure에서 확인 |
| `vm blkio-get/bandwidth` | Hyper-V storage/network readback으로 승격, `0.42.39-admin-smoke` full admin host mutation/current-card/manual-admin package-pair closure와 2026-05-21 설치본 targeted CLI smoke에서 확인. Linux QoS mutation claim은 하지 않음 |
| `vm guest-agent-status/guest-ping` | Hyper-V Integration Services readiness/readback으로 승격, `0.42.39-admin-smoke` full admin host mutation/current-card/manual-admin package-pair closure와 2026-05-21 설치본 targeted CLI smoke에서 확인. qemu guest agent claim은 하지 않음 |
| `vm blkio-set` | ADR-0008 기준 Hyper-V storage QoS mutation으로 승격. `--dry-run`은 preview route, `--yes`는 queued apply route를 호출한다. 0.42.47 설치본 actual VM smoke/fullgate와 0.42.45 -> 0.42.47 manual-admin closure가 PASS. 2026-05-29 follow-up부터 음수, 1,000,000,000 초과, `minimum > maximum` 값은 CLI/API에서 `PCV_VM_QOS_STORAGE_RANGE_INVALID`로 먼저 거절 |
| `vm bandwidth-set` | ADR-0008 기준 Hyper-V network port bandwidth mutation으로 승격. `--dry-run`은 preview route, `--yes`는 queued apply route를 호출한다. 0.42.47 설치본 actual VM smoke/fullgate와 0.42.45 -> 0.42.47 manual-admin closure가 PASS. 2026-05-29 follow-up부터 음수, 1,000,000,000 초과, `minimum > maximum` 값은 CLI/API에서 `PCV_VM_QOS_NETWORK_RANGE_INVALID`로 먼저 거절 |
| `vm guest-agent-ensure-channel --dry-run` | ADR-0009 보안 경계 contract 기준 Hyper-V/Windows guest channel preview로 승격. `guest-channel-preview.v1` contract와 verify/repair 가능 상태를 반환한다. 0.42.53 package/fullgate/current-card와 0.42.54 installed current-card에서 확인 |
| `vm guest-agent-ensure-channel --verify/--repair` | ADR-0009 보안 경계 contract 기준 provider route로 승격. Protected credential reference와 confirmation guard를 통과하면 queued job을 요청한다. 실제 Windows guest credentialed smoke PASS |
| `vm guest-exec --dry-run ... -- <command>` | ADR-0009 보안 경계 contract 기준 redaction/audit preview로 승격. `guest-execution-preview.v1` contract, command hash, credential ref hash, redacted argv를 반환한다. 0.42.53 package/fullgate/current-card와 0.42.54 installed current-card에서 확인 |
| `vm guest-exec --credential-ref REF ... -- <command>` | ADR-0009 보안 경계 contract 기준 provider route로 승격. Raw secret 없이 queued guest execution을 요청한다. 실제 Windows guest credentialed smoke, `0.42.54-admin-smoke` 설치본 long-running cancel smoke, `0.42.55-admin-smoke` actual credentialed guest-exec 재확인 PASS |

제품 범위 밖으로 유지:

| Linux 명령 | 분류 |
|------------|------|
| Linux `blkio-set` 세부 flag(`--read-bps`, `--write-bps`, `--read-iops`, `--write-iops`) | Linux cgroup/libvirt throttle semantics는 미지원. Desktop Node는 Hyper-V `--maximum-iops`/`--minimum-iops` 정책만 code-level로 연다 |
