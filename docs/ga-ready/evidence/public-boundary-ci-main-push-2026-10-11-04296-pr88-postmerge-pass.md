# Public Boundary CI main push 2026-10-11 `0.42.96` PR #88 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-11-04296-pr88-postmerge-pass`
result: `PASS`
scope: `post-pr88-merge-main-push`
head_sha: `d5b8618d26730a96256a3bae1fd20505b7c5576a`
public_boundary_run_id: `38078392505`
public_boundary_job_id: `114290057289`
development_gates_run_id: `38078392575`
product_payload_change_detected: `false`
package_candidate_decision: `pr88-postmerge-includes-04296-candidate-sources`
release_train: `0.42.96-admin-smoke`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.96`의 PR #88 `lane1/train-04296-20261011`를 merge한 뒤 `origin/main` `d5b8618`에서 두 workflow가 success다. train `0.42.94`·`0.42.95` 정차로 lane3-spec이 직전 승격 version 대신 pair baseline을 써(backlog `BL-0021`) train branch에서 Lane 3를 쓰지 못해, 이 train은 두 PR로 돌았다(수정 PR #87).

- Public Boundary run `38078392505` job `114290057289`(`public-boundary-ci-required`)
- Development Gates run `38078392575`. shard `dotnet` job `114290058135`, `web` job `114290058005`, `delivery` job `114290058168`, `installer-policy` job `114290057859`

이 push의 첫 부모 `952fbd4`(PR #87 merge)와 `d5b8618` 사이에서 `web/src`, `.github` 경로는 바뀌지 않았다. `src`·`config` 변경은 `train-path-check` `allowed_pin_paths` 4개(`config/pcv-development-policy-contract-spec-v1.json`, `config/pcv-installed-smoke-contract-spec-v1.json`, `src/DesktopNode.Delivery.Tests/Delivery/Installed/InstalledContractVerifier.cs`, `src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs`)의 SHA-256 한 줄씩뿐이다(문서 현행화의 `AGENTS.md`·packaging README pin). PR #88가 더한 것은 train 출발 기록, 0.42.96 package, pair, fullgate, current-card, Lane 2 probe와 S3 재시연, 문서 현행화다.

`0.42.96` 제품 변경(PR #78 `1b04ff46`, #79 `04ffe117`, #81 `4e109e70`, #84 `3d50f14b`, #86 `179bd198`)은 payload commit `87a7deb`에서 도달한다. 그 뒤 `main`의 `src` 변경은 위 pin과 PR #87(`src/DesktopNode.Verification` lane3-spec 수정과 그 시험)뿐이고, 둘 다 MSI payload(`DesktopNode.Host`와 그 참조 Api·HyperV·Service, web, wrapper)에 들지 않는다.

- clean package는 `e07113c`에서 만들었다(clean MSI `326b867a…`).
- fullgate gate build는 `9ac8eb3`다(operational MSI `37bfc1ff…`). product payload는 `87a7deb`와 같다.
- 세 commit(`e07113c`, `9ac8eb3`, `87a7deb`)은 모두 `d5b8618`에서 도달한다.

2026-10-11 Lane 3가 canonical current를 `0.42.96-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
