# Public Boundary CI main push 2026-11-10 `0.43.2` payload commit

evidence_id: `public-boundary-ci-main-push-2026-11-10-04302-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `00b10b66baa7d704005ebf6638d48bcaa028c940`
public_boundary_run_id: `42804307082`
public_boundary_job_id: `129789439499`
development_gates_run_id: `42804306748`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04302-candidate-sources`
release_train: `0.43.2-admin-smoke`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `d67dcc6cbe932f6e22cc1ff7039760cda93a4679`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.43.2`의 payload commit `00b10b66`은 PR #138 merge다. 그 commit의 main push 두 workflow가 success이고, train head `d67dcc6`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `42804307082` job `129789439499`(`public-boundary-ci-required`).
- Development Gates run `42804306748`. shard `dotnet` job `129789442619`, `web` job `129789443130`, `delivery` job `129789442129`, `installer-policy` job `129789442504`.

- `train-path-check --payload 00b10b66baa7d704005ebf6638d48bcaa028c940 --head d67dcc6cbe932f6e22cc1ff7039760cda93a4679` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=18`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.43.2` 제품 변경(BL-0042 runtime policy `product_version`과 Web Console 설치 version 표시, 변경 commit `692895cf`, `424780d8`)은 PR #138 merge에 있고 payload `00b10b66`에서 도달한다.

- clean package는 `b45e821`에서 만들었다(clean MSI `fa6afbce…`).
- fullgate gate build는 `2f19778`다(operational MSI `82e78f2b…`). product payload는 `00b10b66`과 같다.
- `b45e821`, `2f19778`, `00b10b66`은 모두 train head `d67dcc6`에서 도달한다.

2026-11-10 Lane 3가 canonical current를 `0.43.2-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
