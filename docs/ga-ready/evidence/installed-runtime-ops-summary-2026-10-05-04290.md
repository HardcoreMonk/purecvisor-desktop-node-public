# Installed runtime ops summary `0.42.90-admin-smoke` (2026-10-05)

evidence_id: `installed-runtime-ops-summary-2026-10-05-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261005-04289-04290/installed-runtime-ops-summary`
summary_sha256: `4676fc9df28451254d4b420082ade7767837370b43dc72b4229a573cdf255b0b`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

release train `0.42.90`의 `0.42.89 -> 0.42.90` pair orchestrator가 마지막 bucket으로 runtime ops를 캡처했다. target `0.42.90-admin-smoke`(Host `+0bcc328`)가 설치된 상태다.

설치본 `pcvcli.exe`가 기본 보호 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`, stderr `0` byte다. token 값은 출력하거나 기록하지 않았고, summary에 token 형태 문자열은 `0`개다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.90-admin-smoke` |
| 설치본 Host ProductVersion | `0.42.90-admin-smoke+0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| summary errors | `0` |
| VM | `1` (`pcv-guest-installed-04253-r1` Off) |

## Nonclaims

- 읽기 전용 캡처다. 설치본, 서비스, VM을 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
