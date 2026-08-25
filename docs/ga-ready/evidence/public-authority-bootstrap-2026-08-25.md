# Public authority bootstrap 2026-08-25

evidence_id: `public-authority-bootstrap-2026-08-25`
contract: `purecvisor-desktop-node-public-authority-bootstrap-v1`
result: `LOCAL_SOURCE_GATE_PASS`
phase: `sanitized-source-pre-parentless-export`
operational_version: `0.42.74-admin-smoke`
source_commit: `[private-source-commit]`
source_history_exported: `false`
provider_repository_created: `false`
provider_seed_pushed: `false`
visibility_mutation_performed: `false`
branch_protection_installed: `false`
host_mutation_performed: `false`
package_candidate_created: `false`
public_trusted_signing: `false`
external_stable_publication: `false`

## 결과

공개 source authority 후보의 committed tree는 parentless export 전에 로컬 source gate를 통과했다.
검증은 관리자 권한, 서비스/호스트 변경, MSI 실행, 실제 VM 변경 없이 수행했다. 원본 비공개
저장소의 object/ref/provider 데이터와 사용자 소유 dirty main은 export 대상에 포함하지 않는다.

| gate | 결과 |
| --- | --- |
| `.NET` restore + Release build | `PASS`; warning `0`, error `0` |
| `.NET` solution tests | `1451/1451 PASS`; skipped `0` |
| Web install/tests/parity | `PASS`; audit vulnerability `0` |
| Web contract replacement | `50/50 PASS` |
| Repository-owned public-safety tests | `20/20 PASS` |
| Repository-owned current-tree scan | findings `0` |
| Official Gitleaks current-tree scan | findings `0` |
| Complete legacy Pester reference | `627/627 PASS`; failed/skipped/not-run `0/0/0` |
| Fixed-diff public-safety review | unresolved P0/P1 `0` |

Repository-owned safety report SHA-256:
`603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd`.

Official Gitleaks version is `8.30.1`. Its empty JSON report SHA-256 is
`37517e5f3dc66819f61f5a7bb8ace1921282415f10551d2defa5c3eb0985b570`.
The downloaded scanner archive SHA-256 is
`d29144deff3a68aa93ced33dddf84b7fdc26070add4aa0f4513094c8332afc4e`.

## Sanitization accounting

- Identity/provider-text batch: files `77`, identity replacements `28`, provider replacements `133`.
- Observed private-network evidence batch: files `75`, address replacements `107`, hostname replacements
  `1`.
- Static credential-shaped signing fixtures were replaced with runtime construction while preserving the
  same test input and assertion behavior.
- Exact old provider identifiers, observed private endpoints, personal profile paths, and unresolved
  secret/private-key/token material remaining in the candidate tree: `0`.
- Accepted documentation redaction token: `[redacted-private-endpoint]`.

Sanitization does not rewrite an observed failure as PASS. Synthetic source/test addresses are explicitly
marked and classified by the repository-owned verifier; operational evidence uses redaction instead.

## Legacy reference fixture boundary

The full legacy reference run discovered `627` tests and completed in `193075 ms`. Two frozen-reader tests
require a historical ignored executable that is not tracked. The local source gate used a read-only copy
with SHA-256 `95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83` only as a test input.
The fixture is excluded from `git archive`, the parentless source root, provider seed, release, and package.
This fingerprint is a non-reversible artifact identity, not a publication claim.

## RED/GREEN record

The public-safety verifier was implemented through observed RED/GREEN slices covering API existence,
policy categories, CLI/boundary/symlink behavior, binary/media classification, documentation path parsing,
synthetic schema identifiers, and required authority documents. The final focused suite is `20/20 PASS`;
raw candidate values and scanner matches are intentionally not stored in this repository.

## Pending provider/root fields

The following remain deliberately pending at this checkpoint:

- parentless root commit/tree identity and Git isolation readback;
- post-export full local gate;
- private provider repository creation and exact-main seed push;
- seed CI run/job identities and provider-side safety audit;
- fresh one-way approval for `PUBLIC` visibility;
- main protection and cutover pull request.

Public visibility is not authorized by this evidence. Returning a repository to private after publication
cannot recall clones or cached copies, so the visibility mutation remains a separate explicit checkpoint.
