# Desktop Node 개발자 문서 인덱스

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

## 2026-08-25 Required CI cutover closure

- Required CI는 정확히 `dotnet`, `web`, `delivery`, `installer-policy` 네 context다.
- `DesktopNode.Verification` migration ledger의 62 files / 627 contracts는 모두
  `cutover / local pass / CI pass`이며 Required CI의 Pester 및 비관리자 PowerShell process
  invocation은 각각 `0`이다.
- PR #1 cutover merge `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`과 Development Gates
  run `32901477892`, PR #2 documentation closure main
  `6e2bdb93ce308b632c929e2c17f5550ac3845401`과 run `32904006595`가 PASS했다.
- 비필수 `.github/workflows/public-boundary.yml`, legacy Pester source와 manual/admin
  PowerShell은 residue로 남는다. 이는 repository-wide PowerShell zero 주장이 아니다.
- 단일 전환 증빙은
  `docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md`가 소유한다.

## 2026-08-25 public source authority bootstrap

- Public source authority and rights/security boundaries are `docs/PUBLIC_SOURCE_AUTHORITY.md`, `LICENSE`,
  `SECURITY.md`, and `docs/PUBLIC_RELEASE_BOUNDARY.md`.
- The completed authority delta is
  `docs/superpowers/specs/2026-08-25-purecvisor-desktop-node-public-authority-snapshot-delta-design.md`;
  bootstrap/cutover execution record is
  `docs/superpowers/plans/2026-08-25-purecvisor-desktop-node-public-baseline-and-protection.md`.
- Repository-owned safety commands are `npm run test:public-source-safety --prefix web` and
  `npm run verify:public-source-safety --prefix web`. They use Node argument arrays and do not require a
  PowerShell-language runner.
- Parentless export 전 로컬 source gate의 실측 결과는
  `docs/ga-ready/evidence/public-authority-bootstrap-2026-08-25.md`가 소유한다.
- Public source does not imply a trusted package or release. Current operational and promotion claims remain
  unchanged.

아래 dated section의 버전·수치는 각 작성 시점 snapshot이다. 현재 operational current는 위
생성 블록과 `docs/ga-ready/current-evidence.json`이 소유한다.

## 2026-08-24 Web verification Wave B local parity

- 승인 설계와 실행 계획은 각각
  `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b-design.md`,
  `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b.md`가
  소유한다.
- 62-file migration ledger와 strict schema는
  `config/development-verification-migration-manifest.json`,
  `config/development-verification-migration-manifest.schema.json`이다. Web 한 행은 `mapped` / local
  `pass` / CI `pending`이고, 나머지 61행은 아직 `unmapped` / local·CI `pending`이다.
- Clean input `20ba3b80c211cc6a29bc9ecaf7e9195911678f14`의 legacy Pester `50/50`, Node
  positive `50/50`, controlled negative parity, mapping hash와 명령별 실측은
  `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`가 소유한다.
- 별도 진입점은 `npm run test:web-contracts --prefix web`과
  `npm run verify:web-contract-negative-parity --prefix web`이다. 기존 `npm test`,
  `verify:parity`, legacy Web Pester와 required CI는 바뀌지 않았으며 CI parity와 cutover는
  pending이다. Operational current `0.42.74-admin-smoke`와 saved-lifecycle actual-VM blocker도
  변경하지 않았다.

## 2026-08-24 C# verification Wave A foundation

- 승인 설계는
  `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design.md`,
  Wave A 구현 계획은
  `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-wave-a.md`,
  versioned catalog와 schema는 `config/development-verification-suites.json` /
  `config/development-verification-suites.schema.json`, 실행 정책은
  `docs/DEVELOPMENT_VERIFICATION_POLICY.md`의 `2026-08-24 C# verification Wave A foundation`,
  code-level 기록은
  `docs/ga-ready/evidence/pester-free-csharp-verification-wave-a-foundation-2026-08-24.md`가
  소유한다. 이 진입점은 `activation_state=plan-only-foundation`이고
  `required_ci_pester_zero=false`, `required_ci_nonadmin_powershell_zero=false`이므로 Wave E
  cutover 전의 required 실행 경로를 대체하지 않는다.

## 2026-08-14 서비스 기획 baseline

- 서비스 정체성, 04273 완결도, Workstation 26H1 대조, 따라갈 기능/따라가지 않을 기능은
  `docs/SERVICE_PLAN.md`가 소유한다. Document-ID는
  `purecvisor-desktop-node-service-plan-v1`이다.
- 이 기획은 `0.42.74`와 다음 package-pair를 열지 않는다. 다음 trigger는
  `product-payload-change-after-04273`이다.
- P0 실행 순서는
  `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md`가
  소유한다. P0-1 설계는
  `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-media-attach-design.md`다.
  P0-1 code-level 기록은
  `docs/ga-ready/evidence/service-plan-p0-media-attach-code-level-2026-08-14.md`다.
  P0-3 설계는
  `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-hyperv-saved-design.md`다.
  P0-3 code-level 기록은
  `docs/ga-ready/evidence/service-plan-p0-hyperv-saved-code-level-2026-08-14.md`다.
  P0-4 설계는
  `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-managed-import-design.md`다.
  P0-4 code-level 기록은
  `docs/ga-ready/evidence/service-plan-p0-managed-import-code-level-2026-08-14.md`다.
  이 문서 전용 편집은 product payload가 아니며 `0.42.74`를 열지 않는다.
  P1-5 managed full clone 설계는
  `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone-design.md`이며
  구현 계획은
  `docs/superpowers/plans/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone.md`다.
  그 설계·계획 자체는 `0.42.76`을 열지 않는다.
- 2026-07-16 구현 평가는 `0.42.65` 기준 predecessor다. Web 기본 경로 미완은 04273
  loopback session으로 닫힌 것으로 해석한다.

## 2026-08-13 loopback bootstrap browser gate

- 설계는
  `docs/superpowers/specs/2026-08-13-purecvisor-desktop-node-loopback-bootstrap-browser-gate-design.md`가
  소유한다. Playwright를 required dependency로 추가하지 않는다.
- required 게이트는 in-process Host + 로컬 Edge/Chrome CDP다.
  `DesktopNodeHostLoopbackBootstrapBrowserTests`가 `dotnet test`에 포함된다.
- 설치본 listener Chromium 재실행과 package campaign은 이 slice의 required 조건이 아니다.

## 2026-08-13 Web loopback session bootstrap 설계

- 설치 직후 Web 인증 경로 설계는
  `docs/superpowers/specs/2026-08-13-purecvisor-desktop-node-web-loopback-session-bootstrap-design.md`가
  소유한다. 승인 locator는 `User-Approval: web-loopback-session-bootstrap-20260813`이다.
- 채택안은 loopback-only `POST /api/v1/auth/loopback-session`이다. service token을
  HTML/`pcv-config.js`에 넣지 않고, 기본 `no-default-account` 설치에서 짧은 JWT를 발급한다.
- 구현 계획은
  `docs/superpowers/plans/2026-08-13-purecvisor-desktop-node-web-loopback-session-bootstrap.md`가
  소유한다. Playwright 설치본 E2E와 `0.42.73` package campaign은 설계 범위 밖이다.
- 2026-08-13 code-level 구현 기록은
  `docs/ga-ready/evidence/web-loopback-session-bootstrap-code-level-2026-08-13.md`이며,
  구현 소유자는 위 계획이다. operational current는 바꾸지 않는다.

## 2026-08-07 후속 작업 계획

- 미해결 항목의 착수 계획은 `docs/followup-work-plan-2026-08-07.md`가 소유한다. `6`개 항목마다
  실측한 현재 상태, 착수 조건, 완료 조건, 함정을 갖는다. 착수 우선순위는 §0에 있다.
- 가장 싼 항목은 §2(wave 1 소유자 helper 사본 `11`개 제거)다. 2026-08-06 기록이 "대조가
  선행돼야 한다"고 남긴 차단 사유는 **실재하지 않았고**, 2026-08-07 대조 결과 실제로 확인이
  필요한 항목은 `EmptyObject` `1`개뿐이다.
- 가장 큰 항목은 §1(`web/src/served-app.ts` `4,005`줄 분해)이다. `build-served-asset.mjs`의
  `servedSourceParts` 배열이 이미 분해 seam이므로 번들러 도입이 필요 없고, 산출물 `web/app.js`를
  diff해 순수 이동을 기계로 증명할 수 있다.
- §3(`ServiceTokenRotationRevoke` 간헐 실패)은 **착수할 수 없는 상태**다. 착수 조건은 재현 관측
  `1`회이며, 그 전에 재시도나 `Skip`을 넣는 것은 증상 처리다.

## 2026-08-06 `0.42.70` 버전 일원화, 후속 트랙, 코어 대형 모듈 분해

- 하루치 작업의 서술 기록은 `docs/followup-work-record-2026-08-06.md`가 소유한다. §1~§7은
  manual-admin `0.42.69 -> 0.42.70` closure와 anchor 승격, §8은 위생·이월 재검증·FC 트랙,
  §9는 코어 대형 모듈 분해다. 각 절은 앞 절의 snapshot을 수정하지 않고 뒤에 덧붙인다.
- 2026-08-05 감사의 잔존 항목 처리는 `docs/project-status-audit-2026-08-05.md` §15(후속 작업
  closure)와 §16(P2-2 대형 모듈 분해)이 소유한다. 감사 §11 표의 `OPEN` 판정 중 `#7`(라인 수 gate)은
  작성 시점에 이미 낡은 값이었고 `#9`(FC 검증 환경)는 자산이 실제로 호스트에 있었다.
- `DesktopNodeHostServiceAction` 분해: `4,069` → `1,174`줄(`71`% 감소), `*ForOps` forwarder `9` →
  `0`개. 계획은
  `docs/superpowers/plans/2026-08-06-purecvisor-desktop-node-host-service-action-decomposition.md`,
  evidence는 `docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`가 소유한다.
  남은 대형 모듈은 `DesktopNodeApiRequestProcessor.cs`(`3,367`)와 `web/src/served-app.ts`(`4,005`)이며
  각각 별도 계획이 필요하다.
- 대형 모듈 상한은 `packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`과
  `fixtures/module-size-ratchet.json`이 단일 gate로 소유한다. 라쳇은 한 방향으로만 움직이므로
  순증은 CI에서 실패한다. 같은 날 잠시 존재했던 중복 gate `PcvLargeModuleLineCeiling`은 제거됐다.
- 이월 3항목(QoS 변환, disk shrink guard, expansion) 재검증은
  `docs/ga-ready/evidence/functional-correctness-carry-forward-revalidation-2026-08-06-04270.md`,
  FC-05/FC-12(b)/FC-13 검증은
  `docs/ga-ready/evidence/fc-05-fc-12b-fc-13-verification-2026-08-06-04270.md`가 소유한다. 후자는
  `PARTIAL_PASS`이며 FC-12(b) guest 측 비 ASCII 왕복은 미확정으로 남는다.
- 이 날의 문서 정정 2건: 감사 §P2-2의 라인 수 표는 **옳았고** 이를 "재현 불가"로 판정한 측정
  명령(`Measure-Object -Line`이 빈 줄을 세지 않음)이 틀렸다. 솔루션 테스트 `787`건 기록도 잘린
  출력 창을 읽은 오독이며 실제는 `825`건이다. 두 정정 모두
  `docs/followup-work-record-2026-08-06.md` §8.9가 소유한다.
- 문서 최신화 작업 자체의 기록은 `docs/followup-work-record-2026-08-06.md` §10이 소유한다.
  이 문서가 current-evidence 생성기 target에 편입된 경위를 함께 기록한다.
- FC-12(b) guest 측은 §13에서 **닫혔다**. 원인은 인코딩이 아니라 argv 전달이었다. bridge가
  argv를 공백으로 이어붙여 guest에서 재파싱해, 공백이 든 인자는 쪼개지고 `$(...)`/`;`가 든
  인자는 실행됐다. PCVCLI가 문서화한 `-- <command>` argv 계약을 복원했고 실제 guest에서 비 ASCII
  표본이 기대 UTF-8 길이(`31` bytes)로 돌아오는 것을 확인했다. Evidence는
  `docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`가 소유한다.
  설치본은 바꾸지 않았으므로 `0.42.70-admin-smoke`에는 아직 수정 전 코드가 있다.
- `ServiceTokenRotationRevoke...RedactedAudit` 간헐 실패 조사는 §12가 소유한다. `82`회 재현
  시도가 전부 실패했고 근본 원인은 **미확정**이다. 유력 가설(`File.Replace`의 sharing violation)은
  실측으로 반증했다. 실제 스캐너가 쓰는 `ReadWrite|Delete` share 모드에서 살아남는 것은
  `File.Replace`뿐이고 `File.Move(overwrite)`는 `60/60` 실패한다. "일관성을 위해 `File.Move`로
  통일"하는 수정은 코드를 더 잘 깨지게 만들었을 것이다. 제품 코드는 고치지 않았다.
- `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`는 `69`일 stale이었고 §11에서
  `0.42.69 -> 0.42.70`으로 갱신했다. 기존 assertion이 이를 못 잡은 이유는 전부 anchor 없는
  substring 매칭이어서 `previous_*` 접두사가 붙은 과거 값에 매치됐기 때문이다. 줄 시작 고정으로
  canonical JSON과 대조하는 `PcvManualAdminDescriptorCurrency.Tests.ps1`을 추가했다. 이 descriptor의
  접두사 규약(`current_*` / `latest_*` / `next_*` / `previous_<버전>_*`)은 문서 상단 "읽는 법"이
  소유한다.

