# BL-0042 설치 version 표시 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Web Console 셸의 version 표시(`#version-badge`)가 설치본 version을 보이게 한다(backlog BL-0042). 운영 위생 C7을 다시 `open_counts=0, undecided=0`으로 만든다. 제품 변경은 release-train queue 행으로 쌓고, 설치본 반영은 따로 승인받는 다음 train에서 한다.

**Architecture:** 근거는 backlog 행 BL-0042(train `0.43.1` Task 6 probe)다. `updateVersionBadge()`는 `GET /api/v1/runtime/policy`의 `data.version`·`data.product_version`·`data.service_version`을 읽는데, 응답(`RuntimePolicyData`)에는 그런 값이 없다. 새 route를 만들지 않고 runtime policy 응답에 `product_version` 하나를 더한다. 값은 Host 프로세스(`DesktopNode.Host.exe`)의 `AssemblyInformationalVersion`이고 Host가 API에 넘긴다. Web은 그 값의 `+` 앞부분을 보인다. branch는 `lane1/bl0042-version-badge-20261110` 하나이고 PR도 하나다. grok(Herdr `grok-status`)이 구현하고 Claude가 task마다 리뷰한다.

**Tech Stack:** .NET(`DesktopNode.Contracts`, `DesktopNode.Api`, `DesktopNode.Host`), TypeScript Web Console(`npm run build:served --prefix web`), node 시험(`web/node-tests`), web Pester, `gh`

## 사용자 결정 (2026-11-10)

승인 원문: `"1, 2 수정"` (`train-04301-20261110` 마무리 보고의 다음 승인 1·2에 대한 답). 2번 문장은 "backlog 분류: BL-0042(설치본 Web Console version 표시가 보이지 않음. `runtime/policy`에 version 값이 없다)를 counts 또는 out-of-scope로 정한다."이고, 답 `수정`은 `counts`로 분류하고 고친다는 뜻이다. 1번(train 계정 백업 삭제)은 PR #137 문서 기록으로 끝났다.

