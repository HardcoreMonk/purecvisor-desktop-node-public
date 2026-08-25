# Installed account/noVNC operator surface smoke 2026-05-29 0.42.58

evidence_id: `installed-account-novnc-operator-surface-smoke-2026-05-29-04258`
result: `PASS`
scope: `installed-account-login-browser-and-target-backed-novnc-streaming`
version: `0.42.58-admin-smoke`
installed_account_login_smoke_artifact_root: `artifacts/installed-account-login-smoke-20260529-04258`
installed_account_login_smoke_summary: `artifacts/installed-account-login-smoke-20260529-04258/summary.json`
browser_smoke_artifact_root: `artifacts/web-console-account-login-browser-smoke-20260529-04258`
target_backed_novnc_artifact_root: `artifacts/target-backed-novnc-installed-streaming-smoke-20260529-04258-r2`
target_backed_novnc_summary: `artifacts/target-backed-novnc-installed-streaming-smoke-20260529-04258-r2/summary.json`
token/password/refresh-token observed: `false/false/false`
host_mutation_performed: `true-service-config-temporary-restored`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

Account login smoke는 임시 account/JWT signing key를 주입한 뒤 login/session/rbac/console
capabilities를 HTTP `200`으로 확인하고 원래 파일과 ACL을 복구했다. Browser QA는
dashboard, jobs/network/troubleshooting view, diagnostics create/download를 실행했고
missing button label 및 unlabeled input count는 모두 `0`이다.

Target-backed noVNC streaming smoke는 단독 r2 재실행에서 PASS했다. 초기 병렬 실행은
account smoke와 동시에 같은 Windows service를 재시작하면서 stop 충돌이 발생했으므로
canonical evidence는 `artifacts/target-backed-novnc-installed-streaming-smoke-20260529-04258-r2`
이다. r2는 service PathName을 복구했고 final service는 `Running`이다.

이 evidence는 installed admin-smoke operator surface evidence이며 public trusted signing 또는
외부 stable publication을 주장하지 않는다.
