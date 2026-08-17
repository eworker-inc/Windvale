#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 5 || $1 != *.wvproj || $3 != *.wvb || $5 != *.wvli ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Segmented-Project.sh <project.wvproj> <build-driver.elf> <output.wvb> <canonical-chunk-prefix> <canonical.wvli>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
exec node "$script_directory/Build-Cached-Segmented-Project.mjs" "$@"
