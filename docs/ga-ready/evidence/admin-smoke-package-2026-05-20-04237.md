# Admin-smoke package 2026-05-20 0.42.37

evidence_id: `admin-smoke-package-2026-05-20-04237`
result: `PASS`
scope: `internal-admin-smoke-fast-follow-hyperv-pause-lifecycle-fix`
version: `0.42.37-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260520-04237`
msi_sha256: `05dc31965af68792d21d919e19cb07997207d0514fd0ee39169d92129e95f67e`
payload_aggregate_sha256: `1e2487bfe474daad624a3ef67837a278ab5d25a71c654f8b7c18c95e3cc94e9e`
host_exe_sha256: `fb8e99c656750512b69154ed63de8f5e3b1884fad1ad190192e607dad01a41de`
cli_sha256: `f8e1a7e3350f6fcbb9a7fa38c64bbe55d0f26d64386e7a42d4069c534b04ecb0`
tui_sha256: `423e74661498198a736df4cfd8e0efe268d52712dae9d1ab424eddaada0f84e3`
provenance_commit: `9bed10099e1455717c89c8b2cc7481251705d609`
build_utc: `2026-05-19T19:38:24.8288363Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.35-admin-smoke` 설치본 lifecycle smoke 중 발견한 Hyper-V pause
결함을 닫는 fast-follow package다. 수정 범위는 두 가지다.

1. `vm.pause` WMI `RequestStateChange` 요청값을 결과 상태값 `32768`이 아니라 실제 요청
   상태값 `9`로 사용한다.
2. `RequestStateChange(9)` 이후 WMI `EnabledState=9`로 남는 VM inventory를 `paused`로
   매핑해 `vm get/list` guard가 `PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE`로 막히지
   않게 한다.

## 검증

- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "WmiVmProviderMapsPausedEnabledStatesForNativeParity|WmiVmPowerStateProviderUsesRequestStateChangeConstants|QueuedVmPowerStateWorkerDispatchesToNativeAdapterWithoutExternalFallback|NativeVmPowerStateAdapterMapsProviderResult"`: `21` passed
- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.37-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260520-04237 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20260520-04237/payload -Version 0.42.37-admin-smoke -BatchEvidenceRoot artifacts`: exit `0`, health HTTP `200`
- 실제 VM lifecycle smoke: `artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`, `ok=true`

## 설치본 lifecycle closure

`artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`는 실제 Hyper-V VM
`pcv-cli-04237-*`를 생성해 `host status`, `vm list`, `vm create`, `vm start`,
`vm memory-stats`, `vm cpu-stats`, `vm pause`, `vm get` after pause, `vm resume`,
`vm rename`, `vm list`, cleanup `poweroff/delete`를 모두 PASS로 기록했다.

`artifacts/hyperv-enabledstate-after-pcvcli-pause-20260520-04236/summary.json`는 root-cause
diagnostic으로, pause 직후 WMI `EnabledState=9`와 PowerShell `Get-VM` state 관측값을
기록한다. 이 diagnostic은 code fix의 근거이며 release/package claim 자체는
`0.42.37-admin-smoke` package와 설치본 smoke가 소유한다.

## 경계

`0.42.37-admin-smoke`는 `0.42.35` full admin/manual-admin closure 이후의 내부 fast-follow
설치본 검증 package다. Public trusted signing, public stable installer URL, winget
submission, 외부 stable publication은 주장하지 않는다.
