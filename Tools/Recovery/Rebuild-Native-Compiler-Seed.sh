#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Recovery/Rebuild-Native-Compiler-Seed.sh <output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
canonical_root="$repository_root/Artifacts/Native-Compiler-Seed"
mkdir -p -- "$1/Wvb" "$1/windows-x64" "$1/linux-x64"
destination=$(CDPATH= cd -- "$1" && pwd -P)
if [[ $destination == "$canonical_root" ]]; then
    echo 'Seed reconstruction must use a separate output directory.' >&2
    exit 64
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-compiler-seed.XXXXXXXX")
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-compiler-seed.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

reconstruction_commit=3824f39d0997e3d7ab523f7cc1fe0f4bd8288e35
semantic_freeze_commit=524e84afb6e5bab6bbd95ebc0b9eeaf886af834b
reconstructor="$temporary_directory/Reconstructor"
seed_source="$temporary_directory/Seed-Source"
mkdir -- "$reconstructor" "$seed_source"

git -C "$repository_root" archive --format=tar "$reconstruction_commit" |
    tar -xf - -C "$reconstructor"
git -C "$repository_root" archive --format=tar "$semantic_freeze_commit" -- \
    Windvale-Compiler.wvproj \
    Examples/Compiler/Source-Wvb-Tool.wv \
    Compiler/Windvale \
    Foundation/Byte-Construction.wv \
    Foundation/Decimal-Parsing.wv |
    tar -xf - -C "$seed_source"

(cd -- "$reconstructor" &&
    dotnet build Tools/Windvale.Tool/Windvale.Tool.csproj \
        --configuration Release --nologo --verbosity quiet)
tool="$reconstructor/Tools/Windvale.Tool/bin/Release/net10.0/windvale.dll"
wvb="$destination/Wvb/Windvale-Compiler.wvb"
(cd -- "$seed_source" &&
    dotnet "$tool" build Windvale-Compiler.wvproj -o "$wvb")
dotnet "$tool" aot "$wvb" --target windows-x64-console-v3 \
    -o "$destination/windows-x64/wvcompiler.exe"
dotnet "$tool" aot "$wvb" --target linux-x64-console-v3 \
    -o "$destination/linux-x64/wvcompiler.elf"

(cd -- "$destination" &&
    sha256sum --check --strict "$canonical_root/SHA256SUMS")
echo "Recovered native compiler seed: $destination"
