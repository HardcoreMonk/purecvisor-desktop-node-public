# Public authority bootstrap 2026-08-25

evidence_id: `public-authority-bootstrap-2026-08-25`
contract: `purecvisor-desktop-node-public-authority-bootstrap-v1`
result: `PUBLIC_BASELINE_DRAFT_PR_PASS`
phase: `draft-cutover-pr-open`
operational_version: `0.42.74-admin-smoke`
source_commit: `[private-source-commit]`
source_history_exported: `false`
parentless_root_commit: `c76a831be168a6b5aa122a91df3588a0c5e67f0d`
parentless_root_tree: `7bc239d9278e89758659c37713100f279f74ada7`
provider_repository_created: `true`
provider_seed_pushed: `true`
provider_visibility: `PUBLIC`
visibility_mutation_performed: `true`
private_vulnerability_reporting_enabled: `true`
branch_protection_installed: `true`
cutover_branch_pushed: `true`
cutover_pull_request: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/pull/1`
host_mutation_performed: `false`
package_candidate_created: `false`
public_trusted_signing: `false`
external_stable_publication: `false`

## 결과

정제된 committed tree를 부모 없는 단일 Git root로 export하고 새 public authority의 `main`에
seed했다. Exact root CI, provider audit, 별도 one-way PUBLIC 승인, main 보호와 익명 clone 검증을
통과했다. 검증은 관리자 권한, 서비스/호스트 변경, MSI 실행, 실제 VM 변경 없이 수행했다. 원본
비공개 archive의 object/ref/provider 데이터와 사용자 소유 dirty main은 export 대상에 포함하지
않았다.

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
| Parentless root isolation | parent `0`; commit `1`; branch `main` only; tag `0`; old reachable object `0` |
| Private provider seed audit | unresolved P0/P1 `0`; provider audit PASS |
| Public readback | exact repository/root/tree and rights/security documents `PASS` |
| Anonymous public clone | exact root/tree; parent `0`; commit `1`; safety findings `0` |
| Main protection | exact current four; strict/admin `true`; force-push/deletion `false` |

Repository-owned safety report SHA-256:
`603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd`.

Official Gitleaks version is `8.30.1`. Its empty JSON report SHA-256 is
`37517e5f3dc66819f61f5a7bb8ace1921282415f10551d2defa5c3eb0985b570`.
The downloaded scanner archive SHA-256 is
`d29144deff3a68aa93ced33dddf84b7fdc26070add4aa0f4513094c8332afc4e`.

## Parentless root and provider seed

- Authoritative repository: <https://github.com/HardcoreMonk/purecvisor-desktop-node-public>
- Root commit: `c76a831be168a6b5aa122a91df3588a0c5e67f0d`
- Root tree: `7bc239d9278e89758659c37713100f279f74ada7`
- Git shape at seed: parent count `0`, commit count `1`, branch `main` only, tag `0`, release `0`,
  Actions artifact `0`.
- Provider seed audit contract: `pcv-public-provider-seed-audit-v1`.
- Provider audit SHA-256:
  `6adbc07e74d2a7a92165720ef8fa1f1d1eaa7063ef3cfed7e5b5913f9820047e`.

The exact seed SHA passed:

| workflow | run | job/check | job id | result |
| --- | ---: | --- | ---: | --- |
| Public Boundary Contract | `32853795424` | `public-boundary-ci-required` | `97820736612` | `PASS` |
| Development Gates | `32853795762` | `dotnet-tests` | `97820737912` | `PASS` |
| Development Gates | `32853795762` | `web-tests` | `97820738029` | `PASS` |
| Development Gates | `32853795762` | `packaging-pester` | `97820738472` | `PASS` |
| Development Gates | `32853795762` | `installer-web-pester` | `97820737704` | `PASS` |

## PUBLIC transition and protection

Fresh approval bound the one-way transition to
`HardcoreMonk/purecvisor-desktop-node-public@c76a831be168a6b5aa122a91df3588a0c5e67f0d`.
Independent readback returned owner `HardcoreMonk`, visibility `PUBLIC`, default branch `main`, the exact
root/tree above, and tracked blobs for `LICENSE`, `SECURITY.md`, and
`docs/PUBLIC_SOURCE_AUTHORITY.md`. Private vulnerability reporting changed from disabled to enabled and an
independent GET returned `enabled=true`.

Main protection was absent before installation (`404`, ETag absent). One complete protection request was
applied and the independent readback returned:

- required status checks: `dotnet-tests`, `web-tests`, `packaging-pester`,
  `installer-web-pester`;
- strict freshness `true`; admin enforcement `true`;
- force-push `false`; deletion `false`;
- required pull-request review absent; conversation-resolution requirement `false`;
- linear-history, branch-creation blocking, branch lock, and fork-sync rules `false`.

Protection readback contract is `pcv-public-main-protection-readback-v1`; canonical SHA-256 is
`4a060dc11eba3d8388077120f85515aec7b52113c3314fc3788b3ec814609653`. The post-install GET returned
ETag `W/"8c5e061212cbbfdd77be90c6153695ced62f8a5d10258e2c544c8584641921cc"`.

## Anonymous public baseline

An isolated temporary clone disabled credential helpers and interactive prompts and observed no configured
HTTP authorization extra header. It cloned the public HTTPS URL without credentials and read back the exact
root commit/tree, parent count `0`, commit count `1`, local branch `main`, and tag count `0`. The
repository-owned verifier returned findings `0` and report SHA-256
`603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd` under contract
`pcv-public-anonymous-baseline-v1`.

The original private archive remained `PRIVATE`. Its eight user-owned dirty files matched the immutable
oracle with mismatch count `0`. Contract `pcv-dirty-main-oracle-v1` canonicalizes ordinal-sorted normalized
relative-path/lowercase-SHA-256 lines as UTF-8 LF with a terminal LF; aggregate SHA-256 is
`79f5d440a5fb787d49a09e0d01293da5495fe2c3e5530e8b6afbb0367afd3257`.

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

## Draft cutover pull request

Draft PR [#1](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/pull/1) targets `main` from
`codex/pester-free-verification-cutover`. Provider readback at creation returned state `OPEN`, draft `true`,
base `main`, head branch exact, and creation head
`b6bd92cfb4c2377f0a9fae9edaaccd482d09566c`. The PR links the Wave C/D/E plans and explicitly states that
the current required identities still execute the legacy gates.

Wave C/D replacement implementation, same-SHA legacy/replacement shadow, atomic Wave E required-check
transition, merge SHA, and post-merge CI/evidence remain pending. This evidence does not claim the
Pester-free cutover has occurred.
