# Installed operator surface current-card `0.42.84-admin-smoke` (2026-09-30)

evidence_id: `installed-operator-surface-current-card-2026-09-30-04284`
result: `PASS`
status: `installed-non-promoted-candidate`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260930-04284-r2`
summary_sha256: `4b5130ece499b7f11e62836a7aa0b88cf584cb631259afef7bd55684768e82f3`
version: `0.42.84-admin-smoke`
installed_product_version: `0.42.84-admin-smoke+ee90e0ea2c042e21cd0bf0346440ae2db41dfe43`
fullgate_batch: `full-admin-host-mutation-gate-20260930-04284`
clean_package_msi_sha256: `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87`
operational_fullgate_msi_sha256: `f9e1e341a3541d91feb63723e91bb4956be02d75fbad41dda3344b8f0e6a5482`
operator_surfaces: `web`, `cli`
tui_present: `false`
host_mutation_performed: `false`
secret_observed: `false`
promotion_ledger_status: `not-promoted`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 캡처

fullgate가 남긴 `0.42.84` 설치본을 그대로 두고 캡처했다. 캡처 스크립트는 0.42.83 current-card 스크립트에서 version, 경로, batch id, provenance, 기대 SHA 상수만 바꾼 것이고, artifact root에 함께 두었다.

첫 캡처(`artifacts/installed-operator-surface-current-card-20260930-04284`)도 판정은 `pass`였다. 하지만 스크립트에 남은 0.42.83 상수 때문에 summary의 기대 MSI SHA 두 개, payload aggregate 두 개, `canonical_current_evidence`가 틀렸다. 상수를 고쳐 `-r2`로 다시 캡처했고, 이 evidence는 r2를 기준으로 한다.

| 항목 | 결과 |
| --- | --- |
| CLI `host status`, `runtime policy`, `network inventory` | exit `0` 3개, JSON `ok` 3개, stderr 없음 |
| Web `/`, `/config.js` | HTTP `200` 2개, config에 token 없음 |
| service | Running / Auto, LocalSystem, credential manager target, token flag 없음 |
| ARP | `0.42.84` 1개 |
| 설치본 Host / CLI | fullgate operational payload와 같음 (`ec0f479e…` / `60d8922b…`) |
| TUI | 없음 |
| 테스트 VM | `0`개(보존 VM `pcv-guest-installed-04253-r1`만 남음) |
| secret 관측 | 없음 |

## 판정

current-card는 PASS다. 상태는 `installed-non-promoted-candidate`다. Lane 3 승격은 이 campaign의 승인 밖이라서 `promoted-current`로 바꾸지 않는다.

## Nonclaims

- operational current는 `0.42.83-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
