# 후속 작업 내역서 (2026-08-06)

작성 기준: `codex/manual-admin-04269-04270-closure` 브랜치, commit `e9138988`과 `72fac949`
범위: manual-admin `0.42.69 -> 0.42.70` closure와 `0.42.70` anchor 승격
결과: 세 평면(anchor / 설치본 / manual-admin closure) 모두 `0.42.70-admin-smoke` 정렬
merge 상태: `main`에 병합하지 않음

---

## 1. 착수 시점 상태

| 항목 | 값 |
| --- | --- |
| `main` HEAD | `bb65fd1b` |
| 직전 커밋 | `276afeca` docs: record manual-admin readiness and the cleared 68-day blocker |
| 문서상 manual-admin closure | `0.42.58 -> 0.42.59` (2026-05-29) |
| 문서상 campaign 진행도 | runner 6종 중 `manual-admin-readiness` 1종만 실행 |
| baseline 예약 | `caf337be-e5f8-4bcb-8b9f-719167f2dc12`, `reserved`, 만료 `2026-08-06T10:23:14Z` |

## 2. 조사에서 드러난 실제 상태

착수 지시는 "후속 작업 진행"이었고, 문서 기준으로는 runner 5종을 실행하는 작업으로 보였다.
실측 결과는 달랐다.

### 2.1 직전 커밋 이후 실행된 미기록 결과

`276afeca`는 2026-08-05 `19:33`에 커밋됐다. 그 **직후** 같은 날 runner 4종이 실행돼 모두
PASS했고, 결과가 커밋되지 않은 채 `artifacts/`(gitignore 대상)에만 남아 있었다.

| runner | 커밋 문서의 기록 | 실제 |
| --- | --- | --- |
| `manual-admin-readiness` | `PASS` | `PASS` (19:24) |
| `installed-product-update-rollback` | `not-run` | `ok=true` (20:04) |
| `clean-host-install-update-rollback` | `not-run` | plan만 생성, summary 없음 (20:05) |
| `burn-install-repair-remove` | `not-run` | `ok=true`, 11/11 check (20:07) |
| `msix-build-install-update-remove` | `not-run` | `ok=true` (20:08) |
| `installed-runtime-ops-summary` | `not-run` | `ok=true` (20:09) |

따라서 실제 잔여 작업은 runner 5종이 아니라 `clean-host` 1종이었다.

### 2.2 설치본/문서 버전 불일치

campaign의 `installed-product-update-rollback`이 호스트를 target에 남기므로 설치본은
`0.42.70`이었으나 커밋된 anchor 문서는 `0.42.69`였다. 직전 커밋이 "완주하지 못하면 정렬이
다시 어긋난다"고 예고한 상태가 실제로 발생해 있었다.

### 2.3 고아 VM

`clean-host` 첫 시도가 중단되면서 VM `pcv-cleanhost-20260805-04269-04270`이 남았다.
`RemoveVmOnFailure`가 꺼져 있어 정리되지 않았고, 2026-08-06 `11:42` 호스트 재부팅 때
`AutomaticStartAction=StartIfRunning`으로 자동 재개돼 실행 중이었다.

---

## 3. 실행한 작업

### 3.1 commit `e9138988` — manual-admin package pair closure

| 단계 | 내용 |
| --- | --- |
| 고아 VM 정리 | VM과 VM root 제거, base VHD 무결성 확인 |
| clean-host 재실행 | `KB5099540`, UBR `169 -> 5386`, install/update/rollback exit `0`, `final_web_status_code=200`, `blocker=none`, 성공 시 VM 자동 제거 |
| descriptor 생성 | `runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass` |
| evidence 문서 | `docs/ga-ready/evidence/manual-admin-campaign-2026-08-06-04269-04270.md` 신규 |
| readiness 문서 | point-in-time 기록임을 명시하고 forward pointer 추가 |
| `current-evidence.json` | `latest_closed_*`를 `0.42.69 -> 0.42.70`으로 갱신 |

2026-05-29 이후 `69`일 만의 첫 manual-admin closure다.

### 3.2 commit `72fac949` — `0.42.70` anchor 승격

| 단계 | 결과 |
| --- | --- |
| full admin host mutation gate | `service-msi-hyperv-admin-smoke` exit `0`/1회/`91s`, `os-mutation-gate` exit `0`/1회/`11s` |
| 설치본 Hyper-V 실측 | Gen2 VM 생성/checkpoint/삭제, unmanaged delete guard 동작 확인 |
| 정리 상태 | 잔여 `pcv-spike-*` VM `0`, firewall `0`, Event Log source 없음, boot time 불변 |
| installed current-card | CLI `3/3` exit `0`, Web `2/2` HTTP `200`, service `Running/Automatic`, `secret_observed=false` |
| evidence 문서 | package / fullgate / functional / current-card `4`건 신규 |
| `current-evidence.json` | `current` 블록 전체를 `0.42.70`으로 승격 |

package는 재빌드하지 않았다. `0.42.69` 승격 때는 후보가 커밋 `13`건 뒤처져 재빌드가
필요했으나, 이번에는 campaign target package(provenance `821a6a34`)와 승격 시점 HEAD 사이
커밋 `4`건이 모두 docs이거나 MSI payload에 포함되지 않는 `tools/**` 스크립트였다.
payload source 경로 diff `0`건을 실측 확인하고 그 근거를 evidence 문서에 남겼다.

---

## 4. 발견한 결함과 조치

### 4.1 루트 문서 7종에 하드코딩된 허위 주장 (조치 완료)

`Update-PcvCurrentEvidenceDocs.ps1`이 functional evidence 줄에
`QoS 2048 Kbps -> 2,048,000 bps, disk shrink guard and 10 -> 11 GiB expansion PASS`를
무조건 찍고 있었다. gate artifact 실측 결과 이 버전에서 `disk_resize`/`shrink`/`expand`
관측은 `0`건이고 `qos` 문자열은 `storage_qos`/`network_qos` **readback contract 필드**로만
등장한다. 세 항목은 `0.42.65` 이월이다.

`0.42.69` anchor의 functional 문서도 같은 세 항목을 이월로 기록하고 있었으므로, 최소 한 번의
직전 anchor에서도 생성 블록이 과장 주장을 해왔다. 어떤 테스트도 이 문구를 검증하지 않아
드러나지 않았다.

조치: 하드코딩 절을 제거하고 evidence 문서가 재실행/이월 구분을 소유하게 했다.

### 4.2 `blocked_*` 스키마가 비진실을 강요 (조치 완료)

closure로 `blocked-by-installed-baseline-version-mismatch`가 해소되면서 기록할 blocker가
없어졌다. 그러나 `blocked_baseline`/`blocked_target`/`blocked_reason` 3종은 schema
`required`에 `minLength: 1`이었고 생성기는 `Blocked follow-up` 줄을 무조건 출력했다.
`none` 같은 값을 넣으면 "Blocked follow-up: `none`" 형태의 무의미한 줄이 남는다.

