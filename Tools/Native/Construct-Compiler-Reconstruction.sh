#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Compiler-Reconstruction.sh <existing-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
wvb_directory="$output_root/Wvb"
windows_directory="$output_root/windows-x64"
linux_directory="$output_root/linux-x64"
mkdir -p -- "$wvb_directory" "$windows_directory" "$linux_directory" || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-compiler-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-compiler-reconstruction.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvb="$wvb_directory/Windvale-Compiler.wvb"
windows="$windows_directory/wvcompiler.exe"
linux="$linux_directory/wvcompiler.elf"
object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"

"$script_directory/Bootstrap-Compiler.sh" \
    "$repository_root/Artifacts" "$repository_root" "$wvb" \
    >"$temporary_directory/Bootstrap.txt" 2>"$temporary_directory/Bootstrap.err" || exit $?
"$script_directory/Stage-Compiler-Wvb.sh" \
    "$wvb" "$object_prefix" "$object_manifest" \
    >"$temporary_directory/Stage.txt" 2>"$temporary_directory/Stage.err" || exit $?
"$script_directory/Link-Staged-Compiler-Wvo.sh" \
    "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
    >"$temporary_directory/Link.txt" 2>"$temporary_directory/Link.err" || exit $?
"$script_directory/Transport-Compiler-Image.sh" \
    "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
    >"$temporary_directory/Transport.txt" 2>"$temporary_directory/Transport.err" || exit $?

transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$temporary_directory/Transport.txt")
native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
if [[ $native_entry != 43146 || $fragment_count != 7 ]]; then
    echo 'The canonical compiler image identity is invalid.' >&2
    exit 1
fi

"$script_directory/Package-Hosted-Wvb.sh" image 1 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$windows" windows \
    >"$temporary_directory/Windows.txt" 2>"$temporary_directory/Windows.err" || exit $?
"$script_directory/Package-Hosted-Wvb.sh" image 1 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$linux" linux \
    >"$temporary_directory/Linux.txt" 2>"$temporary_directory/Linux.err" || exit $?

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    [[ -f $path ]] || return 1
    local actual_bytes
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    local digest_line
    local actual_sha256
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}

verify_file "$wvb" 927274 \
    d3dbadd987f10a98ebd90d1357973dca055094e2dbd3cc3e0e90afb3c3c17fae || exit 1
verify_file "$windows" 27776000 \
    0975f6181c78cd4b0007883d4b4ee9275b7cbb46bf904ce0cc79730d32308f7e || exit 1
verify_file "$linux" 27774976 \
    93651adc36557aaa895627e8d8aa022b8765fc4f6cfaafbb5dc7c0a263287f67 || exit 1

echo 'native compiler reconstruction status=Complete compiler-bytes=927274 native-bytes=27744550 entry-offset=43146 chunks=7'
