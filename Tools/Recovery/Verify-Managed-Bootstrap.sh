#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
CONFIGURATION=${CONFIGURATION:-Release}
MAXIMUM_INSTRUCTIONS=${MAXIMUM_INSTRUCTIONS:-64000000000}
TOOL_PROJECT="$REPOSITORY_ROOT/Tools/Windvale.Tool/Windvale.Tool.csproj"
TOOL_DLL="$REPOSITORY_ROOT/Tools/Windvale.Tool/bin/$CONFIGURATION/net10.0/windvale.dll"
ARTIFACTS="$REPOSITORY_ROOT/artifacts"
PROJECT_MANIFEST="$REPOSITORY_ROOT/Windvale-Compiler.wvproj"
ROOT_SOURCE="$REPOSITORY_ROOT/Examples/Compiler/Source-Wvb-Tool.wv"
STAGE1="$ARTIFACTS/Recovery-Bootstrap-Stage1-Source-Wvb-Tool.wvb"
STAGE2="$ARTIFACTS/Recovery-Bootstrap-Stage2-Source-Wvb-Tool.wvb"

set -- \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Bindings-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Body-Parser.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Declaration-Parser.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Graph-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Lexer-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Set-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Symbols-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Wir-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Wvb-Core.wv" \
    "$REPOSITORY_ROOT/Compiler/Windvale/Source-Wvb-Temporary-Slots.wv" \
    "$REPOSITORY_ROOT/Foundation/Byte-Construction.wv" \
    "$REPOSITORY_ROOT/Foundation/Decimal-Parsing.wv"

mkdir -p "$ARTIFACTS"
dotnet build "$TOOL_PROJECT" --configuration "$CONFIGURATION" --nologo
dotnet "$TOOL_DLL" build "$PROJECT_MANIFEST" -o "$STAGE1"

RUN_OUTPUT=$(dotnet "$TOOL_DLL" run "$STAGE1" \
    --allow console.write_line \
    --allow diagnostic.write_line \
    --allow file.read_bytes \
    --allow file.write_bytes \
    --allow process.argument \
    --allow process.argument_count \
    --max-steps "$MAXIMUM_INSTRUCTIONS" --report-steps -- \
    "$ROOT_SOURCE" "$@" "$STAGE2")
printf '%s\n' "$RUN_OUTPUT"
printf '%s\n' "$RUN_OUTPUT" | grep -F 'source wvb status=Valid ' >/dev/null
printf '%s\n' "$RUN_OUTPUT" | grep -F 'Result: 0' >/dev/null

dotnet "$TOOL_DLL" verify "$STAGE2"
cmp -s "$STAGE1" "$STAGE2"

COMPILER_BYTES=$(wc -c < "$STAGE2" | tr -d ' ')
COMPILER_SHA256=$(sha256sum "$STAGE2" | awk '{print $1}')
echo 'Managed recovery bootstrap convergence passed.'
echo "Compiler bytes: $COMPILER_BYTES"
echo "Compiler SHA-256: $COMPILER_SHA256"
echo "Stage 1: $STAGE1"
echo "Stage 2: $STAGE2"
