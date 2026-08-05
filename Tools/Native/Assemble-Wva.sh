#!/usr/bin/env bash
set -uo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
    echo 'Usage: ./Tools/Native/Assemble-Wva.sh <source.wva> [output.wvo]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Front-Door"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'ebe18959f2a057db5181f4e2bbf7979fac9359d50542581b63da6dc48c4163a0' \
    'linux-x64/wvasm.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVA assembler artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wva ]]; then
    echo 'The native assembler input must use the .wva extension.' >&2
    exit 64
fi

if [[ $# -eq 2 ]]; then
    output_input=$2
    output_directory=$(CDPATH= cd -- "$(dirname -- "$output_input")" && pwd -P) || exit 1
    output_path="$output_directory/$(basename -- "$output_input")"
else
    output_path="${input_path%.wva}.wvo"
fi
if [[ $output_path != *.wvo ]]; then
    echo 'The native assembler output must use the .wvo extension.' >&2
    exit 64
fi

"$artifact_root/linux-x64/wvasm.elf" "$input_path" "$output_path"
