#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Lower-Wvb-To-Wvo.sh <input.wvb> <output.wvo>' >&2
    exit 2
fi
if [[ $1 != *.wvb ]]; then
    echo 'The native lowerer input must use the .wvb extension.' >&2
    exit 2
fi
if [[ $2 != *.wvo ]]; then
    echo 'The native lowerer output must use the .wvo extension.' >&2
    exit 2
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '9c331308e5afe852d4c0441e22c1ff68a0ac0c86793c2e403f38556302c90fd3' \
    'Wvb-To-Wvo.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native WVB-to-WVO lowerer artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
output_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$1")"
output_path="$output_directory/$(basename -- "$2")"
temporary_directory=$(mktemp -d "${TMPDIR:-/tmp}/windvale-native-lower.XXXXXXXX") || exit 1
candidate_path="$temporary_directory/Candidate.wvo"
cleanup() {
    rm -f -- "$candidate_path"
    rmdir -- "$temporary_directory" 2>/dev/null || true
}
trap cleanup EXIT

"$artifact_root/Wvb-To-Wvo.elf" "$input_path" "$candidate_path" || exit $?
"$script_directory/Publish-Wvo.sh" "$candidate_path" "$output_path" >/dev/null
