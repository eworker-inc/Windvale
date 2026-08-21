#!/usr/bin/env sh
set -eu

if [ "$#" -ne 0 ]; then
    echo 'Usage: ./Tools/Native/Test-Generic-Nominal-Declarations.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
exec node "$script_directory/Test-Generic-Nominal-Declarations.mjs"
