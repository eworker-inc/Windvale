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
INCLUDE_EXTENDED=${INCLUDE_EXTENDED:-0}
TIMING_REPORT_PATH=${TIMING_REPORT_PATH:-}
TOOL_DLL="$REPOSITORY_ROOT/Tools/Windvale.Tool/bin/$CONFIGURATION/net10.0/windvale.dll"
NATIVE_SEED_FRONT_DOOR="$REPOSITORY_ROOT/Tools/Verify/Verify-Seed-Native-Front-Door.sh"
NATIVE_SEED_CONSOLE_AOT="$REPOSITORY_ROOT/Tools/Verify/Verify-Seed-Native-Console-Aot.sh"
TEST_PROJECT="$REPOSITORY_ROOT/Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj"
OS_TEST_PROJECT="$REPOSITORY_ROOT/Tests/Windvale.Os.Tests/Windvale.Os.Tests.csproj"
ARTIFACTS="$REPOSITORY_ROOT/artifacts"
mkdir -p "$ARTIFACTS"

case "$INCLUDE_EXTENDED" in
    0|1) ;;
    *)
        echo 'INCLUDE_EXTENDED must be 0 or 1.' >&2
        exit 64
        ;;
esac

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
        if [ "$INCLUDE_EXTENDED" != '0' ]; then
            echo 'INCLUDE_EXTENDED is available only with VERIFY_LEVEL=fast; standard and qualification already include extended tests.' >&2
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
    if [ "$INCLUDE_EXTENDED" != '1' ]; then
        set -- "$@" --exclude-extended
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
        --area runtime \
        --exclude-extended
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
    if [ "$INCLUDE_EXTENDED" = '1' ]; then
        echo "Windvale Seed fast verification passed including extended tests matching $SELECTION_DESCRIPTION."
    else
        echo "Windvale Seed fast verification passed for regular tests matching $SELECTION_DESCRIPTION."
    fi
    exit 0
fi

dotnet run \
    --project "$OS_TEST_PROJECT" \
    --configuration "$CONFIGURATION" \
    --no-build

if [ "$VERIFY_LEVEL" = 'development' ]; then
    echo 'Windvale Seed development verification passed for every regular in-process test.'
    echo 'Extended integration contracts and the golden cross-host contract were not executed.'
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
PROJECT_COMPOSITION_MODULE="$ARTIFACTS/Module-Composition-Demo-Project.wvb"
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
NATIVE_UTF8_CORE_MODULE="$ARTIFACTS/Native-X64-Utf8-Service.wvb"
NATIVE_UTF8_BRIDGE_MODULE="$ARTIFACTS/Native-X64-Utf8-Service-Bridge.wvb"
NATIVE_INTEGER_FORMAT_CORE_MODULE="$ARTIFACTS/Native-X64-Integer-Format-Services.wvb"
NATIVE_INTEGER_FORMAT_BRIDGE_MODULE="$ARTIFACTS/Native-X64-Integer-Format-Services-Bridge.wvb"
NATIVE_SERVICE_CODE_BUILDER_MODULE="$ARTIFACTS/Native-X64-Service-Code-Builder.wvb"
NATIVE_WINDOWS_OUTPUT_CORE_MODULE="$ARTIFACTS/Native-X64-Output-Service-Windows.wvb"
NATIVE_LINUX_OUTPUT_CORE_MODULE="$ARTIFACTS/Native-X64-Output-Service-Linux.wvb"
NATIVE_OUTPUT_BRIDGE_MODULE="$ARTIFACTS/Native-X64-Output-Services-Bridge.wvb"
NATIVE_FILE_OUTPUT_CODE_MODULE="$ARTIFACTS/Native-X64-File-Output-Service-Code.wvb"
NATIVE_WINDOWS_FILE_OUTPUT_CORE_MODULE="$ARTIFACTS/Native-X64-File-Output-Service-Windows.wvb"
NATIVE_LINUX_FILE_OUTPUT_CORE_MODULE="$ARTIFACTS/Native-X64-File-Output-Service-Linux.wvb"
NATIVE_FILE_OUTPUT_BRIDGE_MODULE="$ARTIFACTS/Native-X64-File-Output-Services-Bridge.wvb"
NATIVE_FILE_INPUT_CODE_MODULE="$ARTIFACTS/Native-X64-File-Input-Service-Code.wvb"
NATIVE_WINDOWS_FILE_INPUT_CORE_MODULE="$ARTIFACTS/Native-X64-File-Input-Service-Windows.wvb"
NATIVE_LINUX_FILE_INPUT_CORE_MODULE="$ARTIFACTS/Native-X64-File-Input-Service-Linux.wvb"
NATIVE_FILE_INPUT_BRIDGE_MODULE="$ARTIFACTS/Native-X64-File-Input-Services-Bridge.wvb"
NATIVE_TEXT_CONCAT_BRIDGE_MODULE="$ARTIFACTS/Native-X64-Text-Concat-Service-Bridge.wvb"
NATIVE_TEXT_QUOTE_BRIDGE_MODULE="$ARTIFACTS/Native-X64-Text-Quote-Service-Bridge.wvb"
NATIVE_ENUM_NAME_BRIDGE_MODULE="$ARTIFACTS/Native-X64-Enum-Name-Service-Bridge.wvb"
NATIVE_ENUM_METADATA_BRIDGE_MODULE="$ARTIFACTS/Native-Enum-Metadata-Bridge.wvb"
NATIVE_PUBLICATION_BRIDGE_MODULE="$ARTIFACTS/Native-Publication-Bridge.wvb"
NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_MODULE="$ARTIFACTS/Native-Service-Bundle-Materialization-Bridge.wvb"
NATIVE_OUTPUT_TABLE_BRIDGE_MODULE="$ARTIFACTS/Native-Output-Table-Bridge.wvb"
NATIVE_FILE_OUTPUT_TABLE_BRIDGE_MODULE="$ARTIFACTS/Native-File-Output-Table-Bridge.wvb"
NATIVE_FILE_INPUT_TABLE_BRIDGE_MODULE="$ARTIFACTS/Native-File-Input-Table-Bridge.wvb"
NATIVE_SERVICE_TABLE_BRIDGE_MODULE="$ARTIFACTS/Native-Service-Table-Bridge.wvb"
NATIVE_EXECUTION_CONTEXT_BRIDGE_MODULE="$ARTIFACTS/Native-Execution-Context-Bridge.wvb"
NATIVE_ARGUMENT_TABLE_BRIDGE_MODULE="$ARTIFACTS/Native-Argument-Table-Bridge.wvb"
NATIVE_ENTRY_BRIDGE_BRIDGE_MODULE="$ARTIFACTS/Native-Entry-Bridge-Bridge.wvb"
NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_MODULE="$ARTIFACTS/Native-Byte-Result-Admission-Bridge.wvb"
NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE="$ARTIFACTS/Native-Hosted-Tool-Metadata-Construction-Bridge.wvb"
NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE="$ARTIFACTS/Native-Hosted-Startup-Instantiation.wvb"
NATIVE_HOSTED_CONTAINER_PLAN_MODULE="$ARTIFACTS/Native-Hosted-Container-Construction.wvb"
NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE="$ARTIFACTS/Native-Hosted-Container-Windows.wvb"
NATIVE_HOSTED_CONTAINER_LINUX_MODULE="$ARTIFACTS/Native-Hosted-Container-Linux.wvb"
NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE="$ARTIFACTS/Native-Hosted-Container-Segmentation.wvb"
NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_MODULE="$ARTIFACTS/Native-Hosted-Tool-Runtime-Header-Bridge.wvb"
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

