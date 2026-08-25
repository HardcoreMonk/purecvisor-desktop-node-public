# PureCVisor Desktop Node C# 구조 개선 Wave 0 gap registry

- 감사 기준 commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- 작성일: 2026-08-02
- 범위: Wave 0 fault/test ownership 기준선과 behavior-preserving 주입 seam
- 제품 source seam 변경: `true`
- 제품 동작 변경: `false`
- host mutation 수행: `false`
- public trusted signing 변경: `false`
- external stable publication 변경: `false`
- machine-readable 장부: `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json`

## 1. 판정 규칙

이 문서는 현재 코드가 안전하다고 가정하지 않는다. 각 항목은 다음 세 종류의 근거를 구분한다.

- `existing-test`: 현재 실행 가능한 테스트가 해당 trace를 직접 확인한다.
- `static-trace`: source call order로 확인했지만 해당 실패를 주입해 실행한 테스트는 아직 없다.
- `missing`: happy-path 또는 인접 계약만 있고 필수 실패 경계는 검증하지 않는다.

현재 unsafe trace와 기대 안전 결과가 다른 경우 기대 결과 테스트를 skip으로 추가하지 않는다. 담당 wave의 구현 PR에서 기대 결과를 먼저 RED로 확인하고 같은 PR에서 GREEN으로 닫는다. 현재 동작을 고정할 필요가 있는 임시 characterization은 항상 통과하는 별도 테스트로 만들고, machine-readable 장부의 제거 조건을 함께 만족할 때 교체한다.

## 2. 필수 fault scenario 요약

| Gap ID | 실패 시나리오 | 현재 판정 | 현재 직접 evidence | 기대 안전 결과 담당 |
|---|---|---|---|---|
| `W0-FI-01` | job create/save 실패 뒤 memory/queue ghost | `safe / Wave 2A complete` | unique-temp/flush/typed outcome, restart guard, transaction lease/CAS, API 503/worker/native 0 | 완료 |
| `W0-FI-02` | start/cancel/complete 저장 실패 divergence | `safe / Wave 2A complete` | persist-before-publish, durable cancel-before-signal, terminal uncertainty block/no replay | 완료 |
| `W0-FI-03` | GET timeout 뒤 late commit/다음 요청 overlap | `unsafe / static-trace` | 504와 token 전달만 확인 | Wave 5A |
| `W0-FI-04` | malformed 또는 root-non-object store startup | `safe / Wave 2A complete` | typed semantic validator, corrupt/future no-write block, marker semantic guard | 완료 |
| `W0-FI-05` | shutdown 중 HTTP/noVNC child task 미추적 | `unsafe / static-trace` | noVNC frame happy-path만 확인 | Wave 5A |
| `W0-FI-06` | listener/worker fault가 service health에 미전파 | `unsafe / static-trace` | 정상 worker 시작만 확인 | Wave 5A |

## 3. `W0-FI-01` job create/save 실패 뒤 ghost

### 재현 조건

1. `DesktopNodeJobRuntime`을 recording job store와 함께 만든다.
2. queued mutation 생성 직전에 recording store가 첫 save에서 `IOException`을 던지게 한다.
3. 같은 Runtime owner의 job list와 dequeue 결과를 읽고, API companion에서는 `/api/v1/vms/alpha/start`의 route/status/native-dispatch 결과를 확인한다.

### 완료된 안전 trace와 evidence

