#!/usr/bin/env bash
set -uo pipefail

if [[ $# -gt 1 || ($# -eq 1 && ${1-} != '--compiler-only') ]]; then
    echo 'Usage: ./Tools/Native/Test-Source-Containment.sh [--compiler-only]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
node --expose-gc "$script_directory/Test-Random-Containment.mjs" source "$@"
