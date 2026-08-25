# C# architecture Wave 2A job durability completion and legacy installed checkpoint (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Decision: `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`
- ADR: `docs/adr/0013-job-store-single-writer-transaction-lease.md`
- Gap registry: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Physical-writer predecessor: `4d3a0d9782ee5e40fc35df51f44a36bf04a15034`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `L / Release`
- Source status: `code_complete`
- Legacy installed checkpoint: `PASS` (`2026-08-03` post-reboot follow-up)
- Initial 2026-08-02 checkpoint status: `blocked-by-host-tcp-excluded-port-7777`
- Operational full-admin anchor changed: `false`
- Operational anchor carried forward: `0.42.65-admin-smoke` /
  `full-admin-host-mutation-gate-20260716-04265`
- ASP.NET Core introduced: `false`
- TypeScript Web Console replaced: `false`
- Public trusted signing: `false`
- External stable publication: `false`

The completion candidate passed the final source gate and was packaged from exact commit
`3c16f78568cfb54a0cbe586449a540df3596bcf1`. The initial 2026-08-02 installed attempt was blocked
because the host excluded the product's fixed API port 7777. After the approved host reboot removed
the covering range, the same final MSI passed the 2026-08-03 legacy installed checkpoint. The
supplemental PASS evidence is
`docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`.
This checkpoint authorizes MSI/service/installed-file mutation only. It does not authorize Hyper-V,
actual-VM, full-admin gate, explicit product update/rollback, repair, uninstall or remove-data
execution. The initial failed-install transaction's automatic MSI rollback is recorded separately
below as historical evidence.

## Completed behavior

Wave 2A keeps the existing schema v1/v2 JSON shape and `System.Net.HttpListener` transport while
closing the remaining durability paths:

1. create/start/cancel/complete compute a candidate, durably save it and only then publish live state;
2. a completion whose persistence outcome is uncertain blocks later mutation and never replays the
   provider automatically;
3. a persisted `running` job becomes `PCV_JOB_INTERRUPTED`, `retryable=false` only after the recovery
   snapshot is committed;
4. running cancel is committed before the out-of-lock provider cancellation signal, with requested,
   acknowledged, completed-before-cancel and signal-failed outcomes distinguished;
5. schema/root/UTF-8/job/queue/status/timestamp/parameter/state combinations are semantically
   validated and malformed authoritative state remains fail-closed without quarantine rewrite;
6. the current writer uses a fixed-volume GUID plus volume-relative path transaction mutex and
   loaded-base SHA-256/length CAS; UNC/device, non-fixed/network, SUBST, DOS 8.3, ADS, existing
   reparse points and hard-linked primary files are rejected;
7. Host JSONL/Event Log observations are bounded and redacted, and their write set is rejected before
   listener bind when it aliases or contains the job-store primary, marker or temp namespace;
8. store-write and cancel-signal attention clears only after the corresponding durable recovery while
   bounded history remains available through diagnostics and ops summary;
9. a blocked job store still permits read-only `host.status` and `vm.list` observation, while job
   mutation and provider dispatch stay blocked.

## Source verification

| Verification | Result |
|---|---|
| Full .NET solution | PASS, 795/795, skip 0 |
| Runtime owner | PASS, 120/120 |
| API | PASS, 228/228 |
| Host | PASS, 181/181 |
| Gap-registry Pester | PASS, 10/10 |
| Job-hardening contract Pester | PASS, 10/10 |
| Job-hardening dry-run | PASS, `ok=true`, `actual_execution=dry-run-no-http`, `host_mutation_performed=false` |
| Frozen 0.42.65 runner contract | PASS, 5/5 |
| Frozen 0.42.65 actual reader | PASS, 8/8, v1/v2 terminal+FIFO queue initial/restored |
| Release/L development verification | PASS, 7/7 suites, `ok=true` |

The job-hardening dry-run summary is
`artifacts/api-host-job-hardening-wave2a-completion-dryrun/summary.json`. The frozen binary summary is
`artifacts/job-store-04265-reader-compatibility-20260802-wave2a-current-writer-final5/summary.json`.
The final Release/L summary is
`artifacts/development-verification-csharp-wave2a-completion-final-r3/summary.json`; it records
`effective_lane=Release`, `change_tier=L`, all seven results `passed` and `failed_suite` empty.
The pinned Host SHA-256 is
`95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83` and its product version is
`0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876`. Both schema versions preserved
input, backup, restored and final hashes. Native operation requests were 0; service/admin/Hyper-V/host
mutation flags were false.

## Legacy installed checkpoint

### Post-reboot closure (2026-08-03)

The separately authorized post-reboot preflight confirmed that the former IPv4/IPv6 `7765-7864`
covering range was absent before installation. The same final `r2` MSI installed with exit 0. The
service is left `Running`/`Auto`/`LocalSystem`, the installed manifest reports
`0.42.66-admin-smoke`, Host and CLI hashes match the package, Web `/` and `/pcv-config.js` return
HTTP 200, unauthenticated API access returns 401/`PCV_AUTH_REQUIRED`, and protected-token PCVCLI
`runtime policy` exits 0. The installed listener hardening summary is `ok=true` with provider-free
read responses 200, missing-job cancel 404 and oversized body 413; rate-limit and route-timeout
probes were not run.

The authoritative ProgramData store remains version 1 with 18 jobs, queue 0, running 0 and SHA-256
`78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b`; no pending/temp/corrupt
sidecar exists. Hyper-V/VM/provider, full-admin, update/rollback, repair, uninstall and remove-data
execution remained excluded. The checkpoint artifact is
`artifacts/legacy-installed-checkpoint-20260803-04266-postreboot/summary.json` and the final service is
intentionally left installed/running. This PASS does not promote or replace the current
`0.42.65-admin-smoke` operational full-admin anchor.

