# Hyper-V Provider Set Contract Code-Level - 2026-05-15

```text
evidence_id: hyperv-provider-set-contract-code-level-2026-05-15
scope: hyperv-domain-wmi-provider-set-contract
result: CODE_LEVEL_PASS
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

이 code-level evidence는 Hyper-V domain의 WMI provider composition을
`DesktopNodeHyperVNativeAdapter.CreateDefault()` 내부 구현에서
`DesktopNodeHyperVProviderSet` 계약으로 분리한 기록이다. 이 변경은 repository
code/test 변경이며 host mutation을 수행하지 않았다.

## Contract

- `DesktopNodeHyperVProviderSet.CreateDefaultWmi()`가 WMI 기반 provider set 생성을
  소유한다.
- provider set은 `SwitchProvider`, `HostStatusProvider`, `VmProvider`,
  `CheckpointProvider`, `CheckpointMutationProvider`, `VmPowerStateProvider`,
  `VmCreateProvider`, `VmDeleteProvider`를 명시적 boundary로 노출한다.
- `ToProviderBoundaryMap()`은 `DesktopNodeHyperVWmiProviderCatalog`의
  provider boundary와 concrete implementation drift를 테스트할 수 있는
  `IReadOnlyDictionary<string, object>`를 반환한다.
- `DesktopNodeHyperVNativeAdapter.CreateDefault()`는 provider set을 소비하고,
  adapter 생성자의 상세 WMI wiring은 provider set 쪽으로 이동했다.

## Changed Files

- `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs`
- `src/DesktopNode.Api.Tests/HyperVDomainContractTests.cs`

## Verification

RED:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter HyperVWmiProviderSetCreatesDefaultBoundaryMapFromCatalog
```

초기 실패: `CS0103` (`DesktopNodeHyperVProviderSet` 없음).

GREEN:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter HyperVWmiProviderSetCreatesDefaultBoundaryMapFromCatalog
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter HyperVDomainContractTests
```

결과: focused provider-set test PASS, `HyperVDomainContractTests` 8개 PASS.

## Packaging Boundary

이 code-level slice는 `0.42.16-admin-smoke` package/full gate 이후의 repository
변경이다. 따라서 이 slice 자체를 설치본 evidence로 승격하려면 다음 product payload
package build, package-pair, full admin host mutation gate가 필요하다.
