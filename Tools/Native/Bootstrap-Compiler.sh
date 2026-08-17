#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 ]]; then
    echo 'Usage: ./Tools/Native/Bootstrap-Compiler.sh <artifact-root> <source-root> <output.wvb>' >&2
    exit 64
fi

artifact_root=$(CDPATH= cd -- "$1" && pwd -P) || {
    echo 'The native seed artifact root does not exist.' >&2
    exit 64
}
source_root=$(CDPATH= cd -- "$2" && pwd -P) || {
    echo 'The compiler source root does not exist.' >&2
    exit 64
}
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
output_directory=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd -P) || exit 64
output_path="$output_directory/$(basename -- "$3")"
if [[ $output_path != *.wvb ]]; then
    echo 'The native compiler bootstrap output must use the .wvb extension.' >&2
    exit 64
fi

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local label=$4
    if [[ ! -f $path ]]; then
        echo "Missing $label: $path" >&2
        return 1
    fi
    local actual_bytes
    actual_bytes=$(wc -c < "$path") || return 1
    if [[ $actual_bytes -ne $expected_bytes ]]; then
        echo "The $label byte length is invalid." >&2
        return 1
    fi
    local digest_line
    local actual_sha256
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    if [[ $actual_sha256 != "$expected_sha256" ]]; then
        echo "The $label digest is invalid." >&2
        return 1
    fi
}

compiler_wvb="$artifact_root/Native-Compiler-Seed/Wvb/Windvale-Compiler.wvb"
compiler="$artifact_root/Native-Compiler-Seed/linux-x64/wvcompiler.elf"
publisher="$artifact_root/Native-Front-Door/linux-x64/wvpublish.elf"
project="$source_root/Projects/Examples/Windvale-Compiler.wvproj"

verify_file "$compiler_wvb" 914746 \
    48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 \
    'native compiler seed WVB' || exit 1
verify_file "$compiler" 27467776 \
    2f745e2c4dddb7333926783796f06b6f02ef356742fb5873a2efffdca16c696a \
    'Linux native compiler seed' || exit 1
verify_file "$publisher" 1369077 \
    b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af \
    'Linux native publisher' || exit 1
verify_file "$project" 649 \
    a180b171446a6b047b737913ead74fb77a2ecb8d5eedcef833e881dc93ec9b05 \
    'compiler project manifest' || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-compiler-bootstrap.XXXXXXXX") || exit 1
stage1="$temporary_directory/Stage1.wvb"
stage1_compiler="$temporary_directory/Stage1-Compiler.elf"
candidate="$temporary_directory/Candidate.wvb"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-compiler-bootstrap.*)
            rm -f -- "$stage1" "$stage1_compiler" "$candidate"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Compile-Compiler-Source-Set.sh" \
    "$compiler" "$source_root" "$stage1"
result=$?
if [[ $result -ne 0 ]]; then
    exit "$result"
fi

verify_file "$stage1" 959320 \
    e177e418bfd8fdcbe40cfac513ce40e58b95ba5b88a8a1d1db9fe280ae81dbfb \
    'transitional Stage 1 compiler WVB' || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" \
    1 "$stage1" "$stage1_compiler" || exit $?
"$script_directory/Compile-Compiler-Source-Set.sh" \
    "$stage1_compiler" "$source_root" "$candidate" || exit $?
verify_file "$candidate" 935163 \
    a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6 \
    'fixed-point Stage 2 compiler WVB' || exit 1
"$publisher" "$candidate" "$output_path"
