# PureCVisor Desktop Node P2 Off VM 기능군 Lane 2 프로브 설계

- Design-ID: `purecvisor-desktop-node-p2-offvm-lane2-probe-v1`
- 작성일: `2026-09-27`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: 2026-09-27 host mutation ok, Off VM 기능군부터`
- 구현 계획: `docs/superpowers/plans/2026-09-27-purecvisor-desktop-node-p2-offvm-lane2.md`
- 선행 설계: `docs/superpowers/specs/2026-08-28-purecvisor-desktop-node-p1-clone-lane2-probe-design.md`
- 이 문서가 수행하는 host mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 범위

guest OS 없이 Off VM으로 검증할 수 있는 P2 기능군을 설치본 CLI로 한 run에 한 기능군씩 확인한다.
이 문서는 runner 계약만 정한다. current-evidence, feature ledger pass, fullgate, pair는 열지 않는다.

| 기능군 | `-Family` | 설치본 `0.42.78`에서 실행 |
| --- | --- | --- |
| P2-15 NIC/DVD 추가 | `device-add` | 가능 |
| P2-12 checkpoint schedule | `checkpoint-schedule` | 가능 |
| P2-13 export/import | `export-import` | 불가. Off 어휘 결함 수정 package 필요 |
| P2-14 network connect | `network-connect` | 불가. 같은 이유(정책이 switch 확인보다 전원 확인을 먼저 한다) |

## 2. runner

- 파일: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1`
- 필수: `-Version`(설치본 manifest와 대소문자 포함 일치), `-Family`
- 선택: `-ArtifactRoot`, `-VmRoot`(볼륨 루트 아래 세그먼트 2개 이상), `-IsoPath`, `-VmName`, `-SwitchName`(기본 `Default Switch`)
- VM 이름: `pcv-p2-offvm-<version tag>-<8hex>-<dev|sched>`. 표시 이름만 operator id로 쓴다.
- P1 clone runner 구조를 따른다: DryRun, `RuntimeAdapter`, atomic `summary.json`, secret 검사, 정확한 identity cleanup,
  제품 delete만 사용(native `Remove-VM` fallback 없음).

## 3. slice

한 기능군, 고정 순서, fail-stop. 거절 slice는 mutation slice보다 앞선다.

### `device-add`

| slice | 동작 | PASS |
| --- | --- | --- |
| `source_create` | 제품 create, Gen2, `8` GB | Hyper-V `Off`, 제품 off/stopped, managed, NIC `1`, DVD `1` |
| `nic_confirm_required` | `--yes` 없이 NIC 추가 | `PCV_CLI_CONFIRMATION_REQUIRED`, NIC `1` |
| `nic_add` | NIC 추가 `--yes` | job succeeded, Hyper-V NIC `2`, 제품 network `2`, 둘 다 `SwitchName` |
| `nic_limit` | 세 번째 NIC | `PCV_VM_DEVICE_LIMIT`, job 없음, NIC `2` |
| `dvd_guard` | DVD 추가 `--yes` | `PCV_VM_DEVICE_ALREADY_PRESENT`(route 거절 또는 job failed), DVD `1` |
| `cleanup` | 제품 delete, VM 디렉터리 삭제 | VM 없음, 디렉터리 없음 |

제품 create는 ISO DVD를 붙이므로 DVD 추가 성공 경로는 제품 VM으로 도달할 수 없다(nonclaim). 제품 inventory
`storage`에 DVD가 없어 route guard가 DVD 수를 `0`으로 보고 job을 받은 뒤 native가 거절한다. runner는 두 층
거절을 모두 PASS로 보고 `rejected_by`에 기록한다.

### `checkpoint-schedule`

| slice | 동작 | PASS |
| --- | --- | --- |
| `source_create` | 위와 같음 | checkpoint `0` |
| `schedule_preview` | `60`분, retention `2` preview | job 없음, 응답 값 일치, `checkpoint_schedule.enabled=false` |
| `schedule_interval_invalid` | `30`분 set `--yes` | `PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID`, job 없음, enabled false |
| `schedule_set` | `60`분, retention `2` set `--yes` | job succeeded, `vm get` readback enabled/interval/retention 일치, checkpoint `0` |
| `schedule_clear` | clear `--yes` | job succeeded, enabled false |
| `cleanup` | 켜진 스케줄이 남으면 clear 후 제품 delete | 스케줄 해제, VM·디렉터리 없음 |

