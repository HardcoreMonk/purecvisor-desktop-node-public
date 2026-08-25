# Functional Correctness Runtime Hardening Design

## Status

Approved approach A on 2026-07-15. This design covers FC-02, FC-04, FC-16, FC-18, and an explicit FC-01 shrink-prevention guard. FC-05, FC-12(b), and FC-13 remain outside implementation scope because their runtime behavior was not verified with an isolated guest or bootable ISO.

## Problem Statement

Windows/Hyper-V host verification established four reproducible correctness defects and one unsafe source-level gap:

- rollback can move the active product root aside and then fail to promote the previous root, leaving the active root empty;
- network QoS accepts Kbps but writes the numeric value directly into a bits-per-second WMI field;
- backup rotation deletes the previous product root before the new backup move is known to succeed;
- unreadable evidence receives `DateTime.MaxValue` and sorts ahead of valid evidence;
- disk resize has no product-level guard against a target smaller than the current virtual capacity, even though the tested host rejected one destructive shrink request.

The implementation must prevent these failures without touching live installed state during automated tests.

## Goals

1. Keep either the active product root or a recoverable previous root available across every handled filesystem failure.
2. Preserve partial or failed payloads for diagnostics instead of deleting them during compensation.
3. Convert network QoS Kbps to bits per second exactly once at the WMI boundary.
4. Reject VHD/VHDX shrink requests before `ResizeVirtualHardDisk` is invoked.
5. Sort unreadable evidence behind readable evidence while retaining the existing behavior when unreadable evidence is the only candidate.
6. Add deterministic automated regression tests that fail on the current implementation and require no live service, VM, VHDX, or administrator mutation.

## Non-Goals

- No fix is inferred for FC-05, FC-12(b), or FC-13.
- No public signing, package publication, installer build, or host mutation is part of this change.
- No redesign of the complete update state machine or transaction journal schema.
- No change to CLI/Web-only operator-surface decisions.

## Design

### 1. Compensating product-root transactions

The nested default scriptblocks in `Invoke-PcvDesktopNodeProductAction` will delegate to two private module functions so Pester can exercise the real filesystem behavior in module scope:

- `Restore-PcvDesktopNodePreviousProductRoot`
- `Backup-PcvDesktopNodeProductRoot`

The functions remain private implementation details; the exported module surface does not grow.

#### Restore flow (FC-02)

1. Validate both paths and the previous manifest as today.
2. Move the active `ProductRoot` to `ProductRoot.failed`.
3. Attempt to promote `PreviousProductRoot` to `ProductRoot`.
4. If promotion fails:
   - move any partially created `ProductRoot` to a unique `ProductRoot.restore-partial.<guid>` path;
   - move `ProductRoot.failed` back to `ProductRoot`;
   - return `PCV_PRODUCT_RESTORE_FAILED` with detail stating whether active-root compensation succeeded and the partial diagnostic path, when present.
5. If compensation itself fails, return `PCV_PRODUCT_RESTORE_COMPENSATION_FAILED` and preserve every remaining path for operator recovery.

The success path retains the existing `ProductRoot.failed` diagnostic payload contract.

#### Backup flow (FC-16)

1. Use a stable `PreviousProductRoot.staging` path for the old backup.
2. If a staging path exists while `PreviousProductRoot` is absent, recover staging back to previous before starting a new rotation. If both exist, stop with `PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED` rather than deleting either.
3. Move the existing previous root to staging instead of deleting it.
4. Attempt to move the active product root to the previous-root path.
5. On success, delete the staged old backup.
6. On failure:
   - move any partially created previous-root destination to a unique `PreviousProductRoot.partial.<guid>` path;
   - restore staging to `PreviousProductRoot`;
   - return `PCV_PRODUCT_UPDATE_BACKUP_FAILED` while leaving the active root intact.
7. If old-backup restoration fails, return `PCV_PRODUCT_UPDATE_BACKUP_COMPENSATION_FAILED` and preserve all paths.

This is a compensating transaction, not a claim of cross-volume filesystem atomicity. The product and previous roots are expected to be siblings on the same installed volume, but failure handling does not delete the last known-good payload.

### 2. Hyper-V resource-mutation policy boundary

Add an internal policy unit in `DesktopNode.HyperV` and a new `DesktopNode.HyperV.Tests` project.

#### Network QoS (FC-04)

