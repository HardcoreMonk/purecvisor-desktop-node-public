# Public Boundary CI main push 2026-11-09 `0.42.99` PR #118 postmerge

evidence_id: `public-boundary-ci-main-push-2026-11-09-04299-pr118-postmerge-pass`
result: `PASS`
scope: `post-pr118-merge-main-push`
head_sha: `0be27eabad7f98247e7263b858a76e30d599e174`
public_boundary_run_id: `42517685241`
public_boundary_job_id: `128844192221`
development_gates_run_id: `42517685238`
product_payload_change_detected: `false`
package_candidate_decision: `pr118-postmerge-includes-04299-candidate-sources`
release_train: `0.42.99-admin-smoke`
canonical_current_evidence: `0.42.99-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.99`의 PR #118 `lane1/train-04299-20261109`를 merge한 뒤 `origin/main` `0be27ea`에서 두 workflow가 success다. train branch는 Lane 3 pin을 쓸 수 없어(`train-path-check`), 이 train은 두 PR로 돈다(0.42.92, 0.42.96, 0.42.98 선례).

- Public Boundary run `42517685241` job `128844192221`(`public-boundary-ci-required`)
- Development Gates run `42517685238`. shard `dotnet` job `128844192405`, `web` job `128844192330`, `delivery` job `128844192228`, `installer-policy` job `128844192477`

이 push의 첫 부모 `f7426d1`(PR #117 merge, payload commit)와 `0be27ea` 사이에서 `src`, `config`, `web/src`, `.github` 경로는 바뀌지 않았다(`train-path-check --payload f7426d1` `product_paths`·`allowed_pin_paths` 없음). PR #118이 더한 것은 train 출발 기록, 0.42.99 package, 계정 보관·복원, pair, fullgate, current-card, Lane 2 probe 기록이다.

`0.42.99` 제품 변경(PR #113 `154d16cc`, #115 `0575b859`, #116 `5f6c1f0c`)은 payload commit `f7426d1`에서 도달한다. 그 뒤 `main`에는 `src` 변경이 없어 MSI payload(`DesktopNode.Host`와 그 참조 Api·HyperV·Service, web, wrapper)는 같다.

- clean package는 `4931d6d`에서 만들었다(clean MSI `7946f464…`).
- fullgate gate build는 `a95987b`다(operational MSI `7c78f89e…`). product payload는 `f7426d1`과 같다.
- 세 commit(`4931d6d`, `a95987b`, `f7426d1`)은 모두 `0be27ea`에서 도달한다.

2026-11-09 Lane 3가 canonical current를 `0.42.99-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
