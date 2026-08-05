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
$CompilerProject = Join-Path $RepositoryRoot 'Windvale-Compiler.wvproj'
$CompilerMemoryProject = Join-Path $RepositoryRoot 'Windvale-Compiler-Memory.wvproj'
$WvbScalarInterpreterProject = Join-Path $RepositoryRoot 'Windvale-Wvb-Scalar-Interpreter.wvproj'
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
$RuntimeReclaimSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-Reclaim-Main.wv'
$RuntimeU32GuardSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-U32-Guard-Main.wv'
$RuntimeCallsSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Runtime-Calls-Main.wv'
$WvbEnvelopeVerifySource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Envelope-Verify-Main.wv'
$WvbStructuralVerifySource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Structural-Verify-Main.wv'
$WvbSemanticVerifySource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Semantic-Verify-Main.wv'
$WvbExecutableVerifyPhaseSource = Join-Path $RepositoryRoot 'Tests/Fixtures/WebAssembly/Wvb-Executable-Verify-Phase.wv'
$WvbSemanticExpandedSource = Join-Path $ArtifactDirectory 'Wvb-Semantic-Expanded-Main.wv'
$WvbExecutableVerifySource = Join-Path $ArtifactDirectory 'Wvb-Executable-Verify-Main.wv'
$WvbCompilerExecutablePhaseSource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Executable-Verify-Phase.wv'
$WvbCompilerSemanticVerifySource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Semantic-Verify-Main.wv'
$WvbCompilerTypedVerifySource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Verify-Main.wv'
$WvbCompilerTypedSecondVerifySource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Second-Verify-Main.wv'
$WvbCompilerTypedThirdVerifySource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Third-Verify-Main.wv'
$WvbCompilerControlVerifySource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Control-Verify-Main.wv'
$WvbCompilerControlSecondVerifySource = Join-Path $ArtifactDirectory 'Wvb-Compiler-Control-Second-Verify-Main.wv'
$ScalarFunctionOnlySource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Function-Only.wv'
$BytesEntrySource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Bytes-Entry-Guest.wv'
$ScalarGuestSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Scalar-Interpreter-Guest.wv'
$ScalarI32OverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Scalar-Interpreter-I32-Overflow.wv'
$ScalarU32OverflowSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Scalar-Interpreter-U32-Overflow.wv'
$TextBytesGuestSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-Guest.wv'
$TextBytesUtf8Source = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-Utf8-Boundaries.wv'
$TextBytesInvalidUtf8Source = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-Invalid-Utf8.wv'
$TextBytesRangeSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-Range-Failure.wv'
$TextBytesU16Source = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-U16-Failure.wv'
$TextBytesValueSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-Value-Failure.wv'
$TextBytesHeapSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Text-Bytes-Interpreter-Heap-Failure.wv'
$FormattingQuoteSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Formatting-Quote-Interpreter-Guest.wv'
$Sha256Source = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Sha256-Interpreter-Guest.wv'
$NominalDefaultsSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Nominal-Defaults-Interpreter-Guest.wv'
$RecordArenaSource = Join-Path $RepositoryRoot 'Tests/Fixtures/Source-Wvb/Record-Arena-Interpreter-Failure.wv'
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
$RuntimeReclaimWvb = Join-Path $ArtifactDirectory 'Runtime-Reclaim-Main.wvb'
$RuntimeU32GuardWvb = Join-Path $ArtifactDirectory 'Runtime-U32-Guard-Main.wvb'
$RuntimeCallsWvb = Join-Path $ArtifactDirectory 'Runtime-Calls-Main.wvb'
$WvbEnvelopeVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Envelope-Verify-Main.wvb'
$WvbStructuralVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Structural-Verify-Main.wvb'
$WvbSemanticVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Semantic-Verify-Main.wvb'
$WvbSemanticExpandedWvb = Join-Path $ArtifactDirectory 'Wvb-Semantic-Expanded-Main.wvb'
$WvbExecutableVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Executable-Verify-Main.wvb'
$WvbCompilerSemanticVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Compiler-Semantic-Verify-Main.wvb'
$WvbCompilerTypedVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Verify-Main.wvb'
$WvbCompilerTypedSecondVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Second-Verify-Main.wvb'
$WvbCompilerTypedThirdVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Third-Verify-Main.wvb'
$WvbCompilerControlVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Compiler-Control-Verify-Main.wvb'
$WvbCompilerControlSecondVerifyWvb = Join-Path $ArtifactDirectory 'Wvb-Compiler-Control-Second-Verify-Main.wvb'
$WvbScalarInterpreterWvb = Join-Path $ArtifactDirectory 'Wvb-Scalar-Interpreter-Main.wvb'
$CompilerWvb = Join-Path $ArtifactDirectory 'Windvale-Compiler.wvb'
$CompilerMemoryWvb = Join-Path $ArtifactDirectory 'Windvale-Compiler-Memory.wvb'
$ScalarFunctionOnlyWvb = Join-Path $ArtifactDirectory 'Scalar-Function-Only.wvb'
$BytesEntryWvb = Join-Path $ArtifactDirectory 'Bytes-Entry-Guest.wvb'
$ScalarGuestWvb = Join-Path $ArtifactDirectory 'Scalar-Interpreter-Guest.wvb'
$ScalarI32OverflowWvb = Join-Path $ArtifactDirectory 'Scalar-Interpreter-I32-Overflow.wvb'
$ScalarU32OverflowWvb = Join-Path $ArtifactDirectory 'Scalar-Interpreter-U32-Overflow.wvb'
$TextBytesGuestWvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-Guest.wvb'
$TextBytesUtf8Wvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-Utf8-Boundaries.wvb'
$TextBytesInvalidUtf8Wvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-Invalid-Utf8.wvb'
$TextBytesRangeWvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-Range-Failure.wvb'
$TextBytesU16Wvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-U16-Failure.wvb'
$TextBytesValueWvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-Value-Failure.wvb'
$TextBytesHeapWvb = Join-Path $ArtifactDirectory 'Text-Bytes-Interpreter-Heap-Failure.wvb'
$FormattingQuoteWvb = Join-Path $ArtifactDirectory 'Formatting-Quote-Interpreter-Guest.wvb'
$Sha256Wvb = Join-Path $ArtifactDirectory 'Sha256-Interpreter-Guest.wvb'
$NominalDefaultsWvb = Join-Path $ArtifactDirectory 'Nominal-Defaults-Interpreter-Guest.wvb'
$RecordArenaWvb = Join-Path $ArtifactDirectory 'Record-Arena-Interpreter-Failure.wvb'
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
$RuntimeReclaimWasm = Join-Path $ArtifactDirectory 'Runtime-Reclaim-Main.wasm'
$RuntimeU32GuardWasm = Join-Path $ArtifactDirectory 'Runtime-U32-Guard-Main.wasm'
$RuntimeCallsWasm = Join-Path $ArtifactDirectory 'Runtime-Calls-Main.wasm'
$WvbEnvelopeVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Envelope-Verify-Main.wasm'
$WvbStructuralVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Structural-Verify-Main.wasm'
$WvbSemanticVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Semantic-Verify-Main.wasm'
$WvbSemanticExpandedWasm = Join-Path $ArtifactDirectory 'Wvb-Semantic-Expanded-Main.wasm'
$WvbExecutableVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Executable-Verify-Main.wasm'
$WvbCompilerSemanticVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Compiler-Semantic-Verify-Main.wasm'
$WvbCompilerTypedVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Verify-Main.wasm'
$WvbCompilerTypedSecondVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Second-Verify-Main.wasm'
$WvbCompilerTypedThirdVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Compiler-Typed-Third-Verify-Main.wasm'
$WvbCompilerControlVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Compiler-Control-Verify-Main.wasm'
$WvbCompilerControlSecondVerifyWasm = Join-Path $ArtifactDirectory 'Wvb-Compiler-Control-Second-Verify-Main.wasm'
$WvbScalarInterpreterWasm = Join-Path $ArtifactDirectory 'Wvb-Scalar-Interpreter-Main.wasm'

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

