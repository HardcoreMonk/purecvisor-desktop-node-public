# Admin-smoke package 2026-05-16 0.42.25

```text
evidence_id: admin-smoke-package-2026-05-16-04225
result: PASS
scope: internal-admin-smoke-package-build
version: 0.42.25-admin-smoke
package_build_decision: executed-0.42.25-admin-smoke
artifact_root: artifacts/admin-smoke-package-20260516-04225
msi_sha256: 5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06
provenance_commit: 403d4474c4b88136774600cc81ca2d941c0b5e4b
signing_mode: AllowUnsignedDev
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 Runtime/API `current_evidence` artifact discovery와 Web Console
current-card parity 보강을 포함한 `0.42.25-admin-smoke` internal admin-smoke MSI
package build를 기록한다. 이 package는 product payload 후보이며, full admin host
mutation PASS나 installed Web/TUI/CLI current-card PASS를 claim하지 않는다.

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260516-04225/PureCVisorDesktopNode-0.42.25-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06` |
| provenance | `artifacts/admin-smoke-package-20260516-04225/PureCVisorDesktopNode-0.42.25-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260516-04225/PureCVisorDesktopNode-0.42.25-admin-smoke-windows-x64.publication.json` |
| payload aggregate SHA-256 | `c7940a98832013faff8fff096df1be9392e9a13fbb222077d1317bd5745f340a` |
| build UTC | `2026-05-16T09:56:13.1082739Z` |

## 결정

`0.42.25-admin-smoke`는 product payload package candidate record다. 후속 full admin
host mutation과 manual-admin package-pair closure는 같은 version의 operational
package root `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04225`를
사용했다. 이 anchor는 `0.42.26-admin-smoke` 이후 historical predecessor이며, 당시
operational anchor는 `0.42.25-admin-smoke` /
`full-admin-host-mutation-gate-20260516-04225`였다. `0.42.24-admin-smoke ->
0.42.25-admin-smoke` Manual-admin package-pair는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`에서
`missing_count=0`, `not_pass_count=0`, `overall_status=pass`로 닫혔다.
