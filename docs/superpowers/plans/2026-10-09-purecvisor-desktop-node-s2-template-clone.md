# S2 template clone Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** S2(설치한 VM을 template으로 잠그고 복제해 60초 안에 새 VM을 받는다)의 시나리오 스크립트를 plan-only로 갖춘다. 설치본 실행은 별도 승인까지 열지 않는다.

**Architecture:** 제품 route `POST /api/v1/vms/{id}/template-lock`, `POST /api/v1/vms/{id}/clone/preview`, `POST /api/v1/vms/{id}/clone`가 이미 있다. 복제 가드는 managed Generation 2, Off, checkpoint 없음, 독립 VHDX, 보안 키 없음이다. 스크립트는 S1 `run-s1-console-scenario.mjs`와 같이 Web Console이 쓰는 Local API만 적고, 기본은 plan-only다. branch는 `lane1/s2-template-clone-20261009`다.

**Tech Stack:** Node.js, `npm run scenario:s2-clone --prefix web`, `node --test`

## 사용자 결정 (2026-10-09)

승인 원문: `S2` 다음 `그렇게 진행해` (문서 현행화 커밋, Task 11 이관, S2 campaign을 연다).

| 항목 | 범위 |
| --- | --- |
| 1 | S2 시나리오 스크립트를 Lane 1에서 plan-only로 만든다. 로컬 commit. host mutation, push, PR, merge는 없다. |
| 2 | `s1-installed-20261008` Task 11을 이 campaign Task 2로 이관한다. `not_before` 2026-10-19. 문장과 push, PR, green CI 뒤 merge는 원래 승인 그대로다. |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 `current-evidence.json`을 바꾸지 않는다.
- `--execute`는 이 campaign에서 구현하더라도 돌리지 않는다. 설치본 실행은 `next_approval_required`다.
- guest 비밀번호와 token은 기록하지 않는다.
- 한도: Lane 1 30분·tool batch 18회. Lane 2·3은 열지 않는다.

## Task 1: S2 plan-only 스크립트

- [x] `web/scripts/run-s2-clone-scenario.mjs`와 `npm run scenario:s2-clone`을 추가한다. 기본 plan-only는 HTTP를 보내지 않고 template-lock, clone preview, clone, 60초 한도를 출력한다. `--execute`는 exit 2로 거절한다. `node --test web/node-tests/s2-clone-scenario.test.mjs`와 `node --check`로 확인한다. 로컬 commit.

실행 기록(2026-10-09): `node --check`와 `node --test web/node-tests/s2-clone-scenario.test.mjs`가 2개 통과, exit 0이다. `--execute`는 HTTP 없이 exit 2다. host mutation 없음.

## Task 3: S2 설치본 시연 (Lane 2)

- [x] `pcv-it-s2-source`를 managed Generation 2로 만들고 template lock한 뒤 `pcv-it-s2-clone`을 60초 안에 clone하고 복제본을 지운다. 시연 기록과 criteria S2 `passed`. push, PR, green CI 뒤 merge.

실행 기록(2026-10-09): `run-s2-clone-scenario.mjs --execute` `result=pass`. create·template-lock·delete-clone job `succeeded`, preview HTTP 200, clone `elapsed_ms=3091`. 원본은 Off template로 남고 보존 VM은 Off다. 기록 `docs/ga-ready/demo/s2-template-clone-demo-2026-10-09.md`.

## Task 2: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `s1-installed-20261008` Task 11(그 전 `adr17-s1-console-20261008` Task 14)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

이관(2026-10-09): `audit-green-lean-20261009` Task 9. 이 campaign은 닫는다.

## Nonclaims

- S2 통과는 Task 3 시연과 스크립트 결과로만 주장하고, S3~S4는 주장하지 않는다.
- operational current는 `0.42.93-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
