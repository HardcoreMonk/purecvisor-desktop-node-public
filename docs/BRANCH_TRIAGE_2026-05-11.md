# Branch triage - 2026-05-11

작성 기준: 2026-05-11

이 문서는 `purecvisor-desktop-node`의 남은 `codex/*` 브랜치를 역사 보존, 폐기 후보, 재사용 후보로 분류한다. 실제 remote branch 삭제, local branch 삭제, worktree 제거는 이 문서 작성 범위에서 실행하지 않는다.

## 기준

- 기준 main: `origin/main` at `b381a61` (`Merge pull request #92 from HardcoreMonk/codex/desktop-node-stabilize-split-implementation`)
- open PR: 없음
- 확인 명령:
  - `git fetch --all --prune`
  - `git branch -r --no-merged origin/main`
  - `git cherry -v origin/main <branch>`
  - `git branch -vv`
- 판단 원칙:
  - `git cherry` 결과가 `-`인 commit은 patch equivalent 상태로 본다. 이 경우 내용은 main에 반영됐거나 더 최신 형태로 흡수됐으므로 직접 merge하지 않는다.
  - 오래된 branch는 `origin/main` 대비 많은 파일을 삭제/되돌릴 수 있다. Patch가 유용해 보여도 branch merge 대신 필요한 commit/file만 선별 검토한다.
  - Public trusted signing, winget public submission, external stable publication 관련 branch는 ADR-0006 이후 적용 대상이 아니라 역사 기록으로만 둔다.

## 원격 미병합 branch

| Branch | Tip | 분류 | 조치 |
|--------|-----|------|------|
| `origin/codex/diagnostic-bundle-api-action` | `af545c9` `Add diagnostic bundle API evidence` | 폐기 후보 / 역사 보존 | `git cherry` 기준 patch equivalent. Diagnostic bundle API 구현과 evidence는 현재 main에 더 최신 상태로 존재한다. 직접 merge하면 최신 ADR-0006/GA-ready 문서를 되돌릴 위험이 있으므로 삭제 승인 대상이다. |
| `origin/codex/diagnostic-bundle-listener-evidence` | `c41e3e4` `Add diagnostic bundle listener evidence` | 폐기 후보 / 역사 보존 | Patch equivalent. Host listener diagnostic bundle evidence는 main에 흡수됐다. 직접 merge 금지. |
| `origin/codex/diagnostic-bundle-product-wrapper-evidence` | `67332e0` `Add diagnostic bundle product wrapper evidence` | 폐기 후보 / 역사 보존 | Patch equivalent. Product wrapper diagnostics evidence와 tests는 main에 흡수됐다. 직접 merge 금지. |
| `origin/codex/full-admin-host-mutation-0389-evidence` | `9b1d516` `Promote 0.38.9 full admin host mutation evidence` | 폐기 후보 / 역사 보존 | Patch equivalent. `0.38.9` evidence는 역사 기록으로 main에 남아 있고, 최신 canonical full admin evidence는 `0.41.5`다. 직접 merge 금지. |
| `origin/codex/installed-account-novnc-evidence` | `36dff00` `feat: add installed account smoke and novnc bridge` | 폐기 후보 / 선별 검토만 허용 | 핵심 account/noVNC commit은 patch equivalent다. 함께 남은 `119bc99 docs: close product tui plan`은 unique지만 main의 `product-tui-service-plan-closure-2026-05-10.md`가 더 최신 TUI test count와 installed smoke reference를 포함한다. Branch merge 없이 폐기 후보로 둔다. |
| `origin/codex/korean-docs-rewrite` | `d5038b6` `docs: start Korean documentation rewrite` | 재사용 후보 / 활성 PR 후보 | 현재 유일하게 바로 재사용할 수 있는 branch다. 1 commit이며 main 위에 깨끗하게 적용된다. PR 생성 후 merge 대상이다. |
| `origin/codex/lifecycle-rebaseline-0415` | `7232aec` `docs: sync manual admin campaign evidence` | 선별 재사용 후보 / 직접 merge 금지 | Unique commit이 있으나 branch가 main보다 많이 뒤처져 최신 API hardening, domain split, 한국어 문서 진입점 등을 되돌릴 수 있다. Manual admin wording이 필요하면 `7232aec`의 특정 문장만 현재 문서에 수동 반영한다. |
| `origin/codex/ops-mutation-hardening` | `ac78088` `feat: harden ops evidence and diagnostics` | 폐기 후보 / 역사 보존 | Patch equivalent. Ops hardening, diagnostic list pagination, service token rotation/revoke, installed load/rate-limit evidence는 main에 흡수됐다. 직접 merge 금지. |
| `origin/codex/public-ops-beta-followups` | `5fb4fb7` `Add public ops bundle and beta follow-up status` | 폐기 후보 / 역사 보존 | Patch equivalent. Public ops bundle과 beta follow-up status는 ADR-0006 이후 역사 evidence로 main에 남아 있다. 직접 merge 금지. |

