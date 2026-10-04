# Release train 설계

- Design-ID: `pcv-release-train-v1`
- 작성일: `2026-10-04`
- 문서 상태: `accepted` (2026-10-04 결정 §8, 1단계 도입)
- 변경 등급: M (개발 절차와 campaign 구성. 제품 동작 변경 없음)
- host/VM/service/package mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 문제

지금은 제품 payload가 바뀔 때마다 다음 package pair가 열린다. descriptor chain의 출발 조건이 그렇게 적혀 있다(`next_manual_admin_package_pair_trigger: product-payload-change-after-<tag>`). 변경이 작아도 package, manual-admin pair 여섯 bucket, fullgate, installed current-card, Lane 2 probe, Lane 3를 한 바퀴 다 돈다.

2026-09-28부터 2026-10-03까지의 기록이다.

| 항목 | 값 |
| --- | --- |
| package build | `10`개 (`0.42.79`~`0.42.88`) |
| 그중 Lane 2 probe 운반용 | `4`개 (`0.42.79`~`0.42.82`) |
| 승격하지 않은 pair | `1`개 (`0.42.85`, eject 결함) |
| Lane 3 승격 | `5`번 (`0.42.83`, `0.42.84`, `0.42.86`, `0.42.87`, `0.42.88`) |

2026-10-03 하루만 보면 제품 수정은 세 건이었다(reconcile 안내 문구, 같은 version 재설치, managed delete 디스크 정리). 그런데 package pair 두 번, 승격 두 번, commit `46`개, 새 evidence `26`개가 나왔다. 변경량은 제품 코드 약 `550`줄, 테스트 약 `330`줄, 문서·config 약 `3,270`줄이다. 검증 한 바퀴에 손으로 쓰는 evidence가 `13`개(pair `10`, Lane 3 `3`)이기 때문이다.

수정마다 한 바퀴를 돌면 호스트 작업, 승인 왕복, 문서량이 수정 수에 비례해 늘어난다. 그 사이 다른 수정이 끼어들면 순서가 꼬인다.

## 2. 목표와 비목표

목표:

- 검증 한 바퀴(package부터 Lane 3까지)를 제품 수정 하나가 아니라 묶음 하나에 쓴다.
- Lane 2 probe 운반용 package를 없앤다.
- 한 바퀴에 필요한 승인을 묶음 출발 때 한 번에 받는다. 무엇을 승인하는지는 문구에 명시한다.
- 지금의 증거 기준은 낮추지 않는다. package, pair, fullgate, current-card, Lane 2, Lane 3의 PASS 조건은 그대로다.

비목표:

- Lane 정의, 변경 등급, 회로 차단기 한도를 바꾸지 않는다.
- evidence 문서의 내용 기준을 줄이지 않는다. 쓰는 방법(자동 생성)은 §6에서 다룬다.
- public trusted signing과 external stable publication을 다루지 않는다.

## 3. 용어

| 용어 | 뜻 |
| --- | --- |
| train | package 하나, version 하나로 도는 검증 한 바퀴. package부터 Lane 3 merge까지다 |
| 적재 변경 | train에 실을 product payload 변경. `main`에 merge된 PR 단위다 |
| 대기열 | 다음 train을 기다리는 적재 변경 목록 |
| 출발 | 대기열을 고정하고 그 시점 `main` HEAD로 package를 빌드하는 일 |
| 정차 | train 도중 FAIL이 나서 멈춘 상태 |

## 4. 규칙

### 4.1 Lane 1은 계속 `main`에 쌓는다

- 제품 수정은 지금처럼 Lane 1 campaign에서 만들고, Required CI가 green이면 PR로 `main`에 merge한다.
- product payload를 바꾸는 PR은 대기열에 한 행을 더한다(§5). docs-only, test-only, tooling-only PR은 더하지 않는다. 기존 `product_payload_change_detected` 판단과 같다.
- 대기열에 들어간 변경은 설치본에 반영되지 않은 상태다. 그 사실을 PR 본문과 대기열 행에 적는다.

### 4.2 출발 조건

| 조건 | 출발 |
| --- | --- |
| 정기 | 주 1회. 직전 train 출발 7일 뒤부터 정기 출발 대상이다(§8 결정 1). 대기열이 비어 있으면 출발하지 않는다 |
| 조기 | operational current의 결함이 데이터 손실, 설치·업그레이드 실패, 보안 문제를 일으킬 때. 또는 사용자가 명시적으로 요청할 때 |
| 보류 | Lane 2 probe가 필요한 적재 변경인데 probe 설계가 없을 때. 그 변경만 다음 train으로 미룬다 |

대기열 행이 `5`개를 넘으면 정기일 전이라도 출발을 제안한다. train이 커질수록 FAIL 원인을 나누기 어렵기 때문이다.

