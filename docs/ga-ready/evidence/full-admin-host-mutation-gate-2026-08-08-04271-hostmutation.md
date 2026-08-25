# Full admin host mutation gate `0.42.71-admin-smoke` (2026-08-08)

evidence_id: `full-admin-host-mutation-gate-2026-08-08-04271-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.71-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260808-04271`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260808-04271`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260808-04271`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260808-04271`
operational_fullgate_msi_sha256: `4748cc7453ac85178830c179533e7236ed4d3eb15ddb3f968e1dbd4934c27156`
operational_fullgate_payload_aggregate_sha256: `6f325c245808d5d3bb6ead60184cb9c0c2065d79552e22b673ba1be7a010ca16`
provenance_commit: `80f69f31464ce07b2c9eca19211adf1232ea75f6`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `194s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11s` |

batch `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id=null`,
`timed_out=false`.

`service-msi-hyperv-admin-smoke` 내부 단계:

| 내부 step | result |
| --- | --- |
| `build-current-admin-smoke-msi` | `completed` |
| `service-action-smoke` | `completed` |
| `msi-lifecycle-smoke` | `completed` |
| `installed-dotnet-host-hyperv-api-route-smoke` | `completed` |

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.71-admin-smoke` (DisplayVersion `0.42.71`) |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| `boot_time_unchanged` | `true` (양 단계, `2026-08-08 11:10:37` 유지) |
| 잔여 `pcv-spike-*` 검증 VM | `0` |
| `final_firewall_rule_count` | `0` |
| `final_eventlog_source_present` | `false` |
| internal trust root/publisher | present (restore 후) |

## 설치본 Hyper-V route 실행 확인

`installed-dotnet-host-hyperv-api-route-smoke` (`hyperv-api-route-smoke.json`, `ok=true`):

| 관측 | 값 |
| --- | --- |
| managed VM | `pcv-spike-api-a65efa87` |
| `vm_list_contains_vm` | `true` |
| `checkpoint_list_contains_checkpoint` | `true` |
| `checkpoint_restore_precondition` | `vm.poweroff-before-restore` |
| `vm_list_absent_after_delete` | `true` |
| unmanaged VM | `pcv-spike-api-foreign-209317b2` |
| `unmanaged_vm_created` | `true` |
| `unmanaged_vm_still_exists_after_block` | `true` |

## FC-12(b) 설치본 반영 확인 (부분)

| 검사 | 결과 |
| --- | --- |
| 설치본 `DesktopNode.Host.exe`에 `GuestArgvInvocation` 문자열 | **포함** |
| 설치본 Host SHA-256 | `2d3c077e6d8799d3636d9a037fdf33fa583957c0d0990cee15e0a3ed4a56995d` (clean package host와 동일) |
| 설치본 Windows guest credentialed guest-exec 왕복 smoke | **이 gate 범위 아님** (ISO Hyper-V route smoke만 실행) |

## Provenance

operational MSI는 gate 내부 `build-current-admin-smoke-msi`가 HEAD `80f69f31`에서 새로
빌드했다. Clean package
(`docs/ga-ready/evidence/admin-smoke-package-2026-08-08-04271.md`, MSI
`ebb621ada4…`)와는 별개 빌드이므로 MSI/payload aggregate hash가 다르다. 동일 source commit
기준이다.

| artifact | MSI SHA-256 | payload aggregate |
| --- | --- | --- |
| clean package | `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5` | `4a333d60c8f9e10ea4c356f58913e8893d43be644c4736e7ed272e03c3f5a0af` |
| operational fullgate | `4748cc7453ac85178830c179533e7236ed4d3eb15ddb3f968e1dbd4934c27156` | `6f325c245808d5d3bb6ead60184cb9c0c2065d79552e22b673ba1be7a010ca16` |

## 실행 방법

관리자 elevated `Invoke-PcvBatchSupervisor.ps1 -AllowHostMutation` with manifest
`artifacts/batch-runs/full-admin-host-mutation-gate-20260808-04271/batch-manifest.json`.

## Nonclaims

- internal `AllowUnsignedDev`/`LocalTest` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair `0.42.70 -> 0.42.71` closure는 이 gate가 닫지 않는다.
- 설치본 Windows guest credentialed `guest-exec` argv 왕복 재확인은 별도 smoke가 소유한다.
