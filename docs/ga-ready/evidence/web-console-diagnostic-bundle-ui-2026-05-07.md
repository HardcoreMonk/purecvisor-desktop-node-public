# Web Console Diagnostic Bundle UI Evidence - 2026-05-07

```text
evidence_id: web-console-diagnostic-bundle-ui-2026-05-07
artifact_or_package_version: web/index.html, web/src/served-app.ts, web/app.js, web/scripts/verify-browser-fixture.mjs
```

## 요약

2026-05-07 후속 Web Dashboard slice는 `Troubleshooting` 화면에 Diagnostic Bundle operator handoff UI를 추가했다.

- `Troubleshooting` view에 `diagnostics-panel` mount point를 추가했다.
- Web Console은 `%ProgramData%\PureCVisor\desktop-node\diagnostics` 출력 root와 기존 product wrapper `CollectDiagnostics` action을 표시한다.
- UI는 `operator handoff`, `no host mutation`, `read-only UI`, token value/Authorization header redaction boundary를 같이 표시한다.
- Browser fixture는 `Diagnostic Bundle`, `CollectDiagnostics`, `operator handoff`, `no host mutation`, `token values`, `Authorization headers`, diagnostics root가 실제 rendered output에 포함되는지 확인한다.
- 이 slice는 Event Log source registration, firewall, trust-store, LAN, service/MSI lifecycle, reboot, Task Scheduler, Hyper-V mutation, update/rollback mutation을 실행하지 않는다.

## 범위

이 evidence는 Web Console beta 기능의 read-only/static fixture coverage다. 실제 diagnostic bundle 생성은 기존 product wrapper `Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics`가 소유한다.

다음 항목은 이 evidence에서 제외한다.

- Web API를 통한 diagnostic bundle 생성/download action
- elevated product wrapper 실행 대행
- Event Log source, firewall, trust-store, LAN, MSI/service mutation
- Hyper-V VM 생성/삭제
- installed update/rollback mutation
- public trusted signing 또는 외부 stable publication

## 검증

TDD guard:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

결과: PASS, 28 tests. RED 단계에서는 `id="diagnostics-panel"` 누락으로 실패했고, 구현 후 같은 static guard가 통과했다.

최종 Web Console 검증:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

결과:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"`: PASS, 28 tests
- `npm test --prefix web`: PASS, served `app.js` current
- `npm run verify:parity --prefix web`: PASS, static parity manifest current, browser fixture passed
- `npm run browser:fixture --prefix web`: PASS
- `node --check web/app.js`: PASS
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`: PASS, 28 tests
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"`: PASS
- `git diff --check`: PASS

이 evidence는 read-only Web Dashboard evidence이며 public release/public trusted signing/외부 stable publication evidence가 아니다.
