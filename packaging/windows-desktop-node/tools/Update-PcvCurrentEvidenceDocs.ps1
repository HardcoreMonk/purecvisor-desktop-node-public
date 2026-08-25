[CmdletBinding()]
param(
    [string]$EvidencePath,
    [string]$RepoRoot,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Throw-PcvCurrentEvidenceInvalid {
    param(
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Detail
    )

    throw "PCV_CURRENT_EVIDENCE_INVALID|$Field|$Detail"
}

function Get-PcvCurrentEvidenceProperty {
    param(
        [Parameter(Mandatory)][object]$Value,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Field
    )

    if ($null -eq $Value -or $Name -cnotin @($Value.PSObject.Properties.Name)) {
        Throw-PcvCurrentEvidenceInvalid -Field $Field -Detail 'missing'
    }
    $Value.$Name
}

function Assert-PcvCurrentEvidencePropertySet {
    param(
        [Parameter(Mandatory)][object]$Value,
        [Parameter(Mandatory)][string[]]$AllowedNames,
        [Parameter(Mandatory)][string]$Field
    )

    foreach ($propertyName in @($Value.PSObject.Properties.Name)) {
        if ($propertyName -cnotin $AllowedNames) {
            Throw-PcvCurrentEvidenceInvalid -Field "$Field.$propertyName" -Detail 'unexpected'
        }
    }
}

function Assert-PcvCurrentEvidenceSchemaVersion {
    param(
        [Parameter(Mandatory)][object]$Value,
        [Parameter(Mandatory)][string]$Field
    )

    $isNumeric = $Value -is [byte] -or
        $Value -is [sbyte] -or
        $Value -is [int16] -or
        $Value -is [uint16] -or
        $Value -is [int32] -or
        $Value -is [uint32] -or
        $Value -is [int64] -or
        $Value -is [uint64] -or
        $Value -is [single] -or
        $Value -is [double] -or
        $Value -is [decimal]
    if (-not $isNumeric -or $Value -ne 1) {
        Throw-PcvCurrentEvidenceInvalid -Field $Field -Detail ([string]$Value)
    }
}

function Test-PcvFeatureQualification {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Qualification)

    $schemaVersion = Get-PcvCurrentEvidenceProperty -Value $Qualification -Name 'schema_version' -Field 'feature_qualification.schema_version'
    Assert-PcvCurrentEvidenceSchemaVersion `
        -Value $schemaVersion `
        -Field 'feature_qualification.schema_version'
    $contract = [string](Get-PcvCurrentEvidenceProperty -Value $Qualification -Name 'contract' -Field 'feature_qualification.contract')
    if ($contract -cne 'pcv-feature-promotion-decision-v1') {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.contract' -Detail $contract
    }
    $eligible = Get-PcvCurrentEvidenceProperty -Value $Qualification -Name 'promotion_eligible' -Field 'feature_qualification.promotion_eligible'
    if ($eligible -isnot [bool]) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.promotion_eligible' -Detail 'must-be-boolean'
    }
    [void](Get-PcvCurrentEvidenceProperty -Value $Qualification -Name 'blockers' -Field 'feature_qualification.blockers')
    Assert-PcvCurrentEvidencePropertySet `
        -Value $Qualification `
        -AllowedNames @('schema_version', 'contract', 'promotion_eligible', 'blockers') `
        -Field 'feature_qualification'
    $rawBlockers = $Qualification.PSObject.Properties['blockers'].Value
    if ($rawBlockers -isnot [array]) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers' -Detail 'must-be-array'
    }
    $blockers = @($rawBlockers)
    if ([bool]$eligible -and $blockers.Count -ne 0) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers' -Detail 'eligible-must-be-empty'
    }
    if (-not [bool]$eligible -and $blockers.Count -eq 0) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers' -Detail 'blocked-must-not-be-empty'
    }
    $stages = @('code_tested', 'packaged', 'installed_tested', 'actual_vm_tested', 'manual_admin_tested')
    foreach ($blocker in $blockers) {
        $featureId = [string](Get-PcvCurrentEvidenceProperty -Value $blocker -Name 'feature_id' -Field 'feature_qualification.blockers.feature_id')
        $stage = [string](Get-PcvCurrentEvidenceProperty -Value $blocker -Name 'stage' -Field 'feature_qualification.blockers.stage')
        $verdict = [string](Get-PcvCurrentEvidenceProperty -Value $blocker -Name 'verdict' -Field 'feature_qualification.blockers.verdict')
        Assert-PcvCurrentEvidencePropertySet `
            -Value $blocker `
            -AllowedNames @('feature_id', 'stage', 'verdict') `
            -Field 'feature_qualification.blockers'
        if ($featureId -cnotmatch '^pcv\.[a-z0-9._-]+$') {
            Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers.feature_id' -Detail $featureId
        }
        if ($stage -cnotin $stages) {
            Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers.stage' -Detail $stage
        }
        if ($verdict -cnotin @('fail', 'blocked', 'missing')) {
            Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers.verdict' -Detail $verdict
        }
    }
    $Qualification
}

