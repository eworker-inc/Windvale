#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || $1 != *.wvpack || $2 != *.wvlock || $3 != *.wvb ]]; then
    echo 'Usage: ./Tools/Native/Build-Wvdb-Query-Package.sh <manifest.wvpack> <lock.wvlock> <output.wvb>' >&2
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
expected_manifest="$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack"
if [[ $manifest != "$expected_manifest" ]]; then
    echo 'package status=Invalid_invocation reason=manifest-identity' >&2
    exit 64
fi

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $path ]] || return 1
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}
reject_lock() {
    echo 'package status=Lock_rejected reason=identity-or-resource' >&2
    exit 1
}

verify_file "$lock" 1750 ad22e10e41dda772650123b4802518575088973aa73277889b443ad27aa25618 || reject_lock
verify_file "$manifest" 866 835f573302377fdd38e4c3d51fa9106397beba0b9813f99bfc3143d08a156406 || reject_lock
verify_file "$repository_root/Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 || reject_lock
verify_file "$repository_root/Artifacts/Native-Compiler-Seed/Wvb/Windvale-Compiler.wvb" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 || reject_lock
verify_file "$repository_root/Projects/Applications/Windvale-Wvdb-Query.wvproj" 270 86570daa0dac6410dc8a64947901a3fc955db24afe3589bc70986f96abb8f49a || reject_lock
verify_file "$repository_root/Applications/Database/Wvdb-Query.wv" 3168 22d1fb0b883383fd51cd103d9b831d500178bbedc3d79df12dc86af74070c2d8 || reject_lock
verify_file "$repository_root/Foundation/Decimal-Parsing.wv" 1276 797eb31da7e7a8c93e0d082bf910bc6d8e7988bcfad757a87c979075912e668a || reject_lock
verify_file "$repository_root/Libraries/Platform/Filesystem/Read-Only-Directory.wv" 6565 4c6ecc745b0755b0242c7127c391d27408a7694f91d634c55eeb512746393c81 || reject_lock
verify_file "$repository_root/Libraries/Platform/Database/Read-Only-Wvdb.wv" 9084 7b3bd45397878e5468d979a2fb437feb4d72d5d8bbad21c832bcf3f280c018cb || reject_lock
verify_file "$repository_root/Libraries/Database/Wvdb-Reader.wv" 11213 ad6fd38dafdab57793aead612dd050817f65f22179d11b0f3dbab6654ac909c2 || reject_lock

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvdb-package.XXXXXXXX") || exit 1
candidate="$temporary_directory/Candidate.wvb"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvdb-package.*)
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
    "$repository_root/Projects/Applications/Windvale-Wvdb-Query.wvproj" \
    "$candidate" >/dev/null || exit $?
verify_file "$candidate" 26294 61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 || reject_lock
"$repository_root/Artifacts/Native-Front-Door/linux-x64/wvpublish.elf" \
    "$candidate" "$output" >/dev/null || exit $?
echo 'package status=Published root=windvale.wvdb-query target=hosted-wvb-v1 bytes=26294 sha256=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2'
