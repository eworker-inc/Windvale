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
$BoundedCallsSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Bounded-Calls-Main.wv'
$BoundedCallsOverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Bounded-Calls-Overflow-Main.wv'
$CallsWithControlSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Calls-With-Control-Main.wv'
$CallsWithControlElseSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Calls-With-Control-Else-Main.wv'
$MemoryBytesSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Memory-Bytes-Main.wv'
$MemoryTextSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Memory-Text-Main.wv'
$RuntimeValuesSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-Values-Main.wv'
$RuntimeConcatSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-Concat-Main.wv'
$RuntimeU16GuardSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-U16-Guard-Main.wv'
$RuntimeArenaSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-Arena-Main.wv'
$RuntimeU32GuardSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-U32-Guard-Main.wv'
$RuntimeCallsSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-Calls-Main.wv'
$WvbEnvelopeVerifySource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Envelope-Verify-Main.wv'
$WvbStructuralVerifySource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Structural-Verify-Main.wv'
$WvbSemanticVerifySource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Semantic-Verify-Main.wv'
$WvbSemanticExpandedSource = Join-Path $ArtifactDirectory 'Wvb-Semantic-Expanded-Main.wv'
$StructuralDataSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Data-And-Text.wv'
$StructuralTypesSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Nominal-Types.wv'
$StructuralCapabilitiesSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv'
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
$BoundedCallsWvb = Join-Path $ArtifactDirectory 'Bounded-Calls-Main.wvb'
$BoundedCallsOverflowWvb = Join-Path $ArtifactDirectory 'Bounded-Calls-Overflow-Main.wvb'
$CallsWithControlWvb = Join-Path $ArtifactDirectory 'Calls-With-Control-Main.wvb'
$CallsWithControlElseWvb = Join-Path $ArtifactDirectory 'Calls-With-Control-Else-Main.wvb'
$MemoryBytesWvb = Join-Path $ArtifactDirectory 'Memory-Bytes-Main.wvb'
$MemoryTextWvb = Join-Path $ArtifactDirectory 'Memory-Text-Main.wvb'
$RuntimeValuesWvb = Join-Path $ArtifactDirectory 'Runtime-Values-Main.wvb'
$RuntimeConcatWvb = Join-Path $ArtifactDirectory 'Runtime-Concat-Main.wvb'
$RuntimeU16GuardWvb = Join-Path $ArtifactDirectory 'Runtime-U16-Guard-Main.wvb'
$RuntimeArenaWvb = Join-Path $ArtifactDirectory 'Runtime-Arena-Main.wvb'
$RuntimeU32GuardWvb = Join-Path $ArtifactDirectory 'Runtime-U32-Guard-Main.wvb'
$RuntimeCallsWvb = Join-Path $ArtifactDirectory 'Runtime-Calls-Main.wvb'
$WvbEnvelopeVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Envelope-Verify-Main.wvb'
$WvbStructuralVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Structural-Verify-Main.wvb'
$WvbSemanticVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Semantic-Verify-Main.wvb'
$WvbSemanticExpandedWvb = Join-Path $ArtifactDirectory 'Wvb-Semantic-Expanded-Main.wvb'
$StructuralDataWvb = Join-Path $ArtifactDirectory 'Structural-Data-And-Text.wvb'
$StructuralTypesWvb = Join-Path $ArtifactDirectory 'Structural-Nominal-Types.wvb'
$StructuralCapabilitiesWvb = Join-Path $ArtifactDirectory 'Structural-Hosted-Capabilities.wvb'
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
$BoundedCallsWasm = Join-Path $ArtifactDirectory 'Bounded-Calls-Main.wasm'
$BoundedCallsOverflowWasm = Join-Path $ArtifactDirectory 'Bounded-Calls-Overflow-Main.wasm'
$CallsWithControlWasm = Join-Path $ArtifactDirectory 'Calls-With-Control-Main.wasm'
$CallsWithControlElseWasm = Join-Path $ArtifactDirectory 'Calls-With-Control-Else-Main.wasm'
$MemoryBytesWasm = Join-Path $ArtifactDirectory 'Memory-Bytes-Main.wasm'
$MemoryTextWasm = Join-Path $ArtifactDirectory 'Memory-Text-Main.wasm'
$RuntimeValuesWasm = Join-Path $ArtifactDirectory 'Runtime-Values-Main.wasm'
$RuntimeConcatWasm = Join-Path $ArtifactDirectory 'Runtime-Concat-Main.wasm'
$RuntimeU16GuardWasm = Join-Path $ArtifactDirectory 'Runtime-U16-Guard-Main.wasm'
$RuntimeArenaWasm = Join-Path $ArtifactDirectory 'Runtime-Arena-Main.wasm'
$RuntimeU32GuardWasm = Join-Path $ArtifactDirectory 'Runtime-U32-Guard-Main.wasm'
$RuntimeCallsWasm = Join-Path $ArtifactDirectory 'Runtime-Calls-Main.wasm'
$WvbEnvelopeVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Envelope-Verify-Main.wasm'
$WvbStructuralVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Structural-Verify-Main.wasm'
$WvbSemanticVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Semantic-Verify-Main.wasm'
$WvbSemanticExpandedWasm = Join-Path $ArtifactDirectory 'Wvb-Semantic-Expanded-Main.wasm'

