# PureCVisor Desktop Node Phase 11 제품 런타임 승격 판단 설계

## 목적

Phase 11은 Desktop Node를 `spikes/purecvisor-desktop-node/`에 남은 격리 spike로 유지할지, 제품 런타임으로 승격할지 결정한다.

Phase 8, 9, 10은 installer 준비, Local API runtime policy, Web Console/CLI 사용성을 강화했지만, 아직 제품 배포 단위와 운영 복구 계약을 갖춘 런타임은 아니다. 따라서 이번 단계는 기능을 추가하지 않고 릴리스 경계와 승격 gate를 확정한다.

## 결정

```text
PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike
```

2026-04-25 기준 Desktop Node는 제품 런타임으로 승격하지 않는다. 계속 `spikes/purecvisor-desktop-node/**` 아래의 Windows Desktop Node spike로 유지한다.

이 결정의 직접 효과:

- Linux `purecvisorsd` 런타임은 변경하지 않는다.
- Single Edge Web UI/API 공개 표면과 Desktop Node Web Console/API를 공유하지 않는다.
- `spikes/purecvisor-desktop-node/**`를 Single Edge 출시 gate의 성공 조건으로 끌어올리지 않는다.
- Desktop Node 기본 검증 gate는 PowerShell 7/Pester와 gated Hyper-V integration으로 별도 유지한다.
- 제품 승격 전까지 signed release installer, updater, rollback, log collection, service recovery, version policy를 제품 계약으로 약속하지 않는다.

## 후속 결정

2026-04-26 Phase 12는 다음 결정을 추가했다.

```text
DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper
```

이 결정은 Phase 11의 `keep-spike`를 폐기하지 않는다. Desktop Node 전체 GA 제품 런타임 승격은 계속 보류하지만, 설치, 업데이트, 롤백, 로그 수집, 서비스 복구 gate 중 일부를 `packaging/windows-desktop-node/`의 Service-first 제품 후보 wrapper에서 먼저 해소한다. 기존 `spikes/purecvisor-desktop-node/{api,web,hyperv,service}` 구현은 계속 spike 원천으로 유지하고, wrapper가 필요한 자산을 제품 설치 루트로 복사한다.

2026-04-26 Phase 13은 다음 결정을 추가했다.

```text
DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper
```

이 결정은 제품 후보 service host를 WinSW executable/XML staging 기준으로 고정한다. Phase 13은 service start/stop/status, diagnostics, loopback static auth boundary, 관리자 closure smoke를 검증했지만, Desktop Node 전체 GA 승격 판단은 유지하지 않는다.

2026-04-27 Phase 14는 다음 결정을 추가했다.

```text
DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first
```

이 결정은 WiX MSI-first installer source, signing/provenance contract, unsigned dev MSI build, 관리자 install/uninstall smoke를 `packaging/windows-desktop-node/**` 경계에 추가한다. Signed release build, repair/`REMOVE_DATA=1` smoke, full updater, DPAPI 또는 Windows Credential Manager, Windows Event Log provider, LAN TLS 정책, 관리자 권한 integration gate 자동화는 Phase 14 이후 별도 결정으로 남긴다.

## 판단 근거

### 1. 배포 단위가 아직 없다

Phase 14 이후 현재 구현은 PowerShell module, static Web Console, CLI script, service command builder, WinSW product wrapper, WiX MSI source의 조합이다. unsigned dev MSI build와 install/uninstall smoke는 존재하지만, 서명된 installer release, versioned update policy, release artifact naming, code signing chain 운용은 아직 GA 계약이 아니다.

제품 런타임으로 승격하려면 최소한 다음을 먼저 정해야 한다.

- Windows Desktop Node 배포 단위
- 제품 버전 정책
- 설치 경로와 데이터 경로
- upgrade와 downgrade 가능 범위
- uninstall 시 남길 상태와 제거할 상태

### 2. 관리자 권한 통합 검증이 기본 gate가 아니다

기본 검증은 안전하게 실제 시스템 변경을 피한다. 실제 Hyper-V VM 생성, Windows service install/start/stop/delete, firewall rule 적용, token file ACL inspection은 gated integration 또는 수동 smoke로 분리되어 있다.

이 구조는 spike에는 적합하지만 제품 런타임 승격 기준으로는 부족하다. 제품 gate는 최소 한 개의 지원 Windows Hyper-V 호스트에서 다음을 반복 가능하게 검증해야 한다.

- 실제 VM create/start/poweroff/checkpoint lifecycle
- service install/start/status/stop/uninstall
- token file 생성과 ACL 결과
- LAN mode firewall rule 생성과 제거
- 실패 중단 후 재실행 복구

