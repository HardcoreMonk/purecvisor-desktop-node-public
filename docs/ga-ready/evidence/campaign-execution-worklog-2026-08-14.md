# 작업 내역: next admin-smoke package/fullgate/manual-admin (2026-08-14)

evidence_id: `campaign-execution-worklog-2026-08-14`
result: `PASS`
scope: `session-worklog-package-fullgate-functional-manual-admin`
work_date: `2026-08-14`
source_head: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
installed_version_after_work: `0.42.73-admin-smoke`
canonical_current_evidence: `0.42.73-admin-smoke`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 2026-08-14 세션에서 실행한 MSI/fullgate/host mutation과 후속 체인의
작업 내역이다. 개별 게이트의 계약 문서는 아래 연결 evidence가 소유한다.

## 승인과 범위

운영자 승인:

1. `MSI/fullgate/host mutation 허용`
2. Fullgate 실행 확인
3. `확인 처리`
4. `재실행 완료` (fullgate는 관리자 셸에서 실행)
5. `후속 진행`
6. `작업 내역 마크다운 문서 저장` (이 문서)

범위:

- Windows Desktop Node only
- CLI/Web-only, TUI 복원 없음
- `AllowUnsignedDev` / `LocalTest` internal admin-smoke
- public trusted signing / external stable publication 미주장
- 실행 세션에서는 `docs/ga-ready/current-evidence.json`을 승격하지 않았다.
- 후속 승격 변경이 canonical current를 `0.42.73-admin-smoke`로 올렸다.

payload 출처는 origin/main `b84441f0`이다. 포함 내용은 Web loopback session bootstrap,
login `409` `PCV_ACCOUNT_AUTH_NOT_CONFIGURED`, in-process Chromium Host gate, PR #189
diagnostics list다.

## 시간순 작업

### 1. Clean package

관리자 셸, ISO `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`, LAN
`http://[redacted-private-endpoint]:7777/`, 당시 설치본 `0.42.72-admin-smoke`를 확인한 뒤
`packaging/windows-desktop-node/installer/build.ps1`로 clean MSI를 만들었다.

| 항목 | 값 |
| --- | --- |
| version | `0.42.73-admin-smoke` |
| artifact | `artifacts/admin-smoke-package-20260814-04273` |
| build UTC | `2026-08-13T15:26:26.9302611Z` |
| provenance commit | `b84441f0750a9f77fd0588a86912dbdb68b94f0c` |
| clean MSI SHA-256 | `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee` |
| payload aggregate | `bbe2bfde532260eab7bd80de13e4e13350ae6553e4ef6a4037faa6e650359660` |
| Host SHA-256 | `a437a78b7198cb04d588e8b80688a522b3497fe5b8cdddc41d6f3483e197e9e2` |
| CLI SHA-256 | `b8a7374e843999d2979ba5181d18fb91909a375ef0482b840cb942c253b40bc2` |
| payload files | `8` |
| signing | `AllowUnsignedDev` / `LocalTest` |
| 이 단계 host mutation | `false` |

실측 MSI SHA와 sidecar/provenance가 일치했다.

### 2. Fullgate

이 세션의 자동 모드는 `-AllowHostMutation` Batch Supervisor 실행을 차단했다.
매니페스트와 elevated runner만 준비한 뒤, 운영자가 관리자 셸에서 fullgate를 재실행했다.

