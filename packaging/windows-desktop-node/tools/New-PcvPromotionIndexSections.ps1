[CmdletBinding()]
param(
    [string]$SpecPath,
    [string]$CurrentEvidencePath,
    [string]$EvidenceIndexPath,
    [string]$ControlPlaneIndexPath,
    [string]$RepoRoot,
    [switch]$Apply,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Writes the promotion sections every Lane 3 added by hand (0.42.78: ab88076, format of 0.42.77).
# EVIDENCE_INDEX.md gets an `operational current promotion` and a `descriptor consume` section
# directly below its title. CONTROL_PLANE_INDEX.md gets a `Lane 3가 ...로 다시 승격했다.` line in the
# closure paragraph, a new `current promotion` section, and the previous `current promotion`
# heading becomes `predecessor promotion`. Values come from current-evidence.json; the spec only
# carries what a person decides (date, previous and installed versions, pair evidence, notes).

function Throw-PcvPromotionIndexInvalid {
    param(
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Detail
    )

    throw "PCV_PROMOTION_INDEX_INVALID|$Field|$Detail"
}

function ConvertFrom-PcvPromotionIndexJsonElement {
    param([Parameter(Mandatory)][System.Text.Json.JsonElement]$Element)

    $number = [long]0
    switch ([string]$Element.ValueKind) {
        'String' { return $Element.GetString() }
        'Number' { if ($Element.TryGetInt64([ref]$number)) { return $number } return $null }
        'True' { return $true }
        'False' { return $false }
        'Object' {
            $values = [ordered]@{}
            foreach ($entry in $Element.EnumerateObject()) {
                $values[$entry.Name] = ConvertFrom-PcvPromotionIndexJsonElement -Element $entry.Value
            }
            return [pscustomobject]$values
        }
        'Array' {
            return , @(foreach ($item in $Element.EnumerateArray()) { ConvertFrom-PcvPromotionIndexJsonElement -Element $item })
        }
        default { return $null }
    }
}

function ConvertFrom-PcvPromotionIndexJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Json)

    # JsonDocument keeps strings exactly as written; ConvertFrom-Json would turn ISO dates into DateTime.
    $document = [System.Text.Json.JsonDocument]::Parse($Json)
    try {
        ConvertFrom-PcvPromotionIndexJsonElement -Element $document.RootElement
    }
    finally {
        $document.Dispose()
    }
}

function Get-PcvPromotionIndexValue {
    param(
        [Parameter(Mandatory)][object]$Object,
        [Parameter(Mandatory)][string]$Path,
        [string]$Field = $Path,
        [string]$Pattern = '^\S(.*\S)?$'
    )

    $value = $Object
    foreach ($name in $Path.Split('.')) {
        if ($null -eq $value -or $value -isnot [System.Management.Automation.PSCustomObject] -or $name -cnotin @($value.PSObject.Properties.Name)) {
            Throw-PcvPromotionIndexInvalid -Field $Field -Detail 'missing'
        }
        $value = $value.$name
    }
    if ($value -isnot [string] -or $value -cnotmatch $Pattern -or $value -match '[`\r\n]') {
        Throw-PcvPromotionIndexInvalid -Field $Field -Detail 'invalid-value'
    }
    $value
}

