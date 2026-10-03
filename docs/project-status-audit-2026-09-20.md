# PureCVisor Desktop Node 프로젝트 진행 상황 현행화

- 조사 기준 시각: 2026-09-20 (Asia/Seoul)
- 조사 대상: `D:\data\projects\codex-zone\purecvisor-desktop-node-public`
- 공개 소스 권위: `HardcoreMonk/purecvisor-desktop-node-public`
- 기준 브랜치/커밋: `main` / `2e63bd5e5e760a2a625f8d3e617f05eb2d64b3ed`
- 원격 정합: fetch 후 `main == origin/main` (`0` / `0`)
- 선행 감사: `docs/project-status-audit-2026-09-06.md` (14일 전), `docs/project-status-audit-2026-08-05.md` (46일 전)
- 조사 방식: 권위 문서·evidence·git fetch·GitHub CI/PR·이 호스트 product-manifest/서비스/DisplayVersion 직접 점검
- 제외: Required CI 네 shard 재실행, 설치본 current-card HTTP/CLI 재실행, 관리자 MSI/SCM/Hyper-V/방화벽 mutation, 실제 VM 조작, remote push/merge, `current-evidence.json` 승격, public publication, 전체 문서 카탈로그 재생성

이 문서는 작성 시점 snapshot이다. operational current 숫자는
`docs/ga-ready/current-evidence.json`이 소유하며, 이 감사가 그 JSON을 바꾸지 않는다.

## 1. 종합 판정

**종합 상태: 운영 current와 이 호스트 설치본은 GREEN, 소스 HEAD는 다음 승격 대기(YELLOW), 소스 정지는 21일.**

2026-09-06 감사가 남긴 핵심 간격은 14일 동안 **닫히지 않았다.** 닫힌 것은 이 호스트의 설치
부재 기록뿐이다.

1. **운영 current**는 계속 `0.42.75-admin-smoke`다. feature qualification
   `promotion_eligible=true`, blockers `none`, 닫힌 pair
   `0.42.74-admin-smoke -> 0.42.75-admin-smoke`가 유지된다.
2. **이 호스트 설치본**은 오늘 `0.42.75-admin-smoke` / DisplayVersion `0.42.75` /
   service `Running` / `Automatic`이다. 09-06 snapshot의 `installed_current=absent`는
   당시 기록이며 현재 판정이 아니다. 설치 payload의 `source_root`는 공개 clone이 아니라
   parent 경로 `D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\admin-smoke-package-20260821-04275\payload`다.
3. **소스 HEAD**는 2026-08-30 `2e63bd5`에서 한 커밋도 없다. P1 clone과
   `0.42.77-admin-smoke` probe package/fullgate/current-card/clone actual-VM r2 PASS는
   소스에 남아 있으나 current가 아니다. `0.42.75 -> 0.42.77` pair는 계속 `not-opened`다.

공개 trusted signing과 외부 stable publication은 계속 out-of-scope다.

## 2. 세 권위

`docs/DEVELOPMENT_PROCEDURE.md`가 분리하는 세 값은 이번 조사에서 ledger와 설치본이 같고,
소스 HEAD만 앞선다. 하나를 다른 값으로 추정하지 않는다.

