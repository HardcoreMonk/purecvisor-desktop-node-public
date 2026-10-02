# Lane 2 DVD eject와 빈 drive attach `0.42.86-admin-smoke` (2026-10-02)

evidence_id: `lane2-vm-media-eject-attach-2026-10-02-04286`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.86-admin-smoke`
installed_product_version: `0.42.86-admin-smoke`
installed_host_sha256: `55a1c1a80faa4328afcb88a487b64a8cddf603d53d75988d18a33ccd14e8df9b`
installed_cli_sha256: `fd01b161a142e7e2d92ada0a1800e4a413bce1f0946ac7695a73a6ca1d6d5bbd`
package_provenance: `1c488b62bc68783f4673bb4f441f65d5de5c561c`
media_fix: `fb95de1b6349a3d10cfadd814f9e50bbbc72126a`
probe_vm: `pcv-wo-media-1002`
artifact_root: `artifacts/lane2-vm-media-eject-attach-20261002-04286`
host_mutation_performed: `true`
probe_vm_removed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

설치본 `0.42.86-admin-smoke`에서 `vm.eject`와 빈 DVD drive `vm.attach`만 확인했다. Host와 CLI SHA-256은 패키지 evidence와 같다. CLI는 `--protected-token-file`로 인증했고 token 값은 기록하지 않았다.

probe VM은 Generation 2, CPU 2, memory 2048 MB, disk 20 GB로 만들었고 켜지 않았다. 생성 ISO는 `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`다. 빈 drive에 붙인 ISO는 `D:\Downloads\ubuntu-26.04-desktop-amd64.iso`다.

## 결과

| 단계 | 관측 | 판정 |
| --- | --- | --- |
| 생성 뒤 `dvd_media` | `D:\Downloads\ubuntu-26.04-live-server-amd64.iso` | `PASS` |
| 생성 뒤 drive 수 | `1` | `PASS` |
| `vm eject` | job `succeeded`, `dvd_media` 비음 | `PASS` |
| eject 뒤 drive 수 | `1` | `PASS` |
| 빈 drive `vm attach` | job `succeeded`, `dvd_media`가 desktop ISO | `PASS` |
| attach 뒤 drive 수 | `1` | `PASS` |

## 정리

- `vm delete --yes` job이 `succeeded`다.
- 제품이 남긴 `D:\PureCVisor\VMs\pcv-wo-media-1002`와 `disk0.vhdx`는 어떤 VM도 참조하지 않는 것을 확인한 뒤 지웠다.
- 보존 VM `pcv-guest-installed-04253-r1`은 Off 그대로다.
- 기존 잔여 디렉터리 `D:\PureCVisor\VMs\pcv-p1-clone-04276-34a8e66d-dst`는 건드리지 않았다.

## Nonclaims

- 상태는 `installed_non_promoted_candidate`다. operational current는 `0.42.84-admin-smoke`다.
- pair, fullgate, current-card, current-evidence 쓰기는 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
