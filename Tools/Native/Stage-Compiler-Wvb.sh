#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || $1 != *.wvb || $3 != *.wvop ]]; then
    echo 'Usage: ./Tools/Native/Stage-Compiler-Wvb.sh <input.wvb> <wvo-chunk-prefix> <manifest.wvop>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '5ea8569ce076087aa3b11afc19ce492d0a062f96e872a52dd6a93b889860f3cb' \
    'linux-x64-wvstage.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux segmented WVO producer artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
manifest_directory=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
manifest_path="$manifest_directory/$(basename -- "$3")"
"$artifact_root/linux-x64-wvstage.elf" "$input_path" "$2" "$manifest_path"