NATIVE_SEED_OUTPUT=$("$NATIVE_SEED_FRONT_DOOR" "$ARTIFACTS")
if [ "$NATIVE_SEED_OUTPUT" != 'native Seed front-door verification status=Complete artifacts=79 cases=132' ]; then
    echo 'The native Seed front-door verification failed.' >&2
    exit 1
fi

NATIVE_SEED_CONSOLE_AOT_OUTPUT=$("$NATIVE_SEED_CONSOLE_AOT" "$ARTIFACTS")
if [ "$NATIVE_SEED_CONSOLE_AOT_OUTPUT" != \
    'native Seed console AOT verification status=Complete artifacts=2 cases=1' ]; then
    echo 'The native Seed console AOT verification failed.' >&2
    exit 1
fi

FUNCTION_STEP_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" run "$SUM_MODULE" --report-function-steps 2>&1)
printf '%s\n' "$FUNCTION_STEP_REPORT_OUTPUT" | grep -F 'Result: 29' >/dev/null
printf '%s\n' "$FUNCTION_STEP_REPORT_OUTPUT" | grep -F 'Function instructions=163 index=1 name=Main' >/dev/null
printf '%s\n' "$FUNCTION_STEP_REPORT_OUTPUT" | grep -F 'Function instructions=40 index=0 name=Add' >/dev/null

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

COMPOSITION_ROOT="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Demo.wv"
COMPOSITION_MIDDLE="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Middle.wv"
COMPOSITION_LEAF="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Leaf.wv"
dotnet "$TOOL_DLL" \
    compile "$COMPOSITION_ROOT" \
    --module "$COMPOSITION_MIDDLE" \
    --module "$COMPOSITION_LEAF" \
    -o "$COMPOSITION_MODULE"
COMPOSITION_HASH=$(sha256sum "$COMPOSITION_MODULE" | awk '{print $1}')
if [ "$COMPOSITION_HASH" != '030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607' ]; then
    echo "The composed source module has an unexpected digest: $COMPOSITION_HASH" >&2
    exit 1
fi
RECORD_FIELD_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$COMPOSITION_MODULE" --report-function-record-fields 2>&1)
printf '%s\n' "$RECORD_FIELD_REPORT_OUTPUT" | grep -F 'Result: 42' >/dev/null
printf '%s\n' "$RECORD_FIELD_REPORT_OUTPUT" | grep -F \
    'Function record-fields=2 index=1 name=__WvM1F0' >/dev/null
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
cmp "$COMPOSITION_MODULE" "$PROJECT_COMPOSITION_MODULE"
rm -f "$INVALID_COMPOSITION_MODULE"

MACHINE_CONTRACTS_SOURCE="$REPOSITORY_ROOT/Foundation/Machine-Contracts.wv"
BYTE_ORDERING_SOURCE="$REPOSITORY_ROOT/Foundation/Byte-Ordering.wv"
SHA256_SOURCE="$REPOSITORY_ROOT/Foundation/Sha256.wv"
DECIMAL_PARSING_SOURCE="$REPOSITORY_ROOT/Foundation/Decimal-Parsing.wv"
BYTE_CONSTRUCTION_SOURCE="$REPOSITORY_ROOT/Foundation/Byte-Construction.wv"
BYTE_CONSTRUCTION_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE")
printf '%s\n' "$BYTE_CONSTRUCTION_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
DYNAMIC_VALUE_REPORT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE" --report-function-dynamic-values 2>&1)
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F 'Result: 0' >/dev/null
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F \
    'Function dynamic-bytes=8388653 values=27 kind=bytes.concat index=1 name=__WvM1F0' >/dev/null
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F \
    'Function dynamic-bytes=15 values=4 kind=bytes.concat index=2 name=__WvM1F1' >/dev/null
printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -F \
    'Function dynamic-bytes=4 values=4 kind=bytes.from_u8 index=1 name=__WvM1F0' >/dev/null
if [ "$(printf '%s\n' "$DYNAMIC_VALUE_REPORT_OUTPUT" | grep -c '^Function dynamic-bytes=')" -ne 3 ]; then
    echo 'The Seed CLI did not report deterministic per-function dynamic-value construction pressure.' >&2
    exit 1
fi
DYNAMIC_LIFETIME_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE" --report-dynamic-lifetime 2>&1)
printf '%s\n' "$DYNAMIC_LIFETIME_OUTPUT" | grep -F 'Result: 0' >/dev/null
printf '%s\n' "$DYNAMIC_LIFETIME_OUTPUT" | grep -F \
    'Dynamic lifetime constructed-bytes=8388672 constructed-values=35 peak-live-bytes=6291475 peak-live-values=5 peak-operation-bytes=6291475 peak-operation-values=5 retained-bytes=0 retained-values=0 kind=bytes.concat index=1 name=__WvM1F0' >/dev/null
DYNAMIC_ALLOCATOR_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE" --report-dynamic-allocator 2>&1)
printf '%s\n' "$DYNAMIC_ALLOCATOR_OUTPUT" | grep -F 'Result: 0' >/dev/null
printf '%s\n' "$DYNAMIC_ALLOCATOR_OUTPUT" | grep -F \
    'Dynamic allocator arena-bytes=16777216 header-bytes=16 alignment-bytes=16 allocations=35 reused=12 peak-payload-bytes=6291475 peak-charged-bytes=6291600 peak-blocks=5 maximum-addressed-bytes=8389040 peak-fragmentation-bytes=4194640 maximum-free-spans=3 failed=0 first-failure-payload-bytes=0 first-failure-charged-bytes=0 first-failure-largest-free-span-bytes=0 retained-blocks=0 retained-charged-bytes=0' >/dev/null

NATIVE_STENCIL_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$NATIVE_STENCIL_DEMO_MODULE" --max-steps 20000000)
printf '%s\n' "$NATIVE_STENCIL_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

NATIVE_STENCIL_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Stencil-Bridge.wvb"
NATIVE_ARGUMENT_COUNT_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Argument-Count-Service.bin"
NATIVE_ARGUMENT_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Argument-Service.bin"
cmp -s "$NATIVE_STENCIL_BRIDGE_MODULE" "$NATIVE_STENCIL_BRIDGE_RETAINED"
NATIVE_ARGUMENT_COUNT_LEAF_HASH=$(sha256sum "$NATIVE_ARGUMENT_COUNT_LEAF_RETAINED" | awk '{print $1}')
if [ "$NATIVE_ARGUMENT_COUNT_LEAF_HASH" != '2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829' ] ||
    [ "$(wc -c < "$NATIVE_ARGUMENT_COUNT_LEAF_RETAINED")" -ne 5 ]; then
    echo "The retained Windvale process-argument-count leaf has an unexpected identity: $NATIVE_ARGUMENT_COUNT_LEAF_HASH" >&2
    exit 1
fi
NATIVE_ARGUMENT_LEAF_HASH=$(sha256sum "$NATIVE_ARGUMENT_LEAF_RETAINED" | awk '{print $1}')
if [ "$NATIVE_ARGUMENT_LEAF_HASH" != '2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1' ] ||
    [ "$(wc -c < "$NATIVE_ARGUMENT_LEAF_RETAINED")" -ne 70 ]; then
    echo "The retained Windvale process-argument leaf has an unexpected identity: $NATIVE_ARGUMENT_LEAF_HASH" >&2
    exit 1
