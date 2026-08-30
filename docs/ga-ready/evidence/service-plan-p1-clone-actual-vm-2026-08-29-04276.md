# SERVICE_PLAN P1 clone actual-VM 2026-08-29 `0.42.76`

evidence_id: `service-plan-p1-clone-actual-vm-2026-08-29-04276`
result: `FAIL`
evidence_scope: `installed-actual-vm-service-plan-p1-clone-candidate`
version: `0.42.76-admin-smoke`
source_commit: `6b1479e1d6b58c1dec560cd05de51321ac3e9539`
artifact_root: `artifacts/service-plan-p1-clone-actual-vm-20260829-04276`
artifact_summary: `artifacts/service-plan-p1-clone-actual-vm-20260829-04276/summary.json`
summary_sha256: `96a432d529ca8087a163bf351c26cbfa9c6d46d01c18a9a735fb89b6170c3040`
runner_sha256: `b4456b54bab8d1d2ee4d4944e5015dd915849f39d63d656f926d21586d8107fc`
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

설치본 `0.42.76-admin-smoke`에서 Lane 2 clone family 한 프로브를 실행했다.
`overall_verdict=FAIL`이다. 이 summary는 `actual_vm_tested=pass` 입력이 될 수 없다.

| 항목 | 값 |
| --- | --- |
| `installed_manifest_version` | `0.42.76-admin-smoke` |
| `-Version` | `0.42.76-admin-smoke` |
| `error` | `PCV_P1_CLONE_JOB_FAILED` |
| create job | `job-da2001116f3347e8967af3e95e8e0efb` `failed` / `PCV_DISK_OUT_OF_RANGE` |
| leftover `pcv-p1-clone-*` VM | `0` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | `pcvcli vm create --disk-gb 1` job `failed`, `PCV_DISK_OUT_OF_RANGE` | `FAIL` |
| `preview_mismatch` | 미실행 | `NOT_RUN` |
| `preview_ok` | 미실행 | `NOT_RUN` |
| `clone_ok` | 미실행 | `NOT_RUN` |
| `cleanup` | 제품 VM 없음, native fallback 없음 | `PASS` |

소스 이름은 `pcv-p1-clone-04276-a4dd8adc-src`다. create가 실패해 Hyper-V id는 없다.
cleanup 직후 빈 소스 디렉터리가 남아 있어 수동으로 제거했고, `D:\data\pcv-p1-clone-04276`도 비워서 삭제했다.

## 원인

제품 create 디스크 범위는 `8..4096` GB다. P0 runner는 `--disk-gb 8`을 쓴다. P1 clone
runner `Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1`는 `--disk-gb 1`을 써서
`source_create`가 enqueue 직후 실패한다. clone slice는 실행되지 않았다.

다음 Lane 1은 runner/테스트를 `--disk-gb 8`로 맞추는 계약만이다. 같은 artifact root와
같은 VM 이름으로 D를 재실행하지 않는다.

## Nonclaims

- operational current는 `0.42.75-admin-smoke`로 유지한다.
- feature ledger를 pass로 바꾸지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- package-pair, fullgate, current-card는 이 문서가 소유하지 않는다.
