#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Publish-Hosted-Verifier-Application.sh <candidate.exe|candidate.elf> <destination.exe|destination.elf>' >&2
    exit 64
fi
case "$1:$2" in
    *.exe:*.exe|*.elf:*.elf) ;;
    *)
        echo 'Usage: ./Tools/Native/Publish-Hosted-Verifier-Application.sh <candidate.exe|candidate.elf> <destination.exe|destination.elf>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Hosted-Verifier-Application-Publisher-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '510f5ce5d2a494eacf0adc7a613581bc2371c4ad0f5f985f501381edc1632fac' \
    'linux-x64-wvhostverifierpublish.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native hosted-verifier-application publisher artifact digest is invalid.' >&2
    exit 1
fi

"$artifact_root/linux-x64-wvhostverifierpublish.elf" "$1" "$2"