## 2026-08-05 프로젝트 진행 상황 감사와 Web Console 상태 진실성

- 2026-07-13 감사 이후 23일간의 진행 상황 전수 조사는 `docs/project-status-audit-2026-08-05.md`가
  소유한다. 2026-07-13 감사의 P1 두 항목(비상승 테스트 hermeticity, CI 범위)이 닫혔고
  2026-07-15 CONFIRMED 기능 결함 FC-01/02/04/16/18이 전건 수정됐음을 소스 대조로 확인했다.
  잔존 항목은 manual-admin closure 정체, 대형 모듈 순증, FC-05/12(b)/13 검증 환경 부재다.
  세 항목 모두 2026-08-06에 처리했다. 위 2026-08-06 절과 감사 §15/§16을 참조한다.
- Web Console 운영 상태 진실성 설계는
  `docs/superpowers/specs/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness-design.md`,
  구현 계획은
  `docs/superpowers/plans/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness.md`,
  code-level evidence는
  `docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md`가 소유한다.
  정적 셸이 하드코딩하던 `Connected`/`pcv-node-a`/`VM: 3/3`/`API: 10ms avg`를 제거하고 footer와
  hero chip을 실제 state에 바인딩했다. 전면 401에서 게이트가 열리던 원인은 실제 Local API가
  `operation: api.auth`를 반환하는데 게이트가 route id를 기대한 것이었고 `isAuthError`로 닫았다.
  후속 slice는 `getHostReadinessLabel()`의 `Ready` fallback을 helper 안에서 제거해 metric grid와
  ops cockpit까지 해소했다. 상세는 같은 evidence 문서의 closure addendum이 소유한다.
- ADR 검증 명령 정합: 5개 ADR이 `archive/spikes/purecvisor-desktop-node/tests`를 검증 명령으로
  게시했으나 이 경로는 `docs/ga-ready/VERIFICATION_OWNERSHIP.md`에서
  `excluded from default required command`다. 해당 suite는 2026-05 시점의 negative status를 고정
  assertion으로 요구해 항목이 실제로 닫힐수록 깨진다. 검증 명령만
  `packaging/windows-desktop-node/tests`로 교체했고 suite 자체는 archive baseline으로 보존한다.
- 저장소 root `README.md`가 canonical current-evidence 생성기 target에 추가됐다. 이전에는
  target 밖이라 `0.42.63` anchor로 drift했고 canonical은 `0.42.65`였다. 이제 owned 문서는
  7개이며 이전 current 서술은 historical predecessor로 강등했다.
- 2026-07-15 실호스트 functional correctness 검증 결과와 2026-07-16 코어/백엔드/프론트엔드
  구현 평가서는 약 3주간 untracked였다가 `docs/functional-correctness-verification-2026-07-15-results.md`,
  `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md`로 보존했다.
  두 문서는 작성 시점 snapshot이며 current evidence나 operational anchor를 승격하지 않는다.
- 병합되지 않은 PR #172에만 존재하던 2026-07-13 operational follow-up evidence 4건을 복원했다.
  `0.42.59 -> 0.42.62` blocked 시도는 현재 기록된 `0.42.62 -> 0.42.63` follow-up과 다른 건이다.
  당시 index/ledger 본문 편집은 생성기 소유 블록과 충돌하므로 가져오지 않았다.

## 2026-08-03 통합 코딩 가이드

- 현재 적용 규칙, 자동 강제 항목, 관찰된 작성 관행, 향후 ASP.NET Core/QG gate와 범위 외 C++23 경계는
  `docs/CODING_GUIDE.md`에서 통합해 읽는다.
- 이 가이드는 파생 navigation 문서이며 operational current, 적용 ADR, 변경 등급·검증 정책 또는 현재 실행
  계획을 대체하지 않는다. 충돌 시 각 정보의 canonical owner를 우선한다.
- 현재 production transport는 legacy `HttpListener`이고 ASP.NET Core는 Wave 5A 완결과 ADR-0014 이후의
  gated Wave 6 목표다. TypeScript Web Console은 계속 source/build/browser runtime을 소유한다.

## 2026-08-03 1주 단위 서비스 개발 명세

- 주간 서비스 outcome, 기준선, capacity/commitment, observation, 승인과 carry-over 계약은
  `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-weekly-service-development-spec.md`가
  사람이 읽는 non-authoritative delivery projection으로 정리한다.
- Canonical DAG, selector와 mutable state owner는 계속
  `docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md`이며,
  materialization/activation 전에는 아래 2026-08-02 predecessor가 current 실행 계획이다.
- Active approved projection은
  `Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v3`이며 S/M은 Luna Max
  (`gpt-5.6-luna`/`max`),
  L/Release는 Sol high 또는 ultra를 사용한다. Luna unavailable 시 자동 대체하지 않는다.
- 이 명세 작성 시점(2026-08-03) operational current는 `0.42.65-admin-smoke`였다. 설치된
  `0.42.68-admin-smoke`와 current source 55-route snapshot은 non-promoted 상태로 분리했다.
  현재 값은 최상단 생성 블록이 소유한다.
- `WSD-B001`은 v3에서 §5.2/§19 SW-01 seed와 17개 projection row를 보존해 resolved다. `WSD-B002`는
  bootstrap과 Max routing amendment만 resolved이며 control-only materialization은 LC-024까지,
  activation/attestation은 LC-026까지 pending이다.
- Successor는 아직 inactive다. 기존
  `User-Approval: luna-control-materialization-dbac0ae5abd8-20260803`은 사용되지 않은 stale approval이므로
  이 파생 문서 merge 뒤 exact fresh-main 승인을 다시 받아야 한다. Luna selector alias가 canonical
  `gpt-5.6-luna`로 resolve되고 callable함도 확인해야 `LC-001`을 시작할 수 있다. Git activation enforcement와
  LC-024~026이 닫히기 전에는 제품 카드 실행 기준이 아니다.

## 2026-08-02 C# 구조 개선 및 ASP.NET Core 도입 계획

- 현재 실행 계획은 `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`다.
- Wave 0은 동작 보존형 job store/clock/cancellation 및 evidence file-access seam과 테스트 소유권·품질 기준선을 `code_complete`로 닫았고, package 승격은 열지 않아 `promotion_not_triggered`다.
- Wave 1A job runtime owner 분리는 `code_complete`, package 승격은 `promotion_not_triggered`이며 검증 evidence는 `docs/ga-ready/evidence/csharp-architecture-wave1a-job-runtime-owner-2026-08-02.md`가 소유한다.
- Wave 1A는 .NET 전체 642/642·skip 0, API 179/179 3회 반복, quality line `50.322384%`/branch `40.897689%`, Runtime owner scoped line `700/749` (`93.457944%`)/branch `215/260` (`82.692308%`), Full/M summary `ok=true`로 닫았다.
- Wave 1B diagnostics owner 분리도 `code_complete`, package 승격은 `promotion_not_triggered`이며 검증 evidence는 `docs/ga-ready/evidence/csharp-architecture-wave1b-diagnostics-owner-2026-08-02.md`가 소유한다.
- Wave 1B는 .NET 전체 659/659·skip 0, API 196/196 3회 반복, quality line `50.619499%`/branch `41.03664%`, diagnostics owner scoped line `286/299` (`95.652174%`)/branch `76/89` (`85.393258%`), Full/M summary `ok=true`로 닫았다.
- Wave 1C auth/session/RBAC owner 분리도 `code_complete`, package 승격은 `promotion_not_triggered`이며 L/Release 검증 evidence는 `docs/ga-ready/evidence/csharp-architecture-wave1c-auth-owner-2026-08-02.md`가 소유한다.
- Wave 1C는 .NET 전체 673/673·skip 0, API 209/209 3회 반복, Host 162/162, quality line `51.240143%`/branch `41.651865%`, auth owner scoped line `470/514` (`91.439689%`)/branch `188/273` (`68.864469%`), Release/L summary `ok=true`로 닫았다.
- Wave 1D ops dispatch owner 분리도 `code_complete`, package 승격은 `promotion_not_triggered`이며 M/Full 검증 evidence는 `docs/ga-ready/evidence/csharp-architecture-wave1d-ops-dispatch-owner-2026-08-02.md`가 소유한다.
- Wave 1D는 .NET 전체 684/684·skip 0, API 220/220 3회 반복, quality line `51.410248%`/branch `41.696238%`, ops owner+projection scoped line `397/417` (`95.203837%`)/branch `128/172` (`74.418605%`), Full/M summary `ok=true`로 닫았다.
- Wave 2A의 JSON v1/v2 유지, create/start/cancel/complete persist-before-publish, unique candidate/marker temp, durable flush, typed outcome, semantic validation, restart recovery와 transaction lease/CAS 결정은 `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`가 소유한다. `W0-FI-01`/`W0-FI-02`/`W0-FI-04`와 current-writer single-transaction 경계는 `code_complete`다. 선행 physical evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2a-physical-job-store-durability-2026-08-02.md`, completion evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`다.
- Wave 2A completion source는 .NET 전체 795/795·skip 0, API 228/228, Runtime 120/120, Host 181/181, gap registry 10/10, job-hardening 10/10, dry-run `ok=true`와 Release/L 7/7 suite `ok=true`로 검증한다. 실제 frozen `0.42.65-admin-smoke` binary reader도 v1/v2 terminal/FIFO queue initial/restored 8/8, Pester 5/5와 hash 불변을 확인했다.
- Wave 2B operation reconciliation 결정표는 `code_complete + promotion_not_triggered`로 닫았다. 결정 spec은 `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2b-operation-reconciliation-decision.md`, machine-readable fixture는 `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2b-reconciliation.json`, evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2b-operation-reconciliation-decision-2026-08-03.md`가 소유한다. 현재 22개 mutation operation/9개 family를 분류했고 focused Pester 6/6·skip 0이다. persisted-running은 `PCV_JOB_INTERRUPTED`/`retryable=false`/automatic retry=false를 유지하며 QoS 정책 readback 부족과 Guest Execution 별도 설계 경계를 명시했다.

## 2026-08-03 C# architecture Wave 2C `vm.rename`/`vm.delete`/`checkpoint.create` reconciliation code-level pass

- Wave 2C `vm.rename` reconciliation은 `code_complete + promotion_not_triggered`로 닫았다. Spec은 `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-vm-rename-reconciliation.md`, machine-readable fixture는 `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-vm-rename-reconciliation.json`, evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2c-vm-rename-reconciliation-2026-08-03.md`가 소유한다. enqueue는 read-only `vm.list` baseline을 durable metadata로 저장하고, `POST /api/v1/jobs/{jobId}/reconcile`는 confirmed postcondition만 기존 `succeeded`로 commit한다. ambiguous/readback-unavailable 경계는 `409 PCV_JOB_RECONCILIATION_REQUIRED`와 `failed` 유지로 닫고, Web `Reconcile rename`/PCVCLI `job reconcile` parity와 `job-reconciled`/`job-reconciliation-required` observations를 고정했다. 실제 VM/Hyper-V mutation, package candidate, installed smoke와 promotion은 실행하지 않았다.
- 후속 `vm.delete` reconciliation도 `code_complete + promotion_not_triggered`로 닫았다. Spec은 `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-vm-delete-reconciliation.md`, machine-readable fixture는 `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-vm-delete-reconciliation.json`, Pester 계약은 `packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1`, evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2c-vm-delete-reconciliation-2026-08-03.md`가 소유한다. enqueue는 managed marker와 stable VM id를 포함한 read-only `vm.list` before-state를 durable metadata로 저장하고, absent row만 기존 `succeeded`로 reconcile한다. same-id 잔존·재생성 identity·unmanaged collision·중복 이름·readback unavailable은 `409 PCV_JOB_RECONCILIATION_REQUIRED`와 `failed` 유지로 닫았으며 Web `Reconcile delete`/PCVCLI `job reconcile` parity를 유지했다. 실제 VM/Hyper-V mutation, package candidate, installed smoke와 promotion은 실행하지 않았다.
- 후속 `checkpoint.create` reconciliation도 `code_complete + promotion_not_triggered`로 닫았다. Spec은 `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-checkpoint-create-reconciliation.md`, machine-readable fixture는 `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json`, Pester 계약은 `packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1`, evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2c-checkpoint-create-reconciliation-2026-08-03.md`가 소유한다. enqueue는 scoped `checkpoint.list`에서 requested name 부재를 durable metadata로 저장하고, 동일 VM/이름 row가 정확히 하나일 때만 기존 `succeeded`로 reconcile한다. not-applied·duplicate identity·existing-name·readback unavailable은 `409 PCV_JOB_RECONCILIATION_REQUIRED`와 `failed` 유지로 닫았으며 `checkpoint.restore`는 계속 제외한다. Web `Reconcile checkpoint`/PCVCLI `job reconcile` parity를 유지했고 실제 VM/Hyper-V mutation, package candidate, installed smoke와 promotion은 실행하지 않았다.
- 남은 위험과 후속 RED/GREEN 책임은 `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md`가 소유한다. Process-lifetime lease, mixed-version writer, hostile local-admin namespace TOCTOU, directory-fsync/power-loss 및 Hyper-V side-effect exactly-once는 비주장이고, 다음 코드 변경 wave는 명시적 operation 승인 후 2C다. Job/console 얕은 wrapper는 각각 별도 callback-free owner slice, `BatchEvidenceSummaryReader` 내부는 Wave 7로 남겼다.
- 이 단계는 ASP.NET Core를 도입하지 않았고 `System.Net.HttpListener`를 유지한다. Web Console/PCVCLI, TypeScript 정적 자산과 internal/private network 경계도 그대로다. `0.42.66-admin-smoke`의 2026-08-02 초기 legacy 설치 시도는 host TCP excluded range `7765-7864`가 API 7777을 포함해 `blocked-by-host-tcp-excluded-port-7777`였다. Failed-install MSI transaction은 product files/registration을 자동 rollback했지만 각 시도에서 stopped owned service를 남겼고, exact `ImagePath` 소유권 확인 후 수동 삭제했다. 승인된 재부팅 뒤 동일 MSI의 2026-08-03 checkpoint는 install exit 0, service `Running`/`Auto`/`LocalSystem`, Web/API/PCVCLI provider-free smoke와 ProgramData store hash 불변으로 `PASS`했다. 증거는 `docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`가 소유한다. 최종 service는 설치·실행 상태를 유지하고 Hyper-V/VM/full-admin 승격은 실행하지 않았으며, 초기 failed-install service cleanup 결함도 계속 open이다. 이 checkpoint는 `0.42.65-admin-smoke` operational anchor를 대체하지 않는다.
- PR #175 merge 후 `fa5fbaa8930715f8d6d84fed60f94b5d9712ef92` 기준 `0.42.67-admin-smoke` package 후보는 Wave 5A code slice 이전 historical preflight로 보존한다. 최신 `f93370610bf221da00e89131d874e903ba72b644` 기준 `0.42.68-admin-smoke` unsigned internal package 후보를 생성했다. MSI는 `99957937f00c3f26392cae86df7ea090d84f6020821348cc6eb879dd667a2e70`, payload aggregate는 `b0e47050aab167890c1a3e0bec09e4eb6f4889eb1068c1896d58ec8f15d1afa8`이며 preflight evidence는 `docs/ga-ready/evidence/csharp-architecture-wave5a-package-preflight-2026-08-03-04268.md`가 소유한다. 이 문서의 설치 blocked 표시는 설치 전 시점의 historical 상태이며, 관리자 설치/CLI smoke 결과는 `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md`가 소유한다. ASP.NET Core는 여전히 별도 Wave 6 code slice다.
- Wave 5A 후속 bounded admission/lifetime code slice는 `tracked_async_serialized`를 명시적으로 선택할 때만 active `32`/waiting `64` admission, body-read 이전 `503 PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED`/`Retry-After`, noVNC 포함 request task tracking과 shutdown drain을 적용한다. 기본 `legacy` HttpListener, Web/PCVCLI payload와 설치본 listener는 변경하지 않았다. Host/solution 테스트는 각각 `186/186`, `815/815` PASS이며 evidence는 `docs/ga-ready/evidence/csharp-architecture-wave5a-admission-lifetime-code-slice-2026-08-03.md`다. Wave 5A 전체 완료와 ASP.NET Core 전환은 아직 pending이다. 기존 ADR-0013이 job-store 결정으로 적용 중이어서 향후 ASP.NET Core server 결정 번호는 ADR-0014로 예약했다.
- 명시적 관리자 승인 후 `0.42.68-admin-smoke` MSI를 설치하고 `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md`에서 installed Web/API와 elevated protected-token PCVCLI `runtime policy`/`host status`/`ops summary` 3개 exit `0`을 확인했다. 이 설치 smoke는 Hyper-V/VM/provider mutation을 실행하지 않았고 `0.42.65-admin-smoke` operational anchor를 대체하지 않는다.
- 후속 `fbd4b90`에서는 `DesktopNodeHostApplication.StartAsync`의 부분 listener bind cleanup을 고정했다. 점유된 Web prefix로 두 번째 bind를 실패시킨 뒤 첫 API listener 재바인드를 확인했고, focused Host `2/2`, 전체 .NET `816/816`·skip `0`으로 `docs/ga-ready/evidence/csharp-architecture-wave5a-listener-bind-cleanup-2026-08-03.md`에 기록했다. 설치본·Hyper-V/VM/provider mutation은 실행하지 않았다.
- ADR-0012 API read concurrency는 기존 processor 직렬화와 single mutation worker를 유지하는 `closed-not-adopted`로 종결했다. `read_concurrency_mode=bounded`나 route allowlist는 추가하지 않았으며, decision record는 `docs/adr/0012-api-read-concurrency-policy.md`가 소유한다. Wave 5A async lifetime/admission의 나머지 installed/load/cancellation 책임과 Wave 6 ASP.NET Core 전환은 계속 pending이다.

