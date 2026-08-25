# PureCVisor Desktop Node Job Store Durability Decision

- 결정일: 2026-08-02
- 상태: 결정 완료 / Wave 2A persistence·restart·single-writer·`W0-FI-02`·`W0-FI-04` `code_complete` / legacy 설치본 checkpoint `PASS` (`2026-08-03` post-reboot, operational promotion 아님)
- 결정 기록 변경 등급: `M / Full`
- 구현 변경 등급: `L / Release` (persistence/restart 의미 변경)
- 구현 commit: `f3d5d7be4bb24b80fc2fa11be1cee93be13b4362`; single-file Event Log path
  follow-up와 최종 package provenance commit은 `3c16f78568cfb54a0cbe586449a540df3596bcf1`
- 기준 제품: `0.42.65-admin-smoke`
- 범위: `DesktopNodeJobRuntime`의 JSON snapshot create/start/cancel/complete commit, single-writer와 restart 의미
- source/code-level 검증 host mutation 수행: `false`
- legacy 설치 checkpoint host mutation 수행: `true` (2026-08-02 MSI 설치 시도와 controlled
  diagnostic service 비교, 2026-08-03 MSI/service 설치 수행; Hyper-V/VM mutation은 `false`)
- public trusted signing 변경: `false`
- external stable publication 변경: `false`

## 1. 결정 요약

Wave 2A는 기존 JSON job store와 schema v1/v2 write shape를 유지한다. SQLite, 별도 storage
process와 schema v3는 이번 rollout에 도입하지 않는다. Create mutation은 next-state를 별도
snapshot으로 계산하고 물리 store가 `Committed`를 반환한 뒤에만 같은 process의 memory job,
queue와 HTTP success를 공개한다.

물리 store는 `Committed`, `NotCommitted`, `Indeterminate`를 구분한다. `NotCommitted`는 이전
primary를 보존하고 candidate를 공개하지 않는다. `Indeterminate`는 `jobs.json.commit-pending`
guard를 보존하고 같은 process의 mutation/dispatch와 restart load를 fail-closed한다. 현재
marker-aware runtime이 candidate 또는 previous identity를 확정한 뒤에만 guard를 제거한다.

저장 실패를 이유로 이전 backend에 요청을 재실행하거나 stale snapshot/backup/temp를 자동
승격하지 않는다. 이 protocol은 job-store identity만 다루며 Hyper-V side effect의 exactly-once를
증명하지 않는다.

## 2. Runtime publish protocol

1. `DesktopNodeJobRuntime.stateSync` 안에서 현재 memory state를 바꾸지 않고 candidate jobs/queue
   snapshot을 만든다.
2. redaction과 schema v1/v2 shape를 적용해 candidate를 직렬화한다.
3. store write가 `Committed`를 반환한 경우에만 live jobs/queue reference를 바꾸고 HTTP success를
   허용한다.
4. `NotCommitted`는 redacted `PCV_JOB_STORE_SAVE_FAILED`/HTTP 503으로 거절하며 candidate job을
   worker에 노출하지 않는다.
5. `Indeterminate`는 HTTP 503을 반환하고 `loadBlock`을 설정한다. 이후 job mutation route는
   structured block을 반환하고 worker dispatch는 0이어야 한다.

이 순서는 create/start/cancel/complete에 적용됐다. Running cancel은 취소 요청을 먼저 durable
save하고 live state를 publish한 뒤 `stateSync` 밖에서 provider cancellation을 신호한다. 이미 취소
요청된 running job의 반복 요청은 추가 save/signal 없이 idempotent하게 반환한다. Provider signal
실패는 durable request를 보존한 채 `PCV_JOB_CANCEL_SIGNAL_FAILED`로 보고한다.

Completion persistence 실패는 provider side effect가 이미 끝났을 가능성이 있으므로 terminal memory
state를 공개하거나 operation을 자동 재실행하지 않고 같은 runtime의 이후 mutation을 차단한다.
Persisted `running`은 restart 때 `PCV_JOB_INTERRUPTED`, `retryable=false`와 operator reconciliation
action을 가진 failed candidate로 바꾸며, 그 recovery snapshot이 durable commit된 뒤에만 live state에
공개한다.

Physical writer는 absolute canonical path에서 파생한 named mutex로 write transaction을 직렬화하고,
`Exists()`/`ReadSnapshot()`에서 기억한 primary identity와 lease 획득 후 current primary identity를
비교하는 loaded-base CAS를 수행한다. 먼저 같은 base를 읽은 stale current writer는 typed
`NotCommitted`로 거절되며 primary를 덮어쓰지 않는다. 이것은 process lifetime lease나 mixed-version
writer 지원이 아니며 자세한 경계는 ADR-0013이 소유한다.

