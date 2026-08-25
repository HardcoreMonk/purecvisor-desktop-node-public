# PureCVisor Desktop Node Phase 22 Release/Version Policy Plan

> **For agentic workers:** 이 plan은 release/version policy를 채택하거나 구현으로 확장할 때 쓰는 checklist/runbook이다. 개별 정책 작성은 문서 전용이며, shared index에는 진입점만 둔다.

**Goal:** Desktop Node dev/admin-smoke/rc/stable release channel, version naming, artifact naming, upgrade/downgrade, rollback compatibility 정책을 GA 선언 없이 고정한다.

**Architecture:** Phase 22는 Phase 18의 manifest-first safe update 정책과 Phase 19의 evidence-first keep-spike 결정을 유지한다. 정책은 spec/plan 문서로 고정하고, 후속 개발에서 installer artifact naming, provenance `release_channel`, unsigned RC/stable 차단을 code contract에 일부 반영했다. ADR-0002는 이 release/version policy와 installer artifact/channel contract를 현재 적용 결정으로 채택하지만, stable 발행과 서명/관리자 권한 증거는 별도 gate로 남긴다.

**Tech Stack:** PowerShell 7, Pester 5, WiX MSI-first installer, SignTool, WinSW, Desktop Node product wrapper, manifest-first safe update policy, JSONL first diagnostics.

---

## 상태

- 작성 기준: 2026-04-29
- 현재 상태: 문서 정책 작성, installer contract 일부 반영, ADR-0002 현재 적용 결정 채택
- 관련 설계: `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy-design.md`
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 현재 적용 결정: `DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike`
- Phase 22 정책 소스:
  - `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy-design.md`
  - `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md`

## 결정 입력

- [x] **Step 1: Phase 19 제품 승격 재판정 확인**

Read:

```text
docs/ADR_INDEX.md
docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md
docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md
```

Decision input:

- Desktop Node는 독립 Windows 저장소다.
- Phase 19는 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지한다.
- release/version policy는 GA 차단 gate 중 하나다.

- [x] **Step 2: Phase 18 update/rollback boundary 확인**

Read:

```text
packaging/windows-desktop-node/README.md
packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1
```

Decision input:

- installed `product-manifest.json`이 product root version의 단일 진실이다.
- update payload version은 manifest version과 일치해야 한다.
- rollback slot은 `DesktopNode.previous` 하나다.
- job store는 destructive rewrite가 기본값이 아니다.

- [x] **Step 3: Phase 20 signed release/MSI lifecycle evidence boundary 확인**

Read:

```text
docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md
packaging/windows-desktop-node/installer/README.md
```

Decision input:

- `RequireSigned` build는 signing secret과 release WinSW artifact가 준비된 환경에서만 실행한다.
- elevated MSI lifecycle smoke는 관리자 opt-in gate다.
- Phase 22는 signed release evidence를 새로 주장하지 않는다.

## Proposed Policy Matrix

- [x] **Step 1: Release channel matrix 정의**

Policy:

| Channel | Version example | Signing | 목적 | 배포 의미 |
|---------|-----------------|---------|------|-----------|
| `dev` | `0.22.0-dev` | `AllowUnsignedDev` 가능 | 로컬 개발/unsigned build | 배포 금지 |
| `admin-smoke` | `0.22.0-admin-smoke` | unsigned 또는 signed | 관리자 smoke evidence | 배포 금지 |
| `rc` | `0.22.0-rc.1` | `RequireSigned` 필수 | signed release candidate evidence | GA 아님 |
| `stable` | `0.22.0` | `RequireSigned` 필수 | stable channel 후보 | GA 선언 아님 |

- [x] **Step 2: Version meaning 분리**

Policy:

- `0.x-dev`는 개발자 dry-run, Pester, unsigned MSI build 전용이다.
- admin smoke version은 실제 관리자 권한 mutation 증거 수집을 위한 환경 식별자다.
- signed release version은 RC 또는 stable 후보 artifact의 display/release version이며 GA 선언이 아니다.
- MSI `ProductVersion`은 Windows Installer 제약에 맞춰 suffix 없는 `major.minor.patch`로 파생될 수 있지만, provenance와 product manifest는 원본 display/release version을 보존한다.

