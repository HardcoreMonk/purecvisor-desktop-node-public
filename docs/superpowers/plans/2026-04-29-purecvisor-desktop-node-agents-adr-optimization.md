# Desktop Node AGENTS/ADR 최적화 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node 전용 경량 ADR 체계를 추가하고 `AGENTS.md`를 ADR/문서 진입점 중심으로 정리한다.

**Architecture:** `docs/ADR_INDEX.md`를 현재 설계 결정 진입점으로 만들고, `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`가 저장소 분리와 Phase 19 keep-spike 재판정을 요약한다. `AGENTS.md`와 active docs는 상세 결정을 반복하지 않고 ADR index와 phase spec을 가리킨다.

**Tech Stack:** Markdown, PowerShell 7, Pester 5, git.

---

## File Structure

- Create: `docs/ADR_INDEX.md`
  - Desktop Node ADR 목록, 현재 적용 상태, 결정 마커, ADR 작성 규칙을 제공한다.
- Create: `docs/adr/0000-template.md`
  - 새 Desktop Node ADR의 표준 양식이다.
- Create: `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
  - standalone Windows repo, keep-spike, evidence-first Phase 19 재판정의 현재 적용 결정을 기록한다.
- Modify: `AGENTS.md`
  - ADR index를 문서 진입점에 추가하고 상세 phase 설명을 ADR/roadmap으로 위임한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - ADR index를 "먼저 볼 문서"와 저장소 결정 섹션에 추가한다.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - 공개 릴리스 경계와 ADR index의 관계를 추가한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - AGENTS/ADR 문서 변경 검증 행을 추가한다.
- Modify: `spikes/purecvisor-desktop-node/README.md`
  - 관련 문서 목록에 ADR index와 첫 ADR을 추가한다.
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
  - ADR index와 첫 ADR의 결정 마커, AGENTS/active docs 링크를 검증한다.
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
  - ADR index와 첫 ADR을 high-level doc scan 대상에 포함한다.

---

### Task 1: Add Failing ADR Boundary Tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`

- [ ] **Step 1: Add ADR boundary assertions**

Append this Pester block inside `Describe 'Desktop Node runtime promotion boundary'` in `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`, after the existing Phase 19 test:

```powershell
    It 'documents the Desktop Node ADR index and current decision source' {
        $adrIndexPath = Join-Path $script:RepoRoot 'docs/ADR_INDEX.md'
        $adrTemplatePath = Join-Path $script:RepoRoot 'docs/adr/0000-template.md'
        $adr0001Path = Join-Path $script:RepoRoot 'docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md'
        $agentsPath = Join-Path $script:RepoRoot 'AGENTS.md'
        $developerIndexPath = Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md'
        $releaseBoundaryPath = Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md'

        Test-Path -LiteralPath $adrIndexPath | Should -BeTrue
        Test-Path -LiteralPath $adrTemplatePath | Should -BeTrue
        Test-Path -LiteralPath $adr0001Path | Should -BeTrue

        $adrIndex = Get-Content -LiteralPath $adrIndexPath -Raw
        $adr0001 = Get-Content -LiteralPath $adr0001Path -Raw
        $agents = Get-Content -LiteralPath $agentsPath -Raw
        $developerIndex = Get-Content -LiteralPath $developerIndexPath -Raw
        $releaseBoundary = Get-Content -LiteralPath $releaseBoundaryPath -Raw

        $adrIndex | Should -Match 'DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index'
        $adrIndex | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrIndex | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $adrIndex | Should -Match 'DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike'
        $adrIndex | Should -Match '0001-standalone-windows-repo-and-evidence-first-keep-spike'

        $adr0001 | Should -Match '상태: 적용 중'
        $adr0001 | Should -Match 'Linux `purecvisor-single`'
        $adr0001 | Should -Match 'GA 제품 런타임으로 승격하지 않는다'
        $adr0001 | Should -Match 'Signed release build evidence'
        $adr0001 | Should -Match 'Elevated MSI lifecycle smoke'
        $adr0001 | Should -Match 'Hyper-V lifecycle integration evidence'

        $agents | Should -Match 'docs/ADR_INDEX.md'
        $agents | Should -Match 'docs/adr/'
        $developerIndex | Should -Match 'docs/ADR_INDEX.md'
        $releaseBoundary | Should -Match 'docs/ADR_INDEX.md'
    }
```

- [ ] **Step 2: Add ADR files to high-level synchronization scan**

In `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`, extend `$script:HighLevelDocs` with these entries:

```powershell
            'docs/ADR_INDEX.md',
            'docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md',
```

Place them after `docs/PUBLIC_RELEASE_BOUNDARY.md` so ADR docs are scanned with other active documentation.

- [ ] **Step 3: Run root documentation suite and verify expected failure**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: FAIL because `docs/ADR_INDEX.md`, `docs/adr/0000-template.md`, and `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md` do not exist yet.

- [ ] **Step 4: Commit failing tests**

Run:

```bash
git add spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1 spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1
git commit -m "test: guard Desktop Node ADR documentation"
```

---

### Task 2: Add Desktop Node ADR Documents

