# clean-host base VHD 오프라인 갱신 설계 (2026-09-29)

상태: 구현과 첫 갱신 run 완료(2026-09-30, `docs/superpowers/plans/2026-09-30-purecvisor-desktop-node-clean-host-base-vhd-refresh.md`). 설계와 달라진 점은 §8에 있다. 설계는 `docs/superpowers/plans/2026-09-29-purecvisor-desktop-node-post-04283-followups.md` Task 3에서 썼다.

## 1. 문제

manual-admin pair의 dedicated clean-host bucket(`Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`)은 run마다 다음을 한다.

1. 기준 VHD의 differencing 디스크로 새 VM을 만든다.
2. Windows Update COM API로 제목이 `^20\d{2}-\d{2} Cumulative Update for Microsoft server operating system version 21H2`인 최신 LCU를 설치하고 재부팅한다.
3. baseline MSI 설치, catalog update, rollback을 한다.

기준 VHD는 2021-08 빌드(`20348.169`, Windows Server 2022 Datacenter Evaluation)다. 그래서 run마다 약 5년치 누적 업데이트를 새로 설치한다.

0.42.83 run(`internal-clean-host-install-update-rollback-smoke-2026-09-29-04278-04283`)의 관측:

| 구간 | 관측 |
| --- | --- |
| 전체 | 46분(`13:21:49Z`~`14:08:06Z`). 0.42.78 run은 42분 |
| Windows Update | `KB5122882` 1개, UBR `169 → 5622`, 재부팅 |
| 재부팅 뒤 | PowerShell Direct 26회 시도, heartbeat 무응답 자동 복구 실행(`automatic_recovery_performed=true`, 대기 한도 900초) |
| 제품 install/update/rollback | 모두 exit `0` |

run 중 VM 관측 결과는 다음과 같다(KST).
- VM 시작(22:22) 뒤 약 28분에 업데이트를 설치하고 재부팅했다(22:50경).
- 그 뒤 CPU `0`인 무응답 상태가 이어졌다.
- 약 17분 뒤 무응답 자동 복구가 VM을 다시 시작했다(마지막 부팅 23:07). 대기의 대부분은 **LCU 설치, 재부팅, 재부팅 뒤 무응답 대기**다. 다운로드는 이보다 작다.

## 2. 고르지 않은 방안

| 방안 | 고르지 않은 이유 |
| --- | --- |
| 로컬 WSUS | WSUS는 Windows Server 역할이다. 이 호스트(Windows 11 Pro for Workstations)에는 `Install-WindowsFeature`가 없다. 별도 Server VM, IIS, WID, 동기화, 승인 관리가 필요한데 줄어드는 것은 다운로드뿐이다. WSUS는 2024-09에 deprecated됐다. |
| Microsoft Connected Cache | Windows 11 호스트에 WSL2로 올릴 수 있고 Server 2019 이상은 DO 클라이언트다. 그러나 Azure portal/CLI 관리(구독), nested virtualization, 실행 계정, guest `DOCacheHost` 설정이 필요하다. 이것도 다운로드만 줄인다. |
| template VM 온라인 갱신 뒤 sysprep | 이미지가 generalize 상태에서 벗어나므로 매달 sysprep이 필요하다. rearm 한도와 실패 복구가 부담이다. |

## 3. 권장안: 날짜별 base VHD를 오프라인 서비싱으로 만든다

원본 VHD는 바꾸지 않는다. 매달 원본을 복사하고, 그 복사본에 최신 LCU를 DISM으로 오프라인 적용해 날짜가 붙은 새 base를 만든다. 오프라인 적용이라 이미지는 generalize 상태 그대로다. clone마다 OOBE와 unattend가 지금처럼 돈다.

### 3.1 절차 (새 도구 `New-PcvCleanHostBaseVhd.ps1`, 기본 dry-run)

