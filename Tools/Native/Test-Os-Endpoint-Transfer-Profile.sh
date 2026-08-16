#!/usr/bin/env bash
set -u

if (($# != 0)); then
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
work=$(mktemp -d)
cleanup() { rm -rf -- "$work"; }
trap cleanup EXIT

verify() {
    local path=$1 bytes=$2 digest=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $bytes ]] || return 1
    printf '%s  %s\n' "$digest" "$path" | sha256sum --check --status
}

"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Endpoint-Transfer-Profile.wvproj" "$work/Profile.wvb" >/dev/null || exit $?
verify "$work/Profile.wvb" 9657 ef7801e909dd24105e6260cb8f88845e1b8d966fb90dc78350f63eeb8d1bf619 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Endpoint-Transfer-Profile.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 19678 5bf6f45a3090056931b63d17edf0a14db8ac05169f5422b3a75a3c6703d82846 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 281515 dfd6bd9711e3d51c69965a662994281b80d4d4326a9c70e6004ca2c809f5cb43 || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 280932 4ce17841274e3e94a2b3c0878119d715faae51d08b84e24e779e158a0a5e2594 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 282624 359a5e4a9eeb2fc26bd15f73bc91a1c7947b5c6c06da25d2e323ddfcb914d026 || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 286832 7f4b47b73a04d7b24f323dbc00147393d26607c06968e59a57420c727a43de58 || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os endpoint transfer profiles status=Passed projects=2 cases=29 profiles=3 max-pages=17 local-result=47 cross-host-images=Verified'
