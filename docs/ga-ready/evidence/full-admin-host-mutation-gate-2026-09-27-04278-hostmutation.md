# Full admin host mutation gate `0.42.78-admin-smoke` rerun (2026-09-27)

evidence_id: `full-admin-host-mutation-gate-2026-09-27-04278-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.78-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260927-04278`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260927-04278`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260927-04278`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260927-04278`
previous_failed_evidence: `full-admin-host-mutation-gate-2026-09-26-04278-hostmutation`
batch_summary_sha256: `96b35b8fda56bc9b67dd12d6f2ee0d67f49c55e3ffc579b704baaa2a08102f41`
routeparity_summary_sha256: `d184ba5aab534c1882a01eff3df0bd80bb5659c19ffd1118b9e87dfec72b73e9`
os_summary_sha256: `ff43d87a581e2668aec02794e6342c3a6c160a3c23c95b250150e8b2e6ba8879`
operational_fullgate_msi_sha256: `f54a2f1df6dc920aafa6f66aba81e13686cb8f7b17774ec9e225f071bcbf26af`
operational_fullgate_payload_aggregate_sha256: `191e6fa50537ef6c63aecbbbc3df626ee9b139078cca337384ab28036f7d42a6`
service_host_sha256: `62085ebee55795ef83edb783c33b22784e0ca0c52854ba421478cb2a4ce41b94`
cli_sha256: `0e026d12cc3fcc7a01d7a6a0f67a0b82e4c3ddd205a1a0c317abe487bb8d79e7`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
provenance_commit: `240c53e8984787e13ed586bf005f2c2a35767c7a`
signing_mode: `AllowUnsignedDev`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `103.192s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.070s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`,
`failed_step_id=null`, 양 step `timed_out=false`다. retry는 발생하지 않았다.

`service-msi-hyperv-admin-smoke`의 build, service action, MSI lifecycle,
installed .NET Host Hyper-V API route 단계가 모두 `completed`다. boot time
`2026-09-27T00:02:22.5+09:00`는 변하지 않았다.

OS gate는 config migration apply(service running), Event Log register/remove,
firewall enable/remove, LAN listener IP smoke, existing internal trust-store
export/install/remove/restore가 PASS다. final firewall rule count `0`,
Event Log source absent, internal Root/TrustedPublisher present다.

## 09-26 FAIL과의 관계

`full-admin-host-mutation-gate-20260926-04278`는 두 attempt 모두
`GET /api/v1/network/inventory`에서 `PCV_NETWORK_INVENTORY_FAILED`로 멈췄다.
원인은 제품 코드가 아니라 개발 호스트 설정이었다. vmswitch 계열 드라이버 시작 유형이
원래 값으로 복원되지 않은 상태였다.

| driver | 09-26 값 | 원래 값 |
| --- | --- | --- |
| `VmsProxy` | demand(3) | boot(0) |
| `VMSNPXY` | demand(3) | boot(0) |
| `VMSP` | demand(3) | auto(2) |

운영자 승인으로 세 값을 원래대로 되돌리고 `2026-09-27T00:02:22+09:00`에 재부팅했다.
재부팅 후 `Microsoft-Windows-Hyper-V-VmSwitch` id `4`(초기화 실패)는 `0`건,
id `9`(초기화 성공)는 `3`건이다. `Get-VMSwitch`, WMI `Msvm_VirtualEthernetSwitch`,
`pcvcli network inventory`가 모두 `Default Switch`를 반환한 뒤 이 gate를 실행했다.

제품 source와 gate 구성은 바꾸지 않았다. manifest는 09-26 실행과 날짜 stamp만 다르다.
09-26 FAIL evidence와 batch artifact는 덮어쓰지 않고 보존한다.

## 설치본 Hyper-V route

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-eff3ba68` |
| generation / switch | Gen2 / `Default Switch` |
| checkpoint restore precondition | `vm.poweroff-before-restore` |
| unmanaged VM | `pcv-spike-api-foreign-8fc2bfd4` |
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

