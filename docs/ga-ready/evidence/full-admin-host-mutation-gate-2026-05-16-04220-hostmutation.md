# 전체 관리자 Host Mutation Gate - 2026-05-16 0.42.20

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04220-hostmutation
result: PASS
version: 0.42.20-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04220
host_mutation_performed: true
dry_run: false
public_trusted_signing: excluded
external_stable_publication: not-claimed
```

이 evidence는 `0.42.20-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Batch Supervisor가 Service/MSI/Hyper-V route parity와 OS mutation gate 두 단계를
elevated `-AllowHostMutation` 범위에서 실행했고, installed current-card smoke가 같은
batch를 최신 operational evidence로 표시함을 확인했다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260516-04220` |
| installed current-card artifact root | `artifacts/installed-current-card-20260516-04220-fullgate` |
| clean package root | `artifacts/admin-smoke-package-20260516-04220` |
| clean package MSI SHA-256 | `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f` |
| clean package payload aggregate SHA-256 | `d3a2d660b27daf7ee120a5a31cd11ab36fa3fde36ac0f76434d6ae5d6cfe4ed2` |
| full-gate MSI SHA-256 | `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c` |
| full-gate payload aggregate SHA-256 | `033c677ac5a6edecdb666073396b6cc9f41c78d95f3a2617a2c3c1d20f5c659d` |
| host SHA-256 | `d6e44f8b7fb3921453532c9900fec9c8f6b08f10813d07a33f2ca0c636674fa0` |
| CLI SHA-256 | `052ee5909e61de0e600750f78f256b54c6b6b680394c91ab93a4aa12b772c242` |
| TUI SHA-256 | `281b24a7fc27afa5dc17c14c03be038a165e1c2fd9da73b215718e6b08865ad0` |
| provenance commit | `0895d018935298721b25b5d9ce1ae083a6690c25` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`,
`executed_steps=2`다.

| Step | 결과 | Duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `ok=true`, exit `0`, attempt `1`, retry `1` | `103676 ms` |
| `os-mutation-gate` | `ok=true`, exit `0`, attempt `1`, retry `0` | `11093 ms` |

GPU snapshot은 총 `17`개가 current-card에서 확인됐고, batch timeout은 발생하지 않았다.

## Route/MSI/Hyper-V

Route parity summary는 `ok=true`, version `0.42.20-admin-smoke`,
`boot_time_unchanged=true`, final service `Running`, `remaining_pcv_vms=[]`다.
실행 bucket은 product MSI build, service-action smoke, MSI lifecycle smoke,
installed .NET Host Hyper-V API route smoke를 포함한다.

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

Installed current-card smoke는 `artifacts/installed-current-card-20260516-04220-fullgate`에
보존한다.

| 항목 | 값 |
| --- | --- |
| summary | `artifacts/installed-current-card-20260516-04220-fullgate/summary.json` |
| ops summary | `artifacts/installed-current-card-20260516-04220-fullgate/ops-summary.json` |
| installed runtime version | `0.42.20-admin-smoke` |
| service | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260516-04220` |
| latest release version | `0.42.20-admin-smoke` |
| latest release MSI SHA-256 | `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c` |
| route evidence status | `available` |
| OS mutation evidence status | `available` |
| descriptor batch id | `manual-admin-campaign-descriptor-20260516-04219-04220` |
| descriptor excluded from operational latest | `true` |

## Public Boundary Workflow

Public boundary workflow는 최초 재실행 run `25930077313`에서 GitHub
billing/spending-limit로 runner가 시작되지 못했다. 결제 수단 등록 이후 같은
`main` line의 merge commit `6e556e5199e796a8889a9dc47dc925db02c9cb45` 기준으로
다시 실행한 run `25933428239`는 `public-boundary-ci-required` job을 PASS했다.

| 항목 | 값 |
| --- | --- |
| pass evidence | `docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md` |
| latest run id | `25933428239` |
| latest run URL | `[private-archive-repository]/actions/runs/25933428239` |
| latest job id | `76232707240` |
| latest head SHA | `6e556e5199e796a8889a9dc47dc925db02c9cb45` |
| latest conclusion | `success` |
| public boundary guard executed | `true` |
| previous blocker run id | `25930077313` |
| previous blocker artifact | `artifacts/public-boundary-workflow-rerun-20260516-04220/summary.json` |
| previous blocker | `billing-or-spending-limit` |

이전 blocker annotation은 account payment 또는 spending limit 때문에 job이
시작되지 않았다고 보고했다. 이 실패는 historical GitHub account
billing/spending-limit blocker로 보존하며, 최신 public boundary guard 판단은 run
`25933428239` PASS가 소유한다.

## 결정

`0.42.20-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260516-04220`이다.
이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted
signing 또는 외부 stable publication evidence가 아니다. 이전
`0.42.18-admin-smoke` full gate와 `0.42.19 -> 0.42.20` manual-admin package-pair는
historical predecessor와 current package-pair evidence로 각각 보존한다.
