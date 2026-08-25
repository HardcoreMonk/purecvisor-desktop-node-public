# PureCVisor Desktop Node Development Throughput Automation Design

date: `2026-07-16`
status: `approved-design`
scope: `development-feedback-evidence-generation-baseline-reservation-approved-merge`
active_operator_surfaces: `web-console,pcvcli`
tui_product_status: `removed`
current_operational_anchor: `0.42.64-admin-smoke`
host_mutation_performed: `false`
package_build_performed: `false`
product_runtime_payload_change: `false`
packaging_and_development_tooling_change: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 목표

제품 안전성과 현재 전체 검증 범위를 유지하면서 일상 개발의 피드백 시간을 줄인다. 이를 위해
검증을 fast/full/release lane으로 분리하고, 느린 Pester 테스트에서 실제 자식 프로세스와 실시간
대기를 제거하며, current evidence의 단일 원본과 생성기를 도입한다. 변경 규모별 문서 요구를
명시하고 manual-admin N-1 baseline을 예약하며, 한 번 승인된 PR은 운영자 workstation이 필수
workflow를 감시한 뒤 head SHA가 변하지 않았을 때만 병합한다.

이번 설계는 Web Console과 PCVCLI만 활성 운영자 표면으로 유지한다. TUI source, package,
smoke 또는 current 계약을 다시 추가하지 않는다.

## 조사 결과와 문제 정의

2026-07-16 동일 host에서 측정한 기준선은 다음과 같다.

| 검증 | 결과 | 기준 시간 |
| --- | ---: | ---: |
| .NET solution | 591 passed | 20.4초 |
| Web type/static/parity | pass | 11.2초 |
| packaging Pester | 385 passed | 141.9초 |
| installer/Web Pester | 94 passed | 59.3초 |
| 전체 local development gate | pass | 232.8초 |

Pester가 전체 local 시간의 약 86%를 사용한다. `PcvBatchSupervisor.Tests.ps1`은 실제 `pwsh`,
1초 timeout, 2초 sleep과 retry를 실행해 약 40.5초를 사용한다. Installer Plan/Signing 두 파일은
반복해서 별도 `pwsh`를 시작하며 합계 약 45.8초를 사용한다.

저장소 추적 파일 899개 중 docs가 603개이고 `docs/ga-ready/evidence`가 407개다. 0.42.64
operational promotion commit 하나가 같은 current 사실을 15개 파일에 반영했다. 이 구조는
사실의 정확성보다 복제 위치의 동기화에 많은 시간을 사용하고 누락 가능성을 만든다.

Manual-admin `0.42.62 -> 0.42.63` campaign은 검증 host가 이미 target 0.42.63으로
업그레이드되어 `blocked-by-installed-baseline-version-mismatch`로 종료됐다. Baseline host를
target 설치 전에 예약하지 않으면 같은 문제가 반복된다.

GitHub 저장소는 private Free 플랜이다. Native branch protection과 rulesets API는 403을
반환하고 `allow_auto_merge=false`다. 보호 규칙 없는 custom merge bot은 장기 write token과
`pull_request_target` 공격 표면을 추가하므로 사용하지 않는다.

## 검토한 접근

| 접근 | 장점 | 판정 |
| --- | --- | --- |
| 정책 문서만 추가 | 변경량이 작음 | 실제 피드백 시간과 중복 갱신을 줄이지 못하므로 거부 |
| 단계형 검증 + test seam + evidence 생성기 + 운영자 병합 watcher | 기존 gate를 유지하면서 측정된 병목을 직접 제거 | 선택 |
| 무인 merge bot과 전면 release orchestrator | 자동화 범위가 큼 | private Free 보호 규칙 부재와 영구 write 권한 위험으로 거부 |

선택한 접근은 개발자 workstation의 빠른 반복과 release candidate 검증을 분리한다. Main/PR의
전체 비파괴 CI와 실제 host mutation gate는 제거하거나 완화하지 않는다.

## 전체 구조

```text
changed files + change tier
          |
          v
development verification orchestrator
  fast --------> scoped .NET/Web/Pester --------> JSON summary
  full --------> all non-mutating gates --------> JSON summary
  release -----> full + evidence/baseline preflight
                         |
                         v
              separately approved existing package/host runners

operational artifacts ---> current-evidence.json ---> generated current blocks

approved PR + approved head SHA
          |
          v
operator merge watcher ---> named workflows green? ---> merge exactly that SHA
```

## 1. 단계형 개발 검증

### 진입점

새 진입점은
`packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1`이 소유한다.

주요 parameter는 다음과 같다.

- `-Lane Fast|Full|Release`
- `-ChangeTier S|M|L`
- `-BaseRef <git-ref>` 또는 `-ChangedPath <path[]>`
- `-ArtifactRoot <path>`
- `-PlanOnly`

