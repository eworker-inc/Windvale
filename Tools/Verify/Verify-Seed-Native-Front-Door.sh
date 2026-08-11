#!/usr/bin/env sh
set -eu

if [ "$#" -ne 1 ] || [ ! -d "$1" ]; then
    echo 'Usage: Tools/Verify/Verify-Seed-Native-Front-Door.sh <output-directory>' >&2
    exit 64
fi

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
OUTPUT_ROOT=$(CDPATH= cd -- "$1" && pwd)
NATIVE_BUILD="$REPOSITORY_ROOT/Tools/Native/Build-Wvb.sh"
NATIVE_VERIFY="$REPOSITORY_ROOT/Tools/Native/Verify-Wvb.sh"
NATIVE_INSPECT="$REPOSITORY_ROOT/Tools/Native/Inspect-Wvb.sh"
NATIVE_RUN="$REPOSITORY_ROOT/Tools/Native/Run-Wvb.sh"

exact_build() {
    PROJECT_PATH=$1
    OUTPUT_PATH=$2
    EXPECTED_BYTES=$3
    EXPECTED_SHA256=$4
    EXPECTED_HEX_BYTES=$5
    EXPECTED_BUILD_REPORT=$6
    if ! BUILD_OUTPUT=$("$NATIVE_BUILD" "$PROJECT_PATH" "$OUTPUT_PATH"); then
        echo "The native Seed project build failed: $PROJECT_PATH" >&2
        exit 1
    fi
    EXPECTED_OUTPUT=$(printf '%s\n%s' \
        "$EXPECTED_BUILD_REPORT" \
        "publication status=Complete bytes=0x$EXPECTED_HEX_BYTES sha256=$EXPECTED_SHA256")
    if [ "$BUILD_OUTPUT" != "$EXPECTED_OUTPUT" ]; then
        echo "The native Seed project build report is invalid: $PROJECT_PATH" >&2
        exit 1
    fi
    ACTUAL_BYTES=$(wc -c < "$OUTPUT_PATH" | tr -d ' ')
    ACTUAL_SHA256=$(sha256sum "$OUTPUT_PATH" | awk '{print $1}')
    if [ "$ACTUAL_BYTES" != "$EXPECTED_BYTES" ] || [ "$ACTUAL_SHA256" != "$EXPECTED_SHA256" ]; then
        echo "The native Seed project build produced an unexpected module: $OUTPUT_PATH" >&2
        exit 1
    fi
}

exact_verify() {
    if ! VERIFY_OUTPUT=$("$NATIVE_VERIFY" "$1"); then
        echo "The native Seed verifier rejected: $1" >&2
        exit 1
    fi
    if [ "$VERIFY_OUTPUT" != 'wvb status=Valid profile=compiler-aligned' ]; then
        echo "The native Seed verifier report is invalid: $1" >&2
        exit 1
    fi
}

exact_inspect() {
    MODULE_PATH=$1
    shift
    if ! INSPECT_OUTPUT=$("$NATIVE_INSPECT" "$MODULE_PATH"); then
        echo "The native Seed inspector rejected: $MODULE_PATH" >&2
        exit 1
    fi
    for REQUIRED_PATTERN in "$@"; do
        if ! printf '%s\n' "$INSPECT_OUTPUT" | grep -F "$REQUIRED_PATTERN" >/dev/null; then
            echo "The native Seed inspector omitted required evidence: $MODULE_PATH" >&2
            exit 1
        fi
    done
}

exact_run() {
    MODULE_PATH=$1
    EXPECTED_RESULT=$2
    EXPECTED_BYTES=$3
    EXPECTED_SHA256=$4
    RUN_ERROR=$(mktemp "${TMPDIR:-/tmp}/windvale-seed-run.XXXXXX")
    if ! RUN_OUTPUT=$("$NATIVE_RUN" "$MODULE_PATH" 2>"$RUN_ERROR"); then
        rm -f -- "$RUN_ERROR"
        echo "The native Seed runner rejected: $MODULE_PATH" >&2
        exit 1
    fi
    if [ -s "$RUN_ERROR" ] || [ "$RUN_OUTPUT" != "Result: $EXPECTED_RESULT" ]; then
        rm -f -- "$RUN_ERROR"
        echo "The native Seed runner report is invalid: $MODULE_PATH" >&2
        exit 1
    fi
    rm -f -- "$RUN_ERROR"
    ACTUAL_BYTES=$(wc -c < "$MODULE_PATH" | tr -d ' ')
    ACTUAL_SHA256=$(sha256sum "$MODULE_PATH" | awk '{print $1}')
    if [ "$ACTUAL_BYTES" != "$EXPECTED_BYTES" ] || [ "$ACTUAL_SHA256" != "$EXPECTED_SHA256" ]; then
        echo "The native Seed runner modified its input module: $MODULE_PATH" >&2
        exit 1
    fi
}

exact_instruction_report() {
    MODULE_PATH=$1
    EXPECTED_RESULT=$2
    EXPECTED_INSTRUCTIONS=$3
    EXPECTED_BYTES=$4
    EXPECTED_SHA256=$5
    RUN_ERROR=$(mktemp "${TMPDIR:-/tmp}/windvale-seed-report.XXXXXX")
    if ! RUN_OUTPUT=$("$NATIVE_RUN" "$MODULE_PATH" --report-steps 2>"$RUN_ERROR"); then
        rm -f -- "$RUN_ERROR"
        echo "The native Seed runner rejected an instruction report: $MODULE_PATH" >&2
        exit 1
    fi
    EXPECTED_OUTPUT=$(printf 'Result: %s\nInstructions: %s' "$EXPECTED_RESULT" "$EXPECTED_INSTRUCTIONS")
    if [ -s "$RUN_ERROR" ] || [ "$RUN_OUTPUT" != "$EXPECTED_OUTPUT" ]; then
        rm -f -- "$RUN_ERROR"
        echo "The native Seed runner instruction report is invalid: $MODULE_PATH" >&2
        exit 1
    fi
    rm -f -- "$RUN_ERROR"
    ACTUAL_BYTES=$(wc -c < "$MODULE_PATH" | tr -d ' ')
    ACTUAL_SHA256=$(sha256sum "$MODULE_PATH" | awk '{print $1}')
    if [ "$ACTUAL_BYTES" != "$EXPECTED_BYTES" ] || [ "$ACTUAL_SHA256" != "$EXPECTED_SHA256" ]; then
        echo "The native Seed runner modified its reported input module: $MODULE_PATH" >&2
        exit 1
    fi
}

