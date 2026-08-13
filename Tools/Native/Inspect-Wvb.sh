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
    'fe84ab498fde5112e62398982bc76e3334e4bdec9e2502b87a2e4bb191fbdab3' \
    'linux-x64/wvverify.elf' \
    'd3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627' \
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
