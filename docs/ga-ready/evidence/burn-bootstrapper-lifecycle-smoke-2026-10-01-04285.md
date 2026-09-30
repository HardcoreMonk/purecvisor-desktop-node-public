# Burn bootstrapper lifecycle smoke `0.42.85-admin-smoke` (2026-10-01)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-01-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20261001-04285-r2`
bundle: `artifacts/burn-bootstrapper-lifecycle-20261001-04285-r2/PureCVisorDesktopNode-0.42.85-admin-smoke-bootstrapper.exe`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `f4e4b73c1e53d5b36125763e8a6e1efc538f2ea41bbf198095e90dd162295300`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

먼저 설치본을 제품 Update로 `0.42.85-admin-smoke`에 맞췄다(`pre-update.json`, `ok=true`). 그 뒤 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다.

### r1 (FAIL, `artifacts/burn-bootstrapper-lifecycle-20261001-04285`)

실행 시간은 2026-09-30 `15:38:40Z`부터 `15:40:06Z`까지다. build와 install은 `0`이었다. repair가 `3010`(성공, 재부팅 필요)을 냈고 runner가 이를 FAIL로 판정했다(`status=FAIL`).

- repair MSI 로그에 정보 `1603`("`DesktopNode.Host.exe`를 다른 프로세스에서 사용 중")이 있다. Restart Manager가 사용 중 프로그램을 종료했지만, 파일 하나의 교체가 재부팅 뒤로 미뤄졌다.
- 시작 시점에 `MsiSystemRebootPending=1`이었다. 대기 중인 파일 이름 변경 `10`건은 이전 run들이 남긴 bootstrapper 임시 파일 삭제다.
- 복구 단계(target MSI 재설치, `restore-target-msi` exit `0`)는 PASS였다.
- 뒤이어 확인한 결과, `DesktopNode.Host.exe`나 `pcvcli.exe` 교체는 재부팅 대기에 걸려 있지 않다. 설치본 Host는 clean `0.42.85`(`e1c0cb89…`)다.

0.42.83과 0.42.84 run에서는 같은 repair가 `0`이었다. 그래서 제품 Update로 서비스를 재시작한 직후에 생긴 타이밍 문제로 판단했다.

### r2 (PASS, `artifacts/burn-bootstrapper-lifecycle-20261001-04285-r2`)

r1 복구 뒤 5초를 기다려 새 root에서 다시 실행했다. 실행 시간은 `15:41:19Z`부터 `15:41:48Z`까지다.

```text
-ArtifactRoot artifacts/burn-bootstrapper-lifecycle-20261001-04285-r2
-TargetMsiPath artifacts/admin-smoke-package-20261001-04285/PureCVisorDesktopNode-0.42.85-admin-smoke-windows-x64.msi
-TargetVersion 0.42.85-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Execute
```

| 단계 | exit |
| --- | --- |
| bundle build | `0` |
| `/install /quiet /norestart` | `0` |
| `/repair /quiet /norestart` | `0` |
| `/uninstall /quiet /norestart` | `0` |
| target MSI 복구 | `0` |

lifecycle summary는 `ok=true`, `status=PASS`, `restoration_status=PASS`다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{B23EB9BF-F5DA-43BD-9BA4-E827F766387F}` `0.42.85` (1개) |
| product manifest | `0.42.85-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |

## report-only

- 서비스 재시작 직후의 Burn repair는 `DesktopNode.Host.exe`가 아직 잠겨 있어 `3010`을 낼 수 있다. runner는 `3010`을 FAIL로 본다. 다음 pair에서도 나오면 runner에 대기나 재시도를 둘지 결정이 필요하다.

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.84-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
