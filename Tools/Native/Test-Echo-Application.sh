#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Echo-Application.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-echo-application.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-echo-application.*)
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

echo 'START native Windvale echo phase=compile item=1/4'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Applications/Windvale-Echo.wvproj" \
    "$work/Echo-A.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Applications/Windvale-Echo.wvproj" \
    "$work/Echo-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Echo-A.wvb" "$work/Echo-B.wvb" || exit 1
verify_file "$work/Echo-A.wvb" 813 \
    5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64 || exit 1
echo 'PASS  native Windvale echo phase=compile item=1/4'

echo 'START native Windvale echo phase=inspect item=2/4'
"$script_directory/Inspect-Wvb.sh" "$work/Echo-A.wvb" >"$work/Inspect.txt" || exit $?
echo 'PASS  native Windvale echo phase=inspect item=2/4'

echo 'START native Windvale echo phase=package item=3/4'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Echo-A.wvb" "$work/Echo.elf" linux >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Echo-A.wvb" "$work/Echo.exe" windows >/dev/null || exit $?
echo 'PASS  native Windvale echo phase=package item=3/4'

echo 'START native Windvale echo phase=execute item=4/4 cases=9'
node "$script_directory/Verify-Echo-Application.mjs" linux \
    "$work/Echo-A.wvb" "$work/Echo.exe" "$work/Echo.elf" "$work/Inspect.txt" \
    >/dev/null || exit $?
echo 'PASS  native Windvale echo phase=execute item=4/4 cases=9'
echo 'native Windvale echo status=Passed cases=9 capabilities=3 wvb=5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64 cross-host-applications=Verified'
