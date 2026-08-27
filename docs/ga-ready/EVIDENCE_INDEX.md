# Desktop Node 증거 인덱스

## 2026-08-27 `0.42.75` promotion public-boundary main push

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-27-04275-promotion-postpush-pass.md`는
  green head `7cdd56bf0ff3ded2b9541cd242bd1d68905c0e66`의 Public Boundary run
  `33064087018`, job `98489770067` PASS를 기록한다. 같은 head의 Development Gates run
  `33064087022`과 네 job `web`/`dotnet`/`delivery`/`installer-policy`도 모두 `success`다.
- 변경 경로 `37`개 중 product payload 경로는 `0`개다. 이 merge는 이미
  `0.42.75-admin-smoke`로 승격된 operational evidence와 계약 SHA refresh만 착륙시켰으므로
  새 package candidate를 열지 않는다. 04274 P0 landing public-boundary는 predecessor로
  보존한다. 후속 evidence-only 커밋은 재귀적으로 새 package candidate나 전용 post-merge
  evidence를 요구하지 않는다. public trusted signing과 external stable publication은
  주장하지 않는다.

## 2026-08-27 0.42.75 SERVICE_PLAN P0 current promotion

- `docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md`는 clean package PASS를
  기록한다. Clean MSI SHA-256은
  `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md`는
  `full-admin-host-mutation-gate-20260821-04275` PASS를 기록한다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md`는
  설치본 CLI `3/3`, Web `2/2`, service `Running/Automatic`을 `promoted-current`로 닫았다.
  Summary SHA-256은 `3c0378fc0046e328b5637e5872d349920b01bd53a671567fa947e643538f6ce6`다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`는
  actual-VM QoS/disk PASS다. Summary SHA-256은
  `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`다.
- `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-27-04275.md`는 SavedOnly r2 /
  Full r4 / clean-target SavedOnly PASS다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md`는
  `0.42.74-admin-smoke -> 0.42.75-admin-smoke` pair를 descriptor
  `manual-admin-campaign-descriptor-20260827-04274-04275`, `runner_count=6`,
  `missing_count=0`, `not_pass_count=0`으로 닫았다.
- canonical current는 `0.42.75-admin-smoke`다. `promotion_eligible=true`, blockers는
  없다. token R4 SHA-256 `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`는
  04272 carry-forward다. public trusted signing과 external stable publication은 주장하지
  않는다.

## 2026-08-25 Required CI final-main authority PASS and Public Boundary residue

- `docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md`는 shadow SHA
  `f8208f076cb9db69022b4dc060e65f13d23fae8c` / run `32898937784`, direct-child cutover
  `68756f1f2f609951aaf54d76963b10f96409011b`, PR #1 merge SHA
  `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`를 cutover predecessor로 보존하고, PR #2 merge 뒤
  final `main` SHA `6e2bdb93ce308b632c929e2c17f5550ac3845401`를 현재 Required CI authority로 기록한다.
- Final-main Development Gates run [`32904006595`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595),
  attempt `1`은 정확히 `dotnet` job `97983889723`, `web` job `97983889620`, `delivery` job
  `97983889739`, `installer-policy` job `97983889504`를 PASS했다. Workflow wall-clock은
  `120000 ms`, job envelope는 `117000 ms`다.
- 정확히 네 artifact는 .NET `9584155808`, Web `9584126782`, Delivery `9584125168`,
  Installer Policy `9584129949`다. Contract v2 summary는 모두 `active`, `ok=true`, all suites
  `cutover/passed`, timeout/cancel/failure/skip/not-run `0`이다. 실측은 .NET `2210/2210`
  (`55913 ms`), Web registry `50/50` (`19352 ms`), Delivery `528/528` (`5092 ms`), Installer
  `49/49` (`4082 ms`)다.
- Public `main` protection은 `strict=true`, admin enforcement를 유지하고 GitHub Actions app ID
  `15368`의 exact required contexts `dotnet`, `web`, `delivery`, `installer-policy`만 요구한다.
  Force-push와 deletion은 비활성이다. Cutover 당시 old/new required-status payload SHA-256
  `7b2ae4962bea6779aaf4408e2cc7b0b8ddfa6f4a45a13cd4850d486e79197292` /
  `a13b0626b38e46fec320608b07a5f9fec88d22219d8e0bfef06d91336399fd0d`와 disclosed pre-change
  ETag/provider-before hash 누락은 historical cutover evidence에 그대로 보존한다.
- Required CI executable Pester, 비관리자 PowerShell, host/service/MSI/VM mutation invocation은
  `0`이다. 별도 Public Boundary run [`32904006619`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006619),
  job `97983888524`는 Pester/PowerShell을 계속 사용하는 non-required residue이며
  `provider_required=false`다. Repository-wide PowerShell/Pester zero로 확대 해석하지 않는다.
- 당시 Operational authority는 `0.42.74-admin-smoke`, closed pair는
  `0.42.73-admin-smoke -> 0.42.74-admin-smoke`였다. 이후 2026-08-27 Lane 3가
  `0.42.75-admin-smoke`로 승격했다. public trusted signing/external stable publication은
  계속 `false`다.

## 2026-08-25 Packaging verification Wave D local parity

- `docs/ga-ready/evidence/pester-free-packaging-wave-d-2026-08-25.md`는 55개 Packaging legacy
  파일의 528개 계약에 대해 C# replacement와 일회성 Pester 5.7.1 reference가 최종 aggregate
  `528/528`, failed/skipped/not-run `0`으로 PASS했음을 기록한다. Delivery assembly `684/684`,
  전체 solution 9 assemblies `2,153/2,153`, Web contract `50/50`도 PASS했다.
- Strict v2 ledger의 62 files / 627 contracts는 모두 `mapped` / local `pass` / CI `pending`이다.
  `delivery-contracts`와 `evidence-check`는 `mapped`, catalog activation은 계속
  `plan-only-foundation`이며 managed current-evidence 검증은 write/child-process `0`이다.
- Sanitized public root에서 제외한 frozen reader binary는 bootstrap-recorded SHA-256과 일치하는
  read-only copy를 ignored `artifacts/**`에서만 reference에 사용했고 추적·공개하지 않았다.
  Required CI same-SHA parity, Pester/non-admin PowerShell zero와 branch-protection cutover는 Wave E
  전까지 주장하지 않는다. Host mutation, package candidate, public trusted signing 및 external
  stable publication도 모두 이 evidence 범위 밖이다.

## 2026-08-25 Installer verification Wave C local parity

- `docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md`는 clean input
  `0ab1bda71f3398aed302d53e7d6715987ce87b19`에서 Installer legacy/reference와 C# replacement가
  각각 `49/49` PASS하고 failed/skipped/not-run `0`임을 기록한다. Strict v2 ledger는 62 files /
  627 contracts, Web mapped/local-pass 50, Installer mapped/local-pass 49, Packaging unmapped 528,
  missing/duplicate/order drift `0`이다.
- 새 replacement의 PowerShell/Pester/MSI/service/VM/shell 실행은 `0`이며 host mutation도 수행하지
  않았다. `installer-contracts=mapped`만 승격했고 catalog activation은 계속
  `plan-only-foundation`, Delivery/Evidence는 `wave-d-pending`, 모든 CI parity는 `pending`이다.
- Required CI의 Pester/non-admin PowerShell zero, cutover, public trusted signing과 external stable
  publication은 아직 주장하지 않는다. Operational current와 actual-VM blocker도 변경하지 않는다.

## 2026-08-25 public authority bootstrap local source gate

- `docs/ga-ready/evidence/public-authority-bootstrap-2026-08-25.md`는 sanitized committed tree의
  parentless export 전 로컬 gate를 `LOCAL_SOURCE_GATE_PASS`로 기록한다. .NET `1451/1451`, Web
  contract `50/50`, public-safety `20/20`, legacy Pester reference `627/627`이 PASS했고 공식
  Gitleaks 및 repository-owned scan finding은 각각 `0`이다.
- 이 checkpoint는 provider repository 생성·push·visibility 변경·branch protection을 수행하지
  않았다. Historical ignored executable은 read-only reference test에만 사용했고 export하지 않는다.
  Host mutation, package candidate, public trusted signing, external stable publication은 모두 `false`다.

## 2026-08-24 Web verification Wave B local parity

- `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`는 clean input
  `20ba3b80c211cc6a29bc9ecaf7e9195911678f14`에서 legacy Web Pester와 Node replacement가
  각각 `50/50` PASS하고 controlled missing-`app-root` defect가 양쪽에서 실패한 로컬
  code-level evidence다. 50행 mapping SHA-256과 명령별 실측도 같은 문서가 소유한다.
- Migration manifest의 Web 한 행만 `mapped` / local `pass`이고 CI는 evidence 없이
  `pending`이다. 다른 61행은 `unmapped` / local·CI `pending`을 유지한다.
- Required CI parity는 아직 PASS하지 않았고 required gate에서 Pester와 non-admin
  PowerShell을 제거하지 않았으며 cutover도 완료하지 않았다. Host·service·MSI mutation과
  actual-VM 검증은 수행하지 않았다. Public trusted signing과 외부 stable publication도
  주장하지 않는다. 당시 Operational current는 `0.42.74-admin-smoke`였고 열린
  saved-lifecycle actual-VM blocker는 그 시점까지 해결되지 않았다.

## 2026-08-24 C# verification Wave A foundation

- `docs/ga-ready/evidence/pester-free-csharp-verification-wave-a-foundation-2026-08-24.md`는
  `activation_state=plan-only-foundation`의 C# 계획 투영, 비-plan-only 사전 차단과 기존
  required workflow 무변경을 기록한다. 이 기록은 Wave A code-level foundation에 한정되며
  `required_ci_pester_zero=false`, `required_ci_nonadmin_powershell_zero=false`,
  `cutover_completed=false`이므로 Wave B~E 완료, migration 전체 PASS 또는 public 승격의
  근거가 아니다.

## 2026-08-21 `0.42.74` P0 landing public-boundary

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-21-04274-p0-landing-pass.md`는
  green head `5f9cecfd5507e7e5dd726601aae3760e4e1b558c`의 required run `32388996125`, job
  `96490306459` PASS를 기록한다. 같은 head의 Development Gates run `32388996111`과 네 job도
  모두 `success`다.
- 변경 경로 `86`개 중 non-test product payload 경로는 `33`개다. 이 payload는 이미
  `0.42.74-admin-smoke`로 검증됐으므로 새 package candidate를 열지 않는다. 직전 head
  `15cce2f4` Development Gates packaging-pester failure는 ratchet follow-up `5f9cecfd`가
  닫았다.
- 04273 promotion postpush는 predecessor로 보존한다. 후속 evidence-only 커밋은 재귀적으로
  새 package candidate나 전용 post-merge evidence를 요구하지 않는다. public trusted
  signing과 external stable publication은 주장하지 않는다.

## 2026-08-21 `0.42.74` SERVICE_PLAN P0 predecessor promotion

- `docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md`는 local `main` HEAD
  `adc04673b569ef9b587371fdb23bc11ceb14e2e2` clean package PASS를 기록한다. Clean MSI
  SHA-256은 `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md`는
  `full-admin-host-mutation-gate-20260820-04274` Service/MSI/Hyper-V route와 OS mutation
  `2/2` PASS를 기록한다. Operational MSI SHA-256은
  `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`, payload aggregate
  SHA-256은 `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md`는
  설치본 CLI `3/3`, Web `2/2`, service `Running/Automatic`, `tui_present=false`를
  확인해 `promoted-current`로 닫았다. Summary SHA-256은
  `531fc614da5edb0e11994b021383491ccb8830115d59fb211c6c330f5b25f8c8`다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md`는
  설치본 actual-VM QoS/disk `PASS`를 기록한다. Summary SHA-256은
  `5395286b74ca7dabd3edccbb63c0b006c32999a4c350559e8b90ddb1ea1fb4b8`다.
- `docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md`는 P0 attach
  overwrite / restore `is_current` / manage PASS와 `vm.save` FAIL(WMI `32775`,
  RequestedState `32769`)을 기록한다. 전체 P0 actual-VM은 FAIL이며 열린 결함이다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md`는
  `0.42.73-admin-smoke -> 0.42.74-admin-smoke` package pair를 descriptor
  `manual-admin-campaign-descriptor-20260820-04273-04274-closed`, `runner_count=6`,
  `missing_count=0`, `not_pass_count=0`으로 닫았다. Update ZIP SHA-256은
  `cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1`다.
- 당시 canonical current는 `0.42.74-admin-smoke`였다. P0 `vm.save` FAIL는 그 승격이 고치지
  않았다. token R4는 04272 carry-forward다. public trusted signing과 external stable
  publication은 주장하지 않는다.

## 2026-08-14 installed loopback bootstrap smoke

- `docs/ga-ready/evidence/installed-loopback-bootstrap-smoke-2026-08-14-04273.md`는 설치본
  `0.42.73-admin-smoke` Web `http://127.0.0.1/`과 API `http://127.0.0.1:7777`에서
  loopback session 발급과 Edge CDP bootstrap을 PASS로 기록한다. Summary SHA-256은
  `b49dcaf737d1499446be7601297a232a7a26917cb83eb1ea122190a039a17475`다.
- `/pcv-config.js`는 token-free, 계정 수는 `0`, unauthenticated policy는 `401`,
  authenticated policy는 `200`이다. host mutation은 없다. public trusted signing과
  external stable publication은 주장하지 않는다.

## 2026-08-14 0.42.73 promotion public-boundary follow-up

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md`는
  승격 커밋 `291435e374efef7f9639b820ac197c11e2c7e8a4`의 required run `31737488576`, job
  `94572517694` PASS를 기록한다. 같은 head의 Development Gates run `31737488562`와 네 job도
  모두 `success`다.
- 변경 경로 `17`개는 docs/tests-only이며 product payload 경로는 `0`개다. 따라서
  `0.42.73-admin-smoke`와 payload provenance `b84441f0750a9f77fd0588a86912dbdb68b94f0c`을
  유지하고 새 package candidate를 열지 않는다.
- PR #187 post-merge evidence는 0.42.72 승격 이후 docs-only predecessor로 보존한다. 후속
  evidence-only 커밋은 재귀적으로 새 package candidate나 전용 post-merge evidence를 요구하지
  않는다. `0d5bc0d488a63fcaacab83674a9e8d7d9025d0d8` docs-only push의 Public Boundary
  `31741234613`과 Development Gates `31741234494`는 success였고 전용 evidence는 쓰지
  않았다. Public trusted signing과 external stable publication은 주장하지 않는다.

## 2026-08-14 campaign execution worklog

- `docs/ga-ready/evidence/campaign-execution-worklog-2026-08-14.md`는 같은 날
  clean package, fullgate, read-only current-card, actual-VM functional,
  manual-admin package-pair의 세션 작업 내역을 한 문서로 묶는다. 개별 게이트 계약은
  아래 2026-08-14 절의 각 evidence가 소유한다.

## 2026-08-14 `0.42.73` package-to-current promotion closure

- `docs/ga-ready/evidence/admin-smoke-package-2026-08-14-04273.md`는 HEAD
  `b84441f0750a9f77fd0588a86912dbdb68b94f0c` clean package PASS를 기록한다. Clean MSI
  SHA-256은 `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-14-04273-hostmutation.md`는
  `full-admin-host-mutation-gate-20260814-04273`의 Service/MSI/Hyper-V route와 OS mutation
  `2/2` PASS를 기록한다. Operational MSI SHA-256은
  `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1`, payload aggregate
  SHA-256은 `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85`다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md`는
  CLI `3/3`, Web `2/2`, service `Running/Automatic`, `tui_present=false`와 04272 token R4
  carry-forward를 확인해 `promoted-current`로 닫았다. Summary SHA-256은
  `44a91426579c6fb486e6b99cca2321ba4fd8cd547d16797017e0baa6c9d0da14`다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-14-04273.md`는
  실제 VM QoS `2048 Kbps -> 2,048,000 bps`, disk shrink guard, `10 -> 11 GiB` expansion과
  cleanup을 새로 실행해 `10/10` PASS로 기록한다. Summary SHA-256은
  `09a571235524b1a32c6066b7ef8c3c4ab4a425a7016ef4ccd1d284f75f9e6fac`다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md`는
  `0.42.72-admin-smoke -> 0.42.73-admin-smoke` package pair를 descriptor
  `manual-admin-campaign-descriptor-20260814-04272-04273-closed`, `runner_count=6`,
  `missing_count=0`, `not_pass_count=0`으로 닫았다. Update ZIP SHA-256은
  `1a7b17e2f1e2e3175f94c1ffce03b5d358a291f795ca34b3e0d4602e116d1b3c`다.
- token R4와 credential rebootstrap는 token/credential payload 변경이 없어 04272 evidence를
  carry-forward한다. 최신 dedicated public-boundary 문서는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md`다.
  PR #187은 0.42.72 승격 이후 docs-only predecessor다. public trusted signing과 external
  stable publication은 주장하지 않는다.

## 2026-08-13 loopback bootstrap browser gate code-level pass

- `docs/ga-ready/evidence/loopback-bootstrap-browser-gate-code-level-2026-08-13.md`는
  in-process Host + Edge/Chrome CDP required 게이트를 `CODE_LEVEL_PASS`로 기록한다.
- Playwright와 설치본 listener는 required가 아니다. operational current는 바꾸지 않는다.

## 2026-08-13 Web loopback session bootstrap code-level pass

- `docs/ga-ready/evidence/web-loopback-session-bootstrap-code-level-2026-08-13.md`는
  `POST /api/v1/auth/loopback-session`과 Web `ensureLoopbackSession`/`refreshAll` 게이트를
  `CODE_LEVEL_PASS`로 기록한다. Design-ID는
  `purecvisor-desktop-node-web-loopback-session-bootstrap-v1`, 승인 locator는
  `User-Approval: web-loopback-session-bootstrap-20260813`이다.
- `accounts.json`은 발급 전후 계정 0개/`no-default-account`를 유지하고, `/pcv-config.js`는
  token-free다. 빈 세션 401 fan-out은 닫혔다. Full lane 7 suite가 PASS했다.
- `host_mutation_performed=false`, `operational_current_changed=false`이며 operational
  anchor는 `0.42.72-admin-smoke` 그대로다. 설치본 E2E와 다음 package campaign은 열지 않았고
  public trusted signing과 external stable publication은 주장하지 않는다.

## 2026-08-12 PR #187 post-merge public-boundary follow-up

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md`는 PR
  #187 post-merge main push의 required run `31579083573`, job `94057811212`, head
  `a626a7e15d51903f2df5d83d48ffcd2c2115dfc1` PASS를 기록한다. 같은 head의 Development
  Gates run `31579083722`와 네 job도 모두 `success`다.
- PR #187의 변경 경로 `20`개는 docs/tests-only이며 product payload 경로는 `0`개다. 따라서
  `0.42.72-admin-smoke`와 payload provenance `02428fabfe5550e0bb3e412db3da29e8ccb57d40`을
  유지하고 새 package candidate를 열지 않는다.
- PR #186 post-merge evidence는 0.42.72 승격을 시작한 product-payload predecessor로 보존한다.
  후속 evidence-only PR은 재귀적으로 새 package candidate나 전용 post-merge evidence를 요구하지
  않는다. Public trusted signing과 external stable publication은 주장하지 않는다.

## 2026-08-09/10 `0.42.72` package-to-R4 promotion closure

- `docs/ga-ready/evidence/admin-smoke-package-2026-08-09-04272.md`는 PR #186 merge head
  `02428fabfe5550e0bb3e412db3da29e8ccb57d40`의 clean package PASS를 기록한다. Clean MSI
  SHA-256은 `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22`다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-09-04272-hostmutation.md`는
  `full-admin-host-mutation-gate-20260809-04272`의 Service/MSI/Hyper-V route와 OS mutation
  `2/2` PASS를 기록한다. Operational MSI SHA-256은
  `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa`, payload aggregate
  SHA-256은 `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933`다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-08-09-04271-04272.md`는
  `0.42.71-admin-smoke -> 0.42.72-admin-smoke` package pair를 descriptor
  `manual-admin-campaign-descriptor-20260809-04271-04272-closed`, `runner_count=6`,
  `missing_count=0`, `not_pass_count=0`으로 닫았다. Update ZIP SHA-256은
  `f9dfa886dd5db2623ec63342538d775757b5f464e9eb9ca23a5206bcc1d65ba8`다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-09-04272.md`는
  실제 VM QoS, disk shrink guard, `10 -> 11 GiB` expansion과 cleanup을 새로 실행해 PASS했다.
  Summary SHA-256은 `4938e6307ce5beb9e012b1a05ce32a2e0e410ee735b87e3ecc0634897dbb6dd6`다.
- `docs/ga-ready/evidence/operational-credential-rebootstrap-recovery-r2-2026-08-09-04272.md`는
  보존된 transition/rollback evidence를 read-only로 재판정해 verifier false-negative를 닫았다.
  Summary SHA-256은 `529626336fcb79696f5cf765e7f1dacbf81a96beafc30000e00fa591ec7bfacb`다.
- `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`는 R4 read-only
  reconciliation을 `PASS`, `current_claim_eligible=true`로 닫았다. R4 summary SHA-256은
  `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`이며 R4 자체 host
  mutation은 `false`, 보존 retry2 mutation은 `true`다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-09-04272.md`는 CLI
  `3/3`, Web `2/2`, service `Running/Automatic`, `tui_present=false`와 token R4 연결을 확인해
  `promoted-current`로 닫았다. Summary SHA-256은
  `02304c8f93d122d21310ba7549356e7d12decbfc95342e799850d3929cf3f05a`다.
- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-09-pr186-postmerge-pass.md`는 PR
  #186 post-merge main push의 required run `31302773929`, job `93218124085`, head
  `02428fabfe5550e0bb3e412db3da29e8ccb57d40` PASS를 기록한다.
- 이 closure는 internal admin-smoke 전용이다. Public trusted signing과 external stable
  publication은 주장하지 않으며 다음 pair는 `0.42.72-admin-smoke -> 0.42.73-admin-smoke`로
  닫혔다.

## 2026-07-16 0.42.65 package/fullgate/actual-VM/current-card promotion

- `docs/ga-ready/evidence/admin-smoke-package-2026-07-16-04265.md`는 CLI/Web-only clean
  package build PASS를 기록한다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-16-04265-hostmutation.md`는
  Service/MSI/Hyper-V route와 OS mutation `2/2` actual-host PASS를 기록한다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.md`는
  실제 VM QoS `2048 Kbps -> 2,048,000 bps`, disk shrink guard, 10→11 GiB expansion,
  cleanup PASS를 기록한다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-16-04265.md`는
  CLI 3개 exit `0`, Web 2개 HTTP `200`, service `Running/Automatic`, TUI 부재를 기록한다.
- 이 승격은 internal admin-smoke 전용이며 public trusted signing, external stable publication,
  manual-admin package-pair closure를 열지 않는다.

## 2026-07-16 current evidence generation and S/M/L classification code-level pass

- `docs/ga-ready/evidence/current-evidence-generation-code-level-2026-07-16.md`는
  `pcv-current-evidence-v1` JSON에서 6개 current-facing 문서를 생성·검사하고 historical
  section을 보존한 결과를 `CODE_LEVEL_PASS`로 기록한다.
- Generator update 후 두 번의 read-only `-Check`, target hash 불변성, Admin evidence/ownership 88건,
  S/M/L·생성기 focused 17건, workflow 1건을 합친 106건과 Full lane 7개 suite가 PASS했다.
- `host_mutation_performed=false`, `package_build_performed=false`,
  `installed_product_changed=false`이며 operational anchor는 `0.42.64-admin-smoke` 그대로다.
- Active surface는 Web Console/PCVCLI, `tui_present=false`다. Public trusted signing과 external
  stable publication은 열지 않았다.
## 2026-07-16 development feedback loop code-level pass

- `docs/ga-ready/evidence/development-feedback-loop-code-level-2026-07-16.md`는
  Fast/Full/Release selector/runner, Batch Supervisor seam, installer in-process boundary와
  네-job Full-lane CI 계약을 `PASS`로 기록한다.
- Packaging Pester와 installer/Web Pester는 490건 PASS, 합산 134.83초로 기존 479건
  201.2초 대비 33.0% 단축됐다. .NET 591건과 Web type/static/parity도 PASS했다.
- `host_mutation_performed=false`, `package_build_performed=false`,
  `installed_product_changed=false`이며 operational anchor는 계속 `0.42.64-admin-smoke`다.
- 이 evidence는 code-level 개발 피드백 범위이며 public trusted signing 또는 external stable
  publication을 주장하지 않는다.

## 2026-08-05 `0.42.69` anchor 승격과 manual-admin readiness

- `docs/ga-ready/evidence/admin-smoke-package-2026-08-05-04269.md`는 현재 `main`(`7236b813`)에서
  빌드한 package를 기록한다. `0.42.68`은 커밋 `f9337061` 기준이라 이후 제품 코드 커밋 `13`건이
  빠지므로 승격 대상이 아니었다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-05-04269-hostmutation.md`는 2단계
  batch가 exit `0`, 재시도 `0`으로 PASS했고 설치본이 `0.42.68` -> `0.42.69`로 전환됐음을 기록한다.
  `installed-dotnet-host-hyperv-api-route-smoke`가 같은 날 수정한 Gen2 boot order와 Secure Boot
  템플릿을 설치본 경로에서 재확인했다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-05-04269.md`는 이
  버전에서 재실행한 항목과 `0.42.65`에서 이월한 항목을 분리해 기록한다. QoS 변환과 disk shrink
  guard/expansion은 이월이며 재검증이 아니다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-05-04269.md`는 CLI `3/3`
  exit `0`, Web `2/2` HTTP `200`, `secret_observed=false`로 anchor 승격을 닫는다.
- `docs/ga-ready/evidence/manual-admin-campaign-readiness-2026-08-05-04269-04270.md`는 anchor 승격이
  68일간의 `blocked-by-installed-baseline-version-mismatch`를 해소해
  `ready-current-baseline-target-package-pair`와 `reserved-and-matched`에 도달했음을 기록한다.
  campaign closure는 달성하지 않았고 runner 5종 중 1종만 실행했다. 이 문서는 작성 시점
  `19:33` 기준 point-in-time 기록이며
  `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md`가 대체한다.
- 이 evidence들은 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication을 주장하지 않는다.

## 2026-08-06 `0.42.70` anchor 승격

- `docs/ga-ready/evidence/admin-smoke-package-2026-08-06-04270.md`는 campaign target package를
  재빌드 없이 승격 후보로 쓴 근거를 기록한다. provenance `821a6a34`와 승격 시점 HEAD
  `e9138988` 사이 payload source diff는 `0`건이다.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-06-04270-hostmutation.md`는 2단계
  batch가 exit `0`, 재시도 `0`(`91s` + `11s`)으로 PASS했음을 기록한다. 설치본 경로에서 Gen2 VM
  생성/checkpoint/삭제와 unmanaged delete guard를 실측했다.
