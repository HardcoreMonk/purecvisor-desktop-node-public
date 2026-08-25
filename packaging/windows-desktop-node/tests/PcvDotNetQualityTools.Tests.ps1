BeforeAll {
    $script:CapturePath = Join-Path $PSScriptRoot '../tools/Invoke-PcvDotNetQualityCapture.ps1'
    $script:RatchetPath = Join-Path $PSScriptRoot '../tools/Test-PcvDotNetQualityRatchet.ps1'

    function Write-TestProject {
        param(
            [Parameter(Mandatory)]
            [string]$Path,

            [string]$CollectorVersion = '6.0.4'
        )

        $parent = Split-Path -Parent $Path
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
        @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0-windows</TargetFramework></PropertyGroup>
  <ItemGroup><PackageReference Include="coverlet.collector" Version="$CollectorVersion" /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $Path -Encoding utf8
    }

    function Write-TrxFixture {
        param(
            [Parameter(Mandatory)]
            [string]$Path,

            [string[]]$Passed = @('Alpha', 'Beta'),

            [string[]]$Skipped = @()
        )

        $parent = Split-Path -Parent $Path
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
        $results = [System.Collections.Generic.List[string]]::new()
        $ordinal = 0
        foreach ($name in @($Passed)) {
            $ordinal++
            $escaped = [System.Security.SecurityElement]::Escape($name)
            $results.Add("    <UnitTestResult testId=`"pass-$ordinal`" testName=`"$escaped`" outcome=`"Passed`" />")
        }
        foreach ($name in @($Skipped)) {
            $ordinal++
            $escaped = [System.Security.SecurityElement]::Escape($name)
            $results.Add("    <UnitTestResult testId=`"skip-$ordinal`" testName=`"$escaped`" outcome=`"NotExecuted`" />")
        }

        $total = @($Passed).Count + @($Skipped).Count
        $executed = @($Passed).Count
        $resultXml = $results -join [Environment]::NewLine
        @"
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
$resultXml
  </Results>
  <ResultSummary outcome="Completed">
    <Counters total="$total" executed="$executed" passed="$executed" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="$(@($Skipped).Count)" disconnected="0" warning="0" completed="$executed" inProgress="0" pending="0" />
  </ResultSummary>
</TestRun>
"@ | Set-Content -LiteralPath $Path -Encoding utf8
    }

    function Write-CoberturaFixture {
        param(
            [Parameter(Mandatory)]
            [string]$Path,

            [double]$LineRate = 0.8,

            [double]$BranchRate = 0.7,

            [int]$LinesValid = 100,

            [int]$BranchesValid = 10
        )

        $parent = Split-Path -Parent $Path
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
        $lineRateText = $LineRate.ToString('0.####', [System.Globalization.CultureInfo]::InvariantCulture)
        $branchRateText = $BranchRate.ToString('0.####', [System.Globalization.CultureInfo]::InvariantCulture)
        $linesCovered = [int][Math]::Round($LineRate * $LinesValid)
        $branchesCovered = [int][Math]::Round($BranchRate * $BranchesValid)
        @"
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="$lineRateText" branch-rate="$branchRateText" lines-covered="$linesCovered" lines-valid="$LinesValid" branches-covered="$branchesCovered" branches-valid="$BranchesValid" timestamp="0" version="fixture">
  <sources /><packages />
</coverage>
"@ | Set-Content -LiteralPath $Path -Encoding utf8
    }

    function New-QualityFixture {
        param(
            [string]$Name = ([Guid]::NewGuid().ToString('N')),
            [string[]]$Passed = @('Alpha', 'Beta'),
            [string[]]$Skipped = @(),
            [double]$LineRate = 0.8,
            [double]$BranchRate = 0.7
        )

        $repoRoot = Join-Path $TestDrive $Name
        $srcRoot = Join-Path $repoRoot 'src'
        $artifactRoot = Join-Path $repoRoot 'artifacts/quality'
        $resultsRoot = Join-Path $artifactRoot 'test-results'
        $projectName = 'Example.Tests'
        $projectRoot = Join-Path $resultsRoot $projectName
        $baselinePath = Join-Path $repoRoot 'quality-baseline.json'
        $migrationPath = Join-Path $repoRoot 'quality-migrations.json'
        $solutionPath = Join-Path $srcRoot 'Example.sln'

        New-Item -ItemType Directory -Path $srcRoot -Force | Out-Null
        'Microsoft Visual Studio Solution File, Format Version 12.00' |
            Set-Content -LiteralPath $solutionPath -Encoding utf8
        Write-TestProject -Path (Join-Path $srcRoot "$projectName/$projectName.csproj")
        Write-TrxFixture -Path (Join-Path $projectRoot "$projectName.trx") -Passed $Passed -Skipped $Skipped
        Write-CoberturaFixture -Path (Join-Path $projectRoot 'coverage.cobertura.xml') -LineRate $LineRate -BranchRate $BranchRate

        [ordered]@{
            schema_version = 'pcv-dotnet-quality-capture/v1'
            dotnet_sdk_version = '10.0.100'
            coverage_collector_version = '6.0.4'
            solution_path = 'src/Example.sln'
            results_root = 'test-results'
            projects = @(
                [ordered]@{
                    name = $projectName
                    project_path = "src/$projectName/$projectName.csproj"
                    result_directory = "test-results/$projectName"
                    trx_path = "test-results/$projectName/$projectName.trx"
                    cobertura_path = "test-results/$projectName/coverage.cobertura.xml"
                }
            )
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $artifactRoot 'quality-capture-summary.json') -Encoding utf8

        [ordered]@{
            schema_version = 'pcv-dotnet-test-migrations/v1'
            migrations = @()
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $migrationPath -Encoding utf8

        [pscustomobject]@{
            RepoRoot = $repoRoot
            SolutionPath = $solutionPath
            ArtifactRoot = $artifactRoot
            ResultsRoot = $resultsRoot
            ProjectName = $projectName
            ProjectRoot = $projectRoot
            TrxPath = Join-Path $projectRoot "$projectName.trx"
            CoberturaPath = Join-Path $projectRoot 'coverage.cobertura.xml'
            BaselinePath = $baselinePath
            MigrationPath = $migrationPath
        }
    }

    function Write-QualityBaseline {
        param([Parameter(Mandatory)]$Fixture)

        & $script:RatchetPath `
            -RepoRoot $Fixture.RepoRoot `
            -ResultsRoot $Fixture.ResultsRoot `
            -BaselinePath $Fixture.BaselinePath `
            -MigrationManifestPath $Fixture.MigrationPath `
            -AuditBaseCommit 'unavailable' `
            -WriteBaseline | Out-Null
    }
}

Describe 'Invoke-PcvDotNetQualityCapture' {
    It 'collects each project into an isolated fixed result directory and records tool versions' {
        $fixture = New-QualityFixture -Name 'capture-success'
        Remove-Item -LiteralPath $fixture.ArtifactRoot -Recurse -Force
        $calls = [System.Collections.Generic.List[object]]::new()
        $runner = {
            param($FileName, $Arguments, $WorkingDirectory)

            $calls.Add([pscustomobject]@{ file = $FileName; args = @($Arguments); cwd = $WorkingDirectory })
            if (@($Arguments).Count -eq 1 -and $Arguments[0] -eq '--version') {
                return [pscustomobject]@{ exit_code = 0; stdout = '10.0.100'; stderr = '' }
            }

            $resultsIndex = [Array]::IndexOf([object[]]$Arguments, '--results-directory')
            $resultDirectory = [string]$Arguments[$resultsIndex + 1]
            $projectPath = [string]$Arguments[1]
            $projectName = [IO.Path]::GetFileNameWithoutExtension($projectPath)
            Write-TrxFixture -Path (Join-Path $resultDirectory "$projectName.trx")
            Write-CoberturaFixture -Path (Join-Path $resultDirectory 'coverage.cobertura.xml')
            [pscustomobject]@{ exit_code = 0; stdout = 'Passed'; stderr = '' }
        }

        $result = & $script:CapturePath `
            -RepoRoot $fixture.RepoRoot `
            -SolutionPath $fixture.SolutionPath `
            -ArtifactRoot $fixture.ArtifactRoot `
            -CommandRunner $runner

        $result.ok | Should -BeTrue
        $result.project_count | Should -Be 1
        $result.dotnet_sdk_version | Should -Be '10.0.100'
        $result.coverage_collector_version | Should -Be '6.0.4'
        @($calls).Count | Should -Be 2
        @($calls)[1].args | Should -Contain '-p:PcvQualityCapture=true'
        Test-Path -LiteralPath (Join-Path $fixture.ArtifactRoot 'test-results/Example.Tests/Example.Tests.trx') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $fixture.ArtifactRoot 'test-results/Example.Tests/coverage.cobertura.xml') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $fixture.ArtifactRoot 'quality-capture-summary.json') | Should -BeTrue
    }

    It 'rejects an artifact root outside the declared repository before running dotnet' {
        $fixture = New-QualityFixture -Name 'capture-containment'
        $outside = Join-Path $TestDrive 'outside-capture'
        $runner = { throw 'runner must not execute' }

        {
            & $script:CapturePath `
                -RepoRoot $fixture.RepoRoot `
                -SolutionPath $fixture.SolutionPath `
                -ArtifactRoot $outside `
                -CommandRunner $runner
        } | Should -Throw '*PCV_DOTNET_QUALITY_PATH_OUTSIDE_REPO*'
    }

    It 'rejects a reparse-point results directory before recursive cleanup' {
        $fixture = New-QualityFixture -Name 'capture-results-junction'
        $outsideTarget = Join-Path $TestDrive 'outside-capture-results-target'
        $sentinelPath = Join-Path $outsideTarget 'must-survive.txt'
        Remove-Item -LiteralPath $fixture.ResultsRoot -Recurse -Force
        New-Item -ItemType Directory -Path $outsideTarget -Force | Out-Null
        'preserve' | Set-Content -LiteralPath $sentinelPath -Encoding utf8
        New-Item -ItemType Junction -Path $fixture.ResultsRoot -Target $outsideTarget | Out-Null
        $calls = [System.Collections.Generic.List[object]]::new()
        $runner = {
            param($FileName, $Arguments, $WorkingDirectory)

            $calls.Add([pscustomobject]@{ file = $FileName; args = @($Arguments); cwd = $WorkingDirectory })
            if (@($Arguments).Count -eq 1 -and $Arguments[0] -eq '--version') {
                return [pscustomobject]@{ exit_code = 0; stdout = '10.0.100'; stderr = '' }
            }
            throw 'test runner must not execute'
        }

        try {
            {
                & $script:CapturePath `
                    -RepoRoot $fixture.RepoRoot `
                    -SolutionPath $fixture.SolutionPath `
                    -ArtifactRoot $fixture.ArtifactRoot `
                    -CommandRunner $runner
            } | Should -Throw '*PCV_DOTNET_QUALITY_PATH_REPARSE_POINT*ResultsRoot*'

            Test-Path -LiteralPath $sentinelPath -PathType Leaf | Should -BeTrue
            @($calls).Count | Should -Be 1
        }
        finally {
            if (Test-Path -LiteralPath $fixture.ResultsRoot) {
                [IO.Directory]::Delete($fixture.ResultsRoot)
            }
        }
    }

    It 'rejects duplicate test project names before running dotnet' {
        $fixture = New-QualityFixture -Name 'capture-duplicate-project'
        Remove-Item -LiteralPath (Join-Path $fixture.RepoRoot 'src/Example.Tests') -Recurse -Force
        Write-TestProject -Path (Join-Path $fixture.RepoRoot 'src/a/Duplicate.Tests.csproj')
        Write-TestProject -Path (Join-Path $fixture.RepoRoot 'src/b/Duplicate.Tests.csproj')
        $runner = { throw 'runner must not execute' }

        {
            & $script:CapturePath `
                -RepoRoot $fixture.RepoRoot `
                -SolutionPath $fixture.SolutionPath `
                -ArtifactRoot $fixture.ArtifactRoot `
                -CommandRunner $runner
        } | Should -Throw '*PCV_DOTNET_QUALITY_DUPLICATE_PROJECT*'
    }
}

