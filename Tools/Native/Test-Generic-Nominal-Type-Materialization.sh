#!/usr/bin/env sh
set -eu

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
if [ "$#" -eq 1 ] && [ "$1" = '--development' ]; then
    exec node "$script_directory/Test-Generic-Nominal-Development-Bundle.mjs" \
        type-materialization
elif [ "$#" -ne 0 ]; then
    echo 'Usage: ./Tools/Native/Test-Generic-Nominal-Type-Materialization.sh [--development]' >&2
    exit 64
fi
exec node "$script_directory/Test-Generic-Nominal-Type-Materialization.mjs"
