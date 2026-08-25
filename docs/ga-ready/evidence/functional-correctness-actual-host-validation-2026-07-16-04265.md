# Functional correctness actual-host validation 2026-07-16 0.42.65

evidence_id: `functional-correctness-actual-host-validation-2026-07-16-04265`
result: `PASS_WITH_DOCUMENTED_HOST_LIMITATION`
evidence_scope: `internal-admin-smoke-actual-hyperv-vm`
version: `0.42.65-admin-smoke`
qos_artifact: `artifacts/functional-correctness-qos-actual-vm-20260716-04265/summary.json`
disk_artifact: `artifacts/functional-correctness-disk-actual-vm-20260716-04265/summary.json`
host_mutation_performed: `true`
validation_vm_cleanup: `PASS`
validation_root_cleanup: `PASS`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실측 결과

| 검증 항목 | 요청 | 실제 결과 | 판정 |
| --- | --- | --- | --- |
| Network QoS maximum 변환 | `2048 Kbps` | WMI `MaximumBandwidth=2,048,000 bps` | `PASS` |
| Network QoS minimum 0 | `0 Kbps` | 적용 job `succeeded`, 적용 evidence `0 Kbps` | `PASS` |
| Disk shrink guard | `10 GiB -> 9 GiB` | `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`, 크기 `10,737,418,240` bytes 유지 | `PASS` |
| Disk expansion | `10 GiB -> 11 GiB` | job `succeeded`, 크기 `11,811,160,064` bytes | `PASS` |
| Cleanup | 검증 VM·임시 root 제거 | 남은 validation VM/root `0/0` | `PASS` |

QoS 성공 job은 `job-bbf1747d6aff4216b93fd386fbd28eb9`다. Job evidence의 applied policy는
maximum `2048 Kbps`, minimum `0 Kbps`였고 cleanup 직전 `Get-VMNetworkAdapter`의
`BandwidthSetting.MaximumBandwidth`로 `2,048,000 bps`를 직접 확인했다. 설치본 `pcvcli vm
bandwidth`는 현재 numeric applied QoS를 투영하지 않는 inventory readback contract이므로 job
evidence와 raw Hyper-V readback이 이 실측의 authoritative evidence다.

Disk shrink job `job-d5b7abdabb8a4decabe7bcef73fcb8bd`는 의도한 product problem code로
실패했고 VHDX 크기는 변하지 않았다. 이어서 expansion job
`job-34c2b3e6b2224ce38579e46341193460`은 성공했으며 11 GiB가 적용됐다. 검증 VM
`pcv-fc-04265`와 전용 임시 root는 delete job
`job-7b65a09401ee4d90b51263dcf3df2b00` 뒤 모두 제거됐다.

## 확인된 호스트 제약

Hyper-V `Default Switch`에서 minimum `1024 Kbps`를 함께 요청한 probe
`job-a06e4d7b2ec04479ba6e68bdb453085c`는 native `0x80070057` /
`PCV_HYPERV_WMI_JOB_FAILED`로 거부됐다. 이는 maximum Kbps→bps 변환 실패가 아니라 Default
Switch가 non-zero bandwidth Reservation을 지원하지 않는 호스트 제약으로 분류한다. 현재 API는
이 조합을 queue 단계에서 선제 차단하지 않으므로 후속 UX/problem-code 개선 후보로 남긴다.

최초 physical adapter 표시 이름을 target으로 사용한 read-only preflight job은
`PCV_VM_NETWORK_QOS_TARGET_NOT_FOUND`로 실패했고 host mutation 없이 `adapter0` route alias로
교정했다. 해당 job은 기능 판정에서 제외했으며 artifact에 별도로 기록했다.

이 evidence는 실제 Hyper-V VM과 VHDX를 생성·변경한 뒤 모두 제거한 internal admin-smoke다.
Installed update/rollback compensation, public trusted signing 또는 외부 stable publication은 이
evidence의 PASS 범위가 아니다.

## 부록 A. FC-13 Gen2 ISO 부팅 실호스트 검증 (2026-08-05)

이 부록은 위 snapshot을 수정하지 않는다. 2026-07-15에 부팅 가능한 ISO 부재로 skip됐던 FC-13을
같은 호스트에서 실제 ISO로 처음 끝까지 검증한 결과만 추가한다.

