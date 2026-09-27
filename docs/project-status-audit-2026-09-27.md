# PureCVisor Desktop Node 프로젝트 진행 상황 현행화

- 조사 기준 시각: 2026-09-27 (Asia/Seoul)
- 조사 대상: `D:\data\projects\codex-zone\purecvisor-desktop-node-public`
- 공개 소스 권위: `HardcoreMonk/purecvisor-desktop-node-public`
- 기준 브랜치/커밋: `main` / `9959d188a35a7153bb35b223b9e33edeeef5d15d` (PR #12 merge)
- 원격 정합: fetch 후 `main == origin/main`
- 선행 감사: `docs/project-status-audit-2026-09-20.md` (7일 전), `docs/project-status-audit-2026-09-06.md` (21일 전)
- 조사 방식: 권위 문서·evidence·git history·GitHub CI/PR·이 호스트 product-manifest 직접 점검
- 제외: 관리자 MSI/SCM/Hyper-V/방화벽 mutation, 실제 VM 조작, `current-evidence.json` 변경, public publication

이 문서는 작성 시점 snapshot이다. operational current 숫자는
`docs/ga-ready/current-evidence.json`이 소유하며, 이 감사가 그 JSON을 바꾸지 않는다.

## 1. 종합 판정

**종합 상태: 운영 current, 이 호스트 설치본, 소스 HEAD가 모두 `0.42.78-admin-smoke` 계열로 정렬됐다(GREEN). 다음 일은 feature evidence와 검증 표면의 폭을 넓히는 것이다.**

09-20 감사의 1순위(“pair/승격 한 줄을 고정”)는 두 번 닫혔다. 같은 날 `0.42.77`이 승격됐고,
7일 뒤 `0.42.78`이 승격됐다.

1. **운영 current**는 `0.42.78-admin-smoke`다(PR #12, `9959d18`). carry-forward 승격이며
   feature qualification `promotion_eligible=true`, blockers `none`, 닫힌 pair
   `0.42.77-admin-smoke -> 0.42.78-admin-smoke`다.
2. **이 호스트 설치본**은 `0.42.78-admin-smoke`이고 ARP `0.42.78` 항목은 `1`개다. 설치본
   Host/CLI는 fullgate r2 build `0de176f`다.
3. **소스 HEAD**는 0.42.78 build 뒤로 제품 동작 변경이 없다. 제품 경로 변경은 라쳇 복구의
   순수 이동 `27`개 파일뿐이며 다음 package pair가 담는다.

공개 trusted signing과 외부 stable publication은 계속 out-of-scope다.

## 2. 세 권위

| 권위 | 값 | 출처 | 09-20 대비 |
| --- | --- | --- | --- |
| ledger_current | `0.42.78-admin-smoke` | `docs/ga-ready/current-evidence.json` | `0.42.75` → `0.42.77`(09-20) → `0.42.78`(09-27) |
| installed_current | `0.42.78-admin-smoke` | `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json` | `0.42.75` → `0.42.78` |
| source_head | `9959d18` | `origin/main` | `2e63bd5`에서 `55`커밋 전진 |

## 3. 09-20 이후 궤적

`2e63bd5..9959d18`은 `55`커밋(merge `2`, 비-merge `53`)이다. 비-merge 분류는 feat `22`,
docs `18`, refactor `6`, test `4`, fix `3`이다.

| 날짜 | 흐름 | 근거 |
| --- | --- | --- |
| 09-20 | `0.42.77` Lane 3 승격, P1-6 inventory 시각/메모, P1-7 template lock | `a842ede` |
| 09-20~21 | P1-8 guest file, P1-9 account create/disable, P1-10 reconcile(create/shutdown/restart/QoS), P2-11 noVNC target, P2-12 checkpoint schedule, P2-13 export/import | feat commit 다수 |
| 09-25 | P2-14 switch service action, P2-15 NIC/DVD add, `0.42.78` package pair evidence | `e098e0a`, `240c53e` |
| 09-26~27 | fullgate FAIL(호스트 vmswitch 드라이버 시작 유형) → 호스트 복구 → 잔여 ProductCode 정리 → r2 PASS, current-card PASS | `0de176f`, `d5d1a47` |
| 09-27 | 캠페인 러너(`pcv-campaign-runner-v1`), 모듈 크기 라쳇 복구, 기존 실패 테스트 복구, PR #11 merge | `ecffd76`~`17563ee`, `3c677a4` |
| 09-27 | `0.42.78` Lane 3 carry-forward 승격, PR #12 merge | `a28244d`~`53a6b73`, `9959d18` |

## 4. 현재 operational tuple

| 항목 | 값 |
| --- | --- |
| package | `admin-smoke-package-2026-09-25-04278`, clean MSI `c3390c1e…`, source `e098e0a` |
| fullgate | `full-admin-host-mutation-gate-20260927-04278-r2`, operational MSI `0856d07e…`, payload `2dfabb93…`, provenance `0de176f` |
| installed current-card | `installed-operator-surface-current-card-2026-09-27-04278` (`promoted-current`) |
| functional | `functional-correctness-actual-host-validation-2026-09-27-04278-carryforward` (0.42.75 PASS) |
| manual-admin pair | `manual-admin-campaign-2026-09-27-04277-04278` / descriptor `manual-admin-campaign-descriptor-20260927-04277-04278` |
| feature qualification | P0 candidate `4`개 모두 0.42.75 actual-VM PASS, blockers `none` |

## 5. 검증 상태

| 표면 | 결과 |
| --- | --- |
| `main` `9959d18` Development Gates | run `36301817135` success (네 shard) |
| `main` `9959d18` Public Boundary | run `36301817180` success |
| 로컬 `dotnet test src/DesktopNode.sln` | 실패 `0` (Delivery `706`, Verification `557`, Api `410` 등) |
| 로컬 Required CI 네 shard | exit `0` (clean HEAD `ab88076`) |
| 모듈 크기 라쳇 | 등록 `14`개 모두 상한 이하 |

09-27에 고친 기존 실패는 다섯 건이다. `Assert.Fail(digest)`가 남은 route snapshot 테스트,
계약과 모순된 `qos/network` 단언, current 버전 기대값, registry 개수 단언, 그리고
`CutoverGitBoundary` history 출력이 8192자 상한을 넘어 `policy-boundaries`가 깨지는 시한폭탄이다.

## 6. 09-20 권고 이행

| 순위 | 권고 | 결과 |
| ---: | --- | --- |
| 1 | `0.42.75 -> 0.42.77` pair 결정 | 닫힘. 09-20 승격, 이어 09-27 `0.42.78` 승격 |
| 2 | clean/operational MSI 혼동 금지 | 지켜짐. current tuple은 clean `c3390c1e…`와 operational `0856d07e…`를 다른 identity로 기록 |
| 3 | clone을 feature evidence ledger 후보로 넣을지 결정 | 미결. ledger는 여전히 `pcv.vm.clone`을 `not-assessed`로 두며 note는 04277 clone actual-VM r2 PASS를 반영하지 않는다 |
| 4 | PR `#5` 존폐 | 미결. 2026-08-27부터 열려 있다 |
| 5 | stale worktree `04275-stage1-immutable-preflight` | 미결 |
| 6 | `artifacts/` 장기 보존 | 미결 |
| 7 | SERVICE_PLAN/카탈로그 시제 | 부분. 이 문서 §9가 현재 기능 상태를 다시 적는다 |

## 7. 대형 모듈

라쳇 등록 `14`개는 09-27 복구 뒤 모두 상한 이하다. 두 대형 handler는 partial로 나뉘어
`DesktopNodeApiJobReconciliationHandler.cs` `674`, `DesktopNodeApiVmMutationRouteHandler.cs`
`657`이다.

라쳇에 등록되지 않은 비테스트 source 중 `500`줄을 넘는 파일이 `17`개 있다. (처음 작성 때 목록을 앞 `12`개로 잘라 `12`개로 적었다가 backlog Task 1에서 바로잡았다.) 라쳇은 등록된
모듈만 막으므로 이 파일들은 증가를 막는 장치가 없다.

| 줄 | 파일 |
| ---: | --- |
| 1,412 | `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs` |
| 1,066 | `src/DesktopNode.Api/DesktopNodeAccountAuth.cs` |
| 947 | `src/DesktopNode.Runtime/JsonFileDesktopNodeJobStore.cs` |
| 924 | `src/DesktopNode.Cli/DesktopNodeCliFormatter.cs` |
| 907 | `src/DesktopNode.Verification/CurrentEvidenceVerifier.cs` |
| 788 | `src/DesktopNode.Verification/VerificationSummaryWriter.cs` |
| 702 | `src/DesktopNode.HyperV/DesktopNodeHyperVPowerShellDirectGuestExecutionProvider.cs` |
| 675 | `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmCreateProvider.cs` |
| 665 | `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmResourceMutationProvider.cs` |
| 664 | `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs` |
| 644 | `src/DesktopNode.Verification/VerificationProcess.cs` |
| 594 | `web/src/served/mutate.ts` |
| 573 | `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmProvider.cs` |
| 547 | `src/DesktopNode.Api/DesktopNodeApiOpsSummaryBuilder.cs` |
| 535 | `src/DesktopNode.Verification/VerificationPolicy.cs` |
| 533 | `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` |
| 523 | `src/DesktopNode.Api/DesktopNodeApiDiagnosticsHandler.cs` |

## 8. 저장소 위생

| 항목 | 값 |
| --- | --- |
| local branch | 14 (09-20: 11) |
| remote branch | 10 (09-20: 9) |
| `[gone]` | 0 |
| worktree | 3 (main checkout, `04275-stage1-immutable-preflight` @ `216a6ca`, `04277-manual-admin-promotion` @ `2e63bd5`) |
| 열린 PR | `#5` `codex/retire-p0-product-vm-state-adapter` (2026-08-27~) |
| `DOCUMENTATION_INDEX.md` 권위 줄 | 이 감사에서 HEAD와 CI run을 현행화했다 |

merge된 `feat/p1-9-account-crud`, `docs/04278-lane3-promotion` 원격 branch는 남아 있다. 삭제는
승인 후다.

## 9. 남은 제품 공백

SERVICE_PLAN의 P0~P2 기획 항목은 모두 소스에 들어왔다. 공백은 기능 존재가 아니라 evidence
폭이다.

| 영역 | 현재 | 다음 |
| --- | --- | --- |
| feature evidence ledger | 28 feature 중 P0 `4`개만 5단계 PASS, `24`개 `not-assessed` | code/packaged/installed 단계부터 채우고, `pcv.vm.clone`은 04277 clone actual-VM r2를 반영할지 결정 |
| P1-8~P2-15 actual-VM | code/CLI/API 계약만 있음 | Lane 2 actual-VM 검증(host mutation 승인 필요) |
| 다음 package pair | 04278 build 뒤 순수 이동 `27`개 파일 | 다음 제품 변경과 함께 package pair를 연다 |
| 같은 version 재빌드 | fullgate 재실행마다 새 ProductCode가 공존해 설치본 교체와 uninstall 증명을 막는다 | installer 설계 결정(WiX same-version upgrade 정책 또는 gate build version 규칙) |
| 대형 모듈 | 미등록 `500`줄 초과 `17`개 | 라쳇 등록(동결) 또는 분해 결정 |
| CI runner | GitHub `ubuntu-latest`가 2026-10-19부터 Ubuntu 26으로 이동 | web job 영향 확인 |

## 10. 권고 실행 순서

이 감사는 구현·승격·host mutation을 실행하지 않는다. 순위는 다음 backlog 계획
(`docs/superpowers/plans/2026-09-27-purecvisor-desktop-node-post-04278-backlog.md`)이 소유한다.

| 순위 | 항목 | 완료 조건 |
| ---: | --- | --- |
| 1 | 미등록 대형 모듈 `17`개 처리 방식 결정과 적용 | 라쳇이 `500`줄 초과 비테스트 source를 모두 덮음 |
| 2 | feature evidence ledger 현행화 | `pcv.vm.clone` 등 evidence가 있는 feature의 stage를 기록하거나 `not-assessed` 유지 사유를 명시 |
| 3 | 같은 version 재빌드 installer 정책 설계 | 설계 문서와 결정 |
| 4 | Ubuntu 26 runner 영향 점검 | web/public-boundary job이 새 runner에서 통과하거나 pin 결정 |
| 5 | PR `#5`, stale worktree, merge된 원격 branch 정리 | 승인 후 정리 또는 보류 기록 |
| 6 | P1-8~P2-15 actual-VM 검증과 다음 package pair | Lane 2 승인 뒤 |

## 11. 최종 결론

**운영 current `0.42.78-admin-smoke`는 닫힌 상태다.** package, fullgate r2, installed
current-card, manual-admin pair consume, functional carry-forward, feature qualification이 같은
버전에 모였고, 이 호스트 설치본과 소스 HEAD도 그 버전에 정렬돼 있다. `main`의 Development
Gates와 Public Boundary도 green이다.

다음 일은 기능을 더 쌓기보다 evidence 폭과 검증 장치의 사각지대(미등록 대형 모듈 `17`개, feature
ledger `not-assessed`, 같은 version 재빌드)를 줄이는 것이다.

공개 release 경계는 변함없다. 이 evidence는 internal admin-smoke 범위이며 public trusted
signing과 외부 stable publication을 주장하지 않는다.

## 부록 A. 이 checkpoint에서 실행한 명령

```powershell
git fetch origin; git switch main; git merge --ff-only origin/main
git rev-list --count 2e63bd5..9959d18
git log --no-merges --format='%s' 2e63bd5..9959d18
git branch -a; git branch -vv; git worktree list
gh pr list --state open
gh run list --branch main --limit 4
```

## 부록 B. 주요 근거

- `docs/ga-ready/current-evidence.json`
- `docs/superpowers/plans/2026-09-27-purecvisor-desktop-node-04278-lane3-promotion.md`
- `docs/superpowers/plans/2026-09-27-purecvisor-desktop-node-module-ratchet-recovery.md`
- `docs/superpowers/plans/2026-09-27-purecvisor-desktop-node-baseline-test-repair.md`
- `docs/FEATURE_IMPLEMENTATION_LEDGER.md`
- `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`
