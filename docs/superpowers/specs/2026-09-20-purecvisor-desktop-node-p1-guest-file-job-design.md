# Desktop Node P1-8 제한적 guest 파일 job 설계

- Design-ID: `purecvisor-desktop-node-p1-guest-file-job-v1`
- 작성일: `2026-09-20`
- 문서 상태: `implemented-slice-3`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P1-8
- 선행: ADR-0009 guest execution, P1-7 template lock
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

lab에서 ISO/스크립트 한 개를 guest에 넣어야 한다. Workstation HGFS 공유 폴더는 제품
경계 밖이다. SERVICE_PLAN P1-8은 **credential-ref + path/size allowlist**의 queued job만
연다.

## 결정

- Feature는 새 ID를 만들지 않는다. `pcv.vm.guest-execution`에 붙인다 (28 feature 유지).
- 전송은 PowerShell Direct / credential-ref다. `Copy-VMFile`만으로 credential-ref를 우회하지
  않는다.
- Slice 1은 HTTP route를 추가하지 않는다. allowlist 계약만 고정한다.
- 방향은 이 설계에서 `host-to-guest`만. pull은 이후 slice다.
- 한 파일만. 디렉터리, wildcard, ADS, UNC, `..` 탈출은 거절한다.
- 호스트 경로 루트: `%ProgramData%\PureCVisor\desktop-node\guest-files\`
- 게스트 경로 접두사: `C:\Users\Public\PureCVisor\`
- 최대 크기: 64 MiB
- `shared_folder` 또는 HGFS 형태는 `PCV_GUEST_FILE_HGFS_FORBIDDEN`
- template lock된 VM은 이후 native mutation에서 `PCV_VM_TEMPLATE_LOCKED`
- RBAC는 `guest.exec`

## Slice 1 범위

- `GuestFileJobContract.Evaluate`가 path/size/direction/credential-ref/HGFS를 판정한다.
- 거절 코드: `PCV_GUEST_FILE_PATH_NOT_ALLOWED`, `PCV_GUEST_FILE_SIZE_LIMIT`,
  `PCV_GUEST_FILE_DIRECTION_INVALID`, `PCV_GUEST_FILE_CREDENTIAL_REF_REQUIRED`,
  `PCV_GUEST_FILE_HGFS_FORBIDDEN`
- 실제 복사, HTTP, CLI, Web은 이 slice가 아니다.

## Slice 2 범위

- native `vm.guest.file.preview` (Read)와 `vm.guest.file` (Mutation).
- Preview는 allowlist + host file 길이. Copy는 PowerShell Direct `Copy-Item -ToSession`.
- HTTP/CLI/Web은 이 slice가 아니다. Domain catalog 41→43.

## Slice 3 범위

- `POST /api/v1/vms/{vmId}/guest/file/preview` NativeProductOperation.
- `POST /api/v1/vms/{vmId}/guest/file` queued mutation.
- `pcvcli vm guest-file <vm> --host-path PATH --guest-path PATH --credential-ref REF --dry-run|--yes`.
- Web은 excluded. Route catalog 63→65, queued 28→29.

## Slice 4 이후 (이번 checkpoint 아님)

- Web Console guest-file control

## 비목표

- HGFS / 공유 폴더 / 미러
- guest-to-host pull
- 디렉터리 재귀 복사
- raw password
- current-evidence write, package-pair, Lane 2