### 4.3 출발 때 고정한다

- 출발 commit에서 대기열을 train manifest로 고정한다. 고정한 `main` HEAD가 package의 source다.
- 고정 뒤 merge된 변경은 다음 train에 실린다. 도중에 끼워 넣지 않는다.

### 4.4 한 train은 한 version이다

한 train은 version 하나로 다음을 한 번씩 돈다.

1. package
2. manual-admin pair 여섯 bucket과 descriptor
3. fullgate
4. installed current-card
5. 적재 변경별 Lane 2 probe. Lane 2 규칙(checkpoint 하나에 기능군 하나)을 지키려고 probe는 기능군마다 checkpoint 하나다
6. Lane 3 승격

### 4.5 probe 운반용 package를 만들지 않는다

Lane 2 probe는 train candidate 설치본에서 한다. probe 때문에 중간 package를 따로 빌드하지 않는다.

예외는 하나다. merge 전에 실제 호스트 동작을 봐야 판단할 수 있는 Lane 1 변경이 있을 수 있다. 그때는 개발 호스트에서 제품 Update로 로컬 build payload를 올려 보고 Rollback으로 되돌린다(dev probe). 이것은 Lane 2 승인이 필요한 host mutation이다. 결과는 `dev-probe` 관측으로만 남기고 승격 근거로 쓰지 않는다.

### 4.6 정차하면 고쳐서 다음 version으로 다시 출발한다

- bucket, fullgate, current-card, probe 중 하나라도 FAIL이면 그 자리에서 멈춘다. train 안에서 손으로 고쳐 이어 가지 않는다.
- 결함은 Lane 1로 고쳐 `main`에 merge하고, 다음 patch version으로 다시 출발한다. FAIL한 version은 승격하지 않은 기록으로 남는다(`0.42.85` 선례).
- 같은 원인 3회 실패, 범위 밖 설계, 권한 거부 같은 기존 중단 조건은 그대로 적용한다.
- 환경 원인으로 판명된 일시 실패(예: 직전 테스트 프로세스가 잡고 있던 listener 포트)는 같은 단계를 한 번 다시 돌릴 수 있다. 다시 돌린 사실과 원인을 evidence에 적는다.

### 4.7 승인은 출발 때 한 번 받는다

지금은 package, Lane 2, Lane 3, push/PR, merge 승인을 따로 받는다(`docs/DEVELOPMENT_PROCEDURE.md` §4). train에서는 출발 때 승인 하나로 받는다. 단, 문구가 다음을 하나씩 명시해야 한다. 승인을 추정하지 않는 규칙은 그대로다.

- train version과 고정할 대기열 행
- package build
- pair host mutation 범위(MSI 제거·설치, 제품 Update/Rollback, clean-host VM, Burn, MSIX)
- fullgate와 current-card
- 적재 변경별 Lane 2 probe 기능군
- Lane 3 `current-evidence.json` 쓰기
- branch push, PR, green CI 뒤 merge

정차하면 그 승인은 끝난다. 다시 출발하려면 새 승인이 필요하다.

## 5. 대기열 파일

`docs/ga-ready/release-train.json`(새 파일)이 대기열과 train 이력을 갖는다.

```json
{
  "schema_version": 1,
  "contract": "pcv-release-train-v1",
  "operational_current": "0.42.88-admin-smoke",
  "queue": [
    {
      "pr": 33,
      "merge_commit": "<sha>",
      "area": "hyperv",
      "summary": "한 줄 요약",
      "lane2_probe": { "family": "vm.delete", "design": "docs/superpowers/specs/<probe-design>.md" },
      "risk_tier": "M"
    }
  ],
  "trains": [
    {
      "version": "0.42.89-admin-smoke",
      "departed_at": "<iso>",
      "source_commit": "<sha>",
      "carriages": [33],
      "status": "running"
    }
  ]
}
```

- `lane2_probe`가 `null`이면 그 변경은 pair, fullgate, current-card만으로 검증한다.
- 출발 commit에서 `queue` 행을 train의 `carriages`로 옮기고 `queue`를 비운다.
- 정차하거나 승격하면 `status`를 `stopped` 또는 `promoted`로 바꾼다.

## 6. 증적 자동 생성 (2단계)

한 바퀴에 손으로 쓰는 evidence `13`개는 거의 같은 틀에 값만 바뀐다. 2026-10-03의 두 train도 직전 문서를 복사해 값을 바꿨다. 그래서 bucket summary JSON에서 evidence를 생성한다.

| evidence | 입력 |
| --- | --- |
| package | build provenance, update ZIP, MSI Upgrade 테이블 |
| ops summary | `ops summary` 캡처, 비인증 응답, Web 상태 |
| update/rollback | `update.json`, `rollback.json`, 전후 상태 |
| clean-host | clean-host runner `summary.json` |
| Burn, MSIX | 각 runner `summary.json` |
| descriptor | descriptor `summary.json` |
| fullgate | batch summary, step result, MSI lifecycle, provenance, 설치본 hash |
| current-card | current-card `summary.json` |
| Lane 3 세 개(functional carry-forward, consume, main push) | 위 결과와 CI run id |

