# C# architecture Wave 2B operation reconciliation decision evidence

## 결과

Wave 2B operation reconciliation 결정표를 `code_complete + promotion_not_triggered`로 닫았다.
이 slice는 문서·machine-readable fixture·Pester guard만 변경했으며 product retry/recovery behavior,
job schema, Hyper-V state, host 또는 VM을 변경하지 않았다.

| 항목 | 결과 |
|---|---|
| Decision fixture | `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2b-reconciliation.json` |
| Decision spec | `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2b-operation-reconciliation-decision.md` |
| Coverage | current 22 mutation operations / 9 families |
| Required rows | `vm.create`, `vm.delete`, `vm.rename`, QoS, checkpoint 명시 |
| Persisted-running contract | `PCV_JOB_INTERRUPTED`, failed projection, `retryable=false`, automatic retry=false |
| Timeout | current route timeout default 30 seconds, configured range 1~3600; new reconciliation timeout 없음 |
| Guest Execution | Wave 2B 자동 retry/reconcile 제외, ADR-0009 별도 설계 유지 |
| Focused Pester | 6/6 PASS, skip 0 |
| M/Full development verification | 7/7 suites PASS, `ok=true`, `artifacts/development-verification-wave2b/summary.json` |
| host_mutation_performed | `false` |
| hyperv_mutation_performed | `false` |
| actual_vm_validation_performed | `false` |
| public_trusted_signing | `false` |
| external_stable_publication | `false` |
| Operational anchor | `0.42.65-admin-smoke` carry-forward |

## 핵심 판정

- create/delete/rename/checkpoint create-delete는 postcondition readback이 가능하지만 현재 job row에
  persisted before-state가 없어 자동 terminal-success로 올리지 않는다.
- QoS는 `vm.blkio-get`/`vm.bandwidth`가 실제 정책 수치를 제공하지 않으므로 persisted-running 상태를
  자동 reconciliation할 수 없다. terminal apply evidence는 완료된 작업의 보조 증거일 뿐이다.
- checkpoint restore와 guest shutdown/restart는 현재 readback만으로 외부 효과 완료를 증명할 수 없어
  수동 확인만 허용한다.
- Guest Execution 세 operation은 ADR-0009 경계 밖에서 열지 않는다.

## 검증 경계

이 evidence는 code-level/document contract 범위다. M/Full 검증은 repository test/build/evidence suite만
선택했으며 product install이나 host mutation은 수행하지 않았다. package build, installed service mutation, Hyper-V,
actual VM, full-admin gate, update/rollback, public signing/publication은 실행하지 않았다. Wave 2C는
operation별 승인과 별도 L/Release/actual-VM gate 없이는 시작하지 않는다.