$WvbExecutableVerifyPhaseText = [IO.File]::ReadAllText($WvbExecutableVerifyPhaseSource)
$WvbExecutableVerifyInlineText = $WvbExecutableVerifyPhaseText.Replace(
    "module WebAssemblyˉwvbˉexecutableˉverify profile portable;`n`n",
    '')
$WvbExecutableVerifyInlineText = $WvbExecutableVerifyInlineText.Replace(
    'export fn Hˉexecutable',
    'fn Hˉexecutable')
$WvbExecutableVerifyInlineText = $WvbExecutableVerifyInlineText.Replace(
    'export fn Iˉcontrol',
    'fn Iˉcontrol')
$WvbExecutableVerifyText = [IO.File]::ReadAllText($WvbSemanticVerifySource)
$WvbExecutableVerifyText = $WvbExecutableVerifyText.Replace(
    'module WebAssemblyˉwvbˉsemanticˉverify profile portable;',
    "module WebAssemblyˉwvbˉsemanticˉverify profile portable;`n`n" +
        $WvbExecutableVerifyInlineText)
$WvbExecutableVerifyText = $WvbExecutableVerifyText.Replace(
    "State = Gˉtypes(State);`n" +
        '    if Bytesˉlength(State) == 0u32 { return Invalid; }',
    "State = Gˉtypes(State);`n" +
        "    if Bytesˉlength(State) == 0u32 { return Invalid; }`n" +
        "    State = Hˉexecutable(State);`n" +
        "    if Bytesˉlength(State) == 0u32 { return Invalid; }`n" +
        "    State = Iˉcontrol(State);`n" +
        '    if Bytesˉlength(State) == 0u32 { return Invalid; }')
