# Public Boundary CI main push 2026-10-03 `0.42.88` PR #31 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-03-04288-pr31-postmerge-pass`
result: `PASS`
scope: `post-pr31-merge-main-push`
head_sha: `d27086fc81984aeb2ec37d18a0bed341ee2b62ff`
public_boundary_run_id: `37122873756`
public_boundary_job_id: `111202298202`
development_gates_run_id: `37122873755`
product_payload_change_detected: `false`
package_candidate_decision: `pr31-postmerge-includes-04288-candidate-sources`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #31 `lane2/04288-package-pair-20261003`를 merge한 뒤 `origin/main` `d27086f`에서 두 workflow가 success다.

- Public Boundary run `37122873756` job `111202298202`(`public-boundary-ci-required`)
- Development Gates run `37122873755`. shard `dotnet` job `111202298609`, `web` job `111202298853`, `delivery` job `111202298744`, `installer-policy` job `111202298836`

이 push의 첫 부모 `ed4a4fa`(PR #30 merge)와 `d27086f` 사이에서 `src`, `web`, installer, product wrapper와 module, `config`, `.github` 경로는 바뀌지 않았다. PR #31이 더한 것은 0.42.88 package pair 문서와 campaign 기록이다.

`0.42.88` 제품 변경(managed delete 디스크 정리 `96e8570`)은 그 앞에 merge된 PR #30 `ed4a4fa`에 있다.

- clean package는 `ff62e59`에서 만들었다(clean MSI `81ef8527…`).
- fullgate gate build는 `47ff198`다(operational MSI `32b35113…`). product payload는 `ed4a4fa`와 같다.
- 두 commit은 모두 `d27086f`에서 도달한다.

2026-10-03 Lane 3가 canonical current를 `0.42.88-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
