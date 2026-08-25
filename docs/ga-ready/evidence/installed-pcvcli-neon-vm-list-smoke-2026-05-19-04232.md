# Installed PCVCLI neon VM list smoke 2026-05-19 0.42.32

evidence_id: `installed-pcvcli-neon-vm-list-smoke-2026-05-19-04232`
result: `PASS`
scope: `installed-pcvcli-real-vm-list-rendering`
version: `0.42.32-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04232.md`
artifact_root: `artifacts/installed-pcvcli-neon-vm-list-smoke-20260519-04232`
summary: `artifacts/installed-pcvcli-neon-vm-list-smoke-20260519-04232/summary.json`
msi_sha256: `8d8c585fe73c605bd938705ef63790768348791cb479bf42c4bbbf8b31af14dc`
cli_sha256: `a227de915d298e45bdc92d6f8a5341f54f7ee0785c2621dcfc8af0551afa6239`
manifest_version: `0.42.32-admin-smoke`
pcvcli_global_path: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
token_source: `default-protected-token-file-auto-discovery`
host_mutation_performed: `true`
test_vm_name: `pcv-neon-list-04232`
test_vm_iso: `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`
test_vm_state_captured: `running`
test_vm_cleanup: `pass`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 설치된 `0.42.32-admin-smoke` MSI 기준으로 전역 `pcvcli`가 자동 token
discovery를 사용해 실제 Hyper-V VM 정보를 조회하고, `vm list`를 한 줄 단위 table과
neon ANSI 컬러로 렌더링하는지 확인한 결과다.

## 설치본 확인

| 항목 | 결과 |
| --- | --- |
| MSI install/update | `msiexec /i ...0.42.32... /qn /norestart`, exit `0` |
| Installed manifest | `0.42.32-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running` |
| Global `pcvcli` | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| Token | `--token`, `--token-file`, `--token-env`, `--protected-token-file` 없이 protected token file auto discovery |
| `pcvcli --json host status` | exit `0` |
| 초기 `pcvcli --json vm list` | exit `0`, VM count `0` |

Bearer token, password, refresh token, JWT signing key 값은 stdout/stderr 또는
summary에 기록하지 않았다.

## 실제 VM row smoke

| 단계 | 결과 |
| --- | --- |
| `pcvcli --json vm create pcv-neon-list-04232 ...` | job `succeeded` |
| `pcvcli --json vm start pcv-neon-list-04232` | job `succeeded` |
| `pcvcli --json vm list` | VM count `1`, name `pcv-neon-list-04232`, state `running` |
| `pcvcli --no-color vm list` | `SYS_UUID | ENTITY_ID | LIFELINE` table에 실제 VM row 출력 |
| `pcvcli vm list` | ANSI neon header/name/state color 포함 |
| `pcvcli --json vm get pcv-neon-list-04232` | `managed_by_purecvisor=true`, `platform=hyperv`, `generation=2`, `Default Switch` |

무색 table 캡처:

```text
SYS_UUID            | ENTITY_ID           | LIFELINE
--------------------+---------------------+---------
pcv-neon-list-04232 | pcv-neon-list-04232 | running
```

컬러 table 캡처는
`artifacts/installed-pcvcli-neon-vm-list-smoke-20260519-04232/pcvcli-vm-list-table-running-neon.stdout.txt`에
보존한다. 해당 파일은 ANSI escape code를 포함하며 `summary.json`의
`neon_contains_ansi=true`로 확인했다.

## 정리

테스트 VM은 `pcvcli --json vm poweroff`, `pcvcli --json vm delete --yes`로 각각 job
`succeeded`를 확인했다. 제품 delete path가 보존한 테스트 VHD 폴더
`D:\PureCVisor\SmokeVMs\pcv-neon-list-04232`는 evidence 캡처 후 해당 경로가
`D:\PureCVisor\SmokeVMs` 아래인지 확인하고 삭제했다. 최종
`pcvcli --json vm list`는 VM count `0`이다.

## 경계

이 smoke는 설치본 PCVCLI feature verification이다. `0.42.31-admin-smoke ->
0.42.32-admin-smoke` manual-admin package-pair closure, full admin host mutation gate,
Burn/MSIX lifecycle, public trusted signing, external stable publication은 아직
주장하지 않는다.
