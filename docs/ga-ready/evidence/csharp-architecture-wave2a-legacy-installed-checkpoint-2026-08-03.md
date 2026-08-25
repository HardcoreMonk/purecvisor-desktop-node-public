# C# architecture Wave 2A legacy installed checkpoint post-reboot PASS (2026-08-03)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Durability decision: `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`
- Source-completion and pre-reboot predecessor:
  `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`
- Result: `PASS`
- Version: `0.42.66-admin-smoke`
- Checkpoint artifact:
  `artifacts/legacy-installed-checkpoint-20260803-04266-postreboot/summary.json`
- Host mutation performed: `true` (MSI install, service install/start and installed-file mutation)
- Hyper-V mutation performed: `false`
- Actual VM validation performed: `false`
- Native provider route executed: `false`
- Operational promotion performed: `false`
- Operational full-admin anchor changed: `false`
- Operational anchor carried forward: `0.42.65-admin-smoke` /
  `full-admin-host-mutation-gate-20260716-04265`
- Public trusted signing: `false`
- External stable publication: `false`
- ASP.NET Core introduced: `false`
- TypeScript Web Console replaced: `false`

This is the separately authorized legacy MSI/service/installed-file checkpoint that follows the
2026-08-02 pre-reboot blocker. It is not a full-admin host mutation gate, actual-VM validation,
manual-admin package-pair closure or public release promotion.

## Post-reboot preflight

The host boot time was `2026-08-02T22:02:02.5000000+09:00`. Before installation, both IPv4 and
IPv6 excluded-port tables no longer contained the former `7765-7864` range that covered API port
7777. There was no listener on 80 or 7777, no installed service, product root, uninstall
registration or Host process.

The authoritative ProgramData store was captured before installation:

- schema version 1;
- 18 jobs: 14 succeeded and 4 failed;
- queue 0 and running 0;
- SHA-256
  `78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b`;
- pending/temp/corrupt sidecar count 0.

After successful service start, the TCP exclusion display contains exact 80-80 and 7777-7777 rows
and HTTP service state contains active loopback request queues for both endpoints. These exact rows
were observed only with the active HTTP.sys listeners and are not the former covering-range blocker.
`Get-NetTCPConnection` therefore reports HTTP.sys/System PID 4 for the listeners; the Windows service
process is separately verified as PID 32892.

## Package identity and installation

| Field | Result |
|---|---|
| Package | `artifacts/admin-smoke-package-20260802-04266-r2/PureCVisorDesktopNode-0.42.66-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `7249539f2c1c4d597fc73801a1de443bf791bcee13e0e13b3904c86435a83464` |
| Payload aggregate SHA-256 | `dc1c383666bd49d56e1113200ff20f89a394985fdb05bcc53afd1370e2d60eaf` |
| Provenance commit | `3c16f78568cfb54a0cbe586449a540df3596bcf1` |
| Signing/channel | unsigned `AllowUnsignedDev` / `LocalTest` |
| MSI result | PASS, exit 0 / Windows Installer status 0 |
| MSI log | `artifacts/legacy-installed-checkpoint-20260803-04266-postreboot/install.log` |

The install used quiet per-machine MSI installation with restart suppression. Repair, explicit
update/rollback, uninstall and remove-data were not executed. The log records product version
`0.42.66`, successful installation status 0 and `MainEngineThread is returning 0`.

## Installed product and operator surfaces

| Verification | Result |
|---|---|
| Windows service | PASS, `PureCVisorDesktopNode`, `Running`, `Auto`, `LocalSystem`, exit code 0 |
| SCM owner/config | PASS, expected installed Host, API 7777, Web 80, Credential Manager target, no raw token argument |
| Product manifest | PASS, schema 2, `0.42.66-admin-smoke`, `dotnet-windows-service`, `dotnet-local-api-client` |
| Installed Host | PASS, SHA-256 `30486c897ba9126808e8ab46118c21c76380a4b081a5bb035e5f7c9b3b80dc64` |
| Installed PCVCLI | PASS, SHA-256 `5bf293d9ffbf2b7bd42153d4b4e8b03fb16666b50a0883b10ea4a5e64bc37747` |
| TUI | PASS, absent from manifest and installed product root |
| Uninstall registration | PASS, exactly one entry, display version `0.42.66`, product code `{97446CAE-C7C3-4544-A32C-6E16E9DD358E}` |
| Product `Status` action | PASS, service query and manifest present |
| Web `/` | PASS, HTTP 200 |
| Web `/pcv-config.js` | PASS, HTTP 200 and API authority `http://127.0.0.1:7777` |
| API unauthenticated `/api/v1/jobs` | PASS, HTTP 401 and `PCV_AUTH_REQUIRED` |
| PCVCLI `runtime policy` | PASS, exit 0 using the protected token file; no token value logged |