function Assert-PcvFeaturePromotionAllowed {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][object]$ProposedRecord,
        [Parameter(Mandatory)][object]$CanonicalRecord
    )

    $proposedCurrent = Get-PcvCurrentEvidenceProperty -Value $ProposedRecord -Name 'current' -Field 'current'
    $canonicalCurrent = Get-PcvCurrentEvidenceProperty -Value $CanonicalRecord -Name 'current' -Field 'current'
    $qualification = Get-PcvCurrentEvidenceProperty -Value $ProposedRecord -Name 'feature_qualification' -Field 'feature_qualification'
    [void](Test-PcvFeatureQualification -Qualification $qualification)
    $proposedVersion = [string](Get-PcvCurrentEvidenceProperty -Value $proposedCurrent -Name 'version' -Field 'current.version')
    $canonicalVersion = [string](Get-PcvCurrentEvidenceProperty -Value $canonicalCurrent -Name 'version' -Field 'current.version')
    if ($proposedVersion -cne $canonicalVersion -and -not [bool]$qualification.promotion_eligible) {
        throw "PCV_FEATURE_PROMOTION_BLOCKED|$proposedVersion|blockers=$(@($qualification.blockers).Count)"
    }
}

function Test-PcvCurrentEvidenceRecord {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][object]$Record,
        [Parameter(Mandatory)][string]$RepoRoot
    )

    $schemaVersion = Get-PcvCurrentEvidenceProperty -Value $Record -Name 'schema_version' -Field 'schema_version'
    Assert-PcvCurrentEvidenceSchemaVersion -Value $schemaVersion -Field 'schema_version'
    $contract = [string](Get-PcvCurrentEvidenceProperty -Value $Record -Name 'contract' -Field 'contract')
    if ($contract -ne 'pcv-current-evidence-v1') {
        Throw-PcvCurrentEvidenceInvalid -Field 'contract' -Detail $contract
    }

    $current = Get-PcvCurrentEvidenceProperty -Value $Record -Name 'current' -Field 'current'
    $qualification = Get-PcvCurrentEvidenceProperty -Value $Record -Name 'feature_qualification' -Field 'feature_qualification'
    $manualAdmin = Get-PcvCurrentEvidenceProperty -Value $Record -Name 'manual_admin' -Field 'manual_admin'
    $claims = Get-PcvCurrentEvidenceProperty -Value $Record -Name 'claims' -Field 'claims'
    [void](Test-PcvFeatureQualification -Qualification $qualification)

    $version = [string](Get-PcvCurrentEvidenceProperty -Value $current -Name 'version' -Field 'current.version')
    if ($version -cnotmatch '^0\.\d+\.\d+-admin-smoke$') {
        Throw-PcvCurrentEvidenceInvalid -Field 'current.version' -Detail $version
    }
    $surfaces = @(Get-PcvCurrentEvidenceProperty -Value $current -Name 'operator_surfaces' -Field 'current.operator_surfaces')
    if ($surfaces.Count -ne 2 -or $surfaces[0] -ne 'web' -or $surfaces[1] -ne 'cli' -or @($surfaces | Select-Object -Unique).Count -ne 2) {
        Throw-PcvCurrentEvidenceInvalid -Field 'current.operator_surfaces' -Detail ($surfaces -join ',')
    }
    $tuiPresent = Get-PcvCurrentEvidenceProperty -Value $current -Name 'tui_present' -Field 'current.tui_present'
    if ([bool]$tuiPresent) {
        Throw-PcvCurrentEvidenceInvalid -Field 'current.tui_present' -Detail 'must-be-false'
    }

    foreach ($field in @('clean_msi_sha256', 'operational_msi_sha256', 'payload_sha256')) {
        $value = [string](Get-PcvCurrentEvidenceProperty -Value $current -Name $field -Field "current.$field")
        if ($value -notmatch '^[0-9a-f]{64}$') {
            Throw-PcvCurrentEvidenceInvalid -Field "current.$field" -Detail 'invalid-sha256'
        }
    }
    $commit = [string](Get-PcvCurrentEvidenceProperty -Value $current -Name 'provenance_commit' -Field 'current.provenance_commit')
    if ($commit -notmatch '^[0-9a-f]{40}$') {
        Throw-PcvCurrentEvidenceInvalid -Field 'current.provenance_commit' -Detail 'invalid-commit'
    }

    foreach ($field in @('package_evidence', 'fullgate_evidence', 'functional_evidence', 'installed_evidence')) {
        $relativePath = [string](Get-PcvCurrentEvidenceProperty -Value $current -Name $field -Field "current.$field")
        if ($relativePath -notmatch '^docs/ga-ready/evidence/.+\.md$') {
            Throw-PcvCurrentEvidenceInvalid -Field "current.$field" -Detail 'invalid-path'
        }
        $fullPath = Join-Path $RepoRoot $relativePath
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            Throw-PcvCurrentEvidenceInvalid -Field "current.$field" -Detail "missing-reference:$relativePath"
        }
        $referenceText = Get-Content -Raw -LiteralPath $fullPath
        if ($referenceText -notmatch [regex]::Escape($version)) {
            Throw-PcvCurrentEvidenceInvalid -Field "current.$field" -Detail "version-mismatch:$relativePath"
        }
    }

    foreach ($field in @('latest_closed_baseline', 'latest_closed_target', 'latest_closed_descriptor')) {
        $value = [string](Get-PcvCurrentEvidenceProperty -Value $manualAdmin -Name $field -Field "manual_admin.$field")
        if ([string]::IsNullOrWhiteSpace($value)) {
            Throw-PcvCurrentEvidenceInvalid -Field "manual_admin.$field" -Detail 'empty'
        }
    }

    # A cleared blocker has no truthful value to record, so the blocked_* triple is optional.
    # It is all-or-none: a partial triple would render an incomplete "Blocked follow-up" line.
    $blockedFields = @('blocked_baseline', 'blocked_target', 'blocked_reason')
    $blockedPresent = @($blockedFields | Where-Object {
        $manualAdmin.PSObject.Properties.Name -contains $_ -and
        -not [string]::IsNullOrWhiteSpace([string]$manualAdmin.$_)
    })
    if ($blockedPresent.Count -notin @(0, $blockedFields.Count)) {
        Throw-PcvCurrentEvidenceInvalid -Field 'manual_admin.blocked_*' -Detail 'partial-blocked-triple'
    }

    foreach ($field in @('public_trusted_signing', 'external_stable_publication')) {
        $value = Get-PcvCurrentEvidenceProperty -Value $claims -Name $field -Field "claims.$field"
        if ([bool]$value) {
            Throw-PcvCurrentEvidenceInvalid -Field "claims.$field" -Detail 'must-be-false'
        }
    }

    $Record
}

