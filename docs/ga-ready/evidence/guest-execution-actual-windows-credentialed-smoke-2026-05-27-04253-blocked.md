# Guest Execution Actual Windows Credentialed Smoke 2026-05-27 0.42.53 Blocked

evidence_id: `guest-execution-actual-windows-credentialed-smoke-2026-05-27-04253-blocked`
result: `BLOCKED_BY_MISSING_WINDOWS_GUEST_AND_CREDENTIAL`
scope: `actual-windows-guest-credentialed-exec-smoke-preflight`
version: `0.42.53-admin-smoke`
installed_cli: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
product_version: `0.42.53-admin-smoke+cc774b257d6cd772c3a890266aca62aa8ab8eadc`
host_status: `pass-hyperv-vmms-admin-default-switch`
vm_inventory: `empty`
credential_inventory: `no-purecvisor-guest-credential-target`
host_mutation_performed: `queued-guest-exec-and-channel-verify-failed-before-guest-mutation`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행한 확인

```powershell
pcvcli --json runtime policy
pcvcli --json vm list
cmdkey /list | Select-String -Pattern "PureCVisor|pcv|guest"
pcvcli --json vm guest-exec pcv-guest-smoke-missing --credential-ref wincred:PureCVisor/guest/admin --timeout-sec 5 -- powershell -NoProfile -Command hostname
pcvcli --json vm guest-agent-ensure-channel pcv-guest-smoke-missing --verify --credential-ref wincred:PureCVisor/guest/admin --timeout-sec 5
pcvcli --json vm guest-exec pcv-guest-smoke-missing --dry-run --credential-ref wincred:PureCVisor/guest/admin -- powershell -NoProfile -Command hostname
```

## 결과

- `runtime policy`: `guest_execution.enabled=true`, `execute_enabled=true`, channel verify/repair enabled.
- `vm list`: `[]`.
- Credential Manager scan: `PureCVisor/guest/admin` 계열 target 없음.
- `guest-exec` queued job: `job-072a7d004e7f407d8dccd9a8845d8152`.
- `guest-agent-ensure-channel --verify` queued job: `job-cbec0b48d48c4a6dbb2add167ec40e91`.
- 두 job은 provider 실행 전에 `PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED`로 failed.
- Dry-run은 `guest-execution-preview.v1`, `execute_enabled=true`, `execution_queued=false`,
  `guest-execution-audit-v1`, `guest-execution-redaction-v1`를 반환했다.
- 출력에는 credential target 원문이 없고 `credential_ref`는 `[redacted-ref]`로 남았다.

## 판정

이 evidence는 실제 Windows guest 내부 명령 실행 PASS가 아니다. 현재 host에는 실행 대상 Windows
VM과 protected guest credential reference가 없으므로 actual credentialed smoke를 진행할 수 없다.
다만 installed CLI/API provider route, audit/redaction, credential-ref guard, secret non-echo는
설치본에서 재확인했다.

## 다음 실행 조건

1. Windows guest VM이 Hyper-V inventory에 존재해야 한다.
2. VM은 PowerShell Direct가 가능한 상태여야 한다.
3. Windows Credential Manager 또는 DPAPI reference에 guest credential을 등록해야 한다.
4. 동일 명령으로 `hostname` 또는 `whoami` actual execution을 재시도한다.
