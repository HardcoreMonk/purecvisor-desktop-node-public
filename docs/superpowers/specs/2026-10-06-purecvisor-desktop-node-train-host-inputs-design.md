# Train 호스트 입력 생성과 Lane 2 probe 스크립트 추적 설계

- Design-ID: `pcv-train-host-inputs-v1`
- 작성일: `2026-10-06`
- 문서 상태: `accepted` (2026-10-06 사용자 승인 `1,2,3,4`의 3, campaign `process-optimization-20261006`)
- 변경 등급: M (train 도구와 절차. 제품 동작 변경 없음)
- host/VM/service/package mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 문제

release train 한 번에 호스트에서 돌리는 입력 세 가지를 지금은 손으로 만든다.

| 입력 | 지금 방식 | train마다 바뀌는 값 |
| --- | --- | --- |
| fullgate batch manifest (`artifacts/batch-manifests/full-admin-host-mutation-gate-<tag>.json`) | 직전 manifest를 복사해 값을 바꾼다 | version `2`곳, 날짜 tag가 든 경로 `6`곳 |
| current-card 캡처 스크립트 (`capture-current-card.ps1`, `131`줄) | 직전 스크립트를 복사해 값을 바꾼다 | artifact root, payload 경로 `2`곳, version `2`곳, evidence id, fullgate batch, SHA-256 상수 `4`개, `provenance_commit`, `canonical_current_evidence` |
| Lane 2 probe 스크립트 | 세션 scratchpad에 새로 쓴다 | 기능군마다 전부 |

근거(2026-10-06 점검):

- 0.42.90과 0.42.91의 manifest·캡처 스크립트 diff는 위 값만 다르다. train 계획 실행 기록에도 "직전 스크립트에서 ~를 바꿔"가 남아 있다(0.42.90 `2`회, 0.42.91 `3`회).
- probe 스크립트는 `git ls-files`에 없고 artifact에는 결과 JSON만 남는다. 2026-10-06 완료 probe 스크립트 `5`개(`649`줄)는 지난 세션의 임시 폴더에만 있다. P1-10 probe r1은 그 스크립트의 결함(빈 `vm list`가 `$null`로 풀림)으로 멈췄다.
- manifest에는 사설 LAN 주소(`-LanPrefix`)와 실행 사용자(`created_by`)가 들어 있어 그대로 저장소에 둘 수 없다.

## 2. 목표와 비목표

목표:

- fullgate manifest와 current-card 캡처 스크립트를 tracked 템플릿과 train facts에서 만든다. 손으로 옮기는 SHA-256, commit, version을 없앤다.
- 0.42.91 실제 manifest와 캡처 스크립트를 golden으로 재현해 생성기가 지금 절차와 같은 입력을 만든다는 것을 Required CI에서 고정한다.
- Lane 2 probe 스크립트를 저장소에서 추적하고 매개변수로 일반화한다. 실행 없이 확인할 수 있는 plan-only 모드를 둔다.

비목표:

- fullgate, current-card, probe를 실행하지 않는다. 판정 기준과 evidence 내용은 바꾸지 않는다.
- Lane 2 probe evidence 문서는 계속 손으로 쓴다(train 설계 §6과 같다).
- 사설 LAN 주소, guest 인증 정보, token을 저장소에 두지 않는다.

## 3. 설계

### 3.1 `pcvverify train-host-inputs`

`DesktopNode.Verification`의 train 도구 옆에 명령 하나를 더한다. C#이므로 Required CI에서 시험한다.

```text
dotnet run --project src/DesktopNode.Verification -c Release -- train-host-inputs \
  --input docs/ga-ready/trains/<version>.host-inputs.json --kind <fullgate-manifest|current-card> --write
```