function ConvertTo-PcvCurrentEvidenceMarkdown {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Record)

    $current = $Record.current
    $qualification = $Record.feature_qualification
    $manual = $Record.manual_admin
    $claims = $Record.claims
    $begin = '<!-- BEGIN GENERATED CURRENT EVIDENCE -->'
    $end = '<!-- END GENERATED CURRENT EVIDENCE -->'
    $blockedLine = if ($manual.PSObject.Properties.Name -contains 'blocked_reason' -and
        -not [string]::IsNullOrWhiteSpace([string]$manual.blocked_reason)) {
        "- Blocked follow-up: ``$($manual.blocked_baseline) -> $($manual.blocked_target)`` / ``$($manual.blocked_reason)``."
    }
    else {
        $null
    }
    $blockers = @($qualification.blockers)
    $blockerText = if ($blockers.Count -eq 0) {
        'none'
    }
    else {
        ($blockers | ForEach-Object { "$($_.feature_id)/$($_.stage)/$($_.verdict)" }) -join ','
    }
    $qualificationLine = "- Feature qualification: ``contract=$($qualification.contract)``; " +
        "``promotion_eligible=$(([string]([bool]$qualification.promotion_eligible)).ToLowerInvariant())``; " +
        "``blocker_count=$($blockers.Count)``; ``blockers=$blockerText``."
    (@(
        $begin,
        '## Current operational evidence (generated)',
        '',
        "- Version: ``$($current.version)``",
        "- Active operator surfaces: Web Console and PCVCLI; ``tui_present=$(([string]([bool]$current.tui_present)).ToLowerInvariant())``.",
        "- Package evidence: ``$($current.package_evidence)``.",
        "- Full admin host mutation: ``$($current.fullgate_batch)`` / ``$($current.fullgate_evidence)``.",
        # No hardcoded per-item results here: which checks were re-run in this version and which
        # were carried forward changes every anchor, so the referenced evidence document owns it.
        "- Actual-VM functional evidence: ``$($current.functional_evidence)``.",
        $qualificationLine,
        "- Installed CLI/Web current-card: ``$($current.installed_evidence)``; CLI exit 0, Web HTTP 200, service Running/Automatic, TUI absent.",
        "- Clean MSI SHA-256: ``$($current.clean_msi_sha256)``.",
        "- Operational MSI SHA-256: ``$($current.operational_msi_sha256)``.",
        "- Operational payload aggregate SHA-256: ``$($current.payload_sha256)``.",
        "- Provenance commit: ``$($current.provenance_commit)``.",
        "- Latest closed manual-admin pair: ``$($manual.latest_closed_baseline) -> $($manual.latest_closed_target)`` / ``$($manual.latest_closed_descriptor)``.",
        $blockedLine,
        "- Claims: ``public_trusted_signing=$(([string]([bool]$claims.public_trusted_signing)).ToLowerInvariant())``; ``external_stable_publication=$(([string]([bool]$claims.external_stable_publication)).ToLowerInvariant())``.",
        $end
    ) | Where-Object { $null -ne $_ }) -join "`n"
}

