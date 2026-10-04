# Release train pair orchestrator 설계 검토 (3단계)

- Design-ID: `pcv-train-pair-orchestrator-v1`
- 작성일: `2026-10-04`
- 문서 상태: `proposed` (설계 검토. 구현과 host 실행은 별도 승인)
- 변경 등급: L (manual-admin host mutation 실행 경로)
- 상위 설계: `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-release-train-design.md` §6, §9 3단계
- 선행: 2단계 증적 렌더러 `pcv-train-evidence-render-v1` (PR #37, merge `a399c2e`)
- host/VM/service/package mutation: `false` (이 문서를 쓰는 동안)

## 1. 문제

0.42.89 train의 pair는 bucket runner 여섯 개와 readiness, descriptor를 손으로 하나씩 돌렸다. task `7`개, Lane 2 tool batch 약 `8`회다. 실행 명령은 저장소 밖 임시 스크립트에 있었다. 직전 train 명령을 대화 기록에서 다시 찾아야 했다. 2단계 렌더러의 facts 값 `243`개도 사람이 runner 출력에서 옮겨 적는다.

저장소에는 여섯 bucket을 한 번에 도는 `packaging/windows-desktop-node/tools/Invoke-PcvManualAdminPackagePairCampaign.ps1`(post-0.42.83 Task 2, `10111ba`)이 이미 있다. 계약 시험(`PcvManualAdminPackagePairCampaignContractTests`)은 있지만 실제 pair에 쓴 적은 없다.

## 2. 현재 orchestrator와 수동 절차의 차이

| 항목 | 수동 절차(0.42.83~0.42.89) | orchestrator |
| --- | --- | --- |
| 제품 Update 입력 | payload 폴더(`-SourceRoot`) | update catalog URI(`-UpdateCatalogUri`, channel `admin-smoke`) |
| update ZIP | 임시 Python 스크립트로 직접 만든다 | 경로를 받는다. 만드는 도구 없음 |
| update catalog | 없음 | 필수. 만드는 도구 없음(`New-PcvUpdaterCatalogPublicationPreflight.ps1`은 공개 catalog preflight이고 생성기가 아니다) |
| readiness | `-PlanOnly`, reservation `reservation-required-before-actual-execution` | `-ForActualExecution`, dedicated host reservation `reserved-and-matched` 필수. reservation은 스스로 만들고 소비한다 |
| update/rollback bucket | Update → Rollback, baseline으로 끝난다 | Update → Rollback → Update, target으로 끝난다 |
| ops summary | baseline 설치 상태에서 먼저 캡처 | 마지막에 target 설치 상태에서 캡처 |
| Burn 사전 정렬 | 별도 제품 Update와 15초 대기 | 직전 bucket이 target으로 끝나므로 필요 없다 |
| clean-host guest 비밀번호 | runner 기본값 | `-GuestCredential` 필수(없으면 `PCV_MANUAL_ADMIN_CAMPAIGN_GUEST_CREDENTIAL_REQUIRED`) |
| 사전·사후 host 상태 | stdout으로만 봤다 | bucket summary에 일부만 남는다(manifest, service) |
| descriptor | 별도 `-PlanOnly` 실행과 consume | 닫을 때 descriptor `<campaign>-closed`를 만든다 |
| evidence facts | 사람이 옮긴다 | 없음 |

## 3. 권장안

orchestrator를 train의 pair 실행기로 채택한다. 단, 아래 네 가지를 먼저 고치고(3a), 실제 호스트에서 한 번 리허설(3b)한 뒤 train 절차에 넣는다(3c). 처음부터 새 orchestrator를 쓰는 안은 택하지 않는다. 이미 있는 실행기와 계약 시험을 버리게 되기 때문이다.

### 3a. Lane 1 (host mutation 없음)

1. update 패키지 도구. `New-PcvAdminSmokeUpdatePackage.ps1`가 package root에서 update ZIP(payload `8`개 파일, deflate, `/` 구분자)과 admin-smoke update catalog를 함께 만든다. catalog는 orchestrator `Assert-PcvCatalog`가 보는 필드(`source_uri` file URI, `version`, `channel`, `release_channel`, `signing_mode=AllowUnsignedDev`, `expected_sha256`)를 쓴다. train task 1(package)이 이 도구를 부른다.
2. 관측 기록. orchestrator가 bucket마다 앞뒤 host 상태를 `observations.json`에 남긴다. 항목은 product manifest, 설치본 Host ProductVersion, `DesktopNode.previous`/`DesktopNode.failed` version, service 상태와 시작 유형, Web 상태, VM 목록, PureCVisor ARP 항목과 시간이다. token과 guest 비밀번호는 남기지 않는다.
3. facts 생성기. `New-PcvTrainEvidenceFacts.ps1`가 package provenance와 MSI Upgrade 표, 여섯 bucket summary, `observations.json`, fullgate batch 결과, current-card summary를 읽어 2단계 facts 파일의 pair 문서 값을 채운다. 서술 줄(`context`, `prestate` 등)은 정해진 문장 틀에 값을 넣어 만든다. MSI 표를 읽어야 하므로 PowerShell 쪽(packaging 도구)에 둔다. 결과는 `pcvverify train-evidence --write`로 렌더한다.
4. orchestrator 의미 차이를 evidence 틀에 반영한다. update/rollback은 Update → Rollback → Update, ops summary는 target 상태 캡처다. 0.42.89 golden을 지키기 위해 기존 틀은 그대로 두고 새 틀 이름(`update-rollback-orchestrated`, `ops-summary-target`)을 더한다.

guest credential은 실행 경계에서 만든다. clean-host runner 기본값을 쓰는 지금 방식과 같은 값을 orchestrator 호출 직전에 `PSCredential`로 만들어 넘기고, 값은 명령줄, summary, evidence에 남기지 않는다.

시험은 Required CI 원칙(PowerShell 0)에 맞춰 Delivery C# 계약 시험으로 한다. 새 packaging `*.Tests.ps1` 파일은 만들지 않는다.

### 3b. Lane 2 리허설 (별도 승인 필요)

이미 있는 `admin-smoke-package-20261003-04288`과 `admin-smoke-package-20261004-04289`로 orchestrator를 `-Execute`한다. 설치본은 지금 0.42.89 fullgate build이므로 orchestrator가 먼저 baseline catalog로 0.42.88에 맞춘다. 끝나면 설치본은 target `0.42.89` clean package가 된다. 그 뒤 기존 fullgate를 다시 돌려 0.42.89 fullgate build로 되돌린다.

host mutation은 다음과 같다: 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn install/repair/remove, MSIX install/update/remove, fullgate. 0.42.89 train의 pair와 같은 범위다. 리허설 결과는 승격 근거가 아니고, 0.42.89 수동 evidence와 bucket 판정이 같은지 비교하는 데만 쓴다.

### 3c. 절차 반영

리허설이 PASS하면 `DEVELOPMENT_PROCEDURE.md` §10 train task 2a~2g를 task 하나(orchestrator `-Execute`, facts 생성, 렌더)로 바꾼다. 출발 승인 문구의 pair host mutation 범위는 같다.

Lane 3 spec(`lane3-promotion-docs-spec-<tag>.json`) 생성은 3c 뒤로 둔다. 0.42.87~0.42.89에서 직전 spec을 읽어 값만 바꾸는 스크립트로 충분히 돌았고, 행 문구는 train마다 사람이 판단하기 때문이다.

## 4. 기대 효과

| 항목 | 지금 | 3c 뒤 |
| --- | --- | --- |
| pair task 수 | `7` | `1` |
| pair Lane 2 tool batch | 약 `8` | 약 `2` |
| 손으로 옮기는 facts 값 | `243` | Lane 3와 서술 판단 값만 |
| 저장소 밖 임시 스크립트 | ZIP, 명령 `6`개 | 없음 |

## 5. 위험과 정차 조건

- orchestrator는 실제 pair에 쓴 적이 없다. 리허설 전에는 train에 넣지 않는다.
- 리허설 중 FAIL이 나면 orchestrator의 restoration(target catalog Update)과 fullgate 재실행으로 설치본을 되돌린다. 되돌리지 못하면 멈추고 보고한다.
- dedicated host reservation은 호스트당 하나다. 리허설 reservation은 리허설 artifact root 안에 두고 소비한다.
- facts 생성기가 서술 줄을 정해진 문장으로 만들므로 0.42.89 문서와 문장이 조금 다를 수 있다. golden은 0.42.89 facts 그대로 두고, 생성기 출력은 다음 train부터 쓴다.

## 6. 승인 단위

| 단계 | 승인 |
| --- | --- |
| 3a | Lane 1 구현, commit, PR, merge |
| 3b | Lane 2 host mutation(위 범위), 리허설 evidence PR과 merge |
| 3c | 절차 문서 변경 PR과 merge |

## 7. Nonclaims

- 이 문서는 설계 검토다. 이 문서를 쓰는 동안 호스트, 설치본, `current-evidence.json`을 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
