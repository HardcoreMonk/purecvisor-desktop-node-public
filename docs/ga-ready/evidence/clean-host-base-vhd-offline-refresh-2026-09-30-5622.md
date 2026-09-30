# clean-host base VHD 오프라인 갱신 `20348.169` -> `20348.5622`

evidence_id: `clean-host-base-vhd-offline-refresh-2026-09-30-5622`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
tool: `packaging/windows-desktop-node/tools/New-PcvCleanHostBaseVhd.ps1`
tool_commit: `e19539b`
base_file: `20348.5622-20260930.vhd`
base_sha256: `88caf8aae8e51b0756e674f26c175e7953250f4bf4c00004d48b1312546a521e`
base_size_bytes: `17994147328`
source_file: `20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd`
source_sha256: `588355586a3b99f1d47cee02f4861680a7e1bcb353582fbe7da11e2988e7562f`
kb: `KB5122882`
package_sha256: `77e093c74d89421510987f2097a7416ea57a3077eaf81facccfc576239ab5b07`
offline_ubr: `5622`
current_base_changed: `false`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

- 원본은 Windows Server 2022 Datacenter Evaluation VHD(`20348.169`, 2021-08)다. private 저장소 `artifacts/image-cache/windows-server-2022-eval-vhd/`에 있다.
- LCU는 `2026-09 Cumulative Update for Microsoft server operating system version 21H2 for x64-based Systems (KB5122882)`다.
  - Microsoft Update Catalog update id: `45c26f42-4003-45bf-a463-e0e91c88a205`
  - 파일: `windows10.0-kb5122882-x64_4432fee39b6b3ea7136f5ca83544a997ad0a4b5e.msu`(`589944414` bytes)
  - SHA-1은 catalog digest와 같다. Authenticode는 `Valid`(Microsoft Corporation)다.
  - 운영자 단계에서 받았다. 도구는 내려받지 않는다(`download_performed=false`).
  - 0.42.83 clean-host run이 Windows Update로 설치한 것과 같은 KB다(`internal-clean-host-install-update-rollback-smoke-2026-09-29-04278-04283`).

## 실행

2026-09-30 `01:50:55Z`~`02:17:27Z`(1592초)에 Windows PowerShell 5.1(관리자)에서 실행했다.

| 단계 | 결과 |
| --- | --- |
| `.msu` 풀기 | `SSU-20348.5614-x64.cab`, `Windows10.0-KB5122882-x64.cab`, `WSUSSCAN.cab` |
| 복사 | 원본 → `20348.5622-20260930.vhd` |
| mount | `Mount-WindowsImage -Index 1`로 `.vhd`를 직접 mount |
| SSU 적용 | `SSU-20348.5614-x64.cab`(`c60e06a8…`) |
| LCU 적용 | `Windows10.0-KB5122882-x64.cab`(`c1c9e74b…`) |
| 오프라인 확인 | `SOFTWARE` hive의 `CurrentBuild=20348`, `UBR=5622`, `EditionID=ServerDatacenterEval` |
| unmount | `Dismount-WindowsImage -Save` |
| sidecar | `20348.5622-20260930.vhd.base.json`(`pcv-clean-host-base-vhd-v1`) |

DISM 로그의 Error 줄은 `0`개다. component store 정리(`/StartComponentCleanup`)는 하지 않았다.

## 설계 §3.2 확인 결과

앞선 세 번의 시도는 모두 실패했다. 도구는 각 실패에서 mount를 `-Discard`로 풀고 복사본을 지웠다.

| 시도 | 결과 | 원인과 조치 |
| --- | --- | --- |
| 1 | 실패 | Microsoft Store판 PowerShell 7.6.6에서 이미지의 `dismhost.exe` COM 생성이 `0x80040154`로 실패했다. 도구가 packaged host를 복사 전에 거부하도록 고쳤다(Task 4a). |
| 2 | 실패 | 합쳐진 `.msu`를 한 번에 넣으면 `CBS_E_NEW_SERVICING_STACK_REQUIRED`(`0x800f0823`)로 실패한다. LCU `20348.5622`는 SSU `20348.1960` 이상을 요구하고, 이미지는 `20348.169`다. SSU cab을 먼저 적용하도록 고쳤다(Task 4a). |
| 3 | 실패 | Git Bash에서 물려받은 PATH 때문에 `expand.exe`가 coreutils `expand`로 해석됐다. 복사 전에 실패했다. System32 절대 경로로 고쳤다(`e19539b`). |
| 4 | PASS | 이 문서의 결과다. |

- `.vhd`(VHD 형식)는 `Mount-WindowsImage`로 직접 열린다. `Mount-VHD` 대안은 필요 없었다.
- 원본 VHD는 attach되지 않았고 바뀌지 않았다. 원본은 보존 VM `pcv-guest-installed-04253-r1`(Off)의 differencing 부모다.

## 관측

- base 크기는 `17994147328` bytes(약 16.8GiB)다. 원본(`10208214528` bytes)보다 약 7.8GB 크다. 설계 문서의 "base 하나 약 10GB" 가정보다 크므로, 최근 base 두 개를 남길 때의 디스크 사용량을 다시 잡아야 한다.
- 생성 시간은 약 26.5분이다. base를 만들 때 한 번만 든다.

## 아직 하지 않은 것

| 항목 | 상태 |
| --- | --- |
| 새 base로 clean-host 한 run | `not-run`(Task 5) |
| `current-base.json` 지정 | `not-set`(Task 5 PASS 뒤) |
| component store 정리 효과 | `not-measured` |

## Nonclaims

- 이 기록은 base VHD 생성만 증명한다. 제품 install/update/rollback은 이 base로 아직 실행하지 않았다.
- operational current(`0.42.83-admin-smoke`)를 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
