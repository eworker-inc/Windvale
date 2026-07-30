[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$ReportPath
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ToolProject = Join-Path $RepositoryRoot 'Tools/Windvale.Tool/Windvale.Tool.csproj'
$Artifacts = Join-Path $RepositoryRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null
if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $Architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
    $ReportPath = Join-Path $RepositoryRoot "artifacts/seed-conformance-windows-$Architecture.json"
}

dotnet build (Join-Path $RepositoryRoot 'Windvale.slnx') --configuration $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Windvale Seed build failed with exit code $LASTEXITCODE."
}

dotnet run `
    --project (Join-Path $RepositoryRoot 'Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj') `
    --configuration $Configuration `
    --no-build `
    -- `
    --report $ReportPath
if ($LASTEXITCODE -ne 0) {
    throw "Windvale Seed conformance tests failed with exit code $LASTEXITCODE."
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
dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') -o $SumModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Sum-Data.wv.' }

$VerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $SumModule
if ($LASTEXITCODE -ne 0 -or $VerifyOutput -notcontains 'Verified: Sumˉdata') {
    $VerifyText = $VerifyOutput -join ' | '
    throw "The Seed CLI failed to verify Sum-Data.wvb (exit $LASTEXITCODE; output: $VerifyText)."
}

$InspectOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $SumModule
if ($LASTEXITCODE -ne 0 -or ($InspectOutput -join "`n") -notmatch 'data\.load\.i32') {
    throw 'The Seed CLI inspector did not expose the expected data instruction.'
}

$RunOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $SumModule
if ($LASTEXITCODE -ne 0 -or $RunOutput -notcontains 'Result: 29') {
    throw 'The Seed CLI did not produce Result: 29 for Sum-Data.wvb.'
}

dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Seed/Hello-Windvale.wv') -o $HelloModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Hello-Windvale.wv.' }

$UnauthorizedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $HelloModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($UnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse an ungranted console capability.'
}

$HelloOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $HelloModule --allow console.write_line
if ($LASTEXITCODE -ne 0 -or $HelloOutput -notcontains 'Hello from Windvale' -or $HelloOutput -notcontains 'Result: 0') {
    throw 'The Seed CLI did not run the authorized Hello-Windvale module correctly.'
}

dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Foundation/Read-Wvb-Header.wv') -o $FoundationModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Read-Wvb-Header.wv.' }

$FoundationVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $FoundationModule
if ($LASTEXITCODE -ne 0 -or $FoundationVerifyOutput -notcontains 'Verified: Readˉwvbˉheader') {
    throw 'The Seed CLI failed to verify Read-Wvb-Header.wvb.'
}

$FoundationInspectOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $FoundationModule
if ($LASTEXITCODE -ne 0 -or ($FoundationInspectOutput -join "`n") -notmatch 'bytes\.read_u32_little') {
    throw 'The Seed CLI inspector did not expose the expected little-endian read instruction.'
}

$FoundationRunOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $FoundationModule
if ($LASTEXITCODE -ne 0 -or $FoundationRunOutput -notcontains 'Result: 1') {
    throw 'The Seed CLI did not produce Result: 1 for Read-Wvb-Header.wvb.'
}