New-Item -ItemType Directory -Path $ArtifactDirectory -Force | Out-Null

$WvbSemanticExpandedText = [IO.File]::ReadAllText($WvbSemanticVerifySource)
$WvbSemanticExpandedText = $WvbSemanticExpandedText.Replace(
    'export fn Main(Input: bytes) -> bytes {',
    "fn Hˉsemantic(Input: bytes) -> bytes {`n" +
        "    return Gˉtypes(Input);`n" +
        "}`n`n" +
        'export fn Main(Input: bytes) -> bytes {')
$WvbSemanticExpandedText = $WvbSemanticExpandedText.Replace(
    'State = Gˉtypes(State);',
    'State = Hˉsemantic(State);')
[IO.File]::WriteAllText(
    $WvbSemanticExpandedSource,
    $WvbSemanticExpandedText,
    [Text.UTF8Encoding]::new($false))

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
Invoke-Windvale @('compile', $BoundedCallsSource, '-o', $BoundedCallsWvb)
Invoke-Windvale @('compile', $BoundedCallsOverflowSource, '-o', $BoundedCallsOverflowWvb)
Invoke-Windvale @('compile', $CallsWithControlSource, '-o', $CallsWithControlWvb)
Invoke-Windvale @('compile', $CallsWithControlElseSource, '-o', $CallsWithControlElseWvb)
Invoke-Windvale @('compile', $MemoryBytesSource, '-o', $MemoryBytesWvb)
Invoke-Windvale @('compile', $MemoryTextSource, '-o', $MemoryTextWvb)
Invoke-Windvale @('compile', $RuntimeValuesSource, '-o', $RuntimeValuesWvb)
Invoke-Windvale @('compile', $RuntimeConcatSource, '-o', $RuntimeConcatWvb)
Invoke-Windvale @('compile', $RuntimeU16GuardSource, '-o', $RuntimeU16GuardWvb)
Invoke-Windvale @('compile', $RuntimeArenaSource, '-o', $RuntimeArenaWvb)
Invoke-Windvale @('compile', $RuntimeU32GuardSource, '-o', $RuntimeU32GuardWvb)
Invoke-Windvale @('compile', $RuntimeCallsSource, '-o', $RuntimeCallsWvb)
Invoke-Windvale @('compile', $WvbEnvelopeVerifySource, '-o', $WvbEnvelopeVerifyWvb)
Invoke-Windvale @('compile', $WvbStructuralVerifySource, '-o', $WvbStructuralVerifyWvb)
Invoke-Windvale @('compile', $WvbSemanticVerifySource, '-o', $WvbSemanticVerifyWvb)
Invoke-Windvale @('compile', $WvbSemanticExpandedSource, '-o', $WvbSemanticExpandedWvb)
Invoke-Windvale @('compile', $StructuralDataSource, '-o', $StructuralDataWvb)
Invoke-Windvale @('compile', $StructuralTypesSource, '-o', $StructuralTypesWvb)
Invoke-Windvale @('compile', $StructuralCapabilitiesSource, '-o', $StructuralCapabilitiesWvb)

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
Invoke-Windvale ($RunArguments + @($BoundedCallsWvb, $BoundedCallsWasm))
Invoke-Windvale ($RunArguments + @($BoundedCallsOverflowWvb, $BoundedCallsOverflowWasm))
Invoke-Windvale ($RunArguments + @($CallsWithControlWvb, $CallsWithControlWasm))
Invoke-Windvale ($RunArguments + @($CallsWithControlElseWvb, $CallsWithControlElseWasm))
Invoke-Windvale ($RunArguments + @($MemoryBytesWvb, $MemoryBytesWasm))
Invoke-Windvale ($RunArguments + @($MemoryTextWvb, $MemoryTextWasm))
Invoke-Windvale ($RunArguments + @($RuntimeValuesWvb, $RuntimeValuesWasm))
Invoke-Windvale ($RunArguments + @($RuntimeConcatWvb, $RuntimeConcatWasm))
Invoke-Windvale ($RunArguments + @($RuntimeU16GuardWvb, $RuntimeU16GuardWasm))
Invoke-Windvale ($RunArguments + @($RuntimeArenaWvb, $RuntimeArenaWasm))
Invoke-Windvale ($RunArguments + @($RuntimeU32GuardWvb, $RuntimeU32GuardWasm))
Invoke-Windvale ($RunArguments + @($RuntimeCallsWvb, $RuntimeCallsWasm))
Invoke-Windvale ($RunArguments + @($WvbEnvelopeVerifyWvb, $WvbEnvelopeVerifyWasm))
Invoke-Windvale ($RunArguments + @($WvbStructuralVerifyWvb, $WvbStructuralVerifyWasm))
$SemanticRunArguments = @(
    'run', $BackendWvb,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '150000000',
    '--'
)
Invoke-Windvale ($SemanticRunArguments + @($WvbSemanticVerifyWvb, $WvbSemanticVerifyWasm))
Invoke-Windvale ($SemanticRunArguments + @($WvbSemanticExpandedWvb, $WvbSemanticExpandedWasm))

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
    $SequentialIfWasm `
    $BoundedCallsWasm `
    $BoundedCallsOverflowWasm `
    $CallsWithControlWasm `
    $CallsWithControlElseWasm `
    $MemoryBytesWasm `
    $MemoryTextWasm `
    $RuntimeValuesWasm `
    $RuntimeConcatWasm `
    $RuntimeU16GuardWasm `
    $RuntimeArenaWasm `
    $RuntimeU32GuardWasm `
    $WvbEnvelopeVerifyWasm `
    $WvbEnvelopeVerifyWvb `
    $WvbStructuralVerifyWasm `
    $WvbStructuralVerifyWvb `
    $StructuralDataWvb `
    $StructuralTypesWvb `
    $StructuralCapabilitiesWvb `
    $RuntimeCallsWasm `
    $WvbSemanticVerifyWasm `
    $StructuralDataWvb `
    $StructuralTypesWvb `
    $StructuralCapabilitiesWvb `
    $WvbSemanticExpandedWasm `
    $StructuralDataWvb `
    $StructuralTypesWvb `
    $StructuralCapabilitiesWvb
if ($LASTEXITCODE -ne 0) { throw 'The WebAssembly engine verification failed.' }

Write-Output 'Windvale-authored WebAssembly verification passed.'
