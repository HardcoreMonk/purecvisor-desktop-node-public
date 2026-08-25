# MANUAL-ADMIN 캠페인 2026-05-12 0423→0424

```text
evidence_id: manual-admin-campaign-2026-05-12-0423-0424
scope: manual-admin-groups-1-2-3-4-plus-next-slice-contracts
result: PARTIAL_PASS_WITH_CLEAN_HOST_BLOCKER
baseline_version: 0.42.3-admin-smoke
target_version: 0.42.4-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.3-admin-smoke` full admin host mutation PASS 이후
`0.42.4-admin-smoke` target package로 MANUAL-ADMIN 1-2-3-4를 재실행한 결과다.
Public trusted signing, winget, external stable publication, public clean-host release
claim은 추가하지 않는다. 이 evidence는 public release evidence가 아니다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| target package root | `artifacts/admin-smoke-package-20260512-0424` |
| target MSI | `PureCVisorDesktopNode-0.42.4-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `71eaeff1c6f244bc57e9c2ac9fa57b54676d00cfbf66ba119b37c9bb21949277` |
| update package | `artifacts/manadm-0424/lifecycle/PureCVisorDesktopNode-0.42.4-admin-smoke-update.zip` |
| update package SHA-256 | `e6e8c5d24cef91d2765ec48c6ea58a49f16c0379d963512a90114da106980b2d` |
| baseline MSI | `PureCVisorDesktopNode-0.42.3-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `31ea6df1ff11cbaa9a9681b083cb5d1f61bc87ecd49db52c4e60e7a141cb229d` |

## 그룹 1: Full Admin Host Mutation

`full-admin-host-mutation-gate-20260512-042902-0424`는 PASS다.

- Batch root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-042902-0424`
- Service/MSI/Hyper-V root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-042902-0424`
- OS mutation root: `artifacts/os-mutation-gates-batch-profile-20260512-042902-0424`
- 결과: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`

## 그룹 2: Operator Access

Operator Access bucket은 PASS다.

- Installed account login: `artifacts/manual-admin-campaign-20260512-0423-0424/operator-access/installed-account-login`
- Target-backed noVNC streaming: `artifacts/manual-admin-campaign-20260512-0423-0424/operator-access/target-backed-novnc-installed-streaming`
- Installed TUI operator: `artifacts/manual-admin-campaign-20260512-0423-0424/operator-access/installed-tui-operator`

## 그룹 3: Internal Service Hardening

Internal Service Hardening bucket은 PASS다.

- Internal HTTPS/TLS lifecycle: `artifacts/manual-admin-campaign-20260512-0423-0424/internal-service-hardening/internal-https-tls-lifecycle`
- Credential Manager default transition: `artifacts/manadm-0424/cm`
- Windows Event Log default transition: `artifacts/manadm-0424/eventlog`
- Service token rotation/revoke: `artifacts/manadm-0424/service-token/service-token-rotation-revoke.json`
- Credential Manager post-token resync: `artifacts/manadm-0424/cm-post-token`

초기 Credential Manager long-path root 시도는 evidence로 사용하지 않는다.
`artifacts/manual-admin-campaign-20260512-0423-0424/internal-service-hardening/windows-credential-manager-default-transition`
및 `...-retry`는 Windows Installer service / WiX cabinet path 문제로 중단된 invalid
attempt다. 유효 PASS evidence는 short root `artifacts/manadm-0424/cm`와
`artifacts/manadm-0424/cm-post-token`이다.

## 그룹 4: Lifecycle / Packaging

Installed-host package-pair lifecycle은 PASS다.

- 중복 MSI product registration cleanup: `artifacts/manadm-0424/lifecycle/product-update-rollback/stale-product-registration-cleanup.json`
- Baseline install snapshot: `artifacts/manadm-0424/lifecycle/product-update-rollback/baseline-0423-installed-snapshot.json`
- Versioned update PASS: `artifacts/manadm-0424/lifecycle/product-update-rollback/update-0423-to-0424-versioned-summary.json`
- Updated snapshot: `artifacts/manadm-0424/lifecycle/product-update-rollback/updated-0424-installed-snapshot.json`
- Rollback PASS: `artifacts/manadm-0424/lifecycle/product-update-rollback/rollback-0424-to-0423-summary.json`
- Final rollback snapshot: `artifacts/manadm-0424/lifecycle/product-update-rollback/rollback-0423-installed-snapshot.json`

초기 direct MSI downgrade와 uninstall 후 baseline install은 같은 version `0.42.4`
ProductCode가 3개 남아 `WIX_DOWNGRADE_DETECTED`로 막혔다. 세 ProductCode 제거 후
`0.42.3-admin-smoke` baseline install, `0.42.4-admin-smoke` update, rollback은 모두
성공했다.

첫 update 실행은 `-Version` 미지정으로 `requested=0.12.0` mismatch가 발생해
host mutation 전에 중단됐다. 유효 PASS는 `-Version 0.42.4-admin-smoke`를 명시한
`update-0423-to-0424-versioned.json`이다.

## Clean-host 차단 사유

Clean-host package-pair runner는 아직 PASS가 아니다.

- 첫 시도: `artifacts/manadm-0424/clean-host`, 절대 `ArtifactRoot`가 runner 내부에서
  이중 경로로 해석되어 VM 생성 전 중단됐다. Host mutation은 발생하지 않았다.
- 재시도: `artifacts/manadm-0424/clean-host-rerun`, Hyper-V VM 생성과 PowerShell
  Direct 진입은 성공했으나 guest baseline MSI install이 `1603`으로 실패했다.
- guest blocker: `EventLogDefaultTransition` deferred custom action이 clean host에서
  `ConfigureInstalled`보다 먼저 실행되어 서비스가 아직 없는 상태에서 `1722`로
  중단됐다.
- log: `artifacts/manadm-0424/clean-host-rerun/guest-outputs/baseline-msi-install.log`
- summary: `artifacts/manadm-0424/clean-host-rerun/summary.json`

이 PR은 clean-host 실패를 근거로 WiX install sequence를 code-level 수정한다.
`ConfigureInstalled`를 `InstallFiles` 직후 실행하고, `EventLogDefaultTransition`과
`CredentialManagerDefaultTransition`은 그 뒤에 실행하도록 바꿨다. 기존
`0.42.3-admin-smoke` MSI 자체는 이미 생성된 artifact라 clean-host PASS로 소급하지
않는다. 다음 package-pair는 이 sequence fix가 포함된 새 baseline/target으로 다시
닫아야 한다.

## 다음 Slice 계약

다음 slice 계약은 code-level로 시작했다.

- Runtime/Core: `/api/v1/ops/summary`에 `installed_runtime` 요약을 추가해 version,
  service state, auth boundary, diagnostics root, latest full admin evidence anchor,
  public release not-claimed 경계를 분리한다.
- Host Ops: service token rotation과 Credential Manager transition은 service token
  source가 protected-file baseline이 아니면 mutation 전에 실패한다. TLS runner도
  protected-file baseline mismatch를 binding mutation 전에 거부한다.
- Packaging: `New-PcvManualAdminRebaselineReadiness.ps1`는 baseline/target package-pair
  mode를 지원하고 baseline-target match 또는 installed baseline mismatch를 host
  mutation 전에 blocker로 기록한다.

## 판정

Operator Access, Internal Service Hardening, installed-host update/rollback은
`0.42.3 -> 0.42.4` 기준으로 PASS다. Dedicated clean-host package-pair는 baseline MSI
custom action sequencing 결함을 발견했으므로 current PASS로 승격하지 않는다.
