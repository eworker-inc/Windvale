#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || ! $1 =~ ^[1-7]$ || $2 != *.wvb || $3 != *.elf ]]; then
    echo 'Usage: ./Tools/Native/Package-Segmented-Compiler-Wvb.sh <profile-1-through-7> <input.wvb> <output.elf>' >&2
    exit 64
fi

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

"$script_directory/Stage-Compiler-Wvb.sh" "$input" "$object_prefix" "$object_manifest" >"$temporary_directory/Stage.txt" || exit $?
"$script_directory/Link-Staged-Compiler-Wvo.sh" "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" >"$temporary_directory/Link.txt" || exit $?
"$script_directory/Transport-Compiler-Image.sh" "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" >"$temporary_directory/Transport.txt" || exit $?

transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$temporary_directory/Transport.txt")
native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
case "$native_entry" in
    ''|*[!0-9]*) echo 'The compiler-image transport did not report one decimal Main entry.' >&2; exit 1 ;;
esac
case "$fragment_count" in
    [1-8]) ;;
    *) echo 'The compiler-image transport did not report one canonical chunk count.' >&2; exit 1 ;;
esac

"$script_directory/Package-Hosted-Wvb.sh" image "$1" "$input" "$canonical_prefix" "$fragment_count" "$native_entry" "$output"
