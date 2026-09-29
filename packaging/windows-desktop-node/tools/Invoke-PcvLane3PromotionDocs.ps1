[CmdletBinding()]
param(
    [string]$SpecPath,
    [string]$RepoRoot,
    [switch]$Apply,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Runs the Lane 3 document steps from one promotion spec, in the order the 0.42.78 promotion took
# them by hand (064f6f2, ff77943, ab88076): generated current blocks from current-evidence.json,
# descriptor chain, ledger head keys, ledger status rows, index sections, then the contract spec
# pins over the result. current-evidence.json itself, the evidence documents, and the version C#
# verifier stay manual. -Apply validates every sub-spec with a dry-run before it writes anything
# and stops at the first failing step.

$script:PcvLane3Steps = @(
    [pscustomobject]@{ name = 'current_evidence_docs'; tool = 'Update-PcvCurrentEvidenceDocs.ps1'; spec = $null; arguments = @() }
    [pscustomobject]@{ name = 'descriptor_chain'; tool = 'Update-PcvManualAdminDescriptorChain.ps1'; spec = 'descriptor_chain'; arguments = @() }
    [pscustomobject]@{ name = 'ledger_head'; tool = 'Update-PcvManualAdminDescriptorChain.ps1'; spec = 'ledger_head'; arguments = @('-DescriptorPath', 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md') }
    [pscustomobject]@{ name = 'ledger_rows'; tool = 'Update-PcvCurrentEvidenceLedgerRows.ps1'; spec = 'ledger_rows'; arguments = @() }
    [pscustomobject]@{ name = 'index_sections'; tool = 'New-PcvPromotionIndexSections.ps1'; spec = 'index_sections'; arguments = @() }
    [pscustomobject]@{ name = 'spec_pins'; tool = 'Update-PcvContractSpecPins.ps1'; spec = $null; arguments = @() }
)

function Throw-PcvLane3PromotionDocsInvalid {
    param(
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Detail
    )

    throw "PCV_LANE3_PROMOTION_DOCS_INVALID|$Field|$Detail"
}

function Split-PcvLane3PromotionSpec {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Json,
        [Parameter(Mandatory)][string]$Directory
    )

    # Each section is passed to its tool verbatim (GetRawText), so strings reach the tool as written.
    $document = [System.Text.Json.JsonDocument]::Parse($Json)
    try {
        $root = $document.RootElement
        if ([string]$root.ValueKind -cne 'Object') {
            Throw-PcvLane3PromotionDocsInvalid -Field 'spec' -Detail 'not-object'
        }
        $sections = @($script:PcvLane3Steps | Where-Object { $null -ne $_.spec } | ForEach-Object { $_.spec })
        $names = @($root.EnumerateObject() | ForEach-Object { $_.Name })
        foreach ($name in $names) {
            if ($name -cnotin @('schema_version', 'contract') + $sections) {
                Throw-PcvLane3PromotionDocsInvalid -Field $name -Detail 'unknown-field'
            }
        }
        $value = [System.Text.Json.JsonElement]::new()
        $number = 0
        if (-not $root.TryGetProperty('schema_version', [ref]$value) -or [string]$value.ValueKind -cne 'Number' -or
            -not $value.TryGetInt32([ref]$number) -or $number -ne 1) {
            Throw-PcvLane3PromotionDocsInvalid -Field 'schema_version' -Detail 'invalid'
        }
        if (-not $root.TryGetProperty('contract', [ref]$value) -or [string]$value.ValueKind -cne 'String' -or
            $value.GetString() -cne 'pcv-lane3-promotion-docs-v1') {
            Throw-PcvLane3PromotionDocsInvalid -Field 'contract' -Detail 'invalid'
        }
        $paths = [ordered]@{}
        foreach ($section in $sections) {
            if (-not $root.TryGetProperty($section, [ref]$value) -or [string]$value.ValueKind -cne 'Object') {
                Throw-PcvLane3PromotionDocsInvalid -Field $section -Detail 'missing'
            }
            $path = Join-Path $Directory "$section.json"
            [System.IO.File]::WriteAllText($path, $value.GetRawText(), [System.Text.UTF8Encoding]::new($false))
            $paths[$section] = $path
        }
        $paths
    }
    finally {
        $document.Dispose()
    }
}

function Get-PcvLane3StepArguments {
    param(
        [Parameter(Mandatory)][object]$Step,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][System.Collections.IDictionary]$SpecPaths,
        [Parameter(Mandatory)][ValidateSet('dry-run', 'apply', 'check')][string]$Mode
    )

    $arguments = [System.Collections.Generic.List[string]]::new()
    $arguments.AddRange([string[]]@('-NoProfile', '-File', (Join-Path $PSScriptRoot $Step.tool), '-RepoRoot', $RepoRoot))
    if ($null -ne $Step.spec) { $arguments.AddRange([string[]]@('-SpecPath', $SpecPaths[$Step.spec])) }
    if (@($Step.arguments).Count -gt 0) { $arguments.AddRange([string[]]$Step.arguments) }
    # Update-PcvCurrentEvidenceDocs.ps1 writes by default and has no dry-run; its -Check stands in.
    if ($Mode -ceq 'check' -or ($Mode -ceq 'dry-run' -and $Step.name -ceq 'current_evidence_docs')) { $arguments.Add('-Check') }
    elseif ($Mode -ceq 'apply' -and $Step.name -cne 'current_evidence_docs') { $arguments.Add('-Apply') }
    $arguments.ToArray()
}

function Invoke-PcvLane3Step {
    param(
        [Parameter(Mandatory)][object]$Step,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][System.Collections.IDictionary]$SpecPaths,
        [Parameter(Mandatory)][ValidateSet('dry-run', 'apply', 'check')][string]$Mode
    )

    $arguments = Get-PcvLane3StepArguments -Step $Step -RepoRoot $RepoRoot -SpecPaths $SpecPaths -Mode $Mode
    $output = @(& pwsh @arguments 2>&1 | ForEach-Object { [string]$_ })
    $exitCode = $LASTEXITCODE
    $result = $null
    try { $result = ($output -join "`n") | ConvertFrom-Json } catch { $result = $null }
    $errorText = if ($null -ne $result -and $result.PSObject.Properties.Name -contains 'error') { [string]$result.error } else { $null }
    $status = if ($exitCode -eq 0) {
        if ($null -ne $result -and $result.PSObject.Properties.Name -contains 'status') { [string]$result.status } else { 'ok' }
    }
    elseif ($null -ne $errorText -and $errorText -match '^PCV_[A-Z_]+_STALE\|') { 'stale' }
    else { 'failed' }
    [pscustomobject]([ordered]@{
        step = $Step.name
        status = $status
        exit_code = $exitCode
        error = if ($status -ceq 'failed') { if ($null -ne $errorText) { $errorText } else { ($output -join "`n") } } else { $null }
    })
}