조치: 세 필드를 optional로 바꾸고 all-or-none guard를 추가했다. 부분 지정은
`PCV_CURRENT_EVIDENCE_INVALID|manual_admin.blocked_*|partial-blocked-triple`로 거부한다.
양쪽 분기를 실제로 실행해 확인했다.

### 4.3 frozen-status 테스트 결함 2건 (조치 완료)

| 단언 | 문제 | 조치 |
| --- | --- | --- |
| ledger `current_manual_admin_package_pair` | 손으로 유지하는 행이 갱신되지 않아 실패 | 행 갱신 |
| `manual-admin-package-pair-current` 행 버전 고정 | 04262 historical 표에 대응 행이 없어 current 행만 매칭. 고정 시 정당한 closure마다 실패 | canonical record 추적으로 변경 |

두 번째는 같은 파일의 anchor 행들이 이미 같은 이유로 같은 방식으로 고쳐져 있던
`docs/project-status-audit-2026-08-05.md` §3.2의 frozen-status 결함과 동일하다. 04262
historical 고정 단언은 그대로 두었다.

### 4.4 기록된 blocker가 실제 blocker가 아니었던 사례 (확인)

readiness 문서는 `WixToolset.Bal.wixext 5.0.2`의 `damaged` 상태를 Burn 번들 빌드 전 재설치가
필요할 수 있는 미해결 항목으로 남겼다. Burn runner는 이 확장을 쓰지 않는다.
`WixToolset.BootstrapperApplications.wixext`로 exit `0` 빌드하고 install/repair/remove/restore가
모두 통과했다.

이는 readiness 문서 §"세 번 반복된 관측 오류"가 기록한 패턴의 **네 번째** 사례다.

### 4.5 stale ledger 행 (조치 완료)

`current_full_admin_host_mutation`이 `0.42.65`로 남아 있었다. `0.42.69` 승격 때 갱신되지
않은 것으로, 이번에 함께 바로잡았다.

### 4.6 도구 사용상 함정 (미조치, 기록만)

| 항목 | 내용 |
| --- | --- |
| Batch Supervisor `-DryRun` | dry-run이 실제 실행과 **같은** `artifact_root`의 `summary.json`을 덮어쓴다. dry-run 산출물을 완료된 gate로 오독할 수 있다. `dry_run: true` 플래그로 구분은 가능하다 |
| `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1` | `Join-Path (Get-Location) $ArtifactRoot`를 쓰므로 `-ArtifactRoot`는 **상대 경로**여야 한다. 절대 경로를 주면 `D:\repo\D:\repo\...`가 되어 즉시 실패한다 |
| clean-host runner 실패 시 | `RemoveVmOnFailure` 기본 off + VM `AutomaticStartAction=StartIfRunning` 조합으로, 중단된 실행의 VM이 호스트 재부팅 때 되살아난다 |

> **추가 (2026-08-06 후속 커밋에서 조치 완료):** 세 항목 모두 수정했다. dry-run은
> `summary.dry-run.json`/`current-step.dry-run.json`/`batch-manifest.resolved.dry-run.json`으로만
> 쓰고 실제 실행 산출물을 건드리지 않는다. `-ArtifactRoot`는 `IsPathRooted` 분기로 절대/상대
> 경로를 모두 받는다. clean-host runner는 VM 생성 직후 `AutomaticStartAction=Nothing`을 설정해
> 고아 VM이 호스트 재부팅 때 되살아나지 않게 했다(`vm_automatic_start_action` summary 필드로 기록).

---

## 5. 검증

두 커밋 모두 아래를 통과했다.

```powershell
Invoke-Pester -Path 'packaging/windows-desktop-node/tests'            # 474/474
Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'  #  49/49
Invoke-Pester -Path 'web/tests'                                       #  49/49
packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check  # 7/7 current
git diff --check                                                      # PASS
```

`Update-PcvCurrentEvidenceDocs.ps1`은 Windows PowerShell 5.1과 pwsh 7 양쪽에서 실행 확인했다.
초기 수정에 PS7 전용 `Join-String`을 썼다가 이식 가능한 `-join`으로 교체했다.

---

## 6. 최종 상태

| 평면 | 착수 시점 | 종료 시점 |
| --- | --- | --- |
| canonical anchor | `0.42.69-admin-smoke` | `0.42.70-admin-smoke` |
| 설치본 | `0.42.70-admin-smoke` (문서 미반영) | `0.42.70-admin-smoke` |
| manual-admin closure | `0.42.58 -> 0.42.59` (69일 경과) | `0.42.69 -> 0.42.70` |
| blocked follow-up | `0.42.62 -> 0.42.63` mismatch | 없음 |

주요 해시:

| 항목 | 값 |
| --- | --- |
| clean package MSI | `b28e18763ac01137039a9bcfafe0c151945304c8449e307b0412038d6726c86c` |
| operational fullgate MSI | `90aeda60633ec7e6d32d88f71cbea2b2d5bb54eff205cf49d51cd894b44d8165` |
| operational payload aggregate | `625a08ce4fcc4435c2ffa9af6804dbffc9c4b87450ea4b0613b1df52cb217f99` |
| update ZIP | `72d7f2927e21b100f9fdc15ce8c2b4a7923a0577b84d5a58398fdb84a3c7e72a` |
| provenance commit | `e91389880febdfb3c1ba430f97c84c2f7e006591` |

---

## 7. 하지 않은 것

- `main` 병합을 하지 않았다. 두 커밋은 `codex/manual-admin-04269-04270-closure`에 있다.
- QoS 변환, disk shrink guard, disk expansion은 `0.42.65` 이월 상태 그대로다. 이 버전에서
  재검증하지 않았고 재검증했다고 주장하지 않는다.
- `WixToolset.Bal.wixext 5.0.2`의 `damaged` 상태는 해소하지 않았다. 이번 campaign의 blocker가
  아님만 확인했다.
- §4.6의 도구 함정 3건은 기록만 하고 수정하지 않았다. (추가: 같은 날 후속 커밋에서 3건 모두
  수정했다 — §4.6의 추가 절 참조.)
- clean-host VM 폴더 `49`개(합계 `5.0 GB`)가 `C:\ProgramData\PureCVisor\desktop-node\clean-host-vms`에
  남아 있다. C: 여유 `301 GB`라 이번 범위에서 정리하지 않았다.
- public trusted signing과 external stable publication은 이 작업 범위 밖이며 주장하지 않는다.
  모든 evidence는 internal `AllowUnsignedDev`/`LocalTest` admin-smoke 범위다.

---

## 8. 후속 작업 (같은 날, 별도 세션)

이 절은 §1~§7 snapshot을 수정하지 않는다. 그 뒤 이어진 후속 작업의 실측과 처리만 추가한다.
브랜치는 `codex/post-04270-followup-hygiene-gate`이며, 착수 시점 `main` HEAD는 `71fdfcf2`,
`origin/main` 대비 `6`커밋 미푸시 상태였다.