**Files:**
- Create: `docs/ADR_INDEX.md`
- Create: `docs/adr/0000-template.md`
- Create: `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`

- [ ] **Step 1: Create `docs/ADR_INDEX.md`**

Create the file with this content:

````markdown
# Desktop Node ADR 인덱스

> 대상: `purecvisor-desktop-node` Windows 전용 저장소

```text
DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index
```

이 문서는 Desktop Node 저장소에서 현재 적용 중인 설계 결정의 진입점이다. Phase spec과 plan은 상세 설계와 이력을 보존하고, ADR은 현재 적용되는 결정과 변경 시 확인해야 할 검증 기준을 짧게 고정한다.

Linux `purecvisor-single`의 ADR은 이 저장소의 단일 진실이 아니다. Desktop Node 결정은 이 인덱스와 `docs/adr/` 아래의 Desktop Node ADR을 우선한다.

## 현재 적용 중인 ADR

| ADR | 상태 | 결정 | 관련 문서 |
|-----|------|------|-----------|
| `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md` | 적용 중 | 독립 Windows 저장소, GA 승격 보류, evidence-first keep-spike | Phase 11, Phase 12-18, Phase 19 spec |

## 결정 마커

```text
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike
DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper
DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper
DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first
DESKTOP_NODE_PHASE15_TOKEN_STORAGE_DECISION: dpapi-local-machine-protected-file-first
DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred
DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required
DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
```

## ADR 작성 규칙

- 새 설계 결정이 Desktop Node 공개 경계, 제품 승격 gate, installer/service/update/security policy를 바꾸면 ADR을 추가하거나 기존 ADR을 supersede한다.
- Phase spec은 상세 설계와 구현 계획을 담고, ADR은 현재 적용 결정과 영향 범위를 담는다.
- ADR 상태는 `제안`, `적용 중`, `대체됨`, `폐기됨` 중 하나를 사용한다.
- ADR 변경 후에는 `spikes/purecvisor-desktop-node/tests` root documentation suite와 `git diff --check`를 실행한다.

## 관련 진입점

- `AGENTS.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
````

- [ ] **Step 2: Create `docs/adr/0000-template.md`**

Create the file with this content:

````markdown
# ADR-0000: 제목

- 상태: 제안
- 날짜: YYYY-MM-DD
- 결정 마커: `DESKTOP_NODE_EXAMPLE_DECISION: example`

## 맥락

결정이 필요한 배경과 기존 제약을 설명한다.

## 결정

선택한 방향을 명확히 적는다.

## 근거

왜 이 선택이 현재 저장소 경계와 제품 gate에 맞는지 설명한다.

## 영향 범위

- 포함 경로:
- 제외 경로:
- 운영 또는 검증 영향:

## 대안

검토했지만 선택하지 않은 대안과 이유를 적는다.

## 검증 기준

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

## 관련 문서

- `docs/ADR_INDEX.md`
````

- [ ] **Step 3: Create `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`**

Create the file with this content:

````markdown
# ADR-0001: 독립 Windows 저장소와 evidence-first keep-spike

- 상태: 적용 중
- 날짜: 2026-04-29
- 결정 마커:
  - `DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`
  - `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
  - `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`

## 맥락

`purecvisor-desktop-node`는 Windows Desktop Node 전용 저장소다. Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN Single Edge runtime은 이 저장소의 구현 대상이 아니다.

Phase 12-18은 Service-first wrapper, WinSW service host, WiX MSI-first installer, DPAPI LocalMachine protected token file, JSONL first diagnostics, LAN preview security policy, manifest-first update/rollback/config migration을 추가했다. Phase 19는 이 증거를 다시 평가했지만, GA 제품 런타임 승격에 필요한 release evidence는 아직 닫히지 않았다.

## 결정

Desktop Node는 독립 Windows 저장소로 유지한다. Desktop Node 전체는 2026-04-29 기준 GA 제품 런타임으로 승격하지 않는다.

`packaging/windows-desktop-node/**`는 Service-first/WinSW/MSI/protected-token/diagnostics/LAN-security/safe-update 제품 후보 배포 계층이다. `spikes/purecvisor-desktop-node/**`는 component 구현 원천과 검증 경계다.

Single Edge release gate와 Desktop Node GA 승격 판단은 분리한다.

## 충족된 제품화 gate

- DPAPI LocalMachine protected token file을 제품 wrapper 기본 bearer token source로 둔다.
- Diagnostic bundle은 raw token, protected token blob, token hash, host absolute path를 redaction한다.
- LAN mode는 loopback 기본값과 preview/admin opt-in 정책을 유지한다.
- Update/rollback/config migration은 manifest-first safe update 정책과 단일 previous slot을 사용한다.

## GA 차단 gate

다음 증거가 닫히기 전에는 Desktop Node를 GA 제품 런타임으로 승격하지 않는다.

- Signed release build evidence
- Elevated MSI lifecycle smoke
- Hyper-V lifecycle integration evidence
- Release/version policy
- JSONL first 장기 운영 evidence 또는 Windows Event Log writer/provider 전환 evidence
- Single Edge release gate와 Desktop Node release gate의 CI/문서 분리 유지

