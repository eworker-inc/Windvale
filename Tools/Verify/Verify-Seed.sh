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

echo "Windvale Seed verification passed."
echo "Conformance report: $REPORT_PATH"
