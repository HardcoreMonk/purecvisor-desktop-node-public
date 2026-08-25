# PureCVisor Desktop Node Phase 13 WinSW Service Wrapper Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Windows Desktop Node 제품 wrapper가 PowerShell listener를 SCM에 직접 등록하지 않고 WinSW service wrapper로 설치, 시작, 상태 확인, 진단 수집, 롤백, 제거를 수행하게 만든다.

**Architecture:** Phase 12의 Local API, Web Console, Hyper-V helper 자산은 유지하고 `packaging/windows-desktop-node/` wrapper가 WinSW executable과 XML config를 제품 루트의 `winsw/` 아래 staging한다. 제품 action orchestration은 WinSW command를 기준으로 바뀌며, loopback Web Console static asset은 API bearer token과 분리해 무인증으로 제공한다.

**Tech Stack:** PowerShell 7, Pester 5, WinSW XML config, Windows `HttpListener`, JSON product manifest, static Web Console assets.

---

## 파일 구조

- Modify: `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
  - `-WinSwPath` 입력을 받아 제품 plan과 mutating action으로 전달한다.
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
  - WinSW artifact resolve, SHA-256 계산, XML 생성, command builder, staging, product action 전환, diagnostic bundle 확장을 담당한다.
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
  - WinSW 기본 경로, XML, command, missing artifact policy를 검증한다.
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
  - WinSW executable staging, XML staging, manifest의 WinSW metadata를 검증한다.
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
  - entrypoint `-WinSwPath`, Install/Status/Rollback/Uninstall WinSW orchestration 순서를 검증한다.
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
  - diagnostic bundle의 WinSW XML/log/status/hash artifact와 redaction을 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - loopback static asset 무인증 허용 switch를 추가하되 API route bearer token 요구는 유지한다.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1`
  - token이 설정된 상태에서도 loopback static만 무인증으로 열리고 API route는 401을 유지하는 회귀 테스트를 추가한다.
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
  - loopback static asset auth boundary를 기록한다.
- Modify: `packaging/windows-desktop-node/README.md`
  - WinSW input, staging 경로, 관리자 smoke 명령을 갱신한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Phase 13 WinSW product wrapper 검증 기준과 기대 suite를 추가한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Phase 13 plan 진입점을 추가한다.
- Modify: `spikes/purecvisor-desktop-node/README.md`
  - Phase 13이 service host 차단점을 WinSW로 해소하는 제품 wrapper 단계임을 기록한다.
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
  - Phase 13 결정 token, spec, plan, verification policy drift를 검증한다.
- Modify: `follower.md`
  - Phase 13 구현 진행 상태와 남은 관리자 smoke 항목을 기록한다.

## Task 1: WinSW product plan contract red tests

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`

- [ ] **Step 1: Add WinSW plan contract tests**

Append these tests inside the existing `Describe 'PcvDesktopNodeProduct plan contract'` block in `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`:

```powershell
    It 'builds a WinSW product service plan with stable paths and command names' {
        $winSwSource = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource

        $plan.service.mode | Should -Be 'winsw'
        $plan.paths.winsw_dir | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\winsw'
        $plan.paths.winsw_exe | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\winsw\PureCVisorDesktopNode.exe'
        $plan.paths.winsw_xml | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\winsw\PureCVisorDesktopNode.xml'
        $plan.paths.service_logs_root | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\service-logs'
        $plan.service.winsw.source_path | Should -Be $winSwSource
        $plan.service.winsw.source_sha256 | Should -Match '^[A-Fa-f0-9]{64}$'
        $plan.service.winsw.staged_path | Should -Be $plan.paths.winsw_exe
        $plan.service.winsw.xml_path | Should -Be $plan.paths.winsw_xml
        $plan.service.config.binary_path | Should -Be $null

        $install = @($plan.service.commands.install)
        $install.Count | Should -Be 1
        $install[0].file_name | Should -Be $plan.paths.winsw_exe
        $install[0].arguments | Should -Be @('install')

        $plan.service.commands.start[0].arguments | Should -Be @('start')
        $plan.service.commands.stop[0].arguments | Should -Be @('stop')
        $plan.service.commands.status[0].arguments | Should -Be @('status')
        $plan.service.commands.uninstall[0].arguments | Should -Be @('uninstall')
    }

    It 'generates WinSW XML without token values and with absolute Local API paths' {
        $winSwSource = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource

        $xml = $plan.service.winsw.xml
        $xml | Should -Match '<id>PureCVisorDesktopNode</id>'
        $xml | Should -Match '<name>PureCVisor Desktop Node</name>'
        $xml | Should -Match '<executable>.+pwsh\.exe</executable>'
        $xml | Should -Match ([regex]::Escape('-File "C:\Program Files\PureCVisor\DesktopNode\api\Invoke-PcvDesktopApi.ps1"'))
        $xml | Should -Match ([regex]::Escape('-WebRootPath "C:\Program Files\PureCVisor\DesktopNode\web"'))
        $xml | Should -Match ([regex]::Escape('-ApiTokenFile "C:\ProgramData\PureCVisor\desktop-node\api-token.txt"'))
        $xml | Should -Match ([regex]::Escape('<logpath>C:\ProgramData\PureCVisor\desktop-node\service-logs</logpath>'))
        $xml | Should -Match '<stoptimeout>15 sec</stoptimeout>'
        $xml | Should -Not -Match '-ApiToken "'
        $xml | Should -Not -Match 'fake-token'
    }

    It 'marks missing WinSW artifact as an install-time product error' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node'

        $plan.service.mode | Should -Be 'winsw'
        $plan.service.winsw.source_path | Should -Be $null
        $plan.service.winsw.source_sha256 | Should -Be $null
        $plan.service.winsw.missing_source_error.code | Should -Be 'PCV_PRODUCT_WINSW_PATH_REQUIRED'
    }
