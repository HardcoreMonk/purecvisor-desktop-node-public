# Installed runtime ops summary `0.42.78-admin-smoke` (2026-09-29)

evidence_id: `installed-runtime-ops-summary-2026-09-29-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/installed-runtime-ops-summary-20260929-04278`
summary_sha256: `5eb858f644ee60072cf1a624362d7654919f3f780db00bf1a62a3fe32b928d79`
command_surface: `pcvcli --protected-token-file <api-token.dpapi.json> --json ops summary`
route: `GET /api/v1/ops/summary`
host_mutation_performed: `false`
token_value_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 관측

`0.42.78 -> 0.42.83` pair의 runtime ops bucket이다. baseline `0.42.78`이 설치된 상태에서 캡처했다. 0.42.78 pair 때(`installed-runtime-ops-summary-2026-09-25-04277`)도 baseline 설치본에서 캡처했다.

설치본 `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`가 보호된 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`다. token 값은 출력하거나 기록하지 않았다. CLI 출력에 token 형태 문자열(`eyJ`, `Bearer `)은 없었다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.78-admin-smoke` |
| service | `PureCVisorDesktopNode` Running / Automatic |
| Web Console `http://127.0.0.1/` | HTTP `200` |
| 비인증 `GET /api/v1/ops/summary` | HTTP `401` / `PCV_AUTH_REQUIRED` |
| token storage | `windows-credential-manager` |
| network exposure | `loopback` |
| console mode | `windows-hyperv-console-handoff` |
| distribution claim | `internal-only-not-public-release` |
| summary errors | `0` |
| VM total | `1` (`pcv-guest-installed-04253-r1` Off) |

## 아직 닫히지 않은 후속

| 후속 검증 | 결과 |
| --- | --- |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-run` |
| MSIX build/install/update/remove | `not-run` |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |

## Nonclaims

- 읽기 전용 캡처다. 설치본, 서비스, VM을 바꾸지 않았다.
- operational current는 `0.42.78-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
