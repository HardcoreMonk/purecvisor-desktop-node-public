# Release train pair orchestrator 리허설 `0.42.88-admin-smoke` -> `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289`
result: `PARTIAL_PASS_WITH_OBSERVATION_DEFECT`
evidence_scope: `internal-admin-smoke-only`
design: `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3b
orchestrator_status: `pass`
train_facts_status: `partial-blocked-by-observation-defect`
campaign_id: `manual-admin-campaign-20261004-04288-04289-r3b`
campaign_root: `artifacts/manual-admin-campaign-20261004-04288-04289-r3b`
closed_descriptor_batch_id: `manual-admin-campaign-20261004-04288-04289-r3b-closed`
fullgate_batch: `full-admin-host-mutation-gate-20261004-04289-r3b`
current_card_root: `artifacts/installed-operator-surface-current-card-20261004-04289-r3b`
current_card_summary_sha256: `d42d97fc49cbd0eeb357470022fcae8afa7ed17855ee13b8dc246847f9c13b41`
train_facts_root: `artifacts/train-facts-rehearsal-20261004-r3b`
installed_product_version: `0.42.89-admin-smoke+c67d22b77fe991aa77b7133cf581e22b302fc488`
host_mutation_performed: `true`
guest_credential_recorded: `false`
promotion_evidence: `false`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

release train 3단계 3b 리허설이다(사용자 승인 `3b 리허설 승인`, 2026-10-04). 0.42.89 train의 package 두 개(`admin-smoke-package-20261003-04288`, `admin-smoke-package-20261004-04289`)와 3a 도구가 만든 update ZIP·catalog(`artifacts/update-package-check-20261004/`)로 `Invoke-PcvManualAdminPackagePairCampaign.ps1`를 처음 실제로 돌렸다. 그 뒤 fullgate로 0.42.89 fullgate build를 다시 설치하고, current-card를 캡처하고, 실제 출력으로 `pcvverify train-facts`를 돌렸다. 결과는 승격 근거가 아니다.

guest 인증 정보는 clean-host runner 기본값을 실행 경계에서 `PSCredential`로 만들어 넘겼고 명령줄, artifact, evidence에 남기지 않았다.

## orchestrator 실행

`-PlanOnly` `ok=true` 뒤 `-Execute`를 `2026-10-04T06:26:34Z`부터 `252`초 돌렸다. 설치본이 target `0.42.89`(fullgate build `+a780928`)였으므로 orchestrator가 baseline catalog로 `0.42.88`에 먼저 맞췄다. dedicated host reservation을 만들고 끝에 소비했다(`reservation-consumed.json`).

| bucket | 결과 | 시간 | 0.42.89 수동 pair와 비교 |
| --- | --- | ---: | --- |
| readiness | `PASS`, `reserved-and-matched`, `actual_execution_eligible=true` | `2`초 | 수동은 `-PlanOnly`(`reservation-required-before-actual-execution`). 입력 판정 `ready-current-baseline-target-package-pair`는 같다 |
| update/rollback | `PASS`, Update `0.42.89` → Rollback `0.42.88` → Update `0.42.89` | `10`초 | 수동은 payload `-SourceRoot`로 Update → Rollback이고 baseline으로 끝났다. orchestrator는 catalog로 돌고 target으로 끝난다 |
| clean-host | `PASS`, install/update/rollback exit `0`, UBR `5622`, Windows Update 대상 `0`개, PowerShell Direct 시도 `2`, VM `pcv-cleanhost-20261004-r3b` 삭제 | `156`초 | 같다 |
| Burn | `PASS`, build/install/repair/remove/restore `0`, restoration `PASS` | `37`초 | 같다. 수동의 사전 제품 Update와 15초 대기가 없다 |
| MSIX | `PASS`, pack/sign/verify `0`, cleanup `PASS`, manifest 변화 없음 | `24`초 | 같다. 전체 version 문자열을 넘겨도 runner가 `-admin-smoke`를 떼어 세 자리로 쓴다 |
| runtime ops | `PASS`, `ops.summary`, stderr `0` byte, errors `0`, VM `1`, token 형태 `0`, 비인증 `401 PCV_AUTH_REQUIRED` | `3`초 | 수동은 baseline 상태에서 캡처했다. orchestrator는 target 상태에서 캡처한다 |

closed descriptor는 `overall_status=pass`, runner `6/6`, missing `0`, not-pass `0`, next candidate `0.42.89-admin-smoke`다. restoration은 필요 없었다. 끝난 뒤 설치본은 clean package `0.42.89`(Host `+b463903`), ARP `{0E27390F-5A08-4F76-90FD-7694B41672A4}` 1개, service Running/Auto였다.

## 발견: 관측 기록 결함

`observations.json` `16`개 항목 모두에 `observation_error=PropertyNotFoundException`가 있다. orchestrator는 `Set-StrictMode -Version Latest`로 돈다. 관측 함수가 Uninstall 키를 읽을 때 `DisplayName`이 없는 키에서 `$_.DisplayName`을 읽어 예외가 났다. 그래서 ARP 목록만 빠졌다. 나머지 필드(manifest, Host version, previous/failed, service, Web, boot time, firewall, VM)는 기록됐다. 3a에서 관측 함수를 strict mode 없이 떼어 확인한 탓에 놓쳤다.

같은 조건에서 `$_.PSObject.Properties['DisplayName']`을 먼저 보면 통과하는 것을 strict mode probe로 확인했다. 고치는 일은 Lane 1이고 이 리허설 campaign 밖이라 하지 않았다.

## fullgate 복구와 current-card

| 항목 | 결과 |
| --- | --- |
| fullgate `full-admin-host-mutation-gate-20261004-04289-r3b` | `PASS`, `06:32:05Z`부터 약 `457`초. service-msi-hyperv `444.490s`, os-mutation `11.109s` |
| 사전 상태 | orchestrator가 남긴 clean `0.42.89` `{0E27390F-…}`, 같은 version 재설치(A) |
| 사후 검사 | build commit `c67d22b` == gate build, 같은 version ARP `{FACF6D5F-1041-4B77-92B6-7B476447ECB7}` 1개, MSI log `0`/`0` |
| gate build | MSI `dcec4e9228c5376d2e07af84456cb585a2a93004705a011bab06d03da0d3e868`, payload `11228c3ff716395fde7ad0a9aa1eebcff00b7371ad44eb8f84a8f78a3ee9ed41` |
| managed delete | route smoke `vm delete`의 `storage_cleanup`이 `disk0.vhdx`와 VM 디렉터리를 지웠다 |
| current-card r3b | `pass`, CLI `3/3`, Web `2/2`, 설치본 Host/CLI가 gate payload와 같음, 테스트 VM `0`, secret 없음, `not-promoted` |

## `pcvverify train-facts` 확인

0.42.89 facts 파일을 백업한 뒤 실제 리허설 출력으로 네 번 돌렸다. 렌더한 문서와 facts는 `artifacts/train-facts-rehearsal-20261004-r3b/`로 옮기고 0.42.89 facts를 되돌렸다. 0.42.89 golden `--check`는 그대로 `ok`이고 작업 트리는 clean이다.

| 입력 | 결과 |
| --- | --- |
| A: pair 문서 `9`개 | 실패 `train-facts:fact-mismatch:observations:after:installed-runtime-ops-summary`(위 관측 결함) |
| B: package, pair-descriptor, current-card | 실패 `train-facts:document-missing:update-rollback`. pair-descriptor는 다섯 bucket 문서의 evidence id를 표에 넣으므로 그 문서들과 같이만 만들 수 있다. train에서는 항상 같이 만든다 |
| B2: package, current-card | 통과. 생성 값 `19`+서술 `4`, `31`+서술 `1`. 렌더 `written` |
| C: fullgate | 통과. 생성 값 `34`+서술 `3`. 렌더 `written` |

package 문서는 artifact root, evidence id, 서술 줄을 빼면 0.42.89 수동 문서와 값이 같았다(MSI, payload, Upgrade 행 `513`/`1401`, Host ProductVersion). 관측에 기대는 ops summary, update/rollback, clean-host, Burn, MSIX 문서와 이들에 기대는 pair-descriptor는 실제 데이터로 아직 확인하지 못했다.

## 다음

1. Lane 1: 관측 함수의 `DisplayName` 읽기를 strict mode에 맞게 고치고, Delivery 계약 시험에 strict mode 조건을 더한다.
2. Lane 2: 고친 orchestrator로 리허설을 한 번 더 돌려 9개 문서 전부를 실제 데이터로 생성한다.
3. 그 뒤 3c(절차 변경)를 검토한다.

## Nonclaims

- 리허설은 승격 근거가 아니다. operational current는 `0.42.89-admin-smoke` 그대로이고 `current-evidence.json`을 바꾸지 않았다.
- 이 호스트 설치본은 리허설 fullgate build(`+c67d22b`)로 바뀌었다. 0.42.89 operational evidence의 fullgate build(`+a780928`)와 같은 version, 같은 product source다.
- public trusted signing과 external stable publication을 주장하지 않는다.
