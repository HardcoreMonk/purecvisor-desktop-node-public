# S1 브라우저 재증명 시도 (2026-11-11, 중단)

- 시나리오: `S1`. 브라우저에서 ISO로 VM을 만들고, 브라우저 콘솔로 OS를 설치하고, 네트워크 연결을 확인한다.
- 기준: `config/project-completion-criteria.json`의 S1. ADR-0017. 2026-11-11 제로베이스 감사 후속 조치(campaign `zero-base-followup-20261111` Task 6~7)다.
- 이 기록은 이 호스트 설치본 `0.43.4-admin-smoke`에서 한 재증명 시도다. 승격 근거가 아니고, 판정 입력(`config/project-completion-criteria.json`)을 바꾸지 않는다. S2~S4는 다루지 않는다. public trusted signing이나 external stable publication을 주장하지 않는다.
- 결과: **중단**. 마법사로 VM 생성, 네트워크 연결, 시작, 콘솔 화면 표시는 됐다. Ubuntu 설치는 사용자 정보 화면에서 멈췄다. 브라우저 콘솔 키 입력이 비밀번호를 정확히 넣지 못했다(backlog BL-0046, BL-0047).

## 설치본

- 설치본 `0.43.4-admin-smoke+7b929ec`(operational current). service Running/Automatic, Web `http://127.0.0.1/` `200`.
- 설치본은 계정 모드다. 시나리오 operator 계정 `pcv-s13-operator`로 로그인했다(사용자 결정 `"임시 operator 계정 (권장)"`). 비밀번호는 기록하지 않았다.
- 보존 VM `pcv-guest-installed-04253-r1`, `pcv-it-s2-source`는 Off이고 바뀌지 않았다.

## 실행

1. 브라우저 시나리오 스크립트(PR #150, `main` `6da5c47`)
   - 명령: `npm run scenario:browser --prefix web -- --scenario s1 --execute --username pcv-s13-operator --iso <ubuntu-26.04.1-live-server-amd64.iso> --vm pcv-it-s1r-ubuntu --cpu 2 --memory 4096 --disk 32`(비밀번호는 환경 변수 `PCV_SCENARIO_PASSWORD`)
   - 결과: `summary.json` `result=pass`, 77초
     - 로그인 9.3초
     - 마법사 생성 job `succeeded`
     - `Default Switch` 연결 job `succeeded`(확인 수락)
     - 시작 job `succeeded`
     - 콘솔 frame 5.4초 뒤 보임
     - Enter 뒤 7.5초에 화면이 바뀌었다(Ubuntu 커널 부팅 로그)
2. Ubuntu Server 설치는 같은 CDP 도우미를 쓰는 저장소 밖 도우미로 이어 갔다. 도우미는 Web Console 화면만 조작했다. 로그인한 뒤 VM을 고르고 콘솔 탭을 연다. 콘솔 canvas에 키 이벤트를 보내거나 텍스트 보내기 폼을 쓴다. canvas를 캡처한다.
   - 화면 순서: 언어 English → 키보드 English (US) → 설치 유형 Ubuntu Server → 네트워크(DHCP) → 프록시 없음 → 미러 → 저장소(디스크 전체 32GB, LVM, LUKS 없음) → 요약(`/` ext4 LVM, `/boot` ext4, `/boot/efi` fat32) → 확인 대화상자 Continue → 사용자 정보
   - 사용자 정보: 이름 `pcv`, 서버 이름 `pcvs1r`, 사용자 이름 `pcv`는 정확히 들어갔다. 비밀번호(20자, 실행 경계에서 만들고 기록하지 않음)는 두 칸이 일치하지 않아 Done을 누르지 못했다.

## 관찰

| 항목 | 값 |
| --- | --- |
| 콘솔 frame route 응답(경합 없음) | 640×480·800×600·1024×768 모두 장당 0.82~0.95초 |
| 브라우저에 새 frame이 그려지는 간격 | 약 3.5초(1024×768, 2 fps 설정) |
| 키 하나가 반영된 화면을 보기까지 | 약 7초(키 뒤에 요청한 frame 두 장) |
| console input 요청 처리 | press와 release 각각 약 2초, 글자 하나에 약 4.4초 |
| 키를 보통 속도(약 0.35초 간격)로 보낼 때 | 요청이 쌓여 일부가 빠졌다. 비밀번호 20자 중 약 12자만 들어갔고, 뒤따른 이동 키와 확인 칸 입력이 반영되지 않았다 |
| 요청마다 끝나기를 기다리며 보낼 때 | 빠진 요청은 없었다(Backspace 30번에 요청 60건). 그래도 같은 20자를 두 칸에 넣은 결과가 서로 달랐다("Passwords do not match") |
| 텍스트 보내기 폼(`kind=text`) | `pcv` → 빈 칸, `pcvs1r` → `n`, 23자 → 9자와 4자 |
| 서버 감사 기록(`console-input-audit.jsonl`) | 보낸 키 이벤트가 모두 순서대로 서버에 도착했다 |

- 도구 결함(제품 결함이 아님): 시도 초반에 저장소 밖 도우미가 실행마다 Edge를 다 끄지 못했다.
  - 한때 Edge 62개가 같은 VM 화면을 동시에 요청했다. VM당 frame 간격 제한 때문에 서로 막혔고 화면 갱신이 멈췄다. 로그인 요청 시간 초과도 한 번 났다.
  - 남은 Edge를 끈 뒤 frame route는 정상(위 표)이었다. 같은 CDP 도우미를 쓰는 시나리오 스크립트의 종료 처리는 같은 campaign에서 고친다.

## 화면 캡처

- `docs/ga-ready/demo/s1-20261111/installer-language.png`: 브라우저 콘솔 canvas의 Ubuntu 설치 프로그램 첫 화면
- `docs/ga-ready/demo/s1-20261111/storage-summary.png`: 저장소 요약
- `docs/ga-ready/demo/s1-20261111/profile-mismatch.png`: 사용자 정보 화면. 두 비밀번호 칸이 일치하지 않는다

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | (비어 있음) |
| 확인 일시 | - |
| 확인 방법 | - |

확인자 칸이 비어 있으므로 이 기록은 에이전트 자가 보고다.

## 정리

- `pcv-it-s1r-ubuntu`는 Running이고, 설치 프로그램 사용자 정보 화면에 있다. 콘솔 입력 결함을 고친 뒤 다시 시험할 대상으로 남겨 둔다. 지우는 일은 재시연 campaign의 정리 단계에서 한다.
- 시나리오 계정 `pcv-s13-operator`는 enabled다. 재시연 뒤 끈다.
- 보존 VM 두 개는 바뀌지 않았다. 설치본, service, MSI를 바꾸지 않았다.

## 판정

중단. 현행 설치본의 브라우저 콘솔로는 OS 설치를 끝낼 수 없었다. 원인은 콘솔 키 입력(BL-0046, BL-0047)이다. VM 생성·시작·콘솔 화면 표시는 브라우저로 됐다. S1 재시연은 콘솔 입력을 고친 버전이 train으로 설치된 뒤 다시 한다(사용자 결정 2026-11-11 `"1"`).
