# Repo Migration Preflight Evidence - 2026-05-04

evidence_id: repo-migration-preflight-2026-05-04
created_at: 2026-05-04T00:44:00+09:00
source_commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
working_tree_status: dirty-precommit
migration_status: pass
migration_status_reason: physical-archive-move-executed-2026-05-05
physical_spikes_file_count: 46
active_product_path_count_initial: 61
active_product_path_count_current: 0
component_archive_spikes_reference_count_current: 0
component_archive_path_reference_count_current: 22
installer_payload_spike_source_count: 0
standalone_product_asset_spike_source_count: 0
post_reboot_active_spike_command_count: 0
docs_required_spike_command_count: 0
verification_ownership_map_updated: yes
verification_ownership_replacement_status: pass
archive_readonly_rollback_evidence_status: pass
archive_target: archive/spikes/**
archive_inventory_path: docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json
postmove_inventory_path: docs/ga-ready/evidence/archive-spikes-inventory-postmove-2026-05-05.json
machine_readable_json_created: no
last_updated_at: 2026-05-05T01:12:00+09:00

## 범위

이 preflight는 GA-ready repo migration을 실행하기 전 active `spikes/**` 의존성을 고정한다. 2026-05-04 snapshot에서는 파일 이동 실행 승인이 아니었고, 2026-05-05 사용자 physical archive move opt-in 이후 파일 이동을 실행했다.

## 관찰 결과

- 2026-05-04 snapshot에서 `spikes/purecvisor-desktop-node/**` 아래 physical file은 46개였다. 2026-05-05 실행 후 source path는 absent이고 `archive/spikes/purecvisor-desktop-node/**` 아래 physical file은 46개다.
- 초기 active reference count 61은 `packaging/**`, root `README.md`, `AGENTS.md`, `follower.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/ADR_INDEX.md`에서 관찰한 direct `spikes/purecvisor-desktop-node` reference 수다. 2026-05-04 후속 slice 이후 active product path 기준 direct reference 재계산 값은 0개다. 2026-05-05 physical archive move 이후 active docs의 component/archive 문서 진입점은 `archive/spikes/purecvisor-desktop-node/**`로 갱신됐고 direct source `spikes/purecvisor-desktop-node` component reference는 이동 전 inventory/evidence snapshot에만 남는다.
- 초기 preflight 당시 `packaging/windows-desktop-node/installer/build.ps1`는 `spikes/purecvisor-desktop-node/**` payload를 installer input으로 stage했다. 2026-05-04 후속 slice에서 MSI payload는 product wrapper, `DesktopNode.Host.exe`, repo-root `web/**`, manifest만 stage하며 installer payload의 active spike source count는 0이다.
- 초기 preflight 당시 standalone product wrapper asset copy는 legacy `spikes/purecvisor-desktop-node/{api,hyperv,service}` component source를 product asset으로 stage했다. 2026-05-04 후속 slice에서 `Copy-PcvDesktopNodeProductAssets`와 product manifest asset list는 repo-root `web/**`만 stage하며 standalone product asset spike source count는 0이다.
- 초기 preflight 당시 `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`는 root README와 Hyper-V Pester command를 active verification input으로 참조했다. 2026-05-04 후속 slice에서 `HyperVNonIntegration` profile은 active product post-reboot profile에서 퇴역했고, product post-reboot profile의 active spike command count는 0이다.
- 초기 preflight 당시 `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`는 service module을 `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`에서 import했다. 2026-05-04 후속 slice에서 이 import는 제거됐고, runner는 설치본 protected token file을 동일 DPAPI LocalMachine schema로 직접 검증/복호화한다.
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`는 protected token preparation/health check를 product wrapper 내부 DPAPI LocalMachine helper로 처리하며 spike service module을 import하지 않는다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`와 `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`는 product wrapper protected-token helper와 installed root-level `DesktopNode.Host.exe` marker를 검증하며 spike service module path를 검증 입력으로 사용하지 않는다.
- root README, AGENTS, follower, developer index, public release boundary의 developer entry point 또는 component/archive baseline reference는 `archive/spikes/purecvisor-desktop-node/**`로 갱신됐다. 기본 required verification command의 direct spike Pester path count는 0이고 active product path reference count도 0이다.

## 차단 조건

migration_status는 2026-05-05 physical archive move 이후 pass다.

- MSI packaging/static asset input binding은 product-owned payload로 교체됐다. Standalone product wrapper asset copy도 repo-root `web/**` product asset source로 축소됐다. Component verification 경계와 일부 developer/component 문서에는 `spikes/**` reference가 남아 있지만 active product path로 계산하지 않는다.
- post-reboot verification의 Hyper-V component Pester command는 active product profile에서 퇴역했다. Route parity mutation smoke의 spike service module import, MSI installer payload spike staging, post-reboot active spike command, docs required verification command의 direct spike Pester path, product wrapper protected-token spike service module import, post-reboot repo boundary spike marker는 닫혔다.
- docs command update는 기본 개발 loop 기준 완료됐다.
- Verification ownership map은 기본 command owner, component/archive baseline owner, equivalent coverage mapping을 반영하도록 업데이트됐다.
- archive/spikes/** target은 read-only intent, rollback restore criteria, source/target/hash inventory를 기록했고, 2026-05-05 `git mv`로 physical archive move를 실행했다.

## 다음 최소 slice

1. Active product path 기준 `spikes/**` source/reference 제거는 완료됐다.
2. 남은 active docs component/archive entry point는 `archive/spikes/purecvisor-desktop-node/**`로 갱신했다. Direct source `spikes/**` reference는 이동 전 inventory/evidence snapshot에만 남는다.
3. Verification ownership replacement는 pass다. Legacy Pester suite는 component/archive baseline으로 남고 product primary 검증은 xUnit/npm/package/post-reboot product-owned suite로 둔다.
4. Archive source/target/hash inventory는 `docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json`에 기록됐다.
5. 파일 이동과 archive write는 2026-05-05 opt-in 이후 실행했고, no behavior change evidence와 rollback restore evidence는 post-move verification으로 닫는다.

## 후속 slice 기록

2026-05-04T13:21:00+09:00 후속 slice는 `Invoke-PcvRouteParityMutationSmoke.ps1`의 protected token read path에서 `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1` import를 제거했다. 새 runner는 `schema_version=1`, `storage=dpapi-local-machine`, `scope=LocalMachine`, `protected_token` schema를 직접 확인하고 `ProtectedData` LocalMachine scope로 token을 메모리 안에서만 복호화한다. `-SelfTest`는 stdout/stderr capture regression과 protected token DPAPI round trip을 함께 확인한다.

2026-05-04T13:41:49+09:00 후속 slice는 `packaging/windows-desktop-node/installer/build.ps1`의 MSI payload staging에서 active `spikes/**` source를 제거했다. `Product.wxs`도 설치 파일 소유 범위를 `DesktopNode.Host.exe`, product wrapper, repo-root Web Console, manifest로 줄였고, 설치본 product wrapper `SourceRoot` default는 service module 존재 대신 root-level `DesktopNode.Host.exe`를 설치본 신호로 인식한다.

2026-05-04T13:50:24+09:00 후속 slice는 `PcvPostRebootVerification.psm1`의 `HyperVNonIntegration` profile을 active product post-reboot verification에서 퇴역시켰다. 활성 profile은 `ProductStatus`, `PackagingRegression`만 허용하고, retired profile 요청은 `PCV_POST_REBOOT_PROFILE_RETIRED` structured failure로 닫는다.

2026-05-04T13:55:17+09:00 후속 slice는 AGENTS, README, verification policy, public release boundary, follower의 기본 required verification command에서 direct spike Pester path를 제거했다. Component/archive baseline 검증은 verification ownership map으로 분리하며, 기본 개발 loop command는 packaging, installer, web, dotnet, npm, node check, diff check로 유지한다.

2026-05-04T13:59:24+09:00 후속 slice는 `docs/ga-ready/VERIFICATION_OWNERSHIP.md`를 업데이트해 default command owner와 component/archive baseline owner를 분리했다. Legacy API/service/CLI/Hyper-V/root-boundary Pester는 component/archive baseline으로 남고, 기본 required command에서는 제외된다.

2026-05-04T14:03:00+09:00 후속 slice는 `docs/ga-ready/evidence/archive-readonly-rollback-2026-05-04.md`를 추가해 `archive/spikes/**` read-only intent, rollback restore criteria, no behavior change criteria를 정의했다. 이 slice는 파일 이동과 archive write를 실행하지 않았다.

2026-05-04T15:11:00+09:00 후속 slice는 standalone product wrapper asset copy boundary를 product-owned source로 축소했다. `Get-PcvDesktopNodeProductAssets`, `New-PcvDesktopNodeProductManifest`, `Copy-PcvDesktopNodeProductAssets`는 repo-root `web/**`만 product asset으로 stage하고, legacy `api/**`, `hyperv/**`, `service/**` component files를 product manifest/copy output에 포함하지 않는다. 이 slice는 파일 이동, archive write, protected token implementation 교체, administrator host mutation을 실행하지 않았다.

2026-05-04T23:37:43+09:00 후속 slice는 product wrapper protected-token preparation/health check에서 spike service module import를 제거하고 product wrapper 내부 DPAPI LocalMachine helper로 대체했다. `PcvPostRebootVerification.psm1` repo boundary marker는 retired spike README 대신 `src/DesktopNode.Host/DesktopNode.Host.csproj`를 확인한다. Packaging README/installer README/ADR index/verification policy의 active command성 direct spike reference도 제거했다. 동일 범위 direct reference 재계산 값은 22개이며, 이 22개는 component/archive 문서 진입점으로만 남는다. Active product path 기준 `spikes/**` reference count는 0이다.

2026-05-05T01:12:00+09:00 후속 slice는 사용자 physical archive move opt-in에 따라 `spikes/purecvisor-desktop-node/**`를 `archive/spikes/purecvisor-desktop-node/**`로 `git mv` 이동했다. 이동 전 absolute source/target이 repo 내부임을 확인했고, 이동 후 source path absent, archive file count 46개, pre-move target inventory match 46개를 `archive-spikes-inventory-postmove-2026-05-05.json`에 기록했다. README/AGENTS/DEVELOPER_INDEX/GUIDE/PUBLIC_RELEASE_BOUNDARY/ADR/verification ownership/root archive tests의 component/archive entry point는 archive path로 갱신했다.

## 판정

현재 repo migration의 physical archive move blocker는 pass 상태다. Active product `spikes/**` path는 0개이고, source `spikes/purecvisor-desktop-node/**` path는 absent다. 이후 route matrix blocked row 14개는 12개 current-native product operation과 2개 future implementation exclusion으로 정리됐고, tier2/tier3 fresh evidence와 stable internal release/update/rollback execution evidence는 `aggregate-gate-closure-2026-05-05.md`에서 closure 후보로 닫았다.
