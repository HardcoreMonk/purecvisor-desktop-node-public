# Public Boundary CI main push 2026-11-11 `0.43.4` payload commit

evidence_id: `public-boundary-ci-main-push-2026-11-11-04304-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `e957f15b08e233ce9faffa809a178056c6b0e952`
public_boundary_run_id: `42866127722`
public_boundary_job_id: `130004810818`
development_gates_run_id: `42866127498`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04304-candidate-sources`
release_train: `0.43.4-admin-smoke`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `d5d1007acbfb5608a2fe8aa67e79ce83a6ef568d`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.43.4`의 payload commit `e957f15b`은 PR #146 merge다. 그 commit의 main push 두 workflow가 success이고, train head `d5d1007`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `42866127722` job `130004810818`(`public-boundary-ci-required`).
- Development Gates run `42866127498`. shard `dotnet` job `130004810150`, `web` job `130004810853`, `delivery` job `130004810658`, `installer-policy` job `130004810593`.

- `train-path-check --payload e957f15b08e233ce9faffa809a178056c6b0e952 --head d5d1007acbfb5608a2fe8aa67e79ce83a6ef568d` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=17`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.43.4` 제품 변경(BL-0044 대시보드 "경고" 카드 읽는 중 문구, 변경 commit `08589a5f`)은 PR #146 merge에 있고 payload `e957f15b`에서 도달한다.

- clean package는 `54a817c`에서 만들었다(clean MSI `c24ef831…`).
- fullgate gate build는 `7b929ec`다(operational MSI `c9f87149…`). product payload는 `e957f15b`과 같다.
- `54a817c`, `7b929ec`, `e957f15b`은 모두 train head `d5d1007`에서 도달한다.

2026-11-11 Lane 3가 canonical current를 `0.43.4-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
