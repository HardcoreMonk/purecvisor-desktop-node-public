# SERVICE_PLAN P1-8 guest file actual-VM 2026-09-28 `0.42.82`

evidence_id: `service-plan-p1-guest-file-actual-vm-2026-09-28-04282`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p1-guest-file-candidate`
version: `0.42.82-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04282`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1GuestFileActualVmSmoke.ps1`
runner_commit: `16cd1e8`
artifact_root: `artifacts/service-plan-p1-guest-file-actual-vm-20260928-04282`
artifact_summary: `artifacts/service-plan-p1-guest-file-actual-vm-20260928-04282/summary.json`
summary_sha256: `e7d12749ab56f25932b3d9f531b3fed70f9862f295c5952f1e71302ee1d3ea06`
runner_sha256: `9f69947f21c8f65ca11c16d526d4f69f788d46fa927053cbb8cde58237f0d1af` (실행 시 working tree)
installed_cli_sha256: `71728be6269bb0448e299e6db44b1dd8f86bed182b17fc18a7200c63f285bad2`
vm: `pcv-guest-installed-04253-r1` / `030ccf40-0e00-4227-87ab-7bb4b83c3066` (보존 Windows Server 2022 guest)
credential_ref_type: `dpapi-local-machine`
payload_sha256: `43F85694FFA05F16698D1ED8054B56F4807FD77165AF4D9085D91896A0AEEDC9` (run 전용 `1 MiB` 난수)
host_mutation_performed: `true` (VM 시작·정지, guest 파일, host staging. VM 생성·삭제 없음)
guest_command_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.82-admin-smoke`에서 P1-8 guest file 한 프로브를 보존 guest에 실행했다. `overall_verdict=PASS`,
`cleanup.verdict=PASS`, `secret_observed=false`. VM은 시작 전과 같은 `Off`로 돌아갔고 host staging은 지워졌다.
선행 FAIL(`service-plan-p1-guest-file-actual-vm-2026-09-28-04281`, guest 부모 디렉터리 부재)이 닫혔다.

| slice | 관측 | verdict |
| --- | --- | --- |
| `preflight` | 설치본 version, VM 1개·id, keep policy, credential 파일, host root, 시작 전 `Off` | `PASS` |
| `vm_start` | `job-b75406db44e04bbfbd974cd2a26734c9` succeeded, `Running`, heartbeat OK | `PASS` |
| `channel_verify` | 첫 시도 succeeded, `windows-powershell-direct` | `PASS` |
| `guest_prefix_probe` | `C:\Users\Public\PureCVisor` 없음(`false`) | `PASS` |
| `host_staging` | payload `1048576` byte, oversize `67108865` byte | `PASS` |
| `preview_path_not_allowed` | `PCV_GUEST_FILE_PATH_NOT_ALLOWED`, job 없음 | `PASS` |
| `preview_size_limit` | `PCV_GUEST_FILE_SIZE_LIMIT`, job 없음 | `PASS` |
| `preview_ok` | `size_bytes=1048576`, `copied=false`, guest 파일 없음 | `PASS` |
| `copy_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, job 없음 | `PASS` |
| `copy` | `job-814bd7de44814a0993b05157117cae5e` succeeded, `copied=true`, `size_bytes=1048576` | `PASS` |
| `guest_readback` | guest-exec `Get-FileHash` stdout digest가 payload SHA-256 줄(+CRLF)과 같음, `66` byte | `PASS` |
| `cleanup` | guest run 파일 삭제·부재 확인, host staging 삭제, `vm shutdown`으로 `Off` | `PASS` |

## guest 상태 변화

제품이 copy 전에 allowlist 접두사 `C:\Users\Public\PureCVisor\`를 만들었다. runner는 run 파일만 지우고 제품 소유
접두사 디렉터리는 남긴다(설계 §4). 보존 VM의 Notes, credential, 디스크 구성은 바꾸지 않았다.

## Nonclaims

- guest-to-host pull, 디렉터리 복사, Linux guest는 실행하지 않았다.
- operational current는 `0.42.78-admin-smoke` 그대로다. Lane 3, feature ledger pass를 하지 않았다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