Windows lease identity는 local fixed volume GUID와 volume-relative long path로 계산한다. UNC/device,
network/non-fixed, SUBST, DOS 8.3, ADS, existing reparse point와 hard-linked primary는 fail-closed한다.
Host JSONL과 rotation 파일은 job primary/pending/temp와 물리 identity·ancestor·hardlink가 겹치면
listener bind 전에 거절된다. Hostile local-admin namespace TOCTOU와 mixed-version writer는 지원하지
않는다.

## 3. 물리 파일 commit protocol

1. final file과 같은 directory에 `jobs.json.tmp.<GUID-N>` candidate를 `CreateNew`/exclusive share로
   만들고 UTF-8 bytes를 쓴 뒤 `FileStream.Flush(flushToDisk: true)`를 호출한다.
2. candidate SHA-256/length와 previous exists/SHA-256/length를 version 1 marker JSON으로 만든다.
3. marker는 `jobs.json.commit-pending.tmp.<GUID-N>`에 exclusive write와 `Flush(true)`를 수행한 뒤
   fixed `jobs.json.commit-pending`으로 같은 directory에서 rename한다.
4. fixed marker가 publish된 뒤에만 candidate를 `jobs.json`으로 replace/move한다.
5. primary exact bytes를 candidate와 비교한다. candidate match는 `Committed`, previous identity
   match는 `NotCommitted`, 둘 다 아니거나 primary를 읽을 수 없으면 `Indeterminate`다.
6. candidate/previous outcome이 확정되고 fixed marker 삭제까지 성공한 경우에만 결과를 caller에
   반환한다. marker 삭제 실패도 `Indeterminate`로 차단한다.
7. startup은 fixed marker가 있으면 primary length/SHA-256을 두 identity와 비교한다. candidate 또는
   previous가 정확히 일치하면 marker를 제거하고 해당 primary를 load한다. marker가 malformed,
   unreadable 또는 identity mismatch면 primary/temp를 바꾸지 않고 blocked state로 시작한다.

고정 legacy `jobs.json.tmp`와 unique candidate/marker temp는 authoritative가 아니다. startup은 이를
primary나 fixed marker로 승격하지 않는다. 실패한 unique temp cleanup은 best-effort이며 명시적
RemoveData만 exact legacy temp와 GUID-N owner pattern을 정리한다. primary/marker access failure는
missing으로 간주하지 않으며 `PCV_JOB_STORE_LOAD_FAILED` 또는 pending-commit block으로 유지한다.

`PCV_JOB_STORE_LOAD_FAILED`는 authoritative primary/marker를 읽지 못한 startup block을 기존
save-indeterminate 문구와 구분하기 위한 additive diagnostic code다. Job route의 기존 HTTP 409와
error envelope shape는 유지하며 retryable=false와 Operations Guide action을 제공한다.

.NET/Windows의 file flush와 same-volume rename 경계까지만 주장한다. directory fsync, storage
controller cache, 갑작스러운 전원 손실과 외부 side effect를 포함한 power-loss/exactly-once 보장은
이번 code-level evidence 범위가 아니다.

## 4. 배포·migration·제거 companion guard

- Product Update는 service stop+Stopped 확인 뒤 marker absence를 검사하고 나서만 product-root
  backup/copy를 시작한다.
- Update start/health/copy 실패의 모든 자동 restore와 명시적 Rollback은 restore 직전에 다시
  service stop+wait를 수행하고 marker를 재검사한다.
- `DesktopNode.Host.exe service-action job-store-migration-apply`는 stopped writer를 확인한 뒤 marker가
  있거나 검사할 수 없으면 backup/rewrite 전에 차단한다.
- preserve-data `RemoveInstalled`, `Uninstall`과 native host remove는 current marker-aware binary를
  제거하기 전에 marker absence를 확인한다. marker가 있으면 service는 stopped 상태로 남을 수
  있으므로 같은 current binary를 다시 시작해 reconciliation한 뒤 재시도한다.
- 명시적 `RemoveData=true`만 primary, fixed marker, legacy temp와 GUID-owned orphan temp를 삭제한다.
  near-miss 이름은 allowlist 밖이므로 보존한다.

0.42.65 같은 marker-unaware 구 binary는 fixed marker를 해석하지 못한다. marker가 남아 있는 동안
구 binary rollback/start를 허용하지 않으며 marker 단순 삭제를 rollback 절차로 사용하지 않는다.

## 5. Backup, revision과 checksum

- 정상 요청마다 previous snapshot을 자동 복원용 backup으로 만들지 않는다. stale queued job의 무단
  재실행 위험이 durable-write 실패보다 크다.
- 배포/rollback backup은 service와 writer가 멈추고 pending marker가 없는 경계에서만 만든다.
- schema v1/v2 snapshot에 revision/checksum 필드를 추가하지 않았다. 파일 밖 marker identity와
  SHA-256이 create physical outcome을 판정한다.
