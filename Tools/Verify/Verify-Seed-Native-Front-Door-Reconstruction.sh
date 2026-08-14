#!/usr/bin/env sh
set -eu

if [ "$#" -ne 1 ] || [ ! -d "$1" ]; then
    echo 'Usage: Tools/Verify/Verify-Seed-Native-Front-Door-Reconstruction.sh <output-directory>' >&2
    exit 64
fi

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
OUTPUT_ROOT=$(CDPATH= cd -- "$1" && pwd)
NATIVE_BUILD="$REPOSITORY_ROOT/Tools/Native/Build-Wvb.sh"
NATIVE_SOURCE_COMPILER_BUILD="$REPOSITORY_ROOT/Tools/Native/Build-Source-Compiler-Product.sh"
NATIVE_VERIFY="$REPOSITORY_ROOT/Tools/Native/Verify-Wvb.sh"
NATIVE_INSPECT="$REPOSITORY_ROOT/Tools/Native/Inspect-Wvb.sh"
NATIVE_RUN="$REPOSITORY_ROOT/Tools/Native/Run-Wvb.sh"
NATIVE_ASSEMBLER="$REPOSITORY_ROOT/Tools/Native/Assemble-Wva.sh"
NATIVE_WVO_VERIFY="$REPOSITORY_ROOT/Tools/Native/Verify-Wvo.sh"
NATIVE_WVO_INSPECT="$REPOSITORY_ROOT/Tools/Native/Inspect-Wvo.sh"
NATIVE_WVO_APPLICATION="$REPOSITORY_ROOT/Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.elf"
NATIVE_WVDUMP_APPLICATION="$REPOSITORY_ROOT/Artifacts/Native-Front-Door/linux-x64/wvdump.elf"
NATIVE_WVA_APPLICATION="$REPOSITORY_ROOT/Artifacts/Native-Front-Door/linux-x64/wvasm.elf"
NATIVE_LINKER="$REPOSITORY_ROOT/Tools/Native/Link-Wvo.sh"
NATIVE_LINKER_APPLICATION="$REPOSITORY_ROOT/Artifacts/Native-Wv-Linker-Candidate/Wv-Linker.elf"

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

