# Public Boundary CI 재실행 PASS Evidence

evidence_id: public-boundary-ci-rerun-2026-05-16-04220-pass
result: PASS
actual_execution: github-actions-workflow-dispatch
workflow: public-boundary.yml
run_id: 25933428239
run_url: [private-archive-repository]/actions/runs/25933428239
job_name: public-boundary-ci-required
job_id: 76232707240
head_branch: main
head_sha: 6e556e5199e796a8889a9dc47dc925db02c9cb45
source_version_anchor: 0.42.20-admin-smoke
source_full_admin_gate: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md
source_full_admin_gate_batch: full-admin-host-mutation-gate-20260516-04220
source_full_admin_gate_msi_sha256: 12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c
source_full_admin_gate_provenance_commit: 0895d018935298721b25b5d9ce1ae083a6690c25
source_post_04220_dev_slices: docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md
source_post_04220_result: CODE_LEVEL_PASS
public_boundary_guard_executed: true
billing_status: resolved-for-actions-run
previous_blocker_runs: 25930077313, 25931297085, 25933236528
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

이 evidence는 GitHub 결제 수단 등록 이후 `main` 기준으로
`.github/workflows/public-boundary.yml`을 `workflow_dispatch`로 재실행해
`public-boundary-ci-required`가 실제 runner에서 PASS했음을 기록한다. 이전
`25930077313`, `25931297085`, `25933236528` run은 GitHub billing/spending-limit
blocker로 job이 시작되지 못한 historical blocker이며, 최신 판단은 run
`25933428239`의 PASS가 소유한다.

## 실행 결과

| 항목 | 값 |
| --- | --- |
| run id | `25933428239` |
| run URL | `[private-archive-repository]/actions/runs/25933428239` |
| ref | `main` |
| head SHA | `6e556e5199e796a8889a9dc47dc925db02c9cb45` |
| job | `public-boundary-ci-required` |
| job id | `76232707240` |
| runner | `GitHub Actions 1000003892` |
| conclusion | `success` |
| public boundary guard executed | `true` |

PASS step:

- `actions/checkout`
- `Install Pester`
- `Run public boundary evidence guard`
- `Verify packaging regression required step`

GitHub Actions annotation은 `actions/checkout@v4`의 Node.js 20 deprecation warning만
남겼다. 이 warning은 2026-06-02 이후 기본 Node 24 전환 예고이며, 이번 public
boundary guard PASS를 무효화하지 않는다.

## 연결된 운영 Anchor

- Full admin host mutation evidence:
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`
- Full admin host mutation batch:
  `full-admin-host-mutation-gate-20260516-04220`
- Version anchor: `0.42.20-admin-smoke`
- Full-gate MSI SHA-256:
  `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`
- Full-gate provenance commit:
  `0895d018935298721b25b5d9ce1ae083a6690c25`
- Post-04220 development slices:
  `docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md`

## 경계

이 evidence는 GitHub-hosted public boundary CI guard 실행 결과만 기록한다. Host
mutation, clean-host VM, MSI install/update/rollback, firewall, Event Log, trust
store, Credential Manager, public trusted signing, trusted timestamp, external stable
publication, winget submission, public stable installer URL은 실행하거나 주장하지
않는다.
