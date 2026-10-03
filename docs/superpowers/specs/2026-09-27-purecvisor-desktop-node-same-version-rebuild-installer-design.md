# 같은 version MSI 재빌드 설치 동작 설계

- Design-ID: `pcv-same-version-rebuild-installer-v1`
- 작성일: `2026-09-27`
- 문서 상태: `accepted-option-a-and-c` (C 2026-10-03 구현, A 2026-10-03 채택·구현, 재검증은 0.42.87 pair)
- 변경 등급: L (installer lifecycle)
- host/VM/service/package mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 문제

같은 product version(예: `0.42.78`)으로 MSI를 다시 빌드해 설치하면 이전 설치를 대체하지 않고
옆에 새 제품으로 등록된다. 2026-09-26~27 `0.42.78` fullgate에서 다음이 관측됐다
(`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-09-27-04278-hostmutation.md`).

- ARP에 `PureCVisor Desktop Node` `0.42.78` 항목이 `3`개 쌓였다(09-26 FAIL run 두 attempt, 09-27 run).
- install/final-restore-install log: `DesktopNode.Host.exe; Won't Overwrite; Existing file is of an equal version`.
  설치본 Host/CLI는 새 gate build가 아니라 첫 build(`e098e0a`)로 남았다.
- uninstall-remove-data log: `Disallowing uninstallation of component ... since another client exists` `16`건.
  uninstall과 remove-data 증명이 약해졌다.
- 잔여 ProductCode `3`개를 데이터 보존 `msiexec /x`로 지운 뒤 r2 gate는 설치본이 gate build와 같았다.

## 2. 원인

`packaging/windows-desktop-node/installer/Product.wxs`:

- `Package`는 `Version="$(var.MsiProductVersion)"`와 고정 `UpgradeCode`를 쓰고 ProductCode를
  지정하지 않는다. WiX는 build마다 새 ProductCode를 만든다.
- `MajorUpgrade`는 `DowngradeErrorMessage`만 지정한다. `AllowSameVersionUpgrades` 기본값은
  `no`라서 같은 version의 다른 ProductCode는 major upgrade 대상이 아니다.
- 파일 버전(`1.42.78.0`)이 같으면 Windows Installer 기본 규칙은 기존 파일을 덮어쓰지 않는다.
  공유 component는 client가 남아 있는 한 제거되지 않는다.

제품 사용자가 같은 version을 다시 설치할 일은 드물지만, fullgate는 실행마다 gate 내부에서
MSI를 새로 build하므로 같은 version 재실행이 곧 이 조건이다.

## 3. 선택지

| 안 | 변경 | 효과 | 비용과 위험 |
| --- | --- | --- | --- |
| A | `MajorUpgrade`에 `AllowSameVersionUpgrades="yes"` | 같은 version 새 build가 이전 build를 제거하고 설치된다. 파일 교체와 uninstall 증명이 정상화된다 | installer lifecycle 변경(L). WiX ICE61 경고를 다뤄야 한다. clean-host, update/rollback, Burn/MSIX, fullgate 재검증이 필요하다 |
| B | gate build마다 MSI version build 필드를 올림 | 매 build가 정식 major upgrade가 된다 | product version과 evidence 이름 체계가 흔들린다. MSI는 네 번째 필드를 비교하지 않아 세 번째 필드를 써야 한다 |
| C | fullgate 시작 전 preflight: 같은 version ARP 항목이 이미 있거나 설치본 build commit이 gate build와 다르면 멈춤 | 잘못된 PASS를 막는다. installer는 그대로다 | 증상만 막는다. 운영자가 같은 version을 재설치하는 경로는 그대로다 |

## 4. 권고

1. **먼저 C를 넣는다.** batch supervisor 또는 route parity smoke 앞단에서 읽기 전용으로 ARP와
   설치본 ProductVersion의 `+<commit>`을 확인한다. host mutation이 없어 Lane 1로 넣을 수 있다.
   완료 조건은 “같은 version 잔여 항목이 있으면 명확한 오류 코드로 멈춤”과 “gate 뒤 설치본
   build commit이 gate build와 같음 확인”이다.
