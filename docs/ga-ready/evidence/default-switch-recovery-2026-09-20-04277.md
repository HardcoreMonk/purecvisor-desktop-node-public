# Default Switch 복구 2026-09-20 `0.42.77` 설치본 호스트

evidence_id: `default-switch-recovery-2026-09-20-04277`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
lane: `2`
working_authority: `installed_current`
version: `0.42.77-admin-smoke`
host_mutation_performed: `true`
current_evidence_written: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
predecessor_fail: `installed-operator-surface-current-card-2026-09-20-04277`
artifact_root: `artifacts/lane2-default-switch-recovery-20260920`

## 범위

사용자가 Default Switch 복구를 이름으로 지정했다. Lane 2만 수행했다. `docs/ga-ready/current-evidence.json`은
바꾸지 않았다. leftover VM 삭제, msiexec ARP, current-card 승격, P1-8은 범위 밖이다.

## 원인

`Get-VMSwitch`와 `Msvm_VirtualEthernetSwitch`가 `일반 오류`로 실패하고
`host.status`는 `supported=false`, `PCV_DEFAULT_SWITCH_UNKNOWN`이었다.
`vEthernet (Default Switch)`는 `Not Present`였고 PnP `ROOT\VMS_VSMP\0000`은
`CM_PROB_FAILED_DRIVER_ENTRY`였다.

`vmswitch.sys` DriverEntry가 `0xC0000034` (`STATUS_OBJECT_NAME_NOT_FOUND`)로 실패한 뒤
즉시 unload됐다. VMSMP miniport는 `\Driver\VMSP`를 찾는데, 프로토콜 드라이버 `VMSP`가
STOPPED였다. VMSNPXY는 이미 Running이었다.

이 실패는 설치본 payload hash 불일치가 아니다.

## 수행한 mutation

1. `sc.exe start VmsProxy`
2. `sc.exe start VMSP` — Running
3. VMSMP는 VMSP 기동 후 Running (`StartService` 1056 already running)
4. `ROOT\VMS_VSMP\0000` disable/enable — `OK` / `CM_PROB_NONE`
5. `Restart-Service SharedAccess`, `hns`, `vmms`

재부팅은 하지 않았다. leftover VM은 지우지 않았다.

## 복구 후 readback

| 항목 | 결과 |
| --- | --- |
| `vEthernet (Default Switch)` | `Up` |
| `Get-VMSwitch` | `Default Switch` / `Internal` |
| WMI `Msvm_VirtualEthernetSwitch` | `ElementName=Default Switch` |
| CLI `host status` | exit 0, `supported=true`, `default_switch_present=true`, `reasons=[]` |
| CLI `network inventory` | exit 0, `source=hyperv`, `Default Switch` `internal` `is_default=true` `allow_management_os=true` |
| `PureCVisorDesktopNode` | `Running` / `Automatic` |
| leftover VM | `pcv-cleanhost-20260910-r2-04274-04275` Saved, `pcv-guest-installed-04253-r1` Off — 변경하지 않음 |

## Nonclaims

- Lane 2 호스트 복구이며 operational current 승격이 아니다.
- 2026-09-20 current-card FAIL 문서는 historical FAIL로 남긴다. 이 파일이 그 카드를 PASS로 고치지 않는다.
- 재부팅 후 VMSP/VMSMP 기동 순서는 검증하지 않았다. VMSP는 demand-start다.
- public trusted signing 또는 external stable publication evidence가 아니다.
