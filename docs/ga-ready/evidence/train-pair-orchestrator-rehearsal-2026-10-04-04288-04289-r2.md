# Release train pair orchestrator 재리허설 `0.42.88-admin-smoke` -> `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289-r2`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
design: `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3b
predecessor: `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289` (`PARTIAL_PASS_WITH_OBSERVATION_DEFECT`)
observation_fix: PR #41 (merge `3cb2162`)
orchestrator_status: `pass`
train_facts_status: `pass-9-of-9`
campaign_id: `manual-admin-campaign-20261004-04288-04289-r3b2`
campaign_root: `artifacts/manual-admin-campaign-20261004-04288-04289-r3b2`
closed_descriptor_batch_id: `manual-admin-campaign-20261004-04288-04289-r3b2-closed`
fullgate_batch: `full-admin-host-mutation-gate-20261004-04289-r3b2`
current_card_root: `artifacts/installed-operator-surface-current-card-20261004-04289-r3b2`
current_card_summary_sha256: `735ed0d2710d05401de5d020b5c9b708557f68d5c03aeac9bb11a366cf61e9d5`
train_facts_root: `artifacts/train-facts-rehearsal-20261004-r3b2`
installed_product_version: `0.42.89-admin-smoke+b7fe7b229d298dafdedf97bfe6be2ff1ce59a2c0`
host_mutation_performed: `true`
guest_credential_recorded: `false`
promotion_evidence: `false`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

첫 리허설에서 나온 관측 ARP 결함을 PR #41로 고친 뒤, 같은 package 두 개와 3a update ZIP·catalog로 리허설을 한 번 더 돌렸다(사용자 승인 `1,2,3`, 2026-10-04). 이번에는 orchestrator 출력으로 `pcvverify train-facts`가 pair 문서 `9`개를 모두 만드는지 확인했다. 결과는 승격 근거가 아니다. guest 인증 정보는 실행 경계에서 만들었고 기록하지 않았다.

## orchestrator 실행

`-PlanOnly` `ok=true` 뒤 `-Execute`를 `2026-10-04T11:32:22Z`부터 `256`초 돌렸다. 설치본이 target이라 baseline catalog로 `0.42.88`에 먼저 맞췄다. 여섯 bucket 모두 PASS, closed descriptor `overall_status=pass` `6/6`, reservation 소비, restoration 불필요. 첫 리허설과 판정이 같다.

`observations.json` `16`개 항목 모두 `observation_error`가 없고 ARP 목록이 있다. ARP product code는 fullgate MSI `{FACF6D5F-…}`에서 Burn이 복구한 clean MSI `{0E27390F-5A08-4F76-90FD-7694B41672A4}`로 바뀐다.

## fullgate 복구와 current-card

| 항목 | 결과 |
| --- | --- |
| fullgate `full-admin-host-mutation-gate-20261004-04289-r3b2` | `PASS`, `11:37:11Z`부터 약 `463`초. service-msi-hyperv `450.912s`, os-mutation `11.120s` |
| 사후 검사 | build commit `b7fe7b2` == gate build, 같은 version ARP `{CE46D250-007C-449F-8C94-6A1C9F3DD752}` 1개, MSI log `0`/`0`, managed delete 정리 관측 |
| gate build | MSI `feb0a3e266e7365d6197cc9465ef975a1f8b75e3ec7f18a338e812bd116dfd7b`, payload `f7c2bf5238ae5084622d9999ea3e826ba812d17c45a18cbb386cb355d495d1f0` |
| current-card r3b2 | `pass`, 설치본 Host/CLI가 gate payload와 같음, secret 없음, `not-promoted` |

## `pcvverify train-facts`

첫 실행은 `train-facts:fact-mismatch:clean-host:state`에서 멈췄다. clean-host 틀이 `base_vhd_source: current-base`를 literal로 두었는데, orchestrator는 base VHD를 `-BaseVhdPath`로 넘겨 runner가 `explicit`을 기록한다. 생성기가 틀의 literal과 artifact가 다른 것을 잡은 것이다. 틀의 그 머리말을 값으로 바꾸고(0.42.89 facts에는 `current-base`를 넣어 golden을 유지), 생성기는 runner 값을 그대로 쓰도록 고쳐(`db5ad68`) 다시 돌렸다.

| 문서 | 생성 값 | 사람 값 | 렌더 |
| --- | ---: | ---: | --- |
| package | `19` | `4` | `written` |
| ops summary | `10` | `0` | `written` |
| update/rollback | `23` | `0` | `written` |
| clean-host | `21` | `0` | `written` |
| Burn | `16` | `0` | `written` |
| MSIX | `17` | `0` | `written` |
| pair descriptor | `17` | `0` | `written` |
| fullgate | `35` | `2` | `written` |
| current-card | `31` | `1` | `written` |

렌더한 문서와 facts는 `artifacts/train-facts-rehearsal-20261004-r3b2/`에 있다. 사람이 쓴 값은 `7`개뿐이다(package 배경·적재 변경·Lane 2 기능군·build 시간, fullgate 실행 문맥·VM 칸, current-card 스크립트 문장). 0.42.89 facts와 evidence는 바꾸지 않았고 golden `--check`는 `12/12` `current`다.

렌더 문서의 `-TargetMsiPath`, `-BaselineMsiPath`는 리허설 입력의 package root를 3a 확인 폴더(`artifacts/update-package-check-20261004/`)로 준 탓에 MSI가 없는 경로를 가리킨다. train에서는 update 패키지 도구가 package root에 바로 쓰므로 package root를 그대로 넘긴다.

## 판단

3b 목표(orchestrator 실제 실행, 수동 pair와 같은 판정, 실제 출력으로 pair evidence 생성)를 모두 확인했다. 3c(`DEVELOPMENT_PROCEDURE.md` §10 train task 변경)로 넘어갈 수 있다.

## Nonclaims

- 리허설은 승격 근거가 아니다. operational current는 `0.42.89-admin-smoke` 그대로이고 `current-evidence.json`을 바꾸지 않았다.
- 이 호스트 설치본은 재리허설 fullgate build(`+b7fe7b2`)로 바뀌었다. 0.42.89 operational evidence build(`+a780928`)와 같은 version, 같은 product source다.
- public trusted signing과 external stable publication을 주장하지 않는다.
