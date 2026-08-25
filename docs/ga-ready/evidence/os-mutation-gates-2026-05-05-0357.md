# OS Mutation Gate Evidence - 2026-05-05 0.35.7

evidence_id: os-mutation-gates-2026-05-05-0357
created_at: 2026-05-05T18:04:34+09:00
source_commit_sha: 2ec9e71d45b702e106824c86500cd6152b18fab7
artifact_root: artifacts/os-mutation-gates-20260505-180434-0357-rerun
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357
version: 0.35.7-admin-smoke
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 opt-in 범위에서 실제 Hyper-V, MSI, firewall, LAN, Event Log, internal trust-store mutation gate를 실행했다. 이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Hyper-V, MSI, Service, Data Root

- Artifact: `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`
- MSI: `PureCVisorDesktopNode-0.35.7-admin-smoke-windows-x64.msi`
- MSI SHA-256: `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`
- MSI signing mode: `AllowUnsignedDev`
- MSI lifecycle, service-action smoke, installed Hyper-V API route smoke: pass
- Final proof: service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음

## Firewall, LAN, Event Log, Trust Store

- Artifact: `artifacts/os-mutation-gates-20260505-180434-0357-rerun`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source `HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\PureCVisor Desktop Node` absent
- Firewall: owned rule `PureCVisor Desktop Node Local API LAN` enable pass 후 remove pass, final rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/`에서 bearer auth로 `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js` 모두 HTTP `200`
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`

## Config/Job Store Boundary

- `DesktopNode.Host.exe service-action config-migration-apply`는 installed service running precondition에서 `PCV_CONFIG_MIGRATION_SERVICE_RUNNING` blocked descriptor를 반환했다.
- Descriptor는 `MutationPlanned=false`, `MutationPerformed=false`를 기록했다.
- Product config write, backup write, service stop/start, job store migration rewrite는 실행하지 않았다.
- Job store migration apply는 계속 `future-route/not-implemented/blocked`다.

## Failed Attempt Disposition

- Superseded artifact: `artifacts/os-mutation-gates-20260505-175453-0357`
- Root cause: LAN static asset probe가 bearer Authorization header 없이 `/`, `/index.html`, `/app.js`를 요청했다.
- Product policy: LAN non-loopback static auth는 bearer-required다.
- Cleanup proof: superseded artifact와 rerun 모두 final firewall rule count `0`, Event Log source absent, internal trust cert present를 기록했다.

## Final State Proof

- Installed DisplayVersion: `0.35.7`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Final product manifest version: `0.35.7-admin-smoke`
- Final firewall rule count: `0`
- Final Event Log source: absent
- Final internal trust Root/TrustedPublisher: present
- Final job store: `C:\ProgramData\PureCVisor\desktop-node\jobs.json`, length `7117`
- Boot time unchanged: pass
- `pcv-spike-*` VM 잔여물: none

## 판정

`0.35.7-admin-smoke` OS mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며, public trusted signing 또는 외부 stable publication evidence가 아니다. Config/job store migration apply는 destructive apply 구현이 아니라 blocked/no-mutation descriptor 경계로만 확인했다.
