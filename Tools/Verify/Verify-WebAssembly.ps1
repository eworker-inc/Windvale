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
$StraightSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Straight-I32-Main.wv'
$SubtractOverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Checked-Subtract-Overflow-Main.wv'
$MultiplyOverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Checked-Multiply-Overflow-Main.wv'
$NegateOverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Checked-Negate-Overflow-Main.wv'
$MeteredLoopSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Metered-Loop-Main.wv'
$NonterminatingLoopSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Nonterminating-Loop-Main.wv'
$StructuredControlSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Structured-Control-Main.wv'
$StructuredControlElseSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Structured-Control-Else-Main.wv'
$SequentialIfSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Sequential-If-Main.wv'
$EngineVerifier = Join-Path $RepositoryRoot 'Tools/Verify/Verify-WebAssembly-Engine.mjs'
$BackendWvb = Join-Path $ArtifactDirectory 'Windvale-WebAssembly.wvb'
$SuccessWvb = Join-Path $ArtifactDirectory 'Checked-Add-Main.wvb'
$OverflowWvb = Join-Path $ArtifactDirectory 'Checked-Add-Overflow-Main.wvb'
$StraightWvb = Join-Path $ArtifactDirectory 'Straight-I32-Main.wvb'
$SubtractOverflowWvb = Join-Path $ArtifactDirectory 'Checked-Subtract-Overflow-Main.wvb'
$MultiplyOverflowWvb = Join-Path $ArtifactDirectory 'Checked-Multiply-Overflow-Main.wvb'
$NegateOverflowWvb = Join-Path $ArtifactDirectory 'Checked-Negate-Overflow-Main.wvb'
$MeteredLoopWvb = Join-Path $ArtifactDirectory 'Metered-Loop-Main.wvb'
$NonterminatingLoopWvb = Join-Path $ArtifactDirectory 'Nonterminating-Loop-Main.wvb'
$StructuredControlWvb = Join-Path $ArtifactDirectory 'Structured-Control-Main.wvb'
$StructuredControlElseWvb = Join-Path $ArtifactDirectory 'Structured-Control-Else-Main.wvb'
$SequentialIfWvb = Join-Path $ArtifactDirectory 'Sequential-If-Main.wvb'
$SuccessWasm = Join-Path $ArtifactDirectory 'Checked-Add-Main.wasm'
$OverflowWasm = Join-Path $ArtifactDirectory 'Checked-Add-Overflow-Main.wasm'
$StraightWasm = Join-Path $ArtifactDirectory 'Straight-I32-Main.wasm'
$SubtractOverflowWasm = Join-Path $ArtifactDirectory 'Checked-Subtract-Overflow-Main.wasm'
$MultiplyOverflowWasm = Join-Path $ArtifactDirectory 'Checked-Multiply-Overflow-Main.wasm'
$NegateOverflowWasm = Join-Path $ArtifactDirectory 'Checked-Negate-Overflow-Main.wasm'
$MeteredLoopWasm = Join-Path $ArtifactDirectory 'Metered-Loop-Main.wasm'
$NonterminatingLoopWasm = Join-Path $ArtifactDirectory 'Nonterminating-Loop-Main.wasm'
$StructuredControlWasm = Join-Path $ArtifactDirectory 'Structured-Control-Main.wasm'
$StructuredControlElseWasm = Join-Path $ArtifactDirectory 'Structured-Control-Else-Main.wasm'
$SequentialIfWasm = Join-Path $ArtifactDirectory 'Sequential-If-Main.wasm'

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
Invoke-Windvale @('compile', $StraightSource, '-o', $StraightWvb)
Invoke-Windvale @('compile', $SubtractOverflowSource, '-o', $SubtractOverflowWvb)
Invoke-Windvale @('compile', $MultiplyOverflowSource, '-o', $MultiplyOverflowWvb)
Invoke-Windvale @('compile', $NegateOverflowSource, '-o', $NegateOverflowWvb)
Invoke-Windvale @('compile', $MeteredLoopSource, '-o', $MeteredLoopWvb)
Invoke-Windvale @('compile', $NonterminatingLoopSource, '-o', $NonterminatingLoopWvb)
Invoke-Windvale @('compile', $StructuredControlSource, '-o', $StructuredControlWvb)
Invoke-Windvale @('compile', $StructuredControlElseSource, '-o', $StructuredControlElseWvb)
Invoke-Windvale @('compile', $SequentialIfSource, '-o', $SequentialIfWvb)

$RunArguments = @(
    'run', $BackendWvb,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '100000000',
    '--'
)
Invoke-Windvale ($RunArguments + @($SuccessWvb, $SuccessWasm))
Invoke-Windvale ($RunArguments + @($OverflowWvb, $OverflowWasm))
Invoke-Windvale ($RunArguments + @($StraightWvb, $StraightWasm))
Invoke-Windvale ($RunArguments + @($SubtractOverflowWvb, $SubtractOverflowWasm))
Invoke-Windvale ($RunArguments + @($MultiplyOverflowWvb, $MultiplyOverflowWasm))
Invoke-Windvale ($RunArguments + @($NegateOverflowWvb, $NegateOverflowWasm))
Invoke-Windvale ($RunArguments + @($MeteredLoopWvb, $MeteredLoopWasm))
Invoke-Windvale ($RunArguments + @($NonterminatingLoopWvb, $NonterminatingLoopWasm))
Invoke-Windvale ($RunArguments + @($StructuredControlWvb, $StructuredControlWasm))
Invoke-Windvale ($RunArguments + @($StructuredControlElseWvb, $StructuredControlElseWasm))
Invoke-Windvale ($RunArguments + @($SequentialIfWvb, $SequentialIfWasm))

node $EngineVerifier `
    $SuccessWasm `
    $OverflowWasm `
    $StraightWasm `
    $SubtractOverflowWasm `
    $MultiplyOverflowWasm `
    $NegateOverflowWasm `
    $MeteredLoopWasm `
    $NonterminatingLoopWasm `
    $StructuredControlWasm `
    $StructuredControlElseWasm `
    $SequentialIfWasm
if ($LASTEXITCODE -ne 0) { throw 'The WebAssembly engine verification failed.' }

Write-Output 'Windvale-authored WebAssembly verification passed.'
