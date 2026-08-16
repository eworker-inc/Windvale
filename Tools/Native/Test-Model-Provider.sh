#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Model-Provider.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-model-provider.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-model-provider.*)
            rm -f -- "$work"/*
            rmdir -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local candidate=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $candidate && ! -L $candidate ]] || return 1
    [[ $(wc -c < "$candidate") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$candidate" | awk '{ print $1 }') == "$expected_sha256" ]]
}

workspace="$repository_root/Windvale.wvws"
build_project="$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj"
lowerer_project="$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj"
model_project="$repository_root/Projects/Tests/Windvale-Native-Test-Hosted-Model-Provider.wvproj"
front_door="$repository_root/Artifacts/Native-Front-Door/linux-x64/wvbuild.elf"

echo 'START native model provider phase=tools item=1/4'
"$front_door" --workspace "$workspace" --project "$build_project" \
    "$work/Build-Driver.wvb" >/dev/null || exit $?
verify_file "$work/Build-Driver.wvb" 1155121 \
    0cd519556a1cf59321b9418bfbf01643283e10e3dd111c8e2083ec0e51c4ce02 || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Build-Driver.wvb" "$work/Build-Driver.elf" --development-cache \
    >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace" --project "$lowerer_project" "$work/Lowerer.wvb" >/dev/null || exit $?
verify_file "$work/Lowerer.wvb" 522025 \
    318717a608ba37360b9c39f53b9720944ab4463af4ab6a1ec9a267a6ceb85bf6 || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Lowerer.wvb" "$work/Lowerer.elf" --development-cache >/dev/null || exit $?
echo 'PASS  native model provider phase=tools item=1/4'

echo 'START native model provider phase=compile item=2/4'
"$work/Build-Driver.elf" --workspace "$workspace" --project "$model_project" "$work/Model-A.wvb" >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace" --project "$model_project" "$work/Model-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Model-A.wvb" "$work/Model-B.wvb" || exit 1
"$work/Lowerer.elf" "$work/Model-A.wvb" "$work/Model-A.wvo" >/dev/null || exit $?
"$work/Lowerer.elf" "$work/Model-B.wvb" "$work/Model-B.wvo" >/dev/null || exit $?
cmp -s -- "$work/Model-A.wvo" "$work/Model-B.wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$work/Model-A.wvo" >/dev/null || exit $?
echo 'PASS  native model provider phase=compile item=2/4'

echo 'START native model provider phase=host item=3/4'
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/X64-Scripted-Model-Provider-Host.wva" "$work/Host-A.wvo" >/dev/null || exit $?
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/X64-Scripted-Model-Provider-Host.wva" "$work/Host-B.wvo" >/dev/null || exit $?
cmp -s -- "$work/Host-A.wvo" "$work/Host-B.wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$work/Host-A.wvo" >/dev/null || exit $?
"$script_directory/Link-Wvo.sh" 0 Model_host_entry "$work/Model-Image.chunk-0" "$work/Model-A.wvo" "$work/Host-A.wvo" >"$work/Link.txt" || exit $?
entry=$(sed -n 's/^entry name=Model_host_entry address=//p' "$work/Link.txt")
[[ $entry =~ ^[0-9]+$ ]] || exit 1
echo 'PASS  native model provider phase=host item=3/4'

echo 'START native model provider phase=execute item=4/4'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Model-A.wvb" "$work/Model-Image" 1 "$entry" "$work/Model.elf" linux >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Model-A.wvb" "$work/Model-Image" 1 "$entry" "$work/Model.exe" windows >/dev/null || exit $?
"$work/Model.elf" >/dev/null
model_result=$?
[[ $model_result -eq 0 ]] || exit 1
echo 'PASS  native model provider phase=execute item=4/4'
echo 'native model provider status=Passed cases=11 local-result=0 cross-host-images=Verified'
