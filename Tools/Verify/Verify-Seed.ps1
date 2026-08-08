[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$ReportPath,
    [ValidateSet('Fast', 'Development', 'Standard', 'Qualification')]
    [string]$Level = 'Development',
    [string]$TestFilter,
    [ValidateSet('assembler', 'bytecode', 'compiler', 'database', 'foundation', 'golden', 'linker', 'object-model', 'runtime')]
    [string[]]$TestArea,
    [switch]$FailFast,
    [switch]$IncludeExtended,
    [string]$TimingReportPath
)

$ErrorActionPreference = 'Stop'

# Windvale identifiers are emitted as UTF-8 by the CLI. Configure native-process
# decoding inside the verifier so attached and redirected/detached launches
# preserve macron identifiers identically instead of inheriting a console code page.
$Utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $Utf8WithoutBom
[Console]::OutputEncoding = $Utf8WithoutBom
$OutputEncoding = $Utf8WithoutBom

$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ToolDll = Join-Path $RepositoryRoot "Tools/Windvale.Tool/bin/$Configuration/net10.0/windvale.dll"
$TestProject = Join-Path $RepositoryRoot 'Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj'
$OsTestProject = Join-Path $RepositoryRoot 'Tests/Windvale.Os.Tests/Windvale.Os.Tests.csproj'
$Artifacts = Join-Path $RepositoryRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null
$DevelopmentAreas = @(
    'assembler',
    'bytecode',
    'compiler',
    'database',
    'foundation',
    'linker',
    'object-model',
    'runtime'
)
$SelectedAreas = @($TestArea | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
if (
    $Level -eq 'Fast' -and
    [string]::IsNullOrWhiteSpace($TestFilter) -and
    $SelectedAreas.Count -eq 0
) {
    throw 'Fast verification requires -TestFilter or -TestArea so its scope is explicit.'
}
if (
    $Level -ne 'Fast' -and
    (![string]::IsNullOrWhiteSpace($TestFilter) -or $SelectedAreas.Count -ne 0)
) {
    throw 'Test selection is available only with -Level Fast; other levels have fixed suites.'
}
if ($Level -ne 'Fast' -and $FailFast) {
    throw '-FailFast is available only with -Level Fast; other levels have fixed suites.'
}
if ($Level -ne 'Fast' -and $IncludeExtended) {
    throw '-IncludeExtended is available only with -Level Fast; Standard and Qualification already include extended tests.'
}
if ($Level -eq 'Development' -and ![string]::IsNullOrWhiteSpace($ReportPath)) {
    throw '-ReportPath is available only with Standard or Qualification; Development is not conformance evidence.'
}
if ($Level -in @('Standard', 'Qualification') -and [string]::IsNullOrWhiteSpace($ReportPath)) {
    $Architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
    $ReportPath = Join-Path $RepositoryRoot "artifacts/seed-conformance-windows-$Architecture.json"
}

dotnet build (Join-Path $RepositoryRoot 'Windvale.slnx') --configuration $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Windvale Seed build failed with exit code $LASTEXITCODE."
}

$TestArguments = @()
if ($Level -eq 'Fast') {
    if (![string]::IsNullOrWhiteSpace($TestFilter)) {
        $TestArguments += @('--filter', $TestFilter)
    }
    foreach ($Area in $SelectedAreas) {
        $TestArguments += @('--area', $Area)
    }
    if ($FailFast) {
        $TestArguments += '--fail-fast'
    }
    if (!$IncludeExtended) {
        $TestArguments += '--exclude-extended'
    }
} elseif ($Level -eq 'Development') {
    foreach ($Area in $DevelopmentAreas) {
        $TestArguments += @('--area', $Area)
    }
    $TestArguments += '--exclude-extended'
} else {
    $TestArguments += @('--report', $ReportPath)
}
if (![string]::IsNullOrWhiteSpace($TimingReportPath)) {
    $TestArguments += @('--timing-report', $TimingReportPath)
}

dotnet run --project $TestProject --configuration $Configuration --no-build -- @TestArguments
if ($LASTEXITCODE -ne 0) {
    throw "Windvale Seed conformance tests failed with exit code $LASTEXITCODE."
}

if ($Level -eq 'Fast') {
    $Selection = @()
    if (![string]::IsNullOrWhiteSpace($TestFilter)) {
        $Selection += "filter '$TestFilter'"
    }
    if ($SelectedAreas.Count -ne 0) {
        $Selection += "areas [$($SelectedAreas -join ', ')]"
    }
    $Cost = $IncludeExtended ? 'including extended tests' : 'for regular tests'
    Write-Host "Windvale Seed fast verification passed $Cost matching $($Selection -join ' and ')."
    return
}

dotnet run --project $OsTestProject --configuration $Configuration --no-build
if ($LASTEXITCODE -ne 0) {
    throw "Windvale OS in-process tests failed with exit code $LASTEXITCODE."
}

if ($Level -eq 'Development') {
    Write-Host 'Windvale Seed development verification passed for every regular in-process test.'
    Write-Host 'Extended integration contracts and the golden cross-host contract were not executed.'
    return
}
if ($Level -eq 'Standard') {
    Write-Host 'Windvale Seed standard conformance verification passed.'
    Write-Host "Conformance report: $ReportPath"
    return
}

dotnet publish `
    (Join-Path $RepositoryRoot 'Tools/Windvale.Tool/Windvale.Tool.csproj') `
    --configuration $Configuration `
    --runtime linux-x64 `
    --self-contained false `
    -p:UseAppHost=false `
    --output (Join-Path $Artifacts 'publish-linux-x64') `
    --nologo
if ($LASTEXITCODE -ne 0) {
    throw "The framework-dependent Linux CLI publication failed with exit code $LASTEXITCODE."
}

$SumModule = Join-Path $Artifacts 'Sum-Data.wvb'
$SumWindowsApplication = Join-Path $Artifacts 'Sum-Data-Windows.exe'
$SumLinuxApplication = Join-Path $Artifacts 'Sum-Data-Linux.elf'
$HelloModule = Join-Path $Artifacts 'Hello-Windvale.wvb'
$FoundationModule = Join-Path $Artifacts 'Read-Wvb-Header.wvb'
$CompositionModule = Join-Path $Artifacts 'Module-Composition-Demo.wvb'
$CompositionReorderedModule = Join-Path $Artifacts 'Module-Composition-Demo-Reordered.wvb'
$ProjectCompositionModule = Join-Path $Artifacts 'Module-Composition-Demo-Project.wvb'
$InvalidProjectManifest = Join-Path $Artifacts '__windvale_invalid_project__.wvproj'
$InvalidProjectModule = Join-Path $Artifacts '__windvale_invalid_project_output__.wvb'
$InvalidCompositionModule = Join-Path $Artifacts '__windvale_invalid_composition_output__.wvb'
$MachineContractsModule = Join-Path $Artifacts 'Machine-Contracts.wvb'
$MachineContractsDemoModule = Join-Path $Artifacts 'Machine-Contracts-Demo.wvb'
$ByteOrderingModule = Join-Path $Artifacts 'Byte-Ordering.wvb'
$ByteOrderingDemoModule = Join-Path $Artifacts 'Byte-Ordering-Demo.wvb'
$DecimalParsingModule = Join-Path $Artifacts 'Decimal-Parsing.wvb'
$DecimalParsingDemoModule = Join-Path $Artifacts 'Decimal-Parsing-Demo.wvb'
$ByteConstructionModule = Join-Path $Artifacts 'Byte-Construction.wvb'
$ByteConstructionDemoModule = Join-Path $Artifacts 'Byte-Construction-Demo.wvb'
$NativeStencilModule = Join-Path $Artifacts 'Native-Stencil-Core.wvb'
$NativeStencilDemoModule = Join-Path $Artifacts 'Native-Stencil-Demo.wvb'
$NativeStencilBridgeModule = Join-Path $Artifacts 'Native-Stencil-Bridge.wvb'
$NativeUtf8CoreModule = Join-Path $Artifacts 'Native-X64-Utf8-Service.wvb'
$NativeUtf8BridgeModule = Join-Path $Artifacts 'Native-X64-Utf8-Service-Bridge.wvb'
$NativeIntegerFormatCoreModule = Join-Path $Artifacts 'Native-X64-Integer-Format-Services.wvb'
$NativeIntegerFormatBridgeModule = Join-Path $Artifacts 'Native-X64-Integer-Format-Services-Bridge.wvb'
$NativeServiceCodeBuilderModule = Join-Path $Artifacts 'Native-X64-Service-Code-Builder.wvb'
$NativeWindowsOutputCoreModule = Join-Path $Artifacts 'Native-X64-Output-Service-Windows.wvb'
$NativeLinuxOutputCoreModule = Join-Path $Artifacts 'Native-X64-Output-Service-Linux.wvb'
$NativeOutputBridgeModule = Join-Path $Artifacts 'Native-X64-Output-Services-Bridge.wvb'
$NativeFileOutputCodeModule = Join-Path $Artifacts 'Native-X64-File-Output-Service-Code.wvb'
$NativeWindowsFileOutputCoreModule = Join-Path $Artifacts 'Native-X64-File-Output-Service-Windows.wvb'
$NativeLinuxFileOutputCoreModule = Join-Path $Artifacts 'Native-X64-File-Output-Service-Linux.wvb'
$NativeFileOutputBridgeModule = Join-Path $Artifacts 'Native-X64-File-Output-Services-Bridge.wvb'
$NativeFileInputCodeModule = Join-Path $Artifacts 'Native-X64-File-Input-Service-Code.wvb'
$NativeWindowsFileInputCoreModule = Join-Path $Artifacts 'Native-X64-File-Input-Service-Windows.wvb'
$NativeLinuxFileInputCoreModule = Join-Path $Artifacts 'Native-X64-File-Input-Service-Linux.wvb'
$NativeFileInputBridgeModule = Join-Path $Artifacts 'Native-X64-File-Input-Services-Bridge.wvb'
$NativeTextConcatCoreModule = Join-Path $Artifacts 'Native-X64-Text-Concat-Service.wvb'
$NativeTextConcatBridgeModule = Join-Path $Artifacts 'Native-X64-Text-Concat-Service-Bridge.wvb'
$NativeTextQuoteCoreModule = Join-Path $Artifacts 'Native-X64-Text-Quote-Service.wvb'
$NativeTextQuoteBridgeModule = Join-Path $Artifacts 'Native-X64-Text-Quote-Service-Bridge.wvb'
$NativeEnumNameCoreModule = Join-Path $Artifacts 'Native-X64-Enum-Name-Service.wvb'
$NativeEnumNameBridgeModule = Join-Path $Artifacts 'Native-X64-Enum-Name-Service-Bridge.wvb'
$NativeEnumMetadataCoreModule = Join-Path $Artifacts 'Native-Enum-Metadata-Core.wvb'
$NativeEnumMetadataBridgeModule = Join-Path $Artifacts 'Native-Enum-Metadata-Bridge.wvb'
$NativePublicationModule = Join-Path $Artifacts 'Native-Publication-Core.wvb'
$NativePublicationBridgeModule = Join-Path $Artifacts 'Native-Publication-Bridge.wvb'
$NativeServiceBundleMaterializationCoreModule = Join-Path $Artifacts 'Native-Service-Bundle-Materialization-Core.wvb'
$NativeServiceBundleMaterializationBridgeModule = Join-Path $Artifacts 'Native-Service-Bundle-Materialization-Bridge.wvb'
$NativeOutputTableCoreModule = Join-Path $Artifacts 'Native-Output-Table-Core.wvb'
$NativeOutputTableBridgeModule = Join-Path $Artifacts 'Native-Output-Table-Bridge.wvb'
$NativeFileOutputTableCoreModule = Join-Path $Artifacts 'Native-File-Output-Table-Core.wvb'
$NativeFileOutputTableBridgeModule = Join-Path $Artifacts 'Native-File-Output-Table-Bridge.wvb'
$NativeFileInputTableCoreModule = Join-Path $Artifacts 'Native-File-Input-Table-Core.wvb'
$NativeFileInputTableBridgeModule = Join-Path $Artifacts 'Native-File-Input-Table-Bridge.wvb'
$NativeServiceTableCoreModule = Join-Path $Artifacts 'Native-Service-Table-Core.wvb'
$NativeServiceTableBridgeModule = Join-Path $Artifacts 'Native-Service-Table-Bridge.wvb'
$NativePublicationLifetimeModule = Join-Path $Artifacts 'Native-Publication-Lifetime-Core.wvb'
$NativePublicationLifetimeBridgeModule = Join-Path $Artifacts 'Native-Publication-Lifetime-Bridge.wvb'
$SourceLexerModule = Join-Path $Artifacts 'Source-Lexer-Core.wvb'
$SourceLexerDemoModule = Join-Path $Artifacts 'Source-Lexer-Demo.wvb'
$SourceDeclarationParserModule = Join-Path $Artifacts 'Source-Declaration-Parser.wvb'
$SourceDeclarationParserDemoModule = Join-Path $Artifacts 'Source-Declaration-Parser-Demo.wvb'
$SourceDeclarationParserToolModule = Join-Path $Artifacts 'Source-Declaration-Parser-Tool.wvb'
$SourceBodyParserModule = Join-Path $Artifacts 'Source-Body-Parser.wvb'
$SourceBodyParserDemoModule = Join-Path $Artifacts 'Source-Body-Parser-Demo.wvb'
$SourceBodyParserToolModule = Join-Path $Artifacts 'Source-Body-Parser-Tool.wvb'
$SourceSetModule = Join-Path $Artifacts 'Source-Set-Core.wvb'
$SourceSetDemoModule = Join-Path $Artifacts 'Source-Set-Demo.wvb'
$SourceSetToolModule = Join-Path $Artifacts 'Source-Set-Tool.wvb'
$SourceGraphModule = Join-Path $Artifacts 'Source-Graph-Core.wvb'
$SourceGraphDemoModule = Join-Path $Artifacts 'Source-Graph-Demo.wvb'
$SourceGraphToolModule = Join-Path $Artifacts 'Source-Graph-Tool.wvb'
$SourceSymbolsModule = Join-Path $Artifacts 'Source-Symbols-Core.wvb'
$SourceSymbolsDemoModule = Join-Path $Artifacts 'Source-Symbols-Demo.wvb'
$SourceSymbolsToolModule = Join-Path $Artifacts 'Source-Symbols-Tool.wvb'
$SourceBindingsModule = Join-Path $Artifacts 'Source-Bindings-Core.wvb'
$SourceBindingsDemoModule = Join-Path $Artifacts 'Source-Bindings-Demo.wvb'
$SourceBindingsToolModule = Join-Path $Artifacts 'Source-Bindings-Tool.wvb'
$SourceWirModule = Join-Path $Artifacts 'Source-Wir-Core.wvb'
$SourceWirDemoModule = Join-Path $Artifacts 'Source-Wir-Demo.wvb'
$SourceWirToolModule = Join-Path $Artifacts 'Source-Wir-Tool.wvb'
$SourceWvbModule = Join-Path $Artifacts 'Source-Wvb-Core.wvb'
$SourceWvbDemoModule = Join-Path $Artifacts 'Source-Wvb-Demo.wvb'
$SourceWvbToolModule = Join-Path $Artifacts 'Source-Wvb-Tool.wvb'
$SourceWvbFixtureModule = Join-Path $Artifacts 'Source-Wvb-Function-Only.wvb'
$SourceWvbFixtureOracle = Join-Path $Artifacts 'Source-Wvb-Function-Only-Stage0.wvb'
$SourceWvbDataFixtureModule = Join-Path $Artifacts 'Source-Wvb-Data-And-Text.wvb'
$SourceWvbDataFixtureOracle = Join-Path $Artifacts 'Source-Wvb-Data-And-Text-Stage0.wvb'
$SourceWvbNominalFixtureModule = Join-Path $Artifacts 'Source-Wvb-Nominal-Types.wvb'
$SourceWvbNominalFixtureOracle = Join-Path $Artifacts 'Source-Wvb-Nominal-Types-Stage0.wvb'
$SourceWvbHostedFixtureModule = Join-Path $Artifacts 'Source-Wvb-Hosted-Capabilities.wvb'
$SourceWvbHostedFixtureOracle = Join-Path $Artifacts 'Source-Wvb-Hosted-Capabilities-Stage0.wvb'
$SourceWvbCompositionModule = Join-Path $Artifacts 'Source-Wvb-Composition.wvb'
$SourceWvbCompositionOracle = Join-Path $Artifacts 'Source-Wvb-Composition-Stage0.wvb'
$InvalidSourceWvbCompositionModule = Join-Path $Artifacts '__windvale_invalid_source_wvb_composition_output__.wvb'
$WvDumpCoreModule = Join-Path $Artifacts 'Wv-Dump-Core.wvb'
$WvoCoreModule = Join-Path $Artifacts 'Wvo-Object-Core.wvb'
$WvaAssemblerModule = Join-Path $Artifacts 'Wva-Assembler-Core.wvb'
$WvLinkerCoreModule = Join-Path $Artifacts 'Wv-Linker-Core.wvb'
$WvoSample = Join-Path $Artifacts 'Sample.wvo'
$AssemblyObject = Join-Path $Artifacts 'Hello-Object.wvo'
$WindvaleAssemblyObject = Join-Path $Artifacts 'Hello-Object-Windvale.wvo'
$InvalidWindvaleAssemblyObject = Join-Path $Artifacts '__windvale_invalid_assembly_output__.wvo'
$LinkProviderObject = Join-Path $Artifacts 'Console-Provider.wvo'
$WindvaleLinkedImage = Join-Path $Artifacts 'Hello-Linked-Windvale.bin'
$WindvaleLinkMap = Join-Path $Artifacts 'Hello-Linked-Windvale.wvmap'
$InvalidWindvaleLinkedImage = Join-Path $Artifacts '__windvale_invalid_wvlink_output__.bin'
$LinkedImage = Join-Path $Artifacts 'Hello-Linked.bin'
$LinkMap = Join-Path $Artifacts 'Hello-Linked.wvmap'
$InvalidLinkedImage = Join-Path $Artifacts '__windvale_invalid_link_output__.bin'
dotnet $ToolDll compile (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') -o $SumModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Sum-Data.wv.' }

$WindowsApplicationOutput = dotnet $ToolDll compile `
    (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') `
    --target windows-x64-console-v1 `
    -o $SumWindowsApplication
if (
    $LASTEXITCODE -ne 0 -or
    $WindowsApplicationOutput -notcontains 'Target: windows-x64-console-v1' -or
    $WindowsApplicationOutput -notcontains 'SHA-256: 5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77'
) {
    $WindowsApplicationText = $WindowsApplicationOutput -join ' | '
    throw "The Seed CLI failed to produce the canonical Windows application (exit $LASTEXITCODE; output: $WindowsApplicationText)."
}
$WindowsApplicationHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SumWindowsApplication).Hash.ToLowerInvariant()
if (
    (Get-Item -LiteralPath $SumWindowsApplication).Length -ne 5120 -or
    $WindowsApplicationHash -ne '5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77'
) {
    throw 'The Seed CLI Windows application identity is not canonical.'
}
if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows)) {
    & $SumWindowsApplication
    if ($LASTEXITCODE -ne 29) {
        throw "The generated Windows application returned $LASTEXITCODE instead of 29."
    }
}

$LinuxApplicationOutput = dotnet $ToolDll compile `
    (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') `
    --target linux-x64-console-v1 `
    -o $SumLinuxApplication
if (
    $LASTEXITCODE -ne 0 -or
    $LinuxApplicationOutput -notcontains 'Target: linux-x64-console-v1' -or
    $LinuxApplicationOutput -notcontains 'SHA-256: 8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4'
) {
    $LinuxApplicationText = $LinuxApplicationOutput -join ' | '
    throw "The Seed CLI failed to produce the canonical Linux application (exit $LASTEXITCODE; output: $LinuxApplicationText)."
}
$LinuxApplicationHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SumLinuxApplication).Hash.ToLowerInvariant()
if (
    (Get-Item -LiteralPath $SumLinuxApplication).Length -ne 8304 -or
    $LinuxApplicationHash -ne '8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4'
) {
    throw 'The Seed CLI Linux application identity is not canonical.'
}

$VerifyOutput = dotnet $ToolDll verify $SumModule
if ($LASTEXITCODE -ne 0 -or $VerifyOutput -notcontains 'Verified: Sumˉdata') {
    $VerifyText = $VerifyOutput -join ' | '
    throw "The Seed CLI failed to verify Sum-Data.wvb (exit $LASTEXITCODE; output: $VerifyText)."
}

$InspectOutput = dotnet $ToolDll inspect $SumModule
if ($LASTEXITCODE -ne 0 -or ($InspectOutput -join "`n") -notmatch 'data\.load\.i32') {
    throw 'The Seed CLI inspector did not expose the expected data instruction.'
}

$RunOutput = dotnet $ToolDll run $SumModule
if (
    $LASTEXITCODE -ne 0 -or
    $RunOutput -notcontains 'Result: 29' -or
    ($RunOutput -join "`n") -match '(?m)^Function (instructions|record-fields|dynamic-bytes)='
) {
    throw 'The Seed CLI did not produce Result: 29 for Sum-Data.wvb.'
}

$StepReportOutput = dotnet $ToolDll run $SumModule --report-steps
if (
    $LASTEXITCODE -ne 0 -or
    $StepReportOutput -notcontains 'Result: 29' -or
    ($StepReportOutput -join "`n") -notmatch '(?m)^Instructions: [1-9][0-9]*$'
) {
    throw 'The Seed CLI did not report a positive instruction count for Sum-Data.wvb.'
}

$FunctionStepReportOutput = dotnet $ToolDll run $SumModule --report-function-steps 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    $FunctionStepReportOutput -notcontains 'Result: 29' -or
    ($FunctionStepReportOutput -join "`n") -notmatch '(?m)^Function instructions=163 index=1 name=Main$' -or
    ($FunctionStepReportOutput -join "`n") -notmatch '(?m)^Function instructions=40 index=0 name=Add$'
) {
    throw 'The Seed CLI did not report deterministic per-function instruction counts for Sum-Data.wvb.'
}