Describe 'Test-PcvDotNetQualityRatchet baseline contract' {
    It 'requires explicit WriteBaseline and records reproducible source provenance plus quality metrics' {
        $fixture = New-QualityFixture -Name 'baseline-contract'

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_BASELINE_MISSING*WriteBaseline*'

        Write-QualityBaseline -Fixture $fixture
        $baseline = Get-Content -LiteralPath $fixture.BaselinePath -Raw | ConvertFrom-Json
        $baseline.schema_version | Should -Be 'pcv-dotnet-quality-baseline/v1'
        $baseline.audit_base_commit | Should -Be 'unavailable'
        $baseline.source_snapshot.schema_version | Should -Be 'pcv-dotnet-source-snapshot/v1'
        $baseline.source_snapshot.algorithm | Should -Be 'sha256'
        $baseline.source_snapshot.sha256 | Should -Match '^[0-9a-f]{64}$'
        $baseline.source_snapshot.file_count | Should -Be 2
        $baseline.source_snapshot.deleted_path_count | Should -Be 0
        $baseline.dotnet_sdk_version | Should -Be '10.0.100'
        $baseline.coverage_collector_version | Should -Be '6.0.4'
        $baseline.totals.total | Should -Be 2
        $baseline.totals.executed | Should -Be 2
        $baseline.totals.skipped | Should -Be 0
        $baseline.totals.line_coverage_percent | Should -Be 80
        $baseline.totals.branch_coverage_percent | Should -Be 70
        @($baseline.projects[0].test_ids) | Should -Be @('Example.Tests.Alpha', 'Example.Tests.Beta')

        $initialSnapshot = $baseline.source_snapshot.sha256
        Write-QualityBaseline -Fixture $fixture
        $unchanged = Get-Content -LiteralPath $fixture.BaselinePath -Raw | ConvertFrom-Json
        $unchanged.source_snapshot.sha256 | Should -Be $initialSnapshot

        'internal sealed class SnapshotProbe { }' |
            Set-Content -LiteralPath (Join-Path $fixture.RepoRoot 'src/Example.Tests/SnapshotProbe.cs') -Encoding utf8
        Write-QualityBaseline -Fixture $fixture
        $changed = Get-Content -LiteralPath $fixture.BaselinePath -Raw | ConvertFrom-Json
        $changed.source_snapshot.sha256 | Should -Not -Be $initialSnapshot
    }

    It 'rejects inputs and outputs outside the declared repository' {
        $fixture = New-QualityFixture -Name 'ratchet-containment'
        $outsideBaseline = Join-Path $TestDrive 'outside-baseline.json'

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $outsideBaseline `
                -MigrationManifestPath $fixture.MigrationPath `
                -WriteBaseline
        } | Should -Throw '*PCV_DOTNET_QUALITY_PATH_OUTSIDE_REPO*'
    }

    It 'rejects a reparse-point results input before reading external artifacts' {
        $fixture = New-QualityFixture -Name 'ratchet-results-junction'
        $outsideTarget = Join-Path $TestDrive 'outside-ratchet-results-target'
        $sentinelPath = Join-Path $outsideTarget 'must-survive.txt'
        Remove-Item -LiteralPath $fixture.ResultsRoot -Recurse -Force
        New-Item -ItemType Directory -Path $outsideTarget -Force | Out-Null
        'preserve' | Set-Content -LiteralPath $sentinelPath -Encoding utf8
        New-Item -ItemType Junction -Path $fixture.ResultsRoot -Target $outsideTarget | Out-Null

        try {
            {
                & $script:RatchetPath `
                    -RepoRoot $fixture.RepoRoot `
                    -ResultsRoot $fixture.ResultsRoot `
                    -BaselinePath $fixture.BaselinePath `
                    -MigrationManifestPath $fixture.MigrationPath `
                    -WriteBaseline
            } | Should -Throw '*PCV_DOTNET_QUALITY_PATH_REPARSE_POINT*ResultsRoot*'

            Test-Path -LiteralPath $sentinelPath -PathType Leaf | Should -BeTrue
        }
        finally {
            if (Test-Path -LiteralPath $fixture.ResultsRoot) {
                [IO.Directory]::Delete($fixture.ResultsRoot)
            }
        }
    }

    It 'rejects a reparse-point baseline parent before writing outside the repository' {
        $fixture = New-QualityFixture -Name 'ratchet-baseline-junction'
        $outsideTarget = Join-Path $TestDrive 'outside-ratchet-baseline-target'
        $sentinelPath = Join-Path $outsideTarget 'must-survive.txt'
        $baselineJunction = Join-Path $fixture.RepoRoot 'baseline-output'
        $baselinePath = Join-Path $baselineJunction 'quality-baseline.json'
        New-Item -ItemType Directory -Path $outsideTarget -Force | Out-Null
        'preserve' | Set-Content -LiteralPath $sentinelPath -Encoding utf8
        New-Item -ItemType Junction -Path $baselineJunction -Target $outsideTarget | Out-Null

        try {
            {
                & $script:RatchetPath `
                    -RepoRoot $fixture.RepoRoot `
                    -ResultsRoot $fixture.ResultsRoot `
                    -BaselinePath $baselinePath `
                    -MigrationManifestPath $fixture.MigrationPath `
                    -WriteBaseline
            } | Should -Throw '*PCV_DOTNET_QUALITY_PATH_REPARSE_POINT*BaselinePath*'

            Test-Path -LiteralPath $sentinelPath -PathType Leaf | Should -BeTrue
            Test-Path -LiteralPath (Join-Path $outsideTarget 'quality-baseline.json') | Should -BeFalse
        }
        finally {
            if (Test-Path -LiteralPath $baselineJunction) {
                [IO.Directory]::Delete($baselineJunction)
            }
        }
    }

    It 'rejects malformed TRX XML' {
        $fixture = New-QualityFixture -Name 'malformed-trx'
        '<TestRun><broken>' | Set-Content -LiteralPath $fixture.TrxPath -Encoding utf8

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath `
                -WriteBaseline
        } | Should -Throw '*PCV_DOTNET_QUALITY_INVALID_XML*TRX*'
    }

    It 'rejects malformed Cobertura XML' {
        $fixture = New-QualityFixture -Name 'malformed-cobertura'
        '<coverage><broken>' | Set-Content -LiteralPath $fixture.CoberturaPath -Encoding utf8

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath `
                -WriteBaseline
        } | Should -Throw '*PCV_DOTNET_QUALITY_INVALID_XML*Cobertura*'
    }

    It 'rejects duplicate project artifacts' {
        $fixture = New-QualityFixture -Name 'duplicate-artifacts'
        Copy-Item -LiteralPath $fixture.TrxPath -Destination (Join-Path $fixture.ProjectRoot 'duplicate.trx')

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath `
                -WriteBaseline
        } | Should -Throw '*PCV_DOTNET_QUALITY_DUPLICATE_PROJECT_ARTIFACT*'
    }

    It 'accepts byte-identical Cobertura attachment copies while retaining one canonical result' {
        $fixture = New-QualityFixture -Name 'duplicate-identical-coverage'
        $attachmentPath = Join-Path $fixture.ProjectRoot 'run/In/machine/coverage.cobertura.xml'
        New-Item -ItemType Directory -Path (Split-Path -Parent $attachmentPath) -Force | Out-Null
        Copy-Item -LiteralPath $fixture.CoberturaPath -Destination $attachmentPath

        & $script:RatchetPath `
            -RepoRoot $fixture.RepoRoot `
            -ResultsRoot $fixture.ResultsRoot `
            -BaselinePath $fixture.BaselinePath `
            -MigrationManifestPath $fixture.MigrationPath `
            -WriteBaseline | Out-Null

        Test-Path -LiteralPath $fixture.BaselinePath | Should -BeTrue
    }
}

