#!/usr/bin/env bash
set -u

if (($# != 0)); then
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
work=$(mktemp -d)
cleanup() {
    rm -rf -- "$work"
}
trap cleanup EXIT

verify() {
    local path=$1
    local bytes=$2
    local digest=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $bytes ]] || return 1
    printf '%s  %s\n' "$digest" "$path" | sha256sum --check --status
}

"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Volume-Admission.wvproj" "$work/Volume.wvb" >/dev/null || exit $?
verify "$work/Volume.wvb" 7654 564793e2af919a9adf7623f28775f653ac89cc642c5bb0cd22624cde896645e8 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Cluster-Chain.wvproj" "$work/Chain.wvb" >/dev/null || exit $?
verify "$work/Chain.wvb" 6359 75470d2a1c48c86754e2f91cd5919306fe73d76c567b87f7490fc87cc1eeeb1a || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-Volume-Admission.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 25600 c978805d2dec9acb9ba08e3fa9466d5f21aab013aff0f6d6c807666ac986bcd9 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 264918 f76bbf03b2ea89434c089480d89f825b55b39c822a07cfcefb8f655400b99c6c || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 264072 13cfc508c60525df095300d4c97696db27795c069eecfdbe6c7030be27362b81 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 2483 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 265728 3caf2067fcdaefcc142d9b9c92c23f2e4056e2b0303f52793a9c50908b1c61ee || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 2483 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 270448 6b19c412245ecaec65d0e83371f1d160b71d030c47d76d63e1cf67d6856b1ae4 || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 volume and chain admission status=Passed projects=3 cases=45 local-result=47 cross-host-images=Verified'
