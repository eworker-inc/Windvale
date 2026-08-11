#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Aot-Composition-Probe.sh <existing-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64

check_file() {
    local path=$1 expected_sha256=$2 label=$3
    local digest_line actual_sha256
    [[ -f $path ]] || {
        echo "The native AOT composition probe $label is missing." >&2
        return 1
    }
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || {
        echo "The native AOT composition probe $label identity differs." >&2
        return 1
    }
}

wvb="$output_root/Return-42.wvb"
wvo="$output_root/Return-42.wvo"
image="$output_root/Return-42.bin"
map="$output_root/Return-42.wvmap"
windows_application="$output_root/Return-42.exe"
linux_application="$output_root/Return-42.elf"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" "$wvb" \
    >"$output_root/Build.out" 2>"$output_root/Build.err" || exit $?
[[ ! -s $output_root/Build.err ]] || exit 1
check_file "$wvb" \
    7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 \
    'WVB' || exit 1

"$script_directory/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" \
    >"$output_root/Lower.out" 2>"$output_root/Lower.err" || exit $?
[[ ! -s $output_root/Lower.err ]] || exit 1
"$script_directory/Verify-Wvo.sh" "$wvo" \
    >"$output_root/Verify.out" 2>"$output_root/Verify.err" || exit $?
[[ ! -s $output_root/Verify.err ]] || exit 1
check_file "$wvo" \
    0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5 \
    'WVO' || exit 1

"$script_directory/Link-Wvo.sh" 1048576 Main "$image" "$wvo" \
    >"$map" 2>"$output_root/Link.err" || exit $?
[[ ! -s $output_root/Link.err ]] || exit 1
check_file "$image" \
    7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408 \
    'flat image' || exit 1
check_file "$map" \
    857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc \
    'link map' || exit 1

"$script_directory/Package-Console.sh" windows-x64-console-v1 \
    "$image" 0 "$windows_application" \
    >"$output_root/Windows.out" 2>"$output_root/Windows.err" || exit $?
[[ ! -s $output_root/Windows.err ]] || exit 1
check_file "$windows_application" \
    8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6 \
    'Windows application' || exit 1

"$script_directory/Package-Console.sh" linux-x64-console-v1 \
    "$image" 0 "$linux_application" \
    >"$output_root/Linux.out" 2>"$output_root/Linux.err" || exit $?
[[ ! -s $output_root/Linux.err ]] || exit 1
check_file "$linux_application" \
    fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7 \
    'Linux application' || exit 1
[[ -x $linux_application ]] || {
    echo 'The native AOT composition probe Linux application is not executable.' >&2
    exit 1
}

echo 'native AOT composition probe status=Complete artifacts=6'
