# Release train Lane 3 spec 생성기 설계

- Design-ID: `pcv-train-lane3-spec-generator-v1`
- 작성일: `2026-10-05`
- 문서 상태: `accepted` (2026-10-05 사용자 승인 `1,2`의 2, campaign `train-lane3-spec-generator-20261005`)
- 변경 등급: M (검증 도구와 train 절차. 제품 동작 변경 없음)
- 상위 설계: `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-release-train-design.md` §9,
  `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3c
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`

## 1. 문제

train 마지막 단계(Lane 3)는 `packaging/windows-desktop-node/tools/Invoke-PcvLane3PromotionDocs.ps1`이 읽는 spec
`packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-<tag>.json`(계약 `pcv-lane3-promotion-docs-v1`)이
필요하다. 지금은 직전 spec을 복사해 값을 손으로 바꾼다. 0.42.88 → 0.42.89에서 `82`줄이 바뀌었다.

그런데 그 값 대부분은 같은 train이 이미 커밋한 facts 파일 `docs/ga-ready/trains/<version>.evidence-facts.json`에 있다.
facts 파일은 pair, fullgate, current-card와 Lane 3 문서 세 개(functional carry-forward, pair consume, main push)를 합쳐
문서 `12`개의 값을 갖는다. 같은 값을 두 번 손으로 옮기면 어긋날 수 있다.

## 2. 값 출처 (04289 실측)

04289 spec의 말단 값 `103`개를 04288 spec과 비교했다(`same 21`, `rotated 4`, `new 78`).

| 분류 | 개수(약) | 예 | 출처 |
| --- | ---: | --- | --- |
| 상수 | `21` | `contract`, `schema_version`, row `id`, `next_manual_admin_package_pair_payload_changed_source_file_count=0` | 생성기 코드 |
| 직전 spec 회전 | `4` | `previous_tag`(직전 `next_previous_tag`), `p0_feature_ledger_version` | `previous_spec` |
| version·tag·날짜 틀 | `14` | `next_previous_tag`, `promoted_tag`, `descriptor_id`, `current_status`, `next_manual_admin_package_pair_trigger`, `current_required_ci_docs_only_package_candidate_decision` | `version`, facts 문서 `date` |
| facts 값 | `48` | 경로, artifact root, batch id, MSI·ZIP·summary SHA-256, provenance commit, main push run/job id, head SHA | facts 문서 값과 문서 경로 |
| ledger 행 status·evidence 틀 | `14` | `` `pass`, `0.42.89-admin-smoke` ``, evidence 경로 나열 | facts 값으로 채운 문장 틀 |
| 사람 판단 | `9` | ledger 행 `rule` `7`개(supersede `0`~`3`, replace `0`~`2`), `functional_note`, `updated_at` | 입력 `narrative` |

facts 문서와 spec 값의 대응(대표):

| spec 값 | facts |
| --- | --- |
| `current_manual_admin_package_pair`, `latest_manual_admin_candidate_package_pair` | `pair-consume.baseline_version` `->` `target_version` |
| `current_manual_admin_campaign`, `index_sections.pair_evidence`, `ledger_head.current_manual_admin_evidence` | `pair-consume` 문서 경로 |
| `current_manual_admin_campaign_root` | `pair-consume.artifact_root` |
| `current_manual_admin_descriptor_batch_manifest`, `ledger_head.current_descriptor_batch_id` | `pair-consume.descriptor_batch_id` |
| `current_manual_admin_descriptor_summary` | `pair-consume.artifact_root` + `/manual-admin-campaign-descriptor/summary.json` |
| `current_manual_admin_target_msi_sha256` | `pair-consume.approved_target_msi_sha256` |
| `current_manual_admin_update_package_sha256` | `pair-consume.update_package_sha256` |
| `current_manual_admin_target_package_root` | `package.artifact_root` |
| `latest_manual_admin_candidate_provenance_commit` | `package.source_commit` |
| `current_full_admin_host_mutation_*`, `latest_full_admin_gate_batch` | `fullgate` 문서 경로, `batch_id`, `operational_fullgate_*`, `provenance_commit`, artifact root 두 개 |
| `next_manual_admin_package_pair_payload_change_source_commit_range` | `fullgate.provenance_commit` + `..next-admin-smoke-required` |
| `current_installed_operator_surface_current_card_*`, `current_full_admin_host_mutation_current_card` | `current-card` 문서 경로, `artifact_summary`, `summary_sha256` |
| `current_functional_correctness_actual_host_evidence` | `functional-carryforward` 문서 경로 |
| `current_public_boundary_main_push_*`, `index_sections.main_push_evidence` | `main-push` 문서 경로, `public_boundary_run_id`, `public_boundary_job_id`, `head_sha`, `package_candidate_decision`, `pr` |
| `current_manual_admin_days_since_previous_closure` | facts `date` − 직전 spec `index_sections.date` |

