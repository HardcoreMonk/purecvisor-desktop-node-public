# MSIX Package Lifecycle Smoke Evidence - 2026-05-10 0.41.6

이 문서의 본문은 한국어 운영자 설명을 기준으로 작성한다. `MSIX Package Lifecycle Smoke Evidence` 제목과 아래 metadata token은 기존 evidence id와 테스트 계약을 위해 보존한다.

evidence_id: msix-package-lifecycle-smoke-2026-05-10-0416
scope: msix-package-build-install-update-remove-smoke
result: PASS
product_version_source: 0.41.5-admin-smoke to 0.41.6-admin-smoke package pair
artifact_root: artifacts/msix-package-lifecycle-smoke-20260510-0416
summary: artifacts/msix-package-lifecycle-smoke-20260510-0416/summary.json
host_mutation_performed: true
public_trusted_signing: excluded
external_stable_publication: not-claimed
msix: build-install-update-remove-pass-internal-smoke

이 evidence는 현재 package pair에 대해 administrator opt-in으로 실행한 host mutation run을 기록한다. 실행은 `0.41.5-admin-smoke` baseline payload와 `0.41.6-admin-smoke` target payload에서 별도 smoke package identity를 빌드한 뒤 install/update/remove를 수행했다.

이 evidence는 ADR-0003 internal Root/leaf signing trust model 범위 안에 있다. Public trusted signing 또는 external stable publication evidence가 아니다.

## Package 경계

- Package identity: `PureCVisor.DesktopNode.MsixSmoke`
- Packaged service name: `PureCVisorDesktopNodeMsixSmoke`
- Publisher: `CN=PureCVisor Desktop Node Internal Code Signing`
- Signer thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- Internal root thumbprint: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
- Restricted capabilities: `runFullTrust`, `packagedServices`, `localSystemServices`
- Baseline payload: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415/payload`
- Baseline publication descriptor: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415/PureCVisorDesktopNode-0.41.5-admin-smoke-windows-x64.publication.json`
- Target payload: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/target-0416/payload`
- Target publication descriptor: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/target-0416/PureCVisorDesktopNode-0.41.6-admin-smoke-windows-x64.publication.json`

## 실행

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File artifacts/msix-package-lifecycle-smoke-20260510-0416/msix-package-lifecycle-runner.ps1
```

생성된 runner는 Windows SDK `makeappx.exe`와 `signtool.exe`로 signed MSIX package 두 개를 빌드했다. 이후 version `0.41.5.0`을 설치하고, version `0.41.6.0`으로 update한 다음 package를 제거했다. AppX lifecycle이 완료된 뒤 runner process가 shell timeout을 넘었기 때문에, `summary.json`은 built package, signing log, AppX deployment event, final absence check를 기준으로 재구성했다.

## 관찰 결과

- v1 MSIX: `PureCVisorDesktopNode-MsixSmoke-0.41.5.0.msix`
- v1 SHA-256: `c2efc20e29d950f4e2abd924c13c003cb734bc46e95ccd5aacdd7a724a188674`
- v2 MSIX: `PureCVisorDesktopNode-MsixSmoke-0.41.6.0.msix`
- v2 SHA-256: `8329e0af985185515dac65353398763f5951852faecc928b9925de6fb03dc871`
- `makeappx` package creation: PASS for both versions
- `signtool sign`: PASS for both versions
- `signtool verify /pa /v`: PASS for both versions, warnings `0`, errors `0`; packages were not timestamped
- `Add-AppxPackage` install v1: PASS, package `PureCVisor.DesktopNode.MsixSmoke_0.41.5.0_x64__he18zmqd2ahap`
- `Add-AppxPackage` update v2: PASS, package `PureCVisor.DesktopNode.MsixSmoke_0.41.6.0_x64__he18zmqd2ahap`
- `Remove-AppxPackage` remove v2: PASS
- Final smoke package absent: `true`
- Final smoke service absent: `true`
- Existing MSI service `PureCVisorDesktopNode`: `Running`

## 경계

이 문서는 smoke identity와 current `0.41.5` to `0.41.6` package pair에 대한 내부 MSIX package build/install/update/remove smoke만 닫는다. MSI-first product distribution decision을 대체하지 않고, Store/MSIX public package를 제출하지 않으며, public trusted signing 또는 external stable publication을 주장하지 않는다.
