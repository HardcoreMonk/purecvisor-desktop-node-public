# ADR-0013: Job store single-writer transaction lease와 loaded-base CAS

상태: 적용 중
일자: 2026-08-02

## 결정 마커

```text
DESKTOP_NODE_JOB_STORE_WRITER_DECISION: canonical-path-transaction-lease-loaded-base-cas
DESKTOP_NODE_JOB_STORE_SCHEMA_DECISION: v1-v2-compatible-no-revision-field
DESKTOP_NODE_JOB_STORE_EXACTLY_ONCE_CLAIM: false
```

## 맥락

제품은 단일 Windows SCM service runtime을 기본 owner로 사용하지만, 같은 `jobs.json`을 먼저
읽은 두 current binary/runtime가 각각 stale next-state를 저장하면 마지막 writer가 앞선 변경을
덮을 수 있다. 기존 process-local lock과 unique candidate/pending marker protocol은 한 번의 물리
commit을 보호하지만, 서로 다른 runtime/process가 같은 base를 읽은 lost update는 판별하지 못했다.

Schema v1/v2 reader compatibility와 실제 `0.42.65-admin-smoke` rollback reader를 유지해야 하므로
snapshot에 revision 필드나 schema v3를 추가하는 방법은 이번 범위에서 사용할 수 없다.

## 결정

- writer는 지원되는 local fixed-volume identity와 volume-relative long path의 SHA-256으로 이름을
  만든 bounded named mutex를 각 write transaction 동안 획득한다. Windows 이름은
  `Global\\PureCVisor.DesktopNode.JobStore.<hash>`다. 같은 volume의 복수 drive letter는 같은
  identity로 수렴한다.
- runtime/store instance는 `Exists()` 또는 `ReadSnapshot()`에서 읽은 authoritative primary의
  `exists + byte length + SHA-256` identity를 loaded base로 기억한다.
- transaction lease를 얻은 뒤 candidate/pending temp를 만들기 전에 current primary identity와
  loaded base를 비교한다. 불일치는 typed `NotCommitted`/
  `DesktopNodeJobStoreConcurrencyException`으로 끝내며 primary, marker, candidate를 변경하지 않는다.
- `Committed` 뒤에는 instance의 expected identity를 candidate identity로 전진시킨다.
  `NotCommitted` 뒤에는 previous identity를 유지하고, `Indeterminate`는 pending guard reconciliation로
  fail-closed한다.
- lexical `.`/`..`, separator/case와 동일 volume의 복수 drive-letter alias는 동일 lease로
  수렴한다. UNC/device namespace, network/non-fixed drive, SUBST, DOS 8.3, ADS, existing reparse
  point와 hard-linked primary는 fail-closed한다.
- Host는 JSONL `events.jsonl`과 rotation set이 primary/pending/temp 또는 그 ancestor/descendant,
  reparse/volume alias, existing hardlink와 겹치면 listener bind 전에 시작을 거절한다.
- schema v1/v2 JSON shape에는 revision/checksum 필드를 추가하지 않는다. frozen `0.42.65` reader와
  rollback backup byte compatibility를 우선한다.

## 제한과 비주장

- 이 lease는 process lifetime 독점이 아니라 write transaction lease다. SCM service의 단일 runtime
  owner 원칙은 계속 적용된다.
- `0.42.65` 같은 구 binary는 named mutex에 참여하지 않는다. mixed-version concurrent writer는
  지원하지 않으며 update/rollback은 service stop, pending marker absence, backup hash 확인 뒤에만 한다.
- 외부 Hyper-V side effect의 exactly-once를 보장하지 않는다. terminal persistence 실패 또는
  persisted-running recovery는 자동 retry하지 않고 운영자 reconciliation을 요구한다.
- mutex timeout/권한 실패는 요청 단위 old backend fallback이나 provider mutation 재실행으로
  이어지지 않는다.
- 로컬 관리자가 validation 직후 directory namespace를 바꾸는 hostile TOCTOU, 아직 존재하지 않는
  파일에 대한 사후 hardlink 생성과 mixed-version writer는 지원하지 않는다. 설치본은 ACL로 보호된
  단일 ProgramData root와 SCM service owner를 사용한다.

## 결과

- current writer끼리의 concurrent physical write는 직렬화되고, 같은 base에서 출발한 stale writer는
  CAS에서 거절된다.
- unique candidate + durable flush + pending marker protocol은 그대로 유지된다.
- stale/failed candidate는 live memory에 publish되지 않으며 API/worker는 redacted structured error를
  반환한다.
- 실제 frozen `0.42.65` binary는 current writer가 만든 terminal+FIFO queue v1/v2 store와 restored
  backup을 8개 pass에서 읽고 store byte hash를 변경하지 않는다. Queue probe는 read-only file guard로
  start-save를 provider dispatch 전에 차단하고 FIFO attempt 순서를 확인한다.

## 검증과 증거

- `JsonFileDesktopNodeJobStoreTests.StaleLoadedBaseIsRejectedWithoutOverwritingNewerPrimary`
- `JsonFileDesktopNodeJobStoreTests.CanonicalPathAliasesShareOneCrossProcessWriteLeaseName`
- `JsonFileDesktopNodeJobStoreTests.HardLinkedPrimaryIsRejectedBeforeWriterCanSplitAuthoritativeState`
- `DesktopNodeHostJobRuntimeEventSinkTests.HostStartupRejectsRotationTargetOverlappingJobStore`
- `PcvJobStore04265ReaderCompatibility.Tests.ps1`
- `artifacts/job-store-04265-reader-compatibility-20260802-wave2a-current-writer-final5/summary.json`
- `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`

## 롤백

current writer의 named mutex와 identity CAS를 함께 제거하고 직전 physical writer로 되돌릴 수 있다.
롤백 전에는 service stop, pending marker absence, `jobs.json`과 backup SHA-256 확인이 필요하다.
구 binary를 marker가 남은 store에 시작하거나 uncertain mutation을 자동 재실행하지 않는다.