SUM_MODULE="$OUTPUT_ROOT/Sum-Data.wvb"
HELLO_MODULE="$OUTPUT_ROOT/Hello-Windvale.wvb"
FOUNDATION_MODULE="$OUTPUT_ROOT/Read-Wvb-Header.wvb"
COMPOSITION_MODULE="$OUTPUT_ROOT/Module-Composition-Demo-Project.wvb"
MACHINE_CONTRACTS_MODULE="$OUTPUT_ROOT/Machine-Contracts.wvb"
MACHINE_CONTRACTS_DEMO_MODULE="$OUTPUT_ROOT/Machine-Contracts-Demo.wvb"
BYTE_ORDERING_MODULE="$OUTPUT_ROOT/Byte-Ordering.wvb"
BYTE_ORDERING_DEMO_MODULE="$OUTPUT_ROOT/Byte-Ordering-Demo.wvb"
DECIMAL_PARSING_MODULE="$OUTPUT_ROOT/Decimal-Parsing.wvb"
DECIMAL_PARSING_DEMO_MODULE="$OUTPUT_ROOT/Decimal-Parsing-Demo.wvb"
BYTE_CONSTRUCTION_MODULE="$OUTPUT_ROOT/Byte-Construction.wvb"
BYTE_CONSTRUCTION_DEMO_MODULE="$OUTPUT_ROOT/Byte-Construction-Demo.wvb"
NATIVE_STENCIL_MODULE="$OUTPUT_ROOT/Native-Stencil-Core.wvb"
NATIVE_STENCIL_DEMO_MODULE="$OUTPUT_ROOT/Native-Stencil-Demo.wvb"
NATIVE_STENCIL_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Stencil-Bridge.wvb"
NATIVE_UTF8_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Utf8-Service.wvb"
NATIVE_UTF8_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-Utf8-Service-Bridge.wvb"
NATIVE_INTEGER_FORMAT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Integer-Format-Services.wvb"
NATIVE_INTEGER_FORMAT_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-Integer-Format-Services-Bridge.wvb"
NATIVE_SERVICE_CODE_BUILDER_MODULE="$OUTPUT_ROOT/Native-X64-Service-Code-Builder.wvb"
NATIVE_WINDOWS_OUTPUT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Output-Service-Windows.wvb"
NATIVE_LINUX_OUTPUT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Output-Service-Linux.wvb"
NATIVE_OUTPUT_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-Output-Services-Bridge.wvb"
NATIVE_FILE_OUTPUT_CODE_MODULE="$OUTPUT_ROOT/Native-X64-File-Output-Service-Code.wvb"
NATIVE_WINDOWS_FILE_OUTPUT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-File-Output-Service-Windows.wvb"
NATIVE_LINUX_FILE_OUTPUT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-File-Output-Service-Linux.wvb"
NATIVE_FILE_OUTPUT_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-File-Output-Services-Bridge.wvb"
NATIVE_FILE_INPUT_CODE_MODULE="$OUTPUT_ROOT/Native-X64-File-Input-Service-Code.wvb"
NATIVE_WINDOWS_FILE_INPUT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-File-Input-Service-Windows.wvb"
NATIVE_LINUX_FILE_INPUT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-File-Input-Service-Linux.wvb"
NATIVE_FILE_INPUT_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-File-Input-Services-Bridge.wvb"
NATIVE_TEXT_CONCAT_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Text-Concat-Service.wvb"
NATIVE_TEXT_CONCAT_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-Text-Concat-Service-Bridge.wvb"
NATIVE_TEXT_QUOTE_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Text-Quote-Service.wvb"
NATIVE_TEXT_QUOTE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-Text-Quote-Service-Bridge.wvb"
NATIVE_ENUM_NAME_CORE_MODULE="$OUTPUT_ROOT/Native-X64-Enum-Name-Service.wvb"
NATIVE_ENUM_NAME_BRIDGE_MODULE="$OUTPUT_ROOT/Native-X64-Enum-Name-Service-Bridge.wvb"
NATIVE_ENUM_METADATA_CORE_MODULE="$OUTPUT_ROOT/Native-Enum-Metadata-Core.wvb"
NATIVE_ENUM_METADATA_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Enum-Metadata-Bridge.wvb"
NATIVE_PUBLICATION_MODULE="$OUTPUT_ROOT/Native-Publication-Core.wvb"
NATIVE_PUBLICATION_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Publication-Bridge.wvb"
NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_MODULE="$OUTPUT_ROOT/Native-Service-Bundle-Materialization-Core.wvb"
NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Service-Bundle-Materialization-Bridge.wvb"
NATIVE_OUTPUT_TABLE_CORE_MODULE="$OUTPUT_ROOT/Native-Output-Table-Core.wvb"
NATIVE_OUTPUT_TABLE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Output-Table-Bridge.wvb"
NATIVE_FILE_OUTPUT_TABLE_CORE_MODULE="$OUTPUT_ROOT/Native-File-Output-Table-Core.wvb"
NATIVE_FILE_OUTPUT_TABLE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-File-Output-Table-Bridge.wvb"
NATIVE_FILE_INPUT_TABLE_CORE_MODULE="$OUTPUT_ROOT/Native-File-Input-Table-Core.wvb"
NATIVE_FILE_INPUT_TABLE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-File-Input-Table-Bridge.wvb"
NATIVE_SERVICE_TABLE_CORE_MODULE="$OUTPUT_ROOT/Native-Service-Table-Core.wvb"
NATIVE_SERVICE_TABLE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Service-Table-Bridge.wvb"
NATIVE_EXECUTION_CONTEXT_CORE_MODULE="$OUTPUT_ROOT/Native-Execution-Context-Core.wvb"
NATIVE_EXECUTION_CONTEXT_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Execution-Context-Bridge.wvb"
NATIVE_ARGUMENT_TABLE_CORE_MODULE="$OUTPUT_ROOT/Native-Argument-Table-Core.wvb"
NATIVE_ARGUMENT_TABLE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Argument-Table-Bridge.wvb"
NATIVE_ENTRY_BRIDGE_CORE_MODULE="$OUTPUT_ROOT/Native-Entry-Bridge-Core.wvb"
NATIVE_ENTRY_BRIDGE_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Entry-Bridge-Bridge.wvb"
NATIVE_BYTE_RESULT_ADMISSION_CORE_MODULE="$OUTPUT_ROOT/Native-Byte-Result-Admission-Core.wvb"
NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Byte-Result-Admission-Bridge.wvb"
NATIVE_HOSTED_TOOL_METADATA_ADMISSION_MODULE="$OUTPUT_ROOT/Native-Hosted-Tool-Metadata-Admission.wvb"
NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_CORE_MODULE="$OUTPUT_ROOT/Native-Hosted-Tool-Metadata-Construction-Core.wvb"
NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Hosted-Tool-Metadata-Construction-Bridge.wvb"
NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE="$OUTPUT_ROOT/Native-Hosted-Startup-Instantiation.wvb"
NATIVE_HOSTED_CONTAINER_PLAN_MODULE="$OUTPUT_ROOT/Native-Hosted-Container-Construction.wvb"
NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE="$OUTPUT_ROOT/Native-Hosted-Container-Windows.wvb"
NATIVE_HOSTED_CONTAINER_LINUX_MODULE="$OUTPUT_ROOT/Native-Hosted-Container-Linux.wvb"
NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE="$OUTPUT_ROOT/Native-Hosted-Container-Segmentation.wvb"
NATIVE_HOSTED_TOOL_RUNTIME_HEADER_CORE_MODULE="$OUTPUT_ROOT/Native-Hosted-Tool-Runtime-Header-Core.wvb"
NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Hosted-Tool-Runtime-Header-Bridge.wvb"
NATIVE_PUBLICATION_LIFETIME_CORE_MODULE="$OUTPUT_ROOT/Native-Publication-Lifetime-Core.wvb"
NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE="$OUTPUT_ROOT/Native-Publication-Lifetime-Bridge.wvb"
SOURCE_LEXER_MODULE="$OUTPUT_ROOT/Source-Lexer-Core.wvb"
SOURCE_LEXER_DEMO_MODULE="$OUTPUT_ROOT/Source-Lexer-Demo.wvb"
SOURCE_DECLARATION_PARSER_MODULE="$OUTPUT_ROOT/Source-Declaration-Parser.wvb"
SOURCE_DECLARATION_PARSER_DEMO_MODULE="$OUTPUT_ROOT/Source-Declaration-Parser-Demo.wvb"
SOURCE_DECLARATION_PARSER_TOOL_MODULE="$OUTPUT_ROOT/Source-Declaration-Parser-Tool.wvb"
SOURCE_BODY_PARSER_MODULE="$OUTPUT_ROOT/Source-Body-Parser.wvb"
SOURCE_BODY_PARSER_DEMO_MODULE="$OUTPUT_ROOT/Source-Body-Parser-Demo.wvb"
SOURCE_BODY_PARSER_TOOL_MODULE="$OUTPUT_ROOT/Source-Body-Parser-Tool.wvb"

