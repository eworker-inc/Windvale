#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 5 || $1 != *.wvproj || $4 != *.wvb || $5 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Project-Object.sh <project.wvproj> <build-driver.elf> <lowerer.elf> <output.wvb> <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
exec node "$script_directory/Build-Cached-Project-Object.mjs" "$@"
