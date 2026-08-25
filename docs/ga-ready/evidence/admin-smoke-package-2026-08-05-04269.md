# Admin smoke package `0.42.69-admin-smoke` (2026-08-05)

evidence_id: `admin-smoke-package-2026-08-05-04269`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.69-admin-smoke`
source_commit: `7236b813d6a4f594abb8e126b2b5dfb2ad56c1e9`
artifact_root: `artifacts/admin-smoke-package-20260805-04269`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `7a3729224d4a66df9a28b9e8f4f2649949361d9ca66bfce34d04caed390e198b`
clean_package_payload_aggregate_sha256: `a6a2408a3e0b3bbe293a83b7133f1ef45aa97da034aaca9fce8b7bda2856070b`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

`0.42.68-admin-smoke`는 커밋 `f9337061`에서 빌드됐고 그 이후 제품 코드 커밋 `13`건이 `main`에
들어갔다. `0.42.68`을 그대로 승격하면 아래 수정이 모두 빠진 패키지가 anchor가 된다. 따라서 현재
`main`(`7236b813`)에서 새 후보를 빌드했다.

| 커밋 | 내용 |
| --- | --- |
| `0035e000` | Gen2 Secure Boot 템플릿. 없으면 Linux ISO 부팅 불가 |
| `45ba267e` | Gen2 boot order. 없으면 ISO가 PXE와 빈 디스크 뒤로 밀림 |
| `dcb703ad` | guest execution UTF-8 고정. 없으면 비 ASCII credential 손상 |
| `cb231f17` `0501fbec` `e4828bcf` `0fda9739` | Web Console 운영 상태 진실성 |

## 빌드 결과

| item | result |
| --- | --- |
| installer dry-run | PASS |
| self-contained `DesktopNode.Host.exe` publish | PASS |
| self-contained `pcvcli.exe` publish | PASS |
| MSI build | PASS |
| MSI SHA-256 (실측 = provenance = sidecar) | 3중 일치 |
| provenance commit == HEAD | `true` |

## Payload 내용 검증

빌드 산출물이 실제로 오늘 작업을 담았는지 문자열로 확인했다.

| 검사 | 결과 |
| --- | --- |
| payload `app.js`에 `hasRefreshedOperation` | 포함 |
| payload `app.js`에 조작값 `VM: 3/3` | 없음 |

## Nonclaims

- 설치, 서비스, Hyper-V mutation을 수행하지 않았다. 파일 생성만 했다.
- publication descriptor는 Burn `not-built`, MSIX `not-built`, winget `not-generated`,
  catalog `not-published`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
