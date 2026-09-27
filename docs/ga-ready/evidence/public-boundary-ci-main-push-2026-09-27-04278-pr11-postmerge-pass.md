# Public Boundary CI main push 2026-09-27 `0.42.78` PR #11 postmerge

evidence_id: `public-boundary-ci-main-push-2026-09-27-04278-pr11-postmerge-pass`
result: `PASS`
scope: `post-pr11-merge-main-push`
head_sha: `3c677a476517ab239cee3514b330d6440c7d125a`
public_boundary_run_id: `36299263811`
public_boundary_job_id: `108563752427`
development_gates_run_id: `36299263803`
product_payload_change_detected: `true`
package_candidate_decision: `pr11-merge-retains-0.42.78-admin-smoke-candidate`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #11 `feat/p1-9-account-crud` merge 후 `origin/main` `3c677a4`에서 Public Boundary run
`36299263811` job `108563752427`(`public-boundary-ci-required`)과 Development Gates run
`36299263803`의 네 shard(`dotnet`, `web`, `delivery`, `installer-policy`)가 success다.

이 merge는 P1-8~P2-15 기능, 0.42.78 evidence, 모듈 크기 라쳇 복구, 기존 실패 테스트 복구를
`main`에 넣었다. 0.42.78 package candidate는 `e098e0a`(clean MSI `c3390c1e…`)와 r2 gate build
`0de176f`에서 만들어졌다. 그 뒤 `main`에 들어온 제품 source 변경은 라쳇 복구의 순수 이동
(`5ec4f23`~`cbb1103`, `38f3eec`)뿐이고 동작 변경이 없다. 나머지는 테스트, 검증 도구
(`CutoverGitBoundary`), 문서다. 2026-09-27 Lane 3가 0.42.78을 operational current로
승격하며, 순수 이동을 담은 새 package candidate는 다음 package pair에서 연다.
