# MSIX package lifecycle smoke `0.42.84` -> `0.42.85` (2026-10-01)

evidence_id: `msix-package-lifecycle-smoke-2026-10-01-04284-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/msix-package-lifecycle-smoke-20261001-04284-04285`
summary_sha256: `6ad0f713ce5b72408e0700afdaba5dea819a0c12ab9dbdf1e94ea9874f9d7066`
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
- baseline `0.42.84.0`: `admin-smoke-package-20260930-04284/payload`
- target `0.42.85.0`: `admin-smoke-package-20261001-04285/payload`

실행 시간은 2026-09-30 `15:40:07Z`부터 `15:40:25Z`까지다. 인증서는 새로 만들지 않았다. MSI 제품과 product manifest는 바꾸지 않았다.

| 단계 | exit / 상태 |
| --- | --- |
| pack / sign / verify baseline | `0` / `0` / `0` |
| pack / sign / verify target | `0` / `0` / `0` |
| install `0.42.84.0` | 통과 |
| update `0.42.85.0` | 통과 |
| remove | `PASS` (cleanup probe `PASS`) |

summary SHA-256은 0.42.84 run(`msix-package-lifecycle-smoke-2026-09-30-04283-04284`)과 같다. 이 summary에는 상태 필드만 있고 경로와 시각이 없어서, 결과가 같으면 SHA도 같다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| smoke package | 없음 |
| `PureCVisorDesktopNodeMsixSmoke` | 없음 |
| MSI 서비스 `PureCVisorDesktopNode` | Running / Automatic |
| product manifest | `0.42.85-admin-smoke` (변경 없음) |

## Nonclaims

- 내부 root-leaf 서명이다. public trusted signing이 아니다.
- operational current는 `0.42.84-admin-smoke`다.
- external stable publication을 주장하지 않는다.
