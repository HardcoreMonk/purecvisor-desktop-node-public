# PureCVisor Desktop Node Next Batch Design - 0.38.2 Canonical Evidence and Evidence API Hardening

created_at: 2026-05-06T15:20:00+09:00
status: approved-for-planning
owner: Desktop Node

## Context

`0.38.2-admin-smoke` is the current full admin host mutation gate baseline. It completed under Batch Supervisor with Service/MSI/Hyper-V and OS mutation gates passing, final service `Running`, firewall count `0`, Event Log source absent, internal trust cert restored, boot time unchanged, and `pcv-spike-*` VM count `0`.

The next work should run in two batches:

1. Canonicalize the `0.38.2` evidence and documentation baseline.
2. Harden the Batch Supervisor evidence UX/API contract exposed through `ops.summary.batch_evidence`.

## Goals

Batch 1 closes evidence and documentation drift:

- Keep `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md` as the only canonical standalone latest full admin host mutation evidence document.
- Keep `0.38.1` only as historical ledger/reference evidence, not as a standalone canonical document.
- Update high-level docs, README files, ADR index, verification policy, public boundary docs, API tests, and web fixtures so their latest/canonical references point at `0.38.2`.
- Remove stale `0.38.0` or `0.38.1` latest wording from current guidance while preserving explicit historical rows.

Batch 2 hardens read-only evidence consumption:

- Strengthen Local API parsing of Batch Supervisor evidence roots.
- Treat missing, malformed, or partial artifacts as degraded evidence status, not as `ops.summary` route failure.
- Ensure stdout, stderr, absolute artifact roots, token-like strings, protected token paths, and known local paths are redacted from API responses.
- Keep Web Console changes limited to resilient display of latest/degraded evidence. Larger dashboard redesign remains a separate batch.

## Non-Goals

- No new actual host mutation run is part of these implementation batches. Any future host mutation still requires explicit administrator opt-in.
- No public trusted signing or external stable publication claim is introduced.
- No full Web Dashboard redesign is included.
- No internal `RequireSigned` release gate is implemented here.
- No product config or job store destructive migration apply is implemented.

## Batch 1 Design

Batch 1 is a narrow documentation and fixture closure.

The canonical evidence file is `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md`. A standalone `0381` evidence file should not remain in the worktree as canonical evidence. The historical `0.38.1` run may stay in the ledger table or historical prose where it is clearly not the latest baseline.

The Pester documentation guard should assert that high-level docs point to `0.38.2-admin-smoke`, the `full-admin-host-mutation-gate-20260506-145506-0382` batch root, the matching route parity and OS mutation artifact roots, the `d05d395e96d5d8d83b4cc4310c2b8ef11253041c` provenance commit, and the `4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252` MSI hash.

API and web fixtures should use `0.38.2` as the latest evidence example. Fixture values should match the actual run where visible to users: batch id, version, step durations, GPU snapshot count, and signing/publication status.

## Batch 2 Design

Batch 2 is API contract hardening with minimal UI follow-through.

`GET /api/v1/ops/summary` may expose `data.batch_evidence` only from a configured evidence root. It must not accept evidence paths from HTTP request parameters. The parser reads Batch Supervisor `summary.json`, route parity `summary.json`, MSI provenance, MSI lifecycle summary, OS mutation `summary.json`, and GPU snapshots when available.

Each evidence sub-area should degrade independently:

- Missing Batch Supervisor summary: configured but unavailable/degraded.
- Malformed Batch Supervisor summary: degraded with structured error code.
- Missing route parity or OS mutation artifact: latest batch still visible, affected sub-area degraded.
- Malformed provenance or lifecycle JSON: release/lifecycle sub-area degraded without leaking file contents.
- Missing GPU snapshots: count unavailable, not a route failure.

The API response remains compact and redacted. It should include enough for the Web Console to show latest batch id, status, step health, release version, signing mode, publication exclusion, route/MSI/OS health, and host final state.

The Web Console should not gain a new dashboard design in this batch. It should only render available, degraded, and unavailable evidence states without layout breakage or misleading pass labels.

## Data Flow

1. Host is started with an optional `--batch-evidence-root`.
2. `ops.summary` builds host/runtime/job summaries through existing native routes.
3. If configured, the evidence parser scans the configured root for the latest Batch Supervisor summary.
4. The parser resolves child artifact roots only when they are inside the configured evidence root or are `[REPO_ROOT]`-redacted paths that resolve under it.
5. Parsed evidence is normalized to a redacted JSON object.
6. Web Console consumes the same `ops.summary` payload and renders the batch evidence panel.

## Error Handling

Evidence parsing failures should be observable but non-fatal. The API should emit stable status/error fields for degraded evidence and should add an operator signal when evidence exists but cannot be fully parsed.

No response should include command stdout/stderr, bearer tokens, protected token file contents, local absolute artifact roots, or known sensitive paths.

## Verification

Batch 1 verification:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`
- `dotnet test src/DesktopNode.sln`
- `npm test --prefix web`
- `npm run verify:parity --prefix web`
- `node --check web/app.js`
- `git diff --check`

Batch 2 verification:

- API unit tests for missing/malformed/partial evidence.
- API unit tests for path and token redaction.
- Web fixture parity tests for available and degraded evidence.
- Existing required non-mutating verification commands affected by touched files.

## Acceptance Criteria

Batch 1 is complete when:

- Only `0.38.2` is treated as the standalone canonical latest full admin host mutation evidence.
- `0.38.1` appears only as historical ledger/reference material.
- High-level docs and fixtures no longer describe `0.38.0` or `0.38.1` as latest.
- The documentation guard and required non-mutating checks pass.

Batch 2 is complete when:

- `ops.summary.batch_evidence` returns degraded objects instead of route failures for missing, malformed, and partial artifacts.
- Sensitive command/path/token material is redacted by tests.
- Web Console renders available and degraded evidence states without misleading the operator.
- The implementation does not perform host mutation.
