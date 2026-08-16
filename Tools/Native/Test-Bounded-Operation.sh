#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-bounded-operation.XXXXXXXX") || exit 1
cleanup() { case "$work" in "$temporary_root"/windvale-bounded-operation.*) rm -f -- "$work"/*; rmdir -- "$work" ;; *) return 1 ;; esac; }
trap cleanup EXIT
verify() { local path=$1 bytes=$2 digest=$3; [[ $(wc -c < "$path") -eq $bytes ]] && printf '%s  %s\n' "$digest" "$path" | sha256sum --check --strict --quiet; }
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Bounded-Operation.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 12769 dac9582ae8ea2202fc16e5e15020136b63a668c722dbdab6863a98e07d7ff477 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 276790 3a1db405252a444d41c2d2c9a8042ea27de2dbe6e665ed8dc75ed8526d6595a5 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 276445 94b78338cdb4a8a435c656e5171c2304c0a77ed828bb98bcf28d7a841e480c74 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 282736 2366927636b460142314362fd0bb4d7640d9426bb6dd375d5d242c12a6a99c55 || exit 1
"$work/Test.elf" >/dev/null
[[ $? -eq 44 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 278528 85836ff4eb681ba8a6d9a8f3569f3c47a3fa062a78fca40313ae2a9d4b360002 || exit 1
echo 'native bounded operation status=Passed cases=12 local-result=44 cross-host-images=Verified'
