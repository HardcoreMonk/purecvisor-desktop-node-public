# Admin smoke package `0.42.70-admin-smoke` (2026-08-06)

evidence_id: `admin-smoke-package-2026-08-06-04270`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.70-admin-smoke`
source_commit: `821a6a342465ee1c8e17bd8d9a9aa4b27a0a6d6d`
artifact_root: `artifacts/admin-smoke-package-20260805-04270`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `b28e18763ac01137039a9bcfafe0c151945304c8449e307b0412038d6726c86c`
clean_package_payload_aggregate_sha256: `e5bf399740afa6f858a9e2e5fb03317e2588bf7e78eb9342c6f5a58dc6df2a94`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-05T10:23:01Z`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

이 package는 manual-admin campaign `0.42.69 -> 0.42.70`의 target으로 2026-08-05에 빌드됐다.
`0.42.69` anchor 승격과 달리 이번에는 재빌드하지 않고 그 package를 그대로 승격 후보로 쓴다.

## 재빌드하지 않은 근거

`0.42.69` 승격 때는 후보 package가 커밋 `f9337061` 기준이라 이후 제품 코드 커밋 `13`건이
빠져서 재빌드가 필요했다. 이번에는 그 조건이 성립하지 않는다.

package provenance commit `821a6a34`와 승격 시점 `main` HEAD `e9138988` 사이의 커밋은 `4`건이고,
payload를 만드는 경로의 변경은 `0`건이다.

| 커밋 | 변경 경로 | payload 영향 |
| --- | --- | --- |
| `4fe1a76a` | `packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1` | 없음 |
| `276afeca` | `docs/**` | 없음 |
| `bb65fd1b` | merge | 없음 |
| `e9138988` | `docs/**`, `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1` | 없음 |

payload `8`개 파일의 source 경로는 `src/**`, `web/**`,
`packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`,
`packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`다. 실측 확인:

```text
git diff --name-only 821a6a34..e9138988 -- src/ web/ \
  packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 \
  packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1
(출력 없음)
```

두 `tools/**` 스크립트는 MSI payload에 포함되지 않는다. payload는
`DesktopNode.Host.exe`, `pcvcli.exe`, `Invoke-PcvDesktopNodeProduct.ps1`,
`PcvDesktopNodeProduct.psm1`, `product-manifest.json`, `web/app.js`, `web/index.html`,
`web/styles.css` `8`개다.

따라서 이 package는 승격 시점 HEAD의 제품 payload와 동일한 source에서 빌드됐다.

## 이 package가 이미 통과한 검증

`0.42.70`은 anchor 후보가 되기 전에 manual-admin campaign의 target으로 이미 실사용됐다.

| 검증 | 결과 | 소유 evidence |
| --- | --- | --- |
| installed update/rollback | `PASS` | `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md` |
| dedicated clean-host install/update/rollback | `PASS` | 동일 |
| Burn install/repair/remove | `PASS` | 동일 |
| MSIX build/install/update/remove | `PASS` | 동일 |
| installed runtime ops summary | `PASS` | 동일 |

## Nonclaims

- 이 문서는 clean package build evidence다. Operational full-gate MSI는 별도 빌드이며
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-06-04270-hostmutation.md`가
  소유한다. 두 MSI는 같은 source에서 나와도 hash가 다르다.
- `signing_mode`가 `AllowUnsignedDev`이므로 public trusted signing evidence가 아니다.
- external stable publication을 주장하지 않는다.