function Get-PcvPromotionIndexInput {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][object]$Spec,
        [Parameter(Mandatory)][object]$CurrentEvidence
    )

    $version = '^\d+\.\d+\.\d+-admin-smoke$'
    $path = '^docs/ga-ready/evidence/[A-Za-z0-9._-]+\.md$'
    $sha = '^[0-9a-f]{64}$'
    if ([string](Get-PcvPromotionIndexValue -Object $Spec -Path 'contract') -cne 'pcv-promotion-index-sections-v1') {
        Throw-PcvPromotionIndexInvalid -Field 'contract' -Detail ([string]$Spec.contract)
    }
    if ($Spec.schema_version -isnot [int] -and $Spec.schema_version -isnot [long] -or $Spec.schema_version -ne 1) {
        Throw-PcvPromotionIndexInvalid -Field 'schema_version' -Detail ([string]$Spec.schema_version)
    }
    foreach ($name in @($Spec.PSObject.Properties.Name)) {
        if ($name -cnotin @('schema_version', 'contract', 'date', 'previous_version', 'installed_version', 'p0_feature_ledger_version', 'pair_evidence', 'functional_note', 'main_push_evidence')) {
            Throw-PcvPromotionIndexInvalid -Field $name -Detail 'unknown-field'
        }
    }
    if ([string](Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'contract' -Field 'current_evidence.contract') -cne 'pcv-current-evidence-v1') {
        Throw-PcvPromotionIndexInvalid -Field 'current_evidence.contract' -Detail ([string]$CurrentEvidence.contract)
    }
    $qualification = $CurrentEvidence.feature_qualification
    if ($qualification.promotion_eligible -isnot [bool] -or -not $qualification.promotion_eligible -or @($qualification.blockers).Count -ne 0) {
        Throw-PcvPromotionIndexInvalid -Field 'current_evidence.feature_qualification' -Detail 'not-eligible'
    }
    if ($CurrentEvidence.claims.public_trusted_signing -ne $false -or $CurrentEvidence.claims.external_stable_publication -ne $false) {
        Throw-PcvPromotionIndexInvalid -Field 'current_evidence.claims' -Detail 'claimed'
    }

    $promotion = [ordered]@{
        date = Get-PcvPromotionIndexValue -Object $Spec -Path 'date' -Pattern '^\d{4}-\d{2}-\d{2}$'
        previous_version = Get-PcvPromotionIndexValue -Object $Spec -Path 'previous_version' -Pattern $version
        installed_version = Get-PcvPromotionIndexValue -Object $Spec -Path 'installed_version' -Pattern $version
        p0_version = Get-PcvPromotionIndexValue -Object $Spec -Path 'p0_feature_ledger_version' -Pattern $version
        pair_evidence = Get-PcvPromotionIndexValue -Object $Spec -Path 'pair_evidence' -Pattern $path
        functional_note = Get-PcvPromotionIndexValue -Object $Spec -Path 'functional_note'
        main_push_evidence = if ('main_push_evidence' -cin @($Spec.PSObject.Properties.Name)) {
            Get-PcvPromotionIndexValue -Object $Spec -Path 'main_push_evidence' -Pattern $path
        }
        else { $null }
        version = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.version' -Field 'current_evidence.current.version' -Pattern $version
        package_evidence = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.package_evidence' -Pattern $path
        fullgate_evidence = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.fullgate_evidence' -Pattern $path
        fullgate_batch = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.fullgate_batch' -Pattern '^[a-z0-9-]+$'
        functional_evidence = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.functional_evidence' -Pattern $path
        installed_evidence = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.installed_evidence' -Pattern $path
        clean_msi = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.clean_msi_sha256' -Pattern $sha
        operational_msi = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.operational_msi_sha256' -Pattern $sha
        payload = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.payload_sha256' -Pattern $sha
        provenance = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'current.provenance_commit' -Pattern '^[0-9a-f]{40}$'
        baseline = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'manual_admin.latest_closed_baseline' -Pattern $version
        target = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'manual_admin.latest_closed_target' -Pattern $version
        descriptor = Get-PcvPromotionIndexValue -Object $CurrentEvidence -Path 'manual_admin.latest_closed_descriptor' -Pattern '^[a-z0-9-]+$'
    }
    # The consume section names the current package as the pair target MSI.
    if ($promotion.target -cne $promotion.version) {
        Throw-PcvPromotionIndexInvalid -Field 'current_evidence.manual_admin.latest_closed_target' -Detail "not-current:$($promotion.target)"
    }
    if ($promotion.previous_version -ceq $promotion.version) {
        Throw-PcvPromotionIndexInvalid -Field 'previous_version' -Detail 'same-as-current'
    }
    [pscustomobject]$promotion
}

function Get-PcvPromotionShortVersion {
    param([Parameter(Mandatory)][string]$Version)

    $Version.Substring(0, $Version.Length - '-admin-smoke'.Length)
}

