# Development feedback loop code-level evidence — 2026-07-16

## 판정

`PASS` — Fast/Full/Release 선택기와 실행기, Batch Supervisor process/time seam,
installer build in-process boundary, 네-job Full-lane CI 계약이 비변경 검증으로 닫혔다.

- `host_mutation_performed=false`
- `package_build_performed=false`
- `installed_product_changed=false`
- `public_trusted_signing=not-claimed`
- `external_stable_publication=not-claimed`
- operational anchor는 계속 `0.42.64-admin-smoke` /
  `full-admin-host-mutation-gate-20260715-04264`다.

## 구현 소유권

- lane selector: `95821a7bba92e53f781a73cf7caee6e371f74b0e`
- suite runner/thin entrypoint: `3e6e34020a8bd501b8c1cb07cae7801d45778c95`
- Batch Supervisor seam: `389eac553fe40a291cc0e18008b69166c3408822`
- installer in-process module/thin wrapper: `28003312a12ab0031748c0fcd46006641a4d846f`

## 동일 host 계측

기준은 이 변경 전 같은 Windows host에서 수집한 값이다.

| Gate | 변경 전 | 변경 후 | 결과 |
| --- | ---: | ---: | --- |
| .NET solution | 591 pass / 20.4초 | 591 pass / 29.25초 | PASS |
| Web type/static/parity | pass / 11.2초 | pass / 5.26초 | PASS |
| Packaging Pester | 385 pass / 141.9초 | 393 pass / 119.63초 | PASS |
| Installer/Web Pester | 94 pass / 59.3초 | 97 pass / 15.20초 | PASS |
| 합산 local gate | 232.8초 | 169.34초 | 27.3% 단축 |
| 합산 Pester | 201.2초 | 134.83초 | 33.0% 단축 |

Pester 범위는 479건에서 490건으로 늘었지만 합산 시간은 66.37초 감소했다. 설계 목표인
30% 이상 단축을 충족한다.

### 집중 계측

- `PcvBatchSupervisor.Tests.ps1`: 40.5초 기준에서 27/27 PASS, 12.22초와
  12.44초 반복 실행. 실제 CLI child process 1건과 실제 process-start failure 1건은
  유지했다.
- Installer Plan: 21/21 PASS, 6.68초.
- Installer Signing: 6/6 PASS, 2.68초. Plan+Signing 합계 9.36초로 20초 목표를
  충족한다.
- Installer 전체: 49/49 PASS, 12.48초. Wrapper JSON/exit/redaction 통합 3건을 포함한다.
- Full PlanOnly: effective lane `Full`, planned suite `7`, host mutation 없음.

## 검증 명령

```powershell
dotnet test src/DesktopNode.sln -c Release --no-restore
npm test --prefix web
npm run verify:parity --prefix web
Invoke-Pester -Path packaging/windows-desktop-node/tests -Output Normal
Invoke-Pester -Path @('packaging/windows-desktop-node/installer/tests','web/tests') -Output Normal
git diff --check
```

CI는 기존 `dotnet-tests`, `web-tests`, `packaging-pester`, `installer-web-pester` 네 독립
job을 유지한다. Packaging job에서 Full PlanOnly orchestration 계약만 추가 검증하며 suite를
직렬 재실행하지 않는다. Workflow에는 MSI/service/Hyper-V/firewall/trust-store/Event Log
mutation 또는 package/publication 명령이 없다.

## 계측 무효 시도 기록

최초 로컬 .NET/Web 병렬 계측은 실행 도구를 중단하는 과정에서 370개의 `dotnet.exe` child가
남아 무효 처리했다. 생성 시각이 2026-07-16 16:11:55–16:12:05인 해당 child만 식별해 모두
정리했고 `remaining=0`을 확인했다. 이후 .NET을 단독 승인 환경에서 재실행해 591건 PASS와
새 잔여 `dotnet` process `0`을 확인했으며, 위 표에는 이 유효한 단독 계측만 사용했다.

## 범위 제한

이 문서는 code-level development feedback evidence다. 설치본 수명주기, 실제 Hyper-V VM,
관리자 host mutation, trusted public signing 또는 외부 stable publication의 새 증거가 아니다.
