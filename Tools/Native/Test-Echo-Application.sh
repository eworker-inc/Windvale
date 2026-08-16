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
"$script_directory/Build-Echo-Package.sh" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvpack" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvlock" \
    "$work/Echo-A.wvb" >/dev/null || exit $?
"$script_directory/Build-Echo-Package.sh" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvpack" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvlock" \
    "$work/Echo-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Echo-A.wvb" "$work/Echo-B.wvb" || exit 1
verify_file "$work/Echo-A.wvb" 927 \
    b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713 || exit 1
echo 'PASS  native Windvale echo phase=compile item=1/4'

echo 'START native Windvale echo phase=inspect item=2/4'
node "$script_directory/Verify-Echo-Application.mjs" inspect \
    "$work/Echo-A.wvb" >/dev/null || exit $?
echo 'PASS  native Windvale echo phase=inspect item=2/4'

echo 'START native Windvale echo phase=package item=3/4'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Echo-A.wvb" "$work/Echo.elf" linux >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Echo-A.wvb" "$work/Echo.exe" windows >/dev/null || exit $?
echo 'PASS  native Windvale echo phase=package item=3/4'

echo 'START native Windvale echo phase=execute item=4/4 cases=9'
node "$script_directory/Verify-Echo-Application.mjs" linux \
    "$work/Echo-A.wvb" "$work/Echo.exe" "$work/Echo.elf" \
    >/dev/null || exit $?
echo 'PASS  native Windvale echo phase=execute item=4/4 cases=9'
echo 'native Windvale echo status=Passed cases=9 capabilities=3 metadata=Present wvb=b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713 cross-host-applications=Verified'