1. **입력**
   - 원본 VHD 경로
   - LCU `.msu` 경로
   - 기대 KB와 `.msu` SHA-256. `.msu`는 사람이 Microsoft Update Catalog에서 받는다. 도구는 다운로드하지 않는다.
2. **복사**: `artifacts/image-cache/windows-server-2022-eval-vhd/20348.<UBR>-<yyyymmdd>.vhd`로 복사한다. 원본과 기존 base는 덮어쓰지 않는다.
3. **적용**: `Mount-WindowsImage -ImagePath <copy> -Index 1 -Path <mount>` → `Add-WindowsPackage -Path <mount> -PackagePath <msu>` → `Dismount-WindowsImage -Path <mount> -Save`
   - 실패하면 `-Discard`로 unmount하고 복사본을 지운다.
4. **확인**: 복사본의 오프라인 레지스트리(`SOFTWARE`) `CurrentVersion\UBR`가 기대 UBR인지 본다. 이 확인은 적용 단계에서 mount가 풀리기 전에 한다.
5. **기록**: sidecar JSON `…vhd.base.json`을 쓴다.
   - 원본 경로와 SHA, KB, `.msu` SHA, UBR, 만든 시각
   - 복사본 SHA-256(10GB라 수십 초 걸린다)
6. **current 지정**: `artifacts/image-cache/windows-server-2022-eval-vhd/current-base.json`에 현재 base 경로를 적는다. 이전 base 두 개까지 남기고 그보다 오래된 것은 사람이 지운다.

### 3.2 확인할 기술 사항 (구현 첫 spike)