[IO.File]::WriteAllText(
    $WvbExecutableVerifySource,
    $WvbExecutableVerifyText,
    [Text.UTF8Encoding]::new($false))

$WvbCompilerExecutablePhaseText = $WvbExecutableVerifyPhaseText
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Functionˉcount > 256u32 { return Invalid; }',
    'if Functionˉcount > 4096u32 { return Invalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Codeˉlength > 131072u32 { return Invalid; }',
    'if Codeˉlength > 4194304u32 { return Invalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Declaredˉmaximum > 16u32 { return Invalid; }',
    'if Declaredˉmaximum > 4096u32 { return Invalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Depth > 16u32 { return Invalid; }',
    'if Depth > 4096u32 { return Invalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Aggregateˉinstructions > 16000u32 { return Invalid; }',
    'if Aggregateˉinstructions > 400000u32 { return Invalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Controlˉfunctionˉcount > 256u32 { return Controlˉinvalid; }',
    'if Controlˉfunctionˉcount > 4096u32 { return Controlˉinvalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Controlˉcodeˉlength > 131072u32 { return Controlˉinvalid; }',
    'if Controlˉcodeˉlength > 4194304u32 { return Controlˉinvalid; }')
$WvbCompilerExecutablePhaseText = $WvbCompilerExecutablePhaseText.Replace(
    'if Controlˉaggregateˉinstructions > 16000u32 {',
    'if Controlˉaggregateˉinstructions > 400000u32 {')
[IO.File]::WriteAllText(
    $WvbCompilerExecutablePhaseSource,
    $WvbCompilerExecutablePhaseText,
    [Text.UTF8Encoding]::new($false))

$WvbCompilerTypedSplitAnchor =
    "        Functionˉcursor = Functionˉcursor + 12u32;`n" +
    '        if Declaredˉmaximum > 4096u32 { return Invalid; }'
$WvbCompilerTypedFirstPhaseText = $WvbCompilerExecutablePhaseText.Replace(
    $WvbCompilerTypedSplitAnchor,
    $WvbCompilerTypedSplitAnchor + "`n" +
        "        if Functionˉindex >= 200u32 {`n" +
        "            Functionˉindex = Functionˉindex + 1u32;`n" +
        "            continue;`n" +
        '        }')
$WvbCompilerTypedSecondPhaseText = $WvbCompilerExecutablePhaseText.Replace(
    $WvbCompilerTypedSplitAnchor,
    $WvbCompilerTypedSplitAnchor + "`n" +
        "        if Functionˉindex < 200u32 || Functionˉindex >= 300u32 {`n" +
            "            Functionˉindex = Functionˉindex + 1u32;`n" +
            "            continue;`n" +
        '        }')
