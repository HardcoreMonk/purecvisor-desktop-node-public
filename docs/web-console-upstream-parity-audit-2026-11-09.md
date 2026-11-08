# Web Console 원본 정합 감사 (2026-11-09)

- 요청: 사용자 2026-11-09. "프론트엔드 UI/UX는 아직도 구버전으로 나오는데?" 뒤에 "로그인 페이지는 최신 버전으로 확인, 그러나 워크로드 -> 가상머신 메뉴를 확인 해 보면 구 버전과 동일한 레이아웃을 유지 하고 있음"이라고 했고, 이어서 "원본 프론트엔드 전체를 기준으로 감사 처리 해"라고 했다.
- 원본 기준: `HardcoreMonk/purecvisor` `fd754e206dfb7aafaf076872a83c4ae8d811684b`(2026-11-08, Release 2.0.4)의 `ui/`. ADR-0018이 실제로 가져온 출처는 `ed147de`(2.0.0, 2026-10-08)다.
- Desktop Node 기준: `main` `a1cd2e4`(PR #121 merge). `web/`은 설치본 `0.42.99-admin-smoke`의 payload commit `f7426d1`과 같다(`git diff f7426d1 a1cd2e4 -- web` 없음).
- 관련 결정: ADR-0018(Single Edge 구조 차용), ADR-0019(정보 구조와 상호작용 규칙). 설계 `docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`, `docs/superpowers/specs/2026-11-05-purecvisor-desktop-node-ui-redesign-design.md`.

## 1. 결론

사용자 관찰이 맞다. 가상 머신 화면뿐 아니라 본문 화면 대부분이 2026-08 첫 공개본의 배치를 그대로 쓴다. 원본은 2026-11-05 뒤로 이 화면들을 바꾸지 않았다. 차이는 Desktop Node가 본문 영역을 원본으로 옮기지 않은 데서 생겼다.

- 원본과 같은 부분: 로그인 오버레이, 셸(사이드바, 상단바, 상태바, breadcrumb), modal, toast, 테마 엔진, 명령 팔레트 모듈, 공통 모듈(`ui`, `uxlib`, `modal-core`, `modal`, `filter-state`, `theme`). ADR-0018 때 원본 2.0.0을 거의 그대로 옮겼다. 사용자가 로그인 화면을 최신으로 본 이유다.
- 옛 상태로 남은 부분: 대시보드, VM 목록, 네트워크, 작업, 이벤트, 진단, 증적.
  - 모든 route가 옛 `render()`(`web/src/modules/monitor.ts:904-938`)를 부르고, 화면은 `web/index.html:226-473`의 정적 섹션이다(`web/src/modules/nav.ts:51-85`). 원본은 route마다 render 함수가 본문을 다시 그린다.
  - 원본에서 옮겨 온 표·빈 상태·읽기 실패 부품(`createDataTable`, `emptyStatePro`, `renderLoadError`)은 정의 파일(`ui.ts`, `uxlib.ts`) 밖에서 한 번도 불리지 않는다. `pagehead`는 도움말 한 곳에서만 쓴다.
  - VM 화면 골격 `.vm-workspace { grid-template-columns: minmax(0, 1fr) minmax(320px, 380px) }`(`web/style.css:5212-5214`)는 `c76a831`(2026-08-25 첫 공개 snapshot)부터 그대로다. 상세 6개 탭과 콘솔이 그 오른쪽 320~380px 패널(`web/index.html:333-347`) 안에 들어간다. 원본은 왼쪽 260px 목록과 넓은 오른쪽 탭 상세다.
- 왜 남았는가:
  - 2026-10-10 설계 §4는 "실제 이동은 옛 part 단위"이고 함수 단위 분리는 정리 후보로 남겼다. ADR-0018 이행은 데이터 화면을 원본에서 옮기지 않고 옛 Desktop Node part를 파일만 옮겼다.
  - ADR-0019 재설계는 VM 상세 탭, 모달, 생성 마법사, 계정만 새로 그렸다. 설계 5.1절 W1은 VM 목록의 열과 상태 칩만 정했고 화면 골격은 정하지 않았다.
- 원본 쪽 변경에서 생긴 차이는 하나다. 2.0.1의 로그아웃 때 열린 modal 닫기(`api.js:892`의 `_teardownSession`)가 Desktop Node에는 없다(B-12, C-19).

## 2. 방법

- 하위 에이전트 세 개가 범위를 나눴다. A는 VM 목록·상세·생성·콘솔, B는 셸·공통 UX와 전체 화면 목록, C는 VM 밖 데이터 화면이다. Claude가 지휘하고 핵심 주장을 `a1cd2e4`에서 코드로 다시 확인했다(1절의 렌더 모델, 부품 미사용, VM 골격과 B-07, B-11).
- 비교는 코드 위치 대조와 화면 캡처다.
  - Desktop Node: `npm run browser:ui-gates`와 같은 스크립트를 가짜 API와 loopback으로 돌려 26화면을 찍었다(`ok=true`).
  - 원본과 Desktop Node 셸: 각각 loopback 정적 서버로 띄우고 1440×900으로 찍었다. 백엔드가 없어 셸은 JavaScript로 열었고 VM 두 개는 가짜 데이터다.
  - 원본 clone과 캡처는 저장소 밖 작업 폴더에만 있다. 설치된 service, 계정, VM은 건드리지 않았다.
- ADR-0019 조사 기준 `c45dbf5`는 비공개 purecvisor-single commit이라 공개 원본 이력(release squash 117개, shallow 아님)에 없다. 그래서 날짜로 대신했다. 2026-11-05 시점의 공개 상태는 `69448db`(2.0.3, 2026-10-25)이고, `69448db..fd754e2` 사이 `ui/modules` 변경은 0건이다.
- 위치 표기: 원본은 `ui/` 아래 파일 이름과 줄이다. Desktop Node는 `web/` 기준이고, `.ts` 모듈은 `web/src/modules/`, `bootstrap.ts`는 `web/src/`에 있다.
- 분류:

  | 기호 | 뜻 |
  | --- | --- |
  | a | 원본을 따라 가져올 수 있다 |
  | a-API | 가져올 수 있지만 Local API부터 더해야 한다 |
  | b | Linux runtime 전용이라 저장소 경계 밖이다(KVM/libvirt/LXC/ZFS/OVS/OVN, noVNC 등) |
  | c | ADR-0019나 그 설계가 의도해서 다르게 정했다 |
  | d | Desktop Node 전용 화면·내용이라 원본에 대응이 없다 |
  | same | 이미 같다 |

- 체감은 high, med, low이고 규모는 S(한 PR의 작은 수정), M(한 PR), L(여러 PR이나 API 추가)이다.

## 3. 원본 변경 (`ed147de` → `fd754e2`)

| commit | 판 | 바뀐 것 | Desktop Node 영향 |
| --- | --- | --- | --- |
| `969c610` | 2.0.1 | 로그아웃 때 `_teardownSession`이 열린 modal을 닫음(`api.js:892`), WebSocket 재연결 정리, VM 목록 읽기 실패 상태, 생성 job 완료 확인, 디스크 리사이즈 본문, `advanced.js` escape | modal 닫기만 없다(B-12, C-19). WebSocket은 Desktop Node가 쓰지 않는다(ADR-0018) |
| `2f218f2` | 2.0.2 | 버전 문자열, `sw.js` 캐시 이름, 안내서 | 없음 |
| `69448db` | 2.0.3 | 위와 같고, `fetchPost` 요청별 `timeoutMs`, 컨테이너 exec 시간 제한 | 없음. guest 실행은 job으로 돈다 |
| `fd754e2` | 2.0.4 | `index.html`, 안내서, 문서, `sw.js`, bundle | 없음 |

## 4. 원본 화면 대응

| 원본 화면 | 원본 위치 | Desktop Node | 분류 |
| --- | --- | --- | --- |
| 운영 대시보드 | `shell.js:67`, `app.js:1512` | 대시보드(옛 콕핏 배치) | a |
| 가상 머신 요약·콘솔·스냅샷·성능·타임라인 | `shell.js:36,69`, `vm.js:747` | 가상 머신과 상세 6개 탭 | 골격 a, 탭 구성 c |
| 템플릿 | `advanced.js:52` | 템플릿(잠긴 VM 목록) | d |
| 네트워크 | `network.js:300` | 네트워크(읽기 전용, `vm.ts:258`) | a |
| 토폴로지 | `network.js:1218` | 없음 | a |
| 호스트 상태 | `monitor.js:3989` | 진단 troubleshooting(`vm-console.ts:169`), 대시보드 일부 | a |
| 운영 개요, VM 모니터, 호스트 모니터 | `monitor.js:539` | 없음(대시보드 신호 격자뿐) | a-API |
| 이벤트 센터 | `monitor.js:713` | 이벤트(`ops.ts:234`), 진단 incident(`ops.ts:156`) | a |
| 알림 | `monitor.js:3126` | 이벤트·상태바 일부 | a |
| 감사 로그 | `monitor.js:3164` | 없음 | a-API |
| 활동 로그 | `accounts.js:823` | 이벤트의 활동 패널(job 목록을 다시 그림) | a |
| 히트맵 | `monitor.js:4060` | 없음 | a-API |
| API 성능 | `accounts.js:699` | 없음 | a-API |
| 계정과 권한 | `accounts.js:113` | 계정(`accounts.ts:363`) | 표 a, 생성 모달 c |
| API 관리, 세션, API key | `accounts.js:305,862,917` | 진단의 token 패널 일부 | d |
| 도움말 | `help.js:48` | 도움말 | d |
| Swagger API | `help.js:555` | 없음 | c(2026-10-10 설계 §2) |
| 로그인·첫 계정 | `index.html:122`, `modal.js` | 같음 | same |
| 환경설정, 테마 편집기, 알림 센터 | `app.js:635`, `theme.js`, `nav.js:896` | `prefs.ts`, 같음 | same |
| 명령 팔레트, 단축키 도움말 | `nav.js:402`, `help.js:1006` | 팔레트 두 개, 단축키는 등록 없음 | a(B-09, B-10) |
| 컨테이너, Local VPC, OVN, 오버레이, 보안 그룹, 스토리지, 스토리지 모니터, 백업, iSCSI, DPDK, SR-IOV, 커넥션 풀, GPU, 보안 이벤트(Suricata), 자가치유, 설정 관리 | `shell.js:70-103` | 없음 | b |
| TOTP, push, `maintenance.html` | `totp.js`, `push.js` | 없음 | b |
| (원본 없음) 작업, 증적, 진단 번들 | — | `ops.ts:103`, `monitor.ts:497`, `monitor.ts:262` | d |

## 5. 발견 61건

### 5.1 A. VM 목록·상세·생성·콘솔 (23건)

| id | 영역 | 원본 | Desktop Node | 분류 | 체감 | 규모 |
| --- | --- | --- | --- | --- | --- | --- |
| A-01 | 목록·상세 골격. 원본은 왼쪽 260px 목록과 넓은 탭 상세, Desktop Node는 넓은 표와 오른쪽 320~380px sticky 패널 | `vm.js:747-800`, `style.css:3851` | `index.html:333-347`, `style.css:5212-5225` | a | high | M |
| A-02 | 콘솔이 좁은 패널 안에서 맞춤 배율 약 0.33으로 보인다 | `vm-console.js:73-80`(min-height 500px) | `vm-console.ts:686-702,839` | a | high | A-01과 함께 |
| A-03 | VM 이름이 선택 카드, 패널 제목, 헤더 h2로 세 번 나오고 탭이 두 줄로 접힌다 | `vm.js:619` | `vm.ts:1046-1055,1276-1307` | a | med | S |
| A-04 | 페이지 머리. 원본은 `HN.pagehead`(제목, 설명, 새 VM, 스냅샷, VM 삭제), Desktop Node는 eyebrow와 h2에 줄바꿈되는 도구 줄 | `vm.js:785-796` | `index.html:291-331` | a | med | S |
| A-05 | 목록/카드 보기 전환 | `vm.js:268-301` | 없음 | a | low | S |
| A-06 | 목록의 CPU 사용률 막대와 스파크라인. API에 사용률이 없고 cpu-stats는 개수만 준다 | `vm.js:325-328` | 없음 | a-API | med | L |
| A-07 | 행 `⋯` 메뉴와 우클릭 메뉴(원본 15항목). ADR-0019 설계 W1에 있었지만 구현 Task 7a에서 빠졌다 | `vm.js:446-489` | `vm.ts:69-87`(상태별 주 조작 하나뿐) | a | med | S |
| A-08 | 체크박스, 선택 VM 중지, 일괄 삭제, 비교 | `vm.js:764-769`, `vm-lifecycle.js:354-470`, `vm-console.js:985` | 없음 | a | med | M |
| A-09 | 빈 상태 CTA와 목록 읽기 실패 표시 | `vm.js:229-259` | `vm.ts:171-174`(muted 한 줄) | a | med | S |
| A-10 | 정렬 방향 반전, 즐겨찾기 우선, j/k 이동 | `vm.js:167-200,430-445` | `vm.ts:45-54`(select, 방향 고정) | a | low | S |
| A-11 | VM·탭 바로 가는 링크. Desktop Node hash는 화면 이름만 읽는다 | `nav.js:185-189` | `core.ts:351-354` | a | low | S |
| A-12 | 상태 칩 5개와 개수 | 없음 | `vm.ts:89-114` | d | low | — |
| A-13 | 상세 탭 구성(원본 5개, Desktop Node 6개) | `vm.js:696-702` | `vm.ts:489-496` | c | low | — |
| A-14 | 성능 탭 | `vm-console.js:874-938` | 없음 | a-API | med | L |
| A-15 | 헤더 조작 위치(원본은 요약 탭 아래 작업 카드 버튼 17개) | `vm.js:664-680` | `vm.ts:1056-1071` | c | low | — |
| A-16 | 생성 마법사 단계와 검사 | `vm-lifecycle.js:505-546` | `index.html:492-535` | c | low | — |
| A-17 | ISO 찾아보기. ISO 목록 route가 없다 | `vm-lifecycle.js:743-814` | 없음 | a-API | med | L |
| A-18 | 저장 형식(zvol/qcow2/raw), QoS, 브리지 선택 | `vm-lifecycle.js:567-616` | 없음 | b | low | — |
| A-19 | 복제 cow/full 모드. 나머지 복제 흐름은 ADR-0019 모달이다 | `vm-lifecycle.js:1603-1656` | `vm.ts:647-735` | b(나머지 c) | low | — |
| A-20 | 삭제 이름 입력 확인 | `vm-lifecycle.js:359-395` | `vm.ts:2128` | c | low | — |
| A-21 | noVNC 콘솔 | `vm-console.js:165-300` | `vm-console.ts:839`(frame·input API) | b | low | — |
| A-22 | 템플릿 화면. 원본은 spec 자원, Desktop Node는 `template_lock` VM 목록 | `advanced.js:52-160` | `vm.ts:127-163` | d | low | — |
| A-23 | 요약 탭 readback에 `linux_blkio_compatible=false`, `qemu_guest_agent` 같은 원시 필드 이름이 보인다 | 없음 | `vm.ts:1351-1407` | d | med | S |

### 5.2 B. 셸·공통 UX (18건)

| id | 영역 | 원본 | Desktop Node | 분류 | 체감 | 규모 |
| --- | --- | --- | --- | --- | --- | --- |
| B-01 | 본문 렌더 모델. 원본은 route마다 render 함수가 `#cb`를 다시 그리고, Desktop Node는 모든 route가 옛 `render()`와 정적 섹션을 쓴다 | `nav.js:156-226` | `nav.ts:51-85`, `monitor.ts:904-938`, `index.html:226-473` | a | high | L |
| B-02 | 페이지 머리(`HN.pagehead`) | `ui.js:216` | `index.html:265,291`(eyebrow와 h2) | a | med | M |
| B-03 | 대시보드. 원본은 기간 선택, 게이지 4개, 워크로드 표·칩·추이, 경고 피드. Desktop Node는 hero, 고정 메모, 빠른 작업 6칸, 두 번째 머리 "운영 콕핏" | `app.js:1512-1563` | `index.html:228-286` | a(ZFS·자가치유 칸은 b) | high | L |
| B-04 | 대시보드 칩이 버튼처럼 보이지만 `<span>`이다 | — | `index.html:250-256` | d | low | S |
| B-05 | 대시보드 문구. "새 VM"이 두 곳(`:258`, `:270`), 빠른 작업 이름이 사이드바와 다르고 "호스트 상태"는 자기 화면으로 간다(`:261-263`) | — | `index.html:258-270` | d | med | S |
| B-06 | 기본 테마. 원본은 흰 `supanova`, Desktop Node는 어두운 `supanova-cyan`이고 Part 2 `:root`가 token을 덮는다. `web/DESIGN.md:79-81`과 `:84-100`이 서로 다르다 | `index.html:50` | `style.css:3860-3880` | d(BL-0026 사용자 결정) | high | M |
| B-07 | 로그인 화면 노드 상태. 원본은 30초마다 health를 읽는다. Desktop Node는 채우는 코드가 없어 늘 "노드 응답 대기"와 "—"다. 무인증 health route가 있는지부터 확인해야 한다 | `app.js:405-445` | `index.html:103-138`(`lh-state`를 쓰는 TS 없음) | a | med | S |
| B-08 | 로그인 TLS 표시가 빈 pill이다 | `app.js:330-350` | `index.html:95` | a | low | S |
| B-09 | 명령 팔레트가 두 개다. 검색 버튼은 새 모달, Ctrl+K는 옛 `#command-palette`를 열고 빈 결과 문구가 영어다 | `app.js:1794`, `nav.js:402` | `nav.ts:210`, `bootstrap.ts:126`, `monitor.ts:822-853` | a | med | S |
| B-10 | 단축키가 등록되지 않았는데 도움말이 안내한다 | `app.js:1781-1795,2237-2245` | `help.ts:132-133,207-218` | a | med | S |
| B-11 | 600px 이하에서 VM 전원·삭제 버튼이 아무 일도 하지 않는다. 부르는 `PCV.vm.lifecycle`, `vmPower`, `PCV.vm.bulkDelete`, `PCV.vm.vmDel`이 없다 | `mobile.js:386,413,596` | `mobile.ts:219,242,328-329`, export `vm.ts:2738` | a | med | S |
| B-12 | 로그아웃 때 열린 modal이 로그인 화면 위에 남을 수 있다(코드로만 추론, 브라우저 미확인) | `api.js:892` | `api.ts:112-126,308-325` | a | med | S |
| B-13 | 빈 상태·로딩. components 빈 상태는 VM 화면만 쓰고 나머지는 `.muted` 문장이다 | `ui.js:845,1201` | `ops.ts:138`(영어) | c(적용 범위가 VM뿐) | med | M |
| B-14 | 로그인 폼이 세 곳이다(로그인 화면, 경고 영역, 진단 화면) | 로그인 오버레이 하나 | `monitor.ts:697-715`, `index.html:459-465` | a | med | S |
| B-15 | 도움말 카탈로그에 비밀번호 route 2개가 없고 계정 화면 열 값이 `topbar`다 | — | `help.ts:23-33` | d | low | S |
| B-16 | 사이드바·상태바·breadcrumb. 묶음은 ADR-0019대로 바뀌었고 구조·키보드 이동·`aria-current`는 같다 | `shell.js:65-108,226-251,347-366` | `shell.ts:21-40,172-193,289-308` | same | — | — |
| B-17 | 공통 모듈. 주석·공백을 뺀 diff가 1~32줄이다(OVN 제거, `backdropDismiss`, 계정 등록 `role: admin`, i18n 키) | `ui`, `uxlib`, `modal-core`, `modal`, `filter-state`, `theme` | 같은 이름 `.ts` | same | — | — |
| B-18 | 사이드바 `aria-label`이 빠졌다. 원본 VM 탭 스와이프 코드가 남았지만 쓰이지 않고, resize·가장자리 스와이프는 옮기지 않았다 | `shell.js:195`, `app.js:1802-1821` | `shell.ts:152`, `nav.ts:659-693` | a | low | S |

### 5.3 C. VM 밖 데이터 화면 (20건)

| id | 영역 | 원본 | Desktop Node | 분류 | 체감 | 규모 |
| --- | --- | --- | --- | --- | --- | --- |
| C-01 | 데이터 화면 전체가 옛 문자열 렌더러다. 이식된 `pagehead`, `statusPill`, `filterBar`, `createDataTable`을 쓰지 않는다 | `network.js:337`, `accounts.js:126` | `ops.ts:6`, `monitor.ts:183`, `vm.ts:258`. 부품 `ui.ts:44,81,113,554`, `uxlib.ts:441` | a | high | L |
| C-02 | 옛 패널 배경이 `rgba(17,26,43,.78)` 하드코딩이다(B-06의 dark `:root`와 겹침) | 테마 token | `style.css:4795-4829`, `:3860` | a | med | M |
| C-03 | 영어 고정 문구 | `_L` ko 우선 | `ops.ts:15,120,176-186`, `monitor.ts:274-311,389-403,549-567` | a | med | M |
| C-04 | 빈 상태와 읽기 실패 구분 | `network.js:344,465`(`emptyStatePro`, `renderLoadError`) | `ops.ts:113,249`, `vm.ts:296`(`.muted` 문장) | a | med | M |
| C-05 | 네트워크 배치. 원본은 설명과 주 버튼, 개수 chip, 768px 이하 카드. Desktop Node는 내부 요약 카드(source, mutation)와 텍스트 필터 | `network.js:337-341,371-379,382-400` | `vm.ts:284-301`, `index.html:370` | a | med | M |
| C-06 | 네트워크 VM 수 열. `vm.list`의 network로 브라우저에서 셀 수 있다 | `network.js:417` | 없음 | a | low | S |
| C-07 | 스위치 생성·편집·삭제, 호스트 baseline, 방화벽·ACL | `network.js:477-709,145-278,426-447` | 읽기 전용 | b, Hyper-V 스위치 편집은 c(API 필요) | low | L |
| C-08 | 모니터와 차트. `charts.ts`·`metrics.ts`는 원본과 같지만 호출이 0이다. `mobile.ts:284` host.cpu는 늘 0이고 host.status는 readiness뿐이다(`DesktopNodeHyperVModels.cs:514`) | `monitor.js:981,1207-1209` | 없음 | a-API | med | L |
| C-09 | 이벤트 센터. 원본은 12열 격자, VM 검색 표, 빠른 이동. Desktop Node는 심각도 pill과 최대 12행 | `monitor.js:766-827` | `ops.ts:234-256` | a(CPU 막대는 API 필요) | med | M |
| C-10 | 이벤트 화면의 활동 패널이 작업 목록을 다시 그리고 제목이 "운영자 활동"이다 | `accounts.js:823`(브라우저 API 로그) | `ops.ts:328-361`, `i18n.js:967` | a | med | S |
| C-11 | 다음 페이지 버튼이 이벤트 화면에만 있다. 작업 화면은 `next_offset` 글자만 보인다 | (원본 없음) | `ops.ts:368-371`, `bootstrap.ts:388` | d | med | S |
| C-12 | 작업 표. 원본 부품은 정렬·검색·CSV와 개수 chip을 준다. Desktop Node는 div 3칸 행이고 시각·오류 열이 없다 | `createDataTable`, `filterBar` | `ops.ts:123`, `index.html:389-408` | a | med | M |
| C-13 | 계정 배치. 원본은 pagehead, RBAC pill, 표와 생성 카드 | `accounts.js:185-209` | `accounts.ts:411-420`(`pcv-vm-table` 재사용), 생성은 모달 | a(표), c(모달) | low | S |
| C-14 | 계정 역할 변경, 삭제, 재활성화. route가 없다(`ApiHandlerAdapterContract.cs:92-96`) | `accounts.js:145-150,283` | `accounts.ts:390-408`(비활성화와 재설정뿐) | a-API(TOTP 열은 c) | med | M~L |
| C-15 | 보안 이벤트(Suricata), TOTP, API key·세션 | `security.js:116-119`, `totp.js:251` | 없음 | b | low | — |
| C-16 | 진단 화면이 패널 8개를 세로로 나열하고 로그인 폼, registry bridge, Host Ops bucket 표, 경계 칩 같은 내부 값을 운영자에게 보인다 | `monitor.js:4020-4037`(grid-2 카드와 권장 조치) | `index.html:449-472`, `monitor.ts:184-260,318-323` | d(원본 카드 패턴은 a) | high | M |
| C-17 | 메뉴 이름과 화면 제목이 다르다("장애 대응", "감독 증적", "추적 작업", eyebrow "세션") | 메뉴 이름 = 제목 | `i18n.js:950-971` | a | low | S |
| C-18 | 증적 화면이 GPU snapshot, Public boundary head 같은 release 엔지니어링 값을 보인다 | (원본 없음) | `monitor.ts:556-605` | d | low | M |
| C-19 | 로그아웃 때 modal 닫기(B-12와 같은 결함) | `api.js:892-899` | `api.ts:112-126` | a | low | S |
| C-20 | Linux 전용 화면의 UX 패턴(용량 gauge, 빈 상태와 KPI, 승인 대기열). 화면은 경계 밖이고 패턴만 쓸 수 있다 | `storage.js:74-120`, `container.js:54,164-176`, `selfhealing.js:434` | 해당 없음 | b(패턴은 a) | low | S |

## 6. 지금 틀린 동작

레이아웃과 별개로, 지금 Desktop Node에서 동작이 틀린 것들이다.

| id | 증상 | 확인 |
| --- | --- | --- |
| B-11 | 600px 이하에서 VM 전원·삭제 버튼을 눌러도 아무 일이 없다 | Claude가 코드로 확인(부르는 네 함수가 export·정의 어디에도 없음) |
| B-07 | 로그인 화면 노드 상태가 늘 "노드 응답 대기"다 | Claude가 코드로 확인(`lh-state`를 쓰는 TS 없음) |
| B-09 | Ctrl+K가 옛 팔레트를 열고 결과가 없으면 영어 문구가 나온다 | 에이전트 코드 대조 |
| B-10 | 도움말이 등록되지 않은 단축키를 안내한다 | 에이전트 코드 대조 |
| B-14 | 로그인 폼이 세 곳에 있다 | 에이전트 코드 대조와 캡처 |
| B-12, C-19 | 로그아웃 뒤 열린 modal이 남을 수 있다 | 코드로만 추론, 브라우저 미확인 |
| C-10 | 이벤트 화면이 작업 목록을 "운영자 활동"으로 다시 그린다 | 에이전트 코드 대조 |
| C-11 | 작업 화면에 다음 페이지 버튼이 없다 | 에이전트 코드 대조 |
| A-23 | VM 요약 탭에 원시 필드 이름이 보인다 | 에이전트 코드 대조와 캡처 |

## 7. 분류 집계

| 묶음 | a | a-API | b | c | d | same | 합 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| A | 10 | 3 | 3 | 4 | 3 | 0 | 23 |
| B | 11 | 0 | 0 | 1 | 4 | 2 | 18 |
| C | 12 | 2 | 3 | 0 | 3 | 0 | 20 |
| 합 | 33 | 5 | 6 | 5 | 10 | 2 | 61 |

- 행 하나에 두 분류가 걸리면(예: A-19 b와 c, C-13 a와 c) 주 분류로 셌다.
- 같은 결함이 두 묶음에 나온 것: B-12와 C-19(로그아웃 modal), B-13과 C-04(빈 상태), B-06과 C-02(dark `:root`). A-06, A-14, C-08은 같은 사용률 metrics API가 필요하다.
- Local API부터 필요한 5건: A-06(목록 CPU 막대), A-14(성능 탭), C-08(모니터·차트), A-17(ISO 찾아보기), C-14(계정 역할·삭제·재활성화). API 묶음으로는 셋이다(사용률 metrics, ISO 목록, 계정 관리).

## 8. 다음 단계

- 개편 설계는 `docs/superpowers/specs/2026-11-09-purecvisor-desktop-node-web-console-upstream-parity-design.md`, 결정은 ADR-0020, 구현 campaign 계획은 `docs/superpowers/plans/2026-11-09-purecvisor-desktop-node-ui-parity-impl.md`에 둔다(campaign `ui-parity-design-20261109`).
- 범위 밖으로 본 것(report-only): `web/DESIGN.md:79-81`과 `:84-100`의 기본 테마 설명이 서로 다르다(B-06). `web/src/modules/vm.ts`는 2,739줄로 ratchet 상한이다(`docs/project-status-audit-2026-11-09.md` §5).

## Nonclaims

- 이 감사는 코드 대조와 가짜 API 데이터로 찍은 캡처다. 설치본 브라우저 동작을 주장하지 않는다. B-12는 코드로만 추론했다.
- 이 문서는 operational current `0.42.99-admin-smoke`를 바꾸지 않는다. 설치본 화면은 구현 campaign과 release train 뒤에 바뀐다.
- 원본 저장소의 Linux runtime 화면을 가져온다고 주장하지 않는다(저장소 경계).
- public trusted signing과 external stable publication을 주장하지 않는다.