Describe 'Test-PcvDotNetQualityRatchet regression gates' {
    It 'fails when skipped tests increase' {
        $fixture = New-QualityFixture -Name 'skip-increase'
        Write-QualityBaseline -Fixture $fixture
        Write-TrxFixture -Path $fixture.TrxPath -Passed @('Alpha') -Skipped @('Beta')

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_SKIP_INCREASE*'
    }

    It 'fails on any line coverage decline beyond 0.0 percentage points' {
        $fixture = New-QualityFixture -Name 'line-regression'
        Write-QualityBaseline -Fixture $fixture
        Write-CoberturaFixture -Path $fixture.CoberturaPath -LineRate 0.7999 -BranchRate 0.7 -LinesValid 10000

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_COVERAGE_REGRESSION*line*'
    }

    It 'fails on any branch coverage decline beyond 0.0 percentage points' {
        $fixture = New-QualityFixture -Name 'branch-regression'
        Write-QualityBaseline -Fixture $fixture
        Write-CoberturaFixture -Path $fixture.CoberturaPath -LineRate 0.8 -BranchRate 0.6999 -BranchesValid 10000

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_COVERAGE_REGRESSION*branch*'
    }

    It 'requires every removed test to have an owned migration to a current replacement' {
        $fixture = New-QualityFixture -Name 'migration-required'
        Write-QualityBaseline -Fixture $fixture
        Write-TrxFixture -Path $fixture.TrxPath -Passed @('Beta', 'Gamma')

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_UNMAPPED_TEST_REMOVAL*Example.Tests.Alpha*'

        [ordered]@{
            schema_version = 'pcv-dotnet-test-migrations/v1'
            migrations = @(
                [ordered]@{
                    old_test_id = 'Example.Tests.Alpha'
                    status = 'completed'
                    replacement_test_id = 'Example.Tests.Gamma'
                    owner = 'runtime-team'
                    coverage_boundary = 'Example runtime contract'
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fixture.MigrationPath -Encoding utf8

        $result = & $script:RatchetPath `
            -RepoRoot $fixture.RepoRoot `
            -ResultsRoot $fixture.ResultsRoot `
            -BaselinePath $fixture.BaselinePath `
            -MigrationManifestPath $fixture.MigrationPath

        $result.ok | Should -BeTrue
        $result.mapped_removed_test_count | Should -Be 1
    }

    It 'does not authorize a removed test with a planned migration' {
        $fixture = New-QualityFixture -Name 'migration-planned-not-approved'
        Write-QualityBaseline -Fixture $fixture
        Write-TrxFixture -Path $fixture.TrxPath -Passed @('Beta', 'Gamma')
        [ordered]@{
            schema_version = 'pcv-dotnet-test-migrations/v1'
            migrations = @(
                [ordered]@{
                    old_test_id = 'Example.Tests.Alpha'
                    status = 'planned'
                    replacement_test_id = 'Example.Tests.Gamma'
                    owner = 'runtime-team'
                    coverage_boundary = 'Example runtime contract'
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fixture.MigrationPath -Encoding utf8

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_UNMAPPED_TEST_REMOVAL*no completed migration entry*'
    }

    It 'allows a one-to-one wildcard namespace move only when each suffix induces one current replacement' {
        $fixture = New-QualityFixture `
            -Name 'migration-wildcard-one-to-one' `
            -Passed @('OldContractOne', 'OldContractTwo')
        Write-QualityBaseline -Fixture $fixture
        Write-TrxFixture -Path $fixture.TrxPath -Passed @('NewContractOne', 'NewContractTwo')
        [ordered]@{
            schema_version = 'pcv-dotnet-test-migrations/v1'
            migrations = @(
                [ordered]@{
                    old_test_id = 'Example.Tests.Old*'
                    status = 'completed'
                    replacement_test_id = 'Example.Tests.New*'
                    owner = 'runtime-team'
                    coverage_boundary = 'Example runtime contract namespace move'
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fixture.MigrationPath -Encoding utf8

        $result = & $script:RatchetPath `
            -RepoRoot $fixture.RepoRoot `
            -ResultsRoot $fixture.ResultsRoot `
            -BaselinePath $fixture.BaselinePath `
            -MigrationManifestPath $fixture.MigrationPath

        $result.ok | Should -BeTrue
        $result.mapped_removed_test_count | Should -Be 2
    }

    It 'rejects any aggregate test count decrease even when the removed test is mapped' {
        $fixture = New-QualityFixture -Name 'count-decrease'
        Write-QualityBaseline -Fixture $fixture
        Write-TrxFixture -Path $fixture.TrxPath -Passed @('Beta')
        [ordered]@{
            schema_version = 'pcv-dotnet-test-migrations/v1'
            migrations = @(
                [ordered]@{
                    old_test_id = 'Example.Tests.Alpha'
                    status = 'completed'
                    replacement_test_id = 'Example.Tests.Beta'
                    owner = 'runtime-team'
                    coverage_boundary = 'Example runtime contract'
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fixture.MigrationPath -Encoding utf8

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_TEST_COUNT_DECREASE*'
    }

    It 'rejects migration entries whose replacement is absent' {
        $fixture = New-QualityFixture -Name 'migration-invalid'
        Write-QualityBaseline -Fixture $fixture
        Write-TrxFixture -Path $fixture.TrxPath -Passed @('Beta', 'Gamma')
        [ordered]@{
            schema_version = 'pcv-dotnet-test-migrations/v1'
            migrations = @(
                [ordered]@{
                    old_test_id = 'Example.Tests.Alpha'
                    status = 'completed'
                    replacement_test_id = 'Example.Tests.Missing'
                    owner = 'runtime-team'
                    coverage_boundary = 'Example runtime contract'
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fixture.MigrationPath -Encoding utf8

        {
            & $script:RatchetPath `
                -RepoRoot $fixture.RepoRoot `
                -ResultsRoot $fixture.ResultsRoot `
                -BaselinePath $fixture.BaselinePath `
                -MigrationManifestPath $fixture.MigrationPath
        } | Should -Throw '*PCV_DOTNET_QUALITY_INVALID_MIGRATION*'
    }
}
