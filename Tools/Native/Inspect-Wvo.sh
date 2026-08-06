#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Native/Inspect-Wvo.sh <object.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvo-Object-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'bdc4817c252ecf2592299a6646161b396bfb251acabc68d3f5d75ff40891541e' \
    'Wvo-Object.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVO inspector artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvo ]]; then
    echo 'The native WVO inspector input must use the .wvo extension.' >&2
    exit 64
fi

"$artifact_root/Wvo-Object.elf" inspect "$input_path"
