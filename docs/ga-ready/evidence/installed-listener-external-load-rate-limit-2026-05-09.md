# Installed Listener External Load/Rate-Limit Evidence - 2026-05-09

evidence_id: installed-listener-external-load-rate-limit-2026-05-09
scope: installed-listener-external-load-rate-limit-smoke
result: PASS
artifact_root: artifacts/installed-listener-external-load-rate-limit-20260509-0391
actual_execution: installed-listener-external-http-load-executed
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
service_name: PureCVisorDesktopNode
token_value_observed: false

## Summary

The installed `PureCVisorDesktopNode` listener was exercised through real HTTP requests against `http://127.0.0.1:7777/api/v1/runtime/policy`.

The smoke resolved the service token from the DPAPI LocalMachine protected token file in memory only. The token value was not written to the evidence artifact. After allowing the previous rate window to expire, the runner sent 180 external HTTP requests and verified the installed listener rate-limit contract.

## Observed Result

- service before smoke: `Running`
- request count: `180`
- HTTP 200 count: `140`
- HTTP 429 count: `40`
- unexpected status count: `0`
- Retry-After on 429 responses: `40`
- `PCV_RATE_LIMIT_EXCEEDED` problem details on 429 responses: `40`
- load test status: `installed-external-http-pass`
- rate-limit contract: `retry-after-problem-details-pass`

This evidence does not mutate the host and does not claim public trusted signing or external stable publication.

## Verification

```powershell
Get-Content -Raw artifacts/installed-listener-external-load-rate-limit-20260509-0391/summary.json
```
