# 제품 Wrapper Native Repair Duplicate Start RCA - 2026-05-13 0.42.10

```text
evidence_id: product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210
scope: historical-rca-only
result: BLOCKED_HISTORICAL_RCA
affected_version: 0.42.10-admin-smoke
superseded_by: 0.42.11-admin-smoke
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

## 판정

`0.42.10-admin-smoke`는 `0.42.9-admin-smoke -> 0.42.11-admin-smoke`
package-pair PASS 전에 발견된 historical RCA 대상이다. 이 package는
`RepairInstalled -BatchEvidenceRoot`에서 native service-action으로 SCM `PathName`을
갱신했고 서비스도 이미 `Running`으로 만들었지만, outer product wrapper가 이어서
legacy `sc.exe start PureCVisorDesktopNode`를 다시 호출해 Windows `1056 already
running`을 반환했다.

따라서 blocker는 서비스 구성 실패가 아니라 중복 start 처리 실패다. Current
package-pair, product wrapper, full admin host mutation claim은
`0.42.11-admin-smoke` evidence가 소유한다.

## 입력 산출물

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260513-04210` |
| MSI | `PureCVisorDesktopNode-0.42.10-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273` |
| provenance commit | `d7d5ba38ee1d4f74676477eb13701af65abca008` |
| payload aggregate SHA-256 | `bd48eb866d4e3e158e4e82400c67d583f4e652aae09e85ba604e6c07051647b7` |
| product wrapper SHA-256 | `0f0c69194b00631aa5abe6a7ce5883f02b55d42fd7d17f6f228d0183c220f5af` |
| service host SHA-256 | `4838217ee5d233044a4ad6647d973ea2aaf0678fd0486f9c0dee629d0cf95c2b` |
| update package | `artifacts/manual-admin-campaign-20260513-0429-04210/lifecycle/PureCVisorDesktopNode-0.42.10-admin-smoke-update.zip` |
| update package SHA-256 | `05a107f4803ec8ed1e08f7aeba1b49fa3795c7d16565db8f904fd599ba07633f` |

## 관측된 실패 흐름

| 단계 | artifact | 판정 |
| --- | --- | --- |
| update | `artifacts/manual-admin-campaign-20260513-0429-04210/lifecycle/product-update-rollback/02-update.json` | `ok=true`; installed manifest가 `0.42.10-admin-smoke`로 이동 |
| wrapper repair | `artifacts/manual-admin-campaign-20260513-0429-04210/lifecycle/product-update-rollback/04-wrapper-repair-installed-batch-root.json` | `ok=false`; `service.install` native action은 `exit_code=0`이고 service `Status=running`; 이어진 outer `service.start`가 `exit_code=1056` |
| rollback | `artifacts/manual-admin-campaign-20260513-0429-04210/lifecycle/product-update-rollback/05-rollback-after-wrapper-start-1056.json` | `ok=true`; previous version `0.42.9-admin-smoke` restore, service health HTTP `200`, failed root diagnostics 보존 |
| direct native repair | `artifacts/manual-admin-campaign-20260513-0429-04210/lifecycle/product-update-rollback/06-direct-native-repair-after-rollback-to-artifacts-root.json` | `Ok=true`; canonical `artifacts` batch root로 native repair 재적용 |

`04-wrapper-repair-installed-batch-root.json`의 핵심 failure detail은
`Command 'sc.exe start PureCVisorDesktopNode' exited with code 1056.`이다. 같은
artifact 안에서 native `DesktopNode.Host.exe service-action repair-installed`는
SCM `PathName`에 `--batch-evidence-root`를 반영하고 `Service.Status=running`을
반환했다.

## 원인

`d7d5ba38ee1d4f74676477eb13701af65abca008`는 installed repair/configure에서
native service-action을 사용하도록 product wrapper를 수정했다. 하지만 wrapper의
후속 실행 경로가 native action의 final service state를 인식하지 못하고 기존
outer `service.start` step을 계속 실행했다.

Windows SCM에서 이미 실행 중인 service에 `sc.exe start`를 다시 호출하면 `1056`
already running이 반환된다. 이 exit code는 native repair가 실패했다는 신호가
아니라, 같은 repair operation 안에서 final state control이 중복된다는 신호다.

## 닫힘

`987beb51025a5aa926df7d9a905019b4d6d29705`는
`Plan.service.native_service_action`이 있을 때 outer `service.start`를 실행하지 않고
다음 결과를 기록하도록 수정했다.

```text
skipped: true
reason: native-service-action-controls-final-state
```

후속 `0.42.11-admin-smoke` package는
`docs/ga-ready/evidence/product-wrapper-native-repair-package-2026-05-13-04211.md`에서
PASS로 닫혔고, `0.42.9-admin-smoke -> 0.42.11-admin-smoke` package-pair와 04211 full
admin host mutation gate도 각각
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md`가
current claim을 소유한다.

## 후속 판단

- `0.42.10-admin-smoke`는 재실행 대상이 아니라 historical RCA record로 보존한다.
- 다음 package-pair candidate는 실제 product payload 변경이 생길 때
  `0.42.12-admin-smoke`로 연다.
- `origin/main` merge commit `14f56fd7348572e1757413657a68cd17c0aeca52` 기준
  post-merge package build는 새 payload 변경이 없으므로 보류한다.
- Public trusted signing, winget submission, external stable publication, public
  clean-host release claim은 ADR-0006 기준 계속 out-of-scope다.
