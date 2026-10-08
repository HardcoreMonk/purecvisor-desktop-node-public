# Public Boundary CI main push 2026-10-08 `0.42.93` payload commit

evidence_id: `public-boundary-ci-main-push-2026-10-08-04293-payload-pass`
result: `PASS`
scope: `payload-commit-main-push`
head_sha: `56e7cd0ff0df3a4ac2688ff2f4030fa10e72936c`
public_boundary_run_id: `37732628401`
public_boundary_job_id: `113165002134`
development_gates_run_id: `37732628393`
product_payload_change_detected: `false`
package_candidate_decision: `payload-main-push-includes-04293-candidate-sources`
release_train: `0.42.93-admin-smoke`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `true`
train_pr_head: `b6ecb7fe58ef8a5b39619e9ed1c85f33b9f23d6b`
path_check_contract: `pcv-train-path-check-result-v1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.93`의 payload commit `56e7cd0`는 PR #69 merge다. 그 commit의 main push 두 workflow가 success이고, train head `b6ecb7f`까지 제품 경로는 없다. 이 문서는 단일 PR train의 `main-push-payload`다.

- Public Boundary run `37732628401` job `113165002134`(`public-boundary-ci-required`).
- Development Gates run `37732628393`. shard `dotnet` job `113165002357`, `web` job `113165002584`, `delivery` job `113165002515`, `installer-policy` job `113165002470`.

- `train-path-check --payload 56e7cd0ff0df3a4ac2688ff2f4030fa10e72936c --head b6ecb7fe58ef8a5b39619e9ed1c85f33b9f23d6b` exit 0. 계약 `pcv-train-path-check-result-v1`, `ok=true`, `changed_path_count=19`, `product_paths` 없음, `allowed_pin_paths` 없음.

`0.42.93` 제품 변경(QoS readback `92146da`, `vm.create` reconcile `65c376d`)은 PR #68 merge `d9e7d03`에 있고 payload `56e7cd0`에서 도달한다.

- clean package는 `41421d8`에서 만들었다(clean MSI `13d7f0d4…`).
- fullgate gate build는 `818d00f`다(operational MSI `5d5a7c7c…`). product payload는 `56e7cd0`와 같다.
- `41421d8`, `818d00f`, `56e7cd0`는 모두 train head `b6ecb7f`에서 도달한다.

2026-10-08 Lane 3가 canonical current를 `0.42.93-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
