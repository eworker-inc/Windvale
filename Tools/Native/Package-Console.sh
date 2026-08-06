#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 4 ]]; then
    echo 'Usage: ./Tools/Native/Package-Console.sh <windows-x64-console-v1|linux-x64-console-v1> <native-image.bin> <entry-offset> <output>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Console-Packager-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '10b1d752ab6c9c7217f833add9ef77ca0d61b6bcc02d7023b1877f42bab2a683' \
    'Console-Packager.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native console packager artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/Console-Packager.elf" "$@"
