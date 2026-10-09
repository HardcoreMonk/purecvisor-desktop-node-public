---
name: pcv-ship
description: PureCVisor public 저장소 변경을 clean HEAD 종료 검증 → branch push → PR → Required CI 대기 → green이면 head SHA 고정 merge → 로컬 main 동기화 → merge 뒤 main run green 확인까지 진행한다(pcv-campaign-runner-v2 §2.3). campaign task가 push/PR/merge를 적었거나(push_allowed, merge_policy) 사용자가 "PR 열어", "green CI 뒤 merge", /pcv-ship을 부르면 사용한다. 승인 없는 push나 merge는 하지 않는다.
---

# PCV ship (push, PR, merge)

`$pub` = 이 저장소 루트(`purecvisor-desktop-node-public`). shell cwd가 곧 `$pub`이라 `git -C $pub`는 `git`과 같다. GitHub MCP 대신 `gh` CLI를 쓴다.

## 0. 권한

- campaign task에서 부를 때: push/PR은 `push_allowed=true`, merge는 `merge_policy=after-green-ci`일 때만.
- 사용자가 직접 부를 때: 그 메시지가 push/PR 또는 merge를 명시해야 한다. "PR 열어"는 merge 승인이 아니다.
- 승인 없이 `gh pr merge`를 부르면 auto mode classifier가 거부한다. 거부되면 우회하지 않고 멈춘다.

## 1. 종료 검증 (clean HEAD)

1. `git -C $pub status --short`가 비어 있어야 한다. 아니면 멈춘다.
2. 변경 범위에 맞춰 돈다. plan task가 명령을 적었으면 그것을 따른다.

   | 변경 | 명령 |
   | --- | --- |
   | docs, Delivery 계약 | `dotnet test src/DesktopNode.Delivery.Tests -c Release` |
   | C# | `dotnet test src/DesktopNode.sln -c Release` |
   | Web | `npm run test:required --prefix web` |
   | train·넓은 변경 종료 검증 | `dotnet build src/DesktopNode.sln -c Release`, Required CI 네 shard 로컬 실행, Pester 네 종. 위 셋은 shard가 이미 돌므로 따로 돌리지 않는다 |

   Required CI shard 로컬 실행(먼저 `dotnet build src/DesktopNode.sln -c Release`):

       dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path <변경 파일> --artifact-root artifacts/development-gates-<shard> --shard <dotnet|web|delivery|installer-policy>

   artifact root는 저장소 `artifacts/` 아래여야 한다. 실행 기록의 assembly별 시험 수는 dotnet shard
   `summary.json`의 `results[0].standard_output`에서 읽는다(`docs/DEVELOPMENT_PROCEDURE.md` §4).
   C# 시험은 Debug가 아니라 `-c Release`로 돈다. CI가 Release라 Debug만 통과한 변경은 `main`을 red로 만든다.
3. PR을 열기 전 이 호스트에서 PR gate를 돌린다. `summary.json` `ok=true`가 아니면 push하지 않는다.

       pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvPrGate.ps1

   Hyper-V 어댑터 코드(`src/DesktopNode.HyperV/**`)가 바뀌었고 campaign `approval_locator`에 ADR-0016 standing approval
   문장이 있으면 `PCV_HYPERV_INTEGRATION_APPROVAL`을 그 문장으로 두고 `-Integration`을 더한다(`pcv-it-` 접두사 일회용 VM
   생성·삭제, Lane 2). 문장이 없으면 gate가 exit 2로 거절하고, 그때는 `-Integration` 없이 돌린다.
4. `git -C $pub diff --check origin/main...HEAD`. red면 멈춘다.

## 2. branch와 PR

1. 현재 branch가 `main`이면 `lane<L>/<주제>-<yyyymmdd>` branch를 만든다. merge 안 된 다른 PR branch 위에 쌓지 않는다.
2. `git -C $pub push -u origin <branch>`.
3. PR 생성(제목은 conventional commit 형식, 본문은 한국어):

       gh pr create --base main --head <branch> --title "<type>: <요약>" --body-file - <<'EOF'
       ## 요약
       ## 검증
       ## 경계
       - host mutation, current-evidence.json 쓰기 여부, operational current
       - merge 권한(after-green-ci 또는 검토 뒤 별도 확인)

       <system이 지정한 PR attribution 줄>
       EOF

4. PR 번호와 head SHA(`gh pr view <n> --json headRefOid`)를 기록한다.

## 3. CI 대기와 merge

1. CI 대기를 background로 돌리고 완료 알림을 기다린다. polling하지 않는다. `/goal`이 걸려 있으면 background 대신
   같은 명령을 foreground(timeout `600000`)로 돌린다. 턴을 끝내면 평가기가 곧바로 다시 깨운다.

       gh pr checks <n> --watch --interval 30 > /dev/null 2>&1; gh pr checks <n>

2. Required check `dotnet`, `web`, `delivery`, `installer-policy`, `public-boundary-ci-required`가 모두 `pass`인지
   본다. CodeRabbit의 review skipped는 merge 조건이 아니다.
3. 하나라도 fail이나 cancel이면 merge하지 않는다. 실패 check 이름과 run URL을 보고하고 멈춘다(`verification-red`).
4. merge 권한이 있으면 저장소 관례대로 merge commit으로 합친다. release train PR(`train/` branch, 단일 PR train
   `pcv-single-pr-train-v1`)은 merge 직전 `git -C $pub fetch origin` 뒤
   `git -C $pub merge-base --is-ancestor origin/main <head SHA>`로 base가 최신인지 확인한다. 아니면 rebase하고
   push한 뒤 PR CI를 다시 기다린다.

       gh pr merge <n> --merge --match-head-commit <head SHA>

5. `git -C $pub switch main && git -C $pub pull --ff-only`. merge commit SHA를 기록한다.
6. 모든 merge에서 merge commit의 `main` push run(Development Gates, Public Boundary Contract)을 기다린다. PR run이
   green이어도 `main` run은 red일 수 있다(2026-10-09 S2 merge 뒤 Api 타이밍 flake). `/goal`이 걸려 있으면 foreground로 돈다.

       gh run list --branch main --event push --limit 2 --json databaseId,workflowName,headSha --jq '.[] | select(.headSha=="<merge SHA>") | .databaseId' | xargs -n1 gh run watch --exit-status

   red면 승인 문구 범위 안에서 그 merge를 되돌리는 revert PR을 열고 멈춘다. 범위 밖이면 실패 job 이름(`failed-test`
   줄 포함)과 run URL을 보고하고 멈춘다(`verification-red`). green이어야 merge가 끝난 것이다(`docs/DEVELOPMENT_PROCEDURE.md` §10).
7. merge 권한이 없으면 PR을 연 채 두고 "merge는 검토 뒤 별도 확인"으로 보고한다. campaign 안이면
   `next_approval_required`에 merge 문장을 적는다.

## 4. 보고

PR 번호·URL, check 결과, merge commit, merge 뒤 `main` run 결과, 로컬 `main` SHA, `$pub` git status, host mutation 여부.
마지막 줄은 `goal-evidence: pr=#<n> pr_state=<OPEN|MERGED> checks=<pass|fail:<이름>> merge=<sha|none> main_run=<success|failure|none> main=<sha> tree=<clean|dirty> stop=<none|원인>`이다.