각 실행은 schema-versioned JSON summary를 작성한다. Summary는 선택된 suite, 선택 이유, 명령,
exit code, duration, skipped suite와 skip 이유를 포함한다. Secret, token, absolute protected-token
path는 기록하지 않는다.

### Fast lane

Fast lane은 안전한 최소 집합을 선택한다.

| 변경 경로 | 실행 |
| --- | --- |
| `src/**`, `*.sln`, `*.csproj` | 전체 .NET test |
| `web/**` | npm test, static parity, Web Pester |
| installer build/source/tests | installer focused Pester |
| batch supervisor module/tests | batch supervisor focused Pester |
| current evidence JSON/generator/current-block 문서 | evidence generator `-Check`와 focused evidence Pester |
| verification/classification 도구 | 해당 도구 focused Pester |
| 알 수 없는 경로 또는 여러 위험 영역 | Full lane으로 자동 승격 |

Fast lane은 테스트를 생략한 사실을 숨기지 않는다. 선택되지 않은 suite는 summary에
`not-selected-by-scope`로 남긴다. 캐시가 있는 일반 code-only 수정의 목표 피드백 시간은
60초 이하다.

### Full lane

Full lane은 현재 development gate와 동일한 비파괴 범위를 실행한다.

1. `dotnet test src/DesktopNode.sln -c Release`
2. `npm test --prefix web`
3. `npm run verify:parity --prefix web`
4. packaging Pester 전체
5. installer/Web Pester 전체
6. current evidence generation check
7. `git diff --check`

PR과 main GitHub Actions는 Full 범위를 유지한다. Fast lane 성공을 Full 성공으로 표현하지
않는다.

### Release lane

Release lane은 Full 이후 다음 preflight를 추가한다.

- current evidence JSON/schema/generation check
- manual-admin baseline reservation check
- package candidate version과 installed baseline 관계 확인
- existing package/fullgate runner의 PlanOnly manifest 생성
- signing/publication boundary 확인

Release lane 자체는 기본적으로 host mutation을 실행하지 않는다. MSI install, service,
Hyper-V, firewall, Event Log, trust store, Credential Manager mutation은 기존 runner에 대한 별도
명시적 승인과 `AllowHostMutation` 경계를 계속 요구한다.

## 2. 느린 Pester 구조 개선

### Batch Supervisor seam

`PcvBatchSupervisor.psm1`의 production 기본 동작은 실제
`System.Diagnostics.Process`, `Get-Date`, `Start-Sleep`을 계속 사용한다. 테스트를 위해
`Invoke-PcvBatchSupervisor`에서 내부 execution dependency를 전달할 수 있게 한다.

- process factory: start, state, exit, stdout/stderr, kill을 소유
- clock provider: 현재 시간을 반환
- wait action: heartbeat interval 진행을 소유

기본 dependency를 생략하면 기존 production 구현을 사용한다. Focused unit test는 fake process와
fake clock으로 success, timeout, retry, heartbeat, resume를 시간 경과 없이 검증한다. 실제
`pwsh` integration은 정상 exit/output과 process-start failure를 대표하는 최소 사례만 유지한다.

Fake dependency는 설치 제품이나 public runtime surface에 노출하지 않는다. Batch supervisor
tool module의 test seam으로만 사용한다.

### Installer in-process boundary

`packaging/windows-desktop-node/installer/build.ps1`의 reusable plan, validation, redaction,
tool-runner 로직을 `PcvDesktopNodeInstaller.Build.psm1`로 이동한다. `build.ps1`은 parameter를
받아 module entry를 호출하고 structured JSON/exit code로 변환하는 얇은 process wrapper가 된다.

Plan/Signing unit test는 module을 import해 in-process로 호출하고 fake WiX/SignTool runner를
주입한다. Wrapper의 argument binding, JSON, exit code는 성공/실패/secret redaction을 대표하는
소수 end-to-end test가 실제 `pwsh`로 검증한다.

테스트 수와 검증 의미는 줄이지 않는다. 동일 host에서 packaging 및 installer/Web Pester 합계
시간을 기존 기준보다 최소 30% 단축하는 것을 목표로 한다.

## 3. Current evidence 단일 원본

### Canonical document

`docs/ga-ready/current-evidence.json`을 current operational 사실의 단일 원본으로 둔다.
`docs/ga-ready/current-evidence.schema.json`은 다음 필드를 검증한다.

- schema version과 generated contract version
- active operator surfaces와 TUI removed 상태
- current package/fullgate/functional-correctness/installed anchor
- clean/operational MSI와 payload SHA-256
- provenance commit
- latest closed manual-admin package pair
- blocked follow-up과 blocker
- signing/publication claim boundary

