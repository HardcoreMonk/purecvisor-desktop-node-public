# PureCVisor Desktop Node Phase 19 제품 승격 재판정 설계

## 목적

Phase 19는 Phase 11의 Desktop Node 제품 런타임 승격 판단을 현재 증거 기준으로 다시 판정한다.

Phase 12-18은 Service-first wrapper, WinSW service host, WiX MSI-first installer, DPAPI LocalMachine protected token file, JSONL first diagnostics, LAN preview security policy, manifest-first update/rollback/config migration을 차례로 추가했다. 특히 Phase 18은 관리자 update/rollback smoke까지 완료해 일부 제품화 gate를 닫았다. 2026-05-01 replacement slice 이후 기본 제품 service host는 `DesktopNode.Host.exe`지만, 이 Phase 19 문서는 당시 재판정 기준과 keep-spike 결론을 보존한다.

하지만 Desktop Node 전체를 GA 제품 런타임으로 승격하려면 서명된 release build, elevated MSI repair/uninstall/`REMOVE_DATA` smoke, 실제 Hyper-V lifecycle integration, release/version policy, 장기 운영 로그 증거가 모두 필요하다. Phase 19는 이 증거를 bucket별로 판정하고, 현재 결론을 문서와 root boundary test에 고정한다.

## 결정

```text
PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike
DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike
```

2026-04-29 기준 Desktop Node는 GA 제품 런타임으로 승격하지 않는다. Phase 19는 `keep-spike`를 유지한다.

이 결정은 Phase 12-18의 제품 후보 wrapper 작업을 되돌리지 않는다. 현재 `packaging/windows-desktop-node/**`는 Service-first/.NET service host/MSI/protected-token/diagnostics/LAN-security/update 배포 계층이고, Phase 13 WinSW 경계는 이력과 compatibility test로 보존한다. `spikes/purecvisor-desktop-node/**`는 계속 component 구현 원천과 검증 경계다. Linux `purecvisorsd`, Single Edge UI/API, Single Edge release gate와 연결하지 않는다.

## 재판정 원칙

Phase 19 판정은 구현량이 아니라 증거의 질을 기준으로 한다.

- 기본 Pester suite와 dry-run smoke만으로 GA 승격을 선언하지 않는다.
- 실제 Windows host를 변경하는 검증은 관리자 opt-in evidence가 있을 때만 충족으로 본다.
- `signed release`, `elevated MSI lifecycle`, `Hyper-V lifecycle`, `release/version policy`는 문서화만으로 충족 처리하지 않는다.
- 민감정보 redaction과 token 보호는 raw value가 manifest, command line, diagnostics, 문서에 남지 않는 테스트와 smoke 증거가 있을 때만 충족으로 본다.
- Single Edge와 Desktop Node의 저장소, release, CI gate 분리는 계속 독립 조건이다.

## 충족된 Gate

다음 항목은 Phase 12-18 증거 기준으로 제품 승격 전제 중 일부를 충족한다.

### Token storage

Phase 15는 제품 wrapper 기본 bearer token source를 DPAPI LocalMachine protected token file로 전환했다.

- 기본 protected token file: `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`
- 제품 service host command line은 raw token이 아니라 protected token file 경로를 전달한다. Phase 13 WinSW XML도 같은 원칙을 검증했고, 현재 `DesktopNode.Host.exe listen`도 `--api-token-protected-file`을 사용한다.
- Diagnostic bundle과 product manifest는 raw token, protected token blob, token hash를 redaction한다.
- Legacy raw token file은 migration/rollback compatibility 범위로 제한하고, `RemoveData` 삭제 대상에 포함한다.

### Diagnostics redaction과 JSONL first policy

Phase 16은 JSONL first diagnostics policy와 versioned diagnostic bundle manifest를 고정했다.

- `events.jsonl`, `install.jsonl`, service host logs를 1차 운영 진단 자료로 둔다.
- Diagnostic bundle은 `diagnostics-manifest.json`과 redacted artifact 목록을 포함한다.
- Host absolute source/product/data root와 token 계열 key는 bundle artifact에서 redaction한다.
- Windows Event Log source 등록은 기본 install/repair/diagnostics 경로에서 실행하지 않고 admin opt-in plan으로 둔다.