### 3. 설치, 업데이트, 롤백 정책이 없다

Phase 8은 token file 준비와 `icacls.exe` command builder를 만들었지만, installer 전체 정책은 아직 없다.

제품 승격 전 필요한 정책:

- 서명된 installer release 생성과 검증
- 기존 설치 감지
- config/job/event file migration
- service stop/start 순서
- 실패 시 rollback
- rollback 이후 service recovery
- uninstall과 data retention 옵션

### 4. 로그 수집과 서비스 복구가 제품 수준이 아니다

현재 Local API는 JSONL event log를 opt-in으로 쓸 수 있고, service packaging은 `sc.exe failure` command builder를 가진다. 하지만 제품 운영 기준의 log rotation, Windows Event Log provider/source lifecycle, crash dump, 진단 bundle, service recovery 검증은 아직 없다.

제품 승격 전 필요한 항목:

- log file 위치와 rotation
- Windows Event Log provider 또는 JSONL 유지 결정
- 진단 bundle 수집 명령
- service failure action 실제 적용 검증
- service recovery 이후 job store와 runtime policy 일관성 검증

### 5. 보안 경계가 spike 수준이다

현재 auth는 단일 bearer token과 token file 중심이다. LAN mode는 명시 opt-in이고 token source를 요구하지만, 제품 런타임의 장기 보안 정책으로는 부족하다.

제품 승격 전 검토해야 할 보안 항목:

- DPAPI 또는 Windows Credential Manager 사용 여부
- token rotation과 revoke
- LAN mode에서 TLS 또는 reverse proxy 전제
- multi-user auth 필요 여부
- Web Console static file serving의 cache/header 정책
- audit/event log의 민감정보 보존 정책

### 6. Web Console과 CLI가 설치 제품 표면이 아니다

Web Console은 static asset이고 browser-local job history를 사용한다. CLI는 Local API thin client script다. 둘 다 spike 사용자 경험으로는 충분하지만, 설치형 제품의 shortcut, PATH 등록, shell completion, update 호환성, VMConnect launch policy는 아직 없다.

## 승격 전 필수 gate

Desktop Node를 제품 런타임으로 승격하려면 다음 gate를 모두 통과해야 한다.

1. 새 ADR 또는 설계 문서로 제품 공개 표면과 저장소/릴리스 경계를 확정한다.
2. Windows Desktop Node 배포 단위와 version policy를 확정한다.
3. 서명된 installer release와 uninstall/repair/REMOVE_DATA flow evidence를 만든다.
4. update, rollback, config migration, service recovery 정책을 구현하고 검증한다.
5. token storage, LAN auth, log retention 보안 정책을 확정한다.
6. 실제 관리자 권한 integration suite를 지원 Windows Hyper-V host에서 통과한다.
7. Single Edge release gate와 Desktop Node release gate를 별도 문서와 CI job으로 분리한다.
8. 운영자 runbook, troubleshooting, log collection 절차를 작성한다.

## 포함 범위

Phase 11에 포함한다.

- keep-spike 결정 기록
- root Desktop Node spike README 추가
- PUBLIC_RELEASE_BOUNDARY, DEVELOPMENT_VERIFICATION_POLICY, DEVELOPER_INDEX 갱신
- 기존 Desktop Node MVP 설계의 현재 구현 상태와 roadmap 갱신
- root boundary Pester suite 추가
- follower queue에서 Phase 11 완료 처리

## 제외 범위

Phase 11에서 제외한다.

- Desktop Node 코드 이동
- Linux `purecvisorsd` 변경
- Single Edge Web UI/API 변경
- installer 구현
- updater/rollback 구현
- Windows Event Log provider 구현
- DPAPI/Windows Credential Manager 구현
- 관리자 권한 통합 검증 자동화

## 검증

문서 결정이 drift되지 않도록 root boundary suite를 둔다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

기존 Desktop Node 구현 회귀는 각 컴포넌트 suite를 유지한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

## 완료 기준

Phase 11은 다음을 만족하면 완료다.

- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`가 root README, 설계 문서, 공개 릴리스 경계에 남아 있다.
- Desktop Node 제품 승격에 필요한 signed installer release, 업데이트, 롤백, 로그 수집, 서비스 복구, 배포 단위, version policy gate가 문서화되어 있다.
- `spikes/purecvisor-desktop-node/**` 격리 규칙이 유지된다.
- Single Edge 릴리스 gate와 Desktop Node 검증 gate가 분리되어 있다.
- root boundary suite가 통과한다.
