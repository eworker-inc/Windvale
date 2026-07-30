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

COMPOSITION_ROOT="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Demo.wv"
COMPOSITION_MIDDLE="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Middle.wv"
COMPOSITION_LEAF="$REPOSITORY_ROOT/Examples/Foundation/Module-Composition-Leaf.wv"
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$COMPOSITION_ROOT" \
    --module "$COMPOSITION_MIDDLE" \
    --module "$COMPOSITION_LEAF" \
    -o "$COMPOSITION_MODULE"
COMPOSITION_HASH=$(sha256sum "$COMPOSITION_MODULE" | awk '{print $1}')
if [ "$COMPOSITION_HASH" != '0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60' ]; then
    echo "The composed source module has an unexpected digest: $COMPOSITION_HASH" >&2
    exit 1
fi
COMPOSITION_RUN_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$COMPOSITION_MODULE")
printf '%s\n' "$COMPOSITION_RUN_OUTPUT" | grep -F 'Result: 42' >/dev/null
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$COMPOSITION_ROOT" \
    --module "$COMPOSITION_LEAF" \
    --module "$COMPOSITION_MIDDLE" \
    -o "$COMPOSITION_REORDERED_MODULE"
cmp "$COMPOSITION_MODULE" "$COMPOSITION_REORDERED_MODULE"
rm -f "$INVALID_COMPOSITION_MODULE"

MACHINE_CONTRACTS_SOURCE="$REPOSITORY_ROOT/Foundation/Machine-Contracts.wv"
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$MACHINE_CONTRACTS_SOURCE" -o "$MACHINE_CONTRACTS_MODULE"
MACHINE_CONTRACTS_HASH=$(sha256sum "$MACHINE_CONTRACTS_MODULE" | awk '{print $1}')
if [ "$MACHINE_CONTRACTS_HASH" != '9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a' ]; then
    echo "The Foundation machine-contract module has an unexpected digest: $MACHINE_CONTRACTS_HASH" >&2
    exit 1
fi
MACHINE_CONTRACTS_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$MACHINE_CONTRACTS_MODULE")
printf '%s\n' "$MACHINE_CONTRACTS_INSPECTION" | grep -F 'Foundationˉalignmentˉisˉvalid' >/dev/null
printf '%s\n' "$MACHINE_CONTRACTS_INSPECTION" | grep -F 'Foundationˉmachineˉnameˉisˉvalid' >/dev/null
printf '%s\n' "$MACHINE_CONTRACTS_INSPECTION" | grep -F 'Exports (2)' >/dev/null
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Machine-Contracts-Demo.wv" \
    --module "$MACHINE_CONTRACTS_SOURCE" \
    -o "$MACHINE_CONTRACTS_DEMO_MODULE"
MACHINE_CONTRACTS_DEMO_HASH=$(sha256sum "$MACHINE_CONTRACTS_DEMO_MODULE" | awk '{print $1}')
if [ "$MACHINE_CONTRACTS_DEMO_HASH" != 'b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a' ]; then
    echo "The Foundation machine-contract demo has an unexpected digest: $MACHINE_CONTRACTS_DEMO_HASH" >&2
    exit 1
fi
MACHINE_CONTRACTS_DEMO_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$MACHINE_CONTRACTS_DEMO_MODULE")
printf '%s\n' "$MACHINE_CONTRACTS_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

BYTE_ORDERING_SOURCE="$REPOSITORY_ROOT/Foundation/Byte-Ordering.wv"
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$BYTE_ORDERING_SOURCE" -o "$BYTE_ORDERING_MODULE"
BYTE_ORDERING_HASH=$(sha256sum "$BYTE_ORDERING_MODULE" | awk '{print $1}')
if [ "$BYTE_ORDERING_HASH" != '194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1' ]; then
    echo "The Foundation byte-ordering module has an unexpected digest: $BYTE_ORDERING_HASH" >&2
    exit 1
fi
BYTE_ORDERING_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$BYTE_ORDERING_MODULE")
printf '%s\n' "$BYTE_ORDERING_INSPECTION" | grep -F 'Foundationˉbyteˉspansˉcompare' >/dev/null
printf '%s\n' "$BYTE_ORDERING_INSPECTION" | grep -F 'Exports (1)' >/dev/null
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Byte-Ordering-Demo.wv" \
    --module "$BYTE_ORDERING_SOURCE" \
    -o "$BYTE_ORDERING_DEMO_MODULE"
BYTE_ORDERING_DEMO_HASH=$(sha256sum "$BYTE_ORDERING_DEMO_MODULE" | awk '{print $1}')
if [ "$BYTE_ORDERING_DEMO_HASH" != '0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab' ]; then
    echo "The Foundation byte-ordering demo has an unexpected digest: $BYTE_ORDERING_DEMO_HASH" >&2
    exit 1
