# ADR-0018: Single Edge 프론트엔드 구조 차용

상태: 채택
일자: 2026-10-10 (제안·채택 같은 날, 채택은 사용자 승인 `1,2,3,4`의 1·2)

## 결정 마커

```text
DESKTOP_NODE_WEB_FRONTEND_STRUCTURE: single-edge-ui-v1
DESKTOP_NODE_WEB_SINGLE_EDGE_RUNTIME_SURFACES: excluded
DESKTOP_NODE_WEB_VENDOR_ASSETS: pretendard-coolicons-chartjs
DESKTOP_NODE_WEB_SOURCE_PARTS: web/src/modules
```

## 맥락

- 사용자가 2026-10-10에 자신의 공개 저장소 `https://github.com/HardcoreMonk/purecvisor`(Single Edge, Apache-2.0)의
  프론트엔드 구조를 그대로 차용하라고 요청했다. 분석(`docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`)
  결과 Single Edge `ui/`와 Desktop Node `web/`은 같은 빌드 모델(순서표 concat → classic script 하나)이고 시각 셸과
  Supanova 토큰은 이미 이식돼 있었다. 빠진 것은 코드 구조(`window.PCV` 네임스페이스 모듈 배치), 로그인 페이지, i18n, 테마
  엔진, modal, mobile, PWA, vendor 자산, 문서 포털, lint ratchet이다.
- 기존 경계 문서는 "Single Edge 표면을 추가하지 않는다"(`docs/CODING_GUIDE.md` 3.2)와 "Single Edge Web UI/API 공개 표면"
  제외(`docs/PUBLIC_RELEASE_BOUNDARY.md`)를 적고 있어, 구조·공통 모듈 코드 차용이 허용되는지 명시가 필요했다.
- S3 시연(2026-10-10)에서 Web Console 폼 click 결함(BL-0016)이 드러났고, 공통 이벤트 위임 규칙을 구조 수준에서 정할 필요가
  있다.

## 결정

- Desktop Node Web Console은 Single Edge `ui/`의 **프론트엔드 구조**를 채택한다: 파일 배치(`index.html` 로그인 페이지 + 앱 셸,
  `web/src/app.ts` 부트스트랩, `web/src/modules/<도메인>.ts`, `i18n.js`, `style.css`, `sw.js`/`manifest.json`/`offline.html`,
  `vendor/`, `samples/`, `docs.html`/`guide.html`), `window.PCV` IIFE 모듈 규칙, 모듈 순서표 빌드, eslint + domsafe ratchet.
- Single Edge **공통 모듈 코드**(shell, nav, theme, modal-core, modal, ui, uxlib, mobile, filter-state, charts, metrics, help,
  i18n 구조, api 구조, endpoints 형식)는 원문을 가져온다. 가져온 파일은 머리에 출처 주석
  `Ported from purecvisor ui/<path> (Apache-2.0, same author)`를 적는다.
- **제외**: Linux Single Edge runtime·route·화면(container, vpc, storage/ZFS, OVS/OVN network, selfhealing, totp, push,
  security의 Linux 부분, advanced, maintenance, cluster/federation). `web/DESIGN.md`의 `design-boundary` 계약(Linux route
  금지)과 `single-edge-isolation` 계약(Single Edge 트리 경로 import 금지)은 유지한다. 복사된 파일은 `web/` 안에 있으므로
  경로 import가 아니다.
- vendor 자산은 Pretendard(SIL OFL 1.1), Coolicons 1.2.2 subset(CC BY 4.0), Chart.js 4.4.4(MIT)만 가져오고
  `THIRD_PARTY_NOTICES.md`에 고지한다. qrcode와 noVNC는 가져오지 않는다.
- Local API route 계약, loopback 기본값(ADR-0006 내부 사설망 경계), WebSocket 미사용은 그대로다. 이벤트는 polling이다.
- 경계 문서는 "Linux Single Edge runtime·route·화면은 추가하지 않고, 프론트엔드 구조·공통 모듈은 ADR-0018로 차용한다"로
  좁힌다.

## 결과

- campaign `single-edge-frontend-structure-20261010`(Lane 1, 18 task)이 이 결정을 구현한다. 설치본 반영은 그 뒤 train
  0.42.94(BL-0016 수정과 새 프론트엔드)이고, 그 설치본에서 S3 브라우저 예약 저장 재시연과 S4 campaign을 한다.
- Host 정적 서빙은 폰트·SVG·manifest·하위 디렉터리를 서빙하도록 넓어지고, installer 파일 inventory가 web root 전체를 싣는다.
- 정적 계약(static contracts, parity 스냅샷, feature surface ledger, web Pester 줄 범위 pin, Delivery pin)은 새 구조로
  재기준선을 잡는다(설계 §8).

## 검증

- `npm run test:required --prefix web`, `Invoke-Pester web/tests`, `dotnet test src/DesktopNode.Host.Tests -c Release`,
  packaging Pester 두 종, Delivery Release, PR gate. 브라우저 동작은 train 뒤 설치본 시연 기록으로만 주장한다.

## 관련 문서

- 설계: `docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`
- 계획: `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure.md`
- 경계: ADR-0006, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/CODING_GUIDE.md` 3.2, `web/DESIGN.md`
- 라이선스: `THIRD_PARTY_NOTICES.md`
