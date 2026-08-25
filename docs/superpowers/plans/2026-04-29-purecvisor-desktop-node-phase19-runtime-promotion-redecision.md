# PureCVisor Desktop Node Phase 19 Runtime Promotion Redecision Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Desktop Node Phase 19의 evidence-first `keep-spike` 재판정 결론을 active docs와 root boundary tests에 고정한다.

**Architecture:** 이 작업은 runtime code를 바꾸지 않는 documentation/test synchronization change다. `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`를 단일 입력으로 삼고, high-level docs와 Pester root boundary suite가 같은 Phase 19 decision marker와 GA 차단 gate를 가리키게 만든다.

**Tech Stack:** Markdown, PowerShell 7, Pester 5, git.

---

## File Structure

- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
  - Phase 19 spec 존재, `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`, GA 차단 gate, active docs 동기화를 검증한다.
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
  - Phase 19 이후 stale backlog wording과 high-level pass count 복제를 막는다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Phase 19 재판정 spec 진입점을 추가한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Phase 19 문서/test 변경 검증 기준을 추가하고 Phase 19 이후 상태를 설명한다.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Phase 19 상태 섹션으로 `keep-spike` 재확인과 GA 차단 gate를 기록한다.
- Modify: `follower.md`
  - Phase 19를 진행 중/완료 상태로 반영하고 다음 queue를 GA 차단 gate 중심으로 정리한다.
- Modify: `spikes/purecvisor-desktop-node/README.md`
  - Phase 19 재판정 결론, 충족/부분 충족/차단 gate를 요약한다.

## Task 1: Boundary Test로 Phase 19 계약 고정

**Files:**
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [x] **Step 1: Phase 19 spec 검증 테스트를 추가한다**

`spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`의 마지막 `It` block 뒤, 닫는 `}` 앞에 다음 test를 추가한다.

```powershell
    It 'documents the Phase 19 evidence-first keep-spike redecision' {
        $phase19SpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md'
        Test-Path -LiteralPath $phase19SpecPath | Should -BeTrue

        $phase19Spec = Get-Content -LiteralPath $phase19SpecPath -Raw
        $rootReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'spikes/purecvisor-desktop-node/README.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $verificationPolicy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw

        $phase19Spec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $phase19Spec | Should -Match 'DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike'
        $phase19Spec | Should -Match 'Signed release build evidence'
        $phase19Spec | Should -Match 'Elevated MSI lifecycle smoke'
        $phase19Spec | Should -Match 'Hyper-V lifecycle integration evidence'
        $phase19Spec | Should -Match 'Release/version policy'
        $phase19Spec | Should -Match '장기 운영 로그 정책 evidence'

        $rootReadme | Should -Match 'Phase 19'
        $rootReadme | Should -Match 'evidence-first-keep-spike'
        $releaseBoundary | Should -Match 'Phase 19'
        $releaseBoundary | Should -Match 'GA 제품 런타임으로 승격하지 않는다'
        $developerIndex | Should -Match 'Phase 19 제품 승격 재판정'
        $verificationPolicy | Should -Match 'Desktop Node Phase 19 제품 승격 재판정'
    }
```

- [x] **Step 2: 테스트가 아직 실패하는지 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: Phase 19 high-level docs 문구가 아직 없어서 `Failed: 1` 이상으로 실패한다.

## Task 2: Active Docs에 Phase 19 결론 반영

**Files:**
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `follower.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`

- [x] **Step 1: Developer index에 Phase 19 진입점을 추가한다**

`docs/DEVELOPER_INDEX.md`의 "Phase 18 update/rollback/config migration 확인" 행 바로 아래에 다음 행을 추가한다.

```markdown
| Phase 19 제품 승격 재판정 확인 | `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md` |
```

- [x] **Step 2: Verification policy 변경 유형 표에 Phase 19 행을 추가한다**

`docs/DEVELOPMENT_VERIFICATION_POLICY.md`의 "Desktop Node Phase 18 update/rollback/config migration 구현/문서 변경" 행 바로 아래에 다음 행을 추가한다.

```markdown
| Desktop Node Phase 19 제품 승격 재판정 문서/test 변경 | Desktop Node root boundary suite 필수 | `git diff --check` 필수 | signed release build, elevated MSI lifecycle, Hyper-V lifecycle, Event Log/provider 또는 JSONL 장기 운영 evidence는 별도 관리자 opt-in gate |
```

- [x] **Step 3: Verification policy 하단 상태 문장을 Phase 19로 갱신한다**

