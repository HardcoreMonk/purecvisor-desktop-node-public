# ADR-0002: Release/version 정책과 installer artifact channel 계약

- 상태: 적용 중
- 날짜: 2026-04-29
- 결정 마커:
  - `DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike`
  - `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`
  - `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`

## 맥락

Phase 22는 Desktop Node release channel, version naming, artifact naming, upgrade/downgrade, rollback compatibility 정책을 정의했다. 후속 구현은 installer `build.ps1`와 provenance schema에 `windows-x64` MSI/provenance/hash sidecar naming, build plan/provenance `product.release_channel`, unsigned RC/stable 차단을 반영했다.

2026-05-07 후속 구현은 같은 artifact contract에 publication descriptor sidecar를 추가했다. 이 descriptor는 internal artifact descriptor이며 public trusted signing, external stable publication, Burn/MSIX/winget publication을 주장하지 않는다.

이 결정은 이미 구현된 release/version policy와 installer artifact/channel contract를 현재 Desktop Node 결정으로 채택한다. 제품 런타임 승격 판단은 2026-05-05 ADR-0004가 대체하며, 외부 stable 공개 배포는 내부 전용 서비스 범위 밖이다.

## 결정

Desktop Node는 Phase 22 release/version policy를 현재 적용 결정으로 채택한다.

채택 범위는 다음과 같다.

- `dev` channel은 `0.x-dev` 계열 개발자 dry-run, Pester, unsigned MSI build 전용이며 배포하지 않는다.
- `admin-smoke` channel은 관리자 권한 smoke evidence 수집용이며 stable 또는 rc 승격을 뜻하지 않는다.
- `rc` channel은 signed release candidate evidence 수집용이며 `RequireSigned`가 필수이고 GA가 아니다.
- `stable` channel은 signed stable 후보 이름을 예약하지만, stable 발행 승인과 release evidence가 닫히기 전에는 공개 stable release가 아니다.
- MSI artifact는 `PureCVisorDesktopNode-<version>-windows-x64.msi` naming을 사용한다.
- Provenance artifact는 `PureCVisorDesktopNode-<version>-windows-x64.provenance.json` naming을 사용한다.
- MSI hash sidecar는 `PureCVisorDesktopNode-<version>-windows-x64.msi.sha256` naming을 사용한다.
- Publication descriptor sidecar는 `PureCVisorDesktopNode-<version>-windows-x64.publication.json` naming을 사용한다.
- Build plan과 provenance는 `product.release_channel`을 기록한다.
- Build plan, provenance, publication descriptor는 public trusted signing과 external stable publication을 claimed 상태로 기록하지 않는다.
- `rc`와 `stable` version은 unsigned `AllowUnsignedDev` build를 거부하고 `RequireSigned` signing mode만 허용한다.
- `RequireSigned` build provenance는 `signing_trust_model`을 기록한다. 허용값은 `LocalTest`, `InternalEnterprise`, `PublicTrusted`이며 내부 서비스 운영 evidence는 ADR-0003에 따라 `InternalEnterprise`를 사용한다.
- Phase 18 manifest-first safe update/rollback/config migration boundary를 유지한다.

## 채택하지 않는 것

이 ADR은 다음을 채택하지 않는다.

- Stable 공개 release 또는 일반 사용자 대상 stable 발행
- Public trusted signing evidence 완료 주장. 2026-04-30 release approval evidence는 draft-ready gate closure로만 기록하고 public trusted signature 또는 stable publication으로 주장하지 않는다. 같은 날 local self-signed/root trust 우회는 `CurrentUser` 신뢰 저장소에서 test certificate 검증을 통과시킨 host-local workaround이며 public trusted signing evidence가 아니다.
- Public trusted signed stable MSI install/repair/uninstall/`REMOVE_DATA=1` lifecycle evidence 완료 주장
- Public trusted/stable signing 흐름과 묶인 Hyper-V product-flow 또는 signed/elevated product install 흐름 evidence 완료 주장
- 내부 서비스용 `InternalEnterprise` signing evidence를 외부 public trusted evidence로 해석하는 것
- JSONL first diagnostics에서 Windows Event Log writer/provider 전환
- Linux Single Edge release gate 또는 `purecvisor-single` version policy 변경

## 근거

