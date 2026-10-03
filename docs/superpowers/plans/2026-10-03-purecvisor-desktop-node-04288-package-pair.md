# 0.42.88 package pair와 managed delete Lane 2 검증 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** managed delete 디스크 정리(PR #30, merge `ed4a4fa`)를 담은 `0.42.88-admin-smoke`를 빌드한다. 그 package로 다음을 PASS로 만든다.
- manual-admin pair(`0.42.87 → 0.42.88`)
- fullgate
- installed current-card
- 실제 VM에서 `vm delete`가 디스크, checkpoint 차등 디스크, VM 디렉터리를 정리하고 `storage_cleanup`을 남기는지

Lane 3 승격은 하지 않는다.

**Architecture:** 0.42.87 pair(`2026-10-03-purecvisor-desktop-node-04287-package-pair.md`)의 Task 2~6 순서를 따른다. branch는 `lane2/04288-package-pair-20261003`(`origin/main` `ed4a4fa` 기준)이고 commit은 로컬에 쌓는다.

**Tech Stack:** WiX installer `build.ps1`, `msiexec`, `Invoke-PcvDesktopNodeProduct.ps1`, manual-admin runner, `Invoke-PcvBatchSupervisor.ps1`, PCVCLI

## 사용자 결정 (2026-10-03)

승인 원문: `1,2` (managed delete 최종 보고의 다음 승인 1 "push/PR/merge", 2 "새 package pair와 실제 VM delete 확인(Lane 2)"). 1은 PR #30 merge `ed4a4fa`로 끝났다.

| 항목 | 결정 |
| --- | --- |
| 호스트 작업 범위 | MSI 제거·설치, 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX, fullgate, current-card, Lane 2 probe VM의 생성·checkpoint·삭제 |
| 승인 밖 | 이 branch의 push/PR/merge, Lane 3 승격, `current-evidence.json` 쓰기, public trusted signing, external stable publication |

## Global Constraints

