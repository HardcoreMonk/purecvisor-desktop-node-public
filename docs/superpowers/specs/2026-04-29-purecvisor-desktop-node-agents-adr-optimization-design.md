# PureCVisor Desktop Node AGENTS/ADR 최적화 설계

## 목적

이 작업은 `purecvisor-desktop-node` 독립 Windows 저장소의 에이전트 작업 지침과 설계 결정 기록 방식을 정리한다.

현재 `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, phase roadmap, phase spec이 같은 결정 마커를 반복한다. 반복 자체는 안전장치가 될 수 있지만, 장기적으로는 어떤 문서가 결정의 단일 진실인지 불명확해진다. 특히 이 저장소는 Linux `purecvisor-single`에서 분리된 이후 `docs/adr/` 체계가 아직 없어서, phase spec과 README가 ADR 역할을 부분적으로 대신하고 있다.

목표는 Desktop Node 전용 경량 ADR 체계를 추가하고, `AGENTS.md`는 에이전트가 반드시 따라야 하는 경계, 진입점, 검증 원칙에 집중하게 만드는 것이다.

## 결정

```text
DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index
```

`purecvisor-desktop-node`는 Linux 저장소의 ADR을 복사하지 않는다. Desktop Node 전용 `docs/ADR_INDEX.md`와 `docs/adr/`를 새로 만들고, 이미 확정된 Desktop Node 결정 마커를 현재 적용 상태 중심으로 연결한다.

첫 ADR은 저장소 분리, 제품 승격 보류, Phase 12-19 evidence-first gate를 하나의 현재 상태 결정으로 묶는다. 이후 새 phase가 설계 선택을 바꾸면 phase spec만 추가하지 않고 ADR index도 함께 갱신한다.

## 문서 구조

추가할 문서는 다음과 같다.

- `docs/ADR_INDEX.md`
  - Desktop Node ADR 진입점이다.
  - 현재 적용 중인 ADR, superseded 여부, 관련 phase spec을 표로 정리한다.
  - 결정 마커 목록을 한 곳에 모은다.
- `docs/adr/0000-template.md`
  - Desktop Node ADR 작성 양식이다.
  - 상태, 날짜, 결정, 근거, 영향 범위, 검증 기준, 관련 문서를 포함한다.
- `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
  - `DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`
  - `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
  - `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`
  - Phase 12-18의 충족/부분 충족/GA 차단 gate
  - Linux `purecvisor-single`과 release gate 분리

기존 phase spec은 역사와 상세 설계의 원천으로 유지한다. ADR은 phase spec을 대체하지 않고, 현재 적용되는 결정의 짧은 진입점 역할을 한다.

## AGENTS.md 정리

`AGENTS.md`는 다음 원칙으로 줄인다.

- 저장소 경계는 유지하되 장황한 phase 설명은 ADR index로 연결한다.
- 문서 진입점에 `docs/ADR_INDEX.md`와 `docs/adr/`를 추가한다.
- 결정 마커는 핵심 3개와 `DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index`만 둔다.
- 검증 명령과 작업 원칙은 유지한다.
- phase별 상세 결정은 `docs/ADR_INDEX.md`, phase roadmap, 관련 spec으로 위임한다.

`AGENTS.md`는 숫자와 긴 이력의 단일 진실이 아니라, 에이전트가 작업 전에 봐야 할 문서와 절대 넘지 말아야 할 경계를 알려주는 파일로 둔다.

## Active docs 동기화

다음 active docs를 갱신한다.

- `docs/DEVELOPER_INDEX.md`
  - ADR index 진입점을 추가한다.
  - 저장소 결정 섹션에서 ADR index를 현재 결정 단일 진실로 명시한다.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - 공개 릴리스 경계와 ADR index의 관계를 명시한다.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - AGENTS/ADR 문서 변경 시 root documentation suite와 `git diff --check`를 최소 검증으로 둔다.
- `spikes/purecvisor-desktop-node/README.md`
  - 필요하면 ADR index를 참조하되 phase별 긴 기록을 불필요하게 늘리지 않는다.

`GUIDE.md`는 사용자 상위 가이드 성격이므로 ADR 링크가 자연스러운 경우에만 추가한다. 문서 링크가 반복되어 가독성을 해치면 `DEVELOPER_INDEX.md`에만 둔다.

## 테스트 설계

문서 최적화 후 root documentation suite가 다음을 검증한다.

- `docs/ADR_INDEX.md`가 존재한다.
- `docs/adr/0000-template.md`와 첫 ADR이 존재한다.
- ADR index가 핵심 결정 마커를 포함한다.
- `AGENTS.md`가 ADR index를 문서 진입점으로 노출한다.
- `DEVELOPER_INDEX.md`와 `PUBLIC_RELEASE_BOUNDARY.md`가 ADR index를 참조한다.
- Desktop Node는 계속 Linux `purecvisor-single`, `purecvisorsd`, Single Edge release gate와 분리되어 있다.

검증 명령은 다음을 최소 기준으로 한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

## 포함 범위

이번 작업에 포함한다.

- Desktop Node 전용 ADR index와 ADR 디렉터리 추가
- ADR template 추가
- 첫 ADR 추가
- `AGENTS.md`를 ADR/문서 진입점 중심으로 최적화
- active docs의 ADR 링크 동기화
- root documentation/boundary tests 갱신

## 제외 범위

이번 작업에서 제외한다.

- Linux `purecvisor-single` ADR 복사
- phase 11-19 전체를 개별 ADR로 대량 분해
- runtime code 변경
- installer, service, API, CLI, web, Hyper-V 구현 변경
- release CI 또는 GitHub Actions workflow 추가
- signed build, elevated MSI smoke, 실제 Hyper-V integration 실행

## 완료 기준

완료 상태는 다음과 같다.

- Desktop Node의 현재 결정 진입점이 `docs/ADR_INDEX.md`로 생긴다.
- `AGENTS.md`는 ADR index를 가리키고 에이전트 경계/검증 규칙 중심으로 정리된다.
- 핵심 결정 마커는 ADR index와 첫 ADR에서 확인할 수 있다.
- active docs가 ADR index를 모순 없이 참조한다.
- root documentation suite와 `git diff --check`가 통과한다.