`docs/DEVELOPMENT_VERIFICATION_POLICY.md` 마지막 문단을 다음 문장으로 교체한다.

```markdown
Phase 19 이후에도 Desktop Node는 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 상태다. Phase 18의 manifest-first safe update/rollback/config migration 기본 구현과 관리자 update/rollback smoke는 충족 gate로 보지만, signed release build, elevated MSI lifecycle smoke, Hyper-V lifecycle integration, release/version policy, 장기 운영 로그 evidence는 GA 차단 gate로 남는다. Single Edge 릴리스 게이트와 Desktop Node GA 승격 판단은 계속 분리한다.
```

- [x] **Step 4: Public release boundary의 Phase 상태 섹션을 Phase 11-19로 확장한다**

`docs/PUBLIC_RELEASE_BOUNDARY.md`의 `## Phase 11-18 상태` 제목을 다음으로 바꾼다.

```markdown
## Phase 11-19 상태
```

같은 섹션 문단 끝에 다음 문장을 추가한다.

```markdown
Phase 19는 evidence-first 재판정으로 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지한다. DPAPI protected token, JSONL diagnostics/redaction, LAN preview policy, manifest-first update/rollback/config migration은 충족 gate로 보지만, signed release build evidence, elevated MSI lifecycle smoke, Hyper-V lifecycle integration evidence, release/version policy, 장기 운영 로그 evidence가 닫히기 전까지 GA 제품 런타임으로 승격하지 않는다.
```

- [x] **Step 5: follower queue를 Phase 19 이후 상태로 바꾼다**

`follower.md`의 "다음 우선순위" 섹션을 다음 내용으로 교체한다.

