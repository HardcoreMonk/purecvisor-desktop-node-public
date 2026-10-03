# PureCVisor Desktop Node 프로젝트 진행 상황 현행화

> 현재 판정은 `docs/project-status-audit-2026-09-20.md`가 소유한다. 이 문서는 2026-09-06 snapshot이다.

- 조사 기준 시각: 2026-09-06 (Asia/Seoul)
- 조사 대상: `D:\data\projects\codex-zone\purecvisor-desktop-node-public`
- 공개 소스 권위: `HardcoreMonk/purecvisor-desktop-node-public`
- 기준 브랜치/커밋: `main` / `2e63bd5e5e760a2a625f8d3e617f05eb2d64b3ed`
- 원격 정합: `main == origin/main` (`0` / `0`)
- 선행 감사: `docs/project-status-audit-2026-08-05.md` (32일 전)
- 조사 방식: 권위 문서·evidence·git·GitHub CI/PR 직접 점검과 대형 모듈 라인 수 실측
- 제외: Required CI 네 shard 재실행, 관리자 MSI/SCM/Hyper-V/방화벽 mutation, 실제 VM 조작, remote push/merge, `current-evidence.json` 승격, public publication

이 문서는 작성 시점 snapshot이다. operational current 숫자는
`docs/ga-ready/current-evidence.json`이 소유하며, 이 감사가 그 JSON을 바꾸지 않는다.

## 1. 종합 판정

**종합 상태: 운영 current는 GREEN, 소스 HEAD는 다음 승격 대기(YELLOW).**

2026-08-05 감사가 남긴 세 축은 이후 32일 동안 닫히거나 성격이 바뀌었다.

1. **개발 검증 기준선**은 Required CI 네 shard(`dotnet`, `web`, `delivery`, `installer-policy`)로
   전환됐고, `origin/main` 최신 Development Gates run `33312234285`와 Public Boundary run
   `33312234278`이 2026-08-30에 성공했다.
2. **운영 승격 정체**는 풀렸다. closed pair는 `0.42.58 -> 0.42.59`(당시 68일)에서
   `0.42.74-admin-smoke -> 0.42.75-admin-smoke` /
   `manual-admin-campaign-descriptor-20260827-04274-04275`로 회복됐다.
3. **프론트엔드 상태 진실성 P0**는 loopback session과 state 바인딩으로 닫혔다. 정적
   `Connected` / `VM: 3/3` / `API: 10ms avg`는 소스에 없다.

남은 핵심 간격은 **고의가 섞인 승격 지연**이다. 소스 HEAD는 P1 managed clone과
`0.42.77-admin-smoke` probe-vehicle package/fullgate/current-card/clone actual-VM PASS를
가지고 있으나, canonical current는 계속 `0.42.75-admin-smoke`다.
`0.42.75 -> 0.42.77` package-pair는 `not-opened`이고 Lane 3 승격은 별도 승인 대상이다.

공개 trusted signing과 외부 stable publication은 계속 out-of-scope다.

## 2. 세 권위

`docs/DEVELOPMENT_PROCEDURE.md`가 분리하는 세 값은 이번 조사에서 서로 다르다. 하나를 다른
값으로 추정하지 않는다.