fi
NATIVE_UTF8_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service-Bridge.wvb"
NATIVE_UTF8_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service.bin"
cmp -s "$NATIVE_UTF8_BRIDGE_MODULE" "$NATIVE_UTF8_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_UTF8_LEAF_RETAINED" | awk '{print $1}')" != '4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf' ] ||
    [ "$(wc -c < "$NATIVE_UTF8_LEAF_RETAINED")" -ne 800 ]; then
    echo 'The retained Windvale native UTF-8 service leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_INTEGER_FORMAT_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Integer-Format-Services-Bridge.wvb"
NATIVE_I32_FORMAT_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-I32-Format-Service.bin"
NATIVE_U32_FORMAT_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-U32-Format-Service.bin"
cmp -s "$NATIVE_INTEGER_FORMAT_BRIDGE_MODULE" "$NATIVE_INTEGER_FORMAT_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_I32_FORMAT_LEAF_RETAINED" | awk '{print $1}')" != 'c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e' ] ||
    [ "$(wc -c < "$NATIVE_I32_FORMAT_LEAF_RETAINED")" -ne 225 ] ||
    [ "$(sha256sum "$NATIVE_U32_FORMAT_LEAF_RETAINED" | awk '{print $1}')" != 'b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43' ] ||
    [ "$(wc -c < "$NATIVE_U32_FORMAT_LEAF_RETAINED")" -ne 191 ]; then
    echo 'The retained Windvale native integer-format leaves have unexpected exact identities.' >&2
    exit 1
fi
NATIVE_OUTPUT_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Output-Services-Bridge.wvb"
NATIVE_WINDOWS_CONSOLE_OUTPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Windows-Console-Output-Service.bin"
NATIVE_WINDOWS_DIAGNOSTIC_OUTPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Windows-Diagnostic-Output-Service.bin"
NATIVE_LINUX_CONSOLE_OUTPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Linux-Console-Output-Service.bin"
NATIVE_LINUX_DIAGNOSTIC_OUTPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Linux-Diagnostic-Output-Service.bin"
cmp -s "$NATIVE_OUTPUT_BRIDGE_MODULE" "$NATIVE_OUTPUT_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_WINDOWS_CONSOLE_OUTPUT_LEAF" | awk '{print $1}')" != '10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48' ] ||
    [ "$(wc -c < "$NATIVE_WINDOWS_CONSOLE_OUTPUT_LEAF")" -ne 258 ] ||
    [ "$(sha256sum "$NATIVE_WINDOWS_DIAGNOSTIC_OUTPUT_LEAF" | awk '{print $1}')" != '1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2' ] ||
    [ "$(wc -c < "$NATIVE_WINDOWS_DIAGNOSTIC_OUTPUT_LEAF")" -ne 258 ] ||
    [ "$(sha256sum "$NATIVE_LINUX_CONSOLE_OUTPUT_LEAF" | awk '{print $1}')" != 'c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226' ] ||
    [ "$(wc -c < "$NATIVE_LINUX_CONSOLE_OUTPUT_LEAF")" -ne 213 ] ||
    [ "$(sha256sum "$NATIVE_LINUX_DIAGNOSTIC_OUTPUT_LEAF" | awk '{print $1}')" != '1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe' ] ||
    [ "$(wc -c < "$NATIVE_LINUX_DIAGNOSTIC_OUTPUT_LEAF")" -ne 213 ]; then
    echo 'A retained Windvale native output leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_FILE_OUTPUT_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-File-Output-Services-Bridge.wvb"
NATIVE_WINDOWS_FILE_OUTPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Output-Service.bin"
NATIVE_LINUX_FILE_OUTPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Linux-File-Output-Service.bin"
cmp -s "$NATIVE_FILE_OUTPUT_BRIDGE_MODULE" "$NATIVE_FILE_OUTPUT_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_WINDOWS_FILE_OUTPUT_LEAF" | awk '{print $1}')" != 'a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1' ] ||
    [ "$(wc -c < "$NATIVE_WINDOWS_FILE_OUTPUT_LEAF")" -ne 787 ] ||
    [ "$(sha256sum "$NATIVE_LINUX_FILE_OUTPUT_LEAF" | awk '{print $1}')" != 'fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422' ] ||
    [ "$(wc -c < "$NATIVE_LINUX_FILE_OUTPUT_LEAF")" -ne 823 ]; then
    echo 'A retained Windvale native file-output leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_FILE_INPUT_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-File-Input-Services-Bridge.wvb"
NATIVE_WINDOWS_FILE_INPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Input-Service.bin"
NATIVE_LINUX_FILE_INPUT_LEAF="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Linux-File-Input-Service.bin"
cmp -s "$NATIVE_FILE_INPUT_BRIDGE_MODULE" "$NATIVE_FILE_INPUT_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_WINDOWS_FILE_INPUT_LEAF" | awk '{print $1}')" != '3e2fd284d4991d0f713301514d3fbf6af8ec84af7bd7289698c08a41d434c52d' ] ||
    [ "$(wc -c < "$NATIVE_WINDOWS_FILE_INPUT_LEAF")" -ne 1218 ] ||
    [ "$(sha256sum "$NATIVE_LINUX_FILE_INPUT_LEAF" | awk '{print $1}')" != 'cbd78340641fa02589d0d96b73d233a67f9404ab76c3df2b1346b2e31ca43701' ] ||
    [ "$(wc -c < "$NATIVE_LINUX_FILE_INPUT_LEAF")" -ne 996 ]; then
    echo 'A retained Windvale native file-input leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_TEXT_CONCAT_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Text-Concat-Service-Bridge.wvb"
NATIVE_TEXT_CONCAT_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Text-Concat-Service.bin"
cmp -s "$NATIVE_TEXT_CONCAT_BRIDGE_MODULE" "$NATIVE_TEXT_CONCAT_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_TEXT_CONCAT_LEAF_RETAINED" | awk '{print $1}')" != '75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0' ] ||
    [ "$(wc -c < "$NATIVE_TEXT_CONCAT_LEAF_RETAINED")" -ne 249 ]; then
    echo 'The retained Windvale native text-concatenation leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_TEXT_QUOTE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Text-Quote-Service-Bridge.wvb"
NATIVE_TEXT_QUOTE_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Text-Quote-Service.bin"
cmp -s "$NATIVE_TEXT_QUOTE_BRIDGE_MODULE" "$NATIVE_TEXT_QUOTE_BRIDGE_RETAINED"
if [ "$(sha256sum "$NATIVE_TEXT_QUOTE_LEAF_RETAINED" | awk '{print $1}')" != '4f334af9b6349437d36fd703edb6b5882416f033fae47906a40a4bafdc083bb7' ] ||
    [ "$(wc -c < "$NATIVE_TEXT_QUOTE_LEAF_RETAINED")" -ne 1165 ]; then
    echo 'The retained Windvale native text-quote leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_ENUM_NAME_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Enum-Name-Service-Bridge.wvb"
NATIVE_ENUM_NAME_LEAF_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-X64-Enum-Name-Service.bin"
cmp -s "$NATIVE_ENUM_NAME_BRIDGE_MODULE" "$NATIVE_ENUM_NAME_BRIDGE_RETAINED"
NATIVE_ENUM_NAME_LEAF_RETAINED_HASH=$(sha256sum "$NATIVE_ENUM_NAME_LEAF_RETAINED" | awk '{print $1}')
if [ "$NATIVE_ENUM_NAME_LEAF_RETAINED_HASH" != 'fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a' ] ||
    [ "$(wc -c < "$NATIVE_ENUM_NAME_LEAF_RETAINED")" -ne 323 ]; then
    echo 'The retained Windvale native enum-name leaf has an unexpected exact identity.' >&2
    exit 1
