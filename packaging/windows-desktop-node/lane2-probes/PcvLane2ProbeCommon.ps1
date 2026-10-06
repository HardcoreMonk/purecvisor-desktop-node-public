# Shared helpers for the tracked Lane 2 probes (design pcv-train-host-inputs-v1 section 3.4).
# Dot-source this file from a probe script. Nothing here runs pcvcli or Hyper-V until a probe calls it.

function Resolve-PcvLane2ProbePath {
    param([Parameter(Mandatory)][string]$RepoRoot, [Parameter(Mandatory)][string]$Path)
    if ([System.IO.Path]::IsPathRooted($Path)) { return $Path }
    return Join-Path $RepoRoot $Path
}

# -PlanOnly prints the plan and writes nothing, so the real run can still create the artifact root afterwards.
function Write-PcvLane2ProbePlan {
    param([Parameter(Mandatory)][System.Collections.IDictionary]$Plan)
    $Plan['plan_only'] = $true
    $Plan['host_mutation_performed'] = $false
    $Plan | ConvertTo-Json -Depth 8
}

function Initialize-PcvLane2Probe {
    param([Parameter(Mandatory)][string]$ArtifactRoot, [Parameter(Mandatory)][string]$CliPath)
    if (Test-Path -LiteralPath $ArtifactRoot) { throw "artifact root exists: $ArtifactRoot" }
    if (-not (Test-Path -LiteralPath $CliPath)) { throw "pcvcli not found: $CliPath" }
    New-Item -ItemType Directory -Path $ArtifactRoot | Out-Null
    $script:PcvProbeRoot = $ArtifactRoot
    $script:PcvProbeCli = $CliPath
    $script:PcvProbeCall = 0
    $script:PcvProbeSteps = [System.Collections.Generic.List[object]]::new()
}

# An empty JSON array or a missing value both become an empty array, never $null (2026-10-06 P1-10 r1 defect).
function ConvertTo-PcvArray {
    param($Value)
    if ($null -eq $Value) { return , @() }
    return , @($Value)
}

function Get-PcvField {
    param($Object, [string]$Name)
    if ($null -ne $Object -and $Object.PSObject.Properties[$Name]) { return $Object.$Name }
    return $null
}

function ConvertFrom-PcvCliText {
    param([string]$Text)
    try { return $Text | ConvertFrom-Json -Depth 64 -DateKind String } catch { }
    $line = @($Text -split "`r?`n" | Where-Object { $_.TrimStart().StartsWith('{') }) | Select-Object -First 1
    if ($line) { try { return $line | ConvertFrom-Json -Depth 64 -DateKind String } catch { } }
    return $null
}

function Invoke-PcvProbeCli {
    param([Parameter(Mandatory)][string]$Label, [Parameter(Mandatory)][string[]]$CliArgs)
    $script:PcvProbeCall++
    $out = & $script:PcvProbeCli --json @CliArgs 2>&1
    $code = $LASTEXITCODE
    $text = ($out | Out-String)
    [System.IO.File]::WriteAllText((Join-Path $script:PcvProbeRoot ('{0:000}-{1}.json' -f $script:PcvProbeCall, $Label)), $text)
    return [pscustomobject]@{ label = $Label; exit = $code; json = (ConvertFrom-PcvCliText $text); text = $text }
}

function Get-PcvProbeError {
    param($Result)
    if ($null -eq $Result.json) { return ($Result.text.Trim() -split "`n")[0] }
    $error0 = Get-PcvField $Result.json 'error'
    if ($error0) { return ($error0 | ConvertTo-Json -Depth 8 -Compress) }
    return $null
}

function Get-PcvProbeJobId {
    param($Result)
    $data = Get-PcvField $Result.json 'data'
    foreach ($name in 'job_id', 'id') {
        $value = Get-PcvField $data $name
        if ($value) { return [string]$value }
    }
    $job = Get-PcvField $data 'job'
    $value = Get-PcvField $job 'id'
    if ($value) { return [string]$value }
    return $null
}

function Get-PcvProbeJob {
    param([Parameter(Mandatory)][string]$JobId)
    $text = & $script:PcvProbeCli --json job get $JobId 2>&1 | Out-String
    return Get-PcvField (ConvertFrom-PcvCliText $text) 'data'
}

