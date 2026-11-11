# S1~S3 브라우저 재시연 기록 (설치본 0.43.7, 2026-11-12)

- campaign: `train-04307-20261111`(plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04307.md` Task 12~17)
- 설치본: `0.43.7-admin-smoke+3898139`. operational current이고 train PR #161로 승격했다.
- 계정: 시나리오 operator `pcv-s13-operator`. 비밀번호는 실행 경계에서 새로 만들어 재설정했고 기록하지 않았다.
- 결과: **S1 진행 중 중단**. S2와 S3는 하지 않았다. 판정 입력(`config/project-completion-criteria.json`)은 바꾸지 않았다.
  - 브라우저에서 VM 생성부터 Ubuntu 설치 프로그램의 사용자 정보와 설치 시작까지 했다.
  - 재부팅, 로그인, 네트워크 확인 전에 checkpoint 예산(Lane 2 tool batch 12회)에 닿아 멈췄다.
  - 사용자 결정 `"2"`(재시연 중단, 정리와 기록 PR만)로 끝냈다.

## S1: 브라우저 VM 생성과 Ubuntu 설치

1. 브라우저 시나리오 S1 `--execute`: **pass**(56초)
   - 명령: `npm run scenario:browser --prefix web -- --scenario s1 --execute --username pcv-s13-operator --iso <ubuntu-26.04.1-live-server-amd64.iso> --vm pcv-it-s1r2-ubuntu --cpu 2 --memory 4096 --disk 32`
   - 로그인, 생성 마법사, 생성 job, Default Switch 연결, 시작, 콘솔 frame, 키 입력과 화면 변화까지 12단계다.
2. Ubuntu Server 설치는 Web Console 콘솔 canvas의 키 처리기로 진행했다. 텍스트 보내기 폼은 쓰지 않았다(BL-0047).

| 화면 | 결과 |
| --- | --- |
| 언어 | English |
| 키보드 | English (US) |
| 설치 유형 | Ubuntu Server |
| 네트워크 | eth0 DHCPv4 `172.21.65.89/20`(Default Switch NAT) |
| 프록시, 미러 | 프록시 없음, `archive.ubuntu.com` 시험 통과 |
| 설치 프로그램 업데이트 | 업데이트 없이 계속 |
| 저장소 | 디스크 전체 32GB, LVM. 요약 뒤 파괴적 작업 확인에서 Continue |
| 사용자 정보 | 이름 `pcv`, 서버 이름 `pcvs1r2`, 사용자 이름 `pcv`. 비밀번호 20자를 두 칸에 넣었고 일치해 넘어갔다 |
| Ubuntu Pro | 건너뜀 |
| SSH | OpenSSH server 설치 선택 |
| 설치 | 진행 중(`openssh-server` 설치 단계)에서 기록을 멈췄다 |

- 사용자 정보 화면의 키 입력은 요청 144건, 중앙값 169ms, 거절 0건이었다.
  - 첫 S1 시도(설치본 `0.43.4`, `docs/ga-ready/demo/s1-browser-reproof-2026-11-11.md`)는 이 화면에서 두 비밀번호 칸이 일치하지 않아 멈췄다.
  - 이번에는 두 칸이 일치해 다음 화면으로 넘어갔다.
- 화면마다 기본 초점이 `[ Done ]`에 있어 Enter로 넘어갔다. 저장소 화면은 첫 초점이 디스크 선택에 있어서 아래 방향키 다섯 번으로 `[ Done ]`에 갔다.
- 캡처는 `artifacts/s1r2-04307-20261112/console/`(커밋하지 않음)에 있다.

## 멈춘 이유

- 시연 도우미는 화면 하나를 넘길 때마다 키 입력 실행과 캡처 확인에 tool batch 두 번을 썼다. 그래서 설치 하나가 Lane 2 checkpoint 한도(12회)를 넘었다.
- 제품 쪽 막힘은 없었다. 생성, 연결, 시작, 콘솔 화면, 키 입력, 사용자 정보 입력은 모두 됐다.
- 남은 S1 단계는 재부팅, 로그인, `ip -4 addr`와 `ping`(주소와 이름)이다. S2(설치된 VM template 잠금과 복제)와 S3(checkpoint 생성·복원·예약)는 하지 않았다.
- 다음 재시연에서는 설치 화면 순서를 step 파일로 만들어 한 번 실행에 설치를 끝내는 방식을 권장한다.

## 정리

- `pcv-it-s1r2-ubuntu`와 `pcv-it-s1r-ubuntu`(첫 S1 시도 VM)를 PCVCLI `vm stop`, `vm delete --yes`(managed delete)로 지웠다. 두 VM 폴더도 남지 않았다.
- `pcv-s13-operator`를 disable했다. 계정은 `pcv-admin2` admin만 enabled다.
- 저장소 밖 임시 비밀 파일(operator 비밀번호, 게스트 비밀번호)을 지웠다.
- 남은 시나리오 Edge는 0개다. 보존 VM `pcv-guest-installed-04253-r1`과 S2 template `pcv-it-s2-source`는 Off 그대로다. 설치본, service, MSI는 바꾸지 않았다.

## Nonclaims

- 이 기록은 S1 완료를 주장하지 않는다. S2, S3는 이번에 다시 시연하지 않았다.
- 디자인 감사나 접근성 판정을 새로 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
