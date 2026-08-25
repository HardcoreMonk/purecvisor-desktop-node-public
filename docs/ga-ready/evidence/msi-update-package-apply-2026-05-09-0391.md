# 0.39.1 MSI/Update Package Apply Evidence

evidence_id: msi-update-package-apply-2026-05-09-0391
created_at: 2026-05-09T00:30:00+09:00
scope: msi-package-build-update-zip-catalog-installed-apply
result: PASS
artifact_root: artifacts/msi-update-package-20260509-0391
admin_smoke_version: 0.39.1-admin-smoke
source_commit_sha: 8f0c4b6fbac8787932d0e966437fcc62d86e6068
admin_msi_sha256: 9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914
update_zip_sha256: d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5
admin_signing_mode: AllowUnsignedDev
host_mutation_performed: true
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass

This evidence records the elevated MSI apply of the `0.39.1-admin-smoke` package built from commit `8f0c4b6fbac8787932d0e966437fcc62d86e6068`.

This evidence is internal `AllowUnsignedDev` admin-smoke evidence. It is not public trusted signing evidence and is not external stable publication evidence.

## Artifacts

- Artifact root: `artifacts/msi-update-package-20260509-0391`
- MSI: `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.msi`
- MSI SHA-256 sidecar: `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.msi.sha256`
- Provenance: `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.provenance.json`
- Publication descriptor: `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.publication.json`
- Update ZIP: `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-update.zip`
- Update ZIP SHA-256 sidecar: `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-update.zip.sha256`
- Update catalog: `artifacts/msi-update-package-20260509-0391/update-catalog-0.39.1-admin-smoke.json`
- Elevated MSI log: `artifacts/msi-update-package-20260509-0391/msi-apply-0.39.1-admin-smoke-elevated.log`

## Build And Package Result

The package build used:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.39.1-admin-smoke -OutputRoot artifacts/msi-update-package-20260509-0391 -SigningMode AllowUnsignedDev -WixPath "$env:USERPROFILE\.dotnet\tools\wix.exe"
```

Observed build result:

- Build result: `ok=true`
- Product version: `0.39.1-admin-smoke`
- MSI ProductVersion: `0.39.1`
- MSI SHA-256: `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`
- Provenance commit: `8f0c4b6fbac8787932d0e966437fcc62d86e6068`
- Payload aggregate SHA-256: `de7e0634659da802fe62ae5a202461af252c3c331a273e6ca71285dfdafc7882`
- Service host SHA-256: `8a88f50192efb63833d1a2ec310eaa5095a3f57a6ccdc5725517cf5ccb8a286b`
- Product wrapper SHA-256: `7dbd8cadb81b75044f9afdb14fdc0834e835a9db7bc9e8609d937e69fc948250`
- Signing mode: `AllowUnsignedDev`
- Publication descriptor mode: `internal-artifact-descriptor-only`
- Public trusted signing: `excluded`
- External stable publication: `not-claimed`

The generated update ZIP and catalog were validated before installed apply:

- Update ZIP SHA-256: `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`
- Catalog selected channel: `admin-smoke`
- Catalog selected version: `0.39.1-admin-smoke`
- Package source preflight: `ok=true`
- Payload validation: `ok=true`
- Payload validation version: `0.39.1-admin-smoke`
- Payload validation file count: `6`

## Apply Command

The elevated apply used:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File artifacts/msi-update-package-20260509-0391/Apply-0.39.1-admin-smoke.ps1
```

Observed apply result:

- MSI exit code: `0`
- Installed manifest version: `0.39.1-admin-smoke`
- Service `PureCVisorDesktopNode`: `Running`
- Loopback Web Console: HTTP `200`
- Installed manifest Web asset count: `3`
- Installed `web/styles.css` length: `15331`
- SCM `PathName` still includes `--diagnostics-root`, protected token file, route timeout, request limit, burst, and retry-after arguments.

The earlier non-elevated MSI apply attempt in the same artifact root ended with MSI exit `1603` and Windows Installer error `1730`, which confirmed the expected administrator elevation gate. That attempt did not update the installed product. The final elevated apply above is the PASS evidence.

## Verification

Fresh non-mutating verification after the elevated apply confirmed:

- Installed product manifest version: `0.39.1-admin-smoke`
- Installed service state: `Running`
- Loopback Web Console `http://127.0.0.1:7777/`: HTTP `200`
- Installer Pester suite: `41/41` passed
- Public descriptor/readiness Pester suite: `12/12` passed
- Update catalog/package/payload validation: PASS
- `git diff --check`: PASS

## Boundary

This evidence does not execute or claim:

- Full Service/MSI/Hyper-V route parity smoke
- Firewall mutation
- Trust-store mutation
- LAN listener mutation
- Event Log source registration/removal
- Diagnostic bundle installed listener create/download
- Public trusted signing
- External stable publication

Those scopes remain owned by their dedicated evidence documents. This record only proves that the `0.39.1-admin-smoke` MSI/update package was built, hash-validated, applied with administrator elevation, and left the installed loopback service running.