fi
NATIVE_ENUM_METADATA_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Enum-Metadata-Bridge.wvb"
NATIVE_ENUM_METADATA_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Enum-Metadata-Bridge.wvnf"
cmp -s "$NATIVE_ENUM_METADATA_BRIDGE_MODULE" "$NATIVE_ENUM_METADATA_BRIDGE_RETAINED"
NATIVE_ENUM_METADATA_ARTIFACT_HASH=$(sha256sum "$NATIVE_ENUM_METADATA_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_ENUM_METADATA_ARTIFACT_HASH" != '004db29841eeaf5a448ec67c438a820832ed4af3ede0a8ae1b1d672565ea0999' ] ||
    [ "$(wc -c < "$NATIVE_ENUM_METADATA_ARTIFACT_RETAINED")" -ne 137964 ]; then
    echo "The retained Windvale native enum-metadata fragment has an unexpected identity: $NATIVE_ENUM_METADATA_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_PUBLICATION_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Publication-Bridge.wvb"
NATIVE_PUBLICATION_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Publication-Bridge.wvnf"
cmp -s "$NATIVE_PUBLICATION_BRIDGE_MODULE" "$NATIVE_PUBLICATION_BRIDGE_RETAINED"
NATIVE_PUBLICATION_ARTIFACT_HASH=$(sha256sum "$NATIVE_PUBLICATION_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_PUBLICATION_ARTIFACT_HASH" != '9deeb8c4ab8f080cbc187036e0b015932379956930ec9cd1b7f51f7d1daa1f47' ] ||
    [ "$(wc -c < "$NATIVE_PUBLICATION_ARTIFACT_RETAINED")" -ne 61583 ]; then
    echo "The retained Windvale native-publication fragment has an unexpected identity: $NATIVE_PUBLICATION_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Service-Bundle-Materialization-Bridge.wvb"
NATIVE_SERVICE_BUNDLE_MATERIALIZATION_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Service-Bundle-Materialization-Bridge.wvnf"
cmp -s "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_MODULE" "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_RETAINED"
NATIVE_SERVICE_BUNDLE_MATERIALIZATION_ARTIFACT_HASH=$(sha256sum "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_ARTIFACT_HASH" != 'd0b12e426e891f6ee78209ab817dde7c547c0f68541750d39dd665607434e7a9' ] ||
    [ "$(wc -c < "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_ARTIFACT_RETAINED")" -ne 179452 ]; then
    echo "The retained Windvale service-bundle materialization fragment has an unexpected identity: $NATIVE_SERVICE_BUNDLE_MATERIALIZATION_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_OUTPUT_TABLE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Output-Table-Bridge.wvb"
NATIVE_OUTPUT_TABLE_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Output-Table-Bridge.wvnf"
cmp -s "$NATIVE_OUTPUT_TABLE_BRIDGE_MODULE" "$NATIVE_OUTPUT_TABLE_BRIDGE_RETAINED"
NATIVE_OUTPUT_TABLE_ARTIFACT_HASH=$(sha256sum "$NATIVE_OUTPUT_TABLE_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_OUTPUT_TABLE_ARTIFACT_HASH" != 'f444e80b2afbaaee251892ab7a7a6a879b3e5cffcbf029b0fc382b64bef97afb' ] ||
    [ "$(wc -c < "$NATIVE_OUTPUT_TABLE_ARTIFACT_RETAINED")" -ne 50493 ]; then
    echo "The retained Windvale native output-table fragment has an unexpected identity: $NATIVE_OUTPUT_TABLE_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_FILE_OUTPUT_TABLE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-File-Output-Table-Bridge.wvb"
NATIVE_FILE_OUTPUT_TABLE_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-File-Output-Table-Bridge.wvnf"
cmp -s "$NATIVE_FILE_OUTPUT_TABLE_BRIDGE_MODULE" "$NATIVE_FILE_OUTPUT_TABLE_BRIDGE_RETAINED"
NATIVE_FILE_OUTPUT_TABLE_ARTIFACT_HASH=$(sha256sum "$NATIVE_FILE_OUTPUT_TABLE_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_FILE_OUTPUT_TABLE_ARTIFACT_HASH" != '9333d4573b87b829e6e577d8a27c937bf2fb433a93d4a4b11b783b372d31d08a' ] ||
    [ "$(wc -c < "$NATIVE_FILE_OUTPUT_TABLE_ARTIFACT_RETAINED")" -ne 42302 ]; then
    echo "The retained Windvale native file-output-table fragment has an unexpected identity: $NATIVE_FILE_OUTPUT_TABLE_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_FILE_INPUT_TABLE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-File-Input-Table-Bridge.wvb"
NATIVE_FILE_INPUT_TABLE_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-File-Input-Table-Bridge.wvnf"
cmp -s "$NATIVE_FILE_INPUT_TABLE_BRIDGE_MODULE" "$NATIVE_FILE_INPUT_TABLE_BRIDGE_RETAINED"
NATIVE_FILE_INPUT_TABLE_ARTIFACT_HASH=$(sha256sum "$NATIVE_FILE_INPUT_TABLE_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_FILE_INPUT_TABLE_ARTIFACT_HASH" != '378240d8f8770a4707d7f2ae86daae24036fc2eb9fd273d5ab737c9c03e3e70d' ] ||
    [ "$(wc -c < "$NATIVE_FILE_INPUT_TABLE_ARTIFACT_RETAINED")" -ne 52334 ]; then
    echo "The retained Windvale native file-input-table fragment has an unexpected identity: $NATIVE_FILE_INPUT_TABLE_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_SERVICE_TABLE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Service-Table-Bridge.wvb"
NATIVE_SERVICE_TABLE_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Service-Table-Bridge.wvnf"
cmp -s "$NATIVE_SERVICE_TABLE_BRIDGE_MODULE" "$NATIVE_SERVICE_TABLE_BRIDGE_RETAINED"
NATIVE_SERVICE_TABLE_ARTIFACT_HASH=$(sha256sum "$NATIVE_SERVICE_TABLE_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_SERVICE_TABLE_ARTIFACT_HASH" != 'e1b838652150999d13b84cd6f1c527b4e82923190530f707ef8d163d39a1f58e' ] ||
    [ "$(wc -c < "$NATIVE_SERVICE_TABLE_ARTIFACT_RETAINED")" -ne 34830 ]; then
    echo "The retained Windvale native service-table fragment has an unexpected identity: $NATIVE_SERVICE_TABLE_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_EXECUTION_CONTEXT_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Execution-Context-Bridge.wvb"
NATIVE_EXECUTION_CONTEXT_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Execution-Context-Bridge.wvnf"
cmp -s "$NATIVE_EXECUTION_CONTEXT_BRIDGE_MODULE" "$NATIVE_EXECUTION_CONTEXT_BRIDGE_RETAINED"
NATIVE_EXECUTION_CONTEXT_ARTIFACT_HASH=$(sha256sum "$NATIVE_EXECUTION_CONTEXT_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_EXECUTION_CONTEXT_ARTIFACT_HASH" != 'acdfc7d71b5fc2f0c1cfd76242fddc59db2563a4026ac286313711f0e2eb05de' ] ||
    [ "$(wc -c < "$NATIVE_EXECUTION_CONTEXT_ARTIFACT_RETAINED")" -ne 58363 ]; then
    echo "The retained Windvale native execution-context fragment has an unexpected identity: $NATIVE_EXECUTION_CONTEXT_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_ARGUMENT_TABLE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Argument-Table-Bridge.wvb"
