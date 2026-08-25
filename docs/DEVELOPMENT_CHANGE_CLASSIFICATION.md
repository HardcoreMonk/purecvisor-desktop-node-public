# Desktop Node 개발 변경 S/M/L 분류

이 문서는 개발 변경의 최소 위험 등급과 검증 레인을 결정하는 단일 정책이다. 활성 제품 표면은
Web Console과 PCVCLI이며 TUI는 대상이 아니다. 호출자가 낮은 등급을 지정해도
`Resolve-PcvDevelopmentChangeTier`가 변경 경로의 최소 등급을 적용한다.

| 등급 | 범위 | 필수 기록 | 최소 검증 |
| --- | --- | --- | --- |
| `S` | 한 내부 모듈의 국소 구현·테스트 변경. API/CLI/Web 계약, 패키징, 설치, 보안, 현재 증거, 호스트 mutation, 공개 배포 경계를 바꾸지 않는다. | 변경 이유와 focused test 결과 | `Fast` |
| `M` | API/CLI/Web 비변경 계약, 일반 패키징 또는 둘 이상의 모듈에 걸친 변경. 실제 설치나 호스트 mutation은 수행하지 않는다. | 짧은 설계 기록, 영향 계약, 롤백 방법 | `Full` |
| `L` | installer lifecycle, host mutation runner, 보안 정책, current evidence anchor, public release boundary, signing/publication 변경. | 설계와 구현 계획, operational evidence 필요 여부, 승인·롤백 경계 | `Release` |

## 자동 최소 등급

- `packaging/windows-desktop-node/installer/**`는 `installer-lifecycle`로 `L`이다.
- host/OS mutation runner와 대응 테스트는 `host-mutation-boundary`로 `L`이다.
- ADR-0003/0009/0010 및 보안·credential·token·TLS·trust policy는
  `security-policy-boundary`로 `L`이다.
- `current-evidence.json`, schema와 생성 대상 6종은 `current-evidence-anchor`로 `L`이다.
- public release/distribution, signing 또는 publication 경로는 `L`이다.
- `DesktopNode.Api`, `DesktopNode.Cli`, Web API/contract/client/auth 경로는
  `api-cli-web-contract`로 최소 `M`이다.
- 그 밖의 `packaging/windows-desktop-node/**`는 `packaging-contract`로 최소 `M`이다.
- 서로 다른 source/Web/packaging 영역을 함께 바꾸면 `cross-module-change`로 최소 `M`이다.
- 분류되지 않은 경로는 근거 없이 `L`로 주장하지 않는다. 대신 검증 레인만 `Full`로 올린다.

호출자가 지정한 등급보다 자동 최소 등급이 높으면 자동 등급을 사용한다. `M`은 `Fast`를
`Full`로, `L`은 `Fast` 또는 `Full`을 `Release`로 승격한다. 결과 JSON은
`requested_change_tier`, `change_tier`, `tier_reasons`, `requested_lane`, `effective_lane`을
모두 보존한다.

```powershell
& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Fast -ChangeTier S `
  -ChangedPath @('src/DesktopNode.Core/InternalHelper.cs') `
  -ArtifactRoot artifacts/development-verification-fast
```

`Release` 레인은 비변경 preflight다. 이 분류는 package build, 설치본 변경, 서비스·Hyper-V·
firewall·trust store·Event Log mutation 권한을 부여하지 않는다. 실제 mutation과 public
publication은 기존의 별도 명시 승인 및 evidence 절차를 계속 요구한다.