```

- [ ] **Step 2: Add entrypoint parameter red test**

Append this test inside `Describe 'PcvDesktopNodeProduct entrypoint command surface'` in `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`:

```powershell
    It 'passes WinSwPath from the product entrypoint into the JSON plan' {
        $winSwSource = Join-Path $TestDrive 'winsw-entrypoint.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $output = & pwsh -NoProfile -File $script:Entrypoint `
            -Action Plan `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.service.mode | Should -Be 'winsw'
        $json.service.winsw.source_path | Should -Be $winSwSource
        $json.service.winsw.source_sha256 | Should -Match '^[A-Fa-f0-9]{64}$'
    }
```

- [ ] **Step 3: Run red tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: tests fail because `-WinSwPath`, WinSW plan fields, and WinSW XML generation do not exist.

## Task 2: WinSW plan model and XML generator

**Files:**
- Modify: `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Test: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Test: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`

- [ ] **Step 1: Add `-WinSwPath` to the product entrypoint**

In `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`, add the parameter near the other path parameters:

```powershell
    [string]$WinSwPath = '',
```

Then add this block before `$plan = New-PcvDesktopNodeProductPlan @planArgs`:

```powershell
    if (-not [string]::IsNullOrWhiteSpace($WinSwPath)) {
        $planArgs.WinSwPath = $WinSwPath
    }
```

- [ ] **Step 2: Extend product defaults and paths**

In `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`, update `Get-PcvDesktopNodeProductDefaults` so the returned ordered object includes:

```powershell
        winsw_exe_name = 'PureCVisorDesktopNode.exe'
        winsw_xml_name = 'PureCVisorDesktopNode.xml'
        service_logs_root = Join-Path $dataRoot 'service-logs'
```

Update `Resolve-PcvDesktopNodeProductPaths` so it uses `$defaults = Get-PcvDesktopNodeProductDefaults` and returns these additional keys:

```powershell
        winsw_dir = Join-PcvProductPath -Root $ProductRoot -ChildPath @('winsw')
        winsw_exe = Join-PcvProductPath -Root $ProductRoot -ChildPath @('winsw', $defaults.winsw_exe_name)
        winsw_xml = Join-PcvProductPath -Root $ProductRoot -ChildPath @('winsw', $defaults.winsw_xml_name)
        service_logs_root = Join-PcvProductPath -Root $DataRoot -ChildPath @('service-logs')
```

- [ ] **Step 3: Add WinSW helper functions**

Add these functions before `New-PcvDesktopNodeServicePlan`:

```powershell
function Get-PcvFileSha256 {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_PRODUCT_FILE_NOT_FOUND|The file was not found.|Path: '$Path'."
    }

    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function ConvertTo-PcvXmlEscapedText {
    param([AllowNull()][string]$Value)

    if ($null -eq $Value) {
        return ''
    }

    [System.Security.SecurityElement]::Escape($Value)
}

function Quote-PcvWinSwArgument {
    param([Parameter(Mandatory)][string]$Value)

    if ($Value -match '^[A-Za-z0-9._:/\\-]+$') {
        return $Value
    }

    '"' + ($Value -replace '"', '\"') + '"'
}

function New-PcvDesktopNodeWinSwArguments {
    param(
        [Parameter(Mandatory)]$Paths,
        [Parameter(Mandatory)][string]$Prefix,
        [Parameter(Mandatory)][int]$WorkerCount,
        [Parameter(Mandatory)][int]$TimeoutSec
    )

    $arguments = @(
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        $Paths.api_script,
        '-Prefix',
        $Prefix,
        '-HelperScriptPath',
        $Paths.helper_script,
        '-JobStorePath',
        $Paths.job_store,
        '-WebRootPath',
        $Paths.web_root,
        '-ApiTokenFile',
        $Paths.token_file,
        '-EventLogPath',
        $Paths.event_log,
        '-WorkerCount',
        [string]$WorkerCount,
        '-TimeoutSec',
        [string]$TimeoutSec
    )

    ($arguments | ForEach-Object { Quote-PcvWinSwArgument -Value ([string]$_) }) -join ' '
}

function New-PcvDesktopNodeWinSwXml {
    param(
        [Parameter(Mandatory)][string]$ServiceName,
        [Parameter(Mandatory)][string]$DisplayName,
        [Parameter(Mandatory)][string]$Description,
        [Parameter(Mandatory)][string]$PwshPath,
        [Parameter(Mandatory)][string]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$LogPath
    )

    $id = ConvertTo-PcvXmlEscapedText -Value $ServiceName
    $name = ConvertTo-PcvXmlEscapedText -Value $DisplayName
    $descriptionText = ConvertTo-PcvXmlEscapedText -Value $Description
    $executable = ConvertTo-PcvXmlEscapedText -Value $PwshPath
    $argumentsText = ConvertTo-PcvXmlEscapedText -Value $Arguments
    $workingDirectoryText = ConvertTo-PcvXmlEscapedText -Value $WorkingDirectory
    $logPathText = ConvertTo-PcvXmlEscapedText -Value $LogPath

@"
<service>
  <id>$id</id>
  <name>$name</name>
  <description>$descriptionText</description>
  <executable>$executable</executable>
  <arguments>$argumentsText</arguments>
  <workingdirectory>$workingDirectoryText</workingdirectory>
  <logpath>$logPathText</logpath>
  <log mode="roll" />
  <stoptimeout>15 sec</stoptimeout>
  <onfailure action="restart" delay="60 sec" />
  <onfailure action="restart" delay="60 sec" />
  <onfailure action="none" />
</service>
"@
}

function Resolve-PcvDesktopNodeWinSwArtifact {
    param([AllowNull()][string]$WinSwPath)

    if ([string]::IsNullOrWhiteSpace($WinSwPath)) {
        return [ordered]@{
            source_path = $null
            source_sha256 = $null
            missing_source_error = (New-PcvProductError `
                -Code 'PCV_PRODUCT_WINSW_PATH_REQUIRED' `
                -Message 'WinSW executable path is required for mutating product install.' `
                -Detail 'Pass -WinSwPath with a local WinSW executable prepared by the operator or packaging step.')
        }
    }

    $resolved = (Resolve-Path -LiteralPath $WinSwPath -ErrorAction Stop).Path
    [ordered]@{
        source_path = $resolved
        source_sha256 = Get-PcvFileSha256 -Path $resolved
        missing_source_error = $null
    }
}
```

- [ ] **Step 4: Replace product service plan construction with WinSW mode**

Replace the body of `New-PcvDesktopNodeServicePlan` with a WinSW-based plan. Keep the function name so existing product code has one migration point:

```powershell
function New-PcvDesktopNodeServicePlan {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)]$Paths,
        [string]$ServiceName = (Get-PcvDesktopNodeProductDefaults).service_name,
        [string]$DisplayName = (Get-PcvDesktopNodeProductDefaults).display_name,
        [string]$Description = 'PureCVisor Desktop Node Local API service.',
        [string]$Prefix = (Get-PcvDesktopNodeProductDefaults).prefix,
        [string]$ServiceAccount = 'LocalSystem',
        [ValidateRange(1, 64)][int]$WorkerCount = 1,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30,
        [AllowNull()][string]$WinSwPath
    )

    Import-PcvDesktopServiceSupport -SourceRoot $SourceRoot
    $pwshPath = Resolve-PcvDesktopServicePwshPath
    $arguments = New-PcvDesktopNodeWinSwArguments `
        -Paths $Paths `
        -Prefix $Prefix `
        -WorkerCount $WorkerCount `
        -TimeoutSec $TimeoutSec
    $xml = New-PcvDesktopNodeWinSwXml `
        -ServiceName $ServiceName `
        -DisplayName $DisplayName `
        -Description $Description `
        -PwshPath $pwshPath `
        -Arguments $arguments `
        -WorkingDirectory $Paths.product_root `
        -LogPath $Paths.service_logs_root
    $artifact = Resolve-PcvDesktopNodeWinSwArtifact -WinSwPath $WinSwPath

    [ordered]@{
        mode = 'winsw'
        config = [ordered]@{
            service_name = $ServiceName
            display_name = $DisplayName
            description = $Description
            service_account = $ServiceAccount
            prefix = $Prefix
            exposure = 'loopback'
            auth_required = $true
            api_token_source = 'file'
            binary_path = $null
        }
        winsw = [ordered]@{
            source_path = $artifact.source_path
            source_sha256 = $artifact.source_sha256
            staged_path = $Paths.winsw_exe
            xml_path = $Paths.winsw_xml
            xml = $xml
            missing_source_error = $artifact.missing_source_error
        }
        commands = [ordered]@{
            install = @([ordered]@{ file_name = $Paths.winsw_exe; arguments = @('install') })
            start = @([ordered]@{ file_name = $Paths.winsw_exe; arguments = @('start') })
            stop = @([ordered]@{ file_name = $Paths.winsw_exe; arguments = @('stop') })
            uninstall = @([ordered]@{ file_name = $Paths.winsw_exe; arguments = @('uninstall') })
            status = @([ordered]@{ file_name = $Paths.winsw_exe; arguments = @('status') })
        }
    }
}
```

