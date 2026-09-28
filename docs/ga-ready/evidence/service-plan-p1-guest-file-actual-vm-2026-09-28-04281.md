# SERVICE_PLAN P1-8 guest file actual-VM 2026-09-28 `0.42.81`

evidence_id: `service-plan-p1-guest-file-actual-vm-2026-09-28-04281`
result: `FAIL`
failure_class: `product-defect-guest-parent-directory-not-created`
evidence_scope: `installed-actual-vm-service-plan-p1-guest-file-candidate`
version: `0.42.81-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04281`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1GuestFileActualVmSmoke.ps1`
runner_commit: `ad5c328`
artifact_root: `artifacts/service-plan-p1-guest-file-actual-vm-20260928-04281`
artifact_summary: `artifacts/service-plan-p1-guest-file-actual-vm-20260928-04281/summary.json`
vm: `pcv-guest-installed-04253-r1` / `030ccf40-0e00-4227-87ab-7bb4b83c3066` (보존 Windows Server 2022 guest)
credential_ref_type: `dpapi-local-machine`
host_mutation_performed: `true` (VM 시작·정지, guest 파일, host staging. VM 생성·삭제 없음)
guest_command_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

승인: `User-Approval: 2026-09-28 guest file run 승인, host mutation 승인`. `overall_verdict=FAIL`,
`cleanup.verdict=PASS`, `secret_observed=false`. VM은 시작 전과 같은 `Off`로 돌아갔고 host staging은 지워졌다.

| slice | 관측 | verdict |
| --- | --- | --- |
| `preflight` | 설치본 version, VM 1개, keep policy Notes, credential 파일, host root, 시작 전 `Off` | `PASS` |
| `vm_start` | `job-72054debc4d4458ca87f89b9f459ae11` succeeded, `Running`, heartbeat OK | `PASS` |
| `channel_verify` | 첫 시도 succeeded, transport `windows-powershell-direct` | `PASS` |
| `guest_prefix_probe` | `C:\Users\Public\PureCVisor` 없음(`false`), runner는 만들지 않음 | `PASS` |
| `host_staging` | payload `1048576` byte, oversize `67108865` byte | `PASS` |
| `preview_path_not_allowed` | exit `1`, `PCV_GUEST_FILE_PATH_NOT_ALLOWED`, job 없음 | `PASS` |
| `preview_size_limit` | exit `1`, `PCV_GUEST_FILE_SIZE_LIMIT`, job 없음 | `PASS` |
| `preview_ok` | exit `0`, `size_bytes=1048576`, `copied=false`, guest 파일 없음 | `PASS` |
| `copy_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, job 없음 | `PASS` |
| `copy` | `job-5ba21ac57f594175bb0ce977f9d00d02` failed `PCV_GUEST_FILE_COPY_FAILED` | `FAIL` |
| `guest_readback` | 실행 안 함 | `NOT_RUN` |
| `cleanup` | guest 파일 부재 확인, host staging 삭제, `vm shutdown`으로 `Off` 복귀 | `PASS` |

## 원인

job error detail(PowerShell CLIXML)의 오류는 `Copy-Item : '...\pcv-p1-guestfile-04281-2f0341cc.bin' 경로는 존재하지
않으므로 찾을 수 없습니다`, `FullyQualifiedErrorId : RemotePathNotFound`다. 제품 copier(`DesktopNodeHyperVPowerShellDirectFileCopier`)는
`Copy-Item -ToSession -Destination <guest_path>`만 하고 allowlist 접두사 `C:\Users\Public\PureCVisor\` 같은 부모 디렉터리를
만들지 않는다. 새 guest에서는 첫 copy가 항상 실패한다. 설계의 예상 위험(§5)이 실제로 확인됐다.

## report-only

- 실패 job의 `error.detail`에 guest PowerShell CLIXML(진행 레코드와 현지화 메시지)이 그대로 들어간다. 비밀은 없지만 읽기 어렵다.

## Nonclaims

- guest file copy PASS를 주장하지 않는다. P1-8 Lane 2는 FAIL이다.
- VM 생성·삭제, Notes·credential 변경, MSI 설치를 하지 않았다.
- operational current는 `0.42.78-admin-smoke` 그대로다. public trusted signing, external stable publication을 주장하지 않는다.
