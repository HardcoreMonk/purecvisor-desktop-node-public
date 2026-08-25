# PureCVisor Desktop Node TUI 제거 및 CLI/Web 전용 제품 경계 설계

- 날짜: 2026-07-14
- 상태: 사용자 승인
- 결정: active 제품 경계에서 TUI를 완전히 제거하고 CLI와 Web Console만 유지한다.
- 다음 product payload 후보: `0.42.63-admin-smoke`

## 1. 목적

PureCVisor Desktop Node의 사용자 운영 표면을 Web Console과 PCVCLI로 단순화한다.
`DesktopNode.Tui`는 제품, 설치 패키지, active source, current 문서와 검증 계약에서
제거한다. 기존 TUI 설계, 계획, 설치본 smoke와 GA-ready evidence는 당시 사실을 증명하는
역사 기록이므로 삭제하거나 소급 수정하지 않는다.

이 변경은 UI 하나를 숨기는 변경이 아니다. 솔루션, build, product manifest, MSI payload,
설치본 검증, 운영 문서에서 TUI가 더 이상 제품 기능으로 존재하지 않게 만드는 제품 경계
변경이다.

## 2. 결정 배경

현재 제품에는 다음 세 operator surface가 있다.

- Web Console
- `pcvcli.exe`
- `pcvtui.exe`

TUI는 Local API의 별도 .NET client이며 backend 기능을 소유하지 않는다. VM, network, job,
diagnostics, QoS와 guest execution 기능은 Local API에 있고 Web Console과 PCVCLI가 이를
소비한다. 따라서 TUI를 제거해도 Local API, queued job runtime, Hyper-V native adapter,
host operation 경계는 유지할 수 있다.

운영 표면을 두 개로 줄이면 일반 운영자는 Web Console, 자동화와 고급 운영자는 PCVCLI라는
명확한 역할 분리가 생긴다. TUI 전용 렌더링, 키보드 상태, polling, token resolution,
설치본 smoke와 packaging 계약도 함께 제거할 수 있다.

## 3. 목표와 비목표

### 목표

1. `DesktopNode.Tui` production/test project를 active source와 solution에서 제거한다.
2. `pcvtui.exe`를 build, product manifest, MSI와 설치 경로에서 제거한다.
3. 이전 MSI에서 새 MSI로 upgrade하면 기존 `pcvtui.exe`가 남지 않게 한다.
4. current 사용자·개발·운영 문서를 CLI/Web 전용 제품 경계로 정렬한다.
5. 설치본 current-card와 package/full-gate 검증을 CLI/Web 기준으로 재정의한다.
6. TUI가 사용하던 backend 기능은 Local API, Web Console과 PCVCLI에서 유지한다.
7. historical evidence의 무결성과 당시 PASS 판정을 보존한다.

### 비목표

- Local API route 삭제 또는 동작 변경
- Hyper-V provider, queued job runtime, auth/RBAC/JWT 변경
- Web Console 재설계
- PCVCLI command shape 변경
- 기존 역사 evidence 파일의 삭제 또는 본문 재작성
- 호환용 `pcvtui` shim, redirect executable 또는 경고용 placeholder 제공
- public distribution 경계 변경

## 4. 목표 아키텍처

```text
Web Console ─┐
             ├─> DesktopNode.Host / Local API ─> Jobs ─> Hyper-V / Host Ops
PCVCLI ──────┘
```

Web Console은 일반 운영자의 시각적 표면이고 PCVCLI는 terminal, script, JSON 기반 자동화
표면이다. 두 client는 같은 Local API contract와 auth boundary를 사용한다. TUI 제거 후에도
backend route catalog와 mutation guard는 바뀌지 않는다.

## 5. Source와 solution 변경

다음 active project를 삭제한다.

- `src/DesktopNode.Tui/`
- `src/DesktopNode.Tui.Tests/`

