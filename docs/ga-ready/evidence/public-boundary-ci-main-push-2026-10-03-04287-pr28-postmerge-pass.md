# Public Boundary CI main push 2026-10-03 `0.42.87` PR #28 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-03-04287-pr28-postmerge-pass`
result: `PASS`
scope: `post-pr28-merge-main-push`
head_sha: `d48de5558c06946c5325b9e53c4d9e0a92f7c64c`
public_boundary_run_id: `37112963009`
public_boundary_job_id: `111174281306`
development_gates_run_id: `37112963018`
product_payload_change_detected: `true`
package_candidate_decision: `pr28-postmerge-includes-04287-candidate-sources`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #28 `docs/project-status-audit-20261003`를 merge한 뒤 `origin/main` `d48de55`에서 두 workflow가 success다.

- Public Boundary run `37112963009` job `111174281306`(`public-boundary-ci-required`)
- Development Gates run `37112963018`. shard `dotnet` job `111174281341`, `web` job `111174281592`, `delivery` job `111174281515`, `installer-policy` job `111174281537`

이 push의 첫 부모 `a5fc9fa`(PR #5 merge)와 `d48de55` 사이에서 제품 경로가 바뀌었다. 바뀐 것은 Api/Runtime reconcile 안내 문구, `installer/Product.wxs`, route parity smoke, orchestration spec pin이다. 0.42.87 package와 pair, fullgate evidence도 같은 PR에 들어 있다.

- clean package는 `8d940da`에서 만들었다(clean MSI `a0041c9f…`).
- fullgate gate build는 `8ade930`다(operational MSI `f339ab45…`). product payload는 `8d940da`와 같다.
- 두 commit은 모두 `d48de55`에서 도달한다.

2026-10-03 Lane 3가 canonical current를 `0.42.87-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
