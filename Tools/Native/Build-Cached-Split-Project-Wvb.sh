#!/usr/bin/env sh
set -eu

if [ "$#" -ne 6 ]; then
    echo "Usage: Tools/Native/Build-Cached-Split-Project-Wvb.sh <project.wvproj> <output.wvb> <analyzer> <analyzer.identity> <emitter> <emitter.identity>" >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
exec node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$1" "$2" "$3" "$4" "$5" "$6"
