#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Native/Run-Wvb.sh <module.wvb>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Front-Door"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '28a98e35286ab0f0515b147ef34a527335dce1f3a5bf96449ea4495b8079ed0f' \
    'linux-x64/wvrun.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVB runner artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvb ]]; then
    echo 'The native runner input must use the .wvb extension.' >&2
    exit 64
fi

"$artifact_root/linux-x64/wvrun.elf" "$input_path"