`src/DesktopNode.sln`에서는 `DesktopNode.Tui`와 `DesktopNode.Tui.Tests` project entry와
configuration mapping을 제거한다. 다른 project가 TUI assembly를 참조하지 않는지 solution
build와 repository search로 확인한다.

TUI source를 `archive/`로 이동하지 않는다. Git history가 이미 구현 이력을 보존하므로 active
tree에 별도 복제본을 남길 이유가 없다.

## 6. Product manifest와 runtime payload

`PcvDesktopNodeProduct.psm1`에서 다음 계약을 제거한다.

- `tui_exe_name`
- resolved path의 `tui_exe`
- manifest의 `tui` block
- required runtime payload의 `pcvtui.exe`

Manifest `schema_version`은 `1`에서 `2`로 올린다. Schema 2의 supported operator surface는
`web`과 `cli`다. Update/rollback code가 schema 1 manifest를 이전 설치본 입력으로 읽는 경우에는
기존 TUI field가 있어도 허용하지만, 새 schema 2 manifest를 생성할 때는 TUI field를 쓰지
않는다.

새 payload의 필수 executable은 다음과 같다.

- `DesktopNode.Host.exe`
- `pcvcli.exe`

Web runtime asset과 product wrapper 파일은 기존 계약을 유지한다.

## 7. Installer와 upgrade 동작

`packaging/windows-desktop-node/installer/build.ps1`에서 다음을 제거한다.

- `DesktopNodeTuiPath` parameter
- TUI project/publish directory
- TUI publish와 hash/provenance 단계
- `PCV_INSTALLER_TUI_*` build error
- payload copy의 `pcvtui.exe`

`Product.wxs`에서는 `DesktopNodeTuiComponent`를 제거한다. 새 package는 TUI 파일을 포함하지
않는다.

WiX `MajorUpgrade`가 이전 제품을 제거한 뒤 새 제품을 설치하는 현재 lifecycle을 기준으로
`0.42.62-admin-smoke -> 0.42.63-admin-smoke` upgrade 후 설치 경로에 `pcvtui.exe`가 없음을
검증한다. 이전 component의 잔존 가능성이 발견되면 새 MSI에 TUI binary를 다시 싣지 않고
upgrade cleanup용 `RemoveFile`만 추가한다.

Rollback으로 0.42.62 설치 상태로 돌아가면 이전 payload의 `pcvtui.exe`가 복원되는 것은
정상이다. 0.42.63이 최종 설치 상태일 때는 TUI가 없어야 한다.

기본 uninstall과 `REMOVE_DATA=1` 의미, machine PATH의 product root 등록, CLI 실행 계약은
유지한다.

## 8. Smoke와 검증 소유권 변경

다음을 제거한다.

- `Invoke-PcvInstalledTuiOperatorSmoke.ps1`
- TUI 전용 Pester tests
- package/current-card의 `pcvtui --smoke-once` 단계
- TUI renderer와 `PCV_TUI_*` active contract assertions

noVNC와 TUI를 함께 검증하던 test는 noVNC 전용 test로 이름과 범위를 바꾼다. noVNC bridge,
account login과 console handoff는 Web Console/API 기능이므로 유지한다.

Installed operator surface current-card는 다음을 확인한다.

- Web Console HTML, runtime config와 핵심 API-backed surface
- PCVCLI host/runtime/network/VM/job/diagnostics command
- service `Running/Automatic`
- secret non-observation
- 설치 경로의 `pcvtui.exe` 부재

## 9. 사용자 호환성과 오류 처리

새 버전에서는 `pcvtui` command가 존재하지 않는다. 호환 shim을 제공하지 않으며 사용자는
다음 경로로 전환한다.

- 대화형 일반 운영: `http://127.0.0.1/`
- terminal/자동화 운영: `pcvcli`

Release note와 current user guide에는 TUI 제거, 대체 경로와 upgrade 후 binary 제거를
명시한다. API error code나 backend route를 TUI 제거용으로 새로 만들지 않는다.