- [x] **Step 3: Artifact naming matrix 정의**

Policy:

| Artifact | Naming |
|----------|--------|
| MSI | `PureCVisorDesktopNode-<version>-windows-x64.msi` |
| Provenance | `PureCVisorDesktopNode-<version>-windows-x64.provenance.json` |
| MSI hash | `PureCVisorDesktopNode-<version>-windows-x64.msi.sha256` |
| Diagnostic bundle | `PureCVisorDesktopNode-diagnostics-<version>-<utc-timestamp>.zip` |
| Update payload | `PureCVisorDesktopNode-update-<from-version>_to_<to-version>-windows-x64.zip` |
| Update payload manifest | `PureCVisorDesktopNode-update-<to-version>.manifest.json` |

Constraint:

- Artifact 이름과 provenance에는 secret, certificate private key, PFX password, raw API token, protected token blob, host absolute path를 넣지 않는다.
- `RequireSigned` provenance에는 `signing_trust_model`을 기록한다. 내부 서비스 운영은 `InternalEnterprise`, 외부 배포는 `PublicTrusted`, 개발자/test workaround는 `LocalTest`로 분리한다.

- [x] **Step 4: Upgrade/downgrade/rollback boundary 정의**

Policy:

- 일반 upgrade는 manifest version match, config migration dry-run, service start, health check가 통과해야 한다.
- `rc -> stable`은 stable 발행 승인과 signed MSI lifecycle/update evidence가 필요하다.
- 일반 downgrade는 지원하지 않는다.
- 허용 downgrade는 automatic previous root rollback, 명시적 `Rollback`, admin smoke rehearsal로 제한한다.
- rollback은 data root를 삭제하지 않고, previous root 하나와 `.failed` diagnostics preservation을 사용한다.

## Docs Updates

- [x] **Step 1: Phase 22 design spec 작성**

Create:

```text
docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy-design.md
```

Required sections:

- 상태와 목표
- release channel 정의
- `0.x-dev`, admin smoke version, signed release version 의미 분리
- artifact naming
- manifest-first update compatibility
- upgrade policy
- downgrade policy
- rollback compatibility boundary
- ADR impact
- validation policy

- [x] **Step 2: Phase 22 plan/runbook 작성**

Create:

```text
docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md
```

Required sections:

- 결정 입력
- proposed policy matrix
- docs updates
- tests
- completion evidence

- [x] **Step 3: Policy 채택 시 ADR/index 갱신 여부 결정**

Phase 22 정책을 현재 적용 결정으로 승격하면서 수행했다.

채택 후보 문서:

```text
docs/ADR_INDEX.md
docs/adr/0002-release-version-policy.md
docs/DEVELOPMENT_VERIFICATION_POLICY.md
docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md
follower.md
packaging/windows-desktop-node/README.md
packaging/windows-desktop-node/installer/README.md
```

결정 결과:

- `docs/adr/0002-release-version-policy.md`를 추가했다.
- `docs/ADR_INDEX.md`에 `DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike` marker를 추가했다.
- 채택 범위는 dev/admin-smoke/rc/stable channel meaning, `windows-x64` artifact naming, provenance `product.release_channel`, unsigned RC/stable 차단이다.
- 채택하지 않는 범위는 GA 승격, stable 공개 release, public trusted/stable signed release 증거 완료 주장, public trusted signed stable MSI lifecycle 증거 완료 주장, public trusted/stable signing 흐름과 묶인 Hyper-V 제품 흐름 증거 완료 주장, Event Log 전환이다.

## Tests

- [x] **Step 1: Run root documentation suite**

Run after documentation updates:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected:

- root documentation/boundary tests pass.
- pass count는 실제 실행 후에만 이 plan의 `완료 증거`에 기록한다.

- [x] **Step 2: Run diff whitespace check**

Run:

```powershell
git diff --check
```

Expected:

- exit 0.

- [ ] **Step 3: Run installer suite if policy affects installer code**

Run only when `packaging/windows-desktop-node/installer/**` code or contract tests change:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected:

- installer source, signing, provenance, artifact naming tests pass.