## 2026-07-14 CLI/Web-only 운영자 표면

- 현재 결정은 `docs/adr/0011-cli-web-only-operator-surface.md`이며 활성 운영자 표면은
  Web Console과 PCVCLI다.
- TUI source/package/smoke 제거의 code-level evidence는
  `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`다.
- Local API/backend는 유지된다. Operational current와 installed non-promoted candidate는
  `docs/ga-ready/current-evidence.json`과 이 문서 최상단 current section을 우선한다. `0.42.62-admin-smoke`와
  미완료 `0.42.63-admin-smoke` chain은 historical predecessor다.

## 2026-07-13 개발 게이트 복구 historical predecessor

- 비관리자 Windows의 .NET test 격리, CLI protected-token 오류 계약, Host ACL hardener 주입,
  비변경 `Development Gates` workflow의 로컬 검증 결과는
  `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`가 소유한다.
- 이 code-level 변경은 설치본/package anchor `0.42.59-admin-smoke`를 바꾸지 않으며,
  `0.42.60-admin-smoke` 검증 chain은 별도 승인이 필요하다.

## 2026-05-29 historical predecessor

> 아래 문단은 당시 evidence를 보존하는 역사 기록이다. 현재 operational tuple과 활성 운영자 표면은
> `docs/ga-ready/current-evidence.json` 및 이 문서 최상단 current section을 우선한다.

현재 operational full admin host mutation anchor는 `0.42.59-admin-smoke` /
`full-admin-host-mutation-gate-20260529-04259`이고, current ledger는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
`current-evidence-ledger-2026-05-29-04259-manual-admin-package-pair-closed`다. Full admin host mutation evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`,
latest package evidence는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`다.
설치본 Web/TUI/CLI current-card는 04259 fullgate 후 PASS했고, evidence는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`다.
Web/TUI running guest execution cancel affordance는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md`에서
설치본 PASS로 승격됐다.
최신 closed manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. 직전 `0.42.58-admin-smoke`
predecessor는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md`,
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`로 보존한다.
0.42.56 predecessor는 `0.42.56-admin-smoke`,
`docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04256.md`,
`full-admin-host-mutation-gate-20260528-04256`,
`manual-admin-campaign-descriptor-20260528-04255-04256-closed`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md`로
public-boundary follow-up의 기준 anchor를 보존한다. 설치본 console access,
account login, target-backed noVNC streaming smoke는 각각
`artifacts/installed-console-access-smoke-20260526-04245/summary.json`,
`artifacts/installed-account-login-smoke-20260526-04245/summary.json`,
`artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245/summary.json`로 PASS다.

Post-04245 확장 planning은 Phase 2-5를 implementation-ready 산출물로 분리했다.
`docs/adr/0008-hyperv-qos-mutation-policy.md`와
`docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md`가
다음 backend 구현 slice의 기준이다. 2026-05-26 첫 code-level slice는
`docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`가 기록하며,
preview/apply API, queued dispatch, native WMI code path, PCVCLI UX까지 구현했다.
설치본 승격 evidence는 `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`이며,
`0.42.47-admin-smoke` package build, full admin host mutation gate, 실제 VM 대상
PCVCLI storage/network QoS dry-run/apply/rollback smoke를 PASS로 기록한다.
Manual-admin package-pair closure도 `0.42.45-admin-smoke -> 0.42.47-admin-smoke`로
닫혔다. Phase 3 Web/TUI QoS direct control은
`docs/ga-ready/evidence/phase3-web-tui-qos-direct-control-code-level-2026-05-26.md`에서
code-level PASS 후 `0.42.48-admin-smoke` package/fullgate/current-card까지 승격됐다.
`0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair closure도
`manual-admin-campaign-descriptor-20260526-04247-04248-closed`로 닫혔다. Guest Execution은
`docs/adr/0009-guest-execution-security-boundary.md`가 credential/audit/redaction/timeout/cancel/RBAC
경계를 적용 중인 contract로 소유한다. 최신 product payload는
`docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253.md`에서
provider route, channel verify/repair, Web/TUI direct-control surface까지 PASS했다.
실제 Windows guest credentialed execution smoke는 persistent Windows VHD target과 DPAPI
LocalMachine credential reference 기준으로 PASS했다. Running interrupt/cancel은 0.42.54 설치본
package/current-card와 actual long-running Windows guest smoke에서 PASS했고, 0.42.55는
Web/TUI cancel affordance와 actual credentialed guest-exec를 설치본 current-card로 재확인했다.
04250→04254 manual-admin readiness는 baseline mismatch로 blocked
기록만 남겼다.
noVNC target config mutation은 `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md`가
보류 경계를 소유한다. Guest Execution docs-contract evidence는
`docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md`다.

설치본 TUI row projection fix는 실제 VM `pcv-ux-qos-04241` 기반
`pcvtui --smoke-once vm` smoke로 재확인했고,
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`가
소유한다. 최신 post-merge public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`가
current이고 run `26636072420`, job `78496568595`, head
`5a2f91762a6c2a8ab6b84d334fa6cb420474671f`에서 PASS했다. `0.42.60-admin-smoke`
installed current-card payload 후보는 이미 열려 있으며, docs-maintenance postpush는 추가
package 후보를 열지 않는다. account/noVNC는
0.42.58 PASS를 carry-forward하고 actual VM Guest Execution/QoS smoke는 provider/control payload 변경
때 재실행한다. 0.42.57/0.42.56/0.42.54 fullgate/running cancel/0.42.53/0.42.48/0.42.45 public-boundary,
PR #169 public-boundary와 후속
`docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`는
historical predecessor로 보존한다. PR #168/PR #167/PR #164/PR #163/PR #162/PR #160 public-boundary도 historical predecessor로
보존한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부
stable publication evidence가 아니다. 아래 이전 날짜 current 문단은 historical predecessor로
해석한다.

## 2026-05-21 historical predecessor

현재 operational full admin host mutation anchor는 `0.42.40-admin-smoke` /
`full-admin-host-mutation-gate-20260521-04240`이고, current ledger는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
`current-evidence-ledger-2026-05-21-04240-current-card-04241-trigger`다.
Full admin host mutation evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`다.
설치본 Web/TUI/CLI current-card는 04240 기준으로 PASS했고, evidence는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`다.
최신 closed manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md` /
`manual-admin-campaign-descriptor-20260521-04239-04240-closed`다.

최신 product payload package는 `0.42.40-admin-smoke` Web/TUI QoS/guest readback
surface promotion이며 `docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`와
위 full gate/manual-admin closure evidence가 소유한다. 설치본 04239 PCVCLI QoS/guest
targeted smoke는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04239.md`,
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`가
소유한다. `0.42.39-admin-smoke -> 0.42.40-admin-smoke` manual-admin package-pair는
Windows Update clean-host, Burn, MSIX, installed update/rollback으로 closure 전환했다.
Web/TUI QoS/guest readback surface는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`에서
code-level PASS 후 04240 package chain으로 닫혔다.
Actual VM Web/TUI QoS/guest readback evidence는
`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`가
기록한다. Web panel no-overlap은 PASS했고 설치본 TUI row projection blocker는 source fix
code-level PASS 후 `0.42.41-admin-smoke` package chain trigger로 남겼다.
Historical 0.42.38 VM media/resource mutation route promotion과 0.42.37 실제 VM lifecycle
smoke는 predecessor로 보존한다. PR #167
post-merge public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`
가 current이고, PR #164/PR #163/PR #162/PR #160 public-boundary는 historical predecessor로 보존한다. 아래 이전
날짜의 current 문단은 historical predecessor로 해석한다.

## 2026-05-17 현재 기준

Historical `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Host Ops lifecycle predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` / `manual-admin-campaign-descriptor-20260517-04226-04227-closed`이며, target MSI SHA-256 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`, update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`, provenance commit `69aba3eb3ff08c843f1a481818ddc86eac2f019b`와 함께 `host-ops-lifecycle-descriptor-bridge-v1` / `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated` 계약을 보존한다.

Historical `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Operator Surface predecessor는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md` / `manual-admin-campaign-descriptor-20260517-04227-04228-closed`이며, target MSI SHA-256 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`, provenance commit `b9676f6dc37d667ae0d60367e9f4e576a27e3864`로 보존한다. PR #151 public-boundary predecessor는 `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`, run `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`로 보존한다. 둘 다 public trusted signing 또는 외부 stable publication evidence가 아니다.

