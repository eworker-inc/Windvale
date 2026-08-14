#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Native/Verify-Wvo.sh <object.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvo-Object-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840' \
    'Wvo-Object.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVO inspector artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
if [[ $input_path != *.wvo ]]; then
    echo 'The native WVO verifier input must use the .wvo extension.' >&2
    exit 64
fi

"$artifact_root/Wvo-Object.elf" verify "$input_path"
