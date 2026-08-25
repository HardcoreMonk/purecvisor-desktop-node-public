# Public boundary CI main push 2026-07-13 PR #171 post-merge

result: `PASS`
scope: `post-04262-pr171-main-push`
workflow: `Public Boundary Contract`
job_name: `public-boundary-ci-required`
run_id: `29260188921`
job_id: `86851059567`
head_sha: `e08c67ce2bb80529270e258419948e3c573462c0`
run_url: `[private-archive-repository]/actions/runs/29260188921`
merge_commit_title: `Merge pull request #171 from HardcoreMonk/codex/wmi-internal-switch-topology-recovery`
source_pull_request: `[private-archive-repository]/pull/171`
host_mutation_performed: `false`
product_payload_change_detected: `false`
source_version_anchor: `0.42.62-admin-smoke`
additional_package_candidate_opened: `false`
package_candidate_decision: `docs-only-followup-does-not-open-0.42.63`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

PR #171 merge head `e08c67ce2bb80529270e258419948e3c573462c0`에 대한 main push
`Public Boundary Contract` run `29260188921`과 필수 job
`public-boundary-ci-required` `86851059567`은 모두 `success`로 종료했다.

같은 head의 `Development Gates` run `29260188365`도 `success`였으며 job 결과는 다음과 같다.

| job | job_id | conclusion |
| --- | ---: | --- |
| `installer-web-pester` | `86851058296` | `success` |
| `dotnet-tests` | `86851058332` | `success` |
| `packaging-pester` | `86851058335` | `success` |
| `web-tests` | `86851058430` | `success` |

이 문서는 merge 이후 CI 관찰 결과만 기록한다. 제품 payload 변경이나 host mutation을 수행하지
않았으며 `0.42.63-admin-smoke` package candidate를 열지 않는다. 이 결과는 public trusted
signing 또는 외부 stable publication evidence가 아니다.
