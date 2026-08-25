# Public Source Authority

## Authoritative repository

The authoritative source-integration target is:

```text
HardcoreMonk/purecvisor-desktop-node-public
```

It starts from a sanitized, parentless Git root. Previous private Git objects, refs, tags, provider logs,
artifacts, issues, packages, releases, and pull-request metadata are not part of this authority. The original
private archive remains historical and is not a source-integration or rollback remote for this repository.

## Rights boundary

The source is public for inspection under the repository's rights-reserved `LICENSE`. Public visibility does
not make this an open-source project and does not grant permission to reproduce, modify, redistribute,
sublicense, sell, or publish the source or derived binaries without separate written permission.

Security reports use the private route in `SECURITY.md`. Public issues must not contain credentials, personal
or host identity, observed private endpoints, or exploit details.

## Release and operational boundary

Source authority is distinct from trusted binary release authority. The current machine claims are:

```text
version=0.42.74-admin-smoke
promotion_eligible=false
public_trusted_signing=false
external_stable_publication=false
package_candidate_created=false
```

The saved-lifecycle actual-VM blocker remains open. Publishing this source root does not create a package,
installer, trusted signature, stable update channel, winget submission, or external support commitment.

## Integration and verification boundary

The empty-repository seed is the only direct `main` bootstrap. After main protection is installed, source
changes land through protected pull requests. The current four legacy checks remain protected until same-SHA
dual-run evidence passes and the approved Wave E cutover atomically replaces them with `dotnet`, `web`,
`delivery`, and `installer-policy`.

Legacy Pester files remain tracked as rollback/reference material. Approved public-safety comments and
runtime construction of synthetic fixture material preserve their assertion names, counts, and behavior.
Final Required CI—not every historical or optional workflow—targets Pester invocation `0` and non-admin
PowerShell invocation `0`.