- 대상 ISO: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso` (`2918598656` bytes, read-only 사용)
- 검증 방식: 제품 `DesktopNodeHyperVWmiVmCreateProvider`로 throwaway Gen2 VM 생성 후 관측
- Host mutation performed: `true` (throwaway VM create/start/stop/delete)
- 기존 VM `pcv-guest-installed-04253-r1`은 조회 외 조작하지 않았다
- Cleanup: VM `0`개 누수, `D:\pcv-fc13-*` 디렉터리 `0`개 잔여, ISO 원본 무결

### 결함 1 — Gen2 boot order (수정함)

수정 전 제품이 생성한 VM의 `BootSourceOrder` 실측값이다.

| 순서 | BootSourceType | FirmwareDevicePath |
| ---: | ---: | --- |
| 1 | `2` | `MAC(000000000000)` (네트워크/PXE) |
| 2 | `1` | `Scsi(0,0)` (빈 VHD) |
| 3 | `1` | `Scsi(0,1)` (제공한 ISO) |

제품은 boot order를 설정하지 않았고 Hyper-V 기본 정렬이 ISO를 맨 뒤에 놓았다. `--iso_path`로
OS를 설치하려는 VM이 PXE와 빈 디스크를 먼저 시도한다.

`PreferDvdBootSource`를 추가해 Generation 2에서 DVD를 1순위로 올렸다. 수정 후 실측값은
`Scsi(0,1)` / `MAC(...)` / `Scsi(0,0)` 순이며 UEFI Boot Summary도 `1. SCSI DVD (0,1)`을 먼저
시도했음을 보여준다.

### 결함 2 — Secure Boot 템플릿 (미수정, 결정 필요)

boot order를 고친 뒤에도 ISO는 부팅하지 못했다. UEFI Boot Summary 실측이다.

```text
1. SCSI DVD    (0,1)    The signed image's hash is not allowed (DB)
2. Network Adapter      A boot image was not found.
3. SCSI Disk   (0,0)    The boot loader did not load an operating system.
```

제품이 만든 VM은 `SecureBoot=On`, `SecureBootTemplate=MicrosoftWindows`다. 이 템플릿은 Microsoft
third-party UEFI CA를 신뢰하지 않으므로 Linux shim이 거부된다. 즉 제품의 `--iso_path`는 사실상
Windows ISO만 부팅할 수 있다.

가설을 실측으로 확인했다. 같은 VM에서 템플릿만 `MicrosoftUEFICertificateAuthority`로 바꾸고
재부팅하자 Ubuntu 설치 관리자 언어 선택 화면까지 진입했다. 이 변경은 Secure Boot를 끄지 않으며
신뢰 범위만 넓힌다.

제품 기본값 변경은 보안 경계 결정이므로 이 slice에서 수행하지 않았다. 선택지는 기본 템플릿 변경,
create 파라미터 노출, 현행 유지와 문서화다.

### 결함 2 후속 — Secure Boot 템플릿 기본값 변경 (수정함)

사용자 결정으로 기본 템플릿을 `MicrosoftUEFICertificateAuthority`로 바꿨다. Secure Boot는 계속
켜져 있고 신뢰하는 서명자 범위만 넓어진다. 임의 ISO 경로를 받는 create route와 맞는 선택이다.

수정 후 제품 경로만으로 재검증했다. 수동 개입 없이 create -> start만으로 관측한 값이다.

```text
SecureBootEnabled       True
SecureBootTemplateId    272E7447-90A4-4563-A4B9-8E4AB00526CE
BootSourceOrder         Scsi(0,1) / MAC(...) / Scsi(0,0)
```

VM 화면은 Ubuntu 설치 관리자 언어 선택 화면에 도달했다. `MicrosoftWindows` 템플릿에서 나오던
`The signed image's hash is not allowed (DB)`는 재현되지 않았다.

### 판정

- FC-13 boot order: `FIXED` / 실호스트 검증 완료
- FC-13 Secure Boot 템플릿: `FIXED` / 실호스트 검증 완료
- FC-13 전체: `PASS` / 제품 경로만으로 Gen2 VM이 제공한 ISO로 부팅한다. 2026-07-15 skip 이후
  처음으로 끝까지 검증했다.
- 이 검증은 code-level이 아니라 실제 호스트 관측이지만 operational anchor를 승격하지 않는다.
  public trusted signing과 external stable publication을 주장하지 않는다.
