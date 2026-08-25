# PureCVisor Desktop Node Product TUI Service Design

작성 기준: 2026-05-10

## 목적

Windows Desktop Node 제품에 설치되는 TUI 표면을 추가한다. 이 설계에서 "TUI
service"는 Windows SCM service를 의미하지 않는다. `pcvtui.exe`는 설치되는
interactive product client이며, 기존 `DesktopNode.Host.exe` Local API service에
loopback HTTP로 연결한다.

차용 자료는 `D:\data\projects\codex-zone\purecvisor-single\src\tui\**`다. 가져오는
범위는 Linux TUI의 화면 구조, 탭 리듬, polling 모델, widget 개념이다. C/ncurses
코드, UDS JSON-RPC transport, `purecvisorsd`, libvirt/KVM/LXC/ZFS/OVS/OVN runtime
기능은 Desktop Node active product path에 추가하지 않는다.

## 현재 경계

Desktop Node는 Windows Desktop Node 전용 저장소다. Active runtime은
`DesktopNode.Host.exe` Local API, C# native adapter, TypeScript Web Console,
`pcvcli.exe` .NET CLI, Windows packaging wrapper가 소유한다.

이 TUI는 다음 경계를 유지한다.

- Windows/Hyper-V Desktop Node Local API만 호출한다.
- Linux runtime module 또는 Linux service wording을 제품 표면에 노출하지 않는다.
- Token value, `Authorization` header, raw bearer token은 어떤 화면이나 오류에도
  표시하지 않는다.
- Public trusted signing 또는 external stable publication evidence를 주장하지 않는다.
- VM/delete, job/cancel/retry, diagnostics/create/download 같은 mutation은 명시
  keyboard action과 confirm popup이 있어야 실행된다.

## 접근 방식

새 .NET console project `src/DesktopNode.Tui`를 추가하고 assembly name은
`pcvtui`로 둔다. 기존 `src/DesktopNode.Cli`와 같은 Local API client pattern,
token source contract, injectable transport test pattern을 공유하거나 평행하게
구현한다.

권장 project shape:

```text
src/DesktopNode.Tui/
  DesktopNode.Tui.csproj
  Program.cs
  TuiApplication.cs
  TuiOptions.cs
  TuiTokenResolver.cs
  TuiApiClient.cs
  TuiTransport.cs
  TuiPoller.cs
  TuiState.cs
  TuiRenderer.cs
  TuiWidgets.cs
  TuiTabs.cs
src/DesktopNode.Tui.Tests/
  DesktopNode.Tui.Tests.csproj
  TuiApplicationTests.cs
  TuiStateTests.cs
  TuiRendererTests.cs
  TuiPackagingContractTests.cs
```

`pcvtui.exe`는 `pcvcli.exe`와 동일한 token source를 받는다.

```text
--token
--token-file
--token-env
--protected-token-file
--api
--refresh-interval
--no-color
```

`--api` 기본값은 `http://127.0.0.1:7777`이다.

## UI 구조

Linux TUI의 F-key tab rhythm과 panel layout을 Desktop Node 언어로 치환한다.

```text
F1 VM | F2 NET | F3 JOB | F4 DIAG | F5 HOST | F6 RUNTIME
```

공통 화면 구조:

- Header: product name, API endpoint, service reachability, selected token source,
  last refresh time, refresh cadence.
- Top status panel: host/service/API health, VM count, active job count, degraded
  route count.
- Main panel: selected tab table 또는 detail surface.
- Inspector panel: selected row detail, allowed actions, last route error.
- Footer: key hints, refresh status, redaction reminder.
- Help overlay: tab별 key binding과 mutation boundary.

Tab mapping:

