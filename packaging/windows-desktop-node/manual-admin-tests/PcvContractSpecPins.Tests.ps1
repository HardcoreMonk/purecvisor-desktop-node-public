Set-StrictMode -Version Latest

# Every promotion refreshed the contract spec pins by hand (0.42.78: ab88076, three specs and three
# verifier constants). This suite pins what the tool does instead: stale `{ path, sha256 }` entries
# take the file's UTF-8 text hash, StructuredTransitionSources stay exempt, and the verifier that
# names the spec takes the new spec hash in either constant form.

Describe 'contract spec pins' {
    BeforeAll {
        $script:ToolPath = Join-Path (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path 'packaging/windows-desktop-node/tools/Update-PcvContractSpecPins.ps1'
        . $script:ToolPath

        function Write-PcvTestFile {
            param(
                [Parameter(Mandatory)][string]$Root,
                [Parameter(Mandatory)][string]$Path,
                [Parameter(Mandatory)][AllowEmptyString()][string]$Text,
                [switch]$Bom
            )

            $fullPath = Join-Path $Root $Path
            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullPath) | Out-Null
            [System.IO.File]::WriteAllText($fullPath, $Text, [System.Text.UTF8Encoding]::new([bool]$Bom))
        }

        function New-PcvTestPinRepository {
            $root = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-spec-pins-$([guid]::NewGuid().ToString('N'))"
            Write-PcvTestFile -Root $root -Path 'docs/a.md' -Text "a current`n"
            Write-PcvTestFile -Root $root -Path 'docs/exempt.md' -Text "exempt current`n"
            Write-PcvTestFile -Root $root -Path 'docs/bom.md' -Text "bom text`n" -Bom
            Write-PcvTestFile -Root $root -Path 'config/pcv-alpha-contract-spec-v1.json' -Text (@(
                '{'
                '  "contract": "pcv-alpha-contract-spec-v1",'
                '  "source_files": ['
                "    { `"path`": `"docs/a.md`", `"sha256`": `"$('1' * 64)`" },"
                "    { `"path`": `"docs/exempt.md`", `"sha256`": `"$('2' * 64)`" }"
                '  ],'
                '  "legacy_files": ['
                "    {`n      `"key`": `"bom`",`n      `"path`": `"docs/bom.md`",`n      `"sha256`": `"$('3' * 64)`",`n      `"contract_count`": 1`n    }"
                '  ]'
                '}'
                ''
            ) -join "`n")
            Write-PcvTestFile -Root $root -Path 'config/pcv-beta-contract-spec-v1.json' -Text "{ `"source_files`": [ { `"path`": `"docs/a.md`", `"sha256`": `"$(Get-PcvSpecPinTextSha256 -Text "a current`n")`" } ] }`n"
            Write-PcvTestFile -Root $root -Path 'src/Alpha/AlphaVerifier.cs' -Text (@(
                'internal sealed class AlphaVerifier'
                '{'
                '    internal const string SpecPath = "config/pcv-alpha-contract-spec-v1.json";'
                ''
                '    private const string ExpectedSpecSha256 ='
                "        `"$('4' * 64)`";"
                ''
                '    private static readonly HashSet<string> StructuredTransitionSources = new('
                '    ['
                '        "docs/exempt.md",'
                '    ],'
                '    StringComparer.Ordinal);'
                '}'
                ''
            ) -join "`n")
            Write-PcvTestFile -Root $root -Path 'src/Beta/BetaVerifier.cs' -Text (@(
                'internal static class BetaVerifier'
                '{'
                '    internal const string SpecPath ='
                '        "config/pcv-beta-contract-spec-v1.json";'
                ''
                '    private static readonly LegacyBatchContractVerifier Core ='
                '        new('
                '            SpecPath,'
                "            `"$('5' * 64)`","
                '            "PCV_BETA_INVALID");'
                '}'
                ''
            ) -join "`n")
            $root
        }
    }

    It 'refreshes stale pins, keeps exempt pins, and hashes text without a BOM' {
        $root = New-PcvTestPinRepository
        try {
            $alpha = @(Get-PcvContractSpecPinPlan -RepoRoot $root) | Where-Object { $_.spec -ceq 'config/pcv-alpha-contract-spec-v1.json' }

            @($alpha.pins.path) | Should -Be @('docs/bom.md', 'docs/a.md')
            ($alpha.pins | Where-Object { $_.path -ceq 'docs/bom.md' }).new | Should -BeExactly (Get-PcvSpecPinTextSha256 -Text "bom text`n")
            @($alpha.exempt_stale) | Should -Be @('docs/exempt.md')
            $alpha.spec_text | Should -BeLike "*`"sha256`": `"$('2' * 64)`"*"
            $alpha.spec_text | Should -Not -BeLike "*$('1' * 64)*"
            $alpha.verifier | Should -BeExactly 'src/Alpha/AlphaVerifier.cs'
            $alpha.spec_sha_new | Should -BeExactly (Get-PcvSpecPinTextSha256 -Text $alpha.spec_text)
            $alpha.verifier_text | Should -BeLike "*ExpectedSpecSha256 =`n        `"$($alpha.spec_sha_new)`";*"
        }
        finally {
            Remove-Item -LiteralPath $root -Recurse -Force
        }
    }

    It 'pins the spec hash in the argument after SpecPath when the spec itself is current' {
        $root = New-PcvTestPinRepository
        try {
            $beta = @(Get-PcvContractSpecPinPlan -RepoRoot $root) | Where-Object { $_.spec -ceq 'config/pcv-beta-contract-spec-v1.json' }

            @($beta.pins).Count | Should -Be 0
            $beta.spec_changed | Should -BeFalse
            $beta.verifier_changed | Should -BeTrue
            $beta.spec_sha_old | Should -BeExactly ('5' * 64)
            $beta.verifier_text | Should -BeLike "*SpecPath,`n            `"$($beta.spec_sha_new)`",*"
        }
        finally {
            Remove-Item -LiteralPath $root -Recurse -Force
        }
    }

    It 'refuses a spec without a verifier, an ambiguous spec literal, and a missing pinned file' {
        $root = New-PcvTestPinRepository
        try {
            Write-PcvTestFile -Root $root -Path 'config/pcv-gamma-contract-spec-v1.json' -Text "{}`n"
            { Get-PcvContractSpecPinPlan -RepoRoot $root } | Should -Throw '*config/pcv-gamma-contract-spec-v1.json|missing-verifier*'
            Remove-Item -LiteralPath (Join-Path $root 'config/pcv-gamma-contract-spec-v1.json')

            $betaPath = Join-Path $root 'src/Beta/BetaVerifier.cs'
            $beta = [System.IO.File]::ReadAllText($betaPath)
            [System.IO.File]::WriteAllText($betaPath, $beta.Replace('    internal const string SpecPath =', "    private const string ExpectedSpecSha256 =`n        `"$('6' * 64)`";`n    internal const string SpecPath ="))
            { Get-PcvContractSpecPinPlan -RepoRoot $root } | Should -Throw '*src/Beta/BetaVerifier.cs|spec-sha-literal-count:2*'
            [System.IO.File]::WriteAllText($betaPath, $beta)

            Remove-Item -LiteralPath (Join-Path $root 'docs/bom.md')
            { Get-PcvContractSpecPinPlan -RepoRoot $root } | Should -Throw '*PCV_SPEC_PINS_INVALID|file|missing:*bom.md*'
        }
        finally {
            Remove-Item -LiteralPath $root -Recurse -Force
        }
    }

    It 'plans without writing, applies, and then checks current from the command line' {
        $root = New-PcvTestPinRepository
        try {
            $specPath = Join-Path $root 'config/pcv-alpha-contract-spec-v1.json'
            $before = [System.IO.File]::ReadAllText($specPath)

            $stale = & pwsh -NoProfile -File $script:ToolPath -RepoRoot $root -Check | ConvertFrom-Json
            $LASTEXITCODE | Should -Be 1
            $stale.error | Should -BeExactly 'PCV_SPEC_PINS_STALE|config/pcv-alpha-contract-spec-v1.json,config/pcv-beta-contract-spec-v1.json'

            $plan = & pwsh -NoProfile -File $script:ToolPath -RepoRoot $root | ConvertFrom-Json
            $plan.status | Should -Be 'planned'
            [System.IO.File]::ReadAllText($specPath) | Should -BeExactly $before

            $applied = & pwsh -NoProfile -File $script:ToolPath -RepoRoot $root -Apply | ConvertFrom-Json
            $applied.status | Should -Be 'updated'
            $applied.changed_spec_count | Should -Be 2

            $current = & pwsh -NoProfile -File $script:ToolPath -RepoRoot $root -Check | ConvertFrom-Json
            $LASTEXITCODE | Should -Be 0
            $current.status | Should -Be 'current'
            @($current.exempt_stale.paths) | Should -Be @('docs/exempt.md')
        }
        finally {
            Remove-Item -LiteralPath $root -Recurse -Force
        }
    }
}
