#!/usr/bin/env bash
set -uo pipefail

[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
work=$(mktemp -d) || exit 1
trap 'rm -rf -- "$work"' EXIT

verify() {
    local path=$1 bytes=$2 digest=$3
    [[ -f $path && $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

run_case() {
    local name=$1 project=$2 wvb_bytes=$3 wvb_digest=$4
    local wvo_bytes=$5 wvo_digest=$6 bin_bytes=$7 bin_digest=$8 result=$9
    local windows_bytes=${10} windows_digest=${11}
    local linux_bytes=${12} linux_digest=${13}
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Tests/$project" "$work/$name.wvb" >/dev/null || return 1
    verify "$work/$name.wvb" "$wvb_bytes" "$wvb_digest" || return 1
    "$script_directory/Lower-Wvb-To-Wvo.sh" "$work/$name.wvb" "$work/$name.wvo" >/dev/null || return 1
    verify "$work/$name.wvo" "$wvo_bytes" "$wvo_digest" || return 1
    "$script_directory/Link-Wvo.sh" 0 Main "$work/$name.bin" "$work/$name.wvo" >/dev/null || return 1
    verify "$work/$name.bin" "$bin_bytes" "$bin_digest" || return 1
    "$script_directory/Package-Console.sh" linux-x64-console-v1 \
        "$work/$name.bin" 0 "$work/$name.elf" >/dev/null || return 1
    verify "$work/$name.elf" "$linux_bytes" "$linux_digest" || return 1
    "$work/$name.elf" >/dev/null
    [[ $? -eq $result ]] || return 1
    "$script_directory/Package-Console.sh" windows-x64-console-v1 \
        "$work/$name.bin" 0 "$work/$name.exe" >/dev/null || return 1
    verify "$work/$name.exe" "$windows_bytes" "$windows_digest"
}

echo 'step=filesystem-record item=1/3'
run_case Record Windvale-Native-Test-Os-X64-Process-Filesystem-Record-Emission.wvproj \
    16323 c7d1ab82c53d66a191936cc1b0e0c53bc18f806f486e341d69b242c8de24cbe6 \
    236160 44be4d5daca222db55ee3e13871ade20d55f8094395d09a4d8c5100f091d86a0 \
    234416 b89bd95c3e5a14476df7b9d8a595e240aa1a6ae1d946c59b42ca33c5ffd798cb \
    50 236544 0773673d06d069e7478a3ce058f131afe4c136f12350af9c6919375a9df4bf83 \
    241776 17cb0467e32360fe3ba8f70f906e6376625545a8a69a0b5169081e751762baae || exit 1
echo 'step=filesystem-paging item=2/3'
run_case Paging Windvale-Native-Test-Os-X64-Process-Filesystem-Paging-Emission.wvproj \
    14615 1e626b1775f34af1356a10287c23b04523f39cd9971e57f60bf1105c3ec6aeae \
    206117 ec0fde1786a994e0bcb7bad1be7b49e1206ef7e0b136b7e8a9bbc077a57b8027 \
    204075 6f38a21ebbba64e6837dad02d71d911156b456a2d59b9327a62dabdddad92a82 \
    51 205824 9fad8f8be8bfabaabd0d800ba3df4a8533ed7dc7df62a804d19d04a6a2e0db85 \
    209008 aebe8ae480c0e57ff1014030152d987091d463f047b8cdcc3c1a7ad83b887cd1 || exit 1
echo 'step=filesystem-image item=3/3'
run_case Image Windvale-Native-Test-Os-X64-Process-Filesystem-Image-Emission.wvproj \
    12520 c2630d4100b2e3a8447f850ac0be9ffb21160431de199908584ed4b08c49a743 \
    172259 d8f4b3b4567ac919293d97c78ee1833a9042bb57ead53ea97eb373b2accc137b \
    170855 a4cfd98ae0ce7450ac5f2f9ec0a192d3b22f9502cff6843afbf87ed2b069fe42 \
    52 172544 8b1d4296461f2e553ba1b4ed2f42bdfdcd4706cb4f88de74bfaa408b37b2d384 \
    176240 621ed9294cafae354b4b20099cb8bc0ef1f5fc1a5f1593b9efade98ce2aceee4 || exit 1

echo 'native os x64 filesystem machine emission status=Passed cases=3 geometry=85/81/48/33'
