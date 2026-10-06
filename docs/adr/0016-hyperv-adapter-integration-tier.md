# ADR-0016: Hyper-V 어댑터 integration 단계와 접두사 VM standing approval

상태: 채택 / 단계 2 구현 중
일자: 2026-10-07

## 결정 마커

```text
DESKTOP_NODE_HYPERV_INTEGRATION_TIER_DECISION: in-process-production-wmi-adapter-opt-in
DESKTOP_NODE_HYPERV_INTEGRATION_SCOPE: pcv-it-prefixed-vm-create-configure-delete-only
DESKTOP_NODE_HYPERV_INTEGRATION_REQUIRED_CI: excluded
DESKTOP_NODE_HYPERV_INTEGRATION_PROMOTION_EVIDENCE: observation-only
```

## 맥락

2026-09-27부터 2026-10-06까지 제품 수정 `9`건은 모두 설치본이나 actual VM 단계에서 발견됐다. `HyperV.Tests`는 WMI를
가짜 객체 경로로 시험하고, route 시험의 가짜 값(`off`)이 inventory의 실제 어휘(`stopped`)를 가린 사례도 있다(`9402774`).
발견 루프는 release train 한 바퀴(재시도 없이 `43`~`50`분)나 dev probe다.

설계 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-hyperv-adapter-integration-tier-design.md`
(`pcv-hyperv-adapter-integration-tier-v1`)는 제품과 같은 WMI 어댑터를 in-process로 일회용 VM에 돌리는 단계를 제안했다.
이 단계는 Hyper-V VM을 만들고 지우므로 host mutation이다. 지금 standing approval(`docs/DEVELOPMENT_PROCEDURE.md` §5)은
checkpoint 하나와 기능군 하나에만 적용되어 Lane 1 변경마다 돌릴 수 없다.

## 결정

사용자 승인(2026-10-07 `1,2,3,4,5`의 4, "채택으로 보고 구현")으로 다음을 채택한다.

- 시험 project `src/DesktopNode.HyperV.IntegrationTests`는 `DesktopNode.sln`과 Required CI 밖에 둔다. 명시 실행
  (`dotnet test src/DesktopNode.HyperV.IntegrationTests -c Release`)만 한다.
- 어댑터는 제품과 같은 `DesktopNodeHyperVProviderSet.CreateDefaultWmi()`로 만든다.
- 허용 범위: 이름이 `pcv-it-<run id>-`로 시작하는 VM의 생성, 설정, 삭제. 디스크와 VM 폴더는 저장소
  `artifacts/hyperv-integration/<run id>/` 아래에만 둔다. switch와 VM 목록은 읽기만 한다.
- 금지: service, MSI, firewall, switch 생성·변경, 보존 VM(`pcv-guest-installed-04253-r1`)과 접두사 밖 VM 변경,
  `Restart-Computer`, guest OS 접속.
- 승인 방식: 사용자가 standing approval 문장을 campaign `approval_locator`에 남긴다. 실행 때 환경 변수
  `PCV_HYPERV_INTEGRATION_APPROVAL`이 그 locator 안의 문자열이어야 한다. 철회는 그 문장을 campaign에서 빼는 것이다.
- 실행 가드: 승인 locator, 관리자 권한 또는 `Hyper-V Administrators` 구성원, `Msvm_VirtualEthernetSwitch` 읽기와
  `Default Switch` 존재. 하나라도 어긋나면 VM을 만들지 않고 실패한다.
- 정리: 시험마다 자기 VM을 지우고, 끝에 접두사 VM과 저장 폴더를 다시 지운다. 시작과 끝의 VM 이름 목록이 같아야 한다.
  접두사 VM이 남아 있으면 다음 run은 시작하지 않는다.
- 결과는 `hyperv-integration` 관측이다. `actual_vm_tested`, feature 승격, `current-evidence.json`의 근거가 아니다.

## 결과

- Hyper-V 어댑터를 바꾸는 Lane 1 변경은 PR 전에 실제 WMI 동작을 몇 분 안에 볼 수 있다.
- release train의 Lane 2 probe와 승격 기준은 바뀌지 않는다.
- 첫 standing approval: 2026-10-07 campaign `process-followups-20261007`의 `approval_locator` 항목 4
  ("standing approval limited to pcv-it- prefixed VM create and delete").

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- 이 단계의 PASS는 승격 근거가 아니다.
