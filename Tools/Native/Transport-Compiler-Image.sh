#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 4 || $2 != *.wvli || $4 != *.wvli ]]; then
    echo 'Usage: ./Tools/Native/Transport-Compiler-Image.sh <source-chunk-prefix> <source.wvli> <canonical-chunk-prefix> <canonical.wvli>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '9ff5401eca1ffd93a49077dd6ebc56c446c59939379a481f22662465fc3cf6db' \
    'linux-x64-wvimagetransport.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux compiler-image transport artifact digest is invalid.' >&2
    exit 1
fi

source_manifest_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 1
output_manifest_directory=$(CDPATH= cd -- "$(dirname -- "$4")" && pwd -P) || exit 1
source_manifest="$source_manifest_directory/$(basename -- "$2")"
output_manifest="$output_manifest_directory/$(basename -- "$4")"
"$artifact_root/linux-x64-wvimagetransport.elf" "$1" "$source_manifest" "$3" "$output_manifest"
