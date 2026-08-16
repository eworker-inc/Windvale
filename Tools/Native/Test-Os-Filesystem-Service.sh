#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-filesystem-service.XXXXXXXX") || exit 1
cleanup() { case "$work" in "$temporary_root"/windvale-os-filesystem-service.*) rm -f -- "$work"/*; rmdir -- "$work" ;; *) return 1 ;; esac; }
trap cleanup EXIT
verify() { local path=$1 bytes=$2 digest=$3; [[ $(wc -c < "$path") -eq $bytes ]] && printf '%s  %s\n' "$digest" "$path" | sha256sum --check --strict --quiet; }
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Filesystem-Service.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 33871 e2b9279e18676c1a6e3ede3a92d6dee21305c70b14e2f37826ad70b4f2637133 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 360729 fe0826de93dc56153859e17a9d5f939307e3d90acbf8ecb2e5c6bdc7b6a76a5e || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 359171 7268cccb92f81a05820bd6185cf2adfb47cd1c4921a03fc7274b6e7b0b6a63af || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 364656 86a95e3aa17628340a1262400c552bfbedfd46cc0fc14f93731c311873cdec6f || exit 1
"$work/Test.elf" >/dev/null
[[ $? -eq 43 ]] || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 360960 a1fbd73f0fd0581a16dfc8c887beb16d6d4eaa0b2ead67bc611811b43ba09bb4 || exit 1
echo 'native os filesystem service status=Passed cases=19 local-result=43 cross-host-images=Verified'
