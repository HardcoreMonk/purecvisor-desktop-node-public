[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ResultsRoot,

    [Parameter(Mandatory)]
    [string]$BaselinePath,

    [Parameter(Mandatory)]
    [string]$MigrationManifestPath,

    [switch]$WriteBaseline,

    [string]$RepoRoot = (Join-Path $PSScriptRoot '../../..'),

    [Alias('BaselineCommit')]
    [string]$AuditBaseCommit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:InvariantCulture = [System.Globalization.CultureInfo]::InvariantCulture

function Get-PcvFullPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$BasePath
    )

    $candidate = if ([IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $BasePath $Path
    }
    [IO.Path]::GetFullPath($candidate)
}

function Assert-PcvContainedPath {
    param(
        [Parameter(Mandatory)]
        [string]$Root,

        [Parameter(Mandatory)]
        [string]$Candidate,

        [Parameter(Mandatory)]
        [string]$Label,

        [switch]$AllowRoot
    )

    $rootFull = [IO.Path]::GetFullPath($Root)
    $rootPathRoot = [IO.Path]::GetPathRoot($rootFull)
    if (-not [string]::Equals($rootFull, $rootPathRoot, [StringComparison]::OrdinalIgnoreCase)) {
        $rootFull = $rootFull.TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar
        )
    }
    $candidateFull = [IO.Path]::GetFullPath($Candidate)
    $candidatePathRoot = [IO.Path]::GetPathRoot($candidateFull)
    if (-not [string]::Equals($candidateFull, $candidatePathRoot, [StringComparison]::OrdinalIgnoreCase)) {
        $candidateFull = $candidateFull.TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar
        )
    }
    $relativePath = [IO.Path]::GetRelativePath($rootFull, $candidateFull)
    $isRoot = [string]::Equals($relativePath, '.', [StringComparison]::Ordinal)
    $parentPrefix = '..' + [IO.Path]::DirectorySeparatorChar
    $alternateParentPrefix = '..' + [IO.Path]::AltDirectorySeparatorChar
    $isChild = (-not $isRoot) -and
        (-not [IO.Path]::IsPathRooted($relativePath)) -and
        (-not [string]::Equals($relativePath, '..', [StringComparison]::Ordinal)) -and
        (-not $relativePath.StartsWith($parentPrefix, [StringComparison]::Ordinal)) -and
        (-not $relativePath.StartsWith($alternateParentPrefix, [StringComparison]::Ordinal))
    if ((-not $isChild) -and (-not ($AllowRoot -and $isRoot))) {
        throw "PCV_DOTNET_QUALITY_PATH_OUTSIDE_REPO: $Label '$candidateFull' is not contained by '$rootFull'."
    }

    $pathRoot = [IO.Path]::GetPathRoot($candidateFull)
    if ([string]::IsNullOrWhiteSpace($pathRoot)) {
        throw "PCV_DOTNET_QUALITY_PATH_OUTSIDE_REPO: $Label '$candidateFull' has no filesystem root."
    }

    $segments = @($candidateFull.Substring($pathRoot.Length).Split(
        [char[]]@([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar),
        [StringSplitOptions]::RemoveEmptyEntries
    ))
    $currentPath = $pathRoot
    $pathsToInspect = [System.Collections.Generic.List[string]]::new()
    $pathsToInspect.Add($currentPath)
    foreach ($segment in $segments) {
        $currentPath = [IO.Path]::Combine($currentPath, $segment)
        $pathsToInspect.Add($currentPath)
    }

    foreach ($pathToInspect in $pathsToInspect) {
        try {
            $item = Get-Item -LiteralPath $pathToInspect -Force -ErrorAction Stop
        }
        catch [System.Management.Automation.ItemNotFoundException] {
            break
        }

        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "PCV_DOTNET_QUALITY_PATH_REPARSE_POINT: $Label '$candidateFull' traverses reparse point '$pathToInspect'."
        }
    }

    $candidateFull
}

function Read-PcvSafeXml {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$Label
    )

    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = $null
    try {
        $reader = [System.Xml.XmlReader]::Create($Path, $settings)
        $document = [System.Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
        $document
    }
    catch {
        throw "PCV_DOTNET_QUALITY_INVALID_XML: $Label '$Path' could not be parsed. $($_.Exception.Message)"
    }
    finally {
        if ($null -ne $reader) {
            $reader.Dispose()
        }
    }
}

function Read-PcvJson {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$Label
    )

    try {
        Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "PCV_DOTNET_QUALITY_INVALID_JSON: $Label '$Path' could not be parsed. $($_.Exception.Message)"
    }
}

function Get-PcvRequiredProperty {
    param(
        [Parameter(Mandatory)]
        [object]$InputObject,

        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$Label
    )

    if ($InputObject.PSObject.Properties.Name -notcontains $Name) {
        throw "PCV_DOTNET_QUALITY_SCHEMA_INVALID: $Label is missing required property '$Name'."
    }
    $InputObject.$Name
}

function Convert-PcvIntegerAttribute {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement]$Element,

        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$Label
    )

    $text = [string]$Element.GetAttribute($Name)
    $parsed = 0
    if ([string]::IsNullOrWhiteSpace($text) -or
        -not [int]::TryParse(
            $text,
            [System.Globalization.NumberStyles]::Integer,
            $script:InvariantCulture,
            [ref]$parsed
        ) -or
        $parsed -lt 0) {
        throw "PCV_DOTNET_QUALITY_COUNTER_INVALID: $Label attribute '$Name' must be a non-negative integer."
    }
    $parsed
}

function Convert-PcvRateAttribute {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement]$Element,

        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$Label
    )

    $text = [string]$Element.GetAttribute($Name)
    $parsed = 0.0
    if ([string]::IsNullOrWhiteSpace($text) -or
        -not [double]::TryParse(
            $text,
            [System.Globalization.NumberStyles]::Float,
            $script:InvariantCulture,
            [ref]$parsed
        ) -or
        [double]::IsNaN($parsed) -or
        [double]::IsInfinity($parsed) -or
        $parsed -lt 0.0 -or
        $parsed -gt 1.0) {
        throw "PCV_DOTNET_QUALITY_COVERAGE_INVALID: $Label attribute '$Name' must be between 0 and 1."
    }
    $parsed
}