exact_build \
    "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wvproj" \
    "$SUM_MODULE" \
    494 \
    76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df \
    000001ee \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=270 module-bytes=494'
exact_verify "$SUM_MODULE"
exact_inspect "$SUM_MODULE" 'opcode=data.load.i32 operand=0'
exact_run \
    "$SUM_MODULE" \
    29 \
    494 \
    76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df
exact_instruction_report \
    "$SUM_MODULE" \
    29 \
    203 \
    494 \
    76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df

exact_build \
    "$REPOSITORY_ROOT/Examples/Seed/Hello-Windvale.wvproj" \
    "$HELLO_MODULE" \
    253 \
    0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f \
    000000fd \
    'build status=Published verification=compiler-aligned functions=1 code-bytes=36 module-bytes=253'

exact_build \
    "$REPOSITORY_ROOT/Examples/Foundation/Read-Wvb-Header.wvproj" \
    "$FOUNDATION_MODULE" \
    1701 \
    c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793 \
    000006a5 \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=1379 module-bytes=1701'
exact_verify "$FOUNDATION_MODULE"
exact_inspect "$FOUNDATION_MODULE" 'opcode=bytes.read_u32_little'
exact_run \
    "$FOUNDATION_MODULE" \
    1 \
    1701 \
    c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793

exact_build \
    "$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Demo.wvproj" \
    "$COMPOSITION_MODULE" \
    660 \
    030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607 \
    00000294 \
    'build status=Published verification=compiler-aligned functions=4 code-bytes=280 module-bytes=660'
exact_run \
    "$COMPOSITION_MODULE" \
    42 \
    660 \
    030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607

exact_build \
    "$REPOSITORY_ROOT/Foundation/Machine-Contracts.wvproj" \
    "$MACHINE_CONTRACTS_MODULE" \
    2466 \
    f624739461dea01862121daf234b3a838dfcafd73753e3124a038b7efa8b4fa3 \
    000009a2 \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2019 module-bytes=2466'
exact_inspect \
    "$MACHINE_CONTRACTS_MODULE" \
    'Foundation\u02C9alignment\u02C9is\u02C9valid' \
    'Foundation\u02C9machine\u02C9name\u02C9is\u02C9valid' \
    'section name=exports offset=2364 bytes=90 count=2'
exact_build \
    "$REPOSITORY_ROOT/Foundation-Machine-Contracts-Demo.wvproj" \
    "$MACHINE_CONTRACTS_DEMO_MODULE" \
    3487 \
    69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3 \
    00000d9f \
    'build status=Published verification=compiler-aligned functions=3 code-bytes=2899 module-bytes=3487'
exact_run \
    "$MACHINE_CONTRACTS_DEMO_MODULE" \
    0 \
    3487 \
    69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3

exact_build \
    "$REPOSITORY_ROOT/Foundation/Byte-Ordering.wvproj" \
    "$BYTE_ORDERING_MODULE" \
    990 \
    27a3c24b5cc358a4f67e2e1959b5e80559918f0176c52e08648e638212e6dece \
    000003de \
    'build status=Published verification=compiler-aligned functions=1 code-bytes=720 module-bytes=990'
