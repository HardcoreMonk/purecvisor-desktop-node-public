# MSIX build/install/update/remove `0.42.78.0` -> `0.42.83.0`

evidence_id: `msix-package-lifecycle-smoke-2026-09-29-04278-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/msix-package-lifecycle-smoke-20260929-04278-04283`
summary_sha256: `6ad0f713ce5b72408e0700afdaba5dea819a0c12ab9dbdf1e94ea9874f9d7066`
package_identity: `PureCVisor.DesktopNode.MsixSmoke`
smoke_service: `PureCVisorDesktopNodeMsixSmoke`
baseline_msix_sha256: `8e305045a24abb3ead171d6823ba6fa9264f30b080690f7f02d04f2b3600a851`
target_msix_sha256: `3cbfa59dc1f87f6ae77f69dff142b6325b272c9ea1af46e871cd3293b6040e00`
signer_thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
signing_trust_model: `internal-root-leaf`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1`
runner_sha256: `0be42d174923a29be12ff2cca4340492df90a196b49321c1f4140d9f411be3be`
template_layout: `packaging/windows-desktop-node/msix/template-layout`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner와 템플릿 layout은 `f3ac8db`에서 저장소에 들여왔다. runner는 0.42.78 run 때 미추적 worktree에서 실행한 파일과 SHA가 같다. 템플릿은 2026-08-31 이후 run들이 쓴 layout과 내용이 같다.

분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 두 버전을 pack하고, 내부 코드 서명 인증서(`CN=PureCVisor Desktop Node Internal Code Signing`)로 서명·검증한 뒤 설치, 갱신, 제거했다.
- baseline `0.42.78.0`: `admin-smoke-package-20260925-04278/payload`
- target `0.42.83.0`: `admin-smoke-package-20260929-04283/payload`

실행 시간은 2026-09-29 `14:18:16Z`부터 `14:18:36Z`까지다. 인증서는 새로 만들지 않았다(`certificate_generation=forbidden`). MSI 제품 `PureCVisorDesktopNode`와 product manifest는 바꾸지 않았다.

| 단계 | exit / 상태 |
| --- | --- |
| pack baseline | `0` |
| sign/verify baseline | `0` / `0` |
| pack target | `0` |
| sign/verify target | `0` / `0` |
| install `0.42.78.0` | 통과(package version, smoke service Manual / LocalSystem) |
| update `0.42.83.0` | 통과 |
| remove | `PASS` |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| smoke package | 없음 |
| `PureCVisorDesktopNodeMsixSmoke` | 없음 |
| MSI 서비스 `PureCVisorDesktopNode` | Running / Automatic |
| product manifest | `0.42.83-admin-smoke` (변경 없음) |

## 아직 닫히지 않은 후속

| 후속 검증 | 결과 |
| --- | --- |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.78-admin-smoke` |

## Nonclaims

- 내부 root-leaf 서명이다. public trusted signing이 아니다.
- operational current는 `0.42.78-admin-smoke`다.
- external stable publication을 주장하지 않는다.
