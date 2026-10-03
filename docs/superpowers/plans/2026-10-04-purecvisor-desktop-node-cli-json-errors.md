# PCVCLI `--json` 오류 출력 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `--json`일 때 오류도 stdout JSON envelope으로 쓴다(설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-cli-json-errors-design.md`).

**Architecture:** Lane 1 수정이다. release train 규칙(`docs/DEVELOPMENT_PROCEDURE.md` §10)에 따라 merge하는 PR에서 `docs/ga-ready/release-train.json` queue에 한 행을 더한다. 설치 반영은 다음 train이다.

**Tech Stack:** C# / .NET 10, xUnit

## 사용자 결정 (2026-10-04)

승인 원문: `전부 승인 합니다` ("`pcvcli --json` 오류 출력 수정을 Lane 1로 시작할지"에 대한 답). 범위는 소스 수정, 테스트, commit, push, PR, green CI 뒤 merge, queue 행이다. host mutation과 train 출발은 범위 밖이다.

## Task 1: 구현과 테스트

- [x] `DesktopNodeCliErrorJson`과 `DesktopNodeCliApplication` 오류 경로.
- [x] `DesktopNodeCliApplicationTests` 네 사례.
- [x] 설계(Lane 2 probe 절 포함)와 `CLI_COMMAND_USAGE.md` 출력 형식 절.

실행 기록(2026-10-04): Cli.Tests `183/183`.

## Task 2: queue 행, 종료 검증, PR과 merge

- [x] PR 번호로 `release-train.json` queue 행을 더한다. (PR #34, `merge_commit`은 수정 commit `0c95852`, Lane 2 probe `cli.json-errors`. `DEVELOPMENT_PROCEDURE.md` §10에 `merge_commit`의 뜻을 한 줄 적었다)
- [ ] clean HEAD 종료 검증, green CI 뒤 merge.
