# WebPayload RemoveFolder 수정 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.42.94` 정차 원인 BL-0017을 고친다. 생성되는 `installer/WebPayload.wxs`가 web 하위 디렉터리마다 `RemoveFolder`(On=uninstall)를 내고 `web` 폴더도 제거 대상이 되게 해, ZIP 제품 Update가 먼저 만든 디렉터리가 있어도 MSI 제거 뒤 제품 root가 남지 않게 한다. Burn lifecycle dev probe로 설치·수리·제거·부재·복원을 확인한 뒤 PR을 merge하고, 2026-10-10 승인 2로 train `0.42.95` campaign을 연다.

**Architecture:** 정차 기록은 `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-train-04294.md` Task 2와 backlog BL-0017·BL-0018. 생성기 `packaging/windows-desktop-node/tools/Update-PcvWebPayloadWix.ps1`(ADR-0018)이 WebPayload.wxs를 만들고 `WixSourceContractVerifier`(Delivery)와 installer Pester가 구조를 pin한다. Windows Installer는 자신이 만든 빈 폴더만 지우므로 디렉터리마다 `RemoveFolder` 행이 있어야 한다(RemoveFile 표). branch `lane1/webpayload-removefolder-20261010`.

**Tech Stack:** PowerShell 7 생성기, WiX 5(`wix.exe`), C# Delivery 계약, Pester 5, `build.ps1`, `Invoke-PcvBurnBootstrapperLifecycle.ps1`, Windows Installer COM(읽기 전용 표 조회)

## 사용자 결정 (2026-10-10)

승인 원문: `1,2,3` (train `0.42.94` 정차 보고의 다음 승인 1~3에 대한 답).

| 항목 | 범위 |
| --- | --- |
| 1 | `Lane 1 수정 campaign: Update-PcvWebPayloadWix.ps1이 생성 디렉터리마다 RemoveFolder component를 내고 web 폴더도 제거 대상에 포함, WixSourceContractVerifier·Delivery·installer Pester의 component 수 pin과 WiX smoke(설치·제거 뒤 root 부재) 갱신, queue 행 추가, Task 11 이관, push/PR, green CI 뒤 merge(PR #83 merge 포함).` 해석: Lane 0/1 + Lane 2 dev probe 하나. mutation scope: Burn lifecycle dev probe(수정 MSI bundle install, repair, uninstall, target MSI 복원, service stop/start). 승격 근거 아님. `current-evidence.json` 쓰기 없음. push, PR, green CI 뒤 merge |
| 2 | `수정 merge 뒤 train 0.42.95-admin-smoke 출발: queue 78·79·81 + 수정 PR 행 고정, package build, pair host mutation(ADR-0016 범위), fullgate(os-mutation-gate 포함)·current-card, Lane 2 probe vm.create·checkpoint.schedule.set·web.console.shell(S3 재시연), Lane 3 current-evidence 쓰기, 단일 PR merge, main push red면 revert PR. baseline은 설치본 0.42.94 package.` 이 campaign이 닫히면 Task 3이 `pcv-campaign-open`으로 train campaign을 연다 |
| 3 | `train 뒤 S4 campaign, 그 뒤 legacy-retirement campaign(2026-10-10 승인 2·3 그대로).` train campaign 뒤에 연다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정, disk는 artifacts 아래(이 campaign은 VM을 만들지 않는다) |
| 이관 | `train-04294-20261010` Task 11(C5 runner 확인, `not_before` 2026-10-19)을 Task 4로 옮긴다 |

## Global Constraints

- 설치본은 `0.42.94-admin-smoke+59af872`(train 0.42.94 pair의 Burn runner 복원)다. Task 2 dev probe 뒤에는 수정 build의 `0.42.94-admin-smoke+<commit>`가 남는다(ProductVersion 문자열은 같고 commit만 다름). 보존 VM과 `pcv-it-s2-source`는 건드리지 않는다.
- `src/`·`web/src/`·`config/`는 바꾸지 않는다(installer 생성기·wxs·Delivery 시험·Pester만). criteria·backlog 분류는 바꾸지 않는다.
- token, credential은 명령줄·summary·evidence에 남기지 않는다. dev probe는 guest VM이 없다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: 생성기와 계약

- [x] `Update-PcvWebPayloadWix.ps1`: 디렉터리마다 그 디렉터리의 첫 file component 안에 `<RemoveFolder Id="DesktopNodeWebDirRemove_<...>" On="uninstall" />`를 내고(디렉터리 전용 component·registry keypath 없이), `Product.wxs`의 `web` index component와 INSTALLFOLDER host component에 `RemoveFolder`를 더한다. `-Apply`로 재생성. `WixSourceContractVerifier`에 "fragment의 모든 Directory에 RemoveFolder 1개"와 Product.wxs의 두 RemoveFolder를 pin하고 Delivery 시험(정상·negative)과 installer Pester WixSource/Plan 시험을 맞춘다. WiX compile smoke(scratch `wix-smoke.ps1`)로 MSI를 만들고 Windows Installer COM(읽기 전용)으로 RemoveFile 표에 디렉터리 6개 행이 있는지 확인한다. 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, installer Pester, packaging Pester, `git diff --check`. 로컬 commit.

