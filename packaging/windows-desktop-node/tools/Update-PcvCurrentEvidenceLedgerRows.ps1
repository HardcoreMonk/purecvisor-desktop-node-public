[CmdletBinding()]
param(
    [string]$SpecPath,
    [string]$LedgerPath,
    [string]$RepoRoot,
    [switch]$Apply,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Rotates the status table under `## 현재 Anchor` in docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md the way
# every promotion did by hand (0.42.78: ff77943). A `supersede` row keeps its history: the current
# row is shortened to a predecessor row (first evidence path, batch id, `predecessor after <tag>
# promotion`) and the new row goes directly below it. A `replace` row has a single line that is
# swapped. Rows with the same id in older tables are historical and stay untouched. The ledger
# head keys (`current_*`) rotate with Update-PcvManualAdminDescriptorChain.ps1 -DescriptorPath.

$script:PcvLedgerAnchorTableHeader = '| ledger key | current 상태 | Evidence | 운영 규칙 |'
$script:PcvLedgerPredecessorNouns = @{
    'full-admin-host-mutation-current' = 'fullgate'
    'package-build-current' = 'package'
    'installed-operator-surface-smoke-latest' = 'current-card'
}

function Throw-PcvLedgerRowsInvalid {
    param(
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Detail
    )

    throw "PCV_LEDGER_ROWS_INVALID|$Field|$Detail"
}

function ConvertFrom-PcvLedgerRowsJsonElement {
    param([Parameter(Mandatory)][System.Text.Json.JsonElement]$Element)

    $number = [long]0
    switch ([string]$Element.ValueKind) {
        'String' { return $Element.GetString() }
        'Number' { if ($Element.TryGetInt64([ref]$number)) { return $number } return $null }
        'Object' {
            $values = [ordered]@{}
            foreach ($entry in $Element.EnumerateObject()) {
                $values[$entry.Name] = ConvertFrom-PcvLedgerRowsJsonElement -Element $entry.Value
            }
            return [pscustomobject]$values
        }
        'Array' {
            return , @(foreach ($item in $Element.EnumerateArray()) { ConvertFrom-PcvLedgerRowsJsonElement -Element $item })
        }
        default { return $null }
    }
}

function ConvertFrom-PcvLedgerRowsSpecJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Json)

    # JsonDocument keeps every string exactly as written (ConvertFrom-Json would turn ISO
    # timestamps into DateTime); non-string scalars become $null or [long] and fail validation.
    $document = [System.Text.Json.JsonDocument]::Parse($Json)
    try {
        ConvertFrom-PcvLedgerRowsJsonElement -Element $document.RootElement
    }
    finally {
        $document.Dispose()
    }
}

function Test-PcvLedgerRowsCell {
    param(
        [Parameter(Mandatory)][string]$Field,
        [AllowNull()][object]$Value
    )

    if ($Value -isnot [string] -or
        [string]::IsNullOrWhiteSpace($Value) -or
        $Value -match '[\r\n]' -or
        $Value.Contains(' | ') -or
        $Value -cne $Value.Trim()) {
        Throw-PcvLedgerRowsInvalid -Field $Field -Detail 'invalid-cell'
    }
}

