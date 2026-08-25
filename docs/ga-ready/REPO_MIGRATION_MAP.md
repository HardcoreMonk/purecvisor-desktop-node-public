# GA-ready 저장소 Migration Map

이 문서는 ADR-0004 적용 이후 내부 전용 GA-ready 제품 런타임에서 `spikes/**`를 활성 제품 경로에서 제거하고 archive로 이동한 migration result와 후속 target을 고정한다.

현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`과 `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`다. 이 문서는 ADR-0004의 repo migration supporting contract다.

## 목표 Layout

```text
src/
  DesktopNode.Host/
  DesktopNode.Api/
  DesktopNode.Runtime/
  DesktopNode.Contracts/
  DesktopNode.HyperV/
  DesktopNode.Service/
  DesktopNode.Cli/
web/
  package.json
  src/
  tests/
packaging/
  windows-desktop-node/
docs/
  adr/
  superpowers/
  ga-ready/
archive/
  spikes/
```

## Migration 규칙

- 제품 runtime source는 `src/**` 또는 `web/**`에 둔다.
- `spikes/**`는 역사/archive baseline으로 축소한다.
- 경로 이동은 behavior 변경과 분리한다.
- 문서 링크와 검증 command는 migration map에서 함께 갱신한다.
- `packaging/windows-desktop-node/**`의 product root/source root contract는 migration slice마다 검증한다.

## Migration 실행 Guard

- 이 map의 2026-05-04 snapshot은 승인 시 목표 상태를 기록한 target map이었으며, 당시 파일 이동 실행 승인은 아니었다.
- 2026-05-05 사용자 physical archive move opt-in 이후 `spikes/purecvisor-desktop-node/**`는 `archive/spikes/purecvisor-desktop-node/**`로 `git mv` 이동했다.
- 이동 slice는 import/package/test path 갱신 범위, rollback 기준, archive target 검증을 같은 review scope에 포함한다.
- 각 migration slice는 behavior 변경과 경로 이동을 분리하고, 관련 문서 링크와 검증 command 갱신을 같은 review scope에 포함해야 한다.
- Phase 26 alignment 첫 slice 당시에는 파일 이동을 하지 않았다.
- 2026-05-03 Web Console served asset/root migration slice는 별도 implementation plan으로 실행됐으며, former `spikes/purecvisor-desktop-node/web/**` 활성 제품 경로를 `web/**`로 이동했다.
- 2026-05-03 VM power-state/checkpoint/native lifecycle/delete adapter slices는 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete product execution을 C# WMI adapter로 옮겼다. 2026-05-05 physical archive move 이후 `archive/spikes/purecvisor-desktop-node/hyperv/**`는 current served product fallback이 아니라 component/regression 검증 경계다.
- 2026-05-04 standalone product wrapper asset boundary slice는 `Copy-PcvDesktopNodeProductAssets`와 product manifest asset list에서 legacy `spikes/purecvisor-desktop-node/{api,hyperv,service}` source staging을 제거했다. Standalone product asset source는 repo-root `web/**`로 제한한다.
- 2026-05-09 active .NET CLI slice는 archived PowerShell CLI를 되살리지 않고 `src/DesktopNode.Cli/**`에 product-owned Local API client를 추가했다. Published command name은 `pcvcli.exe`이며 MSI payload, product manifest, update payload validation에서 같은 file contract를 추적한다.
- 이동 실행 전 source path inventory, import/relative path graph, packaging/static asset input binding, generated parity manifest update, docs command update, no behavior change evidence, archive target read-only intent, rollback restore 기준이 정의됐다.
- Archive read-only intent, rollback restore 기준, source/target/hash inventory는 `docs/ga-ready/evidence/archive-readonly-rollback-2026-05-04.md`와 `docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json`에 pass 상태로 기록됐다.
- 2026-05-05 이동 후 inventory는 `docs/ga-ready/evidence/archive-spikes-inventory-postmove-2026-05-05.json`에 기록됐다. Source path는 absent, archive file count는 46, pre-move inventory target match는 46개, 문서/테스트 경로 갱신으로 SHA-256 mismatch 8개가 별도 기록됐다.
- 이동 후 no behavior change evidence는 관련 Pester/npm/`verify:parity`/`node --check` evidence로 확인한다.

## 경로 Map

| 기존 경로 | 활성 제품 target | 현재 archive 경로 | Migration 상태 |
|---|---|---|---|
| `spikes/purecvisor-desktop-node/api/**` | `src/DesktopNode.Api/**` | `archive/spikes/purecvisor-desktop-node/api/**` | archived 2026-05-05; product route owner는 .NET |
| `spikes/purecvisor-desktop-node/hyperv/**` | `src/DesktopNode.HyperV/**` | `archive/spikes/purecvisor-desktop-node/hyperv/**` | archived 2026-05-05; C# WMI/CIM parity owner가 product path |
| `spikes/purecvisor-desktop-node/service/**` | `src/DesktopNode.Service/**` | `archive/spikes/purecvisor-desktop-node/service/**` | archived 2026-05-05; product service-action은 native host owner |
| `spikes/purecvisor-desktop-node/cli/**` | `src/DesktopNode.Cli/**` | `archive/spikes/purecvisor-desktop-node/cli/**` | archived 2026-05-05; active .NET CLI reintroduced 2026-05-09 as `pcvcli.exe` Local API client without Linux/KVM helper runtime |
| `spikes/purecvisor-desktop-node/web/**` | `web/**` | 없음 | 2026-05-03 served asset/root migration slice에서 active product target으로 이동됨 |
| `packaging/windows-desktop-node/**` | `packaging/windows-desktop-node/**` | 없음 | PowerShell orchestration 제거 slice별 갱신 |

## 실행 완료 Web Migration Slice

- `web/src/served-app.ts`가 served `web/app.js` build output의 source owner다.
- `web/package.json`의 `build:served`, `check:served`, `generate:parity`, `verify:parity`, `browser:fixture`가 Web Console package 검증 owner다.
- `packaging/windows-desktop-node/installer/build.ps1`는 repo-root `web/app.js`, `web/index.html`, `web/styles.css`를 MSI payload `web/**`로 staging한다.
- 이 slice 자체는 PowerShell helper 제거, API/Hyper-V/service/CLI migration, ADR-0004 current 적용, aggregate closure report 생성을 하지 않았다. 이후 ADR-0004 적용 diff와 2026-05-05 aggregate closure가 current decision을 갱신했다.

## 실행 완료 Product Wrapper Asset Boundary Slice

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`의 standalone product asset list는 repo-root `web/**`만 stage한다.
- Product manifest `assets`에는 `api/**`, `hyperv/**`, `service/**` legacy component file이 들어가지 않는다.
- MSI installed payload의 product-owned boundary와 standalone wrapper asset copy boundary가 같은 방향으로 정렬됐다.
- 이 slice 자체는 `spikes/**` 파일 이동, PowerShell helper 삭제, token storage implementation 교체, administrator host mutation을 실행하지 않았다. 2026-05-05 physical archive move가 별도 slice로 파일 이동을 실행했다.

## 실행 완료 Active .NET CLI Slice

- `src/DesktopNode.Cli/**`가 active product CLI source owner다.
- `src/DesktopNode.Cli.Tests/**`와 `src/DesktopNode.sln`이 command routing, token source precedence, output formatting, transport redaction, project contract를 검증한다.
- Installer build는 published `pcvcli.exe`를 MSI payload에 stage하고, product wrapper manifest는 `paths.cli_exe`와 `cli` metadata로 installed command boundary를 기록한다.
- Product update payload validation은 `pcvcli.exe` 누락을 service mutation 전에 차단한다.
- Archived `archive/spikes/purecvisor-desktop-node/cli/**`는 historical/component baseline으로만 남고 active product runtime에는 staging하지 않는다.
