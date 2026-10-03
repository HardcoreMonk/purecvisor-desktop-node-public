# Lane 2 reconcile 안내 문구 actual-VM 확인 `0.42.87-admin-smoke` (2026-10-03)

evidence_id: `lane2-reconcile-wording-actual-vm-2026-10-03-04287`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.87-admin-smoke`
installed_product_version: `0.42.87-admin-smoke+8ade930587941e24451f31a352fe2a412841f001`
source_fix: `5b5738e`
artifact_root: `artifacts/lane2-reconcile-wording-actual-vm-20261003-04287`
probe_script_sha256: `de53077b1d602d324f12479871de3185df4cd6ba899b17f01ac5bec19d50769c`
probe_summary_sha256: `a91fe5552c752b91662a68a44d8df3927e86187b52c5a82c78eb6c000e57eed4`
wording_check_sha256: `906166523b1ddcddef670ed38ea5141c5d5db84943f1cab38a251351669e8293`
probe_vms: `pcv-lane2-reconcile-1003`(rename 뒤 `pcv-lane2-reconcile-1003-renamed`)
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

`docs/ga-ready/evidence/lane2-development-completion-actual-vm-2026-09-30-04284.md`의 report-only 발견을 고친 `5b5738e`을 fullgate 설치본 `0.42.87`에서 확인했다. 그때 끝난 `vm.export` job을 reconcile하면 안내가 "confirm whether the rename applied"였다. 실행 스크립트는 artifact root의 `probe.ps1`이고 2026-10-03 `07:34:12Z`부터 `07:34:25Z`까지 돌았다. ISO는 fullgate와 같은 최소 ISO 9660(`artifacts/smoke-media-20261003/`)이다.

## 결과

| 단계 | 결과 |
| --- | --- |
| `vm create` (Gen2, 1 vCPU, 1024 MB, 8 GB) | job `succeeded` |
| `vm rename` | job `succeeded` |
| 끝난 `vm.rename` job의 `job reconcile` | `PCV_JOB_RECONCILIATION_REQUIRED`, `job-not-reconcilable`, Next action "... confirm whether the rename applied ..." |
| `vm export --yes` | job `succeeded` |
| 끝난 `vm.export` job의 `job reconcile`(비대상) | `PCV_JOB_RECONCILIATION_REQUIRED`, detail "vm.export is not a reconcile target: Export writes host files that vm.list does not report.", Next action "... confirm whether the mutation applied ..." |

비대상 안내는 `rename`을 말하지 않는다. rename job의 안내는 계속 rename을 말한다. 판정은 `wording-check.json`이다(`result=PASS`).

`probe.ps1`이 자동으로 쓴 `summary.json`은 `result=FAIL`이다. `pcvcli --json job reconcile`이 오류를 JSON이 아니라 `code=…`, `detail=…`, `Next action: …` 텍스트로 출력해서 probe의 JSON 파싱이 안내 문자열을 찾지 못했기 때문이다. 원본 응답(`reconcile-rename.json`, `reconcile-non-target-export.json`)에서 다시 판정한 결과가 `wording-check.json`이다. 제품 동작의 실패가 아니다.

## 정리

- probe VM을 PCVCLI `vm delete --yes`로 지웠다(job `succeeded`). export 디렉터리를 지웠다.
- managed delete가 남긴 VM 디렉터리 `D:\PureCVisor\VMs\pcv-lane2-reconcile-1003`은 어떤 VM도 참조하지 않는 것을 확인한 뒤 지웠다.
- 최종 상태: VM은 보존 VM `pcv-guest-installed-04253-r1` 하나(Off), service Running/Auto, Web `200`, secret 관측 없음.

## report-only 발견

- `pcvcli --json`인데도 오류 응답은 key=value 텍스트로 나온다. 자동화가 JSON으로 읽으려면 이 형식을 알아야 한다.

## Nonclaims

- 상태는 `installed_non_promoted_candidate`다. operational current는 `0.42.86-admin-smoke`다.
- reconcile 완전성이나 Hyper-V exactly-once를 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
