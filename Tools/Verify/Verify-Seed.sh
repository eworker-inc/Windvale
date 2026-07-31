#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
CONFIGURATION=${CONFIGURATION:-Release}
ARCHITECTURE=$(uname -m)
REPORT_PATH=${1:-"$REPOSITORY_ROOT/artifacts/seed-conformance-linux-$ARCHITECTURE.json"}
VERIFY_LEVEL=${VERIFY_LEVEL:-qualification}
TEST_FILTER=${TEST_FILTER:-}
TEST_AREAS=${TEST_AREAS:-}
FAIL_FAST=${FAIL_FAST:-0}
TIMING_REPORT_PATH=${TIMING_REPORT_PATH:-}
TOOL_DLL="$REPOSITORY_ROOT/Tools/Windvale.Tool/bin/$CONFIGURATION/net10.0/windvale.dll"
TEST_PROJECT="$REPOSITORY_ROOT/Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj"
ARTIFACTS="$REPOSITORY_ROOT/artifacts"
mkdir -p "$ARTIFACTS"

case "$VERIFY_LEVEL" in
    fast)
        if [ -z "$TEST_FILTER" ] && [ -z "$TEST_AREAS" ]; then
            echo 'Fast verification requires TEST_FILTER or TEST_AREAS so its scope is explicit.' >&2
            exit 64
        fi
        ;;
    standard|qualification)
        if [ -n "$TEST_FILTER" ] || [ -n "$TEST_AREAS" ]; then
            echo 'Test selection is available only with VERIFY_LEVEL=fast.' >&2
            exit 64
        fi
        if [ "$FAIL_FAST" != '0' ]; then
            echo 'FAIL_FAST is available only with VERIFY_LEVEL=fast.' >&2
            exit 64
        fi
        ;;
    *)
        echo "Unknown VERIFY_LEVEL: $VERIFY_LEVEL" >&2
        exit 64
        ;;
esac

dotnet build "$REPOSITORY_ROOT/Windvale.slnx" --configuration "$CONFIGURATION" --nologo

if [ "$VERIFY_LEVEL" = 'fast' ]; then
    set --
    if [ -n "$TEST_FILTER" ]; then
        set -- "$@" --filter "$TEST_FILTER"
    fi
    if [ -n "$TEST_AREAS" ]; then
        SAVED_IFS=$IFS
        IFS=','
        for TEST_AREA in $TEST_AREAS; do
            if [ -z "$TEST_AREA" ]; then
                echo 'TEST_AREAS contains an empty area name.' >&2
                exit 64
            fi
            set -- "$@" --area "$TEST_AREA"
        done
        IFS=$SAVED_IFS
    fi
    if [ "$FAIL_FAST" = '1' ]; then
        set -- "$@" --fail-fast
    fi
else
    set -- --report "$REPORT_PATH"
fi
if [ -n "$TIMING_REPORT_PATH" ]; then
    set -- "$@" --timing-report "$TIMING_REPORT_PATH"
fi

dotnet run \
    --project "$TEST_PROJECT" \
    --configuration "$CONFIGURATION" \
    --no-build \
    -- \
    "$@"

if [ "$VERIFY_LEVEL" = 'fast' ]; then
    SELECTION_DESCRIPTION=
    if [ -n "$TEST_FILTER" ]; then
        SELECTION_DESCRIPTION="filter '$TEST_FILTER'"
    fi
    if [ -n "$TEST_AREAS" ]; then
        if [ -n "$SELECTION_DESCRIPTION" ]; then
            SELECTION_DESCRIPTION="$SELECTION_DESCRIPTION and areas [$TEST_AREAS]"
        else
            SELECTION_DESCRIPTION="areas [$TEST_AREAS]"
        fi
    fi
    echo "Windvale Seed fast verification passed for $SELECTION_DESCRIPTION."
    exit 0
fi
if [ "$VERIFY_LEVEL" = 'standard' ]; then
    echo 'Windvale Seed standard conformance verification passed.'
    echo "Conformance report: $REPORT_PATH"
    exit 0
fi

SUM_MODULE="$ARTIFACTS/Sum-Data.wvb"
HELLO_MODULE="$ARTIFACTS/Hello-Windvale.wvb"
FOUNDATION_MODULE="$ARTIFACTS/Read-Wvb-Header.wvb"
COMPOSITION_MODULE="$ARTIFACTS/Module-Composition-Demo.wvb"
COMPOSITION_REORDERED_MODULE="$ARTIFACTS/Module-Composition-Demo-Reordered.wvb"
INVALID_COMPOSITION_MODULE="$ARTIFACTS/__windvale_invalid_composition_output__.wvb"
MACHINE_CONTRACTS_MODULE="$ARTIFACTS/Machine-Contracts.wvb"
MACHINE_CONTRACTS_DEMO_MODULE="$ARTIFACTS/Machine-Contracts-Demo.wvb"
BYTE_ORDERING_MODULE="$ARTIFACTS/Byte-Ordering.wvb"
BYTE_ORDERING_DEMO_MODULE="$ARTIFACTS/Byte-Ordering-Demo.wvb"
DECIMAL_PARSING_MODULE="$ARTIFACTS/Decimal-Parsing.wvb"
DECIMAL_PARSING_DEMO_MODULE="$ARTIFACTS/Decimal-Parsing-Demo.wvb"
BYTE_CONSTRUCTION_MODULE="$ARTIFACTS/Byte-Construction.wvb"
BYTE_CONSTRUCTION_DEMO_MODULE="$ARTIFACTS/Byte-Construction-Demo.wvb"
SOURCE_LEXER_MODULE="$ARTIFACTS/Source-Lexer-Core.wvb"
SOURCE_LEXER_DEMO_MODULE="$ARTIFACTS/Source-Lexer-Demo.wvb"
SOURCE_DECLARATION_PARSER_MODULE="$ARTIFACTS/Source-Declaration-Parser.wvb"
SOURCE_DECLARATION_PARSER_DEMO_MODULE="$ARTIFACTS/Source-Declaration-Parser-Demo.wvb"
SOURCE_DECLARATION_PARSER_TOOL_MODULE="$ARTIFACTS/Source-Declaration-Parser-Tool.wvb"
SOURCE_BODY_PARSER_MODULE="$ARTIFACTS/Source-Body-Parser.wvb"
SOURCE_BODY_PARSER_DEMO_MODULE="$ARTIFACTS/Source-Body-Parser-Demo.wvb"
SOURCE_BODY_PARSER_TOOL_MODULE="$ARTIFACTS/Source-Body-Parser-Tool.wvb"
SOURCE_SET_MODULE="$ARTIFACTS/Source-Set-Core.wvb"
SOURCE_SET_DEMO_MODULE="$ARTIFACTS/Source-Set-Demo.wvb"
SOURCE_SET_TOOL_MODULE="$ARTIFACTS/Source-Set-Tool.wvb"
SOURCE_GRAPH_MODULE="$ARTIFACTS/Source-Graph-Core.wvb"
SOURCE_GRAPH_DEMO_MODULE="$ARTIFACTS/Source-Graph-Demo.wvb"
SOURCE_GRAPH_TOOL_MODULE="$ARTIFACTS/Source-Graph-Tool.wvb"
SOURCE_SYMBOLS_MODULE="$ARTIFACTS/Source-Symbols-Core.wvb"
SOURCE_SYMBOLS_DEMO_MODULE="$ARTIFACTS/Source-Symbols-Demo.wvb"
SOURCE_SYMBOLS_TOOL_MODULE="$ARTIFACTS/Source-Symbols-Tool.wvb"
SOURCE_BINDINGS_MODULE="$ARTIFACTS/Source-Bindings-Core.wvb"
SOURCE_BINDINGS_DEMO_MODULE="$ARTIFACTS/Source-Bindings-Demo.wvb"
SOURCE_BINDINGS_TOOL_MODULE="$ARTIFACTS/Source-Bindings-Tool.wvb"
SOURCE_WIR_MODULE="$ARTIFACTS/Source-Wir-Core.wvb"
SOURCE_WIR_DEMO_MODULE="$ARTIFACTS/Source-Wir-Demo.wvb"
SOURCE_WIR_TOOL_MODULE="$ARTIFACTS/Source-Wir-Tool.wvb"
SOURCE_WVB_MODULE="$ARTIFACTS/Source-Wvb-Core.wvb"
SOURCE_WVB_DEMO_MODULE="$ARTIFACTS/Source-Wvb-Demo.wvb"
SOURCE_WVB_TOOL_MODULE="$ARTIFACTS/Source-Wvb-Tool.wvb"
SOURCE_WVB_FIXTURE_MODULE="$ARTIFACTS/Source-Wvb-Function-Only.wvb"
SOURCE_WVB_FIXTURE_ORACLE="$ARTIFACTS/Source-Wvb-Function-Only-Stage0.wvb"
SOURCE_WVB_DATA_FIXTURE_MODULE="$ARTIFACTS/Source-Wvb-Data-And-Text.wvb"
SOURCE_WVB_DATA_FIXTURE_ORACLE="$ARTIFACTS/Source-Wvb-Data-And-Text-Stage0.wvb"
SOURCE_WVB_NOMINAL_FIXTURE_MODULE="$ARTIFACTS/Source-Wvb-Nominal-Types.wvb"
SOURCE_WVB_NOMINAL_FIXTURE_ORACLE="$ARTIFACTS/Source-Wvb-Nominal-Types-Stage0.wvb"
SOURCE_WVB_HOSTED_FIXTURE_MODULE="$ARTIFACTS/Source-Wvb-Hosted-Capabilities.wvb"
SOURCE_WVB_HOSTED_FIXTURE_ORACLE="$ARTIFACTS/Source-Wvb-Hosted-Capabilities-Stage0.wvb"
SOURCE_WVB_COMPOSITION_MODULE="$ARTIFACTS/Source-Wvb-Composition.wvb"
SOURCE_WVB_COMPOSITION_ORACLE="$ARTIFACTS/Source-Wvb-Composition-Stage0.wvb"
INVALID_SOURCE_WVB_COMPOSITION_MODULE="$ARTIFACTS/__windvale_invalid_source_wvb_composition_output__.wvb"
WVDUMP_CORE_MODULE="$ARTIFACTS/Wv-Dump-Core.wvb"
WVO_CORE_MODULE="$ARTIFACTS/Wvo-Object-Core.wvb"
WVA_ASSEMBLER_MODULE="$ARTIFACTS/Wva-Assembler-Core.wvb"
WVLINK_CORE_MODULE="$ARTIFACTS/Wv-Linker-Core.wvb"
WVO_SAMPLE="$ARTIFACTS/Sample.wvo"
ASSEMBLY_OBJECT="$ARTIFACTS/Hello-Object.wvo"
WINDVALE_ASSEMBLY_OBJECT="$ARTIFACTS/Hello-Object-Windvale.wvo"
INVALID_WINDVALE_ASSEMBLY_OBJECT="$ARTIFACTS/__windvale_invalid_assembly_output__.wvo"
LINK_PROVIDER_OBJECT="$ARTIFACTS/Console-Provider.wvo"
WINDVALE_LINKED_IMAGE="$ARTIFACTS/Hello-Linked-Windvale.bin"
WINDVALE_LINK_MAP="$ARTIFACTS/Hello-Linked-Windvale.wvmap"
INVALID_WINDVALE_LINKED_IMAGE="$ARTIFACTS/__windvale_invalid_wvlink_output__.bin"
LINKED_IMAGE="$ARTIFACTS/Hello-Linked.bin"
LINK_MAP="$ARTIFACTS/Hello-Linked.wvmap"
INVALID_LINKED_IMAGE="$ARTIFACTS/__windvale_invalid_link_output__.bin"
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" -o "$SUM_MODULE"

VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SUM_MODULE")
printf '%s\n' "$VERIFY_OUTPUT" | grep -F 'Verified: Sumˉdata' >/dev/null

INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$SUM_MODULE")
printf '%s\n' "$INSPECT_OUTPUT" | grep -F 'data.load.i32' >/dev/null

RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SUM_MODULE")
printf '%s\n' "$RUN_OUTPUT" | grep -F 'Result: 29' >/dev/null
if printf '%s\n' "$RUN_OUTPUT" | grep -E '^Function instructions=' >/dev/null; then
    echo 'The default run unexpectedly reported per-function instruction counts.' >&2
    exit 1
fi

STEP_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" run "$SUM_MODULE" --report-steps)
printf '%s\n' "$STEP_REPORT_OUTPUT" | grep -F 'Result: 29' >/dev/null
printf '%s\n' "$STEP_REPORT_OUTPUT" | grep -E '^Instructions: [1-9][0-9]*$' >/dev/null

FUNCTION_STEP_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" run "$SUM_MODULE" --report-function-steps 2>&1)
printf '%s\n' "$FUNCTION_STEP_REPORT_OUTPUT" | grep -F 'Result: 29' >/dev/null
printf '%s\n' "$FUNCTION_STEP_REPORT_OUTPUT" | grep -F 'Function instructions=163 index=1 name=Main' >/dev/null
printf '%s\n' "$FUNCTION_STEP_REPORT_OUTPUT" | grep -F 'Function instructions=40 index=0 name=Add' >/dev/null

dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Seed/Hello-Windvale.wv" -o "$HELLO_MODULE"

set +e
UNAUTHORIZED_OUTPUT=$(dotnet "$TOOL_DLL" run "$HELLO_MODULE" 2>&1)
UNAUTHORIZED_EXIT=$?
set -e
if [ "$UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized run exit 3, found $UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

HELLO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$HELLO_MODULE" --allow console.write_line)
printf '%s\n' "$HELLO_OUTPUT" | grep -F 'Hello from Windvale' >/dev/null
printf '%s\n' "$HELLO_OUTPUT" | grep -F 'Result: 0' >/dev/null

dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Read-Wvb-Header.wv" -o "$FOUNDATION_MODULE"

FOUNDATION_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$FOUNDATION_MODULE")
printf '%s\n' "$FOUNDATION_VERIFY_OUTPUT" | grep -F 'Verified: Readˉwvbˉheader' >/dev/null

FOUNDATION_INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$FOUNDATION_MODULE")
printf '%s\n' "$FOUNDATION_INSPECT_OUTPUT" | grep -F 'bytes.read_u32_little' >/dev/null

FOUNDATION_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$FOUNDATION_MODULE")
printf '%s\n' "$FOUNDATION_RUN_OUTPUT" | grep -F 'Result: 1' >/dev/null

COMPOSITION_ROOT="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Demo.wv"
COMPOSITION_MIDDLE="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Middle.wv"
COMPOSITION_LEAF="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Leaf.wv"
dotnet "$TOOL_DLL" \
    compile "$COMPOSITION_ROOT" \
    --module "$COMPOSITION_MIDDLE" \
    --module "$COMPOSITION_LEAF" \
    -o "$COMPOSITION_MODULE"
COMPOSITION_HASH=$(sha256sum "$COMPOSITION_MODULE" | awk '{print $1}')
if [ "$COMPOSITION_HASH" != '0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60' ]; then
    echo "The composed source module has an unexpected digest: $COMPOSITION_HASH" >&2
    exit 1
fi
COMPOSITION_RUN_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$COMPOSITION_MODULE")
printf '%s\n' "$COMPOSITION_RUN_OUTPUT" | grep -F 'Result: 42' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$COMPOSITION_ROOT" \
    --module "$COMPOSITION_LEAF" \
    --module "$COMPOSITION_MIDDLE" \
    -o "$COMPOSITION_REORDERED_MODULE"
cmp "$COMPOSITION_MODULE" "$COMPOSITION_REORDERED_MODULE"
rm -f "$INVALID_COMPOSITION_MODULE"

MACHINE_CONTRACTS_SOURCE="$REPOSITORY_ROOT/Foundation/Machine-Contracts.wv"
dotnet "$TOOL_DLL" \
    compile "$MACHINE_CONTRACTS_SOURCE" -o "$MACHINE_CONTRACTS_MODULE"
MACHINE_CONTRACTS_HASH=$(sha256sum "$MACHINE_CONTRACTS_MODULE" | awk '{print $1}')
if [ "$MACHINE_CONTRACTS_HASH" != '9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a' ]; then
    echo "The Foundation machine-contract module has an unexpected digest: $MACHINE_CONTRACTS_HASH" >&2
    exit 1
fi
MACHINE_CONTRACTS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$MACHINE_CONTRACTS_MODULE")
printf '%s\n' "$MACHINE_CONTRACTS_INSPECTION" | grep -F 'Foundationˉalignmentˉisˉvalid' >/dev/null
printf '%s\n' "$MACHINE_CONTRACTS_INSPECTION" | grep -F 'Foundationˉmachineˉnameˉisˉvalid' >/dev/null
printf '%s\n' "$MACHINE_CONTRACTS_INSPECTION" | grep -F 'Exports (2)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Machine-Contracts-Demo.wv" \
    --module "$MACHINE_CONTRACTS_SOURCE" \
    -o "$MACHINE_CONTRACTS_DEMO_MODULE"
MACHINE_CONTRACTS_DEMO_HASH=$(sha256sum "$MACHINE_CONTRACTS_DEMO_MODULE" | awk '{print $1}')
if [ "$MACHINE_CONTRACTS_DEMO_HASH" != 'b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a' ]; then
    echo "The Foundation machine-contract demo has an unexpected digest: $MACHINE_CONTRACTS_DEMO_HASH" >&2
    exit 1
fi
MACHINE_CONTRACTS_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$MACHINE_CONTRACTS_DEMO_MODULE")
printf '%s\n' "$MACHINE_CONTRACTS_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

BYTE_ORDERING_SOURCE="$REPOSITORY_ROOT/Foundation/Byte-Ordering.wv"
dotnet "$TOOL_DLL" \
    compile "$BYTE_ORDERING_SOURCE" -o "$BYTE_ORDERING_MODULE"
BYTE_ORDERING_HASH=$(sha256sum "$BYTE_ORDERING_MODULE" | awk '{print $1}')
if [ "$BYTE_ORDERING_HASH" != '194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1' ]; then
    echo "The Foundation byte-ordering module has an unexpected digest: $BYTE_ORDERING_HASH" >&2
    exit 1
fi
BYTE_ORDERING_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$BYTE_ORDERING_MODULE")
printf '%s\n' "$BYTE_ORDERING_INSPECTION" | grep -F 'Foundationˉbyteˉspansˉcompare' >/dev/null
printf '%s\n' "$BYTE_ORDERING_INSPECTION" | grep -F 'Exports (1)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Byte-Ordering-Demo.wv" \
    --module "$BYTE_ORDERING_SOURCE" \
    -o "$BYTE_ORDERING_DEMO_MODULE"
BYTE_ORDERING_DEMO_HASH=$(sha256sum "$BYTE_ORDERING_DEMO_MODULE" | awk '{print $1}')
if [ "$BYTE_ORDERING_DEMO_HASH" != '0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab' ]; then
    echo "The Foundation byte-ordering demo has an unexpected digest: $BYTE_ORDERING_DEMO_HASH" >&2
    exit 1
fi
BYTE_ORDERING_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_ORDERING_DEMO_MODULE")
printf '%s\n' "$BYTE_ORDERING_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

DECIMAL_PARSING_SOURCE="$REPOSITORY_ROOT/Foundation/Decimal-Parsing.wv"
dotnet "$TOOL_DLL" \
    compile "$DECIMAL_PARSING_SOURCE" -o "$DECIMAL_PARSING_MODULE"
DECIMAL_PARSING_HASH=$(sha256sum "$DECIMAL_PARSING_MODULE" | awk '{print $1}')
if [ "$DECIMAL_PARSING_HASH" != '39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f' ]; then
    echo "The Foundation decimal-parsing module has an unexpected digest: $DECIMAL_PARSING_HASH" >&2
    exit 1
fi
DECIMAL_PARSING_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$DECIMAL_PARSING_MODULE")
printf '%s\n' "$DECIMAL_PARSING_INSPECTION" | grep -F 'Foundationˉu32ˉparse' >/dev/null
printf '%s\n' "$DECIMAL_PARSING_INSPECTION" | grep -F 'Foundationˉu32ˉdecimalˉparse' >/dev/null
printf '%s\n' "$DECIMAL_PARSING_INSPECTION" | grep -F 'Exports (1)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Decimal-Parsing-Demo.wv" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$DECIMAL_PARSING_DEMO_MODULE"
DECIMAL_PARSING_DEMO_HASH=$(sha256sum "$DECIMAL_PARSING_DEMO_MODULE" | awk '{print $1}')
if [ "$DECIMAL_PARSING_DEMO_HASH" != '16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695' ]; then
    echo "The Foundation decimal-parsing demo has an unexpected digest: $DECIMAL_PARSING_DEMO_HASH" >&2
    exit 1
fi
DECIMAL_PARSING_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$DECIMAL_PARSING_DEMO_MODULE")
printf '%s\n' "$DECIMAL_PARSING_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

BYTE_CONSTRUCTION_SOURCE="$REPOSITORY_ROOT/Foundation/Byte-Construction.wv"
dotnet "$TOOL_DLL" \
    compile "$BYTE_CONSTRUCTION_SOURCE" -o "$BYTE_CONSTRUCTION_MODULE"
BYTE_CONSTRUCTION_HASH=$(sha256sum "$BYTE_CONSTRUCTION_MODULE" | awk '{print $1}')
if [ "$BYTE_CONSTRUCTION_HASH" != '6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207' ]; then
    echo "The Foundation byte-construction module has an unexpected digest: $BYTE_CONSTRUCTION_HASH" >&2
    exit 1
fi
BYTE_CONSTRUCTION_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$BYTE_CONSTRUCTION_MODULE")
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Foundationˉbytesˉresult' >/dev/null
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Foundationˉbytesˉrepeat' >/dev/null
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Foundationˉbytesˉreplace' >/dev/null
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Exports (2)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Byte-Construction-Demo.wv" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    -o "$BYTE_CONSTRUCTION_DEMO_MODULE"
BYTE_CONSTRUCTION_DEMO_HASH=$(sha256sum "$BYTE_CONSTRUCTION_DEMO_MODULE" | awk '{print $1}')
if [ "$BYTE_CONSTRUCTION_DEMO_HASH" != 'a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29' ]; then
    echo "The Foundation byte-construction demo has an unexpected digest: $BYTE_CONSTRUCTION_DEMO_HASH" >&2
    exit 1
fi
BYTE_CONSTRUCTION_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE")
printf '%s\n' "$BYTE_CONSTRUCTION_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_LEXER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Lexer-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_LEXER_MODULE"
SOURCE_LEXER_HASH=$(sha256sum "$SOURCE_LEXER_MODULE" | awk '{print $1}')
if [ "$SOURCE_LEXER_HASH" != '4d48af0c208e88d9e84d48c80324f35bed1985a799bd275b65b6a07f70111706' ]; then
    echo "The Windvale source lexer has an unexpected digest: $SOURCE_LEXER_HASH" >&2
    exit 1
fi
SOURCE_LEXER_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_LEXER_MODULE")
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Nominal types (6)' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Compilerˉsourceˉtoken' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Compilerˉtokenˉkind' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Compilerˉlexˉsourceˉbounded' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Exports (14)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Lexer-Demo.wv" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_LEXER_DEMO_MODULE"
SOURCE_LEXER_DEMO_HASH=$(sha256sum "$SOURCE_LEXER_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_LEXER_DEMO_HASH" != '5422673a70ecf92f99f9a2db144f9b7a691d6281a98284dde6c6bc796ada60a4' ]; then
    echo "The Windvale source-lexer demo has an unexpected digest: $SOURCE_LEXER_DEMO_HASH" >&2
    exit 1
fi
SOURCE_LEXER_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_LEXER_DEMO_MODULE" --max-steps 10000000)
printf '%s\n' "$SOURCE_LEXER_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_DECLARATION_PARSER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Declaration-Parser.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_DECLARATION_PARSER_MODULE"
SOURCE_DECLARATION_PARSER_HASH=$(sha256sum "$SOURCE_DECLARATION_PARSER_MODULE" | awk '{print $1}')
if [ "$SOURCE_DECLARATION_PARSER_HASH" != '593e841ce9b751015e3de9f3100f4defe83d575b29324784839c38e227ff1276' ]; then
    echo "The Windvale declaration parser has an unexpected digest: $SOURCE_DECLARATION_PARSER_HASH" >&2
    exit 1
fi
SOURCE_DECLARATION_PARSER_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_DECLARATION_PARSER_MODULE")
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Nominal types (14)' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉdeclaration' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉmoduleˉsummary' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Compilerˉparseˉnextˉdeclarationˉvalidated' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Exports (24)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Declaration-Parser-Demo.wv" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_DECLARATION_PARSER_DEMO_MODULE"
SOURCE_DECLARATION_PARSER_DEMO_HASH=$(sha256sum "$SOURCE_DECLARATION_PARSER_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_DECLARATION_PARSER_DEMO_HASH" != '3ed1fc6ff4453da1cbfb100e6978029c3db2bb9baaec98c230db6ef1f6267e38' ]; then
    echo "The declaration-parser demo has an unexpected digest: $SOURCE_DECLARATION_PARSER_DEMO_HASH" >&2
    exit 1
fi
SOURCE_DECLARATION_PARSER_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_DECLARATION_PARSER_DEMO_MODULE" --max-steps 20000000)
printf '%s\n' "$SOURCE_DECLARATION_PARSER_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Declaration-Parser-Tool.wv" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_DECLARATION_PARSER_TOOL_MODULE"
SOURCE_DECLARATION_PARSER_TOOL_HASH=$(sha256sum "$SOURCE_DECLARATION_PARSER_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_DECLARATION_PARSER_TOOL_HASH" != '143f9c991de2cc309861aa9ea2beb948bca06cfd22b0f932c8f7abcc41ba9408' ]; then
    echo "The declaration-parser tool has an unexpected digest: $SOURCE_DECLARATION_PARSER_TOOL_HASH" >&2
    exit 1
fi
SOURCE_LEXER_DECLARATION_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_DECLARATION_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 30000000 \
    -- "$SOURCE_LEXER_SOURCE")
