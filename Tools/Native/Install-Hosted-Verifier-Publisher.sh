#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Install-Hosted-Verifier-Publisher.sh <candidate.exe|candidate.elf> <destination.exe|destination.elf>' >&2
    exit 64
fi
case "$1:$2" in
    *.exe:*.exe|*.elf:*.elf) ;;
    *)
        echo 'Usage: ./Tools/Native/Install-Hosted-Verifier-Publisher.sh <candidate.exe|candidate.elf> <destination.exe|destination.elf>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Promoter-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58' \
    'linux-x64-wvhostverifierpublisherinstall.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native hosted-verifier publisher promoter artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/linux-x64-wvhostverifierpublisherinstall.elf" "$1" "$2"