| Tab | Route source | UI content | Mutation |
| --- | --- | --- | --- |
| `VM` | `GET /api/v1/vms`, `GET /api/v1/vms/{id}` | VM table, state, cpu/memory/generation/checkpoint count, console availability | start, shutdown, poweroff, restart, delete with confirm |
| `NET` | `GET /api/v1/network/inventory` | Hyper-V switch inventory, default/management OS/external adapter fields | none |
| `JOB` | `GET /api/v1/jobs?limit=50&offset=0`, `GET /api/v1/jobs/{id}` | active and retained terminal jobs, pagination summary | cancel, retry with confirm |
| `DIAG` | `GET /api/v1/diagnostics/bundles?limit=10&offset=0` | bundle list, retention, latest bundle, download route | create, download latest with confirm |
| `HOST` | `GET /api/v1/host/status`, `GET /api/v1/ops/summary` | admin/service/runtime status, evidence summary | none |
| `RUNTIME` | `GET /api/v1/runtime/policy` | dispatch policy, token storage, rate-limit, timeout, diagnostics root | none |

Base key bindings:

| Key | Action |
| --- | --- |
| `F1`..`F6` | switch tab |
| `Up`/`Down` | move selection |
| `PageUp`/`PageDown` | scroll page |
| `/` | filter current table |
| `R` | force refresh |
| `h` | help overlay |
| `Esc` | close popup or clear filter |
| `q` | quit |

Mutation key bindings are tab-local and shown only when available. Delete and
destructive actions require confirm text or an explicit yes popup.

## Data flow

`pcvtui.exe` starts by parsing options and resolving one token source. Token source
conflicts fail before any network call. The token value is retained only in process
memory and is never written to stdout, stderr, screen snapshots, logs, or exception
messages.

The application keeps two loops:

1. Input/render loop reads keyboard events and renders the current `TuiState`.
2. Polling loop fetches tab snapshots and swaps them into `TuiState`.

Polling cadence defaults to three seconds. Route refreshes are isolated; a failure in
one route marks only that panel degraded. The renderer keeps showing the last
successful snapshot with its timestamp.

Mutation flow:

1. User selects an entity.
2. TUI verifies local preconditions, such as running VM delete block.
3. TUI displays confirm popup with stable entity id and action.
4. TUI sends the Local API mutation request.
5. TUI displays job id or API problem details.
6. Poller refreshes affected tab.

## Packaging flow

`pcvtui.exe` is a product payload peer of `DesktopNode.Host.exe` and `pcvcli.exe`.

Installer build changes:

- Add `-DesktopNodeTuiPath` to `packaging/windows-desktop-node/installer/build.ps1`.
- If the path is absent, publish `src/DesktopNode.Tui/DesktopNode.Tui.csproj`.
- Stage `pcvtui.exe` into the MSI payload root.
- Record TUI source path, SHA-256, command name, and mode in installer provenance.
- Return structured errors for missing or failed TUI publish.

WiX changes:

- Add `DesktopNodeTuiComponent` to `Product.wxs`.
- Install `pcvtui.exe` under the product root.

Product wrapper changes:

- Add `tui_exe_name = 'pcvtui.exe'` to product defaults.
- Add `paths.tui_exe` to `Resolve-PcvDesktopNodeProductPaths`.
- Add `tui` metadata to `product-manifest.json`.
- Include `pcvtui.exe` in runtime payload copy and validation.
- Block update before service stop when payload is missing `pcvtui.exe`.

Manifest metadata:

```json
{
  "tui": {
    "mode": "dotnet-local-api-interactive-client",
    "command_name": "pcvtui",
    "executable_path": "C:\\Program Files\\PureCVisor\\DesktopNode\\pcvtui.exe",
    "default_owner": "desktop-node-product-tui",
    "ui_structure_source": "purecvisor-single-tui-structure-port",
    "runtime_boundary": "windows-desktop-node-local-api-only",
    "token_sources": [
      "--token",
      "--token-file",
      "--token-env",
      "--protected-token-file"
    ]
  }
}
```

## Error handling

Messages use a short operator sentence, a stable error code, and a next action. They
stay in English to align with API, CLI, Web Console, and test contracts.

Local TUI blocks:

```text
Action blocked: VM is running.
Power off the VM first, then retry Delete VM.
code=PCV_TUI_VM_DELETE_RUNNING
```

```text
Action blocked: no job is selected.
Select a job, then retry Cancel.
code=PCV_TUI_JOB_SELECTION_REQUIRED
```

```text
Action blocked: diagnostic bundle id is empty.
Create or select a bundle before download.
code=PCV_TUI_DIAGNOSTIC_BUNDLE_REQUIRED
```

