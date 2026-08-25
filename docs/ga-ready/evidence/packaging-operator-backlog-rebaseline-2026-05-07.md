# Packaging/Operator 후속 범위 재기준화 - 2026-05-07

```text
evidence_id: packaging-operator-backlog-rebaseline-2026-05-07
```

## 요약

2026-05-07 후속 재점검은 `packaging/distribution future phase`와 `Operator/Web UX 확장`을 같은 잔여 작업 목록으로 섞지 않도록 분리한다.

- Packaging/distribution future phase는 현재 내부 전용 Desktop Node 제품 런타임의 완료 조건이 아니다.
- Packaging/publication descriptor는 code-level로 추가됐지만, 실제 Burn/MSIX/winget/publication 실행을 뜻하지 않는다.
- Operator/Web UX 확장은 P0/P1/P2, VM delete UI, Web Dashboard Ops Cockpit, API Operations Hardening P2, read-only Network Inventory view, Diagnostic Bundle operator handoff UI, job/activity retention 및 pagination hardening, Token Rotation operator UX가 구현 완료 상태다.
- 이 문서는 실제 Hyper-V, service/MSI, firewall, trust-store, LAN, Event Log, signed build, updater, rollback, credential store mutation을 실행하지 않는다.
- 이 문서는 public trusted signing 또는 외부 stable publication evidence가 아니다.

## Packaging/distribution future phase

다음 항목은 현재 route matrix의 GA-scope current row가 아니며, 별도 future spec/plan/ADR 또는 release approval 없이는 구현 완료로 주장하지 않는다.

| 후보 | 현재 상태 | 다음 조건 |
|------|-----------|-----------|
| Burn bootstrapper | future-noncurrent | MSI chain/bootstrapper 설치 경계, rollback/final-state proof, 관리자 opt-in smoke를 별도 plan으로 정의 |
| MSIX | future-noncurrent | AppX/MSIX identity, service install 가능성, certificate/trust-store boundary, MSI와의 coexistence 정책 필요 |
| winget manifest | future-noncurrent | public/external publication 여부 ADR, package identifier, installer URL/hash/source policy 필요 |
| network download updater | catalog-channel-code-level-partial | file/HTTPS ZIP source gate, SHA-256 verification, extract-before-service-stop, `PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED` HTTP block은 `docs/ga-ready/evidence/network-download-update-source-gate-2026-05-07.md`에 구현 evidence를 기록했다. file/HTTPS JSON catalog/channel resolver와 package SHA-256 handoff는 `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`에 code-level evidence를 기록했다. External publication service, public trusted signing, 외부 stable publication, installed destructive catalog update smoke는 아직 future-noncurrent |
| full transactional rollback | filesystem-rollback-code-level-partial | update payload validation 직후 service stop 전에 `update-transaction.begin` journal을 쓰고 success/`failed-rolled-back`/`PCV_*` error diagnostics를 기록하는 단일 active journal은 `docs/ga-ready/evidence/update-transaction-journal-diagnostics-2026-05-07.md`에 구현 evidence를 기록했다. Product root backup 이후 copy/config/start/health failure에서 previous root restore를 시도하는 filesystem rollback은 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`에 code-level evidence를 기록했다. Post-crash resume/reconcile, service/data/config/job-store transaction manager, installed destructive smoke는 아직 future-noncurrent |
| packaging/publication descriptor | descriptor-code-level-partial | installer build output은 `.publication.json` sidecar를 작성하고 public trusted signing/external stable publication `not-claimed`, Burn/MSIX `not-built`, winget `not-generated`, catalog publication `not-published`를 기록한다. Evidence는 `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`다. 실제 Burn/MSIX/winget artifact generation, external publication service, public stable channel publication은 아직 future-noncurrent |
| Windows Credential Manager transition | future-noncurrent | DPAPI protected token file과의 migration/rollback/redaction contract 필요 |
| default Windows Event Log writer/provider transition | future-noncurrent | JSONL-first primary 정책 대체 여부, provider registration lifecycle, retention/query evidence 필요 |
| built-in TLS certificate lifecycle | future-noncurrent | local certificate issuance/renewal/removal, private key protection, LAN exposure approval boundary 필요 |

현재 내부 전용 서비스 증거는 WiX MSI, internal `RequireSigned` build, `AllowUnsignedDev` admin-smoke, internal stable release/update/rollback, native Event Log source registration/removal action evidence, network update source gate code-level evidence, updater catalog/channel resolver code-level evidence, update transaction journal diagnostics code-level evidence, update filesystem rollback code-level evidence, packaging publication descriptor code-level evidence에 머문다. 위 future phase는 외부 배포 또는 public stable publication을 자동으로 열지 않는다.

## Operator/Web UX 확장

Operator/Web UX 계획 문서는 현재 모두 checkbox closure 상태다.

| 계획 | 현재 상태 | 체크박스 |
|------|-----------|----------|
| `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p0.md` | implemented | unchecked 0 |
| `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p1.md` | implemented | unchecked 0 |
| `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p2.md` | implemented | unchecked 0 |
| `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-vm-delete-ui.md` | implemented | unchecked 0 |
| `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-web-dashboard-ops-cockpit-redesign.md` | implemented | unchecked 0 |
| Web Console `/api/v1/network/inventory` 상세 inventory page | implemented | evidence `docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md` |

현재 Web Console coverage는 Ops Cockpit, VM Workbench, read-only Network Inventory, Incident Command, Diagnostic Bundle operator handoff, Token Rotation operator UX, VM delete queued job UI, paged server-side job snapshot, request/correlation id 표시, runtime policy 기반 monitoring/troubleshooting 요약을 포함한다.

남은 UX 후보는 새 backlog이며 기존 P0/P1/P2 closure의 미완료 항목이 아니다.

- diagnostic bundle server-side collection/download action
- timeout/rate-limit hardening
- service token rotation/revoke mutation API
- checkpoint retention bulk delete workflow

2026-05-07 후속 slice에서 server-wide job/activity retention policy와 pagination hardening은 `docs/ga-ready/evidence/api-web-retention-pagination-hardening-2026-05-07.md`로 closure했다. `GET /api/v1/jobs`는 bounded `limit`/`offset` page와 retention metadata를 제공하고, terminal job은 최신 500개만 보존하며 active job은 보존한다. Timeout/rate-limit policy는 아직 별도 backlog다.

2026-05-07 후속 slice에서 token rotation/revoke UX는 `docs/ga-ready/evidence/web-console-token-rotation-ux-2026-05-07.md`로 closure했다. Web Console은 protected token file root, runtime policy token storage, browser token presence, `Clear browser token`, `rotation handoff`, `no service token mutation` 경계를 표시한다. 실제 service token file rotation/revoke mutation, service restart, Windows Credential Manager transition은 아직 별도 backlog다.

## 검증

이 재기준화의 검증 owner는 문서 동기화 guard다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
git diff --check
```
