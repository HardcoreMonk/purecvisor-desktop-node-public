# PureCVisor Desktop Node Phase 14 Signed Installer와 Repair/Uninstall UX 설계

## 목적

Phase 14는 Phase 13에서 검증한 WinSW service wrapper를 사용자가 설치 가능한 Windows installer 산출물로 감싸는 단계다.

Phase 12/13의 `packaging/windows-desktop-node/` wrapper는 제품 루트, 데이터 루트, WinSW service lifecycle, diagnostics, rollback, `Uninstall -RemoveData` smoke를 검증했다. 하지만 아직 운영자가 직접 PowerShell entrypoint를 실행해야 하며, signed installer, repair UX, uninstall/remove-data UX, WinSW binary provenance, signing chain이 제품 계약으로 고정되어 있지 않다.

Phase 14는 Desktop Node 전체를 제품 런타임으로 GA 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지하고, `packaging/windows-desktop-node/**` 아래 제품 후보 배포 계층에 installer 산출물과 검증 경계를 추가한다.

## 구현 결과 메모

Phase 14 구현 결과 WiX MSI source, build script, provenance schema, MSI 전용 product wrapper action, unsigned dev MSI build, 관리자 install/uninstall smoke가 완료됐다. 2026-05-01 .NET Host replacement 이후 MSI installed custom action은 product wrapper PowerShell entrypoint가 아니라 설치된 `DesktopNode.Host.exe service-action` runner를 호출한다.

구현 중 확인한 경계:

- Deferred custom action은 설치된 `INSTALLFOLDER`의 `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed`를 실행한다.
- MSI payload에는 runtime에 필요한 API/Web/Hyper-V/service 자산과 `DesktopNode.Host.exe`가 포함된다.
- .NET service-action runner가 설치 루트 기준으로 protected token file과 SCM service configuration을 준비한다.
- Windows PowerShell 5.1 custom action fallback은 기본 MSI installed path가 아니며, PowerShell은 Hyper-V/helper adapter와 운영 runbook 경계로 남는다.
- uninstall 중 SCM stopped/missing status wait로 service delete 경합을 처리한다.

Phase 14 당시 남은 조건부 gate는 repair smoke, `REMOVE_DATA=1` smoke, signed release build였다. 현재는 2026-04-30 local test certificate 기준 signed RC MSI lifecycle과 elevated install/repair/uninstall/`REMOVE_DATA=1` smoke를 기록했다. Burn/MSIX/winget, full updater, Windows Credential Manager, public trusted/stable signing, 내장 LAN TLS 정책은 계속 후속 판단으로 둔다.

## 결정

```text
DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first
```

Phase 14는 WiX Toolset 기반 MSI를 1차 installer 산출물로 채택한다.

직접 효과:

- MSI가 `%ProgramFiles%\PureCVisor\DesktopNode` 파일 설치와 제거를 소유한다.
- `DesktopNode.Host.exe service-action` runner는 installed service/data configuration을 소유하고, product wrapper는 standalone 관리자 smoke와 diagnostics/update/rollback 운영 경계를 유지한다.
- Windows Installer repair/uninstall 경로는 Phase 13 service lifecycle을 우회하지 않는다.
- Code signing 인증서와 private key는 repo에 두지 않고 외부 입력으로만 받는다.
- Service host executable은 MSI payload/provenance 검증 대상으로 둔다. Phase 13 WinSW binary provenance는 이력/compatibility 경계로 보존한다.
- Burn bootstrapper, winget manifest, MSIX, full updater, DPAPI, Windows Event Log provider, LAN TLS는 후속 Phase로 유지한다.

## 사용자 승인 범위

2026-04-27 대화에서 다음 방향을 승인했다.

- Phase 14는 signed installer와 repair/uninstall UX를 다룬다.
- 세 선택지 중 WiX MSI 우선 접근을 채택한다.
- 실제 code signing 인증서는 외부 입력 계약으로만 설계하고 repo에는 포함하지 않는다.

## 외부 근거

- WiX Burn 문서는 bundle이 `MsiPackage`, `ExePackage`, `MspPackage`, `MsuPackage` 같은 package chain을 담을 수 있음을 설명한다. Phase 14는 prerequisite chain이 아직 필요하지 않으므로 Burn은 후속으로 보류한다.
- WiX signing 문서는 MSI package signing과 bundle signing을 분리한다. 특히 Burn bundle은 engine signing과 전체 bundle signing이 모두 필요하다. Phase 14가 MSI-first를 택하는 이유는 이 signing surface를 한 단계 작게 유지하기 위해서다.