$CompositionRoot = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Demo.wv'
$CompositionMiddle = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Middle.wv'
$CompositionLeaf = Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Leaf.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $CompositionRoot --module $CompositionMiddle --module $CompositionLeaf -o $CompositionModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the source-module composition demo.' }
$CompositionHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $CompositionModule).Hash.ToLowerInvariant()
if ($CompositionHash -ne '0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60') {
    throw "The composed source module has an unexpected digest: $CompositionHash"
}
$CompositionRunOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $CompositionModule
if ($LASTEXITCODE -ne 0 -or $CompositionRunOutput -notcontains 'Result: 42') {
    throw 'The composed source module did not return Result: 42.'
}
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $CompositionRoot --module $CompositionLeaf --module $CompositionMiddle -o $CompositionReorderedModule
if ($LASTEXITCODE -ne 0) { throw 'The reordered source-module compile failed.' }
$CompositionReorderedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $CompositionReorderedModule).Hash.ToLowerInvariant()
if ($CompositionReorderedHash -ne $CompositionHash) {
    throw 'Reordering explicit source-module inputs changed the composed WVB bytes.'
}
if (Test-Path -LiteralPath $InvalidCompositionModule) {
    Remove-Item -LiteralPath $InvalidCompositionModule -Force
}
$MissingCompositionOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $CompositionRoot --module $CompositionMiddle -o $InvalidCompositionModule 2>&1
if ($LASTEXITCODE -ne 1 -or ($MissingCompositionOutput -join "`n") -notmatch 'WVC0007') {
    throw 'The source-module compiler did not reject a missing transitive import.'
}
if (Test-Path -LiteralPath $InvalidCompositionModule) {
    throw 'A rejected source-module composition created an output module.'
}
[System.IO.File]::WriteAllBytes($InvalidCompositionModule, [byte[]](9, 8, 7))
$MissingCompositionOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $CompositionRoot --module $CompositionMiddle -o $InvalidCompositionModule 2>&1
if ($LASTEXITCODE -ne 1 -or ($MissingCompositionOutput -join "`n") -notmatch 'WVC0007') {
    throw 'The repeated missing-import composition did not fail deterministically.'
}
if ([Convert]::ToHexString([System.IO.File]::ReadAllBytes($InvalidCompositionModule)) -ne '090807') {
    throw 'A rejected source-module composition modified an existing output module.'
}
Remove-Item -LiteralPath $InvalidCompositionModule -Force

