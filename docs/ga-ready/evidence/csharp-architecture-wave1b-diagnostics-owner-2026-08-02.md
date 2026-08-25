# C# architecture improvement Wave 1B diagnostics owner evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Verification source commit: `bfced2701f0e1c682b26c3c52d735601fcd33c8b`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `M / Full`
- Evidence type: code-level behavior-preserving diagnostics ownership move
- Implementation status: `code_complete` (Wave 1B only; Wave 1C/1D remain open)
- Promotion status: `promotion_not_triggered`
- Product source changed: `true`
- Product behavior changed: `false`
- `host_mutation_performed`: `false`
- Package build performed: `false`
- Installed product changed: `false`
- Package/current operational evidence anchor changed: `false`
- Operational anchor: `0.42.65-admin-smoke` carry-forward
- Actual-VM/admin smoke: not run; not required for this code-level `M / Full` slice
- `public_trusted_signing`: `false`
- `external_stable_publication`: `false`

This record closes Wave 1B only. It moves diagnostic bundle creation, listing, download, redaction,
retention and pagination out of `DesktopNodeApiRequestProcessor` while preserving the public API
façade and current global request serialization. It does not introduce ASP.NET Core; the product
HTTP transport remains the existing `System.Net.HttpListener` path. Wave 1C auth/session/RBAC and
Wave 1D ops dispatch are not claimed complete by this evidence.

## Delete-test decision and implemented owner

### Wrapper evaluation

The former `DesktopNodeApiDiagnosticsHandler` was a static three-callback pass-through. It matched
the diagnostics route and delegated all behavior back to processor methods. A deletion evaluation
showed that deleting it would merely return the route `if` statements to the processor, while
merging it into another dispatcher would preserve the same callback indirection. Wave 1B therefore
selected `deepen`:

- commit `c8cea02` replaces the callback wrapper with one instance
  `DesktopNodeApiDiagnosticsHandler`;
- the owner receives `DesktopNodeDiagnosticBundleOptions` once and directly owns create, list and
  download behavior;
- the owner also owns diagnostic file I/O, bundle-ID/path containment, request-body redaction,
  age/count retention, list pagination, response JSON and download headers;
- the processor retains one owner field and a single
  `diagnosticsHandler.TryHandle(request, method, normalizedPath)` call;
- ops-summary receives only the owner's read-only `DiagnosticsRoot` projection.

The public `DesktopNodeDiagnosticBundleOptions` contract moved to the owner file without changing
its namespace, constructor shape, defaults or host call sites. No new project reference or runtime
package was added.

### Compiled ownership guard

Commit `ca9e013` adds owner-direct behavior tests and an initial compiled guard. Commit `91b345b`
hardens it into a private-reflection-free `System.Reflection.Metadata`/PEReader IL guard. The guard
checks all field visibilities and static/instance state, handler constructors and declared method
parameters, and processor declared-method call instructions. It proves that:

- the processor holds exactly one concrete diagnostics owner and no diagnostics options or delegate
  field;
- the retired create/list/download, redaction, retention/pagination helpers and nested page/row
  types are absent from the processor;
- the handler owns exactly one non-literal state field, the diagnostics options;
- the handler has no callback/delegate or processor field, constructor argument or method argument;
- processor methods make no direct `System.IO` calls;
- `HandleCore` contains exactly one direct call to the callback-free three-argument `TryHandle`.

The guard reads compiled metadata/IL and does not inspect production source text.

Commit `bfced27` stabilizes the pre-existing route-timeout cancellation assertion by waiting until
the asynchronous route scope is disposed and asserting against a trace snapshot. This removes a
test-observer race found during the final three-run API repeat; it does not change production code
or timeout/cancellation behavior.

## Preserved behavior

The owner move preserves the following established contracts:

- three diagnostics routes, required permissions and authoritative route registry entries;
- 200/201/400/404/409 status codes, JSON keys, content types, download body and headers;
- `PCV_DIAGNOSTIC_BUNDLE_ROOT_NOT_CONFIGURED`, pagination errors, invalid ID and missing bundle
  error codes with `retryable=false`;
