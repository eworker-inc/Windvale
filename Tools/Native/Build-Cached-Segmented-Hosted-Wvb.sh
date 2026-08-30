#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || ! $1 =~ ^[1-7]$ || $2 != *.wvb || $3 != *.elf ]]; then
    echo 'Usage: ./Tools/Native/Build-Cached-Segmented-Hosted-Wvb.sh <profile-1-through-7> <input.wvb> <output.elf>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P) || exit 1
input_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 64
output_directory=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd -P) || exit 64
input="$input_directory/$(basename -- "$2")"
output="$output_directory/$(basename -- "$3")"
node "$script_directory/Build-Cached-Segmented-Hosted-Wvb.mjs" \
    "$1" "$input" "$output"
