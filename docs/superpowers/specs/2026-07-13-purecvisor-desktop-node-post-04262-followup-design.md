# PureCVisor Desktop Node Post-0.42.62 Follow-up Design

## 상태와 목표

`0.42.62-admin-smoke`는 WMI association 기반 internal switch topology 복구, package
build, full admin host mutation gate, 설치본 Web/TUI/CLI current-card까지 PASS했다. 그러나
current evidence ledger 하단 표, manual-admin package-pair, post-merge public-boundary,
provider-level WMI 회귀 검증, CI 중복 실행, 추가 호스트 smoke, 과거 worktree 정리가 남아
있다.

이 후속의 목표는 새 제품 기능을 추가하는 것이 아니라 다음 여덟 항목을 순서대로 닫는
것이다.

1. current evidence ledger 내부 일관성 복구
2. PR #171 검증·ready 전환·병합
3. post-merge main push public-boundary evidence 승격
4. `0.42.59-admin-smoke -> 0.42.62-admin-smoke` manual-admin package-pair 판정
5. WMI association traversal provider-level 회귀 테스트
6. 두 번째 Hyper-V 환경의 비파괴 topology smoke
7. 기능 브랜치 push와 PR 이벤트의 GitHub Actions 중복 제거
8. clean/merged/patch-equivalent worktree 정리

## 선택한 접근

현재 PR #171을 확장한 뒤 병합하는 접근 A를 사용한다. PR #171에는 ledger guard, WMI
provider 회귀 테스트, CI trigger 수정처럼 병합 전에 검증해야 하는 변경만 추가한다. 제품
binary payload는 바꾸지 않으므로 `0.42.62` package/full-gate provenance
`7f71f0a518c5b592f233373522d36b5401c3f1df`를 다시 만들지 않는다.

PR 병합 후에는 main push CI를 관찰해 public-boundary evidence를 별도 docs-only 후속으로
기록한다. Manual-admin, 추가 호스트 smoke, worktree cleanup은 서로 상태를 공유하지 않는
운영 단계로 분리하되, 사용자 승인 범위 안에서 순차 실행한다.

다른 대안은 채택하지 않는다. PR #171을 먼저 병합하고 모든 수정을 후속 PR로 넘기는
접근은 current ledger 불일치를 main에 남긴다. 항목별로 PR을 완전히 분리하는 접근은 동일
SHA에 이미 중복된 CI 비용을 더 늘린다.

## Pre-merge 변경 경계

### Evidence ledger와 guard

`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md` 상단의 `0.42.62` current anchor와 하단 current
table이 같은 package, installed current-card, manual-admin pair를 가리키도록 정렬한다.
Package/full-gate current와 manual-admin current는 서로 다른 속도로 움직이므로 한 행에
섞지 않는다. Package/current-card는 `0.42.62`, manual-admin은 마지막 closed pair인
`0.42.58 -> 0.42.59`를 유지한다. `current_manual_admin_update_zip_sha256`처럼 소유자가
혼동될 수 있는 필드는 마지막 closed manual-admin pair에 속함을 이름과 설명으로 명시한다.

`packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`에 상단 metadata와
current table의 버전/evidence 경로가 일치하는지 검사하는 회귀 계약을 먼저 추가한다. 기존
불일치에서 RED를 확인한 뒤 ledger를 최소 수정해 GREEN으로 만든다.

### WMI provider 회귀 경계

기존 `MapSwitch` 단위 테스트는 topology 판정만 확인하므로 `ManagementObject.Path`와
`GetRelated` association traversal 실패를 잡지 못했다. Provider가 사용하는 WMI query와
association 판독을 작은 내부 seam으로 분리하고, 실제 production mapping 경로를 거치는
테스트를 추가한다. 새 seam은 PowerShell fallback이나 Linux runtime 경계를 열지 않는다.

테스트 행렬은 다음 계약을 소유한다.

- 완전한 switch object path와 internal management port가 있으면 임의 이름 switch도
  `internal`, `allow_management_os=true`다.
- external host resource binding이 하나라도 있으면 internal로 승격하지 않는다.
- private 또는 topology proof가 없는 switch는 기존 structured incomplete 경계를 유지한다.
- 빈 object path나 association read 예외는 성공으로 삼키지 않고
  `PCV_NETWORK_INVENTORY_FAILED` 계열로 정규화된다.
- production query는 association traversal에 필요한 완전한 object projection을 유지한다.

테스트는 실패하는 provider-level case를 먼저 추가하고, seam과 최소 구현을 추가한 뒤 전체
.NET solution을 재검증한다.

### GitHub Actions 중복 제거

`.github/workflows/development-gates.yml`과 `.github/workflows/public-boundary.yml`은 현재
`pull_request`와 `push: codex/**`를 모두 수신해 같은 PR head SHA를 두 번 실행한다. 두
workflow 모두 `pull_request`, `push: main`, `workflow_dispatch`를 유지하고 기능 브랜치
`push: codex/**`만 제거한다. required check 이름과 job matrix는 바꾸지 않는다.

`PcvDevelopmentGateWorkflow.Tests.ps1`과 public-boundary 문서 guard에 trigger 계약을 먼저
추가한다. 현재 workflow에서 RED를 확인한 뒤 YAML을 수정한다. 이 설계는 PR 검증, main
post-merge 검증, 수동 재실행을 보존하면서 중복 branch-push run만 제거한다.

## PR landing 흐름

