[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$OutputRoot = (Resolve-Path -LiteralPath $OutputDirectory).Path
if (!(Test-Path -LiteralPath $OutputRoot -PathType Container)) {
    throw 'The native Seed front-door output directory must already exist.'
}

$NativeBuild = Join-Path $RepositoryRoot 'Tools/Native/Build-Wvb.cmd'
$NativeVerify = Join-Path $RepositoryRoot 'Tools/Native/Verify-Wvb.cmd'
$NativeInspect = Join-Path $RepositoryRoot 'Tools/Native/Inspect-Wvb.cmd'
$NativeRun = Join-Path $RepositoryRoot 'Tools/Native/Run-Wvb.cmd'

function Invoke-ExactBuild(
    [string]$ProjectPath,
    [string]$OutputPath,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256,
    [string]$ExpectedBuildReport
) {
    $BuildOutput = @(& $NativeBuild $ProjectPath $OutputPath 2>&1)
    $BuildExit = $LASTEXITCODE
    $ExpectedPublicationReport =
        "publication status=Complete bytes=0x$($ExpectedBytes.ToString('x8')) sha256=$ExpectedSha256"
    if (
        $BuildExit -ne 0 -or
        $BuildOutput.Count -ne 2 -or
        $BuildOutput[0] -ne $ExpectedBuildReport -or
        $BuildOutput[1] -ne $ExpectedPublicationReport
    ) {
        throw "The native Seed project build failed: $ProjectPath"
    }

    $Information = Get-Item -LiteralPath $OutputPath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $OutputPath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed project build produced an unexpected module: $OutputPath"
    }
}

function Invoke-ExactVerify([string]$ModulePath) {
    $VerifyOutput = @(& $NativeVerify $ModulePath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $VerifyOutput.Count -ne 1 -or
        $VerifyOutput[0] -ne 'wvb status=Valid profile=compiler-aligned'
    ) {
        throw "The native Seed verifier rejected or misreported: $ModulePath"
    }
}

function Invoke-ExactInspect(
    [string]$ModulePath,
    [string[]]$RequiredPatterns
) {
    $InspectOutput = @(& $NativeInspect $ModulePath 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "The native Seed inspector omitted required evidence: $ModulePath"
    }
    $Inspection = $InspectOutput -join "`n"
    foreach ($RequiredPattern in $RequiredPatterns) {
        if ($Inspection -notmatch $RequiredPattern) {
            throw "The native Seed inspector omitted required evidence: $ModulePath"
        }
    }
}

function Invoke-ExactRun(
    [string]$ModulePath,
    [int]$ExpectedResult,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256
) {
    $RunOutput = @(& $NativeRun $ModulePath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $RunOutput.Count -ne 1 -or
        $RunOutput[0].ToString() -ne "Result: $ExpectedResult"
    ) {
        throw "The native Seed runner rejected or misreported: $ModulePath"
    }
    $Information = Get-Item -LiteralPath $ModulePath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $ModulePath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed runner modified its input module: $ModulePath"
    }
}

function Invoke-ExactInstructionReport(
    [string]$ModulePath,
    [int]$ExpectedResult,
    [int]$ExpectedInstructions,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256
) {
    $RunOutput = @(& $NativeRun $ModulePath --report-steps 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $RunOutput.Count -ne 2 -or
        $RunOutput[0].ToString() -ne "Result: $ExpectedResult" -or
        $RunOutput[1].ToString() -ne "Instructions: $ExpectedInstructions"
    ) {
        throw "The native Seed runner instruction report is invalid: $ModulePath"
    }
    $Information = Get-Item -LiteralPath $ModulePath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $ModulePath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed runner modified its reported input module: $ModulePath"
    }
}

$SumModule = Join-Path $OutputRoot 'Sum-Data.wvb'
$HelloModule = Join-Path $OutputRoot 'Hello-Windvale.wvb'
$FoundationModule = Join-Path $OutputRoot 'Read-Wvb-Header.wvb'
$CompositionModule = Join-Path $OutputRoot 'Module-Composition-Demo-Project.wvb'
$MachineContractsModule = Join-Path $OutputRoot 'Machine-Contracts.wvb'
$MachineContractsDemoModule = Join-Path $OutputRoot 'Machine-Contracts-Demo.wvb'
$ByteOrderingModule = Join-Path $OutputRoot 'Byte-Ordering.wvb'
$ByteOrderingDemoModule = Join-Path $OutputRoot 'Byte-Ordering-Demo.wvb'
$DecimalParsingModule = Join-Path $OutputRoot 'Decimal-Parsing.wvb'
$DecimalParsingDemoModule = Join-Path $OutputRoot 'Decimal-Parsing-Demo.wvb'
$ByteConstructionModule = Join-Path $OutputRoot 'Byte-Construction.wvb'
$ByteConstructionDemoModule = Join-Path $OutputRoot 'Byte-Construction-Demo.wvb'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wvproj') `
    $SumModule `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=270 module-bytes=494'
Invoke-ExactVerify $SumModule
Invoke-ExactInspect $SumModule 'opcode=data\.load\.i32 operand=0'
Invoke-ExactRun `
    $SumModule `
    29 `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df'
