# Feature evidence ledger 모델 확장 결정

- Design-ID: `pcv-feature-stage-observation-decision-v1`
- 작성일: `2026-10-03`
- 문서 상태: `decided` (ledger 모델 확장 안 함)
- 변경 등급: S (문서)
- host/VM/service/package mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 질문

`docs/FEATURE_IMPLEMENTATION_LEDGER.md` stage 표는 28개 feature 중 `24`개를 `not-assessed`로 둔다.
그중 일부는 설치본 actual-VM PASS evidence가 있다(예: `pcv.vm.clone` 0.42.77, `pcv.vm.media-eject` 0.42.86).
09-27 backlog Task 2는 그 기록에 "feature evidence ledger 모델 확장이 먼저 필요하다"고 남겼다.
감사 `docs/project-status-audit-2026-10-03.md` §10 순위 4가 이 확장 여부의 결정을 요구했고, 사용자가
2026-10-03에 승인했다.

## 2. 이미 정한 경계

- ADR-0015: feature qualification은 `config/desktop-node-feature-evidence-ledger.json`의 required stage가
  모두 `pass`일 때만 promotion eligible이다.
- `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-feature-id-surface-parity-design.md` §2, §3.2:
  evidence ledger는 P0 승격 판정 대상 `4`개만 소유한다. 전체 surface catalog로 넓히면 승격 evidence의 의미가
  오염된다. 전체 Feature ID는 `config/desktop-node-feature-surface-ledger.json`이 소유한다.
- 테스트가 이 경계를 고정한다: `ApiHandlerAdapterContractTests.EvidenceCandidateFeaturesAreKnownSurfaceFeatures`
  (`4`개), `D2EvidenceContractVerifier` feature ordinal 1~4.

## 3. 관측의 성격

있는 evidence는 feature 전체가 아니라 operation 일부를 덮는다. 예를 들어
`pcv.network.inventory`는 `vm.network.connect`와 `vm.device.add`만, `pcv.vm.guest-execution`은
`vm.guest.file`만 actual-VM PASS가 있다. 또 대부분 probe 운반 package(`0.42.79`~`0.42.82`)나 그 시점 설치본에서
얻었고 manual-admin stage가 없다. feature 단위 stage `pass`로 적으면 덮지 않은 operation까지 주장하게 된다.

## 4. 결정

1. **evidence ledger 모델을 확장하지 않는다.** schema, `candidate_required`, 후보 `4`개, evaluator는 그대로다.
2. **stage 투영 표는 그대로 둔다.** 후보 밖 feature는 계속 `not-assessed`다. 투영 규칙이 바뀌지 않는다.
3. **비후보 관측은 operation 단위로 따로 적는다.** `docs/FEATURE_IMPLEMENTATION_LEDGER.md`에 "비후보 actual-VM
   관측(승격 판정 밖)" 절을 두고 feature, operation, 설치본 version, evidence id만 적는다. 이 절은 promotion
   판정, current-evidence, stage 표의 입력이 아니다.
4. 비후보 feature를 승격 후보로 올리려면 별도 결정으로 evidence ledger에 후보를 더하고, 다섯 stage evidence를
   모두 갖춘다. 그것은 이 결정 밖이다.

## 5. 버린 선택지

| 안 | 버린 이유 |
| --- | --- |
| evidence ledger에 `candidate_required=false` 항목 추가 | 08-24 §3.2의 "후보 4개만"과 테스트 고정을 깨고, 한 `verdict`가 feature 전체를 덮는다 |
| evidence ledger schema에 stage별 선택 필드 추가 | 같은 이유로 승격 ledger에 비승격 관측이 섞인다. reader(D2, Api 테스트, PowerShell evaluator) 계약도 함께 바뀐다 |
| 별도 기계 판독 관측 ledger(JSON) 신설 | 지금 그 데이터를 읽는 소비자가 없다. 필요해지면 그때 설계한다 |

## 6. 비목표

- 새 검증을 실행하지 않는다. 이미 있는 evidence를 다시 적을 뿐이다.
- 기존 evidence의 판정을 바꾸지 않는다.