- 생성기는 C#으로 `DesktopNode.Verification`에 두어 Required CI에서 시험한다. Required CI는 PowerShell을 돌리지 않기 때문이다.
- Lane 2 probe evidence는 probe마다 판정 기준이 달라 손으로 쓴다.
- Lane 3 문서 도구(`Invoke-PcvLane3PromotionDocs.ps1`)의 spec도 train manifest와 evidence에서 생성한다.

3단계로, 여섯 bucket을 하나씩 돌리는 대신 이미 저장소에 있는 `Invoke-PcvManualAdminPackagePairCampaign.ps1`을 쓰는 것을 검토한다. 이 도구는 package마다 update catalog JSON이 필요하다.

## 7. 기대 효과와 비용

2026-10-03을 train 하나로 돌렸다면 결과는 다음과 같다.

| 항목 | 실제 | train 하나 |
| --- | ---: | ---: |
| package pair | `2` | `1` |
| Lane 3 승격 | `2` | `1` |
| 손으로 쓴 evidence | `26` | `13` (2단계 뒤 Lane 2 probe만) |
| 승인 왕복 | 5회 이상 | 출발 1회 |

2026-09-28~29의 probe 운반 package 네 개(`0.42.79`~`0.42.82`)는 §4.5로 없어진다.

비용과 대응:

| 비용 | 대응 |
| --- | --- |
| 설치본 반영이 최대 한 주 늦어진다 | Lane 1 테스트와 Required CI는 그대로 매 PR에서 돈다. 급한 결함은 조기 출발한다 |
| train이 커지면 FAIL 원인을 나누기 어렵다 | 적재 변경별 Lane 2 probe. 대기열 `5`개 초과 시 출발 제안 |
| 정차하면 묶음 전체가 늦어진다 | FAIL한 변경을 revert하거나 고친 뒤 다음 version으로 다시 출발한다 |
| 대기열 파일 관리 | product PR merge 때 한 행. 2단계에서 PR 본문에서 생성 |

## 8. 결정 (2026-10-04)

사용자가 `1,2,3,4, commit,pr`로 네 권고를 모두 채택했다.

| 항목 | 결정 |
| --- | --- |
| 1 정기 출발 주기 | 주 1회. 요일은 고정하지 않고 직전 train 출발 7일 뒤부터 정기 출발 대상이다 |
| 2 출발 승인 | 출발 승인 하나가 package부터 merge까지 덮는다. 문구에 §4.7 범위를 하나씩 적는다 |
| 3 probe 운반 package | 금지. dev probe 예외(§4.5)를 둔다 |
| 4 증적 생성기 | 1단계가 자리 잡은 뒤 착수한다 |

## 9. 도입 단계

| 단계 | 내용 | 변경 |
| --- | --- | --- |
| 1 (2026-10-04 도입) | 절차 도입 | `docs/DEVELOPMENT_PROCEDURE.md` §10 train 절과 §4 승인 표, `docs/ga-ready/release-train.json`과 C# 구조 계약 `PcvReleaseTrainContractTests`. descriptor chain의 다음 출발 조건 값 `release-train-departure-after-<tag>`는 다음 승격의 Lane 3 spec에서 쓴다 |
| 2 (2026-10-04 도입) | 증적 생성 | `pcvverify train-evidence`, 틀 `12`개, facts 계약 `pcv-train-evidence-facts-v1`, golden 시험 `TrainEvidenceGoldenTests`(설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-evidence-render-design.md`). facts는 사람이 채운다. facts 자동 채우기와 Lane 3 spec 생성은 3단계로 옮긴다 |
| 3 | pair orchestrator | update catalog 생성, `Invoke-PcvManualAdminPackagePairCampaign.ps1` 사용. 설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md`. 3a(update 패키지 도구, orchestrator 관측, `pcvverify train-facts`) 2026-10-04 도입, 3b 재리허설 PASS(`train-pair-orchestrator-rehearsal-2026-10-04-04288-04289-r2`), 3c 절차 반영 |

1단계는 제품 동작을 바꾸지 않는다. descriptor chain의 출발 조건 값은 Lane 3 spec의 데이터라서 다음 승격 때 바꾼다.

## 10. Nonclaims

- 이 문서는 절차 설계다. 이 문서를 쓰는 동안 호스트, 설치본, `current-evidence.json`을 바꾸지 않았다.
- train은 internal admin-smoke 범위의 검증 묶음이다. public trusted signing이나 external stable publication을 뜻하지 않는다.
