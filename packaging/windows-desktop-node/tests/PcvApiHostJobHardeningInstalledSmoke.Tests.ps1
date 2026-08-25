Set-StrictMode -Version Latest

Describe 'API host job hardening installed smoke runner' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ScriptPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1'
        $script:ReadmePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/README.md'
    }

    It 'ships a runner with body cap, route timeout, rate-limit, job, worker, and redaction evidence fields' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match 'PCV_REQUEST_BODY_TOO_LARGE'
        $content | Should -Match 'PCV_ROUTE_TIMEOUT'
        $content | Should -Match 'PCV_RATE_LIMIT_EXCEEDED'
        $content | Should -Match '/api/v1/auth/login'
        $content | Should -Match '/api/v1/runtime/policy'
        $content | Should -Match '/api/v1/runtime/route-timeout-probe'
        $content | Should -Match '/api/v1/jobs\?limit=1&offset=0'
        $content | Should -Match '/api/v1/jobs/pcv-installed-hardening-missing-job/cancel'
        $content | Should -Match '/api/v1/diagnostics/bundles\?limit=1&offset=0'
        $content | Should -Match '/api/v1/console/capabilities'
        $content | Should -Match 'RunRouteTimeoutProbe'
        $content | Should -Match 'worker_responsiveness'
        $content | Should -Match 'cooperative_cancellation_scope'
        $content | Should -Match 'wmi_abort_claim'
        $content | Should -Match 'token_value_observed\s*=\s*\$false'
        $content | Should -Match "public_trusted_signing\s*=\s*'not-claimed'"
        $content | Should -Match "external_stable_publication\s*=\s*'not-claimed'"
    }

    It 'writes a dry-run summary without requiring an installed service or admin mutation' {
        $artifactRoot = Join-Path $TestDrive 'api-host-job-hardening-installed-dryrun'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:ScriptPath `
            -ArtifactRoot $artifactRoot `
            -ApiBaseUri 'http://127.0.0.1:7777' `
            -DryRun | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summaryPath = Join-Path $artifactRoot 'summary.json'
        $summaryPath | Should -Exist
        $summary = Get-Content -Raw -LiteralPath $summaryPath | ConvertFrom-Json

        $summary.ok | Should -BeTrue
        $summary.actual_execution | Should -Be 'dry-run-no-http'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.installed_listener_required | Should -BeTrue
        $summary.bearer_token_source | Should -Be 'none'
        $summary.bearer_token_environment_variable_name | Should -Be 'PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN'
        $summary.token_value_observed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.body_cap.expected_status_code | Should -Be 413
        $summary.body_cap.expected_error_code | Should -Be 'PCV_REQUEST_BODY_TOO_LARGE'
        $summary.body_cap.expected_content_type | Should -Be 'application/problem+json'
        $summary.route_timeout.expected_status_code | Should -Be 504
        $summary.route_timeout.expected_error_code | Should -Be 'PCV_ROUTE_TIMEOUT'
        $summary.rate_limit.expected_error_code | Should -Be 'PCV_RATE_LIMIT_EXCEEDED'
        $summary.job_readability.path | Should -Be '/api/v1/jobs?limit=1&offset=0'
        $summary.job_cancellation.path | Should -Be '/api/v1/jobs/pcv-installed-hardening-missing-job/cancel'
        $summary.wmi_abort_claim | Should -Be 'not-claimed'
    }

    It 'requires body cap problem-details content type and job cancel contract in the pass gate' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match '\$bodyCapEvidence\.content_type\s+-like\s+''application/problem\+json\*'''
        $content | Should -Match '\$cancelResponse\.status_code\s+-in\s+@\(404,\s*401,\s*403\)'
        $content | Should -Match '\$cancelEvidence\.error_code\s+-in\s+@\((''PCV_JOB_NOT_FOUND''|''PCV_AUTH_REQUIRED''|''PCV_AUTH_FORBIDDEN''|''PCV_RBAC_FORBIDDEN''),\s*(''PCV_JOB_NOT_FOUND''|''PCV_AUTH_REQUIRED''|''PCV_AUTH_FORBIDDEN''|''PCV_RBAC_FORBIDDEN''),\s*(''PCV_JOB_NOT_FOUND''|''PCV_AUTH_REQUIRED''|''PCV_AUTH_FORBIDDEN''|''PCV_RBAC_FORBIDDEN''),\s*(''PCV_JOB_NOT_FOUND''|''PCV_AUTH_REQUIRED''|''PCV_AUTH_FORBIDDEN''|''PCV_RBAC_FORBIDDEN'')\)'
    }

    It 'requires rate-limit Retry-After and problem-details content type when the opt-in probe runs' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match 'expected_retry_after_present'
        $content | Should -Match 'expected_content_type_observed'
        $content | Should -Match '\$rateLimitEvidence\.first_429\.retry_after'
        $content | Should -Match '\$rateLimitEvidence\.first_429\.content_type\s+-like\s+''application/problem\+json\*'''
    }

    It 'requires controlled route-timeout 504, Retry-After, and problem-details when the opt-in probe runs' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match 'RunRouteTimeoutProbe'
        $content | Should -Match '/api/v1/runtime/route-timeout-probe'
        $content | Should -Match 'PCV_ROUTE_TIMEOUT'
        $content | Should -Match '\$routeTimeoutEvidence\.status_code\s+-eq\s+504'
        $content | Should -Match '\$routeTimeoutEvidence\.content_type\s+-like\s+''application/problem\+json\*'''
        $content | Should -Match '\$routeTimeoutEvidence\.retry_after'
    }

    It 'requires recorded diagnostics and console read probes in the pass gate' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match '\$diagnosticsResponse\.status_code\s+-eq\s+200'
        $content | Should -Match '\$consoleResponse\.status_code\s+-eq\s+200'
        $content | Should -Match 'Add-PcvEvidenceProperty -Object \$summary\.diagnostics_readability -Name ''expected_contract_observed'''
        $content | Should -Match 'Add-PcvEvidenceProperty -Object \$summary\.console_capabilities -Name ''expected_contract_observed'''
        $content | Should -Not -Match '\$diagnosticsResponse\.status_code\s+-in\s+@\(200,\s*401,\s*403\)'
        $content | Should -Not -Match '\$consoleResponse\.status_code\s+-in\s+@\(200,\s*401,\s*403\)'
    }

    It 'allows success response evidence to have no expected problem code' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match '\[AllowEmptyString\(\)\]\s*\r?\n\s*\[Parameter\(Mandatory\)\]\[string\]\$ExpectedErrorCode'
        $content | Should -Match '\[string\]::IsNullOrWhiteSpace\(\$ExpectedErrorCode\)'
        $content | Should -Match '\[string\]::IsNullOrWhiteSpace\(\$errorCode\)'
        $content | Should -Match '\$statusCode\s+-ge\s+200\s+-and\s+\$statusCode\s+-lt\s+300'
        $content | Should -Match 'function Add-PcvEvidenceProperty'
        $content | Should -Match 'Add-PcvEvidenceProperty -Object \$summary\.body_cap -Name ''expected_content_type'''
    }

    It 'decodes byte-array Invoke-WebRequest content before extracting problem codes' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match 'function ConvertTo-PcvResponseBodyText'
        $content | Should -Match 'body = ConvertTo-PcvResponseBodyText -Content \$response\.Content'
        $content | Should -Match '\[System\.Text\.Encoding\]::UTF8\.GetString\(\$Content\)'
    }

    It 'keeps Task 6 artifacts free of Task 7 wording and raw-token README examples' {
        $script:ScriptPath | Should -Exist
        $script:ReadmePath | Should -Exist
        $scriptText = Get-Content -LiteralPath $script:ScriptPath -Raw
        $readmeText = Get-Content -LiteralPath $script:ReadmePath -Raw

        $scriptText | Should -Not -Match 'Task 7'
        $readmeText | Should -Not -Match 'Task 7'
        $readmeText | Should -Not -Match "\$token\s*=\s*'<[^']*(token|bearer)[^']*>'"
        $readmeText | Should -Not -Match '-BearerToken\s+\$'
        $scriptText | Should -Match 'BearerTokenEnvironmentVariableName'
        $scriptText | Should -Match 'PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN'
        $readmeText | Should -Match 'PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN'
        $readmeText | Should -Match 'BearerTokenEnvironmentVariableName'
        $readmeText | Should -Match 'Read-Host -AsSecureString'
    }

    It 'does not contain host mutation, service reconfiguration, installer, or public publication commands' {
        $script:ScriptPath | Should -Exist
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|msiexec|sc\.exe\s+config|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|certutil|winget\s+submit|catalog upload'
    }
}
