# S1 설치본 확인과 시연 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `main` `6b76274`(S1 브라우저 콘솔 포함) build를 이 호스트에 dev probe로 설치해 S1 시나리오 스크립트를 통과시키고, Ubuntu 26.04.1 live server ISO로 브라우저에서 VM 생성 → 화면·키 입력으로 OS 설치 → 네트워크 확인을 시연한 뒤 0.42.93으로 되돌린다. web public source safety 기존 실패(BL-0010)도 고친다.

**Architecture:** 근거는 ADR-0017(시나리오 S1), 설계 `pcv-s1-browser-console-v1`, `docs/DEVELOPMENT_PROCEDURE.md` §10 dev probe(제품 Update와 Rollback, 승격 근거 아님)다. 설치·되돌리기는 `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update|Rollback`, package는 `build.ps1`과 `New-PcvAdminSmokeUpdatePackage.ps1`, 시연은 Web Console(브라우저 자동화)과 `npm run scenario:s1-console`이다. branch는 `lane1/s1-installed-20261008` 하나, PR 하나다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvDesktopNodeProduct.ps1`, Web Console + 브라우저 자동화, `npm run scenario:s1-console`, `pcvverify completion`, `gh`

## 사용자 결정 (2026-10-08)

승인 원문: `1, D:\Downloads\ubuntu-26.04.1-live-server-amd64.iso, 3` (campaign `adr17-s1-console-20261008` 최종 보고 `next_approval_required` 1~3과 승인 2의 ISO 경로). 남은 C5 task는 직전 결정들과 같이 이관한다.

| 항목 | 범위 |
| --- | --- |
| 1 | "S1 설치본 smoke: PR B merge 뒤 main build로 이 호스트에서 dev probe를 한다(제품 Update로 새 build 설치, service 재시작, `npm run scenario:s1-console -- --execute`를 pcv-it- 접두사 VM과 smoke ISO로 실행, 끝에 제품 Rollback으로 0.42.93 복귀). Lane 2 host mutation은 제품 Update/Rollback, service 재시작, pcv-it- VM 생성·시작·키 입력·중지·삭제만. 결과 기록 push, PR, green CI 뒤 merge." |
| 2 | "S1 시연 기록: 실제 OS 설치 ISO(사용자가 경로 제공)로 브라우저에서 VM 생성 → 화면·키 입력으로 OS 설치 → 네트워크 확인을 한 번 하고, 짧은 시연 기록 문서와 criteria S1 `status=passed`, `demo_record`를 쓴다. Lane 2(pcv-it- 접두사 VM, guest OS 설치 포함), push, PR, green CI 뒤 merge." ISO `D:\Downloads\ubuntu-26.04.1-live-server-amd64.iso` |
| 3 | "backlog BL-0010(web public source safety 기존 실패 4곳) 분류: counts면 Lane 1로 고치고 push, PR, green CI 뒤 merge." 승인으로 `counts` 분류 |
| 이관 | `adr17-s1-console-20261008` Task 14(C5 runner, `not_before` 2026-10-19)를 Task 11로 옮긴다. Lane 0/1, push, PR, green CI 뒤 merge |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·삭제로 한정 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`을 바꾸지 않는다. 새 VM은 `pcv-it-` 접두사뿐이고 Task 9에서 모두 지운다.
- 설치본은 Task 3에서 dev build로 바뀌고 Task 9 Rollback으로 `0.42.93-admin-smoke`에 돌아간다. MSI 설치·제거, REMOVE_DATA, 방화벽, LAN, Event Log 변경은 범위 밖이다. Rollback이 실패하면 멈추고 보고한다.
- guest 계정 비밀번호는 실행 중에 만들어 키 입력으로만 보내고 문서·summary·명령줄에 남기지 않는다. 입력 audit은 길이만 남긴다.
- dev probe는 승격 근거가 아니다. `current-evidence.json`, operational current, release train은 바꾸지 않는다.
- 산출물은 `artifacts/s1-installed-20261008/` 아래에 둔다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 1: BL-0010 수정

- [x] web public source safety가 기존 파일 4곳(train-04290 plan 68행, noVNC 시험 3곳)에서 내는 실패를 고치고 backlog `BL-0010`을 `counts`(근거 승인 3)로 분류한 뒤 닫는다. 검증 `npm run test:public-source-safety --prefix web`, `npm run test:required --prefix web`, 관련 .NET 시험, Delivery. 로컬 commit.

