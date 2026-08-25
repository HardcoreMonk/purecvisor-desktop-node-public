# PureCVisor Desktop Node Phase 22 Release/Version Policy Design

작성 기준: 2026-04-29

## 상태

- Phase: 22
- 범위: Desktop Node release channel, version naming, artifact naming, upgrade/downgrade, rollback compatibility 정책
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 선행 결정:
  - `DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`
  - `DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first`
  - `DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration`
  - `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`

Phase 22는 release/version 정책을 정의하지만, Desktop Node를 GA 제품 런타임으로 선언하지 않는다. Product runtime promotion은 signed release build, elevated MSI lifecycle, Hyper-V lifecycle integration, 장기 운영 로그 evidence, release/version policy 채택 gate가 모두 닫힐 때까지 `keep-spike`로 유지한다.

후속 개발에서 installer `build.ps1`는 이 정책 중 `windows-x64` MSI/provenance/hash sidecar naming, build plan/provenance `release_channel`, unsigned RC/stable 차단을 code contract로 강제한다. ADR-0002는 이 정책을 현재 적용 결정으로 채택했지만, stable 발행 승인은 아직 별도 gate다.

## 목표

Desktop Node artifact와 설치된 product manifest의 버전 의미를 분리하고, dev/admin-smoke/rc/stable channel별 사용 조건을 명확히 한다.

이 정책은 다음 질문에 답한다.

1. 어떤 version string이 개발 빌드, 관리자 smoke, 서명된 release 후보, 안정 채널을 뜻하는가.
2. MSI, provenance, diagnostic bundle, update payload artifact 이름은 어떤 규칙을 따라야 하는가.
3. Phase 18 manifest-first safe update 정책 아래에서 upgrade, downgrade, rollback compatibility의 허용 범위는 어디까지인가.
4. 정책 채택 시 ADR index 또는 새 ADR에 어떤 영향을 주는가.

## 비목표

- GA 선언을 하지 않는다.
- winget, MSIX, Burn bootstrapper, network download updater를 추가하지 않는다.
- code signing secret, certificate private key, PFX password, raw API token, protected token blob을 문서나 provenance에 기록하지 않는다.
- Linux Single Edge release gate나 `purecvisor-single` 버전 정책을 변경하지 않는다.
- ADR-0002 채택을 stable 발행이나 release evidence gate closure로 표시하지 않는다.

## Release Channel

| Channel | Version suffix | Signing | 목적 | 공개 의미 |
|---------|----------------|---------|------|-----------|
| `dev` | `0.<phase>.0-dev` 또는 `0.<phase>.<patch>-dev.<n>` | `AllowUnsignedDev` 가능 | 개발자 dry-run, Pester, unsigned MSI build | 배포 금지, 로컬 검증 전용 |
| `admin-smoke` | `0.<phase>.0-admin-smoke` 또는 `0.<phase>.<patch>-admin-smoke.<n>` | unsigned 가능, signed 가능 | elevated product wrapper/MSI/Hyper-V smoke 증거 수집 | 운영 배포 금지, 관리자 opt-in 증거 전용 |
| `rc` | `0.<minor>.<patch>-rc.<n>` | `RequireSigned` 필수 | signed release candidate와 install/update evidence 수집 | GA 아님, 제한된 검증 후보 |
| `stable` | `0.<minor>.<patch>` | `RequireSigned` 필수 | stable channel 후보로 승격 가능한 signed artifact | GA 선언 아님, policy상 안정 채널 이름만 예약 |

`stable` channel은 이름을 정의하지만, Phase 22 채택만으로 stable artifact를 일반 공개하거나 GA로 승격하지 않는다. Stable artifact를 발행하려면 별도 release approval, evidence gate closure, ADR/문서 update가 필요하다.

## Version Naming

Desktop Node 버전은 SemVer 형태를 따르되, `0.x` 범위에서는 compatibility가 제한적일 수 있음을 명시한다.

### `0.x-dev`

- 예: `0.22.0-dev`, `0.22.1-dev.2`
- source tree, product wrapper dry-run, unsigned MSI build에 사용한다.
- MSI `ProductVersion`은 Windows Installer 제약 때문에 suffix 없는 `major.minor.patch`로 파생할 수 있다.
- artifact provenance와 installed product manifest의 display/release version은 suffix가 포함된 원본 version string을 유지한다.
- update/rollback 호환성 증거로 사용할 수 있지만, release compatibility 보장으로 간주하지 않는다.

### Admin Smoke Version

