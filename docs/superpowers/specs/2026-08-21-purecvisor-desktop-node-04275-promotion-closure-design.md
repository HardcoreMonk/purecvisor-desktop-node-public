# PureCVisor Desktop Node 0.42.75 승격 closure 설계

- Design-ID: `purecvisor-desktop-node-04275-promotion-closure-v1`
- 작성일: `2026-08-21`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: pcv-04275-promotion-closure-20260821`
- operational current: `0.42.74-admin-smoke`
- candidate: `0.42.75-admin-smoke`
- candidate product source commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`
- campaign tooling provenance: candidate 이후 별도 commit/file SHA-256으로 기록
- host mutation: `true`, explicit administrator opt-in only
- public trusted signing: `false`
- external stable publication: `false`

이 설계는 `vm.save`가 Hyper-V Saved 상태로 들어가지 못한 0.42.74 actual-VM 결함을
0.42.75 설치본에서 닫고, 이미 PASS한 package/full-gate 증거와 아직 누락된 actual-VM 및
manual-admin package-pair를 하나의 승격 chain으로 연결한다. 호스트 변경은 UAC로 승인한
외부 관리자 PowerShell에서 사용자가 명시적으로 시작한 helper에서만 실행한다.

## 1. 현재 상태와 문제

0.42.74 SERVICE_PLAN P0 actual-VM 검증에서 attach overwrite, checkpoint restore
reconcile, managed import는 PASS했다. `vm.save`는 WMI `RequestStateChange`에
RequestedState `32769`를 전달해 ReturnValue `32775`로 실패했고, `vm.resume-saved`는
실행되지 못했다. 같은 호스트에서 CIM RequestedState `6`은 Hyper-V `Saved`로 성공했다.

commit `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`는 `vm.save` 요청값을 CIM Offline
`6`으로 바꾸고 EnabledState `6`과 `32769`를 모두 제품 상태 `saved`로 매핑한다.

2026-08-21 사전 조사에서 다음 0.42.75 증거가 이미 생성돼 있음을 확인했다.

| 항목 | 상태 | 값 |
| --- | --- | --- |
| clean package | PASS, 아직 current 아님 | `artifacts/admin-smoke-package-20260821-04275` |
| clean MSI SHA-256 | 고정 | `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6` |
| clean payload aggregate SHA-256 | 고정 | `3c33a35b21eb9cdd2b24156cc98afe2268f82f3ca32c7dd6a03882a262afdd2c` |
| full admin host mutation | PASS | `full-admin-host-mutation-gate-20260821-04275` |
| operational MSI SHA-256 | 고정 | `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b` |
| operational payload aggregate SHA-256 | 고정 | `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7` |
| installed current-card | PASS, `not-promoted-awaiting-functional-and-manual-admin` | `artifacts/installed-operator-surface-current-card-20260821-04275` |
| actual-VM functional | not run | 새 evidence 필요 |
| SERVICE_PLAN P0 actual-VM | not run | 새 evidence 필요 |
| manual-admin package-pair | not run | `0.42.74-admin-smoke -> 0.42.75-admin-smoke` 필요 |

full-gate는 실제 host mutation을 수행했고 두 child step이 모두 첫 attempt에 exit `0`이다.
당시와 후속 readback 모두 설치 manifest는 0.42.75, 서비스는 Running/Automatic이며 잔여
테스트 VM은 없었다. 따라서 같은 full-gate를 관성적으로 반복하지 않는다.

이 campaign의 일반 controller 프로세스는 `.git` write와 Hyper-V access가 제한되며,
controller에서 `RunAs`로 시작한 자식도 `elevated=false`로 확인됐다. 따라서 controller가
UAC 승격을 자동화하지 않는다. 사용자가 별도로 연 관리자 PowerShell에서 고정된 worker
명령을 실행하는 행위 자체를 administrator opt-in으로 삼는다.

## 2. 목표와 비목표

### 목표