- [ ] **Step 5: Pass `WinSwPath` through product plan creation**

Add `[AllowNull()][string]$WinSwPath` to `New-PcvDesktopNodeProductPlan`, and pass it into `New-PcvDesktopNodeServicePlan`:

```powershell
    $service = New-PcvDesktopNodeServicePlan `
        -SourceRoot $SourceRoot `
        -Paths $paths `
        -ServiceName $ServiceName `
        -DisplayName $DisplayName `
        -Prefix $Prefix `
        -ServiceAccount $ServiceAccount `
        -WorkerCount $WorkerCount `
        -TimeoutSec $TimeoutSec `
        -WinSwPath $WinSwPath
```

- [ ] **Step 6: Export the new helper functions**

Add these names to `Export-ModuleMember`:

```powershell
    Get-PcvFileSha256, `
    New-PcvDesktopNodeWinSwArguments, `
    New-PcvDesktopNodeWinSwXml, `
    Resolve-PcvDesktopNodeWinSwArtifact
```

- [ ] **Step 7: Run plan and entrypoint tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -FullName '*passes WinSwPath from the product entrypoint into the JSON plan*' -Output Detailed -PassThru; if ($r.FailedCount -gt 0 -or $r.PassedCount -ne 1) { exit 1 }"
```

Expected: plan suite passes and the new entrypoint parameter test passes.

- [ ] **Step 8: Commit WinSW plan model**

Run:

```powershell
git add packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "Add Desktop Node WinSW product plan"
```

Expected: commit succeeds.

## Task 3: WinSW artifact staging and manifest metadata

**Files:**
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`

- [ ] **Step 1: Add failing WinSW staging tests**

Append these tests inside `Describe 'PcvDesktopNodeProduct manifest and asset copy contract'`:

