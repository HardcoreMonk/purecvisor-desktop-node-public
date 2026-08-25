Set-StrictMode -Version Latest

function Get-PcvEvidenceScope {
    param(
        [Parameter(Mandatory)]
        [object]$Evidence
    )

    if ($Evidence.PSObject.Properties.Name -contains 'lifecycle' -and $null -ne $Evidence.lifecycle) {
        return $Evidence.lifecycle
    }

    $Evidence
}

function Test-PcvEvidenceField {
    param(
        [Parameter(Mandatory)]
        [object]$Value,

        [Parameter(Mandatory)]
        [string]$Name
    )

    if ($null -eq $Value) {
        return $false
    }

    if ($Value.PSObject.Properties.Name -notcontains $Name) {
        return $false
    }

    $null -ne $Value.$Name
}

function Get-PcvEvidenceFieldValue {
    param(
        [Parameter(Mandatory)]
        [object]$Value,

        [Parameter(Mandatory)]
        [string]$Name
    )

    if ($null -eq $Value -or $Value.PSObject.Properties.Name -notcontains $Name) {
        return $null
    }

    $Value.$Name
}

function Test-PcvEvidenceLeafValue {
    param(
        [AllowNull()]
        [object]$Value
    )

    if ($null -eq $Value) {
        return $true
    }

    $leafTypes = @(
        [string],
        [bool],
        [byte],
        [sbyte],
        [int16],
        [uint16],
        [int],
        [uint32],
        [long],
        [uint64],
        [single],
        [double],
        [decimal],
        [datetime],
        [guid]
    )

    foreach ($leafType in $leafTypes) {
        if ($Value -is $leafType) {
            return $true
        }
    }

    $false
}

function Test-PcvEvidenceContainsNamedValue {
    param(
        [AllowNull()]
        [object]$Value,

        [Parameter(Mandatory)]
        [string]$ExpectedName
    )

    if (Test-PcvEvidenceLeafValue -Value $Value) {
        return $false
    }

    if ($Value -is [System.Collections.IDictionary]) {
        foreach ($key in $Value.Keys) {
            $keyName = [string]$key
            $itemValue = $Value[$key]
            if ($keyName -in @('name', 'Name') -and [string]$itemValue -eq $ExpectedName) {
                return $true
            }

            if (Test-PcvEvidenceContainsNamedValue -Value $itemValue -ExpectedName $ExpectedName) {
                return $true
            }
        }

        return $false
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        foreach ($item in $Value) {
            if (Test-PcvEvidenceContainsNamedValue -Value $item -ExpectedName $ExpectedName) {
                return $true
            }
        }

        return $false
    }

    foreach ($property in $Value.PSObject.Properties) {
        if ($property.Name -in @('name', 'Name') -and [string]$property.Value -eq $ExpectedName) {
            return $true
        }

        if (Test-PcvEvidenceContainsNamedValue -Value $property.Value -ExpectedName $ExpectedName) {
            return $true
        }
    }

    $false
}

function Test-PcvEvidenceContainsPropertyValue {
    param(
        [AllowNull()]
        [object]$Value,

        [Parameter(Mandatory)]
        [string]$PropertyName,

        [Parameter(Mandatory)]
        [string]$ExpectedValue
    )

    if (Test-PcvEvidenceLeafValue -Value $Value) {
        return $false
    }

    if ($Value -is [System.Collections.IDictionary]) {
        foreach ($key in $Value.Keys) {
            $keyName = [string]$key
            $itemValue = $Value[$key]
            if ($keyName -eq $PropertyName -and [string]$itemValue -eq $ExpectedValue) {
                return $true
            }

            if (Test-PcvEvidenceContainsPropertyValue -Value $itemValue -PropertyName $PropertyName -ExpectedValue $ExpectedValue) {
                return $true
            }
        }

        return $false
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        foreach ($item in $Value) {
            if (Test-PcvEvidenceContainsPropertyValue -Value $item -PropertyName $PropertyName -ExpectedValue $ExpectedValue) {
                return $true
            }
        }

        return $false
    }

    foreach ($property in $Value.PSObject.Properties) {
        if ($property.Name -eq $PropertyName -and [string]$property.Value -eq $ExpectedValue) {
            return $true
        }

        if (Test-PcvEvidenceContainsPropertyValue -Value $property.Value -PropertyName $PropertyName -ExpectedValue $ExpectedValue) {
            return $true
        }
    }

    $false
}