| 권위 | 값 | 출처 |
| --- | --- | --- |
| `ledger_current` | `0.42.75-admin-smoke` | `docs/ga-ready/current-evidence.json` |
| `source_head` | `2e63bd5` (2026-08-30, PR #10 merge) | `git rev-parse HEAD` |
| `installed_current` | `absent` | `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json` 없음 |

이 워크스테이션은 공개 소스 clone이며 설치본이 없다. 04275/04277 설치 판정은 evidence 문서가
소유하고, 이 호스트의 실시간 설치 상태를 재확인하지 않았다.

04275 provenance commit `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`는 이 공개 clone 역사에
없다. 공개 저장소는 2026-08-25 Required CI cutover 이후 bootstrap됐으므로, 그 SHA는 parent
private 역사의 값이다.

## 3. 상태 대시보드

| 영역 | 판정 | 08-05 대비 | 조사 결과 |
| --- | --- | --- | --- |
| operational current | **PASS** | `0.42.65` → `0.42.75` | tuple은 current-evidence JSON과 generated 블록이 일치 |
| Feature qualification | **PASS** | 신규 계약 | `promotion_eligible=true`, blockers `none` (04275 P0 4종) |
| 닫힌 manual-admin pair | **PASS** | 68일 정체 → 회복 | `0.42.74 -> 0.42.75`, `missing_count=0`, `not_pass_count=0` |
| 최신 origin/main CI | **PASS** | 유지 | Development Gates `33312234285`, Public Boundary `33312234278` |
| Required CI 구조 | **PASS** | Pester 4 job → 네 shard | Pester/비관리자 PowerShell invocation `0` (Required CI만) |
| `main` / `origin/main` | **PASS** | 양방향 분기 → 정렬 | `0` / `0`, working tree 깨끗 |
| Web 상태 진실성 | **PASS** | NOT READY → 닫힘 | `Connected`는 `state.connectionState`에 바인딩 |
| SERVICE_PLAN P0 4종 | **PASS** | 기획만 존재 | 04275 actual-VM Full r4 PASS |
| FC-01/02/04/16/18 | **PASS** | 유지 | 08-05에서 소스 수정 확인, 이후 회귀 주장 없음 |
| FC-05 / FC-13 | **PASS** | 미검증 → 08-06 닫힘 | 04270 actual guest/boot-order evidence |
| FC-12(b) | **PASS** | 미검증 → argv 수정 후 닫힘 | guest UTF-8 왕복 evidence |
| 대형 모듈 라쳇 | **PASS** | 역행 → 강제 | 원본 4파일 상한에 밀착, 순증 CI 실패 |
| 04277 package/fullgate/current-card | **PASS (비승격)** | 신규 | current unchanged, `promotion_ledger_status=not-promoted` |
| P1 clone actual-VM | **PASS (비승격)** | 신규 | 04277-r2 PASS, feature ledger는 `not-assessed` |
| `0.42.75 -> 0.42.77` pair | **OPEN** | 신규 간격 | `not-opened`, Lane 3 별도 승인 |
| next-campaign payload count | **STALE** | 신규 | descriptor는 `changed_source_file_count=0`을 유지 |
| 이 호스트 설치본 | **ABSENT** | 08-05는 `0.42.68` | 공개 clone. 실시간 설치 재확인 불가 |
| SERVICE_PLAN 문서 시제 | **STALE** | 신규 | 본문은 04273 planning-baseline snapshot |
| DOCUMENTATION_INDEX 카탈로그 | **STALE** | 부분 | 헤더 `2026-08-26` / `6e2bdb93`, HEAD는 `2e63bd5` |
| Open PR | **OPEN** | #172 draft → #5 | `#5` adapter 은퇴, 2026-08-27부터 열림 |
| Public stable release | NOT READY | 유지 | 의도적 out-of-scope |
| 이 checkpoint의 전체 테스트 재실행 | **NOT RUN** | 08-05는 실측 | 회로 차단기 Lane 1 범위. 최신 CI PASS를 대리 증거로 기록 |

## 4. 현재 operational tuple

`docs/ga-ready/current-evidence.json` 실측:

| 필드 | 값 |
| --- | --- |
| version | `0.42.75-admin-smoke` |
| operator surfaces | Web, PCVCLI (`tui_present=false`) |
| package evidence | `docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md` |
| fullgate | `full-admin-host-mutation-gate-20260821-04275` |
| functional | `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md` |
| installed current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md` |
| clean MSI SHA-256 | `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6` |
| operational MSI SHA-256 | `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b` |
| payload SHA-256 | `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7` |
| provenance | `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4` |
| closed pair | `0.42.74-admin-smoke -> 0.42.75-admin-smoke` |
| public trusted signing | `false` |
| external stable publication | `false` |

04275 P0 actual-VM (`service-plan-p0-actual-vm-2026-08-27-04275`) Full r4:

| slice | verdict |
| --- | --- |
| saved_lifecycle | PASS |
| media_attach | PASS |
| checkpoint_restore | PASS |
| managed_import | PASS |
| cleanup | PASS, `pcv-p0-*` 잔여 `0` |

기능 승격 ledger 4종(`pcv.checkpoint.restore`, `pcv.vm.managed-import`,
`pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)은 모두 `0.42.75-admin-smoke` /
`verdict=pass`다. `pcv.vm.clone`은 surface ledger에만 있고 evidence 단계는
`not-assessed`다.

