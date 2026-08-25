# ADR-0003: 내부 신뢰 기반 RequireSigned release signing 정책

- 상태: 적용 중
- 날짜: 2026-05-01
- 결정 마커:
  - `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`
  - `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`

## 맥락

Desktop Node는 외부 일반 사용자 배포가 아니라 내부 전용 서비스이다. 이 운영 범위에서는 유료 public CA code signing certificate, Azure Trusted Signing, AD CS, Windows domain, Intune, MDM을 사용할 수 없다.

기존 Phase 20/22 계약은 `rc`와 `stable` channel에서 unsigned artifact를 차단하고 `RequireSigned` MSI를 요구한다. 이 요구는 유지하되, public trusted signing은 내부 전용 서비스 완료 조건이 아니다.

## 결정

내부 서비스 운영 범위에서는 public CA 대신 전용 internal root/leaf code signing 체계를 허용한다.

- `RequireSigned`는 계속 필수다.
- 내부 signing은 전용 Root CA 인증서와 leaf Code Signing 인증서로 구성한다.
- leaf 인증서만 MSI 서명에 사용한다.
- root/leaf public certificate는 대상 host의 승인된 trust store에 배포한다.
- private key, PFX password, raw API token, protected token blob은 repo, provenance, evidence에 기록하지 않는다.
- installer provenance는 `signing_trust_model`을 기록한다.
- 내부 운영 release evidence는 `signing_trust_model = InternalEnterprise`로만 주장한다.
- `LocalTest`는 개발자/test host workaround이고 내부 운영 release evidence가 아니다.
- `PublicTrusted`는 public CA 또는 Azure Trusted Signing 같은 공개 신뢰 체인이 실제로 사용된 경우에만 기록한다.

AD CS/Intune/MDM이 없으므로 target host trust 배포는 수동 또는 별도 승인된 운영 스크립트로 수행한다. 일반 target host에는 public root/leaf `.cer`만 배포하며, signing private key는 배포하지 않는다.

## 채택하지 않는 것

이 ADR은 다음을 채택하지 않는다.

- public trusted signing evidence 완료 주장
- 외부 일반 사용자 대상 stable publication
- Windows domain, AD CS, Intune, MDM 전제
- repo 내부 PFX/private key 보관
- MSI 자동 reboot 허용
- `AllowUnsignedDev`를 RC/stable에 허용하는 예외

## 운영 계약

빌드 host:

- leaf Code Signing 인증서는 `CurrentUser\My` 또는 승인된 machine/HSM signing store에 private key와 함께 존재해야 한다.
- build command는 `-SigningMode RequireSigned`와 `-SigningTrustModel InternalEnterprise`를 함께 사용한다.
- SignTool signing과 timestamp 결과, Authenticode 검증 결과, MSI SHA-256, provenance를 evidence로 남긴다.

Target host:

- internal Root public certificate는 `Trusted Root Certification Authorities`에 설치한다.
- leaf public certificate는 `Trusted Publishers`에 설치한다.
- AD/MDM이 없으면 각 host에서 관리자 승인 하에 trust import를 수행한다.
- private key나 PFX는 target host에 배포하지 않는다.

MSI lifecycle:

- `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` smoke는 계속 관리자 opt-in gate다.
- 모든 MSI 호출은 `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/qn`, `/norestart`를 사용한다.
- repair `3010`은 service/runtime/data assertion 통과 후 reboot-required success로만 기록한다.
- `1641`은 실제 reboot initiated failure로 기록한다.

Runbook:

- Installer runbook은 `packaging/windows-desktop-node/installer/README.md`의 `Internal RequireSigned gate runbook`을 따른다.
- `New-PcvInternalCodeSigningTrust.ps1 -DryRun`은 plan-only check이며 LocalMachine trust import를 실행하지 않는다.
- 실제 internal Root/leaf 생성, `LocalMachine` Root/TrustedPublisher import, signed MSI build, elevated MSI lifecycle smoke는 관리자 opt-in gate로만 실행한다.

## 현재 evidence

2026-05-01 내부 신뢰 기반 RC evidence:

- evidence root: `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`
- version: `0.23.10-rc.1`
- git commit: `318ebc39b8f224c7c24895c485089b1469c4ac66`
- signing trust model: `InternalEnterprise`
- signer: `CN=PureCVisor Desktop Node Internal Code Signing`
- issuer: `CN=PureCVisor Internal Code Signing Root CA`
- signing store: `Cert:\CurrentUser\My`
- trust stores: `Cert:\LocalMachine\Root`, `Cert:\LocalMachine\TrustedPublisher`
- MSI: `PureCVisorDesktopNode-0.23.10-rc.1-windows-x64.msi`
- MSI SHA-256: `5355507f5909d5e17280a90b8ac41af858b871633b8ec2e1b03f2b4eb26297ba`
- Authenticode: `Valid`
- SignTool verify exit: `0`
- lifecycle: install, repair, uninstall preserve, reinstall, `REMOVE_DATA=1` uninstall, final restore install all PASS
- automatic reboot: not used
- boot time unchanged: true

이 evidence는 내부 신뢰 기반 release evidence이며, public trusted signing 또는 외부 stable publication evidence가 아니다.

## 영향 범위

- 포함 경로:
  - `packaging/windows-desktop-node/installer/**`
  - `docs/**`
  - `AGENTS.md`
- 제외 경로:
  - Linux `purecvisor-single`
  - Linux `purecvisorsd`
  - KVM/libvirt/LXC/ZFS/OVS/OVN runtime

## 검증 기준

문서/installer signing contract 변경 후:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

내부 trust bootstrap과 signed MSI build는 관리자 opt-in 및 signing material gate다. 실제 host mutation evidence는 artifacts 아래에만 남기고, repo에는 private key/PFX를 기록하지 않는다.

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/adr/0002-release-version-policy.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md`
- `packaging/windows-desktop-node/installer/README.md`
