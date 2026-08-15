#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || $1 != *.wvpack || $2 != *.wvlock || $3 != *.wvb ]]; then
    echo 'Usage: ./Tools/Native/Build-Wvb-Inspector-Package.sh <manifest.wvpack> <lock.wvlock> <output.wvb>' >&2
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
expected_manifest="$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack"
if [[ $manifest != "$expected_manifest" ]]; then
    echo 'package status=Invalid_invocation reason=manifest-identity' >&2
    exit 64
fi

verify_file() {
    local file_path=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $file_path ]] || return 1
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$file_path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    digest_line=$(sha256sum -- "$file_path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}
reject_lock() {
    echo 'package status=Lock_rejected reason=identity-or-resource' >&2
    exit 1
}

verify_file "$lock" 1021 eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1 || reject_lock
verify_file "$manifest" 412 a58441a48b0e11c4062e77b0176934952c1de238c78d04ba88ca9ca61e0a41b6 || reject_lock
verify_file "$repository_root/Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 || reject_lock
verify_file "$repository_root/Artifacts/Native-Compiler-Seed/Wvb/Windvale-Compiler.wvb" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 || reject_lock
verify_file "$repository_root/Projects/Examples/Windvale-Wvb-Inspector.wvproj" 71 1583142d2fa4acbaa67b5518b676bef670d4c370c1d6164f4096d66474b28e51 || reject_lock
verify_file "$repository_root/Examples/Foundation/Wv-Dump-Core.wv" 63924 46b169b652d5966e7c203c29b4494966895a616489b3ba6b88ec799c13c873ad || reject_lock

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvb-inspector-package.XXXXXXXX") || exit 1
candidate="$temporary_directory/Candidate.wvb"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvb-inspector-package.*)
            rm -f -- "$candidate"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Examples/Windvale-Wvb-Inspector.wvproj" \
    "$candidate" >/dev/null || exit $?
verify_file "$candidate" 76527 293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753 || reject_lock
"$repository_root/Artifacts/Native-Front-Door/linux-x64/wvpublish.elf" \
    "$candidate" "$output" >/dev/null || exit $?
echo 'package status=Published root=windvale.wvb-inspector target=hosted-wvb-v1 bytes=76527 sha256=293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753'
