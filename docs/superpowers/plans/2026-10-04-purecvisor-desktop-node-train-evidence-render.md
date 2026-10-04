# Release train 증적 렌더러 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** release train 2단계. facts JSON 하나로 train evidence `12`개를 렌더하는 C# 도구를 만들고, 0.42.89 문서를 golden으로 Required CI에서 시험한다.

**Architecture:** 설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-evidence-render-design.md`. 렌더러는 `src/DesktopNode.Verification/TrainEvidence/`, 틀은 `docs/ga-ready/trains/templates/`, facts는 `docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json`. branch `lane1/train-evidence-render-20261004`(`origin/main` `53980b4` 기준), PR 하나로 merge한다.

**Tech Stack:** C# (.NET 10), xUnit, `System.Text.Json`

## 사용자 결정 (2026-10-04)

승인 원문: `전부 승인 합니다` 뒤 선택 "train 2단계 증적 생성기 (권장)" (Lane 1, host mutation 없음, commit, PR, merge까지). 같은 답에서 로컬 잔여물 정리, 남은 VM 디렉터리 삭제, train 3단계 pair orchestrator(설계 검토부터)도 승인했다. 잔여물 정리는 이 campaign 전에 끝났다(백업 `artifacts/cleanup-20261004/`).

## Global Constraints

- host mutation 없음. evidence 문서 내용을 바꾸지 않는다(golden이 커밋된 문서를 그대로 재현해야 한다).
- 기존 `pcvverify verify` 문법과 오류 형식은 바꾸지 않는다.
- 한도: checkpoint마다 Lane 1 30분·tool batch 18회.

## Task 1: 설계와 계획

- [x] 설계, 이 계획, campaign `train-evidence-render-20261004`.

## Task 2: 렌더러와 CLI

- [x] `TrainEvidenceRenderer`(facts 읽기, 틀 문법, 엄격한 key 검사), `TrainEvidenceCommand`(`--check`, `--write`, `--allow-update`), `VerificationApplication` 분기.
- [x] 엔진 시험: 치환, 선택 줄, 없는 key, 남는 key, CR/LF 값, 모르는 틀, 경로 밖 문서, 중복 path, `--write` 기존 문서 보호.

실행 기록(2026-10-04): `src/DesktopNode.Verification/TrainEvidence/`에 `TrainEvidenceRenderer.cs`(facts 계약, 틀 문법)와 `TrainEvidenceCommand.cs`(`--check`, `--write`, `--allow-update`, 결과 계약 `pcv-train-evidence-result-v1`)를 더하고 `VerificationApplication.RunAsync` 맨 앞에서 `train-evidence`를 분기했다. 기존 `verify` 문법과 summary 계약은 그대로다. 새 시험 `TrainEvidenceRendererTests` `24`개 통과. 설계의 오류 형식 문장을 실제 계약(`PCV_VERIFY_CONFIG_INVALID`, 별도 결과 계약)으로 고쳤다.

## Task 3: 틀과 0.42.89 golden

- [x] 0.42.89 문서 `12`개에서 틀을 만들고 facts 파일을 쓴다.
- [x] golden 시험: facts로 렌더한 결과가 커밋된 문서와 byte 단위로 같다. `pcvverify train-evidence --check` 통과.

실행 기록(2026-10-04): 0.42.89 문서 `12`개에서 틀 `docs/ga-ready/trains/templates/*.md.tmpl`과 facts `docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json`(값 `243`개)을 만들었다. 머리말 값은 자리표시자로 자동 변환하고(문서마다 고정 key는 literal), 본문은 train마다 바뀌는 서술 줄을 줄 단위 값으로, 표와 명령의 값을 앞뒤 문맥으로 바꿨다. current-card 틀은 `{{?installed_status}}` 선택 줄로 Lane 3 전후 머리말을 모두 낸다. 틀에 남은 literal 날짜·hash는 token rotation evidence(`2026-08-09`)와 functional carry-forward 기준(04275)뿐이다. golden 시험 `TrainEvidenceGoldenTests` `4`개 통과, `pcvverify train-evidence --check`는 `12/12` `current`.

## Task 4: 절차 반영과 종료

- [ ] `DEVELOPMENT_PROCEDURE.md` §10 train task에 렌더러 사용을 적는다. release train 설계 §9 2단계 상태.
- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Nonclaims

- facts 자동 채우기는 3단계 범위다.
- public trusted signing과 external stable publication을 주장하지 않는다.
