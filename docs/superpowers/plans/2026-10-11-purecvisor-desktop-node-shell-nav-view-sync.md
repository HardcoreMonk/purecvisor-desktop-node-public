# Single Edge 셸 화면 전환 수정(BL-0019) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.42.95` 정차 원인 BL-0019를 고친다. Single Edge 셸에서 사이드바 링크나 hash로 화면을 바꾸면 옛 `renderActiveView()`가 옛 `state.activeView`(기본 dashboard) 기준으로 section을 다시 숨겨 dashboard에 머문다. Single Edge nav가 보이는 section의 유일한 주인이 되게 하고, 도움말은 자기 section을 갖게 하며(지금은 troubleshooting section을 지운다), 회귀 시험과 설치본 dev probe로 확인한 뒤 merge하고, 2026-10-11 승인 2로 train `0.42.96` campaign을 연다.

**Architecture:** 정차 기록은 `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-train-04295.md` Task 6, `docs/ga-ready/demo/web-console-shell-demo-2026-10-10.md`, backlog BL-0019. 구조는 ADR-0018과 `docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`. 옛 콘솔의 화면 전환(`navigateToView`, `selectVmFromShell`)은 모두 URL hash를 바꾸므로 Single Edge `navigateToHash`를 거쳐 nav로 모인다. branch `lane1/shell-nav-view-sync-20261011`.

**Tech Stack:** TypeScript(`tsc --noEmit`), `build-served-asset.mjs`, `node --test`(bundle-load 가짜 DOM), C# Host 시험(Chromium DevTools), Playwright(Chromium), 제품 Update/Rollback(`Invoke-PcvDesktopNodeProduct.ps1`)

## 사용자 결정 (2026-10-11)

승인 원문: `1,2,3` (train `0.42.95` 정차 보고의 다음 승인 1~3에 대한 답).

| 항목 | 범위 |
| --- | --- |
| 1 | `Lane 1 수정 campaign: nav route가 옛 render() 전에 setActiveView(viewId)로 view 상태를 맞추고, bundle-load 시험에 사이드바·hash 전환 뒤 section 가시성 회귀와 Host Chromium 시험에 #/vms 전환 확인을 더하며, 설치본 dev probe(제품 Update로 수정 build를 올려 Playwright로 8 화면 전환 확인)를 돌린 뒤 queue 행 추가, Task 11(C5) 이관, push/PR, green CI 뒤 merge(PR #85 merge 포함).` 해석: Lane 0/1 + Lane 2 dev probe 하나. mutation scope: 제품 Update(수정 build의 update ZIP·catalog)와 Rollback(설치본 `0.42.95`로 복귀), service stop/start. 승격 근거 아님. `current-evidence.json` 쓰기 없음. push, PR, green CI 뒤 merge |
| 2 | `수정 merge 뒤 train 0.42.96 출발: queue 78·79·81·84 + 수정 PR 행, package build, pair host mutation(ADR-0016 범위), fullgate(os-mutation-gate 포함)·current-card, Lane 2 probe 3종(S3 재시연 포함), Lane 3 current-evidence 쓰기, 단일 PR merge, main push red면 revert PR. baseline은 설치본 0.42.95 package.` 이 campaign이 닫히면 Task 3이 `pcv-campaign-open`으로 train campaign을 연다 |
| 3 | `train 뒤 S4 campaign, 그 뒤 legacy-retirement campaign(기존 승인 그대로).` train campaign 뒤에 연다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정, disk는 artifacts 아래(이 campaign은 VM을 만들지 않는다) |
| 이관 | `train-04295-20261010` Task 11(C5 runner 확인, `not_before` 2026-10-19)을 Task 4로 옮긴다 |

## Global Constraints

- 설치본은 `0.42.95-admin-smoke+b9898cf`(train 0.42.95 fullgate build)다. Task 2 dev probe는 제품 Update로 수정 build를 올리고 확인 뒤 Rollback으로 `0.42.95`에 돌려 놓는다(다음 train의 baseline이 설치본 `0.42.95`). 보존 VM과 `pcv-it-s2-source`는 건드리지 않는다.
- 옛 콘솔(`web/index.legacy.html`, `web/app.js`, `web/src/served/*`)과 그 계약은 바꾸지 않는다. 수정은 `web/src/modules/*`, `web/src/bootstrap.ts`, `web/index.html`, 생성물, web 시험, Host 시험이다.
- token, credential은 명령줄·summary·evidence·캡처에 남기지 않는다. 사용자 홈 경로·LAN IP·호스트명을 문서에 적지 않는다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: nav가 section 가시성의 주인이 되게 고치고 회귀 시험을 더한다

