#!/usr/bin/env bash
set -uo pipefail

if [[ ${1-} == --vector-borrow-integration ]]; then
    if [[ $# -ne 1 && ( $# -ne 3 || ${2-} != --maximum-seconds ) ]]; then
        echo 'Usage: --vector-borrow-integration [--maximum-seconds <seconds>]' >&2
        exit 64
    fi
elif [[ $# -gt 1 || ( $# -eq 1 && $1 != --foundation-borrow && $1 != --foundation-borrow-plan && $1 != --foundation-borrow-directories && $1 != --foundation-borrow-owners && $1 != --foundation-borrow-components ) ]]; then
    echo 'Usage: ./Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.sh [--foundation-borrow|--foundation-borrow-plan|--foundation-borrow-directories|--foundation-borrow-owners|--foundation-borrow-components|--vector-borrow-integration [--maximum-seconds <seconds>]]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
exec node "$script_directory/Test-Language-1.0-Memory-Budget-Split-Execution.mjs" "$@"
