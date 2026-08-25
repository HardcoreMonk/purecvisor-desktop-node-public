# Web Console Destructive Lifecycle UI Evidence - 2026-05-09

evidence_id: web-console-destructive-lifecycle-ui-2026-05-09
scope: windows-desktop-node-web-console-installed-listener-destructive-lifecycle
base_url: http://127.0.0.1:7777/
browser_qa_artifact_root: artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391
runner: web/scripts/capture-destructive-lifecycle-ui-qa.mjs
host_mutation_performed: true
mutation_source: installed-listener-web-console-ui
token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
linux_runtime_excluded: true

## Summary

Installed listener destructive lifecycle UI QA was executed against
`http://127.0.0.1:7777/` using headless Chrome over CDP. The browser drove the
installed Web Console controls for a managed test VM and did not call the
mutation routes directly from the shell for the PASS path.

The managed test VM was `pcv-spike-ui-20260509-150353`, created under
`%TEMP%\pcv-hyperv-ui-smoke` with the existing local ISO
`D:\Downloads\Rocky-10.1-x86_64-minimal.iso`. The VM and its temp root were
removed after the run.

## UI Action Result

| Action | Job ID | Result |
|---|---|---|
| `vm.create` | `job-28accb9f959d4d36b24f588d6ff5a15d` | `succeeded` |
| `vm.start` | `job-a8fbf062031a495abd3ff215c4f55db9` | `succeeded` |
| `vm.restart` | `job-8350bffd45254de18606794f5a2ea0c7` | `succeeded` |
| `checkpoint.create` | `job-ee381ade6f89432bac0615238d8fda89` | `succeeded` |
| `vm.poweroff` | `job-028bbdf8c9d6427da9b7d44ad5ada456` | `succeeded` |
| `checkpoint.restore` | `job-2e8711201fe94c98b955e57bad86bf3f` | `succeeded` |
| `checkpoint.delete` | `job-1d4c515f291d4c2db2c072e68492b5be` | `succeeded` |
| `vm.delete` | `job-3509475ca0094104ba15eccd75a9215f` | `succeeded` |

Destructive confirmation dialogs were exercised for restart, poweroff,
checkpoint restore, checkpoint delete, and VM delete. The evidence summary
records the confirmation text but no token value.

## Cleanup

- `cleanup.vm_absent_after_delete=true`
- `Get-VM -Name 'pcv-spike-ui-*'` returned no VM after the run.
- `%TEMP%\pcv-hyperv-ui-smoke\pcv-spike-ui-*` leftovers were removed after
  verifying that no matching Hyper-V VM existed.
- `PureCVisorDesktopNode` remained `Running`.

## Screenshot Evidence

The run produced the following screenshot hashes under
`artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391`.

| Screen | SHA-256 |
|---|---|
| `00-dashboard-before-create.png` | `933709713b45a98f29fd1c92a05cde5e8b8138a084345c967cb390426be277a1` |
| `01-vm-selected-after-create.png` | `0902543d447cdab3ff474c129ae28699ba2d109742c8b9368db18072841c6a3b` |
| `02-vm-start-job.png` | `613708296c5ef4c63807d14d4ac1f68345fcef93eaa3b9f32e973cccf1ecc27f` |
| `03-vm-restart-job.png` | `54253d9ab0cb1bf85360e95b258e144a6d9f761a61101bf9500ac33d226db758` |
| `04-checkpoint-created.png` | `0f202faf87faffa0f16d743411068f393d65f198682d2a638078dd4c1bd58a97` |
| `05-vm-powered-off.png` | `ac70b635d0bb4957ee8d2e6159cb63c0960d07ea5f636452b74045cff97ad9c8` |
| `06-checkpoint-restored.png` | `f190cb7ea3882f9d4e543bbf162a5ac3e70410cf7f1e7285885a4dd9797fcd34` |
| `07-checkpoint-deleted.png` | `fadfe5eefbe78af24fcc1e71b5ffe01f75087dc7192c35425d9e42e1a046104b` |
| `08-vm-delete-job.png` | `3b1cbff0e830b57376b7103c27ef6f6a7990f1ef9a6767c48d49950215a39036` |
| `09-jobs-after-delete.png` | `c45c1a4080534ade239cc3b4ca8c5c5e52ceda136aa1ff4d45d2774e5cd6d043` |

## Commit Scope Decision

Include `web/scripts/capture-destructive-lifecycle-ui-qa.mjs` in the commit. It
is a reusable installed-listener CDP runner for explicit admin opt-in lifecycle
UI evidence. Do not commit `artifacts/web-console-destructive-lifecycle-ui-*`;
the preserved artifact root is referenced by this document and remains ignored
as QA output.

## Verdict

The installed Web Console destructive lifecycle UI QA is PASS. This evidence is
host-mutating Hyper-V UI evidence, not public trusted signing evidence and not
external stable publication evidence.