- `DesktopNodeJobRuntime.CreateUnsafe`는 별도 candidate jobs/queue를 계산해 `WriteCreateCandidateSnapshotUnsafe`가 `Committed`를 반환한 뒤에만 live reference와 retention count를 publish한다 (`implemented-safe-create-commit`). 결정 경계는 `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`가 소유한다.
- store write 예외는 typed `DesktopNodeJobStoreWriteException`과 redacted `PCV_JOB_STORE_SAVE_FAILED`로 변환되며, API processor는 request id를 보존한 HTTP 503 JSON을 반환한다 (`implemented-safe-pre-ack`).
- `JobRuntimeDurabilityTests.CreateSaveFailureDoesNotPublishMemoryOrQueueGhost`는 store `NotCommitted`에서 attempted candidate가 있어도 새 ghost가 없고, 빈 state와 이미 승인된 기존 state 모두 same-process/disk/restart에서 보존됨을 확인한다 (`existing-test`, safe create invariant).
- `JsonFileDesktopNodeJobStoreTests`의 12 methods/14 cases는 unique candidate/marker temp와 flush 순서, pre-replace `NotCommitted`, post-replace exact-primary `Committed`, unreadable/mismatch `Indeterminate`, invalid/pending marker restart block, orphan non-promotion, primary access failure와 v1/v2 shape를 real filesystem에서 확인한다.
- `ApiJobStoreFailureCharacterizationTests.CreateSaveFailureDoesNotReturn202OrInvokeNativeMutation`는 HTTP 503/code/redaction, job list 0, worker processed false와 native invoke 0을 확인하는 API integration companion이다.
- `ApiJobStoreFailureCharacterizationTests.IndeterminateCreateCommitReturns503AndBlocksJobRoutesAndWorker`는 indeterminate HTTP 503 뒤 job route 409, worker false와 native invoke 0을 확인한다.
- Host/product tests는 marker가 있을 때 job-store migration, Update/Rollback와 preserve-data removal을 stop+wait 뒤 차단하고, explicit RemoveData가 exact/GUID-owned temp만 삭제함을 확인한다.

### 기대 안전 결과

- durable save가 실패하면 HTTP 202를 반환하지 않는다.
- memory job과 queue publish는 0건이며 worker invoke도 0건이다.
- temp residue를 best-effort 정리하고 redacted `PCV_JOB_STORE_SAVE_FAILED` 계열의 구조화된 실패를 반환한다.
- 같은 process와 restart 후 모두 ghost job이 관찰되지 않는다.

### RED → GREEN 계획

- RED 확인: 기준 코드에서 `JobRuntimeDurabilityTests.CreateSaveFailureDoesNotPublishMemoryOrQueueGhost`는 failed write 뒤 memory job 1건을 발견해 실패했고, API companion은 구조화 응답 대신 `IOException` 전파를 발견해 실패했다.
- GREEN 완료: unique candidate/marker를 `Flush(true)`하고 candidate/previous exact identity를 판정한 typed `Committed` 뒤에만 memory/queue와 HTTP success를 publish한다. `NotCommitted`는 이전 primary를 보존하고 `Indeterminate`는 fixed marker와 current-runtime restart block을 유지한다.
- 임시 characterization 교체: `DesktopNodeJobRuntimeTests.CreateSaveFailurePreservesCurrentPublishBeforeCommitOrder`는 `TM-JOB-CREATE-SAVE-010`에 따라 정확히 하나의 safe Runtime Fact로 교체했다. API companion 이름 교체는 `TM-API-JOB-CREATE-SAVE-013`이 추적한다.
- 완료 범위: `W0-FI-01` create/save failure는 product의 단일 SCM Runtime owner와 ADR-0013의
  canonical-path write-transaction lease/loaded-base CAS 경계에서 safe다. Current writer stale candidate는
  primary overwrite 없이 `NotCommitted`로 거절된다. Physical predecessor evidence는
  `docs/ga-ready/evidence/csharp-architecture-wave2a-physical-job-store-durability-2026-08-02.md`, completion
  evidence는 `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`다.
- 남은 경계: process lifetime lease와 marker-unaware 구 binary를 포함한 mixed-version concurrent writer는
  지원하지 않는다. Update/rollback은 service stop과 marker absence를 요구하며 external side effect
  exactly-once는 주장하지 않는다.

## 4. `W0-FI-02` start/cancel/complete 저장 실패 divergence