function Invoke-PcvLane3PromotionDocs {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Json,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][ValidateSet('dry-run', 'apply', 'check')][string]$Mode
    )

    $directory = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-lane3-promotion-docs-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $directory | Out-Null
    try {
        $specPaths = Split-PcvLane3PromotionSpec -Json $Json -Directory $directory
        $results = [System.Collections.Generic.List[object]]::new()
        if ($Mode -ceq 'apply') {
            # Validate every spec-driven step on the current tree before the first write.
            foreach ($step in @($script:PcvLane3Steps | Where-Object { $null -ne $_.spec })) {
                $preflight = Invoke-PcvLane3Step -Step $step -RepoRoot $RepoRoot -SpecPaths $specPaths -Mode 'dry-run'
                if ($preflight.status -cne 'planned') {
                    $preflight.step = "preflight.$($step.name)"
                    $results.Add($preflight)
                    return [pscustomobject]@{ ok = $false; results = @($results.ToArray()) }
                }
            }
        }
        foreach ($step in $script:PcvLane3Steps) {
            $result = Invoke-PcvLane3Step -Step $step -RepoRoot $RepoRoot -SpecPaths $specPaths -Mode $Mode
            $results.Add($result)
            if ($Mode -ceq 'apply' -and $result.status -ceq 'failed') { break }
        }
        $ok = if ($Mode -ceq 'check') {
            @($results | Where-Object { $_.status -cne 'current' -and $_.status -cne 'ok' }).Count -eq 0
        }
        else {
            @($results | Where-Object { $_.status -ceq 'failed' }).Count -eq 0
        }
        [pscustomobject]@{ ok = $ok; results = @($results.ToArray()) }
    }
    finally {
        Remove-Item -LiteralPath $directory -Recurse -Force -ErrorAction SilentlyContinue
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    $mode = if ($Check) { 'check' } elseif ($Apply) { 'apply' } else { 'dry-run' }
    try {
        if ($Apply -and $Check) {
            throw 'PCV_LANE3_PROMOTION_DOCS_USAGE|apply-and-check-are-exclusive'
        }
        if ([string]::IsNullOrWhiteSpace($SpecPath)) {
            throw 'PCV_LANE3_PROMOTION_DOCS_USAGE|spec-path-required'
        }
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $run = Invoke-PcvLane3PromotionDocs -Json ([System.IO.File]::ReadAllText((Resolve-Path -LiteralPath $SpecPath).Path)) -RepoRoot $resolvedRepoRoot -Mode $mode
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $run.ok
            mode = $mode
            steps = $run.results
        }) | ConvertTo-Json -Depth 6 -Compress
        if (-not $run.ok) { exit 1 }
    }
    catch {
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $false
            mode = $mode
            error = [string]$_
        }) | ConvertTo-Json -Depth 6 -Compress
        exit 1
    }
}
