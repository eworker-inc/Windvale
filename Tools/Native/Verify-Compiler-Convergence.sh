#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Verify-Compiler-Convergence.sh' >&2
    exit 64
fi

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd -P)

node "$repository_root/Tools/Native/Verify-Current-Split-Compiler-Convergence.mjs" \
    "$repository_root"
