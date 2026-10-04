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

- [x] `packaging/windows-desktop-node/tools/New-PcvAdminSmokeUpdatePackage.ps1`: package root에서 update ZIP, admin-smoke update catalog, `package-facts.json`(provenance 요약, ZIP SHA와 항목, MSI Upgrade 행과 `RemoveExistingProducts` 순서, Host/CLI ProductVersion)을 만든다. MSI는 읽기 전용으로 연다.
- [x] Delivery 계약 시험. 로컬 확인: 0.42.88/0.42.89 package로 임시 출력 root에 만들고, ZIP 항목과 내용 hash가 손으로 만든 ZIP과 같은지, orchestrator `-PlanOnly`가 두 catalog를 받아들이는지 본다.

실행 기록(2026-10-04): `New-PcvAdminSmokeUpdatePackage.ps1`를 더했다. MSI는 Windows Installer automation으로 읽기 전용(open mode `0`) 연다. automation은 PowerShell이 감싼 인자와 빈 인자 배열을 `DISP_E_TYPEMISMATCH`로 거부해서, 기본 객체나 null로 넘긴다. 0.42.88과 0.42.89 package로 `artifacts/update-package-check-20261004/`에 만든 ZIP은 손으로 만든 ZIP과 항목 이름, 압축 방식, 내용 hash가 모두 같다. Upgrade 행(`VersionMax` 포함 `513`, `VersionMin` `2`)과 `RemoveExistingProducts` `1401`도 같다. orchestrator `-PlanOnly`가 두 catalog를 받아들였다(`ok=true`, host mutation 없음). Delivery 계약 시험 `PcvAdminSmokeUpdatePackageContractTests` `3`개 통과(Delivery `761`).

## Task 3: orchestrator 관측 기록

- [x] `Invoke-PcvManualAdminPackagePairCampaign.ps1` `-Execute`가 bucket 앞뒤 host 상태를 `observations.json`(`pcv-manual-admin-pair-observations-v1`)에 남긴다. Delivery 계약 시험 갱신.

실행 기록(2026-10-04): orchestrator `-Execute` 경로에 `Add-PcvObservation`을 더했다. 시작, baseline 정렬 뒤, bucket마다 앞뒤, update/rollback bucket의 Update와 Rollback 직후, restoration 뒤에 `observations.json`(`pcv-manual-admin-pair-observations-v1`)을 갱신한다. 항목은 manifest, Host ProductVersion, previous/failed version, service 상태와 시작 유형, Web 상태, VM, PureCVisor ARP다. 관측 실패는 `observation_error`로 남기고 던지지 않는다. 관측 함수만 떼어 실제 호스트에서 읽기 전용으로 돌려 값이 맞고 secret 형태 문자열이 없음을 확인했다. `-PlanOnly`는 관측 전에 끝나며 다시 돌려 `ok=true`, host mutation 없음. Delivery 계약 시험 `1`개 추가(Delivery `762`).

## Task 4a: 틀 일반화와 orchestrator 기록 보강

- [x] 수동 pair와 orchestrated pair에서 문장이 다른 줄을 값으로 옮긴다(package ZIP 문장, ops summary 캡처 문장과 설치 version, update/rollback 명령·단계 줄과 최종 표, 선택 최종 Update 절, clean-host runner 플래그·추가 인자·base 문장, Burn 선택 `preupdate_root`·runner 꼬리 줄·VM 칸, fullgate VM 칸). 0.42.89 facts에 옛 문장을 값으로 넣어 golden을 유지한다.
- [x] orchestrator ops bucket이 원본 출력(`ops-summary.json`), stderr byte, 비인증 거부(status, error code), errors 수, VM 수, token 형태 수를 남기고, 관측에 boot time과 PureCVisor firewall rule 수를 더한다.

실행 기록(2026-10-04): 틀 생성 스크립트에 줄 단위 값, 선택 머리말, 줄 삽입을 더해 틀 `7`개를 바꾸고 0.42.89 facts를 다시 만들었다(값 `254`개). golden 시험과 `train-evidence --check` `12/12` `current`. ops bucket 본문과 관측 함수만 떼어 실제 호스트에서 읽기 전용으로 돌렸다(`ok`, stderr `0` byte, errors `0`, VM `1`, token 형태 `0`, 비인증 `401 PCV_AUTH_REQUIRED`, boot time과 firewall `0`). Delivery 계약 시험 `1`개 추가.

## Task 4b: facts 생성기

- [ ] `pcvverify train-facts`: `package-facts.json`, orchestrator campaign root(bucket summary, `observations.json`, closed descriptor), fullgate batch, current-card summary, 사람이 쓰는 서술 값 파일을 읽어 pair 문서 facts를 만든다. 생성 값과 사람 값이 겹치면 실패한다. 틀의 literal이 사실인지(예: Upgrade 행 `513`/`1401`, 비인증 `401`, MSI log `0`/`0`) 확인하고 아니면 실패한다.
- [ ] Verification 단위 시험(합성 fixture).

## Task 5: 절차 반영과 종료

- [ ] `DEVELOPMENT_PROCEDURE.md` §10과 release train 설계 §9에 3a 도구를 적는다. clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Nonclaims

- 3a는 실제 orchestrator `-Execute`를 하지 않는다. 동작 확인은 3b 리허설이다.
- public trusted signing과 external stable publication을 주장하지 않는다.
