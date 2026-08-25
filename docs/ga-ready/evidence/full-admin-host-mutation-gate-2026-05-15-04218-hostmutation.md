# 전체 관리자 Host Mutation Gate - 2026-05-15 0.42.18

```text
evidence_id: full-admin-host-mutation-gate-2026-05-15-04218-hostmutation
result: PASS
version: 0.42.18-admin-smoke
batch_id: full-admin-host-mutation-gate-20260515-163107-04218
host_mutation_performed: true
dry_run: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.18-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Batch Supervisor가 Service/MSI/Hyper-V route parity와 OS mutation gate 두 단계를
elevated `-AllowHostMutation` 범위에서 실행했고, installed current-card smoke가
같은 batch를 최신 operational evidence로 표시함을 확인했다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260515-163107-04218` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260515-163107-04218` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260515-163107-04218` |
| installed current-card artifact root | `artifacts/installed-current-card-20260515-04218-fullgate` |
| clean package root | `artifacts/admin-smoke-package-20260515-04218` |
| clean package MSI SHA-256 | `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af` |
| clean package payload aggregate SHA-256 | `156497b8a21a2bada24bc5af7f9ea73c60a3a4b0b3cdd0674f0086eed321077b` |
| full-gate MSI SHA-256 | `0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09` |
| full-gate payload aggregate SHA-256 | `011156b7839f565db927fe3792d7db62401f7cd0879d54b55e730ba16e20ca03` |
| host SHA-256 | `140a7b9bb0db3885bf3d63b3dec30f1e41abb04588f66325f7704be4c005e497` |
| CLI SHA-256 | `8c4442d2f7841414f0da994e74beefaf837cfe3fa3f6e0af1233e9318bcebf42` |
| TUI SHA-256 | `bb130aeac8e383ea7ff039c8fb3a2c62d7f62f4436845443b68752ce071eadc9` |
| provenance commit | `9121d1f5e7fa83d803c484a44698d4fc8e825c19` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`,
`executed_steps=2`다.

| Step | 결과 | Duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `ok=true`, exit `0`, attempt `1`, retry `1` | `78445 ms` |
| `os-mutation-gate` | `ok=true`, exit `0`, attempt `1`, retry `0` | `11064 ms` |

GPU snapshot은 총 `13`개가 수집됐고, batch timeout은 발생하지 않았다.

## Route/MSI/Hyper-V

Route parity summary는 `ok=true`, version `0.42.18-admin-smoke`,
`boot_time_unchanged=true`, final service `Running`, `remaining_pcv_vms=[]`다.
실행 bucket은 product MSI build, service-action smoke, MSI lifecycle smoke,
installed .NET Host Hyper-V API route smoke를 포함한다. Publish args는 Host/CLI/TUI
모두 `--self-contained true`를 사용했다.

## OS Mutation Gate

OS mutation summary는 `ok=true`, `actual_execution=completed`,
`host_mutation_performed=true`, `boot_time_unchanged=true`다.

| 최종 상태 | 값 |
| --- | --- |
| final service | `Running` |
| firewall final rule count | `0` |
| Event Log source present | `false` |
| internal trust Root present | `true` |
| internal trust TrustedPublisher present | `true` |
| LAN smoke prefix | `http://[redacted-private-endpoint]:7777/` |

## Installed Current Card

Installed current-card smoke는 `artifacts/installed-current-card-20260515-04218-fullgate`에
보존한다.

| 항목 | 값 |
| --- | --- |
| summary | `artifacts/installed-current-card-20260515-04218-fullgate/summary.json` |
| ops summary | `artifacts/installed-current-card-20260515-04218-fullgate/ops-summary.json` |
| installed runtime version | `0.42.18-admin-smoke` |
| service | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260515-163107-04218` |
| latest release version | `0.42.18-admin-smoke` |
| latest release MSI SHA-256 | `0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09` |
| route evidence status | `available` |
| OS mutation evidence status | `available` |
| descriptor batch id | `manual-admin-campaign-descriptor-20260515-04216-04218` |
| descriptor excluded from operational latest | `true` |

## 결정

`0.42.18-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260515-163107-04218`이다.
이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted
signing 또는 외부 stable publication evidence가 아니다. 이전
`0.42.16-admin-smoke` full gate는 historical predecessor로 보존한다.
