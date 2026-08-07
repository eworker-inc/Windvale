#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Verify-Console-Segmented.sh <first-application-chunk> <second-application-chunk>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Console-Application-Verifier-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '5dbd78b3f67cc179e9848eacca6627a03f5f44ddecc6480d2e9ab98d073f792e' \
    'linux-x64-wvappverify.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native console-application verifier artifact digest is invalid.' >&2
    exit 1
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-application-verifier.XXXXXXXX") || exit 1
temporary_executable="$temporary_directory/wvappverify"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-application-verifier.*)
            rm -f -- "$temporary_executable"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT
cp -- "$artifact_root/linux-x64-wvappverify.elf" "$temporary_executable" || exit 1
chmod 700 -- "$temporary_executable" || exit 1
"$temporary_executable" "$1" "$2"
