# CLAUDE.md

PureCVisor Desktop Node — Windows 전용 내부 서비스(Hyper-V 제어). C#/.NET runtime + TypeScript Web Console + PCVCLI.
이 파일은 Claude Code 세션용 **압축 운영 지침**이다. 저장소 규칙의 단일 진실은 `AGENTS.md`(현재 기준·경계·검증)이고,
historical predecessor 문단은 그 아래쪽에 누적돼 있다(약 90 KB). `AGENTS.md`는 **통째로 읽지 말고** 상단 generated 블록과
필요한 절만 `Grep`으로 찾는다.

이 저장소가 operational current, 열린 campaign, Claude Code skill(`.claude/skills/`)을 소유한다. private 저장소
`../purecvisor-desktop-node`는 2026-10-09부터 read-only archive이며 거기서 작업하지 않는다.

## 1. 작업 방식

- 충분한 정보가 모이면 바로 실행한다. 이미 확정된 결정(ADR, 사용자 지시)을 재논의하지 않는다.
- 독립적인 읽기/검색/검증은 한 응답에서 **병렬 tool call**로 묶는다.
- 넓은 탐색은 `Grep`/`Glob`로 위치를 좁힌 뒤 필요한 줄 범위만 `Read`한다. `docs/ga-ready/evidence/**`, `artifacts/**`,
  `archive/**`는 작업이 명시적으로 요구할 때만 연다.
- 하위 에이전트/Workflow는 사용자가 요청할 때만 사용한다.
- 선택지가 있으면 나열하지 말고 권장안 하나를 제시한다. 결과는 실패·생략 포함 사실대로 보고한다.

## 2. 실행 회로 차단기 (필수)

단일 진실: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`, 기계 계약: `config/agent-execution-circuit-breaker.json`.

- write/외부 mutation 전에 시작 계약 공개: checkpoint `k/N`, 차선 예산(Lane 1 30분·tool batch 18회, Lane 2 45분·12회,
  Lane 3 30분·12회), 리뷰 1회 + 제한 재검토 2회, 허용 파일 범위, 완료 검증 명령, 범위 밖 발견은 `report-only`.
- 70% 시점(Lane 1 21분 또는 13번째 batch)에 수치로 진행 보고.
- 한도 도달·동일 원인 3회 실패·범위 밖 설계 필요·새 `Add-Type`/P/Invoke/native ACL/installer handoff 필요 시 즉시 중단 →
  `git status`/diff 확인, green checkpoint만 보존, 완료/미완료/blocker/mutation 여부 보고. 추가 patch 금지.
- 모호한 `재개`/`계속`/`후속 작업`/`후속 조치`/`다음 단계`: `docs/ga-ready/active-campaign.json`이 `status=open`이고
  `next_task`가 있으면 `pcv-campaign` skill로 campaign task를 연쇄 실행한다(`pcv-campaign-runner-v2`, task마다 그 차선 예산).
  그 외에는 bounded checkpoint **하나**만 의미한다. 같은 checkpoint의 예산 소급 연장과 campaign 밖 범위 확대는 사용자 명시 승인 필요.
- 최종 보고의 다음 승인 번호로 답하면(예: `1,2,3`) `pcv-campaign-open`으로 새 campaign을 연다. push, PR, green CI 뒤 merge와
  merge 뒤 `main` run 확인은 `pcv-ship`을 따른다. 어느 skill도 승인 문장에 없는 권한을 열지 않는다.
- `/goal` 조건은 `pcv-goal`로 만든다. goal은 승인을 만들지 않고, 조건에는 정당한 정지 절(한도·승인 밖 작업·red·권한 거부에서
  멈추고 보고)을 넣는다. goal이 걸린 동안 보고 끝에 `goal-evidence:` 줄을 쓰고 CI 대기는 foreground로 한다.

## 3. 저장소 경계

- Windows Desktop Node 전용. Linux `purecvisor-single`/`purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN 코드를 추가하지 않는다.
- 내부 사설망 전용 배포(ADR-0006). public trusted signing, 외부 stable publication은 **scope 밖**이며 주장하지 않는다.
- 이 저장소는 GitHub public이다. 문서·evidence·시연 기록에 사용자 홈 경로, LAN 사설 IP, 호스트명, token 값을 적지 않는다.
  저장소 상대 경로와 Hyper-V Default Switch NAT 주소까지만 허용한다.