$WvbCompilerTypedThirdPhaseText = $WvbCompilerExecutablePhaseText.Replace(
    $WvbCompilerTypedSplitAnchor,
    $WvbCompilerTypedSplitAnchor + "`n" +
        "        if Functionˉindex < 300u32 {`n" +
            "            Functionˉindex = Functionˉindex + 1u32;`n" +
            "            continue;`n" +
        '        }')
if (
    $WvbCompilerTypedFirstPhaseText -eq $WvbCompilerExecutablePhaseText -or
    $WvbCompilerTypedSecondPhaseText -eq $WvbCompilerExecutablePhaseText -or
    $WvbCompilerTypedThirdPhaseText -eq $WvbCompilerExecutablePhaseText
) {
    throw 'The compiler-capacity typed verifier split anchor was not found.'
}

$WvbCompilerControlSplitAnchor =
    '        Controlˉfunctionˉcursor = Controlˉfunctionˉcursor + 12u32;'
$WvbCompilerControlFirstPhaseText = $WvbCompilerExecutablePhaseText.Replace(
    $WvbCompilerControlSplitAnchor,
    $WvbCompilerControlSplitAnchor + "`n" +
        "        if Controlˉfunctionˉindex >= 200u32 {`n" +
            "            Controlˉfunctionˉindex = Controlˉfunctionˉindex + 1u32;`n" +
            "            continue;`n" +
        '        }')
$WvbCompilerControlSecondPhaseText = $WvbCompilerExecutablePhaseText.Replace(
    $WvbCompilerControlSplitAnchor,
    $WvbCompilerControlSplitAnchor + "`n" +
        "        if Controlˉfunctionˉindex < 200u32 {`n" +
            "            Controlˉfunctionˉindex = Controlˉfunctionˉindex + 1u32;`n" +
            "            continue;`n" +
        '        }')
if (
    $WvbCompilerControlFirstPhaseText -eq $WvbCompilerExecutablePhaseText -or
    $WvbCompilerControlSecondPhaseText -eq $WvbCompilerExecutablePhaseText
) {
    throw 'The compiler-capacity control verifier split anchor was not found.'
}

$WvbCompilerSemanticVerifyText = [IO.File]::ReadAllText($WvbSemanticVerifySource)
$WvbCompilerSemanticVerifyText = $WvbCompilerSemanticVerifyText.Replace(
    'if Index >= 100000u32 { return Invalid; }',
    'if Index >= 400000u32 { return Invalid; }')
[IO.File]::WriteAllText(
    $WvbCompilerSemanticVerifySource,
    $WvbCompilerSemanticVerifyText,
    [Text.UTF8Encoding]::new($false))

function Write-CompilerVerifierPhase(
    [string]$Path,
    [string]$Module,
    [string]$Function,
    [string]$PhaseText
) {
    $InlinePhase = $PhaseText.Replace(
        "module WebAssemblyˉwvbˉexecutableˉverify profile portable;`n`n",
        '')
    $InlinePhase = $InlinePhase.Replace(
        'export fn Hˉexecutable',
        'fn Hˉexecutable')
    $InlinePhase = $InlinePhase.Replace(
        'export fn Iˉcontrol',
        'fn Iˉcontrol')
    $Source = "module $Module profile portable;`n`n" +
        $InlinePhase + "`n" +
        "export fn Main(Input: bytes) -> bytes {`n" +
        "    let State: bytes = $Function(Input);`n" +
        "    if Bytesˉlength(State) == 0u32 { return State; }`n" +
        "    return Bytesˉfromˉu8(1u8);`n" +
        "}`n"
    [IO.File]::WriteAllText($Path, $Source, [Text.UTF8Encoding]::new($false))
}

Write-CompilerVerifierPhase `
    $WvbCompilerTypedVerifySource `
    'WebAssemblyˉwvbˉcompilerˉtypedˉverify' `
    'Hˉexecutable' `
    $WvbCompilerTypedFirstPhaseText
Write-CompilerVerifierPhase `
    $WvbCompilerTypedSecondVerifySource `
    'WebAssemblyˉwvbˉcompilerˉtypedˉsecondˉverify' `
    'Hˉexecutable' `
    $WvbCompilerTypedSecondPhaseText