| 항목 | 범위 |
| --- | --- |
| 1 | Lane 0/1. BL-0042 구현(grok)과 리뷰(Claude). task마다 로컬 commit |
| 2 | push, PR 하나, Required CI 다섯 개 green 뒤 merge(`merge_policy=after-green-ci`, 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`). merge 뒤 main push run 확인 |
| 3 | 제품 변경은 `release-train.json` queue 행 하나로 쌓는다. release train 출발은 하지 않는다(따로 승인) |
| 4 | host mutation 없음. 설치본, service, VM을 바꾸지 않는다. `current-evidence.json`을 쓰지 않는다 |
| 설계 | 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 정한다. version은 새 route가 아니라 runtime policy 응답의 `product_version`으로 내준다. Host가 자기 `AssemblyInformationalVersion`을 읽어 넘긴다(API 안에서 entry assembly를 읽지 않는다). Desktop Node는 온라인 update 확인이 없으므로(ADR-0006) badge는 "✓"와 "최신 버전"을 쓰지 않고, 설치 version을 보인다 |

## Global Constraints

- 고치는 범위는 BL-0042뿐이다. 그 밖의 발견은 backlog `undecided` 행으로 쓴다.
- 새 route, 새 권한, 새 `Add-Type`/P/Invoke/native ACL을 만들지 않는다. version은 reflection 속성 읽기로만 얻는다.
- runtime policy 응답 모양을 고정한 시험·fixture·문서는 같은 commit에서 함께 고친다. pin 대상 파일을 바꾸면 `Update-PcvContractSpecPins.ps1 -Apply`를 돈다.
- Web 규칙(직전 `backlog-counts-20261110`과 같다):
  - `web/src`를 고친 뒤 `npm run build:served --prefix web`를 돌린다. `web/app.bundle.js`와 `web/sw.js` CACHE_NAME은 생성물이다.
  - 문구는 i18n 키(ko 기본, en 같은 키)로 쓴다. 색은 기존 token만 쓴다. 계약·Pester·browser fixture가 붙잡은 markup과 문자열은 같은 강도로 옮긴다.
  - axe violation 0. `--prune`은 쓰지 않는다.
- grok 지시는 task마다 약 20분 분량으로 쓰고, 마감 시각을 적는다. grok은 commit·push하지 않고 `docs/**`를 고치지 않는다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: runtime policy `product_version` (서버)

- [x] `RuntimePolicyData`에 `product_version`(string, null 허용)을 더한다. `RuntimePolicyContract.CreateDefault`가 값을 받는다. Host가 `DesktopNode.Host` assembly의 `AssemblyInformationalVersion`을 읽어 API 요청 처리기에 넘긴다. 값이 없으면 `null`이다.
  - 시험: Contracts(응답 JSON에 `data.product_version`), Api(주입한 값이 `GET /api/v1/runtime/policy`에 나온다), Host(주입 경로). 응답 모양을 고정한 golden·fixture·문서를 함께 고친다.
  - 검증: 바뀐 프로젝트의 `*.Tests`를 `-c Release`로, 그다음 `dotnet test src/DesktopNode.sln -c Release`(commit 뒤 clean HEAD), `git diff --check`. 로컬 commit.

실행 기록(2026-11-10): grok이 19:33~19:39에 구현했고 Claude가 리뷰했다.
- `RuntimePolicyData` 끝에 `product_version`(`string?`)을 더했다. `CreateDefault`는 빈 값과 공백을 `null`로 바꾼다. 값을 안 주면 JSON에 `"product_version":null`이 나온다.
- 주입 경로: Host(`DesktopNodeHostApplication`)가 `typeof(DesktopNodeHostApplication).Assembly`의 `AssemblyInformationalVersionAttribute`를 읽어 `DesktopNodeApiRequestProcessor.CreateDefault(productVersion:)`로 넘긴다. processor는 그 값을 `GET /api/v1/runtime/policy`와 `ops.summary`의 `runtime_policy`에 같이 쓴다. API는 entry assembly를 읽지 않는다.
- 시험: Contracts(값 없음 → JSON null, 값 → 그 문자열), Api(주입한 `0.43.1-admin-smoke+abc`가 policy와 ops summary에 같게 나오고, 없으면 둘 다 null), Host(응답 값이 Host assembly 속성과 같다. 리터럴 version은 단언하지 않는다).
- 결과(Release): Contracts 201/201, Api 545/545, Host 246/246, module-size ratchet Pester 3/3, `git diff --check` clean. 줄 수: `DesktopNodeApiRequestProcessor.cs` 436(상한 449), `DesktopNodeHostApplication.cs` 177(상한 201). ratchet JSON과 spec pin은 바꾸지 않았다. 솔루션 전체는 Task 3 clean HEAD에서 돈다.

## Task 2: Web badge 표시

- [x] `updateVersionBadge()`가 `data.product_version`을 읽어 badge에 `v<+ 앞부분>`(예 `v0.43.1-admin-smoke`)을 보인다. "✓"와 "최신 버전" 대신 title에 설치 version 전체(build metadata 포함)를 i18n 문구로 둔다. 값이 없거나 세션이 없으면 지금처럼 숨긴다. 로그인 화면 version 줄(`#lh-version`)도 같은 값을 쓴다.
  - 시험: node 시험(`product_version` 값 → badge 글자와 title, 값 없음 → hidden, `+` 없는 값), 기존 loopback 세션 순서 시험 유지.
  - 검증: `npm run build:served --prefix web`, `npm run test:required --prefix web`, web Pester, `git diff --check`. 화면은 1440과 768 캡처를 본다. 로컬 commit.

실행 기록(2026-11-10): grok이 19:42~19:50에 구현했고 Claude가 리뷰했다.
- `updateVersionBadge()`는 `data.product_version`을 먼저 읽고, 없으면 지금처럼 `data.version`·`data.service_version`을 읽는다.
- badge 글자는 `v` + 값의 `+` 앞부분이다(예 `v0.43.1-admin-smoke`). "✓"와 "최신 버전"은 뺐다. title은 새 i18n 키 `copy.badge.installed_version`(ko `설치 버전: {version}`, en `Installed version: {version}`)에 전체 값을 넣는다. 로그인 화면 version 줄도 짧은 값을 쓴다. update 분기와 `checkForUpdates`는 그대로다.
- 리뷰 수정 1건: `_pcvConsoleVersion`을 정하는 꼬인 조건식을 지우고, 짧은 값을 한 번 계산해 한 줄로 대입하게 했다.
- `web/src/api-types.ts`에 `product_version?: string | null`, browser gate policy mock에 `product_version: "0.42.96-admin-smoke+0123abc"`를 더했다.
- node 시험 3건 추가: `+` 앞만 보이고 title은 전체, `version` fallback, 값이 없으면 숨김.
- 결과: `npm run test:required --prefix web` exit 0(node 397/397, browser-ui-gates `ok=true` violations 0, baseline 변경 없음), web Pester 50/50(리뷰 수정 전), `git diff --check` clean.
- 화면: gate mock 대시보드 1440과 768(`artifacts/bl0042-badge/`, gitignore)에서 badge가 `v0.42.96-admin-smoke`로 보이고 "✓"가 없다. title은 `설치 버전: 0.42.96-admin-smoke+0123abc`다.

## Task 3: 종료 검증과 PR

- [ ] backlog BL-0042를 닫고(`status=closed`, `closed_by`), `release-train.json` queue 행을 더하고, campaign을 닫는다.
  - 종료 검증(clean HEAD): `dotnet build src/DesktopNode.sln -c Release`, Required CI 네 shard 로컬 실행, Pester 네 종, `Invoke-PcvPrGate.ps1` `ok=true`, `git diff --check origin/main...HEAD`.
  - push, PR, Required CI 다섯 개 green 뒤 `--match-head-commit` merge. merge 뒤 main push run을 기다린다. red면 멈추고 보고한다.

## Nonclaims

- operational current는 `0.43.1-admin-smoke` 그대로다. 설치본 badge는 다음 train 뒤에 바뀐다.
- public trusted signing과 external stable publication을 주장하지 않는다.
