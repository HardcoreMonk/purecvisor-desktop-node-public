# S1 브라우저 콘솔 시연 (2026-10-08)

- 시나리오: `S1`. 브라우저에서 ISO로 VM을 만들고, 브라우저 콘솔로 OS를 설치하고, 네트워크 연결을 확인한다.
- 기준: `config/project-completion-criteria.json`의 S1. 설계 `pcv-s1-browser-console-v1`, ADR-0017.
- 이 기록은 이 호스트의 dev probe 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable publication을 주장하지 않는다. S2~S4는 주장하지 않는다.

## 설치본

- dev probe 설치본: `0.42.94-admin-smoke`. 기준 commit은 `main` `6b76274`이고, package는 `AllowUnsignedDev` / `LocalTest`다.
- 시연 동안 그 설치본에서 Web Console과 Local API를 썼다. 시연이 끝난 뒤 제품 Rollback으로 설치본은 `0.42.93-admin-smoke`다.
- service는 Running, Web은 `http://127.0.0.1/` `200`이었다. 보존 VM `pcv-guest-installed-04253-r1`은 Off로 남았다.

## 시나리오 스크립트

- 명령: `npm run scenario:s1-console -- --execute`
- VM: `pcv-it-s1-smoke-20261008`
- 결과: `artifacts/s1-installed-20261008/scenario-smoke/`의 `summary.json` `result=pass` (`73`초). session `200`, create·start·poweroff·delete job `succeeded`, 화면 캡처 2장. 끝 상태 `pcv-it-` VM `0`개.

## Ubuntu 설치 시연

- ISO: `D:\Downloads\ubuntu-26.04.1-live-server-amd64.iso`
- VM: `pcv-it-s1-ubuntu`. Generation 2, vCPU 2, memory 4096MB, disk 32GB, Secure Boot On / Microsoft UEFI Certificate Authority, Default Switch.
- 브라우저 VM Screen과 같은 콘솔 입력 route로 진행했다. 언어 English, 키보드 English (US), Ubuntu Server, 프록시 없음, guided storage(디스크 32GB, LVM, LUKS 없음). 파일시스템은 `/` ext4, `/boot` ext4, `/boot/efi` fat32다.
- 네트워크: eth0 DHCPv4 `172.29.16.136/20`.
- 프로필: 이름 `pcv`, 서버 `pcvs1`, 사용자 `pcv`. 비밀번호 길이는 20이고 값은 남기지 않았다. Ubuntu Pro는 건너뛰었고 OpenSSH server와 비밀번호 인증을 선택했다. snap은 선택하지 않았다.
- 설치 화면은 `Installing system`까지 갔고 로그가 `installing openssh-server`를 포함했다.
- 재부팅 뒤 로그인 화면: `Ubuntu 26.04.1 LTS pcvs1 tty1` / `pcvs1 login:`.
- `pcv@pcvs1` 로그인 후 `ip -4 addr`의 eth0는 `172.29.16.136/20` dynamic UP다. `ping -c 3 1.1.1.1`은 3개 전송, 3개 수신, 0% packet loss다. `ping -c 2 archive.ubuntu.com`은 `91.189.91.81`로 해석되고 2개 전송, 2개 수신, 0% packet loss다.

## 화면 캡처

- 설치 프로그램 첫 화면: `artifacts/s1-installed-20261008/demo/screen-now.bmp`
- Web Console VM Screen: `artifacts/s1-installed-20261008/demo/web-console-vm-screen.png`
- 설치 진행: `artifacts/s1-installed-20261008/demo/task6/installing.png`
- 로그인 화면: `artifacts/s1-installed-20261008/demo/task7/login-c.png`
- 주소와 `1.1.1.1` ping: `artifacts/s1-installed-20261008/demo/task8/network.png`
- 이름 해석 ping: `artifacts/s1-installed-20261008/demo/task8/ping-name.png`
- 스크립트 산출: `artifacts/s1-installed-20261008/scenario-smoke/`
- Rollback 산출: `artifacts/s1-installed-20261008/rollback/`

## 판정

- `pcvverify completion` 읽기 전용, `--today 2026-10-09`, exit `1`.
- `completion: complete=false met=2/7 gaps=6 head=6a1f608836b031ef641c7a26b43bc2310858ce11`
- S1 `met=true` (`status=passed`, 이 문서). 충족은 S1과 C6이다.
- 남은 갭: C1 `ci-wait`, S2·S3·S4 `scenario`, C5 `ci-wait`, C5 `ubuntu-26-runner` `not_before=2026-10-19`.
- 위생: C2·C3·C4 충족. C7은 `BL-0011`, `BL-0012`가 `undecided`다.
- 결과 JSON: `artifacts/completion/20261009-s1/result.json`. 이 판정은 완료를 주장하지 않는다.

## 정리

- `pcv-it-s1-ubuntu` poweroff·delete job은 `succeeded`다. 끝 상태 `pcv-it-` VM `0`개.
- 제품 Rollback `ok=true`. 설치본 manifest `0.42.93-admin-smoke`, service Running, Web `200`, 보존 VM Off.