exact_inspect \
    "$BYTE_ORDERING_MODULE" \
    'Foundation\u02C9byte\u02C9spans\u02C9compare' \
    'section name=exports offset=933 bytes=45 count=1'
exact_build \
    "$REPOSITORY_ROOT/Foundation-Byte-Ordering-Demo.wvproj" \
    "$BYTE_ORDERING_DEMO_MODULE" \
    2422 \
    fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f \
    00000976 \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2059 module-bytes=2422'
exact_run \
    "$BYTE_ORDERING_DEMO_MODULE" \
    0 \
    2422 \
    fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f

exact_build \
    "$REPOSITORY_ROOT/Foundation/Decimal-Parsing.wvproj" \
    "$DECIMAL_PARSING_MODULE" \
    1698 \
    bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37 \
    000006a2 \
    'build status=Published verification=compiler-aligned functions=1 code-bytes=1301 module-bytes=1698'
exact_inspect \
    "$DECIMAL_PARSING_MODULE" \
    'Foundation\u02C9u32\u02C9parse' \
    'Foundation\u02C9u32\u02C9decimal\u02C9parse' \
    'section name=exports offset=1591 bytes=44 count=1'
exact_build \
    "$REPOSITORY_ROOT/Foundation-Decimal-Parsing-Demo.wvproj" \
    "$DECIMAL_PARSING_DEMO_MODULE" \
    3742 \
    d323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453 \
    00000e9e \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2969 module-bytes=3742'
exact_run \
    "$DECIMAL_PARSING_DEMO_MODULE" \
    0 \
    3742 \
    d323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453

exact_build \
    "$REPOSITORY_ROOT/Foundation/Byte-Construction.wvproj" \
    "$BYTE_CONSTRUCTION_MODULE" \
    2001 \
    3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8 \
    000007d1 \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=1503 module-bytes=2001'
exact_inspect \
    "$BYTE_CONSTRUCTION_MODULE" \
    'Foundation\u02C9bytes\u02C9result' \
    'Foundation\u02C9bytes\u02C9repeat' \
    'Foundation\u02C9bytes\u02C9replace' \
    'section name=exports offset=1862 bytes=73 count=2'
exact_build \
    "$REPOSITORY_ROOT/Foundation-Byte-Construction-Demo.wvproj" \
    "$BYTE_CONSTRUCTION_DEMO_MODULE" \
    5017 \
    ab594976ced7a84573ade0aa50fb4370d96b8004c8b9a5ec1e888968c7b3bf8f \
    00001399 \
    'build status=Published verification=compiler-aligned functions=3 code-bytes=4194 module-bytes=5017'

exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Stencil-Core.wvproj" \
    "$NATIVE_STENCIL_MODULE" \
    21296 \
    6df3c524d0f9bec79cd2516a758985c487cc237c6f94bc5b80e015975d50cca3 \
    00005330 \
    'build status=Published verification=compiler-aligned functions=20 code-bytes=16427 module-bytes=21296'
exact_inspect \
    "$NATIVE_STENCIL_MODULE" \
    'Native\u02C9stencil\u02C9result' \
    'Native\u02C9stencil\u02C9patch\u02C9kind' \
    'Native\u02C9stencil\u02C9process\u02C9argument\u02C9count' \
    'Native\u02C9stencil\u02C9process\u02C9argument' \
    'section name=exports offset=19576 bytes=927 count=20'
exact_build \
    "$REPOSITORY_ROOT/Native-Stencil-Demo.wvproj" \
    "$NATIVE_STENCIL_DEMO_MODULE" \
    25683 \
    6b27fbd10d5f06855354f433ec0b8c9b1af1761ef04458817931e675c26e0da8 \
    00006453 \
    'build status=Published verification=compiler-aligned functions=24 code-bytes=21063 module-bytes=25683'
exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Stencil-Bridge.wvproj" \
    "$NATIVE_STENCIL_BRIDGE_MODULE" \
    20800 \
    0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da \
    00005140 \
    'build status=Published verification=compiler-aligned functions=21 code-bytes=16833 module-bytes=20800'
exact_inspect "$NATIVE_STENCIL_BRIDGE_MODULE" 'name="Main" parameters=0 result=bytes' 'section name=exports offset=20065 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Utf8-Service-Core.wvproj" \
    "$NATIVE_UTF8_CORE_MODULE" \
    11577 \
    adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7 \
    00002d39 \
    'build status=Published verification=compiler-aligned functions=18 code-bytes=9098 module-bytes=11577'
exact_inspect "$NATIVE_UTF8_CORE_MODULE" 'profile=portable' 'Native\u02C9x64\u02C9utf8\u02C9service\u02C9build' 'section name=exports offset=11468 bytes=46 count=1'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Utf8-Service.wvproj" \
    "$NATIVE_UTF8_BRIDGE_MODULE" \
    11511 \
    4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f \
    00002cf7 \
    'build status=Published verification=compiler-aligned functions=19 code-bytes=9114 module-bytes=11511'
exact_inspect "$NATIVE_UTF8_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=11444 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Integer-Format-Services-Core.wvproj" \
    "$NATIVE_INTEGER_FORMAT_CORE_MODULE" \
    11611 \
    6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2 \
    00002d5b \
    'build status=Published verification=compiler-aligned functions=11 code-bytes=9588 module-bytes=11611'
exact_inspect "$NATIVE_INTEGER_FORMAT_CORE_MODULE" 'profile=portable' 'Native\u02C9x64\u02C9integer\u02C9format\u02C9service\u02C9build' 'section name=exports offset=11480 bytes=57 count=1'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Integer-Format-Services.wvproj" \
    "$NATIVE_INTEGER_FORMAT_BRIDGE_MODULE" \
    11598 \
    851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9 \
    00002d4e \
    'build status=Published verification=compiler-aligned functions=12 code-bytes=9654 module-bytes=11598'
exact_inspect "$NATIVE_INTEGER_FORMAT_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=11531 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Service-Code-Builder.wvproj" \
    "$NATIVE_SERVICE_CODE_BUILDER_MODULE" \
    4135 \
    adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06 \
    00001027 \
    'build status=Published verification=compiler-aligned functions=12 code-bytes=2440 module-bytes=4135'