Write-CompilerVerifierPhase `
    $WvbCompilerTypedThirdVerifySource `
    'WebAssemblyˉwvbˉcompilerˉtypedˉthirdˉverify' `
    'Hˉexecutable' `
    $WvbCompilerTypedThirdPhaseText
Write-CompilerVerifierPhase `
    $WvbCompilerControlVerifySource `
    'WebAssemblyˉwvbˉcompilerˉcontrolˉverify' `
    'Iˉcontrol' `
    $WvbCompilerControlFirstPhaseText
Write-CompilerVerifierPhase `
    $WvbCompilerControlSecondVerifySource `
    'WebAssemblyˉwvbˉcompilerˉcontrolˉsecondˉverify' `
    'Iˉcontrol' `
    $WvbCompilerControlSecondPhaseText

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
Invoke-Windvale @('compile', $RuntimeReclaimSource, '-o', $RuntimeReclaimWvb)
Invoke-Windvale @('compile', $RuntimeU32GuardSource, '-o', $RuntimeU32GuardWvb)
Invoke-Windvale @('compile', $RuntimeCallsSource, '-o', $RuntimeCallsWvb)
Invoke-Windvale @('compile', $WvbEnvelopeVerifySource, '-o', $WvbEnvelopeVerifyWvb)
Invoke-Windvale @('compile', $WvbStructuralVerifySource, '-o', $WvbStructuralVerifyWvb)
Invoke-Windvale @('compile', $WvbSemanticVerifySource, '-o', $WvbSemanticVerifyWvb)
Invoke-Windvale @('compile', $WvbSemanticExpandedSource, '-o', $WvbSemanticExpandedWvb)
Invoke-Windvale @('compile', $WvbExecutableVerifySource, '-o', $WvbExecutableVerifyWvb)
Invoke-Windvale @('compile', $WvbCompilerSemanticVerifySource, '-o', $WvbCompilerSemanticVerifyWvb)
Invoke-Windvale @('compile', $WvbCompilerTypedVerifySource, '-o', $WvbCompilerTypedVerifyWvb)
Invoke-Windvale @('compile', $WvbCompilerTypedSecondVerifySource, '-o', $WvbCompilerTypedSecondVerifyWvb)
Invoke-Windvale @('compile', $WvbCompilerTypedThirdVerifySource, '-o', $WvbCompilerTypedThirdVerifyWvb)
Invoke-Windvale @('compile', $WvbCompilerControlVerifySource, '-o', $WvbCompilerControlVerifyWvb)
Invoke-Windvale @('compile', $WvbCompilerControlSecondVerifySource, '-o', $WvbCompilerControlSecondVerifyWvb)
Invoke-Windvale @('build', $CompilerProject, '-o', $CompilerWvb)
Invoke-Windvale @('build', $CompilerMemoryProject, '-o', $CompilerMemoryWvb)
Invoke-Windvale @('build', $WvbScalarInterpreterProject, '-o', $WvbScalarInterpreterWvb)
Invoke-Windvale @('compile', $ScalarFunctionOnlySource, '-o', $ScalarFunctionOnlyWvb)
Invoke-Windvale @('compile', $BytesEntrySource, '-o', $BytesEntryWvb)
Invoke-Windvale @('compile', $ScalarGuestSource, '-o', $ScalarGuestWvb)
Invoke-Windvale @('compile', $ScalarI32OverflowSource, '-o', $ScalarI32OverflowWvb)
Invoke-Windvale @('compile', $ScalarU32OverflowSource, '-o', $ScalarU32OverflowWvb)
Invoke-Windvale @('compile', $TextBytesGuestSource, '-o', $TextBytesGuestWvb)
Invoke-Windvale @('compile', $TextBytesUtf8Source, '-o', $TextBytesUtf8Wvb)
Invoke-Windvale @('compile', $TextBytesInvalidUtf8Source, '-o', $TextBytesInvalidUtf8Wvb)
Invoke-Windvale @('compile', $TextBytesRangeSource, '-o', $TextBytesRangeWvb)
Invoke-Windvale @('compile', $TextBytesU16Source, '-o', $TextBytesU16Wvb)
Invoke-Windvale @('compile', $TextBytesValueSource, '-o', $TextBytesValueWvb)
Invoke-Windvale @('compile', $TextBytesHeapSource, '-o', $TextBytesHeapWvb)
Invoke-Windvale @('compile', $FormattingQuoteSource, '-o', $FormattingQuoteWvb)
Invoke-Windvale @('compile', $Sha256Source, '-o', $Sha256Wvb)
Invoke-Windvale @('compile', $NominalDefaultsSource, '-o', $NominalDefaultsWvb)
Invoke-Windvale @('compile', $RecordArenaSource, '-o', $RecordArenaWvb)
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
Invoke-Windvale ($RunArguments + @($RuntimeReclaimWvb, $RuntimeReclaimWasm))
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
Invoke-Windvale ($SemanticRunArguments + @(
    $WvbCompilerSemanticVerifyWvb,
    $WvbCompilerSemanticVerifyWasm))
