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

"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Chain-Position.wvproj" "$work/Chain.wvb" >/dev/null || exit $?
verify "$work/Chain.wvb" 7186 82eb95c9259e5ee851272c7698f5b2cbea69a9ef585398079346d0bdb7326393 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-File-Read-Transaction.wvproj" "$work/Transaction.wvb" >/dev/null || exit $?
verify "$work/Transaction.wvb" 73587 ed6219dee7ef97ff3bef1fc62bb6fac81c67cf7d51a4613f74c27584aa5da005 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-File-Read-Transaction.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 85489 8471b8c6bafe850b07fdd501999a26921f61fe0a8e80e2e9e9294fcbf0276753 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 2079830 e21ea358dee10eca74bf8c74beff449e38101b6a670220083a8691bd5a80b2d6 || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 2076252 88f38a00c6d8685b986184624e1285770ec2dc48dc74098776a68ab9fac434be || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 2078208 cb77b19db1d2dd43383e40e9577abaf4994a07bd5a8b35bcc5426eea49d87f25 || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 2080880 897af8142136c74800a537b812598e17f95b166312107af0811c96c78e8f99f2 || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 file read transaction status=Passed projects=3 cases=18 exchanges=2 local-result=47 cross-host-images=Verified'