function New-PcvEvidenceIndexPromotionLines {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Promotion)

    $functionalLabel = if ($Promotion.functional_evidence.Contains('carryforward')) { 'functional carry-forward' } else { 'functional' }
    $lines = [System.Collections.Generic.List[string]]::new()
    @(
        "## $($Promotion.date) ``$(Get-PcvPromotionShortVersion $Promotion.version)`` operational current promotion"
        ''
        "- ``docs/ga-ready/current-evidence.json`` current version은 ``$($Promotion.version)``다."
        "- package: ``$($Promotion.package_evidence)``, clean MSI"
        "  ``$($Promotion.clean_msi)``."
        "- fullgate: ``$($Promotion.fullgate_evidence)``"
        "  / ``$($Promotion.fullgate_batch)``. operational MSI"
        "  ``$($Promotion.operational_msi)``, provenance ``$($Promotion.provenance.Substring(0, 7))``."
        '- installed current-card:'
        "  ``$($Promotion.installed_evidence)``."
        "- ${functionalLabel}:"
        "  ``$($Promotion.functional_evidence)``."
        "- pair: ``$($Promotion.pair_evidence)`` /"
        "  ``$($Promotion.descriptor)``."
    ) | ForEach-Object { $lines.Add($_) }
    if ($null -ne $Promotion.main_push_evidence) {
        $lines.Add("- main push: ``$($Promotion.main_push_evidence)``.")
    }
    @(
        "- canonical current는 ``$($Promotion.version)``다. ``promotion_eligible=true``, blockers는 없다."
        "  P0 feature ledger current version은 ``$($Promotion.p0_version)``로 유지한다. 이 Lane 3 호스트"
        "  설치본은 ``$($Promotion.installed_version)``다. public trusted signing과 external stable publication은"
        '  주장하지 않는다.'
        ''
        "## $($Promotion.date) ``$(Get-PcvPromotionShortVersion $Promotion.baseline) -> $(Get-PcvPromotionShortVersion $Promotion.target)`` descriptor consume"
        ''
        "- ``$($Promotion.pair_evidence)``는 이미 PASS한"
        '  여섯 bucket summary를 한 artifact root로 모은 뒤'
        '  `New-PcvManualAdminCampaignDescriptor -PlanOnly`가 `overall_status=pass`,'
        '  `runner_count=6`, `missing_count=0`, `not_pass_count=0`을 낸 consume 기록이다.'
        "- descriptor는 ``$($Promotion.descriptor)``다. pair target MSI는"
        "  clean package ``$($Promotion.clean_msi)``다."
        "- 이 consume는 host mutation을 하지 않았다. 같은 날짜 Lane 3가 current를 ``$($Promotion.previous_version)``에서"
        "  ``$($Promotion.version)``로 승격했다. public trusted signing과 external stable publication은"
        '  주장하지 않는다.'
        ''
    ) | ForEach-Object { $lines.Add($_) }
    $lines.ToArray()
}

function New-PcvControlPlanePromotionLines {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Promotion)

    @(
        "## $($Promotion.date) ``$(Get-PcvPromotionShortVersion $Promotion.version)`` current promotion"
        ''
        "- 최신 product payload package는 ``$($Promotion.version)``이며"
        "  ``$($Promotion.package_evidence)``가 기록한다. Clean MSI"
        "  SHA-256은 ``$($Promotion.clean_msi)``다."
        '- full admin host mutation current는'
        "  ``$($Promotion.fullgate_evidence)`` /"
        "  ``$($Promotion.fullgate_batch)``다. Operational MSI SHA-256은"
        "  ``$($Promotion.operational_msi)``, payload aggregate"
        "  SHA-256은 ``$($Promotion.payload)``, provenance"
        "  commit은 ``$($Promotion.provenance)``다. pair target은 clean MSI이며"
        '  operational fullgate MSI와 다른 identity다.'
        '- installed current-card는'
        "  ``$($Promotion.installed_evidence)``다."
        '- actual-VM functional은'
        "  ``$($Promotion.functional_evidence)``"
        "  로 $($Promotion.functional_note)"
        "- ``$($Promotion.baseline) -> $($Promotion.target)`` package-pair는"
        "  ``$($Promotion.pair_evidence)`` /"
        "  ``$($Promotion.descriptor)``로 PASS다."
        "- operational current는 ``$($Promotion.version)``다. ``promotion_eligible=true``, blockers는"
        "  없다. 이 Lane 3 호스트 설치본은 ``$($Promotion.installed_version)``다. public trusted signing과"
        '  external stable publication은 주장하지 않는다.'
        ''
    )
}