```powershell
    It 'stages WinSW executable and XML into the product root' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeWinSW'
        $dataRoot = Join-Path $TestDrive 'data-winsw'
        $winSwSource = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $result = Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.13.0-test' `
            -WinSwPath $winSwSource

        $result.ok | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.xml') | Should -BeTrue
        $xml = Get-Content -LiteralPath (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.xml') -Raw
        $xml | Should -Match '<id>PureCVisorDesktopNode</id>'
        $xml | Should -Match ([regex]::Escape("-WebRootPath `"$productRoot\web`""))
        $xml | Should -Not -Match '-ApiToken "'
    }

    It 'records WinSW metadata in product-manifest.json' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeWinSWManifest'
        $dataRoot = Join-Path $TestDrive 'data-winsw-manifest'
        $winSwSource = Join-Path $TestDrive 'winsw-manifest.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.13.0-test' `
            -WinSwPath $winSwSource | Out-Null

        $manifest = Get-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Raw | ConvertFrom-Json
        $manifest.winsw.source_path | Should -Be $winSwSource
        $manifest.winsw.staged_path | Should -Be (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.exe')
        $manifest.winsw.xml_path | Should -Be (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.xml')
        $manifest.winsw.source_sha256 | Should -Match '^[A-Fa-f0-9]{64}$'
    }
```

In the existing tests `excludes source tests directories from manifest and copied product assets` and `copies product assets and writes a product manifest`, create a fake WinSW executable before `Copy-PcvDesktopNodeProductAssets`:

```powershell
        $winSwSource = Join-Path $TestDrive 'winsw-existing-test.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
```

Then pass the new argument into each `Copy-PcvDesktopNodeProductAssets` call:

```powershell
            -WinSwPath $winSwSource
```

- [ ] **Step 2: Run red manifest tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because `Copy-PcvDesktopNodeProductAssets` does not accept or stage `-WinSwPath`.

- [ ] **Step 3: Update product manifest function**

Add `[AllowNull()][string]$WinSwPath` to `New-PcvDesktopNodeProductManifest`. Inside the function, after `$paths = Resolve-PcvDesktopNodeProductPaths ...`, create a service plan:

```powershell
    $servicePlan = New-PcvDesktopNodeServicePlan `
        -SourceRoot $SourceRoot `
        -Paths $paths `
        -ServiceName (Get-PcvDesktopNodeProductDefaults).service_name `
        -DisplayName (Get-PcvDesktopNodeProductDefaults).display_name `
        -Prefix (Get-PcvDesktopNodeProductDefaults).prefix `
        -WinSwPath $WinSwPath
```

Add this manifest field to the returned ordered object:

```powershell
        winsw = [ordered]@{
            source_path = $servicePlan.winsw.source_path
            staged_path = $servicePlan.winsw.staged_path
            xml_path = $servicePlan.winsw.xml_path
            source_sha256 = $servicePlan.winsw.source_sha256
        }
```

- [ ] **Step 4: Add WinSW staging function**

Add this function before `Copy-PcvDesktopNodeProductAssets`:

```powershell
function Copy-PcvDesktopNodeWinSwArtifact {
    param(
        [Parameter(Mandatory)]$ServicePlan,
        [Parameter(Mandatory)]$Paths
    )

    if ($null -ne $ServicePlan.winsw.missing_source_error) {
        throw "$($ServicePlan.winsw.missing_source_error.code)|$($ServicePlan.winsw.missing_source_error.message)|$($ServicePlan.winsw.missing_source_error.detail)"
    }

    New-Item -ItemType Directory -Path $Paths.winsw_dir -Force -ErrorAction Stop | Out-Null
    New-Item -ItemType Directory -Path $Paths.service_logs_root -Force -ErrorAction Stop | Out-Null
    Copy-Item `
        -LiteralPath $ServicePlan.winsw.source_path `
        -Destination $ServicePlan.winsw.staged_path `
        -Force `
        -ErrorAction Stop
    Set-Content `
        -LiteralPath $ServicePlan.winsw.xml_path `
        -Value $ServicePlan.winsw.xml `
        -Encoding UTF8 `
        -ErrorAction Stop

    [ordered]@{
        ok = $true
        executable = $ServicePlan.winsw.staged_path
        xml = $ServicePlan.winsw.xml_path
        source_sha256 = $ServicePlan.winsw.source_sha256
    }
}
```

Export `Copy-PcvDesktopNodeWinSwArtifact`.

- [ ] **Step 5: Stage WinSW during asset copy**

Add `[AllowNull()][string]$WinSwPath` to `Copy-PcvDesktopNodeProductAssets`. After copying existing spike assets and before writing `product-manifest.json`, build `$paths` and `$servicePlan`:

```powershell
    $paths = Resolve-PcvDesktopNodeProductPaths `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot
    $servicePlan = New-PcvDesktopNodeServicePlan `
        -SourceRoot $SourceRoot `
        -Paths $paths `
        -ServiceName (Get-PcvDesktopNodeProductDefaults).service_name `
        -DisplayName (Get-PcvDesktopNodeProductDefaults).display_name `
        -Prefix (Get-PcvDesktopNodeProductDefaults).prefix `
        -WinSwPath $WinSwPath
```

Then call:

```powershell
    $winSwStage = Copy-PcvDesktopNodeWinSwArtifact `
        -ServicePlan $servicePlan `
        -Paths $paths
```

Pass `-WinSwPath $WinSwPath` into `New-PcvDesktopNodeProductManifest`. Include the WinSW staging result in the returned object:

```powershell
        winsw = $winSwStage
```

- [ ] **Step 6: Run manifest tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: manifest suite passes.

- [ ] **Step 7: Commit WinSW staging**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1
git commit -m "Stage WinSW service wrapper artifacts"
```

Expected: commit succeeds.

## Task 4: Product action orchestration with WinSW commands

**Files:**
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`

- [ ] **Step 1: Replace install orchestration expectations**

In `PcvDesktopNodeProduct.Invoke.Tests.ps1`, replace the expected labels in `It 'orchestrates Install in meaningful order with full service command arguments'` with:

```powershell
        @($steps.label) | Should -Be @(
            'copy',
            'token',
            'PureCVisorDesktopNode.exe install',
            'PureCVisorDesktopNode.exe start',
            'health'
        )
        $steps[2].command_line | Should -Match 'PureCVisorDesktopNode\.exe install$'
        $steps[3].command_line | Should -Match 'PureCVisorDesktopNode\.exe start$'
        $steps[4].command_line | Should -Be 'health http://127.0.0.1:7777/'
```

Update that test's runner so command labels use the staged executable leaf name:

```powershell
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $steps.Add([ordered]@{
                    label = "$leafName $($Arguments[0])"
                    command_line = "$leafName $($Arguments -join ' ')"
                })
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
```

Update the test setup to create `$winSwSource`, pass `-WinSwPath $winSwSource` to `New-PcvDesktopNodeProductPlan`, and change the copy test double signature to:

```powershell
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath)
            $steps.Add([ordered]@{ label = 'copy'; command_line = "copy $ProductRoot $WinSwPath" })
            [ordered]@{ ok = $true; product_root = $ProductRoot; winsw = [ordered]@{ source_path = $WinSwPath } }
        }
```

Apply the same fake `$winSwSource` and `-WinSwPath $winSwSource` setup to the other Install orchestration tests in this file:

```powershell
        $winSwSource = Join-Path $TestDrive 'winsw-install-test.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data') `
            -WinSwPath $winSwSource
```

- [ ] **Step 2: Replace rollback, status, and uninstall expectations**

In the rollback tests, replace meaningful `sc.exe` strings with:

```powershell
            'PureCVisorDesktopNode.exe stop',
            "restore $($plan.product_root)",
            'PureCVisorDesktopNode.exe start',
            'health http://127.0.0.1:7777/'
```

In the status test, replace:

```powershell
        $steps | Should -Be @('sc.exe query PureCVisorDesktopNode')
```

with:

```powershell
        $steps | Should -Be @("$($plan.paths.winsw_exe) status")
```

Use this runner shape in status tests so the string includes the full staged executable path:

```powershell
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $steps.Add("$FileName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = 'Started'; stderr = '' }
        }
```

In the uninstall tests, replace:

```powershell
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe delete PureCVisorDesktopNode'
```

with:

```powershell
            'PureCVisorDesktopNode.exe stop',
            'PureCVisorDesktopNode.exe uninstall'
```

For the missing-service uninstall test, make the runner return exit code `1` when `$Arguments[0] -eq 'uninstall'` and `stderr = 'NonExistentService'`, then assert removal still continues only for that known missing-service text.

- [ ] **Step 3: Run red orchestration tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails until the product action passes `WinSwPath` to asset copy and no longer expects `sc.exe` missing-service behavior.

- [ ] **Step 4: Pass WinSW path into asset copy**

In `Invoke-PcvDesktopNodeProductAction`, update the default `$CopyAssets` invocation in the Install branch from:

```powershell
            $copyResult = & $CopyAssets `
                -SourceRoot $Plan.source_root `
                -ProductRoot $Plan.product_root `
                -DataRoot $Plan.data_root
```

to:

```powershell
            if ($null -ne $Plan.service.winsw.missing_source_error) {
                throw "$($Plan.service.winsw.missing_source_error.code)|$($Plan.service.winsw.missing_source_error.message)|$($Plan.service.winsw.missing_source_error.detail)"
            }

            $copyResult = & $CopyAssets `
                -SourceRoot $Plan.source_root `
                -ProductRoot $Plan.product_root `
                -DataRoot $Plan.data_root `
                -WinSwPath $Plan.service.winsw.source_path
```

Update the default `$CopyAssets` scriptblock signature to:

```powershell
        $CopyAssets = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath)
            Copy-PcvDesktopNodeProductAssets `
                -SourceRoot $SourceRoot `
                -ProductRoot $ProductRoot `
                -DataRoot $DataRoot `
                -WinSwPath $WinSwPath
        }
```

- [ ] **Step 5: Treat WinSW missing-service uninstall as non-fatal**

In the Uninstall branch, replace the `sc.exe` missing-service special case with a helper:

```powershell
function Test-PcvWinSwMissingServiceResult {
    param([Parameter(Mandatory)]$Result)

    if ($Result.ok) {
        return $false
    }

    $text = (($Result.results | ForEach-Object { "$($_.stdout)`n$($_.stderr)" }) -join "`n")
    $text -match '(?i)(nonexistentservice|service.*does not exist|no such service)'
}
```

Export `Test-PcvWinSwMissingServiceResult`. Use it in Uninstall:

```powershell
            if (-not $uninstallResults.ok -and -not (Test-PcvWinSwMissingServiceResult -Result $uninstallResults)) {
                Throw-PcvProductCommandResult -Result $uninstallResults
            }
```

- [ ] **Step 6: Run product invoke tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: entrypoint orchestration suite passes with WinSW commands.

- [ ] **Step 7: Commit WinSW product orchestration**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "Switch Desktop Node product actions to WinSW"
```

Expected: commit succeeds.

## Task 5: WinSW diagnostics

**Files:**
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`

- [ ] **Step 1: Add failing diagnostics test for WinSW artifacts**

Append this test inside `Describe 'PcvDesktopNodeProduct diagnostics'`:

```powershell
    It 'includes redacted WinSW XML, logs, status, and executable hash in diagnostics' {
        $outRoot = Join-Path $TestDrive 'diagnostics-winsw'
        $productRoot = Join-Path $TestDrive 'DesktopNodeWinSwDiag'
        $dataRoot = Join-Path $TestDrive 'data-winsw-diag'
        $winSwSource = Join-Path $TestDrive 'winsw-diag.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -WinSwPath $winSwSource
        New-Item -ItemType Directory -Path $plan.paths.winsw_dir -Force | Out-Null
        New-Item -ItemType Directory -Path $plan.paths.service_logs_root -Force | Out-Null
        Set-Content -LiteralPath $plan.paths.winsw_exe -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath $plan.paths.winsw_xml -Value $plan.service.winsw.xml -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.wrapper.log') -Value 'wrapper started Authorization: Bearer diag-secret' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.out.log') -Value 'stdout ready' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.err.log') -Value 'stderr ready' -Encoding UTF8

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Started'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $true; body = '{"ok":true}' }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        Test-Path -LiteralPath (Join-Path $bundle.path 'winsw-xml-redacted.xml') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'winsw-status-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'winsw-metadata-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'winsw-log-PureCVisorDesktopNode.wrapper.log') | Should -BeTrue
        $combined = Get-ChildItem -LiteralPath $bundle.path -File |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
            Out-String
        $combined | Should -Match 'Started'
        $combined | Should -Match 'stdout ready'
        $combined | Should -Not -Match 'diag-secret'
        $combined | Should -Match '\[PRODUCT_ROOT\]'
        $combined | Should -Match '\[DATA_ROOT\]'
        $combined | Should -Match 'source_sha256'
    }
```

- [ ] **Step 2: Run red diagnostics test**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because WinSW diagnostic files are not created.

- [ ] **Step 3: Add WinSW diagnostic collection**

Inside `New-PcvDesktopNodeDiagnosticBundle`, after existing service status collection, add:

```powershell
    $winSwStatus = Invoke-PcvProductProcessCommand `
        -Commands $Plan.service.commands.status `
        -InvokeProcess $InvokeProcess
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -Path (Join-Path $bundlePath 'winsw-status-redacted.json') `
        -Value $winSwStatus `
        -PathRedactions $pathRedactions
```

After manifest/event/job collection, add:

```powershell
    if (Test-Path -LiteralPath $Plan.paths.winsw_xml -PathType Leaf) {
        $xmlText = Get-Content -LiteralPath $Plan.paths.winsw_xml -Raw
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $xmlText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'winsw-xml-redacted.xml') -Encoding UTF8
    }

    if (Test-Path -LiteralPath $Plan.paths.service_logs_root -PathType Container) {
        Get-ChildItem -LiteralPath $Plan.paths.service_logs_root -File -ErrorAction Stop |
            Where-Object { $_.Name -match '\.(log|out|err)(\.\d+)?$' -or $_.Extension -eq '.log' } |
            ForEach-Object {
                $targetName = 'winsw-log-' + ($_.Name -replace '[^A-Za-z0-9_.-]', '_')
                $logText = Get-Content -LiteralPath $_.FullName -Raw
                ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $logText -PathRedactions $pathRedactions |
                    Set-Content -LiteralPath (Join-Path $bundlePath $targetName) -Encoding UTF8
            }
    }

    $metadata = [ordered]@{
        source_path = $Plan.service.winsw.source_path
        staged_path = $Plan.service.winsw.staged_path
        xml_path = $Plan.service.winsw.xml_path
        source_sha256 = $Plan.service.winsw.source_sha256
        staged_sha256 = $(if (Test-Path -LiteralPath $Plan.paths.winsw_exe -PathType Leaf) { Get-PcvFileSha256 -Path $Plan.paths.winsw_exe } else { $null })
    }
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -Path (Join-Path $bundlePath 'winsw-metadata-redacted.json') `
        -Value $metadata `
        -PathRedactions $pathRedactions
```

- [ ] **Step 4: Run diagnostics tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: diagnostics suite passes.

- [ ] **Step 5: Commit WinSW diagnostics**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1
git commit -m "Add WinSW diagnostics to Desktop Node product bundle"
```

Expected: commit succeeds.

## Task 6: Loopback static asset auth boundary

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1`

- [ ] **Step 1: Add failing static auth boundary tests**

Append these tests inside `Describe 'PcvDesktopApi static Web Console serving'`:

```powershell
    It 'serves static assets without bearer token when loopback static bypass is enabled' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'required-api-token' `
            -AllowUnauthenticatedStatic

        $response.status | Should -Be 200
        $response.body | Should -Be '<!doctype html><title>PureCVisor Desktop Node</title>'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'still requires bearer token for API routes when loopback static bypass is enabled' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/runtime/policy' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'required-api-token' `
            -AllowUnauthenticatedStatic

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
    }

    It 'requires bearer token for static assets when static bypass is disabled' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'required-api-token'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
    }
```

- [ ] **Step 2: Run red API static tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because `Invoke-PcvApiRequest` does not accept `-AllowUnauthenticatedStatic`.

- [ ] **Step 3: Add static bypass switch and helper**

In `Invoke-PcvApiRequest`, add:

```powershell
        [switch]$AllowUnauthenticatedStatic,
```

Add this helper before `Invoke-PcvApiRequest`:

```powershell
function Test-PcvStaticAssetRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$WebRootPath
    )

    $normalizedMethod = $Method.ToUpperInvariant()
    $pathOnly = ($Path -split '\?', 2)[0].TrimEnd('/')
    if ([string]::IsNullOrWhiteSpace($pathOnly)) {
        $pathOnly = '/'
    }

    $isApiPath = $pathOnly.Equals('/api', [System.StringComparison]::OrdinalIgnoreCase) -or
        $pathOnly.StartsWith('/api/', [System.StringComparison]::OrdinalIgnoreCase)

    [ordered]@{
        ok = ($normalizedMethod -eq 'GET' -and -not $isApiPath -and -not [string]::IsNullOrWhiteSpace($WebRootPath))
        path = $pathOnly
    }
}
```

Export `Test-PcvStaticAssetRequest`.

- [ ] **Step 4: Serve static before auth only when bypass is explicit**

At the beginning of `Invoke-PcvApiRequest`, after `$normalizedMethod = $Method.ToUpperInvariant()`, add:

```powershell
    $staticRequest = Test-PcvStaticAssetRequest `
        -Method $normalizedMethod `
        -Path $Path `
        -WebRootPath $WebRootPath

    if ($AllowUnauthenticatedStatic -and $staticRequest.ok) {
        $staticFile = Resolve-PcvStaticFilePath -WebRootPath $WebRootPath -Path $staticRequest.path
        if (-not $staticFile.ok) {
            return $staticFile.response
        }

        return New-PcvStaticFileResponse -Path $staticFile.path
    }
```

