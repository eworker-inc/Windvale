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

verify_file "$lock" 920 212e5c4ddf28fb347b482c73d5c38d6df8273be4bcf14ce1b581084d7be1652d || reject_lock
verify_file "$manifest" 333 27d32dc98d1c2d57792f0a37b173a77d5dab465e005bc9c47fd8fd086c8b6234 || reject_lock
verify_file "$repository_root/Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 || reject_lock
verify_file "$repository_root/Artifacts/Native-Compiler-Seed/Wvb/Windvale-Compiler.wvb" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 || reject_lock
verify_file "$repository_root/Projects/Applications/Windvale-Echo.wvproj" 62 bf5b476f36512f48c0798fc1683708872500094e6a853ba6274d3ee7a8b3c6ef || reject_lock
verify_file "$repository_root/Applications/Shell/Echo.wv" 755 0738f826901ac6b03121d7a534b2c07f79f89475bd2af33f5c45cba895dae91d || reject_lock

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

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Applications/Windvale-Echo.wvproj" \
    "$candidate" >/dev/null || exit $?
verify_file "$candidate" 813 \
    5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64 || reject_lock
"$repository_root/Artifacts/Native-Front-Door/linux-x64/wvpublish.elf" \
    "$candidate" "$output" >/dev/null || exit $?
echo 'package status=Published root=windvale.echo target=hosted-wvb-v1 bytes=813 sha256=5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64'