Pre-merge 변경은 서로 독립된 작은 커밋으로 PR #171에 push한다. 로컬 전체 .NET, 관련
Pester, Web/installer 검증과 `git diff --check`를 통과한 뒤 원격 필수 체크가 모두 SUCCESS인지
확인한다. PR은 그 뒤에만 draft에서 ready로 전환하고, main과 mergeable 상태를 다시 확인한
후 merge한다. Force-push와 main 직접 push는 사용하지 않는다.

## Post-merge public-boundary

PR merge가 생성한 main head의 `Public Boundary Contract`와 `Development Gates` 완료를
기다린다. 성공한 run ID, job ID, head SHA, workflow 이름을 새 post-merge evidence에 기록하고
AGENTS, evidence index, control-plane index, current ledger의 public-boundary current를 같은
head로 승격한다.

이 단계는 docs-only다. 제품 payload 변경이 없으므로 `0.42.63-admin-smoke` package 후보,
full admin host mutation, manual-admin package-pair를 재귀적으로 열지 않는다. Docs-only
후속은 별도 `codex/` 브랜치와 PR을 사용한다.

## Manual-admin package-pair

목표 pair는 마지막 closed baseline인 `0.42.59-admin-smoke`에서 current target
`0.42.62-admin-smoke`로의 skip-version update/rollback이다. 현재 개발 호스트는 이미
`0.42.62`이므로 downgrade하지 않는다. 전용 baseline host 또는 통제된 clean-host VM이
확보된 경우에만 다음 bucket을 실행한다.

- rebaseline readiness
- installed update/rollback
- clean-host Windows Update install/update/rollback
- Burn install/repair/remove
- MSIX build/install/update/remove
- installed runtime ops summary
- descriptor generation v2

모든 bucket이 PASS이고 descriptor `missing_count=0`, `not_pass_count=0`일 때만 closed current로
승격한다. 전용 baseline 자원, baseline artifact, 권한 중 하나라도 없으면 현재 호스트를
변형해 우회하지 않고 정확한 blocker evidence를 생성한다. Token, password, credential 값은
artifact에 기록하지 않는다.

## 추가 Hyper-V 호스트 smoke

두 번째 Hyper-V 환경이 발견되면 read-only network inventory와 설치본 current-card만
실행한다. Default, WSL, private, external switch의 이름과 topology 결과를 확인하되 VM 생성,
switch 생성/삭제, firewall, trust store, Event Log, service config mutation은 하지 않는다.
가용한 두 번째 호스트가 없으면 single-host PASS를 multi-host PASS로 확대 해석하지 않고
`blocked-no-secondary-hyperv-host` 상태와 발견 절차만 기록한다.

Account/noVNC 및 Guest Execution/QoS provider/control payload는 이번 후속에서 바뀌지 않으므로
해당 destructive actual-VM smoke는 재실행하지 않는다.

## Worktree cleanup

`git worktree list --porcelain`의 각 항목을 다시 감사한다. main checkout, 현재 PR branch,
Codex가 사용 중인 worktree, dirty worktree, untracked file이 있는 worktree, main에 병합되지
않은 unique commit이 있는 branch는 보존한다. 제거 대상은 다음 조건을 모두 만족해야 한다.

- resolved path가 이 저장소의 의도된 `.worktrees` 경계 안에 있다.
- worktree status가 clean이다.
- branch commit이 main 또는 보존된 원격 branch에 포함된다.
- historical triage의 patch-equivalent 판정이 현재 diff에서도 유지된다.

각 대상을 `git worktree remove`로 하나씩 제거한 뒤 merged local branch만 `git branch -d`로
삭제한다. `Remove-Item -Recurse`, force delete, `git reset --hard`, force-push는 사용하지
않는다. 제거 조건을 만족하지 않는 항목은 목록과 보존 이유만 보고한다.

## 오류 처리와 중단 조건

- RED 테스트가 기대한 이유로 실패하지 않으면 구현하지 않고 test seam을 수정한다.
- PR CI가 실패하면 merge하지 않고 실패 job의 최초 원인만 조사한다.
- main post-merge CI가 실패하면 public-boundary PASS evidence를 작성하지 않는다.
- Manual-admin bucket 일부가 실패하면 descriptor를 closed로 만들지 않는다.
- 관리자 권한이나 secondary host가 없으면 자동 우회하지 않고 blocker로 종료한다.
- Worktree 감사 중 dirty 또는 unique commit을 발견하면 해당 항목은 삭제하지 않는다.

## 완료 기준

1. Ledger metadata/current table 일치 계약이 RED/GREEN으로 검증된다.
2. WMI provider-level path/association 회귀 계약이 RED/GREEN으로 검증된다.
3. 기능 branch push는 중복 CI를 만들지 않고 PR과 main push 필수 체크는 유지된다.
4. PR #171이 필수 CI SUCCESS 상태로 merge된다.
5. merge head의 post-merge public-boundary evidence가 별도 docs-only PR로 제출된다.
6. Manual-admin pair는 closed PASS 또는 정직한 blocker evidence 중 하나로 닫힌다.
7. 추가 호스트 smoke는 PASS 또는 `blocked-no-secondary-hyperv-host`로 과장 없이 기록된다.
8. 안전 조건을 만족한 worktree만 제거되고 보존 대상은 이유와 함께 남는다.

Public trusted signing과 external stable publication은 계속 별도 release/ADR 범위이며 이
후속의 완료 조건이 아니다.
