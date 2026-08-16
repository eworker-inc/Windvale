#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Shell-1-Parser.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-shell-one-parser.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-shell-one-parser.*)
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

library_project="$repository_root/Projects/Libraries/Windvale-Library-Shell-1-Parser.wvproj"
test_project="$repository_root/Projects/Tests/Windvale-Native-Test-Shell-1-Parser.wvproj"
webassembly_project="$repository_root/Projects/Tests/Windvale-Native-Test-Shell-1-Parser-WebAssembly-Smoke.wvproj"
lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"

echo 'START native shell 1 parser phase=tools item=1/5'
verify_file "$lowerer" 7483392 30ffb3ce953b173d1bbee77c8e440e901806a676f7ec17683b5cfe3953ebb441 || exit $?
echo 'PASS  native shell 1 parser phase=tools item=1/5'

echo 'START native shell 1 parser phase=compile item=2/5'
"$script_directory/Build-Wvb.sh" "$library_project" "$work/Library-A.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$library_project" "$work/Library-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Library-A.wvb" "$work/Library-B.wvb" || exit 1
"$script_directory/Build-Wvb.sh" "$test_project" "$work/Test-A.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$test_project" "$work/Test-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Test-A.wvb" "$work/Test-B.wvb" || exit 1
"$lowerer" "$work/Test-A.wvb" "$work/Test-A.wvo" >/dev/null || exit $?
"$lowerer" "$work/Test-B.wvb" "$work/Test-B.wvo" >/dev/null || exit $?
cmp -s -- "$work/Test-A.wvo" "$work/Test-B.wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$work/Test-A.wvo" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$webassembly_project" "$work/WebAssembly-Smoke.wvb" >/dev/null || exit $?
echo 'PASS  native shell 1 parser phase=compile item=2/5'

echo 'START native shell 1 parser phase=link item=3/5'
"$script_directory/Link-Wvo.sh" 0 Main "$work/Shell-Image.chunk-0" "$work/Test-A.wvo" >"$work/Link.txt" || exit $?
entry=$(sed -n 's/^entry name=Main address=//p' "$work/Link.txt")
[[ $entry =~ ^[0-9]+$ ]] || exit 1
echo 'PASS  native shell 1 parser phase=link item=3/5'

echo 'START native shell 1 parser phase=execute item=4/5'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" "$work/Shell-Image" 1 "$entry" "$work/Shell.elf" linux >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Test-A.wvb" "$work/Shell-Image" 1 "$entry" "$work/Shell.exe" windows >/dev/null || exit $?
set +e
"$work/Shell.elf" >/dev/null
application_result=$?
set -e
[[ $application_result -eq 42 ]] || exit 1
echo 'PASS  native shell 1 parser phase=execute item=4/5'

echo 'START native shell 1 parser phase=webassembly item=5/5'
node --no-liftoff "$repository_root/Tools/Website/Verify-Shell-1-Parser-WebAssembly.mjs" "$work/WebAssembly-Smoke.wvb" >/dev/null || exit $?
echo 'PASS  native shell 1 parser phase=webassembly item=5/5'
echo 'native shell 1 parser status=Passed cases=47 local-result=42 webassembly-smoke=11 cross-host-images=Verified'
