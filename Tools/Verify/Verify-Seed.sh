#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
CONFIGURATION=${CONFIGURATION:-Release}
ARCHITECTURE=$(uname -m)
REPORT_PATH=${1:-"$REPOSITORY_ROOT/artifacts/seed-conformance-linux-$ARCHITECTURE.json"}
TOOL_PROJECT="$REPOSITORY_ROOT/Tools/Windvale.Tool/Windvale.Tool.csproj"
ARTIFACTS="$REPOSITORY_ROOT/artifacts"
mkdir -p "$ARTIFACTS"

dotnet build "$REPOSITORY_ROOT/Windvale.slnx" --configuration "$CONFIGURATION" --nologo
dotnet run \
    --project "$REPOSITORY_ROOT/Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj" \
    --configuration "$CONFIGURATION" \
    --no-build \
    -- \
    --report "$REPORT_PATH"

SUM_MODULE="$ARTIFACTS/Sum-Data.wvb"
HELLO_MODULE="$ARTIFACTS/Hello-Windvale.wvb"
FOUNDATION_MODULE="$ARTIFACTS/Read-Wvb-Header.wvb"
WVDUMP_CORE_MODULE="$ARTIFACTS/Wv-Dump-Core.wvb"
WVO_CORE_MODULE="$ARTIFACTS/Wvo-Object-Core.wvb"
WVA_SCANNER_MODULE="$ARTIFACTS/Wva-Scanner-Core.wvb"
WVO_SAMPLE="$ARTIFACTS/Sample.wvo"
ASSEMBLY_OBJECT="$ARTIFACTS/Hello-Object.wvo"
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Seed/Sum-Data.wv" -o "$SUM_MODULE"

VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$SUM_MODULE")
printf '%s\n' "$VERIFY_OUTPUT" | grep -F 'Verified: Sumˉdata' >/dev/null

INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$SUM_MODULE")
printf '%s\n' "$INSPECT_OUTPUT" | grep -F 'data.load.i32' >/dev/null

RUN_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$SUM_MODULE")
printf '%s\n' "$RUN_OUTPUT" | grep -F 'Result: 29' >/dev/null

dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Seed/Hello-Windvale.wv" -o "$HELLO_MODULE"

set +e
UNAUTHORIZED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$HELLO_MODULE" 2>&1)
UNAUTHORIZED_EXIT=$?
set -e
if [ "$UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized run exit 3, found $UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

HELLO_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$HELLO_MODULE" --allow console.write_line)
printf '%s\n' "$HELLO_OUTPUT" | grep -F 'Hello from Windvale' >/dev/null
printf '%s\n' "$HELLO_OUTPUT" | grep -F 'Result: 0' >/dev/null

dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Read-Wvb-Header.wv" -o "$FOUNDATION_MODULE"

FOUNDATION_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$FOUNDATION_MODULE")
printf '%s\n' "$FOUNDATION_VERIFY_OUTPUT" | grep -F 'Verified: Readˉwvbˉheader' >/dev/null

FOUNDATION_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$FOUNDATION_MODULE")
printf '%s\n' "$FOUNDATION_INSPECT_OUTPUT" | grep -F 'bytes.read_u32_little' >/dev/null

FOUNDATION_RUN_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$FOUNDATION_MODULE")
printf '%s\n' "$FOUNDATION_RUN_OUTPUT" | grep -F 'Result: 1' >/dev/null

dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Wv-Dump-Core.wv" -o "$WVDUMP_CORE_MODULE"

WVDUMP_CORE_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$WVDUMP_CORE_MODULE")
printf '%s\n' "$WVDUMP_CORE_VERIFY_OUTPUT" | grep -F 'Verified: Wvˉdumpˉcore' >/dev/null

WVDUMP_CORE_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$WVDUMP_CORE_MODULE")
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
WVDUMP_UNAUTHORIZED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$WVDUMP_CORE_MODULE" 2>&1)
WVDUMP_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVDUMP_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WvDump run exit 3, found $WVDUMP_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVDUMP_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVDUMP_CORE_RUN_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVDUMP_CORE_RUN_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVDUMP_HOSTED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVDUMP_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$SUM_MODULE")
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'wvdump 1' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'module version=1.5 profile=portable name="Sum\u02C9data"' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'data index=0 name="Values" type=i32_array elements=4' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'instruction function=1 offset=141 opcode=call operand=0' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'export index=0 name="Main" kind=function target=1' >/dev/null
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVDUMP_INVALID_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WVDUMP_MISSING_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WVDUMP_INVALID_NAME_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Wvo-Object-Core.wv" -o "$WVO_CORE_MODULE"

WVO_CORE_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_VERIFY_OUTPUT" | grep -F 'Verified: Wvoˉobjectˉcore' >/dev/null

WVO_CORE_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.concat' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_u16_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_i32_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'text.to_utf8' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'file.write_bytes' >/dev/null

set +e
WVO_UNAUTHORIZED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$WVO_CORE_MODULE" 2>&1)
WVO_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVO_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WVO writer run exit 3, found $WVO_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVO_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVO_SELF_TEST_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVO_CORE_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVO_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVO_HOSTED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

WVO_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-verify "$WVO_SAMPLE")
printf '%s\n' "$WVO_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