- original `request.Path` query parsing for direct processor requests;
- the existing route-parameter decode followed by diagnostics bundle-ID decode;
- file-name descending order, age/count retention and list-triggered cleanup;
- token/secret/password/authorization property masking, recursive JSON traversal, Bearer string
  masking, missing-body projection and malformed-JSON replacement;
- processor-level JSON request-ID attachment and archive request-ID capture;
- auth/RBAC checks before diagnostics dispatch, GET timeout handling and the outer global `sync`
  serialization lock;
- the TypeScript Web Console, PCVCLI, Web/API port split and current `HttpListener` transport.

No I/O exception was newly caught or translated, and no shadow execution, alternate diagnostics
backend or request retry was introduced.

## Verification

The deterministic .NET source snapshot for verification commit `bfced27` is
`5ce1e17f01c8d902d3ba09e06686853528d4742e556486486fab7c9bfaba8e8d`. Raw TRX and Cobertura
results are stored under
`artifacts/dotnet-quality-wave1b-diagnostics-owner-20260802-final-r3`.

| Verification | Result |
|---|---|
| Solution build | PASS, warnings 0, errors 0, `-warnaserror` |
| Diagnostics/architecture targeted tests | PASS, 22/22 |
| API tests | PASS, 196/196; three repeated runs |
| Runtime tests | PASS, 42/42, skip 0 |
| Contracts tests | PASS, 15/15, skip 0 |
| Host tests | PASS, 161/161, skip 0 |
| Full .NET solution | PASS, 659/659, skip 0 |
| Quality capture/ratchet | PASS, 659 total, skip 0, line `50.619499%`, branch `41.03664%`, removed-test mapping 0 |
| Diagnostics owner scoped coverage | PASS, line `286/299` (`95.652174%`), branch `76/89` (`85.393258%`) |
| 54-route/API JSON/request serialization regression | PASS through API, Host and Full verification |
| Independent diff review | PASS after P2 guard hardening; remaining actionable findings 0 |
| Development verification | PASS, Full/M, `ok=true` |
| `git diff --check` | PASS |

The Full/M summary is
`artifacts/development-verification-csharp-diagnostics-owner-wave1b-20260802-final-r3/summary.json`.
Its effective lane is `Full`, change tier is `M`, and all selected .NET, Web npm/parity, packaging
Pester, installer Pester, Web Pester, diff and current-evidence suites passed. The current-evidence
check also confirms that this code-level slice did not change generated operational anchors.

The current C# inventory is 80 product files / 24,765 physical LOC and 40 test files / 17,916
physical LOC. `DesktopNodeApiRequestProcessor.cs` decreased from 2,972 to 2,558 lines. The new
diagnostics owner is 536 lines behind a callback-free three-argument dispatch interface.

## Deferred security-semantic follow-up

Wave 1B deliberately preserves the existing diagnostics redaction semantics. The current matcher
covers token/secret/password/authorization property names and Bearer strings. Whether diagnostics
must additionally mask broader key/credential/cookie names, JWT/private-key shapes or other
high-entropy strings is a security behavior decision. It must be characterized and implemented in
a separately classified security slice rather than silently added to this ownership move.

## Closure and next boundary

- Wave 1B is `code_complete`; no package candidate was produced, so promotion is
  `promotion_not_triggered`.
- `0.42.65-admin-smoke`, Web Console/PCVCLI, TypeScript static assets and the internal/private
  network boundary carry forward unchanged.
- Wave 1C auth/session/RBAC remains a separate minimum `L / Release` slice.
- Wave 1D ops dispatch remains a separate `M / Full` slice; `BatchEvidenceSummaryReader` internals
  remain assigned to Wave 7.
- The GA-ready evidence index, current-evidence ledger/JSON/schema, ADR index, verification policy,
  package/fullgate/current-card, signing claim and publication claim are intentionally unchanged.
