# Admin Smoke 0.27.6 Evidence Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `0.27.6-admin-smoke` 실제 service/MSI/Hyper-V mutation evidence를 active docs에 반영한다.

**Architecture:** Artifact 원본은 `artifacts/routeparity-service-msi-hyperv-mutation-20260503-140824/**`에 유지하고, git-tracked 문서는 evidence pointer와 해석만 갱신한다. 문서는 latest installed full mutation smoke가 runtime dispatch boundary contract를 포함해 통과했음을 기록하되, `AllowUnsignedDev` admin-smoke라서 public trusted signing, stable publication, GA runtime promotion gate를 닫지 않는다고 명시한다.

**Tech Stack:** Markdown docs, Pester documentation guard, git diff guard.

---

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료
- 관리자 opt-in: 이미 사용자 요청으로 `0.27.6-admin-smoke` full mutation smoke를 실행했다.
- Artifact: `artifacts/routeparity-service-msi-hyperv-mutation-20260503-140824`
- GA 상태: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 유지.

## Files

- Modify: `follower.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/GUIDE.md`
- Modify: `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`
- Modify: `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md`

## Tasks

### Task 1: Evidence docs update

- [x] **Step 1: Update active status docs**

Record:

- `0.27.6-admin-smoke`
- artifact root `artifacts/routeparity-service-msi-hyperv-mutation-20260503-140824`
- commit `3178a62bcf22d00977bf564063befa8e2b2562a5`
- MSI SHA-256 `4485fc3aba902d38a5d1293e9231497ae5f35b4c0730d1815c8df561a67c009c`
- service-action, MSI lifecycle, installed Hyper-V API route smoke PASS
- installed runtime policy dispatch boundary exposes `native_probe_operations` and `helper-process-direct`
- final service `Running`, boot time unchanged, `pcv-spike-*` VM residual none
- evidence remains unsigned `AllowUnsignedDev` admin-smoke, not public trusted/stable/GA evidence

- [x] **Step 2: Keep old evidence as historical**

Do not delete older `0.27.1`, `0.27.3`, `0.27.4`, or `0.27.5` evidence references. Only change “latest” wording where needed.

### Task 2: Verification

- [x] **Step 1: Run docs guard**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: pass.

- [x] **Step 2: Run diff hygiene**

Run:

```powershell
git diff --check
```

Expected: exit 0.

- [x] **Step 3: Confirm no stale latest wording**

Run:

```powershell
rg "최신 설치본 route smoke evidence는 `0\\.27\\.5-admin-smoke`|latest.*0\\.27\\.5|0\\.27\\.1-admin-smoke.*latest" follower.md docs packaging
```

Expected: no stale latest wording.

### Task 3: Commit and push

- [x] **Step 1: Commit docs**

Run:

```powershell
git add docs packaging follower.md
git commit -m "Record 0.27.6 admin smoke evidence"
git push
```

## Completion Evidence

- Docs updated:
- `follower.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`, `packaging/windows-desktop-node/README.md`, `packaging/windows-desktop-node/installer/README.md`, and Phase 25 transition plan now reference `0.27.6-admin-smoke` evidence.
- Verification:
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed.
- `git diff --check` passed with no whitespace errors.
- Stale latest wording check confirms active latest wording no longer points at `0.27.5-admin-smoke`.
- Commit:
- 이 plan 문서를 포함한 documentation sync commit으로 push한다. 최종 commit hash는 git history와 세션 결과를 따른다.
