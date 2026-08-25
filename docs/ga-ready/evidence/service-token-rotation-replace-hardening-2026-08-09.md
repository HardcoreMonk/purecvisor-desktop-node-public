# ServiceTokenRotationRevoke File.Replace 간헐 실패 재현과 보강 (2026-08-09)

evidence_id: `service-token-rotation-replace-hardening-2026-08-09`
result: `PASS`
evidence_scope: `code-level-repro-and-hardening`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

선행: `docs/followup-work-plan-2026-08-07.md` §3, `docs/followup-work-record-2026-08-06.md` §12.
이전 세션은 `82`회 무재현으로 제품 수정을 보류하고 진단 단언만 넣었다.

## 1. 재현

| 조건 | 결과 |
| --- | --- |
| 단독 filter `ServiceTokenRotationRevokeReplacesProtectedTokenFile...` `20`회 | **`20/20` PASS** |
| `DesktopNode.Host.Tests` 전체 suite (동일 프로세스 부하) | **재현 1회 이상** |

전체 suite 실패 메시지(진단 단언 출력, 원문):

```text
rotation reported Ok=false.
error_code=PCV_HOST_SERVICE_TOKEN_ROTATION_FAILED;
error_message=Desktop Node service token rotation failed: 바꿀 파일을 제거할 수 없습니다.;
service_token_mutation=failed;
atomic_replace_status=not-run;
backup_write_status=written;
service_reload_status=not-run;
```

해석:

- `backup_write_status=written` → 당시 경로의 `File.Copy(tokenPath, backupPath)`는 성공
- `atomic_replace_status=not-run` → `File.Replace` 성공 직후에만 `completed`로 올리므로
  **replace 단계에서 예외**
- 메시지 `바꿀 파일을 제거할 수 없습니다` / `Unable to remove the file to be replaced` 는
  destination(`api-token.dpapi.json`) 삭제에 실패한 Windows `IOException`

TEMP 단독 `File.Replace` 루프 `200`회는 `0` 실패였다. 결함은 API 자체 불가역 버그가 아니라
**전체 suite 부하 아래 destination에 대한 일시 잠금** 계열로 분류한다.

## 2. 이전 가설 재확인

§12.3이 `File.Move`로 통일하면 더 약해진다고 측정한 결론은 유지한다. 이번 수정도
`File.Move(overwrite)`로 destination을 바꾸지 않는다. 존재하는 token 경로에는 계속
`File.Replace`를 쓴다.

## 3. 제품 수정 (`DesktopNodeServiceTokenOps`)

변경 파일: `src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs`

1. **`File.Copy` + `File.Replace(..., destinationBackupFileName: null)` 제거**  
   대신 `File.Replace(tempPath, tokenPath, backupPath, ignoreMetadataErrors: true)` 한 호출로
   backup+replace를 수행한다. live token을 미리 열어 복사한 뒤 같은 경로를 지우라는 두 단계
   창을 없앤다.
2. **`IOException` 한정 short retry** (`5`회, `25ms * attempt` backoff)  
   재현 메시지가 가리킨 destination 제거 실패에만 적용한다. temp 파일이 남아 있을 때만 재시도.
3. 존재하지 않는 token 경로는 기존처럼 `File.Move(temp, tokenPath)`.

이 수정은 “재시도로 flake를 가린다”가 아니라, (a) 불필요한 두 단계 open 창을 제거하고
(b) 측정된 transient `IOException`에 한해 Windows 파일 경합 완화를 적용한 것이다.

## 4. 검증

| 명령 | 결과 |
| --- | --- |
| filter ServiceToken 단독 | 통과 `1`, 실패 `0` |
| Host.Tests 전체 suite × `10` (수정 후) | **`10/10` PASS** (`FULL_FAIL=0`) |

## 5. Nonclaims

- 간헐 실패가 **영원히 0**임을 수학적으로 증명하지 않는다. 재현 경로와 수정의 대응 관계만
  고정한다.
- public trusted signing / installed host mutation / package chain을 주장하지 않는다.
- guest-exec credentialed smoke는 이 evidence 범위가 아니다.
