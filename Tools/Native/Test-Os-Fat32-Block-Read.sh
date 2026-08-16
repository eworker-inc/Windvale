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
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 8925 219834cc34ae973e955c885e1878b03b85da8a90cf5f3b066918304b830a28ed || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 107828 ca7679c6e815eb44ef3ba6e39d34abb2e1bed3f4bfe8b3169663ae151a22d50d || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 107168 bf669862fb9b1225385ca6ca11de79a6d6cb3627e231e6d52f27d85e9c2bb556 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 3124 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 109056 77dc582c80e836341a82801ceaf25715aac2dec00b58a6e8bab772ec41bef94c || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 3124 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 114800 f1fb291859372e171c3adf6a98019425382ac3a5f908829be05dfb20ada67c08 || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 block read transaction status=Passed projects=2 cases=14 local-result=47 cross-host-images=Verified'
