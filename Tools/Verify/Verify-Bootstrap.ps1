[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [ValidateRange(1, 60000000000)]
    [long]$MaximumInstructions = 48000000000
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$ToolProject = Join-Path $RepositoryRoot 'Tools/Windvale.Tool/Windvale.Tool.csproj'
$ToolDll = Join-Path $RepositoryRoot "Tools/Windvale.Tool/bin/$Configuration/net10.0/windvale.dll"
$Artifacts = Join-Path $RepositoryRoot 'artifacts'
$ProjectManifest = Join-Path $RepositoryRoot 'Windvale-Compiler.wvproj'
$RootSource = Join-Path $RepositoryRoot 'Examples/Compiler/Source-Wvb-Tool.wv'
$Dependencies = @(
    'Compiler/Windvale/Source-Bindings-Core.wv',
    'Compiler/Windvale/Source-Body-Parser.wv',
    'Compiler/Windvale/Source-Declaration-Parser.wv',
    'Compiler/Windvale/Source-Graph-Core.wv',
    'Compiler/Windvale/Source-Lexer-Core.wv',
    'Compiler/Windvale/Source-Set-Core.wv',
    'Compiler/Windvale/Source-Symbols-Core.wv',
    'Compiler/Windvale/Source-Wir-Core.wv',
    'Compiler/Windvale/Source-Wvb-Core.wv',
    'Foundation/Byte-Construction.wv',
    'Foundation/Decimal-Parsing.wv'
) | ForEach-Object { Join-Path $RepositoryRoot $_ }
$Stage1 = Join-Path $Artifacts 'Bootstrap-Stage1-Source-Wvb-Tool.wvb'
$Stage2 = Join-Path $Artifacts 'Bootstrap-Stage2-Source-Wvb-Tool.wvb'

New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null
dotnet build $ToolProject --configuration $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    throw "The Stage 0 tool build failed with exit code $LASTEXITCODE."
}

dotnet $ToolDll build $ProjectManifest -o $Stage1
if ($LASTEXITCODE -ne 0) {
    throw 'Stage 0 failed to produce the Stage 1 compiler from Windvale-Compiler.wvproj.'
}

$CapabilityArguments = @(
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count'
)
$RunOutput = @(
    & dotnet $ToolDll run $Stage1 @CapabilityArguments `
        --max-steps $MaximumInstructions --report-steps -- `
        $RootSource @Dependencies $Stage2 2>&1
)
$RunOutput | ForEach-Object { Write-Host $_ }
if (
    $LASTEXITCODE -ne 0 -or
    $RunOutput -notcontains 'Result: 0' -or
    !($RunOutput -match '^source wvb status=Valid ')
) {
    throw 'Stage 1 failed to produce a valid Stage 2 compiler.'
}

dotnet $ToolDll verify $Stage2
if ($LASTEXITCODE -ne 0) {
    throw 'Independent verification rejected the Stage 2 compiler.'
}

$Stage1Bytes = [IO.File]::ReadAllBytes($Stage1)
$Stage2Bytes = [IO.File]::ReadAllBytes($Stage2)
if (![Linq.Enumerable]::SequenceEqual($Stage1Bytes, $Stage2Bytes)) {
    throw 'Stage 1 and Stage 2 are not byte-for-byte identical.'
}

$Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $Stage2).Hash.ToLowerInvariant()
Write-Host 'Windvale bootstrap convergence passed.'
Write-Host "Compiler bytes: $($Stage2Bytes.Length)"
Write-Host "Compiler SHA-256: $Digest"
Write-Host "Stage 1: $Stage1"
Write-Host "Stage 2: $Stage2"