| 권위 | 값 | 출처 | 09-06 대비 |
| --- | --- | --- | --- |
| `ledger_current` | `0.42.75-admin-smoke` | `docs/ga-ready/current-evidence.json` | 동일 |
| `source_head` | `2e63bd5` (2026-08-30, PR #10 merge) | `git rev-parse HEAD` after `git fetch origin --prune` | 동일. 이후 커밋 `0` |
| `installed_current` | `0.42.75-admin-smoke` | `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json` | **absent → 04275** |

04275 provenance commit `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`는 이 공개 clone 역사에
없다. 공개 저장소는 2026-08-25 Required CI cutover 이후 bootstrap됐으므로, 그 SHA는 parent
private 역사의 값이다.

이 checkpoint는 설치본 listener HTTP/CLI current-card를 재실행하지 않았다. 오늘 읽은 값은
manifest version, uninstall DisplayVersion, SCM Name/Status/StartType뿐이다.

## 3. 상태 대시보드

| 영역 | 판정 | 09-06 대비 | 조사 결과 |
| --- | --- | --- | --- |
| operational current | **PASS** | 유지 | JSON과 generated 블록이 `0.42.75-admin-smoke`로 일치 |
| Feature qualification | **PASS** | 유지 | `promotion_eligible=true`, blockers `none` (04275 P0 4종) |
| 닫힌 manual-admin pair | **PASS** | 유지 | `0.42.74 -> 0.42.75`, descriptor `20260827-04274-04275` |
| 이 호스트 설치본 | **PASS (읽기)** | absent → 정렬 | manifest `0.42.75-admin-smoke`, DisplayVersion `0.42.75`, service `Running/Automatic` |
| 최신 origin/main CI | **PASS (대리)** | 유지. 이후 run 없음 | Development Gates `33312234285`, Public Boundary `33312234278` (2026-08-30) |
| Required CI 구조 | **PASS** | 유지 | 네 shard. Required CI Pester/비관리자 PowerShell invocation `0` |
| `main` / `origin/main` | **PASS** | 유지 | fetch 후 `0` / `0` |
| Web 상태 진실성 | **PASS (소스)** | 유지 | `connectionState` 바인딩. 정적 `Connected` 없음 |
| SERVICE_PLAN P0 4종 | **PASS** | 유지 | evidence ledger `verdict=pass` / `0.42.75-admin-smoke` |
| 04277 package/fullgate/current-card | **PASS (비승격)** | 유지 | `canonical_current_changed=false`, `promotion_ledger_status=not-promoted` |
| P1 clone actual-VM | **PASS (비승격)** | 유지 | 04277-r2. feature evidence ledger에 clone 행 없음 (`not-assessed`) |
| `0.42.75 -> 0.42.77` pair | **OPEN** | 유지. 21일 | `not-opened`, Lane 3 별도 승인 |
| next-campaign payload count | **STALE** | 유지 | descriptor는 `changed_source_file_count=0`을 유지 |
| 소스 활동 | **STALE** | 7일 정지 → 21일 정지 | 2026-08-30 이후 origin/main 커밋 `0` |
| SERVICE_PLAN 문서 시제 | **STALE** | 유지 | 본문은 04273 planning-baseline snapshot |
| DOCUMENTATION_INDEX 전체 카탈로그 | **STALE** | 유지 | 헤더만 현행화. 779파일 재생성 없음. evidence 목록은 04275/04277를 다 싣지 않음 |
| Open PR | **OPEN** | 유지. 24일 | `#5` adapter 은퇴, mergeable, 2026-08-27 이후 갱신 없음 |
| 로컬 09-06 감사 문서 | **OPEN (이번 작업으로 추적 준비)** | untracked 초안 | 이 checkpoint가 역사 snapshot으로 보존하고 09-20을 현재 판정으로 둔다 |
| Public stable release | NOT READY | 유지 | 의도적 out-of-scope |
| 이 checkpoint의 전체 테스트 재실행 | **NOT RUN** | 동일 | 회로 차단기 Lane 1. 최신 CI PASS를 대리 증거로 기록 |

## 4. 현재 operational tuple

`docs/ga-ready/current-evidence.json` 실측. 09-06과 동일하다.

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

기능 승격 ledger 4종(`pcv.checkpoint.restore`, `pcv.vm.managed-import`,
`pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)은 모두 `0.42.75-admin-smoke` /
`verdict=pass`다. `pcv.vm.clone`은 surface ledger에만 있고 evidence ledger 행이 없다.

## 5. 09-06 권고 이행

| 권고 | 상태 | 근거 |
| --- | --- | --- |
| 1. `0.42.75 -> 0.42.77` pair를 열지 probe-only로 둘지 명시 | **OPEN** | 승인 문장 없음. descriptor는 계속 `not-opened-awaiting-next-product-payload` |
| 2. 승격 시 clean `04b3c9f`와 operational `9f051b5` MSI를 섞지 말 것 | **OPEN (조건)** | 승격이 없어서 위반은 없음. 주의는 유효 |
| 3. clone을 feature evidence ledger 후보로 넣을지 결정 | **OPEN** | evidence ledger에 clone 행 없음 |
| 4. PR `#5` 존폐 | **OPEN** | `MERGEABLE`, draft 아님, 2026-08-27 이후 커밋/리뷰 없음 |
| 5. stale worktree `04275-stage1-immutable-preflight` 정리 | **OPEN** | 계속 `216a6ca` |
| 6. `artifacts/` 장기 보존 | **OPEN** | ignore 유지 |
| 7. SERVICE_PLAN/DOCUMENTATION_INDEX 시제 | **부분** | 카탈로그 헤더만 이 checkpoint가 09-20으로 고침. 기획 본문과 전체 파일 목록은 snapshot |

09-06이 닫힌 것으로 읽은 08-05 P1(프론트 가짜 상태, untracked 08-05 문서, `main` 양방향 분기,
ADR 검증 명령)은 재개방하지 않는다. 소스 재확인 결과 Web `connectionState` 바인딩은 유지된다.

## 6. 09-06 이후 궤적

공개 clone의 마지막 통합은 2026-08-30 PR #10이다. 09-06부터 09-20 사이 origin/main 커밋은
없다.

| 시점 | 상태 |
| --- | --- |
| 2026-08-30 | PR #10 merge `2e63bd5`. 04277 fullgate + current-card 기록, current 유지. CI 마지막 성공 |
| 2026-09-06 | 권위 재확인 초안. 당시 이 호스트 `installed_current=absent`. 문서는 untracked로 남음 |
| 2026-09-20 | 본 현행화. fetch 후 HEAD 불변. 설치본은 04275로 읽힘. pair/PR #5/clone ledger는 그대로 |

지난 14일의 제품 작업은 **0건**이다. 변한 관측은 이 호스트 설치본 존재와, fetch prune으로
사라진 remote 브랜치 2개(`origin/codex/p0-attach-dvd-readback`,
`origin/docs/04275-promotion-public-boundary-ci`)뿐이다.

## 7. HEAD가 current보다 앞선 이유

09-06 §7과 같다. 문서는 정직하고, 승격만 없다.

| 평면 | `0.42.77` | current에 반영? |
| --- | --- | --- |
| clone 코드 (API/CLI/Web/Hyper-V) | origin/main | 소스만. 이 호스트 04275 MSI에는 없음 |
| probe package | `admin-smoke-package-2026-08-29-04277` PASS | `canonical_current_changed=false` |
| clone actual-VM | 04277-r2 PASS | feature ledger 미변경 |
| fullgate | `full-admin-host-mutation-gate-20260830-04277` PASS | current unchanged |
| installed current-card | 2026-08-30 PASS, `not-promoted` | 04275 카드가 canonical |
| package-pair | `0.42.75 -> 0.42.77` `not-opened` | next descriptor도 `not-opened` |

04277 current-card가 명시한 승격 조건은 Lane 3 ledger update와 package-pair의 **별도 승인**이다.
worktree `codex/04277-manual-admin-promotion`은 같은 `2e63bd5`에 있으며 추가 커밋은 없다.

clean package provenance는 `04b3c9f`이고 operational fullgate provenance는 `9f051b5`다.
Host/CLI/MSI/payload aggregate가 서로 다르다. 승격 시 어떤 MSI를 current tuple에 넣을지
섞지 말아야 한다.

`docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`의 next 값은 04275 승격 시점
snapshot이다.

```text
next_manual_admin_package_pair_trigger: product-payload-change-after-04275
next_manual_admin_package_pair_candidate_status: not-opened-awaiting-next-product-payload
next_manual_admin_package_pair_payload_changed_source_file_count: 0
```

소스 HEAD에는 clone product payload가 이미 있다. 이 `0`은 **04275 승격 직후 snapshot**이며
현재 HEAD의 payload diff가 아니다. 이 감사는 descriptor를 고치지 않는다.

## 8. 대형 모듈

HEAD가 09-06과 같으므로 라인 수 재실측은 하지 않는다. 09-06 raw count와 라쳇 상한 carry-forward:

| 파일 | 09-06 실측 | 라쳇 상한 |
| --- | ---: | ---: |
| `DesktopNodeHostServiceAction.cs` | 563 | 563 |
| `DesktopNodeApiRequestProcessor.cs` | 449 | 449 |
| `DesktopNodeHyperVNativeAdapter.cs` | 643 | 643 |
| `web/src/served-app.ts` | 429 | 429 |
| `DesktopNodeHyperVWmiVmCloneProvider.cs` | 1,165 | 1,165 |
| `DesktopNodeApiVmMutationRouteHandler.cs` | 989 | 989 |
| `DesktopNodeHyperVNativeAdapter.Mutations.cs` | 925 | 925 |
| `DesktopNodeApiJobReconciliationHandler.cs` | 1,139 | 1,139 |

원본 4파일과 clone 모듈은 상한에 붙어 있다. 소스 정지가 라쳇 역행을 숨기지는 않는다. 다음
제품 변경이 상한을 넘기면 CI가 막는다.

## 9. 저장소 위생

| 항목 | 값 |
| --- | --- |
| working tree (조사 시작) | `DEVELOPER_INDEX.md` / `DOCUMENTATION_INDEX.md` 수정, `project-status-audit-2026-09-06.md` untracked |
| local branch | 11 |
| remote branch | 9 (fetch prune 후. 09-06은 11) |
| `[gone]` | 0 |
| worktree | 3 (main, `04275-stage1-immutable-preflight` @ `216a6ca`, `04277-manual-admin-promotion` @ `2e63bd5`) |
| 열린 PR | `#5` `codex/retire-p0-product-vm-state-adapter` (2026-08-27~, `MERGEABLE`, +17/−8) |
| 2026-08-30 이후 origin/main 커밋 | 0 (21일 정지) |

`04275-stage1-immutable-preflight`는 main보다 뒤처진 커밋이다. 삭제는 승인 후다.

## 10. 남은 제품 공백

SERVICE_PLAN `docs/SERVICE_PLAN.md`의 기획 표는 04273 시점 snapshot이다. 기능 존재는 아래
현재 소스로 읽는다. 09-06 §10과 같다.

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

이 감사는 구현·승격·host mutation을 실행하지 않는다. 순위는 09-06과 같고, 21일 정지가
우선도를 높인다.

| 순위 | 항목 | 완료 조건 |
| ---: | --- | --- |
| 1 | `0.42.75 -> 0.42.77` pair를 열지, 04277을 probe-only로 둘지 명시 | Lane 3 승인 또는 “비승격 유지” 선언. descriptor `awaiting-payload`와 HEAD clone 불일치를 닫을 것 |
| 2 | 승격한다면 clean `04b3c9f`와 operational `9f051b5` MSI를 섞지 말 것 | current tuple이 하나의 provenance/MSI SHA만 가리킴 |
| 3 | clone을 feature evidence ledger 후보로 넣을지 결정 | 행을 추가하거나 `not-assessed`를 명시적으로 유지 |
| 4 | PR `#5` 존폐 | merge, 닫기, 또는 명시적 보류 |
| 5 | stale worktree `04275-stage1-immutable-preflight` 정리안 | 승인 후 삭제 또는 목적 기록 |
| 6 | `artifacts/` 장기 보존 | 08-05 P2-3과 동일. 이번 checkpoint 밖 |
| 7 | SERVICE_PLAN/전체 카탈로그 시제 | 기획 문서는 snapshot. 전체 파일 재생성은 별도 작업 |

이 호스트 설치본이 ledger와 같아졌으므로, 09-06의 “설치 부재”는 더 이상 1순위가 아니다.
다음 일은 새 P1-6 기능을 쌓는 것이 아니라 pair/승격 한 줄을 고정하는 것이다.

## 12. 최종 결론

**운영 current `0.42.75-admin-smoke`는 닫힌 상태다.** P0 4종, functional carry-forward,
manual-admin pair, installed current-card, feature qualification `promotion_eligible=true`가
같은 버전에 모여 있다. 이 호스트의 설치본도 오늘 그 버전에 정렬돼 있다.

**소스 HEAD `2e63bd5`는 21일째 clone을 포함한 다음 후보를 가지고 있다.** 04277 package,
fullgate, current-card, clone actual-VM r2는 PASS이지만 current가 아니다. 09-06 이후 제품
커밋은 없다. 다음 일은 기능을 더 쌓는 것이 아니라, pair/승격을 승인할 것인지 probe-only로
남길 것인지를 한 줄로 고정하는 것이다.

공개 release 경계는 변함없다. 이 evidence는 internal admin-smoke 범위이며 public trusted
signing과 외부 stable publication을 주장하지 않는다.

## 13. 2026-09-20 B 재조사 (`0.42.75 -> 0.42.77` Lane 3)

사용자는 pair/승격 경로 B를 승인했다. 이 절은 gitignored `artifacts/`와 승격 worktree를
읽은 결과이며 `current-evidence.json`을 쓰지 않는다.

문서 descriptor의 `not-opened`는 **실행 부재가 아니다.** 로컬 campaign은 이미 돌았고,
공식 closed descriptor와 ledger 승격만 없다.

| 실행 | 결과 |
| --- | --- |
| `manual-admin-campaign-20260831-04275-04277` | readiness/update-rollback PASS, clean-host FAIL |
| `...-final-service-state-r1` | 위와 같음, clean-host FAIL |
| `ma-04275-04277-fss-r2` | readiness/update-rollback/clean-host PASS, Burn `remove-absence product remains` FAIL |
| `ma-04275-04277-lane2-burn-20260906` | 남은 Burn/MSIX/ops-summary PASS. `all_buckets_pass=true` |
| `ma-04275-04277-c1` / `c2` | 공식 단일 root consume 미완료. c2에 update ZIP·catalog·consume runner 존재 |

09-06 remaining summary의 자체 판정:

- `ok=true`, 여섯 bucket PASS
- pair target MSI는 clean package `d03eedaf…` (`operational_fullgate_msi_not_used=true`)
- `descriptor_eligible=false`
- 이유: `six-bucket PASS exists across two artifact roots; official single-root descriptor consume was not run`
- `current_evidence_written=false`
- 당시 설치본 `0.42.77-admin-smoke`. 오늘 설치본은 다시 `0.42.75-admin-smoke`

Burn 최초 FAIL 원인은 04277 clean MSI(`04b3c9f`)와 operational fullgate MSI(`9f051b5`)가
같은 `ProductVersion`/`UpgradeCode`에 다른 `ProductCode`로 등록된 것이다. 설계
`docs/superpowers/specs/2026-08-31-purecvisor-desktop-node-immutable-msi-identity-design.md`는
worktree `codex/04277-manual-admin-promotion`에만 있고 main에 없다. campaign runner
`Invoke-PcvManualAdminPackagePairCampaign.ps1`도 그 worktree의 untracked 파일이다.

따라서 B의 남은 공식 게이트는 새 기능이 아니라 **단일 artifact root closed descriptor**다.
이 checkpoint는 host mutation과 Lane 3 ledger write를 하지 않는다.

### 13.1 B1 descriptor consume (같은 날 후속)

`New-PcvManualAdminCampaignDescriptor -PlanOnly`를 단일 root
`artifacts/manual-admin-campaign-20260920-04275-04277`에서 실행했다.

| 필드 | 값 |
| --- | --- |
| descriptor | `manual-admin-campaign-descriptor-20260920-04275-04277` |
| `ok` / `overall_status` | `true` / `pass` |
| `runner_count` / `missing_count` / `not_pass_count` | `6` / `0` / `0` |
| 이 consume의 host mutation | `false` |
| `current-evidence.json` | 유지 `0.42.75-admin-smoke` |
| 증거 | `docs/ga-ready/evidence/manual-admin-campaign-2026-09-20-04275-04277.md` |

여섯 runner status는 모두 `pass`다. Lane 3 ledger write와 이 호스트의 04277 재설치는
아직 열리지 않았다.

## 부록 A. 이 checkpoint에서 실행한 명령

```powershell
git fetch origin --prune
git status --short --branch
git rev-parse HEAD
git rev-parse origin/main
git rev-list --left-right --count main...origin/main
git worktree list
git branch -vv
git log -15 --format='%ad %h %s' --date=short
gh run list --branch main --limit 12
gh pr list --state open
gh pr view 5 --json title,state,createdAt,updatedAt,mergeable,isDraft,url
Get-Content docs/ga-ready/current-evidence.json
Get-Content 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json'
Get-Service -Name PureCVisorDesktopNode
# uninstall DisplayVersion via HKLM Uninstall
```

Required CI 네 shard, `dotnet test`, npm required, 설치본 HTTP/CLI current-card, host
mutation은 실행하지 않았다. `origin/main` 최신 CI PASS를 개발 검증의 대리 증거로 쓴다.

## 부록 B. 주요 근거

- `docs/ga-ready/current-evidence.json`
- `docs/project-status-audit-2026-09-06.md`
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