- 현재 결정은 `docs/ADR_INDEX.md`, `docs/adr/`가 우선한다.
- Active product runtime에 PowerShell helper fallback을 추가하지 않는다. 예외는 guest 실행의 PowerShell Direct transport
  (`powershell.exe` + `Invoke-Command -VMName`, ADR-0009)뿐이다. `archive/spikes/**`는 보존용 baseline이다.
- Local API 기본값은 loopback-only: Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...`.

## 4. 코드 구조

| 경로 | 역할 |
|---|---|
| `src/DesktopNode.sln` | .NET 솔루션 (각 프로젝트마다 `*.Tests` 짝). `src/DesktopNode.HyperV.IntegrationTests`는 sln 밖 opt-in(ADR-0016) |
| `src/DesktopNode.Host` | Windows Service host / listener (`DesktopNode.Host.exe`). SCM/firewall/trust-store/Event Log/Credential Manager 제어(`DesktopNodeWindows*Controller`, advapi32 P/Invoke) |
| `src/DesktopNode.Api` | Local API route, job runtime, reconciliation |
| `src/DesktopNode.HyperV` | WMI(`root\virtualization\v2`) 기반 native Hyper-V adapter |
| `src/DesktopNode.HostOps` | host ops catalog와 planner(observe/dry-run만, Mutate는 항상 거절). 실제 제어는 Host에 있다 |
| `src/DesktopNode.Verification` | `pcvverify`: 검증 shard 실행기, 완료 판정, train 도구. 제품 런타임이 아니다 |
| `src/DesktopNode.Cli` | PCVCLI (`docs/CLI_COMMAND_USAGE.md`) |
| `web/src/modules/*.ts` + `web/src/modules.json` → `web/app.bundle.js` | Web Console(ADR-0018 Single Edge 셸 `web/index.html`). `app.bundle.js`는 **생성물** — `web/src`를 수정 후 `npm run build:served --prefix web`. 같은 명령이 옛 콘솔 `web/app.js`(`web/index.legacy.html`, source `web/src/served-app.ts`)와 `web/sw.js` CACHE_NAME도 갱신한다. MSI web payload 목록은 `web/payload-manifest.json` |
| `packaging/windows-desktop-node/` | 제품 wrapper, installer(MSI), 관리자 smoke 도구, Pester 테스트, 문서 생성기 |
| `.claude/skills/` | `pcv-campaign`, `pcv-campaign-open`, `pcv-ship`, `pcv-goal` |

TUI는 현재 operator surface가 아니다(`tui_present=false`).

## 5. 검증 (영향 범위만 선택 실행)

```powershell
dotnet test src/DesktopNode.sln -c Release               # C# 변경. CI와 같은 Release 구성으로 돈다
dotnet test src/DesktopNode.Delivery.Tests -c Release    # docs, Delivery 계약 변경
npm test --prefix web                                   # Web 변경
npm run verify:parity --prefix web                      # Web 표시/fixture 변경
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

- 단일 .NET 프로젝트만 바뀌었으면 해당 `*.Tests` 프로젝트만 먼저 돌리고, 마무리 때 솔루션 전체를 돌린다.
- `PolicyBoundary` 시험은 dirty tree에서 실패한다. 솔루션 전체는 commit 뒤 clean HEAD에서 돈다.
- `archive/spikes/**` Pester는 기본 loop에 넣지 않는다.

## 6. 관리자/호스트 mutation 금지 (명시적 opt-in 전)

실제 Hyper-V VM 생성, service install/start/stop/delete, Firewall 변경, Event Log source 등록, Task Scheduler,
`Restart-Computer`, `msiexec` install/repair/uninstall/`REMOVE_DATA=1`, signed release build, mutating update/rollback은
사용자가 해당 작업을 명시적으로 승인한 경우에만 실행한다. 예외는 ADR-0016 standing approval이 campaign `approval_locator`에
있을 때 `pcv-it-` 접두사 일회용 VM의 생성·설정·삭제뿐이다. 테스트는 `-WhatIf` 또는 injectable runner로 분리한다.
장기 token 값은 command line에 노출하지 않는다(`-ApiTokenProtectedFile` 사용).

## 7. 문서/커밋 규칙

- 신규/수정 문서 본문은 한국어. 코드 식별자·명령·경로·route·version/evidence id는 원문 유지.
- Evidence 문서는 과거 anchor를 깨지 않도록 새 파일로 추가하고, 기존 evidence를 덮어쓰지 않는다.
- 현재 기준 수치(버전, MSI SHA, blocker)는 `AGENTS.md` 상단 "Current operational evidence (generated)" 섹션과
  `docs/ga-ready/current-evidence.json`이 소유한다. 생성기 `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check`로 확인한다.
- `AGENTS.md`, `.github/workflows/development-gates.yml`, `config/development-verification-suites.json` 등은
  `config/pcv-development-policy-contract-spec-v1.json`의 SHA pin 대상이다. 바꾼 뒤 같은 commit에서
  `packaging/windows-desktop-node/tools/Update-PcvContractSpecPins.ps1 -Apply`를 돌려 spec pin과 Delivery verifier 상수를 갱신하고
  `dotnet test src/DesktopNode.Delivery.Tests -c Release`로 확인한다. 안 하면 Delivery 시험 50건이 `source-sha`로 실패한다.
- 커밋은 사용자가 요청하거나 campaign `commit_policy`가 허용할 때만 한다.

## 8. gstack skill routing (기존 설정)

Project documentation: [complete documentation index](docs/DOCUMENTATION_INDEX.md).

When the user's request matches an available skill, invoke it via the Skill tool. The
skill has multi-step workflows, checklists, and quality gates that produce better
results than an ad-hoc answer. When in doubt, invoke the skill. A false positive is
cheaper than a false negative.

Key routing rules:
- Product ideas, "is this worth building", brainstorming → invoke /office-hours
- Strategy, scope, "think bigger", "what should we build" → invoke /plan-ceo-review
- Architecture, "does this design make sense" → invoke /plan-eng-review
- Design system, brand, "how should this look" → invoke /design-consultation
- Design review of a plan → invoke /plan-design-review
- Developer experience of a plan → invoke /plan-devex-review
- "Review everything", full review pipeline → invoke /autoplan
- Bugs, errors, "why is this broken", "wtf", "this doesn't work" → invoke /investigate
- Test the site, find bugs, "does this work" → invoke /qa (or /qa-only for report only)
- Code review, check the diff, "look at my changes" → invoke /review
- Visual polish, design audit, "this looks off" → invoke /design-review
- Developer experience audit, try onboarding → invoke /devex-review
- Ship, deploy, create a PR, "send it" → invoke /ship
- Merge + deploy + verify → invoke /land-and-deploy
- Configure deployment → invoke /setup-deploy
- Post-deploy monitoring → invoke /canary
- Update docs after shipping → invoke /document-release
- Weekly retro, "how'd we do" → invoke /retro
- Second opinion, codex review → invoke /codex
- Safety mode, careful mode, lock it down → invoke /careful or /guard
- Restrict edits to a directory → invoke /freeze or /unfreeze
- Upgrade gstack → invoke /gstack-upgrade
- Save progress, "save my work" → invoke /context-save
- Resume, restore, "where was I" → invoke /context-restore
- Security audit, OWASP, "is this secure" → invoke /cso
- Make a PDF, document, publication → invoke /make-pdf
- Launch real browser for QA → invoke /open-gstack-browser
- Import cookies for authenticated testing → invoke /setup-browser-cookies
- Performance regression, page speed, benchmarks → invoke /benchmark
- Review what gstack has learned → invoke /learn
- Tune question sensitivity → invoke /plan-tune
- Code quality dashboard → invoke /health
