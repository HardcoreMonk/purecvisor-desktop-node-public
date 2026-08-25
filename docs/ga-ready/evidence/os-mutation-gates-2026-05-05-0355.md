# OS Mutation Gate Evidence - 2026-05-05 0.35.5

evidence_id: os-mutation-gates-2026-05-05-0355
created_at: 2026-05-05T10:17:06+09:00
source_commit_sha: 2fb38f20a8c74433684345ded8a33ba16a863621
artifact_root: artifacts/os-mutation-gates-20260505-101659-0355-final
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355
lan_retry_artifact_root: artifacts/lan-listener-retry-20260505-100452-0355
version: 0.35.5-admin-smoke
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 opt-in 범위에서 실제 Hyper-V, MSI, firewall, trust-store, LAN, Event Log, config/job store 관련 mutation gate를 실행했다.

이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Hyper-V, MSI, Service, Data Root

- Artifact: `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`
- MSI: `PureCVisorDesktopNode-0.35.5-admin-smoke-windows-x64.msi`
- MSI SHA-256: `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`
- MSI signing mode: `AllowUnsignedDev`
- MSI lifecycle: install, repair, uninstall preserve, reinstall, `REMOVE_DATA=1` uninstall, final restore install 모두 pass
- Service/data-root lifecycle: `configure-installed`, service 존재 중 `data-root-remove --remove-data` blocked, `remove-installed --remove-data` handoff-only, service absent 이후 allowlist data-root removal pass
- Hyper-V installed route smoke: VM create/start/restart, guest shutdown unavailable structured failure, checkpoint create/restore/delete, VM delete, repeat delete absent, unmanaged delete guard pass
- Final proof: service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음

## Firewall, LAN, Event Log, Trust Store

- Artifact: `artifacts/os-mutation-gates-20260505-101659-0355-final`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source `HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\PureCVisor Desktop Node` absent
- Firewall: owned rule `PureCVisor Desktop Node Local API LAN` enable pass 후 remove pass, final firewall rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/`에서 `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js` 모두 HTTP `200`
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`

## Config/Job Store Boundary

- Installed runtime Hyper-V route smoke는 실제 `C:\ProgramData\PureCVisor\desktop-node\jobs.json` job store write를 수행했다.
- Service/data-root smoke는 synthetic data root에서 `jobs.json`, token files, `events.jsonl`, `install.jsonl`, diagnostics allowlist removal을 수행했고 non-allowlist log는 보존했다.
- `product config migration apply`와 `job store migration apply`는 route matrix 기준 `future-route`, `not-implemented`, `blocked`다. 현재 제품에는 config/job store migration apply 실행 route가 없으므로 이 destructive migration apply는 실행하지 않았다.

## Final State Proof

- Installed DisplayVersion: `0.35.5`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Final product manifest version: `0.35.5-admin-smoke`
- Final firewall rule count: `0`
- Final Event Log source: absent
- Final internal trust Root/TrustedPublisher: present
- Final job store: `C:\ProgramData\PureCVisor\desktop-node\jobs.json`, length `5929`
- Boot time unchanged: pass
- `pcv-spike-*` VM 잔여물: none

## 판정

`0.35.5-admin-smoke` OS mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며, public trusted signing 또는 외부 stable publication evidence가 아니다. Config/job store migration apply는 현재 구현된 product operation이 아니므로 blocked future-route로 유지한다.
