# PureCVisor Desktop Node 프로젝트 진행 상황 조사·감사 보고서

- 조사 기준 시각: 2026-08-05 (Asia/Seoul)
- 조사 대상: `D:\data\projects\codex-zone\purecvisor-desktop-node`
- 기준 브랜치/커밋: `codex/no-human-code-review-assurance-design` / `0b0e63104125bc8d76e09ea37b9dd31c09affb4f`
- 비교 기준: `origin/main` / `3cc7726dcd12c573d815afe6c0c7c2d910f0c7de`
- 선행 감사: `docs/project-status-audit-2026-07-13.md` (23일 전)
- 조사 방식: 저장소·코드·문서·evidence·원격 GitHub 직접 점검 + 전체 비파괴 검증 lane 실측 재실행
- 제외: 관리자 권한 MSI/SCM/Hyper-V/방화벽/Event Log/trust-store mutation, 실제 VM 조작, reboot, remote push/merge, public publication

## 1. 종합 판정

**종합 상태: 양호(GREEN) — 단, 운영 승격과 프론트엔드 신뢰성은 미완(YELLOW).**

2026-07-13 감사에서 YELLOW 판정의 근거였던 **P1 두 항목이 모두 실제로 닫혔다.** 그리고 2026-07-15
functional correctness 검증에서 CONFIRMED로 판정된 **기능 결함 전건이 현재 소스에서 수정되어 있다.**
이는 지난 23일 동안 실질적인 품질 회복이 있었다는 뜻이다.

1. 비상승 계정 `dotnet test src/DesktopNode.sln -c Release`가 **816건 전건 통과, 실패 0**이다.
   (07-13 FAIL 19건 → 07-15 582건 → 07-16 591건 → 현재 816건)
2. `Development Gates` workflow가 `dotnet-tests`, `web-tests`, `packaging-pester`,
   `installer-web-pester` 4개 job으로 실제 제품 회귀를 CI에서 강제한다.
3. `current-evidence.json` 단일 진실 + 생성기 계약으로 문서 drift가 구조적으로 닫혔다.
   `-Check`가 6개 target 전부 `current`를 반환한다.
4. FC-01/FC-02/FC-04/FC-16/FC-18 결함이 소스 레벨에서 수정 확인됐다.

그러나 다음은 여전히 열려 있고, 일부는 **악화**됐다.

1. `archive/spikes` documentation-sync guard suite가 **53건 중 18건 FAIL**이다. 배제 자체는 이미
   결정된 archive 상태였으나, **5개 ADR이 이 suite를 검증 명령으로 계속 게시**하는 모순이 있었다(§10 처리).
2. 2026-07-16 평가서의 프론트엔드 **P0(가짜 운영 상태 표시)가 그대로 남아 있다.**
3. 2026-07-15/07-16 두 평가 문서가 **3주간 untracked 상태**로 이 워크스테이션에만 존재한다.
4. 로컬 `main`이 `origin/main`과 **양방향 분기**(로컬 전용 30, 원격 전용 28)했다.
5. 설치본 `0.42.68`과 canonical anchor `0.42.65`의 **버전 skew가 다시 벌어졌다.** 최신 closed
   manual-admin pair는 여전히 `0.42.58 -> 0.42.59`(68일 경과)다.
6. 대형 모듈 분리 권고가 **역행**했다. 주요 파일 3개가 모두 커졌다.

즉 **개발 검증 기준선은 회복 완료(PASS)**, **운영 승격 체인은 정체(WARN)**, **프론트엔드 사용자
경로는 미완(NOT READY)**, **공개 stable release는 계속 out-of-scope**다.

## 2. 상태 대시보드

| 영역 | 판정 | 07-13 대비 | 조사 결과 |
| --- | --- | --- | --- |
| .NET tests (비상승, Release) | **PASS** | FAIL → PASS | 816건 통과, 실패 0, exit 0 |
| Web npm test | PASS | 유지 | typecheck/served/frontend batch 통과 |
| Web parity | PASS | 유지 | static parity + browser fixture 통과 |
| Packaging Pester | **PASS** | 377 → 466 | 466/466 |
| Installer + Web Pester | **PASS** | 98 → 97 | 97/97 |
| Current evidence `-Check` | **PASS** | 신규 | 6/6 target `current` |
| `git diff --check` | PASS | 유지 | 위반 0 |
| **archive/spikes Pester** | FAIL(의도된 archive) | 신규 발견 | 18/53 실패. 배제는 기결정, ADR 모순은 §10에서 수정 |
| CI 범위 | **PASS** | WARN → PASS | Development Gates 4 job + Public Boundary |
| 최신 CI run (origin/main) | PASS | 유지 | run `30821977508` / `30821978600` 성공 |
| FC 기능 결함 (07-15) | **PASS** | CONFIRMED 5건 → 수정 완료 | FC-01/02/04/16/18 전건 |
| 문서 일관성 | **PASS** | WARN → PASS | 생성기 기반 단일 진실 |
| 프론트엔드 상태 진실성 | **NOT READY** | 미해결 | 가짜 static status 잔존 |
| Evidence freshness (anchor) | WARN | 45일 → 20일 | `0.42.65`, 2026-07-16 |
| Manual-admin closure | **WARN** | 악화 | `0.42.58 -> 0.42.59`, 68일 경과 |
| 설치본/anchor 버전 정합 | **WARN** | 신규 | 설치본 `0.42.68` vs anchor `0.42.65` |
| 대형 모듈 구조 | **WARN** | 악화 | 3개 주요 파일 모두 증가 |
| 미추적 감사 문서 | **WARN** | 신규 | 2건 3주간 untracked |
| 로컬 `main` 정합 | **WARN** | 신규 | origin/main과 양방향 분기 |
| 저장소 위생 | WARN | 부분 개선 | branch 31(원격 없음 17), worktree 7 |
| Public stable release | NOT READY | 유지 | 의도적 out-of-scope |

## 3. 실측 검증 결과

이 절의 모든 수치는 2026-08-05 조사 중 직접 실행한 결과다.

### 3.1 통과 lane

