# Lifecycle/Packaging 기준 재설정 - 2026-05-10 0.41.5 to 0.41.6

```text
evidence_id: lifecycle-packaging-rebaseline-2026-05-10-0415-0416
artifact_root: artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416
summary: artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/summary.json
status: pass
baseline_version: 0.41.5-admin-smoke
target_version: 0.41.6-admin-smoke
baseline_msi_sha256: add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6
target_msi_sha256: 967ac29bf2928f1fec3a0bb72425d15d2eda65a2466b1cb29dd9183bb18928a3
update_package_sha256: 4e54c19ca6e6a9beec506613d66220c8b0bbbb579d0926d1d840f2cde7592161
installed_product_update_rollback: pass
internal_clean_host_install_update_rollback_smoke: pass
clean_host_windows_update_ubr: 5020
clean_host_final_web_status_code: 200
clean_host_final_api_unauthenticated_status_code: 401
failed_root_manifest_version: 0.41.6-admin-smoke
host_mutation_performed: true
guest_product_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

## Package Pair

이 rebaseline은 origin/main clean branch 상태에서 현재 내부 package pair를 생성했다.

- Baseline MSI: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415/PureCVisorDesktopNode-0.41.5-admin-smoke-windows-x64.msi`
- Target MSI: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/PureCVisorDesktopNode-0.41.6-admin-smoke-windows-x64.msi`
- Update catalog: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/update-catalog-0.41.6-admin-smoke.json`
- Package summary: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/package-pair-summary.json`

## 설치본 Product Update/Rollback

Installed product update/rollback은 `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/product-update-rollback`에서 PASS했다.

- Update exit code `0`
- Rollback exit code `0`
- Pre manifest `0.41.5-admin-smoke`
- Updated manifest `0.41.6-admin-smoke`
- Final manifest `0.41.5-admin-smoke`
- Product wrapper health `200` with protected bearer token source
- Web Console `http://127.0.0.1/` status `200`
- Web API unauthenticated runtime policy `http://127.0.0.1:7777/api/v1/runtime/policy` status `401`
- Failed root preserved with manifest `0.41.6-admin-smoke`
- Boot time unchanged

## 내부 Clean Host

Internal clean-host install/update/rollback은 Windows Update로 guest image가 UBR `5020`까지 올라간 dedicated Hyper-V clean host에서 PASS했다.

- The first clean-host attempt without Windows Update failed before current payload startup because the base VHD UBR `169` did not fully support CET for the current .NET payload.
- The Windows-updated attempt installed KB5082142 and confirmed baseline MSI install exit `0`, but the old runner still checked the API listener root `http://127.0.0.1:7777/` as if it were the Web Console. With the Web/API split, the unauthenticated API boundary returns `401` there.
- The preserved updated clean host was resumed in `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/internal-clean-host-install-update-rollback-smoke-resumed`.
- The resumed run completed install, update, rollback with exit code `0`, final service `Running`, final Web Console status `200`, final API unauthenticated boundary `401`, and final manifest `0.41.5-admin-smoke`.
- The rollback preserved a failed root with manifest `0.41.6-admin-smoke`.
- The dedicated VM and VM root were removed after evidence collection.

## Runner 갱신

`packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`는 이제 baseline, updated, final phase에서 Web Console health를 `http://127.0.0.1/`로 확인한다. API listener는 `http://127.0.0.1:7777/`에 남아 있고, 여기서 unauthenticated runtime policy가 `401`을 반환하는 것이 기대되는 protected boundary다.

## 경계

이 문서는 ADR-0006 private network distribution path의 내부 administrator opt-in Lifecycle/Packaging evidence다. Public trusted signing, trusted timestamping, winget submission, external stable publication, public catalog upload, public stable installer URL, public clean-host signed install/update/rollback evidence가 아니다.
