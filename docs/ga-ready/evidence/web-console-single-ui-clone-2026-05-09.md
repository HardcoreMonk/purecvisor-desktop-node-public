# Web Console Single UI Clone Evidence - 2026-05-09

```text
evidence_id: web-console-single-ui-clone-2026-05-09
artifact_or_package_version: web/index.html, web/styles.css, web/src/served-app.ts, web/src/served/*.ts, web/app.js, web/scripts/build-served-asset.mjs, web/scripts/verify-static-parity.mjs, web/scripts/verify-browser-fixture.mjs, web/tests/PcvDesktopWeb.Static.Tests.ps1
screenshot_root: output/playwright/single-console-clone-20260509
```

## 요약

2026-05-09 후속 Web Console slice는 `purecvisor-single`의 전체 운영 콘솔 UI/UX 프레임을 Desktop Node active Web Console에 맞춰 클론했다.

- 상단 application menu, session/header toolbar, theme/language/API/token control strip을 추가했다.
- 좌측 icon activity rail, VM asset explorer, workspace tabbar, bottom status bar를 추가했다.
- Dashboard 첫 화면에 운영 hero, operator memo, displayed item pills, quick action grid를 추가했다.
- 기존 Desktop Node route registry, optional bearer token UX, `Dashboard`/`VM Workbench`/`Network`/`Jobs`/`Activity`/`Evidence`/`Troubleshooting` view contract는 유지했다.
- API error detail 객체가 `[object Object]`로 표시되지 않도록 `formatErrorDetail` normalization을 추가했다.
- 5개 staged frontend batch로 service core split, partial refresh degradation, scoped VM/checkpoint pending state, job polling backoff/next-page loading, Single shell control binding을 적용했다.
- `web/src/served/*.ts` staged source parts를 `web/app.js`로 결합하는 served asset build pipeline을 추가했다.
- VM asset explorer, workspace tabbar, activity rail, menu command, quick actions가 Desktop Node view/VM state에 연결된다.
- Static guard는 Single console workbench frame marker가 존재하고, Desktop Node Web Console이 Linux 기반 service/runtime surface를 포함하지 않음을 확인한다.

## 범위

이 evidence는 Web Console static/read-only UI/UX evidence다. Linux 기반 service/runtime code, Single Edge active route/auth/session/websocket flow, host mutation, installer mutation은 실행하거나 도입하지 않는다.

다음 항목은 이 evidence에서 제외한다.

- Linux service/runtime adapter 또는 active route 추가
- Single Edge `/ui/` base href, token login/refresh, websocket event flow
- Desktop Node에 없는 container/storage/network runtime 화면 추가
- Hyper-V VM 생성/삭제 또는 checkpoint mutation 실행
- service/MSI install/repair/uninstall
- firewall/trust-store/LAN/Event Log/update mutation
- public trusted signing 또는 외부 stable publication

## 검증

TDD guard:

```powershell
Invoke-Pester -Path web\tests\PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
```

RED 단계에서는 `class="menu-bar"` 등 Single console workbench frame marker가 없어 실패했다. 구현 후 같은 static guard가 통과했다.

최종 Web Console 검증:

```powershell
Invoke-Pester -Path web\tests\PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm test --prefix web
npm run verify:parity --prefix web
node --check web\app.js
git diff --check
```

결과:

- `Invoke-Pester -Path web\tests\PcvDesktopWeb.Static.Tests.ps1 -Output Detailed`: PASS, 40 tests after final frontend edge-case hardening.
- `npm test --prefix web`: PASS, served `app.js` current.
- `npm run verify:parity --prefix web`: PASS, static parity manifest current, browser fixture passed.
- `node --check web\app.js`: PASS.
- `git diff --check`: PASS. Existing LF/CRLF working-copy warnings were informational only.

Browser render evidence:

- `output/playwright/single-console-clone-20260509/desktop-final.png`
- `output/playwright/single-console-clone-20260509/mobile-final.png`

Active Web Console source scan:

```powershell
rg -n "컨테이너|LXC|ZFS|OVN|OVS|KVM|libvirt|purecvisorsd|/containers|/storage|/ovn|/networks|/auth/token|/auth/refresh|/ws/events" web\index.html web\styles.css web\src\served-app.ts web\src\served web\app.js
```

결과: no matches in active Web Console source files.

이 evidence는 read-only/static Web Console UI evidence이며 public release/public trusted signing/외부 stable publication evidence가 아니다.
