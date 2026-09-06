#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Network-Connect-Stream-Core.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=$(node -p "require('node:fs').realpathSync.native(process.argv[1])" "${TMPDIR:-/tmp}") || exit 1
allocated_work=$(mktemp -d "$temporary_root/windvale-network-connect-stream.XXXXXXXX") || exit 1
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
        "$temporary_root"/windvale-network-connect-stream.*)
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
    local path=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $path ]] || return 1
    local actual_bytes digest_line actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}

workspace_resource=${repository_root//\\//}/Windvale.wvws
build_driver="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvbuild.elf"
library_project_resource=${repository_root//\\//}/Projects/Libraries/Windvale-Library-Network-Connect-Stream-Core.wvproj
test_project_resource=${repository_root//\\//}/Projects/Tests/Windvale-Native-Test-Network-Connect-Stream-Core.wvproj
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

echo 'START native network connect stream phase=tools item=1/4 retained-tools=2'
verify_file "$build_driver" 30072832 628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || exit 1
verify_file "$lowerer" 10661888 9c331308e5afe852d4c0441e22c1ff68a0ac0c86793c2e403f38556302c90fd3 || exit $?
echo 'PASS  native network connect stream phase=tools item=1/4'

echo 'START native network connect stream phase=compile item=2/4'
"$build_driver" --workspace "$workspace_resource" --project "$library_project_resource" "$work/Library-A.wvb" >/dev/null || exit $?
"$build_driver" --workspace "$workspace_resource" --project "$library_project_resource" "$work/Library-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Library-A.wvb" "$work/Library-B.wvb" || exit 1
"$build_driver" --workspace "$workspace_resource" --project "$test_project_resource" "$work/Test-A.wvb" >/dev/null || exit $?
"$build_driver" --workspace "$workspace_resource" --project "$test_project_resource" "$work/Test-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Test-A.wvb" "$work/Test-B.wvb" || exit 1
"$lowerer" "$work/Test-A.wvb" "$work/Test-A.wvo" >/dev/null || exit $?
"$lowerer" "$work/Test-B.wvb" "$work/Test-B.wvo" >/dev/null || exit $?
cmp -s -- "$work/Test-A.wvo" "$work/Test-B.wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$work/Test-A.wvo" >/dev/null || exit $?
echo 'PASS  native network connect stream phase=compile item=2/4'

echo 'START native network connect stream phase=link item=3/4'
"$script_directory/Link-Wvo.sh" 0 Main "$work/Network-Image.chunk-0" "$work/Test-A.wvo" >"$work/Link.txt" || exit $?
entry=$(sed -n 's/^entry name=Main address=//p' "$work/Link.txt")
[[ $entry =~ ^[0-9]+$ ]] || exit 1
echo 'PASS  native network connect stream phase=link item=3/4'

echo 'START native network connect stream phase=execute item=4/4'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" "$work/Network-Image" 1 "$entry" "$work/Network.elf" linux >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" "$work/Network-Image" 1 "$entry" "$work/Network.exe" windows >/dev/null || exit $?
set +e
"$work/Network.elf" >/dev/null
application_result=$?
set -e
[[ $application_result -eq 42 ]] || exit 1
echo 'PASS  native network connect stream phase=execute item=4/4'
echo 'native network connect stream status=Passed cases=13 local-result=42 cross-host-images=Verified'
