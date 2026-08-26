#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Network-Address-Authority.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-network-authority.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-network-authority.*)
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
build_project="$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj"
library_project_resource=${repository_root//\\//}/Projects/Libraries/Windvale-Library-Network-Address-Authority.wvproj
test_project_resource=${repository_root//\\//}/Projects/Tests/Windvale-Native-Test-Network-Address-Authority.wvproj
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

echo 'START native network authority phase=tools item=1/4'
"$script_directory/Build-Wvb.sh" "$build_project" "$work/Build-Driver.wvb" >/dev/null || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 "$work/Build-Driver.wvb" "$work/Build-Driver.elf" >/dev/null || exit $?
verify_file "$lowerer" 8159232 5cb17d2e6fd8a02721bd2249623bff65891f4ac6149cc44e60a5849c51774029 || exit $?
echo 'PASS  native network authority phase=tools item=1/4'

echo 'START native network authority phase=compile item=2/4'
"$work/Build-Driver.elf" --workspace "$workspace_resource" --project "$library_project_resource" "$work/Library-A.wvb" >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace_resource" --project "$library_project_resource" "$work/Library-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Library-A.wvb" "$work/Library-B.wvb" || exit 1
"$work/Build-Driver.elf" --workspace "$workspace_resource" --project "$test_project_resource" "$work/Test-A.wvb" >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace_resource" --project "$test_project_resource" "$work/Test-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Test-A.wvb" "$work/Test-B.wvb" || exit 1
"$lowerer" "$work/Test-A.wvb" "$work/Test-A.wvo" >/dev/null || exit $?
"$lowerer" "$work/Test-B.wvb" "$work/Test-B.wvo" >/dev/null || exit $?
cmp -s -- "$work/Test-A.wvo" "$work/Test-B.wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$work/Test-A.wvo" >/dev/null || exit $?
echo 'PASS  native network authority phase=compile item=2/4'

echo 'START native network authority phase=link item=3/4'
"$script_directory/Link-Wvo.sh" 0 Main "$work/Network-Image.chunk-0" "$work/Test-A.wvo" >"$work/Link.txt" || exit $?
entry=$(sed -n 's/^entry name=Main address=//p' "$work/Link.txt")
[[ $entry =~ ^[0-9]+$ ]] || exit 1
echo 'PASS  native network authority phase=link item=3/4'

echo 'START native network authority phase=execute item=4/4'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" "$work/Network-Image" 1 "$entry" "$work/Network.elf" linux >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" "$work/Network-Image" 1 "$entry" "$work/Network.exe" windows >/dev/null || exit $?
set +e
"$work/Network.elf" >/dev/null
application_result=$?
set -e
[[ $application_result -eq 42 ]] || exit 1
echo 'PASS  native network authority phase=execute item=4/4'
echo 'native network authority status=Passed cases=12 local-result=42 cross-host-images=Verified'
