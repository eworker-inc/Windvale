#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Native/Check-Wvo.sh <object.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvo-Object-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'b8f0367a8ced12227c9554101152bd5199ec0fd32e5e78210f5dd8a0761b81c7' \
    'Wvo-Object.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVO inspector artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvo ]]; then
    echo 'The native WVO checker input must use the .wvo extension.' >&2
    exit 64
fi

"$artifact_root/Wvo-Object.elf" check "$input_path"