### Final package candidate

- Wave 2A source commit: `f3d5d7be4bb24b80fc2fa11be1cee93be13b4362`
- Single-file Event Log path fix commit: `3c16f78568cfb54a0cbe586449a540df3596bcf1`
- Version: `0.42.66-admin-smoke`
- Artifact root: `artifacts/admin-smoke-package-20260802-04266-r2`
- Provenance commit: `3c16f78568cfb54a0cbe586449a540df3596bcf1`
- MSI SHA-256: `7249539f2c1c4d597fc73801a1de443bf791bcee13e0e13b3904c86435a83464`
- Payload aggregate SHA-256: `dc1c383666bd49d56e1113200ff20f89a394985fdb05bcc53afd1370e2d60eaf`
- Host SHA-256: `30486c897ba9126808e8ab46118c21c76380a4b081a5bb035e5f7c9b3b80dc64`
- CLI SHA-256: `5bf293d9ffbf2b7bd42153d4b4e8b03fb16666b50a0883b10ea4a5e64bc37747`
- Signing: unsigned, `AllowUnsignedDev` / `LocalTest`
- Publication: internal artifact descriptor only; no public trusted signing or external stable
  publication

The first package build exposed an IL3000 single-file warning in the new Event Log ownership fallback.
`3c16f78` replaced `Assembly.Location` with `Environment.ProcessPath` plus an
`AppContext.BaseDirectory\DesktopNode.Host.exe` fallback, added two tests, passed Host 181/181 and the
Release/L 7/7 gate, and rebuilt the final `r2` package without that warning. The earlier package was
not installed.

### Historical pre-reboot install attempts and root cause (2026-08-02)

Two fresh installs of the final `r2` MSI returned 1603. The verbose logs are
`artifacts/legacy-installed-checkpoint-20260802-04266-r2/install.log` and
`artifacts/legacy-installed-checkpoint-20260802-04266-r2/install-retry1.log`. Both reached the
`ConfigureInstalled` custom action, created and started the owned LocalSystem service, then failed
with MSI error 1722 after `HttpListenerException` native error 32. The service never reached the
required stable Running/API-health state, so Windows Installer rolled back product files and product
registration. In both attempts the automatic rollback left a stopped `PureCVisorDesktopNode`
service. Its exact `ImagePath` ownership was checked before the orphan service was manually deleted.
Both logs record `Error in rollback skipped. Return: 5`; failed-install service cleanup therefore
remains a packaging lifecycle defect rather than being attributed to successful MSI rollback.

The failure was reproduced outside MSI without using the product job store:

- current 0.42.66 with API 7777/Web 80: stopped, API/Web HTTP `000`;
- current 0.42.66 with API 7777/Web 57778: stopped, API/Web HTTP `000`;
- current 0.42.66 with API 57777/Web 80: Running, API/Web HTTP `200`;
- frozen 0.42.65 with API 57777/Web 80: Running, Web HTTP `200`;
- frozen 0.42.65 with API 7777/Web 80: stopped, API/Web HTTP `000`.

Therefore the failure is not a Wave 2A binary regression and not a Web port 80 conflict. On this
host, both IPv4 and IPv6 TCP excluded-port tables contain `7765-7864`, which includes fixed product
API port 7777. There was no active TCP/UDP endpoint, HTTP.sys URL group, NAT mapping or portproxy on
7777. The excluded range is the blocking host condition.

### Historical pre-reboot rollback and final state (2026-08-02)

- `host_mutation_performed=true`: MSI, exact-owner orphan service deletion and controlled diagnostic
  service create/start/stop/delete were executed.
- `hyperv_mutation_performed=false`; no VM, native provider, route-parity, full-admin, explicit
  product update/rollback, repair, uninstall or remove-data workflow was executed.
- Final Windows service: absent.
- Final product root and uninstall registration: absent.
- Final listeners on 80/7777: absent.
- ProgramData authoritative store: version 1, jobs 18, queue 0, running 0.
- Job-store pre/post SHA-256:
  `78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b` (unchanged).
- Pending marker and GUID temp count: 0; corrupt/quarantine sidecars: 0.
- Service was not left installed/running because the checkpoint did not pass.
- Failed-install rollback service cleanup is not claimed complete; the final absent state required
  exact-owner manual cleanup after each attempt.

At this historical boundary, completing the installed checkpoint required a separately authorized
reboot/host-network recovery or a product-wide API port migration. The approved reboot was completed,
the covering range was verified absent and the 2026-08-03 supplemental checkpoint passed. No direct
excluded-range deletion or product-wide port migration was performed. The successful later install
does not resolve the failed-install service cleanup gap; it should still be fixed and covered by an
installer failure-path test before the next package lifecycle promotion.

## Nonclaims and follow-up boundary

- The transaction mutex is not a process-lifetime lease and does not support mixed-version concurrent
  writers.
- Hostile local-administrator namespace TOCTOU or a post-validation alias/reparse creation is outside
  this boundary.
- Directory fsync, controller-cache/power-loss survival and Hyper-V side-effect exactly-once are not
  claimed.
- Wave 2B/2C operation-family reconciliation remains pending.
- This legacy checkpoint does not replace the current `0.42.65-admin-smoke` full-admin/actual-VM
  operational evidence. It is an internal, unsigned development checkpoint only.
