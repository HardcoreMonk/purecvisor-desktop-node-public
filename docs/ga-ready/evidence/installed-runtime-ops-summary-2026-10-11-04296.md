# Installed runtime ops summary `0.42.96-admin-smoke` (2026-10-11)

evidence_id: `installed-runtime-ops-summary-2026-10-11-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261011-04295-04296/installed-runtime-ops-summary`
summary_sha256: `f135ba95f7fe8bf0902be9ddf2318fe3fae041203a8a2f255dac0116883bae6e`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

release train `0.42.96`의 `0.42.95 -> 0.42.96` pair orchestrator가 마지막 bucket으로 runtime ops를 캡처했다. target `0.42.96-admin-smoke`(Host `+e07113c`)가 설치된 상태다.

설치본 `pcvcli.exe`가 기본 보호 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`, stderr `0` byte다. token 값은 출력하거나 기록하지 않았고, summary에 token 형태 문자열은 `0`개다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.96-admin-smoke` |
| 설치본 Host ProductVersion | `0.42.96-admin-smoke+e07113c5f555715955856897c687c7c8843db1dc` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| summary errors | `0` |
| VM | `2` (`pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off) |

## Nonclaims

- 읽기 전용 캡처다. 설치본, 서비스, VM을 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