exact_source_compiler_build() {
    PRODUCT=$1
    OUTPUT_PATH=$2
    EXPECTED_BYTES=$3
    EXPECTED_SHA256=$4
    EXPECTED_HEX_BYTES=$5
    EXPECTED_COMPILER_REPORT=$6
    if ! BUILD_OUTPUT=$("$NATIVE_SOURCE_COMPILER_BUILD" "$PRODUCT" "$OUTPUT_PATH"); then
        echo "The native source compiler product build failed: $PRODUCT" >&2
        exit 1
    fi
    EXPECTED_OUTPUT=$(printf '%s\n%s' \
        "$EXPECTED_COMPILER_REPORT" \
        "publication status=Complete bytes=0x$EXPECTED_HEX_BYTES sha256=$EXPECTED_SHA256")
    if [ "$BUILD_OUTPUT" != "$EXPECTED_OUTPUT" ]; then
        echo "The native source compiler product report is invalid: $PRODUCT" >&2
        exit 1
    fi
    ACTUAL_BYTES=$(wc -c < "$OUTPUT_PATH" | tr -d ' ')
    ACTUAL_SHA256=$(sha256sum "$OUTPUT_PATH" | awk '{print $1}')
    if [ "$ACTUAL_BYTES" != "$EXPECTED_BYTES" ] || [ "$ACTUAL_SHA256" != "$EXPECTED_SHA256" ]; then
        echo "The native source compiler product is unexpected: $OUTPUT_PATH" >&2
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

exact_inspect_without() {
    MODULE_PATH=$1
    FORBIDDEN_PATTERN=$2
    shift 2
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
    if printf '%s\n' "$INSPECT_OUTPUT" | grep -F "$FORBIDDEN_PATTERN" >/dev/null; then
        echo "The native Seed inspector exposed forbidden evidence: $MODULE_PATH" >&2
        exit 1
    fi
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

exact_wvdump_execution() {
    SUM_PATH=$1
    INVALID_PATH=$2
    APPLICATION_BYTES=$(wc -c < "$NATIVE_WVDUMP_APPLICATION" | tr -d ' ')
    APPLICATION_SHA256=$(sha256sum "$NATIVE_WVDUMP_APPLICATION" | awk '{print $1}')
    if [ "$APPLICATION_BYTES" != 794624 ] || \
        [ "$APPLICATION_SHA256" != 'd3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627' ]; then
        echo 'The Linux native WvDump application identity is invalid.' >&2
        exit 1
    fi
    ERROR_FILE=$(mktemp "${TMPDIR:-/tmp}/windvale-seed-wvdump.XXXXXX")
    if ! SELF_OUTPUT=$("$NATIVE_WVDUMP_APPLICATION" 2>"$ERROR_FILE"); then
        rm -f -- "$ERROR_FILE"
        echo 'The digest-bound native WvDump self-test failed.' >&2
        exit 1
    fi
    if [ -n "$SELF_OUTPUT" ] || [ -s "$ERROR_FILE" ]; then
        rm -f -- "$ERROR_FILE"
        echo 'The digest-bound native WvDump self-test emitted unexpected output.' >&2
        exit 1
    fi

    SUM_SHA256=$(sha256sum "$SUM_PATH" | awk '{print $1}')
    if ! REPORT_OUTPUT=$("$NATIVE_WVDUMP_APPLICATION" "$SUM_PATH" 2>"$ERROR_FILE"); then
        rm -f -- "$ERROR_FILE"
        echo 'The digest-bound native WvDump rejected the canonical module.' >&2
        exit 1
    fi
    if [ -s "$ERROR_FILE" ]; then
        rm -f -- "$ERROR_FILE"
        echo 'The digest-bound native WvDump emitted an unexpected diagnostic.' >&2
        exit 1
    fi
    for REQUIRED_LINE in \
        'wvdump 1' \
        'module version=1.11 profile=portable name="Sum\u02C9data"' \
        'data index=0 name="Values" type=i32_array elements=4' \
        'instruction function=1 offset=141 opcode=call operand=0' \
        'export index=0 name="Main" kind=function target=1'; do
        if ! printf '%s\n' "$REPORT_OUTPUT" | grep -F -x "$REQUIRED_LINE" >/dev/null; then
            rm -f -- "$ERROR_FILE"
            echo 'The digest-bound native WvDump report omitted required evidence.' >&2
            exit 1
        fi
    done
    if [ "$(sha256sum "$SUM_PATH" | awk '{print $1}')" != "$SUM_SHA256" ]; then
        rm -f -- "$ERROR_FILE"
        echo 'The digest-bound native WvDump modified the canonical module.' >&2
        exit 1
    fi

    INVALID_SHA256=$(sha256sum "$INVALID_PATH" | awk '{print $1}')
    set +e
    INVALID_OUTPUT=$("$NATIVE_WVDUMP_APPLICATION" "$INVALID_PATH" 2>&1)
    INVALID_EXIT=$?
    set -e
    rm -f -- "$ERROR_FILE"
    if [ "$INVALID_EXIT" -ne 2 ] || [ "$INVALID_OUTPUT" != 'Badˉmagic sections=0 offset=0' ] || \
        [ "$(sha256sum "$INVALID_PATH" | awk '{print $1}')" != "$INVALID_SHA256" ]; then
        echo 'The digest-bound native WvDump invalid-file contract failed.' >&2
        exit 1
    fi
}

exact_wvo_read_only_execution() {
    OBJECT_PATH=$1
    APPLICATION_BYTES=$(wc -c < "$NATIVE_WVO_APPLICATION" | tr -d ' ')
    APPLICATION_SHA256=$(sha256sum "$NATIVE_WVO_APPLICATION" | awk '{print $1}')
    if [ "$APPLICATION_BYTES" != 1036288 ] || \
        [ "$APPLICATION_SHA256" != 'fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840' ]; then
        echo 'The Linux native WVO inspector application identity is invalid.' >&2
        exit 1
    fi
    if ! SELF_TEST_OUTPUT=$("$NATIVE_WVO_APPLICATION"); then
        echo 'The digest-bound native WVO inspector self-test failed.' >&2
        exit 1
    fi
    if [ -n "$SELF_TEST_OUTPUT" ]; then
        echo 'The digest-bound native WVO inspector self-test emitted unexpected output.' >&2
        exit 1
    fi

    ASSEMBLY_SOURCE="$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva"
    if ! ASSEMBLY_OUTPUT=$("$NATIVE_ASSEMBLER" "$ASSEMBLY_SOURCE" "$OBJECT_PATH"); then
        echo 'The digest-bound native WVA assembler did not construct the WVO read-only fixture.' >&2
        exit 1
    fi
    EXPECTED_ASSEMBLY_OUTPUT=$(printf '%s\n%s' \
        'wvasm 1' \
        'assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1')
    OBJECT_BYTES=$(wc -c < "$OBJECT_PATH" | tr -d ' ')
    OBJECT_SHA256=$(sha256sum "$OBJECT_PATH" | awk '{print $1}')
    if [ "$ASSEMBLY_OUTPUT" != "$EXPECTED_ASSEMBLY_OUTPUT" ] || \
        [ "$OBJECT_BYTES" != 218 ] || \
        [ "$OBJECT_SHA256" != '992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85' ]; then
        echo 'The native WVO read-only fixture has an unexpected report or identity.' >&2
        exit 1
    fi

    if ! VERIFY_OUTPUT=$("$NATIVE_WVO_VERIFY" "$OBJECT_PATH"); then
        echo 'The digest-bound native WVO verifier rejected the canonical fixture.' >&2
        exit 1
    fi
    EXPECTED_VERIFY_OUTPUT=$(printf '%s\n%s' \
        'Verified object: X86ˉ64' \
        "SHA-256: $OBJECT_SHA256")
    if [ "$VERIFY_OUTPUT" != "$EXPECTED_VERIFY_OUTPUT" ]; then
        echo 'The digest-bound native WVO verifier report is invalid.' >&2
        exit 1
    fi

    if ! INSPECT_OUTPUT=$("$NATIVE_WVO_INSPECT" "$OBJECT_PATH"); then
        echo 'The digest-bound native WVO inspector rejected the canonical fixture.' >&2
        exit 1
    fi
    for REQUIRED_LINE in \
        'Windvale object 1.0' \
        'Architecture: X86ˉ64' \
        'Sections (2)' \
        '  [2] Console_write binding=Import kind=Function section=undefined offset=0 size=0' \
        '  [0] kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4'; do
        if ! printf '%s\n' "$INSPECT_OUTPUT" | grep -F -x "$REQUIRED_LINE" >/dev/null; then
            echo 'The digest-bound native WVO inspection omitted required evidence.' >&2
            exit 1
        fi
    done
    if [ "$(sha256sum "$OBJECT_PATH" | awk '{print $1}')" != "$OBJECT_SHA256" ]; then
        echo 'The digest-bound native WVO inspector modified its input.' >&2
        exit 1
    fi
}

exact_wva_and_linker_execution() {
    OBJECT_PATH=$1
    INVALID_SOURCE_PATH=$2
    PROVIDER_OBJECT_PATH=$3
    LINKED_IMAGE_PATH=$4
    LINK_MAP_PATH=$5
    INVALID_ASSEMBLY_PATH=$6
    INVALID_LINK_PATH=$7

    ASSEMBLER_BYTES=$(wc -c < "$NATIVE_WVA_APPLICATION" | tr -d ' ')
    ASSEMBLER_SHA256=$(sha256sum "$NATIVE_WVA_APPLICATION" | awk '{print $1}')
    if [ "$ASSEMBLER_BYTES" != 2895872 ] || \
        [ "$ASSEMBLER_SHA256" != '36796a26917e699030e2987c01b74799bcdc339af578f76e02f9a1f47ca10b8c' ]; then
        echo 'The Linux native WVA assembler application identity is invalid.' >&2
        exit 1
    fi
    if ! ASSEMBLER_SELF_OUTPUT=$("$NATIVE_WVA_APPLICATION"); then
        echo 'The digest-bound native WVA assembler self-test failed.' >&2
        exit 1
    fi
    if [ -n "$ASSEMBLER_SELF_OUTPUT" ]; then
        echo 'The digest-bound native WVA assembler self-test emitted unexpected output.' >&2
        exit 1
    fi

    LINKER_BYTES=$(wc -c < "$NATIVE_LINKER_APPLICATION" | tr -d ' ')
    LINKER_SHA256=$(sha256sum "$NATIVE_LINKER_APPLICATION" | awk '{print $1}')
    if [ "$LINKER_BYTES" != 1798144 ] || \
        [ "$LINKER_SHA256" != '8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a' ]; then
        echo 'The Linux native WVO linker application identity is invalid.' >&2
        exit 1
    fi
    if ! LINKER_SELF_OUTPUT=$("$NATIVE_LINKER_APPLICATION"); then
        echo 'The digest-bound native WVO linker self-test failed.' >&2
        exit 1
    fi
    if [ -n "$LINKER_SELF_OUTPUT" ]; then
        echo 'The digest-bound native WVO linker self-test emitted unexpected output.' >&2
        exit 1
    fi

    OBJECT_SHA256=$(sha256sum "$OBJECT_PATH" | awk '{print $1}')
    if ! SCANNER_OUTPUT=$("$NATIVE_LINKER_APPLICATION" "$OBJECT_PATH"); then
        echo 'The digest-bound native WVO linker scanner rejected the canonical object.' >&2
        exit 1
    fi
    if [ "$SCANNER_OUTPUT" != 'object status=Valid sections=2 symbols=3 relocations=2 offset=218' ] || \
        [ "$(sha256sum "$OBJECT_PATH" | awk '{print $1}')" != "$OBJECT_SHA256" ]; then
        echo 'The digest-bound native WVO linker scanner report or preservation contract failed.' >&2
        exit 1
    fi
    INVALID_SOURCE_SHA256=$(sha256sum "$INVALID_SOURCE_PATH" | awk '{print $1}')
    set +e
    INVALID_SCANNER_OUTPUT=$("$NATIVE_LINKER_APPLICATION" "$INVALID_SOURCE_PATH" 2>&1)
    INVALID_SCANNER_EXIT=$?
    set -e
    if [ "$INVALID_SCANNER_EXIT" -ne 2 ] || \
        [ "$INVALID_SCANNER_OUTPUT" != 'object status=Badˉmagic sections=0 symbols=0 relocations=0 offset=0' ] || \
        [ "$(sha256sum "$INVALID_SOURCE_PATH" | awk '{print $1}')" != "$INVALID_SOURCE_SHA256" ]; then
        echo 'The digest-bound native WVO linker scanner invalid-file contract failed.' >&2
        exit 1
    fi

    if [ -e "$INVALID_ASSEMBLY_PATH" ]; then
        echo "The invalid native assembly output unexpectedly exists: $INVALID_ASSEMBLY_PATH" >&2
        exit 1
    fi
    set +e
    INVALID_ASSEMBLY_OUTPUT=$("$NATIVE_WVA_APPLICATION" \
        "$INVALID_SOURCE_PATH" "$INVALID_ASSEMBLY_PATH" 2>&1)
    INVALID_ASSEMBLY_EXIT=$?
    set -e
    if [ "$INVALID_ASSEMBLY_EXIT" -ne 2 ] || \
        [ "$INVALID_ASSEMBLY_OUTPUT" != 'assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1' ] || \
        [ -e "$INVALID_ASSEMBLY_PATH" ]; then
        echo 'The digest-bound native WVA assembler created output for rejected source.' >&2
        exit 1
    fi
    set +e
    EXISTING_ASSEMBLY_OUTPUT=$("$NATIVE_WVA_APPLICATION" \
        "$INVALID_SOURCE_PATH" "$OBJECT_PATH" 2>&1)
    EXISTING_ASSEMBLY_EXIT=$?
    set -e
    if [ "$EXISTING_ASSEMBLY_EXIT" -ne 2 ] || \
        [ "$EXISTING_ASSEMBLY_OUTPUT" != 'assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1' ] || \
        [ "$(sha256sum "$OBJECT_PATH" | awk '{print $1}')" != "$OBJECT_SHA256" ]; then
        echo 'Rejected native assembly modified the canonical object.' >&2
        exit 1
    fi

    PROVIDER_SOURCE="$REPOSITORY_ROOT/Examples/Linker/Console-Provider.wva"
    if ! PROVIDER_OUTPUT=$("$NATIVE_ASSEMBLER" "$PROVIDER_SOURCE" "$PROVIDER_OBJECT_PATH"); then
        echo 'The digest-bound native WVA assembler did not construct the linker provider.' >&2
        exit 1
    fi
    EXPECTED_PROVIDER_OUTPUT=$(printf '%s\n%s' \
        'wvasm 1' \
        'assembly status=valid object-bytes=91 sections=1 symbols=1 relocations=0 offset=163 line=10 column=1')
    PROVIDER_BYTES=$(wc -c < "$PROVIDER_OBJECT_PATH" | tr -d ' ')
    PROVIDER_SHA256=$(sha256sum "$PROVIDER_OBJECT_PATH" | awk '{print $1}')
    if [ "$PROVIDER_OUTPUT" != "$EXPECTED_PROVIDER_OUTPUT" ] || \
        [ "$PROVIDER_BYTES" != 91 ] || \
        [ "$PROVIDER_SHA256" != '486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab' ]; then
        echo 'The native linker provider has an unexpected report or identity.' >&2
        exit 1
    fi

    if ! LINK_OUTPUT=$("$NATIVE_LINKER" 1048576 Main "$LINKED_IMAGE_PATH" \
        "$OBJECT_PATH" "$PROVIDER_OBJECT_PATH"); then
        echo 'The digest-bound native WVO linker rejected the canonical link.' >&2
        exit 1
    fi
    for REQUIRED_LINE in \
        'windvale-link-map 1' \
        'target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24' \
        'entry name=Main address=1048576' \
        'image sha256=0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' \
        'import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592' \
        'relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576'; do
        if ! printf '%s\n' "$LINK_OUTPUT" | grep -F -x "$REQUIRED_LINE" >/dev/null; then
            echo 'The digest-bound native WVO linker map omitted required evidence.' >&2
            exit 1
        fi
    done
    if printf '%s\n' "$LINK_OUTPUT" | grep -F "$REPOSITORY_ROOT" >/dev/null; then
        echo 'The digest-bound native WVO linker map exposed a repository path.' >&2
        exit 1
    fi
    LINKED_BYTES=$(wc -c < "$LINKED_IMAGE_PATH" | tr -d ' ')
    LINKED_SHA256=$(sha256sum "$LINKED_IMAGE_PATH" | awk '{print $1}')
    if [ "$LINKED_BYTES" != 24 ] || \
        [ "$LINKED_SHA256" != '0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a' ]; then
        echo 'The digest-bound native WVO linker wrote unexpected image bytes.' >&2
        exit 1
    fi
    printf '%s\n' "$LINK_OUTPUT" >"$LINK_MAP_PATH"
    LINK_MAP_BYTES=$(wc -c < "$LINK_MAP_PATH" | tr -d ' ')
    LINK_MAP_SHA256=$(sha256sum "$LINK_MAP_PATH" | awk '{print $1}')
    if [ "$LINK_MAP_BYTES" != 1721 ] || \
        [ "$LINK_MAP_SHA256" != '31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4' ]; then
        echo 'The digest-bound native WVO linker wrote an unexpected canonical map.' >&2
        exit 1
    fi

    if [ -e "$INVALID_LINK_PATH" ]; then
        echo "The invalid native link output unexpectedly exists: $INVALID_LINK_PATH" >&2
        exit 1
    fi
    set +e
    UNDEFINED_OUTPUT=$("$NATIVE_LINKER" 1048576 Main "$INVALID_LINK_PATH" \
        "$OBJECT_PATH" 2>&1)
    UNDEFINED_EXIT=$?
    set -e
    if [ "$UNDEFINED_EXIT" -ne 2 ] || \
        [ "$UNDEFINED_OUTPUT" != 'link status=WVL1005 inputs=1 sections=2 symbols=3 relocations=2 image-bytes=0 entry-address=0 input=0' ] || \
        [ -e "$INVALID_LINK_PATH" ]; then
        echo 'The digest-bound native WVO linker created output for an undefined import.' >&2
        exit 1
    fi
    set +e
    EXISTING_LINK_OUTPUT=$("$NATIVE_LINKER" 1048576 Main "$LINKED_IMAGE_PATH" \
        "$OBJECT_PATH" 2>&1)
    EXISTING_LINK_EXIT=$?
    set -e
    if [ "$EXISTING_LINK_EXIT" -ne 2 ] || \
        [ "$EXISTING_LINK_OUTPUT" != 'link status=WVL1005 inputs=1 sections=2 symbols=3 relocations=2 image-bytes=0 entry-address=0 input=0' ] || \
        [ "$(sha256sum "$LINKED_IMAGE_PATH" | awk '{print $1}')" != "$LINKED_SHA256" ]; then
        echo 'A rejected native WVO link modified the existing image.' >&2
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
SOURCE_SET_MODULE="$OUTPUT_ROOT/Source-Set-Core.wvb"
SOURCE_SET_DEMO_MODULE="$OUTPUT_ROOT/Source-Set-Demo.wvb"
SOURCE_SET_TOOL_MODULE="$OUTPUT_ROOT/Source-Set-Tool.wvb"
SOURCE_GRAPH_MODULE="$OUTPUT_ROOT/Source-Graph-Core.wvb"
SOURCE_GRAPH_DEMO_MODULE="$OUTPUT_ROOT/Source-Graph-Demo.wvb"
SOURCE_GRAPH_TOOL_MODULE="$OUTPUT_ROOT/Source-Graph-Tool.wvb"
SOURCE_SYMBOLS_MODULE="$OUTPUT_ROOT/Source-Symbols-Core.wvb"
SOURCE_SYMBOLS_DEMO_MODULE="$OUTPUT_ROOT/Source-Symbols-Demo.wvb"
SOURCE_SYMBOLS_TOOL_MODULE="$OUTPUT_ROOT/Source-Symbols-Tool.wvb"
SOURCE_BINDINGS_MODULE="$OUTPUT_ROOT/Source-Bindings-Core.wvb"
SOURCE_BINDINGS_DEMO_MODULE="$OUTPUT_ROOT/Source-Bindings-Demo.wvb"
SOURCE_BINDINGS_TOOL_MODULE="$OUTPUT_ROOT/Source-Bindings-Tool.wvb"
SOURCE_WIR_MODULE="$OUTPUT_ROOT/Source-Wir-Core.wvb"
SOURCE_WIR_DEMO_MODULE="$OUTPUT_ROOT/Source-Wir-Demo.wvb"
SOURCE_WIR_TOOL_MODULE="$OUTPUT_ROOT/Source-Wir-Tool.wvb"
SOURCE_WVB_MODULE="$OUTPUT_ROOT/Source-Wvb-Core.wvb"
SOURCE_WVB_DEMO_MODULE="$OUTPUT_ROOT/Source-Wvb-Demo.wvb"
SOURCE_WVB_TOOL_MODULE="$OUTPUT_ROOT/Source-Wvb-Tool.wvb"
WVDUMP_CORE_MODULE="$OUTPUT_ROOT/Wv-Dump-Core.wvb"
WVO_CORE_MODULE="$OUTPUT_ROOT/Wvo-Object-Core.wvb"
WVA_ASSEMBLER_MODULE="$OUTPUT_ROOT/Wva-Assembler-Core.wvb"
WVLINK_CORE_MODULE="$OUTPUT_ROOT/Wv-Linker-Core.wvb"
WVO_SAMPLE="$OUTPUT_ROOT/Sample.wvo"
LINK_PROVIDER_OBJECT="$OUTPUT_ROOT/Console-Provider.wvo"
WINDVALE_LINKED_IMAGE="$OUTPUT_ROOT/Hello-Linked-Windvale.bin"
WINDVALE_LINK_MAP="$OUTPUT_ROOT/Hello-Linked-Windvale.wvmap"
INVALID_WINDVALE_ASSEMBLY_OBJECT="$OUTPUT_ROOT/__windvale_invalid_assembly_output__.wvo"
INVALID_WINDVALE_LINKED_IMAGE="$OUTPUT_ROOT/__windvale_invalid_wvlink_output__.bin"

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
    "$REPOSITORY_ROOT/Projects/Examples/Foundation-Machine-Contracts-Demo.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Examples/Foundation-Byte-Ordering-Demo.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Examples/Foundation-Decimal-Parsing-Demo.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Examples/Foundation-Byte-Construction-Demo.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Examples/Native-Stencil-Demo.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Runtime/Windvale-Native-Service-Bundle-Materialization-Core.wvproj" \
    "$NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_MODULE" \
    17185 \
    97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008 \
    00004321 \
    'build status=Published verification=compiler-aligned functions=19 code-bytes=14253 module-bytes=17185'
exact_build \
    "$REPOSITORY_ROOT/Projects/Runtime/Windvale-Native-Service-Bundle-Materialization.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata-Construction-Core.wvproj" \
    "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_CORE_MODULE" \
    24360 \
    5808f778eb21c1214b581f0ce03958a74173a801b886aec7ed32124d7446abcd \
    00005f28 \
    'build status=Published verification=compiler-aligned functions=35 code-bytes=21363 module-bytes=24360'
exact_build \
    "$REPOSITORY_ROOT/Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata.wvproj" \
    "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE" \
    24252 \
    b5e9397326d3106b22ce735369ef8202ff6bb4c8e14f6069a0c467b4266c8208 \
    00005ebc \
    'build status=Published verification=compiler-aligned functions=36 code-bytes=21394 module-bytes=24252'
exact_inspect "$NATIVE_HOSTED_TOOL_METADATA_CONSTRUCTION_BRIDGE_MODULE" 'profile=portable' 'section name=capabilities offset=95 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=24186 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Linker/Windvale/Native-Hosted-Startup-Instantiation.wvproj" \
    "$NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE" \
    21329 \
    8fb31dbbbb70f094da1e5104d9edd49dd9690bc386541e1d19a75a0fd03ae445 \
    00005351 \
    'build status=Published verification=compiler-aligned functions=15 code-bytes=18984 module-bytes=21329'
exact_inspect "$NATIVE_HOSTED_STARTUP_INSTANTIATION_MODULE" 'profile=portable' 'section name=capabilities offset=88 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=21110 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Projects/Linker/Windvale-Native-Hosted-Container-Construction.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_PLAN_MODULE" \
    36010 \
    e7c92413c31571e8af3dd4ed93664faee5e08716c6241d320b1377c681a254cf \
    00008caa \
    'build status=Published verification=compiler-aligned functions=41 code-bytes=31286 module-bytes=36010'
exact_inspect "$NATIVE_HOSTED_CONTAINER_PLAN_MODULE" 'profile=portable' 'section name=capabilities offset=81 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=35392 bytes=17 count=1'
exact_build \
    "$REPOSITORY_ROOT/Projects/Linker/Windvale-Native-Hosted-Container-Windows.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE" \
    17813 \
    f7a8d3e69b347a3deddf81b5eea09ef929c9798081a6743e7d9aa94262db6de0 \
    00004595 \
    'build status=Published verification=compiler-aligned functions=22 code-bytes=15136 module-bytes=17813'
exact_inspect "$NATIVE_HOSTED_CONTAINER_WINDOWS_MODULE" 'profile=portable' 'section name=capabilities offset=76 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=17747 bytes=17 count=1'
exact_build \
    "$REPOSITORY_ROOT/Projects/Linker/Windvale-Native-Hosted-Container-Linux.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_LINUX_MODULE" \
    12328 \
    dac93155c68ba18f6cbe3af2d301a4c4171b9a9c05841057ea57398536fa8b42 \
    00003028 \
    'build status=Published verification=compiler-aligned functions=19 code-bytes=10674 module-bytes=12328'
exact_inspect "$NATIVE_HOSTED_CONTAINER_LINUX_MODULE" 'profile=portable' 'section name=capabilities offset=74 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=12262 bytes=17 count=1'
exact_build \
    "$REPOSITORY_ROOT/Projects/Linker/Windvale-Native-Hosted-Container-Segmentation.wvproj" \
    "$NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE" \
    22584 \
    488e6d26e4d4ff459ea602fa5cd13b6270486332a4eab64796a29391271c2604 \
    00005838 \
    'build status=Published verification=compiler-aligned functions=28 code-bytes=19181 module-bytes=22584'
exact_inspect "$NATIVE_HOSTED_CONTAINER_SEGMENTATION_MODULE" 'profile=portable' 'section name=capabilities offset=81 bytes=4 count=0' 'name="Main" parameters=1 result=bytes' 'section name=exports offset=21891 bytes=17 count=1'

exact_build \
    "$REPOSITORY_ROOT/Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header-Core.wvproj" \
    "$NATIVE_HOSTED_TOOL_RUNTIME_HEADER_CORE_MODULE" \
    19516 \
    f1c156def9fa6f00bb0401097435bb1d1429d9d4be247b8d11f0de0b5ea51be2 \
    00004c3c \
    'build status=Published verification=compiler-aligned functions=29 code-bytes=17050 module-bytes=19516'
exact_build \
    "$REPOSITORY_ROOT/Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header.wvproj" \
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
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Lexer-Core.wvproj" \
    "$SOURCE_LEXER_MODULE" \
    49470 \
    411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e \
    0000c13e \
    'build status=Published verification=compiler-aligned functions=20 code-bytes=40152 module-bytes=49470'
exact_inspect "$SOURCE_LEXER_MODULE" 'profile=portable' 'section name=exports offset=46433 bytes=715 count=17' 'section name=types offset=47156 bytes=2314 count=7' 'Compiler\u02C9source\u02C9token' 'Compiler\u02C9token\u02C9kind' 'Compiler\u02C9lex\u02C9source\u02C9bounded'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Lexer-Demo.wvproj" \
    "$SOURCE_LEXER_DEMO_MODULE" \
    56674 \
    f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db \
    0000dd62 \
    'build status=Published verification=compiler-aligned functions=21 code-bytes=46427 module-bytes=56674'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Declaration-Parser.wvproj" \
    "$SOURCE_DECLARATION_PARSER_MODULE" \
    151197 \
    8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb \
    00024e9d \
    'build status=Published verification=compiler-aligned functions=52 code-bytes=120804 module-bytes=151197'
exact_inspect "$SOURCE_DECLARATION_PARSER_MODULE" 'profile=portable' 'section name=exports offset=145507 bytes=1417 count=32' 'section name=types offset=146932 bytes=4265 count=15' 'Compiler\u02C9source\u02C9declaration' 'Compiler\u02C9source\u02C9module\u02C9summary' 'Compiler\u02C9parse\u02C9next\u02C9declaration\u02C9validated'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Declaration-Parser-Demo.wvproj" \
    "$SOURCE_DECLARATION_PARSER_DEMO_MODULE" \
    154365 \
    9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf \
    00025afd \
    'build status=Published verification=compiler-aligned functions=53 code-bytes=124556 module-bytes=154365'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Declaration-Parser-Tool.wvproj" \
    "$SOURCE_DECLARATION_PARSER_TOOL_MODULE" \
    151731 \
    ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0 \
    000250b3 \
    'build status=Published verification=compiler-aligned functions=55 code-bytes=122750 module-bytes=151731'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Body-Parser.wvproj" \
    "$SOURCE_BODY_PARSER_MODULE" \
    248663 \
    68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589 \
    0003cb57 \
    'build status=Published verification=compiler-aligned functions=100 code-bytes=197096 module-bytes=248663'
exact_inspect "$SOURCE_BODY_PARSER_MODULE" 'profile=portable' 'section name=exports offset=239096 bytes=2112 count=47' 'section name=types offset=241216 bytes=7447 count=25' 'Compiler\u02C9source\u02C9expression' 'Compiler\u02C9source\u02C9statement' 'Compiler\u02C9parse\u02C9expression\u02C9validated' 'Compiler\u02C9parse\u02C9source\u02C9bodies'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Body-Parser-Demo.wvproj" \
    "$SOURCE_BODY_PARSER_DEMO_MODULE" \
    254805 \
    2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f \
    0003e355 \
    'build status=Published verification=compiler-aligned functions=101 code-bytes=204515 module-bytes=254805'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Body-Parser-Tool.wvproj" \
    "$SOURCE_BODY_PARSER_TOOL_MODULE" \
    247844 \
    0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f \
    0003c824 \
    'build status=Published verification=compiler-aligned functions=103 code-bytes=198924 module-bytes=247844'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Set-Core.wvproj" \
    "$SOURCE_SET_MODULE" \
    257061 \
    2daf59f6863a39c662e282cfc272a0203cff9fc0440e033774b40c8b44354d35 \
    0003ec25 \
    'build status=Published verification=compiler-aligned functions=110 code-bytes=205855 module-bytes=257061'
exact_inspect "$SOURCE_SET_MODULE" 'profile=portable' 'section name=exports offset=248458 bytes=430 count=10' 'section name=types offset=248896 bytes=8165 count=29' 'Compiler\u02C9source\u02C9set\u02C9scan' 'Compiler\u02C9source\u02C9set\u02C9summary' 'Compiler\u02C9scan\u02C9source\u02C9set' 'Compiler\u02C9validate\u02C9source\u02C9set'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Set-Demo.wvproj" \
    "$SOURCE_SET_DEMO_MODULE" \
    266391 \
    de6e86890e54a47a2dba9a821c4cb279c8c02468cbd78c8f57df95c6e399f50e \
    00041097 \
    'build status=Published verification=compiler-aligned functions=116 code-bytes=213351 module-bytes=266391'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Set-Tool.wvproj" \
    "$SOURCE_SET_TOOL_MODULE" \
    260914 \
    132e2a7817c704afa4d6ef9f9a33e21ddbd704cc0bd6139e205a0a3048c65fa1 \
    0003fb32 \
    'build status=Published verification=compiler-aligned functions=115 code-bytes=209119 module-bytes=260914'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Graph-Core.wvproj" \
    "$SOURCE_GRAPH_MODULE" \
    281381 \
    f29b234fc07bc4b1e0b01587b28cd6aa422dd61a68fa310b032b3fc3be5c8a68 \
    00044b25 \
    'build status=Published verification=compiler-aligned functions=126 code-bytes=225553 module-bytes=281381'
exact_inspect "$SOURCE_GRAPH_MODULE" 'profile=portable' 'section name=exports offset=271979 bytes=549 count=12' 'section name=types offset=272536 bytes=8845 count=34' 'Compiler\u02C9source\u02C9graph\u02C9status' 'Compiler\u02C9source\u02C9graph\u02C9summary' 'Compiler\u02C9validate\u02C9source\u02C9graph'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Graph-Demo.wvproj" \
    "$SOURCE_GRAPH_DEMO_MODULE" \
    287335 \
    5e8c4add278609866b952bd0a18dcb7e0e9b05ac04e7e7a5a6fec1e5655ad468 \
    00046267 \
    'build status=Published verification=compiler-aligned functions=131 code-bytes=230448 module-bytes=287335'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Graph-Tool.wvproj" \
    "$SOURCE_GRAPH_TOOL_MODULE" \
    284522 \
    1e0494b7e49f0d14a0508367dcb68d054b69faf501b3ef60ca6f14d48998f7f4 \
    0004576a \
    'build status=Published verification=compiler-aligned functions=131 code-bytes=228463 module-bytes=284522'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Symbols-Core.wvproj" \
    "$SOURCE_SYMBOLS_MODULE" \
    442471 \
    29cdfca436073bf628fa92a10f70915f14bdbcddffb659b25dec793722790e2b \
    0006c067 \
    'build status=Published verification=compiler-aligned functions=204 code-bytes=354399 module-bytes=442471'
exact_inspect "$SOURCE_SYMBOLS_MODULE" 'profile=portable' 'section name=exports offset=427553 bytes=3608 count=66' 'section name=types offset=431169 bytes=11302 count=45' 'Compiler\u02C9source\u02C9symbol\u02C9status' 'Compiler\u02C9source\u02C9symbol\u02C9summary' 'Compiler\u02C9source\u02C9symbols\u02C9directory\u02C9is\u02C9valid' 'Compiler\u02C9validate\u02C9source\u02C9symbols'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Symbols-Demo.wvproj" \
    "$SOURCE_SYMBOLS_DEMO_MODULE" \
    453357 \
    b4aed72b84f8c23f3f391b663d1c87a27912bfff355e3f1def848f057b5e8e65 \
    0006eaed \
    'build status=Published verification=compiler-aligned functions=213 code-bytes=364523 module-bytes=453357'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Symbols-Tool.wvproj" \
    "$SOURCE_SYMBOLS_TOOL_MODULE" \
    441304 \
    01b96a2a6f2d6f1d0210e57020b928f4dad5b3ac1407fd0e0a04b875048f87e7 \
    0006bbd8 \
    'build status=Published verification=compiler-aligned functions=209 code-bytes=358393 module-bytes=441304'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Bindings-Core.wvproj" \
    "$SOURCE_BINDINGS_MODULE" \
    545459 \
    cb150812dd5838ae427687f19eefcc77d07e01eaea5821f1b20a92b03c3b0dbc \
    000852b3 \
    'build status=Published verification=compiler-aligned functions=263 code-bytes=440056 module-bytes=545459'
exact_inspect "$SOURCE_BINDINGS_MODULE" 'profile=portable' 'section name=exports offset=529168 bytes=2996 count=59' 'section name=types offset=532172 bytes=13287 count=55' 'Compiler\u02C9source\u02C9binding\u02C9status' 'Compiler\u02C9source\u02C9binding\u02C9summary' 'Compiler\u02C9source\u02C9bindings\u02C9directory\u02C9is\u02C9valid' 'Compiler\u02C9validate\u02C9source\u02C9bindings'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Bindings-Demo.wvproj" \
    "$SOURCE_BINDINGS_DEMO_MODULE" \
    551186 \
    527e5588728764ec58580a969fef7a888473ac034074378f1de94014e8c27c59 \
    00086912 \
    'build status=Published verification=compiler-aligned functions=271 code-bytes=446436 module-bytes=551186'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Bindings-Tool.wvproj" \
    "$SOURCE_BINDINGS_TOOL_MODULE" \
    545484 \
    d6371a386ea64f9836a5b1382142508a69e609c8315f620aef8f44d991b9890f \
    000852cc \
    'build status=Published verification=compiler-aligned functions=268 code-bytes=443686 module-bytes=545484'

exact_build \
    "$REPOSITORY_ROOT/Projects/Compiler/Windvale-Source-Wir-Core.wvproj" \
    "$SOURCE_WIR_MODULE" \
    823640 \
    7a727928b77b3c8a969b410f7c6e5664915765f5a6f515d037e672ab391cfbd3 \
    000c9158 \
    'build status=Published verification=compiler-aligned functions=347 code-bytes=670806 module-bytes=823640'
exact_inspect "$SOURCE_WIR_MODULE" 'profile=portable' 'section name=exports offset=800191 bytes=3755 count=75' 'section name=types offset=803954 bytes=19686 count=66' 'Compiler\u02C9source\u02C9wir\u02C9operation' 'Compiler\u02C9source\u02C9wir\u02C9summary' 'Compiler\u02C9source\u02C9wir\u02C9directory\u02C9is\u02C9valid' 'Compiler\u02C9validate\u02C9source\u02C9wir'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Wir-Demo.wvproj" \
    "$SOURCE_WIR_DEMO_MODULE" \
    828468 \
    106a75c39b994c46165813686fed21ac9ec65c10a1abaa353aec2acb4a4a6aaf \
    000ca434 \
    'build status=Published verification=compiler-aligned functions=353 code-bytes=677321 module-bytes=828468'
exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Source-Wir-Tool.wvproj" \
    "$SOURCE_WIR_TOOL_MODULE" \
    821936 \
    279f6b8a3dc68884e3700cd6b2995ec44ca0d910b2eadd6aff1d34eea3a1ab1d \
    000c8ab0 \
    'build status=Published verification=compiler-aligned functions=352 code-bytes=674318 module-bytes=821936'

exact_source_compiler_build \
    core \
    "$SOURCE_WVB_MODULE" \
    931585 \
    fcea785e80089643a7d807557e5e145f46d33411abfec98f9f570f246e693a87 \
    000e3701 \
    'source wvb status=Valid functions=423 code-bytes=764118 module-bytes=931585'
exact_inspect "$SOURCE_WVB_MODULE" 'profile=portable' 'section name=exports offset=906991 bytes=3322 count=70' 'section name=types offset=910321 bytes=21264 count=82' 'Compiler\u02C9source\u02C9wvb\u02C9summary' 'Compiler\u02C9compile\u02C9source\u02C9wvb'
exact_source_compiler_build \
    demo \
    "$SOURCE_WVB_DEMO_MODULE" \
    931281 \
    432e485b25bee1e4372e4253a87152a4fc2e9846cd40f09952398d525ec2f501 \
    000e35d1 \
    'source wvb status=Valid functions=427 code-bytes=767085 module-bytes=931281'
exact_source_compiler_build \
    tool \
    "$SOURCE_WVB_TOOL_MODULE" \
    929711 \
    79150787761c7d5e6013ddcb136e518d1388811c99551de443adb6f7a3a23d91 \
    000e2faf \
    'source wvb status=Valid functions=428 code-bytes=766777 module-bytes=929711'

exact_build \
    "$REPOSITORY_ROOT/Projects/Examples/Windvale-Wvb-Inspector.wvproj" \
    "$WVDUMP_CORE_MODULE" \
    76527 \
    293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753 \
    00012aef \
    'build status=Published verification=compiler-aligned functions=39 code-bytes=59277 module-bytes=76527'
exact_verify "$WVDUMP_CORE_MODULE"
exact_inspect "$WVDUMP_CORE_MODULE" 'profile=hosted' 'section name=capabilities offset=48 bytes=145 count=5' 'section name=exports offset=75635 bytes=17 count=1' 'section name=types offset=75660 bytes=867 count=5' 'Inspect\u02C9wvb\u02C9envelope' 'opcode=record.create' 'opcode=record.field' 'opcode=enum.name' 'opcode=u32.format' 'opcode=text.concat' 'opcode=bytes.read_i32_little' 'opcode=text.utf8_is_valid' 'opcode=text.from_utf8' 'opcode=text.quote' 'opcode=u32.from_u8'

exact_build \
    "$REPOSITORY_ROOT/Projects/Object-Model/Windvale-Wvo-Object.wvproj" \
    "$WVO_CORE_MODULE" \
    73322 \
    40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070 \
    00011e6a \
    'build status=Published verification=compiler-aligned functions=64 code-bytes=60229 module-bytes=73322'
exact_verify "$WVO_CORE_MODULE"
exact_inspect_without "$WVO_CORE_MODULE" 'file.write_bytes' 'profile=hosted' 'section name=capabilities offset=51 bytes=145 count=5' 'section name=exports offset=71410 bytes=17 count=1' 'section name=types offset=71435 bytes=1887 count=17' 'opcode=bytes.concat' 'opcode=bytes.from_u16_little' 'opcode=bytes.from_i32_little' 'opcode=text.to_utf8' '__WvM1F0' '__WvM2F0' '__WvM3F0' '__WvM4F0' '__WvM5F0' 'file.read_bytes' 'Object\u02C9sha256'

exact_build \
    "$REPOSITORY_ROOT/Projects/Assembler/Windvale-Wva-Assembler.wvproj" \
    "$WVA_ASSEMBLER_MODULE" \
    180071 \
    a50e261fb690b1b2836b7b05da2d94ec7f023ef531ddd2432fc6a9001ae7049c \
    0002bf67 \
    'build status=Published verification=compiler-aligned functions=101 code-bytes=145748 module-bytes=180071'
exact_verify "$WVA_ASSEMBLER_MODULE"
exact_inspect "$WVA_ASSEMBLER_MODULE" 'profile=hosted' 'section name=capabilities offset=54 bytes=172 count=6' 'section name=exports offset=177876 bytes=17 count=1' 'section name=types offset=177901 bytes=2170 count=19' 'Scan\u02C9wva' 'Inspect\u02C9wva\u02C9semantics' 'Encode\u02C9wva' 'Encode\u02C9sections' 'Encode\u02C9symbols' 'Encode\u02C9relocations' '__WvM4F1' '__WvM2F0' '__WvM3F0' '__WvM1F0' 'opcode=bytes.concat' 'opcode=bytes.from_u32_little' 'file.read_bytes' 'file.write_bytes'

exact_build \
    "$REPOSITORY_ROOT/Projects/Linker/Windvale-Wv-Linker.wvproj" \
    "$WVLINK_CORE_MODULE" \
    135740 \
    02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874 \
    0002123c \
    'build status=Published verification=compiler-aligned functions=96 code-bytes=112099 module-bytes=135740'
exact_verify "$WVLINK_CORE_MODULE"
exact_inspect "$WVLINK_CORE_MODULE" 'profile=hosted' 'section name=capabilities offset=50 bytes=172 count=6' 'section name=exports offset=133297 bytes=17 count=1' 'section name=types offset=133322 bytes=2418 count=20' 'Inspect\u02C9object' 'Find\u02C9section' 'Find\u02C9symbol' 'Find\u02C9relocation' 'Validate\u02C9export\u02C9uniqueness' 'Validate\u02C9imports' 'Measure\u02C9layout' 'Validate\u02C9definitions' 'Build\u02C9unrelocated\u02C9image' 'Apply\u02C9relocations' 'Verifier\u02C9place\u02C9section' 'Verifier\u02C9find\u02C9export' 'Verifier\u02C9apply\u02C9relocations\u02C9reverse' 'Accept\u02C9reconstructed\u02C9image' 'Accepted\u02C9object\u02C9view' 'Definition\u02C9map\u02C9minimum\u02C9exceeds\u02C9limit' 'Build\u02C9canonical\u02C9map' '__WvM4F0' '__WvM2F0' '__WvM3F0' '__WvM1F0' '__WvM1F1' 'name="__WvM5F0" parameters=1 result=bytes locals=903' 'opcode=bytes.read_i32_little' 'file.read_bytes' 'file.write_bytes'

exact_wvdump_execution \
    "$SUM_MODULE" \
    "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv"
exact_wvo_read_only_execution "$WVO_SAMPLE"
exact_wva_and_linker_execution \
    "$WVO_SAMPLE" \
    "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" \
    "$LINK_PROVIDER_OBJECT" \
    "$WINDVALE_LINKED_IMAGE" \
    "$WINDVALE_LINK_MAP" \
    "$INVALID_WINDVALE_ASSEMBLY_OBJECT" \
    "$INVALID_WINDVALE_LINKED_IMAGE"

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

LEGACY_PROJECT="$REPOSITORY_ROOT/Tests/Fixtures/Project/Legacy-Project1.wvproj"
EXISTING_OUTPUT="$TEMPORARY_DIRECTORY/Existing.wvb"
printf '\011\010\007' > "$EXISTING_OUTPUT"
set +e
INVALID_OUTPUT=$("$NATIVE_BUILD" "$LEGACY_PROJECT" "$EXISTING_OUTPUT" 2>&1)
INVALID_EXIT=$?
set -e
if [ "$INVALID_EXIT" -ne 1 ] || \
   [ "$INVALID_OUTPUT" != 'build status=Projectˉrejected code=WVP1001 line=1 column=1' ] || \
   [ "$(od -An -tx1 -v "$EXISTING_OUTPUT" | tr -d ' \n')" != '090807' ]; then
    echo 'The native Seed project rejection or output preservation contract failed.' >&2
    exit 1
fi

echo 'native Seed front-door reconstruction status=Complete artifacts=105 cases=185'
