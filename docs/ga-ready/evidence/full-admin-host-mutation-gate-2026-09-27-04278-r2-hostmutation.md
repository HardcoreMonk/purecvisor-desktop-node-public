# Full admin host mutation gate `0.42.78-admin-smoke` r2 (2026-09-27)

evidence_id: `full-admin-host-mutation-gate-2026-09-27-04278-r2-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.78-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260927-04278-r2`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260927-04278-r2`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260927-04278-r2`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260927-04278-r2`
stale_productcode_cleanup_root: `artifacts/stale-productcode-cleanup-20260927-04278`
previous_limited_evidence: `full-admin-host-mutation-gate-2026-09-27-04278-hostmutation`
previous_failed_evidence: `full-admin-host-mutation-gate-2026-09-26-04278-hostmutation`
batch_summary_sha256: `ff15ffdbc864464e6a44e51c788f1836445bc920b43c294e128e0c869c62c792`
routeparity_summary_sha256: `c77af1cc81b530e765652505c293cba7c33151ee13b217e48d79964f215ccd9a`
os_summary_sha256: `4a3248f92376378bf1e4f4f04021b3c1241b68797d502c67d1f08a8d9fa5dcf3`
operational_fullgate_msi_sha256: `0856d07ee7576a1cd44ca18061e2c9351ddef95271adca0b7b0b319a02d278b6`
operational_fullgate_payload_aggregate_sha256: `2dfabb939317e4fd6a29b557d98a2d0f35a4197201b8173698737ff8f68ea4bb`
service_host_sha256: `51f67b6a968c6006eb5f8400061e78f8a9fd2e59f7a6142359a96689fdbe4faa`
cli_sha256: `cdf68bfa72f5d56b21fdddec6af1cac6ef674fecfcc27982bcecd0eebf1eaba1`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.78-admin-smoke+0de176f12cbfe2bc8f842396159e3c81dfbd3b9b`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_after: `1`
provenance_commit: `0de176f12cbfe2bc8f842396159e3c81dfbd3b9b`
signing_mode: `AllowUnsignedDev`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 정리

같은 날 첫 실행(`full-admin-host-mutation-gate-20260927-04278`)은 PASS였지만, 09-26 실행이
남긴 같은 version의 ProductCode 2개 때문에 설치본 Host/CLI가 교체되지 않았고
uninstall/remove-data가 공유 component를 제거하지 못했다. 운영자 승인으로 `0.42.78`
ProductCode 3개를 데이터 보존 `msiexec /x`로 제거한 뒤 이 r2를 실행했다.
`REMOVE_DATA`는 사용하지 않았다.

| ProductCode | InstallDate | exit |
| --- | --- | ---: |
| `{7E70DFC4-74D4-473D-9194-96EA40DC5C0D}` | `20260926` | `0` |
| `{A04313E0-73D2-4014-88EA-C66C27763FC6}` | `20260926` | `0` |
| `{57794CE8-BEF0-4799-9A4B-BAF435CC728D}` | `20260927` | `0` |

정리 후 ARP 항목 `0`, service absent, product root absent다. data root 파일 `9`개
(`accounts.json`, `api-token.dpapi.json`, `jobs.json`, `jwt-signing-key.txt` 등)는 정리 전후와
r2 이후 모두 같은 목록으로 유지됐다.

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `169.902s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.064s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다. retry는 발생하지 않았다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다. MSI lifecycle의
install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data,
final-restore-install은 모두 exit `0`이다. boot time `2026-09-27T00:02:22.5+09:00`는
변하지 않았다.

OS gate는 `11`개 step이 모두 `completed`다. final firewall rule count `0`,
Event Log source absent, internal Root/TrustedPublisher present다.

## 설치본 payload identity

| 관측 | 값 |
| --- | --- |
| 설치본 `DesktopNode.Host.exe` SHA-256 | `51f67b6a968c6006eb5f8400061e78f8a9fd2e59f7a6142359a96689fdbe4faa` (gate build와 같음) |
| 설치본 `pcvcli.exe` SHA-256 | `cdf68bfa72f5d56b21fdddec6af1cac6ef674fecfcc27982bcecd0eebf1eaba1` (gate build와 같음) |
| 설치본 ProductVersion | `0.42.78-admin-smoke+0de176f12cbfe2bc8f842396159e3c81dfbd3b9b` |
| ARP `0.42.78` 등록 | `1`개 (`{2A2AA89B-DAB2-4810-B19F-B1D5C53D6945}`, InstallDate `20260927`) |
| `install.log`, `final-restore-install.log`의 Host `Won't Overwrite` | `0`건 |
| `uninstall-remove-data.log`의 `another client exists` | `0`건 |

