#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Standard-Byte-Output-Core.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P) || exit 1
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P) || exit 1
native="$repository_root/Tools/Native"
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-standard-byte-output.XXXXXXXX") || exit 1

cleanup() {
    case "$work" in
        "$temporary_root"/windvale-standard-byte-output.*)
            rm -rf -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

workspace=$(realpath "$repository_root/Windvale.wvws")
build_driver="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvbuild.elf"
library_project=$(realpath "$repository_root/Projects/Libraries/Windvale-Library-Standard-Byte-Output-Core.wvproj")
test_project=$(realpath "$repository_root/Projects/Tests/Windvale-Native-Test-Standard-Byte-Output-Core.wvproj")
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

verify_file() {
    local path=$1 expected_bytes=$2 expected_hash=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum "$path" | cut -d ' ' -f 1) == "$expected_hash" ]]
}

echo 'START native standard byte output phase=tools item=1/4 retained-tools=2'
verify_file "$build_driver" 30072832 628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || exit 1
verify_file "$lowerer" 10657792 4f7aa0abdf870ada362defee6258ba4e6b8ce1f0f67329563d20ed3eb6c9ff24 || exit 1
echo 'PASS  native standard byte output phase=tools item=1/4'

echo 'START native standard byte output phase=compile item=2/4'
"$build_driver" --workspace "$workspace" --project "$library_project" "$work/Library-A.wvb" >/dev/null || exit 1
"$build_driver" --workspace "$workspace" --project "$library_project" "$work/Library-B.wvb" >/dev/null || exit 1
cmp -s "$work/Library-A.wvb" "$work/Library-B.wvb" || exit 1
verify_file "$work/Library-A.wvb" 55898 d80e98f785e8dfab0e357a7d74457f07775141bf31d2773e2d7745c061a7aa26 || exit 1
"$build_driver" --workspace "$workspace" --project "$test_project" "$work/Test-A.wvb" >/dev/null || exit 1
"$build_driver" --workspace "$workspace" --project "$test_project" "$work/Test-B.wvb" >/dev/null || exit 1
cmp -s "$work/Test-A.wvb" "$work/Test-B.wvb" || exit 1
verify_file "$work/Test-A.wvb" 75874 7fba163fd1087c324bf640879b72a5208375e49ab298950ba97d987a7c2a4d17 || exit 1
"$lowerer" "$work/Test-A.wvb" "$work/Test-A.wvo" >/dev/null || exit 1
"$lowerer" "$work/Test-B.wvb" "$work/Test-B.wvo" >/dev/null || exit 1
cmp -s "$work/Test-A.wvo" "$work/Test-B.wvo" || exit 1
verify_file "$work/Test-A.wvo" 2650952 2abd417b75f497c6f1b9c99395101fec722597bb38ce436ea1bea3fa9ba476b2 || exit 1
"$native/Check-Wvo.sh" "$work/Test-A.wvo" >/dev/null || exit 1
echo 'PASS  native standard byte output phase=compile item=2/4'

echo 'START native standard byte output phase=link item=3/4'
"$native/Link-Wvo.sh" 0 Main "$work/Output-Image.chunk-0" "$work/Test-A.wvo" >"$work/Link.txt" || exit 1
entry=$(sed -n 's/^entry name=Main address=//p' "$work/Link.txt")
[[ $entry == 356514 ]] || exit 1
echo 'PASS  native standard byte output phase=link item=3/4'

echo 'START native standard byte output phase=execute item=4/4'
"$native/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" \
    "$work/Output-Image" 1 "$entry" "$work/Output.elf" linux >/dev/null || exit 1
"$native/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" \
    "$work/Output-Image" 1 "$entry" "$work/Output.exe" windows >/dev/null || exit 1
"$work/Output.elf" >/dev/null
local_result=$?
[[ $local_result -eq 42 ]] || exit 1
echo 'PASS  native standard byte output phase=execute item=4/4'
echo 'native standard byte output status=Passed cases=10 local-result=42 cross-host-images=Verified'
