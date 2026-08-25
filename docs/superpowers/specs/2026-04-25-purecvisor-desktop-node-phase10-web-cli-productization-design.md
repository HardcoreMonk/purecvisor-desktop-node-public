# PureCVisor Desktop Node Phase 10 Web Console/CLI 제품화 후속 설계

## 목적

Phase 10은 Desktop Node Web Console과 CLI를 spike 최소 유틸리티에서 반복 사용 가능한 운영 도구에 가깝게 다듬는다. 새 backend 기능을 만들지 않고, 이미 Phase 3B/4/7/9에서 존재하는 Local API 계약을 Web Console과 CLI 사용자 경험에 연결한다.

이 단계는 계속 `spikes/purecvisor-desktop-node/` 아래의 격리 spike 범위다. Linux `purecvisorsd`, Single Edge Web UI/API, Single Edge CLI 공개 표면과 공유하지 않는다.

## 접근 비교

### 선택안 A: checkpoint UI + browser job history + CLI token file

기존 API와 CLI 명령을 그대로 사용하면서 Web Console의 빈 checkpoint 기능과 세션 한정 job 추적 문제를 해소한다. CLI는 Phase 7/8 token file 정책에 맞춰 `--token-file`을 지원한다.

장점은 새 backend 위험이 낮고, Web/CLI 제품화 체감이 즉시 생긴다는 점이다. 단점은 shell completion, interactive prompt, VMConnect launch 같은 큰 UX 항목은 다음 단계로 남는다.

### 선택안 B: CLI interactive prompt와 shell completion 우선

CLI polish에 집중한다. 반복 입력은 줄지만 Web Console의 checkpoint 공백은 계속 남고, completion은 설치 경로와 shell별 배포 정책을 새로 정해야 한다.

### 선택안 C: VMConnect launch 우선

VM 접속 경험을 빠르게 개선할 수 있지만, local browser와 remote browser의 권한 모델, protocol handler, PowerShell 실행 경계가 커서 spike 격리와 보안 판단이 먼저 필요하다.

Phase 10은 선택안 A를 채택한다. 이유는 새 runtime 표면을 늘리지 않으면서 현재 API의 미사용 기능을 사용자 흐름에 연결하고, Phase 7/8 token file hardening과 CLI UX를 맞출 수 있기 때문이다.

## 포함 범위

Phase 10에 포함한다.

- Web Console VM detail panel에 checkpoint list/create/restore/delete UI 추가
- checkpoint create/restore/delete는 기존 queued job 계약을 사용
- checkpoint restore/delete는 destructive confirmation을 유지
- 선택한 VM 변경 시 checkpoint 목록을 함께 refresh
- job tracking을 browser `localStorage`에 저장하고 다음 reload에서 복원
- Web Console에서 저장된 job history를 지울 수 있는 control 추가
- CLI global option `--token-file <path>` 추가
- `--token`과 `--token-file` 동시 사용 거부
- token file은 존재하지 않거나 비어 있으면 구조화된 CLI 오류로 거부
- README, developer index, verification policy, follower queue 갱신

## 제외 범위

Phase 10에서 제외한다.

- 새 Local API endpoint
- database-backed browser history
- multi-browser 또는 multi-user job history sync
- Web Console checkpoint diff/metadata 상세 뷰
- VMConnect 직접 실행
- shell completion 설치
- CLI interactive prompt
- Windows Credential Manager 또는 DPAPI token storage
- Linux Single Edge runtime/UI/CLI 변경

## Web Console 설계

VM detail panel은 lifecycle actions, inventory details, checkpoint controls 순서로 구성한다.

checkpoint controls:

- `Refresh checkpoints`
- checkpoint name input
- `Create checkpoint`
- checkpoint list row마다 `Restore`, `Delete`

data flow:

1. VM row 선택
2. `GET /api/v1/vms/{id}`로 detail load
3. `GET /api/v1/vms/{id}/checkpoints`로 checkpoint list load
4. create/restore/delete는 기존 queued job endpoint 호출
5. 반환된 job은 tracked job history에 저장하고 polling 시작

checkpoint list 응답은 helper 구현 차이를 흡수하기 위해 기존 `asArray()` normalization을 사용한다. checkpoint id/name은 `name`, `id`, `checkpoint_name` 순서로 고른다.

## browser job history 설계

현재 Web Console의 `trackedJobs`는 browser session 메모리에만 남는다. Phase 10은 `localStorage` key `pcvDesktopTrackedJobs.v1`에 최근 job을 저장한다.

정책:

- 저장 대상은 API가 반환한 job object다.
- 최대 50개만 유지한다.
- 저장 실패나 JSON parse 실패는 UI를 깨지 않고 빈 history로 처리한다.
- `Clear history` 버튼은 localStorage와 memory state를 함께 비운다.
- polling 성공 시 최신 job 상태를 다시 저장한다.

이 방식은 단일 browser local history일 뿐이며, persistent server-side job history가 아니다.

## CLI token file 설계

CLI global option은 다음을 지원한다.

```text
pcvcli --api http://127.0.0.1:7777 --token-file D:\PureCVisor\desktop-node\api-token.txt host status
```

규칙:

- `--token`과 `--token-file`은 mutually exclusive다.
- token file은 CLI 실행 시 읽는다.
- trailing newline은 trim한다.
- missing file은 `Token file was not found.` 오류로 exit code `2`를 반환한다.
- empty token은 `Token file is empty.` 오류로 exit code `2`를 반환한다.
- transport에는 기존 `ApiToken` 문자열로만 전달한다.

## 현재 구현 상태

2026-04-25 기준 Phase 10 구현은 다음 상태다.

- Web Console VM detail panel에 checkpoint refresh/create/restore/delete controls가 추가됐다.
- checkpoint create/restore/delete는 기존 queued job endpoint를 호출하고, 반환 job을 tracked history에 저장한다.
- tracked job history는 `localStorage` key `pcvDesktopTrackedJobs.v1`에 최대 50개 저장한다.
- Jobs section에 `Clear history` control을 추가했다.
- CLI는 `--token-file <path>`를 지원한다.
- CLI는 `--token`과 `--token-file` 동시 사용, missing file, empty file을 요청 전 exit code `2`로 거부한다.
- shell completion, interactive prompt, VMConnect launch는 후속으로 남긴다.

## 완료 기준

Phase 10은 다음을 만족하면 완료다.

- Web Console static suite가 checkpoint UI action과 persistent browser job history 표식을 검증한다.
- CLI suite가 `--token-file`과 token source conflict를 검증한다.
- Web JavaScript syntax check가 통과한다.
- API, CLI, Web, service, Hyper-V 기본 검증 기대값이 최신 문서와 일치한다.
- Desktop Node spike 격리 규칙이 유지된다.
