# MSIX package lifecycle smoke `0.42.90` -> `0.42.91` (2026-10-06)

evidence_id: `msix-package-lifecycle-smoke-2026-10-06-04290-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261006-04290-04291/msix-package-lifecycle-smoke`
summary_sha256: `6ad0f713ce5b72408e0700afdaba5dea819a0c12ab9dbdf1e94ea9874f9d7066`
baseline_msix_sha256: `93cffc21489916f228ac83f16b2ea44702833ec4ba72ee54161393f1f32db9e8`
baseline_msix_bytes: `66690369`
target_msix_sha256: `4c6fb0050b79cef9fe97e5fc051915fe2773cf7d54df841a087b0c2655aa3e8c`
target_msix_bytes: `66690415`
package_identity: `PureCVisor.DesktopNode.MsixSmoke`
smoke_service: `PureCVisorDesktopNodeMsixSmoke`
signer_thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
signing_trust_model: `internal-root-leaf`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1`
template_layout: `packaging/windows-desktop-node/msix/template-layout`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.91` pair orchestrator의 MSIX bucket이다. 분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 두 버전을 pack했다. 내부 코드 서명 인증서로 서명·검증한 뒤 설치, 갱신, 제거했다. 약 `27`초.

- baseline `0.42.90.0`: `admin-smoke-package-20261005-04290/payload`
- target `0.42.91.0`: `admin-smoke-package-20261006-04291/payload`

| 단계 | exit / 상태 |
| --- | --- |
| pack / sign / verify baseline | `0` / `0` / `0` |
| pack / sign / verify target | `0` / `0` / `0` |
| install `0.42.90.0` | 통과 |
| update `0.42.91.0` | 통과 |
| remove | `PASS` (cleanup probe `PASS`, final absence probe `PASS`) |

summary는 상태 필드만 담는다. MSIX 바이트 SHA는 이번 payload로 새로 계산한 값이다.

## 최종 상태

smoke package와 `PureCVisorDesktopNodeMsixSmoke` 없음. MSI 서비스 `PureCVisorDesktopNode` Running/Automatic, product manifest `0.42.91-admin-smoke`(변경 없음).

## Nonclaims

- 내부 root-leaf 서명이다. public trusted signing이 아니다.
- operational current는 `0.42.90-admin-smoke`다.
- external stable publication을 주장하지 않는다.