function Get-PcvTrxQuality {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$ProjectName
    )

    $document = Read-PcvSafeXml -Path $Path -Label 'TRX'
    $counterNodes = @($document.SelectNodes("//*[local-name()='Counters']"))
    if ($counterNodes.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_TRX_SCHEMA_INVALID: Project '$ProjectName' TRX must contain exactly one Counters element."
    }
    $counters = [System.Xml.XmlElement]$counterNodes[0]
    $total = Convert-PcvIntegerAttribute -Element $counters -Name 'total' -Label "$ProjectName TRX Counters"
    $executed = Convert-PcvIntegerAttribute -Element $counters -Name 'executed' -Label "$ProjectName TRX Counters"
    $passed = Convert-PcvIntegerAttribute -Element $counters -Name 'passed' -Label "$ProjectName TRX Counters"
    $failed = Convert-PcvIntegerAttribute -Element $counters -Name 'failed' -Label "$ProjectName TRX Counters"
    $errorCount = Convert-PcvIntegerAttribute -Element $counters -Name 'error' -Label "$ProjectName TRX Counters"
    $timeoutCount = Convert-PcvIntegerAttribute -Element $counters -Name 'timeout' -Label "$ProjectName TRX Counters"
    $abortedCount = Convert-PcvIntegerAttribute -Element $counters -Name 'aborted' -Label "$ProjectName TRX Counters"
    $notExecuted = Convert-PcvIntegerAttribute -Element $counters -Name 'notExecuted' -Label "$ProjectName TRX Counters"
    if ($executed -gt $total -or $passed -gt $executed -or $notExecuted -gt $total) {
        throw "PCV_DOTNET_QUALITY_COUNTER_INVALID: Project '$ProjectName' has inconsistent TRX counters."
    }

    $skipped = $total - $executed
    if ($notExecuted -gt $skipped) {
        throw "PCV_DOTNET_QUALITY_COUNTER_INVALID: Project '$ProjectName' reports notExecuted greater than total minus executed."
    }

    $resultNodes = @($document.SelectNodes("//*[local-name()='UnitTestResult']"))
    if ($resultNodes.Count -ne $total) {
        throw "PCV_DOTNET_QUALITY_TRX_SCHEMA_INVALID: Project '$ProjectName' reports total=$total but contains $($resultNodes.Count) UnitTestResult elements."
    }

    $namedResults = @($resultNodes | ForEach-Object {
        $testName = [string]$_.GetAttribute('testName')
        $testId = [string]$_.GetAttribute('testId')
        if ([string]::IsNullOrWhiteSpace($testName)) {
            throw "PCV_DOTNET_QUALITY_TRX_SCHEMA_INVALID: Project '$ProjectName' contains a UnitTestResult without testName."
        }
        [pscustomobject]@{
            name = $testName.Trim()
            native_id = $testId.Trim()
        }
    })

    $duplicateTestNames = @($namedResults | Group-Object name | Where-Object Count -gt 1 | ForEach-Object Name)
    $testIds = @($namedResults | ForEach-Object {
        $baseId = if ($_.name.StartsWith("$ProjectName.", [StringComparison]::Ordinal)) {
            $_.name
        }
        else {
            "$ProjectName.$($_.name)"
        }
        if ($duplicateTestNames -contains $_.name) {
            if ([string]::IsNullOrWhiteSpace($_.native_id)) {
                throw "PCV_DOTNET_QUALITY_TRX_SCHEMA_INVALID: Duplicate test '$baseId' must include testId."
            }
            "$baseId@$($_.native_id)"
        }
        else {
            $baseId
        }
    } | Sort-Object)

    [pscustomobject]@{
        total = $total
        executed = $executed
        skipped = $skipped
        passed = $passed
        failed = $failed + $errorCount + $timeoutCount + $abortedCount
        test_ids = $testIds
    }
}

function Get-PcvCoberturaQuality {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$ProjectName
    )

    $document = Read-PcvSafeXml -Path $Path -Label 'Cobertura'
    $coverageNodes = @($document.SelectNodes("/*[local-name()='coverage']"))
    if ($coverageNodes.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_COBERTURA_SCHEMA_INVALID: Project '$ProjectName' must contain one coverage root element."
    }
    $coverage = [System.Xml.XmlElement]$coverageNodes[0]
    $lineRate = Convert-PcvRateAttribute -Element $coverage -Name 'line-rate' -Label "$ProjectName Cobertura"
    $branchRate = Convert-PcvRateAttribute -Element $coverage -Name 'branch-rate' -Label "$ProjectName Cobertura"
    $linesCovered = Convert-PcvIntegerAttribute -Element $coverage -Name 'lines-covered' -Label "$ProjectName Cobertura"
    $linesValid = Convert-PcvIntegerAttribute -Element $coverage -Name 'lines-valid' -Label "$ProjectName Cobertura"
    $branchesCovered = Convert-PcvIntegerAttribute -Element $coverage -Name 'branches-covered' -Label "$ProjectName Cobertura"
    $branchesValid = Convert-PcvIntegerAttribute -Element $coverage -Name 'branches-valid' -Label "$ProjectName Cobertura"
    if (($linesValid -gt 0 -and $linesCovered -gt $linesValid) -or
        ($branchesValid -gt 0 -and $branchesCovered -gt $branchesValid)) {
        throw "PCV_DOTNET_QUALITY_COVERAGE_INVALID: Project '$ProjectName' reports covered items greater than valid items."
    }

    $lineCoveragePercent = if ($linesValid -eq 0) {
        [Math]::Round($lineRate * 100.0, 6)
    }
    else {
        [Math]::Round(($linesCovered * 100.0) / $linesValid, 6)
    }
    $branchCoveragePercent = if ($branchesValid -eq 0) {
        [Math]::Round($branchRate * 100.0, 6)
    }
    else {
        [Math]::Round(($branchesCovered * 100.0) / $branchesValid, 6)
    }

    [pscustomobject]@{
        line_coverage_percent = $lineCoveragePercent
        branch_coverage_percent = $branchCoveragePercent
        lines_covered = $linesCovered
        lines_valid = $linesValid
        branches_covered = $branchesCovered
        branches_valid = $branchesValid
    }
}

