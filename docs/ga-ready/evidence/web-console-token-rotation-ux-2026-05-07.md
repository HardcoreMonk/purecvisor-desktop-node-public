# Web Console Token Rotation UX Evidence - 2026-05-07

```text
evidence_id: web-console-token-rotation-ux-2026-05-07
artifact_or_package_version: web/index.html, web/src/served-app.ts, web/app.js, web/scripts/verify-browser-fixture.mjs
```

## 요약

2026-05-07 후속 Web Dashboard slice는 `Troubleshooting` 화면에 Token Rotation operator UX를 추가했다.

- `Troubleshooting` view에 `token-rotation-panel` mount point를 추가했다.
- Web Console은 protected token file root `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`, current token storage, listener exposure, browser token presence 상태를 표시한다.
- Panel의 `Clear browser token` button은 브라우저 입력/세션 token만 지우고 다시 API 상태를 확인한다.
- UI는 `rotation handoff`, `no service token mutation`, `no host mutation`, token value/Authorization header redaction boundary를 표시한다.
- Browser fixture는 `Token Rotation`, `rotation handoff`, `Clear browser token`, `no service token mutation`, protected token file root, browser token empty 상태가 rendered output에 포함되는지 확인한다.

## 범위

이 evidence는 Web Console beta 기능의 read-only/operator handoff coverage다. 실제 service token file 생성, 교체, revoke, service restart, MSI repair, credential store migration은 이 slice에서 실행하지 않는다.

다음 항목은 이 evidence에서 제외한다.

- Web API를 통한 protected token file rotation/revoke mutation
- service stop/start 또는 repair-installed를 통한 token replacement automation
- Windows Credential Manager transition
- LAN/firewall/trust-store/MSI/service/update mutation
- public trusted signing 또는 외부 stable publication

## 검증

TDD guard:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

결과: PASS, 29 tests. RED 단계에서는 `id="token-rotation-panel"` 누락으로 실패했고, 구현 후 같은 static guard가 통과했다.

최종 Web Console 검증:

```powershell
npm run build:served --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
npm run browser:fixture --prefix web
node --check web/app.js
npm test --prefix web
```

결과:

- `npm run build:served --prefix web`: PASS, served `app.js` regenerated.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"`: PASS, 29 tests.
- `npm run browser:fixture --prefix web`: PASS.
- `node --check web/app.js`: PASS.
- `npm test --prefix web`: PASS, served `app.js` current.

이 evidence는 read-only Web Dashboard/token UX evidence이며 public release/public trusted signing/외부 stable publication evidence가 아니다.
