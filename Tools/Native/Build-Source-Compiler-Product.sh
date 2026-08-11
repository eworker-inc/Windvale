#!/usr/bin/env sh
set -eu

if [ "$#" -ne 2 ] ||
    { [ "$1" != core ] && [ "$1" != demo ] && [ "$1" != tool ]; } ||
    [ "${2##*.}" != wvb ]; then
    echo 'Usage: ./Tools/Native/Build-Source-Compiler-Product.sh <core|demo|tool> <output.wvb>' >&2
    exit 64
fi

product=$1
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
seed_root="$repository_root/Artifacts/Native-Compiler-Seed"
front_door_root="$repository_root/Artifacts/Native-Front-Door"
if ! (cd -- "$seed_root" && sha256sum --check --strict --quiet SHA256SUMS); then
    echo 'The native compiler seed artifact inventory is invalid.' >&2
    exit 1
fi
if ! (cd -- "$front_door_root" && sha256sum --check --strict --quiet SHA256SUMS); then
    echo 'The native front-door artifact inventory is invalid.' >&2
    exit 1
fi

compiler="$seed_root/linux-x64/wvcompiler.elf"
publisher="$front_door_root/linux-x64/wvpublish.elf"
output_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 64
output="$output_directory/$(basename -- "$2")"
case "$product" in
    core)
        project="$repository_root/Windvale-Source-Wvb-Core.wvproj"
        project_bytes=603
        project_sha256=989dd0e30bd24a2e11714598405f01a87200ac53d0896c3db1fa5d89cf11faab
        ;;
    demo)
        project="$repository_root/Windvale-Source-Wvb-Demo.wvproj"
        project_bytes=649
        project_sha256=fcb003a7f7b8b6e7107a282034e6853f0969c7c55de948a90b6ad4114dd704d7
        ;;
    tool)
        project="$repository_root/Windvale-Compiler.wvproj"
        project_bytes=649
        project_sha256=e097e9d007909a3cf17476ccfce41ace5fa89c566386d15ae24c7d91d9f91e7b
        ;;
esac
actual_project_bytes=$(wc -c < "$project") || exit 1
project_digest_line=$(sha256sum -- "$project") || exit 1
actual_project_sha256=${project_digest_line%% *}
if [ "$actual_project_bytes" -ne "$project_bytes" ] ||
    [ "$actual_project_sha256" != "$project_sha256" ]; then
    echo 'The source compiler product manifest identity is invalid.' >&2
    exit 1
fi
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-source-compiler-product.XXXXXXXX") || exit 1
candidate="$temporary_directory/Candidate.wvb"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-source-compiler-product.*)
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

case "$product" in
    core)
        root="$repository_root/Compiler/Windvale/Source-Wvb-Core.wv"
        ;;
    demo)
        root="$repository_root/Examples/Compiler/Source-Wvb-Demo.wv"
        ;;
    tool)
        root="$repository_root/Examples/Compiler/Source-Wvb-Tool.wv"
        ;;
esac

if [ "$product" = core ]; then
    "$compiler" \
        "$root" \
        "$repository_root/Compiler/Windvale/Source-Bindings-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Body-Parser.wv" \
        "$repository_root/Compiler/Windvale/Source-Declaration-Parser.wv" \
        "$repository_root/Compiler/Windvale/Source-Graph-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Lexer-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Set-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Symbols-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Wir-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Wvb-Temporary-Slots.wv" \
        "$repository_root/Foundation/Byte-Construction.wv" \
        "$repository_root/Foundation/Decimal-Parsing.wv" \
        "$candidate"
else
    "$compiler" \
        "$root" \
        "$repository_root/Compiler/Windvale/Source-Bindings-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Body-Parser.wv" \
        "$repository_root/Compiler/Windvale/Source-Declaration-Parser.wv" \
        "$repository_root/Compiler/Windvale/Source-Graph-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Lexer-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Set-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Symbols-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Wir-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Wvb-Core.wv" \
        "$repository_root/Compiler/Windvale/Source-Wvb-Temporary-Slots.wv" \
        "$repository_root/Foundation/Byte-Construction.wv" \
        "$repository_root/Foundation/Decimal-Parsing.wv" \
        "$candidate"
fi
"$publisher" "$candidate" "$output"