명령:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File artifacts\batch-runs\full-admin-host-mutation-gate-20260814-04273\run-fullgate-elevated.ps1
```

| 항목 | 값 |
| --- | --- |
| batch | `full-admin-host-mutation-gate-20260814-04273` |
| `service-msi-hyperv-admin-smoke` | PASS, exit `0`, attempt `1`, `84.873s` |
| `os-mutation-gate` | PASS, exit `0`, attempt `1`, `11.112s` |
| operational MSI SHA-256 | `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1` |
| operational payload aggregate | `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85` |
| boot time | `2026-08-13 06:57:43.5 +09:00` 유지 |
| 설치본 | `0.42.73-admin-smoke` / DisplayVersion `0.42.73` |
| service | `Running` / `Automatic` |
| managed VM | `pcv-spike-api-b2cafc24` Gen2 / `Default Switch` |
| delete | 첫 `action=delete`, 반복 `action=absent` |
| unmanaged | `pcv-spike-api-foreign-24af5066`, `PCV_VM_NOT_MANAGED_BY_PURECVISOR` |
| 잔여 `pcv-spike-*` | `0` |

### 3. Installed current-card (read-only)

`pcvcli --json host status|runtime policy|network inventory`와 Web `/`,
`/pcv-config.js`를 캡처했다. 첫 CLI 시도는 `--json` 위치가 아니라 `Start-Process`
인자 전달 문제로 exit `2`였고, `& pcvcli --json ...`로 재캡처해 `3/3`을 닫았다.

| 항목 | 값 |
| --- | --- |
| artifact | `artifacts/installed-operator-surface-current-card-20260814-04273` |
| summary SHA-256 | `44a91426579c6fb486e6b99cca2321ba4fd8cd547d16797017e0baa6c9d0da14` |
| CLI | `3/3` exit `0`, JSON ok |
| Web | `2/2` HTTP `200` |
| TUI | absent |
| host mutation | `false` |
| promotion | `not-promoted` — canonical current-evidence 미변경 |

### 4. Actual-VM functional

`Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`를 설치본 PCVCLI로 실행했다.

| 항목 | 값 |
| --- | --- |
| VM | `pcv-fc-cf-04273` |
| artifact | `artifacts/functional-correctness-carryforward-20260814-04273` |
| summary SHA-256 | `09a571235524b1a32c6066b7ef8c3c4ab4a425a7016ef4ccd1d284f75f9e6fac` |
| runner SHA-256 | `b0ac6cf563df637a9df42dfd8ab7f575bd7d8abc07329edcdcf3f84e90cf06ae` |
| steps | `10/10` exit `0` |
| QoS | `2048 Kbps -> 2,048,000 bps` |
| shrink | `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`, size `10,737,418,240` 유지 |
| expand | `11,811,160,064` bytes |
| cleanup | VM/folder 제거, 잔여 `pcv-fc-cf-*` `0` |

### 5. Manual-admin package pair

`0.42.72-admin-smoke -> 0.42.73-admin-smoke` pair를
`artifacts/manual-admin-campaign-20260814-04272-04273/run-campaign-elevated.ps1`로
실행했다. 04272 runner를 변환했고, baseline MSI hash 치환 순서를 한 번 고친 뒤
실행했다. 저장소 tracked dirty는 `docs/ga-ready/`만 허용했다.

| Bucket | 결과 |
| --- | --- |
| readiness | `ready-current-baseline-target-package-pair` |
| installed update/rollback | align `0.42.72` → update `0.42.73` → rollback `0.42.72` → final `0.42.73` |
| Burn | install/repair/remove + target restore PASS |
| MSIX | `0.42.72.0` install, `0.42.73.0` update, remove, final absent |
| ops summary | CLI JSON PASS |
| clean-host | child `exit=0`, `KB5120242`, UBR `169 -> 5499` |

Update ZIP SHA-256은 `1a7b17e2f1e2e3175f94c1ffce03b5d358a291f795ca34b3e0d4602e116d1b3c`다.
baseline 04272 ZIP SHA-256은 기존 pair와 같은
`f9dfa886dd5db2623ec63342538d775757b5f464e9eb9ca23a5206bcc1d65ba8`다.

clean-host child는 PASS였다. 부모 wrapper는 child `exit=0` 이후 StrictMode
`Count` residue readback에서 실패했다. `resume-descriptor-elevated.ps1`로 VM/VHD
absent를 확인하고 descriptor
`manual-admin-campaign-descriptor-20260814-04272-04273-closed`를
`runner_count=6`, `missing_count=0`, `not_pass_count=0`으로 닫았다.

clean-host VM 디렉터리에 빈 하위 폴더 1개가 남았다. VM과 differencing VHD는 없다.

## 연결 evidence

| 문서 | 역할 |
| --- | --- |
| `docs/ga-ready/evidence/admin-smoke-package-2026-08-14-04273.md` | clean package |
| `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-14-04273-hostmutation.md` | fullgate |
| `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md` | read-only current-card |
| `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-14-04273.md` | actual-VM functional |
| `docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md` | package-pair closure |

## 남긴 것 / 하지 않은 것

- token rotation R4는 재실행하지 않았다. token payload 변경이 없어 04272 R4를
  carry-forward한다.
- public-boundary post-merge evidence(`3b4fb787`, `b84441f0`)는 쓰지 않았다.
  최신 dedicated CI 문서는 PR #187이고, 이후 payload는 이 package chain으로 검증했다.
- public trusted signing과 external stable publication을 주장하지 않는다.

후속 승격 변경이 `current-evidence.json`과 `Pcv04273PromotionEvidence`를
`0.42.72-admin-smoke -> 0.42.73-admin-smoke` 튜플로 맞췄다.
