#!/usr/bin/env bash
set -uo pipefail

if [[ $# -gt 1 ]] || [[ $# -eq 1 && $1 != --streaming ]]; then
    echo 'Usage: ./Tools/Native/Test-Native-Sha256.sh [--streaming]' >&2
    exit 64
fi
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
node "$repository_root/Tools/Native/Test-Native-Sha256.mjs" linux "$repository_root" "$@"
