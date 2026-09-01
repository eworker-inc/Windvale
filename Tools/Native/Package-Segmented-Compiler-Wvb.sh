#!/usr/bin/env bash
set -uo pipefail

if [[ ($# -ne 3 && $# -ne 4) || ! $1 =~ ^[1-8]$ ||
      $2 != *.wvb || $3 != *.elf ||
      ($# -eq 4 && ${4:-} != --development-cache) ]]; then
    echo 'Usage: ./Tools/Native/Package-Segmented-Compiler-Wvb.sh <profile-1-through-8> <input.wvb> <output.elf> [--development-cache]' >&2
    exit 64
fi
development_cache=0
[[ $# -eq 4 ]] && development_cache=1

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
input_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 64
input="$input_directory/$(basename -- "$2")"
output_directory=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd -P) || exit 64
output="$output_directory/$(basename -- "$3")"
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-segmented-compiler-package.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-segmented-compiler-package.*)
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

object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"

echo "segmented compiler package step=stage input=$(basename -- "$input")"
if ! "$script_directory/Stage-Compiler-Wvb.sh" "$input" "$object_prefix" "$object_manifest" >"$temporary_directory/Stage.txt"; then
    cat -- "$temporary_directory/Stage.txt" >&2
    exit 1
fi
echo 'segmented compiler package step=stage status=Complete'
echo 'segmented compiler package step=link'
if ! "$script_directory/Link-Staged-Compiler-Wvo.sh" "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" >"$temporary_directory/Link.txt"; then
    cat -- "$temporary_directory/Link.txt" >&2
    exit 1
fi
echo 'segmented compiler package step=link status=Complete'
echo 'segmented compiler package step=transport'
if ! "$script_directory/Transport-Compiler-Image.sh" "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" >"$temporary_directory/Transport.txt"; then
    cat -- "$temporary_directory/Transport.txt" >&2
    exit 1
fi
echo 'segmented compiler package step=transport status=Complete'

transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$temporary_directory/Transport.txt")
native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-9]\|1[0-6]\) manifest-bytes=.*$/\1/p')
case "$native_entry" in
    ''|*[!0-9]*) echo 'The compiler-image transport did not report one decimal Main entry.' >&2; exit 1 ;;
esac
case "$fragment_count" in
    [1-9]|1[0-6]) ;;
    *) echo 'The compiler-image transport did not report one canonical chunk count.' >&2; exit 1 ;;
esac

echo "segmented compiler package step=container fragments=$fragment_count entry=$native_entry"
if [[ $development_cache -eq 1 ]]; then
    "$script_directory/Build-Cached-Hosted-Application.sh" "$1" "$input" \
        "$canonical_prefix" "$fragment_count" "$native_entry" "$output" linux
else
    "$script_directory/Package-Hosted-Wvb.sh" image "$1" "$input" \
        "$canonical_prefix" "$fragment_count" "$native_entry" "$output"
fi
result=$?
[[ $result -eq 0 ]] || exit "$result"
echo "segmented compiler package status=Complete output=$(basename -- "$output")"
