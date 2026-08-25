# Manual-admin campaign 2026-08-06 0.42.69 -> 0.42.70

evidence_id: `manual-admin-campaign-2026-08-06-04269-04270`
result: `PASS`
scope: `manual-admin-package-pair-closure`
baseline_version: `0.42.69-admin-smoke`
target_version: `0.42.70-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260805-04269-04270`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260805-04269-04270-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260805-04269-04270/manual-admin-campaign-descriptor/summary.json`
baseline_msi_sha256: `7a3729224d4a66df9a28b9e8f4f2649949361d9ca66bfce34d04caed390e198b`
target_msi_sha256: `b28e18763ac01137039a9bcfafe0c151945304c8449e307b0412038d6726c86c`
update_zip_sha256: `72d7f2927e21b100f9fdc15ce8c2b4a7923a0577b84d5a58398fdb84a3c7e72a`
burn_bundle_sha256: `74315de83cccd7e315ac07c38e4fa3934ddc0ff58627a83b1128902c62d4b45d`
msix_v2_sha256: `8042a44680983e9f5c5d53cddbb3df8e07af6324039d9b8bd1793dbed8ac9c47`
host_mutation_performed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-campaign-20260805-04269-04270/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260805-04269-04270/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5099540`, UBR `169 -> 5386` | `artifacts/manual-admin-campaign-20260805-04269-04270/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260805-04269-04270/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260805-04269-04270/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260805-04269-04270/installed-runtime-ops-summary/summary.json` |

Descriptor `manual-admin-campaign-descriptor-20260805-04269-04270-closed`는
`runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass`로
닫혔다. 2026-05-29 `0.42.58 -> 0.42.59` 이후 `69`일 만의 첫 manual-admin closure다.

Installed update/rollback은 `0.42.69-admin-smoke -> 0.42.70-admin-smoke` update,
`0.42.70-admin-smoke -> 0.42.69-admin-smoke` rollback, final update를 모두 `ok=true`로
확인했고 최종 설치본은 `0.42.70-admin-smoke`다.

Clean-host는 Windows Server 2022 Evaluation base VHD에서 throwaway VM
`pcv-cleanhost-20260805-04269-04270`을 띄워 Windows Update 적용 후 baseline install,
catalog update, rollback을 실행했다. `install_exit_code=0`, `update_exit_code=0`,
`rollback_exit_code=0`, `final_web_status_code=200`, `blocker=none`이며 guest 최종
manifest는 baseline `0.42.69-admin-smoke`로 복원됐다. `RemoveVmOnSuccess`로 VM은
제거됐고 `token_value_observed=false`다.

## 2일에 걸친 실행 경계

campaign은 하루에 끝나지 않았다. runner 6종 중 5종은 2026-08-05 `19:24`-`20:09`에,
clean-host는 2026-08-06 `12:14`-`13:04`에 실행됐다.

`docs/ga-ready/evidence/manual-admin-campaign-readiness-2026-08-05-04269-04270.md`의
runner 표는 그 문서가 작성된 `19:33` 시점의 기록이다. 표에 `not-run`으로 적힌 runner 중
`installed-product-update-rollback`, `burn-install-repair-remove`,
`msix-build-install-update-remove`, `installed-runtime-ops-summary` 4종은 그 직후
같은 날 실행됐다. 해당 문서는 point-in-time readiness 기록으로 보존하고, 실행 결과는
이 문서가 소유한다.

## 기록된 blocker 중 실제가 아니었던 것

readiness 문서는 `WixToolset.Bal.wixext 5.0.2`의 `damaged` 상태를 Burn 번들 빌드 전
재설치가 필요할 수 있는 미해결 항목으로 남겼다. Burn runner는 이 확장을 쓰지 않는다.
`wix build ... -ext WixToolset.BootstrapperApplications.wixext`로 exit `0` 빌드했고
install/repair/remove/restore와 11개 check가 모두 통과했다. `Bal.wixext`의 `damaged`
상태는 여전하지만 이 campaign의 blocker가 아니었다.

이는 readiness 문서 §"세 번 반복된 관측 오류"가 기록한 패턴의 네 번째 사례다.

## 중단된 첫 clean-host 시도

2026-08-05 `20:05` clean-host 시도는 plan 파일만 남기고 중단됐다. `RemoveVmOnFailure`가
꺼져 있어 VM `pcv-cleanhost-20260805-04269-04270`이 남았고, 2026-08-06 `11:42` 호스트
재부팅 때 `AutomaticStartAction=StartIfRunning`으로 자동 재개됐다. 재실행 전에 이 VM과
VM root를 제거했다. Base VHD는 손상되지 않았다.

## 설치본과 anchor 정렬 상태

| 평면 | 현재 |
| --- | --- |
| 설치본 | `0.42.70-admin-smoke` |
| canonical anchor | `0.42.69-admin-smoke` |
| manual-admin closure | `0.42.69 -> 0.42.70` |

campaign의 `installed-product-update-rollback`이 호스트를 target에 남기므로 설치본은
`0.42.70`이다. anchor는 `0.42.69`에 머문다. anchor 승격에는 `0.42.70` 전용 full admin
host mutation gate와 installed current-card evidence가 필요하며 이 campaign은 둘 다
수행하지 않았다. 2026-05-29 선례에서 `0.42.59` anchor는
`full-admin-host-mutation-gate-20260529-04259`와 자체 current-card를 별도로 가졌다.

## Nonclaims

- `0.42.70-admin-smoke` anchor 승격을 주장하지 않는다. full admin host mutation gate와
  installed current-card는 실행하지 않았다.
- clean-host guest의 internal root certificate import는 수행되지 않았고
  (`imported=false`) baseline MSI는 `NotSigned`로 관측됐다. 이는 `AllowUnsignedDev`
  admin-smoke 범위다.
- public trusted signing과 external stable publication을 주장하지 않는다.
- winget submission은 `out-of-scope`다.

이 evidence는 internal admin-smoke package-pair evidence이며 public trusted signing 또는
외부 stable publication을 주장하지 않는다.