```markdown
## 다음 우선순위

1. Signed release build와 MSI lifecycle evidence
  - `packaging/windows-desktop-node/installer/build.ps1 -SigningMode RequireSigned -SigningTrustModel <LocalTest|InternalEnterprise|PublicTrusted>`를 signing secret이 준비된 환경에서 검증한다.
   - elevated PowerShell에서 `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` uninstall smoke를 실행한다.
   - 결과는 Phase 14 또는 후속 signed release plan의 완료 증거에 기록한다.

2. Hyper-V lifecycle integration evidence
   - 지원 Windows Hyper-V host에서 VM create/start/poweroff/checkpoint/remove lifecycle을 실행한다.
   - 실패 중단 후 재실행 복구와 job store 상태 일관성을 확인한다.

3. Release/version policy
   - Desktop Node release channel, version naming, artifact naming, upgrade/downgrade support, rollback compatibility 범위를 확정한다.
   - `0.x-dev`, admin smoke version, signed release version의 의미를 분리한다.

4. Windows operational evidence
   - JSONL first 장기 rotation/retention evidence를 만들거나 Windows Event Log writer/provider 전환 여부를 결정한다.
   - service failure action 실제 적용과 recovery evidence를 기록한다.
   - LAN listener + reverse proxy/TLS smoke는 계속 administrator opt-in preview로 유지한다.

5. Repo-root 정리
   - 현재는 phase 이력 보존을 위해 `spikes/purecvisor-desktop-node/**` 경로를 유지한다.
   - 후속 PR에서 `src/api`, `src/cli`, `src/hyperv`, `src/service`, `web`, `packaging/windows-desktop-node` 같은 제품 repo 구조로 이동할지 결정한다.
   - 경로 이동은 product wrapper `SourceRoot` 계약과 Pester suite를 함께 갱신할 때만 수행한다.
```

- [x] **Step 6: follower 현재 상태 bullet을 Phase 19로 갱신한다**

`follower.md`의 "현재 상태" bullet 목록에 다음 bullet을 추가한다.

```markdown
- Phase 19는 evidence-first 재판정으로 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지한다고 결론냈다.
```

- [x] **Step 7: Root component README의 제품 승격 판단 섹션에 Phase 19를 추가한다**

`spikes/purecvisor-desktop-node/README.md`의 Phase 18 결정 문장 뒤에 다음 문장을 추가한다.

```markdown
2026-04-29 Phase 19 결정은 `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`이며, Phase 12-18 evidence를 충족/부분 충족/GA 차단 gate로 재분류하고 Desktop Node 전체 GA 승격은 계속 보류한다.
```

같은 "제품 승격 판단" 섹션의 gate 목록을 다음 목록으로 교체한다.

```markdown
- signed release build evidence와 elevated MSI install/repair/uninstall/`REMOVE_DATA=1` flow 검증
- Hyper-V 실제 VM create/start/poweroff/checkpoint/remove lifecycle integration evidence
- Desktop Node release channel, version naming, artifact naming, upgrade/downgrade support, rollback compatibility 정책 확정
- JSONL first 장기 운영 evidence 또는 Windows Event Log writer/provider 전환 결정과 source lifecycle evidence
- service failure action 실제 적용, recovery, log retention evidence
- Single Edge release gate와 Desktop Node release gate의 CI/문서 분리 유지
```

- [x] **Step 8: 문서 변경 뒤 boundary suite가 통과하는지 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: `Failed: 0`.

## Task 3: Documentation Sync Guard 강화

**Files:**
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`

- [x] **Step 1: stale Phase 19 backlog wording 방지 테스트를 추가한다**

`PcvDesktopNode.DocumentationSync.Tests.ps1`의 Phase 15 stale wording test 뒤에 다음 test를 추가한다.

```powershell
    It 'prevents stale Phase 19 backlog wording in active documents' {
        $forbiddenPatterns = @(
            'Phase 19: Desktop Node 제품 승격 재판정\s*\n\s*- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지할지, 별도 GA gate로 전환할지 결정한다',
            'signed release build, MSI smoke, update/rollback, diagnostics, LAN/TLS, Hyper-V integration gate를 다시 판정한다',
            'Phase 19 후보: 제품 승격 재판정'
        )

        foreach ($relativePath in $script:HighLevelDocs) {
            $path = Join-Path $script:RepoRoot $relativePath
            $content = Get-Content -LiteralPath $path -Raw

            foreach ($pattern in $forbiddenPatterns) {
                $content | Should -Not -Match $pattern
            }
        }
    }
```

- [x] **Step 2: Sync guard를 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

Expected: `Failed: 0`.

## Task 4: 최종 검증과 커밋

**Files:**
- Modify: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision.md`

- [x] **Step 1: 전체 root boundary suite를 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: `Failed: 0`.

- [x] **Step 2: whitespace 검증을 실행한다**

Run:

```powershell
git diff --check
```

Expected: exit code `0`.

- [x] **Step 3: 변경 파일을 확인한다**

Run:

```powershell
git status --short
```

Expected: 다음 파일만 modified 또는 added 상태다.

```text
M docs/DEVELOPER_INDEX.md
M docs/DEVELOPMENT_VERIFICATION_POLICY.md
M docs/PUBLIC_RELEASE_BOUNDARY.md
M follower.md
M spikes/purecvisor-desktop-node/README.md
M spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1
M spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1
M docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision.md
```

- [x] **Step 4: 계획 완료 증거를 이 plan에 기록한다**

이 파일 하단에 다음 형식의 `## 완료 증거` 섹션을 추가한다.

````markdown
## 완료 증거

Phase 19 문서/test 동기화는 `main`에서 완료했다.

구현 범위:

- Phase 19 evidence-first `keep-spike` 재판정 결론을 active docs에 반영했다.
- Root boundary suite가 Phase 19 spec, decision marker, GA 차단 gate, high-level docs 동기화를 검증한다.
- Documentation sync guard가 stale Phase 19 backlog wording과 high-level pass count 복제를 막는다.

검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
# Passed

git diff --check
# exit 0
```
````

- [x] **Step 5: 커밋한다**

Run:

```powershell
git add docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md follower.md spikes/purecvisor-desktop-node/README.md spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1 spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1 docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision.md
git commit -m "docs: synchronize Desktop Node phase 19 redecision"
```

Expected: commit succeeds.

## Self-Review

- Spec coverage: Phase 19 spec의 문서/test 동기화 범위를 Task 1-4가 모두 다룬다.
- Placeholder scan: 이 계획은 미완료 표식 없이 실행 가능한 파일 경로와 명령을 포함한다.
- Type/path consistency: 모든 경로는 `purecvisor-desktop-node` 저장소 기준 상대 경로다.

## 완료 증거

Phase 19 문서/test 동기화는 `main`에서 완료했다.

구현 범위:

- Phase 19 evidence-first `keep-spike` 재판정 결론을 active docs에 반영했다.
- Root boundary suite가 Phase 19 spec, decision marker, GA 차단 gate, high-level docs 동기화를 검증한다.
- Documentation sync guard가 stale Phase 19 backlog wording과 high-level pass count 복제를 막는다.
- Root README 관련 문서 목록을 Phase 17-19까지 이어지도록 갱신했다.

검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
# Tests Passed: 12, Failed: 0

git diff --check
# exit 0
```
