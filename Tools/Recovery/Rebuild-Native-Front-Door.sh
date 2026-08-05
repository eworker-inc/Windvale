#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo 'Usage: ./Tools/Recovery/Rebuild-Native-Front-Door.sh <output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
canonical_root="$repository_root/Artifacts/Native-Front-Door"
mkdir -p -- "$1/Wvb" "$1/windows-x64" "$1/linux-x64"
destination=$(CDPATH= cd -- "$1" && pwd -P)
if [[ $destination == "$canonical_root" ]]; then
    echo 'Recovery reconstruction must use a separate output directory.' >&2
    exit 64
fi

dotnet build "$repository_root/Tools/Windvale.Tool/Windvale.Tool.csproj" \
    -c Release --nologo --verbosity quiet
tool="$repository_root/Tools/Windvale.Tool/bin/Release/net10.0/windvale.dll"

dotnet "$tool" build "$repository_root/Windvale-Compiler-Build-Driver.wvproj" \
    -o "$destination/Wvb/Compiler-Build-Driver.wvb"
dotnet "$tool" aot "$destination/Wvb/Compiler-Build-Driver.wvb" \
    --target windows-x64-build-driver-v1 \
    -o "$destination/windows-x64/wvbuild.exe"
dotnet "$tool" aot "$destination/Wvb/Compiler-Build-Driver.wvb" \
    --target linux-x64-build-driver-v1 \
    -o "$destination/linux-x64/wvbuild.elf"
dotnet "$tool" build "$repository_root/Windvale-Wvb-Publisher.wvproj" \
    -o "$destination/Wvb/Wvb-Publisher.wvb"
dotnet "$tool" aot "$destination/Wvb/Wvb-Publisher.wvb" \
    --target windows-x64-wvb-publisher-v1 \
    -o "$destination/windows-x64/wvpublish.exe"
dotnet "$tool" aot "$destination/Wvb/Wvb-Publisher.wvb" \
    --target linux-x64-wvb-publisher-v1 \
    -o "$destination/linux-x64/wvpublish.elf"

(cd -- "$destination" && sha256sum --check --strict "$canonical_root/SHA256SUMS")
echo "Recovered native front door: $destination"
