# Ops Summary Data Builder Package - 2026-05-13 0.42.12

```text
evidence_id: ops-summary-data-builder-package-2026-05-13-04212
scope: ops-summary-data-builder-split-package-build
result: PASS
product_version: 0.42.12-admin-smoke
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

## 판정

`0.42.12-admin-smoke`는 `DesktopNodeApiRequestProcessor` 밖으로 ops summary data
assembly 책임을 분리한 첫 package build다. Request processor는 route/job/runtime
policy 조합만 유지하고, host status, VM list, batch evidence read, diagnostics root
전달은 `DesktopNodeApiOpsSummaryDataBuilder`가 맡는다.

이 변경은 제품 payload 변경이므로 `0.42.11-admin-smoke -> 0.42.12-admin-smoke`
manual-admin package-pair 후보를 열고, full admin host mutation gate를 다시 실행했다.
후속 2026-05-14 manual-admin campaign에서 update/rollback, clean-host, Burn, MSIX,
installed runtime ops summary까지 PASS로 닫혔다.

## 산출물

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260513-04212` |
| MSI | `PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e` |
| provenance commit | `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3` |
| payload aggregate SHA-256 | `df90fa64b307d1cb9cf080ceb663c367103ce530fde0d860ba62f0c5fb993d7d` |
| product wrapper SHA-256 | `5ba0708413d863e356b166a69ab8e4ae43f26d9609d65b7a3b9cce13f6344c33` |
| service host SHA-256 | `dfe3f9beb83f12030502ed3b1ea63092797ef057f82e5caec775e3cb37490cb6` |
| CLI SHA-256 | `44e86065c29f32ecff9b2bff0a8f00d22c32274d263736f5c8b5eb515fe60a2c` |
| TUI SHA-256 | `bff88ed2deeb4bcb3da914544aa0daba0dfdd12a9a316d174e12b4edab583b6a` |
| current-card artifact | `artifacts/installed-batch-evidence-current-card-20260513-04212` |
| closed package-pair evidence | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md` |

## 회귀 테스트

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~OpsSummary"
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~OpsSummary" --no-restore
git diff --check
```

결과: `DesktopNode.Api.Tests` OpsSummary focused suite `15` passed.

## 릴리스 경계

이 evidence는 internal/admin-smoke package build evidence다. Public trusted signing,
external stable publication, winget submission, public stable URL, public clean-host
release claim을 추가하지 않는다.