### 8.1 §7이 남긴 항목의 실측 결과

| §7 항목 | 실측 | 처리 |
| --- | --- | --- |
| clean-host VM 폴더 `49`개, 합계 `5.0 GB` | 폴더는 `49`개 맞으나 **합계 `0` bytes**. VHD는 이미 없고 빈 `Snapshots`/`Virtual Machines` 껍데기만 남아 있었다 | 제거 |
| `WixToolset.Bal.wixext 5.0.2` `damaged` | 여전히 `damaged` | 해소 (§8.2) |
| QoS / disk shrink / disk expansion 이월 | 이월 상태 그대로 | `0.42.70`에서 재실행, `3`건 모두 PASS (§8.4) |
| §4.6 도구 함정 3건 "모두 수정" | trap #2는 **`8`개 중 `1`개만** 수정돼 있었다 | 나머지 `7`개 수정 + 디렉터리 가드 (§8.3) |

`5.0 GB`는 기록 시점에는 사실이었을 수 있으나 실측 시점에는 아니었다. 용량을 근거로 "정리하지
않았다"고 판단한 부분은 실측으로 무효가 됐다.

### 8.2 `Bal.wixext damaged`의 실체

근본 원인은 손상된 설치가 아니라 **개명**이었다. WiX 5에서 `WixToolset.Bal.wixext`가
`WixToolset.BootstrapperApplications.wixext`로 바뀌었고, 구 id 패키지는 신 DLL만 담은 shim이라
캐시 폴더 이름과 assembly 이름이 어긋난다. WiX 로더는 이 불일치를 `damaged`로 표시한다.
`wix extension remove` 후 정본 id로 재설치해 해소했다.

부수로 `New-PcvBurnBootstrapperPreflight.ps1`이 운영자에게 `-ext WixToolset.Bal.wixext`를
안내하고 있었다. 저장소 전체에서 정본 id를 참조하는 곳은 `0`건이었고, 어떤 테스트도
`build_command`를 검증하지 않았다 — §4.1이 제거한 하드코딩 문자열과 같은 계열이다. 단언을 먼저
추가해 RED를 확인한 뒤 고쳤다.

### 8.3 trap #2는 한 파일만 고쳐져 있었다

`37e07a78`은 `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1` 하나만 `IsPathRooted`
분기로 고쳤다. `-ArtifactRoot`를 받는 나머지 runner 중 `7`개가 같은 무방비
`Join-Path (Get-Location) $ArtifactRoot`를 그대로 갖고 있었다. `7`개 전부에 같은 분기를 적용하고,
디렉터리 전체를 검사하는 계약 테스트를 추가했다.

계약 테스트의 판정 기준은 `Join-Path` 문자열 자체가 아니라 **`IsPathRooted` 가드의 부재**다. 수정된
형태도 상대 경로 else 분기에 같은 표현을 남기기 때문이다. 첫 시도에서 이 구분을 놓쳐 정상 파일까지
위반으로 잡았고, 테스트가 그 오류를 잡아내 기준을 바로잡았다.

### 8.4 이월 3항목 재검증

`Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`을 추가하고 `0.42.70` 설치본에 대해 VM
`pcv-fc-cf-5e6f4823`으로 실행했다. QoS `2048 Kbps -> 2,048,000 bps`(adapter readback), disk shrink
guard `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`(크기 불변), disk expansion `11,811,160,064` bytes가 모두
PASS했고 검증 VM과 임시 root는 제거됐다. 소유 evidence는
`docs/ga-ready/evidence/functional-correctness-carry-forward-revalidation-2026-08-06-04270.md`다.

`0.42.65` 실측이 전용 runner 없이 손으로 수행돼 재현되지 않은 것이 이월이 반복된 원인이었으므로,
이번에는 절차를 runner로 고정했다.

### 8.5 `main`이 깨진 테스트를 담고 있었다

착수 직후 packaging suite가 `481/482`였다. 실패한
`PcvBatchSupervisor ... dry-runs an admin profile with explicit approval`은 `37e07a78`이 dry-run
산출물을 `summary.dry-run.json`으로 옮기면서 함께 갱신하지 못한 테스트였다. 같은 커밋이 다른 두
테스트는 갱신했다. 변경을 stash하고 깨끗한 `71fdfcf2`에서 재현해 제 작업과 무관함을 확인한 뒤
고쳤다.

### 8.6 FC-05 / FC-12(b) / FC-13

감사 §12.5가 "부재"로 본 격리 Windows guest와 전용 credential은 실제로 호스트에 있었다. FC-05는
`71`일 만에 재검증해 PASS했다. FC-13은 계약 test를 추가해 닫았고, FC-12(b)는 호스트 측만 닫히고
guest 측 왕복은 미확정으로 남았다. 상세와 nonclaim은
`docs/ga-ready/evidence/fc-05-fc-12b-fc-13-verification-2026-08-06-04270.md`가 소유한다.

이 과정에서 keep policy 자산 `pcv-guest-installed-04253-r1`을 기동·종료했고, 그 결과 자동
checkpoint `1`개가 병합돼 `0`개가 됐다. 의도한 변경이 아니며 해당 evidence §5에 기록했다.

### 8.7 검증

```powershell
Invoke-Pester -Path 'packaging/windows-desktop-node/tests'            # 485/485
Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'  #  49/49
Invoke-Pester -Path 'web/tests'                                       #  49/49
dotnet test src/DesktopNode.sln                                       # 787/787, 2회 통과
npm test / verify:parity / browser:fixture --prefix web               # 전부 통과
Update-PcvCurrentEvidenceDocs.ps1 -Check                              # 7/7 current
git diff --check                                                      # PASS
```

위 `787`은 **오독이다**. 정정은 §8.9가 소유한다. `485`는 당시 관측으로는 맞았으나 중복 gate를
제거한 뒤 값이 바뀌었다. 두 값 모두 §8.9의 표를 따른다.

### 8.8 하지 않은 것

- FC-12(b) guest 측 비 ASCII 왕복을 PASS로 주장하지 않는다. `vm.guest.exec`의 특수문자 argv 전달은
  별도 조사 대상으로 남긴다.
- `pcv-guest-installed-04253-r1`의 `AutomaticCheckpointsEnabled`를 끄지 않았다. keep policy 자산에
  대한 추가 mutation이므로 권고로만 남겼다.
- Default Switch non-zero reservation 제약은 재검증하지 않았다.
- 대형 모듈 자체를 분해하지 않았다. 이번에 넣은 것은 순증을 막는 gate이지 분해 작업이 아니다.
  (§9에서 `DesktopNodeHostServiceAction` 1건을 실제로 분해했다.)
- public trusted signing과 external stable publication은 여전히 범위 밖이며 주장하지 않는다.

### 8.9 정정 — §P2-2 라인 수 판단이 틀렸다 (2026-08-06 후속)