- `KbpsToBitsPerSecond(int value)` validates non-negative input and returns `checked((ulong)value * 1000UL)`.
- `SetNetworkQos` uses this conversion for WMI `Limit` and `Reservation`.
- Product evidence continues to expose the requested values as Kbps; WMI values are not mislabeled as Kbps.
- Tests pin `2048 Kbps -> 2_048_000 bps`, zero, and the supported maximum.

Microsoft documents `Set-VMNetworkAdapter -MaximumBandwidth` in bits per second, and the host readback confirmed that `Msvm_EthernetSwitchPortBandwidthSettingData.Limit` is surfaced in the same unit.

#### Disk resize (FC-01)

- Introduce an internal virtual-disk metadata reader whose production implementation calls `Msvm_ImageManagementService.GetVirtualHardDiskSettingData` and reads `Msvm_VirtualHardDiskSettingData.MaxInternalSize`.
- Compute requested bytes with checked arithmetic.
- Reject `requestedBytes < currentMaxInternalSize` with `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` before calling `ResizeVirtualHardDisk`.
- Equal-size and growth requests retain the current native path.
- Unit tests inject a fake metadata reader and assert that shrink rejection occurs before any native resize call can be reached. Conversion/policy tests do not require Hyper-V.

The WMI contract is documented at:

- <https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/getvirtualharddisksettingdata-msvm-imagemanagementservice>
- <https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/msvm-virtualharddisksettingdata>

### 3. Evidence ordering (FC-18)

`BatchEvidenceSummaryReader.GetEvidenceSummarySortTime` will return `DateTime.MinValue` when the summary path is unreadable, rather than `DateTime.MaxValue`.

This preserves two behaviors:

- a readable valid run wins over an unreadable run regardless of the unreadable run's file timestamp;
- if unreadable evidence is the only candidate, it can still be selected later in the ordered sequence and reported as unavailable by the existing read guard.

The regression test invokes the real private sort method through the established reflection-testing pattern and pins unreadable paths to `DateTime.MinValue`. Existing malformed-newest and reparse-point guard tests remain unchanged.

## Error and Recovery Contracts

| Condition | Result |
|---|---|
| Previous promotion fails, active-root compensation succeeds | `PCV_PRODUCT_RESTORE_FAILED`; active root restored; partial destination preserved |
| Previous promotion and active-root compensation both fail | `PCV_PRODUCT_RESTORE_COMPENSATION_FAILED`; all remaining paths preserved |
| New backup move fails, old-backup compensation succeeds | `PCV_PRODUCT_UPDATE_BACKUP_FAILED`; active and old previous roots remain usable |
| New backup move and old-backup compensation both fail | `PCV_PRODUCT_UPDATE_BACKUP_COMPENSATION_FAILED`; all remaining paths preserved |
| Stale backup staging conflicts with an existing previous root | `PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED`; no deletion |
| Disk target is smaller than current virtual capacity | `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`; no resize WMI mutation |

## Test Strategy

### PowerShell/Pester

Add default-path tests to `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`:

1. Hold a previous-root executable handle, run default rollback, and assert the current marker is restored to the active root while the previous marker remains recoverable.
2. Hold an active-root executable handle, run default backup, and assert the previous marker is restored after the new backup move fails.
3. Pin successful restore and backup cleanup behavior so `.staging` and compensation-only paths do not leak after success.

All service/process operations use injected no-op scriptblocks.

### .NET/xUnit

- Create `src/DesktopNode.HyperV.Tests` and add it to `src/DesktopNode.sln`.
- Test Kbps-to-bps conversion and disk-shrink policy without connecting to WMI.
- Add the FC-18 unreadable-sort regression test to `DesktopNode.Api.Tests`.
- Run targeted tests red before production changes, then green after each minimal fix.
- Finish with Release build and the complete solution test suite.

## Documentation

Update the 2026-07-15 verification result with a remediation section that distinguishes runtime finding verification from source-level remediation. Do not rewrite historical command evidence or turn unverified findings into fixed claims.

## Acceptance Criteria

1. Each new regression test is observed failing against the current code for the intended reason.
2. The active product root is restored after forced previous-promotion failure.
3. The old previous root is restored after forced new-backup failure.
4. `2048 Kbps` produces `2_048_000` at the WMI write boundary.
5. A shrink request returns `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` before WMI resize invocation.
6. Unreadable evidence sorts behind readable evidence.
7. Release build has zero errors and the full solution test suite has zero failures.
8. No live service, installed root, VM, VHDX, package, or host configuration is mutated by automated tests.
