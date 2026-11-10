# Public Boundary CI main push 2026-11-11 `0.43.3` payload commit

evidence_id: `public-boundary-ci-main-push-2026-11-11-04303-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `69675e0cfd06056f074c0a65dbd3c88f2c6256cf`
public_boundary_run_id: `42841475949`
public_boundary_job_id: `129917539146`
development_gates_run_id: `42841475706`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04303-candidate-sources`
release_train: `0.43.3-admin-smoke`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `1fb874c530ef3d2133df0fe15587abac0dc86c4c`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.43.3`의 payload commit `69675e0c`은 PR #142 merge다. 그 commit의 main push 두 workflow가 success이고, train head `1fb874c`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `42841475949` job `129917539146`(`public-boundary-ci-required`).
- Development Gates run `42841475706`. shard `dotnet` job `129917538846`, `web` job `129917538596`, `delivery` job `129917538250`, `installer-policy` job `129917538840`.

- `train-path-check --payload 69675e0cfd06056f074c0a65dbd3c88f2c6256cf --head 1fb874c530ef3d2133df0fe15587abac0dc86c4c` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=18`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.43.3` 제품 변경(BL-0043 취소된 요청이 Web Console 상태를 쓰지 않음, 변경 commit `3d1ce598`)은 PR #142 merge에 있고 payload `69675e0c`에서 도달한다.

- clean package는 `e1e1b8b`에서 만들었다(clean MSI `66546521…`).
- fullgate gate build는 `db1a762`다(operational MSI `13f716ee…`). product payload는 `69675e0c`과 같다.
- `e1e1b8b`, `db1a762`, `69675e0c`은 모두 train head `1fb874c`에서 도달한다.

2026-11-11 Lane 3가 canonical current를 `0.43.3-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