이 batch의 MSI lifecycle, Hyper-V route, OS gate는 gate build(`0de176f`) payload를
대상으로 실행됐다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-6103c2c6` |
| generation / switch | Gen2 / `Default Switch` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| unmanaged VM | `pcv-spike-api-foreign-ea512800` |
| unmanaged delete guard | `PCV_VM_NOT_MANAGED_BY_PURECVISOR` |
| routeparity `remaining_pcv_vms` | `[]` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 manifest | `0.42.78-admin-smoke` (DisplayVersion `0.42.78`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| `host.status` | `supported=true`, `reasons=[]`, `default_switch_present=true` |
| `network.inventory` | `ok=true`, `Default Switch` / `internal` |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| Web `/` / `/pcv-config.js` | HTTP `200` / HTTP `200` |
| TUI | `pcvtui.exe` absent |

## Provenance

Operational MSI는 gate 내부 build가 source commit `0de176f`에서 생성했다. `0de176f`와
`240c53e`는 모두 `e098e0a` 위의 docs-only commit이라 제품 source 차이는 없다. build마다
source commit이 달라 Host/CLI/MSI/payload aggregate hash가 다르고, product wrapper
SHA-256은 모두 같다.

| artifact | commit | MSI SHA-256 | payload aggregate | service host |
| --- | --- | --- | --- | --- |
| clean package | `e098e0a` | `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d` | `999f7106d6c63594f9e13d0d17dcfa97364f5b40ea4b86e28342776ef7b16ac1` | `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b` |
| 09-26 FAIL gate | `e098e0a` | `b8e579d75b150e843e62f6fbed130638bfd9a0d4ea4657477ce0b3222dd7d6f5` | `08774058b6ffb7fcbe8a5173c34fc645a4a6b7bbce482b98c635ca6aea446ebf` | `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b` |
| 09-27 첫 gate build | `240c53e` | `f54a2f1df6dc920aafa6f66aba81e13686cb8f7b17774ec9e225f071bcbf26af` | `191e6fa50537ef6c63aecbbbc3df626ee9b139078cca337384ab28036f7d42a6` | `62085ebee55795ef83edb783c33b22784e0ca0c52854ba421478cb2a4ce41b94` |
| 09-27 r2 gate build | `0de176f` | `0856d07ee7576a1cd44ca18061e2c9351ddef95271adca0b7b0b319a02d278b6` | `2dfabb939317e4fd6a29b557d98a2d0f35a4197201b8173698737ff8f68ea4bb` | `51f67b6a968c6006eb5f8400061e78f8a9fd2e59f7a6142359a96689fdbe4faa` |

## Report-only 관측

- 같은 version으로 gate를 다시 실행하면 gate 내부 build가 새 ProductCode를 만들고 기존
  `0.42.78` 등록과 공존해 첫 실행과 같은 제한이 재발한다. 근본 수정은 installer 설계
  결정이며 이 문서 범위 밖이다.
- 호스트에 남은 `pcv-guest-installed-04253-r1`(`Off`)는 이 gate가 만들지 않았고
  지우지 않았다.

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair와 Lane 3 current 승격은 이 문서가 소유하지 않는다.
- `docs/ga-ready/current-evidence.json`은 `0.42.77-admin-smoke`로 유지한다.
