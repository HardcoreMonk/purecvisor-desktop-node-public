Set-StrictMode -Version Latest

function Test-PcvFeaturePromotionEligibility {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$LedgerPath,

        [Parameter(Mandatory)]
        [string]$ObservationPath
    )

    $ledger = Get-Content -LiteralPath $LedgerPath -Raw | ConvertFrom-Json -Depth 64
    $observation = Get-Content -LiteralPath $ObservationPath -Raw | ConvertFrom-Json -Depth 64
    $blockers = [System.Collections.Generic.List[object]]::new()

    foreach ($feature in @($ledger.features | Where-Object candidate_required)) {
        $actual = @($observation.features | Where-Object feature_id -EQ $feature.feature_id)

        foreach ($stage in @($feature.required_stages)) {
            $stageVerdict = 'missing'
            $stageObservations = @()

            if ($actual.Count -eq 1) {
                $stageObservations = @($actual[0].stages | Where-Object name -EQ $stage)
                if ($stageObservations.Count -eq 1 -and $null -ne $stageObservations[0].verdict) {
                    $stageVerdict = [string]$stageObservations[0].verdict
                }
            }

            if ($actual.Count -ne 1 -or $stageObservations.Count -ne 1 -or $stageVerdict -ne 'pass') {
                $blockers.Add([pscustomobject][ordered]@{
                        feature_id = $feature.feature_id
                        stage = $stage
                        verdict = $stageVerdict
                    })
            }
        }
    }

    [pscustomobject][ordered]@{
        schema_version = 1
        contract = 'pcv-feature-promotion-decision-v1'
        promotion_eligible = $blockers.Count -eq 0
        blockers = @($blockers)
    }
}

Export-ModuleMember -Function Test-PcvFeaturePromotionEligibility