fi
BYTE_ORDERING_DEMO_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$BYTE_ORDERING_DEMO_MODULE")
printf '%s\n' "$BYTE_ORDERING_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

DECIMAL_PARSING_SOURCE="$REPOSITORY_ROOT/Foundation/Decimal-Parsing.wv"
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$DECIMAL_PARSING_SOURCE" -o "$DECIMAL_PARSING_MODULE"
DECIMAL_PARSING_HASH=$(sha256sum "$DECIMAL_PARSING_MODULE" | awk '{print $1}')
if [ "$DECIMAL_PARSING_HASH" != '39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f' ]; then
    echo "The Foundation decimal-parsing module has an unexpected digest: $DECIMAL_PARSING_HASH" >&2
    exit 1
fi
DECIMAL_PARSING_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$DECIMAL_PARSING_MODULE")
printf '%s\n' "$DECIMAL_PARSING_INSPECTION" | grep -F 'Foundationˉu32ˉparse' >/dev/null
printf '%s\n' "$DECIMAL_PARSING_INSPECTION" | grep -F 'Foundationˉu32ˉdecimalˉparse' >/dev/null
printf '%s\n' "$DECIMAL_PARSING_INSPECTION" | grep -F 'Exports (1)' >/dev/null
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Decimal-Parsing-Demo.wv" \
    --module "$DECIMAL_PARSING_SOURCE" \
    -o "$DECIMAL_PARSING_DEMO_MODULE"
DECIMAL_PARSING_DEMO_HASH=$(sha256sum "$DECIMAL_PARSING_DEMO_MODULE" | awk '{print $1}')
if [ "$DECIMAL_PARSING_DEMO_HASH" != '16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695' ]; then
    echo "The Foundation decimal-parsing demo has an unexpected digest: $DECIMAL_PARSING_DEMO_HASH" >&2
    exit 1
fi
DECIMAL_PARSING_DEMO_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$DECIMAL_PARSING_DEMO_MODULE")
printf '%s\n' "$DECIMAL_PARSING_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null

BYTE_CONSTRUCTION_SOURCE="$REPOSITORY_ROOT/Foundation/Byte-Construction.wv"
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$BYTE_CONSTRUCTION_SOURCE" -o "$BYTE_CONSTRUCTION_MODULE"
BYTE_CONSTRUCTION_HASH=$(sha256sum "$BYTE_CONSTRUCTION_MODULE" | awk '{print $1}')
if [ "$BYTE_CONSTRUCTION_HASH" != '6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207' ]; then
    echo "The Foundation byte-construction module has an unexpected digest: $BYTE_CONSTRUCTION_HASH" >&2
    exit 1
fi
BYTE_CONSTRUCTION_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$BYTE_CONSTRUCTION_MODULE")
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Foundationˉbytesˉresult' >/dev/null
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Foundationˉbytesˉrepeat' >/dev/null
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Foundationˉbytesˉreplace' >/dev/null
printf '%s\n' "$BYTE_CONSTRUCTION_INSPECTION" | grep -F 'Exports (2)' >/dev/null
dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Foundation/Byte-Construction-Demo.wv" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    -o "$BYTE_CONSTRUCTION_DEMO_MODULE"
BYTE_CONSTRUCTION_DEMO_HASH=$(sha256sum "$BYTE_CONSTRUCTION_DEMO_MODULE" | awk '{print $1}')
if [ "$BYTE_CONSTRUCTION_DEMO_HASH" != 'a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29' ]; then
    echo "The Foundation byte-construction demo has an unexpected digest: $BYTE_CONSTRUCTION_DEMO_HASH" >&2
    exit 1
fi
BYTE_CONSTRUCTION_DEMO_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$BYTE_CONSTRUCTION_DEMO_MODULE")
printf '%s\n' "$BYTE_CONSTRUCTION_DEMO_OUTPUT" | grep -F 'Result: 0' >/dev/null
set +e
MISSING_COMPOSITION_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
MISSING_COMPOSITION_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
printf '%s\n' "$WVDUMP_HOSTED_OUTPUT" | grep -F 'module version=1.6 profile=portable name="Sum\u02C9data"' >/dev/null
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
    compile "$REPOSITORY_ROOT/Examples/Foundation/Wvo-Object-Core.wv" \
    --module "$BYTE_ORDERING_SOURCE" \
    -o "$WVO_CORE_MODULE"

WVO_CORE_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_VERIFY_OUTPUT" | grep -F 'Verified: Wvoˉobjectˉcore' >/dev/null

WVO_CORE_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$WVO_CORE_MODULE")
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.concat' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_u16_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'bytes.from_i32_little' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'text.to_utf8' >/dev/null
printf '%s\n' "$WVO_CORE_INSPECT_OUTPUT" | grep -F 'Foundationˉbyteˉspansˉcompare' >/dev/null
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
    compile "$REPOSITORY_ROOT/Examples/Assembler/Wva-Assembler-Core.wv" \
    --module "$MACHINE_CONTRACTS_SOURCE" \
    --module "$BYTE_ORDERING_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    -o "$WVA_ASSEMBLER_MODULE"

