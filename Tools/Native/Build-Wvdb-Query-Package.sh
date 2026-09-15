#!/usr/bin/env bash
set -uo pipefail
if [[ $# -ne 3 ]]; then
    echo 'Usage: ./Tools/Native/Build-Wvdb-Query-Package.sh <manifest.wvpack> <lock.wvlock> <output.wvb>' >&2
    exit 64
fi
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
node "$script_directory/Build-Wvdb-Query-Package.mjs" "$1" "$2" "$3"
