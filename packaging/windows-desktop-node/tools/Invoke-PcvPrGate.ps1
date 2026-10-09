[CmdletBinding()]
param(
    [string]$RepoRoot,
    [string]$ArtifactRoot,
    [switch]$Integration,
    [switch]$SkipBuild,
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# 계약 'pcv-pr-gate-v1' (2026-10-09 감사 §10, campaign audit-green-lean-20261009 Task 6). PR 을 열기 전에 이 호스트에서
# CI 와 같은 Release 구성으로 C# 시험을 돌리고 module size ratchet 을 확인한다. -Integration 이면 ADR-0016 통합 시험
# (pcv-it- 접두사 일회용 VM 생성·삭제, host mutation)을 더 돌리되, PCV_HYPERV_INTEGRATION_APPROVAL 이 active campaign 의
# approval_locator 안 문자열이 아니면 아무것도 돌리지 않고 거절한다(exit 2). -PlanOnly 는 단계만 적는다. 결과는
# artifacts/pr-gate/<yyyyMMdd-HHmmss>/summary.json 이다. GitHub runner 에는 Hyper-V 가 없으므로 이 gate 는 개발 호스트용이다.

$script:Contract = 'pcv-pr-gate-v1'
$script:ApprovalVariable = 'PCV_HYPERV_INTEGRATION_APPROVAL'

function New-PcvPrGateStep {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Kind,
        [Parameter(Mandatory)][string[]]$Command,
        [bool]$HostMutation = $false
    )

    [ordered]@{
        id = $Id
        kind = $Kind
        command = @($Command)
        host_mutation = $HostMutation
        status = 'planned'
        exit_code = $null
        duration_ms = $null
        detail = $null
    }
}

function Test-PcvPrGateIntegrationApproval {
    param([Parameter(Mandatory)][string]$Root)

    $approval = [Environment]::GetEnvironmentVariable($script:ApprovalVariable)
    if ([string]::IsNullOrWhiteSpace($approval) -or $approval.Length -lt 20) {
        return 'PCV_PR_GATE_INTEGRATION_APPROVAL_MISSING'
    }
    $campaignPath = Join-Path $Root 'docs/ga-ready/active-campaign.json'
    if (-not (Test-Path -LiteralPath $campaignPath -PathType Leaf)) {
        return 'PCV_PR_GATE_CAMPAIGN_MISSING'
    }
    $campaign = Get-Content -Raw -LiteralPath $campaignPath | ConvertFrom-Json
    $locator = [string]$campaign.approval_locator
    if (-not $locator.Contains($approval, [System.StringComparison]::Ordinal)) {
        return 'PCV_PR_GATE_INTEGRATION_APPROVAL_NOT_IN_LOCATOR'
    }
    $null
}

