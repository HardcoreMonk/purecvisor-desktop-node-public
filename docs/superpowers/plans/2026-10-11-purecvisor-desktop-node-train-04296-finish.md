# train 0.42.96 마무리(lane3-spec 수정과 두 PR 승격) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.42.96-admin-smoke`를 operational current로 승격한다. 지금 train branch(Task 0~7·9 evidence, Lane 3 없음)를 PR A로 merge하고, `pcvverify lane3-spec`이 `index_sections.previous_version`을 pair baseline 대신 직전 승격 version으로 쓰게 고친 PR B를 merge한 뒤, `main`에서 Lane 3 PR C로 `current-evidence.json`을 쓴다. 끝에 completion 판정을 기록하고 승인 3으로 S4 campaign을 연다.

**Architecture:** 보류 기록은 `docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-train-04296.md` Task 8과 backlog BL-0021이다. train `0.42.94`·`0.42.95`가 정차해 직전 operational current는 `0.42.93`인데 생성기가 pair baseline `0.42.95`를 써서 승격 문서 도구 index 단계가 `previous-heading-mismatch`로 거절했다. 두 PR 방식은 train `0.42.92`(PR #62 + #63)의 선례를 따른다. Lane 3 PR C의 `main-push` 문서는 PR A merge의 main push run을 인용한다. payload commit `87a7deb` 뒤 `main`의 `src/` 변경은 PR A의 SHA pin 네 줄(`train-path-check` `allowed_pin_paths`)과 PR B의 `src/DesktopNode.Verification`(+ 시험)뿐이고, 둘 다 MSI payload(`DesktopNode.Host`와 그 참조 Api·HyperV·Service, web, wrapper)에 들지 않는다. 이 사실을 `main-push` 문서의 path·product note에 적는다. branch는 PR A가 지금 train branch `lane1/train-04296-20261011`, PR B가 `lane1/lane3-spec-skipped-promotion-20261011`, PR C가 `lane3/train-04296-promotion-20261011`이다.

**Tech Stack:** `pcvverify`(`train-path-check`, `train-evidence`, `lane3-spec`, `completion`), `Invoke-PcvLane3PromotionDocs.ps1`, `Update-PcvCurrentEvidenceDocs.ps1`, `Update-PcvContractSpecPins.ps1`, xUnit(`DesktopNode.Verification.Tests`, `DesktopNode.Delivery.Tests`), Pester, `gh`

## 사용자 결정 (2026-10-11)

승인 원문: `1,2,3` (campaign `train-04296-20261011` Task 8 보류 보고의 다음 승인 1~3에 대한 답).

| 항목 | 범위 |
| --- | --- |
| 1 | `Lane 1 수정 PR: TrainLane3SpecBuilder가 index_sections.previous_version을 pair baseline 대신 직전 승격 spec(previous_spec)의 version(직전 operational current)으로 쓰게 고치고, 승격을 건너뛴 train(0.42.93 → 0.42.96) 골든 시험을 더한다. push/PR, green CI 뒤 merge.` Task 2·3 |
| 2 | `train 0.42.96을 두 PR로 마무리: 지금 train branch(Task 0~7·9 evidence와 기록, Lane 3 없음, train running)를 push/PR, green CI 뒤 merge하고, 1번 merge 뒤 main에서 Lane 3 PR(functional carry-forward, consume, 두 PR용 main-push 문서, current-card promoted-current, current-evidence.json 쓰기, lane3-spec, 승격 문서 도구, release-train.json 승격)을 push/PR, green CI 뒤 merge한다. merge 뒤 main push가 red면 revert PR. host mutation 없음.` Task 1, 4~6. Lane 3 `current-evidence.json` 쓰기 |
| 3 | `train 뒤 S4 campaign, 그 뒤 legacy-retirement campaign(2026-10-10 승인 2·3 그대로). criteria S3 닫기와 C5 확인(Task 12, 2026-10-19 이후) 이관은 S4 campaign 첫 PR에서 한다.` Task 7이 S4 campaign을 연다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정, disk는 artifacts 아래(이 campaign은 host mutation이 없다) |
| 이관 | `train-04296-20261011` Task 8(Lane 3) → Task 4·5, Task 10(train PR merge) → Task 1, Task 11(판정과 후속 campaign) → Task 7, Task 12(C5 runner 확인, `not_before` 2026-10-19) → Task 8 |

## Global Constraints

- host mutation 없음. 설치본 `0.42.96-admin-smoke`(train fullgate build)와 보존 VM `pcv-guest-installed-04253-r1`, template `pcv-it-s2-source`는 건드리지 않는다.
- `current-evidence.json`은 Task 4·5(Lane 3)에서만 쓴다. 그 전 기록의 operational current는 `0.42.93-admin-smoke`다.
- PR A merge 뒤에는 train evidence 문서를 덮어쓰지 않는다. Lane 3 산출물은 새 파일로 더한다.
- token, credential 값은 명령줄·summary·evidence에 남기지 않는다. 사용자 홈 경로·LAN IP·호스트명을 문서에 적지 않는다.
- 한도: Lane 1 30분·tool batch 18회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 1: train branch 종료 검증과 PR A merge

- [x] clean HEAD에서 `dotnet build src/DesktopNode.sln -c Release`, Required CI shard 네 개 로컬, Pester 네 종, `npm run test:public-source-safety --prefix web`(기존 BL-0015 두 건 외 신규 0), `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `train-path-check --payload 87a7deb --head HEAD` exit `0`(허용 pin 네 개)을 돌린다. push, PR 하나, green CI 뒤 merge(`--match-head-commit`, merge 직전 base가 `origin/main`인지 확인)한다. merge 뒤 main push run(Development Gates, Public Boundary)을 기다려 run id를 기록하고, red면 revert PR을 열고 멈춘다. 실행 기록은 Task 2 commit에 적는다.

실행 기록(2026-10-11): PR #87 merge 뒤 train branch에 `origin/main`을 merge commit `9d6ea64`로 합쳤다(rebase하지 않음). clean HEAD `9d6ea64`에서 `dotnet build -c Release` 경고 0, PR gate `artifacts/pr-gate/20261010-185835` passed, Pester packaging 528·installer 49·web 50·manual-admin 137 통과, `Update-PcvCurrentEvidenceDocs.ps1 -Check` current, public source safety는 기존 BL-0015 두 건 외 신규 0. `train-path-check --payload 952fbd4 --head HEAD`(PR A 자기 범위) `ok=true`, `changed_path_count=52`, `product_paths` 없음, `allowed_pin_paths` 4. payload `87a7deb` 기준은 `ok=false`이고 `product_paths`는 PR #87의 `src/DesktopNode.Verification` 세 파일뿐이다. PR #88 Required check 다섯 개 pass, base 최신 확인 뒤 `--match-head-commit 9d6ea64`로 merge, merge commit `d5b8618`. main push run Public Boundary `38078392505` success, Development Gates `38078392575` success.

재배치(2026-10-11): 첫 종료 검증(HEAD `85a0f86`)에서 Pester는 packaging 528·installer 49·web 50·manual-admin 137 통과였지만 PR gate `dotnet-test-release`가 `DesktopNode.Verification.Tests` 18개 실패로 red였다(`artifacts/pr-gate/20261010-184221`). completion 평가기 시험 입력 `Cleared()`가 실제 `release-train.json`의 queue만 비우고 trains는 그대로 두어, 이 branch의 train `0.42.96` `running`이 C2를 열어 둔다(PR #83 red와 같은 원인). PR A는 `src/` 시험을 고칠 수 없으므로(허용 pin 밖) Task 2·3(PR B)을 먼저 하고 이 task는 Task 3 뒤에 한다. 그때 `origin/main`을 이 branch에 merge commit으로 합친다. rebase하지 않는 이유는 evidence가 인용한 train commit(fullgate build 등)을 `main`에서 도달하게 두기 위해서다. `train-path-check`는 PR A 자기 범위(`--payload <merge-base>`)로 보고, payload `87a7deb` 기준 결과도 함께 적는다.

## Task 2: lane3-spec previous_version 수정과 골든 시험

- [x] `main`에서 branch를 만든다. `src/DesktopNode.Verification/TrainEvidence/TrainLane3SpecBuilder.Rows.cs`의 `index_sections.previous_version`을 직전 승격 spec(`previous_spec`)의 version으로 채운다. pair 관련 값(`pair`, descriptor chain)은 pair baseline 그대로 둔다. 승격을 건너뛴 train(직전 승격 `0.42.93`, pair baseline `0.42.95`, 새 version `0.42.96`)의 골든 시험을 `DesktopNode.Verification.Tests`에 더하고, 기존 골든 시험은 바뀌지 않음을 확인한다. `dotnet test src/DesktopNode.Verification.Tests -c Release`. Task 1 실행 기록과 함께 로컬 commit.

실행 기록(2026-10-11): branch `lane1/lane3-spec-skipped-promotion-20261011`(`origin/main` `87a7deb`에서) commit `fc2c501`. `TrainLane3SpecBuilder.Rows.cs`의 `index_sections.previous_version`을 직전 승격 spec의 `index_sections.installed_version`으로 바꿨다(pair 값은 baseline 그대로). 새 시험 `IndexSectionsContinueFromThePreviousPromotedSpecWhenAPromotionWasSkipped`는 0.42.89 facts의 pair baseline을 0.42.87로 바꾼 임시 root에서 `previous_version=0.42.88`, pair `0.42.87 -> 0.42.89`를 확인하고, 옛 줄로 되돌리면 `0.42.87`로 실패한다. 골든 재생성(0.42.89~0.42.93 spec)은 바뀌지 않았다. 범위 추가로 `ProjectCompletionEvaluatorTests` `Cleared()`가 승격되지 않은 train을 뺀다. train branch의 `release-train.json`(`0.42.96` `running`)을 넣고 돌린 completion 시험 31/31 통과. clean HEAD `dotnet test src/DesktopNode.Verification.Tests -c Release` 674/674 통과. tooling·test 전용이라 release-train queue 행은 더하지 않는다.

범위 추가(2026-10-11, Task 1 재배치 근거): 같은 PR에서 completion 평가기 시험 입력 `Cleared()`(`src/DesktopNode.Verification.Tests/ProjectCompletionEvaluatorTests.cs`)가 승격되지 않은 train을 빼게 해, 시험이 실제 저장소의 진행 중 train에 묶이지 않게 한다. 시험 전용 변경이고 평가기 동작은 바꾸지 않는다. 확인은 train branch처럼 `running` train이 있는 `release-train.json`에서도 `DesktopNode.Verification.Tests`가 통과하는 것이다.

## Task 3: 수정 PR B merge

- [x] clean HEAD에서 `dotnet build src/DesktopNode.sln -c Release`, Required CI `dotnet`·`delivery` shard 로컬, push, PR, green CI 뒤 merge한다. 실행 기록은 Task 4 commit에 적는다.

실행 기록(2026-10-11): clean HEAD `fc2c501`에서 `dotnet build src/DesktopNode.sln -c Release` 경고 0, PR gate `artifacts/pr-gate/20261010-185419`(`dotnet-test-release`, `module-size-ratchet` passed). PR #87 Required check 다섯 개(`dotnet`, `web`, `delivery`, `installer-policy`, `public-boundary-ci-required`) pass 뒤 `--match-head-commit fc2c501`로 merge, merge commit `952fbd4`. host mutation 없음.

## Task 4: Lane 3 데이터와 lane3-spec (Lane 3)

- [x] `main`에서 branch를 만든다. consume manifest를 확인하고, functional carry-forward·pair consume·두 PR용 `main-push`(PR A merge의 main push run 인용, path·product note에 payload 뒤 `src/` 변경 두 묶음과 MSI 밖이라는 근거) facts를 쓴 뒤 `train-evidence --write`, current-card `promoted-current`(`--write --allow-update`), `--check`를 돌린다. `current-evidence.json`을 쓰고 `lane3-spec --write`로 spec을 만든 뒤 `index_sections.previous_version`이 `0.42.93-admin-smoke`인지 확인한다. Task 3 실행 기록과 함께 로컬 commit.

실행 기록(2026-10-11): branch `lane3/train-04296-promotion-20261011`(`main` `d5b8618`에서). 보류 때 만든 consume manifest(`artifacts/manual-admin-campaign-20261011-04295-04296/consume-manifest.json`)는 summary 7개의 SHA-256과 크기가 같아 다시 쓰지 않고 재사용했다. facts에 functional carry-forward, pair consume, 두 PR용 `main-push`(PR #88 merge `d5b8618`의 Public Boundary `38078392505` job `114290057289`, Development Gates `38078392575`; 첫 부모 `952fbd4`(PR #87)와의 범위는 허용 pin 4개뿐, payload 뒤 `src`는 그 pin과 PR #87 `pcvverify` 수정뿐)을 더하고 current-card를 `promoted-current`로 바꿨다. `train-evidence --write --allow-update <current-card>`와 `--check` ok, 새 문서 3개. `current-evidence.json` current `0.42.96-admin-smoke`(provenance `9ac8eb3`, clean MSI `326b867a…`, operational MSI `37bfc1ff…`), manual_admin `0.42.95 -> 0.42.96`. `release-train.json` train `0.42.96` `promoted`, operational current `0.42.96-admin-smoke`. `lane3-spec --write` ok, `index_sections.previous_version=0.42.93-admin-smoke`, pair `0.42.95 -> 0.42.96`.

## Task 5: 승격 문서와 release-train 승격 (Lane 3)

- [x] `Invoke-PcvLane3PromotionDocs.ps1` dry-run, `-Apply`, `-Check`를 돌리고 Lane 3 숨은 단계(descriptor chain, ledger 행, index 절, D2 시험, spec pin, current evidence 시험 version 줄)를 확인한다. `release-train.json`에서 train `0.42.96-admin-smoke`를 승격하고 operational current를 바꾼다. `Update-PcvCurrentEvidenceDocs.ps1 -Apply`/`-Check`, `Update-PcvContractSpecPins.ps1 -Check`, `dotnet test src/DesktopNode.Verification.Tests -c Release`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

실행 기록(2026-10-11): `Invoke-PcvLane3PromotionDocs.ps1 -SpecPath packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04296.json` dry-run ok(index 단계 planned, 보류 때의 `previous-heading-mismatch` 없음), `-Apply` ok(current evidence 문서, descriptor chain, ledger head·rows, index 절, spec pin 갱신), `-Check` 모든 단계 current. 숨은 단계: current evidence 시험은 version 정규식이라 손댈 줄이 없고, D2 시험·spec pin은 도구가 갱신했다(`Update-PcvContractSpecPins.ps1 -Check` current, spec 9개). 수기 갱신: `docs/DOCUMENTATION_INDEX.md` 권위 줄(payload `87a7deb`, 운영 `0.42.96-admin-smoke`, 설치본=operational), `docs/FEATURE_IMPLEMENTATION_LEDGER.md` operational 줄 두 개(이미 `0.42.92`·`0.42.91`로 낡아 있었다), 문서 현행화 때 적은 보류 표현(ADR_INDEX, ADR-0018, DEVELOPER_INDEX, DEVELOPMENT_PROCEDURE). `Update-PcvCurrentEvidenceDocs.ps1 -Check` ok, `dotnet test src/DesktopNode.Delivery.Tests -c Release` 781/781, `DesktopNode.Verification.Tests` 675/676(dirty tree의 PolicyBoundary 1건만 실패, commit 뒤 다시 본다).

## Task 6: Lane 3 PR C merge

- [x] clean HEAD에서 `dotnet build src/DesktopNode.sln -c Release`, Required CI shard 네 개 로컬, Pester 네 종, `npm run test:public-source-safety --prefix web` 신규 0. push, PR, green CI 뒤 merge한다. merge 뒤 main push run을 기다리고 red면 revert PR을 열고 멈춘다. 실행 기록은 Task 7 기록에 적는다.

실행 기록(2026-10-11): clean HEAD `4d765c5`에서 `dotnet build -c Release` 경고 0, PR gate `artifacts/pr-gate/20261010-191433` passed, Pester packaging 528·installer 49·web 50·manual-admin 137 통과, public source safety는 기존 BL-0015 두 건 외 신규 0. PR #89 Required check 다섯 개 pass, base 최신 확인 뒤 `--match-head-commit 4d765c5`로 merge, merge commit `d28b74a`. main push run Public Boundary `38079434392` success, Development Gates `38079434310` success. operational current는 `0.42.96-admin-smoke`다.

## Task 7: 판정과 S4 campaign 열기

- [x] clean `main`에서 `pcvverify completion`을 돌려 결과를 기록한다. exit `0`이면 감사 문서로 완료를 적고 push, PR, green CI 뒤 merge한다. 그 밖이면 이 campaign을 `status=closed`로 닫고(Task 8은 `carried_tasks`로 새 campaign에 옮김) 승인 3으로 S4 campaign을 `pcv-campaign-open`으로 연다. 그 campaign 첫 PR이 criteria S3 닫기와 Task 8 이관을 한다. S4의 설치본 `--allow-lan`/firewall 변경은 2026-10-10 승인 2의 범위(그 campaign의 기존 사전 승인 범위) 안에서만 한다.

실행 기록(2026-10-11): clean `main` `d28b74a`에서 `pcvverify completion`(`artifacts/completion/20261011/result.json`) exit `1`, `complete=false met=4/7 gaps=3`. 충족 C1·S1·S2·C6, 갭 `S3-scenario`(시연 PASS는 있으나 criteria S3가 아직 `open`), `S4-scenario`, `C5-risk-ubuntu-26-runner`(deadline-wait, 2026-10-19). hygiene C7은 backlog `undecided` 12행(BL-0011~BL-0022). 이 campaign을 닫고(Task 8은 `carried_tasks`로 이관) 승인 3으로 `s4-lan-account-20261011`을 열었다. S4 첫 PR이 criteria S3 닫기, C5 이관, S4 설계를 한다.

## Task 8: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `train-04296-20261011` Task 12(원래 `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR, green CI 뒤 merge.

이관(2026-10-11): `s4-lan-account-20261011` Task 2(`not_before` 2026-10-19), plan `docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-s4-lan-account.md`.

## Nonclaims

- operational current는 Task 6의 Lane 3 PR C가 `main`에 merge된 뒤에만 `0.42.96-admin-smoke`로 바뀐다.
- public trusted signing과 external stable publication을 주장하지 않는다. package는 `AllowUnsignedDev`/`LocalTest`다.
- criteria S3·S4 닫기, backlog 분류, legacy 콘솔 제거는 이 campaign의 산출물이 아니다.
