# Web verification Wave B local parity evidence

## Verdict

This is local, code-level parity evidence for the single Web verification migration row. The
legacy Web Pester suite and its Node replacement both passed all 50 contracts from the same clean
code input, and the controlled missing-`app-root` defect failed on both paths. Required CI has not
run this replacement, the required workflow has not cut over, and the legacy Pester suite remains
authoritative.

evidence_input_head=20ba3b80c211cc6a29bc9ecaf7e9195911678f14
input_dirty_state=clean

## Registry mapping identity

The mapping input is the 50 registry rows in registry order. Each UTF-8 row is serialized as
`${legacyName}\0${id}\n`; the following byte count and lowercase SHA-256 bind this evidence to that
exact ordered mapping.

mapping_rows=50
mapping_bytes=5077
mapping_sha256=91c00cdf3ed8cd6a39ebb27131c629d1b54561f362f8099b2716c21a6c7a4d95

## Same-input measurements

All commands below ran at the clean evidence input. Durations are reported by the test owner when
available and by the outer wall-clock measurement as `wall`.

### Separate-command architecture wiring

```text
node --test web/node-tests/web-verification-architecture-boundary.test.mjs
```

Result: tests `3`, passed `3`, failed `0`, skipped `0`; Node `duration_ms=76.7103`; exit `0`.
This pre-evidence architecture run proves the existing `test` and `verify:parity` commands stayed
unchanged, the catalog stayed at its planning boundary, and the four Wave B commands were exposed
separately.

### Legacy Web Pester

```text
pwsh -NoProfile -NonInteractive -Command "$r=Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -PassThru -Output None; [pscustomobject]@{total=$r.TotalCount;passed=$r.PassedCount;failed=$r.FailedCount;skipped=$r.SkippedCount;not_run=$r.NotRunCount;duration_ms=[math]::Round($r.Duration.TotalMilliseconds)}|ConvertTo-Json -Compress; if($r.FailedCount -ne 0 -or $r.PassedCount -ne 50){exit 1}"
```

Result: total `50`, passed `50`, failed `0`, skipped `0`, not run `0`; Pester
`duration_ms=2857.0`; `wall=4589ms`; exit `0`. This was a controlled, test-only compatibility
measurement and performed no host mutation.

### Node positive projection

```text
node --test --test-reporter=spec web/node-tests/web-static-contracts.test.mjs
```

Result: tests `50`, passed `50`, failed `0`, skipped `0`; Node `duration_ms=3051.3319`;
`wall=3107ms`; exit `0`.

### Controlled negative parity

```text
node web/scripts/verify-web-contract-negative-parity.mjs
```

Result: `Web negative parity PASS: defect=missing-app-root pester_executed=1 pester_failed=1 pester_not_run=49 node_failed=1 node_skipped=49 cleanup=pass`;
`wall=3067ms`; exit `0`.

The defect is a missing temporary-fixture `app-root`. With
`PCV_WEB_CONTRACT_FIXTURE_MODE=negative-parity-v1`, Node registers all 50 contracts, explicitly
skips the 49 contracts other than `root-assets`, and executes the defective root contract. Raw TAP
therefore reports tests `50`, passed `0`, failed `1`, skipped `49`. The Pester side executes and
fails one selected contract and leaves 49 not run. Cleanup passed, and all counts are raw process
output rather than synthesized values.

### Wave B unit suites

```text
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs web/node-tests/verification-migration-manifest.test.mjs web/node-tests/web-contract-negative-parity.test.mjs
```

Result: tests `190`, passed `190`, failed `0`, skipped `0`; Node `duration_ms=15099.8833`;
`wall=15159ms`; exit `0`.

### Existing npm owners

```text
npm test --prefix web
```

Result: exit `0`; `wall=2907ms`; feature surfaces `web=52`, excluded `8`; served asset current;
frontend batches `5`, items `25`.

```text
npm run verify:parity --prefix web
```

Result: exit `0`; `wall=2361ms`; served asset, static parity, and browser fixture all passed.

## Migration state and claim boundary

Only `web/tests/PcvDesktopWeb.Static.Tests.ps1` is promoted to `parity_status=mapped` and
`local_parity.status=pass`. Its `ci_parity.status` remains `pending` with null evidence. The other
61 manifest rows remain `unmapped`, local pending, and CI pending. This local evidence does not
alter the generated operational evidence, resolve the current actual-VM saved-lifecycle blocker,
or qualify a public release.

ci_parity_pass=false
required_ci_pester_zero=false
required_ci_nonadmin_powershell_zero=false
cutover_completed=false
host_mutation_performed=false
msi_or_service_mutation=false
actual_vm_tested=false
public_trusted_signing=false
external_stable_publication=false
operational_current=0.42.74-admin-smoke