최소 주기가 `60`분이라 due worker tick과 retention delete는 관측하지 않는다(nonclaim).

### `export-import`

경로: allowlist root는 `VmRoot\exports`, export 디렉터리는 `VmRoot\exports\<source>`, 거절 확인용 바깥 경로는
`VmRoot\outside-<source>`다. 모든 export/import 호출에 `--allowed-root VmRoot\exports`를 넘긴다.

| slice | 동작 | PASS |
| --- | --- | --- |
| `source_create` | 위와 같음 | checkpoint `0` |
| `export_confirm_required` | `--yes` 없이 export | `PCV_CLI_CONFIRMATION_REQUIRED`, job 없음, export 디렉터리 없음 |
| `export_path_not_allowed` | allowlist 밖 디렉터리로 export preview | `PCV_VM_EXPORT_PATH_NOT_ALLOWED`, 바깥 디렉터리 없음 |
| `export_preview` | export preview | `dry_run=true`, `host_mutation_performed=false`, job 없음, 디렉터리 없음 |
| `export` | export `--yes` | job succeeded, `.vmcx` `1`, `.vhdx` `1`개 이상, `.vmgs` `0`, 소스 Off |
| `import_preview` | import preview `--has-vmcx` | `dry_run`, `generate_new_id`, `apply_managed_marker` 모두 true, 대상 없음 |
| `import` | import `--yes` | job succeeded, 새 identity(소스 id와 다름), managed, Off, 소스 Off |
| `cleanup` | import VM 먼저 제품 delete, 다음 소스, export root와 VM 디렉터리 삭제 | 둘 다 없음, 디렉터리 없음 |

제품 import는 `ImportSystemDefinition`으로 export package를 제자리 등록하므로 import VM 소유 root를
`VmRoot\exports`로 예약한다. import VM 구성 경로가 그 밖이면 identity blocker로 멈추고 정리하지 않는다(수동 정리,
fail-safe). 제품 `vm delete`는 파일을 지우지 않으므로 디렉터리는 runner가 identity 확인 뒤 지운다.

### `network-connect`

대상 switch는 run이 만드는 전용 Private switch `pcv-p2-offvm-<tag>-<8hex>-sw`다. 제품 경로
`DesktopNode.Host.exe service-action switch-create --product-root --service-exe --switch-name --switch-type private`로
만들고, cleanup에서 VM 삭제 뒤 `switch-remove`로 지운다. Private는 management OS host vNIC를 만들지 않는다.
switch는 `Msvm_VirtualEthernetSwitch` id를 기록하고, 같은 이름·같은 id일 때만 지운다. native `New-VMSwitch`/
`Remove-VMSwitch`는 쓰지 않는다.

| slice | 동작 | PASS |
| --- | --- | --- |
| `source_create` | 위와 같음, NIC `1`(Default Switch) | 위와 같음 |
| `switch_create` | Host service-action switch-create | `Ok=true`, WMI switch `1`개, id 기록 |
| `connect_confirm_required` | `--yes` 없이 connect | `PCV_CLI_CONFIRMATION_REQUIRED`, job 없음, 연결 불변 |
| `connect_switch_missing` | 없는 `...-absent` switch로 connect `--yes` | `PCV_NETWORK_SWITCH_NOT_FOUND`, job 없음 |
| `connect` | 전용 switch로 connect `--yes` | job succeeded, WMI 연결과 제품 network 모두 전용 switch 하나, VM Off |
| `cleanup` | 제품 delete, 디렉터리 삭제, 그 뒤 switch-remove | VM·디렉터리·switch 없음 |

## 4. PASS / FAIL

Lane 2 PASS 입력은 `overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`,
`-Version` == 설치본 manifest다. DryRun PASS는 Lane 2 PASS가 아니다. PASS여도 상태는
installed non-promoted candidate이며 feature ledger와 current-evidence를 바꾸지 않는다.

## 5. 비주장

- 이 문서는 Hyper-V/MSI/service mutation을 실행하지 않는다.
- Internal/External switch, guest 트래픽은 검증하지 않는다.
- public trusted signing / external stable publication `not-claimed`
