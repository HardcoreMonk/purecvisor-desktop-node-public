# ADR-0000: 제목

- 상태: 제안
- 날짜: YYYY-MM-DD
- 결정 마커: `DESKTOP_NODE_EXAMPLE_DECISION: example`

## 맥락

결정이 필요한 배경과 기존 제약을 설명한다.

## 결정

선택한 방향을 명확히 적는다.

## 근거

왜 이 선택이 현재 저장소 경계와 제품 gate에 맞는지 설명한다.

## 영향 범위

- 포함 경로:
- 제외 경로:
- 운영 또는 검증 영향:

## 대안

검토했지만 선택하지 않은 대안과 이유를 적는다.

## 검증 기준

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

## 관련 문서

- `docs/ADR_INDEX.md`
