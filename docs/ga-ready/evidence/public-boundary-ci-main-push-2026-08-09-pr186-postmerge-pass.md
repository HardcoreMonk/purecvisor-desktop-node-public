# Public boundary CI main push 2026-08-09 PR #186 post-merge

evidence_id: `public-boundary-ci-main-push-2026-08-09-pr186-postmerge-pass`
result: `PASS`
scope: `post-pr186-main-push`
workflow: `Public Boundary Contract`
job_name: `public-boundary-ci-required`
run_id: `31302773929`
job_id: `93218124085`
head_sha: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
run_url: `[private-archive-repository]/actions/runs/31302773929`
merge_commit_title: `Merge pull request #186 from HardcoreMonk/codex/followup-work-2026-08-08`
source_pull_request: `[private-archive-repository]/pull/186`
merged_at: `2026-08-09T08:07:44Z`
host_mutation_performed: `false`
product_payload_change_detected: `true`
source_version_anchor: `0.42.71-admin-smoke`
additional_package_candidate_opened: `true`
package_candidate_decision: `opened-and-validated-as-0.42.72-admin-smoke`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

PR #186 merge head `02428fab`에 대한 main push `Public Boundary Contract` run
`31302773929`과 필수 job `public-boundary-ci-required` `93218124085`가 모두
`success`로 종료했다.

같은 head의 `Development Gates` run `31302773934`도 `success`였으며 job 결과는
다음과 같다.

| job | job_id | conclusion |
| --- | ---: | --- |
| `installer-web-pester` | `93218124180` | `success` |
| `dotnet-tests` | `93218124155` | `success` |
| `packaging-pester` | `93218124207` | `success` |
| `web-tests` | `93218124181` | `success` |

PR title은 “follow-up: close 0.42.71 evidence, split large modules, and harden token rotation”이다.
이 merge는 product payload 변경을 포함하므로 같은 head에서 `0.42.72-admin-smoke` package
candidate를 열었고 package/fullgate/manual-admin/functional/current-card evidence가 후속으로
생성됐다.

## Nonclaims

- 이 문서는 merge 이후 CI 관찰 결과만 기록하며 host mutation을 수행하지 않았다.
- CI success는 public trusted signing, trusted timestamp 또는 external stable publication
  evidence가 아니다.
- installed token final evidence는 이 CI evidence가 대신하지 않는다.