### 재현 조건

- start: queued job을 정상 저장한 뒤 store parent를 파일로 바꾸고 worker tick을 실행한다.
- queued cancel: queued job 저장 후 다음 save만 실패하게 하고 `/jobs/{id}/cancel`을 호출한다.
- running cancel: native provider가 block된 동안 cancel-state save를 실패시킨다.
- complete: running save까지 성공시킨 뒤 native 결과 반환 직전에 terminal save를 실패시킨다.
- 각 경우 memory snapshot, 마지막 durable file, restart 결과, provider signal/invoke count를 함께 비교한다.

### 완료된 안전 trace와 evidence

- start/cancel/complete는 live state를 직접 바꾸지 않고 candidate jobs/queue를 계산하며,
  `Committed` 뒤에만 publish한다. Start `NotCommitted`는 durable queued/live queued를 보존하고
  provider invoke를 0으로 유지한다.
- running cancel은 `PCV_JOB_CANCEL_REQUESTED`를 durable commit하고 live publish한 뒤
  `stateSync` 밖에서 provider callback을 호출한다. 반복 cancel은 추가 save/signal 없이 같은
  durable request를 반환한다.
- terminal save 실패는 confirmed previous running state를 유지하며 same-process mutation/dispatch를
  block한다. Provider side effect는 자동 재실행하지 않는다. Persisted running restart recovery도
  `PCV_JOB_INTERRUPTED`, `retryable=false` candidate가 commit된 뒤에만 publish한다.
- `JobRuntimeDurabilityTests.StartSaveFailureKeepsRecoverableMeaning`,
  `TransitionSaveFailureKeepsRecoverableMeaning`,
  `RunningCancelPersistsRequestBeforeProviderSignalOutsideStateLock`가 memory/disk/restart/provider
  순서를 직접 검증한다. API worker/cancellation tests는 structured response, linked token과
  provider replay 0의 integration companion으로 유지한다.

### 기대 안전 결과

- start/cancel/complete 각각 next-state durable commit과 memory publish 순서가 하나의 명시적 protocol을 따른다.
- 저장 실패 시 disk의 이전 state, memory의 effective state, provider signal 여부와 recovery action이 구조화되어 동일 의미를 가진다.
- running cancel request는 먼저 저장하고 state lock 밖에서 provider cancellation을 신호한다.
- terminal save 실패 뒤 restart가 stale queued job을 자동 실행하거나 uncertain native side effect를 재실행하지 않는다.

### RED → GREEN 계획

- RED 확인: predecessor의 start/cancel/complete는 memory-before-save와 signal-before-save trace로
  원하는 안전 invariant에 실패했다.
- GREEN 완료: 위 세 Runtime durability invariant와 API companion이 persist-before-publish,
  durable-cancel-before-signal, terminal uncertainty block/no replay를 고정한다.
- 임시 characterization 교체: `TM-JOB-START-SAVE-003`과
  `TM-JOB-RUNNING-CANCEL-011`은 `completed`다. API integration companion은 제거 대상이 아니다.

## 5. `W0-FI-03` GET timeout 뒤 late commit/overlap

### 재현 조건

1. cancellation token을 의도적으로 무시하고 release gate까지 block하는 read adapter를 사용한다.
2. 첫 GET을 route timeout보다 오래 block해 504를 받는다.
3. 첫 adapter를 release하기 전에 두 번째 GET을 보낸다.
4. maximum concurrent invocation, timeout 뒤 state commit counter, 모든 child task 종료를 기록한다.

### 현재 trace와 evidence

