# Admin smoke package `0.42.71-admin-smoke` (2026-08-08)

evidence_id: `admin-smoke-package-2026-08-08-04271`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.71-admin-smoke`
source_commit: `80f69f31464ce07b2c9eca19211adf1232ea75f6`
artifact_root: `artifacts/admin-smoke-package-20260808-04271`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5`
clean_package_payload_aggregate_sha256: `4a333d60c8f9e10ea4c356f58913e8893d43be644c4736e7ed272e03c3f5a0af`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `2d3c077e6d8799d3636d9a037fdf33fa583957c0d0990cee15e0a3ed4a56995d`
cli_sha256: `4702220fa23933b005015c1bafc4109a9e50b7dd4f605110777fac2a82c3100b`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-08T14:23:21Z`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

설치본 operational anchor `0.42.70-admin-smoke`의 provenance commit `821a6a34` 이후 product
payload 변경이 쌓여 다음 package-pair 후보 `0.42.70 -> 0.42.71`를 열었다. 후속 계획
`docs/followup-work-plan-2026-08-07.md` §5(FC-12(b) 설치본 반영)가 이 package를 요구한다.

| 구간 | 내용 |
| --- | --- |
| baseline package | `0.42.70-admin-smoke` / `821a6a34` / MSI `b28e1876…` |
| build HEAD | `80f69f31` (`codex/followup-work-2026-08-08`) |
| payload 변경 파일 수 | `59` (`src/`, `web/`, product wrapper 경로 diff) |

### 이 package가 담는 주요 product payload

| 커밋/작업 | 내용 |
| --- | --- |
| `c1bc5f40` | FC-12(b) guest-exec argv를 데이터로 전달 (공백 join 재파싱 제거) |
| `2e2f72ff` 등 | `DesktopNodeApiRequestProcessor` route owner 분해 |
| `5e370f06` | wave 1 소유자 response helper 사본 제거 |
| `80f69f31` | `web/src/served-app.ts` → domain part `18`개 분해 |
| Host Ops 분해 | `DesktopNode.Host` service-action Ops 분리 (descriptor trigger) |

## 빌드 명령

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version '0.42.71-admin-smoke' `
  -OutputRoot (Join-Path (Get-Location) 'artifacts/admin-smoke-package-20260808-04271') `
  -SigningMode AllowUnsignedDev `
  -SigningTrustModel LocalTest
```

선행 dry-run도 동일 인자로 `ok=true`였다.

## 빌드 결과

| item | result |
| --- | --- |
| installer dry-run | PASS |
| self-contained `DesktopNode.Host.exe` publish | PASS |
| self-contained `pcvcli.exe` publish | PASS |
| MSI build (WiX 5.0.2) | PASS |
| MSI SHA-256 (실측 = provenance = sidecar hash) | 3중 일치 |
| provenance commit == HEAD | `true` (`80f69f31…`) |

## Payload 내용 검증

| 검사 | 결과 |
| --- | --- |
| payload 파일 수 | `8` |
| payload `web/app.js`에 `hasRefreshedOperation` | 포함 |
| payload `web/app.js`에 `renderVms` | 포함 |
| payload `web/app.js`에 조작값 `VM: 3/3` | 없음 |
| 소스 `GuestArgvInvocation` / `-ArgumentList` argv 데이터 경로 | HEAD 소스에 존재 (설치본 왕복은 fullgate 이후) |

payload 구성: `DesktopNode.Host.exe`, `pcvcli.exe`, `Invoke-PcvDesktopNodeProduct.ps1`,
`PcvDesktopNodeProduct.psm1`, `product-manifest.json`, `web/app.js`, `web/index.html`,
`web/styles.css`.

## 다음 단계

1. full admin host mutation gate (`0.42.71-admin-smoke`) — **관리자 opt-in**
2. 설치본 guest-exec 왕복(FC-12(b) 설치본 확인) 및 current-card
3. manual-admin package-pair `0.42.70-admin-smoke -> 0.42.71-admin-smoke` closure

## Nonclaims

- 설치, 서비스, Hyper-V mutation을 수행하지 않았다. 파일 생성만 했다.
- operational full-gate MSI는 별도 빌드일 수 있으며 hash가 다를 수 있다.
- publication descriptor는 Burn `not-built`, MSIX `not-built`, winget `not-generated`,
  catalog `not-published`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
- 설치본 guest-exec 왕복 PASS를 이 evidence가 주장하지 않는다.
