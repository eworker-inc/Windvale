#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || $1 != *.bin || $3 != *.efi ]]; then
    echo 'Usage: ./Tools/Native/Package-Uefi.sh <native-image.bin> <entry-offset> <output.efi>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Uefi-Packager-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '9b1c3a364e21b3fb66b246fb89df907b523272fee4e3ac5eaa39f5414e39e5b6' \
    'Uefi-Packager.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native UEFI packager artifact digest is invalid.' >&2
    exit 1
fi
if [[ $(wc -c < "$artifact_root/Uefi-Packager.elf") -ne 278528 ]]; then
    echo 'The Linux native UEFI packager artifact length is invalid.' >&2
    exit 1
fi
if [[ ! -x $artifact_root/Uefi-Packager.elf ]]; then
    echo 'The Linux native UEFI packager artifact is not executable.' >&2
    exit 1
fi

"$artifact_root/Uefi-Packager.elf" "$1" "$2" "$3"
