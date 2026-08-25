# Aggregate Gate Closure Report - 2026-05-04

report_id: aggregate-gate-closure-2026-05-04
created_at: 2026-05-04T00:56:00+09:00
source_commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
route_matrix_commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
ga_scope_current_route_count: 18
ga_scope_product_operation_count: 24
future_route_exclusion_count: 0
transition_helper_count: 0
blocked_count: 14
powershell_current_owner_count: 0
powershell_fallback_count: 0
active_spikes_path_count: 0
component_archive_spikes_reference_count: 22
repo_migration_preflight_status: blocked
docs_command_update_status: pass
verification_ownership_replacement_status: pass
archive_readonly_rollback_evidence_status: pass
tier2_admin_evidence_status: blocked
tier3_admin_evidence_status: blocked
release_gated_prerelease_evidence_status: pass
lan_gated_preapproval_evidence_status: pass
stale_evidence_count: 0
waived_evidence_count: 0
waiver_only_gate_satisfaction_count: 0
aggregate_gate_status: blocked
machine_readable_json_created: no

## 입력

- route matrix: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- evidence ledger: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- repo migration preflight: `docs/ga-ready/evidence/repo-migration-preflight-2026-05-04.md`

## 계산 요약

- GA-scope row는 `current-route` 18개와 `product-operation` 24개, 총 42개다.
- `promotion_state = current-native` row는 28개다.
- `promotion_state = blocked` row는 14개다.
- `promotion_state = transition-helper` row는 0개다.
- PowerShell-backed current owner는 0개 row다.
- product execution fallback으로 `transition-helper`를 쓰는 row는 0개다.
- active `spikes/**` reference는 초기 61개다. 이후 route parity smoke import, MSI installer payload spike staging, standalone product wrapper asset staging, post-reboot active spike command, docs required verification command direct spike path, product wrapper protected-token spike service-module import, post-reboot repo boundary spike marker는 닫혔다. 현재 active product path 기준 `spikes/**` reference는 0개다.
- 남은 direct `spikes/purecvisor-desktop-node` reference 22개는 README/AGENTS/DEVELOPER_INDEX/follower/PUBLIC_RELEASE_BOUNDARY의 component/archive 문서 진입점이며 product execution, packaging input, required verification command, post-reboot active profile source가 아니다.
- release approval required row는 4개이며 pre-release evidence boundary는 pass다. 별도 release execution approval은 아직 없다.
- LAN exposure approval required row는 1개이며 pre-approval evidence boundary는 pass다. 이 header snapshot 시점에는 별도 LAN exposure approval이 없었다. 2026-05-05 후속 fast-mode opt-in에서 scoped LAN IP exposure smoke를 실행했지만, default LAN exposure와 stable/public release gate는 계속 별도 경계다.

## 차단 사유

aggregate_gate_status는 blocked다.

- blocked row가 14개 남아 있다.
- PowerShell-backed current owner row는 0개로 낮아졌지만, 이것만으로 aggregate gate가 닫히지는 않는다.
- active `spikes/**` product path는 0개로 재분류됐다. 단 physical `spikes/**` 파일 이동과 archive write는 실행하지 않았다.
- repo migration preflight는 파일 이동 미실행 때문에 blocked다.
- verification ownership replacement는 pass 상태다.
- archive/read-only rollback evidence는 inventory/hash proof까지 pass지만 파일 이동 evidence는 아니다.
- tier2/tier3 admin evidence가 모든 row에 대해 fresh evidence로 닫히지 않았다.
- release-gated pre-release evidence와 LAN-gated pre-approval evidence는 pass다. 2026-05-05 후속 scoped LAN/firewall/internal trust-store 실행 이후에도 stable release/update/rollback execution approval은 별도 gate로 남아 있다.

## 판정

