# Desktop Node 개발 검증 규칙

<!-- BEGIN GENERATED CURRENT EVIDENCE -->
## Current operational evidence (generated)

- Version: `0.42.75-admin-smoke`
- Active operator surfaces: Web Console and PCVCLI; `tui_present=false`.
- Package evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md`.
- Full admin host mutation: `full-admin-host-mutation-gate-20260821-04275` / `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md`.
- Actual-VM functional evidence: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`.
- Feature qualification: `contract=pcv-feature-promotion-decision-v1`; `promotion_eligible=true`; `blocker_count=0`; `blockers=none`.
- Installed CLI/Web current-card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md`; CLI exit 0, Web HTTP 200, service Running/Automatic, TUI absent.
- Clean MSI SHA-256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`.
- Operational MSI SHA-256: `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b`.
- Operational payload aggregate SHA-256: `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7`.
- Provenance commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`.
- Latest closed manual-admin pair: `0.42.74-admin-smoke -> 0.42.75-admin-smoke` / `manual-admin-campaign-descriptor-20260827-04274-04275`.
- Claims: `public_trusted_signing=false`; `external_stable_publication=false`.
<!-- END GENERATED CURRENT EVIDENCE -->

## 2026-08-25 Required CI Pester-free cutover closure PASS with disclosed evidence deviation

Authoritative shadow commit `f8208f076cb9db69022b4dc060e65f13d23fae8c`의 pull-request
Development Gates run
`https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32898937784`
attempt 1에서 기존 네 job과 replacement 네 shard가 같은 SHA로 PASS했다. Legacy Pester는
`627/627`, replacement는 Web `50/50`, Installer `49/49`, Delivery `528/528`, .NET
`2210/2210`이며 failed/skipped/not-run은 모두 `0`이다. Provider wall-clock은
`186000 ms`로 `214000 ms` 한도 안이다. 여덟 artifact는 API로 다시 내려받아 provider
SHA-256과 일치함을 확인했다. 단일 증빙은
`docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md`다.

Direct-child cutover `68756f1f2f609951aaf54d76963b10f96409011b`에서 catalog는 `active`,
일곱 suite는 모두 `cutover`, ledger는 62 files / 627 contracts 전체
`cutover / local pass / CI pass`로 전환됐다. Cutover run `32900785756`과 PR #1의 일반 merge
commit `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`에 대한 remote-main push run
`32901477892`가 모두 PASS했다. Main 수치는 .NET `2210/2210`, Web `50/50`, Delivery
`528/528`, Installer `49/49`이고 missing/duplicate/skipped/not-run/timeout/cancel은 `0`,
workflow wall-clock은 `204000 ms`로 `214000 ms` ceiling 이하다.

Required workflow의 실행 job은 정확히 `dotnet`, `web`, `delivery`, `installer-policy` 네
개이며 main protection도 strict/admin enforcement와 force-push/deletion disabled를 유지한 채
이 네 GitHub Actions context만 요구한다. 정적 executable node 기준 Pester, 비관리자
PowerShell, host/service/MSI/VM mutation invocation은 각각 `0`이다. Windows 실행 step은
`cmd`, Ubuntu 실행 step은 기본 Bash를 사용한다. 따라서 `cutover_completed=true`,
`required_ci_pester_zero=true`, `required_ci_nonadmin_powershell_zero=true`다.

보호 규칙 전환은 immediate compare-before/PATCH/readback으로 검증했고 exact rollback JSON과
old/new payload hash를 보존했다. 다만 전환 전 ETag와 원본 provider-before response hash는
closure material에 보존되지 않아 reconstructed pre-state hash로 대체 표기했다. 이는 rollback
재현에는 충분하지만 해당 캡처 단계를 원계획 literal-compliance PASS로 주장하는 근거는 아니다.

Legacy Pester 62 files는 non-required historical parity/rollback source로 보존하되 active
workflow oracle로 사용하지 않는다. `.github/workflows/public-boundary.yml`, local/manual
Pester instructions와 관리자 scripts도 residue이므로 repository-wide PowerShell zero를
주장하지 않는다. Operational current `0.42.74-admin-smoke`, saved-lifecycle actual-VM blocker,
public trusted signing과 external stable binary publication 상태는 변경하지 않는다.

## 2026-08-25 public source safety gate

Before a parentless source root is seeded or any repository visibility changes, run both repository-owned
commands:

```text
npm run test:public-source-safety --prefix web
npm run verify:public-source-safety --prefix web
```

The verifier enumerates tracked paths with a shell-free Git argument array, rejects personal profiles,
configured operator identifiers, observed private endpoints, unmarked synthetic RFC1918 fixtures, credential
URLs, private-key blocks, private provider locators, nested Git metadata, path escape, symlink and unexpected
binary/archive content. It also requires `LICENSE`, `SECURITY.md`, and
`docs/PUBLIC_SOURCE_AUTHORITY.md`. Findings expose only rule, relative path, line, count, and canonical report
digest; matched values are never emitted.

Official pinned Gitleaks is an independent required oracle and must report finding count `0`. Neither scanner
may be bypassed with an ignore file, fingerprint allowlist, exit suppression, or claim downgrade. These gates
are public-root bootstrap policy. 이 bootstrap checkpoint 당시 Required CI 전환은 pending이었고,
현재 cutover 진행 상태는 바로 위 절과 cutover evidence를 우선한다.

아래 dated section의 수치는 각 source-state snapshot이다. 현재 operational tuple은 위 생성 블록과
`docs/ga-ready/current-evidence.json`을 우선한다.

## 2026-08-25 Packaging verification Wave D local parity

Wave D는 Packaging legacy Pester 55개 파일의 528개 계약을 같은 ID와 순서의 C# custom facts로
교체했다. D1~D10과 최종 aggregate에서 replacement와 일회성 Pester 5.7.1 reference가 각각
`528/528`, failed/skipped/not-run `0`으로 PASS했다. 단일 증빙은
`docs/ga-ready/evidence/pester-free-packaging-wave-d-2026-08-25.md`다.

Strict v2 migration ledger의 62 files / 627 contracts는 Web 50, Installer 49, Packaging 528
모두 `mapped` / local `pass` / CI `pending`이다. `delivery-contracts`와 `evidence-check`를
`mapped`로 승격했고, `wave-d-pending`은 schema와 managed catalog에서 더 이상 허용하지 않는다.
Catalog activation은 계속 `plan-only-foundation`이며 current-evidence 검증은 read-only,
write/child-process `0`을 유지한다.

최종 Pester reference에서 sanitized public root에 포함되지 않는 frozen 0.42.65 reader fixture로
인한 조건부 2건은 bootstrap evidence에 고정된 SHA-256의 read-only binary를 ignored
`artifacts/**` 경계에서만 사용해 실제 실행했다. 이 binary는 추적하거나 공개하지 않는다.
Required workflow는 아직 legacy authority이므로 same-SHA dual-run, Pester/non-admin PowerShell
zero, branch protection cutover는 Wave E 완료 전까지 주장하지 않는다. 이 local parity는 host
mutation, package build, actual-VM, public trusted signing 또는 external stable publication의
근거가 아니다.

## 2026-08-25 Installer verification Wave C local parity

Wave C는 Installer legacy Pester 6개 파일의 49개 계약을 동일 ID와 순서의 C# custom facts로
교체했다. Clean input `0ab1bda71f3398aed302d53e7d6715987ce87b19`에서 replacement와 일회성
Pester 5.7.1 reference가 각각 `49/49`, failed/skipped/not-run `0`으로 PASS했다. 단일 증빙은
`docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md`다.

이 Wave C checkpoint 당시 strict v2 migration ledger는 62 files / 627 contracts를 소유했다.
Web 50과 Installer 49는
`mapped` / local `pass` / CI `pending`이고 Packaging 528은 `unmapped` / local·CI `pending`이다.
`installer-contracts.migration_state=mapped`만 승격하며 `delivery-contracts`와 `evidence-check`는
`wave-d-pending`, catalog activation은 `plan-only-foundation`으로 유지한다.

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter Category=Installer --no-restore --nologo
npm run check:verification-migration-manifest --prefix web
node --test web/node-tests/verification-migration-manifest.test.mjs
```

Replacement는 PowerShell, Pester, MSI/service/VM mutation 도구 또는 shell을 실행하지 않는다.
Pester 5.7.1 전체 49건 실행은 로컬 parity를 위한 마지막 일회성 reference oracle이며 Required CI
경로가 아니다. Required workflow는 변경하지 않았으므로 `required_ci_pester_zero=false`,
`required_ci_nonadmin_powershell_zero=false`, `cutover_completed=false`를 유지한다. 이 evidence는
host mutation, package build, actual-VM, public trusted signing 또는 external stable publication의
근거가 아니다.

## 2026-08-24 Web verification Wave B local parity

Wave B는 기존 Web Pester 50개 계약과 같은 순서·이름의 Node `node:test` projection 50개를
구현하고, clean code input `20ba3b80c211cc6a29bc9ecaf7e9195911678f14`에서 positive
`50/50`과 controlled missing-`app-root` negative parity를 로컬 PASS로 확인했다. 단일 진실은
승인 설계, 실행 계획과 code-level evidence다.

- `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b-design.md`
- `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b.md`
- `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`

Migration ledger는 `config/development-verification-migration-manifest.json`, strict shape는
`config/development-verification-migration-manifest.schema.json`이 소유한다. Web 한 행만
`mapped` / local `pass` / CI `pending`이며 다른 61행은 `unmapped` / local `pending` / CI
`pending`을 유지한다.

Wave B 검증은 기존 기본 그래프를 바꾸지 않고 다음 별도 명령으로만 실행한다.

```text
npm run check:web-contract-registry --prefix web
npm run check:verification-migration-manifest --prefix web
npm run test:web-contracts --prefix web
npm run verify:web-contract-negative-parity --prefix web
```

기존 `npm test --prefix web`, `npm run verify:parity --prefix web`, legacy
`web/tests/PcvDesktopWeb.Static.Tests.ps1`와 required workflow는 계속 authoritative하다. CI parity,
required gate의 Pester 제거, non-admin PowerShell 제거와 cutover는 아직 완료되지 않았다. 이
local evidence는 host/service/MSI/VM mutation, actual-VM 검증, 제품 승격, public trusted signing
또는 외부 stable publication의 근거가 아니며 operational current `0.42.74-admin-smoke`와 열린
saved-lifecycle actual-VM blocker를 변경하지 않는다.

## 2026-08-24 C# verification Wave A foundation

Wave A의 C# 계획 투영 진입점은 다음 명령 계약을 따른다.

```text
dotnet run --project src/DesktopNode.Verification -c Release -- verify
  --lane Fast|Full|Release
  --change-tier S|M|L
  --changed-path <path>...
  --artifact-root <path>
  [--suite <id>...]
  [--shard dotnet|web|delivery|installer-policy]
  [--plan-only]
```

versioned catalog와 strict schema의 단일 경로는 각각
`config/development-verification-suites.json`과
`config/development-verification-suites.schema.json`이다. catalog의 suite 순서는
`dotnet`, `web-typecheck`, `web-parity`, `delivery-contracts`, `installer-contracts`,
`evidence-check`, `policy-boundaries`이며, shard 순서는 `dotnet`, `web`, `delivery`,
`installer-policy`다.

- 현재 `activation_state=plan-only-foundation`은 `--plan-only` 계획 투영만 허용한다.
  비-plan-only 요청은 child process 시작 전에 `PCV_VERIFY_CONFIG_INVALID`로 fail-closed된다.
- Wave A~D 동안 `.github/workflows/development-gates.yml`의 기존 required job과
  `packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1`가 계속
  authoritative 실행 경로다. C# 계획 투영은 이를 대체하지 않는다.
- 이 foundation은 제품 API, host/admin 동작, MSI/service/VM mutation, ADR-0009 Guest
  PowerShell Direct transport를 변경하지 않는다.
- required CI의 Pester-free와 non-admin PowerShell-free 상태는 아직 획득하지 않았다.
  `required_ci_pester_zero=false`, `required_ci_nonadmin_powershell_zero=false`이며 Wave E의
  dual-run, cutover와 required workflow 증빙 전에는 이 값을 승격하지 않는다.

## 2026-07-16 개발 피드백 레인과 변경 등급

상세 S/M/L 기준과 자동 경로 분류의 단일 정책은
`docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`다.

- `Fast`는 로컬 변경 범위 피드백이다. `src/**`는 .NET, `web/**`는 npm/parity와 Web
  Pester, installer와 Batch Supervisor는 각 focused Pester, 문서는 `git diff --check`를
  선택한다. 분류할 수 없는 경로는 검증 축소 대신 `Full`로 승격한다.
- `Full`은 pull request와 `main`의 비변경 필수 검증이다. 기존 `dotnet-tests`,
  `web-tests`, `packaging-pester`, `installer-web-pester` 네 독립 job을 유지하고,
  `Invoke-PcvDevelopmentVerification.ps1 -Lane Full -ChangeTier M -PlanOnly`로 일곱 suite
  orchestration 계약을 함께 검증한다.
- `Release`는 비변경 release preflight다. 패키지 빌드, 설치, service/Hyper-V/firewall/
  trust-store/Event Log mutation은 포함하지 않으며 별도 명시 승인과 operational evidence를
  계속 요구한다.

변경 등급은 다음과 같이 고정한다.

- `S`: 한 모듈의 국소 변경이며 public contract, installer lifecycle, host mutation 경계가
  바뀌지 않는다. Focused test와 `Fast`를 요구한다.
- `M`: 여러 모듈 또는 API/CLI/Web/packaging 비파괴 계약 변경이다. 짧은 설계 기록과
  `Full`을 요구한다.
- `L`: 보안 경계, installer lifecycle, current evidence anchor, 실제 host mutation 또는
  public release 경계 변경이다. 전체 설계·구현 계획, `Release`, 필요한 operational
  evidence를 요구한다.

등급이 애매하면 높은 등급을 사용한다. installer lifecycle, host mutation, 보안, current
evidence, public release, signing/publication 경로는 입력값과 무관하게 `L`이다. API/CLI/Web
계약과 일반 packaging 경로는 최소 `M`이다. `Fast + M`은 `Full`, 모든 `L`은
`Release`로 자동 승격한다. Unknown 경로는 등급을 임의로 `L`로 만들지 않고 검증만 `Full`로
승격한다. 로컬 실행 진입점은 다음과 같다.

```powershell
& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Fast -ChangeTier S -ChangedPath @('src/DesktopNode.Core/InternalHelper.cs') `
  -ArtifactRoot artifacts/development-verification-fast
```

2026-07-16 동일 host 계측에서 Batch Supervisor focused suite는 40.5초에서
12.22/12.44초로 단축됐다. Packaging Pester와 installer/Web Pester 합계는
201.2초(385+94건)에서 134.83초(393+97건)로 33.0% 단축됐다. .NET 591건, Web
type/static/parity, packaging 393건, installer/Web 97건이 모두 PASS했다. 이 변경의
`host_mutation_performed=false`이며 `0.42.64-admin-smoke` operational anchor, public
trusted signing, external stable publication 상태를 변경하지 않는다.


## 2026-07-14 CLI/Web-only 검증 경계

- ADR-0011에 따라 활성 운영자 표면 검증은 Web Console과 PCVCLI를 대상으로 한다.
- TUI source/test/package/smoke가 active product로 다시 들어오지 않는 boundary guard를
  유지한다. Code-level evidence는
  `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`다.
- Local API/backend 검증은 계속 유지한다. `0.42.65-admin-smoke`
  package/fullgate/actual-VM functional correctness/CLI-Web installed current-card가 PASS했으며
  `0.42.64-admin-smoke`는 immediate CLI/Web predecessor, `0.42.62-admin-smoke` Web/TUI/CLI
  current-card는 historical TUI predecessor로 보존한다.
- Current installed gate는 explicit protected-token CLI `host status`, `runtime policy`,
  `network inventory`, Web `/`와 `/pcv-config.js`, service `Running/Automatic`,
  `tui_present=false`를 검증한다.

## 2026-07-13 개발 게이트 복구 기준

- 비관리자 Windows checkout에서 `dotnet test src/DesktopNode.sln -c Release`를 통과해야
  한다.
- CLI 단위 테스트는 고유한 누락 default protected-token 경로를 주입하고, Host 단위
  테스트는 recording ACL hardener를 주입한다. 설치된 머신 상태와 실제 Windows ACL 변경은
  단위 테스트의 입력이 아니다.
- 제품 기본값은 계속
  `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`이며, protected token과 account
  bootstrap 파일은 ACL 상속을 차단하고 Administrators/SYSTEM에만 읽기 권한을 부여한다.
- `Development Gates`의 `dotnet-tests`, `web-tests`, `packaging-pester`,
  `installer-web-pester` 네 required job은 pull request, `main` push, manual dispatch에서
  실행하는 필수 개발 검증이다. Feature branch direct push는 동일 head의 pull request run과
  중복되지 않도록 제외한다.
- 이 workflow는 비변경 검사만 수행하며, 관리자 권한이 필요한 설치본 smoke 또는
  service/MSI/Burn/MSIX/Hyper-V/firewall/trust-store/Event Log 검증을 대체하지 않는다.
- package/full-admin/installed current-card anchor는 `0.42.65-admin-smoke` /
  `full-admin-host-mutation-gate-20260716-04265`다. 최신 closed
  manual-admin package-pair는 `0.42.58-admin-smoke -> 0.42.59-admin-smoke`이며, 이 docs/CI
  변경은 public trusted signing 또는 external stable publication을 열지 않는다.

## 2026-05-29 현재 기준

현재 검증 anchor는 `0.42.59-admin-smoke` full admin host mutation /
`0.42.59-admin-smoke` 설치본 current-card이며, 최신 closed manual-admin package-pair는 04258→04259이다. Ledger는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, package evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`, full gate evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`,
manual-admin evidence는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`다.
설치본 current-card는 04259 fullgate 후 PASS했고, 최신 설치본 화면 evidence는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`다.

최신 closed manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`이며,
descriptor `manual-admin-campaign-descriptor-20260529-04258-04259-closed`가
`missing_count=0`, `not_pass_count=0`을 기록한다. 초기 full-gate attempt의 service repair
idempotence hang은 `76c77a86bbb72e415b1968169c16f1638b76fa56` 수정 후 r2 PASS로 닫혔고,
Hyper-V QoS mutation package/fullgate/manual-admin closure는 `77f1a3f291b4f736218cb5110dcecd3b464860d4`
기준으로 닫혔다. Phase 3 Web/TUI QoS direct control package/fullgate/manual-admin closure는
`46e745efc698a06e4b065a19c3f07217e821155e` 기준으로 닫혔다.
Guest Execution provider/direct-control은 `cc774b257d6cd772c3a890266aca62aa8ab8eadc`
기준으로 provider/fullgate/current-card PASS 후 `2c11e359709c775be7a57ea9624716720c5b62d6`
기준으로 0.42.54 fullgate PASS이며 04250→04254 manual-admin readiness는 baseline
mismatch blocker다. Web/TUI running guest execution cancel affordance와 actual credentialed
guest-exec는 `958052181012f7d1be6ccff535316bfaeeef07df` 기준 0.42.55 package/current-card로
승격됐다. `0.42.59-admin-smoke`는 Guest Execution redaction hardening과 Hyper-V QoS mutation
value hardening을 package/fullgate/manual-admin/current-card chain으로 승격했고, 최신
public-boundary `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`는
이미 열린 installed current-card payload 후보 `0.42.60-admin-smoke`를 유지한다. docs-maintenance
postpush만으로 추가 package 후보를 열지 않는다.
이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable
publication evidence가 아니다. 아래 이전 날짜 current 문단은 historical predecessor로
해석한다.
직전 `0.42.58-admin-smoke` predecessor는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.

## 2026-05-21 historical predecessor

현재 검증 anchor는 `0.42.40-admin-smoke` full admin host mutation /
manual-admin package-pair closure다. Ledger는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, full gate evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`,
manual-admin evidence는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md`다.
설치본 current-card는 04240 기준으로 PASS했고, 최신 설치본 화면 evidence는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`다.
설치본 PCVCLI QoS/guest targeted smoke는
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`에서
추가 확인했다.
Web/TUI QoS/guest readback surface는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`에서 code-level
PASS했고, 이 product payload 변경은 `0.42.40-admin-smoke` package chain
`closed-manual-admin-package-pair-04239-04240`로 닫혔다.
Actual VM Web/TUI QoS/guest readback evidence는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`가
소유한다. 설치본 TUI row projection blocker는 source fix code-level PASS 후
`0.42.41-admin-smoke` package chain trigger로 남긴다.
최신 closed manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md`이며,
descriptor `manual-admin-campaign-descriptor-20260521-04239-04240-closed`가
`missing_count=0`, `not_pass_count=0`을 기록한다. 아래 이전 날짜 current 문단은
historical predecessor로 해석한다.

## 2026-05-17 현재 기준

Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` / `manual-admin-campaign-descriptor-20260517-04226-04227-closed`이며, target MSI SHA-256 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`, update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`, provenance commit `69aba3eb3ff08c843f1a481818ddc86eac2f019b`와 함께 `host-ops-lifecycle-descriptor-bridge-v1` / `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated` 계약을 보존한다.

Historical PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

> 대상: `purecvisor-desktop-node`

현재 검증 anchor는 설치본 기준 `0.42.30-admin-smoke` Runtime/API current evidence rollup이다.
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`가 최신 full admin
host mutation과 installed Web/TUI/CLI current-card PASS를 소유한다. 최신 닫힌 manual-admin
package-pair PASS는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`가 소유한다.
`runtime-api-current-evidence-rollup-v1`과 artifact-discovered `current_evidence` rollup은
ops summary current-card 검증 key이며 public trusted signing 또는 외부 stable publication
evidence가 아니다.

## 핵심 원칙

- 검증되지 않은 항목은 정상으로 간주하지 않는다.
- 기본 비관리자 검증은 C#/.NET `DesktopNode.Verification`과 Node required entrypoint 기준이다.
  PowerShell 7과 Pester 5는 비필수 legacy parity 또는 manual/admin 검증에만 사용한다.
- 실제 host mutation은 관리자 권한 opt-in gate로 분리한다.
- Linux Single Edge 릴리스 게이트와 Desktop Node 내부 전용 제품 런타임 판단은 분리한다.

## 프로젝트 종료까지 고정된 개발 가속 기준

이 기준은 내부 사설망 전용 서비스로 확정된 Desktop Node 개발 완료 기준으로 프로젝트 종료까지 고정한다. Public trusted signing, trusted timestamp, 외부 stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke는 ADR-0006 기준 `out-of-scope`다. 변경하려면 별도 ADR 또는 verification policy 변경과 root documentation guard 갱신이 필요하다.

- public trusted signing과 public distribution gate는 개발 완료 scope 밖으로 고정한다. Stable/public signing blocker는 release boundary 문서에서 `out-of-scope` 상태로 분리하고, 일반 개발 완료 판정에 포함하지 않는다.
- 완료 기준은 `AllowUnsignedDev`, internal `RequireSigned`, admin-smoke evidence까지만 사용한다. 이 evidence는 public trusted signing이나 외부 stable publication을 주장하지 않는다.
- 작업은 작은 vertical slice로만 진행한다. 각 slice는 계약 테스트 또는 TDD red를 먼저 만들고, 구현, 영향 범위 검증, 커밋/푸시까지 닫는다.
- 일반 개발 loop는 fake controller, Pester, xUnit, dotnet/npm 검증으로 닫고 가능한 검증은 병렬화한다.
- 실제 Hyper-V/Service/MSI host mutation, firewall/Event Log/trust-store, Task Scheduler, MSI install/repair/uninstall, VM create/delete는 destructive boundary가 바뀌는 milestone evidence 지점에서만 실행한다.
- 2026-05-17 `full-admin-host-mutation-gate-2026-05-17-04228-hostmutation`은 `0.42.28-admin-smoke` historical full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04228`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04228`, `artifacts/os-mutation-gates-batch-profile-20260517-04228`이고 full-gate MSI SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`, signing mode는 `AllowUnsignedDev`다. Installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260517-04228`, `runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`, Host Ops Web diagnostics table `host-ops-web-diagnostics-bucket-table-v1`를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-17 `manual-admin-campaign-2026-05-17-04227-04228`는 `0.42.27-admin-smoke -> 0.42.28-admin-smoke` historical 닫힌 package-pair PASS evidence다. Target operational MSI SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, update ZIP SHA-256은 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`, descriptor `manual-admin-campaign-descriptor-20260517-04227-04228-closed`는 `missing_count=0`, `not_pass_count=0`이다.
- 2026-05-17 `manual-admin-campaign-2026-05-17-04225-04226`는 `0.42.25-admin-smoke -> 0.42.26-admin-smoke` historical 닫힌 package-pair PASS evidence다. Target operational MSI SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP SHA-256은 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`, provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`, descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`는 `missing_count=0`, `not_pass_count=0`이다.
- 2026-05-16 `manual-admin-campaign-2026-05-16-04224-04225`는 `0.42.24-admin-smoke -> 0.42.25-admin-smoke` historical 닫힌 package-pair PASS evidence다. Target MSI SHA-256은 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, update ZIP SHA-256은 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`, provenance commit은 `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`, descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`는 `missing_count=0`, `not_pass_count=0`이다.
- 2026-05-16 `admin-smoke-package-2026-05-16-04226`는 historical product payload package candidate record다. MSI SHA-256은 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`, provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`이며, 당시 operational full-gate package는 routeparity package root의 MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`가 소유했다.
- 2026-05-16 `manual-admin-campaign-descriptor-2026-05-16-04225-04226`은 `0.42.25-admin-smoke -> 0.42.26-admin-smoke` initial candidate descriptor다. Readiness는 PASS지만 당시 lifecycle evidence가 아직 채워지지 않아 `missing_count=4`, `not_pass_count=1`, `overall_status=blocked-by-missing-evidence`였고, 2026-05-17 closure evidence에서 PASS로 승격했다.
- 2026-05-16 `admin-smoke-package-2026-05-16-04224`, `full-admin-host-mutation-gate-2026-05-16-04224-hostmutation`, `installed-operator-surface-current-card-2026-05-16-04224`는 Runtime/API current evidence rollup historical predecessor다. Full-gate batch는 `full-admin-host-mutation-gate-20260516-04224`이고, package build MSI SHA-256은 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256은 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`다. Descriptor `manual-admin-campaign-descriptor-20260516-04223-04224`는 `blocked-by-missing-evidence`, `missing_count=5`, `not_pass_count=1`로 blocked다.
- Previous 04221 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221`, `artifacts/os-mutation-gates-batch-profile-20260516-04221`, full-gate MSI SHA-256 `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`, provenance commit `3b8c48deb4c31675f6fce46c320703f23c27c131`로 보존한다.
- Historical 04220 full admin host mutation evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`, full-gate MSI SHA-256 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`로 보존한다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-13 `full-admin-host-mutation-gate-2026-05-13-04211-hostmutation`은 `0.42.11-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다.
- 2026-05-13 `full-admin-host-mutation-gate-2026-05-13-0429-hostmutation`은 `0.42.9-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`이고 full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260513-040213-0429`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.9-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-12 `full-admin-host-mutation-gate-2026-05-12-0427-hostmutation`은 `0.42.7-admin-smoke` 이전 full admin host mutation PASS evidence다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`이고 full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`, route/OS child evidence `available`, errors `0`을 확인했다. final service `Running`, installed manifest `0.42.7-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `manual-admin-operator-hardening-followup-2026-05-10-0415`은 0.41.5 installed Operator Access와 Internal Service Hardening 재확인 evidence다. `artifacts/manual-admin-followup-20260510-0415`에서 installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager default transition, internal HTTPS/TLS lifecycle, Event Log default transition이 PASS했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-07 `0.38.8-admin-smoke` installed destructive update/rollback smoke는 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`에서 PASS했다. 같은 payload의 최초 non-elevated attempt `artifacts/product-update-rollback-mutation-20260507-0388`는 blocked history로 보존한다. Elevated PASS는 update `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`, rollback restore to `0.38.6-admin-smoke`, `DesktopNode.failed` diagnostics root `0.38.8-admin-smoke`, final service `Running`, boot time unchanged, `host_mutation_performed=true`를 확인했다.
- Network update source gate와 updater catalog/channel resolver 변경은 packaging product wrapper 변경으로 본다. 기본 검증은 `packaging/windows-desktop-node/tests`이며, catalog/channel code-level evidence는 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`에 기록한다. 이 evidence는 external publication service, public trusted signing, 외부 stable publication, installed destructive catalog update smoke를 주장하지 않는다.
- Update filesystem rollback 변경은 packaging product wrapper 변경으로 본다. 기본 검증은 `packaging/windows-desktop-node/tests`이며, product root backup 이후 rollback code-level evidence는 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`에 기록한다. 이 evidence는 post-crash resume/reconcile, service/data/config/job-store transaction manager, public trusted signing, 외부 stable publication을 주장하지 않는다.
- Installer artifact publication descriptor 변경은 installer artifact/channel contract 변경으로 본다. 기본 검증은 `packaging/windows-desktop-node/installer/tests`이며, descriptor code-level evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`에 기록한다. 이 evidence는 Burn bootstrapper, MSIX, winget manifest submission, public trusted signing, 외부 stable publication을 주장하지 않는다.
- ADR-0006 internal private network distribution 변경은 public release boundary 문서 변경으로 본다. 기본 검증은 Desktop Node documentation guard, `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`, `git diff --check`이며, `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`는 `internal-private-network-only`, `public_trusted_signing: out-of-scope`, `winget_submission: out-of-scope`, `external_stable_publication: out-of-scope`, `clean_host_public_signed_install_update_rollback_smoke: out-of-scope`, `internal_signed_msi_status: pass`, `internal_updater_catalog_channel: code-level-pass`, `private_lan_smoke: pass`, `internal_update_rollback_smoke: installed-destructive-pass`, `internal_https_tls_lifecycle_installed_smoke: pass`, `internal_clean_host_install_update_rollback_smoke: pass`를 기록해야 한다.
- ADR-0005 `public-distribution-operations-expansion-candidate` 변경은 public release boundary 문서 변경으로 본다. 기본 검증은 `packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1`, Desktop Node documentation guard, `git diff --check`이며, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`는 public trusted signing/external stable publication을 `not-claimed` 또는 explicit blocked state로 유지해야 한다. External/public row는 `timestamp_evidence: blocked-by-missing-public-signing-cert-and-timestamp-url`, `public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication`, `clean_host_smoke_status: not-run`, `credential_manager_transition: installed-local-system-default-transition-pass`, `credential_manager_system_context_proof_runner: code-level-native-service-action`, `service_credential_manager_default_transition: installed-admin-smoke-pass`, `event_log_provider_transition: installed-provider-register-write-pass`, `event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass`, `event_log_default_writer: installed-admin-smoke-pass`, `event_log_schema_version: 1`, `tls_certificate_lifecycle: partial-code-level-cert-generate-rotate-delete-pass`, `tls_binding: not-run`, `winget_validation_status: winget-cli-validate-pass`, `winget_submission: not-submitted`, `catalog_publication: not-published`, `catalog_publication_blocker: missing-upload-endpoint-and-credentials`, `burn_bootstrapper: build-install-repair-remove-pass-internal-smoke`를 기록한다. Final 1-7 descriptor는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `artifacts/public-ops-final-followup-attempt-20260509-0391`, `New-PcvPublicOpsFinalFollowupAttempt.ps1`를 historical prerequisite scan으로 보존한다. Installed internal ops row는 별도 관리자/evidence로 `service_token_rotation_revoke: installed-admin-smoke-pass`, `service_credential_manager_default_transition: installed-admin-smoke-pass`, `token_source_migration: protected-file-to-credential-manager`, `credential_manager_service_reload_status: restarted`, `credential_manager_old_source_rejection_status: protected-file-source-rejected-after-reload`, `credential_manager_rollback_diagnostics_status: written`, `event_log_default_transition: installed-admin-smoke-pass`, `event_log_default_transition_artifact_root: artifacts/windows-event-log-default-transition-installed-20260510-0396`, `token_value_observed: false`, `installed_listener_external_load_rate_limit: pass`처럼 기록할 수 있다. MSIX는 preflight descriptor의 `msix: feasibility-blocked-by-service-packaging-design`와 별도 관리자 opt-in lifecycle evidence `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`의 `msix: build-install-update-remove-pass-internal-smoke`, `host_mutation_performed: true`, `public_trusted_signing: excluded`, `external_stable_publication: not-claimed` 경계를 함께 보존해야 한다.
- ADR-0005 public distribution ops execution bundle 변경은 `packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1`, 관련 component preflight tests, Desktop Node documentation guard, `npm test --prefix web`/`npm run verify:parity --prefix web`가 Web beta surface를 포함할 때, 그리고 `git diff --check`를 요구한다. Bundle evidence는 `public_distribution_ops_execution_bundle: code-level-nonmutating-bundle-pass`, `actual_execution: local-preflight-bundle-executed`, `host_mutation_performed: false`, public trusted signing/external stable publication `not-claimed`를 보존해야 한다.

- ADR-0005 service token rotation/revoke installed mutation 변경은 `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostServiceActionTests.ServiceTokenRotationRevoke"` RED/GREEN evidence와 관리자 installed smoke evidence를 요구한다. 최신 evidence는 `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`, `artifacts/service-token-rotation-revoke-installed-20260509-150334`이며 `service_token_mutation: performed`, `token_value_observed: false`, `new_token_value_created: true`, `service_reload_status: restarted`, `old_token_rejection_status: old-token-rejected-after-reload`, `token_rotation_audit_status: written`, `host_mutation_performed: true`를 기록한다. Public trusted signing/external stable publication은 계속 `not-claimed`다.

- ADR-0005 winget CLI validation 변경은 `packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1`, 실제 `winget validate --manifest`, `git diff --check`를 요구한다. 최신 evidence는 `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`, `artifacts/winget-cli-validate-20260509-0391`이며 `winget_validation_status: winget-cli-validate-pass`, `winget_submission: not-submitted`, `host_mutation_performed: false`를 기록한다.

- ADR-0005 Burn bootstrapper lifecycle 변경은 Burn authoring/source MSI hash binding, actual build, install/repair/remove smoke, final service restore evidence, Desktop Node documentation guard, `git diff --check`를 요구한다. 최신 evidence는 `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416`이며 `burn_bootstrapper: build-install-repair-remove-pass-internal-smoke`, `host_mutation_performed: true`, public trusted signing/external stable publication `not-claimed`를 기록한다.

- ADR-0005 Windows Credential Manager transition 변경은 preflight tests, current-user capability smoke, token value redaction proof, service-account context analysis, SYSTEM-context proof runner tests, rollback diagnostics plan, installed LocalSystem smoke, Desktop Node documentation guard, `git diff --check`를 요구한다. 최신 installed evidence는 `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`이며 native `credential-manager-default-transition`과 MSI deferred `CredentialManagerDefaultTransition` custom action이 `NT AUTHORITY\SYSTEM` proof, protected-file to Credential Manager migration, service reload, old source rejection, rollback diagnostics를 PASS로 기록했다.

- ADR-0005 Windows Event Log provider default transition 변경은 provider registration/write/query evidence, repair/remove policy review, log volume guard review, Desktop Node documentation guard, `git diff --check`를 요구한다. 최신 installed evidence는 `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`이며 default writer transition, provider repair/remove installed smoke, schema/versioning, volume guard installed evidence를 PASS로 기록한다.

- ADR-0005 public external publication/signing gate 변경은 public signing material, timestamp URL, external catalog/package upload endpoint and credentials, public stable installer URL/SHA-256, winget submission path, clean-host runner availability를 먼저 증명해야 한다. 최신 blocked evidence는 `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`, `artifacts/public-external-gates-blocked-20260509-0391`이며 timestamp evidence, external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback은 blocked 상태다.

- ADR-0005 public ops final follow-up attempt 변경은 `packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1`, Desktop Node documentation guard, `git diff --check`를 요구한다. 최신 evidence는 `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `artifacts/public-ops-final-followup-attempt-20260509-0391`이며 1-7 final public operations follow-up prerequisite scan의 `remaining_follow_up_count: 7`, `host_mutation_performed=false`, `public_release=not-claimed`, public trusted signing/external stable publication not-claimed boundary를 보존한다.
- ADR-0005 public ops gate execution readiness 변경은 `packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1`, Desktop Node documentation guard, `git diff --check`를 요구한다. 최신 readiness descriptor는 `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`, `artifacts/public-ops-gate-execution-readiness-20260509-0392`이며 external stable publication/catalog upload, winget submission, clean-host public signed install/update/rollback blocker를 보존하고 TLS code-level slice `partial-code-level-cert-generate-rotate-delete-pass`, `tls_private_key_material_written=false`, `tls_binding=not-run`, `host_mutation_performed=false`, public trusted signing/external stable publication not-claimed boundary를 기록한다. Credential Manager SYSTEM proof blocker는 후속 `windows-credential-manager-default-transition-installed-2026-05-10-0395` evidence에서 installed PASS로 닫혔다.
- Account/RBAC/JWT/console/noVNC 변경은 API account auth tests, Host listener/service-action tests, packaging product plan/manifest/invoke Pester, Web static tests, `npm test --prefix web`, `npm run verify:parity --prefix web`, installed account login smoke, frontend/backend auth console live smoke, target-backed noVNC installed streaming smoke, `dotnet test src/DesktopNode.sln`, `git diff --check`를 요구한다. 최신 evidence는 `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`, `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`, `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`이며 기본 bootstrap은 `no-default-account`, 계정 미구성 상태의 protected bearer-token gate 유지, installed account login smoke PASS `artifacts/installed-account-login-smoke-20260510-0410-final`, real account login form browser QA PASS `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`, noVNC bridge explicit target host/port opt-in, installed target-backed streaming PASS, token/password/JWT redaction 경계를 보존한다.

## 저장소 경계

```text
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike
DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT: dotnet-windows-service-host-default-with-keep-spike
DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start
DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter
```

Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN Single Edge runtime 검증은 이 저장소의 대상이 아니다.

## 변경 유형별 검증

| 변경 유형 | 기본 검증 | 추가 smoke | 관리자 gate |
|-----------|-----------|------------|-------------|
| Desktop Node Hyper-V helper spike 변경 | Pester non-integration 필수 | 불필요 | Hyper-V host gated integration 조건부 |
| Desktop Node Phase 2A/2B/2C/2D/2E/2F/2G/2H Local API + Phase 4/10 checkpoint-facing route + Phase 5 LAN hardening + Phase 7 token file hardening + Phase 8 installer hardening + Phase 9 runtime hardening + Phase 15 protected token source spike 변경 | Pester API suite 필수 | loopback 또는 명시적 LAN 수동 smoke 조건부 | firewall 실제 적용과 token/protected token ACL 검증 조건부 |
| Desktop Node Phase 3A/3B/10 Web Console spike 변경 | Pester web static suite + `node --check` 필수 | Local API suite 필수 | loopback 수동 smoke 조건부 |
| Desktop Node Phase 4/10/15 CLI spike 변경 | Pester CLI suite 필수 | Local API suite 필수 | loopback 수동 smoke 조건부 |
| Desktop Node Phase 6 Windows service packaging + Phase 7 service token file hardening + Phase 8 installer hardening + Phase 15 protected token storage spike 변경 | Pester service suite 필수 | service `PrepareTokenFile -WhatIf`, `PrepareProtectedTokenFile -WhatIf`, `Config`, `Install -WhatIf` smoke 필수 | 실제 `sc.exe install/start/stop/delete`, firewall rule 적용, token/protected token ACL 검증 조건부 |
| Desktop Node runtime promotion decision 또는 root boundary 문서 변경 | Desktop Node root boundary suite 필수 | Desktop Node component suite smoke 조건부 | 제품 승격 전 관리자 권한 integration gate 별도 설계 필요 |
| Desktop Node AGENTS/ADR index/ADR 문서 변경 | Desktop Node root boundary suite 필수 | `git diff --check` 필수 | 제품 승격 전 관리자 권한 integration gate 별도 설계 필요 |
| Desktop Node Phase 12/13/14/15/16/17/18/19 Service-first product wrapper와 제품 승격 경계 변경 | Packaging Pester suite + API/service/CLI/web component suite 필수 | product dry-run smoke 필수 | 실제 product install/start/rollback/uninstall, protected token ACL, Event Log source 등록, Hyper-V lifecycle, mutating update/rollback/config migration, signed release/MSI lifecycle 조건부 |
| Desktop Node Phase 13 WinSW product wrapper 변경 | Packaging Pester suite + API static/auth/LAN suite + service suite 필수 | product dry-run smoke와 WinSW XML/manifest/diagnostic 검증 필수 | 실제 `Install -WinSwPath`, service RUNNING, token 포함 runtime policy 200, loopback Web Console root 200, CollectDiagnostics, Uninstall 조건부 |
| Desktop Node Phase 14 signed installer와 repair/uninstall UX 변경 | `packaging/windows-desktop-node/installer/tests`, `packaging/windows-desktop-node/tests`, root boundary Pester suite 필수 | WiX CLI가 있으면 unsigned dev build 필수 | 실제 `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` smoke와 signed release build 조건부 |
| Desktop Node Phase 16 Event Log와 long-term diagnostics 변경 | `packaging/windows-desktop-node/tests`, API/service/root boundary suite 필수 | `Plan`, `CollectDiagnostics -WhatIf`, log rotation/dry-run diagnostics contract 필수 | 실제 Windows Event Log source 등록과 장기 운영 log inspection 조건부 |
| Desktop Node Phase 17 LAN mode 제품 보안 정책 변경 | `packaging/windows-desktop-node/tests`, API/service/root boundary suite 필수 | `Plan`, `CollectDiagnostics -WhatIf`, runtime policy network contract 필수 | 실제 LAN listener start, firewall rule ensure, reverse proxy/TLS smoke 조건부 |
| Desktop Node Phase 18 update/rollback/config migration 구현/문서 변경 | Packaging Pester suite + API/service/root boundary suite 필수 | product `Plan`, `Update -WhatIf`, `Rollback -WhatIf`, `CollectDiagnostics -WhatIf` smoke 필수 | 실제 mutating update/rollback/service start/config migration smoke 조건부이며 결과는 Phase 18 plan의 완료 증거에 기록 |
| Desktop Node Phase 19 제품 승격 재판정 문서/test 변경 | Desktop Node root boundary suite 필수 | `git diff --check` 필수 | signed release build, elevated MSI lifecycle, Hyper-V lifecycle, Event Log/provider 또는 JSONL 장기 운영 evidence는 별도 관리자 opt-in gate |
| Desktop Node Phase 20 signed release/MSI lifecycle evidence 문서/계약 변경 | Desktop Node root boundary suite 필수. Lifecycle plan/classification code가 바뀌면 `packaging/windows-desktop-node/installer/tests` 필수 | `git diff --check` 필수 | `RequireSigned` build와 elevated `msiexec` lifecycle smoke는 signing secret과 관리자 권한 opt-in gate. Repair smoke는 `/fa`가 아니라 `/i REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable` 계약을 따른다. Repair `3010`은 assertion 통과 시 reboot-required 성공으로 기록하고, `1641`은 Windows Installer가 실제 reboot를 시작한 실패/중단 evidence로 기록한다. |
| Desktop Node internal signing trust model 변경 | Installer signing/provenance suite + Desktop Node root boundary suite 필수 | `New-PcvInternalCodeSigningTrust.ps1 -DryRun`, `git diff --check` 필수 | 실제 internal Root/leaf 생성, LocalMachine trust import, signed MSI build, lifecycle smoke는 관리자 opt-in gate. Public `.cer`만 evidence에 남기고 private key/PFX/password는 기록하지 않는다. |
| Desktop Node Phase 21 Hyper-V lifecycle integration evidence 문서/분류 변경 | Hyper-V non-integration Pester suite + Desktop Node root boundary suite 필수 | `git diff --check` 필수. Checkpoint evidence classifier가 바뀌면 focused classifier test도 함께 실행 | 실제 Hyper-V VM create/start/poweroff/checkpoint/remove와 product API lifecycle smoke는 관리자 opt-in gate |
| Desktop Node Phase 22 release/version policy 문서 또는 installer artifact/channel contract 변경 | Desktop Node root boundary suite 필수. Installer contract가 바뀌면 installer suite 필수 | `git diff --check` 필수 | signed release/version 정책 채택과 stable 발행은 별도 release approval과 관리자 opt-in gate |
| ADR-0006 internal private network distribution boundary 변경 | `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1` + Desktop Node root boundary suite 필수 | `git diff --check` 필수 | public signing/winget/external upload/public clean-host smoke는 out-of-scope. Internal HTTPS/TLS lifecycle installed smoke와 internal clean-host install/update/rollback smoke는 내부 관리자/runner evidence로 별도 기록 |
| ADR-0005 public distribution/operations expansion gate 변경 | `packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1` + Desktop Node root boundary suite 필수 | `New-PcvPublicDistributionDescriptor.ps1 -PlanOnly`, `New-PcvPublicDistributionReadiness.ps1 -PlanOnly`, `New-PcvWingetManifestCompliancePreflight.ps1 -PlanOnly`, actual `winget validate --manifest` when winget manifest output changes, `New-PcvUpdaterCatalogPublicationPreflight.ps1 -PlanOnly`, `New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1 -PlanOnly`, `New-PcvWindowsCredentialManagerTransitionPreflight.ps1 -PlanOnly`, `New-PcvWindowsEventLogProviderTransitionPreflight.ps1 -PlanOnly`, `New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1 -PlanOnly`, `New-PcvServiceTokenRotationRevokePreflight.ps1 -PlanOnly`, `New-PcvDiagnosticBundleServerPreflight.ps1 -PlanOnly`, `New-PcvBurnBootstrapperPreflight.ps1 -PlanOnly`, `New-PcvMsixPackagingFeasibilityPreflight.ps1 -PlanOnly`, `New-PcvPublicOpsFinalFollowupAttempt.ps1 -AllowLocalEvidenceWrite`, `New-PcvPublicOpsGateExecutionReadiness.ps1 -AllowLocalEvidenceWrite [-RunLocalTlsLifecycle]`, `git diff --check` 필수 | public trusted signing, external stable publication, publication, signed public update/rollback, Event Log/TLS mutation은 별도 ADR 채택과 관리자/release approval gate; Credential Manager default transition has separate internal installed evidence; final 1-7 descriptor keeps remaining public ops follow-up count `7`; execution-readiness descriptor may record TLS `partial-code-level-cert-generate-rotate-delete-pass`; service token and installed listener load may have separate internal installed evidence |
| ADR-0005 public distribution ops execution bundle 변경 | `packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1` + component preflight tests + Desktop Node root boundary suite 필수 | `New-PcvPublicDistributionOperationsBundle.ps1 -AllowLocalDescriptorWrite`, Web beta fixture checks when UI status changes, `git diff --check` 필수 | local non-mutating bundle only; public trusted signing/external publication/host mutation remains not-claimed/not-run |

ADR-0005 timeout/rate-limit hardening gate 변경은 `packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1`, `New-PcvTimeoutRateLimitHardeningPreflight.ps1 -PlanOnly`, Desktop Node root boundary suite, `git diff --check`를 함께 요구한다. 실제 route timeout middleware 적용, request rate-limit middleware 적용, retry semantics 변경, UI/API error contract 검증, load test 실행, server config mutation은 별도 implementation evidence와 관리자/release approval gate를 요구한다.

ADR-0005 diagnostic bundle server code-level 변경은 `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj`, `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj`, 관련 packaging product plan Pester, Desktop Node root boundary suite, `git diff --check`를 요구한다. Code-level API action evidence는 `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`처럼 `POST /api/v1/diagnostics/bundles`, `GET /api/v1/diagnostics/bundles/{bundle_id}/download`, `.bundle.json` archive, `[REDACTED]` redaction, token-required route contract, retention, `--diagnostics-root` service plan wiring을 함께 기록한다. 상태는 `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`로 기록한다. Installed listener execution, product wrapper diagnostics delegation, elevated host mutation, public trusted signing, external stable publication은 별도 implementation evidence와 관리자/release approval gate를 요구한다.

ADR-0005 diagnostic bundle list pagination/retention 변경은 `docs/ga-ready/evidence/diagnostic-bundle-list-pagination-retention-2026-05-09.md`처럼 `GET /api/v1/diagnostics/bundles?limit=&offset=` route contract, retention-before-list behavior, `next_offset` page metadata, Web Console retained bundle list/`Load more bundles` UX, `Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1`, `npm run browser:fixture --prefix web`, `git diff --check`를 함께 기록한다. 이 slice는 read-only API/Web hardening이며 host mutation, public trusted signing, external stable publication은 주장하지 않는다.

ADR-0005 diagnostic bundle Host listener code-level 변경은 `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`처럼 focused `DesktopNodeHostApplicationTests` RED/GREEN evidence와 함께 기록한다. Evidence는 `X-PCV-Request-Id`, `X-Request-Id`, bearer-required listener route, `.bundle.json` redaction, `X-PCV-Diagnostic-Bundle-Id` download header를 확인하고 `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`를 보존해야 한다. Installed service listener PASS 여부는 별도 elevated rerun evidence가 소유한다.

ADR-0005 diagnostic bundle product wrapper code-level 변경은 `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`처럼 focused `PcvDesktopNodeProduct.Diagnostics.Tests.ps1` RED/GREEN evidence와 함께 기록한다. Evidence는 `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`, `New-PcvDesktopNodeDiagnosticBundle`, `product-wrapper-delegation-redacted.json`, `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `actual_execution: code-level-product-wrapper`, `host_mutation_performed: false`, public trusted signing/external stable publication `not-claimed`를 확인해야 한다. Installed service listener execution은 `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md` 같은 elevated rerun evidence가 별도로 소유한다.

ADR-0005 diagnostic bundle native service-action config 변경은 `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`처럼 focused `DesktopNodeHostServiceActionTests` RED/GREEN evidence와 함께 기록한다. Evidence는 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config의 `DesktopNodeWindowsServiceConfiguration.BinaryPathName`에 `--diagnostics-root`, protected token file, `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`가 포함되는지 확인해야 한다. Installed service listener execution은 `0.39.0-admin-smoke` elevated MSI/service rerun에서 `installed-listener-pass`, blocker `none`으로 닫혔으며, future native service-action config 변경은 이 installed rerun evidence를 stale trigger로 취급한다.

ADR-0005 timeout/rate-limit hardening code-level 변경은 `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj`, `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj`, 관련 packaging product plan Pester, Desktop Node root boundary suite, `git diff --check`를 요구한다. Code-level route response deadline은 `timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md`처럼 problem-details/Retry-After evidence와 함께 기록하고, service plan/native service-action config wiring은 `timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md`와 `diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`처럼 `DesktopNode.Host.exe listen` hardening arguments evidence와 함께 기록한다. Code-level in-process load evidence는 `timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md`처럼 `DesktopNodeApiRequestProcessor` 병렬 요청, success/rate-limit 예산, problem-details contract evidence와 함께 기록한다. Installed listener external load evidence는 `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`처럼 real HTTP request count, HTTP 200/429 distribution, `Retry-After`, `PCV_RATE_LIMIT_EXCEEDED`, `token_value_observed=false`, public trusted signing/external stable publication `not-claimed`를 함께 기록한다. Installed service config mutation은 별도 implementation evidence와 관리자/release approval gate를 요구한다.

API/Host job hardening 설치본 evidence 변경은 `dotnet test src/DesktopNode.sln`, 관련 Host/Api/Runtime xUnit, `packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1`, `Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -DryRun`, 그리고 별도 관리자 opt-in 설치본 evidence 문서를 요구한다. Evidence에는 `POST /api/v1/auth/login` oversized body의 HTTP `413`/`PCV_REQUEST_BODY_TOO_LARGE`, `GET /api/v1/runtime/policy`, `GET /api/v1/jobs?limit=1&offset=0`, job cancel/readability contract, `worker_responsiveness`, `host_mutation_performed`, `cooperative_cancellation_scope`, `wmi_abort_claim`, `token_value_observed=false`, `public_trusted_signing`, `external_stable_publication`을 명시한다. 통제된 slow native route 없이 설치본 smoke가 HTTP `504`/`PCV_ROUTE_TIMEOUT`을 실제로 만들 수 없으면 `route_timeout.status=not-run-installed-smoke-has-no-controlled-slow-native-route`로 기록하고 PASS를 주장하지 않는다. `-RunRateLimitProbe`는 반복 요청 부하를 만들기 때문에 명시 opt-in으로만 사용한다.

| Desktop Node Phase 23 Windows operational evidence 문서 변경 | Desktop Node root boundary suite 필수 | diagnostics policy 변경 시 packaging diagnostics suite 추가 | 장기 service run, service failure/recovery, Event Log source registration, firewall/LAN/TLS preview는 관리자 opt-in gate |
| Desktop Node post-reboot verification tooling 변경 | `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1` + packaging suite 필수 | dry-run entrypoint smoke와 injected runner evidence contract 필수 | 실제 Task Scheduler 등록과 reboot 이후 scheduled-task 실행은 별도 관리자 opt-in gate. `Restart-Computer` 자동 호출은 금지하며 `-Reboot` 요청은 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 실패해야 한다. |
| Desktop Node Phase 24 Local API job runtime boundary 변경 | Local API Pester suite + Desktop Node root boundary suite 필수. Hyper-V helper operation이 바뀌면 Hyper-V non-integration suite 추가 | runtime policy contract 변경 시 CLI/Web 영향 검토, 필요 시 CLI/Web suite 추가 | 실제 Hyper-V lifecycle, service install/start/stop/delete, MSI lifecycle은 계속 별도 관리자 opt-in gate |
| Desktop Node Phase 25 .NET/TypeScript 전환 변경 | .NET contract/runtime/API/service/host/route parity 변경은 `dotnet test src/DesktopNode.sln` + 관련 Pester suite 필수. 문서만 바뀌면 Desktop Node root boundary suite + `git diff --check` | TypeScript Web Console 변경 시 `npm test --prefix web`, `npm run generate:parity --prefix web`, `npm run verify:parity --prefix web`, `npm run browser:fixture --prefix web`, `node --check`, Web Pester suite 추가. Product wrapper/MSI 경로 변경 시 packaging + installer suite 추가 | .NET service host replacement는 기본 제품 service/MSI path지만, Hyper-V/service/MSI/firewall/Event Log mutation은 계속 관리자 opt-in gate |
| CLI/Web-only operator surface 변경 | `dotnet test src/DesktopNode.sln`, Web verification, packaging payload/boundary tests, `git diff --check` 필수 | ADR-0011, current ledger와 code-level evidence를 함께 갱신 | TUI source/test/package/smoke 재도입을 금지하고 Local API/backend coverage는 유지한다. Installed promotion은 0.42.64 package/fullgate/actual-VM functional correctness/CLI-Web current-card가 소유한다 |
| Desktop Node Web/API listener port split 변경 | `dotnet test src/DesktopNode.sln`, `Invoke-Pester -Path packaging/windows-desktop-node/tests`, `Invoke-Pester -Path web/tests`, `npm test --prefix web`, `npm run verify:parity --prefix web`, Desktop Node documentation guard, `git diff --check` 필수 | Evidence는 `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`와 `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`처럼 Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...`, `/pcv-config.js`, CORS origin, Web listener `/api/*` rejection, installed listener execution 여부를 함께 기록 | 기존 설치본 service `binPath`를 바꾸거나 port 80/443 URL reservation, service restart, MSI repair/install을 실행하면 관리자 opt-in installed listener smoke가 필요하다. 2026-05-10 Web 80/API 7777 설치본 smoke는 PASS이고, HTTPS/443은 TLS binding/trust evidence 전까지 `not-run` |

## Desktop Node 문서 동기화 규칙

- High-level 문서에는 suite pass count를 복제하지 않는다.
- Pass count는 각 Phase plan의 완료 증거와 실행 로그에 둔다.
- 현재 적용 결정은 `docs/ADR_INDEX.md`와 `docs/adr/`에 연결한다.
- DocumentationSync guard는 위 규칙을 검사하며 component/archive baseline 소유권에 둔다.

## Batch Supervisor / Hang Guard

예상 실행 시간이 길어 hang 복구와 재개 지점 추적이 필요한 개발 배치는 저장소 내부 Batch Supervisor를 우선 사용한다. Evidence root에는 `current-step.json`, `heartbeat.jsonl`, `gpu-snapshots.jsonl`, step별 JSON result, `summary.json`이 포함되어야 한다.

규칙:

- 비파괴 verification profile은 관리자 승인을 요구하지 않는다.
- `requires_admin=true` 또는 `mutates_host=true` step은 명시적 `-AllowHostMutation`과 elevated shell을 요구한다.
- 자동 reboot 또는 scheduled-task command는 v1에서 금지한다.
- Resume은 command fingerprint가 일치하는 successful step만 skip할 수 있다.
- `retry_count`는 명시적 재시도 예산이다. 각 attempt는 `step-results/<ordinal>-<step>.attempt-NN.json`에 남기고, 최종 aggregate는 기존 resume contract와 호환되도록 `step-results/<ordinal>-<step>.json`에 남긴다. Retry 중에는 `current-step.json`과 `heartbeat.jsonl`에 `retrying` 상태를 기록해야 한다.
- 실행 중인 step attempt는 기본 5초 간격으로 Windows GPU adapter/process memory counter snapshot을 `gpu-snapshots.jsonl`에 JSONL로 기록한다. `Get-Counter`가 없는 환경이나 counter 수집 실패는 batch 실패로 승격하지 않고 해당 snapshot line의 `status=unavailable`과 `error`로 남긴다.
- 캡처된 stdout/stderr, arguments, summary artifact는 저장 전에 token/path redaction을 거친다.
- Admin gate profile은 `ServiceMsiHyperVAdminSmoke`, `OsMutationGate`, `FullAdminHostMutationGate` 세 가지다. Manifest 생성과 `-DryRun`, `Invoke-PcvOsMutationGateSmoke.ps1 -PlanOnly` 검증은 non-mutating이지만 실제 실행은 `-AllowHostMutation`과 elevated shell을 요구한다.
- `ServiceMsiHyperVAdminSmoke`와 `FullAdminHostMutationGate` 안의 Service/MSI/Hyper-V step은 `0.37.0-admin-smoke`에서 회복된 MSI repair transient 대응을 위해 기본 `retry_count=1`을 사용한다. `OsMutationGate` 기본값은 `retry_count=0`이며, 실제 OS mutation gate 재시도는 manifest option으로 명시해야 한다.
- Route parity MSI lifecycle은 실패 중간에도 `msi-lifecycle-smoke.json` partial evidence를 저장한다. Repair `1603`은 log marker가 좁은 classifier와 일치할 때만 `msi-repair-retryable-transient`로 기록하며, 성공으로 간주하지 않고 Batch Supervisor retry 또는 명시적 resume으로만 회복한다.
- Batch Supervisor v2 구현 검증은 profile expansion, guard, dry-run, OS gate plan-only summary만 확인한다. 이 검증은 Hyper-V, service, MSI, firewall, LAN, Event Log, trust-store mutation을 재실행하지 않으며 최신 OS gate evidence도 갱신하지 않는다.
- Local API `GET /api/v1/ops/summary`는 `DesktopNode.Host.exe listen --batch-evidence-root <path>`가 설정된 경우 Batch Supervisor evidence를 `data.batch_evidence` read-only object로 요약할 수 있다. 이 API는 HTTP request에서 evidence path를 받지 않고 설정된 root 밖을 읽지 않는다.
- Evidence root가 없거나 JSON parse가 실패해도 ops summary route 자체는 실패하지 않는다. 최신 Batch Supervisor summary 자체가 누락되면 `batch_evidence.status="missing"`, 최신 summary 자체가 파싱 불가이면 `batch_evidence.status="unavailable"`로 내려가며 `signals[key=batch-evidence]`가 `warn` tone을 반환한다.
- 최신 Batch Supervisor summary는 읽히지만 child evidence(route/MSI/Hyper-V, OS mutation, provenance, MSI lifecycle, GPU snapshots)가 누락, malformed, unreadable, containment rejected 상태이면 `batch_evidence.status="degraded"`와 sanitized `PCV_BATCH_EVIDENCE_*` error code를 반환한다. 이 경우 `latest` run identity와 읽을 수 있는 child data는 유지한다.
- `batch_evidence` 응답은 command stdout/stderr, command arguments, bearer token, protected token file content, protected token file path, absolute local evidence root, repository root를 노출하지 않는다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.

## Internal RequireSigned gate 검증 경계

- `rc`와 `stable` installer artifact는 `AllowUnsignedDev`를 거부하고 `RequireSigned`와 명시적 `SigningTrustModel`을 요구한다.
- `InternalEnterprise` evidence는 signed MSI provenance에 `signing_trust_model=InternalEnterprise`와 `msi.signed=true`를 남겨야 한다.
- 최신 InternalEnterprise evidence는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`의 `0.38.7-rc.1` `RequireSigned` build다. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, Authenticode는 `Valid`, SignTool verify exit는 `0`, signer thumbprint는 `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`이다.
- `New-PcvInternalCodeSigningTrust.ps1 -DryRun`은 plan-only check이며 LocalMachine trust import를 실행하지 않는다.
- 실제 internal Root/leaf 생성, `LocalMachine` trust import, signed MSI build, elevated MSI lifecycle smoke는 별도 관리자 opt-in 없이는 실행하지 않는다.
- Provenance, dry-run JSON, docs evidence에는 private key/PFX/password, certificate password, API token, protected token blob을 기록하지 않는다.
- 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.

## 기본 검증 명령

`installer-policy` shard는 cutover 경계 때문에 clean committed HEAD를 요구한다. 따라서 변경 중에는
다음 pre-commit 검증을 실행한다.

```text
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
npm ci --prefix web
npm run test:required --prefix web
git diff --check
```

전체 solution test의 `policy-boundaries`는 활성 cutover 계약상 clean committed HEAD를 요구한다.
변경 중에는 영향 범위의 focused test만 실행한다. Clean committed HEAD에서 전체 solution test는
`dotnet` shard가, Installer 필터와 clean-worktree policy boundary는 `installer-policy` shard가 검증한다.

커밋 후 `git status --short` 출력이 비어 있는 상태에서 Required CI exact four를 실행한다. `web`
shard가 `npm run test:required`를 포함하므로 별도로 다시 실행하지 않는다.

```text
git status --short
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/local-dotnet --shard dotnet
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/local-web --shard web
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/local-delivery --shard delivery
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/local-installer-policy --shard installer-policy
```

위 `.ps1` 값은 changed-path 데이터이며 PowerShell process 호출이 아니다. Component/archive baseline
검증은 기본 개발 loop에서 분리한다. PowerShell Local API, service helper, CLI, Hyper-V helper와
legacy root boundary Pester는 비필수 component/archive 또는 manual/admin baseline으로만 유지한다.

### 비필수 legacy/manual/admin Pester 예시

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

이 예시는 Required CI 명령이 아니다. `.github/workflows/public-boundary.yml`과 관리자 script도
별도 residue이며 repository-wide PowerShell zero를 주장하지 않는다.

## 현재 evidence 요약

2026-05-05 이후 Desktop Node는 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime` 상태이며 내부 전용 서비스로 확정됐다. Single Edge 릴리스 게이트, public trusted signing, 외부 stable publication은 Desktop Node 내부 서비스 완료 기준과 분리한다.

현재 충족 또는 유지 중인 gate:

- Phase 18 manifest-first safe update/rollback/config migration 기본 구현과 관리자 update/rollback smoke는 충족 gate로 본다.
- Phase 22 release channel/version/artifact naming policy와 installer output/provenance contract는 일부 강제됐고 ADR-0002로 현재 적용 결정에 채택됐다.
- Post-reboot verification 도구는 dry-run state/task plan과 runner evidence artifact 계약, 성공 후 continuation profile 실행을 지원한다. 실제 Task Scheduler 등록은 명시 opt-in이며, 자동 reboot 실행은 금지한다.
- Phase 24 후보는 Local API job runtime public boundary를 안정화해 PowerShell 유지와 C++23 전환 판단을 분리했다. 현재 제품 런타임 판단은 ADR-0004가 소유한다.
- Phase 25 후보는 .NET contract/runtime core, TypeScript Web Console, PowerShell adapter 전환 경계를 정의한다.

Phase 25 현재 구현 상태:

- `DesktopNode.Host.exe`는 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner다.
- .NET request processor는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation routes, checkpoint create/restore/delete native mutation routes, job get/cancel/retry, JSON job store save/load/recovery를 처리한다. Current served Hyper-V mutation route contract는 `dotnet-native` product path이며 PowerShell helper fallback을 사용하지 않는다.
- `DesktopNode.Host.exe listen`은 active product runtime에서 Hyper-V PowerShell helper script path를 받지 않는다. `--helper-script`는 retired option이며 product manifest의 `paths`에도 `helper_script`/`api_script`를 기록하지 않는다. Legacy WinSW PowerShell Local API generation은 retired error를 반환해야 한다.
- `host.status`는 C# registry/WMI/service/admin read-only adapter로 전환됐다.
- `network.inventory`는 C# WMI read-only adapter가 직접 처리하며 switch type, `allow_management_os`, external adapter field parity가 불완전하면 PowerShell helper fallback 없이 native structured failure를 반환한다.
- `vm.list`는 C# WMI read-only adapter가 직접 처리한다. Empty inventory는 유효한 success이며, VM identity/state, CPU/startup memory/generation/checkpoint count, storage/network field parity가 불완전하면 PowerShell helper fallback 없이 native structured failure를 반환한다.
- `GET /api/v1/vms/{id}`는 native `vm.list` result를 사용한다. Native inventory에서 VM이 없으면 helper 재시도 없이 `PCV_VM_NOT_FOUND`를 반환하고, native inventory failure도 helper 재시도 없이 반환한다.
- `GET /api/v1/vms/{id}/checkpoints`는 native VM inventory로 VM 존재를 확인하고 WMI snapshot association으로 checkpoint list를 읽는다. Native inventory가 구조적으로 실패하거나 VM/checkpoint parity가 불완전하면 helper 재시도 없이 native structured failure를 반환한다. Empty checkpoint list는 유효한 success다.
- `POST /api/v1/vms/{id}/start`, `POST /api/v1/vms/{id}/shutdown`, `POST /api/v1/vms/{id}/poweroff`, `POST /api/v1/vms/{id}/restart`는 C# WMI `Msvm_ComputerSystem.RequestStateChange` adapter가 직접 실행하며 PowerShell helper fallback 없이 structured success/failure를 반환한다.
- `POST /api/v1/vms`는 native VM create adapter가 처리한다. 이번 native product path는 Hyper-V Generation 2 create만 지원하고 Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다.
- `POST /api/v1/vms/{id}/checkpoints`, `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore`, `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}`는 C# WMI snapshot service adapter가 직접 실행하며 PowerShell helper fallback 없이 structured success/failure를 반환한다.
- Runtime policy는 `helper_boundary=dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation`, `native_core.reason=host.status,network.inventory,vm.list,checkpoint.list,vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete`, `native_mutation_operations=[vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete]`, `mutation_dispatch=native-vm-create-lifecycle-delete-checkpoint-mutation`을 보고한다.
- `DELETE /api/v1/vms/{id}`는 .NET request processor queue를 거쳐 C# WMI `DestroySystem` adapter로 실행한다. Missing VM은 idempotent `action=absent` success이며, `managed-by=purecvisor-desktop-node` marker가 없는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다. 현재 evidence는 xUnit code-level과 `0.30.1-admin-smoke` installed destructive smoke다. Installed smoke는 managed VM delete `action=delete`, repeat delete `action=absent`, unmanaged VM delete block `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, cleanup proof, boot time unchanged를 확인했다.
- Job store create snapshot save는 final과 같은 directory의 `jobs.json.tmp.<GUID-N>` candidate를 exclusive write하고 `FileStream.Flush(true)`한 뒤, candidate/previous SHA-256·length를 담은 `jobs.json.commit-pending.tmp.<GUID-N>`를 동일하게 flush해 fixed `jobs.json.commit-pending`으로 publish한 다음 primary를 replace/move한다. Primary exact identity로 `Committed`/`NotCommitted`/`Indeterminate`를 판정하며 `Committed`만 memory/queue/HTTP success를 publish한다. Unresolved fixed marker와 primary access failure는 current runtime mutation/dispatch를 fail-closed하고 stale temp/backup을 자동 promote하지 않는다. Legacy `jobs.json.tmp`와 GUID-owned orphan temp cleanup은 best-effort이고 authoritative가 아니다. 일반 runtime save는 schema migration apply evidence가 아니며, migration apply evidence는 `DesktopNode.Host.exe service-action job-store-migration-apply`와 그 검증 결과로만 기록한다. 이 경계는 product single SCM Runtime owner를 가정하며 lifetime path lease/CAS, actual frozen 0.42.65 reader, power-loss/exactly-once는 아직 검증하지 않았다.
- Unsupported future job store schema는 quarantine/move 없이 409 blocked diagnostics/no-mutation contract로 처리한다. Schema v2는 `job-store-v1-to-v2` migration target으로 지원하며, v99 같은 더 새로운 future schema는 계속 blocked/no-mutation으로 처리한다.
- `product config migration apply`와 `job store migration apply`는 2026-05-06 code-level product operation으로 구현됐고, 2026-05-07 `0.38.6-admin-smoke` 설치본 destructive admin smoke를 통과했다. Config apply는 `product-config-v1-to-v2` plan/version 1, owned product manifest schema v1, service stopped proof를 요구하고 backup + same-directory temp replace + rollback diagnostics를 반환한다. Job store apply는 `job-store-v1-to-v2` plan/version 1, owned `jobs.json` schema v1, service stopped/runtime writer stopped proof와 fixed pending marker absence를 요구하고 backup + same-directory temp replace + recovery diagnostics를 반환한다. Marker가 있거나 검사할 수 없으면 backup/rewrite 전에 fail-closed한다. 두 action 모두 implicit service stop/start, token mutation, service identity mutation, MSI/update/rollback, Hyper-V/firewall/trust-store/LAN/Event Log mutation은 수행하지 않는다. `artifacts/config-jobstore-migration-apply-installed-20260507-0386` evidence는 MSI SHA-256 `d252110bee12e8c5c129b97474e2e08a51941d79d81d460fd6fe45932b290593`, provenance commit `d4259670e0aa90dae869bbd0e35c8910033fb59e`, signing mode `AllowUnsignedDev`, final service `Running`, manifest/job store schema `2`, boot time unchanged, post-migration API read ok를 확인했다. 이 predecessor installed evidence는 새 pending-marker guard를 실행한 evidence가 아니며, 현재 guard는 code-level `4d3a0d9` 검증이다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- `DesktopNode.Host.exe service-action configure-installed|repair-installed|remove-installed|data-root-remove`는 native SCM/data-root action path로 전환됐고, protected token bootstrap ACL hardening은 C# ACL API를 사용한다. `remove-installed --remove-data`는 handoff descriptor만 반환하고 실제 data-root deletion은 service absent + `--remove-data` opt-in을 요구하는 `data-root-remove`에서만 수행한다. `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`의 `0.30.3-admin-smoke`는 installed destructive service create/configure/repair/delete/remove-data, MSI `REMOVE_DATA=1`, installed Hyper-V route smoke를 자동 reboot 없이 통과했다.
- Web Console browser fixture parity는 TypeScript build output인 served `web/app.js`를 Node `vm` 최소 DOM과 fixture Local API 응답으로 실행해 dashboard/VM/job 렌더링을 확인한다. 이 검증은 Playwright, dev server, 실제 Local API 실행, Hyper-V/MSI/service mutation을 요구하지 않는다.

주요 evidence 위치:

- `artifacts/dotnet-host-admin-smoke-20260501-213444`: direct service-action, MSI lifecycle, Hyper-V helper integration smoke.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729`: 설치본 service-action, MSI lifecycle, Hyper-V API route smoke.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`: `0.26.8-admin-smoke` native `network.inventory` 포함 설치본 smoke.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`: `0.26.9-admin-smoke` MSI repair 재생성, native inventory fallback guard, request processor 직렬화 포함 설치본 smoke.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`: `0.27.1-admin-smoke` native `host.status` 포함 설치본 smoke. Installed `host.status`는 Windows 10 Pro for Workstations `25H2`, `supported=true`, admin elevated, Hyper-V enabled, VMMS running, Default Switch present를 반환했다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-113517`: `0.27.3-admin-smoke` native `vm.list` WMI query guard 수정 후 설치본 service-action, MSI lifecycle, Hyper-V API route smoke.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-115135`: `0.27.4-admin-smoke` VM detail native-first slice 이후 설치본 service-action, MSI lifecycle, Hyper-V API route smoke.
- `artifacts/installed-nonmutating-checkpoint-list-20260503-121824`: `0.27.5-admin-smoke` checkpoint list native-first 설치본 GET-only smoke.
- `artifacts/installed-vm-create-checkpoint-list-20260503-122705`, `artifacts/installed-checkpoint-lifecycle-cleanup-20260503-124330`: 사용자 explicit opt-in 범위의 VM create, checkpoint create/delete, VM poweroff/delete cleanup smoke.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-140824`: `0.27.6-admin-smoke` runtime dispatch boundary contract 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. Commit `3178a62bcf22d00977bf564063befa8e2b2562a5`, MSI SHA-256 `4485fc3aba902d38a5d1293e9231497ae5f35b4c0730d1815c8df561a67c009c`, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283`: `0.28.3-admin-smoke` checkpoint create/delete native mutation adapter 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. Installed `checkpoint.create` job result는 기존 helper parity와 같은 `{ vm_name, name }` payload를 반환했고 `checkpoint.delete`는 `{ vm_name, name, action=delete }` payload를 반환했다. Runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.delete]`를 보고했다. Final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/routeparity-service-msi-hyperv-restore-mutation-20260503-0286`: `0.28.6-admin-smoke` checkpoint restore native mutation adapter 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. Installed `checkpoint.restore` job result는 `vm.poweroff-before-restore` 최소 안정 조건에서 `{ vm_name, name, action=restore }` payload를 반환했고 `checkpoint.delete` cleanup도 성공했다. Runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`를 보고했다. MSI SHA-256은 `1c14c6ceadde8f1cea2189f1942e913c457524a4aeb10995472126ad560b8d0b`, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/routeparity-service-msi-hyperv-vm-create-restart-shutdown-20260503-0290`: `0.29.0-admin-smoke` VM create/start/restart/poweroff와 checkpoint create/restore/delete native mutation adapter 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. Final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301`: `0.30.1-admin-smoke` VM delete native mutation adapter 포함 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. Managed VM delete는 `action=delete`, repeat delete는 `action=absent`, unmanaged VM delete는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 blocked, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/service-action-status-start-stop-20260504-002359`: installed `DesktopNode.Host.exe service-action status/stop/start/status` native SCM smoke. Service owner verified, stopped/running state observation, restart 후 runtime policy health `200`, final service `Running`, boot time unchanged.
- `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`: `0.30.3-admin-smoke` installed service/data-root lifecycle smoke. Service 존재 중 `data-root-remove --remove-data`는 `PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS`로 차단, `remove-installed --remove-data`는 handoff-only, service absent 이후 `data-root-remove --remove-data`는 allowlist data-root 항목만 삭제, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1412-0310`: `0.31.0-admin-smoke` repo migration active path removal 이후 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. MSI build는 product-owned payload staging을 사용했고 build commit은 `ac26cf9a8b355de4984536b3bb5492979719f6b7`이다. Service/data-root handoff, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, managed VM delete `action=delete`, repeat delete `action=absent`, unmanaged guard `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1515-0320`: `0.32.0-admin-smoke` standalone product asset boundary 이후 설치본 service-action, MSI lifecycle, Hyper-V API route smoke. MSI build는 repo-root `web/**` product asset만 포함했고 build commit은 `d852ff54bafb403e16e86057b3cecec2813bf0b6`, MSI SHA-256은 `f3e4456e94d5ee16a8e0bd6d02d17ac04d682be5bd58c77098072f97711d25f5`, payload file count는 7이다. MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, managed VM delete `action=delete`, repeat delete `action=absent`, unmanaged guard `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `artifacts/eventlog-source-registration-20260504-actual-registry`: `DesktopNode.Host.exe service-action eventlog-register` 실제 Event Log source registry 등록 evidence. `Application` log의 `PureCVisor Desktop Node` source는 `EventMessageFile=C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`, `TypesSupported=7`로 등록됐고 final service `Running`, `pcv-spike-*` VM 잔여물 없음. Service/MSI/Hyper-V/firewall/trust-store mutation은 실행하지 않았다.
- `artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330`: `0.33.0-admin-smoke` Service/MSI/Hyper-V mutation과 row-isolated firewall/trust-store mutation evidence. MSI build commit은 `dca492c67c0cb3843832d5f6e1e76c8d686c3cdf`, MSI SHA-256은 `e6522114963be755beab1f54e183eef212a9f32979751e1fe67159a20cd2a4ff`, payload file count는 7이다. Service-action, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, managed VM delete `action=delete`, repeat delete `action=absent`, unmanaged guard `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음. Firewall-only smoke는 owned inbound allow rule을 create/enable/remove 후 final rule count 0을 확인했고, trust-store-only smoke는 self-signed test cert를 LocalMachine Root/TrustedPublisher에 import한 뒤 Root/TrustedPublisher/CurrentUser My final absence를 확인했다.
- 2026-05-04 firewall/trust-store owner migration slice는 `DesktopNode.Host.exe service-action firewall-enable|firewall-remove|trust-store-install|trust-store-remove` code-level native action과 fake controller/xUnit evidence를 추가했다. 실제 firewall/trust-store mutation은 실행하지 않았고, release/LAN gate는 별도 승인 상태로 유지한다.
- `artifacts/os-mutation-gates-20260505-003459-0341`: `0.34.1-admin-smoke` current native OS gate evidence. MSI provenance commit은 `6f97a24aa2bdfacf33d7bd987559eb85e363e119`, follow-up hardening commit은 `49a06acd3493066a10ec26fe541d5d8be1005c2b`, MSI SHA-256은 `550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c`이다. MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, native firewall rule enable/remove, LAN IP runtime policy `HTTP 200`, internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service `Running`, final firewall rule absent, final internal trust cert present, boot time unchanged다.
- `artifacts/os-mutation-gates-20260505-033503-0354`: `0.35.4-admin-smoke` 실행 당시 HEAD native OS gate fresh evidence. MSI provenance commit은 `744a15536569e89f948927bea9179fc0eeae3ff4`, MSI SHA-256은 `bf7d0d2bd83545e83fbdf0dfb96b715f8e09471474445ae1c0db1d076be2c1e4`이다. MSI install/repair/uninstall preserve/reinstall/`REMOVE_DATA=1` uninstall, native firewall rule enable/remove, LAN IP runtime policy와 Web root `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final restore는 internal signed stable `0.35.2`, final service loopback `Running`, final firewall rule count `0`, final internal trust cert present, installed DisplayVersion `0.35.2`, boot time unchanged다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`와 `artifacts/os-mutation-gates-20260505-101659-0355-final`: `0.35.5-admin-smoke` 실행 당시 HEAD native OS gate fresh evidence. MSI provenance commit은 `2fb38f20a8c74433684345ded8a33ba16a863621`, MSI SHA-256은 `ade2e5ea054c9a77c893fcea36dc91535aef5bab0a8fbef8b61158be26ffa046`이다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, native firewall rule enable/remove, LAN IP runtime policy와 Web assets `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service loopback `Running`, final firewall rule count `0`, final Event Log source absent, final internal trust cert present, installed DisplayVersion `0.35.5`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. `product config migration apply`와 `job store migration apply`는 `future-route/not-implemented/blocked`라 실행하지 않았다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun`와 `artifacts/os-mutation-gates-20260505-170454-0356-rerun`: `0.35.6-admin-smoke` 실행 당시 code HEAD native OS gate fresh evidence. MSI provenance commit은 `cc723e28ed62f6f1c5e49c74ca68b87d0f1b8b3a`, MSI SHA-256은 `a24de44049519dea8405854a17272ebb362b061ff03a051cd61fb31669bc7d02`이다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, native firewall rule enable/remove, LAN IP `http://[redacted-private-endpoint]:7777/` runtime policy와 Web assets `HTTP 200`, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service loopback `Running`, final firewall rule count `0`, final Event Log source absent, final internal trust cert present, installed DisplayVersion `0.35.6`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. `product config migration apply`와 `job store migration apply`는 `future-route/not-implemented/blocked`라 실행하지 않았다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`: `0.35.7-admin-smoke` 현재 HEAD native OS gate fresh evidence. MSI provenance commit은 `2ec9e71d45b702e106824c86500cd6152b18fab7`, MSI SHA-256은 `9bd23cb0bd4cfd70bcd406160e3948e830a8ae7bbcdcf7ca255e2745ce23859f`이다. MSI lifecycle, service/data-root handoff, installed Hyper-V route smoke, Event Log register/remove, native firewall rule enable/remove, LAN IP `http://[redacted-private-endpoint]:7777/` bearer runtime policy와 Web assets `HTTP 200`, config-migration-apply blocked/no-mutation descriptor, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였고 final service loopback `Running`, final firewall rule count `0`, final Event Log source absent, final internal trust cert present, installed DisplayVersion `0.35.7`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. `job store migration apply`는 `future-route/not-implemented/blocked`라 실행하지 않았다.
- `artifacts/routeparity-service-msi-hyperv-dotnet100-20260505-0.36.0`: `0.36.0-admin-smoke` active product .NET 100% cleanup Service/MSI/Hyper-V route parity rerun evidence. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `70cb8b720588c6ef69aca59fed48f870865d7bca8c7a4ea8e623ab6b6e99d048`이다. Service-action, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, installed Hyper-V API route smoke가 PASS였고 final service loopback `Running`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다.
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, step `timed_out=false`, `exit_code=0`, heartbeat lines `25`다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`, signing mode는 `AllowUnsignedDev`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`: `0.37.0-admin-smoke` full admin host mutation gate PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, timeout false다. MSI provenance commit은 `485b1a7338fb2b682c3964c858ccc13c322950d7`, MSI SHA-256은 `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.37.0`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`다. First-attempt MSI repair `1603`은 recovered transient evidence다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`: `0.38.4-rc.1` internal enterprise `RequireSigned` MSI build PASS. MSI SHA-256은 `0b4c60d60098f89bd0adea4d183a5224d32b862e9bf69bd6dbaa41077377e8b9`, provenance commit은 `6bbb39f0a3a271e4a1187ce7de2014e009977425`, signing trust model은 `InternalEnterprise`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation`은 `0.41.5-admin-smoke` 이전 full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`이고 MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, signing mode는 `AllowUnsignedDev`다. Service/MSI/Hyper-V route smoke와 firewall/LAN/Event Log/internal trust-store OS mutation gate가 PASS였고, final service `Running`, installed manifest `0.41.5-admin-smoke`, Web Console `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`, Web API `http://127.0.0.1:7777/api/v1/runtime/policy` unauthenticated boundary `401`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`였다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0412-hostmutation`은 `0.41.2-admin-smoke` historical full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`이고 MSI SHA-256은 `ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0`, provenance commit은 `d098f0fc631ff1799d7dd238a84e896fe8616230`, signing mode는 `AllowUnsignedDev`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 2026-05-10 `full-admin-host-mutation-gate-2026-05-10-0410-account-rerun`은 `0.41.0-admin-smoke` account-linked full admin host mutation PASS evidence로 보존한다. Artifact는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`이고 MSI SHA-256은 `cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d`, provenance commit은 `a3226ef637ea895d2f2a9956599e0d5e79d00410`, signing mode는 `AllowUnsignedDev`다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 login/session/RBAC/console `200`, restore/ACL restored를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`: `0.39.0-admin-smoke` MSI/service installed listener rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false다. MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode는 `AllowUnsignedDev`다. Final service는 loopback-only `Running`, product manifest version은 `0.39.0-admin-smoke`, SCM `PathName`은 diagnostic bundle/hardening 인자를 포함했고 protected-token diagnostic bundle create/download POST `201`, GET `200`, redaction PASS를 확인했다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`: `0.39.0-admin-smoke` installed listener 후속 OS mutation gate PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false다. OS summary는 `host_mutation_performed=true`, `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`이며 firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore를 확인했다. Final service는 `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`: `0.38.7-rc.1` internal enterprise `RequireSigned` MSI build PASS. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260507-0387`와 `artifacts/product-update-rollback-mutation-20260507-0387`: `0.38.7-admin-smoke` full admin host mutation gate 및 update/rollback mutation attempt는 non-elevated shell에서 차단됐다. Batch Supervisor는 `PCV_BATCH_ADMIN_REQUIRED`로 step 시작 전 blocked, update는 `sc.exe stop PureCVisorDesktopNode` exit `5`, rollback은 `PCV_PRODUCT_SERVICE_STOP_TIMEOUT`이었고 각 summary의 `host_mutation_performed=false`다. Product root manifest는 `0.38.6-admin-smoke`, previous root는 absent, final service는 `Running`이다. 이 항목은 blocked evidence이며 PASS evidence가 아니다.
- `artifacts/config-jobstore-migration-apply-installed-20260507-0386`: `0.38.6-admin-smoke` focused config/job store migration apply installed destructive admin smoke PASS. MSI provenance commit은 `d4259670e0aa90dae869bbd0e35c8910033fb59e`, MSI SHA-256은 `d252110bee12e8c5c129b97474e2e08a51941d79d81d460fd6fe45932b290593`, signing mode는 `AllowUnsignedDev`다. Product manifest schema `1 -> 2`, job store schema `1 -> 2`, backup write, same-directory temp replace, temp cleanup, final service `Running`, boot time unchanged, post-migration API read ok를 확인했다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`: `0.38.8-admin-smoke` installed destructive update/rollback smoke PASS. MSI provenance commit은 `fd4f854646fc159d54f7578230f00c51f80e201f`, MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`, signing mode는 `AllowUnsignedDev`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`로 성공했고 health `200`, update journal `succeeded/health`를 확인했다. Rollback은 current product root를 `0.38.6-admin-smoke`로 복원하고 `0.38.8-admin-smoke` root를 `DesktopNode.failed` diagnostics로 보존했으며 final service `Running`, boot time unchanged, `host_mutation_performed=true`다. 최초 `artifacts/product-update-rollback-mutation-20260507-0388` non-elevated attempt는 blocked history로만 남긴다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`: GA-ready aggregate gate closure는 closed다. 2026-05-05 당시 재계산 값은 GA-scope blocked row 0개, PowerShell-backed current owner 0개, active product `spikes/**` reference 0개, future implementation exclusion 2개였고 tier2/tier3 full fresh evidence와 internal stable release/update/rollback execution evidence는 pass였다. 2026-05-07 `0.38.6-admin-smoke` 이후 현재 route matrix 재계산 값은 GA-scope `current-route` 18개, `product-operation` 24개, `future-route` exclusion 0개, `current-native` 42개, blocked/ga-ready-candidate 0개다. 이 closure는 ADR-0004 current decision 승격 근거이며 public trusted signing claim이 아니다.
- `docs/ga-ready/evidence/repo-migration-preflight-2026-05-04.md`: active `spikes/**` migration은 pass다. 2026-05-05 physical archive move 이후 source path는 absent이고 `archive/spikes/purecvisor-desktop-node/**` file count는 46개다. MSI installer payload의 active spike source count는 0이다.
- `docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md`: public trusted signing은 제외하고 release/LAN/OS gated operation의 preapproval boundary를 기록했다. 후속 사용자 opt-in으로 Event Log source registration/removal, firewall-only, trust-store-only historical mutation, `0.34.1-admin-smoke`, `0.35.4-admin-smoke`, `0.35.5-admin-smoke` native LAN/firewall/internal trust-store evidence, internal stable release/update/rollback evidence를 별도 artifact와 ledger/closure에 추가했다. Public trusted signing과 외부 stable publication은 계속 excluded/not-claimed이며, ADR-0004는 내부 전용 서비스 current decision으로 적용됐다.
- Installer-ISO VM의 `vm.shutdown`은 guest shutdown integration 미준비 상태를 `PCV_VM_SHUTDOWN_NOT_AVAILABLE` structured failure로 반환했다.
- Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD 기반 Gen1 differencing VM으로 확인했다. Installed Local API `vm.shutdown` job은 `succeeded`, final VM state는 `Off`, smoke VM/ProgramData cleanup은 완료 상태다.
- 2026-05-03 VM summary/storage/network native parity code-level slices는 WMI CPU/startup memory/generation/checkpoint count, storage path, network switch mapping을 추가한다. 이 evidence는 code-level verification이며 installed non-mutating rerun 전에는 GA-ready gate closure 근거로 사용하지 않는다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`: internal Root/leaf 기반 `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS.

위 evidence 중 `AllowUnsignedDev` admin-smoke와 scoped test certificate trust-store mutation은 public trusted signing 또는 외부 stable publication을 주장하지 않는다. Internal trusted signing evidence도 public trusted signature나 외부 stable publication을 의미하지 않는다. Phase 20-23 문서는 각 gate의 증거 수집 기준과 runbook을 정의하며, 실제 host mutation 결과는 각 Phase plan의 완료 증거에만 기록한다.