function Get-PcvControlPlaneCurrentHeadingIndexes {
    param([Parameter(Mandatory)][AllowEmptyString()][System.Collections.Generic.List[string]]$Lines)

    @(for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -cmatch '^## \d{4}-\d{2}-\d{2} `[0-9.]+` current promotion$') { $index }
    })
}

function Invoke-PcvPromotionIndexSections {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$EvidenceIndexLines,
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$ControlPlaneLines,
        [Parameter(Mandatory)][object]$Promotion
    )

    $evidenceSection = @(New-PcvEvidenceIndexPromotionLines -Promotion $Promotion)
    $controlSection = @(New-PcvControlPlanePromotionLines -Promotion $Promotion)
    $evidence = [System.Collections.Generic.List[string]]::new([string[]]$EvidenceIndexLines)
    $control = [System.Collections.Generic.List[string]]::new([string[]]$ControlPlaneLines)

    if ($evidence.Contains($evidenceSection[0])) {
        Throw-PcvPromotionIndexInvalid -Field 'evidence_index' -Detail 'already-applied'
    }
    if ($evidence.Count -lt 3 -or $evidence[0] -cne '# Desktop Node 증거 인덱스' -or $evidence[1] -cne '' -or -not $evidence[2].StartsWith('## ')) {
        Throw-PcvPromotionIndexInvalid -Field 'evidence_index' -Detail 'unexpected-head'
    }
    $evidence.InsertRange(2, [string[]]$evidenceSection)

    if ($control.Contains($controlSection[0])) {
        Throw-PcvPromotionIndexInvalid -Field 'control_plane_index' -Detail 'already-applied'
    }
    $headings = @(Get-PcvControlPlaneCurrentHeadingIndexes -Lines $control)
    if ($headings.Count -ne 1) {
        Throw-PcvPromotionIndexInvalid -Field 'control_plane_index' -Detail "current-heading-count:$($headings.Count)"
    }
    $heading = $headings[0]
    $previousShort = Get-PcvPromotionShortVersion $Promotion.previous_version
    if (-not $control[$heading].Contains(" ``$previousShort`` ")) {
        Throw-PcvPromotionIndexInvalid -Field 'control_plane_index' -Detail "previous-heading-mismatch:$($control[$heading])"
    }
    if ($heading -lt 2 -or $control[$heading - 1] -cne '' -or $control[$heading - 2] -cnotmatch '^  .*Lane 3가 `[0-9.]+-admin-smoke`로 (다시 )?승격했다\.$') {
        Throw-PcvPromotionIndexInvalid -Field 'control_plane_index' -Detail 'missing-closure-line'
    }
    $control[$heading] = $control[$heading].Replace(' current promotion', ' predecessor promotion')
    $control.InsertRange($heading, [string[]]$controlSection)
    $control.Insert($heading - 1, "  $($Promotion.date) Lane 3가 ``$($Promotion.version)``로 다시 승격했다.")

    [pscustomobject]@{
        evidence_index = $evidence.ToArray()
        control_plane_index = $control.ToArray()
    }
}

function Get-PcvPromotionIndexStaleTargets {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$EvidenceIndexLines,
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$ControlPlaneLines,
        [Parameter(Mandatory)][object]$Promotion
    )

    $evidenceSection = (@(New-PcvEvidenceIndexPromotionLines -Promotion $Promotion) -join "`n")
    $controlSection = (@(New-PcvControlPlanePromotionLines -Promotion $Promotion) -join "`n")
    $closure = "  $($Promotion.date) Lane 3가 ``$($Promotion.version)``로 다시 승격했다.`n`n$(@(New-PcvControlPlanePromotionLines -Promotion $Promotion)[0])"
    $control = [System.Collections.Generic.List[string]]::new([string[]]$ControlPlaneLines)
    @(
        if (-not (($EvidenceIndexLines -join "`n").StartsWith("# Desktop Node 증거 인덱스`n`n$evidenceSection", [System.StringComparison]::Ordinal))) { 'evidence_index' }
        $controlText = $ControlPlaneLines -join "`n"
        if (-not $controlText.Contains($controlSection) -or
            -not $controlText.Contains($closure) -or
            @(Get-PcvControlPlaneCurrentHeadingIndexes -Lines $control).Count -ne 1) { 'control_plane_index' }
    )
}