ADR-0004는 current decision으로 승격할 수 없다. 현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`로 유지한다.

## 후속 Evidence 추가

2026-05-04T03:28:52+09:00에 `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`의 `0.30.3-admin-smoke`가 service/data-root lifecycle evidence를 추가했다. 이 evidence는 service 존재 중 `data-root-remove --remove-data` 차단, `remove-installed --remove-data` handoff-only, service absent 이후 allowlist data-root 삭제, MSI `REMOVE_DATA=1`, installed Hyper-V route smoke를 PASS로 확인한다. 당시 report의 aggregate_gate_status는 blocked였고, 이후 후속 slice로 일부 repo migration/docs command blocker를 줄였다.

2026-05-04T13:41:49+09:00에 repo migration active path removal 후속 slice가 route parity smoke의 spike service module import 제거에 이어 MSI installer payload staging에서도 active `spikes/**` source를 제거했다. MSI payload는 product wrapper, `DesktopNode.Host.exe`, repo-root `web/**`, manifest만 포함한다. 당시 aggregate_gate_status는 계속 blocked였고, 이후 후속 slice로 post-reboot profile, docs command, ownership map, archive criteria를 추가 정리했다.

2026-05-04T13:50:24+09:00에 post-reboot verification 후속 slice가 `HyperVNonIntegration` profile을 active product post-reboot profile에서 퇴역시켰다. `ProductStatus`, `PackagingRegression` profile의 command plan에는 active `spikes/**` command path가 없고, retired profile 요청은 `PCV_POST_REBOOT_PROFILE_RETIRED`로 실패한다. 당시 aggregate_gate_status는 계속 blocked였고, 이후 docs command update와 archive criteria를 추가 정리했다.

2026-05-04T13:55:17+09:00에 docs command update 후속 slice가 AGENTS, README, verification policy, public release boundary, follower의 기본 required verification command에서 direct spike Pester path를 제거했다. `docs_command_update_status`는 pass로 전환했지만 aggregate_gate_status는 계속 blocked다. Verification ownership replacement는 blocked로 남아 있다.

2026-05-04T13:59:24+09:00에 verification ownership map 후속 slice가 `docs/ga-ready/VERIFICATION_OWNERSHIP.md`의 default command owner와 component/archive baseline owner를 현행화했다. Legacy API/service/CLI/Hyper-V/root-boundary Pester는 component/archive baseline으로 남고, 기본 required command에서는 제외된다. 당시에는 equivalent coverage mapping이 남아 있어 `verification_ownership_replacement_status`가 blocked였다.

2026-05-04T14:03:00+09:00에 archive/read-only rollback 후속 slice가 `docs/ga-ready/evidence/archive-readonly-rollback-2026-05-04.md`를 추가했다. `archive/spikes/**`는 product execution, packaging input, required verification command, post-reboot product profile source로 사용할 수 없는 planned read-only target이며 rollback restore criteria는 git tracked restore, hash inventory, no behavior change evidence로 정의했다. 파일 이동과 archive write는 실행하지 않았으므로 aggregate_gate_status는 계속 blocked다.

2026-05-04T15:11:00+09:00에 standalone product wrapper asset boundary 후속 slice가 `PcvDesktopNodeProduct.psm1`의 product asset staging을 repo-root `web/**`로 축소했다. Product manifest와 copy output은 legacy `spikes/purecvisor-desktop-node/{api,hyperv,service}` component files를 포함하지 않는다. 당시 direct reference 재계산 값은 component/archive 문서와 tests 기준 30개였으며, standalone product asset spike source count는 0으로 닫혔다. 파일 이동, archive write, administrator host mutation은 실행하지 않았고 aggregate_gate_status는 계속 blocked였다.

2026-05-04T19:29:13+09:00에 사용자 관리자/host mutation opt-in으로 `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1515-0320`의 `0.32.0-admin-smoke`를 실행했다. Standalone product asset boundary 이후 active `spikes/**` product asset 없이 AllowUnsignedDev MSI를 빌드했고, service-action, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, installed Hyper-V route smoke가 PASS로 끝났다. Build commit은 `d852ff54bafb403e16e86057b3cecec2813bf0b6`, MSI SHA-256은 `f3e4456e94d5ee16a8e0bd6d02d17ac04d682be5bd58c77098072f97711d25f5`, final service는 `Running`, boot time은 unchanged, `pcv-spike-*` VM 잔여물은 없었다. 이 evidence는 unsigned `AllowUnsignedDev` admin-smoke이며 public trusted/stable signing 또는 GA 승격 evidence가 아니다. Aggregate gate는 public signing 제외 조건에서도 남은 blocked row, PowerShell-backed current owner, archive/read-only rollback, release/LAN gate 때문에 계속 blocked다.

2026-05-04T19:56:00+09:00에 Event Log source registration owner migration slice가 `DesktopNode.Host.exe service-action eventlog-register` code-level registry-backed action을 추가했다. 이 slice는 실제 Event Log source 등록/제거를 실행하지 않았고, fake controller/xUnit으로 missing source registration, foreign existing source block, external PowerShell command 미사용을 확인했다. Route matrix의 `Event Log source registration` row는 `current_owner = dotnet-native`, `fallback_policy = none`, `promotion_state = current-native`로 전환됐고, PowerShell-backed current owner count는 5로 낮아졌다. Event Log source removal, firewall enable/removal, trust store install/removal은 계속 PowerShell-backed 또는 blocked 후속 row로 남아 있으며 aggregate_gate_status는 계속 blocked다.

2026-05-04T20:04:27+09:00에 사용자 관리자/host mutation opt-in으로 `artifacts/eventlog-source-registration-20260504-actual-registry`의 실제 Event Log source registry 등록을 실행했다. `Application` log의 `PureCVisor Desktop Node` source는 `EventMessageFile=C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`, `TypesSupported=7`로 등록됐고, final service는 `Running`, `pcv-spike-*` VM 잔여물은 없었다. 이 실행은 Event Log source registry 등록만 수행했으며 service/MSI/Hyper-V/firewall/trust-store mutation, Event Log source removal, public trusted signing, stable publication은 실행하지 않았다. Aggregate gate는 다른 blocked row와 release/LAN/archive/repo migration blocker 때문에 계속 blocked다.

2026-05-04T20:21:52+09:00에 사용자 관리자/host mutation opt-in으로 `artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330`의 실제 Service/MSI/Hyper-V/firewall/trust-store mutation을 실행했다. `0.33.0-admin-smoke`는 commit `dca492c67c0cb3843832d5f6e1e76c8d686c3cdf` 기준 AllowUnsignedDev MSI를 빌드했고, MSI SHA-256은 `e6522114963be755beab1f54e183eef212a9f32979751e1fe67159a20cd2a4ff`, payload file count는 7이다. Service-action, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, installed Hyper-V route smoke는 PASS였고 final service는 `Running`, boot time은 unchanged, `pcv-spike-*` VM 잔여물은 없었다. Row-isolated firewall-only smoke는 owned inbound allow rule `PureCVisorDesktopNode-Smoke-20260504-FirewallOnly-20260504202105-03c22cf9`를 `TCP/47778`, `Private`, `LocalSubnet` scope로 create/enable/remove한 뒤 final rule count 0을 확인했다. Row-isolated trust-store-only smoke는 self-signed test certificate thumbprint `18FFB486CB56EBF6AD0C8B841ACF932FE482CACF`를 LocalMachine Root/TrustedPublisher에 import한 뒤 Root/TrustedPublisher/CurrentUser My에서 final absence를 확인했다. 이 evidence는 actual mutation evidence지만 public trusted/stable signing 또는 GA 승격 evidence가 아니며, firewall/trust-store current owner migration도 수행하지 않았다. Aggregate gate는 남은 blocked row, PowerShell-backed current owner, archive/read-only rollback, release/LAN gate 때문에 계속 blocked다.

2026-05-04T22:49:15+09:00에 Event Log source removal owner migration slice가 `DesktopNode.Host.exe service-action eventlog-remove` code-level registry-backed action을 추가했다. 이 slice는 실제 Event Log source 제거를 실행하지 않았고, fake controller/xUnit으로 owned source removal path, missing source idempotent success, external PowerShell command 미사용을 확인했다. Route matrix의 `Event Log source removal` row는 `current_owner = dotnet-native`, `fallback_policy = none`, `promotion_state = current-native`로 전환됐고, PowerShell-backed current owner count는 4로 낮아졌다. Firewall enable/removal과 trust store install/removal은 계속 PowerShell-backed 또는 blocked 후속 row로 남아 있으며 aggregate_gate_status는 계속 blocked다.

2026-05-04T23:18:00+09:00에 firewall/trust-store owner migration slice가 `DesktopNode.Host.exe service-action firewall-enable|firewall-remove|trust-store-install|trust-store-remove` code-level native action을 추가했다. Firewall action은 COM-backed Windows Firewall controller, trust-store action은 X509Store-backed certificate store controller를 사용하며 fake controller/xUnit으로 LAN approval gate, release approval gate, owned mutation path, foreign ownership block, missing removal idempotency, external PowerShell command 미사용을 확인했다. 이 slice는 실제 firewall rule enable/removal 또는 trust store install/removal을 실행하지 않았고, `0.33.0-admin-smoke`의 row-isolated actual mutation evidence는 이 owner migration 이전 commit 기준 historical evidence로만 남긴다. Route matrix의 firewall/trust-store 4개 row는 `current_owner = dotnet-native`, `fallback_policy = none`, `promotion_state = current-native`로 전환됐고, PowerShell-backed current owner count는 0으로 낮아졌다. 당시 aggregate gate는 남은 blocked row 14개, active `spikes/**` reference 30개, repo/verification/archive/read-only rollback, release-gated update/rollback/trust-store, LAN-gated firewall preapproval evidence 때문에 계속 blocked였다.

2026-05-04T23:37:43+09:00에 active `spikes/**` reference 재분류/감소 slice가 product wrapper protected-token preparation/health check에서 spike service module import를 제거하고 product wrapper 내부 DPAPI LocalMachine protected-token helper로 대체했다. Post-reboot repo boundary marker도 retired spike README 대신 `src/DesktopNode.Host/DesktopNode.Host.csproj`를 확인한다. Packaging README/installer README/ADR index/verification policy의 active command성 direct spike reference도 제거했다. 동일 범위 direct reference 재계산 값은 22개이며, 이 22개는 component/archive 문서 진입점으로 재분류됐다. Active product `spikes/**` path count는 0개, verification ownership replacement는 pass, archive/read-only rollback proof는 `docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json` hash inventory로 pass, release/LAN preapproval boundary는 pass다. Aggregate gate는 physical archive move 미실행, blocked row 14개, tier2/tier3 fresh evidence, release execution approval, LAN exposure approval 때문에 계속 blocked다.

2026-05-05T00:44:00+09:00 후속 fast-mode 관리자 opt-in 범위에서 `artifacts/os-mutation-gates-20260505-003459-0341`의 `0.34.1-admin-smoke` current native OS gate를 실행했다. MSI provenance commit은 `6f97a24aa2bdfacf33d7bd987559eb85e363e119`, follow-up firewall missing-rule lookup hardening commit은 `49a06acd3493066a10ec26fe541d5d8be1005c2b`, MSI SHA-256은 `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`다. MSI install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall/final restore는 모두 exit `0`였고 final service는 loopback-only `Running`으로 복구됐다. Native firewall enable/remove는 owned LAN rule `PureCVisor Desktop Node Local API LAN`을 `TCP/7777`, `Private`, `LocalSubnet` scope로 create/remove했고 final rule absence를 확인했다. LAN smoke는 `http://0.0.0.0:7777/` prefix가 Windows HttpListener에서 unsupported임을 기록한 뒤 LAN IP prefix `http://[redacted-private-endpoint]:7777/`에서 bearer token runtime policy `HTTP 200`을 확인했다. Native trust-store action은 기존 ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`를 install/remove/restore했고 final present 상태로 복구했다. Public trusted signing, stable publication, local payload update, rollback restore는 실행하지 않았다. Aggregate gate는 current native OS gate evidence 추가 후에도 physical archive move 미실행, blocked row 14개, tier2/tier3 full fresh evidence, stable release/update/rollback execution blocker 때문에 계속 blocked다.

2026-05-05T01:12:00+09:00 후속 physical archive move opt-in 범위에서 `spikes/purecvisor-desktop-node/**`를 `archive/spikes/purecvisor-desktop-node/**`로 `git mv` 이동했다. 이동 전 repo 내부 source/target absolute path를 확인했고, 이동 후 source path absent, archive file count 46개, pre-move target inventory match 46개를 `docs/ga-ready/evidence/archive-spikes-inventory-postmove-2026-05-05.json`에 기록했다. README/AGENTS/DEVELOPER_INDEX/GUIDE/PUBLIC_RELEASE_BOUNDARY/ADR/verification ownership/root archive tests의 component/archive entry point는 archive path로 갱신했다. 이 follow-up 이후 repo_migration_preflight_status와 archive_readonly_rollback_evidence_status의 physical move blocker는 pass로 전환됐지만, aggregate gate는 blocked row 14개, tier2/tier3 full fresh evidence, stable release/update/rollback execution blocker 때문에 계속 blocked다.
