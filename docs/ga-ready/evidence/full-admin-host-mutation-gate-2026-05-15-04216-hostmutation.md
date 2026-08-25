# 전체 관리자 Host Mutation Gate - 2026-05-15 0.42.16

```text
evidence_id: full-admin-host-mutation-gate-2026-05-15-04216-hostmutation
scope: service-msi-hyperv-firewall-lan-eventlog-truststore-installed-current-card
result: PASS
version: 0.42.16-admin-smoke
batch_id: full-admin-host-mutation-gate-20260515-133741-04216
host_mutation_performed: true
dry_run: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

이 evidence는 `0.42.16-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Service/MSI/Hyper-V route parity, firewall/LAN/Event Log/internal trust-store OS
mutation gate, installed listener current-card smoke가 PASS였다. 이 evidence는 내부
사설망 admin-smoke 증거이며 public trusted signing 또는 외부 stable publication을
주장하지 않는다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260515-133741-04216` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260515-133741-04216` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260515-133741-04216` |
| full-gate MSI SHA-256 | `f1aaa6063bae67841cd99139d04fe4651d47b11fbe57f63b70aae3a790900f34` |
| full-gate payload aggregate SHA-256 | `e92e48ae035df781f25641968b24292a0096518ac824d961c1ddec1a05856ec9` |
| clean package root | `artifacts/admin-smoke-package-20260515-04216` |
| clean package MSI SHA-256 | `8b67c774f5d986c90749f494cc2084626d5bdf63904d3f9dd26b9b5daadde366` |
| clean package payload aggregate SHA-256 | `79e48f2f0b98e72d43bd0b02a3a530df62956a9a9802596109a027da6d89f950` |
| provenance commit | `29197ca7e269d2be9a8fe3f645c004819738838f` |
| product wrapper SHA-256 | `5ba0708413d863e356b166a69ab8e4ae43f26d9609d65b7a3b9cce13f6344c33` |
| service host SHA-256 | `ed3c987f0d2675ddde53f07ed9305af55b4b885e11cba0ef6140211551e8bb4d` |
| CLI SHA-256 | `1c8113d56103145acfd35d0936594ddbb234a450a2924ddc58aea7a8b006264b` |
| TUI SHA-256 | `5d7bd7121920da145a7f6e8e81a13829e651fb7f504e9976bb9bf55f0b5f1d55` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

- Batch summary: `ok=true`, `status=completed`, `executed_steps=2`.
- Route parity step duration: `84801` ms.
- OS mutation step duration: `11057` ms.
- GPU snapshot count: `14`.
- Route summary: `ok=true`, version `0.42.16-admin-smoke`, host boot time unchanged,
  `remaining_pcv_vms=[]`.
- OS mutation summary: `ok=true`, firewall final rule count `0`, Event Log source
  present `false`, internal trust root/publisher present, LAN prefix
  `http://[redacted-private-endpoint]:7777/`.

## Installed Current Card

Installed service current-card smoke는
`artifacts/installed-current-card-20260515-04216-fullgate`에 보존한다. 초기 repair에서
상대 `BatchEvidenceRoot=artifacts`는 service working directory 기준으로 사용할 수
없어, 절대 경로
`D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts`로 repair 후 다시
확인했다.

| 항목 | 값 |
| --- | --- |
| summary | `artifacts/installed-current-card-20260515-04216-fullgate/summary.json` |
| ops summary | `artifacts/installed-current-card-20260515-04216-fullgate/ops-summary.json` |
| installed runtime version | `0.42.16-admin-smoke` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260515-133741-04216` |
| latest release version | `0.42.16-admin-smoke` |
| latest route evidence status | `available` |
| latest OS mutation evidence status | `available` |
| descriptor batch id | `manual-admin-campaign-descriptor-20260515-04215-04216` |
| descriptor excluded from operational latest | `true` |
| service path has batch evidence root | `true` |
| service state | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| runtime policy unauthenticated boundary | HTTP `401` / `PCV_AUTH_REQUIRED` |

## Decision

`0.42.16-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260515-133741-04216`이다.
Descriptor batch는 operational latest 후보에서 제외되는 상태를 유지한다.