Leave the existing bearer-token check immediately after this block:

```powershell
    $auth = Test-PcvBearerToken -Headers $Headers -ApiToken $ApiToken
    if (-not $auth.ok) {
        return $auth.response
    }
```

- [ ] **Step 5: Pass static bypass only for loopback listener**

In `Start-PcvDesktopApi`, update the `Invoke-PcvApiRequest` call to include:

```powershell
                -AllowUnauthenticatedStatic:$prefixPolicy.is_loopback `
```

This keeps non-loopback LAN static assets behind the same bearer token policy.

- [ ] **Step 6: Run API tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: selected API auth/static/LAN tests pass.

- [ ] **Step 7: Commit static auth boundary**

Run:

```powershell
git add spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1 spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1
git commit -m "Allow loopback static Web Console without bearer"
```

Expected: commit succeeds.

## Task 7: Documentation and boundary policy updates

**Files:**
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
- Modify: `follower.md`

- [ ] **Step 1: Add Phase 13 boundary test**

Append this test to `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`:

```powershell
    It 'documents the Phase 13 WinSW service wrapper boundary' {
        $phase13Spec = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper-design.md') -Raw
        $phase13Plan = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $verificationPolicy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $packagingReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/README.md') -Raw

        $phase13Spec | Should -Match 'DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper'
        $phase13Plan | Should -Match 'WinSW'
        $phase13Plan | Should -Match 'AllowUnauthenticatedStatic'
        $developerIndex | Should -Match 'Phase 13'
        $developerIndex | Should -Match 'winsw-service-wrapper'
        $verificationPolicy | Should -Match 'Desktop Node Phase 13 WinSW product wrapper 변경'
        $packagingReadme | Should -Match '-WinSwPath'
    }
