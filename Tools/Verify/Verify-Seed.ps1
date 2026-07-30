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
    $WvDumpCoreInspectOutput -notcontains 'Record types (2)' -or
    $WvDumpCoreInspection -notmatch 'Inspectˉwvbˉenvelope' -or
    $WvDumpCoreInspection -notmatch 'record\.create' -or
    $WvDumpCoreInspection -notmatch 'record\.field'
) {
    throw 'The Seed CLI inspector did not expose the structured Windvale section walker.'
}

$WvDumpCoreRunOutput = dotnet run --project $ToolProject --configuration $Configuration --no-build -- run $WvDumpCoreModule
if ($LASTEXITCODE -ne 0 -or $WvDumpCoreRunOutput -notcontains 'Result: 0') {
    throw 'The Seed CLI did not produce Result: 0 for Wv-Dump-Core.wvb.'
}

Write-Output "Windvale Seed verification passed."
Write-Output "Conformance report: $ReportPath"