function Invoke-PcvPrGateStep {
    param(
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][string]$Root
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $Step.status = 'running'
    try {
        if ($Step.kind -eq 'pester') {
            $result = Invoke-Pester -Path (Join-Path $Root $Step.command[0]) -Output Detailed -PassThru
            $Step.exit_code = [int]$result.FailedCount
            $Step.detail = 'total={0} failed={1}' -f $result.TotalCount, $result.FailedCount
        }
        else {
            $executable = $Step.command[0]
            $arguments = @($Step.command | Select-Object -Skip 1)
            Push-Location -LiteralPath $Root
            try {
                & $executable @arguments
                $Step.exit_code = [int]$LASTEXITCODE
            }
            finally {
                Pop-Location
            }
        }
        $Step.status = if ($Step.exit_code -eq 0) { 'passed' } else { 'failed' }
    }
    catch {
        $Step.status = 'failed'
        $Step.detail = [string]$_
        if ($null -eq $Step.exit_code) { $Step.exit_code = -1 }
    }
    finally {
        $Step.duration_ms = [int64]$stopwatch.ElapsedMilliseconds
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
        (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    }
    else {
        (Resolve-Path -LiteralPath $RepoRoot).Path
    }
    $stamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss', [System.Globalization.CultureInfo]::InvariantCulture)
    $resolvedArtifactRoot = if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
        Join-Path $resolvedRepoRoot "artifacts/pr-gate/$stamp"
    }
    elseif ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
        $ArtifactRoot
    }
    else {
        Join-Path $resolvedRepoRoot $ArtifactRoot
    }

    $steps = [System.Collections.Generic.List[object]]::new()
    if (-not $SkipBuild) {
        $steps.Add((New-PcvPrGateStep -Id 'build' -Kind 'process' -Command @('dotnet', 'build', 'src/DesktopNode.sln', '-c', 'Release', '--nologo')))
    }
    $steps.Add((New-PcvPrGateStep -Id 'dotnet-test-release' -Kind 'process' -Command @('dotnet', 'test', 'src/DesktopNode.sln', '-c', 'Release', '--no-build', '--nologo')))
    $steps.Add((New-PcvPrGateStep -Id 'module-size-ratchet' -Kind 'pester' -Command @('packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1')))
    $approvalProblem = $null
    if ($Integration) {
        $approvalProblem = Test-PcvPrGateIntegrationApproval -Root $resolvedRepoRoot
        $steps.Add((New-PcvPrGateStep -Id 'hyperv-integration' -Kind 'process' -Command @('dotnet', 'test', 'src/DesktopNode.HyperV.IntegrationTests', '-c', 'Release', '--nologo') -HostMutation $true))
    }

    $startedAt = [DateTimeOffset]::UtcNow
    $refused = [bool]$Integration -and $null -ne $approvalProblem
    if (-not $PlanOnly -and -not $refused) {
        foreach ($step in $steps) {
            Invoke-PcvPrGateStep -Step $step -Root $resolvedRepoRoot
            if ($step.status -ne 'passed') { break }
        }
        foreach ($step in $steps) {
            if ($step.status -eq 'planned') { $step.status = 'skipped' }
        }
    }

    $ok = if ($PlanOnly) { -not $refused } else { -not $refused -and @($steps | Where-Object { $_.status -ne 'passed' }).Count -eq 0 }
    $integrationApproval = if (-not $Integration) { 'not-requested' } elseif ($null -eq $approvalProblem) { 'ok' } else { $approvalProblem }
    $summary = [ordered]@{
        schema_version = 1
        contract = $script:Contract
        ok = [bool]$ok
        plan_only = [bool]$PlanOnly
        integration_requested = [bool]$Integration
        integration_approval = $integrationApproval
        repo_root = $resolvedRepoRoot
        started_at = $startedAt.ToString('o')
        completed_at = [DateTimeOffset]::UtcNow.ToString('o')
        host_mutation_performed = (@($steps | Where-Object { $_.host_mutation -and ($_.status -eq 'passed' -or $_.status -eq 'failed') }).Count -gt 0)
        steps = @($steps)
    }
    New-Item -ItemType Directory -Force -Path $resolvedArtifactRoot | Out-Null
    $summaryPath = Join-Path $resolvedArtifactRoot 'summary.json'
    [System.IO.File]::WriteAllText($summaryPath, (($summary | ConvertTo-Json -Depth 6) + "`n"), [System.Text.UTF8Encoding]::new($false))
    Write-Output $summaryPath
    Write-Output ('pr-gate ok={0} plan_only={1} integration={2} steps={3}' -f $ok, [bool]$PlanOnly, $integrationApproval, (($steps | ForEach-Object { $_.id + ':' + $_.status }) -join ','))
    if ($refused) { exit 2 }
    if (-not $ok) { exit 1 }
    # 같은 PowerShell 세션에서 & 로 부를 때 $LASTEXITCODE 가 이전 값으로 남지 않도록 성공도 명시한다.
    exit 0
}
