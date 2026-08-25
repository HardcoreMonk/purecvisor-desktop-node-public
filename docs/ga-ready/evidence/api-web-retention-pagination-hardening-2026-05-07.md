# API/Web Retention Pagination Hardening Evidence - 2026-05-07

```text
evidence_id: api-web-retention-pagination-hardening-2026-05-07
artifact_or_package_version: src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs, web/src/served-app.ts, web/app.js
```

## 요약

2026-05-07 후속 slice는 Local API job list와 Web Console Operator Activity의 retention/pagination contract를 구현했다.

- `GET /api/v1/jobs`는 `limit`/`offset` query를 지원하며 기본 `limit=50`, 최대 `limit=200`으로 제한한다.
- 응답은 기존 `jobs` 배열을 유지하면서 `count`, `returned`, `limit`, `offset`, `next_offset`, `default_limit`, `max_limit`, `retention` metadata를 additive shape로 제공한다.
- Job retention은 terminal job 상태 `succeeded`/`failed`/`canceled`의 최신 500개를 보존하고, `queued`/`running` active job은 보존한다.
- Persisted job store를 로드할 때도 오래된 terminal job을 pruning하고 같은 store에 atomic replace로 반영한다.
- Web Console `Activity` 화면은 `/api/v1/jobs?limit=50&offset=0` 첫 page를 읽고 pagination/retention 요약을 표시한다.
- Browser fixture와 TypeScript API type은 pagination/retention metadata를 contract mirror로 포함한다.

## 범위

이 evidence는 code-level API/Web hardening과 static/browser fixture 검증이다. 다음 항목은 이 slice에서 구현 완료로 주장하지 않는다.

- timeout/rate-limit policy
- server push 또는 realtime event stream
- checkpoint retention bulk delete
- token rotation/revoke UX
- diagnostic bundle server-side collection/download action
- Hyper-V/service/MSI/firewall/trust-store/LAN/Event Log/update mutation
- public trusted signing 또는 외부 stable publication

## 검증

TDD RED/GREEN:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobList|FullyQualifiedName~JobRetention"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

결과:

- RED: 기존 API는 `/api/v1/jobs?limit=2&offset=0` metadata와 invalid pagination guard가 없어 실패했다.
- RED: 기존 Web Console은 `/api/v1/jobs?limit=50&offset=0`를 호출하지 않아 static guard가 실패했다.
- GREEN: focused API tests PASS, 9 tests.
- GREEN: Web static tests PASS, 28 tests.

최종 Web/API 검증:

```powershell
npm run build:served --prefix web
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobList|FullyQualifiedName~JobRetention"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
npm run browser:fixture --prefix web
node --check web/app.js
```

결과:

- `npm run build:served --prefix web`: PASS, served `app.js` regenerated.
- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobList|FullyQualifiedName~JobRetention"`: PASS, 9 tests.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"`: PASS, 28 tests.
- `npm run browser:fixture --prefix web`: PASS.
- `node --check web/app.js`: PASS.

이 evidence는 read-only Web/API contract hardening evidence이며 public release/public trusted signing/외부 stable publication evidence가 아니다.
