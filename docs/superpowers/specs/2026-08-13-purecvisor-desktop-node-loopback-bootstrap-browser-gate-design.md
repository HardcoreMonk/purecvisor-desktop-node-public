# Loopback Bootstrap Browser Gate 설계

- Design-ID: `purecvisor-desktop-node-loopback-bootstrap-browser-gate-v1`
- 작성 기준: `2026-08-13`
- 문서 상태: `approved-design`
- 선행: `docs/superpowers/specs/2026-08-13-purecvisor-desktop-node-web-loopback-session-bootstrap-design.md` §7 다음 slice 1
- host mutation: `false`
- 변경 등급: `M`
- operational current: 바꾸지 않는다

## 1. 문제

loopback session bootstrap은 code-level과 Node `vm` fixture로 닫혔다. 평가서 P1과
bootstrap 설계 §7은 **실제 Chromium이 Web Console을 열어** bootstrap → dashboard → VM
read-only를 확인하라고 한다. GUIDE는 Playwright를 required dependency로 두지 않는다.
설치본 listener는 관리자 opt-in이며 Development Gates `dotnet-tests`는 `windows-latest`에서
돈다.

## 2. 결정

| 경로 | 판정 |
| --- | --- |
| Playwright를 npm/CI required로 추가 | 거부. GUIDE와 기존 fixture 정책을 깬다. |
| 설치본 `http://127.0.0.1/` 를 required CI로 | 거부. MSI/service가 없는 runner에서 실패한다. |
| in-process `DesktopNodeHostApplication` + 로컬 Edge/Chrome CDP | **채택.** Host.Tests가 required `dotnet test`에 포함된다. |
| 설치본 URL 재실행 | opt-in. `PCV_INSTALLED_WEB_URL`이 있을 때만. 이 slice의 required 조건이 아니다. |

여정 (required):

1. `no-default-account` + signing key로 Host listen (Web root = repo `web/`)
2. Edge 또는 Chrome을 headless remote-debugging으로 연다
3. Web Console을 연다
4. service token을 입력하지 않는다
5. `connection-state`가 `Auth required`가 아니고, sessionStorage에 access token이 있으며,
   footer에 `VM: 3/3`이 없다
6. `#vms` 해시로 이동해도 조작된 `pcv-node-a` 행이 없다

브라우저가 없으면 Windows에서 실패한다. CI `windows-latest`는 Edge를 갖는다.

## 3. 비목표

- Playwright 패키지
- 설치본 MSI/service 필수화
- package campaign / current-evidence 승격
- LAN/non-loopback 브라우저
- 시각 디자인 변경
