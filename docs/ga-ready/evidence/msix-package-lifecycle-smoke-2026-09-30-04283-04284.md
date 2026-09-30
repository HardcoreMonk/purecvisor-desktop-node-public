# MSIX package lifecycle smoke `0.42.83` -> `0.42.84` (2026-09-30)

evidence_id: `msix-package-lifecycle-smoke-2026-09-30-04283-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/msix-package-lifecycle-smoke-20260930-04283-04284-r2`
summary_sha256: `6ad0f713ce5b72408e0700afdaba5dea819a0c12ab9dbdf1e94ea9874f9d7066`
package_identity: `PureCVisor.DesktopNode.MsixSmoke`
smoke_service: `PureCVisorDesktopNodeMsixSmoke`
signer_thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
signing_trust_model: `internal-root-leaf`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1`
template_layout: `packaging/windows-desktop-node/msix/template-layout`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 두 버전을 pack했다. 내부 코드 서명 인증서(`CN=PureCVisor Desktop Node Internal Code Signing`, CurrentUser\My)로 서명·검증한 뒤 설치, 갱신, 제거했다. MakeAppx와 SignTool은 Windows SDK `10.0.26100.0` x64다.
- baseline `0.42.83.0`: `admin-smoke-package-20260929-04283/payload`
- target `0.42.84.0`: `admin-smoke-package-20260930-04284/payload`

첫 실행(`artifacts/msix-package-lifecycle-smoke-20260930-04283-04284`)은 `FAIL`이었다. `-BaselineVersion 0.42.83.0`처럼 네 자리 버전을 넘겼는데, runner가 `.0`을 스스로 붙여 `0.42.83.0.0`이 되었다. 그래서 MakeAppx가 manifest schema 오류(`C00CE169`)로 `pack-baseline`에서 멈췄다. 이 실패는 설치 전 단계라 host mutation이 없었다(`host_mutation_performed=false`, 패키지와 smoke 서비스 없음). 세 자리 버전(`0.42.83`, `0.42.84`)으로 새 root `-r2`에서 다시 실행했다.

r2 실행 시간은 2026-09-30 `06:15:38Z`부터 `06:15:57Z`까지다. 인증서는 새로 만들지 않았다. MSI 제품 `PureCVisorDesktopNode`와 product manifest는 바꾸지 않았다.

| 단계 | exit / 상태 |
| --- | --- |
| pack baseline | `0` |
| sign/verify baseline | `0` / `0` |
| pack target | `0` |
| sign/verify target | `0` / `0` |
| install `0.42.83.0` | 통과 |
| update `0.42.84.0` | 통과 |
| remove | `PASS` (cleanup probe `PASS`) |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| smoke package | 없음 |
| `PureCVisorDesktopNodeMsixSmoke` | 없음 |
| MSI 서비스 `PureCVisorDesktopNode` | Running / Automatic |
| product manifest | `0.42.84-admin-smoke` (변경 없음) |

## 아직 닫히지 않은 후속

| 후속 검증 | 결과 |
| --- | --- |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.83-admin-smoke` |

## Nonclaims

- 내부 root-leaf 서명이다. public trusted signing이 아니다.
- operational current는 `0.42.83-admin-smoke`다.
- external stable publication을 주장하지 않는다.