- GET은 `Task.Run`으로 시작되고 호출 thread는 `Wait(timeout)`을 수행한다. timeout 뒤 token을 cancel하지만 route task를 기다리지 않고 continuation으로 exception/dispose만 처리한 뒤 504를 반환한다: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs:194-226` (`static-trace`).
- outer `Handle`의 serialization lock은 504 반환 시 풀리므로 token을 무시한 첫 route task가 살아 있는 동안 다음 request가 새 route task를 시작할 수 있다: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs:171-191` (`static-trace`).
- `RouteTimeoutReturnsProblemDetailsWhenNativeRouteExceedsDeadline`와 `RouteTimeoutPassesCancellationToNativeAdapter`는 504와 cooperative cancellation 전달을 확인하지만 token 무시, late commit, 다음 요청 overlap은 확인하지 않는다: `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs:83`, `:119` (`existing-test`, partial).
- `ProcessorSerializesConcurrentHandleCalls`는 timeout이 없는 정상 경로의 maximum concurrency 1만 확인한다: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs:168` (`existing-test`, timeout coverage 아님).

### 기대 안전 결과

- timeout 응답 뒤 이전 request가 application state를 commit하지 않는다.
- Wave 5A serialization 정책에서는 timeout된 task가 다음 request와 processor/native state owner 안에서 겹치지 않는다.
- child task fault와 completion이 supervisor에 관찰되며 shutdown drain 대상에 포함된다.

### RED → GREEN 계획

- RED: `DesktopNode.Api.Tests.ApiAsyncLifetimeTests.TimeoutIgnoringCancellationCannotCommitOrOverlapNextRequest`가 504 뒤 late commit 0, max concurrent 1, tracked task 0을 요구한다.
- GREEN: Wave 5A에서 end-to-end async, explicit serialization gate, post-timeout commit guard와 child-task supervision을 application lifetime owner에 둔다.
- 임시 characterization: 현재 overlap을 숫자로 고정할 필요가 있으면 별도 always-passing baseline trace 테스트를 만들고 expected `current_max_concurrent`를 명시한다.
- 제거 조건: 새 안전 테스트와 timeout/shutdown fault test가 GREEN이고 sync-over-async `Task.Run + Wait` 경로가 제거되면 임시 trace 테스트를 제거한다.

## 6. `W0-FI-04` malformed/root-non-object store startup

### 재현 조건

- job store에 문법 오류 JSON과 유효하지만 object가 아닌 `[]`, `null`, `"text"`, `42`, `true`를 각각 기록한다.
- processor 또는 service를 시작하고 structured diagnostics, quarantine/block 정책, original file 처리와 host liveness를 확인한다.

### 완료된 안전 trace와 evidence

- `DesktopNodeJobStoreSnapshotValidator`가 projection 전에 strict UTF-8, object root, v1/v2,
  job ID/status/attempt/timestamp/params/result/error, queue reference/중복과 상태 조합을 검사한다.
- malformed/non-object/semantic-corrupt input은 `PCV_JOB_STORE_CORRUPT` blocked state로 남고
  quarantine·rewrite·partial publish·worker dispatch는 0이다. Unsupported future는 별도
  `PCV_JOB_STORE_SCHEMA_UNSUPPORTED`로 원본을 보존한다.
- fixed marker의 identity가 일치해도 primary semantic validation이 성공해야 marker를 제거한다.
- `DesktopNodeJobRuntimeTests.MalformedOrNonObjectRootStartsInStructuredBlockedState` 7 cases와
  `JsonJobStoreSemanticIntegrityTests`가 typed validator 및 v1/v2/future 경계를 직접 검증한다.
  API future-schema test는 integration companion으로 유지한다.

### 기대 안전 결과

- 문법 오류와 non-object root 모두 비구조적 constructor/service crash를 만들지 않는다.
- 지원 가능한 정책에 따라 corrupt store를 quarantine하거나 read-block 상태로 보존하고, redacted 구조화 오류와 operator action을 제공한다.
- 손상 store에서 mutation/worker invoke와 silent overwrite는 0건이다.

### RED → GREEN 계획

- RED 확인: predecessor의 valid non-object roots는 constructor 밖으로 `InvalidOperationException`을
  전파해 structured startup invariant에 실패했다.
- GREEN 완료: root kind/semantic integrity를 parse 직후 typed result로 분류하고 no-write block한다.
- 임시 characterization 교체: `TM-JOB-NONOBJECT-012`는 6 unsafe cases를 malformed 포함 7 safe
  cases로 교체해 `completed`다.

## 7. `W0-FI-05` shutdown 중 HTTP/noVNC task tracking

### 재현 조건

- body read가 block된 HTTP request와 양방향 copy 중인 noVNC session을 각각 연다.
- service stop 또는 `DesktopNodeHostApplication.Dispose()`를 호출한다.
- admission close, request/noVNC child task registry, 양 copy task cancellation/await, socket handle과 10초 drain deadline을 관찰한다.

### 현재 trace와 evidence

- host가 보관하는 `loopTasks`는 listener loop와 mutation worker뿐이다: `src/DesktopNode.Host/DesktopNodeHostApplication.cs:54-60` (`static-trace`).
- accepted HTTP request는 `_ = Task.Run(...)`으로 시작하고 task를 보관하지 않는다: `src/DesktopNode.Host/DesktopNodeHostApplication.cs:154-173` (`static-trace`).
- noVNC는 두 copy task 중 하나만 `WhenAny`로 기다리며 반대 task를 취소하거나 await해 exception을 관찰하지 않는다: `src/DesktopNode.Host/DesktopNodeHostApplication.cs:399-415` (`static-trace`).
- dispose는 global token을 cancel하고 listener/worker `loopTasks`만 최대 5초 기다린다: `src/DesktopNode.Host/DesktopNodeHostApplication.cs:130-151` (`static-trace`).
- `NoVncBridgeProxiesWebSocketFramesToLoopbackTcpTarget`는 frame happy-path만 확인한다: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs:428` (`existing-test`, shutdown coverage 아님).

