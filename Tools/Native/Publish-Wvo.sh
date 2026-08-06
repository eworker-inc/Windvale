#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || $1 != *.wvo || $2 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Publish-Wvo.sh <candidate.wvo> <destination.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvo-Publisher-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'ac2bb513e2145e9eb911a9be142fc2f1f990a1bab21f278dd841043042b51f7a' \
    'linux-x64-wvopublish.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVO publisher artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/linux-x64-wvopublish.elf" "$1" "$2"
