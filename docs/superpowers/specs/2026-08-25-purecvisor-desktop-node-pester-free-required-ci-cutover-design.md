# PureCVisor Desktop Node Pester-free Required CI Cutover Design

- Design-ID: `purecvisor-desktop-node-pester-free-required-ci-cutover-20260825-v1`
- 상태: `approved`
- 구현 상태: `completed-with-disclosed-deviation`
- 완료 증빙: `docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md`
- Section approval: `2026-08-25 user-approved`
- Written-spec approval: `2026-08-25 user-approved`
- 선행 설계:
  `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design.md`
- 선행 구현:
  `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-wave-a.md`,
  `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b.md`
- Active authority delta:
  `docs/superpowers/specs/2026-08-25-purecvisor-desktop-node-public-authority-snapshot-delta-design.md`
- 설계 당시 Wave B HEAD: `[private-source-commit]`
- 제품/호스트 mutation: `false`
- public trusted signing / external stable publication claim: `false / false`

## 1. 목적

Wave C Installer, Wave D Packaging과 Wave E required CI cutover를 완료한다. 최종 상태는 다음
네 문장을 동시에 만족해야 한다.

1. Legacy Pester 파일 `62/62`와 계약 `627/627`이 replacement에 1:1로 연결된다.
2. Required CI의 Pester invocation은 `0`이다.
3. Required CI의 비관리자 `pwsh`/`powershell` invocation은 `0`이다.
4. Shadow dual-run, cutover CI와 merge 후 main CI가 실제 PASS한 뒤에만 cutover를 완료로 기록한다.

기존 Pester 파일을 삭제하지 않는다. 이 파일은 non-required parity reference와 rollback 기준으로
남긴다. 관리자 MSI/Service/Hyper-V/VM 검증과 ADR-0009 Guest PowerShell Direct도 제거 대상이
아니다.

## 2. 승인된 결정

### 2.1 완료 범위

Web만 부분 전환하지 않는다. Installer 6개 파일의 49개 계약과 Packaging 55개 파일의 528개
계약을 모두 이전해 전체 62파일, 627계약을 닫는다.

| Domain | Legacy files | Legacy contracts | Current state | Target state |
|---|---:|---:|---|---|
| Web | 1 | 50 | mapped / local pass / CI pending | cutover |
| Installer | 6 | 49 | unmapped | cutover |
| Packaging | 55 | 528 | unmapped | cutover |
| Total | **62** | **627** | 61 files pending | **62/62 cutover** |

### 2.2 실행 방식

단계형 global cutover를 사용한다.

1. 기존 private remote/history를 public-safety audit하고 P1 exposure를 확인하면 exact-repo 전환을
   중단한다.
2. Local main committed tree와 Wave B/design branch를 clean private source branch에서 병합하고
   user/host/network/secret-fixture data를 정제한다.
3. 정제 tree만 별도 Git repository의 단일 root commit으로 만들고 local P0/P1 `0`을 검증한다.
4. 새 authoritative target `HardcoreMonk/purecvisor-desktop-node-public`을 private로 bootstrap한 뒤
   seed CI/provider audit, 별도 one-way 승인, public 전환과 current-four protection을 수행한다.
5. Wave C Installer를 작은 batch로 이전한다.
6. Wave D Packaging을 domain batch로 이전한다.
7. 같은 commit에서 legacy와 replacement를 실행하는 required shadow CI를 PASS한다.
8. 별도 단일 cutover commit으로 required workflow와 catalog를 전환한다.
9. 새 required checks와 GitHub ruleset readback을 확인한 뒤 PR을 merge한다.
10. Merge 후 main CI와 post-merge evidence를 별도로 닫는다.

단일 대형 commit과 도메인별 부분 cutover는 사용하지 않는다. 전자는 dual-run과 rollback 경계를
섞고, 후자는 required workflow를 장기간 혼합 상태로 남긴다.

