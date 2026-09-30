# clean-host base VHD 오프라인 갱신 구현 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `docs/superpowers/specs/2026-09-29-purecvisor-desktop-node-clean-host-base-vhd-refresh-design.md`를 구현하고 첫 갱신 run까지 마친다.
1. 날짜별 base VHD를 만드는 도구
2. runner의 base 선택
3. 첫 base 생성과 clean-host 한 run

**Architecture:** 한 task가 한 checkpoint다. Lane 1은 30분/tool batch 18회, Lane 2는 45분/tool batch 12회다. 브랜치 `feat/clean-host-base-vhd-refresh-20260930`에서 커밋마다 push하고, 끝에 PR 하나로 올린 뒤 green이면 merge한다.

**Tech Stack:** PowerShell 7 + Pester 5, DISM PowerShell module, C# / .NET 10 xUnit, Markdown 문서

## 사용자 결정 (2026-09-30)

| 항목 | 결정 |
| --- | --- |
| Lane 1 구현(도구, Pester, C# 계약, runner) | 승인 |
| host mutation(VHD 복사본 mount와 DISM 적용, clean-host VM 생성과 삭제) | 승인 |
| Lane 2(첫 base 생성, clean-host 한 run) | 승인 |
| push와 PR | 커밋마다 push, 끝에 PR 하나, green이면 merge |

## Global Constraints

- 원본 VHD(`20348.169…serverdatacentereval_en-us.vhd`)는 읽기만 한다.
- 도구는 기본이 dry-run이고, 업데이트를 내려받지 않는다.
- 도구의 Pester는 `manual-admin-tests/`에 둔다(packaging Pester inventory 밖). required CI용 C# 정적 계약을 더한다.
- 호스트 설정, 설치된 제품, operational current(`0.42.83-admin-smoke`)는 바꾸지 않는다. current-evidence는 쓰지 않는다.
- 기존 evidence는 덮어쓰지 않는다.
- public trusted signing과 external stable publication은 주장하지 않는다.

## Task 1: base VHD 도구와 Pester (Lane 1)

- [x] `packaging/windows-desktop-node/tools/New-PcvCleanHostBaseVhd.ps1`
  - build 모드
    - 입력: 원본 VHD, LCU `.msu`, 기대 KB, `.msu` SHA-256, 기대 UBR
    - 동작: 날짜별 복사본 `20348.<UBR>-<yyyymmdd>.vhd`에 `Mount-WindowsImage` → `Add-WindowsPackage` → 오프라인 UBR 확인 → `Dismount-WindowsImage -Save`를 하고 sidecar `….vhd.base.json`을 쓴다.
    - 실패하면 `-Discard`하고 복사본을 지운다.
  - current 모드: `-SetCurrentBasePath`로 sidecar를 확인하고 `current-base.json`을 쓴다. 최근 두 base보다 오래된 base는 `prune_candidates`로 보고만 한다.
  - `-Execute`가 없으면 계획만 낸다.
- [x] `packaging/windows-desktop-node/manual-admin-tests/PcvCleanHostBaseVhd.Tests.ps1`: 다음을 확인한다.
  - dry-run에서 쓰기가 없는지
  - 원본을 덮어쓰지 않는지
  - 실패하면 `-Discard`하는지
  - UBR 불일치를 거부하는지
  - sidecar와 current 형식이 맞는지
- [x] 검증: `Invoke-Pester -Path packaging/windows-desktop-node/manual-admin-tests/PcvCleanHostBaseVhd.Tests.ps1`, `git diff --check`

실행 기록(2026-09-30):
- 도구는 build 모드와 current 모드 두 가지다. `-Execute`가 없으면 계획만 낸다.
- `.msu` SHA-256, KB 형식, 파일 이름의 KB, 날짜 형식을 모두 확인한 뒤에만 쓴다. 대상 base나 sidecar가 이미 있으면 거부한다.
- 오프라인 UBR은 mount를 풀기 전에 `SOFTWARE` hive를 잠시 load해서 읽는다.
- 검증: Pester `14/14`, `git diff --check` clean.

## Task 2: 도구 C# 정적 계약 (Lane 1)

- [x] `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvCleanHostBaseVhdContractTests.cs`에 다음을 고정한다.
  - 원본 보존
  - dry-run 기본
  - 다운로드 없음
  - 실패 시 discard
  - sidecar/current schema
- [x] 검증: `dotnet test src/DesktopNode.Delivery.Tests`

실행 기록(2026-09-30): `PcvCleanHostBaseVhdContractTests` `3`개를 더했다. 입력 검증 뒤 계획 반환, mount 전 elevation, UBR 확인 뒤 `-Save`, 실패 시 `-Discard`와 복사본 삭제, 다운로드 명령 없음, 두 schema를 고정한다. 검증: Delivery `743/743`.

## Task 3: runner의 base 선택과 summary 필드 (Lane 1)

- [ ] `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`
  - `BaseVhdPath`를 명시하지 않으면 원본 옆 `current-base.json`을 읽는다. 파일이 없으면 원본을 쓴다.
  - plan과 summary에 `base_vhd_source`, `base_vhd_ubr`, `base_vhd_kb`를 더한다. 값은 sidecar에서 읽는다.
  - Windows Update 단계는 바꾸지 않는다.
- [ ] C# 계약에 runner의 base 선택 순서를 더한다.
- [ ] 검증: Delivery.Tests, runner를 다루는 packaging Pester(`PcvRunnerArtifactRootContract`, `PcvManualAdminRebaselineReadiness`, `PcvAdminSmokeEvidenceDocs`)

## Task 4: 첫 base 생성 (Lane 2)

- [ ] 최신 Server 2022 LCU `.msu`를 Microsoft Update Catalog에서 받고 SHA-256을 기록한다.
- [ ] 도구를 dry-run한 뒤 `-Execute`로 실행한다. 설계 §3.2의 확인 사항(합쳐진 `.msu`, `.vhd` mount)을 기록한다.
- [ ] 이때 current는 지정하지 않는다.
- [ ] evidence 문서를 새 파일로 쓴다.

## Task 5: 새 base로 clean-host 한 run (Lane 2)

- [ ] `-BaseVhdPath <새 base>`로 runner를 실행한다. 조건은 0.42.83 run과 같다: `0.42.78 → 0.42.83`, `-InstallWindowsUpdates -RemoveVmOnSuccess`.
- [ ] 다음을 확인한다.
  - `pre_update_os.ubr`가 새 base UBR과 같다.
  - `update_count`가 `0`이거나 갱신 뒤 나온 LCU 1개다.
  - 재부팅과 무응답 복구
  - 전체 시간(0.42.83 run의 46분과 비교)
- [ ] PASS이면 도구 current 모드로 `current-base.json`을 지정한다.
- [ ] evidence 문서를 새 파일로 쓴다.

## Task 6: 마무리 (Lane 1)

- [ ] 설계 문서 상태, 계획 실행 기록, campaign 종료를 기록한다.
- [ ] PR을 올리고 required CI가 green이면 merge한다.

## Nonclaims

- 이 계획은 operational current(`0.42.83-admin-smoke`)를 바꾸지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