- `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-06-04270.md`는 이
  버전에서 재실행한 항목과 `0.42.65`에서 이월한 항목을 분리해 기록한다. QoS 변환과 disk shrink
  guard/expansion은 이월이며 재검증이 아니다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-06-04270.md`는 CLI `3/3`
  exit `0`, Web `2/2` HTTP `200`, `secret_observed=false`로 anchor 승격을 닫는다.
- 이 승격으로 canonical anchor, 설치본, manual-admin closure 세 평면이 모두
  `0.42.70-admin-smoke`를 가리킨다.
- 이 evidence들은 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication을 주장하지 않는다.

## 2026-08-06 manual-admin `0.42.69 -> 0.42.70` closure

- `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md`는 descriptor
  `manual-admin-campaign-descriptor-20260805-04269-04270-closed`가 `runner_count=6`,
  `missing_count=0`, `not_pass_count=0`, `overall_status=pass`로 닫혔음을 기록한다.
  2026-05-29 이후 `69`일 만의 첫 manual-admin closure다.
- campaign은 2일에 걸쳤다. runner 5종은 2026-08-05 `19:24`-`20:09`에, clean-host는
  2026-08-06 `12:14`-`13:04`에 실행됐다. clean-host는 `KB5099540`, UBR `169 -> 5386`,
  install/update/rollback exit `0`, `final_web_status_code=200`, `blocker=none`이다.
- readiness 문서가 미해결로 남긴 `WixToolset.Bal.wixext` `damaged` 상태는 blocker가
  아니었다. Burn runner는 `WixToolset.BootstrapperApplications.wixext`로 빌드한다.
- 설치본은 `0.42.70-admin-smoke`이고 canonical anchor는 `0.42.69-admin-smoke`에 머문다.
  `0.42.70` anchor 승격에 필요한 full admin host mutation gate와 installed current-card는
  실행하지 않았다.
- 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication을 주장하지 않는다.

## 2026-08-06 functional correctness 이월 3항목 `0.42.70` 재검증

- `docs/ga-ready/evidence/functional-correctness-carry-forward-revalidation-2026-08-06-04270.md`는
  `0.42.69`/`0.42.70` anchor가 모두 `0.42.65` 이월로 기록했던 세 항목을 `0.42.70` 설치본에서
  실제로 재실행한 결과를 `PASS`로 기록한다. 같은 날 anchor evidence의 "재실행하지 않고 이월한
  것" 표는 이 evidence로 닫힌다.
- Network QoS maximum `2048 Kbps -> 2,048,000 bps`, disk shrink guard
  `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`로 실패하며 `10,737,418,240` bytes 유지, expansion
  `10 -> 11 GiB` 성공(`11,811,160,064` bytes)이다. 관측값은 job 보고값이 아니라
  `Get-VMNetworkAdapter`/`Get-VHD`에서 직접 읽었다.
- 검증 VM `pcv-fc-cf-5e6f4823`과 임시 root는 cleanup `PASS`이고 잔여 `0/0`이다. runner는
  `packaging/windows-desktop-node/tools/Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`,
  artifact는 `artifacts/functional-correctness-carryforward-20260806-04270/summary.json`이다.
- `host_mutation_performed=true`이며 operational anchor는 `0.42.70-admin-smoke` 그대로다. public
  trusted signing과 external stable publication은 주장하지 않는다.

## 2026-08-06 FC-05 / FC-12(b) / FC-13 검증 partial pass

- `docs/ga-ready/evidence/fc-05-fc-12b-fc-13-verification-2026-08-06-04270.md`는 감사 §12가
  "환경 부재로 미검증"으로 남긴 세 항목을 `PARTIAL_PASS`로 기록한다.
- 감사 §12.5가 부재로 본 자산은 실제로 호스트에 있었다. 격리 guest `pcv-guest-installed-04253-r1`과
  전용 DPAPI credential(`374` bytes)이 그대로 유효했다.
- FC-05는 `PASS`다. `vm.guest.channel.verify` exit `0`
  (`transport=windows-powershell-direct`), `vm.guest.exec -- hostname` exit `0`이며 `04253` PASS
  이후 `71`일 만의 재검증이다. credential은 `dpapi:<protected-file>` 참조로만 전달했고
  `secret_observed=false`, `password_value_observed=false`다.
- FC-13은 `PASS`다. 제품의 boot order 소유는 `45ba267e`가 이미 닫았고 이번에
  `src/DesktopNode.HyperV.Tests/Gen2BootOrderContractTests.cs` `6`건으로 계약을 잠갔다. reorder를
  되돌리면 `3`건이 실패하는 것을 확인해 공허하지 않음을 증명했다.
- FC-12(b)는 호스트 측만 `PASS`이고 **guest 측은 미확정**이다. 비 ASCII 표본의
  `stdout_byte_count=27`은 UTF-8(`33`)도 OEM 손실(`19`)도 아니었고, 순수 ASCII 명령도 `$`/`;`/`[`
  가 들어가면 `0` bytes를 냈다. stream 인코딩과 argv 전달 두 가설을 분리하지 못했으므로 어느
  쪽도 결함으로 확정하지 않는다. 후속 조사 대상으로 남는다.
- 부작용 기록: 이 실행이 `pcv-guest-installed-04253-r1`을 기동하면서 `2026-05-27` 세션의 잔여
  자동 checkpoint `1`개가 병합돼 `0`개가 됐다. VM identity/Notes/keep policy/credential은 그대로다.
  `AutomaticCheckpointsEnabled`를 끄라는 권고만 남기고 실행하지 않았다.

## 2026-08-06 guest-exec argv fidelity와 FC-12(b) guest 측 종결

- `docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`는 FC 검증
  evidence가 `미확정`으로 남긴 FC-12(b) guest 측을 `PASS`로 닫는다. **원인은 인코딩이 아니라
  argv 전달이었다.**
- bridge가 `[scriptblock]::Create($payload.command -join ' ')`로 argv를 이어붙여 guest에서
  재파싱했다. 공백이 든 인자는 쪼개지고 `$(...)`/`;`가 든 인자는 실행됐다. 표본
  `café 한글 日本語 Ж Ω ß`가 `6`줄로 갈라진 것이 기록된 `27` bytes의 정체이며, UTF-8(`33`)도
  OEM 손실(`19`)도 아니었던 이유다.
- 수정은 argv를 데이터로 넘긴다. `-ArgumentList (, $pcvArgv)`와
  `param([string[]]$argv)` splat이며, 단일 원소일 때 `$argv[1..0]`이 내림차순 범위가 되는 함정을
  분기로 막는다. PCVCLI가 이미 문서화한 `-- <command>` argv 계약을 복원하는 방향이고, 기존 동작을
  잠그는 테스트는 없었다.
- 실제 guest `pcv-guest-installed-04253-r1`에서 같은 세션에 수정 전/후를 나란히 실행해 PowerShell
  Direct 경계를 넘는 것까지 확인했다. 비 ASCII 표본은 기대 UTF-8 길이와 정확히 같은 `31` bytes로
  돌아왔다.
- `GuestExecutionArgvFidelityTests` `6`건을 추가했고 공허하지 않음을 실측했다. 옛 join으로
  되돌리면 `5`건이 실패한다. .NET 전체는 `842/0/0`이다.
- 이 VM의 `AutomaticCheckpointsEnabled`를 이전 evidence의 권고대로 껐다. 그 결과 이번 기동은
  디스크 체인에 부작용을 남기지 않았다(checkpoint `0` → `0`, 연결 디스크 불변).
- `installed_product_changed=false`이며 설치본 `0.42.70-admin-smoke`에는 여전히 수정 전 코드가
  있다. 이 수정은 다음 package 후보에 포함된다. operational anchor는 승격하지 않는다.

## 2026-08-06 `DesktopNodeApiRequestProcessor` 도메인 분해 code-level pass

- `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md`는 감사 §P2-2가 기록한
  대형 모듈 중 백엔드 `1`종의 분해를 `PASS`로 기록한다. `3,367`줄에서 `495`줄로 `85`% 줄었고
  callback-free 소유자 `13`개가 생겼다.
- `Func`로 처리기를 되돌려 받던 callback adapter `2`종(`DesktopNodeApiJobRuntimeHandler`,
  `DesktopNodeApiConsoleHandler`)을 제거했다. wave 1이 diagnostics/auth/ops에서 없앤 형태가 job과
  console에는 남아 있었다.
- 공개 표면(`CreateDefault`, `Handle`, `ProcessOneQueuedJob`, `ProcessWorkerPool`,
  `RunWorkerLoopAsync`, `CreateWithDependencies`, `BeforeJobFinalization`)은 불변이다.
- 순수하지 않은 이동은 `3`건이며 evidence §3이 각각의 이유를 적는다. 그중 `JobStoreCommitError`는
  계획서의 "인스턴스 상태를 쓰지 않는다"는 가정이 **측정으로 반증된** 경우다.
- 기존 guard `1`건(`RequestProcessorDelegatesAuthSessionBehaviorToCallbackFreeOwner`)이
  `ResolveActor` 호출자 이동으로 실패했고, 단언의 목적을 유지한 채 실제 호출자를 확인하도록
  갱신했다. 삭제하거나 약화하지 않았다.
- 신규 guard `14`건의 비공허를 probe `3`개로 실측했다. 각 probe는 의도한 테스트 하나만 실패시켰다.
- `PcvModuleSizeRatchet`에서 processor 상한을 `3367`에서 `495`로 내리고, 이번에 생긴 대형 모듈
  `2`종(`856`, `770`줄)을 **생성 시점에 신규 등록**했다. 등록하지 않으면 대형 모듈을 이름만 바꿔
  옮긴 것이 된다.
- .NET `856/0/0`이다. 계획서 대비 미달 `3`건(processor `450`줄 목표 → `495`줄, wave 1 helper 사본
  제거 미실행, task 단위 커밋 분할 미실행)은 evidence §8이 적는다.

## 2026-08-08 wave 1 소유자 helper 사본 제거 code-level pass

- `docs/ga-ready/evidence/wave1-owner-helper-copy-removal-2026-08-08.md`는 위 분해가 남긴
  미이행 `3`건 중 `2`번(wave 1 helper 사본 제거)의 종결을 `PASS`로 기록한다. 사본 `14`개를
  제거했고 `DesktopNode.Api`가 `7,927` → `7,775`줄로 줄었다. 분해가 만든 `292`줄 순증의 절반이
  사본 비용이었다.
- 계획서 `docs/followup-work-plan-2026-08-07.md` §2.2가 적은 제거 대상 `11`개는 **틀렸고 실제로는
  `14`개**였다. 누락 `3`개 중 `2`개는 `DesktopNodeApiOpsSummaryHandler.cs` 안의 **다른 클래스**
  (`DesktopNodeApiOpsSummaryQuery`)에 있었다. 파일이 아니라 클래스 단위로 세야 잡힌다.
- 계획서가 "유일하게 실제 확인이 필요"하다고 지목한 `EmptyObject`는 구현이 실제로 달랐고
  (`SerializeToElement` vs `JsonDocument.Parse`+`Clone`), 임시 probe로 관측 등가성을 측정해 닫았다.
  추론으로 닫지 않았다.
- 신규 guard `1`건(`Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies`)의 비공허를 `2`회 실측했다.
  단언 경로가 메서드 이름과 중첩 타입 둘이므로 각각 사본을 되살려 확인했고, 두 경우 모두 의도한
  테스트 하나만 실패했다. 중첩 타입 단언이 없었다면 `ParsedJson` 재도입은 조용히 통과했다.
- .NET `857/0/0`이며 Full 레인 `7`개 suite가 모두 PASS다(`change_tier=M`).
  `host_mutation_performed=false`, `package_build_performed=false`,
  `installed_product_changed=false`이며 operational anchor는 `0.42.70-admin-smoke` 그대로다.

## 2026-08-09 BatchEvidence / HostApplication / 테스트 fixture 분해 code-level pass

- `docs/ga-ready/evidence/batch-evidence-host-app-test-fixture-decomposition-2026-08-09.md`는
  (1) `BatchEvidenceSummaryReader` `1,350`→partial 4, (2) `DesktopNodeHostApplication`
  `859`→partial 4, (3) 대형 테스트 fixture를 tests/fakes/extensions로 나눈 결과를 `PASS`로
  기록한다.
- 테스트 extension의 `file static` → `internal static` 가시성 조정 1건(동일 파일 한정 때문에
  partial 분리 후 필수). Api.Tests `251/0`, Host.Tests `198/0`.
- ratchet: BatchEvidence core `112` / Pathing `697`, HostApplication core `201`.

## 2026-08-09 `DesktopNodeJobRuntime` partial 도메인 분해 code-level pass

- `docs/ga-ready/evidence/job-runtime-partial-decomposition-2026-08-09.md`는 Runtime 핵심
  `DesktopNodeJobRuntime` `1,466`줄을 partial `4`파일(core `116`, Commands `469`,
  Persistence `643`, Shared `263`)로 나눈 순수 이동을 `PASS`로 기록한다.
- ratchet에 core `116`과 Persistence `643`을 등록했다. Runtime.Tests `126/0`.
- package/fullgate 미개설, operational anchor `0.42.71-admin-smoke` 유지.

## 2026-08-09 installed credentialed guest-exec + argv fidelity PASS

- `docs/ga-ready/evidence/guest-exec-credentialed-smoke-2026-08-09-04271-pass.md`는 설치본
  `0.42.71-admin-smoke`에서 persistent Windows VHD `pcv-guest-installed-04253-r1` + DPAPI
  credential ref로 channel verify / hostname / FC-12(b) argv `4`케이스를 왕복한 결과를
  `PASS`로 기록한다. transport `windows-powershell-direct`, audit `guest-execution-audit-v1`.
- argv 본문 길이는 FC-12(b) 기대값과 일치하고, Windows `Write-Output` CRLF(`+2`)만 관측
  `stdout_byte_count`에 더해진다. subexpression이 평가되지 않고 statement separator가 이중
  실행되지 않는다.
- artifact: `artifacts/guest-exec-credentialed-smoke-20260809-04271/summary-final.json`.
  `secret_observed=false`. operational anchor는 `0.42.71-admin-smoke` 유지.

## 2026-08-09 HostServiceAction partial / ServiceToken replace / guest-exec status

- `docs/ga-ready/evidence/host-service-action-partial-decomposition-2026-08-09.md`는 Ops 이동 후
  남은 `DesktopNodeHostServiceAction` `1,174`줄을 static partial `5`파일로 나눈 순수 이동을
  `PASS`로 기록한다. core `563`, ServiceConfig `187`, Shared `224`, Token `125`, Commands `103`.
  ratchet core 상한 `1,174` → `563`.
- `docs/ga-ready/evidence/service-token-rotation-replace-hardening-2026-08-09.md`는
  `ServiceTokenRotationRevoke...RedactedAudit` 간헐 실패를 전체 Host.Tests 부하에서 재현하고
  (`바꿀 파일을 제거할 수 없습니다`, `backup_write_status=written`,
  `atomic_replace_status=not-run`) `File.Copy`+`File.Replace(null)`를 backup 경로 포함 단일
  `File.Replace` + `IOException` short retry로 보강한 code-level PASS다. `File.Move` 통일은
  하지 않았다(§12.3 반증 유지).
- `docs/ga-ready/evidence/guest-exec-credentialed-smoke-status-2026-08-09.md`는 당시 status
  기록이며 후속 PASS evidence로 supersede됐다.
- package/fullgate를 열지 않으며 operational anchor는 `0.42.71-admin-smoke` 그대로다.

## 2026-08-09 `DesktopNodeHyperVNativeAdapter` partial 도메인 분해 code-level pass

- `docs/ga-ready/evidence/hyperv-native-adapter-partial-decomposition-2026-08-09.md`는 Hyper-V
  provider domain orchestration 단일 파일 `2,038`줄을 같은 클래스의 `partial` `5`파일로 나눈
  순수 이동을 `PASS`로 기록한다. core `450`, Reads `570`, Mutations `765`, Guest `81`,
  Shared `207`(합계 `2,073`, partial 선언·using 복제 `+35`).
- 공개 타입·시그니처·dispatch 키·호출자 변경을 하지 않았다. 새 타입/오류 코드/provider를
  추가하지 않았다.
- `module-size-ratchet.json`에서 core 상한을 `2,038` → `450`으로 내리고, `500`줄을 넘는 신규
  partial `2`종(Reads `570`, Mutations `765`)을 생성 시점에 등록했다. Guest/Shared는 `500`줄
  미만이라 신규 등록 대상이 아니다.
- HyperV suite `137/0`, `PcvModuleSizeRatchet` PASS. package/fullgate/installed surface를 열지
  않으며 operational anchor는 `0.42.71-admin-smoke` 그대로다.
  `host_mutation_performed=false`, `public_trusted_signing=not-claimed`,
  `external_stable_publication=not-claimed`.

## 2026-08-08 `web/src/served-app.ts` 도메인 분해 code-level pass

- `docs/ga-ready/evidence/served-app-decomposition-2026-08-08.md`는 감사 §P2-2가 기록한 대형 모듈
  `2`종 중 **프런트엔드 절반**의 분해를 `PASS`로 기록한다. `4,005` → `413`줄(`-89.7`%)이고 신규
  part `18`개의 최대가 `mutate.ts` `422`줄이다. 백엔드 절반은 2026-08-06에 닫혔으므로 감사가
  기록한 대형 모듈 `2`종이 모두 해소됐다.
- **순수 이동을 기계로 증명했다.** 번들이 단일 스코프이므로 분해 전후 `app.js`의 최상위 선언
  `272`개를 파서로 짝지어 문자 단위로 비교했고 전부 일치했다. 연속 구간을 원본 순서로 잘라
  **선언 순서조차 바뀌지 않았고**, `app.js`의 변경 줄은 예외 없이 주석이다. 검사기의 비공허도
  실측했다(한 글자 변경본을 실패시킨다).
- 계획서가 놓친 함정 `2`개를 발견했다. `served-app.ts` `1`행의 `// @ts-nocheck`가 `4,005`줄 전체를
  타입 검사에서 면제하고 있어 분할하자 `tsc`가 `328`건을 보고했고, part 목록이
  `build-served-asset.mjs` 외에 `verify-static-parity.mjs`와 web Pester에도 복제돼 있어 둘 다
  stale이 됐다. Pester 쪽은 빌드 스크립트에서 파생하도록 바꿔 네 번째 사본을 만들지 않았다.
- **타입 안전성 개선을 주장하지 않는다.** UI layer는 여전히 타입 검사를 받지 않으며, 이 작업은
  그 사실을 `1`곳에서 `19`곳으로 드러냈을 뿐 부채를 갚지 않았다.
- `module-size-ratchet.json`의 `web/src/served-app.ts` 상한을 `4,005`에서 `413`으로 내렸다.
  Full 레인 `7`개 suite와 web Pester `49`/`49`가 통과하며 operational anchor는
  `0.42.70-admin-smoke` 그대로다.

## 2026-08-06 `DesktopNodeHostServiceAction` 도메인 분해 code-level pass

- `docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`는 감사 §P2-2가 기록한
  대형 모듈 순증 `4`종 중 host 파일 `1`종의 분해를 `PASS`로 기록한다. `4,069`줄에서 `1,174`줄로
  `71`% 줄었고 `Ops/` 9개 도메인 클래스 합계는 `199`에서 `3,040`줄이 됐다.
- `ExecuteAsync -> Ops.X.Execute -> ServiceAction.ExecuteNativeXActionForOps` 왕복을 도메인 `9`개
  전부에서 제거했다. `*ForOps` forwarder는 `0`개이며
  `NoOpsForwarderRemainsOnHostServiceAction` 테스트가 기계로 잠근다.
- 공개 표면(`CreatePlan`, `ExecuteAsync` 4개 오버로드, `EnsureProtectedTokenFile`,
  `EnsureAccountAuthBootstrapFiles`)은 불변이고 호출자 `Program.cs` `1`곳과 테스트 `69`곳은
  수정하지 않았다.
- `PcvModuleSizeRatchet`의 host 파일 `max_lines`를 `4069`에서 `1174`로 내렸다. 같은 브랜치에서
  중복 gate `PcvLargeModuleLineCeiling`을 제거해 단일 gate로 통합했다.
- ownership guard는 `BindingFlags` private reflection 대신 `System.Reflection.Metadata`의
  `PEReader`/`MetadataReader`로 구현했다. 테스트 코드의 private reflection 발생 수를 `0`으로
  고정한 `csharp-architecture-test-migration.json` 정책을 지키기 위해서다.
- .NET `836/0/0`, packaging Pester `477/0/2 skip`, installer Pester `49/49`, Web Pester `49/49`,
  `npm test` PASS, `Update-PcvCurrentEvidenceDocs.ps1 -Check` `7/7 current`다. packaging의
  건너뜀 `2`건은 git-ignored `artifacts/`에 의존하는 frozen-host 항목이며 병합된 `main`
  체크아웃에서는 `479/0/0`이다.
- `host_mutation_performed=false`, `package_build_performed=false`,
  `installed_product_changed=false`이며 operational anchor는 `0.42.70-admin-smoke` 그대로다. public
  trusted signing과 external stable publication은 주장하지 않는다.

## 2026-08-05 Web Console 운영 상태 진실성 code-level pass

- `docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md`는
  정적 셸이 표시하던 조작된 운영값을 제거하고 footer/hero를 실제 state에 바인딩한 결과를
  `CODE_LEVEL_PASS`로 기록한다.
- `GET /api/v1/host/status`에 머신 이름 필드가 없어 정적 `pcv-node-a`는 API가 제공하지 않는
  정보였다. 해당 자리는 `windows.caption`으로 재바인딩했고 계측 인프라가 없는 `WS`/`API latency`
  span은 삭제했다.
- 로드 게이트 `hasRefreshedOperation()`은 기록된 실패 중 하나라도 auth 실패면 모든 operation을
  미로드로 판정한다. Local API가 401을 route별이 아닌 단일 `operation: "api.auth"`로 응답하기
  때문이며, 전면 401과 세션 만료 후 stale 값 노출을 함께 막는다.
- 정적 가드(Web Pester)와 미인증/세션만료/부분실패 동작 가드(browser fixture)를 기존 required
  게이트에 추가했다. Web Pester `49/49`와 npm test/parity/browser fixture가 PASS했다.
- `host_mutation_performed=false`, `package_build_performed=false`,
  `installed_product_changed=false`이며 operational anchor는 `0.42.65-admin-smoke` 그대로다.
- 이 evidence는 code-level 범위이며 설치본 authenticated journey, public trusted signing,
  external stable publication을 주장하지 않는다. 미인증 표시 동작은 browser fixture 관측이며
  설치본 브라우저 관측이 아니다.
- Known-open residual: `getHostReadinessLabel()`의 `Ready` fallback은 제거하지 않았다. 로드 게이트는
  footer/hero 렌더 함수에만 적용했고 `getPriorityItems()`, `renderMetrics()`, `renderOpsCockpit()`
  `3`개 호출 지점은 미게이트로 남아 전면 401에서 `#metric-grid`가 `Host Ready / VMs 0`,
  `#ops-summary-panel`이 `Host readiness Ready / VMs total/running 0 / 0`을 렌더한다. 별도 slice다.
- Installer/Packaging Pester는 최초 evidence 커밋 시점 관측(`49/49`, `466/466`)이며 후속 수정
  wave에서 재실행하지 않았다.

## 2026-08-02 C# architecture Wave 1C auth/session/RBAC owner code-level pass

- `docs/ga-ready/evidence/csharp-architecture-wave1c-auth-owner-2026-08-02.md`는 callback
  auth wrapper를 validation, action response, RBAC authorization, runtime auth policy와 guest
  actor projection을 직접 소유하는 instance owner로 심화한 결과를 `code_complete`로 기록한다.
- API 209/209 3회, Host 162/162, 전체 .NET 673/673·skip 0, quality line
  `51.240143%`/branch `41.651865%`, auth owner scoped line `470/514`/branch `188/273`과
  Release/L 7-suite summary `ok=true`를 확인했다.
- 이 L/Release evidence는 비변경 code-level preflight다. `host_mutation_performed=false`,
  `package_build_performed=false`, `installed_product_changed=false`이며 operational anchor는
  `0.42.65-admin-smoke` carry-forward다.
- 실제 VM/admin 및 installed account/browser/noVNC smoke는 실행하지 않았다. 다음 operator-surface
  account/noVNC product payload 변경 때 stale 검증을 재실행한다. Public trusted signing과
  external stable publication은 열지 않았다.

## 2026-08-02 C# architecture Wave 2A physical job-store durability code-level pass

- `docs/ga-ready/evidence/csharp-architecture-wave2a-physical-job-store-durability-2026-08-02.md`는
  unique candidate/marker temp, `Flush(true)`, typed commit outcome과 restart-readable pending guard를
  `W0-FI-01` product single-runtime create protocol의 `code_complete`로 기록한다.
- Runtime 55/55, API 221/221 3회, Host 164/164, 전체 .NET 700/700·skip 0, product
  Plan/Invoke 87/87, quality line `51.492417%`/branch `41.561001%`와 Release/L 7-suite summary
  `ok=true`를 확인했다.
- Update/Rollback/job-store migration/preserve-data removal은 stopped writer 뒤 unresolved marker를
  fail-closed하며 explicit RemoveData만 exact/GUID-owned sidecar를 정리한다.
- Lifetime single-writer lease/CAS, 실제 frozen 0.42.65 reader, FI-02/FI-04, power-loss/exactly-once는
  open이다. `host_mutation_performed=false`, package/installed product/actual VM 변경은 없고 operational
  anchor는 `0.42.65-admin-smoke` carry-forward다. Public trusted signing과 external stable publication은
  열지 않았다.

## 2026-08-03 C# architecture Wave 2A legacy installed checkpoint post-reboot PASS

- `docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`는
  재부팅 후 former IPv4/IPv6 `7765-7864` covering range가 사라진 상태에서 동일 final
  `0.42.66-admin-smoke` MSI 설치 exit 0을 기록한다. MSI SHA-256은
  `7249539f2c1c4d597fc73801a1de443bf791bcee13e0e13b3904c86435a83464`, provenance commit은
  `3c16f78568cfb54a0cbe586449a540df3596bcf1`다.
- 설치본 service는 `Running`/`Auto`/`LocalSystem`, manifest는 `0.42.66-admin-smoke`, Host/CLI
  hash는 package와 일치하고 TUI는 없다. Web `/`와 `/pcv-config.js`는 200, unauthenticated API는
  401/`PCV_AUTH_REQUIRED`, protected-token PCVCLI `runtime policy`는 exit 0이다.
- Installed listener hardening은 `ok=true`, body cap 413, runtime/jobs/diagnostics/console read 200,
  missing-job cancel 404이며 rate-limit/route-timeout probe는 실행하지 않았다. ProgramData store는
  version 1, jobs 18/queue 0/running 0과 SHA-256
  `78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b`를 그대로 보존했다.
- 전체 checkpoint는 MSI/service 설치 때문에 `host_mutation_performed=true`지만
  `hyperv_mutation_performed=false`이고 provider/actual-VM/full-admin/update/rollback/repair/uninstall/
  remove-data를 실행하지 않았다. 최종 service는 설치·실행 상태를 유지한다. 이는 internal unsigned
  development checkpoint이며 `0.42.65-admin-smoke` operational anchor, public trusted signing 또는
  external stable publication을 변경하지 않는다.
- 2026-08-02 failed-install orphan-service cleanup 결함은 성공 설치와 별개로 계속 open이다.

## 2026-08-03 C# architecture Wave 5A bounded admission/lifetime code slice

- `docs/ga-ready/evidence/csharp-architecture-wave5a-admission-lifetime-code-slice-2026-08-03.md`는
  기본 `legacy`를 유지한 `tracked_async_serialized` opt-in host admission, body-read 이전 503
  overload contract, noVNC 포함 request task tracking과 shutdown drain을 기록한다.
- Host tests `186/186`, 전체 .NET `815/815`, skip 0을 확인했다. ASP.NET Core package/framework
  reference, installed service, Hyper-V/VM mutation은 변경하거나 실행하지 않았다.
- 기존 ADR-0013은 job-store single-writer 결정으로 유지하고 ASP.NET Core server/rollout 후보는
  Wave 5A 및 ADR-0012 종결 후 ADR-0014로 예약했다.

## 2026-08-03 ADR-0012 API read concurrency decision

- `docs/adr/0012-api-read-concurrency-policy.md`는 현재 processor-wide serialization과 single
  mutation worker를 유지하고 bounded concurrent-read 대안을 `closed-not-adopted`로 종결한다.
- `tracked_async_serialized` host admission은 request lifetime/backpressure만 소유하며 API read
  concurrency 의미를 바꾸지 않는다. Wave 6 ASP.NET Core server decision은 ADR-0014로 예약한다.

## 2026-08-03 C# architecture Wave 5A package preflight

- `docs/ga-ready/evidence/csharp-architecture-wave5a-package-preflight-2026-08-03-04268.md`는
  commit `f93370610bf221da00e89131d874e903ba72b644`의 `0.42.68-admin-smoke` self-contained MSI
  package candidate를 `PACKAGE_PASS / INSTALLED_BLOCKED`로 기록한다. MSI SHA-256은
  `99957937f00c3f26392cae86df7ea090d84f6020821348cc6eb879dd667a2e70`, payload aggregate는
  `b0e47050aab167890c1a3e0bec09e4eb6f4889eb1068c1896d58ec8f15d1afa8`다.
- 현재 candidate는 legacy HttpListener 기본값을 유지하며 installed/service/Hyper-V mutation은
  실행하지 않았다. `0.42.65-admin-smoke` operational anchor는 변경하지 않는다.

## 2026-08-03 C# architecture Wave 5A administrator install and installed PCVCLI smoke

- `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md`는
  명시적으로 승인된 관리자 MSI 설치를 `INSTALL_PASS`로 기록한다. `0.42.68-admin-smoke` MSI
  exit `0`, installed manifest/service `Running`/`Automatic`, Web `/`·`/pcv-config.js` HTTP
  `200`, unauthenticated API `401`, Web-port API rejection `404`를 확인했다.
- 보호 토큰을 argv에 노출하지 않은 elevated PCVCLI smoke에서 `runtime policy`, `host status`,
  `ops summary`가 모두 exit `0`이었다. Artifact는
  `artifacts/installed-cli-smoke-20260803-04268/summary.json`이며 token/password 값은 기록하지
  않았다.
- `host_mutation_performed=true`는 MSI/service lifecycle 범위만 뜻하고,
  `hyperv_mutation_performed=false`, actual-VM/provider mutation `false`다. IPv4/IPv6
  `7765-7864` covering range는 없고 exact `7777-7777`은 활성 HTTP.sys listener와 결합되어
  있으며 HNS/WinNAT/portproxy/WSL owner는 확인되지 않았다. Operational anchor, public trusted
  signing과 external stable publication은 변경하지 않는다.

## 2026-08-03 C# architecture Wave 5A partial listener-bind cleanup

- `docs/ga-ready/evidence/csharp-architecture-wave5a-listener-bind-cleanup-2026-08-03.md`는
  두 번째 listener bind 또는 processor 생성 실패 시 이미 열린 listener를 stop/close하는
  cleanup boundary를 `CODE_LEVEL_PASS`로 기록한다. 점유된 Web prefix를 사용한 회귀 테스트가
  API prefix 재바인드까지 PASS했고, 전체 .NET 테스트는 `816/816`, skip `0`이다.
- 이 slice는 host/service, package, Hyper-V/VM/provider mutation을 실행하지 않으며
  operational anchor와 public distribution claims를 변경하지 않는다.

## 2026-08-02 C# architecture Wave 2A job durability completion and initial legacy checkpoint blocker (historical)

- `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`는
  create/start/cancel/complete persist-before-publish, running recovery, semantic integrity,
  transaction lease/loaded-base CAS, cancel ordering과 redacted Host 관찰성까지 Wave 2A source
  `code_complete`로 기록한다.
- .NET 795/795·skip 0, Runtime 120/120, API 228/228, Host 181/181, gap registry 10/10,
  job-hardening 10/10과 Release/L 7/7 suite `ok=true`를 확인했다. Frozen `0.42.65-admin-smoke` reader는 current writer의 v1/v2
  terminal/FIFO queue initial/restored 8/8과 Pester 5/5, 모든 hash 불변을 확인했다.
- Final `0.42.66-admin-smoke` MSI는 commit `3c16f785`, SHA-256 `7249539f2c1c4d597fc73801a1de443bf791bcee13e0e13b3904c86435a83464`로 빌드됐다. 설치는 host TCP excluded range `7765-7864`가 API 7777을 포함해 두 번 1603/1722로 실패했고, frozen 0.42.65도 7777에서 동일 실패해 `blocked-by-host-tcp-excluded-port-7777`로 판정했다.
- Failed-install MSI transaction은 product files/registration을 자동 rollback했지만 각 시도에서 stopped owned service를 남겼다. exact `ImagePath`가 이번 product root를 가리키는지 확인한 뒤 수동 삭제했고, controlled service 비교까지 정리한 최종 service/product root/uninstall entry/listener는 없다.
- ProgramData jobs 18/queue 0/running 0과 SHA-256 `78e36aee9d23db2178979a2d80de198040d616651df968d5f53ab7e7bc07c05b`는 불변이다. Hyper-V, actual VM, full-admin gate와 explicit product update/rollback workflow는 실행하지 않았다. 여기서 MSI rollback은 실패한 설치 transaction의 자동 rollback만 뜻하며, failed-install service cleanup gap은 후속 packaging lifecycle 결함으로 남긴다.
- 이 internal unsigned checkpoint는 `0.42.65-admin-smoke` full-admin/actual-VM operational anchor를
  대체하지 않으며 public trusted signing과 external stable publication을 열지 않는다.

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

## 2026-07-15 functional correctness runtime hardening code-level pass

- `docs/ga-ready/evidence/functional-correctness-runtime-hardening-2026-07-15.md`는 감사 실측으로
  확인된 FC-01/02/04/16/18의 수정과 회귀 테스트를 `CODE_LEVEL_PASS`로 기록한다.
- .NET solution `591/591`, 제품 action Pester `55/55`가 PASS했다. 디스크 shrink 사전 차단,
  network QoS Kbps↔bps 변환, rollback/backup 보상, unreadable evidence 후순위화를 포함한다.
- 이 code-level evidence 자체는 `host_mutation_performed=false`, `installed_product_changed=false`,
  `package_build_performed=false`다. 이후 승인된 `0.42.64-admin-smoke` package/fullgate에서 실제
  VM QoS/disk는 승격됐고 installed update/rollback은 여전히 별도 manual-admin campaign 범위다.

## 2026-07-14 0.42.63 CLI/Web-only fullgate and installed pass

- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-14-04263-hostmutation.md`는
  `full-admin-host-mutation-gate-20260714-04263` actual-host PASS를 기록한다. Operational MSI
  SHA-256은 `6a520e52042bdca5d55b73a4614aa0ebddaf54d576ddf60739146c2ad6784589`, payload aggregate
  SHA-256은 `be53d348199ee7fab95b3b4148d805d81aa98f80aa330cc376e94216e6db210e`, provenance는
  `9a020dec285d4fbbfe161ca2d31242f305cde572`다.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-14-04263.md` /
  `artifacts/installed-operator-surface-current-card-20260714-04263/summary.json`는 Web/CLI,
  service `Running/Automatic`, internal switch topology, `tui_present=false`, secret observation
  false를 PASS했다.
- 0.42.62 Web/TUI/CLI current-card는 historical TUI predecessor로 보존한다. 최신 closed
  manual-admin package-pair는 `0.42.58-admin-smoke -> 0.42.59-admin-smoke`다.
- 이 evidence는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke 전용이며 public trusted
  signing 또는 external stable publication을 주장하지 않는다.

## 2026-07-14 0.42.62 -> 0.42.63 manual-admin follow-up blocked

- `docs/ga-ready/evidence/manual-admin-campaign-2026-07-14-04262-04263.md`는
  `0.42.62-admin-smoke -> 0.42.63-admin-smoke` PlanOnly readiness의 `BLOCKED` 결과를 기록한다.
- Readiness exit는 `0`이지만 설치본이 이미 `0.42.63-admin-smoke`여서 campaign blocker는
  `blocked-by-installed-baseline-version-mismatch`다. `host_mutation_performed=false`이며 closed
  descriptor나 Burn/MSIX runner JSON을 생성하지 않았다.
- 최신 closed manual-admin package-pair는 계속 `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
  `manual-admin-campaign-descriptor-20260529-04258-04259-closed`다. 0.42.63
  package/fullgate/CLI-Web installed current-card operational anchor도 유지한다.

## 2026-07-14 0.42.63 CLI/Web-only package build

- 최신 package build는 `0.42.63-admin-smoke`이며
  `docs/ga-ready/evidence/admin-smoke-package-2026-07-14-04263.md`가 `PACKAGE_BUILD_PASS`를
  기록한다. MSI SHA-256은
  `d2f2fff7fb400647135d96449f36704af2d080e1a6a97a551354290cdf1a6f04`, payload aggregate
  SHA-256은 `19f80f3e0b849d180a3e62461742a8a2ab7371e632dbfecfc8fad28bf59721f4`다.
- Payload manifest는 schema `2`이며 Host/CLI를 포함하고 `pcvtui.exe`, root `tui`,
  `paths.tui_exe`를 포함하지 않는다. MSI File table에도 active TUI file row가 없다.
- 이 package는 0.42.63 operational full admin host mutation과 CLI/Web installed
  current-card까지 actual-host PASS로 승격됐다. 0.42.62는 historical TUI predecessor다.
- 이 package는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke 전용이며 public trusted signing
  또는 external stable publication을 주장하지 않는다.

## 2026-07-14 TUI removal and CLI/Web-only code-level pass

- `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`는 ADR-0011의
  `cli-web-only` active surface와 TUI source/package/smoke 제거를 code-level PASS로 기록한다.
- Local API/backend는 유지된다. Code-level evidence 자체는 `host_mutation_performed=false`이며,
  별도 0.42.63 package/fullgate/CLI-Web installed evidence가 actual-host PASS를 소유한다.
- `0.42.62-admin-smoke` Web/TUI/CLI installed current-card는 historical TUI predecessor다.

## 2026-07-13 post-0.42.62 operational follow-up (salvaged from abandoned PR #172)

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md`는
  PR #171 merge head의 main-push Public Boundary PASS를 run/job/head로 기록하며
  `additional_package_candidate_opened: false`다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-07-13-04259-04262.md`는
  `0.42.59-admin-smoke -> 0.42.62-admin-smoke` manual-admin 시도를
  `BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED`로 기록한다. PlanOnly readiness가 설치본 baseline
  불일치를 반환했고 전용 baseline host와 protected credential reference가 구성되지 않았다.
  `manual_admin_current_closure_changed: false`이며 current closed pair는 계속
  `0.42.58-admin-smoke -> 0.42.59-admin-smoke`다. 이후 별도로 기록된
  `0.42.62 -> 0.42.63` blocked follow-up과는 다른 시도다.
- `docs/ga-ready/evidence/secondary-hyperv-wmi-topology-smoke-2026-07-13-04262.md`는
  secondary Hyper-V host 부재로 `BLOCKED_NO_SECONDARY_HYPERV_HOST`를 기록하며 단일 host PASS를
  multi-host로 승격하지 않는다.
- `docs/ga-ready/evidence/post-04262-worktree-cleanup-2026-07-13.md`는 worktree 18개 감사와
  15개 registration 제거를 기록한다. Windows filename-too-long으로 잔여 경로 5개를 보존했고
  force removal은 사용하지 않았다.
- 이 네 문서는 2026-07-13에 작성됐으나 PR #172가 병합되지 않아 저장소에 들어오지 못했다.
  2026-08-05에 증거만 복원했으며 당시 index/ledger 본문 편집은 가져오지 않았다. 현재
  operational anchor와 manual-admin closure는 `docs/ga-ready/current-evidence.json`이 소유한다.
- 네 문서 모두 `host_mutation_performed: false`이며 public trusted signing과 external stable
  publication을 주장하지 않는다.

## 2026-07-13 0.42.62 WMI internal switch topology recovery

- 통합 RCA `docs/ga-ready/evidence/wmi-internal-switch-topology-recovery-2026-07-13-04260-04262.md`는
  `0.42.60-admin-smoke`의 `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`와
  `0.42.61-admin-smoke`의 `PCV_NETWORK_INVENTORY_FAILED` /
  `System.InvalidOperationException`을 보존한다. 두 package는 설치/MSI lifecycle은 PASS했지만
  full gate 첫 단계에서 실패했고 OS mutation은 실행되지 않았으므로 PASS anchor가 아니다.
- 최신 package version은 `0.42.62-admin-smoke`이며 evidence는
  `docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md`다.
  Clean MSI SHA-256은 `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`,
  payload aggregate SHA-256은 `0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e`다.
- 최신 operational anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04262-hostmutation.md` /
  `full-admin-host-mutation-gate-20260713-04262`다. 두 단계가 모두 PASS했고 operational MSI
  SHA-256은 `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`, payload aggregate
  SHA-256은 `ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`, provenance는
  `7f71f0a518c5b592f233373522d36b5401c3f1df`다.
- 2026-07-13 installed predecessor current-card는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04262.md` /
  `artifacts/installed-operator-surface-current-card-20260713-04262/summary.json`에서 PASS했다.
- 최신 closed manual-admin package-pair는 별도 campaign이 없었으므로 계속
  `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
  `manual-admin-campaign-descriptor-20260529-04258-04259-closed`다.
- 이 증거는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke 전용이며 public trusted signing과
  external stable publication을 주장하지 않는다.

## 2026-07-13 개발 게이트 복구 code-level local pass

- `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`는 비관리자
  Windows에서 CLI default-token 경로와 Host ACL hardening을 단위 테스트에서 격리하고,
  CLI protected-token 실패를 안정된 redacted code로 정규화한 결과를 기록한다.
- `Development Gates`의 .NET, Web, packaging Pester, installer/Web Pester 네 job 계약과 전체
  비변경 로컬 gate는 PASS했다. 원격 CI는 승인된 push 전이므로 아직 실행하지 않았다.
- `host_mutation_performed=false`, `package_build_performed=false`이며 설치본 anchor는
  `0.42.59-admin-smoke`로 유지한다. `0.42.60-admin-smoke`는 별도 승인 대상이다.

## 2026-05-29 0.42.59 public-boundary docs-maintenance postpush pass

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`는
  `0.42.59-admin-smoke` closure의 public-boundary postpush evidence가 main에 문서화된 뒤
  Public Boundary Contract가 다시 PASS했음을 기록한다. Run은 `26636072420`, job은 `78496568595`,
  head는 `5a2f91762a6c2a8ab6b84d334fa6cb420474671f`다.
- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-admin-smoke-closure-postpush-pass.md`가
  `0.42.60-admin-smoke` installed current-card payload 후보를 이미 열었으므로, 이번 docs-maintenance
  postpush evidence는 최신 CI verification만 갱신하고 추가 package 후보를 열지 않는다.
- account/noVNC Operator Surface는 `0.42.58-admin-smoke` 이후 payload 변경이 없으므로
  `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md` PASS를
  carry-forward한다.
- Actual VM Guest Execution/QoS smoke는 Guest Execution provider/direct-control 또는 Hyper-V QoS
  provider/control payload가 바뀔 때 재실행한다. 이번 public-boundary evidence rollup만으로는
  재실행하지 않는다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-29 0.42.59 public-boundary postpush pass and 0.42.60 payload decision

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-admin-smoke-closure-postpush-pass.md`는
  `0.42.59-admin-smoke` package/fullgate/manual-admin/current-card closure가 main에 push된 뒤
  Public Boundary Contract가 PASS했음을 기록한다. Run은 `26629340294`, job은 `78473968530`,
  head는 `b1733c1d9777d2c0828897ae2751af33a270b2fe`다.
- 이 evidence는 Runtime/API `current_evidence.public_boundary.latest_main_push`와
  CLI/TUI/Web current-card가 읽는 current evidence input이므로, 다음 installed current-card payload
  후보를 `0.42.60-admin-smoke`로 열었다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-29 0.42.59 package fullgate manual-admin closure

- `0.42.59-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md`가 기록한다. Clean MSI
  SHA-256은 `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`, payload
  aggregate SHA-256은 `666a1351d58963c7908aad4f66d6469de42747a7c7f70d1e30fb0e94771a5808`,
  provenance commit은 `63d57feba605f82dabd44a96ed50a4d622f6310a`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md` /
  `full-admin-host-mutation-gate-20260529-04259`다. Full-gate MSI SHA-256은
  `dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`, payload aggregate
  SHA-256은 `3f015e7743efac3b61de81962c236a03c1bcf882053fc92fd3c525da280a1687`이다.
- 최신 manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md` /
  `manual-admin-campaign-descriptor-20260529-04258-04259-closed`다.
  `0.42.58-admin-smoke -> 0.42.59-admin-smoke` installed update/rollback, dedicated
  clean-host Windows Update, Burn, MSIX, installed runtime ops summary가 모두 PASS이며
  `missing_count=0`, `not_pass_count=0`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`와
  `artifacts/installed-operator-surface-current-card-20260529-04259/summary.json`가 기록한다.
- Guest Execution redaction hardening과 Hyper-V QoS mutation value hardening의 code-level
  evidence는 이 package/fullgate/manual-admin/current-card chain으로
  `pass-code-level-promoted-by-04259-package-chain` 상태로 승격했다.
- account/noVNC operator surface rerun은 `0.42.58-admin-smoke` evidence를 최신 PASS로
  보존한다. `0.42.59-admin-smoke`에서는 account/noVNC payload 변경이 없었으므로 rerun하지 않았다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-29 Hyper-V QoS mutation value hardening code-level

- Hyper-V QoS mutation value-boundary hardening은
  `docs/ga-ready/evidence/hyperv-qos-mutation-value-hardening-code-level-2026-05-29.md`가
  기록한다. Local API preview/apply와 PCVCLI `vm blkio-set` / `vm bandwidth-set`은 음수,
  `1,000,000,000` 초과, `minimum > maximum` 값을
  `PCV_VM_QOS_STORAGE_RANGE_INVALID` 또는 `PCV_VM_QOS_NETWORK_RANGE_INVALID`로 먼저
  거절한다.
- Invalid preview는 native adapter를 호출하지 않고, invalid apply는 queued job을 만들지
  않는다. Rollback/manual restore semantics 때문에 `0`은 유효한 값으로 유지한다.
- 이 evidence 자체는 code-level로 보존하되, 후속 `0.42.59-admin-smoke`
  package/fullgate/manual-admin/current-card chain에서 설치본 evidence로 승격했다. 승격 상태는
  `pass-code-level-promoted-by-04259-package-chain`이다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-29 Guest Execution redaction hardening code-level

- Guest Execution redaction hardening은
  `docs/ga-ready/evidence/guest-execution-redaction-hardening-code-level-2026-05-29.md`가
  기록한다. `GuestExecutionRedactor`는 AWS access-key shape와 공백 없는 고엔트로피 token
  shape를 secret-like material로 분류하고, guest-exec preview/queued execute는
  `PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED`로 차단한다.
- 이 evidence 자체는 code-level로 보존하되, 후속 `0.42.59-admin-smoke`
  package/fullgate/manual-admin/current-card chain에서 설치본 evidence로 승격했다. 승격 상태는
  `pass-code-level-promoted-by-04259-package-chain`이다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-29 0.42.58 package fullgate manual-admin closure

- Public-boundary main push evidence는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04257-main-push-pass.md`가
  기록한다. Run은 `26587524245`, job은 `78337437665`, head는
  `96182b440b35c17183802ad323a123ff6e4b6730`다.
- `0.42.58-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md`가 기록한다. Clean MSI
  SHA-256은 `6ae889eeb1b7134fab9618941748528f6260727abbc8ff36eee301b59dff6c0b`, payload
  aggregate SHA-256은 `9e162bc59527d107c0c6e35105bd5a0f17c7449a94e23cfe138cdc268f3d7184`,
  provenance commit은 `96182b440b35c17183802ad323a123ff6e4b6730`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md` /
  `full-admin-host-mutation-gate-20260529-04258`다. Full-gate MSI SHA-256은
  `7e0aef503b3f56eb116d5931c9560a3dcd2c4ba347f1eb24e4b505b28e6c2845`이다.
- 최신 manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md` /
  `manual-admin-campaign-descriptor-20260529-04257-04258-closed`다.
  `0.42.57-admin-smoke -> 0.42.58-admin-smoke` installed update/rollback, dedicated
  clean-host Windows Update, Burn, MSIX, installed runtime ops summary가 모두 PASS이며
  `missing_count=0`, `not_pass_count=0`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`와
  `artifacts/installed-operator-surface-current-card-20260529-04258/summary.json`가 기록한다.
- account/noVNC rerun은
  `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`,
  `artifacts/installed-account-login-smoke-20260529-04258`,
  `artifacts/target-backed-novnc-installed-streaming-smoke-20260529-04258-r2`에서 PASS다.
  token/password/refresh-token 노출은 `false/false/false`다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-28 0.42.57 payload target selection

- `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md`는
  0.42.56 manual-admin closure main push의 public-boundary CI PASS를 기록한다. Run은
  `26578120570`, job은 `78303066840`, head는
  `7a7d5de822bdb058b04149eeeef0a7eb462828b5`다.
- 다음 product payload는 이 public-boundary current evidence를 Runtime/API
  `current_evidence.public_boundary.latest_main_push` fallback과 CLI/TUI/Web current-card에
  노출하는 `0.42.57-admin-smoke` package target으로 선정했다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-28 0.42.57 package fullgate manual-admin closure

- `0.42.57-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04257.md`가 기록한다. Clean MSI
  SHA-256은 `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`, payload
  aggregate SHA-256은 `c24512aec2dae7e73da4af24778451b3b3dfdc52d2c7914db61ceaaefae67e07`,
  provenance commit은 `16cc0d6b592d7f2f9ead14c41d8f4ad0e1f28b76`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04257-hostmutation.md` /
  `full-admin-host-mutation-gate-20260528-04257`다. Full-gate MSI SHA-256은
  `809eacb97a49aeaa32fc0ea3dce8ac5bdeb7c66b8b4502352519a338a512847e`, payload aggregate
  SHA-256은 `7a34468d3a59c2da182835a03f440f22df9e70f31ff062dc625530a9143ef94d`이다.
- 최신 manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04256-04257.md` /
  `manual-admin-campaign-descriptor-20260528-04256-04257-closed`다.
  `0.42.56-admin-smoke -> 0.42.57-admin-smoke` installed update/rollback, dedicated
  clean-host Windows Update, Burn, MSIX, installed runtime ops summary가 모두 PASS이며
  `missing_count=0`, `not_pass_count=0`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04257.md`와
  `artifacts/installed-operator-surface-current-card-20260528-04257/summary.json`가 기록한다.
  CLI `current.public_boundary_main_push`, TUI `PUBLIC BOUNDARY CURRENT`, Web `Public boundary head`
  표시를 확인했다.
- account/noVNC rerun은
  `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04257.md`,
  `artifacts/installed-account-login-smoke-20260528-04257`,
  `artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04257`에서 PASS다.
  token/password/refresh-token 노출은 `false/false/false`다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-28 0.42.56 manual-admin package-pair closure

- `0.42.56-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04256.md`가 기록한다. Clean MSI
  SHA-256은 `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`, payload
  aggregate SHA-256은 `5670772a193c996fadc0dbe1a9e45ec0ab908bd124092d1a328c22b5e0c7e699`,
  provenance commit은 `5594adc55b013a2bf3ade9c6ae7171ca37bdbeb0`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04256-hostmutation.md` /
  `full-admin-host-mutation-gate-20260528-04256`다. Full-gate MSI SHA-256은
  `085792312b3bba3ba241882156212b40f936748b08a0ad56ae4a877b24759dec`, payload aggregate
  SHA-256은 `98057c20aacd109d451a4b18b5ecb16b012d46bc85443562d3be149be0a0a7f2`이다.
- 최신 manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04255-04256.md` /
  `manual-admin-campaign-descriptor-20260528-04255-04256-closed`다.
  `0.42.55-admin-smoke -> 0.42.56-admin-smoke` installed update/rollback, dedicated
  clean-host Windows Update, Burn, MSIX, installed runtime ops summary가 모두 PASS이며
  `missing_count=0`, `not_pass_count=0`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md`와
  `artifacts/installed-operator-surface-current-card-20260528-04256/summary.json`가 기록한다.
  CLI `current.manual_admin_next_package_pair`, TUI `MANUAL ADMIN NEXT`, Web `Manual admin next`
  표시를 확인했다.
- account/noVNC rerun은
  `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04256.md`,
  `artifacts/installed-account-login-smoke-20260528-04256`,
  `artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04256`에서 PASS다.
  token/password/refresh-token 노출은 `false/false/false`다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-28 0.42.55 follow-up rebaseline and account/noVNC rerun

- `docs/ga-ready/evidence/post-04255-followup-execution-2026-05-28.md`는 사용자 승인
  `1-2-3-4-5-6` 후속 실행을 닫는다. Installed version은 `0.42.55-admin-smoke`,
  manual-admin rebaseline readiness artifact는
  `artifacts/manual-admin-campaign-20260528-04255-next/rebaseline-readiness`이며,
  package pair decision은 `not-opened-no-next-product-payload-target`이다.
- 다음 package-pair 후보는 `0.42.55-admin-smoke -> next-admin-smoke-required`로 재정의했다.
  이 turn에는 새 product payload target이 없어 0.42.56 package build/package-pair를 열지 않았고,
  다음 product payload가 생길 때 target version을 확정한다.
- `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup.md`는
  04255 설치본 account login/browser smoke와 target-backed noVNC streaming rerun을 기록한다.
  `artifacts/installed-account-login-smoke-20260528-04255-followup`,
  `artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04255-followup` 모두 PASS이며,
  token/password/refresh-token 노출은 `false/false/false`다.
- Guest Execution credential/audit/redaction/timeout/cancel, Hyper-V QoS mutation, Web/TUI direct control은
  기존 닫힌 0.42.55/0.42.48 evidence를 carry-forward했다. Public trusted signing과 external stable
  publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-28 0.42.55 Web/TUI running cancel affordance installed fullgate

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-28-04255-installed-running-cancel-affordance`이다.
- `0.42.55-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04255.md`가 기록한다. Clean MSI
  SHA-256은 `530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627`, payload
  aggregate SHA-256은 `ada13e719c47a439c8836fc2138f6419d447fc1eccfcd02fe73d3686a2127ef6`,
  provenance commit은 `958052181012f7d1be6ccff535316bfaeeef07df`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04255-hostmutation.md` /
  `full-admin-host-mutation-gate-20260528-04255`다. Full-gate MSI SHA-256은
  `cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb`, payload aggregate
  SHA-256은 `69019129347920bba88c269a4828dae5b214eace8a6d31bd60bc7fa7f1b81934`,
  provenance commit은 `958052181012f7d1be6ccff535316bfaeeef07df`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md`,
  `artifacts/installed-operator-surface-current-card-20260528-04255/summary.json`가 기록한다.
  runtime policy는 `running_interrupt=true`, `queued_only=false`, `guest_execution.enabled=true`,
  `execute_enabled=true`를 보고하고, Web running cancel affordance는 설치본에서 확인됐다.
- 실제 Windows guest credentialed execution smoke는
  `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md`가
  소유한다. `pcv-guest-installed-04253-r1` persistent Windows VHD target, DPAPI LocalMachine
  credential reference, channel verify job `job-92e44ca99cde460b9e34567168dbb7cd`, guest-exec
  job `job-0e05ae5a574d49a5822237337c1e9ad3`가 모두 `succeeded`로 닫혔다.
- `0.42.50-admin-smoke -> 0.42.54-admin-smoke` manual-admin readiness 재확인은
  `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-04255-fullgate.md`가
  기록한다. baseline/target artifact는 모두 존재하지만 현재 host가 이미 `0.42.55-admin-smoke`라
  baseline 0.42.50 설치 조건을 만족하지 못해 `blocked-by-installed-baseline-version-mismatch`로
  닫았다.
- Persistent Windows guest target keep/delete policy는
  `docs/ga-ready/evidence/persistent-windows-guest-target-policy-2026-05-28-04255.md`가 기록한다.
  `pcv-guest-installed-04253-r1`은 삭제하지 않고 다음 evidence cycle까지 보존한다.
- 최신 public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md`,
  run `26578120570`, job `78303066840`, head
  `7a7d5de822bdb058b04149eeeef0a7eb462828b5`에서 PASS했다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-28 0.42.54 Guest Execution running cancel fullgate roll-forward

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-28-04254-fullgate-running-cancel-rollforward`이다.
- `0.42.54-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04254.md`가 기록한다. MSI
  SHA-256은 `a0181bd156e4e01a57c177639a3eb418009f6fd9dd8bf090a3bb123e69aad36b`, payload
  aggregate SHA-256은 `8443b217a45551bfcaf28d366ff33af80f95fc4527509addf4919621472f6bb3`,
  provenance commit은 `5a1058f55fcd42d28c7075514e1924c5ccdfb525`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04254.md`,
  `artifacts/installed-operator-surface-current-card-20260528-04254/summary.json`가 기록한다.
  runtime policy는 `running_interrupt=true`, `queued_only=false`, `guest_execution.enabled=true`,
  `execute_enabled=true`를 보고하고, `pcv-guest-installed-04253-r1` projection은
  `guest_family=windows`다.
- 실제 Windows guest running cancel smoke는
  `docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md`가
  소유한다. `job-b06eb90e549a481bbf4003399b5604f8`는 running 상태에서 cancel 요청 후
  최종 `canceled`, `PCV_JOB_CANCELED`, `PCV_NATIVE_OPERATION_CANCELED`로 닫혔다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04254-hostmutation.md` /
  `full-admin-host-mutation-gate-20260528-04254`다. Full-gate MSI SHA-256은
  `937ac686aa782a69dc41d06d8694a020cf4a78b45cf7a6674e85593cce3c4cb1`, payload aggregate
  SHA-256은 `bdcb61002f5e3e739ca3db5cb0a189548b9c9b25ef5747c437c7b23d615fef84`,
  provenance commit은 `2c11e359709c775be7a57ea9624716720c5b62d6`이다.
- `0.42.50-admin-smoke -> 0.42.54-admin-smoke` manual-admin package-pair readiness는
  `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-fullgate.md`가
  기록한다. baseline/target artifact는 모두 존재하지만 현재 host가 이미 `0.42.54-admin-smoke`로
  올라간 상태라 baseline 0.42.50 설치 조건을 만족하지 못해
  `blocked-by-installed-baseline-version-mismatch`로 닫혔다.
- Persistent Windows guest target keep/delete policy는
  `docs/ga-ready/evidence/persistent-windows-guest-target-policy-2026-05-28-04254.md`가 기록한다.
  `pcv-guest-installed-04253-r1`은 삭제하지 않고 다음 evidence cycle까지 보존한다.
- Web/TUI running guest execution cancel affordance는
  `docs/ga-ready/evidence/web-tui-running-job-cancel-affordance-code-level-2026-05-28.md`가 code-level
  PASS로 기록한다. 이 변경은 0.42.54 fullgate 이후 code-level payload이므로 다음 package/current-card에서
  설치본으로 승격해야 한다.
- 최신 public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04254-running-cancel-evidence-rollforward-postpush-pass.md`,
  run `26556328902`, job `78228845568`, head
  `2c11e359709c775be7a57ea9624716720c5b62d6`에서 PASS했다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-27 0.42.53 Guest Execution provider/direct-control installed anchor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-27-04253-guest-execution-provider-direct-control-fullgate`이다.
- `0.42.53-admin-smoke` clean package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-27-04253.md`가 기록한다. Clean MSI
  SHA-256은 `39df998c061d9dcecbbc21a966f9ffb495f27502922f2057bd5defc93c9a19ea`, payload
  aggregate SHA-256은 `7cdf2a98d2076149b0c1e6215d85e6b92968066308e15c77aa2eb25fe80745d9`,
  provenance commit은 `cc774b257d6cd772c3a890266aca62aa8ab8eadc`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-27-04253-hostmutation.md` /
  `full-admin-host-mutation-gate-20260527-04253`가 소유한다. Operational MSI SHA-256은
  `14eb351000d3f6324edde5d785040667a5ddbea952cea1e20183a28882b9c669`, payload aggregate
  SHA-256은 `da633431d611acb8e762cb25d1e4c9530ba87887fa6fd92ba4216b70b8ce4ff4`,
  provenance commit은 `cc774b257d6cd772c3a890266aca62aa8ab8eadc`이다.
- 최신 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-27-04253.md`,
  `artifacts/installed-operator-surface-current-card-20260527-04253/summary.json`가 기록한다.
  runtime policy는 `guest_execution.enabled=true`, `execute_enabled=true`, channel verify/repair
  enabled이며 `vm.guest.exec`, `vm.guest.channel.verify`, `vm.guest.channel.ensure` native mutation
  dispatch를 보고한다.
- Guest Execution provider/direct-control code-level evidence는
  `docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253.md`가
  소유한다. API/CLI queued provider route, Web/TUI direct-control surface, dry-run preview
  provider-open 상태를 확인했다. 실제 Windows guest credentialed execution smoke는
  `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass.md`가
  소유한다. `pcv-guest-installed-04253-r1` persistent Windows VHD target, DPAPI LocalMachine
  credential reference, `guest-agent-ensure-channel --verify`, `guest-exec -- hostname`,
  Web HTTP 200, TUI VM row projection이 PASS다. 이전
  `docs/ga-ready/evidence/guest-execution-actual-vm-web-tui-smoke-2026-05-27-04253-blocked.md`와
  `artifacts/guest-execution-windows-iso-boot-shell-smoke-20260527-04253-r1/summary.json`는
  Windows ISO attach/start/readback/poweroff/delete cleanup predecessor로 보존한다.
- Guest Execution running cancel policy는
  `docs/ga-ready/evidence/guest-execution-running-cancel-policy-2026-05-27-04253.md`가 기록한다.
  0.42.53은 queued cancel과 provider timeout을 지원하고 running guest process interrupt는
  `docs/superpowers/specs/2026-05-27-purecvisor-desktop-node-guest-execution-running-interrupt-cancel-design.md`에
  따라 별도 product payload로 defer한다.
- `0.42.50-admin-smoke -> 0.42.53-admin-smoke` manual-admin package-pair readiness는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-27-04250-04253-readiness-blocked.md`가
  기록한다. 현재 host가 이미 `0.42.53-admin-smoke`로 올라간 상태라 baseline 0.42.50 설치
  조건을 만족하지 못해 `blocked-by-installed-baseline-version-mismatch`로 닫혔다.
- 추가 baseline host prep과 closure attempt는
  `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-27-04250-04253-blocked-post-ci-rollforward.md`에
  plan-only evidence로 남겼다. 현재 host downgrade 없이 dedicated 0.42.50 baseline host가 필요하다.
- 최신 public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md`,
  run `26518952796`, job `78104102372`, head
  `12bc72e856ea9ac7c6d54c4094873b2d8db9f672`에서 PASS했다.
  직전 credentialed Windows guest execution smoke public-boundary run은 `26516950720`이다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-27 0.42.50 Guest Execution preview API/CLI predecessor

- `0.42.50-admin-smoke`는 Guest Execution API/CLI preview package/fullgate/current-card
  predecessor로 보존한다.
- Evidence:
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-27-04250.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-27-04250-hostmutation.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-27-04250.md`,
  `docs/ga-ready/evidence/guest-execution-preview-code-level-2026-05-27-04250.md`.

## 2026-05-26 0.42.49 Guest Execution policy/API preview installed anchor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-26-04249-guest-execution-policy-api-preview-fullgate`이다.
- `0.42.49-admin-smoke` clean package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`가 기록한다. Clean MSI
  SHA-256은 `322bddcb89b05a882ed323429bcfce29f6a856701b801925b53c37423de0a6e2`, payload
  aggregate SHA-256은 `e348a46ad635b61347688750162de100914ad991dd255d10892d319872f19d10`,
  provenance commit은 `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04249-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04249`가 소유한다. Operational MSI SHA-256은
  `465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32`, payload aggregate
  SHA-256은 `d49e70c1e291dd28040821fcb659222f4ff524b9c7353994f5e5447ec08610c5`,
  provenance commit은 `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`이다.
- 최신 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04249.md`,
  `artifacts/installed-operator-surface-current-card-20260526-04249/summary.json`가 기록한다.
  `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvcli --json runtime policy`, `pcvtui --smoke-once runtime`, Web `/`, `/pcv-config.js`가
  PASS했고 guest exec preview disabled route는 HTTP `403` / `PCV_GUEST_EXEC_DISABLED`를
  반환하며 secret/credential-ref echo가 없음을 확인했다.
- Guest Execution policy/API preview code-level evidence는
  `docs/ga-ready/evidence/guest-execution-policy-api-preview-code-level-2026-05-26-04249.md`가
  소유한다. Runtime policy, disabled API routes, problem code catalog, credential ref
  resolver, redaction engine, audit writer skeleton을 구현했고 execute/provider/Web/TUI command
  panel은 아직 열지 않는다.
- `0.42.48-admin-smoke -> 0.42.49-admin-smoke` manual-admin package-pair readiness는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04248-04249-readiness-blocked.md`가
  기록한다. 현재 host가 이미 `0.42.49-admin-smoke`로 올라간 상태라 baseline 0.42.48 설치
  조건을 만족하지 못해 `blocked-by-installed-baseline-version-mismatch`로 닫지 않았다.
  최신 closed manual-admin package-pair는 계속 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`다.
- Public trusted signing과 external stable publication은 계속 ADR-0006 범위 밖이다.

## 2026-05-26 ADR-0009 Guest Execution Security Boundary Contract

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  predecessor `current-evidence-ledger-2026-05-26-04248-guest-execution-security-boundary-contract`이다.
- ADR-0009 적용 문서는 `docs/adr/0009-guest-execution-security-boundary.md`다. 이전 후보
  문서 `docs/adr/0009-guest-execution-security-boundary-candidate.md`는 historical predecessor로
  보존한다.
- Docs-contract evidence는
  `docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md`가 기록한다.
  Credential reference, `guest-execution-audit-v1`, `guest-execution-redaction-v1`,
  queued timeout/cancel, `guest.exec`/`guest.channel.configure`/`job.cancel` capability,
  channel dry-run/verify/repair contract를 확정했다.
- 설계와 구현 분할은
  `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary-design.md`,
  `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md`가
  소유한다.
- 이번 변경은 product payload change가 아니므로 package build, full admin host mutation,
  manual-admin package-pair는 실행하지 않았다. Public trusted signing과 external stable
  publication도 주장하지 않는다.

## 2026-05-26 0.42.48 Phase 3 Web/TUI QoS direct control manual-admin closure predecessor

- Predecessor evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md` 이력의 ledger id
  `current-evidence-ledger-2026-05-26-04248-guest-execution-security-boundary-contract`이다.
- `0.42.48-admin-smoke` clean package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`가 기록한다. Clean MSI
  SHA-256은 `a0014960979ed23cec8d882cddd22baaaf9435a71287bdc133a79ff0b381338c`, payload
  aggregate SHA-256은 `2013756155ce1d744ab4383ffdb70dfcc6d9d7c462192b51f4425f921a53850a`,
  provenance commit은 `46e745efc698a06e4b065a19c3f07217e821155e`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04248`가 소유한다. Operational MSI SHA-256은
  `a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf`, payload aggregate
  SHA-256은 `2a14e47bf3fd48b17755ce901ec02b924ba9246ecbe91414f952428ca376d92f`, provenance
  commit은 `46e745efc698a06e4b065a19c3f07217e821155e`이다.
- 최신 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`,
  `artifacts/installed-operator-surface-current-card-20260526-04248-manual-admin/summary.json`가 기록한다.
  `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvtui --smoke-once runtime`, Web `/`, `/pcv-config.js`가 PASS했고 ops summary는
  `batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260526-04248`와
  `manual_admin.latest_package_pair.package_pair=0.42.47-admin-smoke -> 0.42.48-admin-smoke`를
  확인했다.
- 최신 closed manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md` /
  `manual-admin-campaign-descriptor-20260526-04247-04248-closed`가 소유한다.
  Update ZIP SHA-256은 `84d8c28d3cf2e8b7a5abd91d8663e99d7809b4dcc1d9ee53e2696ae091f6e32b`이고
  descriptor `missing_count=0`, `not_pass_count=0`이다.
- 다음 개발 slice는
  `docs/ga-ready/evidence/post-04248-next-slice-selection-2026-05-26.md`에서
  ADR-0009 Guest Execution security boundary로 선택했다.

## 2026-05-26 Phase 3 Web/TUI QoS direct control code-level predecessor

- Predecessor evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md` 이력의 ledger id
  `current-evidence-ledger-2026-05-26-04248-guest-execution-security-boundary-contract`이다.
- Phase 3 Web/TUI QoS direct control code-level evidence는
  `docs/ga-ready/evidence/phase3-web-tui-qos-direct-control-code-level-2026-05-26.md`가 기록한다.
  Web Console은 selected VM storage/network QoS preview/apply form을 열고, TUI는 `P`
  preview와 `A` confirmed reset apply를 연다.
- Guest Execution과 account/noVNC target config mutation은 ADR-0009/ADR-0010 경계가 닫힐
  때까지 보류다.
- 이 변경은 product payload change였고 `0.42.48-admin-smoke` package/fullgate/current-card로
  승격됐고, 이후 `0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair
  closure까지 닫혔다.

## 2026-05-26 0.42.47 closure 최신 operational anchor

- Current evidence ledger predecessor는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의
  `current-evidence-ledger-2026-05-26-04247-plus-phase3-web-tui-qos-direct-control-code-level`였다.
- `0.42.47-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04247.md`가 기록한다. MSI
  SHA-256은 `9589086d092ee902b72ff7790cac5a25e6d806cdaac0d98e431a27048dc5e197`, payload
  aggregate SHA-256은 `b206399efff98c9abf598580051ee9b81d87cc8450c4991de7d1944dafbb4aac`,
  provenance commit은 `77f1a3f291b4f736218cb5110dcecd3b464860d4`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04247-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04247`가 소유한다. Operational MSI SHA-256은
  `5c5b6abb7560e819097a79b494c150e7321018fc30f46329927ed5b3508e80f2`, payload aggregate
  SHA-256은 `fea8aa57792466d319aac33a02fe13345c5c64ac26e1dca72f8e54b0eca1e342`, provenance
  commit은 `77f1a3f291b4f736218cb5110dcecd3b464860d4`이다.
- 최신 closed manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md` /
  `manual-admin-campaign-descriptor-20260526-04245-04247-closed`다. Package pair는
  `0.42.45-admin-smoke -> 0.42.47-admin-smoke`, update ZIP SHA-256은
  `69fda75fc32a187364ac870dac01118bc4c548bebfe596660a5cd70085610a0d`,
  descriptor `missing_count=0`, `not_pass_count=0`이다. Dedicated clean-host with Windows
  Update, Burn, MSIX, installed update/rollback lifecycle이 모두 PASS다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04247.md`,
  `artifacts/installed-operator-surface-current-card-20260526-04247/summary.json`가 기록한다.
  `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvtui --smoke-once runtime`, Web `/`, `/pcv-config.js`가 PASS했고 ops summary는
  `batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260526-04247`와
  `manual_admin.latest_package_pair.package_pair=0.42.45-admin-smoke -> 0.42.47-admin-smoke`를
  확인했다.
- Phase 2 Hyper-V QoS mutation 설치본 evidence는
  `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`가 기록한다.
  `0.42.47-admin-smoke` package build, `full-admin-host-mutation-gate-20260526-04247`,
  실제 VM 대상 `pcvcli vm blkio-set`/`pcvcli vm bandwidth-set` dry-run/apply/rollback
  smoke, 04245→04247 manual-admin package-pair closure가 모두 PASS했다.
- 0.42.50 predecessor public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass.md`,
  run `26489610881`, job `78004396577`, head
  `baba155d6adfd4c9e2b2ba179d6727bb5035d1fc`에서 PASS한 post-04250 Guest Execution preview main push evidence다.
  04250 local closure는 public trusted signing 또는 외부 stable publication을 claim하지 않는다.

## 2026-05-26 0.42.45 closure historical predecessor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-26-04245-fullgate-manual-admin-closure`이다.
- `0.42.45-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04245.md`가 기록한다. MSI
  SHA-256은 `376218a0ee394e124f019e0e49a25718077585bac48f09c951da845bd96087bf`, payload
  aggregate SHA-256은 `3c1f9c9ab17144301976b9996d709c611a99122beb1296b457bf6444e2c6787a`,
  provenance commit은 `76c77a86bbb72e415b1968169c16f1638b76fa56`이다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04245-hostmutation.md` /
  `full-admin-host-mutation-gate-20260526-04245`가 소유한다. Operational MSI SHA-256은
  `379fc96a63d853deb3fb57fa44231479a3785a6f9ca58bf8c924d96410bc3246`, payload aggregate
  SHA-256은 `d0568f69ac061815d06b1a41c819594da7cbb6c577dced2382945ae4502498a3`, provenance
  commit은 `76c77a86bbb72e415b1968169c16f1638b76fa56`이다.
- 최신 closed manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04244-04245.md` /
  `manual-admin-campaign-descriptor-20260526-04244-04245-closed`다. Package pair는
  `0.42.44-admin-smoke -> 0.42.45-admin-smoke`, update ZIP SHA-256은
  `08e526c3a7bccc3cdd53a1ea8d6e3917988cbb296ddfa2089aab49342fcd1641`,
  descriptor `missing_count=0`, `not_pass_count=0`이다. Dedicated clean-host with Windows
  Update, Burn, MSIX, installed update/rollback lifecycle이 모두 PASS다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04245.md`,
  `artifacts/installed-operator-surface-current-card-20260526-04245/summary.json`가 기록한다.
  `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvtui --smoke-once runtime`, Web `/`, `/pcv-config.js`가 PASS했고 ops summary는
  `batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260526-04245`와
  `manual_admin.latest_package_pair.package_pair=0.42.44-admin-smoke -> 0.42.45-admin-smoke`를
  확인했다.
- 설치본 console access/account/noVNC product smoke는
  `artifacts/installed-console-access-smoke-20260526-04245/summary.json`,
  `artifacts/installed-account-login-smoke-20260526-04245/summary.json`,
  `artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245/summary.json`가 PASS로
  기록한다.
- Web Console Console/noVNC UX QA는
  `docs/ga-ready/evidence/web-console-console-novnc-ux-qa-2026-05-26-04245.md`가 기록한다.
  `npm run browser:fixture`, installed browser smoke, target-backed noVNC streaming이 모두 PASS다.
- Phase 1 direct-control 잔여 범위 review는
  `docs/ga-ready/evidence/phase1-account-novnc-direct-control-residual-review-2026-05-26.md`가
  기록한다. Phase 1은 read-only Console Access Card와 open handoff만 닫고 Guest Exec,
  QoS mutation, Web/TUI direct mutation은 다음 phase로 유지한다.
- Post-04245 확장 Phase 2-5 planning evidence는
  `docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26.md`가 기록한다.
  ADR-0008 Hyper-V QoS Mutation Policy, ADR-0009 Guest Execution Security Boundary 후보,
  ADR-0010 account/noVNC target config security policy 후보를 만들고 Phase 3 Web/TUI Direct
  Control을 `backend-policy-first`로 고정했다. 이 evidence는 docs/spec/plan only이며
  host mutation, package build, public release를 주장하지 않는다.
- Phase 2 Hyper-V QoS mutation code-level evidence는
  `docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md`가 기록한다.
  Source payload는 contract DTO, Local API preview/apply route, queued job dispatch,
  Hyper-V native adapter WMI code path, PCVCLI `blkio-set`/`bandwidth-set` UX, Runtime
  Policy operation set을 포함한다. 이 evidence는 0.42.47 설치본 승격의 predecessor다.
- Phase 2 Hyper-V QoS mutation 설치본 predecessor evidence는
  `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`가 기록한다.
  `0.42.47-admin-smoke` package build, `full-admin-host-mutation-gate-20260526-04247`,
  실제 VM 대상 `pcvcli vm blkio-set`/`pcvcli vm bandwidth-set` dry-run/apply/rollback
  smoke가 PASS했고 이후 `0.42.45-admin-smoke -> 0.42.47-admin-smoke` manual-admin
  package-pair closure가 닫히면서 0.42.47 current anchor로 승격됐다.
- 당시 public-boundary main push CI는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass.md`,
  run `26413569064`, job `77753058728`, head
  `4f1f0bd8f7ffe9488dbb7175f65013870cf8d58f`에서 PASS했다. PR #169 public-boundary는
  predecessor로 보존한다.

## 2026-05-25 0.42.44 closure historical predecessor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-25-04244-fullgate-manual-admin-closure`이다.
- `0.42.44-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04244.md`가 기록한다. MSI
  SHA-256은 `eb9b6232a7c61431e2289850eecaba1c9a1d92bc93b88ce8eb4bd6f2ed3e8fe2`, payload
  aggregate SHA-256은 `debe36f469dd4f9782f056854142ff7392a62298962d1d4b9835cd14c3758f38`,
  provenance commit은 `9e96ffd423addfb0de139b1dfde0f8fc555c7566`이다.
- 설치본 CLI read-only surface smoke는 package smoke predecessor로
  `docs/ga-ready/evidence/installed-cli-readonly-surface-smoke-2026-05-25-04244.md` /
  `artifacts/installed-cli-readonly-surface-smoke-20260525-04244/summary.json`가 PASS로 기록한다.
  `runtime policy`, `ops summary`, `network inventory`, `network list`가 direct command와
  interactive REPL 양쪽에서 실제 table data를 출력했고 `ok=True | operation=...` 요약 fallback은
  출력하지 않았다.
- 최신 operational full admin host mutation anchor는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-25-04244-hostmutation.md` /
  `full-admin-host-mutation-gate-20260525-04244-r2`가 소유한다. Operational MSI SHA-256은
  `bd1f45b62c683571fe238d8b570642d4f5818bd0b3f3c2e8d9a587841028e701`, payload aggregate
  SHA-256은 `3bbac62cea3c1e6651367ca8f66bcc49633d398743445325abadc63a35192847`, provenance
  commit은 `c7c7b0c9d4ea0b0296bc3ba423beb8eb7ac865e2`이다.
- 최신 closed manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-25-04243-04244.md` /
  `manual-admin-campaign-descriptor-20260525-04243-04244-closed`다. Package pair는
  `0.42.43-admin-smoke -> 0.42.44-admin-smoke`, update ZIP SHA-256은
  `0af708044505c4d0661b30154914a908ebb77cf721eaaf14671cdc5c9b13c864`,
  descriptor `missing_count=0`, `not_pass_count=0`이다. Dedicated clean-host with Windows
  Update, Burn, MSIX, installed update/rollback lifecycle이 모두 PASS다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-25-04244-r2.md`,
  `artifacts/installed-operator-surface-current-card-20260525-04244-r2/summary.json`가 기록한다.
  `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvtui --smoke-once runtime`, Web `/`, `/pcv-config.js`가 PASS했고 ops summary는
  `batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260525-04244-r2`와
  `manual_admin.latest_package_pair.package_pair=0.42.43-admin-smoke -> 0.42.44-admin-smoke`를
  확인했다.
- 초기 `full-admin-host-mutation-gate-20260525-04244` attempt는 MSI repair custom action
  idempotence 결함으로 summary 없이 supersede됐다. Fix commit
  `c7c7b0c9d4ea0b0296bc3ba423beb8eb7ac865e2` 이후 r2 gate가 PASS했다.

## 2026-05-25 0.42.43 package smoke predecessor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-25-04243-pcvcli-usage-error-trim-package-smoke`이다.
- `0.42.43-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04243.md`가 기록한다. MSI
  SHA-256은 `38be93dd0d944e3657ea6fea2f3e0f922ab4577c09d57183b5be299de90297b1`, payload
  aggregate SHA-256은 `95ba31a501bbf7e3cbb2ba103feb9638e0d01ebdfea922237ddbb15cea0c25f7`,
  provenance commit은 `93131de2bfab5fccfc2761538ead0460d3e7d85d`이다.
- 설치본 CLI usage-trim smoke는
  `docs/ga-ready/evidence/installed-cli-usage-trim-smoke-2026-05-25-04243.md` /
  `artifacts/installed-cli-usage-trim-smoke-20260525-04243/summary.json`가 PASS로 기록한다.
  `pcvcli vm get`과 interactive `vm get` 모두 `PCV_CLI_USAGE|Use: vm get <vm>.` 한 줄만
  출력했고, 전체 `Usage:` / `pcvcli [--api URL]` block은 출력하지 않았다.
- `0.42.43-admin-smoke`는 package/update/CLI usage rendering smoke만 닫았다. 최신
  operational full admin host mutation anchor는 여전히 `0.42.41-admin-smoke` /
  `full-admin-host-mutation-gate-20260522-04241`이며, 최신 closed manual-admin package-pair도
  `0.42.40-admin-smoke -> 0.42.41-admin-smoke`를 유지한다. `0.42.42 -> 0.42.43`
  manual-admin package-pair와 full admin host mutation 승격은 후속 gate 대상이다.

## 2026-05-25 0.42.42 package smoke predecessor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-25-04242-pcvcli-snapshot-surface-package-smoke`이다.
- `0.42.42-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04242.md`가 기록한다. MSI
  SHA-256은 `d92e4c8bc8ee47da4a4c3b64d381725b3a1971b41ee41c9c24ba0a5f65a73582`, payload
  aggregate SHA-256은 `ad5ca2730ea932f08d72541b33b04cfb611ed6ca055f459b8988b48b74737c88`,
  provenance commit은 `37632159aaf0c9445c9b712f11f1dfee1a6f9c4f`이다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-25-04242.md` /
  `artifacts/installed-operator-surface-current-card-20260525-04242/summary.json`가
  PASS로 기록한다. `pcvcli --help`, `pcvcli` interactive `help`, `pcvcli host status`,
  `pcvcli --json vm list`, `pcvcli --json ops summary`, `pcvtui --smoke-once runtime`,
  Web `/`, `/pcv-config.js`, unauth runtime policy boundary가 설치본에서 PASS했다.
- 이번 product payload 변경은 PCVCLI 최상위 `snapshot list|create|rollback|delete`
  제거다. `pcvcli vm snapshot list|create|rollback|delete`와 `pcvcli vm checkpoint ...`는
  유지하며, 설치본 `pcvcli snapshot list demo`는 exit `2`와 `Unknown command group 'snapshot'`로
  거부된다.
- `0.42.42-admin-smoke`는 package/update/current-card smoke만 닫았다. 최신 operational full
  admin host mutation anchor는 여전히 `0.42.41-admin-smoke` /
  `full-admin-host-mutation-gate-20260522-04241`이며, 최신 closed manual-admin package-pair도
  `0.42.40-admin-smoke -> 0.42.41-admin-smoke`를 유지한다. `0.42.41 -> 0.42.42`
  manual-admin package-pair와 full admin host mutation 승격은 후속 gate 대상이다.

## 2026-05-22 0.42.41 closure 현재 operational anchor

- Current evidence ledger는 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`이고 ledger id는
  `current-evidence-ledger-2026-05-22-04241-pr169-public-boundary-followup`이다.
- `0.42.41-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-22-04241.md`가 기록한다. Clean MSI
  SHA-256은 `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`, payload
  aggregate SHA-256은 `21aeb02757495d8296151ce20dda987ef36fcb2f3320f5163131ffc90e65c361`,
  provenance commit은 `2f41da1073df6e65113ae8ddaeb183e9b55874f4`이다.
- 최신 operational full admin host mutation은
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-22-04241-hostmutation.md` /
  `full-admin-host-mutation-gate-20260522-04241`가 소유한다. Operational MSI SHA-256은
  `e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb`, payload aggregate
  SHA-256은 `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`다.
- 최신 closed manual-admin package-pair closure는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-22-04240-04241.md` /
  `manual-admin-campaign-descriptor-20260522-04240-04241-closed`다. Package pair는
  `0.42.40-admin-smoke -> 0.42.41-admin-smoke`, update ZIP SHA-256은
  `9ab7e266c093b98982aa854c19f901a6bb133f51c66904b9bfcdf56d538fee73`, descriptor
  `missing_count=0`, `not_pass_count=0`이다. Dedicated clean-host with Windows Update,
  Burn, MSIX, installed update/rollback lifecycle이 모두 PASS다.
- 설치본 Web/TUI/CLI current-card smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-22-04241.md`,
  `artifacts/installed-operator-surface-current-card-20260522-04241/summary.json`가
  기록한다. `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvtui --smoke-once runtime`, Web `/`, `/pcv-config.js`가 PASS했고 ops summary는
  `batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260522-04241`와
  `manual_admin.latest_package_pair.package_pair=0.42.40-admin-smoke -> 0.42.41-admin-smoke`를
  확인했다.
- ADR-0007 PCVCLI Hyper-V QoS/guest-service parity route는 `vm.limit` mutation,
  `vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status`, `vm.guest-ping` readback을
  `0.42.39-admin-smoke` full admin host mutation에서 PASS로 닫았다. Linux cgroup QoS와
  qemu guest agent 호환 claim은 하지 않는다. Historical predecessor evidence는
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04239-hostmutation.md` /
  `full-admin-host-mutation-gate-20260520-04239`와
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`에 보존한다.
- 설치본 PCVCLI QoS/guest targeted smoke는
  `docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`,
  `artifacts/installed-cli-qos-guest-smoke-20260521-04239/summary.json`가 기록한다. 실제
  VM `pcv-cli-qos-guest-d34eea84`를 생성해 `vm limit`, `blkio-get`, `bandwidth`,
  `guest-agent-status`, `guest-ping`, `poweroff`, `delete`를 설치본 CLI로 실행했고 cleanup
  결과 VM/VHD root가 제거됐다. 이 targeted smoke는 0.42.39 설치본 CLI command path
  anchor다.
- Web/TUI QoS/guest readback surface는
  `docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`가 기록한다.
  Web 선택 VM detail `QoS / Guest Readback` panel과 TUI 선택 VM `G` readback은
  `blkio`, `bandwidth`, `guest-agent/status`, `guest-agent/ping` read-only route만
  조회한다. Direct QoS mutation/control은 열지 않는다. 이 변경은 Operator Surface
  product payload 변경이므로 `0.42.40-admin-smoke` package chain은
  `closed-manual-admin-package-pair-04239-04240`로 닫혔다.
- 실제 VM 기반 설치본 TUI row projection smoke는
  `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`,
  `artifacts/web-tui-qos-guest-readback-actual-vm-20260522-04241/summary.json`가 기록한다.
  실제 VM `pcv-ux-qos-04241` 생성/start 후 설치본 `pcvtui --smoke-once vm`이 VM table에
  `pcv-ux-qos-04241`, `running`, `1 vCPU`, `1024 MB` row를 표시했고 cleanup으로 VM과
  Temp VM root를 제거했다. 04240 Web no-overlap/readback PASS 및 설치본 TUI blocker 기록은
  `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`에
  predecessor로 보존한다.
- Historical fast-follow `0.42.37-admin-smoke` evidence는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04237.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md`,
  `artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`에 보존한다.
- PR #169 post-merge public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md`,
  run `26288103559`, job `77380766318`, head
  `11b123311d718cf77e87ccc7b8dea7c5728dc463`에서 PASS했다. 후속 판단
  `docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`는
  product payload 변경이 없으므로 `0.42.42-admin-smoke` package chain과 installed
  account/noVNC smoke를 열지 않는다고 기록한다. PR #168 public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr168-postmerge-pass.md`로,
  PR #167 public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`로,
  PR #164 public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md`로,
  PR #163 public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr163-postmerge-pass.md`로,
  PR #162 public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr162-postmerge-pass.md`로,
  PR #160 public-boundary는
  historical predecessor로 보존한다. 이 전체 evidence set은
  internal admin-smoke 범위이며 public trusted signing 또는 external stable publication
  evidence가 아니다.

## 2026-05-20 0.42.35 closure + 0.42.37 installed lifecycle historical predecessor

- `0.42.35-admin-smoke` clean package build와 full admin host mutation은
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04235.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04235-hostmutation.md`,
  `full-admin-host-mutation-gate-20260520-04235`에 보존한다.
- `0.42.34-admin-smoke -> 0.42.35-admin-smoke` manual-admin package-pair closure는
  immediate predecessor로 유지하며
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md`가 소유한다.
  Target MSI SHA-256은 `12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697`,
  update ZIP SHA-256은 `71ccbe6188de9a52465beae9afc165f7777631bacbbc14a3137d0f9a6379994d`다.
- `0.42.37-admin-smoke`는 0.42.35 설치본 VM lifecycle smoke 중 발견한 Hyper-V pause
  결함을 닫은 fast-follow package다. 실제 VM `create/start/memory-stats/cpu-stats/pause/resume/rename/cleanup`과
  `pcvcli --interactive` neon palette, `pcvcli --json vm list`, `pcvtui --smoke-once`
  runtime smoke가 PASS했다. MSI SHA-256은
  `05dc31965af68792d21d919e19cb07997207d0514fd0ee39169d92129e95f67e`,
  payload aggregate SHA-256은
  `1e2487bfe474daad624a3ef67837a278ab5d25a71c654f8b7c18c95e3cc94e9e`다.

## 2026-05-19 0.42.34 closure 현재

- `0.42.34-admin-smoke` product payload package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`가 기록한다.
  MSI SHA-256은 `cfd46fb46c1eb886d91112b22a0a21790ad1c4d9d856d5817798edac5167c6f5`,
  payload aggregate SHA-256은 `ca1394ac3e219548da275a1e792a21296d82af4038c554363dfb70789b57eed0`,
  `pcvcli.exe` SHA-256은 `84d38979cb2b4cfab4060022a11d86e5db0f7b4ed7f87c2d90ad6ab377cec9f3`이다.
- Operational full gate package는
  `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`가 소유한다.
  MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`,
  payload aggregate SHA-256은 `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`,
  provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다.
- 최신 full admin host mutation은
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md` /
  `full-admin-host-mutation-gate-20260519-04234`, 최신 manual-admin package-pair는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md` /
  `manual-admin-campaign-descriptor-20260519-04232-04234-closed`다. Update ZIP
  SHA-256은 `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`다.
- 설치본 Web/TUI/CLI current-card product payload smoke는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`가
  기록한다. `pcvcli --interactive`는 Linux `pcvctl` style 256-color palette
  `38;5;33`/`38;5;198`/`38;5;51`/`38;5;46`, prompt `❯`, 한 줄 command row를
  출력했고, `pcvcli --interactive --no-color`는 ANSI 없이 `(pcv) >`와
  `vm create | Create a new VM` 형식을 출력했다.
- `pcvcli vm list`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
  `pcvtui --smoke-once --no-color runtime`, Web `/`, `/pcv-config.js`, unauth runtime
  policy boundary가 설치본 product root에서 PASS했다. 현재 host에는 VM이 없어
  table smoke는 `No VMs found.`, JSON smoke는 `data=[]`, `ok=true`로 기록했다.
- `0.42.33-admin-smoke`는 redirected interactive smoke에서 prompt glyph `❯`가
  `?`로 기록되는 UTF-8 출력 문제가 확인되어 superseded intermediate package로만
  보존한다. `0.42.34-admin-smoke`가 해당 문제를 닫은 최신 payload smoke다.
- PCVCLI Linux command coverage의 backend 미노출 후보 분리는
  `docs/ga-ready/evidence/pcvcli-backend-command-gap-slice-2026-05-19.md`가
  기록한다. 후속 backend slice에서 `vm memory-stats/cpu-stats`는 code-level
  read-only API/CLI/help contract로, `vm rename/pause/resume`은 code-level queued
  mutation API/job/Hyper-V adapter/CLI/help contract로 승격했고, 후속
  `0.42.38-admin-smoke` slice에서 `vm eject/delete-status`와
  `vm set-memory/set-vcpu/disk-resize`도 API/job/Hyper-V adapter/Web/TUI/CLI route
  contract로 승격했다. 설치본 evidence는
  `docs/ga-ready/evidence/pcvcli-vm-stats-lifecycle-backend-slice-2026-05-19.md`와
  `0.42.38-admin-smoke` package/full gate/current-card evidence에서 닫고,
  manual-admin package-pair candidate는 clean-host blocker가 있는 partial evidence로
  보존한다. Public trusted signing 또는 external stable publication evidence가
  아니다.
- 남은 Linux `pcvctl` parity 항목은
  `docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md`에서
  `pcvcli-linux-parity-remaining-scope-lock-and-operator-ux`로 선정했다. 즉시 command를
  추가하지 않고, Hyper-V QoS policy 또는 guest service parity로 재해석할지 ADR/product
  semantics를 먼저 닫는다.
- PR #158 post-merge public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-19-pr158-postmerge-pass.md`,
  run `26094982269`, job `76730240480`, head
  `63df3e42a4e42b4e21646e356968399c1458d89b`에서 PASS했다. PR #159
  post-merge public-boundary는
  `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-19-pr159-postmerge-pass.md`,
  run `26095422721`, job `76731759975`, head
  `a271fb8d5fe9e7c45d30da05f5acd225d08f61d9`에서 PASS했다. 둘 다 public trusted
  signing 또는 external stable publication evidence가 아니다.
- Dedicated clean-host with Windows Update `KB5087545` UBR `5139`, Burn, MSIX,
  installed runtime ops summary, descriptor generation v2, installed Web/TUI/CLI
  current-card가 PASS했고 descriptor `missing_count=0`, `not_pass_count=0`이다.
  0.42.32 closure는 historical predecessor로 보존한다. Public trusted signing과
  외부 stable publication evidence가 아니다.

## 2026-05-19 0.42.32 closure historical predecessor

- Current evidence ledger: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, ledger id
  `current-evidence-ledger-2026-05-19-04232-closure`, installed version anchor
  `0.42.32-admin-smoke`.
- 최신 full admin host mutation은
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04232-hostmutation.md` /
  `full-admin-host-mutation-gate-20260519-04232`, 최신 installed Operator Surface
  current-card는
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04232.md`,
  최신 manual-admin package-pair는
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md` /
  `manual-admin-campaign-descriptor-20260519-04231-04232-closed`다.
- Target operational MSI SHA-256은
  `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`, update ZIP
  SHA-256은 `c2e5c577d1a9bbec1ce6ca7ca2f79588d17b908d4aa639adb7968e5a09ce38da`,
  payload aggregate SHA-256은
  `21e2f8136ac53384bf86966e51f9040f7bbb37e62bc9e761640c0d1aeff35956`, provenance
  commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다.
- Dedicated clean-host with Windows Update `KB5087545` UBR `5139`, Burn, MSIX,
  installed runtime ops summary, descriptor generation v2, installed Web/TUI/CLI
  current-card가 PASS했고 descriptor `missing_count=0`, `not_pass_count=0`이다.
  0.42.31 closure는 historical predecessor로 보존한다. Public trusted signing과
  외부 stable publication evidence가 아니다.

## 2026-05-19 0.42.32 PCVCLI neon VM list smoke

- `0.42.32-admin-smoke` product payload package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04232.md`가 기록한다.
  MSI SHA-256은 `8d8c585fe73c605bd938705ef63790768348791cb479bf42c4bbbf8b31af14dc`,
  payload aggregate SHA-256은 `b17130829d9851410a9d4c31a7b44a3e85d31ed78d15bb2d6ba024423240ddc6`,
  `pcvcli.exe` SHA-256은 `a227de915d298e45bdc92d6f8a5341f54f7ee0785c2621dcfc8af0551afa6239`이다.
- 설치본 전역 `pcvcli` 실제 VM row smoke는
  `docs/ga-ready/evidence/installed-pcvcli-neon-vm-list-smoke-2026-05-19-04232.md`가
  기록한다. `pcvcli --json host status`, `pcvcli --json vm list`, 실제 Hyper-V VM
  create/start/list/get/poweroff/delete가 PASS했고, `pcvcli vm list` neon ANSI table과
  `pcvcli --no-color vm list` 한 줄 단위 table이 실제 VM `pcv-neon-list-04232`
  row를 출력했다. 테스트 VM과 VHD 폴더는 evidence 캡처 후 정리했으며 최종 VM count는 `0`이다.
- 이 항목은 product payload smoke이며, 위 0.42.32 closure에서 operational full-gate
  package와 manual-admin package-pair로 승격됐다. Public trusted signing과 외부 stable
  publication evidence가 아니다.

## 2026-05-18 0.42.31 closure historical predecessor

- Current evidence ledger: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, ledger id `current-evidence-ledger-2026-05-18-04231-closure`, installed version anchor `0.42.31-admin-smoke`. 최신 full admin host mutation은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04231-hostmutation.md` / `full-admin-host-mutation-gate-20260518-04231`, 최신 installed Operator Surface current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04231.md`, 최신 manual-admin package-pair는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04230-04231.md` / `manual-admin-campaign-descriptor-20260518-04230-04231-closed`다. Target operational MSI SHA-256은 `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`, update ZIP SHA-256은 `de258c8f58ff8fd25ea78ea74483746c89190b3a7aa84345f3789eaa02458a44`, payload aggregate SHA-256은 `cea7d1f798e6f0889cf0cd02da049dc7d7b0131e8df51a768c12e02ea76c22f4`, provenance commit은 `fc8cc284b7824172b8bf035858fb86b21bd26e5d`이다. Dedicated clean-host with Windows Update `KB5087545` UBR `5139`, Burn, MSIX, installed runtime ops summary, descriptor generation v2, installed Web/TUI/CLI current-card가 PASS했고 descriptor `missing_count=0`, `not_pass_count=0`이다. 초기 PCVCLI interactive shell package MSI SHA-256 `173c1e1487e1b032c11ca528d83c5bb4ede77b7fec747a082cd79f2b7b6317ee`는 operational full-gate package 이전 smoke artifact로 보존한다. PR #156/#155/#154 public-boundary evidence와 0.42.30 이하 package-pair는 historical predecessor로 보존한다. Installed account/noVNC smoke는 `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04229.md`에서 historical PASS이며 다음 account/noVNC payload 변경 때 재검증한다. Public trusted signing과 외부 stable publication evidence가 아니다.


이 문서는 Desktop Node evidence의 최신 진입점이다. 개별 evidence 파일은
`docs/ga-ready/evidence/`에 보존하며, README/AGENTS/운영 문서는 이 파일을 통해
current와 historical 기준을 구분한다.

## 2026-05-18 0.42.31 PCVCLI package/install smoke

- PCVCLI Linux CLI parity와 interactive shell product payload를 포함한
  `0.42.31-admin-smoke` package build는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04231.md`가 기록한다.
  MSI SHA-256은 `173c1e1487e1b032c11ca528d83c5bb4ede77b7fec747a082cd79f2b7b6317ee`,
  provenance commit은 `068c4d93cf7ab203983427e8999c64d1fcbfb873`이다.
- 설치본 PCVCLI/TUI smoke는
  `docs/ga-ready/evidence/installed-pcvcli-interactive-shell-smoke-2026-05-18-04231.md`가
  기록한다. `pcvcli`, `pcvcli --interactive`, `pcvcli -i`, 자동 token 기반
  `host status`, `vm list`, REPL 내부 `host status`/`vm list`, `pcvtui --smoke-once`
  모두 exit `0`이다.
- Core/Backend product contract 완료 판정은
  `docs/ga-ready/evidence/core-backend-completion-review-2026-05-18-04231.md`가
  기록한다. `dotnet test src\DesktopNode.sln --no-restore`는 544 passed다.
- 이후 full admin host mutation gate와 `0.42.30-admin-smoke -> 0.42.31-admin-smoke`
  manual-admin campaign을 실행해 위 0.42.31 closure로 승격했다. 이 초기 package
  smoke는 operational full-gate package의 predecessor artifact로 보존한다.

## 2026-05-18 0.42.31 historical snapshot

- Current evidence ledger: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
  - ledger id: `current-evidence-ledger-2026-05-18-04231-closure`
  - current full admin/package-pair/public-boundary anchor를 한 곳에서 관리하고,
    2026-05-04 snapshot ledger는 historical 원장으로 유지한다.
  - latest package closure: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04230-04231.md`
  - previous public-boundary follow-up: `docs/ga-ready/evidence/post-04231-pr155-public-boundary-followup-2026-05-18.md`
  - previous 04226 ledger follow-up: `docs/ga-ready/evidence/post-04226-ledger-contract-followup-2026-05-17.md`, ledger id `current-evidence-ledger-2026-05-17-04226`, descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`, next trigger `post-04226-ledger-contract-merge`
  - Runtime/API current-card descriptor id direct expose: `current_card_descriptor_batch_id`
  - descriptor schema: `descriptor_schema_version=2`, contract `manual-admin-descriptor-generation-contract-v2`
  - historical next package-pair trigger: 새 product payload 이후 `0.42.31-admin-smoke -> 0.42.32-admin-smoke` 후보였고, 2026-05-19 closure에서 닫혔다.
- 0.42.31 historical 전체 관리자 host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-18-04231-hostmutation.md`
  - version: `0.42.31-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04231`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`, `artifacts/os-mutation-gates-batch-profile-20260518-04231`
  - full-gate MSI SHA-256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`
  - initial package-smoke MSI SHA-256: `173c1e1487e1b032c11ca528d83c5bb4ede77b7fec747a082cd79f2b7b6317ee`
  - full-gate provenance commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
  - signing mode: `AllowUnsignedDev`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260518-04231`, artifact `artifacts/installed-operator-surface-current-card-20260518-04231`
  - current evidence contract: `runtime-api-current-evidence-rollup-v1`
  - runtime bridge: `runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`
  - route detail count: `4`
    - Host Ops lifecycle descriptor bridge: `host-ops-lifecycle-descriptor-bridge-v1`
    - Host Ops lifecycle bucket contract: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
    - Host Ops Web diagnostics table: `host-ops-web-diagnostics-bucket-table-v1`
  - boundary: ADR-0006 `internal-private-network-only`; public trusted signing과 외부 stable publication은 `out-of-scope` / `not-claimed`다.
- 0.42.31 historical 닫힌 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04230-04231.md`
  - result: `PASS`
  - package pair: baseline `0.42.30-admin-smoke`, target `0.42.31-admin-smoke`
  - baseline package root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
  - target package root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`
  - baseline MSI SHA-256: `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`
  - target MSI SHA-256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`, provenance commit `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
  - update ZIP SHA-256: `de258c8f58ff8fd25ea78ea74483746c89190b3a7aa84345f3789eaa02458a44`
  - PASS: readiness, installed update/rollback, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation v2
  - descriptor: `artifacts/manual-admin-campaign-20260518-04230-04231/manual-admin-campaign-descriptor/summary.json`
  - descriptor status: `pass`, `missing_count=0`, `not_pass_count=0`
  - clean-host: Windows Update `KB5087545`, UBR `5139`, NoContact idle recovery 1회, blocker `none`
  - Burn/MSIX: Burn bundle SHA-256 `1d9240cd95c31a2ff5e7c87f50ed9dd0980465f0e8a8bc0638c681a84ce8bf4f`, MSIX v1/v2 SHA-256 `ff6bee8c19d23156d32140d3e51275e87cb93cfd786da8fa03e6be6545618f28` / `aec084a3337b8e8991947f2b4d4a3934e678341d8f7fca2c0e2c1d0c4792e4d1`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 최신 product payload package build record: `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04231.md`
  - version: `0.42.31-admin-smoke`
  - artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`
  - MSI SHA-256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`
  - provenance commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
  - note: 초기 PCVCLI interactive shell package `artifacts/admin-smoke-package-20260518-04231`, MSI SHA-256 `173c1e1487e1b032c11ca528d83c5bb4ede77b7fec747a082cd79f2b7b6317ee`는 operational full-gate package 이전 smoke artifact로만 보존한다.
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 최신 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md`
  - run: `26518952796`, job `78104102372`, head `12bc72e856ea9ac7c6d54c4094873b2d8db9f672`
  - previous 0.42.53 credentialed Windows guest execution smoke public-boundary: 같은 evidence, run `26516950720`
  - previous 0.42.53 ISO evidence roll-forward public-boundary: 같은 evidence, run `26512890221`
  - previous 0.42.53 evidence closure public-boundary: 같은 evidence, run `26511891436`
  - previous 0.42.53 evidence closure roll-forward public-boundary: 같은 evidence, run `26510159990`
  - previous 0.42.53 evidence gates roll-forward public-boundary: 같은 evidence, run `26496046109`
  - earlier 0.42.53 evidence gates roll-forward public-boundary: 같은 evidence, run `26495580805`
  - initial 0.42.53 evidence closure public-boundary: 같은 evidence, run `26494683032`
  - previous 0.42.53 provider public-boundary: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-provider-postpush-pass.md`, run `26494136304`
  - previous 0.42.50 public-boundary: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass.md`, run `26489610881`
  - post-merge follow-up: `docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`
  - guard: `public-boundary-ci-required`, checkout `actions/checkout@v6.0.2`, result `PASS`
  - 이전 PR #169 public-boundary main push CI:
    `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md`,
    run `26288103559`, job `77380766318`, head
    `11b123311d718cf77e87ccc7b8dea7c5728dc463`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
  - PR #167, PR #164, PR #163, PR #162, PR #160, PR #156 후속 package decision은 historical predecessor로 보존한다.
  - 이전 PR #167 public-boundary main push CI:
    `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`,
    run `26228675428`, job `77182631331`, head
    `f173f9857089de61ca1fb2b7a2da7839a3dd73a8`
  - 이전 PR #164 public-boundary main push CI:
    `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md`,
    run `26170972989`, job `76988240617`, head
    `03402f1607b735f2d92291ae6109d7986d9a57b8`
  - Historical PR #156 public-boundary main push CI:
    `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`,
    run `26017721669`, job `76471545641`, head
    `a4509c552c003ee0fc87b54b26529686e6dfeb84`
  - 이전 PR #155 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`
    - run: `26013384587`, job `76458402221`, head `2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f`
    - historical predecessor로 보존한다.
    - PR #155 후속 package decision: `deferred-no-product-payload-change-after-pr155`
  - 이전 PR #154 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md`
    - run: `25989986761`, job `76394250912`, head `d7f611dfc14a9fa1507f936559209513272b585a`
    - historical predecessor로 보존한다.
    - PR #154 후속 package decision: `deferred-no-product-payload-change-after-pr154`
  - 이전 PR #153 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`
    - run: `25987705546`, job `76388078056`, head `d306712ad671c8a00d5c560765b8952e24a07502`
    - historical predecessor로 보존한다.
  - 이전 `0.42.28-admin-smoke -> 0.42.29-admin-smoke` Manual admin package-pair PASS:
    - evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`, descriptor `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
    - target MSI SHA-256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
    - update ZIP SHA-256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
    - provenance commit: `d306712ad671c8a00d5c560765b8952e24a07502`
  - 이전 PR #152 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md`
    - run: `25985786230`, job `76382711230`, head `ca07514097f4e9524a7f3630d321c9666593c962`
    - historical predecessor로 보존한다.
- 이전 PR #151 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`
    - run: `25984814303`, job `76380096421`, head `26ae50fa7bef11b4919b441e706bde505463aded`
    - `0.42.28-admin-smoke` Operator Surface package chain predecessor로 보존한다.
    - 04228 full admin host mutation batch: `full-admin-host-mutation-gate-20260517-04228`
  - 이전 `0.42.27-admin-smoke -> 0.42.28-admin-smoke` Manual admin package-pair PASS:
    - evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md`, descriptor `manual-admin-campaign-descriptor-20260517-04227-04228-closed`
    - target MSI SHA-256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
    - clean package MSI SHA-256: `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`
    - update ZIP SHA-256: `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`
    - provenance commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
  - 이전 `0.42.26-admin-smoke -> 0.42.27-admin-smoke` Manual admin package-pair PASS:
    - evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`, descriptor `manual-admin-campaign-descriptor-20260517-04226-04227-closed`
    - target MSI SHA-256: `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`
    - update ZIP SHA-256: `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`
    - provenance commit: `69aba3eb3ff08c843f1a481818ddc86eac2f019b`
  - 이전 Manual admin package-pair initial descriptor: `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`
    - package pair: baseline `0.42.25-admin-smoke`, target `0.42.26-admin-smoke`
    - readiness: `pass`, package pair input `ready-current-baseline-target-package-pair`
    - target package root: `artifacts/admin-smoke-package-20260516-04226`
    - target MSI SHA-256: `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`
    - descriptor status: `blocked-by-missing-evidence`, `missing_count=4`, `not_pass_count=1`
    - closure: 2026-05-17 `manual-admin-campaign-2026-05-17-04225-04226`에서 닫힌 package-pair PASS로 승격됐다.
  - 이전 `0.42.25-admin-smoke -> 0.42.26-admin-smoke` Manual admin package-pair PASS:
    - evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`, descriptor `manual-admin-campaign-descriptor-20260517-04225-04226-closed`
    - target MSI SHA-256: `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`
    - update ZIP SHA-256: `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`
    - provenance commit: `d6500c01c972cbc7ca1e290e51120181ceea1501`
    - descriptor status: `pass`, `missing_count=0`, `not_pass_count=0`
  - 이전 Manual admin package-pair PASS: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`
    - package pair: baseline `0.42.24-admin-smoke`, target `0.42.25-admin-smoke`
    - target/full-gate MSI SHA-256: `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`
    - update ZIP SHA-256: `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`
    - provenance commit: `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`
    - descriptor: `manual-admin-campaign-descriptor-20260516-04224-04225-closed`, `missing_count=0`, `not_pass_count=0`
    - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 Manual admin package-pair blocker: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`
  - package pair: baseline `0.42.21-admin-smoke`, target `0.42.22-admin-smoke`
  - result: `BLOCKED_BY_BURN_CREDENTIAL_MANAGER_IDEMPOTENCE`
  - detail: installed update/rollback은 PASS였지만 Burn install이 `CredentialManagerDefaultTransition` idempotence 미지원으로 exit `1603`을 반환했다.
  - closure: `0.42.23-admin-smoke` package와 `0.42.22 -> 0.42.23` package-pair campaign에서 해소했다.
  - closure evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`, descriptor `manual-admin-campaign-descriptor-20260516-04222-04223-closed`, target MSI SHA-256 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`, update ZIP SHA-256 `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`, provenance commit `676b4177b10dc80209969066857bab6008ff2473`
- 0.42.31 historical installed operator surface current-card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04231.md`
  - artifact: `artifacts/installed-operator-surface-current-card-20260518-04231`
  - version: `0.42.31-admin-smoke`, latest batch `full-admin-host-mutation-gate-20260518-04231`
  - Web `200`, `/pcv-config.js` `200`, unauthenticated runtime policy `401`, CLI host status/VM list/ops summary PASS, TUI runtime smoke PASS
  - current evidence contract: `runtime-api-current-evidence-rollup-v1`
  - runtime bridge direct expose: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`
  - manual-admin package-pair: `0.42.30-admin-smoke -> 0.42.31-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260518-04230-04231-closed`
  - machine `PATH`: `pcvcli.exe`, `pcvtui.exe` resolve PASS
  - host mutation performed: `true`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 installed operator surface current-card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`
  - artifact: `artifacts/installed-operator-surface-current-card-20260516-04224`
  - version: `0.42.24-admin-smoke`, latest batch `full-admin-host-mutation-gate-20260516-04224`
  - package build MSI SHA-256: `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, full-gate MSI SHA-256 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`, provenance commit `b974d6b541423f2e4160f726f96155b16f105e9d`
  - historical predecessor로 보존한다.
- Post-04223 full host mutation current-card: `docs/ga-ready/evidence/post-04223-full-host-mutation-current-card-2026-05-16.md`
  - result: `FULL_HOST_MUTATION_CURRENT_CARD_PASS_NEXT_SLICE_SELECTED`
  - next product payload candidate: `0.42.24-admin-smoke`
  - next package-pair candidate: `0.42.23-admin-smoke -> 0.42.24-admin-smoke`
  - stale local codex branch cleanup: merged remote-gone local branches `12`개 삭제, linked worktree/unmerged gone branches 보존
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 전체 관리자 host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`
  - version: `0.42.22-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04222`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04222`, `artifacts/os-mutation-gates-batch-profile-20260516-04222`
  - full-gate MSI SHA-256: `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`
  - clean package MSI SHA-256: `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`
  - provenance commit: `8a38995cc25a888f64473e9a2869740949ad6b24`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04222`
  - public-boundary post-merge evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass.md`, run `25952150476`, job `76291983316`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 전체 관리자 host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`
  - version: `0.42.21-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221`, `artifacts/os-mutation-gates-batch-profile-20260516-04221`
  - full-gate MSI SHA-256: `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`
  - clean package MSI SHA-256: `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b`
  - provenance commit: `3b8c48deb4c31675f6fce46c320703f23c27c131`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04221`, artifact `artifacts/installed-current-card-20260516-04221-fullgate`
  - public-boundary successor: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md`, run `25938745434`, job `76250726268`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 전체 관리자 host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`
  - version: `0.42.20-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04220`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04220`, `artifacts/os-mutation-gates-batch-profile-20260516-04220`
  - full-gate MSI SHA-256: `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`
  - clean package MSI SHA-256: `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`
  - provenance commit: `0895d018935298721b25b5d9ce1ae083a6690c25`
  - signing mode: `AllowUnsignedDev`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260516-04220`, artifact `artifacts/installed-current-card-20260516-04220-fullgate`
  - public-boundary workflow rerun: `docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md`, run `25933428239`, job `76232707240`, guard job PASS
  - previous public-boundary blocker: `artifacts/public-boundary-workflow-rerun-20260516-04220/summary.json`, run `25930077313`, blocker `billing-or-spending-limit`, guard job did not start
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 public-boundary main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md`
  - run: `25933861585`, job `76234195716`, head `686e4201f823295dc65cde302f613a982ab8cade`
  - guard: `public-boundary-ci-required`, result `PASS`
  - maintenance: checkout action target `actions/checkout@v6.0.2`
  - fallback: branch protection/ruleset unavailable on private repo plan, PR/merge guard로 `public-boundary-ci-required` PASS 확인
  - package build: `deferred-no-product-payload-change-after-04220`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 public-boundary checkout v6.0.2 main push CI: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md`
  - run: `25934411998`, job `76236050409`, head `3933231e6e2abf3a398dfcc3fdc999b3df38dac6`
  - guard: `public-boundary-ci-required`, checkout `actions/checkout@v6.0.2`, result `PASS`
  - Node.js 20 deprecation warning: `not-observed`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-ci-maintenance development slices: `docs/ga-ready/evidence/post-ci-maintenance-dev-slices-2026-05-16.md`
  - result: `CODE_LEVEL_PASS`
  - source anchor: `0.42.20-admin-smoke`
  - next product payload candidate: `0.42.21-admin-smoke`
  - machine keys: `runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`, `hyperv_provider_callsite_guard=hyperv-wmi-provider-callsite-drift-guard-v1`, `host_ops_reason_code_contract=host-ops-dryrun-mutation-reason-code-v1`, `manual_admin_descriptor_generation_contract=manual-admin-descriptor-generation-contract-v2`
  - host mutation performed: `false`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04221 successor operator surface: `docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md`
  - result: `CODE_LEVEL_AND_OPERATOR_SURFACE_PASS`
  - public-boundary successor: run `25938745434`, job `76250726268`
  - installed operator surface: `artifacts/installed-operator-surface-current-card-20260516-04221`
  - Web Console diagnostics direct expose: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
  - next product payload candidate: `0.42.22-admin-smoke`
  - host mutation performed: `false`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04220 development slices: `docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md`
  - result: `CODE_LEVEL_PASS`
  - source anchor: `0.42.20-admin-smoke`
  - machine keys: `runtime_diagnostics_ops_summary_contract=runtime-api-diagnostics-ops-summary-contract-v1`, `hyperv_wmi_common_helper_contract=hyperv-wmi-common-helper-contract-v1`, `host_ops_mutation_boundary_contract=service-eventlog-firewall-truststore-credential-manager-data-root`, `packaging_release_next_trigger=product-payload-change-after-04220-fullgate`
  - public-boundary workflow rerun: run `25933428239`, job `76232707240`, guard job PASS; previous blocker run `25931297085`는 GitHub billing/spending-limit로 보존
  - host mutation performed: `false`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04219-04220.md`
  - result: `PASS`
  - package pair: baseline `0.42.19-admin-smoke`, target `0.42.20-admin-smoke`
  - target package root: `artifacts/admin-smoke-package-20260516-04220`
  - target MSI SHA-256: `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`, provenance commit `0895d018935298721b25b5d9ce1ae083a6690c25`
  - update ZIP SHA-256: `8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b`
  - PASS: readiness, installed update/rollback, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation
  - descriptor: `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-campaign-descriptor-supervised/summary.json`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04219 follow-up execution: `docs/ga-ready/evidence/post-04219-followup-execution-2026-05-16.md`
  - result: `CODE_CONTRACT_PASS_DESCRIPTOR_READINESS_EXECUTED_CI_GUARD_WIRED`
  - version: `0.42.19-admin-smoke`
  - descriptor batch: `manual-admin-campaign-descriptor-20260516-04218-04219`
  - readiness summary: `artifacts/manual-admin-04218-04219-readiness-20260516/summary.json`
  - descriptor supervisor summary: `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04218-04219/summary.json`
  - full admin host mutation: `prepared`, manifest `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04219-prepared/manifest.json`
  - machine keys: `runtime_queued_mutation_route_registry=contract-backed`, `hyperv_operation_telemetry_error_contract=operation-level-telemetry-error-contract-v1`, `host_ops_family_helpers=service-eventlog-firewall-truststore-data-root-config-job-service-token-credential-manager`
  - guard: `public-boundary-ci-required`, workflow `.github/workflows/public-boundary.yml`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md`
  - result: `PASS`
  - package pair: baseline `0.42.16-admin-smoke`, target `0.42.18-admin-smoke`
  - target package root: `artifacts/admin-smoke-package-20260515-04218`
  - target MSI SHA-256: `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`, provenance commit `9121d1f5e7fa83d803c484a44698d4fc8e825c19`
  - update ZIP SHA-256: `8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd`
  - PASS: readiness, installed update/rollback, dedicated clean-host with Windows Update NoContact recovery, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation
  - descriptor: `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised/summary.json`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 전체 관리자 host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`
  - version: `0.42.18-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260515-163107-04218`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260515-163107-04218`, `artifacts/os-mutation-gates-batch-profile-20260515-163107-04218`
  - full-gate MSI SHA-256: `0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09`
  - clean package MSI SHA-256: `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`
  - provenance commit: `9121d1f5e7fa83d803c484a44698d4fc8e825c19`
  - signing mode: `AllowUnsignedDev`
  - installed listener current card: `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260515-163107-04218`, artifact `artifacts/installed-current-card-20260515-04218-fullgate`
  - descriptor guard: `descriptor_excluded_from_operational_latest=true`, descriptor batch `manual-admin-campaign-descriptor-20260515-04216-04218`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 product payload package build: `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`
  - version: `0.42.19-admin-smoke`
  - artifact: `artifacts/admin-smoke-package-20260515-04219`
  - MSI SHA-256: `3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55`
  - provenance commit: `2b7bd9ed702a785361ea5bbaa8a969280d400360`
  - scope: post-04218 follow-up code contract payload package build; update ZIP/package-pair/full host mutation은 미실행
  - guard: `public-boundary-ci-required`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Ops summary descriptor selector guard package: `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`
  - version: `0.42.14-admin-smoke`
  - artifact: `artifacts/admin-smoke-package-20260514-04214-selectorfix`
  - MSI SHA-256: `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`
  - latest follow-up artifact: `artifacts/installed-current-card-20260515-04218-fullgate`
  - result: descriptor batch 이후에도 `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260515-163107-04218`, `descriptor_excluded_from_operational_latest=true`, token value UI 노출 없음
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04218 contract alignment: `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`
  - result: `PASS`
  - scope: Runtime/Core API route diagnostics bridge, Hyper-V VM/checkpoint/network dispatch catalog, Host Ops lifecycle bucket, packaging next trigger, Web Console/TUI/CLI operator journey, ADR-0005/0006 public boundary preservation
  - source anchor: `0.42.18-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md`
  - host mutation performed: `false`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04218 runtime/domain development slice: `docs/ga-ready/evidence/post-04218-runtime-domain-slices-2026-05-15.md`
  - result: `CODE_LEVEL_PASS`
  - scope: Runtime/Core route-family evidence bridge, Hyper-V dispatch handler contract, Host Ops lifecycle bucket key, 0.42.19 next-candidate descriptor metadata, Web Console/TUI/CLI current-card journey
  - machine keys: `runtime_api_diagnostics_bridge=route-family-evidence-linked`, `hyperv_dispatch_catalog_contract=vm-checkpoint-network-fixed`, `host_ops_lifecycle_bucket_contract=service-eventlog-firewall-truststore-data-root-separated`, `packaging_release_next_trigger=product-payload-change-after-04218-fullgate`
  - host mutation performed: `false`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04218 follow-up execution: `docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md`
  - result: `PACKAGE_BUILD_PASS_CODE_CONTRACT_PASS`
  - package build: `0.42.19-admin-smoke`, artifact `artifacts/admin-smoke-package-20260515-04219`
  - MSI SHA-256: `3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55`
  - machine keys: `runtime_route_registry_source=ApiHandlerAdapterContract`, `hyperv_dispatch_model=handler-registry-delegate-map`, `host_ops_family_helpers=service-eventlog-firewall-truststore-data-root`, `operator_surface_snapshot_parity=web-console-tui-cli-current-card`
  - update ZIP/package-pair/full admin host mutation: `not-built` / `not-run-package-build-only` / `false`
  - guard: `public-boundary-ci-required`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Hyper-V provider set contract code-level: `docs/ga-ready/evidence/hyperv-provider-set-contract-code-level-2026-05-15.md`
  - result: `CODE_LEVEL_PASS`
  - scope: `DesktopNodeHyperVProviderSet`가 WMI provider composition과 provider boundary map을 소유하고, `DesktopNodeHyperVNativeAdapter.CreateDefault()`가 provider set을 소비한다.
  - boundary: 0.42.16 package/full gate 이후 repository code-level slice이며, 후속 0.42.18 package/package-pair/full gate가 installed evidence로 승격했다.
- 0.42.17 framework-dependent package regression diagnostic:
  `artifacts/manual-admin-campaign-20260515-04216-04217/clean-host-updated-os/summary.json`
  - result: `failed`
  - blocker: `PCV_PRODUCT_UPDATE_START_FAILED`, `sc.exe start PureCVisorDesktopNode` exit `1053`
  - decision: `0.42.17-admin-smoke`는 current evidence가 아니며, self-contained publish를 복구한 `0.42.18-admin-smoke`가 supersede한다.
- Post-04212 `1-2-3-4-5` current-card follow-up:
  `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`
  - result: `pass-dashboard-current-card-smoke-deferred-product-chain`
  - checked main: `8224af81c00482145b6c08dcde8c92a039b2aa26`
  - product payload change detected: `false`
  - decision: `0.42.13-admin-smoke` package build, package-pair, clean-host campaign, full admin host mutation을 `deferred-until-next-product-payload-change` / `not-run-no-product-payload`로 보류
  - current-card artifact: `artifacts/web-console-current-card-20260514-04212-rerun-followup`
  - UI smoke: Dashboard와 Evidence view가 `full-admin-host-mutation-gate-20260514-04212-rerun`, `0.42.12-admin-smoke`를 표시했고 token value는 UI text에 노출되지 않음
  - boundary: host mutation을 실행하지 않았고 public trusted signing과 외부 stable publication evidence가 아니다.
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`
  - result: `PASS`
  - package pair: baseline `0.42.12-admin-smoke`, target `0.42.13-admin-smoke`
  - target MSI SHA-256: `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`, provenance commit `a28bb808386f206c9dbf7dcaeee232eacb648434`
  - update ZIP SHA-256: `638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9f700b83059a8c9`
  - descriptor: `artifacts/manual-admin-campaign-20260514-04212-04213/manual-admin-campaign-descriptor-supervised/summary.json`
  - status: 04214->04215 PASS 이후 historical predecessor로 보존한다.
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`
  - result: `PASS`
  - package pair: baseline `0.42.11-admin-smoke`, target `0.42.12-admin-smoke`
  - target MSI SHA-256: `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`, provenance commit `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`
  - update ZIP SHA-256: `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`
  - PASS: installed update/rollback, wrapper repair, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation
  - descriptor: `artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised/summary.json`
- Clean-host Windows Update NoContact recovery guard: `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`
  - result: `CODE_LEVEL_PASS`
  - runner: `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`
  - scope: Windows Update reboot 이후 heartbeat `NoContact` + CPU idle 상태가 threshold 이상 지속되면 한 번만 `Stop-VM -TurnOff -Force; Start-VM` recovery를 수행하고 `recovery_actions`를 남김
  - boundary: 이 evidence 자체는 host mutation을 실행하지 않았고 public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04212 follow-up execution: `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`
  - result: `pass-triage-deferred-package-and-host-mutation`
  - checked main: `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea`
  - product payload change detected: `false`
  - decision: `0.42.13-admin-smoke` package build, full admin host mutation, `0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign을 `deferred-until-next-product-payload-change`로 보류
  - clean-host recovery summary key: 다음 실제 clean-host run에서 `recovery_actions`와 `automatic_recovery_performed`로 판정
  - boundary: host mutation을 실행하지 않았고 public trusted signing과 외부 stable publication evidence가 아니다.
- Post-04210 follow-up execution: `docs/ga-ready/evidence/post-04210-followup-execution-2026-05-13.md`
  - result: `pass-docs-and-cleanup`
  - checked main: `371e05055c7488f923c0038f87f1a1288054c271`
  - product payload change detected: `false`
  - decision: 당시에는 `deferred-until-next-product-payload-change`; 후속 ops summary data builder payload 변경으로 `0.42.12-admin-smoke` package build, full admin host mutation, package-pair campaign을 실행해 닫음
  - full admin host mutation campaign decision at the time: `not-run-no-new-product-payload`
  - adopted evidence: `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`
- `0.42.10-admin-smoke` duplicate outer start RCA:
  `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`
  - status: `historical-rca-only`; current package-pair나 full gate claim이 아니다.
  - blocker: native service-action repair가 service를 `Running`으로 만든 뒤 outer
    wrapper가 `sc.exe start`를 다시 호출해 `1056 already running`을 반환했다.
  - closure: `0.42.11-admin-smoke`가 `native-service-action-controls-final-state`
    reason으로 outer start를 skip한다.
- 이전 Manual admin package-pair campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`
  - result: `PASS`
  - package pair: baseline `0.42.9-admin-smoke`, target `0.42.11-admin-smoke`
  - target MSI SHA-256: `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`, provenance commit `987beb51025a5aa926df7d9a905019b4d6d29705`
  - update ZIP SHA-256: `734114e0ea7c9d486a1d329cd551a6abc34d20f3801a944bd5dbcb8c1c4a9991`
  - PASS: installed update/rollback, wrapper repair, dedicated clean-host with Windows Update, Burn install/repair/remove, MSIX build/install/update/remove, installed runtime ops summary capture, descriptor generation
  - descriptor: `artifacts/manual-admin-campaign-20260513-0429-04211/manual-admin-campaign-descriptor-supervised/summary.json`
  - status: 04211→04212 PASS 이후 historical predecessor로 보존한다.
- 이전 전체 관리자 host mutation: `0.42.12-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation.md`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit`, `artifacts/os-mutation-gates-batch-profile-20260514-140126-04212-explicit`
  - full-gate MSI SHA-256: `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`
  - provenance commit: `d338b8a99f3e1e3839ac89a6de0da034ff3da148`
  - status: 04215 full admin host mutation 이후 historical predecessor로 보존한다.
- 이전 전체 관리자 host mutation: `0.42.12-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`
  - status: 04215 full admin host mutation 이후 historical predecessor로 보존한다.
- 이전 전체 관리자 host mutation: `0.42.12-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md`
  - status: 04215 full admin host mutation 이후 historical predecessor로 보존한다.
- 이전 전체 관리자 host mutation: `0.42.11-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md`
  - status: 04212 rerun evidence 이후 historical로 보존한다.
- 이전 전체 관리자 host mutation: `0.42.9-admin-smoke`, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`
  - status: 04211/04212 evidence 이후 historical로 보존한다.
- Manual admin hardening: `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`
- Lifecycle packaging historical/current package-pair rebaseline: `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`
- Manual admin 0427→0428 campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`
  - result: `PASS`
  - package pair: baseline `0.42.7-admin-smoke`, target `0.42.8-admin-smoke`
  - target post-merge MSI SHA-256: `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687`, provenance commit `5397e580c98a34e8b7beb5b9773d1d857025315b`
  - update ZIP SHA-256: `f8bb7900687c1a19eafc57266adbd388c826b15b4926808beac8ac0e79871ccc`
  - PASS: installed update/rollback, dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX install/update/remove, installed runtime ops summary capture
  - descriptor: `artifacts/manual-admin-campaign-20260512-0427-0428/manual-admin-campaign-descriptor/summary.json`
- Manual admin 0428→0429 candidate: `docs/ga-ready/evidence/manual-admin-campaign-candidate-2026-05-13-0428-0429.md`
  - result: `CANDIDATE_UPDATE_ROLLBACK_ONLY`
  - package pair candidate: baseline `0.42.8-admin-smoke`, target `0.42.9-admin-smoke`
  - target MSI SHA-256: `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`, provenance commit `f0620f2e18ae25de8751333684cb74b5051dcdc6`
  - update ZIP SHA-256: `7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f`
  - executed: installed update/rollback only; clean-host, Burn, MSIX, descriptor generation은 아직 PASS로 claim하지 않는다.
- Post-0426 follow-up triage: `docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md`
  - result: `PASS`
  - scope: post-merge provenance rebuild, Batch Supervisor descriptor profile, next full gate triage, 0423→0424 historical-only reclassification
  - follow-up result: 사용자 승인 후 `0.42.7-admin-smoke` package build와 full admin host mutation gate를 실행했고, 추가 승인으로 `0.42.7-admin-smoke -> 0.42.8-admin-smoke` package-pair와 0428 full gate까지 PASS했다. 이후 0429 full gate, 04211 full gate, 04212 full gate까지 PASS했으므로 최신 full gate current claim은 04212 evidence가 소유한다.
  - latest descriptor manifest: `manual-admin-campaign-descriptor-20260512-0427-0428`, helper `packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`
  - dashboard/wiki current card: installed listener `batch_evidence.latest` contract PASS, zone wiki path absent
- 내부 사설망 배포 경계: `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md`

## 다음 Manual Admin 준비

- 다음 campaign descriptor: `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`
- Current closed package-pair는 `0.42.25-admin-smoke -> 0.42.26-admin-smoke`이며
  `overall_status=pass`, `missing_count=0`, `not_pass_count=0`이다.
- 최신 operational package root는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226`,
  MSI SHA-256 `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`,
  provenance commit `d6500c01c972cbc7ca1e290e51120181ceea1501`이다.
- `0.42.25-admin-smoke` package build record는
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md`와
  `artifacts/admin-smoke-package-20260516-04225`에 보존한다. MSI SHA-256은
  `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`이며, 후속
  fullgate/manual-admin closure의 operational package와 구분한다.
- `0.42.26-admin-smoke` full admin host mutation gate는 current-card 최신 anchor를
  `full-admin-host-mutation-gate-20260516-04226`으로 올렸고, 2026-05-17 package-pair
  closure는 같은 operational package root로 닫혔다. `0.42.25` 및 이전 full gate와
  `0.42.23 -> 0.42.24`, 2026-05-16 `0.42.25 -> 0.42.26` initial blocked descriptor는
  historical predecessor로 보존한다.
- 다음 clean-host package-pair 실행은
  `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`
  기준의 Windows Update NoContact recovery summary를 남긴다.
- 2026-05-14 post-04212 follow-up triage는
  `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`에 보존한다.
  `main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea` 기준 새 product payload 변경이
  없어 `0.42.13-admin-smoke` package build와 host mutation campaign은 열지 않았다.
- 2026-05-14 `1-2-3-4-5` 재승인 follow-up은
  `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`에
  보존한다. `main` `8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 product
  payload 변경이 없어 package/host mutation chain은 보류했고, Web Console
  Dashboard/Evidence current-card smoke만 PASS로 닫았다.
- `0.42.10-admin-smoke`는
  `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`
  historical RCA로만 보존한다.
- 최신 닫힌 package-pair descriptor는 `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json`이며 `pass` 상태다.
- 최신 Batch Supervisor descriptor manifest는 `manual-admin-campaign-descriptor-20260517-04225-04226-closed`이고 summary는 `ok=true`, `executed_steps=1`, `missing_count=0`, `not_pass_count=0`이다.
- 이전 `0.42.21 -> 0.42.22` descriptor manifest `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04221-04222/manifest.json`와 Burn blocker evidence는 historical regression record로 보존한다.
- 최신 `0.42.26-admin-smoke` package build record는 `artifacts/admin-smoke-package-20260516-04226`에 보존하며, MSI SHA-256은 `aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685`이다.
- 최신 full admin host mutation은 `0.42.26-admin-smoke` / `full-admin-host-mutation-gate-20260516-04226`이며 current-card smoke가 `batch_evidence.status=available`로 닫혔다.

## Dashboard/Wiki Current Card

- dashboard/wiki current card 상태는 `installed-listener-batch-evidence-available`이다.
- Web Console은 정적 문서가 아니라 `GET /api/v1/ops/summary`의 `batch_evidence.latest`를 current evidence card로 표시한다.
- 설치본 smoke에서 최신으로 다시 확인한 값은 installed manifest `0.42.26-admin-smoke`,
  `batch_evidence.status=available`, `latest_batch_id=full-admin-host-mutation-gate-20260516-04226`,
  Runtime/API current evidence `runtime-api-current-evidence-rollup-v1`,
  Runtime/API registry bridge route detail count `4`, Web Console HTTP `200`,
  `/pcv-config.js` HTTP `200`, unauthenticated runtime policy `401`/`PCV_AUTH_REQUIRED`다.
- 2026-05-16 04226 host mutation current-card smoke artifact는 `artifacts/installed-operator-surface-current-card-20260516-04226`이며 2026-05-16 04225→04226 initial descriptor candidate, 2026-05-17 04225→04226 closed package-pair PASS, 이전 04224→04225 닫힌 package-pair PASS(`manual-admin-campaign-descriptor-20260516-04224-04225-closed`)를 구분한다.
- GA-ready current card는 04226 full admin host mutation PASS와 04225→04226 닫힌
  package-pair PASS, 04224→04225 historical closed package-pair, 04224 및 이전 historical predecessor, 04221→04222 Burn blocker 이력을 함께 구분한다.

## 보조

- Web/API port split: `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`, `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`
- Account/RBAC/JWT/console: `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`
- Installed account login/noVNC: `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`
- Frontend/backend auth console live smoke: `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`
  - artifacts: `artifacts/installed-account-login-browser-live-smoke-20260510-235543`, `artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543`, `artifacts/installed-web-asset-refresh-20260510-235258`
  - boundary: public trusted signing과 외부 stable publication evidence가 아니다.
- Product TUI service plan closure: `docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md`
- Runtime/Core, Host Ops, Hyper-V provider boundary follow-up: `docs/ga-ready/evidence/runtime-host-hyperv-domain-followup-code-level-2026-05-12.md`
- Runtime/Core console/ops-summary, Hyper-V provider file split, historical docs follow-up: `docs/ga-ready/evidence/runtime-hyperv-operator-followup-code-level-2026-05-12.md`
- Runtime/API job contract stabilization: `docs/ga-ready/evidence/runtime-api-job-contract-stabilization-code-level-2026-05-15.md`

## 역사

- 이전 full admin host mutation: `0.42.8-admin-smoke` historical, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-233650-0428-r2`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2`, `artifacts/os-mutation-gates-batch-profile-20260512-233650-0428-r2`
  - status: 0429 evidence 이후 historical로 보존한다.
- 이전 full admin host mutation: `0.42.7-admin-smoke` historical, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427`, `artifacts/os-mutation-gates-batch-profile-20260512-181309-0427`
  - status: 0428 evidence 이후 historical로 보존한다.
- 이전 Manual admin 0425→0426 campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`
  - status: `PASS`
  - package pair: baseline `0.42.5-admin-smoke`, target `0.42.6-admin-smoke`
  - status: 0427→0428 package-pair PASS 이후 historical current predecessor로 보존한다.
- 이전 full admin host mutation: `0.42.3-admin-smoke` historical, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-021337-0423`, `artifacts/os-mutation-gates-batch-profile-20260512-021337-0423`
  - status: 0428 evidence 이후 historical로 보존한다.
- Manual admin 0423→0424 campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`
  - status: `historical-partial-pass-clean-host-blocked`
  - target package: `0.42.4-admin-smoke`, MSI SHA-256 `71eaeff1c6f244bc57e9c2ac9fa57b54676d00cfbf66ba119b37c9bb21949277`
  - PASS bucket: full admin host mutation, Operator Access, Internal Service Hardening, installed update/rollback
  - blocker: dedicated clean-host `0.42.3 -> 0.42.4` baseline install sequence. Current package-pair claim은 0427→0428 PASS evidence가 소유한다.
- `0.42.10-admin-smoke` duplicate outer start RCA:
  `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`
  - target MSI SHA-256 `bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273`
  - provenance commit `d7d5ba38ee1d4f74676477eb13701af65abca008`
  - update package SHA-256 `05a107f4803ec8ed1e08f7aeba1b49fa3795c7d16565db8f904fd599ba07633f`
  - current closure는 `0.42.11-admin-smoke` package-pair와 full gate evidence가 소유한다.
- Post-0423 follow-up triage / 다음 slice plan: `docs/ga-ready/evidence/post-0423-followup-triage-2026-05-12.md`, `docs/superpowers/plans/2026-05-12-purecvisor-desktop-node-post-0423-followup-slices.md`
  - status: 0427→0428 PASS 이후 historical planning record로 보존한다.
- 이전 full admin host mutation: `0.42.2-admin-smoke` historical, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-11-0422-hostmutation.md`
  - status: 0423 evidence 이후 historical로 보존한다.
- 이전 full admin host mutation: `0.41.5-admin-smoke` historical, `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`
  - version: `0.41.5-admin-smoke`
  - artifacts: `artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415`, `artifacts/os-mutation-gates-batch-profile-20260510-195837-0415`
  - status: 0423 evidence 이후 historical로 보존한다.
- `0.41.2-admin-smoke`, `0.41.0-admin-smoke`, `0.39.x`, `0.38.x` 계열 admin-smoke와 route parity evidence는 `docs/ga-ready/evidence/`에 삭제 없이 보존한다.
- Stabilize Then Split 중에는 과거 기록을 최신/current로 재표기하지 않는다.

## 미채택 종료

- Public distribution 후보: `docs/adr/0005-public-distribution-operations-expansion-candidate.md`
- Public distribution gate matrix: `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`

## 규칙

Public trusted signing, winget public submission, external stable publication,
public clean-host signed smoke는 새 ADR이 배포 경계를 바꾸기 전까지 범위 밖이다.
