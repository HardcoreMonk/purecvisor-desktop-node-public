# OS Mutation Gate Evidence - 2026-05-05 0.35.6

evidence_id: os-mutation-gates-2026-05-05-0356
created_at: 2026-05-05T16:12:00+09:00
updated_at: 2026-05-05T17:12:00+09:00
source_commit_sha: cc723e28ed62f6f1c5e49c74ca68b87d0f1b8b3a
artifact_root: artifacts/os-mutation-gates-20260505-170454-0356-rerun
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun
version: 0.35.6-admin-smoke
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 opt-in 범위에서 실제 Hyper-V, MSI, firewall, LAN, Event Log, internal trust-store mutation gate를 실행했다. 이 문서는 같은 `0.35.6-admin-smoke` gate의 최신 rerun artifact를 가리킨다.

이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Hyper-V, MSI, Service, Data Root

- Artifact: `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun`
- MSI: `PureCVisorDesktopNode-0.35.6-admin-smoke-windows-x64.msi`
- MSI SHA-256: `a24de44049519dea8405854a17272ebb362b061ff03a051cd61fb31669bc7d02`
- MSI signing mode: `AllowUnsignedDev`
- MSI lifecycle: install, repair, uninstall preserve, reinstall, `REMOVE_DATA=1` uninstall, final restore install 모두 pass
- Service/data-root lifecycle: `configure-installed`, service 존재 중 `data-root-remove --remove-data` blocked, `remove-installed --remove-data` handoff-only, service absent 이후 allowlist data-root removal pass
- Hyper-V installed route smoke: VM create/start/restart, guest shutdown unavailable structured failure, checkpoint create/restore/delete, VM delete, repeat delete absent, unmanaged delete guard pass
- Final proof: service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음

## Firewall, LAN, Event Log, Trust Store

- Artifact: `artifacts/os-mutation-gates-20260505-170454-0356-rerun`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source `HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\PureCVisor Desktop Node` absent
- Firewall: owned rule `PureCVisor Desktop Node Local API LAN` enable pass 후 remove pass, final rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/`에서 `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js` 모두 HTTP `200`
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`

## Config/Job Store Boundary

- Installed runtime Hyper-V route smoke는 실제 `C:\ProgramData\PureCVisor\desktop-node\jobs.json` job store write를 수행했다.
- Service/data-root smoke는 synthetic data root에서 `jobs.json`, token files, `events.jsonl`, `install.jsonl`, diagnostics allowlist removal을 수행했고 non-allowlist log는 보존했다.
- `product config migration apply`와 `job store migration apply`는 route matrix 기준 `future-route`, `not-implemented`, `blocked`다. 현재 제품에는 config/job store migration apply 실행 route가 없으므로 이 destructive migration apply는 실행하지 않았다.

## Final State Proof

- Installed DisplayVersion: `0.35.6`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Final product manifest version: `0.35.6-admin-smoke`
- Final loopback runtime policy: `ok=true`, `operation=runtime.policy`, generated `request_id`, `token_storage=dpapi-local-machine`, `current_exposure=loopback`
- Final firewall rule count: `0`
- Final Event Log source: absent
- Final internal trust Root/TrustedPublisher: present
- Final job store: `C:\ProgramData\PureCVisor\desktop-node\jobs.json`, length `7117`
- Boot time unchanged: pass
- `pcv-spike-*` VM 잔여물: none

## 판정

`0.35.6-admin-smoke` OS mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며, public trusted signing 또는 외부 stable publication evidence가 아니다. Config/job store migration apply는 현재 구현된 product operation이 아니므로 blocked future-route로 유지한다.