- current writer 사이의 transaction lease와 loaded-base CAS는 ADR-0013으로 채택했다. schema v1/v2
  JSON에는 revision 필드를 추가하지 않았다.
- process lifetime lease, mixed-version writer 또는 schema v3가 필요하면 별도 ADR, dual-read migration과
  downgrade/rollback 승인을 요구한다.

## 6. Startup과 운영 recovery

- v1/v2 object root는 계속 읽고 unsupported future version은 quarantine·write·migration 없이
  structured blocked state로 남긴다.
- fixed marker가 유효하고 primary가 candidate/previous identity와 일치하면 current runtime이
  자동으로 marker를 정리한다.
- primary/marker access를 복구하고 current runtime을 restart해도 block이 남으면 Operations Guide의
  pending-commit recovery 절차를 따른다. marker/primary 편집·삭제, orphan temp promote와 blind retry는
  금지한다.
- malformed/root-non-object, job ID/status/attempt/error 구조, queue reference/중복과 상태 조합은 typed
  semantic validator가 검사한다. Corrupt snapshot은 quarantine·rewrite 없이 blocked state로 남기고,
  unsupported future version은 별도 typed 결과로 fail-closed한다.
- fixed marker가 identity match하더라도 primary semantic validation이 성공해야 guard를 제거한다.
  이 검증으로 Hyper-V side-effect 실행 여부를 추론하지 않는다.
- recovery/save/cancel signal 상태는 redacted runtime observation의 최근 32개 항목으로 유지하고 Host
  bounded/rotating JSONL과 owned-source Windows Event Log sink, diagnostic bundle과 ops-summary
  `job_store`에 노출한다. Store path, raw exception, parameters는 기록하지 않으며 sink 실패가 runtime
  결과를 바꾸지 않는다. Job store block은 mutation/dispatch만 차단하고 ops-summary의 read-only
  `host.status`/`vm.list` 관찰은 유지한다.
- acknowledged enqueue 뒤 HTTP 응답 전달만 실패한 경우 correlation/job 조회로 복구한다. 응답
  미수신을 job 미생성 또는 미실행의 증거로 사용하지 않는다.

## 7. Compatibility와 contract 경계

writer는 schema v1/v2 root와 기존 job row shape를 유지한다. Pinned
`0.42.65-admin-smoke` Host binary가 current writer의 v1/v2 terminal store와 2-entry FIFO queue
store를 original/backup-restored 각각 실제로 읽었다. 2개 schema × terminal/queue × initial/restored
8개 pass 모두 input/output SHA-256이 동일했다. Queue probe는 첫 FIFO job부터 save-failed가 발생하며
provider dispatch 0을 확인한다. Frozen reader 실행 중 native/Hyper-V/service/admin/host mutation은
없었다.

`IDesktopNodeJobStore.WriteSnapshot`의 `void`→typed result 변경은 source/binary contract 변경이다.
이 interface는 현재 저장소 내부 assembly/test composition seam이며 external stable public API로
게시되지 않았다. 외부 implementation compatibility는 주장하지 않는다.

## 8. 검증 계약과 비주장

- Baseline RED: fixed `jobs.json.tmp` path를 directory가 점유하면 기존 writer가
  `UnauthorizedAccessException`으로 실패했다.
- GREEN: unique candidate/marker temp, flush 순서, pre/post-replace outcome, marker publication 실패,
  primary access 실패, invalid marker, restart reconciliation, orphan non-promotion과 v1/v2 shape를
  real filesystem test로 검증한다.
- API companion은 indeterminate create가 HTTP 503/redacted diagnostics를 반환하고 이후 job route와
  worker/native dispatch를 차단하는지 확인한다.
- Host/product companion은 migration/update/rollback/preserve-data remove를 fail-closed하고 explicit
  RemoveData allowlist만 정리하는지 확인한다.
- Wave 2A completion source 검증은 최종 Release/L gate와 legacy checkpoint evidence가 소유한다.
  Frozen 0.42.65 reader Pester는 5/5, actual binary pass는 8/8이다. Frozen 실행 summary는
  `artifacts/job-store-04265-reader-compatibility-20260802-wave2a-current-writer-final5/summary.json`이다.
- 2026-08-03 post-reboot legacy 설치본 checkpoint는
  `docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`가
  소유한다. 동일 `0.42.66-admin-smoke` MSI 설치 exit 0, service `Running`/`Auto`/`LocalSystem`,
  Web/API/PCVCLI provider-free smoke와 ProgramData store hash 불변을 확인했다. 이는 operational
  full-admin 승격이 아니며 `0.42.65-admin-smoke` anchor를 유지한다.
- 이 결정 문서의 code-level checkpoint에서는 package build, 설치, service/Hyper-V mutation, actual VM,
  public signing/publication을 수행하지 않았다. Legacy 설치본 checkpoint는 별도 evidence에서 exact
  package provenance와 host/service mutation 여부를 기록한다.
