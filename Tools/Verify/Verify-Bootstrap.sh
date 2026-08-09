#!/usr/bin/env sh
set -eu

if [ "$#" -ne 0 ]; then
    echo 'Usage: ./Tools/Verify/Verify-Bootstrap.sh' >&2
    exit 64
fi

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
ARTIFACTS="$REPOSITORY_ROOT/artifacts"
OUTPUT="$ARTIFACTS/Bootstrap-Windvale-Compiler.wvb"

mkdir -p "$ARTIFACTS"
"$REPOSITORY_ROOT/Tools/Native/Bootstrap-Compiler.sh" \
    "$REPOSITORY_ROOT/Artifacts" "$REPOSITORY_ROOT" "$OUTPUT"

echo 'Native compiler bootstrap verification passed.'
echo "Compiler: $OUTPUT"
