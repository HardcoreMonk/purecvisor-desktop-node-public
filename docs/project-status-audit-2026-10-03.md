# PureCVisor Desktop Node 프로젝트 진행 상황 현행화

- 조사 기준 시각: 2026-10-03 (Asia/Seoul)
- 조사 대상: `D:\data\projects\codex-zone\purecvisor-desktop-node-public`
- 공개 소스 권위: `HardcoreMonk/purecvisor-desktop-node-public`
- 기준 브랜치/커밋: `origin/main` / `2057b403386134ac0abc8444a81c7ec532986daf` (PR #27 merge)
- 원격 정합: fetch 후 작업 branch `docs/project-status-audit-20261003`를 `origin/main`에서 만들었다. 로컬
  `main`은 `origin/main`보다 `17`커밋 뒤이며 이 감사는 그것을 옮기지 않았다.
- 선행 감사: `docs/project-status-audit-2026-09-27.md` (6일 전), `docs/project-status-audit-2026-09-20.md` (13일 전)
- 조사 방식: 권위 문서·evidence·git history·GitHub CI/PR·이 호스트 product-manifest/ARP/서비스 상태 읽기 전용 점검
- 제외: 관리자 MSI/SCM/Hyper-V/방화벽 mutation, 실제 VM 조작, `current-evidence.json` 변경, public publication

이 문서는 작성 시점 snapshot이다. operational current 숫자는
`docs/ga-ready/current-evidence.json`이 소유하며, 이 감사가 그 JSON을 바꾸지 않는다.

## 1. 종합 판정

**종합 상태: 운영 current, 이 호스트 설치본, 소스 HEAD가 모두 `0.42.86-admin-smoke` 계열로 정렬됐다(GREEN). 6일 동안 승격이 세 번 있었고, 큐에 남은 제품 작업은 없다.**

09-27 감사 뒤 운영 current는 `0.42.78` → `0.42.83`(09-29) → `0.42.84`(09-30) → `0.42.86`(10-02)으로
옮겨 갔다. `0.42.79`~`0.42.82`는 Lane 2 probe를 실어 나른 package였다. `0.42.85`는 pair를 닫았지만
`vm.eject` 결함이 드러나 승격하지 않았다.

1. **운영 current**는 `0.42.86-admin-smoke`다(PR #27, `2057b40`). feature qualification
   `promotion_eligible=true`, blockers `none`, 닫힌 pair `0.42.85-admin-smoke -> 0.42.86-admin-smoke`다.
2. **이 호스트 설치본**은 `0.42.86-admin-smoke`이고 ARP `0.42.86` 항목은 `1`개, 서비스
   `PureCVisorDesktopNode`는 `Running/Automatic`이다. 설치본은 fullgate build(operational MSI
   `85387f31…`, provenance `b807803`)다.
3. **소스 HEAD** `2057b40`은 provenance `b807803` 뒤로 제품 동작 변경이 없다. 그 사이 차이는 PR #27의
   문서·evidence, 테스트 파일 `4`개의 기대 버전 `4`줄, `packaging/windows-desktop-node/README.md` 문구다.
   그래서 지금 열어야 할 package pair가 없다.

공개 trusted signing과 외부 stable publication은 계속 out-of-scope다.

## 2. 세 권위

| 권위 | 값 | 출처 | 09-27 대비 |
| --- | --- | --- | --- |
| ledger_current | `0.42.86-admin-smoke` | `docs/ga-ready/current-evidence.json` | `0.42.78` → `0.42.83` → `0.42.84` → `0.42.86` |
| installed_current | `0.42.86-admin-smoke` | `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json` (`generated_at` `2026-10-02T06:25:33Z`), ARP `{1994F4DF-A62D-497F-83FF-39FEB994FA8D}` | `0.42.78` → `0.42.86` |
| source_head | `2057b40` | `origin/main` | `9959d18`에서 `142`커밋 전진 |

## 3. 09-27 이후 궤적

`9959d18..2057b40`은 `142`커밋(merge `16`, 비-merge `126`)이다. 비-merge 분류는 docs `73`, feat `30`,
test `15`, fix `8`이다. merge `16`개 중 `15`개가 PR #13~#27이고, 나머지 하나는 PR #25 branch에 `main`을
합친 `7ba7bf7`이다.

| 날짜 | 흐름 | PR |
| --- | --- | --- |
| 09-27 | post-0.42.78 backlog Task 1~5: 미등록 대형 모듈 라쳇 등록, feature stage 기록, 같은 version 재빌드 설계, Ubuntu 26 점검 | #13 |
| 09-27~28 | P2 Off-VM Lane 2: checkpoint schedule, device add, export/import, network connect actual-VM PASS. probe 운반 package `0.42.79`~`0.42.81` | #14 |
| 09-28 | 잔여 결함: 이름 있는 external switch 분류, DVD guard가 inventory DVD drive를 보게 함 | #15 |
| 09-28 | P1-8 guest file Lane 2 PASS (probe 운반 package `0.42.82`) | #16 |
| 09-29 | manual-admin descriptor chain 생성, Lane 3 promotion 문서 자동화(`Invoke-PcvLane3PromotionDocs.ps1`), `0.42.83` Lane 3 승격, package-pair orchestrator 가져오기 | #17~#19 |
| 09-30 | clean-host base VHD 오프라인 갱신(`current-base.json`, `20348.5622`), 권위 줄 현행화 | #20, #21 |
| 09-30 | development completion Task 1~13: 모든 mutation의 reconcile 분류, Web의 pause/resume/rename, telemetry, checkpoint schedule, export/import, switch connect/device add, guest exec/channel preview. `0.42.84` package pair와 Lane 2 actual-VM PASS | #22 |
| 09-30 | `0.42.84` Lane 3 승격, vm.list readback(`dvd_media`, 내부 `vm.disk.inspect`, media attach/eject reconcile, Web DVD 표시) | #23, #24 |
| 10-01 | `0.42.85` package pair: six bucket, fullgate, current-card PASS. Lane 2 readback probe `PARTIAL`(`vm.eject`가 `0.42.74`부터 실패, 빈 DVD drive에 attach 불가) | #25 |
| 10-02 | eject/빈 drive attach 수정 `fb95de1`. `0.42.86` package, Lane 2 media probe PASS, pair, fullgate, current-card, Lane 3 승격 | #26, #27 |

10-02 작업의 승인 문구와 실행 기록은 닫힌 지시서
`docs/superpowers/plans/2026-10-02-purecvisor-desktop-node-development-work-order.md`가 소유한다.

## 4. 현재 operational tuple

| 항목 | 값 |
| --- | --- |
| package | `admin-smoke-package-2026-10-02-04286`, clean MSI `8edb19ce…`, provenance `1c488b6` |
| fullgate | `full-admin-host-mutation-gate-20261002-04286`, operational MSI `85387f31…`, payload `bba7e10c…`, provenance `b807803` |
| installed current-card | `installed-operator-surface-current-card-2026-10-02-04286` (`promoted-current`, 캡처 artifact summary는 `not-promoted`로 보존) |
| Lane 2 media probe | `lane2-vm-media-eject-attach-2026-10-02-04286` (`PASS`) |
| functional | `functional-correctness-actual-host-validation-2026-10-02-04286-carryforward` (`0.42.75` PASS에서 carry-forward) |
| manual-admin pair | `manual-admin-campaign-2026-10-02-04285-04286` / descriptor `manual-admin-campaign-descriptor-20261002-04285-04286-consume` |
| feature qualification | P0 `4`개 5단계 pass(`0.42.75` evidence), blockers `none` |

## 5. 검증 상태

| 표면 | 결과 |
| --- | --- |
| `main` `2057b40` Development Gates | run `36996858669` success |
| `main` `2057b40` Public Boundary Contract | run `36996858460` success (job `110805450371`) |
| 직전 merge `b807803`, `1c488b6` | 두 workflow 모두 success |
| 로컬 Full shard 네 개 | 10-02 clean HEAD `2b6ef2b` 전용 clone에서 `ok=true` (작업 지시서 기록) |
| 모듈 크기 라쳇 | 등록 `31`개 모두 상한 이하. 미등록 `500`줄 초과 비테스트 `.cs`/`.ts` `0`개 |

10-02에 Temp 아래 worktree에서 돌린 shard는 Hyper-V 테스트 `2`개와 web 테스트 `1`개가 실패했다.
`.git`이 파일이고 경로가 `os.tmpdir()` 안이라는 환경 원인이며, 그 실행은 PASS 근거로 쓰지 않았다.

## 6. 09-27 권고 이행

| 순위 | 권고 | 결과 |
| ---: | --- | --- |
| 1 | 미등록 대형 모듈 `17`개 | 닫힘. backlog Task 1(`70e6640`) 뒤 라쳇 등록은 `31`개이고 미등록 `500`줄 초과는 `0`개다 |
| 2 | feature evidence ledger 현행화 | 부분. Task 2가 `not-assessed` 유지 사유와 `pcv.vm.clone` non-claim을 고쳤다. stage 표는 여전히 `24`개가 `not-assessed`다 |
| 3 | 같은 version 재빌드 installer 설계 | 설계는 닫힘(`docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md`: C 먼저, A는 별도 L checkpoint, B 비권고). 구현은 없다. 10-02 pair와 fullgate는 시작 전에 `REMOVE_DATA` 없이 같은 version 설치본을 손으로 제거해 우회했다 |
| 4 | Ubuntu 26 runner 점검 | 점검 기록은 닫힘(09-27 감사 §12). workflow는 그대로 `ubuntu-latest`다. 이동일 2026-10-19까지 16일 남았다 |
| 5 | PR #5, stale worktree, 병합된 원격 branch 정리 | 미결. 오히려 늘었다(§8) |
| 6 | P1-8~P2-15 actual-VM과 다음 package pair | 대부분 닫힘(§9) |

## 7. 대형 모듈

라쳇 등록 `31`개(`packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`)는 모두 상한
이하이고, 그중 `8`개는 상한보다 작다. `src`와 `web/src`의 비테스트 `.cs`/`.ts` 중 `500`줄을 넘는
미등록 파일은 없다. 09-27의 사각지대는 닫혔다.

## 8. 저장소 위생

| 항목 | 값 |
| --- | --- |
| local branch | 31 (09-27: 14) |
| remote branch | 25, `origin/main` 포함 (09-27: 10). `origin/main`에 병합된 것 `23`, 병합 안 된 것 `1`(`codex/retire-p0-product-vm-state-adapter`, PR #5) |
| `[gone]` | 0 |
| worktree | 5 (09-27: 3): main checkout, `04275-stage1-immutable-preflight` @ `216a6ca`, `04277-manual-admin-promotion` @ `2e63bd5`, `pcv-public-wt-descriptor-chain` @ `f23db4a`(PR #17 병합됨), Temp `pcv-fullgate-04286` @ `2b6ef2b`(PR #27 병합됨) |
| 별도 clone | `D:\data\projects\codex-zone\pcv-04286-fullshard` (10-02 Full shard용) |
| 로컬 `main` | `origin/main`보다 `17` 뒤 (`5b84a4e`) |
| 원격 없는 로컬 branch | `docs/development-work-order-20261002` (`ccca32e`), `package/04286-admin-smoke-20261002` (`020ac4d`), `codex/pester-free-verification-cutover-successor` (`68756f1`) |
| 열린 PR | `#5` `codex/retire-p0-product-vm-state-adapter` (2026-08-27~, 37일) |
| `artifacts/` | `12G`, 항목 `174`개 |

이 감사를 시작할 때 main checkout은 `docs/development-work-order-20261002`에 있었고, `origin/main`과
바이트 단위로 같은 10-02 evidence `13`개가 미추적으로 남아 있었다. 이 감사는 그 `13`개를 지우고
`origin/main`에서 새 branch를 만든 뒤, `ccca32e`의 작업 지시서 파일을 이 branch에 담았다.
`ccca32e`와 그 branch는 그대로 있다. 나머지 정리는 승인 후다.

## 9. 남은 제품 공백

09-27 감사가 남긴 actual-VM 공백은 대부분 닫혔다. 이 감사가 확인한 09-27 이후 Lane 2 actual-VM
evidence는 다음과 같다. 표에 없는 family는 이 감사가 확인하지 않았다.

| family | evidence | 결과 |
| --- | --- | --- |
| P2-12 checkpoint schedule | `service-plan-p2-offvm-checkpoint-schedule-actual-vm-2026-09-27-04278` | `PASS` |
| P2-15 device add | `service-plan-p2-offvm-device-add-actual-vm-2026-09-27-04278-r2` | `PASS` |
| P2-13 export/import | `service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04281` | `PASS` |
| network connect | `service-plan-p2-offvm-network-connect-actual-vm-2026-09-28-04281` | `PASS` |
| P1-8 guest file | `service-plan-p1-guest-file-actual-vm-2026-09-28-04282` | `PASS` |
| development completion(pause/resume, rename, telemetry, switch 연결 등) | `lane2-development-completion-actual-vm-2026-09-30-04284` | `PASS` |
| vm.list readback | `lane2-vm-list-readback-actual-vm-2026-10-01-04285` | `PARTIAL` (eject 결함) |
| DVD eject / 빈 drive attach | `lane2-vm-media-eject-attach-2026-10-02-04286` | `PASS` |

| 영역 | 현재 | 다음 |
| --- | --- | --- |
| feature evidence ledger | 28 feature 중 P0 `4`개만 5단계 pass, `24`개 `not-assessed`. `pcv.vm.clone`(`0.42.77`)과 `pcv.vm.media-eject`(`0.42.86`)는 actual-VM PASS가 있지만 stage 표는 투영 규칙대로 `not-assessed`다 | ledger 모델에 stage별 evidence를 담을지 결정 |
| 같은 version 재빌드 | 설계만 있고, 매 실행 손으로 제거해 우회한다 | 권고 C(읽기 전용 잔여 ProductCode preflight)를 Lane 1로 구현 |
| CI runner | 2026-10-19 Ubuntu 26 이동. Public Boundary는 `shell: pwsh`라 중간 위험 | 10-19 전에 새 image에서 확인하거나 `ubuntu-24.04` pin 결정 |
| reconcile 안내 문구 | `0.42.84` Lane 2 evidence가 비대상 operation 안내가 "confirm whether the rename applied"로 나온다고 기록했다. 기본값 `"rename"`은 `src/DesktopNode.Runtime/DesktopNodeJobRuntime.Persistence.cs:247`에 남아 있다 | 중립 문구로 고치는 Lane 1 결함 수정 |
| 다음 package pair | `0.42.86` build 뒤 제품 변경 없음 | 다음 제품 변경과 함께 연다 |
| campaign 파일 | `next_step`이 이미 끝난 PR merge, `0.42.85` Lane 3, eject 수정을 다음 승인으로 적고 있었다 | 이 감사와 함께 새 campaign으로 바꿨다(부록 B) |

## 10. 권고 실행 순서

이 감사는 구현·승격·host mutation을 실행하지 않는다. 사용자가 2026-10-03에 순위 1~5를 승인했다
(`1,2,3,4,5 전부 모두 승인`). 실행 순서와 완료 조건은 backlog 계획
`docs/superpowers/plans/2026-10-03-purecvisor-desktop-node-post-04286-backlog.md`와 campaign
`post-04286-backlog-20261003`이 소유한다. 순위 6은 승인 밖이다.

| 순위 | 항목 | 완료 조건 |
| ---: | --- | --- |
| 1 | Ubuntu 26 runner 확인 (기한 10-19) | Public Boundary가 Ubuntu 26 image에서 통과하거나 pin 결정 |
| 2 | 같은 version 재빌드 권고 C 구현 | 같은 version 잔여 항목이 있으면 명확한 오류 코드로 멈추고, gate 뒤 설치본 build commit이 gate build와 같음을 확인 |
| 3 | reconcile 안내 문구 기본값 수정 | 비대상 operation 안내가 `rename`을 말하지 않음 |
| 4 | feature evidence ledger 모델 확장 결정 | actual-VM evidence가 있는 feature의 stage를 기록하거나 `not-assessed` 유지를 결정 |
| 5 | 저장소 위생: PR #5, 병합된 worktree, 병합된 원격 branch `23`개, 로컬 `main` fast-forward, `artifacts/` `12G` | 승인 후 정리 또는 보류 기록 |
| 6 | 남은 actual-VM과 다음 package pair | Lane 2 승인 뒤 |

## 11. 최종 결론

**운영 current `0.42.86-admin-smoke`는 닫힌 상태다.** package, Lane 2 media probe, pair
`0.42.85 -> 0.42.86`, fullgate, installed current-card, functional carry-forward, feature qualification이
같은 버전에 모였고, 이 호스트 설치본과 소스 HEAD도 그 버전에 정렬돼 있다. `main`의 Development
Gates와 Public Boundary Contract도 green이다.

09-27 이후 SERVICE_PLAN 기능의 actual-VM 공백은 대부분 PASS로 닫혔고, 그 과정에서 드러난
`vm.eject` 결함도 고쳐 승격까지 마쳤다. 남은 일은 기한이 있는 CI runner 확인, 같은 version 재빌드의
수동 우회 제거, feature ledger 표현력, 그리고 늘어난 branch·worktree 정리다.

공개 release 경계는 변함없다. 이 evidence는 internal admin-smoke 범위이며 public trusted
signing과 외부 stable publication을 주장하지 않는다.

## 부록 A. 이 checkpoint에서 실행한 명령

```powershell
git fetch origin --prune
git switch -c docs/project-status-audit-20261003 origin/main
git rev-list --count 9959d18..2057b40
git log --no-merges --format='%s' 9959d18..2057b40
git diff --stat b807803 2057b40 -- src web/src packaging
git branch -vv; git branch -r --merged origin/main; git worktree list
gh pr list --state open
gh run list --branch main --limit 6
gh run view 36996858460 --json jobs
Get-Content 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json'
Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
Get-Service PureCVisorDesktopNode
```

## 부록 B. 이 checkpoint의 변경

- 이 문서를 추가했다.
- `docs/DOCUMENTATION_INDEX.md` 현재 기준 블록의 HEAD, CI run, campaign, 현행화 줄과 핵심 문서 목록을 고쳤다.
- `docs/ga-ready/active-campaign.json`을 새 campaign `post-04286-backlog-20261003`으로 바꿨다. 닫힌
  선행 `package-pair-04285-20261001`은 `closed_predecessor`로 남는다.
- backlog 계획 `docs/superpowers/plans/2026-10-03-purecvisor-desktop-node-post-04286-backlog.md`를 추가했다.
- 로컬 `ccca32e`의 `docs/superpowers/plans/2026-10-02-purecvisor-desktop-node-development-work-order.md`를 가져왔다.

## 부록 C. 주요 근거

- `docs/ga-ready/current-evidence.json`
- `docs/superpowers/plans/2026-10-02-purecvisor-desktop-node-development-work-order.md`
- `docs/superpowers/plans/2026-10-01-purecvisor-desktop-node-04285-package-pair.md`
- `docs/superpowers/plans/2026-09-30-purecvisor-desktop-node-development-completion.md`
- `docs/superpowers/plans/2026-09-27-purecvisor-desktop-node-post-04278-backlog.md`
- `docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md`
- `docs/FEATURE_IMPLEMENTATION_LEDGER.md`
- `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

## 12. Ubuntu 26 runner probe (backlog Task 1)

2026-10-03에 probe branch `probe/ubuntu-26-runner-20261003`(`60b44e3`, 병합하지 않음)에서
`public-boundary.yml`과 `development-gates.yml` web job의 `runs-on`만 `ubuntu-26.04`로 바꾸고
image 확인 step을 더해 돌렸다. 확인 뒤 probe branch는 원격과 로컬에서 지웠다.

| workflow / job | run / job | runner image | 결과 |
| --- | --- | --- | --- |
| Public Boundary `public-boundary-ci-required` | `37101948685` / `111143061935` | Ubuntu `26.04.1 LTS`, ImageVersion `20260927.149.1`, `pwsh` `7.6.6` | success, Pester `90/90` |
| Development Gates `web` | `37101948686` / `111143061898` | 같은 image | success |
| Development Gates `dotnet`, `delivery`, `installer-policy` | 같은 run | `windows-latest` (바꾸지 않음) | failure. 원인은 workflow 파일 변경 자체다: `workflow-active-shell`(probe step의 `shell: bash`), orchestration `source-sha`, `policy-boundaries`. image와 무관하다 |

결정: `ubuntu-latest`를 유지한다. 2026-10-19 이동 뒤에도 web job과 Public Boundary job은 같은
toolchain으로 통과할 것으로 본다. workflow는 바꾸지 않았다. 이동 뒤 첫 `main` run이 실패하면
그때 `ubuntu-24.04` pin을 다시 판단한다.
