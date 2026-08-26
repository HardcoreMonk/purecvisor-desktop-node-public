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

The empty-repository seed was the only direct `main` bootstrap. Main is protected and later source changes
land through protected pull requests. PR #1 completed the Wave E cutover at merge
`d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`; its exact remote-main Development Gates run
`32901477892` passed. PR #2 completed the documentation closure at main
`6e2bdb93ce308b632c929e2c17f5550ac3845401`; Development Gates run `32904006595` passed.
The required contexts are exactly `dotnet`, `web`, `delivery`, and `installer-policy`.

Legacy Pester files remain tracked as rollback/reference material. Approved public-safety comments and
runtime construction of synthetic fixture material preserve their assertion names, counts, and behavior.
The 62-file / 627-contract migration ledger is `cutover / local pass / CI pass`. Required CI, not every
historical or optional workflow, has Pester invocation `0` and non-admin PowerShell process invocation `0`.
The non-required Public Boundary Contract also passed at main run `32904006619`, but that workflow and
legacy/manual/admin PowerShell remain residue. This is not a repository-wide PowerShell deletion claim.
