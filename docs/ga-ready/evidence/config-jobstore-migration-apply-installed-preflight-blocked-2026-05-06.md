# Config/Job Store Migration Apply 설치본 Smoke Preflight 차단 - 2026-05-06

이 문서는 `product config migration apply`와 `job store migration apply`의 installed destructive admin smoke 재개 시도와 현재 세션의 차단 사유를 기록한다. 이 evidence는 PASS evidence가 아니며, `current-native` 승격 근거가 아니다. 후속 elevated PASS evidence는 `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`가 소유한다.

## 범위

- 대상 version: `0.38.5-admin-smoke`
- Runner: `packaging/windows-desktop-node/tools/Invoke-PcvConfigJobStoreMigrationApplySmoke.ps1`
- Plan-only contract test: `packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1`
- 실행 시도 artifact: `artifacts/config-jobstore-migration-apply-installed-20260506-231702-0385`
- Public trusted signing: excluded
- External stable publication: not claimed

## Runner 계약

새 runner는 다음 installed product path를 계획한다.

- 현재 admin-smoke MSI build
- 설치본 MSI install/upgrade
- 설치본 service stop
- service stopped 상태에서 `jobs.json` v1 smoke fixture seed
- `DesktopNode.Host.exe service-action config-migration-apply --migration-plan-id product-config-v1-to-v2 --migration-plan-version 1`
- `DesktopNode.Host.exe service-action job-store-migration-apply --migration-plan-id job-store-v1-to-v2 --migration-plan-version 1`
- 설치본 service start
- protected token 기반 `/api/v1/runtime/policy`, `/api/v1/jobs`, `/api/v1/jobs/pcv-migration-smoke-v1` read

Plan-only mode는 host mutation을 수행하지 않으며 reboot, scheduler, firewall, trust-store, Hyper-V mutation command text를 포함하지 않는다.

## 실행 시도 결과

- Summary: `artifacts/config-jobstore-migration-apply-installed-20260506-231702-0385/summary.json`
- Result: `ok=false`, `actual_execution=failed`
- Error: `PCV_MIGRATION_SMOKE_PREFLIGHT_FAILED|Admin rights and installer build script are required.`
- Preflight: `admin=false`, `build_script_exists=true`
- 현재 shell integrity: medium mandatory level; `BUILTIN\Administrators` group은 deny-only
- Host mutation performed: `false`
- 최종 service: `PureCVisorDesktopNode`는 `Running` 유지
- Boot time unchanged: `true`

## Promotion 영향

이 차단 기록 자체는 승격 근거가 아니다. 후속 elevated `0.38.6-admin-smoke` PASS 이후 `config-migration-apply`와 `job-store-migration-apply`는 `current-native`로 승격됐으며, 이번 `0.38.5-admin-smoke` 시도에서는 MSI install, service stop/start, config write, job store migration write를 수행하지 않았다.