## 5. 2026-08-05 권고 이행

| 권고 | 상태 | 근거 |
| --- | --- | --- |
| P1-1 ADR 검증 명령 모순 | **CLOSED** | 08-05 §10. archive suite는 계속 archive |
| P1-2 프론트엔드 가짜 운영 상태 | **CLOSED** | 08-05 Web 진실성 slice + 04273 loopback. 현재 `Connected`는 state 바인딩 |
| P1-3 untracked 감사 문서 2건 | **CLOSED** | 두 문서가 저장소에 추적됨 |
| P1-4 로컬 `main` 양방향 분기 | **CLOSED** | `main...origin/main` = `0 0` |
| P2-1 설치본/anchor skew · manual-admin 정체 | **CLOSED then reopened as 04277** | 04275로 정렬 후, clone probe가 다시 HEAD를 앞세움 |
| P2-2 대형 모듈 순증 | **CLOSED** | 라쳇 강제. 원본 파일은 수천 줄에서 상한 밀착으로 축소 |
| P2-3 artifact 장기 보존 | **OPEN** | `artifacts/`는 계속 ignore. clone만으로 재현 불가 |
| P2-5 branch/worktree 위생 | **부분** | local 31→11, gone 0, worktree 7→3 |
| P3-1 assurance 문서만 로컬 | **CLOSED/대체** | 공개 저장소 + Required CI. 해당 브랜치는 이 clone의 주 작업이 아님 |
| P3-2 FC-05/12(b)/13 환경 | **CLOSED** | 08-06 재검증. FC-12(b)는 argv fidelity로 08-08 종결 |
| 라인 수 gate | **CLOSED** | `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json` |
| PR #172 | **해당 없음** | 공개 저장소 열린 PR은 `#5` |

## 6. 08-05 이후 궤적

공개 clone의 첫 기록 가능한 주기는 2026-08-25 cutover다. 그 이전(08-06~08-24)은 parent
저장소 evidence로 읽는다.

| 시점 | 상태 |
| --- | --- |
| 2026-08-05 | 감사. 개발 gate 회복, 프론트 P0와 manual-admin 68일 정체 |
| 2026-08-06 | `0.42.70` 일원화, FC 재검증, 코어 대형 모듈 분해 |
| 2026-08-08~09 | served-app 분해, `0.42.71` pair, FC-12(b) 설치본 왕복 |
| 2026-08-13~14 | loopback session, SERVICE_PLAN P0 설계. `0.42.73` |
| 2026-08-20~21 | P0 payload `0.42.74`/`0.42.75`. 04274 `vm.save` FAIL는 04275에서 닫힘 |
| 2026-08-24~25 | Pester-free Wave A/B, Required CI 네 shard cutover, 공개 소스 권위 bootstrap |
| 2026-08-27 | `0.42.75` current 승격, P0 4종 `promotion_eligible=true`. P1 clone 코드가 main에 합류 |
| 2026-08-28~29 | `0.42.76`/`0.42.77` probe-vehicle. clone actual-VM r2 PASS. 개발 절차 통합 |
| 2026-08-30 | `0.42.77` fullgate + current-card 기록, current 유지. 이후 커밋 `0` |
| 2026-09-06 | 본 현행화. 설치본 없는 공개 clone에서 권위 재확인 |

지난 32일의 작업은 **운영 승격 체인 회복 + P0 완결 + 검증 엔진 교체 + P1 clone 구현**이다.
사용자 경로 미완과 68일 pair 정체는 더 이상 현재 판정이 아니다.

## 7. HEAD가 current보다 앞선 이유

이것은 08-05의 “설치만 앞서고 pair가 죽은” 패턴과 다르다. 문서는 정직하다.

| 평면 | `0.42.77` | current에 반영? |
| --- | --- | --- |
| clone 코드 (API/CLI/Web/Hyper-V) | origin/main | 소스만. 04275 MSI에는 없음 |
| probe package | `admin-smoke-package-2026-08-29-04277` PASS | `canonical_current_changed=false` |
| clone actual-VM | 04277-r2 PASS | feature ledger 미변경 |
| fullgate | `full-admin-host-mutation-gate-20260830-04277` PASS | current unchanged |
| installed current-card | 2026-08-30 PASS, `not-promoted` | 04275 카드가 canonical |
| package-pair | `0.42.75 -> 0.42.77` `not-opened` | next descriptor도 `not-opened` |