$MachineContractsSource = Join-Path $RepositoryRoot 'Foundation/Machine-Contracts.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $MachineContractsSource -o $MachineContractsModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation machine contracts.' }
$MachineContractsHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $MachineContractsModule).Hash.ToLowerInvariant()
if ($MachineContractsHash -ne '9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a') {
    throw "The Foundation machine-contract module has an unexpected digest: $MachineContractsHash"
}
$MachineContractsInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $MachineContractsModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $MachineContractsInspection -notmatch 'Foundationˉalignmentˉisˉvalid' -or
    $MachineContractsInspection -notmatch 'Foundationˉmachineˉnameˉisˉvalid' -or
    $MachineContractsInspection -notmatch 'Exports \(2\)'
) {
    throw 'The Foundation machine-contract module inspection is incomplete.'
}
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Machine-Contracts-Demo.wv') `
    --module $MachineContractsSource `
    -o $MachineContractsDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation machine-contract demo.' }
$MachineContractsDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $MachineContractsDemoModule).Hash.ToLowerInvariant()
if ($MachineContractsDemoHash -ne 'b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a') {
    throw "The Foundation machine-contract demo has an unexpected digest: $MachineContractsDemoHash"
}
$MachineContractsDemoOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $MachineContractsDemoModule
if ($LASTEXITCODE -ne 0 -or $MachineContractsDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation machine-contract demo did not return Result: 0.'
}

$ByteOrderingSource = Join-Path $RepositoryRoot 'Foundation/Byte-Ordering.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $ByteOrderingSource -o $ByteOrderingModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation byte ordering.' }
$ByteOrderingHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteOrderingModule).Hash.ToLowerInvariant()
if ($ByteOrderingHash -ne '194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1') {
    throw "The Foundation byte-ordering module has an unexpected digest: $ByteOrderingHash"
}
$ByteOrderingInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $ByteOrderingModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $ByteOrderingInspection -notmatch 'Foundationˉbyteˉspansˉcompare' -or
    $ByteOrderingInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Foundation byte-ordering module inspection is incomplete.'
}
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Byte-Ordering-Demo.wv') `
    --module $ByteOrderingSource `
    -o $ByteOrderingDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation byte-ordering demo.' }
$ByteOrderingDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteOrderingDemoModule).Hash.ToLowerInvariant()
if ($ByteOrderingDemoHash -ne '0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab') {
    throw "The Foundation byte-ordering demo has an unexpected digest: $ByteOrderingDemoHash"
}
$ByteOrderingDemoOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $ByteOrderingDemoModule
if ($LASTEXITCODE -ne 0 -or $ByteOrderingDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation byte-ordering demo did not return Result: 0.'
}

$DecimalParsingSource = Join-Path $RepositoryRoot 'Foundation/Decimal-Parsing.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $DecimalParsingSource -o $DecimalParsingModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation decimal parsing.' }
$DecimalParsingHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $DecimalParsingModule).Hash.ToLowerInvariant()
if ($DecimalParsingHash -ne '39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f') {
    throw "The Foundation decimal-parsing module has an unexpected digest: $DecimalParsingHash"
}
$DecimalParsingInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $DecimalParsingModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $DecimalParsingInspection -notmatch 'Foundationˉu32ˉparse' -or
    $DecimalParsingInspection -notmatch 'Foundationˉu32ˉdecimalˉparse' -or
    $DecimalParsingInspection -notmatch 'Exports \(1\)'
) {
    throw 'The Foundation decimal-parsing module inspection is incomplete.'
}
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Decimal-Parsing-Demo.wv') `
    --module $DecimalParsingSource `
    -o $DecimalParsingDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation decimal-parsing demo.' }
$DecimalParsingDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $DecimalParsingDemoModule).Hash.ToLowerInvariant()
if ($DecimalParsingDemoHash -ne '16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695') {
    throw "The Foundation decimal-parsing demo has an unexpected digest: $DecimalParsingDemoHash"
}
$DecimalParsingDemoOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $DecimalParsingDemoModule
if ($LASTEXITCODE -ne 0 -or $DecimalParsingDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation decimal-parsing demo did not return Result: 0.'
}

$ByteConstructionSource = Join-Path $RepositoryRoot 'Foundation/Byte-Construction.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $ByteConstructionSource -o $ByteConstructionModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Foundation byte construction.' }
$ByteConstructionHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteConstructionModule).Hash.ToLowerInvariant()
if ($ByteConstructionHash -ne '6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207') {
    throw "The Foundation byte-construction module has an unexpected digest: $ByteConstructionHash"
}
$ByteConstructionInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $ByteConstructionModule) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $ByteConstructionInspection -notmatch 'Foundationˉbytesˉresult' -or
    $ByteConstructionInspection -notmatch 'Foundationˉbytesˉrepeat' -or
    $ByteConstructionInspection -notmatch 'Foundationˉbytesˉreplace' -or
    $ByteConstructionInspection -notmatch 'Exports \(2\)'
) {
    throw 'The Foundation byte-construction module inspection is incomplete.'
}
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Byte-Construction-Demo.wv') `
    --module $ByteConstructionSource `
    -o $ByteConstructionDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Foundation byte-construction demo.' }
$ByteConstructionDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ByteConstructionDemoModule).Hash.ToLowerInvariant()
if ($ByteConstructionDemoHash -ne 'a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29') {
    throw "The Foundation byte-construction demo has an unexpected digest: $ByteConstructionDemoHash"
}
$ByteConstructionDemoOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $ByteConstructionDemoModule
if ($LASTEXITCODE -ne 0 -or $ByteConstructionDemoOutput -notcontains 'Result: 0') {
    throw 'The Foundation byte-construction demo did not return Result: 0.'
}

$SourceLexerSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Lexer-Core.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceLexerModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source lexer.' }
$SourceLexerHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceLexerModule).Hash.ToLowerInvariant()
if ($SourceLexerHash -ne '0a9d5ff05afbe8598491ca636029fdfc7577dda754a048b93b0529d549019b04') {
    throw "The Windvale source lexer has an unexpected digest: $SourceLexerHash"
}
$SourceLexerInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $SourceLexerModule) -join "`n"
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
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Compiler/Source-Lexer-Demo.wv') `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceLexerDemoModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale source-lexer demo.' }
$SourceLexerDemoHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceLexerDemoModule).Hash.ToLowerInvariant()
if ($SourceLexerDemoHash -ne '32429c56b1b027fc440de14487ac0b5c628cec3c9bded1a98c1c21e6cbeed05a') {
    throw "The Windvale source-lexer demo has an unexpected digest: $SourceLexerDemoHash"
}
$SourceLexerDemoOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    run $SourceLexerDemoModule --max-steps 10000000
if ($LASTEXITCODE -ne 0 -or $SourceLexerDemoOutput -notcontains 'Result: 0') {
    throw 'The Windvale source-lexer demo did not return Result: 0.'
}