### 기대 안전 결과

- shutdown은 새 admission을 닫고 모든 HTTP/noVNC request task를 추적·취소·await한다.
- noVNC의 한 방향이 끝나면 반대 방향을 취소하고 두 exception을 모두 관찰한다.
- 정상 close와 deadline 초과 강제 종료를 구분하고 최종 tracked task/socket handle은 0이다.

### RED → GREEN 계획

- RED: `DesktopNode.Host.Tests.DesktopNodeHostShutdownTests.StopTracksAndDrainsBlockedHttpRequest`와 `StopCancelsAndAwaitsBothNoVncDirections`를 recording stream/socket과 task registry로 실행한다.
- GREEN: Wave 5A에서 bounded admission과 request/noVNC task registry를 application lifetime owner에 추가하고 drain 순서를 명시한다.
- 임시 characterization: 현재 happy-path noVNC 테스트는 유지한다. untracked task 존재를 private reflection으로 고정하지 않는다.
- 제거 조건: 두 shutdown 테스트, 10초 policy, handle leak 0과 기존 frame parity가 모두 PASS한다.

## 8. `W0-FI-06` listener/worker fault health propagation

### 재현 조건

- listener accept loop에 unexpected bind/listen fault를, worker/store seam에 non-cancellation fault를 각각 주입한다.
- Windows Service `ExecuteTask`, service health/current-card와 structured log가 fault를 관찰하는지 확인한다.
- process가 `Running but dead` 상태로 남는지 확인한다.

### 현재 trace와 evidence

