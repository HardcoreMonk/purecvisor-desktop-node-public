[CmdletBinding()]
param(
    [string]$RepoRoot,
    [switch]$Apply,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Refreshes the two-level SHA pins every promotion updated by hand (0.42.78: ab88076, three specs
# and three verifier constants). Level one: each `{ "path", "sha256" }` entry in
# config/pcv-*-contract-spec-v1.json pins a source or legacy file. Level two: the Delivery verifier
# whose `SpecPath` names the spec pins the spec file itself, either as `ExpectedSpecSha256` or as
# the argument after `SpecPath,`. Paths a verifier lists in `StructuredTransitionSources` are
# exempt from its source SHA check and stay as they are. Hashes follow the verifiers: UTF-8 text
# without a BOM, no newline normalization (.gitattributes fixes the line endings).

function Throw-PcvSpecPinsInvalid {
    param(
        [Parameter(Mandatory)][string]$Field,
        [Parameter(Mandatory)][string]$Detail
    )

    throw "PCV_SPEC_PINS_INVALID|$Field|$Detail"
}

function Read-PcvSpecPinFile {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-PcvSpecPinsInvalid -Field 'file' -Detail "missing:$Path"
    }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $offset = if ($bom) { 3 } else { 0 }
    [pscustomobject]@{
        bom = $bom
        text = [System.Text.UTF8Encoding]::new($false, $true).GetString($bytes, $offset, $bytes.Length - $offset)
    }
}

function Get-PcvSpecPinTextSha256 {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        [Convert]::ToHexString($sha.ComputeHash([System.Text.UTF8Encoding]::new($false).GetBytes($Text))).ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Get-PcvSpecPinEntries {
    param([Parameter(Mandatory)][string]$Json)

    $document = [System.Text.Json.JsonDocument]::Parse($Json)
    try {
        $entries = [System.Collections.Generic.List[object]]::new()
        $pending = [System.Collections.Generic.Stack[System.Text.Json.JsonElement]]::new()
        $pending.Push($document.RootElement)
        while ($pending.Count -gt 0) {
            $element = $pending.Pop()
            if ([string]$element.ValueKind -ceq 'Object') {
                $path = [System.Text.Json.JsonElement]::new()
                $sha = [System.Text.Json.JsonElement]::new()
                if ($element.TryGetProperty('path', [ref]$path) -and $element.TryGetProperty('sha256', [ref]$sha) -and
                    [string]$path.ValueKind -ceq 'String' -and [string]$sha.ValueKind -ceq 'String') {
                    $entries.Add([pscustomobject]@{ path = $path.GetString(); sha256 = $sha.GetString() })
                }
                foreach ($property in $element.EnumerateObject()) { $pending.Push($property.Value) }
            }
            elseif ([string]$element.ValueKind -ceq 'Array') {
                foreach ($item in $element.EnumerateArray()) { $pending.Push($item) }
            }
        }
        $entries.ToArray()
    }
    finally {
        $document.Dispose()
    }
}

function Get-PcvStructuredTransitionSources {
    param([Parameter(Mandatory)][string]$Text)

    $match = [regex]::Match($Text, 'StructuredTransitionSources = new\(\s*\[(?<body>[^\]]*)\]')
    if (-not $match.Success) { return @() }
    @([regex]::Matches($match.Groups['body'].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
}

function Find-PcvVerifierSpecSha {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Field
    )

    $found = @(
        [regex]::Matches($Text, '(?m)^\s*(?:private|internal) const string ExpectedSpecSha256 =\r?\n\s*"(?<sha>[0-9a-f]{64})";')
        [regex]::Matches($Text, '(?m)^\s*SpecPath,\r?\n\s*"(?<sha>[0-9a-f]{64})",')
    )
    if ($found.Count -ne 1) {
        Throw-PcvSpecPinsInvalid -Field $Field -Detail "spec-sha-literal-count:$($found.Count)"
    }
    $found[0].Groups['sha']
}

function Get-PcvSpecVerifierMap {
    param([Parameter(Mandatory)][string]$RepoRoot)

    $map = [ordered]@{}
    $sourceRoot = Join-Path $RepoRoot 'src'
    foreach ($file in @(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter '*.cs' | Sort-Object FullName)) {
        $text = [System.IO.File]::ReadAllText($file.FullName)
        foreach ($match in [regex]::Matches($text, 'const string SpecPath =\s*"(?<spec>config/pcv-[a-z0-9-]+-contract-spec-v1\.json)";')) {
            $spec = $match.Groups['spec'].Value
            if ($map.Contains($spec)) {
                Throw-PcvSpecPinsInvalid -Field $spec -Detail 'duplicate-verifier'
            }
            $map[$spec] = [System.IO.Path]::GetRelativePath($RepoRoot, $file.FullName).Replace('\', '/')
        }
    }
    $map
}

function Get-PcvContractSpecPinPlan {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RepoRoot)

    $verifiers = Get-PcvSpecVerifierMap -RepoRoot $RepoRoot
    $specFiles = @(Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'config') -File -Filter 'pcv-*-contract-spec-v1.json' | Sort-Object Name)
    if ($specFiles.Count -eq 0) {
        Throw-PcvSpecPinsInvalid -Field 'config' -Detail 'no-spec'
    }
    @(foreach ($specFile in $specFiles) {
        $specPath = "config/$($specFile.Name)"
        if (-not $verifiers.Contains($specPath)) {
            Throw-PcvSpecPinsInvalid -Field $specPath -Detail 'missing-verifier'
        }
        $verifierPath = $verifiers[$specPath]
        $verifier = Read-PcvSpecPinFile -Path (Join-Path $RepoRoot $verifierPath)
        $exempt = @(Get-PcvStructuredTransitionSources -Text $verifier.text)
        $spec = Read-PcvSpecPinFile -Path $specFile.FullName
        $specText = $spec.text
        $pins = [System.Collections.Generic.List[object]]::new()
        $exemptStale = [System.Collections.Generic.List[string]]::new()
        $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        foreach ($entry in @(Get-PcvSpecPinEntries -Json $specText)) {
            if (-not $seen.Add("$($entry.path)|$($entry.sha256)")) { continue }
            $actual = Get-PcvSpecPinTextSha256 -Text (Read-PcvSpecPinFile -Path (Join-Path $RepoRoot $entry.path)).text
            if ($actual -ceq $entry.sha256) { continue }
            if ($entry.path -cin $exempt) {
                $exemptStale.Add($entry.path)
                continue
            }
            $pattern = '("path":\s*"' + [regex]::Escape($entry.path) + '",\s*"sha256":\s*")' + $entry.sha256 + '"'
            $count = [regex]::Matches($specText, $pattern).Count
            if ($count -eq 0) {
                Throw-PcvSpecPinsInvalid -Field "$specPath.$($entry.path)" -Detail 'unsupported-pin-format'
            }
            $specText = [regex]::Replace($specText, $pattern, { param($m) $m.Groups[1].Value + $actual + '"' })
            $pins.Add([pscustomobject]([ordered]@{ path = $entry.path; old = $entry.sha256; new = $actual }))
        }
        $specSha = Get-PcvSpecPinTextSha256 -Text $specText
        $literal = Find-PcvVerifierSpecSha -Text $verifier.text -Field $verifierPath
        $verifierText = $verifier.text.Substring(0, $literal.Index) + $specSha + $verifier.text.Substring($literal.Index + $literal.Length)
        [pscustomobject]([ordered]@{
            spec = $specPath
            verifier = $verifierPath
            pins = @($pins.ToArray())
            exempt_stale = @($exemptStale.ToArray())
            spec_sha_old = $literal.Value
            spec_sha_new = $specSha
            spec_changed = $specText -cne $spec.text
            verifier_changed = $literal.Value -cne $specSha
            spec_bom = $spec.bom
            spec_text = $specText
            verifier_bom = $verifier.bom
            verifier_text = $verifierText
        })
    })
}

function Write-PcvSpecPinFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][bool]$Bom
    )

    $temporaryPath = "$Path.tmp"
    try {
        [System.IO.File]::WriteAllText($temporaryPath, $Text, [System.Text.UTF8Encoding]::new($Bom))
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
            throw 'PCV_SPEC_PINS_USAGE|apply-and-check-are-exclusive'
        }
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $mode = if ($Check) { 'check' } elseif ($Apply) { 'apply' } else { 'dry-run' }
        $plan = @(Get-PcvContractSpecPinPlan -RepoRoot $resolvedRepoRoot)
        $changed = @($plan | Where-Object { $_.spec_changed -or $_.verifier_changed })

        if ($Check -and $changed.Count -ne 0) {
            throw "PCV_SPEC_PINS_STALE|$(@($changed.spec) -join ',')"
        }
        if ($Apply) {
            foreach ($item in $changed) {
                if ($item.spec_changed) {
                    Write-PcvSpecPinFile -Path (Join-Path $resolvedRepoRoot $item.spec) -Text $item.spec_text -Bom $item.spec_bom
                }
                Write-PcvSpecPinFile -Path (Join-Path $resolvedRepoRoot $item.verifier) -Text $item.verifier_text -Bom $item.verifier_bom
            }
        }
        [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $true
            mode = $mode
            status = if ($changed.Count -eq 0) { 'current' } elseif ($Apply) { 'updated' } else { 'planned' }
            spec_count = $plan.Count
            changed_spec_count = $changed.Count
            changes = @($changed | ForEach-Object {
                [pscustomobject]([ordered]@{
                    spec = $_.spec
                    verifier = $_.verifier
                    pins = $_.pins
                    spec_sha_old = $_.spec_sha_old
                    spec_sha_new = $_.spec_sha_new
                })
            })
            exempt_stale = @($plan | Where-Object { $_.exempt_stale.Count -gt 0 } | ForEach-Object {
                [pscustomobject]@{ spec = $_.spec; paths = $_.exempt_stale }
            })
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
