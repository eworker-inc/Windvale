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
$NativeStencilModule = Join-Path $OutputRoot 'Native-Stencil-Core.wvb'
$NativeStencilDemoModule = Join-Path $OutputRoot 'Native-Stencil-Demo.wvb'
$NativeStencilBridgeModule = Join-Path $OutputRoot 'Native-Stencil-Bridge.wvb'
$NativeUtf8CoreModule = Join-Path $OutputRoot 'Native-X64-Utf8-Service.wvb'
$NativeUtf8BridgeModule = Join-Path $OutputRoot 'Native-X64-Utf8-Service-Bridge.wvb'
$NativeIntegerFormatCoreModule = Join-Path $OutputRoot 'Native-X64-Integer-Format-Services.wvb'
$NativeIntegerFormatBridgeModule = Join-Path $OutputRoot 'Native-X64-Integer-Format-Services-Bridge.wvb'
$NativeServiceCodeBuilderModule = Join-Path $OutputRoot 'Native-X64-Service-Code-Builder.wvb'
$NativeWindowsOutputCoreModule = Join-Path $OutputRoot 'Native-X64-Output-Service-Windows.wvb'
$NativeLinuxOutputCoreModule = Join-Path $OutputRoot 'Native-X64-Output-Service-Linux.wvb'
$NativeOutputBridgeModule = Join-Path $OutputRoot 'Native-X64-Output-Services-Bridge.wvb'
$NativeFileOutputCodeModule = Join-Path $OutputRoot 'Native-X64-File-Output-Service-Code.wvb'
$NativeWindowsFileOutputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Output-Service-Windows.wvb'
$NativeLinuxFileOutputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Output-Service-Linux.wvb'
$NativeFileOutputBridgeModule = Join-Path $OutputRoot 'Native-X64-File-Output-Services-Bridge.wvb'
$NativeFileInputCodeModule = Join-Path $OutputRoot 'Native-X64-File-Input-Service-Code.wvb'
$NativeWindowsFileInputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Input-Service-Windows.wvb'
$NativeLinuxFileInputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Input-Service-Linux.wvb'
$NativeFileInputBridgeModule = Join-Path $OutputRoot 'Native-X64-File-Input-Services-Bridge.wvb'

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

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Stencil-Core.wvproj') `
    $NativeStencilModule `
    21296 `
    '6df3c524d0f9bec79cd2516a758985c487cc237c6f94bc5b80e015975d50cca3' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=16427 module-bytes=21296'
Invoke-ExactInspect `
    $NativeStencilModule `
    @('Native\\u02C9stencil\\u02C9result', 'Native\\u02C9stencil\\u02C9patch\\u02C9kind', 'Native\\u02C9stencil\\u02C9process\\u02C9argument\\u02C9count', 'Native\\u02C9stencil\\u02C9process\\u02C9argument', 'section name=exports .* count=20')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Native-Stencil-Demo.wvproj') `
    $NativeStencilDemoModule `
    25683 `
    '6b27fbd10d5f06855354f433ec0b8c9b1af1761ef04458817931e675c26e0da8' `
    'build status=Published verification=compiler-aligned functions=24 code-bytes=21063 module-bytes=25683'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Stencil-Bridge.wvproj') `
    $NativeStencilBridgeModule `
    20800 `
    '0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da' `
    'build status=Published verification=compiler-aligned functions=21 code-bytes=16833 module-bytes=20800'
Invoke-ExactInspect $NativeStencilBridgeModule @('name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Utf8-Service-Core.wvproj') `
    $NativeUtf8CoreModule `
    11577 `
    'adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=9098 module-bytes=11577'
Invoke-ExactInspect $NativeUtf8CoreModule @('profile=portable', 'Native\\u02C9x64\\u02C9utf8\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Utf8-Service.wvproj') `
    $NativeUtf8BridgeModule `
    11511 `
    '4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f' `
    'build status=Published verification=compiler-aligned functions=19 code-bytes=9114 module-bytes=11511'
