#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 4 || $2 != *.wvop || $4 != *.wvli ]]; then
    echo 'Usage: ./Tools/Native/Link-Staged-Compiler-Wvo.sh <wvo-chunk-prefix> <manifest.wvop> <image-chunk-prefix> <manifest.wvli>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '360f05b19181f001439a8309f571d0979eef260285f4995d7dcae1f06679a445' \
    'linux-x64-wvlinkstage.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux segmented compiler-image linker artifact digest is invalid.' >&2
    exit 1
fi

source_manifest_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 1
output_manifest_directory=$(CDPATH= cd -- "$(dirname -- "$4")" && pwd -P) || exit 1
source_manifest="$source_manifest_directory/$(basename -- "$2")"
output_manifest="$output_manifest_directory/$(basename -- "$4")"
"$artifact_root/linux-x64-wvlinkstage.elf" "$1" "$source_manifest" "$3" "$output_manifest"
