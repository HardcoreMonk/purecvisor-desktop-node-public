[CmdletBinding()]
param(
    [string]$SpecPath,
    [string]$DescriptorPath,
    [string]$RepoRoot,
    [switch]$Apply,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Rotates the head key block of docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md the way
# every promotion did by hand: each listed key takes its new value, and its old value moves to a
# `previous_<tag>_<key>` line directly below it. `next_*` keys are tagged with the version being
# promoted, every other key with the version being retired (0.42.78 promotion, ff77943).
# Only the first line-anchored occurrence of a key is current; later ones are historical records
# and stay untouched, which matches PcvManualAdminDescriptorCurrency.Tests.ps1.

function Throw-PcvDescriptorChainInvalid {
    param(
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Detail
    )

    throw "PCV_DESCRIPTOR_CHAIN_INVALID|$Field|$Detail"
}

function ConvertFrom-PcvDescriptorChainSpecJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Json)

    # ConvertFrom-Json turns ISO timestamps such as updated_at into DateTime, which would be
    # written back in a different format. JsonDocument keeps every string exactly as written;
    # any non-string value becomes $null so Test-PcvDescriptorChainSpec rejects it.
    $document = [System.Text.Json.JsonDocument]::Parse($Json)
    try {
        $spec = [ordered]@{}
        foreach ($property in $document.RootElement.EnumerateObject()) {
            $number = [long]0
            $spec[$property.Name] = switch ([string]$property.Value.ValueKind) {
                'String' { $property.Value.GetString() }
                'Number' { if ($property.Value.TryGetInt64([ref]$number)) { $number } else { $null } }
                'Object' {
                    $values = [ordered]@{}
                    foreach ($entry in $property.Value.EnumerateObject()) {
                        $values[$entry.Name] = if ([string]$entry.Value.ValueKind -ceq 'String') { $entry.Value.GetString() } else { $null }
                    }
                    [pscustomobject]$values
                }
                default { $null }
            }
        }
        [pscustomobject]$spec
    }
    finally {
        $document.Dispose()
    }
}

function Test-PcvDescriptorChainSpec {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Spec)

    foreach ($name in @('schema_version', 'contract', 'previous_tag', 'next_previous_tag', 'values')) {
        if ($name -cnotin @($Spec.PSObject.Properties.Name)) {
            Throw-PcvDescriptorChainInvalid -Field $name -Detail 'missing'
        }
    }
    if (($Spec.schema_version -isnot [int] -and $Spec.schema_version -isnot [long]) -or $Spec.schema_version -ne 1) {
        Throw-PcvDescriptorChainInvalid -Field 'schema_version' -Detail ([string]$Spec.schema_version)
    }
    if ([string]$Spec.contract -cne 'pcv-manual-admin-descriptor-chain-rotation-v1') {
        Throw-PcvDescriptorChainInvalid -Field 'contract' -Detail ([string]$Spec.contract)
    }
    foreach ($name in @('previous_tag', 'next_previous_tag')) {
        if ([string]$Spec.$name -cnotmatch '^0\d{4}$') {
            Throw-PcvDescriptorChainInvalid -Field $name -Detail ([string]$Spec.$name)
        }
    }
    $properties = @($Spec.values.PSObject.Properties)
    if ($properties.Count -eq 0) {
        Throw-PcvDescriptorChainInvalid -Field 'values' -Detail 'empty'
    }
    foreach ($property in $properties) {
        if ($property.Name -cnotmatch '^[a-z0-9_]+$' -or
            $property.Name.StartsWith('previous_') -or
            $property.Name.StartsWith('historical_')) {
            Throw-PcvDescriptorChainInvalid -Field "values.$($property.Name)" -Detail 'invalid-key'
        }
        # Numbers such as run ids must arrive as strings; ConvertFrom-Json would otherwise
        # reformat large values.
        if ($property.Value -isnot [string] -or
            [string]::IsNullOrWhiteSpace($property.Value) -or
            $property.Value -match '[`\r\n]') {
            Throw-PcvDescriptorChainInvalid -Field "values.$($property.Name)" -Detail 'invalid-value'
        }
    }
    $Spec
}

