#!/usr/bin/env sh
set -eu

if [ "$#" -ne 0 ]; then
    echo 'Usage: ./Tools/Verify/Verify-Bootstrap.sh' >&2
    exit 64
fi

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
"$REPOSITORY_ROOT/Tools/Native/Verify-Compiler-Convergence.sh"

echo 'Native compiler bootstrap verification passed.'
