param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ArtifactDirectory = Join-Path $RepositoryRoot 'artifacts/webassembly-verification'
$ToolProject = Join-Path $RepositoryRoot 'Tools/Windvale.Tool/Windvale.Tool.csproj'
$ToolDll = Join-Path $RepositoryRoot "Tools/Windvale.Tool/bin/$Configuration/net10.0/windvale.dll"
$BackendProject = Join-Path $RepositoryRoot 'Windvale-WebAssembly.wvproj'
$SuccessSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Checked-Add-Main.wv'
$OverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Checked-Add-Overflow-Main.wv'
$EngineVerifier = Join-Path $RepositoryRoot 'Tools/Verify/Verify-WebAssembly-Engine.mjs'
$BackendWvb = Join-Path $ArtifactDirectory 'Windvale-WebAssembly.wvb'
$SuccessWvb = Join-Path $ArtifactDirectory 'Checked-Add-Main.wvb'
$OverflowWvb = Join-Path $ArtifactDirectory 'Checked-Add-Overflow-Main.wvb'
$SuccessWasm = Join-Path $ArtifactDirectory 'Checked-Add-Main.wasm'
$OverflowWasm = Join-Path $ArtifactDirectory 'Checked-Add-Overflow-Main.wasm'

New-Item -ItemType Directory -Path $ArtifactDirectory -Force | Out-Null

dotnet build $ToolProject -c $Configuration
if ($LASTEXITCODE -ne 0) { throw 'The Windvale tool build failed.' }

function Invoke-Windvale([string[]]$ToolArguments) {
    dotnet $ToolDll @ToolArguments
    if ($LASTEXITCODE -ne 0) {
        throw "The Windvale tool failed: $($ToolArguments -join ' ')"
    }
}

Invoke-Windvale @('build', $BackendProject, '-o', $BackendWvb)
Invoke-Windvale @('compile', $SuccessSource, '-o', $SuccessWvb)
Invoke-Windvale @('compile', $OverflowSource, '-o', $OverflowWvb)

$RunArguments = @(
    'run', $BackendWvb,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--'
)
Invoke-Windvale ($RunArguments + @($SuccessWvb, $SuccessWasm))
Invoke-Windvale ($RunArguments + @($OverflowWvb, $OverflowWasm))

node $EngineVerifier $SuccessWasm $OverflowWasm
if ($LASTEXITCODE -ne 0) { throw 'The WebAssembly engine verification failed.' }

Write-Output 'Windvale-authored WebAssembly verification passed.'
