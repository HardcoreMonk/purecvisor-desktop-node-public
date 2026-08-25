# Admin smoke package 2026-05-28 0.42.56

evidence_id: `admin-smoke-package-2026-05-28-04256`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-manual-admin-next-package-pair-operator-surface-payload`
version: `0.42.56-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260528-04256`
provenance: `artifacts/admin-smoke-package-20260528-04256/PureCVisorDesktopNode-0.42.56-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260528-04256/PureCVisorDesktopNode-0.42.56-admin-smoke-windows-x64.msi`
msi_sha256: `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`
payload_aggregate_sha256: `5670772a193c996fadc0dbe1a9e45ec0ab908bd124092d1a328c22b5e0c7e699`
provenance_commit: `5594adc55b013a2bf3ade9c6ae7171ca37bdbeb0`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.56-admin-smoke` package는 Runtime/API ops summary의
`current_evidence.manual_admin.next_package_pair` projection과 Web/TUI/CLI current-card의
manual-admin next package-pair 표시를 설치본 payload로 승격한다. 이전 `0.42.55-admin-smoke`
Guest Execution provider, running cancel affordance, actual credentialed Windows guest execution
capability는 predecessor로 유지한다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `09bc89f0f3660dc12845629013c7fa2f3a4cd9b1ef3437e1073fac3e3011736d` |
| PCVCLI EXE | `91d54c317ac726db36a49170f22474c7182132e4a3379fb52ce436e4640d5958` |
| PCVTUI EXE | `77c25f3d306e851fdb226b5b0e77b24721e684dd2d6632385ce5cb375f20eb4d` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.56-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260528-04256 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