NATIVE_ARGUMENT_TABLE_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Argument-Table-Bridge.wvnf"
cmp -s "$NATIVE_ARGUMENT_TABLE_BRIDGE_MODULE" "$NATIVE_ARGUMENT_TABLE_BRIDGE_RETAINED"
NATIVE_ARGUMENT_TABLE_ARTIFACT_HASH=$(sha256sum "$NATIVE_ARGUMENT_TABLE_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_ARGUMENT_TABLE_ARTIFACT_HASH" != '4a4cc1d6171126a821c1f96de11c4ffcb78ea83e98d06d5e0802e5921e9062d8' ] ||
    [ "$(wc -c < "$NATIVE_ARGUMENT_TABLE_ARTIFACT_RETAINED")" -ne 44775 ]; then
    echo "The retained Windvale native argument-table fragment has an unexpected identity: $NATIVE_ARGUMENT_TABLE_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_ENTRY_BRIDGE_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Entry-Bridge-Bridge.wvb"
NATIVE_ENTRY_BRIDGE_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Entry-Bridge-Bridge.wvnf"
cmp -s "$NATIVE_ENTRY_BRIDGE_BRIDGE_MODULE" "$NATIVE_ENTRY_BRIDGE_BRIDGE_RETAINED"
NATIVE_ENTRY_BRIDGE_ARTIFACT_HASH=$(sha256sum "$NATIVE_ENTRY_BRIDGE_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_ENTRY_BRIDGE_ARTIFACT_HASH" != '2abde6462aa470f4037aa87ae486f16f2a106932d3022344e85fa5763d44623b' ] ||
    [ "$(wc -c < "$NATIVE_ENTRY_BRIDGE_ARTIFACT_RETAINED")" -ne 37374 ]; then
    echo "The retained Windvale native entry-bridge fragment has an unexpected identity: $NATIVE_ENTRY_BRIDGE_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Byte-Result-Admission-Bridge.wvb"
NATIVE_BYTE_RESULT_ADMISSION_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Byte-Result-Admission-Bridge.wvnf"
cmp -s "$NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_MODULE" "$NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_RETAINED"
NATIVE_BYTE_RESULT_ADMISSION_ARTIFACT_HASH=$(sha256sum "$NATIVE_BYTE_RESULT_ADMISSION_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_BYTE_RESULT_ADMISSION_ARTIFACT_HASH" != '35c29fa9bbc41a00e8797f7812eb1bbf0f95c7f07b96227ca666cc5bf8fd38c2' ] ||
    [ "$(wc -c < "$NATIVE_BYTE_RESULT_ADMISSION_ARTIFACT_RETAINED")" -ne 68608 ]; then
    echo "The retained Windvale native byte-result admission fragment has an unexpected identity: $NATIVE_BYTE_RESULT_ADMISSION_ARTIFACT_HASH" >&2
    exit 1
fi

NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Hosted-Tool-Runtime-Header-Bridge.wvb"
NATIVE_HOSTED_TOOL_RUNTIME_HEADER_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Hosted-Tool-Runtime-Header-Bridge.wvnf"
NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Hosted-Tool-Metadata-Construction-Bridge.wvb"
NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Hosted-Tool-Metadata-Construction-Bridge.wvnf"
cmp -s "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE" "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_RETAINED"
NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_ARTIFACT_HASH=$(sha256sum "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_ARTIFACT_HASH" != '3bcb475b7be2760ad514d656d6ad5bffaaca7f74dce0439eff1e277ac7b2d5cb' ] ||
   [ "$(wc -c < "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_ARTIFACT_RETAINED")" -ne 216203 ]; then
    echo "The retained hosted-tool metadata-construction fragment has an unexpected identity: $NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_ARTIFACT_HASH" >&2
    exit 1
fi
NATIVE_HOSTED_STARTUP_INSTANTIATION_RETAINED="$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Startup-Instantiation.wvb"
NATIVE_HOSTED_STARTUP_INSTANTIATION_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Startup-Instantiation.wvnf"
WINDOWS_HOSTED_COMPILER_STARTUP_OBJECT_RETAINED="$REPOSITORY_ROOT/Linker/Reference/Consumers/Windows-X64-Hosted-Compiler.wvo"
LINUX_HOSTED_COMPILER_STARTUP_OBJECT_RETAINED="$REPOSITORY_ROOT/Linker/Reference/Consumers/Linux-X64-Hosted-Compiler.wvo"
cmp -s "$NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE" "$NATIVE_HOSTED_STARTUP_INSTANTIATION_RETAINED"
NATIVE_HOSTED_STARTUP_INSTANTIATION_ARTIFACT_HASH=$(sha256sum "$NATIVE_HOSTED_STARTUP_INSTANTIATION_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_HOSTED_STARTUP_INSTANTIATION_ARTIFACT_HASH" != 'ad1c049bdf77cb410b95cb638aa401874cca1a21b496e36ecab32ceef1539ffd' ] ||
    [ "$(wc -c < "$NATIVE_HOSTED_STARTUP_INSTANTIATION_ARTIFACT_RETAINED")" -ne 193891 ]; then
    echo "The retained hosted-startup instantiation fragment has an unexpected identity: $NATIVE_HOSTED_STARTUP_INSTANTIATION_ARTIFACT_HASH" >&2
    exit 1
fi
if [ "$(sha256sum "$WINDOWS_HOSTED_COMPILER_STARTUP_OBJECT_RETAINED" | awk '{print $1}')" != 'dbf9314d43b47ffc5d3cdeef3c439456b295ac5c3a1cda0b1faaff6227910161' ] ||
    [ "$(wc -c < "$WINDOWS_HOSTED_COMPILER_STARTUP_OBJECT_RETAINED")" -ne 4398 ] ||
    [ "$(sha256sum "$LINUX_HOSTED_COMPILER_STARTUP_OBJECT_RETAINED" | awk '{print $1}')" != '1b8c08308d3f7320b741ae86022400ced6748352314b7f27954ec1c5a7345946' ] ||
    [ "$(wc -c < "$LINUX_HOSTED_COMPILER_STARTUP_OBJECT_RETAINED")" -ne 2454 ]; then
    echo 'A retained hosted-compiler startup WVO has an unexpected identity.' >&2
    exit 1
fi
verify_hosted_container_artifact() {
    name="$1"
    output="$2"
    retained="$3"
    module_bytes="$4"
    module_hash="$5"
    fragment="$6"
    fragment_bytes="$7"
    fragment_hash="$8"
    actual_module_hash=$(sha256sum "$output" | awk '{print $1}')
    if [ "$actual_module_hash" != "$module_hash" ] ||
        [ "$(wc -c < "$output")" -ne "$module_bytes" ] ||
        [ "$(sha256sum "$retained" | awk '{print $1}')" != "$module_hash" ] ||
        [ "$(wc -c < "$retained")" -ne "$module_bytes" ]; then
        echo "The hosted-container $name WVB has an unexpected identity: $actual_module_hash" >&2
        exit 1
    fi
    actual_fragment_hash=$(sha256sum "$fragment" | awk '{print $1}')
    if [ "$actual_fragment_hash" != "$fragment_hash" ] ||
        [ "$(wc -c < "$fragment")" -ne "$fragment_bytes" ]; then
        echo "The hosted-container $name WVNF has an unexpected identity: $actual_fragment_hash" >&2
        exit 1
    fi
}
verify_hosted_container_artifact \
    'planner' \
    "$NATIVE_HOSTED_CONTAINER_PLAN_MODULE" \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Construction.wvb" \
    35929 ff1b48cfc05baab5f707dcfce7e73b0714e2379ee594e12f6e9c6ea1589fef7e \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Construction.wvnf" \
    561553 f353459548490e28a747c2a9fe37ef047412fca6c55e45da462e0d6d2c2128b3
