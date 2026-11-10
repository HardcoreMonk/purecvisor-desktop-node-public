# Public Boundary CI main push 2026-11-10 `0.43.1` payload commit

evidence_id: `public-boundary-ci-main-push-2026-11-10-04301-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `c685bd514a5c9e6f42e3e647ec6177260e541b6d`
public_boundary_run_id: `42726128946`
public_boundary_job_id: `129532099888`
development_gates_run_id: `42726129133`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04301-candidate-sources`
release_train: `0.43.1-admin-smoke`
canonical_current_evidence: `0.43.1-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `1623d62241bab20e001a0956c149db451fbb721a`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.43.1`의 payload commit `c685bd51`은 PR #134 merge다. 그 commit의 main push 두 workflow가 success이고, train head `1623d62`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `42726128946` job `129532099888`(`public-boundary-ci-required`).
- Development Gates run `42726129133`. shard `dotnet` job `129532103988`, `web` job `129532104414`, `delivery` job `129532104244`, `installer-policy` job `129532104205`.

- `train-path-check --payload c685bd514a5c9e6f42e3e647ec6177260e541b6d --head 1623d62241bab20e001a0956c149db451fbb721a` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=18`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.43.1` 제품 변경(Web Console backlog counts BL-0036~BL-0041, 변경 commit `1e06c13c`)은 PR #134 merge에 있고 payload `c685bd51`에서 도달한다.

- clean package는 `0be6561`에서 만들었다(clean MSI `37c3163f…`).
- fullgate gate build는 `143a55b`다(operational MSI `f4f3f30d…`). product payload는 `c685bd51`과 같다.
- `0be6561`, `143a55b`, `c685bd51`은 모두 train head `1623d62`에서 도달한다.

2026-11-10 Lane 3가 canonical current를 `0.43.1-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
