#!/usr/bin/env bash
set -uo pipefail

if [[ $# -gt 1 || ( $# -eq 1 && $1 != --foundation-borrow && $1 != --foundation-borrow-plan && $1 != --foundation-borrow-directories ) ]]; then
    echo 'Usage: ./Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.sh [--foundation-borrow|--foundation-borrow-plan|--foundation-borrow-directories]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
exec node "$script_directory/Test-Language-1.0-Memory-Budget-Split-Execution.mjs" "$@"
