# ADR-0015: 기능 evidence와 operational current 분리 정책

상태: 채택 / AR-001 evaluator 구현 전
일자: 2026-08-23

## 결정 마커

```text
DESKTOP_NODE_OPERATIONAL_CURRENT_DECISION: installation-rollback-baseline-only
DESKTOP_NODE_FEATURE_QUALIFICATION_DECISION: stable-feature-id-stage-evidence-ledger
DESKTOP_NODE_PROMOTION_ELIGIBILITY_DECISION: mandatory-feature-stages-pass-only
DESKTOP_NODE_FEATURE_CARRY_FORWARD_DECISION: failed-missing-blocked-forbidden
DESKTOP_NODE_FEATURE_PROMOTION_IMPLEMENTATION: ledger-contract-applied-evaluator-pending
```

## 맥락

`0.42.74-admin-smoke`는 package, full admin host mutation, manual-admin package-pair와 installed
CLI/Web current-card를 닫아 설치·rollback 기준선으로 승격됐다. 그러나 SERVICE_PLAN P0 actual-VM은
`vm.save`의 WMI `32775` 실패와 `vm.resume-saved` 미실행 때문에 `FAIL`이다. Current-card는 이
실패를 기록하면서도 version promotion을 `promoted-current`로 표시한다.

이 두 사실은 모순이 아니라 서로 다른 질문에 대한 답이다. Operational current는 현재 설치하고
rollback할 내부 운영 기준선을 뜻한다. Feature qualification은 특정 사용자 기능이 code, package,
installed, actual-VM, manual-admin 요구를 충족했는지를 뜻한다. 한 상태로 표현하면 설치 기준선 PASS가
기능 PASS처럼 보이거나, 기능 하나의 결함 때문에 실제 installed baseline을 소급 삭제하게 된다.

## 결정

operational_current는 설치·rollback 기준선을 뜻하고 feature_qualification은 기능별 검증 상태를
뜻한다. candidate는 required feature stage가 모두 pass일 때만 promotion_eligible=true다.
failed, missing, blocked stage는 자동 carry-forward할 수 없다.

- 기능은 `pcv.`로 시작하는 stable `feature_id`를 가진다.
- `config/desktop-node-feature-evidence-ledger.json`이 기능, operator surface, operation,
  required stage와 current evidence locator를 소유한다.
- ledger shape는 `config/desktop-node-feature-evidence-ledger.schema.json`이 소유한다.
- stage 이름은 `code_tested`, `packaged`, `installed_tested`, `actual_vm_tested`,
  `manual_admin_tested`로 고정한다.
- verdict는 `pass`, `fail`, `blocked`, `missing` 중 하나다.
- `candidate_required=true` 기능은 required stage가 전부 `pass`여야 candidate eligibility를 얻는다.
- current version 자체를 소급 삭제하지 않는다. Known feature failure는 current의
  `feature_qualification` blocker로 투영한다.
- 새로운 candidate를 current로 쓰는 생성기는 `promotion_eligible=false`를 fail-closed해야 한다.
- API, Web, CLI와 사용자 문서는 ledger projection 또는 parity test로 stable Feature ID를 공유한다.

## 0.42.74 / 0.42.75 적용

- 0.42.74의 attach, checkpoint restore, managed import는 P0 actual-VM `pass`다.
- 0.42.74의 Saved lifecycle은 P0 actual-VM `fail`이다.
- 0.42.74는 operational current로 유지하면서 Saved blocker를 숨기지 않는다.
- 0.42.75는 승인된 promotion closure의 SavedOnly, Full P0, functional carry-forward,
  manual-admin, clean-target SavedOnly, final current-card가 모두 PASS하기 전 candidate다.
- 이 ADR은 0.42.75 campaign 실행이나 host mutation을 승인하지 않는다.

## 결과

- 설치 가능한 현재 version과 기능별 준비도를 별도 질문으로 답할 수 있다.
- Package/fullgate PASS가 actual-VM FAIL을 덮지 않는다.
- Web Ops Summary와 문서가 blocker count와 evidence locator를 같은 의미로 표시할 수 있다.
- API route 수와 Web coverage 수가 다르더라도 의도적 surface 제외 사유를 Feature ID에 연결할 수 있다.

## 제한과 비주장

- 이 ADR 자체는 candidate evaluator, current-evidence generator guard 또는 Ops Summary projection을
  구현하지 않는다. AR-001 후속 task가 소유한다.
- Ledger의 current verdict는 dated evidence를 요약할 뿐 원본 evidence를 대체하지 않는다.
- 0.42.74를 public stable release로 재분류하지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- 실제 VM, Hyper-V, MSI, service, firewall, Event Log, trust store mutation을 수행하지 않는다.

## 검증

- `packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1`
- `config/desktop-node-feature-evidence-ledger.schema.json`
- `config/desktop-node-feature-evidence-ledger.json`
- `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md`
- `docs/superpowers/specs/2026-08-21-purecvisor-desktop-node-04275-promotion-closure-design.md`

## 롤백

Evaluator와 current projection이 아직 적용되기 전에는 ledger/schema를 제거해 직전 문서 상태로
돌아갈 수 있다. ADR은 historical decision으로 보존한다. Evaluator 적용 뒤에는 current pointer를
먼저 바꾸지 말고 feature projection reader를 이전 schema와 함께 읽을 수 있게 되돌린다. Failed,
missing, blocked stage를 pass로 바꾸거나 provider mutation을 재실행하는 방식으로 롤백하지 않는다.
