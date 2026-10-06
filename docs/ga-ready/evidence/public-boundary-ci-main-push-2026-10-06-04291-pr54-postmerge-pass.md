# Public Boundary CI main push 2026-10-06 `0.42.91` PR #54 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-06-04291-pr54-postmerge-pass`
result: `PASS`
scope: `post-pr54-merge-main-push`
head_sha: `4b0465392534eb3b2f1b1fbb5efb3e46a1f479bd`
public_boundary_run_id: `37460495186`
public_boundary_job_id: `112258565324`
development_gates_run_id: `37460494865`
product_payload_change_detected: `false`
package_candidate_decision: `pr54-postmerge-includes-04291-candidate-sources`
release_train: `0.42.91-admin-smoke`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.91`의 PR #54 `train/04291-20261006`를 merge한 뒤 `origin/main` `4b04653`에서 두 workflow가 success다.

- Public Boundary run `37460495186` job `112258565324`(`public-boundary-ci-required`)
- Development Gates run `37460494865`. shard `dotnet` job `112258565031`, `web` job `112258564798`, `delivery` job `112258565017`, `installer-policy` job `112258564643`

이 push의 첫 부모 `05f42a2`(PR #53 merge)와 `4b04653` 사이에서 `src`, `web/src`, `config`, `.github` 경로는 바뀌지 않았다. PR #54가 더한 것은 train 출발 기록, 0.42.91 package, pair, fullgate, current-card, 완료 probe 문서다.

`0.42.91` 제품 변경(Hyper-V Notes 원소 하나 쓰기 `c8be5bc`)은 train에 실은 PR #53 `05f42a2`에 있다.

- clean package는 `59cd8b6`에서 만들었다(clean MSI `bdef7609…`).
- fullgate gate build는 `990a4b2`다(operational MSI `46dddccb…`). product payload는 `05f42a2`와 같다.
- 두 commit은 모두 `4b04653`에서 도달한다.

2026-10-06 Lane 3가 canonical current를 `0.42.91-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
