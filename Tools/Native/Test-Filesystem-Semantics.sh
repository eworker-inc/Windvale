#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-filesystem-semantics.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-filesystem-semantics.*) rm -f -- "$work"/*; rmdir -- "$work" ;;
        *) return 1 ;;
    esac
}
trap cleanup EXIT
verify() {
    local path=$1 bytes=$2 digest=$3
    [[ $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" | sha256sum --check --strict --quiet
}
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Filesystem-Semantics.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 9555 f540ca6a7dbaa6ec1e5e8b48dea081288cdb2f6090ce9432bd226a98bf8d4a9d || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 62650 610a53ceeeed2b4ac2d272e897e329e1135d2422fcc590995a26963cf1aaa190 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 61992 5f68d3ba7a6d34750a507fd544eee21e1fbb372de90831e4ec76daa364f227ee || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 69744 d1badd6ebdf1a9f28051465ac197474641815ff210b912373f6779ba11a8c705 || exit 1
"$work/Test.elf" >/dev/null
[[ $? -eq 42 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 64000 f350d86b442a221f5135bd090680f28f976274c702fb75b9ef2e000a5d927194 || exit 1
echo 'native filesystem semantics status=Passed cases=18 local-result=42 cross-host-images=Verified'
