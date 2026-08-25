# PureCVisor Desktop Node CLI Spike

이 디렉터리는 Desktop Node Phase 4 CLI MVP, Phase 10 token file UX, Phase 15 protected token file UX, Phase 24 runtime policy consumer를 검증한다. CLI는 새 Hyper-V backend를 직접 호출하지 않고, Local API를 호출하는 thin client로 동작한다. 기본 사용은 loopback Local API이며, Phase 5 LAN mode listener를 사용할 때는 `--api`와 `--token`, `--token-file`, 또는 `--protected-token-file`을 명시한다.

Phase 19 기준 Desktop Node는 제품 런타임으로 승격하지 않고 `archive/spikes/purecvisor-desktop-node/**` 격리 spike로 유지한다. root 결정은 `archive/spikes/purecvisor-desktop-node/README.md`와 `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`를 따른다.

## 범위

- `host status`
- `runtime policy`
- `vm list`
- `vm get <vm>`
- `vm create`
- `vm start`, `vm stop`, `vm shutdown`, `vm poweroff`, `vm restart`
- `vm checkpoint list/create/restore/delete`
- `job get/cancel/retry`
- `--json` 원본 API 응답 출력
- `--api` Local API base URL 지정
- `--token` bearer token 전달
- `--token-file` bearer token file 전달
- `--protected-token-file` DPAPI LocalMachine protected token file 전달

Windows 서비스 설치, 토큰 생성/저장, shell completion, interactive prompt, VMConnect 실행은 CLI 범위가 아니다. LAN binding 자체는 Phase 5/15 Local API에서 `-AllowLan`과 `-ApiToken`, `-ApiTokenFile`, 또는 `-ApiTokenProtectedFile`로만 켠다.

## 사용 예

Local API listener를 먼저 실행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1 -Prefix 'http://127.0.0.1:7777/'
```

CLI 예시:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json host status
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json runtime policy
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm list
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm get ubuntu-lab-01
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm start ubuntu-lab-01
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm stop ubuntu-lab-01
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm checkpoint list ubuntu-lab-01
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm checkpoint create ubuntu-lab-01 --name before-upgrade
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm checkpoint restore ubuntu-lab-01 before-upgrade
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm checkpoint delete ubuntu-lab-01 before-upgrade
```

VM 생성 예시:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --json vm create `
  --name ubuntu-lab-01 `
  --iso 'D:\isos\ubuntu-24.04-live-server-amd64.iso' `
  --cpu 2 `
  --memory-mb 4096 `
  --disk-gb 40 `
  --generation 2
```

토큰이 켜진 Local API listener를 호출할 때:

`--token`은 짧은 개발자/manual smoke 전용이다. 장기 token이나 LAN/API 운영 호출은 shell history와 process argument 노출을 피하기 위해 `--protected-token-file`을 우선한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --api 'http://127.0.0.1:7777' --token 'change-me' --json host status
```

token file을 사용할 때:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --api 'http://127.0.0.1:7777' --token-file 'D:\PureCVisor\desktop-node\api-token.txt' --json host status
```

protected token file을 사용할 때:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --api 'http://127.0.0.1:7777' --protected-token-file 'D:\PureCVisor\desktop-node\api-token.dpapi.json' --json host status
```

Phase 5 LAN mode listener를 호출할 때도 같은 방식으로 명시적 API URL과 token을 전달한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --api 'http://[redacted-private-endpoint]:7777' --protected-token-file 'D:\PureCVisor\desktop-node\api-token.dpapi.json' --json host status
```

`--token`, `--token-file`, `--protected-token-file`은 동시에 사용할 수 없다. Plain token file은 실행 시 읽고 trailing newline을 제거한다. Missing file 또는 empty file은 요청을 보내기 전에 exit code `2`로 실패한다. Protected token file은 service module의 `Read-PcvDesktopServiceProtectedTokenFile`로 읽는다.

## 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
```

현재 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.