WVO_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-inspect "$WVO_SAMPLE")
printf '%s\n' "$WVO_INSPECTION" | grep -F 'Sections (2)' >/dev/null
printf '%s\n' "$WVO_INSPECTION" | grep -F 'Console_write binding=Import' >/dev/null
printf '%s\n' "$WVO_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=1 symbol=2 addend=-4' >/dev/null

set +e
WVO_INVALID_NAME_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WVO_MISSING_PARENT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Assembler/Wva-Scanner-Core.wv" -o "$WVA_SCANNER_MODULE"

WVA_SCANNER_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$WVA_SCANNER_MODULE")
printf '%s\n' "$WVA_SCANNER_VERIFY_OUTPUT" | grep -F 'Verified: Wvaˉscannerˉcore' >/dev/null

WVA_SCANNER_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$WVA_SCANNER_MODULE")
printf '%s\n' "$WVA_SCANNER_INSPECT_OUTPUT" | grep -F 'Scanˉwva' >/dev/null
printf '%s\n' "$WVA_SCANNER_INSPECT_OUTPUT" | grep -F 'text.utf8_is_valid' >/dev/null
printf '%s\n' "$WVA_SCANNER_INSPECT_OUTPUT" | grep -F 'file.read_bytes' >/dev/null

set +e
WVA_SCANNER_UNAUTHORIZED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$WVA_SCANNER_MODULE" 2>&1)
WVA_SCANNER_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVA_SCANNER_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WVA scanner run exit 3, found $WVA_SCANNER_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVA_SCANNER_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVA_SCANNER_SELF_TEST_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVA_SCANNER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVA_SCANNER_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVA_SCANNER_HOSTED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVA_SCANNER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000 \
    -- "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva")
printf '%s\n' "$WVA_SCANNER_HOSTED_OUTPUT" | grep -F 'wvascan 1' >/dev/null
printf '%s\n' "$WVA_SCANNER_HOSTED_OUTPUT" | grep -F 'status=valid bytes=403 lines=21 meaningful-lines=17 tokens=52 offset=403 line=22 column=1' >/dev/null
printf '%s\n' "$WVA_SCANNER_HOSTED_OUTPUT" | grep -F 'Result: 0' >/dev/null

ASSEMBLY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    assemble "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva" -o "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_OUTPUT" | grep -F 'Assembled:' >/dev/null
printf '%s\n' "$ASSEMBLY_OUTPUT" | grep -F 'SHA-256: 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85' >/dev/null

ASSEMBLY_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-verify "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

ASSEMBLY_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-inspect "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F '.text kind=Code align=16 memory=11 data=11' >/dev/null
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' >/dev/null
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F 'kind=Absoluteˉu32 section=1 offset=3 symbol=1 addend=0' >/dev/null

echo "Windvale Seed verification passed."
echo "Conformance report: $REPORT_PATH"
