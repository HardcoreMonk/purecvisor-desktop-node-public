# Installed loopback session and Chromium bootstrap 2026-08-14 `0.42.73`

evidence_id: `installed-loopback-bootstrap-smoke-2026-08-14-04273`
result: `PASS`
evidence_scope: `installed-listener-read-only`
version: `0.42.73-admin-smoke`
artifact_root: `artifacts/installed-loopback-bootstrap-smoke-20260814-04273`
artifact_summary: `artifacts/installed-loopback-bootstrap-smoke-20260814-04273/summary.json`
summary_sha256: `b49dcaf737d1499446be7601297a232a7a26917cb83eb1ea122190a039a17475`
web_base_uri: `http://127.0.0.1/`
api_base_uri: `http://127.0.0.1:7777`
host_mutation_performed: `false`
token_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.73-admin-smoke` Web Console `http://127.0.0.1/`에서 service token 붙여넣기
없이 loopback session이 발급되고, Edge headless CDP가 sessionStorage에 `access_token`을
넣은 뒤 `Auth required` 없이 열렸다. `accounts.json`은 계속 계정 `0` /
`no-default-account`다.

## 관측

| 항목 | 값 |
| --- | --- |
| `/pcv-config.js` | HTTP `200`, `apiBaseUrl=http://127.0.0.1:7777`, token literal 없음 |
| Web `/` | HTTP `200`, token literal 없음 |
| `POST /api/v1/auth/loopback-session` | HTTP `200`, `grant_type=loopback_session`, `typ=loopback_access` |
| `GET /api/v1/auth/session` | HTTP `200` with loopback JWT |
| `GET /api/v1/runtime/policy` | unauthenticated `401`, authenticated `200` |
| Edge CDP | `has_access_token=true`, `auth_gate=false`, fixture VM 문구 없음 |

## Nonclaims

- 이 검증은 설치본 listener read-only다. MSI/service/Hyper-V/OS mutation을 수행하지 않았다.
- canonical `current-evidence.json`을 바꾸지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