The CLI stdout/stderr records are
`artifacts/legacy-installed-checkpoint-20260803-04266-postreboot/pcvcli-runtime-policy.stdout.log`
and `pcvcli-runtime-policy.stderr.log`. `runtime policy` is a provider-free GET contract; no
`host status`, `network inventory`, `vm list`, `ops summary` or other native-provider command was
called.

The installed listener hardening summary is
`artifacts/legacy-installed-checkpoint-20260803-04266-postreboot/api-host-job-hardening/summary.json`.
It records `ok=true`, `actual_execution=installed-listener-readonly-http-smoke`, oversized login
body HTTP 413, runtime/jobs/diagnostics/console reads HTTP 200, missing-job cancel HTTP 404
`PCV_JOB_NOT_FOUND`, and the same Running service PID before and after. Rate-limit and controlled
route-timeout probes were not enabled. The smoke's own `host_mutation_performed=false` describes its
HTTP requests only; the encompassing MSI checkpoint correctly records `host_mutation_performed=true`.
The bearer token was supplied only through an elevated process environment, removed afterward and
was not observed in evidence (`token_value_observed=false`).

## ProgramData preservation and final state

The post-install/post-smoke authoritative store exactly matches the pre-install snapshot:

- schema version 1, 18 jobs, queue 0, running 0;
- 14 succeeded and 4 failed;
- SHA-256
  `78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b` unchanged;
- last write remains `2026-07-16T14:46:44.3567359Z`;
- pending/temp/corrupt sidecar count remains 0.

The final service is intentionally left installed, `Running` and `Automatic`; Web 80 and API 7777
remain active. No Hyper-V/VM mutation, provider read, actual-VM execution, full-admin gate, package
update/rollback, repair, uninstall or remove-data action was performed.

## Historical predecessor and remaining packaging gap

The two 2026-08-02 MSI 1603/1722 attempts remain historical evidence. On the pre-reboot host, the
IPv4/IPv6 `7765-7864` excluded range covered fixed API port 7777; current and frozen binaries both
failed at 7777 and succeeded with the diagnostic API port moved away from that range. Automatic MSI
rollback removed product files and registration but left an exactly owned stopped service that was
manually deleted after `ImagePath` verification.

This successful post-reboot install closes the host-port checkpoint blocker. It does not erase or
resolve the failed-install rollback service-cleanup defect. That packaging lifecycle gap still needs
an installer failure-path fix and test before a later package lifecycle promotion.

## Evidence closeout verification

| Verification | Result |
|---|---|
| C# architecture gap registry | PASS, 10/10 |
| Installed API hardening contract | PASS, 10/10 |
| Admin evidence documentation | PASS, 88/88 |
| Current-evidence generation | PASS, 5/5 |
| Current-evidence generator read-only check | PASS twice, 6/6 targets current |
| Packaging Pester with CI-style `GITHUB_SHA` injected | PASS, 448/448 |
| `git diff --check` | PASS |
| Independent checkpoint evidence review | PASS, no factual mismatch |

The archived `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
baseline was also audited. It reports 20/38 PASS because 18 test cases still assert superseded
2026-05 anchors/candidate/preflight/not-run wording or a pre-Wave-1B route-owner source layout. Verification ownership explicitly
excludes that component/archive baseline from the active required product gate; no current document
was reverted to stale claims to satisfy it.

## Nonclaims

- This checkpoint does not replace the `0.42.65-admin-smoke` full-admin, actual-VM or installed
  current-card operational anchor.
- It does not claim Hyper-V side-effect exactly-once, directory fsync/power-loss durability,
  mixed-version concurrent writer support or hostile local-admin namespace protection.
- It does not introduce ASP.NET Core or replace the TypeScript Web Console.
- It is an internal unsigned development checkpoint and does not establish public trusted signing
  or external stable publication.
