# PureCVisor Desktop Node CLI

`pcvcli.exe`는 Windows Desktop Node 제품 경로의 active .NET Local API client다. `purecvisor-single`의 command table, transport, formatter 구조를 참고하지만 Desktop Node route만 노출한다.

운영자용 command 문서는 `docs/CLI_COMMAND_USAGE.md`에 둔다.

현재 운영 제품은 `0.42.74-admin-smoke`다. Web Console과 PCVCLI가 active operator
surface이고 TUI는 absent다. 최신 닫힌 manual-admin package-pair는
`0.42.73-admin-smoke -> 0.42.74-admin-smoke`이며 feature qualification은
`promotion_eligible=false`다. Required CI는 final `main`
`6e2bdb93ce308b632c929e2c17f5550ac3845401`, run `32904006595`의 exact contexts
`dotnet`, `web`, `delivery`, `installer-policy`가 소유한다. pwsh 기반 Public Boundary run
`32904006619`는 non-required transition residue다.

현재 first-class command group은 `host`, `runtime`, `ops`, `network`, `vm`, `job`, `diagnostics`다. Account/RBAC/JWT `login`/`refresh`/`logout` route는 의도적으로 Web Console 또는 Web API 직접 호출 경로가 소유한다.

## 사용

```powershell
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj --
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- --interactive
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- --help
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- host status
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- --json vm list
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- job get <job_id>
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- job reconcile <job_id>
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- vm set-memory <vm> 4096
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- vm set-vcpu <vm> 4
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- vm disk-resize <vm> 80
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- vm guest-agent-ensure-channel <vm> --dry-run
dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj -- vm guest-exec <vm> --dry-run -- powershell.exe -NoProfile -Command hostname
```

인자 없이 `pcvcli`를 실행하거나 `--interactive`, `-i`를 지정하면 Linux `pcvctl`
style cyber palette banner, `(pcv) ❯`
prompt, `help`, `exit`/`quit`, prefix 기반 Tab completion을 제공하는 REPL로
진입한다. REPL 안에서 입력한 command는 one-shot CLI와 같은 parser, token resolver,
Local API transport를 사용한다.

## 인증 token source

Token source는 생략할 수 있다. 생략하면 CLI는 기본 protected token file인 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`을 자동으로 읽는다. 파일이 없으면 token 없이 호출하며, bearer token이 필요한 route는 API가 `401`로 거부한다.

명시 token source가 필요하면 정확히 하나만 지정할 수 있다.

- `--token <token>`: 운영 편의를 위한 inline bearer token.
- `--token-file <path>`: plaintext token file.
- `--token-env <name>`: environment variable 조회.
- `--protected-token-file <path>`: Desktop Node host token store contract와 일치하는 DPAPI LocalMachine protected token file.

CLI는 inline token 값을 stdout/stderr에 쓰지 않는다. 반복 실행 script에서는 token source를 생략해 기본 protected token file을 사용하고, 별도 환경 검증이 필요할 때만 `--protected-token-file`, `--token-env`, `--token-file`을 override로 지정한다.

## 명령 범위

```text
pcvcli
pcvcli --interactive|-i
pcvcli host status
pcvcli runtime policy
pcvcli ops summary
pcvcli network inventory|list
pcvcli vm list|get|create|start|stop|shutdown|guest-shutdown|poweroff|restart|pause|resume|save|resume-saved|rename|console|vnc|memory-stats|cpu-stats|limit|blkio-get|blkio-set|bandwidth|bandwidth-set|guest-agent-status|guest-ping|guest-agent-ensure-channel|guest-exec|attach|eject|delete-status|set-memory|set-vcpu|disk-resize|manage|delete
pcvcli vm checkpoint list|create|restore|delete
pcvcli vm snapshot list|create|rollback|delete
pcvcli job list|get|cancel|retry|reconcile
pcvcli diagnostics bundle list [--limit N] [--offset N]
pcvcli diagnostics bundle create
pcvcli diagnostics bundle download <bundle_id> --output <path>
```

Linux `pcvctl` 호환 alias 중 Desktop Node backend가 가진 Hyper-V API에 대응되는 항목은 `network list`, `vm create <name> --vcpu --memory_mb --disk_size_gb --iso_path`, `vm stop`, `vm guest-shutdown`, `vm pause`, `vm resume`, `vm rename`, `vm vnc`, `vm memory-stats`, `vm cpu-stats`, `vm limit`, `vm blkio-get`, `vm blkio-set`, `vm bandwidth`, `vm bandwidth-set`, `vm guest-agent-status`, `vm guest-ping`, `vm eject`, `vm delete-status`, `vm set-memory`, `vm set-vcpu`, `vm disk-resize`, `vm snapshot list|create|rollback|delete`로 연결한다. 최상위 `snapshot list|create|rollback|delete` command group은 PCVCLI surface에서 제거했다. `vm manage`, `vm delete`와 QoS apply는 `--yes`를 요구한다. CLI는 KVM/libvirt/LXC/ZFS/OVN 같은 Linux `purecvisor-single` runtime object를 추가하지 않는다.

`vm eject/delete-status`와 `vm set-memory/set-vcpu/disk-resize`는 `0.42.38-admin-smoke` 개발 slice에서 Local API queued job/native Hyper-V adapter route로 승격했고, Windows Update clean-host rerun을 포함한 manual-admin package-pair closure까지 PASS했다. 어떤 경우에도 public trusted signing 또는 외부 stable publication claim으로 사용하지 않는다.

`vm limit`, `vm blkio-get`, `vm bandwidth`, `vm guest-agent-status`, `vm guest-ping`은
ADR-0007에서 Hyper-V semantics로 제한 승격했다. `vm limit`은 CPU/MEM resource
mutation alias이고, 나머지는 readback/readiness route다. Linux cgroup/libvirt/qemu
guest agent compatibility는 주장하지 않으며, 설치본 evidence는 후속
`0.42.39-admin-smoke` package, full admin host mutation, manual-admin package-pair,
installed Web/TUI/CLI current-card와
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`가
소유한다. 이 문단의 Web/TUI direct QoS control 미개방 설명은 `0.42.39-admin-smoke`
당시 historical predecessor snapshot이다. 이후 ADR-0008 closure에서 Web direct QoS
control이 열렸고, 현재 TUI는 ADR-0011에 따라 absent다.

