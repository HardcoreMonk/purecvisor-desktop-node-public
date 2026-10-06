# Fullgate MSI uninstall 대기 F1 확인 2026-10-07 `0.42.91`

evidence_id: `fullgate-msi-uninstall-f1-check-2026-10-07-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
lane: `2`
working_authority: `source_head`
version: `0.42.91-admin-smoke`
host_mutation_performed: `true`
current_evidence_written: `false`
promotion_ledger_status: `observation-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
baseline_batch: `full-admin-host-mutation-gate-20261006-04291`
batch_id: `full-admin-host-mutation-gate-20261007-04291`
artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261007-04291`
campaign: `process-followups-20261007` Task 1, Task 2

## 범위

2026-10-06 개발 공정 점검(F1)은 fullgate `msi-lifecycle-smoke`의 uninstall 대기가 2026-09-30 `40`초에서 2026-10-06 `340`초로
늘었고, 그 증가가 개발 호스트에 쌓인 headless Edge(`pcv-loopback-browser-*`) 수와 같이 움직인다는 상관관계를 봤다. 원인은
`DesktopNodeHostLoopbackBootstrapBrowserTests`의 정리 누락이었고 PR #57이 고쳤다. 남은 인스턴스 `47`개(프로세스 `470`개)는
2026-10-06에 정리했다.

이 확인은 사용자 승인 `1,2,3,4,5`의 2(2026-10-07, "지금 fullgate 재실행")로 같은 `0.42.91` 제품 payload에서 fullgate의
`service-msi-hyperv-admin-smoke` 단계만 다시 돌려 uninstall 시간을 쟀다. os-mutation 단계는 firewall 규칙·Event Log source·LAN
listener를 바꾸므로 승인 범위 밖이라 돌리지 않았다. operational current와 `current-evidence.json`은 바꾸지 않았다.

## 사전 상태

| 항목 | 값 |
| --- | --- |
| `pcv-loopback-browser` Edge | `0` |
| 호스트 전체 프로세스 | `539` (2026-10-06 점검 때 `1059`) |
| switch | `Default Switch` |
| ARP | `{ED64B13A-742B-422C-9142-DED650CB856B}` `0.42.91` 1개 |
| 설치본 | `0.42.91-admin-smoke+990a4b2`, service Running/Auto |
| PureCVisor firewall 규칙 | `0` |
| VM | `pcv-guest-installed-04253-r1` Off 하나 |

## 실행

- manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`가 만들었다(첫 실제 사용). 첫 단계만 남겼고, LAN prefix는 쓰지
  않으므로 문서용 주소를 넣었다.
- supervisor `-DryRun` 통과 뒤 `-AllowHostMutation` 실행: `ok=true`, `status=completed`, 시도 `1`.
- gate build는 clean HEAD `2e05cc1`이다. 제품 payload 경로는 `0.42.91`(`05f42a2`)과 같다.

## 결과

| 측정 | 2026-10-06 (`20261006-04291`) | 2026-10-07 (`20261007-04291`) |
| --- | ---: | ---: |
| `service-msi-hyperv-admin-smoke` 단계 | `802.8`초 | `321.3`초 |
| `msi-lifecycle-smoke` | `710`초 | `232`초 |
| uninstall-preserve | `340`초 | `91`초 |
| uninstall-remove-data | `342`초 | `90`초 |
| install / repair / install-remove-data / final-restore-install | `5` / `2` / `4` / `5`초 | `18` / `5` / `8` / `8`초 |

출처는 둘이다. 단계 시각은 route parity `summary.json`의 단계 timestamp, uninstall 시간은 각 msiexec 로그의 첫 줄과 마지막
줄 시각이다.

- uninstall 두 번이 각각 약 `250`초 줄었다. fullgate 단계는 `481.5`초(`60%`) 줄었다.
- 두 uninstall 모두 아직 files-in-use로 `DesktopNode.Host.exe` 1개를 보고한다(Restart Manager가 꺼져 있어 기본 FilesInUse 처리를
  쓴다). 2026-09-30의 `40`초까지는 돌아가지 않았다. 남은 대기는 설치된 service process 자체에서 오는 것으로 보이며 이 확인의
  범위 밖이다(report-only).
- 결론: 누수 Edge 정리 뒤 같은 payload의 uninstall 대기가 크게 줄었으므로 F1의 원인 판단을 지지한다. 단일 run 비교라
  프로세스 수 외의 차이(설치 시간 증가 등)는 통제하지 않았다.

## 사후 검사와 복원

- fullgate 직후: 같은 version ARP `{8D07CE80-3D24-4C2F-B3E2-00035653992A}` 1개, 설치본 `+2e05cc1` == gate build, PureCVisor
  firewall 규칙 `0`, VM은 보존 VM Off 하나, service Running/Auto.
- 복원(Task 2): 2026-10-06 `final-restore-install`과 같은 속성으로 `0.42.91` 운영 MSI(`46dddccb…`)를 설치했다. msiexec exit `0`.
  끝 상태는 ARP `{ED64B13A-…}` 1개, 설치본 Host `a935701e…`·CLI `2c249f0d…`(0.42.91 fullgate payload와 같음),
  `+990a4b2`, service Running/Auto, batch root `full-admin-host-mutation-gate-20261006-04291`, Web `200`, firewall 규칙 `0`이다.
  설치본은 operational current와 같다.

## Nonclaims

- 관측이다. operational current(`0.42.91-admin-smoke`), feature 승격, `current-evidence.json`을 바꾸지 않는다.
- os-mutation 단계는 돌리지 않았으므로 이 batch는 full admin host mutation gate PASS가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
