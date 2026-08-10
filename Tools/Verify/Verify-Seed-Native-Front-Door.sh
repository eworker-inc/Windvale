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

echo 'native Seed front-door verification status=Complete artifacts=31 cases=53'