Invoke-ExactInspect $NativeUtf8BridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Integer-Format-Services-Core.wvproj') `
    $NativeIntegerFormatCoreModule `
    11611 `
    '6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2' `
    'build status=Published verification=compiler-aligned functions=11 code-bytes=9588 module-bytes=11611'
Invoke-ExactInspect $NativeIntegerFormatCoreModule @('profile=portable', 'Native\\u02C9x64\\u02C9integer\\u02C9format\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Integer-Format-Services.wvproj') `
    $NativeIntegerFormatBridgeModule `
    11598 `
    '851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9' `
    'build status=Published verification=compiler-aligned functions=12 code-bytes=9654 module-bytes=11598'
Invoke-ExactInspect $NativeIntegerFormatBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Service-Code-Builder.wvproj') `
    $NativeServiceCodeBuilderModule `
    4135 `
    'adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06' `
    'build status=Published verification=compiler-aligned functions=12 code-bytes=2440 module-bytes=4135'
Invoke-ExactInspect `
    $NativeServiceCodeBuilderModule `
    @('profile=portable', 'Native\\u02C9x64\\u02C9service\\u02C9builder', 'Native\\u02C9x64\\u02C9service\\u02C9finish', 'section name=exports .* count=10')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Service-Windows.wvproj') `
    $NativeWindowsOutputCoreModule `
    9435 `
    'a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983' `
    'build status=Published verification=compiler-aligned functions=15 code-bytes=7347 module-bytes=9435'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Service-Linux.wvproj') `
    $NativeLinuxOutputCoreModule `
    8908 `
    'd3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad' `
    'build status=Published verification=compiler-aligned functions=14 code-bytes=6941 module-bytes=8908'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Services.wvproj') `
    $NativeOutputBridgeModule `
    14930 `
    '209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=12050 module-bytes=14930'
Invoke-ExactInspect $NativeOutputBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Code.wvproj') `
    $NativeFileOutputCodeModule `
    6576 `
    '7ed9baf3a21912933045b99cb82d22d73620a318a716931db86670e5ea2212c6' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=4463 module-bytes=6576'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Linux.wvproj') `
    $NativeLinuxFileOutputCoreModule `
    18658 `
    '834d0c45b85b26ffd3ee43e49a85c8c4ffa08f36581c02785729b276eeccdb48' `
    'build status=Published verification=compiler-aligned functions=21 code-bytes=14933 module-bytes=18658'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Windows.wvproj') `
    $NativeWindowsFileOutputCoreModule `
    21129 `
    '9ca03bf6f5b8678389c81e281438160ff4c96c86f11a048aba90238fdc81a45d' `
    'build status=Published verification=compiler-aligned functions=22 code-bytes=16956 module-bytes=21129'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Services.wvproj') `
    $NativeFileOutputBridgeModule `
    33437 `
    '441db0e0e5a90f98c7e4b12b17086f56487e7d754d7b6378a0eb2972591e64f6' `
    'build status=Published verification=compiler-aligned functions=26 code-bytes=27468 module-bytes=33437'
Invoke-ExactInspect $NativeFileOutputBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Code.wvproj') `
    $NativeFileInputCodeModule `
    7869 `
    'e2bfd4521b8f22529f3747eef196bdf7fa7aa0e97644db23ed45939aa10a1a7a' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=5317 module-bytes=7869'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Linux.wvproj') `
    $NativeLinuxFileInputCoreModule `
    26718 `
    '04533e8ecade1f29e0b706c75ec949f5b4c300074cfd65feacb86f5107dcaeba' `
    'build status=Published verification=compiler-aligned functions=26 code-bytes=21582 module-bytes=26718'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Windows.wvproj') `
    $NativeWindowsFileInputCoreModule `
    32085 `
    '6155c4ebb8f4ea76a5d1f22c1bb788aec51e731ceb4a1c5a4ceb7551ba8f409a' `
    'build status=Published verification=compiler-aligned functions=28 code-bytes=25972 module-bytes=32085'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Services.wvproj') `
    $NativeFileInputBridgeModule `
    51341 `
    '09f73787a909ae35ebc1aefb05bd88e4282ff8db7152d196f83b2798ea7c2234' `
    'build status=Published verification=compiler-aligned functions=35 code-bytes=42279 module-bytes=51341'
Invoke-ExactInspect $NativeFileInputBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

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
Write-Output 'native Seed front-door verification status=Complete artifacts=31 cases=53'
