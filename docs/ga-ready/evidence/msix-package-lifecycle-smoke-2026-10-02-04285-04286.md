# MSIX package lifecycle smoke `0.42.85` -> `0.42.86` (2026-10-02)

evidence_id: `msix-package-lifecycle-smoke-2026-10-02-04285-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/msix-package-lifecycle-smoke-20261002-04285-04286`
summary_sha256: `6ad0f713ce5b72408e0700afdaba5dea819a0c12ab9dbdf1e94ea9874f9d7066`
baseline_msix_sha256: `6ab64c08246bfdec74bcd77c1832d78360c97411b4f316d936310cdb29103cef`
baseline_msix_bytes: `66683056`
target_msix_sha256: `5abd887e3c41751c22a5e2dca88770dcf281b6c0e9b10844a998225e38d01ad4`
target_msix_bytes: `66683477`
package_identity: `PureCVisor.DesktopNode.MsixSmoke`
smoke_service: `PureCVisorDesktopNodeMsixSmoke`
signer_thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
signing_trust_model: `internal-root-leaf`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1`
template_layout: `packaging/windows-desktop-node/msix/template-layout`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 두 버전을 pack했다. 내부 코드 서명 인증서(CurrentUser\My)로 서명·검증한 뒤 설치, 갱신, 제거했다. MakeAppx와 SignTool은 Windows SDK `10.0.26100.0` x64다. 버전은 세 자리로 넘겼다(runner가 `.0`을 붙인다).

- baseline `0.42.85.0`: `admin-smoke-package-20261001-04285/payload`
- target `0.42.86.0`: `admin-smoke-package-20261002-04286/payload`

인증서는 새로 만들지 않았다. MSI 제품과 product manifest는 바꾸지 않았다.

| 단계 | exit / 상태 |
| --- | --- |
| pack / sign / verify baseline | `0` / `0` / `0` |
| pack / sign / verify target | `0` / `0` / `0` |
| install `0.42.85.0` | 통과 |
| update `0.42.86.0` | 통과 |
| remove | `PASS` (cleanup probe `PASS`) |

summary SHA-256은 직전 MSIX run(`msix-package-lifecycle-smoke-2026-10-01-04284-04285`)과 같다. 이 summary에는 상태 필드만 있고 경로와 시각이 없어서, 결과가 같으면 SHA도 같다. baseline/target MSIX 바이트 SHA는 이번 payload로 새로 계산한 값이다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| smoke package | 없음 |
| `PureCVisorDesktopNodeMsixSmoke` | 없음 |
| MSI 서비스 `PureCVisorDesktopNode` | Running / Automatic |
| product manifest | `0.42.86-admin-smoke` (변경 없음) |

## Nonclaims

- 내부 root-leaf 서명이다. public trusted signing이 아니다.
- operational current는 `0.42.84-admin-smoke`다.
- external stable publication을 주장하지 않는다.