# Queued routes are polled until they leave queued/running; routes that answer at once report immediate-ok or not-queued.
function Wait-PcvProbeJob {
    param([Parameter(Mandatory)][string]$Label, [Parameter(Mandatory)]$Result, [int]$TimeoutMinutes = 5)
    $id = Get-PcvProbeJobId $Result
    $status = Get-PcvField (Get-PcvField $Result.json 'data') 'status'
    if (-not $id -or $status -notin @('queued', 'running')) {
        $immediate = if ((Get-PcvField $Result.json 'ok') -eq $true) { 'immediate-ok' } else { 'not-queued' }
        return [pscustomobject]@{ label = $Label; job_id = $id; status = $immediate; error = (Get-PcvProbeError $Result) }
    }
    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    do {
        Start-Sleep -Seconds 2
        $job = Get-PcvProbeJob $id
        $status = if ($job) { [string](Get-PcvField $job 'status') } else { 'unknown' }
    } while ($status -in @('queued', 'running', 'unknown') -and (Get-Date) -lt $deadline)
    $jobError = Get-PcvField $job 'error'
    return [pscustomobject]@{
        label = $Label
        job_id = $id
        status = $status
        error = if ($jobError) { $jobError | ConvertTo-Json -Depth 8 -Compress } else { $null }
    }
}

function Get-PcvProbeVmRows {
    param([string]$Label = 'vm-list')
    $result = Invoke-PcvProbeCli $Label @('vm', 'list')
    return ConvertTo-PcvArray (Get-PcvField $result.json 'data')
}

function Get-PcvProbeVm {
    param([Parameter(Mandatory)][string]$Name)
    return @(Get-PcvProbeVmRows | Where-Object { (Get-PcvField $_ 'name') -eq $Name }) | Select-Object -First 1
}

function Add-PcvProbeStep {
    param([string]$Name, [string]$Expect, $Observed, [bool]$Pass)
    $script:PcvProbeSteps.Add([ordered]@{ step = $Name; expect = $Expect; observed = $Observed; pass = $Pass })
}

function Get-PcvProbeServiceState {
    $service = Get-Service PureCVisorDesktopNode
    $web = try { (Invoke-WebRequest http://127.0.0.1/ -UseBasicParsing -TimeoutSec 10).StatusCode } catch { 0 }
    return [ordered]@{
        service = "$($service.Status)/$($service.StartType)"
        web = $web
        pass = ($service.Status -eq 'Running') -and ($service.StartType -eq 'Automatic') -and ($web -eq 200)
    }
}

# Deletes managed probe VMs through pcvcli, then removes any leftover probe VM and folder; Join-Path only (2026-10-06 r2, r3).
function Remove-PcvProbeVms {
    param([Parameter(Mandatory)][string]$Prefix, [string]$VmRoot)
    foreach ($probe in @(Get-PcvProbeVmRows 'vm-list-cleanup' | Where-Object { (Get-PcvField $_ 'name') -like "$Prefix*" })) {
        $name = Get-PcvField $probe 'name'
        if ((Get-PcvField $probe 'state') -eq 'running') {
            $null = Wait-PcvProbeJob 'cleanup-poweroff' (Invoke-PcvProbeCli 'cleanup-poweroff' @('vm', 'poweroff', $name))
        }
        if (Get-PcvField $probe 'template_lock') {
            $null = Wait-PcvProbeJob 'cleanup-unlock' (Invoke-PcvProbeCli 'cleanup-unlock' @('vm', 'template-unlock', $name, '--yes'))
        }
        if (Get-PcvField $probe 'managed_by_purecvisor') {
            $null = Wait-PcvProbeJob 'cleanup-delete' (Invoke-PcvProbeCli 'cleanup-delete' @('vm', 'delete', $name, '--yes'))
        }
    }
    Get-VM -Name "$Prefix*" -ErrorAction SilentlyContinue | Stop-VM -TurnOff -Force -ErrorAction SilentlyContinue
    Get-VM -Name "$Prefix*" -ErrorAction SilentlyContinue | Remove-VM -Force
    if ($VmRoot -and (Test-Path -LiteralPath $VmRoot)) {
        Get-ChildItem -LiteralPath $VmRoot -Directory -Filter "$Prefix*" -ErrorAction SilentlyContinue |
            Where-Object { -not (Get-VM -Name $_.Name -ErrorAction SilentlyContinue) } |
            ForEach-Object { Remove-Item -LiteralPath (Join-Path $VmRoot $_.Name) -Recurse -Force }
    }
    return @(Get-VM -Name "$Prefix*" -ErrorAction SilentlyContinue).Count
}

function Write-PcvProbeSummary {
    param([Parameter(Mandatory)][System.Collections.IDictionary]$Summary)
    $path = Join-Path $script:PcvProbeRoot 'summary.json'
    [System.IO.File]::WriteAllText($path, ($Summary | ConvertTo-Json -Depth 12) + "`n", [System.Text.UTF8Encoding]::new($false))
    $Summary | ConvertTo-Json -Depth 12
}
