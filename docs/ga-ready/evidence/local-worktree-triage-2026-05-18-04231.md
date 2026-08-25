# Local Worktree Triage 2026-05-18 04231

evidence_id: `local-worktree-triage-2026-05-18-04231`
result: `PASS`
scope: `local-worktree-patch-equivalence-triage`
base: `origin/main`
root_branch: `codex/04231-pr155-postmerge-followup`
root_head: `2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f`
dirty_worktree_count: `0`
unmerged_unique_branch_count: `0`
patch_equivalent_delete_candidate_count: `13`
preserve_required_count: `0`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 triage는 `.worktrees/*`에 남아 있는 오래된 local codex worktree를
`git status --short`, `git merge-base --is-ancestor <branch> origin/main`,
`git cherry -v origin/main <branch>` 기준으로 분류했다. 모든 worktree는 clean이며
`git cherry` 기준 `+` unique commit이 없었다. 따라서 보존 또는 rebase 대상은 없고,
local cleanup pass에서 폐기 가능한 patch-equivalent 후보만 남았다.

## 결과

| worktree | branch | category | 근거 |
| --- | --- | --- | --- |
| `.worktrees/04223-followup-hostmutation` | `codex/04223-followup-hostmutation` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04224-current-evidence-rollup` | `codex/04224-current-evidence-rollup` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04224-public-distribution-scope` | `codex/04224-public-distribution-scope` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04225-full-admin-campaign` | `codex/04225-full-admin-campaign` | `patch-equivalent-delete-candidate-cherry-equivalent` | ancestor `false`, cherry `+0/-1`, dirty `0` |
| `.worktrees/04225-package-pair-current-evidence` | `codex/04225-package-pair-current-evidence` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04226-admin-smoke-campaign` | `codex/04226-admin-smoke-campaign` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04226-manual-admin-closure` | `codex/04226-postclosure-ledger-contract` | `patch-equivalent-delete-candidate-cherry-equivalent` | ancestor `false`, cherry `+0/-1`, dirty `0` |
| `.worktrees/04227-hostops-lifecycle` | `codex/04227-hostops-lifecycle` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04227-postmerge-followups` | `codex/04227-postmerge-followups` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04227-pr150-postmerge-followups` | `codex/04227-pr150-postmerge-followups` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04228-manual-admin-package-pair` | `codex/04228-manual-admin-package-pair` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/04228-operator-surface-admin-smoke` | `codex/04228-operator-surface-admin-smoke` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |
| `.worktrees/admin-smoke-04212-product-candidate` | `codex/admin-smoke-04212-product-candidate` | `patch-equivalent-delete-candidate-merged` | ancestor `true`, cherry `+0/-0`, dirty `0` |

## 폐기/보존 판단

| bucket | count | 판단 |
| --- | ---: | --- |
| `patch-equivalent-delete-candidate-merged` | 11 | local worktree/branch cleanup pass에서 폐기 가능 |
| `patch-equivalent-delete-candidate-cherry-equivalent` | 2 | main에 같은 patch가 있으므로 cleanup pass에서 폐기 가능 |
| `preserve-dirty-worktree` | 0 | 보존 대상 없음 |
| `preserve-unmerged-unique` | 0 | rebase 또는 merge 대상 없음 |

이 문서는 destructive cleanup을 실행하지 않고 triage 결과만 고정한다. 실제 local
worktree removal은 별도 cleanup pass에서 `git worktree remove`와 local branch delete로
수행한다.