function Test-PcvLedgerRowsSpec {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Spec)

    foreach ($name in @('schema_version', 'contract', 'promoted_tag', 'supersede', 'replace')) {
        if ($name -cnotin @($Spec.PSObject.Properties.Name)) {
            Throw-PcvLedgerRowsInvalid -Field $name -Detail 'missing'
        }
    }
    if (($Spec.schema_version -isnot [int] -and $Spec.schema_version -isnot [long]) -or $Spec.schema_version -ne 1) {
        Throw-PcvLedgerRowsInvalid -Field 'schema_version' -Detail ([string]$Spec.schema_version)
    }
    if ([string]$Spec.contract -cne 'pcv-current-evidence-ledger-rows-rotation-v1') {
        Throw-PcvLedgerRowsInvalid -Field 'contract' -Detail ([string]$Spec.contract)
    }
    if ([string]$Spec.promoted_tag -cnotmatch '^0\d{4}$') {
        Throw-PcvLedgerRowsInvalid -Field 'promoted_tag' -Detail ([string]$Spec.promoted_tag)
    }
    foreach ($list in @('supersede', 'replace')) {
        if ($Spec.$list -isnot [array]) {
            Throw-PcvLedgerRowsInvalid -Field $list -Detail 'not-array'
        }
    }
    if (@($Spec.supersede).Count + @($Spec.replace).Count -eq 0) {
        Throw-PcvLedgerRowsInvalid -Field 'supersede' -Detail 'empty'
    }
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($list in @('supersede', 'replace')) {
        $position = 0
        foreach ($entry in @($Spec.$list)) {
            $field = "$list[$position]"
            if ($entry -isnot [System.Management.Automation.PSCustomObject]) {
                Throw-PcvLedgerRowsInvalid -Field $field -Detail 'not-object'
            }
            $names = @($entry.PSObject.Properties.Name)
            if ('id' -cnotin $names -or [string]$entry.id -cnotmatch '^[a-z0-9]+(-[a-z0-9]+)*$') {
                Throw-PcvLedgerRowsInvalid -Field "$field.id" -Detail 'invalid-id'
            }
            if (-not $seen.Add([string]$entry.id)) {
                Throw-PcvLedgerRowsInvalid -Field "$field.id" -Detail "duplicate:$($entry.id)"
            }
            foreach ($cell in @('status', 'evidence', 'rule')) {
                Test-PcvLedgerRowsCell -Field "$field.$cell" -Value $(if ($cell -cin $names) { $entry.$cell } else { $null })
            }
            $allowed = if ($list -ceq 'supersede') { @('id', 'status', 'evidence', 'rule', 'predecessor_rule') } else { @('id', 'status', 'evidence', 'rule') }
            foreach ($name in $names) {
                if ($name -cnotin $allowed) {
                    Throw-PcvLedgerRowsInvalid -Field "$field.$name" -Detail 'unknown-field'
                }
            }
            if ('predecessor_rule' -cin $names) {
                Test-PcvLedgerRowsCell -Field "$field.predecessor_rule" -Value $entry.predecessor_rule
            }
            $position++
        }
    }
    $Spec
}

function Get-PcvLedgerAnchorTable {
    param([Parameter(Mandatory)][AllowEmptyString()][System.Collections.Generic.List[string]]$Lines)

    $headers = @(for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -ceq $script:PcvLedgerAnchorTableHeader) { $index }
    })
    if ($headers.Count -ne 1) {
        Throw-PcvLedgerRowsInvalid -Field 'ledger' -Detail "anchor-table-count:$($headers.Count)"
    }
    $start = $headers[0] + 2
    $end = $start
    while ($end -lt $Lines.Count -and $Lines[$end].StartsWith('|')) { $end++ }
    [pscustomobject]@{ start = $start; end = $end }
}

function Find-PcvLedgerRowIndexes {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][System.Collections.Generic.List[string]]$Lines,
        [Parameter(Mandatory)][object]$Table,
        [Parameter(Mandatory)][string]$Id
    )

    $prefix = "| ``$Id`` | "
    @(for ($index = $Table.start; $index -lt $Table.end; $index++) {
        if ($Lines[$index].StartsWith($prefix, [System.StringComparison]::Ordinal)) { $index }
    })
}

function ConvertFrom-PcvLedgerRow {
    param(
        [Parameter(Mandatory)][string]$Line,
        [Parameter(Mandatory)][string]$Id
    )

    $cells = $Line -split ' \| '
    if ($cells.Count -ne 4 -or $cells[0] -cne "| ``$Id``" -or -not $cells[3].EndsWith(' |')) {
        Throw-PcvLedgerRowsInvalid -Field $Id -Detail 'unsupported-row-format'
    }
    [pscustomobject]@{
        status = $cells[1]
        evidence = $cells[2]
        rule = $cells[3].Substring(0, $cells[3].Length - 2)
    }
}

function Format-PcvLedgerRow {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Status,
        [Parameter(Mandatory)][string]$Evidence,
        [Parameter(Mandatory)][string]$Rule
    )

    "| ``$Id`` | $Status | $Evidence | $Rule |"
}

function ConvertTo-PcvLedgerVersionTag {
    param([Parameter(Mandatory)][string]$Version)

    if ($Version -cnotmatch '^(\d+)\.(\d+)\.(\d+)-admin-smoke$') {
        Throw-PcvLedgerRowsInvalid -Field 'version' -Detail $Version
    }
    '{0}{1}{2}' -f $Matches[1], $Matches[2], $Matches[3].PadLeft(2, '0')
}

