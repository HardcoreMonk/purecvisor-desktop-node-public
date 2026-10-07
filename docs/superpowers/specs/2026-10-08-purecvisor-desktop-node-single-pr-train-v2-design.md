# Release train 단일 PR 설계 v2 (Lane 3 pin 허용)

- Design-ID: `pcv-single-pr-train-v2`
- 작성일: `2026-10-08`
- 문서 상태: `accepted` (2026-10-08 사용자 승인 `1,2,3,4,5,6,7`의 5, backlog `BL-0005` counts와 새 설계 승인)
- 대체: `pcv-single-pr-train-v1`(`docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`)의 §3.1 경로 확인.
  나머지(payload commit run 인용, PR 하나, merge 뒤 확인과 revert)는 v1 그대로다.
- 변경 등급: M (검증 도구 규칙. 제품 payload 변경 없음)
- host/VM/service/package mutation: `false`

## 1. 문제

v1 §3.1은 `git diff --name-only <payload commit>..<PR head>`에 `src/`, `web/src/`, `config/`, `.github/`가 있으면 Lane 3를
쓰지 않고 멈춘다. 그런데 모든 Lane 3 승격은 다음 파일을 바꾼다(0.42.91 PR #55, 0.42.92 PR #63에서 같다).

| 파일 | 바뀌는 것 | 바꾸는 쪽 |
| --- | --- | --- |
| `config/pcv-development-policy-contract-spec-v1.json` | `AGENTS.md` pin `sha256` 한 줄 | Lane 3 문서 도구 spec pin 단계 |
| `config/pcv-installed-smoke-contract-spec-v1.json` | `packaging/windows-desktop-node/README.md` pin `sha256` 한 줄 | 같음 |
| `config/pcv-manual-admin-readiness-contract-spec-v1.json` | `current-evidence.json`, descriptor 문서 pin `sha256` 두 줄 | 같음 |
| `src/DesktopNode.Delivery.Tests/Delivery/Installed/InstalledContractVerifier.cs` | spec SHA 상수 한 줄 | 같음 |
| `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/ManualAdminContractVerifier.cs` | spec SHA 상수 한 줄 | 같음 |
| `src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs` | spec SHA 상수 한 줄 | 같음 |
| `src/DesktopNode.Verification.Tests/CurrentEvidenceVerifierTests.cs` | 기대 current version 한 줄 | 손 |

그래서 첫 단일 PR train `0.42.92`가 Lane 3에서 멈췄고, 두 PR 방식으로 마쳤다(backlog `BL-0005`).

## 2. 결정

1. `CurrentEvidenceVerifierTests`는 기대 version을 하드코딩하지 않고 `docs/ga-ready/current-evidence.json`의 current version을
   읽어 검증기 결과와 비교한다. Lane 3는 이 파일을 고치지 않는다.
2. `train-path-check`에 Lane 3 pin 허용 목록을 둔다. 위 표의 앞 여섯 파일이다. 이 파일들은 product payload(MSI에 들어가는
   것)가 아니다.
3. 허용 목록 파일은 내용까지 확인한다. `git diff -U0 <payload>..<head> -- <파일>`의 바뀐 줄(`+`/`-`, 헤더 제외)이 모두 아래
   형식이고 `+` 줄과 `-` 줄 수가 같아야 허용한다. 하나라도 다르면 product path로 보고 멈춘다.

       ^[+-]\s*("sha256":\s*)?"[0-9a-f]{64}"[,;]?\s*$

4. 결과 계약 `pcv-train-path-check-result-v1`에 `allowed_pin_paths`(허용한 pin 파일 목록)를 더한다. `ok`와 exit code의 뜻은
   그대로다: 허용 목록 밖 product path가 하나라도 있으면 `ok=false`, exit `1`.
5. `DEVELOPMENT_PROCEDURE.md` §10과 Lane 3 `main-push-payload` 문서의 경로 확인 문장은 "허용된 Lane 3 pin 외에는"으로 바꾼다.

## 3. 버린 선택지

| 안 | 버린 이유 |
| --- | --- |
| pin 파일을 `docs/`나 `packaging/`로 옮김 | 검증기와 spec 위치, Required CI inventory, 여러 pin 계약을 함께 옮겨야 한다. 범위가 크다 |
| 경로만 허용하고 내용은 보지 않음 | 같은 파일의 검증 로직이 Lane 3 아래에서 바뀌어도 통과한다 |
| 계속 두 PR 방식 | merge 뒤 run 인용 순환(v1 §2)이 다시 생기고 PR CI 대기가 두 번이다 |

## 4. 시험

- `TrainSinglePrTests`(Verification.Tests): 허용 pin 파일의 SHA 한 줄 변경은 `ok=true`와 `allowed_pin_paths`, 같은 파일에서
  SHA가 아닌 줄 변경은 `ok=false`, 허용 목록 밖 `src/` 변경은 지금처럼 `ok=false`.
- `CurrentEvidenceVerifierTests`: 하드코딩 없이 통과.

## 5. Nonclaims

- 제품 동작과 Required CI 구성은 바뀌지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
