# Admin-smoke package 2026-05-16 0.42.26

```text
evidence_id: admin-smoke-package-2026-05-16-04226
result: PASS
scope: internal-admin-smoke-package-build
version: 0.42.26-admin-smoke
package_build_decision: executed-0.42.26-admin-smoke
artifact_root: artifacts/admin-smoke-package-20260516-04226
msi_sha256: aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685
provenance_commit: d6500c01c972cbc7ca1e290e51120181ceea1501
signing_mode: AllowUnsignedDev
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 selector guard가 포함된 `0.42.26-admin-smoke` internal admin-smoke
MSI package build를 기록한다. 이 package는 product payload 후보이며, full admin
host mutation PASS나 installed Web/TUI/CLI current-card PASS를 claim하지 않는다.

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260516-04226/PureCVisorDesktopNode-0.42.26-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685` |
| provenance | `artifacts/admin-smoke-package-20260516-04226/PureCVisorDesktopNode-0.42.26-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260516-04226/PureCVisorDesktopNode-0.42.26-admin-smoke-windows-x64.publication.json` |
| payload aggregate SHA-256 | `f9d838c7fa1ff59b7431d44d9d86760ae90b181ca29226ba4b531b90d943999f` |
| build UTC | `2026-05-16T13:21:43.2504552Z` |

## 결정

`0.42.26-admin-smoke`는 product payload package candidate record다. 같은 turn의
operational full admin host mutation은 별도 routeparity package root
`artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226`를 빌드해
MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`로
닫았다. 따라서 최신 operational anchor는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`가
소유한다.
