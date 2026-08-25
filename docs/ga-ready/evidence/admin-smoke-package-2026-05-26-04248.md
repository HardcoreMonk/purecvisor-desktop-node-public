# Admin-smoke 패키지 2026-05-26 0.42.48

evidence_id: `admin-smoke-package-2026-05-26-04248`
result: `PASS`
scope: `internal-admin-smoke-phase3-web-tui-qos-direct-control`
version: `0.42.48-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260526-04248`
msi_sha256: `a0014960979ed23cec8d882cddd22baaaf9435a71287bdc133a79ff0b381338c`
payload_aggregate_sha256: `2013756155ce1d744ab4383ffdb70dfcc6d9d7c462192b51f4425f921a53850a`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `3d650b7abd026427956c0d8192e02eea5c73380a21734c323439535ccb665243`
cli_sha256: `99258898bb0f904f6ae5d14a4bf4c77d20a8ac47d46d9c4689ccb00574ef62be`
tui_sha256: `e3359707bcd918c81eb858d429b825c6f62d490ef8568c192a1ff19cb2ef4a71`
provenance_commit: `46e745efc698a06e4b065a19c3f07217e821155e`
build_utc: `2026-05-26T08:24:19.6640637Z`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Phase 3 Web/TUI QoS direct control을 설치본 payload로 승격하기 위한
`0.42.48-admin-smoke` clean package build 기록이다. Web Console selected VM storage/network
QoS preview/apply form, TUI selected VM QoS reset preview/apply confirmation, ADR-0009/ADR-0010
deferred boundary copy가 payload에 포함된다.

## Operational full-gate package 구분

Full admin host mutation gate는 같은 commit에서 route parity artifact 안에서 MSI를 다시
빌드했고, 실제 설치와 current-card smoke는 아래 operational full-gate package를 기준으로
통과했다.

| 항목 | 값 |
| --- | --- |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04248` |
| operational MSI SHA-256 | `a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf` |
| operational payload aggregate SHA-256 | `2a14e47bf3fd48b17755ce901ec02b924ba9246ecbe91414f952428ca376d92f` |
| operational build UTC | `2026-05-26T08:26:24.4872116Z` |
| operational signing trust model | `Unspecified` |

Clean package와 operational full-gate package는 같은 provenance commit
`46e745efc698a06e4b065a19c3f07217e821155e`에서 생성됐지만 MSI container hash는 빌드 시각과
Wix output 차이로 다르다. Current installed evidence는 operational full-gate MSI hash를
release anchor로 사용한다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.48-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260526-04248 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`: exit `0`
- Full admin host mutation gate: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04248/summary.json`, `PASS`
- 설치본 Web/TUI/CLI pre-closure current-card smoke: `artifacts/installed-operator-surface-current-card-20260526-04248/summary.json`, `PASS`
- Manual-admin closure 후 설치본 Web/TUI/CLI current-card smoke:
  `artifacts/installed-operator-surface-current-card-20260526-04248-manual-admin/summary.json`,
  `PASS`
- Phase 3 code-level evidence: `docs/ga-ready/evidence/phase3-web-tui-qos-direct-control-code-level-2026-05-26.md`, `PASS`
- Manual-admin package-pair closure:
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`, `PASS`

## 경계

이 package build는 internal admin-smoke evidence다. `0.42.47-admin-smoke ->
0.42.48-admin-smoke` manual-admin package-pair campaign은
`manual-admin-campaign-descriptor-20260526-04247-04248-closed`로 닫혔다. 이전
`0.42.45-admin-smoke -> 0.42.47-admin-smoke` closure는 historical predecessor로 보존한다.
Public trusted signing, public stable installer URL, winget submission, 외부 stable
publication은 주장하지 않는다.