$SourceDeclarationParserSource = Join-Path $RepositoryRoot 'Compiler/Bootstrap/Source-Declaration-Parser.wv'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile $SourceDeclarationParserSource `
    --module $SourceLexerSource `
    --module $DecimalParsingSource `
    -o $SourceDeclarationParserModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale declaration parser.' }
$SourceDeclarationParserHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceDeclarationParserModule).Hash.ToLowerInvariant()
if ($SourceDeclarationParserHash -ne 'b09be82c374636bf0b75a0dcea21afa648d89676e0fb0ffedcef68f9e958ee61') {
    throw "The Windvale declaration parser has an unexpected digest: $SourceDeclarationParserHash"
}
$SourceDeclarationParserInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $SourceDeclarationParserModule) -join "`n"
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
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
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
$SourceDeclarationParserDemoOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    run $SourceDeclarationParserDemoModule --max-steps 20000000
if ($LASTEXITCODE -ne 0 -or $SourceDeclarationParserDemoOutput -notcontains 'Result: 0') {
    throw 'The declaration-parser demo did not return Result: 0.'
}
dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
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
$SourceLexerDeclarationOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    @SourceDeclarationParserArguments --max-steps 30000000 -- $SourceLexerSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceLexerDeclarationOutput -notcontains 'source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=14 tokens=4715 offset=39210' -or
    $SourceLexerDeclarationOutput -notcontains 'Result: 0'
) {
    throw 'The declaration-parser tool did not parse the real Windvale lexer source.'
}
$SourceParserSelfDeclarationOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    @SourceDeclarationParserArguments --max-steps 45000000 -- $SourceDeclarationParserSource
if (
    $LASTEXITCODE -ne 0 -or
    $SourceParserSelfDeclarationOutput -notcontains 'source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=24 tokens=8876 offset=64950' -or
    $SourceParserSelfDeclarationOutput -notcontains 'Result: 0'
) {
    throw 'The declaration-parser tool did not parse its own declaration source.'
}

dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Foundation/Wv-Dump-Core.wv') -o $WvDumpCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Wv-Dump-Core.wv.' }

$WvDumpCoreVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $WvDumpCoreModule
if ($LASTEXITCODE -ne 0 -or $WvDumpCoreVerifyOutput -notcontains 'Verified: Wvˉdumpˉcore') {
    throw 'The Seed CLI failed to verify Wv-Dump-Core.wvb.'
}

$WvDumpCoreInspectOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $WvDumpCoreModule
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

$WvDumpUnauthorizedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvDumpUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WvDump hosted capabilities.'
}

$WvDumpCoreRunOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule @WvDumpCapabilities
if ($LASTEXITCODE -ne 0 -or $WvDumpCoreRunOutput -notcontains 'Result: 0') {
    throw 'The Seed CLI did not produce Result: 0 for Wv-Dump-Core.wvb.'
}

$WvDumpHostedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule @WvDumpCapabilities -- $SumModule
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

$WvDumpInvalidOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule @WvDumpCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') 2>&1
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
$WvDumpMissingOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule @WvDumpCapabilities -- $MissingHostedFile 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvDumpMissingOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The hosted file adapter did not report a missing resource deterministically.'
}

$WvDumpInvalidNameOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule @WvDumpCapabilities -- '' 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvDumpInvalidNameOutput -join "`n") -notmatch 'WVR3021') {
    throw 'The hosted file adapter did not reject an empty resource name deterministically.'
}

dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Foundation/Wvo-Object-Core.wv') `
    --module $ByteOrderingSource `
    -o $WvoCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Wvo-Object-Core.wv.' }

$WvoCoreVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $WvoCoreModule
if ($LASTEXITCODE -ne 0 -or $WvoCoreVerifyOutput -notcontains 'Verified: Wvoˉobjectˉcore') {
    throw 'The Seed CLI failed to verify Wvo-Object-Core.wvb.'
}

$WvoCoreInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $WvoCoreModule) -join "`n"
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

$WvoUnauthorizedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvoCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WVO writer capabilities.'
}

$WvoSelfTestOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvoCoreModule @WvoCapabilities
if ($LASTEXITCODE -ne 0 -or $WvoSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale object core self-test did not return Result: 0.'
}

$WvoHostedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvoCoreModule @WvoCapabilities -- $WvoSample
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

$WvoVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- object-verify $WvoSample
if ($LASTEXITCODE -ne 0 -or $WvoVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The object verifier rejected the Windvale-written sample.'
}

$WvoInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- object-inspect $WvoSample) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $WvoInspection -notmatch 'Sections \(2\)' -or
    $WvoInspection -notmatch 'Console_write binding=Import' -or
    $WvoInspection -notmatch 'kind=Relativeˉi32 section=0 offset=1 symbol=2 addend=-4'
) {
    throw 'The object inspector did not expose the expected symbol and relocation records.'
}

$WvoInvalidNameOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvoCoreModule @WvoCapabilities -- '' 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoInvalidNameOutput -join "`n") -notmatch 'WVR3021') {
    throw 'The hosted file writer did not reject an empty resource name deterministically.'
}

$MissingWriterParent = Join-Path $Artifacts '__windvale_missing_writer_parent__'
if (Test-Path -LiteralPath $MissingWriterParent) {
    throw "The missing writer parent unexpectedly exists: $MissingWriterParent"
}
$WvoMissingParentOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvoCoreModule @WvoCapabilities -- (Join-Path $MissingWriterParent 'Sample.wvo') 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvoMissingParentOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The hosted file writer did not report a missing parent deterministically.'
}

dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Assembler/Wva-Assembler-Core.wv') `
    --module $MachineContractsSource `
    --module $ByteOrderingSource `
    --module $DecimalParsingSource `
    --module $ByteConstructionSource `
    -o $WvaAssemblerModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Wva-Assembler-Core.wv.' }

$WvaAssemblerVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $WvaAssemblerModule
if ($LASTEXITCODE -ne 0 -or $WvaAssemblerVerifyOutput -notcontains 'Verified: Wvaˉassemblerˉcore') {
    throw 'The bytecode verifier rejected the Windvale WVA assembler.'
}

$WvaAssemblerInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $WvaAssemblerModule) -join "`n"
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

$WvaAssemblerUnauthorizedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvaAssemblerModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvaAssemblerUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted WVA assembler capabilities.'
}

$WvaAssemblerSelfTestOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvaAssemblerModule @WvaAssemblerCapabilities
if ($LASTEXITCODE -ne 0 -or $WvaAssemblerSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale WVA assembler self-test did not return Result: 0.'
}

dotnet run --project $ToolProject --configuration $Configuration --no-build -- `
    compile (Join-Path $RepositoryRoot 'Examples/Linker/Wv-Linker-Core.wv') `
    --module $MachineContractsSource `
    --module $ByteOrderingSource `
    --module $DecimalParsingSource `
    --module $ByteConstructionSource `
    -o $WvLinkerCoreModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile the Windvale linker core.' }

$WvLinkerVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $WvLinkerCoreModule
if ($LASTEXITCODE -ne 0 -or $WvLinkerVerifyOutput -notcontains 'Verified: Wvˉlinkerˉcore') {
    throw 'The bytecode verifier rejected the Windvale linker core.'
}

$WvLinkerInspectOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- inspect $WvLinkerCoreModule
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
$WvLinkerUnauthorizedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvLinkerUnauthorizedOutput -join "`n") -notmatch 'WVR3010') {
    throw 'The Seed CLI did not refuse ungranted Windvale linker capabilities.'
}

$WvLinkerSelfTestOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities
if ($LASTEXITCODE -ne 0 -or $WvLinkerSelfTestOutput -notcontains 'Result: 0') {
    throw 'The Windvale linker scanner self-test did not return Result: 0.'
}

$WvaAssemblerHostedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') $WindvaleAssemblyObject
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
$WindvaleAssemblyVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- object-verify $WindvaleAssemblyObject
if ($LASTEXITCODE -ne 0 -or $WindvaleAssemblyVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The independent object verifier rejected the Windvale-written assembler output.'
}

$WvLinkerHostedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities -- $WindvaleAssemblyObject
if (
    $LASTEXITCODE -ne 0 -or
    $WvLinkerHostedOutput -notcontains 'object status=Valid sections=2 symbols=3 relocations=2 offset=218' -or
    $WvLinkerHostedOutput -notcontains 'Result: 0'
) {
    throw 'The Windvale linker scanner did not accept the canonical assembler object.'
}

$WvLinkerInvalidOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') 2>&1
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
$WvaAssemblerMissingParentOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') $MissingAssemblerOutput 2>&1
if ($LASTEXITCODE -ne 3 -or ($WvaAssemblerMissingParentOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The Windvale WVA assembler did not report a missing output parent deterministically.'
}
if (Test-Path -LiteralPath $MissingAssemblerOutput) {
    throw 'The failed Windvale assembler host write left a partial output object.'
}

if (Test-Path -LiteralPath $InvalidWindvaleAssemblyObject) {
    throw "The invalid Windvale assembly output unexpectedly exists: $InvalidWindvaleAssemblyObject"
}
$WvaSemanticInvalidOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') $InvalidWindvaleAssemblyObject 2>&1
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
$WvaSemanticExistingOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvaAssemblerModule @WvaAssemblerCapabilities -- (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') $WindvaleAssemblyObject 2>&1
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

$AssemblyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- assemble (Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva') -o $AssemblyObject
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

$AssemblyVerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- object-verify $AssemblyObject
if ($LASTEXITCODE -ne 0 -or $AssemblyVerifyOutput -notcontains 'Verified object: X86ˉ64') {
    throw 'The object verifier rejected the WVA example object.'
}

$AssemblyInspection = (dotnet run --project $ToolProject --configuration $Configuration --no-build -- object-inspect $AssemblyObject) -join "`n"
if (
    $LASTEXITCODE -ne 0 -or
    $AssemblyInspection -notmatch '\.text kind=Code align=16 memory=11 data=11' -or
    $AssemblyInspection -notmatch 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' -or
    $AssemblyInspection -notmatch 'kind=Absoluteˉu32 section=1 offset=3 symbol=1 addend=0'
) {
    throw 'The object inspector did not expose the expected WVA sections and relocations.'
}

$ProviderAssemblyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- assemble (Join-Path $RepositoryRoot 'Examples/Linker/Console-Provider.wva') -o $LinkProviderObject
if (
    $LASTEXITCODE -ne 0 -or
    $ProviderAssemblyOutput -notcontains 'SHA-256: 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab'
) {
    throw 'The Stage 0 assembler did not produce the canonical linker provider object.'
}

$WvLinkerMapOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $WindvaleLinkedImage $WindvaleAssemblyObject $LinkProviderObject
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
$WvLinkerUndefinedOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $InvalidWindvaleLinkedImage $WindvaleAssemblyObject 2>&1
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

$WvLinkerExistingFailure = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $WindvaleLinkedImage $WindvaleAssemblyObject 2>&1
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
$MissingWindvaleLinkParentOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvLinkerCoreModule @WvLinkerCapabilities -- 1048576 Main $MissingWindvaleLinkOutput $WindvaleAssemblyObject $LinkProviderObject 2>&1
if ($LASTEXITCODE -ne 3 -or ($MissingWindvaleLinkParentOutput -join "`n") -notmatch 'WVR3022') {
    throw 'The Windvale linker did not report a missing output parent deterministically.'
}
if (Test-Path -LiteralPath $MissingWindvaleLinkOutput) {
    throw 'The failed Windvale linker write left a partial image.'
}

$LinkMapOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- link --base-address 1048576 --entry Main -o $LinkedImage $AssemblyObject $LinkProviderObject
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
$UndefinedLinkOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- link --base-address 1048576 --entry Main -o $InvalidLinkedImage $AssemblyObject 2>&1
if ($LASTEXITCODE -ne 1 -or ($UndefinedLinkOutput -join "`n") -notmatch 'WVL1005') {
    throw 'The Stage 0 linker did not reject an undefined import deterministically.'
}
if (Test-Path -LiteralPath $InvalidLinkedImage) {
    throw 'A rejected link created a partial image.'
}

$ExistingLinkFailure = dotnet run --project $ToolProject --configuration $Configuration --no-build -- link --base-address 1048576 --entry Main -o $LinkedImage $AssemblyObject 2>&1
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
$MissingLinkParentOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- link --base-address 1048576 --entry Main -o $MissingLinkOutput $AssemblyObject $LinkProviderObject 2>&1
if ($LASTEXITCODE -ne 74 -or ($MissingLinkParentOutput -join "`n") -notmatch 'I/O failed') {
    throw 'The Stage 0 linker did not report a missing output parent deterministically.'
}
if (Test-Path -LiteralPath $MissingLinkOutput) {
    throw 'The failed linker write left a partial image.'
}

Write-Output "Windvale Seed verification passed."
Write-Output "Conformance report: $ReportPath"