- 보존 VM의 전원, Notes, 디스크를 바꾸지 않는다.
- 새 VM은 clean-host runner의 `pcv-cleanhost-*`와 Task 5 probe VM뿐이다. Task 5는 정리를 손으로 하지 않고 제품 delete 결과를 관측한다. 남으면 그 사실을 기록한 뒤 손으로 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다.
- evidence는 새 파일로만 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, clean-host 180분.
- runner 입력 함정(memory `lane2-pair-runner-gotchas`)과 0.42.87 run의 순서를 따른다. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`를 쓴다.
- 같은 원인으로 3번 실패하거나, 범위 밖 설계가 필요하거나, 권한이 거부되면 멈춘다.

## Task 1: `0.42.88-admin-smoke` package (Lane 1)

- [x] clean HEAD에서 `build.ps1 -Version 0.42.88-admin-smoke -MsiProductVersion 0.42.88 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261003-04288`. update ZIP, evidence `admin-smoke-package-2026-10-03-04288`.

실행 기록(2026-10-03): clean HEAD `ff62e59`, exit `0`, `41`초. MSI `81ef8527…`, payload `4377cc25…`, Host `f797af94…`, CLI `91e43cbe…`, provenance == HEAD, update ZIP `7051e735…`, Upgrade `VersionMax=0.42.88` 포함. evidence `admin-smoke-package-2026-10-03-04288`.

## Task 2a: pair readiness (Lane 2)

- [x] baseline `0.42.87`(clean package `admin-smoke-package-20261003-04287`)과 target `0.42.88`로 readiness `-PlanOnly`.

실행 기록(2026-10-03): 설치본이 baseline `0.42.87`(fullgate build `+8ade930`)이라 설치 변경 없이 실행했다. campaign `manual-admin-campaign-20261003-04287-04288`, `ok=true`, `ready-current-baseline-target-package-pair`, 설치본 일치, baseline MSI `a0041c9f…`, target MSI `81ef8527…`, host mutation 없음. summary SHA `e9533680…`.

## Task 2f: installed runtime ops summary (Lane 2)

- [x] baseline `0.42.87`이 설치된 동안 ops summary를 캡처한다.

실행 기록(2026-10-03): CLI exit `0`, `ok=true`, 비인증 `401 PCV_AUTH_REQUIRED`, Web `200`, errors `0`, VM `1`개, token 형태 문자열 `0`개. summary SHA `a0d095d3…`. evidence `installed-runtime-ops-summary-2026-10-03-04287`.

## Task 2b: 설치본 update/rollback (Lane 2)

- [x] `0.42.88` payload로 Update 뒤 Rollback한다.

실행 기록(2026-10-03): Update(`11`단계)와 Rollback(`7`단계) 모두 exit `0`, `ok=true`. update 직후 Host `+ff62e59`, Web `200`. 최종 manifest `0.42.87`, failed `0.42.88`, service Running, 재부팅·VM 변화 없음. evidence `product-update-rollback-2026-10-03-04287-04288`.

## Task 2c: dedicated clean-host Windows Update (Lane 2)

- [x] baseline `0.42.87` clean MSI, target `0.42.88` update ZIP으로 clean-host runner를 실행한다.

실행 기록(2026-10-03): 약 `137`초, exit `0`. base `current-base`(UBR `5622`), Windows Update 대상 `0`개. install, update, rollback 모두 exit `0`, 최종 manifest `0.42.87`, Web `200`, VM 삭제됨. evidence `internal-clean-host-install-update-rollback-smoke-2026-10-03-04287-04288`.

## Task 2d: Burn (Lane 2)

- [x] 제품 Update로 `0.42.88`에 맞추고 Burn lifecycle runner를 실행한다.

실행 기록(2026-10-03): 제품 Update로 `0.42.88`에 맞추고 15초 기다린 뒤 실행했다. 첫 실행 명령은 와일드카드 이동·삭제 단계가 도구에서 막혀 아무것도 실행되지 않았고(설치본 `0.42.87` 그대로 확인), 그 단계를 뺀 명령으로 다시 실행했다. build, install, repair, remove, 복구 모두 exit `0`(`31`초). 최종 ARP `0.42.88` `{5B28DA82-…}` 1개, Host `+ff62e59`, Running/Automatic, Web `200`. evidence `burn-bootstrapper-lifecycle-smoke-2026-10-03-04288`.

## Task 2e: MSIX (Lane 2)

- [x] MSIX lifecycle runner를 `0.42.87 → 0.42.88`로 실행한다.

실행 기록(2026-10-03): 세 자리 버전으로 한 번에 PASS(`19`초). pack·sign·verify `0`, install·update·remove 통과, smoke 패키지와 서비스 없음, manifest `0.42.88` 유지. evidence `msix-package-lifecycle-smoke-2026-10-03-04287-04288`.

## Task 2g: pair descriptor (Lane 2, non-mutating)

- [x] 여섯 bucket summary로 descriptor를 `-PlanOnly`로 만든다.

실행 기록(2026-10-03): `overall_status=pass`, runner `6/6`, missing `0`, not-pass `0`, host mutation 없음, next candidate `0.42.88-admin-smoke`. evidence `manual-admin-campaign-descriptor-2026-10-03-04287-04288`.

## Task 3: fullgate (Lane 2)

- [x] clean `0.42.88`이 설치된 채로 fullgate를 시작한다. 사후 검사(build commit, 같은 version ARP 1개)가 통과해야 한다.

실행 기록(2026-10-03): clean `0.42.88` `{5B28DA82}` 위에서 시작했다. batch `ok=true`, 두 step attempt `1`, MSI 6 phase exit `0`, 약 `273`초. 같은 version major upgrade, 사후 검사 둘(build `47ff198`, ARP `{3F60088B}` 1개) 통과. route smoke의 `vm delete` 결과 `storage_cleanup`이 `disk0.vhdx`와 `Snapshots`/`Virtual Machines`/root를 지웠고 retained 없음. evidence `full-admin-host-mutation-gate-2026-10-03-04288-hostmutation`.

## Task 4: installed current-card (Lane 2)

- [ ] `0.42.88`을 설치된 채로 두고 current-card를 캡처한다. 결과는 `installed_non_promoted_candidate`다.

## Task 5: managed delete 디스크 정리 Lane 2 확인 (Lane 2)

- [ ] probe VM을 만들고 checkpoint 하나를 만든 뒤 `vm delete --yes`를 실행한다.
- [ ] job 결과 `storage_cleanup`의 `removed_files`에 `disk0.vhdx`와 checkpoint 차등 디스크가 있고, VM 디렉터리가 없어졌는지 확인한다. 손 정리 없이 VM 목록이 사전 상태와 같아야 한다.

## Task 6: 종료

- [ ] clean HEAD 종료 검증 뒤 campaign을 닫는다. `next_step`에 push/PR/merge와 Lane 3 승격이 승인 대상이라고 적는다.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 결과는 `installed_non_promoted_candidate`까지다.
- public trusted signing과 external stable publication을 주장하지 않는다.