### LAN security policy

Phase 17은 Desktop Node LAN mode를 제품 기본값이 아니라 preview/admin opt-in으로 제한했다.

- 기본 product plan과 installed action은 loopback-only다.
- LAN mode는 `-AllowLan`과 explicit admin opt-in이 있을 때만 허용한다.
- Non-loopback static asset은 API route와 같은 bearer token 정책을 유지한다.
- TLS endpoint는 wrapper가 직접 제공하지 않고 reverse proxy 또는 외부 TLS terminator를 전제로 한다.
- Windows Firewall rule lifecycle은 installer 자동 적용이 아니라 explicit opt-in이다.

### Update, rollback, config migration 기본 구현

Phase 18은 manifest-first safe update/rollback/config migration을 제품 wrapper 경계에 구현했다.

- installed `product-manifest.json`을 product root 버전의 단일 진실로 둔다.
- 기본 update는 network updater가 아니라 local payload/source 기반 product wrapper action이다.
- rollback slot은 `DesktopNode.previous` 하나로 제한한다.
- config migration validation 실패와 service health check 실패는 previous root rollback 시도로 이어진다.
- Diagnostic bundle은 update policy, migration plan, rollback state artifact를 포함한다.
- 관리자 mutating update/rollback smoke는 기존 `0.14.0-dev` 설치를 `0.18.0-admin-smoke`로 update한 뒤 rollback해 service health와 diagnostics artifact를 확인했다.

## 부분 충족 Gate

다음 항목은 구조와 일부 smoke는 있지만 GA 승격 evidence로는 아직 부족하다.

### MSI source, build, provenance

Phase 14는 WiX MSI-first installer source와 build script, unsigned dev MSI build, provenance contract를 추가했다.

부분 충족 근거:

- MSI source가 `packaging/windows-desktop-node/installer/**` 아래에 있다.
- Program Files product file 설치와 product wrapper configure/repair/remove action 경계가 정리됐다.
- 기본 uninstall은 ProgramData를 보존하고, `REMOVE_DATA=1`은 data removal path로 분리됐다.

남은 증거:

- signed release build evidence
- elevated `msiexec /i` install smoke
- elevated repair smoke
- elevated uninstall smoke
- elevated `REMOVE_DATA=1` uninstall smoke

### Service-first와 WinSW wrapper

Phase 12/13은 product wrapper와 WinSW service host를 추가했다.

부분 충족 근거:

- Service host는 WinSW executable/XML staging 기준으로 고정됐다.
- Product wrapper는 install/status/rollback/uninstall/diagnostics action을 갖는다.
- Basic service health는 protected token file로 `/api/v1/runtime/policy`를 확인한다.

남은 증거:

- release build artifact에서 service install/start/stop/delete 반복 smoke
- service failure action 실제 적용과 recovery evidence
- service log retention이 장기 실행에서 유지되는 evidence

### Event Log 운영 정책

Phase 16은 JSONL first를 1차 정책으로 확정하고 Event Log는 opt-in plan으로 남겼다.

부분 충족 근거:

- Event Log source registration plan object와 admin smoke 절차가 있다.
- JSONL rotation/diagnostic bundle 정책은 구현과 테스트가 있다.

남은 증거:

- 제품 기본 정책을 JSONL first로 장기 유지할지, Windows Event Log writer/provider로 전환할지 최종 선택
- 선택한 정책의 장기 운영 evidence
- Event Log provider를 채택한다면 source lifecycle, writer, uninstall cleanup evidence

## GA 차단 Gate

다음 항목이 닫히기 전에는 Desktop Node를 GA 제품 런타임으로 승격하지 않는다.

1. **Signed release build evidence**
   - release channel MSI와 wrapper payload가 signing/provenance contract를 충족해야 한다.
   - signing secret, certificate private key, raw token 값은 repo나 diagnostics에 남지 않아야 한다.