최신 installed operational evidence anchor는 `0.42.34-admin-smoke` / `full-admin-host-mutation-gate-20260519-04234`다. Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`와 operational full-gate package `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유하고, full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`, installed Web/TUI/CLI current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가 소유한다. Manual-admin package-pair closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` / `manual-admin-campaign-descriptor-20260519-04232-04234-closed`가 current이며 package pair는 `0.42.32-admin-smoke -> 0.42.34-admin-smoke`, update ZIP SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`, target MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. 0.42.32 closure는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`, `full-admin-host-mutation-gate-20260519-04232`, `manual-admin-campaign-descriptor-20260519-04231-04232-closed`로 historical predecessor로 보존한다. Host Ops lifecycle descriptor bridge는 `host-ops-lifecycle-descriptor-bridge-v1`, bucket count `6`, bucket contract `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`, Web diagnostics table contract `host-ops-web-diagnostics-bucket-table-v1`로 current-card에 연결됐다. Installed account/noVNC smoke는 0.42.29 historical PASS로 보존하고 다음 account/noVNC payload 변경 때 재검증한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

> 대상: `purecvisor-desktop-node` Windows 전용 저장소

현재 개발 기준은 `0.42.30-admin-smoke` operational evidence를 current anchor로 본다.
최신 full admin host mutation evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04230-hostmutation.md`,
installed operator surface evidence는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`다.
최신 manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`이며 descriptor
`manual-admin-campaign-descriptor-20260518-04229-04230-closed`, update ZIP SHA-256
`f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`, target MSI SHA-256
`90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`로 닫혔다. 이전
0.42.29/0.42.28/0.42.27/0.42.26 package/fullgate/current-card/manual-admin closure는 historical predecessor로
보존한다. Evidence는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`와
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`다. 이전
full-gate batch는 `full-admin-host-mutation-gate-20260516-04226`이고 Runtime/API current
evidence contract는 `runtime-api-current-evidence-rollup-v1`이다. Full-gate/target MSI
SHA-256은 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`,
provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`다. 해당 package-pair
update ZIP SHA-256은 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`이고 Descriptor
`manual-admin-campaign-descriptor-20260517-04225-04226-closed`는 `missing_count=0`,
`not_pass_count=0`으로 PASS다. 2026-05-16 `0.42.25 -> 0.42.26` initial descriptor는
`docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`에
candidate로 보존하며 `missing_count=4`, `not_pass_count=1`이다.
Current/historical evidence 중복 설명은
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`로 압축한다. Post-04226 follow-up은
`docs/ga-ready/evidence/post-04226-ledger-contract-followup-2026-05-17.md`이며,
Runtime/API current-card descriptor id direct expose(`current_card_descriptor_batch_id`),
manual-admin descriptor schema v2(`descriptor_schema_version=2`,
`manual-admin-descriptor-generation-contract-v2`), Batch Supervisor `-DescriptorBatchId`
전달을 고정했다. 이 branch 이전에는 새 product payload가 없었지만
이 branch 자체가 product payload 변경이므로 다음 package/fullgate/package-pair trigger는
`post-04226-ledger-contract-merge`다.

## 먼저 볼 문서

| 상황 | 먼저 볼 문서 |
|------|--------------|
| 제품 실행/사용 확인 | `docs/USER_GUIDE.md`, `README.md`, `packaging/windows-desktop-node/README.md` |
| 서비스 기획/다음 기능 경계 확인 | `docs/SERVICE_PLAN.md` |
| 서비스 기획 P0 개발 계획 | `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md` |
| 사용자 기능 사용 명세 확인 | `docs/USER_FEATURE_USAGE_SPEC.md`, `docs/USER_GUIDE.md`, `docs/CLI_COMMAND_USAGE.md` |
| 문서 언어 규칙과 한국어 재작성 상태 확인 | `AGENTS.md`, `docs/KOREAN_DOCUMENTATION_ROLLOUT.md` |
| GA-ready matrix/ADR/evidence 한국어 2차 범위 확인 | `docs/KOREAN_DOCUMENTATION_ROLLOUT.md`, `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/adr/0006-internal-private-network-distribution.md`, `docs/ga-ready/EVIDENCE_INDEX.md` |
| Operational current/evidence ledger 확인 | `docs/ga-ready/current-evidence.json`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, `docs/ga-ready/EVIDENCE_INDEX.md` |
| branch 역사/폐기/재사용 triage 확인 | `docs/BRANCH_TRIAGE_2026-05-11.md` |
| CLI 명령어 사용 설명서 확인 | `docs/CLI_COMMAND_USAGE.md`, `src/DesktopNode.Cli/README.md` |
| Phase 2 Hyper-V QoS mutation 설치본 승격 | `docs/adr/0008-hyperv-qos-mutation-policy.md`, `docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`, `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`, `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation-design.md`, `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md` |
| Guest Execution 보안 경계 확인 | `docs/adr/0009-guest-execution-security-boundary.md`, `docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md` |
| noVNC target config mutation 보안 정책 확인 | `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md` |
| 설치본 운영/runbook 확인 | `docs/OPERATIONS_GUIDE.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `packaging/windows-desktop-node/README.md` |
| 저장소 경계 확인 | `docs/PUBLIC_RELEASE_BOUNDARY.md` |
| 현재 적용 ADR 확인 | `docs/ADR_INDEX.md` |
| 검증 기준 확인 | `docs/DEVELOPMENT_VERIFICATION_POLICY.md` |
| 개발 가속 고정 기준 확인 | `docs/DEVELOPMENT_VERIFICATION_POLICY.md` |
| 전체 phase 순서 확인 | `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md` |
| Phase 11 제품 승격 판단 확인 | `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md` |
| Phase 13 WinSW product wrapper 변경 | `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md` |
| Phase 14 installer 변경 | `packaging/windows-desktop-node/installer/README.md` |
| Phase 15 protected token storage 확인 | `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage.md` |
| Phase 16 diagnostics 확인 | `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics.md` |
| Phase 17 LAN security policy 확인 | `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy.md` |
| Phase 18 update/rollback/config migration 확인 | `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration.md`, `docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md` |
| Updater catalog/channel resolver 확인 | `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`, `packaging/windows-desktop-node/README.md`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` |
| Update filesystem rollback 확인 | `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`, `packaging/windows-desktop-node/README.md`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` |
| Packaging publication descriptor 확인 | `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`, `packaging/windows-desktop-node/installer/README.md`, `packaging/windows-desktop-node/installer/build.ps1` |
| ADR-0005 public distribution gate 종료 기록 확인 | `docs/adr/0005-public-distribution-operations-expansion-candidate.md`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionDescriptor.ps1` |
| ADR-0006 내부 사설망 배포 결정 확인 | `docs/adr/0006-internal-private-network-distribution.md`, `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`, `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md` |
| Internal HTTPS/TLS lifecycle installed smoke 확인 | `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`, `artifacts/internal-https-tls-lifecycle-installed-20260510-0397/summary.json`, `packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1` |
| Internal clean-host install/update/rollback smoke 확인 | `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, `artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417/summary.json`, `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1` |
| Lifecycle/Packaging current rebaseline 확인 | `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/summary.json`, `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1` |
| MANUAL-ADMIN 1-2-3-4 캠페인 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-11-0418-0419.md`, `artifacts/manual-admin-campaign-20260511-0418-0419/summary.json`, `artifacts/internal-clean-host-install-update-rollback-smoke-20260511-0418-0419/summary.json` |
| 최신 MANUAL-ADMIN 1-2-3-4 캠페인 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-11-0420-0421.md`, `artifacts/manual-admin-campaign-20260511-0420-0421/summary.json`, `artifacts/internal-clean-host-install-update-rollback-smoke-20260511-0420-0421/summary.json` |
| Post-0423 후속 triage와 다음 slice 계획 확인 | `docs/ga-ready/evidence/post-0423-followup-triage-2026-05-12.md`, `docs/superpowers/plans/2026-05-12-purecvisor-desktop-node-post-0423-followup-slices.md`, baseline `0.42.3-admin-smoke`, target `0.42.4-admin-smoke` |
| Manual-admin 0423→0424 historical blocker 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`, `artifacts/admin-smoke-package-20260512-0424`, `artifacts/manadm-0424/lifecycle/product-update-rollback`, `artifacts/manadm-0424/clean-host-rerun`; result `historical-partial-pass-clean-host-blocked` |
| Manual-admin 0425→0426 historical predecessor 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`, `artifacts/manual-admin-campaign-20260512-0425-0426/manual-admin-campaign-descriptor/summary.json`, `artifacts/manual-admin-campaign-20260512-0425-0426/clean-host/summary.json`, `artifacts/manual-admin-campaign-20260512-0425-0426/burn-bootstrapper-lifecycle-r2/burn-lifecycle-summary.json`, `artifacts/msix-package-lifecycle-smoke-20260512-0425-0426/summary.json`; result `PASS`; 0427→0428 PASS 이후 current package-pair claim에서 내려감 |
| Manual-admin 04216→04218 current campaign 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md`, `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised/summary.json`, `artifacts/manual-admin-campaign-20260515-04216-04218/clean-host-updated-os/summary.json`, `artifacts/manual-admin-campaign-20260515-04216-04218/burn-bootstrapper-lifecycle/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260515-04216-04218/summary.json`; result `PASS`; target package `artifacts/admin-smoke-package-20260515-04218`, MSI SHA-256 `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`, update ZIP SHA-256 `8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd` |
| Manual-admin 04219→04220 current campaign 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04219-04220.md`, `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-campaign-descriptor-supervised/summary.json`, `artifacts/manual-admin-campaign-20260516-04219-04220/clean-host-updated-os/summary.json`, `artifacts/manual-admin-campaign-20260516-04219-04220/burn-bootstrapper-lifecycle/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260516-04219-04220/summary.json`; result `PASS`; target package `artifacts/admin-smoke-package-20260516-04220`, MSI SHA-256 `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`, update ZIP SHA-256 `8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b`; public trusted signing 또는 외부 stable publication evidence 아님 |
| Manual-admin 04220→04221 current campaign 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04220-04221.md`, `artifacts/manual-admin-campaign-20260516-04220-04221/manual-admin-campaign-descriptor-supervised/summary.json`, `artifacts/manual-admin-campaign-20260516-04220-04221/clean-host-updated-os/summary.json`, `artifacts/manual-admin-campaign-20260516-04220-04221/burn-bootstrapper-lifecycle/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260516-04220-04221/summary.json`; result `PASS`; target package `artifacts/admin-smoke-package-20260516-04221`, MSI SHA-256 `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b`, update ZIP SHA-256 `09e1c3f5a7c8d2afac3d70bddbb1d91f575de2c45c9174a8da2bbb73c2e89767`; public trusted signing 또는 외부 stable publication evidence 아님 |
| Manual-admin 04222→04223 current campaign 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`, `artifacts/manual-admin-campaign-20260516-04222-04223/manual-admin-campaign-descriptor-supervised/summary.json`, `artifacts/manual-admin-campaign-20260516-04222-04223/clean-host-updated-os/summary.json`, `artifacts/manual-admin-campaign-20260516-04222-04223/burn-bootstrapper-lifecycle/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260516-04222-04223/summary.json`; result `PASS`; target version `0.42.23-admin-smoke`, target package `artifacts/admin-smoke-package-20260516-04223`, MSI SHA-256 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, update ZIP SHA-256 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`, provenance commit `676b4177b10dc80209969066857bab6008ff2473`; descriptor `manual-admin-campaign-descriptor-20260516-04222-04223-closed`, `missing_count=0`, `not_pass_count=0`; public trusted signing 또는 외부 stable publication evidence 아님 |
| 0.42.24 Runtime/API current evidence rollup historical 확인 | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`; result `PASS`; full-gate batch `full-admin-host-mutation-gate-20260516-04224`, package build MSI SHA-256 `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit `b974d6b541423f2e4160f726f96155b16f105e9d`; descriptor `manual-admin-campaign-descriptor-20260516-04223-04224`는 `missing_count=5`, `not_pass_count=1`로 blocked이며 04226 closure 이후 historical predecessor로 보존 |
| 0.42.25 fullgate/current-card/manual-admin closure 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04225-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04225.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`, `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md`; full-gate/target MSI SHA-256 `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance commit `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`, update ZIP SHA-256 `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`; descriptor `manual-admin-campaign-descriptor-20260516-04224-04225-closed`, `missing_count=0`, `not_pass_count=0`; public trusted signing 또는 외부 stable publication evidence 아님 |
| 0.42.26 package/fullgate/current-card/manual-admin closure 확인 | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04226.md`, `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`, `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md`; package MSI SHA-256 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`, full-gate/target operational MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`; initial descriptor `manual-admin-campaign-descriptor-20260516-04225-04226`, readiness PASS, `missing_count=4`, `not_pass_count=1`; closure descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`, `missing_count=0`, `not_pass_count=0`, update ZIP SHA-256 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`; public-boundary PR #145 run `25961834812`, job `76318357776`; public trusted signing 또는 외부 stable publication evidence 아님 |
| 0.42.23 full admin current-card 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md`, `docs/ga-ready/evidence/post-04223-full-host-mutation-current-card-2026-05-16.md`; result `PASS`; full-gate batch `full-admin-host-mutation-gate-20260516-04223`, closed package MSI SHA-256 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, full-gate MSI SHA-256 `ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932`, closed package provenance commit `676b4177b10dc80209969066857bab6008ff2473`, full-gate provenance commit `d11a096086326004f27facd9612c2296ded15a4b`; Runtime/API registry bridge route detail count `4`; public-boundary post-merge run `25954744127`, job `76299282407`; next product payload candidate `0.42.24-admin-smoke` |
| 0.42.22 package/full admin current-card 확인 | `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md`, `docs/ga-ready/evidence/post-04222-package-host-mutation-current-card-2026-05-16.md`; result `PASS`; predecessor evidence로 보존 |
| Manual-admin 04221→04222 blocker 확인 | `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04221-04222.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`, `artifacts/manual-admin-campaign-20260516-04221-04222/burn-bootstrapper-lifecycle/summary.json`; descriptor batch `manual-admin-campaign-descriptor-20260516-04221-04222`; result `blocked-by-burn-credential-manager-idempotence`; initial descriptor는 missing count `4`, not-pass count `1`로 생성됐고 실제 campaign은 Burn install exit `1603`에서 닫히지 않음 |
| Post-04218 follow-up package build 확인 | `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`, `artifacts/admin-smoke-package-20260515-04219`; `0.42.19-admin-smoke`, MSI SHA-256 `3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55`, provenance commit `2b7bd9ed702a785361ea5bbaa8a969280d400360`; Runtime route registry/Hyper-V dispatch/Host Ops family/current-card snapshot parity와 `public-boundary-ci-required` guard PASS; update ZIP/package-pair/full admin host mutation 미실행 |
| 0.42.12 manual-admin package-pair 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`, `artifacts/manual-admin-campaign-20260514-04211-04212`; `0.42.11-admin-smoke -> 0.42.12-admin-smoke`, target MSI SHA-256 `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, update ZIP SHA-256 `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`, historical predecessor, public trusted signing 또는 외부 stable publication evidence 아님 |
| 0.42.20 full admin host mutation 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220/summary.json`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220/summary.json`, `artifacts/os-mutation-gates-batch-profile-20260516-04220/summary.json`, `artifacts/installed-current-card-20260516-04220-fullgate/summary.json`; result `PASS`; full-gate MSI SHA-256 `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`; public-boundary workflow PASS evidence `docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md`, run `25933428239`, job `76232707240`; 이전 run `25930077313` billing/spending-limit blocker는 historical; 2026-05-15 04218/04216/04217 regression, 2026-05-14 04215/04212 evidence는 historical predecessor |
| 0.42.21 full admin host mutation 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221/summary.json`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221/summary.json`, `artifacts/os-mutation-gates-batch-profile-20260516-04221/summary.json`, `artifacts/installed-current-card-20260516-04221-fullgate/summary.json`; result `PASS`; full-gate MSI SHA-256 `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`; public-boundary successor evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md`, run `25938745434`, job `76250726268`; installed operator surface smoke `artifacts/installed-operator-surface-current-card-20260516-04221`; 04220 및 이전 evidence는 historical predecessor |
| Public-boundary CI main push / fallback guard 확인 | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md`; run `25961834812`, job `76318357776`, head `d6500c01c972cbc7ca1e290e51120181ceea1501`; `public-boundary-ci-required` PASS; branch protection/ruleset은 private repo 현재 플랜에서 unavailable이므로 fallback guard는 PR/merge check 확인; workflow checkout은 `actions/checkout@v6.0.2`; previous PR #144 run `25959505688`, previous 04224 run `25958514394`, previous 04223 run `25954744127`, previous 04222 run `25952150476`, 04221 successor run `25938745434`, historical 04220 evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md`는 보존; public trusted signing 또는 외부 stable publication evidence 아님 |
| Public-boundary checkout v6.0.2 main push 확인 | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md`; run `25934411998`, job `76236050409`, head `3933231e6e2abf3a398dfcc3fdc999b3df38dac6`; `actions/checkout@v6.0.2`, `public-boundary-ci-required` PASS; Node.js 20 deprecation warning 미관찰; public trusted signing 또는 외부 stable publication evidence 아님 |
| Post-ci-maintenance development slices 확인 | `docs/ga-ready/evidence/post-ci-maintenance-dev-slices-2026-05-16.md`; result `CODE_LEVEL_PASS`; Runtime/API registry bridge `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, Hyper-V WMI provider call-site drift guard, Host Ops dry-run/mutation reason code, manual-admin descriptor generation v2를 고정; next product payload candidate `0.42.21-admin-smoke`; host mutation performed `false`, public trusted signing 또는 외부 stable publication evidence 아님 |
| Post-04221 successor operator surface 확인 | `docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md`; result `CODE_LEVEL_AND_OPERATOR_SURFACE_PASS`; public-boundary successor run `25938745434`, installed Web/TUI/CLI current-card smoke `artifacts/installed-operator-surface-current-card-20260516-04221`, Web Console diagnostics direct expose `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, next product payload candidate `0.42.22-admin-smoke`; host mutation performed `false`, public trusted signing 또는 외부 stable publication evidence 아님 |
| Post-04220 development slices 확인 | `docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md`; result `CODE_LEVEL_PASS`; `0.42.20-admin-smoke` 기준 Runtime diagnostics/ops summary contract, Hyper-V WMI common helper catalog, Host Ops mutation boundary, `0.42.20 -> next` packaging trigger를 code/test contract로 고정; public-boundary workflow rerun `25933428239` PASS; 이전 `25931297085`은 GitHub billing/spending-limit historical blocker; host mutation performed `false`, public trusted signing 또는 외부 stable publication evidence 아님 |
| Post-04218 contract alignment 확인 | `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`, `docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md`, `docs/ga-ready/hyperv-domain-baseline-2026-05-11.md`, `docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md`; `0.42.18-admin-smoke` 기준 Runtime/Core route/evidence bridge, Hyper-V dispatch catalog detail, Host Ops lifecycle bucket, packaging next trigger, Web Console/TUI/CLI operator journey, ADR-0005/0006 public boundary 보존; host mutation performed `false` |
| 0.42.14 selector guard package 확인 | `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`, `artifacts/admin-smoke-package-20260514-04214-selectorfix`; `0.42.14-admin-smoke`; manual-admin descriptor batch를 current-card operational latest 후보에서 제외; MSI SHA-256 `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`, 04218 follow-up current-card smoke `artifacts/installed-current-card-20260515-04218-fullgate` |
| Hyper-V provider set contract code-level 확인 | `docs/ga-ready/evidence/hyperv-provider-set-contract-code-level-2026-05-15.md`, `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs`, `src/DesktopNode.Api.Tests/HyperVDomainContractTests.cs`; provider boundary map과 default WMI provider composition drift guard PASS |
| 0.42.13 manual-admin package-pair 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`, `artifacts/manual-admin-campaign-20260514-04212-04213`; target MSI SHA-256 `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`, update ZIP SHA-256 `638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9`, Windows Update NoContact recovery summary 포함, 04214→04215 PASS 이후 historical predecessor |
| Post-04212 follow-up triage 확인 | `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`; `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 product payload 변경 없음; `0.42.13-admin-smoke` package build/full admin host mutation/package-pair campaign 보류; recovery summary key는 다음 clean-host run에서 `recovery_actions`와 `automatic_recovery_performed`로 판정 |
| Post-04212 `1-2-3-4-5` current-card follow-up 확인 | `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`, `artifacts/web-console-current-card-20260514-04212-rerun-followup/summary.json`; `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product payload 변경 없음; package/host mutation chain 보류; Dashboard/Evidence view current-card smoke `PASS`; 당시 표시 batch `full-admin-host-mutation-gate-20260514-04212-rerun`, version `0.42.12-admin-smoke` |
| Product wrapper native repair package 확인 | `docs/ga-ready/evidence/product-wrapper-native-repair-package-2026-05-13-04211.md`, `artifacts/admin-smoke-package-20260513-04211`, `artifacts/installed-batch-evidence-current-card-20260513-04211`; product wrapper `RepairInstalled -BatchEvidenceRoot` native service-action 호출, outer start skip, current-card PASS |
| 0.42.10 duplicate outer start RCA 확인 | `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`, `artifacts/admin-smoke-package-20260513-04210`, `0.42.10-admin-smoke`, `artifacts/manual-admin-campaign-20260513-0429-04210/lifecycle/product-update-rollback/04-wrapper-repair-installed-batch-root.json`; target MSI SHA-256 `bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273`, update ZIP SHA-256 `05a107f4803ec8ed1e08f7aeba1b49fa3795c7d16565db8f904fd599ba07633f`, provenance `d7d5ba38ee1d4f74676477eb13701af65abca008`; native service-action 이후 duplicate `sc.exe start` 1056 historical RCA, `0.42.11-admin-smoke` `native-service-action-controls-final-state`로 닫힘 |
| Manual-admin 0427→0428 historical campaign 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`, `artifacts/manual-admin-campaign-20260512-0427-0428/manual-admin-campaign-descriptor/summary.json`, `artifacts/manual-admin-campaign-20260512-0427-0428/clean-host/summary.json`, `artifacts/manual-admin-campaign-20260512-0427-0428/burn-bootstrapper-lifecycle/burn-lifecycle-summary.json`, `artifacts/msix-package-lifecycle-smoke-20260512-0427-0428/summary.json`; result `PASS`; 0429→04211 PASS 이후 historical predecessor |
| Post-0426 후속 triage 확인 | `docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md`, post-merge MSI SHA-256 `9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16`, Batch Supervisor `ManualAdminCampaignDescriptor` profile, helper `packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`; 사용자 승인 후 `0.42.7-admin-smoke` build/full admin host mutation gate/current-card smoke를 실행했고, 추가 승인으로 0427→0428 package-pair와 0428 full gate까지 PASS 완료 |
| Batch evidence root service-action package 확인 | `docs/ga-ready/evidence/batch-evidence-root-service-action-package-2026-05-13-0429.md`, `artifacts/admin-smoke-package-20260513-0429`, `0.42.9-admin-smoke`, MSI SHA-256 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit `f0620f2e18ae25de8751333684cb74b5051dcdc6`; Event Log default transition timeout guard와 installer timeout propagation, 04211 이후 historical package evidence, public trusted signing 또는 외부 stable publication evidence 아님 |
| Manual-admin 0428→0429 candidate 확인 | `docs/ga-ready/evidence/manual-admin-campaign-candidate-2026-05-13-0428-0429.md`, `artifacts/manual-admin-campaign-20260513-0428-0429`, update ZIP SHA-256 `7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f`; installed update/rollback only, clean-host/Burn/MSIX/descriptor는 아직 PASS claim 아님 |
| Public distribution ops execution bundle 확인 | `docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionOperationsBundle.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Public distribution readiness preflight 확인 | `docs/ga-ready/evidence/public-distribution-readiness-preflight-2026-05-07.md`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Winget manifest compliance preflight 확인 | `docs/ga-ready/evidence/winget-manifest-compliance-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Winget CLI validate 확인 | `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`, `artifacts/winget-cli-validate-20260509-0391/summary.json`, `packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1` |
| Public external gates blocked scan 확인 | `docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md`, `artifacts/public-external-gates-blocked-20260509-0391/summary.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Public ops final 1-7 follow-up attempt 확인 | `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `artifacts/public-ops-final-followup-attempt-20260509-0391/summary.json`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1` |
| Public ops gate execution readiness 확인 | `docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md`, `artifacts/public-ops-gate-execution-readiness-20260509-0392/summary.json`, `packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1`, `partial-code-level-cert-generate-rotate-delete-pass`, public trusted signing/external stable publication not claimed |
| Public ops installed hardening code-level 확인 | `docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md`, `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`, `src/DesktopNode.Host/DesktopNodeWindowsCredentialManagerController.cs`, `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs` |
| Updater catalog publication preflight 확인 | `docs/ga-ready/evidence/updater-catalog-publication-preflight-2026-05-07.md`, `packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Public signed update/rollback smoke preflight 확인 | `docs/ga-ready/evidence/public-signed-update-rollback-smoke-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Windows Credential Manager transition preflight 확인 | `docs/ga-ready/evidence/windows-credential-manager-transition-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Windows Credential Manager transition capability smoke 확인 | `docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md`, `artifacts/windows-credential-manager-transition-20260509-0391/summary.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Windows Credential Manager default transition installed smoke 확인 | `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, `artifacts/windows-credential-manager-default-transition-installed-20260510-0395/summary.json`, `packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Windows Event Log provider transition preflight 확인 | `docs/ga-ready/evidence/windows-event-log-provider-transition-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Windows Event Log provider/default writer transition smoke 확인 | `docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md`, `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, `artifacts/windows-event-log-default-transition-installed-20260510-0396/summary.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Built-in TLS certificate lifecycle preflight 확인 | `docs/ga-ready/evidence/builtin-tls-certificate-lifecycle-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Service token rotation/revoke preflight 확인 | `docs/ga-ready/evidence/service-token-rotation-revoke-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Diagnostic bundle server-side preflight 확인 | `docs/ga-ready/evidence/diagnostic-bundle-server-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Diagnostic bundle server code-level 확인 | `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`, `src/DesktopNode.Api.Tests/ApiDiagnosticBundleRequestProcessorTests.cs`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Diagnostic bundle Host listener code-level 확인 | `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`, `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`, `src/DesktopNode.Host/DesktopNodeHostApplication.cs` |
| Diagnostic bundle product wrapper code-level 확인 | `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`, `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` |
| Diagnostic bundle native service-action config code-level 확인 | `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`, `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`, `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` |
| Diagnostic bundle MSI/service installed listener 확인 | `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/installed-diagnostic-bundle-listener-smoke.json`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/installed-service-listener-post-rerun.json` |
| Installed listener OS mutation gate 확인 | `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`, `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390/summary.json`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390/summary.json` |
| Timeout/rate-limit hardening preflight 확인 | `docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md`, `packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Timeout/rate-limit hardening code-level 확인 | `docs/ga-ready/evidence/timeout-rate-limit-hardening-code-level-2026-05-08.md`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, `src/DesktopNode.Host/DesktopNodeHostApplication.cs` |
| Timeout/rate-limit route-timeout code-level 확인 | `docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs` |
| Timeout/rate-limit server-config code-level 확인 | `docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`, `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1` |
| Timeout/rate-limit load-test code-level 확인 | `docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md`, `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Installed listener external load/rate-limit smoke 확인 | `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`, `artifacts/installed-listener-external-load-rate-limit-20260509-0391/summary.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Burn bootstrapper preflight 확인 | `docs/ga-ready/evidence/burn-bootstrapper-preflight-2026-05-07.md`, `packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| Burn bootstrapper lifecycle smoke 확인 | `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/burn-bootstrapper-lifecycle-20260510-0416/summary.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| MSIX packaging feasibility preflight 확인 | `docs/ga-ready/evidence/msix-packaging-feasibility-preflight-2026-05-07.md`, `packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| MSIX package lifecycle smoke 확인 | `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/msix-package-lifecycle-smoke-20260510-0416/summary.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| MSI/update package apply 확인 | `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`, `artifacts/msi-update-package-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.provenance.json`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` |
| 자동 non-mutating regression batch 확인 | `docs/ga-ready/evidence/auto-nonmutating-regression-batch-2026-05-09.md`, `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232/summary.json`, `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1` |
| 자동 후속 작업 처리 확인 | `docs/ga-ready/evidence/automatic-followup-processing-2026-05-11.md`, `artifacts/batch-runs/auto-nonmutating-regression-20260511-041415/summary.json`, `artifacts/installed-tui-operator-smoke-20260511-042008/summary.json` |
| 후속 작업/자동 배치 작업 분류 확인 | `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`, `docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md`, `docs/ga-ready/evidence/auto-nonmutating-regression-batch-2026-05-09.md`, `packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1` |
| Phase 19 제품 승격 재판정 확인 | `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md` |
| Phase 20 signed release/MSI lifecycle evidence 확인 | `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md` |
| Phase 21 Hyper-V lifecycle integration evidence 확인 | `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence.md` |
| Phase 22 release/version policy와 installer artifact contract 확인 | `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md` |
| 내부 서비스 signing trust model 확인 | `docs/adr/0003-internal-trusted-signing-policy.md`, `packaging/windows-desktop-node/installer/README.md` |
| Phase 23 Windows operational evidence 확인 | `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence.md` |
| Post-reboot verification dry-run/runner evidence 확인 | `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-post-reboot-verification.md`, `packaging/windows-desktop-node/README.md` |
| Draft PR ready gate 확인 | `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-draft-pr-ready-gate.md` |
| Phase 24 Local API job runtime boundary 후보 확인 | `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`, `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md` |
| Phase 25 .NET/TypeScript 전환 후보 확인 | `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`, `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md` |
| Phase 25 TypeScript Web Console 경계 후보 확인 | `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md` |
| Phase 25 Web Console browser fixture parity 확인 | `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-web-console-browser-fixture-parity.md` |
| Phase 25 Web Console served asset/root migration 확인 | `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-web-served-asset-root-migration.md` |
| Single Edge frontend service import 분석 | `docs/superpowers/specs/2026-05-08-purecvisor-desktop-node-single-edge-frontend-service-import-analysis.md`, `web/DESIGN.md`, `D:\data\projects\codex-zone\purecvisor-single\DESIGN.md`, `web/src/served-app.ts` |
| Single Edge 차용 범위 map 확인 | `docs/superpowers/specs/2026-05-10-purecvisor-desktop-node-single-edge-borrowing-map.md`, `web/DESIGN.md`; 당시 TUI 차용 대상은 제거됐으며 `docs/adr/0011-cli-web-only-operator-surface.md`가 현재 경계를 소유함 |
| Web Console Single UI clone/staged frontend service 확인 | `docs/ga-ready/evidence/web-console-single-ui-clone-2026-05-09.md`, `web/index.html`, `web/styles.css`, `web/src/served-app.ts`, `web/src/served/` |
| Web Console installed listener QA 확인 | `docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md`, `artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b/summary.json`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-frontend.md` |
| Web Console destructive lifecycle UI 확인 | `docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md`, `artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391/summary.json`, `web/scripts/capture-destructive-lifecycle-ui-qa.mjs` |
| Web Console beta follow-up status 확인 | `docs/ga-ready/evidence/web-console-beta-followup-status-2026-05-09.md`, `web/src/served-app.ts`, `web/scripts/verify-browser-fixture.mjs` |
| Web/API port split code-level/installed 확인 | `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`, `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`, `src/DesktopNode.Host/DesktopNodeHostApplication.cs`, `src/DesktopNode.Host/DesktopNodeHostOptions.cs`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`, `web/index.html`, `web/src/served/state.ts` |
| Account/RBAC/JWT/console code-level 확인 | `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-account-rbac-jwt-console.md`, `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`, `src/DesktopNode.Api/DesktopNodeAccountAuth.cs`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`, `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`, `src/DesktopNode.Host/DesktopNodeHostApplication.cs`, `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`, `web/src/served/routes.ts`, `web/src/served-app.ts` |
| Installed account login/noVNC bridge 확인 | `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`, `packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1`, `packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`, `web/scripts/capture-installed-listener-qa.mjs`, `src/DesktopNode.Host/DesktopNodeHostApplication.cs`, `src/DesktopNode.Host/DesktopNodeHostOptions.cs` |
| Frontend/backend auth console live smoke 확인 | `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`, `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`, `artifacts/installed-web-asset-refresh-20260510-235258`; installed Web Console real account login form, auth/session/RBAC/console route, diagnostic create/download, responsive screenshot PASS |
| 0.41.5 manual-admin operator/hardening follow-up 확인 | `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `artifacts/manual-admin-followup-20260510-0415/summary.json`, `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md` |
| 0.41.8 to 0.41.9 manual-admin 1-2-3-4 캠페인 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-11-0418-0419.md`, `artifacts/manual-admin-campaign-20260511-0418-0419/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260511-0418-0419/summary.json` |
| 0.42.0 to 0.42.1 manual-admin 1-2-3-4 캠페인 확인 | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-11-0420-0421.md`, `artifacts/manual-admin-campaign-20260511-0420-0421/summary.json`, `artifacts/msix-package-lifecycle-smoke-20260511-0420-0421/summary.json` |
| guide 기반 운영/확장 backlog 확인 | `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md` |
| guide 기반 운영 콘솔 P0/P1/P2 확장 확인 | `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-operator-ops-console-expansion-design.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p0.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p1.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p2.md` |
| Web Dashboard Ops Cockpit 재설계 확인 | `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-web-dashboard-ops-cockpit-redesign-design.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-web-dashboard-ops-cockpit-redesign.md` |
| Web Dashboard Network Inventory view 확인 | `docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md`, `web/src/served-app.ts`, `web/scripts/verify-browser-fixture.mjs` |
| Web Dashboard Diagnostic Bundle UI 확인 | `docs/ga-ready/evidence/web-console-diagnostic-bundle-ui-2026-05-07.md`, `web/src/served-app.ts`, `web/scripts/verify-browser-fixture.mjs` |
| Diagnostic Bundle list pagination/retention hardening 확인 | `docs/ga-ready/evidence/diagnostic-bundle-list-pagination-retention-2026-05-09.md`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, `web/src/served-app.ts` |
| API/Web retention pagination hardening 확인 | `docs/ga-ready/evidence/api-web-retention-pagination-hardening-2026-05-07.md`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, `web/src/served-app.ts` |
| Web Dashboard Token Rotation UX 확인 | `docs/ga-ready/evidence/web-console-token-rotation-ux-2026-05-07.md`, `web/src/served-app.ts`, `web/scripts/verify-browser-fixture.mjs` |
| Service token rotation/revoke installed smoke 확인 | `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`, `artifacts/service-token-rotation-revoke-installed-20260509-150334/summary.json`, `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` |
| guide 기반 API operations hardening 확인 | `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-api-operations-hardening-design.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-api-operations-hardening.md` |
| guide 기반 VM delete UI 구현 확인 | `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-vm-delete-ui-design.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-vm-delete-ui.md` |
| config/job store migration apply 실제 적용 경로 확인 | `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-product-config-migration-apply-plan-only.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-job-store-migration-apply-plan-only.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-code-level-2026-05-06.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-preflight-blocked-2026-05-06.md`, `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md` |
| .NET Windows Service Host replacement 확인 | `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`, `docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md` |
| .NET native adapter slice 확인 | `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-dotnet-native-network-inventory-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-list-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-detail-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-list-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-power-state-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-mutation-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-checkpoint-restore-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-create-shutdown-restart-native-adapter.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-vm-delete-native-adapter.md` |
| Service/product ops 및 job store 후속 slice 확인 | `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-dotnet-service-status-start-stop.md`, `docs/superpowers/plans/2026-05-04-purecvisor-desktop-node-data-root-remove-handoff.md`, `docs/superpowers/plans/2026-05-03-purecvisor-desktop-node-job-store-atomic-save.md` |
| GA-ready 제품 재설계 결정 확인 | `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md` |
| 내부 전용 GA-ready 제품 런타임 결정 확인 | `docs/adr/0004-ga-ready-product-runtime-candidate.md`, `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md` |
| Operational full admin host mutation gate evidence 확인 | `docs/ga-ready/current-evidence.json`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md`; exact version/MSI/payload/provenance는 canonical JSON을 우선하며 public trusted signing 또는 외부 stable publication evidence가 아님 |
| 이전 full admin host mutation gate evidence 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415/summary.json`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415/summary.json`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415/summary.json` |
| Frontend installed listener QA evidence 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-frontend.md`, `docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md`, `docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-130105-0391-frontend-final2/summary.json` |
| 최신 InternalEnterprise RequireSigned MSI build evidence 확인 | `docs/ga-ready/evidence/host-mutation-signed-build-attempt-2026-05-07-0387.md`, `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387/PureCVisorDesktopNode-0.38.7-rc.1-windows-x64.provenance.json` |
| Batch 후속 closure 확인 | `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`, `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch1-0382-canonical-evidence-closure.md`, `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch-supervisor-evidence-ux-api-foundation.md`, `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch2-batch-evidence-api-hardening.md`, `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch3a-evidence-dashboard-surface.md`, `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch3b-troubleshooting-polish.md` |
| P2 100% 완료 디자인 목업 패키지 확인 | `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-p2-final-mockup-copy-package-design.md` |
| Product wrapper 사용법 | `packaging/windows-desktop-node/README.md` |
| 사용자 기능 사용 명세 | `docs/USER_FEATURE_USAGE_SPEC.md`, `docs/USER_GUIDE.md`, `docs/CLI_COMMAND_USAGE.md` |
| Root component boundary | `archive/spikes/purecvisor-desktop-node/README.md` |
| Local API | `archive/spikes/purecvisor-desktop-node/api/README.md` |
| Active .NET CLI | `src/DesktopNode.Cli/`, `src/DesktopNode.Cli/README.md`, `docs/CLI_COMMAND_USAGE.md`, `docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-active-dotnet-cli.md` |
| Active TypeScript Web Console | `web/`, `web/src/served-app.ts`, `web/package.json` |
| Historical .NET TUI predecessor (active 아님) | `docs/adr/0011-cli-web-only-operator-surface.md`, `docs/superpowers/specs/2026-05-10-purecvisor-desktop-node-product-tui-service-design.md`, `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-product-tui-service.md`, `docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md` |
| Archived PowerShell CLI baseline | `archive/spikes/purecvisor-desktop-node/cli/README.md` |
| Hyper-V helper | `archive/spikes/purecvisor-desktop-node/hyperv/README.md` |
| Service helper | `archive/spikes/purecvisor-desktop-node/service/README.md` |

## 저장소 결정

```text
DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike
DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned
DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT: dotnet-windows-service-host-default-with-keep-spike
DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start
DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_READ_PARITY_GUARD: network-inventory-vm-list-vm-detail-and-checkpoint-list-native-structured-failure-on-incomplete-parity
DESKTOP_NODE_PHASE25_NATIVE_CHECKPOINT_MUTATION_START: checkpoint-create-restore-delete-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_VM_LIFECYCLE_MUTATION_START: vm-create-shutdown-restart-dotnet-native-adapter
DESKTOP_NODE_PHASE25_NATIVE_VM_DELETE_MUTATION_START: vm-delete-dotnet-native-adapter
DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime
DESKTOP_NODE_PUBLIC_DISTRIBUTION_DECISION_CANDIDATE: closed-not-adopted
DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only
```

이 저장소는 Linux `purecvisor-single` 저장소와 분리되어 있으며 Windows Desktop Node 코드와 문서만 포함한다. 현재 적용되는 설계 결정의 진입점은 `docs/ADR_INDEX.md`다.

ADR-0005는 미채택/종료 상태다. `public-distribution-operations-expansion-candidate` 범위는 `PUBLIC_DISTRIBUTION_GATE_MATRIX`와 `New-PcvPublicDistributionDescriptor.ps1` dry-run descriptor로 보존하지만 public trusted signing, timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke는 ADR-0006 기준 `out-of-scope`다.

ADR-0006은 현재 적용되는 내부 사설망 전용 배포 결정이다. `INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX`는 internal signed MSI, internal updater catalog/channel, private LAN smoke, internal HTTPS/TLS lifecycle installed smoke, internal clean-host install/update/rollback smoke를 현재 gate로 추적한다. HTTPS/TLS installed smoke는 `internal-https-tls-lifecycle-installed-2026-05-10-0397`에서 `generate-bind-rotate-remove-pass`로 닫혔고, clean-host install/update/rollback은 `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417`에서 dedicated Hyper-V clean-host PASS로 닫혔다.
`public-distribution-readiness-preflight` 후속은 `New-PcvPublicDistributionReadiness.ps1`가 packaging publication descriptor에서 winget manifest preview와 `winget validate` manual follow-up을 산출하는 범위다. `winget_submission`은 `not-submitted`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`winget-manifest-compliance-preflight` 후속은 `New-PcvWingetManifestCompliancePreflight.ps1`가 생성된 winget singleton manifest preview를 offline compliance로 검증하는 범위다. `validation_status: offline-compliance-pass`, `winget_submission: not-submitted`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`updater-catalog-publication-preflight` 후속은 `New-PcvUpdaterCatalogPublicationPreflight.ps1`가 updater catalog schema v1에서 selected HTTPS channel을 읽어 catalog publication preview를 산출하는 범위다. `catalog_publication: not-published`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`public-signed-update-rollback-smoke-preflight` 후속은 `New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1`가 selected catalog channel에서 clean-host smoke plan preview를 산출하는 범위다. `public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication`, `clean_host_smoke_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`windows-credential-manager-transition-preflight` 후속은 `New-PcvWindowsCredentialManagerTransitionPreflight.ps1`가 service name, credential target, 현재 DPAPI protected token file storage, 목표 Windows Credential Manager storage, transition plan preview를 산출하는 범위다. 후속 `windows-credential-manager-transition-2026-05-09-0391` evidence는 current-user Credential Manager write/read/delete capability PASS와 installed service `LocalSystem` context blocker를 기록했고, `public-ops-installed-hardening-code-level-2026-05-09-0393`는 native `credential-manager-system-proof` runner를 추가했다. 최신 `windows-credential-manager-default-transition-installed-2026-05-10-0395` evidence는 MSI deferred LocalSystem custom action으로 `credential_manager_system_context_proof: installed-local-system-proof-pass`, `service_credential_manager_default_transition: installed-admin-smoke-pass`, `token_source_migration: protected-file-to-credential-manager`, `service_reload_status: restarted`, `old_source_rejection_status: protected-file-source-rejected-after-reload`, `rollback_diagnostics_status: written`, `token_value_observed: false`를 기록한다.
`windows-event-log-provider-transition-preflight` 후속은 `New-PcvWindowsEventLogProviderTransitionPreflight.ps1`가 service name, provider name, log name, 현재 JSONL-first/Event Log opt-in writer policy, 목표 default Windows Event Log provider writer, provider transition plan preview를 산출하는 범위다. 후속 `windows-event-log-provider-default-transition-2026-05-09-0391` evidence는 installed native provider registration과 event id `39100` write/query를 PASS로 기록한다. `public-ops-installed-hardening-code-level-2026-05-09-0393`는 native `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard`를 추가했고, 최신 `windows-event-log-default-transition-installed-2026-05-10-0396` evidence는 MSI deferred LocalSystem `eventlog-default-transition`으로 `event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass`, `event_log_default_writer: installed-admin-smoke-pass`, `event_log_schema_version: 1`을 기록한다.
`builtin-tls-certificate-lifecycle-preflight` 후속은 `New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1`가 service name, certificate subject, HTTPS bind prefix, 현재 TLS mode, 목표 built-in service certificate mode, TLS lifecycle plan preview를 산출하는 범위다. 최신 matrix 상태는 `tls_certificate_lifecycle: partial-code-level-cert-generate-rotate-delete-pass`, `tls_certificate_mutation: not-run`, `tls_binding: not-run`, `trust_store_mutation: not-run`, `lan_binding_mutation: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication `not-claimed`다.
`service-token-rotation-revoke-preflight` 후속은 `New-PcvServiceTokenRotationRevokePreflight.ps1`가 service name, protected token path, 현재 DPAPI protected token file storage, rotation mode, service token rotation revoke plan preview를 산출하는 범위다. 후속 installed-admin smoke는 `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`, `artifacts/service-token-rotation-revoke-installed-20260509-150334`에서 `service_token_rotation_revoke: installed-admin-smoke-pass`, `service_token_mutation: performed`, `token_value_observed: false`, `new_token_value_created: true`, `service_reload_status: restarted`, `old_token_rejection_status: old-token-rejected-after-reload`, `token_rotation_audit_status: written`, `host_mutation_performed: true`를 기록한다. Public trusted signing/external stable publication은 `not-claimed`다.
`diagnostic-bundle-server-preflight` 후속은 `New-PcvDiagnosticBundleServerPreflight.ps1`가 service name, diagnostics root, Local API generation route, download route template, bearer authorization policy, redaction policy, retention policy, diagnostic bundle server-side plan preview를 산출하는 범위다. `diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight`, `diagnostic_bundle_api_action: not-run`, `diagnostic_bundle_archive_created: false`, `diagnostic_bundle_download_served: false`, `diagnostic_bundle_redaction_status: not-run`, `diagnostic_bundle_authz_status: not-run`, `diagnostic_bundle_retention_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`diagnostic-bundle-server-code-level` 후속은 `DesktopNodeApiRequestProcessor`가 `POST /api/v1/diagnostics/bundles`에서 redacted `.bundle.json`을 만들고 `GET /api/v1/diagnostics/bundles/{bundle_id}/download`에서 다운로드로 제공하는 범위다. `DesktopNodeHostOptions`와 `PcvDesktopNodeProduct.psm1`는 `--diagnostics-root`를 service plan에 연결한다. `diagnostic_bundle_server_generation: partial-code-level-api-action`, `diagnostic_bundle_api_action: code-level-applied`, `diagnostic_bundle_archive_created: code-level-created`, `diagnostic_bundle_download_served: code-level-download-served`, `diagnostic_bundle_redaction_status: code-level-applied`, `diagnostic_bundle_authz_status: token-required-route-contract`, `diagnostic_bundle_retention_status: code-level-applied`이지만 installed listener/product wrapper delegation/host mutation/public trusted signing/external stable publication은 `not-claimed`다.
`diagnostic-bundle-listener-code-level` 후속은 `DesktopNodeHostApplication`이 `X-PCV-Request-Id`/`X-Request-Id`를 API processor로 전달하고, in-process `HttpListener` 경유 bearer-required create/download를 확인하는 범위다. `diagnostic_bundle_host_listener_execution: code-level-host-listener`, `diagnostic_bundle_request_id_propagation: code-level-host-header`이지만 이 code-level evidence 자체는 installed service listener execution, host mutation, public trusted signing/external stable publication을 `not-claimed`로 둔다.
`diagnostic-bundle-product-wrapper-code-level` 후속은 `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics`가 `New-PcvDesktopNodeDiagnosticBundle`로 위임되고 `product-wrapper-delegation-redacted.json`을 기록하는 범위다. `diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator`, `actual_execution: code-level-product-wrapper`, `host_mutation_performed: false`이며 installed service listener PASS는 별도 elevated rerun evidence가 소유한다.
`diagnostic-bundle-native-service-action-config-code-level` 후속은 `DesktopNode.Host.exe service-action configure-installed|repair-installed` C# native SCM config가 `--diagnostics-root`, protected token file, route timeout, request limit, burst, retry-after 인자를 `BinaryPathName`에 포함하는지 확인하는 범위다. 0.38.9 artifact의 installed final `PathName`은 아직 이 인자들을 포함하지 않았지만, `0.39.0-admin-smoke` elevated MSI/service rerun에서 installed service listener execution은 `installed-listener-pass`, blocker는 `none`으로 닫혔다.
`timeout-rate-limit-hardening-preflight` 후속은 `New-PcvTimeoutRateLimitHardeningPreflight.ps1`가 service name, Local API route prefix, route timeout target, request limit target, retry-after target, UI/API error contract, timeout/rate-limit hardening plan preview를 산출하는 범위다. `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`timeout-rate-limit-hardening-code-level` 후속은 `DesktopNodeApiRequestProcessor`와 `DesktopNodeHostApplication`가 `/api/v1/` per-client request window, HTTP 429, `Retry-After`, `application/problem+json`, `PCV_RATE_LIMIT_EXCEEDED`를 적용하는 범위다. `timeout_rate_limit_hardening: partial-code-level-request-limit`, `route_timeout_policy: not-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`timeout-rate-limit-hardening-route-timeout-code-level` 후속은 `DesktopNodeApiRequestProcessor`가 `/api/v1/` GET/read route response deadline을 적용하고 초과 시 HTTP 504, `Retry-After`, `application/problem+json`, `PCV_ROUTE_TIMEOUT`, `route_timeout_seconds`, `request_id`를 반환하는 범위다. 기존 request-limit code-level path는 유지되며 `timeout_rate_limit_hardening: partial-code-level-route-and-request-limit`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`timeout-rate-limit-hardening-server-config-code-level` 후속은 `PcvDesktopNodeProduct.psm1` product service plan과 C# native service-action config가 `DesktopNode.Host.exe listen`에 `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`를 싣고 `service.hardening`/SCM `BinaryPathName`에서 같은 값을 노출하는 범위다. Installed service mutation, service stop/start, load test, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: not-run`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, public trusted signing/external stable publication은 `not-claimed`다.
`timeout-rate-limit-hardening-load-test-code-level` 후속은 `ApiHardeningRequestProcessorTests`가 같은 client identity 64개 in-process request load를 실행해 HTTP 200 `20`, HTTP 429 `44`, unexpected status `0`, `PCV_RATE_LIMIT_EXCEEDED` problem-details contract를 확인하는 범위다. Installed listener load, external load generator, service mutation, host mutation은 실행하지 않으며 `timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load`, `route_timeout_policy: code-level-applied`, `request_limit_policy: code-level-applied`, `retry_semantics_status: retry-after-problem-details-code-level`, `ui_api_error_contract_status: problem-details-json-code-level`, `load_test_status: code-level-inprocess-pass`, `server_config_mutation: code-level-product-and-native-service-plan-applied`, public trusted signing/external stable publication은 `not-claimed`다.
`burn-bootstrapper-preflight` 후속은 `New-PcvBurnBootstrapperPreflight.ps1`가 packaging publication descriptor와 HTTPS MSI URL에서 WiX Burn authoring preview를 산출하는 범위다. 후속 `burn-bootstrapper-lifecycle-smoke-2026-05-10-0416` evidence는 actual Burn bundle build/install/repair/remove와 direct MSI restore를 PASS로 기록하므로 `burn_bootstrapper: build-install-repair-remove-pass-internal-smoke`, `host_mutation_performed: true`, public trusted signing/external stable publication `not-claimed`를 유지한다.
`msix-packaging-feasibility-preflight` 후속은 `New-PcvMsixPackagingFeasibilityPreflight.ps1`가 packaging publication descriptor에서 MSIX package manifest preview를 산출하는 범위다. `msix: feasibility-blocked-by-service-packaging-design`, `actual_execution: not-run`, `host_mutation_performed: false`, public trusted signing/external stable publication은 `not-claimed`다.
`msix-package-lifecycle-smoke` 후속은 `0.41.5-admin-smoke` baseline payload and `0.41.6-admin-smoke` target payload에서 internal MSIX package build/sign/verify, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence를 확인한 관리자 opt-in evidence다. `msix: build-install-update-remove-pass-internal-smoke`, `host_mutation_performed: true`, `public_trusted_signing: excluded`, `external_stable_publication: not-claimed`이며 public trusted signing/external stable publication evidence가 아니다.
`msi-update-package-apply` 후속은 `0.39.1-admin-smoke` AllowUnsignedDev MSI/update package apply를 확인한 관리자 opt-in evidence다. `artifacts/msi-update-package-20260509-0391`에서 MSI build, update ZIP/catalog validation, elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 기록한다. `internal_msi_update_package_apply: pass-internal-admin-smoke`, `host_mutation_performed: true`, `public_trusted_signing: excluded`, `external_stable_publication: not-claimed`이며 public trusted signing/external stable publication evidence가 아니다.