Release/version policy와 installer artifact/channel contract는 이미 code/docs/test에 반영되어 있고, 현재 적용 결정의 출처인 ADR index에서 추적할 필요가 있다. `0.23.8-rc.1` signed RC MSI evidence는 local test certificate로 수집됐고, 2026-04-30 후속 evidence에서 elevated MSI lifecycle, Hyper-V product-flow, release approval/signing preflight, 운영/Event Log source lifecycle은 draft-ready 기준으로 기록됐다. 이후 `artifacts/p0-local-root-trust-workaround-20260430-2120`에서 test signer를 `Cert:\CurrentUser\Root`와 `Cert:\CurrentUser\TrustedPublisher`에만 추가해 Authenticode `Valid`, SignTool verify exit `0`을 확인했다. 2026-05-01 관리자 opt-in hardening evidence는 service/Hyper-V/firewall/Event Log/LAN/TLS preview 운영 smoke를 추가로 닫았고, `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`은 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` MSI lifecycle과 product-wrapper update/rollback/config migration smoke를 추가로 닫았다. 이후 `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`에서 internal Root/leaf 기반 `0.23.10-rc.1` `RequireSigned` MSI build, Authenticode `Valid`, SignTool verify exit `0`, elevated MSI lifecycle PASS, boot time unchanged를 기록했다. 이 internal evidence는 public trusted signing이나 외부 stable publication evidence가 아니다. local test `RequireSigned` build는 developer/test host workaround이며, internal enterprise build는 내부 서비스 운영용 trust model evidence다. 둘 다 public trusted signing, 외부 stable publication, GA 승격 근거로 쓰지 않는다.

따라서 이 ADR은 policy와 contract를 현재 적용 결정으로 승격한다. Stable channel은 이름과 signing 조건을 정의한다. 내부 서비스 stable release/update/rollback은 ADR-0003의 internal trust evidence와 별도 운영 승인으로 가능하며, 외부 public stable publication은 현재 내부 전용 서비스 범위 밖이다.

## Stable/GA 개시 조건

Internal stable publication 또는 제품 런타임 승격을 다시 판단하려면 최소한 다음 입력이 먼저 닫혀야 했다.

- signing trust model: 내부 서비스 운영은 ADR-0003의 internal Root/leaf `InternalEnterprise`, 외부 배포는 별도 ADR에서 public CA 또는 Azure Trusted Signing 같은 `PublicTrusted`
- stable artifact 결정: stable version/tag, release notes, GitHub Release 또는 다른 publication target, signed MSI/provenance/SHA-256
- stable lifecycle evidence: selected trust model로 signed stable MSI install/repair/uninstall/`REMOVE_DATA=1`, update compatibility, rollback/config migration evidence
- GA decision evidence: `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`을 채택하는 ADR-0004, public release boundary 변경, 운영 지원 범위

## 영향 범위

- 포함 경로:
  - `packaging/windows-desktop-node/installer/**`
  - `packaging/windows-desktop-node/**`
  - `archive/spikes/purecvisor-desktop-node/**`
  - `docs/**`
- 제외 경로:
  - Linux `purecvisor-single`
  - Linux `purecvisorsd`
  - KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- 운영 또는 검증 영향:
  - 문서/테스트 결정 채택은 host mutation이 아니다.
  - `RequireSigned` build, elevated MSI lifecycle, Hyper-V lifecycle smoke는 계속 관리자 opt-in gate다.
- Stable 발행은 selected trust model approval, signed/elevated evidence, update compatibility evidence가 닫힌 뒤 별도 판단한다. 외부 public stable publication은 현재 scope 밖이며, 별도 ADR 없이는 주장하지 않는다.

## 대안

### Phase 22 spec/plan에만 유지

선택하지 않는다. Installer artifact/channel contract가 이미 구현되어 현재 적용 결정 index에서 찾을 수 있어야 한다.

### ADR-0001 대체

선택하지 않는다. ADR-0001의 저장소 경계와 Phase 19 evidence-first keep-spike 이력은 여전히 유효하다. Phase 22 결정은 release/version policy를 추가로 채택하는 좁은 결정이다.

### Stable release 또는 GA 승격과 함께 채택

선택하지 않는다. 2026-04-30 draft-ready evidence는 닫혔지만 public trusted signing evidence, stable publication, GA 제품 런타임 승격은 당시 별도 판단이었다. 현재 제품 런타임 승격은 ADR-0004가 소유하고, public trusted signing과 외부 publication은 내부 전용 서비스 범위 밖이다.

## 검증 기준

문서 결정 채택 변경 후:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Installer contract가 바뀌면 추가로 실행한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
- `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy-design.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase22-release-version-policy.md`
- `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md`
- `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`
- `packaging/windows-desktop-node/README.md`
- `packaging/windows-desktop-node/installer/README.md`
