# Manual-admin campaign readiness `0.42.69 -> 0.42.70` (2026-08-05)

evidence_id: `manual-admin-campaign-readiness-2026-08-05-04269-04270`
result: `READINESS_PASS_CAMPAIGN_NOT_EXECUTED`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `c-04269-04270`
baseline_version: `0.42.69-admin-smoke`
target_version: `0.42.70-admin-smoke`
installed_version: `0.42.69-admin-smoke`
package_pair_input_status: `ready-current-baseline-target-package-pair`
reservation_status: `reserved-and-matched`
reservation_state: `reserved`
actual_execution_eligible: `true`
host_mutation_performed: `false`
manual_admin_current_closure_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
superseded_by: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md`

> **Point-in-time 기록.** 이 문서는 2026-08-05 `19:33` 기준이다. 아래 runner 표에
> `not-run`으로 적힌 4종은 같은 날 `20:04`-`20:09`에, clean-host는 2026-08-06
> `12:14`-`13:04`에 실행됐고 campaign은 `missing_count=0`, `not_pass_count=0`으로
> 닫혔다. 실행 결과와 최종 closure는
> `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md`가 소유한다.
> `WixToolset.Bal.wixext` 미해결 항목은 Burn runner가 해당 확장을 쓰지 않으므로
> blocker가 아니었다.

## 68일 blocker 해소

manual-admin closure는 2026-05-29의 `0.42.58 -> 0.42.59` 이후 갱신되지 않았다. 직접 원인은
readiness가 반환하던 `blocked-by-installed-baseline-version-mismatch`였다. 예약과 캠페인은 모두
`installed_version == baseline_version`을 요구하는데 설치본이 항상 앞서 있었고, 현재 호스트를
과거 버전으로 downgrade하지 않는다는 정책이 이를 고착시켰다.

`0.42.69` anchor 승격으로 설치본이 `0.42.69`가 되면서 그 다음 pair의 baseline과 일치하게 됐다.

| 항목 | 2026-07-13 | 2026-08-05 |
| --- | --- | --- |
| `requested_version_status` | `blocked-by-installed-baseline-version-mismatch` | `matches-installed-version` |
| `package_pair_input_status` | `blocked-by-installed-baseline-version-mismatch` | `ready-current-baseline-target-package-pair` |
| `reservation_status` | 예약 미존재 | `reserved-and-matched` |
| `actual_execution_eligible` | `false` | `true` |

## 실행한 것

| 항목 | 결과 |
| --- | --- |
| target package `0.42.70` build | `PASS`. MSI SHA-256 `b28e18763ac01137039a9bcfafe0c151945304c8449e307b0412038d6726c86c`, payload `e5bf399740afa6f858a9e2e5fb03317e2588bf7e78eb9342c6f5a58dc6df2a94`, provenance `821a6a34` |
| baseline reservation | `PASS`. `caf337be-e5f8-4bcb-8b9f-719167f2dc12`, kind `dedicated-host`, 만료 `2026-08-06T10:23:14Z` |
| readiness (`-ForActualExecution -PlanOnly`) | `PASS`. 위 status 4종 |

readiness는 host mutation을 수행하지 않았고 예약을 소비하지도 않았다. 예약은 `reserved` 상태로
남아 있으며 consumed sidecar는 생성되지 않았다.

## 실행하지 않은 것

descriptor closure는 runner `5`종의 evidence를 요구한다. 그중 `manual-admin-readiness`만
실행했다.

| runner | 상태 |
| --- | --- |
| `manual-admin-readiness` | `PASS` |
| `installed-product-update-rollback` | `not-run` |
| `clean-host-install-update-rollback` | `not-run` |
| `burn-install-repair-remove` | `not-run` |
| `msix-build-install-update-remove` | `not-run` |
| `installed-runtime-ops-summary` | `not-run` |

중단은 자원 부재가 아니라 명시적 결정이다. `installed-product-update-rollback`은 호스트 설치본을
`0.42.70`으로 남기므로(과거 캠페인의 `Update -> Rollback -> Update` 순서), 완주하지 못하면 anchor
`0.42.69`와 설치본 `0.42.70`이 다시 어긋난다. 현재의 정렬된 상태가 부분 완료 상태보다 정합성이
높다고 판단해 readiness에서 멈췄다.

## 다음 실행에 필요한 것 (조사 완료분)

이전 기록은 전용 runner 도구 부재를 blocker로 시사했으나, 2026-05-29 캠페인 artifact를 실측한
결과 세 runner 모두 전용 래퍼 없이 표준 도구를 직접 호출했다.

| runner | 실제 실행 방식 | 자산 확인 |
| --- | --- | --- |
| `installed-runtime-ops-summary` | `pcvcli --json ops summary` 캡처 | 설치본 CLI 동작 확인 |
| `burn-install-repair-remove` | `wix build` + Burn 번들 install/repair/remove | WiX `5.0.2`, `WixToolset.BootstrapperApplications.wixext` 설치됨 |
| `msix-build-install-update-remove` | `makeappx.exe` + `signtool.exe` | Windows Kits 10 `10.0.18362.0` 존재 |
| `clean-host-install-update-rollback` | base VHD로 throwaway VM 기동 | Windows Server 2022 eval VHD 존재 |

알려진 미해결: `WixToolset.Bal.wixext 5.0.2`가 `damaged` 상태다. Burn 번들 빌드 전에 재설치가
필요할 수 있다.

`PCV_MANUAL_ADMIN_BASELINE_HOST`와 `PCV_MANUAL_ADMIN_CREDENTIAL_REF`는 2026-07-13 계획 문서의
규약이며 저장소의 어떤 도구도 읽지 않는다. 설정해도 기계적 효과가 없고, 운영자가 전용 호스트
사용을 선언하는 의미만 갖는다.

## 부수 수정

첫 실사용에서 `New-PcvManualAdminRebaselineReadiness.ps1`과
`New-PcvManualAdminBaselineReservation.ps1`이 서로 다른 호스트 식별자를 해시하는 결함이 드러났다.
전자는 `MachineName`, 후자는 `"$machineGuid|$env:COMPUTERNAME"`이었다. 지문 발행 주체인 예약
도구를 정본으로 삼아 readiness를 맞췄다. 단위 테스트는 양쪽에 동일한 override를 주입하므로 이
차이를 드러낼 수 없었다.

## Nonclaims

- manual-admin package-pair closure를 달성하지 않았다. current closure는 계속
  `0.42.58-admin-smoke -> 0.42.59-admin-smoke`다.
- descriptor를 생성하지 않았고 `missing_count`/`not_pass_count`를 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
