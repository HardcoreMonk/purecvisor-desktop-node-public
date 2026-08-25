# Admin-smoke package - 2026-05-16 0.42.24

```text
evidence_id: admin-smoke-package-2026-05-16-04224
result: PASS
version: 0.42.24-admin-smoke
package_build_decision: executed-0.42.24-admin-smoke
artifact_root: artifacts/admin-smoke-package-20260516-04224
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 Runtime/API `current_evidence` rollup을 포함한
`0.42.24-admin-smoke` internal admin-smoke MSI/package build를 기록한다. 이 build는
`GET /api/v1/ops/summary`가 기존 `batch_evidence`를 보존하면서
`runtime-api-current-evidence-rollup-v1` current evidence card를 함께 반환하도록 만든
product payload다.

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260516-04224/PureCVisorDesktopNode-0.42.24-admin-smoke-windows-x64.msi` |
| MSI SHA-256 | `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e` |
| provenance | `artifacts/admin-smoke-package-20260516-04224/PureCVisorDesktopNode-0.42.24-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260516-04224/PureCVisorDesktopNode-0.42.24-admin-smoke-windows-x64.publication.json` |
| provenance commit | `b974d6b541423f2e4160f726f96155b16f105e9d` |
| build UTC | `2026-05-16T08:46:28.2693692Z` |
| payload aggregate SHA-256 | `6820f0a4f7ae1e904eb97e555eb714edf7f86e70ab4c304be3bfe38f2b9732be` |
| product wrapper SHA-256 | `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f` |
| host SHA-256 | `e396223284a78816b78d36d4fc4509ebee33ae22dd8e08d491e1ab7f4a915447` |
| CLI SHA-256 | `5f10ba11b07672ba8bf1d2d122a0e11949d9dbe07b6d86aca134349489028477` |
| TUI SHA-256 | `b4f2354848fcd5ad1459afcb745de764bad8c9da3ae9afee8e82a85a751aa1a4` |
| signing mode | `AllowUnsignedDev` |

## Code Contract

- `OpsSummaryIncludesBatchEvidenceWhenRootIsConfigured`는 `current_evidence`가
  `runtime-api-current-evidence-rollup-v1` contract key를 반환하고 full admin host
  mutation latest status를 `batch_evidence.latest`에서 파생함을 고정한다.
- Web Console served bundle은 Dashboard와 Evidence view에 `Current evidence` card를
  추가하고, CLI/TUI가 보는 installed `ops summary`와 같은 current-card source를
  사용한다.
- Public boundary와 Manual admin closed package-pair anchor는 문서 evidence로만
  연결한다. 0.42.24 package build 자체는 public trusted signing, 외부 stable
  publication, winget submission, public stable installer URL을 claim하지 않는다.

이 package는 internal admin-smoke artifact다. Public trusted signing, 외부 stable
publication, winget submission, public stable installer URL은 `not-claimed`다.