- listener loop는 모든 `HttpListenerException`을 정상 return처럼 처리한다: `src/DesktopNode.Host/DesktopNodeHostApplication.cs:154-170` (`static-trace`).
- worker loop는 unexpected exception을 catch한 뒤 현재 inner iteration만 break하고 idle loop를 계속한다: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs:2190-2238` (`static-trace`).
- `DesktopNodeWindowsService.ExecuteAsync`는 host child completion/fault 대신 infinite delay만 기다린다: `src/DesktopNode.Host/DesktopNodeWindowsService.cs:9-20` (`static-trace`).
- dispose는 `Task.WaitAll`의 `AggregateException`을 버린다: `src/DesktopNode.Host/DesktopNodeHostApplication.cs:130-151` (`static-trace`).
- `HostStartsBackgroundWorkerForQueuedJobs`는 정상 시작/처리만 확인한다: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs:33` (`existing-test`, fault health coverage 아님).

### 기대 안전 결과

- listener 또는 worker의 unexpected completion/fault가 Windows Service lifetime과 health에 즉시 전파된다.
- service는 `Running but dead`로 남지 않고 redacted Event Log/health reason을 남긴다.
- stop path가 remaining HTTP/noVNC/worker state를 drain 또는 reconciliation한다.

### RED → GREEN 계획

- RED: `DesktopNode.Host.Tests.DesktopNodeWindowsServiceSupervisionTests.ListenerFaultFailsServiceHealth`와 `WorkerFaultFailsServiceHealth`가 injected child fault 뒤 bounded time 안에 service fault/non-running과 structured reason을 요구한다.
- GREEN: Wave 5A에서 listener/worker completion을 하나의 supervisor가 await하고 unexpected return/fault를 service lifetime에 전파한다.
- 임시 characterization: 현재 정상 worker 시작 테스트를 유지하고 fault를 정상으로 간주하는 테스트는 추가하지 않는다.
- 제거 조건: listener/worker fault tests, current-card non-running/health evidence와 clean shutdown test가 모두 PASS한다.

## 9. private reflection과 process-global CWD inventory

### Private reflection: 0건

기존 `BatchEvidenceReadGuardRejectsReparsePointSegments`와 `BatchEvidenceSortTimePlacesUnreadableSummaryLast`는 Wave 0의 internal file-access seam과 recording fake를 직접 호출하도록 전환했다. `BindingFlags.NonPublic` 접근은 0건이다. 테스트 owner 이동과 configured-root/path policy 분리는 Wave 7 후보로 남긴다.

### Process-global CWD mutation: 1 test / 2 calls

`DesktopNode.Api.Tests.BatchEvidenceSummaryReaderTests.RelativeChildEvidenceIsIgnoredWithoutConfiguredChildRoot`가 `Directory.SetCurrentDirectory`를 per-test 생성자와 `Dispose`에서 각각 한 번 사용한다: `src/DesktopNode.Api.Tests/BatchEvidenceSummaryReaderTests.cs`.

- Wave 0에서 `Batch evidence CWD isolation` collection을 추가하고 `DisableParallelization=true`로 process-global probe를 격리했다.
- test body에는 CWD mutation이 없고 모든 생성 파일은 하나의 per-test GUID sandbox 아래에 있다.
- Wave 7에서 configured child-root/path resolver를 process CWD와 분리하면 이 직렬화 예외와 CWD scope를 제거한다. Wave 0의 low-level file-access seam만으로 상대 경로 의미를 변경하지 않는다.

## 10. production source-text inspection inventory

| 현재 test ID | 검사 대상 | direct replacement |
|---|---|---|
| `DesktopNode.Host.Tests.DesktopNodeHostServiceActionTests.ProtectedTokenBootstrapDoesNotInvokeExternalAclExecutable` | `DesktopNodeHostServiceAction.cs` 문자열 | recording ACL/process boundary에서 external invocation 0 검증, Wave 3 |
| `DesktopNode.Host.Tests.DesktopNodeWindowsServiceControllerTests.DeleteClosesServiceHandleBeforeWaitingForMissing` | controller source regex | injected native service handle lifecycle call-order test, Wave 3 |

