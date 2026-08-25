# ADR-0012: API read concurrency policy

상태: 폐기됨 (`closed-not-adopted`)
일자: 2026-08-03

## 결정 마커

```text
DESKTOP_NODE_API_READ_CONCURRENCY_DECISION: serialized
DESKTOP_NODE_API_CONCURRENT_READ_ROLLOUT: closed-not-adopted
DESKTOP_NODE_MUTATION_CONSUMER: single
```

## 맥락

현재 `DesktopNodeApiRequestProcessor`는 하나의 processor-wide synchronization owner 아래에서
request route를 처리하고, job mutation은 `RunWorkerLoopAsync`의 단일 consumer를 통해 실행한다.
일부 GET route는 job store, diagnostics file, account/session state 또는 native Hyper-V read를
접근하므로 단순히 HTTP method가 GET이라는 이유만으로 snapshot isolation이나 provider read
concurrency가 보장되지 않는다.

Wave 5A의 목적은 기존 의미를 보존한 async lifetime, cancellation, admission과 shutdown drain을
안정화하는 것이다. 이 작업과 read route의 병행성 의미 변경을 동시에 적용하면 timeout, file
snapshot, auth revoke, native provider read의 회귀 원인을 분리할 수 없다.

## 결정

- 모든 API processor entry는 기존 직렬화 의미를 유지한다. `read_concurrency_mode` 설정이나
  route별 concurrent-read allowlist를 추가하지 않는다.
- Hyper-V/job mutation worker는 항상 최대 하나의 operation만 실행한다. transport가 바뀌어도 이
  single-consumer 경계는 변경하지 않는다.
- read route의 fairness, concurrent snapshot, starvation 회복을 새 제품 계약으로 주장하지 않는다.
  현재 monitor/processor lock 순서와 route timeout cancellation을 유지한다.
- Wave 5A에서 추가된 `tracked_async_serialized`는 listener admission과 request task lifetime을
  추적하는 opt-in일 뿐, processor read concurrency를 병행으로 바꾸지 않는다.
- 이 ADR은 bounded concurrent-read 대안을 `closed-not-adopted`로 종결한다. 따라서 5B 구현과
  `read_concurrency_mode=bounded` 설정은 생성하지 않는다.

## 취소·영속성 경계

- route timeout은 request-scoped token을 취소하고 timeout response를 반환하지만, 이미 durable
  enqueue commit된 job을 transport task 취소로 되돌리지 않는다.
- mutation queue의 start/complete persistence와 reconciliation은 기존 job runtime owner가
  소유한다. read concurrency 결정을 이유로 provider 재실행이나 alternate backend fallback을
  추가하지 않는다.
- `tracked_async_serialized` admission reject는 body read와 processor entry 전에 503으로 끝나며,
  job store나 Hyper-V mutation을 호출하지 않는다.

## 결과

- 5B는 `closed-not-adopted`로 닫고 전체 API serialization을 최종 불변조건으로 유지한다.
- Wave 6 ASP.NET Core transport는 동일 application core와 single mutation worker를 재사용하며,
  transport adapter가 read concurrency 정책을 복제하지 않는다.
- 실제 병행 read가 필요해지는 경우에는 snapshot owner, fairness, cancellation, load profile,
  auth revoke와 diagnostics consistency를 별도 L/Release ADR로 다시 결정해야 한다.

## 검증과 증거

- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --no-restore`
- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release --no-restore`
- `dotnet test src/DesktopNode.sln -c Release --no-restore` (`815/815`, skip `0`)
- mutation worker의 `workerCount` clamp와 job runtime serialization 관련 기존 API/Host tests
- `docs/ga-ready/evidence/csharp-architecture-wave5a-admission-lifetime-code-slice-2026-08-03.md`

이 결정은 installed load, package-pair closure, actual VM/Hyper-V mutation, ASP.NET Core transport
promotion을 수행하거나 주장하지 않는다.

## 롤백/재검토

이 결정은 bounded read 구현을 추가하지 않은 상태에서만 유효하다. 별도 ADR이 채택되면
`closed-not-adopted` row를 historical로 보존하고, allowlisted read route와 fairness gate를
새 package/installed evidence로 검증한 뒤에만 정책을 변경한다.

