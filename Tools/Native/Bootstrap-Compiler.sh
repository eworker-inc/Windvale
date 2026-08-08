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
project="$source_root/Windvale-Compiler.wvproj"

verify_file "$compiler_wvb" 914746 \
    48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 \
    'native compiler seed WVB' || exit 1
verify_file "$compiler" 27467776 \
    2f745e2c4dddb7333926783796f06b6f02ef356742fb5873a2efffdca16c696a \
    'Linux native compiler seed' || exit 1
verify_file "$publisher" 1119173 \
    71dccc29333b05cff71e4b36e5e41617e0df4f8d747747479e8a27f4a90ed3b0 \
    'Linux native publisher' || exit 1
verify_file "$project" 649 \
    e097e9d007909a3cf17476ccfce41ace5fa89c566386d15ae24c7d91d9f91e7b \
    'compiler project manifest' || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-compiler-bootstrap.XXXXXXXX") || exit 1
candidate="$temporary_directory/Candidate.wvb"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-compiler-bootstrap.*)
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

"$compiler" \
    "$source_root/Examples/Compiler/Source-Wvb-Tool.wv" \
    "$source_root/Compiler/Windvale/Source-Bindings-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Body-Parser.wv" \
    "$source_root/Compiler/Windvale/Source-Declaration-Parser.wv" \
    "$source_root/Compiler/Windvale/Source-Graph-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Lexer-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Set-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Symbols-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Wir-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Wvb-Core.wv" \
    "$source_root/Compiler/Windvale/Source-Wvb-Temporary-Slots.wv" \
    "$source_root/Foundation/Byte-Construction.wv" \
    "$source_root/Foundation/Decimal-Parsing.wv" \
    "$candidate"
result=$?
if [[ $result -ne 0 ]]; then
    exit "$result"
fi

verify_file "$candidate" 921900 \
    fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556 \
    'bootstrapped compiler WVB' || exit 1
"$publisher" "$candidate" "$output_path"
