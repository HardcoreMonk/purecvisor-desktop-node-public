# 한국어 문서 재작성 롤아웃

작성 기준: 2026-05-11
마지막 갱신: 2026-08-25

이 저장소의 모든 신규/수정 문서는 한국어 본문을 기본으로 작성한다. 코드 식별자, 명령어, 파일 경로, API route, product/version/evidence id, 검증 fixture token은 원문을 유지한다. 영어 문구를 새로 추가해야 하는 경우에는 운영자가 읽을 한국어 설명을 함께 둔다.

## 현재 확인

2026-05-11 단순 Markdown 언어 스캔 기준으로 Markdown 문서 280개 중 다수가 영어 중심 문서였다. 특히 `docs/superpowers/plans/`, `docs/ga-ready/evidence/`, `src/DesktopNode.Cli/README.md`, `src/DesktopNode.Tui/README.md`, GA-ready 인덱스 문서에 영어 본문이 남아 있었다.

2026-08-25 전수 검사는 tracked Markdown `761`개(`9,413,145` bytes)를 읽었다. 분류는
historical evidence `494`, plan `125`, spec `77`, 그 밖의 current/supporting 문서 `65`개다.
2026-05-11 수치는 당시 snapshot으로 보존한다. TUI 문서는 이후 ADR-0011에 따라 active
source/package와 함께 제거됐으므로 새 번역 대상이 아니라 historical predecessor다.

역사 evidence와 phase plan은 테스트나 감사 기록이 참조하는 정확한 문구, id, 파일명, artifact root를 포함한다. 따라서 한 번에 전체 문서를 기계적으로 번역하지 않고, 현재 운영자가 먼저 읽는 문서부터 한국어로 재작성한다.

## 우선순위

1. 활성 진입점과 운영자 문서: `README.md`, `AGENTS.md`, `docs/USER_GUIDE.md`, `docs/CLI_COMMAND_USAGE.md`, `docs/OPERATIONS_GUIDE.md`, `docs/DEVELOPER_INDEX.md`, `docs/OPERATOR_SURFACE_TERMS.md`, `src/DesktopNode.Cli/README.md`; TUI 경계는 `docs/adr/0011-cli-web-only-operator-surface.md`
2. GA-ready control plane 문서: `docs/ga-ready/*INDEX*.md`, route/distribution/verification matrix
3. ADR과 저장소 경계 문서: `docs/ADR_INDEX.md`, `docs/adr/`, `docs/PUBLIC_RELEASE_BOUNDARY.md`
4. 현재 evidence 문서: `docs/ga-ready/EVIDENCE_INDEX.md`가 가리키는 current/supporting evidence
5. 역사 phase/spec 문서: `docs/superpowers/plans/`, `docs/superpowers/specs/`
6. Archive README와 spike 문서: `archive/spikes/**/README.md`

## 1차 적용 범위

이번 1차 재작성은 다음을 한국어 기준으로 맞춘다.

- 운영자 용어 계약: `docs/OPERATOR_SURFACE_TERMS.md`
- 설치/운영 진입점 문구: `docs/USER_GUIDE.md`, `docs/CLI_COMMAND_USAGE.md`, `docs/USER_FEATURE_USAGE_SPEC.md`
- CLI component README와 historical TUI 제거 경계: `src/DesktopNode.Cli/README.md`, `docs/adr/0011-cli-web-only-operator-surface.md`
- GA-ready 진입점: `docs/ga-ready/CONTROL_PLANE_INDEX.md`, `docs/ga-ready/EVIDENCE_INDEX.md`
- 저장소 규칙과 인덱스 링크: `AGENTS.md`, `README.md`, `docs/DEVELOPER_INDEX.md`
- 위 문구를 감시하는 CLI/Web static tests

## 2차 적용 범위

이번 2차 재작성은 GA-ready control plane에서 운영자가 현재 판단에 바로 쓰는 문서를 우선 정리한다.

