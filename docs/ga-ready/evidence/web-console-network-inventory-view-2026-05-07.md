# Web Console Network Inventory View Evidence - 2026-05-07

```text
evidence_id: web-console-network-inventory-view-2026-05-07
artifact_or_package_version: web/index.html, web/src/served-app.ts, web/app.js, web/generated/parity/static-asset-parity.manifest.json
```

## 요약

2026-05-07 후속 Web Dashboard slice는 `GET /api/v1/network/inventory` read-only route를 Web Console의 `Network` 화면으로 승격했다.

- Sidebar에 `Network` view를 추가했다.
- `Network Inventory` 화면은 source, mutation mode, switch count, default switch count를 요약하고 switch name/type/default/management OS/external adapter field를 table로 표시한다.
- `mutation` 표시는 API data의 `mutating=true`가 아닌 이상 `read-only`로 고정된다.
- TypeScript route contract, user-visible fixture, browser fixture, generated parity manifest에 `/api/v1/network/inventory`와 `Default Switch`/`fixture-ethernet` fixture evidence를 추가했다.
- 이 slice는 Hyper-V switch 생성/삭제, IP/firewall 변경, service/MSI/trust-store/LAN/update mutation을 실행하지 않는다.

## 범위

이 evidence는 Web Console beta 기능의 read-only/static fixture coverage다. Native API route 자체의 installed evidence는 기존 `network.inventory` C# native adapter 및 Service/MSI/Hyper-V smoke evidence가 소유한다.

다음 항목은 이 evidence에서 제외한다.

- `New-VMSwitch`, `Remove-VMSwitch`, `New-NetIPAddress`, firewall rule mutation
- Hyper-V VM 생성/삭제
- service/MSI install/repair/uninstall
- trust-store/LAN/Event Log mutation
- installed update/rollback mutation
- public trusted signing 또는 외부 stable publication

## 검증

TDD guard:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

결과: PASS, 27 tests. RED 단계에서는 `data-view-link="network"` 누락으로 실패했고, 구현 후 같은 static guard가 통과했다.

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

- `npm test --prefix web`: PASS, served `app.js` current
- `npm run verify:parity --prefix web`: PASS, static parity manifest current, browser fixture passed
- `npm run browser:fixture --prefix web`: PASS
- `node --check web/app.js`: PASS
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`: PASS, 27 tests
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"`: PASS, 18 tests
- `git diff --check`: PASS

이 evidence는 read-only Web Dashboard evidence이며 public release/public trusted signing/외부 stable publication evidence가 아니다.
