# Managed delete 디스크 정리 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** managed delete가 VM 전용 디렉터리의 디스크를 남기는 결함을 소스에서 고친다(설계 `docs/superpowers/specs/2026-10-03-purecvisor-desktop-node-managed-delete-storage-cleanup-design.md`).

**Architecture:** delete provider가 `DestroySystem` 전후로 저장소 배치와 남은 참조를 읽고, `DesktopNodeHyperVVmStorageCleanup`이 판정과 삭제를 한다. 결과는 job 결과 `storage_cleanup`에 남는다.

**Tech Stack:** C# / .NET 10, WMI, xUnit

## 사용자 결정 (2026-10-03)

승인: `전부 승인 합니다` 뒤 후보 질문에서 `managed delete 잔여 정리 (권장)`를 골랐다. 범위는 소스 수정과 테스트다. 설치 반영과 Lane 2 확인은 다음 package pair 몫이다. push/PR은 승인 밖이다.

## Task 1: 정리 구현과 테스트

- [x] `DesktopNodeHyperVVmStorageCleanup`(판정·실행, 파일 시스템 주입), delete provider의 `ConfigurationDataRoot`·연결 디스크·남은 참조 읽기, delete 결과 `storage_cleanup`.
- [x] HyperV 테스트: 정리 판정 `9`건, adapter 결과 `1`건.
- [x] 설계 문서와 `CLI_COMMAND_USAGE.md` delete 행.

실행 기록(2026-10-03): HyperV.Tests `255/255`. 이 호스트에서 delete를 실행하지 않았다.

## Task 2: 종료 검증

- [x] clean HEAD에서 `dotnet test src/DesktopNode.sln`, Pester, `npm run test:required --prefix web`, Required CI 네 shard.
- [x] campaign을 닫는다.

실행 기록(2026-10-03, clean HEAD `96e8570`): `dotnet test src/DesktopNode.sln` 실패 `0`(HyperV `255`, Delivery `752`, Verification `557`, Api `488`, Host `216`, Contracts `200`, Cli `179`, Runtime `129`, Service `11`). Pester 두 종 `116/116`. `npm run test:required --prefix web` exit `0`. Release build 뒤 Required CI 네 shard 모두 `ok=true`, `plan_only=false`. `NativeAdapter.Mutations.cs`는 라쳇 상한 `763`줄 그대로다.

## Nonclaims

- 실제 VM에서 정리가 동작한다고 아직 주장하지 않는다.
