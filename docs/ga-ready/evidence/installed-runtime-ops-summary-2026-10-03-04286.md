# Installed runtime ops summary `0.42.86-admin-smoke` (2026-10-03)

evidence_id: `installed-runtime-ops-summary-2026-10-03-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/installed-runtime-ops-summary-20261003-04286`
summary_sha256: `a03b8588db7c0d65559f93ec580bdb046cf8b85de440f837a779616cb35d75c3`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

`0.42.86 -> 0.42.87` pair의 runtime ops bucket이다. baseline `0.42.86-admin-smoke`(fullgate build `+b807803`)가 설치된 상태에서 캡처했다.

설치본 `pcvcli.exe`가 보호된 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`, stderr `0` byte다. token 값은 출력하거나 기록하지 않았고, summary에 token 형태 문자열은 `0`개다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.86-admin-smoke` |
| 설치본 Host ProductVersion | `0.42.86-admin-smoke+b807803f778e29c206f1bb2ba8277d2a1136198f` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| summary errors | `0` |
| VM | `1` (`pcv-guest-installed-04253-r1` Off) |

## Nonclaims

- 읽기 전용 캡처다. 설치본, 서비스, VM을 바꾸지 않았다.
- operational current는 `0.42.86-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