WVA_ASSEMBLER_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$WVA_ASSEMBLER_MODULE")
printf '%s\n' "$WVA_ASSEMBLER_VERIFY_OUTPUT" | grep -F 'Verified: Wvaˉassemblerˉcore' >/dev/null

WVA_ASSEMBLER_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$WVA_ASSEMBLER_MODULE")
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
WVA_ASSEMBLER_UNAUTHORIZED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$WVA_ASSEMBLER_MODULE" 2>&1)
WVA_ASSEMBLER_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVA_ASSEMBLER_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized WVA assembler run exit 3, found $WVA_ASSEMBLER_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVA_ASSEMBLER_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVA_ASSEMBLER_SELF_TEST_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVA_ASSEMBLER_MODULE" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 10000000)
printf '%s\n' "$WVA_ASSEMBLER_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    compile "$REPOSITORY_ROOT/Examples/Linker/Wv-Linker-Core.wv" \
    --module "$MACHINE_CONTRACTS_SOURCE" \
    --module "$BYTE_ORDERING_SOURCE" \
    --module "$DECIMAL_PARSING_SOURCE" \
    --module "$BYTE_CONSTRUCTION_SOURCE" \
    -o "$WVLINK_CORE_MODULE"

WVLINK_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- verify "$WVLINK_CORE_MODULE")
printf '%s\n' "$WVLINK_VERIFY_OUTPUT" | grep -F 'Verified: Wvˉlinkerˉcore' >/dev/null

WVLINK_INSPECT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- inspect "$WVLINK_CORE_MODULE")
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
WVLINK_UNAUTHORIZED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$WVLINK_CORE_MODULE" 2>&1)
WVLINK_UNAUTHORIZED_EXIT=$?
set -e
if [ "$WVLINK_UNAUTHORIZED_EXIT" -ne 3 ]; then
    echo "Expected unauthorized Windvale linker run exit 3, found $WVLINK_UNAUTHORIZED_EXIT." >&2
    exit 1
fi
printf '%s\n' "$WVLINK_UNAUTHORIZED_OUTPUT" | grep -F 'WVR3010' >/dev/null

WVLINK_SELF_TEST_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    run "$WVLINK_CORE_MODULE" \
    --allow console.write \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps 20000000)
printf '%s\n' "$WVLINK_SELF_TEST_OUTPUT" | grep -F 'Result: 0' >/dev/null

WVA_ASSEMBLER_HOSTED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WINDVALE_ASSEMBLY_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-verify "$WINDVALE_ASSEMBLY_OBJECT")
printf '%s\n' "$WINDVALE_ASSEMBLY_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

WVLINK_HOSTED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

WVLINK_INVALID_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WVA_ASSEMBLER_MISSING_PARENT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WVA_SEMANTIC_INVALID_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

WVA_SEMANTIC_EXISTING_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

ASSEMBLY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    assemble "$REPOSITORY_ROOT/Examples/Assembler/Hello-Object.wva" -o "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_OUTPUT" | grep -F 'Assembled:' >/dev/null
printf '%s\n' "$ASSEMBLY_OUTPUT" | grep -F 'SHA-256: 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85' >/dev/null
STAGE0_ASSEMBLY_HASH=$(sha256sum "$ASSEMBLY_OBJECT" | awk '{print $1}')
if [ "$STAGE0_ASSEMBLY_HASH" != "$WINDVALE_ASSEMBLY_HASH" ]; then
    echo 'The Windvale-written and Stage 0 assembler objects differ.' >&2
    exit 1
fi

ASSEMBLY_VERIFY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-verify "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_VERIFY_OUTPUT" | grep -F 'Verified object: X86ˉ64' >/dev/null

ASSEMBLY_INSPECTION=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- object-inspect "$ASSEMBLY_OBJECT")
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F '.text kind=Code align=16 memory=11 data=11' >/dev/null
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F 'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4' >/dev/null
printf '%s\n' "$ASSEMBLY_INSPECTION" | grep -F 'kind=Absoluteˉu32 section=1 offset=3 symbol=1 addend=0' >/dev/null

PROVIDER_ASSEMBLY_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
    assemble "$REPOSITORY_ROOT/Examples/Linker/Console-Provider.wva" -o "$LINK_PROVIDER_OBJECT")
printf '%s\n' "$PROVIDER_ASSEMBLY_OUTPUT" | grep -F 'SHA-256: 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab' >/dev/null

WVLINK_MAP_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
WVLINK_UNDEFINED_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

WVLINK_EXISTING_FAILURE=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
MISSING_WINDVALE_LINK_PARENT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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

LINK_MAP_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
UNDEFINED_LINK_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
EXISTING_LINK_FAILURE=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
MISSING_LINK_PARENT_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- \
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
