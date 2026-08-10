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

echo 'native Seed front-door verification status=Complete artifacts=12 cases=24'
