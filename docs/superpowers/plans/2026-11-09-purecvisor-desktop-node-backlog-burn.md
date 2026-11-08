# Backlog 18건 처리 (C7 운영 위생) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 완료 판정(`complete=true`, 2026-11-08) 뒤 남은 운영 위생 C7의 backlog `undecided` 18건을 사용자 결정대로 분류하고, 닫을 행은 닫고, 고칠 행은 Lane 1 PR로 고친다. 끝에 clean `main`에서 C7 `open_counts=0, undecided=0`을 확인한다.

**Architecture:**
- C7은 `status=open`이면서 `counts`이거나 `undecided`인 행이 0이어야 충족이다(`ProjectCompletionEvaluator.EvaluateC7`). `out-of-scope`는 criteria 영구 범위 밖 5개 id에만 쓸 수 있고, 18건 중 맞는 행이 없다.
- 그래서 결정 PR 하나(분류와 즉시 닫기) 뒤에, 구현 PR 둘을 병렬로 낸다. 각 PR은 merge 때 자기 행을 `closed_by`로 닫는다.
  - B1: 도구·시험, payload 없음
  - B2: Web·API, payload 변경, queue 행
- 호스트 확인 둘(BL-0034 재현, BL-0028 보존 VM)은 Lane 2다.

**Tech Stack:** C#/.NET 10, TypeScript Web Console, PowerShell 7 Pester, Node 시나리오 스크립트, `pcvverify completion`.

## 사용자 결정 (2026-11-09)

승인 원문: `"A 3건 닫기, C는 권장안대로, B 구현 시작"`(2026-11-08 backlog 18건 처리 방안 보고의 다음 승인 1번 권장안)

