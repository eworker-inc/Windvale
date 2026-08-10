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
portable_wvb_candidate="$repository_root/Artifacts/Byte-Construction.wvb"
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
    12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5 \
    'construction inventory' || fail
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-Hosted-Verifier-Publisher-Application-Tool.wvproj" \
    "$test_directory/Publisher-Application-Admission-Tool.wvb" \
    > "$test_directory/Admission-Build.out" \
    2> "$test_directory/Admission-Build.err" || fail
check_empty "$test_directory/Admission-Build.err" \
    'admission source build wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Application-Admission-Tool.wvb" 30778 \
    b4e0a2ee04de6cfff0efc723c57031bf5cfcd6706e3156525ce2157c5f287d07 \
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
    88cc97665cfd0de14f2c9ac6c80dfd985edc508fccdc3d9b887da740cd034e23 \
    'native-lowered publisher admission WVO' || fail
cmp --silent "$construction/Publisher-Application-Admission-Tool.wvo" \
    "$test_directory/Publisher-Application-Admission-Tool.wvo" || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-Hosted-Verifier-Publisher-Promoter.wvproj" \
    "$test_directory/Publisher-Promoter.wvb" \
    > "$test_directory/Promoter-Build.out" \
    2> "$test_directory/Promoter-Build.err" || fail
check_empty "$test_directory/Promoter-Build.err" \
    'promoter source build wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Promoter.wvb" 41268 \
    c0c7c88996ef837bc5a2ec3ceb1de61254b025fbd6504e4f3d7dc055c4140672 \
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
    ba5d9c5afde115fede472369d24c3d1fe466806de523773d2e445e6a9e004667 \
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
    e06189a37c038a5237787ffd16fb53466df3d10519efd4129b219bd814f4def2 \
    'linked publisher promoter fragment' || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Windvale-Wvb-Publisher.wvproj" \
    "$test_directory/Wvb-Publisher.wvb" \
    > "$test_directory/Wvb-Publisher-Build.out" \
    2> "$test_directory/Wvb-Publisher-Build.err" || fail
check_empty "$test_directory/Wvb-Publisher-Build.err" \
    'WVB publisher source build wrote a diagnostic' || fail
check_file "$test_directory/Wvb-Publisher.wvb" 159770 \
    8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96 \
    'native-built WVB publisher WVB' || fail
cmp --silent "$construction/Wvb-Publisher.wvb" \
    "$test_directory/Wvb-Publisher.wvb" || fail
"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
    "$test_directory/Wvb-Publisher.wvb" \
    "$test_directory/Wvb-Publisher.wvo" \
    > "$test_directory/Wvb-Publisher-Lower.out" \
    2> "$test_directory/Wvb-Publisher-Lower.err" || fail
check_empty "$test_directory/Wvb-Publisher-Lower.err" \
    'WVB publisher native lowering wrote a diagnostic' || fail
check_file "$test_directory/Wvb-Publisher.wvo" 1319377 \
    edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3 \
    'native-lowered WVB publisher WVO' || fail
cmp --silent "$construction/Wvb-Publisher.wvo" \
    "$test_directory/Wvb-Publisher.wvo" || fail
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main \
    "$test_directory/Wvb-Publisher.bin" "$test_directory/Wvb-Publisher.wvo" \
    > "$test_directory/Wvb-Publisher-Link.out" \
    2> "$test_directory/Wvb-Publisher-Link.err" || fail
check_empty "$test_directory/Wvb-Publisher-Link.err" \
    'WVB publisher native link wrote a diagnostic' || fail
grep -Fx 'entry name=Main address=0' \
    "$test_directory/Wvb-Publisher-Link.out" >/dev/null || fail
check_file "$test_directory/Wvb-Publisher.bin" 1317613 \
    9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271 \
    'linked WVB publisher fragment' || fail
pass 'publisher construction inventory'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher.sh" windows \
    "$test_directory/Publisher.exe" > "$test_directory/Windows.out" \
    2> "$test_directory/Windows.err" || fail
check_empty "$test_directory/Windows.err" 'Windows construction wrote a diagnostic' || fail
grep -Fx 'publisher construction status=Valid target=windows bytes=256000' \
    "$test_directory/Windows.out" >/dev/null || fail
check_file "$test_directory/Publisher.exe" 256000 \
    17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96 \
    'Windows publisher' || fail
check_no_private_scratch || fail
pass 'exact cross-target Windows publisher construction'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher.sh" linux \
    "$test_directory/Publisher.elf" > "$test_directory/Linux.out" \
    2> "$test_directory/Linux.err" || fail
