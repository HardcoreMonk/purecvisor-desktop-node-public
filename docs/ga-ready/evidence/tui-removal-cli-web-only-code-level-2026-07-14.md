# TUI 제거와 CLI/Web-only 운영자 표면 code-level evidence

evidence_id: `tui-removal-cli-web-only-code-level-2026-07-14`
date: `2026-07-14`
result: `PASS`
host_mutation_performed: `false`
operator_surface_decision: `cli-web-only`
tui_product_status: `removed-from-active-product`
installed_promotion_status: `pending-0.42.63-admin-smoke`
adr: `docs/adr/0011-cli-web-only-operator-surface.md`

## 범위

활성 제품의 운영자 표면을 Web Console과 PCVCLI로 제한했다. TUI source/test, solution
membership, package payload와 installed TUI smoke runner는 제거됐고 Local API와 backend
기능은 유지된다. Dated TUI evidence와 `0.42.62-admin-smoke` 설치 사실은 historical
predecessor로 보존한다.

## 관찰된 검증

| Slice | 관찰 결과 |
| --- | --- |
| Task 1 package/installer Pester | `93/93` PASS |
| Task 2 Web verification | `31/31` PASS |
| Task 3 .NET solution | `582/582` PASS |
| Task 3 active product boundary focused test | `1/1` PASS |
| Task 3 noVNC focused test | `1/1` PASS |
| Task 5 .NET solution | `582/582` PASS |
| Task 5 Web commands | `3/3` PASS (`test`, `verify:parity`, `browser:fixture`) |
| Task 5 packaging Pester | `380/380` PASS |
| Task 5 installer/Web Pester | `94/94` PASS |
| Task 5 active production boundary | `0` hits; solution TUI project/name/GUID `0` hits |

위 수치는 각 slice에서 실제 관찰된 값만 기록한다. Task 5 통합 검증은 host mutation 없이
완료됐으며 active production boundary 검색은 test-only negative regression contract를
제외했다.

## 설치본 승격 경계

- 현재 설치 evidence는 `0.42.62-admin-smoke` dated predecessor이며 당시 TUI 포함 사실을
  그대로 보존한다.
- `0.42.63-admin-smoke` package/fullgate/CLI-Web current-card PASS는 아직 주장하지 않는다.
- Package manifest schema `2`, MSI upgrade 잔여 `pcvtui.exe` cleanup, CLI/Web installed
  current-card가 후속 installed promotion에서 검증되어야 한다.
- Public trusted signing과 external stable publication은 이 code-level evidence의 범위가
  아니다.