- 예: `0.18.0-admin-smoke`, `0.22.0-admin-smoke.1`
- 실제 Windows service, MSI lifecycle, Hyper-V lifecycle, rollback smoke를 관리자 권한으로 실행할 때 사용한다.
- admin smoke version은 테스트 환경 식별자다. stable 또는 rc 승격을 뜻하지 않는다.
- 성공 증거는 해당 Phase plan의 `완료 증거`에 기록한다.
- admin smoke artifact는 내부 증거 보존용이며 외부 배포 대상이 아니다.

### Signed Release Version

- RC 예: `0.22.0-rc.1`
- Stable 후보 예: `0.22.0`
- `RequireSigned` build와 provenance SHA-256 기록이 필수다.
- RC와 stable 후보 모두 GA를 뜻하지 않는다. GA 여부는 `PRODUCT_RUNTIME_PROMOTION_DECISION`과 별도 release approval로 판단한다.
- signed release version은 MSI, provenance, update payload manifest의 version과 일치해야 한다.

## Artifact Naming

Artifact 이름은 product, channel, version, architecture, role을 드러낸다. SHA-256은 별도 `.sha256` 또는 provenance에 기록한다.

| Artifact | Naming | 생성 조건 | 비고 |
|----------|--------|-----------|------|
| MSI | `PureCVisorDesktopNode-<version>-windows-x64.msi` | dev/admin-smoke/rc/stable | 현재 build output 기준. 기존 `PureCVisorDesktopNode-<version>.msi`는 과거 artifact 참조로만 다룬다. |
| Provenance | `PureCVisorDesktopNode-<version>-windows-x64.provenance.json` | MSI build마다 필수 | signing mode, toolchain, WinSW SHA-256, MSI SHA-256 포함 |
| MSI hash | `PureCVisorDesktopNode-<version>-windows-x64.msi.sha256` | release evidence 수집 시 권장 | provenance와 값 일치 필요 |
| Diagnostic bundle | `PureCVisorDesktopNode-diagnostics-<version>-<utc-timestamp>.zip` | `CollectDiagnostics` 산출물 | raw token, protected token blob/hash, host absolute path redaction 유지 |
| Diagnostic manifest | `diagnostics-manifest.json` | diagnostic bundle 내부 필수 | bundle 내부 artifact 이름만 기록 |
| Update payload | `PureCVisorDesktopNode-update-<from-version>_to_<to-version>-windows-x64.zip` | update payload를 패키징할 때만 | Phase 18 현재는 local payload/source 기반이며 network updater는 비범위 |
| Update payload manifest | `PureCVisorDesktopNode-update-<to-version>.manifest.json` | update payload가 있을 때 필수 | installed manifest version과 payload version 일치 검증 |

Artifact 이름에는 certificate thumbprint, secret path, host path, username을 포함하지 않는다.

## Manifest-First Update Compatibility

Phase 18 정책을 유지한다.

- installed `product-manifest.json`이 설치된 product root 버전의 단일 진실이다.
- update payload의 target version은 payload manifest와 installed product manifest의 `version`에 일치해야 한다.
- update는 service stop, product root backup, local payload copy, config migration dry-run, service start, health check 순서로 실행한다.
- config migration 실패, service start 실패, health check 실패는 previous root rollback 시도로 이어진다.
- rollback slot은 `DesktopNode.previous` 하나다.
- failed root는 diagnostics 수집을 위해 `.failed` suffix로 보존될 수 있다.
- job store는 기본적으로 파괴적 rewrite를 하지 않고 schema mismatch를 read-only 또는 blocked diagnostics로 남긴다.

## Upgrade Policy

| From | To | 허용 | 조건 |
|------|----|------|------|
| `dev` | `dev` | 허용 | local 개발 검증 전용, compatibility 보장 없음 |
| `dev` | `admin-smoke` | 허용 | 관리자 smoke evidence 목적, 성공 결과를 release compatibility로 해석하지 않음 |
| `admin-smoke` | `admin-smoke` | 허용 | 같은 smoke track에서만 권장 |
| `admin-smoke` | `rc` | 조건부 허용 | clean install 또는 명시적 migration smoke evidence 필요 |
| `rc` | `rc` | 허용 | signed artifact, manifest version match, migration dry-run 통과 필요 |
| `rc` | `stable` | 조건부 허용 | stable 발행 승인, signed provenance, MSI lifecycle evidence, update smoke evidence 필요 |
| `stable` | `stable` | 허용 예정 | stable channel 채택 이후 patch/minor 정책을 ADR로 고정해야 함 |