printf '%s\n' "$SOURCE_LEXER_DECLARATION_OUTPUT" | grep -F 'source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=14 tokens=5061 offset=41736' >/dev/null
printf '%s\n' "$SOURCE_LEXER_DECLARATION_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_PARSER_SELF_DECLARATION_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_DECLARATION_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 45000000 \
    -- "$SOURCE_DECLARATION_PARSER_SOURCE")
printf '%s\n' "$SOURCE_PARSER_SELF_DECLARATION_OUTPUT" | grep -F 'source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=24 tokens=8876 offset=64950' >/dev/null
printf '%s\n' "$SOURCE_PARSER_SELF_DECLARATION_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_BODY_PARSER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Body-Parser.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BODY_PARSER_MODULE"
SOURCE_BODY_PARSER_HASH=$(sha256sum "$SOURCE_BODY_PARSER_MODULE" | awk '{print $1}')
if [ "$SOURCE_BODY_PARSER_HASH" != '7b56ea4d25f2d13467d19123654bb8d617ae2e1b0dd43f2497e1ff9644cc3839' ]; then
    echo "The Windvale body parser has an unexpected digest: $SOURCE_BODY_PARSER_HASH" >&2
    exit 1
fi
SOURCE_BODY_PARSER_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_BODY_PARSER_MODULE")
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Nominal types (23)' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉexpression' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉstatement' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉparseˉexpressionˉvalidated' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉparseˉsourceˉbodies' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Exports (38)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Body-Parser-Demo.wv" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BODY_PARSER_DEMO_MODULE"
SOURCE_BODY_PARSER_DEMO_HASH=$(sha256sum "$SOURCE_BODY_PARSER_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_BODY_PARSER_DEMO_HASH" != '07f9b2d94b4ebefaa3260d04b2cc7400b56007f664ef93a8db97207679039005' ]; then
    echo "The body-parser demo has an unexpected digest: $SOURCE_BODY_PARSER_DEMO_HASH" >&2
    exit 1
fi
SOURCE_BODY_PARSER_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BODY_PARSER_DEMO_MODULE" --max-steps 30000000)
printf '%s\n' "$SOURCE_BODY_PARSER_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Body-Parser-Tool.wv" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BODY_PARSER_TOOL_MODULE"
SOURCE_BODY_PARSER_TOOL_HASH=$(sha256sum "$SOURCE_BODY_PARSER_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_BODY_PARSER_TOOL_HASH" != '9c8b88f9b6aaa27df5d39fc671319ed4890510535321f637a533cf2f01ddeadc' ]; then
    echo "The body-parser tool has an unexpected digest: $SOURCE_BODY_PARSER_TOOL_HASH" >&2
    exit 1
fi
SOURCE_LEXER_BODY_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BODY_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 100000000 \
    -- "$SOURCE_LEXER_SOURCE")
printf '%s\n' "$SOURCE_LEXER_BODY_OUTPUT" | grep -F 'source bodies status=Valid functions=14 top-level=123 statements=567 expression-nodes=1579 statement-depth=17 expression-depth=5 offset=41737' >/dev/null
printf '%s\n' "$SOURCE_LEXER_BODY_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_DECLARATION_BODY_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BODY_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 160000000 \
    -- "$SOURCE_DECLARATION_PARSER_SOURCE")
printf '%s\n' "$SOURCE_DECLARATION_BODY_OUTPUT" | grep -F 'source bodies status=Valid functions=24 top-level=232 statements=527 expression-nodes=2135 statement-depth=5 expression-depth=3 offset=64951' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_BODY_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_BODY_SELF_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BODY_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 160000000 \
    -- "$SOURCE_BODY_PARSER_SOURCE")
printf '%s\n' "$SOURCE_BODY_SELF_OUTPUT" | grep -F 'source bodies status=Valid functions=38 top-level=234 statements=519 expression-nodes=2500 statement-depth=5 expression-depth=3 offset=69023' >/dev/null
printf '%s\n' "$SOURCE_BODY_SELF_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_SET_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Set-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SET_MODULE"
SOURCE_SET_HASH=$(sha256sum "$SOURCE_SET_MODULE" | awk '{print $1}')
if [ "$SOURCE_SET_HASH" != 'c2a420a984a9bd39754a9e842d14e1e94030cd8ff6a0e313cc1703ae2e244386' ]; then
    echo "The Windvale source-set core has an unexpected digest: $SOURCE_SET_HASH" >&2
    exit 1