```

- [ ] **Step 2: Run red boundary test**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails until active docs mention Phase 13 implementation details.

- [ ] **Step 3: Update packaging README**

In `packaging/windows-desktop-node/README.md`, replace the Phase 12 service-start blocker paragraph with:

```markdown
## Phase 13 WinSW service wrapper

Phase 13은 Windows SCM에 `pwsh.exe -File Invoke-PcvDesktopApi.ps1`를 직접 등록하지 않는다. 제품 wrapper는 WinSW executable을 `C:\Program Files\PureCVisor\DesktopNode\winsw\PureCVisorDesktopNode.exe`로 staging하고, 같은 디렉터리에 `PureCVisorDesktopNode.xml`을 생성한다.

관리자는 mutating install에서 WinSW executable 경로를 명시한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan -WinSwPath '<winsw.exe>'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WinSwPath '<winsw.exe>'
```

제품 기본값은 loopback listener와 `-ApiTokenFile`을 유지한다. API route는 bearer token을 요구하며, loopback Web Console static asset은 브라우저 진입을 위해 무인증으로 제공한다.
```

Update the administrator smoke command block to include `-WinSwPath '<winsw.exe>'` on `Install`.

- [ ] **Step 4: Update API README**

Add this section to `spikes/purecvisor-desktop-node/api/README.md`:

