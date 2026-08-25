# Admin-smoke package - 2026-05-16 0.42.23

```text
evidence_id: admin-smoke-package-2026-05-16-04223
result: PASS
version: 0.42.23-admin-smoke
package_build_decision: executed-0.42.23-admin-smoke
artifact_root: artifacts/admin-smoke-package-20260516-04223
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 Credential Manager default transition idempotence 수정을 포함한
`0.42.23-admin-smoke` internal admin-smoke MSI/package build를 기록한다. 이 build는
이미 설치된 서비스가 `--api-token-credential-target`을 사용하고 같은 token이
Credential Manager와 protected file에 존재하는 경우를 완료 상태로 인정한다.

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260516-04223/PureCVisorDesktopNode-0.42.23-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406` |
| provenance | `artifacts/admin-smoke-package-20260516-04223/PureCVisorDesktopNode-0.42.23-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260516-04223/PureCVisorDesktopNode-0.42.23-admin-smoke-windows-x64.publication.json` |
| provenance commit | `676b4177b10dc80209969066857bab6008ff2473` |
| build UTC | `2026-05-16T04:53:01.7567796Z` |
| payload aggregate SHA-256 | `ab22bb9b2f9525991b31e5c1233bbfd5d8610556f5bcddc52a9570e02e8c195d` |
| product wrapper SHA-256 | `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f` |
| host SHA-256 | `4f8189ea6950958c0f4c8d26298c9b95ab94c93582cacd2128fa4eaa04d03ed6` |
| CLI SHA-256 | `1fbf8b927b4c3f50dff668a6bb7aa52066cf4694fea8a8e21098e11768f791dc` |
| TUI SHA-256 | `bc949d566c057a4f24ef508ee6e0d376203ba186c2d17b2b82f13eb158c967ad` |
| signing mode | `AllowUnsignedDev` |

## Code Contract

- `CredentialManagerDefaultTransitionTreatsExistingCredentialManagerSourceAsIdempotent`
  test가 새 idempotence contract를 고정한다.
- 기존 protected-file source migration은 그대로 유지한다.
- 기존 credential-manager source가 target token과 일치하지 않거나 검증할 수 없으면
  `PCV_HOST_CREDENTIAL_MANAGER_TOKEN_SOURCE_MISMATCH`로 계속 실패한다.

이 package는 internal admin-smoke artifact다. Public trusted signing, 외부 stable
publication, winget submission, public stable installer URL은 `not-claimed`다.
