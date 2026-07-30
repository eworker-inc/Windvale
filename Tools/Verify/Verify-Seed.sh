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
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'Nominal types (3)' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'record.create' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'record.field' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'enum.name' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'u32.format' >/dev/null
printf '%s\n' "$WVDUMP_CORE_INSPECT_OUTPUT" | grep -F 'text.concat' >/dev/null

WVDUMP_CORE_RUN_OUTPUT=$(dotnet run --project "$TOOL_PROJECT" --configuration "$CONFIGURATION" --no-build -- run "$WVDUMP_CORE_MODULE")
printf '%s\n' "$WVDUMP_CORE_RUN_OUTPUT" | grep -F 'Result: 0' >/dev/null

echo "Windvale Seed verification passed."
echo "Conformance report: $REPORT_PATH"