### 2.3 통합 방식

GitHub PR 기반 통합을 사용한다. 현재 로컬 main worktree에는 사용자 소유 수정·미추적 파일이
있으므로 local main에서 직접 merge/pull하지 않는다. Empty new repository의 sanitized root seed만
explicit bootstrap main push를 허용하고, 그 뒤 cutover와 evidence는 protected remote PR에서
required CI와 review를 통과한 뒤 merge한다. Force-push는 금지한다.

현재 repository는 private GitHub Free 경계 때문에 ruleset/branch protection API가 `403`을
반환한다. Existing remote audit가 actual user path/identity/private endpoint P1을 확인했으므로
exact repository public 전환은 폐기했다. 사용자는 기존 remote를 변경하지 않는 조건으로 새
authoritative repository `HardcoreMonk/purecvisor-desktop-node-public`과 sanitized single-root snapshot을
승인했다. 새 target의 visibility mutation은 local/seed/provider 감사 PASS와 별도 실행 직전 one-way
확인 전에는 수행하지 않는다.

## 3. 저장소와 branch 경계

2026-08-25 read-only 확인 기준이다.

- Private archive remote: `[private-archive-repository]`
- Private archive visibility: `PRIVATE` and unchanged
- New authoritative remote after local sanitization: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public.git`
- New repository initial visibility: `PRIVATE`; target visibility after fresh approval: `PUBLIC`
- `origin/main` 대비 local `main`: ahead `67`, behind `0`
- `origin/main` 대비 Wave B branch: ahead `96`, behind `0`
- Local main과 Wave B의 merge-base: `[private-source-commit]`
- Local main과 Wave B의 독립 commit: main `3`, Wave B `32`
- Open PR: `0`

Local main의 다음 항목은 사용자 소유이며 baseline branch나 cutover branch에 자동 포함하지 않는다.

- modified: `docs/DEVELOPER_INDEX.md`
- modified: `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md`
- untracked: migration manifest/schema, manifest verifier, 0.42.75 package evidence와 2026-08-23
  audit design/plan

Seed-source branch는 local main의 committed object와 Wave B/design committed history만 병합한다.
Dirty 파일을 stash, clean, delete, overwrite 또는 stage하지 않는다. 정제 후 `git archive` tree만
별도 empty local repository의 single root main으로 만든다. Cutover branch는 이 새 root main에서
만들며 old object database/ref/provider data를 전달하지 않는다.

## 4. 계약 ledger v2

### 4.1 파일과 계약 inventory

현재 v1 manifest는 62개 파일만 계량하고 Web replacement ID만 허용한다. v2는 file aggregate와
627개 contract row를 분리한다.

```json
{
  "contract": "pcv-development-verification-migration-manifest-v2",
  "schema_version": 2,
  "inventory": {
    "files": { "total": 62, "packaging": 55, "installer": 6, "web": 1 },
    "contracts": { "total": 627, "packaging": 528, "installer": 49, "web": 50 }
  },
  "entries": [],
  "contracts": []
}
```

각 contract row는 다음 exact field를 가진다.

```json
{
  "legacy_path": "packaging/windows-desktop-node/installer/tests/example.Tests.ps1",
  "legacy_ordinal": 1,
  "legacy_name": "exact literal It name",
  "domain": "installer",
  "replacement_owner": "src/DesktopNode.Delivery.Tests/...cs",
  "replacement_contract_id": "pcv.installer.example.contract-name",
  "parity_status": "mapped",
  "local_parity": { "status": "pending", "evidence": null },
  "ci_parity": { "status": "pending", "evidence": null }
}
```

ID prefix는 `web.static.*`, `pcv.installer.*`, `pcv.delivery.*`만 허용한다. `legacy_path +
legacy_ordinal`, exact name과 replacement ID는 각각 unique해야 한다. File aggregate의
`legacy_contract_count`와 contract row 수가 다르면 실패한다.

### 4.2 Legacy parser

현재 Installer/Packaging의 577개 `It` 선언은 모두 한 줄 literal name이고 variable interpolation이
없다. C# parser는 PowerShell을 실행하지 않고 comment/here-string/string boundary를 인식해 literal
`It` 선언만 추출한다. 다음을 fail-closed로 거부한다.

- expandable/dynamic test name
- duplicate literal name inside one file
- unsupported multiline declaration
- unmatched quote, here-string 또는 comment boundary
- discovery file, order 또는 count drift

Web 50개 parser와 ledger는 기존 Node owner를 유지하며 v2 aggregate에 결합한다.

### 4.3 C# test metadata

`DesktopNode.Delivery.Tests`의 각 replacement test는 다음 metadata를 가진다.

```csharp
[PcvLegacyContract(
    "pcv.installer.example.contract-name",
    "packaging/windows-desktop-node/installer/tests/example.Tests.ps1",
    1,
    "exact literal It name")]