function Get-PcvAuditBaseCommit {
    param([Parameter(Mandatory)][string]$Root)

    if (-not [string]::IsNullOrWhiteSpace($AuditBaseCommit)) {
        return $AuditBaseCommit.Trim()
    }
    if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_SHA)) {
        return $env:GITHUB_SHA.Trim()
    }
    if (Test-Path -LiteralPath (Join-Path $Root '.git')) {
        $commitOutput = @(& git -C $Root rev-parse HEAD 2>$null)
        if ($LASTEXITCODE -eq 0 -and $commitOutput.Count -gt 0 -and
            -not [string]::IsNullOrWhiteSpace([string]$commitOutput[0])) {
            return ([string]$commitOutput[0]).Trim()
        }
    }
    'unavailable'
}

function Get-PcvDotNetSourceSnapshot {
    param(
        [Parameter(Mandatory)]
        [string]$Root,

        [Parameter(Mandatory)]
        [string]$AuditCommit
    )

    $sourceRoot = Join-Path $Root 'src'
    $sourceRoot = Assert-PcvContainedPath -Root $Root -Candidate $sourceRoot -Label 'DotNetSourceRoot'
    if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
        throw "PCV_DOTNET_QUALITY_SOURCE_SNAPSHOT_INVALID: Source root '$sourceRoot' does not exist."
    }

    $excludedDirectoryNames = @('bin', 'obj', 'TestResults', '.vs')
    $snapshotFiles = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
    $sourceItems = @(Get-ChildItem -LiteralPath $sourceRoot -Recurse -Force)
    foreach ($sourceItem in $sourceItems) {
        if (($sourceItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "PCV_DOTNET_QUALITY_SOURCE_SNAPSHOT_REPARSE_POINT: Source snapshot traverses reparse point '$($sourceItem.FullName)'."
        }
        if ($sourceItem.PSIsContainer) {
            continue
        }

        $relativePath = [IO.Path]::GetRelativePath($Root, $sourceItem.FullName).Replace('\', '/')
        $segments = @($relativePath.Split('/'))
        if (@($segments | Where-Object { $_ -in $excludedDirectoryNames }).Count -gt 0) {
            continue
        }
        $safePath = Assert-PcvContainedPath -Root $sourceRoot -Candidate $sourceItem.FullName -Label 'DotNetSourceFile'
        $snapshotFiles.Add($relativePath, $safePath)
    }

    foreach ($rootInputName in @(
        'global.json',
        'NuGet.config',
        'Directory.Build.props',
        'Directory.Build.targets',
        'Directory.Packages.props'
    )) {
        $rootInputPath = Join-Path $Root $rootInputName
        if (Test-Path -LiteralPath $rootInputPath -PathType Leaf) {
            $safePath = Assert-PcvContainedPath -Root $Root -Candidate $rootInputPath -Label 'DotNetRootInput'
            $snapshotFiles.Add($rootInputName, $safePath)
        }
    }

    $relativePaths = [string[]]@($snapshotFiles.Keys)
    [Array]::Sort($relativePaths, [StringComparer]::Ordinal)
    $incrementalHash = [Security.Cryptography.IncrementalHash]::CreateHash(
        [Security.Cryptography.HashAlgorithmName]::SHA256
    )
    try {
        $seed = "pcv-dotnet-source-snapshot/v1`0$AuditCommit`n"
        $incrementalHash.AppendData([Text.Encoding]::UTF8.GetBytes($seed))
        foreach ($relativePath in $relativePaths) {
            $content = [IO.File]::ReadAllBytes($snapshotFiles[$relativePath])
            $contentHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($content)).ToLowerInvariant()
            $record = "F`0$relativePath`0$($content.LongLength)`0$contentHash`n"
            $incrementalHash.AppendData([Text.Encoding]::UTF8.GetBytes($record))
        }
        $snapshotHash = [Convert]::ToHexString($incrementalHash.GetHashAndReset()).ToLowerInvariant()
    }
    finally {
        $incrementalHash.Dispose()
    }

    $deletedPathCount = 0
    if ($AuditCommit -ne 'unavailable' -and (Test-Path -LiteralPath (Join-Path $Root '.git'))) {
        $deletedPaths = @(& git -C $Root diff --name-only --diff-filter=D $AuditCommit -- src 2>$null)
        if ($LASTEXITCODE -eq 0) {
            $deletedPathCount = @($deletedPaths | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }).Count
        }
    }

    [ordered]@{
        schema_version = 'pcv-dotnet-source-snapshot/v1'
        algorithm = 'sha256'
        scope = 'src/** plus present root .NET build inputs; excludes generated directory segments'
        canonical_record_format = 'F\0relative-path\0byte-length\0content-sha256\n'
        sha256 = $snapshotHash
        file_count = $relativePaths.Count
        deleted_path_count = $deletedPathCount
        excluded_paths = @('src/**/bin/**', 'src/**/obj/**', 'src/**/TestResults/**', 'src/**/.vs/**')
    }
}

function Test-PcvSamePath {
    param(
        [Parameter(Mandatory)][string]$Left,
        [Parameter(Mandatory)][string]$Right
    )

    [string]::Equals(
        [IO.Path]::GetFullPath($Left).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar),
        [IO.Path]::GetFullPath($Right).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar),
        [StringComparison]::OrdinalIgnoreCase
    )
}