check_empty "$test_directory/Linux.err" 'Linux construction wrote a diagnostic' || fail
grep -Fx 'publisher construction status=Valid target=linux bytes=254917' \
    "$test_directory/Linux.out" >/dev/null || fail
check_file "$test_directory/Publisher.elf" 254917 \
    babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97 \
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
    598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23 \
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
grep -Fx 'publisher promoter construction status=Valid target=linux bytes=680901' \
    "$test_directory/Promoter-Linux.out" >/dev/null || fail
check_file "$test_directory/Promoter.elf" 680901 \
    422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58 \
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
grep -Fx 'WVB publisher construction status=Valid target=windows bytes=1340928' \
    "$test_directory/Wvb-Publisher-Windows.out" >/dev/null || fail
check_file "$test_directory/Wvb-Publisher.exe" 1340928 \
    71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3 \
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
grep -Fx 'WVB publisher construction status=Valid target=linux bytes=1340357' \
    "$test_directory/Wvb-Publisher-Linux.out" >/dev/null || fail
check_file "$test_directory/Wvb-Publisher.elf" 1340357 \
    7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c \
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
    4742ee299759728be1b72fed3d3b42620c21b10f77aed12cf150c1549b177b53 \
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
    b03788fad58ce071788b2f30945ed1dc0992559bb04b6cad04e719ff1114dc0a \
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
    17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96 \
    'preserved Windows publisher subject' || fail
check_file "$test_directory/Publisher.elf" 254917 \
    babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97 \
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
    12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5 \
    'rejected metadata input' || fail
check_file "$test_directory/Sentinel.wvhv" 5064 \
    12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5 \
    'preserved metadata destination' || fail
cp -- "$construction/SHA256SUMS" "$test_directory/Sentinel.wvhr" || fail
"$publisher_tools/wvhostverifierpublisherbaseruntime.elf" \
    "$test_directory/Invalid.wvsq" "$test_directory/Sentinel.wvhr" \
    > "$test_directory/Reject.out" 2> "$test_directory/Reject.err"
[[ $? -eq 2 ]] || fail
check_file "$test_directory/Sentinel.wvhr" 5064 \
    12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5 \
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
    12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5 \
    'preserved alias input' || fail
pass 'base tools reject exact path aliases'

total=$((total + 1))
"$repository_root/Tools/Native/Install-Hosted-Verifier-Publisher.sh" \
    "$test_directory/Publisher.elf" "$test_directory/Installed-Publisher.elf" \
    > "$test_directory/Install-Publisher-Linux.out" \
    2> "$test_directory/Install-Publisher-Linux.err" || fail
check_empty "$test_directory/Install-Publisher-Linux.err" \
    'Linux publisher installation wrote a diagnostic' || fail
check_file "$test_directory/Install-Publisher-Linux.out" 117 \
    72e2786587919e14f707b437166f86ae18dca24869249c8c541b9836d6ace397 \
    'Linux publisher installation report' || fail
check_file "$test_directory/Installed-Publisher.elf" 254917 \
    babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97 \
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
    734463bece6f4670d21a7dba7c0a6e865073d36e43990ba3027650263d3e5b8b \
    'Windows publisher installation report' || fail
check_file "$test_directory/Installed-Publisher.exe" 256000 \
    17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96 \
    'installed Windows publisher' || fail
cmp --silent "$test_directory/Publisher.exe" \
    "$test_directory/Installed-Publisher.exe" || fail
check_no_private_scratch || fail
pass 'current-host promoter installs both exact publishers'

total=$((total + 1))
check_file "$verifier_candidate" 1003520 \
    26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b \
    'Linux verifier candidate' || fail
"$test_directory/Installed-Publisher.elf" "$verifier_candidate" \
    "$test_directory/Installed.elf" > "$test_directory/Execute.out" \
    2> "$test_directory/Execute.err" || fail
check_empty "$test_directory/Execute.err" \
    'constructed publisher execution wrote a diagnostic' || fail
check_file "$test_directory/Installed.elf" 1003520 \
    26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b \
    'installed verifier' || fail
cmp --silent "$verifier_candidate" "$test_directory/Installed.elf" || fail
check_no_private_scratch || fail
pass 'promoted current-host publisher execution'

total=$((total + 1))
phase='current-host WVB publisher execution'
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

echo "Tests: $total, Passed: $passed, Failed: 0"