exact_inspect \
    "$NATIVE_SERVICE_CODE_BUILDER_MODULE" \
    'profile=portable' \
    'Native\u02C9x64\u02C9service\u02C9builder' \
    'Native\u02C9x64\u02C9service\u02C9finish' \
    'section name=exports offset=3663 bytes=401 count=10'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Output-Service-Windows.wvproj" \
    "$NATIVE_WINDOWS_OUTPUT_CORE_MODULE" \
    9435 \
    a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983 \
    000024db \
    'build status=Published verification=compiler-aligned functions=15 code-bytes=7347 module-bytes=9435'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Output-Service-Linux.wvproj" \
    "$NATIVE_LINUX_OUTPUT_CORE_MODULE" \
    8908 \
    d3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad \
    000022cc \
    'build status=Published verification=compiler-aligned functions=14 code-bytes=6941 module-bytes=8908'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Output-Services.wvproj" \
    "$NATIVE_OUTPUT_BRIDGE_MODULE" \
    14930 \
    209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed \
    00003a52 \
    'build status=Published verification=compiler-aligned functions=18 code-bytes=12050 module-bytes=14930'
exact_inspect "$NATIVE_OUTPUT_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=14863 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Output-Service-Code.wvproj" \
    "$NATIVE_FILE_OUTPUT_CODE_MODULE" \
    6576 \
    7ed9baf3a21912933045b99cb82d22d73620a318a716931db86670e5ea2212c6 \
    000019b0 \
    'build status=Published verification=compiler-aligned functions=18 code-bytes=4463 module-bytes=6576'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Output-Service-Linux.wvproj" \
    "$NATIVE_LINUX_FILE_OUTPUT_CORE_MODULE" \
    18658 \
    834d0c45b85b26ffd3ee43e49a85c8c4ffa08f36581c02785729b276eeccdb48 \
    000048e2 \
    'build status=Published verification=compiler-aligned functions=21 code-bytes=14933 module-bytes=18658'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Output-Service-Windows.wvproj" \
    "$NATIVE_WINDOWS_FILE_OUTPUT_CORE_MODULE" \
    21129 \
    9ca03bf6f5b8678389c81e281438160ff4c96c86f11a048aba90238fdc81a45d \
    00005289 \
    'build status=Published verification=compiler-aligned functions=22 code-bytes=16956 module-bytes=21129'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Output-Services.wvproj" \
    "$NATIVE_FILE_OUTPUT_BRIDGE_MODULE" \
    33437 \
    441db0e0e5a90f98c7e4b12b17086f56487e7d754d7b6378a0eb2972591e64f6 \
    0000829d \
    'build status=Published verification=compiler-aligned functions=26 code-bytes=27468 module-bytes=33437'
exact_inspect "$NATIVE_FILE_OUTPUT_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=33370 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Input-Service-Code.wvproj" \
    "$NATIVE_FILE_INPUT_CODE_MODULE" \
    7869 \
    e2bfd4521b8f22529f3747eef196bdf7fa7aa0e97644db23ed45939aa10a1a7a \
    00001ebd \
    'build status=Published verification=compiler-aligned functions=20 code-bytes=5317 module-bytes=7869'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Input-Service-Linux.wvproj" \
    "$NATIVE_LINUX_FILE_INPUT_CORE_MODULE" \
    26718 \
    04533e8ecade1f29e0b706c75ec949f5b4c300074cfd65feacb86f5107dcaeba \
    0000685e \
    'build status=Published verification=compiler-aligned functions=26 code-bytes=21582 module-bytes=26718'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Input-Service-Windows.wvproj" \
    "$NATIVE_WINDOWS_FILE_INPUT_CORE_MODULE" \
    32085 \
    6155c4ebb8f4ea76a5d1f22c1bb788aec51e731ceb4a1c5a4ceb7551ba8f409a \
    00007d55 \
    'build status=Published verification=compiler-aligned functions=28 code-bytes=25972 module-bytes=32085'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-File-Input-Services.wvproj" \
    "$NATIVE_FILE_INPUT_BRIDGE_MODULE" \
    51341 \
    09f73787a909ae35ebc1aefb05bd88e4282ff8db7152d196f83b2798ea7c2234 \
    0000c88d \
    'build status=Published verification=compiler-aligned functions=35 code-bytes=42279 module-bytes=51341'
exact_inspect "$NATIVE_FILE_INPUT_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=51274 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Text-Concat-Service-Core.wvproj" \
    "$NATIVE_TEXT_CONCAT_CORE_MODULE" \
    10253 \
    6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73 \
    0000280d \
    'build status=Published verification=compiler-aligned functions=14 code-bytes=8082 module-bytes=10253'
exact_inspect "$NATIVE_TEXT_CONCAT_CORE_MODULE" 'profile=portable' 'Native\u02C9x64\u02C9text\u02C9concat\u02C9service\u02C9build' 'section name=exports offset=10149 bytes=54 count=1'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Text-Concat-Service.wvproj" \
    "$NATIVE_TEXT_CONCAT_BRIDGE_MODULE" \
    10232 \
    87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08 \
    000027f8 \
    'build status=Published verification=compiler-aligned functions=15 code-bytes=8098 module-bytes=10232'
exact_inspect "$NATIVE_TEXT_CONCAT_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=10165 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Text-Quote-Service-Core.wvproj" \
    "$NATIVE_TEXT_QUOTE_CORE_MODULE" \
    1471 \
    b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453 \
    000005bf \
    'build status=Published verification=compiler-aligned functions=1 code-bytes=16 module-bytes=1471'
exact_inspect "$NATIVE_TEXT_QUOTE_CORE_MODULE" 'profile=portable' 'data index=0 name="Native\u02C9x64\u02C9text\u02C9quote\u02C9leaf" type=bytes bytes=1165' 'Native\u02C9x64\u02C9text\u02C9quote\u02C9service\u02C9build' 'section name=exports offset=1406 bytes=53 count=1'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Text-Quote-Service.wvproj" \
    "$NATIVE_TEXT_QUOTE_BRIDGE_MODULE" \
    1435 \
    306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631 \
    0000059b \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=32 module-bytes=1435'