verify_hosted_container_artifact \
    'Windows byte constructor' \
    "$NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE" \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Windows.wvb" \
    17679 a77e4ea3ac2cff35e965ae44cd486f30dd5b0c10aa2cde23c109d0eca37bffcb \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Windows.wvnf" \
    184382 b02d27b75e9c5fd637fa3ba031d6b03820ae6bce41dbcdaff971a0ee57c1bd22
verify_hosted_container_artifact \
    'Linux byte constructor' \
    "$NATIVE_HOSTED_CONTAINER_LINUX_MODULE" \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Linux.wvb" \
    12328 dac93155c68ba18f6cbe3af2d301a4c4171b9a9c05841057ea57398536fa8b42 \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Linux.wvnf" \
    126015 4da05782a516e84af8cc0fc2d5c3056dc99ce3fe6c32bc6dbe6e7f9b85314f81
verify_hosted_container_artifact \
    'segment constructor' \
    "$NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE" \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Segmentation.wvb" \
    22584 d6d74f7d27df9f04f02b8eac2e75fde4fc230ba70d198f90b31ad668a06052e6 \
    "$REPOSITORY_ROOT/Linker/Reference/Consumers/Native-Hosted-Container-Segmentation.wvnf" \
    286727 923f7ff4552e0774e613d5805d8fbdbfff9edaa7347108d3d23626b68fe5dee7
cmp -s "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_MODULE" "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_RETAINED"
NATIVE_HOSTED_TOOL_RUNTIME_HEADER_ARTIFACT_HASH=$(sha256sum "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_ARTIFACT_HASH" != '91590986b8c3421ffdca9ecffb8a1798718f868614b77c581c266f4a2061b632' ] ||
   [ "$(wc -c < "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_ARTIFACT_RETAINED")" -ne 195394 ]; then
    echo "The retained hosted-tool runtime-header fragment has an unexpected identity: $NATIVE_HOSTED_TOOL_RUNTIME_HEADER_ARTIFACT_HASH" >&2
    exit 1
fi
NATIVE_PUBLICATION_LIFETIME_BRIDGE_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Publication-Lifetime-Bridge.wvb"
NATIVE_PUBLICATION_LIFETIME_ARTIFACT_RETAINED="$REPOSITORY_ROOT/Runtime/Windvale.Native/Consumers/Native-Publication-Lifetime-Bridge.wvnf"
cmp -s "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE" "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_RETAINED"
NATIVE_PUBLICATION_LIFETIME_ARTIFACT_HASH=$(sha256sum "$NATIVE_PUBLICATION_LIFETIME_ARTIFACT_RETAINED" | awk '{print $1}')
if [ "$NATIVE_PUBLICATION_LIFETIME_ARTIFACT_HASH" != '4d87911f2f442e6a2e4dd2364138f35a0037ddc0bff0775a16e37156768777a8' ] ||
    [ "$(wc -c < "$NATIVE_PUBLICATION_LIFETIME_ARTIFACT_RETAINED")" -ne 46125 ]; then
    echo "The retained Windvale native publication-lifetime fragment has an unexpected identity: $NATIVE_PUBLICATION_LIFETIME_ARTIFACT_HASH" >&2
    exit 1
fi
SOURCE_LEXER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Lexer-Core.wv"
SOURCE_LEXER_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_LEXER_DEMO_MODULE" --max-steps 10000000)
printf '%s\n' "$SOURCE_LEXER_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_DECLARATION_PARSER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Declaration-Parser.wv"
SOURCE_DECLARATION_PARSER_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_DECLARATION_PARSER_DEMO_MODULE" --max-steps 20000000)
printf '%s\n' "$SOURCE_DECLARATION_PARSER_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_LEXER_DECLARATION_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_DECLARATION_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 30000000 \
    -- "$SOURCE_LEXER_SOURCE")
printf '%s\n' "$SOURCE_LEXER_DECLARATION_OUTPUT" | grep -F 'source declarations status=Valid imports=1 capabilities=0 data=0 records=3 enums=3 functions=19 tokens=6881 offset=56312' >/dev/null
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
printf '%s\n' "$SOURCE_PARSER_SELF_DECLARATION_OUTPUT" | grep -F 'source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=32 tokens=15142 offset=112567' >/dev/null
printf '%s\n' "$SOURCE_PARSER_SELF_DECLARATION_OUTPUT" | grep -F 'Result: 0' >/dev/null

SOURCE_BODY_PARSER_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Body-Parser.wv"
SOURCE_BODY_PARSER_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BODY_PARSER_DEMO_MODULE" --max-steps 30000000)
printf '%s\n' "$SOURCE_BODY_PARSER_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_LEXER_BODY_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$SOURCE_BODY_PARSER_TOOL_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 100000000 \
    -- "$SOURCE_LEXER_SOURCE")
printf '%s\n' "$SOURCE_LEXER_BODY_OUTPUT" | grep -F 'source bodies status=Valid functions=19 top-level=131 statements=749 expression-nodes=2153 statement-depth=17 expression-depth=5 offset=56313' >/dev/null
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
printf '%s\n' "$SOURCE_DECLARATION_BODY_OUTPUT" | grep -F 'source bodies status=Valid functions=32 top-level=365 statements=921 expression-nodes=3601 statement-depth=12 expression-depth=5 offset=112568' >/dev/null
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
printf '%s\n' "$SOURCE_BODY_SELF_OUTPUT" | grep -F 'source bodies status=Valid functions=48 top-level=339 statements=812 expression-nodes=3607 statement-depth=7 expression-depth=3 offset=110706' >/dev/null
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
if [ "$SOURCE_SET_HASH" != '1121320e20d83f685c559ea2d0cff8b8e57583d047a3c6aaf9f5c1fdc9423acb' ]; then
    echo "The Windvale source-set core has an unexpected digest: $SOURCE_SET_HASH" >&2
    exit 1
fi
SOURCE_SET_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_SET_MODULE")
printf '%s\n' "$SOURCE_SET_INSPECTION" | grep -F 'Nominal types (29)' >/dev/null
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
if [ "$SOURCE_SET_DEMO_HASH" != 'ac7fb0e04cf042ab9f9f3bfc8f344f0fdbcdc4198189b65f152eaead84b07742' ]; then
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
if [ "$SOURCE_SET_TOOL_HASH" != '6e8b8c8aaa6fe2c5735719a9b317e8897cf70f87828ea1be5d26d670bc2ed30f' ]; then
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
printf '%s\n' "$SOURCE_SET_SELF_OUTPUT" | grep -F 'source set status=Valid modules=5 source-bytes=297051 imports=6 records=18 enums=11 functions=110' >/dev/null
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
if [ "$SOURCE_GRAPH_HASH" != '9c1ae01b93b9a598fd6b726071dad9a8b4c6fe47d9c8e2d060eff9451724c85b' ]; then
    echo "The Windvale source-graph core has an unexpected digest: $SOURCE_GRAPH_HASH" >&2
    exit 1