참고 링크:

- WiX Burn bundles: `https://docs.firegiant.com/wix/tools/burn/`
- WiX signing packages and bundles: `https://docs.firegiant.com/wix/tools/signing/`

## 대안 비교

| 대안 | 장점 | 단점 | 판정 |
|------|------|------|------|
| WiX MSI first | Windows service와 per-machine install/uninstall/repair 모델에 맞다. 파일 소유권을 Windows Installer database에 남길 수 있다. Burn보다 signing surface가 작다. | Deferred custom action 설계가 필요하고, wrapper와 MSI의 책임 경계를 엄격히 나눠야 한다. | 채택 |
| PowerShell installer CLI first | 빠르게 UX를 정리할 수 있고 기존 product wrapper를 그대로 확장한다. | 사용자가 설치 가능한 signed installer 산출물이라는 Phase 14 목표를 만족하지 못한다. Windows Installer repair/uninstall UX와도 다르다. | 제외 |
| MSIX/winget first | 배포 UX가 좋고 설치/업데이트 생태계와 맞는다. | Windows service, Hyper-V/admin 권한, ProgramData 데이터 보존, WinSW service lifecycle과 충돌할 수 있다. | 후속 후보 |
| Burn bundle first | prerequisite install, bootstrapper UI, chained packages를 한 번에 다룰 수 있다. | 현재 prerequisite chain이 확정되지 않았고, bundle signing은 engine과 bundle을 모두 다뤄야 한다. | MSI 안정화 후 후속 |

## 책임 경계

Phase 14는 MSI와 product wrapper 책임을 분리한다.

| 책임 | 소유자 |
|------|--------|
| 제품 파일을 Program Files에 설치/복구/제거 | MSI |
| `DesktopNode.Host.exe service-action` 기반 service configure/start/stop/remove, health check | .NET Host action runner + product wrapper health contract |
| ProgramData token/job/event/install/diagnostic 경로 준비 | product wrapper |
| 기본 uninstall의 ProgramData 보존 | product wrapper + MSI property |
| `RemoveData` destructive 삭제 | product wrapper + MSI property |
| MSI product/version/upgrade code | installer project |
| signing, hash, provenance manifest | installer build script |

기존 `Install`/`Uninstall` product action은 standalone 관리자 smoke와 개발자 CLI용으로 유지한다. MSI custom action은 파일 설치를 다시 하지 않도록 MSI 전용 configuration action을 호출한다.

## Installer 파일 구조

Phase 14는 다음 경로를 추가한다.

```text
packaging/windows-desktop-node/installer/
  README.md
  PureCVisorDesktopNode.wixproj
  Product.wxs
  ProductActions.wxs
  build.ps1
  installer-provenance.schema.json
  tests/
    PcvDesktopNodeInstaller.Plan.Tests.ps1
    PcvDesktopNodeInstaller.WixSource.Tests.ps1
    PcvDesktopNodeInstaller.Signing.Tests.ps1
```

역할:

- `Product.wxs`: MSI product identity, install directory, components, upgrade code, properties.
- `ProductActions.wxs`: deferred custom actions, install/repair/uninstall sequencing, `REMOVE_DATA` property mapping.
- `PureCVisorDesktopNode.wixproj`: WiX build와 signing target wiring.
- `build.ps1`: payload staging, WiX CLI detection, MSI build, optional signing, provenance output.
- `installer-provenance.schema.json`: provenance artifact의 최소 schema.
- `tests/`: 관리자 권한 없이 WiX source, command builder, signing policy, provenance contract를 검증한다.

## MSI product 계약

기본값:

- Product name: `PureCVisor Desktop Node`
- Manufacturer: `PureCVisor`
- Install scope: per-machine
- Default install dir: `%ProgramFiles%\PureCVisor\DesktopNode`
- Data root: `%ProgramData%\PureCVisor\desktop-node`
- Service name: `PureCVisorDesktopNode`
- Product version source: installer build의 명시적 `-Version` 입력. Product wrapper의 현재 manifest default는 `0.12.0`이지만, Phase 14 installer build는 암묵적 default에 의존하지 않는다.
- UpgradeCode: 한 번 생성해 source에 고정한다.
- ProductCode: major upgrade마다 새로 생성되도록 WiX default pattern을 사용한다.

MSI는 다음 파일을 설치한다.

