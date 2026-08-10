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
    if ! INSPECT_OUTPUT=$("$NATIVE_INSPECT" "$1"); then
        echo "The native Seed inspector rejected: $1" >&2
        exit 1
    fi
    printf '%s\n' "$INSPECT_OUTPUT" | grep -F "$2" >/dev/null
}

SUM_MODULE="$OUTPUT_ROOT/Sum-Data.wvb"
HELLO_MODULE="$OUTPUT_ROOT/Hello-Windvale.wvb"
FOUNDATION_MODULE="$OUTPUT_ROOT/Read-Wvb-Header.wvb"
COMPOSITION_MODULE="$OUTPUT_ROOT/Module-Composition-Demo-Project.wvb"

exact_build \
    "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wvproj" \
    "$SUM_MODULE" \
    494 \
    76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df \
    000001ee \
    'build status=Published verification=compiler-aligned functions=2 code-bytes=270 module-bytes=494'
exact_verify "$SUM_MODULE"
exact_inspect "$SUM_MODULE" 'opcode=data.load.i32 operand=0'

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

exact_build \
    "$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Demo.wvproj" \
    "$COMPOSITION_MODULE" \
    660 \
    030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607 \
    00000294 \
    'build status=Published verification=compiler-aligned functions=4 code-bytes=280 module-bytes=660'

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

echo 'native Seed front-door verification status=Complete artifacts=4 cases=5'
