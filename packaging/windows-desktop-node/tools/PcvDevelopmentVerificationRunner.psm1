Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$selectorModulePath = Join-Path $PSScriptRoot 'PcvDevelopmentVerification.psm1'
Import-Module $selectorModulePath -Force

function Get-PcvDevelopmentVerificationSuiteCatalog {
    [CmdletBinding()]
    param()

    [ordered]@{
        dotnet = [ordered]@{
            file_name = 'dotnet'
            arguments = @('test', 'src/DesktopNode.sln', '-c', 'Release')
        }
        'web-npm' = [ordered]@{
            file_name = 'pwsh'
            arguments = @(
                '-NoProfile',
                '-Command',
                'npm test --prefix web; if($LASTEXITCODE -eq 0){npm run verify:parity --prefix web}; exit $LASTEXITCODE'
            )
        }
        'packaging-pester' = [ordered]@{
            file_name = 'pwsh'
            arguments = @(
                '-NoProfile',
                '-Command',
                '$r=Invoke-Pester -Path ''packaging/windows-desktop-node/tests'' -PassThru -Output Detailed; if($r.FailedCount -gt 0){exit 1}'
            )
        }
        'installer-pester' = [ordered]@{
            file_name = 'pwsh'
            arguments = @(
                '-NoProfile',
                '-Command',
                '$r=Invoke-Pester -Path ''packaging/windows-desktop-node/installer/tests'' -PassThru -Output Detailed; if($r.FailedCount -gt 0){exit 1}'
            )
        }
        'web-pester' = [ordered]@{
            file_name = 'pwsh'
            arguments = @(
                '-NoProfile',
                '-Command',
                '$r=Invoke-Pester -Path ''web/tests'' -PassThru -Output Detailed; if($r.FailedCount -gt 0){exit 1}'
            )
        }
        'git-diff-check' = [ordered]@{
            file_name = 'git'
            arguments = @('diff', '--check')
        }
        'current-evidence-check' = [ordered]@{
            file_name = 'pwsh'
            arguments = @(
                '-NoProfile',
                '-File',
                'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1',
                '-Check'
            )
        }
    }
}

function ConvertTo-PcvDevelopmentVerificationOutput {
    param(
        [AllowNull()][string]$Text,
        [Parameter(Mandatory)][string]$WorkingDirectory
    )

    if ($null -eq $Text) {
        return ''
    }

    $bounded = $Text.Replace($WorkingDirectory, '[REPO_ROOT]')
    if ($bounded.Length -gt 8192) {
        return $bounded.Substring(0, 8192) + '...[truncated]'
    }
    $bounded
}

function Invoke-PcvDevelopmentVerificationCommand {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Suite,
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        [void]$process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        [pscustomobject]([ordered]@{
            exit_code = $process.ExitCode
            duration_ms = [int]$stopwatch.ElapsedMilliseconds
            stdout = ConvertTo-PcvDevelopmentVerificationOutput `
                -Text $stdout `
                -WorkingDirectory $WorkingDirectory
            stderr = ConvertTo-PcvDevelopmentVerificationOutput `
                -Text $stderr `
                -WorkingDirectory $WorkingDirectory
        })
    }
    finally {
        $stopwatch.Stop()
        $process.Dispose()
    }
}

function Invoke-PcvDevelopmentVerification {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Fast', 'Full', 'Release')]
        [string]$Lane,

        [Parameter(Mandatory)]
        [ValidateSet('S', 'M', 'L')]
        [string]$ChangeTier,

        [Parameter(Mandatory)]
        [string[]]$ChangedPath,

        [Parameter(Mandatory)]
        [string]$WorkingDirectory,

        [switch]$PlanOnly,

        [scriptblock]$CommandRunner = ${function:Invoke-PcvDevelopmentVerificationCommand}
    )

    $selection = Resolve-PcvDevelopmentVerificationSelection `
        -Lane $Lane `
        -ChangeTier $ChangeTier `
        -ChangedPath $ChangedPath
    $catalog = Get-PcvDevelopmentVerificationSuiteCatalog
    $selectedSuites = @($selection.suites)
    $results = [System.Collections.Generic.List[object]]::new()
    $failedSuite = ''

    foreach ($suite in $catalog.Keys) {
        $definition = $catalog[$suite]
        if ($suite -notin $selectedSuites) {
            $results.Add([pscustomobject]([ordered]@{
                suite = $suite
                status = 'not-selected-by-scope'
                exit_code = $null
                duration_ms = 0
            }))
            continue
        }

        if (-not [string]::IsNullOrWhiteSpace($failedSuite)) {
            $results.Add([pscustomobject]([ordered]@{
                suite = $suite
                status = 'not-run-after-failure'
                exit_code = $null
                duration_ms = 0
            }))
            continue
        }

        if ($PlanOnly) {
            $results.Add([pscustomobject]([ordered]@{
                suite = $suite
                status = 'planned'
                file_name = $definition.file_name
                arguments = @($definition.arguments)
                exit_code = $null
                duration_ms = 0
            }))
            continue
        }

        $run = & $CommandRunner `
            -Suite $suite `
            -FileName $definition.file_name `
            -Arguments @($definition.arguments) `
            -WorkingDirectory $WorkingDirectory
        $status = if ([int]$run.exit_code -eq 0) { 'passed' } else { 'failed' }
        $results.Add([pscustomobject]([ordered]@{
            suite = $suite
            status = $status
            exit_code = [int]$run.exit_code
            duration_ms = [int]$run.duration_ms
            stdout = [string]$run.stdout
            stderr = [string]$run.stderr
        }))
        if ($status -eq 'failed') {
            $failedSuite = $suite
        }
    }

    [pscustomobject]([ordered]@{
        schema_version = 1
        ok = [string]::IsNullOrWhiteSpace($failedSuite)
        requested_lane = $selection.requested_lane
        effective_lane = $selection.effective_lane
        requested_change_tier = $selection.requested_change_tier
        change_tier = $selection.change_tier
        tier_reasons = @($selection.tier_reasons)
        promotion_reason = $selection.promotion_reason
        failed_suite = $failedSuite
        results = @($results.ToArray())
    })
}

Export-ModuleMember -Function `
    Get-PcvDevelopmentVerificationSuiteCatalog, `
    Invoke-PcvDevelopmentVerification
