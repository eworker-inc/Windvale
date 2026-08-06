#!/usr/bin/env bash
set -uo pipefail

if [[ $# -lt 4 ]]; then
    echo 'Usage: ./Tools/Native/Link-Wvo.sh <base-address> <entry> <output.bin> <input.wvo>...' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wv-Linker-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '994f27f5a2449990b767c0ed8c8c367e2676d41d652ee9a61eab1de36de82dc2' \
    'Wv-Linker.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVO linker artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/Wv-Linker.elf" "$@"