- Server 2022 LCU는 SSU가 합쳐진 패키지다. 오프라인 이미지에 `.msu`를 그대로 `Add-WindowsPackage` 할 수 있는지 확인한다. 안 되면 `.msu`에서 SSU cab을 꺼내 먼저 넣는 순서로 바꾼다.
- `.vhd`(VHD 형식)를 `Mount-WindowsImage`로 직접 열 수 있는지 확인한다. 안 되면 `Mount-VHD`로 붙이고 드라이브 문자 경로에 `Add-WindowsPackage -Path X:\`를 쓴다.
- 적용 뒤 component store 정리(`/StartComponentCleanup`)가 크기와 첫 부팅 시간에 주는 효과를 확인한다. 기본값은 정리하지 않는 것이다.

### 3.3 runner와 evidence 변경

- runner의 `BaseVhdPath` 기본값을 `current-base.json`에서 읽는다. 파일이 없으면 지금 경로(원본)를 쓴다. 명시 인자가 있으면 그것이 우선한다.
- summary에 `base_vhd_path`, `base_vhd_ubr`(sidecar 값)를 더한다. `pre_update_os.ubr`는 이미 있다.
- Windows Update 단계는 바꾸지 않는다.
  - base가 최신이면 `update_count=0`으로 검색만 하고, 재부팅과 무응답 대기가 없다.
  - base 갱신 뒤 새 LCU가 나오면 그달 것 하나만 설치한다. 최근 base라 설치가 작다.
- evidence 문서의 "Windows Update" 행에 base UBR과 설치 수를 함께 적는다. 이것으로 "최신 LCU가 적용된 clean host에서 install/update/rollback"이라는 기존 주장은 그대로 유지된다.

## 4. 검증

- **도구**: Pester(`manual-admin-tests/`)
  - dry-run이 쓰기 없이 계획만 내는지
  - 원본을 덮어쓰지 않는지
  - 실패하면 `-Discard`하는지
  - UBR 불일치를 거부하는지
  - sidecar 형식이 맞는지
- **C# 정적 계약**: 원본 보존, dry-run 기본, 다운로드 없음
- **첫 갱신 뒤 clean-host 한 run**
  - `pre_update_os.ubr`가 새 base UBR과 같다.
  - `update_count`가 `0`이거나, 갱신 뒤 나온 LCU 1개다.
  - 재부팅과 무응답 복구가 없다(0개일 때).
  - 전체 시간을 0.42.83 run(46분)과 비교해 기록한다.

## 5. 위험

| 위험 | 대응 |
| --- | --- |
| DISM 실패로 복사본 손상 | 복사본에만 적용하고 실패하면 지운다. 원본은 읽기만 한다 |
| 새 base에서 첫 부팅이나 OOBE 문제 | current 지정 전에 clean-host 한 run으로 확인한다. 실패하면 `current-base.json`을 이전 base로 되돌린다 |
| evaluation 만료 | clone마다 OOBE를 새로 하므로 run별 평가 기간이 새로 시작한다. 오프라인 적용은 이 동작을 바꾸지 않는다 |
| 디스크 사용량 | base 하나가 약 10GB다. 최근 두 개만 남긴다 |
| 재현성 | sidecar에 KB, `.msu` SHA, 원본과 결과 SHA, UBR을 남긴다 |

## 6. 승인 범위 (구현 때)

- **Lane 1**: 새 도구, Pester, C# 계약, runner 기본값과 summary 필드
- **Lane 2(별도 승인)**
  - LCU `.msu`를 사람이 받는다.
  - 관리자 권한으로 VHD 복사본을 mount하고 DISM을 적용한다. 호스트 설정이나 제품은 바꾸지 않는다.
  - 새 base로 clean-host 한 run을 돌린다. VM 생성과 삭제가 포함된다.
- **범위 밖**: WSUS, Connected Cache, private 저장소 image-cache 구조 변경(경로는 그대로 쓴다)

## 7. Nonclaims

- 이 설계 절(§1~§6)을 쓸 때는 구현하지 않았고, VHD를 mount하거나 업데이트를 받지 않았다. 구현 결과는 §8에 있다.
- public trusted signing과 external stable publication을 주장하지 않는다.

## 8. 구현 결과 (2026-09-30)

§3.2에서 확인하기로 한 사항의 답과, 설계와 달라진 점이다.

| 항목 | 결과 |
| --- | --- |
| `.vhd` mount | `Mount-WindowsImage -Index 1`로 직접 열린다. `Mount-VHD` 대안은 필요 없다 |
| 합쳐진 `.msu` | 오프라인 `20348.169` 이미지에는 한 번에 넣을 수 없다(`0x800f0823`, LCU가 SSU `20348.1960` 이상 요구) |
| 적용 순서 | 도구가 `.msu`를 `%SystemRoot%\System32\expand.exe`로 풀고 `SSU-*.cab` → KB cumulative cab 순서로 적용한다 |
| 실행 host | Microsoft Store판 PowerShell에서는 이미지의 `dismhost.exe` COM 생성이 실패한다(`0x80040154`). 도구가 packaged host를 거부한다. Windows PowerShell 5.1(`powershell.exe`)로 실행한다 |
| component store 정리 | 하지 않았다. 효과는 재지 않았다 |
| base 크기 | `20348.5622` base가 약 18.0GB다. §5의 "약 10GB" 가정보다 약 7.8GB 크다. base 두 개를 남기면 원본을 빼고 약 36GB다 |
| 생성 시간 | 약 26.5분(base를 만들 때 한 번) |
| clean-host run | 134초, `update_count=0`, 재부팅 없음. 0.42.83 run(base `20348.169`)은 46분이었다 |

evidence:
- `clean-host-base-vhd-offline-refresh-2026-09-30-5622`
- `internal-clean-host-install-update-rollback-smoke-2026-09-30-04278-04283-base5622`

`current-base.json`은 `20348.5622-20260930.vhd`를 가리킨다.

매달 할 일:
1. 새 LCU `.msu`를 받는다.
2. 도구 build 모드로 새 base를 만든다.
3. 새 base를 `-BaseVhdPath`로 명시해 clean-host 한 run을 돌린다.
4. PASS이면 current 모드로 지정한다.
