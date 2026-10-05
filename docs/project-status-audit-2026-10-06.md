# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-10-06)

- 기준: `main` `6796572`, operational current `0.42.90-admin-smoke`(release train `0.42.90` 승격, PR #50 merge `b88ef11`)
- 범위: 완료 층 네 개와 `docs/SERVICE_PLAN.md` §7.1 P0~P2 `15`개 항목. 읽기만 했다.
- 목적: "프로젝트 완료" 정의(`docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-completion-definition-design.md`)의 입력
- 직전 감사: `docs/project-status-audit-2026-10-03.md`

## 1. 결론

완료 층 세 개(제품 런타임 GA-ready, operational current, 기능 승격 후보)는 닫혀 있다. SERVICE_PLAN P0~P2 `15`개는
모두 설계가 구현됐고(`implemented-slice-*`), 설치본 actual-VM PASS evidence로 닫힌 항목이 `10`개다. 남은 것은 설치본
evidence가 없는 항목 `4`개(P1-6, P1-7, P1-9, P2-11)와 일부만 있는 항목 `1`개(P1-10)다. 새 기능 구현은 남아 있지 않다.

## 2. 완료 층

| 층 | 완료 조건 | 상태 | 근거 |
| --- | --- | --- | --- |
| 제품 런타임 GA-ready | GA 범위 current route·product operation에 transition-helper·blocked 행 `0`, 제품 경로 PowerShell `0`, tier2/3 mutation 관리자 evidence 최신 | 닫힘 | ADR-0004 (2026-05-05 aggregate gate closure) |
| operational current | package, pair, fullgate, current-card, Lane 3가 한 version에 모이고 설치본과 HEAD가 정렬 | 닫힘 | `docs/ga-ready/current-evidence.json` `0.42.90-admin-smoke`, 설치본 `+648139d`, release train `queue` `0` |
| 기능 승격 | 후보 feature의 필수 stage 다섯 개 pass | 닫힘(후보 `4/4`) | ADR-0015, `config/desktop-node-feature-evidence-ledger.json` |
| 서비스 기획 | SERVICE_PLAN §9 성공 기준, §7.1 항목마다 설계 → code → 설치본 evidence | 부분 | §3 |

기능 승격 층의 비후보 `24`개는 `not-assessed`다. 2026-10-03 결정
(`docs/superpowers/specs/2026-10-03-purecvisor-desktop-node-feature-stage-observation-decision.md`)으로 ledger 모델을
넓히지 않았고, 비후보의 설치본 PASS는 `docs/FEATURE_IMPLEMENTATION_LEDGER.md` "비후보 actual-VM 관측" 절에 operation 단위로 적는다.

## 3. SERVICE_PLAN P0~P2

판정: `설치본 PASS`(설치본 actual-VM evidence 있음), `부분`(일부 operation만 설치본 evidence), `code-level만`(설계·코드·시험은 있고 설치본 evidence는 찾지 못함).

| # | 항목 | 설계 | 코드 | 설치본 actual-VM evidence | 판정 |
| ---: | --- | --- | --- | --- | --- |
| P0-1 | 미디어 재장착 | 2026-08-14 | `vm.attach`, code-level evidence `service-plan-p0-media-attach-code-level-2026-08-14` | `service-plan-p0-actual-vm-2026-08-27-04275`, `lane2-vm-media-eject-attach-2026-10-02-04286` | 설치본 PASS (후보 `pcv.vm.media-attach`) |
| P0-2 | checkpoint restore 추적 | `2026-08-14-...-p0-checkpoint-restore-reconciliation-design` | `checkpoint.restore` reconcile | `service-plan-p0-actual-vm-2026-08-27-04275` | 설치본 PASS (후보 `pcv.checkpoint.restore`) |
| P0-3 | Hyper-V Saved | 2026-08-14 | `vm.save`, `vm.resume-saved` | `service-plan-p0-actual-vm-2026-08-27-04275` (04274는 `vm.save` FAIL, 04275에서 PASS) | 설치본 PASS (후보 `pcv.vm.saved-lifecycle`) |
| P0-4 | 기존 VM 들이기 | 2026-08-14 | `vm.manage` | `service-plan-p0-actual-vm-2026-08-27-04275` | 설치본 PASS (후보 `pcv.vm.managed-import`) |
| P1-5 | managed full clone | `2026-08-27-...-p1-managed-full-clone-design` | `vm.clone` | `service-plan-p1-clone-actual-vm-2026-08-29-04277-r2` | 설치본 PASS |
| P1-6 | inventory 시각·메모 | `2026-09-20-...-p1-inventory-timestamps-design` (`implemented-slice-2`) | `vm.list` `last_powered_on` 등 | 찾지 못함 | code-level만 |
| P1-7 | template lock | `2026-09-20-...-p1-template-lock-design` (`implemented-slice-3`) | `vm.template.lock`, reconcile handler | 찾지 못함 | code-level만 |
| P1-8 | 제한적 guest 파일 job | `2026-09-20-...-p1-guest-file-job-design` (`implemented-slice-4`) | `vm.guest.file` | `service-plan-p1-guest-file-actual-vm-2026-09-28-04282` | 설치본 PASS |
| P1-9 | admin account CRUD | `2026-09-20-...-p1-admin-account-crud-design` (`implemented-slice-4`) | `account.list`, `account.create`, `account.disable` | 찾지 못함 | code-level만 |
| P1-10 | 나머지 조건부 reconcile | `2026-09-20-...-p1-remaining-reconcile-design` (`implemented-slice-4`) | reconcile 대상 `26`, 비대상 `19` 분류 | 전원 상태 reconcile `postcondition-confirmed`와 비대상 거절(`lane2-development-completion-actual-vm-2026-09-30-04284`), 비대상 안내 문구(`lane2-reconcile-wording-actual-vm-2026-10-03-04287`) | 부분 (family별 create·shutdown·restart·QoS 설치본 evidence는 찾지 못함) |
| P2-11 | noVNC target 설정 | `2026-09-21-...-p2-novnc-target-config-design` (`implemented-slice-4`), ADR-0010 적용 중 | `console.novnc-target.preview`, `.set`, `.clear` (Web 입력은 정책상 두지 않음) | 찾지 못함 | code-level만 |
| P2-12 | 주기 checkpoint | `2026-09-21-...-p2-periodic-checkpoint-design` (`implemented-slice-5`) | `checkpoint.schedule.set`, `.clear` | `service-plan-p2-offvm-checkpoint-schedule-actual-vm-2026-09-27-04278` | 설치본 PASS |
| P2-13 | Hyper-V export/import | `2026-09-21-...-p2-hyperv-export-import-design` (`implemented-slice-5`) | `vm.export`, import | `service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04281` | 설치본 PASS |
| P2-14 | 네트워크 변경 | `2026-09-21-...-p2-network-change-design` (`implemented-slice-5`) | `vm.network.connect` (switch·NAT·DHCP 편집은 비목표) | `service-plan-p2-offvm-network-connect-actual-vm-2026-09-28-04281` | 설치본 PASS |
| P2-15 | NIC/DVD 추가 | `2026-09-24-...-p2-nic-dvd-add-design` (`implemented-slice-2`) | `vm.device.add` | `service-plan-p2-offvm-device-add-actual-vm-2026-09-27-04278-r2` | 설치본 PASS |

합계: 설치본 PASS `10`, 부분 `1`, code-level만 `4`. 미착수 `0`.

"찾지 못함"은 evidence 파일 이름과 본문에서 해당 operation 이름(`vm.template.lock`, `account.create`, `account.disable`,
`novnc-target`, `last_powered_on`)을 찾지 못했다는 뜻이다. 다른 이름으로 기록된 evidence가 있을 수 있다.

## 4. 기한이 있는 위험

| 항목 | 기한 | 상태 |
| --- | --- | --- |
| GitHub `ubuntu-latest`가 Ubuntu 26으로 이동 | 2026-10-19 | 2026-10-03 probe에서 Public Boundary와 web job 통과. 이동 뒤 첫 `main` run 확인이 남음 |

## 5. 영구 범위 밖

2026-10-06 사용자가 다시 정했다: public trusted signing과 external stable publication은 하지 않는다(ADR-0004, ADR-0006과 같다).
Workstation 기능 패리티, Hyper-V exactly-once 보장도 목표가 아니다(SERVICE_PLAN §10). 이 항목들은 남은 일로 세지 않는다.

## 6. Nonclaims

- 이 감사는 읽기만 했다. host, 설치본, service, VM, `current-evidence.json`을 바꾸지 않았다.
- GA를 선언하지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
