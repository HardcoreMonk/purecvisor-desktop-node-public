# C# architecture improvement Wave 1C auth/session/RBAC owner evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Wave 1C predecessor commit: `1fdd620d`
- Verification source commit: `1088d0851c6fe119525da75c1cef1fb923a1d4ae`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `L / Release`
- Evidence type: code-level behavior-preserving auth/session/RBAC ownership move
- Implementation status: `code_complete` (Wave 1C only; Wave 1D remains open)
- Promotion status: `promotion_not_triggered`
- Product source changed: `true`
- Product behavior changed: `false`
- `host_mutation_performed`: `false`
- Package build performed: `false`
- Installed product changed: `false`
- Package/current operational evidence anchor changed: `false`
- Operational anchor: `0.42.65-admin-smoke` carry-forward
- Actual-VM/admin smoke: not run; this slice produced no package candidate
- Installed account/browser/noVNC smoke: not run; stale rerun trigger is the next operator-surface
  account/noVNC product payload change
- `public_trusted_signing`: `false`
- `external_stable_publication`: `false`

This record closes Wave 1C only. It moves auth route dispatch, action result assembly, access-token
authorization, RBAC denial policy, runtime auth projection and guest audit actor resolution out of
`DesktopNodeApiRequestProcessor`. It preserves the public processor façade, existing account/JWT
service and the outer global request-serialization lock. It does not introduce ASP.NET Core; the
product HTTP transport remains `System.Net.HttpListener`.

## Delete-test decision and implemented owner

The former `DesktopNodeApiAuthSessionHandler` was a static one-callback pass-through. It checked the
auth route family and returned control to processor methods that repeated route checks and owned all
body parsing, login/refresh/logout/session/RBAC response assembly and authorization. Deleting it
would only return the same `if` statements to the processor. Wave 1C therefore selected `deepen`:

- commit `72096c8` fixes the pre-move auth/JWT/RBAC contract in tracked semantic JSON golden tests;
- commit `e798028` replaces the static callback wrapper with one instance
  `DesktopNodeApiAuthSessionHandler`;
- the owner receives `DesktopNodeAccountAuthOptions` once and holds one
  `DesktopNodeAccountAuthService`;
- the owner directly owns five auth routes, JSON validation, action/error response assembly,
  general route authorization, guest-execution denial, runtime auth policy and guest actor lookup;
- the processor retains one `private readonly` owner field and calls `TryHandle` then `Authorize`
  before the remaining route dispatch;
- commit `1088d08` exercises refresh, replay rejection, logout, RBAC and guest permission/redaction
  through the real Host listener so the L-slice coverage boundary includes transport integration.

The Host composition root still creates its separate account service for noVNC/transport validation.
Combining those instances would change transport and revoke-state boundaries, so it was not mixed
into this behavior-preserving owner move.

## Compiled ownership and lifetime guard

The `System.Reflection.Metadata`/PEReader guard verifies compiled metadata and IL without private
reflection or production source-text inspection. It proves that:

- the processor holds exactly one `private readonly`, non-static auth owner field and no account
  service, auth options or callback field;
- retired processor helpers `HandleAccountAuthRoute`, `EnforceAccountAuthorization`,
  `RequiredPermissionForRoute`, `AuthResult`, `AuthValidationFailure` and `ResolveActor` are absent;
- the owner is a sealed instance type with exactly one `private readonly`, non-static service state
  field and no processor/callback field or parameter;
- `TryHandle`, `Authorize`, `ResolveActor` and `CreateRuntimePolicy` expose callback-free signatures;
- `HandleCore` calls `TryHandle` before `Authorize`, and the processor makes no direct account service
  call.

Owner tests also prove that two processor instances do not share the in-memory refresh-revocation
set. A bounded concurrent refresh test produces exactly one success and one revoked-token response,
while the pre-existing deterministic serialization test continues to prove that the processor does
not run two handlers concurrently.

## Preserved security and compatibility semantics

The following current behavior is fixed by golden/direct/Host tests and was not silently changed:

- auth routes execute before general account authorization;
- route-family matching remains case-insensitive while the five canonical auth action branches
  remain case-sensitive, so a case-changed auth path retains its auth-specific 404;
- unknown routes still use the `read` authorization fallback before their eventual 404;
- runtime-policy routes with no required permission retain their bootstrap behavior;
- `exp <= now` is expired for both access and refresh tokens;
- refresh rotation revokes the old refresh JTI before active-account lookup;
- logout revokes only a supplied refresh token; the corresponding access token remains valid until
  expiry, invalid refresh input still returns the current success projection, and blank logout
  reports `refresh_token_revoked=false`;