Post-04245 확장 planning에서 ADR-0008은 `vm blkio-set`과 `vm bandwidth-set`을
Hyper-V QoS mutation으로 분리했다. 2026-05-26 slice에서 두 명령은
`--dry-run` preview와 `--yes` queued apply UX까지 구현됐고, `0.42.47-admin-smoke`
설치본 actual VM smoke와 full admin host mutation gate, `0.42.45-admin-smoke ->
0.42.47-admin-smoke` manual-admin package-pair closure에서 PASS했다. 2026-05-29
follow-up부터 음수, `1,000,000,000` 초과, `minimum > maximum` 값은 CLI/API에서
`PCV_VM_QOS_STORAGE_RANGE_INVALID` 또는 `PCV_VM_QOS_NETWORK_RANGE_INVALID`로 먼저
거절한다. Rollback/manual restore에 쓰는 `0`은 계속 유효하다.

ADR-0009 기준 `vm guest-exec`와 `vm guest-agent-ensure-channel`은
`0.42.53-admin-smoke`부터 보안 경계 contract 안에서 provider route와 direct-control surface를
지원한다. `vm guest-agent-ensure-channel <vm> --dry-run`은 channel preview route를 호출하고,
`--verify`/`--repair`는 protected credential reference와 confirmation guard를 거쳐 queued
provider job을 요청한다. `vm guest-exec <vm> --dry-run [--credential-ref REF] [--timeout-sec N] -- <command...>`는
redacted command/audit preview route를 호출하고, dry-run이 아니면 raw secret 없이 queued
guest execution을 요청한다. `0.42.54-admin-smoke` 설치본에서는 persistent Windows guest 대상
long-running `guest-exec` cancel smoke까지 PASS했고, running cancel token path는
provider cancellation token interrupt로 닫혔다. `0.42.55-admin-smoke` 설치본에서는 같은
persistent Windows guest 대상 actual credentialed guest-exec를 재확인했다.

`job reconcile <job_id>`는 `PCV_JOB_INTERRUPTED` 상태의 `vm.rename`, `vm.delete`,
`checkpoint.create`에만 적용한다. Provider postcondition이 확인되면 duplicate mutation 없이
기존 job을 reconciled terminal state로 저장하며, 모호한 결과에서는
`PCV_JOB_RECONCILIATION_REQUIRED`로 fail-close한다. `vm set-memory <vm> <memory_mb>`,
`vm set-vcpu <vm> <count>`, `vm disk-resize <vm> <disk_gb>`는 각각 MiB, processor count,
GiB 목표값을 받는 queued Hyper-V mutation이다. Disk resize는 확장만 지원한다. 정확한
실행 예제와 guest credential/argv 경계는 `docs/CLI_COMMAND_USAGE.md`를 따른다.

`GET /api/v1/diagnostics/bundles?limit=&offset=`은
`pcvcli diagnostics bundle list [--limit N] [--offset N]`으로 노출되어 create/list/download
흐름을 완성한다. 목록 조회 시 retention이 적용되어 만료됐거나 최대 개수를 넘은 bundle 파일이
diagnostics root에서 제거될 수 있다.

`GET /api/v1/console/capabilities`는 vmconnect/noVNC/`console.view` 상태를 설명하는
API/Web Console 전용 discovery card다. CLI에는 전역 console capabilities command가 없고,
실제 VM별 console handoff는 `pcvcli vm console|vnc <vm>`으로 제공한다. 응답 필드와 직접 API
호출 예제는 `docs/CLI_COMMAND_USAGE.md`의 해당 절을 따른다.