- `api/**`
- `web/**`
- `hyperv/**`
- `service/**`
- `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- staged WinSW executable
- generated WinSW XML 또는 XML generator 입력
- product manifest와 installer provenance snapshot

MSI는 raw API token 값을 property, registry, MSI log, command line에 남기지 않는다.

## Product wrapper action 변경

MSI가 파일 설치를 소유하므로 product wrapper에 MSI 전용 action을 추가한다.

| Action | 용도 | 파일 설치/삭제 | 데이터 정책 |
|--------|------|----------------|-------------|
| `ConfigureInstalled` | MSI install 이후 service/data 구성 | 파일 복사 없음 | token/job/event 유지, token 없으면 생성 |
| `RepairInstalled` | MSI repair 이후 service/data 재구성 | 파일 복사 없음 | token/job/event 유지 |
| `RemoveInstalled` | MSI uninstall 이전 service 제거 | 제품 루트 삭제 없음 | 기본 보존, `-RemoveData`면 destructive 삭제 |

기존 action 유지:

- `Install`: standalone product wrapper smoke용. 자산 복사와 service start까지 수행한다.
- `Uninstall`: standalone product wrapper smoke용. product root 제거까지 수행한다.
- `Rollback`: Phase 13 wrapper rollback smoke용. MSI transactional rollback과는 별개다.
- `CollectDiagnostics`, `Status`, `Plan`: 기존 의미를 유지한다.

`ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled`는 모두 JSON 결과를 출력하고, MSI custom action은 실패 시 MSI를 실패 처리한다.

## Custom action 계약

MSI custom action은 PowerShell product wrapper를 호출한다.

원칙:

- deferred custom action으로 실행한다.
- per-machine elevation context에서 실행한다.
- user impersonation에 의존하지 않는다.
- custom action data에는 raw token 값을 넣지 않는다.
- `ProductRoot`, `DataRoot`, `WinSwPath`, `RemoveData`, `LogPath`만 전달한다.
- PowerShell stdout/stderr와 JSON result는 `%ProgramData%\PureCVisor\desktop-node\install.jsonl`과 MSI log에 redaction된 형태로 남긴다.

Sequencing:

| MSI operation | Custom action |
|---------------|---------------|
| Install after files installed | `ConfigureInstalled` |
| Repair after files repaired | `RepairInstalled` |
| Uninstall before files removed | `RemoveInstalled` |
| Uninstall with `REMOVE_DATA=1` | `RemoveInstalled -RemoveData` |

MSI rollback은 Windows Installer가 파일 상태를 되돌리는 범위까지만 담당한다. Service/data rollback과 versioned config migration은 Phase 18에서 다룬다.

## Repair UX

Repair는 다음을 보장한다.

- Program Files payload를 MSI database 기준으로 복구한다.
- ProgramData token file을 덮어쓰지 않는다.
- job store와 event log를 삭제하지 않는다.
- WinSW XML을 현재 install dir/data root 기준으로 재생성한다.
- service가 없으면 다시 설치한다.
- service가 있으면 stop/status wait 후 service config를 갱신하고 start한다.
- health check는 token-file bearer runtime policy로 확인한다.

지원 진입점:

```powershell
msiexec /i PureCVisorDesktopNode.msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log
```

후속 installer UI가 추가되기 전까지 repair는 Windows Installer 표준 repair UX와 CLI를 기준으로 한다.

## Uninstall UX와 데이터 보존

기본 uninstall은 ProgramData를 보존한다.

```powershell
msiexec /x PureCVisorDesktopNode.msi /l*v uninstall.log
```

기본 uninstall 결과:

- service stop/uninstall
- Program Files product root 제거
- `%ProgramData%\PureCVisor\desktop-node` 유지
- token, job store, event log, diagnostics, service logs 유지

RemoveData uninstall:

```powershell
msiexec /x PureCVisorDesktopNode.msi REMOVE_DATA=1 /l*v uninstall-remove-data.log
```

RemoveData 결과:

- service stop/uninstall
- Program Files product root 제거
- token file ACL repair 후 token file 제거
- job store, event log, install log, diagnostics 제거
- service logs는 Phase 14에서 기본 보존한다. 로그 retention과 Event Log 전환은 Phase 16에서 결정한다.

`REMOVE_DATA=1`은 destructive property다. README와 installer plan output에 삭제 대상 절대 경로를 명시한다.

## Signing 정책

Release build는 signed artifact를 요구한다.

Signing 대상:

- MSI package
- external cabinet을 사용할 경우 cabinet과 inscribed MSI
- installed PowerShell entrypoint/module 파일
- PureCVisor가 직접 배포하는 executable
- WinSW executable은 다음 중 하나를 만족해야 한다.
  - upstream trusted signature가 있고 pinned SHA-256이 일치한다.
  - PureCVisor redistribution policy가 허용하는 경우 PureCVisor-signed copy를 사용한다.

Build script 입력:

- `-Version`
- `-WinSwPath`
- `-OutputRoot`
- `-SigningMode RequireSigned|AllowUnsignedDev`
- `-SigningTrustModel LocalTest|InternalEnterprise|PublicTrusted`
- `-SignToolPath`
- `-CertificateThumbprint` 또는 `-CertificatePath`
- `-TimestampUrl`

기본값:

- CI/release path는 `RequireSigned`.
- 로컬 개발 smoke는 명시적 `-SigningMode AllowUnsignedDev`가 있을 때만 unsigned MSI를 허용한다.
- Signing certificate secret, PFX password, token 값은 repo와 provenance artifact에 기록하지 않는다.

## Provenance manifest

Installer build는 MSI 옆에 `purecvisor-desktop-node-installer-provenance.json`을 생성한다.

필수 필드:

- schema version
- product name/version
- git commit SHA
- build timestamp UTC
- WiX CLI version
- MSI path and SHA-256
- MSI signed 여부
- payload root
- payload file count
- payload aggregate SHA-256
- product wrapper module SHA-256
- WinSW source path, release label, SHA-256
- WinSW signature status
- signing mode
- build host OS summary

Provenance는 reproducible build를 완전히 보장하지 않는다. Phase 14의 목표는 어떤 입력으로 installer가 만들어졌는지 operator가 추적할 수 있게 하는 것이다.

## Build UX

개발자 build:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.14.0-dev `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode AllowUnsignedDev
```

Release build:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.14.0 `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode RequireSigned `
  -SigningTrustModel InternalEnterprise `
  -SignToolPath '<signtool.exe>' `
  -CertificateThumbprint '<thumbprint>' `
  -TimestampUrl '<timestamp-url>'