- [x] `web/src/modules/nav.ts` `_renderContentPaint`가 tab의 section(도움말은 새 `helppage` section)을 보이고 지금 보이는 view를 기억하며(`PCV.nav.activeView()`), 옛 view면 옛 `render()` 전에 `setActiveView(viewId)`로 옛 상태를 맞춘다. 옛 `monitor.ts` `renderActiveView()`는 `PCV.nav.activeView()`가 있으면 그것을 따른다. 옛 `core.ts` `getHashView()`는 Single Edge hash(`#/vms`)도 읽는다. `web/index.html`에 `<section id="helppage" class="section app-view" data-view="helppage" hidden>`을 더한다. `bundle-load.test.mjs` 가짜 DOM에 `#cb .app-view` 조회를 더하고 8 화면 각각 `navigateTo` 뒤 옛 `render()`(polling 재렌더 흉내)와 `#/jobs` hashchange 뒤에도 그 section만 보이는지, 도움말이 troubleshooting section을 지우지 않는지 검사한다. Host Chromium 시험은 `#vms` 전환 뒤 보이는 section이 `vms`인지 확인한다. `single-edge-shell.test.mjs`에 helppage section을 pin한다. 검증: `npm run test:required --prefix web`, `dotnet test src/DesktopNode.Host.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-11): `web/src/modules/nav.ts` `_renderContentPaint`가 tab의 section을 그대로 보이고(도움말은 새 `helppage` section; 이전에는 troubleshooting section에 그려 그 패널을 지웠다) 보인 view를 `_shownView`에 기억해 `PCV.nav.activeView()`로 내놓으며, 옛 view면 옛 `render()` 전에 `setActiveView(viewId)`로 옛 상태를 맞춘다. 옛 `monitor.ts` `renderActiveView()`는 `PCV.nav.activeView()`가 있으면 그것을 따른다(없으면 옛 `state.activeView`). 옛 `core.ts` `getHashView()`는 `#vms`와 `#/vms`(`#/vms/<id>`)를 모두 읽는다. 옛 콘솔의 화면 전환(`navigateToView`, `selectVmFromShell`)은 URL hash를 바꿔 Single Edge `navigateToHash`를 거치므로 그대로 동작한다. `web/index.html`에 `<section id="helppage" ... hidden>`. 시험: `bundle-load.test.mjs` 가짜 DOM이 `#cb .app-view` 8개를 알고, 새 시험이 8 화면 각각 `navigateTo` 뒤와 옛 `render()`(polling 재렌더) 뒤에 그 section만 보이는지, 옛 상태가 따라오는지, 도움말이 자기 section에 그려지고 troubleshooting section을 지우지 않는지, `#/jobs`·`#network`·`#/vms/pcv-it-x` hashchange 뒤 화면을 검사한다. 이 시험은 수정 전 nav·monitor·core로 빌드한 bundle에서 FAIL, 수정 뒤 PASS임을 확인했다. Host Chromium 시험은 `#vms` 전환 뒤 보이는 section이 `vms`인지 확인한다(최대 3초 대기). `single-edge-shell.test.mjs`에 helppage section pin. 검증: `npm run test:required --prefix web`(test, web-contracts, parity, browser fixture), web Pester 50, `dotnet test src/DesktopNode.Host.Tests -c Release` 217, `git diff --check`. 옛 콘솔 `web/app.js`는 바뀌지 않았다. host mutation 없음.

## Task 2: 설치본 dev probe (Lane 2)

- [ ] Task 1 HEAD에서 dev package를 만들고(update ZIP·catalog 포함, version은 train 번호와 겹치지 않게 정한다) 제품 Update로 설치본에 올린 뒤 Playwright(Chromium)로 `http://127.0.0.1/`의 사이드바 8 화면과 `#/vms` hash 전환마다 그 section만 보이는지, 30초 polling 뒤에도 유지되는지, 도움말 뒤 진단 화면이 그대로인지 확인한다. 끝에 제품 Rollback으로 설치본 `0.42.95-admin-smoke+b9898cf`에 돌려 놓고 service·Web·ARP를 확인한다. FAIL이면 멈춘다. evidence `docs/ga-ready/evidence/lane2-web-shell-navigation-dev-probe-2026-10-11.md`(손으로 씀, 승격 근거 아님). 로컬 commit.

## Task 3: 종료 검증, queue 행, push, PR, merge, train campaign 열기

- [ ] clean HEAD에서 PR gate(`Invoke-PcvPrGate.ps1 -SkipBuild` 뒤 Release build), `npm run test:required --prefix web`, Pester 4종, `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `npm run test:public-source-safety --prefix web`(BL-0015 기존 2건 외 0), `git diff --check origin/main...HEAD`. push, PR, 그 PR 번호로 `release-train.json` `queue` 행(area web, risk S, lane2_probe `web.console.shell`)을 같은 PR에 더한 뒤 green CI 뒤 merge, main push run green 확인. campaign을 닫고(Task 4 이관) 승인 2로 train `0.42.96` campaign을 `pcv-campaign-open`으로 연다.

## Task 4: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `train-04295-20261010` Task 11(원래 `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 바뀌지 않는다(`0.42.93-admin-smoke`). dev probe는 승격 근거가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