fi
SOURCE_SET_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_SET_MODULE")
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Nominal types (27)' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉsourceˉsetˉscan' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉsourceˉsetˉsummary' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉscanˉsourceˉset' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉset' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Exports (9)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Set-Demo.wv" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SET_DEMO_MODULE"
SOURCE_SET_DEMO_HASH=$(sha256sum "$SOURCE_SET_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_SET_DEMO_HASH" != '960c973b7014b9e77b33b55e9fffa7db0a4a3d0a2b87737d54603f09cec022c0' ]; then
    echo "The source-set demo has an unexpected digest: $SOURCE_SET_DEMO_HASH" >&2
    exit 1
fi
SOURCE_SET_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_SET_DEMO_MODULE" --max-steps 200000000)
printf '%s\n' "$SOURCE_SET_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Set-Tool.wv" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SET_TOOL_MODULE"
SOURCE_SET_TOOL_HASH=$(sha256sum "$SOURCE_SET_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_SET_TOOL_HASH" != 'dc8645c9b73fe8bfe10409e2fbd34fd29f125eea42409617ede5256b36a03e2e' ]; then
    echo "The source-set tool has an unexpected digest: $SOURCE_SET_TOOL_HASH" >&2
    exit 1
fi
SOURCE_SET_SELF_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_SET_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 800000000 \
    -- "$SOURCE_SET_SOURCE" \
    "$SOURCE_BODY_PARSER_SOURCE" \
    "$SOURCE_DECLARATION_PARSER_SOURCE" \
    "$SOURCE_LEXER_SOURCE" \
    "$DECIMAL_PARSING_SOURCE")
printf '%s\n' "$SOURCE_SET_SELF_OUTPUT" | grep -F 'source set status=Valid modules=5 source-bytes=194697 imports=4 records=16 enums=11 functions=86' >/dev/null
printf '%s\n' "$SOURCE_SET_SELF_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_GRAPH_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Graph-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_GRAPH_MODULE"
SOURCE_GRAPH_HASH=$(sha256sum "$SOURCE_GRAPH_MODULE" | awk '{print $1}')
if [ "$SOURCE_GRAPH_HASH" != '4b45616ff0304f59f16c44afea637fabd0f66f68ae4b5d7b149e23a9e8e70662' ]; then
    echo "The Windvale source-graph core has an unexpected digest: $SOURCE_GRAPH_HASH" >&2
    exit 1
fi
SOURCE_GRAPH_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_GRAPH_MODULE")
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Nominal types (32)' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Compilerˉsourceˉgraphˉstatus' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Compilerˉsourceˉgraphˉsummary' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉgraph' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Exports (11)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Graph-Demo.wv" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_GRAPH_DEMO_MODULE"
SOURCE_GRAPH_DEMO_HASH=$(sha256sum "$SOURCE_GRAPH_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_GRAPH_DEMO_HASH" != '309c7ca3815c709c759b4673036ffd747d95450c651aa21c3dbf66e59dbe903c' ]; then
    echo "The source-graph demo has an unexpected digest: $SOURCE_GRAPH_DEMO_HASH" >&2
    exit 1
fi
SOURCE_GRAPH_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_GRAPH_DEMO_MODULE" --max-steps 300000000)
printf '%s\n' "$SOURCE_GRAPH_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Graph-Tool.wv" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_GRAPH_TOOL_MODULE"
SOURCE_GRAPH_TOOL_HASH=$(sha256sum "$SOURCE_GRAPH_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_GRAPH_TOOL_HASH" != '4a07816c65d82b6594270a9b253b999700196d07e10faed0221fc6b1ca7e1e9e' ]; then
    echo "The source-graph tool has an unexpected digest: $SOURCE_GRAPH_TOOL_HASH" >&2
    exit 1
fi
SOURCE_GRAPH_SELF_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_GRAPH_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 1500000000 \
    -- "$SOURCE_GRAPH_SOURCE" \
    "$SOURCE_BODY_PARSER_SOURCE" \
    "$SOURCE_DECLARATION_PARSER_SOURCE" \
    "$SOURCE_LEXER_SOURCE" \
    "$SOURCE_SET_SOURCE" \
    "$BYTE_CONSTRUCTION_SOURCE" \
    "$DECIMAL_PARSING_SOURCE")
printf '%s\n' "$SOURCE_GRAPH_SELF_OUTPUT" | grep -F 'source graph status=Valid modules=7 imports=6 reachable=7' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_SELF_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_SYMBOLS_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Symbols-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SYMBOLS_MODULE"
SOURCE_SYMBOLS_HASH=$(sha256sum "$SOURCE_SYMBOLS_MODULE" | awk '{print $1}')
if [ "$SOURCE_SYMBOLS_HASH" != '063a9ff3cb37196b1e6d9c8fb5be39916fe9fc21fab8032b0415d5f8ab0677d2' ]; then
    echo "The Windvale source-symbol core has an unexpected digest: $SOURCE_SYMBOLS_HASH" >&2
    exit 1
fi
SOURCE_SYMBOLS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_SYMBOLS_MODULE")
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Nominal types (38)' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolˉstatus' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolˉsummary' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉsymbols' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Exports (36)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Symbols-Demo.wv" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SYMBOLS_DEMO_MODULE"
SOURCE_SYMBOLS_DEMO_HASH=$(sha256sum "$SOURCE_SYMBOLS_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_SYMBOLS_DEMO_HASH" != '9e7826d354d80d06702555e857f348e08a6e396cca64b11c5eda4895e7294a25' ]; then
    echo "The source-symbol demo has an unexpected digest: $SOURCE_SYMBOLS_DEMO_HASH" >&2
    exit 1
fi
SOURCE_SYMBOLS_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_SYMBOLS_DEMO_MODULE" --max-steps 1500000000)
printf '%s\n' "$SOURCE_SYMBOLS_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Symbols-Tool.wv" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SYMBOLS_TOOL_MODULE"
SOURCE_SYMBOLS_TOOL_HASH=$(sha256sum "$SOURCE_SYMBOLS_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_SYMBOLS_TOOL_HASH" != '54289a4ebfe778c6c2b6ebfb7cb7afaf3d4899af41e68a989eaa26b9dd2f28c4' ]; then
    echo "The source-symbol tool has an unexpected digest: $SOURCE_SYMBOLS_TOOL_HASH" >&2
    exit 1
fi
SOURCE_SYMBOLS_SELF_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_SYMBOLS_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_SYMBOLS_SOURCE" \
    "$SOURCE_BODY_PARSER_SOURCE" \
    "$SOURCE_DECLARATION_PARSER_SOURCE" \
    "$SOURCE_GRAPH_SOURCE" \
    "$SOURCE_LEXER_SOURCE" \
    "$SOURCE_SET_SOURCE" \
    "$BYTE_CONSTRUCTION_SOURCE" \
    "$DECIMAL_PARSING_SOURCE")
printf '%s\n' "$SOURCE_SYMBOLS_SELF_OUTPUT" | grep -F 'source symbols status=Valid modules=8 capabilities=0 data=0 records=24 enums=14 functions=135 fields=291 members=181 parameters=597 directory-bytes=4168 visibility-bytes=64' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_SELF_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_BINDINGS_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Bindings-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BINDINGS_MODULE"
SOURCE_BINDINGS_HASH=$(sha256sum "$SOURCE_BINDINGS_MODULE" | awk '{print $1}')
if [ "$SOURCE_BINDINGS_HASH" != '2c253a188c96d5bbdf7e8b81d44dbd776c27c043ba9df7cc6230972c278335e5' ]; then
    echo "The Windvale source-binding core has an unexpected digest: $SOURCE_BINDINGS_HASH" >&2
    exit 1
fi
SOURCE_BINDINGS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_BINDINGS_MODULE")
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Nominal types (47)' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉsourceˉbindingˉstatus' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉsourceˉbindingˉsummary' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉbindings' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Exports (54)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Bindings-Demo.wv" \
    --module "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BINDINGS_DEMO_MODULE"
SOURCE_BINDINGS_DEMO_HASH=$(sha256sum "$SOURCE_BINDINGS_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_BINDINGS_DEMO_HASH" != '11621f87f3fe198c97ea141f41ac1a4d69bd34e03f10c17d4bd23083b3087b18' ]; then
    echo "The source-binding demo has an unexpected digest: $SOURCE_BINDINGS_DEMO_HASH" >&2
    exit 1
fi
SOURCE_BINDINGS_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BINDINGS_DEMO_MODULE" --max-steps 2000000000)
printf '%s\n' "$SOURCE_BINDINGS_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Bindings-Tool.wv" \
    --module "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BINDINGS_TOOL_MODULE"
SOURCE_BINDINGS_TOOL_HASH=$(sha256sum "$SOURCE_BINDINGS_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_BINDINGS_TOOL_HASH" != '2e7f2885da27a5ba12f15a831bb2429a21acb7e23ad46061009b4b2209b713f4' ]; then
    echo "The source-binding tool has an unexpected digest: $SOURCE_BINDINGS_TOOL_HASH" >&2
    exit 1
fi
SOURCE_BINDINGS_SELF_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BINDINGS_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_BINDINGS_SOURCE" \
    "$SOURCE_BODY_PARSER_SOURCE" \
    "$SOURCE_DECLARATION_PARSER_SOURCE" \
    "$SOURCE_GRAPH_SOURCE" \
    "$SOURCE_LEXER_SOURCE" \
    "$SOURCE_SET_SOURCE" \
    "$SOURCE_SYMBOLS_SOURCE" \
    "$BYTE_CONSTRUCTION_SOURCE" \
    "$DECIMAL_PARSING_SOURCE")
printf '%s\n' "$SOURCE_BINDINGS_SELF_OUTPUT" | grep -F 'source bindings status=Valid modules=9 functions=189 parameters=827 locals=986 reads=8451 assignments=643 calls=1508 directory-bytes=67180' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_SELF_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_WIR_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Wir-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_WIR_SOURCE" \
    --module "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_WIR_MODULE"
SOURCE_WIR_HASH=$(sha256sum "$SOURCE_WIR_MODULE" | awk '{print $1}')
if [ "$SOURCE_WIR_HASH" != 'e11a017226c8b357732f02f5bdc6ff581c876276165e267b1875a2daa247cef0' ]; then
    echo "The Windvale typed-WVIR core has an unexpected digest: $SOURCE_WIR_HASH" >&2
    exit 1
