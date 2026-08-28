# SERVICE_PLAN P1 clone actual-VM 2026-08-29 `0.42.76` r2

evidence_id: `service-plan-p1-clone-actual-vm-2026-08-29-04276-r2`
result: `FAIL`
evidence_scope: `installed-actual-vm-service-plan-p1-clone-candidate`
version: `0.42.76-admin-smoke`
source_commit: `25757397c042e4d1be6cab1555a2b0f617f6de8a`
working_tree_note: `uncommitted --disk-gb 8 runner used; previous FAIL artifact 20260829-04276 not reused`
artifact_root: `artifacts/service-plan-p1-clone-actual-vm-20260829-04276-r2`
artifact_summary: `artifacts/service-plan-p1-clone-actual-vm-20260829-04276-r2/summary.json`
summary_sha256: `ac178073168d1467ed27d8febd4ef6e8095a6324fcd1f9aacbd8deb6da8da8d1`
runner_sha256: `965f73891e1a76b865d351115ec4e80b45eab80f3fae60b3e8783b3ee5df736e`
installed_cli_sha256: `7fc2a92fcc3becceea90c0996afcbcdef863c6485542bd147760d738b3bbe77f`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.76-admin-smoke`에서 clone family 프로브를 새 artifact root로 다시 실행했다.
`--disk-gb 8` create는 성공했다. `overall_verdict=FAIL`이며 이 summary는
`actual_vm_tested=pass` 입력이 될 수 없다.

| 항목 | 값 |
| --- | --- |
| `installed_manifest_version` | `0.42.76-admin-smoke` |
| create job | `job-63b86463e0ec4372872f78d740fe35d9` `succeeded` |
| source | `pcv-p1-clone-04276-46a4e8f3-src` / `ca6ec54f-bcf6-4b76-a9a2-8096c363ad08` |
| Hyper-V after create | `Off` |
| product after create | `stopped` |
| `error` | `PCV_P1_CLONE_STATE_MISMATCH` |
| leftover `pcv-p1-clone-*` VM | `0` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | create succeeded, Hyper-V `Off`, 제품 `stopped`. runner는 제품 `off`만 PASS | `FAIL` |
| `preview_mismatch` | 미실행 | `NOT_RUN` |
| `preview_ok` | 미실행 | `NOT_RUN` |
| `clone_ok` | 미실행 | `NOT_RUN` |
| `cleanup` | source product delete succeeded, native fallback 없음 | `PASS` |

선행 `20260829-04276` FAIL는 `--disk-gb 1` / `PCV_DISK_OUT_OF_RANGE`다. r2는 그 원인을
닫았고 새 원인이다. 같은 artifact root와 같은 VM 이름으로 D를 재실행하지 않는다.

## 원인

`Get-ProductVmState`는 `vm get`의 `state`를 소문자로 비교한다. 설치본은 Off VM을
`stopped`로 돌려준다. runner는 `$productOff -ne 'off'`이면
`PCV_P1_CLONE_STATE_MISMATCH`다. 동작 테스트 adapter는 `state = 'off'`라 이 불일치를
막지 못했다.

다음 Lane 1은 제품 Off 동치를 `off`/`stopped`로 맞추는 계약만이다. clone family를
다시 열지 않는다.

## Nonclaims

- operational current는 `0.42.75-admin-smoke`로 유지한다.
- feature ledger를 pass로 바꾸지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