function Test-PcvTestIdPattern {
    param(
        [Parameter(Mandatory)][string]$Pattern,
        [Parameter(Mandatory)][string]$TestId
    )

    if ($Pattern.IndexOfAny([char[]]@('*', '?')) -lt 0) {
        return [string]::Equals($Pattern, $TestId, [StringComparison]::Ordinal)
    }
    $wildcard = [System.Management.Automation.WildcardPattern]::new(
        $Pattern,
        [System.Management.Automation.WildcardOptions]::CultureInvariant
    )
    $wildcard.IsMatch($TestId)
}

function Resolve-PcvMigrationReplacements {
    param(
        [Parameter(Mandatory)][object]$Migration,
        [Parameter(Mandatory)][string]$RemovedTestId,
        [Parameter(Mandatory)][System.Collections.Generic.HashSet[string]]$CurrentTestIds
    )

    $oldPattern = [string]$Migration.old_test_id
    $oldStars = @($oldPattern.ToCharArray() | Where-Object { $_ -eq '*' }).Count
    $oldQuestions = @($oldPattern.ToCharArray() | Where-Object { $_ -eq '?' }).Count
    $resolved = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($replacementPatternValue in @($Migration.replacement_test_ids)) {
        $replacementPattern = [string]$replacementPatternValue
        $replacementStars = @($replacementPattern.ToCharArray() | Where-Object { $_ -eq '*' }).Count
        $replacementQuestions = @($replacementPattern.ToCharArray() | Where-Object { $_ -eq '?' }).Count

        if ($oldStars -eq 0 -and $oldQuestions -eq 0) {
            if ($replacementStars -eq 0 -and $replacementQuestions -eq 0) {
                if ($CurrentTestIds.Contains($replacementPattern)) {
                    [void]$resolved.Add($replacementPattern)
                }
                continue
            }

            $matches = @($CurrentTestIds | Where-Object {
                Test-PcvTestIdPattern -Pattern $replacementPattern -TestId ([string]$_)
            })
            if ($matches.Count -eq 1) {
                [void]$resolved.Add([string]$matches[0])
            }
            elseif ($matches.Count -gt 1) {
                throw "PCV_DOTNET_QUALITY_INVALID_MIGRATION: Replacement pattern '$replacementPattern' for '$RemovedTestId' is not one-to-one."
            }
            continue
        }

        if ($oldStars -ne 1 -or $oldQuestions -ne 0 -or
            $replacementStars -ne 1 -or $replacementQuestions -ne 0) {
            throw "PCV_DOTNET_QUALITY_INVALID_MIGRATION: Wildcard migration '$oldPattern' -> '$replacementPattern' must contain exactly one '*' on each side."
        }

        $oldStarIndex = $oldPattern.IndexOf('*')
        $oldPrefix = $oldPattern.Substring(0, $oldStarIndex)
        $oldSuffix = $oldPattern.Substring($oldStarIndex + 1)
        if (-not $RemovedTestId.StartsWith($oldPrefix, [StringComparison]::Ordinal) -or
            -not $RemovedTestId.EndsWith($oldSuffix, [StringComparison]::Ordinal) -or
            $RemovedTestId.Length -lt ($oldPrefix.Length + $oldSuffix.Length)) {
            continue
        }
        $captureLength = $RemovedTestId.Length - $oldPrefix.Length - $oldSuffix.Length
        $capturedId = $RemovedTestId.Substring($oldPrefix.Length, $captureLength)
        $replacementStarIndex = $replacementPattern.IndexOf('*')
        $derivedReplacement = $replacementPattern.Substring(0, $replacementStarIndex) +
            $capturedId +
            $replacementPattern.Substring($replacementStarIndex + 1)
        if ($CurrentTestIds.Contains($derivedReplacement)) {
            [void]$resolved.Add($derivedReplacement)
        }
    }

    @($resolved)
}

$repoRootFull = [IO.Path]::GetFullPath($RepoRoot)
if (-not (Test-Path -LiteralPath $repoRootFull -PathType Container)) {
    throw "PCV_DOTNET_QUALITY_REPO_ROOT_MISSING: Repository root '$repoRootFull' does not exist."
}

$resultsRootFull = Get-PcvFullPath -Path $ResultsRoot -BasePath $repoRootFull
$resultsRootFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $resultsRootFull -Label 'ResultsRoot'
$baselineFull = Get-PcvFullPath -Path $BaselinePath -BasePath $repoRootFull
$baselineFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $baselineFull -Label 'BaselinePath'
$migrationFull = Get-PcvFullPath -Path $MigrationManifestPath -BasePath $repoRootFull
$migrationFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $migrationFull -Label 'MigrationManifestPath'

if (-not (Test-Path -LiteralPath $resultsRootFull -PathType Container)) {
    throw "PCV_DOTNET_QUALITY_RESULTS_MISSING: Results root '$resultsRootFull' does not exist."
}
if (-not (Test-Path -LiteralPath $migrationFull -PathType Leaf)) {
    throw "PCV_DOTNET_QUALITY_MIGRATION_MISSING: Migration manifest '$migrationFull' does not exist."
}
if ((-not $WriteBaseline) -and (-not (Test-Path -LiteralPath $baselineFull -PathType Leaf))) {
    throw "PCV_DOTNET_QUALITY_BASELINE_MISSING: Baseline '$baselineFull' does not exist. Use -WriteBaseline explicitly to create it."
}