fi
SOURCE_WIR_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WIR_MODULE")
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉsourceˉwirˉoperation' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉsourceˉwirˉsummary' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉsourceˉwirˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉwir' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Exports (64)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Wir-Demo.wv" \
    --module "$SOURCE_WIR_SOURCE" \
    --module "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_WIR_DEMO_MODULE"
SOURCE_WIR_DEMO_HASH=$(sha256sum "$SOURCE_WIR_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_WIR_DEMO_HASH" != '1af3a5d7523ce5ace0091e4892e35c54a4835487186c4e49e653a8af98ecc721' ]; then
    echo "The typed-WVIR demo has an unexpected digest: $SOURCE_WIR_DEMO_HASH" >&2
    exit 1
fi
SOURCE_WIR_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WIR_DEMO_MODULE" --max-steps 4000000000)
printf '%s\n' "$SOURCE_WIR_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Wir-Tool.wv" \
    --module "$SOURCE_WIR_SOURCE" \
    --module "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_WIR_TOOL_MODULE"
SOURCE_WIR_TOOL_HASH=$(sha256sum "$SOURCE_WIR_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_WIR_TOOL_HASH" != '3096f68bd7c1ab9e08b5112937d75ef6481b724ba84a9571fd4ce9076156cfa2' ]; then
    echo "The typed-WVIR tool has an unexpected digest: $SOURCE_WIR_TOOL_HASH" >&2
    exit 1
fi
SOURCE_WIR_FIXTURE_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WIR_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 2000000000 \
    -- "$REPOSITORY_ROOT/Tests/Fixtures/Source-Wir/Valid.wv")
printf '%s\n' "$SOURCE_WIR_FIXTURE_OUTPUT" | grep -F 'source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=3200' >/dev/null
printf '%s\n' "$SOURCE_WIR_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_WVB_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Wvb-Core.wv"
compile_source_wvb() {
    dotnet "$TOOL_DLL" \
        compile "$1" \
        --module "$SOURCE_WVB_SOURCE" \
        --module "$SOURCE_WIR_SOURCE" \
        --module "$SOURCE_BINDINGS_SOURCE" \
        --module "$SOURCE_SYMBOLS_SOURCE" \
        --module "$SOURCE_GRAPH_SOURCE" \
        --module "$SOURCE_SET_SOURCE" \
        --module "$SOURCE_BODY_PARSER_SOURCE" \
        --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
        --module "$SOURCE_LEXER_SOURCE" \
        --module "$BYTE_CONSTRUCTION_SOURCE" \
        --module "$DECIMAL_PARSING_SOURCE" \
        -o "$2"
}
dotnet "$TOOL_DLL" \
    compile "$SOURCE_WVB_SOURCE" \
    --module "$SOURCE_WIR_SOURCE" \
    --module "$SOURCE_BINDINGS_SOURCE" \
    --module "$SOURCE_SYMBOLS_SOURCE" \
    --module "$SOURCE_GRAPH_SOURCE" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_WVB_MODULE"
SOURCE_WVB_HASH=$(sha256sum "$SOURCE_WVB_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_HASH" != '32c739a08fd70e3df8551a4c15571f5f53da8d661300f55500eb15cc2c909468' ]; then
    echo "The Windvale WVB backend core has an unexpected digest: $SOURCE_WVB_HASH" >&2
    exit 1
fi
SOURCE_WVB_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WVB_MODULE")
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Compilerˉsourceˉwvbˉsummary' >/dev/null
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Compilerˉcompileˉsourceˉwvb' >/dev/null
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Exports (55)' >/dev/null
compile_source_wvb "$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Demo.wv" "$SOURCE_WVB_DEMO_MODULE"
SOURCE_WVB_DEMO_HASH=$(sha256sum "$SOURCE_WVB_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_DEMO_HASH" != '426fcdba5267db8390dfa301d16ea93f5391f25a4fd139ba17268736aefb306e' ]; then
    echo "The Windvale WVB backend demo has an unexpected digest: $SOURCE_WVB_DEMO_HASH" >&2
    exit 1
fi
SOURCE_WVB_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_DEMO_MODULE" --max-steps 4000000000)
printf '%s\n' "$SOURCE_WVB_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
compile_source_wvb "$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Tool.wv" "$SOURCE_WVB_TOOL_MODULE"
SOURCE_WVB_TOOL_HASH=$(sha256sum "$SOURCE_WVB_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_TOOL_HASH" != 'd68581ed1e89e22eee9c59d051cd1e79e2e75104287f401697528380198c0527' ]; then
    echo "The Windvale WVB backend tool has an unexpected digest: $SOURCE_WVB_TOOL_HASH" >&2
    exit 1
fi
SOURCE_WVB_FIXTURE="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Function-Only.wv"
rm -f -- "$SOURCE_WVB_FIXTURE_MODULE" "$SOURCE_WVB_FIXTURE_ORACLE"
SOURCE_WVB_FIXTURE_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_WVB_FIXTURE" "$SOURCE_WVB_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=4 code-bytes=532 module-bytes=815' >/dev/null
printf '%s\n' "$SOURCE_WVB_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉfixture' >/dev/null
SOURCE_WVB_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_RUN_OUTPUT" | grep -F 'Result: 6' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_FIXTURE" -o "$SOURCE_WVB_FIXTURE_ORACLE"
SOURCE_WVB_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_FIXTURE_HASH" != '9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761' ]; then
    echo "The Windvale-written WVB fixture has an unexpected digest: $SOURCE_WVB_FIXTURE_HASH" >&2
    exit 1
fi
cmp -s "$SOURCE_WVB_FIXTURE_MODULE" "$SOURCE_WVB_FIXTURE_ORACLE"

SOURCE_WVB_DATA_FIXTURE="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Data-And-Text.wv"
rm -f -- "$SOURCE_WVB_DATA_FIXTURE_MODULE" "$SOURCE_WVB_DATA_FIXTURE_ORACLE"
SOURCE_WVB_DATA_FIXTURE_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_WVB_DATA_FIXTURE" "$SOURCE_WVB_DATA_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_DATA_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=3 code-bytes=1210 module-bytes=1651' >/dev/null
printf '%s\n' "$SOURCE_WVB_DATA_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_DATA_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_DATA_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_DATA_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉdataˉandˉtext' >/dev/null
SOURCE_WVB_DATA_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_DATA_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_DATA_RUN_OUTPUT" | grep -F 'Result: 13' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_DATA_FIXTURE" -o "$SOURCE_WVB_DATA_FIXTURE_ORACLE"
SOURCE_WVB_DATA_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_DATA_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_DATA_FIXTURE_HASH" != '5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704' ]; then
    echo "The Windvale-written data-and-text fixture has an unexpected digest: $SOURCE_WVB_DATA_FIXTURE_HASH" >&2
    exit 1
fi
cmp -s "$SOURCE_WVB_DATA_FIXTURE_MODULE" "$SOURCE_WVB_DATA_FIXTURE_ORACLE"

SOURCE_WVB_NOMINAL_FIXTURE="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Nominal-Types.wv"
rm -f -- "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE" "$SOURCE_WVB_NOMINAL_FIXTURE_ORACLE"
SOURCE_WVB_NOMINAL_FIXTURE_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_WVB_NOMINAL_FIXTURE" "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_NOMINAL_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=3 code-bytes=1097 module-bytes=1781' >/dev/null
printf '%s\n' "$SOURCE_WVB_NOMINAL_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_NOMINAL_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_NOMINAL_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉnominalˉtypes' >/dev/null
SOURCE_WVB_NOMINAL_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_NOMINAL_RUN_OUTPUT" | grep -F 'Result: 11' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_NOMINAL_FIXTURE" -o "$SOURCE_WVB_NOMINAL_FIXTURE_ORACLE"
SOURCE_WVB_NOMINAL_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_NOMINAL_FIXTURE_HASH" != '1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a' ]; then
    echo "The Windvale-written nominal-types fixture has an unexpected digest: $SOURCE_WVB_NOMINAL_FIXTURE_HASH" >&2
    exit 1
fi
cmp -s "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE" "$SOURCE_WVB_NOMINAL_FIXTURE_ORACLE"

SOURCE_WVB_HOSTED_FIXTURE="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv"
rm -f -- "$SOURCE_WVB_HOSTED_FIXTURE_MODULE" "$SOURCE_WVB_HOSTED_FIXTURE_ORACLE"
SOURCE_WVB_HOSTED_FIXTURE_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_WVB_HOSTED_FIXTURE" "$SOURCE_WVB_HOSTED_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_HOSTED_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=7 code-bytes=249 module-bytes=849' >/dev/null
printf '%s\n' "$SOURCE_WVB_HOSTED_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_HOSTED_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_HOSTED_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_HOSTED_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉhostedˉcapabilities' >/dev/null
SOURCE_WVB_HOSTED_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WVB_HOSTED_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_HOSTED_INSPECTION" | grep -F 'Profile: hosted' >/dev/null
printf '%s\n' "$SOURCE_WVB_HOSTED_INSPECTION" | grep -F 'Capabilities (7)' >/dev/null
printf '%s\n' "$SOURCE_WVB_HOSTED_INSPECTION" | grep -F 'call.capability capability[0] (console.write)' >/dev/null
printf '%s\n' "$SOURCE_WVB_HOSTED_INSPECTION" | grep -F 'call.capability capability[6] (process.argument_count)' >/dev/null
SOURCE_WVB_HOSTED_RUN_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_HOSTED_FIXTURE_MODULE" \
    --allow console.write \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count)
printf '%s\n' "$SOURCE_WVB_HOSTED_RUN_OUTPUT" | grep -F 'Result: 0' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_HOSTED_FIXTURE" -o "$SOURCE_WVB_HOSTED_FIXTURE_ORACLE"
SOURCE_WVB_HOSTED_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_HOSTED_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_HOSTED_FIXTURE_HASH" != '1df4503a21abf5f2c0b0307ac2dc79402bc8550ec5e4a016df43fdeb8197d528' ]; then
    echo "The Windvale-written hosted-capabilities fixture has an unexpected digest: $SOURCE_WVB_HOSTED_FIXTURE_HASH" >&2
    exit 1
