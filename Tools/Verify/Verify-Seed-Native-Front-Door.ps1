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
$NativeSourceCompilerBuild = Join-Path $RepositoryRoot 'Tools/Native/Build-Source-Compiler-Product.cmd'
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

function Invoke-ExactSourceCompilerBuild(
    [string]$Product,
    [string]$OutputPath,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256,
    [string]$ExpectedCompilerReport
) {
    $BuildOutput = @(& $NativeSourceCompilerBuild $Product $OutputPath 2>&1)
    $BuildExit = $LASTEXITCODE
    $ExpectedPublicationReport =
        "publication status=Complete bytes=0x$($ExpectedBytes.ToString('x8')) sha256=$ExpectedSha256"
    if (
        $BuildExit -ne 0 -or
        $BuildOutput.Count -ne 2 -or
        $BuildOutput[0] -ne $ExpectedCompilerReport -or
        $BuildOutput[1] -ne $ExpectedPublicationReport
    ) {
        throw "The native source compiler $Product product did not reproduce its exact report."
    }

    $Information = Get-Item -LiteralPath $OutputPath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $OutputPath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native source compiler $Product product has an unexpected identity."
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
$NativeTextConcatCoreModule = Join-Path $OutputRoot 'Native-X64-Text-Concat-Service.wvb'
$NativeTextConcatBridgeModule = Join-Path $OutputRoot 'Native-X64-Text-Concat-Service-Bridge.wvb'
$NativeTextQuoteCoreModule = Join-Path $OutputRoot 'Native-X64-Text-Quote-Service.wvb'
$NativeTextQuoteBridgeModule = Join-Path $OutputRoot 'Native-X64-Text-Quote-Service-Bridge.wvb'
$NativeEnumNameCoreModule = Join-Path $OutputRoot 'Native-X64-Enum-Name-Service.wvb'
$NativeEnumNameBridgeModule = Join-Path $OutputRoot 'Native-X64-Enum-Name-Service-Bridge.wvb'
$NativeEnumMetadataCoreModule = Join-Path $OutputRoot 'Native-Enum-Metadata-Core.wvb'
$NativeEnumMetadataBridgeModule = Join-Path $OutputRoot 'Native-Enum-Metadata-Bridge.wvb'
$NativePublicationModule = Join-Path $OutputRoot 'Native-Publication-Core.wvb'
$NativePublicationBridgeModule = Join-Path $OutputRoot 'Native-Publication-Bridge.wvb'
$NativeServiceBundleMaterializationCoreModule = Join-Path $OutputRoot 'Native-Service-Bundle-Materialization-Core.wvb'
$NativeServiceBundleMaterializationBridgeModule = Join-Path $OutputRoot 'Native-Service-Bundle-Materialization-Bridge.wvb'
$NativeOutputTableCoreModule = Join-Path $OutputRoot 'Native-Output-Table-Core.wvb'
$NativeOutputTableBridgeModule = Join-Path $OutputRoot 'Native-Output-Table-Bridge.wvb'
$NativeFileOutputTableCoreModule = Join-Path $OutputRoot 'Native-File-Output-Table-Core.wvb'
$NativeFileOutputTableBridgeModule = Join-Path $OutputRoot 'Native-File-Output-Table-Bridge.wvb'
$NativeFileInputTableCoreModule = Join-Path $OutputRoot 'Native-File-Input-Table-Core.wvb'
$NativeFileInputTableBridgeModule = Join-Path $OutputRoot 'Native-File-Input-Table-Bridge.wvb'
$NativeServiceTableCoreModule = Join-Path $OutputRoot 'Native-Service-Table-Core.wvb'
$NativeServiceTableBridgeModule = Join-Path $OutputRoot 'Native-Service-Table-Bridge.wvb'
$NativeExecutionContextCoreModule = Join-Path $OutputRoot 'Native-Execution-Context-Core.wvb'
$NativeExecutionContextBridgeModule = Join-Path $OutputRoot 'Native-Execution-Context-Bridge.wvb'
$NativeArgumentTableCoreModule = Join-Path $OutputRoot 'Native-Argument-Table-Core.wvb'
$NativeArgumentTableBridgeModule = Join-Path $OutputRoot 'Native-Argument-Table-Bridge.wvb'
$NativeEntryBridgeCoreModule = Join-Path $OutputRoot 'Native-Entry-Bridge-Core.wvb'
$NativeEntryBridgeBridgeModule = Join-Path $OutputRoot 'Native-Entry-Bridge-Bridge.wvb'
$NativeByteResultAdmissionCoreModule = Join-Path $OutputRoot 'Native-Byte-Result-Admission-Core.wvb'
$NativeByteResultAdmissionBridgeModule = Join-Path $OutputRoot 'Native-Byte-Result-Admission-Bridge.wvb'
$NativeHostedToolMetadataAdmissionModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Metadata-Admission.wvb'
$NativeHostedToolMetadataConstructionCoreModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Metadata-Construction-Core.wvb'
$NativeHostedToolMetadataConstructionBridgeModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Metadata-Construction-Bridge.wvb'
$NativeHostedStartupInstantiationModule = Join-Path $OutputRoot 'Native-Hosted-Startup-Instantiation.wvb'
$NativeHostedContainerPlanModule = Join-Path $OutputRoot 'Native-Hosted-Container-Construction.wvb'
$NativeHostedContainerWindowsModule = Join-Path $OutputRoot 'Native-Hosted-Container-Windows.wvb'
$NativeHostedContainerLinuxModule = Join-Path $OutputRoot 'Native-Hosted-Container-Linux.wvb'
$NativeHostedContainerSegmentationModule = Join-Path $OutputRoot 'Native-Hosted-Container-Segmentation.wvb'
$NativeHostedToolRuntimeHeaderCoreModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Runtime-Header-Core.wvb'
$NativeHostedToolRuntimeHeaderBridgeModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Runtime-Header-Bridge.wvb'
$NativePublicationLifetimeCoreModule = Join-Path $OutputRoot 'Native-Publication-Lifetime-Core.wvb'
$NativePublicationLifetimeBridgeModule = Join-Path $OutputRoot 'Native-Publication-Lifetime-Bridge.wvb'
$SourceLexerModule = Join-Path $OutputRoot 'Source-Lexer-Core.wvb'
$SourceLexerDemoModule = Join-Path $OutputRoot 'Source-Lexer-Demo.wvb'
$SourceDeclarationParserModule = Join-Path $OutputRoot 'Source-Declaration-Parser.wvb'
$SourceDeclarationParserDemoModule = Join-Path $OutputRoot 'Source-Declaration-Parser-Demo.wvb'
$SourceDeclarationParserToolModule = Join-Path $OutputRoot 'Source-Declaration-Parser-Tool.wvb'
$SourceBodyParserModule = Join-Path $OutputRoot 'Source-Body-Parser.wvb'
$SourceBodyParserDemoModule = Join-Path $OutputRoot 'Source-Body-Parser-Demo.wvb'
$SourceBodyParserToolModule = Join-Path $OutputRoot 'Source-Body-Parser-Tool.wvb'
$SourceSetModule = Join-Path $OutputRoot 'Source-Set-Core.wvb'
$SourceSetDemoModule = Join-Path $OutputRoot 'Source-Set-Demo.wvb'
$SourceSetToolModule = Join-Path $OutputRoot 'Source-Set-Tool.wvb'
$SourceGraphModule = Join-Path $OutputRoot 'Source-Graph-Core.wvb'
$SourceGraphDemoModule = Join-Path $OutputRoot 'Source-Graph-Demo.wvb'
$SourceGraphToolModule = Join-Path $OutputRoot 'Source-Graph-Tool.wvb'
$SourceSymbolsModule = Join-Path $OutputRoot 'Source-Symbols-Core.wvb'
$SourceSymbolsDemoModule = Join-Path $OutputRoot 'Source-Symbols-Demo.wvb'
$SourceSymbolsToolModule = Join-Path $OutputRoot 'Source-Symbols-Tool.wvb'
$SourceBindingsModule = Join-Path $OutputRoot 'Source-Bindings-Core.wvb'
$SourceBindingsDemoModule = Join-Path $OutputRoot 'Source-Bindings-Demo.wvb'
$SourceBindingsToolModule = Join-Path $OutputRoot 'Source-Bindings-Tool.wvb'
$SourceWirModule = Join-Path $OutputRoot 'Source-Wir-Core.wvb'
$SourceWirDemoModule = Join-Path $OutputRoot 'Source-Wir-Demo.wvb'
$SourceWirToolModule = Join-Path $OutputRoot 'Source-Wir-Tool.wvb'
$SourceWvbModule = Join-Path $OutputRoot 'Source-Wvb-Core.wvb'
$SourceWvbDemoModule = Join-Path $OutputRoot 'Source-Wvb-Demo.wvb'
$SourceWvbToolModule = Join-Path $OutputRoot 'Source-Wvb-Tool.wvb'

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

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Concat-Service-Core.wvproj') `
    $NativeTextConcatCoreModule `
    10253 `
    '6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73' `
    'build status=Published verification=compiler-aligned functions=14 code-bytes=8082 module-bytes=10253'
Invoke-ExactInspect $NativeTextConcatCoreModule @('profile=portable', 'Native\\u02C9x64\\u02C9text\\u02C9concat\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Concat-Service.wvproj') `
    $NativeTextConcatBridgeModule `
    10232 `
    '87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08' `
    'build status=Published verification=compiler-aligned functions=15 code-bytes=8098 module-bytes=10232'
Invoke-ExactInspect $NativeTextConcatBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Quote-Service-Core.wvproj') `
    $NativeTextQuoteCoreModule `
    1471 `
    'b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=16 module-bytes=1471'
Invoke-ExactInspect $NativeTextQuoteCoreModule @('profile=portable', 'data index=0 name="Native\\u02C9x64\\u02C9text\\u02C9quote\\u02C9leaf" type=bytes bytes=1165', 'Native\\u02C9x64\\u02C9text\\u02C9quote\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Quote-Service.wvproj') `
    $NativeTextQuoteBridgeModule `
    1435 `
    '306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=32 module-bytes=1435'
Invoke-ExactInspect $NativeTextQuoteBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Enum-Name-Service-Core.wvproj') `
    $NativeEnumNameCoreModule `
    625 `
    'b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=16 module-bytes=625'
Invoke-ExactInspect $NativeEnumNameCoreModule @('profile=portable', 'data index=0 name="Native\\u02C9x64\\u02C9enum\\u02C9name\\u02C9leaf" type=bytes bytes=323', 'Native\\u02C9x64\\u02C9enum\\u02C9name\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Enum-Name-Service.wvproj') `
    $NativeEnumNameBridgeModule `
    592 `
    '46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=32 module-bytes=592'
Invoke-ExactInspect $NativeEnumNameBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Enum-Metadata-Core.wvproj') `
    $NativeEnumMetadataCoreModule `
    15414 `
    '8f22e1ba56985fc5a330fcb73cda84456ecc3ef51f9ddffd6bc2edd740f73659' `
    'build status=Published verification=compiler-aligned functions=17 code-bytes=13480 module-bytes=15414'
Invoke-ExactInspect $NativeEnumMetadataCoreModule @('profile=portable', 'Native\\u02C9enum\\u02C9metadata\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Enum-Metadata.wvproj') `
    $NativeEnumMetadataBridgeModule `
    15292 `
    '052be4402df26ed542107d666ed894cadb04a46ba6b2428bafc9f1879e38a072' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=13511 module-bytes=15292'
Invoke-ExactInspect $NativeEnumMetadataBridgeModule @('profile=portable', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Core.wvproj') `
    $NativePublicationModule `
    7190 `
    '3048902ce708d6e640d484507efc1d567399bcafed6e2c133ca2827aff83189f' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=5333 module-bytes=7190'
Invoke-ExactInspect $NativePublicationModule @('profile=portable', 'Native\\u02C9publication\\u02C9result', 'Native\\u02C9publication\\u02C9status', 'Native\\u02C9publication\\u02C9plan', 'section name=exports .* count=8')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication.wvproj') `
    $NativePublicationBridgeModule `
    6758 `
    '111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c' `
    'build status=Published verification=compiler-aligned functions=9 code-bytes=5399 module-bytes=6758'
Invoke-ExactInspect $NativePublicationBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Service-Bundle-Materialization-Core.wvproj') `
    $NativeServiceBundleMaterializationCoreModule `
    17185 `
    '97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008' `
    'build status=Published verification=compiler-aligned functions=19 code-bytes=14253 module-bytes=17185'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Service-Bundle-Materialization.wvproj') `
    $NativeServiceBundleMaterializationBridgeModule `
    17150 `
    '327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=14319 module-bytes=17150'
Invoke-ExactInspect $NativeServiceBundleMaterializationBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Output-Table-Core.wvproj') `
    $NativeOutputTableCoreModule `
    4710 `
    'ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4002 module-bytes=4710'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Output-Table.wvproj') `
    $NativeOutputTableBridgeModule `
    4714 `
    'b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=4033 module-bytes=4714'
Invoke-ExactInspect $NativeOutputTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Output-Table-Core.wvproj') `
    $NativeFileOutputTableCoreModule `
    3926 `
    'fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=3293 module-bytes=3926'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Output-Table.wvproj') `
    $NativeFileOutputTableBridgeModule `
    3930 `
    '94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3324 module-bytes=3930'
Invoke-ExactInspect $NativeFileOutputTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Input-Table-Core.wvproj') `
    $NativeFileInputTableCoreModule `
    5078 `
    '0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=4381 module-bytes=5078'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Input-Table.wvproj') `
    $NativeFileInputTableBridgeModule `
    5084 `
    'e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4412 module-bytes=5084'
Invoke-ExactInspect $NativeFileInputTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Table-Core.wvproj') `
    $NativeServiceTableCoreModule `
    3065 `
    'ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=2492 module-bytes=3065'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Table.wvproj') `
    $NativeServiceTableBridgeModule `
    3079 `
    '04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=2523 module-bytes=3079'
Invoke-ExactInspect $NativeServiceTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Execution-Context-Core.wvproj') `
    $NativeExecutionContextCoreModule `
    5530 `
    'dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4767 module-bytes=5530'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Execution-Context.wvproj') `
    $NativeExecutionContextBridgeModule `
    5531 `
    '86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=4798 module-bytes=5531'
Invoke-ExactInspect $NativeExecutionContextBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Argument-Table-Core.wvproj') `
    $NativeArgumentTableCoreModule `
    4362 `
    '08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=3707 module-bytes=4362'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Argument-Table.wvproj') `
    $NativeArgumentTableBridgeModule `
    4374 `
    '080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3738 module-bytes=4374'
Invoke-ExactInspect $NativeArgumentTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Entry-Bridge-Core.wvproj') `
    $NativeEntryBridgeCoreModule `
    3385 `
    '8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=2799 module-bytes=3385'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Entry-Bridge.wvproj') `
    $NativeEntryBridgeBridgeModule `
    3401 `
    'd66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=2830 module-bytes=3401'
Invoke-ExactInspect $NativeEntryBridgeBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Byte-Result-Admission-Core.wvproj') `
    $NativeByteResultAdmissionCoreModule `
    7078 `
    'eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04' `
    'build status=Published verification=compiler-aligned functions=10 code-bytes=6085 module-bytes=7078'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Byte-Result-Admission.wvproj') `
    $NativeByteResultAdmissionBridgeModule `
    7057 `
    '9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf' `
    'build status=Published verification=compiler-aligned functions=11 code-bytes=6116 module-bytes=7057'
Invoke-ExactInspect $NativeByteResultAdmissionBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wvproj') `
    $NativeHostedToolMetadataAdmissionModule `
    10872 `
    'd7b0084ed2c69ee03ad65ee4bfffa72550fd8d9ef2889efa0be116350b80b8b5' `
    'build status=Published verification=compiler-aligned functions=13 code-bytes=9503 module-bytes=10872'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Tool-Metadata-Construction-Core.wvproj') `
    $NativeHostedToolMetadataConstructionCoreModule `
    24360 `
    '5808f778eb21c1214b581f0ce03958a74173a801b886aec7ed32124d7446abcd' `
    'build status=Published verification=compiler-aligned functions=35 code-bytes=21363 module-bytes=24360'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Tool-Metadata.wvproj') `
    $NativeHostedToolMetadataConstructionBridgeModule `
    24252 `
    'b5e9397326d3106b22ce735369ef8202ff6bb4c8e14f6069a0c467b4266c8208' `
    'build status=Published verification=compiler-aligned functions=36 code-bytes=21394 module-bytes=24252'
Invoke-ExactInspect $NativeHostedToolMetadataConstructionBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Linker/Windvale/Native-Hosted-Startup-Instantiation.wvproj') `
    $NativeHostedStartupInstantiationModule `
    21143 `
    '933864be78b28394b9fc8e495b5ac872311ebca2a624db6e6731cdb8b399d309' `
    'build status=Published verification=compiler-aligned functions=15 code-bytes=18808 module-bytes=21143'
Invoke-ExactInspect $NativeHostedStartupInstantiationModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Container-Construction.wvproj') `
    $NativeHostedContainerPlanModule `
    35929 `
    'ff1b48cfc05baab5f707dcfce7e73b0714e2379ee594e12f6e9c6ea1589fef7e' `
    'build status=Published verification=compiler-aligned functions=41 code-bytes=31210 module-bytes=35929'
Invoke-ExactInspect $NativeHostedContainerPlanModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Container-Windows.wvproj') `
    $NativeHostedContainerWindowsModule `
    17679 `
    'a77e4ea3ac2cff35e965ae44cd486f30dd5b0c10aa2cde23c109d0eca37bffcb' `
    'build status=Published verification=compiler-aligned functions=22 code-bytes=15041 module-bytes=17679'
Invoke-ExactInspect $NativeHostedContainerWindowsModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Container-Linux.wvproj') `
    $NativeHostedContainerLinuxModule `
    12328 `
    'dac93155c68ba18f6cbe3af2d301a4c4171b9a9c05841057ea57398536fa8b42' `
    'build status=Published verification=compiler-aligned functions=19 code-bytes=10674 module-bytes=12328'
Invoke-ExactInspect $NativeHostedContainerLinuxModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Container-Segmentation.wvproj') `
    $NativeHostedContainerSegmentationModule `
    22584 `
    'd6d74f7d27df9f04f02b8eac2e75fde4fc230ba70d198f90b31ad668a06052e6' `
    'build status=Published verification=compiler-aligned functions=28 code-bytes=19181 module-bytes=22584'
Invoke-ExactInspect $NativeHostedContainerSegmentationModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Tool-Runtime-Header-Core.wvproj') `
    $NativeHostedToolRuntimeHeaderCoreModule `
    19516 `
    'f1c156def9fa6f00bb0401097435bb1d1429d9d4be247b8d11f0de0b5ea51be2' `
    'build status=Published verification=compiler-aligned functions=29 code-bytes=17050 module-bytes=19516'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Native-Hosted-Tool-Runtime-Header.wvproj') `
    $NativeHostedToolRuntimeHeaderBridgeModule `
    19459 `
    '3cc8d0850b888911ee3338600bc7699578b163e7400c2b3631ef14649b9a3f18' `
    'build status=Published verification=compiler-aligned functions=30 code-bytes=17081 module-bytes=19459'
Invoke-ExactInspect $NativeHostedToolRuntimeHeaderBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Lifetime-Core.wvproj') `
    $NativePublicationLifetimeCoreModule `
    4955 `
    'a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3358 module-bytes=4955'
Invoke-ExactInspect $NativePublicationLifetimeCoreModule @('profile=portable', 'Native\\u02C9publication\\u02C9lifetime\\u02C9result', 'Native\\u02C9publication\\u02C9lifetime\\u02C9status', 'Native\\u02C9publication\\u02C9lifetime\\u02C9plan', 'section name=exports .* count=7')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Lifetime.wvproj') `
    $NativePublicationLifetimeBridgeModule `
    4442 `
    'f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=3424 module-bytes=4442'
Invoke-ExactInspect $NativePublicationLifetimeBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Lexer-Core.wvproj') `
    $SourceLexerModule `
    49470 `
    '411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=40152 module-bytes=49470'
Invoke-ExactInspect $SourceLexerModule @('profile=portable', 'section name=exports offset=46433 bytes=715 count=17', 'section name=types offset=47156 bytes=2314 count=7', 'Compiler\\u02C9source\\u02C9token', 'Compiler\\u02C9token\\u02C9kind', 'Compiler\\u02C9lex\\u02C9source\\u02C9bounded')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Lexer-Demo.wvproj') `
    $SourceLexerDemoModule `
    56674 `
    'f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db' `
    'build status=Published verification=compiler-aligned functions=21 code-bytes=46427 module-bytes=56674'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Declaration-Parser.wvproj') `
    $SourceDeclarationParserModule `
    151197 `
    '8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb' `
    'build status=Published verification=compiler-aligned functions=52 code-bytes=120804 module-bytes=151197'
Invoke-ExactInspect $SourceDeclarationParserModule @('profile=portable', 'section name=exports offset=145507 bytes=1417 count=32', 'section name=types offset=146932 bytes=4265 count=15', 'Compiler\\u02C9source\\u02C9declaration', 'Compiler\\u02C9source\\u02C9module\\u02C9summary', 'Compiler\\u02C9parse\\u02C9next\\u02C9declaration\\u02C9validated')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Declaration-Parser-Demo.wvproj') `
    $SourceDeclarationParserDemoModule `
    154365 `
    '9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf' `
    'build status=Published verification=compiler-aligned functions=53 code-bytes=124556 module-bytes=154365'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Declaration-Parser-Tool.wvproj') `
    $SourceDeclarationParserToolModule `
    151731 `
    'ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0' `
    'build status=Published verification=compiler-aligned functions=55 code-bytes=122750 module-bytes=151731'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Body-Parser.wvproj') `
    $SourceBodyParserModule `
    248663 `
    '68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589' `
    'build status=Published verification=compiler-aligned functions=100 code-bytes=197096 module-bytes=248663'
Invoke-ExactInspect $SourceBodyParserModule @('profile=portable', 'section name=exports offset=239096 bytes=2112 count=47', 'section name=types offset=241216 bytes=7447 count=25', 'Compiler\\u02C9source\\u02C9expression', 'Compiler\\u02C9source\\u02C9statement', 'Compiler\\u02C9parse\\u02C9expression\\u02C9validated', 'Compiler\\u02C9parse\\u02C9source\\u02C9bodies')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Body-Parser-Demo.wvproj') `
    $SourceBodyParserDemoModule `
    254805 `
    '2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f' `
    'build status=Published verification=compiler-aligned functions=101 code-bytes=204515 module-bytes=254805'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Body-Parser-Tool.wvproj') `
    $SourceBodyParserToolModule `
    247844 `
    '0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f' `
    'build status=Published verification=compiler-aligned functions=103 code-bytes=198924 module-bytes=247844'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Set-Core.wvproj') `
    $SourceSetModule `
    257873 `
    '1121320e20d83f685c559ea2d0cff8b8e57583d047a3c6aaf9f5c1fdc9423acb' `
    'build status=Published verification=compiler-aligned functions=110 code-bytes=206538 module-bytes=257873'
Invoke-ExactInspect $SourceSetModule @('profile=portable', 'section name=exports offset=249270 bytes=430 count=10', 'section name=types offset=249708 bytes=8165 count=29', 'Compiler\\u02C9source\\u02C9set\\u02C9scan', 'Compiler\\u02C9source\\u02C9set\\u02C9summary', 'Compiler\\u02C9scan\\u02C9source\\u02C9set', 'Compiler\\u02C9validate\\u02C9source\\u02C9set')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Set-Demo.wvproj') `
    $SourceSetDemoModule `
    267203 `
    'ac7fb0e04cf042ab9f9f3bfc8f344f0fdbcdc4198189b65f152eaead84b07742' `
    'build status=Published verification=compiler-aligned functions=116 code-bytes=214034 module-bytes=267203'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Set-Tool.wvproj') `
    $SourceSetToolModule `
    261726 `
    '6e8b8c8aaa6fe2c5735719a9b317e8897cf70f87828ea1be5d26d670bc2ed30f' `
    'build status=Published verification=compiler-aligned functions=115 code-bytes=209802 module-bytes=261726'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Graph-Core.wvproj') `
    $SourceGraphModule `
    278894 `
    '9c1ae01b93b9a598fd6b726071dad9a8b4c6fe47d9c8e2d060eff9451724c85b' `
    'build status=Published verification=compiler-aligned functions=126 code-bytes=223460 module-bytes=278894'
Invoke-ExactInspect $SourceGraphModule @('profile=portable', 'section name=exports offset=269556 bytes=549 count=12', 'section name=types offset=270113 bytes=8781 count=34', 'Compiler\\u02C9source\\u02C9graph\\u02C9status', 'Compiler\\u02C9source\\u02C9graph\\u02C9summary', 'Compiler\\u02C9validate\\u02C9source\\u02C9graph')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Graph-Demo.wvproj') `
    $SourceGraphDemoModule `
    284848 `
    'a762e564411e9fe72b906c3c37521c9047bb40b1267d2fb46223f382f1c7966c' `
    'build status=Published verification=compiler-aligned functions=131 code-bytes=228355 module-bytes=284848'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Graph-Tool.wvproj') `
    $SourceGraphToolModule `
    282035 `
    '0a23a10c6abb9eb82229300ab92324f3298fcbf26d3be0948dbc984274a9ac10' `
    'build status=Published verification=compiler-aligned functions=131 code-bytes=226370 module-bytes=282035'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Symbols-Core.wvproj') `
    $SourceSymbolsModule `
    439545 `
    'a7df71802871d48561c8045d7e997266365d74f7e5158d531164ae636d57a5e7' `
    'build status=Published verification=compiler-aligned functions=204 code-bytes=351993 module-bytes=439545'
Invoke-ExactInspect $SourceSymbolsModule @('profile=portable', 'section name=exports offset=424691 bytes=3608 count=66', 'section name=types offset=428307 bytes=11238 count=45', 'Compiler\\u02C9source\\u02C9symbol\\u02C9status', 'Compiler\\u02C9source\\u02C9symbol\\u02C9summary', 'Compiler\\u02C9source\\u02C9symbols\\u02C9directory\\u02C9is\\u02C9valid', 'Compiler\\u02C9validate\\u02C9source\\u02C9symbols')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Symbols-Demo.wvproj') `
    $SourceSymbolsDemoModule `
    450431 `
    '4cf84322af1cd514bc7ac9ac5e752ef689bb1729e83ea9021b9660c823243457' `
    'build status=Published verification=compiler-aligned functions=213 code-bytes=362117 module-bytes=450431'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Symbols-Tool.wvproj') `
    $SourceSymbolsToolModule `
    438378 `
    '58732a7cb3352f1f61ba4cecb65ae0280aecc975ca06eca359a2881e14477a66' `
    'build status=Published verification=compiler-aligned functions=209 code-bytes=355987 module-bytes=438378'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Bindings-Core.wvproj') `
    $SourceBindingsModule `
    542309 `
    'a772a75fe625f47e165ca190e76d8cd59fa0b591a0270a5817e02e0fac62542c' `
    'build status=Published verification=compiler-aligned functions=263 code-bytes=437438 module-bytes=542309'
Invoke-ExactInspect $SourceBindingsModule @('profile=portable', 'section name=exports offset=526082 bytes=2996 count=59', 'section name=types offset=529086 bytes=13223 count=55', 'Compiler\\u02C9source\\u02C9binding\\u02C9status', 'Compiler\\u02C9source\\u02C9binding\\u02C9summary', 'Compiler\\u02C9source\\u02C9bindings\\u02C9directory\\u02C9is\\u02C9valid', 'Compiler\\u02C9validate\\u02C9source\\u02C9bindings')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Bindings-Demo.wvproj') `
    $SourceBindingsDemoModule `
    548036 `
    '563caeb4a76fb34d6c2b2b8340260cc1da518c4cbaad9e5f355201f6bd1fa933' `
    'build status=Published verification=compiler-aligned functions=271 code-bytes=443818 module-bytes=548036'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Bindings-Tool.wvproj') `
    $SourceBindingsToolModule `
    542334 `
    '17e877b3c59d2f9a99d26be4c478f10ce8879e6bce925b65894d158fd4a6e0a9' `
    'build status=Published verification=compiler-aligned functions=268 code-bytes=441068 module-bytes=542334'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Wir-Core.wvproj') `
    $SourceWirModule `
    817391 `
    'c4c3bd9164ccdf75acd1140e74c256295bb1f8ea8bdbf69cdcd3225ceea70fbb' `
    'build status=Published verification=compiler-aligned functions=346 code-bytes=665606 module-bytes=817391'
Invoke-ExactInspect $SourceWirModule @('profile=portable', 'section name=exports offset=794006 bytes=3755 count=75', 'section name=types offset=797769 bytes=19622 count=66', 'Compiler\\u02C9source\\u02C9wir\\u02C9operation', 'Compiler\\u02C9source\\u02C9wir\\u02C9summary', 'Compiler\\u02C9source\\u02C9wir\\u02C9directory\\u02C9is\\u02C9valid', 'Compiler\\u02C9validate\\u02C9source\\u02C9wir')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Wir-Demo.wvproj') `
    $SourceWirDemoModule `
    822254 `
    '7f533fcb38a9311ba4d390b814ea3741ab25d5db9ac2167bd9f4f6b58bddc02f' `
    'build status=Published verification=compiler-aligned functions=352 code-bytes=672121 module-bytes=822254'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Windvale-Source-Wir-Tool.wvproj') `
    $SourceWirToolModule `
    815722 `
    '7fbfc8f57620dd81a5d2024310a21a8ce32d56cc986d94b39ca03428c1404db5' `
    'build status=Published verification=compiler-aligned functions=351 code-bytes=669118 module-bytes=815722'

Invoke-ExactSourceCompilerBuild `
    'core' `
    $SourceWvbModule `
    923514 `
    'c4602b6c026a65e0b9de11c025768b7f652ee73640b6f5ff1806d40ee5d0071b' `
    'source wvb status=Valid functions=422 code-bytes=757261 module-bytes=923514'
Invoke-ExactInspect $SourceWvbModule @('profile=portable', 'section name=exports offset=898984 bytes=3322 count=70', 'section name=types offset=902314 bytes=21200 count=82', 'Compiler\\u02C9source\\u02C9wvb\\u02C9summary', 'Compiler\\u02C9compile\\u02C9source\\u02C9wvb')
Invoke-ExactSourceCompilerBuild `
    'demo' `
    $SourceWvbDemoModule `
    923210 `
    'ef5a7cad94cce135dd937756980f9268fa2964f49dbb4fccca95ba4d09713fc9' `
    'source wvb status=Valid functions=426 code-bytes=760228 module-bytes=923210'
Invoke-ExactSourceCompilerBuild `
    'tool' `
    $SourceWvbToolModule `
    921640 `
    '18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754' `
    'source wvb status=Valid functions=427 code-bytes=759920 module-bytes=921640'

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
Write-Output 'native Seed front-door verification status=Complete artifacts=97 cases=156'
