# Release train 3단계 3a Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** pair orchestrator를 train에 쓰기 전에 필요한 Lane 1 변경을 만든다. update ZIP·catalog·package facts 도구, orchestrator host 관측 기록, train evidence facts 생성기다. host mutation은 하지 않는다.

**Architecture:** 설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3a(이 campaign에서 accepted, 보정 포함). branch `lane1/train-pair-orchestrator-3a-20261004`(`origin/main` `01da41e` 기준), PR 하나로 merge한다.

**Tech Stack:** PowerShell 7 packaging 도구, C# (.NET 10) `DesktopNode.Verification`, xUnit, Delivery C# 계약 시험

## 사용자 결정 (2026-10-04)

승인 원문: `PR #38 merge, 3a 구현 승인`. PR #38은 merge `01da41e`로 끝났다. 3a는 설계 §6 승인 단위대로 Lane 1 구현, commit, PR, merge까지다. 3b 리허설(host mutation)과 3c 절차 변경은 승인 밖이다.

## Global Constraints

- host mutation 없음. orchestrator는 `-PlanOnly`만 돌린다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다. 시험은 C#(Delivery 계약, Verification 단위)이다.
- 0.42.89 golden(`TrainEvidenceGoldenTests`)은 그대로 통과해야 한다. 틀을 일반화할 때 0.42.89 facts에 옛 literal을 값으로 옮긴다.
- token, guest 비밀번호는 명령줄, artifact, evidence에 남기지 않는다.
- 한도: checkpoint마다 Lane 1 30분·tool batch 18회.

## Task 1: 계획과 설계 확정

- [x] 이 계획, campaign `train-pair-orchestrator-3a-20261004`, 설계 상태 `accepted`와 §3a 보정(facts 생성기를 C#으로, MSI 표는 package 도구가 읽는다, 틀은 새 이름 대신 일반화).

## Task 2: update ZIP, catalog, package facts 도구

- [ ] `packaging/windows-desktop-node/tools/New-PcvAdminSmokeUpdatePackage.ps1`: package root에서 update ZIP, admin-smoke update catalog, `package-facts.json`(provenance 요약, ZIP SHA와 항목, MSI Upgrade 행과 `RemoveExistingProducts` 순서, Host/CLI ProductVersion)을 만든다. MSI는 읽기 전용으로 연다.
- [ ] Delivery 계약 시험. 로컬 확인: 0.42.88/0.42.89 package로 임시 출력 root에 만들고, ZIP 항목과 내용 hash가 손으로 만든 ZIP과 같은지, orchestrator `-PlanOnly`가 두 catalog를 받아들이는지 본다.

## Task 3: orchestrator 관측 기록

- [ ] `Invoke-PcvManualAdminPackagePairCampaign.ps1` `-Execute`가 bucket 앞뒤 host 상태를 `observations.json`(`pcv-manual-admin-pair-observations-v1`)에 남긴다. Delivery 계약 시험 갱신.

## Task 4: facts 생성기

- [ ] `pcvverify train-facts`: `package-facts.json`, orchestrator campaign root, fullgate batch, current-card summary, 사람이 쓰는 서술 값 파일을 읽어 pair 문서 facts를 만든다. 틀은 orchestrated 실행도 표현하도록 일반화하고 0.42.89 golden을 유지한다.
- [ ] Verification 단위 시험(합성 fixture).

## Task 5: 절차 반영과 종료

- [ ] `DEVELOPMENT_PROCEDURE.md` §10과 release train 설계 §9에 3a 도구를 적는다. clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Nonclaims

- 3a는 실제 orchestrator `-Execute`를 하지 않는다. 동작 확인은 3b 리허설이다.
- public trusted signing과 external stable publication을 주장하지 않는다.
