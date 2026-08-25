[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$SolutionPath,

    [Parameter(Mandatory)]
    [string]$ArtifactRoot,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$RepoRoot = (Join-Path $PSScriptRoot '../../..'),

    [string]$DotNetPath = 'dotnet',

    [scriptblock]$CommandRunner
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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

function Get-PcvRelativePath {
    param(
        [Parameter(Mandatory)]
        [string]$BasePath,

        [Parameter(Mandatory)]
        [string]$Path
    )

    ([IO.Path]::GetRelativePath($BasePath, $Path) -replace '\\', '/')
}

function Invoke-PcvQualityCommand {
    param(
        [Parameter(Mandatory)]
        [string]$FileName,

        [Parameter(Mandatory)]
        [string[]]$Arguments,

        [Parameter(Mandatory)]
        [string]$WorkingDirectory
    )

    if ($null -ne $CommandRunner) {
        $customResult = & $CommandRunner $FileName $Arguments $WorkingDirectory
        if ($null -eq $customResult -or
            $customResult.PSObject.Properties.Name -notcontains 'exit_code') {
            throw 'PCV_DOTNET_QUALITY_INVALID_RUNNER_RESULT: CommandRunner must return exit_code, stdout, and stderr.'
        }

        return [pscustomobject]@{
            exit_code = [int]$customResult.exit_code
            stdout = if ($customResult.PSObject.Properties.Name -contains 'stdout') { [string]$customResult.stdout } else { '' }
            stderr = if ($customResult.PSObject.Properties.Name -contains 'stderr') { [string]$customResult.stderr } else { '' }
        }
    }

    Push-Location -LiteralPath $WorkingDirectory
    try {
        $commandOutput = @(& $FileName @Arguments 2>&1)
        $commandExitCode = $LASTEXITCODE
        [pscustomobject]@{
            exit_code = [int]$commandExitCode
            stdout = ($commandOutput | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
            stderr = ''
        }
    }
    finally {
        Pop-Location
    }
}

$repoRootFull = [IO.Path]::GetFullPath($RepoRoot)
if (-not (Test-Path -LiteralPath $repoRootFull -PathType Container)) {
    throw "PCV_DOTNET_QUALITY_REPO_ROOT_MISSING: Repository root '$repoRootFull' does not exist."
}

$solutionFull = Get-PcvFullPath -Path $SolutionPath -BasePath $repoRootFull
$solutionFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $solutionFull -Label 'SolutionPath'
if (-not (Test-Path -LiteralPath $solutionFull -PathType Leaf)) {
    throw "PCV_DOTNET_QUALITY_SOLUTION_MISSING: Solution '$solutionFull' does not exist."
}

$artifactRootFull = Get-PcvFullPath -Path $ArtifactRoot -BasePath $repoRootFull
$artifactRootFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $artifactRootFull -Label 'ArtifactRoot'
$solutionDirectory = Split-Path -Parent $solutionFull
$solutionDirectory = Assert-PcvContainedPath -Root $repoRootFull -Candidate $solutionDirectory -Label 'SolutionDirectory' -AllowRoot

$testProjects = @(Get-ChildItem -LiteralPath $solutionDirectory -Recurse -File -Filter '*.Tests.csproj' |
    Sort-Object FullName)
if ($testProjects.Count -eq 0) {
    throw "PCV_DOTNET_QUALITY_NO_TEST_PROJECTS: No *.Tests.csproj files were found below '$solutionDirectory'."
}

$projectRecords = [System.Collections.Generic.List[object]]::new()
foreach ($testProject in $testProjects) {
    $projectPath = Assert-PcvContainedPath -Root $repoRootFull -Candidate $testProject.FullName -Label 'TestProject'
    $projectName = [IO.Path]::GetFileNameWithoutExtension($projectPath)
    $projectXml = Read-PcvSafeXml -Path $projectPath -Label 'test project'
    $collectorReferences = @($projectXml.SelectNodes("//*[local-name()='PackageReference']") |
        Where-Object { [string]$_.GetAttribute('Include') -ieq 'coverlet.collector' })
    if ($collectorReferences.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_COLLECTOR_REFERENCE: Project '$projectName' must contain exactly one coverlet.collector PackageReference."
    }

    $collectorVersion = [string]$collectorReferences[0].GetAttribute('Version')
    if ([string]::IsNullOrWhiteSpace($collectorVersion)) {
        $versionNode = $collectorReferences[0].SelectSingleNode("./*[local-name()='Version']")
        if ($null -ne $versionNode) {
            $collectorVersion = [string]$versionNode.InnerText
        }
    }
    if ([string]::IsNullOrWhiteSpace($collectorVersion)) {
        throw "PCV_DOTNET_QUALITY_COLLECTOR_REFERENCE: Project '$projectName' does not declare a coverlet.collector version."
    }

    $projectRecords.Add([pscustomobject]@{
        name = $projectName
        path = $projectPath
        collector_version = $collectorVersion.Trim()
    })
}

$duplicateNames = @($projectRecords |
    Group-Object { ([string]$_.name).ToUpperInvariant() } |
    Where-Object Count -gt 1)
if ($duplicateNames.Count -gt 0) {
    $duplicates = @($duplicateNames | ForEach-Object { @($_.Group.name) -join ', ' }) -join '; '
    throw "PCV_DOTNET_QUALITY_DUPLICATE_PROJECT: Test project names must be unique. Duplicates: $duplicates"
}

$collectorVersions = @($projectRecords.collector_version | Sort-Object -Unique)
if ($collectorVersions.Count -ne 1) {
    throw "PCV_DOTNET_QUALITY_COLLECTOR_VERSION_MISMATCH: Test projects use multiple coverlet.collector versions: $($collectorVersions -join ', ')."
}

$sdkResult = Invoke-PcvQualityCommand -FileName $DotNetPath -Arguments @('--version') -WorkingDirectory $repoRootFull
if ($sdkResult.exit_code -ne 0) {
    throw "PCV_DOTNET_QUALITY_DOTNET_VERSION_FAILED: '$DotNetPath --version' exited $($sdkResult.exit_code). $($sdkResult.stderr)"
}
$sdkVersion = @($sdkResult.stdout -split '\r?\n' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
if ($sdkVersion.Count -ne 1) {
    throw 'PCV_DOTNET_QUALITY_DOTNET_VERSION_FAILED: dotnet --version returned no SDK version.'
}
$sdkVersion = ([string]$sdkVersion[0]).Trim()

New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
$artifactRootFull = Assert-PcvContainedPath -Root $repoRootFull -Candidate $artifactRootFull -Label 'ArtifactRoot'
$resultsRoot = Join-Path $artifactRootFull 'test-results'
$resultsRoot = Assert-PcvContainedPath -Root $artifactRootFull -Candidate $resultsRoot -Label 'ResultsRoot'
$summaryPath = Join-Path $artifactRootFull 'quality-capture-summary.json'
$summaryPath = Assert-PcvContainedPath -Root $artifactRootFull -Candidate $summaryPath -Label 'CaptureSummaryPath'
if (Test-Path -LiteralPath $summaryPath -PathType Leaf) {
    $summaryPath = Assert-PcvContainedPath -Root $artifactRootFull -Candidate $summaryPath -Label 'CaptureSummaryPath'
    Remove-Item -LiteralPath $summaryPath -Force
}
if (Test-Path -LiteralPath $resultsRoot) {
    $resultsRoot = Assert-PcvContainedPath -Root $artifactRootFull -Candidate $resultsRoot -Label 'ResultsRoot'
    Remove-Item -LiteralPath $resultsRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null
$resultsRoot = Assert-PcvContainedPath -Root $artifactRootFull -Candidate $resultsRoot -Label 'ResultsRoot'

$capturedProjects = [System.Collections.Generic.List[object]]::new()
foreach ($project in $projectRecords) {
    $projectResultDirectory = Join-Path $resultsRoot $project.name
    $projectResultDirectory = Assert-PcvContainedPath -Root $resultsRoot -Candidate $projectResultDirectory -Label 'ProjectResultDirectory'
    if (Test-Path -LiteralPath $projectResultDirectory) {
        $projectResultDirectory = Assert-PcvContainedPath -Root $resultsRoot -Candidate $projectResultDirectory -Label 'ProjectResultDirectory'
        Remove-Item -LiteralPath $projectResultDirectory -Recurse -Force
    }
    New-Item -ItemType Directory -Path $projectResultDirectory -Force | Out-Null
    $projectResultDirectory = Assert-PcvContainedPath -Root $resultsRoot -Candidate $projectResultDirectory -Label 'ProjectResultDirectory'

    $testArguments = @(
        'test',
        $project.path,
        '-c',
        $Configuration,
        '-p:PcvQualityCapture=true',
        '--collect:XPlat Code Coverage',
        '--logger',
        "trx;LogFileName=$($project.name).trx",
        '--results-directory',
        $projectResultDirectory
    )
    $testResult = Invoke-PcvQualityCommand `
        -FileName $DotNetPath `
        -Arguments $testArguments `
        -WorkingDirectory $repoRootFull
    if ($testResult.exit_code -ne 0) {
        throw "PCV_DOTNET_QUALITY_TEST_FAILED: Project '$($project.name)' exited $($testResult.exit_code). $($testResult.stdout) $($testResult.stderr)"
    }

    $projectResultDirectory = Assert-PcvContainedPath -Root $resultsRoot -Candidate $projectResultDirectory -Label 'ProjectResultDirectory'
    $trxFiles = @(Get-ChildItem -LiteralPath $projectResultDirectory -Recurse -File -Filter '*.trx')
    $coverageFiles = @(Get-ChildItem -LiteralPath $projectResultDirectory -Recurse -File |
        Where-Object Name -eq 'coverage.cobertura.xml')
    $trxFiles = @($trxFiles | ForEach-Object {
        $safePath = Assert-PcvContainedPath -Root $projectResultDirectory -Candidate $_.FullName -Label 'TRX artifact'
        Get-Item -LiteralPath $safePath -Force
    })
    $coverageFiles = @($coverageFiles | ForEach-Object {
        $safePath = Assert-PcvContainedPath -Root $projectResultDirectory -Candidate $_.FullName -Label 'Cobertura artifact'
        Get-Item -LiteralPath $safePath -Force
    })
    if ($trxFiles.Count -ne 1 -or $coverageFiles.Count -eq 0) {
        throw "PCV_DOTNET_QUALITY_RESULT_ARTIFACT_MISSING: Project '$($project.name)' produced $($trxFiles.Count) TRX and $($coverageFiles.Count) Cobertura files; exactly one TRX and at least one Cobertura file are required."
    }
    $coverageHashes = @($coverageFiles | ForEach-Object {
        (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    } | Sort-Object -Unique)
    if ($coverageHashes.Count -ne 1) {
        throw "PCV_DOTNET_QUALITY_CONFLICTING_COBERTURA: Project '$($project.name)' produced $($coverageFiles.Count) non-identical Cobertura files."
    }
    $canonicalCoverage = @($coverageFiles | Sort-Object @{ Expression = { $_.FullName.Length } }, FullName)[0]

    $capturedProjects.Add([ordered]@{
        name = $project.name
        project_path = Get-PcvRelativePath -BasePath $repoRootFull -Path $project.path
        result_directory = Get-PcvRelativePath -BasePath $artifactRootFull -Path $projectResultDirectory
        trx_path = Get-PcvRelativePath -BasePath $artifactRootFull -Path $trxFiles[0].FullName
        cobertura_path = Get-PcvRelativePath -BasePath $artifactRootFull -Path $canonicalCoverage.FullName
        cobertura_copy_count = $coverageFiles.Count
        cobertura_paths = @($coverageFiles | Sort-Object FullName | ForEach-Object {
            Get-PcvRelativePath -BasePath $artifactRootFull -Path $_.FullName
        })
    })
}

$summary = [ordered]@{
    schema_version = 'pcv-dotnet-quality-capture/v1'
    ok = $true
    captured_at_utc = [DateTimeOffset]::UtcNow.ToString('o')
    configuration = $Configuration
    dotnet_sdk_version = $sdkVersion
    coverage_collector_version = $collectorVersions[0]
    solution_path = Get-PcvRelativePath -BasePath $repoRootFull -Path $solutionFull
    results_root = Get-PcvRelativePath -BasePath $artifactRootFull -Path $resultsRoot
    project_count = $capturedProjects.Count
    projects = @($capturedProjects)
}
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $summaryPath -Encoding utf8

[pscustomobject]$summary
