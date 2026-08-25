# C# architecture Wave 5A partial listener-bind cleanup code-level PASS (2026-08-03)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Source commit: `fbd4b90` (`fix(host): clean up partial listener binds`)
- Result: `CODE_LEVEL_PASS`
- Host/service mutation: `false`
- Hyper-V/VM/provider mutation: `false`
- Package/install mutation: `false`
- Operational anchor: unchanged (`0.42.65-admin-smoke`)

## Change

`DesktopNodeHostApplication.StartAsync` now owns the listener-bind sequence inside a cleanup
boundary. If the second Web/API listener bind or subsequent processor construction fails after an
earlier listener has opened, every already-open listener is stopped and closed before the original
exception is rethrown. Successful startup and the existing single-listener path retain the same
endpoint and request behavior.

## Verification

| Check | Result |
|---|---|
| `FailedSecondListenerBindCleansUpFirstListener` | PASS |
| `TrackedAsyncAdmissionRejectsBeforeCapacityIsAvailable` | PASS |
| Focused Host test filter | `2/2`, skip `0` |
| Full `dotnet test src/DesktopNode.sln -c Release --no-restore` | `816/816`, skip `0` |
| `git diff --check` | PASS |

The regression test occupies the configured Web prefix, starts the Host against a separate API
prefix, asserts the second bind fails, and then successfully rebinds the API prefix. This proves the
first listener is not left behind after a partial startup failure.

The admission companion probe keeps its first controlled request active for 10 seconds so a loaded
CI runner cannot release the lease before the rejection request is issued; this is test-only timing
hardening and does not change the default runtime configuration.

## Remaining scope

This slice does not claim listener/worker fault propagation to Windows Service health, noVNC
opposite-copy cancellation, installed stop/drain behavior, ASP.NET Core transport parity, package
promotion, public trusted signing or external stable publication.