- 입력 `docs/ga-ready/trains/<version>.host-inputs.json`(계약 `pcv-train-host-inputs-v1`)은 version, 날짜 tag, train facts 경로, smoke ISO 상대 경로만 담는다. tracked이므로 사설 값은 없다.
- 실행 경계 값은 명령줄이나 환경 변수로만 받는다. 저장소 root는 실행 위치에서 구하고, LAN prefix는 `PCV_TRAIN_LAN_PREFIX`, `created_by`는 실행 사용자 이름이다. 출력은 ignored `artifacts/` 아래에만 쓴다.
- 출력 파일이 이미 있으면 쓰지 않고 멈춘다(`train-host-inputs:output-exists:<output>`). 필요한 fact가 없으면 이름을 담아 멈춘다(`train-host-inputs:fact-missing:<template>.<key>`). 오류 코드는 다른 train 도구와 같은 `PCV_CONFIG_INVALID` 계열이다. 기존 artifact를 덮어쓰지 않는 규칙과 같다.
- `--plan`은 아무것도 쓰지 않고 출력 경로와 SHA-256만 보여 준다.

### 3.2 fullgate manifest

- 템플릿 `docs/ga-ready/trains/host-templates/fullgate-batch-manifest.json.tmpl`. 값 자리는 `{{version}}`, `{{tag}}`, `{{repo_root}}`, `{{iso}}`, `{{lan_prefix}}`, `{{created_by}}`다.
- `path_redactions`도 실행 값으로 채운다(`{{lan_prefix}}` 호스트 → `[redacted-private-endpoint]`, `{{repo_root}}` → `[REPO_ROOT]`).
- 출력: `artifacts/batch-manifests/full-admin-host-mutation-gate-<tag>.json`.
- fullgate 전에 만들 수 있어야 하므로 train facts가 아니라 입력 파일만 읽는다.

### 3.3 current-card 캡처 스크립트

- 템플릿 `docs/ga-ready/trains/host-templates/capture-current-card.ps1.tmpl`. 지금 스크립트의 본문을 그대로 두고 1장의 값만 자리로 바꾼다.
- 값 출처:

  | 값 | 출처 |
  | --- | --- |
  | clean MSI·payload SHA-256 | train facts `package` 문서 |
  | fullgate MSI·payload SHA-256, gate build commit, fullgate batch | train facts `fullgate` 문서 |
  | `canonical_current_evidence` | train facts `package` 문서. 출발 때 `docs/ga-ready/current-evidence.json`의 `current.version`을 담으므로 승격 뒤에도 같은 값을 재현한다 |
  | version, tag, evidence id | 입력 파일 |

- 출력: `artifacts/installed-operator-surface-current-card-<tag>.capture.ps1`. artifact root 밖에 두는 지금 규칙(스크립트가 root가 있으면 멈춤)을 지킨다. 캡처 뒤 지금처럼 root 안에 복사본을 남긴다.
- fullgate 문서를 facts에 렌더한 뒤(train task 4 앞) 만든다.

### 3.4 Lane 2 probe 스크립트

- 위치: `packaging/windows-desktop-node/lane2-probes/`. 파일 이름은 `Invoke-PcvLane2Probe<기능군>.ps1`, 공통 함수는 `PcvLane2ProbeCommon.ps1`(dot-source)이다. 새 `*.Tests.ps1`는 만들지 않는다(Delivery inventory와 migration manifest).
- 첫 묶음은 2026-10-06 완료 probe 세 개다. inventory·template lock(P1-6·P1-7), account·noVNC target(P1-9·P2-11), family reconcile(P1-10).
- 공통 매개변수: `-RepoRoot`, `-ArtifactRoot`(있으면 멈춤), `-ProbePrefix`(probe VM·계정 이름, 기본 `pcv-probe-`), `-PlanOnly`.
- `-PlanOnly`는 `pcvcli`나 Hyper-V를 부르지 않는다. 단계 목록과 정리 대상을 `plan.json`으로 쓰고 끝난다.
- 비밀 값: password, token, credential 상수를 두지 않는다. probe 계정 비밀번호는 실행 중 `RandomNumberGenerator`로 만들고 환경 변수로만 `pcvcli`에 넘기며, summary와 evidence에 남기지 않는다. guest 인증 정보는 지금처럼 clean-host runner 기본값을 runner 소스에서 읽는다.
- 2026-10-06 r1의 `$null` 결함은 공통 함수 `ConvertTo-PcvArray`로 막는다. 정리 경로는 `Join-Path`만 쓴다(r2·r3 정리 결함).
- 카탈로그 `packaging/windows-desktop-node/lane2-probes/catalog.json`(계약 `pcv-lane2-probe-catalog-v1`)에 probe id, 기능군, 대상 SERVICE_PLAN 항목, host mutation 종류(VM 생성·삭제, 계정 생성·비활성화, noVNC target 쓰기), 필요한 승인 문구를 적는다.

