# Persistent Windows Guest Target Policy 2026-05-28 0.42.54

evidence_id: `persistent-windows-guest-target-policy-2026-05-28-04254`
result: `POLICY_CONFIRMED_KEEP_AFTER_04254_FULLGATE`
scope: `persistent-installed-windows-vhd-guest-target-lifecycle-after-04254-fullgate`
version_anchor: `0.42.54-admin-smoke`
vm_name: `pcv-guest-installed-04253-r1`
vm_state: `Running`
guest_family_note: `guest_family=windows`
guest_os_note: `Microsoft Windows Server 2022 Datacenter Evaluation`
persistent_policy_note: `keep-until-next-evidence-cycle`
host_mutation_performed: `false-existing-notes-already-match-policy`
cleanup_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결정

`pcv-guest-installed-04253-r1`은 삭제하지 않고 다음 installed guest execution evidence cycle까지
보존한다. 현재 Hyper-V Notes는 이미 `guest_family=windows`, guest OS, `persistent_policy=keep-until-next-evidence-cycle`를
포함하므로 추가 `Set-VM` 변경은 수행하지 않았다.

## 근거

- 0.42.53 credentialed Windows guest execution smoke에서 persistent target으로 PASS했다.
- 0.42.54 running cancel installed smoke에서도 같은 VM을 기준으로 long-running command cancel을 PASS했다.
- Web/TUI/CLI current-card에서 `pcv-guest-installed-04253-r1` projection은 `guest_family=windows`로
  확인됐다.

## 경계

- VM delete/cleanup은 수행하지 않았다.
- raw credential value는 출력하거나 문서화하지 않았다.
- 이 정책은 internal admin-smoke target lifecycle이며 public clean-host smoke가 아니다.