- 0.42.75 설치본에서 실제 VM `Running -> Saved -> Running` 왕복을 제품 API/PCVCLI
  경로로 PASS하고 package-pair final update 후 clean target 설치본에서도 다시 PASS한다.
- SERVICE_PLAN P0 네 slice와 functional carry-forward를 실제 VM에서 PASS한다.
- `0.42.74 -> 0.42.75` manual-admin package-pair의 여섯 bucket을 모두 닫고 주 호스트의
  최종 설치본을 0.42.75로 둔다.
- final current-card를 다시 캡처하고 package/full-gate/actual-VM/manual-admin/current-card
  증거가 모두 연결됐을 때만 canonical operational evidence를 0.42.75로 전환한다.
- 이전 campaign-local P0 smoke를 파라미터화된 정식 repository runner로 승격한다.

### 비목표

- 이미 무결한 0.42.75 full admin host mutation gate의 무조건 재실행
- 호스트 재부팅 또는 기존 사용자 VM 변경
- P1 full clone 또는 다른 SERVICE_PLAN 신규 기능 개발
- public trusted signing, trusted timestamp, external stable publication, winget publication
- TUI 복원; active operator surface는 Web Console과 PCVCLI로 유지
- 전역 Git 설정 변경 또는 public signing material 부재 우회

## 3. 선택한 접근

결함 우선 검증을 선택한다. 새 Saved runner를 먼저 actual VM에 실행하고, 이 핵심 경로가
PASS할 때만 전체 P0, functional carry-forward, package-pair로 진행한다.

정규 package-pair부터 시작하면 핵심 수정이 실패한 경우 clean-host Windows Update, Burn,
MSIX 같은 고비용 검증이 낭비된다. full-gate 우선 접근은 이미 PASS한 gate를 반복하고
baseline churn을 늘리므로 제외한다.

## 4. Controller / elevated worker 구조

일반 세션은 controller다. controller는 파일과 기존 evidence를 읽고 immutable campaign
manifest와 정확한 launch command를 만든다. 사용자는 외부에서 관리자 PowerShell을 열고
그 command로 worker를 한 번 실행한다. controller는 관리자 토큰을 획득하거나 전달하지
않으며 `.git` 또는 repository ACL도 변경하지 않는다.

worker는 다음 계약을 지킨다.

- 시작 즉시 관리자 토큰, manifest SHA-256, campaign tooling file SHA-256을 검증한다.
- 단계별 로그, JSON summary, 종료 코드를 고유 artifact root에 기록한다.
- 현재 단계가 PASS하지 않으면 후속 단계를 시작하지 않는다.
- 상태 파일을 atomic replace해 일반 세션이 진행 상황을 읽을 수 있게 한다.
- secret이나 bearer token을 argv, 로그, summary에 기록하지 않는다.
- host reboot를 요청하지 않는다. clean-host throwaway guest 내부 재부팅만 기존 runner 계약대로
  허용한다.

외부 관리자 PowerShell은 사용자가 승인과 실행 상태를 확인할 수 있게 가시적으로 유지한다.
controller는 worker 로그를 polling하되 변경 명령을 중복 실행하지 않는다.

## 5. 정식 P0 actual-VM runner 계약

이전 artifact-local `Invoke-PcvServicePlanP0ActualVmSmoke.ps1`의 검증 내용을
`packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1`로
승격한다. campaign artifact의 스크립트를 그대로 실행하거나 복사하지 않는다.

### 5.1 입력

- mandatory `Version`
- `ArtifactRoot`, `ProductRoot`, `IsoPath`, `VmRoot`
- `ManagedVm`, `ForeignVm`, `CheckpointName`
- `Mode`: `SavedOnly` 또는 `Full`
- job/command timeout
- `DryRun`

`Version`은 installed product manifest와 같아야 한다. VM 이름은 campaign ID에서 유도한
고유 이름이어야 하고 실행 전에 부재를 확인한다. `ArtifactRoot`와 `VmRoot`는 resolve한
절대 경로를 기록한다.

### 5.2 SavedOnly

SavedOnly는 다음 한 경로만 검증한다.