실행 기록(2026-10-08): train-04290 plan 68행의 실제 LAN 주소를 가렸고(주소는 실행 값으로만 쓴다는 문장으로 바꿈), noVNC 시험 3개 파일의 합성 사설 IP 줄 7곳에 `public-safety: synthetic-rfc1918` 표시를 더했다(검사는 첫 줄만 보고하지만 파일의 모든 IP 줄을 본다). backlog `BL-0010`을 `counts`(승인 3)로 분류하고 닫았다. 시험: `npm run test:public-source-safety` 20/0(exit 0), web required exit 0, Api 517, Contracts 200, Delivery 775. host mutation 없음.

## Task 2: dev package

- [x] `main` `6b76274` 기준 worktree나 clean HEAD에서 `build.ps1`(version `0.42.94-admin-smoke`, `AllowUnsignedDev`, `LocalTest`)과 `New-PcvAdminSmokeUpdatePackage.ps1`로 package와 update catalog를 `artifacts/s1-installed-20261008/package`에 만든다. host mutation 없음. 실행 기록과 로컬 commit.

실행 기록(2026-10-08 23:29 KST): clean HEAD `235ecf7`에서 build `45`초(`build_utc` `2026-10-08T14:29:20Z`). 제품 코드는 `main` `6b76274`와 같다(branch 차이는 시험 주석 3개 파일과 문서뿐). MSI `PureCVisorDesktopNode-0.42.94-admin-smoke-windows-x64.msi` SHA-256 `ea9d6c432b2cca3320ec12a14b0382b1e598a5b301c9f087429739d693a3bbae`, payload `8`개 aggregate `3a48514de76257980e769fa6c1706bedb58f32cef90ce5fbe929434325b3bf85`, update ZIP SHA-256 `8013672f45f64cfb7a7063941a4290cc3a4b754c8293b4edbe9562aa7a618c73`, catalog `PureCVisorDesktopNode-0.42.94-admin-smoke-update-catalog.json`. 모두 `artifacts/s1-installed-20261008/package`에 있고 승격 근거가 아니다. host mutation 없음.

## Task 3: dev build 설치 (Lane 2)

- [x] `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`(가능하면 먼저 계획·WhatIf)로 dev build를 설치하고 service가 Running, 설치본 version, Web `200`, `GET .../console/frame/640x480`이 꺼진 VM에 `409`를 주는지 확인한다. 로컬 commit.

실행 기록(2026-10-08 23:33 KST): repo wrapper `-Action Update -UpdateCatalogUri <dev catalog> -UpdateChannel admin-smoke -DryRun`이 `ok=true`, 실행은 exit `0`(`3`초). 끝 상태: 설치본 manifest `0.42.94-admin-smoke`, service Running, Web `200`, `GET /api/v1/vms/pcv-guest-installed-04253-r1/console/frame/640x480`이 `409 PCV_CONSOLE_VM_NOT_RUNNING`(새 route 동작, 보존 VM은 읽기만), `pcv-it-` VM `0`개, 보존 VM Off. 결과 JSON은 `artifacts/s1-installed-20261008/devprobe/`에 있다.

## Task 4: S1 시나리오 스크립트 (Lane 2)

- [x] `npm run scenario:s1-console -- --execute --iso=<smoke ISO>`를 `pcv-it-s1-` VM으로 돌려 `summary.json` `result=pass`와 화면 캡처 2장을 얻는다. 끝에 VM이 지워졌는지 확인한다. 로컬 commit.

실행 기록(2026-10-08 23:30~23:31 UTC+9 기준 14:30:40~14:31:53Z): 설치본 `0.42.94-admin-smoke`에서 `run-s1-console-scenario.mjs --execute`를 VM `pcv-it-s1-smoke-20261008`과 smoke ISO로 돌려 `result=pass`(`73`초). session `200`, create·start·poweroff·delete job 모두 `succeeded`, 화면 캡처 2장(BMP), Enter 입력 `200`. 바뀐 화면은 37번째 읽기(약 60초 뒤)에 잡혀 Enter 효과가 아니라 firmware PXE 진행일 수 있다(키 입력 효과는 Task 10 integration과 Task 5~8 시연이 본다). 결과 `artifacts/s1-installed-20261008/scenario-smoke/`. 끝 상태 `pcv-it-` VM `0`개.

## Task 5: Ubuntu 시연 1 — 생성과 부팅 (Lane 2)

- [x] 브라우저(Web Console)에서 Ubuntu ISO로 `pcv-it-s1-ubuntu` VM(vCPU 2, 4096MB, 32GB)을 만들고 켠 뒤 VM Screen으로 설치 프로그램 첫 화면까지 간다. Gen 2 Secure Boot로 부팅이 막히면 Gen 1로 다시 만든다. 화면 캡처를 남긴다. 로컬 commit.