04277 current-card가 명시한 승격 조건은 Lane 3 ledger update와 package-pair의 **별도 승인**이다.
worktree `codex/04277-manual-admin-promotion`은 같은 `2e63bd5`에 있으나 추가 커밋은 없다.

### 7.1 next-campaign descriptor 시제

`docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`는 04275 승격 시점의 next 값을
유지한다.

```text
next_manual_admin_package_pair_trigger: product-payload-change-after-04275
next_manual_admin_package_pair_candidate_status: not-opened-awaiting-next-product-payload
next_manual_admin_package_pair_payload_changed_source_file_count: 0
```

소스 HEAD에는 clone product payload가 이미 있다. 이 `0`은 **04275 승격 직후 snapshot**이며
현재 HEAD의 payload diff가 아니다. pair를 열 때 이 필드를 다시 재는 것이 맞다. 이 감사는
descriptor를 고치지 않는다.

### 7.2 04277 해시 주의

clean package provenance는 `04b3c9f`이고 operational fullgate provenance는 `9f051b5`다.
Host/CLI/MSI/payload aggregate가 서로 다르다. 승격 시 어떤 MSI를 current tuple에 넣을지
섞지 말아야 한다.

## 8. 대형 모듈 실측

08-05 원본 파일과 2026-09-06 raw line count(`Get-Content.Count`, 라쳇과 동일 단위):

| 파일 | 08-05 | 현재 | 라쳇 상한 |
| --- | ---: | ---: | ---: |
| `DesktopNodeHostServiceAction.cs` | 4,069 | **563** | 563 |
| `DesktopNodeApiRequestProcessor.cs` | 3,367 | **449** | 449 |
| `DesktopNodeHyperVNativeAdapter.cs` | 2,038 | **643** | 643 |
| `web/src/served-app.ts` | 3,896 | **429** | 429 |

원본 4파일은 모두 상한에 붙어 있다. 신규 기능은 새 모듈로 등록됐다.

| 신규/분해 모듈 | 줄 | 상한 |
| --- | ---: | ---: |
| `DesktopNodeHyperVWmiVmCloneProvider.cs` | 1,165 | 1,165 |
| `DesktopNodeApiVmMutationRouteHandler.cs` | 989 | 989 |
| `DesktopNodeHyperVNativeAdapter.Mutations.cs` | 925 | 925 |
| `DesktopNodeApiJobReconciliationHandler.cs` | 1,139 | 1,139 |
| `web/src/served/mutate.ts` | 516 | (500 초과 신규 등록 대상은 당시 기록상 없음) |

테스트 파일 `ApiRuntimePolicyRequestProcessorTests.cs` 4,480줄은 제품 모듈 라쳇 밖이다.

## 9. 저장소 위생

| 항목 | 값 |
| --- | --- |
| working tree | 깨끗 |
| local branch | 11 |
| remote branch | 11 |
| `[gone]` | 0 |
| worktree | 3 (main, `04275-stage1-immutable-preflight` @ `216a6ca`, `04277-manual-admin-promotion` @ `2e63bd5`) |
| 열린 PR | `#5` `codex/retire-p0-product-vm-state-adapter` (2026-08-27~) |
| 2026-08-30 이후 커밋 | 0 (7일 정지) |

`04275-stage1-immutable-preflight`는 main보다 뒤처진 커밋이다. 보존 가치가 없으면 정리 후보지만
삭제는 승인 후다.

## 10. 남은 제품 공백

SERVICE_PLAN `docs/SERVICE_PLAN.md`의 기획 표는 04273 시점 snapshot이다. 기능 존재는 아래
현재 소스로 읽는다.