fi
SOURCE_GRAPH_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_GRAPH_MODULE")
printf '%s\n' "$SOURCE_GRAPH_INSPECTION" | grep -F 'Nominal types (34)' >/dev/null
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
if [ "$SOURCE_GRAPH_DEMO_HASH" != 'a762e564411e9fe72b906c3c37521c9047bb40b1267d2fb46223f382f1c7966c' ]; then
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
if [ "$SOURCE_GRAPH_TOOL_HASH" != '0a23a10c6abb9eb82229300ab92324f3298fcbf26d3be0948dbc984274a9ac10' ]; then
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
if [ "$SOURCE_SYMBOLS_HASH" != 'a7df71802871d48561c8045d7e997266365d74f7e5158d531164ae636d57a5e7' ]; then
    echo "The Windvale source-symbol core has an unexpected digest: $SOURCE_SYMBOLS_HASH" >&2
    exit 1
fi
SOURCE_SYMBOLS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_SYMBOLS_MODULE")
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Nominal types (45)' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolˉstatus' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolˉsummary' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉsourceˉsymbolsˉdirectoryˉisˉvalid' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Compilerˉvalidateˉsourceˉsymbols' >/dev/null
printf '%s\n' "$SOURCE_SYMBOLS_INSPECTION" | grep -F 'Exports (66)' >/dev/null
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
if [ "$SOURCE_SYMBOLS_DEMO_HASH" != '4cf84322af1cd514bc7ac9ac5e752ef689bb1729e83ea9021b9660c823243457' ]; then
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
if [ "$SOURCE_SYMBOLS_TOOL_HASH" != '58732a7cb3352f1f61ba4cecb65ae0280aecc975ca06eca359a2881e14477a66' ]; then
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
printf '%s\n' "$SOURCE_SYMBOLS_SELF_OUTPUT" | grep -F 'source symbols status=Valid modules=8 capabilities=0 data=0 records=31 enums=14 functions=202 fields=344 members=245 parameters=891 directory-bytes=5944 visibility-bytes=64' >/dev/null
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
if [ "$SOURCE_BINDINGS_HASH" != 'a772a75fe625f47e165ca190e76d8cd59fa0b591a0270a5817e02e0fac62542c' ]; then
    echo "The Windvale source-binding core has an unexpected digest: $SOURCE_BINDINGS_HASH" >&2
    exit 1
