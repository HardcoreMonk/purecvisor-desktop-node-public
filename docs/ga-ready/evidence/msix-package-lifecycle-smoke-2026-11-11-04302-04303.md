# MSIX package lifecycle smoke `0.43.2` -> `0.43.3` (2026-11-11)

evidence_id: `msix-package-lifecycle-smoke-2026-11-11-04302-04303`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04302-04303/msix-package-lifecycle-smoke`
summary_sha256: `6ad0f713ce5b72408e0700afdaba5dea819a0c12ab9dbdf1e94ea9874f9d7066`
baseline_msix_sha256: `f8ca64221a59a5776f049c62caa4046b1dff315bc3250b9ea40125981cf97652`
baseline_msix_bytes: `71699197`
target_msix_sha256: `bd670e916524175e24b967fea7ba38f98f1e65ce6a5693d8a5aa55ea4d3093d3`
target_msix_bytes: `71699334`
package_identity: `PureCVisor.DesktopNode.MsixSmoke`
smoke_service: `PureCVisorDesktopNodeMsixSmoke`
signer_thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
signing_trust_model: `internal-root-leaf`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1`
template_layout: `packaging/windows-desktop-node/msix/template-layout`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.3` pair orchestrator의 MSIX bucket이다. 분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 두 버전을 pack했다. 내부 코드 서명 인증서로 서명·검증한 뒤 설치, 갱신, 제거했다. 약 `25`초.

- baseline `0.43.2.0`: `admin-smoke-package-20261110-04302/payload`
- target `0.43.3.0`: `admin-smoke-package-20261111-04303/payload`

| 단계 | exit / 상태 |
| --- | --- |
| pack / sign / verify baseline | `0` / `0` / `0` |
| pack / sign / verify target | `0` / `0` / `0` |
| install `0.43.2.0` | 통과 |
| update `0.43.3.0` | 통과 |
| remove | `PASS` (cleanup probe `PASS`, final absence probe `PASS`) |

summary는 상태 필드만 담는다. MSIX 바이트 SHA는 이번 payload로 새로 계산한 값이다.

## 최종 상태

smoke package와 `PureCVisorDesktopNodeMsixSmoke` 없음. MSI 서비스 `PureCVisorDesktopNode` Running/Automatic, product manifest `0.43.3-admin-smoke`(변경 없음).

## Nonclaims

- 내부 root-leaf 서명이다. public trusted signing이 아니다.
- operational current는 `0.43.2-admin-smoke`다.
- external stable publication을 주장하지 않는다.
