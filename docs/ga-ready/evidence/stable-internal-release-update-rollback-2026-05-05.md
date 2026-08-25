# Stable Internal Release/Update/Rollback Evidence - 2026-05-05

evidence_id: stable-internal-release-update-rollback-2026-05-05
created_at: 2026-05-05T02:08:00+09:00
source_commit_sha: d5fce841a9e7d969a2abc531c4b7f6c8b3b39468
artifact_root: artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353
release_version: 0.35.2
update_payload_version: 0.35.3
rollback_target_version: 0.35.2
trust_model: InternalEnterprise
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 opt-in 범위에서 public trusted signing을 제외하고 ADR-0003 internal Root/leaf 기반 stable `RequireSigned` MSI build, install/repair/uninstall/remove-data, product-wrapper local payload update, rollback restore를 실행했다.

이 evidence는 내부 신뢰 기반 release/update/rollback 실행 증거다. Public CA 기반 Authenticode 또는 외부 stable publication claim으로 해석하지 않는다. 제품 런타임 승격 판단은 이 evidence 단독이 아니라 2026-05-05 aggregate closure와 ADR-0004가 소유한다.

## 빌드 산출물

- `0.35.2` MSI: `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353/msi-build-0.35.2/PureCVisorDesktopNode-0.35.2-windows-x64.msi`
- `0.35.2` SHA-256: `7d9cf1f7ed157027ff128c3fadfa8fd82576d86166f6a214ac52c7190191e959`
- `0.35.3` MSI: `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353/msi-build-0.35.3/PureCVisorDesktopNode-0.35.3-windows-x64.msi`
- `0.35.3` SHA-256: `25942dea9fb0476bc8648acfae6cb09f1194b6366bb60d4bdf23f2c488e2d8de`
- Authenticode status: `Valid`
- SignTool verify exit code: `0`
- Signer subject: `CN=PureCVisor Desktop Node Internal Code Signing`
- Signer issuer: `CN=PureCVisor Internal Code Signing Root CA`
- Signer thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- Payload manifest: `schema_version=1`, `service_host_mode=dotnet-windows-service`, `update_installed_manifest_is_source_of_truth=true`

상세 signature/payload 검증은 `signature-and-payload-verify.json`에 있다.

## 실행 결과

- 실행 evidence: `stable-release-update-rollback-evidence.json`
- 단계 수: 26
- 전체 단계 결과: pass
- Stable MSI install `0.35.2`: pass
- Stable MSI repair `0.35.2`: pass
- Stable MSI uninstall preserve-data: pass
- Stable MSI `REMOVE_DATA=1` uninstall: pass
- Final stable reinstall `0.35.2`: pass
- Product-wrapper update to signed payload `0.35.3`: pass
- Product-wrapper rollback to `0.35.2`: pass
- Diagnostics collection after rollback: pass

## Final State Proof

- Final service: `PureCVisorDesktopNode` `Running`, `Auto`
- Final listener: loopback `http://127.0.0.1:7777/`
- Final active product version: `0.35.2`
- Final failed root version: `0.35.3`
- Previous root: absent
- Failed root: preserved for diagnostics at `C:\Program Files\PureCVisor\DesktopNode.failed`
- Active product root legacy WinSW root files: none
- Runtime policy endpoint: HTTP `200`
- Host status endpoint: HTTP `200`
- Web root endpoint: HTTP `200`
- Diagnostics bundle: `C:\ProgramData\PureCVisor\desktop-node\diagnostics\bundle-20260504-170543-34471963`
- Boot time unchanged: pass

## 판정

Stable internal release/update/rollback gate는 pass다. 이 pass는 GA-ready aggregate closure의 release/update/rollback execution blocker를 닫지만, public trusted signing 또는 외부 stable publication claim은 계속 제외한다.
