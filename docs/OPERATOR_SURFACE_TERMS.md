# Desktop Node 운영자 화면 용어

작성 기준: 2026-07-14

ADR-0011에 따라 현재 운영자 표면은 Web Console과 PCVCLI다. Web Console은 대화형
운영, PCVCLI는 terminal automation과 JSON 출력을 소유한다.

## 제품

- 제품 이름: PureCVisor Desktop Node
- 배포 경계: 내부 사설망 전용
- Public release 경계: public trusted signing, winget public submission, external stable publication은 범위 밖이다

## 접근

- 로그인: account/RBAC/JWT session이 구성되어 있으면 사용한다
- 대체 인증: account가 구성되지 않은 경우 bearer token이 authoritative gate다
- Secret 규칙: token, password, JWT, refresh token, signing key, private key, PFX password 값은 표시하지 않는다

## VM 작업

- Inventory: VM 목록과 VM 상세
- Lifecycle actions: create, start, stop/poweroff, shutdown/guest-shutdown, restart, delete
- Console access: Web Console/noVNC session 조회는 `console` 또는 Linux `pcvctl` 호환 `vnc` alias로 부른다
- Hyper-V QoS/readback: `limit`은 CPU/MEM resource mutation alias이고, `blkio-get`,
  `bandwidth`, `guest-agent-status`, `guest-ping`은 Hyper-V readback/readiness로 부른다.
  Linux cgroup, libvirt blkio, qemu guest agent 호환 claim으로 설명하지 않는다
- Web QoS exposure: Web Console 선택 VM detail은 ADR-0008의 QoS preview/apply와
  `QoS / Guest Readback` panel에서 `blkio`, `bandwidth`, `guest-agent/status`,
  `guest-agent/ping` readback을 제공한다.
  2026-05-21의 TUI readback 기록은 dated historical predecessor다
- Delete 경계: PureCVisor-managed VM만 삭제할 수 있다
- VM manage 확인: Web Console과 CLI는 existing Hyper-V VM을 PureCVisor managed로 승격하기 전에 명시 확인을 요구한다
- VM delete 확인: Web Console과 CLI는 destructive VM delete 전에 명시 확인을 요구한다
- Checkpoint mutation: Web Console은 checkpoint restore/delete confirmation dialog를 요구한다. CLI checkpoint command는 API job으로 라우팅되는 명시 subcommand다. CLI snapshot command는 같은 API job으로 라우팅되는 Linux `pcvctl` 호환 alias다

## Diagnostics

- Diagnostic bundle: redaction이 적용된 server-side support bundle
- Download: 사용자가 직접 시작하는 bundle download
- Evidence handoff: 운영자용 요약은 raw secret 값이 아니라 sanitized artifact root를 가리킨다

## Current-card 여정

- Web Console과 CLI는 GET /api/v1/ops/summary를 같은 운영자 current-card 출처로 사용한다
- Web Console은 `batch_evidence.latest`를 Dashboard/Evidence current card에 표시한다
- CLI는 `pcvcli ops summary`를 current-card 확인 명령으로 사용한다
- current-card snapshot parity는 Web Console과 CLI가 같은 ops summary snapshot을 읽는지 확인하는 operator surface contract다

## Release와 Update

- Update: 검증된 source 또는 catalog를 통한 internal package apply
- Rollback: transaction journal evidence와 함께 previous product root를 복원
- Distribution: internal signed 또는 AllowUnsignedDev admin-smoke evidence만 인정하며 public distribution claim으로 해석하지 않는다