| 기획 | 현재 | 다음 |
| --- | --- | --- |
| P0-1 media attach | 04275 PASS | 유지 |
| P0-2 restore reconcile | 04275 PASS | 유지 |
| P0-3 Saved | 04275 PASS | 유지 |
| P0-4 managed import | 04275 PASS | 유지 |
| P1-5 managed full clone | 코드+04277 actual-VM PASS | pair/ledger/Lane 3 |
| P1-6 inventory 시각/메모 | 미착수 | 기획 유지 |
| P1-7 template lock | 미착수 | clone 선행 |
| P1-8 제한적 guest 파일 job | 미착수 | 정책 유지 |
| P1-9 admin account CRUD | 미착수 | `no-default-account` 유지 |
| P1-10 나머지 reconcile | 부분 | family별 slice |
| P2 noVNC/export/네트워크 에디터 | 의도적 닫힘 | ADR-0006/0010 |

## 11. 권고 실행 순서

이 감사는 구현·승격·host mutation을 실행하지 않는다.

| 순위 | 항목 | 완료 조건 |
| ---: | --- | --- |
| 1 | `0.42.75 -> 0.42.77` pair를 열지, 04277을 probe-only로 둘지 명시 | Lane 3 승인 또는 “비승격 유지” 선언. 현재처럼 descriptor는 `awaiting-payload`인데 HEAD는 clone인 상태는 닫을 것 |
| 2 | 승격한다면 clean `04b3c9f`와 operational `9f051b5` MSI를 섞지 말 것 | current tuple이 하나의 provenance/MSI SHA만 가리킴 |
| 3 | clone을 feature evidence ledger 후보로 넣을지 결정 | `not-assessed`를 유지하거나 required stage를 채움 |
| 4 | PR `#5` 존폐 | merge, 닫기, 또는 명시적 보류 |
| 5 | stale worktree `04275-stage1-immutable-preflight` 정리안 | 승인 후 삭제 또는 목적 기록 |
| 6 | `artifacts/` 장기 보존 | 08-05 P2-3과 동일. 이번 checkpoint 밖 |
| 7 | SERVICE_PLAN/DOCUMENTATION_INDEX 시제 | 기획 문서는 snapshot임을 유지하되, 카탈로그 HEAD 표기는 별도 생성 작업 |

## 12. 최종 결론

**운영 current `0.42.75-admin-smoke`는 닫힌 상태다.** P0 4종, functional carry-forward,
manual-admin pair, installed current-card, feature qualification `promotion_eligible=true`가
같은 버전에 모여 있다. 08-05의 프론트 P0와 68일 pair 정체는 현재 판정이 아니다.

**소스 HEAD `2e63bd5`는 clone을 포함한 다음 후보를 이미 가지고 있다.** 04277 package,
fullgate, current-card, clone actual-VM r2는 PASS이지만 current가 아니다. 다음 일은 새
기능을 더 쌓는 것이 아니라, pair/승격을 승인할 것인지 probe-only로 남길 것인지를 한 줄로
고정하는 것이다.

공개 release 경계는 변함없다. 이 evidence는 internal admin-smoke 범위이며 public trusted
signing과 외부 stable publication을 주장하지 않는다.

## 부록 A. 이 checkpoint에서 실행한 명령

```powershell
git status --short --branch
git rev-parse HEAD
git rev-parse main origin/main
git rev-list --left-right --count main...origin/main
git worktree list
git branch -vv
git log -40 --format='%ad %h %s' --date=short
gh run list --branch main --limit 8
gh pr list --state open
Get-Content docs/ga-ready/current-evidence.json
Test-Path 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json'
# 대형 모듈 raw line count: Get-Content.Count
```

Required CI 네 shard, `dotnet test`, npm required, host mutation은 실행하지 않았다.
`origin/main` 최신 CI PASS를 개발 검증의 대리 증거로 쓴다.

## 부록 B. 주요 근거

- `docs/ga-ready/current-evidence.json`
- `docs/project-status-audit-2026-08-05.md`
- `docs/SERVICE_PLAN.md`
- `docs/DEVELOPMENT_PROCEDURE.md`
- `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`
- `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-27-04275.md`
- `docs/ga-ready/evidence/admin-smoke-package-2026-08-29-04277.md`
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-30-04277-hostmutation.md`
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-30-04277.md`
- `docs/ga-ready/evidence/service-plan-p1-clone-actual-vm-2026-08-29-04277-r2.md`
- `config/desktop-node-feature-evidence-ledger.json`
- `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`
- `.github/workflows/development-gates.yml`