function Resolve-PcvPromotionIndexPath {
    param(
        [Parameter(Mandatory)][string]$RepoRoot,
        [string]$Path,
        [Parameter(Mandatory)][string]$Default
    )

    if ([string]::IsNullOrWhiteSpace($Path)) { Join-Path $RepoRoot $Default }
    elseif ([System.IO.Path]::IsPathRooted($Path)) { $Path }
    else { Join-Path $RepoRoot $Path }
}

function Read-PcvPromotionIndexFile {
    param([Parameter(Mandatory)][string]$Path)

    $text = [System.IO.File]::ReadAllText($Path)
    [pscustomobject]@{
        newline = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
        lines = $text.Replace("`r`n", "`n").Split("`n")
    }
}

function Write-PcvPromotionIndexFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][string]$Newline
    )

    $temporaryPath = "$Path.tmp"
    try {
        [System.IO.File]::WriteAllText($temporaryPath, ($Lines -join "`n").Replace("`n", $Newline), [System.Text.UTF8Encoding]::new($false))
        Move-Item -LiteralPath $temporaryPath -Destination $Path -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        if ($Apply -and $Check) {
            throw 'PCV_PROMOTION_INDEX_USAGE|apply-and-check-are-exclusive'
        }
        if ([string]::IsNullOrWhiteSpace($SpecPath)) {
            throw 'PCV_PROMOTION_INDEX_USAGE|spec-path-required'
        }
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $paths = [ordered]@{
            current_evidence = Resolve-PcvPromotionIndexPath -RepoRoot $resolvedRepoRoot -Path $CurrentEvidencePath -Default 'docs/ga-ready/current-evidence.json'
            evidence_index = Resolve-PcvPromotionIndexPath -RepoRoot $resolvedRepoRoot -Path $EvidenceIndexPath -Default 'docs/ga-ready/EVIDENCE_INDEX.md'
            control_plane_index = Resolve-PcvPromotionIndexPath -RepoRoot $resolvedRepoRoot -Path $ControlPlaneIndexPath -Default 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        }
        $spec = ConvertFrom-PcvPromotionIndexJson -Json (Get-Content -Raw -LiteralPath $SpecPath)
        $currentEvidence = ConvertFrom-PcvPromotionIndexJson -Json ([System.IO.File]::ReadAllText($paths.current_evidence))
        $promotion = Get-PcvPromotionIndexInput -Spec $spec -CurrentEvidence $currentEvidence
        $evidenceFile = Read-PcvPromotionIndexFile -Path $paths.evidence_index
        $controlFile = Read-PcvPromotionIndexFile -Path $paths.control_plane_index
        $mode = if ($Check) { 'check' } elseif ($Apply) { 'apply' } else { 'dry-run' }

        if ($Check) {
            $stale = @(Get-PcvPromotionIndexStaleTargets -EvidenceIndexLines $evidenceFile.lines -ControlPlaneLines $controlFile.lines -Promotion $promotion)
            if ($stale.Count -ne 0) {
                throw "PCV_PROMOTION_INDEX_STALE|$($stale -join ',')"
            }
            [pscustomobject]([ordered]@{ schema_version = 1; ok = $true; mode = $mode; version = $promotion.version; status = 'current' }) |
                ConvertTo-Json -Depth 6 -Compress
            return
        }

        $result = Invoke-PcvPromotionIndexSections -EvidenceIndexLines $evidenceFile.lines -ControlPlaneLines $controlFile.lines -Promotion $promotion
        if ($Apply) {
            Write-PcvPromotionIndexFile -Path $paths.evidence_index -Lines $result.evidence_index -Newline $evidenceFile.newline
            Write-PcvPromotionIndexFile -Path $paths.control_plane_index -Lines $result.control_plane_index -Newline $controlFile.newline
        }
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $true
            mode = $mode
            version = $promotion.version
            status = if ($Apply) { 'updated' } else { 'planned' }
            evidence_index_added_lines = $result.evidence_index.Count - $evidenceFile.lines.Count
            control_plane_index_added_lines = $result.control_plane_index.Count - $controlFile.lines.Count
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
