# MANUAL-ADMIN 캠페인 2026-05-16 04221->04222 Burn Blocked

```text
evidence_id: manual-admin-campaign-2026-05-16-04221-04222-burn-blocked
result: BLOCKED_BY_BURN_CREDENTIAL_MANAGER_IDEMPOTENCE
package_pair: 0.42.21-admin-smoke -> 0.42.22-admin-smoke
baseline_version: 0.42.21-admin-smoke
target_version: 0.42.22-admin-smoke
host_mutation_performed: true
superseded_by: manual-admin-campaign-2026-05-16-04222-04223
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.21-admin-smoke -> 0.42.22-admin-smoke` package-pair 실행 중
Burn bootstrapper install이 exit `1603`으로 막힌 이력을 보존한다. Installed product
update/rollback과 installed runtime ops summary는 PASS였지만, Burn install 단계에서
MSI custom action `CredentialManagerDefaultTransition`이 이미
`--api-token-credential-target` 상태인 서비스를 재실행 가능한 완료 상태로 인정하지 못해
campaign 전체를 닫지 않는다.

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260516-04221-04222` |
| product update/rollback summary | `artifacts/manual-admin-campaign-20260516-04221-04222/lifecycle/product-update-rollback/summary.json` |
| Burn summary | `artifacts/manual-admin-campaign-20260516-04221-04222/burn-bootstrapper-lifecycle/summary.json` |
| MSI log | `artifacts/manual-admin-campaign-20260516-04221-04222/burn-bootstrapper-lifecycle/burn-install_000_PureCVisorDesktopNodeMsi.log` |
| baseline MSI SHA-256 | `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b` |
| target MSI SHA-256 | `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3` |
| update ZIP SHA-256 | `491b0a84bcbe75bf0a59d098b5385eb18f1b599c9ea919ea6f647b8e68f46c2e` |

## 보존 판단

- `0.42.21 -> 0.42.22`는 closed PASS package-pair가 아니다.
- blocker는 `0.42.23-admin-smoke`의 Credential Manager idempotence fix와
  `0.42.22 -> 0.42.23` closed campaign으로 해소했다.
- 이 evidence는 regression history로 보존하며, current package-pair claim은
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`가 소유한다.
- Public trusted signing, 외부 stable publication, winget submission, public stable
  installer URL은 `not-claimed`다.