- [ ] **Step 4: Run packaging suite if policy affects wrapper code**

Run only when `packaging/windows-desktop-node/**` product wrapper code or tests change:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected:

- product manifest, update policy, diagnostics, invocation tests pass.

- [ ] **Step 5: Run signed/elevated evidence only by opt-in**

Do not run by default.

Run only with signing secret, release WinSW artifact, and elevated PowerShell:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.22.0-rc.1 `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node-release `
  -SigningMode RequireSigned `
  -SigningTrustModel InternalEnterprise `
  -SignToolPath '<signtool.exe>' `
  -CertificateThumbprint '<thumbprint>' `
  -TimestampUrl '<timestamp-url>'
```

Then follow Phase 20 elevated `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` lifecycle smoke.

Expected:

- signed build and MSI lifecycle evidence is recorded in the relevant evidence plan, not in high-level docs.

## Completion Evidence

Documentation created:

- [x] `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy-design.md`
- [x] `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md`

Validation status:

- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 13 passed, 0 failed
- [x] `git diff --check`: exit 0
- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 26 passed, 0 failed
- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 72 passed, 0 failed

Policy status:

- [x] dev/admin-smoke/rc/stable channel policy defined without GA declaration
- [x] MSI/provenance/diagnostic bundle/update payload naming defined
- [x] `0.x-dev`, admin smoke version, signed release version meanings separated
- [x] upgrade/downgrade/rollback compatibility boundaries tied to Phase 18 manifest-first safe update
- [x] Product runtime promotion remains `keep-spike`
- [x] ADR-0002가 policy와 installer artifact/channel contract를 현재 적용 결정으로 채택했다.

## 후속 개발 증거

2026-04-29 후속 개발에서 installer `build.ps1`의 MSI/provenance/hash sidecar output naming을 Phase 22 artifact naming matrix에 맞춰 적용했다.

적용된 output:

- `PureCVisorDesktopNode-<version>-windows-x64.msi`
- `PureCVisorDesktopNode-<version>-windows-x64.provenance.json`
- `PureCVisorDesktopNode-<version>-windows-x64.msi.sha256`
- build plan과 provenance `product.release_channel`
- RC/stable version의 `AllowUnsignedDev` 차단

관련 테스트:

- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`에 fake WiX 기반 artifact naming/sidecar 검증을 추가했다.
- RED/GREEN 확인: 새 artifact naming 테스트가 기존 legacy MSI name에서 실패한 뒤, `build.ps1` 수정 후 installer plan suite가 12 passed, 0 failed로 통과했다.
- RED/GREEN 확인: unsigned RC 차단과 `release_channel` 출력/schema 테스트가 실패한 뒤, `build.ps1`와 provenance schema 수정 후 관련 테스트가 통과했다.

## Concerns and Follow-Up

- [x] ADR-0002 승격 완료: `DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike`
- [x] Installer `build.ps1` artifact naming은 Phase 22 `windows-x64` MSI/provenance/hash sidecar name을 출력한다.
- [x] Installer `build.ps1`는 release channel을 분류하고 unsigned RC/stable artifact를 차단한다.
- [ ] Stable channel naming은 정책상 예약됐지만, stable 발행은 selected trust model approval, 릴리스 승인, signed stable MSI lifecycle, Hyper-V 제품 흐름, update compatibility, GA/product promotion 증거 gate가 닫힐 때까지 차단한다. 외부 public stable publication은 public trusted signing material이 필요하다.

2026-05-01 후속 evidence:

- `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`에서 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` MSI build와 elevated lifecycle/update compatibility smoke를 완료했다.
- 이 evidence는 `rc` channel의 local test signer smoke이며, public trusted signing 또는 stable publication evidence가 아니다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`에서 current-head `318ebc3` 기준 `0.23.10-rc.1` internal enterprise `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`, elevated lifecycle smoke PASS를 완료했다.
- 이 evidence는 내부 서비스용 `InternalEnterprise` trust model evidence이며, public trusted signing 또는 외부 stable publication evidence가 아니다.
- Stable publication은 selected trust model과 publication target이 준비된 별도 approval 뒤에만 실행한다.
