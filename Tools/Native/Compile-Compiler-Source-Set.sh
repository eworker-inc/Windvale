#!/usr/bin/env sh
set -eu

if [ "$#" -ne 3 ] || [ ! -f "$1" ] || [ ! -x "$1" ] ||
    [ ! -d "$2" ] || [ "${3##*.}" != wvb ]; then
    echo 'Usage: ./Tools/Native/Compile-Compiler-Source-Set.sh <compiler.elf> <source-root> <output.wvb>' >&2
    exit 64
fi

COMPILER=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd)/$(basename -- "$1")
SOURCE_ROOT=$(CDPATH= cd -- "$2" && pwd)
OUTPUT_DIRECTORY=$(CDPATH= cd -- "$(dirname -- "$3")" && pwd)
OUTPUT="$OUTPUT_DIRECTORY/$(basename -- "$3")"

"$COMPILER" \
    "$SOURCE_ROOT/Examples/Compiler/Source-Wvb-Tool.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Bindings-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Body-Parser.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Declaration-Parser.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Graph-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Lexer-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Set-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Symbols-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Wir-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Wvb-Core.wv" \
    "$SOURCE_ROOT/Compiler/Windvale/Source-Wvb-Temporary-Slots.wv" \
    "$SOURCE_ROOT/Foundation/Byte-Construction.wv" \
    "$SOURCE_ROOT/Foundation/Decimal-Parsing.wv" \
    "$OUTPUT"
