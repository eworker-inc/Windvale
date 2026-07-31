[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$ReportPath,
    [ValidateSet('Fast', 'Standard', 'Qualification')]
    [string]$Level = 'Qualification',
    [string]$TestFilter,
    [ValidateSet('assembler', 'bytecode', 'compiler', 'foundation', 'golden', 'linker', 'object-model', 'runtime')]
    [string[]]$TestArea,
    [switch]$FailFast,
    [string]$TimingReportPath
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ToolDll = Join-Path $RepositoryRoot "Tools/Windvale.Tool/bin/$Configuration/net10.0/windvale.dll"
$TestProject = Join-Path $RepositoryRoot 'Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj'
$Artifacts = Join-Path $RepositoryRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null
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
    throw 'Test selection is available only with -Level Fast; Standard and Qualification require all tests.'
}
if ($Level -ne 'Fast' -and $FailFast) {
    throw '-FailFast is available only with -Level Fast; Standard and Qualification require the complete suite.'
}
if ($Level -ne 'Fast' -and [string]::IsNullOrWhiteSpace($ReportPath)) {
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
    Write-Host "Windvale Seed fast verification passed for $($Selection -join ' and ')."
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
$HelloModule = Join-Path $Artifacts 'Hello-Windvale.wvb'
$FoundationModule = Join-Path $Artifacts 'Read-Wvb-Header.wvb'
$CompositionModule = Join-Path $Artifacts 'Module-Composition-Demo.wvb'
$CompositionReorderedModule = Join-Path $Artifacts 'Module-Composition-Demo-Reordered.wvb'
$InvalidCompositionModule = Join-Path $Artifacts '__windvale_invalid_composition_output__.wvb'
$MachineContractsModule = Join-Path $Artifacts 'Machine-Contracts.wvb'
$MachineContractsDemoModule = Join-Path $Artifacts 'Machine-Contracts-Demo.wvb'
$ByteOrderingModule = Join-Path $Artifacts 'Byte-Ordering.wvb'
$ByteOrderingDemoModule = Join-Path $Artifacts 'Byte-Ordering-Demo.wvb'
$DecimalParsingModule = Join-Path $Artifacts 'Decimal-Parsing.wvb'
$DecimalParsingDemoModule = Join-Path $Artifacts 'Decimal-Parsing-Demo.wvb'
$ByteConstructionModule = Join-Path $Artifacts 'Byte-Construction.wvb'
$ByteConstructionDemoModule = Join-Path $Artifacts 'Byte-Construction-Demo.wvb'
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
if ($LASTEXITCODE -ne 0 -or $RunOutput -notcontains 'Result: 29') {
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
if ($CompositionHash -ne '0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60') {
    throw "The composed source module has an unexpected digest: $CompositionHash"
}
$CompositionRunOutput = dotnet $ToolDll run $CompositionModule
if ($LASTEXITCODE -ne 0 -or $CompositionRunOutput -notcontains 'Result: 42') {
    throw 'The composed source module did not return Result: 42.'
}
dotnet $ToolDll `
    compile $CompositionRoot --module $CompositionLeaf --module $CompositionMiddle -o $CompositionReorderedModule
if ($LASTEXITCODE -ne 0) { throw 'The reordered source-module compile failed.' }
$CompositionReorderedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $CompositionReorderedModule).Hash.ToLowerInvariant()
if ($CompositionReorderedHash -ne $CompositionHash) {
    throw 'Reordering explicit source-module inputs changed the composed WVB bytes.'
}
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
if ($MachineContractsHash -ne '9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a') {
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
if ($MachineContractsDemoHash -ne 'b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a') {
    throw "The Foundation machine-contract demo has an unexpected digest: $MachineContractsDemoHash"
}
$MachineContractsDemoOutput = dotnet $ToolDll run $MachineContractsDemoModule
if ($LASTEXITCODE -ne 0 -or $MachineContractsDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation machine-contract demo did not return Result: 0.'
}

$ByteOrderingSource = Join-Path $RepositoryRoot 'Foundation/Byte-Ordering.wv'
dotnet $ToolDll `
    compile $ByteOrderingSource -o $ByteOrderingModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation byte ordering.' }
$ByteOrderingHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteOrderingModule).Hash.ToLowerInvariant()
if ($ByteOrderingHash -ne '194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1') {
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
if ($ByteOrderingDemoHash -ne '0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab') {
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
if ($DecimalParsingHash -ne '39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f') {
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
if ($DecimalParsingDemoHash -ne '16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695') {
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
if ($ByteConstructionHash -ne '6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207') {
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
if ($ByteConstructionDemoHash -ne 'a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29') {
    throw "The Foundation byte-construction demo has an unexpected digest: $ByteConstructionDemoHash"
}
$ByteConstructionDemoOutput = dotnet $ToolDll run $ByteConstructionDemoModule
if ($LASTEXITCODE -ne 0 -or $ByteConstructionDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation byte-construction demo did not return Result: 0.'
}

$SourceLexerSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Lexer-Core.wv'
dotnet $ToolDll `
    compile $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceLexerModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source lexer.' }
$SourceLexerHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceLexerModule).Hash.ToLowerInvariant()
if ($SourceLexerHash -ne '0a9d5ff05afbe8598491ca636029fdfc7577dda754a048b93b0529d549019b04') {
    throw "The Windvale source lexer has an unexpected digest: $SourceLexerHash"
}
$SourceLexerInspection = (dotnet $ToolDll inspect $SourceLexerModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceLexerInspection -notmatch 'Nominal types \(6\)' -or
    $SourceLexerInspection -notmatch 'Compilerˉsourceˉtoken' -or
    $SourceLexerInspection -notmatch 'Compilerˉtokenˉkind' -or
    $SourceLexerInspection -notmatch 'Compilerˉlexˉsourceˉbounded' -or
    $SourceLexerInspection -notmatch 'Exports \(14\)'
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
if ($SourceLexerDemoHash -ne '32429c56b1b027fc440de14487ac0b5c628cec3c9bded1a98c1c21e6cbeed05a') {
    throw "The Windvale source-lexer demo has an unexpected digest: $SourceLexerDemoHash"
}
$SourceLexerDemoOutput = dotnet $ToolDll `
    run $SourceLexerDemoModule --max-steps 10000000
if ($LASTEXITCODE -ne 0 -or $SourceLexerDemoOutput -notcontains 'Result: 0') {
    throw 'The Windvale source-lexer demo did not return Result: 0.'
}

$SourceDeclarationParserSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Declaration-Parser.wv'
dotnet $ToolDll `
    compile $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceDeclarationParserModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale declaration parser.' }
$SourceDeclarationParserHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceDeclarationParserModule).Hash.ToLowerInvariant()
if ($SourceDeclarationParserHash -ne 'b09be82c374636bf0b75a0dcea21afa648d89676e0fb0ffedcef68f9e958ee61') {
    throw "The Windvale declaration parser has an unexpected digest: $SourceDeclarationParserHash"
}
$SourceDeclarationParserInspection = (dotnet $ToolDll inspect $SourceDeclarationParserModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceDeclarationParserInspection -notmatch 'Nominal types \(14\)' -or
    $SourceDeclarationParserInspection -notmatch 'Compilerˉsourceˉdeclaration' -or
    $SourceDeclarationParserInspection -notmatch 'Compilerˉsourceˉmoduleˉsummary' -or
    $SourceDeclarationParserInspection -notmatch 'Compilerˉparseˉnextˉdeclarationˉvalidated' -or
    $SourceDeclarationParserInspection -notmatch 'Exports \(24\)'
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
if ($SourceDeclarationParserDemoHash -ne '82dd2f72d2b2d148289353045fda861e07638e8fac8ba97164642d185c3b8e9a') {
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
if ($SourceDeclarationParserToolHash -ne '36406acea0ccab9cf9f91cc9723638ae133daa1d5893dcf64454a983427a520c') {
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
    $SourceLexerDeclarationOutput -notcontains 'source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=14 tokens=4715 offset=39210' -or
    $SourceLexerDeclarationOutput -notcontains 'Result: 0'
) {
    throw 'The declaration-parser tool did not parse the real Windvale lexer source.'
}
$SourceParserSelfDeclarationOutput = dotnet $ToolDll `
    @SourceDeclarationParserArguments --max-steps 45000000 -- $SourceDeclarationParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceParserSelfDeclarationOutput -notcontains 'source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=24 tokens=8876 offset=64950' -or
    $SourceParserSelfDeclarationOutput -notcontains 'Result: 0'
) {
    throw 'The declaration-parser tool did not parse its own declaration source.'
}

$SourceBodyParserSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Body-Parser.wv'
dotnet $ToolDll `
    compile $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceBodyParserModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale body parser.' }
$SourceBodyParserHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceBodyParserModule).Hash.ToLowerInvariant()
if ($SourceBodyParserHash -ne 'bb04309dfd4b037c05a4f0d52903d937336e90e64077fbc1b78cf5ea88c1de5f') {
    throw "The Windvale body parser has an unexpected digest: $SourceBodyParserHash"
}
$SourceBodyParserInspection = (dotnet $ToolDll inspect $SourceBodyParserModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBodyParserInspection -notmatch 'Nominal types \(23\)' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉsourceˉexpression' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉsourceˉstatement' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉparseˉexpressionˉvalidated' -or
    $SourceBodyParserInspection -notmatch 'Compilerˉparseˉsourceˉbodies' -or
    $SourceBodyParserInspection -notmatch 'Exports \(38\)'
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
if ($SourceBodyParserDemoHash -ne '5c479f4e922852043696a599a7832a4111d326ef54ce8222166caf3570ec28ba') {
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
if ($SourceBodyParserToolHash -ne '761887d3674833854d976dd394ad3f83f27d2c74748b6dd0f296c97b117140ca') {
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
    $SourceLexerBodyOutput -notcontains 'source bodies status=Valid functions=14 top-level=138 statements=510 expression-nodes=1432 statement-depth=17 expression-depth=5 offset=39211' -or
    $SourceLexerBodyOutput -notcontains 'Result: 0'
) {
    throw 'The body-parser tool did not parse the real Windvale lexer bodies.'
}
$SourceDeclarationBodyOutput = dotnet $ToolDll `
    @SourceBodyParserArguments --max-steps 160000000 -- $SourceDeclarationParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceDeclarationBodyOutput -notcontains 'source bodies status=Valid functions=24 top-level=232 statements=527 expression-nodes=2135 statement-depth=5 expression-depth=3 offset=64951' -or
    $SourceDeclarationBodyOutput -notcontains 'Result: 0'
) {
    throw 'The body-parser tool did not parse the declaration-parser bodies.'
}
$SourceBodySelfOutput = dotnet $ToolDll `
    @SourceBodyParserArguments --max-steps 160000000 -- $SourceBodyParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBodySelfOutput -notcontains 'source bodies status=Valid functions=38 top-level=234 statements=519 expression-nodes=2500 statement-depth=5 expression-depth=3 offset=69023' -or
    $SourceBodySelfOutput -notcontains 'Result: 0'
) {
    throw 'The body-parser tool did not parse its own statement and expression source.'
}

$SourceSetSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Set-Core.wv'
dotnet $ToolDll `
    compile $SourceSetSource `
    --module $SourceBodyParserSource `
    --module $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceSetModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-set core.' }
$SourceSetHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceSetModule).Hash.ToLowerInvariant()
if ($SourceSetHash -ne 'c03b3e9daa5b20fc2f77a0d1dd15cb1fdc1728e2a6eda021aa766b19b1bfa2b8') {
    throw "The Windvale source-set core has an unexpected digest: $SourceSetHash"
}
$SourceSetInspection = (dotnet $ToolDll inspect $SourceSetModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceSetInspection -notmatch 'Nominal types \(27\)' -or
    $SourceSetInspection -notmatch 'Compilerˉsourceˉsetˉscan' -or
    $SourceSetInspection -notmatch 'Compilerˉsourceˉsetˉsummary' -or
    $SourceSetInspection -notmatch 'Compilerˉscanˉsourceˉset' -or
    $SourceSetInspection -notmatch 'Compilerˉvalidateˉsourceˉset' -or
    $SourceSetInspection -notmatch 'Exports \(9\)'
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
if ($SourceSetDemoHash -ne '0054138c6e39f3c99e5cd4751c796cd599b495880d7db174323342fb7b687488') {
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
if ($SourceSetToolHash -ne 'dc290826985f66f80d469b99235ca290dc617997edee0aab2ea0d4227984aab6') {
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
    $SourceSetSelfOutput -notcontains 'source set status=Valid modules=5 source-bytes=192171 imports=4 records=16 enums=11 functions=86' -or
    $SourceSetSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-set tool did not validate the real compiler frontend set.'
}

$SourceGraphSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Graph-Core.wv'
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
if ($SourceGraphHash -ne '1617419c838effd80e4ab3f167912f47f4959002a77b0b166970b1d8f30f3133') {
    throw "The Windvale source-graph core has an unexpected digest: $SourceGraphHash"
}
$SourceGraphInspection = (dotnet $ToolDll inspect $SourceGraphModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceGraphInspection -notmatch 'Nominal types \(32\)' -or
    $SourceGraphInspection -notmatch 'Compilerˉsourceˉgraphˉstatus' -or
    $SourceGraphInspection -notmatch 'Compilerˉsourceˉgraphˉsummary' -or
    $SourceGraphInspection -notmatch 'Compilerˉvalidateˉsourceˉgraph' -or
    $SourceGraphInspection -notmatch 'Exports \(11\)'
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
if ($SourceGraphDemoHash -ne '53c976f867dccf60bf26aa74e3942cf877b048405f57dd42e462dbe0b63c9073') {
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
if ($SourceGraphToolHash -ne '75fdf22e93f154599cdf4530ebcf828eec061458c73f6ab09b00d0765e3ebdc1') {
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
    $SourceGraphSelfOutput -notcontains 'source graph status=Valid modules=7 imports=6 reachable=7' -or
    $SourceGraphSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-graph tool did not validate the real compiler graph.'
}

$SourceSymbolsSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Symbols-Core.wv'
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
if ($SourceSymbolsHash -ne '624fd35749645c0cf269c6d298303b614efad1e112e86cb045016485386d58f6') {
    throw "The Windvale source-symbol core has an unexpected digest: $SourceSymbolsHash"
}
$SourceSymbolsInspection = (dotnet $ToolDll inspect $SourceSymbolsModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceSymbolsInspection -notmatch 'Nominal types \(38\)' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉsourceˉsymbolˉstatus' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉsourceˉsymbolˉsummary' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid' -or
    $SourceSymbolsInspection -notmatch 'Compilerˉvalidateˉsourceˉsymbols' -or
    $SourceSymbolsInspection -notmatch 'Exports \(36\)'
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
if ($SourceSymbolsDemoHash -ne 'ca513e0ea10a84f6c5ccc630927b3c18793b6c2e3d1badabffab08fdcdd2146c') {
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
if ($SourceSymbolsToolHash -ne '840492af48d93af014fb12c59b6711752e80519d50ec45dbecee4483b42dce05') {
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
    $SourceSymbolsSelfOutput -notcontains 'source symbols status=Valid modules=8 capabilities=0 data=0 records=24 enums=14 functions=135 fields=290 members=181 parameters=597 directory-bytes=4168 visibility-bytes=64' -or
    $SourceSymbolsSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-symbol tool did not bind the real compiler closure.'
}

$SourceBindingsSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Bindings-Core.wv'
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
if ($SourceBindingsHash -ne '5440db53fa42f819321d91f67a2440cf128a8685e564b057d61d1b7ee9a2e1c3') {
    throw "The Windvale source-binding core has an unexpected digest: $SourceBindingsHash"
}
$SourceBindingsInspection = (dotnet $ToolDll inspect $SourceBindingsModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceBindingsInspection -notmatch 'Nominal types \(47\)' -or
    $SourceBindingsInspection -notmatch 'Compilerˉsourceˉbindingˉstatus' -or
    $SourceBindingsInspection -notmatch 'Compilerˉsourceˉbindingˉsummary' -or
    $SourceBindingsInspection -notmatch 'Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid' -or
    $SourceBindingsInspection -notmatch 'Compilerˉvalidateˉsourceˉbindings' -or
    $SourceBindingsInspection -notmatch 'Exports \(54\)'
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
if ($SourceBindingsDemoHash -ne '1dabd3cb09339e63f2d8495eb27258fce1054c5163f92a5afd3258995d5bb8e5') {
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
if ($SourceBindingsToolHash -ne '47dab8589a4ca5a7c460be981fc183a5efcdb551174e1bb734b415f4f4198cb8') {
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
    $SourceBindingsSelfOutput -notcontains 'source bindings status=Valid modules=9 functions=189 parameters=827 locals=942 reads=8277 assignments=609 calls=1475 directory-bytes=65596' -or
    $SourceBindingsSelfOutput -notcontains 'Result: 0'
) {
    throw 'The source-binding tool did not bind the real compiler closure.'
}

$SourceWirSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Wir-Core.wv'
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
if ($SourceWirHash -ne '95b63f627f05efbdc95d65da7654a69dde89a7e7ec38e2ed80d3f5cfbff5c17f') {
    throw "The Windvale typed-WVIR core has an unexpected digest: $SourceWirHash"
}
$SourceWirInspection = (dotnet $ToolDll inspect $SourceWirModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWirInspection -notmatch 'Compilerˉsourceˉwirˉoperation' -or
    $SourceWirInspection -notmatch 'Compilerˉsourceˉwirˉsummary' -or
    $SourceWirInspection -notmatch 'Compilerˉsourceˉwirˉdirectoryˉisˉvalid' -or
    $SourceWirInspection -notmatch 'Compilerˉvalidateˉsourceˉwir' -or
    $SourceWirInspection -notmatch 'Exports \(64\)'
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
if ($SourceWirDemoHash -ne '23da120afffa4f450f703fc17a9719f1ed6c9a080ced67fbcd23bb7aa232f42e') {
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
if ($SourceWirToolHash -ne '2d8cbae0f87e2043eb18e43b6e6659044ada2f8c453cc17ca1b406efb6df517e') {
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

$SourceWvbSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Wvb-Core.wv'
$SourceWvbDependencies = @(
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
if ($SourceWvbHash -ne 'bb259fe85c6593e3e13091faf77252468fd85ad5d0d2f73e4d6f450bacbd83a6') {
    throw "The Windvale WVB backend core has an unexpected digest: $SourceWvbHash"
}
$SourceWvbInspection = (dotnet $ToolDll inspect $SourceWvbModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $SourceWvbInspection -notmatch 'Compilerˉsourceˉwvbˉsummary' -or
    $SourceWvbInspection -notmatch 'Compilerˉcompileˉsourceˉwvb' -or
    $SourceWvbInspection -notmatch 'Exports \(55\)'
) {
    throw 'The Windvale WVB backend inspection is incomplete.'
}
dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Wvb-Demo.wv') `
    --module $SourceWvbSource @SourceWvbDependencies `
    -o $SourceWvbDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale WVB backend demo.' }
$SourceWvbDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceWvbDemoModule).Hash.ToLowerInvariant()
if ($SourceWvbDemoHash -ne 'a873e870d005fb49cd48043d5f8d3f54c33d889bc2ab5f118f7c09b0f90f5fa0') {
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
if ($SourceWvbToolHash -ne 'b85ff9d1be9815304617be13d532636ab0a5a3ce2d018a610565ca067e52245c') {
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
    $SourceWvbFixtureOutput -notcontains 'source wvb status=Valid functions=4 code-bytes=532 module-bytes=815' -or
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
if ($SourceWvbFixtureHash -ne '9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761') {
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
    $SourceWvbDataFixtureOutput -notcontains 'source wvb status=Valid functions=3 code-bytes=1210 module-bytes=1651' -or
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
if ($SourceWvbDataFixtureHash -ne '5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704') {
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
    $SourceWvbNominalFixtureOutput -notcontains 'source wvb status=Valid functions=3 code-bytes=1097 module-bytes=1781' -or
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
if ($SourceWvbNominalFixtureHash -ne '1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a') {
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
    $SourceWvbHostedFixtureOutput -notcontains 'source wvb status=Valid functions=7 code-bytes=249 module-bytes=849' -or
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
if ($SourceWvbHostedFixtureHash -ne '1df4503a21abf5f2c0b0307ac2dc79402bc8550ec5e4a016df43fdeb8197d528') {
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
    $SourceWvbCompositionOutput -notcontains 'source wvb status=Valid functions=5 code-bytes=451 module-bytes=1030' -or
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
    $SourceWvbCompositionInspection -notmatch 'Data \(3\)' -or
    $SourceWvbCompositionInspection -notmatch '\[2\] __Text_000001: text' -or
    $SourceWvbCompositionInspection -notmatch 'Nominal types \(2\)' -or
    $SourceWvbCompositionInspection -notmatch 'Functions \(5\)' -or
    $SourceWvbCompositionInspection -notmatch 'Exports \(1\)' -or
    $SourceWvbCompositionInspection -notmatch 'Main -> function\[4\]'
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
if ($SourceWvbCompositionHash -ne '7279011a12f3d2becc1e9775fb92bd7c74b8760b2c94f13a282d71c0849f8e6f') {
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
    $WvDumpHostedOutput -notcontains 'module version=1.6 profile=portable name="Sum\u02C9data"' -or
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
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Wvo-Object-Core.wv') `
    --module $ByteOrderingSource `
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
    $WvoCoreInspection -notmatch 'Foundationˉbyteˉspansˉcompare' -or
    $WvoCoreInspection -notmatch 'file\.write_bytes'
) {
    throw 'The Seed CLI inspector did not expose the Windvale object writer operations.'
}

$WvoCapabilities = @(
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '10000000'
)

$WvoUnauthorizedOutput = dotnet $ToolDll run $WvoCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WVO writer capabilities.'
}

$WvoSelfTestOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities
if ($LASTEXITCODE -ne 0 -or $WvoSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale object core self-test did not return Result: 0.'
}

$WvoHostedOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- $WvoSample
if (
    $LASTEXITCODE -ne 0 -or
    $WvoHostedOutput -notcontains 'Wrote WVO 1.0 bytes=189' -or
    $WvoHostedOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale object core did not write the expected native-host object.'
}

$WvoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $WvoSample).Hash.ToLowerInvariant()
if ($WvoHash -ne '006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a') {
    throw "The Windvale object core wrote unexpected bytes: $WvoHash"
}

$WvoVerifyOutput = dotnet $ToolDll object-verify $WvoSample
if ($LASTEXITCODE -ne 0 -or $WvoVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The object verifier rejected the Windvale-written sample.'
}

$WvoInspection = (dotnet $ToolDll object-inspect $WvoSample) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvoInspection -notmatch 'Sections \(2\)' -or
    $WvoInspection -notmatch 'Console_write binding=Import' -or
    $WvoInspection -notmatch 'kind=Relativeˉi32 section=0 offset=1 symbol=2 addend=-4'
) {
    throw 'The object inspector did not expose the expected symbol and relocation records.'
}

$WvoInvalidNameOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- '' 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoInvalidNameOutput -join "`n") -notmatch 'WVR3021') {
    throw 'The hosted file writer did not reject an empty resource name deterministically.'
}

$MissingWriterParent = Join-Path $Artifacts '__windvale_missing_writer_parent__'
if (Test-Path -LiteralPath $MissingWriterParent) {
    throw "The missing writer parent unexpectedly exists: $MissingWriterParent"
}
$WvoMissingParentOutput = dotnet $ToolDll run $WvoCoreModule @WvoCapabilities -- (Join-Path $MissingWriterParent 'Sample.wvo') 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoMissingParentOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The hosted file writer did not report a missing parent deterministically.'
}

dotnet $ToolDll `
    compile (Join-Path $RepositoryRoot 'Examples/Assembler/Wva-Assembler-Core.wv') `
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
    $WvaAssemblerInspection -notmatch 'Foundationˉmachineˉnameˉisˉvalid' -or
    $WvaAssemblerInspection -notmatch 'Foundationˉbyteˉspansˉcompare' -or
    $WvaAssemblerInspection -notmatch 'Foundationˉu32ˉdecimalˉparse' -or
    $WvaAssemblerInspection -notmatch 'Foundationˉbytesˉrepeat' -or
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
    compile (Join-Path $RepositoryRoot 'Examples/Linker/Wv-Linker-Core.wv') `
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
    $WvLinkerInspection -notmatch 'Foundationˉalignmentˉisˉvalid' -or
    $WvLinkerInspection -notmatch 'Foundationˉbyteˉspansˉcompare' -or
    $WvLinkerInspection -notmatch 'Foundationˉu32ˉdecimalˉparse' -or
    $WvLinkerInspection -notmatch 'Foundationˉbytesˉrepeat' -or
    $WvLinkerInspection -notmatch 'Foundationˉbytesˉreplace' -or
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

Write-Output "Windvale Seed verification passed."
Write-Output "Conformance report: $ReportPath"
