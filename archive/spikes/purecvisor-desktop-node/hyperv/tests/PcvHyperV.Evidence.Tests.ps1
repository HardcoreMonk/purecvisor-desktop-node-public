BeforeAll {
    $Root = Split-Path -Parent $PSScriptRoot
    $script:EvidenceModulePath = Join-Path $Root 'PcvHyperVEvidence.psm1'
    Import-Module $script:EvidenceModulePath -Force
}

Describe 'Phase 21 Hyper-V checkpoint evidence assessment' {
    It 'classifies the 2026-04-30 product-flow checkpoint result as inconclusive when raw evidence is missing' {
        $evidence = [pscustomobject]@{
            lifecycle = [pscustomobject]@{
                checkpoint_status = 'succeeded'
                checkpoint_list_contains_name = $false
            }
        }

        $assessment = Get-PcvPhase21CheckpointEvidenceAssessment `
            -Evidence $evidence `
            -CheckpointName 'pcv-phase21-before-poweroff'

        $assessment.ok | Should -BeFalse
        $assessment.status | Should -Be 'inconclusive_missing_raw_evidence'
        $assessment.root_cause | Should -Be 'evidence_capture_incomplete'
        $assessment.missing_evidence | Should -Contain 'checkpoint_job_result'
        $assessment.missing_evidence | Should -Contain 'checkpoint_list_response'
        $assessment.missing_evidence | Should -Contain 'direct_snapshots'
    }

    It 'accepts checkpoint evidence only when job result API list and direct snapshot all contain the checkpoint' {
        $evidence = [pscustomobject]@{
            lifecycle = [pscustomobject]@{
                checkpoint_status = 'succeeded'
                checkpoint_job_result = [pscustomobject]@{
                    data = [pscustomobject]@{
                        status = 'succeeded'
                        result = [pscustomobject]@{
                            data = [pscustomobject]@{ name = 'pcv-phase21-before-poweroff' }
                        }
                    }
                }
                checkpoint_list_response = [pscustomobject]@{
                    data = @(
                        [pscustomobject]@{ name = 'pcv-phase21-before-poweroff'; vm_name = 'pcv-phase21-00000000' }
                    )
                }
                direct_snapshots = @(
                    [pscustomobject]@{ Name = 'pcv-phase21-before-poweroff'; VMName = 'pcv-phase21-00000000' }
                )
            }
        }

        $assessment = Get-PcvPhase21CheckpointEvidenceAssessment `
            -Evidence $evidence `
            -CheckpointName 'pcv-phase21-before-poweroff'

        $assessment.ok | Should -BeTrue
        $assessment.status | Should -Be 'verified_visible'
        $assessment.job_result_contains_name | Should -BeTrue
        $assessment.api_list_contains_name | Should -BeTrue
        $assessment.direct_snapshot_contains_name | Should -BeTrue
    }

    It 'separates API list mismatch from direct Hyper-V visibility' {
        $evidence = [pscustomobject]@{
            lifecycle = [pscustomobject]@{
                checkpoint_status = 'succeeded'
                checkpoint_job_result = [pscustomobject]@{
                    data = [pscustomobject]@{
                        status = 'succeeded'
                        result = [pscustomobject]@{
                            data = [pscustomobject]@{ name = 'pcv-phase21-before-poweroff' }
                        }
                    }
                }
                checkpoint_list_response = [pscustomobject]@{
                    data = @()
                }
                direct_snapshots = @(
                    [pscustomobject]@{ Name = 'pcv-phase21-before-poweroff'; VMName = 'pcv-phase21-00000000' }
                )
            }
        }

        $assessment = Get-PcvPhase21CheckpointEvidenceAssessment `
            -Evidence $evidence `
            -CheckpointName 'pcv-phase21-before-poweroff'

        $assessment.ok | Should -BeFalse
        $assessment.status | Should -Be 'api_checkpoint_list_mismatch'
        $assessment.root_cause | Should -Be 'api_or_evidence_list_capture_mismatch'
        $assessment.direct_snapshot_contains_name | Should -BeTrue
        $assessment.api_list_contains_name | Should -BeFalse
    }

    It 'preserves helper visibility failure as the actionable root cause' {
        $evidence = [pscustomobject]@{
            lifecycle = [pscustomobject]@{
                checkpoint_status = 'failed'
                checkpoint_job_result = [pscustomobject]@{
                    data = [pscustomobject]@{
                        status = 'failed'
                        error = [pscustomobject]@{
                            code = 'PCV_CHECKPOINT_NOT_VISIBLE'
                            retryable = $true
                        }
                    }
                }
                checkpoint_list_response = [pscustomobject]@{ data = @() }
                direct_snapshots = @()
            }
        }

        $assessment = Get-PcvPhase21CheckpointEvidenceAssessment `
            -Evidence $evidence `
            -CheckpointName 'pcv-phase21-before-poweroff'

        $assessment.ok | Should -BeFalse
        $assessment.status | Should -Be 'create_visibility_failure'
        $assessment.root_cause | Should -Be 'hyperv_checkpoint_not_visible_after_create'
        $assessment.retryable | Should -BeTrue
    }
}