$ExecutableSemanticRunArguments = @(
    'run', $BackendWvb,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '275000000',
    '--'
)
$ScalarInterpreterRunArguments = @(
    'run', $BackendWvb,
    '--allow', 'console.write_line',
    '--allow', 'diagnostic.write_line',
    '--allow', 'file.read_bytes',
    '--allow', 'file.write_bytes',
    '--allow', 'process.argument',
    '--allow', 'process.argument_count',
    '--max-steps', '500000000',
    '--'
)
Invoke-Windvale ($ExecutableSemanticRunArguments + @(
    $WvbExecutableVerifyWvb,
    $WvbExecutableVerifyWasm))
Invoke-Windvale ($ExecutableSemanticRunArguments + @(
    $WvbCompilerTypedVerifyWvb,
    $WvbCompilerTypedVerifyWasm))
Invoke-Windvale ($ExecutableSemanticRunArguments + @(
    $WvbCompilerTypedSecondVerifyWvb,
    $WvbCompilerTypedSecondVerifyWasm))
Invoke-Windvale ($ExecutableSemanticRunArguments + @(
    $WvbCompilerTypedThirdVerifyWvb,
    $WvbCompilerTypedThirdVerifyWasm))
Invoke-Windvale ($ExecutableSemanticRunArguments + @(
    $WvbCompilerControlVerifyWvb,
    $WvbCompilerControlVerifyWasm))
Invoke-Windvale ($ExecutableSemanticRunArguments + @(
    $WvbCompilerControlSecondVerifyWvb,
    $WvbCompilerControlSecondVerifyWasm))
Invoke-Windvale ($ScalarInterpreterRunArguments + @(
    $WvbScalarInterpreterWvb,
    $WvbScalarInterpreterWasm))

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
    $StructuralCapabilitiesWvb `
    $WvbExecutableVerifyWasm `
    $StructuralDataWvb `
    $StructuralTypesWvb `
    $StructuralCapabilitiesWvb `
    $WvbScalarInterpreterWasm `
    $ScalarFunctionOnlyWvb `
    $ScalarGuestWvb `
    $ScalarI32OverflowWvb `
    $ScalarU32OverflowWvb `
    $TextBytesGuestWvb `
    $TextBytesUtf8Wvb `
    $TextBytesInvalidUtf8Wvb `
    $TextBytesRangeWvb `
    $TextBytesU16Wvb `
    $TextBytesValueWvb `
    $TextBytesHeapWvb `
    $FormattingQuoteWvb `
    $Sha256Wvb `
    $NominalDefaultsWvb `
    $RecordArenaWvb `
    $WvbCompilerSemanticVerifyWasm `
    $WvbCompilerTypedVerifyWasm `
    $WvbCompilerControlVerifyWasm `
    $CompilerWvb `
    $BytesEntryWvb `
    $CompilerMemoryWvb `
    $ScalarFunctionOnlySource `
    $WvbCompilerTypedSecondVerifyWasm `
    $RuntimeReclaimWasm `
    $WvbCompilerTypedThirdVerifyWasm `
    $WvbCompilerControlSecondVerifyWasm
if ($LASTEXITCODE -ne 0) { throw 'The WebAssembly engine verification failed.' }

Write-Output 'Windvale-authored WebAssembly verification passed.'
