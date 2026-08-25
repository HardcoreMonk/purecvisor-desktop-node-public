# 0.39.1 Frontend Full Admin Host Mutation Gate Evidence

evidence_id: full-admin-host-mutation-gate-2026-05-09-0391-frontend
created_at: 2026-05-09T12:22:31+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-122028-0391-frontend
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-122028-0391-frontend
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260509-122028-0391-frontend
admin_smoke_version: 0.39.1-admin-smoke
source_commit_sha: d8e7e162a13817dc869f30712d77c5c036981786
admin_msi_sha256: f5086e64a58bdb43a8196574dacf383d600c5cccca0f60aeb99ed3f95b65bd73
admin_signing_mode: AllowUnsignedDev
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
host_mutation_performed: true
linux_runtime_excluded: true

## Scope

This evidence records the requested frontend host mutation run
`20260509-122028-0391-frontend`. It was an explicit administrator opt-in full
admin host mutation gate for the Windows Desktop Node installed payload.

The run covered Service/MSI/Hyper-V route parity plus firewall, LAN listener,
Event Log, and ADR-0003 internal Root/TrustedPublisher trust-store mutation
gates. It does not claim public trusted signing or external stable publication.

## Batch Supervisor Result

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-122028-0391-frontend`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `duration_ms=109807`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `duration_ms=11068`

## Service/MSI/Hyper-V Result

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-122028-0391-frontend`
- MSI: `PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.msi`
- MSI SHA-256: `f5086e64a58bdb43a8196574dacf383d600c5cccca0f60aeb99ed3f95b65bd73`
- MSI provenance commit: `d8e7e162a13817dc869f30712d77c5c036981786`
- MSI signing mode: `AllowUnsignedDev`
- Payload aggregate SHA-256: `db36f3ab755dded5f9c69cbc8a40b5ace972ec70ead9dc92675ffe012e3339a7`
- Service host SHA-256: `4d37bba70feb6a3340afe444955d50de4986728617d7f9b2ec1d7309384d3229`
- Product wrapper SHA-256: `7dbd8cadb81b75044f9afdb14fdc0834e835a9db7bc9e8609d937e69fc948250`
- Final proof: service `Running`, startup `Auto`, boot time unchanged, `remaining_pcv_vms=[]`

## Firewall, LAN, Event Log, Trust Store Result

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260509-122028-0391-frontend`
- OS mutation summary: `ok=true`, `actual_execution=completed`, `host_mutation_performed=true`
- LAN smoke: `http://[redacted-private-endpoint]:7777/`
- Firewall final rule count: `0`
- Event Log final source present: `false`
- Final service: `PureCVisorDesktopNode` `Running`
- Boot time unchanged: `true`
- Final internal Root thumbprint: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
- Final internal TrustedPublisher thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`

## Follow-up Installed Frontend Payload

The requested `20260509-122028-0391-frontend` run is preserved as pass evidence.
After additional Web Console hardening, the installed payload was rebuilt and
rerun as `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-130105-0391-frontend-final2`.
That later run is the installed listener baseline used by
`docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md`.

## Verdict

`20260509-122028-0391-frontend` is PASS internal admin-smoke evidence for the
Windows Desktop Node frontend payload line. It is not public trusted signing
evidence and is not external stable publication evidence.
