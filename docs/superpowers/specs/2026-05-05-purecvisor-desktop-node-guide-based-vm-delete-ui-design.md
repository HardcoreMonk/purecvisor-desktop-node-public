# PureCVisor Desktop Node guide 기반 VM delete UI 설계

## 목적

이 문서는 `https://purecvisor.site/ui/guide.html`의 Single Edge 운영 가이드 중 VM 운영 패턴을 Desktop Node Web Console 후속 후보로 좁혀 적용하는 기준을 정의한다.

사용자가 선택한 1차 후보는 `VM delete UI`다. 이 후보는 새로운 Hyper-V delete runtime을 만드는 작업이 아니다. 현재 제품 API에 이미 존재하는 `DELETE /api/v1/vms/{id}` managed VM delete job queue를 Web Console detail panel에서 안전하게 사용할 수 있게 노출하는 UX slice다.

## 결정 후보

```text
DESKTOP_NODE_GUIDE_BASED_VM_DELETE_UI_CANDIDATE: managed-hyperv-delete-web-console-queued-job
```

이 결정 후보는 ADR이 아니다. 다음 조건을 만족하면 후속 implementation plan 작성과 구현 slice 착수를 검토한다.

- Web Console은 existing Local API `DELETE /api/v1/vms/{id}` route만 사용한다.
- 실제 provider mutation의 authoritative guard는 .NET native adapter의 managed marker guard가 유지한다.
- UI는 running VM 또는 상태 불명 VM에 대해 operator confirmation과 advisory guard를 제공하되, API contract를 우회하지 않는다.
- Delete job은 기존 `Tracked Jobs` localStorage history와 polling flow에 들어간다.
- Token 값은 DOM, log, fixture, 문서 예시에 노출하지 않는다.
- Linux Single Edge VM storage/runtime contract를 Desktop Node에 반입하지 않는다.

## 가이드 기반 채택/제외 기준

Single Edge guide의 VM 장은 비동기 VM delete, delete status 확인, Web UI/CLI/API 운영 흐름을 강조한다. Desktop Node는 이 중 다음 패턴만 채택한다.

- destructive VM operation은 명시적 사용자 확인 뒤 queued job으로 실행한다.
- 작업 직후 결과는 VM 목록이 아니라 job status에서 먼저 확인한다.
- delete 실패는 structured failure code/detail을 사용자에게 보여준다.
- 운영 문서는 delete가 host mutation이며 rollback/final-state proof가 필요한 gate임을 유지한다.

다음 항목은 이 slice에서 제외한다.

- libvirt, KVM, QEMU, qcow2/raw, ZFS zvol 기반 delete/storage contract
- Linux snapshot dependency cleanup, ZFS promote/rollback, clone/template storage cleanup
- Container, OVS/OVN, LXC, ZFS, iSCSI 관련 화면 또는 adapter
- 새 Hyper-V runtime delete adapter 구현
- VM clone/template, ISO attach, NIC hotplug, backup/retention 정책
- MSI/service/firewall/trust-store/LAN/Event Log mutation 실행
- public trusted signing 또는 외부 stable publication claim

## 현재 제품 계약

현재 Desktop Node 제품 path는 다음 계약을 이미 갖고 있다.

- `DELETE /api/v1/vms/{id}`는 C# native VM delete adapter가 처리한다.
- PureCVisor managed marker가 없는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단된다.
- 대상 VM이 이미 없으면 idempotent `action=absent` result를 반환한다.
- Installed destructive smoke는 managed delete `action=delete`, repeat delete `action=absent`, unmanaged guard block, cleanup/no-reboot final state를 확인했다.
- `940999e Add VM delete action to web console` 이후 Web Console은 VM delete button/action handler를 노출한다.
- `940999e`는 UI/static asset 변경 evidence다. 실제 Hyper-V delete, MSI/service/firewall/trust-store/LAN/Event Log mutation을 새로 실행한 OS mutation gate evidence가 아니다.