## 10. 문서와 evidence 분류

Active current 문서에서는 제품 표면을 Web/CLI로만 설명한다.

- `README.md`
- `AGENTS.md`
- `docs/USER_GUIDE.md`
- `docs/USER_FEATURE_USAGE_SPEC.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/OPERATIONS_GUIDE.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/ADR_INDEX.md`
- `docs/ga-ready/*INDEX*.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`

Historical design, plan과 evidence는 그대로 보존한다. Current index에서 이를 가리켜야 할 때는
`historical TUI predecessor`로 명시하며 active 기능처럼 표현하지 않는다. Evidence guard는
역사 문서에 기록된 `pcvtui` 문자열 자체를 금지하지 않고, active product/source/package
계약에 새 TUI reference가 생기는 것을 금지한다.

## 11. 검증 전략

### 비파괴 repository gate

1. `dotnet test src/DesktopNode.sln -c Release`
2. Web TypeScript/static parity/browser fixture
3. packaging, installer, Web Pester suite
4. installer dry-run과 payload plan test
5. `git diff --check`
6. active-boundary search:
   - TUI project가 solution에 없음
   - build와 manifest에 `pcvtui.exe`가 없음
   - MSI source에 TUI component가 없음
   - current user docs가 Web/CLI만 선언

Historical evidence와 archived plan/spec은 search exclusion이 아니라 허용된 historical
reference로 분류한다.

### 설치본 gate

Product payload 변경이므로 `0.42.63-admin-smoke`에서 다음을 실행한다.

1. clean package build
2. clean install/repair/uninstall payload 검사
3. 0.42.62 -> 0.42.63 update와 rollback campaign
4. update 후 `pcvtui.exe` 부재 확인
5. installed Web/CLI current-card
6. full admin host mutation gate
7. manual-admin package-pair closure

실제 host mutation과 package campaign은 별도 실행 단계에서 현재 관리자 승인·안전 gate를
따른다.

## 12. 완료 조건

다음을 모두 만족해야 TUI 제거를 완료로 판정한다.

- TUI production/test source와 solution entry가 없다.
- 새 build와 MSI에 `pcvtui.exe`가 없다.
- schema 2 product manifest가 Web/CLI surface만 선언한다.
- 0.42.62 -> 0.42.63 update 후 설치 경로에 TUI file이 없다.
- Web Console과 PCVCLI의 기존 backend 기능 회귀가 없다.
- noVNC, account, QoS, guest execution 검증이 TUI 없이 통과한다.
- repository test와 installed gate가 모두 PASS다.
- current 문서와 evidence ledger가 CLI/Web-only 경계를 일관되게 표시한다.
- historical TUI evidence는 변조되지 않고 predecessor로 보존된다.

## 13. 위험과 대응

| 위험 | 대응 |
| --- | --- |
| Major upgrade 후 TUI file 잔존 | installed package-pair에서 명시 검사하고 필요 시 `RemoveFile` cleanup 추가 |
| Manifest schema 1 consumer 파손 | schema 1 read compatibility 유지, schema 2 generation만 TUI 제거 |
| TUI와 결합된 noVNC test 손실 | noVNC 전용 test로 분리하고 Web/API handoff를 계속 검증 |
| 역사 evidence guard 실패 | historical reference와 active product reference를 분리한 guard로 변경 |
| Web/CLI에 없는 TUI 전용 workflow | backend route 기준으로 기능 대조 후 Web 또는 CLI existing path를 검증 |
| 문서에 TUI current 표현 잔존 | current-entry 문서 목록을 고정하고 boundary search test 추가 |

## 14. 승인된 최종 결정

PureCVisor Desktop Node의 active operator surface는 Web Console과 PCVCLI 두 개다. TUI는
deprecated 상태로 남기지 않고 제품에서 완전히 제거한다. 과거 TUI evidence는 historical
predecessor로만 보존한다.
