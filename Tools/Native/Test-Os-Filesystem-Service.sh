#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-filesystem-service.XXXXXXXX") || exit 1
cleanup() { case "$work" in "$temporary_root"/windvale-os-filesystem-service.*) rm -f -- "$work"/*; rmdir -- "$work" ;; *) return 1 ;; esac; }
trap cleanup EXIT
verify() {
    local path=$1 bytes=$2 digest=$3 actual_bytes actual_digest status=0
    if [[ ! -f $path ]]; then
        echo "FAIL native os filesystem service artifact=$(basename -- "$path") check=exists" >&2
        return 1
    fi
    actual_bytes=$(wc -c < "$path")
    if [[ $actual_bytes -ne $bytes ]]; then
        echo "FAIL native os filesystem service artifact=$(basename -- "$path") check=bytes expected=$bytes actual=$actual_bytes" >&2
        status=1
    fi
    actual_digest=$(sha256sum "$path" | cut -d' ' -f1)
    if [[ $actual_digest != "$digest" ]]; then
        echo "FAIL native os filesystem service artifact=$(basename -- "$path") check=sha256 expected=$digest actual=$actual_digest" >&2
        status=1
    fi
    return "$status"
}
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Filesystem-Service.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 33871 e2b9279e18676c1a6e3ede3a92d6dee21305c70b14e2f37826ad70b4f2637133 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 360745 8850cb504be473f7aef51fc07598c070cf6e82b2b445a702f1948efd492c28de || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 359187 da1c1d9d2e9048e35da9ba7661ee9f086dd1e566aa7ec41f0a79559063af76dd || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 364656 fca9aa51babcfd33b6ab051d565b16089c99f37a8e577e68f862bdcbb13548c4 || exit 1
"$work/Test.elf" >/dev/null
[[ $? -eq 43 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 360960 74aa3bde234216a0aa787585ac88ab1a748cca8bc181693412d67dfe3e92860c || exit 1
echo 'native os filesystem service status=Passed cases=19 local-result=43 cross-host-images=Verified'