```

Build script는 WiX CLI가 없으면 구조화된 오류를 반환한다. Phase 14 구현은 WiX download/install을 자동 수행하지 않는다.

## 보안 경계

- 기본 listener는 계속 loopback only다.
- MSI property와 custom action data에 raw token을 넣지 않는다.
- ProgramData token file은 기존 ACL hardening 정책을 유지한다.
- `REMOVE_DATA=1` 없이는 token/job/event/diagnostics를 삭제하지 않는다.
- Code signing credential은 외부 secret으로만 전달한다.
- Unsigned dev build는 명시적 opt-in이다.
- WinSW binary provenance가 없으면 release build를 거부한다.
- Installer는 Linux `purecvisorsd` 또는 Single Edge 공개 UI/API 표면을 변경하지 않는다.

## 검증 기준

관리자 권한 없는 기본 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

WiX CLI가 있는 개발 환경에서의 build 검증:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.14.0-dev -WinSwPath '<winsw.exe>' -OutputRoot artifacts/windows-desktop-node -SigningMode AllowUnsignedDev
```

관리자 opt-in smoke:

```powershell
$msi = 'artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi'
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install.log
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
msiexec /i $msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log
msiexec /x $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall.log
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install-remove-data.log
msiexec /x $msi REMOVE_DATA=1 REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall-remove-data.log
```

관리자 smoke는 다음을 확인한다.

- install 후 service status가 started다.
- token 포함 runtime policy가 HTTP 200을 반환한다.
- loopback root가 HTTP 200을 반환한다.
- repair 후 token/job/event data가 보존된다.
- 기본 uninstall 후 ProgramData data root가 보존된다.
- `REMOVE_DATA=1` uninstall 후 token/job/event/install/diagnostics가 제거된다.
- port 7777 listener와 Windows service가 남지 않는다.

## 제외 범위

Phase 14는 다음을 구현하지 않는다.

- Burn bootstrapper와 prerequisite chain
- winget manifest 제출
- MSIX packaging
- DPAPI 또는 Windows Credential Manager token storage
- Windows Event Log provider
- LAN TLS/reverse proxy 정책
- Full updater와 config migration
- Product GA 승격 재판정

## 후속 Phase 연결

- Phase 15: Secure token storage. MSI property와 custom action data에 raw token을 넣지 않는 Phase 14 정책을 유지한다.
- Phase 16: Event Log와 long-term diagnostics. MSI action log와 product install log의 관계를 재정리한다.
- Phase 18: Update/rollback/config migration. MSI major upgrade와 product wrapper rollback의 책임 경계를 다시 정의한다.
- Phase 19: 제품 승격 재판정. Signed installer evidence와 provenance manifest를 GA gate 입력으로 사용한다.
