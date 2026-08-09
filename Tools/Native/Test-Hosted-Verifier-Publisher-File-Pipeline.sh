#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
construction="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate"
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
    rmdir -- "$test_directory"
    return "$status"
}
trap cleanup EXIT

total=0
passed=0
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
    local publication_scratch=("$test_directory"/.wvpublish-*)
    if [[ -e ${construction_scratch[0]} || -e ${publication_scratch[0]} ]]; then
        echo 'FAIL  hosted-verifier publisher files: private scratch remains' >&2
        return 1
    fi
}
pass() {
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

total=$((total + 1))
check_file "$construction/SHA256SUMS" 4634 \
    83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273 \
    'construction inventory' || fail
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || fail
"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-Hosted-Verifier-Publisher-Application-Tool.wvproj" \
    "$test_directory/Publisher-Application-Admission-Tool.wvb" \
    > "$test_directory/Admission-Build.out" \
    2> "$test_directory/Admission-Build.err" || fail
check_empty "$test_directory/Admission-Build.err" \
    'admission source build wrote a diagnostic' || fail
check_file "$test_directory/Publisher-Application-Admission-Tool.wvb" 30837 \
    f1e7497dc1acba1a08190021d4dac83ec65c3e6b58f80edb3bfcd62eeda55ed3 \
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
check_file "$test_directory/Publisher-Application-Admission-Tool.wvo" 556273 \
    ac5972e8de83ad962874217ed6e0fba49586096df4c3b69d61abdf7509e2dff5 \
    'native-lowered publisher admission WVO' || fail
cmp --silent "$construction/Publisher-Application-Admission-Tool.wvo" \
    "$test_directory/Publisher-Application-Admission-Tool.wvo" || fail
pass 'publisher construction inventory'

total=$((total + 1))
"$repository_root/Tools/Native/Construct-Hosted-Verifier-Publisher.sh" windows \
    "$test_directory/Publisher.exe" > "$test_directory/Windows.out" \
    2> "$test_directory/Windows.err" || fail
check_empty "$test_directory/Windows.err" 'Windows construction wrote a diagnostic' || fail
grep -Fx 'publisher construction status=Valid target=windows bytes=256000' \
    "$test_directory/Windows.out" >/dev/null || fail
check_file "$test_directory/Publisher.exe" 256000 \
    735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6 \
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
    de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a \
    'Linux publisher' || fail
check_no_private_scratch || fail
pass 'exact Linux publisher construction'

cp -- "$construction/SHA256SUMS" "$test_directory/Invalid.wvsq" || fail
cp -- "$construction/SHA256SUMS" "$test_directory/Sentinel.wvhv" || fail
total=$((total + 1))
"$publisher_tools/wvhostverifierpublisherbasemetadata.elf" 1 3001 \
    "$test_directory/Invalid.wvsq" "$test_directory/Sentinel.wvhv" \
    > "$test_directory/Reject.out" 2> "$test_directory/Reject.err"
[[ $? -eq 2 ]] || fail
check_empty "$test_directory/Reject.out" 'metadata rejection wrote standard output' || fail
check_empty "$test_directory/Reject.err" 'metadata rejection wrote a diagnostic' || fail
check_file "$test_directory/Invalid.wvsq" 4634 \
    83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273 \
    'rejected metadata input' || fail
check_file "$test_directory/Sentinel.wvhv" 4634 \
    83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273 \
    'preserved metadata destination' || fail
cp -- "$construction/SHA256SUMS" "$test_directory/Sentinel.wvhr" || fail
"$publisher_tools/wvhostverifierpublisherbaseruntime.elf" \
    "$test_directory/Invalid.wvsq" "$test_directory/Sentinel.wvhr" \
    > "$test_directory/Reject.out" 2> "$test_directory/Reject.err"
[[ $? -eq 2 ]] || fail
check_file "$test_directory/Sentinel.wvhr" 4634 \
    83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273 \
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
check_file "$test_directory/Invalid.wvsq" 4634 \
    83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273 \
    'preserved alias input' || fail
pass 'base tools reject exact path aliases'

total=$((total + 1))
check_file "$verifier_candidate" 1003520 \
    26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b \
    'Linux verifier candidate' || fail
"$test_directory/Publisher.elf" "$verifier_candidate" \
    "$test_directory/Installed.elf" > "$test_directory/Execute.out" \
    2> "$test_directory/Execute.err" || fail
check_empty "$test_directory/Execute.err" \
    'constructed publisher execution wrote a diagnostic' || fail
check_file "$test_directory/Installed.elf" 1003520 \
    26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b \
    'installed verifier' || fail
cmp --silent "$verifier_candidate" "$test_directory/Installed.elf" || fail
check_no_private_scratch || fail
pass 'constructed current-host publisher execution'

echo "Tests: $total, Passed: $passed, Failed: 0"