dotnet $ToolDll compile (Join-Path $RepositoryRoot 'Examples/Seed/Hello-Windvale.wv') -o $HelloModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Hello-Windvale.wv.' }

$UnauthorizedOutput = dotnet $ToolDll run $HelloModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($UnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse an ungranted console capability.'
}

$HelloOutput = dotnet $ToolDll run $HelloModule --allow console.write_line
if ($LASTEXITCODE -ne 0 -or $HelloOutput -notcontains 'Hello from Windvale' -or $HelloOutput -notcontains 'Result: 0') {
    throw 'The Seed CLI did not run the authorized Hello-Windvale module correctly.'
}

dotnet $ToolDll compile (Join-Path $RepositoryRoot 'Examples/Foundation/Read-Wvb-Header.wv') -o $FoundationModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Read-Wvb-Header.wv.' }

$FoundationVerifyOutput = dotnet $ToolDll verify $FoundationModule
if ($LASTEXITCODE -ne 0 -or $FoundationVerifyOutput -notcontains 'Verified: Readˉwvbˉheader') {
    throw 'The Seed CLI failed to verify Read-Wvb-Header.wvb.'
}

$FoundationInspectOutput = dotnet $ToolDll inspect $FoundationModule
if ($LASTEXITCODE -ne 0 -or ($FoundationInspectOutput -join "`n") -notmatch 'bytes\.read_u32_little') {
    throw 'The Seed CLI inspector did not expose the expected little-endian read instruction.'
}

$FoundationRunOutput = dotnet $ToolDll run $FoundationModule
if ($LASTEXITCODE -ne 0 -or $FoundationRunOutput -notcontains 'Result: 1') {
    throw 'The Seed CLI did not produce Result: 1 for Read-Wvb-Header.wvb.'
}

$CompositionRoot = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Demo.wv'
$CompositionMiddle = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Middle.wv'
$CompositionLeaf = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Leaf.wv'
dotnet $ToolDll `
    compile $CompositionRoot --module $CompositionMiddle --module $CompositionLeaf -o $CompositionModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-module composition demo.' }
$CompositionHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $CompositionModule).Hash.ToLowerInvariant()
if ($CompositionHash -ne '030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607') {
    throw "The composed source module has an unexpected digest: $CompositionHash"
}
$CompositionRunOutput = dotnet $ToolDll run $CompositionModule
if ($LASTEXITCODE -ne 0 -or $CompositionRunOutput -notcontains 'Result: 42') {
    throw 'The composed source module did not return Result: 42.'
}
$RecordFieldReportOutput = dotnet $ToolDll run $CompositionModule --report-function-record-fields 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    $RecordFieldReportOutput -notcontains 'Result: 42' -or
    ($RecordFieldReportOutput -join "`n") -notmatch '(?m)^Function record-fields=2 index=1 name=__WvM1F0$' -or
    [regex]::Matches(($RecordFieldReportOutput -join "`n"), '(?m)^Function record-fields=').Count -ne 1
) {
    throw 'The Seed CLI did not report deterministic per-function record construction pressure.'
}
dotnet $ToolDll `
    compile $CompositionRoot --module $CompositionLeaf --module $CompositionMiddle -o $CompositionReorderedModule
if ($LASTEXITCODE -ne 0) { throw 'The reordered source-module compile failed.' }
$CompositionReorderedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $CompositionReorderedModule).Hash.ToLowerInvariant()
if ($CompositionReorderedHash -ne $CompositionHash) {
    throw 'Reordering explicit source-module inputs changed the composed WVB bytes.'
}
$CompositionProject = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Demo.wvproj'
Push-Location $Artifacts
try {
    dotnet $ToolDll build $CompositionProject -o $ProjectCompositionModule
} finally {
    Pop-Location
}
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to build the source-module project.' }
$ProjectCompositionHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ProjectCompositionModule).Hash.ToLowerInvariant()
if ($ProjectCompositionHash -ne $CompositionHash) {
    throw "The project build changed the composed WVB digest: $ProjectCompositionHash"
}
if (![Linq.Enumerable]::SequenceEqual(
    [IO.File]::ReadAllBytes($CompositionModule),
    [IO.File]::ReadAllBytes($ProjectCompositionModule))) {
    throw 'The project and explicit compile commands produced different WVB bytes.'
}
[IO.File]::WriteAllText(
    $InvalidProjectManifest,
    "windvale-project 1`nroot `"Missing.wv`"`n",
    [Text.UTF8Encoding]::new($false, $true))
[IO.File]::WriteAllBytes($InvalidProjectModule, [byte[]](9, 8, 7))
$InvalidProjectOutput = dotnet $ToolDll build $InvalidProjectManifest -o $InvalidProjectModule 2>&1
if ($LASTEXITCODE -ne 1 -or ($InvalidProjectOutput -join "`n") -notmatch 'WVP1004') {
    throw 'The project builder did not reject a missing emit directive deterministically.'
}
if ([Convert]::ToHexString([IO.File]::ReadAllBytes($InvalidProjectModule)) -ne '090807') {
    throw 'A rejected project build modified its existing output module.'
}
Remove-Item -LiteralPath $InvalidProjectManifest, $InvalidProjectModule -Force
if (Test-Path -LiteralPath $InvalidCompositionModule) {
    Remove-Item -LiteralPath $InvalidCompositionModule -Force
}
$MissingCompositionOutput = dotnet $ToolDll `
    compile $CompositionRoot --module $CompositionMiddle -o $InvalidCompositionModule 2>&1
if ($LASTEXITCODE -ne 1 -or ($MissingCompositionOutput -join "`n") -notmatch 'WVC0007') {
    throw 'The source-module compiler did not reject a missing transitive import.'
}
if (Test-Path -LiteralPath $InvalidCompositionModule) {
    throw 'A rejected source-module composition created an output module.'
}
[System.IO.File]::WriteAllBytes($InvalidCompositionModule, [byte[]](9, 8, 7))
$MissingCompositionOutput = dotnet $ToolDll `
    compile $CompositionRoot --module $CompositionMiddle -o $InvalidCompositionModule 2>&1
if ($LASTEXITCODE -ne 1 -or ($MissingCompositionOutput -join "`n") -notmatch 'WVC0007') {
    throw 'The repeated missing-import composition did not fail deterministically.'
}
if ([Convert]::ToHexString([System.IO.File]::ReadAllBytes($InvalidCompositionModule)) -ne '090807') {
    throw 'A rejected source-module composition modified an existing output module.'
}
Remove-Item -LiteralPath $InvalidCompositionModule -Force

$MachineContractsSource = Join-Path $RepositoryRoot 'Foundation/Machine-Contracts.wv'
dotnet $ToolDll `
    compile $MachineContractsSource -o $MachineContractsModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation machine contracts.' }
$MachineContractsHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $MachineContractsModule).Hash.ToLowerInvariant()
if ($MachineContractsHash -ne 'f624739461dea01862121daf234b3a838dfcafd73753e3124a038b7efa8b4fa3') {
    throw "The Foundation machine-contract module has an unexpected digest: $MachineContractsHash"
}
$MachineContractsInspection = (dotnet $ToolDll inspect $MachineContractsModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $MachineContractsInspection -notmatch 'Foundationˉalignmentˉisˉvalid' -or
    $MachineContractsInspection -notmatch 'Foundationˉmachineˉnameˉisˉvalid' -or
    $MachineContractsInspection -notmatch 'Exports \(2\)'
) {
    throw 'The Foundation machine-contract module inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Machine-Contracts-Demo.wv') `
    --module $MachineContractsSource `
    -o $MachineContractsDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation machine-contract demo.' }
$MachineContractsDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $MachineContractsDemoModule).Hash.ToLowerInvariant()
if ($MachineContractsDemoHash -ne '69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3') {
    throw "The Foundation machine-contract demo has an unexpected digest: $MachineContractsDemoHash"
}
$MachineContractsDemoOutput = dotnet $ToolDll run $MachineContractsDemoModule
if ($LASTEXITCODE -ne 0 -or $MachineContractsDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation machine-contract demo did not return Result: 0.'
}

$ByteOrderingSource = Join-Path $RepositoryRoot 'Foundation/Byte-Ordering.wv'
$Sha256Source = Join-Path $RepositoryRoot 'Foundation/Sha256.wv'
dotnet $ToolDll `
    compile $ByteOrderingSource -o $ByteOrderingModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation byte ordering.' }
$ByteOrderingHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteOrderingModule).Hash.ToLowerInvariant()
if ($ByteOrderingHash -ne '27a3c24b5cc358a4f67e2e1959b5e80559918f0176c52e08648e638212e6dece') {
    throw "The Foundation byte-ordering module has an unexpected digest: $ByteOrderingHash"
}
$ByteOrderingInspection = (dotnet $ToolDll inspect $ByteOrderingModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $ByteOrderingInspection -notmatch 'Foundationˉbyteˉspansˉcompare' -or
    $ByteOrderingInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Foundation byte-ordering module inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Byte-Ordering-Demo.wv') `
    --module $ByteOrderingSource `
    -o $ByteOrderingDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation byte-ordering demo.' }
$ByteOrderingDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteOrderingDemoModule).Hash.ToLowerInvariant()
if ($ByteOrderingDemoHash -ne 'fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f') {
    throw "The Foundation byte-ordering demo has an unexpected digest: $ByteOrderingDemoHash"
}
$ByteOrderingDemoOutput = dotnet $ToolDll run $ByteOrderingDemoModule
if ($LASTEXITCODE -ne 0 -or $ByteOrderingDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation byte-ordering demo did not return Result: 0.'
}

$DecimalParsingSource = Join-Path $RepositoryRoot 'Foundation/Decimal-Parsing.wv'
dotnet $ToolDll `
    compile $DecimalParsingSource -o $DecimalParsingModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation decimal parsing.' }
$DecimalParsingHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $DecimalParsingModule).Hash.ToLowerInvariant()
if ($DecimalParsingHash -ne 'bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37') {
    throw "The Foundation decimal-parsing module has an unexpected digest: $DecimalParsingHash"
}
$DecimalParsingInspection = (dotnet $ToolDll inspect $DecimalParsingModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $DecimalParsingInspection -notmatch 'Foundationˉu32ˉparse' -or
    $DecimalParsingInspection -notmatch 'Foundationˉu32ˉdecimalˉparse' -or
    $DecimalParsingInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Foundation decimal-parsing module inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Decimal-Parsing-Demo.wv') `
    --module $DecimalParsingSource `
    -o $DecimalParsingDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation decimal-parsing demo.' }
$DecimalParsingDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $DecimalParsingDemoModule).Hash.ToLowerInvariant()
if ($DecimalParsingDemoHash -ne 'd323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453') {
    throw "The Foundation decimal-parsing demo has an unexpected digest: $DecimalParsingDemoHash"
}
$DecimalParsingDemoOutput = dotnet $ToolDll run $DecimalParsingDemoModule
if ($LASTEXITCODE -ne 0 -or $DecimalParsingDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation decimal-parsing demo did not return Result: 0.'
}

$ByteConstructionSource = Join-Path $RepositoryRoot 'Foundation/Byte-Construction.wv'
dotnet $ToolDll `
    compile $ByteConstructionSource -o $ByteConstructionModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation byte construction.' }
$ByteConstructionHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteConstructionModule).Hash.ToLowerInvariant()
if ($ByteConstructionHash -ne '3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8') {
    throw "The Foundation byte-construction module has an unexpected digest: $ByteConstructionHash"
}
$ByteConstructionInspection = (dotnet $ToolDll inspect $ByteConstructionModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $ByteConstructionInspection -notmatch 'Foundationˉbytesˉresult' -or
    $ByteConstructionInspection -notmatch 'Foundationˉbytesˉrepeat' -or
    $ByteConstructionInspection -notmatch 'Foundationˉbytesˉreplace' -or
    $ByteConstructionInspection -notmatch 'Exports \(2\)'
) {
    throw 'The Foundation byte-construction module inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Byte-Construction-Demo.wv') `
    --module $ByteConstructionSource `
    -o $ByteConstructionDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation byte-construction demo.' }
$ByteConstructionDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteConstructionDemoModule).Hash.ToLowerInvariant()
if ($ByteConstructionDemoHash -ne 'ab594976ced7a84573ade0aa50fb4370d96b8004c8b9a5ec1e888968c7b3bf8f') {
    throw "The Foundation byte-construction demo has an unexpected digest: $ByteConstructionDemoHash"
}
$ByteConstructionDemoOutput = dotnet $ToolDll run $ByteConstructionDemoModule
if ($LASTEXITCODE -ne 0 -or $ByteConstructionDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation byte-construction demo did not return Result: 0.'
}
$DynamicValueReportOutput = dotnet $ToolDll run $ByteConstructionDemoModule --report-function-dynamic-values 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    $DynamicValueReportOutput -notcontains 'Result: 0' -or
    ($DynamicValueReportOutput -join "`n") -notmatch '(?m)^Function dynamic-bytes=8388653 values=27 kind=bytes\.concat index=1 name=__WvM1F0$' -or
    ($DynamicValueReportOutput -join "`n") -notmatch '(?m)^Function dynamic-bytes=15 values=4 kind=bytes\.concat index=2 name=__WvM1F1$' -or
    ($DynamicValueReportOutput -join "`n") -notmatch '(?m)^Function dynamic-bytes=4 values=4 kind=bytes\.from_u8 index=1 name=__WvM1F0$' -or
    [regex]::Matches(($DynamicValueReportOutput -join "`n"), '(?m)^Function dynamic-bytes=').Count -ne 3
) {
    throw 'The Seed CLI did not report deterministic per-function dynamic-value construction pressure.'
}
$DynamicLifetimeOutput = dotnet $ToolDll run $ByteConstructionDemoModule --report-dynamic-lifetime 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    $DynamicLifetimeOutput -notcontains 'Result: 0' -or
    ($DynamicLifetimeOutput -join "`n") -notmatch '(?m)^Dynamic lifetime constructed-bytes=8388672 constructed-values=35 peak-live-bytes=6291475 peak-live-values=5 peak-operation-bytes=6291475 peak-operation-values=5 retained-bytes=0 retained-values=0 kind=bytes\.concat index=1 name=__WvM1F0$'
) {
    throw 'The Seed CLI did not report deterministic dynamic-value lifetime pressure.'
}
$DynamicAllocatorOutput = dotnet $ToolDll run $ByteConstructionDemoModule --report-dynamic-allocator 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    $DynamicAllocatorOutput -notcontains 'Result: 0' -or
    ($DynamicAllocatorOutput -join "`n") -notmatch '(?m)^Dynamic allocator arena-bytes=16777216 header-bytes=16 alignment-bytes=16 allocations=35 reused=12 peak-payload-bytes=6291475 peak-charged-bytes=6291600 peak-blocks=5 maximum-addressed-bytes=8389040 peak-fragmentation-bytes=4194640 maximum-free-spans=3 failed=0 first-failure-payload-bytes=0 first-failure-charged-bytes=0 first-failure-largest-free-span-bytes=0 retained-blocks=0 retained-charged-bytes=0$'
) {
    throw 'The Seed CLI did not report deterministic first-fit dynamic allocation evidence.'
}

$NativeStencilSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Stencil-Core.wv'
dotnet $ToolDll `
    compile $NativeStencilSource -o $NativeStencilModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native-stencil core.' }
$NativeStencilHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeStencilModule).Hash.ToLowerInvariant()
if ($NativeStencilHash -ne '6df3c524d0f9bec79cd2516a758985c487cc237c6f94bc5b80e015975d50cca3') {
    throw "The Windvale native-stencil core has an unexpected digest: $NativeStencilHash"
}
$NativeStencilInspection = (dotnet $ToolDll inspect $NativeStencilModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeStencilInspection -notmatch 'Nativeˉstencilˉresult' -or
    $NativeStencilInspection -notmatch 'Nativeˉstencilˉpatchˉkind' -or
    $NativeStencilInspection -notmatch 'Nativeˉstencilˉprocessˉargumentˉcount' -or
    $NativeStencilInspection -notmatch 'Nativeˉstencilˉprocessˉargument' -or
    $NativeStencilInspection -notmatch 'Exports \(20\)'
) {
    throw 'The Windvale native-stencil core inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Native-Stencil-Demo.wv') `
    --module $NativeStencilSource `
    -o $NativeStencilDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native-stencil demo.' }
$NativeStencilDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeStencilDemoModule).Hash.ToLowerInvariant()
if ($NativeStencilDemoHash -ne '6b27fbd10d5f06855354f433ec0b8c9b1af1761ef04458817931e675c26e0da8') {
    throw "The Windvale native-stencil demo has an unexpected digest: $NativeStencilDemoHash"
}
$NativeStencilDemoOutput = dotnet $ToolDll `
    run $NativeStencilDemoModule --max-steps 20000000