2. **Elevated MSI lifecycle smoke**
   - 지원 Windows host에서 `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` uninstall을 반복 가능하게 검증해야 한다.
   - Program Files product root와 ProgramData data retention/remove semantics가 실제 결과로 확인되어야 한다.

3. **Hyper-V lifecycle integration evidence**
   - 실제 Hyper-V host에서 VM create/start/poweroff/checkpoint/remove lifecycle이 검증되어야 한다.
   - 실패 중단 후 재실행 복구와 job store 상태 일관성이 확인되어야 한다.

4. **Release/version policy**
   - Desktop Node release channel, version naming, artifact naming, upgrade/downgrade support, rollback compatibility 범위를 확정해야 한다.
   - `0.x-dev`, admin smoke version, signed release version의 의미가 분리되어야 한다.

5. **장기 운영 로그 정책 evidence**
   - JSONL first를 유지한다면 장기 rotation/retention 운영 증거가 필요하다.
   - Event Log provider로 전환한다면 provider/source lifecycle과 uninstall cleanup 증거가 필요하다.

6. **Release gate 분리**
   - Single Edge release gate와 Desktop Node release gate가 CI와 문서에서 독립적으로 유지되어야 한다.
   - Desktop Node 실패가 Linux Single Edge release를 막는 구조로 연결되면 안 된다.

## 문서와 Test 동기화 설계

Phase 19 구현은 제품 runtime code를 바꾸지 않는다. 변경은 설계 문서, active documentation, root boundary/documentation sync tests에 제한한다.

갱신 대상:

- `docs/DEVELOPER_INDEX.md`: Phase 19 재판정 spec 진입점을 추가한다.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: Phase 19 문서/test 변경 검증 기준을 추가한다.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`: Phase 19가 `keep-spike`를 재확인했음을 기록한다.
- `follower.md`: 다음 우선순위를 Phase 19 이후 남은 GA 차단 gate 중심으로 갱신한다.
- `spikes/purecvisor-desktop-node/README.md`: Phase 19 재판정과 충족/부분 충족/차단 gate를 요약한다.
- `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`: Phase 19 spec 존재, decision marker, GA 차단 gate, high-level docs 동기화를 검증한다.
- `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`: stale Phase 19 backlog wording과 high-level docs pass count 복제를 막는다.

## 검증 기준

Phase 19 문서/test 갱신 후 최소 검증은 다음과 같다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

문서 변경이 product wrapper, installer, API/service/CLI/web/hyperv implementation을 건드리지 않으면 component suite는 조건부다. 다만 Phase 19 이후 실제 GA 차단 gate를 닫는 구현에서는 해당 component suite와 관리자 opt-in smoke를 별도 실행해야 한다.

## 포함 범위

Phase 19에 포함한다.

- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 재판정
- `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike` 결정 기록
- Phase 12-18 evidence를 충족/부분 충족/GA 차단 gate로 분류
- active docs와 root boundary suite 동기화
- Phase 19 이후 follow-up queue 정리

## 제외 범위

Phase 19에서 제외한다.

- runtime code 변경
- 제품 디렉터리 구조 이동
- signed release build 실행
- elevated MSI lifecycle smoke 실행
- 실제 Hyper-V VM lifecycle integration 실행
- Event Log writer/provider 구현
- release CI job 또는 GitHub Actions workflow 추가
- Linux `purecvisorsd` 또는 Single Edge 공개 UI/API 변경

## 완료 기준

Phase 19는 다음을 만족하면 완료다.

- Phase 19 spec이 `keep-spike` 재판정과 `evidence-first-keep-spike` decision marker를 포함한다.
- active docs가 Phase 19 이후에도 Desktop Node GA 승격 보류 상태를 같은 방식으로 설명한다.
- 충족/부분 충족/GA 차단 gate가 문서에서 모순 없이 정리되어 있다.
- root boundary/documentation sync suite가 Phase 19 상태를 검증한다.
- `git diff --check`가 통과한다.