Invoke-ExactInstructionReport `
    $SumModule `
    29 `
    203 `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Seed/Hello-Windvale.wvproj') `
    $HelloModule `
    253 `
    '0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=36 module-bytes=253'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Foundation/Read-Wvb-Header.wvproj') `
    $FoundationModule `
    1701 `
    'c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=1379 module-bytes=1701'
Invoke-ExactVerify $FoundationModule
Invoke-ExactInspect $FoundationModule 'opcode=bytes\.read_u32_little(?:\r?$|\s)'
Invoke-ExactRun `
    $FoundationModule `
    1 `
    1701 `
    'c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Demo.wvproj') `
    $CompositionModule `
    660 `
    '030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607' `
    'build status=Published verification=compiler-aligned functions=4 code-bytes=280 module-bytes=660'
Invoke-ExactRun `
    $CompositionModule `
    42 `
    660 `
    '030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Machine-Contracts.wvproj') `
    $MachineContractsModule `
    2466 `
    'f624739461dea01862121daf234b3a838dfcafd73753e3124a038b7efa8b4fa3' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2019 module-bytes=2466'
Invoke-ExactInspect `
    $MachineContractsModule `
    @('Foundation\\u02C9alignment\\u02C9is\\u02C9valid', 'Foundation\\u02C9machine\\u02C9name\\u02C9is\\u02C9valid', 'section name=exports .* count=2')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation-Machine-Contracts-Demo.wvproj') `
    $MachineContractsDemoModule `
    3487 `
    '69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3' `
    'build status=Published verification=compiler-aligned functions=3 code-bytes=2899 module-bytes=3487'
Invoke-ExactRun `
    $MachineContractsDemoModule `
    0 `
    3487 `
    '69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Byte-Ordering.wvproj') `
    $ByteOrderingModule `
    990 `
    '27a3c24b5cc358a4f67e2e1959b5e80559918f0176c52e08648e638212e6dece' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=720 module-bytes=990'
Invoke-ExactInspect `
    $ByteOrderingModule `
    @('Foundation\\u02C9byte\\u02C9spans\\u02C9compare', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation-Byte-Ordering-Demo.wvproj') `
    $ByteOrderingDemoModule `
    2422 `
    'fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2059 module-bytes=2422'
Invoke-ExactRun `
    $ByteOrderingDemoModule `
    0 `
    2422 `
    'fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Decimal-Parsing.wvproj') `
    $DecimalParsingModule `
    1698 `
    'bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=1301 module-bytes=1698'
Invoke-ExactInspect `
    $DecimalParsingModule `
    @('Foundation\\u02C9u32\\u02C9parse', 'Foundation\\u02C9u32\\u02C9decimal\\u02C9parse', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation-Decimal-Parsing-Demo.wvproj') `
    $DecimalParsingDemoModule `
    3742 `
    'd323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2969 module-bytes=3742'
Invoke-ExactRun `
    $DecimalParsingDemoModule `
    0 `
    3742 `
    'd323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Byte-Construction.wvproj') `
    $ByteConstructionModule `
    2001 `
    '3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=1503 module-bytes=2001'
Invoke-ExactInspect `
    $ByteConstructionModule `
    @('Foundation\\u02C9bytes\\u02C9result', 'Foundation\\u02C9bytes\\u02C9repeat', 'Foundation\\u02C9bytes\\u02C9replace', 'section name=exports .* count=2')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation-Byte-Construction-Demo.wvproj') `
    $ByteConstructionDemoModule `
    5017 `
    'ab594976ced7a84573ade0aa50fb4370d96b8004c8b9a5ec1e888968c7b3bf8f' `
    'build status=Published verification=compiler-aligned functions=3 code-bytes=4194 module-bytes=5017'

$TemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$TemporaryDirectory = Join-Path `
    $TemporaryRoot `
    "windvale-seed-front-door-$PID-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $TemporaryDirectory | Out-Null
try {
    $InvalidProject = Join-Path $TemporaryDirectory 'Invalid.wvproj'
    $ExistingOutput = Join-Path $TemporaryDirectory 'Existing.wvb'
    [IO.File]::WriteAllText(
        $InvalidProject,
        "windvale-project 1`nroot `"Missing.wv`"`n",
        [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllBytes($ExistingOutput, [byte[]](9, 8, 7))
    $InvalidOutput = @(& $NativeBuild $InvalidProject $ExistingOutput 2>&1)
    if (
        $LASTEXITCODE -ne 1 -or
        $InvalidOutput.Count -ne 1 -or
        $InvalidOutput[0].ToString() -ne 'build status=Projectˉrejected code=WVP1004 line=3 column=1' -or
        [Convert]::ToHexString([IO.File]::ReadAllBytes($ExistingOutput)) -ne '090807'
    ) {
        throw 'The native Seed project rejection or output preservation contract failed.'
    }
} finally {
    $ResolvedTemporary = [IO.Path]::GetFullPath($TemporaryDirectory)
    if (!$ResolvedTemporary.StartsWith($TemporaryRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove an unexpected native Seed temporary directory.'
    }
    Remove-Item -LiteralPath $ResolvedTemporary -Recurse -Force
}

$global:LASTEXITCODE = 0
Write-Output 'native Seed front-door verification status=Complete artifacts=12 cases=24'