실행 기록(2026-10-10): `Update-PcvWebPayloadWix.ps1`이 디렉터리마다 그 디렉터리의 첫 file component 안에 `<RemoveFolder Id="DesktopNodeWebDirRemove_<dir>" On="uninstall" />`를 내고, file component가 없는 디렉터리는 `PCV_WEB_PAYLOAD_MANIFEST_INVALID`로 거절한다(SYNOPSIS에 BL-0017 근거). `-Apply`로 재생성한 `WebPayload.wxs`는 RemoveFolder 5개(samples, vendor, vendor/coolicons, vendor/pretendard, vendor/pretendard/woff2). `Product.wxs`는 `DesktopNodeServiceHostComponent`에 `RemoveInstallFolder`, `DesktopNodeWebIndexComponent`에 `RemoveWebFolder`를 더했다. `WixSourceContractVerifier`가 fragment의 모든 Directory에 RemoveFolder 정확히 1개(`web-payload:remove-folder:<id>`), `On=uninstall`(`web-payload:remove-folder-on`), Product.wxs의 두 RemoveFolder(`product:remove-folder:<id>`)를 pin하고 negative parity 2건(`WixSourceVerifierRejectsWebPayloadDirectoryWithoutRemoveFolder`, `WixSourceVerifierRejectsProductWithoutWebFolderRemoval`)을 더했다. installer Pester It 'installs only product-owned Desktop Node MSI payload assets'에 RemoveFolder 2개와 fragment의 RemoveFolder 수 == Directory 수 검사를 더했다(It 이름·legacy contract id 불변). WiX compile smoke exit 0, MSI RemoveFile 표(Windows Installer COM 읽기 전용): 행 9, 제거 폴더 행 7(web 디렉터리 5 + DesktopNodeWebFolder + INSTALLFOLDER). 검증: Delivery Release 781 PASS(+2), installer Pester 49, packaging Pester 528, `git diff --check`. host mutation 없음.

## Task 2: Burn lifecycle dev probe (Lane 2)

- [x] Task 1 HEAD에서 `build.ps1 -Version 0.42.94-admin-smoke -MsiProductVersion 0.42.94 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/removefolder-probe-20261010-04294`로 수정 MSI를 만들고, 현재 설치본(web 하위 디렉터리가 이미 있는 상태) 위에서 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -TargetMsiPath <수정 MSI> -TargetVersion 0.42.94-admin-smoke -WixPath D:\Users\yohan\.dotnet\tools\wix.exe -Execute`를 artifact root `artifacts/burn-removefolder-probe-20261010`에 돌린다. 기대: install/repair/remove exit 0, `remove-absence` `Absent=true`, 복원 PASS, 끝 상태 service Running/Automatic, ARP 1개, Web 200. FAIL이면 멈춘다. evidence `docs/ga-ready/evidence/lane2-burn-removefolder-probe-2026-10-10.md`(손으로 씀; 승격 근거 아님). 로컬 commit.

실행 기록(2026-10-10): Task 1 HEAD `3d50f14`에서 `build.ps1`로 수정 MSI를 만들었다(39초, MSI SHA-256 `51b5106d1ece8f96c34675b6bd47b25d3acfac5c99a768dcc7d1d4eedb11ab28`, payload 32, RemoveFile 표 제거 폴더 행 7). 사전 조건은 정차 때와 같다(설치본 `0.42.94-admin-smoke+59af872`, web 하위 디렉터리 5개 존재). runner `-PlanOnly` `PLANNED`(host mutation 없음) 뒤 `-Execute` `2026-10-10T23:03:49+09:00`부터 43초: build/install/repair/remove/restore-target-msi exit 모두 0, install-state·repair-state Running/Automatic `0.42.94-admin-smoke`, `remove-absence` `Absent=true`(service·manifest·product root 없음), 복원 PASS. 끝 상태: 설치본 `0.42.94-admin-smoke+3d50f14`, ARP `{5120E936-8996-4B70-9CD4-9A03C766A63A}` 1개, service Running/Automatic, Web 200, 보존 VM과 `pcv-it-s2-source` Off. evidence `docs/ga-ready/evidence/lane2-burn-removefolder-probe-2026-10-10.md`(손으로 씀, 승격 근거 아님). summary는 `artifacts/burn-removefolder-probe-20261010/summary.json`. operational current는 `0.42.93-admin-smoke` 그대로다.

## Task 3: 종료 검증, queue 행, push, PR, merge, train campaign 열기

- [ ] clean HEAD에서 PR gate(`Invoke-PcvPrGate.ps1 -SkipBuild` 뒤 Release build), Pester 4종, `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check origin/main...HEAD`. push, PR, 그 PR 번호로 `release-train.json` `queue` 행(area installer, risk M, lane2_probe `burn.lifecycle`)을 같은 PR에 더한 뒤 green CI 뒤 merge, main push run green 확인. campaign을 닫고(Task 4 이관) 승인 2로 train `0.42.95` campaign을 `pcv-campaign-open`으로 연다.

## Task 4: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `train-04294-20261010` Task 11(원래 `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 바뀌지 않는다(`0.42.93-admin-smoke`). dev probe는 승격 근거가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
