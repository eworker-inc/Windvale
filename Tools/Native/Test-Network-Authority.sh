#!/usr/bin/env bash
set -u

if [[ $# -ne 0 ]]; then
    exit 64
fi

script_directory=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
repository_root=$(cd -- "$script_directory/../.." && pwd)
work=$(mktemp -d "${TMPDIR:-/tmp}/windvale-network-authority.XXXXXX") || exit 1
status=1
trap 'rm -rf -- "$work"' EXIT

verify_file() {
    local path=$1
    local expected_length=$2
    local expected_sha=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c <"$path") -eq $expected_length ]] || return 1
    [[ $(sha256sum "$path" | cut -d ' ' -f 1) == "$expected_sha" ]]
}

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Network-Authority.wvproj" \
    "$work/Test.wvb" >/dev/null || exit $?
verify_file "$work/Test.wvb" 7813 \
    1d3be8e490b5a7927156a57b019ce7fef2956d8793c8085f77d01afa395bf8e4 || exit 1
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Test.wvb" "$work/Test.wvo" \
    >/dev/null || exit $?
verify_file "$work/Test.wvo" 79489 \
    c2383d99750c00c972fdf366ade7f15dbbd7c9829b01f6d5cf9d96344b648bc1 || exit 1
"$script_directory/Link-Wvo.sh" 0 Main "$work/Test.bin" "$work/Test.wvo" \
    >/dev/null || exit $?
verify_file "$work/Test.bin" 79144 \
    e4d0002f808b7bd3b956436a37aa606cab945713053f50274bc1d97b0d66506d || exit 1
"$script_directory/Package-Console.sh" windows-x64-console-v1 \
    "$work/Test.bin" 0 "$work/Test.exe" >/dev/null || exit $?
verify_file "$work/Test.exe" 80896 \
    bcbeaf820e970c7369a942ffb2cf407a92c3f399002c2fb478b96588986449a3 || exit 1
"$script_directory/Package-Console.sh" linux-x64-console-v1 \
    "$work/Test.bin" 0 "$work/Test.elf" >/dev/null || exit $?
verify_file "$work/Test.elf" 86128 \
    95c342a6a027baec2f41aa2959cc78e855463619dd04eb4eaca9aeaa4ac73b9e || exit 1
"$work/Test.elf" >/dev/null || result=$?
result=${result:-0}
[[ $result -eq 45 ]] || exit 1

echo 'native network authority status=Passed cases=18 local-result=45 cross-host-images=Verified'
status=0
exit "$status"