| 명령 | 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.sln -c Release` | **816 PASS / 0 FAIL**, exit 0 |
| `npm test --prefix web` | PASS |
| `npm run verify:parity --prefix web` | PASS |
| `Invoke-Pester packaging/windows-desktop-node/tests` | **466 / 466 PASS** |
| `Invoke-Pester installer/tests + web/tests` | **97 / 97 PASS** |
| `Update-PcvCurrentEvidenceDocs.ps1 -Check` | PASS, `ok:true`, 6 target `current` |
| `git diff --check` | PASS |

.NET assembly별 내역:

| Assembly | 통과 |
| --- | ---: |
| `DesktopNode.Api.Tests` | 236 |
| `DesktopNode.Host.Tests` | 187 |
| `DesktopNode.Runtime.Tests` | 126 |
| `DesktopNode.HyperV.Tests` | 122 |
| `DesktopNode.Cli.Tests` | 113 |
| `DesktopNode.Contracts.Tests` | 21 |
| `DesktopNode.Service.Tests` | 11 |
| **합계** | **816** |

07-15 검증서가 지적한 **`DesktopNode.HyperV.Tests` 부재가 해소**되어 122건이 추가됐다. 이는 native
provider 경로에 대한 회귀 방어가 새로 생겼다는 뜻으로, 이번 주기의 가장 의미 있는 개선 중 하나다.

### 3.2 실패 lane — `archive/spikes` documentation-sync guard

```text
ARCHIVE total=53 passed=35 failed=18
```

대상: `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`

저장소 루트의 untracked `testResults.xml`(2026-08-04 05:49 생성)이 같은 실패 18건을 기록하고 있으며,
이번 조사에서 **동일하게 재현**됐다.

#### 근본 원인: 진행을 금지하는 frozen negative status assertion

실패 원인은 제품 결함도, 한국어 재작성으로 인한 문구 유실도 아니다. **guard가 2026-05-07/08 시점의
"아직 하지 않음" 상태를 영구 고정 assertion으로 박아 둔 것**이 원인이다.

guard 기대값과 현재 canonical 값을 대조하면 성격이 분명해진다.

| guard 기대값 (2026-05-07/08 고정) | 현재 canonical 값 | 전환 근거 |
| --- | --- | --- |
| `burn_bootstrapper: not-built` | `build-install-repair-remove-pass-internal-smoke` | `burn-bootstrapper-lifecycle-smoke-2026-05-10-0416` PASS |
| `credential_manager_transition: blocked-by-no-mutation-preflight` | `installed-local-system-default-transition-pass` | `windows-credential-manager-default-transition-installed-2026-05-10-0395` PASS |
| `event_log_provider_transition: blocked-by-no-mutation-preflight` | `installed-provider-register-write-pass` | `windows-event-log-default-transition-installed-2026-05-10-0396` PASS |
| `tls_certificate_lifecycle: blocked-by-no-mutation-preflight` | `partial-code-level-cert-generate-rotate-delete-pass` | `internal-https-tls-lifecycle-installed-2026-05-10-0397` PASS |
| `service_token_rotation_revoke: blocked-by-no-mutation-preflight` | `installed-admin-smoke-pass` | `service-token-rotation-revoke-installed-2026-05-09` PASS |
| `diagnostic_bundle_installed_listener_execution: not-run` | `installed-listener-pass` | `0.39.0-admin-smoke` installed listener rerun PASS |

즉 **guard는 프로젝트가 성공했기 때문에 실패한다.** 막혀 있던 항목을 실제로 완료하고 PASS evidence를
남길 때마다 이 guard가 깨지는 구조다. 나머지 실패는 과거 버전 pin(`0.35.7-admin-smoke`), 과거 토큰
(`PCV_BATCH_ADMIN_REQUIRED`), Wave 1B 이전 route-owner source layout 가정
(`/api/v1/diagnostics/bundles`를 특정 C# 파일에서 기대) 때문이다.

또한 guard는 6개 문서가 **동일한 status token을 각각 중복 보유**할 것을 요구한다. 이는 2026-07-13
감사가 지적하고 `current-evidence.json` 생성기가 제거한 바로 그 중복 구조다.

#### 이 배제 결정은 이미 내려져 있었다

`docs/ga-ready/VERIFICATION_OWNERSHIP.md`는 2026-05-04 slice에서 이미 이 경로를
**component/archive baseline, `excluded from default required command`**로 분류했다(§Component/Archive
Baseline 49–53행·70행). `docs/DEVELOPMENT_VERIFICATION_POLICY.md:365`도 *"기본 검증 명령에는 active
`spikes/**` Pester path를 넣지 않는다"*를 명시한다.

2026-08-03 `csharp-architecture-wave2a-legacy-installed-checkpoint` evidence는 이 suite를 다시 감사해
동일 원인(`superseded 2026-05 anchors/candidate/preflight/not-run wording or a pre-Wave-1B route-owner
source layout`)을 기록하고, **"no current document was reverted to stale claims to satisfy it"**로
현행 문서를 되돌리지 않겠다고 못박았다.

따라서 이 red는 **의도된 archive 상태**이며, 현행 문서를 guard에 맞춰 되돌리는 것은 이미 명시적으로
배제된 선택지다.

#### 실제로 남아 있던 결함: 문서 간 모순

문제는 red 자체가 아니라, **5개 ADR 문서가 이 배제된 suite를 여전히 자신의 `## 검증 기준` 실행 명령으로
게시하고 있었다는 점**이다.

- `docs/adr/0000-template.md` (신규 ADR 작성 템플릿)
- `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
- `docs/adr/0002-release-version-policy.md`
- `docs/adr/0003-internal-trusted-signing-policy.md`
- `docs/adr/0004-ga-ready-product-runtime-candidate.md`

verification ownership은 "기본 검증에 넣지 말라"고 하는데 ADR은 "이것을 실행해 검증하라"고 지시하는
모순이다. 특히 `0000-template.md`는 신규 ADR을 만들 때마다 이 모순을 복제한다. 이 항목은 §10에서
처리했다.

## 4. 2026-07-13 감사 권고 이행 상태

| 권고 | 상태 | 근거 |
| --- | --- | --- |
| P1-1 개발 gate 비상승 hermeticity 복구 | **CLOSED** | 816건 전건 통과, CLI/Host 환경 결합 실패 0 |
| P1-2 CI 전체 회귀 검사 확대 | **CLOSED** | `development-gates.yml` 4 job, origin/main 최신 run 성공 |
| P2-2 current 문서 drift 제거 | **CLOSED** | `current-evidence.json` + 생성기 `-Check` 6/6 `current` |
| P2-1 evidence freshness / 후보 정리 | **부분** | 45일 → 20일로 개선했으나 anchor 승격은 여전히 정체 |
| P2-3 artifact 장기 보존 | **OPEN** | `artifacts/`는 계속 ignore 대상, immutable store 미도입 |
| P2-4 대형 중앙 파일 분해 | **역행** | 아래 6.3 참조 |
| P2-5 branch/worktree 정리 | **부분** | worktree 15 → 7로 감소, branch 25 → 31로 증가 |
| P3-1 제품 maturity 명칭 정리 | 부분 | evidence 문서의 boundary 선언은 매우 명확해짐 |
| P3-2 public distribution 결정 | OPEN | ADR-0006 internal-only 유지 |

## 5. 기능 결함(FC) 수정 검증

2026-07-15 `docs/functional-correctness-verification-2026-07-15-results.md`가 실측으로 CONFIRMED 판정한
결함을 현재 소스에서 직접 대조했다. **전건 수정 확인.**

| ID | 07-15 판정 | 현재 상태 | 수정 근거 |
| --- | --- | --- | --- |
| FC-01 | 조건부(축소 가드 부재) | **FIXED** | `DesktopNodeHyperVWmiVmResourceMutationProvider.cs:159` `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` 사전 가드 |
| FC-02 | CONFIRMED (rollback 비원자적) | **FIXED** | `Restore-PcvDesktopNodePreviousProductRoot`에 보상 이동 + `PCV_PRODUCT_RESTORE_COMPENSATION_FAILED` 도입 |
| FC-04 | CONFIRMED (Kbps 1000배 단위 오류) | **FIXED** | `KbpsToBitsPerSecond` / `BitsPerSecondToKbps` 변환 적용 (동 파일 238–252, 377–388) |
| FC-16 | CONFIRMED (기존 backup 축출) | **FIXED** | `Backup-PcvDesktopNodeProductRoot`가 `.staging` 경유 + 실패 시 원복, `PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED`/`..._COMPENSATION_FAILED` 도입 |
| FC-18 | CONFIRMED (`DateTime.MaxValue`로 최신 run 가림) | **FIXED** | `BatchEvidenceSummaryReader.GetEvidenceSummarySortTime`이 `DateTime.MinValue` 반환 |
| FC-05 / FC-12(b) / FC-13 | 미검증 (격리 guest/ISO 부재) | **여전히 미검증** | 부팅 가능한 격리 guest·전용 credential·throwaway ISO 준비 필요 |

FC-04는 `AGENTS.md` current evidence 블록의 `QoS 2048 Kbps -> 2,048,000 bps` PASS 기록과도 일치한다.
FC-01은 같은 블록의 `disk shrink guard and 10 -> 11 GiB expansion PASS`와 일치한다.

**남은 조치:** FC-05, FC-12(b), FC-13은 환경 부재로 skip된 상태 그대로다. 격리 Windows guest와
throwaway 부팅 ISO를 준비해 재검증 대상으로 남겨야 한다.

## 6. 신규·잔존 위험

### P1 — 즉시 처리 권고

#### P1-1. ADR 검증 명령과 verification ownership의 모순 — **본 감사에서 처리 완료(§10)**

- 현상: `archive/spikes` suite가 53건 중 18건 FAIL. 2026-08-04 `testResults.xml`과 이번 조사에서 동일 재현.
- 원인: §3.2 참조. guard가 2026-05 시점의 negative status를 영구 고정하고 있어, 해당 항목을 실제로
  완료할수록 깨진다. suite의 배제는 이미 결정·기록된 상태다.
- 실제 결함: 5개 ADR 문서가 배제된 suite를 `## 검증 기준` 실행 명령으로 계속 게시해
  verification ownership과 모순됐다. `0000-template.md`는 신규 ADR마다 이 모순을 복제한다.
- 조치: §10에서 active gate에 회귀 테스트를 먼저 추가(RED)한 뒤 5개 ADR을 수정(GREEN)했다.
- 잔여 판단 사항: guard 파일 자체는 historical archive baseline으로 **보존**한다. 이후 이 suite를
  실제로 정리하려면 frozen status assertion을 canonical `current-evidence.json` 대조 방식으로
  다시 쓰거나 공식 폐기해야 하며, 이는 별도 승인 대상이다.

#### P1-2. 프론트엔드 P0 미해결 — 가짜 운영 상태 표시

2026-07-16 평가서가 P0로 지정한 항목이 그대로 남아 있다. `web/index.html` 실측:

| 위치 | 정적 표시 |
| --- | --- |
| `web/index.html:113` | `pcv-node-a` |
| `web/index.html:163` | 활성 워크로드 `4/5` |
| `web/index.html:363` | `Connected` |
| `web/index.html:366` | `VM: 3/3` |
| `web/index.html:369` | `API: 10ms avg` |

또한 account auth는 여전히 미구성 시 `account_rbac_jwt_not_configured`
(`src/DesktopNode.Api/DesktopNodeAccountAuth.cs:207`)로 떨어진다.

- 위험: 이는 디자인 품질이 아니라 **운영 상태의 진실성** 문제다. API가 401로 전부 실패하는 상태에서도
  화면은 정상 연결·정상 대수를 표시한다. 운영자가 장애를 정상으로 오인할 수 있다.
- 권고: 인증 이전에는 `Unknown` / `Auth required`만 표시하고 fallback 샘플값 출력을 제거한다.
  이 조합(401 + Connected 표시)을 **실패로 판정하는 fixture**를 함께 추가해 회귀를 막는다.

#### P1-3. 감사 문서 2건이 3주간 untracked

- `docs/functional-correctness-verification-2026-07-15-results.md` (227줄)
- `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` (361줄)

두 문서는 실제 호스트 실측과 실제 브라우저 결과를 담은 **대체 불가능한 1차 증거**다. 그러나 Git에
없으므로 이 워크스테이션 디스크 장애 시 소실되고, 다른 clone에서는 열람·검증이 불가능하다.

단, `docs/superpowers/plans/2026-08-04-...-assurance-program.md` §1은 **"User-owned untracked files are
never staged, deleted, rewritten, or treated as plan evidence"**를 명시한다. 따라서 이 두 파일의 커밋
여부는 **자동 처리 대상이 아니라 사용자 결정 사항**이다.

- 권고: 사용자가 (a) 저장소에 커밋, (b) 별도 내부 보존소로 이전, (c) 의도적 로컬 전용 유지 중
  하나를 명시적으로 선택한다. 현 상태(무결정 방치)만 피하면 된다.

#### P1-4. 로컬 `main`이 `origin/main`과 양방향 분기

```text
git rev-list --left-right --count main...origin/main  →  30  28
merge-base: 2e98ff4f (2026-07-16)
```

내용 대조 결과 로컬 전용 30커밋은 PR #175 등으로 squash merge되어 `origin/main`에 반영된 것이며,
`git diff main origin/main`은 63 파일 차이를 보인다(원격이 더 앞섬).

- 위험: 로컬 `main`이 실제 최신이 아니면서 "main"이라는 이름을 갖고 있어 **잘못된 기준선 선택** 위험이
  있다. 실제로 이번 조사에서도 `main`과 `origin/main`을 구분해야만 정확한 비교가 가능했다.
- 권고: 로컬 `main`을 `origin/main`으로 정렬한다. 로컬 전용 커밋이 모두 원격에 반영됐음을 확인한 뒤
  수행해야 하며, **파괴적 명령이므로 사용자 승인 후 실행**한다.

### P2 — 단기 처리 권고

#### P2-1. 설치본/anchor 버전 skew 재발

| 항목 | 버전 | 날짜 | 경과 |
| --- | --- | --- | ---: |
| 설치본 admin smoke (최신) | `0.42.68-admin-smoke` | 2026-08-03 | 2일 |
| canonical operational anchor | `0.42.65-admin-smoke` | 2026-07-16 | 20일 |
| 최신 closed manual-admin pair | `0.42.58 -> 0.42.59` | 2026-05-29 | **68일** |
| blocked follow-up | `0.42.62 -> 0.42.63` | — | `blocked-by-installed-baseline-version-mismatch` |

`0.42.68` evidence는 스스로 "not a full-admin host mutation gate, actual-VM validation, manual-admin
package-pair closure or operational-anchor promotion"이라고 정확히 선언한다. 즉 문서는 정직하다.

문제는 **패턴**이다. `0.42.62 -> 0.42.63` follow-up이 이미 "설치본 baseline 버전 불일치"로 막혀 있는데,
`0.42.68` 설치가 같은 불일치를 한 단계 더 벌렸다. manual-admin closure는 68일째 정지 상태다.

- 권고: 다음 중 하나를 선택한다. (a) `0.42.68` 기준 fresh manual-admin package-pair campaign을 열어
  closure를 회복한다. (b) manual-admin closure를 의도적으로 유예 중임을 lifecycle 상태로 선언한다.
  현재처럼 "블록된 채 skew만 벌어지는" 상태를 계속 두지 않는다.

#### P2-2. 대형 모듈 분해 권고 역행

| 파일 | 07-13 | 07-16 | **현재** | 증감 |
| --- | ---: | ---: | ---: | --- |
| `DesktopNodeHostServiceAction.cs` | 3,573 | ~3.9K | **4,069** | ▲ 496 |
| `DesktopNodeApiRequestProcessor.cs` | 3,077 | ~3.1K | **3,367** | ▲ 290 |
| `web/src/served-app.ts` | — | ~3.6K | **3,896** | ▲ |
| `DesktopNodeHyperVNativeAdapter.cs` | 1,891 | ~1.9K | **2,038** | ▲ 147 |
| `web/app.js` (생성물) | — | ~4.4K | **4,406** | — |

C# architecture Wave 1A–1D에서 `DesktopNodeApiDiagnosticsHandler`(565),
`DesktopNodeApiOpsSummaryBuilder`(507), `DesktopNodeApiAuthSessionHandler`(276) 등을 실제로 분리했다.
그럼에도 **핵심 파일은 순증**했다. 추출량보다 신규 기능(admission, durability, reconciliation) 추가량이
컸기 때문이다.

- 권고: 분리 작업에 **상한 지표**를 건다. 예: 핵심 파일 라인 수를 gate로 고정하고 초과 시 CI 실패.
  지표 없이 "분리 권고"만 반복하면 이번처럼 순증을 막지 못한다.

#### P2-3. 저장소 위생

- local branch **31**개 중 **17**개가 대응 origin 브랜치 없음
- worktree **7**개 (07-13 15개 → 개선됨)
- 저장소 루트에 untracked `testResults.xml` (1.6 MB, 2026-08-04) 방치
- draft PR **#172**가 2026-07-13부터 **23일째** 열려 있음
- `artifacts/`는 계속 `.gitignore` 대상 — clone만으로 evidence 재현 불가 (07-13 P2-3 미해결)

- 권고: `testResults.xml`을 `.gitignore`에 추가하거나 제거한다. PR #172의 존폐를 결정한다.
  origin 없는 17개 branch는 보존 필요성 확인 후 일괄 정리안을 만든다(삭제는 승인 후).

### P3 — 전략 결정 필요

#### P3-1. 현재 브랜치 작업이 CI·리뷰 밖에 있음

`codex/no-human-code-review-assurance-design`은 `origin/main` 대비 3커밋 ahead이며 **push되지 않았고,
PR도 없고, CI run도 없다.** 내용은 문서 8건 약 **8,680줄**(design spec 953줄 + 7개 plan 7,727줄)이다.

- 관찰: 이 프로그램의 목적은 "사용자가 소스를 읽지 않고 Decision Packet만으로 판단"하는 체계 구축이다.
  프로그램 문서 자체는 `Status: proposed implementation plan; separate user execution approval is pending`
  이며, 스스로 `required_enforced=false`, `automatic_landing=false`, `overall_readiness=red`을 truthful하게
  선언한다. 즉 **설계는 정직하고 경계도 명확하다.**
- 권고: 실행 승인 전이라도 문서 lane(markdown lint, 링크 검증, public-boundary)은 PR로 올려 CI를 태우는
  편이 안전하다. 8,680줄이 로컬에만 있는 상태가 길어질수록 P1-3과 같은 소실 위험이 생긴다.

#### P3-2. 미검증 FC 항목의 검증 환경

FC-05, FC-12(b), FC-13은 두 차례 연속 "격리 guest/ISO 미준비"로 skip됐다. 검증 환경을 갖추지 않으면
이 항목들은 영구 미검증으로 남는다.

- 권고: throwaway 부팅 ISO와 격리 Windows guest, 전용 test credential을 표준 검증 자산으로 확보한다.

## 7. 프로젝트 진행 궤적

| 시점 | 상태 |
| --- | --- |
| 2026-07-13 | 전수 조사 — YELLOW. .NET test FAIL 19건, CI 범위 협소, 문서 drift, evidence 45일 정체 |
| 2026-07-15 | 실호스트 functional correctness 검증 — 기능 결함 5건 CONFIRMED |
| 2026-07-16 | 코어/백엔드/프론트엔드 구현 평가 — 77/100, 프론트엔드 P0 3건 도출. `0.42.65` anchor 승격 |
| 2026-08-02~03 | C# architecture Wave 0~2C, 5A, 6 — job durability, reconciliation, admission, owner 분리. `0.42.68` 설치 smoke |
| 2026-08-03 | Luna 제어/전달 문서 체계 정비 (PR #176~#182 병합) |
| 2026-08-04 | No-human-code-review assurance 설계·계획 7문서 작성 (미push) |
| 2026-08-05 | 본 감사 — 개발 검증 기준선 회복 확인, 운영 승격 정체와 프론트엔드 P0 잔존 확인 |

지난 23일의 작업은 **품질 인프라(테스트·CI·문서 단일 진실·결함 수정)에 집중**됐고 그 목표는
달성됐다. 반면 **사용자 대면 완성도(프론트엔드)와 운영 승격 체인(manual-admin closure)은 진전이 없다.**

## 8. 권고 실행 순서

| 순위 | 항목 | 완료 조건 |
| ---: | --- | --- |
| 1 | ~~ADR 검증 명령과 verification ownership 모순 제거~~ **완료(§10)** | active gate 회귀 테스트 + 5개 ADR 수정, packaging Pester 467/467 |
| 2 | 프론트엔드 가짜 운영 상태 제거 | 401 상태에서 `Connected`/`VM: 3/3`/`API: 10ms avg` 표시 0건, 회귀 fixture 존재 |
| 3 | untracked 감사 문서 2건 보존 결정 | 사용자가 커밋/이전/로컬유지 중 택1 |
| 4 | 로컬 `main` 정렬 (승인 후) | `main == origin/main` |
| 5 | manual-admin closure 재개 또는 유예 선언 | `0.42.68` 기준 pair closure, 또는 lifecycle 상태 문서화 |
| 6 | assurance 문서 branch push + PR | 문서 lane CI PASS, 로컬 전용 8,680줄 해소 |
| 7 | 대형 모듈 라인 수 gate 도입 | 핵심 4파일 상한 지표가 CI에서 강제됨 |
| 8 | 저장소 위생 정리 (승인 후) | `testResults.xml` 처리, PR #172 결정, orphan branch 정리안 |
| 9 | FC-05/12(b)/13 검증 환경 확보 | 격리 guest + throwaway ISO로 재검증 완료 |

## 9. 최종 결론

**개발 검증 기준선은 회복됐다.** 2026-07-13 감사의 P1 두 항목(비상승 테스트 hermeticity, CI 범위)이
실제로 닫혔고, 2026-07-15에 실측 확인된 기능 결함 5건이 전부 수정됐다. 816건 .NET 테스트, 466건
packaging Pester, 97건 installer/web Pester, Web 전 lane이 이번 조사에서 실패 0으로 통과했다. 문서
drift는 `current-evidence.json` 단일 진실 + 생성기 `-Check`로 구조적으로 차단됐다. 이는 실질적이고
검증 가능한 진전이다.

**남은 문제는 세 가지로 압축된다.**

1. **문서 간 모순(처리 완료)** — `archive/spikes` suite 18건 실패는 의도된 archive 상태였으나 5개
   ADR이 이를 검증 명령으로 게시하고 있었다. §10에서 회귀 테스트와 함께 수정했다.
2. **사용자 경로 미완** — Web Console이 인증 실패 상태에서 정상 운영값을 표시한다. 기능 부재가 아니라
   상태 진실성 문제이며, 2026-07-16에 P0로 지정된 뒤 진전이 없다.
3. **운영 승격 정체** — manual-admin closure가 68일째 멈춘 채 설치본만 `0.42.68`로 앞서 나가 skew가
   벌어졌다. 문서는 이를 정직하게 표기하고 있으나 상태 자체는 해소되지 않았다.

공개 release 관점은 변함없다. 현재 증거는 전부 internal admin-smoke 범위이며 public trusted signing,
trusted timestamp, external stable publication은 의도적 미완료다. 이 경계 선언은 모든 evidence 문서에서
일관되게 유지되고 있으며, 이 점은 이 프로젝트의 분명한 강점이다.

## 부록 A. 실행한 검증 명령

```powershell
git status --short --branch
git rev-list --left-right --count main...origin/main
git worktree list
gh pr list --state open ; gh run list --branch main
dotnet test src/DesktopNode.sln -c Release --nologo --verbosity minimal
npm test --prefix web
npm run verify:parity --prefix web
Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -PassThru
Invoke-Pester -Path @('packaging/windows-desktop-node/installer/tests','web/tests') -PassThru
Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests' -PassThru
& './packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1' -Check
git diff --check
```

## 부록 B. 주요 근거 파일

- `AGENTS.md`
- `docs/project-status-audit-2026-07-13.md`
- `docs/functional-correctness-verification-2026-07-15-results.md` (untracked)
- `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` (untracked)
- `docs/ga-ready/current-evidence.json`
- `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md`
- `docs/superpowers/plans/2026-08-04-purecvisor-desktop-node-no-human-code-review-assurance-program.md`
- `.github/workflows/development-gates.yml`
- `.github/workflows/public-boundary.yml`
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
- `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmResourceMutationProvider.cs`
- `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`
- `src/DesktopNode.Api/DesktopNodeAccountAuth.cs`
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- `web/index.html`

## 10. 처리 기록 — ADR 검증 명령 모순 제거 (2026-08-05)

권고 실행 순서 1번을 이 감사 안에서 처리했다. 조사 snapshot을 소급 수정하지 않고 후속 조치만 기록한다.

### 근본 원인

§3.2에 기록한 대로, `archive/spikes` suite의 배제는 이미 결정·기록된 상태였다
(`VERIFICATION_OWNERSHIP.md` §Component/Archive Baseline, `DEVELOPMENT_VERIFICATION_POLICY.md:365`,
2026-08-03 checkpoint evidence). 남아 있던 결함은 **5개 ADR이 그 배제된 suite를 자신의 검증 명령으로
게시해 verification ownership과 모순**된 것이다.

### RED — active gate에 회귀 테스트 먼저 추가

`packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`에
`keeps component archive baseline suites out of active ADR verification commands`를 추가했다. 이 파일은
`Development Gates`의 `packaging-pester` job이 실제로 실행하는 required 경로다.

테스트 계약:

1. `VERIFICATION_OWNERSHIP.md`가 `archive/spikes/purecvisor-desktop-node/tests`를
   `excluded from default required command`로 계속 기록하는지 확인한다.
2. `docs/adr/*.md` 전체를 순회하며 `Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests'`
   지시가 없는지 확인한다.

추가 직후 실행 결과는 실패였고, 실패 사유는 의도한 그대로였다.

```text
0000-template.md must not prescribe the excluded component/archive baseline suite
as a verification command, but it did match.
```

### GREEN — 5개 ADR 검증 명령 교체

`DEVELOPMENT_VERIFICATION_POLICY.md`가 component/archive 분리 문단에서 직접 제시하는 대체 명령
(`packaging/windows-desktop-node/tests`)으로 교체했다.

| 파일 | 변경 |
| --- | --- |
| `docs/adr/0000-template.md` | archive suite → packaging suite |
| `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md` | archive suite → packaging suite |
| `docs/adr/0002-release-version-policy.md` | archive suite → packaging suite |
| `docs/adr/0003-internal-trusted-signing-policy.md` | archive suite → packaging suite (기존 installer suite 행은 유지) |
| `docs/adr/0004-ga-ready-product-runtime-candidate.md` | archive suite → packaging suite |

ADR의 결정 내용, 상태 마커, 근거, 영향 범위는 변경하지 않았다. `## 검증 기준` 실행 명령만 교체했다.

### 검증 결과

| 검증 | 결과 |
| --- | --- |
| 신규 테스트 (추가 직후) | **FAIL** — 의도한 사유로 실패 확인 |
| 신규 테스트 (수정 후) | **PASS** |
| Packaging Pester 전체 | **467 / 467 PASS** (466 → 467, 신규 1건) |
| Installer + Web Pester | **97 / 97 PASS** |
| Current evidence `-Check` | **PASS**, 6/6 target `current` |
| `git diff --check` | **PASS** |

### 이번 조치에서 제외한 것

- `PcvDesktopNode.DocumentationSync.Tests.ps1` 자체는 **수정하지 않았다.** historical archive baseline으로
  보존하며, 18건 실패는 의도된 archive 상태로 남는다.
- 현행 문서를 guard의 stale claim에 맞춰 되돌리지 않았다. 이는 2026-08-03 checkpoint가 명시적으로
  금지한 방향이다.
- `.github/workflows/**`, 제품 코드, current/GA evidence, 설치본, 서비스, Hyper-V는 변경하지 않았다.
- untracked 파일은 staging·수정·삭제하지 않았다.

## 부록 C. 조사 중 수행하지 않은 것

- 관리자 권한 MSI/SCM/Hyper-V/방화벽/Event Log/trust-store mutation
- 실제 VM 생성·시작·변경·삭제
- 설치본 서비스 재시작 또는 구성 변경
- remote push, merge, PR 생성
- 브랜치·worktree 삭제
- untracked 파일의 staging·수정·삭제
- 제품 코드(`src/**`, `web/**`, `packaging/**` 제품 wrapper) 변경
- workflow(`.github/workflows/**`) 변경
- `PcvDesktopNode.DocumentationSync.Tests.ps1` 수정 또는 삭제

§10 조치로 변경한 파일은 ADR 검증 명령 5건과 packaging 테스트 1건뿐이며, 본 보고서 파일이 추가됐다.

## 11. 권고 처리 closure addendum (2026-08-05)

이 부록은 위 조사 snapshot을 수정하거나 소급 재해석하지 않는다. §8 권고 실행 순서에 대한 같은 날
후속 처리 상태만 추가한다.

| 순위 | 항목 | 상태 | 근거 |
| ---: | --- | --- | --- |
| 1 | ADR 검증 명령과 verification ownership 모순 제거 | **CLOSED** | §10 |
| 2 | 프론트엔드 가짜 운영 상태 제거 | **CLOSED** | `docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md`와 그 closure addendum |
| 3 | untracked 감사 문서 2건 보존 결정 | **CLOSED** | 저장소에 커밋 |
| 4 | 로컬 `main` 정렬 | **CLOSED** | `origin/main`으로 정렬 후 동기 |
| 5 | manual-admin closure 재개 또는 유예 선언 | **OPEN** | 아래 참조 |
| 6 | assurance 문서 branch push + PR | **OPEN** | `codex/no-human-code-review-assurance-design` 3커밋 미푸시 |
| 7 | 대형 모듈 라인 수 gate 도입 | **OPEN** | — |
| 8 | 저장소 위생 정리 | **부분 CLOSED** | 아래 참조 |
| 9 | FC-05/12(b)/13 검증 환경 확보 | **OPEN** | 격리 guest와 throwaway ISO 여전히 부재 |

### 추가로 닫힌 항목

- **§6 P1-2 프론트엔드 P0**: footer/hero를 실제 state에 바인딩했고, 후속 slice가
  `getHostReadinessLabel()`의 `Ready` fallback을 helper 안에서 제거해 metric grid와 ops cockpit까지
  닫았다. 최종 리뷰가 게이트를 프로덕션에서 무력화하던 Critical 결함(실제 API의 401 `operation`이
  `api.auth`인데 게이트가 route id를 기대)을 찾아 함께 수정했다.
- **§6 P2-3 저장소 위생**: 로컬 branch `31` → `18`, worktree `7` → `5`로 줄였다. 삭제는 `origin/main`에
  완전 포함된 것만 수행했다. draft PR #172는 고유 evidence 4건을 복원한 뒤 닫았다.
- **§3.2 문서 drift 예외**: 저장소 root `README.md`가 생성기 target 밖이라 `0.42.63` anchor로
  drift해 있었다. 생성기 owned 문서에 편입해 `7/7 current`가 됐다. §2 대시보드의 "문서 일관성 PASS"는
  조사 시점 기준 6개 target에 대한 판정이었으며 root README는 그 범위 밖이었다.

### 새로 관측된 항목

- **.NET 간헐 실패**: 병합 결과 검증 중 `DesktopNode.Runtime.Tests`가 전체 솔루션 실행에서 `1`건
  실패했다(`125/126`). 같은 스위트 단독 실행은 `126/126`, 이후 전체 솔루션 재실행 `3`회와 원격 CI는
  전부 통과했다. 실패 테스트 이름은 확보하지 못했다. 최근 job store durability/lease 작업이 파일 I/O와
  동시성을 도입한 영역이므로 병렬 실행 경합 가능성이 있다. 재현되면 별도 조사가 필요하다.
- **미커밋 고유 작업**: `.worktrees/development-throughput-automation`에 2026-07-16자 `136`줄 테스트
  작업이 `origin/main`에 없는 채로 남아 있다. worktree 정리 중 삭제 직전에 발견해 보존했다. 커밋 또는
  폐기 결정이 필요하다.

### 여전히 열린 핵심 항목

manual-admin closure는 `0.42.58 -> 0.42.59` 이후 갱신되지 않았고 설치본은 `0.42.68`로 앞서 있다.
이번에 복원한 `manual-admin-campaign-2026-07-13-04259-04262.md`는 이 정체의 시작점인
`BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED`를 기록한다. 전용 baseline host와 protected credential
reference가 구성되지 않는 한 이 항목은 열린 상태로 남는다.

## 12. FC-05 / FC-12(b) / FC-13 미검증 항목 상세 (2026-08-05)

§5는 세 항목을 "환경 부재로 미검증"으로만 기록했다. 이 절은 각 항목이 실제로 무엇을 확인하려는
것인지, 환경 없이도 확인 가능한 코드 레벨 표면이 무엇인지, 그리고 어떤 자산이 있어야 닫히는지를
분리한다.

**핵심 정정:** 세 항목이 모두 환경에 막혀 있는 것은 아니다. FC-12(b)와 FC-13은 격리 guest나 ISO
없이도 **지금 테스트할 수 있는 코드 레벨 결함 표면**을 가진다. 환경이 필요한 것은 그 표면이 실제
guest/boot 동작으로 이어지는지의 최종 확인이다.

### 12.1 FC-13 — Gen2 create 경로의 boot order

| 항목 | 내용 |
| --- | --- |
| 확인 대상 | 제품 Gen2 create 경로가 첨부한 ISO로 실제 부팅되는가 |
| 코드 | `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmCreateProvider.cs` `AttachDiskAndDvd` |
| 코드 레벨 관찰 | disk를 `AddressOnParent=0`, DVD를 `AddressOnParent=1`로 SCSI controller에 붙인다. 그러나 provider 전체에서 `BootSourceOrder`, `Msvm_BootSourceSettingData`, firmware boot entry를 설정하는 코드가 **`0`건**이다. `VirtualSystemSubType`으로 generation만 지정한다. |
| 의미 | Generation 2는 firmware boot entry 목록이 명시적이다. 제품이 boot order를 설정하지 않으므로, 생성 직후 ISO로 부팅되는지는 전적으로 Hyper-V 기본 정렬에 의존한다. 빈 VHD가 DVD보다 앞서면 첨부한 ISO는 부팅되지 않는다. |
| 환경 없이 가능한 것 | boot order를 설정하지 않는다는 사실 자체는 이미 확정이다. 제품이 boot order를 명시적으로 소유해야 하는지 결정하고, 소유한다면 그 계약을 unit test로 고정할 수 있다. |
| 환경이 필요한 것 | 기본 정렬만으로 ISO 부팅이 실제로 되는지의 최종 판정 |

### 12.2 FC-12(b) — 비 ASCII stdin/credential

| 항목 | 내용 |
| --- | --- |
| 확인 대상 | 비 ASCII 사용자명·암호·명령·출력이 guest 실행 경로에서 손상되지 않는가 |
| 코드 | `src/DesktopNode.HyperV/DesktopNodeHyperVPowerShellDirectGuestExecutionProvider.cs` |
| 코드 레벨 관찰 | bridge script 자체는 `Encoding.Unicode` base64로 `-EncodedCommand`에 전달해 올바르다. 그러나 credential/command JSON payload는 stdin으로 넘기며(`process.StandardInput.Write(payload)`), `ProcessStartInfo`에 `StandardInputEncoding`, `StandardOutputEncoding`, `StandardErrorEncoding` 설정이 **`0`건**이다. |
| 의미 | .NET은 명시 설정이 없으면 콘솔 기본 인코딩을 쓴다. Windows에서 이는 보통 UTF-8이 아니라 OEM 코드 페이지다. 따라서 비 ASCII 사용자명·암호·명령이 stdin에서, 비 ASCII 출력이 stdout/stderr에서 손상될 수 있다. 암호가 손상되면 인증 실패로 나타나 원인 파악이 어렵다. |
| 환경 없이 가능한 것 | 세 인코딩을 명시적으로 UTF-8로 고정하고 그 계약을 unit test로 잠글 수 있다. process 기동 계약 검증이므로 guest가 필요 없다. |
| 환경이 필요한 것 | 실제 guest에서 비 ASCII credential로 인증과 출력 왕복이 성립하는지의 최종 판정 |

### 12.3 FC-05 — credentialed guest execution

| 항목 | 내용 |
| --- | --- |
| 확인 대상 | PowerShell Direct credentialed 실행이 실제 Windows guest에서 동작하는가 |
| 이력 | **한 번도 검증되지 않은 경로가 아니다.** `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass.md`에서 PASS했다. 2026-07-15 항목은 이후 변경에 대한 **재검증**이었다. |
| 환경 없이 가능한 것 | 없음. 이 항목의 본질이 실제 guest 왕복이다. |
| 환경이 필요한 것 | 부팅 가능한 격리 Windows guest와 전용 throwaway credential |

### 12.4 skip 판단은 옳았다

2026-07-15 검증은 빈 파일을 부팅 ISO로 가장하지 않았고, 운영 VM이나 기존 자격증명을 사용하지
않았다. 가짜 자산으로 만든 PASS는 evidence-first 저장소에서 유실보다 나쁘다. skip 기록은 정확한
판단이었다.

### 12.5 필요한 표준 검증 자산

임시 조달이 아니라 표준 자산으로 확보해야 반복 검증이 가능하다.

- 부팅 가능한 throwaway ISO 1개. Windows 평가판 ISO로 충분하며 지정 경로에 고정한다.
- 부팅 가능한 격리 Windows guest VHD 1개와 전용 throwaway credential. 운영 VM·계정과 분리한다.
- 두 자산 모두 `artifacts/` 밖의 고정 경로에 두고, 경로와 자산 해시를 검증 문서가 참조한다.

### 12.6 권고 실행 순서

1. FC-12(b)의 인코딩 명시와 계약 test를 먼저 닫는다. 환경이 필요 없고 결함 표면이 이미 확정이다.
2. FC-13의 boot order 소유 여부를 제품 결정으로 확정한다. 소유한다면 계약 test를 추가한다.
3. 표준 검증 자산을 확보한 뒤 FC-05, FC-12(b), FC-13의 실제 guest/boot 판정을 한 번에 실행한다.

이 절은 §5의 판정을 뒤집지 않는다. 세 항목은 여전히 미검증이며, 위 코드 레벨 관찰은 결함 확정이
아니라 검증 대상 표면의 식별이다.

### 12.7 부수 관찰 — CI trigger 계약 기록 오류

`docs/project-status-audit-2026-07-13.md`의 개발 게이트 복구 addendum은 workflow 계약을
"pull request, `main`, `codex/**` push와 manual dispatch"로 기록한다. 실제
`.github/workflows/development-gates.yml`과 `.github/workflows/public-boundary.yml`은 모두
`pull_request`, `main` push, `workflow_dispatch`만 선언하며 `codex/**` push trigger는 없다.

2026-08-05에 `codex/no-human-code-review-assurance-design`을 push했으나 CI run이 생성되지
않은 것으로 이를 실측 확인했다. 해당 브랜치의 `8,683`줄을 CI로 검증하려면 pull request를 열거나
`workflow_dispatch`를 사용해야 한다.

## 13. 간헐 실패 근본 원인 확정 (2026-08-05)

§11 "새로 관측된 항목"은 `DesktopNode.Runtime.Tests` 간헐 실패의 테스트 이름을 확보하지
못했다고 기록했다. PR #183의 `dotnet-tests` 실행이 이를 확보했다.

```
Failed DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests
       .RestartRecoveryPublishesOnlyAfterDurableRewriteAndEmitsRedactedObservations [5 s]
Assert.True() Failure
Failed: 1, Passed: 125, Total: 126
```

### 정정

같은 날 먼저 강화한 `BlockingEventSinkDoesNotHoldRuntimeStateLockOrDelayLoadOutcome`는
**이 실패의 원인이 아니었다.** 그 테스트의 wall-clock 성능 단언도 실재하는 결함이었고 수정은
유효하지만, 실제 flake는 다른 테스트였다. 당시 "같은 시그니처"라는 정황은 `125/126`이라는
숫자가 일치했을 뿐이며 동일 테스트임을 뜻하지 않았다.

### 근본 원인

실패 테스트는 비동기로 발행되는 observation을 `SpinWait.SpinUntil(..., 5초)`로 기다렸다.
로그의 소요 시간 `5 s`가 타임아웃 소진과 정확히 일치한다.

`SpinWait`는 양보 전에 회전하며 CPU를 소비한다. GitHub runner에서 `7`개 test assembly가
병렬로 도는 상황에서는 **기다리는 쪽이 기다림의 대상이 필요로 하는 CPU와 경쟁한다.** 즉 작업이
느린 것이 아니라 굶주린 상태에서 짧은 마감이 만료된다.

### 수정

- `RecordingEventSink`가 `Monitor.PulseAll`로 신호하고 `Wait(condition)`이 `Monitor.Wait`으로
  블로킹 대기한다. 유휴 중 CPU를 쓰지 않는다.
- 마감을 `60`초 hang guard로 올렸다. 성능 임계값이 아니라 교착 방지다.
- 같은 패턴을 `ApiHardeningRequestProcessorTests`(`3`초)와
  `DesktopNodeHostJobRuntimeEventSinkTests`(`5`초)에서도 제거했다. 두 건은 확인된 실패는
  아니지만 동일 결함 클래스이며 다음 미확인 flake의 후보였다.

전체 솔루션 `819/819`을 `3`회 연속 통과했다. 다만 간헐 실패의 부재는 짧은 반복으로 증명되지
않는다. 이 항목은 원인 확정과 수정으로 닫으며, 재발하면 이 절을 갱신한다.

## 14. 버전 일원화와 manual-admin blocker 해소 (2026-08-05)

§8 권고 5번(manual-admin closure 재개 또는 유예 선언)의 후속이다. 조사 결과 이 항목의 실제
blocker는 §6 P2-1이 기록한 "설치본/anchor 버전 skew" 그 자체였다.

### 세 평면이 갈라져 있었다

| 평면 | 조사 시점 |
| --- | --- |
| canonical anchor | `0.42.65` |
| 설치본 | `0.42.68` |
| manual-admin closure | `0.42.58 -> 0.42.59` |

`0.42.68`은 승격 대상이 될 수 없었다. 커밋 `f9337061` 기준이라 이후 제품 코드 커밋 `13`건이
빠지며, 그중에는 같은 날 실호스트로 검증한 FC-13 수정 두 건과 Web Console 진실성 작업이 포함된다.
따라서 현재 `main`에서 `0.42.69`를 새로 빌드해 승격했다.

### 실행 결과

| 단계 | 결과 |
| --- | --- |
| `0.42.69` package build | PASS. 해시 3중 일치, provenance == HEAD, payload에 당일 수정 포함을 실측 확인 |
| full admin host mutation gate | PASS. 2단계 exit 0, 재시도 0, 정리 완전 |
| installed current-card | PASS. CLI 3/3, Web 2/2, secret 미관측 |
| anchor 승격 | `0.42.65` -> `0.42.69`. anchor와 설치본 정렬 |

### 68일 blocker의 실체

manual-admin closure가 막힌 직접 원인은 전용 호스트 부재가 아니라 **버전 불일치**였다. 예약과
캠페인이 `installed_version == baseline_version`을 요구하는데 설치본이 항상 앞서 있었다.

anchor 승격으로 이 조건이 해소됐다.

```text
requested_version_status: blocked-by-installed-baseline-version-mismatch  ->  matches-installed-version
package_pair_input_status: ...mismatch  ->  ready-current-baseline-target-package-pair
reservation_status: (예약 미존재)  ->  reserved-and-matched
actual_execution_eligible: false  ->  true
```

2026-05-29 이후 처음으로 package-pair가 ready 상태다. 상세는
`docs/ga-ready/evidence/manual-admin-campaign-readiness-2026-08-05-04269-04270.md`가 소유한다.

### 세 번 반복된 관측 오류

이 항목을 포함해 같은 패턴이 하루에 세 번 나왔다. 기록이 자원 부재를 시사했지만 실제로는 확인하지
않은 것이었다.

| 항목 | 기록된 blocker | 실제 |
| --- | --- | --- |
| FC-13 | "부팅 가능한 ISO가 지정 후보 경로에 없음" | `D:\Downloads`에 Ubuntu ISO 존재. 결함 2건 발견 |
| clean-host runner | "전용 호스트 필요" | runner가 base VHD로 throwaway VM을 띄움. VHD 존재 |
| Burn/MSIX/ops runner | "실행 도구 없음" | 전용 래퍼 없이 표준 도구 직접 호출. WiX/SDK/인증서 모두 존재 |

blocker를 기록할 때는 "무엇이 없는지"와 "무엇을 확인하지 않았는지"를 구분해야 한다. 세 건 모두
후자였다.

### 여전히 열린 것

campaign closure는 달성하지 않았다. runner `5`종 중 `manual-admin-readiness`만 실행했고 나머지는
`not-run`이다. 중단은 명시적 결정이다. `installed-product-update-rollback`이 호스트를 target에
남기므로 완주하지 못하면 방금 맞춘 정렬이 다시 어긋난다.

알려진 미해결: `WixToolset.Bal.wixext 5.0.2`가 `damaged`이며 Burn 번들 빌드 전 재설치가 필요할 수
있다.

## 15. 2026-08-06 후속 작업 closure addendum

이 부록은 위 조사 snapshot을 수정하거나 소급 재해석하지 않는다. §8 권고와 §11 표에 대한
2026-08-06 후속 작업의 처리 상태만 추가한다.

| 순위 | 항목 | §11 상태 | 현재 | 근거 |
| ---: | --- | --- | --- | --- |
| 5 | manual-admin closure 재개 | OPEN | **CLOSED** | `0.42.69 -> 0.42.70` closure (`e9138988`) |
| 6 | assurance 문서 branch push + PR | OPEN | **CLOSED** | PR #183이 `2026-08-05T08:13:03Z`에 merge됐다(merge commit `65ba044d`, `main`에 포함). branch ref는 merge 후 삭제돼 없지만 커밋은 유실되지 않았다 |
| 7 | 대형 모듈 라인 수 gate 도입 | OPEN | **이미 CLOSED였음** | `packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`, 커밋 `695189d4` (`2026-08-05 15:38`). §11 표의 `OPEN`은 작성 시점에 이미 낡은 값이었다 |
| 8 | 저장소 위생 정리 | 부분 CLOSED | **진행** | 아래 참조 |
| 9 | FC-05/12(b)/13 검증 환경 확보 | OPEN | **자산은 이미 있었음** | `docs/ga-ready/evidence/fc-05-fc-12b-fc-13-verification-2026-08-06-04270.md` |

### §P2-2 라인 수 표는 옳았다 (2026-08-06 정정)

이 부록은 처음에 "§P2-2의 '현재' 열은 어느 트리에서도 재현되지 않았다"고 적었다. **그 주장이
틀렸다.** 재현되지 않은 원인은 감사가 아니라 당시 사용한 측정 명령이었다.

`Get-Content <path> | Measure-Object -Line`은 각 입력 문자열 **안의** 줄 수를 센다. 빈 줄은 빈
문자열이므로 `0`을 기여하고, 합계가 파일의 빈 줄 수만큼 적게 나온다. 입력 `@('a','','b','','c')`에
대해 `5`가 아니라 `3`을 돌려준다.

올바르게 세면 C# `3`종이 감사 수치와 **정확히 일치**한다.

| 파일 | §P2-2 "현재" | 잘못된 측정 | 올바른 측정 | 판정 |
| --- | ---: | ---: | ---: | --- |
| `DesktopNodeHostServiceAction.cs` | `4,069` | `3,793` | **`4,069`** | 감사가 옳음 |
| `DesktopNodeApiRequestProcessor.cs` | `3,367` | `3,024` | **`3,367`** | 감사가 옳음 |
| `DesktopNodeHyperVNativeAdapter.cs` | `2,038` | `1,891` | **`2,038`** | 감사가 옳음 |
| `web/src/served-app.ts` | `3,896` | `3,719` | `4,005` | 08-05 이후 실제 변경 |
| `web/app.js` | `4,406` | `4,520` | `4,520` | 08-05 이후 실제 변경 |

나머지 `2`종은 측정 결함이 아니라 `web-console-state-truthfulness` 작업으로 파일이 실제로
바뀌었기 때문이다. 따라서 §P2-2가 기록한 대형 모듈 순증은 실재했고, "재현 불가"로 판단해 감사
수치를 배척한 것은 잘못이었다.

측정 결함은 중복 gate였던 `PcvLargeModuleLineCeiling.Tests.ps1`(커밋 `9f80a888`)에만 있었다. 그
gate는 같은 커밋(`67d4cf9e`)에서 `PcvModuleSizeRatchet.Tests.ps1`과 중복임이 밝혀져 픽스처와
함께 삭제됐다. 남아 있는 gate(`packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`,
커밋 `695189d4`)의 `Measure-RepoFileLines`는 처음부터 `Get-Content -Raw`로 전체 텍스트를 읽어
CRLF를 정규화한 뒤 `TrimEnd`·`Split`으로 줄을 세므로 이 결함을 가진 적이 없다. 빈 줄을 세는지
검증하는 회귀 테스트는 존재하지 않으며, 이 gate에는 필요하지도 않다.

### §11 "새로 관측된 항목" 처리

- **미커밋 고유 작업**: `.worktrees/development-throughput-automation`의 `136`줄은 고유 작업이
  아니었다. 같은 내용이 `0433a0c7`로 이미 `main`에 있고, `main` 판은 읽기 전용 자동 변수 `$host`를
  변수명으로 쓰던 결함까지 `4fe1a76a`에서 고친 우월한 판이다. 폐기했다.
- **.NET 간헐 실패**: 이번 작업에서 전체 솔루션을 `2`회 실행해 모두 통과했다. 재현되지
  않았고 원인도 확정하지 못했으므로 항목은 닫지 않는다. (당시 `787`건으로 적었으나 잘린 출력
  창을 읽은 오독이고 실제는 `825`건이다. 정정은
  `docs/followup-work-record-2026-08-06.md` §8.9가 소유한다.)

### §12 세 항목의 실제 상태

§12.6이 제시한 실행 순서 `1`·`2`는 감사와 **같은 날** 이미 이행돼 있었다. FC-12(b) 인코딩 고정은
`dcb703ad`, FC-13 boot order 소유는 `45ba267e`다. §12 본문은 두 항목을 미조치로 서술하지만 그
서술은 조사 시점 기준이며 커밋 시각 기준으로는 이미 해소된 상태였다.

남아 있던 것은 FC-13의 계약 test였고 이번에 추가했다. FC-05는 §12.5가 부재로 본 자산이 실제로는
호스트에 있어 재검증까지 실행했다. FC-12(b)의 guest 측 왕복은 미확정으로 남는다.

### 저장소 위생 처리 (§P2-3)

- clean-host VM 폴더 `49`개 제거. `docs/followup-work-record-2026-08-06.md` §7이 기록한
  `5.0 GB`는 실측 시점에 `0` bytes였고 빈 디렉터리 껍데기만 남아 있었다.
- `pcv-guest-installed-04253-r1`은 삭제하지 않았다. ledger의
  `guest-execution-persistent-windows-target-policy=keep`이 지정한 보존 자산이다.
- 로컬 `main`이 `origin/main` 대비 `6`커밋 앞서 있던 상태는 이번 후속 작업과 함께 PR로 올린다.
  §P3-1이 지적한 "브랜치 작업이 CI·리뷰 밖" 상태를 닫기 위해서다.

---

## 16. 2026-08-06 P2-2 대형 모듈 분해 addendum

이 부록도 위 snapshot을 수정하지 않는다. §P2-2가 기록한 순증에 대해 같은 날 별도 세션에서
실행한 분해 결과만 추가한다. Evidence는
`docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`,
서술 기록은 `docs/followup-work-record-2026-08-06.md` §9가 소유한다.

| 파일 | §P2-2 "현재" | 2026-08-06 | 처리 |
| --- | ---: | ---: | --- |
| `DesktopNodeHostServiceAction.cs` | `4,069` | **`1,174`** | 분해 완료 (`71`% 감소) |
| `DesktopNodeApiRequestProcessor.cs` | `3,367` | `3,367` | 미착수, 별도 계획 필요 |
| `DesktopNodeHyperVNativeAdapter.cs` | `2,038` | `2,038` | 미착수 |
| `web/src/served-app.ts` | `3,896` | `4,005` | 미착수, 별도 계획 필요 |

§P2-2의 권고("분리 작업에 상한 지표를 건다")는 `PcvModuleSizeRatchet`이 이미 이행하고 있었고,
이번 분해로 host 파일의 `max_lines`를 `4069`에서 `1174`로 내렸다. 라쳇은 한 방향으로만 움직이므로
순증은 CI에서 실패한다.

`4`종 중 `1`종만 닫혔다. §P2-2 항목 자체는 열린 상태로 유지한다.
