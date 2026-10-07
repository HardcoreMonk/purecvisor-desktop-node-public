# Public Boundary CI main push 2026-10-08 `0.42.92` PR #62 postmerge

evidence_id: `public-boundary-ci-main-push-2026-10-08-04292-pr62-postmerge-pass`
result: `PASS`
scope: `post-pr62-merge-main-push`
head_sha: `34b2b1697f3ac42bf11a85fd1ff1ab8d4673547c`
public_boundary_run_id: `37643151392`
public_boundary_job_id: `112866922673`
development_gates_run_id: `37643151377`
product_payload_change_detected: `false`
package_candidate_decision: `pr62-postmerge-includes-04292-candidate-sources`
release_train: `0.42.92-admin-smoke`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

release train `0.42.92`의 PR #62 `train/04292-20261007`를 merge한 뒤 `origin/main` `34b2b16`에서 두 workflow가 success다. 단일 PR train path check가 Lane 3 pin 변경과 충돌해(backlog `BL-0005`) 이 train은 두 PR로 돌았다.

- Public Boundary run `37643151392` job `112866922673`(`public-boundary-ci-required`)
- Development Gates run `37643151377`. shard `dotnet` job `112866923201`, `web` job `112866923489`, `delivery` job `112866923522`, `installer-policy` job `112866924129`

이 push의 첫 부모 `ad8b5c2`(PR #61 merge)와 `34b2b16` 사이에서 `src`, `web/src`, `config`, `.github` 경로는 바뀌지 않았다. PR #62가 더한 것은 train 출발 기록, 0.42.92 package, pair, fullgate, current-card, `vm.create` probe 문서다.

`0.42.92` 제품 변경(끊긴 create 잔여물 회수 `ee90474`)은 train에 실은 PR #59 `3f55831`에 있다.

- clean package는 `e250950`에서 만들었다(clean MSI `dc79fdd1…`).
- fullgate gate build는 `b51b8cf`다(operational MSI `67b257a3…`). product payload는 `ad8b5c2`와 같다.
- 두 commit은 모두 `34b2b16`에서 도달한다.

2026-10-08 Lane 3가 canonical current를 `0.42.92-admin-smoke`로 썼다. public trusted signing과 external stable publication을 주장하지 않는다.
