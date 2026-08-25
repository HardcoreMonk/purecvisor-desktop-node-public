# PCVCLI interactive shell evidence

status: pass
date: 2026-05-18
target_cli: src/DesktopNode.Cli/pcvcli

## 목적

`pcvcli`를 인자 없이 실행했을 때 Linux `pcvctl` 운영 경험과 유사한 banner,
interactive prompt, `help`, `exit`/`quit`, Tab completion 기반 REPL을 제공한다.

## 구현 범위

- `pcvcli`
- `pcvcli --interactive`
- `pcvcli -i`

위 진입점은 모두 같은 interactive shell로 들어간다. Shell command는 `help`, `?`,
`exit`, `quit`이다. 그 외 입력은 one-shot `pcvcli`와 같은 parser, token resolver,
Local API transport를 사용한다.

## Operator UX

시작 시 PureCVisor banner와 다음 안내를 출력한다.

```text
Type 'help' for commands | 'exit' to quit | Tab to complete

(pcv) >
```

`help`는 banner, global flags, Desktop Node Hyper-V command table을 출력한다. Tab
completion은 command prefix 기반으로 동작한다.

## 검증

- `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliInteractiveShellTests"`: 7 passed
- `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliOptionsTests.ShortVerboseSelectsVerboseMode"`: 1 passed
- `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore`: 63 passed
- `@('help','exit') | dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj --no-restore --`: exit code 0
- `@('help','exit') | dotnet run --project src\DesktopNode.Cli\DesktopNode.Cli.csproj --no-restore -- --interactive --no-color`: exit code 0