function Test-PcvEvidenceContainsRetryableTrue {
    param(
        [AllowNull()]
        [object]$Value
    )

    if ($null -eq $Value -or $Value -is [string]) {
        return $false
    }

    if ($Value -is [System.Collections.IDictionary]) {
        foreach ($key in $Value.Keys) {
            $itemValue = $Value[$key]
            if ([string]$key -eq 'retryable' -and $itemValue -eq $true) {
                return $true
            }

            if (Test-PcvEvidenceContainsRetryableTrue -Value $itemValue) {
                return $true
            }
        }

        return $false
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        foreach ($item in $Value) {
            if (Test-PcvEvidenceContainsRetryableTrue -Value $item) {
                return $true
            }
        }

        return $false
    }

    foreach ($property in $Value.PSObject.Properties) {
        if ($property.Name -eq 'retryable' -and $property.Value -eq $true) {
            return $true
        }

        if (Test-PcvEvidenceContainsRetryableTrue -Value $property.Value) {
            return $true
        }
    }

    $false
}

function New-PcvPhase21CheckpointAssessment {
    param(
        [Parameter(Mandatory)]
        [bool]$Ok,

        [Parameter(Mandatory)]
        [string]$Status,

        [AllowNull()]
        [string]$RootCause,

        [string[]]$MissingEvidence = @(),

        [bool]$JobResultContainsName = $false,

        [bool]$ApiListContainsName = $false,

        [bool]$DirectSnapshotContainsName = $false,

        [bool]$Retryable = $false
    )

    [pscustomobject][ordered]@{
        ok = $Ok
        status = $Status
        root_cause = $RootCause
        missing_evidence = @($MissingEvidence)
        job_result_contains_name = $JobResultContainsName
        api_list_contains_name = $ApiListContainsName
        direct_snapshot_contains_name = $DirectSnapshotContainsName
        retryable = $Retryable
    }
}

function Get-PcvPhase21CheckpointEvidenceAssessment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$Evidence,

        [Parameter(Mandatory)]
        [string]$CheckpointName
    )

    $scope = Get-PcvEvidenceScope -Evidence $Evidence
    $rawFields = @('checkpoint_job_result', 'checkpoint_list_response', 'direct_snapshots')
    $missing = @($rawFields | Where-Object { -not (Test-PcvEvidenceField -Value $scope -Name $_) })

    if ($missing.Count -gt 0) {
        return New-PcvPhase21CheckpointAssessment `
            -Ok $false `
            -Status 'inconclusive_missing_raw_evidence' `
            -RootCause 'evidence_capture_incomplete' `
            -MissingEvidence $missing
    }

    $jobResult = Get-PcvEvidenceFieldValue -Value $scope -Name 'checkpoint_job_result'
    $listResponse = Get-PcvEvidenceFieldValue -Value $scope -Name 'checkpoint_list_response'
    $directSnapshots = Get-PcvEvidenceFieldValue -Value $scope -Name 'direct_snapshots'

    $jobResultContainsName = Test-PcvEvidenceContainsNamedValue -Value $jobResult -ExpectedName $CheckpointName
    $apiListContainsName = Test-PcvEvidenceContainsNamedValue -Value $listResponse -ExpectedName $CheckpointName
    $directSnapshotContainsName = Test-PcvEvidenceContainsNamedValue -Value $directSnapshots -ExpectedName $CheckpointName
    $checkpointNotVisible = Test-PcvEvidenceContainsPropertyValue -Value $jobResult -PropertyName 'code' -ExpectedValue 'PCV_CHECKPOINT_NOT_VISIBLE'

    if ($checkpointNotVisible) {
        return New-PcvPhase21CheckpointAssessment `
            -Ok $false `
            -Status 'create_visibility_failure' `
            -RootCause 'hyperv_checkpoint_not_visible_after_create' `
            -JobResultContainsName $jobResultContainsName `
            -ApiListContainsName $apiListContainsName `
            -DirectSnapshotContainsName $directSnapshotContainsName `
            -Retryable (Test-PcvEvidenceContainsRetryableTrue -Value $jobResult)
    }

    if ($jobResultContainsName -and $apiListContainsName -and $directSnapshotContainsName) {
        return New-PcvPhase21CheckpointAssessment `
            -Ok $true `
            -Status 'verified_visible' `
            -RootCause $null `
            -JobResultContainsName $true `
            -ApiListContainsName $true `
            -DirectSnapshotContainsName $true
    }

    if ($directSnapshotContainsName -and -not $apiListContainsName) {
        return New-PcvPhase21CheckpointAssessment `
            -Ok $false `
            -Status 'api_checkpoint_list_mismatch' `
            -RootCause 'api_or_evidence_list_capture_mismatch' `
            -JobResultContainsName $jobResultContainsName `
            -ApiListContainsName $false `
            -DirectSnapshotContainsName $true `
            -Retryable $true
    }

    New-PcvPhase21CheckpointAssessment `
        -Ok $false `
        -Status 'checkpoint_visibility_unverified' `
        -RootCause 'checkpoint_name_missing_from_required_evidence' `
        -JobResultContainsName $jobResultContainsName `
        -ApiListContainsName $apiListContainsName `
        -DirectSnapshotContainsName $directSnapshotContainsName `
        -Retryable $true
}

Export-ModuleMember -Function Get-PcvPhase21CheckpointEvidenceAssessment