Operational MSI는 gate 내부 build가 source commit `240c53e`에서 생성했다.
`240c53e`는 `e098e0a` 위의 docs-only commit이다. clean package
`admin-smoke-package-2026-09-25-04278`와 09-26 FAIL gate는 Host/CLI SHA-256이 같고,
이번 gate build는 source commit이 달라 Host/CLI/MSI/payload aggregate hash가 다르다.
product wrapper SHA-256은 세 artifact가 같다.

| artifact | MSI SHA-256 | payload aggregate | service host |
| --- | --- | --- | --- |
| clean package | `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d` | `999f7106d6c63594f9e13d0d17dcfa97364f5b40ea4b86e28342776ef7b16ac1` | `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b` |
| 09-26 FAIL gate | `b8e579d75b150e843e62f6fbed130638bfd9a0d4ea4657477ce0b3222dd7d6f5` | `08774058b6ffb7fcbe8a5173c34fc645a4a6b7bbce482b98c635ca6aea446ebf` | `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b` |
| 09-27 operational fullgate | `f54a2f1df6dc920aafa6f66aba81e13686cb8f7b17774ec9e225f071bcbf26af` | `191e6fa50537ef6c63aecbbbc3df626ee9b139078cca337384ab28036f7d42a6` | `62085ebee55795ef83edb783c33b22784e0ca0c52854ba421478cb2a4ce41b94` |

## 설치본 payload identity 제한

gate 이후 read-only current-card 캡처에서 설치본 Host/CLI가 이번 gate build가 아니라
`e098e0a` build임을 확인했다.

| 관측 | 값 |
| --- | --- |
| 설치본 `DesktopNode.Host.exe` SHA-256 | `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b` |
| 설치본 `pcvcli.exe` SHA-256 | `9222ef938873bf5e05e12f8b5af9be30cf0e2f72602309d12dcaf8669748ade7` |
| 설치본 ProductVersion | `0.42.78-admin-smoke+e098e0a55333afe7eccd9150c5ef9ca14578cc40` |
| 이번 gate build ProductVersion | `0.42.78-admin-smoke+240c53e8984787e13ed586bf005f2c2a35767c7a` |
| FileVersion (양쪽) | `1.42.78.0` |
| 설치본 Host/CLI LastWriteTime | `2026-09-26 03:46` |
| ARP `0.42.78` 등록 | `3`개 (InstallDate `20260926` 2개, `20260927` 1개) |

MSI log 근거는 다음과 같다.

- `install.log`, `final-restore-install.log`: `DesktopNode.Host.exe`가
  `Won't Overwrite; Existing file is of an equal version`이다.
- `uninstall-remove-data.log`: `Disallowing uninstallation of component ... since another
  client exists`가 `16`건이다.

09-26 실행이 남긴 같은 version의 ProductCode 2개가 공유 component를 계속 소유해서,
이번 MSI의 install은 파일을 교체하지 않았고 uninstall/remove-data는 공유 component를
제거하지 않았다. 따라서 이 batch의 Hyper-V route와 OS gate는 `e098e0a` build
(clean package와 같은 Host/CLI hash)를 대상으로 실행됐다. `240c53e`는 docs-only
commit이라 source 차이는 없지만, 이 host에서 MSI lifecycle의 uninstall/remove-data
증명은 제한된다. 이 제한을 해소하려면 잔여 ProductCode 정리와 gate 재실행이 필요하며,
둘 다 별도 관리자 승인 대상이다.

## Report-only 관측

- 호스트에 남은 `pcv-guest-installed-04253-r1`(`Off`)는 이 gate가 만들지 않았고
  지우지 않았다.

## Nonclaims

- internal `AllowUnsignedDev` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair, installed current-card, Lane 3 current 승격은 이 문서가
  소유하지 않는다.
- `docs/ga-ready/current-evidence.json`은 `0.42.77-admin-smoke`로 유지한다.
