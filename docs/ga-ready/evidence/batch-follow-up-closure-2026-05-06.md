# Batch 후속 작업 종료 정리 - 2026-05-06

## 요약

2026-05-06 후속 작업은 Batch 2 evidence API/Web hardening과 Batch 4 internal `RequireSigned` gate prep을 PR로 병합하고, Batch 3-A/3-B Web evidence dashboard/troubleshooting surface의 계획 상태를 완료로 정리했다.

## 병합된 PR

| Batch | PR | 구현 commit | merge commit | 결과 |
| --- | --- | --- | --- | --- |
| Batch 2 evidence API hardening | `#4` `Harden batch evidence summary degradation` | `c3163e23fad504677aac5d55f07c8124b9fb4d56` | `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940` | 병합 완료 |
| Batch 4 internal `RequireSigned` gate prep | `#5` `Harden internal RequireSigned gate` | `97b6fd892eca874486efdc6cd09cea9247c0c910` | `d9c833e70834647e6ff907ac6dc48745dcdf2adf` | 병합 완료 |

## Batch 2 결과

`ops.summary.batch_evidence`는 child evidence가 누락, malformed, unreadable, containment rejected 상태일 때 route failure로 승격하지 않고 `status="degraded"`와 sanitized `PCV_BATCH_EVIDENCE_*` error를 반환한다.

응답은 configured evidence root, repository root, stdout/stderr, raw command arguments, bearer token, API token 값, protected token file path/content를 노출하지 않는 contract로 강화됐다.

## Batch 3-A/3-B 결과

Web Console은 evidence view와 dashboard badge, troubleshooting/incident triage surface에서 `available`, `missing`, `unavailable`, `degraded`, `not_configured` 상태를 표시한다. 이 정리는 기존 `ops.summary.batch_evidence` read-only payload를 소비하며 새 host mutation route나 evidence path input을 추가하지 않는다.

## Batch 4 결과

Installer gate는 `rc`와 `stable` artifact에서 `AllowUnsignedDev`를 거부하고, `RequireSigned` build에 명시적 `SigningTrustModel`을 요구한다. `InternalEnterprise` provenance는 `signing_trust_model=InternalEnterprise`와 `msi.signed=true`를 남기며 certificate secret, private key/PFX/password, token material을 dry-run/provenance output에 기록하지 않는다.

Runbook은 `New-PcvInternalCodeSigningTrust.ps1 -DryRun` plan-only check와 실제 `LocalMachine` trust import, signed MSI build, elevated MSI lifecycle smoke를 분리한다.

## 후속 plan drift closure

2026-05-06 추가 후속 정리는 main에 이미 반영된 Batch 1 canonical evidence closure와 Batch Supervisor evidence UX/API foundation plan의 checkbox 상태를 현재 산출물 기준으로 닫았다.

- Batch 1 canonical evidence closure plan은 작성 당시 `0.38.2-admin-smoke` standalone canonical evidence, `0.38.1` historical-only ledger/reference, high-level docs guard, API/Web fixture 갱신 상태를 반영한다. 이후 실제 실행 evidence는 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-08-0389.md`가 `0.38.9-admin-smoke` full admin host mutation PASS 기준을 승계했고, internal signed build 기준은 후속 `0.38.7-rc.1` evidence가 승계했다.
- Batch Supervisor evidence UX/API foundation plan은 `ops.summary.batch_evidence`, Host `--batch-evidence-root`, Web API type, verification policy/README boundary가 main에 반영된 상태를 반영한다.
- Historical `0.38.0` next-batch sequence plan의 세부 checkbox는 superseded 항목이라 완료 처리하지 않고, 상단 closure update만 current 상태로 유지한다.

## 검증 evidence

Batch 2 검증:

- `dotnet test src\DesktopNode.sln`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`
- `npm test --prefix web`
- `npm run verify:parity --prefix web`
- `node --check web\app.js`
- `git diff --check`

Batch 4 검증:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 39 passed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 143 passed
- `git diff --check`

## 경계

이 closure는 실제 Hyper-V VM 생성, service install/start/stop/delete, MSI install/repair/uninstall, firewall mutation, Event Log mutation, trust-store mutation, LAN exposure, signed MSI build, LocalMachine trust import, public trusted signing, 외부 stable publication을 실행하거나 주장하지 않는다.

이 closure 작성 당시에는 `0.38.2-admin-smoke` full admin host mutation gate가 최신 canonical admin evidence였고, 이 문서는 그 evidence를 대체하지 않고 Batch 2/4 후속 code/docs 병합 상태만 기록했다. 2026-05-08 후속 실행으로 최신 full admin host mutation 기준은 `0.38.9-admin-smoke`로 갱신됐고, 2026-05-07 후속 실행으로 최신 internal enterprise `RequireSigned` build 기준은 `0.38.7-rc.1`로 갱신됐다.