- GA-ready matrix: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`, `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
- 경계 baseline: `docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md`, `docs/ga-ready/hyperv-domain-baseline-2026-05-11.md`, `docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md`
- ADR: `docs/ADR_INDEX.md`, `docs/adr/0002-release-version-policy.md`, `docs/adr/0004-ga-ready-product-runtime-candidate.md`, `docs/adr/0005-public-distribution-operations-expansion-candidate.md`, `docs/adr/0006-internal-private-network-distribution.md`
- 현재 evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`, `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`, `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md`
- 문서 guard: `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`의 GA-ready route matrix 기대값을 현재 served `GET /api/v1/jobs` row, account/RBAC/JWT auth surface와 맞춘다.
- Matrix schema: `ROUTE_PROMOTION_MATRIX.md`의 Field Schema enum과 network exposure gate 규칙을 현재 noVNC WebSocket bridge row의 `dotnet-host-listener`, `websocket-to-vnc-tcp-bridge`, `lan-exposure-approval-required` 값과 맞춘다.

Matrix의 field schema, enum, evidence id, route, command, artifact path, SHA, status token은 machine-readable contract와 test guard가 참조하므로 번역하지 않는다. 사람에게 의미를 전달하는 제목, heading, 설명 문장은 한국어 우선으로 맞춘다.

## 남은 영문 Historical 문서 재작성 우선순위

Stabilize Then Split 후속 개발 중에는 history를 삭제하지 않고, 운영자가 현재 판단에 다시 쓰는 순서대로 한국어 재작성한다.

1. GA-ready historical evidence 중 current index가 직접 가리키는 파일: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`, `docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`
   - 2026-05-12 후속: `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md`, `msix-package-lifecycle-smoke-2026-05-10-0416.md`의 운영자 본문을 한국어로 재작성했다. `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`는 이미 한국어 본문 기준을 충족한다.
2. ADR-0005 closed-not-adopted public distribution evidence: public signing/winget/external publication blocker 의미가 흐려지면 안 되므로 `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`가 참조하는 evidence를 우선한다.
3. 2026-05-11 implementation plan 계열: `docs/superpowers/plans/2026-05-11-purecvisor-desktop-node-runtime-core-boundary.md`, `docs/superpowers/plans/2026-05-11-purecvisor-desktop-node-hyperv-domain-split.md`, `docs/superpowers/plans/2026-05-11-purecvisor-desktop-node-host-ops-domain-split.md`, `docs/superpowers/plans/2026-05-11-purecvisor-desktop-node-operator-surfaces-alignment.md`
4. Phase 24/25 old plan/spec 계열: ADR 현재 결정과 충돌하지 않도록 historical marker를 붙인 뒤 한국어 요약을 추가한다.
5. Archive/spike README: active product path가 아니라 historical/read-only임을 첫 문단에 한국어로 표시한다.

각 historical 문서는 원문 artifact id, SHA, command, route, version token을 보존한다. 본문 의미를 바꾸는 재작성은 새 evidence로 취급하지 않고, 최신/current로 승격하지 않는다.

## 재작성 규칙

- 한국어 문장을 기본으로 하되 `Local API`, `Web Console`, `TUI`, `CLI`, `Hyper-V`, `DPAPI`, `JWT`, `RBAC`, `Diagnostic bundle` 같은 제품 용어는 기존 operator surface와 맞춘다.
- Public distribution 관련 문구는 번역 중에도 의미를 약화하지 않는다. `public trusted signing`, `winget public submission`, `external stable publication`은 새 ADR 전까지 계속 범위 밖이다.
- Evidence 파일의 artifact id, SHA-256, commit hash, route, command, `PCV_*` code는 그대로 둔다.
- 문서 재작성과 별개로 host mutation, MSI install/uninstall, firewall/trust-store/Event Log 변경은 실행하지 않는다.

## 검증

문서 재작성 후 최소 검증은 다음을 우선한다.

```powershell
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --filter UsageAndSharedTermsDescribeVmDeleteConfirmation
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
node web/scripts/verify-web-contract-registry.mjs
git diff --check
```
