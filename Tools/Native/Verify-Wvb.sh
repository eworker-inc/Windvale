#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Native/Verify-Wvb.sh <module.wvb>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Front-Door"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'cb77e47f1d69530a16c661deecd91640764a13994d75c4994780e488e938b1f4' \
    'linux-x64/wvverify.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVB verifier artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvb ]]; then
    echo 'The native verifier input must use the .wvb extension.' >&2
    exit 64
fi

"$artifact_root/linux-x64/wvverify.elf" "$input_path"
