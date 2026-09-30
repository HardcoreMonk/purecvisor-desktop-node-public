# Installed runtime ops summary `0.42.84-admin-smoke` (2026-10-01)

evidence_id: `installed-runtime-ops-summary-2026-10-01-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/installed-runtime-ops-summary-20261001-04284`
summary_sha256: `10005d5925bf15ffbe360ed040225541c2732a1a5f71197aaa50aeb8e9887bee`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

`0.42.84 -> 0.42.85` pair의 runtime ops bucket이다. 이전 pair들과 같이 baseline `0.42.84`(fullgate build `+ee90e0e`)가 설치된 상태에서 캡처했다.

설치본 `pcvcli.exe`가 보호된 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`다. token 값은 출력하거나 기록하지 않았다. CLI 출력에 token 형태 문자열(`eyJ`, `Bearer `)은 없었다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.84-admin-smoke` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| summary errors | `0` |
| VM total | `1` (`pcv-guest-installed-04253-r1` Off) |

## Nonclaims

- 읽기 전용 캡처다. 설치본, 서비스, VM을 바꾸지 않았다.
- operational current는 `0.42.84-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
