# ADR-0020: Web Console 본문 원본 정합

상태: 제안
일자: 2026-11-09 (제안. 채택은 구현 campaign 승인 문장으로 한다)

## 결정 마커

```text
DESKTOP_NODE_WEB_CONTENT_PARITY: upstream-fd754e2-content-components-v1
DESKTOP_NODE_WEB_VM_LAYOUT: list-detail-260-v1
DESKTOP_NODE_WEB_RENDER_MODEL: per-view-render-registry-v1
DESKTOP_NODE_WEB_API_FIRST_ITEMS: separate-designs-metrics-adr
```

## 맥락

- 2026-11-09 사용자가 이 PC의 Web Console에서 "로그인 페이지는 최신이지만 워크로드 → 가상 머신 화면은 구 버전과 같은 레이아웃"이라고
  했고, 원본 프론트엔드 전체를 기준으로 감사하라고 했다.
- 감사(`docs/web-console-upstream-parity-audit-2026-11-09.md`, 원본 `fd754e2` 대 `a1cd2e4`, 발견 61건)의 결론:
  - 셸과 공통 모듈은 ADR-0018 때 원본 2.0.0을 거의 그대로 옮겨 같다.
  - 본문 화면은 2026-08 첫 공개본 배치다. 모든 route가 옛 `render()`와 정적 섹션을 쓰고, 옮겨 온 표·빈 상태·읽기 실패 부품은 쓰이지
    않는다. VM 화면은 넓은 표와 오른쪽 320~380px 패널이라 상세 6개 탭과 콘솔이 좁은 칸에 들어간다.
  - 원본은 2026-11-05 뒤로 이 화면들을 바꾸지 않았다. ADR-0018 이행이 본문을 옛 part 단위로만 옮겼고, ADR-0019는 VM 상세·모달·마법사·계정만
    다시 그렸다.

## 결정

- ADR-0018과 ADR-0019는 그대로 둔다. 이 결정은 그 위에서 본문 화면의 골격과 부품 사용을 정한다.
- **본문 정합**: 대시보드, 가상 머신, 네트워크, 작업, 이벤트, 진단, 증적, 계정 화면은 원본 `fd754e2`에서 이미 옮겨 온 부품(`HN.pagehead`,
  `statusPill`, `filterBar`, `createDataTable`, `PCV.components.emptyState`·`loadError`)으로 그린다. 새 부품은 만들지 않는다. 색은 theme
  token만 쓰고, 기본 테마는 BL-0026 결정(어두운 기본)대로 둔다.
- **VM 골격**: 고르지 않았을 때는 표를 전체 폭으로, 골랐을 때는 왼쪽 260px 목록과 나머지 폭의 상세로 둔다. 이름은 상세 헤더에 한 번만
  보인다. hash `#/vms/<vm-id>/<tab>`가 VM과 탭을 고른다. 행 `⋯` 메뉴와 우클릭은 ADR-0019 더 보기 메뉴와 같은 항목이다. 일괄 시작·종료·삭제를
  두고, 일괄 삭제는 고른 개수를 입력해야 켜진다.
- **렌더 모델**: 화면마다 등록된 `render` 함수가 자기 mount를 그린다. 옛 `render()`는 등록되지 않은 화면만 그리고, 모든 화면을 옮긴 뒤
  옛 `render()`, 정적 섹션, 옛 명령 팔레트를 지운다.
- **Local API 선행 항목**: 사용률 metrics(목록 CPU 막대, 성능 탭, 모니터·차트), ISO 목록, 계정 역할 변경·삭제·재활성화는 이 개편에서 뗀다.
  metrics는 새 runtime 동작이라 따로 ADR을 쓴다. ISO 목록과 계정 관리는 기존 결정 안에서 설계 문서로 정한다. API가 생기기 전까지 그
  자리는 0이나 빈 칸으로 보이지 않고 뺀다.
- 넣지 않는 원본 기능: VM 카드 보기, 즐겨찾기, VM 비교, 대시보드 기간 선택. 이유는 설계 문서 5절이다.

## 결과

- 구현은 단계 U1~U8(설계 문서 10절)이고 단계마다 Lane 1 PR 하나다. 계획 초안은
  `docs/superpowers/plans/2026-11-09-purecvisor-desktop-node-ui-parity-impl.md`다.
- Web 파일은 MSI payload라 단계 PR마다 release train queue 행이 생긴다. 설치본 화면은 release train 뒤에 바뀌고, 출발은 사용자 승인이다.
- `web/src/modules/vm.ts`는 줄 수 상한이라 목록·골격을 새 모듈 `vm-list.ts`로 나눈다.
- VM 골격을 붙잡는 계약·시험(설계 문서 7절)은 U2 PR에서 함께 고친다.

## 검증

- `npm run test:required --prefix web`(node 시험, 정적 계약, parity, browser UI gate와 axe), 새 node 시험(설계 문서 8절),
  `dotnet test src/DesktopNode.Delivery.Tests -c Release`, PR gate.
- 화면은 단계마다 가짜 API 캡처(1440, 768, 390 폭)를 원본 캡처와 나란히 검토한다. 설치본 동작은 release train 뒤 시연 기록으로만 주장한다.

## 관련 문서

- 감사: `docs/web-console-upstream-parity-audit-2026-11-09.md`
- 설계: `docs/superpowers/specs/2026-11-09-purecvisor-desktop-node-web-console-upstream-parity-design.md`
- 선행 결정: ADR-0018, ADR-0019, ADR-0012(읽기 동시성, metrics ADR이 참조)
