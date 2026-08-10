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
    '8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a' \
    'Wv-Linker.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVO linker artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/Wv-Linker.elf" "$@"
