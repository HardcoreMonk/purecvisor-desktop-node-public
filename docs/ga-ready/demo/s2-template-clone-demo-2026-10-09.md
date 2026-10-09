# S2 template clone 시연 (2026-10-09)

- 시나리오: `S2`. 설치한 VM을 template으로 만들고 복제해 새 VM을 1분 안에 받는다.
- 기준: `config/project-completion-criteria.json`의 S2. ADR-0017.
- 이 기록은 이 호스트 설치본 `0.42.93-admin-smoke`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable publication을 주장하지 않는다. S3~S4는 주장하지 않는다.

## 실행

- 명령: `node web/scripts/run-s2-clone-scenario.mjs --execute`
- 원본: `pcv-it-s2-source`. managed Generation 2, vCPU 1, memory 512MB, disk 8GB 독립 VHDX, Off. create가 ISO 경로를 요구해서 기존 Ubuntu ISO를 DVD로만 붙였고 VM은 켜지 않았다. checkpoint 없음, TPM 없음.
- template lock job `succeeded`.
- clone preview HTTP 200.
- clone job `succeeded`, `elapsed_ms=3091` (`bound_ms=60000`).
- 복제본 `pcv-it-s2-clone` delete job `succeeded`.
- 보존 VM `pcv-guest-installed-04253-r1`은 Off 그대로다. 원본 `pcv-it-s2-source`는 Off인 template로 남긴다.

## 산출

- `artifacts/s2-template-clone-20261009/summary.json` (`result=pass`)