위 §8과 그 커밋은 감사 §P2-2의 라인 수 표를 "재현되지 않는다"고 기록했다. **틀렸다.**

원인은 감사가 아니라 당시 쓴 측정 명령이다. `Measure-Object -Line`은 빈 줄을 세지 않아 파일의
빈 줄 수만큼 적게 나온다. 올바르게 세면 C# `3`종이 감사 수치와 정확히 일치한다
(`4,069` / `3,367` / `2,038`). 상세는 `docs/project-status-audit-2026-08-05.md` §15의 정정 절이
소유한다.

이 오류는 라인 수 gate를 도입한 커밋 `9f80a888`과 PR #184 본문에도 들어갔다. 결함 있는 측정기는
그 gate 안에만 있었고, gate 자체가 중복으로 판명돼 삭제되면서 함께 사라졌다. 남은
`PcvModuleSizeRatchet`의 `Measure-RepoFileLines`는 `Get-Content -Raw`로 전체 텍스트를 읽어 CRLF를
정규화한 뒤 세므로 이 결함을 가진 적이 없다. **측정기를 교체하거나 빈 줄 회귀 테스트를 추가한
사실은 없다.** 그렇게 적었던 서술은 커밋 `49aaedbd`에서 정정했다.

이 항목은 §4.1이 제거한 "검증되지 않는 하드코딩 주장"과 같은 계열이다. 이번에는 주장이 아니라
**측정 도구**가 검증되지 않았다.

더 근본적인 문제가 함께 드러났다. **§8이 "도입"했다는 라인 수 gate는 중복이었다.**
`packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`과
`packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`이 커밋 `695189d4`
(`2026-08-05 15:38`)로 이미 존재했다. 같은 목적, 같은 `slack_lines: 50`, 같은 모듈 목록이었고
`max_lines`는 `4069`/`3367`/`2038`/`4005`로 **처음부터 올바른 값**이었다.

즉 감사 §11 표의 "#7 대형 모듈 라인 수 gate 도입 — OPEN"은 작성 시점에 이미 낡은 값이었고,
§8이 그 표를 근거로 중복 gate를 만들었다. 기존 gate를 찾았다면 거기 적힌 `4069`를 보고 측정
결함을 즉시 알아차렸을 것이다. 탐색 실패의 구체적 원인은 테스트 파일명을
`architect|ownership|guard|contract`로만 검색하고 `ratchet`/`max_lines`로는 검색하지 않은 것이다.

조치: 중복 gate(`PcvLargeModuleLineCeiling.Tests.ps1`, `large-module-line-ceiling.json`)를
제거하고 기존 `PcvModuleSizeRatchet`을 단일 gate로 유지한다. 기존 gate의 측정기는 CRLF/LF를
정규화해 `wc -l`과 같게 세므로 빈 줄 결함이 없다.

§8.7이 기록한 두 숫자의 정정은 다음과 같다.

| 명령 | §8.7 기록 | 실제 | 원인 |
| --- | ---: | ---: | --- |
| `dotnet test src/DesktopNode.sln` | `787` | **`825`** | 잘린 출력 창을 읽어 Service `11`, Contracts `21`이 빠지고 HyperV를 boot order test 추가 전 값 `125`로 셌다 |
| `Invoke-Pester packaging/.../tests` | `485` | **`477` + skip `2`** | 관측 시점에는 맞았다. 중복 gate 삭제로 값이 바뀐 것이며 오독이 아니다 |

---

## 9. 코어 대형 모듈 분해 (같은 날, 별도 세션)

이 절도 앞 snapshot을 수정하지 않는다. §8.8이 "하지 않았다"고 적은 대형 모듈 분해를 실제로
수행한 결과만 추가한다. 브랜치는 `codex/host-service-action-decomposition`,
계획은 `docs/superpowers/plans/2026-08-06-purecvisor-desktop-node-host-service-action-decomposition.md`
(`10` task / `76` step), 실행 방식은 subagent-driven이다. Evidence는
`docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`가 소유한다.

### 9.1 결과

| 항목 | 착수 전 | 종료 후 |
| --- | ---: | ---: |
| `DesktopNodeHostServiceAction.cs` | `4,069`줄 | `1,174`줄 (`71`% 감소) |
| `Ops/` 9개 도메인 클래스 합계 | `199`줄 | `3,040`줄 |
| `ExecuteNative*ActionForOps` forwarder | `9`개 | `0`개 |
| ratchet ceiling (host 파일) | `4069` | `1174` |

제거한 것은 `ExecuteAsync -> Ops.X.Execute -> ServiceAction.ExecuteNativeXActionForOps` 왕복이다.
공개 표면(`CreatePlan`, `ExecuteAsync` 4개 오버로드, `EnsureProtectedTokenFile`,
`EnsureAccountAuthBootstrapFiles`)은 불변이며 호출자 `Program.cs` `1`곳과 테스트 `69`곳은
수정하지 않았다.

### 9.2 gate 통합

§8.9가 지목한 중복 gate를 이 브랜치에서 제거했다. `PcvModuleSizeRatchet`이 단일 gate이고
ceiling은 task마다 실제 값으로 내려 `4069 -> 3949 -> ... -> 1174`로 라쳇을 걸었다.

### 9.3 ownership guard가 추적 정책을 위반했다

`HostServiceActionOwnershipTests`의 첫 판은 `BindingFlags.NonPublic` 리플렉션으로 선언 멤버를
셌다. 이는 테스트 코드에서 유일한 private reflection이었고
`csharp-architecture-test-migration.json`의 `private_reflection.current_occurrence_count`는 `0`으로
고정돼 있다. `System.Reflection.Metadata`의 `PEReader`/`MetadataReader`로 어셈블리 메타데이터를
직접 읽도록 다시 썼다. 정책 위반 없이 같은 불변식을 잠근다.

### 9.4 검증 (실측)

