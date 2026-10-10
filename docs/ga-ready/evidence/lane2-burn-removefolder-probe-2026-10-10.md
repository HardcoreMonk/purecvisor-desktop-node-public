# Lane 2 dev probe: Burn lifecycle with the RemoveFolder fix (2026-10-10)

- 종류: Lane 2 dev probe(`docs/DEVELOPMENT_PROCEDURE.md` §10). campaign `webpayload-removefolder-20261010` Task 2. 승격 근거가 아니고 `current-evidence.json`에 쓰지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 목적: train `0.42.94` 정차 원인 BL-0017(ZIP 제품 Update가 먼저 만든 web 하위 디렉터리가 MSI 제거 뒤 남아 Burn `remove-absence`가 `product remains`로 실패)이 `RemoveFolder` 수정으로 사라지는지 같은 호스트·같은 사전 조건에서 확인한다.

## 사전 조건

- 설치본 `0.42.94-admin-smoke+59af872`(train 0.42.94 pair의 Burn runner 복원), `C:\Program Files\PureCVisor\DesktopNode\web` 아래에 `samples`, `vendor`, `vendor\coolicons`, `vendor\pretendard`, `vendor\pretendard\woff2`가 있는 상태(정차 때와 같은 조건).
- 보존 VM `pcv-guest-installed-04253-r1`과 `pcv-it-s2-source`는 Off 그대로. 이 probe는 VM을 만들지 않는다.

## 수정 MSI

- build commit `3d50f14b658c070b3c5df5cede2a3c3dbbb94916`(campaign Task 1 HEAD), `build_utc` `2026-10-10T14:03:30.5620492Z`, WiX `5.0.2+aa65968c`, MSI SHA-256 `51b5106d1ece8f96c34675b6bd47b25d3acfac5c99a768dcc7d1d4eedb11ab28`, payload 32개, `AllowUnsignedDev`/`LocalTest`. root `artifacts/removefolder-probe-20261010-04294`(git 밖).
- Windows Installer COM(읽기 전용) RemoveFile 표: 행 9, 제거 폴더 행 7(`DesktopNodeWebDir_samples`, `DesktopNodeWebDir_vendor`, `DesktopNodeWebDir_vendor_coolicons`, `DesktopNodeWebDir_vendor_pretendard`, `DesktopNodeWebDir_vendor_pretendard_woff2`, `DesktopNodeWebFolder`, `INSTALLFOLDER`).

## 실행

- 명령: `Invoke-PcvBurnBootstrapperLifecycle.ps1 -ArtifactRoot artifacts/burn-removefolder-probe-20261010 -TargetMsiPath <수정 MSI> -TargetVersion 0.42.94-admin-smoke -WixPath <wix.exe> -ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Execute`(`-PlanOnly` `PLANNED` 뒤).
- 시작 `2026-10-10T23:03:49+09:00`, 43초. host mutation: bundle install, repair, uninstall, target MSI 복원, service stop/start.

| 단계 | 결과 |
| --- | --- |
| exits | build 0, install 0, repair 0, remove 0, restore-target-msi 0 |
| install-state | Running/Automatic, `0.42.94-admin-smoke` |
| repair-state | Running/Automatic, `0.42.94-admin-smoke` |
| remove-absence | Absent=true (service·manifest·product root 모두 없음) |
| 복원 | attempted=true, status=PASS, manifest `0.42.94-admin-smoke`, service Running/Automatic |

## 끝 상태

- 설치본 `0.42.94-admin-smoke+3d50f14`(복원), ARP `{5120E936-8996-4B70-9CD4-9A03C766A63A}` `0.42.94` 1개, service Running/Automatic, Web 200, web 하위 디렉터리 5개는 복원 설치가 다시 만들었고, 보존 VM과 `pcv-it-s2-source`는 Off 그대로다.
- 정차 때(수정 전 MSI, 같은 사전 조건)는 `remove-absence product remains`였고 이번에는 `Absent=true`다. 차이는 `RemoveFolder` 행뿐이다.

## 경계

- operational current는 `0.42.93-admin-smoke` 그대로다. 설치본은 복원된 수정 build `0.42.94-admin-smoke+3d50f14`이며 다음 train의 baseline(package `0.42.94`)과 version 문자열이 같다.
- 이 문서는 손으로 썼다(`train-evidence` 템플릿 밖). summary는 `artifacts/burn-removefolder-probe-20261010/summary.json`(git 밖).
