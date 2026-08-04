#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
CONFIGURATION=${CONFIGURATION:-Release}
ARCHITECTURE=$(uname -m)
REPORT_PATH=${1:-"$REPOSITORY_ROOT/artifacts/seed-conformance-linux-$ARCHITECTURE.json"}
VERIFY_LEVEL=${VERIFY_LEVEL:-development}
TEST_FILTER=${TEST_FILTER:-}
TEST_AREAS=${TEST_AREAS:-}
FAIL_FAST=${FAIL_FAST:-0}
TIMING_REPORT_PATH=${TIMING_REPORT_PATH:-}
TOOL_DLL="$REPOSITORY_ROOT/Tools/Windvale.Tool/bin/$CONFIGURATION/net10.0/windvale.dll"
TEST_PROJECT="$REPOSITORY_ROOT/Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj"
OS_TEST_PROJECT="$REPOSITORY_ROOT/Tests/Windvale.Os.Tests/Windvale.Os.Tests.csproj"
ARTIFACTS="$REPOSITORY_ROOT/artifacts"
mkdir -p "$ARTIFACTS"

case "$VERIFY_LEVEL" in
    fast)
        if [ -z "$TEST_FILTER" ] && [ -z "$TEST_AREAS" ]; then
            echo 'Fast verification requires TEST_FILTER or TEST_AREAS so its scope is explicit.' >&2
            exit 64
        fi
        ;;
    development|standard|qualification)
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
elif [ "$VERIFY_LEVEL" = 'development' ]; then
    set -- \
        --area assembler \
        --area bytecode \
        --area compiler \
        --area database \
        --area foundation \
        --area linker \
        --area object-model \
        --area runtime
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

dotnet run \
    --project "$OS_TEST_PROJECT" \
    --configuration "$CONFIGURATION" \
    --no-build

if [ "$VERIFY_LEVEL" = 'development' ]; then
    echo 'Windvale Seed development verification passed for every regular in-process test.'
    echo 'The qualification-only golden cross-host contract was not executed.'
    exit 0
fi
if [ "$VERIFY_LEVEL" = 'standard' ]; then
    echo 'Windvale Seed standard conformance verification passed.'
    echo "Conformance report: $REPORT_PATH"
    exit 0
fi

SUM_MODULE="$ARTIFACTS/Sum-Data.wvb"
SUM_WINDOWS_APPLICATION="$ARTIFACTS/Sum-Data-Windows.exe"
SUM_LINUX_APPLICATION="$ARTIFACTS/Sum-Data-Linux.elf"
HELLO_MODULE="$ARTIFACTS/Hello-Windvale.wvb"
FOUNDATION_MODULE="$ARTIFACTS/Read-Wvb-Header.wvb"
COMPOSITION_MODULE="$ARTIFACTS/Module-Composition-Demo.wvb"
COMPOSITION_REORDERED_MODULE="$ARTIFACTS/Module-Composition-Demo-Reordered.wvb"
PROJECT_COMPOSITION_MODULE="$ARTIFACTS/Module-Composition-Demo-Project.wvb"
INVALID_PROJECT_MANIFEST="$ARTIFACTS/__windvale_invalid_project__.wvproj"
INVALID_PROJECT_MODULE="$ARTIFACTS/__windvale_invalid_project_output__.wvb"
INVALID_COMPOSITION_MODULE="$ARTIFACTS/__windvale_invalid_composition_output__.wvb"
MACHINE_CONTRACTS_MODULE="$ARTIFACTS/Machine-Contracts.wvb"
MACHINE_CONTRACTS_DEMO_MODULE="$ARTIFACTS/Machine-Contracts-Demo.wvb"
BYTE_ORDERING_MODULE="$ARTIFACTS/Byte-Ordering.wvb"
BYTE_ORDERING_DEMO_MODULE="$ARTIFACTS/Byte-Ordering-Demo.wvb"
DECIMAL_PARSING_MODULE="$ARTIFACTS/Decimal-Parsing.wvb"
DECIMAL_PARSING_DEMO_MODULE="$ARTIFACTS/Decimal-Parsing-Demo.wvb"
BYTE_CONSTRUCTION_MODULE="$ARTIFACTS/Byte-Construction.wvb"
BYTE_CONSTRUCTION_DEMO_MODULE="$ARTIFACTS/Byte-Construction-Demo.wvb"
NATIVE_STENCIL_MODULE="$ARTIFACTS/Native-Stencil-Core.wvb"
NATIVE_STENCIL_DEMO_MODULE="$ARTIFACTS/Native-Stencil-Demo.wvb"
NATIVE_STENCIL_BRIDGE_MODULE="$ARTIFACTS/Native-Stencil-Bridge.wvb"
NATIVE_PUBLICATION_MODULE="$ARTIFACTS/Native-Publication-Core.wvb"
NATIVE_PUBLICATION_BRIDGE_MODULE="$ARTIFACTS/Native-Publication-Bridge.wvb"
NATIVE_PUBLICATION_LIFETIME_MODULE="$ARTIFACTS/Native-Publication-Lifetime-Core.wvb"
NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE="$ARTIFACTS/Native-Publication-Lifetime-Bridge.wvb"
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