| 명령 | 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.sln` | 통과 `836`, 실패 `0`, 건너뜀 `0` |
| `Invoke-Pester packaging/windows-desktop-node/tests` | 통과 `477`, 실패 `0`, 건너뜀 `2` |
| `Invoke-Pester packaging/windows-desktop-node/installer/tests` | 통과 `49`, 실패 `0` |
| `Invoke-Pester web/tests` | 통과 `49`, 실패 `0` |
| `npm test --prefix web` | TypeScript 오류 `0`, served `app.js` current |
| `Update-PcvCurrentEvidenceDocs.ps1 -Check` | `7/7 current` |

`main` 병합 후 push(`ef3f5f65`)에서 GitHub Actions `Development Gates` 4개 job과
`Public Boundary Contract`가 모두 success다.

병합된 `main` 체크아웃에서 다시 실측하면 packaging Pester는 `479/0/0`이다. worktree에서 건너뛴
`2`건은 `PcvJobStore04265ReaderCompatibility`의 frozen-host 항목으로,
`artifacts/admin-smoke-package-20260716-04265/host-publish/DesktopNode.Host.exe` 존재를 조건으로
건다. `artifacts/`는 git-ignored라 새 worktree에는 없다. 총 항목 수는 `479`로 같다.

### 9.5 하지 않은 것

- 남은 대형 모듈 `2`종은 손대지 않았다. `DesktopNodeApiRequestProcessor.cs`(`3,367`, 백엔드)와
  `web/src/served-app.ts`(`4,005`, 프론트엔드)는 각각 별도 계획이 필요하다.
- `ServiceTokenRotationRevoke...RedactedAudit` 간헐 실패는 근본 원인을 확정하지 않았다. 이
  브랜치 이전부터 있던 항목이다.
- `DesktopNodeCredentialManagerOps`에 private `ExecuteNativeCredentialManagerActionForOps` wrapper가
  남아 있다. forwarder가 아니라 클래스 내부 헬퍼이며 형제 `8`개 중 `7`개와 형태가 다른
  미관 항목이다.
- host mutation, package build, 설치본 변경은 없다. operational anchor는 `0.42.70-admin-smoke`
  그대로이며 public trusted signing과 external stable publication은 범위 밖이다.

---

## 10. 문서 최신화 (같은 날, 별도 세션)

§8·§9의 결과를 canonical 문서에 반영한 작업이다. 제품 코드 변경은 없다.

### 10.1 처리

| 문서 | 처리 |
| --- | --- |
| `docs/ga-ready/EVIDENCE_INDEX.md` | 2026-08-06 evidence `3`건(이월 재검증, FC 검증, host 분해)이 미인덱스 상태였다. 절 `3`개 추가 |
| `docs/DEVELOPER_INDEX.md` | 2026-08-06 절 추가. current-evidence 생성기 target에 편입 |
| `docs/project-status-audit-2026-08-05.md` | §16 P2-2 분해 addendum 추가. §15의 `787`건 기록을 오독으로 표시 |
| 이 문서 | §8.8/§8.9 순서 교정, §8.7 두 숫자 정정, §9 추가 |
| `docs/CODING_GUIDE.md` | §13.1 자동 강제 목록에 모듈 라인 수 라쳇 추가. §13.3 `향후 게이트`의 ratchet과 구분 명시 |
| `docs/ga-ready/VERIFICATION_OWNERSHIP.md` | `0.42.64 installed current-card`와 `최신 0.35.7 OS gate` 두 고정 pin을 canonical owner 참조로 교체 |

### 10.2 `DEVELOPER_INDEX`는 root README와 같은 결함을 갖고 있었다

이 문서는 "Operational current와 installed non-promoted candidate는 ... 이 문서 최상단 current
section을 우선한다"고 적으면서 **최상단에 그 section이 없었다.** 참조가 끊긴 채 본문은
`0.42.65-admin-smoke`로 drift해 있었다(canonical은 `0.42.70`).

감사 §11이 기록한 root `README.md`의 결함과 같은 계열이고 처리도 같다. 생성기
`Update-PcvCurrentEvidenceDocs.ps1`의 target에 편입해 owned 문서를 `7`개에서 `8`개로 늘렸다.
`PcvCurrentEvidenceGeneration.Tests.ps1`의 target 목록도 함께 갱신했다. 이제 끊긴 참조가
해소되고 drift는 `-Check`가 잡는다.

### 10.3 검증 (실측)

| 명령 | 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.sln` | 통과 `836`, 실패 `0`, 건너뜀 `0` |
| `Invoke-Pester packaging/windows-desktop-node/tests` | 통과 `479`, 실패 `0`, 건너뜀 `0` |
| `Update-PcvCurrentEvidenceDocs.ps1 -Check` | `8/8 current` |
| `git diff --check` | PASS |

### 10.4 하지 않은 것 — `MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`가 stale이다

(추가: 같은 날 후속 작업에서 처리했다 — §11 참조. 아래 서술은 처리 전 판단 기록이다.)

`docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`는 `updated_at: 2026-05-29`이고
`current_manual_admin_package_pair`를 `0.42.58-admin-smoke -> 0.42.59-admin-smoke`로 적는다.
canonical ledger의 현재 closed pair는 `0.42.69 -> 0.42.70`이므로 `11`개 버전, `69`일 stale이다.
`current_*` 접두사를 단 필드 전체가 같은 시점에 멈춰 있다.

이번에 고치지 않았다. `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`이
이 파일을 `10`곳 이상에서 읽고 일부는 `2026-05-14` descriptor id 같은 문자열을 고정 assertion으로
건다. 어느 assertion이 historical baseline이고 어느 것이 current 주장인지 가려야 하며, 잘못
가리면 canonical descriptor에 허위 주장을 심게 된다. §4.3이 이미 두 번 기록한 frozen-status
테스트 결함과 같은 계열이라 별도 slice로 다룬다.

이 문서를 생성기 target에 편입하는 것도 후보지만, `current_*` 필드가 생성 블록 스키마
(`pcv-current-evidence-v1`)에 없는 것들이라 스키마 확장이 선행돼야 한다.

---

## 11. Manual-admin descriptor 최신화와 정합성 gate (같은 날, 별도 세션)

§10.4가 별도 slice로 미룬 항목이다. 우려했던 "고정 assertion 충돌"은 실측 결과 존재하지 않았다.

### 11.1 기존 assertion은 왜 stale을 못 잡았나

`PcvAdminSmokeEvidenceDocs.Tests.ps1`이 descriptor를 읽는 곳은 `10`곳이 넘지만, 전부
**anchor 없는 substring 매칭**이다. 예를 들어