## UX 계약

VM detail panel에 destructive `Delete VM` action을 추가한다.

- Button 위치는 기존 lifecycle action group 안쪽 또는 바로 아래 destructive group으로 둔다.
- 선택된 VM id/name이 없으면 button은 disabled 상태다.
- VM state가 `Running` 계열로 판단되면 기본 UX는 delete 실행을 막고 먼저 `Power off`를 요구한다.
- VM state가 비어 있거나 알 수 없는 값이면 강한 확인 dialog를 띄우되, API guard가 최종 판단한다는 copy를 포함한다.
- 확인 dialog는 VM name/id, destructive host mutation, managed VM만 삭제 가능, job tracking 위치를 포함한다.
- 확인 후 `DELETE /api/v1/vms/{id}`를 호출하고 반환된 job을 `Tracked Jobs`에 추가한다.
- job queue 성공 후 VM detail은 refresh한다. 대상 VM이 사라졌거나 API가 `PCV_VM_NOT_FOUND`를 반환하면 선택 상태를 비운다.
- API가 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`를 반환하면 alert region에 code/detail을 그대로 보여주고 provider mutation이 실행되지 않았다는 의미를 사용자 가이드에 연결한다.

## 보안/운영 경계

- UI button 추가는 host mutation 실행 증거가 아니다.
- 실제 VM delete 실행은 사용자가 Web Console에서 명시적으로 누른 경우에만 발생한다.
- 기본 service listener는 loopback-only다. LAN exposure 정책은 변경하지 않는다.
- Token은 Web Console state와 Authorization header에만 사용하며 source/test fixture에 장기 token 값을 넣지 않는다.
- Delete UX는 public release, public trusted signing, external stable publication 범위를 넓히지 않는다.

## 구현 범위 후보

구현 plan은 `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-vm-delete-ui.md`에 기록되어 있고 `940999e`로 main에 반영됐다. 구현 범위는 다음 파일을 다뤘다.

- `web/src/served-app.ts`
  - VM delete button render
  - delete action handler
  - confirmation copy
  - job tracking 연결
  - selected VM refresh/clear handling
- `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - `DELETE /api/v1/vms/{id}` endpoint string
  - `data-action="vm-delete"`
  - destructive confirmation
  - no forbidden host mutation command literals
- `web/scripts/verify-browser-fixture.mjs`
  - fixture Local API가 선택된 VM detail에서 `Delete VM` action을 렌더링하는지 확인한다.
  - no actual Local API, Hyper-V, service, MSI, firewall, trust-store mutation을 유지한다.
- `docs/USER_GUIDE.md`
  - Web Console VM 작업 섹션에 delete button 설명 추가
  - managed marker guard와 `Tracked Jobs` 확인 흐름 보강

## 검증 후보

문서/spec만 변경할 때:

```powershell
git diff --check
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

Web Console 구현이 들어갈 때:

```powershell
npm test --prefix web
npm run generate:parity --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

이 slice의 기본 검증은 실제 Hyper-V delete를 실행하지 않는다. 실제 installed destructive VM delete smoke가 필요하면 기존 OS mutation gate와 같은 administrator opt-in, rollback/final-state proof, no-auto-reboot evidence를 별도 실행한다.

## 완료 기준

- guide 기반 채택 범위가 Desktop Node Windows/Hyper-V 경계로 제한되어 있다.
- VM delete UI가 기존 API route와 queued job runtime을 사용한다고 명시되어 있다.
- managed marker guard가 API authoritative guard로 남아 있다.
- running/unknown state delete에 대한 UI advisory guard가 명시되어 있다.
- Linux Single Edge runtime/storage 기능을 제외한다.
- LAN/firewall/trust-store/MSI/service/Event Log mutation을 이 slice에서 실행하지 않는다.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- 구현 plan과 Web Console 반영은 `940999e`로 완료됐다.
