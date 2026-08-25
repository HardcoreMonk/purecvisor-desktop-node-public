# Config/Job Store Migration Apply 설치본 Smoke PASS - 2026-05-07

이 문서는 `product config migration apply`와 `job store migration apply`의 installed destructive admin smoke PASS evidence를 기록한다. 이 evidence는 내부 `AllowUnsignedDev` admin-smoke 범위이며, public trusted signing 또는 외부 stable publication evidence가 아니다.

## 범위

- 대상 version: `0.38.6-admin-smoke`
- Runner: `packaging/windows-desktop-node/tools/Invoke-PcvConfigJobStoreMigrationApplySmoke.ps1`
- Artifact root: `artifacts/config-jobstore-migration-apply-installed-20260507-0386`
- Summary: `artifacts/config-jobstore-migration-apply-installed-20260507-0386/summary.json`
- Public trusted signing: excluded
- External stable publication: not claimed

## 실행 요약

- Result: `ok=true`
- Actual execution: completed
- Host mutation performed: `true`
- MSI SHA-256: `d252110bee12e8c5c129b97474e2e08a51941d79d81d460fd6fe45932b290593`
- Provenance commit: `d4259670e0aa90dae869bbd0e35c8910033fb59e`
- Signing mode: `AllowUnsignedDev`
- Final service: `PureCVisorDesktopNode` `Running`
- Final manifest schema: `2`
- Final job store schema: `2`
- Boot time unchanged: `true`
- Post-migration API read: ok

## Config Migration 검증

`config-migration-apply-installed.json`은 설치본 `DesktopNode.Host.exe service-action config-migration-apply --migration-plan-id product-config-v1-to-v2 --migration-plan-version 1` 실행 결과를 기록한다.

- Process exit code: `0`
- Parsed result: `Ok=true`
- Service owner verified: `true`
- Service stopped precondition: `true`
- Product manifest path: `C:\Program Files\PureCVisor\DesktopNode\product-manifest.json`
- Migration plan id/version: `product-config-v1-to-v2` / `1`
- Source schema: `1`
- Target schema: `2`
- Mutation planned/performed: `true` / `true`
- Backup exists: `true`
- Backup path: `C:\ProgramData\PureCVisor\desktop-node\backups\config\product-config-v1-to-v2\20260506152451474\product-manifest.json`
- Temp path absent after apply: `true`

## Job Store Migration 검증

`job-store-migration-apply-installed.json`은 설치본 `DesktopNode.Host.exe service-action job-store-migration-apply --migration-plan-id job-store-v1-to-v2 --migration-plan-version 1` 실행 결과를 기록한다.

- Process exit code: `0`
- Parsed result: `Ok=true`
- Service owner verified: `true`
- Service stopped/runtime writer stopped precondition: `true`
- Job store path: `C:\ProgramData\PureCVisor\desktop-node\jobs.json`
- Migration plan id/version: `job-store-v1-to-v2` / `1`
- Source schema: `1`
- Target schema: `2`
- Mutation planned/performed: `true` / `true`
- Job count: `1`
- Seeded job present: `true`
- Backup exists: `true`
- Backup path: `C:\ProgramData\PureCVisor\desktop-node\backups\jobs\job-store-v1-to-v2\20260506152451692\jobs.json`
- Temp path absent after apply: `true`

## Post-Migration API Read

`post-migration-api-read.json`은 protected token 기반 read를 확인했다.

- `/api/v1/runtime/policy`: ok
- `/api/v1/jobs`: ok
- `/api/v1/jobs/pcv-migration-smoke-v1`: ok
- Seeded job status: `succeeded`

## Promotion 영향

`product config migration apply`와 `job store migration apply` row는 code-level candidate 상태를 벗어나 `current-native`로 승격한다. 이 승격은 `0.38.6-admin-smoke` 설치본 destructive admin smoke PASS에 한정되며, public trusted signing 또는 외부 stable publication을 주장하지 않는다.
