# PureCVisor Desktop Node Phase 9 Local API runtime hardening 설계

## 목적

Phase 9는 Desktop Node Local API spike의 제품화 전 런타임 결정을 명시하고, 기존 job 상태 전이를 더 안전하게 만든다. Phase 8이 service token file과 설치 준비 경계를 다뤘다면, Phase 9는 Local API 자체의 persistence, retry, cancel, worker, CORS, auth 정책을 한곳에 고정한다.

이 단계는 여전히 `spikes/purecvisor-desktop-node/` 아래의 격리 spike 범위다. Linux `purecvisorsd`, Single Edge REST/UDS API, Single Edge Web UI에는 연결하지 않는다.

## 현재 판단

Phase 9의 결론은 다음과 같다.

- database-backed persistence는 아직 도입하지 않는다.
- JSON file job store를 spike 기본 persistence로 유지한다.
- automatic retry worker는 도입하지 않는다.
- manual retry는 유지하되 attempt 상한을 둔다.
- retry backoff는 자동 재시도 스케줄이 생길 때까지 실행 지연으로 적용하지 않고, runtime policy에 decision으로 남긴다.
- 실행 중 helper interruption/cancel은 도입하지 않는다.
- cancel은 계속 queued job에만 허용한다.
- runspace/threaded worker execution은 도입하지 않는다.
- bounded worker-pool tick을 유지한다.
- CORS/OPTIONS는 기본 공개하지 않는다.
- bundled Web Console은 same-origin static serving을 계속 사용한다.
- multi-user auth/RBAC는 도입하지 않는다.
- Phase 7/8의 single bearer token file 정책을 유지한다.

## 포함 범위

Phase 9에 포함한다.

- Local API runtime policy helper 추가
- `GET /api/v1/runtime/policy` route 추가
- runtime policy route의 read-only method gate 추가
- manual retry attempt 상한 추가
- retry 상한 초과 시 `PCV_JOB_RETRY_LIMIT_REACHED` 반환
- runtime policy에 persistence, retry, cancel, worker, CORS, auth, token storage 결정을 명시
- API README, developer index, verification policy, follower queue 갱신

## 제외 범위

Phase 9에서 제외한다.

- SQLite/PGLite/Supabase 같은 database-backed job store
- automatic retry loop
- delayed retry queue execution
- running helper process interruption
- PowerShell runspace/threaded worker engine
- CORS preflight 공개
- multi-user auth/RBAC
- Windows Credential Manager 또는 DPAPI token storage
- Linux Single Edge runtime 변경

## runtime policy route

새 route는 다음 계약을 가진다.

```text
GET /api/v1/runtime/policy
```

응답 body의 `data`에는 다음 필드를 둔다.

```json
{
  "persistence": {
    "backend": "json-file",
    "database_backed": false
  },
  "retry": {
    "automatic": false,
    "manual": true,
    "max_attempts": 3,
    "backoff": "deferred"
  },
  "cancel": {
    "queued": true,
    "running": false
  },
  "worker": {
    "mode": "bounded_tick",
    "threaded": false
  },
  "cors": {
    "enabled": false,
    "options_preflight": false
  },
  "auth": {
    "mode": "single_bearer_token",
    "multi_user": false,
    "rbac": false,
    "token_storage": "external_token_file"
  }
}
```

이 route는 문서와 테스트가 참조할 수 있는 runtime decision surface다. 제품 런타임 승격 전에는 이 route의 값이 실제 구현보다 앞서가면 안 된다.

## retry 정책

manual retry는 실패한 job에만 허용한다. `attempt`는 기존 job의 `attempt + 1`로 계산한다.

```text
attempt 1 failed -> retry 가능, retry job attempt 2
attempt 2 failed -> retry 가능, retry job attempt 3
attempt 3 failed -> retry 거부, PCV_JOB_RETRY_LIMIT_REACHED
```

이 상한은 무한 retry로 job store와 Web Console polling 상태가 불어나지 않도록 하는 최소 hardening이다.

## 현재 구현 상태

2026-04-25 기준 Phase 9 구현은 다음 상태다.

- `PcvDesktopApi.psm1`에 `Get-PcvApiRuntimePolicy`와 `/api/v1/runtime/policy` route가 추가됐다.
- `POST /api/v1/runtime/policy`는 `405 PCV_METHOD_NOT_ALLOWED`로 거부한다.
- `Retry-PcvApiJob`은 기본 `MaxAttempts=3`을 적용하고, 초과 시 `409 PCV_JOB_RETRY_LIMIT_REACHED`를 반환한다.
- Local API contract suite는 runtime policy route를 검증한다.
- Local API job-control suite는 attempt `3` failed job의 manual retry 거부를 검증한다.
- 제품 런타임 승격, database-backed persistence, automatic retry, running helper interruption, CORS/OPTIONS, multi-user auth/RBAC는 후속 단계로 남긴다.

## 완료 기준

Phase 9는 다음을 만족하면 완료다.

- `GET /api/v1/runtime/policy`가 현재 runtime 결정을 반환한다.
- runtime policy route가 read-only로 유지된다.
- retry 상한 초과가 구조화된 409 오류로 반환된다.
- 기존 queued-only cancel 정책과 bounded worker pool 정책이 유지된다.
- 기본 API Pester suite가 통과한다.
- CLI/Web/service/Hyper-V 기본 검증이 기존 기대 결과를 유지한다.