실행 기록(2026-10-09 15:02 KST): checkpoint 시작 시 `pcv-it-s1-ubuntu`가 이미 Running이었다. 실측은 Generation 2, vCPU 2, memory 4096MB, disk 32GB, DVD `D:\Downloads\ubuntu-26.04.1-live-server-amd64.iso`, Secure Boot On / Microsoft UEFI Certificate Authority, Default Switch, vm root `artifacts/s1-installed-20261008/vms/pcv-it-s1-ubuntu`, uptime 약 13시간 24분. Gen 2 Secure Boot는 부팅을 막지 않아 Gen 1 재생성은 하지 않았다. 콘솔 프레임 `1024x768`(`captured_at` `2026-10-09T05:59:36Z`, sha256 `fbd1ac0c6c08cb1a42e16aa25c21754496b582e3e77b1c41afb9edc4232881cf`)은 Subiquity 언어 선택이고 English가 선택되어 있다. Web Console(`http://127.0.0.1/`) VM Screen은 `streaming 640x480 at 2 fps`로 같은 첫 화면을 보여 준다(png sha256 `802852bd1eecb0b08384435da5800b36cfa920c2d60f3e5cd52ec833654ed8bb`). 캡처는 `artifacts/s1-installed-20261008/demo/screen-now.bmp`와 `web-console-vm-screen.png`. service Running, Web `200`, 보존 VM `pcv-guest-installed-04253-r1`은 Off. 이 checkpoint의 추가 host mutation은 없다.

## Task 6: Ubuntu 시연 2 — 설치 설정 (Lane 2)

- [x] VM Screen 키 입력으로 언어, 키보드, 네트워크(DHCP), 저장소(guided)를 진행하고 프로필(비밀번호는 실행 중 생성)과 SSH 선택까지 마쳐 설치를 시작한다. 화면 캡처를 남긴다. 로컬 commit.

실행 기록(2026-10-09): `pcv-it-s1-ubuntu` 콘솔 키 입력으로 설치를 시작했다. 언어 English, 키보드 English (US), 설치 종류 Ubuntu Server, 네트워크 eth0 DHCPv4 `172.29.16.136/20`, 프록시 없음, guided storage는 디스크 32GB 전체와 LVM(LUKS 없음)이다. 파일시스템은 `/` ext4, `/boot` ext4, `/boot/efi` fat32다. 프로필은 이름 `pcv`, 서버 이름 `pcvs1`, 사용자 `pcv`, 비밀번호 길이 20이고 값은 남기지 않았다. Ubuntu Pro는 Skip for now, OpenSSH server와 비밀번호 인증을 선택했고 가져온 키는 없다. snap은 선택하지 않았다. 끝 화면은 `Installing system`이며 로그가 `installing openssh-server`까지 진행했다. 중간 입력으로 설치기 디버그 셸이 열렸고 `exit`로 Network configuration에 돌아온 뒤 같은 설정을 다시 진행했다. 캡처는 `artifacts/s1-installed-20261008/demo/task6/`. 보존 VM은 그대로이고 데모 VM은 설치가 진행 중인 Running이다.

## Task 7: Ubuntu 시연 3 — 설치 완료와 재부팅 (Lane 2)

- [x] 설치 완료를 기다려 재부팅하고 로그인 화면까지 간다. 화면 캡처를 남긴다. 로컬 commit.

실행 기록(2026-10-09): 시작 화면은 `Installation complete!`였다. 설치 ISO를 `vm.eject`로 뺐고(job succeeded, DVD 경로 비음) `Reboot Now`를 눌렀다. 게스트 화면이 검은 상태로 남고 heartbeat는 `OkApplicationsUnknown`이었다. 제품 `restart`로 `pcv-it-s1-ubuntu`를 다시 켠 뒤 설치된 Ubuntu 26.04.1이 부팅했다. 로그인 화면은 `Ubuntu 26.04.1 LTS pcvs1 tty1` / `pcvs1 login:`이다. 캡처는 `artifacts/s1-installed-20261008/demo/task7/login-c.png`. 보존 VM은 그대로이고 데모 VM은 Running이다.

## Task 8: Ubuntu 시연 4 — 로그인과 네트워크 (Lane 2)

- [x] VM Screen으로 로그인해 `ip -4 addr`와 외부 이름 해석·ping으로 네트워크를 확인하고 화면 캡처를 남긴다. 로컬 commit.

