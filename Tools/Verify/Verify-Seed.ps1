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
$WvDumpCoreModule = Join-Path $Artifacts 'Wv-Dump-Core.wvb'
$WvoCoreModule = Join-Path $Artifacts 'Wvo-Object-Core.wvb'
$WvaAssemblerModule = Join-Path $Artifacts 'Wva-Assembler-Core.wvb'
$WvLinkerCoreModule = Join-Path $Artifacts 'Wv-Linker-Core.wvb'
$WvoSample = Join-Path $Artifacts 'Sample.wvo'
$AssemblyObject = Join-Path $Artifacts 'Hello-Object.wvo'
$WindvaleAssemblyObject = Join-Path $Artifacts 'Hello-Object-Windvale.wvo'
$InvalidWindvaleAssemblyObject = Join-Path $Artifacts '__windvale_invalid_assembly_output__.wvo'
$LinkProviderObject = Join-Path $Artifacts 'Console-Provider.wvo'
$LinkedImage = Join-Path $Artifacts 'Hello-Linked.bin'
$LinkMap = Join-Path $Artifacts 'Hello-Linked.wvmap'
$InvalidLinkedImage = Join-Path $Artifacts '__windvale_invalid_link_output__.bin'
dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') -o $SumModule
if ($LASTEXITCODE -ne 0) { throw 'The Seed CLI failed to compile Sum-Data.wv.' }

$VerifyOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- verify $SumModule
if ($LASTEXITCODE -ne 0 -or $VerifyOutput -notcontains 'Verified: Sumˉdata') {
    throw 'The Seed CLI failed to verify Sum-Data.wvb.'
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

dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Foundation/Wvo-Object-Core.wv') -o $WvoCoreModule
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

dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Assembler/Wva-Assembler-Core.wv') -o $WvaAssemblerModule
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

dotnet run --project $ToolProject --configuration $Configuration --no-build -- compile (Join-Path $RepositoryRoot 'Examples/Linker/Wv-Linker-Core.wv') -o $WvLinkerCoreModule
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
    $WvLinkerInspection -notmatch 'bytes\.read_i32_little' -or
    $WvLinkerInspection -notmatch 'file\.read_bytes'
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
