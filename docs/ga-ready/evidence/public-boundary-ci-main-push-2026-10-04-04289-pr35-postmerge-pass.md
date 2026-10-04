# Public Boundary CI main push 2026-10-04 `0.42.89` PR #35 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-04-04289-pr35-postmerge-pass`
result: `PASS`
scope: `post-pr35-merge-main-push`
head_sha: `8631947ccb6329384a41a86fc1c0e94778296eef`
public_boundary_run_id: `37171644656`
public_boundary_job_id: `111345611153`
development_gates_run_id: `37171644667`
product_payload_change_detected: `false`
package_candidate_decision: `pr35-postmerge-includes-04289-candidate-sources`
release_train: `0.42.89-admin-smoke`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.89`의 PR #35 `train/04289-20261004`를 merge한 뒤 `origin/main` `8631947`에서 두 workflow가 success다.

- Public Boundary run `37171644656` job `111345611153`(`public-boundary-ci-required`)
- Development Gates run `37171644667`. shard `dotnet` job `111345611517`, `web` job `111345611268`, `delivery` job `111345611455`, `installer-policy` job `111345611394`

이 push의 첫 부모 `d6711f3`(PR #34 merge)와 `8631947` 사이에서 `src`, `web/src`, `config`, `.github` 경로는 바뀌지 않았다. PR #35가 더한 것은 train 출발 기록, 0.42.89 package pair, fullgate, current-card, Lane 2 probe 문서다.

`0.42.89` 제품 변경(`pcvcli --json` 오류 출력 `0c95852`)은 train에 실은 PR #34 `d6711f3`에 있다.

- clean package는 `b463903`에서 만들었다(clean MSI `e4574861…`).
- fullgate gate build는 `a780928`다(operational MSI `fe5677ff…`). product payload는 `d6711f3`와 같다.
- 두 commit은 모두 `8631947`에서 도달한다.

2026-10-04 Lane 3가 canonical current를 `0.42.89-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