function Get-PcvDescriptorChainTag {
    param(
        [Parameter(Mandatory)][object]$Spec,
        [Parameter(Mandatory)][string]$Key
    )

    if ($Key.StartsWith('next_')) { [string]$Spec.next_previous_tag } else { [string]$Spec.previous_tag }
}

function Get-PcvDescriptorHeadBlock {
    param([Parameter(Mandatory)][AllowEmptyString()][System.Collections.Generic.List[string]]$Lines)

    # The key block starts at the first `key: ` line and ends at the first dated `## ` section.
    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -cmatch '^[a-z0-9_]+: ') { $start = $index; break }
    }
    if ($start -lt 0) {
        Throw-PcvDescriptorChainInvalid -Field 'descriptor' -Detail 'missing-key-block'
    }
    $end = $Lines.Count
    for ($index = $start + 1; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index].StartsWith('## ')) { $end = $index; break }
    }
    [pscustomobject]@{ start = $start; end = $end }
}

function Find-PcvDescriptorKeyLine {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][System.Collections.Generic.List[string]]$Lines,
        [Parameter(Mandatory)][object]$Block,
        [Parameter(Mandatory)][string]$Key
    )

    $prefix = "${Key}: "
    $indexes = @(for ($index = $Block.start; $index -lt $Block.end; $index++) {
        if ($Lines[$index].StartsWith($prefix, [System.StringComparison]::Ordinal)) { $index }
    })
    $indexes
}

function Invoke-PcvDescriptorChainRotation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][object]$Spec
    )

    [void](Test-PcvDescriptorChainSpec -Spec $Spec)
    $list = [System.Collections.Generic.List[string]]::new([string[]]$Lines)
    $block = Get-PcvDescriptorHeadBlock -Lines $list
    $changes = [System.Collections.Generic.List[object]]::new()
    $duplicates = [System.Collections.Generic.List[string]]::new()
    foreach ($property in @($Spec.values.PSObject.Properties)) {
        $key = $property.Name
        $tag = Get-PcvDescriptorChainTag -Spec $Spec -Key $key
        $previousKey = "previous_${tag}_$key"
        $indexes = @(Find-PcvDescriptorKeyLine -Lines $list -Block $block -Key $key)
        if ($indexes.Count -eq 0) {
            Throw-PcvDescriptorChainInvalid -Field "values.$key" -Detail 'missing-in-descriptor'
        }
        if ($indexes.Count -gt 1) { $duplicates.Add($key) }
        if (@(Find-PcvDescriptorKeyLine -Lines $list -Block $block -Key $previousKey).Count -ne 0) {
            Throw-PcvDescriptorChainInvalid -Field "values.$key" -Detail "already-rotated:$previousKey"
        }
        $index = $indexes[0]
        $pattern = '^' + [regex]::Escape($key) + ': `([^`]*)`$'
        if ($list[$index] -cmatch $pattern) {
            $oldValue = $Matches[1]
        }
        else {
            Throw-PcvDescriptorChainInvalid -Field "values.$key" -Detail 'unsupported-value-format'
        }
        $newValue = [string]$property.Value
        $list[$index] = "${key}: ``$newValue``"
        $list.Insert($index + 1, "${previousKey}: ``$oldValue``")
        $block.end++
        $changes.Add([pscustomobject]([ordered]@{
            key = $key
            previous_key = $previousKey
            old = $oldValue
            new = $newValue
            value_changed = $oldValue -cne $newValue
        }))
    }
    [pscustomobject]@{
        lines = $list.ToArray()
        changes = @($changes.ToArray())
        duplicate_keys = @($duplicates.ToArray())
    }
}