if ($LASTEXITCODE -ne 0 -or $NativeStencilDemoOutput -notcontains 'Result: 0') {
    throw 'The Windvale native-stencil demo did not return Result: 0.'
}
$NativeStencilBridgeSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Stencil-Bridge.wv'
$NativeStencilBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Stencil-Bridge.wvb'
$NativeArgumentCountLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Argument-Count-Service.bin'
$NativeArgumentLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Argument-Service.bin'
dotnet $ToolDll `
    compile $NativeStencilBridgeSource `
    --module $NativeStencilSource `
    -o $NativeStencilBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native-stencil bridge.' }
$NativeStencilBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeStencilBridgeModule).Hash.ToLowerInvariant()
if ($NativeStencilBridgeHash -ne '0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da') {
    throw "The Windvale native-stencil bridge has an unexpected digest: $NativeStencilBridgeHash"
}
$NativeStencilBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeStencilBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeStencilBridgeRetainedHash -ne $NativeStencilBridgeHash -or
    (Get-Item -LiteralPath $NativeStencilBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeStencilBridgeModule).Length
) {
    throw 'The retained Windvale native-stencil bridge does not match its exact source compilation.'
}
$NativeArgumentCountLeafHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeArgumentCountLeafRetained).Hash.ToLowerInvariant()
if (
    $NativeArgumentCountLeafHash -ne '2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829' -or
    (Get-Item -LiteralPath $NativeArgumentCountLeafRetained).Length -ne 5
) {
    throw "The retained Windvale process-argument-count leaf has an unexpected identity: $NativeArgumentCountLeafHash"
}
$NativeArgumentLeafHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeArgumentLeafRetained).Hash.ToLowerInvariant()
if (
    $NativeArgumentLeafHash -ne '2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1' -or
    (Get-Item -LiteralPath $NativeArgumentLeafRetained).Length -ne 70
) {
    throw "The retained Windvale process-argument leaf has an unexpected identity: $NativeArgumentLeafHash"
}
$NativeStencilBridgeInspection = (dotnet $ToolDll inspect $NativeStencilBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeStencilBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeStencilBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native-stencil bridge inspection is incomplete.'
}

$NativeUtf8CoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Utf8-Service.wv'
dotnet $ToolDll compile $NativeUtf8CoreSource -o $NativeUtf8CoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native UTF-8 service core.' }
$NativeUtf8CoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeUtf8CoreModule).Hash.ToLowerInvariant()
if ($NativeUtf8CoreHash -ne 'adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7') {
    throw "The Windvale native UTF-8 service core has an unexpected digest: $NativeUtf8CoreHash"
}
$NativeUtf8CoreInspection = (dotnet $ToolDll inspect $NativeUtf8CoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeUtf8CoreInspection -notmatch 'Profile: portable' -or
    $NativeUtf8CoreInspection -notmatch 'Nativeˉx64ˉutf8ˉserviceˉbuild' -or
    $NativeUtf8CoreInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native UTF-8 service core inspection is incomplete.'
}
$NativeUtf8BridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Utf8-Service-Bridge.wv'
$NativeUtf8BridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service-Bridge.wvb'
$NativeUtf8LeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service.bin'
dotnet $ToolDll `
    compile $NativeUtf8BridgeSource `
    --module $NativeUtf8CoreSource `
    -o $NativeUtf8BridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native UTF-8 service bridge.' }
$NativeUtf8BridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeUtf8BridgeModule).Hash.ToLowerInvariant()
if ($NativeUtf8BridgeHash -ne '4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f') {
    throw "The Windvale native UTF-8 service bridge has an unexpected digest: $NativeUtf8BridgeHash"
}
$NativeUtf8BridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeUtf8BridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeUtf8BridgeRetainedHash -ne $NativeUtf8BridgeHash -or
    (Get-Item -LiteralPath $NativeUtf8BridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeUtf8BridgeModule).Length
) {
    throw 'The retained Windvale native UTF-8 service bridge does not match its exact source compilation.'
}
$NativeUtf8LeafRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeUtf8LeafRetained).Hash.ToLowerInvariant()
if ($NativeUtf8LeafRetainedHash -ne '4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf' -or
    (Get-Item -LiteralPath $NativeUtf8LeafRetained).Length -ne 800) {
    throw 'The retained Windvale native UTF-8 service leaf has an unexpected exact identity.'
}
$NativeUtf8BridgeInspection = (dotnet $ToolDll inspect $NativeUtf8BridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeUtf8BridgeInspection -notmatch 'Profile: portable' -or
    $NativeUtf8BridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeUtf8BridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native UTF-8 service bridge inspection is incomplete.'
}

$NativeIntegerFormatCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Integer-Format-Services.wv'
dotnet $ToolDll compile $NativeIntegerFormatCoreSource -o $NativeIntegerFormatCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native integer-format service core.' }
$NativeIntegerFormatCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeIntegerFormatCoreModule).Hash.ToLowerInvariant()
if ($NativeIntegerFormatCoreHash -ne '6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2') {
    throw "The Windvale native integer-format service core has an unexpected digest: $NativeIntegerFormatCoreHash"
}
$NativeIntegerFormatCoreInspection = (dotnet $ToolDll inspect $NativeIntegerFormatCoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeIntegerFormatCoreInspection -notmatch 'Profile: portable' -or
    $NativeIntegerFormatCoreInspection -notmatch 'Nativeˉx64ˉintegerˉformatˉserviceˉbuild' -or
    $NativeIntegerFormatCoreInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native integer-format service core inspection is incomplete.'
}
$NativeIntegerFormatBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Integer-Format-Services-Bridge.wv'
$NativeIntegerFormatBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Integer-Format-Services-Bridge.wvb'
$NativeI32FormatLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-I32-Format-Service.bin'
$NativeU32FormatLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-U32-Format-Service.bin'
dotnet $ToolDll `
    compile $NativeIntegerFormatBridgeSource `
    --module $NativeIntegerFormatCoreSource `
    -o $NativeIntegerFormatBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native integer-format service bridge.' }
$NativeIntegerFormatBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeIntegerFormatBridgeModule).Hash.ToLowerInvariant()
if ($NativeIntegerFormatBridgeHash -ne '851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9') {
    throw "The Windvale native integer-format service bridge has an unexpected digest: $NativeIntegerFormatBridgeHash"
}
$NativeIntegerFormatBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeIntegerFormatBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeIntegerFormatBridgeRetainedHash -ne $NativeIntegerFormatBridgeHash -or
    (Get-Item -LiteralPath $NativeIntegerFormatBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeIntegerFormatBridgeModule).Length
) {
    throw 'The retained Windvale native integer-format service bridge does not match its exact source compilation.'
}
$NativeI32FormatLeafRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeI32FormatLeafRetained).Hash.ToLowerInvariant()
$NativeU32FormatLeafRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeU32FormatLeafRetained).Hash.ToLowerInvariant()
if ($NativeI32FormatLeafRetainedHash -ne 'c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e' -or
    (Get-Item -LiteralPath $NativeI32FormatLeafRetained).Length -ne 225 -or
    $NativeU32FormatLeafRetainedHash -ne 'b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43' -or
    (Get-Item -LiteralPath $NativeU32FormatLeafRetained).Length -ne 191) {
    throw 'The retained Windvale native integer-format leaves have unexpected exact identities.'
}
$NativeIntegerFormatBridgeInspection = (dotnet $ToolDll inspect $NativeIntegerFormatBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeIntegerFormatBridgeInspection -notmatch 'Profile: portable' -or
    $NativeIntegerFormatBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeIntegerFormatBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native integer-format service bridge inspection is incomplete.'
}

$NativeServiceCodeBuilderSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Service-Code-Builder.wv'
dotnet $ToolDll compile $NativeServiceCodeBuilderSource -o $NativeServiceCodeBuilderModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native service-code builder.' }
$NativeServiceCodeBuilderHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceCodeBuilderModule).Hash.ToLowerInvariant()
if ($NativeServiceCodeBuilderHash -ne 'adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06') {
    throw "The Windvale native service-code builder has an unexpected digest: $NativeServiceCodeBuilderHash"
}
$NativeServiceCodeBuilderInspection = (dotnet $ToolDll inspect $NativeServiceCodeBuilderModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeServiceCodeBuilderInspection -notmatch 'Profile: portable' -or
    $NativeServiceCodeBuilderInspection -notmatch 'Nativeˉx64ˉserviceˉbuilder' -or
    $NativeServiceCodeBuilderInspection -notmatch 'Nativeˉx64ˉserviceˉfinish' -or
    $NativeServiceCodeBuilderInspection -notmatch 'Exports \(10\)'
) {
    throw 'The Windvale native service-code builder inspection is incomplete.'
}

$NativeWindowsOutputCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Service-Windows.wv'
$NativeLinuxOutputCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Service-Linux.wv'
$NativeOutputBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Services-Bridge.wv'
$NativeOutputBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Output-Services-Bridge.wvb'
$NativeWindowsConsoleOutputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Windows-Console-Output-Service.bin'
$NativeWindowsDiagnosticOutputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Windows-Diagnostic-Output-Service.bin'
$NativeLinuxConsoleOutputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Linux-Console-Output-Service.bin'
$NativeLinuxDiagnosticOutputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Linux-Diagnostic-Output-Service.bin'
dotnet $ToolDll `
    compile $NativeWindowsOutputCoreSource `
    --module $NativeServiceCodeBuilderSource `
    -o $NativeWindowsOutputCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale Windows output-service core.' }
$NativeWindowsOutputCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeWindowsOutputCoreModule).Hash.ToLowerInvariant()
if ($NativeWindowsOutputCoreHash -ne 'a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983') {
    throw "The Windvale Windows output-service core has an unexpected digest: $NativeWindowsOutputCoreHash"
}
dotnet $ToolDll `
    compile $NativeLinuxOutputCoreSource `
    --module $NativeServiceCodeBuilderSource `
    -o $NativeLinuxOutputCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale Linux output-service core.' }
$NativeLinuxOutputCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeLinuxOutputCoreModule).Hash.ToLowerInvariant()
if ($NativeLinuxOutputCoreHash -ne 'd3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad') {
    throw "The Windvale Linux output-service core has an unexpected digest: $NativeLinuxOutputCoreHash"
}
dotnet $ToolDll `
    compile $NativeOutputBridgeSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeLinuxOutputCoreSource `
    --module $NativeWindowsOutputCoreSource `
    -o $NativeOutputBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale output-service bridge.' }
$NativeOutputBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputBridgeModule).Hash.ToLowerInvariant()
if ($NativeOutputBridgeHash -ne '209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed') {
    throw "The Windvale output-service bridge has an unexpected digest: $NativeOutputBridgeHash"
}
$NativeOutputBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeOutputBridgeRetainedHash -ne $NativeOutputBridgeHash -or
    (Get-Item -LiteralPath $NativeOutputBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeOutputBridgeModule).Length
) {
    throw 'The retained Windvale output-service bridge does not match its exact source compilation.'
}
$NativeOutputLeaves = @(
    @($NativeWindowsConsoleOutputLeaf, 258, '10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48'),
    @($NativeWindowsDiagnosticOutputLeaf, 258, '1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2'),
    @($NativeLinuxConsoleOutputLeaf, 213, 'c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226'),
    @($NativeLinuxDiagnosticOutputLeaf, 213, '1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe')
)
foreach ($NativeOutputLeaf in $NativeOutputLeaves) {
    $NativeOutputLeafHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputLeaf[0]).Hash.ToLowerInvariant()
    if ($NativeOutputLeafHash -ne $NativeOutputLeaf[2] -or
        (Get-Item -LiteralPath $NativeOutputLeaf[0]).Length -ne $NativeOutputLeaf[1]) {
        throw "The retained Windvale native output leaf has an unexpected identity: $($NativeOutputLeaf[0])"
    }
}
$NativeOutputBridgeInspection = (dotnet $ToolDll inspect $NativeOutputBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeOutputBridgeInspection -notmatch 'Profile: portable' -or
    $NativeOutputBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeOutputBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale output-service bridge inspection is incomplete.'
}

$NativeFileOutputCodeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Code.wv'
$NativeWindowsFileOutputCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Windows.wv'
$NativeLinuxFileOutputCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Linux.wv'
$NativeFileOutputBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Services-Bridge.wv'
$NativeFileOutputBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-File-Output-Services-Bridge.wvb'
$NativeWindowsFileOutputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Output-Service.bin'
$NativeLinuxFileOutputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Linux-File-Output-Service.bin'
dotnet $ToolDll `
    compile $NativeFileOutputCodeSource `
    --module $NativeServiceCodeBuilderSource `
    -o $NativeFileOutputCodeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the shared Windvale file-output code module.' }
$NativeFileOutputCodeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputCodeModule).Hash.ToLowerInvariant()
if ($NativeFileOutputCodeHash -ne '7ed9baf3a21912933045b99cb82d22d73620a318a716931db86670e5ea2212c6') {
    throw "The shared Windvale file-output code module has an unexpected digest: $NativeFileOutputCodeHash"
}
dotnet $ToolDll `
    compile $NativeWindowsFileOutputCoreSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeFileOutputCodeSource `
    -o $NativeWindowsFileOutputCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale Windows file-output core.' }
$NativeWindowsFileOutputCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeWindowsFileOutputCoreModule).Hash.ToLowerInvariant()
if ($NativeWindowsFileOutputCoreHash -ne '9ca03bf6f5b8678389c81e281438160ff4c96c86f11a048aba90238fdc81a45d') {
    throw "The Windvale Windows file-output core has an unexpected digest: $NativeWindowsFileOutputCoreHash"
}
dotnet $ToolDll `
    compile $NativeLinuxFileOutputCoreSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeFileOutputCodeSource `
    -o $NativeLinuxFileOutputCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale Linux file-output core.' }
$NativeLinuxFileOutputCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeLinuxFileOutputCoreModule).Hash.ToLowerInvariant()
if ($NativeLinuxFileOutputCoreHash -ne '834d0c45b85b26ffd3ee43e49a85c8c4ffa08f36581c02785729b276eeccdb48') {
    throw "The Windvale Linux file-output core has an unexpected digest: $NativeLinuxFileOutputCoreHash"
}
dotnet $ToolDll `
    compile $NativeFileOutputBridgeSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeFileOutputCodeSource `
    --module $NativeWindowsFileOutputCoreSource `
    --module $NativeLinuxFileOutputCoreSource `
    -o $NativeFileOutputBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale file-output bridge.' }
$NativeFileOutputBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputBridgeModule).Hash.ToLowerInvariant()
if ($NativeFileOutputBridgeHash -ne '441db0e0e5a90f98c7e4b12b17086f56487e7d754d7b6378a0eb2972591e64f6') {
    throw "The Windvale file-output bridge has an unexpected digest: $NativeFileOutputBridgeHash"
}
$NativeFileOutputBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeFileOutputBridgeRetainedHash -ne $NativeFileOutputBridgeHash -or
    (Get-Item -LiteralPath $NativeFileOutputBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeFileOutputBridgeModule).Length
) {
    throw 'The retained Windvale file-output bridge does not match its exact source compilation.'
}
$NativeFileOutputLeaves = @(
    @($NativeWindowsFileOutputLeaf, 787, 'a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1'),
    @($NativeLinuxFileOutputLeaf, 823, 'fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422')
)
foreach ($NativeFileOutputLeaf in $NativeFileOutputLeaves) {
    $NativeFileOutputLeafHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputLeaf[0]).Hash.ToLowerInvariant()
    if ($NativeFileOutputLeafHash -ne $NativeFileOutputLeaf[2] -or
        (Get-Item -LiteralPath $NativeFileOutputLeaf[0]).Length -ne $NativeFileOutputLeaf[1]) {
        throw "The retained Windvale native file-output leaf has an unexpected identity: $($NativeFileOutputLeaf[0])"
    }
}
$NativeFileOutputBridgeInspection = (dotnet $ToolDll inspect $NativeFileOutputBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeFileOutputBridgeInspection -notmatch 'Profile: portable' -or
    $NativeFileOutputBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeFileOutputBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale file-output bridge inspection is incomplete.'
}

$NativeFileInputCodeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Code.wv'
$NativeWindowsFileInputCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Windows.wv'
$NativeLinuxFileInputCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Linux.wv'
$NativeFileInputBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Services-Bridge.wv'
$NativeFileInputBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-File-Input-Services-Bridge.wvb'
$NativeWindowsFileInputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Input-Service.bin'
$NativeLinuxFileInputLeaf = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Linux-File-Input-Service.bin'
dotnet $ToolDll `
    compile $NativeFileInputCodeSource `
    --module $NativeServiceCodeBuilderSource `
    -o $NativeFileInputCodeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the shared Windvale file-input code module.' }
$NativeFileInputCodeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputCodeModule).Hash.ToLowerInvariant()
if ($NativeFileInputCodeHash -ne 'e2bfd4521b8f22529f3747eef196bdf7fa7aa0e97644db23ed45939aa10a1a7a') {
    throw "The shared Windvale file-input code module has an unexpected digest: $NativeFileInputCodeHash"
}
dotnet $ToolDll `
    compile $NativeWindowsFileInputCoreSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeFileInputCodeSource `
    -o $NativeWindowsFileInputCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale Windows file-input core.' }
$NativeWindowsFileInputCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeWindowsFileInputCoreModule).Hash.ToLowerInvariant()
if ($NativeWindowsFileInputCoreHash -ne '795e45f1efb6c8a864962fc01a6e8374d758fa4e098eda8900b6639a02b8cf2e') {
    throw "The Windvale Windows file-input core has an unexpected digest: $NativeWindowsFileInputCoreHash"
}
dotnet $ToolDll `
    compile $NativeLinuxFileInputCoreSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeFileInputCodeSource `
    -o $NativeLinuxFileInputCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale Linux file-input core.' }
$NativeLinuxFileInputCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeLinuxFileInputCoreModule).Hash.ToLowerInvariant()
if ($NativeLinuxFileInputCoreHash -ne '49b7990dd51ae3108f387331d907f4127c18726d95a453a8f768459016ad497e') {
    throw "The Windvale Linux file-input core has an unexpected digest: $NativeLinuxFileInputCoreHash"
}
dotnet $ToolDll `
    compile $NativeFileInputBridgeSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeFileInputCodeSource `
    --module $NativeWindowsFileInputCoreSource `
    --module $NativeLinuxFileInputCoreSource `
    -o $NativeFileInputBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale file-input bridge.' }
$NativeFileInputBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputBridgeModule).Hash.ToLowerInvariant()
if ($NativeFileInputBridgeHash -ne '81cb5ed76e0e885055b13ae23bfbca118c99c7ea905d3ae75a5bc87ccb35269b') {
    throw "The Windvale file-input bridge has an unexpected digest: $NativeFileInputBridgeHash"
}
$NativeFileInputBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeFileInputBridgeRetainedHash -ne $NativeFileInputBridgeHash -or
    (Get-Item -LiteralPath $NativeFileInputBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeFileInputBridgeModule).Length
) {
    throw 'The retained Windvale file-input bridge does not match its exact source compilation.'
}
$NativeFileInputLeaves = @(
    @($NativeWindowsFileInputLeaf, 1218, '3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8'),
    @($NativeLinuxFileInputLeaf, 996, '55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb')
)
foreach ($NativeFileInputLeaf in $NativeFileInputLeaves) {
    $NativeFileInputLeafHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputLeaf[0]).Hash.ToLowerInvariant()
    if ($NativeFileInputLeafHash -ne $NativeFileInputLeaf[2] -or
        (Get-Item -LiteralPath $NativeFileInputLeaf[0]).Length -ne $NativeFileInputLeaf[1]) {
        throw "The retained Windvale native file-input leaf has an unexpected identity: $($NativeFileInputLeaf[0])"
    }
}
$NativeFileInputBridgeInspection = (dotnet $ToolDll inspect $NativeFileInputBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeFileInputBridgeInspection -notmatch 'Profile: portable' -or
    $NativeFileInputBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeFileInputBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale file-input bridge inspection is incomplete.'
}

$NativeTextConcatCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Concat-Service.wv'
dotnet $ToolDll `
    compile $NativeTextConcatCoreSource `
    --module $NativeServiceCodeBuilderSource `
    -o $NativeTextConcatCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native text-concatenation service core.' }
$NativeTextConcatCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextConcatCoreModule).Hash.ToLowerInvariant()
if ($NativeTextConcatCoreHash -ne '6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73') {
    throw "The Windvale native text-concatenation service core has an unexpected digest: $NativeTextConcatCoreHash"
}
$NativeTextConcatCoreInspection = (dotnet $ToolDll inspect $NativeTextConcatCoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeTextConcatCoreInspection -notmatch 'Profile: portable' -or
    $NativeTextConcatCoreInspection -notmatch 'Nativeˉx64ˉtextˉconcatˉserviceˉbuild' -or
    $NativeTextConcatCoreInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native text-concatenation service core inspection is incomplete.'
}

$NativeTextConcatBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Concat-Service-Bridge.wv'
$NativeTextConcatBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Text-Concat-Service-Bridge.wvb'
$NativeTextConcatLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Text-Concat-Service.bin'
dotnet $ToolDll `
    compile $NativeTextConcatBridgeSource `
    --module $NativeServiceCodeBuilderSource `
    --module $NativeTextConcatCoreSource `
    -o $NativeTextConcatBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native text-concatenation service bridge.' }
$NativeTextConcatBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextConcatBridgeModule).Hash.ToLowerInvariant()
if ($NativeTextConcatBridgeHash -ne '87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08') {
    throw "The Windvale native text-concatenation service bridge has an unexpected digest: $NativeTextConcatBridgeHash"
}
$NativeTextConcatBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextConcatBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeTextConcatBridgeRetainedHash -ne $NativeTextConcatBridgeHash -or
    (Get-Item -LiteralPath $NativeTextConcatBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeTextConcatBridgeModule).Length
) {
    throw 'The retained Windvale native text-concatenation service bridge does not match its exact source compilation.'
}
$NativeTextConcatLeafRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextConcatLeafRetained).Hash.ToLowerInvariant()
if ($NativeTextConcatLeafRetainedHash -ne '75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0' -or
    (Get-Item -LiteralPath $NativeTextConcatLeafRetained).Length -ne 249) {
    throw 'The retained Windvale native text-concatenation leaf has an unexpected exact identity.'
}
$NativeTextConcatBridgeInspection = (dotnet $ToolDll inspect $NativeTextConcatBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeTextConcatBridgeInspection -notmatch 'Profile: portable' -or
    $NativeTextConcatBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeTextConcatBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native text-concatenation service bridge inspection is incomplete.'
}

$NativeTextQuoteCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Quote-Service.wv'
dotnet $ToolDll compile $NativeTextQuoteCoreSource -o $NativeTextQuoteCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native text-quote service core.' }
$NativeTextQuoteCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextQuoteCoreModule).Hash.ToLowerInvariant()
if ($NativeTextQuoteCoreHash -ne 'b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453') {
    throw "The Windvale native text-quote service core has an unexpected digest: $NativeTextQuoteCoreHash"
}
$NativeTextQuoteCoreInspection = (dotnet $ToolDll inspect $NativeTextQuoteCoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeTextQuoteCoreInspection -notmatch 'Profile: portable' -or
    $NativeTextQuoteCoreInspection -notmatch 'Nativeˉx64ˉtextˉquoteˉleaf: bytes length=1165' -or
    $NativeTextQuoteCoreInspection -notmatch 'Nativeˉx64ˉtextˉquoteˉserviceˉbuild' -or
    $NativeTextQuoteCoreInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native text-quote service core inspection is incomplete.'
}

$NativeTextQuoteBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Quote-Service-Bridge.wv'
$NativeTextQuoteBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Text-Quote-Service-Bridge.wvb'
$NativeTextQuoteLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Text-Quote-Service.bin'
dotnet $ToolDll `
    compile $NativeTextQuoteBridgeSource `
    --module $NativeTextQuoteCoreSource `
    -o $NativeTextQuoteBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native text-quote service bridge.' }
$NativeTextQuoteBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextQuoteBridgeModule).Hash.ToLowerInvariant()
if ($NativeTextQuoteBridgeHash -ne '306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631') {
    throw "The Windvale native text-quote service bridge has an unexpected digest: $NativeTextQuoteBridgeHash"
}
$NativeTextQuoteBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextQuoteBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeTextQuoteBridgeRetainedHash -ne $NativeTextQuoteBridgeHash -or
    (Get-Item -LiteralPath $NativeTextQuoteBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeTextQuoteBridgeModule).Length
) {
    throw 'The retained Windvale native text-quote service bridge does not match its exact source compilation.'
}
$NativeTextQuoteLeafRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeTextQuoteLeafRetained).Hash.ToLowerInvariant()
if ($NativeTextQuoteLeafRetainedHash -ne '4f334af9b6349437d36fd703edb6b5882416f033fae47906a40a4bafdc083bb7' -or
    (Get-Item -LiteralPath $NativeTextQuoteLeafRetained).Length -ne 1165) {
    throw 'The retained Windvale native text-quote leaf has an unexpected exact identity.'
}
$NativeTextQuoteBridgeInspection = (dotnet $ToolDll inspect $NativeTextQuoteBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeTextQuoteBridgeInspection -notmatch 'Profile: portable' -or
    $NativeTextQuoteBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeTextQuoteBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native text-quote service bridge inspection is incomplete.'
}

$NativeEnumNameCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Enum-Name-Service.wv'
dotnet $ToolDll compile $NativeEnumNameCoreSource -o $NativeEnumNameCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native enum-name service core.' }
$NativeEnumNameCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumNameCoreModule).Hash.ToLowerInvariant()
if ($NativeEnumNameCoreHash -ne 'b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948') {
    throw "The Windvale native enum-name service core has an unexpected digest: $NativeEnumNameCoreHash"
}
$NativeEnumNameCoreInspection = (dotnet $ToolDll inspect $NativeEnumNameCoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeEnumNameCoreInspection -notmatch 'Profile: portable' -or
    $NativeEnumNameCoreInspection -notmatch 'Nativeˉx64ˉenumˉnameˉleaf: bytes length=323' -or
    $NativeEnumNameCoreInspection -notmatch 'Nativeˉx64ˉenumˉnameˉserviceˉbuild' -or
    $NativeEnumNameCoreInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native enum-name service core inspection is incomplete.'
}

$NativeEnumNameBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Enum-Name-Service-Bridge.wv'
$NativeEnumNameBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Enum-Name-Service-Bridge.wvb'
$NativeEnumNameLeafRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-X64-Enum-Name-Service.bin'
dotnet $ToolDll `
    compile $NativeEnumNameBridgeSource `
    --module $NativeEnumNameCoreSource `
    -o $NativeEnumNameBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native enum-name service bridge.' }
$NativeEnumNameBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumNameBridgeModule).Hash.ToLowerInvariant()
if ($NativeEnumNameBridgeHash -ne '46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c') {
    throw "The Windvale native enum-name service bridge has an unexpected digest: $NativeEnumNameBridgeHash"
}
$NativeEnumNameBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumNameBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeEnumNameBridgeRetainedHash -ne $NativeEnumNameBridgeHash -or
    (Get-Item -LiteralPath $NativeEnumNameBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeEnumNameBridgeModule).Length
) {
    throw 'The retained Windvale native enum-name service bridge does not match its exact source compilation.'
}
$NativeEnumNameLeafRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumNameLeafRetained).Hash.ToLowerInvariant()
if (
    $NativeEnumNameLeafRetainedHash -ne 'fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a' -or
    (Get-Item -LiteralPath $NativeEnumNameLeafRetained).Length -ne 323
) {
    throw 'The retained Windvale native enum-name leaf has an unexpected exact identity.'
}
$NativeEnumNameBridgeInspection = (dotnet $ToolDll inspect $NativeEnumNameBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeEnumNameBridgeInspection -notmatch 'Profile: portable' -or
    $NativeEnumNameBridgeInspection -notmatch 'Main\(\) -> bytes' -or
    $NativeEnumNameBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native enum-name service bridge inspection is incomplete.'
}

$NativeEnumMetadataCoreSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Enum-Metadata-Core.wv'
dotnet $ToolDll compile $NativeEnumMetadataCoreSource -o $NativeEnumMetadataCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native enum-metadata core.' }
$NativeEnumMetadataCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumMetadataCoreModule).Hash.ToLowerInvariant()
if ($NativeEnumMetadataCoreHash -ne '9c61f7d436854ace71ab17fcf33da73c40d37d612f68ba08bfa929ab4e710ef1') {
    throw "The Windvale native enum-metadata core has an unexpected digest: $NativeEnumMetadataCoreHash"
}
$NativeEnumMetadataCoreInspection = (dotnet $ToolDll inspect $NativeEnumMetadataCoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeEnumMetadataCoreInspection -notmatch 'Profile: portable' -or
    $NativeEnumMetadataCoreInspection -notmatch 'Nativeˉenumˉmetadataˉbuild' -or
    $NativeEnumMetadataCoreInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native enum-metadata core inspection is incomplete.'
}

$NativeEnumMetadataBridgeSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Enum-Metadata-Bridge.wv'
$NativeEnumMetadataBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Enum-Metadata-Bridge.wvb'
$NativeEnumMetadataArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Enum-Metadata-Bridge.wvnf'
dotnet $ToolDll `
    compile $NativeEnumMetadataBridgeSource `
    --module $NativeEnumMetadataCoreSource `
    -o $NativeEnumMetadataBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native enum-metadata bridge.' }
$NativeEnumMetadataBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumMetadataBridgeModule).Hash.ToLowerInvariant()
if ($NativeEnumMetadataBridgeHash -ne 'a43a89cedd7fc58740132c2f666ea69866ceff6ebb87d090124207ff3e9154ce') {
    throw "The Windvale native enum-metadata bridge has an unexpected digest: $NativeEnumMetadataBridgeHash"
}
$NativeEnumMetadataBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumMetadataBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeEnumMetadataBridgeRetainedHash -ne $NativeEnumMetadataBridgeHash -or
    (Get-Item -LiteralPath $NativeEnumMetadataBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeEnumMetadataBridgeModule).Length
) {
    throw 'The retained Windvale native enum-metadata bridge does not match its exact source compilation.'
}
$NativeEnumMetadataArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeEnumMetadataArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativeEnumMetadataArtifactHash -ne 'd2f53cd0fdd7812699a06234e19586f18492ffbca68ae0e5f507b09253c5a39b' -or
    (Get-Item -LiteralPath $NativeEnumMetadataArtifactRetained).Length -ne 115167
) {
    throw "The retained Windvale native enum-metadata fragment has an unexpected identity: $NativeEnumMetadataArtifactHash"
}
$NativeEnumMetadataBridgeInspection = (dotnet $ToolDll inspect $NativeEnumMetadataBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeEnumMetadataBridgeInspection -notmatch 'Profile: portable' -or
    $NativeEnumMetadataBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativeEnumMetadataBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native enum-metadata bridge inspection is incomplete.'
}

$NativePublicationSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Core.wv'
dotnet $ToolDll `
    compile $NativePublicationSource -o $NativePublicationModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native-publication core.' }
$NativePublicationHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationModule).Hash.ToLowerInvariant()
if ($NativePublicationHash -ne '3048902ce708d6e640d484507efc1d567399bcafed6e2c133ca2827aff83189f') {
    throw "The Windvale native-publication core has an unexpected digest: $NativePublicationHash"
}
$NativePublicationInspection = (dotnet $ToolDll inspect $NativePublicationModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativePublicationInspection -notmatch 'Profile: portable' -or
    $NativePublicationInspection -notmatch 'Nativeˉpublicationˉresult' -or
    $NativePublicationInspection -notmatch 'Nativeˉpublicationˉstatus' -or
    $NativePublicationInspection -notmatch 'Nativeˉpublicationˉplan' -or
    $NativePublicationInspection -notmatch 'Exports \(8\)'
) {
    throw 'The Windvale native-publication core inspection is incomplete.'
}
$NativePublicationBridgeSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Bridge.wv'
$NativePublicationBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Publication-Bridge.wvb'
$NativePublicationArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Publication-Bridge.wvnf'
dotnet $ToolDll `
    compile $NativePublicationBridgeSource `
    --module $NativePublicationSource `
    -o $NativePublicationBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native-publication bridge.' }
$NativePublicationBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationBridgeModule).Hash.ToLowerInvariant()
if ($NativePublicationBridgeHash -ne '111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c') {
    throw "The Windvale native-publication bridge has an unexpected digest: $NativePublicationBridgeHash"
}
$NativePublicationBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativePublicationBridgeRetainedHash -ne $NativePublicationBridgeHash -or
    (Get-Item -LiteralPath $NativePublicationBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativePublicationBridgeModule).Length
) {
    throw 'The retained Windvale native-publication bridge does not match its exact source compilation.'
}
$NativePublicationArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativePublicationArtifactHash -ne '9deeb8c4ab8f080cbc187036e0b015932379956930ec9cd1b7f51f7d1daa1f47' -or
    (Get-Item -LiteralPath $NativePublicationArtifactRetained).Length -ne 61583
) {
    throw "The retained Windvale native-publication fragment has an unexpected identity: $NativePublicationArtifactHash"
}
$NativePublicationBridgeInspection = (dotnet $ToolDll inspect $NativePublicationBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativePublicationBridgeInspection -notmatch 'Profile: portable' -or
    $NativePublicationBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativePublicationBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativePublicationBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native-publication bridge inspection is incomplete.'
}

$NativeServiceBundleMaterializationCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv'
$NativeServiceBundleMaterializationBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Bundle-Materialization-Bridge.wv'
$NativeServiceBundleMaterializationBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Service-Bundle-Materialization-Bridge.wvb'
$NativeServiceBundleMaterializationArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Service-Bundle-Materialization-Bridge.wvnf'
dotnet $ToolDll `
    compile $NativeServiceBundleMaterializationCoreSource `
    --module $NativePublicationSource `
    --module $ByteConstructionSource `
    -o $NativeServiceBundleMaterializationCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale service-bundle materialization core.' }
$NativeServiceBundleMaterializationCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceBundleMaterializationCoreModule).Hash.ToLowerInvariant()
if ($NativeServiceBundleMaterializationCoreHash -ne '97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008') {
    throw "The Windvale service-bundle materialization core has an unexpected digest: $NativeServiceBundleMaterializationCoreHash"
}
dotnet $ToolDll `
    compile $NativeServiceBundleMaterializationBridgeSource `
    --module $NativePublicationSource `
    --module $ByteConstructionSource `
    --module $NativeServiceBundleMaterializationCoreSource `
    -o $NativeServiceBundleMaterializationBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale service-bundle materialization bridge.' }
$NativeServiceBundleMaterializationBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceBundleMaterializationBridgeModule).Hash.ToLowerInvariant()
if ($NativeServiceBundleMaterializationBridgeHash -ne '327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902') {
    throw "The Windvale service-bundle materialization bridge has an unexpected digest: $NativeServiceBundleMaterializationBridgeHash"
}
$NativeServiceBundleMaterializationBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceBundleMaterializationBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeServiceBundleMaterializationBridgeRetainedHash -ne $NativeServiceBundleMaterializationBridgeHash -or
    (Get-Item -LiteralPath $NativeServiceBundleMaterializationBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeServiceBundleMaterializationBridgeModule).Length
) {
    throw 'The retained Windvale service-bundle materialization bridge does not match its exact source compilation.'
}
$NativeServiceBundleMaterializationArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceBundleMaterializationArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativeServiceBundleMaterializationArtifactHash -ne 'd0b12e426e891f6ee78209ab817dde7c547c0f68541750d39dd665607434e7a9' -or
    (Get-Item -LiteralPath $NativeServiceBundleMaterializationArtifactRetained).Length -ne 179452
) {
    throw "The retained Windvale service-bundle materialization fragment has an unexpected identity: $NativeServiceBundleMaterializationArtifactHash"
}
$NativeServiceBundleMaterializationBridgeInspection = (dotnet $ToolDll inspect $NativeServiceBundleMaterializationBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeServiceBundleMaterializationBridgeInspection -notmatch 'Profile: portable' -or
    $NativeServiceBundleMaterializationBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativeServiceBundleMaterializationBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativeServiceBundleMaterializationBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale service-bundle materialization bridge inspection is incomplete.'
}

$NativeOutputTableCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Output-Table-Core.wv'
$NativeOutputTableBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Output-Table-Bridge.wv'
$NativeOutputTableBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Output-Table-Bridge.wvb'
$NativeOutputTableArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Output-Table-Bridge.wvnf'
dotnet $ToolDll compile $NativeOutputTableCoreSource -o $NativeOutputTableCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native output-table core.' }
$NativeOutputTableCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputTableCoreModule).Hash.ToLowerInvariant()
if ($NativeOutputTableCoreHash -ne 'ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8') {
    throw "The Windvale native output-table core has an unexpected digest: $NativeOutputTableCoreHash"
}
dotnet $ToolDll `
    compile $NativeOutputTableBridgeSource `
    --module $NativeOutputTableCoreSource `
    -o $NativeOutputTableBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native output-table bridge.' }
$NativeOutputTableBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputTableBridgeModule).Hash.ToLowerInvariant()
if ($NativeOutputTableBridgeHash -ne 'b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8') {
    throw "The Windvale native output-table bridge has an unexpected digest: $NativeOutputTableBridgeHash"
}
$NativeOutputTableBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputTableBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeOutputTableBridgeRetainedHash -ne $NativeOutputTableBridgeHash -or
    (Get-Item -LiteralPath $NativeOutputTableBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeOutputTableBridgeModule).Length
) {
    throw 'The retained Windvale native output-table bridge does not match its exact source compilation.'
}
$NativeOutputTableArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeOutputTableArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativeOutputTableArtifactHash -ne 'f444e80b2afbaaee251892ab7a7a6a879b3e5cffcbf029b0fc382b64bef97afb' -or
    (Get-Item -LiteralPath $NativeOutputTableArtifactRetained).Length -ne 50493
) {
    throw "The retained Windvale native output-table fragment has an unexpected identity: $NativeOutputTableArtifactHash"
}
$NativeOutputTableBridgeInspection = (dotnet $ToolDll inspect $NativeOutputTableBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeOutputTableBridgeInspection -notmatch 'Profile: portable' -or
    $NativeOutputTableBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativeOutputTableBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativeOutputTableBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native output-table bridge inspection is incomplete.'
}

$NativeFileOutputTableCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Output-Table-Core.wv'
$NativeFileOutputTableBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Output-Table-Bridge.wv'
$NativeFileOutputTableBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-File-Output-Table-Bridge.wvb'
$NativeFileOutputTableArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-File-Output-Table-Bridge.wvnf'
dotnet $ToolDll compile $NativeFileOutputTableCoreSource -o $NativeFileOutputTableCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native file-output-table core.' }
$NativeFileOutputTableCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputTableCoreModule).Hash.ToLowerInvariant()
if ($NativeFileOutputTableCoreHash -ne 'fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f') {
    throw "The Windvale native file-output-table core has an unexpected digest: $NativeFileOutputTableCoreHash"
}
dotnet $ToolDll `
    compile $NativeFileOutputTableBridgeSource `
    --module $NativeFileOutputTableCoreSource `
    -o $NativeFileOutputTableBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native file-output-table bridge.' }
$NativeFileOutputTableBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputTableBridgeModule).Hash.ToLowerInvariant()
if ($NativeFileOutputTableBridgeHash -ne '94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06') {
    throw "The Windvale native file-output-table bridge has an unexpected digest: $NativeFileOutputTableBridgeHash"
}
$NativeFileOutputTableBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputTableBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeFileOutputTableBridgeRetainedHash -ne $NativeFileOutputTableBridgeHash -or
    (Get-Item -LiteralPath $NativeFileOutputTableBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeFileOutputTableBridgeModule).Length
) {
    throw 'The retained Windvale native file-output-table bridge does not match its exact source compilation.'
}
$NativeFileOutputTableArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileOutputTableArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativeFileOutputTableArtifactHash -ne '9333d4573b87b829e6e577d8a27c937bf2fb433a93d4a4b11b783b372d31d08a' -or
    (Get-Item -LiteralPath $NativeFileOutputTableArtifactRetained).Length -ne 42302
) {
    throw "The retained Windvale native file-output-table fragment has an unexpected identity: $NativeFileOutputTableArtifactHash"
}
$NativeFileOutputTableBridgeInspection = (dotnet $ToolDll inspect $NativeFileOutputTableBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeFileOutputTableBridgeInspection -notmatch 'Profile: portable' -or
    $NativeFileOutputTableBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativeFileOutputTableBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativeFileOutputTableBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native file-output-table bridge inspection is incomplete.'
}

$NativeFileInputTableCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Input-Table-Core.wv'
$NativeFileInputTableBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Input-Table-Bridge.wv'
$NativeFileInputTableBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-File-Input-Table-Bridge.wvb'
$NativeFileInputTableArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-File-Input-Table-Bridge.wvnf'
dotnet $ToolDll compile $NativeFileInputTableCoreSource -o $NativeFileInputTableCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native file-input-table core.' }
$NativeFileInputTableCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputTableCoreModule).Hash.ToLowerInvariant()
if ($NativeFileInputTableCoreHash -ne '0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438') {
    throw "The Windvale native file-input-table core has an unexpected digest: $NativeFileInputTableCoreHash"
}
dotnet $ToolDll `
    compile $NativeFileInputTableBridgeSource `
    --module $NativeFileInputTableCoreSource `
    -o $NativeFileInputTableBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native file-input-table bridge.' }
$NativeFileInputTableBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputTableBridgeModule).Hash.ToLowerInvariant()
if ($NativeFileInputTableBridgeHash -ne 'e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9') {
    throw "The Windvale native file-input-table bridge has an unexpected digest: $NativeFileInputTableBridgeHash"
}
$NativeFileInputTableBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputTableBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeFileInputTableBridgeRetainedHash -ne $NativeFileInputTableBridgeHash -or
    (Get-Item -LiteralPath $NativeFileInputTableBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeFileInputTableBridgeModule).Length
) {
    throw 'The retained Windvale native file-input-table bridge does not match its exact source compilation.'
}
$NativeFileInputTableArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeFileInputTableArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativeFileInputTableArtifactHash -ne '378240d8f8770a4707d7f2ae86daae24036fc2eb9fd273d5ab737c9c03e3e70d' -or
    (Get-Item -LiteralPath $NativeFileInputTableArtifactRetained).Length -ne 52334
) {
    throw "The retained Windvale native file-input-table fragment has an unexpected identity: $NativeFileInputTableArtifactHash"
}
$NativeFileInputTableBridgeInspection = (dotnet $ToolDll inspect $NativeFileInputTableBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeFileInputTableBridgeInspection -notmatch 'Profile: portable' -or
    $NativeFileInputTableBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativeFileInputTableBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativeFileInputTableBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native file-input-table bridge inspection is incomplete.'
}

$NativeServiceTableCoreSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Table-Core.wv'
$NativeServiceTableBridgeSource = Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Table-Bridge.wv'
$NativeServiceTableBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Service-Table-Bridge.wvb'
$NativeServiceTableArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Service-Table-Bridge.wvnf'
dotnet $ToolDll compile $NativeServiceTableCoreSource -o $NativeServiceTableCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native service-table core.' }
$NativeServiceTableCoreHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceTableCoreModule).Hash.ToLowerInvariant()
if ($NativeServiceTableCoreHash -ne 'ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26') {
    throw "The Windvale native service-table core has an unexpected digest: $NativeServiceTableCoreHash"
}
dotnet $ToolDll `
    compile $NativeServiceTableBridgeSource `
    --module $NativeServiceTableCoreSource `
    -o $NativeServiceTableBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native service-table bridge.' }
$NativeServiceTableBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceTableBridgeModule).Hash.ToLowerInvariant()
if ($NativeServiceTableBridgeHash -ne '04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b') {
    throw "The Windvale native service-table bridge has an unexpected digest: $NativeServiceTableBridgeHash"
}
$NativeServiceTableBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceTableBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativeServiceTableBridgeRetainedHash -ne $NativeServiceTableBridgeHash -or
    (Get-Item -LiteralPath $NativeServiceTableBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativeServiceTableBridgeModule).Length
) {
    throw 'The retained Windvale native service-table bridge does not match its exact source compilation.'
}
$NativeServiceTableArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativeServiceTableArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativeServiceTableArtifactHash -ne 'e1b838652150999d13b84cd6f1c527b4e82923190530f707ef8d163d39a1f58e' -or
    (Get-Item -LiteralPath $NativeServiceTableArtifactRetained).Length -ne 34830
) {
    throw "The retained Windvale native service-table fragment has an unexpected identity: $NativeServiceTableArtifactHash"
}
$NativeServiceTableBridgeInspection = (dotnet $ToolDll inspect $NativeServiceTableBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativeServiceTableBridgeInspection -notmatch 'Profile: portable' -or
    $NativeServiceTableBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativeServiceTableBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativeServiceTableBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native service-table bridge inspection is incomplete.'
}

$NativePublicationLifetimeSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Lifetime-Core.wv'
dotnet $ToolDll `
    compile $NativePublicationLifetimeSource -o $NativePublicationLifetimeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native publication-lifetime core.' }
$NativePublicationLifetimeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationLifetimeModule).Hash.ToLowerInvariant()
if ($NativePublicationLifetimeHash -ne 'a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3') {
    throw "The Windvale native publication-lifetime core has an unexpected digest: $NativePublicationLifetimeHash"
}
$NativePublicationLifetimeInspection = (dotnet $ToolDll inspect $NativePublicationLifetimeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativePublicationLifetimeInspection -notmatch 'Profile: portable' -or
    $NativePublicationLifetimeInspection -notmatch 'Nativeˉpublicationˉlifetimeˉresult' -or
    $NativePublicationLifetimeInspection -notmatch 'Nativeˉpublicationˉlifetimeˉstatus' -or
    $NativePublicationLifetimeInspection -notmatch 'Nativeˉpublicationˉlifetimeˉplan' -or
    $NativePublicationLifetimeInspection -notmatch 'Exports \(7\)'
) {
    throw 'The Windvale native publication-lifetime core inspection is incomplete.'
}
$NativePublicationLifetimeBridgeSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv'
$NativePublicationLifetimeBridgeRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Publication-Lifetime-Bridge.wvb'
$NativePublicationLifetimeArtifactRetained = Join-Path $RepositoryRoot 'Runtime/Windvale.Native/Consumers/Native-Publication-Lifetime-Bridge.wvnf'
dotnet $ToolDll `
    compile $NativePublicationLifetimeBridgeSource `
    --module $NativePublicationLifetimeSource `
    -o $NativePublicationLifetimeBridgeModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale native publication-lifetime bridge.' }
$NativePublicationLifetimeBridgeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationLifetimeBridgeModule).Hash.ToLowerInvariant()
if ($NativePublicationLifetimeBridgeHash -ne 'f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554') {
    throw "The Windvale native publication-lifetime bridge has an unexpected digest: $NativePublicationLifetimeBridgeHash"
}
$NativePublicationLifetimeBridgeRetainedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationLifetimeBridgeRetained).Hash.ToLowerInvariant()
if (
    $NativePublicationLifetimeBridgeRetainedHash -ne $NativePublicationLifetimeBridgeHash -or
    (Get-Item -LiteralPath $NativePublicationLifetimeBridgeRetained).Length -ne
        (Get-Item -LiteralPath $NativePublicationLifetimeBridgeModule).Length
) {
    throw 'The retained Windvale native publication-lifetime bridge does not match its exact source compilation.'
}
$NativePublicationLifetimeArtifactHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $NativePublicationLifetimeArtifactRetained).Hash.ToLowerInvariant()
if (
    $NativePublicationLifetimeArtifactHash -ne '4d87911f2f442e6a2e4dd2364138f35a0037ddc0bff0775a16e37156768777a8' -or
    (Get-Item -LiteralPath $NativePublicationLifetimeArtifactRetained).Length -ne 46125
) {
    throw "The retained Windvale native publication-lifetime fragment has an unexpected identity: $NativePublicationLifetimeArtifactHash"
}
$NativePublicationLifetimeBridgeInspection = (dotnet $ToolDll inspect $NativePublicationLifetimeBridgeModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $NativePublicationLifetimeBridgeInspection -notmatch 'Profile: portable' -or
    $NativePublicationLifetimeBridgeInspection -notmatch 'Capabilities \(0\)' -or
    $NativePublicationLifetimeBridgeInspection -notmatch 'Main\(bytes\) -> bytes' -or
    $NativePublicationLifetimeBridgeInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Windvale native publication-lifetime bridge inspection is incomplete.'
}

$SourceLexerSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Lexer-Core.wv'
dotnet $ToolDll `
    compile $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceLexerModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source lexer.' }
$SourceLexerHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceLexerModule).Hash.ToLowerInvariant()
if ($SourceLexerHash -ne '411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e') {
    throw "The Windvale source lexer has an unexpected digest: $SourceLexerHash"
}
$SourceLexerInspection = (dotnet $ToolDll inspect $SourceLexerModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceLexerInspection -notmatch 'Nominal types \(7\)' -or
    $SourceLexerInspection -notmatch 'Compilerˉsourceˉtoken' -or
    $SourceLexerInspection -notmatch 'Compilerˉtokenˉkind' -or
    $SourceLexerInspection -notmatch 'Compilerˉlexˉsourceˉbounded' -or
    $SourceLexerInspection -notmatch 'Exports \(17\)'
) {
    throw 'The Windvale source-lexer inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Lexer-Demo.wv') `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceLexerDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-lexer demo.' }
$SourceLexerDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceLexerDemoModule).Hash.ToLowerInvariant()
if ($SourceLexerDemoHash -ne 'f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db') {
    throw "The Windvale source-lexer demo has an unexpected digest: $SourceLexerDemoHash"
}
$SourceLexerDemoOutput = dotnet $ToolDll `
    run $SourceLexerDemoModule --max-steps 10000000
if ($LASTEXITCODE -ne 0 -or $SourceLexerDemoOutput -notcontains 'Result: 0') {
    throw 'The Windvale source-lexer demo did not return Result: 0.'
}

$SourceDeclarationParserSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Declaration-Parser.wv'
dotnet $ToolDll `
    compile $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceDeclarationParserModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale declaration parser.' }
$SourceDeclarationParserHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceDeclarationParserModule).Hash.ToLowerInvariant()
if ($SourceDeclarationParserHash -ne '8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb') {
    throw "The Windvale declaration parser has an unexpected digest: $SourceDeclarationParserHash"
}
$SourceDeclarationParserInspection = (dotnet $ToolDll inspect $SourceDeclarationParserModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceDeclarationParserInspection -notmatch 'Nominal types \(15\)' -or
    $SourceDeclarationParserInspection -notmatch 'Compilerˉsourceˉdeclaration' -or
    $SourceDeclarationParserInspection -notmatch 'Compilerˉsourceˉmoduleˉsummary' -or
    $SourceDeclarationParserInspection -notmatch 'Compilerˉparseˉnextˉdeclarationˉvalidated' -or
    $SourceDeclarationParserInspection -notmatch 'Exports \(32\)'
) {
    throw 'The Windvale declaration-parser inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Declaration-Parser-Demo.wv') `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceDeclarationParserDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the declaration-parser demo.' }
$SourceDeclarationParserDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceDeclarationParserDemoModule).Hash.ToLowerInvariant()
if ($SourceDeclarationParserDemoHash -ne '9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf') {
    throw "The declaration-parser demo has an unexpected digest: $SourceDeclarationParserDemoHash"
}
$SourceDeclarationParserDemoOutput = dotnet $ToolDll `
    run $SourceDeclarationParserDemoModule --max-steps 20000000
if ($LASTEXITCODE -ne 0 -or $SourceDeclarationParserDemoOutput -notcontains 'Result: 0') {
    throw 'The declaration-parser demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Declaration-Parser-Tool.wv') `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceDeclarationParserToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the declaration-parser tool.' }
$SourceDeclarationParserToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceDeclarationParserToolModule).Hash.ToLowerInvariant()
if ($SourceDeclarationParserToolHash -ne 'ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0') {
    throw "The declaration-parser tool has an unexpected digest: $SourceDeclarationParserToolHash"
}
$SourceDeclarationParserArguments = @(
    'run', $SourceDeclarationParserToolModule,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$SourceLexerDeclarationOutput = dotnet $ToolDll `
    @SourceDeclarationParserArguments --max-steps 30000000 -- $SourceLexerSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceLexerDeclarationOutput -notcontains 'source declarations status=Valid imports=1 capabilities=0 data=0 records=3 enums=3 functions=19 tokens=6881 offset=56312' -or
    $SourceLexerDeclarationOutput -notcontains 'Result: 0'
) {
    throw 'The declaration-parser tool did not parse the real Windvale lexer source.'
}
$SourceParserSelfDeclarationOutput = dotnet $ToolDll `
    @SourceDeclarationParserArguments --max-steps 45000000 -- $SourceDeclarationParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceParserSelfDeclarationOutput -notcontains 'source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=32 tokens=15142 offset=112567' -or
    $SourceParserSelfDeclarationOutput -notcontains 'Result: 0'
) {
    throw 'The declaration-parser tool did not parse its own declaration source.'
}

$SourceBodyParserSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Body-Parser.wv'
dotnet $ToolDll `
    compile $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceBodyParserModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale body parser.' }
$SourceBodyParserHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBodyParserModule).Hash.ToLowerInvariant()
if ($SourceBodyParserHash -ne '68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589') {
    throw "The Windvale body parser has an unexpected digest: $SourceBodyParserHash"
}
$SourceBodyParserInspection = (dotnet $ToolDll inspect $SourceBodyParserModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBodyParserInspection -notmatch 'Nominal types \(25\)' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉsourceˉexpression' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉsourceˉstatement' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉparseˉexpressionˉvalidated' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉparseˉsourceˉbodies' -or
    $SourceBodyParserInspection -notmatch 'Exports \(47\)'
) {
    throw 'The Windvale body-parser inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Body-Parser-Demo.wv') `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceBodyParserDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the body-parser demo.' }
$SourceBodyParserDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBodyParserDemoModule).Hash.ToLowerInvariant()
if ($SourceBodyParserDemoHash -ne '2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f') {
    throw "The body-parser demo has an unexpected digest: $SourceBodyParserDemoHash"
}
$SourceBodyParserDemoOutput = dotnet $ToolDll `
    run $SourceBodyParserDemoModule --max-steps 30000000
if ($LASTEXITCODE -ne 0 -or $SourceBodyParserDemoOutput -notcontains 'Result: 0') {
    throw 'The body-parser demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Body-Parser-Tool.wv') `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceBodyParserToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the body-parser tool.' }
$SourceBodyParserToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBodyParserToolModule).Hash.ToLowerInvariant()
if ($SourceBodyParserToolHash -ne '0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f') {
    throw "The body-parser tool has an unexpected digest: $SourceBodyParserToolHash"
}
$SourceBodyParserArguments = @(
    'run', $SourceBodyParserToolModule,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$SourceLexerBodyOutput = dotnet $ToolDll `
    @SourceBodyParserArguments --max-steps 100000000 -- $SourceLexerSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceLexerBodyOutput -notcontains 'source bodies status=Valid functions=19 top-level=131 statements=749 expression-nodes=2153 statement-depth=17 expression-depth=5 offset=56313' -or
    $SourceLexerBodyOutput -notcontains 'Result: 0'
) {
    throw 'The body-parser tool did not parse the real Windvale lexer bodies.'
}
$SourceDeclarationBodyOutput = dotnet $ToolDll `
    @SourceBodyParserArguments --max-steps 160000000 -- $SourceDeclarationParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceDeclarationBodyOutput -notcontains 'source bodies status=Valid functions=32 top-level=365 statements=921 expression-nodes=3601 statement-depth=12 expression-depth=5 offset=112568' -or
    $SourceDeclarationBodyOutput -notcontains 'Result: 0'
) {
    throw 'The body-parser tool did not parse the declaration-parser bodies.'
}
$SourceBodySelfOutput = dotnet $ToolDll `
    @SourceBodyParserArguments --max-steps 160000000 -- $SourceBodyParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBodySelfOutput -notcontains 'source bodies status=Valid functions=48 top-level=339 statements=812 expression-nodes=3607 statement-depth=7 expression-depth=3 offset=110706' -or
    $SourceBodySelfOutput -notcontains 'Result: 0'
) {
    throw 'The body-parser tool did not parse its own statement and expression source.'
}

$SourceSetSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Set-Core.wv'
dotnet $ToolDll `
    compile $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceSetModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-set core.' }
$SourceSetHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSetModule).Hash.ToLowerInvariant()
if ($SourceSetHash -ne '1121320e20d83f685c559ea2d0cff8b8e57583d047a3c6aaf9f5c1fdc9423acb') {
    throw "The Windvale source-set core has an unexpected digest: $SourceSetHash"
}
$SourceSetInspection = (dotnet $ToolDll inspect $SourceSetModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceSetInspection -notmatch 'Nominal types \(29\)' -or
    $SourceSetInspection -notmatch 'Compilerˉsourceˉsetˉscan' -or
    $SourceSetInspection -notmatch 'Compilerˉsourceˉsetˉsummary' -or
    $SourceSetInspection -notmatch 'Compilerˉscanˉsourceˉset' -or
    $SourceSetInspection -notmatch 'Compilerˉvalidateˉsourceˉset' -or
    $SourceSetInspection -notmatch 'Exports \(10\)'
) {
    throw 'The Windvale source-set inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Set-Demo.wv') `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceSetDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-set demo.' }
$SourceSetDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSetDemoModule).Hash.ToLowerInvariant()
if ($SourceSetDemoHash -ne 'ac7fb0e04cf042ab9f9f3bfc8f344f0fdbcdc4198189b65f152eaead84b07742') {
    throw "The source-set demo has an unexpected digest: $SourceSetDemoHash"
}
$SourceSetDemoOutput = dotnet $ToolDll `
    run $SourceSetDemoModule --max-steps 200000000
if ($LASTEXITCODE -ne 0 -or $SourceSetDemoOutput -notcontains 'Result: 0') {
    throw 'The source-set demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Set-Tool.wv') `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceSetToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-set tool.' }
$SourceSetToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSetToolModule).Hash.ToLowerInvariant()
if ($SourceSetToolHash -ne '6e8b8c8aaa6fe2c5735719a9b317e8897cf70f87828ea1be5d26d670bc2ed30f') {
    throw "The source-set tool has an unexpected digest: $SourceSetToolHash"
}
$SourceSetArguments = @(
    'run', $SourceSetToolModule,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$SourceSetSelfOutput = dotnet $ToolDll `
    @SourceSetArguments --max-steps 800000000 -- `
    $SourceSetSource `
    $SourceBodyParserSource `
    $SourceDeclarationParserSource `
    $SourceLexerSource `
    $DecimalParsingSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceSetSelfOutput -notcontains 'source set status=Valid modules=5 source-bytes=297051 imports=6 records=18 enums=11 functions=110' -or
    $SourceSetSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-set tool did not validate the real compiler frontend set.'
}

$SourceGraphSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Graph-Core.wv'
dotnet $ToolDll `
    compile $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceGraphModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-graph core.' }
$SourceGraphHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceGraphModule).Hash.ToLowerInvariant()
if ($SourceGraphHash -ne '7fe9276273e48432f6206bde0d8a533dbc35ddf055b2f68b8a03810f07b4fb14') {
    throw "The Windvale source-graph core has an unexpected digest: $SourceGraphHash"
}
$SourceGraphInspection = (dotnet $ToolDll inspect $SourceGraphModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceGraphInspection -notmatch 'Nominal types \(34\)' -or
    $SourceGraphInspection -notmatch 'Compilerˉsourceˉgraphˉstatus' -or
    $SourceGraphInspection -notmatch 'Compilerˉsourceˉgraphˉsummary' -or
    $SourceGraphInspection -notmatch 'Compilerˉvalidateˉsourceˉgraph' -or
    $SourceGraphInspection -notmatch 'Exports \(12\)'
) {
    throw 'The Windvale source-graph inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Graph-Demo.wv') `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceGraphDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-graph demo.' }
$SourceGraphDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceGraphDemoModule).Hash.ToLowerInvariant()
if ($SourceGraphDemoHash -ne 'fc046e27037f0b2f18129b270b21d8d2c5c3c532601c5a4bfdf211881b0c2acb') {
    throw "The source-graph demo has an unexpected digest: $SourceGraphDemoHash"
}
$SourceGraphDemoOutput = dotnet $ToolDll `
    run $SourceGraphDemoModule --max-steps 300000000
if ($LASTEXITCODE -ne 0 -or $SourceGraphDemoOutput -notcontains 'Result: 0') {
    throw 'The source-graph demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Graph-Tool.wv') `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceGraphToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-graph tool.' }
$SourceGraphToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceGraphToolModule).Hash.ToLowerInvariant()
if ($SourceGraphToolHash -ne '9d49a4e321429ea203bd9a00ab2f6a9196c844c7cdf77a6b707ad6e0dc48b0fd') {
    throw "The source-graph tool has an unexpected digest: $SourceGraphToolHash"
}
$SourceGraphArguments = @(
    'run', $SourceGraphToolModule,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$SourceGraphSelfOutput = dotnet $ToolDll `
    @SourceGraphArguments --max-steps 1500000000 -- `
    $SourceGraphSource `
    $SourceBodyParserSource `
    $SourceDeclarationParserSource `
    $SourceLexerSource `
    $SourceSetSource `
    $ByteConstructionSource `
    $DecimalParsingSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceGraphSelfOutput -notcontains 'source graph status=Valid modules=7 imports=10 reachable=7' -or
    $SourceGraphSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-graph tool did not validate the real compiler graph.'
}

$SourceSymbolsSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Symbols-Core.wv'
dotnet $ToolDll `
    compile $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceSymbolsModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-symbol core.' }
$SourceSymbolsHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSymbolsModule).Hash.ToLowerInvariant()
if ($SourceSymbolsHash -ne '9d82d52310cd542a6a2854d10f990080f7a56fd98567ef4ec4042ce6fd6e7a9b') {
    throw "The Windvale source-symbol core has an unexpected digest: $SourceSymbolsHash"
}
$SourceSymbolsInspection = (dotnet $ToolDll inspect $SourceSymbolsModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceSymbolsInspection -notmatch 'Nominal types \(45\)' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉsourceˉsymbolˉstatus' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉsourceˉsymbolˉsummary' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉvalidateˉsourceˉsymbols' -or
    $SourceSymbolsInspection -notmatch 'Exports \(66\)'
) {
    throw 'The Windvale source-symbol inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Symbols-Demo.wv') `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceSymbolsDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-symbol demo.' }
$SourceSymbolsDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSymbolsDemoModule).Hash.ToLowerInvariant()
if ($SourceSymbolsDemoHash -ne '43f6aef685cbc265105abf36e7b8cae4de9974824fadfb8e5b83270c96d1ad1e') {
    throw "The source-symbol demo has an unexpected digest: $SourceSymbolsDemoHash"
}
$SourceSymbolsDemoOutput = dotnet $ToolDll `
    run $SourceSymbolsDemoModule --max-steps 1500000000
if ($LASTEXITCODE -ne 0 -or $SourceSymbolsDemoOutput -notcontains 'Result: 0') {
    throw 'The source-symbol demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Symbols-Tool.wv') `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceSymbolsToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-symbol tool.' }
$SourceSymbolsToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSymbolsToolModule).Hash.ToLowerInvariant()
if ($SourceSymbolsToolHash -ne '0555ca0b6d86a67cf94e8ee150d0baa92252f6a8ae7a06d7cbf2f7649f1ae43c') {
    throw "The source-symbol tool has an unexpected digest: $SourceSymbolsToolHash"
}
$SourceSymbolsArguments = @(
    'run', $SourceSymbolsToolModule,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$SourceSymbolsSelfOutput = dotnet $ToolDll `
    @SourceSymbolsArguments --max-steps 4000000000 -- `
    $SourceSymbolsSource `
    $SourceBodyParserSource `
    $SourceDeclarationParserSource `
    $SourceGraphSource `
    $SourceLexerSource `
    $SourceSetSource `
    $ByteConstructionSource `
    $DecimalParsingSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceSymbolsSelfOutput -notcontains 'source symbols status=Valid modules=8 capabilities=0 data=0 records=31 enums=14 functions=202 fields=344 members=245 parameters=891 directory-bytes=5944 visibility-bytes=64' -or
    $SourceSymbolsSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-symbol tool did not bind the real compiler closure.'
}

$SourceBindingsSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Bindings-Core.wv'
dotnet $ToolDll `
    compile $SourceBindingsSource `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceBindingsModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-binding core.' }
$SourceBindingsHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBindingsModule).Hash.ToLowerInvariant()
if ($SourceBindingsHash -ne '64b9fb7d92df4b40fc315d1f13f80118856a05ac2fb6b1d27e58dbb2d3bd9999') {
    throw "The Windvale source-binding core has an unexpected digest: $SourceBindingsHash"
}
$SourceBindingsInspection = (dotnet $ToolDll inspect $SourceBindingsModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBindingsInspection -notmatch 'Nominal types \(55\)' -or
    $SourceBindingsInspection -notmatch 'Compilerˉsourceˉbindingˉstatus' -or
    $SourceBindingsInspection -notmatch 'Compilerˉsourceˉbindingˉsummary' -or
    $SourceBindingsInspection -notmatch 'Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid' -or
    $SourceBindingsInspection -notmatch 'Compilerˉvalidateˉsourceˉbindings' -or
    $SourceBindingsInspection -notmatch 'Exports \(59\)'
) {
    throw 'The Windvale source-binding inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Bindings-Demo.wv') `
    --module $SourceBindingsSource `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceBindingsDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-binding demo.' }
$SourceBindingsDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBindingsDemoModule).Hash.ToLowerInvariant()
if ($SourceBindingsDemoHash -ne 'f6b9c184bfe8668a5c597dbe76d7709120a541789661df0156ce8f1785272e0a') {
    throw "The source-binding demo has an unexpected digest: $SourceBindingsDemoHash"
}
$SourceBindingsDemoOutput = dotnet $ToolDll `
    run $SourceBindingsDemoModule --max-steps 2000000000
if ($LASTEXITCODE -ne 0 -or $SourceBindingsDemoOutput -notcontains 'Result: 0') {
    throw 'The source-binding demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Bindings-Tool.wv') `
    --module $SourceBindingsSource `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceBindingsToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-binding tool.' }
$SourceBindingsToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBindingsToolModule).Hash.ToLowerInvariant()
if ($SourceBindingsToolHash -ne '7b75e3cd72eb7a2521d3b382effbdf2435879ceca2ed24561683ac63b956dad8') {
    throw "The source-binding tool has an unexpected digest: $SourceBindingsToolHash"
}
$SourceBindingsArguments = @(
    'run', $SourceBindingsToolModule,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$SourceBindingsSelfOutput = dotnet $ToolDll `
    @SourceBindingsArguments --max-steps 4000000000 -- `
    $SourceBindingsSource `
    $SourceBodyParserSource `
    $SourceDeclarationParserSource `
    $SourceGraphSource `
    $SourceLexerSource `
    $SourceSetSource `
    $SourceSymbolsSource `
    $ByteConstructionSource `
    $DecimalParsingSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBindingsSelfOutput -notcontains 'source bindings status=Valid modules=9 functions=261 parameters=1154 locals=1584 reads=13354 assignments=1098 calls=2317 directory-bytes=101120' -or
    $SourceBindingsSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-binding tool did not bind the real compiler closure.'
}

$SourceWirSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Wir-Core.wv'
dotnet $ToolDll `
    compile $SourceWirSource `
    --module $SourceBindingsSource `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceWirModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale typed-WVIR core.' }
$SourceWirHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWirModule).Hash.ToLowerInvariant()
if ($SourceWirHash -ne '3345bdfb062cf467f8b658414a672157b518af8c5a4aa994b0eb0e32e15837a4') {
    throw "The Windvale typed-WVIR core has an unexpected digest: $SourceWirHash"
}
$SourceWirInspection = (dotnet $ToolDll inspect $SourceWirModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWirInspection -notmatch 'Compilerˉsourceˉwirˉoperation' -or
    $SourceWirInspection -notmatch 'Compilerˉsourceˉwirˉsummary' -or
    $SourceWirInspection -notmatch 'Compilerˉsourceˉwirˉdirectoryˉisˉvalid' -or
    $SourceWirInspection -notmatch 'Compilerˉvalidateˉsourceˉwir' -or
    $SourceWirInspection -notmatch 'Exports \(72\)'
) {
    throw 'The Windvale typed-WVIR inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Wir-Demo.wv') `
    --module $SourceWirSource `
    --module $SourceBindingsSource `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceWirDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the typed-WVIR demo.' }
$SourceWirDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWirDemoModule).Hash.ToLowerInvariant()
if ($SourceWirDemoHash -ne 'd0b3f7b8fdbf1a7c56ebd8a2f024988933019f6e8b75202d5e97c5a96e567983') {
    throw "The typed-WVIR demo has an unexpected digest: $SourceWirDemoHash"
}
$SourceWirDemoOutput = dotnet $ToolDll run $SourceWirDemoModule --max-steps 4000000000
if ($LASTEXITCODE -ne 0 -or $SourceWirDemoOutput -notcontains 'Result: 0') {
    throw 'The typed-WVIR demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Wir-Tool.wv') `
    --module $SourceWirSource `
    --module $SourceBindingsSource `
    --module $SourceSymbolsSource `
    --module $SourceGraphSource `
    --module $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $ByteConstructionSource `
    --module $DecimalParsingSource `
    -o $SourceWirToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the typed-WVIR tool.' }
$SourceWirToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWirToolModule).Hash.ToLowerInvariant()
if ($SourceWirToolHash -ne '302409f6f1e8c0a1c3061dda2400083e06cf6781a28922f0138a952f2edfb574') {
    throw "The typed-WVIR tool has an unexpected digest: $SourceWirToolHash"
}
$SourceWirFixtureOutput = dotnet $ToolDll `
    run $SourceWirToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 2000000000 -- `
    (Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wir/Valid.wv')
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWirFixtureOutput -notcontains 'source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=3200' -or
    $SourceWirFixtureOutput -notcontains 'Result: 0'
) {
    throw 'The typed-WVIR tool did not lower and validate the control-heavy fixture.'
}

$SourceWvbSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Wvb-Core.wv'
$SourceWvbTemporarySlotsSource = Join-Path $RepositoryRoot 'Compiler/Windvale/Source-Wvb-Temporary-Slots.wv'
$SourceWvbDependencies = @(
    '--module', $SourceWvbTemporarySlotsSource,
    '--module', $SourceWirSource,
    '--module', $SourceBindingsSource,
    '--module', $SourceSymbolsSource,
    '--module', $SourceGraphSource,
    '--module', $SourceSetSource,
    '--module', $SourceBodyParserSource,
    '--module', $SourceDeclarationParserSource,
    '--module', $SourceLexerSource,
    '--module', $ByteConstructionSource,
    '--module', $DecimalParsingSource
)
dotnet $ToolDll compile $SourceWvbSource @SourceWvbDependencies -o $SourceWvbModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale WVB backend core.' }
$SourceWvbHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbModule).Hash.ToLowerInvariant()
if ($SourceWvbHash -ne '20ab85982a821dee87c8674c16c520a10646b604a5d3f0e38442589e521bf1c0') {
    throw "The Windvale WVB backend core has an unexpected digest: $SourceWvbHash"
}
$SourceWvbInspection = (dotnet $ToolDll inspect $SourceWvbModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbInspection -notmatch 'Compilerˉsourceˉwvbˉsummary' -or
    $SourceWvbInspection -notmatch 'Compilerˉcompileˉsourceˉwvb' -or
    $SourceWvbInspection -notmatch 'Exports \(72\)'
) {
    throw 'The Windvale WVB backend inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Wvb-Demo.wv') `
    --module $SourceWvbSource @SourceWvbDependencies `
    -o $SourceWvbDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale WVB backend demo.' }
$SourceWvbDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbDemoModule).Hash.ToLowerInvariant()
if ($SourceWvbDemoHash -ne 'c8f96a8d973c1d03d5bb115612ddc765a2a7e3b617ec61af641f540fb7b2ce04') {
    throw "The Windvale WVB backend demo has an unexpected digest: $SourceWvbDemoHash"
}
$SourceWvbDemoOutput = dotnet $ToolDll run $SourceWvbDemoModule --max-steps 4000000000
if ($LASTEXITCODE -ne 0 -or $SourceWvbDemoOutput -notcontains 'Result: 0') {
    throw 'The Windvale WVB backend demo did not return Result: 0.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Wvb-Tool.wv') `
    --module $SourceWvbSource @SourceWvbDependencies `
    -o $SourceWvbToolModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale WVB backend tool.' }
$SourceWvbToolHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbToolModule).Hash.ToLowerInvariant()
if ($SourceWvbToolHash -ne 'fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556') {
    throw "The Windvale WVB backend tool has an unexpected digest: $SourceWvbToolHash"
}
$SourceWvbFixture = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Function-Only.wv'
Remove-Item -LiteralPath $SourceWvbFixtureModule, $SourceWvbFixtureOracle -Force -ErrorAction SilentlyContinue
$SourceWvbFixtureOutput = dotnet $ToolDll `
    run $SourceWvbToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 4000000000 -- `
    $SourceWvbFixture $SourceWvbFixtureModule
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbFixtureOutput -notcontains 'source wvb status=Valid functions=4 code-bytes=532 module-bytes=816' -or
    $SourceWvbFixtureOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale WVB backend tool did not lower the function-only fixture.'
}
$SourceWvbVerifyOutput = dotnet $ToolDll verify $SourceWvbFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbVerifyOutput -notcontains 'Verified: Sourceˉwvbˉfixture') {
    throw 'The Windvale-written WVB fixture did not pass the Stage 0 verifier.'
}
$SourceWvbRunOutput = dotnet $ToolDll run $SourceWvbFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbRunOutput -notcontains 'Result: 6') {
    throw 'The Windvale-written WVB fixture did not execute with Result: 6.'
}
dotnet $ToolDll compile $SourceWvbFixture -o $SourceWvbFixtureOracle
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Stage 0 WVB fixture oracle.' }
$SourceWvbFixtureHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbFixtureModule).Hash.ToLowerInvariant()
if ($SourceWvbFixtureHash -ne '28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936') {
    throw "The Windvale-written WVB fixture has an unexpected digest: $SourceWvbFixtureHash"
}
if (-not [System.Linq.Enumerable]::SequenceEqual(
    [IO.File]::ReadAllBytes($SourceWvbFixtureModule),
    [IO.File]::ReadAllBytes($SourceWvbFixtureOracle)
)) {
    throw 'The Windvale-written WVB fixture differs from the Stage 0 oracle.'
}