exact_inspect "$NATIVE_TEXT_QUOTE_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=1406 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Enum-Name-Service-Core.wvproj" \
    "$NATIVE_ENUM_NAME_CORE_MODULE" \
    625 \
    b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948 \
    00000271 \
    'build status=Published verification=compiler-aligned functions=1 code-bytes=16 module-bytes=625'
exact_inspect "$NATIVE_ENUM_NAME_CORE_MODULE" 'profile=portable' 'data index=0 name="Native\u02C9x64\u02C9enum\u02C9name\u02C9leaf" type=bytes bytes=323' 'Native\u02C9x64\u02C9enum\u02C9name\u02C9service\u02C9build' 'section name=exports offset=561 bytes=52 count=1'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-X64-Enum-Name-Service.wvproj" \
    "$NATIVE_ENUM_NAME_BRIDGE_MODULE" \
    592 \
    46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c \
    00000250 \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=32 module-bytes=592'
exact_inspect "$NATIVE_ENUM_NAME_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=0 result=bytes' 'section name=exports offset=563 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Enum-Metadata-Core.wvproj" \
    "$NATIVE_ENUM_METADATA_CORE_MODULE" \
    15414 \
    8f22e1ba56985fc5a330fcb73cda84456ecc3ef51f9ddffd6bc2edd740f73659 \
    00003c36 \
    'build status=Published verification=compiler-aligned functions=17 code-bytes=13480 module-bytes=15414'
exact_inspect "$NATIVE_ENUM_METADATA_CORE_MODULE" 'profile=portable' 'Native\u02C9enum\u02C9metadata\u02C9build' 'section name=exports offset=15294 bytes=42 count=1'
exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Enum-Metadata.wvproj" \
    "$NATIVE_ENUM_METADATA_BRIDGE_MODULE" \
    15292 \
    052be4402df26ed542107d666ed894cadb04a46ba6b2428bafc9f1879e38a072 \
    00003bbc \
    'build status=Published verification=compiler-aligned functions=18 code-bytes=13511 module-bytes=15292'
exact_inspect "$NATIVE_ENUM_METADATA_BRIDGE_MODULE" 'profile=portable' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=15221 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Core.wvproj" \
    "$NATIVE_PUBLICATION_MODULE" \
    7190 \
    3048902ce708d6e640d484507efc1d567399bcafed6e2c133ca2827aff83189f \
    00001c16 \
    'build status=Published verification=compiler-aligned functions=8 code-bytes=5333 module-bytes=7190'
exact_inspect "$NATIVE_PUBLICATION_MODULE" 'profile=portable' 'Native\u02C9publication\u02C9result' 'Native\u02C9publication\u02C9status' 'Native\u02C9publication\u02C9plan' 'section name=exports offset=6507 bytes=336 count=8'
exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication.wvproj" \
    "$NATIVE_PUBLICATION_BRIDGE_MODULE" \
    6758 \
    111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c \
    00001a66 \
    'build status=Published verification=compiler-aligned functions=9 code-bytes=5399 module-bytes=6758'
exact_inspect "$NATIVE_PUBLICATION_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=71 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=6432 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Service-Bundle-Materialization-Core.wvproj" \
    "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_MODULE" \
    17185 \
    97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008 \
    00004321 \
    'build status=Published verification=compiler-aligned functions=19 code-bytes=14253 module-bytes=17185'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Service-Bundle-Materialization.wvproj" \
    "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_MODULE" \
    17150 \
    327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902 \
    000042fe \
    'build status=Published verification=compiler-aligned functions=20 code-bytes=14319 module-bytes=17150'
exact_inspect "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=91 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=16693 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Output-Table-Core.wvproj" \
    "$NATIVE_OUTPUT_TABLE_CORE_MODULE" \
    4710 \
    ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8 \
    00001266 \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4002 module-bytes=4710'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Output-Table.wvproj" \
    "$NATIVE_OUTPUT_TABLE_BRIDGE_MODULE" \
    4714 \
    b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8 \
    0000126a \
    'build status=Published verification=compiler-aligned functions=8 code-bytes=4033 module-bytes=4714'
exact_inspect "$NATIVE_OUTPUT_TABLE_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=72 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=4685 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-File-Output-Table-Core.wvproj" \
    "$NATIVE_FILE_OUTPUT_TABLE_CORE_MODULE" \
    3926 \
    fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f \
    00000f56 \
    'build status=Published verification=compiler-aligned functions=6 code-bytes=3293 module-bytes=3926'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-File-Output-Table.wvproj" \
    "$NATIVE_FILE_OUTPUT_TABLE_BRIDGE_MODULE" \
    3930 \
    94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06 \
    00000f5a \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3324 module-bytes=3930'
exact_inspect "$NATIVE_FILE_OUTPUT_TABLE_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=78 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=3901 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-File-Input-Table-Core.wvproj" \
    "$NATIVE_FILE_INPUT_TABLE_CORE_MODULE" \
    5078 \
    0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438 \
    000013d6 \
    'build status=Published verification=compiler-aligned functions=6 code-bytes=4381 module-bytes=5078'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-File-Input-Table.wvproj" \
    "$NATIVE_FILE_INPUT_TABLE_BRIDGE_MODULE" \
    5084 \
    e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9 \
    000013dc \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4412 module-bytes=5084'
exact_inspect "$NATIVE_FILE_INPUT_TABLE_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=77 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=5055 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Service-Table-Core.wvproj" \
    "$NATIVE_SERVICE_TABLE_CORE_MODULE" \
    3065 \
    ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26 \
    00000bf9 \
    'build status=Published verification=compiler-aligned functions=6 code-bytes=2492 module-bytes=3065'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Service-Table.wvproj" \
    "$NATIVE_SERVICE_TABLE_BRIDGE_MODULE" \
    3079 \
    04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b \
    00000c07 \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=2523 module-bytes=3079'