```markdown
## Loopback Web Console static auth boundary

제품 service가 `-ApiTokenFile`을 사용할 때도 loopback prefix에서는 Web Console static asset을 무인증으로 제공한다. API route는 계속 bearer token을 요구한다.

- `GET /`: loopback static asset, token 없음 허용
- `GET /api/v1/runtime/policy`: bearer token 필요
- non-loopback LAN prefix: static asset도 token 정책 유지

이 경계는 Phase 13 WinSW service wrapper 관리자 smoke에서 브라우저가 먼저 `/`를 열고 이후 API token을 입력할 수 있게 하기 위한 제품 UX 결정이다.
```

- [ ] **Step 5: Update verification policy**

Add this row to the table in `docs/DEVELOPMENT_VERIFICATION_POLICY.md` after the Phase 12 row:

```markdown
| Desktop Node Phase 13 WinSW product wrapper 변경 | Packaging Pester suite + API static/auth/LAN suite + service suite 필수 | product dry-run smoke와 WinSW XML/manifest/diagnostic 검증 필수 | 실제 `Install -WinSwPath`, service RUNNING, token 포함 runtime policy 200, loopback Web Console root 200, CollectDiagnostics, Uninstall은 관리자 권한 환경에서 조건부 | Single Edge 릴리스 게이트와 분리 |
```

Add this command block near the Phase 12 command block:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan -WinSwPath '<winsw.exe>'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WinSwPath '<winsw.exe>' -WhatIf
```

- [ ] **Step 6: Update developer index and follower**

In `docs/DEVELOPER_INDEX.md`, add a Phase 13 plan entry next to the Phase 13 spec entry:

```markdown
### [Desktop Node Phase 13 WinSW service wrapper 구현 계획](superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md)

- 언제 보는지: WinSW command builder, XML generator, product action 전환, loopback static auth boundary, 관리자 smoke 절차를 구현할 때
- 왜 보는지: Phase 13 구현 task, 수정 파일, 테스트 명령, 커밋 단위를 기록하기 때문
- 같이 봐야 하는 문서: [Desktop Node Phase 13 WinSW service wrapper 결정](superpowers/specs/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper-design.md), [Desktop Node Phase 12/13/14 제품 wrapper README](../packaging/windows-desktop-node/README.md), [DEVELOPMENT_VERIFICATION_POLICY.md](DEVELOPMENT_VERIFICATION_POLICY.md)
```

In `follower.md`, update the Phase 13 section so it says:

```markdown
- Phase 13 구현 계획 문서 승인 대기: `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md`
- 구현 완료 축: WinSW plan/XML/staging, product action 전환, loopback static asset 무인증, diagnostics, 관리자 smoke 재검증 준비
```

- [ ] **Step 7: Run boundary checks**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
rg -n "Phase 13|WinSW|winsw-service-wrapper|WinSwPath|AllowUnauthenticatedStatic" docs packaging/windows-desktop-node spikes/purecvisor-desktop-node follower.md
git diff --check
```

Expected: boundary suite passes, grep shows Phase 13 references in active docs, diff hygiene passes.

- [ ] **Step 8: Commit docs and boundary**

Run:

```powershell
git add packaging/windows-desktop-node/README.md spikes/purecvisor-desktop-node/api/README.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md spikes/purecvisor-desktop-node/README.md spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1 follower.md
git commit -m "Document Desktop Node phase 13 WinSW implementation"
```

Expected: commit succeeds.

## Task 8: Full Phase 13 verification and evidence

**Files:**
- Modify: `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md`
- Modify: `follower.md`

- [ ] **Step 1: Run packaging suite**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: packaging tests pass with WinSW plan, staging, action, diagnostics coverage.

- [ ] **Step 2: Run Desktop Node component suites**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
node --check spikes/purecvisor-desktop-node/web/app.js
```

Expected: component suites pass and `node --check` exits 0.

- [ ] **Step 3: Run product dry-run smoke with a local fake WinSW executable**

Run:

```powershell
$fakeWinSw = Join-Path $env:TEMP 'pcv-fake-winsw.exe'
Set-Content -LiteralPath $fakeWinSw -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan -WinSwPath $fakeWinSw
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WinSwPath $fakeWinSw -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall -RemoveData -WhatIf
Remove-Item -LiteralPath $fakeWinSw -Force
```

Expected: each command exits 0 and returns JSON with `service.mode = winsw`.

- [ ] **Step 4: Run diff hygiene**

Run:

```powershell
git diff --check
git status -sb
```

Expected: `git diff --check` exits 0. `git status -sb` shows only intended files before the final verification commit.

- [ ] **Step 5: Record verification evidence**

Add a `## 완료 증거` section at the end of this plan with the exact pass counts and command outcomes from Steps 1-4. Update `follower.md` with the same Phase 13 status and list these administrator smoke items:

```markdown
- 실제 `Install -WinSwPath '<winsw.exe>'`
- service `RUNNING`
- token 포함 `GET /api/v1/runtime/policy` 200
- loopback `GET /` 무인증 200
- `CollectDiagnostics` WinSW artifact/redaction 확인
- 기본 `Uninstall`
- 실제 `Rollback`
- 실제 `Uninstall -RemoveData`
- 실제 Hyper-V VM create/start/poweroff/checkpoint lifecycle integration
```

- [ ] **Step 6: Commit final verification notes**

Run:

```powershell
git add docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md follower.md
git commit -m "Record Desktop Node phase 13 verification"
```

Expected: commit succeeds.

## 최종 검증 명령

Implementation completion requires these commands:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Administrator opt-in smoke remains separate:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WinSwPath '<winsw.exe>'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
$token = Get-Content "$env:ProgramData\PureCVisor\desktop-node\api-token.txt" -Raw
$headers = @{ Authorization = "Bearer $token" }
Invoke-WebRequest http://127.0.0.1:7777/api/v1/runtime/policy -Headers $headers
Invoke-WebRequest http://127.0.0.1:7777/
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall
```

## 완료 증거

작성 기준: 2026-04-27.

기본 검증:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests'`: 44 passed, 0 failed, 0 NotRun.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests'`: 8 passed, 0 failed, 0 NotRun.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests'`: 88 passed, 0 failed, 0 NotRun.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests'`: 13 passed, 0 failed, 0 NotRun.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests'`: 11 passed, 0 failed, 0 NotRun.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests'`: 11 passed, 0 failed, 0 NotRun.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration`: 41 passed, 0 failed, 1 NotRun.
- `PCV_HYPERV_INTEGRATION=1`, `PCV_HYPERV_TEST_ISO=D:\Downloads\Rocky-10.1-x86_64-minimal.iso`, `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests'`: 42 passed, 0 failed, 0 NotRun.
- `node --check spikes/purecvisor-desktop-node/web/app.js`: exit 0.

Product dry-run smoke:

- Fake WinSW executable을 `%TEMP%` 아래에 만들고 `Plan -WinSwPath`, `Install -WinSwPath -WhatIf`, `Uninstall -RemoveData -WhatIf`를 실행했다.
- 결과: `PlanAction=Plan`, `ServiceMode=winsw`, `WinSwSource=true`, `WinSwMissing=null`, `InstallDryRun=true`, `InstallOk=true`, `UninstallDryRun=true`, `UninstallOk=true`.
- `git diff --check`: exit 0. 출력은 CRLF 변환 warning만 있었고 whitespace error는 없었다.

관리자 smoke 상태:

- 2026-04-27 관리자 smoke에서 실제 WinSW executable `C:\Users\Operator\AppData\Local\Temp\WinSW-x64-v2.12.0.exe`를 사용했다. SHA256은 `05b82d46ad331cc16bdc00de5c6332c1ef818df8ceefcd49c726553209b3a0da`였다.
- 사전 확인에서 `PureCVisorDesktopNode` 서비스는 존재하지 않았고, `127.0.0.1:7777` listener도 없었다.
- `Plan -WinSwPath`는 `service.mode = winsw`, WinSW XML/staged executable 경로, source hash를 포함한 JSON을 반환했다.
- 1차 관리자 smoke에서 `Install -WinSwPath` product action health check가 bearer token 없이 `/api/v1/runtime/policy`를 호출해 `401 Unauthorized`가 발생했다. 이를 token-file bearer health check RED/GREEN으로 보강했다.
- 1차 기본 `Uninstall`은 service stop/uninstall 이후 WinSW executable lock 때문에 product root 제거가 실패했다. 이를 WinSW `status` stop wait와 remove retry RED/GREEN으로 보강했다.
- 기본 install/status/API/root/diagnostics/uninstall smoke 재실행 결과: `Install -WinSwPath` exit 0, health auth `bearer-token-file`, health HTTP 200, WinSW status `Started`, token 포함 runtime policy HTTP 200, loopback root HTTP 200, `CollectDiagnostics` bundle `C:\ProgramData\PureCVisor\desktop-node\diagnostics\bundle-20260427-125254-a25e79ea` 생성, 기본 `Uninstall` exit 0.
- 기본 `Uninstall`은 `service.stop.wait` 16회 polling 후 `Stopped`를 확인하고 service uninstall/product root removal을 완료했다. 최종 확인에서 service query는 1060, port `7777` listener는 0, product root는 제거, data root는 보존 상태였다.
- `Rollback` smoke 결과: previous product root를 준비한 뒤 `Rollback` exit 0, `service.stop.wait` 16회 polling 후 restore/start/health가 성공했다. Rollback 후 runtime policy HTTP 200을 확인했고, cleanup uninstall 후 service query 1060, port `7777` listener 0, product/current/previous/failed root 모두 제거 상태였다.
- `Uninstall -RemoveData` smoke 1차 실행에서 Phase 8 token file ACL hardening 때문에 token file 삭제가 `Access denied`로 실패했다. 이를 `RemoveData` token ACL repair RED/GREEN으로 보강했다.
- `Uninstall -RemoveData` smoke 재실행 결과: install health auth `bearer-token-file`, health HTTP 200, runtime policy HTTP 200, loopback root HTTP 200, uninstall exit 0, `service.stop.wait` 16회, token ACL repair 수행, remove target 6개 처리. 최종 확인에서 service query는 1060, port `7777` listener는 0, product root/token/job/event/install/diagnostics는 제거됐고 data root와 WinSW service log directory는 보존됐다.
- Hyper-V lifecycle integration 결과: `host.status`, `vm.create`, `vm.list`, `vm.start`, `checkpoint.create`, `vm.poweroff` runner path가 성공했고, test cleanup이 임시 `pcv-spike-*` VM과 VM directory를 정리했다.