fi
cmp -s "$SOURCE_WVB_HOSTED_FIXTURE_MODULE" "$SOURCE_WVB_HOSTED_FIXTURE_ORACLE"

SOURCE_WVB_COMPOSITION_ROOT="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Composition-Root.wv"
SOURCE_WVB_COMPOSITION_LEAF="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Composition-Leaf.wv"
SOURCE_WVB_COMPOSITION_MIDDLE="$REPOSITORY_ROOT/Tests/Fixtures/Source-Wvb/Composition-Middle.wv"
rm -f -- \
    "$SOURCE_WVB_COMPOSITION_MODULE" \
    "$SOURCE_WVB_COMPOSITION_ORACLE" \
    "$INVALID_SOURCE_WVB_COMPOSITION_MODULE"
SOURCE_WVB_COMPOSITION_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_WVB_COMPOSITION_ROOT" \
    "$SOURCE_WVB_COMPOSITION_LEAF" \
    "$SOURCE_WVB_COMPOSITION_MIDDLE" \
    "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'source wvb status=Valid functions=5 code-bytes=451 module-bytes=1030' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_COMPOSITION_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_VERIFY_OUTPUT" | grep -F 'Verified: Compositionˉdemo' >/dev/null
SOURCE_WVB_COMPOSITION_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Data (3)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F '[2] __Text_000001: text' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Nominal types (2)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Functions (5)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Exports (1)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Main -> function[4]' >/dev/null
SOURCE_WVB_COMPOSITION_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_RUN_OUTPUT" | grep -F 'Result: 42' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$SOURCE_WVB_COMPOSITION_ROOT" \
    --module "$SOURCE_WVB_COMPOSITION_LEAF" \
    --module "$SOURCE_WVB_COMPOSITION_MIDDLE" \
    -o "$SOURCE_WVB_COMPOSITION_ORACLE"
SOURCE_WVB_COMPOSITION_HASH=$(sha256sum "$SOURCE_WVB_COMPOSITION_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_COMPOSITION_HASH" != '7279011a12f3d2becc1e9775fb92bd7c74b8760b2c94f13a282d71c0849f8e6f' ]; then
    echo "The Windvale-written multi-module fixture has an unexpected digest: $SOURCE_WVB_COMPOSITION_HASH" >&2
    exit 1
fi
cmp -s "$SOURCE_WVB_COMPOSITION_MODULE" "$SOURCE_WVB_COMPOSITION_ORACLE"

set +e
REJECTED_SOURCE_WVB_COMPOSITION_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_WVB_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 4000000000 \
    -- "$SOURCE_WVB_COMPOSITION_ROOT" \
    "$SOURCE_WVB_COMPOSITION_MIDDLE" \
    "$SOURCE_WVB_COMPOSITION_LEAF" \
    "$INVALID_SOURCE_WVB_COMPOSITION_MODULE" 2>&1)
REJECTED_SOURCE_WVB_COMPOSITION_EXIT=$?
set -e
if [ "$REJECTED_SOURCE_WVB_COMPOSITION_EXIT" -ne 0 ]; then
    echo "Expected the runtime command to complete, found exit $REJECTED_SOURCE_WVB_COMPOSITION_EXIT." >&2
    exit 1
fi
printf '%s\n' "$REJECTED_SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'source wvb status=Sourceˉwir' >/dev/null
printf '%s\n' "$REJECTED_SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'Result: 1' >/dev/null
if [ -e "$INVALID_SOURCE_WVB_COMPOSITION_MODULE" ]; then
    echo 'The rejected source-WVB composition created an output file.' >&2
    exit 1
fi

set +e
MISSING_COMPOSITION_OUTPUT=$(dotnet "$TOOL_DLL" \
    compile "$COMPOSITION_ROOT" --module "$COMPOSITION_MIDDLE" -o "$INVALID_COMPOSITION_MODULE" 2>&1)
MISSING_COMPOSITION_EXIT=$?
set -e
if [ "$MISSING_COMPOSITION_EXIT" -ne 1 ]; then
    echo "Expected missing source import exit 1, found $MISSING_COMPOSITION_EXIT." >&2
    exit 1
fi
printf '%s\n' "$MISSING_COMPOSITION_OUTPUT" | grep -F 'WVC0007' >/dev/null
if [ -e "$INVALID_COMPOSITION_MODULE" ]; then
    echo 'A rejected source-module composition created an output module.' >&2
    exit 1
fi
printf '\011\010\007' > "$INVALID_COMPOSITION_MODULE"
set +e
MISSING_COMPOSITION_OUTPUT=$(dotnet "$TOOL_DLL" \
    compile "$COMPOSITION_ROOT" --module "$COMPOSITION_MIDDLE" -o "$INVALID_COMPOSITION_MODULE" 2>&1)
MISSING_COMPOSITION_EXIT=$?
set -e
if [ "$MISSING_COMPOSITION_EXIT" -ne 1 ]; then
    echo "Expected repeated missing source import exit 1, found $MISSING_COMPOSITION_EXIT." >&2
    exit 1
fi
printf '%s\n' "$MISSING_COMPOSITION_OUTPUT" | grep -F 'WVC0007' >/dev/null
EXPECTED_EXISTING_COMPOSITION=$(printf '\011\010\007' | sha256sum | awk '{print $1}')
ACTUAL_EXISTING_COMPOSITION=$(sha256sum "$INVALID_COMPOSITION_MODULE" | awk '{print $1}')
if [ "$ACTUAL_EXISTING_COMPOSITION" != "$EXPECTED_EXISTING_COMPOSITION" ]; then
    echo 'A rejected source-module composition modified an existing output module.' >&2
    exit 1
fi
rm -f "$INVALID_COMPOSITION_MODULE"

dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Wv-Dump-Core.wv" -o "$WVDUMP_CORE_MODULE"

WVDUMP_CORE_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$WVDUMP_CORE_MODULE")
printf '%s\n' "$WVDUMP_CORE_VERIFY_OUTPUT" | grep -F 'Verified: Wvˉdumpˉcore' >/dev/null

WVDUMP_CORE_INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$WVDUMP_CORE_MODULE")
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'Inspectˉwvbˉenvelope' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'Nominal types (5)' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'record.create' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'record.field' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'enum.name' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'u32.format' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'text.concat' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'bytes.read_i32_little' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'text.utf8_is_valid' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'text.from_utf8' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'text.quote' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'u32.from_u8' >/dev/null

set +e
WVDUMP_UNAUTHORIZED_OUTPUT=$(dotnet "$TOOL_DLL" run "$WVDUMP_CORE_MODULE" 2>&1)
WVDUMP_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVDUMP_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WvDump run exit 3, found $WVDUMP_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVDUMP_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVDUMP_CORE_RUN_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVDUMP_CORE_RUN_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVDUMP_HOSTED_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$SUM_MODULE")
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'wvdump 1' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'module version=1.6 profile=portable name="Sum\u02C9data"' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'data index=0 name="Values" type=i32_array elements=4' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'instruction function=1 offset=141 opcode=call operand=0' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'export index=0 name="Main" kind=function target=1' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVDUMP_INVALID_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    -- "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" 2>&1)
printf '%s\n' "$WVDUMP_INVALID_OUTPUT" | grep -F 'Badˉmagic sections=0 offset=0' >/dev/null
printf '%s\n' "$WVDUMP_INVALID_OUTPUT" | grep -F 'Result: 2' >/dev/null

MISSING_HOSTED_FILE="$ARTIFACTS/__windvale_missing_hosted_resource__.wvb"
if [ -e "$MISSING_HOSTED_FILE" ]; then
    echo "The missing-file verifier path unexpectedly exists: $MISSING_HOSTED_FILE" >&2
    exit 1
fi
set +e
WVDUMP_MISSING_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    -- "$MISSING_HOSTED_FILE" 2>&1)
WVDUMP_MISSING_EXIT=$?
set -e
if [ "$WVDUMP_MISSING_EXIT" -ne 3 ]; then
    echo "Expected missing hosted file exit 3, found $WVDUMP_MISSING_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVDUMP_MISSING_OUTPUT" | grep -F 'WVR3022' >/dev/null

set +e
WVDUMP_INVALID_NAME_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    -- '' 2>&1)
WVDUMP_INVALID_NAME_EXIT=$?
set -e
if [ "$WVDUMP_INVALID_NAME_EXIT" -ne 3 ]; then
    echo "Expected invalid hosted file name exit 3, found $WVDUMP_INVALID_NAME_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVDUMP_INVALID_NAME_OUTPUT" | grep -F 'WVR3021' >/dev/null

dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Wvo-Object-Core.wv" \
    --module "$BYTE_ORDERING_SOURCE" \
    -o "$WVO_CORE_MODULE"

WVO_CORE_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_VERIFY_OUTPUT" | grep -F 'Verified: Wvoˉobjectˉcore' >/dev/null

WVO_CORE_INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.concat' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_u16_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_i32_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'text.to_utf8' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'Foundationˉbyteˉspansˉcompare' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'file.write_bytes' >/dev/null

set +e
WVO_UNAUTHORIZED_OUTPUT=$(dotnet "$TOOL_DLL" run "$WVO_CORE_MODULE" 2>&1)
WVO_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVO_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WVO writer run exit 3, found $WVO_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVO_SELF_TEST_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVO_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVO_HOSTED_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$WVO_SAMPLE")
printf '%s\n' "$WVO_HOSTED_OUTPUT" | grep -F 'Wrote WVO 1.0 bytes=189' >/dev/null
printf '%s\n' "$WVO_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVO_HASH=$(sha256sum "$WVO_SAMPLE" | awk '{print $1}')
if [ "$WVO_HASH" != '006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a' ]; then
    echo "The Windvale object core wrote unexpected bytes: $WVO_HASH" >&2
    exit 1
fi

WVO_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" object-verify "$WVO_SAMPLE")
printf '%s\n' "$WVO_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

WVO_INSPECTION=$(dotnet "$TOOL_DLL" object-inspect "$WVO_SAMPLE")
printf '%s\n' "$WVO_INSPECTION" | grep -F 'Sections (2)' >/dev/null
printf '%s\n' "$WVO_INSPECTION" | grep -F 'Console_write binding=Import' >/dev/null
printf '%s\n' "$WVO_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=1 symbol=2 addend=-4' >/dev/null

