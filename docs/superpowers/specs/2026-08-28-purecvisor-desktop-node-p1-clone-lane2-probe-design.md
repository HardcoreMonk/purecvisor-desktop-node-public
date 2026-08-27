# PureCVisor Desktop Node P1-5 clone Lane 2 프로브 설계

- Design-ID: `purecvisor-desktop-node-p1-clone-lane2-probe-v1`
- 작성일: `2026-08-28`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: pcv-p1-clone-lane2-probe-20260828`
- 구현 계획: `docs/superpowers/plans/2026-08-28-purecvisor-desktop-node-p1-clone-lane2-probe.md`
- 선행: P1-5 managed full clone Lane 1 PASS, PR #8 merge `aee39b9`
- 선행 설계: `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone-design.md`
- 차선 절차: `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md`
- 이 문서가 수행하는 host mutation: `false`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 **clone family 한 설치본 프로브**만 연다. Full P0 연쇄, fullgate, manual-admin
pair, `current-evidence.json` write는 열지 않는다.

## 0. Lane 0 권위 (2026-08-28 실측)

```text
ledger_current=0.42.75-admin-smoke
installed_current=0.42.75-admin-smoke
source_head=aee39b9460463a34086582a84b0669d8e03875d3
working_authority=installed_current (Lane 2 규칙)
```

설치본 manifest는
`C:\Program Files\PureCVisor\DesktopNode\product-manifest.json`의
`0.42.75-admin-smoke`다. origin/main `aee39b9`는 `vm.clone.preview` /
`vm.clone`를 포함하지만 04275 MSI payload에는 없다. 세 값이 다른 것은 오류가 아니라
상태다.

Lane 2 작업 권위는 `installed_current`다. `-Version`은 설치본 manifest와 대소문자 포함
완전 일치해야 한다. 지금 04275 설치본에서 clone을 호출하면 제품 경로가 없어 프로브가
즉시 FAIL한다. 그 FAIL는 clone 구현 결함이 아니라 **payload 부재**다.

## 1. 문제

P1-5 clone은 Lane 1에서 코드/CI가 닫혔다. ADR-0015 `actual_vm_tested`와 차선 절차는
설치본 CLI가 같은 표시 이름으로 preview/clone/get/delete 하는 것을 요구한다.
04275 설치본은 그 경로를 갖고 있지 않다.

P1-5 설계 문장 “승인된 설치본 04275에서 clone family 한 프로브”는 **호스트와
operational current를 04275로 유지**하라는 뜻으로 읽는다. clone 바이너리가 04275
안에 있다는 뜻이 아니다. 04275 바이너리로 clone을 증명할 수는 없다.

## 2. 목표와 비목표

### 목표

- clone을 포함한 **probe-vehicle** `0.42.76-admin-smoke` MSI를 `aee39b9`(또는 그
  이후 clone-preserving HEAD)에서 빌드한다. `AllowUnsignedDev` / `LocalTest`.
- 그 MSI를 관리자 opt-in으로 이 호스트에 적용해 `installed_current`를
  `0.42.76-admin-smoke`로 만든다. `ledger_current`는 `0.42.75-admin-smoke`로 둔다.
- 전용 `ArtifactRoot` 하나, 전용 `VmRoot`(볼륨 루트 아래 세그먼트 2개 이상) 하나에서
  clone family만 실행한다.
- 소스 VM은 runner가 만든 managed Gen2 Off, checkpoint 0, 독립 VHDX 1개다. 사용자
  VM을 소스로 쓰지 않는다.
- preview mismatch는 파일 write 0으로 `PCV_VM_CLONE_CONFIRMATION_MISMATCH`다.
- clone job `succeeded` 후 대상은 표시 이름으로 get 200, `managed_by_purecvisor=true`,
  디스크는 `VmRoot/<target>/disk0.vhdx`, 소스는 Off·디스크 불변이다.
- cleanup은 대상 다음 소스 순서로 제품 delete다. `pcv-p1-clone-*` 잔여 0.
- canonical operator id는 표시 이름이다. GUID로 get/delete 하지 않는다.

### 비목표

- 04275 설치본 CLI로 clone 호출
- source-built `DesktopNode.Host.exe`를 설치본 대신 listen
- Full P0 (save/attach/restore/manage) 자동 연쇄
- full admin host mutation gate
- `0.42.75 -> 0.42.76` manual-admin pair closure
- `docs/ga-ready/current-evidence.json` write, generated current 블록
- feature ledger `pcv.vm.clone`를 pass로 승격 (Lane 3)
- linked clone, TPM 소스, checkpoint 소스, Gen1, unmanaged 소스
- 사용자 기존 VM 변경, 호스트 재부팅
- public trusted signing, 외부 publication

## 3. 선택한 접근

세 후보:

1. **04275 설치본에서 clone 호출** — 기각. 경로 없음. payload 부재를 clone FAIL로
   오인한다.
2. **04275 서비스를 멈추고 source HEAD Host.exe listen** — 기각. Lane 2 권위는
   `installed_current`다. 이 경로는 설치본 evidence가 아니다.
3. **0.42.76 probe-vehicle 패키지 빌드 → 설치 → clone family 프로브** — 채택.
   04275 승격 때와 같다. 그때는 ledger가 04274인 채 04275 MSI를 설치하고 Lane 2를
   돌렸으며 current write는 나중 Lane 3였다.

이 문서의 “0.42.76 package”는 **operational current 승격이 아니다.**
`canonical_current_changed=false`인 admin-smoke 패키지 증거와 설치본 버전 이동만
허용한다. fullgate/pair/current는 clone `overall_verdict=PASS`와
`cleanup.verdict=PASS` 뒤 **별도 Lane 3 승인**에서만 연다.

## 4. checkpoint 순서

이 설계 승인 뒤 구현 계획이 연다. 실행은 한 checkpoint에 한 칸이다.

| 순서 | 차선 | 하는 일 | mutation |
| --- | --- | --- | --- |
| A | Lane 1 | clone Lane 2 runner + DryRun 계약. 설치본/Hyper-V 호출 없음 | false |
| B | Lane 3 문서가 아님. **probe-vehicle 패키지** | `0.42.76-admin-smoke` clean MSI 빌드, package evidence. current 금지 | false |
| C | 설치 opt-in | 04275 → 04276 MSI apply. installed_current만 이동 | true, 관리자 PowerShell |
| D | Lane 2 | `-Version 0.42.76-admin-smoke` clone family 한 프로브 | true, 관리자 PowerShell |

B는 current를 04276으로 올리지 않는다. C 이후 Lane 0는 다음을 보고한다.

```text
ledger_current=0.42.75-admin-smoke
installed_current=0.42.76-admin-smoke
source_head=<clone-preserving sha>
working_authority=installed_current
```

D의 `-Version`은 `0.42.76-admin-smoke`와 완전 일치해야 한다. DryRun PASS는 Lane 2
PASS가 아니다.

## 5. 프로브 계약

### 5.1 이름과 경로

- 소스: `pcv-p1-clone-04276-<8hex>-src`
- 대상: `pcv-p1-clone-04276-<8hex>-dst`
- 접두사 `pcv-p1-clone-`, version tag `04276`, 전체 길이 가드는 P0와 같이 6~60
- `VmRoot` 예: `D:\data\pcv-p1-clone-04276` (세그먼트 ≥ 2)
- `ArtifactRoot` 예: `artifacts/service-plan-p1-clone-actual-vm-YYYYMMDD-04276`
- 같은 artifact root, 같은 VM 이름을 재실행에 재사용하지 않는다

### 5.2 slice

한 family, 고정 순서, fail-stop.

| slice | 동작 | PASS |
| --- | --- | --- |
| `source_create` | 제품 create, Gen2, 작은 독립 VHDX, Off 확인, manage marker | job succeeded, get 표시 이름 200, Hyper-V Off |
| `preview_mismatch` | `pcvcli vm clone <src> --name <dst>` confirm 없이 또는 잘못된 `--yes` 대상. 구현은 `confirm_name` Ordinal 불일치 | `PCV_VM_CLONE_CONFIRMATION_MISMATCH` 또는 `PCV_CLI_CONFIRMATION_REQUIRED`. 대상 디렉터리/VM 없음 |
| `preview_ok` | `pcvcli vm clone <src> --name <dst> --dry-run` | preview 200, `planned_copy_bytes` = 소스 VHDX 파일 길이, 파일 write 0 |
| `clone_ok` | `pcvcli vm clone <src> --name <dst> --yes`, job 대기 | job `vm.clone` succeeded, 대상 get 200, managed true, `disk0.vhdx`가 대상 디렉터리, 소스 VHDX 길이/경로 불변, 소스 Off |
| `cleanup` | 대상 delete 다음 소스 delete | 둘 다 absent, `pcv-p1-clone-*` 0, VmRoot 비움 |

`preview_mismatch`는 write 0을 증명하기 위해 `clone_ok`보다 앞선다.

### 5.3 runner

새 파일:

- `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1`
- `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP1CloneActualVmSmoke.Tests.ps1`

P0 runner를 clone family로 축소한다. `-Mode Full`/`SavedOnly`를 넣지 않는다. 모드 하나,
clone family다.

필수 파라미터: `-Version`, `-ArtifactRoot`, `-VmRoot`. 관리자 PowerShell 7.
`RuntimeAdapter`로 DryRun/단위 테스트를 주입한다.

`summary.json` 필수 키: `ok`, `overall_verdict`, slice verdict, queued job status,
Hyper-V/product readback, `error`(내부 첫 `PCV_*`), `cleanup.verdict`,
`host_mutation_performed`, `secret_observed`, `installed_manifest_version`,
`installed_cli_sha256`.

식별자: create에 쓴 표시 이름으로만 get/delete/clone. GUID 금지.

### 5.4 설치 전제

D를 열기 전:

- 설치본 `product-manifest.json` version = `0.42.76-admin-smoke`
- 설치본 `pcvcli.exe`가 `vm clone` 도움말을 갖는다
- 서비스 Running, loopback Web `200`, API unauthenticated `401`/`PCV_AUTH_REQUIRED`
- 전용 VmRoot 비어 있음

이 전제가 없으면 D를 시작하지 않고 Lane 0로 보고한다.

## 6. 패키지 차량 (checkpoint B)

- version: `0.42.76-admin-smoke`
- provenance: clone이 들어간 main HEAD (`aee39b9` 또는 후속 ratchet/docs-only가 아니면
  그 HEAD)
- signing: `AllowUnsignedDev`
- evidence: `docs/ga-ready/evidence/admin-smoke-package-YYYY-MM-DD-04276.md`
- `host_mutation_performed=false`, `package_installed=false`
- `canonical_current_evidence=0.42.75-admin-smoke`, `canonical_current_changed=false`

B는 MSI/hash/provenance만 남긴다. C의 msiexec apply는 별도 승인 문장과 관리자
PowerShell 명령으로만 실행한다. controller 프로세스가 UAC를 자동화하지 않는다.

## 7. PASS / FAIL

Lane 2 PASS 입력:

- `overall_verdict=PASS`
- `cleanup.verdict=PASS`
- `secret_observed=false`
- `-Version` == 설치본 manifest

FAIL summary는 `actual_vm_tested=pass` 입력이 될 수 없다. 부분 성공(preview PASS,
clone FAIL)은 slice를 합치지 않고 다음 Lane 1이 그 코드만 고친다. 동일 원인 3회면
회로를 연다.

## 8. Lane 3 경계

이 설계가 끝나도 operational current는 `0.42.75-admin-smoke`다. 다음을 자동으로
하지 않는다.

- fullgate
- `0.42.75-admin-smoke -> 0.42.76-admin-smoke` pair
- current-evidence / feature ledger pass
- AGENTS.md generated current 블록

clone family PASS는 Lane 3를 **검토할 자격**만 만든다. 승격은 새 승인이다.

## 9. 비주장

- 이 문서 자체는 Hyper-V/MSI/service mutation을 실행하지 않는다
- 04275를 clone-capable로 재해석하지 않는다
- public trusted signing / external stable publication `not-claimed`
