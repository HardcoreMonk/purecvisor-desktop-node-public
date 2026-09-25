# Installed runtime ops summary `0.42.77-admin-smoke`

evidence_id: `installed-runtime-ops-summary-2026-09-25-04277`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/installed-runtime-ops-summary-20260925-04277`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

설치본 `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`가 보호된 토큰 파일로
`ops summary`를 읽었다. CLI exit `0`, 응답 `ok=true`, operation `ops.summary`다.
토큰 값은 출력하거나 기록하지 않았다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.77-admin-smoke` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| token storage | `windows-credential-manager` |
| network exposure | `loopback` |
| console mode | `windows-hyperv-console-handoff` |
| distribution claim | `internal-only-not-public-release` |
| summary errors | `0` |
| VM total | `2` |
| VM | `pcv-cleanhost-20260910-r2-04274-04275` Saved, `pcv-guest-installed-04253-r1` Off |

설치본 서비스의 ops summary는 batch evidence를 `not_configured`, feature promotion을
`unavailable`로 보고한다. 이 캡처는 그 읽기 결과를 남기며 current evidence를 쓰지 않는다.

## 아직 닫히지 않은 후속

| 후속 검증 | 결과 |
| --- | --- |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.77-admin-smoke` |

## Nonclaims

- 이 기록은 설치본 읽기 캡처다. 서비스, MSI, Hyper-V를 변경하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