set +e
WVO_INVALID_NAME_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- '' 2>&1)
WVO_INVALID_NAME_EXIT=$?
set -e
if [ "$WVO_INVALID_NAME_EXIT" -ne 3 ]; then
    echo "Expected invalid hosted file writer name exit 3, found $WVO_INVALID_NAME_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_INVALID_NAME_OUTPUT" | grep -F 'WVR3021' >/dev/null

MISSING_WRITER_PARENT="$ARTIFACTS/__windvale_missing_writer_parent__"
if [ -e "$MISSING_WRITER_PARENT" ]; then
    echo "The missing writer parent unexpectedly exists: $MISSING_WRITER_PARENT" >&2
    exit 1
fi
set +e
WVO_MISSING_PARENT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$MISSING_WRITER_PARENT/Sample.wvo" 2>&1)
WVO_MISSING_PARENT_EXIT=$?
set -e
if [ "$WVO_MISSING_PARENT_EXIT" -ne 3 ]; then
    echo "Expected missing hosted writer parent exit 3, found $WVO_MISSING_PARENT_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_MISSING_PARENT_OUTPUT" | grep -F 'WVR3022' >/dev/null

dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Assembler/Windvale/Wva-Assembler-Core.wv" \
    --module "$MACHINE_CONTRACTS_SOURCE" \
    --module "$BYTE_ORDERING_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    -o "$WVA_ASSEMBLER_MODULE"

WVA_ASSEMBLER_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$WVA_ASSEMBLER_MODULE")
printf '%s\n' "$WVA_ASSEMBLER_VERIFY_OUTPUT" | grep -F 'Verified: Wvaˉassemblerˉcore' >/dev/null

WVA_ASSEMBLER_INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$WVA_ASSEMBLER_MODULE")
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Scanˉwva' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Inspectˉwvaˉsemantics' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Encodeˉwva' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Encodeˉsections' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Encodeˉsymbols' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Encodeˉrelocations' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Foundationˉmachineˉnameˉisˉvalid' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Foundationˉbyteˉspansˉcompare' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Foundationˉu32ˉdecimalˉparse' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'Foundationˉbytesˉrepeat' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'bytes.concat' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'bytes.from_u32_little' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'file.read_bytes' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F 'file.write_bytes' >/dev/null

set +e
WVA_ASSEMBLER_UNAUTHORIZED_OUTPUT=$(dotnet "$TOOL_DLL" run "$WVA_ASSEMBLER_MODULE" 2>&1)
WVA_ASSEMBLER_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVA_ASSEMBLER_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WVA assembler run exit 3, found $WVA_ASSEMBLER_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVA_ASSEMBLER_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVA_ASSEMBLER_SELF_TEST_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVA_ASSEMBLER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVA_ASSEMBLER_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Linker/Windvale/Wv-Linker-Core.wv" \
    --module "$MACHINE_CONTRACTS_SOURCE" \
    --module "$BYTE_ORDERING_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    -o "$WVLINK_CORE_MODULE"

WVLINK_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$WVLINK_CORE_MODULE")
printf '%s\n' "$WVLINK_VERIFY_OUTPUT" | grep -F 'Verified: Wvˉlinkerˉcore' >/dev/null

WVLINK_INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$WVLINK_CORE_MODULE")
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Inspectˉobject' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Findˉsection' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Findˉsymbol' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Findˉrelocation' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Validateˉexportˉuniqueness' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Validateˉimports' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Measureˉlayout' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Validateˉdefinitions' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Buildˉunrelocatedˉimage' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Applyˉrelocations' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Verifierˉplaceˉsection' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Verifierˉfindˉexport' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Verifierˉapplyˉrelocationsˉreverse' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Acceptˉreconstructedˉimage' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Acceptedˉobjectˉview' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Definitionˉmapˉminimumˉexceedsˉlimit' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Buildˉcanonicalˉmap' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Foundationˉalignmentˉisˉvalid' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Foundationˉbyteˉspansˉcompare' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Foundationˉu32ˉdecimalˉparse' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Foundationˉbytesˉrepeat' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'Foundationˉbytesˉreplace' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'bytes.read_i32_little' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'bytes.sha256_hex' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'file.read_bytes' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F 'file.write_bytes' >/dev/null

set +e
WVLINK_UNAUTHORIZED_OUTPUT=$(dotnet "$TOOL_DLL" run "$WVLINK_CORE_MODULE" 2>&1)
WVLINK_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVLINK_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized Windvale linker run exit 3, found $WVLINK_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVLINK_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVLINK_SELF_TEST_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000)
printf '%s\n' "$WVLINK_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVA_ASSEMBLER_HOSTED_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVA_ASSEMBLER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva" "$WINDVALE_ASSEMBLY_OBJECT")
printf '%s\n' "$WVA_ASSEMBLER_HOSTED_OUTPUT" | grep -F 'wvasm 1' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_HOSTED_OUTPUT" | grep -F 'assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

WINDVALE_ASSEMBLY_HASH=$(sha256sum "$WINDVALE_ASSEMBLY_OBJECT" | awk '{print $1}')
if [ "$WINDVALE_ASSEMBLY_HASH" != '992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85' ]; then
    echo "The Windvale WVA assembler wrote unexpected bytes: $WINDVALE_ASSEMBLY_HASH" >&2
    exit 1
fi
WINDVALE_ASSEMBLY_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" object-verify "$WINDVALE_ASSEMBLY_OBJECT")
printf '%s\n' "$WINDVALE_ASSEMBLY_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

WVLINK_HOSTED_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000 \
    -- "$WINDVALE_ASSEMBLY_OBJECT")
printf '%s\n' "$WVLINK_HOSTED_OUTPUT" | grep -F 'object status=Valid sections=2 symbols=3 relocations=2 offset=218' >/dev/null
printf '%s\n' "$WVLINK_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVLINK_INVALID_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000 \
    -- "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" 2>&1)
printf '%s\n' "$WVLINK_INVALID_OUTPUT" | grep -F 'object status=Badˉmagic' >/dev/null
printf '%s\n' "$WVLINK_INVALID_OUTPUT" | grep -F 'Result: 2' >/dev/null

MISSING_ASSEMBLER_PARENT="$ARTIFACTS/__windvale_missing_assembler_parent__"
if [ -e "$MISSING_ASSEMBLER_PARENT" ]; then
    echo "The missing assembler parent unexpectedly exists: $MISSING_ASSEMBLER_PARENT" >&2
    exit 1
fi
MISSING_ASSEMBLER_OUTPUT="$MISSING_ASSEMBLER_PARENT/Hello.wvo"
set +e
WVA_ASSEMBLER_MISSING_PARENT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVA_ASSEMBLER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva" "$MISSING_ASSEMBLER_OUTPUT" 2>&1)
WVA_ASSEMBLER_MISSING_PARENT_EXIT=$?
set -e
if [ "$WVA_ASSEMBLER_MISSING_PARENT_EXIT" -ne 3 ]; then
    echo "Expected missing assembler parent exit 3, found $WVA_ASSEMBLER_MISSING_PARENT_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVA_ASSEMBLER_MISSING_PARENT_OUTPUT" | grep -F 'WVR3022' >/dev/null
if [ -e "$MISSING_ASSEMBLER_OUTPUT" ]; then
    echo 'The failed Windvale assembler host write left a partial output object.' >&2
    exit 1
fi

if [ -e "$INVALID_WINDVALE_ASSEMBLY_OBJECT" ]; then
    echo "The invalid Windvale assembly output unexpectedly exists: $INVALID_WINDVALE_ASSEMBLY_OBJECT" >&2
    exit 1
fi
WVA_SEMANTIC_INVALID_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVA_ASSEMBLER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" "$INVALID_WINDVALE_ASSEMBLY_OBJECT" 2>&1)
printf '%s\n' "$WVA_SEMANTIC_INVALID_OUTPUT" | grep -F 'assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1' >/dev/null
printf '%s\n' "$WVA_SEMANTIC_INVALID_OUTPUT" | grep -F 'Result: 2' >/dev/null
if [ -e "$INVALID_WINDVALE_ASSEMBLY_OBJECT" ]; then
    echo 'Rejected Windvale assembly created a partial output object.' >&2
    exit 1
fi

WVA_SEMANTIC_EXISTING_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVA_ASSEMBLER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" "$WINDVALE_ASSEMBLY_OBJECT" 2>&1)
printf '%s\n' "$WVA_SEMANTIC_EXISTING_OUTPUT" | grep -F 'assembly status=WVA1001' >/dev/null
printf '%s\n' "$WVA_SEMANTIC_EXISTING_OUTPUT" | grep -F 'Result: 2' >/dev/null
PRESERVED_WINDVALE_ASSEMBLY_HASH=$(sha256sum "$WINDVALE_ASSEMBLY_OBJECT" | awk '{print $1}')
if [ "$PRESERVED_WINDVALE_ASSEMBLY_HASH" != "$WINDVALE_ASSEMBLY_HASH" ]; then
    echo 'Rejected Windvale assembly modified an existing output object.' >&2
    exit 1
fi