1. 설치본 PCVCLI로 disposable managed VM을 생성한다.
2. VM을 Running으로 만든다.
3. `vm save` queued job이 `succeeded`인지 확인한다.
4. Hyper-V `Get-VM`이 `Saved`, 제품 `vm get`이 `saved`인지 확인한다.
5. `Paused`가 아님을 확인한다.
6. `vm resume-saved` job이 `succeeded`이고 최종 Hyper-V 상태가 `Running`인지 확인한다.
7. exact VM ID와 전용 root를 cleanup한다.

이 단계는 `vm.save` 결함의 fail-fast gate다.

### 5.3 Full

Full은 SERVICE_PLAN P0 네 slice를 실행한다.

- media attach: attach job 성공과 DVD HostResource/ISO readback 일치
- checkpoint restore: restore job 성공과 같은 checkpoint의 `is_current=true` count `1`
- Hyper-V Saved: SavedOnly와 같은 save/resume-saved 왕복
- managed import: unmanaged delete 거부, manage marker 기록, 이후 managed delete 성공

0.42.74에서 관측된 eject prelude 실패는 attach overwrite 계약 자체를 깨지 않으면 note로
남긴다. 각 slice verdict와 overall verdict를 분리한다.

### 5.4 summary와 cleanup

summary는 최소한 version, mode, installed CLI hash, VM 이름과 ID, slice verdict, 각 queued
job ID/status, Hyper-V 및 제품 state readback, cleanup 결과, host mutation 여부, secret 관측,
시작/종료 시각을 포함한다.

runner는 생성 직후 VM ID와 root를 기록한다. `finally` cleanup은 기록한 정확한 VM ID와
검증된 root 하위 경로만 대상으로 한다. wildcard는 잔여 VM 관찰에만 사용하며 삭제 대상
선정에 사용하지 않는다. 제품 delete가 실패하면 같은 기록 ID에 한해서만 native Hyper-V
stop/remove fallback을 허용한다. 이름은 같지만 ID가 다르면 삭제하지 않고 blocker로 남긴다.

cleanup 실패, summary write 실패, service loss 또는 state mismatch는 overall FAIL이다.

## 6. 실행 순서

### Stage 0 — code-level runner 준비

1. Pester failing tests를 먼저 추가한다.
2. 정식 P0 runner를 구현한다.
3. `SavedOnly`, `Full`, `DryRun`, exact cleanup, fail-stop summary 계약을 code-level로 PASS한다.
4. runner file SHA-256과 tooling commit을 manifest 입력으로 고정한다.
5. 이 단계에서는 actual VM이나 host mutation을 수행하지 않는다.

controller의 `.git` write가 제한된 동안 commit은 사용자가 외부 관리자 PowerShell에서 정확한
staged path를 검토한 후 실행한다. ACL 변경이나 global `safe.directory` 변경으로 우회하지
않는다.

### Stage 1 — immutable preflight

controller는 다음을 모두 확인한다.

- clean package metadata의 product source commit이 candidate commit과 일치
- clean MSI/payload와 operational MSI/payload aggregate hash가 이 문서의 고정값과 일치;
  docs/tooling 후속 commit 때문에 repository HEAD가 candidate commit과 같을 필요는 없음
- installed Host/CLI hash가 target artifact에서 계산한 값과 일치하고 manifest에 기록됨
- worker script가 committed campaign tooling revision과 file SHA-256에 일치
- installed manifest가 0.42.75이고 서비스가 Running
- ISO가 존재하고 읽을 수 있음
- elevated worker에서 Hyper-V 접근 가능
- 선택한 VM 이름과 VM root가 미사용
- pre-existing VM의 name/ID snapshot을 기록하고, 기존 runner가 broad prefix `0`을 요구하는
  경우 해당 prefix VM이 이미 있으면 명시적 blocker로 중단
- 동일 campaign helper가 실행 중이지 않음
- 기존 04275 full-gate/current-card summary가 PASS이고 artifact가 훼손되지 않음