```powershell
$descriptor | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke`'
```

는 문서의 `previous_04256_current_manual_admin_package_pair:` 줄에 매치된다. `previous_04256_`
접두사가 매칭을 막지 못하기 때문이다. 즉 이 assertion들은 **과거 값의 보존**을 검사할 뿐
`current_*`가 현재를 가리키는지는 한 번도 검사하지 않았다. 문서가 `69`일 stale이어도 영원히
green이다.

이것이 §10.4가 "어느 assertion이 baseline이고 어느 것이 current 주장인지 가려야 한다"고 본
문제의 실체다. 답은 **전부 baseline**이었다.

### 11.2 처리

문서 자신의 누적 규약(`current_*` → `previous_<직전버전>_*` 강등)에 따라 갱신했다.

| 평면 | 갱신 전 | 갱신 후 |
| --- | --- | --- |
| `current_manual_admin_package_pair` | `0.42.58 -> 0.42.59` (2026-05-29) | `0.42.69 -> 0.42.70` |
| `current_full_admin_host_mutation_batch` | `...-20260529-04259` | `...-20260806-04270` |
| `current_installed_operator_surface_current_card_evidence` | `...2026-05-29-04259.md` | `...2026-08-06-04270.md` |
| `current_public_boundary_main_push_evidence` | `...2026-05-29-04259-...md` | `...2026-07-13-pr171-...md` |
| `next_manual_admin_package_pair_candidate` | `0.42.59 -> 0.42.60` | `0.42.70 -> 0.42.71` |

강등이 과거 값을 지우지 않았음을 기계로 확인했다. 갱신 전후 문서의 백틱 인용 값 집합을 비교해
**사라진 값 `0`개**를 확인했다. 최초 시도에서는 `descriptor_id`와 `updated_at` 두 값이
사라졌는데, 이는 같은 커밋에서 내가 문서 상단에 쓴 "삭제 없이 덧붙인다" 규약과 모순이므로
`previous_04259_*`로 보존했다.

문서 상단에 접두사 규약을 설명하는 "읽는 법" 절을 추가했다. 이 문서가 `1,000`줄 넘게 누적되는
동안 접두사 의미가 어디에도 적혀 있지 않았고, 그것이 drift를 눈에 띄지 않게 만든 조건이다.

`next_manual_admin_package_pair_candidate`를 연 근거는 실측이다. `0.42.70` package의
source commit `821a6a34` 이후 `src/**`·`web/src/**` 제품 소스 `13`개 파일이 바뀌었다.

### 11.3 새 gate

`packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1` `6`건을 추가했다.
`current_*` 필드의 **줄 시작 고정 첫 occurrence**만 읽어 `docs/ga-ready/current-evidence.json`과
대조한다. 값은 canonical record에서 파생하며 리터럴로 고정하지 않는다. 기존
`PcvCurrentEvidenceGeneration` 테스트가 주석으로 남긴 교훈("버전을 고정했더니 정당한 anchor 승격이
전부 잘못된 이유로 실패했다")을 따른 것이다.

`current_*` 필드가 접두사 없이 두 번 선언되면 실패시킨다. 두 번 선언되는 순간 "현재 값"이
모호해지고 이 gate가 막으려는 substring 구멍이 다시 열리기 때문이다.

공허하지 않음을 실측으로 증명했다.

| 조작 | `FailedCount` |
| --- | ---: |
| `current_manual_admin_package_pair`를 stale 값으로 되돌림 | `1` |
| `previous_04259_descriptor_id` 강등 기록 삭제 | `1` |
| 원상 복구 | `0` |

### 11.4 검증 (실측)

| 명령 | 결과 |
| --- | --- |
| `Invoke-Pester .../PcvAdminSmokeEvidenceDocs.Tests.ps1` | 통과 `90`, 실패 `0` |
| `Invoke-Pester .../PcvManualAdminDescriptorCurrency.Tests.ps1` | 통과 `6`, 실패 `0` |
| `Invoke-Pester packaging/windows-desktop-node/tests` | 통과 `485`, 실패 `0`, 건너뜀 `0` (기존 `479` + 신규 `6`) |
| `Update-PcvCurrentEvidenceDocs.ps1 -Check` | `8/8 current` |
| 백틱 인용 값 집합 비교 | 사라진 값 `0`개 |
| `git diff --check` | PASS |

`dotnet test`는 실행하지 않았다. 이 변경은 문서와 Pester 테스트만 건드리며 C# 소스는 한 줄도
바뀌지 않았다.

### 11.5 하지 않은 것

- 오늘 `main` push `2`건(`ef3f5f65`, `ba918e7a`)은 `Public Boundary Contract`를 통과했지만 전용
  evidence 문서가 없다. 없는 문서를 지어내지 않았고, descriptor에
  `current_public_boundary_main_push_evidence_gap` 필드로 이 공백을 명시했다.
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의 legacy 필드 `current_provenance_commit`과
  `current_full_gate_provenance_commit`은 `4855947f`(`0.42.65`)로, 생성 블록의
  `e9138988`과 어긋난다. 해당 절이 "legacy compatibility fields ... read-only until
  schema-specific consumers migrate"로 명시적으로 동결을 선언하고 있어 건드리지 않았다. 소비자
  마이그레이션이 선행돼야 하는 별도 항목이다.

---

## 12. `ServiceTokenRotationRevoke` 간헐 실패 조사 (같은 날, 별도 세션)

결론부터: **근본 원인을 확정하지 못했다.** 항목은 열린 채로 둔다. 다만 유력했던 가설 하나를
실측으로 **반증**했고, 다음 재현이 스스로 원인을 말하도록 계측을 넣었다.

### 12.1 재현 시도 — 82회 전부 실패

| 맥락 | 실행 | 재현 |
| --- | ---: | ---: |
| 해당 테스트 단독 (`--filter`) | `40` | `0` |
| `DesktopNode.Host.Tests` assembly 전체 | `12` | `0` |
| 전체 솔루션 (`7` assembly 병렬) | `10` | `0` |
| Task 9의 선행 격리 재실행 | `5` | `0` |

원본 관측은 단 한 번이다. Task 8 구현자가 full Host.Tests 실행에서 `189` 통과 / `1` 실패로
이 테스트 이름을 보고했다. **그때 실패 메시지를 아무도 기록하지 않았고, 그것이 지금까지 원인이
안 나온 이유다.**

이 저장소는 같은 함정을 이미 한 번 밟았다. 감사 §13의 정정은 `125/126`이라는 숫자가 일치한다는
정황만으로 간헐 실패를 엉뚱한 테스트에 귀속시켰던 일을 기록한다. 이번 건도 단일 관측이므로
테스트 귀속 자체가 확정적이지 않다는 점을 명시해 둔다.

### 12.2 배제한 것

| 후보 | 판정 근거 |
| --- | --- |
| 토큰 값 충돌 (`Assert.NotEqual`, `DoesNotContain`) | `CreateToken`은 `32` byte 난수다. 충돌 확률은 `2^-256`이며 간헐 실패 빈도와 자릿수가 맞지 않는다 |
| 공유 static 옵션 오염 | 테스트 클래스의 `static NativeActionOptions`는 공유되지만 `WithAction`/`WithDataRoot`가 매번 새 인스턴스를 반환한다. 변이 없음 |
| 제품 코드의 static 가변 상태 | `DesktopNode.Host`의 `static`은 전부 메서드다. 가변 필드 없음 |
| 프로세스 전역 상태 (CWD, 환경변수) | `Host.Tests`에 `SetCurrentDirectory`/`SetEnvironmentVariable` 호출 `0`건 |
| fake controller의 시간 의존 | `FakeWindowsServiceController`는 완전 동기다. 감사 §13이 확정한 CPU 기아 패턴(`SpinWait` + 짧은 마감)이 이 경로에는 없다 |
| backup 파일명 충돌 | 타임스탬프가 `100ns` 정밀도이고 테스트마다 `dataRoot`가 GUID로 분리된다 |
| `Directory.Delete(recursive)` finally 실패 | 같은 클래스 `83`개 테스트 중 `26`개가 동일 패턴을 쓴다. 이것이 원인이면 실패가 그 `26`개에 흩어져야 하는데 관측은 이 테스트 하나뿐이다. 약화됐을 뿐 완전 배제는 아니다 |

### 12.3 유력 가설을 실측으로 반증했다

Task 9 구현자와 이번 조사가 **독립적으로 같은 가설**에 도달했다. `%TEMP%`에 대한 일시적
sharing violation이 기존 `catch`(`IOException` 포함)에 삼켜져 `Ok=false`가 되고,
`Assert.True(result.Ok)`가 이유 없이 실패한다는 것이다.

`DesktopNodeServiceTokenOps.cs:131`의 `File.Replace`는 제품 전체에서 **유일한** `File.Replace`이고,
나머지 원자적 교체 `4`곳은 모두 `File.Move(overwrite: true)`를 쓴다. 이 불일치가
"`File.Move`로 통일하면 견고해진다"는 수정안으로 이어질 뻔했다.

목적지 파일에 읽기 핸들을 잡아둔 채 두 API를 각각 `60`회 호출해 실측했다.

| API | share 모드 | 실패 |
| --- | --- | ---: |
| `File.Replace` | 핸들 없음 (대조군) | `0` / 60 |
| `File.Move(overwrite)` | 핸들 없음 (대조군) | `0` / 60 |
| `File.Replace` | `Read` | `60` / 60 |
| `File.Move(overwrite)` | `Read` | `60` / 60 |
| `File.Replace` | `ReadWrite` | `60` / 60 |
| `File.Move(overwrite)` | `ReadWrite` | `60` / 60 |
| **`File.Replace`** | **`ReadWrite\|Delete`** | **`0` / 60** |
| `File.Move(overwrite)` | `ReadWrite\|Delete` | `60` / 60 |

`ReadWrite|Delete`는 Windows Defender를 비롯한 정상 동작 스캐너가 실제로 사용하는 share 모드다.
그 모드에서 **살아남는 것은 `File.Replace`뿐이다.** 즉 이 경로는 제품에서 가장 견고한 API를 이미
쓰고 있었고, "일관성을 위해 `File.Move`로 바꾼다"는 수정은 코드를 **더 잘 깨지게** 만들었을
것이다. 가설을 확인하지 않고 고쳤다면 정확히 반대 방향으로 갔다.

이 결과는 sharing-violation 가설 자체도 약화시킨다. 현실적인 스캐너 share 모드에서 이 API는
실패하지 않는다.

부수 관측: 취약한 쪽은 나머지 `4`곳(`DesktopNodeConfigMigrationOps`, `DesktopNodeJobStoreMigrationOps`,
`JsonFileDesktopNodeJobStore` `2`곳)이다. 이 조사 범위 밖이고 실제 결함으로 관측된 바 없어
기록만 남긴다.

### 12.4 넣은 것 — 진단 계측

`Assert.True(result.Ok)`는 결과가 이미 실어 나르는 구조화된 오류를 통째로 버리고 있었다.
rotation은 `IOException`/`CryptographicException`을 던지지 않고 `Ok=false`로 변환하므로, 원인이
결과 객체 안에 있는데도 실패 출력에는 아무것도 안 나온다. 이것이 단일 관측으로 원인을 못 잡은
직접적 이유다.

`error_code`, `error_message`, `service_token_mutation`, `atomic_replace_status`,
`backup_write_status`, `service_reload_status`, `old_token_sha256`, `new_token_sha256`을 실패
메시지에 싣도록 바꿨다. 다음 재현은 스스로 원인을 말한다.

### 12.5 하지 않은 것

- **제품 코드를 고치지 않았다.** 근본 원인이 확정되지 않았고, 유일하게 구체적이던 가설은 위에서
  반증됐다. 확정되지 않은 원인에 대한 추측성 수정은 하지 않는다.
- 항목을 닫지 않는다. 재현이 `82`회 나오지 않았다는 것은 부재의 증명이 아니다. 감사 §13도
  같은 이유로 "간헐 실패의 부재는 짧은 반복으로 증명되지 않는다"고 적었다.
- 테스트 귀속을 확정적으로 주장하지 않는다. 관측이 한 번뿐이다.

---

## 13. FC-12(b) guest 측 종결 — 원인은 인코딩이 아니었다 (같은 날, 별도 세션)

§8.8과 §9.5가 "별도 조사 대상"으로 남긴 항목이다. 닫혔다. Evidence는
`docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`가 소유한다.

### 13.1 두 가설 모두 틀렸다

FC 검증 evidence §3은 표본의 `stdout_byte_count=27`이 UTF-8(`33`)도 OEM 손실(`19`)도 아니라는
관측에서 "stream 인코딩" 대 "argv 전달" 두 가설을 세우고 분리하지 못했다고 기록했다. 실제
원인은 argv 전달이었고, **인코딩은 애초에 깨진 적이 없다.** `dcb703ad`의 UTF-8 고정은 옳았고
충분했다.

`DesktopNodeHyperVPowerShellDirectGuestExecutionProvider.cs`의 bridge는 argv 배열을 공백으로
이어붙여 guest에서 PowerShell로 재파싱했다.

```powershell
$script = [scriptblock]::Create(([string[]]$payload.command -join ' '))
```

표본 `café 한글 日本語 Ж Ω ß`는 뭉개진 게 아니라 **공백에서 6개로 갈라져 6줄로 출력**됐다.
기록된 모든 관측이 이것으로 설명된다. `hostname`이 정상이었던 건 단일 토큰이라 join이 원본과
같은 문자열을 만들기 때문이고, `[Console]::Out.Write('abc')`가 `0` bytes였던 건 메타문자 재파싱
때문이다.

### 13.2 인코딩 문제가 아니라 실행 문제였다

공백 분해보다 중요한 게 함께 드러났다. argv 원소에 든 PowerShell 메타문자가 guest에서
**실행된다.** `$(1+1)`은 `2`로 평가되고 `x; Write-Output INJECTED`는 두 문장으로 실행된다.

이 경로는 ADR-0009가 보안 bounded context로 고정한 곳이고, PCVCLI 계약은
`pcvcli vm guest-exec <vm> -- <command>`로 argv 전달을 이미 문서화하고 있었다. 구현이 문서화된
계약을 지키지 않은 것이며, 현재 동작을 잠그는 테스트는 없었다.

권한 상승은 아니다. 이 endpoint의 목적 자체가 인가된 호출자의 guest 명령 실행이다. 실질 영향은
운영자 인자의 조용한 재해석과, 자동화가 신뢰할 수 없는 데이터를 argv로 넘길 때의 호출자 측
위험이다.

### 13.3 수정과 실측

argv를 코드가 아닌 데이터로 넘긴다. 단일 원소일 때 `$argv[1..0]`이 내림차순 범위
`@($argv[1], $argv[0])`가 되는 함정은 길이 분기로 막았다.

실제 guest `pcv-guest-installed-04253-r1`에서 같은 세션에 수정 전/후를 나란히 실행했다.

| 케이스 | 수정 전 | 수정 후 |
| --- | --- | --- |
| `Write-Output` / `a b c` | `a` `b` `c` 3줄 | `a b c` |
| `Write-Output` / `$(1+1)` | `2` (평가됨) | `$(1+1)` |
| `Write-Output` / `x; Write-Output INJECTED` | `x` + `INJECTED` (실행됨) | `x; Write-Output INJECTED` |
| `Write-Output` / `café 한글 日本語 Ж Ω ß` | `36` bytes, 6줄 | **`31` bytes** = 기대 UTF-8 길이 |
| `hostname` | `15` bytes | `15` bytes (동일) |

로컬 테스트만으로는 PowerShell Direct 직렬화 경계를 넘지 못하므로 실제 guest 실행이 필요했다.
`-ArgumentList (, $argv)`가 그 경계를 넘어 배열로 유지되는 것까지 확인했다.

### 13.4 새 테스트

`src/DesktopNode.HyperV.Tests/GuestExecutionArgvFidelityTests.cs` `6`건. 기존
`GuestExecutionTransportEncodingTests`의 방식을 따라 shipped bridge를 그대로 실행하되 guest는
필요 없다. argv 충실도는 guest 접촉 전에 결정되기 때문이다.

공허하지 않음을 실측했다. 구현을 옛 join으로 되돌리면 `6`건 중 `5`건이 실패하고 복구하면
`6/6` 통과한다. 통과하는 `1`건은 단일 인자 케이스이며 양쪽 동작이 같은 것이 정상이다.

.NET 전체는 `842/0/0`이다(HyperV `131` → `137`).

### 13.5 guest 자산 — 권고를 실행했다

FC evidence가 권고로만 남겼던 `AutomaticCheckpointsEnabled` 비활성화를 이번에 실행했다(승인됨).
그 결과 이번 기동은 디스크 체인에 부작용을 남기지 않았다. checkpoint `0` → `0`이고 연결 디스크는
base `.vhd` 그대로다. 직전 세션이 기록한 "기동만으로 체인이 바뀐다"는 부작용이 해소됐다.

### 13.6 하지 않은 것

- 설치본을 바꾸지 않았다. `0.42.70-admin-smoke`에는 여전히 수정 전 코드가 있고 이 수정은 다음
  package 후보에 들어간다. package build와 anchor 승격은 실행하지 않았다.
- 수정 전 동작을 권한 상승 취약점으로 주장하지 않는다.
- ADR-0009 본문은 고치지 않았다. argv 충실도 조항이 없다는 것은 확인했으나, ADR 개정은 결정
  기록의 변경이므로 별도 판단이 필요하다.

## 14. 백엔드 API processor 도메인 분해 (같은 날, 별도 세션)

evidence: `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md`
계획서: `docs/superpowers/plans/2026-08-06-purecvisor-desktop-node-api-request-processor-decomposition.md`

§9가 남긴 대형 모듈 `2`종 중 백엔드 쪽이다. `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`가
`3,367` → `495`줄(`-85`%)이 됐고, callback-free 소유자 `13`개가 생겼다.

### 14.1 왜 이렇게 나눴나

새 패턴을 만들지 않았다. 이 파일은 이미 wave 1A~1D에서 소유자 `3`개를 떼어냈고,
`ApiArchitectureOwnershipTests`가 그 형태 — `sealed`, `Func`/`Action` 없음, processor 역참조 없음,
`HandleCore`가 유일한 dispatcher — 를 IL 수준에서 잠그고 있었다. 남은 도메인에 같은 형태를 반복했다.

순서는 공용 helper 먼저였다. 도메인 블록 전부가 `Json`/`Body`/`Failure`/`Read*`에 의존하므로,
helper가 나가기 전에는 어떤 도메인도 파일을 떠날 수 없다.

### 14.2 callback adapter `2`종을 걷어냈다

`DesktopNodeApiRuntimeCoreHandlers.cs`의 `DesktopNodeApiJobRuntimeHandler`와
`DesktopNodeApiConsoleHandler`는 `Func` `7`개를 받아 곧바로 processor의 private 메서드로 되돌려
보냈다. wave 1이 없앤 형태가 job과 console에는 그대로 남아 있었다. 파일째 삭제했다.

### 14.3 계획서의 가정 하나가 측정으로 반증됐다

계획서에 `JobStoreCommitError`가 "인스턴스 상태를 쓰지 않으므로 `static`으로 올린다"고 썼는데
**틀렸다.** 본문이 `jobRuntime.LoadBlock`을 두 번 읽는다. 파라미터를 추가해 호출부가 넘기도록
고쳤고, 계획서와 evidence 양쪽에 정정으로 남겼다. 오늘 세션에서 내 문서화된 주장이 측정에
반증된 세 번째 사례다(§10.4 `MEASURE` 전제, §13 인코딩 가설에 이은).

### 14.4 기존 guard 하나가 실패했고, 삭제하지 않았다

`RequestProcessorDelegatesAuthSessionBehaviorToCallbackFreeOwner`가 processor의
`ResolveActor` 호출을 단언하고 있었는데, 그 호출자가 route 소유자로 옮겨가면서 실패했다.
단언의 목적(actor 해석은 auth 소유자에 남는다)은 그대로이므로, 호출자를 processor로 고정하는
대신 실제 호출자를 확인하도록 갱신했다. **실패하는 guard를 지우는 것이 가장 쉬운 길이었고,
그 길을 택하지 않았다는 사실 자체를 여기 남긴다.**

### 14.5 분해가 대형 모듈을 이름만 바꿔 옮기지 않도록

새로 만든 `DesktopNodeApiJobReconciliationHandler.cs`(`856`줄)와
`DesktopNodeApiVmMutationRouteHandler.cs`(`770`줄)를 생성 시점에 `module-size-ratchet.json`에
등록했다. 등록하지 않으면 `3,367`줄 모듈을 무제한 `856`/`770`줄 모듈로 옮긴 것이 되고, 라쳇이
막으려던 것을 그대로 통과시킨다.

### 14.6 남은 것

- **wave 1 소유자 `3`종의 helper 사본 제거.** `DesktopNodeApiDiagnosticsHandler` /
  `DesktopNodeApiAuthSessionHandler` / `DesktopNodeApiOpsSummaryHandler`가 각자 `Json`/`Body`/
  `Failure`/`TryParseBody` 사본을 갖고 있다. 계획서 task `12`였으나 **하지 않았다.**

  > **2026-08-07 정정.** 여기 원래 "auth 쪽 `Body`는 시그니처가 달라 대조가 선행돼야 한다"고
  > 적었는데 **틀렸다.** 대조해 보니 시그니처는 같고 줄바꿈만 다르다. 차단 사유가 실재하지
  > 않았다. 대조표는 `docs/followup-work-plan-2026-08-07.md` §2에 있다.
- **프런트엔드 `web/src/served-app.ts`(`4,005`줄) 분해.** 별도 계획서가 필요하다.
- **processor `495`줄.** 계획서 목표 `450`줄에 미달했다. 원인과 남은 `495`줄의 구성은
  evidence §8.1에 있다.
