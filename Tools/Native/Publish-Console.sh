#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Publish-Console.sh <candidate.exe|candidate.elf> <destination.exe|destination.elf>' >&2
    exit 64
fi
case "$1:$2" in
    *.exe:*.exe|*.elf:*.elf) ;;
    *)
        echo 'Usage: ./Tools/Native/Publish-Console.sh <candidate.exe|candidate.elf> <destination.exe|destination.elf>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Console-Application-Publisher-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'fdfe5876f1217b747ec637a3a8407948f1402505ec27c91aa6a44fd3e06fcfa2' \
    'linux-x64-wvappublish.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native console-application publisher artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/linux-x64-wvappublish.elf" "$1" "$2"
