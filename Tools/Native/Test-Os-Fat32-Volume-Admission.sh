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

"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Fat32-Volume-Admission.wvproj" "$work/Policy.wvb" >/dev/null || exit $?
verify "$work/Policy.wvb" 7367 d7f5e96b7d4710f8ba9d68c991239ad1a77b23943ca3d112862b3307168d93e2 || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Fat32-Volume-Admission.wvproj" "$work/Test.wvb" >/dev/null || exit $?
verify "$work/Test.wvb" 13866 1d500f81f31fd79a79bf9710fc4adabdc3247b911c7a08ce73945a7872c45c87 || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.wvo" 133972 85253e6a09bc720a329dee126fa2489e770ebd5c22c56062940b7ae017c2cf2b || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" >/dev/null || exit $?
verify "$work/Test.bin" 133432 f2b218eaae3ef40b501eaf264c638d7b60f3627294aeb53210f2c8d4660b77bc || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify "$work/Test.exe" 135168 165ba51a97e89b7e93ac70d727201ee9c57edb72858b41e538a1d6ef8a0e1512 || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify "$work/Test.elf" 139376 40cd9dbee4a19ac6bd328c2e4eccd0683b7fe0520c88d3cb4180910facd7e048 || exit $?
"$work/Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
echo 'native os fat32 volume admission status=Passed projects=2 cases=25 local-result=47 cross-host-images=Verified'
