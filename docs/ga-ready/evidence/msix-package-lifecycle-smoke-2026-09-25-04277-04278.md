# MSIX build/install/update/remove `0.42.77.0` -> `0.42.78.0`

evidence_id: `msix-package-lifecycle-smoke-2026-09-25-04277-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/msix-package-lifecycle-smoke-20260925-04277-04278`
package_identity: `PureCVisor.DesktopNode.MsixSmoke`
smoke_service: `PureCVisorDesktopNodeMsixSmoke`
baseline_msix_sha256: `d4d45761138faad44b192c68ca0b5d86247f009b0ec35f2d66fa858e8b839e05`
target_msix_sha256: `43de3769930165495ef40bc8f5d44843de610f45dceb24c65e3d78135401075d`
signer_thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
signing_trust_model: `internal-root-leaf`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 baseline `0.42.77.0`과
target `0.42.78.0`을 pack, 내부 코드 서명, 설치, 갱신, 제거했다. MSI 제품
`PureCVisorDesktopNode`와 product manifest는 바꾸지 않았다.

| 단계 | exit |
| --- | --- |
| pack baseline | `0` |
| sign/verify baseline | `0` |
| pack target | `0` |
| sign/verify target | `0` |
| install `0.42.77.0` | 통과 |
| update `0.42.78.0` | 통과 |
| remove | `PASS` |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| smoke package | 없음 |
| `PureCVisorDesktopNodeMsixSmoke` | 없음 |
| MSI service | `PureCVisorDesktopNode` Running / Automatic |
| product manifest | `0.42.77-admin-smoke` |
| manifest 변경 | 없음 |
| VM | `pcv-cleanhost-20260910-r2-04274-04275` Saved, `pcv-guest-installed-04253-r1` Off |

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| installed runtime ops summary | `not-run` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `0.42.77-admin-smoke -> 0.42.78-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.77-admin-smoke` |

## Nonclaims

- 서명은 ADR-0003 내부 root/leaf다. public trusted signing이 아니다.
- operational current를 `0.42.78-admin-smoke`로 올리지 않았다.
- external stable publication을 주장하지 않는다.
