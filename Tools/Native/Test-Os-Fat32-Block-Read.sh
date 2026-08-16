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
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Block-Image-Provider.wvproj" "$work/Image-Provider.wvb" >/dev/null || exit $?
verify "$work/Image-Provider.wvb" 4639 60b56a15ad26ff54993e004768439f6a567353debd4a95e05efe60550b89a5bf || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Block-Exchange-State.wvproj" "$work/Exchange.wvb" >/dev/null || exit $?
verify "$work/Exchange.wvb" 20279 820617dc73799c5cbaea318d85a0e6352e539889eb6f3ea525c2dee22cca6690 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 48627 d46c881e3313836e5f6293940e6a35d072344f4541787870dfe2b9bd61a53de6 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 999688 363e58478646a9dac0ce9af53b33f4a650c17282f0019b990fe66c5b81bfa1a6 || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 997504 6cb0edc44d71524197e52a396731a1fb78aae60e2882b1744918961faaf25f91 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 33885 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 999424 38079168f595ec6d488579b241a0e2018ee12ee52244cf905d7b9404de00588f || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 33885 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 1003632 0659762421ef3ddf1c394ca7b4e305fec3bf571da020a6b377fa47020fd7db5c || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 block image and exchange lifecycle status=Passed projects=5 cases=59 local-result=47 cross-host-images=Verified'