exact_inspect "$NATIVE_SERVICE_TABLE_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=73 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=3050 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Execution-Context-Core.wvproj" \
    "$NATIVE_EXECUTION_CONTEXT_CORE_MODULE" \
    5530 \
    dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b \
    0000159a \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4767 module-bytes=5530'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Execution-Context.wvproj" \
    "$NATIVE_EXECUTION_CONTEXT_BRIDGE_MODULE" \
    5531 \
    86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68 \
    0000159b \
    'build status=Published verification=compiler-aligned functions=8 code-bytes=4798 module-bytes=5531'
exact_inspect "$NATIVE_EXECUTION_CONTEXT_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=77 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=5502 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Argument-Table-Core.wvproj" \
    "$NATIVE_ARGUMENT_TABLE_CORE_MODULE" \
    4362 \
    08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75 \
    0000110a \
    'build status=Published verification=compiler-aligned functions=6 code-bytes=3707 module-bytes=4362'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Argument-Table.wvproj" \
    "$NATIVE_ARGUMENT_TABLE_BRIDGE_MODULE" \
    4374 \
    080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788 \
    00001116 \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3738 module-bytes=4374'
exact_inspect "$NATIVE_ARGUMENT_TABLE_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=74 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=4345 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Entry-Bridge-Core.wvproj" \
    "$NATIVE_ENTRY_BRIDGE_CORE_MODULE" \
    3385 \
    8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae \
    00000d39 \
    'build status=Published verification=compiler-aligned functions=6 code-bytes=2799 module-bytes=3385'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Entry-Bridge.wvproj" \
    "$NATIVE_ENTRY_BRIDGE_BRIDGE_MODULE" \
    3401 \
    d66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1 \
    00000d49 \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=2830 module-bytes=3401'
exact_inspect "$NATIVE_ENTRY_BRIDGE_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=72 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=3372 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Byte-Result-Admission-Core.wvproj" \
    "$NATIVE_BYTE_RESULT_ADMISSION_CORE_MODULE" \
    7078 \
    eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04 \
    00001ba6 \
    'build status=Published verification=compiler-aligned functions=10 code-bytes=6085 module-bytes=7078'
exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Byte-Result-Admission.wvproj" \
    "$NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_MODULE" \
    7057 \
    9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf \
    00001b91 \
    'build status=Published verification=compiler-aligned functions=11 code-bytes=6116 module-bytes=7057'
exact_inspect "$NATIVE_BYTE_RESULT_ADMISSION_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=82 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=7028 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wvproj" \
    "$NATIVE_HOSTED_TOOL_METADATA_ADMISSION_MODULE" \
    10872 \
    d7b0084ed2c69ee03ad65ee4bfffa72550fd8d9ef2889efa0be116350b80b8b5 \
    00002a78 \
    'build status=Published verification=compiler-aligned functions=13 code-bytes=9503 module-bytes=10872'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Tool-Metadata-Construction-Core.wvproj" \
    "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_CORE_MODULE" \
    24360 \
    5808f778eb21c1214b581f0ce03958a74173a801b886aec7ed32124d7446abcd \
    00005f28 \
    'build status=Published verification=compiler-aligned functions=35 code-bytes=21363 module-bytes=24360'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Tool-Metadata.wvproj" \
    "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE" \
    24252 \
    b5e9397326d3106b22ce735369ef8202ff6bb4c8e14f6069a0c467b4266c8208 \
    00005ebc \
    'build status=Published verification=compiler-aligned functions=36 code-bytes=21394 module-bytes=24252'
exact_inspect "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=95 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=24186 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Linker/Windvale/Native-Hosted-Startup-Instantiation.wvproj" \
    "$NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE" \
    21143 \
    933864be78b28394b9fc8e495b5ac872311ebca2a624db6e6731cdb8b399d309 \
    00005297 \
    'build status=Published verification=compiler-aligned functions=15 code-bytes=18808 module-bytes=21143'
exact_inspect "$NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE" 'profile=portable' 'section name=capabilities offset=88 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=20924 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Container-Construction.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_PLAN_MODULE" \
    35929 \
    ff1b48cfc05baab5f707dcfce7e73b0714e2379ee594e12f6e9c6ea1589fef7e \
    00008c59 \
    'build status=Published verification=compiler-aligned functions=41 code-bytes=31210 module-bytes=35929'
exact_inspect "$NATIVE_HOSTED_CONTAINER_PLAN_MODULE" 'profile=portable' 'section name=capabilities offset=81 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=35311 bytes=17 count=1'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Container-Windows.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE" \
    17679 \
    a77e4ea3ac2cff35e965ae44cd486f30dd5b0c10aa2cde23c109d0eca37bffcb \
    0000450f \
    'build status=Published verification=compiler-aligned functions=22 code-bytes=15041 module-bytes=17679'
exact_inspect "$NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE" 'profile=portable' 'section name=capabilities offset=76 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=17613 bytes=17 count=1'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Container-Linux.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_LINUX_MODULE" \
    12328 \
    dac93155c68ba18f6cbe3af2d301a4c4171b9a9c05841057ea57398536fa8b42 \
    00003028 \
    'build status=Published verification=compiler-aligned functions=19 code-bytes=10674 module-bytes=12328'
exact_inspect "$NATIVE_HOSTED_CONTAINER_LINUX_MODULE" 'profile=portable' 'section name=capabilities offset=74 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=12262 bytes=17 count=1'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Container-Segmentation.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE" \
    22584 \
    d6d74f7d27df9f04f02b8eac2e75fde4fc230ba70d198f90b31ad668a06052e6 \
    00005838 \
    'build status=Published verification=compiler-aligned functions=28 code-bytes=19181 module-bytes=22584'
exact_inspect "$NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE" 'profile=portable' 'section name=capabilities offset=81 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=21891 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Tool-Runtime-Header-Core.wvproj" \
    "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_CORE_MODULE" \
    19516 \
    f1c156def9fa6f00bb0401097435bb1d1429d9d4be247b8d11f0de0b5ea51be2 \
    00004c3c \
    'build status=Published verification=compiler-aligned functions=29 code-bytes=17050 module-bytes=19516'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Native-Hosted-Tool-Runtime-Header.wvproj" \
    "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_MODULE" \
    19459 \
    3cc8d0850b888911ee3338600bc7699578b163e7400c2b3631ef14649b9a3f18 \
    00004c03 \
    'build status=Published verification=compiler-aligned functions=30 code-bytes=17081 module-bytes=19459'
