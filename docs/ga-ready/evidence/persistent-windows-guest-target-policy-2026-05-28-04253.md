# Persistent Windows Guest Target Policy 2026-05-28 0.42.53

evidence_id: `persistent-windows-guest-target-policy-2026-05-28-04253`
result: `POLICY_CONFIRMED_KEEP_UNTIL_NEXT_EVIDENCE_CYCLE`
scope: `persistent-installed-windows-vhd-guest-target-lifecycle`
version_anchor: `0.42.53-admin-smoke`
vm_name: `pcv-guest-installed-04253-r1`
guest_family_note: `guest_family=windows`
guest_os_note: `Microsoft Windows Server 2022 Datacenter Evaluation`
persistent_policy_note: `keep-until-next-evidence-cycle`
host_mutation_performed: `true-set-hyperv-vm-notes-only`
cleanup_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결정

`pcv-guest-installed-04253-r1`은 다음 installed guest execution evidence cycle까지 보존한다.
이 VM은 `guest-exec`, `guest-agent-ensure-channel --verify`, Web/TUI selected VM row smoke의
persistent Windows target이다.

## 실행

```powershell
Set-VM -Name pcv-guest-installed-04253-r1 -Notes @"
guest_family=windows
guest_os=Microsoft Windows Server 2022 Datacenter Evaluation
managed-by=purecvisor-desktop-node-smoke
persistent_policy=keep-until-next-evidence-cycle
"@
```

## 경계

- VM delete/cleanup은 수행하지 않았다.
- raw credential value는 출력하거나 문서화하지 않았다.
- 이 정책은 internal admin-smoke target lifecycle이며 public clean-host smoke가 아니다.
- 새 package install 전까지 설치본 `pcvcli`는 이전 projection code를 사용할 수 있다. Code-level
  projection fix는 다음 package/current-card에서 설치본으로 승격한다.