Historical evidence 문서는 수정하거나 JSON으로 합치지 않는다. JSON은 current pointer와 current
facts만 소유한다.

### Generator

`Update-PcvCurrentEvidenceDocs.ps1`은 JSON에서 bounded generated block을 만든다. 각 대상은
`BEGIN/END GENERATED CURRENT EVIDENCE` marker를 정확히 하나씩 가진다.

- `AGENTS.md`
- `docs/ga-ready/EVIDENCE_INDEX.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `packaging/windows-desktop-node/README.md`

기본 mode는 block을 갱신한다. `-Check`는 파일을 쓰지 않고 예상 내용과 다르면 non-zero로
종료한다. Marker 밖의 historical 서술은 절대 재작성하지 않는다. Generator는 JSON schema,
SHA-256 형식, referenced evidence/artifact 존재와 CLI/Web-only surface를 검증한 뒤에만 쓴다.

새 operational promotion은 JSON 한 곳과 새로운 immutable evidence 문서만 사람이 작성한다.
나머지 current mirror는 generator가 소유한다.

## 4. 변경 규모별 문서 요구

새 정책 `docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`는 다음 tier를 단일 진실로 정의한다.

| Tier | 기준 | 필수 검증/문서 |
| --- | --- | --- |
| S | 한 module, public contract/installer/host mutation 없음 | focused test, Fast lane, 명확한 commit |
| M | 여러 module, API/CLI/Web/packaging 비파괴 contract 변경 | brief design note, Full lane, 필요 evidence |
| L | 보안 경계, installer lifecycle, current anchor, 실제 host mutation 또는 public release | 전체 design + implementation plan + Release lane + operational evidence |

경계가 애매하면 높은 tier를 사용한다. S 변경에 별도 수백 줄 design/plan을 강제하지 않는다.
L 변경의 기존 승인·증거 계약은 완화하지 않는다. 검증 orchestrator는 lane과 tier가 맞지 않으면
자동 승격하거나 stable error를 반환한다.

## 5. N-1 baseline reservation

새 reservation descriptor는 campaign 실행 전에 baseline 자원을 고정한다.

- campaign id
- baseline version과 target version
- host identity fingerprint
- reservation kind: `dedicated-host` 또는 `hyperv-checkpoint`
- checkpoint/host reference
- installed manifest version at reservation time
- created/expiry timestamp
- status: `reserved|consumed|released`

`New-PcvManualAdminBaselineReservation.ps1`은 descriptor를 artifact root에 만든다. 이 도구는
VM이나 host를 생성·변경하지 않는다. Reservation을 만들 때 실제 installed manifest가 requested
baseline과 다르면 `PCV_MANUAL_ADMIN_BASELINE_VERSION_MISMATCH`로 실패한다.

기존 `New-PcvManualAdminRebaselineReadiness.ps1`은 실제 campaign 전에 reservation을 요구한다.
Campaign id, baseline, target, host fingerprint, unexpired 상태가 모두 일치해야 runner command를
연다. Target 설치 뒤 reservation은 consumed가 되며 동일 descriptor의 재사용을 금지한다.

Dedicated host 또는 checkpoint의 실제 생성과 복원은 기존 approved manual-admin runner가
소유한다. Reservation layer는 그 자원을 실수로 선행 업그레이드하거나 다른 campaign에서
재사용하는 것을 막는다.

## 6. 승인 후 PR 감시·병합

Native GitHub auto-merge 대신
`packaging/windows-desktop-node/tools/Wait-PcvPullRequestGreenAndMerge.ps1`을 운영자
workstation에서 실행한다.

명령은 다음 승인 정보를 고정한다.

- repository와 PR number
- approved head SHA
- expected base branch `main`
- required workflow names: `Development Gates`, `Public Boundary Contract`
- timeout, poll interval, merge method

병합 전 매 poll에서 다음을 확인한다.

1. 인증된 GitHub repository가 expected repository와 일치한다.
2. PR이 open, non-draft이고 base가 main이다.
3. 현재 head SHA가 approved SHA와 정확히 일치한다.
4. 두 named workflow가 approved SHA에서 완료됐고 conclusion이 success다.
5. mergeability가 conflict/blocked가 아니다.
6. 현재 repository의 PR이며 fork/untrusted head가 아니다.

Head SHA가 바뀌거나 workflow가 누락/실패하면 병합하지 않고 종료한다. Timeout도 실패로
종료한다. 성공 조건을 만족하면 `gh pr merge`를 한 번 호출하고 merged SHA/URL을 structured
summary에 기록한다.

`-PlanOnly`와 injected `gh` command runner를 제공해 테스트에서 실제 GitHub mutation을 하지
않는다. 도구는 장기 token 저장, GitHub Actions write permission, `pull_request_target`, 무인
label bot을 추가하지 않는다. 실행 자체가 운영자의 한 번 승인이다.

## 오류 처리

- Fast scope를 결정할 수 없으면 suite를 생략하지 않고 Full로 승격한다.
- Test dependency seam이 누락되면 production default를 사용한다.
- Evidence JSON/schema/reference/marker 오류가 하나라도 있으면 문서 쓰기를 시작하지 않는다.
- Generator는 임시 파일에 전체 결과를 만든 뒤 validation이 끝난 경우에만 대상 block을
  교체한다.
- Reservation mismatch/expiry/consumed 상태는 host mutation runner 실행 전에 실패한다.
- Merge watcher는 approved head SHA 변경을 재승인 없이 받아들이지 않는다.
- GitHub API rate limit/network 오류는 제한된 backoff 후 실패하며 merge 성공으로 간주하지
  않는다.

## 테스트 전략

모든 동작 변경은 test-first로 구현한다.

1. Verification lane path selection과 unknown-path Full fallback red test
2. Batch fake clock/process timeout/retry red test
3. Installer in-process plan/signing/redaction red test
4. Current evidence schema와 stale generated block red test
5. S/M/L lane compatibility red test
6. Baseline mismatch/expiry/consumed red test
7. Merge watcher SHA drift, missing workflow, failed workflow, successful merge red test

실제 `pwsh` integration, 전체 Pester, .NET, Web parity와 `git diff --check`를 green verification에
포함한다. Performance comparison은 동일 host에서 변경 전 기준과 동일 명령으로 측정한다.

## 수용 기준

1. Fast lane은 선택 이유와 skip 이유가 있는 JSON summary를 남긴다.
2. Unknown scope와 tier mismatch가 검증 축소로 이어지지 않는다.
3. Full lane은 현재 591 .NET, Web parity, 385 packaging Pester, 94 installer/Web Pester 범위를
   유지한다.
4. 같은 host에서 두 Pester 묶음 합계 시간이 기준보다 최소 30% 감소한다.
5. Current operational 사실은 `current-evidence.json` 한 곳에서 변경되고 여섯 current block은
   generator `-Check`를 통과한다.
6. Historical evidence 본문은 generator가 수정하지 않는다.
7. S 변경은 별도 full design/plan 없이 Fast lane으로 완료할 수 있다.
8. L 변경은 기존 full design/plan/release/host evidence 요구를 유지한다.
9. Baseline mismatch는 host mutation 전에 stable code로 차단된다.
10. Merge watcher는 approved head SHA와 두 workflow success가 모두 일치할 때만 병합한다.
11. TUI source/package/smoke/current surface가 재도입되지 않는다.
12. 설치본 0.42.64와 current operational anchor는 이 tooling slice로 변경되지 않는다.

## 구현 경계

### 포함

- fast/full/release development verification orchestrator
- Batch Supervisor process/clock test seam
- Installer build logic module화와 thin wrapper
- Current evidence JSON/schema/generator/current block migration
- S/M/L change classification policy
- Manual-admin baseline reservation descriptor와 readiness guard
- 승인 후 PR workflow watcher/merge 도구
- focused/full tests와 개발 문서

### 제외

- 제품 runtime/API/CLI/Web behavior 변경
- TUI 복원
- 실제 N-1 VM/host 생성 또는 checkpoint mutation
- 새 admin-smoke version 발행
- MSI install/update/rollback 또는 full host mutation 실행
- public trusted signing, winget, 외부 stable publication
- 보호 규칙 없는 GitHub Actions custom merge bot
- GitHub 요금제 변경 또는 repository 공개 전환

## Rollout

1. Verification scope selector와 JSON summary를 도입한다.
2. Batch Supervisor 느린 unit test를 fake dependency로 전환한다.
3. Installer build module과 in-process tests를 도입한다.
4. Current evidence JSON/schema/generator를 추가하고 0.42.64 current block을 migration한다.
5. Change tier 정책과 lane guard를 연결한다.
6. Baseline reservation을 manual-admin readiness 앞에 연결한다.
7. Merge watcher를 fake GitHub runner로 검증한 뒤 현재 PR에서 실제 감시·병합한다.
8. 전체 local/CI gate와 performance 기준을 확인한다.

## Rollback

각 component는 독립 commit으로 나눈다. 문제가 생기면 해당 commit을 revert한다. Evidence
generator rollback 시 marker block은 마지막 생성된 정적 Markdown으로 남아 문서 가독성을
유지한다. Verification orchestrator rollback은 기존 직접 명령과 GitHub workflow를 그대로
사용한다. Baseline reservation과 merge watcher rollback은 기존 수동 readiness/merge 절차로
복귀한다. 어떤 rollback도 설치본, Hyper-V, service, firewall, Event Log, trust store 또는
Credential Manager 상태 변경을 요구하지 않는다.