서명된 RC 또는 stable 후보로 승격하려면 Phase 20 signed release/MSI lifecycle evidence와 관련 installer suites가 통과해야 한다.

## Downgrade Policy

일반 downgrade는 지원하지 않는다.

허용되는 예외는 다음뿐이다.

- update 실패 중 자동 previous root rollback
- 운영자가 같은 host에서 직전 version으로 되돌리는 명시적 `Rollback`
- admin smoke 중 증거 수집을 위한 controlled downgrade rehearsal

Downgrade를 지원하지 않는 이유는 DPAPI LocalMachine protected token, job store, diagnostics schema, product manifest schema가 `0.x` 동안 backward compatibility를 항상 보장하지 않기 때문이다. 다운그레이드가 필요하면 이전 설치의 diagnostic bundle을 먼저 수집하고, data root 보존/삭제 선택을 명시해야 한다.

## Rollback Compatibility Boundary

Rollback은 release channel 간 자유 이동이 아니라 안전장치다.

- rollback 대상은 단일 previous root다.
- rollback 성공 기준은 service start와 protected-token runtime policy health check다.
- rollback은 `%ProgramData%\PureCVisor\desktop-node`의 protected token, legacy token, job store, event log를 삭제하지 않는다.
- config migration이 data root를 변경하는 경우 mutation 전 backup artifact를 남겨야 한다.
- migration이 irreversible이면 해당 update는 stable 또는 rc release payload가 될 수 없다.
- rollback 후 diagnostic bundle은 `update-policy-redacted.json`, `migration-plan-redacted.json`, `rollback-state-redacted.json`을 포함해야 한다.

## Policy Matrix

| 항목 | `dev` | `admin-smoke` | `rc` | `stable` |
|------|-------|---------------|------|----------|
| 예시 version | `0.22.0-dev` | `0.22.0-admin-smoke` | `0.22.0-rc.1` | `0.22.0` |
| signing mode | `AllowUnsignedDev` 가능 | `AllowUnsignedDev` 또는 `RequireSigned` | `RequireSigned` | `RequireSigned` |
| MSI build | optional | smoke에 필요 | 필수 | 필수 |
| provenance | build 시 필수 | build 시 필수 | 필수 | 필수 |
| elevated MSI lifecycle | 불필요 | 목적에 따라 필수 | 필수 | 필수 |
| Hyper-V lifecycle | 불필요 | 목적에 따라 필수 | release gate로 필요 | release gate로 필요 |
| update smoke | optional | 목적에 따라 필수 | 필수 | 필수 |
| 배포 | 금지 | 금지 | 제한 검증 후보 | GA 아님, 발행 승인 필요 |

## ADR 영향

이 정책을 채택하면 현재 ADR 운영 규칙상 ADR 반영이 필요할 수 있다.

- `docs/ADR_INDEX.md`에 release/version policy decision marker를 추가할지 결정해야 한다.
- 새 ADR을 만든다면 예: `DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike`
- Phase 18 update policy의 provenance/channel field가 code contract로 확장되면 ADR-0001을 supersede하지 말고 새 ADR로 release/version policy를 추가하는 편이 적합하다.
- 단순 문서 정책으로만 유지한다면 Phase 22 spec/plan을 상세 이력으로 두고 ADR decision marker 추가는 policy 채택 시점까지 보류할 수 있다.

공유 문서에는 Phase 22 진입점 링크와 후속 개발 반영 상태를 둘 수 있다. 정책의 ADR/현재 적용 결정 채택과 stable channel 발행 의미는 별도 채택 작업에서 기록한다.

## Validation Policy

문서 정책만 추가할 때:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

이 정책을 installer, wrapper, provenance, manifest code contract에 반영할 때:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

정책이 Web Console, CLI, Local API runtime policy 노출을 바꾸면 해당 component suite와 `node --check web/app.js`도 추가한다.

## Completion Criteria

- channel과 version naming이 dev/admin-smoke/rc/stable로 분리되어 있다.
- artifact naming이 MSI/provenance/diagnostic bundle/update payload에 대해 정의되어 있다.
- installer build output과 provenance가 Phase 22 MSI/provenance/hash naming, `release_channel`, unsigned RC/stable 차단을 강제한다.
- upgrade/downgrade/rollback compatibility가 Phase 18 manifest-first safe update 정책과 충돌하지 않는다.
- signed release version과 stable channel이 GA 선언이 아님을 명시한다.
- Product runtime promotion decision은 `keep-spike`로 유지된다.
- ADR impact와 validation command가 문서화되어 있다.