2. **A는 별도 L 등급 checkpoint로 결정한다.** 채택하면 fullgate, clean-host install/update/rollback,
   Burn/MSIX lifecycle을 관리자 승인 아래 다시 돌려야 한다.
3. B는 권고하지 않는다. version 정체성을 흔드는 비용이 문제보다 크다.

## 5. 비목표

- 이 문서는 installer, batch supervisor, fullgate 도구를 바꾸지 않는다.
- 이미 기록한 09-26~27 evidence의 판정을 바꾸지 않는다.
- public trusted signing, external stable publication을 다루지 않는다.

## 6. C 구현 (2026-10-03)

post-0.42.86 backlog Task 2(`docs/superpowers/plans/2026-10-03-purecvisor-desktop-node-post-04286-backlog.md`)가
`packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`에 C를 넣었다.

- `same-version-preflight` step: gate MSI build 전에 ARP(`HKLM` Uninstall 64/32bit)를 읽기 전용으로 읽는다.
  같은 MSI product version의 `PureCVisor Desktop Node` 항목이 있으면
  `PCV_SMOKE_SAME_VERSION_RESIDUAL`로 멈추고 ProductCode와 권고(`REMOVE_DATA` 없이 제거 뒤 재실행)를 남긴다.
- `msi-lifecycle-smoke` 끝: `final-restore-install` 뒤 설치본 `DesktopNode.Host.exe` ProductVersion의 `+<commit>`을
  build provenance `git_commit`과 비교한다. 다르거나 읽을 수 없으면 `PCV_SMOKE_INSTALLED_BUILD_MISMATCH`로 멈춘다.
- `-SelfTest`에 `same-version-preflight-self-test`를 더했다. Required CI는 PowerShell을 돌리지 않으므로 C# 계약
  `PcvRouteParitySameVersionPreflightContractTests`가 배선을 고정한다.

결과: batch supervisor가 같은 step을 재시도할 때, 앞 attempt가 설치한 같은 version 항목이 남아 있으면 재시도도
preflight에서 멈춘다. 잘못된 PASS 대신 명확한 오류를 내는 것이 C의 목적이므로 의도한 동작이다. 운영자는 잔여 항목을
`REMOVE_DATA` 없이 지운 뒤 다시 돌린다. A(`AllowSameVersionUpgrades`)는 계속 별도 L 등급 결정이다.

## 7. A 채택 (2026-10-03)

사용자가 2026-10-03에 A를 승인했다(`1,2,3`의 3). 0.42.87 campaign Task 1
(`docs/superpowers/plans/2026-10-03-purecvisor-desktop-node-04287-package-pair.md`)이 구현했다.

- `Product.wxs` `MajorUpgrade`에 `AllowSameVersionUpgrades="yes"`와 `Schedule="afterInstallValidate"`(기본값을 명시)를
  더했다. 같은 version의 다른 ProductCode가 major upgrade 대상이 되고, 새 파일을 쓰기 전에 이전 제품을 지운다.
  `AllowDowngrades`는 쓰지 않으므로 downgrade 차단은 그대로다. `wix build`는 ICE 검증을 돌리지 않아 ICE61 경고가
  build를 막지 않는다.
- C의 사전 검사는 차단에서 기록(`same_version_upgrade_expected`)으로 바꿨다. gate install이 잔여 항목을 지운다.
- 사후 검사는 둘 다 차단이다. 설치본 build commit이 gate build와 같아야 하고(`PCV_SMOKE_INSTALLED_BUILD_MISMATCH`),
  `final-restore-install` 뒤 같은 version ARP 항목이 정확히 `1`개여야 한다(`PCV_SMOKE_SAME_VERSION_DUPLICATE`,
  `PCV_SMOKE_SAME_VERSION_MISSING`).
- C# 계약 `PcvSameVersionUpgradeContractTests`가 `MajorUpgrade` 속성을 고정한다.
- 재검증: 0.42.87 pair의 clean-host install/update/rollback, Burn, MSIX와, 같은 version 설치본 위에서 시작하는
  fullgate가 맡는다.
