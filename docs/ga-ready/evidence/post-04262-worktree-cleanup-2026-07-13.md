# Post-0.42.62 worktree cleanup 2026-07-13

result: `PARTIAL_CLEANUP_RESIDUAL_PATHS_PRESERVED`
blocker: `windows-filename-too-long-after-worktree-registration-removal`
audited_worktree_count: `18`
cleanup_candidate_count: `15`
removed_worktree_count: `15`
preserved_worktree_count: `3`
fully_removed_worktree_path_count: `10`
registration_removed_residual_path_count: `5`
removed_branch_count: `9`
preserved_cleanup_branch_count: `5`
dirty_cleanup_candidate_removed_count: `0`
unique_commit_worktree_removed_count: `0`
force_removal_performed: `false`
host_mutation_performed: `false`
additional_package_candidate_opened: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 경계 및 판정

- repository root: `D:\data\projects\codex-zone\purecvisor-desktop-node`
- cleanup boundary: `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees`
- active follow-up: `C:\Users\Operator\.codex\worktrees\9181\purecvisor-desktop-node`

fresh audit에서 repository root, active follow-up, cleanup boundary 밖의 Codex worktree를
보존했다. boundary 내부에서는 `dirty_count=0`, `git cherry main <ref>`의 `+` entry가 0이고
main ancestor 또는 cherry-equivalent인 항목만 non-force `git worktree remove` 대상으로 삼았다.

15개 cleanup candidate는 Git worktree 등록에서 제거돼 최종 `git worktree list --porcelain`은
보존 대상 3개만 반환한다. 그중 10개 path는 완전히 제거됐고 branch 9개를 `git branch -d`로
삭제했다. detached worktree 1개에는 삭제할 branch가 없었다.

나머지 5개는 non-force remove가 `Filename too long`을 반환한 뒤 worktree 등록과 `.git` link만
제거되고 filesystem path가 남았다. 각 original branch와 residual path를 보존했으며 force,
`git branch -D`, 수동 재귀 삭제를 실행하지 않았다. 따라서 아래 표는 이 5개 path의 삭제를
claim하지 않는다.

## Fresh audit 및 action

| resolved path | branch | category | dirty | ancestor | cherry +/- | action | reason |
| --- | --- | --- | ---: | --- | --- | --- | --- |
| `D:\data\projects\codex-zone\purecvisor-desktop-node` | `main` | repository-root | 0 | true | 0/0 | preserve | repository root |
| `C:\Users\Operator\.codex\worktrees\3a2c\purecvisor-desktop-node` | `codex/development-gate-recovery-design` | external-path | 0 | true | 0/0 | preserve | outside cleanup boundary |
| `C:\Users\Operator\.codex\worktrees\9181\purecvisor-desktop-node` | `codex/post-04262-evidence-followup` | active-follow-up | 3 | true | 0/0 | preserve | active follow-up; dirty evidence edits |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04223-followup-hostmutation` | `codex/04223-followup-hostmutation` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04224-current-evidence-rollup` | `codex/04224-current-evidence-rollup` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04224-public-distribution-scope` | `codex/04224-public-distribution-scope` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04225-full-admin-campaign` | `codex/04225-full-admin-campaign` | clean-cherry-equivalent | 0 | false | 0/1 | registration-removed; path-and-branch-preserved | filename-too-long; no force |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04225-package-pair-current-evidence` | `codex/04225-package-pair-current-evidence` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04226-admin-smoke-campaign` | `codex/04226-admin-smoke-campaign` | clean-merged | 0 | true | 0/0 | registration-removed; path-and-branch-preserved | filename-too-long; no force |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04226-manual-admin-closure` | `codex/04226-postclosure-ledger-contract` | clean-cherry-equivalent | 0 | false | 0/1 | registration-removed; path-and-branch-preserved | filename-too-long; no force |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04227-hostops-lifecycle` | `codex/04227-hostops-lifecycle` | clean-merged | 0 | true | 0/0 | registration-removed; path-and-branch-preserved | filename-too-long; no force |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04227-postmerge-followups` | `codex/04227-postmerge-followups` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04227-pr150-postmerge-followups` | `codex/04227-pr150-postmerge-followups` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04228-manual-admin-package-pair` | `codex/04228-manual-admin-package-pair` | clean-merged | 0 | true | 0/0 | registration-removed; path-and-branch-preserved | filename-too-long; no force |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04228-operator-surface-admin-smoke` | `codex/04228-operator-surface-admin-smoke` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04229-baseline-rebuild` | `(detached)` | clean-merged | 0 | true | 0/0 | worktree-removed | detached clean ancestor; no branch |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\admin-smoke-04212-product-candidate` | `codex/admin-smoke-04212-product-candidate` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | clean ancestor |
| `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\post-04262-followup` | `codex/wmi-internal-switch-topology-recovery` | clean-merged | 0 | true | 0/0 | worktree-and-branch-removed | PR #171 merged ancestor |

cleanup은 repository filesystem과 Git metadata에만 한정되며 제품 host mutation을 수행하지
않았다. `0.42.63-admin-smoke` package candidate, public trusted signing 또는 외부 stable
publication을 claim하지 않는다.