$SourceWvbDataFixture = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Data-And-Text.wv'
Remove-Item -LiteralPath $SourceWvbDataFixtureModule, $SourceWvbDataFixtureOracle -Force -ErrorAction SilentlyContinue
$SourceWvbDataFixtureOutput = dotnet $ToolDll `
    run $SourceWvbToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 4000000000 -- `
    $SourceWvbDataFixture $SourceWvbDataFixtureModule
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbDataFixtureOutput -notcontains 'source wvb status=Valid functions=3 code-bytes=1210 module-bytes=1652' -or
    $SourceWvbDataFixtureOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale WVB backend tool did not lower the data-and-text fixture.'
}
$SourceWvbDataVerifyOutput = dotnet $ToolDll verify $SourceWvbDataFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbDataVerifyOutput -notcontains 'Verified: Sourceˉwvbˉdataˉandˉtext') {
    throw 'The Windvale-written data-and-text WVB fixture did not pass the Stage 0 verifier.'
}
$SourceWvbDataRunOutput = dotnet $ToolDll run $SourceWvbDataFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbDataRunOutput -notcontains 'Result: 13') {
    throw 'The Windvale-written data-and-text WVB fixture did not execute with Result: 13.'
}
dotnet $ToolDll compile $SourceWvbDataFixture -o $SourceWvbDataFixtureOracle
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Stage 0 data-and-text oracle.' }
$SourceWvbDataFixtureHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbDataFixtureModule).Hash.ToLowerInvariant()
if ($SourceWvbDataFixtureHash -ne '8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc') {
    throw "The Windvale-written data-and-text fixture has an unexpected digest: $SourceWvbDataFixtureHash"
}
if (-not [System.Linq.Enumerable]::SequenceEqual(
    [IO.File]::ReadAllBytes($SourceWvbDataFixtureModule),
    [IO.File]::ReadAllBytes($SourceWvbDataFixtureOracle)
)) {
    throw 'The Windvale-written data-and-text fixture differs from the Stage 0 oracle.'
}

$SourceWvbNominalFixture = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Nominal-Types.wv'
Remove-Item -LiteralPath $SourceWvbNominalFixtureModule, $SourceWvbNominalFixtureOracle -Force -ErrorAction SilentlyContinue
$SourceWvbNominalFixtureOutput = dotnet $ToolDll `
    run $SourceWvbToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 4000000000 -- `
    $SourceWvbNominalFixture $SourceWvbNominalFixtureModule
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbNominalFixtureOutput -notcontains 'source wvb status=Valid functions=3 code-bytes=1097 module-bytes=1782' -or
    $SourceWvbNominalFixtureOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale WVB backend tool did not lower the nominal-types fixture.'
}
$SourceWvbNominalVerifyOutput = dotnet $ToolDll verify $SourceWvbNominalFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbNominalVerifyOutput -notcontains 'Verified: Sourceˉwvbˉnominalˉtypes') {
    throw 'The Windvale-written nominal-types WVB fixture did not pass the Stage 0 verifier.'
}
$SourceWvbNominalRunOutput = dotnet $ToolDll run $SourceWvbNominalFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbNominalRunOutput -notcontains 'Result: 11') {
    throw 'The Windvale-written nominal-types WVB fixture did not execute with Result: 11.'
}
dotnet $ToolDll compile $SourceWvbNominalFixture -o $SourceWvbNominalFixtureOracle
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Stage 0 nominal-types oracle.' }
$SourceWvbNominalFixtureHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbNominalFixtureModule).Hash.ToLowerInvariant()
if ($SourceWvbNominalFixtureHash -ne 'b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b') {
    throw "The Windvale-written nominal-types fixture has an unexpected digest: $SourceWvbNominalFixtureHash"
}
if (-not [System.Linq.Enumerable]::SequenceEqual(
    [IO.File]::ReadAllBytes($SourceWvbNominalFixtureModule),
    [IO.File]::ReadAllBytes($SourceWvbNominalFixtureOracle)
)) {
    throw 'The Windvale-written nominal-types fixture differs from the Stage 0 oracle.'
}

$SourceWvbHostedFixture = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv'
Remove-Item -LiteralPath $SourceWvbHostedFixtureModule, $SourceWvbHostedFixtureOracle -Force -ErrorAction SilentlyContinue
$SourceWvbHostedFixtureOutput = dotnet $ToolDll `
    run $SourceWvbToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 4000000000 -- `
    $SourceWvbHostedFixture $SourceWvbHostedFixtureModule
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbHostedFixtureOutput -notcontains 'source wvb status=Valid functions=7 code-bytes=249 module-bytes=850' -or
    $SourceWvbHostedFixtureOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale WVB backend tool did not lower the hosted-capabilities fixture.'
}
$SourceWvbHostedVerifyOutput = dotnet $ToolDll verify $SourceWvbHostedFixtureModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbHostedVerifyOutput -notcontains 'Verified: Sourceˉwvbˉhostedˉcapabilities') {
    throw 'The Windvale-written hosted-capabilities WVB fixture did not pass the Stage 0 verifier.'
}
$SourceWvbHostedInspection = (dotnet $ToolDll inspect $SourceWvbHostedFixtureModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbHostedInspection -notmatch 'Profile: hosted' -or
    $SourceWvbHostedInspection -notmatch 'Capabilities \(7\)' -or
    $SourceWvbHostedInspection -notmatch 'call\.capability capability\[0\] \(console\.write\)' -or
    $SourceWvbHostedInspection -notmatch 'call\.capability capability\[6\] \(process\.argument_count\)'
) {
    throw 'The Windvale-written hosted-capabilities WVB inspection is incomplete.'
}
$SourceWvbHostedRunOutput = dotnet $ToolDll `
    run $SourceWvbHostedFixtureModule `
    --allow console.write `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count
if ($LASTEXITCODE -ne 0 -or $SourceWvbHostedRunOutput -notcontains 'Result: 0') {
    throw 'The Windvale-written hosted-capabilities WVB fixture did not execute with Result: 0.'
}
dotnet $ToolDll compile $SourceWvbHostedFixture -o $SourceWvbHostedFixtureOracle
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Stage 0 hosted-capabilities oracle.' }
$SourceWvbHostedFixtureHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbHostedFixtureModule).Hash.ToLowerInvariant()
if ($SourceWvbHostedFixtureHash -ne 'bad95ed62ed8406c169ddadaa8da8576825d9213af2faa74b945db44afdfd41f') {
    throw "The Windvale-written hosted-capabilities fixture has an unexpected digest: $SourceWvbHostedFixtureHash"
}
if (-not [System.Linq.Enumerable]::SequenceEqual(
    [IO.File]::ReadAllBytes($SourceWvbHostedFixtureModule),
    [IO.File]::ReadAllBytes($SourceWvbHostedFixtureOracle)
)) {
    throw 'The Windvale-written hosted-capabilities fixture differs from the Stage 0 oracle.'
}

$SourceWvbCompositionRoot = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Composition-Root.wv'
$SourceWvbCompositionLeaf = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Composition-Leaf.wv'
$SourceWvbCompositionMiddle = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Composition-Middle.wv'
Remove-Item -LiteralPath `
    $SourceWvbCompositionModule, `
    $SourceWvbCompositionOracle, `
    $InvalidSourceWvbCompositionModule `
    -Force -ErrorAction SilentlyContinue
$SourceWvbCompositionOutput = dotnet $ToolDll `
    run $SourceWvbToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 4000000000 -- `
    $SourceWvbCompositionRoot `
    $SourceWvbCompositionLeaf `
    $SourceWvbCompositionMiddle `
    $SourceWvbCompositionModule
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbCompositionOutput -notcontains 'source wvb status=Valid functions=9 code-bytes=627 module-bytes=1388' -or
    $SourceWvbCompositionOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale WVB backend tool did not lower the multi-module fixture.'
}
$SourceWvbCompositionVerifyOutput = dotnet $ToolDll verify $SourceWvbCompositionModule
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbCompositionVerifyOutput -notcontains 'Verified: Compositionˉdemo'
) {
    throw 'The Windvale-written multi-module WVB fixture did not pass verification.'
}
$SourceWvbCompositionInspection = (dotnet $ToolDll inspect $SourceWvbCompositionModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbCompositionInspection -notmatch 'Data \(4\)' -or
    $SourceWvbCompositionInspection -notmatch '\[2\] __Text_000001: text' -or
    $SourceWvbCompositionInspection -notmatch 'Nominal types \(5\)' -or
    $SourceWvbCompositionInspection -notmatch 'Functions \(9\)' -or
    $SourceWvbCompositionInspection -notmatch 'Exports \(1\)' -or
    $SourceWvbCompositionInspection -notmatch 'Main -> function\[1\]'
) {
    throw 'The Windvale-written multi-module WVB inspection is incomplete.'
}
$SourceWvbCompositionRunOutput = dotnet $ToolDll run $SourceWvbCompositionModule
if ($LASTEXITCODE -ne 0 -or $SourceWvbCompositionRunOutput -notcontains 'Result: 42') {
    throw 'The Windvale-written multi-module WVB fixture did not execute with Result: 42.'
}
dotnet $ToolDll `
    compile $SourceWvbCompositionRoot `
    --module $SourceWvbCompositionLeaf `
    --module $SourceWvbCompositionMiddle `
    -o $SourceWvbCompositionOracle
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Stage 0 multi-module oracle.' }
$SourceWvbCompositionHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbCompositionModule).Hash.ToLowerInvariant()
if ($SourceWvbCompositionHash -ne '42d134ee0674dcc2cfa97d018ea03b27f014b2f916d8273ba02a0aee868e0fd5') {
    throw "The Windvale-written multi-module fixture has an unexpected digest: $SourceWvbCompositionHash"
}
if (-not [System.Linq.Enumerable]::SequenceEqual(
    [IO.File]::ReadAllBytes($SourceWvbCompositionModule),
    [IO.File]::ReadAllBytes($SourceWvbCompositionOracle)
)) {
    throw 'The Windvale-written multi-module fixture differs from the Stage 0 oracle.'
}
$RejectedSourceWvbCompositionOutput = dotnet $ToolDll `
    run $SourceWvbToolModule `
    --allow console.write_line `
    --allow diagnostic.write_line `
    --allow file.read_bytes `
    --allow file.write_bytes `
    --allow process.argument `
    --allow process.argument_count `
    --max-steps 4000000000 -- `
    $SourceWvbCompositionRoot `
    $SourceWvbCompositionMiddle `
    $SourceWvbCompositionLeaf `
    $InvalidSourceWvbCompositionModule 2>&1
$RejectedSourceWvbCompositionExit = $LASTEXITCODE
if (
    $RejectedSourceWvbCompositionExit -ne 0 -or
    -not ($RejectedSourceWvbCompositionOutput -match 'source wvb status=Sourceˉwir') -or
    -not ($RejectedSourceWvbCompositionOutput -match 'Result: 1') -or
    (Test-Path -LiteralPath $InvalidSourceWvbCompositionModule)
) {
    throw 'The Windvale WVB backend did not reject noncanonical dependency order without output.'
}

dotnet $ToolDll compile (Join-Path $RepositoryRoot 'Examples/Foundation/Wv-Dump-Core.wv') -o $WvDumpCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Wv-Dump-Core.wv.' }

$WvDumpCoreVerifyOutput = dotnet $ToolDll verify $WvDumpCoreModule
if ($LASTEXITCODE -ne 0 -or $WvDumpCoreVerifyOutput -notcontains 'Verified: Wvˉdumpˉcore') {
    throw 'The Seed CLI failed to verify Wv-Dump-Core.wvb.'
}

$WvDumpCoreInspectOutput = dotnet $ToolDll inspect $WvDumpCoreModule
$WvDumpCoreInspection = $WvDumpCoreInspectOutput -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvDumpCoreInspectOutput -notcontains 'Nominal types (5)' -or
    $WvDumpCoreInspection -notmatch 'Inspectˉwvbˉenvelope' -or
    $WvDumpCoreInspection -notmatch 'record\.create' -or
    $WvDumpCoreInspection -notmatch 'record\.field' -or
    $WvDumpCoreInspection -notmatch 'enum\.name' -or
    $WvDumpCoreInspection -notmatch 'u32\.format' -or
    $WvDumpCoreInspection -notmatch 'text\.concat' -or
    $WvDumpCoreInspection -notmatch 'bytes\.read_i32_little' -or
    $WvDumpCoreInspection -notmatch 'text\.utf8_is_valid' -or
    $WvDumpCoreInspection -notmatch 'text\.from_utf8' -or
    $WvDumpCoreInspection -notmatch 'text\.quote' -or
    $WvDumpCoreInspection -notmatch 'u32\.from_u8'
) {
    throw 'The Seed CLI inspector did not expose the structured Windvale section walker.'
}

$WvDumpCapabilities = @(
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '10000000'
)

$WvDumpUnauthorizedOutput = dotnet $ToolDll run $WvDumpCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvDumpUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WvDump hosted capabilities.'
}

$WvDumpCoreRunOutput = dotnet $ToolDll run $WvDumpCoreModule @WvDumpCapabilities
if ($LASTEXITCODE -ne 0 -or $WvDumpCoreRunOutput -notcontains 'Result: 0') {
    throw 'The Seed CLI did not produce Result: 0 for Wv-Dump-Core.wvb.'
}

$WvDumpHostedOutput = dotnet $ToolDll run $WvDumpCoreModule @WvDumpCapabilities -- $SumModule
if (
    $LASTEXITCODE -ne 0 -or
    $WvDumpHostedOutput -notcontains 'wvdump 1' -or
    $WvDumpHostedOutput -notcontains 'module version=1.11 profile=portable name="Sum\u02C9data"' -or
    $WvDumpHostedOutput -notcontains 'data index=0 name="Values" type=i32_array elements=4' -or
    $WvDumpHostedOutput -notcontains 'instruction function=1 offset=141 opcode=call operand=0' -or
    $WvDumpHostedOutput -notcontains 'export index=0 name="Main" kind=function target=1' -or
    $WvDumpHostedOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale-written WvDump core did not produce the expected real-module report.'
}

$WvDumpInvalidOutput = dotnet $ToolDll run $WvDumpCoreModule @WvDumpCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    ($WvDumpInvalidOutput -join "`n") -notmatch 'Badˉmagic sections=0 offset=0' -or
    $WvDumpInvalidOutput -notcontains 'Result: 2'
) {
    throw 'The Windvale-written WvDump core did not route an invalid-file diagnostic separately.'
}

$MissingHostedFile = Join-Path $Artifacts '__windvale_missing_hosted_resource__.wvb'
if (Test-Path -LiteralPath $MissingHostedFile) {
    throw "The missing-file verifier path unexpectedly exists: $MissingHostedFile"
}
$WvDumpMissingOutput = dotnet $ToolDll run $WvDumpCoreModule @WvDumpCapabilities -- $MissingHostedFile 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvDumpMissingOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The hosted file adapter did not report a missing resource deterministically.'
}

$WvDumpInvalidNameOutput = dotnet $ToolDll run $WvDumpCoreModule @WvDumpCapabilities -- '' 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvDumpInvalidNameOutput -join "`n") -notmatch 'WVR3021') {
    throw 'The hosted file adapter did not reject an empty resource name deterministically.'
}

dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Object-Model/Windvale/Wvo-Object-Core.wv') `
    --module $ByteOrderingSource `
    --module $Sha256Source `
    -o $WvoCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Wvo-Object-Core.wv.' }

$WvoCoreVerifyOutput = dotnet $ToolDll verify $WvoCoreModule
if ($LASTEXITCODE -ne 0 -or $WvoCoreVerifyOutput -notcontains 'Verified: Wvoˉobjectˉcore') {
    throw 'The Seed CLI failed to verify Wvo-Object-Core.wvb.'
}

$WvoCoreInspection = (dotnet $ToolDll inspect $WvoCoreModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvoCoreInspection -notmatch 'bytes\.concat' -or
    $WvoCoreInspection -notmatch 'bytes\.from_u16_little' -or
    $WvoCoreInspection -notmatch 'bytes\.from_i32_little' -or
    $WvoCoreInspection -notmatch 'text\.to_utf8' -or
    $WvoCoreInspection -notmatch '__WvM1F0' -or
    $WvoCoreInspection -notmatch 'file\.read_bytes' -or
    $WvoCoreInspection -notmatch 'Objectˉsha256' -or
    $WvoCoreInspection -notmatch '__WvM2F0\(bytes\) -> bytes' -or
    $WvoCoreInspection -match 'file\.write_bytes'
) {
    throw 'The Seed CLI inspector did not expose the read-only Windvale object operations.'
}

$WvoCapabilities = @(
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '10000000'
)

$WvoUnauthorizedOutput = dotnet $ToolDll run $WvoCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WVO read-only capabilities.'
}

$WvoSelfTestOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities
if ($LASTEXITCODE -ne 0 -or $WvoSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale object core self-test did not return Result: 0.'
}

$WvoSampleOutput = dotnet $ToolDll assemble `
    (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') `
    -o $WvoSample
if ($LASTEXITCODE -ne 0 -or $WvoSampleOutput -notcontains "Assembled: $WvoSample") {
    throw 'The Stage 0 assembler did not create the WVO inspector input.'
}

$WvoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WvoSample).Hash.ToLowerInvariant()
if ($WvoHash -ne '992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85') {
    throw "The WVO inspector input has unexpected bytes: $WvoHash"
}

$WvoHostedOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- verify $WvoSample
if (
    $LASTEXITCODE -ne 0 -or
    $WvoHostedOutput -notcontains 'Verified object: X86ˉ64' -or
    $WvoHostedOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale object core did not verify the expected object.'
}

$WvoHostedInspection = (dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- inspect $WvoSample) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvoHostedInspection -notmatch 'Sections \(2\)' -or
    $WvoHostedInspection -notmatch 'Console_write binding=Import' -or
    $WvoHostedInspection -notmatch 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' -or
    $WvoHostedInspection -notmatch 'Result: 0'
) {
    throw 'The Windvale object core did not inspect the expected records.'
}

$WvoVerifyOutput = dotnet $ToolDll object-verify $WvoSample
if ($LASTEXITCODE -ne 0 -or $WvoVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The Stage 0 object verifier rejected the WVO inspector input.'
}

$WvoInspection = (dotnet $ToolDll object-inspect $WvoSample) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvoInspection -notmatch 'Sections \(2\)' -or
    $WvoInspection -notmatch 'Console_write binding=Import' -or
    $WvoInspection -notmatch 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4'
) {
    throw 'The Stage 0 object inspector did not expose the expected records.'
}

$WvoInvalidNameOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- verify '' 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoInvalidNameOutput -join "`n") -notmatch 'WVR3021') {
    throw 'The hosted file reader did not reject an empty resource name deterministically.'
}

$MissingWvoInput = Join-Path $Artifacts '__windvale_missing_wvo_input__.wvo'
if (Test-Path -LiteralPath $MissingWvoInput) {
    throw "The missing WVO input unexpectedly exists: $MissingWvoInput"
}
$WvoMissingInputOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- verify $MissingWvoInput 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoMissingInputOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The hosted file reader did not report a missing WVO input deterministically.'
}

dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Assembler/Windvale/Wva-Assembler-Core.wv') `
    --module $MachineContractsSource `
    --module $ByteOrderingSource `
    --module $DecimalParsingSource `
    --module $ByteConstructionSource `
    -o $WvaAssemblerModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Wva-Assembler-Core.wv.' }

$WvaAssemblerVerifyOutput = dotnet $ToolDll verify $WvaAssemblerModule
if ($LASTEXITCODE -ne 0 -or $WvaAssemblerVerifyOutput -notcontains 'Verified: Wvaˉassemblerˉcore') {
    throw 'The bytecode verifier rejected the Windvale WVA assembler.'
}

