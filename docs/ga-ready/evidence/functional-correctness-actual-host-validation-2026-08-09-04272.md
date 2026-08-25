# Functional correctness actual-host validation 2026-08-09 `0.42.72`

evidence_id: `functional-correctness-actual-host-validation-2026-08-09-04272`
result: `PASS`
evidence_scope: `installed-actual-vm-functional-correctness`
version: `0.42.72-admin-smoke`
source_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
artifact_root: `artifacts/functional-correctness-carryforward-20260809-04272`
artifact_summary: `artifacts/functional-correctness-carryforward-20260809-04272/summary.json`
summary_sha256: `4938e6307ce5beb9e012b1a05ce32a2e0e410ee735b87e3ecc0634897dbb6dd6`
attestation: `artifacts/functional-correctness-carryforward-20260809-04272/run-attestation.json`
attestation_sha256: `d53f9a7855cb7b34c5fe46db75ac626301917ea16fb782f422b01b8d5bacc736`
runner_sha256: `b0ac6cf563df637a9df42dfd8ab7f575bd7d8abc07329edcdcf3f84e90cf06ae`
installed_cli_sha256: `c7fac8d2f671596878ae58808b79028d7a2951dee371c5f371984a1d23f2d60c`
operational_msi_sha256: `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa`
operational_payload_sha256: `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933`
vm_name: `pcv-fc-cf-04272`
host_mutation_performed: `true`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.72-admin-smoke` 설치본 PCVCLI로 실제 Hyper-V VM을 생성해 QoS와 disk resize
경계를 재실행했다. 이 문서는 `0.42.71` carry-forward가 아니라 새 actual-VM 실행 evidence다.

| 계약 | 관측 | verdict |
| --- | --- | --- |
| command graph | `10` steps: primary `5` + job readback `5` | `PASS` |
| exit/readback | `10/10` exit `0`, `10/10` ok | `PASS` |
| QoS maximum | `2048 Kbps -> 2,048,000 bps` WMI readback | `PASS` |
| disk shrink | `10 GiB -> 9 GiB` 요청, `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` | `PASS` |
| shrink size guard | `10,737,418,240` bytes 유지 | `PASS` |
| disk expansion | `10 GiB -> 11 GiB`, `11,811,160,064` bytes | `PASS` |
| cleanup | VM 제거, VM folder/root 제거 | `PASS` |

## 실행 및 attestation

- run id:
  `functional-correctness-carryforward-20260809-04272-196f5c2b-6b11-479d-9b3f-352d1764ffbc`
- installed CLI, operational MSI/payload, source commit, service, Web, registration을 실행 전후
  attestation이 고정했다.
- service는 `Running/Automatic/LocalSystem`, credential target은
  `PureCVisor/PureCVisorDesktopNode/api-token`이다.
- 실행 후 installed payload aggregate는 실행 전과 같은 `deb40a67c…`다.
- raw secret은 artifact에 persisted되지 않았다.

## Nonclaims

- 이 검증은 actual VM을 만들고 삭제했으므로 host mutation을 수행했다.
- guest OS credentialed `guest-exec` smoke나 token rotation을 이 evidence가 주장하지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
