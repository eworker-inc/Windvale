#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Model-Provider.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=$(node -p "require('node:fs').realpathSync.native(process.argv[1])" "${TMPDIR:-/tmp}") || exit 1
allocated_work=$(mktemp -d "$temporary_root/windvale-model-provider.XXXXXXXX") || exit 1
if ! work=$(node -p "require('node:fs').realpathSync.native(process.argv[1])" "$allocated_work"); then
    rmdir -- "$allocated_work"
    exit 1
fi
if ! temporary_root=$(node -p "require('node:path').dirname(process.argv[1])" "$work"); then
    rmdir -- "$work"
    exit 1
fi
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
model_project="$repository_root/Projects/Tests/Windvale-Native-Test-Hosted-Model-Provider.wvproj"
build_driver="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvbuild.elf"
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

echo 'START native model provider phase=tools item=1/4 retained-tools=2'
echo 'Progress: step=model-provider-tools item=1/2 detail=verify-build-driver'
verify_file "$build_driver" 30072832 \
    628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || exit 1
echo 'Progress: step=model-provider-tools item=2/2 detail=verify-lowerer'
verify_file "$lowerer" 10076160 \
    9eb1ac6a547657a18e68b920b5e8523ae465de556a6f412f652680ccb9dd2d37 || exit 1
echo 'PASS  native model provider phase=tools item=1/4'

echo 'START native model provider phase=compile item=2/4'
"$build_driver" --workspace "$workspace" --project "$model_project" "$work/Model-A.wvb" >/dev/null || exit $?
"$build_driver" --workspace "$workspace" --project "$model_project" "$work/Model-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Model-A.wvb" "$work/Model-B.wvb" || exit 1
"$lowerer" "$work/Model-A.wvb" "$work/Model-A.wvo" >/dev/null || exit $?
"$lowerer" "$work/Model-B.wvb" "$work/Model-B.wvo" >/dev/null || exit $?
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
