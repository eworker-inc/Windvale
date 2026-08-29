#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || $1 != *.wvpack || $2 != *.wvlock || $3 != *.wvb ]]; then
    echo 'Usage: ./Tools/Native/Build-Echo-Package.sh <manifest.wvpack> <lock.wvlock> <output.wvb>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
manifest_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 64
manifest="$manifest_directory/$(basename -- "$1")"
lock_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 64
lock="$lock_directory/$(basename -- "$2")"
output_directory=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd -P) || exit 64
output="$output_directory/$(basename -- "$3")"
expected_manifest="$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvpack"
compiler="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvcompiler.elf"
publisher="$repository_root/Artifacts/Native-Wvb-Publisher-Candidate/linux-x64-wvpublish.elf"
if [[ $manifest != "$expected_manifest" ]]; then
    echo 'package status=Invalid_invocation reason=manifest-identity' >&2
    exit 64
fi

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $path && ! -L $path ]] || return 1
    [[ $(wc -c < "$path") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$path" | awk '{ print $1 }') == "$expected_sha256" ]]
}
reject_lock() {
    echo 'package status=Lock_rejected reason=identity-or-resource' >&2
    exit 1
}

verify_file "$lock" 940 caf8b109f7b0d817115f53dac539da4ec63760cb2a34e23943871b787d74836e || reject_lock
verify_file "$manifest" 333 27d32dc98d1c2d57792f0a37b173a77d5dab465e005bc9c47fd8fd086c8b6234 || reject_lock
verify_file "$repository_root/Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 || reject_lock
verify_file "$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/Wvb/Windvale-Compiler.wvb" 935163 a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6 || reject_lock
verify_file "$compiler" 28172288 da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b || reject_lock
verify_file "$publisher" 1541109 7bf4593566401853ab7f551ca5d45125ac0ea3a6c4e34315703785ed7d6cdfb6 || reject_lock
verify_file "$repository_root/Projects/Applications/Windvale-Echo.wvproj" 62 bf5b476f36512f48c0798fc1683708872500094e6a853ba6274d3ee7a8b3c6ef || reject_lock
verify_file "$repository_root/Applications/Shell/Echo.wv" 845 f843e69b9549a890aa808331f6ef503941c0a1d5240ecd5859e46f6f8ae044c7 || reject_lock

temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-echo-package.XXXXXXXX") || exit 1
candidate="$work/Candidate.wvb"
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-echo-package.*)
            rm -f -- "$candidate"
            rmdir -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$compiler" "$repository_root/Applications/Shell/Echo.wv" \
    "$candidate" >/dev/null || exit $?
verify_file "$candidate" 927 \
    b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713 || reject_lock
"$publisher" "$candidate" "$output" >/dev/null || exit $?
echo 'package status=Published root=windvale.echo target=hosted-wvb-v1 bytes=927 sha256=b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713 metadata=Present'