현재 runtime 전환 상태:

- Phase 24 후보는 ADR 채택 전 단계의 Local API job runtime 경계 안정화 작업이며, 제품 승격이나 C++23 전환 결정을 의미하지 않는다.
- Phase 25 후보는 ADR 채택 전 단계의 .NET/TypeScript 전환 경계 작업이다.
- 2026-05-01 replacement slice에서 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner를 `DesktopNode.Host.exe`로 교체했다.
- `src/DesktopNode.Api/**`는 queued job runtime, job store save/load/recovery, native read adapter, VM create/start/shutdown/poweroff/restart/delete native lifecycle adapter, checkpoint create/restore/delete native mutation adapter를 포함한다.
- `GET /api/v1/host/status`는 C# registry/WMI/service/admin native read adapter가 처리한다.
- `GET /api/v1/network/inventory`는 C# native WMI read adapter가 직접 처리하며 switch type, management OS, external adapter field를 보존하지 못하면 native structured failure를 반환한다.
- `GET /api/v1/vms`는 C# native WMI read adapter가 직접 처리한다. Empty inventory는 유효한 success이며, VM identity/state, CPU/startup memory/generation/checkpoint count, storage/network field parity가 불완전하면 PowerShell helper fallback 없이 native structured failure를 반환한다.
- `GET /api/v1/vms/{id}`는 native `vm.list` 결과에서 VM을 찾고, missing VM 또는 native inventory failure를 helper 재시도 없이 반환한다.
- `GET /api/v1/vms/{id}/checkpoints`는 native VM inventory와 WMI snapshot association을 사용하며 VM/checkpoint parity failure를 helper 재시도 없이 반환한다.
- `POST /api/v1/vms/{id}/start`, `POST /api/v1/vms/{id}/shutdown`, `POST /api/v1/vms/{id}/poweroff`, `POST /api/v1/vms/{id}/restart`는 .NET request processor queue를 유지하되 C# WMI `Msvm_ComputerSystem.RequestStateChange` adapter가 직접 실행한다. PowerShell helper fallback은 사용하지 않는다.
- `POST /api/v1/vms`는 native VM create adapter가 처리한다. 이번 native product path는 Hyper-V Generation 2 create만 지원하고 Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다.
- `DELETE /api/v1/vms/{id}`는 native VM delete adapter가 처리한다. Managed VM은 C# WMI `DestroySystem` adapter로 삭제하고, missing VM은 `action=absent`, unmanaged VM은 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 반환한다. `0.30.1-admin-smoke` installed destructive smoke가 managed delete, repeat absent, unmanaged guard block, cleanup/no-reboot evidence를 확인했다.
- `POST /api/v1/vms/{id}/checkpoints`, `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore`, `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}`는 .NET request processor queue를 유지하되 C# WMI snapshot service adapter가 직접 실행한다. PowerShell helper fallback은 사용하지 않는다.
- Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD guest 기준 installed Local API `vm.shutdown` job `succeeded`, final VM `Off`, cleanup 완료로 확인했다.
- Web Console served `web/app.js`는 `web/src/served-app.ts` TypeScript build output이며, `npm run verify:parity --prefix web`가 served freshness와 Node `vm` browser fixture smoke를 함께 실행한다. `Network` 화면은 `GET /api/v1/network/inventory`를 read-only로 표시하며 switch type/default/management OS/external adapter field를 fixture와 browser smoke로 검증한다. `Troubleshooting` 화면의 Diagnostic Bundle 패널은 server-side bundle API create/download와 product wrapper fallback 안내, diagnostics root/redaction boundary를 표시하며, host mutation은 실행하지 않는다.
- Web/API listener 기본값은 Web Console `http://127.0.0.1/`, Web API `http://127.0.0.1:7777/api/v1/...` 분리다. Host가 `/pcv-config.js`로 API origin을 주입하고 Web listener의 `/api/*`는 `PCV_API_ROUTE_ON_WEB_PORT`로 거부한다. `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`는 설치본 `0.39.2-port-split-smoke` payload와 service `PathName` `--web-prefix "http://127.0.0.1:80/"` 적용, Web/API HTTP smoke PASS를 기록한다. 기본 Web Console은 HTTP loopback이고, internal HTTPS/TLS lifecycle installed smoke는 별도 ADR-0006 evidence로 PASS다. Public 443 publication은 scope 밖이다.
- `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`는 사용자 재승인으로 `0.35.7-admin-smoke` Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate를 확인한 historical evidence다. Final service는 loopback-only `Running`, installed DisplayVersion은 `0.35.7`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present다. Public trusted signing은 제외했고, evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store 범위다.
- `artifacts/routeparity-service-msi-hyperv-dotnet100-20260505-0.36.0`는 후속 active product .NET 100% cleanup Service/MSI/Hyper-V route parity rerun이다. Final service는 loopback-only `Running`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이번 rerun 범위가 아니다.
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`는 `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity evidence다. Batch Supervisor는 timeout 없이 완료됐고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다.
- `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`는 `0.38.7-rc.1` 최신 internal enterprise `RequireSigned` MSI build evidence다. MSI SHA-256은 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, provenance commit은 `dd4e7379c515b05eb82038404519c9e63f54bf51`, signing trust model은 `InternalEnterprise`, Authenticode는 `Valid`, SignTool verify exit는 `0`이다. 이 evidence는 ADR-0003 internal trust 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
- 이전 `0.38.4-rc.1` signed build evidence는 `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384`에 historical evidence로 보존한다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429`, `artifacts/os-mutation-gates-batch-profile-20260513-040213-0429`는 `0.42.9-admin-smoke` 이전 full admin host mutation gate evidence다. provenance commit은 `f0620f2e18ae25de8751333684cb74b5051dcdc6`, full-gate MSI SHA-256은 `78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9`, package MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, signing mode는 `AllowUnsignedDev`다. Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고 installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260513-040213-0429`를 확인했다. final service는 Web `80`/API `7777` split 상태로 `Running`, installed manifest `0.42.9-admin-smoke`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, API unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-233650-0428-r2`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2`, `artifacts/os-mutation-gates-batch-profile-20260512-233650-0428-r2`는 `0.42.8-admin-smoke` 이전 full admin host mutation gate evidence로 보존한다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`는 `0.42.7-admin-smoke` 이전 full admin host mutation gate evidence다. provenance commit은 `8d6aea7bac30ce279093ec61406c62428f69e79c`, full-gate MSI SHA-256은 `9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4`, package build MSI SHA-256은 `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9`, signing mode는 `AllowUnsignedDev`다. Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 timeout 없이 완료했고 installed listener current-card smoke는 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260512-181309-0427`을 확인했다. final service는 Web `80`/API `7777` split 상태로 `Running`, installed manifest `0.42.7-admin-smoke`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, API unauthenticated boundary `401`/`PCV_AUTH_REQUIRED`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다. `0.42.3-admin-smoke` / 0423 evidence와 `0.42.2-admin-smoke` / 0422 evidence는 historical evidence로 보존한다.
- `artifacts/admin-smoke-package-20260512-0424`와 `artifacts/manadm-0424/lifecycle/product-update-rollback`는 `0.42.3 -> 0.42.4` manual-admin package-pair historical blocker evidence다. Full admin host mutation, Operator Access, Internal Service Hardening, installed update/rollback은 PASS였고, dedicated clean-host는 `0.42.3` baseline MSI custom action sequence 때문에 blocked였다. Current package-pair claim은 0427→0428 PASS evidence가 소유한다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/manual-admin-campaign-20260512-0427-0428`는 `0.42.7 -> 0.42.8` manual-admin package-pair current PASS evidence다. Installed update/rollback, dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation이 PASS다. Target post-merge rebuild `artifacts/admin-smoke-package-20260512-0428-postmerge`는 MSI SHA-256 `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, provenance commit `5397e580c98a34e8b7beb5b9773d1d857025315b`로 보존한다. 다음 descriptor manifest는 `New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`가 `manual-admin-campaign-descriptor-20260512-0427-0428`로 생성한다.
- `artifacts/manual-admin-campaign-20260513-0428-0429`는 `0.42.8 -> 0.42.9` manual-admin package-pair candidate evidence다. Installed update/rollback은 PASS지만 clean-host/Burn/MSIX/descriptor는 아직 PASS claim이 아니며, target MSI SHA-256은 `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, update ZIP SHA-256은 `7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f`다.
- `artifacts/manual-admin-campaign-20260512-0425-0426`는 `0.42.5 -> 0.42.6` manual-admin package-pair historical predecessor PASS evidence다. Installed update/rollback, dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture가 PASS다. Post-merge rebuild `artifacts/admin-smoke-package-20260512-0426-postmerge`는 MSI SHA-256 `9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16`, provenance commit `37f4d6b83d6caef1338e0a60e5df0a60209b51f8`로 보존한다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`는 `0.41.5-admin-smoke` 이전 full admin host mutation gate evidence로 보존한다. MSI provenance commit은 `c9efe852db0e3fb4d120bc5058c56a38c7cb30db`, MSI SHA-256은 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`, signing mode는 `AllowUnsignedDev`다. Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 timeout 없이 완료했고 final service는 Web `80`/API `7777` split 상태로 `Running`, installed manifest `0.41.5-admin-smoke`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/manual-admin-followup-20260510-0415`는 0.41.5 기준 Operator Access와 Internal Service Hardening 후속 evidence다. Installed account login, target-backed noVNC, service token rotation/revoke, Windows Credential Manager default transition, internal HTTPS/TLS lifecycle, Windows Event Log default transition이 PASS했다. Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 0.41.5 to 0.41.6 package pair, installed product update/rollback, internal clean-host install/update/rollback PASS로 닫혔다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/manual-admin-campaign-20260511-0418-0419`는 0.41.8 기준 MANUAL-ADMIN 1-2-3-4 캠페인 증거다. Full admin host mutation gate, installed account login, target-backed noVNC, internal HTTPS/TLS lifecycle, Credential Manager default transition, Event Log default transition, service token rotation/revoke, product update/rollback `0.41.8 -> 0.41.9 -> 0.41.8`, Burn lifecycle, MSIX lifecycle, internal clean-host install/update/rollback, MSI/update package apply composed evidence가 PASS했다. MSIX runner timeout 후 AppX event 기반 summary 재구성, clean-host Windows Update 후 dedicated VM `Restart-VM -Force` 1회 수행은 operator note로 기록했다. 최종 installed service는 `Running`, manifest `0.41.8-admin-smoke`, Web HTTP `200`, unauthenticated API `401`, `--max-request-body-bytes 1048576` 유지다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412`, `artifacts/os-mutation-gates-batch-profile-20260510-161416-0412`는 `0.41.2-admin-smoke` historical full admin host mutation gate evidence로 보존한다.
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun`, `artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun`는 `0.41.0-admin-smoke` account-linked full admin host mutation gate evidence로 보존한다. 후속 installed account login smoke는 `artifacts/installed-account-login-smoke-20260510-0410-final`에서 PASS했다.
- `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`는 `0.39.0-admin-smoke` MSI/service installed listener PASS evidence다. MSI provenance commit은 `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256은 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode는 `AllowUnsignedDev`다. Final service는 loopback-only `Running`, product manifest version은 `0.39.0-admin-smoke`, SCM `PathName`은 diagnostic bundle/hardening 인자를 포함하고 diagnostic bundle create/download POST `201`, GET `200`, redaction PASS를 확인했다. Firewall/trust-store/LAN/Event Log OS gate는 이 rerun 범위가 아니다.
- `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390`는 `0.39.0-admin-smoke` installed listener 후속 OS mutation gate PASS evidence다. Batch summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, timeout false이고 firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, internal Root/TrustedPublisher install/remove/restore가 PASS였다. Final service는 `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, boot time unchanged이며 public trusted signing은 `excluded`, external stable publication은 `not-claimed`다.
- `artifacts/msi-update-package-20260509-0391`는 `0.39.1-admin-smoke` MSI/update package apply PASS evidence다. MSI SHA-256은 `9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914`, update ZIP SHA-256은 `d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5`, provenance commit은 `8f0c4b6fbac8787932d0e966437fcc62d86e6068`, signing mode는 `AllowUnsignedDev`다. Elevated MSI apply exit `0`, installed manifest `0.39.1-admin-smoke`, service `Running`, loopback Web Console HTTP `200`을 확인했다. `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`다.
- `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232`는 2026-05-09 자동 non-mutating regression batch PASS evidence다. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=5`, `executed_steps=5`, failed step `null`이며 packaging Pester `248/248`, installer Pester `41/41`, web Pester `31/31`, npm web verification, dotnet solution tests, `git diff --check`가 모두 PASS였다. Host mutation, public trusted signing, external stable publication은 실행하거나 주장하지 않는다.
- `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`는 `0.38.8-admin-smoke` installed destructive update/rollback smoke PASS evidence다. MSI SHA-256은 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`다. Update는 `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`였고 rollback은 current manifest를 `0.38.6-admin-smoke`로 복원하고 `0.38.8-admin-smoke`를 `DesktopNode.failed` diagnostics root로 보존했다. Final service는 `Running`, boot time unchanged, `host_mutation_performed=true`다. 최초 `artifacts/product-update-rollback-mutation-20260507-0388` non-elevated attempt는 blocked history다.
- 이 변경은 public trusted signing 또는 외부 stable publication을 의미하지 않는다. PowerShell Local API와 Hyper-V helper는 component/archive baseline으로만 유지하며, active product Host/API/manifest 경로는 helper process path나 legacy `api_script` path를 받지 않는다. Legacy WinSW PowerShell Local API generation은 retired error로 차단한다.
- GA-ready 제품 재설계는 ADR-0004로 적용됐고, 현재 제품 런타임 결정은 내부 전용 서비스 범위의 `ga-ready-product-runtime`이다.
- Phase 26 정렬 문서, route promotion matrix, repo migration map, verification ownership map은 ADR-0004 current decision의 supporting docs다.