function Update-PcvCurrentEvidenceDocument {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Block,
        [switch]$Check
    )

    $begin = '<!-- BEGIN GENERATED CURRENT EVIDENCE -->'
    $end = '<!-- END GENERATED CURRENT EVIDENCE -->'
    $original = Get-Content -Raw -LiteralPath $Path
    $normalized = $original.Replace("`r`n", "`n")
    $normalizedBlock = $Block.Replace("`r`n", "`n").TrimEnd("`n")
    if ([regex]::Matches($normalized, [regex]::Escape($begin)).Count -ne 1 -or
        [regex]::Matches($normalized, [regex]::Escape($end)).Count -ne 1) {
        throw "PCV_CURRENT_EVIDENCE_MARKERS_INVALID|$Path"
    }

    $beginIndex = $normalized.IndexOf($begin, [System.StringComparison]::Ordinal)
    $endIndex = $normalized.IndexOf($end, [System.StringComparison]::Ordinal)
    if ($beginIndex -lt 0 -or $endIndex -le $beginIndex) {
        throw "PCV_CURRENT_EVIDENCE_MARKERS_INVALID|$Path"
    }
    $expected = $normalized.Substring(0, $beginIndex) + $normalizedBlock +
        $normalized.Substring($endIndex + $end.Length)

    if ($expected -ceq $normalized) {
        return [pscustomobject]@{ path = $Path; status = 'current' }
    }
    if ($Check) {
        throw "PCV_CURRENT_EVIDENCE_STALE|$Path"
    }

    $newline = if ($original.Contains("`r`n")) { "`r`n" } else { "`n" }
    $output = $expected.Replace("`n", $newline)
    $temporaryPath = "$Path.tmp"
    try {
        [System.IO.File]::WriteAllText($temporaryPath, $output, [System.Text.UTF8Encoding]::new($false))
        Move-Item -LiteralPath $temporaryPath -Destination $Path -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
    [pscustomobject]@{ path = $Path; status = 'updated' }
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $resolvedEvidencePath = if ([string]::IsNullOrWhiteSpace($EvidencePath)) {
            Join-Path $resolvedRepoRoot 'docs/ga-ready/current-evidence.json'
        }
        elseif ([System.IO.Path]::IsPathRooted($EvidencePath)) {
            $EvidencePath
        }
        else {
            Join-Path $resolvedRepoRoot $EvidencePath
        }
        $record = Get-Content -Raw -LiteralPath $resolvedEvidencePath | ConvertFrom-Json
        $canonicalPath = Join-Path $resolvedRepoRoot 'docs/ga-ready/current-evidence.json'
        $canonicalRecord = Get-Content -Raw -LiteralPath $canonicalPath | ConvertFrom-Json
        Assert-PcvFeaturePromotionAllowed -ProposedRecord $record -CanonicalRecord $canonicalRecord
        [void](Test-PcvCurrentEvidenceRecord -Record $record -RepoRoot $resolvedRepoRoot)
        $block = ConvertTo-PcvCurrentEvidenceMarkdown -Record $record
        $targetRelativePaths = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ga-ready/EVIDENCE_INDEX.md',
            'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md',
            'docs/ga-ready/CONTROL_PLANE_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md'
        )
        $targetResults = [System.Collections.Generic.List[object]]::new()
        foreach ($relativePath in $targetRelativePaths) {
            $targetPath = Join-Path $resolvedRepoRoot $relativePath
            $targetResult = Update-PcvCurrentEvidenceDocument -Path $targetPath -Block $block -Check:$Check
            $targetResults.Add([pscustomobject]@{ path = $relativePath; status = $targetResult.status })
        }
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $true
            check = [bool]$Check
            source = $resolvedEvidencePath
            targets = @($targetResults.ToArray())
        }) | ConvertTo-Json -Depth 6 -Compress
    }
    catch {
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $false
            check = [bool]$Check
            error = [string]$_
        }) | ConvertTo-Json -Depth 6 -Compress
        exit 1
    }
}
