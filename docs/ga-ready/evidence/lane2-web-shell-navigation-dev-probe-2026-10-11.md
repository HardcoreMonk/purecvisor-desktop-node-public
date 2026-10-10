# Lane 2 dev probe: Single Edge shell navigation with the BL-0019 fix (2026-10-11)

- 종류: Lane 2 dev probe(`docs/DEVELOPMENT_PROCEDURE.md` §10). campaign `shell-nav-view-sync-20261011` Task 2. 승격 근거가 아니고 `current-evidence.json`에 쓰지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 목적: train `0.42.95` 정차 원인 BL-0019(Single Edge 셸에서 사이드바·hash 화면 전환이 dashboard에 머묾)가 수정 build에서 사라지는지 같은 호스트의 설치본에서 확인한다.

## 사전 조건

- 설치본 `0.42.95-admin-smoke+b9898cf`(train 0.42.95 fullgate build), service Running/Automatic, Web 200, ARP `0.42.95` 1개. 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`는 Off.

## dev build

- campaign Task 1 HEAD `179bd198362abbf820b06b10852f552a657cf8fb`에서 `build.ps1 -Version 0.42.96-admin-smoke`(`AllowUnsignedDev`/`LocalTest`), `New-PcvAdminSmokeUpdatePackage.ps1`. MSI SHA-256 `3ce658544dfcf7aa05d382860cec6168532bfbb8c97b581b2d60eddc4f6b849b`, payload 32, update ZIP SHA-256 `e4bd87519a753895c78084a762ce2b67d66b14276f6c1ec43078e6f4fab05b63`, catalog SHA-256 `8f2c4fb65948d219549b3797b2b82e3bf87e73a4120859a3b70f88913053b291`. root `artifacts/navfix-probe-20261011-04296`(git 밖).
- version 문자열은 다음 train과 같지만 제품 updater는 내려받은 package를 ZIP SHA-256 이름(`updates/packages/update-<sha>.zip`, `updates/payloads/<sha>`)으로 두므로 train package와 섞이지 않는다. 끝에 Rollback으로 `0.42.95`에 돌려 놓았다.

## 실행

| 단계 | 결과 |
| --- | --- |
| 제품 Update(catalog `file:` URI, channel `admin-smoke`) | `ok=true`. steps: current-manifest, update-catalog-preflight, update-source-preflight, update-payload-preflight, update-transaction.begin, service.stop, backup-product-root, copy, config-migration, service.start, health. 설치본 `0.42.96-admin-smoke+179bd19`, manifest `0.42.96-admin-smoke`, service Running/Automatic, Web 200, ARP는 `0.42.95` 1개 그대로(ZIP update는 MSI를 건드리지 않음). 설치된 `web/app.bundle.js`에 `PCV.nav.activeView`, `web/index.html`에 `helppage` section 존재 |
| 사이드바 8 화면(Playwright Chromium, 1280×800) | 운영 대시보드·네트워크·작업·이벤트 센터·증적·진단과 계정·도움말은 클릭마다 그 section만 보이고 `PCV.nav.activeView()`와 옛 `state.activeView`가 따라왔다(도움말은 `helppage`, 옛 상태는 직전 view 유지). 가상 머신 클릭은 첫 회차에 새 service worker가 제어를 넘겨받으며 설계된 1회 `controllerchange` reload와 겹쳐 dashboard로 읽혔고, reload 뒤 운영 대시보드·작업·도움말에서 각각 가상 머신을 눌러 3회 모두 `vms`(VM 2행)가 보였다(그 사이 page navigation 0회) |
| 도움말 뒤 진단 화면 | troubleshooting section 표시, `-panel` 7개, help banner 없음(도움말이 진단 패널을 지우지 않음) |
| hash `#/vms` | `vms` 표시 |
| polling 35초 뒤 | `vms` 유지(옛 재렌더가 dashboard로 되돌리지 않음) |
| service worker | `activated`, bundle `PCV_UI_SOURCE_SHA1` `15e0c8ef` |
| 콘솔 오류 | 첫 로드에서 loopback 세션 전 `GET /runtime/policy` 401 1건(세션 뒤 회복), 이 브라우저 localStorage의 옛 tracked job id `GET /jobs/<id>` 404 1건. 제품 결함 아님 |
| 제품 Rollback | `ok=true`. steps: service.stop, restore, service.start, health. 설치본 `0.42.95-admin-smoke+b9898cf`, manifest `0.42.95-admin-smoke`, service Running/Automatic, Web 200, ARP `{F5203439-7074-4F2C-8F20-0D9571420514}` `0.42.95` 1개, 보존 VM과 `pcv-it-s2-source` Off |

## 판정: PASS

- 수정 build에서 Single Edge 셸의 화면 전환이 사이드바·hash·polling 재렌더 모두에서 유지된다. 정차 때(같은 호스트, 수정 전 build)는 사이드바 클릭과 `#/vms`가 dashboard에 머물렀다.
- 캡처(`.playwright-mcp/navfix-probe-20261011/`, 저장소 밖)는 VM 화면, 도움말 화면, 도움말 뒤 진단 화면이다.
- 이 문서는 손으로 썼다. 제품 Update/Rollback 결과는 `artifacts/navfix-probe-20261011-04296/product-update.json`, `product-rollback.json`(git 밖)이다.
