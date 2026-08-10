#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Wvb-Publisher.sh <windows|linux> <output.exe|output.elf>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
"$script_directory/Construct-Hosted-Verifier-Publisher.sh" wvb-publisher "$1" "$2"