하나라도 실패하면 worker mutation 전에 중단한다.

### Stage 2 — SavedOnly actual-VM fail-fast

정식 runner를 `Mode=SavedOnly`로 실행한다. save/resume 왕복과 cleanup이 모두 PASS해야
Stage 3으로 진행한다.

### Stage 3 — Full P0와 functional carry-forward

정식 runner를 새로운 VM 이름과 artifact root에서 `Mode=Full`로 실행한다. 성공하면 기존
`Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`를 실행해 다음을 확인한다.

- network QoS Kbps -> bps 변환
- disk shrink가 계약된 오류로 거부됨
- disk expansion이 성공하고 실제 VHD size가 증가함
- `pcv-p0-*`, `pcv-fc-cf-*`, `pcv-spike-*` 잔여 테스트 VM `0`

각 runner의 cleanup이 PASS하지 않으면 package-pair로 진행하지 않는다.

### Stage 4 — manual-admin package-pair

baseline은 0.42.74 clean package, target은 0.42.75 clean package로 고정한다. baseline과
target hash 또는 installed/runner baseline이 섞이면 실행하지 않는다.

- baseline clean MSI SHA-256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
- target clean MSI SHA-256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`
- update ZIP, Burn bundle, MSIX v2는 실행 전에 생성·검증하고 각각의 SHA-256을 campaign
  manifest와 최종 evidence에 고정한다.

필수 bucket은 다음 여섯 개다.

1. manual-admin rebaseline readiness
2. installed update / rollback / final update
3. Windows Update 포함 dedicated clean-host install / update / rollback
4. Burn install / repair / remove와 target restore
5. MSIX baseline install / target update / remove
6. installed runtime ops summary

descriptor는 `runner_count=6`, `missing_count=0`, `not_pass_count=0`,
`overall_status=pass`여야 한다. 주 호스트 update/rollback runner는 마지막 final update로
0.42.75를 복원해야 한다. clean-host guest와 differencing VHD는 성공/실패 모두 cleanup을
시도하고 잔여물이 있으면 FAIL이다.

### Stage 5 — final clean-target SavedOnly parity

package-pair final update 후 installed Host/CLI hash가 clean target artifact와 일치하는지 먼저
확인한다. 새 VM 이름과 artifact root를 사용해 `Mode=SavedOnly`를 다시 실행한다. 이 단계는
full-gate build에서 얻은 actual-VM PASS가 최종 clean target MSI에도 그대로 적용됨을 증명한다.
Saved 왕복 또는 cleanup이 실패하면 final current-card와 promotion을 실행하지 않는다.

### Stage 6 — final installed current-card

package-pair final update 후 read-only current-card를 충돌 없는 `...04275-r2` artifact root에
다시 캡처한다. 기존 01:19 current-card artifact는 덮어쓰지 않는다.

필수 조건은 다음과 같다.

- installed manifest `0.42.75-admin-smoke`
- CLI 3/3 exit `0`, JSON 정상
- Web root/config 2/2 HTTP `200`
- service Running/Automatic, LocalSystem
- Credential Manager target 사용, raw/protected token argv 없음
- TUI absent
- 잔여 campaign VM `0`
- source commit과 clean/operational hash가 고정값과 일치

### Stage 7 — evidence promotion

모든 원본 summary가 PASS한 뒤에만 문서와 canonical ledger를 변경한다. 어느 단계든 FAIL이면
0.42.74 operational current를 유지한다.

이 설계는 local repository 문서 변경과 commit까지만 승인한다. remote push/PR과 그에 따른
public-boundary main-push CI evidence 작성은 별도 사용자 승인이 필요한 후속 단계다. 그 CI는
internal repository boundary 검증이며 public release 또는 stable publication claim이 아니다.

## 7. 실패와 복구 의미론

- 실패 후 다음 단계를 자동 시작하지 않는다.
- runner가 명시적으로 소유한 rollback/restore만 실행한다. 실제 installed version을 모르는
  상태에서 임의 MSI를 재설치하지 않는다.
- package-pair 실패 시 installed manifest, service state, boot time, 마지막 성공 단계와 로그를
  기록하고 중단한다.
- 호스트 boot time이 바뀌면 예상치 못한 reboot blocker다.
- cleanup 대상의 ID/path가 manifest와 다르면 삭제하지 않는다.
- cleanup 판정은 preflight VM ID snapshot과 비교해 campaign이 새로 남긴 ID가 `0`인지
  확인한다. 기존 VM은 잔여물로 세지도, cleanup 대상으로 삼지도 않는다.
- cleanup 실패는 별도 blocker가 아니라 campaign 전체 FAIL이다.
- 기존 사용자 VM, 기존 VM root, 기존 04275 artifact를 변경하지 않는다.
- controller의 `.git`/Hyper-V 제한을 ACL 변경, global Git 설정 또는 in-app UAC 재시도로
  우회하지 않는다. 필요한 commit과 worker 실행은 외부 관리자 PowerShell에서 사용자가
  정확한 파일/manifest를 확인한 뒤 수행한다.
- full-gate는 hash/provenance 불일치, artifact 훼손, 또는 최종 target 복원 실패를 분석한 뒤
  별도 승인된 recovery가 필요한 경우에만 재실행한다.

## 8. 테스트와 acceptance

### Code-level

Pester는 다음을 검증한다.

- `Version` 필수 및 installed manifest mismatch fail-fast
- `SavedOnly`와 `Full`의 단계 선택
- `DryRun`에서 host mutation 없음
- Saved job/Hyper-V/CLI readback 세 조건 중 하나라도 다르면 FAIL
- resume-saved 성공과 최종 Running 요구
- exact VM ID/path cleanup, wildcard delete 부재
- cleanup 실패가 overall FAIL이고 후속 단계가 차단됨
- summary schema, atomic write, secret non-observation

### Actual host

- SavedOnly overall PASS, cleanup PASS
- Full P0 네 slice PASS, cleanup PASS
- functional carry-forward 세 항목 PASS, cleanup PASS
- package-pair six buckets PASS와 descriptor counts `6/0/0`
- final clean-target SavedOnly PASS, cleanup PASS
- final current-card PASS
- host reboot 없음

### Repository verification

- 새 runner Pester
- packaging evidence/document contract tests
- relevant installer/package tests
- `git diff --check`
- generated current evidence sync 검사
- evidence summary와 MSI/payload SHA-256 재계산

완료 주장은 위 명령의 최신 출력과 실제 summary를 확인한 뒤에만 한다.

## 9. 문서와 ledger 산출물

성공 시 최소한 다음 evidence를 생성하거나 갱신한다.

- 0.42.75 package evidence의 후속 chain 상태
- 0.42.75 full admin host mutation evidence
- 0.42.75 SERVICE_PLAN P0 actual-VM evidence
- 0.42.75 functional correctness actual-host evidence
- `0.42.74 -> 0.42.75` manual-admin campaign evidence와 closed descriptor
- 0.42.75 final installed operator surface current-card evidence
- `docs/ga-ready/current-evidence.json`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- `docs/ga-ready/EVIDENCE_INDEX.md`
- `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`
- generated current evidence block가 있는 `AGENTS.md`

기존 0.42.74 문서와 artifact는 predecessor로 보존한다. 현재 untracked 0.42.75 package
evidence는 삭제하거나 재작성하지 않고 원문을 보존한 채 실제 결과를 연결한다.

## 10. 승격 판정

0.42.75는 다음 논리식이 참일 때만 operational current다.

```text
clean package PASS
AND existing full admin host mutation PASS + integrity match
AND SavedOnly actual-VM PASS
AND Full P0 actual-VM PASS
AND functional carry-forward PASS
AND manual-admin six-bucket descriptor PASS
AND final clean-target SavedOnly actual-VM PASS
AND final installed current-card PASS
AND cleanup PASS
```

이 closure는 internal `AllowUnsignedDev` 운영 evidence다. public trusted signing 또는 외부
stable publication evidence가 아니다.
