# Public Boundary CI main push 2026-10-02 `0.42.86` PR #25 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-02-04286-pr25-postmerge-pass`
result: `PASS`
scope: `post-pr25-merge-main-push`
head_sha: `b807803f778e29c206f1bb2ba8277d2a1136198f`
public_boundary_run_id: `36970815175`
public_boundary_job_id: `110724204534`
development_gates_run_id: `36970815183`
product_payload_change_detected: `false`
package_candidate_decision: `pr25-postmerge-includes-04286-candidate-sources`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #25 `lane2/04285-package-pair-20261001`를 merge한 뒤 `origin/main` `b807803`에서 두 workflow가 success다.

- Public Boundary run `36970815175` job `110724204534`(`public-boundary-ci-required`)
- Development Gates run `36970815183`. shard `dotnet` job `110724204964`, `web` job `110724204929`, `delivery` job `110724204907`, `installer-policy` job `110724204961`

이 push의 첫 부모 `1c488b6`과 `b807803` 사이에서 `src`, `web`, installer, product wrapper와 module, `config`, `.github` 경로는 바뀌지 않았다. PR #25가 더한 것은 0.42.85 package pair 문서와 campaign 기록이다.

`0.42.86` 제품 변경은 그 앞에 merge된 PR #26 `1c488b6`에 있다. media fix `fb95de1`은 `1c488b6`와 `b807803` 둘 다에서 도달한다.

- clean package는 `1c488b6`에서 만들었다(clean MSI `8edb19ce…`).
- fullgate gate build는 `b807803`다(operational MSI `85387f31…`).
- `0.42.85-admin-smoke`에는 `fb95de1`이 없다.

2026-10-02 Lane 3가 canonical current를 `0.42.86-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
