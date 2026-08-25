# Archive Read-Only Rollback Evidence - 2026-05-04

evidence_id: archive-readonly-rollback-2026-05-04
created_at: 2026-05-04T14:03:00+09:00
archive_status: physical-archive-read-only
file_move_execution: pass-2026-05-05
archive_write_execution: pass-2026-05-05
source_inventory_status: pass
hash_inventory_status: pass
inventory_path: docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json
postmove_inventory_path: docs/ga-ready/evidence/archive-spikes-inventory-postmove-2026-05-05.json
rollback_restore_status: proof-defined
no_behavior_change_status: pass
public_trusted_signing: excluded
machine_readable_json_created: yes

## 범위

이 evidence는 `archive/spikes/**` 목표의 read-only intent와 rollback restore 기준을 정의한다. 2026-05-04 snapshot에서는 파일 이동을 실행하지 않았고, 2026-05-05 사용자 physical archive move opt-in 이후 `spikes/purecvisor-desktop-node/**`를 `archive/spikes/purecvisor-desktop-node/**`로 `git mv` 이동했다.

Public trusted signing은 scope 밖으로 고정한다. 이 evidence는 internal `RequireSigned`, `AllowUnsignedDev`, admin-smoke 개발 기준과 별개이며 public/stable signing blocker를 닫지 않는다.

2026-05-04T23:37:43+09:00 후속 slice에서 source/target/hash inventory를 `docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json`으로 생성했다. Inventory는 46개 physical `spikes/purecvisor-desktop-node/**` file의 source path, planned archive target, length, SHA-256을 기록한다. 이 inventory는 archive write나 파일 이동 실행이 아니다.

2026-05-05T01:12:00+09:00 후속 slice에서 이동 후 inventory를 `docs/ga-ready/evidence/archive-spikes-inventory-postmove-2026-05-05.json`으로 생성했다. Source path는 absent, archive target은 present, archived file count는 46개다. Pre-move target inventory match는 46개 모두 확인됐고, 경로 참조 갱신이 들어간 archive README/test 8개 file만 SHA-256 mismatch로 별도 기록했다. 이 mismatch는 이동 실패가 아니라 이동 후 문서/테스트 path update에 따른 content change다.

## Read-Only Intent

- `archive/spikes/**`는 historical/component baseline 보관용이다.
- `archive/spikes/**`는 product execution source로 사용할 수 없다.
- `archive/spikes/**`는 packaging input으로 사용할 수 없다.
- `archive/spikes/**`는 required verification command source로 사용할 수 없다.
- `archive/spikes/**`는 post-reboot product profile command source로 사용할 수 없다.
- Archive baseline을 참조하는 경우 evidence 문서에서 목적, 경로, owner, rollback 기준을 명시해야 한다.

## Rollback Restore Criteria

Archive migration slice에서 파일 이동 승인과 실행은 다음 기준으로 증명한다.

1. Source path inventory와 target archive path inventory를 남긴다. 현재 source/target inventory는 `archive-spikes-inventory-2026-05-04.json`에 기록됐다.
2. 이동 전 hash inventory를 남긴다. 현재 source hash inventory는 `archive-spikes-inventory-2026-05-04.json`에 기록됐고, 이동 후 hash inventory는 `archive-spikes-inventory-postmove-2026-05-05.json`에 기록됐다.
3. git tracked restore 기준을 남긴다.
4. product runtime, packaging, docs required command가 archive path를 참조하지 않음을 확인한다.
5. rollback restore는 git tracked restore 또는 승인된 archive-to-source restore plan으로만 수행한다.
6. rollback restore 후 packaging, installer, web, dotnet, npm, node check, git diff check evidence를 다시 남긴다.
7. host mutation, Hyper-V VM 생성/삭제, service install/delete, MSI install/uninstall, firewall/Event Log/trust store 변경은 rollback restore 기준 검증에 포함하지 않는다.

## No Behavior Change Criteria

Archive migration은 behavior 변경과 분리한다.

- 파일 이동 slice는 runtime logic, route behavior, service identity, listener policy, token storage, data-root deletion policy를 동시에 바꾸지 않는다.
- Archive target은 read-only baseline이어야 하며 product path fallback이 되면 안 된다.
- no behavior change evidence는 package tests, installer tests, web tests, `dotnet test`, npm parity, browser fixture, `node --check`, `git diff --check` 결과로 닫는다.
- destructive/admin smoke는 archive path 이동 자체의 기본 evidence가 아니다. Destructive boundary가 바뀌는 milestone에서만 별도 opt-in evidence로 실행한다.

## 판정

archive/read-only rollback proof는 source/target/hash inventory와 post-move archive inventory까지 기록됐다. 2026-05-05 physical archive move는 실행됐고 source path absent, archive target present, archived file count 46개로 확인됐다. Archive는 계속 product execution, packaging input, required verification command source로 사용할 수 없는 component/read-only baseline이다.