function Get-PcvLedgerPredecessorRule {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Status
    )

    if ($Status -cmatch '`([0-9.]+-admin-smoke) -> ([0-9.]+-admin-smoke)`') {
        $from = ConvertTo-PcvLedgerVersionTag -Version $Matches[1]
        $to = ConvertTo-PcvLedgerVersionTag -Version $Matches[2]
        return "$from→$to pair PASS는 predecessor다."
    }
    if ($script:PcvLedgerPredecessorNouns.ContainsKey($Id) -and $Status -cmatch '`([0-9.]+-admin-smoke)`') {
        $tag = ConvertTo-PcvLedgerVersionTag -Version $Matches[1]
        return "$tag $($script:PcvLedgerPredecessorNouns[$Id]) PASS는 predecessor다."
    }
    Throw-PcvLedgerRowsInvalid -Field $Id -Detail 'predecessor-rule-required'
}

function Get-PcvLedgerPredecessorEvidence {
    param(
        [Parameter(Mandatory)][string]$Evidence,
        [Parameter(Mandatory)][string]$PromotedTag
    )

    # Keep the evidence document and, for a fullgate, the batch id (the `artifacts/batch-runs/`
    # leaf); hashes, descriptors, and summary paths stay in the evidence document.
    $items = @($Evidence -split '; ')
    $kept = [System.Collections.Generic.List[string]]::new()
    $kept.Add($items[0])
    if ($items.Count -gt 1 -and $items[1] -cmatch '^`artifacts/batch-runs/([^`/]+)`$') {
        $kept.Add("``$($Matches[1])``")
    }
    $kept.Add("predecessor after $PromotedTag promotion")
    $kept -join '; '
}

function Get-PcvLedgerSupersedeState {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][System.Collections.Generic.List[string]]$Lines,
        [Parameter(Mandatory)][object]$Table,
        [Parameter(Mandatory)][object]$Entry
    )

    $indexes = @(Find-PcvLedgerRowIndexes -Lines $Lines -Table $Table -Id $Entry.id)
    $current = @($indexes | Where-Object {
        -not (ConvertFrom-PcvLedgerRow -Line $Lines[$_] -Id $Entry.id).evidence.Contains('predecessor after ')
    })
    [pscustomobject]@{ indexes = $indexes; current = $current }
}

function Invoke-PcvLedgerRowsRotation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][object]$Spec
    )

    [void](Test-PcvLedgerRowsSpec -Spec $Spec)
    $list = [System.Collections.Generic.List[string]]::new([string[]]$Lines)
    $table = Get-PcvLedgerAnchorTable -Lines $list
    $changes = [System.Collections.Generic.List[object]]::new()
    $tag = [string]$Spec.promoted_tag

    foreach ($entry in @($Spec.supersede)) {
        $state = Get-PcvLedgerSupersedeState -Lines $list -Table $table -Entry $entry
        if ($state.current.Count -ne 1) {
            Throw-PcvLedgerRowsInvalid -Field "supersede.$($entry.id)" -Detail "current-row-count:$($state.current.Count)"
        }
        $index = $state.current[0]
        $newRow = Format-PcvLedgerRow -Id $entry.id -Status $entry.status -Evidence $entry.evidence -Rule $entry.rule
        if ($list[$index] -ceq $newRow) {
            Throw-PcvLedgerRowsInvalid -Field "supersede.$($entry.id)" -Detail 'already-rotated'
        }
        $old = ConvertFrom-PcvLedgerRow -Line $list[$index] -Id $entry.id
        $rule = if ('predecessor_rule' -cin @($entry.PSObject.Properties.Name)) {
            [string]$entry.predecessor_rule
        }
        else {
            Get-PcvLedgerPredecessorRule -Id $entry.id -Status $old.status
        }
        $evidence = Get-PcvLedgerPredecessorEvidence -Evidence $old.evidence -PromotedTag $tag
        $list[$index] = Format-PcvLedgerRow -Id $entry.id -Status $old.status -Evidence $evidence -Rule $rule
        $list.Insert($index + 1, $newRow)
        $table.end++
        $changes.Add([pscustomobject]([ordered]@{ id = $entry.id; action = 'supersede'; line = $index + 1 }))
    }

    foreach ($entry in @($Spec.replace)) {
        $indexes = @(Find-PcvLedgerRowIndexes -Lines $list -Table $table -Id $entry.id)
        if ($indexes.Count -ne 1) {
            Throw-PcvLedgerRowsInvalid -Field "replace.$($entry.id)" -Detail "row-count:$($indexes.Count)"
        }
        $newRow = Format-PcvLedgerRow -Id $entry.id -Status $entry.status -Evidence $entry.evidence -Rule $entry.rule
        if ($list[$indexes[0]] -ceq $newRow) {
            Throw-PcvLedgerRowsInvalid -Field "replace.$($entry.id)" -Detail 'already-rotated'
        }
        [void](ConvertFrom-PcvLedgerRow -Line $list[$indexes[0]] -Id $entry.id)
        $list[$indexes[0]] = $newRow
        $changes.Add([pscustomobject]([ordered]@{ id = $entry.id; action = 'replace'; line = $indexes[0] + 1 }))
    }

    [pscustomobject]@{
        lines = $list.ToArray()
        changes = @($changes.ToArray())
    }
}