`release-train.json`은 값의 출처가 아니라 확인 대상이다. 생성기는 입력 `version`이 train 행으로 있고 그 상태가
`running`이나 `promoted`인지 본다. 다른 train을 위한 spec을 만드는 실수를 막는다.

## 3. 결정

### 3.1 위치와 명령

3a facts 생성기(`pcvverify train-facts`) 선례를 따라 C# `src/DesktopNode.Verification/TrainEvidence/`에 둔다. Required CI는
PowerShell을 부르지 않으므로 생성기와 golden 시험이 CI에서 실제로 돈다.

    dotnet run --project src/DesktopNode.Verification -c Release -- lane3-spec --input <lane3-spec-input.json> (--write | --check)

`--write`는 spec을 쓰고, `--check`는 커밋된 spec이 생성 결과와 byte 단위로 같은지 본다. 결과 JSON 계약은
`pcv-train-lane3-spec-result-v1`이다.

### 3.2 입력 계약 `pcv-train-lane3-spec-input-v1`

```json
{
  "schema_version": 1,
  "contract": "pcv-train-lane3-spec-input-v1",
  "version": "0.42.89-admin-smoke",
  "previous_spec": "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04288.json",
  "facts": "docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json",
  "output": "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04289.json",
  "narrative": {
    "updated_at": "2026-10-04T11:42:00+09:00",
    "functional_note": "...",
    "rules": {
      "full-admin-host-mutation-current": "...",
      "manual-admin-package-pair-current": "...",
      "package-build-current": "...",
      "installed-operator-surface-smoke-latest": "...",
      "latest-product-payload-smoke": "...",
      "functional-correctness-actual-host-latest": "...",
      "manual-admin-package-pair-next": "..."
    }
  }
}
```

- 경로는 저장소 상대 경로만 받는다. `..`와 절대 경로는 거부한다.
- `output` 파일 이름의 tag는 `version`에서 만든 tag(`0.42.89` → `04289`)와 같아야 한다.
- `narrative`에 생성 값과 같은 키가 있으면 거부한다(`narrative-conflict`). 틀이 요구하는 사람 값이 없으면 거부한다(`narrative-missing`).
- 입력 파일은 train마다 `docs/ga-ready/trains/<version>.lane3-spec-input.json`으로 커밋한다.

### 3.3 golden

`docs/ga-ready/trains/0.42.89-admin-smoke.lane3-spec-input.json`을 더하고, Verification 시험이 `--check`와 같은 경로로
04288 spec과 0.42.89 facts에서 04289 fixture를 byte 단위로 다시 만드는지 본다. 기존 fixture와 facts는 바꾸지 않는다.
맞지 않는 값은 출처 분류를 고치거나, 판단이 들어간 값이면 `narrative`로 옮기고 이 문서 §2에 적는다.

### 3.4 train 절차

train task 7(Lane 3)은 evidence 문서와 `current-evidence.json`을 쓴 뒤, Lane 3 문서 세 개를 facts에 더해 렌더하고,
입력 파일을 만들어 `lane3-spec --write`로 spec을 만든다. 그 다음은 지금처럼 `Invoke-PcvLane3PromotionDocs.ps1`
dry-run, `-Apply`, `-Check`다. 절차 문서는 Task 5에서 바꾼다.

## 4. 시험

- 입력 계약: 필수 필드, 경로 규칙, tag 불일치, `narrative-conflict`, `narrative-missing`, train 행 없음.
- 값 생성: 합성 facts로 descriptor chain, ledger head, ledger 행, index 절의 값과 키 순서.
- golden: 0.42.89 입력으로 04289 fixture byte 일치.

새 packaging `*.Tests.ps1`은 만들지 않는다.

## 5. 비목표

- `Invoke-PcvLane3PromotionDocs.ps1`과 `pcv-lane3-promotion-docs-v1` 계약 변경
- `current-evidence.json`과 Lane 3 evidence 문서 생성
- 사람 판단 문장(ledger 행 `rule`, functional note)의 자동 작성

## 6. Nonclaims

- 이 설계는 Lane 1 도구 변경이다. 호스트, 설치본, `current-evidence.json`을 바꾸지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
