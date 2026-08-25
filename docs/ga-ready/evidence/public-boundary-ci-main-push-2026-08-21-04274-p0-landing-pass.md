# Public boundary CI main push 2026-08-21 0.42.74 P0 landing

evidence_id: `public-boundary-ci-main-push-2026-08-21-04274-p0-landing-pass`
result: `PASS`
scope: `post-04274-p0-landing-main-push`
workflow: `Public Boundary Contract`
job_name: `public-boundary-ci-required`
run_id: `32388996125`
job_id: `96490306459`
head_sha: `5f9cecfd5507e7e5dd726601aae3760e4e1b558c`
run_url: `[private-archive-repository]/actions/runs/32388996125`
job_url: `[private-archive-repository]/actions/runs/32388996125/job/96490306459`
development_gates_run_id: `32388996111`
development_gates_run_url: `[private-archive-repository]/actions/runs/32388996111`
commit_title: `test(packaging): raise P0 module size and Hyper-V case ratchets`
source_pull_request: `none-direct-main-push`
merged_at: `2026-08-20T15:55:00Z`
merge_parent_main_sha: `17b9828c50add14901a1a632c8f5ccdca4645f08`
host_mutation_performed: `false`
product_payload_change_detected: `true`
changed_path_count: `86`
product_payload_path_count: `33`
docs_and_tests_only: `false`
current_version_anchor: `0.42.74-admin-smoke`
additional_package_candidate_opened: `false`
package_candidate_decision: `landed-already-validated-as-0.42.74-admin-smoke`
recursive_evidence_policy: `postmerge-evidence-docs-do-not-open-additional-package-candidate`
predecessor_failed_development_gates_run_id: `32388381230`
predecessor_failed_development_gates_head_sha: `15cce2f4c9865a52c48ec330da985fceb8f8b92a`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

local `main`이 origin보다 앞서 있던 SERVICE_PLAN P0 payload와
`0.42.74-admin-smoke` operational 승격을 `17b9828c..5f9cecfd` 범위로 직접 main push했다.
green head `5f9cecfd5507e7e5dd726601aae3760e4e1b558c`의 `Public Boundary Contract` run
`32388996125`과 필수 job `public-boundary-ci-required` `96490306459`가 모두 `success`다.

같은 head의 `Development Gates` run `32388996111`도 `success`이며 job 결과는 다음과 같다.

| job | job_id | conclusion |
| --- | ---: | --- |
| `web-tests` | `96490306336` | `success` |
| `dotnet-tests` | `96490306529` | `success` |
| `packaging-pester` | `96490306503` | `success` |
| `installer-web-pester` | `96490306137` | `success` |

직전 head `15cce2f4`의 Development Gates run `32388381230`은 packaging-pester가
module-size ratchet과 Hyper-V ownership inventory 상한을 넘겨 `failure`였다. 후속 커밋
`5f9cecfd`가 그 상한을 실측 값으로 올렸다. Public Boundary run `32388381239`는 그
failure head에서도 `success`였다.

## Product payload 및 version 판정

변경 경로 `86`개 중 non-test product payload 경로는 `33`개다. 이 payload는 이미
`0.42.74-admin-smoke` package/fullgate/manual-admin/functional/current-card로 검증되고
`current-evidence.json` current다. 따라서 이 main push는 새 package candidate를 열지
않고 `landed-already-validated-as-0.42.74-admin-smoke`로 닫는다.

04273 promotion docs-only postpush는 predecessor로 보존한다. 이 문서를 추가한 후속
docs-only 커밋마다 다시 전용 evidence를 만드는 재귀적 승격을 요구하지 않는다.

P0 `vm.save` actual-VM FAIL는 열린 결함이며 이 CI evidence가 고치지 않는다.

## Nonclaims

- 이 문서는 main push 이후 CI 관찰 결과만 기록하며 host mutation을 수행하지 않았다.
- CI success는 public trusted signing, trusted timestamp 또는 external stable publication
  evidence가 아니다.
- 이 evidence는 package/fullgate/manual-admin/functional/current-card 또는 token R4 evidence를
  대체하지 않는다.
