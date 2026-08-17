#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 3 || ! -f $1 || ! -d $2 || $3 != *.wvb ]]; then
    echo 'Usage: ./Tools/Native/Compile-Compiler-Build-Driver-Source-Set.sh <compiler.elf> <source-root> <output.wvb>' >&2
    exit 64
fi

compiler=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P)/$(basename -- "$1")
source_root=$(CDPATH= cd -- "$2" && pwd -P) || exit 64
output=$3

"$compiler" \
    "$source_root/Tools/Windvale.Build/Compiler-Build-Driver.wv" \
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
    "$source_root/Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv" \
    "$source_root/Tools/Windvale.Verify/Compiler-Wvb-Verifier-Metadata-Core.wv" \
    "$source_root/Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv" \
    "$source_root/Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv" \
    "$source_root/Tools/Windvale.Project/Project-Manifest-Core.wv" \
    "$source_root/Tools/Windvale.Verify/Wvb-Metadata-Normalization.wv" \
    "$output"
