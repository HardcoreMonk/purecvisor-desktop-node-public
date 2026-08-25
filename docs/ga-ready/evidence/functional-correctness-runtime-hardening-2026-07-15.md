# Functional Correctness Runtime Hardening Code-Level Evidence — 2026-07-15

## 판정

- 상태: `CODE_LEVEL_PASS`
- 대상 finding: `FC-01`, `FC-02`, `FC-04`, `FC-16`, `FC-18`
- 기준 branch: `codex/functional-correctness-runtime-hardening`
- 기준 base: `999f4efa` (`main`)
- 운영자 표면: CLI/Web only. TUI source/package surface를 다시 추가하지 않았다.

이 문서는 감사에서 실측 확인된 기능 정확성 결함에 대한 소스·단위 테스트 수준의 수정 결과를
기록한다. 설치본, 실제 VM, Hyper-V 호스트, MSI/package lifecycle의 승격 증거가 아니다.

## 수정 결과

| Finding | 수정 | 회귀 계약 |
|---|---|---|
| FC-01 | `Msvm_ImageManagementService.GetVirtualHardDiskSettingData`의 `MaxInternalSize`를 읽고 요청 크기가 더 작으면 `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`로 `ResizeVirtualHardDisk` 호출 전에 차단한다. | fake virtual disk operations에서 shrink는 resize 호출 `0`, 동일/확장은 호출 `1`을 검증한다. |
| FC-02 | rollback을 active→`.failed`, previous→active 두 단계로 분리했다. 두 번째 단계 실패 시 부분 active를 `.restore-partial.<guid>`로 보존하고 `.failed`를 active로 복원한다. | 이전 제품 승격 실패를 주입해 현재 제품, 이전 제품, 부분 승격 파일이 모두 보존되는지 검증한다. |
| FC-04 | API의 `maximum_kbps`/`minimum_kbps`를 decimal `×1000`으로 WMI bps에 기록하고, 기존 WMI bps는 `/1000`으로 Kbps evidence에 투영한다. | `1 Kbps = 1,000 bps`, `2,048 Kbps = 2,048,000 bps`와 역변환을 검증한다. |
| FC-16 | 기존 `.previous`를 `.previous.staging`으로 이동한 뒤 active backup을 수행한다. 실패 시 부분 previous를 `.partial.<guid>`로 보존하고 staging을 previous로 복구한다. previous와 staging이 동시에 있으면 자동 삭제 없이 `PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED`를 반환한다. | backup 승격 실패와 양쪽 경로 동시 존재를 주입해 기존 backup과 부분 결과 보존을 검증한다. |
| FC-18 | 읽을 수 없거나 reparse guard에서 거부된 `summary.json`의 정렬 시간을 `DateTime.MinValue`로 처리한다. | private sort-time 계약에서 unreadable 후보가 최신 후보가 되지 않도록 `MinValue`를 검증한다. |

## 구현 provenance

- `0271c461`: rollback compensation
- `b1cf9c03`: update backup staging/compensation
- `57f643a2`: Hyper-V disk/QoS mutation policy와 전용 테스트 프로젝트
- `87096267`: unreadable evidence sort ordering
- `cca81f8`: active root 부재 시 backup recovery 충돌 우선 판정

설계와 실행 계획은 각각
`docs/superpowers/specs/2026-07-15-functional-correctness-runtime-hardening-design.md`,
`docs/superpowers/plans/2026-07-15-functional-correctness-runtime-hardening.md`가 소유한다.

## 검증 결과

2026-07-15 KST에 격리 worktree에서 다음 명령을 실행했다.

```powershell
dotnet test src/DesktopNode.sln -c Release
```

- 결과: PASS
- 합계: `591 passed`, `0 failed`, `0 skipped`
- 세부: Contracts `15`, Service `11`, Runtime `17`, CLI `112`, Hyper-V `8`, Host `150`, API `278`

```powershell
pwsh -NoProfile -Command '$result = Invoke-Pester -Path "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1" -Output Normal -PassThru; if ($result.FailedCount -gt 0) { exit 1 }'
```

- 결과: PASS
- 합계: `55 passed`, `0 failed`, `0 skipped`

각 finding은 구현 전에 대상 RED를 확인했다. FC-02/FC-16은 helper 부재, FC-01/FC-04는 내부
정책/작업 경계 부재, FC-18은 expected `DateTime.MinValue` / actual `DateTime.MaxValue`로 실패한 뒤
구현 후 GREEN으로 전환했다.

## 증거 경계와 후속 조치

- `host_mutation_performed=false`
- `installed_product_changed=false`
- `package_build_performed=false`
- `public_trusted_signing=not-claimed`
- `external_stable_publication=not-claimed`
- 최신 operational installed/package anchor는 계속 `0.42.63-admin-smoke`다.
- Hyper-V provider payload가 바뀌었으므로 다음 승인된 package chain에서 actual VM disk expansion과
  network QoS Kbps readback smoke를 재실행해야 operational 승격을 주장할 수 있다.
- rollback/backup 보상은 다음 승인된 설치본 update/rollback campaign에서 파일 잠금 주입 또는 이에
  준하는 installed lifecycle 증거로 후속 확인한다.