현재 production source-text 검사는 2건이다. Wave 1D에서
`OpsSummarySnapshotAssemblyStaysOutsideRequestProcessor`를 제거하고 compiled metadata/IL guard
`ApiArchitectureOwnershipTests.OpsSummaryProjectionUsesDedicatedOwner`로 교체했으며,
`TM-SOURCE-OPS-007`을 `completed`로 닫았다. 남은 문자열 검사는 임시 guard로 보존하되
replacement direct test가 같은 coverage boundary를 증명하면 제거한다.

## 11. 테스트 소유권 migration inventory

### 완료된 이동

- `DesktopNode.Api.Tests.HyperVDomainContractTests.*` → `DesktopNode.HyperV.Tests.HyperVDomainContractTests.*`
- source: `src/DesktopNode.Api.Tests/HyperVDomainContractTests.cs` → `src/DesktopNode.HyperV.Tests/HyperVDomainContractTests.cs`
- owner: `DesktopNode.HyperV.Tests`
- boundary: domain/dispatch/WMI provider contract
- test case: 35 → 35, 전체 합계 감소 없음
- `DesktopNode.Api.Tests.ApiRuntimePolicyRequestProcessorTests.Native*` 38 methods/49 cases → `DesktopNode.HyperV.Tests.DesktopNodeHyperVNativeAdapterTests.Native*`
- `DesktopNode.Api.Tests.ApiRuntimePolicyRequestProcessorTests.Wmi*` 22 methods/30 cases → `DesktopNode.HyperV.Tests.DesktopNodeHyperVWmiProviderTests.Wmi*`
- API old owner 발견 수 0, Hyper-V replacement 79/79, 전체 합계 감소 없음
- CWD test는 `BatchEvidenceSummaryReaderTests.RelativeChildEvidenceIsIgnoredWithoutConfiguredChildRoot`로 1→1 이동하고 명시적 직렬화 collection을 적용했다.

### 완료된 additive Runtime owner와 남은 후보

- Wave 1A additive owner 분리로 `DesktopNode.Runtime.Tests`가 job state/queue/store/retention/recovery 직접 동작 18 methods/23 cases와 compiled ownership guards 2 cases를 소유한다. API compiled guards 2 cases도 processor가 Runtime owner를 위임하고 retired store/clock seam을 재도입하지 않음을 고정한다.
- API에는 route/status/JSON/native-dispatch/linked-cancellation integration companion 23 cases를
  유지한다. Wave 2A의 create-save/start-save/running-cancel/non-object migration 4건은 모두
  `completed`이며 Runtime owner의 safe replacement와 API companion을 함께 보존한다.
- Batch evidence path/sort tests는 `BatchEvidenceSummaryReaderTests` 또는 명시적 policy test 파일로 분리한다.
- HTTP/noVNC/service supervision은 `DesktopNode.Host.Tests`가 계속 소유하며 API processor 내부 task를 reflection으로 검사하지 않는다.

정확한 old/replacement ID pattern, owner와 removal condition은 machine-readable migration 장부가 소유한다.

## 12. Wave 0 closure checklist

- [x] 여섯 필수 fault scenario에 재현 조건과 현재 trace를 기록했다.
- [x] 현재 test evidence와 static inference를 구분했다.
- [x] 기대 안전 결과, 담당 wave와 RED/GREEN test ID를 기록했다.
- [x] 임시 characterization의 replacement/removal 조건을 machine-readable 장부에 기록했다.
- [x] private reflection, CWD mutation, production source-text 검사와 test ownership 후보를 inventory했다.
- [ ] 각 담당 wave 구현 PR에서 기대 결과 테스트를 RED로 확인하고 같은 PR에서 GREEN으로 닫는다.
- [x] 감사 전 기준 591 대비 Wave 0 최종 611 tests/skip 0과 coverage fixture를 상위 계획의 quality baseline 작업에서 고정했다.

이 registry 자체는 제품 동작이 안전해졌다는 evidence가 아니다. 실패 재현과 안전 결과를 누락 없이 다음 wave로 넘기는 감사 장부다.
