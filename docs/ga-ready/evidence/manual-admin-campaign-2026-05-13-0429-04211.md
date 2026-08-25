# MANUAL-ADMIN 캠페인 2026-05-13 0429→04211

```text
evidence_id: manual-admin-campaign-2026-05-13-0429-04211
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.9-admin-smoke
target_version: 0.42.11-admin-smoke
skipped_target_version: 0.42.10-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.9-admin-smoke -> 0.42.11-admin-smoke` package-pair를
manual-admin 운영 단위로 재검증한다. Installed update/rollback, wrapper repair
`BatchEvidenceRoot` 재설정, dedicated clean-host install/update/rollback, Burn
install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary
capture, manual-admin descriptor generation을 모두 PASS로 닫았다.

`0.42.10-admin-smoke`는 중간 조사 대상으로 보존한다. 해당 package는 wrapper
`RepairInstalled -BatchEvidenceRoot`가 SCM `PathName`을 갱신했지만 native
service-action이 이미 서비스를 시작한 뒤 outer wrapper가 다시 `sc.exe start`를
호출해 `1056 already running`을 반환했다. 이 duplicate start는
`0.42.11-admin-smoke`에서 outer start skip으로 수정됐다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260513-0429` |
| baseline MSI | `PureCVisorDesktopNode-0.42.9-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb` |
| target package root | `artifacts/admin-smoke-package-20260513-04211` |
| target MSI | `PureCVisorDesktopNode-0.42.11-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1` |
| target provenance commit | `987beb51025a5aa926df7d9a905019b4d6d29705` |
| signing mode | `AllowUnsignedDev` |
| update package | `artifacts/manual-admin-campaign-20260513-0429-04211/lifecycle/PureCVisorDesktopNode-0.42.11-admin-smoke-update.zip` |
| update package SHA-256 | `734114e0ea7c9d486a1d329cd551a6abc34d20f3801a944bd5dbcb8c1c4a9991` |

## Campaign Descriptor 산출물

- Descriptor root:
  `artifacts/manual-admin-campaign-20260513-0429-04211/manual-admin-campaign-descriptor-supervised`
- Summary: `overall_status=pass`, `runner_count=6`, `missing_count=0`,
  `not_pass_count=0`
- Batch manifest:
  `artifacts/batch-runs/manual-admin-campaign-descriptor-20260513-0429-04211/manifest.json`
- Batch summary: `ok=true`, `status=completed`, `executed_steps=1`

## 설치본 Update/Rollback

Installed-host product update/rollback은 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260513-0429-04211/lifecycle/product-update-rollback`
- Update PASS:
  `02-update.json`
- Wrapper repair PASS:
  `04-wrapper-repair-installed-batch-root.json`
- Rollback PASS:
  `05-rollback.json`
- Direct native repair after rollback PASS:
  `07-direct-native-repair-after-rollback.json`
- Summary:
  `summary.json`

Update action은 `Update`, rollback action은 `Rollback`이며 둘 다 `ok=true`다.
Wrapper repair 이후 service `PathName`은 campaign root를 포함했고, rollback 이후
direct native repair는 canonical `artifacts` root를 다시 적용했다.

## Dedicated Clean-host 검증

Dedicated clean-host install/update/rollback은 Windows Update 적용 후 PASS다.

- PASS artifact root:
  `artifacts/manual-admin-campaign-20260513-0429-04211/clean-host-updated-os`
- Result: `internal_clean_host_install_update_rollback_smoke=pass`
- Baseline manifest: `0.42.9-admin-smoke`
- Updated manifest: `0.42.11-admin-smoke`
- Final manifest: `0.42.9-admin-smoke`
- Install/update/rollback exit code: `0` / `0` / `0`
- Final Web Console: HTTP `200`
- Blocker: `none`
- Windows Update preparation: `ok=true`, `reboot_performed=true`

Windows Update 없이 실행한 첫 clean-host attempt는 MSI custom action에서
`.NET` CET 지원 오류로 실패했다. Debug VM에서
`Your Windows doesn't fully support CET. Please install all available Windows updates.`
fatal message를 확인했고, 같은 campaign을 `-InstallWindowsUpdates`로 재실행해 PASS로
닫았다. 실패 attempt와 debug artifact는 원인 보존용이며 current PASS claim은
`clean-host-updated-os`가 소유한다.

## Burn Bootstrapper 검증

Burn bootstrapper lifecycle은 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260513-0429-04211/burn-bootstrapper-lifecycle`
- Bundle version: `0.42.11.0`
- Bundle SHA-256:
  `ce8dc410e0ce9ca00ace0d929f29f75c1d47c1f31beeae03591a0eb7aa2f4d3a`
- Install exit code: `0`
- Repair exit code: `0`
- Remove exit code: `0`
- Baseline restore exit code: `0`
- Final restore manifest: `0.42.9-admin-smoke`

## MSIX Lifecycle 검증

MSIX internal package lifecycle은 PASS다.

- Artifact root: `artifacts/msix-package-lifecycle-smoke-20260513-0429-04211`
- Result: `build-install-update-remove-pass-internal-smoke`
- v1 package SHA-256:
  `33891a67c7c47caccd65289b1933d486476cce7cea39fda483aaf33237fad28b`
- v2 package SHA-256:
  `7169222bdbfffc3e44cccdedfe0988be13eef57a12656bfde8ffb6c47c5e02d8`
- Final MSIX package/service absence: PASS
- Existing MSI service final state: `Running`

이 MSIX evidence는 internal smoke이며 public trusted signing 또는 external stable
publication evidence가 아니다.

## 설치본 Runtime Ops Summary

Installed runtime ops summary capture는 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260513-0429-04211/installed-runtime-ops-summary`
- Installed manifest: `0.42.9-admin-smoke`
- Service state: `Running`
- Service path: `--batch-evidence-root` 포함
- Batch evidence root:
  `D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts`
- `batch_evidence.status`: `available`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260513-040213-0429`
- Errors: `0`

이 capture는 package-pair campaign 시점의 latest full gate가 0429임을 보여준다.
후속 04211 full gate와 post-gate current-card는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md`가
소유한다.

## 판정

`0.42.9 -> 0.42.11` package-pair는 installed update/rollback, wrapper repair,
dedicated clean-host, Burn, MSIX, installed runtime summary, descriptor 기준으로 PASS다.

이 evidence는 internal/admin-smoke host mutation evidence다. Public trusted signing,
winget, external stable publication, public clean-host release claim은 추가하지 않는다.
