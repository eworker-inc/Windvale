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
    'cee61bb2bf6d805cbb98766c448d7d0985b95dd0fa55a3cd80b2e3697b369a8d' \
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