ASSEMBLY_OUTPUT=$(dotnet "$TOOL_DLL" \
    assemble "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva" -o "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_OUTPUT" | grep -F 'Assembled:' >/dev/null
printf '%s\n' "$ASSEMBLY_OUTPUT" | grep -F 'SHA-256: 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85' >/dev/null
STAGE0_ASSEMBLY_HASH=$(sha256sum "$ASSEMBLY_OBJECT" | awk '{print $1}')
if [ "$STAGE0_ASSEMBLY_HASH" != "$WINDVALE_ASSEMBLY_HASH" ]; then
    echo 'The Windvale-written and Stage 0 assembler objects differ.' >&2
    exit 1
fi

ASSEMBLY_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" object-verify "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

ASSEMBLY_INSPECTION=$(dotnet "$TOOL_DLL" object-inspect "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F '.text kind=Code align=16 memory=11 data=11' >/dev/null
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' >/dev/null
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F 'kind=Absoluteˉu32 section=1 offset=3 symbol=1 addend=0' >/dev/null

PROVIDER_ASSEMBLY_OUTPUT=$(dotnet "$TOOL_DLL" \
    assemble "$REPOSITORY_ROOT/Examples/Linker/Console-Provider.wva" -o "$LINK_PROVIDER_OBJECT")
printf '%s\n' "$PROVIDER_ASSEMBLY_OUTPUT" | grep -F 'SHA-256: 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab' >/dev/null

WVLINK_MAP_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000 \
    -- 1048576 Main "$WINDVALE_LINKED_IMAGE" "$WINDVALE_ASSEMBLY_OBJECT" "$LINK_PROVIDER_OBJECT")
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'windvale-link-map 1' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'entry name=Main address=1048576' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'image sha256=0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=6 patch-address=1048582 target=Console_write target-input=1 target-source-index=0 target-address=1048592 addend=-4 value=6' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -Fx 'relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576' >/dev/null
printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -F 'Result: 0' >/dev/null
if printf '%s\n' "$WVLINK_MAP_OUTPUT" | grep -F "$REPOSITORY_ROOT" >/dev/null; then
    echo 'The Windvale canonical link map exposed a repository path.' >&2
    exit 1
fi
WINDVALE_LINK_HASH=$(sha256sum "$WINDVALE_LINKED_IMAGE" | awk '{print $1}')
if [ "$WINDVALE_LINK_HASH" != '0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' ]; then
    echo "The Windvale linker wrote unexpected image bytes: $WINDVALE_LINK_HASH" >&2
    exit 1
fi
printf '%s\n' "$WVLINK_MAP_OUTPUT" | sed '/^Result: 0$/d' > "$WINDVALE_LINK_MAP"
WINDVALE_LINK_MAP_HASH=$(sha256sum "$WINDVALE_LINK_MAP" | awk '{print $1}')
if [ "$WINDVALE_LINK_MAP_HASH" != '31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4' ]; then
    echo "The Windvale linker wrote an unexpected canonical map: $WINDVALE_LINK_MAP_HASH" >&2
    exit 1
fi

if [ -e "$INVALID_WINDVALE_LINKED_IMAGE" ]; then
    echo "The invalid Windvale link output unexpectedly exists: $INVALID_WINDVALE_LINKED_IMAGE" >&2
    exit 1
fi
WVLINK_UNDEFINED_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000 \
    -- 1048576 Main "$INVALID_WINDVALE_LINKED_IMAGE" "$WINDVALE_ASSEMBLY_OBJECT" 2>&1)
printf '%s\n' "$WVLINK_UNDEFINED_OUTPUT" | grep -F 'link status=WVL1005 inputs=1 sections=2 symbols=3 relocations=2 image-bytes=0 entry-address=0 input=0' >/dev/null
printf '%s\n' "$WVLINK_UNDEFINED_OUTPUT" | grep -F 'Result: 2' >/dev/null
if [ -e "$INVALID_WINDVALE_LINKED_IMAGE" ]; then
    echo 'A rejected Windvale link created a partial image.' >&2
    exit 1
fi

WVLINK_EXISTING_FAILURE=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000 \
    -- 1048576 Main "$WINDVALE_LINKED_IMAGE" "$WINDVALE_ASSEMBLY_OBJECT" 2>&1)
printf '%s\n' "$WVLINK_EXISTING_FAILURE" | grep -F 'link status=WVL1005' >/dev/null
printf '%s\n' "$WVLINK_EXISTING_FAILURE" | grep -F 'Result: 2' >/dev/null
PRESERVED_WINDVALE_LINK_HASH=$(sha256sum "$WINDVALE_LINKED_IMAGE" | awk '{print $1}')
if [ "$PRESERVED_WINDVALE_LINK_HASH" != "$WINDVALE_LINK_HASH" ]; then
    echo 'A rejected Windvale link modified an existing image.' >&2
    exit 1
fi

MISSING_WINDVALE_LINK_PARENT="$ARTIFACTS/__windvale_missing_wvlink_parent__"
if [ -e "$MISSING_WINDVALE_LINK_PARENT" ]; then
    echo "The missing Windvale linker parent unexpectedly exists: $MISSING_WINDVALE_LINK_PARENT" >&2
    exit 1
fi
MISSING_WINDVALE_LINK_OUTPUT="$MISSING_WINDVALE_LINK_PARENT/Hello.bin"
set +e
MISSING_WINDVALE_LINK_PARENT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000 \
    -- 1048576 Main "$MISSING_WINDVALE_LINK_OUTPUT" "$WINDVALE_ASSEMBLY_OBJECT" "$LINK_PROVIDER_OBJECT" 2>&1)
MISSING_WINDVALE_LINK_PARENT_EXIT=$?
set -e
if [ "$MISSING_WINDVALE_LINK_PARENT_EXIT" -ne 3 ]; then
    echo "Expected missing Windvale link parent exit 3, found $MISSING_WINDVALE_LINK_PARENT_EXIT." >&2
    exit 1
fi
printf '%s\n' "$MISSING_WINDVALE_LINK_PARENT_OUTPUT" | grep -F 'WVR3022' >/dev/null
if [ -e "$MISSING_WINDVALE_LINK_OUTPUT" ]; then
    echo 'The failed Windvale linker write left a partial image.' >&2
    exit 1
fi

LINK_MAP_OUTPUT=$(dotnet "$TOOL_DLL" \
    link --base-address 1048576 --entry Main -o "$LINKED_IMAGE" "$ASSEMBLY_OBJECT" "$LINK_PROVIDER_OBJECT")
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'windvale-link-map 1' >/dev/null
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24' >/dev/null
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'entry name=Main address=1048576' >/dev/null
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'image sha256=0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' >/dev/null
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592' >/dev/null
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=6 patch-address=1048582 target=Console_write target-input=1 target-source-index=0 target-address=1048592 addend=-4 value=6' >/dev/null
printf '%s\n' "$LINK_MAP_OUTPUT" | grep -Fx 'relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576' >/dev/null
if printf '%s\n' "$LINK_MAP_OUTPUT" | grep -F "$REPOSITORY_ROOT" >/dev/null; then
    echo 'The canonical link map exposed a repository path.' >&2
    exit 1
fi
LINK_HASH=$(sha256sum "$LINKED_IMAGE" | awk '{print $1}')
if [ "$LINK_HASH" != '0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' ]; then
    echo "The Stage 0 linker wrote unexpected image bytes: $LINK_HASH" >&2
    exit 1
fi
printf '%s\n' "$LINK_MAP_OUTPUT" > "$LINK_MAP"
LINK_MAP_HASH=$(sha256sum "$LINK_MAP" | awk '{print $1}')
if [ "$LINK_MAP_HASH" != '31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4' ]; then
    echo "The Stage 0 linker wrote an unexpected canonical map: $LINK_MAP_HASH" >&2
    exit 1
fi
if ! cmp -s "$WINDVALE_LINKED_IMAGE" "$LINKED_IMAGE"; then
    echo 'The Windvale-written and Stage 0 linked images differ.' >&2
    exit 1
fi
if ! cmp -s "$WINDVALE_LINK_MAP" "$LINK_MAP"; then
    echo 'The Windvale-written and Stage 0 canonical maps differ.' >&2
    exit 1
fi

if [ -e "$INVALID_LINKED_IMAGE" ]; then
    echo "The invalid link output unexpectedly exists: $INVALID_LINKED_IMAGE" >&2
    exit 1
fi
set +e
UNDEFINED_LINK_OUTPUT=$(dotnet "$TOOL_DLL" \
    link --base-address 1048576 --entry Main -o "$INVALID_LINKED_IMAGE" "$ASSEMBLY_OBJECT" 2>&1)
UNDEFINED_LINK_EXIT=$?
set -e
if [ "$UNDEFINED_LINK_EXIT" -ne 1 ]; then
    echo "Expected undefined link exit 1, found $UNDEFINED_LINK_EXIT." >&2
    exit 1
fi
printf '%s\n' "$UNDEFINED_LINK_OUTPUT" | grep -F 'WVL1005' >/dev/null
if [ -e "$INVALID_LINKED_IMAGE" ]; then
    echo 'A rejected link created a partial image.' >&2
    exit 1
fi

set +e
EXISTING_LINK_FAILURE=$(dotnet "$TOOL_DLL" \
    link --base-address 1048576 --entry Main -o "$LINKED_IMAGE" "$ASSEMBLY_OBJECT" 2>&1)
EXISTING_LINK_EXIT=$?
set -e
if [ "$EXISTING_LINK_EXIT" -ne 1 ]; then
    echo "Expected existing-image invalid link exit 1, found $EXISTING_LINK_EXIT." >&2
    exit 1
fi
printf '%s\n' "$EXISTING_LINK_FAILURE" | grep -F 'WVL1005' >/dev/null
PRESERVED_LINK_HASH=$(sha256sum "$LINKED_IMAGE" | awk '{print $1}')
if [ "$PRESERVED_LINK_HASH" != "$LINK_HASH" ]; then
    echo 'A rejected link modified an existing image.' >&2
    exit 1
fi

MISSING_LINK_PARENT="$ARTIFACTS/__windvale_missing_link_parent__"
if [ -e "$MISSING_LINK_PARENT" ]; then
    echo "The missing linker parent unexpectedly exists: $MISSING_LINK_PARENT" >&2
    exit 1
fi
MISSING_LINK_OUTPUT="$MISSING_LINK_PARENT/Hello.bin"
set +e
MISSING_LINK_PARENT_OUTPUT=$(dotnet "$TOOL_DLL" \
    link --base-address 1048576 --entry Main -o "$MISSING_LINK_OUTPUT" "$ASSEMBLY_OBJECT" "$LINK_PROVIDER_OBJECT" 2>&1)
MISSING_LINK_PARENT_EXIT=$?
set -e
if [ "$MISSING_LINK_PARENT_EXIT" -ne 74 ]; then
    echo "Expected missing link parent exit 74, found $MISSING_LINK_PARENT_EXIT." >&2
    exit 1
fi
printf '%s\n' "$MISSING_LINK_PARENT_OUTPUT" | grep -F 'I/O failed' >/dev/null
if [ -e "$MISSING_LINK_OUTPUT" ]; then
    echo 'The failed linker write left a partial image.' >&2
    exit 1
fi

echo "Windvale Seed verification passed."
echo "Conformance report: $REPORT_PATH"
