#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Native/Inspect-Wvb.sh <module.wvb>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Front-Door"
if ! (cd -- "$artifact_root" && printf '%s  %s\n%s  %s\n' \
    'dd98cd8f42ee8237b030d96dd1305e23843f92ae7dfd92469a67579e2cbe718a' \
    'linux-x64/wvverify.elf' \
    '4f99dc43e1af4ad074cc15a38bfe44a433af9979985a600739780ac156a52791' \
    'linux-x64/wvdump.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVB verifier or inspector artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvb ]]; then
    echo 'The native inspector input must use the .wvb extension.' >&2
    exit 64
fi

"$artifact_root/linux-x64/wvverify.elf" "$input_path" >/dev/null || exit $?
"$artifact_root/linux-x64/wvdump.elf" "$input_path"
