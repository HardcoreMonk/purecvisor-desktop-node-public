# WMI Internal Switch Topology Recovery Design

## Status

- Date: 2026-07-13
- Decision: approved
- Target package: `0.42.62-admin-smoke` (execution amendment; original target was `0.42.61-admin-smoke`)
- Affected route: `network.inventory`
- Scope: arbitrary-name Hyper-V internal switches, including `WSL (Hyper-V firewall)`

## Context

The `0.42.60-admin-smoke` full admin host mutation gate installed the package and passed the MSI and service lifecycle operations, but stopped in the first route-parity step. The installed `network.inventory` request returned `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE` before VM creation, so the OS mutation profile did not run.

The original `0.42.61-admin-smoke` recovery package also installed and passed the MSI and service lifecycle operations, but its projected switch objects had empty WMI paths. The first `GetRelated` call raised `System.InvalidOperationException`, normalized as `PCV_NETWORK_INVENTORY_FAILED`, and the OS mutation profile again did not run. Elevated diagnostics proved that `SELECT *` preserved the paths and allowed both association traversals, so the final execution target moved to `0.42.62-admin-smoke` without changing the approved topology-classification decision.

The host contains both `Default Switch` and `WSL (Hyper-V firewall)`. Hyper-V reports both as internal switches with management-OS adapters. The current WMI provider recognizes only the exact `Default Switch` display name and maps every other switch to `unknown`. No runtime, API, or Hyper-V provider payload changed between the passing `0.42.59-admin-smoke` provenance and the `0.42.60-admin-smoke` provenance; the newly present WSL switch exposed an existing classification gap.

The native WMI provider remains authoritative. Restoring a PowerShell fallback would violate the current read-route boundary and would hide incomplete native topology instead of fixing it.

## Goals

- Classify a Hyper-V internal switch from WMI topology rather than its display name.
- Preserve the existing `Default Switch` identity and complete topology fields.
- Keep structured failure when topology cannot be classified confidently.
- Add a regression test that fails against the current name-only implementation.
- Produce and verify the amended `0.42.62-admin-smoke` target through the full admin host mutation gate and installed operator current-card.

## Non-goals

- Complete external-switch adapter mapping.
- Complete private-switch mapping.
- Reintroducing PowerShell helper fallback.
- Changing the public network inventory schema or structured error codes.
- Re-running actual-VM Guest Execution or QoS smoke without a related provider or control-payload change.

## Considered approaches

### 1. WMI port relationship classification — selected

Query `Msvm_InternalEthernetPort` names and compare them with the `Msvm_EthernetSwitchPort` objects related to each `Msvm_VirtualEthernetSwitch`. A matching port proves that the switch exposes a management-OS path. The provider also checks the related allocation settings for an `Msvm_ExternalEthernetPort` host resource, because an external switch may expose a management-OS port as well. Positive internal-port evidence combined with no external binding is independent of localized or product-generated switch names and preserves the WMI-native boundary.

### 2. Display-name allowlist — rejected

Recognizing names such as `WSL (Hyper-V firewall)` would be small but brittle. WSL, Windows, localization, and third-party components can change names while retaining the same topology.

### 3. PowerShell fallback — rejected

Calling `Get-VMSwitch` when WMI topology is incomplete would restore behavior on this host, but it would conflict with the authoritative native-provider design and weaken structured failure semantics.

## Design

### Provider responsibilities

`DesktopNodeHyperVWmiSwitchProvider` will read the following topology within the existing virtualization namespace:

1. Read the `Name` values of `Msvm_InternalEthernetPort` into a case-insensitive set.
2. Read `Msvm_VirtualEthernetSwitch` objects and obtain their related `Msvm_EthernetSwitchPort` objects.
3. For each related switch port, inspect its `Msvm_EthernetPortAllocationSettingData.HostResource` references for an `Msvm_ExternalEthernetPort` binding.

For each switch, the provider will collect related switch-port names and pass two topology facts to the pure mapping boundary: whether any related port name is present in the internal-port set and whether any related allocation is bound to an external Ethernet port.

### Mapping contract

The mapping boundary accepts the switch display name, the internal-management-port fact, and the external-binding fact.

- If an internal-management port is present and no external binding is present, return `Type=internal`, `AllowManagementOs=true`, and no external adapter description.
- Preserve `IsDefault=true` only when the display name equals `Default Switch`, matching the existing contract.
- Preserve the existing Default Switch result even if a legacy or restricted host does not expose the additional relationship query.
- If an external binding is present, or neither the Default Switch rule nor positive internal-port evidence applies, return `Type=unknown` and `AllowManagementOs=null`.

This change does not infer external or private topology. The native adapter therefore continues returning `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE` for switches that remain unknown.

### Cancellation and resource handling

Cancellation checks remain at operation entry and during switch enumeration. Every WMI searcher, result collection, switch object, and related port object is disposed within the provider boundary. A WMI query or association failure continues through the existing provider-error path; the provider does not silently downgrade to name heuristics or helper fallback.

## Test strategy

Implementation follows red-green-refactor:

1. Add a focused test mapping an arbitrary name such as `WSL (Hyper-V firewall)` with positive internal-port evidence.
2. Run the focused test and confirm it fails because the current mapping API cannot express or honor that evidence.
3. Add the minimum mapping and WMI relationship implementation required to pass.
4. Retain the existing Default Switch test and add negative assertions that an unproven arbitrary switch and an externally bound switch remain unknown.
5. Run the focused test project and the repository's relevant solution and PowerShell verification gates once.

The installed verification is not replaced by unit tests. The amended `0.42.62-admin-smoke` full admin host mutation gate must demonstrate that the same host inventory containing `Default Switch` and `WSL (Hyper-V firewall)` passes before VM and OS mutation work proceeds.

## Operational acceptance criteria

- `0.42.62-admin-smoke` package provenance points to the final recovery commit.
- Package MSI and payload aggregate hashes are recorded.
- The full admin host mutation batch completes both the route-parity/service/MSI/Hyper-V step and the OS mutation step.
- Installed `network.inventory` reports both current internal switches with complete topology.
- Service health and the installed Web/TUI/CLI current-card pass.
- Failure evidence from `0.42.60-admin-smoke` and `0.42.61-admin-smoke` remains preserved as predecessor RCA; neither is rewritten as a pass.
- Evidence states that internal admin-smoke verification is not public trusted signing or external stable publication evidence.

## Execution outcome amendment

The approved name-independent topology design remains unchanged. `0.42.60-admin-smoke` exposed the
name-only classification gap, while `0.42.61-admin-smoke` exposed the incomplete WMI object projection
used by the first implementation. `0.42.62-admin-smoke`, provenance
`7f71f0a518c5b592f233373522d36b5401c3f1df`, closed both issues:
`full-admin-host-mutation-gate-20260713-04262` completed 2/2 steps and the installed current-card
reported both current switches as `internal` with `allow_management_os=true`.

## Primary platform references

- [Msvm_InternalEthernetPort](https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/msvm-internalethernetport)
- [Msvm_ExternalEthernetPort](https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/msvm-externalethernetport)
- [Hyper-V networking classes](https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/hyper-v-networking-classes)
