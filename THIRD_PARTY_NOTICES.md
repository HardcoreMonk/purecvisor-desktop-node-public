# Third-party notices

이 저장소가 배포 payload에 싣는 제3자 구성 요소와 라이선스다. 가져온 시점과 경로는 ADR-0018과
`docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`를 따른다.
자산 파일은 campaign `single-edge-frontend-structure-20261010` Task 5에서 `web/vendor/`에 들어오며, 이 표는 그 전에
고지 목록으로 먼저 둔다.

| 구성 요소 | 버전 | 용도 | 라이선스 | 경로 |
| --- | --- | --- | --- | --- |
| Pretendard | purecvisor `ui/vendor/pretendard` 사본 | Web Console 자체 호스팅 글꼴(CSS + woff2 6개) | SIL Open Font License 1.1 | `web/vendor/pretendard/` |
| Coolicons | 1.2.2 subset | Web Console SVG 아이콘 sprite | CC BY 4.0 (출처 표기: Coolicons by Kryston Schwarze) | `web/vendor/coolicons/` |
| Chart.js | 4.4.4 | Web Console 차트 렌더링 | MIT | `web/vendor/chart.umd.min.js` |
| PureCVisor Single Edge Web UI 공통 모듈 | `HardcoreMonk/purecvisor` `ed147de` | 셸·nav·theme·modal·ui·uxlib·mobile·charts·help·i18n·api 구조(파일 머리에 출처 주석) | Apache License 2.0 (같은 저작자) | `web/src/modules/`, `web/style.css`, `web/index.html`, `web/sw.js` |

가져오지 않는 것: qrcode(TOTP 전용), noVNC(Desktop Node는 자체 console frame route를 쓴다).
