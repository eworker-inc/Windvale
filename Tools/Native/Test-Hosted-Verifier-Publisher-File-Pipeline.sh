#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
construction="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate"
admission_candidate="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Admission-Candidate"
promoter_candidate="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Promoter-Candidate"
wvb_publisher_candidate="$repository_root/Artifacts/Native-Wvb-Publisher-Candidate"
publisher_tools="$construction/linux-x64"
verifier_candidate="$repository_root/Artifacts/Native-Hosted-Verifier-Application-Candidate/linux-x64-wvverify.elf"
original_temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d \
    "$original_temporary_root/windvale-publisher-file-test.XXXXXXXX") || exit 1
export TMPDIR=$test_directory
cleanup() {
    local status=$?
    export TMPDIR=$original_temporary_root
    rm -f -- "$test_directory"/* "$test_directory"/.wvpublish-* 2>/dev/null || true
    for directory in "$test_directory"/windvale-hosted-verifier-publisher.*; do
        if [[ -d $directory ]]; then
            rm -f -- "$directory"/*
            rmdir -- "$directory"
        fi
    done
    for directory in "$test_directory"/windvale-hosted-verifier-publisher-admitter.*; do
        if [[ -d $directory ]]; then
            rm -f -- "$directory"/*
            rmdir -- "$directory"
        fi
    done
    rmdir -- "$test_directory"
    return "$status"
}
trap cleanup EXIT

total=0
passed=0
phase=initialization
check_hash() {
    local path=$1
    local digest=$2
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' "$digest" "$(basename -- "$path")" |
        sha256sum --check --strict --quiet)
}
check_file() {
    local path=$1
    local bytes=$2
    local digest=$3
    local label=$4
    if [[ ! -f $path ]]; then
        echo "FAIL  hosted-verifier publisher files: missing $label" >&2
        return 1
    fi
    if [[ $(wc -c < "$path") -ne $bytes ]]; then
        echo "FAIL  hosted-verifier publisher files: $label byte length differs" >&2
        return 1
    fi
    if ! check_hash "$path" "$digest"; then
        echo "FAIL  hosted-verifier publisher files: $label digest differs" >&2
        return 1
    fi
}
check_empty() {
    local path=$1
    local label=$2
    if [[ -s $path ]]; then
        echo "FAIL  hosted-verifier publisher files: $label" >&2
        cat -- "$path" >&2
        return 1
    fi
}
check_no_private_scratch() {
    local construction_scratch=("$test_directory"/windvale-hosted-verifier-publisher.*)
    local admitter_scratch=("$test_directory"/windvale-hosted-verifier-publisher-admitter.*)
    local publication_scratch=("$test_directory"/.wvpublish-*)
    if [[ -e ${construction_scratch[0]} || -e ${admitter_scratch[0]} ||
        -e ${publication_scratch[0]} ]]; then
        echo 'FAIL  hosted-verifier publisher files: private scratch remains' >&2
        return 1
    fi
}
pass() {
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    echo "FAIL  hosted-verifier publisher files: $phase" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

total=$((total + 1))
check_file "$construction/SHA256SUMS" 5064 \
    15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 \
    'construction inventory' || fail
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Application-Tool.wvproj" \
    "$test_directory/Publisher-Application-Admission-Tool.wvb" \
    > "$test_directory/Admission-Build.out" \
    2> "$test_directory/Admission-Build.err" || fail
check_empty "$test_directory/Admission-Build.err" \
    'admission source build wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Application-Admission-Tool.wvb" 30778 \
    73c6bfb23c277b6e0384a79bb00a9631709f3d4e9c727e7c27eb9e5dcbbd97f9 \
    'native-built publisher admission WVB' || fail
cmp --silent "$construction/Publisher-Application-Admission-Tool.wvb" \
    "$test_directory/Publisher-Application-Admission-Tool.wvb" || fail
"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
    "$test_directory/Publisher-Application-Admission-Tool.wvb" \
    "$test_directory/Publisher-Application-Admission-Tool.wvo" \
    > "$test_directory/Admission-Lower.out" \
    2> "$test_directory/Admission-Lower.err" || fail
check_empty "$test_directory/Admission-Lower.err" \
    'admission native lowering wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Application-Admission-Tool.wvo" 555690 \
    84857e95e94062206f6ba4b6ccb6a46033b7c82aa0699f90da135428ba74c596 \
    'native-lowered publisher admission WVO' || fail
cmp --silent "$construction/Publisher-Application-Admission-Tool.wvo" \
    "$test_directory/Publisher-Application-Admission-Tool.wvo" || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Native-Hosted-Verifier-Publisher-Promoter.wvproj" \
    "$test_directory/Publisher-Promoter.wvb" \
    > "$test_directory/Promoter-Build.out" \
    2> "$test_directory/Promoter-Build.err" || fail
check_empty "$test_directory/Promoter-Build.err" \
    'promoter source build wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Promoter.wvb" 41268 \
    7ea1cda2842c4258f654ee17deb441c1b06a3fcedfc29f7382e9259b2f3800fe \
    'native-built publisher promoter WVB' || fail
cmp --silent "$construction/Publisher-Promoter.wvb" \
    "$test_directory/Publisher-Promoter.wvb" || fail
"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
    "$test_directory/Publisher-Promoter.wvb" \
    "$test_directory/Publisher-Promoter.wvo" \
    > "$test_directory/Promoter-Lower.out" \
    2> "$test_directory/Promoter-Lower.err" || fail
check_empty "$test_directory/Promoter-Lower.err" \
    'promoter native lowering wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Promoter.wvo" 660123 \
    9ee875a6668b1661087dc6a59384c2427e6ef6febb5c83a4ed936e56cd13b44f \
    'native-lowered publisher promoter WVO' || fail
cmp --silent "$construction/Publisher-Promoter.wvo" \
    "$test_directory/Publisher-Promoter.wvo" || fail
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main \
    "$test_directory/Publisher-Promoter.bin" \
    "$test_directory/Publisher-Promoter.wvo" \
    > "$test_directory/Promoter-Link.out" \
    2> "$test_directory/Promoter-Link.err" || fail
check_empty "$test_directory/Promoter-Link.err" \
    'promoter native link wrote a diagnostic' || fail
grep -Fx 'entry name=Main address=1178' \
    "$test_directory/Promoter-Link.out" >/dev/null || fail
check_file "$test_directory/Publisher-Promoter.bin" 658339 \
    843094cf8ba3de92697568abab6788a276f0ea7bd193e65abfb5c7b56918fb43 \
    'linked publisher promoter fragment' || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Wvb-Publisher.wvproj" \
    "$test_directory/Wvb-Publisher.wvb" \
    > "$test_directory/Wvb-Publisher-Build.out" \
    2> "$test_directory/Wvb-Publisher-Build.err" || fail
check_empty "$test_directory/Wvb-Publisher-Build.err" \
    'WVB publisher source build wrote a diagnostic' || fail
check_file "$test_directory/Wvb-Publisher.wvb" 181772 \
    c90f5325ea409d0710254812e1d434cce712de68385dec74d23eef5a475cf3c4 \
    'metadata-aware WVB publisher candidate' || fail
"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
    "$test_directory/Wvb-Publisher.wvb" \
    "$test_directory/Wvb-Publisher.wvo" \
    > "$test_directory/Wvb-Publisher-Lower.out" \
    2> "$test_directory/Wvb-Publisher-Lower.err" || fail
check_empty "$test_directory/Wvb-Publisher-Lower.err" \
    'WVB publisher native lowering wrote a diagnostic' || fail
check_file "$test_directory/Wvb-Publisher.wvo" 1523708 \
    c1ce50f68e12dc94e56fa848c6f09f707ad117294af5e19f15659b7901c0bf35 \
    'metadata-aware WVB publisher object candidate' || fail
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main \
    "$test_directory/Wvb-Publisher.bin" "$test_directory/Wvb-Publisher.wvo" \
    > "$test_directory/Wvb-Publisher-Link.out" \
    2> "$test_directory/Wvb-Publisher-Link.err" || fail
check_empty "$test_directory/Wvb-Publisher-Link.err" \
    'WVB publisher native link wrote a diagnostic' || fail
grep -Fx 'entry name=Main address=0' \
    "$test_directory/Wvb-Publisher-Link.out" >/dev/null || fail
check_file "$test_directory/Wvb-Publisher.bin" 1520746 \
    98aba65ccfdb0455f9fcb78ad3ffa0ecbe7aa942fcbf9064d179018dec12178a \
    'linked metadata-aware WVB publisher fragment' || fail
pass 'metadata-aware publisher source and refreshed construction inventory'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher.sh" windows \
    "$test_directory/Publisher.exe" > "$test_directory/Windows.out" \
    2> "$test_directory/Windows.err" || fail
check_empty "$test_directory/Windows.err" 'Windows construction wrote a diagnostic' || fail
grep -Fx 'publisher construction status=Valid target=windows bytes=256000' \
    "$test_directory/Windows.out" >/dev/null || fail
check_file "$test_directory/Publisher.exe" 256000 \
    2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12 \
    'Windows publisher' || fail
check_no_private_scratch || fail
pass 'exact cross-target Windows publisher construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher.sh" linux \
    "$test_directory/Publisher.elf" > "$test_directory/Linux.out" \
    2> "$test_directory/Linux.err" || fail
check_empty "$test_directory/Linux.err" 'Linux construction wrote a diagnostic' || fail
grep -Fx 'publisher construction status=Valid target=linux bytes=254965' \
    "$test_directory/Linux.out" >/dev/null || fail
check_file "$test_directory/Publisher.elf" 254965 \
    8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e \
    'Linux publisher' || fail
check_no_private_scratch || fail
pass 'exact Linux publisher construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher-Promoter.sh" \
    windows "$test_directory/Promoter.exe" \
    > "$test_directory/Promoter-Windows.out" \
    2> "$test_directory/Promoter-Windows.err" || fail
check_empty "$test_directory/Promoter-Windows.err" \
    'Windows promoter construction wrote a diagnostic' || fail
grep -Fx 'publisher promoter construction status=Valid target=windows bytes=681472' \
    "$test_directory/Promoter-Windows.out" >/dev/null || fail
check_file "$test_directory/Promoter.exe" 681472 \
    5690fb32c7fec85551e0c5cd58e4f56589a5ad4c09108b5dde86fa9fc7b3fb92 \
    'Windows publisher promoter' || fail
cmp --silent \
    "$promoter_candidate/windows-x64-wvhostverifierpublisherinstall.exe" \
    "$test_directory/Promoter.exe" || fail
check_no_private_scratch || fail
pass 'exact cross-target Windows publisher-promoter construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher-Promoter.sh" \
    linux "$test_directory/Promoter.elf" \
    > "$test_directory/Promoter-Linux.out" \
    2> "$test_directory/Promoter-Linux.err" || fail
check_empty "$test_directory/Promoter-Linux.err" \
    'Linux promoter construction wrote a diagnostic' || fail
grep -Fx 'publisher promoter construction status=Valid target=linux bytes=680949' \
    "$test_directory/Promoter-Linux.out" >/dev/null || fail
check_file "$test_directory/Promoter.elf" 680949 \
    3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5 \
    'Linux publisher promoter' || fail
cmp --silent \
    "$promoter_candidate/linux-x64-wvhostverifierpublisherinstall.elf" \
    "$test_directory/Promoter.elf" || fail
check_no_private_scratch || fail
pass 'exact Linux publisher-promoter construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Wvb-Publisher.sh" \
    windows "$test_directory/Wvb-Publisher.exe" \
    > "$test_directory/Wvb-Publisher-Windows.out" \
    2> "$test_directory/Wvb-Publisher-Windows.err" || fail
check_empty "$test_directory/Wvb-Publisher-Windows.err" \
    'Windows WVB publisher construction wrote a diagnostic' || fail
grep -Fx 'WVB publisher construction status=Valid target=windows bytes=1544192' \
    "$test_directory/Wvb-Publisher-Windows.out" >/dev/null || fail
check_file "$test_directory/Wvb-Publisher.exe" 1544192 \
    0fdb432aa54cc7b9cc4a1d42a438d2b56a29695e06b2369540dac845989751c1 \
    'Windows WVB publisher' || fail
cmp --silent "$wvb_publisher_candidate/windows-x64-wvpublish.exe" \
    "$test_directory/Wvb-Publisher.exe" || fail
check_no_private_scratch || fail
pass 'exact cross-target Windows WVB-publisher construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Wvb-Publisher.sh" \
    linux "$test_directory/Wvb-Publisher.elf" \
    > "$test_directory/Wvb-Publisher-Linux.out" \
    2> "$test_directory/Wvb-Publisher-Linux.err" || fail
check_empty "$test_directory/Wvb-Publisher-Linux.err" \
    'Linux WVB publisher construction wrote a diagnostic' || fail
grep -Fx 'WVB publisher construction status=Valid target=linux bytes=1541109' \
    "$test_directory/Wvb-Publisher-Linux.out" >/dev/null || fail
check_file "$test_directory/Wvb-Publisher.elf" 1541109 \
    7bf4593566401853ab7f551ca5d45125ac0ea3a6c4e34315703785ed7d6cdfb6 \
    'Linux WVB publisher' || fail
cmp --silent "$wvb_publisher_candidate/linux-x64-wvpublish.elf" \
    "$test_directory/Wvb-Publisher.elf" || fail
check_no_private_scratch || fail
pass 'exact Linux WVB-publisher construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher-Admitter.sh" \
    windows "$test_directory/Admitter.exe" \
    > "$test_directory/Admitter-Windows.out" \
    2> "$test_directory/Admitter-Windows.err" || fail
check_empty "$test_directory/Admitter-Windows.err" \
    'Windows admitter construction wrote a diagnostic' || fail
grep -Fx 'publisher admitter construction status=Valid target=windows bytes=570368' \
    "$test_directory/Admitter-Windows.out" >/dev/null || fail
check_file "$test_directory/Admitter.exe" 570368 \
    1407ed428387986e170b4d8394e9a0a6295408ef668d5d6e16d719102428dd4f \
    'Windows publisher admitter' || fail
cmp --silent \
    "$admission_candidate/windows-x64-wvhostverifierpublisheradmit.exe" \
    "$test_directory/Admitter.exe" || fail
check_no_private_scratch || fail
pass 'exact Windows publisher-admitter construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher-Admitter.sh" \
    linux "$test_directory/Admitter.elf" \
    > "$test_directory/Admitter-Linux.out" \
    2> "$test_directory/Admitter-Linux.err" || fail
check_empty "$test_directory/Admitter-Linux.err" \
    'Linux admitter construction wrote a diagnostic' || fail
grep -Fx 'publisher admitter construction status=Valid target=linux bytes=569344' \
    "$test_directory/Admitter-Linux.out" >/dev/null || fail
check_file "$test_directory/Admitter.elf" 569344 \
    27fff54e139228586a6948aa234de60e5d4f5439e6b0616a55c057d4ad8661c2 \
    'Linux publisher admitter' || fail
cmp --silent \
    "$admission_candidate/linux-x64-wvhostverifierpublisheradmit.elf" \
    "$test_directory/Admitter.elf" || fail
check_no_private_scratch || fail
pass 'exact Linux publisher-admitter construction'

total=$((total + 1))
phase='current-host Windows publisher admission'
"$repository_root/Tools/Native/Admit-Hosted-Verifier-Publisher.sh" windows \
    "$test_directory/Publisher.exe" > "$test_directory/Admit-Windows.out" \
    2> "$test_directory/Admit-Windows.err" || fail
check_file "$test_directory/Admit-Windows.out" 58 \
    449d559e4d7f203e2f9d99cffb28144c171559c65344b3cd9335c34ee4be9708 \
    'Windows publisher admission output' || fail
check_empty "$test_directory/Admit-Windows.err" \
    'Windows publisher admission wrote a diagnostic' || fail
phase='current-host Linux publisher admission'
"$repository_root/Tools/Native/Admit-Hosted-Verifier-Publisher.sh" linux \
    "$test_directory/Publisher.elf" > "$test_directory/Admit-Linux.out" \
    2> "$test_directory/Admit-Linux.err" || fail
check_file "$test_directory/Admit-Linux.out" 58 \
    449d559e4d7f203e2f9d99cffb28144c171559c65344b3cd9335c34ee4be9708 \
    'Linux publisher admission output' || fail
check_empty "$test_directory/Admit-Linux.err" \
    'Linux publisher admission wrote a diagnostic' || fail
phase='publisher target-swap rejection'
"$repository_root/Tools/Native/Admit-Hosted-Verifier-Publisher.sh" linux \
    "$test_directory/Publisher.exe" > "$test_directory/Admit-Swap.out" \
    2> "$test_directory/Admit-Swap.err"
[[ $? -eq 2 ]] || fail
check_file "$test_directory/Admit-Swap.err" 61 \
    ffadaf98e0978439eb19a97ccfe2d4c06f810b8c9926d5193eb4827f3c126b89 \
    'target-swap rejection diagnostic' || fail
check_empty "$test_directory/Admit-Swap.out" \
    'target-swap rejection wrote standard output' || fail
phase='wrong-digest publisher creation'
truncate -s 256000 "$test_directory/Wrong-Digest.exe" || fail
phase='wrong-digest publisher rejection'
"$repository_root/Tools/Native/Admit-Hosted-Verifier-Publisher.sh" windows \
    "$test_directory/Wrong-Digest.exe" > "$test_directory/Admit-Corrupt.out" \
    2> "$test_directory/Admit-Corrupt.err"
[[ $? -eq 2 ]] || fail
check_empty "$test_directory/Admit-Corrupt.out" \
    'wrong-digest rejection wrote standard output' || fail
check_file "$test_directory/Admit-Corrupt.err" 61 \
    ffadaf98e0978439eb19a97ccfe2d4c06f810b8c9926d5193eb4827f3c126b89 \
    'wrong-digest rejection diagnostic' || fail
check_file "$test_directory/Wrong-Digest.exe" 256000 \
    24a046dc04fefdb652e4077b41162490b344a4dd45f918505477f84c592f3070 \
    'preserved wrong-digest publisher' || fail
phase='invalid publisher target rejection'
"$repository_root/Tools/Native/Admit-Hosted-Verifier-Publisher.sh" other \
    "$test_directory/Publisher.exe" > "$test_directory/Admit-Usage.out" \
    2> "$test_directory/Admit-Usage.err"
[[ $? -eq 64 ]] || fail
check_empty "$test_directory/Admit-Usage.out" \
    'invalid-target usage wrote standard output' || fail
check_file "$test_directory/Admit-Usage.err" 103 \
    e8018a9ba1fbf52bb988fb7ad5c57bd4b3b7443af6e187a3d0096ebe5c4b36d0 \
    'invalid-target usage diagnostic' || fail
check_file "$test_directory/Publisher.exe" 256000 \
    2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12 \
    'preserved Windows publisher subject' || fail
check_file "$test_directory/Publisher.elf" 254965 \
    8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e \
    'preserved Linux publisher subject' || fail
check_no_private_scratch || fail
pass 'current-host publisher admission matrix'

cp -- "$construction/SHA256SUMS" "$test_directory/Invalid.wvsq" || fail
cp -- "$construction/SHA256SUMS" "$test_directory/Sentinel.wvhv" || fail
total=$((total + 1))
"$publisher_tools/wvhostverifierpublisherbasemetadata.elf" 1 3001 \
    "$test_directory/Invalid.wvsq" "$test_directory/Sentinel.wvhv" \
    > "$test_directory/Reject.out" 2> "$test_directory/Reject.err"
[[ $? -eq 2 ]] || fail
check_empty "$test_directory/Reject.out" 'metadata rejection wrote standard output' || fail
check_empty "$test_directory/Reject.err" 'metadata rejection wrote a diagnostic' || fail
check_file "$test_directory/Invalid.wvsq" 5064 \
    15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 \
    'rejected metadata input' || fail
check_file "$test_directory/Sentinel.wvhv" 5064 \
    15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 \
    'preserved metadata destination' || fail
cp -- "$construction/SHA256SUMS" "$test_directory/Sentinel.wvhr" || fail
"$publisher_tools/wvhostverifierpublisherbaseruntime.elf" \
    "$test_directory/Invalid.wvsq" "$test_directory/Sentinel.wvhr" \
    > "$test_directory/Reject.out" 2> "$test_directory/Reject.err"
[[ $? -eq 2 ]] || fail
check_file "$test_directory/Sentinel.wvhr" 5064 \
    15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 \
    'preserved runtime destination' || fail
pass 'base tools reject malformed input and preserve destinations'

total=$((total + 1))
"$publisher_tools/wvhostverifierpublisherbasemetadata.elf" 1 3001 \
    "$test_directory/Invalid.wvsq" "$test_directory/Invalid.wvsq" \
    > "$test_directory/Alias.out" 2> "$test_directory/Alias.err"
[[ $? -eq 64 ]] || fail
"$publisher_tools/wvhostverifierpublisherbaseruntime.elf" \
    "$test_directory/Invalid.wvsq" "$test_directory/Invalid.wvsq" \
    >> "$test_directory/Alias.out" 2>> "$test_directory/Alias.err"
[[ $? -eq 64 ]] || fail
check_empty "$test_directory/Alias.out" 'alias rejection wrote standard output' || fail
check_empty "$test_directory/Alias.err" 'alias rejection wrote a diagnostic' || fail
check_file "$test_directory/Invalid.wvsq" 5064 \
    15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 \
    'preserved alias input' || fail
pass 'base tools reject exact path aliases'

total=$((total + 1))
phase='Linux publisher installation'
"$repository_root/Tools/Native/Install-Hosted-Verifier-Publisher.sh" \
    "$test_directory/Publisher.elf" "$test_directory/Installed-Publisher.elf" \
    > "$test_directory/Install-Publisher-Linux.out" \
    2> "$test_directory/Install-Publisher-Linux.err" || fail
check_empty "$test_directory/Install-Publisher-Linux.err" \
    'Linux publisher installation wrote a diagnostic' || fail
check_file "$test_directory/Install-Publisher-Linux.out" 117 \
    90150edc169fa87c89a2a631374bd0cb1f15a1f5b21dbd17be48ec6a0d140a30 \
    'Linux publisher installation report' || fail
check_file "$test_directory/Installed-Publisher.elf" 254965 \
    8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e \
    'installed Linux publisher' || fail
cmp --silent "$test_directory/Publisher.elf" \
    "$test_directory/Installed-Publisher.elf" || fail
"$repository_root/Tools/Native/Install-Hosted-Verifier-Publisher.sh" \
    "$test_directory/Publisher.exe" "$test_directory/Installed-Publisher.exe" \
    > "$test_directory/Install-Publisher-Windows.out" \
    2> "$test_directory/Install-Publisher-Windows.err" || fail
check_empty "$test_directory/Install-Publisher-Windows.err" \
    'Windows publisher installation wrote a diagnostic' || fail
check_file "$test_directory/Install-Publisher-Windows.out" 117 \
    dac63802f9402658072559d44b43c27eb03f5027d38b8a3d0993a97f5b356396 \
    'Windows publisher installation report' || fail
check_file "$test_directory/Installed-Publisher.exe" 256000 \
    2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12 \
    'installed Windows publisher' || fail
cmp --silent "$test_directory/Publisher.exe" \
    "$test_directory/Installed-Publisher.exe" || fail
check_no_private_scratch || fail
pass 'current-host promoter installs both exact publishers'

total=$((total + 1))
check_file "$verifier_candidate" 1003520 \
    824e90ae07e82af3d6d0b4cf23bc4d3327fc3367684215171247fa71ab274982 \
    'Linux verifier candidate' || fail
"$test_directory/Installed-Publisher.elf" "$verifier_candidate" \
    "$test_directory/Installed.elf" > "$test_directory/Execute.out" \
    2> "$test_directory/Execute.err" || fail
check_empty "$test_directory/Execute.err" \
    'constructed publisher execution wrote a diagnostic' || fail
check_file "$test_directory/Installed.elf" 1003520 \
    824e90ae07e82af3d6d0b4cf23bc4d3327fc3367684215171247fa71ab274982 \
    'installed verifier' || fail
cmp --silent "$verifier_candidate" "$test_directory/Installed.elf" || fail
check_no_private_scratch || fail
pass 'promoted current-host publisher execution'

total=$((total + 1))
phase='current-host WVB publisher execution'
portable_wvb_candidate="$test_directory/Byte-Construction.wvb"
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Foundation/Byte-Construction.wvproj" \
    "$portable_wvb_candidate" \
    > "$test_directory/Byte-Construction-Build.out" \
    2> "$test_directory/Byte-Construction-Build.err" || fail
check_empty "$test_directory/Byte-Construction-Build.err" \
    'native Byte Construction build wrote a diagnostic' || fail
check_file "$portable_wvb_candidate" 2001 \
    3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8 \
    'native-built portable WVB' || fail
"$wvb_publisher_candidate/linux-x64-wvpublish.elf" \
    "$portable_wvb_candidate" "$test_directory/Published-Portable.wvb" \
    > "$test_directory/Wvb-Publisher-Execute.out" \
    2> "$test_directory/Wvb-Publisher-Execute.err" || fail
check_empty "$test_directory/Wvb-Publisher-Execute.err" \
    'WVB publisher execution wrote a diagnostic' || fail
check_file "$test_directory/Wvb-Publisher-Execute.out" 117 \
    6e988c238fb917825f93e21b147567e04e256be0ec1e4df9c8dc07e19e4fa32e \
    'WVB publisher completion report' || fail
check_file "$test_directory/Published-Portable.wvb" 2001 \
    3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8 \
    'published portable WVB' || fail
cmp --silent "$portable_wvb_candidate" \
    "$test_directory/Published-Portable.wvb" || fail
check_no_private_scratch || fail
pass 'current-host WVB publisher execution'

total=$((total + 1))
phase='current-host metadata-present WVB publisher execution'
metadata_wvb_candidate="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Metadata.wvb"
check_file "$metadata_wvb_candidate" 369 \
    94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa \
    'metadata-present WVB' || fail
"$wvb_publisher_candidate/linux-x64-wvpublish.elf" \
    "$metadata_wvb_candidate" "$test_directory/Published-Metadata.wvb" \
    > "$test_directory/Wvb-Publisher-Metadata.out" \
    2> "$test_directory/Wvb-Publisher-Metadata.err" || fail
check_empty "$test_directory/Wvb-Publisher-Metadata.err" \
    'metadata-present WVB publication wrote a diagnostic' || fail
check_file "$test_directory/Wvb-Publisher-Metadata.out" 117 \
    65e72413bf11bafc3b08abe4c53b8abc65d85f4c4cc576de9dd2ff721418ce1d \
    'metadata-present WVB publication report' || fail
check_file "$test_directory/Published-Metadata.wvb" 369 \
    94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa \
    'published metadata-present WVB' || fail
cmp --silent "$metadata_wvb_candidate" \
    "$test_directory/Published-Metadata.wvb" || fail
check_no_private_scratch || fail
pass 'current-host metadata-present WVB publisher execution'

echo "Tests: $total, Passed: $passed, Failed: 0"