실행 기록(2026-10-09): 이전 실행의 비밀번호는 남아 있지 않아 GRUB 루트 셸에서 `pcv` 비밀번호를 다시 설정했다. 길이는 20이고 값은 남기지 않았다. `passwd: password updated successfully` 뒤 정상 부팅으로 `pcv@pcvs1`에 로그인했다. eth0는 `172.29.16.136/20` dynamic UP다. `ping -c 3 1.1.1.1`은 3개 전송, 3개 수신, 0% packet loss다. `ping -c 2 archive.ubuntu.com`은 `91.189.91.81`로 해석되고 2개 전송, 2개 수신, 0% packet loss다. 캡처는 `artifacts/s1-installed-20261008/demo/task8/network.png`와 `ping-name.png`. 보존 VM은 그대로이고 데모 VM은 Running이다.

## Task 9: 정리와 Rollback (Lane 2)

- [x] 데모 VM을 끄고 지우고, `-Action Rollback`으로 `0.42.93-admin-smoke`에 돌아가 service Running, Web `200`, 설치본 version, `pcv-it-` VM `0`개, 보존 VM Off를 확인한다. 로컬 commit.

실행 기록(2026-10-09): `pcv-it-s1-ubuntu` poweroff·delete job이 모두 `succeeded`였다. Rollback DryRun은 `ok=true`였고 previous manifest는 `0.42.93-admin-smoke`였다. 실행 Rollback도 `ok=true`, exit `0`이다. 끝 상태: 설치본 manifest `0.42.93-admin-smoke`, service Running, Web `200`, `pcv-it-` VM `0`개, 보존 VM `pcv-guest-installed-04253-r1` Off. 결과는 `artifacts/s1-installed-20261008/rollback/`. 이 되돌리기는 승격 근거가 아니다.

## Task 10: 기록과 PR

- [x] 시연 기록 `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md`(시나리오, 설치본 version, 스크립트 결과, 화면 캡처 경로)를 쓰고 criteria S1을 `status=passed`, `demo_record`로 바꾼다. `pcvverify completion`을 읽기 전용으로 돌려 결과를 적는다. clean HEAD에서 솔루션 시험, web required, `git diff --check origin/main...HEAD` 뒤 push, PR, green CI 뒤 merge한다.

실행 기록(2026-10-09): 시연 기록을 쓰고 S1을 `status=passed`, `demo_record=docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md`로 바꿨다. `pcvverify completion --today 2026-10-09`는 exit `1`, `complete=false met=2/7 gaps=6 head=6a1f608`이다. S1 `met=true`, C6 `met=true`. 남은 갭은 C1·C5 `ci-wait`, S2·S3·S4 `scenario`, C5 `ubuntu-26-runner` `not_before=2026-10-19`다. 결과 `artifacts/completion/20261009-s1/result.json`. host mutation 없음. push와 merge 결과는 이 기록 뒤에 이어진다.

## Task 11: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `adr17-s1-console-20261008` Task 14(그 전 `scenario-pivot-20261008` Task 6, `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Task 12: 문서 현행화 (2026-10-09 사용자 지시)

- [x] 사용자 지시 `모든 문서 현행화`. 현재 상태 문서를 ADR-0017, S1 브라우저 콘솔, 이 호스트 dev probe 상태에 맞춘다. 과거 evidence, plan, spec은 바꾸지 않는다.

실행 기록(2026-10-09): `pcvverify completion`(v3)을 `main` `6b76274`에 읽기 전용으로 돌려 `met=1/7`을 얻고 새 감사 `docs/project-status-audit-2026-10-09.md`를 썼다. `docs/DOCUMENTATION_INDEX.md`(현황, 완료 정의, 감사 목록), `docs/DEVELOPER_INDEX.md`(v3 절, v2는 역사 기록), `README.md`(제품 API 목록), `docs/USER_GUIDE.md`(Browser console), `docs/OPERATIONS_GUIDE.md`(입력 audit 파일), `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/SERVICE_PLAN.md`를 고쳤다. 판정 중 `main` push Development Gates가 두 번 간헐 실패한 것을 찾아 backlog `BL-0011`(콘솔 rate limit 시험 시간 의존), `BL-0012`(loopback bootstrap browser 시험 재발)를 `undecided`로 남겼다. host mutation 없음.

## Nonclaims

- dev probe 설치는 승격 근거가 아니다. operational current는 `0.42.93-admin-smoke` 그대로이고 끝에 설치본도 그 version으로 돌아간다.
- S1 통과는 이 시연과 스크립트 결과로만 주장하고, S2~S4는 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
