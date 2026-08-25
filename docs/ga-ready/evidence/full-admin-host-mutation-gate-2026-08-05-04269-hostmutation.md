# Full admin host mutation gate `0.42.69-admin-smoke` (2026-08-05)

evidence_id: `full-admin-host-mutation-gate-2026-08-05-04269-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.69-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260805-04269`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260805-04269`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260805-04269`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260805-04269`
operational_fullgate_msi_sha256: `07e30ca90d96747f5cc5f5e76a2a2556198356cf51db354356f795d9d3cc1a3a`
operational_fullgate_payload_aggregate_sha256: `d0ee0d0593f28603fd59daa90e9fa7a2fd24316f957423f5393db4f82d730db3`
provenance_commit: `7236b813d6a4f594abb8e126b2b5dfb2ad56c1e9`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 실행 결과

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `433s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11s` |

두 단계 모두 재시도 없이 통과했고 batch `ok=true`, `executed_steps=2`다.

## 최종 호스트 상태

| 항목 | 값 |
| --- | --- |
| 설치본 버전 | `0.42.68-admin-smoke` -> `0.42.69-admin-smoke` |
| service | `PureCVisorDesktopNode` `Running` / `Automatic` |
| `boot_time_unchanged` | `true` (양 단계) |
| 잔여 pcv 검증 VM | `0` |
| `final_firewall_rule_count` | `0` |
| `final_eventlog_source_present` | `false` |

기존 VM `pcv-guest-installed-04253-r1`은 조작하지 않았다.

## FC-13 제품 경로 재확인

`service-msi-hyperv-admin-smoke`의 `installed-dotnet-host-hyperv-api-route-smoke` 단계가 설치된
`0.42.69` 서비스로 실제 Hyper-V VM을 생성·조작했다. 같은 날 수정한 Gen2 boot order와 Secure Boot
템플릿이 설치본 경로에서 동작함을 이 gate가 확인한다. 코드 레벨 관측이 아니라 설치된 제품의
실행 결과다.

## Nonclaims

- 이 evidence는 internal `AllowUnsignedDev`/`LocalTest` admin-smoke 범위다.
- public trusted signing, trusted timestamp, external stable publication을 주장하지 않는다.
- manual-admin package-pair closure는 이 gate가 닫지 않는다.