```

Reflection inventory test가 assembly metadata와 manifest v2를 exact order로 비교한다. Test method
이름이나 source line만으로 mapping을 추론하지 않는다.

## 5. C# verification 구조

### 5.1 `DesktopNode.Delivery.Tests`

새 `.NET 10` xUnit project를 solution에 추가한다. 한 giant fixture를 만들지 않고 legacy file과
책임 경계를 보존한다.

- `Installer/InternalTrust`, `Lifecycle`, `Plan`, `Signing`, `WixSource`, `Wrapper`
- `Delivery/Evidence`, `ProductManifest`, `ProductPlan`, `Orchestration`, `Policy`, `Preflight`
- 공통 read-only filesystem/JSON/XML/Markdown/source contract helpers
- contract metadata discovery와 deterministic negative fixture helpers

각 legacy file은 독립 fixture 또는 명시된 소규모 fixture group을 가진다. Category는
`Installer` 또는 `Delivery`로 고정하고 catalog shard filter와 일치시킨다.

### 5.2 Replacement 원칙

- XML/JSON/Markdown/WiX/source 계약은 C# reader가 repository file을 직접 읽어 검증한다.
- 기존 Pester가 생성한 non-admin dry-run plan 계약은 host mutation 없는 C# fixture/port로 옮긴다.
- 관리자 command 실행, installed state와 실제 VM 결과는 실행하지 않고 descriptor/source/evidence
  shape만 검증한다.
- C# replacement는 `pwsh`, `powershell`, `msiexec`, `sc.exe` 또는 mutation tool을 child process로
  호출할 수 없다.
- 단순 문자열 존재 검사만으로 동등성을 주장하지 않는다. Structured source이면 parser를 쓰고,
  behavioral policy이면 valid/invalid fixture 쌍으로 양쪽 branch를 검증한다.

### 5.3 Current evidence checker

`evidence-check` managed handler를 활성화한다. Required path는 check-only다.

1. `docs/ga-ready/current-evidence.json` schema를 읽는다.
2. referenced evidence와 current-facing generated blocks를 project한다.
3. expected/current tuple, duplicates와 stale pointer를 비교한다.
4. mismatch는 수정하지 않고 stable error와 relative path를 반환한다.

Writer나 promotion은 별도 관리자/운영 명령으로 남기고 required CI에서 호출하지 않는다.

### 5.4 Catalog activation

Migration 중 catalog는 `shadow-ready`를 사용한다. 모든 627 local/CI parity가 확보되고 cutover
commit에서만 `active`로 바꾼다. `wave-c-pending`, `wave-d-pending`, `wave-b-pending` suite는
각 batch 완료에 따라 `mapped`, `dual-run-pass`, `cutover` 순서로 전이한다. 상태 건너뛰기는
architecture test가 거부한다.

Checked-in manifest는 shadow commit에서 미래 run을 참조할 수 없으므로 `mapped`/CI pending을
유지한다. Shadow run PASS 뒤 바로 다음 단일 cutover commit은 parent가 recorded shadow SHA이고
그 immutable run이 `dual-run-pass`를 증명할 때에만 `mapped -> cutover`를 원자적으로 기록할 수
있다. 이는 논리 상태를 건너뛰는 예외가 아니라 self-reference 없는 persisted transition이며,
중간 evidence commit을 삽입해 cutover parent 관계를 끊는 것은 금지한다.

## 6. Batch 전략

### 6.1 Wave C Installer

6개 legacy file을 책임별 commit으로 이전한다. 각 commit은 metadata RED, replacement GREEN,
positive/negative parity, full Installer category와 legacy file Pester를 통과해야 한다.

### 6.2 Wave D Packaging

55개 file을 다음 domain batch로 분할한다.

1. canonical evidence와 promotion projection
2. product manifest/plan/diagnostics
3. development verification와 CI policy
4. batch/orchestration/timeout/retry
5. installer/package/public distribution preflight
6. manual-admin descriptor/readiness
7. installed-smoke descriptor와 non-mutation boundary
8. reconciliation and lifecycle policy

한 batch는 최대 8개 legacy file을 소유한다. 90계약 또는 61계약처럼 큰 file은 단독 batch로
분리할 수 있다. Batch 간 공유 helper는 첫 소비자와 함께 추가하고 unused abstraction을 미리
만들지 않는다.

### 6.3 Batch gate

각 batch는 다음을 모두 요구한다.

- exact contract inventory RED → GREEN
- 모든 contract의 valid fixture PASS와 deterministic invalid fixture FAIL
- focused `dotnet test` Category/fixture PASS
- 전체 `DesktopNode.Delivery.Tests` PASS
- 기존 대응 Pester file PASS, failed/skipped/not-run `0`
- full .NET Release와 Web required-equivalent local command PASS
- manifest missing/duplicate/unmapped 수 정직한 전이
- independent fixed-diff review finding `0`
- host/service/MSI/VM mutation `0`

## 7. Required CI shadow

Shadow commit은 기존 네 required job identity를 유지하고 legacy와 replacement를 같은 SHA에서
실행한다.

| Existing job | Legacy path | Replacement path |
|---|---|---|
| `.NET product/tests` | direct solution test | runner `--shard dotnet` |
| `Web type/parity/browser` | existing npm + Web Pester reference | Node Web contracts + runner `--shard web` |
| `Packaging Pester` | Packaging Pester 55 files | runner `--shard delivery` |
| `Installer/Web Pester` | Installer 6 + Web 1 Pester | runner `--shard installer-policy` + Web contracts |

각 job은 legacy와 replacement 결과를 별도 artifact로 업로드한다. Shadow evidence는 commit SHA,
run ID/URL, job ID, contract count, pass/fail/skip/not-run, summary SHA-256, duration과 host mutation
false를 기록한다.

Shadow commit의 checked-in manifest는 CI 실행 전 `ci pending`을 유지한다. CI artifact가 자기
commit SHA의 pass를 소유한다. Cutover commit이 그 immutable predecessor run을 evidence locator로
고정한다. 이렇게 해야 future run ID를 미리 적는 자기참조를 만들지 않는다.

## 8. Wave E cutover

### 8.1 단일 cutover commit

Shadow CI가 PASS한 직후 다음 범위만 한 commit으로 변경한다.

- `.github/workflows/development-gates.yml`
- suite catalog/schema activation state와 executable mapping
- migration manifest/schema parity state/evidence locator
- workflow/catalog/manifest cutover guards
- current-facing policy와 cutover evidence skeleton

이 commit을 revert하면 shadow workflow, catalog와 manifest가 함께 복구되어야 한다. Product code,
legacy Pester source와 관리자 workflow를 섞지 않는다.

### 8.2 최종 required jobs

1. `.NET` shard
2. Web Node shard
3. Delivery/evidence C# shard
4. Installer/policy C# shard

Required workflow executable step에는 Pester install, `Invoke-Pester`, `pwsh`, `powershell`이 없다.
PowerShell 문자열이 documentation이나 non-required reference locator에 존재하는 것은 invocation으로
세지 않지만 executable/shell/run command token으로 나타나면 실패한다.

### 8.3 Static cutover guard

Guard는 YAML을 structured parse하고 다음을 검사한다.

- required four-job union exact
- suite missing/duplicate/skip `0`
- Pester executable/invocation `0`
- non-admin PowerShell executable/shell invocation `0`
- forbidden mutation executable/argument `0`
- catalog `active`
- manifest files `62/62`, contracts `627/627`, unmapped `0`
- cutover commit parent가 recorded shadow SHA와 같음
- cutover diff가 approved allowlist 밖을 변경하지 않음

## 9. Public conversion과 GitHub ruleset 전환

### 9.1 Public conversion safety gate

현재 private repository에서는 ruleset과 branch protection API가 다음 provider 오류로 차단된다.

```text
403: Upgrade to GitHub Pro or make this repository public to enable this feature.
```

Repository public 전환은 clone, cache와 fork로 복제된 이력을 회수할 수 없으므로 reversible
rollback으로 취급하지 않는다. Existing remote scan에서 P1이 확인됐으므로 그 repository는
visibility mutation 대상에서 영구 제외한다. 다음 gate를 전부 PASS해야 새 target의 visibility
mutation을 열 수 있다.

1. Private archive가 계속 `PRIVATE`이고 remote state가
   변경되지 않았음을 readback한다.
2. Sanitized tree를 별도 empty Git repository의 parent 없는 root commit으로 만들고 old ref/object,
   tag, provider data가 없음을 검증한다.
3. Full current root에서 credential, token, private key/certificate material, personal data, internal
   URL, absolute user path와 confidential evidence를 검사해 unresolved P0/P1 `0`을 요구한다.
4. Rights-reserved License, SECURITY policy와 public release boundary를 추가·검증하고 source 공개와
   trusted binary publication을 구분한다.
5. Exact new target `HardcoreMonk/purecvisor-desktop-node-public`을 private/empty로 생성하고 root main
   하나만 explicit refspec으로 bootstrap한다.
6. New provider branch/tag/release/issue/Actions log/artifact/package metadata를 inventory하고 seed SHA의
   CI와 provider scan을 PASS한다.
7. P0/P1 노출 후보는 allowlist로 숨기지 않고 제거·credential rotation·새 root regeneration 후
   처음부터 재검증한다.
8. Fresh audit PASS 뒤 visibility 변경 직전에 exact new owner/repository/root SHA와 irreversible
   exposure를 사용자에게 다시 제시하고 one-way 실행 승인을 받는다.

Public 전환은 GitHub의 explicit consequence acknowledgement를 사용하고 즉시 visibility `PUBLIC`을
독립 GET으로 read back한다. 변경 후 private로 되돌려도 이미 발생한 노출을 회수했다고 주장하지
않는다.

License policy는 2026-08-25 사용자 선택으로 **rights reserved / inspection only**로 고정한다.
저장소가 public이어도 복제·수정·재배포·sublicense·판매 권한을 부여하지 않으며 open-source
license나 public domain 전환으로 해석하지 않는다. Copyright holder는 exact repository owner
`HardcoreMonk`로 기록하고, 별도 서면 허가 없는 사용권을 만들지 않는다.

2026-08-25 archive initial inventory는 remote branch 18개, remote tag 0개, local-only tag 1개,
release 0개, retained Actions artifact 0개다. Archive history의 P1 때문에 exact-repo conversion은
중단됐다. New target inventory는 root bootstrap 뒤 fresh zero/one-state로 별도 측정한다.

### 9.2 Required-check protection

Cutover branch의 새 네 job이 PASS한 뒤에만 required status check 이름을 바꾼다.

1. Public conversion 직후 current four checks를 main branch protection에 등록하고 exact readback한다.
2. GitHub ruleset/branch protection before JSON과 ETag를 read-only로 저장한다.
3. Expected repository, branch, actor와 current required checks를 검증한다.
4. 새 네 check를 한 요청으로 갱신한다.
5. 독립 GET으로 exact readback하고 before/after digest를 기록한다.
6. Readback mismatch이면 before JSON으로 복구하고 PR merge를 금지한다.

Ruleset mutation은 사용자가 승인한 required CI cutover 범위다. 다른 permission, review rule,
force-push/deletion setting은 변경하지 않는다.

## 10. Sanitized root bootstrap과 cutover PR

### 10.1 New authority seed

- Local path: `D:/data/projects/codex-zone/purecvisor-desktop-node-public`
- Remote: `HardcoreMonk/purecvisor-desktop-node-public`
- Source tree: committed local main + Wave B/design merge + approved sanitization only
- Git shape: one parentless root `main`; old object/ref/tag/provider data `0`
- Excludes: local main dirty/untracked files and all old provider data
- Gate: local full test/audit, private seed push/CI/provider audit, fresh one-way public approval
- Exception: empty-repo seed is the sole direct main bootstrap; protection 뒤 direct main push 금지

### 10.2 Cutover PR

- Branch: `codex/pester-free-verification-cutover`
- Source: protected sanitized public root main
- Contains: Wave C/D implementation, shadow evidence, single Wave E cutover commit
- Gate: new required checks, ruleset readback, plan completion and no open findings
- Merge method: repository가 허용하는 non-force method

Local dirty main은 bootstrap/cutover 동안 그대로 둔다. New remote merge 뒤 private archive local main을
자동 pull/reset하지 않는다.

## 11. Test and evidence matrix

| Layer | Required result |
|---|---|
| Contract inventory | 62 files, 627 contracts, missing/duplicate/order drift 0 |
| C# Delivery | Installer 49/49, Packaging 528/528, failed/skipped 0 |
| Web Node | 50/50 positive, deterministic negative parity |
| Legacy reference | Pester 627/627 on shadow commit, failed/skipped/not-run 0 |
| .NET Release | all assemblies PASS, count drift explained and reviewed |
| Required shadow CI | legacy and replacement both PASS on same SHA |
| Required cutover CI | four replacement jobs PASS, Pester/PowerShell invocation 0 |
| Main post-merge CI | same four jobs PASS |
| Performance | full required wall-clock <= 3 minutes 34 seconds |
| Mutation | host/service/MSI/VM/Guest mutation 0 |

Every positive contract has one deterministic negative fixture. File removal, assertion omission, invalid
JSON/XML/YAML, unsafe path, duplicate mapping, stale evidence, secret exposure and forbidden executable
fixtures must fail with stable identifiers.

## 12. Failure and rollback

The workflow is fail-closed.

- In-branch test failure: stop before push.
- Shadow CI failure or skip: no cutover commit.
- Cutover CI failure: keep PR unmerged, fix outside cutover commit or revert cutover to shadow.
- Ruleset mismatch: restore exact before JSON and stop.
- Public safety audit P0/P1 또는 unresolved finding: visibility mutation을 실행하지 않는다.
- Public visibility 변경 후 문제 발견: merge와 push를 중단하고 exposure incident로 보고한다.
  Visibility를 private로 되돌리는 것은 추가 노출을 줄일 뿐 이미 복제된 history의 rollback이 아니다.
- Merge conflict with user-owned local main files: do not resolve in local main; keep remote PR path.
- Main post-merge failure: revert cutover commit through a PR, restore old required checks and verify readback.
- Performance over 3:34: profile shard timing, do not weaken timeout or drop tests.

No destructive reset, force-push, broad clean, user-file stash deletion or host mutation is permitted.

## 13. Documentation and claims

Current-facing documents are updated only from measured output. Sanitized public historical copies preserve
the original semantic result while removing P1 paths/endpoints and marking redaction. Post-merge evidence records root seed, shadow run, cutover PR, ruleset
before/after, cutover CI and main CI as separate immutable locators.

Only after every completion condition passes may documents set:

```text
ci_parity_pass=true
required_ci_pester_zero=true
required_ci_nonadmin_powershell_zero=true
cutover_completed=true
host_mutation_performed=false
msi_or_service_mutation=false
actual_vm_tested=false
public_trusted_signing=false
external_stable_publication=false
```

This cutover does not create a package candidate, change operational current, claim public signing or
publish an external stable binary release. Repository source visibility는 approved safety gate 뒤
`PUBLIC`으로 전환하지만 `public_trusted_signing=false`와 `external_stable_publication=false`는
계속 유지한다. The repository has no root `VERSION` or `CHANGELOG.md`; verification-only integration
does not invent them.

## 14. Completion conditions

1. Private archive remains private/unchanged and its P1 history is not copied.
2. Sanitized new root/provider public-safety audit PASS, Gitleaks finding `0`, unresolved P0/P1 `0`.
3. Exact new repository `HardcoreMonk/purecvisor-desktop-node-public` visibility `PUBLIC` readback and main
   branch protection active.
4. Installer `49/49` exact contract mapping and local parity PASS.
5. Packaging `528/528` exact contract mapping and local parity PASS.
6. Web `50/50` remains PASS.
7. Manifest file `62/62`, contract `627/627`, missing/duplicate/unmapped `0`.
8. Same shadow commit legacy/replacement required CI PASS.
9. Single cutover commit is revertable to the shadow state.
10. Required workflow Pester invocation `0`.
11. Required workflow non-admin PowerShell invocation `0`.
12. Required four-job missing/fail/skip `0` and wall-clock <= 3:34.
13. GitHub ruleset before/after readback and rollback material complete.
14. Cutover PR merged and remote main required CI PASS.
15. Post-merge evidence/current-facing docs merged and CI PASS.
16. Local main user-owned dirty files remain byte-for-byte untouched.
17. New public Git root has no old parent/ref/tag/provider data and uses GitHub no-reply author identity.
18. Host/service/MSI/VM mutation `0`, trusted signing/stable binary publication claims remain false.

## 15. Implementation planning boundary

하나의 거대 실행 파일에 577개 계약과 provider mutation을 섞지 않는다. Program index는 dependency와
전체 완료 판정만 소유하고 다음 네 상세 plan이 실제 실행을 소유한다.

1. Sanitized root bootstrap, public-safety audit, one-way visibility gate와 current-four protection:
   `docs/superpowers/plans/2026-08-25-purecvisor-desktop-node-public-baseline-and-protection.md`
2. Wave C Installer 49-contract migration:
   `docs/superpowers/plans/2026-08-25-purecvisor-desktop-node-pester-free-installer-wave-c.md`
3. Wave D Packaging 528-contract domain batches:
   `docs/superpowers/plans/2026-08-25-purecvisor-desktop-node-pester-free-packaging-wave-d.md`
4. Shadow CI, Wave E cutover, ruleset transition, merge와 post-merge evidence:
   `docs/superpowers/plans/2026-08-25-purecvisor-desktop-node-required-ci-cutover-wave-e.md`

각 plan은 독립 review checkpoint를 가지며 다음 plan은 predecessor의 committed PASS evidence를
입력으로 요구한다. 각 task는 RED → GREEN → fixed-diff review → commit 순서를 지킨다.

시간이나 commit 수를 줄이기 위해 577개 mapping, shadow CI, ruleset readback 또는 post-merge CI를
생략할 수 없다. 반대로 관리자 PowerShell 전환, Pester 파일 삭제, actual-host/VM 검증, package
build와 public release는 이 계획에 추가하지 않는다.