$artifactRoot = Split-Path -Parent $resultsRootFull
$artifactRoot = Assert-PcvContainedPath -Root $repoRootFull -Candidate $artifactRoot -Label 'ArtifactRoot' -AllowRoot
$captureSummaryPath = Join-Path $artifactRoot 'quality-capture-summary.json'
$captureSummaryPath = Assert-PcvContainedPath -Root $artifactRoot -Candidate $captureSummaryPath -Label 'CaptureSummaryPath'
if (-not (Test-Path -LiteralPath $captureSummaryPath -PathType Leaf)) {
    throw "PCV_DOTNET_QUALITY_CAPTURE_SUMMARY_MISSING: '$captureSummaryPath' does not exist."
}
$captureSummary = Read-PcvJson -Path $captureSummaryPath -Label 'capture summary'
if ([string](Get-PcvRequiredProperty -InputObject $captureSummary -Name 'schema_version' -Label 'capture summary') -ne
    'pcv-dotnet-quality-capture/v1') {
    throw 'PCV_DOTNET_QUALITY_SCHEMA_INVALID: Unsupported capture summary schema_version.'
}
$sdkVersion = [string](Get-PcvRequiredProperty -InputObject $captureSummary -Name 'dotnet_sdk_version' -Label 'capture summary')
$collectorVersion = [string](Get-PcvRequiredProperty -InputObject $captureSummary -Name 'coverage_collector_version' -Label 'capture summary')
if ([string]::IsNullOrWhiteSpace($sdkVersion) -or [string]::IsNullOrWhiteSpace($collectorVersion)) {
    throw 'PCV_DOTNET_QUALITY_SCHEMA_INVALID: Capture summary SDK and collector versions must be non-empty.'
}
$capturedProjectEntries = @(Get-PcvRequiredProperty -InputObject $captureSummary -Name 'projects' -Label 'capture summary')
if ($capturedProjectEntries.Count -eq 0) {
    throw 'PCV_DOTNET_QUALITY_SCHEMA_INVALID: Capture summary projects must not be empty.'
}

$captureProjectNames = @($capturedProjectEntries | ForEach-Object {
    $entryName = [string](Get-PcvRequiredProperty -InputObject $_ -Name 'name' -Label 'capture project')
    if ([string]::IsNullOrWhiteSpace($entryName)) {
        throw 'PCV_DOTNET_QUALITY_SCHEMA_INVALID: Capture project name must be non-empty.'
    }
    foreach ($pathProperty in @('result_directory', 'trx_path', 'cobertura_path')) {
        $relativePath = [string](Get-PcvRequiredProperty -InputObject $_ -Name $pathProperty -Label "capture project '$entryName'")
        $referencedPath = Get-PcvFullPath -Path $relativePath -BasePath $artifactRoot
        $referencedPath = Assert-PcvContainedPath -Root $artifactRoot -Candidate $referencedPath -Label "capture project '$entryName' $pathProperty"
    }
    $entryName.Trim()
})
$duplicateCaptureProjects = @($captureProjectNames |
    Group-Object { ([string]$_).ToUpperInvariant() } |
    Where-Object Count -gt 1)
if ($duplicateCaptureProjects.Count -gt 0) {
    throw 'PCV_DOTNET_QUALITY_DUPLICATE_PROJECT: Capture summary contains duplicate project names.'
}

$projectDirectories = @(Get-ChildItem -LiteralPath $resultsRootFull -Directory | ForEach-Object {
    $safePath = Assert-PcvContainedPath -Root $resultsRootFull -Candidate $_.FullName -Label 'ProjectResultDirectory'
    Get-Item -LiteralPath $safePath -Force
} | Sort-Object Name)
if ($projectDirectories.Count -eq 0) {
    throw 'PCV_DOTNET_QUALITY_NO_PROJECT_RESULTS: Results root contains no project directories.'
}
$duplicateProjectDirectories = @($projectDirectories |
    Group-Object { $_.Name.ToUpperInvariant() } |
    Where-Object Count -gt 1)
if ($duplicateProjectDirectories.Count -gt 0) {
    throw 'PCV_DOTNET_QUALITY_DUPLICATE_PROJECT: Results root contains duplicate project directories.'
}

$currentProjectNames = @($projectDirectories.Name)
$captureMismatch = @(Compare-Object -ReferenceObject @($captureProjectNames | Sort-Object) -DifferenceObject @($currentProjectNames | Sort-Object))
if ($captureMismatch.Count -gt 0) {
    throw "PCV_DOTNET_QUALITY_CAPTURE_PROJECT_MISMATCH: Capture summary and result directories differ: $($captureMismatch.InputObject -join ', ')."
}