| 항목 | 범위 |
| --- | --- |
| A | 바로 닫는다. BL-0029(PR #108로 수정, train 0.42.98 pair clean-host PASS), BL-0012(10-09 뒤 `main` Development Gates 37회 연속 성공, 재현 안 됨), BL-0013(생성기 확장은 효율 개선이라 하지 않음) |
| C | 권장안대로 정한다. BL-0026: 테마 선택에서 라이트 제거. BL-0030: 이전 승격 버전 update preflight 필수 파일 fixture와 Delivery 시험, 호환 파일은 0.42.96 이하 설치본이 없을 때 제거. BL-0033: API 전용 거절 코드와 화면 안내. BL-0028: 다시 받은 부모 VHD의 해시가 원본과 같으면 `Set-VHD -ParentPath`로 다시 연결(Lane 2, 보존 VM 변경. 다르면 멈추고 묻는다) |
| B | 구현을 시작한다. B1 도구·시험(BL-0011, 0015, 0018, 0022, 0025, 0035, 0030 시험), B2 Web·API(BL-0024, 0026, 0027, 0031, 0032, 0033), B3 BL-0034 재현(Lane 2) 뒤 원인에 따라 수정 |
| 차선 | Lane 0/1/2. Lane 2는 ADR-0016 `pcv-it-probe-*` 일회용 VM 생성·삭제(BL-0034 재현)와 보존 VM `pcv-guest-installed-04253-r1`의 VHD 부모 재연결(BL-0028, 해시 같을 때만)뿐이다 |
| PR | push와 PR을 연다. merge는 PR마다 사용자의 명시 merge 문장으로 한다 |
| release train | 이 campaign에 없다. B2 merge 뒤 queue(PR #113, B2)로 train 출발은 따로 승인받는다 |

## Global Constraints

- 분류와 닫기는 사용자 결정이다. 각 행의 `basis`에 결정 날짜와 승인 문장을 적는다.
- 고치는 행은 원래 증상을 재현하는 회귀 시험을 먼저 쓴다. PR 본문에 BL id와 시험 이름을 적는다.
- 구현자: B2는 grok(Herdr, worktree `../pcv-public-wt-web-20261108`), B1은 Claude 하위 에이전트(격리 worktree)다. Claude(lead)가 결정 PR, Lane 2, 리뷰, PR, 기록을 맡는다.
- 보존 VM `pcv-it-s2-source`는 건드리지 않는다. `pcv-guest-installed-04253-r1`은 BL-0028 범위 안에서만 바꾼다.
- public 저장소다. 사용자 홈 경로, LAN 사설 IP, 호스트명, token을 적지 않는다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·12회(checkpoint마다). B2는 여러 checkpoint로 나뉜다.

## Task 1: 결정 기록

- [x] Lane 1. `docs/ga-ready/backlog.json` 18행에 분류와 `basis`를 적는다. A 3행(BL-0012, 0013, 0029)은 `status=closed`와 `closed_by`를 적는다. C 4행(BL-0026, 0028, 0030, 0033)은 결정이 났으니 `needs_design=false`로 둔다. campaign을 열고 plan을 더한다. 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `pcvverify completion`(로컬, C7 undecided 0 확인). push, PR, 사용자 merge 문장 뒤 merge.

실행 기록(2026-11-09): 18행 분류와 A 3행 닫기, Task 4·5 기록을 PR #114로 냈다. Required CI 다섯 개 pass 뒤 사용자 문장 `"PR #114, #115, #116 순서대로 merge 승인"`으로 merge했다(merge `b665628`).

## Task 2: B1 도구·시험 PR

- [x] Lane 1. 하위 에이전트 결과(branch `lane1/tooling-fixes-20261109`)를 리뷰한다. 행 7개(BL-0011, 0015, 0018, 0022, 0025, 0030, 0035)를 같은 PR에서 닫는다. 종료 검증: PR gate, Pester, public-source-safety 전부 green. push, PR, 사용자 merge 문장 뒤 merge.

실행 기록(2026-11-09): PR #115로 냈다. 행은 BL-0028을 더해 8개(BL-0011, 0015, 0018, 0022, 0025, 0028, 0030, 0035)를 닫았다. BL-0011이 product payload라 queue 행(`0575b859`, risk S)을 더했다. #114 merge 뒤 rebase해 행 닫기 commit `d79717f`를 얹었고, Delivery 785건 통과, Required CI 다섯 개 pass 뒤 merge했다(merge `0363083`).

## Task 3: B2 Web·API PR

- [ ] Lane 1. grok 결과(branch `lane1/web-console-fixes-20261108`)를 리뷰한다. 행 6개(BL-0024, 0026, 0027, 0031, 0032, 0033)를 같은 PR에서 닫고 release-train queue 행을 더한다. 종료 검증: PR gate, `npm run test:required`, `verify:parity`, Pester. push, PR, 사용자 merge 문장 뒤 merge.

## Task 4: BL-0034 재현 (Lane 2)

- [x] Lane 2. 설치본 `0.42.98` loopback에서 `pcv-it-probe-*` VM을 만들고 지우는 동안 VM 목록(`GET /api/v1/vms`)과 Web 목록을 반복해서 읽는다. `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE`가 나는지, 지운 행이 화면에 남는지 기록한다. 재현되면 원인 범위(API 일시 상태인지 화면 갱신인지)를 정해 수정 task를 더한다. 재현되지 않으면 관측 횟수를 근거로 사용자에게 닫기를 묻는다. 끝 상태 `pcv-it-probe-*` 0개.

실행 기록(2026-11-09 00:05~00:06 KST): 설치본 `0.42.98` loopback에서 PCVCLI(service bearer)로 `pcv-it-probe-list-1109`를 세 번 만들고 지웠다. 각 job이 도는 동안과 끝난 직후 `vm list`를 이어서 읽었다(job당 9회).
- 생성 3회: 목록 실패 0회.
- 삭제 3회: 매번 정확히 1회 `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE`. 삭제 시작 뒤 1,818 / 1,852 / 1,822 ms 시점이고, 그다음 읽기는 정상이다.
- 원인: `DesktopNodeHyperVNativeAdapter.Reads.cs` `TryInvokeVmList`가 지워지는 중인 VM 한 행의 요약 필드가 잠깐 비면 목록 전체를 실패로 돌려준다. 화면은 그 사이 옛 행을 붙잡고 있는다.
- 수정은 B2에 항목 7로 더했다: 요약이 불완전하면 짧게 한 번 다시 읽고, 화면은 삭제 job이 성공한 VM 행을 바로 지운다.
- 끝 상태: probe VM 0개. 빈 `artifacts/pcv-it-probe-list-1109` 폴더는 지웠다. 보존 VM 둘은 Off 그대로다.

## Task 5: BL-0028 보존 VM 부모 VHD 재연결 (Lane 2)

- [x] Lane 2. `pcv-guest-installed-04253-r1` disk의 부모 경로와, 다시 받은 `artifacts/image-cache` 평가판 VHD의 정체를 비교한다. `Get-VHD` 부모 식별자를 보고, 가능하면 해시도 비교한다. 같으면 VM이 꺼진 상태에서 `Set-VHD -ParentPath`로 다시 연결하고 `Get-VHD`로 확인한다. 시작은 하지 않는다(보존 VM). 다르면 바꾸지 않고 멈춰 묻는다. clean-host smoke 도구의 `DefaultSourceBaseVhdPath`가 archive 경로를 가리키는 문제는 별도 Lane 1 수정으로 B1에 묶거나 보고한다.

실행 기록(2026-11-09): VM은 Off, Generation 1이다.
- disk `pcv-guest-installed-04253-r1.vhd`는 VHD differencing이다. 부모는 read-only archive 저장소 `artifacts/image-cache/windows-server-2022-eval-vhd/20348.169…vhd`였고 그 파일은 없다.
- 같은 이름의 평가판 VHD(Dynamic VHD, 9.51 GB, 2026-11-08 Task 3a에서 다시 받음)가 public 저장소 `artifacts/image-cache`에 있다.
- 원본 파일이 없어 해시는 직접 비교할 수 없다. 그래서 `Set-VHD -ParentPath`가 `-IgnoreIdMismatch` 없이 자식에 기록된 부모 식별자를 검사하는 것을 동일성 확인으로 썼다.
- `Set-VHD -ParentPath <public artifacts 경로>`는 거절 없이 통과했다. `Test-VHD` 체인은 `True`이고, VM은 시작하지 않아 Off 그대로다.
- 이제 이 VM의 부모는 public 저장소 `artifacts/image-cache`의 그 파일이다(gitignore). 이 파일을 고치거나 지우면 VM이 다시 깨진다.
- clean-host smoke 도구의 archive 기본 경로 3개(`BaselineMsiPath`, `UpdatePackagePath`, `DefaultSourceBaseVhdPath`)는 B1 항목으로 넘겼다. 그래서 BL-0028은 B1 merge 때 닫는다.

## Task 6: 마무리

- [ ] Lane 1. merge된 PR과 닫힌 행을 기록한다. clean `main`에서 `pcvverify completion`을 돌려 C7을 확인한다. campaign을 닫는다. push, PR, 사용자 merge 문장 뒤 merge.

## Nonclaims

- operational current는 바꾸지 않는다(`0.42.98-admin-smoke`). B2 payload는 다음 release train이 싣는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
