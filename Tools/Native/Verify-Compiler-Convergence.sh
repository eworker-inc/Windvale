#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 ]]; then
    echo 'Usage: ./Tools/Native/Verify-Compiler-Convergence.sh <artifact-root> <source-root>' >&2
    exit 64
fi

artifact_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
source_root=$(CDPATH= cd -- "$2" && pwd -P) || exit 64
repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd -P)
verifier="$artifact_root/Native-Front-Door/linux-x64/wvverify.elf"

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local label=$4
    [[ -f $path ]] || { echo "Missing $label: $path" >&2; return 1; }
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || {
        echo "The $label byte length is invalid." >&2
        return 1
    }
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || {
        echo "The $label digest is invalid." >&2
        return 1
    }
}

verify_file "$verifier" 1007616 \
    dd98cd8f42ee8237b030d96dd1305e23843f92ae7dfd92469a67579e2cbe718a \
    'Linux native WVB verifier' || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-compiler-convergence.XXXXXXXX") || exit 1
stage1="$temporary_directory/Stage1.wvb"
stage1_compiler="$temporary_directory/Stage1-Compiler.elf"
stage2="$temporary_directory/Stage2.wvb"

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-compiler-convergence.*)
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

"$repository_root/Tools/Native/Bootstrap-Compiler.sh" \
    "$artifact_root" "$source_root" "$stage1" || exit $?
"$repository_root/Tools/Native/Package-Segmented-Compiler-Wvb.sh" \
    1 "$stage1" "$stage1_compiler" || exit $?
"$repository_root/Tools/Native/Compile-Compiler-Source-Set.sh" \
    "$stage1_compiler" "$source_root" "$stage2" || exit $?

verify_file "$stage2" 921640 \
    18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754 \
    'Stage 2 compiler WVB' || exit 1
"$verifier" "$stage2" >"$temporary_directory/Verify.txt" || exit $?
cmp --silent -- "$stage1" "$stage2" || exit 1

echo 'native compiler convergence status=Complete compiler-bytes=921640 compiler-sha256=18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754'
