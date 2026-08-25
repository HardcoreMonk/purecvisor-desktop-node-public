# 2026-05-12 Post-0426 MANUAL-ADMIN 후속 Triage

```text
evidence_id: post-0426-manual-admin-followup-triage-2026-05-12
scope: post-merge-provenance-batch-descriptor-and-next-gate-triage
result: PASS
actual_execution: post-merge-package-build-descriptor-linkage-and-approved-0427-host-mutation
host_mutation_performed: true
next_admin_smoke_package_build_decision: executed-0.42.7-admin-smoke
next_full_admin_host_mutation_gate_decision: executed-0.42.7-admin-smoke
descriptor_batch_manifest: manual-admin-campaign-descriptor-20260512-0425-0426
dashboard/wiki current card: installed-listener-batch-evidence-available
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

이 문서는 `manual-admin-campaign-2026-05-12-0425-0426` merge 이후 남은 후속
판정을 정리한다. 이번 follow-up은 merge commit 기준 package provenance rebuild,
Batch Supervisor manifest와 manual-admin descriptor의 직접 연결, 0423→0424 blocker의
historical-only 재분류, 다음 full admin host mutation gate 실행 여부 triage를 먼저
수행했다. 이후 사용자 승인에 따라 `0.42.7-admin-smoke` package build, full admin
host mutation gate, installed listener current-card smoke를 실행했다.

## 병합 후 패키지 Provenance

`0.42.6-admin-smoke` package를 merge commit 기준으로 다시 빌드했다.

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260512-0426-postmerge` |
| version | `0.42.6-admin-smoke` |
| MSI | `PureCVisorDesktopNode-0.42.6-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16` |
| provenance commit | `37f4d6b83d6caef1338e0a60e5df0a60209b51f8` |
| signing mode | `AllowUnsignedDev` |
| public trusted signing | `not-claimed` |
| external stable publication | `not-claimed` |

이 rebuild는 merge commit provenance를 맞춘 package input이다. 이미 PASS로 닫힌
`0.42.5 -> 0.42.6` lifecycle campaign의 runner evidence를 소급 교체하지 않는다.
이 package를 release/update/rollback 또는 full host mutation evidence로 주장하려면
별도의 elevated operator campaign이 필요하다.

## `0.42.7-admin-smoke` Package Build 판단

초기 follow-up에서는 packaging helper와 문서/current-card contract 정리만 수행했기
때문에 build를 보류했다. 이후 사용자 승인으로 `0.42.7-admin-smoke` build를 실행하고
그 package 기준 full admin host mutation gate까지 닫았다.

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260512-0427` |
| package build MSI SHA-256 | `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9` |
| provenance commit | `8d6aea7bac30ce279093ec61406c62428f69e79c` |
| signing mode | `AllowUnsignedDev` |

## 전체 관리자 Host Mutation Gate 결과

이 섹션은 사용자 승인 직후 실행한 `0.42.7-admin-smoke` full admin host mutation
PASS를 이력으로 기록한다. 현재 latest/current full admin host mutation PASS는
`0.42.8-admin-smoke` /
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md`가
소유한다.

| 항목 | 값 |
| --- | --- |
| batch id | `full-admin-host-mutation-gate-20260512-181309-0427` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427/summary.json` |
| route artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427` |
| full-gate MSI SHA-256 | `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4` |
| installed listener current card | `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427` |

`0.42.6-admin-smoke` post-merge rebuild는 provenance-aligned package input으로
보존한다. 당시 full gate claim은 0427 evidence가 소유했고, 이후 current claim은
0428 evidence가 소유한다. 다음 full gate도 새 version 기준 `-AllowHostMutation`
elevated campaign에서만 실행한다.

## Batch Supervisor Descriptor 연결

Batch Supervisor는 `ManualAdminCampaignDescriptor` profile을 지원한다. 이 profile은
`New-PcvManualAdminCampaignDescriptor.ps1 -PlanOnly`를 non-mutating step으로 실행해
manual-admin runner summaries와 descriptor generation을 같은 manifest contract 안에
넣는다.

다음 manifest는 helper로 생성한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1 -RepoRoot (Resolve-Path .).Path -PassThru
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/manual-admin-campaign-descriptor-20260512-0425-0426/manifest.json -DryRun
```

- batch id: `manual-admin-campaign-descriptor-20260512-0425-0426`
- manifest path: `artifacts/batch-runs/manual-admin-campaign-descriptor-20260512-0425-0426/manifest.json`
- profile: `ManualAdminCampaignDescriptor`
- generated step: `manual-admin-campaign-descriptor`
- dry-run result: `ok=true`, `dry_run=true`, `total_steps=1`
- host mutation: `requires_admin=false`, `mutates_host=false`

이 profile은 `requires_admin=false`, `mutates_host=false`이며 install/update/rollback,
Hyper-V VM, firewall, Event Log, trust store, service restart를 수행하지 않는다. 실제
lifecycle runner는 계속 `MANUAL-ADMIN` bucket에 남긴다.

## Dashboard/Wiki Current Card 동기화

Web Console evidence card는 정적 문서가 아니라 `GET /api/v1/ops/summary`의
`data.batch_evidence.latest`를 읽는다. 설치본 listener smoke는 dashboard/wiki
current card를 `installed-listener-batch-evidence-available`로 판정했다.

- Web Console: `batch_evidence.latest`가 가리키는 Batch Supervisor summary를 현재 카드로
  표시한다. 설치본 smoke에서 `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`와 child route/OS evidence `available`을 확인했다.
- GA-ready docs: `docs/ga-ready/EVIDENCE_INDEX.md`와 `docs/ga-ready/CONTROL_PLANE_INDEX.md`
  는 0427 full admin host mutation PASS와 0425→0426 package-pair PASS를 같은 current card로 묶는다.
- Zone wiki: 이 workspace에는 `/data/projects/codex-zone/wiki/index.md`가 없어 별도 wiki
  파일을 수정하지 않았다. Wiki가 복구되면 같은 current card text를 이 evidence와
  `CONTROL_PLANE_INDEX.md`에서 가져간다.

## Historical-only 재분류

`manual-admin-campaign-2026-05-12-0423-0424`는 historical-only blocker record로 낮춘다.
해당 campaign의 PASS bucket은 보존하지만 current package-pair claim은
`manual-admin-campaign-2026-05-12-0425-0426`가 소유한다.

- 0423→0424 status: `historical-partial-pass-clean-host-blocked`
- blocker: baseline `0.42.3-admin-smoke` MSI custom action sequence
- current replacement: `0.42.5 -> 0.42.6` package-pair PASS
- current descriptor: `artifacts/manual-admin-campaign-20260512-0425-0426/manual-admin-campaign-descriptor/summary.json`

## 판정

후속 1-2-3-4-5는 PASS다. Current package-pair는 0425→0426 PASS evidence를 기준으로
보고, post-merge rebuild는 다음 campaign input 후보로만 보존한다. `0.42.7-admin-smoke`
package build와 full admin host mutation gate는 사용자 승인 후 실행 완료됐으며,
installed listener `batch_evidence.latest` current-card smoke도 `available`로 닫혔다.
