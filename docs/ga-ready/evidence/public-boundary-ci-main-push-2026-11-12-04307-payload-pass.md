# Public Boundary CI main push 2026-11-12 `0.43.7` payload commit

evidence_id: `public-boundary-ci-main-push-2026-11-12-04307-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `bf5c2143e9e8ea90809b529dbf49805dd61c0a15`
public_boundary_run_id: `43017019394`
public_boundary_job_id: `130510516593`
development_gates_run_id: `43017019405`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04307-candidate-sources`
release_train: `0.43.7-admin-smoke`
canonical_current_evidence: `0.43.7-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `776e4953f19800ad7f4438b18dfafde337f1148d`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.43.7`의 payload commit `bf5c214`는 PR #160 merge다(제품 소스는 PR #159 merge `dc042b8`와 같다). 그 commit의 main push 두 workflow가 success이고, train head `776e495`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `43017019394` job `130510516593`(`public-boundary-ci-required`).
- Development Gates run `43017019405`. shard `dotnet` job `130510519499`, `web` job `130510519355`, `delivery` job `130510519087`, `installer-policy` job `130510519410`.

- `train-path-check --payload bf5c2143e9e8ea90809b529dbf49805dd61c0a15 --head 776e4953f19800ad7f4438b18dfafde337f1148d` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=17`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.43.7` 제품 변경(PR #152, #153, #157, #159, 변경 commit `ff40acd7`, `25709198`, `39976fd2`, `04b1a91b`)은 각 PR merge에 있고 payload `bf5c214`에서 도달한다.

- clean package는 `74b9562`에서 만들었다(clean MSI `a808cda4…`).
- fullgate gate build는 `3898139`다(operational MSI `e6a89367…`). product payload는 `bf5c214`와 같다.
- `74b9562`, `3898139`, `bf5c214`는 모두 train head `776e495`에서 도달한다.

2026-11-12 Lane 3가 canonical current를 `0.43.7-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
