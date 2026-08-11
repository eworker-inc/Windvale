#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || ( $1 != windows && $1 != linux ) ]]; then
    echo 'Usage: ./Tools/Native/Admit-Hosted-Verifier-Publisher.sh <windows|linux> <publisher.exe|publisher.elf>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Admission-Candidate"
admitter="$artifact_root/linux-x64-wvhostverifierpublisheradmit.elf"

if [[ ! -f "$admitter" || $(wc -c < "$admitter") -ne 569344 ]] ||
    ! (cd -- "$artifact_root" && printf '%s  %s\n' \
        '18777615d60e1279cb855b05ba03933bb65c9a622036dad2e954e3df683216e2' \
        'linux-x64-wvhostverifierpublisheradmit.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native hosted-verifier publisher admitter identity is invalid.' >&2
    exit 1
fi

"$admitter" "$1" "$2"
