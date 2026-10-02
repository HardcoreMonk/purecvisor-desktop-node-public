# Installed runtime ops summary `0.42.85-admin-smoke` (2026-10-02)

evidence_id: `installed-runtime-ops-summary-2026-10-02-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/installed-runtime-ops-summary-20261002-04285`
summary_sha256: `b83f86848b9a17e10d62f75a14b41889ebfad57beaaffcbe3948363893b28692`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

`0.42.85 -> 0.42.86` pair의 runtime ops bucket이다. baseline `0.42.85-admin-smoke`(clean package `+f5b6d10`)가 설치된 상태에서 캡처했다.

설치본 `pcvcli.exe`가 보호된 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`다. token 값은 출력하거나 기록하지 않았다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.85-admin-smoke` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| summary errors | `0` |
| VM total | `1` (`pcv-guest-installed-04253-r1` Off) |

## Nonclaims

- 읽기 전용 캡처다. 설치본, 서비스, VM을 바꾸지 않았다.
- operational current는 `0.42.84-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