function Get-PcvLedgerRowsStaleIds {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][object]$Spec
    )

    [void](Test-PcvLedgerRowsSpec -Spec $Spec)
    $list = [System.Collections.Generic.List[string]]::new([string[]]$Lines)
    $table = Get-PcvLedgerAnchorTable -Lines $list
    $marker = "; predecessor after $($Spec.promoted_tag) promotion | "
    @(foreach ($entry in @($Spec.supersede)) {
        $state = Get-PcvLedgerSupersedeState -Lines $list -Table $table -Entry $entry
        $newRow = Format-PcvLedgerRow -Id $entry.id -Status $entry.status -Evidence $entry.evidence -Rule $entry.rule
        $current = $state.current.Count -eq 1 -and
            $list[$state.current[0]] -ceq $newRow -and
            $state.current[0] - 1 -in $state.indexes -and
            $list[$state.current[0] - 1].Contains($marker)
        if (-not $current) { $entry.id }
    }
    foreach ($entry in @($Spec.replace)) {
        $indexes = @(Find-PcvLedgerRowIndexes -Lines $list -Table $table -Id $entry.id)
        $newRow = Format-PcvLedgerRow -Id $entry.id -Status $entry.status -Evidence $entry.evidence -Rule $entry.rule
        if (-not ($indexes.Count -eq 1 -and $list[$indexes[0]] -ceq $newRow)) { $entry.id }
    })
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        if ($Apply -and $Check) {
            throw 'PCV_LEDGER_ROWS_USAGE|apply-and-check-are-exclusive'
        }
        if ([string]::IsNullOrWhiteSpace($SpecPath)) {
            throw 'PCV_LEDGER_ROWS_USAGE|spec-path-required'
        }
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $resolvedLedgerPath = if ([string]::IsNullOrWhiteSpace($LedgerPath)) {
            Join-Path $resolvedRepoRoot 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        }
        elseif ([System.IO.Path]::IsPathRooted($LedgerPath)) {
            $LedgerPath
        }
        else {
            Join-Path $resolvedRepoRoot $LedgerPath
        }
        $spec = ConvertFrom-PcvLedgerRowsSpecJson -Json (Get-Content -Raw -LiteralPath $SpecPath)
        $original = [System.IO.File]::ReadAllText($resolvedLedgerPath)
        $newline = if ($original.Contains("`r`n")) { "`r`n" } else { "`n" }
        $lines = $original.Replace("`r`n", "`n").Split("`n")
        $mode = if ($Check) { 'check' } elseif ($Apply) { 'apply' } else { 'dry-run' }

        if ($Check) {
            $staleIds = @(Get-PcvLedgerRowsStaleIds -Lines $lines -Spec $spec)
            if ($staleIds.Count -ne 0) {
                throw "PCV_LEDGER_ROWS_STALE|$($staleIds -join ',')"
            }
            [pscustomobject]([ordered]@{
                schema_version = 1
                ok = $true
                mode = $mode
                ledger = $resolvedLedgerPath
                status = 'current'
            }) | ConvertTo-Json -Depth 6 -Compress
            return
        }

        $rotation = Invoke-PcvLedgerRowsRotation -Lines $lines -Spec $spec
        if ($Apply) {
            $output = ($rotation.lines -join "`n").Replace("`n", $newline)
            $temporaryPath = "$resolvedLedgerPath.tmp"
            try {
                [System.IO.File]::WriteAllText($temporaryPath, $output, [System.Text.UTF8Encoding]::new($false))
                Move-Item -LiteralPath $temporaryPath -Destination $resolvedLedgerPath -Force
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
            ledger = $resolvedLedgerPath
            status = if ($Apply) { 'updated' } else { 'planned' }
            superseded_count = @($rotation.changes | Where-Object { $_.action -ceq 'supersede' }).Count
            replaced_count = @($rotation.changes | Where-Object { $_.action -ceq 'replace' }).Count
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