$WvaAssemblerInspection = (dotnet $ToolDll inspect $WvaAssemblerModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvaAssemblerInspection -notmatch 'Scanˉwva' -or
    $WvaAssemblerInspection -notmatch 'Inspectˉwvaˉsemantics' -or
    $WvaAssemblerInspection -notmatch 'Encodeˉwva' -or
    $WvaAssemblerInspection -notmatch 'Encodeˉsections' -or
    $WvaAssemblerInspection -notmatch 'Encodeˉsymbols' -or
    $WvaAssemblerInspection -notmatch 'Encodeˉrelocations' -or
    $WvaAssemblerInspection -notmatch '__WvM4F1' -or
    $WvaAssemblerInspection -notmatch '__WvM2F0' -or
    $WvaAssemblerInspection -notmatch '__WvM3F0' -or
    $WvaAssemblerInspection -notmatch '__WvM1F0' -or
    $WvaAssemblerInspection -notmatch 'bytes\.concat' -or
    $WvaAssemblerInspection -notmatch 'bytes\.from_u32_little' -or
    $WvaAssemblerInspection -notmatch 'file\.read_bytes' -or
    $WvaAssemblerInspection -notmatch 'file\.write_bytes'
) {
    throw 'The Seed CLI inspector did not expose the Windvale WVA assembler operations.'
}

$WvaAssemblerCapabilities = @(
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '10000000'
)

$WvaAssemblerUnauthorizedOutput = dotnet $ToolDll run $WvaAssemblerModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvaAssemblerUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WVA assembler capabilities.'
}

$WvaAssemblerSelfTestOutput = dotnet $ToolDll run $WvaAssemblerModule @WvaAssemblerCapabilities
if ($LASTEXITCODE -ne 0 -or $WvaAssemblerSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale WVA assembler self-test did not return Result: 0.'
}

dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Linker/Windvale/Wv-Linker-Core.wv') `
    --module $MachineContractsSource `
    --module $ByteOrderingSource `
    --module $DecimalParsingSource `
    --module $ByteConstructionSource `
    -o $WvLinkerCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale linker core.' }

$WvLinkerVerifyOutput = dotnet $ToolDll verify $WvLinkerCoreModule
if ($LASTEXITCODE -ne 0 -or $WvLinkerVerifyOutput -notcontains 'Verified: Wvˉlinkerˉcore') {
    throw 'The bytecode verifier rejected the Windvale linker core.'
}

$WvLinkerInspectOutput = dotnet $ToolDll inspect $WvLinkerCoreModule
$WvLinkerInspection = $WvLinkerInspectOutput -join "`n"
if (
    $WvLinkerInspection -notmatch 'Inspectˉobject' -or
    $WvLinkerInspection -notmatch 'Findˉsection' -or
    $WvLinkerInspection -notmatch 'Findˉsymbol' -or
    $WvLinkerInspection -notmatch 'Findˉrelocation' -or
    $WvLinkerInspection -notmatch 'Validateˉexportˉuniqueness' -or
    $WvLinkerInspection -notmatch 'Validateˉimports' -or
    $WvLinkerInspection -notmatch 'Measureˉlayout' -or
    $WvLinkerInspection -notmatch 'Validateˉdefinitions' -or
    $WvLinkerInspection -notmatch 'Buildˉunrelocatedˉimage' -or
    $WvLinkerInspection -notmatch 'Applyˉrelocations' -or
    $WvLinkerInspection -notmatch 'Verifierˉplaceˉsection' -or
    $WvLinkerInspection -notmatch 'Verifierˉfindˉexport' -or
    $WvLinkerInspection -notmatch 'Verifierˉapplyˉrelocationsˉreverse' -or
    $WvLinkerInspection -notmatch 'Acceptˉreconstructedˉimage' -or
    $WvLinkerInspection -notmatch 'Acceptedˉobjectˉview' -or
    $WvLinkerInspection -notmatch 'Definitionˉmapˉminimumˉexceedsˉlimit' -or
    $WvLinkerInspection -notmatch 'Buildˉcanonicalˉmap' -or
    $WvLinkerInspection -notmatch '__WvM4F0' -or
    $WvLinkerInspection -notmatch '__WvM2F0' -or
    $WvLinkerInspection -notmatch '__WvM3F0' -or
    $WvLinkerInspection -notmatch '__WvM1F0' -or
    $WvLinkerInspection -notmatch '__WvM1F1' -or
    $WvLinkerInspection -notmatch 'bytes\.read_i32_little' -or
    $WvLinkerInspection -notmatch 'bytes\.sha256_hex' -or
    $WvLinkerInspection -notmatch 'file\.read_bytes' -or
    $WvLinkerInspection -notmatch 'file\.write_bytes'
) {
    throw 'The Seed CLI inspector did not expose the Windvale linker scanner operations.'
}

$WvLinkerCapabilities = @(
    '--allow', 'console.write',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '20000000'
)
$WvLinkerUnauthorizedOutput = dotnet $ToolDll run $WvLinkerCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvLinkerUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted Windvale linker capabilities.'
}

$WvLinkerSelfTestOutput = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities
if ($LASTEXITCODE -ne 0 -or $WvLinkerSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale linker scanner self-test did not return Result: 0.'
}

$WvaAssemblerHostedOutput = dotnet $ToolDll run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') $WindvaleAssemblyObject
if (
    $LASTEXITCODE -ne 0 -or
    $WvaAssemblerHostedOutput -notcontains 'wvasm 1' -or
    $WvaAssemblerHostedOutput -notcontains 'assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1' -or
    $WvaAssemblerHostedOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale WVA assembler did not encode the canonical assembly source.'
}
$WindvaleAssemblyHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WindvaleAssemblyObject).Hash.ToLowerInvariant()
if ($WindvaleAssemblyHash -ne '992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85') {
    throw "The Windvale WVA assembler wrote unexpected bytes: $WindvaleAssemblyHash"
}
$WindvaleAssemblyVerifyOutput = dotnet $ToolDll object-verify $WindvaleAssemblyObject
if ($LASTEXITCODE -ne 0 -or $WindvaleAssemblyVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The independent object verifier rejected the Windvale-written assembler output.'
}

$WvLinkerHostedOutput = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities -- $WindvaleAssemblyObject
if (
    $LASTEXITCODE -ne 0 -or
    $WvLinkerHostedOutput -notcontains 'object status=Valid sections=2 symbols=3 relocations=2 offset=218' -or
    $WvLinkerHostedOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale linker scanner did not accept the canonical assembler object.'
}

$WvLinkerInvalidOutput = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    ($WvLinkerInvalidOutput -join "`n") -notmatch 'object status=Badˉmagic' -or
    $WvLinkerInvalidOutput -notcontains 'Result: 2'
) {
    throw 'The Windvale linker scanner did not reject a non-WVO input deterministically.'
}
$MissingAssemblerParent = Join-Path $Artifacts '__windvale_missing_assembler_parent__'
if (Test-Path -LiteralPath $MissingAssemblerParent) {
    throw "The missing assembler parent unexpectedly exists: $MissingAssemblerParent"
}
$MissingAssemblerOutput = Join-Path $MissingAssemblerParent 'Hello.wvo'
$WvaAssemblerMissingParentOutput = dotnet $ToolDll run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') $MissingAssemblerOutput 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvaAssemblerMissingParentOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The Windvale WVA assembler did not report a missing output parent deterministically.'
}
if (Test-Path -LiteralPath $MissingAssemblerOutput) {
    throw 'The failed Windvale assembler host write left a partial output object.'
}

if (Test-Path -LiteralPath $InvalidWindvaleAssemblyObject) {
    throw "The invalid Windvale assembly output unexpectedly exists: $InvalidWindvaleAssemblyObject"
}
$WvaSemanticInvalidOutput = dotnet $ToolDll run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') $InvalidWindvaleAssemblyObject 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    ($WvaSemanticInvalidOutput -join "`n") -notmatch 'assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1' -or
    $WvaSemanticInvalidOutput -notcontains 'Result: 2'
) {
    throw 'The Windvale WVA assembler did not reject non-WVA source deterministically.'
}
if (Test-Path -LiteralPath $InvalidWindvaleAssemblyObject) {
    throw 'Rejected Windvale assembly created a partial output object.'
}
$WvaSemanticExistingOutput = dotnet $ToolDll run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') $WindvaleAssemblyObject 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    ($WvaSemanticExistingOutput -join "`n") -notmatch 'assembly status=WVA1001' -or
    $WvaSemanticExistingOutput -notcontains 'Result: 2'
) {
    throw 'The Windvale WVA assembler did not reject invalid input targeting an existing output.'
}
$PreservedWindvaleAssemblyHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WindvaleAssemblyObject).Hash.ToLowerInvariant()
if ($PreservedWindvaleAssemblyHash -ne $WindvaleAssemblyHash) {
    throw 'Rejected Windvale assembly modified an existing output object.'
}

$AssemblyOutput = dotnet $ToolDll assemble (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') -o $AssemblyObject
if (
    $LASTEXITCODE -ne 0 -or
    ($AssemblyOutput -join "`n") -notmatch 'Assembled:' -or
    $AssemblyOutput -notcontains 'SHA-256: 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85'
) {
    throw 'The Stage 0 assembler did not produce the canonical WVA example object.'
}
$Stage0AssemblyHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $AssemblyObject).Hash.ToLowerInvariant()
if ($Stage0AssemblyHash -ne $WindvaleAssemblyHash) {
    throw 'The Windvale-written and Stage 0 assembler objects differ.'
}

$AssemblyVerifyOutput = dotnet $ToolDll object-verify $AssemblyObject
if ($LASTEXITCODE -ne 0 -or $AssemblyVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The object verifier rejected the WVA example object.'
}

$AssemblyInspection = (dotnet $ToolDll object-inspect $AssemblyObject) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $AssemblyInspection -notmatch '\.text kind=Code align=16 memory=11 data=11' -or
    $AssemblyInspection -notmatch 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' -or
    $AssemblyInspection -notmatch 'kind=Absoluteˉu32 section=1 offset=3 symbol=1 addend=0'
) {
    throw 'The object inspector did not expose the expected WVA sections and relocations.'
}

$ProviderAssemblyOutput = dotnet $ToolDll assemble (Join-Path $RepositoryRoot 'Examples/Linker/Console-Provider.wva') -o $LinkProviderObject
if (
    $LASTEXITCODE -ne 0 -or
    $ProviderAssemblyOutput -notcontains 'SHA-256: 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab'
) {
    throw 'The Stage 0 assembler did not produce the canonical linker provider object.'
}

$WvLinkerMapOutput = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $WindvaleLinkedImage $WindvaleAssemblyObject $LinkProviderObject
if (
    $LASTEXITCODE -ne 0 -or
    $WvLinkerMapOutput -notcontains 'windvale-link-map 1' -or
    $WvLinkerMapOutput -notcontains 'target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24' -or
    $WvLinkerMapOutput -notcontains 'entry name=Main address=1048576' -or
    $WvLinkerMapOutput -notcontains 'image sha256=0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' -or
    $WvLinkerMapOutput -notcontains 'import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592' -or
    $WvLinkerMapOutput -notcontains 'relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=6 patch-address=1048582 target=Console_write target-input=1 target-source-index=0 target-address=1048592 addend=-4 value=6' -or
    $WvLinkerMapOutput -notcontains 'relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576' -or
    $WvLinkerMapOutput -notcontains 'Result: 0' -or
    ($WvLinkerMapOutput -join "`n") -match [regex]::Escape($RepositoryRoot)
) {
    throw 'The Windvale linker did not produce the canonical path-free map.'
}
$WindvaleLinkHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WindvaleLinkedImage).Hash.ToLowerInvariant()
if ($WindvaleLinkHash -ne '0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a') {
    throw "The Windvale linker wrote unexpected image bytes: $WindvaleLinkHash"
}
[System.IO.File]::WriteAllText(
    $WindvaleLinkMap,
    ((($WvLinkerMapOutput | Where-Object { $_ -ne 'Result: 0' }) -join "`n") + "`n"),
    [System.Text.UTF8Encoding]::new($false))
$WindvaleLinkMapHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WindvaleLinkMap).Hash.ToLowerInvariant()
if ($WindvaleLinkMapHash -ne '31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4') {
    throw "The Windvale linker wrote an unexpected canonical map: $WindvaleLinkMapHash"
}

if (Test-Path -LiteralPath $InvalidWindvaleLinkedImage) {
    throw "The invalid Windvale link output unexpectedly exists: $InvalidWindvaleLinkedImage"
}
$WvLinkerUndefinedOutput = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $InvalidWindvaleLinkedImage $WindvaleAssemblyObject 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    ($WvLinkerUndefinedOutput -join "`n") -notmatch 'link status=WVL1005 inputs=1 sections=2 symbols=3 relocations=2 image-bytes=0 entry-address=0 input=0' -or
    $WvLinkerUndefinedOutput -notcontains 'Result: 2'
) {
    throw 'The Windvale linker did not reject an undefined import deterministically.'
}
if (Test-Path -LiteralPath $InvalidWindvaleLinkedImage) {
    throw 'A rejected Windvale link created a partial image.'
}

$WvLinkerExistingFailure = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $WindvaleLinkedImage $WindvaleAssemblyObject 2>&1
if (
    $LASTEXITCODE -ne 0 -or
    ($WvLinkerExistingFailure -join "`n") -notmatch 'link status=WVL1005' -or
    $WvLinkerExistingFailure -notcontains 'Result: 2'
) {
    throw 'The Windvale linker did not reject an invalid link targeting an existing image.'
}
$PreservedWindvaleLinkHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WindvaleLinkedImage).Hash.ToLowerInvariant()
if ($PreservedWindvaleLinkHash -ne $WindvaleLinkHash) {
    throw 'A rejected Windvale link modified an existing image.'
}

$MissingWindvaleLinkParent = Join-Path $Artifacts '__windvale_missing_wvlink_parent__'
if (Test-Path -LiteralPath $MissingWindvaleLinkParent) {
    throw "The missing Windvale linker parent unexpectedly exists: $MissingWindvaleLinkParent"
}
$MissingWindvaleLinkOutput = Join-Path $MissingWindvaleLinkParent 'Hello.bin'
$MissingWindvaleLinkParentOutput = dotnet $ToolDll run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $MissingWindvaleLinkOutput $WindvaleAssemblyObject $LinkProviderObject 2>&1
if ($LASTEXITCODE -ne 3 -or ($MissingWindvaleLinkParentOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The Windvale linker did not report a missing output parent deterministically.'
}
if (Test-Path -LiteralPath $MissingWindvaleLinkOutput) {
    throw 'The failed Windvale linker write left a partial image.'
}

$LinkMapOutput = dotnet $ToolDll link --base-address 1048576 --entry Main -o $LinkedImage $AssemblyObject $LinkProviderObject
if (
    $LASTEXITCODE -ne 0 -or
    $LinkMapOutput -notcontains 'windvale-link-map 1' -or
    $LinkMapOutput -notcontains 'target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24' -or
    $LinkMapOutput -notcontains 'entry name=Main address=1048576' -or
    $LinkMapOutput -notcontains 'image sha256=0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' -or
    $LinkMapOutput -notcontains 'import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592' -or
    $LinkMapOutput -notcontains 'relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=6 patch-address=1048582 target=Console_write target-input=1 target-source-index=0 target-address=1048592 addend=-4 value=6' -or
    $LinkMapOutput -notcontains 'relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576' -or
    ($LinkMapOutput -join "`n") -match [regex]::Escape($RepositoryRoot)
) {
    throw 'The Stage 0 linker did not produce the canonical path-free map.'
}
$LinkHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $LinkedImage).Hash.ToLowerInvariant()
if ($LinkHash -ne '0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a') {
    throw "The Stage 0 linker wrote unexpected image bytes: $LinkHash"
}
[System.IO.File]::WriteAllText(
    $LinkMap,
    (($LinkMapOutput -join "`n") + "`n"),
    [System.Text.UTF8Encoding]::new($false))
$LinkMapHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $LinkMap).Hash.ToLowerInvariant()
if ($LinkMapHash -ne '31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4') {
    throw "The Stage 0 linker wrote an unexpected canonical map: $LinkMapHash"
}
$ImagesMatch = [System.Linq.Enumerable]::SequenceEqual(
    [byte[]][System.IO.File]::ReadAllBytes($WindvaleLinkedImage),
    [byte[]][System.IO.File]::ReadAllBytes($LinkedImage))
if (-not $ImagesMatch) {
    throw 'The Windvale-written and Stage 0 linked images differ.'
}
$MapsMatch = [System.Linq.Enumerable]::SequenceEqual(
    [byte[]][System.IO.File]::ReadAllBytes($WindvaleLinkMap),
    [byte[]][System.IO.File]::ReadAllBytes($LinkMap))
if (-not $MapsMatch) {
    throw 'The Windvale-written and Stage 0 canonical maps differ.'
}

if (Test-Path -LiteralPath $InvalidLinkedImage) {
    throw "The invalid link output unexpectedly exists: $InvalidLinkedImage"
}
$UndefinedLinkOutput = dotnet $ToolDll link --base-address 1048576 --entry Main -o $InvalidLinkedImage $AssemblyObject 2>&1
if ($LASTEXITCODE -ne 1 -or ($UndefinedLinkOutput -join "`n") -notmatch 'WVL1005') {
    throw 'The Stage 0 linker did not reject an undefined import deterministically.'
}
if (Test-Path -LiteralPath $InvalidLinkedImage) {
    throw 'A rejected link created a partial image.'
}

$ExistingLinkFailure = dotnet $ToolDll link --base-address 1048576 --entry Main -o $LinkedImage $AssemblyObject 2>&1
if ($LASTEXITCODE -ne 1 -or ($ExistingLinkFailure -join "`n") -notmatch 'WVL1005') {
    throw 'The Stage 0 linker did not reject an invalid link targeting an existing image.'
}
$PreservedLinkHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $LinkedImage).Hash.ToLowerInvariant()
if ($PreservedLinkHash -ne $LinkHash) {
    throw 'A rejected link modified an existing image.'
}

$MissingLinkParent = Join-Path $Artifacts '__windvale_missing_link_parent__'
if (Test-Path -LiteralPath $MissingLinkParent) {
    throw "The missing linker parent unexpectedly exists: $MissingLinkParent"
}
$MissingLinkOutput = Join-Path $MissingLinkParent 'Hello.bin'
$MissingLinkParentOutput = dotnet $ToolDll link --base-address 1048576 --entry Main -o $MissingLinkOutput $AssemblyObject $LinkProviderObject 2>&1
if ($LASTEXITCODE -ne 74 -or ($MissingLinkParentOutput -join "`n") -notmatch 'I/O failed') {
    throw 'The Stage 0 linker did not report a missing output parent deterministically.'
}
if (Test-Path -LiteralPath $MissingLinkOutput) {
    throw 'The failed linker write left a partial image.'
}

# The final qualification check intentionally observes EX_IOERR from a rejected
# native command. Clear that verified result so callers receive the gate's
# successful outcome instead of the expected child-process exit code.
$global:LASTEXITCODE = 0
Write-Output "Windvale Seed verification passed."
Write-Output "Conformance report: $ReportPath"
