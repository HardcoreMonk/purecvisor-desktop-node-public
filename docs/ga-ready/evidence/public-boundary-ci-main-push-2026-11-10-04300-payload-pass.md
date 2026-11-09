# Public Boundary CI main push 2026-11-10 `0.43.0` payload commit

evidence_id: `public-boundary-ci-main-push-2026-11-10-04300-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `6355e6ba6ae5592fa669e34e84ef316925d1876f`
public_boundary_run_id: `42650495136`
public_boundary_job_id: `129275212333`
development_gates_run_id: `42650495121`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04300-candidate-sources`
release_train: `0.43.0-admin-smoke`
canonical_current_evidence: `0.43.0-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `9c315622228b7b166d2b212e3442c3cac0e3796a`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.43.0`의 payload commit `6355e6ba`는 PR #130 merge다. 그 commit의 main push 두 workflow가 success이고, train head `9c31562`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `42650495136` job `129275212333`(`public-boundary-ci-required`).
- Development Gates run `42650495121`. shard `dotnet` job `129275212799`, `web` job `129275212863`, `delivery` job `129275212918`, `installer-policy` job `129275212218`.

- `train-path-check --payload 6355e6ba6ae5592fa669e34e84ef316925d1876f --head 9c315622228b7b166d2b212e3442c3cac0e3796a` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=19`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.43.0` 제품 변경(Web Console UI 개편 U1~U8, 변경 commit `866d18ef`, `7b189a98`, `06d958af`, `a0178508`, `43bba65a`, `bdc5874e`, `388bf658`, `d32daa8f`)은 PR #123~#130 merge에 있고 payload `6355e6ba`에서 도달한다.

- clean package는 `87eb923`에서 만들었다(clean MSI `1eb5d984…`).
- fullgate gate build는 `1f646ee`다(operational MSI `32d7ec56…`). product payload는 `6355e6ba`와 같다.
- `87eb923`, `1f646ee`, `6355e6ba`는 모두 train head `9c31562`에서 도달한다.

2026-11-10 Lane 3가 canonical current를 `0.43.0-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
