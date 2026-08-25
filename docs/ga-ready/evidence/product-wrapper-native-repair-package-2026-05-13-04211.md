# 제품 Wrapper Native Repair Package - 2026-05-13 0.42.11

```text
evidence_id: product-wrapper-native-repair-package-2026-05-13-04211
scope: product-wrapper-native-service-action-repair-installed
result: PASS
product_version: 0.42.11-admin-smoke
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

## 판정

`0.42.11-admin-smoke` product wrapper는 installed `RepairInstalled`와
`ConfigureInstalled`에서 legacy `sc.exe create` 경로 대신 설치된
`DesktopNode.Host.exe service-action configure-installed|repair-installed`를 호출한다.
따라서 기존 서비스가 이미 있을 때도 SCM `PathName`을 native host service-action이
권위 있게 재작성한다.

이 slice는 `RepairInstalled -BatchEvidenceRoot`가 서비스 `PathName`에
`--batch-evidence-root`를 반영하지 못하던 문제와, native service-action 이후
outer wrapper가 다시 `sc.exe start`를 호출해 `1056 already running`을 반환하던
문제를 함께 닫는다.

## 산출물

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260513-04211` |
| MSI | `PureCVisorDesktopNode-0.42.11-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1` |
| provenance commit | `987beb51025a5aa926df7d9a905019b4d6d29705` |
| payload aggregate SHA-256 | `dfc36900c1347cf51d380fac8aa2b399dbf2f8cd444ec3bfaecfc6d0d6fa8673` |
| product wrapper SHA-256 | `b116eb5619d7e94af1a866e7c067e4329f1762dfddcfdd6c5a42116514ab6989` |
| service host SHA-256 | `f9551362a3ccc511a1b160ed3cf402e286509a7bd814f7fc7e450a6ae967a816` |
| CLI SHA-256 | `035fc72365ffd29494d1be59ac49a46d23b403e95817954a01d206a7c8266d71` |
| TUI SHA-256 | `bc3fb9e1d565821e332b519e3a20f245bbbc6fbe246715380c0772e91faafd2a` |
| current-card artifact | `artifacts/installed-batch-evidence-current-card-20260513-04211` |

## 동작 증거

- Product wrapper `RepairInstalled -BatchEvidenceRoot` 실행:
  `artifacts/installed-batch-evidence-current-card-20260513-04211/product-repair-installed.json`
- 서비스 상태 확인:
  `artifacts/installed-batch-evidence-current-card-20260513-04211/service-status-after-product-repair.json`
- 요약:
  `artifacts/installed-batch-evidence-current-card-20260513-04211/summary.json`

요약 값:

- `service_path_has_batch_evidence_root=true`
- `wrapper_repair_used_native_service_action=true`
- `wrapper_repair_skipped_outer_start=true`
- `batch_evidence.status=available`
- `latest.batch_id=full-admin-host-mutation-gate-20260513-0429-04211`
- `installed_runtime_version=0.42.11-admin-smoke`
- `errors_count=0`

## 회귀 테스트

다음 Pester suite가 PASS다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

결과: `93` passed.

추가 focused test는 native service-action repair path와 outer start skip을 확인했다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -FullName '*MSI installed*','*repairs service configuration*','*native service-action*' -Output Detailed"
```

결과: `2` passed.

## 릴리스 경계

이 evidence는 internal/admin-smoke product wrapper와 installed service repair evidence다.
Public trusted signing, external stable publication, winget submission, public stable URL,
public clean-host release claim을 추가하지 않는다.