fi
SOURCE_BINDINGS_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_BINDINGS_MODULE")
printf '%s\n' "$SOURCE_BINDINGS_INSPECTION" | grep -F 'Nominal types (55)' >/dev/null
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
if [ "$SOURCE_BINDINGS_DEMO_HASH" != '563caeb4a76fb34d6c2b2b8340260cc1da518c4cbaad9e5f355201f6bd1fa933' ]; then
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
if [ "$SOURCE_BINDINGS_TOOL_HASH" != '17e877b3c59d2f9a99d26be4c478f10ce8879e6bce925b65894d158fd4a6e0a9' ]; then
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
printf '%s\n' "$SOURCE_BINDINGS_SELF_OUTPUT" | grep -F 'source bindings status=Valid modules=9 functions=261 parameters=1154 locals=1584 reads=13354 assignments=1098 calls=2317 directory-bytes=101120' >/dev/null
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
if [ "$SOURCE_WIR_HASH" != 'c4c3bd9164ccdf75acd1140e74c256295bb1f8ea8bdbf69cdcd3225ceea70fbb' ]; then
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
if [ "$SOURCE_WIR_DEMO_HASH" != '7f533fcb38a9311ba4d390b814ea3741ab25d5db9ac2167bd9f4f6b58bddc02f' ]; then
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
if [ "$SOURCE_WIR_TOOL_HASH" != '7fbfc8f57620dd81a5d2024310a21a8ce32d56cc986d94b39ca03428c1404db5' ]; then
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
SOURCE_WVB_TEMPORARY_SLOTS_SOURCE="$REPOSITORY_ROOT/Compiler/Windvale/Source-Wvb-Temporary-Slots.wv"
compile_source_wvb() {
    dotnet "$TOOL_DLL" \
        compile "$1" \
        --module "$SOURCE_WVB_SOURCE" \
        --module "$SOURCE_WVB_TEMPORARY_SLOTS_SOURCE" \
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
    --module "$SOURCE_WVB_TEMPORARY_SLOTS_SOURCE" \
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
if [ "$SOURCE_WVB_HASH" != 'c4602b6c026a65e0b9de11c025768b7f652ee73640b6f5ff1806d40ee5d0071b' ]; then
    echo "The Windvale WVB backend core has an unexpected digest: $SOURCE_WVB_HASH" >&2
    exit 1
fi
SOURCE_WVB_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WVB_MODULE")
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Compilerˉsourceˉwvbˉsummary' >/dev/null
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Compilerˉcompileˉsourceˉwvb' >/dev/null
printf '%s\n' "$SOURCE_WVB_INSPECTION" | grep -F 'Exports (72)' >/dev/null
compile_source_wvb "$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Demo.wv" "$SOURCE_WVB_DEMO_MODULE"
SOURCE_WVB_DEMO_HASH=$(sha256sum "$SOURCE_WVB_DEMO_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_DEMO_HASH" != 'ef5a7cad94cce135dd937756980f9268fa2964f49dbb4fccca95ba4d09713fc9' ]; then
    echo "The Windvale WVB backend demo has an unexpected digest: $SOURCE_WVB_DEMO_HASH" >&2
    exit 1
fi
SOURCE_WVB_DEMO_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_DEMO_MODULE" --max-steps 4000000000)
printf '%s\n' "$SOURCE_WVB_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
compile_source_wvb "$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Tool.wv" "$SOURCE_WVB_TOOL_MODULE"
SOURCE_WVB_TOOL_HASH=$(sha256sum "$SOURCE_WVB_TOOL_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_TOOL_HASH" != '18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754' ]; then
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
printf '%s\n' "$SOURCE_WVB_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=4 code-bytes=532 module-bytes=816' >/dev/null
printf '%s\n' "$SOURCE_WVB_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉfixture' >/dev/null
SOURCE_WVB_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_RUN_OUTPUT" | grep -F 'Result: 6' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_FIXTURE" -o "$SOURCE_WVB_FIXTURE_ORACLE"
SOURCE_WVB_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_FIXTURE_HASH" != '28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936' ]; then
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
printf '%s\n' "$SOURCE_WVB_DATA_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=3 code-bytes=1210 module-bytes=1652' >/dev/null
printf '%s\n' "$SOURCE_WVB_DATA_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_DATA_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_DATA_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_DATA_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉdataˉandˉtext' >/dev/null
SOURCE_WVB_DATA_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_DATA_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_DATA_RUN_OUTPUT" | grep -F 'Result: 13' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_DATA_FIXTURE" -o "$SOURCE_WVB_DATA_FIXTURE_ORACLE"
SOURCE_WVB_DATA_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_DATA_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_DATA_FIXTURE_HASH" != '8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc' ]; then
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
printf '%s\n' "$SOURCE_WVB_NOMINAL_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=3 code-bytes=1097 module-bytes=1782' >/dev/null
printf '%s\n' "$SOURCE_WVB_NOMINAL_FIXTURE_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_NOMINAL_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_NOMINAL_VERIFY_OUTPUT" | grep -F 'Verified: Sourceˉwvbˉnominalˉtypes' >/dev/null
SOURCE_WVB_NOMINAL_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE")
printf '%s\n' "$SOURCE_WVB_NOMINAL_RUN_OUTPUT" | grep -F 'Result: 11' >/dev/null
dotnet "$TOOL_DLL" compile "$SOURCE_WVB_NOMINAL_FIXTURE" -o "$SOURCE_WVB_NOMINAL_FIXTURE_ORACLE"
SOURCE_WVB_NOMINAL_FIXTURE_HASH=$(sha256sum "$SOURCE_WVB_NOMINAL_FIXTURE_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_NOMINAL_FIXTURE_HASH" != 'b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b' ]; then
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
printf '%s\n' "$SOURCE_WVB_HOSTED_FIXTURE_OUTPUT" | grep -F 'source wvb status=Valid functions=7 code-bytes=249 module-bytes=850' >/dev/null
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
if [ "$SOURCE_WVB_HOSTED_FIXTURE_HASH" != 'bad95ed62ed8406c169ddadaa8da8576825d9213af2faa74b945db44afdfd41f' ]; then
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
printf '%s\n' "$SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'source wvb status=Valid functions=9 code-bytes=627 module-bytes=1388' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_OUTPUT" | grep -F 'Result: 0' >/dev/null
SOURCE_WVB_COMPOSITION_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_VERIFY_OUTPUT" | grep -F 'Verified: Compositionˉdemo' >/dev/null
SOURCE_WVB_COMPOSITION_INSPECTION=$(dotnet "$TOOL_DLL" inspect "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Data (4)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F '[2] __Text_000001: text' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Nominal types (5)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Functions (9)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Exports (1)' >/dev/null
printf '%s\n' "$SOURCE_WVB_COMPOSITION_INSPECTION" | grep -F 'Main -> function[1]' >/dev/null
SOURCE_WVB_COMPOSITION_RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$SOURCE_WVB_COMPOSITION_MODULE")
printf '%s\n' "$SOURCE_WVB_COMPOSITION_RUN_OUTPUT" | grep -F 'Result: 42' >/dev/null
dotnet "$TOOL_DLL" \
    compile "$SOURCE_WVB_COMPOSITION_ROOT" \
    --module "$SOURCE_WVB_COMPOSITION_LEAF" \
    --module "$SOURCE_WVB_COMPOSITION_MIDDLE" \
    -o "$SOURCE_WVB_COMPOSITION_ORACLE"
SOURCE_WVB_COMPOSITION_HASH=$(sha256sum "$SOURCE_WVB_COMPOSITION_MODULE" | awk '{print $1}')
if [ "$SOURCE_WVB_COMPOSITION_HASH" != '42d134ee0674dcc2cfa97d018ea03b27f014b2f916d8273ba02a0aee868e0fd5' ]; then
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
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'module version=1.11 profile=portable name="Sum\u02C9data"' >/dev/null
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
    compile "$REPOSITORY_ROOT/Object-Model/Windvale/Wvo-Object-Core.wv" \
    --module "$BYTE_ORDERING_SOURCE" \
    --module "$SHA256_SOURCE" \
    -o "$WVO_CORE_MODULE"

WVO_CORE_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" verify "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_VERIFY_OUTPUT" | grep -F 'Verified: Wvoˉobjectˉcore' >/dev/null

WVO_CORE_INSPECT_OUTPUT=$(dotnet "$TOOL_DLL" inspect "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.concat' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_u16_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_i32_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'text.to_utf8' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F '__WvM1F0' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'file.read_bytes' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'Objectˉsha256' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F '__WvM2F0(bytes) -> bytes' >/dev/null
if printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'file.write_bytes' >/dev/null; then
    echo 'The read-only Windvale object core unexpectedly retained file-write authority.' >&2
    exit 1
fi

set +e
WVO_UNAUTHORIZED_OUTPUT=$(dotnet "$TOOL_DLL" run "$WVO_CORE_MODULE" 2>&1)
WVO_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVO_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WVO read-only run exit 3, found $WVO_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVO_SELF_TEST_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVO_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVO_SAMPLE_OUTPUT=$(dotnet "$TOOL_DLL" \
    assemble "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva" \
    -o "$WVO_SAMPLE")
printf '%s\n' "$WVO_SAMPLE_OUTPUT" | grep -F "Assembled: $WVO_SAMPLE" >/dev/null

WVO_HASH=$(sha256sum "$WVO_SAMPLE" | awk '{print $1}')
if [ "$WVO_HASH" != '992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85' ]; then
    echo "The WVO inspector input has unexpected bytes: $WVO_HASH" >&2
    exit 1
fi

WVO_HOSTED_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- verify "$WVO_SAMPLE")
printf '%s\n' "$WVO_HOSTED_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null
printf '%s\n' "$WVO_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVO_HOSTED_INSPECTION=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- inspect "$WVO_SAMPLE")
printf '%s\n' "$WVO_HOSTED_INSPECTION" | grep -F 'Sections (2)' >/dev/null
printf '%s\n' "$WVO_HOSTED_INSPECTION" | grep -F 'Console_write binding=Import' >/dev/null
printf '%s\n' "$WVO_HOSTED_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' >/dev/null
printf '%s\n' "$WVO_HOSTED_INSPECTION" | grep -F 'Result: 0' >/dev/null

WVO_VERIFY_OUTPUT=$(dotnet "$TOOL_DLL" object-verify "$WVO_SAMPLE")
printf '%s\n' "$WVO_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

WVO_INSPECTION=$(dotnet "$TOOL_DLL" object-inspect "$WVO_SAMPLE")
printf '%s\n' "$WVO_INSPECTION" | grep -F 'Sections (2)' >/dev/null
printf '%s\n' "$WVO_INSPECTION" | grep -F 'Console_write binding=Import' >/dev/null
printf '%s\n' "$WVO_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' >/dev/null

set +e
WVO_INVALID_NAME_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- verify '' 2>&1)
WVO_INVALID_NAME_EXIT=$?
set -e
if [ "$WVO_INVALID_NAME_EXIT" -ne 3 ]; then
    echo "Expected invalid hosted file reader name exit 3, found $WVO_INVALID_NAME_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_INVALID_NAME_OUTPUT" | grep -F 'WVR3021' >/dev/null

MISSING_WVO_INPUT="$ARTIFACTS/__windvale_missing_wvo_input__.wvo"
if [ -e "$MISSING_WVO_INPUT" ]; then
    echo "The missing WVO input unexpectedly exists: $MISSING_WVO_INPUT" >&2
    exit 1
fi
set +e
WVO_MISSING_INPUT_OUTPUT=$(dotnet "$TOOL_DLL" \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- verify "$MISSING_WVO_INPUT" 2>&1)
WVO_MISSING_INPUT_EXIT=$?
set -e
if [ "$WVO_MISSING_INPUT_EXIT" -ne 3 ]; then
    echo "Expected missing hosted WVO input exit 3, found $WVO_MISSING_INPUT_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_MISSING_INPUT_OUTPUT" | grep -F 'WVR3022' >/dev/null

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
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F '__WvM4F1' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F '__WvM2F0' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F '__WvM3F0' >/dev/null
printf '%s\n' "$WVA_ASSEMBLER_INSPECT_OUTPUT" | grep -F '__WvM1F0' >/dev/null
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
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F '__WvM4F0' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F '__WvM2F0' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F '__WvM3F0' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F '__WvM1F0' >/dev/null
printf '%s\n' "$WVLINK_INSPECT_OUTPUT" | grep -F '__WvM1F1' >/dev/null
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