## 대안

### Linux ADR 복사

선택하지 않는다. Linux ADR은 Single Edge runtime과 C service 경계를 다루며, Desktop Node Windows product wrapper 결정의 단일 진실이 아니다.

### Phase 11-19 전체 개별 ADR 분해

지금은 선택하지 않는다. 이미 phase spec과 plan이 상세 이력을 보존하므로, 현재 적용 결정을 빠르게 찾는 경량 ADR이 더 적합하다.

## 영향 범위

- 포함 경로:
  - `spikes/purecvisor-desktop-node/**`
  - `packaging/windows-desktop-node/**`
  - `docs/**`
- 제외 경로:
  - Linux `purecvisorsd`
  - Linux Single Edge UI/API
  - KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- 운영 영향:
  - signed build, elevated MSI lifecycle, 실제 Hyper-V lifecycle은 계속 관리자 opt-in gate다.

## 검증 기준

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
````

- [ ] **Step 4: Run root documentation suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: PASS for the new ADR existence and decision marker assertions, or fail only because active docs do not yet point to ADR index.

- [ ] **Step 5: Commit ADR docs**

Run:

```bash
git add docs/ADR_INDEX.md docs/adr/0000-template.md docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md
git commit -m "docs: add Desktop Node ADR index"
```

---

### Task 3: Optimize AGENTS and Active Docs

**Files:**
- Modify: `AGENTS.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`

- [ ] **Step 1: Update `AGENTS.md`**

Replace the "저장소 경계" decision bullets with:

```markdown
- 단일 진실: 이 저장소는 Windows Desktop Node 전용이다.
- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime 코드를 추가하지 않는다.
- 현재 코드 경로는 phase 이력과 테스트 계약을 보존하기 위해 `spikes/purecvisor-desktop-node/**`와 `packaging/windows-desktop-node/**`를 유지한다.
- 현재 적용 결정은 `docs/ADR_INDEX.md`와 `docs/adr/`를 우선한다.
```

Add these entries to "문서 진입점" before the phase roadmap:

```markdown
- ADR 현재 적용 상태 인덱스: `docs/ADR_INDEX.md`
- 설계 결정 단일 진실: `docs/adr/`
```

Add this decision block under the document entrypoints:

````markdown
## 현재 핵심 결정

```text
DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
```

Phase별 상세 결정은 `docs/ADR_INDEX.md`, phase roadmap, 관련 phase spec을 따른다.
````

- [ ] **Step 2: Update `docs/DEVELOPER_INDEX.md`**

Add this row near the top of the "먼저 볼 문서" table:

```markdown
| 현재 적용 ADR 확인 | `docs/ADR_INDEX.md` |
```

Replace the paragraph after the repository decision block with:

```markdown
이 저장소는 Linux `purecvisor-single` 저장소와 분리되어 있으며 Windows Desktop Node 코드와 문서만 포함한다. 현재 적용되는 설계 결정의 진입점은 `docs/ADR_INDEX.md`다.
```

- [ ] **Step 3: Update `docs/PUBLIC_RELEASE_BOUNDARY.md`**

Add this sentence after the decision block:

```markdown
현재 적용 결정의 진입점은 `docs/ADR_INDEX.md`이며, 이 문서는 공개 릴리스 경계와 금지 표면을 요약한다.
```

- [ ] **Step 4: Update `docs/DEVELOPMENT_VERIFICATION_POLICY.md`**

Add this row to the verification table after "Desktop Node runtime promotion decision 또는 root boundary 문서 변경":

```markdown
| Desktop Node AGENTS/ADR index/ADR 문서 변경 | Desktop Node root boundary suite 필수 | `git diff --check` 필수 | 제품 승격 전 관리자 권한 integration gate 별도 설계 필요 |
```

Add this bullet to "Desktop Node 문서 동기화 규칙":

```markdown
- 현재 적용 결정은 `docs/ADR_INDEX.md`와 `docs/adr/`에 연결한다.
```

- [ ] **Step 5: Update `spikes/purecvisor-desktop-node/README.md`**

Add these entries at the start of "관련 문서":

```markdown
- `docs/ADR_INDEX.md`
- `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
```

- [ ] **Step 6: Run root documentation suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 7: Commit active doc synchronization**

Run:

```bash
git add AGENTS.md docs/DEVELOPER_INDEX.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/DEVELOPMENT_VERIFICATION_POLICY.md spikes/purecvisor-desktop-node/README.md
git commit -m "docs: point Desktop Node active docs to ADR index"
```

---

### Task 4: Final Verification and Push

**Files:**
- Verify all changed files.

- [ ] **Step 1: Run root documentation suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: all tests pass.

- [ ] **Step 2: Run whitespace verification**

Run:

```bash
git diff --check
```

Expected: exit code 0 with no output.

- [ ] **Step 3: Inspect status and recent commits**

Run:

```bash
git status --short --branch
git log --oneline -5
```

Expected: branch is ahead of `origin/main` by the new AGENTS/ADR commits and has no uncommitted changes.

- [ ] **Step 4: Push**

Run:

```bash
git push origin main
```

Expected: push succeeds.
