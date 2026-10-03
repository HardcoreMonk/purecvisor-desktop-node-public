# Desktop Node 개발 작업 지시서 (2026-10-02)

이 문서는 2026-10-02 작업의 닫힌 지시서다. Task 1–5와 그 뒤의 commit, Full shard, PR #27 병합까지 끝났다. 승인 표의 문구는 이미 사용됐다. 새 버전은 이 파일에 없다.

## 기준 시각

2026-10-02. 아래 저장소 표는 PR #27 병합 뒤의 현행이다. 착수 당시 값은 그 다음 절에 둔다. 다시 열 때는 같은 경로를 읽고, 문서와 다르면 실측을 따른다. 이 현행화는 새 버전 작업을 열지 않는다.

## 저장소

| 항목 | 값 |
| --- | --- |
| 살아 있는 제품 | `D:\data\projects\codex-zone\purecvisor-desktop-node-public` |
| 원격 | `https://github.com/HardcoreMonk/purecvisor-desktop-node-public.git` |
| 이 워크스페이스 `purecvisor-desktop-node` | 별도 git 이력. 생성 evidence는 `0.42.74-admin-smoke` snapshot. 현재 버전으로 쓰지 않는다 |
| 원장 | `origin/main`의 `docs/ga-ready/current-evidence.json` = `0.42.86-admin-smoke`. `promotion_eligible=true`, blocker 없음. provenance `b807803f778e29c206f1bb2ba8277d2a1136198f`. 닫힌 pair `0.42.85-admin-smoke -> 0.42.86-admin-smoke` / `manual-admin-campaign-descriptor-20261002-04285-04286-consume` |
| 설치본 | 마지막 확인은 2026-10-02 fullgate와 current-card. `C:\Program Files\PureCVisor\DesktopNode`, operational `0.42.86-admin-smoke`, provenance `b807803`, ARP `{1994F4DF-A62D-497F-83FF-39FEB994FA8D}` |
| `origin/main` | `2057b403386134ac0abc8444a81c7ec532986daf` (PR #27 merge) |
| 이 파일 | 로컬 브랜치 `docs/development-work-order-20261002`. `origin/main`에는 없다 |

착수 당시(이 값으로 되돌리지 않는다): 원장 `0.42.84-admin-smoke`, 설치본 `0.42.85-admin-smoke`, `origin/main` `5b84a4e`(PR #24, `vm.list` readback).

명령은 public 저장소를 명시한다. 기본 cwd는 private 워크스페이스라 `dotnet test`와 `npm`이 다른 트리를 칠 수 있다.

## 이미 끝난 일

다시 하지 않는다.

- `0.42.85-admin-smoke` package, manual-admin pair `0.42.84 -> 0.42.85` 여섯 bucket, fullgate `full-admin-host-mutation-gate-20261001-04285`, installed current-card, Lane 2 readback probe. 기록은 PR #25에 있다. probe 결과는 `PARTIAL`이다. 새 readback은 PASS이고, 그 시점의 `vm.eject`와 빈 DVD `vm.attach`는 기존 결함으로 실패했다. `0.42.85`는 operational current가 아니다.
- 그 결함의 소스 수정은 `fb95de1` (`fix(hyperv): eject DVD media and attach to an empty drive`)이다. PR #26 merge commit `1c488b62bc68783f4673bb4f441f65d5de5c561c`. `fb95de1`은 `origin/main`의 조상이다. 로컬 Full shard 네 개는 `fb95de1` clean HEAD에서 `ok=true`였다.
- `0.42.86-admin-smoke` package, Lane 2 media probe, pair `0.42.85 -> 0.42.86`, fullgate, current-card, Lane 3 원장. 승격 커밋 `2b6ef2bbf1e5bb6b6f9e770028bab295a8d12808`. PR #27 merge commit `2057b403386134ac0abc8444a81c7ec532986daf`.

`0.42.85-admin-smoke`와 `0.42.86-admin-smoke`를 다시 빌드하지 않는다. 같은 버전으로 다른 트리를 포장하지 않는다.

## 소스 계약 (PR #26)

`DesktopNodeHyperVWmiVmMediaProvider`가 이렇게 동작한다.

- drive는 `Msvm_ResourceAllocationSettingData`의 `Microsoft:Hyper-V:Synthetic DVD Drive`다. media SASD를 drive로 세지 않는다.
- media가 있는 `vm.eject`는 그 SASD에 `RemoveResourceSettings`를 호출한다. 빈 `HostResource`로 `ModifyResourceSettings`를 호출하지 않는다.
- media가 없는 `vm.eject`는 WMI 호출 없이 성공한다.
- media가 있는 `vm.attach`는 그 SASD의 `HostResource`를 바꾼다.
- drive만 있는 `vm.attach`는 drive RASD 아래에 `Microsoft:Hyper-V:Virtual CD/DVD Disk`를 `AddResourceSettings`로 추가한다.
- `vm.dvd.add`는 drive RASD가 있으면 `PCV_VM_DEVICE_ALREADY_PRESENT`로 거절한다. media가 없어도 같다.
- Parent가 drive와 맞지 않는 DVD disk가 둘 이상이면 `PCV_HYPERV_WMI_RESOURCE_MISSING`이다.

## 캠페인

`origin/main`의 `docs/ga-ready/active-campaign.json`은 `package-pair-04285-20261001`, `status=open`, `next_task=null`이다. `completed_tasks`는 Task 1부터 Task 6까지다. `next_step`은 끝난 `0.42.85` pair를 설명하면서 PR merge, `0.42.85` Lane 3, eject 수정을 다음 승인으로 적는다. 그 세 가지는 이미 끝났다. eject 수정은 `fb95de1`이고, `0.42.86` 승격은 PR #27이다. PR #27은 campaign 파일을 고치지 않았다.

이 `next_step`으로 `0.42.85`를 다시 만들거나 다음 버전을 열지 않는다. 큐에 다음 제품 작업은 없다.

## 승인 표

사용자 메시지에 해당 문구가 있을 때만 연다. 문장 안의 다른 승인을 추정하지 않는다.

| 문구 | 허용 |
| --- | --- |
| `push/PR` | 현재 public 브랜치를 push하고 PR을 만들거나 갱신한다. merge는 포함하지 않는다 |
| `merge PR #26` | PR #26만 `main`에 병합한다 |
| `merge PR #25` | PR #25만 `main`에 병합한다 |
| `package 0.42.86` | `0.42.86-admin-smoke` 패키지를 로컬에 만든다. 설치하지 않는다 |
| `Lane 2 media probe` | 아래 Task 4의 probe VM만 만들고 지운다 |
| `package pair 0.42.85 -> 0.42.86` | 여섯 bucket pair. clean-host, Burn, MSIX를 포함한다 |
| `fullgate` | `0.42.86` full admin host mutation gate |
| `current-card` | 설치된 `0.42.86` current-card |
| `current-evidence` | Lane 3 원장 쓰기. feature qualification이 PASS일 때만 |

`알아서`, `계속`, `후속`은 이 표의 문구가 아니다. 그 말만 있으면 큐에 있는 미승인 작업을 열지 않고, 이미 승인된 task 하나만 진행한다.

이 표의 문구는 2026-10-02에 모두 사용되어 이 지시서 범위에서는 닫혔다. 표 밖에서 같은 날 사용된 문구는 `로컬 commit`, `Full shard`, `merge PR #27`이다. 그 문구로 이 작업을 다시 열지 않는다.

## 공통 금지

- public trusted signing, external stable publication, winget 공개 제출을 하지 않고 주장하지 않는다. 패키지 서명은 `AllowUnsignedDev` / `LocalTest`만 쓴다.
- TUI를 되살리지 않는다. 운영 표면은 Web Console과 PCVCLI다.
- Linux, KVM, libvirt 코드를 넣지 않는다. 제품 요청 경로에 PowerShell helper fallback을 넣지 않는다.
- 보존 VM을 끄거나 지우지 않는다. 2026-10-01 종료 시점의 보존 VM은 `pcv-guest-installed-04253-r1`(Off)였다. 착수 시 `Get-VM`으로 다시 확인하고, 이 지시서의 probe 접두사 `pcv-wo-media-` 가 아닌 VM은 유지한다.
- `pcv-spike-*` 가 아닌 VM을 fullgate 정리 대상으로 넣지 않는다.
- `docs/ga-ready/current-evidence.json`과 생성 evidence 블록은 `current-evidence` 문구 없이 고치지 않는다.
- 기존 evidence 파일을 덮어쓰지 않는다. 새 날짜 파일을 추가한다.
- 장기 token을 명령줄에 넣지 않는다. `--protected-token-file`을 쓴다.
- Web 제외 4개(noVNC target 저장 3개, `vm.limit`)를 열지 않는다.
- 새 `Add-Type`, P/Invoke, native ACL, installer handoff가 필요하면 멈추고 보고한다.

## Task 1: PR #26 병합

승인 문구: `merge PR #26`

- [x] CI 다섯 개가 아직 SUCCESS인지 확인한다. 하나라도 아니면 병합하지 않는다.
- [x] `main`에 병합한다. squash 여부는 그 PR에 이미 쓰인 저장소 방식을 따른다.
- [x] 병합 뒤 `origin/main`이 `fb95de1`의 변경을 포함하는지 `git log`로 확인한다.
- [x] current-evidence와 설치본은 그대로 둔다.

실행 기록(2026-10-02): merge commit `1c488b62bc68783f4673bb4f441f65d5de5c561c`. `fb95de1`은 `origin/main`의 조상이다.

완료 상태: `code_complete`가 `main`에 있다. operational current는 아니다.

## Task 2: PR #25 병합

승인 문구: `merge PR #25`

- [x] 이 PR은 evidence와 campaign 기록이다. `fb95de1`을 포함하지 않는다. 제품 결함이 고친 것으로 적지 않는다.
- [x] `main`과 충돌하면 evidence를 다시 쓰지 않고 충돌 파일 이름을 보고한 뒤 멈춘다.
- [x] 병합 뒤 `main`의 `active-campaign.json`이 `0.42.85` pair 완료를 가리키는지 확인한다. `0.42.85`를 다시 빌드할 이유로 읽지 않는다.

실행 기록(2026-10-02): PR #26 병합으로 브랜치가 behind가 되어 required check가 비었다. `main`을 브랜치에 병합한 커밋 `7ba7bf7`에서 CI 다섯 개가 SUCCESS가 된 뒤 merge commit `b807803`으로 병합했다. 충돌은 없었다. `active-campaign.json` id는 `package-pair-04285-20261001`이고 `next_task`는 null이다. `next_step` 문장은 여전히 eject 수정을 다음 승인으로 적고 있다. 그 수정은 이미 `main`의 `fb95de1`이다. campaign 파일은 고치지 않았다.

Task 1과 순서는 어느 쪽이 먼저여도 된다. Task 3은 Task 1이 `main`에 반영된 뒤에만 한다.

## Task 3: `0.42.86-admin-smoke` 패키지

승인 문구: `package 0.42.86`

전제: 패키지 HEAD가 `fb95de1`의 미디어 변경을 포함한다. `main`에 없으면 그 커밋을 패키지하지 않는다.

- [x] `packaging/windows-desktop-node/installer/build.ps1`로 `0.42.86-admin-smoke`를 만든다. `-SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`.
- [x] 출력은 `artifacts/admin-smoke-package-20261002-04286` 아래만 쓴다.
- [x] evidence `docs/ga-ready/evidence/admin-smoke-package-2026-10-02-04286.md`를 새로 만든다. MSI SHA-256, payload SHA-256, provenance commit, `installed=not-run`, `pair=not-closed`를 적는다.
- [x] 설치, 서비스 재시작, Hyper-V 변경은 하지 않는다.

실행 기록(2026-10-02): provenance `1c488b6`. MSI SHA-256 `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6`. payload SHA-256 `afa5cb95c4268f7118c52c554e3061b738c01cd3d4a3b4c75af04d094ee61484`. 패키지 당시 evidence commit은 로컬 브랜치 `package/04286-admin-smoke-20261002`의 `0d2b79b`였다. 그 브랜치는 push하지 않았다. 같은 evidence 파일은 이후 `2b6ef2b`에 들어가 PR #27로 `origin/main`에 반영됐다.

완료 상태: `package_candidate`.

## Task 4: 설치본 미디어 probe

승인 문구: `Lane 2 media probe`

전제: Task 3 패키지가 이 호스트에 설치되어 있다. 설치 승인은 이 task 문구에 포함되지 않는다. 설치본이 `0.42.86-admin-smoke`이고 Host/CLI가 그 패키지와 같을 때만 probe를 한다. 설치본이 `0.42.85`이면 멈추고 그 사실을 보고한다.

- [x] probe VM 이름은 `pcv-wo-media-`로 시작한다. Generation 2, ISO `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`, CPU 2, memory 2048 MB, disk 20 GB. VM은 켜지 않는다.
- [x] 생성 뒤 `vm list`의 `dvd_media`가 서버 ISO인지 확인한다.
- [x] `vm eject`가 성공하고 `dvd_media`가 비는지 확인한다. drive 수는 1로 남는다.
- [x] 빈 drive에 다른 존재하는 ISO로 `vm attach`가 성공하고 `dvd_media`가 그 ISO인지 확인한다.
- [x] `vm delete --yes`로 probe VM을 지운다. 제품이 남긴 VHD와 VM 디렉터리는 그 VM만 지운다.
- [x] evidence `docs/ga-ready/evidence/lane2-vm-media-eject-attach-2026-10-02-04286.md`를 새로 만든다. 상태는 통과해도 `installed_non_promoted_candidate`다.

실행 기록(2026-10-02): probe VM `pcv-wo-media-1002`에서 생성, eject, 빈 drive attach가 모두 `PASS`다. VM과 `D:\PureCVisor\VMs\pcv-wo-media-1002`는 지웠다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.

실패하면 첫 `PCV_*`를 적고 같은 원인으로 세 번 이상 반복하지 않는다. FAIL을 원장에 쓰지 않는다.

## Task 5: pair, fullgate, current-card, 원장

각 문구가 따로 있을 때만 연다. 한 문구가 다음 문구를 열지 않는다.

- [x] `package pair 0.42.85 -> 0.42.86`: readiness, ops summary, installed update/rollback, clean-host, Burn, MSIX, descriptor. 하나라도 실패하면 나머지를 건너뛰고 설치본을 직전 성공 버전으로 되돌릴 수 있는지부터 보고한다.

실행 기록(2026-10-02): 여섯 runner와 descriptor가 `PASS`다. descriptor `manual-admin-campaign-descriptor-20261002-04285-04286`, `missing_count=0`, `not_pass_count=0`. readiness 전에 `REMOVE_DATA` 없이 `0.42.86`을 제거하고 clean `0.42.85` MSI를 다시 설치했다. Burn 뒤에는 설치본이 `0.42.86-admin-smoke`다.
- [x] `fullgate`: batch id `full-admin-host-mutation-gate-20261002-04286`.

실행 기록(2026-10-02): supervisor exit `0`, 두 step 모두 exit `0`. evidence `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-10-02-04286-hostmutation.md`. 시작 전에 `REMOVE_DATA` 없이 clean `0.42.86`을 제거했다. 끝난 설치본은 operational MSI `85387f31…`, provenance `b807803`다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.
- [x] `current-card`: 설치본이 `0.42.86-admin-smoke`인 상태에서 CLI와 Web을 기록한다. `promotion_ledger_status`는 원장 쓰기 전에 `not-promoted`다.

실행 기록(2026-10-02): 캡처 `status=pass`, summary SHA-256 `4f5d802cfa5793331259d9cd7780d2419084297915fc841560cf01fb61aa43b2`. evidence `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-10-02-04286.md`. 캡처 당시 canonical current는 `0.42.84-admin-smoke`이고 캡처 artifact summary는 `not-promoted`다. Lane 3 이후 evidence 헤더는 `promoted-current`, canonical `0.42.86-admin-smoke`다. 캡처 artifact summary는 `not-promoted`로 남겼다.
- [x] `current-evidence`: qualification이 PASS일 때만 `0.42.86-admin-smoke`를 원장에 쓴다. `0.42.85`를 `fb95de1`이 들어 있는 버전으로 적지 않는다.

실행 기록(2026-10-02): qualification `promotion_eligible=true`, blockers 없음. `Invoke-PcvLane3PromotionDocs.ps1` dry-run은 생성 블록 `stale`, 나머지 `planned`였다. `-Apply`와 `-Check`는 6단계 모두 통과했다. 도구는 `CurrentEvidenceVerifierTests`의 기대 버전을 바꾸지 않았다. 그 테스트가 `0.42.84-admin-smoke`를 기대해 실패했고, 사용자 문구 `에러 해결` 뒤에 그 한 줄을 `0.42.86-admin-smoke`로 맞췄다. `CurrentEvidenceVerifierTests` 13/13, `DesktopNode.Delivery.Tests` 744/744이 통과했다. 로컬 commit `2b6ef2bbf1e5bb6b6f9e770028bab295a8d12808`은 `lane3/04286-promotion-20261002`에 있고 PR #27로 `origin/main`에 병합됐다.

Lane 3 전에는 `Invoke-PcvLane3PromotionDocs.ps1`를 dry-run으로 보고, 그 다음 `-Apply`, 끝에 `-Check`를 한다. 위 한 줄은 도구가 기대 버전을 남긴 뒤의 수정이다.

## 지시서 뒤 닫힌 실행

Task 5 원장 쓰기 뒤에 같은 날 닫았다. 다시 실행하지 않는다.

- `로컬 commit`: `2b6ef2bbf1e5bb6b6f9e770028bab295a8d12808`, 제목 `docs: promote 0.42.86-admin-smoke to operational current`. 33 files. docs 체크아웃 `docs/development-work-order-20261002`는 커밋하지 않았다.
- `Full shard`: clean HEAD `2b6ef2b`의 로컬 clone `D:\data\projects\codex-zone\pcv-04286-fullshard`에서 네 shard 모두 `ok=true`, `plan_only=false`. dotnet 42.5초, web 24.6초, delivery 2.7초, installer-policy 1.8초. Temp 아래 worktree에서는 `.git`이 파일이고 경로가 `os.tmpdir()` 안이라 Hyper-V 테스트 2개와 web 테스트 1개가 환경 때문에 실패했다. 그 실행은 PASS 근거가 아니다.
- `push/PR`: PR #27, base `main`, head `lane3/04286-promotion-20261002`. https://github.com/HardcoreMonk/purecvisor-desktop-node-public/pull/27
- `merge PR #27`: merge commit `2057b403386134ac0abc8444a81c7ec532986daf`, 2026-10-02T10:41:24Z. squash가 아니다. 병합 전 필수 검사 다섯 개와 CodeRabbit이 SUCCESS였다.
- 병합 뒤 `main` push CI: Development Gates `36996858669`(`dotnet`, `web`, `delivery`, `installer-policy`)와 Public Boundary `36996858460`(`public-boundary-ci-required`)이 success다. head `2057b40`.

도달한 상태: `promotion_complete`. `current_evidence_written=true`는 PR #27에 들어 있다. 이 현행화는 원장을 다시 쓰지 않는다. host mutation은 이 절의 실행에 없다.

## 검증

Lane 1에서 미디어 코드를 다시 만지면 먼저 다음을 돌린다.

```powershell
dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --filter FullyQualifiedName~DesktopNodeHyperVDvdMediaPlanTests
dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj
```

커밋 승인 뒤 clean HEAD에서 Full shard 네 개를 돌린다. `--plan-only` 출력은 PASS가 아니다.

```powershell
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/local-dotnet --shard dotnet
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/local-web --shard web
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/local-delivery --shard delivery
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/local-installer-policy --shard installer-policy
```

실행 전 `dotnet restore src/DesktopNode.sln`과 `dotnet build src/DesktopNode.sln -c Release --no-restore`를 한다. 위 명령은 public 저장소에서 실행한다.

## 착수 계약

작업을 열 때마다 사용자에게 다음을 먼저 적는다.

- checkpoint 번호와 task id
- 허용 파일
- 검증 명령
- Lane 1이면 30분, tool batch 18회
- 범위 밖 발견은 `report-only`

## 마칠 때 적을 것

lane, public HEAD, 원장 버전, 설치본 버전, 실행한 검증, 실행하지 않은 검증, host mutation 여부, `current_evidence_written`, 도달한 상태(`code_complete`부터 `promotion_complete` 사이), 다음에 필요한 승인 문구.

## Nonclaims

- `0.42.85-admin-smoke`는 pair baseline이다. operational current로 올리지 않았고, `fb95de1`을 포함하지 않는다.
- operational current는 `origin/main`의 `0.42.86-admin-smoke`다. 착수 당시 원장 `0.42.84-admin-smoke`로 되돌리지 않는다.
- PR #26의 CI PASS만으로 설치본 eject/attach PASS를 주장하지 않는다. 설치본 PASS는 Lane 2 probe 기록이다. probe 해시는 clean package와 같고, fullgate operational 해시와는 다르다.
- public trusted signing과 external stable publication을 주장하지 않는다.
- `active-campaign.json`의 `next_step`은 다음 제품 작업의 승인이 아니다.
