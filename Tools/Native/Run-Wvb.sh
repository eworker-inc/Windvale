#!/usr/bin/env bash
set -uo pipefail

if [[ $# -lt 1 || $# -gt 2 || ( $# -eq 2 && $2 != --report-steps ) ]]; then
    echo 'Usage: ./Tools/Native/Run-Wvb.sh <module.wvb> [--report-steps]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvb-Runner-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '6f645b05d9d3b8e2cae34703487f559e5212155fc4ff02c374176ed7e9844054' \
    'linux-x64-wvrun.elf' | sha256sum --check --strict --quiet) ||
    [[ $(wc -c < "$artifact_root/linux-x64-wvrun.elf") -ne 5431296 ]]; then
    echo 'The Linux native WVB runner artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvb ]]; then
    echo 'The native runner input must use the .wvb extension.' >&2
    exit 64
fi

if [[ $# -eq 1 ]]; then
    "$artifact_root/linux-x64-wvrun.elf" "$input_path"
else
    "$artifact_root/linux-x64-wvrun.elf" "$input_path" --report-steps
fi
