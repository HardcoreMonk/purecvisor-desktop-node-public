# Persistent Windows guest target policy 2026-05-28 0.42.55

evidence_id: `persistent-windows-guest-target-policy-2026-05-28-04255`
result: `POLICY_CONFIRMED_KEEP_AFTER_04255_FULLGATE`
scope: `guest-execution-persistent-windows-target-keep-policy`
version: `0.42.55-admin-smoke`
vm_name: `pcv-guest-installed-04253-r1`
state: `Running`
path: `D:\PureCVisor\SmokeVMs\pcv-guest-installed-04253-r1\pcv-guest-installed-04253-r1`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 확인 결과

`pcv-guest-installed-04253-r1`은 삭제하지 않고 다음 installed guest execution evidence cycle까지
보존한다. Hyper-V Notes는 이미 아래 policy 정보를 포함한다.

```text
guest_family=windows
guest_os=Microsoft Windows Server 2022 Datacenter Evaluation
managed-by=purecvisor-desktop-node-smoke
persistent_policy=keep-until-next-evidence-cycle
```

이번 확인에서는 `Set-VM` 또는 cleanup mutation을 실행하지 않았다. 0.42.55 installed
current-card와 actual credentialed guest-exec smoke는 이 persistent target을 재사용했다.
