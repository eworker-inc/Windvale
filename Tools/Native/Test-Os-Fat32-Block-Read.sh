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
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 15872 f342cd005c88851805a82fd68d36deb9795b187862f71f250c9a455e9a6ba626 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 203304 a53fe125aa70aa9708edba372d90d2f59ed7dcf8c56338ce2d39ff6749f64c87 || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 202208 1435288069e2eacab975e4ed998d0e268fe06cfb3bd4fadcdb4cd4dbeb804ed7 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 7930 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 204288 29ec32f60c50262bb386cc15288d227205beea1b5f0ed63c888826cc7fed59fc || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 7930 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 209008 e4da4f92c0f12367d662a93a489ce8246ca1d92baca3a93b042b4614967ac853 || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 block provider protocol status=Passed projects=3 cases=22 local-result=47 cross-host-images=Verified'
