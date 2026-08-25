Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-PcvDevelopmentChangeTier {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('S', 'M', 'L')]
        [string]$RequestedTier,

        [Parameter(Mandatory)]
        [string[]]$ChangedPath
    )

    $tierRank = @{ S = 1; M = 2; L = 3 }
    $state = [ordered]@{ effective_tier = $RequestedTier }
    $reasons = [System.Collections.Generic.List[string]]::new()
    $reasonSet = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    $domains = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)

    function Add-PcvTierReason {
        param(
            [Parameter(Mandatory)][string]$Reason,
            [Parameter(Mandatory)][ValidateSet('M', 'L')][string]$MinimumTier
        )
        if ($reasonSet.Add($Reason)) {
            $reasons.Add($Reason)
        }
        if ($tierRank[$MinimumTier] -gt $tierRank[$state.effective_tier]) {
            $state.effective_tier = $MinimumTier
        }
    }

    foreach ($path in $ChangedPath) {
        $normalized = $path.Replace('\', '/')
        if ($normalized -match '^src/([^/]+)(/|$)') {
            [void]$domains.Add("src:$($Matches[1])")
        }
        elseif ($normalized -match '^web/') {
            [void]$domains.Add('web')
        }
        elseif ($normalized -match '^packaging/windows-desktop-node/') {
            [void]$domains.Add('packaging')
        }

        if ($normalized -match '^packaging/windows-desktop-node/installer/') {
            Add-PcvTierReason -Reason 'installer-lifecycle' -MinimumTier L
        }
        if ($normalized -match '^packaging/windows-desktop-node/(tools|tests)/.*(HostMutation|OsMutation|FullAdminHostMutation)') {
            Add-PcvTierReason -Reason 'host-mutation-boundary' -MinimumTier L
        }
        if ($normalized -match '^docs/(adr/(0003|0009|0010)-|.*(security|credential|token|tls|trust).*policy)') {
            Add-PcvTierReason -Reason 'security-policy-boundary' -MinimumTier L
        }
        if ($normalized -match '^(AGENTS\.md|docs/ga-ready/(current-evidence(\.schema)?\.json|EVIDENCE_INDEX\.md|CURRENT_EVIDENCE_LEDGER\.md|CONTROL_PLANE_INDEX\.md)|docs/DEVELOPMENT_VERIFICATION_POLICY\.md|packaging/windows-desktop-node/README\.md)$') {
            Add-PcvTierReason -Reason 'current-evidence-anchor' -MinimumTier L
        }
        if ($normalized -match '^(docs/PUBLIC_RELEASE_BOUNDARY\.md|docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX\.md|docs/adr/0005-|\.github/workflows/.*(release|publish))') {
            Add-PcvTierReason -Reason 'public-release-boundary' -MinimumTier L
        }
        if ($normalized -match '(sign(ed|ing)?|publication|publish)') {
            Add-PcvTierReason -Reason 'signing-publication-boundary' -MinimumTier L
        }
        if ($normalized -match '^src/DesktopNode\.(Api|Cli)(/|$)' -or
            $normalized -match '^web/(src|tests)/.*(api|contract|client|auth)') {
            Add-PcvTierReason -Reason 'api-cli-web-contract' -MinimumTier M
        }
        if ($normalized -match '^packaging/windows-desktop-node/') {
            Add-PcvTierReason -Reason 'packaging-contract' -MinimumTier M
        }
    }

    if ($domains.Count -gt 1) {
        Add-PcvTierReason -Reason 'cross-module-change' -MinimumTier M
    }

    [pscustomobject]([ordered]@{
        requested_tier = $RequestedTier
        effective_tier = $state.effective_tier
        reasons = @($reasons.ToArray())
    })
}

function Resolve-PcvDevelopmentVerificationSelection {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Fast', 'Full', 'Release')]
        [string]$Lane,

        [Parameter(Mandatory)]
        [ValidateSet('S', 'M', 'L')]
        [string]$ChangeTier,

        [Parameter(Mandatory)]
        [string[]]$ChangedPath
    )

    $allSuites = @(
        'dotnet',
        'web-npm',
        'packaging-pester',
        'installer-pester',
        'web-pester',
        'git-diff-check',
        'current-evidence-check'
    )

    $tierResolution = Resolve-PcvDevelopmentChangeTier `
        -RequestedTier $ChangeTier `
        -ChangedPath $ChangedPath
    $effectiveChangeTier = $tierResolution.effective_tier

    $effectiveLane = $Lane
    $promotionReason = ''
    if ($effectiveChangeTier -eq 'L' -and $Lane -ne 'Release') {
        $effectiveLane = 'Release'
        $promotionReason = 'tier-l-requires-release'
    }
    elseif ($effectiveChangeTier -eq 'M' -and $Lane -eq 'Fast') {
        $effectiveLane = 'Full'
        $promotionReason = 'tier-m-requires-full'
    }

    if ($effectiveLane -ne 'Fast') {
        return [pscustomobject]([ordered]@{
            requested_lane = $Lane
            effective_lane = $effectiveLane
            requested_change_tier = $ChangeTier
            change_tier = $effectiveChangeTier
            tier_reasons = @($tierResolution.reasons)
            promotion_reason = $promotionReason
            suites = $allSuites
        })
    }

    $selected = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    $unknownPath = $false

    foreach ($path in $ChangedPath) {
        $normalized = $path.Replace('\', '/')
        switch -Regex ($normalized) {
            '^(AGENTS\.md|docs/ga-ready/(current-evidence(\.schema)?\.json|EVIDENCE_INDEX\.md|CURRENT_EVIDENCE_LEDGER\.md|CONTROL_PLANE_INDEX\.md)|docs/DEVELOPMENT_VERIFICATION_POLICY\.md|packaging/windows-desktop-node/(README\.md|tools/Update-PcvCurrentEvidenceDocs\.ps1|tests/PcvCurrentEvidenceGeneration\.Tests\.ps1))$' {
                [void]$selected.Add('current-evidence-check')
                continue
            }
            '^(src/|.*\.sln$|.*\.csproj$)' {
                [void]$selected.Add('dotnet')
                continue
            }
            '^web/' {
                [void]$selected.Add('web-npm')
                [void]$selected.Add('web-pester')
                continue
            }
            '^packaging/windows-desktop-node/installer/' {
                [void]$selected.Add('installer-pester')
                continue
            }
            '^packaging/windows-desktop-node/(tools/PcvBatchSupervisor\.psm1|tests/PcvBatchSupervisor\.Tests\.ps1)$' {
                [void]$selected.Add('packaging-pester')
                continue
            }
            '^docs/' {
                [void]$selected.Add('git-diff-check')
                continue
            }
            default {
                $unknownPath = $true
            }
        }
    }

    if ($unknownPath -or $selected.Count -eq 0) {
        return [pscustomobject]([ordered]@{
            requested_lane = $Lane
            effective_lane = 'Full'
            requested_change_tier = $ChangeTier
            change_tier = $effectiveChangeTier
            tier_reasons = @($tierResolution.reasons)
            promotion_reason = 'unknown-change-scope'
            suites = $allSuites
        })
    }

    [pscustomobject]([ordered]@{
        requested_lane = $Lane
        effective_lane = 'Fast'
        requested_change_tier = $ChangeTier
        change_tier = $effectiveChangeTier
        tier_reasons = @($tierResolution.reasons)
        promotion_reason = ''
        suites = @($selected | Sort-Object)
    })
}

Export-ModuleMember -Function `
    Resolve-PcvDevelopmentChangeTier, `
    Resolve-PcvDevelopmentVerificationSelection