## Local branch / worktree cleanup 후보

다음 local branch는 tracking remote가 `gone` 상태다. 대부분 tip이 `origin/main`의 ancestor라 내용은 main에 들어가 있다. 삭제는 별도 승인 후 `git worktree remove`와 `git branch -d` 순서로 처리한다.

| Local branch | 상태 | 조치 |
|--------------|------|------|
| `codex/credential-manager-default-transition` | main ancestor | local worktree/branch 제거 후보 |
| `codex/eventlog-default-writer-hardening` | patch equivalent, remote gone | local worktree/branch 제거 후보. Tip 자체는 main ancestor가 아니지만 `git cherry` 기준 patch equivalent다. |
| `codex/host-mutation-smoke-0412` | main ancestor | local branch 제거 후보 |
| `codex/host-mutation-smoke-0415` | main ancestor | local branch 제거 후보 |
| `codex/internal-private-network-boundary` | main ancestor | local branch 제거 후보 |
| `codex/manual-admin-batch-boundaries` | main ancestor | local branch 제거 후보 |
| `codex/manual-admin-campaign-0415` | main ancestor | local branch 제거 후보 |
| `codex/manual-admin-campaign-classification` | main ancestor | local branch 제거 후보 |
| `codex/manual-admin-campaign-version-guard` | main ancestor | local branch 제거 후보 |
| `codex/manual-admin-entrypoint-sync-0415` | main ancestor | local branch 제거 후보 |
| `codex/public-ops-final-seven` | main ancestor | local worktree/branch 제거 후보 |
| `codex/public-ops-gates-implementation` | main ancestor | local worktree/branch 제거 후보 |
| `codex/public-ops-installed-hardening` | main ancestor | local worktree/branch 제거 후보 |
| `codex/sync-all-docs` | main ancestor | local worktree/branch 제거 후보 |

## 권장 정리 순서

1. `origin/codex/korean-docs-rewrite` PR을 생성하고 merge한다.
2. `origin/codex/lifecycle-rebaseline-0415`의 `7232aec`에서 필요한 문구가 남았는지 현재 `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`와 비교한다. 없으면 폐기 후보로 내린다.
3. 나머지 원격 미병합 branch는 remote 삭제 승인 목록으로 묶는다.
4. Remote 삭제 후 local `gone` worktree와 branch를 제거한다.

## 삭제 승인 전 확인 command

아래 command는 아직 실행하지 않았다. 삭제 승인 시에도 먼저 dry-run 성격의 목록 확인을 다시 수행한다.

```powershell
git branch -r --no-merged origin/main
git branch -vv
git worktree list
```

Remote branch 삭제 후보:

```text
origin/codex/diagnostic-bundle-api-action
origin/codex/diagnostic-bundle-listener-evidence
origin/codex/diagnostic-bundle-product-wrapper-evidence
origin/codex/full-admin-host-mutation-0389-evidence
origin/codex/installed-account-novnc-evidence
origin/codex/ops-mutation-hardening
origin/codex/public-ops-beta-followups
```

조건부 삭제 후보:

```text
origin/codex/lifecycle-rebaseline-0415
```

유지/PR 후보:

```text
origin/codex/korean-docs-rewrite
```