Auth failures:

```text
Authentication required.
Start pcvtui with --token, --token-file, --token-env, or --protected-token-file.
code=PCV_TUI_AUTH_REQUIRED
```

```text
Authentication failed.
Check the selected token source and retry.
code=PCV_TUI_AUTH_FAILED
```

Route failures:

```text
Request failed: route timed out.
Showing last successful VM snapshot from 14:03:22.
code=PCV_ROUTE_TIMEOUT retry_after=15s
```

```text
Request limited: too many Local API requests.
Refresh paused until Retry-After expires.
code=PCV_RATE_LIMIT_EXCEEDED retry_after=15s
```

Diagnostic failures:

```text
Diagnostic bundle download failed.
Bundle pcv-diag-20260509T010101Z-abcdef12 was not found.
code=PCV_DIAGNOSTIC_BUNDLE_NOT_FOUND
```

Packaging gates:

```text
Installer build blocked.
DesktopNode.Tui payload was not found.
code=PCV_INSTALLER_TUI_NOT_FOUND
```

```text
Installer build blocked.
DesktopNode.Tui publish failed.
code=PCV_INSTALLER_TUI_PUBLISH_FAILED
```

```text
Update blocked before service stop.
Payload is missing pcvtui.exe.
code=PCV_PRODUCT_TUI_MISSING
```

Redaction rules:

- Never render token values.
- Never render `Authorization` header values.
- Redact token-like fields from exception details.
- Show token source names and safe file paths only when useful.

## Testing

Implementation must follow TDD.

Focused .NET tests:

- `TuiOptionsTests`: option parsing, token source conflicts, API default.
- `TuiApiClientTests`: route mapping for each tab.
- `TuiStateTests`: tab switch, filter, selection, last-success snapshot retention.
- `TuiApplicationTests`: key dispatch and confirm flow.
- `TuiRendererTests`: no token leakage, degraded panel rendering, help overlay content.
- `TuiErrorContractTests`: 401, 403, 429, 504, problem-details, retry-after display.

Packaging tests:

- Installer build plan exposes `-DesktopNodeTuiPath`.
- Missing explicit TUI path returns `PCV_INSTALLER_TUI_NOT_FOUND`.
- Failed publish returns `PCV_INSTALLER_TUI_PUBLISH_FAILED`.
- Dry-run plan records `tui_source`, `tui_path`, and `tui_sha256`.
- Payload root contains `pcvtui.exe`.
- WiX source includes `DesktopNodeTuiComponent`.
- Product manifest records `paths.tui_exe` and `tui`.
- Runtime payload copy includes `pcvtui.exe`.
- Update blocks before service stop when payload is missing `pcvtui.exe`.

Verification commands:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore
dotnet test src\DesktopNode.sln --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests','packaging/windows-desktop-node/installer/tests' -Output Detailed"
git diff --check
```

## Documentation updates

Implementation should update:

- `docs/DEVELOPER_INDEX.md`
- `docs/USER_GUIDE.md`
- `docs/OPERATIONS_GUIDE.md`
- `packaging/windows-desktop-node/README.md`
- `src/DesktopNode.Cli/README.md` or a new `src/DesktopNode.Tui/README.md`

Documentation must describe `pcvtui.exe` as a Windows Desktop Node Local API TUI
client, not as a Linux TUI runtime or Windows SCM service.

## Out of scope

- Porting C/ncurses source code.
- Adding UDS JSON-RPC support.
- Adding Linux KVM/libvirt/LXC/ZFS/OVS/OVN features.
- Replacing Web Console.
- Running installed host mutation as part of the code-level implementation.
- Claiming public trusted signing or external stable publication.

## Open decisions resolved in design

- Product command name: `pcvtui.exe`.
- Tab names: `VM`, `NET`, `JOB`, `DIAG`, `HOST`, `RUNTIME`.
- UI structure source: Linux TUI structure and interaction rhythm only.
- Runtime boundary: Windows Desktop Node Local API only.
- Packaging stance: installed product payload and update validation peer of `pcvcli.exe`.