function Get-PcvDescriptorChainStaleKeys {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][object]$Spec
    )

    [void](Test-PcvDescriptorChainSpec -Spec $Spec)
    $list = [System.Collections.Generic.List[string]]::new([string[]]$Lines)
    $block = Get-PcvDescriptorHeadBlock -Lines $list
    @(foreach ($property in @($Spec.values.PSObject.Properties)) {
        $key = $property.Name
        $tag = Get-PcvDescriptorChainTag -Spec $Spec -Key $key
        $indexes = @(Find-PcvDescriptorKeyLine -Lines $list -Block $block -Key $key)
        $current = $indexes.Count -gt 0 -and
            $list[$indexes[0]] -ceq "${key}: ``$($property.Value)``" -and
            $indexes[0] + 1 -lt $block.end -and
            $list[$indexes[0] + 1].StartsWith("previous_${tag}_${key}: ``", [System.StringComparison]::Ordinal)
        if (-not $current) { $key }
    })
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        if ($Apply -and $Check) {
            throw 'PCV_DESCRIPTOR_CHAIN_USAGE|apply-and-check-are-exclusive'
        }
        if ([string]::IsNullOrWhiteSpace($SpecPath)) {
            throw 'PCV_DESCRIPTOR_CHAIN_USAGE|spec-path-required'
        }
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $resolvedDescriptorPath = if ([string]::IsNullOrWhiteSpace($DescriptorPath)) {
            Join-Path $resolvedRepoRoot 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        }
        elseif ([System.IO.Path]::IsPathRooted($DescriptorPath)) {
            $DescriptorPath
        }
        else {
            Join-Path $resolvedRepoRoot $DescriptorPath
        }
        $spec = ConvertFrom-PcvDescriptorChainSpecJson -Json (Get-Content -Raw -LiteralPath $SpecPath)
        $original = [System.IO.File]::ReadAllText($resolvedDescriptorPath)
        $newline = if ($original.Contains("`r`n")) { "`r`n" } else { "`n" }
        $lines = $original.Replace("`r`n", "`n").Split("`n")
        $mode = if ($Check) { 'check' } elseif ($Apply) { 'apply' } else { 'dry-run' }

        if ($Check) {
            $staleKeys = @(Get-PcvDescriptorChainStaleKeys -Lines $lines -Spec $spec)
            if ($staleKeys.Count -ne 0) {
                throw "PCV_DESCRIPTOR_CHAIN_STALE|$($staleKeys -join ',')"
            }
            [pscustomobject]([ordered]@{
                schema_version = 1
                ok = $true
                mode = $mode
                descriptor = $resolvedDescriptorPath
                status = 'current'
            }) | ConvertTo-Json -Depth 6 -Compress
            return
        }

        $rotation = Invoke-PcvDescriptorChainRotation -Lines $lines -Spec $spec
        if ($Apply) {
            $output = ($rotation.lines -join "`n").Replace("`n", $newline)
            $temporaryPath = "$resolvedDescriptorPath.tmp"
            try {
                [System.IO.File]::WriteAllText($temporaryPath, $output, [System.Text.UTF8Encoding]::new($false))
                Move-Item -LiteralPath $temporaryPath -Destination $resolvedDescriptorPath -Force
            }
            finally {
                if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
                    Remove-Item -LiteralPath $temporaryPath -Force
                }
            }
        }
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $true
            mode = $mode
            descriptor = $resolvedDescriptorPath
            status = if ($Apply) { 'updated' } else { 'planned' }
            rotated_key_count = @($rotation.changes).Count
            value_changed_count = @($rotation.changes | Where-Object { $_.value_changed }).Count
            duplicate_keys = @($rotation.duplicate_keys)
            changes = @($rotation.changes)
        }) | ConvertTo-Json -Depth 6 -Compress
    }
    catch {
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $false
            mode = if ($Check) { 'check' } elseif ($Apply) { 'apply' } else { 'dry-run' }
            error = [string]$_
        }) | ConvertTo-Json -Depth 6 -Compress
        exit 1
    }
}
