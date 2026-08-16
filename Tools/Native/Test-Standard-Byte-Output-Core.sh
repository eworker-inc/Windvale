#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Standard-Byte-Output-Core.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P) || exit 1
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P) || exit 1
native="$repository_root/Tools/Native"
recovery_commit=4aca9935679b67f46bfb97f37c2e566980bbab68
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-standard-byte-output.XXXXXXXX") || exit 1
recovery="$work/compiler-recovery"
worktree_added=0

cleanup() {
    if [[ $worktree_added -eq 1 ]]; then
        git -C "$repository_root" worktree remove "$recovery" >/dev/null 2>&1 || true
    fi
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
library_project=$(realpath "$repository_root/Projects/Libraries/Windvale-Library-Standard-Byte-Output-Core.wvproj")
test_project=$(realpath "$repository_root/Projects/Tests/Windvale-Native-Test-Standard-Byte-Output-Core.wvproj")
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

verify_file() {
    local path=$1 expected_bytes=$2 expected_hash=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum "$path" | cut -d ' ' -f 1) == "$expected_hash" ]]
}

echo 'START native standard byte output phase=tools item=1/4'
git -C "$repository_root" cat-file -e "$recovery_commit^{commit}" || exit 1
git -C "$repository_root" worktree add --detach "$recovery" "$recovery_commit" >/dev/null 2>&1 || exit 1
worktree_added=1
"$recovery/Tools/Native/Build-Wvb.sh" \
    "$recovery/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
    "$work/Build-Driver.wvb" >/dev/null || exit 1
verify_file "$work/Build-Driver.wvb" 1121370 ed5bbceaa0f1b4d889a7d17fe1d138d0bd5a01a593f6925ba34023ff0b0960ef || exit 1
"$native/Package-Segmented-Compiler-Wvb.sh" 2 "$work/Build-Driver.wvb" \
    "$work/Build-Driver.elf" --development-cache >/dev/null || exit 1
verify_file "$lowerer" 7491584 deb75ead2af0d06d2357cdf88d8cf58fefd284bf4834e6489198b517f3a4908e || exit 1
echo 'PASS  native standard byte output phase=tools item=1/4'

echo 'START native standard byte output phase=compile item=2/4'
"$work/Build-Driver.elf" --workspace "$workspace" --project "$library_project" "$work/Library-A.wvb" >/dev/null || exit 1
"$work/Build-Driver.elf" --workspace "$workspace" --project "$library_project" "$work/Library-B.wvb" >/dev/null || exit 1
cmp -s "$work/Library-A.wvb" "$work/Library-B.wvb" || exit 1
verify_file "$work/Library-A.wvb" 55898 d80e98f785e8dfab0e357a7d74457f07775141bf31d2773e2d7745c061a7aa26 || exit 1
"$work/Build-Driver.elf" --workspace "$workspace" --project "$test_project" "$work/Test-A.wvb" >/dev/null || exit 1
"$work/Build-Driver.elf" --workspace "$workspace" --project "$test_project" "$work/Test-B.wvb" >/dev/null || exit 1
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
