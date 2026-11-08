# Public Boundary CI main push 2026-11-08 `0.42.98` PR #109 postmerge

evidence_id: `public-boundary-ci-main-push-2026-11-08-04298-pr109-postmerge-pass`
result: `PASS`
scope: `post-pr109-merge-main-push`
head_sha: `4e8562f43c897c62bc904e7282d590e1fa1ae548`
public_boundary_run_id: `42467121432`
public_boundary_job_id: `128689088875`
development_gates_run_id: `42467121444`
product_payload_change_detected: `false`
package_candidate_decision: `pr109-postmerge-includes-04298-candidate-sources`
release_train: `0.42.98-admin-smoke`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.98`의 PR #109 `lane1/train-04298-20261108`를 merge한 뒤 `origin/main` `4e8562f`에서 두 workflow가 success다. train branch는 Lane 3 pin을 쓸 수 없어(`train-path-check`), 이 train은 두 PR로 돈다(0.42.92, 0.42.96 선례).

- Public Boundary run `42467121432` job `128689088875`(`public-boundary-ci-required`)
- Development Gates run `42467121444`. shard `dotnet` job `128689089057`, `web` job `128689089227`, `delivery` job `128689089320`, `installer-policy` job `128689089343`

이 push의 첫 부모 `8e4ebe2`(PR #108 merge, payload commit)와 `4e8562f` 사이에서 `src`, `config`, `web/src`, `.github` 경로는 바뀌지 않았다(`train-path-check --payload 8e4ebe2` `product_paths`·`allowed_pin_paths` 없음). PR #109가 더한 것은 train 출발 기록, 0.42.98 package, pair, fullgate, current-card, Lane 2 probe 기록과 Web Console 시연 캡처다.

`0.42.98` 제품 변경(PR #94 `271ab040`, #95 `08c50e08`, #97~#103, #104 `213bfe36`, #105~#107, #108 `d8bf7be9`)은 payload commit `8e4ebe2`에서 도달한다. 그 뒤 `main`에는 `src` 변경이 없어 MSI payload(`DesktopNode.Host`와 그 참조 Api·HyperV·Service, web, wrapper)는 같다.

- clean package는 `54cb61e`에서 만들었다(clean MSI `9a26d4ce…`).
- fullgate gate build는 `2dfe2ac`다(operational MSI `0a42b89f…`). product payload는 `8e4ebe2`와 같다.
- 세 commit(`54cb61e`, `2dfe2ac`, `8e4ebe2`)은 모두 `4e8562f`에서 도달한다.

2026-11-08 Lane 3가 canonical current를 `0.42.98-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