$currentProjects = [System.Collections.Generic.List[object]]::new()
foreach ($projectDirectory in $projectDirectories) {
    $projectName = $projectDirectory.Name
    $trxFiles = @(Get-ChildItem -LiteralPath $projectDirectory.FullName -Recurse -File -Filter '*.trx')
    $coverageFiles = @(Get-ChildItem -LiteralPath $projectDirectory.FullName -Recurse -File |
        Where-Object Name -eq 'coverage.cobertura.xml')
    $trxFiles = @($trxFiles | ForEach-Object {
        $safePath = Assert-PcvContainedPath -Root $projectDirectory.FullName -Candidate $_.FullName -Label 'TRX artifact'
        Get-Item -LiteralPath $safePath -Force
    })
    $coverageFiles = @($coverageFiles | ForEach-Object {
        $safePath = Assert-PcvContainedPath -Root $projectDirectory.FullName -Candidate $_.FullName -Label 'Cobertura artifact'
        Get-Item -LiteralPath $safePath -Force
    })
    if ($trxFiles.Count -ne 1 -or $coverageFiles.Count -eq 0) {
        throw "PCV_DOTNET_QUALITY_DUPLICATE_PROJECT_ARTIFACT: Project '$projectName' has $($trxFiles.Count) TRX and $($coverageFiles.Count) Cobertura files; exactly one TRX and at least one Cobertura file are required."
    }
    $coverageHashes = @($coverageFiles | ForEach-Object {
        (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    } | Sort-Object -Unique)
    if ($coverageHashes.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_CONFLICTING_COBERTURA: Project '$projectName' has non-identical Cobertura files."
    }

    $captureEntry = @($capturedProjectEntries | Where-Object { [string]$_.name -ieq $projectName })[0]
    $expectedTrx = Get-PcvFullPath -Path ([string]$captureEntry.trx_path) -BasePath $artifactRoot
    $expectedCoverage = Get-PcvFullPath -Path ([string]$captureEntry.cobertura_path) -BasePath $artifactRoot
    $matchingCoverage = @($coverageFiles | Where-Object {
        Test-PcvSamePath -Left $expectedCoverage -Right $_.FullName
    })
    if (-not (Test-PcvSamePath -Left $expectedTrx -Right $trxFiles[0].FullName) -or
        $matchingCoverage.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_CAPTURE_ARTIFACT_MISMATCH: Capture summary paths do not match project '$projectName' artifacts."
    }

    $trxQuality = Get-PcvTrxQuality -Path $trxFiles[0].FullName -ProjectName $projectName
    $coverageQuality = Get-PcvCoberturaQuality -Path $matchingCoverage[0].FullName -ProjectName $projectName
    if ($trxQuality.failed -gt 0) {
        throw "PCV_DOTNET_QUALITY_TEST_FAILURE: Project '$projectName' contains $($trxQuality.failed) failed/error/timeout/aborted tests."
    }
    $currentProjects.Add([pscustomobject][ordered]@{
        name = $projectName
        total = $trxQuality.total
        executed = $trxQuality.executed
        skipped = $trxQuality.skipped
        passed = $trxQuality.passed
        failed = $trxQuality.failed
        line_coverage_percent = $coverageQuality.line_coverage_percent
        branch_coverage_percent = $coverageQuality.branch_coverage_percent
        lines_covered = $coverageQuality.lines_covered
        lines_valid = $coverageQuality.lines_valid
        branches_covered = $coverageQuality.branches_covered
        branches_valid = $coverageQuality.branches_valid
        test_ids = @($trxQuality.test_ids)
    })
}

$totalTests = [int](($currentProjects | Measure-Object total -Sum).Sum)
$totalExecuted = [int](($currentProjects | Measure-Object executed -Sum).Sum)
$totalSkipped = [int](($currentProjects | Measure-Object skipped -Sum).Sum)
$totalPassed = [int](($currentProjects | Measure-Object passed -Sum).Sum)
$totalFailed = [int](($currentProjects | Measure-Object failed -Sum).Sum)
$linesCoveredTotal = [int](($currentProjects | Measure-Object lines_covered -Sum).Sum)
$linesValidTotal = [int](($currentProjects | Measure-Object lines_valid -Sum).Sum)
$branchesCoveredTotal = [int](($currentProjects | Measure-Object branches_covered -Sum).Sum)
$branchesValidTotal = [int](($currentProjects | Measure-Object branches_valid -Sum).Sum)
$aggregateLineCoverage = if ($linesValidTotal -eq 0) {
    [Math]::Round((($currentProjects | Measure-Object line_coverage_percent -Average).Average), 6)
}
else {
    [Math]::Round(($linesCoveredTotal * 100.0) / $linesValidTotal, 6)
}
$aggregateBranchCoverage = if ($branchesValidTotal -eq 0) {
    [Math]::Round((($currentProjects | Measure-Object branch_coverage_percent -Average).Average), 6)
}
else {
    [Math]::Round(($branchesCoveredTotal * 100.0) / $branchesValidTotal, 6)
}

$auditBaseCommitValue = Get-PcvAuditBaseCommit -Root $repoRootFull
$sourceSnapshot = Get-PcvDotNetSourceSnapshot -Root $repoRootFull -AuditCommit $auditBaseCommitValue
$currentSnapshot = [ordered]@{
    schema_version = 'pcv-dotnet-quality-baseline/v1'
    generated_at_utc = [DateTimeOffset]::UtcNow.ToString('o')
    audit_base_commit = $auditBaseCommitValue
    source_snapshot = $sourceSnapshot
    dotnet_sdk_version = $sdkVersion.Trim()
    coverage_collector_version = $collectorVersion.Trim()
    project_count = $currentProjects.Count
    totals = [ordered]@{
        total = $totalTests
        executed = $totalExecuted
        skipped = $totalSkipped
        passed = $totalPassed
        failed = $totalFailed
        line_coverage_percent = $aggregateLineCoverage
        branch_coverage_percent = $aggregateBranchCoverage
        lines_covered = $linesCoveredTotal
        lines_valid = $linesValidTotal
        branches_covered = $branchesCoveredTotal
        branches_valid = $branchesValidTotal
    }
    projects = @($currentProjects)
}

$migrationManifest = Read-PcvJson -Path $migrationFull -Label 'migration manifest'
if ([string](Get-PcvRequiredProperty -InputObject $migrationManifest -Name 'schema_version' -Label 'migration manifest') -ne
    'pcv-dotnet-test-migrations/v1') {
    throw 'PCV_DOTNET_QUALITY_INVALID_MIGRATION: Unsupported migration manifest schema_version.'
}
$migrationEntries = @(Get-PcvRequiredProperty -InputObject $migrationManifest -Name 'migrations' -Label 'migration manifest')
$migrationByOldId = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
foreach ($migration in $migrationEntries) {
    try {
        $oldTestId = [string](Get-PcvRequiredProperty -InputObject $migration -Name 'old_test_id' -Label 'migration entry')
        $status = [string](Get-PcvRequiredProperty -InputObject $migration -Name 'status' -Label "migration '$oldTestId'")
        $owner = [string](Get-PcvRequiredProperty -InputObject $migration -Name 'owner' -Label "migration '$oldTestId'")
        $coverageBoundary = [string](Get-PcvRequiredProperty -InputObject $migration -Name 'coverage_boundary' -Label "migration '$oldTestId'")
        $replacementIds = if ($migration.PSObject.Properties.Name -contains 'replacement_test_ids') {
            @($migration.replacement_test_ids)
        }
        elseif ($migration.PSObject.Properties.Name -contains 'replacement_test_id') {
            @($migration.replacement_test_id)
        }
        else {
            @()
        }
        $replacementIds = @($replacementIds | ForEach-Object { [string]$_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        if ([string]::IsNullOrWhiteSpace($oldTestId) -or
            [string]::IsNullOrWhiteSpace($status) -or
            [string]::IsNullOrWhiteSpace($owner) -or
            [string]::IsNullOrWhiteSpace($coverageBoundary) -or
            $replacementIds.Count -eq 0) {
            throw 'old_test_id, status, replacement_test_id(s), owner, and coverage_boundary must be non-empty.'
        }
        $status = $status.Trim()
        if ($status -notin @('completed', 'planned')) {
            throw "migration '$oldTestId' has unsupported status '$status'."
        }
        if ($migrationByOldId.ContainsKey($oldTestId)) {
            throw "duplicate old_test_id '$oldTestId'."
        }
        $migrationByOldId.Add($oldTestId, [pscustomobject]@{
            old_test_id = $oldTestId
            status = $status
            replacement_test_ids = $replacementIds
            owner = $owner.Trim()
            coverage_boundary = $coverageBoundary.Trim()
        })
    }
    catch {
        throw "PCV_DOTNET_QUALITY_INVALID_MIGRATION: $($_.Exception.Message)"
    }
}

if ($WriteBaseline) {
    $baselineDirectory = Split-Path -Parent $baselineFull
    $baselineDirectory = Assert-PcvContainedPath -Root $repoRootFull -Candidate $baselineDirectory -Label 'BaselineDirectory' -AllowRoot
    New-Item -ItemType Directory -Path $baselineDirectory -Force | Out-Null
    $baselineFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $baselineFull -Label 'BaselinePath'
    $temporaryBaseline = Join-Path $baselineDirectory ('.pcv-quality-baseline-{0}.tmp' -f [Guid]::NewGuid().ToString('N'))
    $temporaryBaseline = Assert-PcvContainedPath -Root $baselineDirectory -Candidate $temporaryBaseline -Label 'TemporaryBaselinePath'
    try {
        $currentSnapshot | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $temporaryBaseline -Encoding utf8
        Move-Item -LiteralPath $temporaryBaseline -Destination $baselineFull -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporaryBaseline) {
            Remove-Item -LiteralPath $temporaryBaseline -Force
        }
    }

    [pscustomobject]@{
        ok = $true
        mode = 'baseline-written'
        baseline_path = $baselineFull
        project_count = $currentProjects.Count
        total = $totalTests
        skipped = $totalSkipped
        line_coverage_percent = $aggregateLineCoverage
        branch_coverage_percent = $aggregateBranchCoverage
        source_snapshot_sha256 = $sourceSnapshot.sha256
        mapped_removed_test_count = 0
    }
    exit 0
}

$baseline = Read-PcvJson -Path $baselineFull -Label 'quality baseline'
if ([string](Get-PcvRequiredProperty -InputObject $baseline -Name 'schema_version' -Label 'quality baseline') -ne
    'pcv-dotnet-quality-baseline/v1') {
    throw 'PCV_DOTNET_QUALITY_SCHEMA_INVALID: Unsupported quality baseline schema_version.'
}
$baselineAuditBaseCommit = [string](Get-PcvRequiredProperty -InputObject $baseline -Name 'audit_base_commit' -Label 'quality baseline')
$baselineSourceSnapshot = Get-PcvRequiredProperty -InputObject $baseline -Name 'source_snapshot' -Label 'quality baseline'
$baselineSourceSnapshotSchema = [string](Get-PcvRequiredProperty -InputObject $baselineSourceSnapshot -Name 'schema_version' -Label 'quality baseline source_snapshot')
$baselineSourceSnapshotAlgorithm = [string](Get-PcvRequiredProperty -InputObject $baselineSourceSnapshot -Name 'algorithm' -Label 'quality baseline source_snapshot')
$baselineSourceSnapshotSha256 = [string](Get-PcvRequiredProperty -InputObject $baselineSourceSnapshot -Name 'sha256' -Label 'quality baseline source_snapshot')
$baselineSourceSnapshotFileCount = [int](Get-PcvRequiredProperty -InputObject $baselineSourceSnapshot -Name 'file_count' -Label 'quality baseline source_snapshot')
if ([string]::IsNullOrWhiteSpace($baselineAuditBaseCommit) -or
    $baselineSourceSnapshotSchema -ne 'pcv-dotnet-source-snapshot/v1' -or
    $baselineSourceSnapshotAlgorithm -ne 'sha256' -or
    $baselineSourceSnapshotSha256 -notmatch '^[0-9a-f]{64}$' -or
    $baselineSourceSnapshotFileCount -lt 1) {
    throw 'PCV_DOTNET_QUALITY_SCHEMA_INVALID: Quality baseline source provenance is invalid.'
}
$baselineSdk = [string](Get-PcvRequiredProperty -InputObject $baseline -Name 'dotnet_sdk_version' -Label 'quality baseline')
$baselineCollector = [string](Get-PcvRequiredProperty -InputObject $baseline -Name 'coverage_collector_version' -Label 'quality baseline')
if ($baselineSdk -ne $sdkVersion -or $baselineCollector -ne $collectorVersion) {
    throw "PCV_DOTNET_QUALITY_TOOLCHAIN_DRIFT: Baseline SDK/collector '$baselineSdk/$baselineCollector' differs from current '$sdkVersion/$collectorVersion'."
}

$baselineProjects = @(Get-PcvRequiredProperty -InputObject $baseline -Name 'projects' -Label 'quality baseline')
$baselineTotals = Get-PcvRequiredProperty -InputObject $baseline -Name 'totals' -Label 'quality baseline'
$baselineTotalTests = [int](Get-PcvRequiredProperty -InputObject $baselineTotals -Name 'total' -Label 'quality baseline totals')
if ($totalTests -lt $baselineTotalTests) {
    throw "PCV_DOTNET_QUALITY_TEST_COUNT_DECREASE: Aggregate test count declined from $baselineTotalTests to $totalTests; allowed decline is 0."
}
$baselineTotalSkipped = [int](Get-PcvRequiredProperty -InputObject $baselineTotals -Name 'skipped' -Label 'quality baseline totals')
if ($totalSkipped -gt $baselineTotalSkipped) {
    throw "PCV_DOTNET_QUALITY_SKIP_INCREASE: Aggregate skipped tests increased from $baselineTotalSkipped to $totalSkipped."
}
$baselineTotalLine = [double](Get-PcvRequiredProperty -InputObject $baselineTotals -Name 'line_coverage_percent' -Label 'quality baseline totals')
$baselineTotalBranch = [double](Get-PcvRequiredProperty -InputObject $baselineTotals -Name 'branch_coverage_percent' -Label 'quality baseline totals')
if ($aggregateLineCoverage -lt $baselineTotalLine) {
    throw "PCV_DOTNET_QUALITY_COVERAGE_REGRESSION: Aggregate line coverage declined from $baselineTotalLine to $aggregateLineCoverage percentage points; allowed decline is 0.0."
}
if ($aggregateBranchCoverage -lt $baselineTotalBranch) {
    throw "PCV_DOTNET_QUALITY_COVERAGE_REGRESSION: Aggregate branch coverage declined from $baselineTotalBranch to $aggregateBranchCoverage percentage points; allowed decline is 0.0."
}
$duplicateBaselineProjects = @($baselineProjects |
    Group-Object { ([string]$_.name).ToUpperInvariant() } |
    Where-Object Count -gt 1)
if ($duplicateBaselineProjects.Count -gt 0) {
    throw 'PCV_DOTNET_QUALITY_DUPLICATE_PROJECT: Quality baseline contains duplicate project names.'
}

$currentByName = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($project in $currentProjects) {
    $currentByName.Add([string]$project.name, $project)
}
$currentTestIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($project in $currentProjects) {
    foreach ($testId in @($project.test_ids)) {
        [void]$currentTestIds.Add([string]$testId)
    }
}

$removedTestIds = [System.Collections.Generic.List[string]]::new()
foreach ($baselineProject in $baselineProjects) {
    $baselineProjectName = [string](Get-PcvRequiredProperty -InputObject $baselineProject -Name 'name' -Label 'baseline project')
    if (-not $currentByName.ContainsKey($baselineProjectName)) {
        throw "PCV_DOTNET_QUALITY_PROJECT_REMOVED: Baseline project '$baselineProjectName' is absent from current results."
    }
    $currentProject = $currentByName[$baselineProjectName]
    $baselineSkipped = [int](Get-PcvRequiredProperty -InputObject $baselineProject -Name 'skipped' -Label "baseline project '$baselineProjectName'")
    if ([int]$currentProject.skipped -gt $baselineSkipped) {
        throw "PCV_DOTNET_QUALITY_SKIP_INCREASE: Project '$baselineProjectName' skipped tests increased from $baselineSkipped to $($currentProject.skipped)."
    }

    $baselineLine = [double](Get-PcvRequiredProperty -InputObject $baselineProject -Name 'line_coverage_percent' -Label "baseline project '$baselineProjectName'")
    $baselineBranch = [double](Get-PcvRequiredProperty -InputObject $baselineProject -Name 'branch_coverage_percent' -Label "baseline project '$baselineProjectName'")
    if ([double]$currentProject.line_coverage_percent -lt $baselineLine) {
        throw "PCV_DOTNET_QUALITY_COVERAGE_REGRESSION: Project '$baselineProjectName' line coverage declined from $baselineLine to $($currentProject.line_coverage_percent) percentage points; allowed decline is 0.0."
    }
    if ([double]$currentProject.branch_coverage_percent -lt $baselineBranch) {
        throw "PCV_DOTNET_QUALITY_COVERAGE_REGRESSION: Project '$baselineProjectName' branch coverage declined from $baselineBranch to $($currentProject.branch_coverage_percent) percentage points; allowed decline is 0.0."
    }

    $baselineTestIds = @(Get-PcvRequiredProperty -InputObject $baselineProject -Name 'test_ids' -Label "baseline project '$baselineProjectName'")
    foreach ($baselineTestId in $baselineTestIds) {
        if (-not $currentTestIds.Contains([string]$baselineTestId)) {
            $removedTestIds.Add([string]$baselineTestId)
        }
    }
}

$claimedReplacementIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($removedTestId in $removedTestIds) {
    $matchingMigrations = @($migrationByOldId.Values | Where-Object {
        [string]$_.status -eq 'completed' -and
            (Test-PcvTestIdPattern -Pattern ([string]$_.old_test_id) -TestId $removedTestId)
    })
    if ($matchingMigrations.Count -eq 0) {
        throw "PCV_DOTNET_QUALITY_UNMAPPED_TEST_REMOVAL: Removed test '$removedTestId' has no completed migration entry."
    }
    if ($matchingMigrations.Count -gt 1) {
        throw "PCV_DOTNET_QUALITY_INVALID_MIGRATION: Removed test '$removedTestId' matches multiple migration entries."
    }
    $migration = $matchingMigrations[0]
    $liveReplacements = @(Resolve-PcvMigrationReplacements `
        -Migration $migration `
        -RemovedTestId $removedTestId `
        -CurrentTestIds $currentTestIds)
    if ($liveReplacements.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_INVALID_MIGRATION: Removed test '$removedTestId' must induce exactly one replacement_test_id present in current results."
    }
    if (-not $claimedReplacementIds.Add([string]$liveReplacements[0])) {
        throw "PCV_DOTNET_QUALITY_INVALID_MIGRATION: Replacement test '$($liveReplacements[0])' is claimed by more than one removed test."
    }
}

[pscustomobject]@{
    ok = $true
    mode = 'ratchet-verified'
    baseline_path = $baselineFull
    project_count = $currentProjects.Count
    total = $totalTests
    skipped = $totalSkipped
    line_coverage_percent = $aggregateLineCoverage
    branch_coverage_percent = $aggregateBranchCoverage
    baseline_source_snapshot_sha256 = $baselineSourceSnapshotSha256
    current_source_snapshot_sha256 = $sourceSnapshot.sha256
    mapped_removed_test_count = $removedTestIds.Count
}