## 4. 시험

모두 C#이고 Required CI의 `dotnet`, `delivery` shard에서 돈다. PowerShell을 실행하지 않는다.

| 시험 | 내용 |
| --- | --- |
| `TrainHostInputsTests` (current-card) | 0.42.91 입력과 그 train의 repo root로 만든 캡처 스크립트의 SHA-256이 facts `current-card` 문서의 `capture_script_sha256`(실제로 돈 스크립트)과 같다. 임시 저장소에서 `--plan`·`--write` 뒤 다시 쓰면 멈추고, fact가 없으면 그 이름으로 멈추며 아무것도 쓰지 않는다 |
| `TrainHostInputsTests` (fullgate manifest) | 0.42.91 입력으로 만든 manifest가 golden과 byte 단위로 같다. golden은 실제 0.42.91 manifest에서 LAN 주소를 `192.0.2.10`(문서용 주소), 사용자를 `pcv-operator`로 바꾼 것이다. LAN prefix가 없으면 manifest를 쓰지 않는다 |
| `Lane2ProbeCatalogContractTests` | 카탈로그의 스크립트가 모두 있고, `-PlanOnly`·`-ArtifactRoot` 매개변수와 root 존재 시 멈춤이 있으며, 비밀 값 상수 패턴과 사설 주소가 없다 |

## 5. 절차 변경

- `docs/DEVELOPMENT_PROCEDURE.md` §10 train task 3(fullgate)은 manifest를 `train-host-inputs --kind fullgate-manifest`로 만든다.
- §10 train task 4(current-card)는 "직전 스크립트를 복사해 바꾼다" 문단을 `train-host-inputs --kind current-card`로 바꾼다.
- §5 Lane 2 probe는 카탈로그에 있는 기능군이면 tracked 스크립트를 `-PlanOnly`로 먼저 돌린 뒤 실행한다. 카탈로그에 없는 기능군은 새 스크립트를 카탈로그와 함께 더한 뒤 실행한다.

## 6. 위험과 대응

| 위험 | 대응 |
| --- | --- |
| 템플릿과 실제 스크립트가 어긋남 | golden이 0.42.91 실제 파일을 재현한다. 템플릿을 바꾸면 golden도 같은 commit에서 바뀐다 |
| 사설 값이 tracked 파일로 샘 | LAN prefix와 사용자는 실행 값만 쓰고, 카탈로그 시험이 사설 주소 패턴을 거부한다 |
| probe 스크립트가 시험 없이 낡음 | Required CI는 정적 계약만 본다. 실제 동작은 다음 train Lane 2 probe에서 확인하고, 그 전에는 `-PlanOnly`로 확인한다 |

## 7. 도입 단계

| 단계 | 내용 | campaign task |
| --- | --- | --- |
| 1 | current-card 템플릿과 `train-host-inputs --kind current-card`, golden, §10 task 4 | Task 4 |
| 2 | fullgate manifest 템플릿과 `--kind fullgate-manifest`, golden, §10 task 3 | Task 5 |
| 3 | probe 스크립트 세 개, 공통 함수, 카탈로그와 계약 시험, §5 | Task 6 |

## 8. Nonclaims

- 이 설계를 쓰는 동안 호스트, 설치본, `current-evidence.json`을 바꾸지 않았다.
- 생성된 입력으로 돈 fullgate·current-card·probe의 PASS는 다음 train에서 확인한다. 이 문서는 그 결과를 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
