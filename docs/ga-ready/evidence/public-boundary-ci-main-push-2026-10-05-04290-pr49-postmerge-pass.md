# Public Boundary CI main push 2026-10-05 `0.42.90` PR #49 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-05-04290-pr49-postmerge-pass`
result: `PASS`
scope: `post-pr49-merge-main-push`
head_sha: `366172a2eb7196a0c4dc5d879b0ac4d84ce6fbff`
public_boundary_run_id: `37311188276`
public_boundary_job_id: `111766733999`
development_gates_run_id: `37311188281`
product_payload_change_detected: `false`
package_candidate_decision: `pr49-postmerge-includes-04290-candidate-sources`
release_train: `0.42.90-admin-smoke`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.90`의 PR #49 `train/04290-20261005`를 merge한 뒤 `origin/main` `366172a`에서 두 workflow가 success다.

- Public Boundary run `37311188276` job `111766733999`(`public-boundary-ci-required`)
- Development Gates run `37311188281`. shard `dotnet` job `111766735949`, `web` job `111766735873`, `delivery` job `111766735778`, `installer-policy` job `111766735524`

이 push의 첫 부모 `a66a8cd`(PR #48 merge)와 `366172a` 사이에서 `src`, `web/src`, `config`, `.github` 경로는 바뀌지 않았다. PR #49가 더한 것은 train 출발 기록, 0.42.90 package, pair, fullgate, current-card 문서다.

`0.42.90` 제품 변경(Release build nullable 경고 정리 `b433c50`)은 train에 실은 PR #48 `a66a8cd`에 있다.

- clean package는 `0bcc328`에서 만들었다(clean MSI `54277baa…`).
- fullgate gate build는 `648139d`다(operational MSI `ac367ea4…`). product payload는 `a66a8cd`와 같다.
- 두 commit은 모두 `366172a`에서 도달한다.

2026-10-05 Lane 3가 canonical current를 `0.42.90-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