- refresh revocation remains in-memory and processor-instance-local;
- JWT account, role and permissions remain issuance-time snapshot claims; access validation does not
  re-read current account state;
- role normalization, custom permission snapshots, admin wildcard matching and guest-execution
  special denial code/action remain unchanged;
- response status, JSON keys, request-ID attachment, content type and existing `PCV_*` codes remain
  unchanged;
- password, password hash, signing key, rejected bearer and refresh tokens are not echoed in error
  responses;
- Host service-token/noVNC precedence, TypeScript Web Console, PCVCLI and Web/API port split remain
  unchanged.

## Verification

The deterministic .NET source snapshot for verification source commit `1088d08` is
`0344f983ae18448afab928b898ceadb6efc25ff5be800ffd31b66dd03c86463c`. Raw TRX and Cobertura
results are stored under `artifacts/dotnet-quality-wave1c-auth-owner-20260802-final`.

| Verification | Result |
|---|---|
| Solution build | PASS, warnings 0, errors 0, `-warnaserror` |
| Auth characterization/owner/concurrency/architecture tests | PASS, 13/13 |
| API tests | PASS, 209/209; three repeated final-state runs |
| Runtime tests | PASS, 42/42, skip 0 |
| Contracts tests | PASS, 15/15, skip 0 |
| Host tests | PASS, 162/162, skip 0 |
| Full .NET solution | PASS, 673/673, skip 0 |
| Quality capture/ratchet | PASS, 673 total, skip 0, line `51.240143%`, branch `41.651865%`, removed-test mapping 0 |
| API project coverage | PASS, line `5015/9189` (`54.576124%`), branch `1676/3523` (`47.573091%`) |
| Auth owner scoped coverage | PASS, line `470/514` (`91.439689%`), branch `188/273` (`68.864469%`) |
| Host project coverage | PASS, line `6149/13651` (`45.044319%`), branch `1684/5130` (`32.826511%`) |
| 54-route/API JSON/request serialization regression | PASS through API, Host and full verification |
| Independent implementation/test reviews | PASS; remaining commit-blocking findings 0 |
| Development verification | PASS, Release/L, 7/7 suites, `ok=true` |
| `git diff --check` | PASS |

The Release/L summary is
`artifacts/development-verification-csharp-auth-owner-wave1c-20260802-final/summary.json`. It uses the
actual committed path list from `1fdd620d...HEAD`, classifies the evidence-index change as
`current-evidence-anchor`, and passes dotnet, Web npm, packaging Pester, installer Pester, Web
Pester, diff and current-evidence checks. This is a non-mutating development preflight and is not
package, installed-product or actual-VM evidence.

One initial full-solution run encountered a transient `HttpListener` access-denied failure in
`SeparateWebPrefixServesStaticAwayFromApiPort`. The same test immediately passed in isolation and a
clean full-solution rerun passed Host 161/161 before the new Host lifecycle test; the final Host run
then passed 162/162. No product source change was made in response to that transient observation.

The current C# inventory is 81 product files / 24,888 physical LOC and 42 test files / 18,760
physical LOC. `DesktopNodeApiRequestProcessor.cs` decreased from 2,558 to 2,421 lines. The new
callback-free auth owner is 276 lines.

## Deferred policy decision

In account-ready mode, a correct service bearer sent to a permissioned API route is currently
interpreted by the processor as an account JWT and rejected, while older capability text describes
`service-token-or-account-jwt`. Changing precedence or accepting both credentials is a security
policy and transport compatibility decision. Wave 1C deliberately neither fixes nor adds a new
golden that would bless one side of that inconsistency. It requires a separate policy decision and
security-classified implementation slice.

## Closure and next boundary

- Wave 1C is `code_complete`; no package candidate was produced, so promotion is
  `promotion_not_triggered`.
- `0.42.65-admin-smoke`, Web Console/PCVCLI, TypeScript static assets and the internal/private
  network boundary carry forward unchanged.
- Installed account/browser/noVNC evidence is not refreshed by this code-level record. The next
  operator-surface account/noVNC product payload change must rerun those stale installed checks.
- Wave 1D ops dispatch remains a separate `M / Full` slice; `BatchEvidenceSummaryReader` internals
  remain assigned to Wave 7.
- The current-evidence ledger/JSON/schema, ADR index, verification policy, package/fullgate/current
  card, signing claim and publication claim are intentionally unchanged.