exact_inspect "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=88 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=19393 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Lifetime-Core.wvproj" \
    "$NATIVE_PUBLICATION_LIFETIME_CORE_MODULE" \
    4955 \
    a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3 \
    0000135b \
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3358 module-bytes=4955'
exact_inspect "$NATIVE_PUBLICATION_LIFETIME_CORE_MODULE" 'profile=portable' 'Native\u02C9publication\u02C9lifetime\u02C9result' 'Native\u02C9publication\u02C9lifetime\u02C9status' 'Native\u02C9publication\u02C9lifetime\u02C9plan' 'section name=exports offset=4321 bytes=358 count=7'
exact_build \
    "$REPOSITORY_ROOT/Compiler/Windvale/Native-Publication-Lifetime.wvproj" \
    "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE" \
    4442 \
    f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554 \
    0000115a \
    'build status=Published verification=compiler-aligned functions=8 code-bytes=3424 module-bytes=4442'
exact_inspect "$NATIVE_PUBLICATION_LIFETIME_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=81 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=4207 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Lexer-Core.wvproj" \
    "$SOURCE_LEXER_MODULE" \
    49470 \
    411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e \
    0000c13e \
    'build status=Published verification=compiler-aligned functions=20 code-bytes=40152 module-bytes=49470'
exact_inspect "$SOURCE_LEXER_MODULE" 'profile=portable' 'section name=exports offset=46433 bytes=715 count=17' 'section name=types offset=47156 bytes=2314 count=7' 'Compiler\u02C9source\u02C9token' 'Compiler\u02C9token\u02C9kind' 'Compiler\u02C9lex\u02C9source\u02C9bounded'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Lexer-Demo.wvproj" \
    "$SOURCE_LEXER_DEMO_MODULE" \
    56674 \
    f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db \
    0000dd62 \
    'build status=Published verification=compiler-aligned functions=21 code-bytes=46427 module-bytes=56674'

exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Declaration-Parser.wvproj" \
    "$SOURCE_DECLARATION_PARSER_MODULE" \
    151197 \
    8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb \
    00024e9d \
    'build status=Published verification=compiler-aligned functions=52 code-bytes=120804 module-bytes=151197'
exact_inspect "$SOURCE_DECLARATION_PARSER_MODULE" 'profile=portable' 'section name=exports offset=145507 bytes=1417 count=32' 'section name=types offset=146932 bytes=4265 count=15' 'Compiler\u02C9source\u02C9declaration' 'Compiler\u02C9source\u02C9module\u02C9summary' 'Compiler\u02C9parse\u02C9next\u02C9declaration\u02C9validated'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Declaration-Parser-Demo.wvproj" \
    "$SOURCE_DECLARATION_PARSER_DEMO_MODULE" \
    154365 \
    9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf \
    00025afd \
    'build status=Published verification=compiler-aligned functions=53 code-bytes=124556 module-bytes=154365'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Declaration-Parser-Tool.wvproj" \
    "$SOURCE_DECLARATION_PARSER_TOOL_MODULE" \
    151731 \
    ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0 \
    000250b3 \
    'build status=Published verification=compiler-aligned functions=55 code-bytes=122750 module-bytes=151731'

exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Body-Parser.wvproj" \
    "$SOURCE_BODY_PARSER_MODULE" \
    248663 \
    68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589 \
    0003cb57 \
    'build status=Published verification=compiler-aligned functions=100 code-bytes=197096 module-bytes=248663'
exact_inspect "$SOURCE_BODY_PARSER_MODULE" 'profile=portable' 'section name=exports offset=239096 bytes=2112 count=47' 'section name=types offset=241216 bytes=7447 count=25' 'Compiler\u02C9source\u02C9expression' 'Compiler\u02C9source\u02C9statement' 'Compiler\u02C9parse\u02C9expression\u02C9validated' 'Compiler\u02C9parse\u02C9source\u02C9bodies'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Body-Parser-Demo.wvproj" \
    "$SOURCE_BODY_PARSER_DEMO_MODULE" \
    254805 \
    2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f \
    0003e355 \
    'build status=Published verification=compiler-aligned functions=101 code-bytes=204515 module-bytes=254805'
exact_build \
    "$REPOSITORY_ROOT/Windvale-Source-Body-Parser-Tool.wvproj" \
    "$SOURCE_BODY_PARSER_TOOL_MODULE" \
    247844 \
    0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f \
    0003c824 \
    'build status=Published verification=compiler-aligned functions=103 code-bytes=198924 module-bytes=247844'

TEMPORARY_DIRECTORY=$(mktemp -d "${TMPDIR:-/tmp}/windvale-seed-front-door.XXXXXX")
cleanup() {
    case "$TEMPORARY_DIRECTORY" in
        "${TMPDIR:-/tmp}"/windvale-seed-front-door.*)
            rm -rf -- "$TEMPORARY_DIRECTORY"
            ;;
        *)
            echo 'Refusing to remove an unexpected native Seed temporary directory.' >&2
            exit 1
            ;;
    esac
}
trap cleanup EXIT HUP INT TERM

INVALID_PROJECT="$TEMPORARY_DIRECTORY/Invalid.wvproj"
EXISTING_OUTPUT="$TEMPORARY_DIRECTORY/Existing.wvb"
printf '%s\n' \
    'windvale-project 1' \
    'root "Missing.wv"' > "$INVALID_PROJECT"
printf '\011\010\007' > "$EXISTING_OUTPUT"
set +e
INVALID_OUTPUT=$("$NATIVE_BUILD" "$INVALID_PROJECT" "$EXISTING_OUTPUT" 2>&1)
INVALID_EXIT=$?
set -e
if [ "$INVALID_EXIT" -ne 1 ] || \
   [ "$INVALID_OUTPUT" != 'build status=Projectˉrejected code=WVP1004 line=3 column=1' ] || \
   [ "$(od -An -tx1 -v "$EXISTING_OUTPUT" | tr -d ' \n')" != '090807' ]; then
    echo 'The native Seed project rejection or output preservation contract failed.' >&2
    exit 1
fi

echo 'native Seed front-door verification status=Complete artifacts=79 cases=132'