WINDOWS_APPLICATION_OUTPUT=$(dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" \
    --target windows-x64-console-v1 \
    -o "$SUM_WINDOWS_APPLICATION")
printf '%s\n' "$WINDOWS_APPLICATION_OUTPUT" | \
    grep -F 'Target: windows-x64-console-v1' >/dev/null
printf '%s\n' "$WINDOWS_APPLICATION_OUTPUT" | \
    grep -F 'SHA-256: 5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77' >/dev/null
WINDOWS_APPLICATION_HASH=$(sha256sum "$SUM_WINDOWS_APPLICATION" | awk '{print $1}')
WINDOWS_APPLICATION_BYTES=$(wc -c < "$SUM_WINDOWS_APPLICATION" | tr -d ' ')
if [ "$WINDOWS_APPLICATION_HASH" != '5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77' ] || \
   [ "$WINDOWS_APPLICATION_BYTES" != '5120' ]; then
    echo 'The Seed CLI Windows application identity is not canonical.' >&2
    exit 1
fi

LINUX_APPLICATION_OUTPUT=$(dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" \
    --target linux-x64-console-v1 \
    -o "$SUM_LINUX_APPLICATION")
printf '%s\n' "$LINUX_APPLICATION_OUTPUT" | \
    grep -F 'Target: linux-x64-console-v1' >/dev/null
printf '%s\n' "$LINUX_APPLICATION_OUTPUT" | \
    grep -F 'SHA-256: 8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4' >/dev/null
LINUX_APPLICATION_HASH=$(sha256sum "$SUM_LINUX_APPLICATION" | awk '{print $1}')
LINUX_APPLICATION_BYTES=$(wc -c < "$SUM_LINUX_APPLICATION" | tr -d ' ')
LINUX_APPLICATION_MODE=$(stat -c '%a' "$SUM_LINUX_APPLICATION")
if [ "$LINUX_APPLICATION_HASH" != '8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4' ] || \
   [ "$LINUX_APPLICATION_BYTES" != '8304' ] || \
   [ "$LINUX_APPLICATION_MODE" != '755' ]; then
    echo 'The Seed CLI Linux application identity or executable mode is not canonical.' >&2
    exit 1
fi
if "$SUM_LINUX_APPLICATION"; then
    LINUX_APPLICATION_EXIT=0
else
    LINUX_APPLICATION_EXIT=$?
fi
if [ "$LINUX_APPLICATION_EXIT" != '29' ]; then
    echo "The generated Linux application returned $LINUX_APPLICATION_EXIT instead of 29." >&2
    exit 1
fi

VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SUM_MODULE")
printf '%s\n' "$VERIFY_OUTPUT" | grep -F 'Verified: Sumˉdata' >/dev/null

INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$SUM_MODULE")
printf '%s\n' "$INSPECT_OUTPUT" | grep -F 'data.load.i32' >/dev/null

RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SUM_MODULE")
printf '%s\n' "$RUN_OUTPUT" | grep -F 'Result: 29' >/dev/null
if printf '%s\n' "$RUN_OUTPUT" | grep -E '^Function (instructions|record-fields|dynamic-bytes)=' >/dev/null; then
    echo 'The default run unexpectedly reported per-function profiling data.' >&2
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
if [ "$COMPOSITION_HASH" != 'a9250b544544cae8f4183d8db66a4391b619caa41001c86dd7142fb204b9d979' ]; then
    echo "The composed source module has an unexpected digest: $COMPOSITION_HASH" >&2
    exit 1
fi
COMPOSITION_RUN_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$COMPOSITION_MODULE")
printf '%s\n' "$COMPOSITION_RUN_OUTPUT" | grep -F 'Result: 42' >/dev/null
RECORD_FIELD_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$COMPOSITION_MODULE" --report-function-record-fields 2>&1)
printf '%s\n' "$RECORD_FIELD_REPORT_OUTPUT" | grep -F 'Result: 42' >/dev/null
printf '%s\n' "$RECORD_FIELD_REPORT_OUTPUT" | grep -F \
    'Function record-fields=2 index=2 name=Compositionˉmake' >/dev/null
if [ "$(printf '%s\n' "$RECORD_FIELD_REPORT_OUTPUT" | grep -c '^Function record-fields=')" -ne 1 ]; then
    echo 'The Seed CLI did not report deterministic per-function record construction pressure.' >&2
    exit 1
fi
dotnet "$TOOL_DLL" \
    compile "$COMPOSITION_ROOT" \
    --module "$COMPOSITION_LEAF" \
    --module "$COMPOSITION_MIDDLE" \
    -o "$COMPOSITION_REORDERED_MODULE"
cmp "$COMPOSITION_MODULE" "$COMPOSITION_REORDERED_MODULE"
COMPOSITION_PROJECT="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Demo.wvproj"
(cd "$ARTIFACTS" && dotnet "$TOOL_DLL" \
    build "$COMPOSITION_PROJECT" -o "$PROJECT_COMPOSITION_MODULE")
cmp "$COMPOSITION_MODULE" "$PROJECT_COMPOSITION_MODULE"
printf '%s\n' \
    'windvale-project 1' \
    'root "Missing.wv"' > "$INVALID_PROJECT_MANIFEST"
printf '\011\010\007' > "$INVALID_PROJECT_MODULE"
set +e
INVALID_PROJECT_OUTPUT=$(dotnet "$TOOL_DLL" \
    build "$INVALID_PROJECT_MANIFEST" -o "$INVALID_PROJECT_MODULE" 2>&1)
INVALID_PROJECT_EXIT=$?
set -e
if [ "$INVALID_PROJECT_EXIT" -ne 1 ]; then
    echo "Expected invalid project exit 1, found $INVALID_PROJECT_EXIT." >&2
    exit 1
fi
printf '%s\n' "$INVALID_PROJECT_OUTPUT" | grep -F 'WVP1004' >/dev/null
if [ "$(od -An -tx1 -v "$INVALID_PROJECT_MODULE" | tr -d ' \n')" != '090807' ]; then
    echo 'A rejected project build modified its existing output module.' >&2
    exit 1
fi
rm -f "$INVALID_PROJECT_MANIFEST" "$INVALID_PROJECT_MODULE"
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
if [ "$MACHINE_CONTRACTS_DEMO_HASH" != '68ea0056db52ca3a4f5bb2dc6071bab49da8db1bf33272c495c115cb40db3e66' ]; then
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
if [ "$BYTE_ORDERING_DEMO_HASH" != '10ca64b40a74cd23f801bc59d64ab271c03fbe8b5a59d2426781cd0bf9b817c2' ]; then
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
if [ "$DECIMAL_PARSING_DEMO_HASH" != '2a9789972a77e3fcf0fbacf686050e22d30f0bc0aab5a7e09f1d5620d8168ac8' ]; then
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
if [ "$BYTE_CONSTRUCTION_DEMO_HASH" != 'e12b9f36be719c4f448b074f8d40e19e2dc044908e85945759f024ae69335a1b' ]; then
    echo "The Foundation byte-construction demo has an unexpected digest: $BYTE_CONSTRUCTION_DEMO_HASH" >&2
    exit 1
fi
BYTE_CONSTRUCTION_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE")
printf '%s\n' "$BYTE_CONSTRUCTION_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
DYNAMIC_VALUE_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE" --report-function-dynamic-values 2>&1)
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F 'Result: 0' >/dev/null
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F \
    'Function dynamic-bytes=8388653 values=27 kind=bytes.concat index=0 name=Foundationˉbytesˉrepeat' >/dev/null
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F \
    'Function dynamic-bytes=15 values=4 kind=bytes.concat index=1 name=Foundationˉbytesˉreplace' >/dev/null
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F \
    'Function dynamic-bytes=4 values=4 kind=bytes.from_u8 index=0 name=Foundationˉbytesˉrepeat' >/dev/null
if [ "$(printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -c '^Function dynamic-bytes=')" -ne 3 ]; then
    echo 'The Seed CLI did not report deterministic per-function dynamic-value construction pressure.' >&2
    exit 1
fi
DYNAMIC_LIFETIME_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE" --report-dynamic-lifetime 2>&1)
printf '%s\n' "$DYNAMIC_LIFETIME_OUTPUT" | grep -F 'Result: 0' >/dev/null
printf '%s\n' "$DYNAMIC_LIFETIME_OUTPUT" | grep -F \
    'Dynamic lifetime constructed-bytes=8388672 constructed-values=35 peak-live-bytes=6291475 peak-live-values=5 peak-operation-bytes=6291475 peak-operation-values=5 retained-bytes=0 retained-values=0 kind=bytes.concat index=0 name=Foundationˉbytesˉrepeat' >/dev/null
DYNAMIC_ALLOCATOR_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE" --report-dynamic-allocator 2>&1)
printf '%s\n' "$DYNAMIC_ALLOCATOR_OUTPUT" | grep -F 'Result: 0' >/dev/null
printf '%s\n' "$DYNAMIC_ALLOCATOR_OUTPUT" | grep -F \
    'Dynamic allocator arena-bytes=16777216 header-bytes=16 alignment-bytes=16 allocations=35 reused=12 peak-payload-bytes=6291475 peak-charged-bytes=6291600 peak-blocks=5 maximum-addressed-bytes=8389040 peak-fragmentation-bytes=4194640 maximum-free-spans=3 failed=0 first-failure-payload-bytes=0 first-failure-charged-bytes=0 first-failure-largest-free-span-bytes=0 retained-blocks=0 retained-charged-bytes=0' >/dev/null

NATIVE_STENCIL_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Native-Stencil-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$NATIVE_STENCIL_SOURCE" -o "$NATIVE_STENCIL_MODULE"
NATIVE_STENCIL_HASH=$(sha256sum "$NATIVE_STENCIL_MODULE" | awk '{print $1}')
if [ "$NATIVE_STENCIL_HASH" != 'd40fc83c3288043c7af80a261e351066bf3507913b34371a9839014b51ed4b2f' ]; then
    echo "The Windvale native-stencil core has an unexpected digest: $NATIVE_STENCIL_HASH" >&2
    exit 1
fi
NATIVE_STENCIL_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$NATIVE_STENCIL_MODULE")
printf '%s\n' "$NATIVE_STENCIL_INSPECTION" | grep -F 'Nativeˉstencilˉresult' >/dev/null
printf '%s\n' "$NATIVE_STENCIL_INSPECTION" | grep -F 'Nativeˉstencilˉpatchˉkind' >/dev/null
printf '%s\n' "$NATIVE_STENCIL_INSPECTION" | grep -F 'Nativeˉstencilˉprocessˉargumentˉcount' >/dev/null
printf '%s\n' "$NATIVE_STENCIL_INSPECTION" | grep -F 'Nativeˉstencilˉprocessˉargument' >/dev/null
printf '%s\n' "$NATIVE_STENCIL_INSPECTION" | grep -F 'Exports (20)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Native-Stencil-Demo.wv" \
    --module "$NATIVE_STENCIL_SOURCE" \
    -o "$NATIVE_STENCIL_DEMO_MODULE"
NATIVE_STENCIL_DEMO_HASH=$(sha256sum "$NATIVE_STENCIL_DEMO_MODULE" | awk '{print $1}')
if [ "$NATIVE_STENCIL_DEMO_HASH" != '0bd2c8989e763c4d84463a197244607c56fa884896cecf9ca64bc995c8f86f6f' ]; then
    echo "The Windvale native-stencil demo has an unexpected digest: $NATIVE_STENCIL_DEMO_HASH" >&2
    exit 1
fi
NATIVE_STENCIL_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$NATIVE_STENCIL_DEMO_MODULE" --max-steps 20000000)
printf '%s\n' "$NATIVE_STENCIL_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

NATIVE_STENCIL_BRIDGE_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Native-Stencil-Bridge.wv"
NATIVE_STENCIL_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Stencil-Bridge.wvb"
dotnet "$TOOL_DLL" \
    compile "$NATIVE_STENCIL_BRIDGE_SOURCE" \
    --module "$NATIVE_STENCIL_SOURCE" \
    -o "$NATIVE_STENCIL_BRIDGE_MODULE"
NATIVE_STENCIL_BRIDGE_HASH=$(sha256sum "$NATIVE_STENCIL_BRIDGE_MODULE" | awk '{print $1}')
if [ "$NATIVE_STENCIL_BRIDGE_HASH" != 'fca2a0ba6c3ec864a2f77295f39326b1196a675dc6defd7a749c0d5541499770' ]; then
    echo "The Windvale native-stencil bridge has an unexpected digest: $NATIVE_STENCIL_BRIDGE_HASH" >&2
    exit 1
fi
cmp -s "$NATIVE_STENCIL_BRIDGE_MODULE" "$NATIVE_STENCIL_BRIDGE_RETAINED"
NATIVE_STENCIL_BRIDGE_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$NATIVE_STENCIL_BRIDGE_MODULE")
printf '%s\n' "$NATIVE_STENCIL_BRIDGE_INSPECTION" | grep -F 'Main() -> bytes' >/dev/null
printf '%s\n' "$NATIVE_STENCIL_BRIDGE_INSPECTION" | grep -F 'Exports (1)' >/dev/null

NATIVE_PUBLICATION_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$NATIVE_PUBLICATION_SOURCE" -o "$NATIVE_PUBLICATION_MODULE"
NATIVE_PUBLICATION_HASH=$(sha256sum "$NATIVE_PUBLICATION_MODULE" | awk '{print $1}')
if [ "$NATIVE_PUBLICATION_HASH" != 'f2c315c4c52099b8682396358563eef2eb9dceecf1feb84ce5bef5f8465bdeba' ]; then
    echo "The Windvale native-publication core has an unexpected digest: $NATIVE_PUBLICATION_HASH" >&2
    exit 1
fi
NATIVE_PUBLICATION_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$NATIVE_PUBLICATION_MODULE")
printf '%s\n' "$NATIVE_PUBLICATION_INSPECTION" | grep -F 'Profile: portable' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_INSPECTION" | grep -F 'Nativeˉpublicationˉresult' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_INSPECTION" | grep -F 'Nativeˉpublicationˉstatus' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_INSPECTION" | grep -F 'Nativeˉpublicationˉplan' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_INSPECTION" | grep -F 'Exports (8)' >/dev/null

NATIVE_PUBLICATION_BRIDGE_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Bridge.wv"
NATIVE_PUBLICATION_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Publication-Bridge.wvb"
dotnet "$TOOL_DLL" \
    compile "$NATIVE_PUBLICATION_BRIDGE_SOURCE" \
    --module "$NATIVE_PUBLICATION_SOURCE" \
    -o "$NATIVE_PUBLICATION_BRIDGE_MODULE"
NATIVE_PUBLICATION_BRIDGE_HASH=$(sha256sum "$NATIVE_PUBLICATION_BRIDGE_MODULE" | awk '{print $1}')
if [ "$NATIVE_PUBLICATION_BRIDGE_HASH" != '7b18c009c2d2c8ade970d784168376498eadc833f7ab149b713a2d6ae8e0dc81' ]; then
    echo "The Windvale native-publication bridge has an unexpected digest: $NATIVE_PUBLICATION_BRIDGE_HASH" >&2
    exit 1
fi
cmp -s "$NATIVE_PUBLICATION_BRIDGE_MODULE" "$NATIVE_PUBLICATION_BRIDGE_RETAINED"
NATIVE_PUBLICATION_BRIDGE_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$NATIVE_PUBLICATION_BRIDGE_MODULE")
printf '%s\n' "$NATIVE_PUBLICATION_BRIDGE_INSPECTION" | grep -F 'Profile: hosted' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_BRIDGE_INSPECTION" | grep -F 'Capabilities (1)' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_BRIDGE_INSPECTION" | grep -F 'file.read_bytes' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_BRIDGE_INSPECTION" | grep -F 'Main() -> bytes' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_BRIDGE_INSPECTION" | grep -F 'Exports (1)' >/dev/null

NATIVE_PUBLICATION_LIFETIME_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Lifetime-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$NATIVE_PUBLICATION_LIFETIME_SOURCE" -o "$NATIVE_PUBLICATION_LIFETIME_MODULE"
NATIVE_PUBLICATION_LIFETIME_HASH=$(sha256sum "$NATIVE_PUBLICATION_LIFETIME_MODULE" | awk '{print $1}')
if [ "$NATIVE_PUBLICATION_LIFETIME_HASH" != '52b1cb6dd0d7fa9d17c1cba50b527912876e4acf1cd9663846ce915b4c56aed5' ]; then
    echo "The Windvale native publication-lifetime core has an unexpected digest: $NATIVE_PUBLICATION_LIFETIME_HASH" >&2
    exit 1
fi
NATIVE_PUBLICATION_LIFETIME_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$NATIVE_PUBLICATION_LIFETIME_MODULE")
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_INSPECTION" | grep -F 'Profile: portable' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_INSPECTION" | grep -F 'Nativeˉpublicationˉlifetimeˉresult' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_INSPECTION" | grep -F 'Nativeˉpublicationˉlifetimeˉstatus' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_INSPECTION" | grep -F 'Nativeˉpublicationˉlifetimeˉplan' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_INSPECTION" | grep -F 'Exports (7)' >/dev/null

NATIVE_PUBLICATION_LIFETIME_BRIDGE_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv"
NATIVE_PUBLICATION_LIFETIME_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Publication-Lifetime-Bridge.wvb"
dotnet "$TOOL_DLL" \
    compile "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_SOURCE" \
    --module "$NATIVE_PUBLICATION_LIFETIME_SOURCE" \
    -o "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE"
NATIVE_PUBLICATION_LIFETIME_BRIDGE_HASH=$(sha256sum "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE" | awk '{print $1}')
if [ "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_HASH" != '9f7b3c331f4afb56d2e55d51cdea32c5b1536e6856f8da20ade1479e75682bcf' ]; then
    echo "The Windvale native publication-lifetime bridge has an unexpected digest: $NATIVE_PUBLICATION_LIFETIME_BRIDGE_HASH" >&2
    exit 1
fi
cmp -s "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE" "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_RETAINED"
NATIVE_PUBLICATION_LIFETIME_BRIDGE_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE")
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_INSPECTION" | grep -F 'Profile: hosted' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_INSPECTION" | grep -F 'Capabilities (1)' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_INSPECTION" | grep -F 'file.read_bytes' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_INSPECTION" | grep -F 'Main() -> bytes' >/dev/null
printf '%s\n' "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_INSPECTION" | grep -F 'Exports (1)' >/dev/null

SOURCE_LEXER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Lexer-Core.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_LEXER_MODULE"
SOURCE_LEXER_HASH=$(sha256sum "$SOURCE_LEXER_MODULE" | awk '{print $1}')
if [ "$SOURCE_LEXER_HASH" != 'e108cc3721092a114c8bab3b58224aef7e4fb63b8ed46c368cf341860c0a44f9' ]; then
    echo "The Windvale source lexer has an unexpected digest: $SOURCE_LEXER_HASH" >&2
    exit 1
fi
SOURCE_LEXER_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_LEXER_MODULE")
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Nominal types (6)' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Compilerˉsourceˉtoken' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Compilerˉtokenˉkind' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Compilerˉlexˉsourceˉbounded' >/dev/null
printf '%s\n' "$SOURCE_LEXER_INSPECTION" | grep -F 'Exports (17)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Lexer-Demo.wv" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_LEXER_DEMO_MODULE"
SOURCE_LEXER_DEMO_HASH=$(sha256sum "$SOURCE_LEXER_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_LEXER_DEMO_HASH" != 'b0ee43b2441448e0e719fc8e80902a5cffea162450ce10a704685e8fec6c6918' ]; then
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
if [ "$SOURCE_DECLARATION_PARSER_HASH" != '85d9d909c378a69223c3321b882ddf483eef19afc5042568da70a183bd8ed193' ]; then
    echo "The Windvale declaration parser has an unexpected digest: $SOURCE_DECLARATION_PARSER_HASH" >&2
    exit 1
fi
SOURCE_DECLARATION_PARSER_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_DECLARATION_PARSER_MODULE")
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Nominal types (14)' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉdeclaration' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉmoduleˉsummary' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Compilerˉparseˉnextˉdeclarationˉvalidated' >/dev/null
printf '%s\n' "$SOURCE_DECLARATION_PARSER_INSPECTION" | grep -F 'Exports (32)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Declaration-Parser-Demo.wv" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_DECLARATION_PARSER_DEMO_MODULE"
SOURCE_DECLARATION_PARSER_DEMO_HASH=$(sha256sum "$SOURCE_DECLARATION_PARSER_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_DECLARATION_PARSER_DEMO_HASH" != 'e3e5606b8a7fe63bd03bfb53da31980e1f6457980621b1d7db038c3a43b6e16d' ]; then
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
if [ "$SOURCE_DECLARATION_PARSER_TOOL_HASH" != '4f24f9f6b5b7bdbffec6d60952b33e8b9e9df8fe17ad5b255a3c8aa28b017729' ]; then
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
printf '%s\n' "$SOURCE_LEXER_DECLARATION_OUTPUT" | grep -F 'source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=17 tokens=6175 offset=51134' >/dev/null
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
printf '%s\n' "$SOURCE_PARSER_SELF_DECLARATION_OUTPUT" | grep -F 'source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=32 tokens=15098 offset=112327' >/dev/null
printf '%s\n' "$SOURCE_PARSER_SELF_DECLARATION_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_BODY_PARSER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Body-Parser.wv"
dotnet "$TOOL_DLL" \
    compile "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BODY_PARSER_MODULE"
SOURCE_BODY_PARSER_HASH=$(sha256sum "$SOURCE_BODY_PARSER_MODULE" | awk '{print $1}')
if [ "$SOURCE_BODY_PARSER_HASH" != 'a9fb34ab9d6fe7a8fd44c81f1ea03f890664c131a4f0a085959910379c3655a6' ]; then
    echo "The Windvale body parser has an unexpected digest: $SOURCE_BODY_PARSER_HASH" >&2
    exit 1
fi
SOURCE_BODY_PARSER_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_BODY_PARSER_MODULE")
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Nominal types (24)' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉexpression' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉsourceˉstatement' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉparseˉexpressionˉvalidated' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Compilerˉparseˉsourceˉbodies' >/dev/null
printf '%s\n' "$SOURCE_BODY_PARSER_INSPECTION" | grep -F 'Exports (47)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Body-Parser-Demo.wv" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_BODY_PARSER_DEMO_MODULE"
SOURCE_BODY_PARSER_DEMO_HASH=$(sha256sum "$SOURCE_BODY_PARSER_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_BODY_PARSER_DEMO_HASH" != '18eac99ee5f93a8d179dc23746a3d63b46797d3ab8c4cd55fd6792f5a9a5ae77' ]; then
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
if [ "$SOURCE_BODY_PARSER_TOOL_HASH" != '841e14f4a4dd209658d8d0e123dbfa8730d58718dc139c6702964420259e7cd5' ]; then
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
printf '%s\n' "$SOURCE_LEXER_BODY_OUTPUT" | grep -F 'source bodies status=Valid functions=17 top-level=118 statements=686 expression-nodes=1916 statement-depth=17 expression-depth=5 offset=51135' >/dev/null
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
printf '%s\n' "$SOURCE_DECLARATION_BODY_OUTPUT" | grep -F 'source bodies status=Valid functions=32 top-level=363 statements=917 expression-nodes=3593 statement-depth=12 expression-depth=5 offset=112328' >/dev/null
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
printf '%s\n' "$SOURCE_BODY_SELF_OUTPUT" | grep -F 'source bodies status=Valid functions=47 top-level=338 statements=811 expression-nodes=3576 statement-depth=7 expression-depth=3 offset=109506' >/dev/null
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
if [ "$SOURCE_SET_HASH" != '4b497ed318d685259dcff69f91da17bf10f5de5340aa42144c55eb87ea12ac74' ]; then
    echo "The Windvale source-set core has an unexpected digest: $SOURCE_SET_HASH" >&2
    exit 1
fi
SOURCE_SET_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_SET_MODULE")
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Nominal types (28)' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉsourceˉsetˉscan' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉsourceˉsetˉsummary' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉscanˉsourceˉset' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉset' >/dev/null
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Exports (10)' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$REPOSITORY_ROOT/Examples/Compiler/Source-Set-Demo.wv" \
    --module "$SOURCE_SET_SOURCE" \
    --module "$SOURCE_BODY_PARSER_SOURCE" \
    --module "$SOURCE_DECLARATION_PARSER_SOURCE" \
    --module "$SOURCE_LEXER_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$SOURCE_SET_DEMO_MODULE"
SOURCE_SET_DEMO_HASH=$(sha256sum "$SOURCE_SET_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_SET_DEMO_HASH" != '1977c8e4ade8519b9a9e75a526f402b07b579323220cf8d29120ae246497729b' ]; then
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
if [ "$SOURCE_SET_TOOL_HASH" != 'f7ddb4be4ab2a3b1cc2a33ed6ff995be0707de12a492f60c478208351dd74b09' ]; then
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
printf '%s\n' "$SOURCE_SET_SELF_OUTPUT" | grep -F 'source set status=Valid modules=5 source-bytes=290433 imports=6 records=17 enums=11 functions=107' >/dev/null
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
if [ "$SOURCE_GRAPH_HASH" != '574528635f818694fb72ba1fe1d4634cf0fddf4976b6733a1f96a9cf2dbd8cd0' ]; then
    echo "The Windvale source-graph core has an unexpected digest: $SOURCE_GRAPH_HASH" >&2
    exit 1
fi
SOURCE_GRAPH_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_GRAPH_MODULE")
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Nominal types (33)' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Compilerˉsourceˉgraphˉstatus' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Compilerˉsourceˉgraphˉsummary' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉgraph' >/dev/null
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Exports (12)' >/dev/null
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
if [ "$SOURCE_GRAPH_DEMO_HASH" != '63e72328ec5897695ac4c7b9c044a409068d726e294011f00ff4f72221a6087a' ]; then
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
if [ "$SOURCE_GRAPH_TOOL_HASH" != '697a803b57229ca4e5a7e66053f696f45f0b6f35c0c3bc6cfe87f47a6f3aa56b' ]; then
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
printf '%s\n' "$SOURCE_GRAPH_SELF_OUTPUT" | grep -F 'source graph status=Valid modules=7 imports=10 reachable=7' >/dev/null
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
if [ "$SOURCE_SYMBOLS_HASH" != '6619d6b2de2512efca21e08888042382a6e676d089b85ce7f13133399c11343d' ]; then
    echo "The Windvale source-symbol core has an unexpected digest: $SOURCE_SYMBOLS_HASH" >&2
    exit 1
fi
SOURCE_SYMBOLS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_SYMBOLS_MODULE")
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Nominal types (42)' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolˉstatus' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolˉsummary' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉsymbols' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Exports (65)' >/dev/null
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
if [ "$SOURCE_SYMBOLS_DEMO_HASH" != '5b70c55c0462bf76b0e0ded51fc5c712de1fb16108ff4ca01fd6aea512b80f8c' ]; then
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
if [ "$SOURCE_SYMBOLS_TOOL_HASH" != 'f4414d04c23e35d461b00916330b53dd0bea2f3a072a6ddc0ca2f15b18d516b4' ]; then
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
printf '%s\n' "$SOURCE_SYMBOLS_SELF_OUTPUT" | grep -F 'source symbols status=Valid modules=8 capabilities=0 data=0 records=28 enums=14 functions=186 fields=330 members=239 parameters=795 directory-bytes=5488 visibility-bytes=64' >/dev/null
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
if [ "$SOURCE_BINDINGS_HASH" != '205112ba67f9c3dca1f602d09b573f684035cbeec956d36755085f02723c12ca' ]; then
    echo "The Windvale source-binding core has an unexpected digest: $SOURCE_BINDINGS_HASH" >&2
    exit 1
fi
SOURCE_BINDINGS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_BINDINGS_MODULE")
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Nominal types (52)' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉsourceˉbindingˉstatus' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉsourceˉbindingˉsummary' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉbindings' >/dev/null
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Exports (59)' >/dev/null
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
if [ "$SOURCE_BINDINGS_DEMO_HASH" != '4a0e3ddca159f40ebaa4e7dea97f88b4c91f97e0a62c590bca44724c7ffa7a51' ]; then
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
if [ "$SOURCE_BINDINGS_TOOL_HASH" != 'f25c6c4ac04da77a3498587b62220376475680b46797017eb7855009bcd2995b' ]; then
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
printf '%s\n' "$SOURCE_BINDINGS_SELF_OUTPUT" | grep -F 'source bindings status=Valid modules=9 functions=245 parameters=1058 locals=1501 reads=12665 assignments=989 calls=2212 directory-bytes=94524' >/dev/null
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
if [ "$SOURCE_WIR_HASH" != '8414d674d7373f977199f96319c33af6df86f1cff7664aa2ad15374e8b75cd09' ]; then
    echo "The Windvale typed-WVIR core has an unexpected digest: $SOURCE_WIR_HASH" >&2
    exit 1
fi
SOURCE_WIR_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WIR_MODULE")
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉsourceˉwirˉoperation' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉsourceˉwirˉsummary' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉsourceˉwirˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉwir' >/dev/null
printf '%s\n' "$SOURCE_WIR_INSPECTION" | grep -F 'Exports (72)' >/dev/null
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
if [ "$SOURCE_WIR_DEMO_HASH" != '2f856568394b108404f4c6fb4135af93bbf35be6a33102c81cce9e9b1d73cd05' ]; then
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
if [ "$SOURCE_WIR_TOOL_HASH" != '729c477cb8d178984806ef1ad5e816debc7bcd919df2d78cf87e955dea754f62' ]; then
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
if [ "$SOURCE_WVB_HASH" != 'ba0480fcedebd09f6ae7cc2ec1469b366ae86ab21081b17455d0da2c559a93ce' ]; then
    echo "The Windvale WVB backend core has an unexpected digest: $SOURCE_WVB_HASH" >&2
    exit 1
fi
SOURCE_WVB_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WVB_MODULE")
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Compilerˉsourceˉwvbˉsummary' >/dev/null
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Compilerˉcompileˉsourceˉwvb' >/dev/null
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Exports (72)' >/dev/null
compile_source_wvb "$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Demo.wv" "$SOURCE_WVB_DEMO_MODULE"
SOURCE_WVB_DEMO_HASH=$(sha256sum "$SOURCE_WVB_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_DEMO_HASH" != 'c0408401d6a3290173acd7d50e114c6857c8714350c7a8a4296b3a7576fa61d4' ]; then
    echo "The Windvale WVB backend demo has an unexpected digest: $SOURCE_WVB_DEMO_HASH" >&2
    exit 1
fi
SOURCE_WVB_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_DEMO_MODULE" --max-steps 4000000000)
printf '%s\n' "$SOURCE_WVB_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
compile_source_wvb "$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Tool.wv" "$SOURCE_WVB_TOOL_MODULE"
SOURCE_WVB_TOOL_HASH=$(sha256sum "$SOURCE_WVB_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_TOOL_HASH" != 'c08f76e998e0280b7c2e3e801a9752f000825c874abeb86e88420c31444d63f9' ]; then
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
printf '%s\n' "$SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'source wvb status=Valid functions=9 code-bytes=627 module-bytes=1387' >/dev/null
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
if [ "$SOURCE_WVB_COMPOSITION_HASH" != '61fc1644b2952aa3dc0b4c30d3d1c1f43532bed89032ede32eee946027c85d85' ]; then
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
