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

"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Block-Read-Transaction.wvproj" "$work/Policy.wvb" >/dev/null || exit $?
verify "$work/Policy.wvb" 5036 8e6d447b4ee2bcbb6b549d37d42d1093ac7c1aa18ffacaa3f2e09bb4fcc913b5 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Block-Provider-Protocol.wvproj" "$work/Protocol.wvb" >/dev/null || exit $?
verify "$work/Protocol.wvb" 8726 5d37a54cc6e6763aca7f1e2c76d128cedae49d5febeef6ffa85d1d4de7e1348e || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Block-Exchange-State.wvproj" "$work/Exchange.wvb" >/dev/null || exit $?
verify "$work/Exchange.wvb" 20279 820617dc73799c5cbaea318d85a0e6352e539889eb6f3ea525c2dee22cca6690 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 34028 00f91945f789b8b8349ea54089b746f1de3de596c8ff7588a1b57277820a2dc9 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 805998 5f9cd5bd8bb2f2ffd2fe98b78d12320e068ac07d9e90d28f8bdeaf75a9139342 || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 804176 2f1b7f97db4f39c867c8f421f238345469897a074c0476a7cedcbdd962324b16 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 23090 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 805888 95e78a464a2a2ab5aeba45374ac39d02836ce4720cd8e6530d88aec595360991 || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 23090 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 811120 4e6d3527cbcb1fd63cfd9ceebd6d23feed516d7c65a30e49624bd158148ade6b || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 block exchange lifecycle status=Passed projects=4 cases=37 local-result=47 cross-host-images=Verified'
